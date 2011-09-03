using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Services;

namespace MySoft.Aop
{
    /// <summary>
    /// AopProxyBase 所有自定义AOP代理类都从此类派生，覆写IAopOperator接口，实现具体的前/后处理 。
    /// 2010.11.09
    /// </summary>
    public abstract class AopProxyBase : RealProxy, IAopOperator
    {
        private readonly MarshalByRefObject target; //默认透明代理

        public AopProxyBase(MarshalByRefObject obj, Type type)
            : base(type)
        {
            this.target = obj;
        }

        #region Invoke

        public override IMessage Invoke(IMessage msg)
        {
            bool useAspect = false;
            IMethodCallMessage call = (IMethodCallMessage)msg;

            //查询目标方法是否使用了启用AOP的AopSwitcherAttribute
            //默认为使用Aspect
            var attr = CoreHelper.GetMemberAttribute<AopSwitcherAttribute>(call.MethodBase);
            if (attr != null)
            {
                useAspect = attr.UseAspect;
            }
            else
            {
                useAspect = true;
            }

            if (useAspect)
            {
                this.PreProceed(call);
            }

            //如果触发的是构造函数，此时target的构建还未开始
            IConstructionCallMessage ctor = call as IConstructionCallMessage;
            if (ctor != null)
            {
                //获取最底层的默认真实代理
                RealProxy default_proxy = RemotingServices.GetRealProxy(this.target);

                default_proxy.InitializeServerObject(ctor);
                MarshalByRefObject tp = (MarshalByRefObject)this.GetTransparentProxy(); //自定义的透明代理 this

                return EnterpriseServicesHelper.CreateConstructionReturnMessage(ctor, tp);
            }

            IMethodReturnMessage result_msg = RemotingServices.ExecuteMessage(this.target, call); //将消息转化为堆栈，并执行目标方法，方法完成后，再将堆栈转化为消息

            if (useAspect)
            {
                this.PostProceed(call, ref result_msg);
            }

            return result_msg;

        }

        #endregion

        #region IAopOperator 成员

        /// <summary>
        /// 抽象PreProcess方法
        /// </summary>
        /// <param name="requestMsg"></param>
        public abstract void PreProceed(IMethodCallMessage requestMsg);

        /// <summary>
        /// 抽象PostProcess方法
        /// </summary>
        /// <param name="requestMsg"></param>
        /// <param name="respondMsg"></param>
        public abstract void PostProceed(IMethodCallMessage requestMsg, ref IMethodReturnMessage respondMsg);

        #endregion

    }
}
