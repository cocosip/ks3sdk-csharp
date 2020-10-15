using System;
using System.Collections.Generic;
using System.Text;

namespace KS3.Model
{
    public class PutAdpRequest : KS3Request
    {
        public string BucketName { get; set; }

        public string ObjectKey { get; set; }

        /// <summary>
        /// 数据处理任务完成后通知的url
        /// </summary>
        public string NotifyURL { get; set; }

        public List<Adp> Adps { get; set; }

        public PutAdpRequest()
        {
            Adps = new List<Adp>();
        }

        public PutAdpRequest AddAdp(Adp adp)
        {
            Adps.Add(adp);
            return this;
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(BucketName))
            {
                throw new Exception("bucketname is not null");
            }
            if (string.IsNullOrWhiteSpace(ObjectKey))
            {
                throw new Exception("objectKey is not null");
            }
            if (Adps.Count == 0)
            {
                throw new Exception("adps is not null");
            }
            else
            {
                foreach (Adp adp in Adps)
                {
                    if (string.IsNullOrEmpty(adp.Command))
                    {
                        throw new Exception("adp's Command is not null");
                    }
                }
            }
            if (string.IsNullOrEmpty(NotifyURL))
            {
                throw new Exception("notifyURL is not null");
            }
        }

        public string ConvertAdpsToString()
        {
            Validate();
            var fopStringBuffer = new StringBuilder();
            foreach (Adp fop in Adps)
            {
                fopStringBuffer.Append(fop.Command);
                fopStringBuffer.Append("|tag=saveas");
                if (!string.IsNullOrWhiteSpace(fop.Bucket))
                {
                    fopStringBuffer.Append("&bucket=" + fop.Bucket);
                }
                if (!string.IsNullOrWhiteSpace(fop.Key))
                {
                    fopStringBuffer.Append($"&object={Convert.ToBase64String(Encoding.UTF8.GetBytes(fop.Key))}");
                }
                fopStringBuffer.Append(";");
            }
            return fopStringBuffer.ToString().TrimEnd(';');
        }
    }
}
