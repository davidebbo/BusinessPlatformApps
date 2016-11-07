using Microsoft.Deployment.Actions.Test.TestHelpers;
using Microsoft.Deployment.Common;
using Microsoft.Deployment.Common.ActionModel;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Deployment.Actions.Test.ActionsTest
{
    [TestClass]
    public class CDMTests
    {

        [TestMethod]
        public async Task CreateEnvironment()
        {
            //Create Environment
            var dataStore = await AAD.GetUserTokenFromPopup();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", dataStore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);

            dataStore.AddToDataStore("Location", "unitedstates", DataStoreType.Private);
            dataStore.AddToDataStore("EnvironmentName", "TestEnvironment", DataStoreType.Private);
            var environmentResponse = TestHarness.ExecuteAction("Microsoft-CreateEnvironment", dataStore);
            Assert.IsTrue(environmentResponse.Status == ActionStatus.Success);
        }

        [TestMethod]
        public async Task CreateDatabase()
        {
            //Create Environment
            var dataStore = await AAD.GetUserTokenFromPopup();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", dataStore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);

            dataStore.AddToDataStore("Location", "unitedstates", DataStoreType.Private);
            dataStore.AddToDataStore("EnvironmentName", "TestDatabase", DataStoreType.Private);
            var environmentResponse = TestHarness.ExecuteAction("Microsoft-CreateEnvironment", dataStore);
            Assert.IsTrue(environmentResponse.Status == ActionStatus.Success);
            var databaseResponse = TestHarness.ExecuteAction("Microsoft-CreateDatabase", dataStore);
            Assert.IsTrue(environmentResponse.Status == ActionStatus.Success);
        }

        [TestMethod]
        public async Task ProvisionCDM()
        {
            //Create Environment
            var dataStore = await AAD.GetUserTokenFromPopup();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", dataStore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);

            dataStore.AddToDataStore("Location", "unitedstates", DataStoreType.Private);
            dataStore.AddToDataStore("EnvironmentName", "TestDatabase", DataStoreType.Private);

            var environmentResponse = TestHarness.ExecuteAction("Microsoft-ProvisionCDM", dataStore);
            Assert.IsTrue(environmentResponse.Status == ActionStatus.Success);
        }


        [TestMethod]
        public async Task GetCDMNamespaces()
        {
            //Get Token
            var datastore = await AAD.GetTokenWithDataStore();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", datastore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);
        }

        [TestMethod]
        public async Task GetObjID()
        {
            //Get Token
            var dataStore = await AAD.GetUserTokenFromPopup();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", dataStore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);
            var subscriptionId = responseBody["value"][0]["SubscriptionId"].ToString();

            dataStore.AddToDataStore("SubscriptionId", subscriptionId, DataStoreType.Private);
            var res = TestHarness.ExecuteAction("Microsoft-GetObjID", dataStore);
            Assert.IsTrue(res.Status == ActionStatus.Success);
        }

        [TestMethod]
        public async Task GetEnvironID()
        {
            //Get Token
            var dataStore = await AAD.GetUserTokenFromPopup();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", dataStore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);
            var subscriptionId = responseBody["value"][0]["SubscriptionId"].ToString();

            dataStore.AddToDataStore("SubscriptionId", subscriptionId, DataStoreType.Private);
            var objIdResponse = TestHarness.ExecuteAction("Microsoft-GetObjID", dataStore);

            var environIdResponse = TestHarness.ExecuteAction("Microsoft-GetEnvironID", dataStore);


            Assert.IsTrue(environIdResponse.Status == ActionStatus.Success);
        }

        [TestMethod]
        public async Task CheckCDMEntities()
        {
            //Get Token
            var dataStore = await AAD.GetUserTokenFromPopup();
            var result = await TestHarness.ExecuteActionAsync("Microsoft-GetAzureSubscriptions", dataStore);
            Assert.IsTrue(result.IsSuccess);
            var responseBody = JObject.FromObject(result.Body);

            //var objIdResponse = TestHarness.ExecuteAction("Microsoft-GetObjID", dataStore);
            var environIdResponse = TestHarness.ExecuteAction("Microsoft-GetEnvironID", dataStore);
            var getCDMEntityResponse = TestHarness.ExecuteAction("Microsoft-CheckCDMEntities", dataStore);


            Assert.IsTrue(getCDMEntityResponse.Status == ActionStatus.Success);
        }

        [TestMethod]
        public async Task GetAllEnvironments()
        {
            var dataStore = await AAD.GetUserTokenFromPopup();
            var environIdResponse = TestHarness.ExecuteAction("Microsoft-GetAllEnvironments", dataStore);
        }

        [TestMethod]
        public async Task GetAllAndDeleteEnvironments()
        {
            var dataStore = await AAD.GetUserTokenFromPopup();
            var environIdResponse = await TestHarness.ExecuteActionAsync("Microsoft-GetAllEnvironments", dataStore);
            var environments = JObject.FromObject(environIdResponse.Body);

            foreach (var environment in environments["value"])
            {
                dataStore.AddToDataStore(environment["name"].ToString(), "EnvironmentIds", environment["name"].ToString());
            }

            var response = await TestHarness.ExecuteActionAsync("Microsoft-DeleteEnvironment", dataStore);
        }
    }
}
