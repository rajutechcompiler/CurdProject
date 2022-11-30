using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using System.Globalization;
using TabFusionRMS.DataBaseManagerVB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices;
using TabFusionRMS.WebCS.Controllers;
using System.Data;
using TabFusionRMS.Resource;
using Microsoft.Extensions.Hosting.Internal;
using TabFusionRMS.WebCS.Models;
using Microsoft.AspNetCore.StaticFiles;
using Smead.Security;
using Newtonsoft.Json;
using System.IO;
using Smead.RecordsManagement;
using Smead.RecordsManagement.Imaging.Export;
using System.Drawing;
using Smead.RecordsManagement.Imaging;
using System.Net;
using Exceptions = Smead.RecordsManagement.Imaging.Permissions.ExceptionString;
using System.Drawing.Text;
using System.Data.SqlClient;
using Microsoft.SharePoint.Client.Utilities;
using TabFusionRMS.WebCS.FusionElevenModels;
using Leadtools;
using Leadtools.Codecs;
using Grpc.Core;
using static Smead.RecordsManagement.Imaging.Export.IO;

namespace TabFusionRMS.WebCS.Controllers
{
    public class CommonController : BaseController
    {
        private IDBManager _IDBManager = new DBManager();

        public CultureInfo cultureInfo;
        private readonly IConfiguration _config; 
        public CommonController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetGridViewSettings(string pGridName)
        {
            IRepository<vwGridSetting> _ivwGridSetting = new Repositories<vwGridSetting>();
            var lGridColumns = _ivwGridSetting.All().Where(x => x.GridSettingsName.ToLower().Trim().Equals(pGridName.ToLower().Trim()) && x.IsActive == true).
                Select(a => new {
                    srno = a.GridColumnSrNo,
                    name = a.GridColumnName,
                    sortable = a.IsSortable,
                    columnWithCheckbox = a.IsCheckbox,
                    displayName = a.GridColumnDisplayName
                });

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            var jsonObject = JsonConvert.SerializeObject(lGridColumns, Formatting.Indented, Setting);
            return Json(jsonObject);
        }

        public JsonResult GetGridViewSettings1(string pGridName)
        {
            IRepository<Table> _iTable= new Repositories<Table>();
            IRepository<Databas> _iDatabase = new Repositories<Databas>();
            try
            {
                DataTable dtRecords = null;
                var pTableEntity = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pGridName.Trim().ToLower())).FirstOrDefault();
                Databas pDatabaseEntity = null;

                if(pTableEntity is not null)
                {
                    if (!string.IsNullOrEmpty(pTableEntity.DBName))
                    {
                        pDatabaseEntity = _iDatabase.Where(x => x.DBName.Trim().ToLower().Equals(pTableEntity.DBName.Trim().ToLower())).FirstOrDefault();
                        _IDBManager.ConnectionString = Keys.get_GetDBConnectionString(pDatabaseEntity);
                    }
                    else
                    {
                        _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
                    }
                }
                else
                {
                    _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
                }

                _IDBManager.CreateParameters(6);
                _IDBManager.AddParameters(0, "@TableName", pGridName);
                _IDBManager.AddParameters(1, "@PageNo", 1);
                _IDBManager.AddParameters(2, "@PageSize", 0);
                _IDBManager.AddParameters(3, "@DataAndColumnInfo", false);
                _IDBManager.AddParameters(4, "@ColName", "");
                _IDBManager.AddParameters(5, "@Sort", "");
                DataSet loutput = _IDBManager.ExecuteDataSetWithSchema(System.Data.CommandType.StoredProcedure, "SP_RMS_GetTableData");
                _IDBManager.Dispose();
                dtRecords = loutput.Tables[0];
                var dateColumn = new List<Int32>();
                Int32 i = 0;
                var GridColumnEntities = new List<GridColumns>();
                foreach(DataColumn column in dtRecords.Columns)
                {
                    var GridColumnEntity = new GridColumns();
                    GridColumnEntity.ColumnSrNo = i + 1;
                    GridColumnEntity.ColumnId = i + 1;
                    GridColumnEntity.ColumnName = column.ColumnName;
                    GridColumnEntity.ColumnDisplayName = column.ColumnName;
                    GridColumnEntity.ColumnDataType = column.DataType.Name;
                    GridColumnEntity.ColumnMaxLength = column.MaxLength.ToString();
                    GridColumnEntity.IsPk = column.Unique;
                    GridColumnEntity.AutoInc = column.AutoIncrement;
                    GridColumnEntity.IsNull = column.AllowDBNull;
                    GridColumnEntity.ReadOnlye = column.ReadOnly;
                    GridColumnEntities.Add(GridColumnEntity);

                    if(column.DataType.Name.Trim().ToString().IndexOf("Date") >=0)
                    {
                        dateColumn.Add(i);
                    }
                    i = i + 1;
                }

                Int32 j =0;
                foreach(DataRow rows in loutput.Tables[0].Rows)
                {
                    foreach(Int32 item in dateColumn)
                    {
                        var tempDate = loutput.Tables[0].Rows[j].ItemArray[item];
                        if (!(tempDate == DBNull.Value))
                        {
                            DateTime convertInDate;
                                if (DateTime.TryParse(tempDate.ToString(),out convertInDate)) 
                                {
                                rows[item] = DateTime.Parse(tempDate.ToString());// '.ToString(CultureInfo.InvariantCulture) 'Don't convert data grid shown directly from database
                                }
                        }
                    }
                    j = j + 1;
                }

