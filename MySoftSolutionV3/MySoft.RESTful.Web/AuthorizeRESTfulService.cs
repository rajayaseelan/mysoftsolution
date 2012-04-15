using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using MySoft.Auth;

namespace MySoft.RESTful.Web
{
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