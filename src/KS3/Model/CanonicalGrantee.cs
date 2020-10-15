namespace KS3.Model
{
    /// <summary>
    /// Represents a grantee identified by their canonical KS3 ID. The canonical KS3
    /// ID can be thought of as an KS3-internal ID specific to a user.
    /// </summary>
    public class CanonicalGrantee : IGrantee
    {
        private readonly string _id;

        public string DisplayName { get; set; }

        /// <summary>
        /// Constructs a new CanonicalGrantee object with the given canonical ID.
        /// </summary>
        /// <param name="id"></param>
        public CanonicalGrantee(string id) : this(id, "")
        {

        }

        public CanonicalGrantee(string id, string displayName)
        {
            _id = id;
            DisplayName = displayName;
        }

        public string GetTypeIdentifier()
        {
            return "id";
        }

        /// <summary>
        /// Gets the group grantee's URI.
        /// </summary>
        /// <returns></returns>
        public string GetIdentifier()
        {
            return _id;
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj.GetType().Equals(GetType()))
            {
                CanonicalGrantee other = (CanonicalGrantee)obj;
                return _id.Equals(other._id);
            }
            return false;
        }

        public override string ToString()
        {
            return $"CanonicalGrantee [id='{_id}',displayName='{DisplayName}']";
        }


    }
}
