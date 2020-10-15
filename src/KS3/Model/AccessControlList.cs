using System;
using System.Collections.Generic;
using System.Text;

namespace KS3.Model
{
    /// <summary>
    /// Represents an KS3 Access Control List (ACL), including the ACL's set of grantees and the permissions assigned to each grantee.
    /// </summary>
    public class AccessControlList
    {
        private readonly ISet<Grant> _grants;

        public Owner Owner { get; set; }

        public AccessControlList()
        {
            _grants = new HashSet<Grant>();
        }


        /// <summary>
        /// Adds a grantee to the access control list (ACL) with the given permission.
        /// If this access control list already contains the grantee (i.e. the same grantee object) the permission for the grantee will be updated.
        /// </summary>
        /// <param name="grantee"></param>
        /// <param name="permission"></param>
        public void GrantPermission(IGrantee grantee, string permission)
        {
            _grants.Add(new Grant(grantee, permission));
        }

        /// <summary>
        /// Adds a set of grantee/permission pairs to the access control list (ACL), where each item in the set is a Gran object.
        /// </summary>
        /// <param name="grantList"></param>
        public void GrantAllPermissions(List<Grant> grantList)
        {
            foreach (Grant grant in grantList)
            {
                GrantPermission(grant.Grantee, grant.Permission);
            }
        }

        /// <summary>
        /// Revokes the permissions of a grantee by removing the grantee from the access control list (ACL).
        /// </summary>
        /// <param name="grantee"></param>
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
            {
                _grants.Remove(grant);
            }
        }

        /// <summary>
        /// Gets the set of Grant objects in this access control list (ACL).
        /// </summary>
        /// <returns></returns>
        public ISet<Grant> GetGrants()
        {
            return _grants;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("AccessControlList:");
            builder.AppendLine("Owner:" + Owner);
            builder.AppendLine("Grants:");

            foreach (Grant grant in _grants)
            {
                builder.AppendLine("" + grant);
            }
            return builder.ToString();
        }
    }
}
