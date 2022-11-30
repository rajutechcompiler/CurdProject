using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Smead.Security;
using TabFusionRMS.DataBaseManagerVB;
using TabFusionRMS.Models;
using TabFusionRMS.Resource;
using slimShared;
using TabFusionRMS.WebCS;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Http.Features;
using System.Configuration;

public sealed class Keys
{
    public static bool IsWarnning = false;
    public static bool IsError = false;
    public static bool IsExist = false;
    public static string ErrorType = "s";
    public static string iAdminRefId = string.Empty;
    public static string iImportRefId = string.Empty;
    public static string iScannerRefId = string.Empty;
    public static string iLabelRefId = string.Empty;
    public static string iRetentionRefId = string.Empty;
    public static string CookieName = "beforeLogin";

    public static bool bImportPermission = false;
    public static bool bScannerPermission = false;
    public static bool bLabelPermission = false;
    public static bool bRetentionPermission = false;

    public static string ErrorMessage = SaveSuccessMessage();
    public const string SECURE_REPORT_STYLES = "Report Styles";
    public const string IMPORT_TRACK = "<<TRACKING IMPORT>>";
    public const string SECURE_TRACKING = "Tracking";
    public const int TRACKING_LOCATION = 1;
    public const int TRACKING_EMPLOYEE = 2;
    


    public static string AlreadyExistMessage(string pRecordValue)
    {
        return string.Format(Languages.get_Translation("msgAdminCtrlTheRecordFor"), pRecordValue.ToUpper());
    }
    public static string AlreadyExistMessageAddBarCode(string pRecordValue)
    {
        return string.Format(Languages.get_Translation("msgAdminCtrlTheRecordForAddBarCode"), pRecordValue.ToUpper());
    }
    public static string SaveSuccessMessage()
    {
        return Languages.get_Translation("msgJsRequestorRecSaveSuccessfully");
    }

    public static string EditRetentionSaveSuccessMessage()
    {
        return Languages.get_Translation("msgJsRetentionEditSaveSuccessfully");
    }
    public static string RetentionSaveSuccessMessage()
    {
        return Languages.get_Translation("msgJsRetentionRecSaveSuccessfully");
    }
    public static string SaveWorkSuccessMessage()
    {
        return Languages.get_Translation("msgJsRequestorWorkgroupSaveSuccessfully");
    }
    public static string ChangeOrderSuccessMessage()
    {
        return Languages.get_Translation("msgJsRequestorRecChangeOrderSuccessfully");
    }

    public static string ErrorMessageJS()
    {
        return Languages.get_Translation("msgAdminCtrlErrorOccPlzContactAdmin");
    }
    public static string DeleteSuccessMessage()
    {
        return Languages.get_Translation("msgAdminCtrlRecDeletedSuccessfully");
    }
    public static string DeleteReportSuccessMessage()
    {
        return Languages.get_Translation("msgAdminCtrlReportDeletedSuccessfully");
    }
    public static string DeleteWorkSuccessMessage()
    {
        return Languages.get_Translation("msgAdminCtrlWorkgroupDeletedSuccessfully");
    }
    public static string PurgeSuccessMessage()
    {
        return Languages.get_Translation("msgAdminCtrlRecPurgedSuccessfully");
    }
    public static string UnableToConnect(string pServer)
    {
        return string.Format("{0} {1} {2}", Languages.get_Translation("msgAdminCtrlTheSQLServer"), pServer, Languages.get_Translation("msgAdminCtrlCouldNotBeOpened"));
    }
    public static string LoginFailed()
    {
        return Languages.get_Translation("msgAdminCtrlLoginFailedPlzConfUNPWD");
    }

    public static string GetClientIpAddress(HttpContext httpContext)
    {
        return ContextService.GetServerVariablesFeature("REMOTE_HOST", httpContext)??"";
    }

    private static DateTime _LastLoginDate { get; set; }
    public static DateTime LastLoginDate
    {
        get
        {
            return _LastLoginDate;
        }
        set
        {
            _LastLoginDate = value;
        }
    }

    private static int? _LastLoginAttempt { get; set; }
    public static int? LastLoginAttempt
    {
        get
        {
            return _LastLoginAttempt;
        }
        set
        {
            _LastLoginAttempt = value;
        }
    }

    private static string _LastLoginUser { get; set; }
    public static string LastLoginUser
    {
        get
        {
            return _LastLoginUser;
        }
        set
        {
            _LastLoginUser = value;
        }
    }

    public static void IsValidQuerySet(bool value, HttpContext httpContext)
    {
        ContextService.SetSessionValue("IsValidQuery", Convert.ToString(value), httpContext);
    }

    public static bool IsValidQueryGet(HttpContext httpContext)
    {
        bool IsValid = true;
        var isvalidquery = ContextService.GetSessionValueByKey("IsValidQuery", httpContext);
        if (!string.IsNullOrWhiteSpace(isvalidquery))
            IsValid = Conversions.ToBoolean(isvalidquery);
        return IsValid;
    }

