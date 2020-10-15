namespace KS3.Model
{
    public class HeadObjectResult
    {
        public ObjectMetadata ObjectMetadata { get; set; }

        public bool IfModified { get; set; } = true;

        public bool IfPreconditionSuccess { get; set; } = true;

        public HeadObjectResult()
        {
            ObjectMetadata = new ObjectMetadata();
        }

    }
}
