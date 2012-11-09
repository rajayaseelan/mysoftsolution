using System;

namespace MySoft.Web.Configuration
{
    /// <summary>
    /// Represents a rewriter rule.  A rewriter rule is composed of a pattern to search for and a string to replace
    /// the pattern with (if matched).
    /// </summary>
    [Serializable]
    public class RedirectPageRule : StaticPageRule
    {
        // private member variables...
        private string errorTo, extension;
        private bool genHtml;

        #region Public Properties

        /// <summary>
        /// The string to replace the pattern with, if found.
        /// </summary>
        /// <remarks>The replacement string may use grouping symbols, like $1, $2, etc.  Specifically, the
        /// <b>System.Text.RegularExpression.Regex</b> class's <b>Replace()</b> method is used to replace
        /// the match in <see cref="LookFor"/> with the value in <b>ErrorTo</b>.</remarks>
        public string ErrorTo
        {
            get
            {
                return errorTo;
            }
            set
            {
                errorTo = value;
            }
        }

        /// <summary>
        /// The string to replace the pattern with, if found.
        /// </summary>
        /// <remarks>The replacement string may use grouping symbols, like $1, $2, etc.  Specifically, the
        /// <b>System.Text.RegularExpression.Regex</b> class's <b>Replace()</b> method is used to replace
        /// the match in <see cref="LookFor"/> with the value in <b>IsStatic</b>.</remarks>
        public bool GenHtml
        {
            get
            {
                return genHtml;
            }
            set
            {
                genHtml = value;
            }
        }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string Extension
        {
            get
            {
                if (string.IsNullOrEmpty(extension))
                    return ".htm";
                else
                    return extension;
            }
            set
            {
                extension = value;
            }
        }

        #endregion
    }
}
