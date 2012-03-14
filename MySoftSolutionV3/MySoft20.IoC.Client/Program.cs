using System;
using System.Collections.Generic;
using System.Text;
using MySoft.IoC;
using MySoft.PlatformService.UserService;
using System.Threading;

namespace MySoft20.IoC.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    var user1 = CastleFactory.Create().GetChannel<IUserService>().GetUsers();
                    Console.WriteLine(user1.Count + "," + user1[0].Description);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Thread.Sleep(1000);
            }
            Console.ReadLine();
        }
    }
}

namespace MySoft.PlatformService.UserService
{
    public interface IUserService
    {
        IList<UserInfo> GetUsers();
    }

    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public Sex Sex { get; set; }
    }

    public enum Sex
    {
        男,
        女
    }
}
