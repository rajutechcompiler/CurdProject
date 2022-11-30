using System.Globalization;

static class DateAndTime
{
    public static string ToClientTime(this DateTime dt, HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.ContainsKey("timezoneoffset"))
        {
            string timeOffSet = ContextService.GetCookieVal("timezoneoffset", httpContext).ToString();
            // read the value from session
            if (timeOffSet is not null)
            {
                int offset = int.Parse(timeOffSet.ToString());
                dt = dt.AddMinutes(-1 * offset);
                return dt.ToShortDateString();
            }
        }
        // if there is no offset in session return the datetime in server timezone
        return dt.ToLocalTime().ToString();
    }
    public static DateTime ToClientTimeDate(this DateTime dt, HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.ContainsKey("timezoneoffset"))
        {
            string timeOffSet = ContextService.GetCookieVal("timezoneoffset", httpContext).ToString();
            // read the value from session
            if (timeOffSet is not null)
            {
                int offset = int.Parse(timeOffSet.ToString());
                dt = dt.AddMinutes(-1 * offset);

                return dt;
            }
        }
        // if there is no offset in session return the datetime in server timezone
        return dt.ToLocalTime();
    }

    public static bool IncludesTime(this DateTime dt)
    {
        var standartTime = new TimeSpan(0, 0, 0, 0, 0);
        int intval = dt.TimeOfDay.CompareTo(standartTime);
        return intval != 0;
    }

    public static string ToClientDateFormat(this DateTime dt)
    {
        return string.Format(CultureInfo.CurrentCulture, "{0:d}", dt);
    }

    public static string ToClientDateTimeFormat(this DateTime dt)
    {
        return string.Format(CultureInfo.CurrentCulture, "{0:g}", dt);
    }

    public static string ToUSDateFormat(this string s)
    {
        return string.Format(new CultureInfo("en-US"), "{0:d}", DateTime.Parse(s, CultureInfo.CurrentCulture));
    }

    public static bool CheckDateNull(this DateTime? dt)
    {
        if ((object)dt == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}


//public partial class Main : MasterPage
//{

//    private string _lastSearch;
//    public string _saveSearchSource;
//    public const int CONTENTBOTTOM = 40;
//    public const int CONTENTTOP = 95;

//    public WebVB.DialogBox DialogControl
//    {
//        get
//        {
//            return this.Dialog;
//        }
//        set
//        {
//            this.Dialog = value;
//        }
//    }

//    private Passport _passport
//    {
//        get
//        {
//            return (Passport)Session["Passport"];
//        }
//    }

//    private int _LId
//    {
//        get
//        {
//            return Conversions.ToInteger(ViewState["LId"]);
//        }
//        set
//        {
//            ViewState["LId"] = value;
//        }
//    }

//    private int _ALId
//    {
//        get
//        {
//            return Conversions.ToInteger(ViewState["ALId"]);
//        }
//        set
//        {
//            ViewState["ALId"] = value;
//        }
//    }

//    private int _FALId
//    {
//        get
//        {
//            return Conversions.ToInteger(ViewState["FALId"]);
//        }
//        set
//        {
//            ViewState["FALId"] = value;
//        }
//    }

//    private LevelManager _levelManager
//    {
//        get
//        {
//            return (LevelManager)Session["LevelManager"];
//        }
//    }

//    public Main()
//    {
//        Load += Page_Load;
//        PreRender += Page_PreRender;
//    }

//    private void Page_Load(object sender, EventArgs e)
//    {
//        this.chkAttachments.Text = Languages.get_Translation("chkMainAttachments");
//        this.chkAttachments.ToolTip = Languages.get_Translation("toolMainchkAttachments");
//        this.chkQuick.Text = Languages.get_Translation("chkMainQuick");
//        this.chkQuick.ToolTip = Languages.get_Translation("toolMainchkQuick");
//        this.chkNode.Text = Languages.get_Translation("chkMainNode");
//        this.chkNode.ToolTip = Languages.get_Translation("toolMainchkNode");
//        this.txtSearch.ToolTip = Languages.get_Translation("toolMaintxtSearch");
//        this.btnSearch.Attributes.Add("title", Languages.get_Translation("Search")); // Added by Hemin
//        this.reqSearchText.ErrorMessage = Languages.get_Translation("SearchRequiredValidatorMsg");
//        if (_passport is not null)
//        {
//            if (!IsPostBack)
//            {
//                _LId = Conversions.ToInteger(Conversions.ToInteger(_ALId == _FALId) == 0);
//                HandleAdminMenu();
//                BindUserAccessMenu();
//                DrawAdminMenu();
//                BackgroundStatusNotification();
//                if (WebVB.AzureADSettings.isADEnabled)
//                {
//                    this.hlSignout.NavigateUrl = "logout.aspx?ad=1";
//                }
//            }
//        }

//        if (Request.Browser.Browser == "IE")
//        {
//            int version;
//            try
//            {
//                version = Conversions.ToInteger(Request.Browser.Version);
//            }
//            catch (Exception ex)
//            {
//                version = Conversions.ToInteger(Request.Browser.Version.Replace(".", ","));
//            }
//            if (version < 8)
//            {
//                if (Request.UserAgent.Contains("Trident"))
//                {
//                    Response.AddHeader("X-UA-Compatible", "IE=8");
//                }
//                else
//                {
//                    Response.Redirect("IERequired.aspx");
//                }
//            }
//        }

//        // '''Commented by HK for resolving search issue after postback (01/04/2019)
//        // _saveSearchSource = "["

//        // If Session("savedSearches") Is Nothing Then
//        // If Request.Cookies("SavedSearch") IsNot Nothing AndAlso Request.Cookies("SavedSearch").Value <> String.Empty Then
//        // Dim savedSearches As New List(Of String)
//        // _saveSearchSource += Request.Cookies("SavedSearch").Value.Replace("%22", """").Replace("%2C", ",").Replace("%20", " ")
//        // For Each search In Request.Cookies("SavedSearch").Value.Replace("%22%2C%22", ";").Replace("\"",\""", ";").Replace("%22", "").Replace("%20", " ").Replace("\""", "").Split(";")
//        // savedSearches.Add(search)
//        // 'If _saveSearchSource <> "[" Then
//        // '    _saveSearchSource += ","   
//        // 'End If
//        // '_saveSearchSource += """" + search.Replace("*!*semicolon*!*", ";") + """"
//        // Next
//        // Session("savedSearches") = savedSearches
//        // End If
//        // Else
//        // Dim savedSearches = CType(Session("savedSearches"), List(Of String))
//        // For Each search In savedSearches
//        // If _saveSearchSource <> "[" Then _saveSearchSource += ","
//        // _saveSearchSource += String.Format("'{0}'", search)
//        // Next
//        // End If

//        // _saveSearchSource += "]"
//        // '''Commented by HK for resolving search issue after postback (01/04/2019)

//        if (this.tsm.IsInAsyncPostBack)
//            return;
//        DrawHeader();
//        // If Not IsPostBack And _passport IsNot Nothing Then DrawAdminMenu()
//        // DrawFooter() Removed
//        // If Not IsPostBack and Page.
//    }

//    public void BackgroundStatusNotification()
//    {
//        this.spanBackgroundStatusNotification.InnerHtml = "";
//        IRepository<SLServiceTask> s_BackgroundStatusNotification = new Repositories<SLServiceTask>();
//        int backgroundStatusNotification = s_BackgroundStatusNotification.All().Where(x => (_passport.UserId is var arg2 && x.UserId is { } arg1 ? arg1 == arg2 : (bool?)null) & (true is var arg4 && x.IsNotification is { } arg3 ? arg3 == arg4 : (bool?)null)).ToList().Count;

//        if (backgroundStatusNotification > 0)
//        {
//            this.spanBackgroundStatusNotification.InnerHtml = backgroundStatusNotification.ToString();
//            this.spanBackgroundStatusNotification.Style.Add("padding", "1px 3px");
//        }
//    }

//    private void BindUserAccessMenu()
//    {
//        IRepository<s_SavedCriteria> _is_SavedCriteria = new Repositories<s_SavedCriteria>();

//        var sbMenu = new StringBuilder();
//        this.divMenu.Style.Add("Visibility", "hidden");
//        this.divmenuloader.Visible = true;

//        string strLId;

//        // ''' Start : Bind My Queries and My Favourites Menu
//        var lWorkGroupItem = new List<WorkGroupItem>();
//        if (_passport.CheckPermission(WebVB.Common.SECURE_MYQUERY, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
//        {
//            lWorkGroupItem.Add(new WorkGroupItem()
//            {
//                ID = (long)WebVB.Enums.SavedType.Query,
//                WorkGroupName = Languages.get_Translation("MyQuery")
//            });
//        }

//        if (_passport.CheckPermission(WebVB.Common.SECURE_MYFAVORITE, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
//        {
//            lWorkGroupItem.Add(new WorkGroupItem()
//            {
//                ID = (long)WebVB.Enums.SavedType.Favorite,
//                WorkGroupName = Languages.get_Translation("MyFavourite")
//            });
//        }

//        foreach (WorkGroupItem workGroupItem in lWorkGroupItem)
//        {
//            strLId = string.Format("L{0}", _LId);
//            sbMenu.Append("<ul class='drillDownMenu MyQuery_Fav'><li>");
//            sbMenu.Append(string.Format("<a href='#' id='RA{0}'><i class='font_icon theme_color fa fa-database'></i>{1}</a>", strLId, workGroupItem.WorkGroupName));

//            var subMenu = _is_SavedCriteria.All().Where(x => (_passport.UserId is var arg6 && x.UserId is { } arg5 ? arg5 == arg6 : (bool?)null) is var arg7 && arg7.HasValue && !arg7.Value ? (bool?)false : (long)x.SavedType == workGroupItem.ID ? arg7 : (bool?)false).ToList();

//            if (subMenu.Count > 0)
//            {
//                _LId += 1;
//                strLId = string.Format("{0}{1}", strLId, _LId);
//                sbMenu.Append("<ul>");
//                this.BindSavedSubMenu(subMenu, ref sbMenu, strLId);
//                sbMenu.Append("</ul>");
//            }
//            else
//            {
//                sbMenu.Append("<ul>");
//                sbMenu.Append("</ul>");
//            }
//            sbMenu.Append("</li></ul>");
//            _LId += 1;
//        }
//        // ''' End : Bind My Queries and My Favourites Menu

//        foreach (WorkGroupItem workGroupItem in GetWorkGroups(_passport))
//        {
//            strLId = string.Format("L{0}", _LId);
//            sbMenu.Append("<ul class='drillDownMenu'><li>");
//            sbMenu.Append(string.Format("<a href='#' id='RA{0}'><i class='font_icon theme_color fa fa-database'></i>{1}</a>", strLId, workGroupItem.WorkGroupName));

//            var subMenu = GetWorkGroupMenu((short)workGroupItem.ID, _passport);

//            if (subMenu.Count > 0)
//            {
//                _LId += 1;
//                strLId = string.Format("{0}{1}", strLId, _LId);
//                sbMenu.Append("<ul>");
//                BindUserSubMenu(subMenu, ref sbMenu, strLId);
//                sbMenu.Append("</ul>");
//            }
//            sbMenu.Append("</li></ul>");
//            _LId += 1;
//        }

//        if (CheckPermssionsReportTab())
//        {
//            _LId += 1;
//            sbMenu.Append("<ul class='drillDownMenu'><li>");
//            sbMenu.Append(string.Format("<a href='#' id='RAL{0}'><i class='font_icon theme_color fa fa-database'></i>{1}</a>", _LId, Languages.get_Translation("Reports")));
//            sbMenu.Append("<ul>");
//            LoadCustomReportsMenu(ref sbMenu, "L" + _LId.ToString());
//            sbMenu.Append("</ul>");
//            sbMenu.Append("</li></ul>");
//        }

//        if (_passport.CheckPermission(WebVB.Common.SECURE_RETENTION_SETUP, Smead.Security.SecureObject.SecureObjectType.Retention, Permissions.Permission.Access))
//        {
//            _LId += 1;
//            sbMenu.Append("<ul class='drillDownMenu'><li>");
//            sbMenu.Append(string.Format("<a href='/Retention/RetentionCodeMaintenance' id='FAL{0}'><i class='font_icon theme_color fa fa-database'></i>{1}</a>", _LId, Languages.get_Translation("Retention")));
//            sbMenu.Append("</li></ul>");
//        }

//        this.divMenu.InnerHtml = sbMenu.ToString();
//    }

//    private void LoadCustomReportsMenu(ref StringBuilder sbMenu, string parentIdNo)
//    {
//        _ALId += 1;
//        sbMenu.Append(string.Format("<li><a href='#' id='A{0}'>{1}</a>", parentIdNo + _ALId.ToString(), Languages.get_Translation("lblReportsLandingCustomReports")));
//        var customReports = GetViewReports(_passport);
//        sbMenu.Append("<ul>");

//        foreach (var report in customReports)
//        {
//            if (_passport.CheckPermission(report.ViewName, Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
//            {
//                sbMenu.Append(string.Format("<li><a onClick=\"LoadQueryWindow(this,'{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), report.ViewName, report.Id));
//            }
//            _FALId += 1;
//        }

//        sbMenu.Append("</ul></li>");
//        if (_passport.CheckPermission(" Auditing", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
//        {
//            _ALId += 1;
//            sbMenu.Append(string.Format("<li><a onClick=\"LoadQueryWindow(this,'','AuditReport.aspx')\" id='A{0}'>{1}</a>", parentIdNo + _ALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlAuditReport")));
//        }
//        if (_passport.CheckPermission(" Tracking", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
//        {
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a href='#' id='AL{0}'>{1}</a>", parentIdNo + _FALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlTraRepo")));
//            sbMenu.Append("<ul>");

