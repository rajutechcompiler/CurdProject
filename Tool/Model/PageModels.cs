using System;
using System.Runtime.Serialization;
using Leadtools.Documents;
using TABFusionRMS.Web.Tool.Model;

namespace TabFusionRMS.Web.Tool.Model.Page
{
    // Page/
    // *    GET GetLinks
    // *    GET GetImage
    // *    GET GetText
    // *    GET GetSvg
    // *    GET GetSvgBackImage
    // *    GET GetThumbnail
    // *    GET GetText
    // *    GET ReadBarcodes
    // *    
    // *    == LINKED ==
    // *    POST SetAnnotations
    // *    GET GetAnnotations
    // 


    [DataContract]
    public class GetImageRequest : Request
    {
        /// <summary>
        /// The document to get the image from.
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
        /// The page from which to get the image.
        /// </summary>
        [DataMember(Name = "pageNumber")]
        public int PageNumber
        {
            get
            {
                return m_PageNumber;
            }
            set
            {
                m_PageNumber = value;
            }
        }
        private int m_PageNumber;

        /// <summary>
        /// The resolution of the image to use, if different from the default.
        /// </summary>
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

        /// <summary>
        /// The mimetype to load the image as.
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
        /// The bits per pixel value to use.
        /// </summary>
        [DataMember(Name = "bitsPerPixel")]
        public int BitsPerPixel
        {
            get
            {
                return m_BitsPerPixel;
            }
            set
            {
                m_BitsPerPixel = value;
            }
        }
        private int m_BitsPerPixel;

        /// <summary>
        /// The quality factor used for rendering the image.
        /// </summary>
        [DataMember(Name = "qualityFactor")]
        public int QualityFactor
        {
            get
            {
                return m_QualityFactor;
            }
            set
            {
                m_QualityFactor = value;
            }
        }
        private int m_QualityFactor;

        /// <summary>
        /// The width of the image to return.
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
        /// The height of the image to return.
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

    [DataContract]
    public class GetSvgBackImageRequest : Request
    {
        /// <summary>
        /// The document from which to load the back image.
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
        /// The page number to load the back image from.
        /// </summary>
        [DataMember(Name = "pageNumber")]
        public int PageNumber
        {
            get
            {
                return m_PageNumber;
            }
            set
            {
                m_PageNumber = value;
            }
        }
        private int m_PageNumber;

        /// <summary>
        /// The color to use as the background color, such as "transparent".
        /// </summary>
        [DataMember(Name = "backColor")]
        public string BackColor
        {
            get
            {
                return m_BackColor;
            }
            set
            {
                m_BackColor = value;
            }
        }
        private string m_BackColor;

        /// <summary>
        /// The resolution to use.
        /// </summary>
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

        /// <summary>
        /// The mimetype to load with.
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
        /// The bits per pixel to use.
        /// </summary>
        [DataMember(Name = "bitsPerPixel")]
        public int BitsPerPixel
        {
            get
            {
                return m_BitsPerPixel;
            }
            set
            {
                m_BitsPerPixel = value;
            }
        }
        private int m_BitsPerPixel;

        /// <summary>
        /// The quality factor to use.
        /// </summary>
        [DataMember(Name = "qualityFactor")]
        public int QualityFactor
        {
            get
            {
                return m_QualityFactor;
            }
            set
            {
                m_QualityFactor = value;
            }
        }
        private int m_QualityFactor;

        /// <summary>
        /// The width to use for the image.
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
        /// The height to use for the image.
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

    [DataContract]
    public class GetThumbnailRequest : Request
    {
        /// <summary>
        /// The ID of the document to get the thumbnail from.
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
        /// The page number of the thumbnail.
        /// </summary>
        [DataMember(Name = "pageNumber")]
        public int PageNumber
        {
            get
            {
                return m_PageNumber;
            }
            set
            {
                m_PageNumber = value;
            }
        }
        private int m_PageNumber;

        /// <summary>
        /// The mimetype to use for the thumbnail.
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
        /// The width to return for the thumbnail.
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
        /// The height to return for the thumbnail.
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