    //public static string GetUserId
    //{
    //    get
    //    {
    //        string strRefDetails = string.Empty;
    //        if (!string.IsNullOrEmpty(ContextService.GetSessionValueByKey("passport")))
    //            strRefDetails = ContextService.GetObjectFromJson<Passport>("passport").UserId.ToString();

    //        return strRefDetails;
    //    }
    //}

    public static UserPreferences GetUserPreferences
    {
        get
        {
            try
            {
                var httpContext = new HttpContextAccessor().HttpContext;
                var pUserPreferences = new UserPreferences();
                // ' Get browser language
                string[] strLanguages;
                string strLanguage = "en-US";
                if (httpContext.Request.Headers["Accept-Language"].ToString() != "")
                {
                    strLanguages = httpContext.Request.Headers["Accept-Language"].ToString().Split(",");
                    if (Languages.GetXMLLanguages().ContainsKey(strLanguages[0]))
                        strLanguage = strLanguages[0];
                }

                var ci = new CultureInfo(strLanguage);

                // ' Set browser preference
                pUserPreferences.sPreferedLanguage = "en-US";
                pUserPreferences.sPreferedDateFormat = ci.DateTimeFormat.ShortDatePattern;

                // ' Read preference cookies if exists
                if (httpContext.Request.Cookies[CurrentUserName] is not null)
                {
                    
                    string pPreferedLanguage = System.Net.WebUtility.HtmlEncode(ContextService.GetCookiesValueFromMultiple(CurrentUserName, "PreferedLanguage", httpContext));
                    string pPreferedDateFormat = System.Net.WebUtility.HtmlEncode(ContextService.GetCookiesValueFromMultiple(CurrentUserName, "PreferedDateFormat", httpContext));

                    if (!string.IsNullOrEmpty(pPreferedLanguage))
                        pUserPreferences.sPreferedLanguage = pPreferedLanguage;

                    if (!string.IsNullOrEmpty(pPreferedDateFormat))
                        pUserPreferences.sPreferedDateFormat = pPreferedDateFormat;

                }
                else if (!string.IsNullOrEmpty(CurrentUserName))
                {
                    var multiCookie = new Dictionary<string, string>()
                {
                    { "PreferedLanguage", strLanguage },
                    { "PreferedDateFormat", ci.DateTimeFormat.ShortDatePattern }
                };

                    ContextService.RemoveCookie(CurrentUserName, httpContext);

                    if (httpContext.Request.IsHttps)
                        ContextService.CreateCookies(CurrentUserName, multiCookie, httpContext, https: true);
                    else
                        ContextService.CreateCookies(CurrentUserName, multiCookie, httpContext, https: false);
                }
                else
                {
                    var multiCookie = new Dictionary<string, string>()
                {
                    { "PreferedLanguage", strLanguage },
                    { "PreferedDateFormat", ci.DateTimeFormat.ShortDatePattern }
                };

                    ContextService.RemoveCookie(CurrentUserName, httpContext);

                    if (httpContext.Request.IsHttps)
                        ContextService.CreateCookies(CurrentUserName, multiCookie, httpContext, https: true);
                    else
                        ContextService.CreateCookies(CurrentUserName, multiCookie, httpContext, https: false);


                    Thread.CurrentThread.CurrentCulture = ci;
                }

                return pUserPreferences;
            }
            catch (Exception)
            {
                return new UserPreferences();
            }
        }
    }

    public static CultureInfo GetCultureCookies(HttpContext httpContext)
    {
        var pUserPreferences = GetUserPreferences;
        var ci = new CultureInfo(pUserPreferences.sPreferedLanguage);
        ci.DateTimeFormat.ShortDatePattern = pUserPreferences.sPreferedDateFormat;
        string d = CurrentUserName;
        return ci;
    }

    /// <summary>
    /// Convert string date to culture date format for view purpose
    /// </summary>
    /// <returns></returns>
    public static string get_ConvertCultureDate(string strDate, HttpContext httpContext, bool bWithTime = false, bool bConvertToLocalTimeZone = true, bool bDetectTime = false)
    {
        DateTime dtPreFormat;
        if (string.IsNullOrWhiteSpace(strDate))
            return strDate;

        try
        {
            dtPreFormat = DateTime.Parse(strDate);
        }
        catch (Exception)
        {
            dtPreFormat = DateTime.ParseExact(strDate, GetCultureCookies(httpContext).DateTimeFormat.ShortDatePattern + " hh:mm:ss tt", CultureInfo.InvariantCulture);
        }

        return get_ConvertCultureDate(dtPreFormat, httpContext, bWithTime, bConvertToLocalTimeZone, bDetectTime);
    }

