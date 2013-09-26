using MySoft.Logger;
using System;
using System.Configuration;

namespace MySoft.RESTful.Business
{
    /// <summary>
    /// 默认服务控制器
    /// </summary>
    public class DefaultServiceController : IServiceController
    {
        private volatile int longProcessTime = -1;

        #region IServiceController 成员

        /// <summary>
        /// 默认服务控制器
        /// </summary>
        public DefaultServiceController()
        {
            try
            {
                longProcessTime = Convert.ToInt32(ConfigurationManager.AppSettings["longProcessTime"]);
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 处理耗时时间
        /// </summary>
        public int LongProcessTime
        {
            get
            {
                return longProcessTime;
            }
        }

        /// <summary>
        /// 开始调用
        /// </summary>
        /// <param name="caller"></param>
        public virtual void BeginCall(AppCaller caller)
        {
            //TODO:
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public virtual object CallService(AppCaller caller, object instance)
        {
            return caller.Method.FastInvoke(instance, caller.Parameters);
        }

        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="value"></param>
        /// <param name="elapsedTime"></param>
        public void EndCall(AppCaller caller, object value, long elapsedTime)
        {
            //记录10毫秒以上的处理
            if (longProcessTime > 10 && elapsedTime > longProcessTime)
            {
                var log = string.Format("Process business ({0}, {1}) elapsed time: {3} ms.\r\nParameters: ({2})",
                    caller.ApiKind, caller.ApiMethod, caller.ApiParameters, elapsedTime);

                SimpleLog.Instance.WriteLogForDir("BusinessProcess", log);
            }
        }

        #endregion
    }
}
