using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MySoft.RESTful.Demo
{
    /// <summary>
    /// 用户信息
    /// </summary>
    [Serializable]
    public class User
    {
        public int Id;
        public string Name;
    }

    /// <summary>
    /// 用户接口
    /// </summary>
    [PublishKind("user", Description = "用户接口")]
    public interface IUserService
    {
        /// <summary>
        /// 获取登录用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("getloginuser", Description = "获取登录用户")]
        User GetLoginUser(AuthenticationUser user);

        /// <summary>
        /// 获取一个用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("getuser", Description = "获取一个用户", Authorized = false)]
        User GetUser(int id, string name);

        /// <summary>
        /// 获取一组用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("getusers", Description = "获取一组用户", Authorized = false)]
        IList<User> GetUsers();

        /// <summary>
        /// 保存用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("saveuser", Description = "保存一个用户", Method = SubmitType.POST)]
        void SaveUser(User user);
    }

    /// <summary>
    /// 用户服务
    /// </summary>
    public class UserService : IUserService
    {
        #region IUserService 成员

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <returns></returns>
        public User GetLoginUser(AuthenticationUser user)
        {
            return new User { Id = user.AuthName.Length, Name = user.AuthName };
        }

        /// <summary>
        /// 获取一个用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("getuser", Description = "获取一个用户")]
        public User GetUser(int id, string name)
        {
            return new User { Id = id, Name = name };
        }

        /// <summary>
        /// 获取一组用户
        /// </summary>
        /// <returns></returns>
        public IList<User> GetUsers()
        {
            return new List<User> { new User { Id = 1, Name = "test1" }, new User { Id = 2, Name = "test2" } };
        }

        /// <summary>
        /// 保存用户
        /// </summary>
        /// <returns></returns>
        public void SaveUser(User user)
        {
            //未实现
        }

        #endregion
    }
}
