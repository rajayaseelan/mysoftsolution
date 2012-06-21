using System;

namespace MySoft.Mail
{
    /// <summary>
    /// 邮件发送结果
    /// </summary>
    [Serializable]
    public class SendResult : ResponseResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        public SendResult()
            : base()
        {
            this.Success = true;
            this.Message = "邮件处理成功！";
        }
    }
}
