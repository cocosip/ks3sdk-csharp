using System;
using System.Collections.Generic;

namespace KS3.Model
{
    public class ListMultipartUploadsResult
    {
        public string BucketName { get; set; }
        public string ObjectKey { get; set; }
        public string UploadId { get; set; }
        public bool IsTruncated { get; set; }
        public List<Part> Parts { get; set; }
        public ListMultipartUploadsResult()
        {
            Parts = new List<Part>();
        }
    }
    public class Part
    {
        public int PartNumber { get; set; }

        public string ETag { get; set; }

        public DateTime LastModified { get; set; }

        public int Size { get; set; }
    }
}
