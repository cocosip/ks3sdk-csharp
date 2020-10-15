namespace KS3.Model
{
    /**
     * Provides options for deleting a specified object in a specified bucket. 
     */
    public class DeleteObjectRequest : KS3Request
    {

        /// <summary>
        /// The name of the KS3 bucket containing the object to delete.
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// The key of the object to delete.
        /// </summary>
        public string Key { get; set; }


        /// <summary>
        /// Constructs a new DeleteObjectRequest, specifying the object's bucket name and key.
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        public DeleteObjectRequest(string bucketName, string key)
        {
            BucketName = bucketName;
            Key = key;
        }

    }
}
