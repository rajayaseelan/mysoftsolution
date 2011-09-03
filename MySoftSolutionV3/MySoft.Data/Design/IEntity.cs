using System.Collections.Generic;

namespace MySoft.Data
{
    /// <summary>
    /// 实体相关信息
    /// </summary>
    public interface IEntityInfo
    {
        /// <summary>
        /// 表信息
        /// </summary>
        Table Table { get; }

        /// <summary>
        /// 字段信息
        /// </summary>
        Field[] Fields { get; }

        /// <summary>
        /// 字段及值信息
        /// </summary>
        FieldValue[] FieldValues { get; }

        /// <summary>
        /// 更新字段
        /// </summary>
        Field[] UpdateFields { get; }

        /// <summary>
        /// 更新字段及值信息
        /// </summary>
        FieldValue[] UpdateFieldValues { get; }

        /// <summary>
        /// 是否修改
        /// </summary>
        bool IsUpdate { get; }

        /// <summary>
        /// 是否只读 (只读时为视图或自定义实例)
        /// </summary>
        bool IsReadOnly { get; }
    }

    /// <summary>
    /// 实体基类接口
    /// </summary>
    public interface IEntityBase
    {
        /// <summary>
        /// 转换成另一对象
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        TResult As<TResult>();

        /// <summary>
        /// 返回一个行阅读对象
        /// </summary>
        IRowReader ToRowReader();

        /// <summary>
        /// 返回字典对象
        /// </summary>
        /// <returns></returns>
        IDictionary<string, object> ToDictionary();

        /// <summary>
        /// 克隆一个对象
        /// </summary>
        /// <returns></returns>
        EntityBase CloneObject();

        /// <summary>
        /// 获取对象状态
        /// </summary>
        EntityState GetObjectState();

        /// <summary>
        /// 使用propertyName获取值信息
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        object GetValue(string propertyName);

        /// <summary>
        /// 使用propertyName获设置信息
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        void SetValue(string propertyName, object value);

        /// <summary>
        /// 使用field获取值信息
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        object GetValue(Field field);

        /// <summary>
        /// 使用field获设置信息
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        void SetValue(Field field, object value);

        /// <summary>
        /// 通过属性获取字段
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        Field GetField(string propertyName);
    }
}

namespace MySoft.Data.Design
{
    /// <summary>
    /// 实体接口
    /// </summary>
    public interface IEntity
    {
        #region 状态操作

        /// <summary>
        /// 置为修改状态并移除字段
        /// </summary>
        /// <param name="removeFields"></param>
        void Attach(params Field[] removeFields);

        /// <summary>
        /// 置为修改状态并设置字段
        /// </summary>
        /// <param name="setFields"></param>
        void AttachSet(params Field[] setFields);

        /// <summary>
        /// 置为修改状态并移除字段
        /// </summary>
        /// <param name="removeFields"></param>
        void AttachAll(params Field[] removeFields);

        /// <summary>
        /// 置为插入状态并移除字段
        /// </summary>
        /// <param name="removeFields"></param>
        void Detach(params Field[] removeFields);

        /// <summary>
        /// 置为插入状态并设置字段
        /// </summary>
        /// <param name="setFields"></param>
        void DetachSet(params Field[] setFields);

        /// <summary>
        /// 置为插入状态并移除字段
        /// </summary>
        /// <param name="removeFields"></param>
        void DetachAll(params Field[] removeFields);

        #endregion
    }
}
