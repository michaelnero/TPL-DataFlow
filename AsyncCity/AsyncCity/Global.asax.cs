using log4net;
using log4net.Config;
using System.IO;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace AsyncCity {
    public class MvcApplication : HttpApplication {
        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var fileInfo = new FileInfo(@"C:\Projects\AsyncCity\AsyncCity\log4net.config");
            XmlConfigurator.Configure(fileInfo);
        }
    }
}