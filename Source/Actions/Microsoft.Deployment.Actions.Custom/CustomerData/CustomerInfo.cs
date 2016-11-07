using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Deployment.Actions.Custom.CustomerData
{
    public class CustomerInfoSimplement : TableEntity
    {
        public CustomerInfoSimplement(string emailAddress) : this()
        {
            RowKey = emailAddress;
        }

        private CustomerInfoSimplement()
        {
            PartitionKey = "Simplement.SolutionTemplate.AR";
        }

        public string FirstName;
        public string LastName;
        public string City;
        public string State;
        public string CompanyName;
        public string DepartmentName;
    }

}
