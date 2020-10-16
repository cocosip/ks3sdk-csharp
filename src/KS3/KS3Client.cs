using KS3.Auth;
using KS3.Http;
using KS3.Internal;
using KS3.KS3Exception;
using KS3.Model;
using KS3.Transform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


namespace KS3
{
    public class KS3Client : IKS3
    {
        private XmlResponseHandler<Type> voidResponseHandler = new XmlResponseHandler<Type>(null);

        /// <summary>
        /// KS3 credentials.
        /// </summary>
        private readonly IKS3Credentials _ks3Credentials;

        /// <summary>
        /// The service endpoint to which this client will send requests.
        /// </summary>
        private Uri _endpoint;

        /// <summary>
        /// The client configuration
        /// </summary>
        private ClientConfiguration _clientConfiguration;

        /// <summary>
        /// Low level client for sending requests to KS3. 
        /// </summary>
        private KS3HttpClient _client;

        /// <summary>
        /// Optional offset (in seconds) to use when signing requests
        /// </summary>
        private int _timeOffset;

        /**
         * Constructs a new KS3Client object using the specified Access Key ID and Secret Key.
         */
        public KS3Client(string accessKey, string secretKey)
            : this(new BasicKS3Credentials(accessKey, secretKey)) { }

        /**
         * Constructs a new KS3Client object using the specified configuration.
         */
        public KS3Client(IKS3Credentials ks3Credentials)
            : this(ks3Credentials, new ClientConfiguration()) { }

        /**
         * Constructs a new KS3Client object using the specified Access Key ID, Secret Key and configuration.
         */
        public KS3Client(string accessKey, string secretKey, ClientConfiguration clientConfiguration)
            : this(new BasicKS3Credentials(accessKey, secretKey), clientConfiguration) { }

        /**
         * Constructs a new KS3Client object using the specified credential and configuration.
         */
        public KS3Client(IKS3Credentials ks3Credentials, ClientConfiguration clientConfiguration)
        {
            _clientConfiguration = clientConfiguration;
            _client = new KS3HttpClient(clientConfiguration);
            _ks3Credentials = ks3Credentials;

            Init();
        }

        private void Init()
        {
            SetEndpoint(Constants.KS3_HOSTNAME);
        }

        public void SetEndpoint(string endpoint)
        {
            if (!endpoint.Contains("://"))
            {
                endpoint = _clientConfiguration.Protocol + "://" + endpoint;
            }
            _endpoint = new Uri(endpoint);
        }

        public void SetConfiguration(ClientConfiguration clientConfiguration)
        {
            _clientConfiguration = clientConfiguration;
            _client = new KS3HttpClient(clientConfiguration);
        }

        /**
         * Sets the optional value for time offset for this client.  This
         * value will be applied to all requests processed through this client.
         * Value is in seconds, positive values imply the current clock is "fast",
         * negative values imply clock is slow.
         */
        public void SetTimeOffset(int timeOffset)
        {
            this._timeOffset = timeOffset;
        }

        /**
         * Returns the optional value for time offset for this client.  This
         * value will be applied to all requests processed through this client.
         * Value is in seconds, positive values imply the current clock is "fast",
         * negative values imply clock is slow.
         */
        public int GetTimeOffset()
        {
            return _timeOffset;
        }

        /**
         * Returns a list of all KS3 buckets that the authenticated sender of the request owns. 
         */
        public IList<Bucket> ListBuckets()
        {
            return ListBuckets(new ListBucketsRequest());
        }

        /**
         * Returns a list of all KS3 buckets that the authenticated sender of the request owns. 
         */
        public IList<Bucket> ListBuckets(ListBucketsRequest listBucketsRequest)
        {
            IRequest<ListBucketsRequest> request = CreateRequest(null, null, listBucketsRequest, HttpMethod.GET);
            return Invoke(request, new ListBucketsUnmarshaller(), null, null);
        }

        /**
         * Deletes the specified bucket. 
         */
        public void DeleteBucket(string bucketName)
        {
            DeleteBucket(new DeleteBucketRequest(bucketName));
        }

        /**
         * Deletes the specified bucket. 
         */
        public void DeleteBucket(DeleteBucketRequest deleteBucketRequest)
        {
            string bucketName = deleteBucketRequest.BucketName;

            IRequest<DeleteBucketRequest> request = CreateRequest(bucketName, null, deleteBucketRequest, HttpMethod.DELETE);
            this.Invoke(request, voidResponseHandler, bucketName, null);
        }

        /**
         * Gets the AccessControlList (ACL) for the specified KS3 bucket.
         */
        public AccessControlList GetBucketAcl(string bucketName)
        {
            return GetBucketAcl(new GetBucketAclRequest(bucketName));
        }

        /// <summary>
        ///  Gets the AccessControlList (ACL) for the specified KS3 bucket.
        /// </summary>
        /// <param name="getBucketAclRequest"></param>
        /// <returns></returns>
        public AccessControlList GetBucketAcl(GetBucketAclRequest getBucketAclRequest)
        {
            string bucketName = getBucketAclRequest.BucketName;

            IRequest<GetBucketAclRequest> request = CreateRequest(bucketName, null, getBucketAclRequest, HttpMethod.GET);
            request.SetParameter("acl", null);

            return Invoke(request, new AccessControlListUnmarshaller(), bucketName, null);
        }

        /**
         * Creates a new KS3 bucket. 
         */
        public Bucket CreateBucket(string bucketName)
        {
            return CreateBucket(new CreateBucketRequest(bucketName));
        }

