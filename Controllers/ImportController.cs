using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
//using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic
using Newtonsoft.Json;
using Smead.RecordsManagement;
using Smead.Security;
using TabFusionRMS.DataBaseManagerVB;
using TabFusionRMS.Models;
//using System.Data.OleDb;
using TabFusionRMS.RepositoryVB;
using Microsoft.AspNetCore.Http;
using TabFusionRMS.Resource;
using Newtonsoft.Json.Linq;
using TabFusionRMS.WebCS;
using Microsoft.AspNetCore.Mvc;
using System.Data.OleDb;
using Directory = System.IO.Directory;
using TabFusionRMS.WebCS.FusionElevenModels;

public partial struct TRACKDESTELEMENTS
{
    public bool bDoRecon;
    public string sDestination;
    public string sUserName;
    public DateTime dDateDue;
    public DateTime dTransactionDateTime;
    public string sTrackingAdditionalField1;
    public string sTrackingAdditionalField2;
}

namespace TabFusionRMS.WebCS.Controllers
{
    public partial class ImportController : BaseController
    {

        private const string IMPORT_TRACK = "<<TRACKING IMPORT>>";
        private const string SECURE_TRACKING = "Tracking";
        private const string TRACK_DEST = "<<TRACKING DESTINATION>>";
        private const string TRACK__OBJ = "<<TRACKING OBJECT>>";
        private const string TRACK_DATE = "<<TRACKING DATE>>";
        private const string TRACK_OPER = "<<TRACKING OPERATOR>>";
        private const string TRACK_RECN = "<<TRACKING RECONCILATION>>";
        private const string TRACK__DUE = "<<TRACKING DUE DATE>>";
        private const string TRACK_ADDIT_FIELD1 = "<<TRACKING TEXT FIELD>>";
        private const string TRACK_ADDIT_FIELD2 = "<<TRACKING MEMO FIELD>>";
        private const string SKIP_FIELD = "<<SKIP FIELD>>";
        private const string IMAGE_COPY = "<<IMAGE COPY>>";
        private const string TRACK_OCR = "<<OCR TEXT>>";
        private const string PCFILE_COPY = "<<PCFILE COPY>>";
        private const string TRACK_PC_OCR = "<<PCFILE TEXT>>";
        private const string ATTMT_LINK = "<<ATTACHMENT LINK>>";
        private const string NON_FIELDS = "|" + SKIP_FIELD + "|" + ATTMT_LINK + "|" + IMAGE_COPY + "|" + TRACK_DEST + "|" + TRACK__OBJ + "|" + TRACK_OPER + "|" + TRACK__DUE + "|" + TRACK_DATE + "|" + TRACK_RECN + "|" + TRACK_ADDIT_FIELD1 + "|" + TRACK_ADDIT_FIELD2 + "|" + "|" + TRACK_OCR + "|" + PCFILE_COPY + "|" + TRACK_PC_OCR + "|";
        private const string IMAGE_FIELDS = "|" + IMAGE_COPY + "|" + PCFILE_COPY + "|";

        private const string BOTH_DISPLAY = "Overwrite Existing and Add New Records";
        private const string BOTH_DB_TEXT = "UPDATE";
        private const string OVERWRITE_DISPLAY = "Overwrite Existing Records Only";
        private const string OVERWRITE_DB_TEXT = "SKIPNONDUPS";
        private const string NEW_DISPLAY = "Add New Records Only";
        private const string NEW_DB_TEXT = "SKIP";
        private const string NEW_SKIP_DISPLAY = "Add New Records Only / Skip Duplicates";
        private const string NEW_SKIP_DB_TEXT = "IGNORE";

        private const string PRINT_NEW_ONLY = "Print New Records Only";
        private const string PRINT_EXISTING_ONLY = "Print Existing Only";
        private const string PRINT_ALL = "Print New and Existing Records";
        private const string IMPORTBY_NONE = "(None) - for Adding New Records Only";

        private const int IMPORT_ONLY = 0;

        // Private Property _IDBManager As IDBManager
        //private IRepository<Table> _iTables { get; set; }
        //private IRepository<Databas> _iDatabas { get; set; }
        //private IRepository<ImportLoad> _iImportLoad { get; set; }
        //private IRepository<ImportJob> _iImportJob { get; set; }
        //private IRepository<TabFusionRMS.Models.System> _iSystem { get; set; }
        //private IRepository<Setting> _iSetting { get; set; }
        //private IRepository<ImportField> _iImportField { get; set; }
        //private IRepository<OneStripJob> _iOneStripJob { get; set; }
        //private IRepository<ScanList> _iScanList { get; set; }
        //private IRepository<TabSet> _iTabSet { get; set; }
        //private IRepository<TableTab> _iTableTab { get; set; }
        //private IRepository<RelationShip> _iRelationship { get; set; }
        //private IRepository<View> _iView { get; set; }
        //private IRepository<SLTrackingSelectData> _iSLTrackingSelectData { get; set; }
        //private IRepository<SLTextSearchItem> _iSLTextSearchItems { get; set; }
        //private IRepository<SLRequestor> _iSLRequestor { get; set; }
        //private IRepository<SLIndexer> _iSLIndexer { get; set; }
        //private IRepository<SLAuditUpdate> _iSLAuditUpdate { get; set; }
        //private IRepository<SLAuditUpdChildren> _iSLAuditUpdChildren { get; set; }
        //private IRepository<TrackingHistory> _iTrackingHistory { get; set; }
        //private IRepository<SLDestructCertItem> _iSLDestCertItem { get; set; }
        //private IRepository<SLRetentionCode> _iSLRetentionCode { get; set; }
        //private IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject { get; set; }
        //private IRepository<SecureObjectPermission> _iSecureObjectPermission { get; set; }
        //private IRepository<SecureUser> _iSecureUser { get; set; }
        //private IRepository<OutputSetting> _iOutputSetting { get; set; }
        //private IRepository<Volume> _iVolume { get; set; }
        //private IRepository<ImageTablesList> _iImageTablesList { get; set; }
        //private IRepository<Userlink> _iUserLink { get; set; }
        //private IRepository<Trackable> _iTrackables { get; set; }
        //private IRepository<PCFilesPointer> _iPCFilePointer { get; set; }
        //private IRepository<ScanRule> _iScanRule { get; set; }
        //private IRepository<TrackingStatu> _iTrackingStatus { get; set; }
        //private IRepository<AssetStatu> _iAssetStatus { get; set; }
        private IDBManager _IDBManager = new DBManager();
        private IConfiguration _configuration { get; set; }

        // IDBManager As IDBManager, 
        public ImportController(IConfiguration config)
        {
            _configuration = config;
        }
        // GET: /Import

        #region Import

        public IActionResult Index()
        {
            httpContext.Session.SetString("iImportRefId", passport.UserId.ToString());
            return PartialView("_ImportMainForm");
        }

