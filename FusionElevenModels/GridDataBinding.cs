using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
using Smead.RecordsManagement;
using static Smead.RecordsManagement.Navigation;
using Smead.Security;
using SecureObject = Smead.Security.SecureObject;
using TabFusionRMS.Models;
using View = TabFusionRMS.Models.View;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.Resource;
using System.Data.Common;
using static Smead.RecordsManagement.Tracking;
using System.Text.Json.Serialization;

namespace TabFusionRMS.WebCS.FusionElevenModels
{
    // GRID DATA MODEL RETURN DOM OBJECT AND DATA (MAIN OBJECT FOR GRID AND BUTTONS FUNCTIONS)
    public class GridDataBinding : BaseModel
    {
        public GridDataBinding(Passport passport, int viewId, int pageNumber, int crumblevel1, int viewtype, HttpContext httpContext)
            : this(passport, viewId, pageNumber, crumblevel1, viewtype, "", httpContext)
        {
        }
        public GridDataBinding(Passport passport, int viewId, int pageNumber, int crumblevel, int viewtype, string childKeyField, HttpContext httpContext)
        {
            this._passport = passport;
            _httpContext = httpContext;
            // Me._levelManager = LevelManger
            ListOfHeaders = new List<TableHeadersProperty>();
            ListofEditableHeader = new List<TableEditableHeader>();
            ListOfDatarows = new List<List<string>>();
            ListOfAttachmentLinks = new List<string>();
            ListOfColumn = new List<string>();
            ViewId = viewId;
            ListOfdropdownColumns = new List<DropDownproperties>();
            PageNumber = pageNumber;
            fvList = new List<FieldValue>();
            RightClickToolBar = new RightclickToolBar();
            ListOfBreadCrumbsRightClick = new List<BreadCrumbsRightClick>();
            ListOfBreadCrumbs = new List<BreadCrumbsUI>();
            crumbLevel = crumblevel;
            fViewType = viewtype;
            ChildKeyField = String.IsNullOrEmpty(childKeyField) ? String.Empty : childKeyField;
        }

        internal Parameters @params { get; set; }
        private int fViewType;
        private TableColums Cell;
        private List<string> ListOfColumn { get; set; }

        //[JsonIgnore]
        //private IRepository<Table> _tables { get; set; }

        //[JsonIgnore]
        //public IRepository<View> _iView { get; set; }

