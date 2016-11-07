using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Common.AppLoad
{
    public class UIPage
    {
        public string PageName { get; set; }
        public string RoutePageName { get; set; }
        public string Path { get; set; }
        public string AppName { get; set; }
        public string DisplayName { get; set; }
        public string UserGeneratedPath { get; set; }
        public JToken Parameters { get; set; }

        public UIPage Clone()
        {
            return new UIPage()
            {
                PageName = this.PageName,
                RoutePageName = this.RoutePageName,
                Path = this.Path,
                AppName = this.AppName,
                DisplayName = this.DisplayName,
                UserGeneratedPath = this.UserGeneratedPath,
                Parameters = this.Parameters
            };
        }
    }
}