                var lGridColumns = GridColumnEntities.Select(a => new {
                    srno = a.ColumnSrNo,
                    name = a.ColumnName,
                    displayName = a.ColumnDisplayName,
                    dataType = a.ColumnDataType,
                    maxLength = a.ColumnMaxLength,
                    isPk = a.IsPk,
                    isNull = !a.IsNull ,
                    autoInc = a.AutoInc,
                    readOnly = a.ReadOnlye
                });
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                var jsonObject = JsonConvert.SerializeObject(lGridColumns, Formatting.Indented, Setting);
                return Json(jsonObject);

               }
            catch (Exception)
            {
                throw;
            }

        }
        public IActionResult ArrangeGridOrder(List<GridSettingsColumns> pGridOrders)
        {
            ContextService.SetSessionValue("GridOrders", JsonConvert.SerializeObject(pGridOrders), httpContext);
            return Json(Languages.get_Translation("Success"));
        }

        public void SetCulture()
        {
            cultureInfo = new CultureInfo(httpContext.Request.Headers["Accept-Language"].ToString().Split(",")[0]);
            if (cultureInfo.IsNeutralCulture)
            {
                cultureInfo = CultureInfo.CreateSpecificCulture(cultureInfo.Name);
            }
            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }

        public IActionResult SetGridOrders(string pGridName)
        {
            if (ContextService.GetSessionValueByKey("GridOrders", httpContext) is null)
            {
                return Json("Error");
            }
            if (string.IsNullOrEmpty(pGridName))
            {
                return Json(Languages.get_Translation("msgCommonGridNameNotValid"));
            }

            List<GridSettingsColumns> lGridSettingsColumnsEntities = ContextService.GetObjectFromJson<List<GridSettingsColumns>>("GridOrders", httpContext);
            IRepository<vwGridSetting> _ivwGridSetting = new Repositories<vwGridSetting>();
            var pGridSettingsEntity = _ivwGridSetting.All().Where(x => x.GridSettingsName.ToLower().Trim().Equals(pGridName.ToLower().Trim())).FirstOrDefault();

            if (pGridSettingsEntity is null)
            {
                return Json(Languages.get_Translation("msgCommonReOrderYourColumn"));
            }

            int pGridSettingId = pGridSettingsEntity.GridSettingsId;
            IRepository<GridColumn> _iGridColumn =new Repositories<GridColumn>();
            var lGridColumnEntities = _iGridColumn.Where(x => Operators.ConditionalCompareObjectEqual(x.GridSettingsId, pGridSettingId, false) && Operators.ConditionalCompareObjectEqual(x.IsActive, true, false));
            int iCount = 0;
            foreach (GridSettingsColumns pGridSettingsColumnsEntity in lGridSettingsColumnsEntities)
            {
                if (pGridSettingsColumnsEntity.index is not null)
                {

                    var pGridColumnEntities = lGridColumnEntities.Where(x => x.GridColumnName.Trim().ToLower().Equals(pGridSettingsColumnsEntity.index.Trim().ToLower())).FirstOrDefault();

                    if (pGridSettingsColumnsEntity.key == true)
                    {
                        pGridColumnEntities.GridColumnSrNo = -1;
                    }
                    else
                    {
                        pGridColumnEntities.GridColumnSrNo = iCount;
                    }

                    _iGridColumn.Update(pGridColumnEntities);

                    if (pGridSettingsColumnsEntity.key == false)
                    {
                        iCount += 1;
                    }
                }
            }

            httpContext.Session.Remove("GridOrders");

            return Json(Languages.get_Translation("Success"), "application/json; charset=utf-8");
        }

        public JsonResult GetTableListLabel()
        {
            var model = new CommonControllerModel();
            IRepository<Table> _iTable =new Repositories<Table>();
            IRepository<vwTablesAll> _ivwTablesAll = new Repositories<vwTablesAll>();
            try
            {
                var pTableList = from t in _iTable.All().OrderBy(x=>x.TableName) select t;
                var lAllTables = _ivwTablesAll.All().Select(x => x.TABLE_NAME).ToList();
                pTableList = pTableList.Where(x => lAllTables.Contains(x.TableName));

                List<Table> tableList = new List<Table>();

                foreach (Table tempTable in pTableList)
                {
                    if ((passport.CheckPermission(tempTable.TableName, Smead.Security.SecureObject.SecureObjectType.Table, Smead.Security.Permissions.Permission.View)))
                        tableList.Add(tempTable);
                }

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                var jsonObject = JsonConvert.SerializeObject(tableList, Formatting.Indented, Setting);
                model.JsonString = jsonObject;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }

        //for _Auditing
        public JsonResult GetTableList()
        {
            var model = new CommonControllerModel();
            IRepository<Table> _iTable= new Repositories<Table>();
            IRepository<vwTablesAll> _ivwTablesAll = new Repositories<vwTablesAll>();
            try
            {
                var pTableList = from t in _iTable.All().OrderBy(x => x.TableName) select t;
                // Dim pTableList = From t In _iTable.Where(MachineKey,).SortBy("TableName")
                var lAllTables = _ivwTablesAll.All().Select(x => x.TABLE_NAME).ToList();
                pTableList = pTableList.Where(x => lAllTables.Contains(x.TableName));

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                var jsonObject = JsonConvert.SerializeObject(pTableList, Formatting.Indented, Setting);
                model.JsonString = jsonObject;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }

        public JsonResult GetTrackableTableList()
        {
            var model = new CommonControllerModel();
            IRepository<Table> _iTable =new Repositories<Table>();
            IRepository<vwTablesAll> _ivwTablesAll = new Repositories<vwTablesAll>();
            try
            {

                var pTableList = _iTable.Where(x => x.TrackingTable > 0 || x.Trackable == true);
                // Dim pTableList = From t In _iTable.Where(MachineKey,).SortBy("TableName")
                var lAllTables = _ivwTablesAll.All().Select(x => x.TABLE_NAME).ToList();
                pTableList = pTableList.Where(x => lAllTables.Contains(x.TableName));

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                var jsonObject = JsonConvert.SerializeObject(pTableList, Formatting.Indented, Setting);
                model.JsonString = jsonObject;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }

        public JsonResult GetColumnList(string pTableName, int type)
        {
            var model = new CommonControllerModel();
            IRepository<vwColumnsAll> _ivwColumnsAll = new Repositories<vwColumnsAll>();
            IRepository<Databas> _iDatabase = new Repositories<Databas>();
            IRepository<Table> _iTable = new Repositories<Table>();
            try
            {
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

                string sSQL;
                ADODB.Connection sAdoConn;
                ADODB.Recordset pColumns;
                var dataAdapter = new System.Data.OleDb.OleDbDataAdapter();
                var dataSet = new DataSet();

                if (type == 0)
                {
                    var pColumnLists = _ivwColumnsAll.All();
                    var pColumnList = pColumnLists.Where(x => x.TABLE_NAME.Trim().ToLower().Equals(pTableName.Trim().ToLower()));
                    pColumnList = pColumnList.Where(x => !x.COLUMN_NAME.StartsWith("%sl"));

                    // ''' added by hk 
                    // Dim pIDBManager As IDBManager = New DBManager(Keys.GetDBConnectionString)
                    // pIDBManager.ConnectionString = Keys.GetDBConnectionString
                    // Dim strin = String.Format("SELECT * from [vwColumnsAll] where TABLE_NAME = '{0}'; ", pTableName.Trim())
                    // Dim result = pIDBManager.ExecuteDataSet(CommandType.Text, strin)
                    // pIDBManager.Dispose()
                    // ''' added by hk 

                    var jsonObject = JsonConvert.SerializeObject(pColumnList, Formatting.Indented, Setting);
                    if (jsonObject.Length == 2)
                    {
                        var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableName.Trim().ToLower())).FirstOrDefault();
                        sAdoConn = DataServices.DBOpen(oTables, _iDatabase.All());
                        sSQL = "SELECT ROW_NUMBER() OVER (ORDER BY COLUMN_NAME) AS ID, COLUMN_NAME, TABLE_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + pTableName + "'";
                        string lError = "";
                        pColumns = DataServices.GetADORecordSet(ref sSQL, Cnxn: sAdoConn, ref lError);
                        dataAdapter.Fill(dataSet, pColumns, "Columns");
                        jsonObject = JsonConvert.SerializeObject(dataSet, Formatting.Indented, Setting);
                    }
                    model.JsonString = jsonObject;
                    return Json(model);
                }
                else
                {
                    _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
                    sSQL = pTableName.Split(new string[] { "WHERE" }, StringSplitOptions.None)[0];
                    var loutput = _IDBManager.ExecuteDataSet(System.Data.CommandType.Text, sSQL.Text());

                    var tColumnList = new List<string>();

                    for (int i = 0, loopTo = loutput.Tables[0].Columns.Count - 1; i <= loopTo; i++)
                    {
                        if (!loutput.Tables[0].Columns[i].ToString().StartsWith("%sl"))
                        {
                            tColumnList.Add(loutput.Tables[0].Columns[i].ToString());
                        }
                    }

                    var jsonObj = JsonConvert.SerializeObject(tColumnList, Formatting.Indented, Setting);
                    model.JsonString = jsonObj;
                    return Json(model);
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return Json(model);
            }

        }

        public JsonResult TruncateTrackingHistory(string sTableName = "", string sId = "")
        {
            var model = new CommonControllerModel();
            IRepository<TrackingHistory> _iTrackingHistory = new Repositories<TrackingHistory>();
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            try
            {
                // Dim trackingServiceObj As New TrackingServices()
                bool catchFlag;
                string KeysType = "";
                // Dim trackingServiceObj As New TrackingServices(_IDBManager, iTable, iDatabase, iScanList, iTabSet, iTableTab, iRelationship, iView, iSystem, _iTrackingHistory)
                catchFlag = TrackingServices.InnerTruncateTrackingHistory(_iSystem.All(), _iTrackingHistory, sTableName, sId, ref KeysType);
                if (catchFlag == true)
                {
                    if (KeysType == "s")
                    {
                        model.ErrorType = "s";
                        model.ErrorMessage = Languages.get_Translation("HistoryHasBeenTruncated");
                    }
                    else
                    {
                        model.ErrorType = "w";
                        model.ErrorMessage = Languages.get_Translation("NoMoreHistoryToTruncate");
                    }
                }

                else
                {
                    model.ErrorType = "w";
                    model.ErrorMessage = Languages.get_Translation("ForAnotherUse");
                }
            }

            catch (Exception)
            {
                model.ErrorType = "e";
                model.ErrorMessage = Keys.ErrorMessageJS();
            }
            finally
            {

            }
            return Json(model);
        }

        //Get all registered databases
        public IActionResult GetRegisteredDatabases()
        {
            IRepository<Databas> _iDatabase = new Repositories<Databas>();
            var pDatabase = _iDatabase.All();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            var jsonObject = JsonConvert.SerializeObject(pDatabase, Formatting.Indented, Setting);

            return Json(jsonObject);
        }
        public IActionResult GetCheckSession()
        {
            bool IsSessionExpired = false;
            try
            {
                if (httpContext.Session is not null)
                {
                    if (ContextService.GetSessionValueByKey("Passport", httpContext) is null)
                    {
                        IsSessionExpired = true;
                    }
                }
            }
            catch (Exception)
            {
            }
            return Json(new { isexpire = IsSessionExpired });
        }

        public new RedirectToRouteResult RedirectToAction(string action, string controller)
        {
            return RedirectToAction(action, controller);
        }
        public IActionResult SessionExpired()
        {
            if (httpContext.Session is not null)
            {
                if (ContextService.GetSessionValueByKey("passport", httpContext) is null)
                {
                    Response.Redirect("~/Login", true);
                }
            }
            return Redirect("~/Login");
        }

        //Check the permission for submodules.
        [HttpPost]
        public IActionResult CheckTabLevelAccessPermission([FromBody] ChkTabPermissionModelReq req)
        {
            bool bAccess = false;
            object jsonObject = null;
            var model = new CommonControllerModel();
            try
            {
                bAccess = passport.CheckPermission(req.pSecureObjectName, (Smead.Security.SecureObject.SecureObjectType)req.pSecureObjectType, (Smead.Security.Permissions.Permission)req.pPassportPermissions);

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                jsonObject = JsonConvert.SerializeObject(bAccess, Formatting.Indented, Setting);
                model.JsonString = (string)jsonObject;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }

        public IActionResult GetFontFamilies()
        {
            var model = new CommonControllerModel();
            try
            {
                FontFamily[] fontFamily;
                var installFonts = new InstalledFontCollection();
                fontFamily = installFonts.Families;

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                var jsonObject = JsonConvert.SerializeObject(fontFamily, Formatting.Indented, Setting);
                model.JsonString = jsonObject;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }
        private int SafeInt(string value)
        {
            try
            {
                return Conversions.ToInteger(value);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public FileResult DownloadAttachment([FromServices] IWebHostEnvironment webHostEnvironment, string filePath, string fileName, string docKey, string viewName)
        {
            byte[] fileContent = null;
            string fileExtension = string.Empty;
            string responseMessage = string.Empty;
            string mimeType = string.Empty;
            if (passport.CheckPermission(viewName, Smead.Security.SecureObject.SecureObjectType.View, Smead.Security.Permissions.Permission.Export))
            {
                var downloadModel = new FileDownloads();

                downloadModel.deleteTempFile = new List<string>();
                // check if file created in desktop
                // Dim params = Common.DecryptURLParameters(docKey.Substring(docKey.IndexOf("=") + 1))
                var @params = Common.DecryptURLParameters(docKey);
                // downloadModel.AttachNum = params.Split("&")(2).Split("=")(1)
                // downloadModel.AttachVer = attchVersion
                downloadModel.TableId = @params.Split("&")[0].Split("=")[1];
                downloadModel.TableName = @params.Split("&")[1].Split("=")[1];

                if (!string.IsNullOrEmpty(filePath))
                {
                    downloadModel._passport = passport;
                    downloadModel._serverPath = webHostEnvironment.ContentRootPath + @"wwwroot\Downloads\";
                    if (downloadModel.CheckIsDesktopFileBeforeDownload(downloadModel))
                    {
                        filePath = downloadModel.SaveTempPDFFileToDisk(Path.GetFileName(Common.DecryptURLParameters(System.Net.WebUtility.UrlDecode(filePath))));
                    }
                    else
                    {
                        filePath = Common.DecryptURLParameters(System.Net.WebUtility.UrlDecode(filePath));
                    }
                }
                try
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        fileExtension = Path.GetExtension(filePath);
                        //mimeType = MimeMapping.GetMimeMapping(filePath);

                        new FileExtensionContentTypeProvider().TryGetContentType(filePath, out mimeType);

                        // Read the bytes from file
                        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            using (var binaryReader = new BinaryReader(fs))
                            {
                                long byteLength = new FileInfo(filePath).Length;
                                fileContent = binaryReader.ReadBytes((int)byteLength);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("File does not exists.");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                foreach (var del in downloadModel.deleteTempFile)
                {
                    try
                    {
                        System.IO.File.Delete(del);
                    }
                    catch (Exception)
                    {
                        // do nothing, it's a temporary file anyway
                    }
                }

                return File(fileContent, mimeType, fileName + "." + (fileExtension.Contains(".")? fileExtension.Trim('.') : fileExtension));
            }
            else
            {
                return File(fileContent, mimeType, fileName + "."+ (fileExtension.Contains(".") ? fileExtension.Trim('.'): fileExtension));
            }
        }

        //This function are using for orphans.
        public FileResult DownloadAttachmentOrphan([FromServices] IWebHostEnvironment webHostEnvironment, string filePath, string fileName)
        {
            byte[] fileContent = null;
            string fileExtension = string.Empty;
            string responseMessage = string.Empty;
            string mimeType = string.Empty;
            var downloadModel = new FileDownloads();

            downloadModel.deleteTempFile = new List<string>();
            if (!string.IsNullOrEmpty(filePath))
            {
                downloadModel._passport = passport;
                downloadModel._serverPath = webHostEnvironment.ContentRootPath + @"wwwroot\Downloads\";
                filePath = Common.DecryptURLParameters(System.Net.WebUtility.UrlDecode(filePath));
            }
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    fileExtension = Path.GetExtension(filePath);
                    //mimeType = MimeMapping.GetMimeMapping(filePath);

                    new FileExtensionContentTypeProvider().TryGetContentType(filePath, out mimeType);

                    // Read the bytes from file
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var binaryReader = new BinaryReader(fs))
                        {
                            long byteLength = new FileInfo(filePath).Length;
                            fileContent = binaryReader.ReadBytes((int)byteLength);
                        }
                    }
                }
                else
                {
                    throw new Exception("File does not exists.");
                }
            }
            catch (Exception)
            {
                fileContent = null;
                return File(fileContent, " ", fileName + "." + (fileExtension.Contains('.') ? fileExtension.Trim('.') : fileExtension));
                //Throw New System.Exception(ex.Message);
            }

            foreach (var del in downloadModel.deleteTempFile)
            {
                try
                {
                    System.IO.File.Delete(del);
                }
                catch (Exception)
                {
                    // do nothing, it's a temporary file anyway
                }
            }
            var fn = Operators.ConcatenateObject(fileName + ".", fileExtension.Contains('.') ? fileExtension.Trim('.'): fileExtension);
            return File(fileContent, mimeType, fileName + "." + (fileExtension.Contains('.') ? fileExtension.Trim('.') : fileExtension));

        }

        public async Task<FlyoutModel> LoadAttachmentData(string docdata, int PageIndex, int PageSize, string viewName, IWebHostEnvironment webHostEnvironment)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();

                var @params = new string[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
                var data = Navigation.DecryptString(docdata).Split(Navigation.DelimiterText);
                int count = data.GetUpperBound(0);
                for (int i = 0; i <= 4; i++)
                {
                    if (i < count)
                        @params[i] = data[i];
                }
                string TableName = @params[3];
                var TableId = Navigation.PrepPad(TableName, @params[4], passport);
                bool IsStringId = true;

                var flyoutModel = new FlyoutModel();
                var lstFlyOutDetails = new List<FlyOutDetails>();
                flyoutModel.sPageSize = PageSize;
                flyoutModel.sPageIndex = PageIndex;

                var oTable = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(TableName.Trim().ToLower())).FirstOrDefault();
                if (oTable is null)
                    return default;

                int totalRowCount = 0;
                using (var dbManger = new DBManager(Keys.get_GetDBConnectionString()))
                {
                    dbManger.CreateParameters(6);
                    dbManger.AddParameters(0, "@tableId", TableId);
                    dbManger.AddParameters(1, "@tableName", TableName);
                    dbManger.AddParameters(2, "@PageNo", PageIndex);
                    dbManger.AddParameters(3, "@RecsPerPage", PageSize);
                    dbManger.AddParameters(4, "@UserId", @params[1]);
                    dbManger.AddParameters(5, "@totalRecCount", totalRowCount, ParameterDirection.Output, DbType.Int32);

                    IDbCommand dbcommand = dbManger.Command;
                    var ds = dbManger.ExecuteDataSet(System.Data.CommandType.StoredProcedure, "SP_RMS_GetPopupAttachments", ref dbcommand);
                    dbManger.CommandSet = dbcommand;

                    DataTable dt = ds.Tables[0];
                    totalRowCount = dbManger.Command.Parameters["@totalRecCount"].IntValue();
                    flyoutModel.totalRecCount = totalRowCount;
                    const string CachedFlyouts = "Flyouts";
                    string fullPath = string.Empty;
                    int pastAttachNumber = 0;

                    // Not in used
                    //var levelManger = ContextService.GetObjectFromJson<Smead.RecordsManagement.LevelManager>("LevelManager", httpContext);
                    //if (levelManger != null)
                    //{
                    //    Smead.RecordsManagement.Parameters oParams = levelManger.ActiveLevel.Parameters;
                    //    if (string.IsNullOrWhiteSpace(viewName))
                    //    {
                    //        viewName = levelManger.ActiveLevel.Parameters.ViewName;
                    //    }
                    //}
                    var call = new ApiCalls(_config);
                    flyoutModel.viewName = viewName;
                    foreach (DataRow oDataRow in dt.Rows)
                    {
                        bool isFormatValid = true;
                        string filePath = Path.Combine(Path.GetDirectoryName(oDataRow["FullPath"].Text()), CachedFlyouts, string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(oDataRow["FullPath"].Text()), Smead.RecordsManagement.Imaging.Export.Output.Format.Jpg.ToString().ToLower()));
                        bool validAttachment = System.IO.File.Exists(filePath);
                        if (!validAttachment)
                            fullPath = oDataRow["FullPath"].Text();
                        var flyOutDetails = new FlyOutDetails();

                        try
                        {
                            if (!System.IO.Directory.Exists(Path.GetDirectoryName(filePath)))
                                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        }
                        catch (Exception ex)
                        {
                            slimShared.SlimShared.logInformation(string.Format("Info {0} in CommonController.LoadAttachmentData", ex.Message));
                        }

                        // conditions comes here moti mashiah
                        var checkAnnotation = InitCheckAnnotation(TableId, TableName, oDataRow["AttachmentNumber"].IntValue(), oDataRow["TrackablesRecordVersion"].IntValue());
                        flyOutDetails.attchVersion = oDataRow["TrackablesRecordVersion"].IntValue();
                        if (checkAnnotation)
                        {
                            filePath = webHostEnvironment.ContentRootPath + "/resources/images/HasAnnotations.png";
                            flyOutDetails.sOrgFilePath = "disabled";
                        }
                        else
                        {
                            flyOutDetails.sOrgFilePath = Common.EncryptURLParameters(System.Net.WebUtility.UrlDecode(oDataRow["FullPath"].Text()));
                        }

                        // written for handling invalid format exception usually happening with kinda of xlsx files
                        FileStreamResult fileStreamResult = default;
                        try
                        {
                            fileStreamResult = await call.DocumentServices.APIGETStreamFlyOutFirstPage(filePath, fullPath, validAttachment);//CommonFunctions.GetSubImageFlyOut(filePath, fullPath, validAttachment);//
                            validAttachment = System.IO.File.Exists(filePath);
                        }
                        catch (Exception ex)
                        {
                            // heyreggie
                            if (("Invalid file format".ToLower()) == (ex.Message.ToLower() ))
                            {
                                var fileReplace = webHostEnvironment.ContentRootPath + "/resources/images/InvalidFormat.PNG";
                                validAttachment = System.IO.File.Exists(fileReplace);
                                fileStreamResult = await call.DocumentServices.APIGETStreamFlyOutFirstPage(filePath, fullPath, validAttachment);//GetSubImageFlyOut(fileReplace, fileReplace, validAttachment);
                                isFormatValid = false;
                            }
                        }

                        if (validAttachment == false)
                        {
                            flyOutDetails.sOrgFilePath = "disabled";
                        }

                        var buffer = new byte[(int)(0)];

                        if(fileStreamResult !=null)
                        {
                            long filesize = fileStreamResult.FileStream.Length;
                            buffer = new byte[(int)(filesize + 1)];
                            fileStreamResult.FileStream.Read(buffer, 0, (int)filesize);
                        }

                        flyOutDetails.sAttachmentName = string.IsNullOrEmpty(oDataRow["OrgFileName"].Text()) ? "Attachment " + oDataRow["AttachmentNumber"].Text() : oDataRow["OrgFileName"].Text();
                        flyOutDetails.sFlyoutImages = buffer;
                        flyOutDetails.sAttachId = oDataRow["TrackablesId"].IntValue();

                        string display = Navigation.GetItemName(TableName, TableId, passport);
                        var encryptURL = Common.EncryptURLParameters("id=" + Convert.ToString(TableId) + "&Table=" + Convert.ToString(TableName) + "&attachment=" + oDataRow["AttachmentNumber"].Text() + "&itemname=" + display);

                        if (httpContext.Request.Headers["User-Agent"].ToString().Contains("MSIE") || httpContext.Request.Headers["User-Agent"].ToString().Contains("Trident"))
                        {
                            string oIEURL = null;
                            string paramIE = Navigation.DecryptString(docdata).ToString();
                            docdata = Navigation.EncryptString(paramIE.Substring(0, paramIE.Length - 1) + oDataRow["AttachmentNumber"].Text());
                            flyOutDetails.sViewerLink = "undocked.aspx?v=" + System.Net.WebUtility.UrlEncode(docdata);
                            flyOutDetails.downloadEncryptAttachment = encryptURL;
                        }
                        else
                        {
                            // flyOutDetails.sViewerLink = "documentviewer.aspx?" + encryptURL
                            if (isFormatValid)
                            {
                                flyOutDetails.sViewerLink = "/DocumentViewer/Index?documentKey=" + encryptURL;
                            }
                            else
                            {
                                flyOutDetails.sViewerLink = "0";
                            }

                            flyOutDetails.downloadEncryptAttachment = encryptURL;
                        }
                        // FUS-6339 security fix for view permission in voliumn level - moti mashiah
                        string VolumName = Convert.ToString(oDataRow["VolumnName"].Text());
                        if (passport.CheckPermission(VolumName, Smead.Security.SecureObject.SecureObjectType.Volumes, Smead.Security.Permissions.Permission.View))
                        {
                            slimShared.SlimShared.logError(string.Format("Volume: {0} passed", VolumName));
                            lstFlyOutDetails.Add(flyOutDetails);
                        }
                        else
                        {
                            slimShared.SlimShared.logError(string.Format("Volume: {0} failed", VolumName));
                        }

                    }
                    flyoutModel.stringQuery = Common.EncryptURLParameters("id=" + Convert.ToString(TableId) + "&Table=" + Convert.ToString(TableName) + "&attachment=0");
                    flyoutModel.FlyOutDetails = lstFlyOutDetails;
                    if (passport.CheckPermission(viewName, Smead.Security.SecureObject.SecureObjectType.View, Smead.Security.Permissions.Permission.Export))
                    {
                        flyoutModel.downloadPermission = true;
                    }
                    else
                    {
                        flyoutModel.downloadPermission = false;
                    }

                    return flyoutModel;
                }
            }
            catch (Exception ex)
            {
                slimShared.SlimShared.logError(string.Format("Error {0} in CommonController.LoadAttachmentData", ex.Message));
                throw ex;
            }
        }

        public async Task<FlyoutModel> LoadAttachmentOrphanData(IWebHostEnvironment webHostEnvironment, string docdata, int PageIndex, int PageSize, string viewName, string filter)
        {
            try
            {
                var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", passport.ServerName, passport.DatabaseName), Attachments.OrphanName, "").ToString();

                var flyoutModel = new FlyoutModel();
                var lstFlyOutDetails = new List<FlyOutDetails>();
                flyoutModel.sPageSize = PageSize;
                flyoutModel.sPageIndex = PageIndex;
                int totalRowCount = 0;
                var orphanList = Attachments.GetAllOrphans(passport, filter, PageIndex, PageSize);

                flyoutModel.totalRecCount = orphanList.Rows.Count;

                const string CachedFlyouts = "Flyouts";
                string fullPath = string.Empty;
                int pastAttachNumber = 0;
                flyoutModel.viewName = "Orphans";
                var call = new ApiCalls(_config);
                foreach (DataRow oDataRow in orphanList.Rows)
                {
                    bool isFormatValid = true;
                    string filePath = Path.Combine(Path.GetDirectoryName(oDataRow["FullPath"].Text()), CachedFlyouts, string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(oDataRow["FullPath"].Text()),
                        Smead.RecordsManagement.Imaging.Export.Output.Format.Jpg.ToString().ToLower()));
                    bool validAttachment = System.IO.File.Exists(filePath);
                    if (!validAttachment)
                        fullPath = oDataRow["FullPath"].Text();
                    var flyOutDetails = new FlyOutDetails();

                    try
                    {
                        if (!System.IO.Directory.Exists(Path.GetDirectoryName(filePath)))
                            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }
                    catch (Exception ex)
                    {
                        slimShared.SlimShared.logInformation(string.Format("Info {0} in CommonController.LoadAttachmentData", ex.Message));
                    }

                    flyOutDetails.attchVersion = oDataRow["TrackablesRecordVersion"].IntValue();
                    flyOutDetails.recordType = oDataRow[ "RecordType"].IntValue();
                    flyOutDetails.sOrgFilePath = Common.EncryptURLParameters(WebUtility.UrlDecode(oDataRow["FullPath"].Text()));

                    // written for handling invalid format exception usually happening with kinda of xlsx files
                    FileStreamResult fileStreamResult = default;
                    try
                    {
                        fileStreamResult = await call.DocumentServices.APIGETStreamFlyOutFirstPage(filePath, fullPath, validAttachment);//GetSubImageFlyOut(filePath, fullPath, validAttachment);
                    }
                    catch (Exception)
                    {
                        // If "Invalid file format".ToLower() = ex.Message.ToLower() Then
                        // Dim fileReplace = HttpContext.Server.MapPath("~/resources/images/InvalidFormat.PNG")
                        // fileStreamResult = GetSubImageFlyOut(fileReplace, fileReplace, validAttachment)
                        // isFormatValid = False
                        // End If
                        var fileReplace = webHostEnvironment.ContentRootPath + "/wwwroot" + "/resources/images/InvalidFormat.PNG";
                        fileStreamResult = await call.DocumentServices.APIGETStreamFlyOutFirstPage(filePath, fullPath, validAttachment);//GetSubImageFlyOut(fileReplace, fileReplace, validAttachment);
                        isFormatValid = false;
                    }

                    long filesize = fileStreamResult.FileStream.Length;
                    var buffer = new byte[(int)(filesize + 1)];
                    fileStreamResult.FileStream.Read(buffer, 0, (int)filesize);

                    flyOutDetails.sAttachmentName = string.IsNullOrEmpty(oDataRow["OrgFileName"].Text()) ? Path.GetFileName(oDataRow["orgfullpath"].Text()) : oDataRow["OrgFileName"].Text();
                    flyOutDetails.sFlyoutImages = buffer;
                    flyOutDetails.sAttachId = oDataRow["TrackablesId"].IntValue();
                    // flyOutDetails.scanDateTime = IIf(String.IsNullOrEmpty(oDataRow("ScanDateTime")), "-", oDataRow("ScanDateTime").ToString())
                    flyOutDetails.scanDateTime = object.ReferenceEquals(oDataRow["ScanDateTime"].Text(), DBNull.Value) ? string.Empty : oDataRow["ScanDateTime"].Text();

                    var encryptURL = Common.EncryptURLParameters("pointerId=" + oDataRow["PointerId"].Text() + "&Id=" + oDataRow["TrackablesId"] + "&Rtype=" + Convert.ToString(oDataRow["RecordType"]) + "&itempath=" + oDataRow["FullPath"].Text() + "&itemname=" + oDataRow["OrgFileName"].Text());

                    flyOutDetails.sViewerLink = "/DocumentViewer/Orphan?documentKey=" + encryptURL;
                    flyOutDetails.downloadEncryptAttachment = "";
                    flyOutDetails.sRowNum = oDataRow["RowNum"].IntValue();


                    // FUS-6339 security fix for view permission in voliumn level - moti mashiah
                    lstFlyOutDetails.Add(flyOutDetails);

                }
                flyoutModel.stringQuery = "";
                flyoutModel.FlyOutDetails = lstFlyOutDetails;
                flyoutModel.downloadPermission = true;
               
                return flyoutModel;
            }
            catch (Exception ex)
            {

                slimShared.SlimShared.logError(string.Format("Error {0} in CommonController.LoadAttachmentOrphanData", ex.Message));
                throw;
            }
        }
        //check for annotation
        private bool InitCheckAnnotation(string tableid, string tableName, int AttachmentNumber, int VarsionNumber)
        {
            using (var cmd = new SqlCommand("SP_RMS_GetFilesPaths", passport.Connection()))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tableId", tableid);
                cmd.Parameters.AddWithValue("@tableName", tableName);
                cmd.Parameters.AddWithValue("@AttachmentNumber", AttachmentNumber);
                cmd.Parameters.AddWithValue("@RecordVersion", VarsionNumber);

                var adp = new SqlDataAdapter(cmd);
                var dTable = new DataTable();
                var datat = adp.Fill(dTable);
                foreach (DataRow row in dTable.Rows)
                {
                    var getpath = row["FullPath"];
                    int pointerid = Convert.ToInt32(row["pointerId"]);
                    var cAnnotation = this.CheckAnnotation(pointerid);

                    // check for annotation;
                    if (cAnnotation)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        private bool CheckAnnotation(int pointerid)
        {
            var lstImages = new List<int>();
            string sql = "select * from Annotations a where a.[Table] = 'REDLINE' and a.TableId = '010001' + RIGHT('000000000000000000000000' + CAST(@pointerid AS VARCHAR), 24)";
            using (SqlConnection conn = passport.Connection())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@pointerid", pointerid);
                    return Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
        }

        public PartialViewResult LoadFlyoutPartial([FromServices] IWebHostEnvironment webHostEnvironment, string docdata, bool isMVC)
        {
            // if the call comes from the new MVC model create encryp and pass the variable
            string viewName = string.Empty;
            if (isMVC)
            {
                string tblName = docdata.Split(",")[0].ToString();
                string pkidValue = docdata.Split(",")[1].ToString();
                viewName = docdata.Split(",")[2].ToString();
                string tableName = tblName + Smead.RecordsManagement.Navigation.DelimiterText;
                string tableid = "***pkey***" + Smead.RecordsManagement.Navigation.DelimiterText;
                string userid = passport.UserId.ToString() + Smead.RecordsManagement.Navigation.DelimiterText;
                string database = string.Format(@"{0}\{1}", ContextService.GetSessionValueByKey("Server", httpContext).ToString(), ContextService.GetSessionValueByKey("DatabaseName", httpContext).ToString()) + Smead.RecordsManagement.Navigation.DelimiterText;
                string attachmentNumber = "0";
                var pass = "***ticket***" + Smead.RecordsManagement.Navigation.DelimiterText + userid + database + tableName + tableid + attachmentNumber;
                var rowPass = pass.Replace("***pkey***", pkidValue).Replace("%%%pkey%%%", pkidValue).Replace("***ticket***", passport.get_CreateTicket(string.Format(@"{0}\{1}", ContextService.GetSessionValueByKey("Server", httpContext).ToString(), ContextService.GetSessionValueByKey("DatabaseName", httpContext).ToString()), tblName, "1"));
                var encryptedRowPass = Smead.RecordsManagement.Navigation.EncryptString(rowPass);
                docdata = encryptedRowPass;
            }

            var flyoutModel = LoadAttachmentData(docdata, 1, 6, viewName, webHostEnvironment);
            return PartialView("_FlyoutPartial", flyoutModel);
        }

        public IActionResult LazyLoadPopupAttachments([FromServices] IWebHostEnvironment webHostEnvironment, string docdata, short PageIndex, short PageSize, string viewName, bool isMVC)
        {
            // if the call comes from the new MVC model create encryp and pass the variable
            var model = new CommonControllerModel();
            try
            {
                if (isMVC)
                {
                    string tblName = docdata.Split(",")[0].ToString();
                    string pkidValue = docdata.Split(",")[1].ToString();
                    viewName = docdata.Split(",")[2].ToString();
                    string tableName = tblName + Smead.RecordsManagement.Navigation.DelimiterText;
                    string tableid = "***pkey***" + Smead.RecordsManagement.Navigation.DelimiterText;
                    string userid = passport.UserId.ToString() + Smead.RecordsManagement.Navigation.DelimiterText;
                    string database = string.Format(@"{0}\{1}", ContextService.GetSessionValueByKey("Server", httpContext).ToString(), ContextService.GetSessionValueByKey("DatabaseName", httpContext).ToString()) + Smead.RecordsManagement.Navigation.DelimiterText;
                    string attachmentNumber = "0";
                    var pass = "***ticket***" + Smead.RecordsManagement.Navigation.DelimiterText + userid + database + tableName + tableid + attachmentNumber;
                    var rowPass = pass.Replace("***pkey***", pkidValue).Replace("%%%pkey%%%", pkidValue).Replace("***ticket***", passport.get_CreateTicket(string.Format(@"{0}\{1}", ContextService.GetSessionValueByKey("Server", httpContext).ToString(), ContextService.GetSessionValueByKey("DatabaseName", httpContext).ToString()), tblName, "1"));
                    var encryptedRowPass = Smead.RecordsManagement.Navigation.EncryptString(rowPass);
                    docdata = encryptedRowPass;
                }

                var flyoutModel = LoadAttachmentData(docdata, PageIndex, PageSize, viewName, webHostEnvironment);
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                var jsonObject = JsonConvert.SerializeObject(flyoutModel, Formatting.Indented, Setting);
                model.JsonString = jsonObject;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }
        public async Task<List<FileStreamResult>> GetAllImageFlyOut(string docdata, string oTableId, string oTableName,  List<string> OrgFilePath, List<string> AttachmentName, List<short> AttchUniqueId)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<Databas> _iDatabase = new Repositories<Databas>();

                var @params = new string[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
                var data = Smead.RecordsManagement.Navigation.DecryptString(docdata).Split(Smead.RecordsManagement.Navigation.DelimiterText);
                int count = data.GetUpperBound(0);
                var oList = new List<FileStreamResult>();
                for (int i = 0; i <= 4; i++)
                {
                    if (i < count)
                        @params[i] = data[i];
                }
                string tableName = @params[3];
                string tableId = @params[4];
                oTableName = @params[3];
                oTableId = @params[4];
                var oTable = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                if (oTable is null)
                    return null;

                var idFieldName = oTable.IdFieldName;
                var csADOConn = new ADODB.Connection();
                csADOConn = DataServices.DBOpen(_iDatabase.All().Where(x => x.DBName.Trim().ToLower().Equals(oTable.DBName.Trim().ToLower())).FirstOrDefault());

                if (csADOConn is not null)
                {
                    if (!DataServices.IdFieldIsString(csADOConn, tableName, idFieldName))
                        tableId = tableId.PadLeft(30, '0');
                    csADOConn.Close();
                }

                using (var dbManger = new DBManager(Keys.get_GetDBConnectionString()))
                {
                    dbManger.CreateParameters(2);
                    dbManger.AddParameters(0, "@tableId", tableId);
                    dbManger.AddParameters(1, "@tableName", tableName);
                    var ds = dbManger.ExecuteDataSet(System.Data.CommandType.StoredProcedure, "SP_RMS_GetAttchmentName");
                    DataTable dt = ds.Tables[0];
                    dt = dt.Select("", "AttachmentNumber ASC").CopyToDataTable();
                    const string CachedFlyouts = "Flyouts";
                    string fullPath = string.Empty;
                    int pastAttachNumber = 0;
                    var call = new ApiCalls(_config);
                    foreach (DataRow oDataRow in dt.Rows)
                    {
                        if (!pastAttachNumber.Equals(oDataRow["AttachmentNumber"]))
                        {
                            string filePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(oDataRow["FullPath"].Text()), CachedFlyouts, string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(oDataRow["FullPath"].ToString()), Output.Format.Jpg.ToString().ToLower()));
                            bool validAttachment = System.IO.File.Exists(filePath);
                            if (!validAttachment)
                            {
                                fullPath = oDataRow["FullPath"].Text();
                            }
                            oList.Add(await call.DocumentServices.APIGETStreamFlyOutFirstPage(filePath, fullPath, validAttachment));//GetSubImageFlyOut(filePath, fullPath, validAttachment));
                            AttchUniqueId.Add((short)oDataRow["TrackablesId"].IntValue());
                            OrgFilePath.Add(oDataRow["FullPath"].Text());
                            AttachmentName.Add(string.IsNullOrEmpty(oDataRow["OrgFileName"].Text()) ? "Attachment " + oDataRow["AttachmentNumber"].Text() : oDataRow["OrgFileName"].Text());
                            pastAttachNumber = oDataRow["AttachmentNumber"].IntValue();
                        }
                    }
                    return oList;
                }
            }
            catch (Exception ex)
            {
                slimShared.SlimShared.logError(string.Format("Error {0} in CommonController.GetAllImageFlyOut", ex.Message));
                throw;
            }
        }
        //public FileStreamResult GetSubImageFlyOut(string filePath, string fullPath, bool validAttachment)
        //{
        //    try
        //    {
        //        bool stampWithMessage = false;
        //        Bitmap bmp = Smead.RecordsManagement.Imaging.Export.Output.NotAvailableImage();

        //        if (!validAttachment)
        //        {
        //            if (!string.IsNullOrEmpty(fullPath) & System.IO.File.Exists(fullPath))
        //            {
        //                // 2. realize redactions (cannot be done)

        //                Smead.RecordsManagement.Imaging.Export.Output.Format format = (Smead.RecordsManagement.Imaging.Export.Output.Format)Output.Format.Jpg;

        //                using (var codec = new RasterCodecs())
        //                {
        //                    using (RasterImage img = codec.Load(fullPath, 1))
        //                    {

        //                        var rc = new Rectangle(0, 0, Attachment.FlyoutSize.Width, Attachment.FlyoutSize.Height);

        //                        if (img.BitsPerPixel <= 2)
        //                            format = Output.Format.Tif;
        //                        if (img.Width < Attachment.FlyoutSize.Width || img.Height < Attachment.FlyoutSize.Height)
        //                        {
        //                            rc.Width = img.Width;
        //                            rc.Height = img.Height;
        //                        }

        //                        rc = RasterImageList.GetFixedAspectRatioImageRectangle(img.Width, img.Height, rc);

        //                        var command = new Leadtools.ImageProcessing.ResizeCommand();
        //                        command.Flags = RasterSizeFlags.None;
        //                        command.DestinationImage = new RasterImage(RasterMemoryFlags.Conventional, rc.Width, rc.Height, img.BitsPerPixel, img.Order, img.ViewPerspective, img.GetPalette(), IntPtr.Zero, 0);
        //                        command.Run(img);

        //                        codec.Save(command.DestinationImage, filePath, TranslateToLeadToolsFormat(format, img.BitsPerPixel), Attachment.ConvertBitsPerPixel(format, img.BitsPerPixel));
        //                        using (var stream = new MemoryStream())
        //                        {
        //                            codec.Save(img, stream, RasterImageFormat.Jpeg, 8);
        //                            return new FileStreamResult(new MemoryStream(stream.ToArray()), "image/jpg");
        //                        }
        //                        //ImageIncompatibleReason reason = RasterImageConverter.TestCompatible(command.DestinationImage, true);
        //                        //System.Drawing.Imaging.PixelFormat pf = RasterImageConverter.GetNearestPixelFormat(command.DestinationImage);
        //                        //if (reason != ImageIncompatibleReason.Compatible)
        //                        //    RasterImageConverter.MakeCompatible(command.DestinationImage, pf, true);

        //                        //using (Bitmap bmp1 = (Bitmap)RasterImageConverter.ConvertToImage(command.DestinationImage, ConvertToImageOptions.None))
        //                        //{
        //                        //    using (var stream = new MemoryStream())
        //                        //    {
        //                        //        bmp1.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        //                        //        return new FileStreamResult(new MemoryStream(stream.ToArray()), "image/jpg");
        //                        //    }
        //                        //}
        //                    }
        //                }
        //            }

        //            else
        //            {
        //                if (!string.IsNullOrEmpty(filePath))
        //                {
        //                    if (filePath.ToLower().StartsWith(Exceptions.FileNotFound.ToLower()))
        //                    {
        //                        bmp = Output.Invalid();
        //                        if (stampWithMessage)
        //                            Attachments.DrawTextOnErrorImage(bmp, Exceptions.FileNotFound);
        //                    }
        //                    else
        //                    {
        //                        if (bmp is null)
        //                            bmp = Output.NotAvailableImage();
        //                        if (stampWithMessage)
        //                            Attachments.DrawTextOnErrorImage(bmp, filePath);
        //                    }
        //                }
        //                else
        //                {
        //                    if (bmp is null)
        //                        bmp = Output.NotAvailableImage();
        //                    if (stampWithMessage)
        //                        Attachments.DrawTextOnErrorImage(bmp, "File Not Found");
        //                }

        //                using (var stream = new MemoryStream())
        //                {
        //                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        //                    return new FileStreamResult(new MemoryStream(stream.ToArray()), "image/jpg");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            using (var codec = new RasterCodecs())
        //            {
        //                using (RasterImage img = codec.Load(filePath, 1))
        //                {
        //                    using (var stream = new MemoryStream())
        //                    {
        //                        codec.Save(img, stream, RasterImageFormat.Jpeg, 8);
        //                        return new FileStreamResult(new MemoryStream(stream.ToArray()), "image/jpg");
        //                    }
        //                    //ImageIncompatibleReason reason = RasterImageConverter.TestCompatible(img, true);
        //                    //System.Drawing.Imaging.PixelFormat pf = RasterImageConverter.GetNearestPixelFormat(img);
        //                    //if (reason != ImageIncompatibleReason.Compatible)
        //                    //    RasterImageConverter.MakeCompatible(img, pf, true);

        //                    //using (Bitmap bmp1 = (Bitmap)RasterImageConverter.ConvertToImage(img, ConvertToImageOptions.None))
        //                    //{
        //                    //    using (var stream = new MemoryStream())
        //                    //    {
        //                    //        bmp1.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        //                    //        return new FileStreamResult(new MemoryStream(stream.ToArray()), "image/jpg");
        //                    //    }
        //                    //}
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //}

        //public FileStreamResult GetImageFlyOut(string docdata)
        //{
        //    bool stampWithMessage = false;
        //    string message = string.Empty;
        //    bool validAttachment = false;
        //    string fullPath = string.Empty;
        //    Bitmap bmp = Output.NotAvailableImage();

        //    var @params = new string[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
        //    var data = Smead.RecordsManagement.Navigation.DecryptString(docdata).Split(Smead.RecordsManagement.Navigation.DelimiterText);
        //    int count = data.GetUpperBound(0);

        //    for (int i = 0; i <= 4; i++)
        //    {
        //        if (i < count)
        //            @params[i] = data[i];
        //    }
        //    try
        //    {
        //        string filePath = Attachments.GetImageFlyout(@params[0].Replace("'", string.Empty).Replace("\"", string.Empty), SafeInt(@params[1]), @params[2], @params[3], @params[4], ref validAttachment, ref fullPath);

        //        if (!validAttachment)
        //        {
        //            if (!string.IsNullOrEmpty(fullPath))
        //            {
        //                // 2. realize redactions (cannot be done)

        //                Output.Format format = Output.Format.Jpg;

        //                using (var codec = new RasterCodecs())
        //                {
        //                    using (RasterImage img = codec.Load(fullPath, 1))
        //                    {

        //                        var rc = new System.Drawing.Rectangle(0, 0, Attachment.FlyoutSize.Width, Attachment.FlyoutSize.Height);

        //                        if (img.BitsPerPixel <= 2)
        //                            format = Output.Format.Tif;
        //                        if (img.Width < Attachment.FlyoutSize.Width || img.Height < Attachment.FlyoutSize.Height)
        //                        {
        //                            rc.Width = img.Width;
        //                            rc.Height = img.Height;
        //                        }

        //                        rc = RasterImageList.GetFixedAspectRatioImageRectangle(img.Width, img.Height, rc);

        //                        var command = new Leadtools.ImageProcessing.ResizeCommand();
        //                        command.Flags = RasterSizeFlags.None;
        //                        command.DestinationImage = new RasterImage(RasterMemoryFlags.Conventional, rc.Width, rc.Height, img.BitsPerPixel, img.Order, img.ViewPerspective, img.GetPalette(), IntPtr.Zero, 0);
        //                        command.Run(img);

        //                        codec.Save(command.DestinationImage, filePath, TranslateToLeadToolsFormat(format, img.BitsPerPixel), Attachment.ConvertBitsPerPixel(format, img.BitsPerPixel));

        //                        using (var stream = new MemoryStream())
        //                        {
        //                            codec.Save(img, stream, RasterImageFormat.Jpeg, 8);
        //                            return new FileStreamResult(new MemoryStream(stream.ToArray()), "image/jpg");
        //                        }

        //                        //ImageIncompatibleReason reason = RasterImageConverter.TestCompatible(command.DestinationImage, true);
        //                        //System.Drawing.Imaging.PixelFormat pf = RasterImageConverter.GetNearestPixelFormat(command.DestinationImage);
        //                        //if (reason != ImageIncompatibleReason.Compatible)
        //                        //    RasterImageConverter.MakeCompatible(command.DestinationImage, pf, true);

        //                        //using (Bitmap bmp1 = (Bitmap)RasterImageConverter.ConvertToImage(command.DestinationImage, ConvertToImageOptions.None))
        //                        //{
        //                        //    using (var stream = new MemoryStream())
        //                        //    {
        //                        //        bmp1.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        //                        //        return new FileStreamResult(new MemoryStream(stream.ToArray()), "image/jpg");
        //                        //    }
        //                        //}
        //                    }
        //                }
        //            }

        //            else
        //            {
        //                if (!string.IsNullOrEmpty(filePath))
        //                {
        //                    if (filePath.ToLower().StartsWith(Exceptions.FileNotFound.ToLower()))
        //                    {
        //                        bmp = Output.Invalid();
        //                        if (stampWithMessage)
        //                            Attachments.DrawTextOnErrorImage(bmp, Exceptions.FileNotFound);
        //                    }
        //                    else
        //                    {
        //                        if (bmp is null)
        //                            bmp = Output.NotAvailableImage();
        //                        if (stampWithMessage)
        //                            Attachments.DrawTextOnErrorImage(bmp, filePath);
        //                    }
        //                }
        //                else
        //                {
        //                    if (bmp is null)
        //                        bmp = Output.NotAvailableImage();
        //                    if (stampWithMessage)
        //                        Attachments.DrawTextOnErrorImage(bmp, "File Not Found");
        //                }

        //                using (var stream = new MemoryStream())
        //                {
        //                    bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        //                    return new FileStreamResult(new MemoryStream(stream.ToArray()), "image/jpg");
        //                }
        //            }
        //        }
        //        else
        //        {
        //            using (var codec = new RasterCodecs())
        //            {
        //                using (RasterImage img = codec.Load(filePath, 1))
        //                {
        //                    using (var stream = new MemoryStream())
        //                    {
        //                        codec.Save(img, stream, RasterImageFormat.Jpeg, 8);
        //                        return new FileStreamResult(new MemoryStream(stream.ToArray()), "image/jpg");
        //                    }
        //                    //ImageIncompatibleReason reason = RasterImageConverter.TestCompatible(img, true);
        //                    //System.Drawing.Imaging.PixelFormat pf = RasterImageConverter.GetNearestPixelFormat(img);
        //                    //if (reason != ImageIncompatibleReason.Compatible)
        //                    //    RasterImageConverter.MakeCompatible(img, pf, true);

        //                    //using (Bitmap bmp1 = (Bitmap)RasterImageConverter.ConvertToImage(img, ConvertToImageOptions.None))
        //                    //{
        //                    //    using (var stream = new MemoryStream())
        //                    //    {
        //                    //        bmp1.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        //                    //        return new FileStreamResult(new MemoryStream(stream.ToArray()), "image/jpg");
        //                    //    }
        //                    //}
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        return default;
        //    }
        //}

        [HttpPost]
        public JsonResult CheckAttachmentMaxSize([FromBody] PopupdocViewerReqModel req)
        {
            var pop = new PopupdocViewerModel();
            try
            {
                if (CheckaddNewAttachmentPermission(req.paramss.tabName, pop))
                {
                    // check if file over size
                    IRepository<Setting> s_Setting = new Repositories<Setting>();
                    var expMaxSize = s_Setting.Where(x=> x.Section == "DragAndDropAttachment" && x.Item == "MaxSize").FirstOrDefault()?.ItemValue;
                    if (!CheckFilesize(req.paramss.filesizeMB) && !string.IsNullOrEmpty(req.paramss.tableId) | req.paramss.tabName == "Orphans")
                    {
                        pop.checkConditions = "maxsize";
                        pop.WarringMsg = string.Format(Languages.get_Translation("msgDragAndDropAttachmentWarringMsg"), expMaxSize);
                    }
                    else
                    {
                        pop.checkConditions = "success";
                    }
                }
                else
                {
                    pop.checkConditions = "permission";
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, pop, Convert.ToInt32(req.paramss.viewId??"0"), passport.DatabaseName);
            }

            return Json(pop);
        }

        //add new attachment start from here.
        //moti here
        [HttpPost]
        public async Task<JsonResult> AddNewAttachment([FromServices] IWebHostEnvironment webHostEnvironment, PopupdocViewerModel.popupdocViewerUI @params)
        {
            var pop = new PopupdocViewerModel();
            pop.checkConditions = "success";
            // check permission
            var files = httpContext.Request.Form.Files;
            try
            {
                if (CheckaddNewAttachmentPermission(@params.tabName, pop))
                {
                    // check if file over size
                    IRepository<Setting> s_Setting = new RepositoryVB.Repositories<Setting>();
                    var expMaxSize = s_Setting.Where(x=> x.Section == "DragAndDropAttachment" && x.Item == "MaxSize").FirstOrDefault()?.ItemValue;
                    if (!CheckFilesize(files) && !string.IsNullOrEmpty(@params.tableId) | @params.tabName == "Orphans")
                    {
                        pop.checkConditions = "maxsize";
                        pop.WarringMsg = string.Format(Languages.get_Translation("msgDragAndDropAttachmentWarringMsg"), expMaxSize);
                    }
                    else if (string.IsNullOrEmpty(@params.tableId))
                    {
                        SaveOrphanAttachment(@params, await GetTemporaryOrphanFilePaths(files, webHostEnvironment));
                    }
                    else
                    {
                        SaveNewAttachment(@params, await GetTemporaryFilePaths(files, webHostEnvironment));

                    }
                }
                else
                {
                    pop.checkConditions = "permission";
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, pop, Convert.ToInt32(@params.viewId??"0"), passport.DatabaseName);
                pop.checkConditions = "error";
                pop.Msg = ex.Message;
            }

            return Json(pop);
        }
        private async void SaveOrphanAttachment(PopupdocViewerModel.popupdocViewerUI @params, List<FileInfoModel> filePathList)
        {
            var lst = new List<string>();
            foreach (var file in filePathList)
            {
                lst.Add(file.Path);
            }
            var call = new ApiCalls(_config);
            var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", passport.ServerName, passport.DatabaseName), Smead.RecordsManagement.Imaging.Attachments.OrphanName, "").ToString();
            int counter = 0;
            var filesinfo = await call.DocumentServices.GetCodecInfoFromFileList(lst);
            foreach (var item in filesinfo)
            {
                var OrgFileName = filePathList[counter].OrgFileName;
                //var info = Common.GetCodecInfoFromFile(path.Path, Path.GetExtension(path.Path));
                Smead.RecordsManagement.Imaging.Attachments.AddAnOrphan(ticket, passport.UserId, string.Format(@"{0}\{1}", passport.ServerName, passport.DatabaseName), "", OrgFileName, item.filepath, Path.GetExtension(item.filepath), new Size(0, 0), true, item.Info);
                counter++;
            }
        }
        private bool CheckFilesize(string filesSizes)
        {
            IRepository<Setting> s_Setting = new RepositoryVB.Repositories<Setting>();
            var expMaxSize = s_Setting.Where(x=> x.Section == "DragAndDropAttachment" && x.Item == "MaxSize").FirstOrDefault().ItemValue;
            var filearr = filesSizes.Split(",");
            for (int i = 0, loopTo = filearr.Count() - 1; i <= loopTo; i++)
            {
                if (Math.Round(Convert.ToDecimal(expMaxSize)) < Math.Round(Convert.ToDecimal(Convert.ToInt64(filearr[i]) / 1024d / 1024d)))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CheckFilesize(IFormFileCollection files)
        {
            IRepository<Setting> s_Setting = new RepositoryVB.Repositories<Setting>();
            var expMaxSize = s_Setting.Where(x=> x.Section == "DragAndDropAttachment" && x.Item == "MaxSize").FirstOrDefault()?.ItemValue;

            foreach(IFormFile item in files)
            {
                if (Math.Round(Convert.ToDecimal(expMaxSize)) < Math.Round(Convert.ToDecimal(item.Length / 1024 / 1024)))
                {
                    return false;
                }
            }
            
            return true;
        }

        [HttpPost]
        private async void SaveNewAttachment(PopupdocViewerModel.popupdocViewerUI param, List<string> filePathList)
        {
            var call = new ApiCalls(_config);
            var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", passport.ServerName, passport.DatabaseName), param.tabName, param.tableId).ToString();
            // Dim oDefaultOutputSetting = _iSystem.All.FirstOrDefault.DefaultOutputSettingsId
            string oDefaultOutputSetting = string.Empty;
            var filesinfo = await call.DocumentServices.GetCodecInfoFromFileList(filePathList);
            foreach (var item in filesinfo)
            {
                //var info = Common.GetCodecInfoFromFile(path, Path.GetExtension(path));
                if (item.Info is null)
                {
                    Attachments.AddAnAttachment(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", passport.ServerName, passport.DatabaseName), param.tabName, param.tableId, 0, oDefaultOutputSetting, item.filepath, item.filepath, Path.GetExtension(item.filepath), false, param.name, false, 1, 0, 0, 0);
                }
                else
                {
                    _ = Attachments.AddAnAttachment(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", passport.ServerName, passport.DatabaseName), param.tabName, param.tableId, 0, oDefaultOutputSetting, item.filepath, item.filepath, Path.GetExtension(item.filepath), false, param.name, true, item.Info.TotalPages, item.Info.Height, item.Info.Width, item.Info.SizeDisk);
                }
            }
        }
        public async Task<List<FileInfoModel>> GetTemporaryOrphanFilePaths(IFormFileCollection files, IWebHostEnvironment webHostEnvironment)
        {
            var lst = new List<FileInfoModel>();
            
            foreach (var item in files)
            {
                if (item.Length>0)
                {
                    string FileName = Guid.NewGuid().ToString();
                    string orgFileName = Path.GetFileNameWithoutExtension(item.FileName);
                    FileName = FileName + Path.GetExtension(item.FileName);
                    using (Stream fileStream = new FileStream(webHostEnvironment.ContentRootPath + "wwwroot/Downloads/" + FileName, FileMode.Create))
                    {
                        await item.CopyToAsync(fileStream);
                    }
                    var filePath = webHostEnvironment.ContentRootPath + "wwwroot/Downloads/" + FileName;
                    var obj = new FileInfoModel();
                    obj.Path = filePath;
                    obj.OrgFileName = orgFileName;
                    lst.Add(obj);
                }
            }
            
            return lst;
        }
        public async Task<List<string>>  GetTemporaryFilePaths(IFormFileCollection files, IWebHostEnvironment webHostEnvironment)
        {
            var lst = new List<string>();
            var model = new ErrorBaseModel();

            foreach(var item in files)
            {
                if (item.Length > 0)
                {
                    string FileName = Guid.NewGuid().ToString();
                    FileName = FileName + Path.GetExtension(item.FileName);
                    using (Stream fileStream = new FileStream(webHostEnvironment.ContentRootPath +@"wwwroot\Downloads\" + FileName, FileMode.Create))
                    {
                        await item.CopyToAsync(fileStream);
                    }
                    var filePath = webHostEnvironment.ContentRootPath + @"wwwroot\Downloads\" + FileName;
                    try
                    {
                        // check If file Is corrupted Or has any error if yes then don't add it to the list
                        //CheckFileHealth(filePath);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Invalid file format")
                        {
                            model.isError = false;
                        }
                        else
                        {
                            DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                        }
                    }
                    if (!model.isError)
                    {
                        lst.Add(filePath);
                    }
                }
            }
           
            return lst;
        }

        //private void CheckFileHealth(string filepath)
        //{
        //    using (var codec = new RasterCodecs())
        //    {
        //        using (RasterImage img = codec.Load(filepath, 1))
        //        {
        //        }

        //        var x = "";
        //    }
        //}
        public bool CheckaddNewAttachmentPermission(string tableName, PopupdocViewerModel pop)
        {
            var _iSystem = new RepositoryVB.Repositories<TabFusionRMS.Models.System>();
            var _iOutputSetting = new RepositoryVB.Repositories<OutputSetting>();
            var _iVolume = new RepositoryVB.Repositories<Volume>();
            var _iSystemAddress = new RepositoryVB.Repositories<SystemAddress>();
            var oSystem = _iSystem.All().FirstOrDefault();
            var oOutputSetting = _iOutputSetting.All().Where(x => x.Id.Trim().ToLower().Equals(oSystem.DefaultOutputSettingsId.Trim().ToLower())).FirstOrDefault();
            var oVolume = _iVolume.All().Where(x => x.Id == oOutputSetting.VolumesId).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(tableName) || string.Compare(tableName, Smead.RecordsManagement.Imaging.Attachments.OrphanName) == 0)
                return true;
            // Return Passport.CheckPermission(tableName, Smead.Security.SecureObject.SecureObjectType.Orphans, Smead.Security.Permissions.Permission.View)
            if (!passport.CheckPermission(tableName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Add))
            {
                pop.errorNumber = 0;
                return false;
            }
            if (!passport.CheckPermission(oVolume.Name, Smead.Security.SecureObject.SecureObjectType.Volumes, Smead.Security.Permissions.Permission.Add))
            {
                pop.errorNumber = 1;
                return false;
            }

            return true;
        }
        //public static RasterImageFormat TranslateToLeadToolsFormat(Output.Format format, int bitsPerPixel)
        //{
        //    switch (format)
        //    {
        //        case var @case when @case == Output.Format.Bmp:
        //            {
        //                return RasterImageFormat.Bmp;
        //            }
        //        case var case1 when case1 == Output.Format.Gif:
        //            {
        //                return RasterImageFormat.Gif;
        //            }
        //        case var case2 when case2 == Output.Format.Png:
        //            {
        //                return RasterImageFormat.Png;
        //            }
        //        case var case3 when case3 == Output.Format.Tif:
        //            {
        //                if (bitsPerPixel <= 2)
        //                    return RasterImageFormat.TifxFaxG4;
        //                return RasterImageFormat.Tif;
        //            }

        //        default:
        //            {
        //                return RasterImageFormat.Jpeg;
        //            }
        //    }
        //}
        public List<Smead.RecordsManagement.WorkGroupItem> GetWorkGroupList()
        {
            var wlist = Smead.RecordsManagement.Navigation.GetWorkGroups(passport).OrderBy(x => x.WorkGroupName).ToList();
            return wlist;
        }
        [HttpGet]
        public async Task<IActionResult> OrphanPartial([FromServices] IWebHostEnvironment webHostEnvironment, string docdata, string ViewType)
        {
            var model = new OrphanModel(passport);
            try
            {
                model.OrphanPerPageRecord();
                FlyoutModel flyoutModel = await LoadAttachmentOrphanData(webHostEnvironment,docdata, 1, model.DefaultPerPageRecord, "Orphans", "");
                var dt = Attachments.GetAllOrphansCount(passport, "");
                var TotalRecords = dt.Rows[0].ItemArray[0].IntValue();
                model.SetValues(docdata, flyoutModel);
                model.TotalRecords = TotalRecords;
                model.SelectedViewType = ViewType;
                return PartialView("../Data/_OrphanContent", model);
            }
            catch (Exception)
            {
                return PartialView("../Data/_OrphanContent", model);
            }
        }
        public class CommonControllerModel : BaseModel
        {
            public CommonControllerModel()
            {
                this.JsonString = "";
            }
            public string JsonString
            {
                get;
                set;
            }
            public string ErrorType
            {
                get;
                set;
            }
            public string ErrorMessage
            {
                get;
                set;
            }
        }

        public class ChkTabPermissionModelReq
        {
            public string pSecureObjectName { get; set; } = "";
            public int pSecureObjectType { get; set; } 
            public int pPassportPermissions { get; set; }
        }
        
    }
    public class PopupdocViewerReqModel
    {
        public PopupdocViewerModel.popupdocViewerUI paramss { get; set; }
    }
}
