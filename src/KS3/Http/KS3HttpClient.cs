using KS3.Auth;
using KS3.Internal;
using KS3.KS3Exception;
using KS3.Model;
using KS3.Transform;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace KS3.Http
{
    public class KS3HttpClient
    {
        /// <summary>
        /// Client configuration options, such as proxy settings, max retries, etc.
        /// </summary>
        private readonly ClientConfiguration _config;

        private readonly ErrorResponseHandler _errorResponseHandler = new ErrorResponseHandler(new ErrorResponseUnmarshaller());

        public KS3HttpClient(ClientConfiguration clientConfiguration)
        {
            _config = clientConfiguration;
            Init();
        }

        private void Init()
        {
            //Setting for https proctol
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

            //Set max connections
            ServicePointManager.DefaultConnectionLimit = _config.MaxConnections;

            //Set proxy if configured
            var proxyHost = _config.ProxyHost;
            int proxyPort = _config.ProxyPort;
            if (!string.IsNullOrWhiteSpace(proxyHost) && proxyPort > 0)
            {
                WebProxy webProxy = new WebProxy(proxyHost, proxyPort);

                var proxyUsername = _config.ProxyUsername;
                var proxyPassword = _config.ProxyPassword;
                if (!string.IsNullOrWhiteSpace(proxyUsername) && !string.IsNullOrWhiteSpace(proxyPassword))
                {
                    NetworkCredential credential = new NetworkCredential(proxyUsername, proxyPassword);
                    webProxy.Credentials = credential;
                }

                WebRequest.DefaultWebProxy = webProxy;
            }
            else
            {
                WebRequest.DefaultWebProxy = null;
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; // always accept
        }

        public X Excute<X, Y>(IRequest<Y> request, IHttpResponseHandler<X> responseHandler, KS3Signer<Y> ks3Signer) where Y : KS3Request
        {
            SetUserAgent(request);
            HttpWebResponse httpResponse = null;
            X result = default;
            for (int i = 0; i < Constants.RETRY_TIMES; i++)
            {
                try
                {
                    // Sign the request if a signer was provided
                    if (ks3Signer != null && request.GetOriginalRequest().Credentials != null)
                    {
                        ks3Signer.Sign(request, request.GetOriginalRequest().Credentials);
                    }
                    request.SetResourcePath(request.GetResourcePath().Replace("%5C", "/").Replace("//", "/%2F"));
                    if (request.GetResourcePath().EndsWith("%2F"))
                    {
                        request.SetResourcePath(request.GetResourcePath().Substring(0, request.GetResourcePath().Length - 3));
                    }
                    HttpWebRequest httpRequest = HttpRequestFactory.CreateHttpRequest(request, this._config);
                    httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                    result = responseHandler.Handle(httpResponse);
                    break;
                }
                catch (WebException we)
                {
                    HttpWebResponse errorResponse = (HttpWebResponse)we.Response;
                    ServiceException serviceException = null;
                    try
                    {
                        serviceException = _errorResponseHandler.Handle(errorResponse);
                    }
                    catch
                    {
                        throw we;
                    }
                    throw serviceException;
                }
                catch (IOException ex)
                {
                    if (i == Constants.RETRY_TIMES - 1)
                    {
                        throw ex;
                    }
                    Thread.Sleep(100);
                }
                finally
                {
                    httpResponse?.Close();
                }
            }
            return result;

        }

        /// <summary>
        /// Sets a User-Agent for the specified request, taking into account any custom data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        private void SetUserAgent<T>(IRequest<T> request) where T : KS3Request
        {
            if (!string.IsNullOrWhiteSpace(_config.UserAgent))
            {
                request.SetHeader(Headers.USER_AGENT, _config.UserAgent);
            }
        }
    }
}
