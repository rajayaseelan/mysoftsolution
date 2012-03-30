using System;
using System.Collections;
using MySoft.IoC.Aspect;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// The dynamic service.
    /// </summary>
    public class DynamicService : BaseService
    {
        private static Hashtable hashtable = Hashtable.Synchronized(new Hashtable());
        private IServiceContainer container;
        private Type serviceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicService"/> class.
        /// </summary>
        /// <param name="serviceType">Type of the service interface.</param>
        public DynamicService(IServiceContainer container, Type serviceType)
            : base(container, serviceType)
        {
            this.container = container;
            this.serviceType = serviceType;
        }

        /// <summary>
        /// Runs the specified MSG.
        /// </summary>
        /// <param name="reqMsg">The MSG.</param>
        /// <returns>The msg.</returns>
        protected override ResponseMessage Run(RequestMessage reqMsg)
        {
            ResponseMessage resMsg = new ResponseMessage();
            resMsg.TransactionId = reqMsg.TransactionId;
            resMsg.ServiceName = reqMsg.ServiceName;
            resMsg.MethodName = reqMsg.MethodName;
            resMsg.ReturnType = reqMsg.ReturnType;

            #region 获取相应的方法

            var methodKey = string.Format("{0}_{1}", reqMsg.ServiceName, reqMsg.MethodName);
            if (!hashtable.ContainsKey(methodKey))
            {
                lock (hashtable.SyncRoot)
                {
                    if (!hashtable.ContainsKey(methodKey))
                    {
                        var m = CoreHelper.GetMethodFromType(serviceType, reqMsg.MethodName);
                        if (m == null)
                        {
                            string message = string.Format("The server not find called method ({0},{1}).", reqMsg.ServiceName, reqMsg.MethodName);
                            resMsg.Error = new WarningException(message);

                            return resMsg;
                        }

                        hashtable[methodKey] = m;
                    }
                }
            }

            #endregion

            //容器实例对象
            object instance = null;

            try
            {
                //定义Method
                var method = hashtable[methodKey] as System.Reflection.MethodInfo;

                //解析服务
                instance = container.Resolve(serviceType);

                //返回拦截服务
                var service = AspectFactory.CreateProxyService(serviceType, instance);

                var pis = method.GetParameters();
                if (reqMsg.Invoked)
                {
                    var objValue = reqMsg.Parameters["InvokeParameter"];
                    var jsonString = string.Empty;
                    if (objValue != null) jsonString = objValue.ToString();

                    //解析参数
                    ServiceConfig.ParseParameter(jsonString, resMsg, pis);
                }
                else
                {
                    resMsg.Parameters = reqMsg.Parameters;
                }

                //参数赋值
                object[] parameters = new object[pis.Length];
                int index = 0;
                foreach (var p in pis)
                {
                    //处理默认值
                    parameters[index] = resMsg.Parameters[p.Name] ?? CoreHelper.GetTypeDefaultValue(p.ParameterType);
                    index++;
                }

                //调用对应的服务
                object returnValue = DynamicCalls.GetMethodInvoker(method).Invoke(service, parameters);

                var outValues = new Hashtable();
                index = 0;
                foreach (var p in pis)
                {
                    if (p.ParameterType.IsByRef)
                    {
                        resMsg.Parameters[p.Name] = parameters[index];
                        outValues[p.Name] = parameters[index];
                    }
                    index++;
                }

                //返回结果数据
                if (reqMsg.Invoked)
                {
                    resMsg.Parameters.Clear();
                    resMsg.Value = returnValue;

                    //返回值
                    string json1 = SerializationManager.SerializeJson(returnValue);

                    //输出参数值
                    string json2 = SerializationManager.SerializeJson(outValues);

                    returnValue = new InvokeData
                    {
                        Value = json1,
                        Count = resMsg.Count,
                        OutParameters = json2
                    };
                }

                resMsg.Value = returnValue;
            }
            catch (Exception ex)
            {
                //捕获全局错误
                resMsg.Error = ex;
            }
            finally
            {
                //释放资源
                container.Release(instance);
            }

            return resMsg;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose()
        {
            hashtable.Clear();
        }
    }
}
