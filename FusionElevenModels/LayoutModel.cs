using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
//using System.Web.Script.Serialization;
//using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.Expressions;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Smead.RecordsManagement;
using static Smead.RecordsManagement.Navigation;
using Smead.Security;
using TabFusionRMS.Models;
using View = TabFusionRMS.Models.View;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.Resource;
using slimShared;
using Newtonsoft.Json;
using System.Reflection;

namespace TabFusionRMS.WebCS
{
    // DATA MODEL RETURN DOM OBJECT
    public class LayoutModel : BaseModel
    {
        public LayoutModel(Passport passport, HttpContext httpContext)
        {
            _passport = passport;
            _httpContext = httpContext;
            // Me._levelManager = levelmanager
            Layout = new MainLayout(_passport, httpContext);
            Taskbar = new TasksBar(_passport);
            NewsFeed = new NewsFeed(_passport);
            Footer = new Footer(_passport, httpContext);
        }
        // Private Property _passport As Passport
        // Private Property _levelManager As LevelManager
        public MainLayout Layout { get; set; }
        public TasksBar Taskbar { get; set; }
        public NewsFeed NewsFeed { get; set; }
        public Footer Footer { get; set; }
        // Property errorNumber As String

    }
    // LAYOUT MODEL RETURN DOM OBJECT
    public class MainLayout : BaseModel
    {
        public MainLayout(Passport passport, HttpContext httpContext)
        {
            _passport = passport;
            _httpContext = httpContext;
        }
        public string LinkLabelManager { get; set; }
        public string LinkImport { get; set; }
        public string LinkTracking { get; set; }
        public string Reports { get; set; }
        public string Vault { get; set; }
        public string LinkAdmin { get; set; }
        public string UserAccessMenuHtml { get; set; }
        public string BackgroundStatusNotification { get; set; }
        public string LanguageCulture { get; set; }
        public string LinkLabelDashboard { get; set; }
        private int _ALId { get; set; }
        private int _LId { get; set; }
        private int _FALId { get; set; }
        private IRepository<Table> _iTable { get; set; }
        private IRepository<View> _iView { get; set; }

