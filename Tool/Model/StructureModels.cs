using Leadtools.Documents;
using System.Runtime.Serialization;
using ltDocument = Leadtools.Documents.Document;

namespace TABFusionRMS.Web.Tool.Model.Structure
{
    // Structure/
    // *    GET ParseStructure
    // 


    [DataContract]
    public partial class ParseStructureRequest : Request
    {
        /// <summary>
        /// The document to parse.
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
        /// Whether or not to parse bookmarks from the document.
        /// </summary>
        [DataMember(Name = "parseBookmarks")]
        public bool ParseBookmarks
        {
            get
            {
                return m_ParseBookmarks;
            }
            set
            {
                m_ParseBookmarks = value;
            }
        }
        private bool m_ParseBookmarks;

        /// <summary>
        /// Whether or not to parse bookmarks from the document.
        /// </summary>
        [DataMember(Name = "parsePageLinks")]
        public bool ParsePageLinks
        {
            get
            {
                return m_ParsePageLinks;
            }
            set
            {
                m_ParsePageLinks = value;
            }
        }
        private bool m_ParsePageLinks;
    }

    [DataContract]
    public partial class ParseStructureResponse : Response
    {
        /// <summary>
        /// The bookmarks that exist for the document.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [DataMember(Name = "bookmarks")]
        public DocumentBookmark[] Bookmarks
        {
            get
            {
                return m_Bookmarks;
            }
            set
            {
                m_Bookmarks = value;
            }
        }
        private DocumentBookmark[] m_Bookmarks;

        /// <summary>
        /// The page links that exist on the document.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [DataMember(Name = "pageLinks")]
        public DocumentLink[][] PageLinks
        {
            get
            {
                return m_PageLinks;
            }
            set
            {
                m_PageLinks = value;
            }
        }
        private DocumentLink[][] m_PageLinks;
    }
}