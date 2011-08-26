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
    /// 验证返回信息
    /// </summary>
    public class ValidateResult
    {
        /// <summary>
        /// 默认的验证器
        /// </summary>
        public static readonly ValidateResult Default = new ValidateResult();

        private IList<string> messages;

        /// <summary>
        /// 实例化ValidateResult
        /// </summary>
        private ValidateResult()
        {
            this.messages = new List<string>();
        }

        /// <summary>
        /// 实例化ValidateResult
        /// </summary>
        /// <param name="messages"></param>
        public ValidateResult(IList<string> messages)
            : this()
        {
            if (messages != null)
                this.messages = messages;
        }

        /// <summary>
        /// 验证是否成功
        /// </summary>
        public bool IsSuccess
        {
            get
            {
                return messages.Count == 0;
            }
        }

        /// <summary>
        /// 消息列表
        /// </summary>
        public IList<string> Messages
        {
            get
            {
                return messages;
            }
            private set
            {
                messages = value;
            }
        }
    }
}
