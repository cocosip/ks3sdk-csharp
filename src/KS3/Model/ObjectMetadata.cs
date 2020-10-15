using System;
using System.Collections.Generic;
using System.Text;

namespace KS3.Model
{
    public class ObjectMetadata
    {
        /// <summary>
        /// Custom user metadata, represented in responses with the x-kss-meta- header prefix
        /// </summary>
        public Dictionary<string, string> UserMetadata { get; }

        /// <summary>
        ///  All other (non user custom) headers such as Content-Length, Content-Type,
        /// </summary>
        public Dictionary<string, object> Metadata { get; }

        public ObjectMetadata()
        {
            UserMetadata = new Dictionary<string, string>();
            Metadata = new Dictionary<string, object>();
        }


        /// <summary>
        /// For internal use only. Sets a specific metadata header value. Not intended to be called by external code.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetHeader(string key, object value)
        {
            Metadata[key] = value;
        }

        public void SetUserMetaData(string key, string value)
        {
            UserMetadata[key] = value;
        }

        /// <summary>
        /// Gets the value of the Last-Modified header, indicating the date and time at which KS3 last recorded a modification to the associated object.
        /// </summary>
        /// <returns></returns>
        public DateTime? GetLastModified()
        {
            if (Metadata.TryGetValue(Headers.LAST_MODIFIED, out object value))
            {
                return (DateTime)value;
            }
            return null;
        }

        /// <summary>
        /// For internal use only. Sets the Last-Modified header value indicating the date and time at which KS3 last recorded a modification to the associated object.
        /// </summary>
        /// <param name="lastModified"></param>
        public void SetLastModified(DateTime lastModified)
        {
            Metadata[Headers.LAST_MODIFIED] = lastModified;
        }

        /// <summary>
        ///  Gets the Content-Length HTTP header indicating the size of the associated object in bytes.
        /// </summary>
        /// <returns></returns>
        public long GetContentLength()
        {
            if (Metadata.TryGetValue(Headers.CONTENT_LENGTH, out object value))
            {
                return (long)value;
            }
            return default;
        }

        /// <summary>
        ///  Sets the Content-Length HTTP header indicating the size of the associated object in bytes.
        /// </summary>
        /// <param name="contentLength"></param>
        public void SetContentLength(long contentLength)
        {
            Metadata[Headers.CONTENT_LENGTH] = contentLength;
        }

        /// <summary>
        /// Gets the Content-Type HTTP header, which indicates the type of content stored in the associated object. The value of this header is a standard MIME type.
        /// </summary>
        /// <returns></returns>
        public string GetContentType()
        {
            if (Metadata.TryGetValue(Headers.CONTENT_TYPE, out object value))
            {
                return (string)value;
            }
            return string.Empty;
        }

        /// <summary>
        ///  Sets the Content-Type HTTP header indicating the type of content stored in the associated object. The value of this header is a standard MIME type.
        /// </summary>
        /// <param name="contentType"></param>
        public void SetContentType(string contentType)
        {
            Metadata[Headers.CONTENT_TYPE] = contentType;
        }

        /// <summary>
        /// Gets the optional Content-Encoding HTTP header specifying what content encodings have been applied to the object and what decoding mechanisms must be applied in order to obtain the media-type referenced by the Content-Type field.
        /// </summary>
        /// <returns></returns>
        public string GetContentEncoding()
        {
            if (Metadata.TryGetValue(Headers.CONTENT_ENCODING, out object value))
            {
                return (string)value;
            }
            return string.Empty;
        }

        /// <summary>
        /// Sets the optional Content-Encoding HTTP header specifying what content encodings have been applied to the object and what decoding mechanisms must be applied in order to obtain the media-type referenced by the Content-Type field.
        /// </summary>
        /// <param name="encoding"></param>
        public void SetContentEncoding(string encoding)
        {
            Metadata[Headers.CONTENT_ENCODING] = encoding;
        }

        /// <summary>
        /// Sets the base64 encoded 128-bit MD5 digest of the associated object (content - not including headers) according to RFC 1864.
        /// This data is used as a message integrity check to verify that the data received by KS3 is the same data that the caller sent.
        /// If set to null,then the MD5 digest is removed from the metadata.
        /// </summary>
        /// <param name="md5Base64"></param>
        public void SetContentMD5(string md5Base64)
        {
            if (string.IsNullOrWhiteSpace(md5Base64))
            {
                Metadata.Remove(Headers.CONTENT_MD5);
            }
            else
            {
                Metadata[Headers.CONTENT_MD5] = md5Base64;
            }
        }

        /// <summary>
        /// Gets the base64 encoded 128-bit MD5 digest of the associated object (content - not including headers) according to RFC 1864.
        /// This data is used as a message integrity check to verify that the data received by KS3 is the same data that the caller sent.
        /// </summary>
        /// <returns></returns>
        public string GetContentMD5()
        {
            if (Metadata.TryGetValue(Headers.CONTENT_MD5, out object value))
            {
                return (string)value;
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the hex encoded 128-bit MD5 digest of the associated object according to RFC 1864.
        /// This data is used as an integrity check to verify that the data received by the caller is the same data that was sent by KS3.
        /// </summary>
        /// <returns></returns>
        public string GetETag()
        {
            if (Metadata.TryGetValue(Headers.ETAG, out object value))
            {
                return (string)value;
            }
            return string.Empty;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("<metadata>");
            foreach (string name in Metadata.Keys)
            {
                builder.Append("\n" + name + ": " + Metadata[name]);
            }
            builder.Append("\n</metadata>");

            builder.Append("\n<userMetadata>");
            foreach (string name in UserMetadata.Keys)
            {
                builder.Append("\n" + name + ": " + UserMetadata[name]);
            }
            builder.Append("\n</userMetadata>");

            return builder.ToString();
        }
    }
}
