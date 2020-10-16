using System;

namespace KS3.KS3Exception
{
    public class ProgressInterruptedException : Exception
    {
        public ProgressInterruptedException(string message) :
            base(message)
        {

        }

        public ProgressInterruptedException(string message, Exception cause) :
            base(message, cause)
        {

        }
    }
}
