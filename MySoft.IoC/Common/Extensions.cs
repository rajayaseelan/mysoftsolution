using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.IoC.Messages;
using MySoft.IoC.Logger;
using MySoft.Cache;

namespace MySoft.IoC
{
    /// <summary>
    /// 扩展类
    /// </summary>
    public static class LogExtensions
    {
        /// <summary>
        /// 请求开始
        /// </summary>
        /// <param name="reqMsg"></param>
        internal static void BeginRequest(this IServiceLog logger, RequestMessage reqMsg)
        {
            if (logger == null) return;

            try
            {
                var callMsg = new CallMessage
                {
                    AppName = reqMsg.AppName,
                    IPAddress = reqMsg.IPAddress,
                    HostName = reqMsg.HostName,
                    ServiceName = reqMsg.ServiceName,
                    MethodName = reqMsg.MethodName,
                    Parameters = reqMsg.Parameters
                };

                //开始调用
                logger.Begin(callMsg);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 请求结束
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <param name="elapsedMilliseconds"></param>
        internal static void EndRequest(this IServiceLog logger, RequestMessage reqMsg, ResponseMessage resMsg, long elapsedMilliseconds)
        {
            if (logger == null) return;

            try
            {
                var callMsg = new CallMessage
                {
                    AppName = reqMsg.AppName,
                    IPAddress = reqMsg.IPAddress,
                    HostName = reqMsg.HostName,
                    ServiceName = reqMsg.ServiceName,
                    MethodName = reqMsg.MethodName,
                    Parameters = reqMsg.Parameters
                };

                var returnMsg = new ReturnMessage
                {
                    ServiceName = resMsg.ServiceName,
                    MethodName = resMsg.MethodName,
                    Parameters = resMsg.Parameters,
                    Count = resMsg.Count,
                    Error = resMsg.Error,
                    Value = resMsg.Value
                };

                //结束调用
                logger.End(callMsg, returnMsg, elapsedMilliseconds);
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// 缓存扩展
    /// </summary>
    public static class CacheExtensions
    {
        #region ICache 成员

        /// <summary>
        /// 插入缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="seconds"></param>
        internal static void InsertCache(this ICacheStrategy cache, string key, object value, int seconds)
        {
            if (cache == null)
                CacheHelper.Insert(key, value, seconds);
            else
                cache.AddObject(key, value, TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static T GetCache<T>(this ICacheStrategy cache, string key)
        {
            if (cache == null)
                return CacheHelper.Get<T>(key);
            else
                return cache.GetObject<T>(key);
        }

        /// <summary>
        /// 移除缓存数据
        /// </summary>
        /// <param name="key"></param>
        internal static void RemoveCache(this ICacheStrategy cache, string key)
        {
            if (cache == null)
                CacheHelper.Remove(key);
            else
                cache.RemoveObject(key);
        }

        #endregion
    }
}
