using System.Runtime.Remoting.Messaging;

namespace MySoft.Aop
{
    /// <summary>
    /// IAopOperator AOP操作符接口，包括前处理和后处理
    /// 2010.11.09
    /// </summary>
    public interface IAopOperator
    {
        /// <summary>
        /// 前置处理
        /// </summary>
        /// <param name="requestMsg"></param>
        void PreProceed(IMethodCallMessage requestMsg);

        /// <summary>
        /// 后置处理
        /// </summary>
        /// <param name="requestMsg"></param>
        /// <param name="respondMsg"></param>
        void PostProceed(IMethodCallMessage requestMsg, ref IMethodReturnMessage respondMsg);
    }
}
