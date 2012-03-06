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
        /// 方法名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否异步
        /// </summary>
        public bool Async { get; set; }

        /// <summary>
        /// 实例化AjaxMethodAttribute
        /// </summary>
        public AjaxMethodAttribute() { }

        /// <summary>
        /// 实例化AjaxMethodAttribute
        /// </summary>
        /// <param name="name"></param>
        public AjaxMethodAttribute(string name)
        {
            this.Name = name;
        }
    }

    internal class AjaxMethodInfo
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否异步
        /// </summary>
        public bool Async { get; set; }

        /// <summary>
        /// 参数列表
        /// </summary>
        public string[] Paramters { get; set; }

        public AjaxMethodInfo()
        {
            this.Paramters = new string[0];
        }
    }
}