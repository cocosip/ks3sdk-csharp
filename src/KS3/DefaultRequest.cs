using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using KS3.Http;

namespace KS3.Model
{
    public class DefaultRequest<T> : IRequest<T>
    {
        /** The resource path being requested */
        private String resourcePath;

        /** Map of the parameters being sent as part of this request */
        private IDictionary<String, String> parameters = new Dictionary<String, String>();

        /** Map of the headers included in this request */
        private IDictionary<String, String> headers = new Dictionary<String, String>();

        /** The service endpoint to which this request should be sent */
        private Uri endpoint;

        /**
         * The original, user facing request object which this internal request
         * object is representing
         */
        private KS3Request originalRequest;

        /** The HTTP method to use when sending this request. */
        private HttpMethod httpMethod;

        /** An optional stream from which to read the request payload. */
        private Stream content;

        /** An optional time offset to account for clock skew */
        private int timeOffset;

        /**
         * Constructs a new DefaultRequest with the specified original, user facing request object.
         */
        public DefaultRequest(KS3Request originalRequest)
        {
            this.originalRequest = originalRequest;
        }

        /**
         * Returns the original, user facing request object which this internal
         */
        public KS3Request GetOriginalRequest()
        {
            return this.originalRequest;
        }

        public void SetHeader(String name, String value)
        {
            this.headers[name] = value;
        }

        public IDictionary<String, String> GetHeaders()
        {
            return this.headers;
        }

        public void SetResourcePath(String resourcePath)
        {
            this.resourcePath = resourcePath;
        }

        public String GetResourcePath()
        {
            return this.resourcePath;
        }

        public void SetParameter(String name, String value)
        {
            this.parameters[name] = value;
        }

        public IDictionary<String, String> GetParameters()
        {
            return this.parameters;
        }

        public HttpMethod GetHttpMethod()
        {
            return this.httpMethod;
        }

        public void SetHttpMethod(HttpMethod httpMethod)
        {
            this.httpMethod = httpMethod;
        }

        public void SetEndpoint(Uri endpoint)
        {
            this.endpoint = endpoint;
        }

        public Uri GetEndpoint()
        {
            return this.endpoint;
        }

        public Stream GetContent()
        {
            return this.content;
        }

        public void SetContent(Stream content)
        {
            this.content = content;
        }

        public void SetHeaders(Dictionary<String, String> headers)
        {
            this.headers = new Dictionary<String, String>(headers);
        }

        public void SetParameters(Dictionary<String, String> parameters)
        {
            this.parameters = new Dictionary<String, String>(parameters);
        }

        public int GetTimeOffset()
        {
            return this.timeOffset;
        }

        public void SetTimeOffset(int timeOffset)
        {
            this.timeOffset = timeOffset;
        }

        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(this.GetHttpMethod().ToString() + " ");
            builder.Append(this.GetEndpoint().ToString() + " ");

            builder.Append("/" + (this.GetResourcePath() != null ? this.GetResourcePath() : "") + " ");

            if (this.GetParameters().Count() != 0)
            {
                builder.Append("Parameters: (");
                foreach (String key in this.GetParameters().Keys)
                {
                    String value = this.GetParameters()[key];
                    builder.Append(key + ": " + value + ", ");
                }
                builder.Append(") ");
            }

            if (this.GetHeaders().Count() != 0)
            {
                builder.Append("Headers: (");
                foreach (String key in this.GetHeaders().Keys)
                {
                    String value = this.GetHeaders()[key];
                    builder.Append(key + ": " + value + ", ");
                }
                builder.Append(") ");
            }

            return builder.ToString();
        }
    }
}