//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a id='FA{0}' onClick=\"LoadQueryWindow(this,'','{2}')\">{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlPastDue"), ReportURL("PastDueTrackableItemsReport")));
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a id='FA{0}' onClick=\"LoadQueryWindow(this,'','{2}')\">{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlObjOut"), ReportURL("CurrentItemsOutReport")));
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a href='#' id='AL{0}' onClick=\"LoadQueryWindow(this,'')\">{1}</a>", parentIdNo + _FALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlObjInven")));
//            sbMenu.Append("<ul>");

//            foreach (DataRow row in GetTrackableTables(_passport).Rows)
//            {
//                _FALId += 1;
//                sbMenu.Append(string.Format("<li><a id='FA{0}' onClick=\"LoadQueryWindow(this,'','{2}')\">{1}</a></li>", parentIdNo + _FALId.ToString(), row["UserName"].ToString(), ReportURL("ObjectsOutReport", row["TableName"].ToString())));
//            }

//            sbMenu.Append("</ul></li>");
//            sbMenu.Append("</ul></li>");
//        }

//        if (_passport.CheckPermission(" Requestor", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
//        {
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a href='#' id='AL{0}'>{1}</a>", parentIdNo + _FALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlReqRepo")));
//            sbMenu.Append("<ul>");
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a  onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlNewReq"), ReportURL("NewRequestsReport")));
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a  onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlNewBatch"), ReportURL("NewBatchesReport")));
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a  onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlPullLists"), ReportURL("PullListsReport")));
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a  onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlExcep"), ReportURL("RequestExceptionsReport")));
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a  onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlInPro"), ReportURL("RequestsInProcessReport")));
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a  onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("mnuWorkGroupMenuControlWaitList"), ReportURL("WaitListReport")));

