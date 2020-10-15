using KS3.Http;
using KS3.Model;
using System.IO;
using System.Net;

namespace KS3.Internal
{
    public class ObjectResponseHandler : IHttpResponseHandler<KS3Object>
    {
        private readonly GetObjectRequest _getObjectRequest;

        public ObjectResponseHandler(GetObjectRequest getObjectRequest)
        {
            _getObjectRequest = getObjectRequest;
        }

        public KS3Object Handle(HttpWebResponse response)
        {
            KS3Object ks3Object = new KS3Object();

            FileInfo destinationFile = _getObjectRequest.DestinationFile;
            byte[] content = null;

            IProgressListener progressListener = _getObjectRequest.ProgressListener;

            ObjectMetadata metadata = new ObjectMetadata();
            RestUtils.populateObjectMetadata(response, metadata);
            ks3Object.setObjectMetadata(metadata);

            Stream input = null, output = null;

            try
            {
                input = response.GetResponseStream();

                if (progressListener != null)
                    input = new ProgressReportingInputStream(input, progressListener);

                int SIZE = Constants.DEFAULT_STREAM_BUFFER_SIZE;
                byte[] buf = new byte[SIZE];

                if (destinationFile != null)
                    output = new FileStream(_getObjectRequest.DestinationFile.FullName, FileMode.Create);
                else
                {
                    content = new byte[metadata.getContentLength()];
                    output = new MemoryStream(content);
                }

                for (; ; )
                {
                    int size = input.Read(buf, 0, SIZE);
                    if (size <= 0) break;
                    output.Write(buf, 0, size);
                }
            }
            finally
            {
                if (input != null)
                    input.Close();

                if (output != null)
                    output.Close();
            }

            if (destinationFile != null)
                ks3Object.setObjectContent(destinationFile.OpenRead());
            else
                ks3Object.setObjectContent(new MemoryStream(content));

            return ks3Object;
        }
    }
}
