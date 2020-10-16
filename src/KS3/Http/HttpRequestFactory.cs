using KS3.Internal;
using KS3.Model;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace KS3.Http
{
    public static class HttpRequestFactory
    {
        /// <summary>
        /// Creates an HttpWebRequest based on the specified request and populates any parameters, headers, etc. from the original request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="clientConfiguration"></param>
        /// <returns></returns>
        public static HttpWebRequest CreateHttpRequest<T>(IRequest<T> request, ClientConfiguration clientConfiguration) where T : KS3Request
        {
            Uri endpoint = request.GetEndpoint();
            string uri = endpoint.ToString();

            if (!string.IsNullOrWhiteSpace(request.GetResourcePath()))
            {
                if (request.GetResourcePath().StartsWith("/"))
                {
                    uri = uri.TrimEnd('/');
                }
                else if (!uri.EndsWith("/"))
                {
                    uri += "/";
                }
                uri += request.GetResourcePath();
            }
            else if (!uri.EndsWith("/"))
            {
                uri += "/";
            }
            //if (request.GetResourcePath() != null && request.GetResourcePath().Length > 0)
            //{
            //    if (request.GetResourcePath().StartsWith("/"))
            //    {
            //        if (uri.EndsWith("/")) uri = uri.Substring(0, uri.Length - 1);
            //    }
            //    else if (!uri.EndsWith("/")) uri += "/";
            //    uri += request.GetResourcePath();
            //}
            //else if (!uri.EndsWith("/")) uri += "/";

            string encodedParams = EncodeParameters(request);

            /*
             * For all non-POST requests, and any POST requests that already have a
             * payload, we put the encoded params directly in the URI, otherwise,
             * we'll put them in the POST request's payload.
             */
            bool putParamsInUri = request.GetHttpMethod() != HttpMethod.POST || request.GetContent() != null;

            if (encodedParams != null && (putParamsInUri || encodedParams.Contains("upload")))
            {
                uri += "?" + encodedParams;
            }
            if (request.GetHttpMethod() == HttpMethod.POST && encodedParams != null && !putParamsInUri && !encodedParams.Contains("upload"))
            {
                request.SetContent(new MemoryStream(Constants.DEFAULT_ENCODING.GetBytes(encodedParams)));
            }

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpRequest.Method = request.GetHttpMethod().ToString();

            httpRequest.AllowWriteStreamBuffering = false; // important

            httpRequest.Timeout = clientConfiguration.Timeout;
            httpRequest.ReadWriteTimeout = clientConfiguration.ReadWriteTimeout;

            ConfigureHeaders(httpRequest, request, clientConfiguration);

            if (request.GetContent() != null)
            {
                Stream inputStream = request.GetContent();
                if (inputStream.CanSeek)
                {
                    inputStream.Seek(0, SeekOrigin.Begin);
                }
                Stream requestStream = httpRequest.GetRequestStream();
                int SIZE = Constants.DEFAULT_STREAM_BUFFER_SIZE;
                byte[] buf = new byte[SIZE];

                for (; ; )
                {
                    int size = inputStream.Read(buf, 0, Constants.DEFAULT_STREAM_BUFFER_SIZE);
                    if (size <= 0) break;
                    requestStream.Write(buf, 0, size);
                }

                requestStream.Flush();
                requestStream.Close();
            }

            return httpRequest;
        }

        /// <summary>
        /// Creates an encoded query string from all the parameters in the specified request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string EncodeParameters<T>(IRequest<T> request) where T : KS3Request
        {
            if (request.GetParameters().Count == 0)
                return null;

            StringBuilder builder = new StringBuilder();
            bool first = true;
            char separator = '&';

            foreach (string name in request.GetParameters().Keys)
            {
                string value = request.GetParameters()[name];
                if (!first) builder.Append(separator);
                else first = false;
                builder.Append(name + (value != null ? ("=" + value) : ""));
            }

            return builder.ToString();
        }

        /// <summary>
        ///  Configures the headers in the specified HTTP request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpRequest"></param>
        /// <param name="request"></param>
        /// <param name="clientConfiguration"></param>
        private static void ConfigureHeaders<T>(HttpWebRequest httpRequest, IRequest<T> request, ClientConfiguration clientConfiguration) where T : KS3Request
        {
            // Copy over any other headers already in our request
            foreach (string name in request.GetHeaders().Keys)
            {
                if (name.Equals(Headers.HOST, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                string value = request.GetHeaders()[name];

                if (name.Equals(Headers.CONTENT_TYPE))
                {
                    httpRequest.ContentType = value;
                }
                else if (name.Equals(Headers.CONTENT_LENGTH))
                {
                    httpRequest.ContentLength = long.Parse(value);
                }
                else if (name.Equals(Headers.USER_AGENT))
                {
                    httpRequest.UserAgent = value;
                }
                else if (name.Equals(Headers.DATE))
                {
                    httpRequest.Date = DateTime.Parse(value);
                }
                else if (name.Equals(Headers.RANGE))
                {
                    string[] range = value.Split('-');
                    httpRequest.AddRange(long.Parse(range[0]), long.Parse(range[1]));
                }
                else if (name.Equals(Headers.GET_OBJECT_IF_MODIFIED_SINCE))
                {
                    httpRequest.IfModifiedSince = DateTime.Parse(value);
                }
                else
                {
                    httpRequest.Headers[name] = value;
                }
            }

            /* Set content type and encoding */
            if (!httpRequest.Headers.AllKeys.Contains(Headers.CONTENT_TYPE) || httpRequest.Headers[Headers.CONTENT_TYPE].Length == 0)
                httpRequest.ContentType = Mimetypes.DEFAULT_MIMETYPE;
        }

    }
}
