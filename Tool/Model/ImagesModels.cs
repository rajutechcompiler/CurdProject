
using System.Runtime.Serialization;

namespace TABFusionRMS.Web.Tool.Model.Images
{
    // Images/
    // *    GET GetThumbnailsGrid
    // 


    [DataContract]
    public partial class GetThumbnailsGridRequest : Request
    {
        /// <summary>
        /// The document to get thumbnails from.
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
        /// The first page number to use.
        /// </summary>
        [DataMember(Name = "firstPageNumber")]
        public int FirstPageNumber
        {
            get
            {
                return m_FirstPageNumber;
            }
            set
            {
                m_FirstPageNumber = value;
            }
        }
        private int m_FirstPageNumber;

        /// <summary>
        /// The last page number to use.
        /// </summary>
        [DataMember(Name = "lastPageNumber")]
        public int LastPageNumber
        {
            get
            {
                return m_LastPageNumber;
            }
            set
            {
                m_LastPageNumber = value;
            }
        }
        private int m_LastPageNumber;

        /// <summary>
        /// The mimetype to use for the images.
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
        /// The maximum width for the grid.
        /// </summary>
        [DataMember(Name = "maximumGridWidth")]
        public int MaximumGridWidth
        {
            get
            {
                return m_MaximumGridWidth;
            }
            set
            {
                m_MaximumGridWidth = value;
            }
        }
        private int m_MaximumGridWidth;

        /// <summary>
        /// The actual width for the grid.
        /// </summary>
        [DataMember(Name = "width")]
        public int Width
        {
            get
            {
                return m_Width;
            }
            set
            {
                m_Width = value;
            }
        }
        private int m_Width;

        /// <summary>
        /// The actual height for the grid.
        /// </summary>
        [DataMember(Name = "height")]
        public int Height
        {
            get
            {
                return m_Height;
            }
            set
            {
                m_Height = value;
            }
        }
        private int m_Height;
    }
}