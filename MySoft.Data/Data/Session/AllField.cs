using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data
{
    /// <summary>
    /// 所有字段（特殊字段）
    /// </summary>
    [Serializable]
    public class AllField<T> : AllField
        where T : Entity
    {
        /// <summary>
        /// All实例化
        /// </summary>
        public AllField()
            : base()
        {
            this.tableName = Table.GetTable<T>().OriginalName;
        }

        /// <summary>
        /// 通过属性返回字段
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public override Field this[string propertyName]
        {
            get
            {
                return EntityCache<T>.Get(() => CoreHelper.CreateInstance<T>())
                        .As<IEntityBase>().GetField(propertyName);
            }
        }
    }

    /// <summary>
    /// 所有字段（特殊字段）
    /// </summary>
    [Serializable]
    public class AllField : Field
    {
        /// <summary>
        /// All实例化
        /// </summary>
        public AllField() : base("All", null, "*", null) { }

        /// <summary>
        /// 选择被排除的列
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public IFieldFilter Exclude(params Field[] fields)
        {
            return new ExcludeField(fields);
        }

        /// <summary>
        /// 选择被选择的列
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public IFieldFilter Include(params Field[] fields)
        {
            return new IncludeField(fields);
        }

        /// <summary>
        /// 从实体中获取属性转换成Field
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public virtual Field this[string propertyName]
        {
            get
            {
                return new Field(propertyName);
            }
        }
    }

    /// <summary>
    /// 字段筛选
    /// </summary>
    public interface IFieldFilter
    {
        /// <summary>
        /// 获取字段
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        Field[] GetFields(Field[] fields);
    }
}
