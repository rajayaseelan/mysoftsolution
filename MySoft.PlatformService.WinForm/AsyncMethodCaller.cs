using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;

namespace MySoft.PlatformService.WinForm
{
    /// <summary>
    /// 异步方法调用
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public delegate InvokeData AsyncMethodCaller(InvokeMessage message);

    /// <summary>
    /// 异步方法调用
    /// </summary>
    /// <param name="method"></param>
    /// <param name="instance"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    public delegate BinaryResponse AsyncDoMethod(System.Reflection.MethodInfo method, object instance, object[] parameter);

    /// <summary>
    /// 调用响应
    /// </summary>
    [Serializable]
    public class InvokeResponse : InvokeData
    {
        /// <summary>
        /// 实例化InvokeResponse
        /// </summary>
        public InvokeResponse() { }

        /// <summary>
        /// 实例化InvokeResponse
        /// </summary>
        /// <param name="data"></param>
        public InvokeResponse(InvokeData data)
        {
            base.Count = data.Count;
            base.Value = data.Value;
            base.ElapsedTime = data.ElapsedTime;
            base.OutParameters = data.OutParameters;

            this.ElapsedMilliseconds = data.ElapsedTime;
        }

        /// <summary>
        /// 耗时
        /// </summary>
        public long ElapsedMilliseconds { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 是否异常
        /// </summary>
        public bool IsError
        {
            get { return Exception != null; }
        }
    }

    /// <summary>
    /// 调用响应
    /// </summary>
    [Serializable]
    public class BinaryResponse : InvokeResponse
    {
        /// <summary>
        /// 数据
        /// </summary>
        public new object Value { get; set; }
    }
}