        /**
         * Creates a new KS3 bucket. 
         */
        public Bucket CreateBucket(CreateBucketRequest createBucketRequest)
        {
            var bucketName = createBucketRequest.BucketName;

            IRequest<CreateBucketRequest> request = CreateRequest(bucketName, null, createBucketRequest, HttpMethod.PUT);
            request.GetHeaders()[Headers.CONTENT_LENGTH] = "0";

            if (createBucketRequest.Acl != null)
            {
                AddAclHeaders(request, createBucketRequest.Acl);
            }
            else if (createBucketRequest.CannedAcl != null)
            {
                request.SetHeader(Headers.KS3_CANNED_ACL, createBucketRequest.CannedAcl.GetCannedAclHeader());
            }

            Invoke(request, voidResponseHandler, bucketName, null);

            return new Bucket(bucketName);
        }
        /// <summary>
        /// This operation is useful to determine if a bucket exists and you have permission to access it
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public HeadBucketResult headBucket(string bucketName)
        {
            return headBucket(new HeadBucketRequest(bucketName));
        }
        public HeadBucketResult headBucket(HeadBucketRequest headBucketRequest)
        {
            string bucketname = headBucketRequest.BucketName;
            IRequest<HeadBucketRequest> request = this.CreateRequest(bucketname, null, headBucketRequest, HttpMethod.HEAD);
            return this.Invoke(request, new HeadBucketResponseHandler(), bucketname, null);
        }
        /// <summary>
        /// Returns the cors configuration information set for the bucket.
        /// To use this operation, you must have permission to perform the s3:GetBucketCORS action. By default, the bucket owner has this permission and can grant it to others.
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public BucketCorsConfigurationResult getBucketCors(string bucketName)
        {
            return this.getBucketCors(new GetBucketCorsRequest(bucketName));
        }
        public BucketCorsConfigurationResult getBucketCors(GetBucketCorsRequest getBucketCorsRequest)
        {
            BucketCorsConfigurationResult result = new BucketCorsConfigurationResult();
            string bucketname = getBucketCorsRequest.BucketName;
            IRequest<GetBucketCorsRequest> request = this.CreateRequest(bucketname, null, getBucketCorsRequest, HttpMethod.GET);
            request.GetParameters().Add("cors", null);
            result = this.Invoke(request, new BucketCorsConfigurationResultUnmarshaller(), bucketname, null);
            return result;
        }
        /// <summary>
        /// This implementation of the GET operation uses the location subresource to return a bucket's region. You set the bucket's region using the LocationConstraint request parameter in a PUT Bucket request. 
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public GetBucketLocationResult getBucketLocation(string bucketName)
        {
            return getBucketLocation(new GetBucketLocationRequest(bucketName));
        }
        public GetBucketLocationResult getBucketLocation(GetBucketLocationRequest getBucketLocationRequest)
        {
            GetBucketLocationResult result = new GetBucketLocationResult();
            IRequest<GetBucketLocationRequest> request = this.CreateRequest(getBucketLocationRequest.BucketName, null, getBucketLocationRequest, HttpMethod.GET);
            request.GetParameters().Add("location", null);
            result = this.Invoke(request, new GetBucketLocationResultUnmarshaller(), getBucketLocationRequest.BucketName, null);
            return result;
        }
        /// <summary>
        /// This implementation of the GET operation uses the logging subresource to return the logging status of a bucket and the permissions users have to view and modify that status. To use GET, you must be the bucket owner. 
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public GetBucketLoggingResult getBucketLogging(string bucketName)
        {
            return getBucketLogging(new GetBucketLoggingRequest(bucketName));
        }
        public GetBucketLoggingResult getBucketLogging(GetBucketLoggingRequest getBucketLoggingRequest)
        {
            GetBucketLoggingResult result = new GetBucketLoggingResult();
            IRequest<GetBucketLoggingRequest> request = this.CreateRequest(getBucketLoggingRequest.BucketName, null, getBucketLoggingRequest, HttpMethod.GET);
            request.GetParameters().Add("logging", null);
            result = this.Invoke(request, new GetBucketLoggingResultUnmarshaller(), getBucketLoggingRequest.BucketName, null);
            return result;
        }

        /**
         * Sets the AccessControlList for the specified KS3 bucket.
         */
        public void SetBucketAcl(string bucketName, AccessControlList acl)
        {
            this.SetBucketAcl(new SetBucketAclRequest(bucketName, acl));
        }

        /**
         * Sets the AccessControlList for the specified KS3 bucket.
         */
        public void SetBucketAcl(string bucketName, CannedAccessControlList cannedAcl)
        {
            this.SetBucketAcl(new SetBucketAclRequest(bucketName, cannedAcl));
        }

