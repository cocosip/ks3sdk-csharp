using KS3.Http;
using KS3.Model;

namespace KS3.Internal
{
    public class HeadBucketResponseHandler : IHttpResponseHandler<HeadBucketResult> 
    {
        public HeadBucketResult Handle(System.Net.HttpWebResponse response)
        {
            HeadBucketResult result = new HeadBucketResult
            {
                StatueCode = response.StatusCode
            };
            return result;
        }
    }
}
