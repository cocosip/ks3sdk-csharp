using KS3.Http;
using KS3.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace KS3
{
    public interface IRequest<T>
    {
        /**
         * Sets the specified header to this request.
         */
        void SetHeader(string name, string value);
        
        /**
         * Returns a map of all the headers included in this request.
         */
        IDictionary<string, string> GetHeaders();

        /**
         * Sets all headers, clearing any existing ones.
         */
        void SetHeaders(Dictionary<string, string> headers);

        /**
         * Sets the path to the resource being requested.
         */
        void SetResourcePath(string path);

        /**
         * Returns the path to the resource being requested.
         */
        string GetResourcePath();

        /**
         * Sets the specified request parameter to this request.
         */
        void SetParameter(string name, string value);

        /**
         * Returns a map of all parameters in this request.
         */
        IDictionary<string, string> GetParameters();

        /**
         * Sets all parameters, clearing any existing values.
         */
        void SetParameters(Dictionary<string, string> parameters);

        /**
         * Returns the service endpoint to which this request should be sent.
         */
        Uri GetEndpoint();

        /**
         * Sets the service endpoint to which this request should be sent.
         */
        void SetEndpoint(Uri endpoint);

        /**
	     * Returns the HTTP method (GET, POST, etc) to use when sending this
	     * request.
	     */ 
        HttpMethod GetHttpMethod();

	    /**
	     * Sets the HTTP method (GET, POST, etc) to use when sending this request.
         */
        void SetHttpMethod(HttpMethod httMethod);
	    
        /**
	     * Returns the optional stream containing the payload data to include for
	     * this request.  Not all requests will contain payload data.
	     */
        Stream GetContent();

	    /**
	     * Sets the optional stream containing the payload data to include for this
	     * request. Not all requests will contain payload data.
         */
        void SetContent(Stream content);

        /**
         * Returns the original, user facing request object which this internal
         * request object is representing.
         */
        KS3Request GetOriginalRequest();

        /**
         * Returns the optional value for time offset for this request.  This
         * will be used by the signer to adjust for potential clock skew.  
         * Value is in seconds, positive values imply the current clock is "fast",
         * negative values imply clock is slow.
         */
        int GetTimeOffset();

        /**
         * Sets the optional value for time offset for this request.  This
         * will be used by the signer to adjust for potential clock skew.  
         * Value is in seconds, positive values imply the current clock is "fast",
         * negative values imply clock is slow.
         */
        void SetTimeOffset(int timeOffset);
    }
}
