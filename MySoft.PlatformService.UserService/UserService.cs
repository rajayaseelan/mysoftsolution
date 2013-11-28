using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading;
using MySoft.IoC;
using MySoft.IoC.Aspect;
using System.Collections.Specialized;
using Castle.Core;

namespace MySoft.PlatformService.UserService
{
    //[AspectProxy(typeof(AspectLog))]
    public class UserService : TypeInitializable, IUserService
    {
        //private DateTime startTime;
        //public UserService()
        //{
        //    this.startTime = DateTime.Now;
        //}

        public User GetUserFromName(string name)
        {
            return new User { Id = name.Length, Name = name };
        }

        public object GetUser(object user)
        {
            return user;
        }

        //[AspectSwitcher(true, Description = "获取用户")]
        User IUserService.GetUser(int id)
        {
            //throw new ApplicationException("出错了！");

            var name = DateTime.Now.ToString() + "__" + id.ToString().PadRight(100000, '#');
            var sb = new StringBuilder(name);
            for (int i = 0; i < 10; i++)
            {
                sb.Append(name);
            }

            return new User { Id = id, Name = sb.ToString() };
        }

        public User GetUserForName(string name)
        {
            return new User { Id = name.Length, Name = name };
        }

        public string GetUser(NameValueCollection nv)
        {
            return "maoyong";
        }

        public string GetSex(Sex value)
        {
            return Convert.ToString(value);
        }

        [AspectSwitcher(true)]
        public virtual UserInfo GetUserInfo(string name, out string userid, out Guid guid, out UserInfo user)
        {
            //Thread.Sleep(60000);

            var context = OperationContext.Current;

            //var count = new Random(Guid.NewGuid().GetHashCode()).Next(1, 100) * new Random(Guid.NewGuid().GetHashCode()).Next(1, 100);
            //if (count % 5 == 0)
            //{
            //    throw new UserException("Error Count: " + count);
            //}
            //else if (count % 6 == 0)
            //{
            //    Thread.Sleep(new Random().Next(1, 10) * 1000);
            //}

            user = new UserInfo()
            {
                Name = name,
                Description = string.Format("您的用户名为：{0}", name)
            };

            userid = user.Name;

            try
            {
                guid = new Guid(userid.Split('_')[1]);
            }
            catch
            {
                guid = Guid.NewGuid();
            }

            //int value = new Random().Next(1, 5);
            //Thread.Sleep(value * 1000);
            Console.WriteLine("{0} => {1}", DateTime.Now, guid);

            //Thread.Sleep(10);

            return user;
        }

        public string GetDateTime(Guid guid, DateTime time, UserInfo user, Sex sex)
        {
            return string.Format("{0} => {1}", guid, DateTime.Now);
        }

        private static string value;
        private static readonly object syncRoot = new object();
        public string GetUsersString(int count, out int length)
        {
            //if (DateTime.Now.Ticks % 15 == 0)
            //{
            //    Thread.Sleep(10000);
            //}
            //else
            //{
            //Thread.Sleep(2100);
            //}

            try
            {
                if (value == null)
                {
                    lock (syncRoot)
                    {
                        if (value == null)
                        {
                            value = System.IO.File.ReadAllText("c:\\test.htm");
                        }
                    }
                }

                var sb = new StringBuilder();
                for (int i = 0; i < count; i++)
                {
                    sb.Append(value);
                }

                length = sb.Length;

                return sb.ToString();
            }
            catch (Exception ex)
            {
                //return ErrorHelper.GetHtmlError(ex);

                throw ex;
            }
        }

        public IList<UserInfo> GetUsers()
        {
            //throw new BusinessException("业务异常");

            //Thread.Sleep(1000);

            var list = new List<UserInfo>();

            for (int i = 0; i < 10000; i++)
            {
                var count = new Random(Guid.NewGuid().GetHashCode()).Next(1, 100) * new Random(Guid.NewGuid().GetHashCode()).Next(1, 100);
                list.Add(new UserInfo { Name = "test_" + count, Description = "test_" + count + "_" + Thread.CurrentThread.ManagedThreadId });
            }

            return list;
        }

        public UserInfo GetUser(int id, string name, IList<string> list, int[] values)
        {
            return new UserInfo { Name = id.ToString(), Description = id.ToString() + "你好！" + name };
        }

        public IDictionary<Sex, IList<UserInfo>> GetDict()
        {
            return null;
        }

        public int[] GetUserIds()
        {
            return new int[] { 1, 2, 3, 4, 5, 6 };
        }

        #region IInitializable 成员

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Initialize()
        {
            //throw new NotImplementedException();
            Console.WriteLine("{0}: Initialize...", DateTime.Now);
        }

        #endregion
    }

    public class AspectLog : AspectInterceptor
    {
        protected override void PreProceed(IInvocation invocation)
        {
            base.PreProceed(invocation);
        }

        protected override void PerformProceed(IInvocation invocation)
        {
            try
            {
                base.PerformProceed(invocation);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override void PostProceed(IInvocation invocation)
        {
            base.PostProceed(invocation);
        }
    }
}
