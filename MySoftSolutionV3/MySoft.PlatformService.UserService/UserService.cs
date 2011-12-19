using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using MySoft.IoC;

namespace MySoft.PlatformService.UserService
{
    public class UserService : IUserService
    {
        public string GetUser(UserInfo user)
        {
            return user.Name;
        }

        public int GetSex(Sex value)
        {
            return Convert.ToInt32(value);
        }

        public UserInfo GetUserInfo(string name, ref int length)
        {
            var count = new Random(Guid.NewGuid().GetHashCode()).Next(1, 100) * new Random(Guid.NewGuid().GetHashCode()).Next(1, 100);
            if (count % 5 == 0)
            {
                throw new UserException("Count: " + count);
            }
            else if (count % 6 == 0)
            {
                Thread.Sleep(new Random().Next(1, 10) * 1000);
            }

            //var a = SerializationManager.DeserializeJson(typeof(Sex), "1");

            var user = new UserInfo()
            {
                Name = name,
                Description = string.Format("您的用户名为：{0}", name)
            };

            length = user.Description.Length;

            return user;
        }

        public string GetDateTime(Guid guid, DateTime time, UserInfo user, Sex sex)
        {
            return string.Format("{0} => {1}", guid, DateTime.Now);
        }

        public IList<UserInfo> GetUsers()
        {
            //Thread.Sleep(15000);
            //throw new Exception("sdfsad");
            var list = new List<UserInfo>();

            var count = new Random(Guid.NewGuid().GetHashCode()).Next(1, 100) * new Random(Guid.NewGuid().GetHashCode()).Next(1, 100);
            //if (count % 5 == 0)
            //{
            //    throw new UserException("Error Message : " + count);
            //}
            //else if (count % 6 == 0)
            //{
            //    Thread.Sleep(new Random().Next(1, 10) * 1000);
            //}

            list.Add(new UserInfo { Name = "test_" + count, Description = "test_" + count + "_" + Thread.CurrentThread.ManagedThreadId });

            return list;
        }
    }
}
