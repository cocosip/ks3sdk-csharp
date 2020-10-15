/**
 * Provides access to the KS3 credentials used for accessing KS3 services: KS3
 * access key ID and secret access key. These credentials are used to securely
 * sign requests to KS3 services.
 */
namespace KS3.Auth
{
    public interface IKS3Credentials
    {
        /**
         * Returns the KS3 access key ID for this credentials object.
         */
        string GetKS3AccessKeyId();
        
        /**
         * Returns the KS3 secret access key for this credentials object.
         */ 
        string GetKS3SecretKey();
    }
}
