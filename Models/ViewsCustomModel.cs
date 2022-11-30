using System.Collections.Generic;
using TabFusionRMS.Models;

namespace TabFusionRMS.WebCS
{

    public class ViewsCustomModel
    {
        public View ViewsModel
        {
            get
            {
                return m_ViewsModel;
            }
            set
            {
                m_ViewsModel = value;
            }
        }
        private View m_ViewsModel;

        public List<ViewFilter> ViewFilterList
        {
            get
            {
                return m_ViewFilterList;
            }
            set
            {
                m_ViewFilterList = value;
            }
        }
        private List<ViewFilter> m_ViewFilterList;
    }
}