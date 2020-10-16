using KS3.Http;
using KS3.Model;
using System.Net;

namespace KS3.Internal
{
    public class PutAdpResponseHandler:IHttpResponseHandler<PutAdpResult> 
    {
        public PutAdpResult Handle(HttpWebResponse response)
        {
            PutAdpResult result = new PutAdpResult
            {
                Status = response.StatusCode
            };
            if (HttpStatusCode.OK.Equals(response.StatusCode))
            {
                result.TaskId = response.Headers.Get(Headers.TaskId);
            }
            return result;
        }
    }
}
