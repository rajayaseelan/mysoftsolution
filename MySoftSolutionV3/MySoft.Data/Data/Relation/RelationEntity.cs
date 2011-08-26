using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.Data
{
    /// <summary>
    /// 关系实体
    /// </summary>
    [Serializable]
    public abstract class RelationEntity<T>
        where T : Entity
    {
        /// <summary>
        /// 返回表关系
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected internal abstract TableRelation<T> GetRelation();
    }
}
