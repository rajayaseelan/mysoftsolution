using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MySoft.RESTful.Business
{
    /// <summary>
    /// 服务控制器
    /// </summary>
    public interface IServiceController
    {
        /// <summary>
        /// 开始调用
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        void BeginCall(AppCaller caller);

        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="value"></param>
        /// <param name="elapsedTime"></param>
        void EndCall(AppCaller caller, object value, long elapsedTime);
    }
}