//            sbMenu.Append("</ul></li>");
//        }

//        if (_passport.CheckPermission(" Retention", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View) & _passport.CheckLicense(Smead.Security.SecureObject.SecureObjectType.Retention))
//        {
//            bool turnOffCitation = Conversions.ToBoolean(GetSystemSetting("RetentionTurnOffCitations", _passport));
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a href='#' id='AL{0}'>{1}</a>", parentIdNo + _FALId.ToString(), Languages.get_Translation("RetentionReports")));
//            sbMenu.Append("<ul>");

//            // 'Adding permission
//            if (_passport.CheckPermission("Disposition", Smead.Security.SecureObject.SecureObjectType.Retention, Permissions.Permission.Access))
//            {
//                _FALId += 1;
//                sbMenu.Append(string.Format("<li><a onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("FinalDispositionReport"), ReportURL("FinalDispositionReport")));
//            }

//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("CertificateDispositionReport"), ReportURL("CertificateDispositionReport")));
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("InactivePullListsReport"), ReportURL("InactivePullListsReport")));
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("InactiveReport"), ReportURL("InactiveReport")));
//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("OnHoldReport"), ReportURL("OnHoldReport")));

//            if (!turnOffCitation)
//            {
//                _FALId += 1;
//                sbMenu.Append(string.Format("<li><a onClick=\"LoadQueryWindow(this,'','{2}')\"  id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("CitationReport"), ReportURL("CitationReport")));
//                _FALId += 1;
//                sbMenu.Append(string.Format("<li><a onClick=\"LoadQueryWindow(this,'','{2}')\"  id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("CitationRetionCodesReport"), ReportURL("CitationRetionCodesReport")));
//            }