    /// <summary>
    /// Convert date object to culture date formated string for view purpose
    /// </summary>
    /// <returns></returns>
    public static string get_ConvertCultureDate(DateTime dtPreFormat, HttpContext httpContext, bool bWithTime = false, bool bConvertToLocalTimeZone = true, bool bDetectTime = false)
    {
        string dtCultureFormat = string.Empty;
        if (dtPreFormat == default)
            return dtCultureFormat;
        if (bDetectTime)
            bWithTime = dtPreFormat.IncludesTime();

        if (bWithTime)
        {
            try
            {
                if (bConvertToLocalTimeZone)
                    dtPreFormat = dtPreFormat.ToUniversalTime().ToClientTimeDate(httpContext);
                dtCultureFormat = dtPreFormat.ToString(GetCultureCookies(httpContext).DateTimeFormat.ShortDatePattern + " hh:mm:ss tt", CultureInfo.InvariantCulture);
            }
            // dtCultureFormat = Date.Parse(strDate, New CultureInfo("en-US")).ToString(Keys.GetCultureCookies().DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture)
            catch (Exception)
            {
                if (bConvertToLocalTimeZone)
                {
                    dtCultureFormat = dtPreFormat.ToUniversalTime().ToClientTimeDate(httpContext).ToShortDateString();
                }
                else
                {
                    dtCultureFormat = dtPreFormat.ToShortDateString();
                }
            }
        }
        else
        {
            try
            {
                dtCultureFormat = dtPreFormat.ToString(GetCultureCookies(httpContext).DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture);
            }
            // dtCultureFormat = Date.Parse(strDate, New CultureInfo("en-US")).ToString(Keys.GetCultureCookies().DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture)
            catch (Exception)
            {
                dtCultureFormat = dtPreFormat.ToShortDateString();
            }
        }

        return dtCultureFormat;
    }
    /// <summary>
    /// Convert string date to culture date format for insert (it Returns string date)
    /// </summary>
    /// <returns></returns>
    public static string get_ConvertCultureDate(string strDate, string strMode, HttpContext httpContext)
    {
        string dtCultureFormat = string.Empty;
        // Languages.WriteTxtFile("Edit fucntion before => In Convert Function ")
        if (!string.IsNullOrEmpty(strDate))
            dtCultureFormat = get_ConvertDateForEdit(strDate, httpContext);
        return dtCultureFormat;
    }
    /// <summary>
    /// Convert string date to culture date format for insert (it Returns date)
    /// </summary>
    /// <returns></returns>
    public static string get_ConvertStringtoDateWithTime(DateTime strDate, HttpContext httpContext)
    {
        var dtCultureFormat = new DateTime();
        // dtCutlureFormat = Date.Parse(strDate)
        dtCultureFormat = strDate;
        var tm = dtCultureFormat.TimeOfDay;
        var dt = get_ConvertStringToCultureDate(Conversions.ToString(strDate), httpContext);
        var dtup = dt.Add(tm);
        return dtup.ToString("yyyy-MM-dd hh:mm:ss tt", CultureInfo.InvariantCulture);
    }

    public static string get_ConvertDateForEdit(string strDate, HttpContext httpContext)
    {
        var dt = get_ConvertStringToCultureDate(strDate, httpContext);
        // ' Read database culture
        string sDBCulture = string.Empty;
        //if (Current.Request.Cookies["DBCulture"] is not null)
        if (httpContext.Request.Cookies["DBCulture"] is not null)
        {
            sDBCulture = httpContext.Request.Cookies["DBCulture"].ToString().Trim();
        }
        else
        {
            sDBCulture = GetSQLCulture(httpContext);
            //Current.Response.Cookies["DBCulture"].Value = sDBCulture;
            //Current.Response.Cookies["DBCulture"].Expires = DateTime.Now.AddDays(180d);
            ContextService.CreateCookies("DBCulture", sDBCulture, httpContext);

        }
        // check Datetime
        string date = "";
        try { date = dt.ToString("MM/dd/yyyy").Split(" ")[0]; } catch (Exception) { }
        return string.Format(new CultureInfo(sDBCulture), "{0:d}", date);
    }

