
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
using Smead.RecordsManagement;
using static Smead.RecordsManagement.Navigation;
using static Smead.RecordsManagement.Tracking;
using Smead.Security;
using TabFusionRMS.Resource;

namespace TabFusionRMS.WebCS.FusionElevenModels
{
    // reporting per row
    public class ReportingPerRow : BaseModel
    {
        public ReportingPerRow(string tableid, string tablename, int viewid, int pagenumber, Passport passport, HttpContext httpContext)
        {
            _tableName = tablename.Replace("'", "''");
            _Tableid = tableid.Replace("'", "''");
            _viewId = viewid;
            _passport = passport;
            _pageNum = pagenumber;
            hasPermission = true;
            ListOfHeader = new List<string>();
            ListOfRows = new List<List<string>>();
            Paging.PageNumber = pagenumber;
            _httpContext = httpContext;
        }
        public PagingModel Paging { get; set; } = new PagingModel();
        private string _Tableid { get; set; }
        private string _tableName { get; set; }
        private int _viewId { get; set; }
        private int _pageNum { get; set; }
        private string ReportName { get; set; }
        public List<string> ListOfHeader { get; set; }
        public List<List<string>> ListOfRows { get; set; }
        public bool hasPermission { get; set; }
        public int TotalPages { get; set; }
        public int RowsPerPage { get; set; }
        public string Title { get; set; }
        public string ItemDescription { get; set; }
        public void ExecuteReporting(Reports reportName)
        {
            // offset pages start from 
            int offsetPages;
            if (_pageNum == 1)
            {
                offsetPages = 0;
            }
            else
            {
                offsetPages = _pageNum * 300;
            }
            // get item description
            ItemDescription = GetItemName(_tableName, _Tableid, _passport);
            switch (reportName)
            {
                case Reports.AuditHistoryPerRow:
                    {
                        var model = new AuditHistoryPerRow();
                        model.GenerateAuditHistoryPerRow(offsetPages, this);
                        break;
                    }
                case Reports.TrackingHistoryPerRow:
                    {
                        var model = new TrackingHistoryPerRow();
                        model.GenerateTrackingHistoryPerRow(offsetPages, this, _httpContext);
                        break;
                    }
                case Reports.ContentsRow:
                    {
                        var model = new ContentsPerRow();
                        model.GenerateContentPerRow(offsetPages, this, _httpContext);
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }

        public void ExecuteReportingCount(Reports reportName)
        {
            // offset pages start from 

            ItemDescription = GetItemName(_tableName, _Tableid, _passport);
            switch (reportName)
            {
                case Reports.AuditHistoryPerRow:
                    {
                        var model = new AuditHistoryPerRow();
                        model.GenerateAuditHistoryPerRowCount(this);
                        break;
                    }
                case Reports.TrackingHistoryPerRow:
                    {
                        var model = new TrackingHistoryPerRow();
                        model.GenerateTrackingHistoryPerRowCount(this, _httpContext);
                        break;
                    }
                case Reports.ContentsRow:
                    {
                        var model = new ContentsPerRow();
                        model.GenerateContentPerRowCount(this);
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }

        private class AuditHistoryPerRow
        {
            public void GenerateAuditHistoryPerRow(int offsetValue, ReportingPerRow report)
            {
                if (!report._passport.CheckPermission(" Auditing", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                {
                    report.Msg = Languages.get_Translation("lblHTMLReportsAuditHisRepo");
                    report.hasPermission = false;
                    return;
                }
                // headers
                GenerateHeader(report);
                GenerateRows(offsetValue, report);
            }
            public void GenerateAuditHistoryPerRowCount(ReportingPerRow report)
            {
                if (!report._passport.CheckPermission(" Auditing", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                {
                    report.Msg = Languages.get_Translation("lblHTMLReportsAuditHisRepo");
                    report.hasPermission = false;
                    return;
                }
                // headers
                GenerateRowsCount(report);
            }
            private void GenerateHeader(ReportingPerRow report)
            {
                report.ListOfHeader.Add(Languages.get_Translation("lblStatusDate"));
                report.ListOfHeader.Add(Languages.get_Translation("ARF_lblUser"));
                report.ListOfHeader.Add(Languages.get_Translation("lblNetworking"));
                report.ListOfHeader.Add(Languages.get_Translation("lbldomain"));
                report.ListOfHeader.Add(Languages.get_Translation("lblCompName"));
                report.ListOfHeader.Add(Languages.get_Translation("lblMacAddress"));
                report.ListOfHeader.Add(Languages.get_Translation("lblIp"));
                report.ListOfHeader.Add(Languages.get_Translation("lblAction"));
                report.ListOfHeader.Add(Languages.get_Translation("lblDataBefore"));
                report.ListOfHeader.Add(Languages.get_Translation("lblDataAfter"));
            }
            private DataTable sqlQuery(int offsetValue, ReportingPerRow report)
            {
                string tableid = PrepPad(report._tableName, report._Tableid, report._passport);
                // sql query
                string sql = string.Format(string.Format("SELECT CONVERT(VARCHAR, [UpdateDateTime], 100) AS 'Date', [OperatorsId] AS 'User', [NetworkLoginName] AS 'Network Login'," + " [Domain], [ComputerName], [MacAddress], [IP], [Action], [DataBefore], [DataAfter] FROM [SLAuditUpdates]" + " WHERE TableName = '{0}' AND TableId = '{1}' order by Id offset {2} rows fetch next 1000 rows only", report._tableName, tableid, offsetValue));
                var data = new DataTable();
                using (var conn = report._passport.Connection())
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(data);
                        }
                    }
                }
                return data;
            }

            private DataTable sqlQueryPaging(int offsetValue, ReportingPerRow report)
            {
                string tableid = PrepPad(report._tableName, report._Tableid, report._passport);
                // sql query
                string sql = string.Format(string.Format("SELECT CONVERT(VARCHAR, [UpdateDateTime], 100) AS 'Date', [OperatorsId] AS 'User', [NetworkLoginName] AS 'Network Login'," + " [Domain], [ComputerName], [MacAddress], [IP], [Action], [DataBefore], [DataAfter] FROM [SLAuditUpdates]" + " WHERE TableName = '{0}' AND TableId = '{1}' ORDER BY UpdateDateTime DESC ", report._tableName, tableid));

                sql += Query.QueryPaging(report.Paging.PageNumber, report.Paging.PerPageRecord);

                var data = new DataTable();
                using (var conn = report._passport.Connection())
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(data);
                        }
                    }
                }
                return data;
            }

            private DataTable sqlQueryPagingCount(ReportingPerRow report)
            {
                string tableid = PrepPad(report._tableName, report._Tableid, report._passport);
                // sql query
                string sql = string.Format(string.Format("SELECT count(*) FROM [SLAuditUpdates]" + " WHERE TableName = '{0}' AND TableId = '{1}' ", report._tableName, tableid));

                var data = new DataTable();
                using (var conn = report._passport.Connection())
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(data);
                        }
                    }
                }
                return data;
            }

