using System;
using System.Collections.Generic;

namespace MySoft.RESTful.Business
{
    /// <summary>
    /// 业务类别模型
    /// </summary>
    [Serializable]
    public class BusinessKindModel : BusinessStateModel
    {
        /// <summary>
        /// 业务元数据集合,封装了该类别下所有的业务方法和业务对象
        /// </summary>
        public IDictionary<string, BusinessMethodModel> MethodModels { get; set; }

        public BusinessKindModel()
        {
            MethodModels = new Dictionary<string, BusinessMethodModel>();
        }
    }
}
