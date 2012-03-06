using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using MySoft.Auth;
using System.ServiceModel;

namespace MySoft.RESTful.Web
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Edit the base address of Service1 by replacing the "Service1" string below
            RouteTable.Routes.Add(new ServiceRoute("", new WebServiceHostFactory(), typeof(AuthorizeRESTfulService)));
        }
    }

    /// <summary>
    /// 认证的服务
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AuthorizeRESTfulService : DefaultRESTfulService
    {
        protected override AuthorizeResult Authorize(AuthorizeToken token)
        {
            return new AuthorizeResult
            {
                Succeed = true,
                Name = "my181"
            };
        }
    }
}
