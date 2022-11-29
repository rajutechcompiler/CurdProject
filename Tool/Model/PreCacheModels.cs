using System;
using System.Runtime.Serialization;
using Leadtools.Documents;

namespace TABFusionRMS.Web.Tool.Model.PreCache
{
    [DataContract]
    public partial class PreCacheDocumentRequest : Request
    {
        /// <summary>
        /// The URI to the document to be pre-cached
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
        /// The date this document will be expired. If null, then the document will never expire.
        /// </summary>
        [DataMember(Name = "expiryDate")]
        public DateTime ExpiryDate
        {
            get
            {
                return m_ExpiryDate;
            }
            set
            {
                m_ExpiryDate = value;
            }
        }
        private DateTime m_ExpiryDate;

        /// <summary>
        /// What part of the document to pre-cache. If None (default value), then uses DocumentCacheOptions.All
        /// </summary>
        [DataMember(Name = "cacheOptions")]
        public DocumentCacheOptions CacheOptions
        {
            get
            {
                return m_CacheOptions;
            }
            set
            {
                m_CacheOptions = value;
            }
        }
        private DocumentCacheOptions m_CacheOptions;

        /// <summary>
        /// The maximum pixel size values to use when pre-caching images. If null, then uses 4096 and 2048
        /// </summary>
        [DataMember(Name = "maximumImagePixelSizes")]
        public int[] MaximumImagePixelSizes
        {
            get
            {
                return m_MaximumImagePixelSizes;
            }
            set
            {
                m_MaximumImagePixelSizes = value;
            }
        }
        private int[] m_MaximumImagePixelSizes;

        /// <summary>
        /// A simple passcode to restrict others from pre-caching.
        /// </summary>
        [DataMember(Name = "passcode")]
        public string Passcode
        {
            get
            {
                return m_Passcode;
            }
            set
            {
                m_Passcode = value;
            }
        }
        private string m_Passcode;
    }

    [DataContract]
    public partial class PreCacheDocumentResponse : Response
    {
        /// <summary>
        /// The pre-cache response item.
        /// </summary>
        [DataMember(Name = "item")]
        public PreCacheResponseItem Item
        {
            get
            {
                return m_Item;
            }
            set
            {
                m_Item = value;
            }
        }
        private PreCacheResponseItem m_Item;
    }

    [DataContract]
    public partial class ReportPreCacheRequest : Request
    {
        /// <summary>
        /// Choose whether to remove pre-cache entries that don't have a matching cache entry.
        /// </summary>
        [DataMember(Name = "clean")]
        public bool Clean
        {
            get
            {
                return m_Clean;
            }
            set
            {
                m_Clean = value;
            }
        }
        private bool m_Clean;

        /// <summary>
        /// A simple passcode to restrict others from receiving a pre-cache report.
        /// </summary>
        [DataMember(Name = "passcode")]
        public string Passcode
        {
            get
            {
                return m_Passcode;
            }
            set
            {
                m_Passcode = value;
            }
        }
        private string m_Passcode;
    }

    [DataContract]
    public partial class ReportPreCacheResponse : Response
    {
        /// <summary>
        /// An array of the pre-cache entries stored in the pre-cache.
        /// </summary>
        [DataMember(Name = "entries")]
        public PreCacheResponseItem[] Entries
        {
            get
            {
                return m_Entries;
            }
            set
            {
                m_Entries = value;
            }
        }
        private PreCacheResponseItem[] m_Entries;

        /// <summary>
        /// An array of the pre-cache document items that were removed.
        /// </summary>
        [DataMember(Name = "removed")]
        public PreCacheResponseItem[] Removed
        {
            get
            {
                return m_Removed;
            }
            set
            {
                m_Removed = value;
            }
        }
        private PreCacheResponseItem[] m_Removed;
    }

    [DataContract]
    public partial class PreCacheResponseItem
    {
        /// <summary>
        /// The value of the URI for the pre-cache document, as a string.
        /// </summary>
        [DataMember(Name = "uri")]
        public string Uri
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
        private string m_Uri;

        /// <summary>
        /// The pre-cached items.
        /// </summary>
        [DataMember(Name = "items")]
        public PreCacheResponseSizeItem[] Items
        {
            get
            {
                return m_Items;
            }
            set
            {
                m_Items = value;
            }
        }
        private PreCacheResponseSizeItem[] m_Items;

        /// <summary>
        /// The mapped hashkey of the pre-cache entry from the request URI.
        /// </summary>
        [DataMember(Name = "hashKey")]
        public string RegionHash
        {
            get
            {
                return m_RegionHash;
            }
            set
            {
                m_RegionHash = value;
            }
        }
        private string m_RegionHash;
    }

    [DataContract]
    public partial class PreCacheResponseSizeItem
    {
        /// <summary>
        /// The mapped documentId of the cached document.
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
        /// The value of MaximumImagePixelSize
        /// </summary>
        [DataMember(Name = "maximumImagePixelSize")]
        public int MaximumImagePixelSize
        {
            get
            {
                return m_MaximumImagePixelSize;
            }
            set
            {
                m_MaximumImagePixelSize = value;
            }
        }
        private int m_MaximumImagePixelSize;

        /// <summary>
        /// The time it took to pre-cache the document, if relevant to the operation.
        /// Else it will be zero.
        /// </summary>
        [DataMember(Name = "seconds")]
        public double Seconds
        {
            get
            {
                return m_Seconds;
            }
            set
            {
                m_Seconds = value;
            }
        }
        private double m_Seconds;

        /// <summary>
        /// The times this item has been accessed, if relevant to the operation.
        /// Else it will be zero.
        /// </summary>
        [DataMember(Name = "reads")]
        public int Reads
        {
            get
            {
                return m_Reads;
            }
            set
            {
                m_Reads = value;
            }
        }
        private int m_Reads;
    }

    [Serializable]
    [DataContract]
    public partial class PreCacheEntry
    {
        [DataMember(Name = "documentId")]
        public string documentId
        {
            get
            {
                return m_documentId;
            }
            set
            {
                m_documentId = value;
            }
        }
        private string m_documentId;

        [DataMember(Name = "maximumImagePixelSize")]
        public int maximumImagePixelSize
        {
            get
            {
                return m_maximumImagePixelSize;
            }
            set
            {
                m_maximumImagePixelSize = value;
            }
        }
        private int m_maximumImagePixelSize;

        [DataMember(Name = "reads")]
        public int reads
        {
            get
            {
                return m_reads;
            }
            set
            {
                m_reads = value;
            }
        }
        private int m_reads;
    }
}