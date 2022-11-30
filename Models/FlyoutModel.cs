using System.Collections.Generic;
using Microsoft.VisualBasic.CompilerServices;

namespace TabFusionRMS.WebCS
{
    public class FlyoutModel : BaseModel
    {
        private int p_sPageSize;
        private int p_sPageIndex;
        private int p_totalRecCount;
        private bool p_downloadPermission;
        // Private p_sViewerLink As String
        private List<FlyOutDetails> p_sFlyOutDetails;
        private bool p_hasAnnotation;
        private string P_viewName;
        private string P_stringQuery;
        private string P_filter;

        public FlyoutModel()
        {
            p_sFlyOutDetails = new List<FlyOutDetails>();
        }

        public int sPageSize
        {
            get
            {
                return p_sPageSize;
            }
            set
            {
                p_sPageSize = value;
            }
        }

        public int sPageIndex
        {
            get
            {
                return p_sPageIndex;
            }
            set
            {
                p_sPageIndex = value;
            }
        }

        public int totalRecCount
        {
            get
            {
                return p_totalRecCount;
            }
            set
            {
                p_totalRecCount = value;
            }
        }

        // Public Property sViewerLink As String
        // Get
        // Return p_sViewerLink
        // End Get
        // Set(value As String)
        // p_sViewerLink = value
        // End Set
        // End Property

        public List<FlyOutDetails> FlyOutDetails
        {
            get
            {
                return p_sFlyOutDetails;
            }
            set
            {
                p_sFlyOutDetails = value;
            }
        }

        public bool downloadPermission
        {
            get
            {
                return p_downloadPermission;
            }
            set
            {
                p_downloadPermission = value;
            }
        }
        public string viewName
        {
            get
            {
                return P_viewName;
            }
            set
            {
                P_viewName = value;
            }
        }
        public string filter
        {
            get
            {
                return P_filter;
            }
            set
            {
                P_filter = value;
            }
        }
        public string stringQuery
        {
            get
            {
                return P_stringQuery;
            }
            set
            {
                P_stringQuery = value;
            }
        }
    }

    public class FlyOutDetails
    {
        private int p_sAttachId;
        private string p_sOrgFilePath;
        private string p_sAttachmentName;
        private byte[] p_sFlyoutImages;
        private string p_sViewerLink;
        private int P_attchVersion;
        private int P_viewid;
        private object P_downloadEncryptAttachment;
        private int p_RecordType;
        private int p_RowNum;
        private string p_ScanDateTime;

        public int sRowNum
        {
            get
            {
                return p_RowNum;
            }
            set
            {
                p_RowNum = value;
            }
        }
        public int sAttachId
        {
            get
            {
                return p_sAttachId;
            }
            set
            {
                p_sAttachId = value;
            }
        }

        public int recordType
        {
            get
            {
                return p_RecordType;
            }
            set
            {
                p_RecordType = value;
            }
        }
        public string scanDateTime
        {
            get
            {
                return p_ScanDateTime;
            }
            set
            {
                p_ScanDateTime = value;
            }
        }

        public string downloadEncryptAttachment
        {
            get
            {
                return Conversions.ToString(P_downloadEncryptAttachment);
            }
            set
            {
                P_downloadEncryptAttachment = value;
            }
        }


        public string sOrgFilePath
        {
            get
            {
                return p_sOrgFilePath;
            }
            set
            {
                p_sOrgFilePath = value;
            }
        }

        public string sAttachmentName
        {
            get
            {
                return p_sAttachmentName;
            }
            set
            {
                p_sAttachmentName = value;
            }
        }


        public byte[] sFlyoutImages
        {
            get
            {
                return p_sFlyoutImages;
            }
            set
            {
                p_sFlyoutImages = value;
            }
        }

        public string sViewerLink
        {
            get
            {
                return p_sViewerLink;
            }
            set
            {
                p_sViewerLink = value;
            }
        }
        public int attchVersion
        {
            get
            {
                return P_attchVersion;
            }
            set
            {
                P_attchVersion = value;
            }
        }
    }
}
