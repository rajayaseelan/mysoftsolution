using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MySoft
{
    /// <summary>
    /// DnsHelper
    /// </summary>
    public static class DnsHelper
    {
        /// <summary>
        /// 用户的机器名
        /// </summary>
        public static string GetHostName()
        {
            return Dns.GetHostName();
        }

        /// <summary>
        /// 获得本机局域网IP地址   
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddress()
        {
            var list = GetAddressList();

            if (list.Count > 0)
            {
                return list[0].ToString();
            }

            return IPAddress.Loopback.ToString();
        }

        /// <summary>
        /// 获得拨号动态分配IP地址   
        /// </summary>
        /// <returns></returns>
        public static string GetDynamicIPAddress()
        {
            var list = GetAddressList();

            if (list.Count > 1)
            {
                return list[1].ToString();
            }

            return IPAddress.Loopback.ToString();
        }

        /// <summary>
        /// 获取地址列表
        /// </summary>
        /// <returns></returns>
        private static IList<IPAddress> GetAddressList()
        {
            var entry = Dns.GetHostEntry(Dns.GetHostName());

            var list = new List<IPAddress>();

            foreach (var ipAddress in entry.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    list.Add(ipAddress);
                }
            }

            return list;
        }
    }
}
