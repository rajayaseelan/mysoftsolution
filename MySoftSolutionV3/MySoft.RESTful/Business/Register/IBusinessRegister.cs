using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.RESTful.Business;
using MySoft.RESTful.Business.Pool;

namespace MySoft.RESTful.Business.Register
{
    /// <summary>
    /// 业务注册器
    /// </summary>
    public interface IBusinessRegister
    {
        /// <summary>
        /// 注册业务
        /// </summary>
        void Register(IBusinessPool businessPool);
    }
}
