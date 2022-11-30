using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Smead.RecordsManagement;
using static Smead.RecordsManagement.Navigation;
using static Smead.RecordsManagement.Query;
using static Smead.RecordsManagement.Retention;
using Smead.Security;
using TabFusionRMS.Resource;
using Newtonsoft.Json.Linq;
using TabFusionRMS;
using System.Reflection;

namespace TabFusionRMS.WebCS
{
    // REPORTING 
    public class ReportingMenu : BaseModel
    {
        public ReportingMenu(Passport passport, HttpContext httpContext)
        {
            this._passport = passport;
            _httpContext = httpContext;
        }
        public string AccessMenu { get; set; }
        public string dateFormat { get; set; }
        public PagingModel Paging { get; set; } = new PagingModel();
        public void GenerateMenu()
        {
            var sbMenu = new StringBuilder();
            if (CheckPermssionsReportTab())
            {
                sbMenu.Append("<ul class='drillDownMenu'><li>");
                sbMenu.Append(string.Format("<a href='#'><i class='font_icon theme_color fa fa-database'></i>{0}</a>", Languages.get_Translation("Reports")));
                sbMenu.Append("<ul>");
                CreateHtml(sbMenu);
                sbMenu.Append("</ul>");
                sbMenu.Append("</li></ul>");
            }
            dateFormat = Keys.GetUserPreferences.sPreferedDateFormat.ToString().Trim().ToUpper();
        }
        public void CreateHtml(StringBuilder sbMenu)
        {
            try
            {
                sbMenu.Append(string.Format("<li><a href='#'>{0}</a>", Languages.get_Translation("lblReportsLandingCustomReports")));
                var customReports = Navigation.GetViewReports(this._passport);
                sbMenu.Append("<ul>");

                foreach (var report in customReports)
                {
                    if (this._passport.CheckPermission(report.ViewName, SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                    {
                        sbMenu.Append(string.Format("<li><a onClick=\"reports.LoadCustomReport(this,'{1}')\">{0}</a></li>", report.ViewName, report.Id));
                    }
                }

                sbMenu.Append("</ul></li>");
                if (this._passport.CheckPermission(" Auditing", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                {
                    sbMenu.Append(string.Format("<li><a onClick=\"reports.LoadAuditReport(this)\">{0}</a>", Languages.get_Translation("mnuWorkGroupMenuControlAuditReport")));
                }
                if (this._passport.CheckPermission(" Tracking", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                {
                    sbMenu.Append(string.Format("<li><a href='#'>{0}</a>", Languages.get_Translation("mnuWorkGroupMenuControlTraRepo")));
                    sbMenu.Append("<ul>");
                    sbMenu.Append(string.Format("<li><a id='FA{0}' onClick=\"reports.TrackableReport(this, 0, 1)\">{0}</a></li>", Languages.get_Translation("mnuWorkGroupMenuControlPastDue")));
                    sbMenu.Append(string.Format("<li><a id='FA{0}' onClick=\"reports.TrackableReport(this, 1, 1)\">{0}</a></li>", Languages.get_Translation("mnuWorkGroupMenuControlObjOut")));
                    sbMenu.Append(string.Format("<li><a href='#'\">{0}</a>", Languages.get_Translation("mnuWorkGroupMenuControlObjInven")));
                    sbMenu.Append("<ul>");

                    foreach (DataRow row in Tracking.GetTrackableTables(this._passport).Rows)
                        sbMenu.Append(string.Format("<li><a id='FA{0}' onClick=\"reports.TrackableReport(this,'{1}',1)\">{0}</a></li>", row["UserName"].ToString(), row["TableName"].ToString()));

                    sbMenu.Append("</ul></li>");
                    sbMenu.Append("</ul></li>");
                }

                if (this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                {

                    sbMenu.Append(string.Format("<li><a href='#'>{0}</a>", Languages.get_Translation("mnuWorkGroupMenuControlReqRepo")));
                    sbMenu.Append("<ul>");

                    sbMenu.Append(string.Format("<li><a id=\"newrequest\" onClick=\"reports.RequestorReport(this, 'new', 1)\">{0}</a></li>", Languages.get_Translation("mnuWorkGroupMenuControlNewReq")));

                    // sbMenu.Append(String.Format("<li><a  onClick=""reports.RequestorReport(this, 'newbatch', 1)"">{0}</a></li>", Languages.Translation("mnuWorkGroupMenuControlNewBatch")))

                    sbMenu.Append(string.Format("<li><a  onClick=\"reports.RequestorReport(this, 'pulllist', 1)\">{0}</a></li>", Languages.get_Translation("mnuWorkGroupMenuControlPullLists")));

                    sbMenu.Append(string.Format("<li><a id=\"exceptions\" onClick=\"reports.RequestorReport(this, 'exception', 1)\">{0}</a></li>", Languages.get_Translation("mnuWorkGroupMenuControlExcep")));

                    sbMenu.Append(string.Format("<li><a  onClick=\"reports.RequestorReport(this, 'inprocess', 1)\">{0}</a></li>", Languages.get_Translation("mnuWorkGroupMenuControlInPro")));

                    sbMenu.Append(string.Format("<li><a  onClick=\"reports.RequestorReport(this, 'waitlist', 1)\">{0}</a></li>", Languages.get_Translation("mnuWorkGroupMenuControlWaitList")));

                    sbMenu.Append("</ul></li>");
                }

                if (this._passport.CheckPermission(" Retention", SecureObject.SecureObjectType.Reports, Permissions.Permission.View) & this._passport.CheckLicense(SecureObject.SecureObjectType.Retention))
                {
                    bool turnOffCitation = Conversions.ToBoolean(Navigation.GetSystemSetting("RetentionTurnOffCitations", this._passport));

                    sbMenu.Append(string.Format("<li><a href='#'>{0}</a>", Languages.get_Translation("RetentionReports")));
                    sbMenu.Append("<ul>");

                    // 'Adding permission
                    if (this._passport.CheckPermission("Disposition", SecureObject.SecureObjectType.Retention, Permissions.Permission.Access))
                    {
                        sbMenu.Append(string.Format("<li><a onClick=\"retentionreport.RetentionReport(this, 'finaldisposition', 1)\">{0}</a></li>", Languages.get_Translation("FinalDispositionReport")));
                    }
                    // coninue from here. 

                    sbMenu.Append(string.Format("<li><a onClick=\"retentionreport.RetentionReport(this, 'certifiedisposition', 1)\">{0}</a></li>", Languages.get_Translation("CertificateDispositionReport")));

                    sbMenu.Append(string.Format("<li><a onClick=\"retentionreport.RetentionReport(this, 'inactivepulllist', 1)\">{0}</a></li>", Languages.get_Translation("InactivePullListsReport")));

                    sbMenu.Append(string.Format("<li><a onClick=\"retentionreport.RetentionReport(this, 'inactivereport', 1)\">{0}</a></li>", Languages.get_Translation("InactiveReport")));

                    sbMenu.Append(string.Format("<li><a onClick=\"retentionreport.RetentionReport(this, 'recordonhold', 1)\">{0}</a></li>", Languages.get_Translation("OnHoldReport")));

                    if (!turnOffCitation)
                    {

                        sbMenu.Append(string.Format("<li><a onClick=\"retentionreport.RetentionReport(this, 'citations', 1)\">{0}</a></li>", Languages.get_Translation("CitationReport")));

                        sbMenu.Append(string.Format("<li><a onClick=\"retentionreport.RetentionReport(this, 'citationwithretcode', 1)\">{0}</a></li>", Languages.get_Translation("CitationRetionCodesReport")));
                    }


                    sbMenu.Append(string.Format("<li><a onClick=\"retentionreport.RetentionReport(this, 'codes', 1)\">{0}</a></li>", Languages.get_Translation("RetentionCodesReport")));

                    if (!turnOffCitation)
                    {

                        sbMenu.Append(string.Format("<li><a onClick=\"retentionreport.RetentionReport(this, 'codewithcitations', 1)\">{0}</a></li>", Languages.get_Translation("RetentionCodesCitationReport")));
                    }

                    sbMenu.Append("</ul></li>");
                }
            }
            catch (Exception)
            {
                // call error method
            }

            AccessMenu = sbMenu.ToString();
        }
        private bool CheckPermssionsReportTab()
        {
            if (this._passport.CheckPermission(" Auditing", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                return true;
            if (this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                return true;
            if (this._passport.CheckPermission(" Retention", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                return true;
            if (this._passport.CheckPermission(" Tracking", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                return true;
            return false;
        }
    }
    // Audit reports
    public class AuditReportSearch : BaseModel
    {
        public AuditReportSearch(Passport passport, HttpContext httpContext)
        {
            this._passport = passport;
            _httpContext = httpContext;
            userDDL = new List<DDLprops>();
            objectDDL = new List<DDLprops>();
            ListOfRows = new List<ArrayList>();
            ListOfHeader = new List<string>();
        }
        public PagingModel Paging { get; set; } = new PagingModel();
        public List<DDLprops> userDDL { get; set; }
        public List<DDLprops> objectDDL { get; set; }
        public string SubTitle { get; set; }
        public List<ArrayList> ListOfRows { get; set; }
        public List<string> ListOfHeader { get; set; }
        public string dateFormat { get; set; }

        private void BindUserDDL()
        {
            var bReturnAllUsers = default(bool);
            string sSQL;
            var dsSettings = new DataSet();
            var dsUser = new DataTable();
            if (this._passport.UsingActiveDirectory)
            {
                sSQL = "SELECT UserName, ISNULL(DisplayName,'<unknown user>') AS DispName FROM SecureUser WHERE AccountType = 'A'";
            }
            else if (this._passport.UsingAzureActiveDirectory)
            {
                sSQL = "SELECT UserName, ISNULL(DisplayName,'<unknown user>') AS DispName FROM SecureUser WHERE AccountType = 'Z'";
            }
            else
            {
                sSQL = "SELECT UserName, ISNULL(DisplayName,'<unknown user>') AS DispName FROM SecureUser WHERE AccountType = 'S'";
            }

            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand("select * from Settings where Section='AuditReport' and Item='ReturnAll'", conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dsSettings);
                    }
                }
            }

            if (dsSettings is not null)
            {
                if (dsSettings.Tables.Count > 0 && dsSettings.Tables[0].Rows.Count > 0)
                {

                    if (Convert.ToInt32(dsSettings.Tables[0].Rows[0]["Id"]) != 0 & Convert.ToString(dsSettings.Tables[0].Rows[0]["ItemValue"]).Length > 0)
                    {
                        bReturnAllUsers = Conversions.ToBoolean(Convert.ToString(dsSettings.Tables[0].Rows[0]["ItemValue"]));
                    }
                }
                if (!bReturnAllUsers)
                {
                    sSQL = sSQL + " AND UserName IN " + Constants.vbCrLf;
                    sSQL = sSQL + " (SELECT DISTINCT [OperatorsId] FROM [SLAuditUpdates] UNION" + Constants.vbCrLf;
                    sSQL = sSQL + "  SELECT DISTINCT [OperatorsId] FROM [SLAuditConfData] UNION" + Constants.vbCrLf;
                    sSQL = sSQL + "  SELECT DISTINCT [OperatorsId] FROM [SLAuditLogins]" + Constants.vbCrLf;
                    sSQL = sSQL + "  WHERE [OperatorsId] IS NOT NULL AND [OperatorsId] > '')";
                }
            }

            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sSQL, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dsUser);
                    }
                }
            }
            userDDL.Add(new DDLprops() { text = "All Users", value = -1 });
            int ddlOrder = 0;
            foreach (DataRow row in dsUser.Rows)
            {
                userDDL.Add(new DDLprops() { text = row["UserName"].ToString(), value = ddlOrder });
                ddlOrder = ddlOrder + 1;
            }
        }
        private void BindTableDDL()
        {
            string sSQL;
            var dsObject = new DataTable();
            sSQL = "select UserName,TableName +'|'+cast((select COUNT(*) from [dbo].[FNGetChildTables](TableName))AS varchar(20)) as ObjectValue from Tables where TableName = 'true' or AuditUpdate = 'true' or AuditConfidentialData = 'true'";
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sSQL, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dsObject);
                    }
                }
            }
            objectDDL.Add(new DDLprops() { text = "Select Table", valuetxt = "-1" });
            objectDDL.Add(new DDLprops() { text = "All Tables", valuetxt = "All" });
            foreach (DataRow row in dsObject.Rows)
            {
                bool isIdstring = Navigation.FieldIsAString(row["UserName"].ToString(), this._passport);
                objectDDL.Add(new DDLprops() { text = row["UserName"].ToString(), valuetxt = row["ObjectValue"].ToString(), isIdstring = isIdstring });
            }

        }
        public void loadDialogReportSearch()
        {
            BindUserDDL();
            BindTableDDL();
            dateFormat = Keys.GetUserPreferences.sPreferedDateFormat.ToString().Trim().ToUpper();
        }
        public void RunQuery(UIproperties UIparam)
        {
            var dt = new DataTable();
            using (var cmd = new SqlCommand(BuildSqlQuery(UIparam).ToString(), this._passport.Connection()))
            {
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            GenerateData(dt);
            GenerateHeader();
        }
        public void RunQuery(UIproperties UIparam, bool count)
        {
            var dt = new DataTable();
            using (var cmd = new SqlCommand(BuildSqlQuery(UIparam, count).ToString(), this._passport.Connection()))
            {
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            if (count)
            {
                if (dt.Rows.Count > 0)
                {
                    Paging.Execute(Conversions.ToInteger(dt.Rows[0][0]));
                }
                else
                {
                    Paging.Execute(0);
                }
            }
            else
            {
                GenerateData(dt);
                GenerateHeader();
            }

        }
        public void RunQueryCount(UIproperties UIparam)
        {
            var dt = new DataTable();
            using (var cmd = new SqlCommand(BuildSqlQuery(UIparam).ToString(), this._passport.Connection()))
            {
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            GenerateData(dt);
            GenerateHeader();
        }
        private void GenerateData(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                var cell = new ArrayList();
                cell.Add(row["TableName"].ToString());
                cell.Add(row["Object Description"].ToString());
                cell.Add(row["AccessDateTime"].ToString());
                cell.Add(row["AuditOperatorsId"].ToString());
                cell.Add(row["Module"].ToString());
                cell.Add(row["Action"].ToString());
                cell.Add(row["DataBefore"].ToString());
                cell.Add(row["DataAfter"].ToString());
                cell.Add(row["Domain"].ToString());
                cell.Add(row["NetworkLoginName"].ToString());
                cell.Add(row["ComputerName"].ToString());
                cell.Add(row["IP"].ToString());
                cell.Add(row["MacAddress"].ToString());
                ListOfRows.Add(cell);
            }
        }
        private void GenerateHeader()
        {
            ListOfHeader.Add(Languages.get_Translation("auditHeaderTable"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderDescription"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderDateTime"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderUser"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderFunction"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderAction"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderBeforeData"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderAfterData"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderDomain"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderNetworkUserName"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderComoputerName"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderIp"));
            ListOfHeader.Add(Languages.get_Translation("auditHeaderMac"));
        }
        private StringBuilder BuildSqlQuery(UIproperties UIparam)
        {
            var param = new Parameters(this._passport);
            // get subtitle
            GetSubTitle(UIparam);
            // format the date before sending to query
            UIparam.StartDate = UIparam.StartDate + " 00:00:00";
            UIparam.EndDate = UIparam.EndDate + " 23:59:59";
            // build string objective
            var sqlQuery = new StringBuilder();
            var sqlWrapper = new StringBuilder();
            string sqlId = string.Empty;
            sqlQuery.Append("Select * START_FROM_TOKEN (");
            sqlId = Getid(UIparam);
            if (UIparam.AddEditDelete)
            {
                if (!UIparam.ChildTable)
                {
                    ChkChildTableFalse(UIparam, sqlQuery, sqlId);
                }
                else
                {
                    ChkChildTableTrue(UIparam, sqlQuery, sqlId);
                }

                if (UIparam.UserDDLId != "-1")
                    sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditUpdates.[OperatorsId] = '{0}') ", UIparam.UserName));
                sqlQuery.Append(Environment.NewLine);
            }
            if (UIparam.SuccessLogin)
            {
                ChkSuccessLoginTrue(UIparam, sqlQuery, sqlId);
            }
            if (UIparam.ConfDataAccess)
            {
                ChkConfDataAccessTrue(UIparam, sqlQuery, sqlId);
            }
            if (UIparam.FailedLogin)
            {
                ChkloginFailedTrue(UIparam, sqlQuery, sqlId);
            }
            EndSqlWrapper(UIparam, sqlQuery, sqlWrapper, param);
            return sqlWrapper;
        }

        private StringBuilder BuildSqlQuery(UIproperties UIparam, bool count)
        {
            var param = new Parameters(this._passport);

            try
            {
                param.PageIndex = Convert.ToInt32(UIparam.PageNumber);
            }
            catch (Exception)
            {
                param.PageIndex = 1;
            }

            // get subtitle
            GetSubTitle(UIparam);
            // format the date before sending to query
            UIparam.StartDate = Keys.get_ConvertCultureDate(UIparam.StartDate, "E", _httpContext);
            UIparam.EndDate = Keys.get_ConvertCultureDate(UIparam.EndDate, "E", _httpContext);

            UIparam.StartDate = UIparam.StartDate + " 00:00:00";
            UIparam.EndDate = UIparam.EndDate + " 23:59:59";
            // build string objective
            var sqlQuery = new StringBuilder();
            var sqlWrapper = new StringBuilder();
            string sqlId = string.Empty;
            sqlQuery.Append("Select * START_FROM_TOKEN (");
            sqlId = Getid(UIparam);
            if (UIparam.AddEditDelete)
            {
                if (!UIparam.ChildTable)
                {
                    ChkChildTableFalse(UIparam, sqlQuery, sqlId, count);
                }
                else
                {
                    ChkChildTableTrue(UIparam, sqlQuery, sqlId, count);
                }

                if (UIparam.UserDDLId != "-1")
                    sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditUpdates.[OperatorsId] = '{0}') ", UIparam.UserName));
                sqlQuery.Append(Environment.NewLine);
            }
            if (UIparam.SuccessLogin)
            {
                ChkSuccessLoginTrue(UIparam, sqlQuery, sqlId, count);
            }
            if (UIparam.ConfDataAccess)
            {
                ChkConfDataAccessTrue(UIparam, sqlQuery, sqlId, count);
            }
            if (UIparam.FailedLogin)
            {
                ChkloginFailedTrue(UIparam, sqlQuery, sqlId, count);
            }
            EndSqlWrapper(UIparam, sqlQuery, sqlWrapper, param, count);
            return sqlWrapper;
        }
        private string Getid(UIproperties UIparam)
        {
            string sqlId = string.Empty;
            if (!string.IsNullOrEmpty(UIparam.Id))
            {
                if (UIparam.Id.Trim().Length > 0 && ObjectIdIsSingleTable(UIparam.ObjectId))
                {
                    if (Navigation.FieldIsAString(GetTableNameFromObjectId(UIparam.ObjectId), this._passport.Connection()))
                    {
                        sqlId = string.Format(" ({0}.TableId = '{1}') AND ", "{0}", UIparam.Id.Trim());
                    }
                    else
                    {
                        sqlId = string.Format(" ({0}.TableId = RIGHT('000000000000000000000000000000' + CAST({1} AS VARCHAR(30)), 30)) AND ", "{0}", UIparam.Id.Trim());
                    }
                }
            }
            return sqlId;
        }
        private void ChkChildTableFalse(UIproperties UIparam, StringBuilder sqlQuery, string sqlId)
        {
            sqlQuery.Append(string.Format("SELECT [Id], [TableName], (dbo.fnGetObjectDescription([TableName],[TableId])) AS 'Object Description', " + Environment.NewLine + "[TableId], ISNULL(SecureUser.DisplayName, OperatorsId) AS AuditOperatorsId, CASE WHEN NetworkLoginName = '' THEN '[unknown user]' ELSE NetworkLoginName END AS [NetworkLoginName], " + Environment.NewLine + "[Domain], [ComputerName], [MacAddress], [IP], [UpdateDateTime] AS [AccessDateTime], " + Environment.NewLine + "[Module], [Action], CAST([DataBefore] AS VARCHAR(8000)) AS DataBefore, CAST([DataAfter] AS VARCHAR(8000)) AS DataAfter " + Environment.NewLine + "FROM SLAuditUpdates LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditUpdates.OperatorsId " + Environment.NewLine + "WHERE {2}" + Environment.NewLine + "(([UpdateDateTime] >= '{0}') AND ([UpdateDateTime] <= '{1}')) ", UIparam.StartDate, UIparam.EndDate, string.Format(sqlId, "SLAuditUpdates")));
            if (ObjectIdIsSingleTable(UIparam.ObjectId))
            {
                if (!(UIparam.ObjectId == "All"))
                {
                    sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditUpdates.TableName = '{0}') ", GetTableNameFromObjectId(UIparam.ObjectId)));
                }
            }
        }
        private void ChkChildTableFalse(UIproperties UIparam, StringBuilder sqlQuery, string sqlId, bool count)
        {
            if (count)
            {
                sqlQuery.Append(string.Format("SELECT COUNT(*) As Total FROM SLAuditUpdates LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditUpdates.OperatorsId " + Environment.NewLine + "WHERE {2}" + Environment.NewLine + "(([UpdateDateTime] >= '{0}') AND ([UpdateDateTime] <= '{1}')) ", UIparam.StartDate, UIparam.EndDate, string.Format(sqlId, "SLAuditUpdates")));
            }
            else
            {
                sqlQuery.Append(string.Format("SELECT [Id], [TableName], (dbo.fnGetObjectDescription([TableName],[TableId])) AS 'Object Description', " + Environment.NewLine + "[TableId], ISNULL(SecureUser.DisplayName, OperatorsId) AS AuditOperatorsId, CASE WHEN NetworkLoginName = '' THEN '[unknown user]' ELSE NetworkLoginName END AS [NetworkLoginName], " + Environment.NewLine + "[Domain], [ComputerName], [MacAddress], [IP], [UpdateDateTime] AS [AccessDateTime], " + Environment.NewLine + "[Module], [Action], CAST([DataBefore] AS VARCHAR(8000)) AS DataBefore, CAST([DataAfter] AS VARCHAR(8000)) AS DataAfter " + Environment.NewLine + "FROM SLAuditUpdates LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditUpdates.OperatorsId " + Environment.NewLine + "WHERE {2}" + Environment.NewLine + "(([UpdateDateTime] >= '{0}') AND ([UpdateDateTime] <= '{1}')) ", UIparam.StartDate, UIparam.EndDate, string.Format(sqlId, "SLAuditUpdates")));
            }

            if (ObjectIdIsSingleTable(UIparam.ObjectId))
            {
                if (!(UIparam.ObjectId == "All"))
                {
                    sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditUpdates.TableName = '{0}') ", GetTableNameFromObjectId(UIparam.ObjectId)));
                }
            }
        }
        private void ChkChildTableTrue(UIproperties UIparam, StringBuilder sqlQuery, string sqlId)
        {
            sqlQuery.Append(string.Format("SELECT SLAuditUpdates.[Id], SLAuditUpdates.[TableName], (dbo.fnGetObjectDescription(SLAuditUpdates.[TableName], SLAuditUpdates.[TableId])) AS 'Object Description', " + Environment.NewLine + "SLAuditUpdates.[TableId], ISNULL(SecureUser.DisplayName, OperatorsId) AS AuditOperatorsId, CASE WHEN NetworkLoginName='' THEN '[unknown user]' ELSE NetworkLoginName END AS [NetworkLoginName], " + Environment.NewLine + "[Domain], [ComputerName], [MacAddress], [IP], [UpdateDateTime] AS [AccessDateTime], " + Environment.NewLine + "[Module], [Action], CAST([DataBefore] AS VARCHAR(8000)) AS DataBefore, CAST([DataAfter] AS VARCHAR(8000)) AS DataAfter " + Environment.NewLine + "FROM SLAuditUpdChildren INNER JOIN SLAuditUpdates ON SLAuditUpdChildren.SLAuditUpdatesId = SLAuditUpdates.Id  " + Environment.NewLine + "LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditUpdates.OperatorsId  " + Environment.NewLine + "WHERE {2}" + Environment.NewLine + "(([UpdateDateTime] >= '{0}') AND ([UpdateDateTime] <= '{1}')) ", UIparam.StartDate, UIparam.EndDate, string.Format(sqlId, "SLAuditUpdChildren")));

            if (ObjectIdIsSingleTable(UIparam.ObjectId))
            {
                if (!(UIparam.ObjectId == "All"))
                {
                    sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditUpdChildren.TableName = '{0}') ", GetTableNameFromObjectId(UIparam.ObjectId)));
                }
            }
        }
        private void ChkChildTableTrue(UIproperties UIparam, StringBuilder sqlQuery, string sqlId, bool count)
        {
            if (count)
            {
                sqlQuery.Append(string.Format("SELECT COUNT(*) As Total FROM SLAuditUpdChildren INNER JOIN SLAuditUpdates ON SLAuditUpdChildren.SLAuditUpdatesId = SLAuditUpdates.Id  " + Environment.NewLine + "LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditUpdates.OperatorsId  " + Environment.NewLine + "WHERE {2}" + Environment.NewLine + "(([UpdateDateTime] >= '{0}') AND ([UpdateDateTime] <= '{1}')) ", UIparam.StartDate, UIparam.EndDate, string.Format(sqlId, "SLAuditUpdChildren")));
            }
            else
            {
                sqlQuery.Append(string.Format("SELECT SLAuditUpdates.[Id], SLAuditUpdates.[TableName], (dbo.fnGetObjectDescription(SLAuditUpdates.[TableName], SLAuditUpdates.[TableId])) AS 'Object Description', " + Environment.NewLine + "SLAuditUpdates.[TableId], ISNULL(SecureUser.DisplayName, OperatorsId) AS AuditOperatorsId, CASE WHEN NetworkLoginName='' THEN '[unknown user]' ELSE NetworkLoginName END AS [NetworkLoginName], " + Environment.NewLine + "[Domain], [ComputerName], [MacAddress], [IP], [UpdateDateTime] AS [AccessDateTime], " + Environment.NewLine + "[Module], [Action], CAST([DataBefore] AS VARCHAR(8000)) AS DataBefore, CAST([DataAfter] AS VARCHAR(8000)) AS DataAfter " + Environment.NewLine + "FROM SLAuditUpdChildren INNER JOIN SLAuditUpdates ON SLAuditUpdChildren.SLAuditUpdatesId = SLAuditUpdates.Id  " + Environment.NewLine + "LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditUpdates.OperatorsId  " + Environment.NewLine + "WHERE {2}" + Environment.NewLine + "(([UpdateDateTime] >= '{0}') AND ([UpdateDateTime] <= '{1}')) ", UIparam.StartDate, UIparam.EndDate, string.Format(sqlId, "SLAuditUpdChildren")));

            }

            if (ObjectIdIsSingleTable(UIparam.ObjectId))
            {
                if (!(UIparam.ObjectId == "All"))
                {
                    sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditUpdChildren.TableName = '{0}') ", GetTableNameFromObjectId(UIparam.ObjectId)));
                }
            }
        }
        private void ChkSuccessLoginTrue(UIproperties UIparam, StringBuilder sqlQuery, string sqlId)
        {
            if (UIparam.AddEditDelete)
                sqlQuery.Append("UNION ALL " + Environment.NewLine);
            sqlQuery.Append(string.Format("SELECT [Id], '' AS [TableName], '' AS 'Object Description', '' AS [TableId], ISNULL(SecureUser.DisplayName, OperatorsId) AS AuditOperatorsId, " + Environment.NewLine + "CASE WHEN NetworkLoginName = '' THEN '[unknown user]' ELSE NetworkLoginName END AS [NetworkLoginName], [Domain], [ComputerName], [MacAddress], [IP], " + Environment.NewLine + "[LoginDateTime] AS [AccessDateTime], '' AS [Module], 'Successful Login' AS [Action], '' AS [DataBefore], '' AS [DataAfter]  " + Environment.NewLine + "FROM SLAuditLogins LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditLogins.OperatorsId  " + Environment.NewLine + "WHERE (([LoginDateTime] >= '{0}') AND ([LoginDateTime] <= '{1}'))", UIparam.StartDate, UIparam.EndDate));

            if (UIparam.UserDDLId != "-1")
                sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditLogins.[OperatorsId] = '{0}') ", UIparam.UserName));
            sqlQuery.Append(Environment.NewLine);
        }
        private void ChkSuccessLoginTrue(UIproperties UIparam, StringBuilder sqlQuery, string sqlId, bool count)
        {
            if (UIparam.AddEditDelete)
                sqlQuery.Append("UNION ALL " + Environment.NewLine);
            if (count)
            {
                sqlQuery.Append(string.Format("SELECT COUNT(*) As Total FROM SLAuditLogins LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditLogins.OperatorsId  " + Environment.NewLine + "WHERE (([LoginDateTime] >= '{0}') AND ([LoginDateTime] <= '{1}'))", UIparam.StartDate, UIparam.EndDate));
            }
            else
            {
                sqlQuery.Append(string.Format("SELECT [Id], '' AS [TableName], '' AS 'Object Description', '' AS [TableId], ISNULL(SecureUser.DisplayName, OperatorsId) AS AuditOperatorsId, " + Environment.NewLine + "CASE WHEN NetworkLoginName = '' THEN '[unknown user]' ELSE NetworkLoginName END AS [NetworkLoginName], [Domain], [ComputerName], [MacAddress], [IP], " + Environment.NewLine + "[LoginDateTime] AS [AccessDateTime], '' AS [Module], 'Successful Login' AS [Action], '' AS [DataBefore], '' AS [DataAfter]  " + Environment.NewLine + "FROM SLAuditLogins LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditLogins.OperatorsId  " + Environment.NewLine + "WHERE (([LoginDateTime] >= '{0}') AND ([LoginDateTime] <= '{1}'))", UIparam.StartDate, UIparam.EndDate));
            }

            if (UIparam.UserDDLId != "-1")
                sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditLogins.[OperatorsId] = '{0}') ", UIparam.UserName));
            sqlQuery.Append(Environment.NewLine);

        }
        private void ChkConfDataAccessTrue(UIproperties UIparam, StringBuilder sqlQuery, string sqlId)
        {
            if (UIparam.AddEditDelete | UIparam.SuccessLogin)
                sqlQuery.Append("UNION ALL " + Environment.NewLine);

            sqlQuery.Append(string.Format("SELECT [Id], [TableName], (dbo.fnGetObjectDescription([TableName],[TableId])) AS 'Object Description', " + Environment.NewLine + "[TableId], ISNULL(SecureUser.DisplayName, OperatorsId) AS AuditOperatorsId, CASE WHEN NetworkLoginName='' THEN '[unknown user]' ELSE NetworkLoginName END AS [NetworkLoginName], " + Environment.NewLine + "[Domain], [ComputerName], [MacAddress], [IP], [AccessDateTime], " + Environment.NewLine + "[Module], 'Confidential Data Access' AS [Action], '' AS [DataBefore], '' AS [DataAfter]  " + Environment.NewLine + "FROM SLAuditConfData LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditConfData.OperatorsId " + Environment.NewLine + "WHERE {2} " + Environment.NewLine + "(([AccessDateTime] >= '{0}') AND ([AccessDateTime] <= '{1}'))", UIparam.StartDate, UIparam.EndDate, string.Format(sqlId, "SLAuditUpdates")));
            if (!(UIparam.ObjectId == "All"))
            {
                if (ObjectIdIsSingleTable(UIparam.ObjectId))
                    sqlQuery.Append(Environment.NewLine + string.Format(" AND ([TableName] = '{0}') ", GetTableNameFromObjectId(UIparam.ObjectId)));
            }
            if (UIparam.UserDDLId != "-1")
                sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditConfData.[OperatorsId] = '{0}') ", UIparam.UserName));
            sqlQuery.Append(Environment.NewLine);
        }

        private void ChkConfDataAccessTrue(UIproperties UIparam, StringBuilder sqlQuery, string sqlId, bool count)
        {
            if (UIparam.AddEditDelete | UIparam.SuccessLogin)
                sqlQuery.Append("UNION ALL " + Environment.NewLine);

            if (count)
            {
                sqlQuery.Append(string.Format("SELECT COUNT(*) As Total FROM SLAuditConfData LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditConfData.OperatorsId " + Environment.NewLine + "WHERE {2} " + Environment.NewLine + "(([AccessDateTime] >= '{0}') AND ([AccessDateTime] <= '{1}'))", UIparam.StartDate, UIparam.EndDate, string.Format(sqlId, "SLAuditConfData")));
            }
            else
            {
                sqlQuery.Append(string.Format("SELECT [Id], [TableName], (dbo.fnGetObjectDescription([TableName],[TableId])) AS 'Object Description', " + Environment.NewLine + "[TableId], ISNULL(SecureUser.DisplayName, OperatorsId) AS AuditOperatorsId, CASE WHEN NetworkLoginName='' THEN '[unknown user]' ELSE NetworkLoginName END AS [NetworkLoginName], " + Environment.NewLine + "[Domain], [ComputerName], [MacAddress], [IP], [AccessDateTime], " + Environment.NewLine + "[Module], 'Confidential Data Access' AS [Action], '' AS [DataBefore], '' AS [DataAfter]  " + Environment.NewLine + "FROM SLAuditConfData LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditConfData.OperatorsId " + Environment.NewLine + "WHERE {2} " + Environment.NewLine + "(([AccessDateTime] >= '{0}') AND ([AccessDateTime] <= '{1}'))", UIparam.StartDate, UIparam.EndDate, string.Format(sqlId, "SLAuditConfData")));
            }


            if (!(UIparam.ObjectId == "All"))
            {
                if (ObjectIdIsSingleTable(UIparam.ObjectId))
                    sqlQuery.Append(Environment.NewLine + string.Format(" AND ([TableName] = '{0}') ", GetTableNameFromObjectId(UIparam.ObjectId)));
            }
            if (UIparam.UserDDLId != "-1")
                sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditConfData.[OperatorsId] = '{0}') ", UIparam.UserName));
            sqlQuery.Append(Environment.NewLine);
        }
        private void ChkloginFailedTrue(UIproperties UIparam, StringBuilder sqlQuery, string sqlId)
        {
            if (UIparam.AddEditDelete | UIparam.SuccessLogin | UIparam.ConfDataAccess)
                sqlQuery.Append(Environment.NewLine + " UNION ALL " + Environment.NewLine);

            sqlQuery.Append(string.Format("SELECT [Id], '' AS 'TableName', '' AS 'Object Description', " + Environment.NewLine + "'' AS [TableId], ISNULL(SecureUser.DisplayName, OperatorsId) AS AuditOperatorsId, CASE WHEN NetworkLoginName= '' THEN '[unknown user]' ELSE NetworkLoginName END AS [NetworkLoginName], " + Environment.NewLine + "[Domain], [ComputerName], [MacAddress], [IP], [LoginDateTime] AS [AccessDateTime], " + Environment.NewLine + "'' AS [Module], '" + Languages.get_Translation("ARF_lblFailedLogin") + ": ' + [ReasonForFailure] AS [Action], CASE WHEN SecureUser.DisplayName IS NULL THEN [TextEntered] ELSE '' END AS [DataBefore],'' AS [DataAfter] " + Environment.NewLine + "FROM SLAuditFailedLogins LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditFailedLogins.OperatorsId " + Environment.NewLine + "WHERE (([LoginDateTime] >= '{0}') AND ([LoginDateTime] <= '{1}'))", UIparam.StartDate, UIparam.EndDate));

            if (UIparam.UserDDLId != "-1")
                sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditFailedLogins.[OperatorsId] = '{0}') ", UIparam.UserName));
            sqlQuery.Append(Environment.NewLine);
        }
        private void ChkloginFailedTrue(UIproperties UIparam, StringBuilder sqlQuery, string sqlId, bool count)
        {
            if (UIparam.AddEditDelete | UIparam.SuccessLogin | UIparam.ConfDataAccess)
                sqlQuery.Append(Environment.NewLine + " UNION ALL " + Environment.NewLine);

            if (count)
            {
                sqlQuery.Append(string.Format("SELECT COUNT(*) As Total FROM SLAuditFailedLogins LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditFailedLogins.OperatorsId " + Environment.NewLine + "WHERE (([LoginDateTime] >= '{0}') AND ([LoginDateTime] <= '{1}'))", UIparam.StartDate, UIparam.EndDate));
            }
            else
            {
                sqlQuery.Append(string.Format("SELECT [Id], '' AS 'TableName', '' AS 'Object Description', " + Environment.NewLine + "'' AS [TableId], ISNULL(SecureUser.DisplayName, OperatorsId) AS AuditOperatorsId, CASE WHEN NetworkLoginName= '' THEN '[unknown user]' ELSE NetworkLoginName END AS [NetworkLoginName], " + Environment.NewLine + "[Domain], [ComputerName], [MacAddress], [IP], [LoginDateTime] AS [AccessDateTime], " + Environment.NewLine + "'' AS [Module], '" + Languages.get_Translation("ARF_lblFailedLogin") + ": ' + [ReasonForFailure] AS [Action], CASE WHEN SecureUser.DisplayName IS NULL THEN [TextEntered] ELSE '' END AS [DataBefore],'' AS [DataAfter] " + Environment.NewLine + "FROM SLAuditFailedLogins LEFT JOIN SecureUser ON SecureUser.UserName = SLAuditFailedLogins.OperatorsId " + Environment.NewLine + "WHERE (([LoginDateTime] >= '{0}') AND ([LoginDateTime] <= '{1}'))", UIparam.StartDate, UIparam.EndDate));
            }

            if (UIparam.UserDDLId != "-1")
                sqlQuery.Append(Environment.NewLine + string.Format(" AND (SLAuditFailedLogins.[OperatorsId] = '{0}') ", UIparam.UserName));
            sqlQuery.Append(Environment.NewLine);
        }
        private void EndSqlWrapper(UIproperties UIparam, StringBuilder sqlQuery, StringBuilder sqlWrapper, Parameters param)
        {
            sqlQuery.Append(") AS tmp");
            sqlWrapper.Append("DECLARE @cnt INT, @Total INT, @tQuery NVARCHAR(MAX), @tOD VARCHAR(MAX)");
            sqlWrapper.Append(Environment.NewLine);
            string orderClause = " ORDER BY [TableName], TableId, AccessDateTime";

            if (!UIparam.AddEditDelete & !UIparam.SuccessLogin & !UIparam.ConfDataAccess & !UIparam.ChildTable)
                orderClause = " ORDER BY TableId, AccessDateTime";
            if (UIparam.AddEditDelete | UIparam.SuccessLogin | UIparam.ConfDataAccess | UIparam.FailedLogin | UIparam.ChildTable)
            {
                if (param is null || !param.Paged)
                {
                    sqlWrapper.Append(string.Format("SELECT (ROW_NUMBER() OVER ({1})) AS RowNum, overall_count = COUNT(*) OVER(), * INTO #testTemp FROM ({0}) Union_Table ", sqlQuery.Replace(" START_FROM_TOKEN ", " FROM "), orderClause));
                }
                else
                {
                    sqlWrapper.Append(Pagify(sqlQuery.ToString(), orderClause).Replace(" AS RowNum", " AS RowNum, overall_count = COUNT(*) OVER()").Replace("* FROM ", "* INTO #testTemp FROM ").Replace(" START_FROM_TOKEN ", " FROM "));
                }
            }

            if (param is null || !param.Paged)
            {
                sqlWrapper.Append(Environment.NewLine + "SELECT @cnt = 1, @Total = overall_count FROM #testTemp" + Environment.NewLine);
            }
            else
            {
                sqlWrapper.Append(Environment.NewLine + "SELECT @cnt = " + ((param.PageIndex - 1) * param.RequestedRows).ToString() + ", @Total = " + (param.PageIndex * param.RequestedRows).ToString() + " FROM #testTemp" + Environment.NewLine);
            }

            sqlWrapper.Append(Environment.NewLine);
            sqlWrapper.Append(" WHILE (@cnt <= @Total) BEGIN " + Environment.NewLine + "     SELECT @tQuery = [Object Description], @tOD='' FROM #testTemp WHERE RowNum = @cnt" + Environment.NewLine + "     IF (@tQuery <> '') BEGIN " + Environment.NewLine + "         EXEC sp_executesql @tQuery, N'@rVal NVARCHAR(MAX) OUT', @tOD OUT " + Environment.NewLine + "         UPDATE #testTemp SET [Object Description] = (CASE WHEN @tOD = '' OR @tOD IS NULL THEN CAST(CONVERT(INT, TableId) AS NVARCHAR(MAX)) ELSE @tOD END) WHERE RowNum = @cnt " + Environment.NewLine + "     END " + Environment.NewLine + "     SELECT @cnt = @cnt + 1" + Environment.NewLine + " END " + Environment.NewLine + " SELECT * FROM #testTemp" + Environment.NewLine + " DROP TABLE #testTemp");
        }
        private void EndSqlWrapper(UIproperties UIparam, StringBuilder sqlQuery, StringBuilder sqlWrapper, Parameters param, bool count)
        {

            if (count)
            {
                sqlQuery.Append(") AS tmp");
                sqlWrapper.Clear().Append(sqlQuery.Replace("* START_FROM_TOKEN", "sum(Total) FROM"));
            }
            else
            {
                sqlQuery.Append(") AS tmp");
                sqlWrapper.Append("DECLARE @cnt INT, @Total INT, @tQuery NVARCHAR(MAX), @tOD VARCHAR(MAX)");
                sqlWrapper.Append(Environment.NewLine);
                string orderClause = " ORDER BY [TableName], TableId, AccessDateTime";

                if (!UIparam.AddEditDelete & !UIparam.SuccessLogin & !UIparam.ConfDataAccess & !UIparam.ChildTable)
                    orderClause = " ORDER BY TableId, AccessDateTime";
                if (UIparam.AddEditDelete | UIparam.SuccessLogin | UIparam.ConfDataAccess | UIparam.FailedLogin | UIparam.ChildTable)
                {
                    if (param is null || !param.Paged)
                    {
                        sqlWrapper.Append(string.Format("SELECT (ROW_NUMBER() OVER ({1})) AS RowNum, overall_count = COUNT(*) OVER(), * INTO #testTemp FROM ({0}) Union_Table ", sqlQuery.Replace(" START_FROM_TOKEN ", " FROM "), orderClause));
                    }
                    else
                    {
                        sqlWrapper.Append(Pagify(sqlQuery.ToString(), orderClause).Replace(" AS RowNum", " AS RowNum, overall_count = COUNT(*) OVER()").Replace("* FROM ", "* INTO #testTemp FROM ").Replace(" START_FROM_TOKEN ", " FROM "));
                    }
                }

                if (param is null || !param.Paged)
                {
                    sqlWrapper.Append(Environment.NewLine + "SELECT @cnt = 1, @Total = overall_count FROM #testTemp" + Environment.NewLine);
                }
                else
                {
                    sqlWrapper.Append(Environment.NewLine + "SELECT @cnt = " + ((param.PageIndex - 1) * param.RequestedRows).ToString() + ", @Total = " + (param.PageIndex * param.RequestedRows).ToString() + " FROM #testTemp" + Environment.NewLine);
                }

                sqlWrapper.Append(Environment.NewLine);
                sqlWrapper.Append(" WHILE (@cnt <= @Total) BEGIN " + Environment.NewLine + "     SELECT @tQuery = [Object Description], @tOD='' FROM #testTemp WHERE RowNum = @cnt" + Environment.NewLine + "     IF (@tQuery <> '') BEGIN " + Environment.NewLine + "         EXEC sp_executesql @tQuery, N'@rVal NVARCHAR(MAX) OUT', @tOD OUT " + Environment.NewLine + "         UPDATE #testTemp SET [Object Description] = (CASE WHEN @tOD = '' OR @tOD IS NULL THEN CAST(CONVERT(INT, TableId) AS NVARCHAR(MAX)) ELSE @tOD END) WHERE RowNum = @cnt " + Environment.NewLine + "     END " + Environment.NewLine + "     SELECT @cnt = @cnt + 1" + Environment.NewLine + " END " + Environment.NewLine + " SELECT * FROM #testTemp " + orderClause + QueryPaging(param.PageIndex, Paging.PerPageRecord) + Environment.NewLine + " DROP TABLE #testTemp");


            }


        }

        private string GetTableNameFromObjectId(string ObjectId)
        {
            try
            {
                return ObjectId.Split('|')[0].ToString();
            }
            catch (Exception)
            {
                return ObjectId;
            }
        }
        private bool ObjectIdIsSingleTable(string ObjectId)
        {
            try
            {
                return string.Compare(ObjectId.Split('|')[0].ToString(), "Select Table", true) != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private string Pagify(string sql, string orderClause)
        {
            var @params = new Parameters(this._passport);
            string pageClause = "SELECT TOP ";
            if (@params.RequestedRows <= 0)
                @params.RequestedRows = 250;
            pageClause += @params.RequestedRows.ToString() + " * FROM (SELECT ROW_NUMBER() OVER (" + orderClause.Trim() + ") AS RowNum,";
            pageClause = Strings.Replace(sql, "SELECT", pageClause, 1, 1, CompareMethod.Text);
            pageClause += ") AS PagedResult WHERE RowNum > " + ((@params.PageIndex - 1) * @params.RequestedRows).ToString() + " AND RowNum <= " + (@params.PageIndex * @params.RequestedRows).ToString();
            pageClause += " ORDER BY PagedResult.RowNum";
            return pageClause;
        }
        private void GetSubTitle(UIproperties UIparam)
        {
            if (string.IsNullOrEmpty(UIparam.Id))
                UIparam.Id = "";
            string SubTitle = string.Empty;
            SubTitle = "Audit Report for " + UIparam.UserName;
            if (UIparam.ObjectName.Trim() != "Select Object")
            {
                // Get table primary key and DescFieldNameTwo field
                var dsTableName = new DataSet();
                string strDescFieldNameTwo = string.Empty;
                string strPKField = string.Empty;

                using (var conn = this._passport.Connection())
                {
                    using (var cmd = new SqlCommand(string.Format("SELECT (CASE WHEN t.IdFieldName IS NULL THEN '' ELSE (SUBSTRING(t.IdFieldName, CHARINDEX('.', t.IdFieldName) + 1, LEN(t.IdFieldName))) END) AS PKField " + ", t.DescFieldNameTwo FROM Tables t WHERE t.TableName = '{0}'", UIparam.ObjectName.Trim()), conn))
                    {
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dsTableName);
                        }
                    }
                }
                if (dsTableName.Tables.Count > 0)
                {
                    if (dsTableName.Tables[0].Rows.Count > 0)
                    {
                        if (!(dsTableName.Tables[0].Rows[0]["DescFieldNameTwo"] is DBNull))
                            strDescFieldNameTwo = Conversions.ToString(dsTableName.Tables[0].Rows[0]["DescFieldNameTwo"]);
                        if (!(dsTableName.Tables[0].Rows[0]["PKField"] is DBNull))
                            strPKField = Conversions.ToString(dsTableName.Tables[0].Rows[0]["PKField"]);
                    }
                }
                // Get table DescFieldNameTwo field value
                var dsFieldTableName = new DataSet();
                string strDescFieldNameTwoVal = string.Empty;

                if (!string.IsNullOrWhiteSpace(strDescFieldNameTwo))
                {
                    using (var conn = this._passport.Connection())
                    {
                        using (var cmd = new SqlCommand(string.Format("SELECT [{0}] AS DescFieldNameTwo FROM [{1}] WHERE [{2}] = '{3}'", strDescFieldNameTwo, UIparam.ObjectName.Trim(), strPKField, UIparam.Id), conn))
                        {
                            using (var da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(dsFieldTableName);
                            }
                        }
                    }
                    if (dsFieldTableName.Tables.Count > 0)
                    {
                        if (dsFieldTableName.Tables[0].Rows.Count > 0 && !(dsFieldTableName.Tables[0].Rows[0]["DescFieldNameTwo"] is DBNull))
                        {
                            strDescFieldNameTwoVal = Conversions.ToString(dsFieldTableName.Tables[0].Rows[0]["DescFieldNameTwo"]);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(strDescFieldNameTwoVal))
                    strDescFieldNameTwoVal = " - " + strDescFieldNameTwoVal;
                SubTitle += " and " + UIparam.ObjectName + ":" + UIparam.Id + strDescFieldNameTwoVal;
            }

            SubTitle += " from " + UIparam.StartDate + " thru " + UIparam.EndDate;
            this.SubTitle = SubTitle;
        }
        public class UIproperties
        {
            public string UserName { get; set; }
            public string UserDDLId { get; set; }
            public string ObjectId { get; set; }
            public string ObjectName { get; set; }
            public string Id { get; set; }
            public string StartDate { get; set; }
            public string EndDate { get; set; }
            public bool AddEditDelete { get; set; } = false;
            public bool SuccessLogin { get; set; } = false;
            public bool ConfDataAccess { get; set; } = false;
            public bool FailedLogin { get; set; } = false;
            public bool ChildTable { get; set; } = false;
            public int PageNumber { get; set; } = 0;
        }
        public class DDLprops
        {
            public string text { get; set; }
            public int value { get; set; }
            public string valuetxt { get; set; }
            public bool isIdstring { get; set; }
        }
    }

    public class RunAuditSearchReqModel
    {
        public AuditReportSearch.UIproperties paramss { get; set; }
    }


    // reports initiator
    public class ReportsModels : BaseModel
    {
        public ReportsModels(Passport passport, ReportingJsonModel UIparams, HttpContext httpContext)
        {
            this._passport = passport;
            _httpContext = httpContext;
            ListOfHeader = new List<string>();
            ListOfRows = new List<List<string>>();
            CultureInfo = Keys.GetCultureCookies(httpContext);
            // Me.PageNumber = UIparams.pageNumber
            // Me.TableName = UIparams.tableName
            ddlSelectReport = new List<DDLItems>();
            // Me.ddlid = UIparams.id
            // Me.isPullListDDLCall = UIparams.isPullListDDLCall
            UI = UIparams;
            Paging.PageNumber = UIparams.pageNumber;
        }
        public ReportsModels(Passport passport, HttpContext httpContext)
        {
            this._passport = passport;
            _httpContext = httpContext;
        }

        public PagingModel Paging { get; set; } = new PagingModel();
        public ReportingJsonModel UI { get; set; }
        private Dictionary<string, string> IdsByTable = null;
        private Dictionary<string, DataTable> Descriptions = null;
        public List<string> ListOfHeader { get; set; }
        public List<List<string>> ListOfRows { get; set; }
        public string DisplayNotAuthorized { get; set; }
        public string PageTitle { get; set; }
        public string lblTitle { get; set; }
        public string lblSubtitle { get; set; }
        public string lblReportDate { get; set; }
        public string TotalRowsCount { get; set; }
        protected CultureInfo CultureInfo { get; set; }
        // Private Property PageNumber As Integer
        private DateTime dateFromTxt { get; set; }
        // Private Property TableName As String
        protected DataTable _TrackingTables { get; set; }
        public string lblSelectReport { get; set; }
        public List<DDLItems> ddlSelectReport { get; set; }
        protected int ddlid { get; set; }
        private bool isPullListDDLCall { get; set; }
        public void ExecuteReporting(ReportsType repNum)
        {

            string TodayDate = DateTime.Now.Date.ToString("MM/dd/yyyy").Split(' ')[0];
            dateFromTxt = DateTime.Parse(TodayDate, CultureInfo);
            //dateFromTxt = Convert.ToDateTime(dateFromTxt.ToString().Split(' ')[0]);
            try
            {
                switch (repNum)
                {
                    case ReportsType.PastDueTrackableItemsReport:
                        {
                            PastDueTrackableItemsReport();
                            break;
                        }
                    case ReportsType.ObjectOut:
                        {
                            ObjectsOut();
                            break;
                        }
                    case ReportsType.ObjectsInventory:
                        {
                            ObjectsInventory();
                            break;
                        }
                    case ReportsType.RequestNew:
                        {
                            Requestnew();
                            break;
                        }
                    case ReportsType.RequestNewBatch:
                        {
                            RequestNewBatch();
                            break;
                        }
                    case ReportsType.RequestPullList:
                        {
                            PullList();
                            break;
                        }
                    case ReportsType.RequestException:
                        {
                            RequestExceptions();
                            break;
                        }
                    case ReportsType.RequestInProcess:
                        {
                            InProcess();
                            break;
                        }
                    case ReportsType.RequestWaitList:
                        {
                            WaitList();
                            break;
                        }
                }
            }
            catch (Exception)
            {
                // return error message
            }

        }

        public void ExecuteReportingPagination(ReportsType repNum)
        {
            string TodayDate = DateTime.Now.Date.ToString().Split(' ')[0];
            dateFromTxt = DateTime.Parse(TodayDate, CultureInfo);
            dateFromTxt = Conversions.ToDate(dateFromTxt.ToString().Split(' ')[0]);
            try
            {
                switch (repNum)
                {
                    case ReportsType.PastDueTrackableItemsReport:
                        {
                            PastDueTrackableItemsReport_QueryCount();
                            break;
                        }
                    case ReportsType.ObjectOut:
                        {
                            ObjectsOut_QueryCount();
                            break;
                        }
                    case ReportsType.ObjectsInventory:
                        {
                            ObjectsInventory_QueryTableCount();
                            break;
                        }
                    case ReportsType.RequestNew:
                        {
                            Requestnew_QueryCount();
                            break;
                        }
                    case ReportsType.RequestNewBatch:
                        {
                            RequestNewBatch_QueryCount();
                            break;
                        }
                    case ReportsType.RequestPullList:
                        {
                            PullList_QueryCount();
                            break;
                        }
                    case ReportsType.RequestException:
                        {
                            RequestExceptions_Count();
                            break;
                        }
                    case ReportsType.RequestInProcess:
                        {
                            InProcess_QueryCount();
                            break;
                        }
                    case ReportsType.RequestWaitList:
                        {
                            WaitList_QueryCount();
                            break;
                        }
                }
            }
            catch (Exception)
            {
                // return error message
            }

        }
        // PastDueTrackableItemsReport
        private void PastDueTrackableItemsReport()
        {
            if (!this._passport.CheckPermission(" Tracking", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsPastDueRepo");
                return;
            }
            PageTitle = Languages.get_Translation("tiHTMLReportsPastDueTrackable");
            lblTitle = Languages.get_Translation("tiHTMLReportsPastDueTrackable");
            lblSubtitle = Languages.get_Translation("lblHTMLReportsAsof") + " " + Keys.get_ConvertCultureDate(dateFromTxt.ToShortDateString(), _httpContext);
            lblReportDate = string.Format(Languages.get_Translation("lblHTMReportsReportDate"), Keys.get_ConvertCultureDate(dateFromTxt.ToShortDateString(), _httpContext));
            PastDueTrackableItems_Headers();
            PastDueTrackableItems_Rows();
        }

        private void PastDueTrackableItemsReport_QueryCount()
        {
            if (!this._passport.CheckPermission(" Tracking", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsPastDueRepo");
                return;
            }
            PageTitle = Languages.get_Translation("tiHTMLReportsPastDueTrackable");
            lblTitle = Languages.get_Translation("tiHTMLReportsPastDueTrackable");
            lblSubtitle = Languages.get_Translation("lblHTMLReportsAsof") + " " + Keys.get_ConvertCultureDate(dateFromTxt.ToShortDateString(), _httpContext);
            lblReportDate = string.Format(Languages.get_Translation("lblHTMReportsReportDate"), Keys.get_ConvertCultureDate(dateFromTxt.ToShortDateString(), _httpContext));


            var _trackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            var _trackedTables = Tracking.GetTrackedTables(this._passport);
            // Dim dateFromTxt As Date = DateTime.Parse(txtDate.Text, CultureInfo)
            var param = new Parameters(this._passport);
            param.Paged = true;
            param.PageIndex = UI.pageNumber;
            var dt = Tracking.GetPagedPastDueTrackablesCount(dateFromTxt, this._passport, param, _trackingTables, _trackedTables, ref IdsByTable, ref Descriptions);

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }

        }

        private void PastDueTrackableItems_Headers()
        {
            ListOfHeader.Add(Languages.get_Translation("lblImportMainFormDateDue"));
            ListOfHeader.Add(Languages.get_Translation("tiHTMLReportsItemType"));
            ListOfHeader.Add(Languages.get_Translation("Description"));
            ListOfHeader.Add(Languages.get_Translation("lblStatusLocation"));
            ListOfHeader.Add(Languages.get_Translation("tiHTMLReportsTranDate"));
            ListOfHeader.Add(Languages.get_Translation("tiHTMLReportsScanOperator1"));
            ListOfHeader.Add(Languages.get_Translation("tiHTMLReportsAuthorization"));
        }
        private void PastDueTrackableItems_Rows()
        {
            var _trackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            var _trackedTables = Tracking.GetTrackedTables(this._passport);
            // Dim dateFromTxt As Date = DateTime.Parse(txtDate.Text, CultureInfo)
            var param = new Parameters(this._passport);
            param.Paged = true;
            param.PageIndex = UI.pageNumber;
            var lst = Tracking.GetPagedPastDueTrackablesList(dateFromTxt, this._passport, param, Paging.PerPageRecord, _trackingTables, _trackedTables, ref IdsByTable, ref Descriptions);

            var tablesInfo = Tracking.GetTrackedTableInfo(this._passport, _trackingTables, _trackedTables);
            if (Descriptions is null)
            {
                Descriptions = new Dictionary<string, DataTable>();
                foreach (DataRow tableInfo in tablesInfo.Rows)
                {
                    string ids = "";
                    if (IdsByTable.TryGetValue(tableInfo["TableName"].ToString().ToLower(), out ids)) // If we have ids, prepopulate; otherwise it'll have to get each one individually
                    {
                        Descriptions.Add(tableInfo["TableName"].ToString().ToLower(), Navigation.GetItemNames(tableInfo["TableName"].ToString(), this._passport, tableInfo, ids));
                    }
                }
            }
            TotalRowsCount = lst.Count.ToString();

            foreach (TrackingTransaction trk in lst)
            {
                var cell = new List<string>();
                cell.Add(Keys.get_ConvertCultureDate(trk.DateDue.ToString(), _httpContext));
                cell.Add(trk.Type);
                var itemNameInfo = tablesInfo.Select("TableName='" + trk.Type + "'");

                if (itemNameInfo.Count() > 0)
                {
                    var itemNames = new DataTable();
                    if (Descriptions.TryGetValue(trk.Type.ToLower(), out itemNames))
                    {
                        var itemName = itemNames.Select("id='" + trk.ID.Replace("'", "''") + "'");
                        if (itemName.Count() > 0)
                        {
                            cell.Add(Navigation.ItemNamesRowToItemName(itemName[0], itemNameInfo[0], this._passport, trk.ID));
                        }
                        else
                        {
                            cell.Add(Navigation.GetItemName(trk.Type, trk.ID, this._passport, itemNameInfo[0]));
                        }
                    }
                    else
                    {
                        cell.Add(Navigation.GetItemName(trk.Type, trk.ID, this._passport, itemNameInfo[0]));
                    }
                }
                else
                {
                    cell.Add(Navigation.GetItemName(trk.Type, trk.ID, this._passport));
                }
                string location = "";
                foreach (var cont in trk.Containers)
                    location += string.Format("{0}: ", cont.Type);
                foreach (var cont in trk.Containers)
                    location += string.Format(" {0}", cont.Name);
                cell.Add(location);
                cell.Add(Keys.get_ConvertCultureDate(trk.TransactionDate, _httpContext));
                cell.Add(trk.UserName);
                cell.Add("");
                ListOfRows.Add(cell);
            }
        }
        // ObjectsOut
        private void ObjectsOut()
        {
            if (!this._passport.CheckPermission(" Tracking", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsItemsOutRepo");
                return;
            }
            lblTitle = Languages.get_Translation("CurrentItemsOutRpt");
            lblSubtitle = Languages.get_Translation("lblHTMLReportsAllItemsOut");
            ObjectsOut_Header();
            ObjectsOut_Rows();
        }

        private void ObjectsOut_QueryCount()
        {
            var @params = new Parameters(this._passport);
            @params.Paged = true;
            @params.PageIndex = UI.pageNumber;
            if (!this._passport.CheckPermission(" Tracking", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsItemsOutRepo");
                return;
            }
            Dictionary<string, string> argidsByTable = null;
            var dt = Tracking.GetCurrentItemsOutReportCount(@params, this._passport, idsByTable: ref argidsByTable);

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }
        }

        private void ObjectsOut_Header()
        {
            ListOfHeader.Add(Languages.get_Translation("tiHTMLReportsEmployee"));
            ListOfHeader.Add(Languages.get_Translation("Phone"));
            ListOfHeader.Add(Languages.get_Translation("MailStop"));
            ListOfHeader.Add(Languages.get_Translation("tiHTMLReportsItemType"));
            ListOfHeader.Add(Languages.get_Translation("Description"));
            ListOfHeader.Add(Languages.get_Translation("tiHTMLReportsTranDate"));
        }
        private void ObjectsOut_Rows()
        {
            var @params = new Parameters(this._passport);
            @params.Paged = true;
            @params.PageIndex = UI.pageNumber;
            Dictionary<string, string> argidsByTable = null;
            var lst = Tracking.GetCurrentItemsOutReportList(@params, this._passport, Paging.PerPageRecord, idsByTable: ref argidsByTable);
            if (lst is null)
                return;
            foreach (TrackingTransaction trk in lst)
            {
                var cell = new List<string>();
                string tdName = string.Empty;
                string tdMail = string.Empty;
                string tdPhone = string.Empty;
                string content = "";
                foreach (var cont in trk.Containers)
                {
                    // content = String.Format("{0} {1} {2}", cont.Name, con)
                    tdName += cont.Name;
                    tdMail += cont.MailStop;
                    tdPhone += cont.Phone;
                }

                cell.Add(tdName);
                cell.Add(tdPhone);
                cell.Add(tdMail);
                cell.Add(trk.Type);
                cell.Add(Navigation.GetItemName(trk.Type, trk.ID, this._passport));
                cell.Add(Keys.get_ConvertCultureDate(trk.TransactionDate, _httpContext));
                ListOfRows.Add(cell);
            }
        }
        // ObjectsInventory
        private void ObjectsInventory()
        {
            if (!this._passport.CheckPermission(" Tracking", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsObjInvenRepo");
                return;
            }

            lblTitle = Languages.get_Translation("lblHTMLReportsObjInventoryRpt");
            var lstTable = QueryTableObjectsInventory();
            ObjectsInventory_Headers(lstTable);
            ObjectsInventory_Rows(lstTable);
        }

        private void ObjectsInventory_QueryTableCount()
        {
            if (!this._passport.CheckPermission(" Tracking", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsObjInvenRepo");
                return;
            }

            var lstTable = QueryTableObjectsInventoryCount();
        }

        private void ObjectsInventory_Headers(DataTable data)
        {
            foreach (DataColumn col in data.Columns)
                ListOfHeader.Add(col.ColumnName.ToString());
        }
        private void ObjectsInventory_Rows(DataTable data)
        {
            foreach (DataRow row in data.Rows)
            {
                var cell = new List<string>();
                cell.Add(row.ItemArray[0].Text());
                cell.Add(row.ItemArray[1].Text());
                cell.Add(row.ItemArray[2].Text());
                cell.Add(row.ItemArray[3].Text());
                cell.Add(row.ItemArray[4].Text());
                cell.Add(row.ItemArray[5].Text());
                ListOfRows.Add(cell);
            }
        }
        private DataTable QueryTableObjectsInventory()
        {
            DataRow locationInfo = null;
            var @params = new Parameters(this._passport);
            var query = new Query(this._passport);

            var dts = new DataTable();
            using (var conn = this._passport.Connection())
            {
                var tableInfo = Navigation.GetTableInfo(UI.tableName, conn);
                lblSubtitle = tableInfo["UserName"].ToString();
                using (var cmd = new SqlCommand("SELECT * FROM Tables WHERE TrackingTable = 1", conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                            locationInfo = dt.AsEnumerable().ElementAtOrDefault(0);
                    }
                }

                dts = QueryTableObjectsInventoryCountOrData(false, locationInfo, tableInfo);

            }
            return dts;
        }
        private DataTable QueryTableObjectsInventoryCount()
        {
            DataRow locationInfo = null;
            var @params = new Parameters(this._passport);
            var query = new Query(this._passport);

            var dts = new DataTable();
            using (var conn = this._passport.Connection())
            {
                var tableInfo = Navigation.GetTableInfo(UI.tableName, conn);
                lblSubtitle = tableInfo["UserName"].ToString();
                using (var cmd = new SqlCommand("SELECT * FROM Tables WHERE TrackingTable = 1", conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                            locationInfo = dt.AsEnumerable().ElementAtOrDefault(0);
                    }
                }

                dts = QueryTableObjectsInventoryCountOrData(true, locationInfo, tableInfo);

                if (dts.Rows.Count > 0)
                {
                    Paging.Execute(Convert.ToInt32(dts.Rows[0][0]));
                }
                else
                {
                    Paging.Execute(0);
                }

            }
            return dts;
        }
        private DataTable QueryTableObjectsInventoryCountOrData(bool count, DataRow locationInfo, DataRow tableInfo)
        {

            var dts = new DataTable();
            using (var conn = this._passport.Connection())
            {
                if (locationInfo is not null)
                {
                    string tTable = tableInfo["TableName"].ToString();
                    string lTable = locationInfo["TableName"].ToString();
                    string fileRoomOrder = Navigation.GetFileRoomOrderSQL(tableInfo, conn, tTable);
                    string sql = string.Empty;
                    if (count)
                    {
                        sql += "SELECT COUNT(*) ";
                    }
                    else
                    {
                        sql += "SELECT " + fileRoomOrder + " AS [File Room Order], '" + locationInfo["DescFieldPrefixOne"].ToString() + "' + " + Navigation.GetFieldAsVarCharSQL(locationInfo, locationInfo["DescFieldNameOne"].ToString(), conn, true) + " + '" + locationInfo["DescFieldPrefixTwo"].ToString() + "' + " + Navigation.GetFieldAsVarCharSQL(locationInfo, locationInfo["DescFieldNameTwo"].ToString(), conn, true) + " AS Locations, " + lTable + ".[" + locationInfo["InactiveLocationField"].ToString() + "] AS [Inactive Storage], " + lTable + ".[" + locationInfo["TrackingRequestableFieldName"].ToString() + "] AS Requestable, '" + tableInfo["UserName"].ToString() + "' AS [Object], ";




                        if (!string.IsNullOrEmpty(tableInfo["DescFieldNameOne"].ToString().Trim()))
                        {
                            if (!string.IsNullOrEmpty(tableInfo["DescFieldNameTwo"].ToString().Trim()))
                            {
                                sql += "'" + tableInfo["DescFieldPrefixOne"].ToString() + "' + " + Navigation.GetFieldAsVarCharSQL(tableInfo, tableInfo["DescFieldNameOne"].ToString(), conn, true) + " + '" + tableInfo["DescFieldPrefixTwo"].ToString() + "' + " + Navigation.GetFieldAsVarCharSQL(tableInfo, tableInfo["DescFieldNameTwo"].ToString(), conn, true) + " AS [" + tableInfo["UserName"].ToString() + " Description]";
                            }
                            else
                            {
                                sql += "'" + tableInfo["DescFieldPrefixOne"].ToString() + "' + " + Navigation.GetFieldAsVarCharSQL(tableInfo, tableInfo["DescFieldNameOne"].ToString(), conn, true) + " AS [" + tableInfo["UserName"].ToString() + " Description]";
                            }
                        }
                        else if (!string.IsNullOrEmpty(tableInfo["DescFieldNameTwo"].ToString().Trim()))
                        {
                            sql += "'" + tableInfo["DescFieldPrefixTwo"].ToString() + "' + " + Navigation.GetFieldAsVarCharSQL(tableInfo, tableInfo["DescFieldNameTwo"].ToString(), conn, true) + " AS [" + tableInfo["UserName"].ToString() + " Description]";
                        }
                        else
                        {
                            sql += "'Configure Display Fields' AS [" + tableInfo["UserName"].ToString() + " Description]";
                        }
                    }

                    sql += " FROM " + locationInfo["TableName"].ToString() + ", " + tableInfo["TableName"].ToString() + ", TrackingStatus ts WHERE ts.TrackedTable = '" + tTable + "' AND ";

                    if (Navigation.FieldIsAString(tableInfo, conn))
                    {
                        sql += "REPLICATE('0', 30 - LEN(" + tableInfo["IdFieldName"].ToString() + ")) + " + tableInfo["IdFieldName"].ToString() + " = ts.TrackedTableId AND ";
                    }
                    else
                    {
                        sql += tableInfo["IdFieldName"].ToString() + " = ts.TrackedTableId AND ";
                    }
                    if (Navigation.FieldIsAString(locationInfo, conn))
                    {
                        sql += "REPLICATE('0', 30 - LEN(" + locationInfo["IdFieldName"].ToString() + ")) + " + locationInfo["IdFieldName"].ToString() + " = ts.[" + locationInfo["TrackingStatusFieldName"].ToString() + "]";
                    }
                    else
                    {
                        sql += locationInfo["IdFieldName"].ToString() + " = ts.[" + locationInfo["TrackingStatusFieldName"].ToString() + "]";
                    }

                    if (!count)
                    {
                        string orderClause = " ORDER BY " + fileRoomOrder;
                        sql += orderClause;

                        // NOTE: query will through exception if does not include ORDER BY
                        sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);
                    }

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dts);
                        }
                    }
                }
            }

            return dts;
        }
        // NewRequest
        private void Requestnew()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("HTMLReportsNewReqReport");
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            lblTitle = Languages.get_Translation("lblHTMLReportsNewReqRpt");
            lblSubtitle = string.Empty;
            // btnCommand.Text = Languages.Translation("lblHTMLReportsSendCheckedItmLst")
            string sql = WriteBaseRequestQuery(false);
            sql += " WHERE [SLRequestor].[Status] = 'New' ORDER BY [SLRequestor].[Id] ";

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = RequestTable(sql, "new");
            buildHeadersWithHiddenFields(dt);
            buildRowsWithHiddenFields(dt);
        }
        // NewRequest count
        private void Requestnew_QueryCount()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("HTMLReportsNewReqReport");
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            string sql = WriteBaseRequestQuery(true);
            sql += " WHERE [SLRequestor].[Status] = 'New'";

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }

        }

        protected string GetTrackingItemName(DataRow row)
        {
            var sb = new StringBuilder();
            for (int i = 0, loopTo = _TrackingTables.Rows.Count - 1; i <= loopTo; i++)
            {
                if (!string.IsNullOrEmpty(row[_TrackingTables.Rows[i]["TrackingStatusFieldName"].ToString()].ToString()))
                {
                    sb.Append(Navigation.GetItemName(_TrackingTables.Rows[i]["TableName"].ToString(), row[_TrackingTables.Rows[i]["TrackingStatusFieldName"].ToString()].ToString(), this._passport));
                    if (i < _TrackingTables.Rows.Count - 1)
                        sb.Append(Constants.vbCrLf);
                }
            }
            return sb.ToString();
        }
        private string AddTrackingFields()
        {
            var sb = new StringBuilder();
            foreach (DataRow row in _TrackingTables.Rows)
                sb.Append(string.Format(", [TrackingStatus].[{0}]", row["TrackingStatusFieldName"].ToString()));
            return sb.ToString();
        }
        private string AddTrackingField(int index)
        {
            return string.Format(", [TrackingStatus].[{0}]", _TrackingTables.Rows[index]["TrackingStatusFieldName"].ToString());
        }
        // request newbatch
        private void RequestNewBatch()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = string.Format(Languages.get_Translation("NewBatch"), ddlid);
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            if (!isPullListDDLCall)
            {
                GenerateDropdownRequestBatch();
            }
            lblTitle = string.Format(Languages.get_Translation("NewBatch"), ddlid);
            string sql = WriteBaseRequestQuery(false);
            if (ddlid == 0)
            {
                ddlid = -1;
            }
            sql += string.Format(" WHERE [SLRequestor].[SLPullListsId] = {0} ORDER BY SLRequestor.Priority, SLRequestor.FileRoomOrder ", ddlid);

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = RequestTable(sql, ddlid.ToString());
            buildHeadersWithHiddenFields(dt);
            buildRowsWithHiddenFields(dt);
        }
        // request newbatch count
        private void RequestNewBatch_QueryCount()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = string.Format(Languages.get_Translation("NewBatch"), ddlid);
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());

            string sql = WriteBaseRequestQuery(true);
            if (ddlid == 0)
            {
                ddlid = -1;
            }
            sql += string.Format(" WHERE [SLRequestor].[SLPullListsId] = {0} ", ddlid);

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }

        }
        private void GenerateDropdownRequestBatch()
        {
            string Sql = "SELECT Id, [DateCreated], OperatorsId, COALESCE('[' + SLBatchRequestComment + ']', '') AS Comment" + " FROM SLPullLists WHERE BatchPullList <> 0 AND BatchPrinted = 0 " + " ORDER BY Id Desc";
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(Sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            int maxDateLength = TrimAMPMIndicators(new DateTime(2000, 12, 31, 12, 59, 59).ToString("g", CultureInfo)).Length;
                            int maxIdLength = dt.Rows[0]["Id"].ToString().Length;
                            int maxCommentLength = 20;
                            int start = 0;
                            foreach (DataRow row in dt.Rows)
                            {
                                if (start == 0)
                                {
                                    ddlid = Convert.ToInt32(row["Id"]);
                                }
                                start = start + 1;
                                string test = string.Empty;
                                test = Keys.get_ConvertCultureDate(row["DateCreated"].ToString(),_httpContext ,false, true, true);
                                test = row["Comment"].ToString();
                                int maxUserLength = Conversions.ToInteger(GetMaxUserLength(dt));
                                if (test.Length > maxCommentLength)
                                    test = string.Format("{0}]...", test.Substring(0, maxCommentLength));
                                string items = string.Format("{0} {1} {2}", Keys.get_ConvertCultureDate(row["DateCreated"].ToString(),_httpContext, false, true, true), row["OperatorsId"].ToString().PadLeft(maxUserLength, ' '), test);
                                ddlSelectReport.Add(new DDLItems() { text = items, value = row["Id"].ToString() });
                            }
                        }
                    }
                }
            }
        }
        // build request headers and rows with hidden field
        private void buildHeadersWithHiddenFields(DataTable data)
        {
            ListOfHeader.Add("id");
            foreach (DataColumn col in data.Columns)
            {
                if (!IsSystemColumn(col.ColumnName))
                {
                    if (col.ExtendedProperties["heading"] is null)
                    {
                        if (string.Compare(col.ColumnName, col.Caption) != 0)
                        {
                        }

                        else
                        {
                            ListOfHeader.Add(col.ColumnName);
                        }
                    }
                    else
                    {
                        ListOfHeader.Add(Conversions.ToString(col.ExtendedProperties["heading"]));
                    }
                }
            }
        }
        private void buildRowsWithHiddenFields(DataTable data)
        {
            string tableid = string.Empty;
            string tableName = string.Empty;

            foreach (DataRow row in data.Rows)
            {
                var cell = new List<string>();
                foreach (DataColumn col in data.Columns)
                {
                    if (!IsSystemColumn(col.ColumnName))
                    {
                        if (IsADateType(col.DataType))
                        {
                            cell.Add(Conversions.ToDate(row[col].ToString()).ToClientDateFormat());
                        }
                        else
                        {
                            cell.Add(row[col].ToString());
                        }
                    }
                    else
                    {
                        if (col.ColumnName.ToLower() == "tableid")
                        {
                            // cell.Add(row(col).ToString)
                            tableid = row[col].ToString();
                        }
                        if (col.ColumnName.ToLower() == "tablename")
                        {
                            // cell.Add(row(col).ToString)
                            tableName = row[col].ToString();
                        }
                    }
                }
                var cells = new List<string>();
                cells.Add(tableid + "||" + tableName);
                // this extra loop written for rearranging the loop
                foreach (string item in cell)
                    cells.Add(item);
                ListOfRows.Add(cells);
            }
        }
        // request PullList
        private void PullList()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsPullListRpt");
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            lblTitle = Languages.get_Translation("lblHTMLReportsPullListRpt");
            lblSelectReport = Languages.get_Translation("lblHTMLReportsSelectPullList");
            if (!UI.isQueryFromDDL)
            {
                GenerateDropdownPullList();
            }
            else
            {
                ddlid = Conversions.ToInteger(UI.id);
            }
            string sql = WriteBaseRequestQuery(false);
            if (ddlid == 0)
            {
                ddlid = -1;
            }
            sql += string.Format(" WHERE [SLRequestor].[SLPullListsId] = {0} ORDER BY SLRequestor.Priority, SLRequestor.FileRoomOrder ", ddlid);

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = RequestTable(sql, ddlid.ToString());
            PullListHeaders(dt);
            PullListRows(dt);

        }
        // request PullList Count
        private void PullList_QueryCount()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsPullListRpt");
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            if (!UI.isQueryFromDDL)
            {
                GenerateDropdownPullList();
            }
            else
            {
                ddlid = Conversions.ToInteger(UI.id);
            }
            string sql = WriteBaseRequestQuery(true);
            if (ddlid == 0)
            {
                ddlid = -1;
            }
            sql += string.Format(" WHERE [SLRequestor].[SLPullListsId] = {0} ", ddlid);
            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }
        }
        private void GenerateDropdownPullList()
        {
            string Sql = "SELECT Id, [DateCreated], OperatorsId, CASE BatchPullList WHEN 0 THEN 'Regular' ELSE 'Batch' END AS BatchType" + " FROM SLPullLists WHERE ((BatchPullList = 0) OR (BatchPullList <> 0 AND BatchPrinted <> 0))" + " ORDER BY Id Desc";
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(Sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {
                            int maxDateLength = TrimAMPMIndicators(new DateTime(2000, 12, 31, 12, 59, 59).ToString("g", CultureInfo)).Length;
                            int maxIdLength = dt.Rows[0]["Id"].ToString().Length;
                            int start = 0;
                            foreach (DataRow row in dt.Rows)
                            {
                                if (start == 0)
                                {
                                    ddlid = Convert.ToInt32(row["Id"]);
                                }
                                start = start + 1;
                                ddlSelectReport.Add(new DDLItems() { value = row["Id"].ToString().PadLeft(maxIdLength, ' '), text = string.Format("{0} {1} {2}", Keys.get_ConvertCultureDate(row["DateCreated"].ToString(),_httpContext, false, true, true), row["BatchType"].ToString().PadRight(10, ' '), row["OperatorsId"].ToString()) });
                            }
                        }
                    }
                }
            }
        }
        private void PullListHeaders(DataTable data)
        {
            foreach (DataColumn col in data.Columns)
            {
                if (!IsSystemColumn(col.ColumnName))
                {
                    if (col.ExtendedProperties["heading"] is null)
                    {
                        if (string.Compare(col.ColumnName, col.Caption) != 0)
                        {
                        }

                        else
                        {
                            ListOfHeader.Add(col.ColumnName);
                        }
                    }
                    else
                    {
                        ListOfHeader.Add(Conversions.ToString(col.ExtendedProperties["heading"]));
                    }
                }
            }
        }
        private void PullListRows(DataTable data)
        {
            foreach (DataRow row in data.Rows)
            {
                var cell = new List<string>();
                foreach (DataColumn col in data.Columns)
                {
                    if (!IsSystemColumn(col.ColumnName))
                    {
                        if (IsADateType(col.DataType))
                        {
                            cell.Add(Conversions.ToDate(row[col].ToString()).ToClientDateFormat());
                        }
                        else
                        {
                            cell.Add(row[col].ToString());
                        }
                    }
                }
                ListOfRows.Add(cell);
            }
        }
        protected object GetMaxUserLength(DataTable dt)
        {
            int rtn = 0;

            foreach (DataRow row in dt.Rows)
            {
                if (row["OperatorsId"].ToString().Length > rtn)
                    rtn = row["OperatorsId"].ToString().Length;
            }

            return rtn;
        }
        // request Exception
        private void RequestExceptions()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsExceptionsRpt");
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            lblTitle = Languages.get_Translation("lblHTMLReportsExceptionsRpt");
            var td = RequestExceptionQuery();
            RequestHeaders(td);
            RequestRows(td);
        }

        private void RequestExceptions_Count()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsExceptionsRpt");
                return;
            }
            // _TrackingTables = GetTrackingContainerTypes(_passport.Connection())
            RequestExceptionQueryCount();
        }

        private DataTable RequestExceptionQuery()
        {
            string sql = string.Format("SELECT [Tables].[UserName] AS 'Folder Type', '' AS 'Folder Description'," + " CONVERT(VARCHAR, [SLRequestor].[DateRequested], 100) AS 'Request Date', '' AS 'Requestor', CONVERT(VARCHAR, [SLRequestor].[DateNeeded], 100) AS 'Date Needed'," + " [SLRequestor].[ExceptionComments] AS 'Exception Comments', [SLRequestor].[Priority]," + " CONVERT(VARCHAR, [SLRequestor].[DatePulled], 100) AS 'Date Pulled','' AS 'Currently At', [SLRequestor].[Id] AS 'Request #'," + " [SLRequestor].[TableName], [SLRequestor].[TableId], [SLRequestor].[EmployeeId]{0}" + " FROM ([SLRequestor] LEFT JOIN [TrackingStatus] ON ([SLRequestor].[TableName] = [TrackingStatus].[TrackedTable] AND [SLRequestor].[TableId] = [TrackingStatus].[TrackedTableId])) " + " LEFT JOIN [Tables] ON [SLRequestor].[TableName] = [Tables].[TableName] WHERE [SLRequestor].[Status] = @status" + "", AddTrackingFields());
            string orderClause = " ORDER BY [Tables].[UserName], SLRequestor.TableId, SLRequestor.DateRequested";
            sql += orderClause;

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@status", "Exception");
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        MassageDataForRequests(dt);
                        RemoveUnneededColumns(dt, string.Empty);
                    }
                }
            }
            return dt;
        }
        private void RequestExceptionQueryCount()
        {
            string sql = string.Format("SELECT count(*) " + " FROM ([SLRequestor] LEFT JOIN [TrackingStatus] ON ([SLRequestor].[TableName] = [TrackingStatus].[TrackedTable] AND [SLRequestor].[TableId] = [TrackingStatus].[TrackedTableId])) " + " LEFT JOIN [Tables] ON [SLRequestor].[TableName] = [Tables].[TableName] WHERE [SLRequestor].[Status] = @status" + "");
            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@status", "Exception");
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }

        }
        // in process
        private void InProcess()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsInProcRpt");
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            lblTitle = Languages.get_Translation("lblHTMLReportsInProcRpt");
            var td = RequestInProcessQuery();
            RequestHeaders(td);
            RequestRows(td);
        }

        private void InProcess_QueryCount()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("lblHTMLReportsInProcRpt");
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            RequestInProcessQueryCount();
        }
        private DataTable RequestInProcessQuery()
        {
            string sql = string.Format("SELECT [SLRequestor].[SLPullListsId] AS 'Pull List #', CONVERT(VARCHAR, [SLRequestor].[DatePulled], 100) AS 'Date Pulled'," + " [Tables].[UserName] AS 'Folder Type', '' AS 'Folder Description'," + " CONVERT(VARCHAR, [SLRequestor].[DateRequested], 100) AS 'Request Date', '' AS 'Requestor'," + " [SLRequestor].[Priority], '' AS 'Currently At', [SLRequestor].[Id] AS 'Request #'," + " [SLRequestor].[TableName], [SLRequestor].[TableId], [SLRequestor].[EmployeeId]{0}" + " FROM ([SLRequestor] LEFT JOIN [TrackingStatus] ON ([SLRequestor].[TableName] = [TrackingStatus].[TrackedTable] AND [SLRequestor].[TableId] = [TrackingStatus].[TrackedTableId])) " + " LEFT JOIN [Tables] ON [SLRequestor].[TableName] = [Tables].[TableName] WHERE [SLRequestor].[Status] = @status" + "", AddTrackingFields());
            string orderClause = " ORDER BY SLRequestor.SLPullListsId, SLRequestor.DatePulled, Tables.UserName";
            sql += orderClause;

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@status", "In Process");
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        // Me.MassageDataForRequests(dt)
                        RemoveUnneededColumns(dt, string.Empty);
                    }
                }
            }
            return dt;
        }
        private void RequestInProcessQueryCount()
        {
            string sql = string.Format("SELECT count(*)" + " FROM ([SLRequestor] LEFT JOIN [TrackingStatus] ON ([SLRequestor].[TableName] = [TrackingStatus].[TrackedTable] AND [SLRequestor].[TableId] = [TrackingStatus].[TrackedTableId])) " + " LEFT JOIN [Tables] ON [SLRequestor].[TableName] = [Tables].[TableName] WHERE [SLRequestor].[Status] = @status" + "");

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@status", "In Process");
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }

        }
        // wait list
        private void WaitList()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("WaitListReport");
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            lblTitle = Languages.get_Translation("WaitListReport");
            var td = RequestWaitListQuery();
            RequestHeaders(td);
            RequestRows(td);
        }
        // wait list count
        private void WaitList_QueryCount()
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("WaitListReport");
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            RequestWaitListQueryCount();
        }
        private DataTable RequestWaitListQuery()
        {
            string sql = string.Format("SELECT [SLRequestor].[Priority], CONVERT(VARCHAR, [SLRequestor].[DateRequested], 100) AS 'Request Date'," + " [Tables].[UserName] AS 'Folder Type', '' AS 'Folder Description', '' AS 'Currently At', '' AS 'Requested By'," + " [SLRequestor].[Id] AS 'Request #', [SLRequestor].[TableName], [SLRequestor].[TableId], [SLRequestor].[EmployeeId]{0}" + " FROM ([SLRequestor] LEFT JOIN [TrackingStatus] ON ([SLRequestor].[TableName] = [TrackingStatus].[TrackedTable]" + " AND [SLRequestor].[TableId] = [TrackingStatus].[TrackedTableId])) LEFT JOIN [Tables] ON" + " [SLRequestor].[TableName] = [Tables].[TableName] WHERE [SLRequestor].[Status] = @status", AddTrackingFields());
            string orderClause = " ORDER BY SLRequestor.Priority, SLRequestor.DateRequested, Tables.UserName, SLRequestor.TableId ";
            sql += orderClause;

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@status", "WaitList");
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        // MassageDataForWaitList(dt)
                        RemoveUnneededColumns(dt, string.Empty);
                    }
                }
            }
            return dt;
        }
        private void RequestWaitListQueryCount()
        {
            string sql = string.Format("SELECT Count(*)" + " FROM ([SLRequestor] LEFT JOIN [TrackingStatus] ON ([SLRequestor].[TableName] = [TrackingStatus].[TrackedTable]" + " AND [SLRequestor].[TableId] = [TrackingStatus].[TrackedTableId])) LEFT JOIN [Tables] ON" + " [SLRequestor].[TableName] = [Tables].[TableName] WHERE [SLRequestor].[Status] = @status", AddTrackingFields());
            // Dim orderClause = " ORDER BY SLRequestor.Priority, SLRequestor.DateRequested, Tables.UserName, SLRequestor.TableId"

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@status", "WaitList");
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }

        }
        // build request headers and rows
        private void RequestHeaders(DataTable data)
        {
            foreach (DataColumn col in data.Columns)
            {
                if (!IsSystemColumn(col.ColumnName))
                {
                    if (col.ExtendedProperties["heading"] is null)
                    {
                        if (string.Compare(col.ColumnName, col.Caption) != 0)
                        {
                        }

                        else
                        {
                            ListOfHeader.Add(col.ColumnName);
                        }
                    }
                    else
                    {
                        ListOfHeader.Add(Conversions.ToString(col.ExtendedProperties["heading"]));
                    }
                }
            }
        }
        private void RequestRows(DataTable data)
        {
            foreach (DataRow row in data.Rows)
            {
                var cell = new List<string>();
                foreach (DataColumn col in data.Columns)
                {
                    if (!IsSystemColumn(col.ColumnName))
                    {
                        if (IsADateType(col.DataType))
                        {
                            cell.Add(Conversions.ToDate(row[col].ToString()).ToClientDateFormat());
                        }
                        else
                        {
                            cell.Add(row[col].ToString());
                        }
                    }
                }
                ListOfRows.Add(cell);
            }
        }
        // reques functions helper
        private DataTable RequestTable(string sql, string status)
        {
            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        MassageDataForRequests(dt);
                        RemoveUnneededColumns(dt, status);
                        // DataTableReport(dt)
                    }
                }
            }
            return dt;
        }
        protected void MassageDataForRequests(DataTable data)
        {
            string requestorTableName = Tracking.GetRequestorTableName(this._passport);
            var idsByTable = new Dictionary<string, string>();
            var listOfTableNames = new List<string>();
            idsByTable.Add(requestorTableName, "");
            listOfTableNames.Add(requestorTableName);

            foreach (DataRow row in data.Rows)
            {
                if (!idsByTable.ContainsKey(row["TableName"].ToString()))
                {
                    idsByTable.Add(row["TableName"].ToString(), "");
                    listOfTableNames.Add(row["TableName"].ToString());
                }

                if (string.IsNullOrEmpty(idsByTable[row["TableName"].ToString()]))
                {
                    idsByTable[row["TableName"].ToString()] += "'" + row["TableId"].ToString() + "'";
                }
                else
                {
                    idsByTable[row["TableName"].ToString()] += ",'" + row["TableId"].ToString().Replace("'", "''") + "'";
                }
            }

            var tablesInfo = Navigation.GetMultipleTableInfo(listOfTableNames, this._passport);
            var descriptions = new Dictionary<string, DataTable>();

            foreach (DataRow tableInfo in tablesInfo.Rows)
            {
                string ids = string.Empty;
                if (idsByTable.TryGetValue(tableInfo["TableName"].ToString(), out ids)) // If we have ids, prepopulate; otherwise it'll have to get each one individually
                {
                    descriptions.Add(tableInfo["TableName"].ToString(), Navigation.GetItemNames(tableInfo["TableName"].ToString(), this._passport, tableInfo, ids));
                }
            }

            foreach (DataRow row in data.Rows)
            {
                row["Folder Description"] = Navigation.ExtractItemName(row["TableName"].ToString(), row["TableId"].ToString(), descriptions, tablesInfo, this._passport);
                row["Currently At"] = GetTrackingItemName(row);

                if (!string.IsNullOrEmpty(row["EmployeeId"].ToString()))
                {
                    row["Requestor"] = Navigation.ExtractItemName(requestorTableName, row["EmployeeId"].ToString(), descriptions, tablesInfo, this._passport);
                }

                if (!string.IsNullOrEmpty(row["Date Needed"].ToString()))
                    row["Date Needed"] = Keys.get_ConvertCultureDate(row["Date Needed"].ToString(), _httpContext);
                if (!string.IsNullOrEmpty(row["Date Pulled"].ToString()))
                    row["Date Pulled"] = Keys.get_ConvertCultureDate(row["Date Pulled"].ToString(), _httpContext);
            }
        }
        protected void RemoveUnneededColumns(DataTable data, string status)
        {
            switch (status.ToLower() ?? "")
            {
                case "new":
                    {
                        if (data.Columns.Contains("Status"))
                            data.Columns.Remove(data.Columns["Status"]);
                        if (data.Columns.Contains("Date Pulled"))
                            data.Columns.Remove(data.Columns["Date Pulled"]);
                        break;
                    }
                case "newbatchesreport":
                    {
                        if (data.Columns.Contains("Date Pulled"))
                            data.Columns.Remove(data.Columns["Date Pulled"]);
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            if (data.Columns.Contains("EmployeeId"))
                data.Columns.Remove(data.Columns["EmployeeId"]);
            if (data.Columns.Contains("UserName"))
                data.Columns.Remove(data.Columns["UserName"]);

            foreach (DataRow row in _TrackingTables.Rows)
            {
                if (data.Columns.Contains(row["TrackingStatusFieldName"].ToString()))
                    data.Columns.Remove(data.Columns[row["TrackingStatusFieldName"].ToString()]);
            }
        }
        private string WriteBaseRequestQuery(bool count)
        {
            string sql;

            if (count)
            {
                sql = string.Format("SELECT count(*) " + "FROM ([SLRequestor] LEFT JOIN [TrackingStatus] ON ([SLRequestor].[TableName] = [TrackingStatus].[TrackedTable] AND [SLRequestor].[TableId] = [TrackingStatus].[TrackedTableId])) " + " LEFT JOIN [Tables] ON [SLRequestor].[TableName] = [Tables].[TableName]");
            }
            else
            {
                sql = string.Format("SELECT [SLRequestor].[Priority], '' AS 'Currently At', [Tables].[UserName] AS 'Table Name'," + " [SLRequestor].[FileRoomOrder] AS 'File Room Order', '' AS 'Folder Description'," + " [SLRequestor].[DateRequested] AS 'Request Date', '' AS 'Requestor', [SLRequestor].[Instructions] AS 'Request Instructions'," + " CONVERT(VARCHAR, [SLRequestor].[DateNeeded], 100) AS 'Date Needed', [SLRequestor].[Status], [SLRequestor].[Id] AS 'Request #'," + " CONVERT(VARCHAR, [SLRequestor].[DatePulled], 100) AS 'Date Pulled'," + " [SLRequestor].[TableName], [SLRequestor].[TableId], [SLRequestor].[EmployeeId]{0}" + " FROM ([SLRequestor] LEFT JOIN [TrackingStatus] ON ([SLRequestor].[TableName] = [TrackingStatus].[TrackedTable] AND [SLRequestor].[TableId] = [TrackingStatus].[TrackedTableId])) " + " LEFT JOIN [Tables] ON [SLRequestor].[TableName] = [Tables].[TableName]", AddTrackingFields());
            }

            return sql;
        }
        protected string TrimAMPMIndicators(string text)
        {
            text = text.ToLower();
            if (!string.IsNullOrEmpty(System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.PMDesignator))
            {
                text = text.Replace(string.Format(" {0}", System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.PMDesignator.ToLower()), System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.PMDesignator.ToLower());
                text = text.Replace(string.Format(" {0}", System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.PMDesignator.Substring(0, 1).ToLower()), System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.PMDesignator.Substring(0, 1).ToLower());
            }
            if (!string.IsNullOrEmpty(System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.AMDesignator))
            {
                text = text.Replace(string.Format(" {0}", System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.AMDesignator.ToLower()), System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.AMDesignator.ToLower());
                text = text.Replace(string.Format(" {0}", System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.AMDesignator.Substring(0, 1).ToLower()), System.Threading.Thread.CurrentThread.CurrentUICulture.DateTimeFormat.AMDesignator.Substring(0, 1).ToLower());
            }
            return text;
        }
        // NewRequest Btn sendRequestPullList
        public void SendRequestToThePullList(ReportingJsonModel param)
        {
            if (!this._passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("HTMLReportsNewReqReport");
                return;
            }
            System.Resources.ResourceManager rm = new System.Resources.ResourceManager("TabFusionRMS.WebCS.Resource", Assembly.GetExecutingAssembly());

            int pullListId;
            string sql = rm.GetString("InsertSLPullList") == null ? "" : rm.GetString("InsertSLPullList");
            string status = "new";
            this.errorNumber = 0;
            if (param.isBatchRequest)
            {
                sql = rm.GetString("UpdateBatchSLPullList") == null ? "" : "";
            }
            try
            {
                using (var conn = this._passport.Connection())
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        if (param.isBatchRequest)
                        {
                            status = "New Batch";
                            cmd.Parameters.AddWithValue("@pullListId", param.id);
                            cmd.ExecuteScalar();
                            pullListId = Convert.ToInt32(param.id);
                        }
                        else
                        {
                            var user = new User(this._passport, true);
                            cmd.Parameters.AddWithValue("@userName", user.UserName);
                            cmd.Parameters.AddWithValue("@batchRequest", param.isBatchRequest);
                            var res = cmd.ExecuteScalar();
                            pullListId = Convert.ToInt32(res);
                        }
                    }

                    if (pullListId == 0)
                    {
                        pullListId = Convert.ToInt32(false);
                    }

                    using (var cmd = new SqlCommand(rm.GetString("UpdateSLRequestor"), conn))
                    {
                        cmd.Parameters.AddWithValue("@tableName", string.Empty);
                        cmd.Parameters.AddWithValue("@tableID", string.Empty);
                        cmd.Parameters.AddWithValue("@pullListId", pullListId);
                        cmd.Parameters.AddWithValue("@status", status);

                        foreach (items row in param.ListofPullItem)
                        {
                            cmd.Parameters["@tableName"].Value = row.tableName;
                            cmd.Parameters["@tableID"].Value = row.tableid;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.errorNumber = Convert.ToInt32(string.Format("{0} {1}", 1, ex.Message));
                // write to the errorevent method
            }
        }
    }
    public class RetentionReportModel : ReportsModels
    {
        public RetentionReportModel(Passport passport, ReportingJsonModel UIparams, HttpContext httpContext) : base(passport, UIparams, httpContext)
        {
            PermanentArchive = new RetentionButtons(passport);
            Purge = new RetentionButtons(passport);
            Destruction = new RetentionButtons(passport);
            SubmitDisposition = new RetentionButtons(passport);
            // Me.Paging.PageNumber = UIparams.pageNumber
        }
        public void ExecuteRetentionReports(ReportsType repNum)
        {
            if (!this._passport.CheckPermission(" Retention", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("msgHTMLReportsTheRetCodesReport");
                return;
            }
            try
            {
                switch (repNum)
                {
                    case ReportsType.RetentionFinalDisposition:
                        {
                            RetentionFinalDisposition();
                            break;
                        }
                    case ReportsType.RetentionCertifieDisposition:
                        {
                            RetentionCertifieDisposition();
                            break;
                        }
                    case ReportsType.RetentionInactivePullList:
                        {
                            InactivePullList();
                            break;
                        }
                    case ReportsType.RetentionInactiveRecords:
                        {
                            RetentionInactiveRecords();
                            break;
                        }
                    case ReportsType.RetentionRecordsOnHold:
                        {
                            RetentionRecordsOnHold();
                            break;
                        }
                    case ReportsType.RetentionCitations:
                        {
                            RetentionCitations();
                            break;
                        }
                    case ReportsType.RetentionCitationsWithRetCodes:
                        {
                            RetentionCitationsWithRetCodes();
                            break;
                        }
                    case ReportsType.RetentionCodes:
                        {
                            RetentionCodes();
                            break;
                        }
                    case ReportsType.RetentionCodesWithCitations:
                        {
                            RetentionCodesWithCitations();
                            break;
                        }
                }
            }
            catch (Exception)
            {
                // error message need to implement
            }

        }
        public void ExecuteRetentionReportsPagination(ReportsType repNum)
        {
            if (!this._passport.CheckPermission(" Retention", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
            {
                DisplayNotAuthorized = Languages.get_Translation("msgHTMLReportsTheRetCodesReport");
                return;
            }
            try
            {
                switch (repNum)
                {
                    case ReportsType.RetentionFinalDisposition:
                        {
                            RetentionFinalDisposition_QueryTableCount();
                            break;
                        }
                    case ReportsType.RetentionCertifieDisposition:
                        {
                            RetentionCertifieDisposition_QueryTableCount();
                            break;
                        }
                    case ReportsType.RetentionInactivePullList:
                        {
                            InactivePullList_QueryTableCount();
                            break;
                        }
                    case ReportsType.RetentionInactiveRecords:
                        {
                            RetentionInactiveRecords_QueryTableCount();
                            break;
                        }
                    // Case ReportsType.RetentionRecordsOnHold
                    // RetentionRecordsOnHold()
                    case ReportsType.RetentionCitations:
                        {
                            RetentionCitations_QueryTableCount();
                            break;
                        }
                    case ReportsType.RetentionCitationsWithRetCodes:
                        {
                            RetentionCitationsWithRetCodes_QueryTableCount();
                            break;
                        }
                    case ReportsType.RetentionCodes:
                        {
                            RetentionCodes_QueryTableCount();
                            break;
                        }
                    case ReportsType.RetentionCodesWithCitations:
                        {
                            RetentionCodesWithCitations_QueryTableCount();
                            break;
                        }
                }
            }
            catch (Exception)
            {
                // error message need to implement
            }

        }
        public RetentionButtons PermanentArchive { get; set; }
        public RetentionButtons Purge { get; set; }
        public RetentionButtons Destruction { get; set; }
        public RetentionButtons SubmitDisposition { get; set; }
        // Retention Final Disposition
        private void RetentionFinalDisposition()
        {
            if (this._passport.CheckPermission("Disposition", SecureObject.SecureObjectType.Retention, Permissions.Permission.Access))
            {
                lblTitle = Languages.get_Translation("mnuViewMenuControlElegibleForFinalDisposition");
                // get trackble containers
                _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
                // check for btn conditions
                RetentionFinalDisposition_btnConditions();
                // check if call was from dropdown
                if (!UI.isQueryFromDDL)
                {
                    RetentionFinalDisposition_Dropdown();
                }
                else
                {
                    ddlid = Conversions.ToInteger(UI.id);
                }
            }
            // query the table
            var dt = RetentionFinalDisposition_QueryTable();
            // create header and rows
            RetentionHeaders(dt, false, true);
            RetentionRows(dt, true, false, true);
        }
        private DataTable RetentionFinalDisposition_QueryTable()
        {
            string sql = "SELECT [SLDestructCertItems].TableName, [Tables].UserName AS [Table Name], [SLDestructCertItems].TableId, '' AS Item, [SLRetentionCodes].[DeptOfRecord] AS [Department of Record], " + "[SLRetentionCodes].[Id] AS [Retention Code], [SLRetentionCodes].[RetentionEventType] AS [Event Type], [SLDestructCertItems].EventDate AS [Event Date],  " + "[SLDestructCertItems].ScheduledDestruction AS [Scheduled Disposition], '' AS [Approved for Disposition]";

            foreach (DataRow row in _TrackingTables.Rows)
                sql += ", [TrackingStatus].[" + MakeSimpleField(row["TrackingStatusFieldName"].ToString()) + "]";

            sql += " FROM (([SLDestructCertItems] " + " LEFT JOIN [TrackingStatus] ON ([SLDestructCertItems].[TableName] = [TrackingStatus].[TrackedTable] AND [SLDestructCertItems].[TableId] = [TrackingStatus].[TrackedTableId])) " + " LEFT JOIN [Tables] ON [SLDestructCertItems].[TableName] = [Tables].[TableName]) " + " LEFT JOIN [SLRetentionCodes] ON [SLDestructCertItems].[RetentionCode] = [SLRetentionCodes].[Id] " + " LEFT JOIN SecureUser ON SLDestructCertItems.ApprovedBy = SecureUser.UserName " + " WHERE ([SLDestructCertItems].[DispositionDate] IS NULL AND [SLDestructCertItems].[SLDestructionCertsId] = @SLDestructionCertsId) " + "   AND ([SLDestructCertItems].[LegalHold] IS NULL OR [SLDestructCertItems].[LegalHold] = 0) " + "   AND ([SLDestructCertItems].[RetentionHold] IS NULL OR [SLDestructCertItems].[RetentionHold] = 0) " + "   AND ([SLRetentionCodes].[RetentionLegalHold] IS NULL OR [SLRetentionCodes].[RetentionLegalHold] = 0) ";

            sql += " ORDER BY [SLDestructCertItems].Id";

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SLDestructionCertsId ", ddlid);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        RetentionMassageDataForRequests(dt);
                        RetentionRemoveUnneededColumns(dt, string.Empty);
                        // DataTableReport(dt, False, True, False)
                    }
                }
            }
            return dt;
        }
        // this function are using for counting total rows of Retention Final Disposition
        private void RetentionFinalDisposition_QueryTableCount()
        {

            if (this._passport.CheckPermission("Disposition", SecureObject.SecureObjectType.Retention, Permissions.Permission.Access))
            {
                lblTitle = Languages.get_Translation("mnuViewMenuControlElegibleForFinalDisposition");
                // get trackble containers
                _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
                // check for btn conditions
                RetentionFinalDisposition_btnConditions();
                // check if call was from dropdown
                if (!UI.isQueryFromDDL)
                {
                    RetentionFinalDisposition_Dropdown();
                }
                else
                {
                    ddlid = Conversions.ToInteger(UI.id);
                }
            }

            string sql = "SELECT COUNT(*) ";

            sql += " FROM (([SLDestructCertItems] " + " LEFT JOIN [TrackingStatus] ON ([SLDestructCertItems].[TableName] = [TrackingStatus].[TrackedTable] AND [SLDestructCertItems].[TableId] = [TrackingStatus].[TrackedTableId])) " + " LEFT JOIN [Tables] ON [SLDestructCertItems].[TableName] = [Tables].[TableName]) " + " LEFT JOIN [SLRetentionCodes] ON [SLDestructCertItems].[RetentionCode] = [SLRetentionCodes].[Id] " + " LEFT JOIN SecureUser ON SLDestructCertItems.ApprovedBy = SecureUser.UserName " + " WHERE ([SLDestructCertItems].[DispositionDate] IS NULL AND [SLDestructCertItems].[SLDestructionCertsId] = @SLDestructionCertsId) " + "   AND ([SLDestructCertItems].[LegalHold] IS NULL OR [SLDestructCertItems].[LegalHold] = 0) " + "   AND ([SLDestructCertItems].[RetentionHold] IS NULL OR [SLDestructCertItems].[RetentionHold] = 0) " + "   AND ([SLRetentionCodes].[RetentionLegalHold] IS NULL OR [SLRetentionCodes].[RetentionLegalHold] = 0)";

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SLDestructionCertsId ", ddlid);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }

        }
        private void RetentionFinalDisposition_Dropdown()
        {
            var dTomorrow = DateTime.Now.AddDays(1d);
            string Sql = "SELECT DISTINCT [SLDestructionCerts].[Id], [SLDestructionCerts].[DateDestroyed], [SLDestructionCerts].[DateCreated], ISNULL([SecureUser].[DisplayName], [SLDestructionCerts].[CreatedBy]) AS OperatorsId, DispositionTypeDesc = CASE WHEN ([SLDestructionCerts].[RetentionDispositionType] = '1') THEN 'Archived' " + " WHEN ([SLDestructionCerts].[RetentionDispositionType] = '2') THEN 'Destruction' WHEN ([SLDestructionCerts].[RetentionDispositionType] = '3') THEN 'Purged' END " + " FROM [SLDestructionCerts] " + " INNER JOIN [SLDestructCertItems] ON [SLDestructCertItems].[SLDestructionCertsId] = [SLDestructionCerts].[Id] " + " LEFT OUTER JOIN [SLRetentionCodes] ON [SLRetentionCodes].[Id] = [SLDestructCertItems].[RetentionCode] " + " LEFT OUTER JOIN [SecureUser] ON [SecureUser].[UserName] = [SLDestructionCerts].[CreatedBy] " + " WHERE ([SLDestructionCerts].[RetentionDispositionType] IN ({0})) " + "   AND ([SLDestructCertItems].[DispositionDate] IS NULL) " + "   AND ([SLDestructCertItems].[LegalHold] IS NULL OR [SLDestructCertItems].[LegalHold] = 0) " + "   AND ([SLDestructCertItems].[RetentionHold] IS NULL OR [SLDestructCertItems].[RetentionHold] = 0) " + string.Format("   AND ([SLDestructCertItems].[SnoozeUntil] IS NULL OR [SLDestructCertItems].[SnoozeUntil] < '{0}-{1}-{2}') ", dTomorrow.Year, dTomorrow.Month, dTomorrow.Day) + "   AND ([SLRetentionCodes].[RetentionLegalHold] IS NULL OR [SLRetentionCodes].[RetentionLegalHold] = 0) " + " ORDER BY [SLDestructionCerts].[Id] DESC";
            using (var conn = this._passport.Connection())
            {
                Sql = string.Format(Sql, RetentionFinalDispositiontypesInUse());
                using (var cmd = new SqlCommand(Sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        int start = 0;
                        string spacer = new string(' ', 1);
                        string extra;
                        foreach (DataRow row in dt.Rows)
                        {
                            if (start == 0)
                            {
                                ddlid = Conversions.ToInteger(row["id"]);
                                start = start + 1;
                            }
                            int maxDateLength = TrimAMPMIndicators(new DateTime(2000, 12, 31, 12, 59, 59).ToString("g", CultureInfo)).Length;
                            int maxIdLength = dt.Rows[0]["Id"].ToString().Length;
                            string item = string.Empty;
                            int maxUserLength = Conversions.ToInteger(GetMaxUserLength(dt));
                            extra = Keys.get_ConvertCultureDate(row["DateCreated"].ToString(),_httpContext, false, true, true);
                            item = string.Format("{0}{1}", spacer, extra.PadRight(maxDateLength, ' '));
                            item += string.Format("{0}{1}{0}{2}", spacer, row["OperatorsId"].ToString().PadRight(maxUserLength, ' '), row["DispositionTypeDesc"].ToString());
                            ddlSelectReport.Add(new DDLItems() { value = Conversions.ToString(row["Id"]), text = item, Id = Conversions.ToString(row["Id"]) });
                        }
                    }
                }
            }
        }
        private object RetentionFinalDispositiontypesInUse()
        {
            string typesInUse = "SELECT TOP 1 (STUFF((SELECT ',' + CAST(i.[RetentionFinalDisposition] AS VARCHAR) " + " FROM [Tables] i WHERE (i.RetentionFinalDisposition <> 0 AND i.RetentionFinalDisposition IS NOT NULL AND i.RetentionPeriodActive <> 0 AND i.RetentionPeriodActive IS NOT NULL) FOR XML PATH('')), 1, 1, '')) " + " FROM [Tables] t WHERE (t.RetentionFinalDisposition <> 0 AND t.RetentionFinalDisposition IS NOT NULL AND t.RetentionPeriodActive <> 0 AND t.RetentionPeriodActive IS NOT NULL)";
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(string.Empty, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        cmd.CommandText = typesInUse;
                        typesInUse = cmd.ExecuteScalar().ToString();
                    }
                }
            }
            return typesInUse;
        }
        private void RetentionFinalDisposition_btnConditions()
        {
            using (var conn = this._passport.Connection())
            {
                if (Retention.UsesRetentionType(FinalDisposition.PermanentArchive, conn))
                {
                    PermanentArchive.isBtnVisibal = true;
                    PermanentArchive.btnText = Languages.get_Translation("mnuHTMLReportsArchivalReport");
                }
                if (Retention.UsesRetentionType(FinalDisposition.Purge, conn))
                {
                    Purge.isBtnVisibal = true;
                    Purge.btnText = Languages.get_Translation("mnuHTMLReportsPurgeReport");
                }
                if (Retention.UsesRetentionType(FinalDisposition.Destruction, conn))
                {
                    Destruction.isBtnVisibal = true;
                    Destruction.btnText = Languages.get_Translation("mnuHTMLReportsDestructionReport");
                }
            }
            SubmitDisposition.btnText = Languages.get_Translation("mnuHTMLReportsSubmitForDisposition");
            SubmitDisposition.isBtnVisibal = PermanentArchive.isBtnVisibal || Purge.isBtnVisibal || Destruction.isBtnVisibal;
        }
        // Citation with retention codes
        private void RetentionCitationsWithRetCodes()
        {
            lblTitle = Languages.get_Translation("tiHTMLReportsCitationswithRetentionCodes");
            var dt = new DataTable();
            dt = RetentionCitationsWithRetCodes_QueryTable();
            RetentionCitationsWithRetCodes_QueryTableCount();
            RetentionHeaders(dt, false, false);
            RetentionRows(dt, false, false, false);
        }

        private DataTable RetentionCitationsWithRetCodes_QueryTable()
        {
            string sql = "SELECT Ci.Citation, Ci.Subject, Ci.LegalPeriod AS [Citation Legal Period], C.Id AS [Retention Code], C.Description AS [Retention Description],  " + "C.RetentionPeriodLegal AS [Retention Legal Period], C.RetentionPeriodUser AS [Retention User Period], C.RetentionPeriodTotal AS [Retention Total Period] " + "FROM ([SLRetentionCitations] Ci " + "LEFT JOIN [SLRetentionCitaCodes] CC ON Ci.[Citation] = CC.[SLRetentionCitationsCitation]) " + "LEFT JOIN [SLRetentionCodes] C ON CC.[SLRetentionCodesId] = C.[Id] ORDER BY Citation";

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        private void RetentionCitationsWithRetCodes_QueryTableCount()
        {
            string sql = "SELECT  count(*) FROM ([SLRetentionCitations] Ci " + "LEFT JOIN [SLRetentionCitaCodes] CC ON Ci.[Citation] = CC.[SLRetentionCitationsCitation]) " + "LEFT JOIN [SLRetentionCodes] C ON CC.[SLRetentionCodesId] = C.[Id]";

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }
        }
        // retention code report
        private void RetentionCodes()
        {
            lblTitle = Languages.get_Translation("tiHTMLReportsAllRetCoses");
            var dt = new DataTable();
            dt = RetentionCodes_QueryTable();
            RetentionCodes_QueryTableCount();
            RetentionHeaders(dt, false, false);
            RetentionRows(dt, false, false, false);
        }

        private DataTable RetentionCodes_QueryTable()
        {
            string sql = "SELECT Id AS [Code], Description, DeptOFRecord AS [Department of Record], InactivityEventType AS [Inactivity Event Type]," + "InactivityPeriod AS [Inactivity Period], InactivityForceToEndOfYear AS [Inactivity Force To Year End], RetentionEventType AS [Retention Event Type], " + "RetentionPeriodLegal AS [Retention Legal Period], RetentionPeriodUser AS [Retention User Period], RetentionPeriodTotal AS [Retention Total Period], " + "RetentionLegalHold AS [Legal Hold], RetentionPeriodForceToEndOfYear AS [Retention Force to Year End]" + " FROM [SLRetentionCodes] ORDER BY Id";

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        private void RetentionCodes_QueryTableCount()
        {
            string sql = "SELECT Count(*) FROM [SLRetentionCodes]";

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }
        }

        // retention code with citations
        private void RetentionCodesWithCitations()
        {
            lblTitle = Languages.get_Translation("tiHTMLReportsRetCodesCitations");

            var dt = new DataTable();
            dt = RetentionCodesWithCitations_QueryTable();
            RetentionCodesWithCitations_QueryTableCount();
            RetentionHeaders(dt, false, false);
            RetentionRows(dt, false, false, false);
        }

        private DataTable RetentionCodesWithCitations_QueryTable()
        {
            string sql = "SELECT C.Id AS Code, C.Description, C.DeptOfRecord AS [Department of Record], " + "C.RetentionPeriodLegal AS [Retention Legal Period], C.RetentionPeriodUser AS [Retention User Period], C.RetentionPeriodTotal AS [Retention Total Period], " + "Ci.Citation, Ci.Subject, Ci.LegalPeriod AS [Legal Period] " + "FROM ([SLRetentionCodes] C " + "LEFT JOIN [SLRetentionCitaCodes] CC ON C.Id = CC.[SLRetentionCodesId]) " + "LEFT JOIN [SLRetentionCitations] Ci ON CC.[SLRetentionCitationsCitation] = Ci.Citation ORDER BY C.Id";

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        private void RetentionCodesWithCitations_QueryTableCount()
        {
            string sql = "SELECT count(*) FROM ([SLRetentionCodes] C " + "LEFT JOIN [SLRetentionCitaCodes] CC ON C.Id = CC.[SLRetentionCodesId]) " + "LEFT JOIN [SLRetentionCitations] Ci ON CC.[SLRetentionCitationsCitation] = Ci.Citation";

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }

        }

        // citations
        private void RetentionCitations()
        {
            lblTitle = Languages.get_Translation("tiHTMLReportsAllCitations");
            var dt = new DataTable();
            dt = RetentionCitations_QueryTable();
            RetentionCitations_QueryTableCount();
            RetentionHeaders(dt, false, false);
            RetentionRows(dt, false, false, false);
        }

        private DataTable RetentionCitations_QueryTable()
        {
            string sql = "SELECT Citation, Subject, LegalPeriod AS [Legal Period] FROM [SLRetentionCitations] ORDER BY Citation";

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        private void RetentionCitations_QueryTableCount()
        {
            string sql = "SELECT count(*) FROM [SLRetentionCitations]";
            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }
        }
        // retention record on hold
        private void RetentionRecordsOnHold()
        {
            lblTitle = Languages.get_Translation("tiHTMLReportsAllRecsOnHold");
            RepositoryVB.IRepository<TabFusionRMS.Models.SLRetentionInactive> SLRetentionInactive = new RepositoryVB.Repositories<TabFusionRMS.Models.SLRetentionInactive>();
            var objSLRetentionInactive = new TabFusionRMS.Models.SLRetentionInactive();
            SLRetentionInactive.Add(objSLRetentionInactive);
            SLRetentionInactive.Delete(objSLRetentionInactive);
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());

            string insertQuery = "INSERT INTO [SLRetentionInactive] ([TableName], [TableId], [Batch], [RetentionCode], [ScheduledInactivity], " + "[EventDate], [DeptOfRecord], [HoldReason], [LegalHold], [RetentionHold], [SLDestructCertItemId]) SELECT [SLDestructCertItems].[TableName], " + "[SLDestructCertItems].[TableId], " + objSLRetentionInactive.Id.ToString() + ", [SLDestructCertItems].[RetentionCode], [SLDestructCertItems].[ScheduledDestruction], " + "[SLDestructCertItems].[SnoozeUntil], [SLRetentionCodes].[DeptOfRecord], [SLDestructCertItems].[HoldReason], [SLDestructCertItems].[LegalHold], " + "[SLDestructCertItems].[RetentionHold], [SLDestructCertItems].[Id] FROM ([SLDestructCertItems] LEFT JOIN [SLRetentionCodes] ON [SLDestructCertItems].[RetentionCode] = [SLRetentionCodes].[Id]) WHERE ([SLDestructCertItems].[LegalHold] <> 0) Or ([SLDestructCertItems].[RetentionHold] <> 0)";

            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            RepositoryVB.IRepository<TabFusionRMS.Models.Table> table = new RepositoryVB.Repositories<TabFusionRMS.Models.Table>();
            var result = table.Where(x => x.RetentionPeriodActive == true).ToList();

            foreach (var item in result)
            {
                var data = new DataTable();
                string tableName = item.TableName;
                string idFieldName = item.IdFieldName;

                string sqlQuery = string.Format("SELECT {0}.*, {1} AS [TableId], RetentionEventType, RetentionPeriodTotal, [SLRetentionCodes].[Id] AS [RetentionCode], " + " [SLRetentionCodes].[DeptOfRecord] AS [DeptOfRecord], [SLRetentionCodes].[RetentionLegalHold] AS [RetentionCodeHold], 'Retention Code on Hold' AS [HoldReason] ", tableName, idFieldName) + string.Format("  FROM ([{0}] LEFT JOIN [SLRetentionCodes] ON {0}.{1} = [SLRetentionCodes].[Id]) ", tableName, item.RetentionFieldName) + string.Format("  LEFT JOIN ([SLDestructCertItems] LEFT JOIN [SLDestructionCerts] ON [SLDestructCertItems].[SLDestructionCertsId] = [SLDestructionCerts].[Id]) ON {0} = [SLDestructCertItems].[TableId] ", idFieldName) + string.Format("   AND [SLDestructCertItems].[TableName] = '{0}' ", tableName) + string.Format(" WHERE ([SLRetentionCodes].[RetentionLegalHold] <> 0) AND (([SLDestructCertItems].[TableName] = '{0}') OR ([SLDestructCertItems].[TableName] IS NULL)) AND (([SLDestructionCerts].[DateDestroyed] = 0) OR ([SLDestructionCerts].[DateDestroyed] IS NULL))", tableName);

                using (var conn = this._passport.Connection())
                {
                    using (var cmd = new SqlCommand(sqlQuery, conn))
                    {
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(data);
                        }
                    }
                }

                if (data.Rows.Count > 0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        string tableId = row["TableId"].ToString();
                        var DoesExist = SLRetentionInactive.All().FirstOrDefault(x => x.TableName == tableName & x.TableId == tableId & x.Batch == objSLRetentionInactive.Id);

                        if (DoesExist is not null)
                        {
                            DoesExist.RetentionCodeHold = Conversions.ToBoolean(1);
                            SLRetentionInactive.Update(DoesExist);
                        }
                        else
                        {
                            string retentionEventType = Convert.ToString(row["RetentionEventType"]);
                            string dateField = string.Empty;
                            var ScheduledInactivity = new DateTime();

                            if (retentionEventType == "Date Opened")
                            {
                                dateField = item.RetentionDateOpenedField;
                            }
                            else if (retentionEventType == "Date Created")
                            {
                                dateField = item.RetentionDateCreateField;
                            }
                            else if (retentionEventType == "Date Closed")
                            {
                                dateField = item.RetentionDateClosedField;
                            }
                            else if (retentionEventType == "Date Other")
                            {
                                dateField = item.RetentionDateOtherField;
                            }

                            if (!string.IsNullOrWhiteSpace(dateField) && !string.IsNullOrWhiteSpace(row[dateField].ToString()))
                            {
                                ScheduledInactivity = Conversions.ToDate(row[dateField]).AddYears(Conversions.ToInteger(row["RetentionPeriodTotal"]));
                            }

                            string insertRow = "INSERT INTO SLRetentionInactive(TableName, TableId, Batch, RetentionCode, ScheduledInactivity, DeptOfRecord, RetentionCodeHold, SLDestructCertItemId) " + "VALUES(@TableName, @TableId, @Batch, @RetentionCode, @ScheduledInactivity, @DeptOfRecord, @RetentionCodeHold, @SLDestructCertItemId)";

                            using (var conn = this._passport.Connection())
                            {
                                using (var cmd = new SqlCommand(insertRow, conn))
                                {
                                    cmd.Parameters.AddWithValue("@TableName", tableName);
                                    cmd.Parameters.AddWithValue("@TableId", tableId);
                                    cmd.Parameters.AddWithValue("@Batch", objSLRetentionInactive.Id);
                                    cmd.Parameters.AddWithValue("@RetentionCode", row["RetentionCode"]);

                                    if (ScheduledInactivity == DateTime.MinValue)
                                    {
                                        cmd.Parameters.AddWithValue("@ScheduledInactivity", DBNull.Value);
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@ScheduledInactivity", ScheduledInactivity);
                                    }

                                    cmd.Parameters.AddWithValue("@DeptOfRecord", row["DeptOfRecord"]);
                                    cmd.Parameters.AddWithValue("@RetentionCodeHold", 1);
                                    cmd.Parameters.AddWithValue("@SLDestructCertItemId", 0);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }

            string sql = "Select  ri.TableName AS [Folder Type], ri.TableName, '' AS Item, ri.TableId, ri.DeptofRecord AS [Department of Record], ri.RetentionCode AS [Retention Code]," + "ri.ScheduledInactivity AS [Scheduled Disposition], ri.EventDate AS [Snooze Until], ri.HoldReason AS [Reason], ri.LegalHold AS [Legal Hold], ri.RetentionHold  " + "as [Retention Hold], ri.RetentionCodeHold AS [Retention Code Hold], [TrackingStatus].[LocationsId], [Tables].[UserName]  " + "FROM (([SLRetentionInactive] ri " + "LEFT JOIN [TrackingStatus] ON (ri.[TableName] = [TrackingStatus].[TrackedTable] AND ri.[TableId] = [TrackingStatus].[TrackedTableId]))  " + "LEFT JOIN [Tables] ON ri.[TableName] = [Tables].[TableName]) WHERE ri.Batch = " + objSLRetentionInactive.Id.ToString() + "";

            sql += " ORDER BY ri.TableId ";

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            if (UI.isCountRecord)
            {
                string sqlCount = "Select count(*) " + "FROM (([SLRetentionInactive] ri " + "LEFT JOIN [TrackingStatus] ON (ri.[TableName] = [TrackingStatus].[TrackedTable] AND ri.[TableId] = [TrackingStatus].[TrackedTableId]))  " + "LEFT JOIN [Tables] ON ri.[TableName] = [Tables].[TableName]) WHERE ri.Batch = " + objSLRetentionInactive.Id.ToString() + "";

                RetentionRecordsOnHold_QueryTableCount(sqlCount);

            }

            var dt = new DataTable();
            string orderClause = " ORDER BY ri.TableId";
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        RetentionMassageDataForRequests(dt);
                        RetentionRemoveUnneededColumns(dt, string.Empty);
                        // DataTableReport(dt, False, True, False)
                    }
                }
            }
            var delete = SLRetentionInactive.Where(x => x.Batch == objSLRetentionInactive.Id).ToList();
            SLRetentionInactive.DeleteRange(delete);
            RetentionHeaders(dt, false, false);
            RetentionRows(dt, true, false, false);
        }

        private void RetentionRecordsOnHold_QueryTableCount(object sql)
        {
            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(Conversions.ToString(sql), conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }
        }

        // inactive record
        private void RetentionInactiveRecords()
        {
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            lblTitle = Languages.get_Translation("mnuViewMenuControlInactiveRecs");
            var dt = RetentionInactiveRecords_Query();
            RetentionHeaders(dt, false, false);
            RetentionRows(dt, true, false, false);
        }
        private DataTable RetentionInactiveRecords_Query()
        {

            string sql = "SELECT [Tables].[UserName] AS [Folder Type], [SLRetentionInactive].[TableName], CAST([FileRoomOrder] AS VARCHAR) AS [File Room Order], " + "'' AS [Item],'' AS [Currently At], [SLRetentionCodes].[InactivityEventType] AS [Event Type], [SLRetentionInactive].[TableId], " + "[SLRetentionInactive].[EventDate] AS [Event Date], [SLRetentionInactive].[ScheduledInactivity] AS [Scheduled Inactivity]";

            foreach (DataRow row in _TrackingTables.Rows)
                sql = sql + ", TrackingStatus." + row["TrackingStatusFieldName"].Text();

            sql = sql + " FROM (([SLRetentionInactive] " + "LEFT JOIN [TrackingStatus] ON ([SLRetentionInactive].[TableName] = [TrackingStatus].[TrackedTable] AND [SLRetentionInactive].[TableId] = [TrackingStatus].[TrackedTableId])) " + "LEFT JOIN [Tables] ON [SLRetentionInactive].[TableName] = [Tables].[TableName]) " + "LEFT JOIN [SLRetentionCodes] ON [SLRetentionInactive].[RetentionCode] = [SLRetentionCodes].[Id] ORDER BY [SLRetentionInactive].ID ";

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        RetentionMassageDataForRequests(dt);
                        RetentionRemoveUnneededColumns(dt, string.Empty);
                    }
                }
            }

            return dt;
        }
        private void RetentionInactiveRecords_QueryTableCount()
        {
            string sql = "SELECT count(*) ";
            sql = sql + " FROM (([SLRetentionInactive] " + "LEFT JOIN [TrackingStatus] ON ([SLRetentionInactive].[TableName] = [TrackingStatus].[TrackedTable] AND [SLRetentionInactive].[TableId] = [TrackingStatus].[TrackedTableId])) " + "LEFT JOIN [Tables] ON [SLRetentionInactive].[TableName] = [Tables].[TableName]) " + "LEFT JOIN [SLRetentionCodes] ON [SLRetentionInactive].[RetentionCode] = [SLRetentionCodes].[Id] ";
            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }
        }
        // inactive pull list
        private void InactivePullList()
        {
            if (!this._passport.CheckPermission(" Retention", SecureObject.SecureObjectType.Reports, Permissions.Permission.View) | !this._passport.CheckLicense(SecureObject.SecureObjectType.Retention))
            {
                DisplayNotAuthorized = Languages.get_Translation("msgHTMLReportsTheInactivePullList");
                return;
            }
            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            lblTitle = Languages.get_Translation("tiHTMLReportsInactivePullList");

            // btnCommand.Text = Languages.Translation("btnHTMLReportsSetInactive")

            var dt = InactivePullList_Query();
            RetentionHeaders(dt, false, true);
            RetentionRows(dt, true, false, true);
        }

        private DataTable InactivePullList_Query()
        {

            string sql = "SELECT [Tables].[UserName] AS FolderType, [SLRetentionInactive].[TableName], CAST([FileRoomOrder] AS VARCHAR) AS [File Room Order], " + "'' AS [Item], '' AS [Currently At], [SLRetentionInactive].[TableId], [SLRetentionInactive].[EventDate] AS [Event Date]," + "[SLRetentionCodes].[InactivityEventType] AS [Event Type],[SLRetentionInactive].[ScheduledInactivity] AS [Scheduled Inactivity]";

            foreach (DataRow row in _TrackingTables.Rows)
            {
                sql = sql + ", TrackingStatus." + row["TrackingStatusFieldName"].ToString();
            }

            sql = sql + " FROM (([SLRetentionInactive] " + "LEFT JOIN [TrackingStatus] ON ([SLRetentionInactive].[TableName] = [TrackingStatus].[TrackedTable] AND [SLRetentionInactive].[TableId] = [TrackingStatus].[TrackedTableId])) " + "LEFT JOIN [Tables] ON [SLRetentionInactive].[TableName] = [Tables].[TableName]) " + "LEFT JOIN [SLRetentionCodes] ON [SLRetentionInactive].[RetentionCode] = [SLRetentionCodes].[Id]";

            sql += " ORDER BY [SLRetentionInactive].ID";

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = new DataTable();
            // Dim orderClause = " ORDER BY [SLRetentionInactive].ID"
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        RetentionMassageDataForRequests(dt);
                        RetentionRemoveUnneededColumns(dt, string.Empty);
                    }
                }
            }

            return dt;
        }
        private void InactivePullList_QueryTableCount()
        {

            string sql = "SELECT count(*) ";

            sql = sql + " FROM (([SLRetentionInactive] " + "LEFT JOIN [TrackingStatus] ON ([SLRetentionInactive].[TableName] = [TrackingStatus].[TrackedTable] AND [SLRetentionInactive].[TableId] = [TrackingStatus].[TrackedTableId])) " + "LEFT JOIN [Tables] ON [SLRetentionInactive].[TableName] = [Tables].[TableName]) " + "LEFT JOIN [SLRetentionCodes] ON [SLRetentionInactive].[RetentionCode] = [SLRetentionCodes].[Id]";

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }
        }
        // Retention Certifie Disposition
        private void RetentionCertifieDisposition()
        {

            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            lblTitle = Languages.get_Translation("tiHTMLReportsCertiOfDisposition");
            if (UI.isQueryFromDDL)
            {
                ddlid = Conversions.ToInteger(UI.id);
            }
            else
            {
                RetentionCertifieDisposition_DropDown();
            }

            var dt = RetentionCertifieDisposition_Query();
            // RetentionCertifieDisposition_QueryTableCount()
            RetentionHeaders(dt, false, false);
            RetentionRows(dt, false, false, false);
        }
        private void RetentionCertifieDisposition_DropDown()
        {
            lblSelectReport = Languages.get_Translation("mnuHTMLReportsCertiOfDispositionRepo");
            string sql = "SELECT Id, DateDestroyed, DateCreated FROM SlDestructionCerts WHERE NOT DateDestroyed IS NULL order by id desc";
            var dt = new DataTable();

            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            int start = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (start == 0)
                {
                    ddlid = Conversions.ToInteger(row["Id"]);
                }
                start = start + 1;
                ddlSelectReport.Add(new DDLItems() { text = Keys.get_ConvertCultureDate(row["DateCreated"].ToString(),_httpContext, false, true, true), value = Conversions.ToString(row["Id"]), Id = Conversions.ToString(row["Id"]) });
            }
        }
        private DataTable RetentionCertifieDisposition_Query()
        {
            string sql = "SELECT [SLDestructCertItems].TableName, [Tables].UserName AS [Table Name],[SLDestructCertItems].TableId, '' AS Item,[SLRetentionCodes].[DeptOfRecord] AS [Department of Record], " + "[SLRetentionCodes].[Id] AS [Retention Code], [SLRetentionCodes].[RetentionEventType] AS [Event Type], [SLDestructCertItems].EventDate AS [Event Date],  " + "[SLDestructCertItems].ScheduledDestruction AS [Scheduled Disposition]," + "[Type of Disposition] = CASE " + "    WHEN ([SLDestructCertItems].[DispositionType] = '1') THEN 'Archived'" + "    WHEN ([SLDestructCertItems].[DispositionType] = '2') THEN 'Destroyed'" + "    WHEN ([SLDestructCertItems].[DispositionType] = '3') THEN 'Purged' " + "END";

            foreach (DataRow row in _TrackingTables.Rows)
                sql += ", TrackingStatus." + row["TrackingStatusFieldName"].ToString();

            sql += ", ISNULL(SecureUser.DisplayName, SLDestructCertItems.ApprovedBy) AS [Authorized by]" + "FROM (([SLDestructCertItems] " + "LEFT JOIN [TrackingStatus] ON ([SLDestructCertItems].[TableName] = [TrackingStatus].[TrackedTable] AND [SLDestructCertItems].[TableId] = [TrackingStatus].[TrackedTableId])) " + "LEFT JOIN [Tables] ON [SLDestructCertItems].[TableName] = [Tables].[TableName]) " + "LEFT JOIN [SLRetentionCodes] ON [SLDestructCertItems].[RetentionCode] = [SLRetentionCodes].[Id] " + "LEFT JOIN SecureUser ON SLDestructCertItems.ApprovedBy = SecureUser.UserName " + "WHERE [SLDestructCertItems].[SLDestructionCertsID] = @param AND [SLDestructCertItems].[DispositionType] <> 0 ORDER BY [Tables].[TableName], [SLDestructCertItems].[EventDate]";

            // NOTE: query will through exception if does not include ORDER BY
            sql += QueryPaging(UI.pageNumber, Paging.PerPageRecord);

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@param", ddlid);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        RetentionMassageDataForRequests(dt);
                        RetentionRemoveUnneededColumns(dt, string.Empty);
                    }
                }
            }
            return dt;
        }
        private void RetentionCertifieDisposition_QueryTableCount()
        {

            _TrackingTables = Tracking.GetTrackingContainerTypes(this._passport.Connection());
            lblTitle = Languages.get_Translation("tiHTMLReportsCertiOfDisposition");
            if (UI.isQueryFromDDL)
            {
                ddlid = Conversions.ToInteger(UI.id);
            }
            else
            {
                RetentionCertifieDisposition_DropDown();
            }

            string sql = "SELECT count(*) ";

            sql += " FROM (([SLDestructCertItems] " + "LEFT JOIN [TrackingStatus] ON ([SLDestructCertItems].[TableName] = [TrackingStatus].[TrackedTable] AND [SLDestructCertItems].[TableId] = [TrackingStatus].[TrackedTableId])) " + "LEFT JOIN [Tables] ON [SLDestructCertItems].[TableName] = [Tables].[TableName]) " + "LEFT JOIN [SLRetentionCodes] ON [SLDestructCertItems].[RetentionCode] = [SLRetentionCodes].[Id] " + "LEFT JOIN SecureUser ON SLDestructCertItems.ApprovedBy = SecureUser.UserName " + "WHERE [SLDestructCertItems].[SLDestructionCertsID] = @param AND [SLDestructCertItems].[DispositionType] <> 0";

            var dt = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@param", ddlid);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            if (dt.Rows.Count > 0)
            {
                Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
            }
            else
            {
                Paging.Execute(0);
            }
        }
        // helper functions
        protected void RetentionMassageDataForRequests(DataTable data)
        {
            var requestorRow = Tracking.GetRequestorTableInfo(this._passport);
            string requestorTableName = requestorRow["TableName"].ToString();
            var idsByTable = new Dictionary<string, string>();
            var listOfTableNames = new List<string>();
            idsByTable.Add(requestorTableName, "");
            listOfTableNames.Add(requestorTableName);

            foreach (DataRow row in data.Rows)
            {
                if (!idsByTable.ContainsKey(row["TableName"].ToString()))
                {
                    idsByTable.Add(row["TableName"].ToString(), "");
                    listOfTableNames.Add(row["TableName"].ToString());
                }

                if (string.IsNullOrEmpty(idsByTable[row["TableName"].ToString()]))
                {
                    idsByTable[row["TableName"].ToString()] += "'" + row["TableId"].ToString() + "'";
                }
                else
                {
                    idsByTable[row["TableName"].ToString()] += ",'" + row["TableId"].ToString().Replace("'", "''") + "'";
                }
            }

            var tablesInfo = Navigation.GetMultipleTableInfo(listOfTableNames, this._passport);
            var descriptions = new Dictionary<string, DataTable>();

            foreach (DataRow tableInfo in tablesInfo.Rows)
            {
                string ids = string.Empty;
                if (idsByTable.TryGetValue(tableInfo["TableName"].ToString(), out ids)) // If we have ids, prepopulate; otherwise it'll have to get each one individually
                {
                    descriptions.Add(tableInfo["TableName"].ToString(), Navigation.GetItemNames(tableInfo["TableName"].ToString(), this._passport, tableInfo, ids));
                }
            }

            foreach (DataRow row in data.Rows)
            {
                row["Item"] = Navigation.ExtractItemName(row["TableName"].ToString(), row["TableId"].ToString(), descriptions, tablesInfo, this._passport);
                if (data.Columns.Contains("Currently At"))
                    row["Currently At"] = GetTrackingItemName(row);
                if (data.Columns.Contains("Requestor") && !string.IsNullOrEmpty(row[requestorRow["TrackingStatusFieldName"].ToString()].ToString()))
                {
                    row["Requestor"] = Navigation.ExtractItemName(requestorTableName, row[requestorRow["TrackingStatusFieldName"].ToString()].ToString(), descriptions, tablesInfo, this._passport);
                }
            }
        }
        private void RetentionRemoveUnneededColumns(DataTable data, string status)
        {
            // _TrackingTables = GetTrackingContainerTypes(_passport.Connection())
            switch (status.ToLower() ?? "")
            {
                case "new":
                    {
                        if (data.Columns.Contains("Status"))
                            data.Columns.Remove(data.Columns["Status"]);
                        if (data.Columns.Contains("Date Pulled"))
                            data.Columns.Remove(data.Columns["Date Pulled"]);
                        break;
                    }
                case "newbatchesreport":
                    {
                        if (data.Columns.Contains("Date Pulled"))
                            data.Columns.Remove(data.Columns["Date Pulled"]);
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            if (data.Columns.Contains("EmployeeId"))
                data.Columns.Remove(data.Columns["EmployeeId"]);
            if (data.Columns.Contains("UserName"))
                data.Columns.Remove(data.Columns["UserName"]);

            foreach (DataRow row in _TrackingTables.Rows)
            {
                if (data.Columns.Contains(row["TrackingStatusFieldName"].ToString()))
                    data.Columns.Remove(data.Columns[row["TrackingStatusFieldName"].ToString()]);
            }
        }
        private void RetentionHeaders(DataTable data, bool auditReport, bool ishiddenField)
        {
            // written for if table need the hidden field which provide tableid and tableName
            if (ishiddenField)
                ListOfHeader.Add("id");
            foreach (DataColumn col in data.Columns)
            {
                if (!auditReport && !IsSystemColumn(col.ColumnName) || auditReport && (!IsSystemColumn(col.ColumnName) || string.Compare(col.ColumnName, "tablename", true) == 0))
                {
                    if (col.ExtendedProperties["heading"] is null)
                    {
                        if (string.Compare(col.ColumnName, col.Caption) != 0)
                        {
                            ListOfHeader.Add(col.Caption);
                        }
                        else
                        {
                            ListOfHeader.Add(col.ColumnName);
                        }
                    }
                    else
                    {
                        ListOfHeader.Add(Conversions.ToString(col.ExtendedProperties["heading"]));
                    }
                }
                else
                {
                    // add logic
                }
            }
        }
        private void RetentionRows(DataTable data, bool replaceCrLf, bool auditReport, bool ishiddenField)
        {
            string tableid = string.Empty;
            string tableName = string.Empty;
            foreach (DataRow row in data.Rows)
            {
                var cell = new List<string>();
                foreach (DataColumn col in data.Columns)
                {
                    if (!auditReport && !IsSystemColumn(col.ColumnName) || auditReport && (!IsSystemColumn(col.ColumnName) || string.Compare(col.ColumnName, "tablename", true) == 0))
                    {
                        if (replaceCrLf && "|data before|databefore|data after|dataafter|".IndexOf(string.Format("|{0}|", col.ColumnName.ToLower())) > -1)
                        {
                            cell.Add(row[col].ToString().Replace(Constants.vbCrLf, "<br>"));
                        }
                        else if (IsADateType(col.DataType))
                        {
                            if (!string.IsNullOrEmpty(row[col].ToString()))
                            {
                                cell.Add(Conversions.ToDate(row[col].ToString()).ToClientDateFormat());
                            }
                            else
                            {
                                cell.Add("");
                            }
                        }
                        else if (!string.IsNullOrEmpty(row[col].ToString()))
                        {
                            cell.Add(row[col].ToString());
                        }
                        else
                        {
                            cell.Add("");

                        }
                    }
                    // written for if table need the hidden field which provide tableid and tableName
                    else if (ishiddenField)
                    {
                        if (col.ColumnName.ToLower() == "tableid")
                        {
                            // cell.Add(row(col).ToString)
                            tableid = row[col].ToString();
                        }
                        if (col.ColumnName.ToLower() == "tablename")
                        {
                            // cell.Add(row(col).ToString)
                            tableName = row[col].ToString();
                        }
                    }
                }
                if (ishiddenField)
                {
                    var cells = new List<string>();
                    cells.Add(tableid + "||" + tableName);
                    // this extra loop written for rearranging the loop
                    foreach (string item in cell)
                        cells.Add(item);
                    ListOfRows.Add(cells);
                }
                else
                {
                    ListOfRows.Add(cell);
                }
            }
        }
    }

    public class RetentionButtons : BaseModel
    {
        public RetentionButtons(Passport passport)
        {
            this._passport = passport;
            ddlSelection = new List<DDLItems>();
        }
        public string username { get; set; }
        public string TodayDate { get; set; }
        public List<DDLItems> ddlSelection { get; set; }
        public bool isBtnVisibal { get; set; }
        public string btnText { get; set; }
        public string btnSubmitText { get; set; }
        public string btnSetSubmitType { get; set; }

        public void GetInactivePopup()
        {
            try
            {
                var user = new User(this._passport, true);
                TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
                username = user.UserName;
                var locationList = Tracking.GetInactiveLocations(this._passport);
                if (locationList.Count > 0)
                {
                    foreach (var location in locationList)
                        ddlSelection.Add(new DDLItems() { text = location.Text(), value = location.Value });
                }
            }
            catch (Exception)
            {
                // need to return error message
            }
        }
        public void GetSubmitForm(string submitType)
        {
            try
            {
                var user = new User(this._passport, true);
                TodayDate = DateTime.Now.ToString("yyyy-MM-dd");
                username = user.UserName;
                if (submitType == "archived")
                {
                    var locationList = Tracking.GetArchiveLocations(_passport);
                    btnSubmitText = "Archive";
                    btnSetSubmitType = "archive";
                    if (locationList.Count > 0)
                    {
                        foreach (var item in locationList)
                            ddlSelection.Add(new DDLItems() { value = item.Value, text = item.Text() });
                    }
                }
                if (submitType == "destruction")
                {
                    btnSubmitText = "Destruction";
                    btnSetSubmitType = "destruction";
                }
                if (submitType == "purge")
                {
                    btnSubmitText = "Purge";
                    btnSetSubmitType = "purge";
                }
            }

            catch (Exception)
            {
                // need to return error message
            }
        }
    }

    public class ReportingJsonModel
    {
        public int reportType { get; set; }
        public int pageNumber { get; set; }
        public string tableName { get; set; } = "";
        public List<items> ListofPullItem { get; set; }
        public string id { get; set; } = "";
        public bool isQueryFromDDL { get; set; }
        public bool isBatchRequest { get; set; }
        public List<string> ids { get; set; }
        public string udate { get; set; }
        public string ddlSelected { get; set; }
        public string username { get; set; }
        public string locationId { get; set; }
        public string submitType { get; set; }
        public int reportId { get; set; }
        public bool isCountRecord { get; set; }
    }

    public class ReportingJsonModelReq
    {
        public ReportingJsonModel paramss { get; set; }
    }

    public class PagingModel
    {
        public int TotalPage;
        public int TotalRecord;
        public int PerPageRecord;
        public int PageNumber;
        public PagingModel()
        {
            PerPageRecord = 1000;
        }
        public void Execute(int totalRecord)
        {
            TotalRecord = totalRecord;
            if (TotalRecord > 0)
            {
                if (TotalRecord / (double)PerPageRecord > 0d & TotalRecord / (double)PerPageRecord < 1d)
                {
                    TotalPage = 1;
                }
                else if (TotalRecord % PerPageRecord == 0)
                {
                    TotalPage = (int)Math.Round(TotalRecord / (double)PerPageRecord);
                }
                else
                {
                    int tp = (int)Math.Round(Conversion.Int(TotalRecord / (double)PerPageRecord));
                    TotalPage = tp + 1;
                }
            }
        }
    }
    // this model are using for same property for Report
    public class ReportCommonModelx
    {
        public ReportCommonModelx()
        {
            Paging = new PagingModel();
        }
        public PagingModel Paging { get; set; }
    }

    public class items
    {
        public string tableName { get; set; }
        public string tableid { get; set; }
    }

    public class DDLItems
    {
        public string text { get; set; }
        public string value { get; set; }
        public string Id { get; set; }
    }

    public enum ReportsType
    {
        PastDueTrackableItemsReport,
        ObjectOut,
        ObjectsInventory,
        RequestNew,
        RequestNewBatch,
        RequestPullList,
        RequestException,
        RequestInProcess,
        RequestWaitList,
        RetentionFinalDisposition,
        RetentionCertifieDisposition,
        RetentionInactivePullList,
        RetentionInactiveRecords,
        RetentionRecordsOnHold,
        RetentionCitations,
        RetentionCitationsWithRetCodes,
        RetentionCodes,
        RetentionCodesWithCitations
    }

    public enum SubmitType
    {
        Purged,
        Archived,
        Destroyed
    }
}