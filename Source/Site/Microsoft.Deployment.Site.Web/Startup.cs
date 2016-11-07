using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Deployment.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();

            //app.Use(async (context, next) =>
            //{
            //    if (context.Request.IsHttps)
            //    {
            //        await next();
            //    }
            //    else
            //    {

            //        var withHttps = "https://" + context.Request.Host + context.Request.Path +
            //                        context.Request.QueryString;
            //        context.Response.Redirect(withHttps);

            //    }
            //});
        }
    }
}
