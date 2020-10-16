using KS3.Internal;
using KS3.Model;

namespace KS3.Auth
{
    public class KS3Signer<T> : ISigner<T> where T : KS3Request
    {
        /// <summary>
        /// The HTTP verb (GET, PUT, HEAD, DELETE) the request to sign is using.
        /// </summary>
        private readonly string _httpVerb;

        /// <summary>
        /// The canonical resource path portion of the S3 string to sign. Examples: "/", "/<bucket name>/", or "/<bucket name>/<key>"
        /// </summary>
        private readonly string _resourcePath;

        /// <summary>
        /// Constructs a new KS3Signer to sign requests based on the KS3 credentials, HTTP method and canonical KS3 resource path.
        /// </summary>
        /// <param name="httpVerb"></param>
        /// <param name="resourcePath"></param>
        public KS3Signer(string httpVerb, string resourcePath)
        {
            _httpVerb = httpVerb;
            _resourcePath = resourcePath;
        }

        public void Sign(IRequest<T> request, IKS3Credentials credentials)
        {
            string date = SignerUtils.GetSignatrueDate(request.GetTimeOffset());
            request.SetHeader(Headers.DATE, date);

            string canonicalString = RestUtils.makeKS3CanonicalString(_httpVerb, _resourcePath, request, null);

            string signature = SignerUtils.Base64(SignerUtils.HmacSha1(credentials.KS3SecretKey, canonicalString));
            request.SetHeader("Authorization", "KSS " + credentials.KS3AccessKeyId + ":" + signature);
        }

        public string GetSignature(IKS3Credentials credentials, string expires)
        {
            var voidRequest = new DefaultRequest<NoneKS3Request>(new NoneKS3Request());
            string canonicalString = RestUtils.makeKS3CanonicalString(_httpVerb, _resourcePath, voidRequest, expires);
            string signature = SignerUtils.Base64(SignerUtils.HmacSha1(credentials.KS3SecretKey, canonicalString));
            return signature;
        }

    }
}
