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

        /** KS3 credentials. */
        private IKS3Credentials _ks3Credentials;

        /** The service endpoint to which this client will send requests. */
        private Uri endpoint;

        /** The client configuration */
        private ClientConfiguration _clientConfiguration;

        /** Low level client for sending requests to KS3. */
        private KS3HttpClient _client;

        /** Optional offset (in seconds) to use when signing requests */
        private int timeOffset;

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
                endpoint = _clientConfiguration.getProtocol() + "://" + endpoint;
            this.endpoint = new Uri(endpoint);
        }

        public void SetConfiguration(ClientConfiguration clientConfiguration)
        {
            this._clientConfiguration = clientConfiguration;
            _client = new KS3HttpClient(clientConfiguration);
        }

        /**
         * Sets the optional value for time offset for this client.  This
         * value will be applied to all requests processed through this client.
         * Value is in seconds, positive values imply the current clock is "fast",
         * negative values imply clock is slow.
         */
        public void setTimeOffset(int timeOffset)
        {
            this.timeOffset = timeOffset;
        }

        /**
         * Returns the optional value for time offset for this client.  This
         * value will be applied to all requests processed through this client.
         * Value is in seconds, positive values imply the current clock is "fast",
         * negative values imply clock is slow.
         */
        public int GetTimeOffset()
        {
            return timeOffset;
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

        /**
         * Gets the AccessControlList (ACL) for the specified KS3 bucket.
         */
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
                addAclHeaders(request, createBucketRequest.Acl);
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

        /**
         * Sets the AccessControlList for the specified KS3 bucket.
         */
        public void SetBucketAcl(SetBucketAclRequest setBucketAclRequest)
        {
            string bucketName = setBucketAclRequest.getBucketName();
            AccessControlList acl = setBucketAclRequest.getAcl();
            CannedAccessControlList cannedAcl = setBucketAclRequest.getCannedAcl();

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
            request.SetHeader(Headers.CONTENT_LENGTH, putBucketLoggingRequest.toXmlAdapter().Length.ToString());
            request.SetHeader(Headers.CONTENT_TYPE, "application/xml");
            request.SetContent(putBucketLoggingRequest.toXmlAdapter());
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

        /**
         * Returns a list of summary information about the objects in the specified bucket.
         */
        public ObjectListing ListObjects(string bucketName, string prefix)
        {
            return this.ListObjects(new ListObjectsRequest(bucketName, prefix, null, null, null));
        }

        /**
         * Returns a list of summary information about the objects in the specified bucket.
         */
        public ObjectListing ListObjects(ListObjectsRequest listObjectRequest)
        {
            string bucketName = listObjectRequest.getBucketName();
            IRequest<ListObjectsRequest> request = this.CreateRequest(bucketName, null, listObjectRequest, HttpMethod.GET);

            if (listObjectRequest.getPrefix() != null)
                request.SetParameter("prefix", listObjectRequest.getPrefix());
            if (listObjectRequest.getMarker() != null)
                request.SetParameter("marker", listObjectRequest.getMarker());
            if (listObjectRequest.getDelimiter() != null)
                request.SetParameter("delimiter", listObjectRequest.getDelimiter());
            if (listObjectRequest.getMaxKeys() != null && listObjectRequest.getMaxKeys() >= 0)
                request.SetParameter("max-keys", listObjectRequest.getMaxKeys().ToString());

            return this.Invoke(request, new ListObjectsUnmarshallers(), bucketName, null);
        }

        /**
         * Deletes the specified object in the specified bucket.
         */
        public void DeleteObject(string bucketName, string key)
        {
            this.DeleteObject(new DeleteObjectRequest(bucketName, key));
        }

        /**
         * Deletes the specified object in the specified bucket.
         */
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

            addDateHeader(request, Headers.GET_OBJECT_IF_MODIFIED_SINCE, getObjectRequest.ModifiedSinceConstraint);
            addDateHeader(request, Headers.GET_OBJECT_IF_UNMODIFIED_SINCE, getObjectRequest.UnmodifiedSinceConstraint);
            addstringListHeader(request, Headers.GET_OBJECT_IF_MATCH, getObjectRequest.MatchingETagConstraints);
            addstringListHeader(request, Headers.GET_OBJECT_IF_NONE_MATCH, getObjectRequest.NonmatchingETagContraints);

            IProgressListener progressListener = getObjectRequest.ProgressListener;

            fireProgressEvent(progressListener, ProgressEvent.STARTED);

            KS3Object ks3Object = null;
            try
            {
                ks3Object = this.Invoke(request, new ObjectResponseHandler(getObjectRequest), bucketName, key);
            }
            catch (ProgressInterruptedException e)
            {
                fireProgressEvent(progressListener, ProgressEvent.CANCELED);
                throw e;
            }
            catch (Exception e)
            {
                fireProgressEvent(progressListener, ProgressEvent.FAILED);
                throw e;
            }
            fireProgressEvent(progressListener, ProgressEvent.COMPLETED);

            ks3Object.setBucketName(bucketName);
            ks3Object.setKey(key);

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

        /**
         * Uploads the specified file to KS3 under the specified bucket and key name.
         */
        public PutObjectResult PutObject(string bucketName, string key, FileInfo file)
        {
            PutObjectRequest putObjectRequest = new PutObjectRequest(bucketName, key, file);
            putObjectRequest.setMetadata(new ObjectMetadata());
            return this.PutObject(putObjectRequest);
        }

        /**
         * Uploads the specified input stream and object metadata to KS3 under the specified bucket and key name. 
         */
        public PutObjectResult PutObject(string bucketName, string key, Stream input, ObjectMetadata metadata)
        {
            return this.PutObject(new PutObjectRequest(bucketName, key, input, metadata));
        }

        /**
         * Uploads a new object to the specified KS3 bucket.
         */
        public PutObjectResult PutObject(PutObjectRequest putObjectRequest)
        {
            string bucketName = putObjectRequest.getBucketName();
            string key = putObjectRequest.getKey();
            ObjectMetadata metadata = putObjectRequest.getMetadata();
            Stream input = putObjectRequest.getInputStream();
            IProgressListener progressListener = putObjectRequest.getProgressListener();
            if (metadata == null)
                metadata = new ObjectMetadata();

            // If a file is specified for upload, we need to pull some additional
            // information from it to auto-configure a few options
            if (putObjectRequest.getFile() != null)
            {
                FileInfo file = putObjectRequest.getFile();

                // Always set the content length, even if it's already set
                metadata.setContentLength(file.Length);

                // Only set the content type if it hasn't already been set
                if (metadata.getContentType() == null)
                    metadata.setContentType(Mimetypes.GetMimetype(file));

                if (metadata.getContentMD5() == null)
                {
                    using (FileStream fileStream = file.OpenRead())
                    {
                        MD5 md5 = MD5.Create();
                        metadata.setContentMD5(Convert.ToBase64String(md5.ComputeHash(fileStream)));
                    }
                }

                input = file.OpenRead();
            }
            else
            {
                metadata.setContentLength(input.Length);

                if (metadata.getContentType() == null)
                    metadata.setContentType(Mimetypes.DEFAULT_MIMETYPE);
                if (metadata.getContentMD5() == null)
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        metadata.setContentMD5(Convert.ToBase64String(md5.ComputeHash(input)));
                    }

                    input.Seek(0, SeekOrigin.Begin); // It is needed after calculated MD5.
                }
            }

            IRequest<PutObjectRequest> request = this.CreateRequest(bucketName, key, putObjectRequest, HttpMethod.PUT);

            if (putObjectRequest.getAcl() != null)
                addAclHeaders(request, putObjectRequest.getAcl());
            else if (putObjectRequest.getCannedAcl() != null)
                request.SetHeader(Headers.KS3_CANNED_ACL, putObjectRequest.getCannedAcl().GetCannedAclHeader());

            if (progressListener != null)
            {
                input = new ProgressReportingInputStream(input, progressListener);
                fireProgressEvent(progressListener, ProgressEvent.STARTED);
            }

            populateRequestMetadata(metadata, request);
            request.SetContent(input);

            //-----------------------------------------------

            ObjectMetadata returnedMetadata = null;
            try
            {
                returnedMetadata = this.Invoke(request, new MetadataResponseHandler(), bucketName, key);
            }
            catch (ProgressInterruptedException e)
            {
                fireProgressEvent(progressListener, ProgressEvent.CANCELED);
                throw e;
            }
            catch (Exception e)
            {
                fireProgressEvent(progressListener, ProgressEvent.FAILED);
                throw e;
            }
            finally
            {
                if (input != null)
                    input.Close();
            }

            fireProgressEvent(progressListener, ProgressEvent.COMPLETED);

            PutObjectResult result = new PutObjectResult();
            result.setETag(returnedMetadata.getETag());
            result.setContentMD5(metadata.getContentMD5());

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
                addAclHeaders(request, copyObjectRequest.AccessControlList);
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
            if (!headObjectRequest.ModifiedSinceConstraint.Equals(DateTime.MinValue))
            {
                request.GetHeaders().Add(Headers.GET_OBJECT_IF_MODIFIED_SINCE, headObjectRequest.ModifiedSinceConstraint.ToUniversalTime().ToString("r"));
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
            IRequest<InitiateMultipartUploadRequest> request = this.CreateRequest(param.Bucketname, param.Objectkey, param, HttpMethod.POST);
            request.SetParameter("uploads", null);
            request.GetHeaders()[Headers.CONTENT_LENGTH] = "0";
            InitiateMultipartUploadResult result = new InitiateMultipartUploadResult();
            result = this.Invoke(request, new MultipartUploadResultUnmarshaller(), param.Bucketname, param.Objectkey);
            return result;
        }
        /**
         * upload multi file by part
         * **/
        public PartETag uploadPart(UploadPartRequest param)
        {
            string bucketName = param.getBucketname();
            string key = param.getObjectkey();
            ObjectMetadata metadata = param.getMetadata();
            Stream input = param.getInputStream();
            IProgressListener progressListener = param.getProgressListener();

            if (metadata == null)
                metadata = new ObjectMetadata();

            // If a file is specified for upload, we need to pull some additional
            // information from it to auto-configure a few options
            metadata.setContentLength(input.Length);

            if (metadata.getContentType() == null)
                metadata.setContentType(Mimetypes.DEFAULT_MIMETYPE);
            if (metadata.getContentMD5() == null)
            {
                using (MD5 md5 = MD5.Create())
                {
                    metadata.setContentMD5(Convert.ToBase64String(md5.ComputeHash(input)));
                }

                input.Seek(0, SeekOrigin.Begin); // It is needed after calculated MD5.
            }

            IRequest<UploadPartRequest> request = this.CreateRequest(param.getBucketname(), param.getObjectkey(), param, HttpMethod.PUT);
            request.SetParameter("partNumber", param.getPartNumber().ToString());
            request.SetParameter("uploadId", param.getUploadId());



            if (progressListener != null)
            {
                input = new ProgressReportingInputStream(input, progressListener);
                fireProgressEvent(progressListener, ProgressEvent.STARTED);
            }

            populateRequestMetadata(metadata, request);
            request.SetContent(input);

            //-----------------------------------------------

            ObjectMetadata returnedMetadata = null;
            try
            {
                returnedMetadata = this.Invoke(request, new MetadataResponseHandler(), bucketName, key);
            }
            catch (ProgressInterruptedException e)
            {
                fireProgressEvent(progressListener, ProgressEvent.CANCELED);
                throw e;
            }
            catch (Exception e)
            {
                fireProgressEvent(progressListener, ProgressEvent.FAILED);
                throw e;
            }
            finally
            {
                if (input != null)
                    input.Close();
            }

            fireProgressEvent(progressListener, ProgressEvent.COMPLETED);

            PartETag result = new PartETag(param.getPartNumber(), returnedMetadata.getETag());

            return result;
        }
        /**
         * getlist had uploaded part list
         * **/
        public ListMultipartUploadsResult getListMultipartUploads(ListMultipartUploadsRequest param)
        {
            IRequest<ListMultipartUploadsRequest> request = this.CreateRequest(param.getBucketname(), param.getObjectkey(), param, HttpMethod.GET);
            request.SetParameter("uploadId", param.getUploadId());
            request.GetHeaders()[Headers.CONTENT_LENGTH] = "0";
            ListMultipartUploadsResult result = new ListMultipartUploadsResult();
            result = this.Invoke(request, new ListMultipartUploadsResultUnmarshaller(), param.getBucketname(), param.getObjectkey());
            return result;
        }
        /**
         * submit the all part,the server will complete join part
         * **/
        public CompleteMultipartUploadResult completeMultipartUpload(CompleteMultipartUploadRequest param)
        {
            IRequest<CompleteMultipartUploadRequest> request = this.CreateRequest(param.BucketName, param.ObjectKey, param, HttpMethod.POST);
            request.SetParameter("uploadId", param.UploadId);
            request.SetHeader(Headers.CONTENT_LENGTH, param.Content.Length.ToString());
            request.SetContent(param.Content);
            CompleteMultipartUploadResult result = new CompleteMultipartUploadResult();
            result = Invoke(request, new CompleteMultipartUploadResultUnmarshaller(), param.BucketName, param.ObjectKey);
            return result;
        }
        /**
         * abort the upload opertion by uploadid
         * **/
        public void AbortMultipartUpload(AbortMultipartUploadRequest param)
        {
            IRequest<AbortMultipartUploadRequest> request = this.CreateRequest(param.BucketName, param.ObjectKey, param, HttpMethod.DELETE);
            request.SetParameter("uploadId", param.UploadId);
            request.GetHeaders()[Headers.CONTENT_LENGTH] = "0";
            this.Invoke(request, voidResponseHandler, param.BucketName, param.ObjectKey);
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

        /**
         * Sets the AccessControlList for the specified object in KS3.
         */
        public void SetObjectAcl(SetObjectAclRequest setObjectAclRequest)
        {
            string bucketName = setObjectAclRequest.getBucketName();
            string key = setObjectAclRequest.getKey();
            AccessControlList acl = setObjectAclRequest.getAcl();
            CannedAccessControlList cannedAcl = setObjectAclRequest.getCannedAcl();

            IRequest<SetObjectAclRequest> request = this.CreateRequest(bucketName, key, setObjectAclRequest, HttpMethod.PUT);

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

            this.Invoke(request, this.voidResponseHandler, bucketName, key);
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
            return this.generatePresignedUrl(bucketName, key, expiration, null);
        }
        /// <summary>
        /// generate PresignedUrl the url can apply for other user
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="key"></param>
        /// <param name="expiration"></param>
        /// <param name="overrides"></param>
        /// <returns></returns>
        public string generatePresignedUrl(string bucketName, string key, DateTime expiration, ResponseHeaderOverrides overrides)
        {
            string url = "";
            string param = "";

            overrides = overrides == null ? new ResponseHeaderOverrides() : overrides;
            if (!string.IsNullOrEmpty(overrides.CacheControl))
                param += "response-cache-control=" + overrides.CacheControl;
            if (!string.IsNullOrEmpty(overrides.ContentType))
                param += "&response-content-type=" + overrides.ContentType;
            if (!string.IsNullOrEmpty(overrides.ContentLanguage))
                param += "&response-content-language=" + overrides.ContentLanguage;
            if (!string.IsNullOrEmpty(overrides.Expires))
                param += "&response-expires=" + overrides.Expires;
            if (!string.IsNullOrEmpty(overrides.ContentDisposition))
                param += "&response-content-disposition=" + overrides.ContentDisposition;
            if (!string.IsNullOrEmpty(overrides.ContentEncoding))
                param += "&response-content-encoding=" + overrides.ContentEncoding;

            var baselineTime = new DateTime(1970, 1, 1);
            var expires = Convert.ToInt64((expiration.ToUniversalTime() - baselineTime).TotalSeconds);
            try
            {
                KS3Signer<NoneKS3Request> ks3Signer = createSigner<NoneKS3Request>(HttpMethod.GET.ToString(), bucketName, key);
                string signer = ks3Signer.GetSignature(this._ks3Credentials, expires.ToString());
                url += @"http://" + bucketName + "." + Constants.KS3_HOSTNAME
                             + "/" + filterSpecial(UrlEncoder.Encode(key, Constants.DEFAULT_ENCODING)) + "?AccessKeyId="
                             + UrlEncoder.Encode(this._ks3Credentials.GetKS3AccessKeyId(), Constants.DEFAULT_ENCODING)
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

            request.GetHeaders().Add(Headers.AsynchronousProcessingList, putAdpRequest.convertAdpsToString());
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
            request.SetEndpoint(endpoint);

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
            IDictionary<string, string> parameters = request.GetOriginalRequest().copyPrivateRequestParameters();
            foreach (string name in parameters.Keys)
                request.SetParameter(name, parameters[name]);

            request.SetTimeOffset(timeOffset);

            /**
             * The string we sign needs to include the exact headers that we
             * send with the request, but the client runtime layer adds the
             * Content-Type header before the request is sent if one isn't set, so
             * we have to set something here otherwise the request will fail.
             */
            if (!request.GetHeaders().ContainsKey(Headers.CONTENT_TYPE))
                request.SetHeader(Headers.CONTENT_TYPE, Mimetypes.DEFAULT_MIMETYPE);

            /**
             * Set the credentials which will be used by the KS3Signer later.
             */
            if (request.GetOriginalRequest().getRequestCredentials() == null)
                request.GetOriginalRequest().setRequestCredentials(this._ks3Credentials);

            return _client.Excute(request, responseHandler, createSigner(request, bucket, key));
        }

        private KS3Signer<T> createSigner<T>(IRequest<T> request, string bucketName, string key) where T : KS3Request
        {
            return createSigner<T>(request.GetHttpMethod().ToString(), bucketName, key);
        }
        private KS3Signer<T> createSigner<T>(string httpMethod, string bucketName, string key) where T : KS3Request
        {
            string resourcePath = "/" + (bucketName != null ? bucketName + "/" : "") + (key != null ? key : "");
            resourcePath = UrlEncoder.Encode(resourcePath, Constants.DEFAULT_ENCODING);
            return new KS3Signer<T>(httpMethod, resourcePath);
        }
        /**
         * Fires a progress event with the specified event type to the specified
         * listener.
         */
        private static void fireProgressEvent(IProgressListener listener, int eventType)
        {
            if (listener == null) return;

            ProgressEvent e = new ProgressEvent(eventType);
            listener.ProgressChanged(e);
        }

        /**
         * Populates the specified request object with the appropriate headers from
         * the {@link ObjectMetadata} object.
         */
        private static void populateRequestMetadata<X>(ObjectMetadata metadata, IRequest<X> request) where X : KS3Request
        {
            IDictionary<string, Object> rawMetadata = metadata.getRawMetadata();
            if (rawMetadata != null)
            {
                foreach (string name in rawMetadata.Keys)
                    request.SetHeader(name, rawMetadata[name].ToString());
            }

            IDictionary<string, string> userMetadata = metadata.getUserMetadata();
            if (userMetadata != null)
            {
                foreach (string name in userMetadata.Keys)
                    request.SetHeader(Headers.KS3_USER_METADATA_PREFIX + name, userMetadata[name]);
            }
        }

        /**
	     * Adds the specified date header in RFC 822 date format to the specified
	     * request. This method will not add a date header if the specified date
	     * value is <code>null</code>.
	     */
        private static void addDateHeader<X>(IRequest<X> request, string header, DateTime? value)
        {
            if (value != null)
                request.SetHeader(header, SignerUtils.GetSignatrueDate(value.Value));
        }

        /*
         * Adds the specified string list header, joined together separated with
         * commas, to the specified request.
         * This method will not add a string list header if the specified values
         * are <code>null</code> or empty.
         */
        private static void addstringListHeader<X>(IRequest<X> request, string header, IList<string> values)
        {
            if (values != null && values.Count > 0)
                request.SetHeader(header, string.Join(", ", values));
        }
        private static string filterSpecial(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                key = key.Replace("%5C", "/").Replace("//", "/%2F");
            }
            return key;
        }
        /**
         * Sets the acccess control headers for the request given.
         */
        private static void addAclHeaders<X>(IRequest<X> request, AccessControlList acl) where X : KS3Request
        {
            ISet<Grant> grants = acl.getGrants();
            IDictionary<string, IList<IGrantee>> grantsByPermission = new Dictionary<string, IList<IGrantee>>();
            foreach (Grant grant in grants)
            {
                if (!grantsByPermission.ContainsKey(grant.Permission))
                {
                    grantsByPermission[grant.Permission] = new List<IGrantee>();
                }
                grantsByPermission[grant.Permission].Add(grant.Grantee);
            }
            foreach (string permission in Permission.listPermissions())
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
                    request.SetHeader(Permission.getHeaderName(permission), granteestring.ToString());
                }
            }
        } // end of addAclHeader
    } // end of class KS3Client
}