        private void BindUserAccessMenu()
        {
            IRepository<s_SavedCriteria> _is_SavedCriteria = new Repositories<s_SavedCriteria>();
            var sbMenu = new StringBuilder();
            string strLId;
            // ''' Start : Bind My Queries and My Favourites Menu
            var lWorkGroupItem = new List<WorkGroupItem>();

            if (_passport.CheckPermission(Common.SECURE_MYQUERY, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
            {
                lWorkGroupItem.Add(new WorkGroupItem()
                {
                    ID = (long)Enums.SavedType.Query,
                    WorkGroupName = Languages.get_Translation("MyQuery")
                });
            }

            if (_passport.CheckPermission(Common.SECURE_MYFAVORITE, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
            {
                lWorkGroupItem.Add(new WorkGroupItem()
                {
                    ID = (long)Enums.SavedType.Favorite,
                    WorkGroupName = Languages.get_Translation("MyFavourite")
                });
            }

            // Added By Raju (India)
            // lWorkGroupItem.Add(New WorkGroupItem() With {
            // .ID = Enums.SavedType.Valt,
            // .WorkGroupName = "Valt"})

            foreach (WorkGroupItem workGroupItem in lWorkGroupItem)
            {
                strLId = string.Format("L{0}", _LId);
                sbMenu.Append("<ul class='drillDownMenu MyQuery_Fav'><li>");
                if (workGroupItem.ID == 1L)
                {
                    sbMenu.Append(string.Format("<a href='#' id='MyfavClickMenu'><i class='font_icon theme_color fa fa-database'></i>{0}</a>", workGroupItem.WorkGroupName));
                }
                else if (workGroupItem.ID == 0L)
                {
                    sbMenu.Append(string.Format("<a href='#' id='MyQueryClickMenu'><i class='font_icon theme_color fa fa-database'></i>{0}</a>", workGroupItem.WorkGroupName));
                    // ElseIf workGroupItem.ID = 2 Then
                    // sbMenu.Append(String.Format("<a href='#' onclick=""obJgridfunc.LoadViewValt(this,'{0}')""><i class='font_icon theme_color fa fa-database'></i>{1}</a>", workGroupItem.WorkGroupName, workGroupItem.WorkGroupName))
                }


                var subMenu = _is_SavedCriteria.All().Where(x => x.UserId == _passport.UserId && x.SavedType == workGroupItem.ID).ToList();
                if (subMenu.Count > 0)
                {
                    _LId += 1;
                    strLId = string.Format("{0}{1}", strLId, _LId);
                    sbMenu.Append("<ul>");
                    this.BindSavedSubMenu(subMenu, ref sbMenu, strLId);
                    sbMenu.Append("</ul>");
                }
                else
                {
                    sbMenu.Append("<ul>");
                    sbMenu.Append("</ul>");
                }
                sbMenu.Append("</li></ul>");
                _LId += 1;
            }
            // ''' End : Bind My Queries and My Favourites Menu
            foreach (WorkGroupItem workGroupItem in GetWorkGroups(_passport))
            {
                strLId = string.Format("L{0}", _LId);
                sbMenu.Append("<ul class='drillDownMenu'><li>");
                sbMenu.Append(string.Format("<a href='#' id='RA{0}'><i class='font_icon theme_color fa fa-database'></i>{1}</a>", strLId, workGroupItem.WorkGroupName));

                var subMenu = GetWorkGroupMenu((short)workGroupItem.ID, _passport);

                if (subMenu.Count > 0)
                {
                    _LId += 1;
                    strLId = string.Format("{0}{1}", strLId, _LId);
                    sbMenu.Append("<ul>");
                    BindUserSubMenu(subMenu, ref sbMenu, strLId);
                    sbMenu.Append("</ul>");
                }
                sbMenu.Append("</li></ul>");
                _LId += 1;
            }

            // If CheckPermssionsReportTab() Then
            // _LId += 1
            // sbMenu.Append("<ul class='drillDownMenu'><li>")
            // sbMenu.Append(String.Format("<a href='#' id='RAL{0}'><i class='font_icon theme_color fa fa-database'></i>{1}</a>", _LId, Languages.Translation("Reports")))
            // sbMenu.Append("<ul>")
            // LoadCustomReportsMenu(sbMenu, ("L" + _LId.ToString()))
            // sbMenu.Append("</ul>")
            // sbMenu.Append("</li></ul>")
            // End If

            if (_passport.CheckPermission(Common.SECURE_RETENTION_SETUP, Smead.Security.SecureObject.SecureObjectType.Retention, Permissions.Permission.Access))
            {
                _LId += 1;
                sbMenu.Append("<ul class='drillDownMenu'><li>");
                sbMenu.Append(string.Format("<a href='/Retention/RetentionCodeMaintenance' id='FAL{0}'><i class='font_icon theme_color fa fa-database'></i>{1}</a>", _LId, Languages.get_Translation("Retention")));
                sbMenu.Append("</li></ul>");
            }

            UserAccessMenuHtml = sbMenu.ToString();
        }
        // Private Function ReportURL(ByVal queryString, Optional secondQueryString = Nothing) As String
        // If secondQueryString IsNot Nothing Then
        // Return "htmlreports.aspx?r=" + queryString + "&t=" + secondQueryString
        // Else
        // Return "htmlreports.aspx?r=" + queryString
        // End If
        // End Function
        // Private Sub LoadCustomReportsMenu(ByRef sbMenu As StringBuilder, ByVal parentIdNo As String)
        // _ALId += 1

        // sbMenu.Append(String.Format("<li><a href='#' id='A{0}'>{1}</a>", (parentIdNo + _ALId.ToString()), Languages.Translation("lblReportsLandingCustomReports")))
        // Dim customReports = GetViewReports(_passport)
        // sbMenu.Append("<ul>")

        // For Each report In customReports
        // If _passport.CheckPermission(report.ViewName, Smead.Security.SecureObject.SecureObjectType.Reports, Smead.Security.Permissions.Permission.View) Then
        // sbMenu.Append(String.Format("<li><a onClick=""obJreports.LoadCustomReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), report.ViewName, report.Id))
        // End If
        // _FALId += 1
        // Next

        // sbMenu.Append("</ul></li>")
        // If _passport.CheckPermission(" Auditing", Smead.Security.SecureObject.SecureObjectType.Reports, Smead.Security.Permissions.Permission.View) Then
        // _ALId += 1
        // sbMenu.Append(String.Format("<li><a onClick=""obJreports.LoadAuditReport(this,'','AuditReport.aspx')"" id='A{0}'>{1}</a>", (parentIdNo + _ALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlAuditReport")))
        // End If
        // If _passport.CheckPermission(" Tracking", Smead.Security.SecureObject.SecureObjectType.Reports, Smead.Security.Permissions.Permission.View) Then
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a href='#' id='AL{0}'>{1}</a>", (parentIdNo + _FALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlTraRepo")))
        // sbMenu.Append("<ul>")

        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a id='FA{0}' onClick=""obJreports.TrackableReport(this,'{2}')"">{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlPastDue"), "PastDueTrackableItemsReport"))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a id='FA{0}' onClick=""obJreports.TrackableReport(this,'{2}')"">{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlObjOut"), "CurrentItemsOutReport"))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a href='#' id='AL{0}'"">{1}</a>", (parentIdNo + _FALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlObjInven")))
        // sbMenu.Append("<ul>")

        // For Each row As DataRow In GetTrackableTables(_passport).Rows
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a id='FA{0}' onClick=""obJreports.TrackableReport(this,'{2}')"">{1}</a></li>", (parentIdNo + _FALId.ToString()), row("UserName").ToString(), "ObjectsOutReport", row("TableName").ToString()))
        // Next

        // sbMenu.Append("</ul></li>")
        // sbMenu.Append("</ul></li>")
        // End If

        // If _passport.CheckPermission(" Requestor", Smead.Security.SecureObject.SecureObjectType.Reports, Smead.Security.Permissions.Permission.View) Then
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a href='#' id='AL{0}'>{1}</a>", (parentIdNo + _FALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlReqRepo")))
        // sbMenu.Append("<ul>")
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a  onClick=""obJreports.RequestorReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlNewReq"), "NewRequestsReport"))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a  onClick=""obJreports.RequestorReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlNewBatch"), "NewBatchesReport"))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a  onClick=""obJreports.RequestorReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlPullLists"), "PullListsReport"))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a  onClick=""obJreports.RequestorReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlExcep"), "RequestExceptionsReport"))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a  onClick=""obJreports.RequestorReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlInPro"), "RequestsInProcessReport"))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a  onClick=""obJreports.RequestorReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("mnuWorkGroupMenuControlWaitList"), "WaitListReport"))

        // sbMenu.Append("</ul></li>")
        // End If

        // If _passport.CheckPermission(" Retention", Smead.Security.SecureObject.SecureObjectType.Reports, Smead.Security.Permissions.Permission.View) And _passport.CheckLicense(Smead.Security.SecureObject.SecureObjectType.Retention) Then
        // Dim turnOffCitation = CBool(GetSystemSetting("RetentionTurnOffCitations", _passport))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a href='#' id='AL{0}'>{1}</a>", (parentIdNo + _FALId.ToString()), Languages.Translation("RetentionReports")))
        // sbMenu.Append("<ul>")

        // ''Adding permission
        // If _passport.CheckPermission("Disposition", Smead.Security.SecureObject.SecureObjectType.Retention, Smead.Security.Permissions.Permission.Access) Then
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a onClick=""obJreports.RetentionReport(this,'','{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("FinalDispositionReport"), "FinalDispositionReport"))
        // End If
        // 'coninue from here. 
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a onClick=""obJreports.RetentionReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("CertificateDispositionReport"), "CertificateDispositionReport"))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a onClick=""obJreports.RetentionReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("InactivePullListsReport"), "InactivePullListsReport"))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a onClick=""obJreports.RetentionReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("InactiveReport"), "InactiveReport"))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a onClick=""obJreports.RetentionReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("OnHoldReport"), "OnHoldReport"))

        // If Not turnOffCitation Then
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a onClick=""obJreports.RetentionReport(this,'{2}')""  id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("CitationReport"), "CitationReport"))
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a onClick=""obJreports.RetentionReport(this,'{2}')""  id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("CitationRetionCodesReport"), "CitationRetionCodesReport"))
        // End If

        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a onClick=""obJreports.RetentionReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("RetentionCodesReport"), "RetentionCodesReport"))

        // If Not turnOffCitation Then
        // _FALId += 1
        // sbMenu.Append(String.Format("<li><a onClick=""obJreports.RetentionReport(this,'{2}')"" id='FA{0}'>{1}</a></li>", (parentIdNo + _FALId.ToString()), Languages.Translation("RetentionCodesCitationReport"), "RetentionCodesCitationReport"))
        // End If

        // sbMenu.Append("</ul></li>")
        // End If
        // End Sub
        private bool CheckPermssionsReportTab()
        {
            if (_passport.CheckPermission(" Auditing", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                return true;
            if (_passport.CheckPermission(" Requestor", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                return true;
            if (_passport.CheckPermission(" Retention", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                return true;
            if (_passport.CheckPermission(" Tracking", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                return true;
            return false;
        }
        private void BindSavedSubMenu(List<s_SavedCriteria> subMenu, ref StringBuilder sbMenu, string strLId)
        {
            if (subMenu.Count > 0)
            {
                IRepository<View> s_Views = new Repositories<View>();

                foreach (var V in subMenu)
                {
                    var objView = s_Views.Where(x => x.Id == V.ViewId);
                    if (objView is not null)
                    {
                        if (_passport.CheckPermission(objView.FirstOrDefault().ViewName, Smead.Security.SecureObject.SecureObjectType.View, Permissions.Permission.View))
                        {
                            if (V.SavedType == 1)
                            {
                                // sbMenu.Append(String.Format("<li><a id=""A{2}{3}"" onClick=""obJfavorite.LoadFavoriteTogrid(this,'{0}_{4}_{5}', 1)"">{1}</a></li>", V.ViewId, V.SavedName, strLId, _ALId.ToString, V.Id, V.SavedType))
                                sbMenu.Append(string.Format("<li><div class=\"row\"><div class=\"col-md-10\"><a data-location=\"1\" data-viewname=\"{1}\"  data-viewid=\"favorite\" onClick=\"obJfavorite.LoadFavoriteTogrid(this,'{0}_{4}_{5}', 1)\">{1}</a></div><div class=\"col-md-2\"><i onclick=\"obJfavorite.DeleteCeriteria(this,{4}, {0})\" style=\"margin-top:11px;margin-left: -6px;\" class=\"fa fa-trash-o\"></i></div></div></li>", V.ViewId, V.SavedName, strLId, _ALId.ToString(), V.Id, V.SavedType));
                            }
                            else
                            {
                                sbMenu.Append(string.Format("<li><div class=\"row\"><div class=\"col-md-10\"><a data-location=\"2\" data-viewname=\"{1}\" data-viewid=\"query\" onClick=\"obJmyquery.LoadSaveQuery(this,'{0}_{4}_{5}')\">{1}</a></div><div class=\"col-md-2\"><i onclick=\"obJmyquery.DeleteQuery(this, {4})\" style=\"margin-top:11px;margin-left: -6px;\" class=\"fa fa-trash-o\"></div></div></i>", V.ViewId, V.SavedName, strLId, _ALId.ToString(), V.Id, V.SavedType));
                            }

                            _ALId += 1;
                        }
                    }
                    else
                    {
                        // Dim SavedCriteriaSplitValues = Request.Cookies("SavedCriteriaType").Value.ToString().Trim().Split("_")
                        // SavedCriteria.DeleteSavedCriteria(V.Id, SavedCriteriaSplitValues(1))
                    }
                }
            }
            _ALId += 1;
        }
        private void BindUserSubMenu(List<TableItem> subMenu, ref StringBuilder sbMenu, string parentIdNo)
        {
            foreach (var table in subMenu)
            {
                var lstViewItems = GetViewsByTableName(table.TableName, _passport);
                if (lstViewItems.Count > 0)
                {
                    sbMenu.Append(string.Format("<li><a href='#' id='A{0}{1}'>{2}</a>", parentIdNo, _ALId, table.UserName));
                    sbMenu.Append("<ul>");

                    foreach (var V in lstViewItems)
                    {
                        sbMenu.Append(string.Format("<li><a  name=\"viewAccess\" data-viewname=\"{1}\" data-location=\"0\" data-viewid=\"{0}\"  onClick=\"obJgridfunc.LoadView(this,'{0}')\">{1}</a></li>", V.Id, V.ViewName, parentIdNo, _ALId.ToString(), _FALId.ToString()));
                        _FALId += 1;
                    }
                    sbMenu.Append("</ul></li>");
                }
                _ALId += 1;
            }
        }
        private void HandleAdminMenu()
        {
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            IRepository<ImportField> _iImportField = new Repositories<ImportField>();
            LinkAdminSecurityCheck();
            // Check for LABEL MANAGER access
            if (_passport.CheckPermission(Common.SECURE_LABEL_SETUP, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
            {
                LinkLabelManager = string.Format("<a target=\"_blank\" href=\"/LabelManager\">{0}</a>", Languages.get_Translation("LabelManager"));
            }
            else
            {
                LinkLabelManager = "";
            }
            List<KeyValuePair<string, bool>> lstImportLoad = null;
            lstImportLoad = Keys.GetImportDDL(_iImportLoad.All(), _iImportField.All(), _passport);
            bool ImportPermission = _passport.CheckPermission(Common.SECURE_IMPORT_SETUP, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access);
            if (ImportPermission)
            {
                LinkImport = string.Format("<a target=\"_blank\" title=\"Import\" href=\"/Import\">{0}</a>", Languages.get_Translation("Import"));
            }
            else if (lstImportLoad.Count == 0)
            {
                LinkImport = "";
            }
            else
            {
                LinkImport = string.Format("<a target=\"_blank\" title=\"Import\" href=\"/Import\">{0}</a>", Languages.get_Translation("Import"));
            }
            // If _passport.CheckPermission(Common.SECURE_ORPHANS, Smead.Security.SecureObject.SecureObjectType.Orphans, Smead.Security.Permissions.Permission.Index) AndAlso
            // _passport.CheckPermission(Common.SECURE_ORPHANS, Smead.Security.SecureObject.SecureObjectType.Orphans, Smead.Security.Permissions.Permission.View) Then
            // Me.Vault = String.Format("<a target=""_blank"" href=""/Vault"">{0}</a>", Languages.Translation("LabelVault"))
            // End If

            if (_passport.CheckPermission(Common.SECURE_ORPHANS, Smead.Security.SecureObject.SecureObjectType.Orphans, Permissions.Permission.View))
            {
                Vault = string.Format("<a target=\"_blank\" href=\"/Vault\">{0}</a>", Languages.get_Translation("LabelVault"));
            }

            if (_passport.CheckPermission(Common.SECURE_TRACKING, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
            {
                LinkTracking = string.Format("<a target=\"_blank\" title=\"Tracking\" href=\"/BarcodeTracker/Index\">{0}</a>", Languages.get_Translation("Tracking"));
            }
            else
            {
                LinkTracking = "";
            }
            if (_passport.CheckPermission(Common.SECURE_RETENTION_SETUP, Smead.Security.SecureObject.SecureObjectType.Retention, Permissions.Permission.Access))
            {
            }
            // Session("bRetentionPermission") = True
            else
            {
                // Session("bRetentionPermission") = False
            }
            if (_passport.CheckPermission(Common.SECURE_DASHBOARD, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
            {
                LinkLabelDashboard = string.Format("<a href=\"/Dashboard\" target=\"_blank\">{0}</a>", Languages.get_Translation("LabelDashboard"));
            }
            else
            {
                LinkLabelDashboard = "";
            }
            // If _passport.CheckPermission(Common.SECURE_REPORTS, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.View) Then
            Reports = string.Format("<a href=\"/Reports/Index\" target=\"_blank\">{0}</a>", Languages.get_Translation("LabelReports"));
            // End If
        }
        private void LinkAdminSecurityCheck()
        {
            IRepository<Table> iTable = new Repositories<Table>();
            IRepository<View> iView = new Repositories<View>();

            int iCntRpts = CollectionsClass.CheckReportsPermission(iTable.All(), iView.All(), _passport, _httpContext);
            bool mbMgrGroup = _passport.CheckAdminPermission(Permissions.Permission.Access);
            bool bAtLeastOneTablePermission = CollectionsClass.CheckTablesPermission(iTable.All(), mbMgrGroup, _passport, _httpContext);
            bool bAtLeastOneViewPermission = CollectionsClass.CheckViewsPermission(iView.All(), mbMgrGroup, _passport, _httpContext);
            bool testSECURE_SECURITY = _passport.CheckPermission(Common.SECURE_SECURITY, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Permissions.Permission)Enums.PassportPermissions.Access);
            bool testSECURE_SECURITY_USER = _passport.CheckPermission(Common.SECURE_SECURITY_USER, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Permissions.Permission)Enums.PassportPermissions.Access);
            bool testSECURE_REPORT_STYLES = _passport.CheckPermission(Common.SECURE_REPORT_STYLES, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Permissions.Permission)Enums.PassportPermissions.Access);

            if (!mbMgrGroup && !_passport.CheckPermission(Common.SECURE_SECURITY, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Permissions.Permission)Enums.PassportPermissions.Access) && !_passport.CheckPermission(Common.SECURE_SECURITY_USER, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Permissions.Permission)Enums.PassportPermissions.Access) && !_passport.CheckPermission(Common.SECURE_REPORT_STYLES, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Permissions.Permission)Enums.PassportPermissions.Access) && !(iCntRpts != 0) && !bAtLeastOneTablePermission && !bAtLeastOneViewPermission)
            {
                LinkAdmin = "";
            }
            else
            {
                LinkAdmin = string.Format("<a title=\"Admin\" href=\"/Admin\" target=\"_blank\">{0}</a>", Languages.get_Translation("Admin"));
            }
        }
        private void GetClientResourceKeys()
        {
            var pCurrentCulture = Keys.GetCultureCookies;
            var resouceObject = Languages.get_GetValuesByModule(Languages.ResModule.clients, pCurrentCulture(_httpContext).Name);

            // #Region "Add keys in Dictionary"
            var resourceSetCommon = new Dictionary<string, string>();
            resourceSetCommon = Languages.get_GetValuesByModule(Languages.ResModule.common, pCurrentCulture(_httpContext).Name);

            foreach (var resource in resourceSetCommon)
            {
                if (!resouceObject.ContainsKey(Conversions.ToString(resource.Key)))
                {
                    resouceObject.Add(Conversions.ToString(resource.Key), Conversions.ToString(resource.Value));
                }
            }
            resouceObject.Add("cultureKey", pCurrentCulture(_httpContext).Name);
            resouceObject.Add("currentUserName", Keys.CurrentUserName.ToString());
            LanguageCulture = JsonConvert.SerializeObject(resouceObject);
        }
        private void BackgroundStatusNotifications()
        {
            IRepository<SLServiceTask> s_BackgroundStatusNotification = new Repositories<SLServiceTask>();
            int backgroundStatusNotification = s_BackgroundStatusNotification.All().Where(x => x.UserId == _passport.UserId && x.IsNotification == true).ToList().Count;
            if (backgroundStatusNotification > 0)
            {
                BackgroundStatusNotification = backgroundStatusNotification.ToString();
            }
            else
            {
                BackgroundStatusNotification = 0.ToString();
            }
        }
        // execute method API
        public void ExecuteLayout()
        {
            GetClientResourceKeys();
            BindUserAccessMenu();
            HandleAdminMenu();
            BackgroundStatusNotifications();
        }
    }
    // TASTBAR FUNCTIONS RETURN DOM OBJECT 
    public class TasksBar : BaseModel
    {
        public TasksBar(Passport passport)
        {
            _passport = passport;
        }
        public string TaskList { get; set; }
        public string RequestNewButtonLabel { get; set; }
        public string RequestBatchButtonLabel { get; set; }
        public string RequestExceptionButtonLabel { get; set; }
        public string RequestNewButton { get; set; }
        public string imgRequestNewButton { get; set; }
        public string ancRequestNewButton { get; set; }
        public string RequestBatchButton { get; set; }
        public string imgRequestBatchButton { get; set; }
        public string ancRequestBatchButton { get; set; }
        public string RequestExceptionButton { get; set; }
        public string imgRequestExceptionButton { get; set; }
        public string ancRequestExceptionButton { get; set; }

        private void LoadTasks()
        {
            var sbMenu = new StringBuilder();
            foreach (var item in GetTasksMvc(_passport))
            {
                var replaced_item = item.Replace("style='color: blue;'", "");
                sbMenu.Append(string.Format("<li>{0}</li>", replaced_item));
            }
            TaskList = sbMenu.ToString();
        }
        private void GetTaskLightValues()
        {
            RequestNewButtonLabel = Languages.get_Translation("toolTaskLightsNoNewReq");
            RequestNewButton = "0";
            imgRequestNewButton = "/Content/themes/TAB/img/top-action-req-green.png";
            ancRequestNewButton = "";

            RequestBatchButtonLabel = Languages.get_Translation("toolTaskLightsNoBatchReq");
            RequestBatchButton = "0";
            imgRequestBatchButton = "/Content/themes/TAB/img/top-action-req-green.png";
            ancRequestBatchButton = "";

            RequestExceptionButtonLabel = Languages.get_Translation("toolTaskLightsNoRequestExcep");
            RequestExceptionButton = "0";
            imgRequestExceptionButton = "/Content/themes/TAB/img/top-action-req-green.png";
            ancRequestExceptionButton = "";

            var lstTask = Navigation.GetTaskLightValues(_passport);
            if (lstTask is null)
                return;

            if (lstTask[0] > 0)
            {
                RequestNewButtonLabel = Languages.get_Translation("toolTaskLightsNewReqCount");
                RequestNewButton = lstTask[0].ToString();
                imgRequestNewButton = "/Content/themes/TAB/img/top-action-req-red.png";
                ancRequestNewButton = "/Reports/Index/newRequest";
            }

            if (lstTask[1] > 0)
            {
                RequestBatchButtonLabel = Languages.get_Translation("toolTaskLightsBatchReqCount");
                RequestBatchButton = lstTask[1].ToString();
                imgRequestBatchButton = "/Content/themes/TAB/img/top-action-req-red.png";
                ancRequestBatchButton = "/handler.aspx?r=NewBatchesReport&requesting=1";
            }

            if (lstTask[2] > 0)
            {
                RequestExceptionButtonLabel = Languages.get_Translation("toolTaskLightsRequestExcepCount");
                RequestExceptionButton = lstTask[2].ToString();
                imgRequestExceptionButton = "/Content/themes/TAB/img/top-action-req-red.png";
                ancRequestExceptionButton = "/Reports/Index/exceptions";
            }
        }
        // Execute method API
        public void ExecuteTasksbar()
        {
            LoadTasks();
            GetTaskLightValues();
        }

        public void ExecuteDashboardTasksbar()
        {
            LoadTasks();
        }

    }
    // NEWS FEED FUNCTIONS
    public class NewsFeed : BaseModel
    {
        public NewsFeed(Passport passport)
        {
            _passport = passport;
            LstBlockHtml = new List<string>();
        }

        public string newsURL { get; set; }
        public string TitleNews { get; set; }
        public string BlockHtml { get; set; }
        public List<string> LstBlockHtml { get; set; }
        public int IsTabNewsFeed { get; set; }
        public string UrlNewsFeed { get; set; }
        public bool isAdmin { get; set; }
        public bool isDisplay { get; set; }
        [JsonIgnore]
        public IConfiguration configuration { get; set; }

        private void LoadNews()
        {
            string Linkfeed = GetSetting("News", "NewsURL", _passport);
            string LinkSetting = configuration["NewsURL"];

            //string LinkSetting = ConfigurationManager.AppSettings["NewsURL"];
            isAdmin = _passport.IsAdmin();
            if (GetSetting("News", "Display", _passport).ToLower() == "true" || GetSetting("News", "Display", _passport).ToLower() == "1")
            {
                isDisplay = true;
            }
            else
            {
                isDisplay = false;
            }
            if (Linkfeed.Length > 0)
            {
                if (Linkfeed.Contains("recordsmanagement.tab.com"))
                {
                    newsURL = Linkfeed;
                    ReadXmlFeed();
                    TitleNews = Languages.get_Translation("lblTABNews");
                    IsTabNewsFeed = 1;
                    UrlNewsFeed = Linkfeed;
                }
                else
                {
                    TitleNews = Languages.get_Translation("lblDataNewsFeed");
                    UrlNewsFeed = Linkfeed;
                    IsTabNewsFeed = 0;
                }
            }
            else if (LinkSetting.Length > 0)
            {
                if (LinkSetting.Contains("recordsmanagement.tab.com"))
                {
                    newsURL = LinkSetting;
                    ReadXmlFeed();
                    TitleNews = Languages.get_Translation("lblTABNews");
                    IsTabNewsFeed = 1;
                    UrlNewsFeed = LinkSetting;
                }
                else
                {
                    TitleNews = Languages.get_Translation("lblDataNewsFeed");
                    UrlNewsFeed = LinkSetting;
                    IsTabNewsFeed = 0;
                }
            }
        }
        private void ReadXmlFeed()
        {
            bool rssFeedError = false;
            try
            {
                var rssSettings = new XmlReaderSettings();
                rssSettings.DtdProcessing = DtdProcessing.Ignore;
                using (var rssStream = WebRequest.Create(newsURL).GetResponse().GetResponseStream())
                {
                    var rssDocument = new XmlDocument();
                    using (var rssReader = XmlReader.Create(rssStream, rssSettings))
                    {
                        rssDocument.Load(rssReader);
                    }
                    using (var rssList = rssDocument.SelectNodes("rss/channel/item"))
                    {
                        rssFeedError = rssList.Count == 0;
                        if (!rssFeedError)
                        {
                            for (int i = 0, loopTo = rssList.Count - 1; i <= loopTo; i++)
                            {
                                string title = string.Empty;
                                string link = string.Empty;
                                string description = string.Empty;
                                title = rssList.Item(i).SelectSingleNode("title").InnerText;
                                link = rssList.Item(i).SelectSingleNode("link").InnerText;
                                description = rssList.Item(i).SelectSingleNode("description").InnerText;
                                BlockHtml = string.Format("<span style='margin: 0px; font-size:18px; padding: 0px;'><a style='font-size:18px;' href='{0}' target='new'>{1}</a></span><p align='justify' style='color: gray;'>{2}</p>", link, title, description);
                                LstBlockHtml.Add(BlockHtml);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SlimShared.logInformation(string.Format("Error {0} occurred in Data.LoadNews; feed should still display correctly.", ex.Message));
                rssFeedError = true;
            }

            if (rssFeedError)
            {
                // divNews.Attributes.Add("style", "width: 100%; border: 1px solid #666666; background-color: White; padding: 5px 5px 20px 5px;")
                // NewsFrame.Attributes.Add("src", newsURL)
            }
        }
        // execute method API
        public void ExecuteNewsFeed()
        {
            LoadNews();
        }
    }
    // FOOTER FUNCTIONS
    public class Footer : BaseModel
    {
        public Footer(Passport passport, HttpContext httpContext)
        {
            _passport = passport;
            _httpContext = httpContext;
        }
        public string LblAttempt { get; set; }
        public string LblService { get; set; }
        public string LblServiceVer { get; set; }
        private void GetFooter()
        {
            LblAttempt = GetLastLoginFailedAndAttempt();
            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var copyright = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
            LblService = string.Format(Languages.get_Translation("Copyright"), (object)DateTime.Now.ToUniversalTime().Year, version);
            LblServiceVer = string.Format(version);
        }

        private string GetLastLoginFailedAndAttempt()
        {
            string strLastLogin = "";
            if (_passport.UserName is not null)
            {
                if (Keys.LastLoginAttempt is null)
                {
                    try
                    {
                        IRepository<SLAuditFailedLogin> oSLAuditFailedLogin = new Repositories<SLAuditFailedLogin>();
                        IRepository<SLAuditLogin> oSLAuditLogin = new Repositories<SLAuditLogin>();
                        var lSLAuditLogin = oSLAuditLogin.All().OrderByDescending(x => x.Id).Take(2);
                        var oSLAuditLoginLast = lSLAuditLogin.OrderByDescending(x => x.Id).Take(1).FirstOrDefault();
                        var LastLoginDate = Interaction.IIf(oSLAuditLoginLast is not null, oSLAuditLoginLast.LoginDateTime, DateTime.Now);
                        var oSLAuditLoginPrevLast = lSLAuditLogin.OrderBy(x => x.Id).Take(1).FirstOrDefault();
                        var prevLastLoginDate = Interaction.IIf(oSLAuditLoginPrevLast is not null, oSLAuditLoginPrevLast.LoginDateTime, DateTime.Now);
                        var lAll = oSLAuditFailedLogin.All().ToList();
                        var lSLAuditFailedLogin = lAll.Where(x => Operators.ConditionalCompareObjectGreater(x.LoginDateTime, prevLastLoginDate, false) && Operators.ConditionalCompareObjectLess(x.LoginDateTime, LastLoginDate, false)).ToList();
                        Keys.LastLoginDate = Conversions.ToDate(prevLastLoginDate);
                        Keys.LastLoginAttempt = lSLAuditFailedLogin.Count;
                    }
                    catch (Exception) { }
                }
                strLastLogin = string.Format(Languages.get_Translation("lblSignInLastLoginAndAttempt"), Keys.get_ConvertCultureDate(Keys.LastLoginDate.ToString(), _httpContext, bDetectTime: true), (object)Keys.LastLoginAttempt, Keys.LastLoginUser);
            }
            return strLastLogin;
        }

        // execute method API
        public void ExecuteFooter()
        {
            GetFooter();
        }
    }
}
