namespace KS3.Model
{
    public class CompleteMultipartUploadResult
    {
        public string Location { get; set; }

        public string Bucket { get; set; }

        public string Key { get; set; }

        public string ETag { get; set; }
    }
}
