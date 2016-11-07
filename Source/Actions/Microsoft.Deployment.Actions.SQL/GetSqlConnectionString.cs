using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.Deployment.Common.Actions;
using Microsoft.Deployment.Common.Enums;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Common.Model;

namespace Microsoft.Deployment.Actions.SQL
{
    [Export(typeof(IAction))]
    public class GetSqlConnectionString : BaseAction
    {
        public override async Task<ActionResponse> ExecuteActionAsync(ActionRequest request)
        {
            var sqlCreds = request.DataStore.GetJson("SqlCredentials");

            string server = sqlCreds.SelectToken("Server")?.ToString();
            string user = sqlCreds.SelectToken("User")?.ToString();
            string password = sqlCreds.SelectToken("Password")?.ToString();
            var auth = sqlCreds.SelectToken("AuthType")?.ToString();
            var database = sqlCreds.SelectToken("Database")?.ToString();

            SqlCredentials credentials = new SqlCredentials()
            {
                Server = server,
                Username = user,
                Password = password,
                Authentication = auth.EqualsIgnoreCase("Windows") ? SqlAuthentication.Windows : SqlAuthentication.SQL,
                Database = string.IsNullOrEmpty(database) ? "master" : database
            };

            var response = SqlUtility.GetConnectionString(credentials);
            return new ActionResponse(ActionStatus.Success, JsonUtility.CreateJObjectWithValueFromObject(response), true);
        }
    }
}
