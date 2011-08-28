using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using MySoft.Net.Sockets;

namespace MySoft.IoC.Message
{
    /// <summary>
    /// The response msg.
    /// </summary>
    [Serializable]
    [BufferType(10000)]
    public class ResponseMessage : RequestBase
    {
        private ResponseData data;

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public ResponseData Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                exception = null;
            }
        }

        /// <summary>
        /// ¼ÇÂ¼Êý
        /// </summary>
        public int RowCount
        {
            get
            {
                if (data == null)
                    return 0;
                else
                    return data.Count;
            }
        }

        private Exception exception;

        /// <summary>
        /// Gets or sets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception
        {
            get
            {
                return exception;
            }
            set
            {
                exception = value;
                data = null;
            }
        }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public override string Message
        {
            get
            {
                if (exception != null)
                {
                    return string.Format("Error: {0} (Type:{1} Compress:{2} Encrypt:{3}).", ErrorHelper.GetInnerException(exception).Message, base.ReturnType, base.Compress, base.Encrypt);
                }
                else
                {
                    return string.Format("RowCount: {0} (Type:{1} Compress:{2} Encrypt:{3}).", (data == null ? 0 : data.Count), base.ReturnType, base.Compress, base.Encrypt);
                }
            }
        }
    }
}