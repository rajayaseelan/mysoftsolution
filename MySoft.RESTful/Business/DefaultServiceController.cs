using System;
using System.Configuration;
using MySoft.Logger;

namespace MySoft.RESTful.Business
{
    /// <summary>
    /// 默认服务控制器
    /// </summary>
    public class DefaultServiceController : IServiceController
    {
        private volatile int longProcessTime = -1;

        #region IServiceController 成员

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
        /// 开始调用
        /// </summary>
        /// <param name="caller"></param>
        public virtual void BeginCall(AppCaller caller)
        {
            //TODO:
        }

        /// <summary>
        /// 结束调用
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="value"></param>
        /// <param name="elapsedTime"></param>
        public virtual void EndCall(AppCaller caller, object value, long elapsedTime)
        {
            //记录10毫秒以上的处理
            if (longProcessTime > 10 && elapsedTime > longProcessTime)
            {
                var log = string.Format("Process business ({0}, {1}) elapsed time: {3} ms.\r\nParameters: ({2})",
                    caller.AppData.Kind, caller.AppData.Method, caller.AppData.Parameters, elapsedTime);

                SimpleLog.Instance.WriteLogForDir("BusinessProcess", log);
            }
        }

        #endregion
    }
}
