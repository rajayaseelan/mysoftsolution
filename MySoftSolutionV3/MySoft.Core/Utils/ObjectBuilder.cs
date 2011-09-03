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
        public IList<PropertyHandler> mProperties = new List<PropertyHandler>();

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

            foreach (PropertyHandler handler in this.Properties)
            {
                if (handler.Property.CanWrite)
                {
                    object obj3 = values[mPrefix + "." + handler.Property.Name];
                    if (obj3 == null)
                    {
                        obj3 = values[mPrefix + "_" + handler.Property.Name];
                    }
                    if (obj3 == null)
                    {
                        obj3 = values[mPrefix + handler.Property.Name];
                    }
                    this.BindProperty(obj2, handler, (string)obj3);
                }
            }
            return obj2;
        }

        private void BindProperty(object obj, PropertyHandler property, string value)
        {
            bool succeeded = false;
            if (ConverterFactory.Converters.ContainsKey(property.Property.PropertyType))
            {
                object newobj = ConverterFactory.Converters[property.Property.PropertyType].ConvertTo(value, out succeeded);
                if (succeeded)
                {
                    property.Set(obj, newobj);
                }
            }
        }

        protected void OnInit()
        {
            foreach (PropertyInfo info in this.ObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                this.Properties.Add(new PropertyHandler(info));
            }
        }

        internal Type ObjectType
        {
            get
            {
                return this.mObjectType;
            }
        }

        internal IList<PropertyHandler> Properties
        {
            get
            {
                return this.mProperties;
            }
        }
    }
}

