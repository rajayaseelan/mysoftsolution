using System;
using System.Collections;
using System.Text;
using System.Threading;
using MySoft.Cache;
using MySoft.IoC.Messages;
using MySoft.Logger;
using MySoft.Security;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 同步调用器
    /// </summary>
    internal class SyncCaller
    {
        private IDataCache cache;
        private bool enabledCache;
        private bool fromServer;

        /// <summary>
        /// 实例化SyncCaller
        /// </summary>
        /// <param name="fromServer"></param>
        public SyncCaller(bool fromServer)
        {
            this.fromServer = fromServer;
            this.enabledCache = false;
        }

        /// <summary>
        /// 实例化SyncCaller
        /// </summary>
        /// <param name="fromServer"></param>
        /// <param name="cache"></param>
        public SyncCaller(bool fromServer, IDataCache cache)
            : this(fromServer)
        {
            this.cache = cache;
            this.enabledCache = true;
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
            if (CheckCache(reqMsg))
            {
                //从缓存获取数据
                var item = GetResponseFromCache(service, context, reqMsg);

                //从缓存中获取数据
                if (item != null)
                {
                    SetResponse(reqMsg, item);
                }

                return item;
            }
            else
            {
                //返回正常响应
                var resMsg = GetResponse(service, context, reqMsg);

                if (resMsg == null) return null;

                //实例化Item
                return new ResponseItem(resMsg);
            }
        }

        /// <summary>
        /// 判断是否需要缓存
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private bool CheckCache(RequestMessage reqMsg)
        {
            return enabledCache && reqMsg.CacheTime > 0;
        }

        /// <summary>
        /// 获取响应信息
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private void SetResponse(RequestMessage reqMsg, ResponseItem item)
        {
            if (!fromServer && item.Message == null)
            {
                var buffer = CompressionManager.DecompressGZip(item.Buffer);

                var resMsg = SerializationManager.DeserializeBin<ResponseMessage>(buffer);

                //设置同步返回传输Id
                resMsg.TransactionId = reqMsg.TransactionId;

                item.Buffer = null;
                item.Message = resMsg;
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
        /// 从缓存中获取数据
        /// </summary>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem GetResponseFromCache(IService service, OperationContext context, RequestMessage reqMsg)
        {
            try
            {
                //定义回调函数
                Func<string, IService, OperationContext, RequestMessage, ResponseItem> func = null;

                if (cache == null)
                {
                    //获取响应从本地缓存
                    func = GetResponseFromLocalCache;
                }
                else
                {
                    //获取响应从远程缓存
                    func = GetResponseFromRemoteCache;
                }

                //获取CallerKey
                var callKey = GetCallerKey(service, context.Caller);

                //如果是状态服务，则使用内部缓存
                if (reqMsg.InvokeMethod)
                {
                    callKey = string.Format("invoke_{0}", callKey);
                }

                return func(callKey, service, context, reqMsg);
            }
            catch (Exception ex)
            {
                SimpleLog.Instance.WriteLogForDir("SyncCaller", ex);

                return null;
            }
        }

        /// <summary>
        /// 获取响应从本地缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem GetResponseFromLocalCache(string callKey, IService service, OperationContext context, RequestMessage reqMsg)
        {
            //双缓存保护获取方式
            var array = new ArrayList { service, context, reqMsg };

            var key = new CacheKey
            {
                UniqueId = callKey,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName.Substring(reqMsg.MethodName.IndexOf(' ') + 1)
            };

            return ServiceCacheHelper.Get(key, TimeSpan.FromSeconds(reqMsg.CacheTime), state =>
                    {
                        var arr = state as ArrayList;
                        var _service = arr[0] as IService;
                        var _context = arr[1] as OperationContext;
                        var _reqMsg = arr[2] as RequestMessage;

                        //同步请求响应数据
                        var resMsg = GetResponse(_service, _context, _reqMsg);

                        if (resMsg == null) return null;

                        var item = new ResponseItem(resMsg);

                        if (CheckResponse(resMsg))
                        {
                            var buffer = SerializationManager.SerializeBin(resMsg);
                            buffer = CompressionManager.CompressGZip(buffer);

                            item.Buffer = buffer;
                        }

                        return item;

                    }, array);
        }

        /// <summary>
        /// 获取响应从远程缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="service"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseItem GetResponseFromRemoteCache(string callKey, IService service, OperationContext context, RequestMessage reqMsg)
        {
            //定义一个响应值
            ResponseItem item = null;

            //从缓存获取
            var cacheItem = cache.Get<CacheItem>(callKey);

            if (cacheItem == null)
            {
                //同步请求响应数据
                var resMsg = GetResponse(service, context, reqMsg);

                if (resMsg == null) return null;

                item = new ResponseItem(resMsg);

                if (CheckResponse(resMsg))
                {
                    try
                    {
                        //序列化对象
                        var buffer = SerializationManager.SerializeBin(resMsg);
                        buffer = CompressionManager.CompressGZip(buffer);
                        item.Buffer = buffer;

                        var timeout = TimeSpan.FromSeconds(reqMsg.CacheTime);

                        cacheItem = new CacheItem
                        {
                            ExpiredTime = DateTime.Now.Add(timeout),
                            Count = resMsg.Count,
                            Value = buffer
                        };

                        //插入缓存
                        cache.Insert(callKey, cacheItem, timeout);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                //实例化Item
                item = new ResponseItem { Buffer = cacheItem.Value, Count = cacheItem.Count };
            }

            return item;
        }

        /// <summary>
        /// 获取CallerKey
        /// </summary>
        /// <param name="service"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        private string GetCallerKey(IService service, AppCaller caller)
        {
            //对Key进行组装
            var callKey = string.Format("{0}${1}${2}${3}", service.ServiceName, caller.ServiceName, caller.MethodName
                                        , caller.Parameters).Replace(" ", "").Replace("\r\n", "").Replace("\t", "");

            //返回加密Key
            return MD5.HexHash(Encoding.Default.GetBytes(callKey.ToLower()));
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