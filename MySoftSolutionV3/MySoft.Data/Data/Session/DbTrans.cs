using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using MySoft.Data.Design;

namespace MySoft.Data
{
    /// <summary>
    /// 事务处理类
    /// </summary>
    public class DbTrans : IDbTrans, IDisposable
    {
        private DbConnection dbConnection;
        private DbTransaction dbTransaction;
        private DbProvider dbProvider;
        private DbBatch dbBatch;

        internal DbConnection Connection
        {
            get { return this.dbConnection; }
        }

        internal DbTransaction Transaction
        {
            get { return this.dbTransaction; }
        }

        /// <summary>
        /// 以DbTransaction方式实例化一个事务
        /// </summary>
        /// <param name="dbProvider"></param>
        /// <param name="dbTran"></param>
        internal DbTrans(DbProvider dbProvider, DbTransaction dbTran)
        {
            this.dbConnection = dbTran.Connection;
            this.dbTransaction = dbTran;
            if (this.dbConnection.State != ConnectionState.Open)
            {
                this.dbConnection.Open();
            }
            this.dbProvider = dbProvider;
            this.dbBatch = new DbBatch(dbProvider, this);
        }

        /// <summary>
        /// 以BbConnection方式实例化一个事务
        /// </summary>
        /// <param name="dbProvider"></param>
        /// <param name="dbConnection"></param>
        internal DbTrans(DbProvider dbProvider, DbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
            if (this.dbConnection.State != ConnectionState.Open)
            {
                this.dbConnection.Open();
            }
            this.dbProvider = dbProvider;
            this.dbBatch = new DbBatch(dbProvider, this);
        }

        internal DbTrans(DbProvider dbProvider, bool useTrans)
        {
            if (useTrans)
            {
                this.dbConnection = dbProvider.CreateConnection();
                this.dbConnection.Open();
                this.dbTransaction = dbConnection.BeginTransaction();
            }
            this.dbProvider = dbProvider;
            this.dbBatch = new DbBatch(dbProvider, this);
        }

        internal DbTrans(DbProvider dbProvider, IsolationLevel isolationLevel)
        {
            this.dbConnection = dbProvider.CreateConnection();
            this.dbConnection.Open();
            this.dbTransaction = dbConnection.BeginTransaction(isolationLevel);
            this.dbProvider = dbProvider;
            this.dbBatch = new DbBatch(dbProvider, this);
        }

        #region Batch操作

        /// <summary>
        /// 返回一个Batch
        /// </summary>
        /// <returns></returns>
        public DbBatch BeginBatch()
        {
            return BeginBatch(10);
        }

        /// <summary>
        /// 返回一个Batch
        /// </summary>
        /// <param name="batchSize">Batch大小</param>
        /// <returns></returns>
        public DbBatch BeginBatch(int batchSize)
        {
            return new DbBatch(dbProvider, this, batchSize);
        }

        #endregion

        #region Trans操作

        #region 增删改操作(指定表名)

        /// <summary>
        /// 保存一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Save<T>(Table table, T entity)
            where T : Entity
        {
            return dbBatch.Save<T>(table, entity);
        }


        /// <summary>
        ///  插入一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int Insert<T>(Table table, Field[] fields, object[] values)
            where T : Entity
        {
            List<FieldValue> fvlist = DataHelper.CreateFieldValue(fields, values, true);
            object retVal;
            return dbBatch.Insert<T>(table, fvlist, out retVal);
        }

        /// <summary>
        ///  插入一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int Insert<T, TResult>(Table table, Field[] fields, object[] values, out TResult retVal)
            where T : Entity
        {
            List<FieldValue> fvlist = DataHelper.CreateFieldValue(fields, values, true);
            object retValue;
            int ret = dbBatch.Insert<T>(table, fvlist, out retValue);
            retVal = CoreHelper.ConvertValue<TResult>(retValue);

            return ret;
        }

