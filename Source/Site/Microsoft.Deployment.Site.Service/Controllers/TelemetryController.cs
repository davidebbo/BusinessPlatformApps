using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.Deployment.Common;

namespace Microsoft.Deployment.Site.Service.Controllers
{
    public class TelemetryController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage TelemtrySave(string id)
        {
            //Logger log = new Logger("","","","PBITelemtry","",id,"","",new Dictionary<string, string>());
            //log.LogEvent("PBITelemtry"  + "-" + id, new Dictionary<string, string>());
            //log.Flush();

            //string folderPath = System.AppDomain.CurrentDomain.BaseDirectory;
            //string templatePath = folderPath + Constants.TemplatePath;
            //if (!Directory.Exists(templatePath))
            //{
            //    templatePath = folderPath + Constants.TemplatePathMsi;
            //    if (!Directory.Exists(templatePath))
            //    {
            //        throw new Exception("Template Root Path invalid");
            //    }
            //}

            //string path = templatePath + "\\Common\\Web\\images\\tagging.png";

            //Image img = Image.FromFile(path);
            //MemoryStream ms = new MemoryStream();
            //img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            //result.Content = new ByteArrayContent(ms.ToArray());
            //result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            //return result;

            return null;
        }
    }
}