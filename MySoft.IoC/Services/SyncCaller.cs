using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;
using MySoft.Cache;
using MySoft.IoC.Messages;
using MySoft.Security;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 同步调用器
    /// </summary>
    internal class SyncCaller
    {
        private IService service;
        private IDataCache cache;
        private bool enabledCache;

        /// <summary>
        /// 实例化SyncCaller
        /// </summary>
        /// <param name="service"></param>
        public SyncCaller(IService service)
        {
            this.service = service;
            this.enabledCache = false;
        }

        /// <summary>
        /// 实例化SyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cache"></param>
        public SyncCaller(IService service, IDataCache cache)
            : this(service)
        {
            this.cache = cache;
            this.enabledCache = true;
        }

        /// <summary>
        /// 同步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage Run(OperationContext context, RequestMessage reqMsg)
        {
            //开始一个记时器
            var watch = Stopwatch.StartNew();

            try
            {
                byte[] buffer = null;

                //响应数据
                var resMsg = Run(context, reqMsg, out buffer);

                //反序列化成对象
                if (resMsg == null && buffer != null)
                {
                    buffer = CompressionManager.DecompressGZip(buffer);

                    resMsg = SerializationManager.DeserializeBin<ResponseMessage>(buffer);

                    //设置同步返回传输Id
                    resMsg.TransactionId = reqMsg.TransactionId;
                    resMsg.ElapsedTime = watch.ElapsedMilliseconds;
                }

                return resMsg;
            }
            catch (Exception ex)
            {
                //返回异常响应
                return IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                if (watch.IsRunning)
                {
                    watch.Stop();
                }
            }
        }

        /// <summary>
        /// 同步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public ResponseMessage Run(OperationContext context, RequestMessage reqMsg, out byte[] buffer)
        {
            buffer = null;

            if (enabledCache)
            {
                //从缓存获取数据
                buffer = GetResponseFromCache(context, reqMsg);

                //从缓存中获取数据
                if (buffer != null) return null;
            }

            //返回响应
            return GetResponse(context, reqMsg);
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponse(OperationContext context, RequestMessage reqMsg)
        {
            //定义一个响应值
            ResponseMessage resMsg = null;

            //设置上下文
            OperationContext.Current = context;

            try
            {
                //响应结果，清理资源
                resMsg = service.CallService(reqMsg);
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
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private byte[] GetResponseFromCache(OperationContext context, RequestMessage reqMsg)
        {
            //从缓存中获取数据
            if (reqMsg.CacheTime <= 0) return null;

            //定义回调函数
            Func<string, OperationContext, RequestMessage, byte[]> func = null;

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
            var callKey = GetCallerKey(context.Caller);

            //如果是状态服务，则使用内部缓存
            if (reqMsg.InvokeMethod)
            {
                callKey = string.Format("invoke_{0}", callKey);
            }

            return func(callKey, context, reqMsg);
        }

        /// <summary>
        /// 获取响应从本地缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private byte[] GetResponseFromLocalCache(string callKey, OperationContext context, RequestMessage reqMsg)
        {
            //双缓存保护获取方式
            var array = new ArrayList { context, reqMsg };

            var key = new CacheKey
            {
                UniqueId = callKey,
                ServiceName = reqMsg.ServiceName,
                MethodName = reqMsg.MethodName.Substring(reqMsg.MethodName.IndexOf(' ') + 1)
            };

            return ServiceCacheHelper.Get(key, TimeSpan.FromSeconds(reqMsg.CacheTime), state =>
                    {
                        var arr = state as ArrayList;
                        var _context = arr[0] as OperationContext;
                        var _reqMsg = arr[1] as RequestMessage;

                        //同步请求响应数据
                        var resMsg = GetResponse(_context, _reqMsg);

                        if (CheckResponse(resMsg))
                        {
                            var buffer = SerializationManager.SerializeBin(resMsg);

                            return CompressionManager.CompressGZip(buffer);
                        }

                        return null;

                    }, array);
        }

        /// <summary>
        /// 获取响应从远程缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private byte[] GetResponseFromRemoteCache(string callKey, OperationContext context, RequestMessage reqMsg)
        {
            //定义一个响应值
            byte[] buffer = null;

            try
            {
                //从缓存获取
                buffer = cache.Get<byte[]>(callKey);
            }
            catch
            {
            }

            if (buffer == null)
            {
                //同步请求响应数据
                var resMsg = GetResponse(context, reqMsg);

                if (CheckResponse(resMsg))
                {
                    try
                    {
                        buffer = SerializationManager.SerializeBin(resMsg);

                        buffer = CompressionManager.CompressGZip(buffer);

                        //插入缓存
                        cache.Insert(callKey, buffer, TimeSpan.FromSeconds(reqMsg.CacheTime));
                    }
                    catch
                    {
                    }
                }
            }

            return buffer;
        }

        /// <summary>
        /// 获取CallerKey
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        private string GetCallerKey(AppCaller caller)
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