        /// <summary>
        /// Sets the AccessControlList for the specified KS3 bucket.
        /// </summary>
        /// <param name="setBucketAclRequest"></param>
        public void SetBucketAcl(SetBucketAclRequest setBucketAclRequest)
        {
            var bucketName = setBucketAclRequest.BucketName;
            AccessControlList acl = setBucketAclRequest.Acl;
            CannedAccessControlList cannedAcl = setBucketAclRequest.CannedAcl;

            IRequest<SetBucketAclRequest> request = this.CreateRequest(bucketName, null, setBucketAclRequest, HttpMethod.PUT);

            if (acl != null)
            {
                string xml = AclXmlFactory.ConvertToXmlString(acl);
                MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

                request.SetContent(memoryStream);
                request.SetHeader(Headers.CONTENT_LENGTH, memoryStream.Length.ToString());
            }
            else if (cannedAcl != null)
            {
                request.SetHeader(Headers.KS3_CANNED_ACL, cannedAcl.GetCannedAclHeader());
                request.SetHeader(Headers.CONTENT_LENGTH, "0");
            }

            request.SetParameter("acl", null);

            this.Invoke(request, this.voidResponseHandler, bucketName, null);
        }
        /// <summary>
        /// Sets the cors configuration for your bucket. If the configuration exists, Amazon S3 replaces it. 
        /// </summary>
        /// <param name="putBucketCorsRequest"></param>
        public void setBucketCors(PutBucketCorsRequest putBucketCorsRequest)
        {
            IRequest<PutBucketCorsRequest> request = this.CreateRequest(putBucketCorsRequest.BucketName, null, putBucketCorsRequest, HttpMethod.PUT);
            request.GetParameters().Add("cors", null);
            request.SetHeader(Headers.CONTENT_LENGTH, putBucketCorsRequest.toXmlAdapter().Length.ToString());
            request.SetHeader(Headers.CONTENT_TYPE, "application/xml");
            request.SetHeader(Headers.CONTENT_MD5, putBucketCorsRequest.GetMd5());
            request.SetContent(putBucketCorsRequest.toXmlAdapter());
            this.Invoke(request, this.voidResponseHandler, putBucketCorsRequest.BucketName, null);
        }
        /// <summary>
        /// This implementation of the PUT operation uses the logging subresource to set the logging parameters for a bucket and to specify permissions for who can view and modify the logging parameters. To set the logging status of a bucket, you must be the bucket owner.
        /// </summary>
        /// <param name="putBucketLoggingRequest"></param>
        public void setBucketLogging(PutBucketLoggingRequest putBucketLoggingRequest)
        {
            IRequest<PutBucketLoggingRequest> request = this.CreateRequest(putBucketLoggingRequest.BucketName, null, putBucketLoggingRequest, HttpMethod.PUT);
            request.GetParameters().Add("logging", null);
            request.SetHeader(Headers.CONTENT_LENGTH, putBucketLoggingRequest.ToXmlAdapter().Length.ToString());
            request.SetHeader(Headers.CONTENT_TYPE, "application/xml");
            request.SetContent(putBucketLoggingRequest.ToXmlAdapter());
            this.Invoke(request, this.voidResponseHandler, putBucketLoggingRequest.BucketName, null);
        }
        /// <summary>
        /// Deletes the cors configuration information set for the bucket.
        /// </summary>
        /// <param name="bucketName"></param>
        public void deleteBucketCors(string bucketName)
        {
            deleteBucketCors(new DeleteBucketCorsRequest(bucketName));
        }
        public void deleteBucketCors(DeleteBucketCorsRequest deleteBucketCorsRequest)
        {
            IRequest<DeleteBucketCorsRequest> request = this.CreateRequest(deleteBucketCorsRequest.BucketName, null, deleteBucketCorsRequest, HttpMethod.DELETE);
            request.GetParameters().Add("cors", null);
            this.Invoke(request, this.voidResponseHandler, deleteBucketCorsRequest.BucketName, null);
        }
        /// <summary>
        /// The Multi-Object Delete operation enables you to delete multiple objects from a bucket using a single HTTP request.
        /// </summary>
        /// <param name="deleteMultipleObjectsRequest"></param>
        /// <returns></returns>
        public DeleteMultipleObjectsResult deleteMultiObjects(DeleteMultipleObjectsRequest deleteMultipleObjectsRequest)
        {
            IRequest<DeleteMultipleObjectsRequest> request = this.CreateRequest(deleteMultipleObjectsRequest.BucketName, null, deleteMultipleObjectsRequest, HttpMethod.POST);
            request.GetParameters().Add("delete", null);
            request.SetHeader(Headers.CONTENT_LENGTH, deleteMultipleObjectsRequest.ToXmlAdapter().Length.ToString());
            request.SetHeader(Headers.CONTENT_TYPE, "application/xml");
            request.SetHeader(Headers.CONTENT_MD5, deleteMultipleObjectsRequest.GetMd5());
            request.SetContent(deleteMultipleObjectsRequest.ToXmlAdapter());
            return this.Invoke(request, new DeleteMultipleObjectsResultUnmarshaller(), deleteMultipleObjectsRequest.BucketName, null);
        }
        /**
         * Returns a list of summary information about the objects in the specified bucket.
         */
        public ObjectListing ListObjects(string bucketName)
        {
            return this.ListObjects(new ListObjectsRequest(bucketName, null, null, null, null));
        }

        /// <summary>
        /// Returns a list of summary information about the objects in the specified bucket.
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public ObjectListing ListObjects(string bucketName, string prefix)
        {
            return ListObjects(new ListObjectsRequest(bucketName, prefix, null, null, null));
        }

        /// <summary>
        /// Returns a list of summary information about the objects in the specified bucket.
        /// </summary>
        /// <param name="listObjectRequest"></param>
        /// <returns></returns>
        public ObjectListing ListObjects(ListObjectsRequest listObjectRequest)
        {
            var bucketName = listObjectRequest.BucketName;
            IRequest<ListObjectsRequest> request = CreateRequest(bucketName, null, listObjectRequest, HttpMethod.GET);

            if (!string.IsNullOrWhiteSpace(listObjectRequest.Prefix))
            {
                request.SetParameter("prefix", listObjectRequest.Prefix);
            }
            if (!string.IsNullOrWhiteSpace(listObjectRequest.Marker))
            {
                request.SetParameter("marker", listObjectRequest.Marker);
            }
            if (!string.IsNullOrWhiteSpace(listObjectRequest.Delimiter))
            {
                request.SetParameter("delimiter", listObjectRequest.Delimiter);
            }

            if (listObjectRequest.MaxKeys.HasValue)
            {
                request.SetParameter("max-keys", listObjectRequest.MaxKeys.ToString());
            }

            return Invoke(request, new ListObjectsUnmarshallers(), bucketName, null);
        }

        /// <summary>
        /// Deletes the specified object in the specified bucket.
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        public void DeleteObject(string bucketName, string key)
        {
            DeleteObject(new DeleteObjectRequest(bucketName, key));
        }

        /// <summary>
        /// Deletes the specified object in the specified bucket.
        /// </summary>
        /// <param name="deleteObjectRequest"></param>
        public void DeleteObject(DeleteObjectRequest deleteObjectRequest)
        {
            var bucketName = deleteObjectRequest.BucketName;
            var key = deleteObjectRequest.Key;
            IRequest<DeleteObjectRequest> request = CreateRequest(bucketName, key, deleteObjectRequest, HttpMethod.DELETE);
            Invoke(request, voidResponseHandler, bucketName, key);
        }

        /**
         * Gets the object stored in KS3 under the specified bucket and key.
         */
        public KS3Object GetObject(string bucketName, string key)
        {
            return this.GetObject(new GetObjectRequest(bucketName, key));
        }

        /**
         * Gets the object stored in KS3 under the specified bucket and key, and saves the object contents to the specified file.
         */
        public KS3Object GetObject(string bucketName, string key, FileInfo destinationFile)
        {
            return this.GetObject(new GetObjectRequest(bucketName, key, destinationFile));
        }

