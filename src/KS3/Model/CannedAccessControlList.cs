namespace KS3.Model
{
    /// <summary>
    /// Specifies constants defining a canned access control list.
    /// </summary>
    public class CannedAccessControlList
    {
        public static string PUBLICK_READ_WRITE = "public-read-write";
        public static string PUBLICK_READ = "public-read";
        public static string PRIVATE = "private";

        /// <summary>
        /// The KS3 x-kss-acl header value representing the canned acl 
        /// </summary>
        private readonly string _cannedAclHeader;

        public CannedAccessControlList()
        {

        }

        public CannedAccessControlList(string cannedAclHeader)
        {
            _cannedAclHeader = cannedAclHeader;
        }

        /// <summary>
        ///  Returns the KS3 x-kss-acl header value for this canned acl.
        /// </summary>
        /// <returns></returns>
        public string GetCannedAclHeader()
        {
            return _cannedAclHeader;
        }
    }
}
