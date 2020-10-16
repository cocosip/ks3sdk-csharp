namespace KS3.Auth
{
    /// <summary>
    /// Basic implementation of the KS3Credentials interface that allows callers to pass in the KS3 access key and secret access in the constructor.
    /// </summary>
    public class BasicKS3Credentials : IKS3Credentials
    {

        public string KS3AccessKeyId { get; }
        public string KS3SecretKey { get; }

        public BasicKS3Credentials(string kS3AccessKeyId, string kS3SecretKey)
        {
            KS3AccessKeyId = kS3AccessKeyId;
            KS3SecretKey = kS3SecretKey;
        }

    }
}