    public static DateTime get_ConvertStringToCultureDate(string strDate, HttpContext httpContext)
    {
        // Languages.WriteTxtFile(" ConvertStringToCultureDate Before conversion  Date Format = " + strDate)
        string dtCultureFormat = string.Empty;
        // Dim clrenUS As New CultureInfo("en-US")
        // dtCutlureFormat = Date.Parse(strDate.ToString(Keys.GetCultureCookies().DateTimeFormat.ShortDatePattern, clrenUS)
        dtCultureFormat = get_ConvertCultureDate(strDate, httpContext);

        // Languages.WriteTxtFile(" ConvertStringToCultureDate  after Date Format = " + dtCutlureFormat + "    Format : " + Keys.GetCultureCookies().DateTimeFormat.ShortDatePattern)

        var seperator = new[] { '/', '-', '.', ' ' };
        var strDateSplit = dtCultureFormat.Split(seperator);
        var strDateFormatSplit = GetCultureCookies(httpContext).DateTimeFormat.ShortDatePattern.Split(seperator);
        int iDay = default, iMonth = default, iYear = default;

        for (int i = 0, loopTo = strDateFormatSplit.Length - 1; i <= loopTo; i++)
        {
            switch (strDateFormatSplit[i].ToLower() ?? "")
            {
                case "dd":
                    {
                        iDay = Conversions.ToInteger(strDateSplit[i]);
                        break;
                    }
                case "mm":
                    {
                        iMonth = Conversions.ToInteger(strDateSplit[i]);
                        break;
                    }
                case "d":
                    {
                        iDay = Conversions.ToInteger(strDateSplit[i]);
                        break;
                    }
                case "m":
                    {
                        iMonth = Conversions.ToInteger(strDateSplit[i]);
                        break;
                    }
                case "mmm":
                    {
                        switch (strDateSplit[i].ToLower() ?? "")
                        {
                            case "jan":
                                {
                                    iMonth = 1;
                                    break;
                                }
                            case "feb":
                                {
                                    iMonth = 2;
                                    break;
                                }
                            case "mar":
                                {
                                    iMonth = 3;
                                    break;
                                }
                            case "apr":
                                {
                                    iMonth = 4;
                                    break;
                                }
                            case "may":
                                {
                                    iMonth = 5;
                                    break;
                                }
                            case "jun":
                                {
                                    iMonth = 6;
                                    break;
                                }
                            case "jul":
                                {
                                    iMonth = 7;
                                    break;
                                }
                            case "aug":
                                {
                                    iMonth = 8;
                                    break;
                                }
                            case "sep":
                                {
                                    iMonth = 9;
                                    break;
                                }
                            case "oct":
                                {
                                    iMonth = 10;
                                    break;
                                }
                            case "nov":
                                {
                                    iMonth = 11;
                                    break;
                                }
                            case "dec":
                                {
                                    iMonth = 12;
                                    break;
                                }
                        }

                        break;
                    }
                case "yy":
                    {
                        iYear = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(Conversions.ToInteger(strDateSplit[i]));
                        break;
                    }
                case "yyyy":
                    {
                        iYear = Conversions.ToInteger(strDateSplit[i]);
                        break;
                    }
            }
        }
        return new DateTime(iYear, iMonth, iDay, CultureInfo.InvariantCulture.Calendar);
        //System.Globalization.CultureInfo cultureinfo = new System.Globalization.CultureInfo("en-US");
        //return DateTime.Parse(new DateTime(iYear, iMonth, iDay).ToString(), new System.Globalization.CultureInfo("en-US"));

    }

    public static string GetSQLCulture(HttpContext httpContext)
    {
        IDBManager pIDBManager = new DBManager(get_GetDBConnectionString());
        pIDBManager.ConnectionString = get_GetDBConnectionString();
        string result = pIDBManager.ExecuteScalar(CommandType.Text, "SELECT LOWER(alias) as Alias,name FROM sys.syslanguages where rtrim(ltrim(lower(name))) in (SELECT top 1 RTRIM(LTRIM(LOWER(@@language))));").ToString();
        pIDBManager.Dispose();
        if (string.IsNullOrEmpty(result))
            result = "english";

        string sCulture = "en-US";
        switch (result.Trim().ToLower() ?? "")
        {
            case "english":
                {
                    sCulture = "en-US";
                    break;
                }
            case "german":
                {
                    sCulture = "en-US";
                    break;
                }
            case "french":
                {
                    sCulture = "en-US";
                    break;
                }
            case "japanese":
                {
                    sCulture = "en-US";
                    break;
                }
            case "danish":
                {
                    sCulture = "en-US";
                    break;
                }
            case "spanish":
                {
                    sCulture = "en-US";
                    break;
                }
            case "italian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "dutch":
                {
                    sCulture = "en-US";
                    break;
                }
            case "norwegian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "portuguese":
                {
                    sCulture = "en-US";
                    break;
                }
            case "finnish":
                {
                    sCulture = "en-US";
                    break;
                }
            case "swedish":
                {
                    sCulture = "en-US";
                    break;
                }
            case "czech":
                {
                    sCulture = "en-US";
                    break;
                }
            case "hungarian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "polish":
                {
                    sCulture = "en-US";
                    break;
                }
            case "romanian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "croatian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "slovak":
                {
                    sCulture = "en-US";
                    break;
                }
            case "slovenian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "greek":
                {
                    sCulture = "en-US";
                    break;
                }
            case "bulgarian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "russian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "turkish":
                {
                    sCulture = "en-US";
                    break;
                }
            case "british english":
                {
                    sCulture = "en-US";
                    break;
                }
            case "estonian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "latvian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "lithuanian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "brazilian":
                {
                    sCulture = "en-US";
                    break;
                }
            case "traditional chinese":
                {
                    sCulture = "en-US";
                    break;
                }
            case "korean":
                {
                    sCulture = "en-US";
                    break;
                }
            case "simplified chinese":
                {
                    sCulture = "en-US";
                    break;
                }
            case "arabic":
                {
                    sCulture = "en-US";
                    break;
                }
            case "thai":
                {
                    sCulture = "en-US";
                    break;
                }

            default:
                {
                    sCulture = "en-US";
                    break;
                }
        }
        return sCulture;
    }
    /// Converting orignal culture string date to user defined format date
    public static string get_ConvertBrowserCultureStringToUDFDateString(string strDate, HttpContext httpContext)
    {

        string[] strLanguages;
        string strLanguage = "en-US";
        if (httpContext.Request.Headers["Accept-Language"].ToString() != "")
        {
            strLanguages = httpContext.Request.Headers["Accept-Language"].ToString().Split(",");
            if (Languages.GetXMLLanguages().ContainsKey(strLanguages[0]))
                strLanguage = strLanguages[0];
        }

        var ci = new CultureInfo(strLanguage);
        string dtCultureFormat = string.Empty;

        try
        {
            dtCultureFormat = DateTime.ParseExact(strDate, ci.DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture).ToShortDateString();
        }
        catch (Exception)
        {
            dtCultureFormat = string.Empty;
        }

        return dtCultureFormat;
    }

