using KS3.Http;
using KS3.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace KS3
{
    public interface IRequest<T>
    {
        /// <summary>
        /// Sets the specified header to this request.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetHeader(string name, string value);

        /// <summary>
        /// Returns a map of all the headers included in this request.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string> GetHeaders();

        /// <summary>
        /// Sets all headers, clearing any existing ones.
        /// </summary>
        /// <param name="headers"></param>
        void SetHeaders(Dictionary<string, string> headers);

        /// <summary>
        /// Sets the path to the resource being requested.
        /// </summary>
        /// <param name="path"></param>
        void SetResourcePath(string path);

        /// <summary>
        /// Returns the path to the resource being requested.
        /// </summary>
        /// <returns></returns>
        string GetResourcePath();

        /// <summary>
        /// Sets the specified request parameter to this request.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        void SetParameter(string name, string value);

        /// <summary>
        /// Returns a map of all parameters in this request.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string> GetParameters();

        /// <summary>
        /// Sets all parameters, clearing any existing values.
        /// </summary>
        /// <param name="parameters"></param>
        void SetParameters(Dictionary<string, string> parameters);

        /// <summary>
        /// Returns the service endpoint to which this request should be sent.
        /// </summary>
        /// <returns></returns>
        Uri GetEndpoint();

        /// <summary>
        /// Sets the service endpoint to which this request should be sent.
        /// </summary>
        /// <param name="endpoint"></param>
        void SetEndpoint(Uri endpoint);

        /// <summary>
        /// Returns the HTTP method (GET, POST, etc) to use when sending this request.
        /// </summary>
        /// <returns></returns>
        HttpMethod GetHttpMethod();

        /// <summary>
        /// Sets the HTTP method (GET, POST, etc) to use when sending this request.
        /// </summary>
        /// <param name="httMethod"></param>
        void SetHttpMethod(HttpMethod httMethod);

        /// <summary>
        /// Returns the optional stream containing the payload data to include for this request.  Not all requests will contain payload data.
        /// </summary>
        /// <returns></returns>
        Stream GetContent();

        /// <summary>
        /// Sets the optional stream containing the payload data to include for this request. Not all requests will contain payload data.
        /// </summary>
        /// <param name="content"></param>
        void SetContent(Stream content);

        /// <summary>
        /// Returns the original, user facing request object which this internal request object is representing.
        /// </summary>
        /// <returns></returns>
        KS3Request GetOriginalRequest();

        /// <summary>
        /// Returns the optional value for time offset for this request. 
        /// This will be used by the signer to adjust for potential clock skew. Value is in seconds, positive values imply the current clock is "fast", negative values imply clock is slow.
        /// </summary>
        /// <returns></returns>
        int GetTimeOffset();

        /// <summary>
        /// Sets the optional value for time offset for this request.
        /// This will be used by the signer to adjust for potential clock skew. Value is in seconds, positive values imply the current clock is "fast", negative values imply clock is slow.
        /// </summary>
        /// <param name="timeOffset"></param>
        void SetTimeOffset(int timeOffset);
    }
}
