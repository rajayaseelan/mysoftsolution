using System;
using System.Collections.Generic;
using System.Text;
using MySoft.IoC;
using MySoft.PlatformService.UserService;

namespace MySoft20.IoC.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            int length = 10;
            UserInfo user;
            var user1 = CastleFactory.Create().GetService<IUserService>().GetUserInfo("maoyong", ref length, out user);
            Console.WriteLine(user1.Description);
            Console.ReadLine();
        }
    }
}

namespace MySoft.PlatformService.UserService
{
    public interface IUserService
    {
        UserInfo GetUserInfo(string name, ref int length, out UserInfo user);
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