    public static bool get_RefereshDetails(string strPageName, Passport passport, HttpContext httpContext)
    {
        string strRefDetails = string.Empty;
        bool bSameUser = true;

        if (passport != null)
        {
            strRefDetails = passport.UserId.ToString();

            switch (strPageName ?? "")
            {
                case "Admin":
                    {
                        if (ContextService.GetSessionValueByKey("iAdminRefId", httpContext) is not null)
                        {
                            if (!string.IsNullOrEmpty(ContextService.GetSessionValueByKey("iAdminRefId", httpContext)))
                            {
                                if (!ContextService.GetSessionValueByKey("iAdminRefId", httpContext).Equals(strRefDetails))
                                    bSameUser = false;
                            }
                        }

                        break;
                    }
                case "Import":
                    {
                        var iImportRef = ContextService.GetSessionValueByKey("iImportRefId", httpContext);
                        if (!string.IsNullOrEmpty(iImportRef))
                        {
                            if (!iImportRef.ToString().Equals(strRefDetails))
                                bSameUser = false;
                        }

                        break;
                    }
                case "Scanner":
                    {
                        var iScannerRefId = ContextService.GetSessionValueByKey("iScannerRefId", httpContext);
                        if (!string.IsNullOrEmpty(iScannerRefId))
                        {
                            if (!iScannerRefId.ToString().Equals(strRefDetails))
                                bSameUser = false;
                        }

                        break;
                    }
                case "LabelManager":
                    {
                        var iLabelRefId = ContextService.GetSessionValueByKey("iLabelRefId", httpContext);
                        if (!string.IsNullOrEmpty(iLabelRefId))
                        {
                            if (!iLabelRefId.ToString().Equals(strRefDetails))
                                bSameUser = false;
                        }

                        break;
                    }
                case "Retention":
                    {
                        var iLabelRefId = ContextService.GetSessionValueByKey("iLabelRefId", httpContext);
                        if (!string.IsNullOrEmpty(iLabelRefId))
                        {
                            if (!iLabelRefId.ToString().Equals(strRefDetails))
                                bSameUser = false;
                        }

                        break;
                    }

                default:
                    {
                        return true;
                    }
            }
        }

        return bSameUser;
    }

    public static string get_DefaultDBConText(string stringType, HttpContext httpContext)
    {
        string sConnStr = get_GetDBConnectionString();
        var builder = new SqlConnectionStringBuilder(sConnStr);
        string sConnect = string.Empty;
        string DBServer = builder.DataSource;
        string DBDatabase = builder.InitialCatalog;
        string DBUserId = builder.UserID;
        string DBPassword = builder.Password;
        int DBConTimeOut = builder.ConnectTimeout;

        switch (stringType ?? "")
        {
            case "DBConnectionText":
                {
                    sConnect += "UID=" + DBUserId + ";PWD=" + DBPassword + ";DATABASE=" + DBDatabase + ";Server=" + DBServer + ";";
                    return sConnect;
                }
            case "DBConnectionTimeout":
                {
                    return DBConTimeOut.ToString();
                }
            case "DBDatabase":
                {
                    return DBDatabase;
                }
            case "DBPassword":
                {
                    return DBPassword;
                }
            case "DBProvider":
                {
                    return DBProvider;
                }
            case "DBServer":
                {
                    return DBServer;
                }
            case "DBUserId":
                {
                    return DBUserId;
                }

            default:
                {
                    return sConnect;
                }
        }
    }

