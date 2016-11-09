using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Actions.Salesforce.Helpers;
using Microsoft.Deployment.Actions.Salesforce.Models;
using Microsoft.Deployment.Actions.Salesforce.SalesforceSOAP;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Helpers;
using Newtonsoft.Json;

namespace Microsoft.Deployment.Actions.Salesforce
{
    [Export(typeof(IAction))]
    public class SalesforceSqlArtefacts : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            string schema = "dbo";
            string connString = request.DataStore.GetValue("SqlConnectionString");

            var objectMetadata = request.DataStore.GetValue("Objects");
            List<DescribeSObjectResult> metadataList = JsonConvert.DeserializeObject(objectMetadata, typeof(List<DescribeSObjectResult>)) as List<DescribeSObjectResult>;
            List<Tuple<string, List<ADFField>>> adfFields = new List<Tuple<string, List<ADFField>>>();

            foreach (var obj in metadataList)
            {
                var simpleMetadata = ExtractSimpleMetadata(obj);

                adfFields.Add(new Tuple<string, List<ADFField>>(obj.name, simpleMetadata));

                CreateSqlTableAndTableType(simpleMetadata, obj.fields, schema, obj.name, connString);
                CreateStoredProcedure(simpleMetadata, string.Concat("spMerge", obj.name), schema, string.Concat(obj.name, "Type"), obj.name, connString);
            }

            dynamic resp = new ExpandoObject();
            resp.ADFPipelineJsonData = new ExpandoObject();
            resp.ADFPipelineJsonData.fields = adfFields;

