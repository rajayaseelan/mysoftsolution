using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "http://sandbox.trade.fund123.cn/fundapi/tradereq/purchase?fundcode=020001&oauth_nonce=cbfdacdafcdegidcieiegciiacdheeaa&oauth_version=1.0&applysum=1111&oauth_consumer_key=iphone_smb&usertype=p&format=json&businflag=022&tradepassword=123123%26&sharetype=A&oauth_signature=kznUZY6x65gpAbG3eP1NnSVayiY%3D&tradeacco=0023&oauth_signature_method=HMAC-SHA1&oauth_token=abe60d01ff73482b8b4b0ee217d69dbf&channel=1&capsource=0&detailcapitalmode=01&oauth_timestamp=1375946716";
            //var url = System.Web.HttpUtility.UrlEncode("http://www.fund123.cn?uid=aabbccee&");

            var http = System.Web.HttpUtility.ParseQueryString(url);

            Console.WriteLine(http["tradepassword"]);
            Console.ReadLine();

        }
    }
}
