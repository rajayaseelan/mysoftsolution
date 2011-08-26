using System;

namespace MySoft.Data.Design
{
    /// <summary>
    /// ×Ö¶ÎÓ³ÉäµÄÃû³Æ
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class MappingAttribute : Attribute
    {
        private string name;
        public string Name
        {
            get
            {
                return name;
            }
        }

        public MappingAttribute(string name)
        {
            this.name = name;
        }
    }
}
