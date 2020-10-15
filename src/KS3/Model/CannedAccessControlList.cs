namespace KS3.Model
{
    /**
     * Specifies constants defining a canned access control list.
     */
    public class CannedAccessControlList
    {
        public static string PUBLICK_READ_WRITE = "public-read-write";
        public static string PUBLICK_READ = "public-read";
        public static string PRIVATE = "private";

        /** The KS3 x-kss-acl header value representing the canned acl */
        private readonly string _cannedAclHeader;

        public CannedAccessControlList()
        {

        }

        public CannedAccessControlList(string cannedAclHeader)
        {
            _cannedAclHeader = cannedAclHeader;
        }

        /**
         * Returns the KS3 x-kss-acl header value for this canned acl.
         */
        public string GetCannedAclHeader()
        {
            return _cannedAclHeader;
        }
    }
}
