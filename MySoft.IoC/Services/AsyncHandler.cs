using System;
using System.Threading;
using MySoft.Cache;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 异步处理器
    /// </summary>
    internal class AsyncHandler
    {
        private string callKey;
        private IService service;
        private OperationContext context;
        private RequestMessage reqMsg;

        /// <summary>
        /// 实例化AsyncHandler
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        public AsyncHandler(string callKey, IService service, OperationContext context, RequestMessage reqMsg)
        {
            this.callKey = callKey;
            this.service = service;
            this.context = context;
            this.reqMsg = reqMsg;
        }

        /// <summary>
        /// 获取响应信息
        /// </summary>
        /// <param name="fromServer"></param>
        /// <returns></returns>
        public ResponseItem GetResponseItem(bool fromServer)
        {
            //开始请求
            var ar = BeginRequest(fromServer);

            try
            {
                ar.AsyncWaitHandle.WaitOne();

                var caller = ar.AsyncState as Func<ResponseItem>;

                //异步回调
                return caller.EndInvoke(ar);
            }
            catch (Exception ex)
            {
                //获取异常响应
                var resMsg = IoCHelper.GetResponse(reqMsg, ex);

                return new ResponseItem(resMsg);
            }
            finally
            {
                //释放资源，必写
                ar.AsyncWaitHandle.Close();
            }
        }

        /// <summary>
        /// 开始请求
        /// </summary>
        /// <param name="fromServer"></param>
        /// <returns></returns>
        private IAsyncResult BeginRequest(bool fromServer)
        {
            //定义委托
            Func<ResponseItem> caller;

            //是否需要缓存
            if (NeedCacheResult(reqMsg))
            {
                if (fromServer)
                    caller = GetResponseFromFile;
                else
                    caller = GetResponseFromCache;
            }
            else
                caller = GetResponseFromService;

            //开始异步调用
            return caller.BeginInvoke(null, caller);
        }

        /// <summary>
        /// 获取响应从本地缓存
        /// </summary>
        /// <returns></returns>
        private ResponseItem GetResponseFromService()
        {
            ResponseMessage resMsg = null;

            //设置上下文
            OperationContext.Current = context;

            try
            {
                //响应结果，清理资源
                resMsg = service.CallService(reqMsg);
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    Thread.ResetAbort();
                }

                //获取异常响应
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                OperationContext.Current = null;
            }

            //实例化ResponseItem
            return new ResponseItem(resMsg);
        }

        /// <summary>
        /// 从内存获取数据项
        /// </summary>
        /// <returns></returns>
        private ResponseItem GetResponseFromCache()
        {
            //获取内存缓存
            return CacheHelper<ResponseItem>.Get(callKey, TimeSpan.FromSeconds(reqMsg.CacheTime), () =>
            {
                //同步请求响应数据
                return GetResponseFromService();
            });
        }

        /// <summary>
        /// 获取响应从本地缓存
        /// </summary>
        /// <returns></returns>
        private ResponseItem GetResponseFromFile()
        {
            //双缓存保护获取方式
            var key = new CacheKey
            {
                UniqueId = callKey,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName.Substring(reqMsg.MethodName.IndexOf(' ') + 1)
            };

            return FileCacheHelper.Get(key, TimeSpan.FromSeconds(reqMsg.CacheTime), () =>
            {
                //同步请求响应数据
                var item = GetResponseFromService();

                if (item != null && CheckResponse(item.Message))
                {
                    var buffer = SerializationManager.SerializeBin(item.Message);
                    item.Buffer = CompressionManager.CompressGZip(buffer);
                    item.Message = null;
                }

                return item;
            });
        }

        /// <summary>
        /// 判断是否需要缓存
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private bool NeedCacheResult(RequestMessage reqMsg)
        {
            return reqMsg.EnableCache && reqMsg.CacheTime > 0;
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
