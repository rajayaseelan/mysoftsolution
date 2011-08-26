using System.Collections.Generic;

namespace MySoft.Data
{
    interface IDbProcess
    {
        #region 增删改操作

        int Save<T>(T entity) where T : Entity;
        int Delete<T>(T entity) where T : Entity;
        int Delete<T>(params object[] pkValues) where T : Entity;

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        int InsertOrUpdate<T>(T entity, params Field[] fields) where T : Entity;

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fvs"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        int InsertOrUpdate<T>(FieldValue[] fvs, WhereClip where) where T : Entity;

        #endregion

        #region 增删改操作(分表处理)

        int Save<T>(Table table, T entity) where T : Entity;
        int Delete<T>(Table table, T entity) where T : Entity;
        int Delete<T>(Table table, params object[] pkValues) where T : Entity;

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        int InsertOrUpdate<T>(Table table, T entity, params Field[] fields) where T : Entity;

        /// <summary>
        /// 插入或更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fvs"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        int InsertOrUpdate<T>(Table table, FieldValue[] fvs, WhereClip where) where T : Entity;

        #endregion
    }

    interface IDbBatch : IDbProcess
    {
        /// <summary>
        /// 执行批处理操作
        /// </summary>
        /// <param name="errors">输出的错误</param>
        /// <returns></returns>
        int Execute(out IList<DataException> errors);
    }
}
