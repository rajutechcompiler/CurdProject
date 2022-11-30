
namespace TabFusionRMS.WebCS
{
    public class TABQUIKModel
    {
        private string m_TABQUIKField;

        public string TABQUIKField
        {
            get
            {
                return m_TABQUIKField;
            }
            set
            {
                m_TABQUIKField = value;
            }
        }

        public string TABFusionRMSField
        {
            get
            {
                return m_TABFusionRMSField;
            }
            set
            {
                m_TABFusionRMSField = value;
            }
        }

        public string Manual
        {
            get
            {
                return m_Manual;
            }
            set
            {
                m_Manual = value;
            }
        }

        public string Format
        {
            get
            {
                return m_Format;
            }
            set
            {
                m_Format = value;
            }
        }

        private string m_TABFusionRMSField;
        private string m_Manual;

        private string m_Format;
    }
}