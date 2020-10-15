using KS3.Model;
using System.Collections.Generic;
using System.IO;

namespace KS3
{
    /**
     * Provides an interface for accessing the KS3.
     */
    public interface IKS3
    {
        /**
         * Overrides the default endpoint for this client.
         */
        void SetEndpoint(string endpoint);

        /**
         * Returns a list of all KS3 buckets that the authenticated sender of the request owns. 
         */
        IList<Bucket> ListBuckets();

        /**
         * Returns a list of all KS3 buckets that the authenticated sender of the request owns. 
         */
        IList<Bucket> ListBuckets(ListBucketsRequest listBucketRequest);

        /**
         * Deletes the specified bucket. 
         */
        void DeleteBucket(string bucketName);

        /**
         * Deletes the specified bucket. 
         */
        void DeleteBucket(DeleteBucketRequest deleteBucketRequest);

        /**
         * Gets the AccessControlList (ACL) for the specified KS3 bucket.
         */
        AccessControlList GetBucketAcl(string bucketName);

        /**
         * Gets the AccessControlList (ACL) for the specified KS3 bucket.
         */
        AccessControlList GetBucketAcl(GetBucketAclRequest getBucketAclRequest);

        /**
         * Creates a new KS3 bucket. 
         */
        Bucket CreateBucket(string bucketName);

        /**
         * Creates a new KS3 bucket. 
         */
        Bucket CreateBucket(CreateBucketRequest createBucketRequest);

        /**
         * Sets the AccessControlList for the specified KS3 bucket.
         */
        void SetBucketAcl(string bucketName, AccessControlList acl);

        /**
         * Sets the AccessControlList for the specified KS3 bucket.
         */
        void SetBucketAcl(string bucketName, CannedAccessControlList cannedAcl);

        /**
         * Sets the AccessControlList for the specified KS3 bucket.
         */
        void SetBucketAcl(SetBucketAclRequest setBucketAclRequset);

        /**
         * Returns a list of summary information about the objects in the specified bucket.
         */
        ObjectListing ListObjects(string bucketName);

        /**
         * Returns a list of summary information about the objects in the specified bucket.
         */
        ObjectListing ListObjects(string bucketName, string prefix);

        /**
         * Returns a list of summary information about the objects in the specified bucket.
         */
        ObjectListing ListObjects(ListObjectsRequest listObjectRequest);

        /**
         * Deletes the specified object in the specified bucket.
         */
        void DeleteObject(string bucketName, string key);

        /**
         * Deletes the specified object in the specified bucket.
         */
        void DeleteObject(DeleteObjectRequest deleteObjectRequest);

        /**
         * Gets the object stored in KS3 under the specified bucket and key.
         */
        KS3Object GetObject(string bucketName, string key);

        /**
         * Gets the object stored in KS3 under the specified bucket and key, and saves the object contents to the specified file.
         */
        KS3Object GetObject(string bucketName, string key, FileInfo destinationFile);

        /**
         * Gets the object stored in KS3 under the specified bucket and key.
         */
        KS3Object GetObject(GetObjectRequest getObjectRequest);

        /**
         * Gets the metadata for the specified KS3 object without actually fetching the object itself.
         */
        ObjectMetadata GetObjectMetadata(string bucketName, string key);

        /**
         * Gets the metadata for the specified KS3 object without actually fetching the object itself.
         */
        ObjectMetadata GetObjectMetadata(GetObjectMetadataRequest getObjectMetadataRequest);

        /**
         * Uploads the specified file to KS3 under the specified bucket and key name.
         */
        PutObjectResult PutObject(string bucketName, string key, FileInfo file);

        /**
         * Uploads the specified input stream and object metadata to KS3 under the specified bucket and key name. 
         */
        PutObjectResult PutObject(string bucketName, string key, Stream input, ObjectMetadata metadata);

        /**
         * Uploads a new object to the specified KS3 bucket.
         */
        PutObjectResult PutObject(PutObjectRequest putObjectRequest);

        InitiateMultipartUploadResult InitiateMultipartUpload(string bucketname, string objectkey);

        InitiateMultipartUploadResult InitiateMultipartUpload(InitiateMultipartUploadRequest request);

        /**
         * Gets the AccessControlList (ACL) for the specified object in KS3.
         */
        AccessControlList GetObjectAcl(string bucketName, string key);

        /**
         * Gets the AccessControlList (ACL) for the specified object in KS3.
         */
        AccessControlList GetObjectAcl(GetObjectAclRequest getObjectAclRequest);

        /**
         * Sets the AccessControlList for the specified object in KS3.
         */
        void SetObjectAcl(string bucketName, string key, AccessControlList acl);

        /**
         * Sets the AccessControlList for the specified object in KS3.
         */
        void SetObjectAcl(string bucketName, string key, CannedAccessControlList cannedAcl);

        /**
         * Sets the AccessControlList for the specified object in KS3.
         */
        void SetObjectAcl(SetObjectAclRequest setObjectRequestAcl);
    }
}
