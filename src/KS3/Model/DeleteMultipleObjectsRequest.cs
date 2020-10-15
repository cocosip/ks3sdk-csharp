using KS3.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace KS3.Model
{
    public class DeleteMultipleObjectsRequest : KS3Request, ICalculatorMd5
    {

        public string BucketName { get; set; }

        public List<string> ObjectKeys { get; set; }

        public DeleteMultipleObjectsRequest()
        {
            ObjectKeys = new List<string>();
        }


        private string GetXmlContent()
        {
            Validate();
            //XNamespace v = "http://s3.amazonaws.com/doc/2006-03-01/";
            XElement root = new XElement("Delete");
            foreach (string key in ObjectKeys)
            {
                XElement Object = new XElement("Object");
                Object.Add(new XElement("Key", key));
                root.Add(Object);
            }
            return root.ToString();
        }
        public Stream ToXmlAdapter()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(GetXmlContent()));
        }


        /// <summary>
        /// get the md5 digest byte and convert to base64 string
        /// </summary>
        /// <returns></returns>
        public string GetMd5()
        {
            byte[] md5 = Md5Util.Md5Digest(GetXmlContent());
            return Convert.ToBase64String(md5);
        }

        private void Validate()
        {
            if (string.IsNullOrEmpty(BucketName))
            {
                throw new Exception("bucketname is not null");
            }
            if (ObjectKeys.Any())
            {
                throw new Exception("objectKeys is not null or empty");
            }
        }

    }
}