//            _FALId += 1;
//            sbMenu.Append(string.Format("<li><a onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("RetentionCodesReport"), ReportURL("RetentionCodesReport")));

//            if (!turnOffCitation)
//            {
//                _FALId += 1;
//                sbMenu.Append(string.Format("<li><a onClick=\"LoadQueryWindow(this,'','{2}')\" id='FA{0}'>{1}</a></li>", parentIdNo + _FALId.ToString(), Languages.get_Translation("RetentionCodesCitationReport"), ReportURL("RetentionCodesCitationReport")));
//            }

//            sbMenu.Append("</ul></li>");

//        }

//    }

//    private string ReportURL(object queryString, object secondQueryString = null)
//    {
//        if (secondQueryString is not null)
//        {
//            return Conversions.ToString(Operators.AddObject(Operators.AddObject(Operators.AddObject("htmlreports.aspx?r=", queryString), "&t="), secondQueryString));
//        }
//        else
//        {
//            return Conversions.ToString(Operators.AddObject("htmlreports.aspx?r=", queryString));
//        }
//    }

//    private bool CheckPermssionsReportTab()
//    {
//        if (_passport.CheckPermission(" Auditing", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
//            return true;
//        if (_passport.CheckPermission(" Requestor", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
//            return true;
//        if (_passport.CheckPermission(" Retention", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
//            return true;
//        if (_passport.CheckPermission(" Tracking", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.View))
//            return true;
//        return false;
//    }

//    private void BindUserSubMenu(List<TableItem> subMenu, ref StringBuilder sbMenu, string parentIdNo)
//    {
//        foreach (var table in subMenu)
//        {
//            var lstViewItems = GetViewsByTableName(table.TableName, _passport);
//            if (lstViewItems.Count > 0)
//            {
//                sbMenu.Append(string.Format("<li><a href='#' id='A{0}{1}'>{2}</a>", parentIdNo, _ALId, table.UserName));
//                sbMenu.Append("<ul>");

//                foreach (var view in lstViewItems)
//                {
//                    sbMenu.Append(string.Format("<li><a id=\"FA{2}{3}{4}\" onClick=\"LoadQueryWindow(this,'{0}')\">{1}</a></li>", view.Id, view.ViewName, parentIdNo, _ALId.ToString(), _FALId.ToString()));
//                    _FALId += 1;
//                }
//                sbMenu.Append("</ul></li>");
//            }
//            _ALId += 1;
//        }
//    }

//    protected void btnitem_Click(object sender, EventArgs e)
//    {
//        Level level;
//        int viewID = Convert.ToInt32(this.hdnviewId.Value);
//        this.hdnviewId.Value = "0";

//        if (Conversions.ToBoolean(GetViewInfo(viewID, _passport)["Printable"]))
//        {
//            Response.Redirect("HTMLReports.aspx?r=Custom&viewid=" + viewID, true);
//        }

//        Session["autoStartedQuery"] = _levelManager.Clone();
//        string oldTable = "";
//        if (_levelManager.ActiveLevel is not null && _levelManager.ActiveLevel.Parameters is not null)
//        {
//            oldTable = _levelManager.ActiveLevel.Parameters.TableName;
//        }
//        _levelManager.Levels.Clear();
//        Session["ExportAll"] = false;
//        Session["ExportAllSQL"] = string.Empty;

//        level = new Level(viewID, _passport);
//        level.Parameters.QueryType = queryTypeEnum.OpenTable;
//        level.Parameters.Scope = ScopeEnum.Table;

//        // 'Changed by Hasmukh on 01/18/2017 for date format changes
//        // level.Parameters.Culture = New CultureInfo(Request.UserLanguages(0))
//        level.Parameters.Culture = WebVB.Keys.GetCultureCookies;
//        level.Parameters.ParentField = string.Empty;
//        level.Parameters.ParentValue = string.Empty;

