using System;
using System.Collections.Generic;
using System.Text;

namespace KS3.Model
{
    /**
     * Represents an KS3 Access Control List (ACL), including the ACL's set of
     * grantees and the permissions assigned to each grantee.
     */
    public class AccessControlList
    {
        private ISet<Grant> _grants = new HashSet<Grant>();
        public Owner Owner { get; set; }

        /**
         * Adds a grantee to the access control list (ACL) with the given permission. 
         * If this access control list already
         * contains the grantee (i.e. the same grantee object) the permission for the
         * grantee will be updated.
         */
        public void GrantPermission(IGrantee grantee, String permission)
        {
            _grants.Add(new Grant(grantee, permission));
        }

        /**
         * Adds a set of grantee/permission pairs to the access control list (ACL), where each item in the
         * set is a Gran object.
         */
        public void GrantAllPermissions(List<Grant> grantList)
        {
            foreach (Grant grant in grantList)
            {
                GrantPermission(grant.Grantee, grant.Permission);
            }
        }

        /**
         * Revokes the permissions of a grantee by removing the grantee from the access control list (ACL).
         */
        public void RevokeAllPermissions(IGrantee grantee)
        {
            var grantsToRemove = new List<Grant>();
            foreach (Grant grant in _grants)
            {
                if (grant.Grantee.Equals(grantee))
                {
                    grantsToRemove.Add(grant);
                }
            }

            foreach (Grant grant in grantsToRemove)
                _grants.Remove(grant);
        }

        /**
         * Gets the set of Grant objects in this access control list (ACL).
         */
        public ISet<Grant> getGrants()
        {
            return _grants;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("AccessControlList:");
            builder.Append("\nOwner:\n" + Owner);
            builder.Append("\nGrants:");

            foreach (Grant grant in _grants)
                builder.Append("\n" + grant);

            return builder.ToString();
        }
    }
}
