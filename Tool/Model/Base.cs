
using System.Runtime.Serialization;

namespace TABFusionRMS.Web.Tool.Model
{
    [DataContract]
    public partial class Request
    {
        /// <summary>
        /// Any arbitrary data the user may wish to pass along through the LEADTOOLS Documents library to the service.
        /// </summary>
        [DataMember(Name = "userData")]
        public object UserData
        {
            get
            {
                return m_UserData;
            }
            set
            {
                m_UserData = value;
            }
        }
        private object m_UserData;
    }

    [DataContract]
    public partial class Response
    {
        /// <summary>
        /// Any arbitrary data the user may wish to pass along from the service through the LEADTOOLS Documents library.
        /// </summary>
        [DataMember(Name = "userData")]
        public object UserData
        {
            get
            {
                return m_UserData;
            }
            set
            {
                m_UserData = value;
            }
        }
        private object m_UserData;
    }
}