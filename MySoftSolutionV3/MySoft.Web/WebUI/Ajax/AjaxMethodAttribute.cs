#region usings

using System;
using System.Reflection;

#endregion

namespace MySoft.Web.UI
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AjaxMethodAttribute : Attribute
    {
        /// <summary>
        /// 自定义方法名称
        /// </summary>
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private bool async;
        public bool Async
        {
            get
            {
                return async;
            }
            set
            {
                async = value;
            }
        }

        public AjaxMethodAttribute()
        {
            this.name = null;
            this.async = false;
        }

        public AjaxMethodAttribute(string name)
        {
            this.name = name;
            this.async = false;
        }

        public AjaxMethodAttribute(bool async)
        {
            this.name = null;
            this.async = async;
        }

        public AjaxMethodAttribute(string name, bool async)
        {
            this.name = name;
            this.async = async;
        }
    }

    internal class AsyncMethodInfo
    {
        private bool async;
        public bool Async
        {
            get
            {
                return async;
            }
            set
            {
                async = value;
            }
        }

        private MethodInfo methodInfo;
        public MethodInfo MethodInfo
        {
            get
            {
                return methodInfo;
            }
            set
            {
                methodInfo = value;
            }
        }
    }

    internal class AjaxMethodInfo
    {
        /// <summary>
        /// 函数名称
        /// </summary>
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private bool async;
        public bool Async
        {
            get
            {
                return async;
            }
            set
            {
                async = value;
            }
        }

        /// <summary>
        /// 参数列表
        /// </summary>
        private string[] paramters;
        public string[] Paramters
        {
            get
            {
                return paramters;
            }
            set
            {
                paramters = value;
            }
        }
    }
}