using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using MySoft.Converter;

namespace MySoft
{
    /// <summary>
    /// 对象构造器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectBuilder<T> : ObjectBuilder
    {
        public ObjectBuilder()
            : base(typeof(T))
        { }

        public ObjectBuilder(string prefix)
            : base(typeof(T), prefix)
        { }

        public new T Bind(NameValueCollection values)
        {
            return (T)base.Bind(values);
        }
    }

    /// <summary>
    /// 对象构造器
    /// </summary>
    public class ObjectBuilder
    {
        private Type mObjectType;
        private string mPrefix;
        public IList<PropertyInfo> mProperties = new List<PropertyInfo>();

        public ObjectBuilder(Type objType)
        {
            this.mObjectType = objType;
            this.OnInit();
        }

        public ObjectBuilder(Type objType, string prefix)
        {
            this.mObjectType = objType;
            this.mPrefix = prefix;
            this.OnInit();
        }

        public object Bind(NameValueCollection values)
        {
            object obj2 = Activator.CreateInstance(this.ObjectType);
            if (mPrefix == null) mPrefix = "";

            foreach (PropertyInfo property in this.Properties)
            {
                if (property.CanWrite)
                {
                    object obj3 = values[mPrefix + "." + property.Name];
                    if (obj3 == null)
                    {
                        obj3 = values[mPrefix + "_" + property.Name];
                    }
                    if (obj3 == null)
                    {
                        obj3 = values[mPrefix + property.Name];
                    }
                    this.BindProperty(obj2, property, (string)obj3);
                }
            }
            return obj2;
        }

        private void BindProperty(object obj, PropertyInfo property, string value)
        {
            bool succeeded = false;
            if (ConverterFactory.Converters.ContainsKey(property.PropertyType))
            {
                object newobj = ConverterFactory.Converters[property.PropertyType].ConvertTo(value, out succeeded);
                if (succeeded)
                {
                    var setter = DynamicCalls.GetPropertySetter(property);
                    setter(obj, newobj);
                }
            }
        }

        protected void OnInit()
        {
            foreach (PropertyInfo info in CoreHelper.GetPropertiesFromType(this.ObjectType))
            {
                this.Properties.Add(info);
            }
        }

        internal Type ObjectType
        {
            get
            {
                return this.mObjectType;
            }
        }

        internal IList<PropertyInfo> Properties
        {
            get
            {
                return this.mProperties;
            }
        }
    }
}

