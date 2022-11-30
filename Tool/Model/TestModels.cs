using System;
using System.Runtime.Serialization;

namespace TABFusionRMS.Web.Tool.Model.Test
{
    [DataContract]
    public partial class PingResponse : Response
    {
        /// <summary>
        /// A simple message, for testing.
        /// </summary>
        [DataMember(Name = "message")]
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

        /// <summary>
        /// The current time, so the user may tell if it was cached.
        /// </summary>
        [DataMember(Name = "time")]
        public DateTime Time
        {
            get
            {
                return m_Time;
            }
            set
            {
                m_Time = value;
            }
        }
        private DateTime m_Time;

        /// <summary>
        /// Whether or not the license was able to be checked.
        /// </summary>
        [DataMember(Name = "isLicenseChecked")]
        public bool IsLicenseChecked
        {
            get
            {
                return m_IsLicenseChecked;
            }
            set
            {
                m_IsLicenseChecked = value;
            }
        }
        private bool m_IsLicenseChecked;

        /// <summary>
        /// Whether or not the license is expired.
        /// </summary>
        [DataMember(Name = "isLicenseExpired")]
        public bool IsLicenseExpired
        {
            get
            {
                return m_IsLicenseExpired;
            }
            set
            {
                m_IsLicenseExpired = value;
            }
        }
        private bool m_IsLicenseExpired;

        /// <summary>
        /// The type of kernel - evalutation, for example.
        /// </summary>
        [DataMember(Name = "kernelType")]
        public string KernelType
        {
            get
            {
                return m_KernelType;
            }
            set
            {
                m_KernelType = value;
            }
        }
        private string m_KernelType;

        /// <summary>
        /// Whether the cache was accessed successfully.
        /// </summary>
        [DataMember(Name = "isCacheAccessible")]
        public bool IsCacheAccessible
        {
            get
            {
                return m_IsCacheAccessible;
            }
            set
            {
                m_IsCacheAccessible = value;
            }
        }
        private bool m_IsCacheAccessible;

        /// <summary>
        /// The value of the OCREngineStatus enum indicating the OCR Engine Status.
        /// </summary>
        [DataMember(Name = "ocrEngineStatus")]
        public int OcrEngineStatus
        {
            get
            {
                return m_OcrEngineStatus;
            }
            set
            {
                m_OcrEngineStatus = value;
            }
        }
        private int m_OcrEngineStatus;
    }

    [DataContract]
    public partial class CheckLicenseResponse : Response
    {
        /// <summary>
        /// Is the license expired?
        /// </summary>
        [DataMember(Name = "isExpired")]
        public bool IsExpired
        {
            get
            {
                return m_IsExpired;
            }
            set
            {
                m_IsExpired = value;
            }
        }
        private bool m_IsExpired;
    }


}