        /// <summary>
        /// 删除一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Delete<T>(Table table, T entity)
             where T : Entity
        {
            return dbBatch.Delete<T>(table, entity);
        }

        /// <summary>
        /// 删除指定主键的记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        public int Delete<T>(Table table, params object[] pkValues)
            where T : Entity
        {
            return dbBatch.Delete<T>(table, pkValues);
        }

        #endregion

        #region 增删改操作

        /// <summary>
        /// 保存一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Save<T>(T entity)
            where T : Entity
        {
            return dbBatch.Save(entity);
        }

        /// <summary>
        ///  插入一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int Insert<T>(Field[] fields, object[] values)
            where T : Entity
        {
            return Insert<T>(null, fields, values);
        }

        /// <summary>
        ///  插入一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int Insert<T, TResult>(Field[] fields, object[] values, out TResult retVal)
            where T : Entity
        {
            return Insert<T, TResult>(null, fields, values, out retVal);
        }

        /// <summary>
        /// 删除一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Delete<T>(T entity)
             where T : Entity
        {
            return dbBatch.Delete<T>(entity);
        }

        /// <summary>
        /// 按主键删除指定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        public int Delete<T>(params object[] pkValues)
            where T : Entity
        {
            return dbBatch.Delete<T>(pkValues);
        }

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int InsertOrUpdate<T>(T entity, params Field[] fields)
            where T : Entity
        {
            return dbBatch.InsertOrUpdate<T>(entity, fields);
        }

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fvs"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int InsertOrUpdate<T>(FieldValue[] fvs, WhereClip where)
            where T : Entity
        {
            return dbBatch.InsertOrUpdate<T>(fvs, where);
        }

        #endregion

        #region 事务操作

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            try
            {
                dbTransaction.Commit();
            }
            catch
            {
                this.Close();
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            try
            {
                dbTransaction.Rollback();
            }
            catch
            {
                this.Close();
            }
        }

        /// <summary>
        /// Dispose事务
        /// </summary>
        public void Dispose()
        {
            this.Close();
        }

        /// <summary>
        /// 关闭事务
        /// </summary>
        public void Close()
        {
            if (dbConnection.State != ConnectionState.Closed)
            {
                dbConnection.Close();
                dbConnection.Dispose();
            }
        }

        #endregion

        #endregion

        #region 常用操作(指定表名)

        /// <summary>
        /// 按主键获取一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        public T Single<T>(Table table, params object[] pkValues)
            where T : Entity
        {
            WhereClip where = DataHelper.GetPkWhere<T>(table, pkValues);
            return Single<T>(table, where);
        }

        /// <summary>
        /// 按条件获取一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public T Single<T>(Table table, WhereClip where)
            where T : Entity
        {
            return From<T>(table).Where(where).ToSingle();
        }

        /// <summary>
        /// 是否存在指定的实体，按主键匹配
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Exists<T>(Table table, T entity)
            where T : Entity
        {
            WhereClip where = DataHelper.GetPkWhere<T>(table, entity);
            return Exists<T>(table, where);
        }

        /// <summary>
        /// 是否存在指定主键的记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        public bool Exists<T>(Table table, params object[] pkValues)
            where T : Entity
        {
            WhereClip where = DataHelper.GetPkWhere<T>(table, pkValues);
            return Exists<T>(table, where);
        }

        /// <summary>
        /// 是否存在指定条件的记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public bool Exists<T>(Table table, WhereClip where)
            where T : Entity
        {
            return Count<T>(table, where) > 0;
        }

        /// <summary>
        /// 按条件获取记录条数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Count<T>(Table table, WhereClip where)
            where T : Entity
        {
            return From<T>(table).Where(where).Count();
        }

        /// <summary>
        /// 按条件进行Sum操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public object Sum<T>(Table table, Field field, WhereClip where)
            where T : Entity
        {
            return From<T>(table).Select(field.Sum()).Where(where).ToScalar();
        }

