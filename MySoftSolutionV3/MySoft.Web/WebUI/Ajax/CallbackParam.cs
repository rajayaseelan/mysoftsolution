using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace MySoft.Web.UI
{
    /// <summary>
    /// Callback参数值
    /// </summary>
    public class CallbackParam
    {
        private string keyValue;
        public string Value
        {
            get
            {
                return keyValue;
            }
        }

        internal CallbackParam(string value)
        {
            this.keyValue = value;
        }

        /// <summary>
        /// 将value转换成对应的类型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T To<T>()
        {
            return To<T>(default(T));
        }

        /// <summary>
        /// 将value转换成对应的类型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defvalue"></param>
        /// <returns></returns>
        public T To<T>(T defvalue)
        {
            return CoreHelper.ConvertTo<T>(this.keyValue, defvalue);
        }

        /// <summary>
        /// 返回原始值
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.keyValue;
        }
    }

    /// <summary>
    /// 返回Callback参数字典
    /// </summary>
    public class CallbackParams
    {
        private Dictionary<string, CallbackParam> dictValues;
        internal CallbackParams()
        {
            this.dictValues = new Dictionary<string, CallbackParam>();
        }

        /// <summary>
        /// 重载字典
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CallbackParam this[string key]
        {
            get
            {
                if (dictValues.ContainsKey(key))
                    return dictValues[key];

                return null;
            }
            set
            {
                dictValues[key] = value;
            }
        }

        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string value)
        {
            if (Contains(key))
            {
                throw new Exception("已经存在Key为（" + key + "）的值");
            }
            dictValues.Add(key, new CallbackParam(value));
        }

        /// <summary>
        /// 判断是否存在key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Contains(string key)
        {
            return dictValues.ContainsKey(key);
        }

        /// <summary>
        /// 将当前集合转换成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T To<T>()
        {
            return To<T>(null);
        }

        /// <summary>
        /// 将当前集合转换成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public T To<T>(string prefix)
        {
            ObjectBuilder<T> builder = new ObjectBuilder<T>(prefix);
            return builder.Bind(ToNameValueCollection());
        }

        /// <summary>
        /// 返回NameValueCollection
        /// </summary>
        /// <returns></returns>
        private NameValueCollection ToNameValueCollection()
        {
            NameValueCollection values = new NameValueCollection();
            foreach (var pair in dictValues)
            {
                values.Add(pair.Key, pair.Value.ToString());
            }

            return values;
        }
    }
}