    public static string DBProvider
    {
        get
        {
            string rtn;

            try
            {
                IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                
                rtn = ContextService.AppSetting("DBProvider", configuration);
                if (string.IsNullOrEmpty(rtn))
                    rtn = SlimShared.DBProvider;
            }
            catch (Exception)
            {
                SlimShared.logInformationAsType(string.Format("DBProvider could not be found in Web.config, using \"{0}\"", SlimShared.DBProvider), EventLogEntryType.Error);
                rtn = SlimShared.DBProvider;
            }

            return rtn;
        }
    }

    public static string get_DefaultConnectionString(bool includeProvider, string sDatabaseName = "")
    {
        var http = new HttpContextAccessor().HttpContext;
        string sConnStr = get_GetDBConnectionString();
        var builder = new SqlConnectionStringBuilder(sConnStr);

        string sConnect = string.Empty;
        string DBServer = builder.DataSource;
        string DBDatabase = builder.InitialCatalog;
        if (!string.IsNullOrEmpty(sDatabaseName))
            DBDatabase = sDatabaseName;

        string DBUserId = builder.UserID;
        string DBPassword = builder.Password;
        if (includeProvider)
            sConnect += string.Format("Provider={0};", DBProvider);

        sConnect += "Data Source=" + DBServer + "; Initial Catalog=" + DBDatabase + "; "; // Persist Security Info=true; "

        if (!string.IsNullOrEmpty(Strings.Trim(DBUserId)))
        {
            sConnect += "User Id=" + DBUserId + "; Password=" + DBPassword + ";";
        }
        else
        {
            sConnect += "Persist Security Info=True;Integrated Security=SSPI;";
        }

        return sConnect;  // System.Configuration.ConfigurationManager.ConnectionStrings("TABFusionRMSRMSDemoContext").ToString()
    }

    public static string get_GetDBConnectionString(Databas oDatabase = null)
    {
        var httpContext = new HttpContextAccessor().HttpContext;
        var connectionString = ContextService.GetSessionValueByKey("ConnectionString", httpContext);
        string strConn = string.Empty;

        if (!ContextService.SessionIsNull(httpContext))
        {
            if (connectionString is not null)
            {
                if (oDatabase is not null)
                {
                    strConn = DataServices.GetConnectionString(oDatabase, false);
                }
                else
                {
                    strConn = connectionString;
                }

                if (!strConn.Contains("MultipleActiveResultSets"))
                    strConn = strConn + ";MultipleActiveResultSets=True";
            }
        }

        return strConn;
    }

    public static List<KeyValuePair<string, string>> GetParamFromConnString(HttpContext httpContext, Databas dbObj = null)
    {
        string conString = get_GetDBConnectionString(dbObj);
        var builder = new SqlConnectionStringBuilder(conString);
        var conStrList = new List<KeyValuePair<string, string>>();
        // Dim sConnect As String = String.Empty
        string DBServer = builder.DataSource;
        string DBDatabase = builder.InitialCatalog;
        string DBUserId = builder.UserID;
        string DBPassword = builder.Password;
        conStrList.Add(new KeyValuePair<string, string>("DBServer", DBServer));
        conStrList.Add(new KeyValuePair<string, string>("DBDatabase", DBDatabase));
        conStrList.Add(new KeyValuePair<string, string>("DBUserId", DBUserId));
        conStrList.Add(new KeyValuePair<string, string>("DBPassword", DBPassword));
        return conStrList;
    }

    //public static void ShowErrorMessage(Page pg)
    //{
    //    string StrScript = "ShowErrorMessge();";
    //    ScriptManager.RegisterStartupScript(pg, pg.GetType(), "Pop", StrScript, true);
    //}

    //public static void ShowMessagePop(Page pg, string strMessage, string msgtype)
    //{
    //    string StrScript = "showAjaxReturnMessage('" + strMessage + "', '" + msgtype + "');";
    //    ScriptManager.RegisterStartupScript(pg, pg.GetType(), "Pop", StrScript, true);
    //}

    //public static void timedRefresh(Page pg, int timeoutPeriod)
    //{
    //    string StrScript = "timedRefresh(" + timeoutPeriod + ");";
    //    ScriptManager.RegisterStartupScript(pg, pg.GetType(), "Pop", StrScript, true);
    //}