            private void GenerateRows(int offsetValue, ReportingPerRow report)
            {
                // rows
                var data = sqlQueryPaging(offsetValue, report);
                foreach (DataRow row in data.Rows)
                {
                    var cell = new List<string>();
                    foreach (DataColumn col in data.Columns)
                        cell.Add(row[col.Caption].ToString());
                    report.ListOfRows.Add(cell);
                }
            }
            private void GenerateRowsCount(ReportingPerRow report)
            {
                // rows
                var data = sqlQueryPagingCount(report);
                if (data.Rows.Count > 0)
                {
                    report.Paging.Execute(Convert.ToInt32(data.Rows[0][0]));
                }
                else
                {
                    report.Paging.Execute(0);
                }
            }
        }
        private class TrackingHistoryPerRow
        {
            public void GenerateTrackingHistoryPerRow(int offsetValue, ReportingPerRow report, HttpContext httpContext)
            {
               
                if (!report._passport.CheckSetting(report._tableName, SecureObject.SecureObjectType.Table, Permissions.Permission.Transfer))
                {
                    report.Msg = Languages.get_Translation("lblHTMLReportsTrackingHisRepo");
                    report.hasPermission = false;
                    return;
                }
                GenerateHeader(report);
                GenerateRows(report, httpContext);
            }
            public void GenerateTrackingHistoryPerRowCount(ReportingPerRow report, HttpContext httpContext)
            {
                if (!report._passport.CheckPermission(" Tracking", SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
                {
                    report.Msg = Languages.get_Translation("lblHTMLReportsTrackingHisRepo");
                    report.hasPermission = false;
                    return;
                }
                GenerateRowsCount(report, httpContext);
            }
            private void GenerateHeader(ReportingPerRow report)
            {
                // headers
                report.ListOfHeader.Add(Languages.get_Translation("tiHTMLReportsTransactionDate"));
                report.ListOfHeader.Add(Languages.get_Translation("lblStatusLocation"));
                report.ListOfHeader.Add(Languages.get_Translation("tiHTMLReportsScanOperator"));
            }
            private void GenerateRows(ReportingPerRow report, HttpContext httpContext)
            {
                var model = new TrackingReport(report._passport, report._tableName, report._Tableid);
                var list = model.GetTrackableHistoryPaging(report.Paging.PageNumber, report.Paging.PerPageRecord);
                if (list is null)
                    return;
                // rows
                for (int i = 0, loopTo = list.Count - 1; i <= loopTo; i++)
                {
                    var cell = new List<string>();
                    var sb = new StringBuilder();
                    cell.Add(Conversions.ToString(list[i].TransactionDate));
                    for (int j = 0, loopTo1 = list[i].Containers.Count - 1; j <= loopTo1; j++)
                        sb.AppendLine(list[i].Containers[j].Type + ":       " + list[i].Containers[j].Name);
                    cell.Add(sb.ToString());
                    cell.Add(list[i].UserName);
                    report.ListOfRows.Add(cell);
                }
            }
            private void GenerateRowsCount(ReportingPerRow report, HttpContext httpContext)
            {
                var model = new TrackingReport(report._passport, report._tableName, report._Tableid);
                var list = model.GetTrackableHistoryCount();
                if (list.Rows.Count > 0)
                {
                    report.Paging.Execute(Convert.ToInt32(list.Rows[0][0]));
                }
                else
                {
                    report.Paging.Execute(0);
                }
            }

            private void GenerateRowsPaging(ReportingPerRow report, HttpContext httpContext)
            {
                var model = new TrackingReport(report._passport, report._tableName, report._Tableid);
                var list = model.GetTrackableHistoryPaging(report.Paging.PageNumber, report.Paging.PerPageRecord);
                // rows
                for (int i = 0, loopTo = list.Count - 1; i <= loopTo; i++)
                {
                    var cell = new List<string>();
                    var sb = new StringBuilder();
                    cell.Add(Conversions.ToString(list[i].TransactionDate));
                    for (int j = 0, loopTo1 = list[i].Containers.Count - 1; j <= loopTo1; j++)
                        sb.AppendLine(list[i].Containers[j].Type + ":       " + list[i].Containers[j].Name);
                    cell.Add(sb.ToString());
                    cell.Add(list[i].UserName);
                    report.ListOfRows.Add(cell);
                }
            }
        }
        private class ContentsPerRow
        {
            public void GenerateContentPerRow(int offsetValue, ReportingPerRow report, HttpContext httpContext)
            {
                GenerateHeader(report);
                GenerateRows(report, httpContext);
            }
            public void GenerateContentPerRowCount(ReportingPerRow report)
            {
                GenerateRowsCount(report);
            }
            private void GenerateHeader(ReportingPerRow report)
            {
                report.ListOfHeader.Add("Tran Date");
                report.ListOfHeader.Add("Item Name");
                report.ListOfHeader.Add("UserName");
            }
            private void GenerateRows(ReportingPerRow report, HttpContext httpContext)
            {
                var dt = GetContainerContentsPaging(report._tableName, report._Tableid, report._passport, report.Paging.PageNumber, report.Paging.PerPageRecord);
                var idsByTable = new Dictionary<string, string>();
                var permissionsByTable = new Dictionary<string, bool>();
                var listOfTableNames = new List<string>();

                foreach (DataRow row in dt.Rows)
                {
                    if (!idsByTable.ContainsKey(row["TrackedTable"].ToString()))
                    {
                        idsByTable.Add(row["TrackedTable"].ToString(), "");
                        listOfTableNames.Add(row["TrackedTable"].ToString());
                        permissionsByTable.Add(row["TrackedTable"].ToString(), report._passport.CheckPermission(Conversions.ToString(row["TrackedTable"]), SecureObject.SecureObjectType.Table, Permissions.Permission.View));
                    }
                    if (string.IsNullOrEmpty(idsByTable[row["TrackedTable"].ToString()]))
                    {
                        idsByTable[row["TrackedTable"].ToString()] += "'" + row["TrackedTableID"].ToString() + "'";
                    }
                    else
                    {
                        idsByTable[row["TrackedTable"].ToString()] += ",'" + row["TrackedTableID"].ToString().Replace("'", "''") + "'";
                    }
                }

                var tablesInfo = GetMultipleTableInfo(listOfTableNames, report._passport);
                var descriptions = new Dictionary<string, DataTable>();

                foreach (DataRow tableInfo in tablesInfo.Rows)
                {
                    string ids = string.Empty;
                    if (idsByTable.TryGetValue(tableInfo["TableName"].ToString(), out ids)) // If we have ids, prepopulate; otherwise it'll have to get each one individually
                    {
                        descriptions.Add(tableInfo["TableName"].ToString(), GetItemNames(tableInfo["TableName"].ToString(), report._passport, tableInfo, ids));
                    }
                }

                foreach (DataRow row in dt.Rows)
                {
                    bool permission = false;
                    if (!permissionsByTable.TryGetValue(row["TrackedTable"].ToString(), out permission))
                    {
                        permission = report._passport.CheckPermission(Conversions.ToString(row["TrackedTable"]), SecureObject.SecureObjectType.Table, Permissions.Permission.View);
                    }
                    if (permission)
                    {
                        var cell = new List<string>();
                        cell.Add(Keys.get_ConvertCultureDate(row["TransactionDateTime"].ToString(), httpContext));
                        cell.Add(ExtractItemName(row["TrackedTable"].ToString(), row["TrackedTableID"].ToString(), descriptions, tablesInfo, report._passport));
                        cell.Add(Conversions.ToString(row["UserName"]).ToString());
                        report.ListOfRows.Add(cell);
                    }
                }
            }
            private void GenerateRowsCount(ReportingPerRow report)
            {
                var dt = GetContainerContentsCount(report._tableName, report._Tableid, report._passport);
                if (dt.Rows.Count > 0)
                {
                    report.Paging.Execute(Convert.ToInt32(dt.Rows[0][0]));
                }
                else
                {
                    report.Paging.Execute(0);
                }
            }
        }
        public enum Reports
        {
            AuditHistoryPerRow,
            TrackingHistoryPerRow,
            ContentsRow
        }
    }

    public class reportRowUIparams
    {
        public string Tableid { get; set; }
        public string tableName { get; set; }
        public int viewId { get; set; }
        public int reportNum { get; set; }
        public int pageNumber { get; set; }
        public string childid { get; set; }
        public int reportId { get; set; }
    }
    public class ReportingReqModel 
    {
        public reportRowUIparams paramss { get; set; }
    }
    
}