        /// <summary>
        /// 按条件进行Avg操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public object Avg<T>(Table table, Field field, WhereClip where)
            where T : Entity
        {
            return From<T>(table).Select(field.Avg()).Where(where).ToScalar();
        }

        /// <summary>
        /// 按条件进行Max操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public object Max<T>(Table table, Field field, WhereClip where)
            where T : Entity
        {
            return From<T>(table).Select(field.Max()).Where(where).ToScalar();
        }

        /// <summary>
        /// 按条件进行Min操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public object Min<T>(Table table, Field field, WhereClip where)
            where T : Entity
        {
            return From<T>(table).Select(field.Min()).Where(where).ToScalar();
        }

        #region 返回相应的类型

        /// <summary>
        /// 按条件进行Sum操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public TResult Sum<T, TResult>(Table table, Field field, WhereClip where)
            where T : Entity
        {
            return From<T>(table).Select(field.Sum()).Where(where).ToScalar<TResult>();
        }

        /// <summary>
        /// 按条件进行Avg操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public TResult Avg<T, TResult>(Table table, Field field, WhereClip where)
            where T : Entity
        {
            return From<T>(table).Select(field.Avg()).Where(where).ToScalar<TResult>();
        }

        /// <summary>
        /// 按条件进行Max操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public TResult Max<T, TResult>(Table table, Field field, WhereClip where)
            where T : Entity
        {
            return From<T>(table).Select(field.Max()).Where(where).ToScalar<TResult>();
        }

        /// <summary>
        /// 按条件进行Min操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public TResult Min<T, TResult>(Table table, Field field, WhereClip where)
            where T : Entity
        {
            return From<T>(table).Select(field.Min()).Where(where).ToScalar<TResult>();
        }

        #endregion

        #endregion

        #region 常用操作

        /// <summary>
        /// 按主键获取一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        public T Single<T>(params object[] pkValues)
            where T : Entity
        {
            return Single<T>(null, pkValues);
        }

        /// <summary>
        /// 按条件获取一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public T Single<T>(WhereClip where)
            where T : Entity
        {
            return Single<T>(null, where);
        }

        /// <summary>
        /// 是否存在指定的实体，按主键匹配
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Exists<T>(T entity)
            where T : Entity
        {
            return Exists(null, entity);
        }

        /// <summary>
        /// 是否存在指定主键的记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        public bool Exists<T>(params object[] pkValues)
            where T : Entity
        {
            return Exists<T>(null, pkValues);
        }

        /// <summary>
        /// 是否存在指定条件的记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public bool Exists<T>(WhereClip where)
            where T : Entity
        {
            return Exists<T>(null, where);
        }

        /// <summary>
        /// 按条件获取记录条数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Count<T>(WhereClip where)
            where T : Entity
        {
            return Count<T>(null, where);
        }

        /// <summary>
        /// 按条件进行Sum操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public object Sum<T>(Field field, WhereClip where)
            where T : Entity
        {
            return Sum<T>(null, field, where);
        }

        /// <summary>
        /// 按条件进行Avg操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public object Avg<T>(Field field, WhereClip where)
            where T : Entity
        {
            return Avg<T>(null, field, where);
        }

        /// <summary>
        /// 按条件进行Max操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public object Max<T>(Field field, WhereClip where)
            where T : Entity
        {
            return Max<T>(null, field, where);
        }

        /// <summary>
        /// 按条件进行Min操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public object Min<T>(Field field, WhereClip where)
            where T : Entity
        {
            return Min<T>(null, field, where);
        }

        #region 返回相应的类型

        /// <summary>
        /// 按条件进行Sum操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public TResult Sum<T, TResult>(Field field, WhereClip where)
            where T : Entity
        {
            return Sum<T, TResult>(null, field, where);
        }

        /// <summary>
        /// 按条件进行Avg操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public TResult Avg<T, TResult>(Field field, WhereClip where)
            where T : Entity
        {
            return Avg<T, TResult>(null, field, where);
        }

