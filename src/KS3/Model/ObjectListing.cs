using System.Collections.Generic;
using System.Text;

namespace KS3.Model
{
    /// <summary>
    /// Contains the results of listing the objects in an KS3 bucket.
    /// </summary>
    public class ObjectListing
    {
        /// <summary>
        /// A list of summary information describing the objects stored in the bucket
        /// </summary>
        public List<ObjectSummary> ObjectSummaries { get; set; }

        /// <summary>
        /// A list of the common prefixes included in this object listing - common prefixes will only be populated for requests that specified a delimiter
        /// </summary>
        public List<string> CommonPrefixes { get; set; }

        /// <summary>
        /// The name of the KS3 bucket containing the listed objects
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// The marker to use in order to request the next page of results - only populated if the isTruncated member indicates that this object listing is truncated
        /// </summary>
        public string NextMarker { get; set; }

        /// <summary>
        /// Indicates if this is a complete listing, or if the caller needs to make additional requests to KS3 to see the full object listing for an KS3  bucket
        /// </summary>
        public bool Truncated { get; set; }

        /// <summary>
        /// The prefix parameter originally specified by the caller when this object listing was returned
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// The marker parameter originally specified by the caller when this object listing was returned
        /// </summary>
        public string Marker { get; set; }

        /// <summary>
        /// The maxKeys parameter originally specified by the caller when this object listing was returned
        /// </summary>
        public int? MaxKeys { get; set; }

        /// <summary>
        /// The delimiter parameter originally specified by the caller when this object listing was returned
        /// </summary>
        public string Delimiter { get; set; }

        public ObjectListing()
        {
            ObjectSummaries = new List<ObjectSummary>();
            CommonPrefixes = new List<string>();
        }



        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append($"ObjectListing [bucketName={BucketName}");
            if (!string.IsNullOrWhiteSpace(Delimiter))
            {
                builder.Append($", delimiter={Delimiter}");
            }
            if (MaxKeys.HasValue)
            {
                builder.Append($", maxKeys={MaxKeys.Value}");
            }
            if (!string.IsNullOrWhiteSpace(Prefix))
            {
                builder.Append($", prefix={Prefix}");
            }
            if (!string.IsNullOrWhiteSpace(Marker))
            {
                builder.Append($", prefix={Marker}");
            }
            if (!string.IsNullOrWhiteSpace(NextMarker))
            {
                builder.Append($", nextMarker={NextMarker}");
            }

            builder.Append($", isTruncated={Truncated}]");

            foreach (var objectSummary in ObjectSummaries)
            {
                builder.AppendLine("Object:" + objectSummary.ToString());
            }
            foreach (string s in CommonPrefixes)
            {
                builder.AppendLine("CommonPrefix:" + s);
            }
            return builder.ToString();
        }
    }
}
