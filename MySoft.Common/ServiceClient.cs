using System;
using System.Collections;
using System.Net;

namespace MySoft.Common
{
    /// <summary>
    /// ServiceMethod : 客户端可以调用服务的类。
    /// </summary>
    public abstract class ServiceClient
    {
        private static readonly PostService cs = new PostService();

        /// <summary>
        /// 发送 byte[] 数据，可选择加密或明文发送
        /// </summary>
        /// <param name="PostUri"></param>
        /// <param name="Content">待加密的内容</param>
        /// <param name="IsEncryptoSend">是否加密发送</param>
        /// <param name="enableKeyIV">是否允许DES的加密算法Key,IV使用一致的</param>
        /// <returns>WebResponse</returns>
        public static WebResponse Send(string PostUri, string Content, bool IsEncryptoSend, bool enableKeyIV)
        {
            if (IsEncryptoSend)
            {
                string publicKey = FileHelper.ReadFile(FunctionHelper.GetRealFile(FunctionHelper.GetAppSettings("publicKey")));

                string desKey = StringHelper.RandomSTR(8);
                string desIV = desKey;

                string rsaDes = "";

                string encryptContent = cs.EncryptString(Content, publicKey, desKey, desIV, out rsaDes);

                Hashtable ht = new Hashtable();
                ht.Add("CS_DESSTRING", rsaDes);

                return cs.Send(PostUri, encryptContent, ht);
            }
            else
            {
                return cs.Send(PostUri, Content);
            }
        }


        /// <summary>
        /// 发送 byte[] 数据，可选择加密或明文发送
        /// </summary>
        /// <param name="PostUri"></param>
        /// <param name="pui">待加密的内容</param>
        /// <param name="IsEncryptoSend">是否加密发送</param>
        /// <param name="enableKeyIV">是否允许DES的加密算法Key,IV使用一致的</param>
        /// <returns>WebResponse</returns>
        public static WebResponse Send(string PostUri, PostUserInfo pui, bool IsEncryptoSend, bool enableKeyIV)
        {
            byte[] bufferContent = SerializationManager.SerializeBin(pui);
            string str = Convert.ToBase64String(bufferContent);

            if (IsEncryptoSend)
            {
                string publicKey = FileHelper.ReadFile(FunctionHelper.GetRealFile(FunctionHelper.GetAppSettings("publicKey")));

                string desKey = StringHelper.RandomSTR(8);
                string desIV = desKey;

                string rsaDes = "";


                string encryptContent = cs.EncryptString(bufferContent, publicKey, desKey, desIV, out rsaDes);

                Hashtable ht = new Hashtable();
                ht.Add("CS_DESSTRING", rsaDes);

                return cs.Send(PostUri, encryptContent, ht);
            }
            else
            {
                return cs.Send(PostUri, str);
            }
        }


        /// <summary>
        /// 发送方获取接收方返回的信息
        /// </summary>
        /// <param name="res">返回给发送方的 Response 对象</param>
        /// <param name="CS_RESULT">返回操作结果</param>
        /// <returns>string</returns>
        public static string GetResponseContent(WebResponse res, out string CS_RESULT)
        {
            CS_RESULT = FunctionHelper.CheckValiable(res.Headers["CS_RESULT"]) ? res.Headers["CS_RESULT"] : "";

            return cs.GetResponseStream(res);
        }

        /// <summary> 
        /// 根据指定的信息，调用远程WebService方法 
        /// </summary> 
        /// <param name="url">WebService的http形式的地址</param> 
        /// <param name="namespace">欲调用的WebService的命名空间</param> 
        /// <param name="classname">欲调用的WebService的类名（不包括命名空间前缀）</param> 
        /// <param name="methodname">欲调用的WebService的方法名</param> 
        /// <param name="args">参数列表</param> 
        /// <returns>WebService的执行结果</returns> 
        /// <remarks> 
        /// 如果调用失败，将会抛出Exception。请调用的时候，适当截获异常。 
        /// 异常信息可能会发生在两个地方： 
        /// 1、动态构造WebService的时候，CompileAssembly失败。 
        /// 2、WebService本身执行失败。 
        /// </remarks> 
        /// <example> 
        /// <code> 
        /// object obj = InvokeWebservice("http://localhost/GSP_WorkflowWebservice/common.asmx","Genersoft.Platform.Service.Workflow","Common","GetToolType",new object[]{"1"}); 
        /// </code> 
        /// </example> 
        public static object InvokeWebservice(string url, string @namespace, string classname, string methodname, params object[] args)
        {
            try
            {
                System.Net.WebClient wc = new System.Net.WebClient();
                System.IO.Stream stream = wc.OpenRead(url + "?WSDL");
                System.Web.Services.Description.ServiceDescription sd = System.Web.Services.Description.ServiceDescription.Read(stream);
                System.Web.Services.Description.ServiceDescriptionImporter sdi = new System.Web.Services.Description.ServiceDescriptionImporter();
                sdi.AddServiceDescription(sd, "", "");
                System.CodeDom.CodeNamespace cn = new System.CodeDom.CodeNamespace(@namespace);
                System.CodeDom.CodeCompileUnit ccu = new System.CodeDom.CodeCompileUnit();
                ccu.Namespaces.Add(cn);
                sdi.Import(cn, ccu);

                Microsoft.CSharp.CSharpCodeProvider icc = new Microsoft.CSharp.CSharpCodeProvider();
                System.CodeDom.Compiler.CompilerParameters cplist = new System.CodeDom.Compiler.CompilerParameters();
                cplist.GenerateExecutable = false;
                cplist.GenerateInMemory = true;
                cplist.ReferencedAssemblies.Add("System.dll");
                cplist.ReferencedAssemblies.Add("System.XML.dll");
                cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
                cplist.ReferencedAssemblies.Add("System.Data.dll");

                System.CodeDom.Compiler.CompilerResults cr = icc.CompileAssemblyFromDom(cplist, ccu);
                if (true == cr.Errors.HasErrors)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
                    {
                        sb.Append(ce.ToString());
                        sb.Append(System.Environment.NewLine);
                    }
                    throw new Exception(sb.ToString());
                }
                System.Reflection.Assembly assembly = cr.CompiledAssembly;
                Type t = assembly.GetType(@namespace + "." + classname, true, true);
                object obj = Activator.CreateInstance(t);
                System.Reflection.MethodInfo mi = t.GetMethod(methodname);
                return mi.Invoke(obj, args);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException.Message, new Exception(ex.InnerException.StackTrace));
            }
        }
    }
}