    [DataContract]
    [Flags]
    public enum DocumentGetSvgOptions
    {
        None = 0,
        /// <summary>
        /// Should polyline text be allowed?
        /// </summary>
        AllowPolylineText = 1 << 0,
        /// <summary>
        /// The created svg will have the text as paths. Default is the output file may have text as both text spans and path objects depending on the input document. Currently
        /// supported by PDF documents only
        /// </summary>
        ForceTextPath = 1 << 0,
        /// <summary>
        /// Should any images be dropped?
        /// </summary>
        DropImages = 1 << 1,
        /// <summary>
        /// Should any shapes be dropped?
        /// </summary>
        DropShapes = 1 << 2,
        /// <summary>
        /// Should any text be dropped?
        /// </summary>
        DropText = 1 << 3,
        /// <summary>
        /// Is this SVG for conversion?
        /// </summary>
        ForConversion = 1 << 4,
        /// <summary>
        /// Should XML parsing errors be ignored?
        /// </summary>
        IgnoreXmlParsingErrors = 1 << 5,
        /// <summary>
        /// The created svg will have the text as real text spans. Default is the output file may have text as both text spans and path objects depending on the input document.
        /// Currently supported by PDF documents only
        /// </summary>
        ForceRealText = 1 << 6
    }

    [DataContract]
    public class GetSvgRequest : Request
    {
        /// <summary>
        /// The ID of the document to load the SVG from.
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
        /// The page number from which to load the SVG.
        /// </summary>
        [DataMember(Name = "pageNumber")]
        public int PageNumber
        {
            get
            {
                return m_PageNumber;
            }
            set
            {
                m_PageNumber = value;
            }
        }
        private int m_PageNumber;

        /// <summary>
        /// SVG options
        /// </summary>
        [DataMember(Name = "options")]
        public DocumentGetSvgOptions Options
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
        private DocumentGetSvgOptions m_Options;

        /// <summary>
        /// Should images be unembeded?
        /// </summary>
        [DataMember(Name = "unembedImages")]
        public bool UnembedImages
        {
            get
            {
                return m_UnembedImages;
            }
            set
            {
                m_UnembedImages = value;
            }
        }
        private bool m_UnembedImages;
    }

    [DataContract]
    public class GetTextRequest : Request
    {
        /// <summary>
        /// The ID of the document to get text for.
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
        /// The number of the page text should be parsed from.
        /// </summary>
        [DataMember(Name = "pageNumber")]
        public int PageNumber
        {
            get
            {
                return m_PageNumber;
            }
            set
            {
                m_PageNumber = value;
            }
        }
        private int m_PageNumber;

        /// <summary>
        /// Clipping rectangle. Use LeadRectD.Empty to get the page for the whole page
        /// </summary>
        [DataMember(Name = "clip")]
        public Leadtools.LeadRectD Clip
        {
            get
            {
                return m_Clip;
            }
            set
            {
                m_Clip = value;
            }
        }
        private Leadtools.LeadRectD m_Clip;

        /// <summary>
        /// The type of text extraction to use.
        /// </summary>
        [DataMember(Name = "textExtractionMode")]
        public DocumentTextExtractionMode TextExtractionMode
        {
            get
            {
                return m_TextExtractionMode;
            }
            set
            {
                m_TextExtractionMode = value;
            }
        }
        private DocumentTextExtractionMode m_TextExtractionMode;
    }
    [DataContract]
    public class GetTextResponse : Response
    {
        /// <summary>
        /// The page text that was processed.
        /// </summary>
        [DataMember(Name = "pageText")]
        public DocumentPageText PageText
        {
            get
            {
                return m_PageText;
            }
            set
            {
                m_PageText = value;
            }
        }
        private DocumentPageText m_PageText;
    }

    [DataContract]
    public class GetAnnotationsRequest : Request
    {
        /// <summary>
        /// The ID of the document to get annotations from.
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
        /// The pag enumber of the document.
        /// </summary>
        [DataMember(Name = "pageNumber")]
        public int PageNumber
        {
            get
            {
                return m_PageNumber;
            }
            set
            {
                m_PageNumber = value;
            }
        }
        private int m_PageNumber;

        /// <summary>
        /// Create empty annotations, if none exist?
        /// </summary>
        [DataMember(Name = "createEmpty")]
        public bool CreateEmpty
        {
            get
            {
                return m_CreateEmpty;
            }
            set
            {
                m_CreateEmpty = value;
            }
        }
        private bool m_CreateEmpty;
    }

    [DataContract]
    public class GetAnnotationsResponse : Response
    {
        /// <summary>
        /// The annotations, as a string.
        /// </summary>
        [DataMember(Name = "annotations")]
        public string Annotations
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
        private string m_Annotations;
    }

    [DataContract]
    public class SetAnnotationsRequest : Request
    {
        /// <summary>
        /// The ID of the document to set annotations for.
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
        /// The page number.
        /// </summary>
        [DataMember(Name = "pageNumber")]
        public int PageNumber
        {
            get
            {
                return m_PageNumber;
            }
            set
            {
                m_PageNumber = value;
            }
        }
        private int m_PageNumber;

        /// <summary>
        /// The annotations, as a string.
        /// </summary>
        [DataMember(Name = "annotations")]
        public string Annotations
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
        private string m_Annotations;
    }
}