        /// <summary>
        /// 按条件进行Max操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public TResult Max<T, TResult>(Field field, WhereClip where)
            where T : Entity
        {
            return Max<T, TResult>(null, field, where);
        }

        /// <summary>
        /// 按条件进行Min操作
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="field"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public TResult Min<T, TResult>(Field field, WhereClip where)
            where T : Entity
        {
            return Min<T, TResult>(null, field, where);
        }

        #endregion

        #endregion

        #region 进行连接操作

        /// <summary>
        /// 返回一个From节
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FromSection<T> From<T>()
            where T : Entity
        {
            return From<T>((string)null);
        }

        /// <summary>
        /// 返回一个From节
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public QuerySection From<T>(TableRelation<T> relation)
            where T : Entity
        {
            var query = relation.Section.Query;

            //给查询设置驱动与事务
            query.SetDbProvider(dbProvider, this);

            //返回结果的查询
            var newquery = query.CreateQuery<ViewEntity>();

            return new QuerySection(newquery);
        }

        /// <summary>
        /// 返回一个查询
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public RelationQuery<TResult> Query<TResult, T>()
            where TResult : RelationEntity<T>
            where T : Entity
        {
            //判断是否存在关系
            var entity = CoreHelper.CreateInstance<TResult>();
            var relation = entity.GetRelation();
            var query = relation.Section.Query;

            //给查询设置驱动与事务
            query.SetDbProvider(dbProvider, this);

            //返回结果的查询
            var newquery = query.CreateQuery<ViewEntity>();

            //返回关系查询
            return new RelationQuery<TResult>(newquery);
        }

        /// <summary>
        /// 返回一个From节
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FromSection<T> From<T>(Table table)
            where T : Entity
        {
            return new FromSection<T>(dbProvider, this, table);
        }

        /// <summary>
        /// 返回一个From节，并可指定其别名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aliasName"></param>
        /// <returns></returns>
        public FromSection<T> From<T>(string aliasName)
            where T : Entity
        {
            return new FromSection<T>(dbProvider, this, aliasName);
        }

        /// <summary>
        /// 返回一个Sql节
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqlSection FromSql(string sql, params SQLParameter[] parameters)
        {
            SqlSection section = new SqlSection(sql, dbProvider, this);
            return section.AddParameters(parameters);
        }

        /// <summary>
        /// 返回一个Proc节
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        public ProcSection FromProc(string procName, params SQLParameter[] parameters)
        {
            ProcSection section = new ProcSection(procName, dbProvider, this);
            return section.AddParameters(parameters);
        }

        /// <summary>
        /// 返回一个Sql节
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqlSection FromSql(string sql, IDictionary<string, object> parameters)
        {
            SqlSection section = new SqlSection(sql, dbProvider, this);
            return section.AddParameters(parameters);
        }

        /// <summary>
        /// 返回一个Proc节
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        public ProcSection FromProc(string procName, IDictionary<string, object> parameters)
        {
            ProcSection section = new ProcSection(procName, dbProvider, this);
            return section.AddParameters(parameters);
        }

        #endregion

        #region 按创建器操作

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="creator"></param>
        /// <returns></returns>
        public int Execute(InsertCreator creator)
        {
            if (creator.Table == null)
            {
                throw new DataException("用创建器操作时，表不能为null！");
            }

            object retVal;
            return dbProvider.Insert<ViewEntity>(creator.Table, creator.FieldValues, this, creator.IdentityField, creator.SequenceName, false, out retVal);
        }

        /// <summary>
        ///  插入数据
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="creator"></param>
        /// <param name="identityValue"></param>
        /// <returns></returns>
        public int Execute<TResult>(InsertCreator creator, out TResult identityValue)
        {
            identityValue = default(TResult);

            if (creator.Table == null)
            {
                throw new DataException("用创建器操作时，表不能为null！");
            }

            if ((IField)creator.IdentityField == null)
            {
                throw new DataException("返回主键值时需要设置KeyField！");
            }

            object retVal;
            int ret = dbProvider.Insert<ViewEntity>(creator.Table, creator.FieldValues, this, creator.IdentityField, creator.SequenceName, true, out retVal);
            identityValue = CoreHelper.ConvertValue<TResult>(retVal);

            return ret;
        }

