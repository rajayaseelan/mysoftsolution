﻿using System;
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
        private string description;
        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}