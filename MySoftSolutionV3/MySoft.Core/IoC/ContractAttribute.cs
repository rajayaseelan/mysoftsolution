using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft.IoC
{
    /// <summary>
    /// Attribute used to mark service interfaces.
    /// </summary>
    public abstract class ContractAttribute : Attribute
    {
        private string name;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        private string description;
        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

        /// <summary>
        /// 实例化ContractAttribute
        /// </summary>
        public ContractAttribute() { }

        /// <summary>
        /// 实例化ContractAttribute
        /// </summary>
        /// <param name="name"></param>
        public ContractAttribute(string name)
        {
            this.Name = name;
        }
    }
}