//        // ''' Start : Hasmukh : Favourite Functionality click filter for view 
//        if (Request.Cookies["SavedCriteriaType"] is not null)
//        {
//            if (!string.IsNullOrEmpty(Request.Cookies["SavedCriteriaType"].Value.ToString()))
//            {
//                var SavedCriteriaSplitValues = Request.Cookies["SavedCriteriaType"].Value.ToString().Trim().Split('_');
//                if (SavedCriteriaSplitValues[1].ToString().Trim() == "1")
//                {
//                    string whereinquery = "select [TableId] from s_SavedChildrenFavorite where SavedCriteriaId=" + SavedCriteriaSplitValues[0].Trim().ToString();
//                    string strWhereQuery = string.Format(" {0} in ({1}) ", level.Parameters.KeyField, whereinquery);
//                    level.Parameters.WhereClause = strWhereQuery; // " where Id In (1,2,3)  "
//                }
//            }
//        }
//        // ''' End : Hasmukh : Favourite Functionality click filter for view

//        _levelManager.CurrentLevel = 0;
//        _levelManager.Levels.Add(level);

//        if (Page.GetType().Name == "data_aspx")
//            ((WebVB.Data)Page).Editor.setRecord(_levelManager.ActiveLevel.CursorValue);
//        bool startQuery = true;
//        if (Request.Cookies["startQuery"] is not null)
//            startQuery = Conversions.ToBoolean(Request.Cookies["startQuery"].Value);
//        // hdnBreadCrumbsClick.Value = 1
//        if (startQuery & this.Dialog is not null && this.hdnBreadCrumbsClick.Value.Trim().Equals("0"))
//        {
//            if ((_levelManager.ActiveLevel.Parameters.TableName ?? "") != (oldTable ?? ""))
//            {
//                Session["doClear"] = true;
//            }
//            this.pnlSearch.DefaultButton = "";
//            Session["menu"] = this.divMenu.InnerHtml;
//            this.Dialog.Visible = true;
//            this.Dialog.tabControl = WebVB.DialogBox.tabControls.Query;

//            this.Dialog.LoadControls();
//        }
//        else
//        {
//            Session["autoStartedQuery"] = null;
//            this.pnlSearch.DefaultButton = "btnDefaultSearch";
//            switch (Page.GetType().Name ?? "")
//            {
//                case "data_aspx":
//                    {
//                        ((WebVB.Data)Page).LoadRootDataScreen(newView: true);
//                        break;
//                    }
//                case "htmlreports_aspx":
//                    {
//                        Response.Redirect("data.aspx");
//                        break;
//                    }
//            }
//        }
//    }

//    private void ExecuteSearch()
//    {
//        if (Conversions.ToInteger(Conversions.ToBoolean(this.txtSearch.Text.Length)) == 0)
//            return;

//        var qry = new Query(_passport);
//        var @params = new Parameters(_passport);
//        var savedSearches = new List<string>();
//        if (Session["savedSearches"] is not null)
//            savedSearches = (List<string>)Session["savedSearches"];
//        string alreadySaved = savedSearches.Find(value => (this.txtSearch.Text.ToLower() ?? "") == (value.ToLower() ?? ""));

//        if (alreadySaved is null)
//        {
//            savedSearches.Add(this.txtSearch.Text);
//            if (savedSearches.Count > 10)
//                savedSearches.RemoveAt(0);
//            // '''Commented out because of fix bug FUS-5886
//            // SaveSearches(savedSearches)
//            Session["savedSearches"] = savedSearches;
//        }

//        if (this.chkNode.Checked)
//        {
//            @params.Scope = ScopeEnum.Node;

//            if (_levelManager.ActiveLevel.Parameters.TableName is not null && !string.IsNullOrEmpty(_levelManager.ActiveLevel.Parameters.TableName))
//            {
//                @params.ChangeViewID(_levelManager.ActiveLevel.Parameters.ViewId);
//            }
//            @params.CursorValue = _levelManager.ActiveLevel.CursorValue;
//        }
//        // params. = _levelManager.ActiveLevel.CursorValue
//        else if (this.chkQuick.Checked)
//        {
//            @params.Scope = ScopeEnum.Table;
//            if (_levelManager.ActiveLevel.Parameters.TableName is not null && !string.IsNullOrEmpty(_levelManager.ActiveLevel.Parameters.TableName))
//            {
//                @params.ChangeViewID(GetTableFirstSearchableViewId(_levelManager.ActiveLevel.Parameters.TableName, _passport));
//            }
//        }
//        else
//        {
//            @params.Scope = ScopeEnum.Database;
//        }

//        @params.Text = this.txtSearch.Text;
//        // added cookie for document viewer OCR when user link attachment from search.

//        // Dim searchCookie As HttpCookie = New HttpCookie("searchInput", params.Text)
//        // searchCookie.HttpOnly = False
//        // Response.Cookies.Add(searchCookie)
//        @params.IncludeAttachments = this.chkAttachments.Checked;
//        @params.QueryType = queryTypeEnum.Text;
//        // params.RequestedRows = 500
//        IRepository<Setting> _iSettings = new Repositories<Setting>();
//        int globalSearchMaxVal = 25;
//        string sMaxVal = _iSettings.All().Where(x => x.Section == "GlobalSearch" & x.Item == "MaxRecordsFetch").Select(y => y.ItemValue).FirstOrDefault();
//        if (!string.IsNullOrEmpty(sMaxVal))
//        {
//            globalSearchMaxVal = Convert.ToInt32(sMaxVal);
//        }
//        @params.RequestedRows = globalSearchMaxVal;

//        // If _levelManager Is Nothing Then _levelManager = CType(Session("LevelManager"), LevelManager)
//        _levelManager.DatabaseSearchParams = @params;
//        // check if view searchable

//        qry.Search(@params);
//        // Response.Redirect("SearchResults.aspx", False)
//        this.Dialog.Visible = true;
//        this.Dialog.tabControl = WebVB.DialogBox.tabControls.Search;
//        this.Dialog.LoadControls();
//        // End If
//    }

