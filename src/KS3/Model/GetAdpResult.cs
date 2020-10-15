using System.Collections.Generic;
using System.Xml.Linq;

namespace KS3.Model
{
    public class GetAdpResult
    {
        public XDocument Doc { get; set; }

        public string TaskId { get; set; }

        public string Processstatus { get; set; }

        public string Notifystatus { get; set; }

        /// <summary>
        /// 每条命令的具体处理结果
        /// </summary>
        public List<AdpInfo> AdpInfos { get; set; }

        public GetAdpResult()
        {
            AdpInfos = new List<AdpInfo>();
        }

    }
}
