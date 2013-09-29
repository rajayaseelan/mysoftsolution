using System;
using System.Collections.Generic;

namespace MySoft.Data
{
    /// <summary>
    /// 实体验证接口
    /// </summary>
    interface IValidator
    {
        /// <summary>
        /// 根据实体状态来验证实体的有效性，返回一组错误信息
        /// </summary>
        /// <returns></returns>
        ValidateResult Validation();
    }

    /// <summary>
    /// 无效的对象
    /// </summary>
    [Serializable]
    public class InvalidValue
    {
        /// <summary>
        /// 字段
        /// </summary>
        public Field Field { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 验证返回信息
    /// </summary>
    public class ValidateResult
    {
        /// <summary>
        /// 默认的验证器
        /// </summary>
        public static readonly ValidateResult Default = new ValidateResult();

        private IList<InvalidValue> values;

        /// <summary>
        /// 实例化ValidateResult
        /// </summary>
        private ValidateResult()
        {
            this.values = new List<InvalidValue>();
        }

        /// <summary>
        /// 实例化ValidateResult
        /// </summary>
        /// <param name="values"></param>
        public ValidateResult(IList<InvalidValue> values)
            : this()
        {
            if (values != null)
                this.values = values;
        }

        /// <summary>
        /// 验证是否成功
        /// </summary>
        public bool IsSuccess
        {
            get
            {
                return values.Count == 0;
            }
        }

        /// <summary>
        /// 消息列表
        /// </summary>
        public IList<InvalidValue> Values
        {
            get
            {
                return values;
            }
            private set
            {
                values = value;
            }
        }
    }
}
