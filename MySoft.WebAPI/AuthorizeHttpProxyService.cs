using System.ServiceModel;
using System.ServiceModel.Activation;
using MySoft.IoC.HttpProxy;
using MySoft.RESTful;

namespace MySoft.WebAPI
{
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
        /// <returns></returns>
        protected override AuthorizeUser Authorize(AuthorizeToken token)
        {
            return new AuthorizeUser
            {
                UserId = 1,
                UserName = "my181"
            };
        }
    }
}