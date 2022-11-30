using System;
using System.Text;
using Smead.RecordsManagement;
using Smead.Security;
using TabFusionRMS.Resource;

namespace TabFusionRMS.WebCS
{
    public class OrphanModel : BaseModel
    {
        public OrphanModel(Passport passport)
        {
            this._passport = passport;
            TableName = "";
            TableRecordId = "";
            ViewId = "";
            ViewName = "";
            FlyoutModel = new FlyoutModel();
            IsAttachable = 0;
            TotalRecords = 0;
            SelectedViewType = "Grid";
            DeletePermission = false;
        }
        public string AccessMenu { get; set; }
        public void GenerateMenu()
        {
            var sbMenu = new StringBuilder();
            sbMenu.Append("<ul class='drillDownMenu '><li>");
            sbMenu.Append(string.Format("<a href='#'><i class='font_icon theme_color fa fa-database'></i> " + Languages.get_Translation("LabelVault") + " </a>"));
            sbMenu.Append("<ul>");
            CreateHtml(sbMenu);
            sbMenu.Append("</ul>");
            sbMenu.Append("</li></ul>");
        }
        public void GenerateMenuWithOutSubmenu()
        {
            var sbMenu = new StringBuilder();
            sbMenu.Append(@"<ul class='drillDownMenu' style='display: flex;' href='javascript:;' onclick='obJvaultfunction.RefreshAttachments(this)'><li style='font-size: 17px'><a>
                        <i style='font-size:17px;' class='font_icon theme_color fa fa-database'></i></a></li><li style='margin-Left:-13px;font-size:17px; width: 100%;'>
                        <a id='vaultLabel' class='selectedMenu'>" + Languages.get_Translation("LabelVault") + " </a></li></ul>");
            AccessMenu = sbMenu.ToString();
        }
        // docdata =>  'TableName,TableRecordId,ViewName, ViewId'
        public void SetValues(string docdata, FlyoutModel flyoutModel)
        {
            if (docdata.Length > 0)
            {
                var splitDoc = docdata.Split(',');
                if (splitDoc.Length == 4)
                {
                    TableName = splitDoc[0];
                    TableRecordId = splitDoc[1];
                    ViewName = splitDoc[2];
                    ViewId = splitDoc[3];

                    // Set Display Name
                    DisplayName = Navigation.GetItemName(TableName, TableRecordId, this._passport);
                    IsAttachable = 1;
                }
            }
            FlyoutModel = flyoutModel;
        }
        public void CreateHtml(StringBuilder sbMenu)
        {
            try
            {
                sbMenu.Append(string.Format("<li><a onclick='obJvaultfunction.RefreshAttachments(this)'> " + Languages.get_Translation("LabelVault") + "</a>"));
                sbMenu.Append("<ul><li><a id='listHover'>Vault</a></li></ul></li>");
            }

            catch (Exception)
            {
                // call error method
            }

            AccessMenu = sbMenu.ToString();
        }

        public int OrphanPerPageRecord()
        {
            var dt = Smead.RecordsManagement.Imaging.Attachments.GetOrphansPerPageRecord(this._passport);
            DefaultPerPageRecord = new PagingModel().PerPageRecord;
            if (dt.Rows.Count > 0)
            {
                int ppr = Convert.ToInt32(dt.Rows[0][0]);
                if (ppr > 0)
                {
                    DefaultPerPageRecord = ppr;
                }
            }
            return DefaultPerPageRecord;
        }

        public string TableName;
        public string ViewId;
        public string ViewName;
        public string TableRecordId;
        public string SelectedViewType;
        public string DisplayName;
        public FlyoutModel FlyoutModel;
        public int IsAttachable;
        public int TotalRecords;
        public bool DeletePermission;
        public int DefaultPerPageRecord;
    }

    public class FileInfoModel
    {
        public string Path;
        public string OrgFileName;
    }
}