    private static string msDBName;
    public static string DBName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(msDBName))
                msDBName = "DB_Engine";
            return msDBName;
        }
        set
        {
            msDBName = Strings.Trim(value);
        }
    }

    public static string ServerName(HttpContext httpContext)
    {
        string strServer = string.Empty;
        if (httpContext is not null)
        {
            if (httpContext.Session.GetString("Server") is not null)
                strServer = httpContext.Session.GetString("Server");
        }

        return strServer;
    }

    public static string DatabaseName(HttpContext httpContext)
    {
        string strDatabaseName = "";
        if (httpContext is not null)
        {
            if (httpContext.Session.GetString("DatabaseDisplayName") is not null)
                strDatabaseName = httpContext.Session.GetString("DatabaseDisplayName");
        }
        return strDatabaseName;
    }

    public static string CurrentUserName
    {
         get
        {
            var httpContext = new HttpContextAccessor().HttpContext;
            string strCurrentUserName = "";
            if (httpContext is not null)
            {
                if (httpContext.Session is not null)
                    if (httpContext.Session.GetString("UserName") is not null)
                        strCurrentUserName = httpContext.Session.GetString("UserName");
            }
            return strCurrentUserName;
        }
    }

    public static List<KeyValuePair<string, bool>> GetImportDDL(IQueryable<ImportLoad> IQueryImportLoad, IQueryable<ImportField> IQueryImportField, Passport passport)
    {
        //var passport = ContextService.GetObjectFromJson<Passport>("passport");
        var importLoadData = IQueryImportLoad.Where(m => m.FileName != null && m.LoadName != null && (m.FromHandHeldEnum == 0 || m.FromHandHeldEnum != null)).OrderBy(m => m.ID);
        var importDDLList = new List<KeyValuePair<string, bool>>();
        string sHoldJobName = "";
        // Dim bHavePermission As Boolean

        if (importLoadData is not null)
        {
            foreach (ImportLoad importLoadObj in importLoadData)
            {
                if (Strings.StrComp(importLoadObj.FileName, IMPORT_TRACK, Constants.vbTextCompare) == 0)
                {
                    if (passport.CheckPermission(SECURE_TRACKING, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.Application, (Permissions.Permission)Enums.PassportPermissions.Access))
                    {
                        importDDLList.Add(new KeyValuePair<string, bool>(importLoadObj.LoadName, false));
                    }
                }
                else if (passport.CheckPermission(importLoadObj.FileName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.Table, (Permissions.Permission)Enums.PassportPermissions.Import))
                {
                    bool HasPCOrImageFile = IQueryImportField.Where(m => m.ImportLoad.Trim().ToLower().Equals(importLoadObj.LoadName.Trim().ToLower())).Any(m => m.FieldName.Trim().ToUpper().Equals("<<PCFILE COPY>>") | m.FieldName.Trim().ToUpper().Equals("<<IMAGE COPY>>"));
                    importDDLList.Add(new KeyValuePair<string, bool>(importLoadObj.LoadName, HasPCOrImageFile));
                }
            }
        }

        return importDDLList;
    }

    #region SharePoint
    public static string IsMovedFromSP
    {
        get
        {
            string strServer = "";
            var http = new HttpContextAccessor().HttpContext;
            if (http.Session is not null)
            {
                if (ContextService.GetSessionValueByKey("IsMovedFromSP", http) is not null)
                    strServer = ContextService.GetSessionValueByKey("IsMovedFromSP", http);
            }
            return strServer;
        }
        set
        {
            var http = new HttpContextAccessor().HttpContext;

            if (!string.IsNullOrEmpty(value))
            {
                ContextService.SetSessionValue("IsMovedFromSP", value.ToString(), http);
            }
            else
            {
                http.Session.Remove("IsMovedFromSP");
            }
        }
    }

    public static string SPSiteURL
    {
        get
        {
            string strServer = "";
            var http = new HttpContextAccessor().HttpContext;

            if (!ContextService.SessionIsNull(http))
            {
                var SPSiteURL = ContextService.GetSessionValueByKey("SPSiteURL", http);
                if (!string.IsNullOrEmpty(SPSiteURL))
                    strServer = SPSiteURL.ToString();
            }
            return strServer;
        }
        set
        {
            var http = new HttpContextAccessor().HttpContext;

            if (!string.IsNullOrEmpty(value))
            {
                ContextService.SetSessionValue("SPSiteURL", value, http);
            }
            else
            {
                ContextService.RemoveSession("SPSiteURL", http);
            }
        }
    }

    //public static void UpdateSPInfoWebConfig(string strSPURL, string strSPFileURL)
    //{
    //    var webConfigApp = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

    //    if (webConfigApp.AppSettings.Settings["SPURL"] is not null)
    //    {
    //        if (!webConfigApp.AppSettings.Settings["SPURL"].Value.ToString().Equals(strSPURL))
    //            webConfigApp.AppSettings.Settings["SPURL"].Value = strSPURL;
    //    }
    //    else
    //    {
    //        webConfigApp.AppSettings.Settings.Add("SPURL", strSPURL);
    //    }

    //    if (webConfigApp.AppSettings.Settings["SPDocURL"] is not null)
    //    {
    //        if (!webConfigApp.AppSettings.Settings["SPDocURL"].Value.ToString().Equals(strSPFileURL))
    //            webConfigApp.AppSettings.Settings["SPDocURL"].Value = strSPFileURL;
    //    }
    //    else
    //    {
    //        webConfigApp.AppSettings.Settings.Add("SPDocURL", strSPFileURL);
    //    }
    //    webConfigApp.Save();
    //}

    //public static void removespinfowebconfig()
    //{
    //    var webconfigapp = system.web.configuration.webconfigurationmanager.openwebconfiguration("~");

    //    if (webconfigapp.appsettings.settings["spurl"] is not null)
    //        webconfigapp.appsettings.settings.remove("spurl");
    //    if (webconfigapp.appsettings.settings["spdocurl"] is not null)
    //        webconfigapp.appsettings.settings.remove("spdocurl");
    //    webconfigapp.save();
    //}

    public static void RemoveDocument(IConfiguration configuration)
    {
        string SharepointUsername = GetSPUserName(configuration); // Encrypt.AesDecrypt(ConfigurationManager.AppSettings("SPUserName").ToString())
        string SharepointPassword = GetSPPassword(configuration); // Encrypt.AesDecrypt(ConfigurationManager.AppSettings("SPPassword").ToString())
        string SiteURL = configuration["SPURL"].ToString();
        string SPFileParameters = configuration["SPDocURL"].ToString();

        try
        {
            var spParaSplit = SPFileParameters.Split('#');
            string spLibrary = spParaSplit[0];
            string spItemId = spParaSplit[1];
            spLibrary = spLibrary.ToLower().Equals("shared documents") ? "Documents" : spLibrary;

            if (configuration["SPVersion"].ToString().Trim().ToLower().Equals("office365"))
            {
                // Languages.WriteTxtFile("Inside remove 365 condition")
                var context = new Microsoft.SharePoint.Client.ClientContext(SiteURL);
                var passWord = new System.Security.SecureString();
                foreach (var c in SharepointPassword.ToCharArray())
                    passWord.AppendChar(c);
                context.Credentials = new Microsoft.SharePoint.Client.SharePointOnlineCredentials(SharepointUsername.Trim(), passWord);
                // Languages.WriteTxtFile("Library Name & item id : " + spLibrary + " : " + spItemId)
                var oList = context.Web.Lists.GetByTitle(spLibrary);
                var oListItem = oList.GetItemById(spItemId);
                // Languages.WriteTxtFile("Got list success : ")
                oListItem.DeleteObject();
                context.ExecuteQuery();
            }
            // Languages.WriteTxtFile("Delete Success : ")
            else
            {
                var clientContext = new Microsoft.SharePoint.Client.ClientContext(SiteURL);
                var credential = new System.Net.NetworkCredential(SharepointUsername, SharepointPassword);
                clientContext.Credentials = credential;
                var oList = clientContext.Web.Lists.GetByTitle(spLibrary);
                var oListItem = oList.GetItemById(spItemId);
                oListItem.DeleteObject();
                clientContext.ExecuteQuery();
            }
        }
        // RemoveSPInfoWebConfig()
        catch (Exception ex)
        {
            string msg = ex.Message;
            // Languages.WriteTxtFile("error : " + msg)
        }
    }

    public static string GetSPUserName(IConfiguration configuration)
    {
        string strSPUserName = string.Empty;
        try
        {

            strSPUserName = Encrypt.AesDecrypt(configuration["SPUserName"].ToString());
        }
        catch (Exception)
        {
            strSPUserName = configuration["SPUserName"].ToString();
        }

        if (string.IsNullOrEmpty(strSPUserName))
        {
            Keys.WriteToEventLog("SharePoint UserName is not provided in web.config or Error occured during get SP username");
        }

        return strSPUserName;
    }

    public static string GetSPPassword(IConfiguration configuration)
    {
        string strSPPassword = string.Empty;
        try
        {
            strSPPassword = Encrypt.AesDecrypt(configuration["SPPassword"].ToString());
        }
        catch (Exception)
        {
            strSPPassword = configuration["SPPassword"].ToString();
        }

        if (string.IsNullOrEmpty(strSPPassword))
        {
            WriteToEventLog("SharePoint password is not provided in web.config or Error occured during get password");
        }

        return strSPPassword;
    }

    public static bool WriteToEventLog(string strMessage, string appName = "TAB FusionRMS", EventLogEntryType eventType = EventLogEntryType.Information, string logName = "WebAccess")
    {
        try
        {
            string sMachine;
            sMachine = ".";
            EventLog.DeleteEventSource(appName);
            if (!EventLog.SourceExists(appName, sMachine))
            {
                var source = new EventSourceCreationData(appName, logName);
                EventLog.CreateEventSource(source);
                // EventLog.CreateEventSource(appName, sLog, sMachine)
            }

            var ELog = new EventLog(logName, sMachine, appName);
            ELog.WriteEntry(strMessage);
            ELog.WriteEntry(strMessage, EventLogEntryType.Error, 234, 3);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    #endregion
}