using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MySoft
{
    /// <summary>
    /// 验证帮助类
    /// </summary>
    public static class ValidateHelper
    {
        /// <summary>
        /// 开始验证
        /// </summary>
        /// <returns></returns>
        public static Validation Begin()
        {
            return null;
        }
    }

    /// <summary>
    /// 验证类
    /// </summary>
    public sealed class Validation
    {
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid { get; set; }
    }

    /// <summary>
    /// 验证扩展类
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// 通讯检测，抛出异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="validation"></param>
        /// <param name="filterMethod"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static Validation Check<T>(this Validation validation, Func<bool> filterMethod, T exception) where T : Exception
        {
            if (filterMethod())
            {
                return validation ?? new Validation() { IsValid = true };
            }
            else
            {
                throw exception;
            }
        }

        /// <summary>
        /// 通讯检测
        /// </summary>
        /// <param name="validation"></param>
        /// <param name="filterMethod"></param>
        /// <returns></returns>
        public static Validation Check(this Validation validation, Func<bool> filterMethod)
        {
            return Check<Exception>(validation, filterMethod, new Exception("Parameter InValid!"));
        }

        /// <summary>
        /// 是否为null
        /// </summary>
        /// <param name="validation"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Validation NotNull(this Validation validation, Object obj)
        {
            return Check<ArgumentNullException>(validation,
                () => obj != null,
                new ArgumentNullException(string.Format("Parameter {0} can't be null", obj))
            );
        }

        /// <summary>
        /// 数据区间判断
        /// </summary>
        /// <param name="validation"></param>
        /// <param name="obj"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Validation InRange(this Validation validation, double obj, double min, double max)
        {
            return Check<ArgumentOutOfRangeException>(validation,
                () =>
                {
                    double input = double.Parse(obj.ToString());
                    if (obj >= min && obj <= max)
                        return true;
                    else
                        return false;
                },
                new ArgumentOutOfRangeException(string.Format("Parameter should be between {0} and {1}", min, max))
            );
        }

        /// <summary>
        /// 正则匹配
        /// </summary>
        /// <param name="validation"></param>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static Validation RegexMatch(this Validation validation, string input, string pattern)
        {
            return Check<ArgumentException>(validation,
                () => Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase),
                new ArgumentException(string.Format("Parameter should match format {0}", pattern))
            );
        }

        /// <summary>
        /// 是否Email
        /// </summary>
        /// <param name="validation"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static Validation IsEmail(this Validation validation, string email)
        {
            return RegexMatch(validation, email, @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,6}$");
        }
    }
}