        /// <summary>
        /// 按条件删除指定记录
        /// </summary>
        /// <param name="creator"></param>
        /// <returns></returns>
        public int Execute(DeleteCreator creator)
        {
            if (creator.Table == null)
            {
                throw new DataException("用创建器操作时，表不能为null！");
            }

            if (DataHelper.IsNullOrEmpty(creator.Where))
            {
                throw new DataException("用删除创建器操作时，条件不能为空！");
            }

            return Delete<ViewEntity>(creator.Table, creator.Where);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="creator"></param>
        /// <returns></returns>
        public int Execute(UpdateCreator creator)
        {
            if (creator.Table == null)
            {
                throw new DataException("用创建器操作时，表不能为null！");
            }

            if (DataHelper.IsNullOrEmpty(creator.Where))
            {
                throw new DataException("用更新创建器操作时，条件不能为空！");
            }

            return Update<ViewEntity>(creator.Table, creator.Fields, creator.Values, creator.Where);
        }

        /// <summary>
        /// 返回一个Query节
        /// </summary>
        /// <returns></returns>
        public QuerySection From(QueryCreator creator)
        {
            if (creator.Table == null)
            {
                throw new DataException("用创建器操作时，表不能为null！");
            }

            FromSection<ViewEntity> f = this.From<ViewEntity>(creator.Table);
            if (creator.IsRelation)
            {
                foreach (TableJoin join in creator.Relations.Values)
                {
                    if (join.Type == JoinType.LeftJoin)
                        f.LeftJoin<ViewEntity>(join.Table, join.Where);
                    else if (join.Type == JoinType.RightJoin)
                        f.RightJoin<ViewEntity>(join.Table, join.Where);
                    else
                        f.InnerJoin<ViewEntity>(join.Table, join.Where);
                }
            }

            QuerySection<ViewEntity> query = f.Select(creator.Fields).Where(creator.Where)
                    .OrderBy(creator.OrderBy);

            return new QuerySection(query);
        }

        #endregion

        #region 按条件操作

        /// <summary>
        /// 按条件删除指定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Delete<T>(WhereClip where)
            where T : Entity
        {
            return Delete<T>(null, where);
        }

        /// <summary>
        /// 按条件更新指定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Update<T>(Field field, object value, WhereClip where)
            where T : Entity
        {
            return Update<T>(null, field, value, where);
        }

        /// <summary>
        /// 按条件更新指定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Update<T>(Field[] fields, object[] values, WhereClip where)
            where T : Entity
        {
            return Update<T>(null, fields, values, where);
        }

        /// <summary>
        /// 按条件删除指定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Delete<T>(Table table, WhereClip where)
            where T : Entity
        {
            return dbProvider.Delete<T>(table, where, this);
        }

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int InsertOrUpdate<T>(Table table, T entity, params Field[] fields)
            where T : Entity
        {
            return dbBatch.InsertOrUpdate<T>(table, entity, fields);
        }

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fvs"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int InsertOrUpdate<T>(Table table, FieldValue[] fvs, WhereClip where)
            where T : Entity
        {
            return dbBatch.InsertOrUpdate<T>(table, fvs, where);
        }

        /// <summary>
        /// 按条件更新指定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Update<T>(Table table, Field field, object value, WhereClip where)
            where T : Entity
        {
            return Update<T>(table, new Field[] { field }, new object[] { value }, where);
        }

        /// <summary>
        /// 按条件更新指定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Update<T>(Table table, Field[] fields, object[] values, WhereClip where)
            where T : Entity
        {
            List<FieldValue> fvlist = DataHelper.CreateFieldValue(fields, values, false);
            return dbProvider.Update<T>(table, fvlist, where, this);
        }

        #endregion

        #region 支持FieldValue方式

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fvs"></param>
        /// <returns></returns>
        public int Insert<T>(FieldValue[] fvs)
            where T : Entity
        {
            return Insert<T>(null, fvs);
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="fvs"></param>
        /// <param name="retVal"></param>
        /// <returns></returns>
        public int Insert<T, TResult>(FieldValue[] fvs, out TResult retVal)
            where T : Entity
        {
            return Insert<T, TResult>(null, fvs, out retVal);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fv"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Update<T>(FieldValue fv, WhereClip where)
            where T : Entity
        {
            return Update<T>(null, fv, where);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fvs"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Update<T>(FieldValue[] fvs, WhereClip where)
            where T : Entity
        {
            return Update<T>(null, fvs, where);
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="fvs"></param>
        /// <returns></returns>
        public int Insert<T>(Table table, FieldValue[] fvs)
            where T : Entity
        {
            List<FieldValue> list = new List<FieldValue>(fvs);
            list.ForEach(p =>
            {
                if (p.Value is Field) p.IsIdentity = true;
            });

            object retVal;
            return dbBatch.Insert<T>(table, list, out retVal);
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="table"></param>
        /// <param name="fvs"></param>
        /// <param name="retVal"></param>
        /// <returns></returns>
        public int Insert<T, TResult>(Table table, FieldValue[] fvs, out TResult retVal)
            where T : Entity
        {
            List<FieldValue> list = new List<FieldValue>(fvs);
            list.ForEach(p =>
            {
                if (p.Value is Field) p.IsIdentity = true;
            });

            object retValue;
            int ret = dbBatch.Insert<T>(table, list, out retValue);
            retVal = CoreHelper.ConvertValue<TResult>(retValue);

            return ret;
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="fv"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Update<T>(Table table, FieldValue fv, WhereClip where)
            where T : Entity
        {
            return Update<T>(table, new FieldValue[] { fv }, where);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="fvs"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Update<T>(Table table, FieldValue[] fvs, WhereClip where)
            where T : Entity
        {
            List<FieldValue> list = new List<FieldValue>(fvs);
            list.ForEach(p =>
            {
                p.IsChanged = true;
            });

            return dbProvider.Update<T>(table, list, where, this);
        }

        #endregion

        #region 插入时返回标识列

        /// <summary>
        /// 插入实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="entity"></param>
        /// <param name="retVal"></param>
        /// <returns></returns>
        public int Insert<T, TResult>(T entity, out TResult retVal, params FieldValue[] fvs)
            where T : Entity
        {
            return Insert<T, TResult>(null, entity, out retVal);
        }

        /// <summary>
        /// 插入实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="table"></param>
        /// <param name="entity"></param>
        /// <param name="retVal"></param>
        /// <returns></returns>
        public int Insert<T, TResult>(Table table, T entity, out TResult retVal, params FieldValue[] fvs)
            where T : Entity
        {
            var list = entity.GetFieldValues();
            if (fvs != null && fvs.Length > 0)
            {
                foreach (var fv in fvs)
                {
                    if (fv.Value == null) continue;

                    var item = list.Find(p => p.Field.Name == fv.Field.Name);
                    if (item != null)
                    {
                        if (fv.Value is DbValue && DbValue.Default.Equals(fv.Value))
                        {
                            list.Remove(item);
                        }

                        item.Value = fv.Value;
                    }
                }
            }

            #region 实体验证处理

            //对实体进行验证
            ValidateResult result = entity.Validation();
            if (!result.IsSuccess)
            {
                List<string> msgs = new List<string>();
                foreach (var msg in result.InvalidValues)
                {
                    msgs.Add(msg.Field.PropertyName + " : " + msg.Message);
                }
                string message = string.Join("\r\n", msgs.ToArray());
                throw new DataException(message);
            }

            #endregion

            return Insert<T, TResult>(table, list.ToArray(), out retVal);
        }

        #endregion
    }
}
