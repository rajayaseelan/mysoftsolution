using System;

namespace MySoft.RESTful.Auth
{
    /// <summary>
    /// 认证操作封装
    /// </summary>
    public interface IAuthentication
    {
        /// <summary>
        /// 授权及认证方法
        /// </summary>
        /// <returns>通过认证后返回是否响应成功，通过AuthenticationContext.Current.Token获取令牌信息进行认证</returns>
        bool Authorize();

        /// <summary>
        /// 认证类型
        /// </summary>
        AuthType AuthType { get; }
    }

    /// <summary>
    /// 认证类型
    /// </summary>
    public enum AuthType : int
    {
        /// <summary>
        /// OAuth认证
        /// </summary>
        OAuth,
        /// <summary>
        /// Cookie认证
        /// </summary>
        Cookie,
        /// <summary>
        /// 用户名密码
        /// </summary>
        UidPwd
    }
}
