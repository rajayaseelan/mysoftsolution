using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using MySoft.IoC.Messages;

namespace MySoft.IoC
{
    /// <summary>
    /// Appclient 对比类
    /// </summary>
    public class AppClientComparer : IEqualityComparer<AppClient>
    {
        #region IEqualityComparer<AppClient> 成员

        /// <summary>
        /// 数据对比
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(AppClient x, AppClient y)
        {
            return x.AppName == y.AppName && x.IPAddress == y.IPAddress && x.AppPath == y.AppPath;
        }

        /// <summary>
        /// 获取hash值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(AppClient obj)
        {
            var hashCode = obj.AppName.GetHashCode() + obj.IPAddress.GetHashCode();
            if (!string.IsNullOrEmpty(obj.AppPath)) hashCode += obj.AppPath.GetHashCode();

            return hashCode;
        }

        #endregion
    }
}
