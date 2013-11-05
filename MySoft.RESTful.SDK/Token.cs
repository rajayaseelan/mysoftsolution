using System;

namespace MySoft.RESTful.SDK
{
    /// <summary>
    /// Token参数
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 参数集合
        /// </summary>
        public ApiParameterCollection Headers { get; set; }

        /// <summary>
        /// 实例化Token
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        public Token(string key, string name)
        {
            this.Key = key;
            this.Name = name;
            this.Headers = new ApiParameterCollection();
        }

        /// <summary>
        /// 解析一个参数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <param name="urlParameter"></param>
        /// <returns></returns>
        public static Token Parse(string key, string name, string urlParameter)
        {
            Token token = new Token(key, name);
            var items = urlParameter.Split('&');
            foreach (var item in items)
            {
                var vitem = item.Split('=');
                token.AddParameter(vitem[0], vitem[1]);
            }

            return token;
        }

        /// <summary>
        /// 查找指定名称的对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ApiParameter Find(string name)
        {
            return this.Headers.Find(p => p.Name == name);
        }

        /// <summary>
        /// 添加一个参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddParameter(string name, object value)
        {
            this.Headers.Add(name, value);
        }

        /// <summary>
        /// 添加一组参数
        /// </summary>
        /// <param name="names"></param>
        /// <param name="values"></param>
        public void AddParameter(string[] names, object[] values)
        {
            for (int index = 0; index < names.Length; index++)
            {
                AddParameter(names[index], values[index]);
            }
        }

        /// <summary>
        /// 添加一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        public void AddParameter<T>(T item)
            where T : class
        {
            //添加对象参数
            foreach (var p in CoreHelper.GetPropertiesFromType(item.GetType()))
            {
                AddParameter(p.Name, CoreHelper.GetPropertyValue(item, p));
            }
        }
    }
}
