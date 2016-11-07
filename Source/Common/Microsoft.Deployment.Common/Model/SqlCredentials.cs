using Microsoft.Deployment.Common.Enums;

namespace Microsoft.Deployment.Common.Model
{
    public class SqlCredentials
    {
        public string Server;

        public string Username;

        public string Password;

        public SqlAuthentication Authentication;

        public string Database;

        public string AlternativeDatabaseToConnect;

    }
}
