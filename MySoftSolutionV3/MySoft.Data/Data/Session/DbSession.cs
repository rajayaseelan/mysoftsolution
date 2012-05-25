using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using MySoft.Logger;
using MySoft.Cache;
using MySoft.Data.Cache;
using MySoft.Data.Logger;

namespace MySoft.Data
{
    /// <summary>
    /// 会话操作对象
    /// </summary>
    public partial class DbSession : IDbSession, ICacheDependent
    {
        /// <summary>
        /// 默认会话对象
        /// </summary>
        public static DbSession Default;
        private DbProvider dbProvider;
        private DbTrans dbTrans;
        private string connectName;

        #region 初始化Session

        static DbSession()
        {
            if (Default == null)
            {
                try
                {
                    Default = new DbSession(DbProviderFactory.Default);
                }
                catch { }
            }
        }

        /// <summary>
        ///  指定配制节名实例化一个Session会话
        /// </summary>
        /// <param name="connectName"></param>
        public DbSession(string connectName)
            : this(DbProviderFactory.CreateDbProvider(connectName))
        {
            this.connectName = connectName;
        }

        /// <summary>
        /// 指定驱动实例化一个Session会话
        /// </summary>
        /// <param name="dbProvider"></param>
        public DbSession(DbProvider dbProvider)
        {
            try
            {
                this.connectName = dbProvider.ToString();
                InitSession(dbProvider);
            }
            catch
            {
                throw new DataException("初始化DbSession失败，请检查配置是否正确！");
            }
        }

        /// <summary>
        /// 设置指定配制节名Session会话为默认会话
        /// </summary>
        /// <param name="connectName"></param>
        public static void SetDefault(string connectName)
        {
            Default = new DbSession(connectName);
        }

        /// <summary>
        /// 设置指定驱动Session会话为默认会话
        /// </summary>
        /// <param name="dbProvider"></param>
        public static void SetDefault(DbProvider dbProvider)
        {
            Default = new DbSession(dbProvider);
        }

        /// <summary>
        /// 设置指定的会话为默认会话
        /// </summary>
        /// <param name="dbSession"></param>
        public static void SetDefault(DbSession dbSession)
        {
            Default = dbSession;
        }

        #endregion

        #region 实现IDbSession

        /// <summary>
        /// 设置新的驱动
        /// </summary>
        /// <param name="connectName"></param>
        public void SetProvider(string connectName)
        {
            SetProvider(DbProviderFactory.CreateDbProvider(connectName));
        }

        /// <summary>
        /// 设置新的驱动
        /// </summary>
        /// <param name="dbProvider"></param>
        public void SetProvider(DbProvider dbProvider)
        {
            InitSession(dbProvider);
        }

        /// <summary>
        /// 开始一个事务
        /// </summary>
        /// <returns></returns>
        public DbTrans BeginTrans()
        {
            return new DbTrans(dbProvider, true);
        }

        /// <summary>
        /// 开始一个事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        public DbTrans BeginTrans(IsolationLevel isolationLevel)
        {
            return new DbTrans(dbProvider, isolationLevel);
        }

        /// <summary>
        /// 设置一个外部事务
        /// </summary>
        /// <param name="trans"></param>
        /// <returns></returns>
        public DbTrans SetTransaction(DbTransaction trans)
        {
            return new DbTrans(dbProvider, trans);
        }

        /// <summary>
        /// 开始一个外部事务
        /// </summary>
        /// <returns></returns>
        public DbTransaction BeginTransaction()
        {
            return BeginTrans().Transaction;
        }

        /// <summary>
        /// 开始一个外部事务
        /// </summary>
        /// <param name="isolationLevel"></param>
        /// <returns></returns>
        public DbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return BeginTrans(isolationLevel).Transaction;
        }

        /// <summary>
        /// 设置一个外部链接
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public DbTrans SetConnection(DbConnection connection)
        {
            return new DbTrans(dbProvider, connection);
        }

