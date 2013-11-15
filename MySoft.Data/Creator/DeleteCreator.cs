using System;
using System.Collections.Generic;

namespace MySoft.Data
{
    /// <summary>
    /// 删除创建器
    /// </summary>
    [Serializable]
    public class DeleteCreator<T> : DeleteCreator, IDeleteCreator<T>
        where T : Entity
    {
        private DeleteCreator(Table table) : base(table) { }

        /// <summary>
        /// 创建一个新的更新器
        /// </summary>
        public static DeleteCreator<T> NewCreator()
        {
            return new DeleteCreator<T>(Table.GetTable<T>());
        }
    }

    /// <summary>
    /// 删除创建器
    /// </summary>
    [Serializable]
    public class DeleteCreator : WhereCreator<DeleteCreator>, IDeleteCreator
    {
        /// <summary>
        /// 创建一个新的删除器
        /// </summary>
        public static DeleteCreator NewCreator(string tableName)
        {
            return new DeleteCreator(tableName);
        }

        /// <summary>
        /// 创建一个新的删除器
        /// </summary>
        public static DeleteCreator NewCreator(Table table)
        {
            return new DeleteCreator(table);
        }

        /// <summary>
        /// 实例化DeleteCreator
        /// </summary>
        /// <param name="tableName"></param>
        private DeleteCreator(string tableName)
            : base(tableName, null)
        {
        }

        /// <summary>
        /// 实例化DeleteCreator
        /// </summary>
        /// <param name="table"></param>
        protected DeleteCreator(Table table)
            : base(table)
        {
        }
    }
}