        public JsonResult GetImportDDL()
        {
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            IRepository<ImportField> _iImportField = new Repositories<ImportField>();
            string oImportDDLJSON = "";
            try
            {
                var importDDLList = Keys.GetImportDDL(_iImportLoad.All(), _iImportField.All(), passport);
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                oImportDDLJSON = JsonConvert.SerializeObject(importDDLList, Formatting.Indented, Setting);
                Keys.ErrorType = "s";
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message;
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                errorMessage = Keys.ErrorMessage,
                oImportDDLJSON
            });
        }
        public void GetConnStringForOLEDB(string extension, ref string connString, bool headerFlag = false, string Delimiter = "", string uploadedFilePath = "")
        {
            try
            {
                OleDbConnectionStringBuilder sb = new System.Data.OleDb.OleDbConnectionStringBuilder();
                switch (extension ?? "")
                {
                    case ".mdb":
                    case ".accdb":
                        {
                            if (Strings.StrComp(extension, ".accdb", Constants.vbTextCompare) == 0)
                            {
                                sb.Provider = "Microsoft.ACE.OLEDB.12.0";
                                sb.DataSource = uploadedFilePath;
                            }
                            // connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};"
                            else
                            {
                                sb.Provider = "Microsoft.Jet.OLEDB.4.0";
                                sb.DataSource = uploadedFilePath;
                                // connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};"
                            }
                            connString = sb.ToString();
                            break;
                        }
                    case ".xls":
                    case ".xlsx":
                        {
                            if (Strings.StrComp(extension, ".xls", Constants.vbTextCompare) == 0)
                            {
                                if (headerFlag)
                                {
                                    sb.Provider = "Microsoft.Jet.OLEDB.4.0";
                                    sb.DataSource = uploadedFilePath;
                                    sb.Add("Extended Properties", "Excel 8.0;HDR=YES;IMEX=1");
                                }
                                // connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;HDR=YES;IMEX=1';"
                                else
                                {
                                    sb.Provider = "Microsoft.Jet.OLEDB.4.0";
                                    sb.DataSource = uploadedFilePath;
                                    sb.Add("Extended Properties", "Excel 8.0;HDR=NO;IMEX=1");
                                    // connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Excel 8.0;HDR=NO;IMEX=1';"
                                }
                            }
                            else if (headerFlag)
                            {
                                sb.Provider = "Microsoft.ACE.OLEDB.12.0";
                                sb.DataSource = uploadedFilePath;
                                sb.Add("Extended Properties", "Excel 12.0;HDR=YES;IMEX=1");
                            }
                            // connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=YES;IMEX=1';"
                            else
                            {
                                sb.Provider = "Microsoft.ACE.OLEDB.12.0";
                                sb.DataSource = uploadedFilePath;
                                sb.Add("Extended Properties", "Excel 12.0;HDR=NO;IMEX=1");
                                // connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0;HDR=NO;IMEX=1';"
                            }
                            connString = sb.ToString();
                            break;
                        }
                    case ".dbf":
                        {
                            sb.Provider = "Microsoft.Jet.OLEDB.4.0";
                            sb.DataSource = uploadedFilePath;
                            sb.Add("Extended Properties", "dBASE IV;User ID=Admin;Password=");
                            // connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=dBASE IV;User ID=Admin;Password=;"
                            connString = sb.ToString();
                            break;
                        }
                    case ".txt":
                    case ".csv":
                        {
                            if (Convert.ToString(Delimiter) == ",")
                            {
                                if (headerFlag)
                                {
                                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR=YES;FMT=CsvDelimited';";
                                }
                                else
                                {
                                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR=NO;FMT=CsvDelimited';";
                                }
                            }

                            else if (Convert.ToString(Delimiter) == Strings.Space(1))
                            {
                                if (headerFlag)
                                {
                                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR=Yes;IMEX=1;FMT=Delimited(" + Strings.Space(1) + @")\';";
                                }
                                else
                                {
                                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR=NO;IMEX=1;FMT=Delimited(" + Strings.Space(1) + @" )\';";
                                }
                            }
                            else if (Convert.ToString(Delimiter) == Constants.vbTab)
                            {
                                if (headerFlag)
                                {
                                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR=YES;IMEX=1;FMT=TabDelimited';";
                                }
                                else
                                {
                                    connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR=NO;IMEX=1;FMT=TabDelimited';";
                                }
                            }
                            else if (Convert.ToString(Delimiter) == ";")
                            {
                                if (headerFlag)
                                {
                                    connString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=""Text;HDR=YES;IMEX=1;FMT=Delimited(';')\"";";
                                }
                                else
                                {
                                    connString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=""Text;HDR=NO;IMEX=1;FMT=Delimited(';')\"";";
                                }
                            }
                            else if (headerFlag)
                            {
                                connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR=YES;IMEX=1;FMT=Delimited(" + Convert.ToString(Delimiter) + @")\';";
                            }
                            else
                            {
                                connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR=NO;IMEX=1;FMT=Delimited(" + Convert.ToString(Delimiter) + @")\';";
                            }
                            connString = string.Format(connString, uploadedFilePath);
                            break;
                        }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void FindSheetType(string extension, string UploadedFilePath, ref List<KeyValuePair<string, string>> sheetTableList, ref string SheetType, [Optional, DefaultParameterValue(false)] ref bool exceptionFlag)
        {
            try
            {
                string connString = null;
                if (!(extension.Equals(".csv") | extension.Equals(".txt") | extension.Equals(".dbf")))
                {
                    GetConnStringForOLEDB(extension, ref connString, uploadedFilePath: UploadedFilePath);
                    using (var excel_con = new OleDbConnection(connString))
                    {
                        excel_con.Open();
                        var count = excel_con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, default).Rows.Count;
                        for (int m = 0, loopTo = count - 1; m <= loopTo; m++)
                        {
                            string sheetName = excel_con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, default).Rows[m]["TABLE_NAME"].ToString();
                            switch (extension ?? "")
                            {
                                case ".mdb":
                                case ".accdb":
                                    {
                                        if (Strings.StrComp(Strings.Left(sheetName, 4), "msys", Constants.vbTextCompare) != 0)
                                        {
                                            sheetTableList.Add(new KeyValuePair<string, string>(sheetName, sheetName));
                                        }
                                        break;
                                    }
                                case ".xls":
                                case ".xlsx":
                                    {
                                        if (Strings.Left(sheetName, 1) == "'" & Strings.Right(sheetName, 1) == "'")
                                        {
                                            sheetName = Strings.Mid(sheetName, 2);
                                            sheetName = Strings.Left(sheetName, (int)(Strings.Len(sheetName) - 1L));
                                        }
                                        if (Strings.Right(sheetName, 1) != "_")
                                        {
                                            if (Strings.Right(sheetName, 1) == "$")
                                            {
                                                sheetTableList.Add(new KeyValuePair<string, string>(sheetName, Strings.Left(sheetName, Strings.Len(sheetName) - 1)));
                                                SheetType = "WorkSheet";
                                            }
                                            else
                                            {
                                                sheetTableList.Add(new KeyValuePair<string, string>(sheetName, Strings.Left(sheetName, Strings.Len(sheetName) - 1)));
                                                SheetType = "Named Range";
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                        // excel_con.Close()
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                exceptionFlag = true;
                throw ex;
            }
            catch (Exception)
            {
                exceptionFlag = false;
                throw;
            }

        }

        public async Task<IActionResult> UploadSingleFile()
        {
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            do
            {
                try
                {
                    var pfile = httpContext.Request.Form.Files[0];
                    var ImportLoadName = httpContext.Request.Form["ImportLoadName"].Text();
                    //var ImportLoadName = importload.Count>0 ? importload[0] : "";
                    string UploadedFilePath;
                    string UploadedFileName = Path.GetFileName(pfile.FileName);
                    string extension = Path.GetExtension(UploadedFileName);
                    string ServerPath = "";
                    ImportLoad oImportLoad = default;

                    if (!string.IsNullOrEmpty(ImportLoadName))
                    {
                        oImportLoad = _iImportLoad.All().Where(m => m.LoadName.Trim().ToLower().Equals(ImportLoadName.Trim().ToLower())).FirstOrDefault();
                        if (oImportLoad is not null)
                        {
                            // FUS-5700
                            var oFileName = string.IsNullOrEmpty(oImportLoad.TempInputFile) ? Path.GetFileName(oImportLoad.InputFile) : Path.GetFileName(oImportLoad.TempInputFile);
                            if (oFileName.Trim().ToLower().Equals(UploadedFileName.Trim().ToLower()))
                            {
                                string sGUID = Guid.NewGuid().ToString();


                                ServerPath = Path.Combine("~/ImportFiles/" + oImportLoad.LoadName.Trim().ToLower() + "/" + httpContext.Session.GetString("UserName").ToString().Trim().ToLower() + "/" + sGUID + "/");

                                if (!System.IO.Directory.Exists(ServerPath))
                                {
                                    System.IO.Directory.CreateDirectory(ServerPath);
                                }

                                UploadedFilePath = Path.Combine(ServerPath, UploadedFileName);

                                using (var stream = System.IO.File.Create(UploadedFilePath))
                                {
                                    await pfile.CopyToAsync(stream);
                                }

                                //pfile.(UploadedFilePath);

                                if (extension.Equals(".txt") | extension.Equals(".csv"))
                                {
                                    string oSourcePath = Path.Combine(Path.GetDirectoryName(UploadedFilePath), "schema.ini");
                                    string oDestinationPath = Path.Combine(ServerPath, "schema.ini");
                                    if (System.IO.File.Exists(oSourcePath))
                                    {
                                        System.IO.File.Copy(oSourcePath, oDestinationPath);
                                    }
                                }

                                // oImportLoad.InputFile = UplodedFilePath
                                // _iImportLoad.Update(oImportLoad)
                                httpContext.Session.SetString("ImportFilePath", UploadedFilePath);
                                // FUS-5700
                                if (string.IsNullOrEmpty(oImportLoad.TempInputFile))
                                {
                                    oImportLoad.TempInputFile = UploadedFilePath;
                                    _iImportLoad.Update(oImportLoad);
                                }
                                Keys.ErrorType = "s";
                            }
                            else
                            {
                                Keys.ErrorType = "w";
                                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlSelectProperFile"), oFileName);
                                break;
                            }
                        }

                        else
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = Languages.get_Translation("msgImportCtrlDoesNtExist");
                            break;
                        }
                        if (System.IO.File.ReadAllText(UploadedFilePath).Length == 0)
                        {
                            System.IO.File.Delete(UploadedFilePath);
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgimportctrlisempty"), UploadedFileName);
                            Keys.ErrorType = "w";
                            break;
                        }
                    }
                    else
                    {
                        Keys.ErrorMessage = string.Format(Languages.get_Translation("msgimportctrlisempty"), UploadedFileName);
                        Keys.ErrorType = "w";
                    }
                }
                catch (Exception ex)
                {
                    Keys.ErrorMessage = ex.Message;
                    Keys.ErrorType = "e";
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public async Task<IActionResult> UploadFileAndRunLoad()
        {
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            bool IsProcessLoad = false;
            do
            {
                try
                {
                    var pfile = httpContext.Request.Form.Files[0];
                    var ImportLoadName = httpContext.Request.Form["ImportLoadName"].Text();
                    //var ImportLoadName = importload.Count > 0 ? importload[0]:"" ;
                    string UploadedFilePath = string.Empty;
                    string UploadedFileName = Path.GetFileName(pfile.FileName);
                    string extension = Path.GetExtension(UploadedFileName);
                    string ServerPath = "";
                    ImportLoad oImportLoad = default;
                    var physicalwwwrootpath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "wwwroot";

                    if (!string.IsNullOrEmpty(ImportLoadName))
                    {
                        oImportLoad = _iImportLoad.All().Where(m => m.LoadName.Trim().ToLower().Equals(ImportLoadName.Trim().ToLower())).FirstOrDefault();
                        if (oImportLoad is not null)
                        {
                            string sGUID = Guid.NewGuid().ToString();
                            //ServerPath = Path.Combine("~/ImportFiles/" + oImportLoad.LoadName.Trim().ToLower() + "/" + (httpContext.Session.GetString("UserName")??"").Trim().ToLower() + "/" + sGUID + "/");
                            ServerPath = physicalwwwrootpath + @"\ImportFiles\" + oImportLoad.LoadName.Trim().ToLower() + @"\" + (httpContext.Session.GetString("UserName") ?? "").Trim().ToLower() + @"\" + sGUID + @"\";

                            if (!System.IO.Directory.Exists(ServerPath))
                                System.IO.Directory.CreateDirectory(ServerPath);

                            UploadedFileName = UploadedFileName.Replace("[", "").Replace("]", "");
                            UploadedFilePath = Path.Combine(ServerPath, UploadedFileName);
                            using (var stream = System.IO.File.Create(UploadedFilePath))
                            {
                                await pfile.CopyToAsync(stream);
                            }

                            if (extension.Equals(".txt") | extension.Equals(".csv"))
                            {
                                string oSourcePath = Path.Combine(Path.GetDirectoryName(UploadedFilePath), "schema.ini");
                                string oDestinationPath = Path.Combine(ServerPath, "schema.ini");
                                if (System.IO.File.Exists(oSourcePath))
                                {
                                    System.IO.File.Copy(oSourcePath, oDestinationPath);
                                }
                            }

                            httpContext.Session.SetString("ImportFilePath", UploadedFilePath);

                            if (string.IsNullOrEmpty(oImportLoad.TempInputFile))
                            {
                                oImportLoad.TempInputFile = UploadedFilePath;
                                _iImportLoad.Update(oImportLoad);
                            }
                        }
                        else
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = Languages.get_Translation("msgImportCtrlDoesNtExist");
                            break;
                        }
                        if (System.IO.File.ReadAllText(UploadedFilePath).Length == 0)
                        {
                            System.IO.File.Delete(UploadedFilePath);
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlisEmpty"), UploadedFileName);
                            Keys.ErrorType = "w";
                            break;
                        }
                        else
                        {
                            IsProcessLoad = true;
                            JsonResult JSONObj = ProcessLoadForQuietProcessing(oImportLoad.LoadName);
                            dynamic obj = JSONObj.Value;
                            Keys.ErrorType = obj.errortype;
                            Keys.ErrorMessage = obj.message;
                        }
                    }
                    else
                    {
                        Keys.ErrorMessage = string.Format(Languages.get_Translation("msgimportctrlisempty"), UploadedFileName);
                        Keys.ErrorType = "w";
                    }
                }


                catch (Exception ex)
                {
                    Keys.ErrorMessage = ex.Message;
                    Keys.ErrorType = "e";
                }
            }
            while (false);
            return Json(new
            {
                IsProcessLoad,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        public async Task<IActionResult> SendFileContent()
        {
            bool exceptionFlag = false;
            string sheetTableListJSON = "";
            string SheetTypeJSON = "";
            string OriginalInputFileJSON = "";
            do
            {
                try
                {
                    var pfile = httpContext.Request.Form.Files[0];
                    var importload = httpContext.Request.Form["formatFlag"][0];
                    var ImportLoadName = httpContext.Request.Form["ImportLoadName"].Text();
                    //var ImportLoadName = importload1.Count > 0 ? importload1[0] : "";
                    var physicalwwwrootpath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath") + "wwwroot";
                    bool IsFirstTime = Convert.ToBoolean(importload);
                    if (IsFirstTime == true)
                    {
                        if (TempData.ContainsKey("ImportFields"))
                        {
                            TempData.Remove("ImportLoad");
                        }
                        if (TempData.ContainsKey("ImportLoad"))
                        {
                            TempData.Remove("ImportLoad");
                        }
                    }

                    string UploadedFilePath;
                    string filename = pfile.FileName;
                    filename = filename.Replace("[", "").Replace("]", "");

                    string UploadedFileName = Path.GetFileName(filename);

                    string extension = Path.GetExtension(UploadedFileName);

                    // Dim oInputFile As String = String.Empty
                    string[] DirectoryFolder;
                    ImportLoad oImportLoad = default;
                    bool oNewUser = false;
                    if (string.IsNullOrEmpty(extension))
                    {
                        extension = ".csv";
                    }
                    extension = extension.Trim().ToLower();
                    string ServerPath = "";
                    if (TempData.ContainsKey("ImportLoad"))
                    {
                        //oImportLoad = (ImportLoad)TempData.Peek("ImportLoad");
                        oImportLoad = TempData.GetPeek<ImportLoad>("ImportLoad");
                        // If (Not String.IsNullOrEmpty(oImportLoad.InputFile)) Then
                        if (String.IsNullOrEmpty(httpContext.Session.GetString("ImportFilePath")) is not true)
                        {
                            // DirectoryFolder = oImportLoad.InputFile.Split(Path.DirectorySeparatorChar)
                            DirectoryFolder = (httpContext.Session.GetString("ImportFilePath") ?? "").Split(Path.DirectorySeparatorChar);
                            if (DirectoryFolder[DirectoryFolder.ToString().Length - 3].Trim().ToLower().Equals((httpContext.Session.GetString("UserName") ?? "").Trim().ToLower()))
                            {
                                // ServerPath = Path.GetDirectoryName(oImportLoad.InputFile)
                                ServerPath = Path.GetDirectoryName(httpContext.Session.GetString("ImportFilePath") ?? "");
                                if (System.IO.Directory.Exists(ServerPath))
                                {
                                    foreach (string _file in System.IO.Directory.GetFiles(ServerPath))
                                        System.IO.File.Delete(_file);
                                }
                            }
                            else
                            {
                                oNewUser = true;
                                string sGUID = Guid.NewGuid().ToString();
                                if (string.IsNullOrEmpty(oImportLoad.LoadName))
                                {
                                    ServerPath = physicalwwwrootpath + @"\ImportFiles\" + (httpContext.Session.GetString("UserName") ?? "").Trim().ToLower() + @"\" + (httpContext.Session.GetString("UserName") ?? "").Trim().ToLower() + @"\" + sGUID + @"\";
                                }
                                else
                                {
                                    ServerPath = physicalwwwrootpath + @"\ImportFiles\" + oImportLoad.LoadName.Trim().ToLower() + @"\" + (httpContext.Session.GetString("UserName") ?? "").Trim().ToLower() + @"\" + sGUID + @"\";
                                }
                            }
                        }
                        else
                        {
                            string sGUID = Guid.NewGuid().ToString();
                            if (string.IsNullOrEmpty(oImportLoad.LoadName))
                            {
                                ServerPath = physicalwwwrootpath + @"\ImportFiles\" + (httpContext.Session.GetString("UserName") ?? "").Trim().ToLower() + @"\" + (httpContext.Session.GetString("UserName") ?? "").Trim().ToLower() + @"\" + sGUID + @"\";
                            }
                            else
                            {
                                ServerPath = physicalwwwrootpath + @"\ImportFiles\" + oImportLoad.LoadName.Trim().ToLower() + @"\" + (httpContext.Session.GetString("UserName") ?? "").Trim().ToLower() + @"\" + sGUID + @"\";
                            }
                        }
                    }
                    // oInputFile = Path.Combine(Path.GetDirectoryName(oImportLoad.InputFile), UplodedFileName)
                    else
                    {
                        string sGUID = Guid.NewGuid().ToString();
                        ServerPath = physicalwwwrootpath + @"\ImportFiles\" + (httpContext.Session.GetString("UserName") ?? "").Trim().ToLower() + @"\" + (httpContext.Session.GetString("UserName") ?? "").Trim().ToLower() + @"\" + sGUID + @"\";
                    }

                    if (!System.IO.Directory.Exists(ServerPath))
                    {
                        System.IO.Directory.CreateDirectory(ServerPath);
                    }

                    UploadedFilePath = Path.Combine(ServerPath, UploadedFileName);

                    using (var stream = System.IO.File.Create(UploadedFilePath))
                    {
                        await pfile.CopyToAsync(stream);
                    }
                    if (System.IO.File.ReadAllText(UploadedFilePath).Length == 0)
                    {
                        System.IO.File.Delete(UploadedFilePath);
                        Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlisEmpty"), UploadedFileName);
                        Keys.ErrorType = "w";
                        break;
                    }

                    var sheetTableList = new List<KeyValuePair<string, string>>();
                    string SheetType = "";
                    FindSheetType(extension, UploadedFilePath, ref sheetTableList, ref SheetType, ref exceptionFlag);
                    var Setting = new JsonSerializerSettings();
                    Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    OriginalInputFileJSON = JsonConvert.SerializeObject(UploadedFilePath, Formatting.Indented, Setting);
                    sheetTableListJSON = JsonConvert.SerializeObject(sheetTableList, Formatting.Indented, Setting);
                    SheetTypeJSON = JsonConvert.SerializeObject(SheetType, Formatting.Indented, Setting);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = "";
                }
                catch (Exception ex)
                {
                    if (exceptionFlag == true)
                    {
                        Keys.ErrorMessage = Languages.get_Translation("msgImportCtrlProviderNotFound");
                    }
                    else
                    {
                        Keys.ErrorMessage = ex.Message;
                    }
                    Keys.ErrorType = "e";
                }
            }
            while (false);
            return Json(new
            {
                sheetTableListJSON,
                SheetTypeJSON,
                OriginalInputFileJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public void SetDelimiterById(string DelimiterId, ref string DelimiterVal, ref string Delimiter)
        {
            switch (DelimiterId ?? "")
            {
                case "comma_radio":
                    {
                        DelimiterVal = "CsvDelimited";
                        Delimiter = ",";
                        break;
                    }
                case "Tab_radio":
                    {
                        DelimiterVal = "TabDelimited";
                        Delimiter = Constants.vbTab;
                        break;
                    }
                case "semicolon_radio":
                    {
                        DelimiterVal = "Delimited(;)";
                        Delimiter = ";";
                        break;
                    }
                case "space_radio":
                    {
                        DelimiterVal = "Delimited(" + Strings.Space(1) + ")";
                        Delimiter = Strings.Space(1);
                        break;
                    }
                case "other_radio":
                    {
                        DelimiterVal = "Delimited(" + Delimiter + ")";
                        Delimiter = Delimiter;
                        break;
                    }
            }
        }

        private string DetectEncoding(string filename)
        {
            string enc = "";

            if (System.IO.File.Exists(filename))
            {
                var filein = new FileStream(filename, FileMode.Open, FileAccess.Read);
                if (filein.CanSeek)
                {
                    var bom = new byte[5];
                    filein.Read(bom, 0, 4);
                    if (bom[0] == 0xEF & bom[1] == 0xBB & bom[2] == 0xBF | bom[0] == 0xFF & bom[1] == 0xFE | bom[0] == 0xFE & bom[1] == 0xFF | bom[0] == 0x0 & bom[1] == 0x0 & bom[2] == 0xFE & bom[3] == 0xFF)
                    {
                        enc = "Unicode";
                    }
                    else
                    {
                        enc = "ANSI";
                    }
                    filein.Seek(0L, SeekOrigin.Begin);
                }
                filein.Close();
            }
            return enc;
        }

        public void CreateINIFileForCSV(string Delimiter, string filepath, bool headerFlag, DataTable TempDataTable = null)
        {
            try
            {
                using (var sw = new StreamWriter(Path.Combine(Path.GetDirectoryName(filepath), "schema.ini"), false, Encoding.ASCII))
                {
                    sw.WriteLine("[{0}]", Path.GetFileName(filepath));
                    sw.WriteLine("ColNameHeader={0}", Interaction.IIf(headerFlag, "TRUE", "FALSE"));
                    sw.WriteLine("Format={0}", Delimiter);
                    string extension = Path.GetExtension(filepath);

                    if (!string.IsNullOrEmpty(extension))
                    {
                        if (extension.Trim().ToLower().Equals(".txt"))
                        {
                            sw.WriteLine("TextDelimiter='");
                            sw.WriteLine("CharacterSet=" + DetectEncoding(filepath));
                        }
                    }

                    if (TempDataTable is null)
                        return;
                    if (TempDataTable.Columns.Count != 0)
                        return;

                    //List<ImportField> oImportFields = (List<ImportField>)TempData.GetPeek("ImportFields");
                    List<ImportField> oImportFields = TempData.GetPeek<List<ImportField>>("ImportFields");
                    int iCount = 0;
                    int ImportFieldCount = 0;
                    if (oImportFields is not null)
                        ImportFieldCount = oImportFields.Count;

                    if (TempDataTable.Columns.Count != 0)
                    {
                        if (headerFlag)
                        {
                            for (int index = 1, loopTo = TempDataTable.Columns.Count; index <= loopTo; index++)
                            {
                                if (!string.IsNullOrEmpty(TempDataTable.Columns[index - 1].ColumnName))
                                {
                                    TempDataTable.Columns[index - 1].ColumnName = TempDataTable.Columns[index - 1].ColumnName.Replace("\"", "");
                                }

                                if (ImportFieldCount == 0)
                                {
                                    sw.WriteLine("Col{0}=\"{1}\" Text", index, TempDataTable.Columns[index - 1].ColumnName);
                                }
                                else if (ImportFieldCount <= TempDataTable.Columns.Count & iCount < ImportFieldCount)
                                {
                                    if (oImportFields[iCount].FieldName.Trim().Equals(SKIP_FIELD))
                                    {
                                        sw.WriteLine("Col{0}=\"{1}:(Skipped)\" Text", index, TempDataTable.Columns[iCount].ColumnName);
                                    }
                                    else if (oImportFields[index - 1].FieldName.Contains("<<"))
                                    {
                                        sw.WriteLine("Col{0}=\"{1}\" Text", index, TempDataTable.Columns[iCount].ColumnName);
                                    }
                                    else
                                    {
                                        sw.WriteLine("Col{0}=\"{1}:{2}\" Text", index, TempDataTable.Columns[iCount].ColumnName, oImportFields[iCount].FieldName);
                                    }

                                    iCount += 1;
                                }
                                else if (ImportFieldCount > TempDataTable.Columns.Count)
                                {
                                    if (oImportFields[index - 1].FieldName.Trim().Equals(SKIP_FIELD))
                                    {
                                        sw.WriteLine("Col{0}=\"{1}:(Skipped)\" Text", index, TempDataTable.Columns[index - 1].ColumnName);
                                    }
                                    else if (oImportFields[index - 1].FieldName.Contains("<<"))
                                    {
                                        sw.WriteLine("Col{0}=\"{1}\" Text", index, TempDataTable.Columns[index - 1].ColumnName);
                                    }
                                    else
                                    {
                                        sw.WriteLine("Col{0}=\"{1}:{2}\" Text", index, TempDataTable.Columns[index - 1].ColumnName, oImportFields[index - 1].FieldName);
                                    }
                                }
                                else
                                {
                                    sw.WriteLine("Col{0}=\"{1}\" Text", index, TempDataTable.Columns[index - 1].ColumnName);
                                }
                            }
                        }
                        else
                        {
                            for (int index = 1, loopTo1 = TempDataTable.Columns.Count; index <= loopTo1; index++)
                            {
                                if (ImportFieldCount == 0)
                                {
                                    sw.WriteLine("Col{0}=\"F{0}\" Text", index);
                                }
                                else if (ImportFieldCount <= TempDataTable.Columns.Count & iCount < ImportFieldCount)
                                {
                                    if (oImportFields[iCount].FieldName.Trim().Equals(SKIP_FIELD))
                                    {
                                        sw.WriteLine("Col{0}=\"F{0}:(Skipped)\" Text", index);
                                    }
                                    else if (oImportFields[index - 1].FieldName.Contains("<<"))
                                    {
                                        sw.WriteLine("Col{0}=\"F{0}\" Text", index);
                                    }
                                    else
                                    {
                                        sw.WriteLine("Col{0}=\"F{0}:{1}\" Text", index, oImportFields[iCount].FieldName);
                                    }

                                    iCount += 1;
                                }
                                else if (ImportFieldCount > TempDataTable.Columns.Count)
                                {
                                    if (oImportFields[index - 1].FieldName.Trim().Equals(SKIP_FIELD))
                                    {
                                        sw.WriteLine("Col{0}=\"F{0}:(Skipped)\" Text", index);
                                    }
                                    else if (oImportFields[index - 1].FieldName.Contains("<<"))
                                    {
                                        sw.WriteLine("Col{0}=\"F{0}\" Text", index);
                                    }
                                    else
                                    {
                                        sw.WriteLine("Col{0}=\"F{0}:{1}\" Text", index, oImportFields[index - 1].FieldName);
                                    }
                                }
                                else
                                {
                                    sw.WriteLine("Col{0}=\"F{0}\" Text", index);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }
        public void SetGridByDelimiter(DataTable TempDataTable, ref DataTable dtExcelData, string DelimiterVal, string filePath, bool headerFlag, string connString, int numberOfRow, string objectName)
        {
            try
            {
                CreateINIFileForCSV(DelimiterVal, filePath, headerFlag, TempDataTable);
                using (var excel_con = new OleDbConnection(connString))
                {
                    excel_con.Open();
                    string query = Convert.ToString("SELECT TOP " + numberOfRow + " * FROM [") + objectName.Trim() + "]";
                    using (var oda = new OleDbDataAdapter(query, excel_con))
                    {
                        oda.Fill(dtExcelData);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public JsonResult GetGridDataFromFile(string filePath, bool headerFlag = false, string sCurrentLoad = null, int numberOfRow = 10, string firstSheet = "", string DelimiterId = "", string Delimiter = "", bool mbFixedWidth = false, bool formatFlag = true)
        {
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            IRepository<ImportField> _iImportField = new Repositories<ImportField>();
            try
            {
                var GridColumnEntities = new List<GridColumns>();
                var dtExcelData = new DataTable();
                var TempDataTable = new DataTable();
                string connString = null;
                string extension = Path.GetExtension(filePath).ToLower();
                object varObject = "";
                string objectName = Path.GetFileName(filePath);
                string originalname = objectName;
                string GridCaption = "";
                var sImportLoad = new ImportLoad();
                var lstImportField = new List<ImportField>();
                if (System.IO.File.Exists(filePath))
                {
                    string DelimiterVal = "";
                    if (extension.Equals(".csv") | extension.Equals(".txt") | extension.Equals(".dbf"))
                    {
                        string pathName = Path.GetDirectoryName(filePath);
                        SetDelimiterById(DelimiterId, ref DelimiterVal, ref Delimiter);
                        CreateINIFileForCSV(DelimiterVal, filePath, headerFlag);
                        GetConnStringForOLEDB(extension, ref connString, headerFlag, Convert.ToString(Delimiter), pathName);
                    }
                    else
                    {
                        GetConnStringForOLEDB(extension, ref connString, headerFlag, Convert.ToString(Delimiter), filePath);
                        objectName = firstSheet;
                    }
                    using (var excel_con = new OleDbConnection(connString))
                    {
                        excel_con.Open();
                        string query = Convert.ToString("SELECT TOP " + numberOfRow + " * FROM [") + objectName.Trim() + "]";
                        using (var oda = new OleDbDataAdapter(query, excel_con))
                        {
                            oda.Fill(TempDataTable);
                        }
                        string objExtension = Path.GetExtension(objectName);

                        if (objExtension.Equals(".csv") | objExtension.Equals(".txt"))
                        {
                            SetGridByDelimiter(TempDataTable, ref dtExcelData, DelimiterVal, filePath, headerFlag, connString, numberOfRow, objectName);
                        }
                        else
                        {
                            if (TempDataTable is not null)
                            {
                                int ColumnCount = TempDataTable.Columns.Count;
                                var oImportFields = new List<ImportField>();
                                //oImportFields = (List<ImportField>)TempData.Peek("ImportFields");
                                oImportFields = TempData.GetPeek<List<ImportField>>("ImportFields");
                                int ImportFieldCount = 0;
                                if (oImportFields is not null)
                                {
                                    ImportFieldCount = oImportFields.Count;
                                }
                                if (ColumnCount != 0)
                                {
                                    int iCount = 0;
                                    for (int index = 1, loopTo = ColumnCount; index <= loopTo; index++)
                                    {
                                        if (ImportFieldCount != 0)
                                        {
                                            if (ImportFieldCount <= ColumnCount & iCount < ImportFieldCount)
                                            {
                                                if (oImportFields[iCount].FieldName.Trim().Equals(SKIP_FIELD))
                                                {
                                                    TempDataTable.Columns[index - 1].ColumnName = TempDataTable.Columns[index - 1].ColumnName + ":(Skipped)";
                                                }
                                                else if (!oImportFields[iCount].FieldName.Contains("<<"))
                                                {
                                                    TempDataTable.Columns[iCount].ColumnName = TempDataTable.Columns[iCount].ColumnName + ":" + oImportFields[iCount].FieldName;
                                                }
                                                iCount = iCount + 1;
                                            }
                                            else if (ImportFieldCount > ColumnCount)
                                            {
                                                if (oImportFields[index - 1].FieldName.Trim().Equals(SKIP_FIELD))
                                                {
                                                    TempDataTable.Columns[index - 1].ColumnName = TempDataTable.Columns[index - 1].ColumnName + ":(Skipped)";
                                                }
                                                else if (!oImportFields[index - 1].FieldName.Contains("<<"))
                                                {
                                                    TempDataTable.Columns[index - 1].ColumnName = TempDataTable.Columns[index - 1].ColumnName + ":" + oImportFields[index - 1].FieldName;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            dtExcelData = TempDataTable.Copy();
                        }
                        if (TempData is not null)
                        {
                            if (TempData.ContainsKey("FileDataTable"))
                            {
                                TempData.Remove("FileDataTable");
                            }
                            //TempData["FileDataTable"] = dtExcelData;
                            TempData.Set<DataTable>("FileDataTable", dtExcelData);
                        }
                        switch (extension ?? "")
                        {
                            case ".mdb":
                            case ".accdb":
                                {
                                    GridCaption = string.Format(Languages.get_Translation("tiImportCtrlSampleDataFromTbl"), objectName);
                                    break;
                                }
                            case ".xls":
                            case ".xlsx":
                                {
                                    if (Strings.Right(objectName, 1) == "$")
                                    {
                                        GridCaption = string.Format(Languages.get_Translation("tiImportCtrlSampleDataFromWS"), Strings.Left(objectName, Strings.Len(objectName) - 1));
                                    }
                                    else
                                    {
                                        GridCaption = string.Format(Languages.get_Translation("tiImportCtrlSampleDataFromRange"), objectName);
                                    }
                                    break;
                                }
                            case ".dbf":
                                {
                                    break;
                                }
                            case ".txt":
                            case ".csv":
                                {
                                    if (!string.IsNullOrEmpty(objectName))
                                    {
                                        GridCaption = string.Format(Languages.get_Translation("tiImportCtrlSampleDataFromFile"), originalname);
                                    }
                                    break;
                                }
                        }

                        int i = 0;
                        if (dtExcelData is not null & dtExcelData.Rows.Count >= 0)
                        {
                            foreach (DataColumn column in dtExcelData.Columns)
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
                                GridColumnEntities.Add(GridColumnEntity);
                                i = i + 1;
                            }
                            var lGridColumns = GridColumnEntities.Select(a => new
                            {
                                srno = a.ColumnSrNo,
                                name = a.ColumnName,
                                displayName = a.ColumnDisplayName,
                                dataType = a.ColumnDataType,
                                maxLength = a.ColumnMaxLength,
                                isPk = a.IsPk,
                                autoInc = a.AutoInc
                            });
                            if (formatFlag)
                            {
                                if (TempData.ContainsKey("ImportLoad"))
                                {
                                    TempData.Remove("ImportLoad");
                                }
                                if (TempData.ContainsKey("ImportFields"))
                                {
                                    TempData.Remove("ImportFields");
                                }
                                if (!string.IsNullOrEmpty(sCurrentLoad))
                                {
                                    sImportLoad = _iImportLoad.All().Where(m => m.LoadName.Trim().ToLower().Equals(sCurrentLoad.Trim().ToLower())).FirstOrDefault();
                                    lstImportField = _iImportField.All().Where(m => m.ImportLoad.Trim().ToLower().Equals(sCurrentLoad.Trim().ToLower())).ToList();
                                }
                                //TempData["ImportFields"] = lstImportField;
                                TempData.Set<List<ImportField>>("ImportFields", lstImportField);
                                //TempData["ImportLoad"] = sImportLoad;
                                TempData.Set<ImportLoad>("ImportLoad", sImportLoad);
                            }
                            var Setting = new JsonSerializerSettings();
                            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                            var jsonObject = JsonConvert.SerializeObject(lGridColumns, Formatting.Indented, Setting);
                            var GridCaptionJSON = JsonConvert.SerializeObject(GridCaption, Formatting.Indented, Setting);
                            Keys.ErrorType = "s";
                            Keys.ErrorMessage = "";
                            return Json(new
                            {
                                jsonObject,
                                GridCaptionJSON,
                                errortype = Keys.ErrorType,
                                message = Keys.ErrorMessage
                            });
                        }
                    }
                }
                else
                {
                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlFileNotFound"), originalname);
                    Keys.ErrorType = "w";
                }
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message;
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public JsonResult ConvertDataToGrid(string sidx, string sord, int page, int rows)
        {
            var FileDT = new DataTable();
            if (TempData is not null)
            {
                if (TempData.ContainsKey("FileDataTable"))
                {
                    //FileDT = (DataTable)TempData.Peek("FileDataTable");
                    FileDT = TempData.GetPeek<DataTable>("FileDataTable");
                }
            }
            return Json(Common.ConvertDataTableToJQGridResult(FileDT, sidx, sord, page, rows));
        }

        public JsonResult GetDestinationDDL(string LoadName, string strmsHoldName, string RecordType, bool FirstRowHeader, string InputFile, string TableSheetName, int Id, string Delimiter = null)
        {
            IRepository<Table> _iTables = new Repositories<Table>();
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            string tableListJSON = "";
            string oImportLoadJSON = "";
            do
            {
                try
                {
                    var tableList = new List<KeyValuePair<string, string>>();
                    var mTableList = _iTables.All().OrderBy(m => m.TableName);
                    // Reload Permissions dataset before loading destination DDL. Bug #680
                    //CollectionsClass.ReloadPermissionDataSet();
                    passport.FillSecurePermissions();

                    if (!string.IsNullOrEmpty(LoadName))
                    {
                        strmsHoldName = strmsHoldName ?? "";
                        if (!strmsHoldName.Trim().ToLower().Equals(LoadName.Trim().ToLower()))
                        {
                            var allImportLoad = _iImportLoad.All().OrderBy(m => m.ID);
                            var IsExist = allImportLoad.Any(m => m.LoadName.Trim().ToLower().Equals(LoadName.Trim().ToLower()));
                            if (IsExist)
                            {
                                Keys.ErrorType = "w";
                                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlLoadAlreadyExistsVal"), LoadName);
                                break;
                            }
                        }
                    }

                    foreach (Table tbObj in mTableList)
                    {
                        var bDoNotAdd = CollectionsClass.IsEngineTable(tbObj.TableName.Trim().ToLower());
                        var bDoNotAddImport = CollectionsClass.EngineTablesOkayToImportList.Contains(tbObj.TableName.Trim().ToLower());
                        if (!bDoNotAdd | bDoNotAddImport)
                        {
                            // Enums.SecureObjectType.Table, Enums.PassportPermissions.Import
                            if (passport.CheckPermission(tbObj.TableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.Table, (Permissions.Permission)Enums.PassportPermissions.Import))
                            {
                                tableList.Add(new KeyValuePair<string, string>(tbObj.TableName, tbObj.UserName));
                            }
                        }
                    }
                    foreach (string tableName in CollectionsClass.EngineTablesOkayToImportList)
                    {
                        var retentionObj = MakeSureIsALoadedTable(tableName);
                        var table = (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.Table;
                        var import = (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Import;
                        if (retentionObj is not null)
                        {
                            //httpContext.Session.SetString(tableName.Trim().ToLower(), retentionObj.ToString());
                            ContextService.SetSessionValue(tableName.Trim().ToLower(), JsonConvert.SerializeObject(retentionObj), httpContext);
                        }
                        if (passport.CheckPermission(retentionObj?.TableName, table, import))
                        {
                            tableList.Add(new KeyValuePair<string, string>(retentionObj.TableName, retentionObj.UserName));
                        }
                    }
                    // Enums.SecureObjectType.Application, Enums.PassportPermissions.Access
                    if (passport.CheckPermission(SECURE_TRACKING, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.Application, (Permissions.Permission)Enums.PassportPermissions.Access))
                    {
                        tableList.Add(new KeyValuePair<string, string>(IMPORT_TRACK, IMPORT_TRACK));
                    }
                    var oImportLoad = new ImportLoad();
                    if (TempData.ContainsKey("ImportLoad"))
                    {
                        //oImportLoad = (ImportLoad)TempData.Peek("ImportLoad");
                        oImportLoad = TempData.GetPeek<ImportLoad>("ImportLoad");
                        if (RecordType.Trim().ToUpper().Equals("COMMA"))
                        {
                            if (string.IsNullOrWhiteSpace(Delimiter))
                            {
                                oImportLoad.Delimiter = null;
                            }
                            else
                            {
                                oImportLoad.Delimiter = Delimiter;
                            }
                        }
                        oImportLoad.ID = Id;
                        oImportLoad.LoadName = LoadName;
                        if (!string.IsNullOrEmpty(InputFile))
                        {
                            // oImportLoad.InputFile = HttpUtility.UrlDecode(InputFile)
                            oImportLoad.TempInputFile = System.Web.HttpUtility.UrlDecode(InputFile);
                        }

                        oImportLoad.RecordType = RecordType;
                        oImportLoad.FirstRowHeader = FirstRowHeader;
                        if (string.IsNullOrEmpty(TableSheetName))
                        {
                            oImportLoad.TableSheetName = null;
                        }
                        else
                        {
                            oImportLoad.TableSheetName = System.Web.HttpUtility.UrlDecode(TableSheetName);
                        }
                        //TempData["ImportLoad"]=oImportLoad;
                        TempData.Set<ImportLoad>("ImportLoad", oImportLoad);
                    }
                    var Setting = new JsonSerializerSettings();
                    Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    tableListJSON = JsonConvert.SerializeObject(tableList, Formatting.Indented, Setting);
                    oImportLoadJSON = JsonConvert.SerializeObject(oImportLoad, Formatting.Indented, Setting);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = "";
                }

                catch (Exception)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = "";
                }
            }
            while (false);
            return Json(new
            {
                tableListJSON,
                oImportLoadJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public void AllowAttachmentLinking(ref bool flagAttach)
        {
            IRepository<Setting> _iSetting = new Repositories<Setting>();
            try
            {
                var settingObj = _iSetting.Where(m => m.Section.Trim().ToLower().Equals("Import".Trim().ToLower()) && m.Item.Trim().ToLower().Equals("AllowLinking".Trim().ToLower())).FirstOrDefault();
                if (settingObj is not null)
                {
                    if (settingObj.Id != 0)
                    {
                        if ((long)Strings.Len(settingObj.ItemValue) > 0L)
                        {
                            if (Information.IsNumeric(settingObj.ItemValue))
                            {
                                flagAttach = settingObj.ItemValue != 0L.ToString();
                            }
                            else
                            {
                                flagAttach = Convert.ToBoolean(settingObj.ItemValue);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [HttpPost]
        //[System.Web.Mvc.ValidateInput(false)]
        public JsonResult GetAvailableField(string currentLoad, string tableName1, bool changeFlag, bool IsSkipField = false)
        {
            IRepository<Table> _iTables = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            IRepository<ImportField> _iImportField = new Repositories<ImportField>();


            string tableListJSON = "";
            string flagImportByJSON = "";
            string flagOverwriteAddJSON = "";
            try
            {
                if (changeFlag)
                {
                    if (TempData.ContainsKey("ImportFields"))
                    {
                        TempData.Remove("ImportFields");
                    }
                }
                string tableName = JsonConvert.DeserializeObject<object>(tableName1).ToString();
                var SelectedList = new List<KeyValuePair<string, string>>();
                Table tbObj;
                var tableList = new List<KeyValuePair<string, string>>();
                bool flagImportBy = false;
                bool flagOverwriteAdd = false;
                List<TabFusionRMS.WebCS.SchemaColumns> oSchemaColumns;
                var pSystemInfo = _iSystem.All().OrderBy(m => m.Id).FirstOrDefault();
                if (tableName.Trim().Equals(IMPORT_TRACK))
                {
                    tableList.Add(new KeyValuePair<string, string>(TRACK_DEST, TRACK_DEST));
                    tableList.Add(new KeyValuePair<string, string>(TRACK__OBJ, TRACK__OBJ));
                    tableList.Add(new KeyValuePair<string, string>(TRACK_DATE, TRACK_DATE));
                    tableList.Add(new KeyValuePair<string, string>(TRACK_OPER, TRACK_OPER));
                    if ((bool)pSystemInfo.ReconciliationOn)
                    {
                        tableList.Add(new KeyValuePair<string, string>(TRACK_RECN, TRACK_RECN));
                    }
                    if (pSystemInfo.DateDueOn == true)
                    {
                        tableList.Add(new KeyValuePair<string, string>(TRACK__DUE, TRACK__DUE));
                    }
                    if (pSystemInfo.TrackingAdditionalField1Desc.Length > 0)
                    {
                        tableList.Add(new KeyValuePair<string, string>(TRACK_ADDIT_FIELD1, TRACK_ADDIT_FIELD1));
                    }
                    if (pSystemInfo.TrackingAdditionalField2Desc.Length > 0)
                    {
                        tableList.Add(new KeyValuePair<string, string>(TRACK_ADDIT_FIELD2, TRACK_ADDIT_FIELD2));
                    }
                    tableList.Add(new KeyValuePair<string, string>(SKIP_FIELD, SKIP_FIELD));
                    flagImportBy = false;
                    flagOverwriteAdd = false;
                }
                else
                {
                    flagImportBy = true;
                    flagOverwriteAdd = true;
                    var bDoNotAddImport = CollectionsClass.EngineTablesOkayToImportList.Contains(tableName.Trim().ToLower());
                    if (bDoNotAddImport)
                    {
                        tbObj = ContextService.GetObjectFromJson<Table>(tableName.Trim().ToLower(), httpContext);
                    }
                    else
                    {
                        tbObj = _iTables.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                    }

                    Databas pDatabaseEntity = null;

                    if (tbObj != null)
                    {
                        pDatabaseEntity = _iDatabas.All().Where(m => m.DBName.Trim().ToLower().Equals(tbObj.DBName.Trim().ToLower())).FirstOrDefault();
                    }
                    ADODB.Connection sADOConn;

                    if (pDatabaseEntity is not null)
                    {
                        sADOConn = DataServices.DBOpen(pDatabaseEntity);
                    }
                    else
                    {
                        sADOConn = DataServices.DBOpen();
                    }
                    oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(tbObj.TableName, sADOConn);
                    foreach (SchemaColumns schemaColObj in oSchemaColumns)
                    {
                        if (!SchemaInfoDetails.IsSystemField(schemaColObj.ColumnName))
                        {
                            if (DatabaseMap.RemoveTableNameFromField(tbObj.RetentionFieldName).Equals(DatabaseMap.RemoveTableNameFromField(schemaColObj.ColumnName)))
                            {
                                tableList.Add(new KeyValuePair<string, string>(schemaColObj.ColumnName, schemaColObj.ColumnName));
                            }
                            else
                            {
                                tableList.Add(new KeyValuePair<string, string>(schemaColObj.ColumnName, schemaColObj.ColumnName));
                            }
                        }
                    }
                    tableList.Add(new KeyValuePair<string, string>(SKIP_FIELD, SKIP_FIELD));
                    if ((bool)tbObj.Attachments)
                    {
                        tableList.Add(new KeyValuePair<string, string>(IMAGE_COPY, IMAGE_COPY));
                        tableList.Add(new KeyValuePair<string, string>(PCFILE_COPY, PCFILE_COPY));
                        bool attachflag = false;
                        AllowAttachmentLinking(ref attachflag);
                        if (attachflag)
                        {
                            tableList.Add(new KeyValuePair<string, string>(ATTMT_LINK, ATTMT_LINK));
                        }
                    }
                    var boolval = passport.CheckPermission(tbObj.TableName.Trim(), (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Transfer);
                    if (boolval)
                    {
                        tableList.Add(new KeyValuePair<string, string>(TRACK_DEST, TRACK_DEST));
                        tableList.Add(new KeyValuePair<string, string>(TRACK_DATE, TRACK_DATE));
                        tableList.Add(new KeyValuePair<string, string>(TRACK_OPER, TRACK_OPER));
                        if (pSystemInfo.DateDueOn == true)
                        {
                            tableList.Add(new KeyValuePair<string, string>(TRACK__DUE, TRACK__DUE));
                        }
                        if ((bool)pSystemInfo.ReconciliationOn)
                        {
                            tableList.Add(new KeyValuePair<string, string>(TRACK_RECN, TRACK_RECN));
                        }
                        if ((pSystemInfo.TrackingAdditionalField1Desc ?? "").Length > 0)
                        {
                            tableList.Add(new KeyValuePair<string, string>(TRACK_ADDIT_FIELD1, TRACK_ADDIT_FIELD1));
                        }
                        if ((pSystemInfo.TrackingAdditionalField2Desc ?? "").Length > 0)
                        {
                            tableList.Add(new KeyValuePair<string, string>(TRACK_ADDIT_FIELD2, TRACK_ADDIT_FIELD2));
                        }
                    }
                }
                if (TempData.ContainsKey("ImportLoad"))
                {
                    ImportLoad oImportLoad;
                    //oImportLoad = (ImportLoad)TempData.Peek("ImportLoad");
                    oImportLoad = TempData.GetPeek<ImportLoad>("ImportLoad");
                    oImportLoad.FileName = (string)tableName;
                    //TempData["ImportLoad"] = oImportLoad;
                    TempData.Set<ImportLoad>("ImportLoad", oImportLoad);
                }
                if (IsSkipField)
                {
                    AddMemberToTempData(currentLoad, SKIP_FIELD);
                }
                if (TempData.ContainsKey("ImportFields"))
                {
                    var lstImportFields = new List<ImportField>();
                    //lstImportFields = (List<ImportField>)TempData.Peek("ImportFields");
                    lstImportFields = TempData.GetPeek<List<ImportField>>("ImportFields");

                    if (lstImportFields.Count != 0)
                    {
                        lstImportFields = lstImportFields.OrderBy(m => m.ReadOrder).ToList();
                        foreach (ImportField importObj in lstImportFields)
                        {
                            foreach (KeyValuePair<string, string> tableObj in tableList.ToList())
                            {
                                if (tableObj.Value.Trim().ToLower().Equals(importObj.FieldName.Trim().ToLower()))
                                {
                                    if (tableObj.Key.Trim().Equals("Selected".Trim()))
                                    {
                                        tableList.Add(new KeyValuePair<string, string>("Selected", importObj.FieldName));
                                    }
                                    else
                                    {
                                        tableList.Remove(new KeyValuePair<string, string>(importObj.FieldName, importObj.FieldName));
                                        tableList.Add(new KeyValuePair<string, string>("Selected", importObj.FieldName));
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    var oSelected = tableList.Where(m => m.Key.Trim().Equals("Selected"));
                    bool IsChanged = false;
                    if (oSelected.Count() < lstImportFields.Count)
                    {
                        foreach (var oEach in lstImportFields)
                        {
                            bool oKeyValue = oSelected.Any(m => m.Value.Trim().Equals(oEach.FieldName.Trim()));
                            if (oKeyValue == false)
                            {
                                var oImportField = _iImportField.Where(m => m.FieldName.Trim().Equals(oEach.FieldName.Trim())).FirstOrDefault();
                                if (oImportField is not null)
                                {
                                    _iImportField.Delete(oImportField);
                                    IsChanged = true;
                                }
                            }
                        }
                        if (IsChanged == true)
                        {
                            var pImportField = _iImportField.All().Where(m => m.ImportLoad.Trim().ToLower().Equals(currentLoad.Trim().ToLower())).OrderBy(m => m.ReadOrder).ToList();
                            int oReadOrder = 0;
                            foreach (var oImportField in pImportField)
                            {
                                oReadOrder = oReadOrder + 1;
                                oImportField.ReadOrder = (short?)oReadOrder;
                                _iImportField.Update(oImportField);
                            }
                            lstImportFields = _iImportField.Where(m => m.ImportLoad.Trim().ToLower().Equals(currentLoad.Trim().ToLower())).ToList();
                            //TempData["ImportFields"] = lstImportFields;
                            TempData.Set<List<ImportField>>("ImportFields", lstImportFields);

                        }
                    }

                }
                if (!tableList.Contains(new KeyValuePair<string, string>(SKIP_FIELD, SKIP_FIELD)))
                {
                    tableList.Add(new KeyValuePair<string, string>(SKIP_FIELD, SKIP_FIELD));
                }

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                tableListJSON = JsonConvert.SerializeObject(tableList, Formatting.Indented, Setting);
                flagImportByJSON = JsonConvert.SerializeObject(flagImportBy, Formatting.Indented, Setting);
                flagOverwriteAddJSON = JsonConvert.SerializeObject(flagOverwriteAdd, Formatting.Indented, Setting);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = "";
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = "";
            }
            return Json(new
            {
                tableListJSON,
                flagImportByJSON,
                flagOverwriteAddJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        //[ValidateInput(false)]
        public object ValidateOnMoveClick(ImportField serializedForm, string currentLoad, string AvailableVal1, string SelectObjString)
        {
            do
            {
                try
                {
                    string mAddInSelectList = "";
                    bool bHasImageCopy = false;
                    bool bHasPCFileCopy = false;
                    string AvailableVal = JsonConvert.DeserializeObject<object>(AvailableVal1).ToString();
                    if (!string.IsNullOrEmpty(AvailableVal))
                    {
                        //object SelectObjArray = new JavaScriptSerializer().Deserialize<object>(SelectObjString);
                        object SelectObjArray = JsonConvert.DeserializeObject<object>(SelectObjString);

                        Keys.ErrorType = "s";
                        if (AvailableVal.Trim().ToUpper().Equals(ATTMT_LINK.Trim().ToUpper()))
                        {
                            bool exitTry = false;
                            bool exitTry1 = false;
                            bool exitTry2 = false;
                            foreach (string Str in (IEnumerable)SelectObjArray)
                            {
                                if (Str.Trim().ToUpper().Equals(ATTMT_LINK.Trim().ToUpper()))
                                {
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlAlreadySelected"), ATTMT_LINK);
                                    Keys.ErrorType = "w";
                                    exitTry = true;
                                    break;
                                }
                                else if (Str.Trim().ToUpper().Equals(IMAGE_COPY.Trim().ToUpper()))
                                {
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlNotImportTheSame"), IMAGE_COPY, ATTMT_LINK);
                                    Keys.ErrorType = "w";
                                    exitTry1 = true;
                                    break;
                                }
                                else if (Str.Trim().ToUpper().Equals(PCFILE_COPY.Trim().ToUpper()))
                                {
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlNotImportTheSame"), PCFILE_COPY, ATTMT_LINK);
                                    Keys.ErrorType = "w";
                                    exitTry2 = true;
                                    break;
                                }
                            }

                            if (exitTry)
                            {
                                break;
                            }

                            if (exitTry1)
                            {
                                break;
                            }

                            if (exitTry2)
                            {
                                break;
                            }
                        }

                        if (AvailableVal.Trim().ToUpper().Equals(IMAGE_COPY.Trim().ToUpper()))
                        {
                            bool exitTry3 = false;
                            bool exitTry4 = false;
                            bool exitTry5 = false;
                            foreach (string Str in (IEnumerable)SelectObjArray)
                            {
                                if (Str.Trim().ToUpper().Equals(IMAGE_COPY.Trim().ToUpper()))
                                {
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlAlreadySelected"), IMAGE_COPY);
                                    Keys.ErrorType = "w";
                                    exitTry3 = true;
                                    break;
                                }
                                else if (Str.Trim().ToUpper().Equals(ATTMT_LINK.Trim().ToUpper()))
                                {
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlNotImportTheSame"), ATTMT_LINK, IMAGE_COPY);
                                    Keys.ErrorType = "w";
                                    exitTry4 = true;
                                    break;
                                }
                                else if (Str.Trim().ToUpper().Equals(PCFILE_COPY.Trim().ToUpper()))
                                {
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlNotImportTheSame"), PCFILE_COPY, IMAGE_COPY);
                                    Keys.ErrorType = "w";
                                    exitTry5 = true;
                                    break;
                                }
                            }

                            if (exitTry3)
                            {
                                break;
                            }

                            if (exitTry4)
                            {
                                break;
                            }

                            if (exitTry5)
                            {
                                break;
                            }
                        }

                        if (AvailableVal.Trim().ToUpper().Equals(PCFILE_COPY.Trim().ToUpper()))
                        {
                            bool exitTry6 = false;
                            bool exitTry7 = false;
                            bool exitTry8 = false;
                            foreach (string Str in (IEnumerable)SelectObjArray)
                            {
                                if (Str.Trim().ToUpper().Equals(PCFILE_COPY.Trim().ToUpper()))
                                {
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlAlreadySelected"), PCFILE_COPY);
                                    Keys.ErrorType = "w";
                                    exitTry6 = true;
                                    break;
                                }
                                else if (Str.Trim().ToUpper().Equals(ATTMT_LINK.Trim().ToUpper()))
                                {
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlNotImportTheSame"), ATTMT_LINK, PCFILE_COPY);
                                    Keys.ErrorType = "w";
                                    exitTry7 = true;
                                    break;
                                }
                                else if (Str.Trim().ToUpper().Equals(IMAGE_COPY.Trim().ToUpper()))
                                {
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlNotImportTheSame"), IMAGE_COPY, PCFILE_COPY);
                                    Keys.ErrorType = "w";
                                    exitTry8 = true;
                                    break;
                                }
                            }

                            if (exitTry6)
                            {
                                break;
                            }

                            if (exitTry7)
                            {
                                break;
                            }

                            if (exitTry8)
                            {
                                break;
                            }
                        }

                        if (AvailableVal.Trim().ToUpper().Equals(TRACK_DEST.Trim().ToUpper()))
                        {
                            bool exitTry9 = false;
                            foreach (string Str in (IEnumerable)SelectObjArray)
                            {
                                if (Str.Trim().ToUpper().Equals(TRACK_DEST.Trim().ToUpper()))
                                {
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlAlreadySelected"), TRACK_DEST);
                                    Keys.ErrorType = "w";
                                    exitTry9 = true;
                                    break;
                                }
                            }

                            if (exitTry9)
                            {
                                break;
                            }
                        }
                        AddMemberToTempData(currentLoad, AvailableVal);
                    }
                }
                catch (Exception ex)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = ex.Message;
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public void AddMemberToTempData(string currentLoad, string addNewField)
        {
            try
            {
                if (!TempData.ContainsKey("ImportFields"))
                {
                    var lstImportField = new List<ImportField>();
                    //TempData["ImportFields"] = lstImportField;
                    TempData.Set<List<ImportField>>("ImportFields", lstImportField);
                }
                var tempImportFields = new List<ImportField>();
                //tempImportFields = (List<ImportField>)TempData.Peek("ImportFields");
                tempImportFields = TempData.GetPeek<List<ImportField>>("ImportFields");
                var sImportField = new ImportField();
                int readOrder;
                if (tempImportFields.Count == 0)
                {
                    readOrder = 1;
                }
                else
                {
                    readOrder = (int)(tempImportFields.Max(x => x.ReadOrder) + 1);
                }
                sImportField = SaveImportFields(currentLoad, addNewField, readOrder);
                tempImportFields.Add(sImportField);
                //TempData["ImportFields"] = tempImportFields;
                TempData.Set<List<ImportField>>("ImportFields", tempImportFields);

            }
            catch (Exception)
            {

            }
        }

        public ImportField SaveImportFields(string loadName, string addNewField, int readOrder, bool mbHandheldLoad = false, bool optDensoImport = false)
        {
            try
            {
                var sImportField = new ImportField();
                if (mbHandheldLoad)
                {
                    if (Strings.StrComp(addNewField, TRACK__DUE, Constants.vbTextCompare) == 0L)
                    {
                        if (optDensoImport)
                        {
                            sImportField.DateFormat = "mmddyyyy";
                        }
                        else
                        {
                            sImportField.DateFormat = "mm/dd/yyyy";
                        }
                    }
                    else if (Strings.StrComp(addNewField, TRACK_DATE, Constants.vbTextCompare) == 0L)
                    {
                        if (optDensoImport)
                        {
                            sImportField.DateFormat = "mmddyyyyhhmmss";
                        }
                        else
                        {
                            sImportField.DateFormat = "mm/dd/yyyy hh:mm:ss";
                        }
                    }
                }
                sImportField.FieldName = addNewField;
                sImportField.ImportLoad = loadName;
                sImportField.ReadOrder = (short?)readOrder;
                sImportField.SwingYear = (int?)29L;
                return sImportField;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [HttpPost]
        // [ValidateInput(false)]
        public object RemoveOnClick(bool removeAll, string SelectObject1 = "", string arrSerialized = "")
        {
            try
            {
                if (removeAll)
                {
                    RemoveMemberFromTempData();
                    Keys.ErrorType = "s";
                }
                else
                {
                    string SelectObject = JsonConvert.DeserializeObject<object>(SelectObject1).ToString();
                    if (!string.IsNullOrEmpty(SelectObject))
                    {
                        Keys.ErrorType = "s";
                        object SelectObjArray = JsonConvert.DeserializeObject<object>(arrSerialized);
                        RemoveMemberFromTempData(SelectObject);
                    }
                }
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message;
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public void RemoveMemberFromTempData(string SelectObject = null)
        {
            try
            {
                if (TempData.ContainsKey("ImportFields"))
                {
                    var lstImportFields = new List<ImportField>();
                    //lstImportFields = (List<ImportField>)TempData.Peek("ImportFields");
                    lstImportFields = TempData.GetPeek<List<ImportField>>("ImportFields");
                    if (lstImportFields is not null)
                    {
                        lstImportFields = lstImportFields.OrderBy(m => m.ReadOrder).ToList();
                        if (SelectObject is not null)
                        {
                            var sImportField = lstImportFields.Where(m => m.FieldName.Trim().ToUpper().Equals(SelectObject.Trim().ToUpper())).FirstOrDefault();
                            var currentOrder = sImportField.ReadOrder;
                            lstImportFields.Remove(sImportField);
                            foreach (ImportField pImportFieldObj in lstImportFields)
                            {
                                if (pImportFieldObj.ReadOrder > currentOrder & pImportFieldObj.ReadOrder != currentOrder)
                                {
                                    pImportFieldObj.ReadOrder = currentOrder;
                                    currentOrder = (short?)(currentOrder + 1);
                                }
                            }
                            //TempData["ImportFields"] = lstImportFields;
                            TempData.Set<List<ImportField>>("ImportFields", lstImportFields);
                        }
                        else
                        {
                            TempData.Remove("ImportFields");
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [HttpPost]
        //[ValidateInput(false)]
        public JsonResult ReorderImportField(string selValue, int increment, int selIndex)
        {
            try
            {
                if (TempData.ContainsKey("ImportFields"))
                {
                    var lstImportFields = new List<ImportField>();
                    //lstImportFields = (List<ImportField>)TempData.Peek("ImportFields");
                    lstImportFields = TempData.GetPeek<List<ImportField>>("ImportFields");
                    var actualVal = default(int);
                    var updateVal = default(int);
                    var selValue1 = JsonConvert.DeserializeObject<object>(selValue).ToString();
                    var pImportField = lstImportFields.Where(m => (bool)Operators.AndObject(m.FieldName.Trim().ToUpper().Equals(selValue1.Trim().ToUpper()), Operators.ConditionalCompareObjectEqual(m.ReadOrder, selIndex + 1, false))).FirstOrDefault();
                    var reOrderVal = pImportField.ReadOrder;

                    if (increment == -1)
                    {
                        updateVal = (int)(reOrderVal - 1);
                        actualVal = (int)reOrderVal;
                    }
                    else if (increment == 1)
                    {
                        updateVal = (int)(reOrderVal + 1);
                        actualVal = (int)reOrderVal;
                    }
                    var qImportField = lstImportFields.Where(m => Operators.ConditionalCompareObjectEqual(m.ReadOrder, updateVal, false)).FirstOrDefault();
                    lstImportFields.Remove(pImportField);
                    lstImportFields.Remove(qImportField);
                    pImportField.ReadOrder = (short?)updateVal;
                    qImportField.ReadOrder = (short?)actualVal;
                    lstImportFields.Add(pImportField);
                    lstImportFields.Add(qImportField);
                    //TempData["ImportFields"] = lstImportFields;
                    TempData.Set<List<ImportField>>("ImportFields", lstImportFields);
                }
                Keys.ErrorMessage = "";
                Keys.ErrorType = "s";
            }
            catch (Exception ex)
            {
                Keys.ErrorMessage = ex.Message;
                Keys.ErrorType = "e";
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        // [ValidateInput(false)]
        public JsonResult GetPropertyByType(string sFieldName1, string sTableName1)
        {
            IRepository<Table> _iTables = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            string pImportFieldJSON = "";
            string bAttachmentLinkJSON = "";
            string fieldIsDateJSON = "";
            try
            {
                bool bAttachmentLink;
                bool fieldIsDate;
                Table oTable;
                string sTableName = JsonConvert.DeserializeObject<object>(sTableName1).ToString();
                string sFieldName = JsonConvert.DeserializeObject<object>(sFieldName1).ToString();
                ADODB.Connection sAdoCon = DataServices.DBOpen();
                List<TabFusionRMS.WebCS.SchemaColumns> oSchemaColumn;
                if (TempData.ContainsKey("ImportFields"))
                {
                    var lstImportFields = new List<ImportField>();
                    //lstImportFields = (List<ImportField>)TempData.Peek("ImportFields");
                    lstImportFields = TempData.GetPeek<List<ImportField>>("ImportFields");
                    var pImportField = lstImportFields.Where(m => m.FieldName.Trim().ToUpper().Equals(sFieldName.Trim().ToUpper())).FirstOrDefault();
                    if (pImportField is not null)
                    {
                        bAttachmentLink = Strings.StrComp(sFieldName, ATTMT_LINK, Constants.vbBinaryCompare) == 0L;
                        if (!sFieldName.Trim().ToLower().Equals(IMPORT_TRACK.Trim().ToLower()) & Strings.InStr(NON_FIELDS, "|" + sFieldName + "|") == 0)
                        {
                            oTable = _iTables.All().Where(m => m.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())).FirstOrDefault();
                            if (oTable is not null)
                            {
                                var oDatabase = _iDatabas.All().Where(m => m.DBName.Trim().ToLower().Equals(oTable.DBName.Trim().ToLower())).FirstOrDefault();
                                sAdoCon = DataServices.DBOpen(oDatabase);
                            }
                            oSchemaColumn = SchemaInfoDetails.GetSchemaInfo(oTable.TableName, sAdoCon, DatabaseMap.RemoveTableNameFromField(sFieldName));
                            if (oSchemaColumn is not null)
                            {
                                fieldIsDate = oSchemaColumn[0].IsADate;
                            }
                            else
                            {
                                fieldIsDate = false;
                            }
                        }
                        else
                        {
                            fieldIsDate = Convert.ToBoolean(Strings.InStr("|" + TRACK__DUE + TRACK_DATE + "|", sFieldName.ToString()));
                        }
                        var Setting = new JsonSerializerSettings();
                        Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                        pImportFieldJSON = JsonConvert.SerializeObject(pImportField, Formatting.Indented, Setting);
                        bAttachmentLinkJSON = JsonConvert.SerializeObject(bAttachmentLink, Formatting.Indented, Setting);
                        fieldIsDateJSON = JsonConvert.SerializeObject(fieldIsDate, Formatting.Indented, Setting);
                    }
                }
                Keys.ErrorMessage = "";
                Keys.ErrorType = "s";
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message;
            }
            return Json(new
            {
                pImportFieldJSON,
                bAttachmentLinkJSON,
                fieldIsDateJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        // [ValidateInput(false)]
        public object SaveImportProperties(ImportField formEntity, string sFieldName1)
        {
            try
            {
                if (TempData.ContainsKey("ImportFields"))
                {
                    var lstImportFields = new List<ImportField>();
                    //lstImportFields = (List<ImportField>)TempData.Peek("ImportFields");
                    lstImportFields = TempData.GetPeek<List<ImportField>>("ImportFields");
                    string sFieldName = JsonConvert.DeserializeObject<object>(sFieldName1).ToString();
                    var pImportField = lstImportFields.Where(m => m.FieldName.Trim().ToUpper().Equals(sFieldName.Trim().ToUpper())).FirstOrDefault();
                    lstImportFields.Remove(pImportField);
                    if (pImportField is not null)
                    {
                        if (formEntity.DateFormat is not null)
                        {
                            pImportField.DateFormat = formEntity.DateFormat;
                        }
                        if (formEntity.DefaultValue is not null)
                        {
                            pImportField.DefaultValue = formEntity.DefaultValue;
                        }
                        else
                        {
                            pImportField.DefaultValue = null;
                        }
                        if (formEntity.EndPosition is not null)
                        {
                            pImportField.EndPosition = formEntity.EndPosition;
                        }
                        if (formEntity.StartPosition is not null)
                        {
                            pImportField.StartPosition = formEntity.StartPosition;
                        }
                        if (formEntity.ImportLoad is not null)
                        {
                            pImportField.ImportLoad = formEntity.ImportLoad;
                        }
                        if (formEntity.ReadOrder is not null)
                        {
                            pImportField.ReadOrder = formEntity.ReadOrder;
                        }
                        if (formEntity.SwingYear is not null)
                        {
                            pImportField.SwingYear = formEntity.SwingYear;
                        }
                        lstImportFields.Add(pImportField);

                        TempData.Set<List<ImportField>>("ImportFields", lstImportFields);
                    }
                }
                Keys.ErrorType = "s";
                Keys.ErrorMessage = "";
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message;
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public JsonResult CloseAllObject()
        {
            try
            {
                if (TempData is not null)
                {
                    if (TempData.ContainsKey("FileDataTable"))
                    {
                        TempData.Remove("FileDataTable");
                    }
                    if (TempData.ContainsKey("ImportLoad"))
                    {
                        TempData.Remove("ImportLoad");
                    }
                    if (TempData.ContainsKey("ImportFields"))
                    {
                        TempData.Remove("ImportFields");
                    }
                    if (TempData.ContainsKey("ImportJobs"))
                    {
                        TempData.Remove("ImportJobs");
                    }
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = "";
                }
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message;
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        //  [ValidateInput(false)]
        public JsonResult LoadTabInfoDDL(string tableName1)
        {
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            IRepository<Table> _iTables = new Repositories<Table>();
            string OverwriteListJSON = "";
            string ImportByListJSON = "";
            string ImporLoadJSON = "";
            string DateDueJSON = "";
            string DisplayDateJSON = "";
            string ShowImageTabJSON = "";
            try
            {
                string tableName = JsonConvert.DeserializeObject<object>(tableName1).ToString();
                var otable = _iTables.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                var oImportLoad = new ImportLoad();
                var OverwriteList = new List<KeyValuePair<string, string>>();
                var ImportByList = new List<KeyValuePair<string, string>>();
                bool DateDue;
                string oIdFieldName = null;
                bool ShowImageTab = false;
                OverwriteList.Add(new KeyValuePair<string, string>(BOTH_DB_TEXT, BOTH_DISPLAY));
                OverwriteList.Add(new KeyValuePair<string, string>(OVERWRITE_DB_TEXT, OVERWRITE_DISPLAY));
                OverwriteList.Add(new KeyValuePair<string, string>(NEW_DB_TEXT, NEW_DISPLAY));
                OverwriteList.Add(new KeyValuePair<string, string>(NEW_SKIP_DB_TEXT, NEW_SKIP_DISPLAY));
                ImportByList.Add(new KeyValuePair<string, string>("0", IMPORTBY_NONE));
                if (TempData.ContainsKey("ImportLoad"))
                {
                    //oImportLoad = (ImportLoad)TempData.Peek("ImportLoad");
                    oImportLoad = TempData.GetPeek<ImportLoad>("ImportLoad");
                    oIdFieldName = oImportLoad.IdFieldName;
                }
                if (TempData.ContainsKey("ImportFields"))
                {
                    var lstImportFields = new List<ImportField>();
                    //lstImportFields = (List<ImportField>)TempData.Peek("ImportFields");
                    lstImportFields = TempData.GetPeek<List<ImportField>>("ImportFields");
                    foreach (ImportField sImportField in lstImportFields)
                    {
                        if (Strings.InStr(NON_FIELDS, "|" + sImportField.FieldName + "|") == 0)
                        {
                            ImportByList.Add(new KeyValuePair<string, string>(sImportField.FieldName, sImportField.FieldName));
                            if (oImportLoad is not null)
                            {
                                if (oImportLoad.ID == 0)
                                {
                                    if (otable is not null)
                                    {
                                    }
                                    // Fixed - FUS-5848 by Nikunj
                                    // If (StrComp(sImportField.FieldName, DatabaseMap.RemoveTableNameFromField(otable.IdFieldName), vbTextCompare) = 0) Then
                                    // oIdFieldName = sImportField.FieldName
                                    // Exit For
                                    // End If
                                    else if (tableName.Trim().ToUpper().Equals("SLRETENTIONCODES") | tableName.Trim().ToUpper().Equals("SLRETENTIONCITACODES"))
                                    {
                                        if (Strings.StrComp(sImportField.FieldName, "Id", Constants.vbTextCompare) == 0)
                                        {
                                            oIdFieldName = sImportField.FieldName;
                                            break;
                                        }
                                    }
                                    else if (tableName.Trim().ToUpper().Equals("SLRETENTIONCITATIONS"))
                                    {
                                        if (Strings.StrComp(sImportField.FieldName, "Citation", Constants.vbTextCompare) == 0)
                                        {
                                            oIdFieldName = sImportField.FieldName;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (ImportField sImportField in lstImportFields)
                    {
                        if (Strings.InStr(IMAGE_FIELDS, "|" + sImportField.FieldName + "|") != 0)
                        {
                            ShowImageTab = true;
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(tableName))
                {
                    if (oImportLoad is not null)
                    {
                        oImportLoad.FileName = tableName;
                        oImportLoad.IdFieldName = oIdFieldName;
                        //TempData["ImportLoad"] = oImportLoad;
                        TempData.Set<ImportLoad>("ImportLoad", oImportLoad);
                    }
                }
                DateDue = (bool)_iSystem.All().FirstOrDefault().DateDueOn;
                string DisplayDateStr = "";
                if (oImportLoad.DateDue is not null)
                {
                    DateTime DisplayDate = default;
                    DisplayDate = (DateTime)oImportLoad.DateDue;
                    DisplayDateStr = Keys.get_ConvertCultureDate(DisplayDate, httpContext);
                }

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                DisplayDateJSON = JsonConvert.SerializeObject(DisplayDateStr, Formatting.Indented, Setting);
                OverwriteListJSON = JsonConvert.SerializeObject(OverwriteList, Formatting.Indented, Setting);
                ImportByListJSON = JsonConvert.SerializeObject(ImportByList, Formatting.Indented, Setting);
                ImporLoadJSON = JsonConvert.SerializeObject(oImportLoad, Formatting.Indented, Setting);
                DateDueJSON = JsonConvert.SerializeObject(DateDue, Formatting.Indented, Setting);
                ShowImageTabJSON = JsonConvert.SerializeObject(ShowImageTab, Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = "";
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message;
            }
            return Json(new
            {
                OverwriteListJSON,
                ImportByListJSON,
                ImporLoadJSON,
                DateDueJSON,
                DisplayDateJSON,
                ShowImageTabJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public void AddInImportLoad(ref ImportLoad dbImportLoad, ImportLoad serializedForm)
        {
            try
            {
                dbImportLoad.ID = serializedForm.ID;
                dbImportLoad.IdFieldName = serializedForm.IdFieldName;
                // dbImportLoad.InputFile = serializedForm.InputFile
                dbImportLoad.TempInputFile = serializedForm.TempInputFile;
                dbImportLoad.TableSheetName = serializedForm.TableSheetName;
                dbImportLoad.TrackDestinationId = serializedForm.TrackDestinationId;
                dbImportLoad.LoadName = serializedForm.LoadName;
                dbImportLoad.RecordType = serializedForm.RecordType;
                dbImportLoad.Delimiter = serializedForm.Delimiter;
                dbImportLoad.Duplicate = serializedForm.Duplicate;
                dbImportLoad.ReverseOrder = serializedForm.ReverseOrder;
                dbImportLoad.ScanRule = serializedForm.ScanRule;
                dbImportLoad.DirectFromHandheld = 0;
                dbImportLoad.FromHandHeldEnum = 0;
                dbImportLoad.DatabaseName = serializedForm.DatabaseName;
                dbImportLoad.DateDue = serializedForm.DateDue;
                dbImportLoad.FileName = serializedForm.FileName;
                dbImportLoad.FirstRowHeader = serializedForm.FirstRowHeader;
                dbImportLoad.SaveImageAsNewVersion = serializedForm.SaveImageAsNewVersion;
                dbImportLoad.SaveImageAsNewVersionAsOfficialRecord = serializedForm.SaveImageAsNewVersionAsOfficialRecord;
                dbImportLoad.SaveImageAsNewPage = serializedForm.SaveImageAsNewPage;
                dbImportLoad.MaxDupCount = null;
                dbImportLoad.UpdateParent = Convert.ToBoolean(0);
                dbImportLoad.DifferentImagePath = false;
                dbImportLoad.RecordLength = serializedForm.RecordLength;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        [HttpPost]
        //  [ValidateInput(false)]
        public JsonResult GetImportLoadData(string loadName)
        {
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            IRepository<ImportField> _iImportField = new Repositories<ImportField>();
            bool exceptionFlag = false;
            string oImportLoadJSON = "";
            string extensionJSON = "";
            string sheetTableListJSON = "";
            string SheetTypeJSON = "";
            string InputFileNameJSON = "";
            try
            {
                var oImportLoad = new ImportLoad();
                string extension = "";
                string UploadedFilePath = "";
                var sheetTableList = new List<KeyValuePair<string, string>>();
                string SheetType = "";
                string InputFileName = "";
                if (!string.IsNullOrEmpty(loadName))
                {
                    if (TempData.ContainsKey("ImportLoad"))
                    {
                        TempData.Remove("ImportLoad");
                    }

                    oImportLoad = _iImportLoad.Where(m => m.LoadName.Trim().ToLower().Equals(loadName.Trim().ToLower())).FirstOrDefault();
                    if (oImportLoad is not null)
                    {
                        //TempData["ImportLoad"] = oImportLoad;
                        TempData.Set<ImportLoad>("ImportLoad", oImportLoad);
                        if (TempData.ContainsKey("ImportFields"))
                        {
                            TempData.Remove("ImportFields");
                        }
                        var lstImportField = new List<ImportField>();
                        if (!string.IsNullOrEmpty(loadName))
                        {
                            lstImportField = _iImportField.All().Where(m => m.ImportLoad.Trim().ToLower().Equals(loadName.Trim().ToLower())).OrderBy(m => m.ReadOrder).ToList();
                        }
                        //TempData["ImportFields"] = lstImportField;
                        TempData.Set<List<ImportField>>("ImportFields", lstImportField);
                        if (oImportLoad.FromHandHeldEnum == 0 | oImportLoad.FromHandHeldEnum is null && !string.IsNullOrEmpty(oImportLoad.TempInputFile))
                        {
                            extension = Path.GetExtension(oImportLoad.TempInputFile);
                            InputFileName = Path.GetFileName(oImportLoad.TempInputFile);

                            if (string.IsNullOrEmpty(extension))
                            {
                                extension = ".csv";
                            }
                            UploadedFilePath = oImportLoad.TempInputFile.Trim();
                            if (System.IO.File.Exists(UploadedFilePath))
                            {
                                FindSheetType(extension.Trim().ToLower(), UploadedFilePath, ref sheetTableList, ref SheetType, ref exceptionFlag);
                                extension = extension.Replace(".", "");
                                Keys.ErrorMessage = "";
                                Keys.ErrorType = "s";
                            }
                            else
                            {
                                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlFileNotFound"), Path.GetFileName(UploadedFilePath));
                                Keys.ErrorType = "p";
                            }
                        }
                        else if (oImportLoad.FromHandHeldEnum != 0)
                        {
                            Keys.ErrorMessage = Languages.get_Translation("msgImportCtrlNeed2AcquireData");
                            Keys.ErrorType = "w";
                        }
                        else if (string.IsNullOrEmpty(oImportLoad.TempInputFile))
                        {
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlFileNotFound"), "");
                            Keys.ErrorType = "p";
                        }
                    }
                }
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                oImportLoadJSON = JsonConvert.SerializeObject(oImportLoad, Formatting.Indented, Setting);
                extensionJSON = JsonConvert.SerializeObject(extension, Formatting.Indented, Setting);
                sheetTableListJSON = JsonConvert.SerializeObject(sheetTableList, Formatting.Indented, Setting);
                SheetTypeJSON = JsonConvert.SerializeObject(SheetType, Formatting.Indented, Setting);
                InputFileNameJSON = JsonConvert.SerializeObject(InputFileName, Formatting.Indented, Setting);
            }
            catch (Exception ex)
            {
                if (exceptionFlag == true)
                {
                    Keys.ErrorMessage = Languages.get_Translation("msgImportCtrlProviderNotFound");
                }
                else
                {
                    Keys.ErrorMessage = ex.Message;
                }
                Keys.ErrorType = "e";
            }
            return Json(new
            {
                oImportLoadJSON,
                extensionJSON,
                sheetTableListJSON,
                SheetTypeJSON,
                InputFileNameJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        // [ValidateInput(false)]
        public JsonResult SaveImportLoadOnConfrim(ImportLoad serializedForm, string strmsHoldName, string RecordType, bool pFirstRowHeader, string Delimiter, string TableSheetName, bool pReverseOrder, int SaveAsNewVal)
        {
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            IRepository<ImportField> _iImportField = new Repositories<ImportField>();
            try
            {
                var oImportLoad = new ImportLoad();
                if (TempData.ContainsKey("ImportLoad"))
                {
                    //oImportLoad = (ImportLoad)TempData.Peek("ImportLoad");
                    oImportLoad = TempData.GetPeek<ImportLoad>("ImportLoad");
                    oImportLoad.ID = serializedForm.ID;
                    if (serializedForm.LoadName != "")
                    {
                        oImportLoad.LoadName = serializedForm.LoadName;
                    }
                    if (RecordType.Trim().ToUpper().Equals("COMMA"))
                    {
                        if (string.IsNullOrWhiteSpace(Delimiter))
                        {
                            oImportLoad.Delimiter = null;
                        }
                        else
                        {
                            oImportLoad.Delimiter = Delimiter;
                        }
                    }

                    // If (Not String.IsNullOrEmpty(serializedForm.InputFile)) Then
                    // oImportLoad.InputFile = HttpUtility.UrlDecode(serializedForm.InputFile)
                    // End If

                    if (!string.IsNullOrEmpty(serializedForm.TempInputFile))
                    {
                        oImportLoad.TempInputFile = System.Net.WebUtility.HtmlDecode(serializedForm.TempInputFile);
                    }

                    oImportLoad.RecordType = RecordType;
                    oImportLoad.FirstRowHeader = pFirstRowHeader;
                    oImportLoad.ReverseOrder = pReverseOrder;
                    oImportLoad.RecordLength = 0;
                    if (string.IsNullOrEmpty(TableSheetName))
                    {
                        oImportLoad.TableSheetName = null;
                    }
                    else
                    {
                        oImportLoad.TableSheetName = System.Net.WebUtility.HtmlDecode(TableSheetName);
                    }

                    oImportLoad.TrackDestinationId = serializedForm.TrackDestinationId;
                    oImportLoad.DateDue = serializedForm.DateDue;

                    if (serializedForm.ScanRule is not null & !string.IsNullOrEmpty(serializedForm.ScanRule))
                    {
                        oImportLoad.ScanRule = serializedForm.ScanRule;
                    }
                    else
                    {
                        oImportLoad.ScanRule = null;
                    }
                    if (!string.IsNullOrEmpty(serializedForm.Duplicate))
                    {
                        oImportLoad.Duplicate = serializedForm.Duplicate;
                    }
                    else
                    {
                        oImportLoad.Duplicate = BOTH_DB_TEXT;
                    }

                    if (string.IsNullOrEmpty(serializedForm.IdFieldName) | serializedForm.IdFieldName == "0")
                    {
                        oImportLoad.IdFieldName = null;
                    }
                    else
                    {
                        oImportLoad.IdFieldName = serializedForm.IdFieldName;
                    }

                    switch (SaveAsNewVal)
                    {
                        case 0:
                            {
                                oImportLoad.SaveImageAsNewPage = false;
                                oImportLoad.SaveImageAsNewVersion = false;
                                oImportLoad.SaveImageAsNewVersionAsOfficialRecord = false;
                                break;
                            }
                        case 1:
                            {
                                oImportLoad.SaveImageAsNewPage = true;
                                oImportLoad.SaveImageAsNewVersion = false;
                                oImportLoad.SaveImageAsNewVersionAsOfficialRecord = false;
                                break;
                            }
                        case 2:
                            {
                                oImportLoad.SaveImageAsNewVersion = true;
                                oImportLoad.SaveImageAsNewPage = false;
                                oImportLoad.SaveImageAsNewVersionAsOfficialRecord = false;
                                break;
                            }
                        case 3:
                            {
                                oImportLoad.SaveImageAsNewVersionAsOfficialRecord = true;
                                oImportLoad.SaveImageAsNewPage = false;
                                oImportLoad.SaveImageAsNewVersion = false;
                                break;
                            }
                    }
                    if (oImportLoad.FromHandHeldEnum is null | oImportLoad.FromHandHeldEnum == 0)
                    {
                        oImportLoad.DatabaseName = passport.DatabaseName;
                    }
                    else
                    {
                        oImportLoad.DatabaseName = oImportLoad.InputFile;
                    }
                }

                // If (Not String.IsNullOrEmpty(oImportLoad.InputFile)) Then
                if (!(httpContext.Session.GetString("ImportFilePath") == null))
                {
                    // Dim FoldersArray = oImportLoad.InputFile.Split(Path.DirectorySeparatorChar)
                    var FoldersArray = (httpContext.Session.GetString("ImportFilePath") ?? "").Split(Path.DirectorySeparatorChar);
                    var oUserName = (httpContext.Session.GetString("UserName") ?? "").Trim().ToLower();
                    var oldDir = Path.GetDirectoryName((httpContext.Session.GetString("ImportFilePath") ?? ""));
                    var newDir = Path.Combine("~/ImportFiles/" + oImportLoad.LoadName.Trim().ToLower() + "/" + oUserName + "/" + FoldersArray[FoldersArray.Length - 2]);
                    if (!oldDir.Equals(newDir))
                    {
                        if (Directory.Exists((httpContext.Session.GetString("ImportFilePath") ?? "")))
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(oldDir, newDir);
                            var oTemp = Path.Combine("~/ImportFiles/" + oUserName);
                            string oTempSub = Path.Combine(oTemp, oUserName);
                            if (Directory.Exists(oTempSub))
                            {
                                foreach (string _folder in Directory.GetDirectories(oTempSub))
                                {
                                    if (_folder.Substring(_folder.Length - 10).Equals("Attachment"))
                                    {
                                        var attachDir = Path.Combine("~/ImportFiles/" + oImportLoad.LoadName.Trim().ToLower() + "/" + oUserName + "/Attachment");
                                        Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(_folder, attachDir);
                                    }
                                    else
                                    {
                                        foreach (string _file in Directory.GetFiles(_folder))
                                            System.IO.File.Delete(_file);
                                        Directory.Delete(_folder);
                                    }
                                }
                            }
                            if (Directory.Exists(oTempSub))
                            {
                                Directory.Delete(oTempSub);
                            }
                            if (Directory.Exists(oTemp))
                            {
                                Directory.Delete(oTemp);
                            }
                            // oImportLoad.InputFile = Path.Combine(newDir, FoldersArray(FoldersArray.Length - 1))
                            oImportLoad.TempInputFile = Path.Combine(newDir, FoldersArray[FoldersArray.Length - 1]);
                        }
                    }
                }

                if (oImportLoad.ID >= (Int32)1)
                {
                    var dbImportLoad = _iImportLoad.All().Where(m => m.ID == oImportLoad.ID).FirstOrDefault();
                    var dbImportFields = _iImportField.All().Where(m => m.ImportLoad.Trim().ToLower().Equals(dbImportLoad.LoadName.Trim().ToLower()));
                    if (dbImportFields is not null)
                    {
                        _iImportField.DeleteRange(dbImportFields);
                    }
                    AddInImportLoad(ref dbImportLoad, oImportLoad);
                    _iImportLoad.Update(dbImportLoad);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgModifiedImport");
                }
                else
                {
                    oImportLoad.DirectFromHandheld = 0;
                    oImportLoad.FromHandHeldEnum = 0;
                    oImportLoad.FileName = serializedForm.FileName;
                    oImportLoad.UpdateParent = Convert.ToBoolean(0);
                    oImportLoad.DifferentImagePath = false;
                    oImportLoad.MaxDupCount = null;
                    _iImportLoad.Add(oImportLoad);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgNewImportAdded");
                }
                if (TempData.ContainsKey("ImportFields"))
                {
                    //List<ImportField> lstImportFields = (List<ImportField>)TempData.Peek("ImportFields");
                    List<ImportField> lstImportFields = TempData.GetPeek<List<ImportField>>("ImportFields");
                    foreach (ImportField importFields in lstImportFields)
                    {
                        importFields.ImportLoad = oImportLoad.LoadName;
                        _iImportField.Add(importFields);
                    }
                    TempData.Remove("ImportFields");
                }
                if (TempData.ContainsKey("ImportLoad"))
                {
                    TempData.Remove("ImportLoad");
                }
                if (TempData.ContainsKey("FileDataTable"))
                {
                    TempData.Remove("FileDataTable");
                }
            }
            catch (Exception ex)
            {
                Keys.ErrorMessage = ex.Message;
                Keys.ErrorType = "e";
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        // [ValidateInput(false)]
        public JsonResult SetTrackingInfo(ImportLoad serializedForm, bool pReverseOrder)
        {
            try
            {
                if (TempData.ContainsKey("ImportLoad"))
                {
                    //ImportLoad oImportLoad = (ImportLoad)TempData.Peek("ImportLoad");
                    ImportLoad oImportLoad = TempData.GetPeek<ImportLoad>("ImportLoad");
                    oImportLoad.TrackDestinationId = serializedForm.TrackDestinationId;
                    oImportLoad.DateDue = serializedForm.DateDue;
                    oImportLoad.Duplicate = serializedForm.Duplicate;
                    oImportLoad.IdFieldName = serializedForm.IdFieldName;
                    oImportLoad.ReverseOrder = pReverseOrder;
                    //TempData["ImportLoad"] = oImportLoad;
                    TempData.Set<ImportLoad>("ImportLoad", oImportLoad);
                }
                Keys.ErrorMessage = "";
                Keys.ErrorType = "s";
            }
            catch (Exception ex)
            {
                Keys.ErrorMessage = ex.Message;
                Keys.ErrorType = "e";
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public JsonResult SetImageInfo(ImportLoad serializedForm, int SaveAsNewVal)
        {
            try
            {
                if (TempData.ContainsKey("ImportLoad"))
                {
                    //ImportLoad oImportLoad = (ImportLoad)TempData.Peek("ImportLoad");
                    ImportLoad oImportLoad = TempData.GetPeek<ImportLoad>("ImportLoad");
                    if (serializedForm.ScanRule is not null & !string.IsNullOrEmpty(serializedForm.ScanRule))
                    {
                        oImportLoad.ScanRule = serializedForm.ScanRule;
                    }
                    else
                    {
                        oImportLoad.ScanRule = null;
                    }
                    switch (SaveAsNewVal)
                    {
                        case 1:
                            {
                                oImportLoad.SaveImageAsNewPage = true;
                                oImportLoad.SaveImageAsNewVersion = false;
                                oImportLoad.SaveImageAsNewVersionAsOfficialRecord = false;
                                break;
                            }
                        case 2:
                            {
                                oImportLoad.SaveImageAsNewVersion = true;
                                oImportLoad.SaveImageAsNewPage = false;
                                oImportLoad.SaveImageAsNewVersionAsOfficialRecord = false;
                                break;
                            }
                        case 3:
                            {
                                oImportLoad.SaveImageAsNewVersionAsOfficialRecord = true;
                                oImportLoad.SaveImageAsNewPage = false;
                                oImportLoad.SaveImageAsNewVersion = false;
                                break;
                            }
                    }
                    //TempData["ImportLoad"] = oImportLoad;
                    TempData.Set<ImportLoad>("ImportLoad", oImportLoad);
                }
                Keys.ErrorMessage = "";
                Keys.ErrorType = "s";
            }
            catch (Exception ex)
            {
                Keys.ErrorMessage = ex.Message;
                Keys.ErrorType = "e";
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public JsonResult CheckIfInUsed(string currentLoad)
        {
            IRepository<ImportJob> _iImportJob = new Repositories<ImportJob>();
            string ActiveLoadListJSON = "";
            try
            {
                Keys.ErrorType = "w";
                var ActiveLoadList = new List<KeyValuePair<string, string>>();
                string currentJob = "";
                bool bFlag = false;
                if (!string.IsNullOrEmpty(currentLoad))
                {
                    var sImportJob = _iImportJob.All().OrderBy(m => m.JobName);
                    if (sImportJob is not null & sImportJob.Count() != 0)
                    {
                        currentJob = sImportJob.FirstOrDefault().JobName;
                    }

                    foreach (ImportJob importJobObj in sImportJob)
                    {
                        if (!string.IsNullOrEmpty(importJobObj.LoadName) & !string.IsNullOrEmpty(currentLoad))
                        {
                            if (Strings.StrComp(importJobObj.LoadName, currentLoad, Constants.vbTextCompare) == 0)
                            {
                                if (!string.IsNullOrEmpty(importJobObj.JobName) & !string.IsNullOrEmpty(currentJob))
                                {
                                    if (!bFlag | !importJobObj.JobName.Trim().ToLower().Equals(currentJob.Trim().ToLower()))
                                    {
                                        ActiveLoadList.Add(new KeyValuePair<string, string>(importJobObj.JobName, "Job"));
                                        Keys.ErrorType = "r";
                                        Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlTheImportLoad"), currentLoad);
                                        currentJob = importJobObj.JobName.Trim();
                                        bFlag = true;
                                    }
                                }
                            }
                        }
                    }
                }
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                ActiveLoadListJSON = JsonConvert.SerializeObject(ActiveLoadList, Formatting.Indented, Setting);
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
            }
            return Json(new
            {
                ActiveLoadListJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public JsonResult RemoveImportLoad(string currentLoad, string currentLoadVal)
        {
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            IRepository<ImportField> _iImportField = new Repositories<ImportField>();
            IRepository<ImportJob> _iImportJob = new Repositories<ImportJob>();
            try
            {
                if (!string.IsNullOrEmpty(currentLoad))
                {
                    if (currentLoad.Trim().Contains("[Job]"))
                    {
                        var oImportJob = _iImportJob.Where(m => m.JobName.Trim().ToLower().Equals(currentLoadVal.Trim().ToLower()));
                        if (oImportJob is not null)
                        {
                            _iImportJob.DeleteRange(oImportJob);
                            Keys.ErrorMessage = Languages.get_Translation("msgImportCtrlJobDelSuccessfully");
                            Keys.ErrorType = "s";
                        }
                    }
                    else
                    {
                        var oImportLoad = _iImportLoad.Where(m => m.LoadName.Trim().ToLower().Equals(currentLoad.Trim().ToLower())).FirstOrDefault();
                        if (oImportLoad is not null)
                        {
                            if (!string.IsNullOrEmpty(oImportLoad.LoadName))
                            {
                                var dbImportFields = _iImportField.All().Where(m => m.ImportLoad.Trim().ToLower().Equals(oImportLoad.LoadName.Trim().ToLower()));
                                if (dbImportFields is not null)
                                {
                                    _iImportField.DeleteRange(dbImportFields);
                                }
                            }
                            _iImportLoad.Delete(oImportLoad);
                            Keys.ErrorMessage = Languages.get_Translation("msgImportCtrlLoadDelSuccessfully");
                            Keys.ErrorType = "s";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Keys.ErrorMessage = ex.Message;
                Keys.ErrorType = "e";
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        //// MOTI MASHIAH
        public ADODB.Recordset GetRecordsetOfLoadFile(ImportLoad oImportLoad,
            [Optional, DefaultParameterValue(null)] ref ADODB.Connection mcnImport)
        {
            try
            {
                var mrsImport = new ADODB.Recordset();
                string connString = null;
                string extension = string.Empty;
                string LoadInputFile = string.Empty;
                ImportJob oImportLoadFromJob = default;
                bool oUseLoadInputFile = true;
                // extension = Path.GetExtension(oImportLoad.InputFile).ToLower
                // LoadInputFile = oImportLoad.InputFile
                if (!(httpContext.Session.GetString("ImportFilePath") == null))
                {
                    extension = Path.GetExtension((httpContext.Session.GetString("ImportFilePath") ?? "")).ToLower();
                    LoadInputFile = (httpContext.Session.GetString("ImportFilePath") ?? "").ToString();
                }

                string objectName = "";
                string Delimiter = "";
                if (extension.Equals(".csv") | extension.Equals(".txt") | extension.Equals(".dbf"))
                {
                    string DelimiterVal = "";
                    SetDelValBydelimiter(oImportLoad.Delimiter, ref DelimiterVal, ref Delimiter);
                    string pathName = Path.GetDirectoryName(LoadInputFile);
                    GetConnStringForOLEDB(extension, ref connString, oImportLoad.FirstRowHeader ?? false, Delimiter, pathName);
                    objectName = Path.GetFileName(LoadInputFile);
                    if (!objectName.Contains("."))
                    {
                        objectName = objectName + ".csv";
                    }

                    var TempDataTable = new DataTable();
                    // fix for tab delimited.
                    CreateINIFileForCSV(DelimiterVal, LoadInputFile, oImportLoad.FirstRowHeader ?? false, TempDataTable);
                    using (var excel_con = new OleDbConnection(connString))
                    {
                        excel_con.Open();
                        string query = Convert.ToString("SELECT TOP 2 * FROM [") + objectName.Trim() + "]";
                        using (var oda = new OleDbDataAdapter(query, excel_con))
                        {
                            oda.Fill(TempDataTable);
                        }
                    }
                    // CREAT THE INI FILE MOTI MASH
                    CreateINIFileForCSV(DelimiterVal, LoadInputFile, oImportLoad.FirstRowHeader ?? false, TempDataTable);
                }
                else
                {
                    GetConnStringForOLEDB(extension, ref connString, oImportLoad.FirstRowHeader ?? false, oImportLoad.Delimiter, LoadInputFile);
                    if (!string.IsNullOrEmpty(oImportLoad.TableSheetName))
                    {
                        objectName = oImportLoad.TableSheetName;
                    }
                    else
                    {
                        var sheetTableList = new List<KeyValuePair<string, string>>();
                        string SheetType = string.Empty;
                        bool argexceptionFlag = false;
                        FindSheetType(extension, LoadInputFile, ref sheetTableList, ref SheetType, exceptionFlag: ref argexceptionFlag);
                        if (sheetTableList is not null)
                        {
                            objectName = sheetTableList[0].Key;
                        }
                    }
                }
                if (System.IO.File.Exists(LoadInputFile))
                {
                    mcnImport = new ADODB.Connection();
                    mcnImport.ConnectionString = connString;
                    mcnImport.CursorLocation = ADODB.CursorLocationEnum.adUseClient;
                    mcnImport.Open();
                    string sSQl = Convert.ToString("SELECT * FROM [") + objectName.Trim() + "]";
                    mrsImport.Open(sSQl, mcnImport, (ADODB.CursorTypeEnum)Enums.CursorTypeEnum.rmOpenStatic, (ADODB.LockTypeEnum)Enums.LockTypeEnum.rmLockReadOnly, (int)Enums.CommandTypeEnum.rmCmdText);
                }
                return mrsImport;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void SetDelValBydelimiter(string importDelimiter, ref string DelimiterVal, ref string Delimiter)
        {
            try
            {
                switch (importDelimiter ?? "")
                {
                    case ",":
                        {
                            DelimiterVal = "CsvDelimited";
                            Delimiter = ",";
                            break;
                        }
                    case "t":
                        {
                            DelimiterVal = "TabDelimited";
                            Delimiter = Constants.vbTab;
                            break;
                        }
                    case ";":
                        {
                            DelimiterVal = "Delimited(;)";
                            Delimiter = ";";
                            break;
                        }

                    case var @case when @case == "":
                        {
                            DelimiterVal = "Delimited(" + Strings.Space(1) + ")";
                            Delimiter = Strings.Space(1);
                            break;
                        }

                    default:
                        {
                            DelimiterVal = "Delimited(" + importDelimiter.Trim() + ")";
                            Delimiter = importDelimiter.Trim();
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        private void LoadPaths(int oVolumeId, ref string sVolumePath)
        {
            string sSQL;
            ADODB.Recordset rsADO;
            string sDriveLetterField = "PhysicalDriveLetter";
            string sTemp;

            sTemp = _configuration["DriveLetterField"];

            if (sTemp is not null)
            {
                if (sTemp.Trim().Length > 0)
                {
                    sDriveLetterField = sTemp;
                }
            }
            sSQL = "SELECT  [Volumes].[Id]  as [VolumesId] ,[Volumes].[PathName] as [VolumesPath], [Volumes].[Online] as [VolumesOnline], [Volumes].[ImageTableName] as [VolumesImageTableName], [Volumes].[Name] as [VolumesName], [Volumes].[OfflineLocation] as [VolumesOfflineLocation], [SystemAddresses].[" + sDriveLetterField + "] as PhysicalDriveLetter FROM [Volumes] INNER JOIN [SystemAddresses] ON [Volumes].[SystemAddressesId] = [SystemAddresses].[Id]";
            string lError = "";
            rsADO = DataServices.GetADORecordSet(ref sSQL, DataServices.DBOpen(), ref lError);

            if (rsADO is not null)
            {
                while (!rsADO.EOF)
                {
                    if (Convert.ToInt32(rsADO.Fields["VolumesId"].Value) == oVolumeId)
                    {
                        // Set Path...
                        sVolumePath = Strings.Trim(rsADO.Fields["PhysicalDriveLetter"].ToString()) + Strings.Trim(rsADO.Fields["VolumesPath"].ToString());
                        if (Strings.Right(sVolumePath, 1) != @"\")
                        {
                            sVolumePath = sVolumePath + @"\";
                        }

                        if (Strings.Right(sVolumePath, 1) != @"\")
                        {
                            sVolumePath = sVolumePath + @"\";
                        }
                        else
                        {
                            sVolumePath = sVolumePath;
                        }
                        rsADO.Close();
                        break;
                    }
                    rsADO.MoveNext();
                }
            }
        }

        public JsonResult FillOutputSetting(ImportLoad serializedForm, bool pReverseOrder)
        {
            IRepository<OutputSetting> _iOutputSetting = new Repositories<OutputSetting>();
            IRepository<Volume> _iVolume = new Repositories<Volume>();
            IRepository<ImageTablesList> _iImageTablesList = new Repositories<ImageTablesList>();
            string JSONResult = "";
            string ImportLoadJSON = "";
            try
            {
                string sDirectory;
                string sImageTableName;
                ImageTablesList oImageTableList;
                IQueryable<OutputSetting> oOutputSettings;
                Volume oVolumes;
                var dt = new DataTable();
                var oImportLoad = new ImportLoad();
                dt.Columns.Add(new DataColumn("Id"));
                dt.Columns.Add(new DataColumn("Path"));
                dt.Columns.Add(new DataColumn("Prefix"));
                dt.Columns.Add(new DataColumn("ImageTable"));
                oOutputSettings = _iOutputSetting.All();
                foreach (var oOutputSetObj in oOutputSettings)
                {
                    var outputsettings = (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.OutputSettings;
                    var access = (Permissions.Permission)Enums.PassportPermissions.Access;
                    if (oOutputSetObj.InActive == false & passport.CheckPermission(oOutputSetObj.Id, outputsettings, access))
                    {
                        sImageTableName = "";
                        string VolumePath = "";
                        LoadPaths((int)oOutputSetObj.VolumesId, ref VolumePath);
                        if (VolumePath != @"\")
                        {
                            sDirectory = VolumePath;
                        }
                        else
                        {
                            sDirectory = "";
                        }
                        oVolumes = _iVolume.All().Where(m => Operators.ConditionalCompareObjectEqual(m.Id, oOutputSetObj.VolumesId, false)).FirstOrDefault();
                        if (oVolumes is not null)
                        {
                            if (Convert.ToBoolean((oVolumes.ImageTableName).Length))
                            {
                                oImageTableList = _iImageTablesList.Where(m => m.TableName.Trim().ToLower().Equals(oVolumes.ImageTableName)).FirstOrDefault();
                                if (oImageTableList is null)
                                {
                                    sImageTableName = oVolumes.ImageTableName;
                                }
                                else if (oImageTableList.Id == 0)
                                {
                                    sImageTableName = oVolumes.ImageTableName;
                                }
                                else
                                {
                                    sImageTableName = oImageTableList.UserName;
                                }

                                oImageTableList = default;
                                sDirectory = string.Format(Languages.get_Translation("msgImportCtrlStoreInDatabase"), sImageTableName);
                                sImageTableName = oVolumes.ImageTableName;
                            }
                        }

                        oVolumes = default;
                        var dr = dt.NewRow();
                        dr["Id"] = oOutputSetObj.Id;
                        dr["Path"] = sDirectory;
                        dr["Prefix"] = oOutputSetObj.FileNamePrefix;
                        dt.Rows.Add(dr);
                    }
                }
                if (TempData.ContainsKey("ImportLoad"))
                {
                    //oImportLoad = (ImportLoad)TempData.Peek("ImportLoad");
                    oImportLoad = TempData.GetPeek<ImportLoad>("ImportLoad");
                }
                oImportLoad.Duplicate = serializedForm.Duplicate;
                if (string.IsNullOrEmpty(serializedForm.IdFieldName) | serializedForm.IdFieldName == "0")
                {
                    oImportLoad.IdFieldName = null;
                }
                else
                {
                    oImportLoad.IdFieldName = serializedForm.IdFieldName;
                }
                oImportLoad.ReverseOrder = pReverseOrder;
                oImportLoad.DateDue = serializedForm.DateDue;
                oImportLoad.TrackDestinationId = serializedForm.TrackDestinationId;
                //TempData["ImportLoad"] = oImportLoad;
                TempData.Set<ImportLoad>("ImportLoad", oImportLoad);

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                JSONResult = JsonConvert.SerializeObject(dt, Formatting.Indented, Setting);
                ImportLoadJSON = JsonConvert.SerializeObject(oImportLoad, Formatting.Indented, Setting);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            return Json(new
            {
                JSONResult,
                ImportLoadJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        public async Task<IActionResult> AttachImages(object file)
        {
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            bool ProcessLoad = true;
            try
            {
                var oImportLoad = new ImportLoad();
                string DirectoryName = "";
                var oImportLoadName = httpContext.Request.Form["ImportLoadName"].Text();
                //var oImportLoadName = oimportloadname.Count > 0 ? oimportloadname[0] : "";
                oImportLoad = _iImportLoad.Where(m => m.LoadName.Trim().ToLower().Equals(oImportLoadName.Trim().ToLower())).FirstOrDefault();
                if (oImportLoad is not null)
                {
                    // If (Not String.IsNullOrEmpty(oImportLoad.InputFile)) Then
                    if (!(httpContext.Session.GetString("ImportFilePath") == null))
                    {
                        // DirectoryName = Path.GetDirectoryName(oImportLoad.InputFile)
                        DirectoryName = Path.GetDirectoryName(httpContext.Session.GetString("ImportFilePath") ?? "");
                        var DirectoryFolder = DirectoryName.Split(Path.DirectorySeparatorChar);
                        DirectoryName = DirectoryName.Replace(DirectoryFolder[DirectoryFolder.Length - 1], "");
                        DirectoryName = DirectoryName + "Attachment";
                        if (!Directory.Exists(DirectoryName))
                        {
                            Directory.CreateDirectory(DirectoryName);
                        }
                    }

                    foreach (var pfilePath in httpContext.Request.Form.Files)
                    {
                        //HttpPostedFileBase pfile = Request.Files(pfilePath);

                        var pfile = httpContext.Request.Form.Files[0];
                        var importload = httpContext.Request.Query["pfilePath"];
                        var ImportLoadName = importload.Count > 0 ? importload[0] : "";
                        using (var stream = System.IO.File.Create(ImportLoadName))
                        {
                            await pfile.CopyToAsync(stream);
                        }
                        // pfile.SaveAs(filePath);
                    }

                    var JSONObj = ProcessLoadForQuietProcessing(oImportLoad.LoadName);
                    dynamic obj = JSONObj.Value;
                    Keys.ErrorType = obj.errortype;
                    Keys.ErrorMessage = obj.message;
                }
                else
                {
                    ProcessLoad = false;
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Languages.get_Translation("msgImportCtrlDoesNtExist");
                }
            }
            catch (Exception ex)
            {
                ProcessLoad = false;
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message.ToString();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public JsonResult ProcessLoadForQuietProcessing(string currentLoadName)
        {
            IRepository<Table> _iTables = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            IRepository<ImportField> _iImportField = new Repositories<ImportField>();
            IRepository<ScanList> _iScanList = new Repositories<ScanList>();
            IRepository<TabSet> _iTabSet = new Repositories<TabSet>();
            IRepository<TableTab> _iTableTab = new Repositories<TableTab>();
            IRepository<RelationShip> _iRelationship = new Repositories<RelationShip>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<SLTrackingSelectData> _iSLTrackingSelectData = new Repositories<SLTrackingSelectData>();
            IRepository<SLTextSearchItem> _iSLTextSearchItems = new Repositories<SLTextSearchItem>();
            IRepository<SLRequestor> _iSLRequestor = new Repositories<SLRequestor>();
            IRepository<TrackingHistory> _iTrackingHistory = new Repositories<TrackingHistory>();
            IRepository<SLDestructCertItem> _iSLDestCertItem = new Repositories<SLDestructCertItem>();
            IRepository<SLRetentionCode> _iSLRetentionCode = new Repositories<SLRetentionCode>();
            IRepository<SecureUser> _iSecureUser = new Repositories<SecureUser>();
            IRepository<OutputSetting> _iOutputSetting = new Repositories<OutputSetting>();
            IRepository<Volume> _iVolume = new Repositories<Volume>();
            IRepository<ImageTablesList> _iImageTablesList = new Repositories<ImageTablesList>();

            ADODB.Connection mcnImport = default;
            ImportLoad oImportLoad = default;
            IDBManager _IDBManager = new ImportDBManager();
            IDBManager _IDBManagerDefault = new ImportDBManager();
            do
            {
                try
                {
                    var mrsImport = new ADODB.Recordset();
                    Table oTable = default;
                    var oSchemaColumns = new SchemaColumns();
                    bool WillDelete = false;
                    var tTracking = new TRACKDESTELEMENTS();
                    int lFieldIndex = -1;
                    bool bTrackingOnly;
                    var bAbort = default(bool);
                    object moTrackDestination = null;
                    string sReturnMessage = "";
                    var bAutoIncrementCheck = default(bool);
                    var bHasAutoIncrement = default(bool);
                    var maSkipFields = Array.Empty<string>();
                    string tableUserName;
                    OutputSetting oOutputSettings;
                    Volume oVolume;
                    List<ImportField> importFields = null;
                    string VolumePath = string.Empty;
                    string sDirectory = string.Empty;
                    string sImageTableName = string.Empty;
                    string LoadInputFile = string.Empty;
                    TrackingServices.StartTime = DateTime.Now;
                    int recordErrorTrack = 0;
                    bool errorFlag = false;
                    int mlRecordsAdded = 0;
                    int mlRecordsChanged = 0;
                    int mlRecordsRead = 0;
                    int mlSQLHits = 0;
                    int recordIndex = -1;
                    bool bContinue = true;
                    int errorIndex = 0;
                    bool mbQuietProcessing = true;
                    DataTable upperTable = null;
                    bool oTableTrackable = false;
                    object oDefaultTrackingDest = null;
                    var TrackingObjectDT = new DataTable();
                    var TrackingDestDT = new DataTable();
                    var IdDataTableForRetention = new DataTable();
                    var DataForUpdateTracking = new DataTable();
                    Table oBarCodeTable = default;
                    TabFusionRMS.FoundBarCode oDefaultTrackDestination = default;
                    bool bGoodConfig = false;
                    // #Region "Get All Data"
                    List<ImportLoad> data_iImportLoad = _iImportLoad.All().ToList();
                    List<Table> data_iTables = _iTables.All().ToList();
                    List<ImportField> data_iImportField = _iImportField.All().ToList();
                    List<OutputSetting> data_iOutputSetting = _iOutputSetting.All().ToList();
                    List<Volume> data_iVolume = _iVolume.All().ToList();
                    List<ImageTablesList> data_iImageTablesList = _iImageTablesList.All().ToList();
                    List<TabFusionRMS.Models.System> data_iSystem = _iSystem.All().ToList();
                    List<Databas> data_iDatabas = _iDatabas.All().ToList();
                    List<ScanList> data_iScanList = _iScanList.All().ToList();
                    List<TabFusionRMS.Models.View> data_iView = _iView.All().ToList();
                    List<TableTab> data_iTableTab = _iTableTab.All().ToList();
                    List<TabSet> data_iTabSet = _iTabSet.All().ToList();
                    List<RelationShip> data_iRelationship = _iRelationship.All().ToList();
                    List<SLTrackingSelectData> data_iSLTrackingSelectData = _iSLTrackingSelectData.All().ToList();
                    List<SecureUser> data_iSecureUser = _iSecureUser.All().ToList();
                    List<SLRetentionCode> data_iSLRetentionCode = _iSLRetentionCode.All().ToList();
                    List<SLDestructCertItem> data_iSLDestCertItem = _iSLDestCertItem.All().ToList();
                    List<SLRequestor> data_iSLRequestor = _iSLRequestor.All().ToList();
                    IQueryable<TrackingHistory> data_iTrackingHistory = _iTrackingHistory.All();
                    List<SLTextSearchItem> data_iSLTextSearchItems = _iSLTextSearchItems.All().ToList();
                    IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
                    // #End Region
                    if (!string.IsNullOrEmpty(currentLoadName))
                    {
                        oImportLoad = _iImportLoad.All().Where(m => m.LoadName.Trim().ToLower().Equals(currentLoadName.Trim().ToLower())).FirstOrDefault();
                    }

                    if (!string.IsNullOrEmpty(oImportLoad.LoadName))
                    {
                        if (oImportLoad is not null)
                        {
                            if (oImportLoad.FromHandHeldEnum != 0)
                            {
                                Keys.ErrorMessage = Languages.get_Translation("msgJsImportNotSupportInWebApp");
                                Keys.ErrorType = "p";
                                break;
                            }
                            else
                            {
                                if (!(httpContext.Session.GetString("ImportFilePath") == null))
                                {
                                    if (!string.IsNullOrEmpty(httpContext.Session.GetString("ImportFilePath")))
                                    {
                                        // LoadInputFile = oImportLoad.InputFile
                                        LoadInputFile = httpContext.Session.GetString("ImportFilePath") ?? "";
                                        if (System.IO.File.Exists(LoadInputFile))
                                        {
                                            bGoodConfig = true;
                                        }
                                        else
                                        {
                                            sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlImportFileNotExists"), Path.GetFileName(httpContext.Session.GetString("ImportFilePath") ?? ""));
                                            Keys.ErrorMessage = sReturnMessage;
                                            Keys.ErrorType = "p";
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        sReturnMessage = Languages.get_Translation("msgJsImportFileCudNotFound");
                                        Keys.ErrorMessage = sReturnMessage;
                                        Keys.ErrorType = "p";
                                        break;
                                    }
                                }

                                // Open the default connection
                                _IDBManagerDefault.ConnectionString = Keys.get_GetDBConnectionString();
                                _IDBManagerDefault.Open();

                                importFields = data_iImportField.Where(m => m.ImportLoad.Trim().ToLower().Equals(oImportLoad.LoadName.Trim().ToLower())).ToList();
                                if (!string.IsNullOrEmpty(oImportLoad.FileName))
                                {
                                    if (Strings.StrComp(oImportLoad.FileName, IMPORT_TRACK, Constants.vbTextCompare) != 0)
                                    {
                                        var bDoNotAddImport = CollectionsClass.EngineTablesOkayToImportList.Contains(oImportLoad.FileName.Trim().ToLower());
                                        if (bDoNotAddImport)
                                        {
                                            var retentionObj = GetInfoUsingADONET.MakeSureIsALoadedTable(oImportLoad.FileName.Trim(), _iSecureObject, _IDBManagerDefault);
                                            //httpContext.Session.SetString(oImportLoad.FileName.Trim().ToLower(), retentionObj.ToString());
                                            ContextService.SetSessionValue(oImportLoad.FileName.Trim().ToLower(), JsonConvert.SerializeObject(retentionObj), httpContext);
                                            oTable = ContextService.GetObjectFromJson<Table>(httpContext.Session.GetString(oImportLoad.FileName.Trim().ToLower()), httpContext);
                                        }
                                        else
                                        {
                                            oTable = data_iTables.Where(m => m.TableName.Trim().ToLower().Equals(oImportLoad.FileName.Trim().ToLower())).FirstOrDefault();
                                        }
                                        bGoodConfig = oTable is not null;
                                    }
                                }
                                if (bGoodConfig)
                                {
                                    if (importFields is not null)
                                    {
                                    }
                                    // If (importFields.Count = 0) Then
                                    // bGoodConfig = False
                                    // sReturnMessage = String.Format(Languages.Translation("msgImportCtrlNoFieldSelected"), oImportLoad.LoadName)
                                    // Keys.ErrorMessage = sReturnMessage
                                    // Keys.ErrorType = "p"
                                    // Exit Try
                                    // End If
                                    else
                                    {
                                        bGoodConfig = false;
                                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlNoFieldSelected"), oImportLoad.LoadName);
                                        Keys.ErrorMessage = sReturnMessage;
                                        Keys.ErrorType = "p";
                                        break;
                                    }
                                }

                                var attachments = (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Attachments;
                                var add = (Permissions.Permission)Enums.PassportPermissions.Add;
                                if (bGoodConfig)
                                {
                                    bool IsPCFile = importFields.Any(m => m.FieldName.Trim().Equals(PCFILE_COPY));
                                    bool IsImageFile = importFields.Any(m => m.FieldName.Trim().Equals(IMAGE_COPY));

                                    if (IsPCFile || IsImageFile)
                                    {
                                        if (Strings.StrComp(oImportLoad.FileName, IMPORT_TRACK, Constants.vbTextCompare) != 0 && !passport.CheckPermission(oTable.TableName, attachments, add))
                                        {
                                            bGoodConfig = false;
                                            sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlNoTableAdd"), oTable.UserName);
                                            Keys.ErrorMessage = sReturnMessage;
                                            Keys.ErrorType = "p";
                                            break;
                                        }

                                        if (!string.IsNullOrEmpty(oImportLoad.ScanRule))
                                        {
                                            oOutputSettings = data_iOutputSetting.Where(m => m.Id.Trim().ToLower().Equals(oImportLoad.ScanRule.Trim().ToLower())).FirstOrDefault();
                                            if (oOutputSettings is not null)
                                            {
                                                oVolume = data_iVolume.Where(m => Operators.ConditionalCompareObjectEqual(m.Id, oOutputSettings.VolumesId, false)).FirstOrDefault();
                                                if (oVolume is not null)
                                                {
                                                    if (IsPCFile)
                                                    {
                                                        if (!string.IsNullOrEmpty(oVolume.ImageTableName))
                                                        {
                                                            bGoodConfig = false;
                                                            sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlPCFileNotFoundInTbl"), oVolume.ImageTableName);
                                                            Keys.ErrorMessage = sReturnMessage;
                                                            Keys.ErrorType = "p";
                                                        }
                                                    }
                                                }

                                                var otsetting = (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.OutputSettings;
                                                var acess = (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access;
                                                var volume = (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Volumes;
                                                //var add = (Permissions.Permission)Enums.PassportPermissions.Add;
                                                if (bGoodConfig)
                                                {
                                                    if (string.IsNullOrEmpty(Strings.Trim(oOutputSettings.Id)))
                                                    {
                                                        bGoodConfig = false;
                                                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlOptSettingRequired"), oImportLoad.LoadName);
                                                        Keys.ErrorMessage = sReturnMessage;
                                                        Keys.ErrorType = "p";
                                                    }
                                                    else if (string.IsNullOrEmpty(Strings.Trim(oOutputSettings.FileNamePrefix)))
                                                    {
                                                        bGoodConfig = false;
                                                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlOptSettingNotValid"), oOutputSettings.Id);
                                                        Keys.ErrorMessage = sReturnMessage;
                                                        Keys.ErrorType = "p";
                                                    }
                                                    else if ((bool)oOutputSettings.InActive)
                                                    {
                                                        bGoodConfig = false;
                                                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlOptSettingNotActive"), oOutputSettings.Id);
                                                        Keys.ErrorMessage = sReturnMessage;
                                                        Keys.ErrorType = "p";
                                                    }
                                                    else if (!passport.CheckPermission(oOutputSettings.Id, otsetting, acess))
                                                    {
                                                        bGoodConfig = false;
                                                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlNoRights4OptSetting"), oOutputSettings.Id);
                                                        Keys.ErrorMessage = sReturnMessage;
                                                        Keys.ErrorType = "p";
                                                    }
                                                    else if (oVolume is null)
                                                    {
                                                        bGoodConfig = false;
                                                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlOptSetInValid"), oOutputSettings.Id);
                                                        Keys.ErrorMessage = sReturnMessage;
                                                        Keys.ErrorType = "p";
                                                    }
                                                    else
                                                    {
                                                        if (!passport.CheckPermission(oVolume.Name, volume, acess))
                                                        {
                                                            bGoodConfig = false;
                                                            sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlOptSetInValid"), oOutputSettings.Id);
                                                            Keys.ErrorMessage = sReturnMessage;
                                                            Keys.ErrorType = "p";
                                                        }
                                                        else if (!passport.CheckPermission(oVolume.Name, volume, add))
                                                        {
                                                            bGoodConfig = false;
                                                            sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlNoVolumeAdd"), oVolume.Name);
                                                            Keys.ErrorMessage = sReturnMessage;
                                                            Keys.ErrorType = "p";
                                                        }

                                                        oVolume = default;
                                                    }
                                                }

                                                if (bGoodConfig)
                                                {
                                                    LoadPaths((int)oOutputSettings.VolumesId, ref VolumePath);
                                                    if (VolumePath != @"\")
                                                    {
                                                        sDirectory = VolumePath;
                                                    }
                                                    else
                                                    {
                                                        sDirectory = "";
                                                    }
                                                    var oVolumes = data_iVolume.Where(m => Operators.ConditionalCompareObjectEqual(m.Id, oOutputSettings.VolumesId, false)).FirstOrDefault();
                                                    if (oVolumes is not null)
                                                    {
                                                        if (Convert.ToBoolean((oVolumes.ImageTableName).Length))
                                                        {
                                                            var oImageTableList = data_iImageTablesList.Where(m => m.TableName.Trim().ToLower().Equals(oVolumes.ImageTableName)).FirstOrDefault();
                                                            if (oImageTableList is null)
                                                            {
                                                                sImageTableName = oVolumes.ImageTableName;
                                                            }
                                                            else if (oImageTableList.Id == 0)
                                                            {
                                                                sImageTableName = oVolumes.ImageTableName;
                                                            }
                                                            else
                                                            {
                                                                sImageTableName = oImageTableList.UserName;
                                                            }

                                                            oImageTableList = default;
                                                            sDirectory = string.Format(Languages.get_Translation("msgImportCtrlStoreInDatabase"), sImageTableName);
                                                            sImageTableName = oVolumes.ImageTableName;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bGoodConfig = false;
                                                sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlOptSettingRequired"), oImportLoad.LoadName);
                                                Keys.ErrorMessage = sReturnMessage;
                                                Keys.ErrorType = "p";
                                            }
                                        }
                                        else
                                        {
                                            bGoodConfig = false;
                                            sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlOptSettingRequired"), oImportLoad.LoadName);
                                            Keys.ErrorMessage = sReturnMessage;
                                            Keys.ErrorType = "p";
                                        }
                                    }
                                }

                                if (bGoodConfig)
                                {
                                    mrsImport = GetRecordsetOfLoadFile(oImportLoad, ref mcnImport);
                                    if (mrsImport.EOF & mrsImport.BOF)
                                    {
                                        bGoodConfig = false;
                                    }
                                    else
                                    {
                                        if (TempData.ContainsKey("ImportDataRS"))
                                        {
                                            TempData.Remove("ImportDataRS");
                                        }
                                        //TempData["ImportDataRS"] = mrsImport;
                                        //TempData.Set<ADODB.Recordset>("ImportDataRS", mrsImport);
                                    }
                                }

                                if (bGoodConfig)
                                {
                                    if (bContinue & recordIndex == -1)
                                    {
                                        mrsImport.MoveFirst();
                                        if ((bool)oImportLoad.ReverseOrder)
                                        {
                                            recordIndex = mrsImport.RecordCount - 1;
                                            mrsImport.MoveLast();
                                        }
                                    }
                                    // This portion to point record in recordset..Not required for quiet processing
                                    else if (bContinue & recordIndex != -1)
                                    {
                                        if (oImportLoad.ReverseOrder == false)
                                        {
                                            if (recordIndex >= mrsImport.RecordCount)
                                            {
                                                if (recordErrorTrack != 0)
                                                {
                                                    Keys.ErrorMessage = Languages.get_Translation("msgJsImportCompErrOccur");
                                                    Keys.ErrorType = "e";
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                mrsImport.Move(recordIndex);
                                            }
                                        }
                                        else
                                        {
                                            mrsImport.Move(recordIndex);
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(oImportLoad.IdFieldName))
                                    {
                                        if (oImportLoad.IdFieldName != "0")
                                        {
                                            var oImportField = importFields.Where(m => m.FieldName.Trim().ToLower().Equals(oImportLoad.IdFieldName.Trim().ToLower())).FirstOrDefault();
                                            if (oImportField is not null)
                                            {
                                                lFieldIndex = (int)oImportField.ReadOrder;
                                            }
                                        }
                                    }

                                    if (Strings.StrComp(oImportLoad.FileName, IMPORT_TRACK, Constants.vbTextCompare) != 0)
                                    {
                                        tableUserName = oTable.UserName;
                                        bTrackingOnly = false;
                                    }
                                    else
                                    {
                                        tableUserName = IMPORT_TRACK;
                                        bTrackingOnly = true;
                                    }

                                    if ((bool)(oImportLoad.DoReconciliation & data_iSystem.FirstOrDefault().ReconciliationOn))
                                    {
                                        tTracking.bDoRecon = true;
                                    }
                                    else
                                    {
                                        tTracking.bDoRecon = false;
                                    }
                                    // Initialize Additional Tracking Fields
                                    tTracking.sTrackingAdditionalField1 = "";
                                    tTracking.sTrackingAdditionalField2 = "";
                                    bool bSecurityCheck = default;
                                    Table oCurrentTable = null;
                                    string sTabsetIds = "";

                                    if (!string.IsNullOrEmpty(oImportLoad.TrackDestinationId))
                                    {
                                        moTrackDestination = GetInfoUsingADONET.BarCodeLookup(default, data_iDatabas,
                                                                           data_iTables, data_iSystem, data_iScanList,
                                                                           data_iView, data_iTableTab, data_iTabSet,
                                                                           data_iRelationship, oImportLoad.TrackDestinationId, httpContext,
                                                                            ref sTabsetIds, ref oCurrentTable, ref bSecurityCheck, _IDBManagerDefault, true, ref sReturnMessage);
                                        if (moTrackDestination is not null)
                                        {
                                            if (oTable.TrackingTable > 0)
                                            {
                                                moTrackDestination = moTrackDestination;
                                            }
                                            else
                                            {
                                                moTrackDestination = null;
                                            }
                                        }
                                    }
                                    var table = (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table;
                                    var transfer = (Permissions.Permission)Enums.PassportPermissions.Transfer;

                                    if (oTable is not null)
                                    {
                                        oTableTrackable = passport.CheckPermission(oTable.TableName.Trim(), table, transfer);
                                        if ((oTable.DefaultTrackingId ?? "").Length > 0)
                                        {
                                            if (Convert.ToBoolean((oTable.DefaultTrackingTable).Length))
                                            {
                                                oBarCodeTable = data_iTables.Where(m => m.TableName.Trim().ToLower().Equals(oTable.DefaultTrackingTable.Trim().ToLower())).FirstOrDefault();
                                            }
                                            oDefaultTrackDestination = GetInfoUsingADONET.BarCodeLookup(default,
                                                data_iDatabas,
                                                data_iTables,
                                                data_iSystem,
                                                data_iScanList, data_iView,
                                                data_iTableTab, data_iTabSet,
                                                data_iRelationship, oTable.DefaultTrackingId, httpContext,
                                                ref sTabsetIds, ref oCurrentTable, ref bSecurityCheck,
                                                _IDBManagerDefault, true, ref sReturnMessage);
                                        }
                                        if (!string.IsNullOrEmpty(oTable.DBName))
                                        {
                                            var oDatabase = data_iDatabas.Where(m => m.DBName.Trim().ToLower().Equals(oTable.DBName.Trim().ToLower())).FirstOrDefault();
                                            // open the external db connection
                                            _IDBManager.ConnectionString = Keys.get_GetDBConnectionString(oDatabase);
                                            _IDBManager.Open();
                                        }
                                    }

                                    while (bContinue)
                                    {
                                        sReturnMessage = string.Empty;
                                        if (mrsImport is null)
                                        {
                                            bAbort = true;
                                        }
                                        else
                                        {

                                            bAbort = !ImportLoadRecordForQuietProcessing(oImportLoad, importFields, oTable, (TabFusionRMS.FoundBarCode)moTrackDestination, lFieldIndex, bTrackingOnly, tTracking, ref sReturnMessage, ref bAutoIncrementCheck,
                                                                                         ref bHasAutoIncrement, ref mlRecordsAdded, ref mlRecordsChanged, ref mbQuietProcessing, ref mlRecordsRead, ref mlSQLHits, ref maSkipFields, ref recordIndex, ref recordErrorTrack,
                                                                                         ref errorIndex, ref errorFlag, tableUserName, sDirectory, data_iDatabas, data_iSystem, data_iSLTrackingSelectData, data_iTables, data_iScanList, data_iView, data_iTableTab,
                                                                                         data_iTabSet, data_iRelationship, data_iSecureUser, data_iSLRetentionCode, data_iSLDestCertItem, data_iSLRequestor, data_iTrackingHistory, data_iOutputSetting, data_iSLTextSearchItems,
                                                                                         oTableTrackable, VolumePath, oDefaultTrackDestination, _IDBManagerDefault, _IDBManager);
                                            mlRecordsRead = mlRecordsRead + 1;
                                        }
                                        if (oImportLoad.ReverseOrder == false)
                                        {
                                            bContinue = !(mrsImport.EOF & mrsImport.EOF);
                                            if (bContinue)
                                            {
                                                mrsImport.MoveNext();
                                            }
                                        }
                                        else
                                        {
                                            bContinue = !(recordIndex == -1);
                                            if (bContinue)
                                            {
                                                mrsImport.MovePrevious();
                                            }
                                        }
                                        var Setting = new JsonSerializerSettings();
                                        Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                                        if (bAbort == true)
                                        {
                                            break;
                                        }
                                        else if (bAbort == false)
                                        {
                                            if (string.IsNullOrEmpty(sReturnMessage))
                                            {
                                                if ((bool)oImportLoad.ReverseOrder)
                                                {
                                                    recordIndex = recordIndex - 1;
                                                }
                                                else if (recordIndex == -1)
                                                {
                                                    recordIndex = recordIndex + 2;
                                                }
                                                else
                                                {
                                                    recordIndex = recordIndex + 1;
                                                }
                                                WillDelete = true;
                                            }
                                        }
                                        if (oImportLoad.ReverseOrder == false)
                                        {
                                            bContinue = !(mrsImport.EOF & mrsImport.EOF);
                                        }
                                        else
                                        {
                                            bContinue = !(recordIndex == -1);
                                        }
                                    }

                                    WriteReportFile(oImportLoad, tableUserName, mlRecordsRead, mlRecordsAdded, mlRecordsChanged, mlSQLHits, recordErrorTrack);

                                    if (bAbort == false)
                                    {
                                        if (mcnImport is not null)
                                        {
                                            mcnImport.Close();
                                            mcnImport = default;
                                        }

                                        if ((bool)(WillDelete & oImportLoad.DeleteSourceFile))
                                        {
                                            if (System.IO.File.Exists(LoadInputFile))
                                            {
                                                System.IO.File.Delete(LoadInputFile);
                                                string DirectoryName = Path.GetDirectoryName(LoadInputFile);
                                                string schemaFile = DirectoryName + @"\schema.ini";
                                                if (System.IO.File.Exists(schemaFile))
                                                {
                                                    System.IO.File.Delete(schemaFile);
                                                }
                                                Directory.Delete(DirectoryName);
                                            }
                                        }

                                        var oHasAttachment = data_iImportField.Where(m => m.ImportLoad.Trim().ToLower().Equals(oImportLoad.LoadName.Trim().ToLower())).Any(m => m.FieldName.Trim().ToUpper().Equals(PCFILE_COPY) | m.FieldName.Trim().ToUpper().Equals(IMAGE_COPY));
                                        if (oHasAttachment)
                                        {
                                            string _parentfolder = Path.GetDirectoryName(Path.GetDirectoryName(LoadInputFile));
                                            string oAttachmentPath = Path.Combine(_parentfolder, "Attachment");
                                            foreach (string _file in Directory.GetFiles(oAttachmentPath))
                                                System.IO.File.Delete(_file);
                                        }
                                    }

                                    if (bAbort == true)
                                    {
                                        Keys.ErrorMessage = sReturnMessage;
                                        Keys.ErrorType = "w";
                                    }
                                    else if (recordErrorTrack == 0)
                                    {
                                        Keys.ErrorMessage = Languages.get_Translation("msgJsImportCompNoError");
                                        Keys.ErrorType = "s";
                                    }
                                    else
                                    {
                                        Keys.ErrorMessage = Languages.get_Translation("msgJsImportCompErrOccurRun");
                                        Keys.ErrorType = "e";
                                    }
                                }
                            }
                        }
                        else
                        {
                            Keys.ErrorMessage = Languages.get_Translation("msgImportCtrlImportFileNotExistsMsg");
                            Keys.ErrorType = "p";
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Keys.ErrorMessage = ex.Message;
                    Keys.ErrorType = "c";
                    int argrecordErrorTrack = 0;
                    WriteErrorFile(oImportLoad, ex.Message, false, ref argrecordErrorTrack, ex);
                }
                finally
                {
                    if (mcnImport is not null)
                    {
                        mcnImport.Close();
                    }
                    if (_IDBManager.Connection is not null)
                    {
                        _IDBManager.Dispose();
                    }
                    if (_IDBManagerDefault.Connection is not null)
                    {
                        _IDBManagerDefault.Dispose();
                    }
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });

        }

        public bool ImportLoadRecordForQuietProcessing(ImportLoad oImportLoad, List<ImportField> oImportFields, Table oTable, TabFusionRMS.FoundBarCode moTrackDestination, int lFieldIndex, bool bTrackingOnly, TRACKDESTELEMENTS tTracking,
                                                         ref string sReturnMessage, ref bool bAutoIncrementCheck, ref bool bHasAutoIncrement, ref int mlRecordsAdded, ref int mlRecordsChanged, ref bool mbQuietProcessing, ref int mlRecordsRead,
                                                         ref int mlSQLHits, ref string[] maSkipFields, ref int abortRecordIndex, ref int recordErrorTrack, ref int errorIndex, ref bool errorFlag, string tableUserName, string sDirectory,
                                                         List<Databas> data_iDatabas, List<TabFusionRMS.Models.System> data_iSystem, List<SLTrackingSelectData> data_iSLTrackingSelectData, List<Table> data_iTables, List<ScanList> data_iScanList,
                                                         List<View> data_iView, List<TableTab> data_iTableTab, List<TabSet> data_iTabSet, List<RelationShip> data_iRelationship, List<SecureUser> data_iSecureUser, List<SLRetentionCode> data_iSLRetentionCode,
                                                         List<SLDestructCertItem> data_iSLDestCertItem, List<SLRequestor> data_iSLRequestor, IQueryable<TrackingHistory> data_iTrackingHistory, List<OutputSetting> data_iOutputSetting, List<SLTextSearchItem> data_iSLTextSearchItems,
                                                         bool oTableTrackable, string VolumePath, TabFusionRMS.FoundBarCode oDefaultTrackDestination, IDBManager _IDBManagerDefault, IDBManager _IDBManager)
        {
            //big method
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<Table> _iTables = new Repositories<Table>();
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            IRepository<SLIndexer> _iSLIndexer = new Repositories<SLIndexer>();
            IRepository<SLAuditUpdate> _iSLAuditUpdate = new Repositories<SLAuditUpdate>();
            IRepository<SLAuditUpdChildren> _iSLAuditUpdChildren = new Repositories<SLAuditUpdChildren>();
            IRepository<SecureUser> _iSecureUser = new Repositories<SecureUser>();
            IRepository<TrackingStatu> _iTrackingStatus = new Repositories<TrackingStatu>();
            IRepository<AssetStatu> _iAssetStatus = new Repositories<AssetStatu>();
            IRepository<ImportField> _iImportField = new Repositories<ImportField>();

            bool ImportLoadRecordForQuietProcessingRet = default;
            var bHoldTelxonMode = default(bool);
            var dHoldTelxonDate = default(DateTime);
            string sHoldTelxonUser = "";
            var mrsImport = new ADODB.Recordset();
            IDBManager commonIDBManager = default;
            try
            {
                var oSchemaColumn = new SchemaColumns();
                string sSql;
                object vDefaultValue = null;
                object vFieldIndex = null;
                var bRecordError = default(bool);
                TabFusionRMS.FoundBarCode oTrackObject = null;
                DataTable dtRecords = null;
                int lError = 0;
                bool bFound = false;
                bool bDoAddNew = false;
                TabFusionRMS.FoundBarCode oTrackDestination = default;
                var cFieldNames = new Collection();
                var cFieldBeforeValues = new Collection();
                var cFieldAfterValues = new Collection();
                string currentImageName = "";
                string UpdatedRecordIdVal = "";
                string UpdateByFieldVal = "";
                bHoldTelxonMode = TrackingServices.TelxonModeOn;
                dHoldTelxonDate = TrackingServices.ScanDateTime;
                sHoldTelxonUser = TrackingServices.TelxonUserName;
                DataTable SchemaTable = null;
                TrackingServices.ScanDateTime = DateTime.Now;
                TrackingServices.TelxonUserName = "";
                var ListOfClsFieldWithVal = new List<ClsFieldWithVal>();

                if (TempData.ContainsKey("ImportDataRS"))
                {
                    //mrsImport = (ADODB.Recordset)TempData.Peek("ImportDataRS");
                    mrsImport = TempData.GetPeek<ADODB.Recordset>("ImportDataRS");
                }

                if (lFieldIndex != -1)
                {
                    lFieldIndex = lFieldIndex - 1;
                }

                if (bTrackingOnly)
                {
                    bFound = true;
                    TrackingServices.TelxonModeOn = true;
                }
                else
                {
                    if (oTable is not null)
                    {
                        if (string.IsNullOrEmpty(oTable.DBName))
                        {
                            commonIDBManager = _IDBManagerDefault;
                        }
                        else
                        {
                            commonIDBManager = _IDBManager;
                        }
                    }

                    TrackingServices.TelxonModeOn = false;
                    if (Strings.StrComp(oImportLoad.IdFieldName, IMPORTBY_NONE) == 0 | lFieldIndex == -1)
                    {
                        sSql = "SELECT * FROM [" + oTable.TableName + "] WHERE 0 = 1";
                    }
                    else
                    {
                        SchemaTable = GetInfoUsingADONET.GetSchemaInfo(commonIDBManager, oTable.TableName, DatabaseMap.RemoveTableNameFromField(oImportLoad.IdFieldName));
                        sSql = "SELECT TOP 2 * FROM [" + oTable.TableName + "] WHERE " + oImportLoad.IdFieldName + " = ";
                        vDefaultValue = oImportFields.Where(m => m.FieldName.Trim().ToLower().Equals(oImportLoad.IdFieldName.Trim().ToLower())).FirstOrDefault().DefaultValue;
                        if (IsFunction(vDefaultValue))
                        {
                            vFieldIndex = ProcessFunctionValue(vDefaultValue);
                        }
                        else
                        {
                            vFieldIndex = mrsImport.Fields[lFieldIndex].Value;
                        }
                        if (vFieldIndex is DBNull)
                        {
                            vFieldIndex = 0;
                        }
                        else
                        {
                            string argsStripFrom = Convert.ToString(vFieldIndex);
                            StripQuotes(ref argsStripFrom);
                            vFieldIndex = argsStripFrom;
                        }

                        if (SchemaTable is not null)
                        {
                            if (GetInfoUsingADONET.IsAStringForSchema((string)SchemaTable.AsEnumerable().ElementAtOrDefault(0)["DATA_TYPE"]))
                            {
                                sSql = sSql + "'" + Strings.Replace(Convert.ToString(vFieldIndex), "'", "''") + "'";
                            }
                            else
                            {
                                if (Strings.Len(Strings.Trim(Convert.ToString(vFieldIndex))) == 0L)
                                {
                                    sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlImprtFldNotEmpty"), oImportLoad.IdFieldName);
                                    errorIndex = 0;
                                    errorFlag = false;
                                    WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default);
                                    ImportLoadRecordForQuietProcessingRet = false;
                                    return ImportLoadRecordForQuietProcessingRet;
                                }
                                if (GetInfoUsingADONET.IsADateForSchema((string)SchemaTable.AsEnumerable().ElementAtOrDefault(0)["DATA_TYPE"]))
                                {
                                    sSql = Convert.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(sSql + "'", vFieldIndex), "'"));
                                }
                                else
                                {
                                    bRecordError = Strings.StrComp(oImportLoad.IdFieldName.Trim(), (string?)vFieldIndex, Constants.vbTextCompare) == 0L & mlRecordsRead == 0L;
                                    if (bRecordError)
                                    {
                                        int errNum = 1;
                                        if (errorIndex == errNum)
                                        {
                                            errorFlag = true;
                                        }
                                        else
                                        {
                                            errorFlag = false;
                                        }
                                        errorIndex = errNum;
                                        if ((bool)oImportLoad.ReverseOrder)
                                        {
                                            abortRecordIndex = abortRecordIndex - 1;
                                        }
                                        else
                                        {
                                            abortRecordIndex = abortRecordIndex + 1;
                                        }
                                        sReturnMessage = Languages.get_Translation("msgImportCtrlNotMatchedDT");
                                        WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default);
                                        ImportLoadRecordForQuietProcessingRet = true;
                                        return ImportLoadRecordForQuietProcessingRet;
                                    }
                                    sSql = Convert.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(sSql, vFieldIndex), ""));
                                }
                            }
                        }
                        UpdateByFieldVal = Convert.ToString(vFieldIndex);
                    }

                    lError = 0;
                    dtRecords = GetInfoUsingADONET.GetADONETRecord(sSql, commonIDBManager, ref sReturnMessage, ref lError, oTable.TableName);
                    if (!string.IsNullOrEmpty(sReturnMessage) | dtRecords is null | lError != 0)
                    {
                        errorIndex = 0;
                        errorFlag = false;
                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlErrWhileAddRec"), Constants.vbNewLine);
                        WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default);
                        ImportLoadRecordForQuietProcessingRet = false;
                        return ImportLoadRecordForQuietProcessingRet;
                    }
                    if (dtRecords is not null)
                    {
                        bFound = dtRecords.Rows.Count != 0;
                    }

                    if (bFound & (oImportLoad.Duplicate == BOTH_DB_TEXT | oImportLoad.Duplicate == OVERWRITE_DB_TEXT))
                    {
                        if (dtRecords.Rows.Count > 1)
                        {
                            int errNum = 2;
                            if (errorIndex == errNum)
                            {
                                errorFlag = true;
                            }
                            else
                            {
                                errorFlag = false;
                            }
                            errorIndex = errNum;
                            if ((bool)oImportLoad.ReverseOrder)
                            {
                                abortRecordIndex = abortRecordIndex - 1;
                            }
                            else
                            {
                                abortRecordIndex = abortRecordIndex + 1;
                            }
                            sReturnMessage = Languages.get_Translation("msgImportCtrlManyRecWhich2Update");
                            WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default);
                            ImportLoadRecordForQuietProcessingRet = true;
                            return ImportLoadRecordForQuietProcessingRet;
                        }

                    }
                    SetOverWriteError(oImportLoad.Duplicate, ref bDoAddNew, ref bFound, ref sReturnMessage, ref recordErrorTrack, ref errorIndex, ref errorFlag);

                    if (!string.IsNullOrEmpty(sReturnMessage))
                    {
                        if ((bool)oImportLoad.ReverseOrder)
                        {
                            abortRecordIndex = abortRecordIndex - 1;
                        }
                        else
                        {
                            abortRecordIndex = abortRecordIndex + 1;
                        }
                        WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                        ImportLoadRecordForQuietProcessingRet = true;
                        return ImportLoadRecordForQuietProcessingRet;
                    }

                    if (bFound & oTableTrackable)
                    {
                        int errNum = 0;
                        oTrackObject = new TabFusionRMS.FoundBarCode();
                        // Start from here
                        oTrackObject.oTable = oTable;
                        string sTrackingReturn = "";
                        if (Convert.ToBoolean(Strings.Len(Strings.Trim(tTracking.sDestination))))
                        {
                            sTrackingReturn = GetInfoUsingADONET.ValidateTracking(data_iDatabas, data_iTables,
                                data_iSystem, data_iScanList, data_iView, data_iTableTab,
                                data_iTabSet, data_iRelationship, ref oTrackObject,
                                ref oTrackDestination, tTracking.sDestination, false, passport, httpContext, default, ref errNum, _IDBManagerDefault, true);
                        }
                        else if (moTrackDestination is not null)
                        {
                            errNum = 0;
                            sTrackingReturn = GetInfoUsingADONET.ValidateTracking(data_iDatabas,
                                data_iTables, data_iSystem,
                                data_iScanList, data_iView,
                                data_iTableTab, data_iTabSet,
                                data_iRelationship, ref oTrackObject,
                                ref oTrackDestination,
                                tTracking.sDestination, true, passport, httpContext, moTrackDestination
                                , ref errNum, _IDBManagerDefault, true);
                        }

                        bFound = string.IsNullOrEmpty(Strings.Trim(sTrackingReturn));

                        if (!bFound)
                        {
                            oTrackObject = default;
                            oTrackDestination = default;
                            errNum = 6;
                            if (errorIndex == errNum)
                            {
                                errorFlag = true;
                            }
                            else
                            {
                                errorFlag = false;
                            }
                            errorIndex = errNum;
                            if ((bool)oImportLoad.ReverseOrder)
                            {
                                abortRecordIndex = abortRecordIndex - 1;
                            }
                            else
                            {
                                abortRecordIndex = abortRecordIndex + 1;
                            }
                            sReturnMessage = sTrackingReturn;
                            WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                            ImportLoadRecordForQuietProcessingRet = true;
                            return ImportLoadRecordForQuietProcessingRet;
                        }
                    }
                }
                if (bFound && oTable is not null)
                {
                    ScriptReturn result;
                    if (bDoAddNew)
                    {
                        if ((oTable.LSBeforeAddRecord).Length > 0)
                        {
                            result = ScriptEngine.RunScriptBeforeAdd(oTable.TableName, passport);
                            bFound = result.Successful;
                        }
                    }
                    else if ((oTable.LSBeforeEditRecord).Length > 0)
                    {
                        result = ScriptEngine.RunScriptBeforeEdit(oTable.TableName, (string)vFieldIndex, passport);
                        bFound = result.Successful;
                    }
                }

                cFieldNames = new Collection();
                cFieldBeforeValues = new Collection();
                cFieldAfterValues = new Collection();
                if (bFound)
                {
                    tTracking.sDestination = "";
                    int lIndex = 0;
                    string sImportName;
                    string sFieldName;
                    string sDateFormat = "";
                    string vSwingYear = "";
                    object vField = "";
                    string sOCRText = "";
                    object sTemp;
                    bool bFoundTrackingSel;
                    RelationShip oRelationShip;
                    bool bFoundRelatedField;
                    string[] asDelimitedText;
                    var bDefaultRetentionCodeUsed = default(bool);
                    string sBeforeValue;
                    string sBeforeData = "";
                    string sAfterData = "";
                    string sMultiFieldName = "";
                    string sFindFieldName;
                    int lCnt;
                    string sCitationCode = "";
                    string sRetentionCode = "";
                    ADODB.Recordset rsTestADO;
                    DataTable rsTestADONET;
                    bool bRetentionDescriptionFound;
                    string sTrackingReturn = "";
                    var FieldNameCollection = new List<KeyValuePair<int, string>>();
                    string sCounterName;
                    string sCounterValue;
                    string sFusionCounterValue;
                    SystemCustModel oTempSys = default;
                    string sId = "";
                    var oBarCodeTable = new Table();
                    string sTableName = "";
                    string sImageName = "";
                    var bImageCopyField = default(bool);
                    var bPCFileCopyField = default(bool);
                    object pDefaultValue = null;

                    foreach (ImportField importFieldObj in oImportFields.OrderBy(m => m.ReadOrder))
                        FieldNameCollection.Add(new KeyValuePair<int, string>((int)importFieldObj.ReadOrder, importFieldObj.FieldName));
                    asDelimitedText = new string[mrsImport.Fields.Count];
                    for (int iCount = 0, loopTo = mrsImport.Fields.Count - 1; iCount <= loopTo; iCount++)
                    {
                        var oFieldVal = mrsImport.Fields[iCount].Value.ToString();
                        if (!(oFieldVal is DBNull))
                        {
                            StripQuotes(ref oFieldVal);
                            asDelimitedText[iCount] = (string)oFieldVal;
                        }
                    }
                    while (lIndex < oImportFields.Count & lIndex < mrsImport.Fields.Count)
                    {
                        if (lIndex <= mrsImport.Fields.Count - 1)
                        {
                            sImportName = mrsImport.Fields[lIndex].Name;
                        }
                        else
                        {
                            sImportName = oImportFields.Where(m => Operators.ConditionalCompareObjectEqual(m.ReadOrder, lIndex + 1, false)).FirstOrDefault().FieldName;
                        }
                        if (!string.IsNullOrEmpty(sImportName))
                        {
                            StripQuotes(ref sImportName);
                        }

                        if (bTrackingOnly)
                        {
                            maSkipFields = new string[oImportFields.Count];
                        }
                        else if (!bAutoIncrementCheck)
                        {
                            maSkipFields = new string[oImportFields.Count];

                            var loopTo1 = (int)(oImportFields.Count - 1L);
                            for (lFieldIndex = 0; lFieldIndex <= loopTo1; lFieldIndex++)
                            {
                                sFieldName = oImportFields.Where(m => Operators.ConditionalCompareObjectEqual(m.ReadOrder, lFieldIndex + 1, false)).FirstOrDefault().FieldName;
                                if (Strings.InStr(NON_FIELDS, "|" + sFieldName + "|") == 0)
                                {
                                    if (Strings.StrComp(sFieldName, dtRecords.Columns[sFieldName].ColumnName, Constants.vbTextCompare) == 0L)
                                    {
                                        if (dtRecords.Columns[sFieldName].AutoIncrement)
                                        {
                                            bHasAutoIncrement = true;
                                            maSkipFields[lFieldIndex] = SKIP_FIELD;
                                        }
                                    }
                                }
                            }
                            bAutoIncrementCheck = true;
                        }

                        sFieldName = oImportFields.Where(m => Operators.ConditionalCompareObjectEqual(m.ReadOrder, lIndex + 1, false)).FirstOrDefault().FieldName;
                        if (Strings.StrComp(sFieldName, SKIP_FIELD, Constants.vbTextCompare) != 0L & Strings.StrComp(maSkipFields[lIndex], SKIP_FIELD, Constants.vbTextCompare) != 0L)
                        {
                            sDateFormat = oImportFields.Where(m => Operators.ConditionalCompareObjectEqual(m.ReadOrder, lIndex + 1, false)).FirstOrDefault().DateFormat;
                            pDefaultValue = oImportFields.Where(m => Operators.ConditionalCompareObjectEqual(m.ReadOrder, lIndex + 1, false)).FirstOrDefault().DefaultValue;
                            vSwingYear = oImportFields.Where(m => Operators.ConditionalCompareObjectEqual(m.ReadOrder, lIndex + 1, false)).FirstOrDefault().SwingYear.ToString();
                            bool UseDefaultValue = false;
                            if (!string.IsNullOrEmpty(Convert.ToString(pDefaultValue)))
                            {
                                switch (pDefaultValue.ToString().ToUpper() ?? "")
                                {
                                    case "@@SL_USERNAME":
                                        {
                                            string sUserName = string.Empty;
                                            var sUserId = passport.UserId;
                                            if (sUserId != 0)
                                            {
                                                pDefaultValue = _iSecureUser.Where(m => Operators.ConditionalCompareObjectEqual(m.UserID, sUserId, false)).FirstOrDefault().UserName;
                                            }
                                            break;
                                        }
                                    case "@@TIME":
                                        {
                                            pDefaultValue = DateTime.Now.ToLongTimeString();
                                            // sDateFormat = "mm/dd/yyyy"
                                            break;
                                        }
                                }
                            }

                            if (lIndex <= mrsImport.Fields.Count - 1)
                            {
                                vField = mrsImport.Fields[sImportName].Value;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(Convert.ToString(pDefaultValue)))
                                {
                                    string localValue = pDefaultValue.ToString().Trim().ToUpper();
                                    if (localValue.Equals("@@TODAY") | localValue.Equals("TODAY") | localValue.Equals("@@NOW"))
                                    {
                                        UseDefaultValue = true;
                                    }
                                }

                                if (IsFunction(pDefaultValue))
                                {
                                    vField = ProcessFunctionValue(pDefaultValue);
                                }
                                else
                                {
                                    vField = pDefaultValue;
                                }
                            }

                            if (vField is DBNull)
                            {
                                vField = "";
                            }
                            else
                            {
                                string argsStripFrom1 = Convert.ToString(vField);
                                StripQuotes(ref argsStripFrom1);
                                vField = argsStripFrom1;
                                if (Strings.Len(vField) > 0)
                                {
                                    vField = vField.ToString();
                                }
                            }
                            bool exitDo = false;
                            switch (sFieldName ?? "")
                            {
                                case IMAGE_COPY:
                                case PCFILE_COPY:
                                    {
                                        sImageName = Strings.Trim(Convert.ToString(vField));
                                        if (Strings.Len(sImageName) > 0)
                                        {
                                            StripQuotes(ref sImageName);
                                            bImageCopyField = Strings.StrComp(sFieldName, IMAGE_COPY, Constants.vbTextCompare) == 0L;
                                            bPCFileCopyField = Strings.StrComp(sFieldName, PCFILE_COPY, Constants.vbTextCompare) == 0L;

                                            if (!(httpContext.Session.GetString("ImportFilePath") == null))
                                            {
                                                // If (Not String.IsNullOrEmpty(oImportLoad.InputFile)) Then
                                                if (!string.IsNullOrEmpty(httpContext.Session.GetString("ImportFilePath")))
                                                {
                                                    var DirectoryName = Path.GetDirectoryName(httpContext.Session.GetString("ImportFilePath") ?? "");
                                                    var DirectoryFolder = DirectoryName.Split(Path.DirectorySeparatorChar);
                                                    DirectoryName = DirectoryName.Replace(DirectoryFolder[DirectoryFolder.Length - 1], "");
                                                    DirectoryName = DirectoryName + "Attachment";
                                                    currentImageName = Path.GetFileName(sImageName);
                                                    sImageName = Path.Combine(DirectoryName, currentImageName);
                                                    if (!System.IO.File.Exists(sImageName))
                                                    {
                                                        errorFlag = false;
                                                        if ((bool)oImportLoad.ReverseOrder)
                                                        {
                                                            abortRecordIndex = abortRecordIndex - 1;
                                                        }
                                                        else
                                                        {
                                                            abortRecordIndex = abortRecordIndex + 1;
                                                        }
                                                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlFileNotFoundPlzVerify"), currentImageName);
                                                        WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, msImport: mrsImport);
                                                        ImportLoadRecordForQuietProcessingRet = true;
                                                        return ImportLoadRecordForQuietProcessingRet;
                                                    }
                                                    else if (oImportLoad.ScanRule is null)
                                                    {
                                                        int errNum = 25;
                                                        if (errorIndex == errNum)
                                                        {
                                                            errorFlag = true;
                                                        }
                                                        else
                                                        {
                                                            errorFlag = false;
                                                        }
                                                        errorIndex = errNum;
                                                        if ((bool)oImportLoad.ReverseOrder)
                                                        {
                                                            abortRecordIndex = abortRecordIndex - 1;
                                                        }
                                                        else
                                                        {
                                                            abortRecordIndex = abortRecordIndex + 1;
                                                        }
                                                        sReturnMessage = Languages.get_Translation("msgImportCtrlOptSettingsNotFound");
                                                        WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                        ImportLoadRecordForQuietProcessingRet = true;
                                                        return ImportLoadRecordForQuietProcessingRet;
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case ATTMT_LINK:
                                    {
                                        break;
                                    }
                                case TRACK_DEST:
                                    {
                                        tTracking.sDestination = Convert.ToString(vField);
                                        break;
                                    }
                                case TRACK_OPER:
                                    {
                                        TrackingServices.TelxonUserName = (string)vField;
                                        if (!bTrackingOnly)
                                        {
                                            TrackingServices.TelxonModeOn = true;
                                            TrackingServices.ScanDateTime = Convert.ToDateTime(TrackingServices.StartTime);
                                        }
                                        break;
                                    }
                                case TRACK__DUE:
                                    {
                                        sTemp = ConvertDateTime(vField, sDateFormat, pDefaultValue, vSwingYear, ref sReturnMessage, ref errorIndex, ref errorFlag, UseDefaultValue);
                                        if (!string.IsNullOrEmpty(sReturnMessage))
                                        {
                                            if ((bool)oImportLoad.ReverseOrder)
                                            {
                                                abortRecordIndex = abortRecordIndex - 1;
                                            }
                                            else
                                            {
                                                abortRecordIndex = abortRecordIndex + 1;
                                            }
                                            WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                            ImportLoadRecordForQuietProcessingRet = true;
                                            return ImportLoadRecordForQuietProcessingRet;
                                        }
                                        if (Strings.Len(sTemp) > 0L)
                                        {
                                            if (Strings.StrComp(Convert.ToString(sTemp), Constants.vbNullChar) != 0L)
                                            {
                                                tTracking.dDateDue = Convert.ToDateTime(sTemp);
                                            }
                                        }
                                        else
                                        {
                                            bRecordError = true;
                                        }

                                        break;
                                    }
                                case TRACK_DATE:
                                    {
                                        TrackingServices.TelxonModeOn = true;
                                        sTemp = ConvertDateTime(vField, sDateFormat, pDefaultValue, vSwingYear, ref sReturnMessage, ref errorIndex, ref errorFlag, UseDefaultValue);
                                        if (!string.IsNullOrEmpty(sReturnMessage))
                                        {
                                            if ((bool)oImportLoad.ReverseOrder)
                                            {
                                                abortRecordIndex = abortRecordIndex - 1;
                                            }
                                            else
                                            {
                                                abortRecordIndex = abortRecordIndex + 1;
                                            }
                                            WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                            ImportLoadRecordForQuietProcessingRet = true;
                                            return ImportLoadRecordForQuietProcessingRet;
                                        }
                                        if (Strings.Len(sTemp) > 0)
                                        {
                                            if (Strings.StrComp(Convert.ToString(sTemp), Constants.vbNullChar) == 0)
                                            {
                                                sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlCantBeBlank"), TRACK_DATE);
                                                int errNum = 7;
                                                if (errorIndex == errNum)
                                                {
                                                    errorFlag = true;
                                                }
                                                else
                                                {
                                                    errorFlag = false;
                                                }
                                                errorIndex = errNum;
                                                if ((bool)oImportLoad.ReverseOrder)
                                                {
                                                    abortRecordIndex = abortRecordIndex - 1;
                                                }
                                                else
                                                {
                                                    abortRecordIndex = abortRecordIndex + 1;
                                                }
                                                WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                ImportLoadRecordForQuietProcessingRet = true;
                                                return ImportLoadRecordForQuietProcessingRet;
                                            }
                                            else
                                            {
                                                TrackingServices.ScanDateTime = Convert.ToDateTime(sTemp);
                                            }
                                        }
                                        else
                                        {
                                            bRecordError = true;
                                        }

                                        break;
                                    }
                                case TRACK_RECN:
                                    {
                                        if ((bool)data_iSystem.FirstOrDefault().ReconciliationOn)
                                        {
                                            if (Convert.ToBoolean(Strings.InStr(1, "TR", Convert.ToString(vField), Constants.vbTextCompare)))
                                            {
                                                tTracking.bDoRecon = Strings.StrComp(Convert.ToString(vField), "R", Constants.vbTextCompare) == 0L;
                                            }
                                            else if (Strings.Len(Strings.Trim(Convert.ToString(vField))) == 0)
                                            {
                                                if (Convert.ToBoolean(Strings.Len(Strings.Trim(Convert.ToString(pDefaultValue)))))
                                                {
                                                    tTracking.bDoRecon = Convert.ToBoolean(pDefaultValue);
                                                }
                                            }
                                            else if (vField.ToString().Trim().ToUpper().Equals("TRUE") | vField.ToString().Trim().ToUpper().Equals("FALSE"))
                                            {
                                                tTracking.bDoRecon = Convert.ToBoolean(vField);
                                            }
                                        }
                                        break;
                                    }
                                case TRACK_ADDIT_FIELD1:
                                    {
                                        if (Convert.ToBoolean((data_iSystem.FirstOrDefault().TrackingAdditionalField1Desc).Length))
                                        {
                                            if (data_iSystem.FirstOrDefault().TrackingAdditionalField1Type.Equals((int)Enums.SelectionTypes.stSelection & Strings.Len(Strings.Trim(Convert.ToString(vField)))))
                                            {
                                                var SLTrackingSelectDataList = data_iSLTrackingSelectData.OrderBy(m => m.Id);
                                                bFoundTrackingSel = false;
                                                foreach (SLTrackingSelectData oSLTrackingSelections in SLTrackingSelectDataList)
                                                {
                                                    bFoundTrackingSel = Strings.StrComp(Strings.RTrim(oSLTrackingSelections.Id), Strings.RTrim(Convert.ToString(vField)), Constants.vbTextCompare) == 0L;
                                                    if (bFoundTrackingSel)
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bFoundTrackingSel = true;
                                            }

                                            if (bFoundTrackingSel)
                                            {
                                                tTracking.sTrackingAdditionalField1 = Convert.ToString(vField);
                                            }
                                            else
                                            {
                                                sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlFieldNotValid4Selection"), Strings.RTrim(Convert.ToString(vField)), _iSystem.All().FirstOrDefault().TrackingAdditionalField1Desc);
                                                int errNum = 8;
                                                if (errorIndex == errNum)
                                                {
                                                    errorFlag = true;
                                                }
                                                else
                                                {
                                                    errorFlag = false;
                                                }
                                                errorIndex = errNum;
                                                if ((bool)oImportLoad.ReverseOrder)
                                                {
                                                    abortRecordIndex = abortRecordIndex - 1;
                                                }
                                                else
                                                {
                                                    abortRecordIndex = abortRecordIndex + 1;
                                                }
                                                WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                ImportLoadRecordForQuietProcessingRet = true;
                                                return ImportLoadRecordForQuietProcessingRet;
                                            }
                                        }
                                        break;
                                    }
                                case TRACK_ADDIT_FIELD2:
                                    {
                                        if (Convert.ToBoolean((data_iSystem.FirstOrDefault().TrackingAdditionalField2Desc).Length))
                                        {
                                            tTracking.sTrackingAdditionalField2 = Convert.ToString(vField);
                                        }
                                        break;
                                    }
                                case TRACK__OBJ:
                                    {
                                        sReturnMessage = "";
                                        string LookupString = "";
                                        string sTabsetIds = "";
                                        Table oCurrentTable = null;
                                        bool bSecurityCheck = default;
                                        oTrackObject = GetInfoUsingADONET.BarCodeLookup(default, data_iDatabas,
                                            data_iTables, data_iSystem,
                                            data_iScanList, data_iView, data_iTableTab,
                                            data_iTabSet, data_iRelationship, LookupString, httpContext, ref sTabsetIds,
                                           ref oCurrentTable, ref bSecurityCheck, _IDBManagerDefault, false, ref sReturnMessage);
                                        if (oTrackObject is null)
                                        {
                                            exitDo = true;
                                            break;
                                        }
                                        break;
                                    }

                                default:
                                    {
                                        if (Strings.StrComp(oTable.TableName, "SLRetentionCodes", Constants.vbTextCompare) == 0)
                                        {
                                            if (Strings.StrComp(sFieldName, "ID", Constants.vbTextCompare) == 0)
                                            {
                                                // importing records directly into the Retention Code table
                                                if (Strings.InStr(1, Convert.ToString(vField), " ") > 0)
                                                {
                                                    int errNum = 9;
                                                    if (errorIndex == errNum)
                                                    {
                                                        errorFlag = true;
                                                    }
                                                    else
                                                    {
                                                        errorFlag = false;
                                                    }
                                                    errorIndex = errNum;
                                                    if ((bool)oImportLoad.ReverseOrder)
                                                    {
                                                        abortRecordIndex = abortRecordIndex - 1;
                                                    }
                                                    else
                                                    {
                                                        abortRecordIndex = abortRecordIndex + 1;
                                                    }
                                                    sReturnMessage = Languages.get_Translation("msgImportCtrlRentCodeNotContainBlankSpace");
                                                    WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                    ImportLoadRecordForQuietProcessingRet = true;
                                                    return ImportLoadRecordForQuietProcessingRet;
                                                }
                                            }
                                            else if (Strings.StrComp(sFieldName, "Description", Constants.vbTextCompare) == 0)
                                            {
                                                if (Operators.ConditionalCompareObjectEqual(vField, "", false))
                                                {
                                                    int errNum = 10;
                                                    if (errorIndex == errNum)
                                                    {
                                                        errorFlag = true;
                                                    }
                                                    else
                                                    {
                                                        errorFlag = false;
                                                    }
                                                    errorIndex = errNum;
                                                    if ((bool)oImportLoad.ReverseOrder)
                                                    {
                                                        abortRecordIndex = abortRecordIndex - 1;
                                                    }
                                                    else
                                                    {
                                                        abortRecordIndex = abortRecordIndex + 1;
                                                    }
                                                    sReturnMessage = Languages.get_Translation("msgImportCtrlRetentionDescRequired");
                                                    WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                    ImportLoadRecordForQuietProcessingRet = true;
                                                    return ImportLoadRecordForQuietProcessingRet;
                                                }
                                            }
                                        }

                                        if (Strings.StrComp(sFieldName, DatabaseMap.RemoveTableNameFromField(oTable.RetentionFieldName), Constants.vbTextCompare) == 0)
                                        {
                                            if (Strings.InStr(1, Convert.ToString(vField), " ") > 0)
                                            {
                                                int errNum = 11;
                                                if (errorIndex == errNum)
                                                {
                                                    errorFlag = true;
                                                }
                                                else
                                                {
                                                    errorFlag = false;
                                                }
                                                errorIndex = errNum;
                                                if ((bool)oImportLoad.ReverseOrder)
                                                {
                                                    abortRecordIndex = abortRecordIndex - 1;
                                                }
                                                else
                                                {
                                                    abortRecordIndex = abortRecordIndex + 1;
                                                }
                                                sReturnMessage = Languages.get_Translation("msgImportCtrlRentCodeNotContainBlankSpace");
                                                WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                ImportLoadRecordForQuietProcessingRet = true;
                                                return ImportLoadRecordForQuietProcessingRet;
                                            }
                                        }

                                        if ((bool)(oTable.RetentionPeriodActive | oTable.RetentionInactivityActive))
                                        {
                                            if (Strings.StrComp(sFieldName, DatabaseMap.RemoveTableNameFromField(oTable.RetentionFieldName), Constants.vbTextCompare) == 0)
                                            {
                                                if (oTable.RetentionAssignmentMethod == (int)Enums.meRetentionCodeAssignment.rcaRelatedTable)
                                                {
                                                    oRelationShip = data_iRelationship.Where(m => m.UpperTableName.Trim().ToLower().Equals(oTable.RetentionRelatedTable.Trim().ToLower()) & m.LowerTableName.Trim().ToLower().Equals(oTable.TableName.Trim().ToLower())).OrderBy(m => m.TabOrder).FirstOrDefault();
                                                    if (oRelationShip is null)
                                                    {
                                                        int errNum = 12;
                                                        if (errorIndex == errNum)
                                                        {
                                                            errorFlag = true;
                                                        }
                                                        else
                                                        {
                                                            errorFlag = false;
                                                        }
                                                        errorIndex = errNum;
                                                        if ((bool)oImportLoad.ReverseOrder)
                                                        {
                                                            abortRecordIndex = abortRecordIndex - 1;
                                                        }
                                                        else
                                                        {
                                                            abortRecordIndex = abortRecordIndex + 1;
                                                        }
                                                        sReturnMessage = Languages.get_Translation("msgImportCtrlRetenTblNotBlank");
                                                        WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                        ImportLoadRecordForQuietProcessingRet = true;
                                                        return ImportLoadRecordForQuietProcessingRet;
                                                    }
                                                    else
                                                    {
                                                        bFoundRelatedField = false;
                                                        var oLowerFieldName = DatabaseMap.RemoveTableNameFromField(oRelationShip.LowerTableFieldName);
                                                        var oRelatedFieldName = _iImportField.All().Where(m => m.FieldName.Trim().ToLower().Equals(oLowerFieldName.Trim().ToLower())).FirstOrDefault();
                                                        if (oRelatedFieldName is not null)
                                                        {
                                                            bFoundRelatedField = true;
                                                            if (string.IsNullOrEmpty(asDelimitedText[(int)(oRelatedFieldName.ReadOrder - 1)]))
                                                            {
                                                                int errNum = 13;
                                                                if (errorIndex == errNum)
                                                                {
                                                                    errorFlag = true;
                                                                }
                                                                else
                                                                {
                                                                    errorFlag = false;
                                                                }
                                                                errorIndex = errNum;
                                                                if ((bool)oImportLoad.ReverseOrder)
                                                                {
                                                                    abortRecordIndex = abortRecordIndex - 1;
                                                                }
                                                                else
                                                                {
                                                                    abortRecordIndex = abortRecordIndex + 1;
                                                                }
                                                                sReturnMessage = Languages.get_Translation("msgImportCtrlRetenTblNotBlank");
                                                                WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                                ImportLoadRecordForQuietProcessingRet = true;
                                                                return ImportLoadRecordForQuietProcessingRet;
                                                            }
                                                            else
                                                            {
                                                                var oCodeValue = GetInfoUsingADONET.GetRetentionCodeValueUsingADONET(oTable, data_iTables.Where(m => m.TableName.Trim().ToLower().Equals(oTable.RetentionRelatedTable.Trim().ToLower())).FirstOrDefault(),
                                                                    asDelimitedText[(int)(oRelatedFieldName.ReadOrder - 1)], false, data_iDatabas, _IDBManagerDefault, httpContext);
                                                                if (Strings.StrComp((string?)vField, oCodeValue, Constants.vbTextCompare) != 0)
                                                                {
                                                                    int errNum = 14;
                                                                    if (errorIndex == errNum)
                                                                    {
                                                                        errorFlag = true;
                                                                    }
                                                                    else
                                                                    {
                                                                        errorFlag = false;
                                                                    }
                                                                    errorIndex = errNum;
                                                                    if ((bool)oImportLoad.ReverseOrder)
                                                                    {
                                                                        abortRecordIndex = abortRecordIndex - 1;
                                                                    }
                                                                    else
                                                                    {
                                                                        abortRecordIndex = abortRecordIndex + 1;
                                                                    }
                                                                    sReturnMessage = Languages.get_Translation("msgImportCtrlRelTblNotMatchedWithCode");
                                                                    WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                                    ImportLoadRecordForQuietProcessingRet = true;
                                                                    return ImportLoadRecordForQuietProcessingRet;
                                                                }
                                                            }
                                                        }

                                                        if (!bFoundRelatedField)
                                                        {
                                                            if (Strings.StrComp((string?)vField, GetInfoUsingADONET.GetRetentionCodeValueUsingADONET(oTable, data_iTables.Where(m => m.TableName.Trim().ToLower().Equals(oTable.RetentionRelatedTable.Trim().ToLower())).FirstOrDefault(),
                                                                (string)dtRecords.Rows[0][DatabaseMap.RemoveTableNameFromField(oRelationShip.LowerTableFieldName)], false, data_iDatabas, _IDBManagerDefault, httpContext), Constants.vbTextCompare) != 0)
                                                            {
                                                                int errNum = 15;
                                                                if (errorIndex == errNum)
                                                                {
                                                                    errorFlag = true;
                                                                }
                                                                else
                                                                {
                                                                    errorFlag = false;
                                                                }
                                                                errorIndex = errNum;
                                                                if ((bool)oImportLoad.ReverseOrder)
                                                                {
                                                                    abortRecordIndex = abortRecordIndex - 1;
                                                                }
                                                                else
                                                                {
                                                                    abortRecordIndex = abortRecordIndex + 1;
                                                                }
                                                                sReturnMessage = Languages.get_Translation("msgImportCtrlRelTblNotMatchedWithCode");
                                                                WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                                ImportLoadRecordForQuietProcessingRet = true;
                                                                return ImportLoadRecordForQuietProcessingRet;
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (oTable.RetentionAssignmentMethod == (int)Enums.meRetentionCodeAssignment.rcaCurrentTable)
                                                {
                                                    // Make general GetRetentionCodeValue() method for all record
                                                    if (Strings.StrComp((string?)vField, GetInfoUsingADONET.GetRetentionCodeValueUsingADONET(oTable, default, "", false, data_iDatabas, _IDBManagerDefault, httpContext), Constants.vbTextCompare) != 0)
                                                    {
                                                        int errNum = 16;
                                                        if (errorIndex == errNum)
                                                        {
                                                            errorFlag = true;
                                                        }
                                                        else
                                                        {
                                                            errorFlag = false;
                                                        }
                                                        errorIndex = errNum;
                                                        if ((bool)oImportLoad.ReverseOrder)
                                                        {
                                                            abortRecordIndex = abortRecordIndex - 1;
                                                        }
                                                        else
                                                        {
                                                            abortRecordIndex = abortRecordIndex + 1;
                                                        }
                                                        sReturnMessage = Languages.get_Translation("msgImportCtrlRelTblNotMatchedWithCode");
                                                        WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                        ImportLoadRecordForQuietProcessingRet = true;
                                                        return ImportLoadRecordForQuietProcessingRet;
                                                    }
                                                }
                                                else
                                                {
                                                    bDefaultRetentionCodeUsed = Strings.Len(pDefaultValue) > 0;
                                                }
                                            }
                                        }

                                        // Added by Akruti
                                        string recordFieldVal = "";
                                        if (bDoAddNew)
                                        {
                                            recordFieldVal = "";
                                        }
                                        else if (dtRecords.Rows[0][sFieldName] is DBNull)
                                        {
                                            recordFieldVal = "";
                                        }
                                        else
                                        {
                                            recordFieldVal = Convert.ToString(dtRecords.Rows[0][sFieldName]);
                                        }

                                        sBeforeValue = recordFieldVal;
                                        if (GetInfoUsingADONET.IsAString(dtRecords.Columns[sFieldName].DataType.ToString()))
                                        {
                                            if (Strings.Len(Strings.Trim(Convert.ToString(vField))) == 0L)
                                            {
                                                vField = pDefaultValue;
                                            }

                                            if (Strings.StrComp(oImportFields.Where(m => Operators.ConditionalCompareObjectEqual(m.ReadOrder, lIndex + 1, false)).FirstOrDefault().FieldName, sMultiFieldName, Constants.vbTextCompare) == 0)
                                            {
                                                if (Strings.Len(Strings.Trim(Convert.ToString(pDefaultValue))) > 0L)
                                                {
                                                    if (Strings.StrComp(Convert.ToString(vField), Convert.ToString(pDefaultValue), Constants.vbTextCompare) != 0L)
                                                    {
                                                        vField = Operators.ConcatenateObject(pDefaultValue, vField);
                                                    }
                                                }
                                                if (dtRecords.Columns[sFieldName].MaxLength < 10000)
                                                {
                                                    ListOfClsFieldWithVal.Add(new ClsFieldWithVal() { colName = sFieldName, colVal = Strings.Left(Convert.ToString(Operators.ConcatenateObject(Strings.Trim(Convert.ToString(dtRecords.Rows[0][sFieldName])), vField)), dtRecords.Columns[sFieldName].MaxLength), colType = dtRecords.Columns[sFieldName].DataType });
                                                }
                                                else
                                                {
                                                    ListOfClsFieldWithVal.Add(new ClsFieldWithVal() { colName = sFieldName, colVal = Strings.Trim(Convert.ToString(Operators.ConcatenateObject(dtRecords.Rows[0][sFieldName], vField))), colType = dtRecords.Columns[sFieldName].DataType });
                                                }
                                            }
                                            else
                                            {
                                                sMultiFieldName = oImportFields.Where(m => Operators.ConditionalCompareObjectEqual(m.ReadOrder, lIndex + 1, false)).FirstOrDefault().FieldName;
                                                if (dtRecords.Columns[sFieldName].MaxLength < 10000)
                                                {
                                                    ListOfClsFieldWithVal.Add(new ClsFieldWithVal() { colName = sFieldName, colVal = Strings.Left(Strings.Trim(Convert.ToString(vField)), dtRecords.Columns[sFieldName].MaxLength), colType = dtRecords.Columns[sFieldName].DataType });
                                                }
                                                else
                                                {
                                                    ListOfClsFieldWithVal.Add(new ClsFieldWithVal() { colName = sFieldName, colVal = Strings.Trim(Convert.ToString(vField)), colType = dtRecords.Columns[sFieldName].DataType });
                                                }
                                            }
                                        }
                                        else if (GetInfoUsingADONET.IsADate(dtRecords.Columns[sFieldName].DataType.ToString()))
                                        {
                                            sTemp = ConvertDateTime(vField, sDateFormat, pDefaultValue, vSwingYear, ref sReturnMessage, ref errorIndex, ref errorFlag, UseDefaultValue);
                                            if (!string.IsNullOrEmpty(sReturnMessage))
                                            {
                                                if ((bool)oImportLoad.ReverseOrder)
                                                {
                                                    abortRecordIndex = abortRecordIndex - 1;
                                                }
                                                else
                                                {
                                                    abortRecordIndex = abortRecordIndex + 1;
                                                }
                                                WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                ImportLoadRecordForQuietProcessingRet = true;
                                                return ImportLoadRecordForQuietProcessingRet;
                                            }
                                            if (dtRecords.Columns[sFieldName].AllowDBNull || Strings.Len(sTemp) > 0L)
                                            {
                                                if (Strings.StrComp(Convert.ToString(sTemp), Constants.vbNullChar) == 0L)
                                                {
                                                    sTemp = null;
                                                }
                                                else if (string.IsNullOrEmpty(Strings.Trim(Convert.ToString(sTemp))) && dtRecords.Columns[sFieldName].AllowDBNull)
                                                {
                                                    sTemp = null;
                                                }
                                                ListOfClsFieldWithVal.Add(new ClsFieldWithVal() { colName = sFieldName, colVal = (string)sTemp, colType = dtRecords.Columns[sFieldName].DataType });
                                            }
                                            else
                                            {
                                                bRecordError = true;
                                            }
                                        }
                                        else
                                        {
                                            if (vField is DBNull | Strings.Len(Strings.Trim(Convert.ToString(vField))) == 0L)
                                            {
                                                vField = pDefaultValue;
                                            }
                                            if (dtRecords.Columns[sFieldName].AutoIncrement == false)
                                            {
                                                ListOfClsFieldWithVal.Add(new ClsFieldWithVal() { colName = sFieldName, colVal = (string)vField, colType = dtRecords.Columns[sFieldName].DataType });
                                            }
                                        }
                                        if ((bool)oTable.AuditUpdate)
                                        {
                                            var AfterValRecord = ListOfClsFieldWithVal.Where(m => m.colName.Trim().ToLower().Equals(sFieldName.Trim().ToLower())).FirstOrDefault();
                                            object AfterVal = null;
                                            if (AfterValRecord is not null)
                                            {
                                                AfterVal = AfterValRecord.colVal;
                                            }

                                            if (Strings.StrComp(sBeforeValue, Convert.ToString(AfterVal), Constants.vbTextCompare) != 0)
                                            {
                                                sFindFieldName = "";
                                                var loopTo2 = cFieldNames.Count;
                                                for (lCnt = 1; lCnt <= loopTo2; lCnt++)
                                                {
                                                    sFindFieldName = Convert.ToString(cFieldNames[lCnt]);
                                                    if (Strings.StrComp(sFindFieldName, DatabaseMap.RemoveTableNameFromField(sFieldName), Constants.vbTextCompare) == 0)
                                                    {
                                                        break;
                                                    }
                                                    sFindFieldName = "";
                                                }
                                                if (Strings.Len(Strings.Trim(sFindFieldName)) == 0)
                                                {
                                                    cFieldBeforeValues.Add(sBeforeValue);
                                                    cFieldNames.Add(DatabaseMap.RemoveTableNameFromField(sFieldName));
                                                    cFieldAfterValues.Add(AfterVal);
                                                }
                                                else
                                                {
                                                    cFieldAfterValues.Add(AfterVal, Before: lCnt);
                                                    cFieldAfterValues.Remove(lCnt);
                                                }
                                            }
                                        }
                                        break;
                                    }
                            }

                            if (exitDo)
                            {
                                break;
                            }
                        }
                        lIndex = (int)(lIndex + 1L);
                    }
                    if (!bTrackingOnly)
                    {
                        if (Strings.StrComp(oTable.TableName, "SLRetentionCitaCodes", Constants.vbTextCompare) == 0L)
                        {
                            for (long lRetentionFieldCounter = 0L, loopTo3 = oImportFields.Count - 1; lRetentionFieldCounter <= loopTo3; lRetentionFieldCounter++)
                            {
                                if (Strings.StrComp(FieldNameCollection[(int)lRetentionFieldCounter].Value, "SLRetentionCitationsCitation", Constants.vbTextCompare) == 0)
                                {
                                    sCitationCode = asDelimitedText[(int)lRetentionFieldCounter];
                                }
                                else if (Strings.StrComp(FieldNameCollection[(int)lRetentionFieldCounter].Value, "SLRetentionCodesId", Constants.vbTextCompare) == 0)
                                {
                                    sRetentionCode = asDelimitedText[(int)lRetentionFieldCounter];
                                }
                            }

                            if (Operators.CompareString(sCitationCode, "", false) > 0 & Operators.CompareString(sRetentionCode, "", false) > 0)
                            {
                                string sErrMsg = "";
                                lError = 0;
                                sSql = "SELECT TOP 1 * FROM [SLRetentionCitaCodes] WHERE [SLRetentionCitationsCitation] = '" + sCitationCode + "' AND [SLRetentionCodesId] = '" + sRetentionCode + "'";
                                rsTestADONET = GetInfoUsingADONET.GetADONETRecord(sSql, _IDBManagerDefault, ref sErrMsg, ref lError);
                                if (rsTestADONET is not null)
                                {
                                    if (rsTestADONET.Rows.Count > 0L)
                                    {
                                        rsTestADONET = null;
                                        int errNum = 17;
                                        if (errorIndex == errNum)
                                        {
                                            errorFlag = true;
                                        }
                                        else
                                        {
                                            errorFlag = false;
                                        }
                                        errorIndex = errNum;
                                        if ((bool)oImportLoad.ReverseOrder)
                                        {
                                            abortRecordIndex = abortRecordIndex - 1;
                                        }
                                        else
                                        {
                                            abortRecordIndex = abortRecordIndex + 1;
                                        }
                                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlRetentionCodeAlreadyAssignCitationCode"), sRetentionCode, sCitationCode);
                                        WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                        ImportLoadRecordForQuietProcessingRet = true;
                                        return ImportLoadRecordForQuietProcessingRet;
                                    }
                                }
                            }
                        }
                        if (Strings.StrComp(oTable.TableName, "SLRetentionCodes", Constants.vbTextCompare) == 0)
                        {
                            bRetentionDescriptionFound = false;

                            for (long lRetentionFieldCounter = 0L, loopTo4 = FieldNameCollection.Count - 1; lRetentionFieldCounter <= loopTo4; lRetentionFieldCounter++)
                            {
                                if (Strings.StrComp(FieldNameCollection[(int)lRetentionFieldCounter].Value, "Description", Constants.vbTextCompare) == 0L)
                                    bRetentionDescriptionFound = true;
                            }

                            if (!bRetentionDescriptionFound)
                            {
                                int errNum = 18;
                                if (errorIndex == errNum)
                                {
                                    errorFlag = true;
                                }
                                else
                                {
                                    errorFlag = false;
                                }
                                errorIndex = errNum;
                                if ((bool)oImportLoad.ReverseOrder)
                                {
                                    abortRecordIndex = abortRecordIndex - 1;
                                }
                                else
                                {
                                    abortRecordIndex = abortRecordIndex + 1;
                                }
                                sReturnMessage = Languages.get_Translation("msgImportCtrlRetentionDescRequired");
                                WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                ImportLoadRecordForQuietProcessingRet = true;
                                return ImportLoadRecordForQuietProcessingRet;
                            }
                        }
                    }
                    if (!bRecordError)
                    {
                        if (bTrackingOnly)
                        {
                            var errNum = default(int);
                            sTrackingReturn = "";
                            if (oTrackObject is null)
                            {
                                sTrackingReturn = Languages.get_Translation("msgImportCtrlTracObjNotSupplied");
                                errNum = 19;
                            }
                            else if (Convert.ToBoolean(Strings.Len(tTracking.sDestination)))
                            {
                                sTrackingReturn = GetInfoUsingADONET.ValidateTracking(data_iDatabas, data_iTables, data_iSystem, data_iScanList, data_iView,
                                    data_iTableTab, data_iTabSet, data_iRelationship,
                                    ref oTrackObject, ref oTrackDestination, tTracking.sDestination, false, passport, httpContext,
                                    default, ref errNum, _IDBManagerDefault, true);
                            }

                            else
                            {
                                sTrackingReturn = Languages.get_Translation("msgImportCtrlTracDestNotSupplied");
                                errNum = 32;
                            }

                            if (Strings.Len(sTrackingReturn) > 0)
                            {
                                oTrackObject = default;
                                oTrackDestination = default;
                                if (errorIndex == errNum)
                                {
                                    errorFlag = true;
                                }
                                else
                                {
                                    errorFlag = false;
                                }
                                errorIndex = errNum;
                                if ((bool)oImportLoad.ReverseOrder)
                                {
                                    abortRecordIndex = abortRecordIndex - 1;
                                }
                                else
                                {
                                    abortRecordIndex = abortRecordIndex + 1;
                                }
                                sReturnMessage = sTrackingReturn;
                                WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                ImportLoadRecordForQuietProcessingRet = true;
                                return ImportLoadRecordForQuietProcessingRet;
                            }
                            else
                            {
                                errNum = errorIndex;
                            }
                            if ((bool)(data_iSystem.FirstOrDefault().TrackingOutOn & data_iSystem.FirstOrDefault().DateDueOn))
                            {
                                if (tTracking.dDateDue == DateTime.MinValue)
                                {
                                    if (oImportLoad.DateDue is not null)
                                    {
                                        tTracking.dDateDue = (DateTime)oImportLoad.DateDue;
                                    }
                                    else if (oTrackDestination.DueBackDays != default)
                                    {
                                        tTracking.dDateDue = DateTime.Now.AddDays(oTrackDestination.DueBackDays);
                                    }
                                    else
                                    {
                                        tTracking.dDateDue = DateTime.Now.AddDays((double)data_iSystem.FirstOrDefault().DefaultDueBackDays);
                                    }
                                }
                                if (oTrackDestination.IsOut)
                                {
                                    if (tTracking.dDateDue == DateTime.MinValue)
                                    {
                                        errNum = 33;
                                        sTrackingReturn = Languages.get_Translation("msgBarCodeTrackingDueBackDateReq");
                                    }
                                    else if (DateTime.Parse(tTracking.dDateDue.ToShortDateString()) < DateTime.Parse(DateTime.Now.ToShortDateString()))
                                    {
                                        errNum = 34;
                                        sTrackingReturn = Languages.get_Translation("DueBackDateLessThanCurrent");
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(sTrackingReturn))
                            {
                                errorIndex = 0;
                                errorFlag = false;
                                if ((bool)oImportLoad.ReverseOrder)
                                {
                                    abortRecordIndex = abortRecordIndex - 1;
                                }
                                else
                                {
                                    abortRecordIndex = abortRecordIndex + 1;
                                }
                                WriteErrorFile(oImportLoad, sTrackingReturn, errorFlag, ref recordErrorTrack, default, mrsImport);
                                ImportLoadRecordForQuietProcessingRet = true;
                                return ImportLoadRecordForQuietProcessingRet;
                            }

                            if (oTrackObject is not null & oTrackDestination is not null)
                            {
                                sTrackingReturn = string.Empty;
                                TrackingServices.PrepareTransferDataForImport(_IDBManagerDefault, _iSystem, _iSecureUser, _iTables, _iTrackingStatus, _iAssetStatus, _iDatabas, oTrackObject, oTrackDestination, tTracking.bDoRecon, tTracking.dDateDue, passport, tTracking.sTrackingAdditionalField1, tTracking.sTrackingAdditionalField2, default, ref sTrackingReturn);
                                if (!string.IsNullOrEmpty(sTrackingReturn))
                                {
                                    errorIndex = 0;
                                    errorFlag = false;
                                    if ((bool)oImportLoad.ReverseOrder)
                                    {
                                        abortRecordIndex = abortRecordIndex - 1;
                                    }
                                    else
                                    {
                                        abortRecordIndex = abortRecordIndex + 1;
                                    }
                                    WriteErrorFile(oImportLoad, sTrackingReturn, errorFlag, ref recordErrorTrack, default, mrsImport);
                                    ImportLoadRecordForQuietProcessingRet = true;
                                    return ImportLoadRecordForQuietProcessingRet;
                                }
                                if (Strings.Len(sTrackingReturn) == 0)
                                {
                                    mlRecordsChanged = (int)(mlRecordsChanged + 1L);
                                }
                            }
                        }
                        else
                        {
                            var IsIdFieldString = GetInfoUsingADONET.IsAString(dtRecords.Columns[DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)].DataType.ToString());
                            var oIdFieldObject = ListOfClsFieldWithVal.Where(m => m.colName.Trim().ToLower().Equals(DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName).Trim().ToLower())).FirstOrDefault();
                            sCounterName = Strings.Trim(oTable.CounterFieldName);
                            if (Strings.Len(sCounterName) > 0 & !bHasAutoIncrement)
                            {
                                lError = 0;
                                string sErrMsg = "";
                                sCounterValue = "";
                                sFusionCounterValue = "0";
                                rsTestADONET = GetInfoUsingADONET.GetADONETRecord("SELECT [" + sCounterName + "] FROM [System]", _IDBManagerDefault, ref sErrMsg, ref lError);

                                if (rsTestADONET is not null)
                                {
                                    if (rsTestADONET.Rows.Count > 0)
                                        sFusionCounterValue = Convert.ToString(rsTestADONET.AsEnumerable().ElementAtOrDefault(0)[sCounterName]);
                                    rsTestADONET = null;
                                }
                                if (dtRecords.Rows.Count > 0)
                                {
                                    sCounterValue = Convert.ToString(dtRecords.Rows[0][DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)]);
                                }
                                else
                                {
                                    sCounterValue = 0.ToString();
                                }

                                if (Strings.Len(Strings.Trim(sCounterValue)) == 0)
                                {
                                    // Make Sure the id in question is not in use
                                    do
                                    {
                                        oTempSys = GetInfoUsingADONET.IncrementCounter(sCounterName, _IDBManagerDefault);
                                        if (IsIdFieldString)
                                        {
                                            sSql = "SELECT [" + DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName) + "] FROM [" + oTable.TableName + "] WHERE [" + DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName) + "] = '" + Strings.Format(oTempSys.get_CounterValue(sCounterName)) + "'";
                                        }
                                        else
                                        {
                                            sSql = "SELECT [" + DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName) + "] FROM [" + oTable.TableName + "] WHERE [" + DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName) + "] = " + Strings.Format(oTempSys.get_CounterValue(sCounterName));
                                        }
                                        rsTestADONET = GetInfoUsingADONET.GetADONETRecord(sSql, commonIDBManager, ref sErrMsg, ref lError);
                                        if (rsTestADONET is not null)
                                        {
                                            if (rsTestADONET.Rows.Count == 0)
                                            {
                                                rsTestADONET = null;
                                                break;
                                            }
                                            rsTestADO = default;
                                        }
                                    }
                                    while (true);
                                    if (oIdFieldObject is not null)
                                    {
                                        ListOfClsFieldWithVal.Remove(oIdFieldObject);
                                        ListOfClsFieldWithVal.Add(new ClsFieldWithVal() { colName = DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), colVal = oTempSys.get_CounterValue(sCounterName), colType = dtRecords.Columns[DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)].DataType });
                                    }
                                    oTempSys = default;
                                }
                                else if (Information.IsNumeric(sCounterValue))
                                {
                                    if (Convert.ToDouble(sFusionCounterValue) < Convert.ToDouble(sCounterValue) | Convert.ToDouble(sCounterValue) == 0.0d)
                                        GetInfoUsingADONET.IncrementCounter(sCounterName, _IDBManagerDefault, sCounterValue);
                                    if (Convert.ToDouble(sCounterValue) == 0.0d)
                                    {
                                        if (oIdFieldObject is not null)
                                        {
                                            ListOfClsFieldWithVal.Remove(oIdFieldObject);
                                            ListOfClsFieldWithVal.Add(new ClsFieldWithVal() { colName = DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), colVal = sFusionCounterValue, colType = dtRecords.Columns[DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)].DataType });
                                        }
                                    }
                                }
                            }

                            lError = 0;
                            bool IsIdAutoIncrement = true;
                            var oNewIdFieldObject = ListOfClsFieldWithVal.Where(m => m.colName.Trim().ToLower().Equals(DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName).Trim().ToLower())).FirstOrDefault();
                            if (oNewIdFieldObject is not null)
                            {
                                IsIdAutoIncrement = false;
                                UpdatedRecordIdVal = oNewIdFieldObject.colVal;
                            }
                            if (!bDoAddNew)
                            {
                                UpdatedRecordIdVal = Convert.ToString(dtRecords.Rows[0][DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName).Trim().ToLower()]);
                            }
                            sReturnMessage = "";
                            var sqlFinalQuery = GetInfoUsingADONET.PrepareSQLUsingList(bDoAddNew, bFound, ListOfClsFieldWithVal, oTable, UpdatedRecordIdVal, IsIdFieldString, IsIdAutoIncrement, oImportLoad.IdFieldName, UpdateByFieldVal, ref sReturnMessage);

                            if (Strings.Len(sReturnMessage.Trim()) == 0)
                            {
                                string sErrMsg = "";
                                sReturnMessage = "";
                                dtRecords = TabFusionRMS.WebCS.GetInfoUsingADONET.GetADONETRecord(sqlFinalQuery, commonIDBManager, ref sReturnMessage, ref lError);
                            }

                            if (dtRecords is null | !string.IsNullOrEmpty(sReturnMessage))
                            {
                                int errNum = 27;
                                if (errorIndex == errNum)
                                {
                                    errorFlag = true;
                                }
                                else
                                {
                                    errorFlag = false;
                                }
                                // errorIndex = errNum Changed by nikunj to fix: FUS-5850
                                if ((bool)oImportLoad.ReverseOrder)
                                {
                                    abortRecordIndex = abortRecordIndex - 1;
                                }
                                else
                                {
                                    abortRecordIndex = abortRecordIndex + 1;
                                }
                                WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                ImportLoadRecordForQuietProcessingRet = true;
                                return ImportLoadRecordForQuietProcessingRet;
                            }
                            if (dtRecords.Rows.Count > 0)
                            {
                                sId = Convert.ToString(dtRecords.Rows[0][DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)]);
                            }

                            if (Strings.Len(sId) > 0)
                            {
                                if ((bool)(oTable.RetentionPeriodActive | oTable.RetentionInactivityActive))
                                {
                                    if (dtRecords.Rows[0][DatabaseMap.RemoveTableNameFromField(oTable.RetentionFieldName)] is DBNull)
                                    {
                                        if (oTable.RetentionAssignmentMethod == (int)Enums.meRetentionCodeAssignment.rcaManual)
                                        {
                                            if (!bDefaultRetentionCodeUsed)
                                            {
                                                GetInfoUsingADONET.AssignRetentionCode(oTable, sId, data_iDatabas, data_iTables, data_iRelationship, httpContext, _IDBManagerDefault, _IDBManager);
                                            }
                                        }
                                        else
                                        {
                                            GetInfoUsingADONET.AssignRetentionCode(oTable, sId, data_iDatabas, data_iTables, data_iRelationship, httpContext, _IDBManagerDefault, _IDBManager);
                                        }
                                    }
                                }
                            }
                            var TextSearchItems = data_iSLTextSearchItems.Where(m => m.IndexTableName.Trim().ToLower().Equals(oTable.TableName.Trim().ToLower()));
                            if (lError == 0 & TextSearchItems.Count() > 0 & Strings.Len(sId) > 0)
                            {
                                GetInfoUsingADONET.UpdateFullTextIndexerFieldType(sId, oTable, dtRecords, bDoAddNew, data_iSLTextSearchItems, _iSLIndexer);
                            }

                            if (Strings.Len(sId) > 0)
                            {
                                if ((bool)oTable.AuditUpdate)
                                {
                                    if (cFieldNames.Count > 0L)
                                    {
                                        if (bDoAddNew)
                                        {
                                            sFindFieldName = "";
                                            var loopTo5 = cFieldNames.Count;
                                            for (lCnt = 1; lCnt <= loopTo5; lCnt++)
                                            {
                                                sFindFieldName = Convert.ToString(cFieldNames[lCnt]);
                                                if (Strings.StrComp(sFindFieldName, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), Constants.vbTextCompare) == 0L)
                                                {
                                                    break;
                                                }
                                                sFindFieldName = "";
                                            }
                                            if (Strings.Len(sFindFieldName) == 0L)
                                            {
                                                cFieldBeforeValues.Add("");
                                                cFieldNames.Add(DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName));
                                                cFieldAfterValues.Add(dtRecords.Rows[0][DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)]);
                                            }
                                            else
                                            {
                                                cFieldAfterValues.Add(dtRecords.Rows[0][DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)], lCnt.ToString());
                                                cFieldAfterValues.Remove(lCnt);
                                            }
                                        }

                                        var loopTo6 = cFieldNames.Count;
                                        for (lCnt = 1; lCnt <= loopTo6; lCnt++)
                                        {
                                            sBeforeData = Convert.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(sBeforeData, cFieldNames[lCnt]), ": "), cFieldBeforeValues[lCnt]), Constants.vbCrLf));
                                            sAfterData = Convert.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(sAfterData, cFieldNames[lCnt]), ": "), cFieldAfterValues[lCnt]), Constants.vbCrLf));
                                        }
                                        if (Strings.Len(sAfterData) > 2)
                                        {
                                            // Strip last CF LF
                                            sAfterData = Strings.Left(sAfterData, Strings.Len(sAfterData) - 2);
                                        }
                                        if (Strings.Len(sBeforeData) > 2)
                                        {
                                            // Strip last CF LF
                                            sBeforeData = Strings.Left(sBeforeData, Strings.Len(sBeforeData) - 2);
                                        }
                                        var passport = new Smead.Security.Passport();
                                        var oSLAuditUpdates = new SLAuditUpdate();
                                        {
                                            ref var withBlock = ref oSLAuditUpdates;
                                            withBlock.OperatorsId = httpContext.Session.GetString("UserName") ?? "";
                                            withBlock.TableName = oTable.TableName;
                                            if (bDoAddNew)
                                            {
                                                withBlock.Action = "Add Record";
                                                withBlock.ActionType = (int?)AuditType.ImportWizardActionType.AddRecord;
                                            }
                                            else
                                            {
                                                withBlock.DataBefore = sBeforeData;
                                                withBlock.Action = "Update Record";
                                                withBlock.ActionType = (int?)AuditType.ImportWizardActionType.UpdateRecord;
                                            }
                                            withBlock.TableId = GetInfoUsingADONET.ZeroPaddedString(oTable, sId, _IDBManagerDefault, IsIdFieldString);
                                            withBlock.UpdateDateTime = DateTime.Now;

                                            withBlock.DataAfter = sAfterData;
                                            withBlock.Domain = passport.DomainName;
                                            withBlock.ComputerName = passport.ComputerName;
                                            withBlock.IP = Keys.GetClientIpAddress(httpContext);
                                            withBlock.Module = "Import Wizard";
                                        }
                                        if (oSLAuditUpdates is not null)
                                        {
                                            _iSLAuditUpdate.Add(oSLAuditUpdates);
                                            // ' Need to work PJ
                                            GetInfoUsingADONET.WalkRelationshipsForAuditUpdates(sId, ref oSLAuditUpdates, oTable, oTable, (int)0L, DateTime.Now, data_iDatabas, data_iRelationship, data_iTables, _iSLAuditUpdChildren, httpContext, _IDBManagerDefault, _IDBManager);
                                            oSLAuditUpdates = default;
                                        }
                                    }
                                }

                                if (bDoAddNew)
                                {
                                    if ((oTable.LSAfterAddRecord).Length > 0)
                                    {
                                        ScriptEngine.RunScriptAfterAdd(oTable.TableName, sId, passport);
                                    }
                                    if (oTrackDestination is null & (long)(oTable.DefaultTrackingId).Length > 0L)
                                    {
                                        oTrackDestination = oDefaultTrackDestination;
                                    }
                                }
                                else if ((oTable.LSAfterEditRecord).Length > 0)
                                {
                                    ScriptEngine.RunScriptAfterEdit(oTable.TableName, sId, passport);
                                }
                                if (lError == 0)
                                {
                                    if (oTrackObject is not null)
                                    {
                                        var errNum = default(int);
                                        sTrackingReturn = "";
                                        if (Convert.ToBoolean(Strings.Len(tTracking.sDestination)))
                                        {
                                            sTrackingReturn = GetInfoUsingADONET.ValidateTracking(data_iDatabas, data_iTables, data_iSystem, data_iScanList, data_iView, data_iTableTab, data_iTabSet, data_iRelationship, ref oTrackObject, ref oTrackDestination, tTracking.sDestination, false, passport, httpContext, default, ref errNum, _IDBManagerDefault, true);
                                        }
                                        else if (moTrackDestination is not null)
                                        {
                                            sTrackingReturn = GetInfoUsingADONET.ValidateTracking(data_iDatabas, data_iTables, data_iSystem, data_iScanList, data_iView, data_iTableTab, data_iTabSet, data_iRelationship, ref oTrackObject, ref oTrackDestination, tTracking.sDestination, true, passport, httpContext, moTrackDestination, ref errNum, _IDBManagerDefault, true);
                                        }
                                        if (Strings.Len(sTrackingReturn) == 0 & oTrackObject.oTable is not null)
                                        {


                                            sTrackingReturn = GetInfoUsingADONET.ValidateRetention(default, data_iDatabas, ref oTable, ref sId, _IDBManagerDefault, httpContext);
                                        }
                                        if (Strings.Len(sTrackingReturn) > 0)
                                        {
                                            oTrackObject = default;
                                            oTrackDestination = default;
                                            sReturnMessage = sTrackingReturn;

                                            if (errorIndex == errNum)
                                            {
                                                errorFlag = true;
                                            }
                                            else
                                            {
                                                errorFlag = false;
                                            }
                                            errorIndex = errNum;
                                            if ((bool)oImportLoad.ReverseOrder)
                                            {
                                                abortRecordIndex = abortRecordIndex - 1;
                                            }
                                            else
                                            {
                                                abortRecordIndex = abortRecordIndex + 1;
                                            }
                                            WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                            ImportLoadRecordForQuietProcessingRet = true;
                                            return ImportLoadRecordForQuietProcessingRet;
                                        }
                                        else if (oTrackDestination is not null)
                                        {
                                            oTrackObject.Id = (string)dtRecords.Rows[0][DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)];
                                            oTrackObject.TypedText = oTable.BarCodePrefix + oTrackObject.Id;
                                            oTrackObject.UserLinkId = oTrackObject.Id;
                                            if (!IsIdFieldString)
                                            {
                                                // Append values...
                                                oTrackObject.UserLinkId = GetInfoUsingADONET.ZeroPaddedString(oTrackObject.oTable, oTrackObject.UserLinkId, _IDBManagerDefault, IsIdFieldString);
                                            }
                                            if ((bool)(data_iSystem.FirstOrDefault().TrackingOutOn & data_iSystem.FirstOrDefault().DateDueOn))
                                            {
                                                if (tTracking.dDateDue == DateTime.MinValue)
                                                {
                                                    if (oImportLoad.DateDue is not null)
                                                    {
                                                        tTracking.dDateDue = (DateTime)oImportLoad.DateDue;
                                                    }
                                                    else if (oTrackDestination.DueBackDays != default)
                                                    {
                                                        tTracking.dDateDue = DateTime.Now.AddDays(oTrackDestination.DueBackDays);
                                                    }
                                                    else
                                                    {
                                                        tTracking.dDateDue = DateTime.Now.AddDays((double)data_iSystem.FirstOrDefault().DefaultDueBackDays);
                                                    }
                                                }
                                                if (oTrackDestination.IsOut)
                                                {
                                                    if (tTracking.dDateDue == DateTime.MinValue)
                                                    {
                                                        errNum = 33;
                                                        sTrackingReturn = Languages.get_Translation("msgBarCodeTrackingDueBackDateReq");
                                                    }
                                                    else if (DateTime.Parse(tTracking.dDateDue.ToShortDateString()) < DateTime.Parse(DateTime.Now.ToShortDateString()))
                                                    {
                                                        errNum = 34;
                                                        sTrackingReturn = Languages.get_Translation("DueBackDateLessThanCurrent");
                                                    }
                                                }
                                            }

                                            if (Strings.Len(sTrackingReturn) > 0)
                                            {
                                                oTrackObject = default;
                                                oTrackDestination = default;
                                                if (errorIndex == errNum)
                                                {
                                                    errorFlag = true;
                                                }
                                                else
                                                {
                                                    errorFlag = false;
                                                }
                                                errorIndex = errNum;
                                                if ((bool)oImportLoad.ReverseOrder)
                                                {
                                                    abortRecordIndex = abortRecordIndex - 1;
                                                }
                                                else
                                                {
                                                    abortRecordIndex = abortRecordIndex + 1;
                                                }
                                                sReturnMessage = sTrackingReturn;
                                                WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                ImportLoadRecordForQuietProcessingRet = true;
                                                return ImportLoadRecordForQuietProcessingRet;
                                            }
                                            else
                                            {
                                                errNum = errorIndex;
                                            }
                                            sReturnMessage = string.Empty;
                                            TrackingServices.PrepareTransferDataForImport(_IDBManagerDefault, _iSystem, _iSecureUser, _iTables, _iTrackingStatus, _iAssetStatus, _iDatabas, oTrackObject, oTrackDestination, tTracking.bDoRecon, tTracking.dDateDue, passport, tTracking.sTrackingAdditionalField1, tTracking.sTrackingAdditionalField2, default, ref sReturnMessage);

                                            if (!string.IsNullOrEmpty(sReturnMessage))
                                            {
                                                errorIndex = 0;
                                                errorFlag = false;
                                                if ((bool)oImportLoad.ReverseOrder)
                                                {
                                                    abortRecordIndex = abortRecordIndex - 1;
                                                }
                                                else
                                                {
                                                    abortRecordIndex = abortRecordIndex + 1;
                                                }
                                                WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                                ImportLoadRecordForQuietProcessingRet = true;
                                                return ImportLoadRecordForQuietProcessingRet;
                                            }
                                        }
                                    }

                                    if (bDoAddNew)
                                    {
                                        mlRecordsAdded = (int)(mlRecordsAdded + 1L);
                                    }
                                    else
                                    {
                                        mlRecordsChanged = (int)(mlRecordsChanged + 1L);
                                    }
                                }
                                if (bImageCopyField | bPCFileCopyField)
                                {
                                    string sReturnError = "";
                                    var oOutputSetting = data_iOutputSetting.Where(m => m.Id.Trim().ToLower().Equals(oImportLoad.ScanRule.Trim().ToLower())).FirstOrDefault();
                                    ImageCopyForQuietProcessing(bImageCopyField, oTable, oOutputSetting, sImageName, (string)dtRecords.Rows[0][DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)], sOCRText, sReturnError, oImportLoad, _IDBManagerDefault, IsIdFieldString);
                                    if (!string.IsNullOrEmpty(sReturnError))
                                    {
                                        errorIndex = 0;
                                        errorFlag = false;
                                        if ((bool)oImportLoad.ReverseOrder)
                                        {
                                            abortRecordIndex = abortRecordIndex - 1;
                                        }
                                        else
                                        {
                                            abortRecordIndex = abortRecordIndex + 1;
                                        }
                                        sReturnMessage = sReturnError;
                                        WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                        ImportLoadRecordForQuietProcessingRet = true;
                                        return ImportLoadRecordForQuietProcessingRet;
                                    }
                                    if (!Directory.Exists(sDirectory))
                                    {
                                        string DefaultsDirectory = "";
                                        if (VolumePath != @"\")
                                        {
                                            DefaultsDirectory = VolumePath;
                                        }
                                        else
                                        {
                                            DefaultsDirectory = "";
                                        }
                                        if (!Directory.Exists(DefaultsDirectory))
                                        {
                                            errorFlag = false;
                                            if ((bool)oImportLoad.ReverseOrder)
                                            {
                                                abortRecordIndex = abortRecordIndex - 1;
                                            }
                                            else
                                            {
                                                abortRecordIndex = abortRecordIndex + 1;
                                            }
                                            sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlProblemUsingDefaultOutputWithBR"), " <br/><br/>", currentImageName);
                                            string TempsErrorMesg = string.Format(Languages.get_Translation("msgImportCtrlProblemUsingDefaultOutput"), currentImageName);
                                            WriteErrorFile(oImportLoad, TempsErrorMesg, errorFlag, ref recordErrorTrack, default, mrsImport);
                                            ImportLoadRecordForQuietProcessingRet = true;
                                            return ImportLoadRecordForQuietProcessingRet;
                                        }
                                    }
                                }
                            }
                        }

                        sId = "";
                        lError = 0;
                        sSql = Strings.Trim(oImportLoad.SQLQuery);
                        if (bTrackingOnly)
                        {
                            if (oTrackObject is not null)
                            {
                                sId = oTrackObject.Id;
                                sTableName = oTrackObject.oTable.TableName;
                            }
                        }
                        else
                        {
                            lIndex = Strings.InStr(1, sSql, "%ID%", Constants.vbTextCompare);

                            if (lIndex == 0)
                            {
                                lIndex = Strings.InStr(1, sSql, "%VALUE%", Constants.vbTextCompare);
                            }

                            if (lIndex > 0)
                            {
                                sId = Convert.ToString(dtRecords.Rows[0][DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)]);
                                sTableName = oTable.TableName;
                            }
                        }

                        if (Strings.Len(sId) > 0)
                        {
                            lIndex = Strings.InStr(1, sSql, "%ID%", Constants.vbTextCompare);

                            if (lIndex > 0)
                            {
                                sSql = Strings.Mid(sSql, 1, (int)(lIndex - 1L)) + sId + Strings.Mid(sSql, (int)(lIndex + 4L));
                            }
                            else
                            {
                                lIndex = Strings.InStr(1, sSql, "%VALUE%", Constants.vbTextCompare);

                                if (lIndex > 0)
                                {
                                    sSql = Strings.Mid(sSql, 1, lIndex - 1) + sId + Strings.Mid(sSql, lIndex + 7);
                                }
                            }
                            Table sTable = default;
                            IDBManager _IDBManagerForsTable = new ImportDBManager();
                            if (!string.IsNullOrEmpty(sTableName))
                            {
                                sTable = data_iTables.Where(m => m.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())).FirstOrDefault();
                                if (sTable is not null)
                                {
                                    if (!string.IsNullOrEmpty(sTable.DBName))
                                    {
                                        var sDatabase = data_iDatabas.Where(m => m.DBName.Trim().ToLower().Equals(sTable.DBName.Trim().ToLower())).FirstOrDefault();
                                        _IDBManagerForsTable.ConnectionString = Keys.get_GetDBConnectionString(sDatabase);
                                        _IDBManagerForsTable.Open();
                                    }
                                }
                            }
                            if (lIndex > 0)
                            {
                                if (!string.IsNullOrEmpty(sTable.DBName))
                                {
                                    lIndex = Convert.ToInt32(GetInfoUsingADONET.ProcessADONETRecord(sSql, _IDBManagerForsTable, ref sReturnMessage, ref lError));
                                    _IDBManagerForsTable.Dispose();
                                }
                                else
                                {
                                    lIndex = Convert.ToInt32(GetInfoUsingADONET.ProcessADONETRecord(sSql, _IDBManagerDefault, ref sReturnMessage, ref lError));
                                }
                                if (!string.IsNullOrEmpty(sReturnMessage))
                                {
                                    errorIndex = 0;
                                    errorFlag = false;
                                    WriteErrorFile(oImportLoad, sReturnMessage, errorFlag, ref recordErrorTrack, default, mrsImport);
                                }

                                if (Convert.ToBoolean(lIndex & Convert.ToInt16(lError == 0L)))
                                {
                                    mlSQLHits = (int)(mlSQLHits + 1L);
                                }
                            }
                        }
                    }
                }

                ImportLoadRecordForQuietProcessingRet = true;
                TrackingServices.TelxonModeOn = bHoldTelxonMode;
                TrackingServices.ScanDateTime = dHoldTelxonDate;
                TrackingServices.TelxonUserName = sHoldTelxonUser;
                return ImportLoadRecordForQuietProcessingRet;
            }
            catch (Exception ex)
            {
                int errNum = 3;
                if (errorIndex == errNum)
                {
                    errorFlag = true;
                }
                else
                {
                    errorFlag = false;
                }
                errorIndex = errNum;
                if ((bool)oImportLoad.ReverseOrder)
                {
                    abortRecordIndex = abortRecordIndex - 1;
                }
                else
                {
                    abortRecordIndex = abortRecordIndex + 1;
                }
                WriteErrorFile(oImportLoad, ex.Message, errorFlag, ref recordErrorTrack, ex, mrsImport);
                TrackingServices.TelxonModeOn = bHoldTelxonMode;
                TrackingServices.ScanDateTime = dHoldTelxonDate;
                TrackingServices.TelxonUserName = sHoldTelxonUser;
                sReturnMessage = ex.Message;
                ImportLoadRecordForQuietProcessingRet = true;
            }

            return ImportLoadRecordForQuietProcessingRet;
        }

        public JsonResult ValidateLoadOnEdit(string LoadName, string msHoldName, string TrackDestinationId)
        {
            IRepository<Table> _iTables = new Repositories<Table>();
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            IRepository<ScanList> _iScanList = new Repositories<ScanList>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<TabSet> _iTabSet = new Repositories<TabSet>();
            IRepository<TableTab> _iTableTab = new Repositories<TableTab>();
            IRepository<RelationShip> _iRelationship = new Repositories<RelationShip>();
            IRepository<View> _iView = new Repositories<View>();

            string validTrackingJSON = "";
            string importLoadJSON = "";
            do
            {
                try
                {
                    Keys.ErrorType = "r";
                    if (!string.IsNullOrEmpty(LoadName))
                    {
                        if (!(msHoldName ?? "").Trim().ToLower().Equals(LoadName.Trim().ToLower()))
                        {
                            var allImportLoad = _iImportLoad.All().OrderBy(m => m.ID);
                            var IsExist = allImportLoad.Any(m => m.LoadName.Trim().ToLower().Equals(LoadName.Trim().ToLower()));
                            if (IsExist)
                            {
                                Keys.ErrorType = "w";
                                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlLoadAlreadyExistsVal"), LoadName);
                                break;
                            }
                        }
                    }

                    ImportLoad oImportLoad;
                    bool validTracking = false;
                    string LookupString = "";
                    bool bSecurityCheck = default;
                    string sTabsetIds = "";
                    Table oCurrentTable = null;
                    if (TempData.ContainsKey("ImportLoad"))
                    {
                        //oImportLoad = (ImportLoad)TempData.Peek("ImportLoad");
                        oImportLoad = TempData.GetPeek<ImportLoad>("ImportLoad");
                    }
                    else
                    {
                        oImportLoad = _iImportLoad.Where(m => m.LoadName.Trim().ToLower().Equals(LoadName.Trim().ToLower())).FirstOrDefault();
                    }

                    if (!string.IsNullOrEmpty(TrackDestinationId))
                    {
                        var FoundBarCodeObj = TrackingServices.BarCodeLookup(default, _iDatabas.All(), _iTables.All(),
                                                                            _iSystem.All(), _iScanList.All(), _iView.All(),
                                                                            _iTableTab.All(), _iTabSet.All(), _iRelationship.All(),
                                                                            LookupString, ref sTabsetIds, ref oCurrentTable, ref bSecurityCheck);
                        if (FoundBarCodeObj is not null)
                        {
                            if (FoundBarCodeObj.oTable.TrackingTable > 0)
                            {
                                FoundBarCodeObj = FoundBarCodeObj;
                            }
                            else
                            {
                                FoundBarCodeObj = default;
                            }
                        }
                        if (FoundBarCodeObj is null)
                        {
                            validTracking = true;
                            // Keys.ErrorType = "r"
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgImportCtrlTrackingDestination"), TrackDestinationId);
                        }
                    }

                    var Setting = new JsonSerializerSettings();
                    Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    validTrackingJSON = JsonConvert.SerializeObject(validTracking, Formatting.Indented, Setting);
                    importLoadJSON = JsonConvert.SerializeObject(oImportLoad, Formatting.Indented, Setting);
                }
                catch (Exception ex)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = ex.Message;
                }
            }
            while (false);
            return Json(new
            {
                validTrackingJSON,
                importLoadJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public async Task<FileContentResult> ShowLogFiles(int LogType, string LoadName)
        {
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            string pathNameJSON = "";
            try
            {
                ImportLoad oImportLoad = default;
                string ServerPath = string.Empty;
                string path = string.Empty;
                if (!string.IsNullOrEmpty(LoadName))
                {
                    oImportLoad = _iImportLoad.Where(m => m.LoadName.Trim().ToLower().Equals(LoadName.Trim().ToLower())).FirstOrDefault();
                }
                if (oImportLoad is not null)
                {
                    // ServerPath = System.IO.Path.GetDirectoryName(oImportLoad.InputFile)
                    if (!(httpContext.Session.GetString("ImportFilePath") == null))
                    {
                        ServerPath = Path.GetDirectoryName(httpContext.Session.GetString("ImportFilePath"));
                    }
                    if (LogType == 1)
                    {
                        path = Path.Combine(ServerPath, "ReportLogFile.txt");
                    }
                    else
                    {
                        path = Path.Combine(ServerPath, "ErrorLogFile.txt");
                    }
                }

                if (LogType == 1)
                {
                    if (!System.IO.File.Exists(path))
                    {
                        var fs = System.IO.File.Create(path);
                        fs.Close();
                    }
                }
                else if (!System.IO.File.Exists(path))
                {
                    var fs = System.IO.File.Create(path);
                    fs.Close();
                }

                byte[] data = await System.IO.File.ReadAllBytesAsync(path);
                return new FileContentResult(data, "text/plain");
                //return File(path, "text/plain");
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        public void WriteErrorFile(ImportLoad oImportLoad, string sReturnMessage, bool errorFlag, ref int recordErrorTrack, Exception Ex = null, ADODB.Recordset msImport = default)
        {
            try
            {
                string path = string.Empty;
                // Dim errorTableName = oImportLoad.FileName
                // Dim ServerPath = System.IO.Path.GetDirectoryName(oImportLoad.InputFile)
                string errorTableName = string.Empty;
                string ServerPath = string.Empty;
                if (!(httpContext.Session.GetString("ImportFilePath") == null))
                {
                    errorTableName = oImportLoad.FileName;
                    ServerPath = Path.GetDirectoryName(httpContext.Session.GetString("ImportFilePath"));
                }
                path = Path.Combine(ServerPath, "ErrorLogFile.txt");

                if (!System.IO.File.Exists(path))
                {
                    var fs = System.IO.File.Create(path);
                    fs.Close();
                    using (var writer = System.IO.File.AppendText(path))
                    {
                        writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlTABFusionRMSImportErrorRecord"), "                      TAB FusionRMS"));
                        writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlStartTime"), "                      ", "             ", Keys.get_ConvertCultureDate(TrackingServices.StartTime, httpContext, true)));
                        writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlEndTime"), "                        ", "             ", Keys.get_ConvertCultureDate(DateTime.Now, httpContext, true)));
                        writer.WriteLine("===========================================================================");
                        writer.WriteLine("===========================================================================");
                        if (!string.IsNullOrEmpty(errorTableName))
                        {
                            writer.WriteLine(string.Format(Languages.get_Translation("UserTableName"), errorTableName));
                        }
                    }
                }
                using (var writer = System.IO.File.AppendText(path))
                {
                    try
                    {
                        recordErrorTrack = recordErrorTrack + 1;
                        if (errorFlag == false)
                        {
                            writer.WriteLine();
                            writer.WriteLine();
                            writer.WriteLine(Languages.get_Translation("msgImportCtrlError"), sReturnMessage);
                        }
                        if (msImport is not null)
                        {
                            string recordString = "";
                            for (int iCount = 0, loopTo = msImport.Fields.Count - 1; iCount <= loopTo; iCount++)
                                recordString = recordString + "'" + msImport.Fields[iCount].Value + "',";
                            recordString = recordString.TrimEnd(',');
                            writer.WriteLine();
                            writer.Write(recordString);
                        }
                    }
                    catch (Exception ex1)
                    {
                        int argrecordErrorTrack = 0;
                        WriteErrorFile(oImportLoad, ex1.Message, true, ref argrecordErrorTrack, ex1);
                    }
                }
            }
            catch (Exception ex2)
            {
                int argrecordErrorTrack = 0;
                WriteErrorFile(oImportLoad, ex2.Message, true, ref argrecordErrorTrack, ex2);
                throw ex2.InnerException;
            }
        }

        public void WriteReportFile(ImportLoad oImportLoad, string tableUserName, int mlRecordsRead, int mlRecordsAdded, int mlRecordsChanged, int mlSQLHits, int recordErrorTrack)
        {
            try
            {
                // Dim ServerPath = System.IO.Path.GetDirectoryName(oImportLoad.InputFile)
                string path = "";
                // Dim oImportLoadFileName = System.IO.Path.GetFileName(oImportLoad.InputFile)

                string ServerPath = string.Empty;
                string oImportLoadFileName = string.Empty;

                if (!(httpContext.Session.GetString("ImportFilePath") == null))
                {
                    ServerPath = Path.GetDirectoryName(httpContext.Session.GetString("ImportFilePath"));
                    oImportLoadFileName = Path.GetFileName(httpContext.Session.GetString("ImportFilePath"));
                }

                path = Path.Combine(ServerPath, "ReportLogFile.txt");
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                var fs = System.IO.File.Create(path);
                fs.Close();

                using (var writer = System.IO.File.AppendText(path))
                {
                    writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlTABFusionRMSImportStatusReport"), "                      TAB FusionRMS"));
                    writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlStartTime"), "                      ", "             ", Keys.get_ConvertCultureDate(TrackingServices.StartTime, httpContext, true)));
                    writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlEndTime"), "                        ", "             ", Keys.get_ConvertCultureDate(DateTime.Now, httpContext, true)));
                    writer.WriteLine("===========================================================================");
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlLoadNameWithSpace"), "                       ", "             ", oImportLoad.LoadName));
                    writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlFileImported"), "                   ", "             ", oImportLoadFileName));
                    writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlDatabase"), "                        ", "             ", oImportLoad.DatabaseName));
                    writer.WriteLine(string.Format(Languages.get_Translation("ImportTableWithSpace"), "                           ", "            ", tableUserName));
                    writer.WriteLine();
                    writer.WriteLine();
                    writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlRecordsRead"), "                    ", "             ", mlRecordsRead));
                    writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlRecordsAdded"), "                   ", "             ", mlRecordsAdded));
                    writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlRecordsChanged"), "                 ", "             ", mlRecordsChanged));
                    writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlRecordsError"), "                   ", "             ", recordErrorTrack));
                    writer.WriteLine();
                    writer.WriteLine(string.Format(Languages.get_Translation("msgImportCtrlSQLHits"), "                        ", "             ", mlSQLHits));
                }
            }

            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        private async void ImageCopyForQuietProcessing(bool bImageCopy, Table oTables, OutputSetting oOutputSettings, string sImageName, string sTableId, string sOCRText, string sReturnError, ImportLoad moImportLoads, IDBManager commonIDBManager, bool IsIdFieldString)
        {
            var call = new ApiCalls(_configuration);
            bool bBadFile = true;
            string sErrorMessage = string.Empty;
            long lAttachmentNumber;
            long lVersionNumber;
            Enums.geSaveAction eSaveAction;
            // Dim oTrackable As Trackable
            if (!bBadFile & bImageCopy)
            {
                sReturnError = string.Format(Languages.get_Translation("msgImportCtrlPoints2PCFileNotImg"), IMAGE_COPY);
                return;
            }
            else if (!bBadFile & !bImageCopy)
            {
                sReturnError = string.Format(Languages.get_Translation("msgImportCtrlPoints2ImgNotPCFile"), PCFILE_COPY);
                return;
            }
            if ((bool)moImportLoads.SaveImageAsNewPage)
            {
                eSaveAction = Enums.geSaveAction.saNewPage;
            }
            else if ((bool)(moImportLoads.SaveImageAsNewVersion | moImportLoads.SaveImageAsNewVersionAsOfficialRecord))
            {
                eSaveAction = Enums.geSaveAction.saNewVersion;
            }
            else
            {
                eSaveAction = Enums.geSaveAction.saNewAttachment;
            }

            if (bImageCopy)
            {
                long arglTrackableId = -1L;
                long arglTrackableVersion = -1L;
                long arglTrackablePageCount = -1L;
                lAttachmentNumber = FindExistingAttachmentForQuietProcessing(ref sErrorMessage, ref eSaveAction, Enums.geTrackableType.tkImage, sImageName, oTables, sTableId, commonIDBManager, IsIdFieldString, lTrackableId: ref arglTrackableId, lTrackableVersion: ref arglTrackableVersion, lTrackablePageCount: ref arglTrackablePageCount);
            }
            else
            {
                long arglTrackableId1 = -1L;
                long arglTrackableVersion1 = -1L;
                long arglTrackablePageCount1 = -1L;
                lAttachmentNumber = FindExistingAttachmentForQuietProcessing(ref sErrorMessage, ref eSaveAction, Enums.geTrackableType.tkWPDoc, sImageName, oTables, sTableId, commonIDBManager, IsIdFieldString, lTrackableId: ref arglTrackableId1, lTrackableVersion: ref arglTrackableVersion1, lTrackablePageCount: ref arglTrackablePageCount1);
            }
            try
            {
                if (Strings.Len(sErrorMessage) > 0L)
                {
                    if (bImageCopy)
                    {
                        sReturnError = string.Format(Languages.get_Translation("msgImportCtrlImage"), sErrorMessage);
                    }
                    else
                    {
                        sReturnError = string.Format(Languages.get_Translation("msgImportCtrlPCFile"), sErrorMessage);
                    }
                }
                else
                {
                    var ticket = passport.get_CreateTicket(String.Format("{0}\\{1}",
                                                       httpContext.Session.GetString("Server"),
                                                       httpContext.Session.GetString("databaseName")),
                                                        oTables.TableName,
                                                        sTableId);
                    Smead.RecordsManagement.Imaging.Attachment attach = default;
                    // Dim IsException As Boolean = False
                    var info = await call.DocumentServices.GetCodecInfoFromFile(sImageName, Path.GetExtension(sImageName));//Common.GetCodecInfoFromFile(sImageName, Path.GetExtension(sImageName));
                    switch (eSaveAction)
                    {
                        case var @case when @case == Enums.geSaveAction.saNewAttachment:
                            {
                                if (info is null)
                                {
                                    attach = Smead.RecordsManagement.Imaging.Attachments.AddAttachmentForImport(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}",
                                        httpContext.Session.GetString("Server"),
                                        httpContext.Session.GetString("databaseName")), oTables.TableName, sTableId, 0, oOutputSettings.Id, sImageName, sImageName, Path.GetExtension(sImageName), false, "", false, 1, 0, 0, 0);
                                }
                                else
                                {
                                    attach = Smead.RecordsManagement.Imaging.Attachments.AddAttachmentForImport(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}",
                                        httpContext.Session.GetString("Server"),
                                        httpContext.Session.GetString("databaseName")), oTables.TableName, sTableId, 0, oOutputSettings.Id, sImageName, sImageName, Path.GetExtension(sImageName), false, "", true, info.TotalPages, info.Height, info.Width, info.SizeDisk);
                                }

                                break;
                            }
                        case var case1 when case1 == Enums.geSaveAction.saNewVersion:
                            {
                                if (info is null)
                                {
                                    attach = Smead.RecordsManagement.Imaging.Attachments.AddVersionForImport(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}",
                                        httpContext.Session.GetString("Server"),
                                        httpContext.Session.GetString("databaseName")), oTables.TableName, sTableId, (int)lAttachmentNumber, 0, oOutputSettings.Id, sImageName, sImageName, moImportLoads.SaveImageAsNewVersionAsOfficialRecord ?? false, Path.GetExtension(sImageName), false, false, 1, 0, 0, 0);
                                }
                                else
                                {
                                    attach = Smead.RecordsManagement.Imaging.Attachments.AddVersionForImport(Keys.GetClientIpAddress(httpContext),
                                        ticket, passport.UserId, string.Format(@"{0}\{1}",
                                        httpContext.Session.GetString("Server"), httpContext.Session.GetString("databaseName")),
                                        oTables.TableName, sTableId, (int)lAttachmentNumber, 0,
                                        oOutputSettings.Id, sImageName, sImageName,
                                        moImportLoads.SaveImageAsNewVersionAsOfficialRecord ?? false,
                                        Path.GetExtension(sImageName), false, true, info.TotalPages,
                                        info.Height, info.Width, info.SizeDisk);
                                }

                                break;
                            }

                        case var case2 when case2 == Enums.geSaveAction.saNewPage:
                            {
                                lVersionNumber = Smead.RecordsManagement.Imaging.Attachments.GetVersionCount(ticket, passport.UserId, string.Format(@"{0}\{1}",
                                    httpContext.Session.GetString("Server"),
                                    httpContext.Session.GetString("databaseName")), oTables.TableName, sTableId, (int)lAttachmentNumber);
                                if (info is null)
                                {
                                    var returnMessagePage = Smead.RecordsManagement.Imaging.Attachments.AddPageForImport(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}",
                                        httpContext.Session.GetString("Server"),
                                        httpContext.Session.GetString("databaseName")), oTables.TableName, sTableId, (int)lAttachmentNumber, (int)lVersionNumber, oOutputSettings.Id, sImageName, sImageName, Path.GetExtension(sImageName), false, true, false, true, false, 1, 0, 0, 0);
                                }
                                else
                                {
                                    var returnMessagePage = Smead.RecordsManagement.Imaging.Attachments.AddPageForImport(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}",
                                        httpContext.Session.GetString("Server"),
                                        httpContext.Session.GetString("databaseName")), oTables.TableName, sTableId, (int)lAttachmentNumber, (int)lVersionNumber, oOutputSettings.Id, sImageName, sImageName, Path.GetExtension(sImageName), false, true, false, true, true, info.TotalPages, info.Height, info.Width, info.SizeDisk);
                                }

                                break;
                            }

                    }
                    // If (IsException) Then
                    // sReturnError = String.Format(Languages.Translation("msgImportCtrlProblemUsingDefaultOutputImageCopy"), Path.GetFileName(sImageName), vbNewLine)
                    // End If
                }
            }
            catch (Exception)
            {
                sReturnError = string.Format(Languages.get_Translation("msgImportCtrlProblemUsingDefaultOutputImageCopy"), Path.GetFileName(sImageName), Constants.vbNewLine);
            }

            // If (moImportLoads.DeleteSourceImage) Then
            // Kill(sImageName)
            // End If
            // oTrackable = Nothing
            return;
        }

        public long FindExistingAttachmentForQuietProcessing(ref string sErrorMsg, ref Enums.geSaveAction eSaveAction, Enums.geTrackableType eTrackableType, string sImageName, Table sTable, string sTableId, IDBManager commonIDBManager, bool IsIdFieldString, [Optional, DefaultParameterValue(-1L)] ref long lTrackableId, [Optional, DefaultParameterValue(-1L)] ref long lTrackableVersion, [Optional, DefaultParameterValue(-1L)] ref long lTrackablePageCount)
        {
            IRepository<Userlink> _iUserLink = new Repositories<Userlink>();
            IRepository<Trackable> _iTrackables = new Repositories<Trackable>();
            IRepository<PCFilesPointer> _iPCFilePointer = new Repositories<PCFilesPointer>();
            long FindExistingAttachmentForQuietProcessingRet = default;
            var bFoundOne = default(bool);
            PCFilesPointer oPCFilePointer;
            Trackable oTrackable;
            Userlink oUserLink;
            string sExtension;
            FindExistingAttachmentForQuietProcessingRet = 0L;
            if (eSaveAction == Enums.geSaveAction.saNewAttachment)
                return FindExistingAttachmentForQuietProcessingRet;
            var tempIndex = GetInfoUsingADONET.ZeroPaddedString(sTable, sTableId, commonIDBManager, IsIdFieldString);
            IQueryable<Userlink> moUserLinks = _iUserLink.Where(m => m.IndexTable.Trim().ToLower().Equals(sTable.TableName.Trim().ToLower()) & Operators.ConditionalCompareObjectEqual(m.IndexTableId, tempIndex, false)).OrderBy(m => m.AttachmentNumber);
            foreach (var oUserLinks in moUserLinks)
            {
                IQueryable<Trackable> moTrackable = _iTrackables.Where(m => Operators.ConditionalCompareObjectEqual(m.Id, oUserLinks.TrackablesId, false) & Operators.ConditionalCompareObjectEqual(m.RecordTypesId, eTrackableType, false)).OrderByDescending(m => m.RecordVersion);

                foreach (var oTrackables in moTrackable)
                {
                    bool exitFor = false;
                    bool exitFor1 = false;
                    switch (eTrackableType)
                    {
                        case var @case when @case == Enums.geTrackableType.tkImage:
                            {
                                var dDateTime = new DateTime();
                                if ((IsCheckedOut(oTrackables, ref dDateTime).Length) > 0)
                                {
                                    sErrorMsg = string.Format(Languages.get_Translation("msgImportCtrlIsCheckedOut"), oUserLinks.AttachmentNumber);
                                }
                                else
                                {
                                    sErrorMsg = "";
                                    bFoundOne = true;
                                    FindExistingAttachmentForQuietProcessingRet = (long)oUserLinks.AttachmentNumber;
                                    lTrackableId = (long)oUserLinks.TrackablesId;
                                    lTrackableVersion = oTrackables.RecordVersion;
                                    lTrackablePageCount = (long)oTrackables.PageCount;
                                    oTrackable = default;
                                    exitFor = true;
                                    break;
                                }

                                break;
                            }
                        case var case1 when case1 == Enums.geTrackableType.tkWPDoc:
                            {
                                IQueryable<PCFilesPointer> moPcFilePointer = _iPCFilePointer.Where(m => Operators.ConditionalCompareObjectEqual(m.TrackablesId, oTrackables.Id, false) & Operators.ConditionalCompareObjectEqual(m.TrackablesRecordVersion, oTrackables.RecordVersion, false)).OrderBy(m => m.PageNumber).ThenBy(m => m.Id);
                                sExtension = Path.GetExtension(sImageName);

                                foreach (var oPCFilePointers in moPcFilePointer)
                                {
                                    FindExistingAttachmentForQuietProcessingRet = (long)oUserLinks.AttachmentNumber;
                                    lTrackableId = (long)oUserLinks.TrackablesId;
                                    lTrackableVersion = (long)oTrackables.RecordVersion;
                                    lTrackablePageCount = (long)oTrackables.PageCount;

                                    if (Strings.StrComp(sExtension, Path.GetExtension(oPCFilePointers.FileName), Constants.vbTextCompare) == 0)
                                    {
                                        var dDateTime = new DateTime();
                                        if ((IsCheckedOut(oTrackables, ref dDateTime).Length) > 0)
                                        {
                                            sErrorMsg = string.Format(Languages.get_Translation("msgImportCtrlIsCheckedOut"), oUserLinks.AttachmentNumber);
                                        }
                                        else
                                        {
                                            sErrorMsg = "";
                                            bFoundOne = true;
                                            if (eSaveAction == Enums.geSaveAction.saNewPage)
                                                eSaveAction = Enums.geSaveAction.saNewVersion;
                                            oPCFilePointer = default;
                                            break;
                                        }
                                    }
                                }

                                if (bFoundOne)
                                {
                                    oTrackable = default;
                                    exitFor1 = true;
                                    break;
                                }

                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }

                    if (exitFor)
                    {
                        break;
                    }

                    if (exitFor1)
                    {
                        break;
                    }
                }

                if (bFoundOne)
                {
                    oUserLink = default;
                    break;
                }
            }

            if (!bFoundOne)
                eSaveAction = Enums.geSaveAction.saNewAttachment;
            return FindExistingAttachmentForQuietProcessingRet;

        }

        internal string IsCheckedOut(Trackable oTrackable, ref DateTime dDateTime)
        {
            IRepository<Trackable> _iTrackables = new Repositories<Trackable>();
            string sReturn = string.Empty;

            if (oTrackable is not null && oTrackable.Id != 0)
            {
                var moTrackable = _iTrackables.Where(m => Operators.ConditionalCompareObjectEqual(m.Id, oTrackable.Id, false) & Operators.ConditionalCompareObjectEqual(m.RecordVersion, oTrackable.RecordVersion, false));
                if (moTrackable is not null)
                {
                    foreach (Trackable oTmpTrackable in moTrackable)
                    {
                        if ((bool)oTmpTrackable.CheckedOut)
                        {
                            dDateTime = (DateTime)oTmpTrackable.CheckedOutDate;
                            sReturn = oTmpTrackable.CheckedOutUser;
                            break;
                        }
                    }
                }
            }

            return sReturn;
        }

        private object ConvertDateTime(object vDateTime, string sDateFormat, object pDefaultValue, object vSwingYear, ref string sReturnMessage, ref int errorIndex, ref bool errorFlag, bool UseDefaultValue = false)
        {
            try
            {
                int errNum = 0;
                string[] DateArray = null;
                string[] TimeArray = null;
                string vDateTimeStr = null;
                string localDateFormat = null;
                if (string.IsNullOrEmpty(Strings.Trim(sDateFormat)))
                    sDateFormat = "mm/dd/yyyy";

                if (string.IsNullOrEmpty(Convert.ToString(vDateTime)))
                {
                    vDateTime = "";
                }
                else
                {
                    vDateTime = Strings.Replace(Convert.ToString(vDateTime), @"\", "/");
                    vDateTime = Strings.Replace(Convert.ToString(vDateTime), "-", "/");
                    vDateTime = Strings.Replace(Convert.ToString(vDateTime), " ", "/");
                    vDateTime = Strings.Replace(Convert.ToString(vDateTime), ".", ":");
                }

                if (string.IsNullOrEmpty(Strings.Trim(Convert.ToString(vDateTime))))
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(pDefaultValue)))
                    {
                        pDefaultValue = pDefaultValue.ToString().ToUpper();
                        if (Strings.StrComp(Convert.ToString(pDefaultValue), "TODAY", Constants.vbTextCompare) == 0L | Strings.StrComp(Convert.ToString(pDefaultValue), "@@TODAY", Constants.vbTextCompare) == 0L)
                        {
                            UseDefaultValue = true;
                        }
                        else if (Strings.StrComp(Convert.ToString(pDefaultValue), "@@TIME", Constants.vbTextCompare) == 0L)
                        {
                        }

                        else if (Strings.StrComp(Convert.ToString(pDefaultValue), "@@NOW", Constants.vbTextCompare) == 0L)
                        {
                            UseDefaultValue = true;
                        }
                        else
                        {
                            if (pDefaultValue == null)
                                pDefaultValue = "";
                            if (string.IsNullOrEmpty(Strings.Trim(Convert.ToString(pDefaultValue))))
                            {
                                vDateTime = "";
                            }
                            else
                            {
                                vDateTime = pDefaultValue;
                            }
                        }
                    }
                }
                else if (Strings.StrComp(Convert.ToString(vDateTime), "Today", Constants.vbTextCompare) == 0L)
                {
                    vDateTime = DateTime.Now.ToShortDateString();
                    UseDefaultValue = true;
                }

                // Added by Nikunj - FUS-4357 - Fix:2 - If there are any empty fields in the Excel file - Consider those as empty string on the record 
                if (string.IsNullOrEmpty(Strings.Trim(Convert.ToString(vDateTime))) && UseDefaultValue == false)
                    return vDateTime;
                if (UseDefaultValue == true)
                {
                    var oNow = DateTime.Now;
                    if (sDateFormat.Contains("hh") & pDefaultValue.Equals("@@NOW"))
                    {
                        sDateFormat = "mm/dd/yyyy hh:mm:ss";
                        TimeArray = new string[3];
                        TimeArray[0] = oNow.Hour.ToString();
                        TimeArray[1] = oNow.Minute.ToString();
                        TimeArray[2] = oNow.Second.ToString();
                    }
                    else
                    {
                        sDateFormat = "mm/dd/yyyy";
                    }

                    DateArray = new string[3];
                    DateArray[0] = oNow.Month.ToString();
                    DateArray[1] = oNow.Day.ToString();
                    DateArray[2] = oNow.Year.ToString();
                }
                else
                {
                    vDateTimeStr = vDateTime.ToString();
                    sDateFormat = sDateFormat.Trim().ToLower();
                    if (Convert.ToBoolean(Strings.InStr(sDateFormat, "/")))
                    {
                        if (Convert.ToBoolean(Strings.InStr(vDateTimeStr, "/")))
                        {
                            DateArray = vDateTimeStr.Split("/");
                            if (Convert.ToBoolean(Convert.ToInt16(sDateFormat.Contains("hh:mm:ss")) & Strings.InStr(vDateTimeStr, ":") & Convert.ToInt16(DateArray.Length == 4) | Convert.ToInt16(DateArray.Length == 5)))
                            {
                                TimeArray = DateArray[3].Split(":");
                            }
                        }
                        else
                        {
                            errNum = 22;
                            goto lbl_ConvertDateTime;
                        }
                    }
                }

                switch (sDateFormat ?? "")
                {
                    case "dd/mm/yyyy":
                    case "dd/mm/yy":
                    case "dd/mm/yyyy hh:mm:ss":
                    case "dd/mm/yy hh:mm:ss":
                    case "mm/dd/yyyy":
                    case "mm/dd/yy":
                    case "mm/dd/yyyy hh:mm:ss":
                    case "mm/dd/yy hh:mm:ss":
                        {
                            if (DateArray.Length >= 3)
                            {
                                if (sDateFormat.Contains("yy")) // sDateFormat.Contains("yyyy") Fixed: FUS-4357
                                {
                                    if (DateArray[2].Length != 4)
                                    {
                                        errNum = 20;
                                        goto lbl_ConvertDateTime;
                                        // Exit Select
                                    }
                                }
                                else if (DateArray[2].Length != 2)
                                {
                                    errNum = 20;
                                    goto lbl_ConvertDateTime;
                                }
                                if (sDateFormat.Contains("dd/mm/yy"))
                                {
                                    if (Convert.ToDouble(DateArray[1]) > 12d & Convert.ToDouble(DateArray[0]) <= 12d)
                                    {
                                        errNum = 20;
                                        goto lbl_ConvertDateTime;
                                    }
                                }
                                else if (sDateFormat.Contains("mm/dd/yy"))
                                {
                                    if (Convert.ToDouble(DateArray[0]) > 12d & Convert.ToDouble(DateArray[1]) <= 12d)
                                    {
                                        errNum = 20;
                                        goto lbl_ConvertDateTime;
                                    }
                                }
                                if (DateArray[2].Length == 2)
                                {
                                    DateArray[2] = AddSwingYear(DateArray[2], vSwingYear);
                                }
                                if (sDateFormat.Contains("dd/mm/yy"))
                                {
                                    vDateTime = DateArray[1] + "/" + DateArray[0] + "/" + DateArray[2];
                                }
                                else if (sDateFormat.Contains("mm/dd/yy"))
                                {
                                    vDateTime = DateArray[0] + "/" + DateArray[1] + "/" + DateArray[2];
                                }
                            }
                            else
                            {
                                errNum = 22;
                                goto lbl_ConvertDateTime;
                            }
                            break;
                        }
                    case "yyyy/mm/dd":
                    case "yyyy/mm/dd hh:mm:ss":
                    case "yy/mm/dd":
                    case "yy/mm/dd hh:mm:ss":
                    case "yyyy/dd/mm":
                    case "yyyy/dd/mm hh:mm:ss":
                    case "yy/dd/mm":
                    case "yy/dd/mm hh:mm:ss":
                        {
                            if (DateArray.Length >= 3)
                            {
                                if (sDateFormat.Contains("yyyy"))
                                {
                                    if (DateArray[0].Length != 4)
                                    {
                                        errNum = 20;
                                        goto lbl_ConvertDateTime;
                                    }
                                }
                                else if (DateArray[0].Length != 2)
                                {
                                    errNum = 20;
                                    goto lbl_ConvertDateTime;
                                }
                                if (sDateFormat.Contains("yy/mm/dd"))
                                {
                                    if (Convert.ToDouble(DateArray[1]) > 12d & Convert.ToDouble(DateArray[2]) <= 12d)
                                    {
                                        errNum = 20;
                                        goto lbl_ConvertDateTime;
                                    }
                                }
                                else if (sDateFormat.Contains("yy/dd/mm"))
                                {
                                    if (Convert.ToDouble(DateArray[2]) > 12d & Convert.ToDouble(DateArray[1]) <= 12d)
                                    {
                                        errNum = 20;
                                        goto lbl_ConvertDateTime;
                                    }
                                }
                                if (DateArray[0].Length == 2)
                                {
                                    DateArray[0] = AddSwingYear(DateArray[0], vSwingYear);
                                }
                                if (sDateFormat.Contains("yyyy/mm/dd") | sDateFormat.Contains("yy/mm/dd"))
                                {
                                    vDateTime = DateArray[1] + "/" + DateArray[2] + "/" + DateArray[0];
                                }
                                else if (sDateFormat.Contains("yyyy/dd/mm") | sDateFormat.Contains("yy/dd/mm"))
                                {
                                    vDateTime = DateArray[2] + "/" + DateArray[1] + "/" + DateArray[0] + " ";
                                }
                            }
                            else
                            {
                                errNum = 22;
                                goto lbl_ConvertDateTime;
                            }
                            break;
                        }
                    case "mmddyy":
                    case "mmddyyhhmmss":
                    case "ddmmyy":
                    case "ddmmyyhhmmss":
                    case "yyddmm":
                    case "yyddmmhhmmss":
                    case "yymmdd":
                    case "yymmddhhmmss":
                        {
                            if (vDateTimeStr.Length == 6 | vDateTimeStr.Length == 12)
                            {
                                if (sDateFormat.Contains("mmddyy"))
                                {
                                    vDateTime = Strings.Left(vDateTimeStr, 2) + "/" + Strings.Mid(vDateTimeStr, 3, 2) + "/" + AddSwingYear(Strings.Mid(vDateTimeStr, 5, 2), vSwingYear);
                                }
                                else if (sDateFormat.Contains("ddmmyy"))
                                {
                                    vDateTime = Strings.Mid(vDateTimeStr, 3, 2) + "/" + Strings.Left(vDateTimeStr, 2) + "/" + AddSwingYear(Strings.Mid(vDateTimeStr, 5, 2), vSwingYear);
                                }
                                else if (sDateFormat.Contains("yyddmm"))
                                {
                                    vDateTime = Strings.Mid(vDateTimeStr, 5, 2) + "/" + Strings.Mid(vDateTimeStr, 3, 2) + "/" + AddSwingYear(Strings.Left(vDateTimeStr, 2), vSwingYear);
                                }
                                else
                                {
                                    vDateTime = Strings.Mid(vDateTimeStr, 3, 2) + "/" + Strings.Mid(vDateTimeStr, 5, 2) + "/" + AddSwingYear(Strings.Left(vDateTimeStr, 2), vSwingYear);
                                }
                                if (sDateFormat.Contains("hhmmss"))
                                {
                                    if (sDateFormat.Length == 12)
                                    {
                                        if (vDateTimeStr.Length == 12)
                                        {
                                            vDateTime = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(vDateTime, " "), Strings.Mid(vDateTimeStr, 7, 2)), ":"), Strings.Mid(vDateTimeStr, 9, 2)), ":"), Strings.Mid(vDateTimeStr, 11, 2));
                                        }
                                        else if (vDateTimeStr.Length == 6)
                                        {
                                            vDateTime = Operators.ConcatenateObject(Operators.ConcatenateObject(vDateTime, " "), "00:00:00");
                                        }
                                    }
                                    else
                                    {
                                        errNum = 21;
                                        goto lbl_ConvertDateTime;
                                    }
                                }
                            }
                            else
                            {
                                errNum = 21;
                                goto lbl_ConvertDateTime;
                            }
                            break;
                        }
                    case "ddmmyyyy":
                    case "ddmmyyyyhhmmss":
                    case "mmddyyyy":
                    case "mmddyyyyhhmmss":
                        {
                            if (vDateTimeStr.Length == 8 | vDateTimeStr.Length == 14)
                            {
                                if (sDateFormat.Contains("mmddyyyy"))
                                {
                                    vDateTime = Strings.Left(vDateTimeStr, 2) + "/" + Strings.Mid(vDateTimeStr, 3, 2) + "/" + Strings.Mid(vDateTimeStr, 5, 4);
                                }
                                else if (sDateFormat.Contains("ddmmyyyy"))
                                {
                                    vDateTime = Strings.Mid(vDateTimeStr, 3, 2) + "/" + Strings.Left(vDateTimeStr, 2) + "/" + Strings.Mid(vDateTimeStr, 5, 4);
                                }
                                if (sDateFormat.Contains("hhmmss"))
                                {
                                    if (sDateFormat.Length == 14)
                                    {
                                        if (vDateTimeStr.Length == 14)
                                        {
                                            vDateTime = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(vDateTime, " "), Strings.Mid(vDateTimeStr, 9, 2)), ":"), Strings.Mid(vDateTimeStr, 11, 2)), ":"), Strings.Mid(vDateTimeStr, 13, 2));
                                        }
                                        else if (vDateTimeStr.Length == 8)
                                        {
                                            vDateTime = Operators.ConcatenateObject(Operators.ConcatenateObject(vDateTime, " "), "00:00:00");
                                        }
                                    }
                                    else
                                    {
                                        errNum = 21;
                                        goto lbl_ConvertDateTime;
                                    }
                                }
                            }
                            else
                            {
                                errNum = 21;
                                goto lbl_ConvertDateTime;
                            }
                            break;
                        }
                    case "yyyyddmm":
                    case "yyyyddmmhhmmss":
                    case "yyyymmdd":
                    case "yyyymmddhhmmss":
                        {
                            if (vDateTimeStr.Length == 8 | vDateTimeStr.Length == 14)
                            {
                                if (sDateFormat.Contains("yyyyddmm"))
                                {
                                    vDateTime = Strings.Mid(vDateTimeStr, 7, 2) + "/" + Strings.Mid(vDateTimeStr, 5, 2) + "/" + Strings.Left(vDateTimeStr, 4);
                                }
                                else
                                {
                                    vDateTime = Strings.Mid(vDateTimeStr, 5, 2) + "/" + Strings.Mid(vDateTimeStr, 7, 2) + "/" + Strings.Left(vDateTimeStr, 4);
                                }
                                if (sDateFormat.Contains("hhmmss"))
                                {
                                    if (sDateFormat.Length == 14)
                                    {
                                        if (vDateTimeStr.Length == 14)
                                        {
                                            vDateTime = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(vDateTime, " "), Strings.Mid(vDateTimeStr, 9, 2)), ":"), Strings.Mid(vDateTimeStr, 11, 2)), ":"), Strings.Mid(vDateTimeStr, 13, 2));
                                        }
                                        else if (vDateTimeStr.Length == 8)
                                        {
                                            vDateTime = Operators.ConcatenateObject(Operators.ConcatenateObject(vDateTime, " "), "00:00:00");
                                        }
                                    }
                                    else
                                    {
                                        errNum = 21;
                                        goto lbl_ConvertDateTime;
                                    }
                                }
                            }
                            else
                            {
                                errNum = 21;
                                goto lbl_ConvertDateTime;
                            }
                            break;
                        }
                }

            lbl_ConvertDateTime:
                ;

                if (errNum != 0)
                {
                    if (errorIndex == errNum)
                    {
                        errorFlag = true;
                    }
                    else
                    {
                        errorFlag = false;
                    }
                    errorIndex = errNum;
                    if (errNum == 20)
                    {
                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlDateFormatInValidORMissing"), Constants.vbNewLine);
                    }
                    else if (errNum == 21)
                    {
                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlDateFormatLengthNotMatched"), Constants.vbNewLine, Constants.vbTab, Strings.Len(sDateFormat).ToString(), sDateFormat, Strings.Len(vDateTime).ToString(), vDateTime);
                    }
                    else
                    {
                        sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlDateFormatInvalid"), "frmImportManager.ConvertDateTime");
                    }
                }
                else if (sDateFormat.Contains("hh:mm:ss"))
                {
                    if (TimeArray is not null)
                    {
                        string hour = TimeArray[0];
                        if (DateArray.Length > 4 && string.Compare(DateArray[4].Substring(0, 1), "p", true) == 0)
                            hour = (Convert.ToInt16(hour) + 12).ToString();
                        vDateTime = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(vDateTime, " "), hour), ":"), TimeArray[1]), ":"), TimeArray[2]);
                    }
                    else
                    {
                        vDateTime = Operators.ConcatenateObject(Operators.ConcatenateObject(vDateTime, " "), "00:00:00");
                    }
                }
            }
            catch (Exception ex)
            {
                sReturnMessage = ex.Message;
            }
            return vDateTime;
        }

        private bool DateContainsTime(object vDateTime)
        {
            bool DateContainsTimeRet = default;
            long lPos;
            string sDateTime;
            ;
            sDateTime = Convert.ToString(vDateTime);
            if (Strings.Len(sDateTime) == 0L)
            {
                DateContainsTimeRet = false;
                return DateContainsTimeRet;
            }

            lPos = Strings.InStr(sDateTime, " ");
            if (lPos <= 8L)
            {
                DateContainsTimeRet = false;
                return DateContainsTimeRet;
            }

            if (Strings.Len(sDateTime) <= lPos + 1L)
            {
                DateContainsTimeRet = false;
                return DateContainsTimeRet;
            }


            DateContainsTimeRet = Information.IsNumeric(Strings.Mid(sDateTime, (int)(lPos + 1L), 1));
            return DateContainsTimeRet;

        lbl_DateContainsTime:
            ;

            DateContainsTimeRet = true;
        }

        private string AddSwingYear(object vYear, object vSwingYear)
        {
            string AddSwingYearRet = default;
            long lCentury;
            ;

            lCentury = Convert.ToInt64(Strings.Left(Strings.Format(DateTime.Now, "yyyy"), 2));

            if (Strings.Len(Convert.ToString(vYear)) == 2L)
            {
                if (!Information.IsNumeric(vYear))
                {
                    AddSwingYearRet = Convert.ToString(vYear);
                }
                else if (Convert.ToInt64(vYear) <= Convert.ToInt64(vSwingYear))
                {
                    AddSwingYearRet = lCentury.ToString() + Convert.ToString(vYear);
                }
                else
                {
                    AddSwingYearRet = (lCentury - 1L).ToString() + Convert.ToString(vYear);
                }
            }
            else
            {
                AddSwingYearRet = Convert.ToString(vYear);
            }

            return AddSwingYearRet;

        lbl_AddSwingYear:
            ;

            AddSwingYearRet = Convert.ToString(vYear);
        }

        public void SetOverWriteError(string eUpdateType, ref bool bDoAddNew, ref bool bFound, ref string sReturnMessage, ref int recordErrorTrack, ref int errorIndex, ref bool errorflag)
        {
            switch (eUpdateType ?? "")
            {
                case BOTH_DB_TEXT:
                    {
                        if (!bFound)
                        {
                            bFound = true;
                            bDoAddNew = true;
                        }

                        break;
                    }
                case OVERWRITE_DB_TEXT:
                    {
                        if (!bFound)
                        {
                            int errNum = 4;
                            if (errorIndex == errNum)
                            {
                                errorflag = true;
                            }
                            else
                            {
                                errorflag = false;
                            }
                            errorIndex = errNum;
                            sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlNoRecFound"), OVERWRITE_DISPLAY);
                        }

                        break;
                    }
                case NEW_DB_TEXT:
                    {
                        if (!bFound)
                        {
                            bFound = true;
                            bDoAddNew = true;
                        }
                        else
                        {
                            int errNum = 5;
                            if (errorIndex == errNum)
                            {
                                errorflag = true;
                            }
                            else
                            {
                                errorflag = false;
                            }
                            errorIndex = errNum;
                            sReturnMessage = string.Format(Languages.get_Translation("msgImportCtrlRecFound"), NEW_DISPLAY);
                            bFound = false;
                        }

                        break;
                    }
                case NEW_SKIP_DB_TEXT:
                    {
                        if (!bFound)
                        {
                            bFound = true;
                            bDoAddNew = true;
                        }
                        else
                        {
                            // we found an existing record, so skip it.
                            bFound = false;
                        }

                        break;
                    }
            }
        }

        private bool IsFunction(object vDefaultValue)
        {
            bool IsFunctionRet = default;
            IsFunctionRet = false;
            if (!(vDefaultValue == null | string.IsNullOrEmpty(Strings.Trim(Convert.ToString(vDefaultValue)))))
            {
                if (Strings.StrComp("concat::", Strings.Left(Strings.Trim(Convert.ToString(vDefaultValue)), Strings.Len("concat::")), Constants.vbTextCompare) == 0)
                {
                    IsFunctionRet = true;
                }
            }

            return IsFunctionRet;
        }

        private string ProcessFunctionValue(object vDefaultValue)
        {
            string ProcessFunctionValueRet = default;
            long lFuncIndex;
            long lFuncLBound;
            long lFuncUBound;
            string sReturn;
            var mrsImport = new ADODB.Recordset();
            lFuncLBound = -1;
            lFuncUBound = -1;
            sReturn = StripFunction(vDefaultValue);

            if (Strings.Len(Strings.Trim(sReturn)) == 0L)
            {
                ProcessFunctionValueRet = Convert.ToString(vDefaultValue);
                return ProcessFunctionValueRet;
            }

            if (TempData.ContainsKey("ImportDataRS"))
            {
                //mrsImport = (ADODB.Recordset)TempData.Peek("ImportDataRS");
                mrsImport = TempData.GetPeek<ADODB.Recordset>("ImportDataRS");
            }
            var loopTo = mrsImport.Fields.Count;
            for (lFuncIndex = 1L; lFuncIndex <= loopTo; lFuncIndex++)
            {
                var FieldValue = mrsImport.Fields[lFuncIndex - 1L].ToString();
                if (!(FieldValue is DBNull))
                {
                    StripQuotes(ref FieldValue);
                    sReturn = ProcessFunction(sReturn, lFuncIndex, FieldValue);
                }
            }

            sReturn = PostProcessFunction(sReturn);
            ProcessFunctionValueRet = sReturn;
            return ProcessFunctionValueRet;
        }

        private string StripFunction(object vDefaultValue)
        {
            string StripFunctionRet = default;
            long lIndex;
            StripFunctionRet = "";

            if (!(vDefaultValue == null | string.IsNullOrEmpty(Strings.Trim(Convert.ToString(vDefaultValue)))))
            {
                lIndex = Strings.InStr(1, Convert.ToString(vDefaultValue), "::", Constants.vbTextCompare);

                if (lIndex > 0L)
                {
                    StripFunctionRet = Strings.Right(Convert.ToString(vDefaultValue), (int)(Strings.Len(vDefaultValue) - (lIndex + 1L)));
                }
            }

            return StripFunctionRet;
        }

        private string ProcessFunction(string sField, long lFuncIndex, string sReplaceText)
        {
            string ProcessFunctionRet = default;
            string sLookFor;
            string sTemp;

            sTemp = sField;
            sTemp = Strings.Replace(sTemp, @"\{", "[{[", 1, -1, Constants.vbTextCompare);
            sTemp = Strings.Replace(sTemp, @"\}", "]}]", 1, -1, Constants.vbTextCompare);

            sLookFor = "{f" + Strings.Trim(lFuncIndex.ToString()) + "}";
            sTemp = Strings.Replace(sTemp, sLookFor, sReplaceText, 1, -1, Constants.vbTextCompare);

            sTemp = Strings.Replace(sTemp, "[{[", @"\{", 1, -1, Constants.vbTextCompare);
            sTemp = Strings.Replace(sTemp, "]}]", @"\}", 1, -1, Constants.vbTextCompare);
            ProcessFunctionRet = sTemp;
            return ProcessFunctionRet;
        }

        private string PostProcessFunction(string sField)
        {
            string PostProcessFunctionRet = default;
            string sTemp;
            sTemp = sField;
            sTemp = Strings.Replace(sTemp, @"\{", "{", 1, -1, Constants.vbTextCompare);
            sTemp = Strings.Replace(sTemp, @"\}", "}", 1, -1, Constants.vbTextCompare);
            PostProcessFunctionRet = sTemp;
            return PostProcessFunctionRet;
        }

        public bool StripQuotes(ref string sStripFrom, string sQuote = "\"", bool bReturnBlankOK = true)
        {
            bool StripQuotesRet = default;
            string sTestString;
            ;

            if (Strings.StrComp(Strings.Left(sStripFrom, 1), sQuote) == 0 & Strings.StrComp(Strings.Right(sStripFrom, 1), sQuote) == 0)
            {
                sTestString = Strings.Mid(sStripFrom, 2, (int)(Strings.Len(sStripFrom) - 2L));
                if (Strings.Len(Strings.Trim(sTestString)) > 0 | bReturnBlankOK)
                    sStripFrom = sTestString;
                StripQuotesRet = true;
            }

            return StripQuotesRet;
        }

        public bool CheckIfObjectRegister(string objName, Enums.SecureObjectType objType)
        {
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            try
            {
                bool returnFlag = false;
                if (objName is not null & !string.IsNullOrEmpty(objName))
                {
                    var objId = _iSecureObject.Where(m => Operators.ConditionalCompareObjectEqual(m.SecureObjectTypeID, objType, false) & m.Name.Trim().ToLower().Equals(objName.Trim().ToLower()));
                    if (objId is null)
                    {
                        returnFlag = false;
                    }
                    else
                    {
                        returnFlag = true;
                    }

                }
                return returnFlag;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SetRegisterTable(string objName, Enums.SecureObjectType objType)
        {
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            try
            {
                if (objName is not null & !string.IsNullOrEmpty(objName))
                {
                    var SecureObjEntity = new TabFusionRMS.Models.SecureObject();
                    var TableTypeId = _iSecureObject.All().Where(m => Operators.ConditionalCompareObjectEqual(m.Name, "Tables", false)).FirstOrDefault();
                    if (TableTypeId is not null)
                    {
                        SecureObjEntity.SecureObjectTypeID = TableTypeId.SecureObjectTypeID;
                        SecureObjEntity.BaseID = TableTypeId.SecureObjectTypeID;
                    }
                    SecureObjEntity.Name = objName.Trim();
                    _iSecureObject.Add(SecureObjEntity);
                    _iSecureObject.CommitTransaction();
                    var pSecureObject = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals(objName.Trim().ToLower())).FirstOrDefault();
                    var sSql = "INSERT INTO SecureObjectPermission (GroupID, SecureObjectID, PermissionID) SELECT GroupID," + pSecureObject.SecureObjectID + " AS SecureObjectId, PermissionID FROM SecureObjectPermission AS SecureObjectPermission WHERE     (SecureObjectID = " + pSecureObject.BaseID + ") AND (PermissionID IN (SELECT     PermissionID FROM          SecureObjectPermission AS SecureObjectPermission_1 WHERE (SecureObjectID = " + objType + ") AND (GroupID = 0)))";

                    _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
                    _IDBManager.ExecuteNonQuery(CommandType.Text, sSql);
                    _IDBManager.Dispose();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Table MakeSureIsALoadedTable(string sTableName, string sUserName = "")
        {
            var oTables = new Table();
            var dbEngine = new DatabaseEngine();

            oTables.TableName = sTableName.Trim();
            oTables.IdFieldName2 = "";

            if (dbEngine.DBName != "DB_Engine")
            {
                oTables.DBName = dbEngine.DBName;
            }
            else
            {
                oTables.DBName = "";
            }

            switch (sTableName.ToUpper() ?? "")
            {
                case "SYSNEXTTRACKABLE":
                    {
                        oTables.IdFieldName = "NextTrackablesId";
                        break;
                    }
                case "LITIGATIONSUPPORT":
                    {
                        oTables.IdFieldName = "CaseNumber";
                        break;
                    }
                case "LINKSCRIPT":
                    {
                        oTables.IdFieldName = "ScriptName";
                        oTables.IdFieldName2 = "ScriptSequence";
                        break;
                    }
                case "TRACKABLES":
                    {
                        oTables.IdFieldName = "Id";
                        oTables.IdFieldName2 = "RecordVersion";
                        break;
                    }
                case "DATABASES":
                    {
                        oTables.IdFieldName = "DBName";
                        break;
                    }
                case "TABLES":
                    {
                        oTables.IdFieldName = "TableName";
                        break;
                    }
                case "SLRETENTIONCODES":
                    {
                        oTables.TableName = "SLRetentionCodes";
                        oTables.IdFieldName = "Id";
                        oTables.DescFieldPrefixOne = "Retention Code";
                        oTables.DescFieldNameOne = "Id";
                        oTables.DescFieldPrefixTwo = "Retention Desc";
                        oTables.DescFieldNameTwo = "Description";
                        if (Strings.Len(sUserName) == 0)
                            sUserName = "Retention Codes";

                        if (!CheckIfObjectRegister(sTableName, Enums.SecureObjectType.Table))
                        {
                            SetRegisterTable(sTableName, Enums.SecureObjectType.Table);
                        }

                        break;
                    }
                case "SLRETENTIONCITATIONS":
                    {
                        oTables.TableName = "SLRetentionCitations";
                        oTables.IdFieldName = "Citation";
                        if (Strings.Len(sUserName) == 0)
                            sUserName = "Citations Codes";

                        if (!CheckIfObjectRegister(sTableName, Enums.SecureObjectType.Table))
                        {
                            SetRegisterTable(sTableName, Enums.SecureObjectType.Table);
                        }

                        break;
                    }
                case "SLRETENTIONCITACODES":
                    {
                        oTables.TableName = "SLRetentionCitaCodes";
                        oTables.IdFieldName = "Id";
                        if (Strings.Len(sUserName) == 0)
                            sUserName = "Retention Citations Cross Reference";
                        if (!CheckIfObjectRegister(sTableName, Enums.SecureObjectType.Table))
                        {
                            SetRegisterTable(sTableName, Enums.SecureObjectType.Table);
                        }

                        break;
                    }
                case "OPERATORS":
                    {
                        oTables.IdFieldName = "UserName";
                        break;
                    }
                case "SECUREGROUP":
                    {
                        oTables.IdFieldName = "GroupId";
                        break;
                    }
                case "SECUREOBJECT":
                    {
                        oTables.IdFieldName = "SecureObjectId";
                        break;
                    }
                case "SECUREOBJECTPERMISSION":
                    {
                        oTables.IdFieldName = "SecureObjectPermissionId";
                        break;
                    }
                case "SECUREPERMISSION":
                    {
                        oTables.IdFieldName = "PermissionId";
                        break;
                    }
                case "SECUREPERMISSIONDESCRIPTION":
                    {
                        oTables.IdFieldName = "SecureObjectId";
                        oTables.IdFieldName2 = "PermissionId";
                        break;
                    }
                case "SECUREUSER":
                    {
                        oTables.IdFieldName = "UserId";
                        break;
                    }

                default:
                    {
                        oTables.IdFieldName = "Id";
                        break;
                    }
            }

            oTables.OutTable = (short?)Enums.geTrackingOutType.otUseOutField;
            oTables.IdFieldName = sTableName + "." + oTables.IdFieldName;
            oTables.ADOQueryTimeout = 30;
            oTables.ADOCacheSize = 1;
            oTables.ADOServerCursor = false;
            if (Strings.Len(sUserName) > 0)
            {
                oTables.UserName = sUserName;
            }
            else
            {
                oTables.UserName = sTableName;
            }
            return oTables;
        }
        #endregion
    }
}