        /// <summary>
        /// 创建一个外部连接
        /// </summary>
        /// <returns></returns>
        public DbConnection CreateConnection()
        {
            return dbProvider.CreateConnection();
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <returns></returns>
        public DbParameter CreateParameter()
        {
            return dbProvider.CreateParameter();
        }

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
            return dbTrans.Single<T>(table, pkValues);
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
            return dbTrans.Single<T>(table, where);
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
            return dbTrans.Exists<T>(table, entity);
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
            return dbTrans.Exists<T>(table, pkValues);
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
            return dbTrans.Exists<T>(table, where);
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
            return dbTrans.Count<T>(table, where);
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
            return dbTrans.Sum<T>(table, field, where);
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
            return dbTrans.Avg<T>(table, field, where);
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
            return dbTrans.Max<T>(table, field, where);
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
            return dbTrans.Min<T>(table, field, where);
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
            return dbTrans.Sum<T, TResult>(table, field, where);
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
            return dbTrans.Avg<T, TResult>(table, field, where);
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
            return dbTrans.Max<T, TResult>(table, field, where);
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
            return dbTrans.Min<T, TResult>(table, field, where);
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
            return dbTrans.Single<T>(pkValues);
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
            return dbTrans.Single<T>(where);
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
            return dbTrans.Exists<T>(entity);
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
            return dbTrans.Exists<T>(pkValues);
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
            return dbTrans.Exists<T>(where);
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
            return dbTrans.Count<T>(where);
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
            return dbTrans.Sum<T>(field, where);
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
            return dbTrans.Avg<T>(field, where);
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
            return dbTrans.Max<T>(field, where);
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
            return dbTrans.Min<T>(field, where);
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
            return dbTrans.Sum<T, TResult>(field, where);
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
            return dbTrans.Avg<T, TResult>(field, where);
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
            return dbTrans.Max<T, TResult>(field, where);
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
            return dbTrans.Min<T, TResult>(field, where);
        }

        #endregion

        #endregion

        #region 进行连接操作

        /// <summary>
        /// 返回一个Query节
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public QuerySection<T> From<T>(TableRelation<T> relation)
            where T : Entity
        {
            return dbTrans.From<T>(relation);
        }

        /// <summary>
        /// 返回一个From节
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FromSection<T> From<T>()
            where T : Entity
        {
            return dbTrans.From<T>();
        }

        /// <summary>
        /// 返回一个From节
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FromSection<T> From<T>(Table table)
            where T : Entity
        {
            return dbTrans.From<T>(table);
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
            return dbTrans.From<T>(aliasName);
        }

        /// <summary>
        /// 返回一个Sql节
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqlSection FromSql(string sql, params SQLParameter[] parameters)
        {
            return dbTrans.FromSql(sql, parameters);
        }

        /// <summary>
        /// 返回一个Proc节
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        public ProcSection FromProc(string procName, params SQLParameter[] parameters)
        {
            return dbTrans.FromProc(procName, parameters);
        }

        /// <summary>
        /// 返回一个Sql节
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqlSection FromSql(string sql, IDictionary<string, object> parameters)
        {
            return dbTrans.FromSql(sql, parameters);
        }

        /// <summary>
        /// 返回一个Proc节
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        public ProcSection FromProc(string procName, IDictionary<string, object> parameters)
        {
            return dbTrans.FromProc(procName, parameters);
        }

        #endregion

        #region 使用创建器操作

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="creator"></param>
        /// <returns></returns>
        public int Execute(InsertCreator creator)
        {
            return dbTrans.Execute(creator);
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
            return dbTrans.Execute(creator, out identityValue);
        }

