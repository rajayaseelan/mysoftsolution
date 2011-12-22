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
            return x.AppName == y.AppName && x.IPAddress == y.IPAddress;
        }

        /// <summary>
        /// 获取hash值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(AppClient obj)
        {
            return obj.AppName.GetHashCode() + obj.IPAddress.GetHashCode();
        }

        #endregion
    }
}
