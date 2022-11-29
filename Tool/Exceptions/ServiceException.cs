using System;
using System.Net;
using System.Runtime.Serialization;
using Leadtools;

namespace TABFusionRMS.Web.Tool.Exceptions
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed partial class ServiceErrorAttribute : Attribute
    {
        // The user-safe message that will eventually be the ServiceError.Message.
        public string Message
        {
            get
            {
                return m_Message;
            }
            set
            {
                m_Message = value;
            }
        }
        private string m_Message;

        // An alternate MethodName value to return.
        public string MethodName
        {
            get
            {
                return m_MethodName;
            }
            set
            {
                m_MethodName = value;
            }
        }
        private string m_MethodName;
    }


    [DataContract]
    public partial class ServiceError
    {
        // Always safe to show to an end-user.
        // * Will hold the "SafeErrorMessage" attribute of a method
        // * or the Exception.Message of an explicit ServiceException.
        // 

        [DataMember]
        public string Message
        {
            get
            {
                return m_Message;
            }
            set
            {
                m_Message = value;
            }
        }
        private string m_Message;

        // Sometimes null. Not end-user safe.
        // * Often contains the actual Exception.Message from an 
        // * Exception that wasn't thrown as a ServiceException.
        // 

        [DataMember]
        public string Detail
        {
            get
            {
                return m_Detail;
            }
            set
            {
                m_Detail = value;
            }
        }
        private string m_Detail;

        [DataMember]
        public int Code
        {
            get
            {
                return m_Code;
            }
            set
            {
                m_Code = value;
            }
        }
        private int m_Code;

        [DataMember]
        public string Link
        {
            get
            {
                return m_Link;
            }
            set
            {
                m_Link = value;
            }
        }
        private string m_Link;

        [DataMember]
        public string ExceptionType
        {
            get
            {
                return m_ExceptionType;
            }
            set
            {
                m_ExceptionType = value;
            }
        }
        private string m_ExceptionType;

        [DataMember]
        public string MethodName
        {
            get
            {
                return m_MethodName;
            }
            set
            {
                m_MethodName = value;
            }
        }
        private string m_MethodName;
    }


    // 
    // * For use with all top-level errors.
    // * Has special methods to match with GlobalExceptionHandler
    // * and make a user-safe error message.
    // 

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2240:ImplementISerializableCorrectly")]
    [Serializable]
    public sealed partial class ServiceException : Exception
    {
        public string Action
        {
            get
            {
                return m_Action;
            }
            set
            {
                m_Action = value;
            }
        }
        private string m_Action;
        private int Code
        {
            get
            {
                return m_Code;
            }
            set
            {
                m_Code = value;
            }
        }
        private int m_Code;
        private string Link
        {
            get
            {
                return m_Link;
            }
            set
            {
                m_Link = value;
            }
        }
        private string m_Link;
        private string Detail
        {
            get
            {
                return m_Detail;
            }
            set
            {
                m_Detail = value;
            }
        }
        private string m_Detail;
        public HttpStatusCode StatusCode
        {
            get
            {
                return m_StatusCode;
            }
            private set
            {
                m_StatusCode = value;
            }
        }
        private HttpStatusCode m_StatusCode;

        public ServiceException() : base()
        {
        }

        public ServiceException(string userMessage) : base(userMessage)
        {
            SetDefaults();
        }

        public ServiceException(string userMessage, Exception innerException) : base(userMessage, innerException)
        {
            SetDefaults();

            Detail = innerException is not null ? innerException.Message : null;
            CheckInnerException();
        }

        public ServiceException(string userMessage, HttpStatusCode statusCode) : base(userMessage)
        {
            SetDefaults();

            statusCode = statusCode;
        }

        public ServiceException(string userMessage, Exception innerException, HttpStatusCode statusCode) : base(userMessage, innerException)
        {
            SetDefaults();

            statusCode = statusCode;
            Detail = innerException is not null ? innerException.Message : null;
            CheckInnerException();
        }

        private void CheckInnerException()
        {
            try
            {
                // add more here, if necessary.
                Leadtools.RasterException rasterException = InnerException as Leadtools.RasterException;
                if (rasterException is not null)
                {
                    Code = (int)rasterException.Code;
                    Link = rasterException.HelpLink is not null ? rasterException.HelpLink : "https://www.leadtools.com/help/leadtools/v19m/dh/l/rasterexceptioncode.html";
                    return;
                }

                Leadtools.Forms.Ocr.OcrException ocrException = InnerException as Leadtools.Forms.Ocr.OcrException;
                if (ocrException is not null)
                {
                    Code = (int)ocrException.Code;
                    Link = ocrException.HelpLink is not null ? ocrException.HelpLink : "https://www.leadtools.com/help/leadtools/v19m/dh/fo/ocrexception.html";
                    return;
                }
            }

            // Dim barcodeException As Barcode.BarcodeException = TryCast(Me.InnerException, Leadtools.Barcode.BarcodeException)
            // If barcodeException IsNot Nothing Then
            // Code = CInt(barcodeException.Code)
            // Link = If(barcodeException.HelpLink IsNot Nothing, barcodeException.HelpLink, "https://www.leadtools.com/help/leadtools/v19m/dh/ba/barcodeexceptioncode.html")
            // Return
            // End If
            catch
            {
            }
        }

        // For when we don't have an InnerException with anything to offer.
        private void SetDefaults()
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Detail = Message;
            Action = "Unknown";
            Code = 0;
            Link = "https://www.leadtools.com/help/leadtools/v19m/dh/javascript/to/introduction.htm";
        }

        public ServiceError ToServiceError()
        {
            return new ServiceError()
            {
                Message = Message,
                Detail = Detail,
                Code = Code,
                Link = Link,
                ExceptionType = InnerException is not null ? InnerException.GetType().ToString() : GetType().ToString(),
                MethodName = Action
            };
        }

        // Can't use this for Message property, because it's required in constructor
        // For Exceptions
        public void ConsumeAttribute(ServiceErrorAttribute attribute)
        {
            if (attribute is null)
            {
                throw new ArgumentNullException("attribute");
            }

            if (!string.IsNullOrEmpty(attribute.MethodName))
            {
                Action = attribute.MethodName;
            }
        }
    }

}