using System;
using System.Collections.Generic;
using System.Text;

namespace MySoft.IoC
{
    /// <summary>
    /// The service node info.
    /// </summary>
    [Serializable]
    public class ServiceNodeInfo
    {
        private string key;
		private string service;
		private string impl;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceNodeInfo"/> class.
        /// </summary>
        public ServiceNodeInfo()
		{
		}

        /// <summary>
        /// Gets or sets the DEFAULT_KEY.
        /// </summary>
        /// <value>The DEFAULT_KEY.</value>
		public string Key
        {
			get { return key; }
			set { key = value; }
		}

        /// <summary>
        /// Gets or sets the sevice.
        /// </summary>
        /// <value>The sevice.</value>
		public string Sevice
        {
			get { return service; }
			set { service = value; }
		}

        /// <summary>
        /// Gets or sets the implementation.
        /// </summary>
        /// <value>The implementation.</value>
		public string Implementation
        {
			get { return impl; }
			set { impl = value; }
		}

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
		public override string ToString() 
        {
			return key + "/[" + service + "]" + impl;
		}
    }
}