        /**
         * Gets the object stored in KS3 under the specified bucket and key.
         */
        public KS3Object GetObject(GetObjectRequest getObjectRequest)
        {
            string bucketName = getObjectRequest.BucketName;
            string key = getObjectRequest.Key;

            IRequest<GetObjectRequest> request = CreateRequest(bucketName, key, getObjectRequest, HttpMethod.GET);

            if (getObjectRequest.Range.Any())
            {
                var range = getObjectRequest.Range;
                request.SetHeader(Headers.RANGE, range[0].ToString() + "-" + range[1].ToString());
            }

            AddDateHeader(request, Headers.GET_OBJECT_IF_MODIFIED_SINCE, getObjectRequest.ModifiedSinceConstraint);
            AddDateHeader(request, Headers.GET_OBJECT_IF_UNMODIFIED_SINCE, getObjectRequest.UnmodifiedSinceConstraint);
            AddstringListHeader(request, Headers.GET_OBJECT_IF_MATCH, getObjectRequest.MatchingETagConstraints);
            AddstringListHeader(request, Headers.GET_OBJECT_IF_NONE_MATCH, getObjectRequest.NonmatchingETagContraints);

            IProgressListener progressListener = getObjectRequest.ProgressListener;

            FireProgressEvent(progressListener, ProgressEvent.STARTED);

            KS3Object ks3Object = null;
            try
            {
                ks3Object = this.Invoke(request, new ObjectResponseHandler(getObjectRequest), bucketName, key);
            }
            catch (ProgressInterruptedException e)
            {
                FireProgressEvent(progressListener, ProgressEvent.CANCELED);
                throw e;
            }
            catch (Exception e)
            {
                FireProgressEvent(progressListener, ProgressEvent.FAILED);
                throw e;
            }
            FireProgressEvent(progressListener, ProgressEvent.COMPLETED);

            ks3Object.BucketName = bucketName;
            ks3Object.Key = key;

            return ks3Object;
        }

        /*
         * Gets the metadata for the specified KS3 object without actually fetching the object itself.
         */
        public ObjectMetadata GetObjectMetadata(string bucketName, string key)
        {
            return this.GetObjectMetadata(new GetObjectMetadataRequest(bucketName, key));
        }

        /// <summary>
        /// Gets the metadata for the specified KS3 object without actually fetching the object itself.
        /// </summary>
        /// <param name="getObjectMetadataRequest"></param>
        /// <returns></returns>
        public ObjectMetadata GetObjectMetadata(GetObjectMetadataRequest getObjectMetadataRequest)
        {
            string bucketName = getObjectMetadataRequest.BucketName;
            string key = getObjectMetadataRequest.Key;

            IRequest<GetObjectMetadataRequest> request = CreateRequest(bucketName, key, getObjectMetadataRequest, HttpMethod.HEAD);

            return Invoke(request, new MetadataResponseHandler(), bucketName, key);
        }

        /// <summary>
        /// Uploads the specified file to KS3 under the specified bucket and key name.
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public PutObjectResult PutObject(string bucketName, string key, FileInfo file)
        {
            PutObjectRequest putObjectRequest = new PutObjectRequest(bucketName, key, file);
            putObjectRequest.Metadata = new ObjectMetadata();
            return this.PutObject(putObjectRequest);
        }

        /// <summary>
        /// Uploads the specified input stream and object metadata to KS3 under the specified bucket and key name. 
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        /// <param name="input"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public PutObjectResult PutObject(string bucketName, string key, Stream input, ObjectMetadata metadata)
        {
            return PutObject(new PutObjectRequest(bucketName, key, input, metadata));
        }

        /// <summary>
        ///  Uploads a new object to the specified KS3 bucket.
        /// </summary>
        /// <param name="putObjectRequest"></param>
        /// <returns></returns>
        public PutObjectResult PutObject(PutObjectRequest putObjectRequest)
        {
            var bucketName = putObjectRequest.BucketName;
            var key = putObjectRequest.Key;
            ObjectMetadata metadata = putObjectRequest.Metadata;
            var input = putObjectRequest.InputStream;
            IProgressListener progressListener = putObjectRequest.ProgressListener;
            if (metadata == null)
            {
                metadata = new ObjectMetadata();
            }

            // If a file is specified for upload, we need to pull some additional
            // information from it to auto-configure a few options
            if (putObjectRequest.File != null)
            {
                FileInfo file = putObjectRequest.File;

                // Always set the content length, even if it's already set
                metadata.SetContentLength(file.Length);

                // Only set the content type if it hasn't already been set
                if (metadata.GetContentType() == null)
                {
                    metadata.SetContentType(Mimetypes.GetMimetype(file));
                }
                if (metadata.GetContentMD5() == null)
                {
                    using (FileStream fileStream = file.OpenRead())
                    {
                        MD5 md5 = MD5.Create();
                        metadata.SetContentMD5(Convert.ToBase64String(md5.ComputeHash(fileStream)));
                    }
                }

                input = file.OpenRead();
            }
            else
            {
                metadata.SetContentLength(input.Length);

                if (metadata.GetContentType() == null)
                {
                    metadata.SetContentType(Mimetypes.DEFAULT_MIMETYPE);
                }
                if (metadata.GetContentMD5() == null)
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        metadata.SetContentMD5(Convert.ToBase64String(md5.ComputeHash(input)));
                    }

                    input.Seek(0, SeekOrigin.Begin); // It is needed after calculated MD5.
                }
            }

            IRequest<PutObjectRequest> request = CreateRequest(bucketName, key, putObjectRequest, HttpMethod.PUT);

            if (putObjectRequest.Acl != null)
            {
                AddAclHeaders(request, putObjectRequest.Acl);
            }
            else if (putObjectRequest.CannedAcl != null)
            {
                request.SetHeader(Headers.KS3_CANNED_ACL, putObjectRequest.CannedAcl.GetCannedAclHeader());
            }
            if (progressListener != null)
            {
                input = new ProgressReportingInputStream(input, progressListener);
                FireProgressEvent(progressListener, ProgressEvent.STARTED);
            }

            PopulateRequestMetadata(metadata, request);
            request.SetContent(input);

            //-----------------------------------------------

