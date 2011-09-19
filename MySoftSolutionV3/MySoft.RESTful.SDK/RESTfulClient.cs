using System;
using System.Globalization;
using OAuth.Net.Common;
using System.IO;
using System.Text;
using OAuth.Net.Components;
using System.Net;

namespace MySoft.RESTful.SDK
{
    /// <summary>
    /// RESTful客户端
    /// </summary>
    public class RESTfulClient
    {
        private string url;
        private DataFormat format;

        /// <summary>
        /// RESTfulClient实例化
        /// </summary>
        public RESTfulClient(string url)
        {
            this.url = url;
            this.format = DataFormat.JSON;
        }

        /// <summary>
        /// RESTfulClient实例化
        /// </summary>
        /// <param name="url"></param>
        /// <param name="format"></param>
        public RESTfulClient(string url, DataFormat format)
        {
            this.url = url;
            this.format = format;
        }

        /// <summary>
        /// 认证Token
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public Token AuthorizeToken(TokenParameter parameter)
        {
            try
            {
                Token token = new Token();
                var requestUri = BuildRequestTokenUri(parameter.AuthorizeUrl, parameter.SignatureMethod, parameter.ConsumerKey, parameter.ConsumerSecret);
                var value = Request(requestUri, parameter.Encoding, null);
                token.AddParameter(value);

                var oauth_token = token.Find("oauth_token");
                requestUri = new Uri(string.Format(parameter.SignatureUrl + "?{0}={1}&username={2}&password={3}", oauth_token.Name, oauth_token.Value, parameter.UserName, parameter.Password));
                value = Request(requestUri, parameter.Encoding, null);
                token.AddParameter(value);

                var oauth_verifier = token.Find("oauth_verifier");
                var oauth_token_secret = token.Find("oauth_token_secret");
                requestUri = BuildAccessTokenUri(parameter.AccessTokenUrl, parameter.SignatureMethod, oauth_token.Value.ToString(), oauth_verifier.Value.ToString(), parameter.ConsumerKey, parameter.ConsumerSecret, oauth_token_secret.Value.ToString());
                value = Request(requestUri, parameter.Encoding, null);
                token.AddParameter(value);

                return token;
            }
            catch (WebException ex)
            {
                throw new RESTfulException(ex.Message, ex) { Code = (int)(ex.Response as HttpWebResponse).StatusCode };
            }
            catch (Exception ex)
            {
                throw new RESTfulException(ex.Message, ex) { Code = 404 };
            }
        }

        private string Request(Uri uri, Encoding enc, string input)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.KeepAlive = false;
            request.Timeout = 60 * 1000;
            if (!string.IsNullOrEmpty(input))
            {
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";

                var stream = request.GetRequestStream();
                var buffer = enc.GetBytes(input);
                stream.Write(buffer, 0, buffer.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                StreamReader sr = new StreamReader(response.GetResponseStream(), enc);
                string value = sr.ReadToEnd();

                return value;
            }
            else
            {
                throw new RESTfulException(response.StatusDescription) { Code = (int)response.StatusCode };
            }
        }

        private Uri BuildRequestTokenUri(string url, string signatureMethod, string consumerKey, string consumerSecret)
        {
            int timestamp = UnixTime.ToUnixTime(DateTime.Now);
            OAuthParameters parameters = new OAuthParameters();
            parameters.ConsumerKey = consumerKey;
            parameters.Nonce = new GuidNonceProvider().GenerateNonce(timestamp);
            parameters.SignatureMethod = signatureMethod;
            parameters.Timestamp = timestamp.ToString(CultureInfo.InvariantCulture);
            parameters.Callback = "oob";
            parameters.Signature = new HmacSha1SigningProvider().ComputeSignature(SignatureBase.Create("GET", new Uri(url), parameters), consumerSecret, null);
            UriBuilder builder2 = new UriBuilder(url);
            builder2.Query = parameters.ToQueryStringFormat();
            UriBuilder builder = builder2;
            return builder.Uri;
        }

        private Uri BuildAccessTokenUri(string url, string signatureMethod, string auth_token, string auth_verifier, string consumerKey, string consumerSecret, string tokenSecret)
        {
            Uri AccessTokenBaseUri = new Uri(url);
            int timestamp = UnixTime.ToUnixTime(DateTime.Now);
            OAuthParameters parameters = new OAuthParameters();
            parameters.ConsumerKey = consumerKey;
            parameters.Nonce = new GuidNonceProvider().GenerateNonce(timestamp);
            parameters.SignatureMethod = signatureMethod;
            parameters.Timestamp = timestamp.ToString(CultureInfo.InvariantCulture);
            parameters.Token = auth_token;
            parameters.Verifier = auth_verifier;
            parameters.Signature = (new HmacSha1SigningProvider()).ComputeSignature(SignatureBase.Create("GET", AccessTokenBaseUri, parameters), consumerSecret, tokenSecret);
            UriBuilder builder = new UriBuilder(AccessTokenBaseUri);
            builder.Query = parameters.ToQueryStringFormat();
            return builder.Uri;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <returns></returns>
        public IServiceInterfaceType GetService<IServiceInterfaceType>()
        {
            return GetService<IServiceInterfaceType>(new Token());
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="IServiceInterfaceType"></typeparam>
        /// <returns></returns>
        public IServiceInterfaceType GetService<IServiceInterfaceType>(Token token)
        {
            Exception ex = new ArgumentException("Generic parameter type - 【" + typeof(IServiceInterfaceType).FullName
                + "】 must be an interface marked with PublishKindAttribute.");


            PublishKindAttribute kindattr = null;
            if (!typeof(IServiceInterfaceType).IsInterface)
            {
                throw ex;
            }
            else
            {
                bool markedWithServiceContract = false;
                var attr = CoreHelper.GetTypeAttribute<PublishKindAttribute>(typeof(IServiceInterfaceType));
                if (attr != null)
                {
                    markedWithServiceContract = true;
                }

                kindattr = attr;
                attr = null;

                if (!markedWithServiceContract)
                {
                    throw ex;
                }
            }

            var serviceType = typeof(IServiceInterfaceType);
            var handler = new RESTfulInvocationHandler(url, kindattr, token, format);
            var dynamicProxy = ProxyFactory.GetInstance().Create(handler, serviceType, true);

            return (IServiceInterfaceType)dynamicProxy;
        }
    }
}
