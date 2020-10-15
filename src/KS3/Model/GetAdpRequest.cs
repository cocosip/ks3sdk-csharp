namespace KS3.Model
{
    public class GetAdpRequest : KS3Request
    {
        public string TaskId { get; set; }

        public GetAdpRequest()
        {

        }

        public GetAdpRequest(string taskId)
        {
            TaskId = taskId;
        }
    }
}
