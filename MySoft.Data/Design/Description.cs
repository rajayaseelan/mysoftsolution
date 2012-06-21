using System;

namespace MySoft.Data.Design
{
    /// <summary>
    /// ◊÷∂Œ√Ë ˆ–≈œ¢
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class DescriptionAttribute : Attribute
    {
        private string description;
        public string Description
        {
            get
            {
                return description;
            }
        }

        public DescriptionAttribute(string description)
        {
            this.description = description;
        }
    }
}
