using System;
using System.Collections.Generic;

namespace MySoft.Data
{
    /// <summary>
    /// 实体验证类
    /// </summary>
    public class Validator<T>
        where T : Entity
    {
        private T entity;
        private List<Field> vlist;

        /// <summary>
        /// 实例化验证类
        /// </summary>
        /// <param name="entity"></param>
        public Validator(T entity)
        {
            this.entity = entity;
            this.messages = new List<string>();

            //获取需要处理的字段列表
            if (entity.As<IEntityBase>().GetObjectState() == EntityState.Insert)
                this.vlist = entity.GetFieldValues().FindAll(fv => !fv.IsChanged)
                    .ConvertAll<Field>(fv => fv.Field);
            else
                this.vlist = entity.GetFieldValues().FindAll(fv => fv.IsChanged)
                    .ConvertAll<Field>(fv => fv.Field);
        }

        private IList<string> messages;

        /// <summary>
        /// 验证的结果
        /// </summary>
        public ValidateResult Result
        {
            get
            {
                return new ValidateResult(messages);
            }
        }

        /// <summary>
        /// 验证实体属性的有效性并返回错误列表(验证所有的列)
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Validator<T> Check(Predicate<T> predicate, string message)
        {
            if (predicate(this.entity))
            {
                this.messages.Add(message);
            }
            return this;
        }

        /// <summary>
        /// 验证实体属性的有效性并返回错误列表(只验证需要插入或更新的列)
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="field"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Validator<T> Check(Predicate<T> predicate, Field field, string message)
        {
            if (this.vlist.Exists(p => p.Name == field.Name))
            {
                if (predicate(this.entity))
                {
                    this.messages.Add(message);
                }
            }
            return this;
        }
    }
}