//    private void SaveSearches(List<string> searches)
//    {
//        string save = "";
//        var newCookie = Request.Cookies["SavedSearch"];

//        foreach (var search in searches)
//        {
//            if (!string.IsNullOrEmpty(save))
//                save += ",";
//            save += @"\""" + search + @"\""";
//        }

//        // ToolkitScriptManager.RegisterClientScriptBlock(Page, Page.GetType, "SaveSearch", "updateSource([" + save.Replace("\""", """") + "]);setCookie('SavedSearch','" + save.Replace("'", "\'") + "',180);", True)
//        Response.Cookies["SavedSearch"].Value = save;
//        Response.Cookies["SavedSearch"].Expires = DateTime.Now.AddDays(180d);
//    }

//    private void DrawHeader()
//    {
//        this.chkAttachments.Visible = false;
//        this.chkQuick.Visible = false;
//        this.chkNode.Visible = false;

//        if (Conversions.ToBoolean(Session["sharepoint"]))
//        {
//            this.divSearch.Visible = false;
//            this.AdminMenu.Visible = false;
//            // divContent.Style.Add("top", "0px")
//            this.divSearchContent.Visible = false;
//            this.divSearch.Visible = true;
//        }
//        else
//        {
//            this.divSearch.Visible = true;
//            this.AdminMenu.Visible = true;
//            // divContent.Style.Add("top", CONTENTTOP.ToString + "px")
//            this.divSearchContent.Visible = true;
//        }
//        if (Conversions.ToBoolean(!Session["sharepoint"]))
//        {
//            if (Request.ServerVariables["URL"].ToLower().EndsWith("/signin.aspx") | Request.ServerVariables["URL"].ToLower().EndsWith("/signinmessage.aspx") | CBoolean(Session["mustChangePassword"]))
//            {
//                this.txtSearch.Style.Add("padding-top", "14px");
//                this.txtSearch.Visible = false;
//                // datasearch.Visible = False 'Not Required
//                this.AdminMenu.Visible = false;
//                this.btnSearch.Visible = false;
//                this.divSearch.Visible = false;
//                if (CBoolean(Session["mustChangePassword"]) == true)
//                {
//                    this.ShowOnlyLogo.Visible = true;
//                }
//                else
//                {
//                    this.ShowOnlyLogo.Visible = false;
//                }
//            }
//            else
//            {
//                this.btnSearch.Visible = true;
//                this.txtSearch.Visible = true;
//                // datasearch.Visible = True  'Not Required
//                Page.Title = Languages.get_Translation("pgTiTABWebAccess");
//                if (_passport is not null)
//                    this.chkAttachments.Visible = _passport.CheckLicense(Smead.Security.SecureObject.SecureObjectType.Attachments);
//                this.AdminMenu.Visible = true;
//                this.ShowOnlyLogo.Visible = false;
//            }
//        }


//    }

//    private void DrawAdminMenu()
//    {
//        // 'Commented by Hasmukh to Fix : FUS-4569
//        // lbTools.Visible = Not _passport.UsingActiveDirectory

//        // hlTracking.Visible = _passport.CheckPermission("Tracking", Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access)

//        string pageHelp = Strings.Replace(Request.ServerVariables["URL"], "/", "");
//        pageHelp = Strings.Replace(pageHelp, ".aspx", "");
//        if (pageHelp == "SearchResults")
//            pageHelp = "search";

//        if (System.IO.File.Exists(Server.MapPath(@"help\" + pageHelp + ".htm")))
//        {
//            this.hlHelp.NavigateUrl = "help/Default.htm"; // "help/index.html"
//        }
//        else
//        {
//            this.hlHelp.NavigateUrl = "help/Default.htm";
//        } // "help/index.html"

//        this.hlHelp.Target = "Help";
//    }

//    private void HandleAdminMenu()
//    {
//        MySession.Current.sConnectionString = WebVB.Keys.get_GetDBConnectionString();

//        IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
//        IRepository<ImportJob> _iImportJob = new Repositories<ImportJob>();
//        IRepository<ImportField> _iImportField = new Repositories<ImportField>();
//        AdminSecurityCheck();
//        // Check for LABEL MANAGER access
//        if (_passport.CheckPermission(WebVB.Common.SECURE_LABEL_SETUP, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
//        {
//            this.hlLabelManager.Visible = true;
//            // Keys.bLabelPermission = True
//            Session["bLabelPermission"] = true;
//        }
//        else
//        {
//            this.hlLabelManager.Visible = false;
//            // Keys.bLabelPermission = False
//            Session["bLabelPermission"] = false;
//        }

//        if (_passport.CheckPermission(WebVB.Common.SECURE_DASHBOARD, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
//        {
//            this.hlLabelDashboard.Visible = true;
//        }
//        else
//        {
//            this.hlLabelDashboard.Visible = false;
//        }

