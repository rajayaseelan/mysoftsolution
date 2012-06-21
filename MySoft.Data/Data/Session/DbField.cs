using System;

namespace MySoft.Data
{
    /// <summary>
    /// 数据库字段
    /// </summary>
    [Serializable]
    public class DbField : Field
    {
        public DbField(string fieldName)
            : base(string.Format("__${0}$__", fieldName)) { }

        /// <summary>
        /// 返回原始字段名称
        /// </summary>
        internal override string Name
        {
            get
            {
                return base.OriginalName;
            }
        }
    }

    interface IProvider
    {
        /// <summary>
        /// 设置驱动
        /// </summary>
        /// <param name="dbProvider"></param>
        /// <param name="dbTran"></param>
        void SetDbProvider(DbProvider dbProvider, DbTrans dbTran);
    }

    /// <summary>
    /// 系统字段
    /// </summary>
    [Serializable]
    internal class CustomField : Field, IProvider
    {
        private QueryCreator creator;
        private string qString;
        public CustomField(string fieldName, QueryCreator creator)
            : base(fieldName)
        {
            this.creator = creator;
        }

        /// <summary>
        /// 处理qString;
        /// </summary>
        /// <param name="dbProvider"></param>
        /// <param name="dbTran"></param>
        void IProvider.SetDbProvider(DbProvider dbProvider, DbTrans dbTran)
        {
            if (creator != null)
            {
                var query = GetQuery(creator);
                query.SetDbProvider(dbProvider, dbTran);
                qString = query.GetTop(1).QueryString;
            }
        }

        /// <summary>
        /// 重载名称
        /// </summary>
        internal override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(qString))
                {
                    throw new DataException("需要设置DbProvider及DbTrans才能处理CustomField！");
                }
                return string.Format("({1}) as {0}", base.Name, qString);
            }
        }
    }

    /// <summary>
    /// 系统字段
    /// </summary>
    [Serializable]
    internal class CustomField<T> : Field, IProvider
        where T : Entity
    {
        private TableRelation<T> relation;
        private string qString;
        public CustomField(string fieldName, QuerySection<T> query)
            : base(fieldName)
        {
            this.qString = query.GetTop(1).QueryString;
        }

        public CustomField(string fieldName, TableRelation<T> relation)
            : base(fieldName)
        {
            this.relation = relation;
        }

        /// <summary>
        /// 处理qString;
        /// </summary>
        /// <param name="dbProvider"></param>
        /// <param name="dbTran"></param>
        void IProvider.SetDbProvider(DbProvider dbProvider, DbTrans dbTran)
        {
            if (string.IsNullOrEmpty(qString) && relation != null)
            {
                var query = relation.GetFromSection().Query;
                query.SetDbProvider(dbProvider, dbTran);
                qString = query.GetTop(1).QueryString;
            }
        }

        /// <summary>
        /// 重载名称
        /// </summary>
        internal override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(qString))
                {
                    throw new DataException("需要设置DbProvider及DbTrans才能处理CustomField！");
                }
                return string.Format("({1}) as {0}", base.Name, qString);
            }
        }
    }
}
