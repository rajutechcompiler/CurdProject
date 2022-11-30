using System;
using System.Collections.Specialized;
using Microsoft.VisualBasic;
using TabFusionRMS.Models;

namespace TabFusionRMS.WebCS
{

    public class CustomAssetStatus
    {
        public AssetStatu AssetStatus
        {
            get
            {
                return m_AssetStatus;
            }
            set
            {
                m_AssetStatus = value;
            }
        }
        private AssetStatu m_AssetStatus;


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