using System.Collections.Generic;

namespace MySoft.RESTful.Business.Pool
{
    /// <summary>
    /// 业务池接口
    /// </summary>
    public interface IBusinessPool
    {
        IDictionary<string, BusinessKindModel> KindMethods { get; }
        /// <summary>
        /// 添加业务类别和业务对象,如果业务对象已经添加则不会再次绑定
        /// </summary>
        /// <param name="businessKindName">业务类别</param>
        /// <param name="metadata">业务对象</param>
        /// <returns>返回该业务类别下的业务模型实例</returns>
        void AddKindModel(string businessKindName, BusinessKindModel businessKindModel);
        /// <summary>
        /// 查找业务元数据对象
        /// </summary>
        /// <param name="businessKindName">业务员类型名称</param>
        /// <param name="businessModelName">业务方法名称</param>
        /// <returns></returns>
        BusinessMethodModel FindMethod(string businessKindName, string businessModelName);
        /// <summary>
        /// 获取业务类别绑定的所有业务对象
        /// </summary>
        /// <param name="businessKindName">业务类别</param>
        /// <returns>和当前业务类别绑定的业务类别对象</returns>
        BusinessKindModel GetKindModel(string businessKindName);
        /// <summary>
        /// 移除指定的业务类别绑定的所有对象
        /// </summary>
        /// <param name="businessKindName">业务类别</param>
        /// <returns>返回移除的业务类别对象</returns>
        BusinessKindModel RemoveKindModel(string businessKindName);
        /// <summary>
        /// 移除指定业务类别下的业务对象包括重载的业务方法
        /// </summary>
        /// <param name="businessKindName">业务类别</param>
        /// <param name="businessMethodName">业务方法</param>
        /// <returns>返回该业务类别下的业务模型实例</returns>
        void RemoveMethodModel(string businessKindName, string businessMethodName);
    }
}
