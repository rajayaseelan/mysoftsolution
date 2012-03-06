using System;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Routing;
using MySoft.IoC.HttpProxy;
using System.ServiceModel;
using MySoft.Auth;

namespace MySoft.WebAPI
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
            RouteTable.Routes.Add(new ServiceRoute("", new WebServiceHostFactory(), typeof(AuthorizeHttpProxyService)));
        }
    }

    /// <summary>
    /// 用户认证的HttpProxy服务
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AuthorizeHttpProxyService : DefaultHttpProxyService
    {
        /// <summary>
        /// 进行认证处理，如用户认证
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
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
