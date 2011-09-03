using System;

namespace MySoft.RESTful
{
    /// <summary>
    /// RESTful结果
    /// </summary>
    [Serializable]
    public class RESTfulResult
    {
        /// <summary>
        /// 代码
        /// </summary>
        public string Code;

        /// <summary>
        /// 消息
        /// </summary>
        public string Message;
    }

    /// <summary>
    /// RESTfulCode
    /// </summary>
    public enum RESTfulCode : int
    {
        /// <summary>
        /// 正确返回数据
        /// </summary>
        OK,
        /// <summary>
        /// 认证失败
        /// </summary>
        AUTH_FAULT,
        /// <summary>
        /// 验证错误
        /// </summary>
        AUTH_ERROR,
        /// <summary>
        /// 业务错误
        /// </summary>
        BUSINESS_ERROR,
        /// <summary>
        /// 业务类型没找到
        /// </summary>
        BUSINESS_KIND_NOT_FOUND,
        /// <summary>
        /// 业务方法没找到
        /// </summary>
        BUSINESS_METHOD_NOT_FOUND,
        /// <summary>
        /// 业务类型不是激活状态
        /// </summary>
        BUSINESS_KIND_NO_ACTIVATED,
        /// <summary>
        /// 业务方法不是激活状态
        /// </summary>
        BUSINESS_METHOD_NO_ACTIVATED,
        /// <summary>
        /// 业务方法参数个数不匹配
        /// </summary>
        BUSINESS_METHOD_PARAMS_COUNT_NOT_MATCH,
        /// <summary>
        /// 业务方法参数类型不匹配
        /// </summary>
        BUSINESS_METHOD_PARAMS_TYPE_NOT_MATCH,
        /// <summary>
        /// 业务方法调用类型不匹配
        /// </summary>
        BUSINESS_METHOD_CALL_TYPE_NOT_MATCH,
        /// <summary>
        /// 业务方法没有通过检查
        /// </summary>
        BUSINESS_METHOD_NOT_PASS_CHECK
    }
}
