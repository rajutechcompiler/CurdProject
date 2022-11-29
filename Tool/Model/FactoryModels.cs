using System;
using System.Runtime.Serialization;
using Leadtools.Caching;
using Leadtools.Documents;
using TABFusionRMS.Web.Tool.Model;
using ltDocument = Leadtools.Documents.Document;

namespace TabFusionRMS.WebVB.TABFusionRMS.Web.Tool.Model.Factory
{

    // Factory/
    // *    GET LoadFromCache
    // *    PUT LoadFromUri
    // *    GET BeginUpload
    // *    POST UploadDocument
    // *    GET AbortUploadDocument
    // *    POST SaveToCache
    // *    GET Delete
    // *    GET PurgeCache
    // 


    [DataContract]
    public class LoadFromCacheRequest : Request
    {
        /// <summary>
        /// The ID to load from the cache (which must be retrieved from an item after LoadFromUri was called, and saved).
        /// </summary>
        [DataMember(Name = "documentId")]
        public string DocumentId
        {
            get
            {
                return m_DocumentId;
            }
            set
            {
                m_DocumentId = value;
            }
        }
        private string m_DocumentId;
    }

    [DataContract]
    public class LoadFromCacheResponse : Response
    {
        /// <summary>
        /// The LEADTOOLS Document info.
        /// </summary>
        [DataMember(Name = "document")]
        public ltDocument Document
        {
            get
            {
                return m_Document;
            }
            set
            {
                m_Document = value;
            }
        }
        private ltDocument m_Document;
    }

    [DataContract]
    public class LoadFromUriRequest : Request
    {
        /// <summary>
        /// The options to use when loading this document.
        /// </summary>
        [DataMember(Name = "options")]
        public LoadDocumentOptions Options
        {
            get
            {
                return m_Options;
            }
            set
            {
                m_Options = value;
            }
        }
        private LoadDocumentOptions m_Options;

        /// <summary>
        /// The URI to the document to be loaded.
        /// </summary>
        [DataMember(Name = "uri")]
        public Uri Uri
        {
            get
            {
                return m_Uri;
            }
            set
            {
                m_Uri = value;
            }
        }
        private Uri m_Uri;

        // Should eventually be removed, as it exists in LoadDocumentOptions in JS but not in .NET
        [DataMember(Name = "resolution")]
        public int Resolution
        {
            get
            {
                return m_Resolution;
            }
            set
            {
                m_Resolution = value;
            }
        }
        private int m_Resolution;
    }

    [DataContract]
    public class LoadFromUriResponse : Response
    {
        /// <summary>
        /// The loaded document.
        /// </summary>
        [DataMember(Name = "document")]
        public ltDocument Document
        {
            get
            {
                return m_Document;
            }
            set
            {
                m_Document = value;
            }
        }
        private ltDocument m_Document;
    }

    [DataContract]
    public class BeginUploadRequest : Request
    {
        /// <summary>
        /// The ID to use for the new document, or null to create a new random DocumentId.
        /// </summary>
        [DataMember(Name = "documentId")]
        public string DocumentId
        {
            get
            {
                return m_DocumentId;
            }
            set
            {
                m_DocumentId = value;
            }
        }
        private string m_DocumentId;
    }

    [DataContract]
    public class BeginUploadResponse : Response
    {
        /// <summary>
        /// The URI to which the document should be uploaded to the cache.
        /// </summary>
        [DataMember(Name = "uploadUri")]
        public Uri UploadUri
        {
            get
            {
                return m_UploadUri;
            }
            set
            {
                m_UploadUri = value;
            }
        }
        private Uri m_UploadUri;
    }

    [DataContract]
    public class UploadDocumentRequest : Request
    {
        /// <summary>
        /// The uri, retrieved from BeginUpload, that is used for uploading.
        /// </summary>
        [DataMember(Name = "uri")]
        public Uri Uri
        {
            get
            {
                return m_Uri;
            }
            set
            {
                m_Uri = value;
            }
        }
        private Uri m_Uri;

        /// <summary>
        /// The data to upload.
        /// </summary>
        [DataMember(Name = "data")]
        public string Data
        {
            get
            {
                return m_Data;
            }
            set
            {
                m_Data = value;
            }
        }
        private string m_Data;
    }

    [DataContract]
    public class AbortUploadDocumentRequest : Request
    {
        /// <summary>
        /// The URI from BeginUpload to stop loading to.
        /// </summary>
        [DataMember(Name = "uri")]
        public Uri Uri
        {
            get
            {
                return m_Uri;
            }
            set
            {
                m_Uri = value;
            }
        }
        private Uri m_Uri;
    }

    [DataContract]
    public class GetCacheStatisticsResponse : Response
    {
        /// <summary>
        /// The cache statistics information.
        /// </summary>
        [DataMember(Name = "statistics")]
        public CacheStatistics Statistics
        {
            get
            {
                return m_Statistics;
            }
            set
            {
                m_Statistics = value;
            }
        }
        private CacheStatistics m_Statistics;
    }

    [DataContract]
    public class SaveToCacheRequest : Request
    {
        /// <summary>
        /// The data to use when creating or saving this document.
        /// </summary>
        [DataMember(Name = "descriptor")]
        public DocumentDescriptor Descriptor
        {
            get
            {
                return m_Descriptor;
            }
            set
            {
                m_Descriptor = value;
            }
        }
        private DocumentDescriptor m_Descriptor;
    }

    [DataContract]
    public class SaveToCacheResponse : Response
    {
        /// <summary>
        /// The loaded or updated document.
        /// </summary>
        [DataMember(Name = "document")]
        public ltDocument Document
        {
            get
            {
                return m_Document;
            }
            set
            {
                m_Document = value;
            }
        }
        private ltDocument m_Document;
    }

    [DataContract]
    public class DeleteRequest : Request
    {
        /// <summary>
        /// The document to delete.
        /// </summary>
        [DataMember(Name = "documentId")]
        public string DocumentId
        {
            get
            {
                return m_DocumentId;
            }
            set
            {
                m_DocumentId = value;
            }
        }
        private string m_DocumentId;
    }

    [DataContract]
    public class DownloadRequest : Request
    {
        // Either this is not null or the other
        /// <summary>
        /// The ID of the document in the cache to download. Cannot be used if URI is used.
        /// </summary>
        [DataMember(Name = "documentId")]
        public string DocumentId
        {
            get
            {
                return m_DocumentId;
            }
            set
            {
                m_DocumentId = value;
            }
        }
        private string m_DocumentId;

        /// <summary>
        /// The URI to the document to download. Cannot be used if ID is used.
        /// </summary>
        [DataMember(Name = "uri")]
        public Uri Uri
        {
            get
            {
                return m_Uri;
            }
            set
            {
                m_Uri = value;
            }
        }
        private Uri m_Uri;

        /// <summary>
        /// The position to copy data from.
        /// </summary>
        [DataMember(Name = "position")]
        public long Position
        {
            get
            {
                return m_Position;
            }
            set
            {
                m_Position = value;
            }
        }
        private long m_Position;

        /// <summary>
        /// The size of the data to return.
        /// </summary>
        [DataMember(Name = "dataSize")]
        public int DataSize
        {
            get
            {
                return m_DataSize;
            }
            set
            {
                m_DataSize = value;
            }
        }
        private int m_DataSize;
    }

    [DataContract]
    public class DownloadResponse : Response
    {
        /// <summary>
        /// The downloaded document data.
        /// </summary>
        [DataMember(Name = "data")]
        public string Data
        {
            get
            {
                return m_Data;
            }
            set
            {
                m_Data = value;
            }
        }
        private string m_Data;
    }

}