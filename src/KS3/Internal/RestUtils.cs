﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using KS3.Model;

namespace KS3.Internal
{
    public static class RestUtils
    {
        private static IList<String> SIGNED_PARAMETERS = new List<String> {
            "acl","adp","torrent", "logging", "location", "policy", "requestPayment", "versioning",
            "versions", "versionId", "notification", "uploadId", "uploads", "partNumber", "website",
            "delete", "lifecycle", "tagging", "cors", "restore",
            "response-cache-contro", "response-content-disposition", "response-content-encoding",
            "response-content-language", "response-content-type", "response-expires"};


        /**
         * Calculate the canonical string for a REST/HTTP request to KS3.
         */
        public static String makeKS3CanonicalString<T>(String method, String resource, IRequest<T> request, String expires) where T : KS3Request
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(method + "\n");

            // Add all interesting headers to a list, then sort them.  "Interesting"
            // is defined as Content-MD5, Content-Type, Date, and x-kss-
            IDictionary<String, String> headers = request.GetHeaders();
            IDictionary<String, String> interestingHeaders = new SortedDictionary<String, String>();
            if (headers != null && headers.Count > 0)
            {
                foreach (String name in headers.Keys)
                {
                    String value = headers[name];

                    String lname = name.ToLower();

                    // Ignore any headers that are not particularly interesting.
                    if (lname.Equals(Headers.CONTENT_TYPE.ToLower()) || lname.Equals(Headers.CONTENT_MD5.ToLower()) || lname.Equals(Headers.DATE.ToLower()) ||
                        lname.StartsWith(Headers.KS3_PREFIX))
                        interestingHeaders.Add(lname, value);
                }
            }

            // Remove default date timestamp if "x-kss-date" is set.
            if (interestingHeaders.ContainsKey(Headers.KS3_ALTERNATE_DATE))
                interestingHeaders[Headers.DATE.ToLower()] = "";

            // Use the expires value as the timestamp if it is available. This trumps both the default
            // "date" timestamp, and the "x-kss-date" header.
            if (expires != null)
                interestingHeaders[Headers.DATE.ToLower()] = expires;

            // These headers require that we still put a new line in after them,
            // even if they don't exist.
            if (!interestingHeaders.ContainsKey(Headers.CONTENT_TYPE.ToLower()))
                interestingHeaders.Add(Headers.CONTENT_TYPE.ToLower(), "");
            if (!interestingHeaders.ContainsKey(Headers.CONTENT_MD5.ToLower()))
                interestingHeaders.Add(Headers.CONTENT_MD5.ToLower(), "");

            // Any parameters that are prefixed with "x-kss-" need to be included
            // in the headers section of the canonical string to sign
            foreach (String name in request.GetParameters().Keys)
                if (name.StartsWith(Headers.KS3_PREFIX))
                {
                    String value = request.GetParameters()[name];
                    interestingHeaders[name] = value;
                }

            // Add all the interesting headers (i.e.: all that startwith x-kss- ;-))
            foreach (String name in interestingHeaders.Keys)
            {
                if (name.StartsWith(Headers.KS3_PREFIX))
                    buf.Append(name + ":" + interestingHeaders[name]);
                else buf.Append(interestingHeaders[name]);
                buf.Append("\n");
            }

            // Add all the interesting parameters
            resource = resource.Replace("%5C", "/").Replace("//", "/%2F");
            resource = resource.EndsWith("%2F") ? resource.Substring(0, resource.Length - 3) : resource;
            buf.Append(resource);
            String[] parameterNames = request.GetParameters().Keys.ToArray();
            Array.Sort(parameterNames);
            char separator = '?';
            foreach (String parameterName in parameterNames)
            {
                // Skip any parameters that aren't part of the canonical signed string
                if (!SIGNED_PARAMETERS.Contains(parameterName)) continue;

                buf.Append(separator);
                buf.Append(parameterName);
                String parameterValue = request.GetParameters()[parameterName];
                if (parameterValue != null) buf.Append("=" + parameterValue);

                separator = '&';
            }

            return buf.ToString();
        }

        public static void PopulateObjectMetadata(HttpWebResponse response, ObjectMetadata metadata)
        {
            ISet<string> ignoredHeaders = new HashSet<string> {
                Headers.DATE,
                Headers.SERVER,
                Headers.REQUEST_ID,
                Headers.CONNECTION };
            foreach (string name in response.Headers.AllKeys)
            {
                if (name.StartsWith(Headers.KS3_USER_METADATA_PREFIX))
                {
                    string value = response.Headers[name];
                    string key = name.Substring(Headers.KS3_USER_METADATA_PREFIX.Length);

                    metadata.SetUserMetaData(key, value);
                }
                else if (ignoredHeaders.Contains(name))
                {
                    // ignore...
                }
                else if (name.Equals(Headers.LAST_MODIFIED, StringComparison.OrdinalIgnoreCase))
                {
                    metadata.SetHeader(name, DateTime.Parse(response.Headers[name]));
                }
                else if (name.Equals(Headers.CONTENT_LENGTH, StringComparison.OrdinalIgnoreCase))
                {
                    metadata.SetHeader(name, long.Parse(response.Headers[name]));
                }
                else if (name.Equals(Headers.ETAG))
                {
                    metadata.SetHeader(name, removeQuotes(response.Headers[name]));
                }
                else
                {
                    metadata.SetHeader(name, response.Headers[name]);
                }
            }
        }

        /**
         * Removes any surrounding quotes from the specified string and returns a
         * new string.
         */
        private static String removeQuotes(String s)
        {
            if (s == null) return null;
            s = s.Trim();
            if (s.StartsWith("\"")) s = s.Substring(1);
            if (s.EndsWith("\"")) s = s.Substring(0, s.Length - 1);
            return s;
        }
    }
}
