using System;

namespace MySoft.Data.Design
{
    /// <summary>
    /// 只用于显示的实体
    /// </summary>
    [Serializable]
    public sealed class ViewEntity : Entity
    {
        /// <summary>
        /// 重载GetFields
        /// </summary>
        /// <returns></returns>
        internal protected override Field[] GetFields()
        {
            return new Field[] { };
        }

        /// <summary>
        /// 重载GetValues
        /// </summary>
        /// <returns></returns>
        protected override object[] GetValues()
        {
            return new object[] { };
        }

        /// <summary>
        /// 重载SetValues
        /// </summary>
        /// <param name="reader"></param>
        protected override void SetValues(IRowReader reader)
        { }
    }
}