//        // Check for SCANNER access
//        // Commented out for now until scanner is ready
//        // If _passport.CheckPermission(Common.SECURE_SCANNER, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access) Then
//        // hlScanner.Visible = False
//        // 'Keys.bScannerPermission = True
//        // Session("bScannerPermission") = True
//        // Else
//        this.hlScanner.Visible = false;
//        // Keys.bScannerPermission = False
//        Session["bScannerPermission"] = false;
//        // End If
//        List<KeyValuePair<string, bool>> lstImportLoad = null;
//        lstImportLoad = WebVB.Keys.GetImportDDL(_iImportLoad.All(), _iImportField.All());
//        // Fixed - FUS-5818 by Nikunj
//        bool ImportPermission = _passport.CheckPermission(WebVB.Common.SECURE_IMPORT_SETUP, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access);
//        if (ImportPermission)
//        {
//            this.hlImport.Visible = true;
//        }
//        else if (lstImportLoad.Count == 0)
//        {
//            this.hlImport.Visible = false;
//        }
//        else
//        {
//            this.hlImport.Visible = true;
//        }
//        // Check for IMPORT access - Commented by Ganesh for Security fix
//        // If _passport.CheckPermission(Common.SECURE_IMPORT_SETUP, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access) Then
//        // hlImport.Visible = True
//        // Else
//        // hlImport.Visible = False
//        // End If
//        // Check for TRACKING access
//        if (_passport.CheckPermission(WebVB.Common.SECURE_TRACKING, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
//        {
//            this.hlTracking.Visible = true;
//        }
//        else
//        {
//            this.hlTracking.Visible = false;
//        }
//        if (_passport.CheckPermission(WebVB.Common.SECURE_RETENTION_SETUP, Smead.Security.SecureObject.SecureObjectType.Retention, Permissions.Permission.Access))
//        {
//            // Keys.bRetentionPermission = True
//            Session["bRetentionPermission"] = true;
//        }
//        else
//        {
//            // Keys.bRetentionPermission = False
//            Session["bRetentionPermission"] = false;
//        }
//    }

//    private void AdminSecurityCheck()
//    {
//        IRepository<Models.Table> _iTable = new Repositories<Models.Table>();
//        IRepository<Models.View> _iView = new Repositories<Models.View>();
//        var BaseWebPage = new WebVB.BaseWebPage();
//        int iCntRpts = WebVB.CollectionsClass.CheckReportsPermission(_iTable.All(), _iView.All(), BaseWebPage);
//        bool mbMgrGroup = _passport.CheckAdminPermission(Permissions.Permission.Access);
//        bool bAtLeastOneTablePermission = WebVB.CollectionsClass.CheckTablesPermission(_iTable.All(), mbMgrGroup, BaseWebPage);
//        bool bAtLeastOneViewPermission = WebVB.CollectionsClass.CheckViewsPermission(_iView.All(), mbMgrGroup, BaseWebPage);
//        bool testSECURE_SECURITY = BaseWebPage.Passport.CheckPermission(WebVB.Common.SECURE_SECURITY, (Smead.Security.SecureObject.SecureObjectType)WebVB.Enums.SecureObjects.Application, (Permissions.Permission)WebVB.Enums.PassportPermissions.Access);
//        bool testSECURE_SECURITY_USER = BaseWebPage.Passport.CheckPermission(WebVB.Common.SECURE_SECURITY_USER, (Smead.Security.SecureObject.SecureObjectType)WebVB.Enums.SecureObjects.Application, (Permissions.Permission)WebVB.Enums.PassportPermissions.Access);
//        bool testSECURE_REPORT_STYLES = BaseWebPage.Passport.CheckPermission(WebVB.Common.SECURE_REPORT_STYLES, (Smead.Security.SecureObject.SecureObjectType)WebVB.Enums.SecureObjects.Application, (Permissions.Permission)WebVB.Enums.PassportPermissions.Access);

//        if (!mbMgrGroup & !_passport.CheckPermission(WebVB.Common.SECURE_SECURITY, (Smead.Security.SecureObject.SecureObjectType)WebVB.Enums.SecureObjects.Application, (Permissions.Permission)WebVB.Enums.PassportPermissions.Access) & !_passport.CheckPermission(WebVB.Common.SECURE_SECURITY_USER, (Smead.Security.SecureObject.SecureObjectType)WebVB.Enums.SecureObjects.Application, (Permissions.Permission)WebVB.Enums.PassportPermissions.Access) & !_passport.CheckPermission(WebVB.Common.SECURE_REPORT_STYLES, (Smead.Security.SecureObject.SecureObjectType)WebVB.Enums.SecureObjects.Application, (Permissions.Permission)WebVB.Enums.PassportPermissions.Access) & !(iCntRpts != 0) & !bAtLeastOneTablePermission & !bAtLeastOneViewPermission)
//        {
//            this.hlAdmin.Visible = false;
//        }
//        else
//        {
//            this.hlAdmin.Visible = true;
//        }
//    }

//    // Private Sub DrawFooter()
//    // Dim td As New TableCell

//    // If Session("sharepoint") Then
//    // divFooter.Visible = False
//    // divContent.Style.Add("bottom", "0px")
//    // Else
//    // divFooter.Visible = True
//    // divContent.Style.Add("bottom", CONTENTBOTTOM.ToString + "px")
//    // End If

//    // If Request.ServerVariables("URL").ToLower.EndsWith("/signin.aspx") Or Session("sharepoint") Or CBoolean(Session("mustChangePassword")) Then
//    // tblFooter.Visible = False
//    // Else
//    // tblFooter.Visible = True

//    // td = New TableCell
//    // td.Text = "<img style='padding-left: 20px;' src='images/server.png' />"
//    // tblFooter.Rows(0).Cells.Add(td)

//    // td = New TableCell
//    // td.Text = "<div style='color: pink;'>" & Languages.Translation("lblMainSERVER") & "</div> " & Session("Server").ToString
//    // tblFooter.Rows(0).Cells.Add(td)

