using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySoft.Net.HTTP;

namespace MySoft.IoC.Http
{
    /// <summary>
    /// Http认证异常
    /// </summary>
    public class HTTPAuthMessageException : HTTPMessageException
    {
        public HTTPAuthMessageException(string message)
            : base(message)
        { }
    }
}
