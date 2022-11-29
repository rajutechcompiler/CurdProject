using System;
using System.Runtime.Serialization;

namespace TABFusionRMS.Web.Tool.Model.Document
{
    [DataContract]
    public class DecryptRequest : Model.Request
    {
        /// <summary>
        /// The document's identification number.
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
        /// The attempted password for the document.
        /// </summary>
        [DataMember(Name = "password")]
        public string Password
        {
            get
            {
                return m_Password;
            }
            set
            {
                m_Password = value;
            }
        }
        private string m_Password;
    }
    [DataContract]
    public class DecryptResponse : Model.Response
    {
        /// <summary>
        /// The decrypted document's information.
        /// </summary>
        [DataMember(Name = "document")]
        public Leadtools.Documents.Document Document
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
        private Leadtools.Documents.Document m_Document;
    }

    [DataContract]
    public class ConvertRequest : Model.Request
    {
        /// <summary>
        /// The ID of the document to convert.
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
        /// Teh relevant job data that will be used to understand the conversion needed.
        /// </summary>
        [DataMember(Name = "jobData")]
        public Tool.Helpers.DocumentConverterJobData JobData
        {
            get
            {
                return m_JobData;
            }
            set
            {
                m_JobData = value;
            }
        }
        private Tool.Helpers.DocumentConverterJobData m_JobData;
    }

    [DataContract]
    public class ConvertItem
    {
        /// <summary>
        /// The user-friendly name of the item being returned, for downloading.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }
        private string m_Name;

        /// <summary>
        /// The URL of the item being returned, for downloading.
        /// </summary>
        [DataMember(Name = "url")]
        public Uri Url
        {
            get
            {
                return m_Url;
            }
            set
            {
                m_Url = value;
            }
        }
        private Uri m_Url;

        /// <summary>
        /// The mimetype of the converted item.
        /// </summary>
        [DataMember(Name = "mimeType")]
        public string MimeType
        {
            get
            {
                return m_MimeType;
            }
            set
            {
                m_MimeType = value;
            }
        }
        private string m_MimeType;

        /// <summary>
        /// The file length, in bytes, of the converted item.
        /// </summary>
        [DataMember(Name = "length")]
        public long Length
        {
            get
            {
                return m_Length;
            }
            set
            {
                m_Length = value;
            }
        }
        private long m_Length;
    }

    [DataContract]
    public class ConvertResponse : Model.Response
    {
        /// <summary>
        /// If the item had to be served as one archived file, it exists here.
        /// </summary>
        [DataMember(Name = "archive")]
        public ConvertItem Archive
        {
            get
            {
                return m_Archive;
            }
            set
            {
                m_Archive = value;
            }
        }
        private ConvertItem m_Archive;

        /// <summary>
        /// The newly converted document, if it was not archived.
        /// </summary>
        [DataMember(Name = "document")]
        public ConvertItem Document
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
        private ConvertItem m_Document;

        /// <summary>
        /// The converted annotations, if not archived.
        /// </summary>
        [DataMember(Name = "annotations")]
        public ConvertItem Annotations
        {
            get
            {
                return m_Annotations;
            }
            set
            {
                m_Annotations = value;
            }
        }
        private ConvertItem m_Annotations;
    }
}