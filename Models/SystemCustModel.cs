using Microsoft.VisualBasic;
using System;
using System.Collections.Specialized;
using System.Linq;
namespace TabFusionRMS.WebCS
{
    public partial class SystemCustModel
    {
        public TabFusionRMS.Models.System SystemModel
        {
            get
            {
                return m_SystemModel;
            }
            set
            {
                m_SystemModel = value;
            }
        }
        private TabFusionRMS.Models.System m_SystemModel;

        public string get_CounterValue(string sCounterName)
        {
            try
            {
                return mcCounterValues[sCounterName].ToString();
            }
            catch (Exception ex)
            {
                return "0";
            }
        }

        public void set_CounterValue(string sCounterName, string value)
        {
            if (mcCounterValues.Contains(sCounterName))
            {
                mcCounterValues[sCounterName] = Strings.Trim(value);
            }
            else
            {
                mcCounterValues.Add(sCounterName, Strings.Trim(value));
            }
        }
        private string m_CounterValue;

        private ListDictionary mcCounterValues = new ListDictionary();
    }
}
