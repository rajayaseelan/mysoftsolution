using System;
using System.Collections.Generic;
using System.Linq;
using SM = System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.IO;

namespace MySoft.IoC.HttpProxy
{
    // Start the service and browse to http://<machine_name>:<port>/Service1/help to view the service's generated help page
    // NOTE: By default, a new instance of the service is created for each call; change the InstanceContextMode to Single if you want
    // a single instance of the service to process all calls.	
    [SM.ServiceContract]
    public interface IHttpProxyService
    {
        // TODO: Implement the collection resource that will contain the SampleItem instances

        /// <summary>
        /// GET入口
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <returns>字节数据流</returns>
        [SM.OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "{name}")]
        Stream GetTextEntry(string name);

        /// <summary>
        /// POST入口
        /// </summary>
        /// <param name="name">方法名称</param>
        /// <returns>字节数据流</returns>
        [SM.OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "{name}")]
        Stream PostTextEntry(string name, Stream parameters);

        /// <summary>
        /// GET入口
        /// </summary>
        /// <returns>字节数据流</returns>
        [SM.OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "help")]
        Stream GetHttpDocument();

        /// <summary>
        /// GET入口
        /// </summary>
        /// <param name="kind"></param>
        /// <returns>字节数据流</returns>
        [SM.OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "help/{kind}")]
        Stream GetHttpDocumentFromKind(string kind);

        /// <summary>
        /// GET入口
        /// </summary>
        /// <returns>字节数据流</returns>
        [SM.OperationContract]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "tcp")]
        Stream GetTcpDocument();
    }
}
