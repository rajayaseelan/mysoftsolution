using System.Net;

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
            IPAddress addr = new IPAddress(Dns.GetHostByName(Dns.GetHostName()).AddressList[0].Address);
            return addr.ToString();
        }

        /// <summary>
        /// 获得拨号动态分配IP地址   
        /// </summary>
        /// <returns></returns>
        public static string GetDynamicIPAddress()
        {
            IPAddress addr = new IPAddress(Dns.GetHostByName(Dns.GetHostName()).AddressList[1].Address);
            return addr.ToString();
        }
    }
}
