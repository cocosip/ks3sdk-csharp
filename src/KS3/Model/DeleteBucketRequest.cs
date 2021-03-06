﻿namespace KS3.Model
{
    /// <summary>
    /// Provides options for deleting a specified bucket. KS3 buckets can only be deleted when empty.
    /// </summary>
    public class DeleteBucketRequest : KS3Request
    {
        /// <summary>
        ///  The name of the KS3 bucket to delete.
        /// </summary>
        public string BucketName { get; set; }


        /// <summary>
        ///  Constructs a new DeleteBucketRequest, ready to be executed to delete the specified bucket.
        /// </summary>
        /// <param name="bucketName"></param>
        public DeleteBucketRequest(string bucketName)
        {
            BucketName = bucketName;
        }

    }
}