            ObjectMetadata returnedMetadata = null;
            try
            {
                returnedMetadata = this.Invoke(request, new MetadataResponseHandler(), bucketName, key);
            }
            catch (ProgressInterruptedException e)
            {
                FireProgressEvent(progressListener, ProgressEvent.CANCELED);
                throw e;
            }
            catch (Exception e)
            {
                FireProgressEvent(progressListener, ProgressEvent.FAILED);
                throw e;
            }
            finally
            {
                if (input != null)
                {
                    input.Close();
                }
            }

            FireProgressEvent(progressListener, ProgressEvent.COMPLETED);

            var result = new PutObjectResult
            {
                ETag = returnedMetadata.GetETag(),
                ContentMD5 = metadata.GetContentMD5()
            };

            return result;
        }
        /// <summary>
        /// This implementation of the PUT operation creates a copy of an object that is already stored in S3. A PUT copy operation is the same as performing a GET and then a PUT. Adding the request header, x-amz-copy-source, makes the PUT operation copy the source object into the destination bucket.
        /// </summary>
        /// <param name="copyObjectRequest"></param>
        /// <returns></returns>
        public CopyObjectResult copyObject(CopyObjectRequest copyObjectRequest)
        {
            IRequest<CopyObjectRequest> request = this.CreateRequest(copyObjectRequest.DestinationBucket, copyObjectRequest.DestinationObject, copyObjectRequest, HttpMethod.PUT);
            request.GetHeaders().Add(Headers.XKssCopySource, "/" + copyObjectRequest.SourceBucket + "/" + UrlEncoder.Encode(copyObjectRequest.SourceObject, Encoding.UTF8));
            if (copyObjectRequest.AccessControlList != null)
                AddAclHeaders(request, copyObjectRequest.AccessControlList);
            else if (copyObjectRequest.CannedAcl != null)
                request.SetHeader(Headers.KS3_CANNED_ACL, copyObjectRequest.CannedAcl.GetCannedAclHeader());
            request.GetHeaders()[Headers.CONTENT_LENGTH] = "0";
            return this.Invoke(request, new CopyObjectResultUnmarshaller(), copyObjectRequest.DestinationBucket, copyObjectRequest.DestinationObject);
        }
        /// <summary>
        /// The HEAD operation retrieves metadata from an object without returning the object itself. This operation is useful if you are interested only in an object's metadata. To use HEAD, you must have READ access to the object.
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="objectKey"></param>
        /// <returns></returns>
        public HeadObjectResult headObject(string bucketName, string objectKey)
        {
            return headObject(bucketName, objectKey);
        }
        public HeadObjectResult headObject(HeadObjectRequest headObjectRequest)
        {
            IRequest<HeadObjectRequest> request = this.CreateRequest(headObjectRequest.BucketName, headObjectRequest.ObjectKey, headObjectRequest, HttpMethod.HEAD);
            headObjectRequest.Validate();
            if (headObjectRequest.MatchingETagConstraints.Count > 0)
            {
                var Etags = new StringBuilder();
                foreach (string Etag in headObjectRequest.MatchingETagConstraints)
                {
                    Etags.Append(Etag);
                    Etags.Append(",");
                }
                request.GetHeaders().Add(Headers.GET_OBJECT_IF_MATCH, Etags.ToString().TrimEnd(','));
            }
            if (headObjectRequest.NonmatchingEtagConstraints.Count > 0)
            {
                StringBuilder noEtags = new StringBuilder();
                foreach (string etag in headObjectRequest.NonmatchingEtagConstraints)
                {
                    noEtags.Append(etag);
                    noEtags.Append(",");
                }
                request.GetHeaders().Add(Headers.GET_OBJECT_IF_NONE_MATCH, noEtags.ToString().TrimEnd(','));
            }
            if (headObjectRequest.ModifiedSinceConstraint.HasValue)
            {
                request.GetHeaders().Add(Headers.GET_OBJECT_IF_MODIFIED_SINCE, headObjectRequest.ModifiedSinceConstraint.Value.ToUniversalTime().ToString("r"));
            }
            if (!headObjectRequest.UnmodifiedSinceConstraint.Equals(DateTime.MinValue))
            {
                request.GetHeaders().Add(Headers.GET_OBJECT_IF_UNMODIFIED_SINCE, headObjectRequest.UnmodifiedSinceConstraint.ToUniversalTime().ToString("r"));
            }

            if (!string.IsNullOrEmpty(headObjectRequest.Overrides.CacheControl))
                request.GetParameters().Add("response-cache-control", headObjectRequest.Overrides.CacheControl);
            if (!string.IsNullOrEmpty(headObjectRequest.Overrides.ContentType))
                request.GetParameters().Add("&response-content-type", headObjectRequest.Overrides.ContentType);
            if (!string.IsNullOrEmpty(headObjectRequest.Overrides.ContentLanguage))
                request.GetParameters().Add("&response-content-language", headObjectRequest.Overrides.ContentLanguage);
            if (!string.IsNullOrEmpty(headObjectRequest.Overrides.Expires))
                request.GetParameters().Add("&response-expires", headObjectRequest.Overrides.Expires);
            if (!string.IsNullOrEmpty(headObjectRequest.Overrides.ContentDisposition))
                request.GetParameters().Add("&response-content-disposition", headObjectRequest.Overrides.ContentDisposition);
            if (!string.IsNullOrEmpty(headObjectRequest.Overrides.ContentEncoding))
                request.GetParameters().Add("&response-content-encoding", headObjectRequest.Overrides.ContentEncoding);

            return this.Invoke(request, new HeadObjectResultHandler(), headObjectRequest.BucketName, headObjectRequest.ObjectKey);
        }
        /**
         * init multi upload big file
         * **/
        public InitiateMultipartUploadResult InitiateMultipartUpload(string bucketname, string objectkey)
        {
            return InitiateMultipartUpload(new InitiateMultipartUploadRequest(bucketname, objectkey));
        }
        public InitiateMultipartUploadResult InitiateMultipartUpload(InitiateMultipartUploadRequest param)
        {
            IRequest<InitiateMultipartUploadRequest> request = this.CreateRequest(param.BucketName, param.Objectkey, param, HttpMethod.POST);
            request.SetParameter("uploads", null);
            request.GetHeaders()[Headers.CONTENT_LENGTH] = "0";
            InitiateMultipartUploadResult result = new InitiateMultipartUploadResult();
            result = this.Invoke(request, new MultipartUploadResultUnmarshaller(), param.BucketName, param.Objectkey);
            return result;
        }

        /// <summary>
        ///  upload multi file by part
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public PartETag UploadPart(UploadPartRequest param)
        {
            var bucketName = param.BucketName;
            var key = param.ObjectKey;
            ObjectMetadata metadata = param.Metadata;
            Stream input = param.InputStream;
            IProgressListener progressListener = param.ProgressListener;

            if (metadata == null)
            {
                metadata = new ObjectMetadata();
            }
            // If a file is specified for upload, we need to pull some additional
            // information from it to auto-configure a few options
            metadata.SetContentLength(input.Length);

            if (string.IsNullOrWhiteSpace(metadata.GetContentType()))
            {
                metadata.SetContentType(Mimetypes.DEFAULT_MIMETYPE);
            }
            if (metadata.GetContentMD5() == null)
            {
                using (MD5 md5 = MD5.Create())
                {
                    metadata.SetContentMD5(Convert.ToBase64String(md5.ComputeHash(input)));
                }

                input.Seek(0, SeekOrigin.Begin); // It is needed after calculated MD5.
            }

            IRequest<UploadPartRequest> request = this.CreateRequest(param.BucketName, param.ObjectKey, param, HttpMethod.PUT);
            request.SetParameter("partNumber", param.PartNumber.ToString());
            request.SetParameter("uploadId", param.UploadId);

            if (progressListener != null)
            {
                input = new ProgressReportingInputStream(input, progressListener);
                FireProgressEvent(progressListener, ProgressEvent.STARTED);
            }

            PopulateRequestMetadata(metadata, request);
            request.SetContent(input);

            //-----------------------------------------------

            ObjectMetadata returnedMetadata = null;
            try
            {
                returnedMetadata = this.Invoke(request, new MetadataResponseHandler(), bucketName, key);
            }
            catch (ProgressInterruptedException e)
            {
                FireProgressEvent(progressListener, ProgressEvent.CANCELED);
                throw e;
            }
            catch (Exception e)
            {
                FireProgressEvent(progressListener, ProgressEvent.FAILED);
                throw e;
            }
            finally
            {
                if (input != null)
                {
                    input.Close();

                }
            }

            FireProgressEvent(progressListener, ProgressEvent.COMPLETED);

            PartETag result = new PartETag(param.PartNumber, returnedMetadata.GetETag());

            return result;
        }

        /// <summary>
        /// getlist had uploaded part list
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public ListMultipartUploadsResult GetListMultipartUploads(ListMultipartUploadsRequest param)
        {
            IRequest<ListMultipartUploadsRequest> request = CreateRequest(param.BucketName, param.ObjectKey, param, HttpMethod.GET);
            request.SetParameter("uploadId", param.UploadId);
            request.GetHeaders()[Headers.CONTENT_LENGTH] = "0";
            ListMultipartUploadsResult result = Invoke(request, new ListMultipartUploadsResultUnmarshaller(), param.BucketName, param.ObjectKey);
            return result;
        }

        /// <summary>
        /// submit the all part,the server will complete join part
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public CompleteMultipartUploadResult CompleteMultipartUpload(CompleteMultipartUploadRequest param)
        {
            IRequest<CompleteMultipartUploadRequest> request = CreateRequest(param.BucketName, param.ObjectKey, param, HttpMethod.POST);
            request.SetParameter("uploadId", param.UploadId);
            request.SetHeader(Headers.CONTENT_LENGTH, param.Content.Length.ToString());
            request.SetContent(param.Content);
            CompleteMultipartUploadResult result = Invoke(request, new CompleteMultipartUploadResultUnmarshaller(), param.BucketName, param.ObjectKey);
            return result;
        }

        /// <summary>
        /// abort the upload opertion by uploadid
        /// </summary>
        /// <param name="param"></param>
        public void AbortMultipartUpload(AbortMultipartUploadRequest param)
        {
            IRequest<AbortMultipartUploadRequest> request = CreateRequest(param.BucketName, param.ObjectKey, param, HttpMethod.DELETE);
            request.SetParameter("uploadId", param.UploadId);
            request.GetHeaders()[Headers.CONTENT_LENGTH] = "0";
            Invoke(request, voidResponseHandler, param.BucketName, param.ObjectKey);
        }

        /**
         * Gets the AccessControlList (ACL) for the specified object in KS3.
         */
        public AccessControlList GetObjectAcl(string bucketName, string key)
        {
            return this.GetObjectAcl(new GetObjectAclRequest(bucketName, key));
        }

        /// <summary>
        ///  Gets the AccessControlList (ACL) for the specified object in KS3.
        /// </summary>
        /// <param name="getObjectAclRequest"></param>
        /// <returns></returns>
        public AccessControlList GetObjectAcl(GetObjectAclRequest getObjectAclRequest)
        {
            string bucketName = getObjectAclRequest.BucketName;
            string key = getObjectAclRequest.Key;

            IRequest<GetObjectAclRequest> request = this.CreateRequest(bucketName, key, getObjectAclRequest, HttpMethod.GET);
            request.SetParameter("acl", null);

            return this.Invoke(request, new AccessControlListUnmarshaller(), bucketName, key);
        }

        /**
         * Sets the AccessControlList for the specified object in KS3.
         */
        public void SetObjectAcl(string bucketName, string key, AccessControlList acl)
        {
            this.SetObjectAcl(new SetObjectAclRequest(bucketName, key, acl));
        }

        /**
         * Sets the AccessControlList for the specified object in KS3.
         */
        public void SetObjectAcl(string bucketName, string key, CannedAccessControlList cannedAcl)
        {
            this.SetObjectAcl(new SetObjectAclRequest(bucketName, key, cannedAcl));
        }

        /// <summary>
        /// Sets the AccessControlList for the specified object in KS3.
        /// </summary>
        /// <param name="setObjectAclRequest"></param>
        public void SetObjectAcl(SetObjectAclRequest setObjectAclRequest)
        {
            var bucketName = setObjectAclRequest.BukcetName;
            var key = setObjectAclRequest.Key;
            AccessControlList acl = setObjectAclRequest.Acl;
            CannedAccessControlList cannedAcl = setObjectAclRequest.CannedAcl;

            IRequest<SetObjectAclRequest> request = CreateRequest(bucketName, key, setObjectAclRequest, HttpMethod.PUT);

            if (acl != null)
            {
                string xml = AclXmlFactory.ConvertToXmlString(acl);
                MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

                request.SetContent(memoryStream);
                request.SetHeader(Headers.CONTENT_LENGTH, memoryStream.Length.ToString());
            }
            else if (cannedAcl != null)
            {
                request.SetHeader(Headers.KS3_CANNED_ACL, cannedAcl.GetCannedAclHeader());
                request.SetHeader(Headers.CONTENT_LENGTH, "0");
            }
            request.SetParameter("acl", null);

            Invoke(request, this.voidResponseHandler, bucketName, key);
        }
        /// <summary>
        /// generate presignerd url for private object with in limit times
        /// </summary>
        /// <param name="bucketName">bucketname</param>
        /// <param name="key">objectkey</param>
        /// <param name="expiration">expire time</param>
        /// <returns>url</returns>
        public string generatePresignedUrl(string bucketName, string key, DateTime expiration)
        {
            return GeneratePresignedUrl(bucketName, key, expiration, null);
        }
        /// <summary>
        /// generate PresignedUrl the url can apply for other user
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        /// <param name="expiration"></param>
        /// <param name="overrides"></param>
        /// <returns></returns>
        public string GeneratePresignedUrl(string bucketName, string key, DateTime expiration, ResponseHeaderOverrides overrides)
        {
            string url = "";
            string param = "";

            overrides ??= new ResponseHeaderOverrides();

            if (!string.IsNullOrWhiteSpace(overrides.CacheControl))
            {
                param += "response-cache-control=" + overrides.CacheControl;
            }
            if (!string.IsNullOrWhiteSpace(overrides.ContentType))
            {
                param += "&response-content-type=" + overrides.ContentType;
            }
            if (!string.IsNullOrWhiteSpace(overrides.ContentLanguage))
            {
                param += "&response-content-language=" + overrides.ContentLanguage;
            }
            if (!string.IsNullOrWhiteSpace(overrides.Expires))
            {
                param += "&response-expires=" + overrides.Expires;
            }

            if (!string.IsNullOrWhiteSpace(overrides.ContentDisposition))
            {
                param += "&response-content-disposition=" + overrides.ContentDisposition;
            }

            if (!string.IsNullOrWhiteSpace(overrides.ContentEncoding))
            {
                param += "&response-content-encoding=" + overrides.ContentEncoding;
            }

            var baselineTime = new DateTime(1970, 1, 1);
            var expires = Convert.ToInt64((expiration.ToUniversalTime() - baselineTime).TotalSeconds);
            try
            {
                KS3Signer<NoneKS3Request> ks3Signer = CreateSigner<NoneKS3Request>(HttpMethod.GET.ToString(), bucketName, key);
                string signer = ks3Signer.GetSignature(_ks3Credentials, expires.ToString());



                url += @"http://" + bucketName + "." + Constants.KS3_HOSTNAME
                             + "/" + FilterSpecial(UrlEncoder.Encode(key, Constants.DEFAULT_ENCODING)) + "?AccessKeyId="
                             + UrlEncoder.Encode(_ks3Credentials.KS3AccessKeyId, Constants.DEFAULT_ENCODING)
                             + "&Expires=" + expires + "&Signature="
                             + UrlEncoder.Encode(signer, Constants.DEFAULT_ENCODING) + "&" + param;

            }
            catch (Exception e)
            {
                throw e;
            }

            return url;
        }
        /// <summary>
        /// add Asynchronous Data Processing 可以通过adp执行图片缩略图处理、执行转码操作等
        /// </summary>
        /// <param name="putAdpRequest"></param>
        /// <returns></returns>
        public string putAdpTask(PutAdpRequest putAdpRequest)
        {
            IRequest<PutAdpRequest> request = CreateRequest(putAdpRequest.BucketName, putAdpRequest.ObjectKey, putAdpRequest, HttpMethod.PUT);
            request.GetParameters().Add("adp", null);

            request.GetHeaders().Add(Headers.AsynchronousProcessingList, putAdpRequest.ConvertAdpsToString());
            request.GetHeaders().Add(Headers.NotifyURL, putAdpRequest.NotifyURL);
            request.SetHeader(Headers.CONTENT_LENGTH, "0");
            PutAdpResult result = Invoke(request, new PutAdpResponseHandler(), putAdpRequest.BucketName, putAdpRequest.ObjectKey);
            return result.TaskId;
        }
        public GetAdpResult getAdpTask(GetAdpRequest getAdpRequest)
        {
            IRequest<GetAdpRequest> request = this.CreateRequest(getAdpRequest.TaskId, null, getAdpRequest, HttpMethod.GET);
            request.GetParameters().Add("queryadp", null);
            return this.Invoke(request, new GetAdpResultUnmarshaller(), null, null);
        }
        ////////////////////////////////////////////////////////////////////////////////////////


        /**
         * Creates and initializes a new request object for the specified KS3 resource.
         * Three parameters needed to be set
         * 1. http method (GET, PUT, HEAD or DELETE)
         * 2. endpoint (http or https, and the host name. e.g. http://kss.ksyun.com)
         * 3. resource path (bucketName/[key], e.g. my-bucket/my-object)
         */
        private IRequest<X> CreateRequest<X>(string bucketName, string key, X originalRequest, HttpMethod httpMethod) where X : KS3Request
        {
            IRequest<X> request = new DefaultRequest<X>(originalRequest);
            request.SetHttpMethod(httpMethod);
            request.SetEndpoint(_endpoint);

            string resourcePath = "/" + (bucketName != null ? bucketName + "/" : "") + (key != null ? key : "");
            resourcePath = UrlEncoder.Encode(resourcePath, Constants.DEFAULT_ENCODING);
            //resourcePath = filterSpecial(resourcePath);

            request.SetResourcePath(resourcePath);

            return request;
        }

        private X Invoke<X, Y>(IRequest<Y> request, IUnmarshaller<X, Stream> unmarshaller, string bucketName, string key) where Y : KS3Request
        {
            return Invoke(request, new XmlResponseHandler<X>(unmarshaller), bucketName, key);
        }

        /**
         * Before the KS3HttpClient deal with the request, we want the request looked like a collection of that:
         * 1. http method
         * 2. endpoint
         * 3. resource path
         * 4. headers
         * 5. parameters
         * 6. content
         * 7. time offset
         * 
         * The first three points are done in "createRequest".
         * The content was set before "createRequest" when we need to put a object to server. And some metadata like Content-Type, Content-Length, etc.
         * So at here, we need to complete 4, 5, and 7.
         */
        private X Invoke<X, Y>(IRequest<Y> request, IHttpResponseHandler<X> responseHandler, string bucket, string key) where Y : KS3Request
        {
            IDictionary<string, string> parameters = request.GetOriginalRequest().CopyPrivateRequestParameters();
            foreach (string name in parameters.Keys)
            {
                request.SetParameter(name, parameters[name]);
            }
            request.SetTimeOffset(_timeOffset);

            /**
             * The string we sign needs to include the exact headers that we
             * send with the request, but the client runtime layer adds the
             * Content-Type header before the request is sent if one isn't set, so
             * we have to set something here otherwise the request will fail.
             */
            if (!request.GetHeaders().ContainsKey(Headers.CONTENT_TYPE))
            {
                request.SetHeader(Headers.CONTENT_TYPE, Mimetypes.DEFAULT_MIMETYPE);
            }

            //Set the credentials which will be used by the KS3Signer later.
            if (request.GetOriginalRequest().Credentials == null)
            {
                request.GetOriginalRequest().Credentials = _ks3Credentials;
            }
            return _client.Excute(request, responseHandler, CreateSigner(request, bucket, key));
        }

        private KS3Signer<T> CreateSigner<T>(IRequest<T> request, string bucketName, string key) where T : KS3Request
        {
            return CreateSigner<T>(request.GetHttpMethod().ToString(), bucketName, key);
        }

        private KS3Signer<T> CreateSigner<T>(string httpMethod, string bucketName, string key) where T : KS3Request
        {
            string resourcePath = "/" + (bucketName != null ? bucketName + "/" : "") + (key != null ? key : "");
            resourcePath = UrlEncoder.Encode(resourcePath, Constants.DEFAULT_ENCODING);
            return new KS3Signer<T>(httpMethod, resourcePath);
        }


        /// <summary>
        /// Fires a progress event with the specified event type to the specified listener.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="eventType"></param>
        private static void FireProgressEvent(IProgressListener listener, int eventType)
        {
            if (listener == null) return;

            ProgressEvent e = new ProgressEvent(eventType);
            listener.ProgressChanged(e);
        }

        /// <summary>
        /// Populates the specified request object with the appropriate headers from the {@link ObjectMetadata} object.
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <param name="metadata"></param>
        /// <param name="request"></param>
        private static void PopulateRequestMetadata<X>(ObjectMetadata metadata, IRequest<X> request) where X : KS3Request
        {
            foreach (var kv in metadata.Metadata)
            {
                request.SetHeader(kv.Key, kv.Value.ToString());
            }

            foreach (var kv in metadata.UserMetadata)
            {
                request.SetHeader(Headers.KS3_USER_METADATA_PREFIX + kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// Adds the specified date header in RFC 822 date format to the specified request.
        /// This method will not add a date header if the specified date value is <code>null</code>.
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <param name="request"></param>
        /// <param name="header"></param>
        /// <param name="value"></param>
        private static void AddDateHeader<X>(IRequest<X> request, string header, DateTime? value)
        {
            if (value != null)
            {
                request.SetHeader(header, SignerUtils.GetSignatrueDate(value.Value));
            }
        }

        /// <summary>
        /// Adds the specified string list header, joined together separated with commas, to the specified request. 
        /// This method will not add a string list header if the specified values are <code>null</code> or empty.
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <param name="request"></param>
        /// <param name="header"></param>
        /// <param name="values"></param>
        private static void AddstringListHeader<X>(IRequest<X> request, string header, List<string> values)
        {
            if (values != null && values.Count > 0)
            {
                request.SetHeader(header, string.Join(", ", values));
            }
        }
        private static string FilterSpecial(string key)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                key = key.Replace("%5C", "/").Replace("//", "/%2F");
            }
            return key;
        }

        /// <summary>
        /// Sets the acccess control headers for the request given.
        /// </summary>
        /// <typeparam name="X"></typeparam>
        /// <param name="request"></param>
        /// <param name="acl"></param>
        private static void AddAclHeaders<X>(IRequest<X> request, AccessControlList acl) where X : KS3Request
        {
            ISet<Grant> grants = acl.GetGrants();
            IDictionary<string, IList<IGrantee>> grantsByPermission = new Dictionary<string, IList<IGrantee>>();
            foreach (Grant grant in grants)
            {
                if (!grantsByPermission.ContainsKey(grant.Permission))
                {
                    grantsByPermission[grant.Permission] = new List<IGrantee>();
                }
                grantsByPermission[grant.Permission].Add(grant.Grantee);
            }
            foreach (string permission in Permission.ListPermissions())
            {
                if (grantsByPermission.ContainsKey(permission))
                {
                    IList<IGrantee> grantees = grantsByPermission[permission];
                    bool first = true;
                    StringBuilder granteestring = new StringBuilder();
                    foreach (IGrantee grantee in grantees)
                    {
                        if (first)
                            first = false;
                        else
                            granteestring.Append(", ");
                        granteestring.Append(grantee.GetTypeIdentifier() + "=\"" + grantee.GetIdentifier() + "\"");
                    }
                    request.SetHeader(Permission.GetHeaderName(permission), granteestring.ToString());
                }
            }
        } // end of addAclHeader
    } // end of class KS3Client
}
