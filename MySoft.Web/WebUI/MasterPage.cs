using System;
using System.Text;
using System.Web.UI;

namespace MySoft.Web.UI
{
    /// <summary>
    /// The MasterPage base class.
    /// </summary>
    public class MasterPage : System.Web.UI.MasterPage
    {
        protected WebHelper.ClientScriptFactoryHelper ClientScriptFactory = new WebHelper.ClientScriptFactoryHelper();

        #region Add Object To Page

        /// <summary>
        /// 注册js文件到页面上
        /// </summary>
        /// <param name="jsfiles"></param>
        protected void RegisterPageJsFile(params string[] jsfiles)
        {
            WebHelper.RegisterPageJsFile(this.Page, jsfiles);
        }

        /// <summary>
        /// 注册css文件到页面上
        /// </summary>
        /// <param name="jsfiles"></param>
        protected void RegisterPageCssFile(params string[] cssfiles)
        {
            WebHelper.RegisterPageCssFile(this.Page, cssfiles);
        }

        /// <summary>
        /// 注册script脚本到页面上
        /// </summary>
        protected void RegisterPageScript(params string[] scripts)
        {
            WebHelper.RegisterPageScript(this.Page, scripts);
        }

        /// <summary>
        /// 将指定url页面中的ajax调用所需js注册到本页
        /// </summary>
        /// <param name="path"></param>
        protected void RegisterPageForAjax(string url)
        {
            WebHelper.RegisterPageForAjax(this.Page, url);
        }

        /// <summary>
        /// 将指定url页面中的ajax调用所需js注册到本页
        /// </summary>
        /// <param name="path"></param>
        protected void RegisterPageForAjax(Type urlType, string url)
        {
            WebHelper.RegisterPageForAjax(this.Page, urlType, url);
        }

        /// <summary>
        /// 添加对象到页面上
        /// </summary>
        /// <param name="ObjectName"></param>
        /// <param name="values"></param>
        protected void AddObjectToPage(string ObjectName, params object[] values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var " + ObjectName + " = ");
            if (values == null)
            {
                sb.Append(SerializationManager.SerializeJson(values) + ";\r\n");
            }
            else
            {
                if (values.Length == 1)
                {
                    object value = values[0];
                    sb.Append(SerializationManager.SerializeJson(values[0]) + ";\r\n");
                }
                else
                {
                    sb.Append(SerializationManager.SerializeJson(values) + ";\r\n");
                }
            }
            WebHelper.RegisterPageScript(this.Page, sb.ToString());
        }

        /// <summary>
        /// 添加枚举到页面上
        /// </summary>
        /// <param name="ObjectName"></param>
        /// <param name="type"></param>
        protected void AddEnumToPage(Type type)
        {
            if (!type.IsEnum) return;
            StringBuilder sb = new StringBuilder();
            string ObjectName = type.Name;
            string[] enumNames = Enum.GetNames(type);

            sb.Append("var " + ObjectName + " = ");
            sb.Append("{\r\n");
            int index = 0;
            foreach (object obj in Enum.GetValues(type))
            {
                sb.Append("\t");
                sb.Append(enumNames[index]);
                sb.Append(" : ");
                sb.Append(obj.GetHashCode());
                if (index < enumNames.Length - 1)
                {
                    sb.Append(",\r\n");
                }
                index++;
            }
            sb.Append("\r\n\t};\r\n");
            WebHelper.RegisterPageScript(this.Page, sb.ToString());
        }
        #endregion

        #region Helper Methods

        /// <summary>
        /// 将value转换成对应的类型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(string value)
        {
            return ConvertTo<T>(value, default(T));
        }

        /// <summary>
        /// 将value转换成对应的类型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="outValue"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(string value, T outValue)
        {
            return CoreHelper.ConvertTo<T>(value, outValue);
        }

        /// <summary>
        /// Gets the string param.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <returns>The param value.</returns>
        protected TResult GetRequestParam<TResult>(string paramName)
        {
            return WebHelper.GetRequestParam<TResult>(Request, paramName);
        }

        /// <summary>
        /// Gets the string param.
        /// </summary>
        /// <param name="paramName">Name of the param.</param>
        /// <param name="errorReturn">The error return.</param>
        /// <returns>The param value.</returns>
        protected TResult GetRequestParam<TResult>(string paramName, TResult errorReturn)
        {
            return WebHelper.GetRequestParam<TResult>(Request, paramName, errorReturn);
        }

        /// <summary>
        /// Strongs the typed.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The strong typed instance.</returns>
        protected TObject StrongTyped<TObject>(object obj)
        {
            return WebHelper.StrongTyped<TObject>(obj);
        }

        /// <summary>
        /// Strongs the typed.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The strong typed instance.</returns>
        protected ControlType FindControl<ControlType>(string id)
            where ControlType : Control
        {
            Control control = FindControl(id);
            return WebHelper.StrongTyped<ControlType>(control);
        }

        /// <summary>
        /// Texts to HTML.
        /// </summary>
        /// <param name="txtStr">The TXT STR.</param>
        /// <returns>The formated str.</returns>
        protected string TextToHtml(string txtStr)
        {
            return WebHelper.TextToHtml(txtStr);
        }

        #endregion

        #region Resource

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The resource value.</returns>
        protected string GetResourceString(string key)
        {
            return WebHelper.GetResourceString(key);
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="ci">The ci.</param>
        /// <returns>The resource value.</returns>
        protected string GetResourceString(string key, System.Globalization.CultureInfo ci)
        {
            return WebHelper.GetResourceString(key, ci);
        }

        #endregion
    }
}
