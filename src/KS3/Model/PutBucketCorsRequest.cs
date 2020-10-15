using KS3.Http;
using KS3.Internal;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace KS3.Model
{
    public class PutBucketCorsRequest : KS3Request, ICalculatorMd5
    {
        public string BucketName { get; set; }

        public BucketCorsConfigurationResult BucketCorsConfiguration { get; set; }
        public PutBucketCorsRequest() { }
        public PutBucketCorsRequest(string bucketName, BucketCorsConfigurationResult bucketCorsConfiguration)
        {
            BucketName = bucketName;
            BucketCorsConfiguration = bucketCorsConfiguration;
        }
        private string GetXmlContent()
        {
            Validate();
            XNamespace v = "http://s3.amazonaws.com/doc/2006-03-01/";
            XElement root = new XElement(v + "CORSConfiguration");

            foreach (CorsRule cr in BucketCorsConfiguration.Rules)
            {
                XElement CORSRule = new XElement("CORSRule");
                foreach (string origin in cr.AllowedOrigins)
                {
                    CORSRule.Add(new XElement("AllowedOrigin", origin));
                }
                foreach (string header in cr.AllowedHeaders)
                {
                    CORSRule.Add(new XElement("AllowedHeader", header));
                }
                foreach (string eheader in cr.ExposedHeaders)
                {
                    CORSRule.Add(new XElement("ExposeHeader", eheader));
                }
                foreach (HttpMethod method in cr.AllowedMethods)
                {
                    CORSRule.Add(new XElement("AllowedMethod", method.ToString()));
                }
                CORSRule.Add(new XElement("MaxAgeSeconds", cr.MaxAgeSeconds));
                root.Add(CORSRule);
            }
            return root.ToString();
        }
        /// <summary>
        /// return the xml stream content
        /// </summary>
        /// <returns></returns>
        public Stream toXmlAdapter()
        {
            return new MemoryStream(Encoding.Default.GetBytes(GetXmlContent()));
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
            if (string.IsNullOrWhiteSpace(BucketName))
            {
                throw new Exception("bucketname is not null");
            }
            if (!BucketCorsConfiguration.Rules.Any())
            {
                throw new Exception("cors rules is not null");
            }
            if (BucketCorsConfiguration.Rules.Count > Constants.corsMaxRules)
            {
                throw new Exception("cors rules number must limit in " + Constants.corsMaxRules);
            }
            foreach (CorsRule cr in BucketCorsConfiguration.Rules)
            {
                if (!cr.AllowedMethods.Any())
                {
                    throw new Exception("bucketCorsConfiguration.rules.allowedMethods not null");
                }
                if (!cr.AllowedOrigins.Any())
                {
                    throw new Exception("bucketCorsConfiguration.rules.allowedOrigins not null");
                }
            }
        }
    }
}
