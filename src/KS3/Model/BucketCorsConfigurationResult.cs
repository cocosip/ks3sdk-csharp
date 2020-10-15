using System.Collections.Generic;

namespace KS3.Model
{
    public class BucketCorsConfigurationResult
    {
        public List<CorsRule> Rules { get; set; }

        public BucketCorsConfigurationResult AddCorsRule(CorsRule corsRule)
        {
            Rules.Add(corsRule);
            return this;
        }
    }
}
