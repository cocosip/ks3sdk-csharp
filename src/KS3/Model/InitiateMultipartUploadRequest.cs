namespace KS3.Model
{
    public class InitiateMultipartUploadRequest : KS3Request
    {
        public string BucketName { get; set; }

        public string Objectkey { get; set; }

        public ObjectMetadata ObjectMeta { get; set; }

        public AccessControlList Acl { get; set; }

        public CannedAccessControlList CannedAcl { get; set; }

        public string RedirectLocation { get; set; }

        public InitiateMultipartUploadRequest()
        {
            ObjectMeta = new ObjectMetadata();
            Acl = new AccessControlList();
            CannedAcl = new CannedAccessControlList();
        }

        public InitiateMultipartUploadRequest(string bucketname, string objectkey) : this()
        {
            BucketName = bucketname;
            Objectkey = objectkey;
        }


    }
}
