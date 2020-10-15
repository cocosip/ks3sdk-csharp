using KS3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace KS3.Transform
{
    public class ListMultipartUploadsResultUnmarshaller : IUnmarshaller<ListMultipartUploadsResult, Stream>
    {
        public ListMultipartUploadsResult Unmarshall(Stream input)
        {
            var re = new ListMultipartUploadsResult();
            XDocument doc = XDocument.Load(input);
            var xml = doc.Elements().First().Elements();

            re.BucketName = xml.Where(w => w.Name.LocalName == "Bucket").ToList()[0].Value;
            re.ObjectKey = xml.Where(w => w.Name.LocalName == "Key").ToList()[0].Value;
            re.UploadId = xml.Where(w => w.Name.LocalName == "UploadId").ToList()[0].Value;
            re.IsTruncated = Convert.ToBoolean(xml.Where(w => w.Name.LocalName == "IsTruncated").ToList()[0].Value);

            var plist = new List<Part>();
            var parts = xml.Where(x => x.Name.LocalName == "Part").ToList();
            foreach (var item in parts)
            {
                var p = new Part
                {
                    PartNumber = Convert.ToInt32(item.Element("PartNumber").Value),
                    ETag = item.Element("ETag").Value,
                    LastModified = Convert.ToDateTime(item.Element("LastModified").Value),
                    Size = Convert.ToInt32(item.Element("Size").Value)
                };
                plist.Add(p);
            }
            re.Parts.AddRange(plist);
            return re;
        }
    }
}
