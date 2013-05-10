using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MySoft.IoC.Messages;
using MySoft.Security;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 同步调用器
    /// </summary>
    internal class SyncCaller
    {
        private IDictionary<string, QueueManager> hashtable;
        private bool fromServer;

        /// <summary>
        /// 实例化SyncCaller
        /// </summary>
        /// <param name="fromServer"></param>
        public SyncCaller(bool fromServer)
        {
            this.fromServer = fromServer;
            this.hashtable = new Dictionary<string, QueueManager>();
        }

        /// <summary>
        /// 同步调用服务
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseItem Run(IService service, OperationContext context, RequestMessage reqMsg)
        {
            //获取callerKey
            var callKey = GetCallerKey(reqMsg, context.Caller);

            QueueManager manager = null;

            lock (hashtable)
            {
                if (!hashtable.ContainsKey(callKey))
                {
                    hashtable[callKey] = new QueueManager();
                }

                manager = hashtable[callKey];
            }

            //合并请求响应
            using (var channelResult = new ChannelResult())
            {
                if (manager.Count == 0)
                {
                    manager.Add(channelResult);

                    Func<string, IService, OperationContext, RequestMessage, ResponseItem> func = null;

                    if (NeedServerCache(reqMsg))
                        func = GetResponseFromCache;
                    else
                        func = GetResponseFromService;

                    //开始异步调用
                    func.BeginInvoke(callKey, service, context, reqMsg, AsyncCallback, new ArrayList { callKey, manager, func });
                }
                else
                {
                    manager.Add(channelResult);
                }

                //永久等待超时
                channelResult.WaitOne();

                return channelResult.Message;
            }
        }

        /// <summary>
        /// 回调方法
        /// </summary>
        /// <param name="ar"></param>
        private static void AsyncCallback(IAsyncResult ar)
        {
            try
            {
                var arr = ar.AsyncState as ArrayList;

                //回调方法
                var callKey = Convert.ToString(arr[0]);
                var _manager = arr[1] as QueueManager;
                var _func = arr[2] as Func<string, IService, OperationContext, RequestMessage, ResponseItem>;

                try
                {
                    //响应请求
                    _manager.Set(_func.EndInvoke(ar));
                }
                finally
                {
                    if (_manager != null) _manager.Clear();
                }
            }
            catch (Exception ex) { }
            finally
            {
                //关闭句柄
                ar.AsyncWaitHandle.Close();
            }
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(IService service, OperationContext context, RequestMessage reqMsg)
        {
            //定义一个响应值
            ResponseMessage resMsg = null;

            //设置上下文
            OperationContext.Current = context;

            try
            {
                //响应结果，清理资源
                return service.CallService(reqMsg);
            }
            catch (ThreadInterruptedException ex) { }
            catch (ThreadAbortException ex)
            {
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                //获取异常响应
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                OperationContext.Current = null;
            }

            return resMsg;
        }

        /// <summary>
        /// 获取响应从本地缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem GetResponseFromService(string callKey, IService service, OperationContext context, RequestMessage reqMsg)
        {
            //从本地获取数据
            var resMsg = GetResponse(service, context, reqMsg);

            //实例化ResponseItem
            return resMsg == null ? null : new ResponseItem(resMsg);
        }

        /// <summary>
        /// 判断是否需要缓存
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private bool NeedServerCache(RequestMessage reqMsg)
        {
            return fromServer && reqMsg.EnableCache && reqMsg.CacheTime > 0;
        }

        /// <summary>
        /// 获取响应从本地缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem GetResponseFromCache(string callKey, IService service, OperationContext context, RequestMessage reqMsg)
        {
            //双缓存保护获取方式
            var key = new CacheKey
            {
                UniqueId = callKey,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName.Substring(reqMsg.MethodName.IndexOf(' ') + 1)
            };

            return ServiceCacheHelper.Get(key, TimeSpan.FromSeconds(reqMsg.CacheTime), state =>
            {
                //获取响应信息项
                var arr = state as ArrayList;
                var _service = arr[0] as IService;
                var _context = arr[1] as OperationContext;
                var _reqMsg = arr[2] as RequestMessage;

                //同步请求响应数据
                var item = GetResponseFromService(callKey, service, context, reqMsg);

                if (item != null && CheckResponse(item.Message))
                {
                    item.Buffer = GetResponseBuffer(item.Message);
                }

                return item;

            }, new ArrayList { service, context, reqMsg });
        }

        /// <summary>
        /// 获取缓冲
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private byte[] GetResponseBuffer(ResponseMessage resMsg)
        {
            var buffer = SerializationManager.SerializeBin(resMsg);
            return CompressionManager.CompressGZip(buffer);
        }

        /// <summary>
        /// 获取CallerKey
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        private string GetCallerKey(RequestMessage reqMsg, AppCaller caller)
        {
            //对Key进行组装
            var callKey = string.Format("{0}${1}${2}", caller.ServiceName, caller.MethodName, caller.Parameters);

            //返回加密Key
            callKey = MD5.HexHash(Encoding.Default.GetBytes(callKey.ToLower()));

            //如果是状态服务，则使用内部缓存
            if (reqMsg.InvokeMethod)
            {
                callKey = string.Format("invoke_{0}", callKey);
            }

            return callKey;
        }

        /// <summary>
        /// 检测响应是否有效
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private bool CheckResponse(ResponseMessage resMsg)
        {
            if (resMsg == null) return false;

            //如果符合条件，则缓存 
            if (!resMsg.IsError && resMsg.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}