//    // td = New TableCell
//    // td.Text = "<img style='padding-left: 20px;' src='images/database v4.png' />"
//    // tblFooter.Rows(0).Cells.Add(td)


//    // td = New TableCell
//    // td.Text = "<div style='color: pink;'>" & Languages.Translation("lblMainDATABASE") & "</div> " & Session("DatabaseName").ToString
//    // tblFooter.Rows(0).Cells.Add(td)

//    // td = New TableCell
//    // td.Text = "<img style='padding-left: 20px;' src='images/user login sh.png' />"
//    // tblFooter.Rows(0).Cells.Add(td)

//    // td = New TableCell
//    // td.Text = "<div style='color: pink;'>" & Languages.Translation("lblMainUser") & "</div>" & Session("UserName").ToString
//    // tblFooter.Rows(0).Cells.Add(td)


//    // If _passport IsNot Nothing AndAlso _passport.CheckPermission("Tracking", Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access) Then
//    // td = New TableCell
//    // Dim tl As TaskLights = CType(LoadControl("TaskLights.ascx"), TaskLights)
//    // td.Controls.Add(tl)
//    // tblFooter.Rows(0).Cells.Add(td)
//    // tl.GetTaskLightValues()
//    // tl.Visible = tl.Allowed
//    // End If

//    // lblCopyRight.ForeColor = Drawing.Color.White
//    // lblCopyRight.Text = String.Format(Languages.Translation("Copyright"), Date.Today.ToUniversalTime.ToClientTimeDate().Year.ToString, My.Application.Info.Version.ToString)
//    // End If
//    // End Sub

//    private void Page_PreRender(object sender, EventArgs e)
//    {
//        if (Session["LevelManager"] is null)
//            return;

//        if ((_levelManager is not null && _levelManager.ActiveLevel is not null && _levelManager.ActiveLevel.Parameters is not null) & !(Page.ToString() == "ASP.signin_aspx"))
//        {
//            this.chkQuick.Visible = _levelManager.ActiveLevel.Parameters.ViewId > 0;
//            this.chkNode.Visible = _levelManager.ActiveLevel.Parameters.ViewId > 0;
//        }

//        if (!(Page.ToString() == "ASP.searchresults_aspx"))
//            return;
//        if (_levelManager.DatabaseSearchParams.Scope == ScopeEnum.Database)
//            this.txtSearch.Text = _levelManager.DatabaseSearchParams.Text;
//        this.chkAttachments.Checked = _levelManager.DatabaseSearchParams.IncludeAttachments;
//    }

//    public HtmlGenericControl BodyTag
//    {
//        get
//        {
//            return this.MasterBody;
//        }
//        set
//        {
//            this.MasterBody = value;
//        }
//    }

//    // Private Sub btnsearch_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSearch.Click
//    // ExecuteSearch()
//    // End Sub

//    protected void btnSearch_Click(object sender, EventArgs e)
//    {
//        ExecuteSearch();
//    }

//    private void lbTools_Click(object sender, EventArgs e)
//    {
//        this.Dialog.Visible = true;
//        this.Dialog.tabControl = WebVB.DialogBox.tabControls.Tools;
//        this.Dialog.LoadControls();
//    }

//    // Private Sub lbAboutUs_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lbAboutUs.Click
//    // 'Dialog.Visible = True
//    // 'Dialog.tabControl = DialogBox.tabControls.AboutUs
//    // 'Dialog.LoadControls()
//    // End Sub

//    public void BindSavedSubMenu(List<s_SavedCriteria> subMenu, ref StringBuilder sbMenu, string strLId)
//    {
//        if (subMenu.Count > 0)
//        {
//            IRepository<Models.View> s_Views = new Repositories<Models.View>();

//            foreach (var view in subMenu)
//            {
//                var objView = s_Views.Where(x => x.Id is var arg9 && view.ViewId is { } arg10 ? arg9 == arg10 : (bool?)null);
//                if (objView is not null)
//                {
//                    if (_passport.CheckPermission(objView.FirstOrDefault().ViewName, Smead.Security.SecureObject.SecureObjectType.View, Permissions.Permission.View))
//                    {
//                        // sbMenu.Append(String.Format("<li><a id=""FA{2}{3}{4}"" onClick=""LoadQueryWindow(this,'{0}')"">{1}</a></li>", view.ViewId, view.SavedName, parentIdNo, _ALId.ToString, _FALId.ToString))
//                        // sbMenu.Append(String.Format("<li><a id=""A{2}{3}"" onClick=""LoadQueryWindow(this,'{0}_{4}')"">{1}</a></li>", view.ID, view.UserName, strLId, _ALId.ToString, view.TableName))
//                        sbMenu.Append(string.Format("<li><a id=\"A{2}{3}\" onClick=\"LoadQueryWindow(this,'{0}_{4}_{5}')\">{1}</a></li>", view.ViewId, view.SavedName, strLId, _ALId.ToString(), view.Id, view.SavedType));
//                        // _FALId += 1
//                        _ALId += 1;
//                    }
//                }
//                else
//                {
//                    var SavedCriteriaSplitValues = Request.Cookies["SavedCriteriaType"].Value.ToString().Trim().Split('_');
//                    WebVB.SavedCriteria.DeleteSavedCriteria(view.Id, SavedCriteriaSplitValues[1]);
//                }
//            }
//        }
//        _ALId += 1;
//    }
//}