using System;
using System.Linq;
using System.Runtime.Serialization;
using Leadtools;
using Leadtools.Documents;
using Leadtools.Forms.DocumentWriters;
using Leadtools.Documents.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TABFusionRMS.Web.Tool.Helpers
{
    internal sealed partial class DocumentHelper
    {
        private DocumentHelper()
        {
        }
        public static void CheckLoadFromCache(Document document)
        {
            if (document is null)
            {
                throw new InvalidOperationException("Document was not found in the cache");
            }
        }

        public static void CheckPageNumber(Document document, int pageNumber)
        {
            if (pageNumber > document.Pages.Count)
            {
                throw new ArgumentOutOfRangeException("pageNumber", pageNumber, "Must be a value between 1 and " + Convert.ToString(document.Pages.Count));
            }
        }

        public static void DeleteDocument(string documentId, bool preventIfPreCached, bool throwIfNull)
        {
            Leadtools.Caching.ObjectCache cache = ServiceHelper.Cache;
            using (Document document = DocumentFactory.LoadFromCache(cache, documentId))
            {
                if (throwIfNull)
                {
                    CheckLoadFromCache(document);
                }

                if (document is not null)
                {
                    // Check if it's one of our pre-cached documents. 
                    // If it is, don't remove it from the cache.
                    if (PreCacheHelper.PreCacheExists && document.Uri is not null)
                    {
                        if (preventIfPreCached && PreCacheHelper.CheckDocument(document.Uri, document.Images.MaximumImagePixelSize) is not null)
                        {
                            return;
                        }
                        else
                        {
                            PreCacheHelper.RemoveDocument(document.Uri, new int[] { document.Images.MaximumImagePixelSize });
                        }
                    }

                    document.AutoDeleteFromCache = true;
                    // But not the children documents (if any)
                    foreach (Document child in document.Documents)
                        child.AutoDeleteFromCache = false;
                    document.AutoDisposeDocuments = true;
                }
            }
        }
    }

    [Serializable]
    [DataContract]
    public partial class DocumentConverterJobData
    {
        public DocumentConverterJobData()
        {
            JobErrorMode = DocumentConverterJobErrorMode.Continue;
            PageNumberingTemplate = "##name##_Page(##page##).##extension##";
            EnableSvgConversion = true;
            SvgImagesRecognitionMode = DocumentConverterSvgImagesRecognitionMode.Auto;
        }

        [DataMember(Name = "jobErrorMode")]
        public DocumentConverterJobErrorMode JobErrorMode
        {
            get
            {
                return m_JobErrorMode;
            }
            set
            {
                m_JobErrorMode = value;
            }
        }
        private DocumentConverterJobErrorMode m_JobErrorMode;

        [DataMember(Name = "pageNumberingTemplate")]
        public string PageNumberingTemplate
        {
            get
            {
                return m_PageNumberingTemplate;
            }
            set
            {
                m_PageNumberingTemplate = value;
            }
        }
        private string m_PageNumberingTemplate;

        [DataMember(Name = "enableSvgConversion")]
        public bool EnableSvgConversion
        {
            get
            {
                return m_EnableSvgConversion;
            }
            set
            {
                m_EnableSvgConversion = value;
            }
        }
        private bool m_EnableSvgConversion;

        [DataMember(Name = "svgImagesRecognitionMode")]
        public DocumentConverterSvgImagesRecognitionMode SvgImagesRecognitionMode
        {
            get
            {
                return m_SvgImagesRecognitionMode;
            }
            set
            {
                m_SvgImagesRecognitionMode = value;
            }
        }
        private DocumentConverterSvgImagesRecognitionMode m_SvgImagesRecognitionMode;

        [DataMember(Name = "emptyPageMode")]
        public DocumentConverterEmptyPageMode EmptyPageMode
        {
            get
            {
                return m_EmptyPageMode;
            }
            set
            {
                m_EmptyPageMode = value;
            }
        }
        private DocumentConverterEmptyPageMode m_EmptyPageMode;

        [DataMember(Name = "preprocessorDeskew")]
        public bool PreprocessorDeskew
        {
            get
            {
                return m_PreprocessorDeskew;
            }
            set
            {
                m_PreprocessorDeskew = value;
            }
        }
        private bool m_PreprocessorDeskew;

        [DataMember(Name = "preprocessorOrient")]
        public bool PreprocessorOrient
        {
            get
            {
                return m_PreprocessorOrient;
            }
            set
            {
                m_PreprocessorOrient = value;
            }
        }
        private bool m_PreprocessorOrient;

        [DataMember(Name = "preprocessorInvert")]
        public bool PreprocessorInvert
        {
            get
            {
                return m_PreprocessorInvert;
            }
            set
            {
                m_PreprocessorInvert = value;
            }
        }
        private bool m_PreprocessorInvert;

        [DataMember(Name = "inputDocumentFirstPageNumber")]
        public int InputDocumentFirstPageNumber
        {
            get
            {
                return m_InputDocumentFirstPageNumber;
            }
            set
            {
                m_InputDocumentFirstPageNumber = value;
            }
        }
        private int m_InputDocumentFirstPageNumber;

        [DataMember(Name = "inputDocumentLastPageNumber")]
        public int InputDocumentLastPageNumber
        {
            get
            {
                return m_InputDocumentLastPageNumber;
            }
            set
            {
                m_InputDocumentLastPageNumber = value;
            }
        }
        private int m_InputDocumentLastPageNumber;

        [DataMember(Name = "documentFormat")]
        public DocumentFormat DocumentFormat
        {
            get
            {
                return m_DocumentFormat;
            }
            set
            {
                m_DocumentFormat = value;
            }
        }
        private DocumentFormat m_DocumentFormat;

        [DataMember(Name = "rasterImageFormat")]
        public RasterImageFormat RasterImageFormat
        {
            get
            {
                return m_RasterImageFormat;
            }
            set
            {
                m_RasterImageFormat = value;
            }
        }
        private RasterImageFormat m_RasterImageFormat;

        [DataMember(Name = "rasterImageBitsPerPixel")]
        public int RasterImageBitsPerPixel
        {
            get
            {
                return m_RasterImageBitsPerPixel;
            }
            set
            {
                m_RasterImageBitsPerPixel = value;
            }
        }
        private int m_RasterImageBitsPerPixel;

        // We deserialize this field manually from JSON Object to a specific DocumentOptions type
        [DataMember(Name = "documentOptions")]
        public JObject DocumentOptions
        {
            get
            {
                return m_DocumentOptions;
            }
            set
            {
                m_DocumentOptions = value;
            }
        }
        private JObject m_DocumentOptions;

        [DataMember(Name = "jobName")]
        public string JobName
        {
            get
            {
                return m_JobName;
            }
            set
            {
                m_JobName = value;
            }
        }
        private string m_JobName;

        [DataMember(Name = "annotationsMode")]
        public DocumentConverterAnnotationsMode AnnotationsMode
        {
            get
            {
                return m_AnnotationsMode;
            }
            set
            {
                m_AnnotationsMode = value;
            }
        }
        private DocumentConverterAnnotationsMode m_AnnotationsMode;

        [DataMember(Name = "documentName")]
        public string DocumentName
        {
            get
            {
                return m_DocumentName;
            }
            set
            {
                m_DocumentName = value;
            }
        }
        private string m_DocumentName;

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