            return new ActionResponse(ActionStatus.Success, JsonUtility.GetJObjectFromObject(resp));
        }

        public List<ADFField> ExtractSimpleMetadata(DescribeSObjectResult sfobject)
        {
            List<ADFField> simpleFields = new List<ADFField>();

            foreach (var field in sfobject.fields)
            {
                // check to go around ADF unsupported fields
                if (field.type != fieldType.address &&
                    field.type != fieldType.location &&
                    SupportedField(field))
                {
                    var newField = new ADFField();
                    var rawField = field.type;
                    newField.name = field.name;
                    var cleanedField = field.type.ToString().Contains('@') ? field.type.ToString().Replace('@', ' ') : field.type.ToString();
                    var netType = TypeMapper.SalesforceToDotNet.Where(p => p.Key == cleanedField).FirstOrDefault();
                    Debug.WriteLine(string.Concat(field.name, ", ", rawField, ", ", cleanedField, ", ", netType.Value));
                    if (netType.Key != null)
                    {
                        if (netType.Value == "int")
                        {
                            if (field.digits <= 5)
                            {
                                newField.type = "Int16";
                            }
                            if (field.digits > 5 && field.digits <= 10)
                            {
                                newField.type = "Int32";
                            }
                            if (field.digits > 10 && field.digits <= 19)
                            {
                                newField.type = "Int64";
                            }
                        }
                        else
                        {
                            newField.type = netType.Value;
                        }
                        simpleFields.Add(newField);
                    }
                }
            }
            return simpleFields;
        }

        public bool SupportedField(SalesforceSOAP.Field field)
        {
            List<string> userFieldsThatAreNotSupportedByADF = new List<string>();

            userFieldsThatAreNotSupportedByADF.Add("MediumPhotoUrl");
            userFieldsThatAreNotSupportedByADF.Add("UserPreferencesHideBiggerPhotoCallout");
            userFieldsThatAreNotSupportedByADF.Add("UserPreferencesHideSfxWelcomeMat");
            userFieldsThatAreNotSupportedByADF.Add("UserPreferencesHideLightningMigrationModal");
            userFieldsThatAreNotSupportedByADF.Add("UserPreferencesHideEndUserOnboardingAssistantModal");
            userFieldsThatAreNotSupportedByADF.Add("UserPreferencesPreviewLightning");

            if (userFieldsThatAreNotSupportedByADF.Contains(field.name))
            {
                return false;
            }
            return true;
        }


        public void CreateSqlTableAndTableType(List<ADFField> fields, SalesforceSOAP.Field[] sfFields, string schemaName, string tableName, string connString)
        {
            StringBuilder sb = new StringBuilder();

            string createTable = string.Format("CREATE TABLE [{0}].[{1}](", schemaName, tableName);
            string createTableType = string.Format("CREATE TYPE [{0}].[{1}Type] AS TABLE(", schemaName, tableName);

            sb.AppendLine(createTable);

            foreach (var field in fields)
            {
                var sqlType = TypeMapper.DotNetToSql.Where(p => p.Key == field.type).First();

                if (sqlType.Key != null && sqlType.Value == "nvarchar")
                {
                    int nvarcharSize = sfFields.First(e => e.name == field.name).length;

                    string size = string.Empty;

                    if (nvarcharSize > 4000)
                    {
                        size = "max";
                    }
                    if (nvarcharSize == 0)
                    {
                        size = "255";
                    }

                    sb.AppendLine(string.Format("[{0}] [{1}]({2}) NULL,",
                        field.name,
                        string.IsNullOrEmpty(sqlType.Value) ? field.type.ToString() : sqlType.Value,
                        !string.IsNullOrEmpty(size) ? size : nvarcharSize.ToString()));
                }
                else
                {
                    sb.AppendLine(string.Format("[{0}] [{1}] NULL,", field.name, string.IsNullOrEmpty(sqlType.Value) ? field.type.ToString() : sqlType.Value));
                }
            }

            sb.Remove(sb.Length - 3, 1);
            sb.AppendLine(")");

            SqlUtility.InvokeSqlCommand(connString, sb.ToString(), null);

            sb.Replace(createTable, createTableType);

            SqlUtility.InvokeSqlCommand(connString, sb.ToString(), null);
        }

        public void CreateStoredProcedure(List<ADFField> fields, string sprocName, string schemaName, string tableTypeName, string targetTableName, string connString)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("CREATE procedure [{0}].[{1}] @{2} [{0}].[{3}] READONLY as BEGIN", schemaName, sprocName, targetTableName, tableTypeName));
            sb.AppendLine(string.Format("MERGE [{0}].[{1}] AS TARGET \r\nUSING\r\n(SELECT", schemaName, targetTableName));

            foreach (var field in fields)
            {
                sb.AppendLine(string.Format("[{0}],", field.name));
            }

            sb.Remove(sb.Length - 3, 1);

            sb.AppendLine(string.Format("FROM @{0}\r\n) AS SOURCE\r\n ON SOURCE.ID = TARGET.ID \r\n WHEN MATCHED AND source.[LastModifiedDate] > target.[LastModifiedDate] THEN", targetTableName));

            sb.AppendLine("UPDATE \r\n SET");

            foreach (var field in fields)
            {
                sb.AppendLine(string.Format("TARGET.[{0}] = SOURCE.[{0}],", field.name));
            }

            sb.Remove(sb.Length - 3, 1);

            sb.AppendLine("WHEN NOT MATCHED BY TARGET THEN \r\nINSERT(");

            foreach (var field in fields)
            {
                sb.AppendLine(string.Format("[{0}],", field.name));
            }

            sb.Remove(sb.Length - 3, 1);
            sb.Append(")\r\n VALUES (");

            foreach (var field in fields)
            {
                sb.AppendLine(string.Format("SOURCE.[{0}],", field.name));
            }

            sb.Remove(sb.Length - 3, 1);
            sb.Append(");");

            var containsDelete = fields.Select(p => p.name == "IsDeleted");

            if (containsDelete.Contains(true))
            {
                sb.AppendLine($"DELETE FROM [{schemaName}].[{targetTableName}]");
                sb.AppendLine("WHERE IsDeleted = 1");
            }
            sb.AppendLine(@"END");

            SqlUtility.InvokeSqlCommand(connString, sb.ToString(), null);
        }
    }
}
