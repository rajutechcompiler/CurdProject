using System;
using System.Collections.Specialized;
using Microsoft.VisualBasic;
using TabFusionRMS.Models;

namespace TabFusionRMS.WebCS
{
    public class CustomTrackingHistory
    {
        public TrackingHistory TrackingHistory
        {
            get
            {
                return m_TrackingHistory;
            }
            set
            {
                m_TrackingHistory = value;
            }
        }
        private TrackingHistory m_TrackingHistory;


        public string get_ContainerColumns(string sKey)
        {
            try
            {
                return mcContainerColumns[sKey].ToString();
            }
            catch (Exception)
            {
                return "0";
            }
        }

        public void set_ContainerColumns(string sKey, string value)
        {
            if (mcContainerColumns.Contains(sKey))
            {
                mcContainerColumns[sKey] = Strings.Trim(value);
            }
            else
            {
                mcContainerColumns.Add(sKey, Strings.Trim(value));
            }
        }
        private string m_CounterValue;
        private ListDictionary mcContainerColumns = new ListDictionary();
    }
}