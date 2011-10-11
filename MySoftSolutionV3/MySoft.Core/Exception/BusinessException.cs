using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace MySoft
{
    /// <summary>
    /// 业务异常类，继承自SystemException
    /// 用于业务出错时抛出业务异常信息
    /// </summary>
    [Serializable]
    public class BusinessException : SystemException
    {
        const int BUSINESS_EXCEPTION = 503;

        /// <summary>
        /// 异常代码
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 实例化BusinessException
        /// </summary>
        public BusinessException()
            : base()
        {
            this.Code = BUSINESS_EXCEPTION;
        }

        /// <summary>
        /// 实例化BusinessException
        /// </summary>
        /// <param name="message">异常消息</param>
        public BusinessException(string message)
            : base(message)
        {
            this.Code = BUSINESS_EXCEPTION;
        }

        /// <summary>
        /// 实例化BusinessException
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="inner">内部异常</param>
        public BusinessException(string message, Exception inner)
            : base(message, inner)
        {
            this.Code = BUSINESS_EXCEPTION;
        }

        /// <summary>
        /// 实例化BusinessException
        /// </summary>
        /// <param name="code">业务代码</param>
        /// <param name="message">异常消息</param>
        public BusinessException(int code, string message)
            : base(message)
        {
            this.Code = code;
        }

        /// <summary>
        /// 实例化BusinessException
        /// </summary>
        /// <param name="code">业务代码</param>
        /// <param name="message">异常消息</param>
        /// <param name="inner">内部异常</param>
        public BusinessException(int code, string message, Exception inner)
            : base(message, inner)
        {
            this.Code = code;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="info">存储对象序列化和反序列化所需的全部数据</param>
        /// <param name="context">描述给定的序列化流的源和目标，并提供一个由调用方定义的附加上下文</param>
        protected BusinessException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Code = (int)info.GetValue("Code", typeof(int));
        }

        /// <summary>
        /// 重载GetObjectData方法
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Code", this.Code);
        }
    }
}
