 

using System.Collections.Generic;
using System.Web;
// POPUP DOCUMENT VIEWER
public class PopupdocViewerModel : BaseModel
{
    public PopupdocViewerModel()
    {

    }
    public string checkConditions { get; set; }
    public string WarringMsg { get; set; }

    public class popupdocViewerUI
    {
        public string filesizeMB { get; set; }
        public string tabName { get; set; }
        public string tableId { get; set; }
        public string viewId { get; set; }
        public string name { get; set; }
        //public List<HttpPostedFile> files { get; set; }
    }
}