        /// <summary>
        /// 按条件删除指定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="creator"></param>
        /// <returns></returns>
        public int Execute(DeleteCreator creator)
        {
            return dbTrans.Execute(creator);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="creator"></param>
        /// <returns></returns>
        public int Execute(UpdateCreator creator)
        {
            return dbTrans.Execute(creator);
        }

        /// <summary>
        ///  返回一个Query节
        /// </summary>
        /// <param name="creator"></param>
        /// <returns></returns>
        public QuerySection From(QueryCreator creator)
        {
            return dbTrans.From(creator);
        }

        #endregion

        #region 增删改操作

        /// <summary>
        /// 返回一个Batch
        /// </summary>
        /// <returns></returns>
        public DbBatch BeginBatch()
        {
            return dbTrans.BeginBatch();
        }

        /// <summary>
        /// 返回一个Batch
        /// </summary>
        /// <returns></returns>
        public DbBatch BeginBatch(int batchSize)
        {
            return dbTrans.BeginBatch(batchSize);
        }

        /// <summary>
        /// 保存一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Save<T>(Table table, T entity)
             where T : Entity
        {
            return dbTrans.Save<T>(table, entity);
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
            return dbTrans.Insert<T>(table, fields, values);
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
            return dbTrans.Insert<T, TResult>(table, fields, values, out retVal);
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
            return dbTrans.Delete<T>(table, entity);
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
            return dbTrans.Delete<T>(table, pkValues);
        }

        /// <summary>
        /// 删除符合条件的记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Delete<T>(Table table, WhereClip where)
            where T : Entity
        {
            return dbTrans.Delete<T>(table, where);
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
            return dbTrans.InsertOrUpdate<T>(table, entity, fields);
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
            return dbTrans.InsertOrUpdate<T>(table, fvs, where);
        }

        /// <summary>
        /// 更新指定条件的记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Update<T>(Table table, Field field, object value, WhereClip where)
            where T : Entity
        {
            return dbTrans.Update<T>(table, field, value, where);
        }

        /// <summary>
        /// 更新指定条件的记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Update<T>(Table table, Field[] fields, object[] values, WhereClip where)
            where T : Entity
        {
            return dbTrans.Update<T>(table, fields, values, where);
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
            return dbTrans.Save(entity);
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
            return dbTrans.Insert<T>(fields, values);
        }

        /// <summary>
        /// 插入一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="fields"></param>
        /// <param name="values"></param>
        /// <param name="retVal"></param>
        /// <returns></returns>
        public int Insert<T, TResult>(Field[] fields, object[] values, out TResult retVal)
            where T : Entity
        {
            return dbTrans.Insert<T, TResult>(fields, values, out retVal);
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
            return dbTrans.Delete<T>(entity);
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
            return dbTrans.Delete<T>(pkValues);
        }

        /// <summary>
        /// 按条件删除指定记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public int Delete<T>(WhereClip where)
            where T : Entity
        {
            return dbTrans.Delete<T>(where);
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
            return dbTrans.InsertOrUpdate<T>(entity, fields);
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
            return dbTrans.InsertOrUpdate<T>(fvs, where);
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
            return dbTrans.Update<T>(field, value, where);
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
            return dbTrans.Update<T>(fields, values, where);
        }

        #endregion

        #region 系列化WhereClip

        /// <summary>
        /// 返回最终条件的SQL
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public string Serialization(WhereClip where)
        {
            string sql = dbProvider.FormatCommandText(where.ToString());
            foreach (SQLParameter p in where.Parameters)
            {
                sql = sql.Replace(p.Name, DataHelper.FormatValue(p.Value));
            }
            return sql;
        }

        /// <summary>
        /// 返回最终排序的SQL
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public string Serialization(OrderByClip order)
        {
            return dbProvider.FormatCommandText(order.ToString());
        }

        #endregion

        #region 注入信息

        /// <summary>
        /// 注册解密的Handler
        /// </summary>
        /// <param name="handler"></param>
        public void RegisterDecryptor(DecryptEventHandler handler)
        {
            this.dbProvider.SetDecryptHandler(handler);
        }

        /// <summary>
        /// 注册日志依赖
        /// </summary>
        /// <param name="logger"></param>
        public void RegisterLogger(IExecuteLog logger)
        {
            this.dbProvider.Logger = logger;
        }

        /// <summary>
        /// 注册缓存依赖
        /// </summary>
        /// <param name="cache"></param>
        public void RegisterCache(ICacheStrategy cache)
        {
            this.dbProvider.Cache = new DataCacheDependent(cache, connectName);
        }

        /// <summary>
        /// 设置命令操作超时时间
        /// </summary>
        /// <param name="timeout"></param>
        public void SetCommandTimeout(int timeout)
        {
            this.dbProvider.Timeout = timeout;
        }

        /// <summary>
        /// 设置是否抛出异常
        /// </summary>
        /// <param name="throwError"></param>
        public void SetThrowError(bool throwError)
        {
            this.dbProvider.ThrowError = throwError;
        }

        #endregion

        #region 私有方法

        private void InitSession(DbProvider dbProvider)
        {
            this.dbProvider = dbProvider;
            this.dbTrans = new DbTrans(dbProvider, false);
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
            return dbTrans.Insert<T>(fvs);
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
            return dbTrans.Insert<T, TResult>(fvs, out retVal);
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
            return dbTrans.Update<T>(fv, where);
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
            return dbTrans.Update<T>(fvs, where);
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
            return dbTrans.Insert<T>(table, fvs);
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
            return dbTrans.Insert<T, TResult>(table, fvs, out retVal);
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
            return dbTrans.Update<T>(table, fv, where);
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
            return dbTrans.Update<T>(table, fvs, where);
        }

        #endregion

        #region 插入时返回标识列

        /// <summary>
        /// 插入实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Insert<T>(T entity, params FieldValue[] fvs)
            where T : Entity
        {
            return dbTrans.Insert<T>(entity, fvs);
        }

        /// <summary>
        /// 插入实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="table"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Insert<T>(Table table, T entity, params FieldValue[] fvs)
            where T : Entity
        {
            return dbTrans.Insert<T>(table, entity, fvs);
        }

        #endregion

        #endregion

        #region ICacheDependent 成员

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="cacheValue"></param>
        /// <param name="cacheTime"></param>
        public void AddCache<T>(string cacheKey, T cacheValue, int cacheTime)
        {
            if (dbProvider.Cache != null)
                dbProvider.Cache.AddCache<T>(cacheKey, cacheValue, cacheTime);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        public void RemoveCache<T>(string cacheKey)
        {
            if (dbProvider.Cache != null)
                dbProvider.Cache.RemoveCache<T>(cacheKey);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public T GetCache<T>(string cacheKey)
        {
            if (dbProvider.Cache != null)
                return dbProvider.Cache.GetCache<T>(cacheKey);
            else
                return default(T);
        }

        #endregion
    }
}
