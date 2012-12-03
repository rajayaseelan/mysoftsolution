using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace MySoft
{
    /// <summary>
    /// Dynamic reflection cache
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    internal static class DynamicReflectionCache<TKey, TValue>
    {
        private static Hashtable m_cache = Hashtable.Synchronized(new Hashtable());

        public static TValue Get(TKey key, Func<TKey, TValue> func)
        {
            TValue value = default(TValue);

            if (key == null) return value;

            if (m_cache.ContainsKey(key))
            {
                value = (TValue)m_cache[key];
            }
            else
            {
                value = func(key);

                if (value != null)
                {
                    m_cache[key] = value;
                }
            }

            return value;
        }
    }

    /// <summary>
    /// 动态反射扩展类
    /// </summary>
    public static class DynamicReflectionExtentions
    {
        /// <summary>
        /// 快速创建实例
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object FastInvoke(this Type type)
        {
            var invoker = DynamicReflectionCache<Type, FastInstanceHandler>.Get(type, DynamicCalls.GetInstanceInvoker);
            return invoker();
        }

        /// <summary>
        /// 快速调用方法
        /// </summary>
        /// <param name="method"></param>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object FastInvoke(this MethodInfo method, object target, object[] parameters)
        {
            var invoker = DynamicReflectionCache<MethodInfo, FastInvokeHandler>.Get(method, DynamicCalls.GetMethodInvoker);
            return invoker(target, parameters);
        }

        /// <summary>
        /// 快速获取属性
        /// </summary>
        /// <param name="property"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static object FastGetValue(this PropertyInfo property, object target)
        {
            var invoker = DynamicReflectionCache<PropertyInfo, FastPropertyGetHandler>.Get(property, DynamicCalls.GetPropertyGetter);
            return invoker(target);
        }

        /// <summary>
        /// 快速属性赋值
        /// </summary>
        /// <param name="property"></param>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void FastSetValue(this PropertyInfo property, object target, object value)
        {
            var invoker = DynamicReflectionCache<PropertyInfo, FastPropertySetHandler>.Get(property, DynamicCalls.GetPropertySetter);
            invoker(target, value);
        }
    }
}
