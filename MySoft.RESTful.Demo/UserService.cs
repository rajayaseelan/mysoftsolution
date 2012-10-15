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
        /// 获取一个用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("getuser", Description = "获取一个用户")]
        User GetUser(string name);

        /// <summary>
        /// 获取一组用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("getusers", Description = "获取一组用户", AuthorizeType = AuthorizeType.App)]
        IList<User> GetUsers();

        /// <summary>
        /// 添加一组用户
        /// </summary>
        /// <returns></returns>
        [PublishMethod("addusers", Description = "添加一组用户")]
        int AddUsers(IList<User> users);

        /// <summary>
        /// 获取用户名
        /// </summary>
        /// <returns></returns>
        [PublishMethod("getusername", Description = "获取用户名")]
        string GetUserName(int id);
    }

    /// <summary>
    /// 用户服务
    /// </summary>
    public class UserService : IUserService
    {
        #region IUserService 成员

        /// <summary>
        /// 获取一个用户
        /// </summary>
        /// <returns></returns>
        public User GetUser(string name)
        {
            return new User { Id = name.Length, Name = name };
        }

        /// <summary>
        /// 获取用户名
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetUserName(int id)
        {
            return "maoyong" + id;
        }


        /// <summary>
        /// 获取一组用户
        /// </summary>
        /// <returns></returns>
        public IList<User> GetUsers()
        {
            return new List<User> {
                new User { Id = 1, Name = "test1" },
                new User { Id = 2, Name = "test2" }            , 
                new User { Id = 3, Name = "test3" }
            };
        }

        /// <summary>
        /// 添加一组用户
        /// </summary>
        /// <param name="users"></param>
        public int AddUsers(IList<User> users)
        {
            return users.Count;
        }

        #endregion
    }
}
