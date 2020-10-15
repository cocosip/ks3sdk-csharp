using System.Collections.Generic;

namespace KS3.Model
{
    public class DeleteMultipleObjectsResult
    {
        /// <summary>
        /// the success delete keys
        /// </summary>
        public List<string> Deleted { get; set; }

        /// <summary>
        /// the error delete keys and error message
        /// </summary>
        public List<DeleteMultipleObjectsError> Errors { get; set; }

        public DeleteMultipleObjectsResult()
        {
            Deleted = new List<string>();
            Errors = new List<DeleteMultipleObjectsError>();
        }


        public DeleteMultipleObjectsResult AddDeleteErrors(DeleteMultipleObjectsError error)
        {
            Errors.Add(error);
            return this;
        }


    }
}