        internal string WhereClauseStr { get; set; }
        internal bool IsWhereClauseRequest { get; set; } = false;
        internal bool GsIsGlobalSearch { get; set; } = false;
        internal string GsKeyvalue { get; set; }
        internal string GsSearchText { get; set; }
        internal bool GsIsAllGlobalRequest { get; set; }
        internal bool GsIncludeAttchment { get; set; }
        public string ToolBarHtml { get; set; }
        public List<FieldValue> fvList { get; set; }
        public List<TableHeadersProperty> ListOfHeaders { get; set; }
        public List<TableEditableHeader> ListofEditableHeader { get; set; }
        public List<List<string>> ListOfDatarows { get; set; }
        public bool HasAttachmentcolumn { get; set; }
        public bool HasDrillDowncolumn { get; set; }
        public List<string> ListOfAttachmentLinks { get; set; }
        public string ListOfdrilldownLinks { get; set; }
        public List<BreadCrumbsRightClick> ListOfBreadCrumbsRightClick { get; set; }
        public List<BreadCrumbsUI> ListOfBreadCrumbs { get; set; }
        public List<DropDownproperties> ListOfdropdownColumns { get; set; }
        public int PageNumber { get; set; }
        public int TotalRows { get; set; }
        public int RowPerPage { get; set; }
        public int TotalPagesNumber { get; set; }
        public bool ShowTrackableTable { get; set; }
        public string ChildKeyField { get; set; }
        public int ViewId { get; set; }
        public string ViewName { get; set; }
        public string TableName { get; set; }
        public RightclickToolBar RightClickToolBar { get; set; }
        internal bool IsOpenWhereClause { get; set; } = false;
        public string ChildKeyname { get; set; }
        public string Message { get; set; }
        public bool hasPermission { get; set; }
        public string ItemDescription { get; set; }
        public int crumbLevel { get; set; }
        public string dateFormat { get; set; }
        public List<SortableFileds> sortableFields { get; set; } = new List<SortableFileds>();
        internal string IdFieldDataType { get; set; }
        internal string TotalRowsQuery { get; set; }
        private void BuildNewTableData()
        {
            //Using conn As SqlConnection = _passport.Connection()
            // End Using
            var _query = new Query(this._passport);
            @params = new Parameters(ViewId, this._passport);

            @params.ParentField = ChildKeyField;
            IdFieldDataType = @params.IdFieldDataType.FullName;

            ViewName = @params.ViewName;
            TableName = @params.TableName;

            if (fvList.Count > 0)
            {
                fieldValueParams();
            }
            if (IsWhereClauseRequest)
            {
                WhereClauseParams();
            }
            if (GsIsGlobalSearch)
            {
                GlobalSearchParams();
            }
            @params.Paged = true;
            @params.PageIndex = PageNumber;
            IRepository<View> _iView = new Repositories<View>();
            var oViews = _iView.All().Where(x => x.Id == ViewId).FirstOrDefault();
            @params.RequestedRows = (int)oViews.MaxRecsPerFetch;
            RowPerPage = (int)oViews.MaxRecsPerFetch;
            @params.IsMVCCall = true;
            _query.FillData(@params);
            // get the string totalrow query
            TotalRowsQuery = @params.TotalRowsQuery;
            if (BuildDrillDownLinks() > 0)
            {
                HasDrillDowncolumn = true;
            }
            else
            {
                HasDrillDowncolumn = false;
            }
            // date format 
            dateFormat = Keys.GetUserPreferences.sPreferedDateFormat.ToString().Trim().ToUpper();
            // build header
            BuildNewTableHeaderData();
            // build toolbar buttons
            buildToolBarButtons(@params.Data.Rows.Count);
            // check if table is trackable
            IsTableTrackable();
            // get sortable fields
            SetSortablefields();
            // build breadcrumbs right click
            BuildBreadCrumbRightClick();
            Buildrows();
        }
        private void SetSortablefields()
        {
            foreach (RecordsManage.ViewColumnsRow row in Navigation.GetsortableFields(ViewId, this._passport))
            {
                string fieldname = string.Empty;
                if (row.FieldName.Contains("."))
                {
                    fieldname = MakeSimpleField(row.FieldName);
                }
                else
                {
                    fieldname = row.FieldName;
                }
                sortableFields.Add(new SortableFileds() { FieldName = fieldname, SortOrder = row.SortOrder, SortOrderDesc = row.SortOrderDesc });
            }
        }
        private void fieldValueParams()
        {
            @params.QueryType = queryTypeEnum.AdvancedFilter;
            @params.FilterList = fvList;
        }
        private void WhereClauseParams()
        {
            @params.QueryType = queryTypeEnum.OpenTable;
            @params.Scope = ScopeEnum.Table;
            @params.ParentField = string.IsNullOrEmpty(@params.ParentField)? String.Empty: @params.ParentField;
            @params.ParentValue = string.IsNullOrEmpty(@params.ParentValue) ? String.Empty : @params.ParentValue;
            if (IsOpenWhereClause)
            {
                @params.WhereClause = WhereClauseStr; // String.Format("{0} in ({1}) ", ChildKeyname, Me.WhereClauseStr)
            }
            else if (fvList.Count > 0)
            {
                @params.WhereClause = string.Format("{0} in ({1}) {2}", @params.KeyField, WhereClauseStr, @params.AndFilter);
            }
            else
            {
                @params.WhereClause = string.Format("{0} in ({1})", @params.KeyField, WhereClauseStr);
            }

        }
        private void GlobalSearchParams()
        {
            if (GsIsAllGlobalRequest)
            {
                @params.QueryType = queryTypeEnum.Text;
                @params.Text = GsSearchText;
                @params.Scope = ScopeEnum.Table;
                @params.IncludeAttachments = GsIncludeAttchment;
            }
            else
            {
                @params.QueryType = queryTypeEnum.KeyValuePair;
                @params.Scope = ScopeEnum.Table;
                @params.KeyField = Navigation.GetPrimaryKeyFieldName(Navigation.GetViewTableName(ViewId, this._passport), this._passport);
                @params.KeyValue = GsKeyvalue;
            }
        }
        private void Buildrows()
        {
            // build rows
            foreach (DataRow dr in @params.Data.Rows)
            {
                // 'get the pkey
                Cell = new TableColums();
                Cell.DataColumn = dr["pkey"].ToString();
                ListOfColumn.Add(Cell.DataColumn);
                if (HasDrillDowncolumn)
                {
                    Cell.DataColumn = "drilldown";
                    ListOfColumn.Add(Cell.DataColumn);
                }
                if (HasAttachmentcolumn)
                {
                    if (!string.IsNullOrEmpty(dr["Attachments"].ToString()))
                    {
                        Cell.DataColumn = dr["Attachments"].ToString();
                    }
                    else
                    {
                        Cell.DataColumn = "0";
                    }
                    ListOfColumn.Add(Cell.DataColumn);
                }

                foreach (DataColumn col in @params.Data.Columns)
                {
                    // If Not dr(col.ColumnName).GetType.ToString.ToLower = "system.boolean" And Not dr(col.ColumnName).GetType.ToString.ToLower = "system.datetime" Then
                    if (ShowColumn(col, crumbLevel, @params.ParentField) & col.ColumnName.ToString().Length > 0)
                    {
                        if (Convert.ToString(col.ColumnName) is not null)
                        {
                            if (!string.IsNullOrEmpty(dr[col.ColumnName].ToString()))
                            {
                                Cell.DataColumn = Convert.ToString(dr[col.ColumnName.ToString()]);
                            }
                            else
                            {
                                Cell.DataColumn = "";
                            }
                        }
                        ListOfColumn.Add(Cell.DataColumn);
                    }
                }
                ListOfDatarows.Add(ListOfColumn);
                ListOfColumn = new List<string>();
            }
        }
        private List<TableHeadersProperty> BuildNewTableHeaderData()
        {
            int columnOrder = 0;

            // hide column for pkey
            ListOfHeaders.Add(new TableHeadersProperty("pkey", "False", "none", "False", "False", columnOrder, "", false, "", "", false, 0, false));

            // if not hide the drill down column
            if (HasDrillDowncolumn)
            {
                columnOrder = columnOrder + 1;
                ListOfHeaders.Add(new TableHeadersProperty("drilldown", "False", "none", "False", "False", columnOrder, "", false, "", "", false, 0, false));
            }
            // HasDrillDowncolumn = False
            // If CheckIfviewHasDrilldown() > 0 Then
            // HasDrillDowncolumn = True
            // columnOrder = columnOrder + 1
            // ListOfHeaders.Add(New TableHeadersProperty("drilldown", "False", "none", "False", "False", columnOrder, "", False, "", "", False, 0))
            // End If

            // create attachment header
            HasAttachmentcolumn = false;
            bool checkViewPermission = this._passport.CheckPermission(@params.TableName, SecureObject.SecureObjectType.Attachments, Permissions.Permission.View);
            bool HasLicense = this._passport.CheckLicense(SecureObject.SecureObjectType.Attachments);
            if (checkViewPermission && HasLicense)
            {
                HasAttachmentcolumn = true;
                columnOrder = columnOrder + 1;
                ListOfHeaders.Add(new TableHeadersProperty("attachment", "False", "none", "False", "False", columnOrder, "", false, "", "", false, 0, false));
                // ListOfHeaders.Add("<i title='"" + Languages.Translation(""dataGridAttachment_AddAttachment"") + ""' class='fa fa-paperclip fa-flip-horizontal fa-2x theme_color'></i>" + "&&sorter:false")
            }

            // create table headers
            foreach (DataColumn col in @params.Data.Columns)
            {
                if (ShowColumn(col, crumbLevel, @params.ParentField))
                {
                    string dataType = col.DataType.Name;
                    var headerName = col.ExtendedProperties["heading"];
                    var isSortable = col.ExtendedProperties["sortable"];
                    var isdropdown = col.ExtendedProperties["dropdownflag"];
                    var isEditable = col.ExtendedProperties["editallowed"];
                    var editmask = col.ExtendedProperties["editmask"];
                    int MaxLength = col.MaxLength;
                    bool isCounterField = false;
                    if (dataType == "Int16")
                    {
                        MaxLength = 5;
                    }
                    else if (dataType == "Int32")
                    {
                        MaxLength = 10;
                    }
                    else if (dataType == "Double")
                    {
                        MaxLength = 53;
                    }

                    var dataTypeFullName = col.DataType.FullName;
                    string ColumnName = col.ColumnName;
                    columnOrder = columnOrder + 1;
                    // build dropdown table
                    if (Convert.ToBoolean(col.ExtendedProperties["dropdownflag"]))
                    {
                        BuildDropdownForcolumn(col, columnOrder);
                        if (((DataTable)col.ExtendedProperties["LookupData"]).Columns.Count > 1)
                        {
                            ColumnName = Navigation.MakeSimpleField(((DataTable)col.ExtendedProperties["LookupData"]).TableName);
                            //ColumnName = Navigation.MakeSimpleField(col.ExtendedProperties("LookupData").TableName);
                        }
                    }
                    bool PrimaryKey = false;
                    if ((@params.PrimaryKey ?? "") == (ColumnName ?? ""))
                    {
                        isCounterField = !string.IsNullOrEmpty(@params.TableInfo["CounterFieldName"].ToString());
                        ListOfHeaders.Add(new TableHeadersProperty(Convert.ToString(headerName).ToString(), Convert.ToString(isSortable), dataType, Convert.ToString(isdropdown), Convert.ToString(isEditable), columnOrder, Convert.ToString(editmask), col.AllowDBNull, dataTypeFullName, ColumnName, true, MaxLength, isCounterField));
                        PrimaryKey = true;
                    }
                    else
                    {
                        ListOfHeaders.Add(new TableHeadersProperty(Convert.ToString(headerName), Convert.ToString(isSortable), dataType, Convert.ToString(isdropdown), Convert.ToString(isEditable), columnOrder, Convert.ToString(editmask), col.AllowDBNull, dataTypeFullName, ColumnName, false, MaxLength, isCounterField));
                    }

                    // holding editable model for lader edit and new row (UI)
                    if (Convert.ToBoolean(isEditable))
                    {
                        string DefaultRetentionId = string.Empty;
                        if ((@params.TableInfo["RetentionFieldName"].ToString() ?? "") == (ColumnName ?? ""))
                        {
                            DefaultRetentionId = @params.TableInfo["DefaultRetentionId"].ToString();
                        }
                        ListofEditableHeader.Add(new TableEditableHeader() { HeaderName = Convert.ToString(headerName), Issort = Convert.ToBoolean(isSortable), DataType = dataType, isDropdown = Convert.ToBoolean(isdropdown), isEditable = Convert.ToBoolean(isEditable), columnOrder = columnOrder, editMask = Convert.ToString(editmask), Allownull = col.AllowDBNull, DataTypeFullName = dataTypeFullName, ColumnName = ColumnName, IsPrimarykey = PrimaryKey, MaxLength = MaxLength, isCounterField = isCounterField, DefaultRetentionId = DefaultRetentionId });
                    }

                }
            }

            return ListOfHeaders;
        }
        private void BuildDropdownForcolumn(DataColumn col1, int colorder)
        {
            var col = col1;
            var LookupData_Obj = col.ExtendedProperties["LookupData"];
            var displays = new StringBuilder(((DataTable)LookupData_Obj).Rows.Count);
            var values = new StringBuilder(((DataTable)LookupData_Obj).Rows.Count);

            //var displays = new StringBuilder(((DataTable)col.ExtendedProperties("LookupData")).Rows.Count);
            //var values = new StringBuilder(((DataTable)col.ExtendedProperties("LookupData")).Rows.Count);


            foreach (DataRow row in ((DataTable)LookupData_Obj).Rows)
            {
                if (((DataTable)LookupData_Obj).Columns.Count > 1)
                {
                    displays.Append(string.Format("{0},", row["Display"].ToString()));
                    values.Append(string.Format("{0},", row["Value"].ToString()));
                }
                else
                {
                    displays.Append(string.Format("{0},", row["Display"].ToString()));
                    values.Append(string.Format("{0},", row["Display"].ToString()));
                }
            }

            ListOfdropdownColumns.Add(new DropDownproperties(colorder, values.ToString(), displays.ToString()));
        }
        public int BuildDrillDownLinksV2()
        {
            var sb = new StringBuilder();
            string tables = "," + "" + ",";
            string lastTableName = null;
            int index = 0;
            foreach (var item in GetChildViews(this.ViewId, _passport))
            {
                if (tables.Contains("," + item.ChildTableName + ","))
                {
                    if (item.ChildTableName != lastTableName)
                    {
                        lastTableName = item.ChildTableName;
                        sb.Append(string.Format("<li><a data-location=\"3\" onclick=\"obJdrildownclick.Run(this,'{0}','{1}','{2}', '{3}', {4}, {5}, '{6}', '{7}')\">{3}</a></li>", item.ChildTableName, item.ChildKeyField, item.ChildViewID, item.ChildUserName, index, @params.ViewId, item.ChildViewName, item.ChildKeyType));
                        // item.ChildViewID,
                        // params.TableName,
                        // params.ViewName,
                        // params.ViewId,
                        // item.ChildUserName
                        ListOfBreadCrumbs.Add(new BreadCrumbsUI()
                        {
                            ChildKeyField = item.ChildKeyField,
                            ChildTableName = item.ChildTableName,
                            ChildUserName = item.ChildUserName,
                            ChildViewid = item.ChildViewID,
                            ChildViewName = item.ChildViewName,
                            TableName = @params.TableName,
                            ViewId = @params.ViewId,
                            ViewName = @params.ViewName,
                            ChildKeyType = item.ChildKeyType
                        });

                        index += 1;
                    }
                }
                var drillModel = new BreadCrumbsUI();
            }

            if (get_IsContainer(@params.TableName, _passport))
            {
                sb.Append(string.Format("<li><a onclick=\"obJreportsrecord.ContentsPerRow()\">{0}</a></li>", Languages.get_Translation("lnkDataContents")));
            }

            if (_passport.CheckPermission(" Auditing", SecureObject.SecureObjectType.Reports, Permissions.Permission.View) & CollectionsClass.IsAuditingEnabled(@params.TableName))
            {
                sb.Append(string.Format("<li><a onclick=\"obJreportsrecord.AuditHistoryRow()\">{0}</a></li>", Languages.get_Translation("lnkDataAuditHistory")));
            }
            // Start: Added RetentionInfo Link
            var retentionField = @params.TableInfo["RetentionFieldName"];

            if (@params.Data.Rows.Count > 0 && !string.IsNullOrEmpty(Convert.ToString(retentionField)) && (CBoolean(@params.TableInfo["RetentionPeriodActive"]) || CBoolean(@params.TableInfo["RetentionInactivityActive"])))
            {
                sb.Append(string.Format("<li><a onclick=\"obJretentioninfo.GetInfo()\">{0}</a></li>", Languages.get_Translation("lblEditDispositionStaRetentionInfo")));
            }
            // End: Added RetentionInfo Link
            // If _passport.CheckSetting(params.TableName, SecureObject.SecureObjectType.Table, Permissions.Permission.Transfer) AndAlso
            // _passport.CheckPermission(" Tracking", Smead.Security.SecureObject.SecureObjectType.Reports, Smead.Security.Permissions.Permission.View) Then
            // sb.Append(String.Format("<li><a onclick=""obJreportsrecord.TrackingHistoryRow()"">{0}</a></li>", Languages.Translation("TrackingHistory")))
            // End If
            if (_passport.CheckSetting(@params.TableName, SecureObject.SecureObjectType.Table, Permissions.Permission.Transfer))
            {
                sb.Append(string.Format("<li><a onclick=\"obJreportsrecord.TrackingHistoryRow()\">{0}</a></li>", Languages.get_Translation("TrackingHistory")));
            }
            // sb.Append("</ul>")
            ListOfdrilldownLinks = sb.ToString();
            return ListOfdrilldownLinks.Count();
        }
        public int BuildDrillDownLinks()
        {
            var sb = new StringBuilder();
            string tables = "," + "" + ",";
            string lastTableName = null;
            int index = 0;
            foreach (var item in Navigation.GetChildViews(ViewId, this._passport))
            {
                if (!tables.Contains("," + item.ChildTableName + ","))
                {
                    if ((item.ChildTableName ?? "") != (lastTableName ?? ""))
                    {
                        lastTableName = item.ChildTableName;
                        sb.Append(string.Format("<li><a data-location=\"3\" onclick=\"obJdrildownclick.Run(this,'{0}','{1}','{2}', '{3}', {4}, {5}, '{6}', '{7}')\">{3}</a></li>", item.ChildTableName, item.ChildKeyField, item.ChildViewID, item.ChildUserName, index, @params.ViewId, item.ChildViewName, item.ChildKeyType));
                        // item.ChildViewID,
                        // params.TableName,
                        // params.ViewName,
                        // params.ViewId,
                        // item.ChildUserName
                        ListOfBreadCrumbs.Add(new BreadCrumbsUI()
                        {
                            ChildKeyField = item.ChildKeyField,
                            ChildTableName = item.ChildTableName,
                            ChildUserName = item.ChildUserName,
                            ChildViewid = item.ChildViewID,
                            ChildViewName = item.ChildViewName,
                            TableName = @params.TableName,
                            ViewId = @params.ViewId,
                            ViewName = @params.ViewName,
                            ChildKeyType = item.ChildKeyType
                        });

                        index += 1;
                    }
                }
                var drillModel = new BreadCrumbsUI();
            }

            if (Tracking.get_IsContainer(@params.TableName, this._passport))
            {
                sb.Append(string.Format("<li><a onclick=\"obJreportsrecord.ContentsPerRow()\">{0}</a></li>", Languages.get_Translation("lnkDataContents")));
            }

            if (this._passport.CheckPermission(" Auditing", SecureObject.SecureObjectType.Reports, Permissions.Permission.View) & CollectionsClass.IsAuditingEnabled(@params.TableName))
            {
                sb.Append(string.Format("<li><a onclick=\"obJreportsrecord.AuditHistoryRow()\">{0}</a></li>", Languages.get_Translation("lnkDataAuditHistory")));
            }
            // Start: Added RetentionInfo Link
            string retentionField = @params.TableInfo["RetentionFieldName"].ToString();
            if (@params.Data.Rows.Count > 0 && !string.IsNullOrEmpty(retentionField) && (CBoolean(@params.TableInfo["RetentionPeriodActive"]) || CBoolean(@params.TableInfo["RetentionInactivityActive"])))
            {
                sb.Append(string.Format("<li><a onclick=\"obJretentioninfo.GetInfo()\">{0}</a></li>", Languages.get_Translation("lblEditDispositionStaRetentionInfo")));
            }
            // End: Added RetentionInfo Link
            // If _passport.CheckSetting(params.TableName, SecureObject.SecureObjectType.Table, Permissions.Permission.Transfer) AndAlso
            // _passport.CheckPermission(" Tracking", Smead.Security.SecureObject.SecureObjectType.Reports, Smead.Security.Permissions.Permission.View) Then
            // sb.Append(String.Format("<li><a onclick=""obJreportsrecord.TrackingHistoryRow()"">{0}</a></li>", Languages.Translation("TrackingHistory")))
            // End If
            if (this._passport.CheckSetting(@params.TableName, SecureObject.SecureObjectType.Table, Permissions.Permission.Transfer))
            {
                sb.Append(string.Format("<li><a onclick=\"obJreportsrecord.TrackingHistoryRow()\">{0}</a></li>", Languages.get_Translation("TrackingHistory")));
            }
            // sb.Append("</ul>")
            ListOfdrilldownLinks = sb.ToString();
            return ListOfdrilldownLinks.Count();
        }
        private void BuildBreadCrumbRightClick()
        {
            // get right click views
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand("SELECT Id, ViewName FROM Views WHERE TableName = @tableName AND (Printable IS NULL OR Printable = 0) order by ViewOrder", conn))
                {
                    cmd.Parameters.AddWithValue("@tableName", TableName);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            if (this._passport.CheckPermission(row["ViewName"].ToString(), SecureObject.SecureObjectType.View, Permissions.Permission.View))
                            {
                                ListOfBreadCrumbsRightClick.Add(new BreadCrumbsRightClick() { viewId = Convert.ToInt32(row["Id"]), viewName = Convert.ToString(row["ViewName"]) });
                            }

                            // Dim x = Convert.ToString(dt.Rows(0).ItemArray(0))
                        }
                        Convert.ToString(dt.Rows[0].ItemArray[0]);
                    }
                }
            }
        }
        public static bool ShowColumn(DataColumn col, int crumblevel, string parentField)
        {
            switch (Convert.ToInt32(col.ExtendedProperties["columnvisible"]))
            {
                case 3:  // Not visible
                    {
                        return false;
                    }
                case 1:  // Visible on level 1 only
                    {
                        if (crumblevel != 0)
                            return false;
                        break;
                    }
                case 2:  // Visible on level 2 and below only
                    {
                        if (crumblevel < 1)
                            return false;
                        break;
                    }
                case 4:  // Smart column- not visible in a drill down when it's the parent.
                    {
                        if (crumblevel > 0 & (parentField.ToLower() ?? "") == (col.ColumnName.ToLower() ?? ""))
                        {
                            return false;
                        }

                        break;
                    }
            }

            if (col.ColumnName.ToLower() == "formattedid")
                return false;
            // If col.ColumnName.ToLower = "id" Then Return False
            if (col.ColumnName.ToLower() == "attachments")
                return false;
            if (col.ColumnName.ToLower() == "slrequestable")
                return false;
            if (col.ColumnName.ToLower() == "itemname")
                return false;
            if (col.ColumnName.ToLower() == "pkey")
                return false;
            if (col.ColumnName.ToLower() == "dispositionstatus")
                return false;
            if (col.ColumnName.ToLower() == "processeddescfieldnameone")
                return false;
            if (col.ColumnName.ToLower() == "processeddescfieldnametwo")
                return false;
            if (col.ColumnName.ToLower() == "rownum")
                return false;
            return true;
        }
        // build toolbar and right click menu
        private void buildToolBarButtons(int rowCount)
        {
            var sb = new StringBuilder();
            ToolBarQueryButton(sb);
            ToolBarNewRecordButton(sb);
            if (rowCount > 0)
            {
                ToolBarFileButton(sb);
                ToolBarArrowButton(sb);
                ToolBarFavoriteButton(sb);
                sb.Append(string.Format("<input type=\"button\" name=\"saveRow\" value=\"{0}\" id=\"saveRow\" class=\"btn btn-secondary tab_btn\" style=\"min-width: 70px; margin-left:4px\" />", "Save Edit"));
                sb.Append(string.Format("<span style=\"margin-left: 11px\"> # of Rows Selected: <span id=\"rowcounter\"> 0</span></span>"));
                // sb.Append(Environment.NewLine + String.Format("<input type=""text"" style=""height: 34;width: 155px;""placeholder=""Search in page"" id=""searchInpage"">"))
            }
            sb.Append(string.Format("<span id=\"emptymsg\" style=\"display:none;\" class=\"emptymsg-txt\"><i>Click on <strong>New</strong> Button to Add Record(s) into the Table View</i></span>"));

            // sb.Append(Environment.NewLine + String.Format("<input type=""button"" style=""min-width: 80px;top:0"" id=""autosavebtn"" type=""button"" class=""btn btn-secondary tab_btn"" value=""{0}"" />", Languages.Translation("Autosave")))
            sb.Append(Environment.NewLine + string.Format(" <label class=\"switch pull-right\"><input type=\"checkbox\" id=\"autosavebtn\"><div class=\"slider round\"><span class=\"on\">ON</span><span class=\"off\">OFF</span></div></label><span class=\"pull-right\" style=\"position: relative;left: -9px; top: 4px;\">{0}</span>", Languages.get_Translation("Autosave")));
            ToolBarHtml = sb.ToString();
        }
        private void ToolBarQueryButton(StringBuilder sb)
        {
            sb.Append(string.Format("<button class=\"btn btn-secondary tab_btn\" onclick=\"obJgridfunc.RefreshGrid(this)\"><img src=\"/Content/themes/TAB/css/images/refresh30px.png\" width=\"20px;\"></button>"));
            sb.Append(string.Format("<input type=\"button\" name=\"btnQuery\" value=\"{0}\" id=\"btnQuery\" class=\"btn btn-secondary tab_btn\" style=\"min-width: 70px; margin-left:3px\" />", Languages.get_Translation("Query")));
        }
        private void ToolBarNewRecordButton(StringBuilder sb)
        {
            if (this._passport.CheckPermission(@params.ViewName, SecureObject.SecureObjectType.View, Permissions.Permission.Add) && !(fViewType == (int)ViewType.Favorite))
            {
                sb.Append(Environment.NewLine + string.Format("<input type=\"button\" onclick=\"obJaddnewrecord.LoadNewRowDialog()\" name=\"btnNew\" value=\"{0}\" id=\"btnNew\" class=\"btn btn-secondary tab_btn\" />", Languages.get_Translation("btnNew")));
            }
        }
        private void ToolBarFileButton(StringBuilder sb)
        {
            // CREATE Tool button dropdown file
            sb.Append(Environment.NewLine + "<div class=\"btn-group\">");
            sb.Append(Environment.NewLine + "<button class=\"btn btn-secondary dropdown-toggle tab_btn\" data-toggle=\"dropdown\" type=\"button\" aria-expanded=\"False\">");
            sb.Append(Environment.NewLine + "<i class=\"fa fa-file-text-o fa-fw\"></i>");
            sb.Append(Environment.NewLine + "<i class=\"fa fa-angle-down\"></i>");
            sb.Append(Environment.NewLine + "</button>");
            sb.Append(Environment.NewLine + "<ul class=\"dropdown-menu btn_menu\">");
            // add print button button
            if (this._passport.CheckPermission(@params.ViewName, SecureObject.SecureObjectType.View, Permissions.Permission.Print))
            {
                RightClickToolBar.Menu1Print = true;
                sb.Append(Environment.NewLine + string.Format("<li><a id=\"btnPrint\">{0}</a></li>", Languages.get_Translation("Print")));
            }
            // check if customer has license for tabquik
            bool hasTabquikLicense = !string.IsNullOrEmpty(Navigation.GetSetting("TABQUIK", "Key", this._passport));
            if (hasTabquikLicense)
            {
                var labelExists = Navigation.LabelExists(@params.TableName, this._passport);
                bool setbutton = labelExists == Navigation.Enums.eLabelExists.Color | labelExists == Navigation.Enums.eLabelExists.BWAndColor;
                if (setbutton)
                {
                    RightClickToolBar.Menu1Tabquick = true;
                    sb.Append(Environment.NewLine + string.Format("<li><a id=\"tabquikId\">{0}</a></li>", Languages.get_Translation("btnTabQuick")));
                }

            }
            // add print label
            var islabelExist = Navigation.LabelExists(TableName, this._passport);

            if (_passport.CheckPermission(@params.TableName, SecureObject.SecureObjectType.Table, Permissions.Permission.PrintLabel) && (islabelExist & Navigation.Enums.eLabelExists.BlackAndWhite) == Navigation.Enums.eLabelExists.BlackAndWhite)
            {
                RightClickToolBar.Menu1btnBlackWhite = true;
                sb.Append(Environment.NewLine + string.Format("<div id=\"ulPrintButtons\" class=\"div_listed\">"));
                sb.Append(Environment.NewLine + string.Format("<li><a id = \"btnBlackWhite\">{0}</a></li>", Languages.get_Translation("btnBlackWhite")));
                sb.Append(Environment.NewLine + string.Format("</div>"));
            }
            // add export 
            if (this._passport.CheckPermission(@params.ViewName, SecureObject.SecureObjectType.View, Permissions.Permission.Export))
            {
                RightClickToolBar.Menu1btnExportCSV = true;
                RightClickToolBar.Menu1btnExportCSVAll = true;
                RightClickToolBar.Menu1btnExportTXT = true;
                RightClickToolBar.Menu1btnExportTXTAll = true;
                sb.Append(string.Format("<li><a id=\"btnExportCSV\">{0}</a><a id=\"ButtonCSVHidden\" style=\"display: none;\">Export Selected (CSVHidden)</a></li>", Languages.get_Translation("Export") + "(csv)"));
                sb.Append(string.Format("<li><a id=\"btnExportCSVAll\">{0}</a></li>", Languages.get_Translation("ExportAll") + "(csv)"));
                sb.Append(string.Format("<li><a id=\"btnExportTXT\">{0}</a><a id=\"ButtinTXTHidden\" style=\"display: none;\">Export Selected (TXTHidden)</a></li>", Languages.get_Translation("Export") + "(txt)"));
                sb.Append(string.Format("<li><a id=\"btnExportTXTAll\">{0}</a></li>", Languages.get_Translation("ExportAll") + "(txt)"));
            }
            sb.Append(Environment.NewLine + "</ul>");
            sb.Append(Environment.NewLine + "</div>");
            // END Tool button dropdown file
        }
        private void ToolBarArrowButton(StringBuilder sb)
        {
            // CREATE tool button dropdown arrow
            sb.Append(Environment.NewLine + "<div class=\"btn-group\">");
            sb.Append(Environment.NewLine + "<button class=\"btn btn-secondary dropdown-toggle tab_btn\" data-toggle=\"dropdown\" type=\"button\">");
            sb.Append(Environment.NewLine + "<i class=\"fa fa-send-o fa-fw\"></i>");
            sb.Append(Environment.NewLine + "<i class=\"fa fa-angle-down\"></i>");
            sb.Append(Environment.NewLine + "</button>");

            sb.Append(Environment.NewLine + "<ul class=\"dropdown-menu btn_menu\">");
            LinkScriptLoadWorkFlowButtons(sb);
            if (this._passport.CheckPermission(@params.TableName, SecureObject.SecureObjectType.Table, Permissions.Permission.Request))
            {
                sb.Append(Environment.NewLine + string.Format("<li id=\"divRequestTransfer\"><a id=\"btnRequest\">{0}</a></li>", Languages.get_Translation("Request")));
            }

            if (HasAttachmentcolumn && this._passport.CheckPermission(" Orphans", SecureObject.SecureObjectType.Orphans, Permissions.Permission.Index) && this._passport.CheckPermission(" Orphans", SecureObject.SecureObjectType.Orphans, Permissions.Permission.View) && this._passport.CheckPermission(@params.TableName, SecureObject.SecureObjectType.Attachments, Permissions.Permission.Add) && checkOrphanVolumPermission())
            {
                sb.Append(Environment.NewLine + string.Format("<li id=\"divRequestTransfer\"><a onclick=\"obJvaultfunction.AttachOrphanRecord()\" id=\"OrphanAttachid\">{0}</a></li>", Languages.get_Translation("btnOrphanAttachment")));

            }
            if (this._passport.CheckPermission(@params.TableName, SecureObject.SecureObjectType.Table, Permissions.Permission.Transfer))
            {
                RightClickToolBar.Menu2btnRequest = true;
                RightClickToolBar.Menu2btnTransfer = true;
                RightClickToolBar.Menu2btnTransfersTransferAll = true;

                sb.Append(Environment.NewLine + string.Format("<li><a id=\"btnTransfer\">{0}</a></li>", Languages.get_Translation("btnTransfersTransferSelected")));
                sb.Append(Environment.NewLine + "<li><a id=\"ButtonTransferHidden\" style=\"display: none;\">Transfer(Hidden)</a></li>");
                sb.Append(string.Format(Environment.NewLine + string.Format("<li><a id=\"btnTransferAll\">{0}</a></li>", Languages.get_Translation("btnTransfersTransferAll"))));
            }
            if (this._passport.CheckPermission(@params.ViewName, SecureObject.SecureObjectType.View, Permissions.Permission.Delete))
            {
                RightClickToolBar.Menu2delete = true;
                sb.Append(Environment.NewLine + string.Format("<li><a id=\"btndeleterow\">{0}</a></li>", Languages.get_Translation("Delete")));
            }
            if (this._passport.CheckPermission(@params.TableName, SecureObject.SecureObjectType.Table, Permissions.Permission.Move))
            {
                RightClickToolBar.Menu2move = true;
                sb.Append(Environment.NewLine + string.Format("<li><a id=\"btnMoverows\">{0}</a></li>", Languages.get_Translation("Move")));
            }
            if (this._passport.CheckPermission(@params.TableName, SecureObject.SecureObjectType.Table, Permissions.Permission.Transfer))
            {
                RightClickToolBar.Menu2btnTracking = true;
                // sb.Append(String.Format("<li><a id=""btnTracking"" title=""Toggle tracking and request panes"">{0}</a></li>", Languages.Translation("btnTrackingShow")))
                sb.Append(Environment.NewLine + string.Format("<li><a id=\"btnTracking\" title=\"Toggle tracking and request panes\">{0}</a></li>", Languages.get_Translation("btnTrackingHide")));
            }
            sb.Append(Environment.NewLine + string.Format("<li><a onclick=\"obJlastquery.ResetQueries()\" title=\"reset queries\">{0}</a></li>", "Reset Queries"));
            sb.Append(Environment.NewLine + "</ul>");
            sb.Append(Environment.NewLine + "</div>");
        }
        private void ToolBarFavoriteButton(StringBuilder sb)
        {
            if (this._passport.CheckPermission(Common.SECURE_MYFAVORITE, SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
            {
                RightClickToolBar.Favorive = true;
                sb.Append(Environment.NewLine + "<div id=\"divFavOptions\" class=\"btn-group\">");
                sb.Append(Environment.NewLine + "<button class=\"btn btn-secondary dropdown-toggle tab_btn\" data-toggle=\"dropdown\" type=\"button\">");
                sb.Append(Environment.NewLine + "<i class=\"fa fa-heart-o fa-fw\"></i>");
                sb.Append(Environment.NewLine + "<i class=\"fa fa-angle-down\"></i>");
                sb.Append(Environment.NewLine + "</button>");
                sb.Append(Environment.NewLine + "<ul class=\"dropdown-menu btn_menu\">");
                sb.Append(Environment.NewLine + string.Format("<li><a id=\"btnAddFavourite\">{0}</a></li>", Languages.get_Translation("btnAddFavourite")));
                sb.Append(Environment.NewLine + string.Format("<li><a id=\"btnUpdateFavourite\">{0}</a></li>", Languages.get_Translation("btnUpdateFavourite")));
                sb.Append(Environment.NewLine + string.Format("<li id=\"lnkDeleteFavouriteRecords\" style=\"display: none\"><a id=\"btnDeleteFavourite\">{0}</a></li>", Languages.get_Translation("btnDeleteFavourite")));
                sb.Append(Environment.NewLine + string.Format("<li><a id=\"btnImportFavourite\">{0}</a></li>", Languages.get_Translation("btnImportFavourite")));
                sb.Append(Environment.NewLine + "</ul>");
                sb.Append(Environment.NewLine + "</div>");
            }
        }
        private void IsTableTrackable()
        {
            IRepository<Table> _tables = new Repositories<Table>();
            ShowTrackableTable = (bool)_tables.Where(a => (a.TableName ?? "") == (@params.TableName ?? "")).FirstOrDefault().Trackable;
        }
        // Private Sub ToolBarDeleteMyQuery(sb As StringBuilder)
        // sb.Append("<div id =""divMyQueryDelete"" style=""display:none;""><button id=""btnDeleteSearchCriteria"" class=""btn btn-secondary"" type=""button""><i class=""fa fa-trash-o""></i></button></div>")
        // End Sub
        public static List<FieldValue> CreateQuery(List<searchQueryModel> searchQuery, HttpContext httpContext)
        {
            var list = new List<FieldValue>();
            if (!(searchQuery == null))
            {
                foreach (var row in searchQuery)
                {
                    var fv = new FieldValue(row.columnName, row.ColumnType);
                    if (!string.IsNullOrEmpty(row.operators.Trim()))
                    {
                        fv.Operate = row.operators;
                        if (string.IsNullOrEmpty(row.values))
                        {
                            fv.value = "";
                        }
                        else if (row.ColumnType == "System.DateTime")
                        {
                            if (row.values.Contains("|"))
                            {
                                var dt = row.values.Split('|');
                                string checkFieldDateStart = Keys.get_ConvertCultureDate(dt[0], "E", httpContext);
                                string checkFieldDateEnd =Keys.get_ConvertCultureDate(dt[1], "E", httpContext);
                                fv.value = string.Format("{0}|{1}", checkFieldDateStart, checkFieldDateEnd);
                            }
                            else
                            {
                                fv.value =Keys.get_ConvertCultureDate(row.values, "E", httpContext);
                            }
                        }
                        else
                        {
                            fv.value = row.values;
                        }
                        list.Add(fv);
                    }
                }
            }
            return list;
        }
        private void LinkScriptLoadWorkFlowButtons(StringBuilder sb)
        {
            var dt = Navigation.GetViewWorkFlows(@params.ViewId, this._passport);
            string Title = "";
            string ButtonName = "";
            string ScriptId = "";
            if (dt.Rows.Count != 0)
            {
                for (int i = 1; i <= 5; i++)
                {
                    if (!ReferenceEquals(dt.AsEnumerable().ElementAtOrDefault(0)["WorkFlow" + i.ToString()], DBNull.Value) && this._passport.CheckPermission(Convert.ToString(dt.AsEnumerable().ElementAtOrDefault(0)["WorkFlow" + i.ToString()]), SecureObject.SecureObjectType.LinkScript, Permissions.Permission.Execute))
                    {
                        ScriptId = dt.AsEnumerable().ElementAtOrDefault(0)["WorkFlow" + i.ToString()].ToString();
                        if (!ReferenceEquals(dt.AsEnumerable().ElementAtOrDefault(0)["WorkFlowDesc" + i.ToString()], DBNull.Value))
                        {
                            ButtonName = dt.AsEnumerable().ElementAtOrDefault(0)["WorkFlowDesc" + i.ToString()].ToString();
                        }
                        if (!ReferenceEquals(dt.AsEnumerable().ElementAtOrDefault(0)["WorkFlowToolTip" + i.ToString()], DBNull.Value))
                        {
                            Title = dt.AsEnumerable().ElementAtOrDefault(0)["WorkFlowToolTip" + i.ToString()].ToString();
                        }

                        sb.Append(Environment.NewLine + "<li><span>");
                        sb.Append(Environment.NewLine + string.Format("<a id=\"{0}\" title=\"{1}\" onclick=\"obJlinkscript.ClickButton(this)\" >{2}</a>", ScriptId, Title, ButtonName));
                        sb.Append(Environment.NewLine + "</li></span>");
                    }
                }
            }
        }
        private bool checkOrphanVolumPermission()
        {
            IRepository<Volume> volume = new Repositories<Volume>();
            foreach (var v in volume.All().ToList())
            {
                if (this._passport.CheckPermission(v.Name, SecureObject.SecureObjectType.Volumes, Permissions.Permission.Add))
                {
                    return true;
                }
            }
            return false;
        }
        // execute method API
        public void ExecuteGridData()
        {
            BuildNewTableData();
        }
    }

    // BREADCRUMBS MODEL RETURN DOM OBJECT
    public class BreadCrumbsRightClick
    {
        public int viewId { get; set; }
        public string viewName { get; set; }
    }

    public class BreadCrumbsUI
    {
        public string ChildTableName { get; set; }
        public string ChildKeyField { get; set; }
        public string Childid { get; set; }
        public int ChildViewid { get; set; }
        public string TableName { get; set; }
        public string ViewName { get; set; }
        public int ViewId { get; set; }
        public string ChildUserName { get; set; }
        public string ChildViewName { get; set; }
        public string ChildKeyType { get; set; }
        public string preTableName { get; set; }
    }
    // RIGHT CLICK TOOLBAR RETURN TOOLBAR DOM OBJEC
    public class RightclickToolBar
    {
        public RightclickToolBar()
        {
            Menu1Print = false;
            Menu1Tabquick = false;
            Menu1btnBlackWhite = false;
            Menu1btnExportCSV = false;
            Menu1btnExportCSVAll = false;
            Menu1btnExportTXT = false;
            Menu1btnExportTXTAll = false;
            Menu2btnTransfer = false;
            Menu2btnTransfersTransferAll = false;
            Menu2delete = false;
            Menu2move = false;
            Menu2btnTracking = false;
            Menu2btnRequest = false;
            Favorive = false;
        }
        public bool Menu1Print { get; set; }
        public bool Menu1Tabquick { get; set; }
        public bool Menu1btnBlackWhite { get; set; }
        public bool Menu1btnExportCSV { get; set; }
        public bool Menu1btnExportCSVAll { get; set; }
        public bool Menu1btnExportTXT { get; set; }
        public bool Menu1btnExportTXTAll { get; set; }
        public bool Menu2btnTransfer { get; set; }
        public bool Menu2btnTransfersTransferAll { get; set; }
        public bool Menu2delete { get; set; }
        public bool Menu2move { get; set; }
        public bool Menu2btnTracking { get; set; }
        public bool Menu2btnRequest { get; set; }
        public bool Favorive { get; set; }
    }
    // SEARCH INTERNAL MODEL FOR QUERY SEARCH
    public class searchQueryModel
    {
        public string columnName { get; set; }
        public string ColumnType { get; set; }
        public string operators { get; set; }
        public string values { get; set; }
        public class Searchparams
        {
            public int ViewId { get; set; }
            public int pageNum { get; set; }
            public string ChildKeyField { get; set; }
            public string keyFieldValue { get; set; }
            public int firstCrumbChild { get; set; } = 0;
            public string columntype { get; set; }
            public string rowid { get; set; }
            public string preTableName { get; set; }
            public string Childid { get; set; }
            public string password { get; set; }
            public int ViewType { get; set; } = 0;
            public int crumbLevel { get; set; } = 0;
        }
        
    }

    public class SearchQueryRequestModal
    {
        public searchQueryModel.Searchparams paramss { get; set; }
        public searchQueryModel.Searchparams paramsUI { get; set; }
        public List<searchQueryModel> searchQuery { get; set; }
    }

    // TABLE HEADERS PROPERTIES BIND MODEL RETURN DOM OBJECT
    public class TableHeadersProperty
    {
        public TableHeadersProperty(string headername, string issort, string datatype, string isdropdown, string isEditable, int columnOrder, string editmask, bool allownull, string dataTypeFullName, string ColumnName, bool isprimarykey, int maxlength, bool iscounterField)
        {
            HeaderName = headername;
            Issort = Convert.ToBoolean(issort);
            DataType = datatype;
            isDropdown = Convert.ToBoolean(isdropdown);
            this.isEditable = Convert.ToBoolean(isEditable);
            this.columnOrder = columnOrder;
            editMask = editmask;
            Allownull = allownull;
            DataTypeFullName = dataTypeFullName;
            this.ColumnName = ColumnName;
            IsPrimarykey = isprimarykey;
            MaxLength = maxlength;
            isCounterField = iscounterField;
        }
        public string HeaderName { get; set; }
        public bool Issort { get; set; }
        public string DataType { get; set; }
        public bool isDropdown { get; set; }
        public bool isEditable { get; set; }
        public int columnOrder { get; set; }
        public string editMask { get; set; }
        public bool Allownull { get; set; }
        public string DataTypeFullName { get; set; }
        public string ColumnName { get; set; }
        public bool IsPrimarykey { get; set; }
        public int MaxLength { get; set; }
        public bool isCounterField { get; set; }
    }

    public class TableEditableHeader
    {
        public string HeaderName { get; set; }
        public bool Issort { get; set; }
        public string DataType { get; set; }
        public bool isDropdown { get; set; }
        public bool isEditable { get; set; }
        public int columnOrder { get; set; }
        public string editMask { get; set; }
        public bool Allownull { get; set; }
        public string DataTypeFullName { get; set; }
        public string ColumnName { get; set; }
        public bool IsPrimarykey { get; set; }
        public int MaxLength { get; set; }
        public bool isCounterField { get; set; }
        public string DefaultRetentionId { get; set; }
    }
    // TABLE COLUMNS RETURN DOM OBJECT
    public class TableColums
    {
        public string DataColumn { get; set; }
    }
    // DROPDOWN FUNCTIONS RETURN DATA AND DOM OBJECT
    public class DropDownproperties
    {
        public DropDownproperties(int colOrder, string value, string display)
        {
            colorder = colOrder;
            this.display = display;
            this.value = value;
        }
        public int colorder { get; set; }
        public string value { get; set; }
        public string display { get; set; }
    }

    public class SortableFileds
    {
        public string FieldName { get; set; }
        public int SortOrder { get; set; }
        public bool SortOrderDesc { get; set; }
    }

    public enum ViewType
    {
        FusionView,
        Favorite,
        GlobalSearch
    }
}
