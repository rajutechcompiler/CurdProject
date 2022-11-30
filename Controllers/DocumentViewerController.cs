using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Smead.Security;
using System.Collections.Generic;
using System.Linq;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.DataBaseManagerVB;
using System.Globalization;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Text;
using System.Threading;
using Smead.RecordsManagement;
using Exceptions = Smead.RecordsManagement.Imaging.Permissions.ExceptionString;
using System.IO;
using System.IO.Compression;
using System.Data;
using TabFusionRMS.Resource;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Http;
using System.Text;
using TabFusionRMS.WebCS.FusionElevenModels;
using Leadtools;
using Leadtools.Codecs;

namespace TabFusionRMS.WebCS.Controllers
{
    public class DocumentViewerController : BaseController
    {

        private readonly IConfiguration _config;
        //private static ObjectCache Cache { get; }
        private static string DocumentService { get; set; }
        public DocumentViewerController(IConfiguration fconfig)
        {
            this._config = fconfig;
            DocumentService = _config.GetSection("lt.leadtools.documentService").Value;
        }
        public IActionResult Index([FromServices] IConfiguration configuration, string documentKey)
        {
            var ViewModel = new DocumentViewerModel();
            if (documentKey == null)
                ViewModel.HasLink = "False";
            else
                try
                {
                    ViewModel.HasLink = "True";
                    ViewModel.documentKey = documentKey;
                    ViewModel.DocumentService = DocumentService;
                    GetListOfAttachments(configuration, ViewModel, -1);
                }
                catch (Exception ex)
                {
                    slimShared.SlimShared.logError(ex.Message);
                }
            return View(ViewModel);
        }

        public IActionResult Orphan(string documentKey)
        {
            var ViewModel = new DocumentViewerOrphanModel();
            ViewModel.IsVault = true;
            if (documentKey == null)
                ViewModel.HasLink = "False";
            else
                try
                {
                    ViewModel.HasLink = "True";
                    ViewModel.documentKey = documentKey;
                    ViewModel.AttachmentNumber = -1;
                    ViewModel.DocumentService = DocumentService;    
                    GetListOfOrphanAttachments(ViewModel, -1);
                }
                catch (Exception ex)
                {
                    slimShared.SlimShared.logError(ex.Message);
                }
            return View(ViewModel);
        }

        public void GetListOfAttachments(IConfiguration configuration, DocumentViewerModel viewModel, int AttachmentNumber = -1)
        {
            var TABDocService = ContextService.AppSetting("DocumentServicePort", configuration);


            //MySession.Current.sConnectionString = Keys.get_GetDBConnectionString();
            var _iTable = new RepositoryVB.Repositories<Table>();
            var _iDatabas = new RepositoryVB.Repositories<Databas>();
            var _iSystem = new RepositoryVB.Repositories<TabFusionRMS.Models.System>();
            var _iPCFilePointer = new RepositoryVB.Repositories<PCFilesPointer>();
            var _iUserLink = new RepositoryVB.Repositories<Userlink>();
            var _iImagePointer = new RepositoryVB.Repositories<ImagePointer>();
            var oSystem = _iSystem.All().FirstOrDefault();
            viewModel.renameOnScan = oSystem.RenameOnScan ?? false;
            string queryString = viewModel.documentKey;
            queryString = viewModel.documentKey.Substring(queryString.IndexOf("?") + 1);
            queryString = Common.DecryptURLParameters(queryString);
            string[] values = queryString.Split("&");
            viewModel.TableName = values[1].Split("=")[1];
            viewModel.AttachmentNumberClick = values[2].Split("=")[1];
            viewModel.Tableid = values[0].Split("=")[1];

            ViewAttachmentOnLoad(viewModel, false, AttachmentNumber);
        }

        public void GetListOfOrphanAttachments(DocumentViewerOrphanModel viewModel, int AttachmentNumber = -1)
        {
            string queryString = viewModel.documentKey;
            queryString = viewModel.documentKey.Substring(queryString.IndexOf("?") + 1);
            queryString = Common.DecryptURLParameters(queryString);
            string[] values = queryString.Split("&");
            // Dim PointerId As Integer = 0
            viewModel.trackbalesId = Convert.ToInt32(values[1].Split("=")[1]);
            viewModel.RecordType = Convert.ToInt32(values[2].Split("=")[1]);
            viewModel.FileName = values[4].Split("=")[1];
            viewModel.TableName = "Orphan";
            viewModel.AttachmentNumberClick = "1";
            viewModel.Tableid = "0";
            viewModel.FilePath = values[3].Split("=")[1];
            viewModel.ShouldLast = false;
            ViewOrphanAttachmentOnLoad(viewModel);
        }

        private void ViewAttachmentOnLoad(DocumentViewerModel ViewModel, bool ShouldLast = false, int AttachmentNumber = -1)
        {
            try
            {
                if (!passport.CheckPermission(ViewModel.TableName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.View))
                {
                    ViewModel.ErrorMsg = Languages.get_Translation("msgHTML5CtrlNoPermissionViews"); // "no permissions to view"
                    ViewModel.isPermission = false;
                    return;
                }

                bool canviewOtherVersions = passport.CheckPermission(ViewModel.TableName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Versioning);

                DataSet loutput = GetAllAttachmentVersion(ViewModel);
                var cnt = loutput.Tables[0].Rows.Count;
                int CurrentVersion = 0;
                if ((cnt == 0))
                    ViewModel.FilePath = "empty";
                else
                {
                    ViewModel.FilePath = loutput.Tables[0].Rows[0]["FullPath"].ToString();
                    // Dim checkVersion As String = "False"
                    int lastAttachmentNumber = -1;

                    foreach (DataRow row in loutput.Tables[0].Rows)
                    {
                        // Dim addAttachment As Boolean = False

                        if (lastAttachmentNumber == -1)
                        {
                            lastAttachmentNumber = System.Convert.ToInt32(row["AttachmentNumber"]);
                            CurrentVersion = 0;
                        }
                        else if (lastAttachmentNumber != System.Convert.ToInt32(row["AttachmentNumber"]))
                        {
                            lastAttachmentNumber = System.Convert.ToInt32(row["AttachmentNumber"]);
                            CurrentVersion = 0;
                        }

                        int PageNumber = row["PageNumber"].IntValue();
                        string path = row["FullPath"].Text();
                        var Name = row["OrgFileName"].Text();
                        var noteCount = System.Convert.ToInt32(row["NoteCount"]);
                        var Version = System.Convert.ToInt32(row["RecordVersion"]);
                        var pointerid = System.Convert.ToInt32(row["PointerId"]);
                        if (CurrentVersion == 0)
                            CurrentVersion = Version;
                        var attchNumber = Convert.ToString(row["AttachmentNumber"]);
                        var RecordType = Convert.ToInt32(row["RecordType"]);
                        var trackbalesId = Convert.ToInt32(row["TrackablesId"]);
                        // Dim hasAnnotation As Boolean = CheckAnnotation(pointerid, attchNumber, Version, PageNumber)
                        var noName = string.Format(Languages.get_Translation("ddlDocumentViewerAttachmentName"), row["AttachmentNumber"].ToString());
                        if ((Information.IsDBNull(row["OrgFileName"])))
                            Name = noName;

                        if ((PageNumber == 0 || PageNumber == 1))
                        {
                            if ((canviewOtherVersions) || (!canviewOtherVersions && Version == CurrentVersion))
                            {
                                if ((Version > 0) || (Information.IsDBNull(row["CheckedOutUserId"]) || passport.UserId == System.Convert.ToInt32(row["CheckedOutUserId"])))
                                {
                                    var VolumName = Convert.ToString(row["VolumnName"]);
                                    if (passport.CheckPermission(VolumName, Smead.Security.SecureObject.SecureObjectType.Volumes, Smead.Security.Permissions.Permission.View))
                                        ViewModel.AttachmentList.Add(new UIparams(Name, path, attchNumber, Math.Abs(Version), PageNumber, RecordType, noteCount, pointerid, trackbalesId, passport, ViewModel));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                slimShared.SlimShared.logError(string.Format("Error \"{0}\" occurred in DocumentViewerController.ViewAttachmentOnLoad", ex.Message));
            }
        }

        private void ViewOrphanAttachmentOnLoad(DocumentViewerOrphanModel ViewModel)
        {
            try
            {
                var attchNumber = Convert.ToString("1");
                var Version = System.Convert.ToInt32(1);
                int PageNumber = 1;
                var noteCount = System.Convert.ToInt32(1);

                ViewModel.AttachmentList.Add(new UIparams(ViewModel.FileName, ViewModel.FilePath, attchNumber, Math.Abs(Version), PageNumber, ViewModel.RecordType, noteCount, ViewModel.PointerId, ViewModel.trackbalesId, passport, ViewModel));
            }
            catch (Exception ex)
            {
                slimShared.SlimShared.logError(string.Format("Error \"{0}\" occurred in DocumentViewerController.ViewOrphanAttachmentOnLoad", ex.Message));
            }
        }
        [HttpPost]
        public string GetAttachmtsPermissions(List<string> getVariable)
        {
            var queryString = Common.DecryptURLParameters(getVariable[0]);
            string[] values = queryString.Split("&");
            var oTableName = values[1].Split("=")[1];
            bool aPermission = false;
            string tabName = oTableName;
            try
            {
                switch (getVariable[1])
                {
                    case "Add":
                        {
                            aPermission = passport.CheckPermission(tabName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Add);
                            break;
                        }

                    case "Edit":
                        {
                            aPermission = passport.CheckPermission(tabName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Edit);
                            break;
                        }

                    case "Delete":
                        {
                            aPermission = passport.CheckPermission(tabName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Delete);
                            break;
                        }

                    case "Print":
                        {
                            aPermission = passport.CheckPermission(tabName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Print);
                            break;
                        }

                    default:
                        {
                            aPermission = false;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                slimShared.SlimShared.logError(string.Format("Error \"{0}\" occurred in DocumentViewerController.GetAttachmtsPermissions", ex.Message));
                aPermission = false;
            }
            return aPermission.ToString();
        }

        private DataSet GetAllAttachmentVersion(DocumentViewerModel viewModel)
        {
            var queryString = viewModel.documentKey;
            string recordIdParam = "";
            queryString = queryString.Substring(queryString.IndexOf("?") + 1);
            queryString = Common.DecryptURLParameters(queryString);
            var _iTable = new RepositoryVB.Repositories<Table>();
            var _iDatabas = new RepositoryVB.Repositories<Databas>();

            string[] values = queryString.Split("&");
            viewModel.TableName = values[1].Split("=")[1];
            // fix-https://jiratabfusion.atlassian.net/browse/FUS-6424
            // check the id type
            var command = string.Format("select TOP 1 * from {0}", viewModel.TableName);
            var cmd = new SqlCommand(command, passport.Connection());
            var reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
            var schematable = reader.GetSchemaTable();
            var idType = schematable.Rows[0]["DataType"].ToString();



            if (idType == "System.Int32")
            {
                recordIdParam = values[0].Split("=")[1].PadLeft(30, '0');
                var record = Convert.ToInt32((values[0].Split("=")[1]));
                viewModel.RecordId = record.ToString();
            }
            else
            {
                viewModel.RecordId = System.Net.WebUtility.HtmlDecode(values[0].Split("=")[1]);
                recordIdParam = System.Net.WebUtility.HtmlDecode(values[0].Split("=")[1]);
            }


            // gets crumbs
            viewModel.crumbName = _iTable.Where(a => a.TableName == viewModel.TableName).FirstOrDefault().DescFieldPrefixOne;
            var attachmentId = values[2].Split("=")[1];

            var oTable = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(viewModel.TableName.Trim().ToLower())).FirstOrDefault();
            if (oTable == null)
                return null/* TODO Change to default(_) if this is not a reference type */;

            var idFieldName = oTable.IdFieldName;
            var csADOConn = new ADODB.Connection();
            csADOConn = DataServices.DBOpen(_iDatabas.All().Where(x => x.DBName.Trim().ToLower().Equals(oTable.DBName.Trim().ToLower())).FirstOrDefault());

            if (csADOConn != null)
            {
                if (!(DataServices.IdFieldIsString(csADOConn, viewModel.TableName, idFieldName)))
                    viewModel.RecordId = viewModel.RecordId;
                csADOConn.Close();
            }

            using (DBManager abc = new DBManager(Keys.get_GetDBConnectionString()))
            {
                abc.CreateParameters(2);
                abc.AddParameters(0, "@tableId", recordIdParam);
                abc.AddParameters(1, "@tableName", viewModel.TableName);
                return abc.ExecuteDataSet(System.Data.CommandType.StoredProcedure, "SP_RMS_GetAllAttchmentVersion");
            }
        }

        [HttpGet]
        public string GetDefaultSystemDrive(string opt)
        {
            bool bIsValidOutputSettings = false;
            bool bIsOutputSettingsActive = false;
            bool vPermission = false;
            try
            {
                RepositoryVB.Repositories<TabFusionRMS.Models.System> _iSystem = new RepositoryVB.Repositories<TabFusionRMS.Models.System>();
                RepositoryVB.Repositories<TabFusionRMS.Models.OutputSetting> _iOutputSetting = new RepositoryVB.Repositories<TabFusionRMS.Models.OutputSetting>();
                RepositoryVB.Repositories<TabFusionRMS.Models.Volume> _iVolume = new RepositoryVB.Repositories<TabFusionRMS.Models.Volume>();
                RepositoryVB.Repositories<TabFusionRMS.Models.SystemAddress> _iSystemAddress = new RepositoryVB.Repositories<TabFusionRMS.Models.SystemAddress>();
                var oSystem = _iSystem.All().FirstOrDefault();

                if (oSystem != null)
                {
                    var oOutputSetting = _iOutputSetting.All().Where(x => x.Id.Trim().ToLower().Equals(oSystem.DefaultOutputSettingsId.Trim().ToLower())).FirstOrDefault();
                    if (oOutputSetting != null)
                    {
                        var oVolume = _iVolume.All().Where(x => x.Id == oOutputSetting.VolumesId).FirstOrDefault();
                        if (oVolume != null)
                        {
                            switch (opt)
                            {
                                case "Add":
                                    {
                                        vPermission = passport.CheckPermission(oVolume.Name, Smead.Security.SecureObject.SecureObjectType.Volumes, Smead.Security.Permissions.Permission.Add);
                                        break;
                                    }

                                case "Edit":
                                    {
                                        vPermission = passport.CheckPermission(oVolume.Name, Smead.Security.SecureObject.SecureObjectType.Volumes, Smead.Security.Permissions.Permission.Edit);
                                        break;
                                    }

                                case "Delete":
                                    {
                                        vPermission = passport.CheckPermission(oVolume.Name, Smead.Security.SecureObject.SecureObjectType.Volumes, Smead.Security.Permissions.Permission.Delete);
                                        break;
                                    }

                                default:
                                    {
                                        vPermission = passport.CheckPermission(oVolume.Name, Smead.Security.SecureObject.SecureObjectType.Volumes, Smead.Security.Permissions.Permission.View);
                                        break;
                                    }
                            }

                            if (oVolume.Active != false)
                            {
                                var oSystemAddress = _iSystemAddress.All().Where(x => x.Id == oVolume.SystemAddressesId).FirstOrDefault();
                                if (oSystemAddress != null)
                                {
                                    string checkPath = oSystemAddress.PhysicalDriveLetter;
                                    if (checkPath.StartsWith(@"\\"))
                                        checkPath += string.Format("{0}{1}", oVolume.PathName.StartsWith(@"\") ? string.Empty : @"\", oVolume.PathName);
                                    bIsValidOutputSettings = System.IO.Directory.Exists(checkPath) && passport.CheckPermission(oOutputSetting.Id, Smead.Security.SecureObject.SecureObjectType.OutputSettings, Smead.Security.Permissions.Permission.Access);
                                    bIsOutputSettingsActive = Convert.ToBoolean(oOutputSetting.InActive) == false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                slimShared.SlimShared.logError(string.Format("Error \"{0}\" occurred in DocumentViewerController.GetDefaultSystemDrive", ex.Message));
            }
            return "ValidOutput:" + bIsValidOutputSettings.ToString() + ", vPermission:" + vPermission.ToString() + ", OutputActive:" + bIsOutputSettingsActive.ToString();
        }

        #region "ADD Attachment"
        [HttpPost]
        public async Task<IActionResult> BtnOkClickAddFileWithName([FromServices] IConfiguration configuration, List<string> getVariable)
        {
            var errmodel = new ErrorBaseModel();
            DocumentViewerModel ViewModel = new DocumentViewerModel();
            try
            {
                string keydocument = getVariable[0];
                string filenamerequst = getVariable[1];

                var File = httpContext.Request.Form.Files[0];
                string queryString = keydocument;

                ViewModel.documentKey = keydocument;
                queryString = Common.DecryptURLParameters(queryString);
                string[] values = queryString.Split("&");
                var oTableName = System.Net.WebUtility.UrlDecode(values[1].Split("=")[1]);
                var oTableId = System.Net.WebUtility.UrlDecode(values[0].Split("=")[1]);
                var PostedFile = httpContext.Request.Form.Files[0];
                var contentPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath");
                var ServerPath = contentPath + @"\wwwroot\ImportFiles\" + passport.UserId.ToString() + @"\";
                if ((!System.IO.Directory.Exists(ServerPath)))
                    System.IO.Directory.CreateDirectory(ServerPath);


                var filePath = ServerPath + PostedFile.FileName;
                using (var stream = System.IO.File.Create(filePath))
                {
                    await PostedFile.CopyToAsync(stream);
                }

                string oAttachName = filenamerequst; // Request.Form("AttachmentID")
                //if (!CheckFileHealth(filePath, ViewModel))
                    AddAnAttachmentToDataBaseAndHardDrive(oTableName, oTableId, filePath, oAttachName);

                GetListOfAttachments(configuration, ViewModel, -1);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, errmodel, 0, passport.DatabaseName);
                ViewModel.isError = errmodel.isError;
                ViewModel.ErrorMsg = ex.Message;
            }
            return PartialView("_DocviewerInit", ViewModel);
        }

        //private bool CheckFileHealth(string filepath, DocumentViewerModel viewmodel)
        //{
        //    var errmodel = new ErrorBaseModel();
        //    bool isFileCorrupted = false;
        //    try
        //    {
        //        using (RasterCodecs codec = new RasterCodecs())
        //        {
        //            using (RasterImage img = codec.Load(filepath, 1))
        //            {
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message == "Invalid file format")
        //            isFileCorrupted = false;
        //        else
        //        {
        //            isFileCorrupted = true;
        //            DataErrorHandler.ErrorHandler(ex, errmodel, 0, passport.DatabaseName);
        //            viewmodel.isError = errmodel.isError;
        //            viewmodel.ErrorMsg = ex.Message;
        //        }
        //    }
        //    return isFileCorrupted;
        //}

        [HttpPost]
        //moti mashiah leadtools
        public ActionResult AddAttachmentFromScan([FromServices] IConfiguration configuration, List<string> getVariable)
        {
            // Dim fileNameAfterSave As String
            DocumentViewerModel ViewModel = new DocumentViewerModel();
            var directory = ContextService.AppSetting("lt.Cache.Directory", configuration);
            try
            {
                string keydocument = getVariable[0];
                string filenamerequst = getVariable[1];
                string queryString = keydocument;
                ViewModel.documentKey = keydocument;
                queryString = Common.DecryptURLParameters(queryString);
                string[] values = queryString.Split("&");
                var oTableName = System.Net.WebUtility.UrlDecode(values[1].Split("=")[1]);
                var oTableId = System.Net.WebUtility.UrlDecode(values[0].Split("=")[1]);
                var filePath = directory + "/" + getVariable[2];
                string oAttachName = filenamerequst;
                AddAnAttachmentToDataBaseAndHardDrive(oTableName, oTableId, filePath, oAttachName);
                GetListOfAttachments(configuration, ViewModel, -1);
                return PartialView("_DocviewerInit", ViewModel);
            }
            catch (Exception ex)
            {
                return PartialView("_DocviewerInit", ViewModel);
            }
        }

        [HttpPost]
        public ActionResult returnListOfAttachment([FromServices] IConfiguration configuration, string keydocument)
        {
            DocumentViewerModel ViewModel = new DocumentViewerModel();
            ViewModel.documentKey = keydocument;
            GetListOfAttachments(configuration, ViewModel, -1);
            return PartialView("_DocviewerInit", ViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddMultipleAttachments([FromServices] IConfiguration configuration, string Keydocument)
        {
            DocumentViewerModel ViewModel = new DocumentViewerModel();
            try
            {
                string queryString = Keydocument;
                queryString = Common.DecryptURLParameters(queryString);
                ViewModel.documentKey = Keydocument;
                string[] values = queryString.Split("&");
                var oTableName = System.Net.WebUtility.UrlDecode(values[1].Split("=")[1]);
                var oTableId = System.Net.WebUtility.UrlDecode(values[0].Split("=")[1]);
                var contentPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath");
                var ServerPath = contentPath + @"\wwwroot\ImportFiles\";
                if ((!System.IO.Directory.Exists(ServerPath)))
                    System.IO.Directory.CreateDirectory(ServerPath);

                if ((httpContext.Request.Form.Files.Count > 1))
                {
                    for (int i = 0; i <= httpContext.Request.Form.Files.Count - 1; i++)
                    {
                        var PostedFile = httpContext.Request.Form.Files[i];
                        if (PostedFile.Length > 0)
                        {
                            string FileName = System.Guid.NewGuid().ToString();
                            FileName = FileName + Path.GetExtension(PostedFile.FileName);
                            using (var stream = System.IO.File.Create(ServerPath + FileName))
                            {
                                await PostedFile.CopyToAsync(stream);
                            }
                            var filePath = ServerPath + FileName;
                            AddAnAttachmentToDataBaseAndHardDrive(oTableName, oTableId, filePath, string.Empty);
                        }
                    }
                    GetListOfAttachments(configuration, ViewModel, -1);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return PartialView("_DocviewerInit", ViewModel);
        }


        private async void AddAnAttachmentToDataBaseAndHardDrive(string oTableName, string oTableId, string filePath, string name)
        {
            var call = new ApiCalls(_config);
            if (VerifyDrivePermissions("Add") && VerifySecurePermission("Add", oTableName))
            {
                var Server = ContextService.GetSessionValueByKey("Server", httpContext);
                var DatabaseName = ContextService.GetSessionValueByKey("DatabaseName", httpContext);
                var _iSystem = new RepositoryVB.Repositories<TabFusionRMS.Models.System>();
                var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", Server, DatabaseName), oTableName, oTableId).ToString();
                var oDefaultOutputSetting = _iSystem.All().FirstOrDefault()?.DefaultOutputSettingsId;
                var info = await call.DocumentServices.GetCodecInfoFromFile(filePath, Path.GetExtension(filePath));//Common.GetCodecInfoFromFile(filePath, Path.GetExtension(filePath));
                if (info == null)
                    Smead.RecordsManagement.Imaging.Attachments.AddAnAttachment(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", Server, DatabaseName), oTableName, oTableId, 0, oDefaultOutputSetting, filePath, filePath, Path.GetExtension(filePath), false, name, false, 1, 0, 0, 0);
                else
                    Smead.RecordsManagement.Imaging.Attachments.AddAnAttachment(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", Server, DatabaseName), oTableName, oTableId, 0, oDefaultOutputSetting, filePath, filePath, Path.GetExtension(filePath), false, name, true, info.TotalPages, info.Height, info.Width, info.SizeDisk);
            }
        }


        #endregion

        public bool VerifyDrivePermissions(string objc)
        {
            // this method created to check again if there is any hacking on the browser level.
            var hasPermision = GetDefaultSystemDrive(objc);
            var validOutput = hasPermision.Split(",")[0].Split(":")[1].ToString();
            var vPermission = hasPermision.Split(",")[1].Split(":")[1].ToString();
            var OutputActive = hasPermision.Split(",")[2].Split(":")[1].ToString();
            if (validOutput != "True" || vPermission != "True" || OutputActive != "True")
                return false;
            else
                return true;
        }

        public bool VerifySecurePermission(string objc, string tabName)
        {
            // this method created to check again if there is any hacking on the browser level.
            bool aPermission = false;
            try
            {
                switch (objc)
                {
                    case "Add":
                        {
                            aPermission = passport.CheckPermission(tabName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Add);
                            break;
                        }

                    case "Edit":
                        {
                            aPermission = passport.CheckPermission(tabName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Edit);
                            break;
                        }

                    case "Delete":
                        {
                            aPermission = passport.CheckPermission(tabName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Delete);
                            break;
                        }

                    case "Print":
                        {
                            aPermission = passport.CheckPermission(tabName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Print);
                            break;
                        }

                    default:
                        {
                            aPermission = false;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                aPermission = false;
            }
            return aPermission;
        }

        #region "DELETE Attachment"
        public IActionResult BtnokClickDeleteAttachment([FromServices] IConfiguration configuration, string Keydocument, List<string> attachments)
        {
            if ((!VerifyDrivePermissions("Delete") == true))
                return new EmptyResult();

            try
            {
                DocumentViewerModel ViewModel = new DocumentViewerModel();
                ViewModel.documentKey = Keydocument;
                var queryString = Keydocument;
                queryString = Common.DecryptURLParameters(queryString);

                string[] values = queryString.Split("&");
                var oTableName = values[1].Split("=")[1];
                var oTableId = values[0].Split("=")[1];
                int VersionNumber = 0;
                Smead.RecordsManagement.Imaging.Attachment ReturnMessage = null/* TODO Change to default(_) if this is not a reference type */;

                foreach (var v in attachments)
                {
                    var getValueRow = v.Split(",");
                    var attachmentNum = getValueRow[0];
                    var versionNum = getValueRow[1];
                    var PageNum = getValueRow[2];
                    Int16 IsAttachmentDelete = 1;
                    var AttachmentNumber = Convert.ToInt32(attachmentNum);
                    if (passport.CheckPermission(oTableName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Versioning))
                        VersionNumber = Convert.ToInt32(versionNum);
                    else
                        VersionNumber = 0;
                    // delete from the cart in case the file appear there
                    try
                    {
                        string command1 = "delete from s_AttachmentCart where attachmentNumber = @attachmentNumber and versionNumber = @versionNumber";
                        using (SqlCommand cmd = new SqlCommand(command1, passport.Connection()))
                        {
                            cmd.Parameters.AddWithValue("@attachmentNumber", AttachmentNumber);
                            cmd.Parameters.AddWithValue("@versionNumber", versionNum);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    var PageNumber = Convert.ToInt32(PageNum);
                    var Server = ContextService.GetSessionValueByKey("Server", httpContext);
                    var DatabaseName = ContextService.GetSessionValueByKey("DatabaseName", httpContext);
                    var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", Server, DatabaseName), oTableName, oTableId).ToString();
                    var ChckOutVersion = CheckIfIsCheckOut(AttachmentNumber, Keydocument);
                    if ((IsAttachmentDelete == 0))
                    {
                        if ((ChckOutVersion == 0))
                            ReturnMessage = Smead.RecordsManagement.Imaging.Attachments.DeleteAttachment(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", Server, DatabaseName), oTableName, oTableId, AttachmentNumber, 0, false);
                        else
                        {
                        }
                    }
                    else if ((IsAttachmentDelete == 1))
                    {
                        if ((ChckOutVersion == 0))
                            ReturnMessage = Smead.RecordsManagement.Imaging.Attachments.DeleteAttachment(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", Server, DatabaseName), oTableName, oTableId, AttachmentNumber, VersionNumber, false);
                        else
                        {
                        }
                    }
                    else if ((IsAttachmentDelete == 2))
                    {
                        if ((ChckOutVersion == 0))
                        {
                        }
                        else
                        {
                            string ReturnString = Smead.RecordsManagement.Imaging.Attachments.DeletePage(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", Server, DatabaseName), oTableName, oTableId, AttachmentNumber, ChckOutVersion, PageNumber);
                            if (!string.IsNullOrWhiteSpace(ReturnString))
                                ReturnMessage = new Smead.RecordsManagement.Imaging.ErrorAttachment(new Exception(ReturnString), passport.UserId, ContextService.GetSessionValueByKey("Database", httpContext), oTableName, oTableId);
                        }
                    }

                    if (ReturnMessage != null)
                    {
                        if (ReturnMessage is Smead.RecordsManagement.Imaging.ErrorAttachment)
                        {
                            if (string.IsNullOrWhiteSpace(ViewModel.ErrorMsg))
                                ViewModel.ErrorMsg = "";//Smead.RecordsManagement.Imaging.ErrorAttachment)ReturnMessage; // check raju
                            else
                                ViewModel.ErrorMsg = "";//string.Format("{0}{1}{2}", ViewModel.ErrorMsg, Environment.NewLine, (Imaging.ErrorAttachment)ReturnMessage.Message); // check raju
                        }
                        ReturnMessage = null/* TODO Change to default(_) if this is not a reference type */;
                    }
                }

                GetListOfAttachments(configuration, ViewModel, -1);
                return PartialView("_DocviewerInit", ViewModel);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public JsonResult UIMultipelFilecallCheckIffileIscheckOut(string documentKey, List<string> attachments)
        {
            DocumentViewerModel ViewModel = new DocumentViewerModel();

            var queryString = documentKey;
            queryString = Common.DecryptURLParameters(queryString);
            string[] values = queryString.Split("&");
            var tabName = values[1].Split("=")[1];

            if (passport.CheckPermission(tabName, Smead.Security.SecureObject.SecureObjectType.Attachments, Smead.Security.Permissions.Permission.Delete))
            {
                foreach (var a in attachments)
                {
                    var value = a.Split(",");
                    var number = Convert.ToInt32(value[0]);
                    var FileFullName = value[1];
                    var isCheckedOut = CheckIfIsCheckOut(number, documentKey);
                    if (isCheckedOut == 0)
                        // ViewModel.MsgFileCheckout.Add(FileFullName + "  Delete approve")
                        ViewModel.MsgFileCheckout.Add(string.Format(Languages.get_Translation("msgHTML5CtrlDelApprove"), FileFullName));
                    else
                        // ViewModel.MsgFileCheckout.Add(FileFullName + "  File is checked out can't be delete")
                        ViewModel.MsgFileCheckout.Add(string.Format(Languages.get_Translation("msgHTML5CtrlFileCheckedOutNoDel"), FileFullName));
                }
                ViewModel.ErrorMsg = "";
            }
            else
                ViewModel.ErrorMsg = Languages.get_Translation("msgHTML5CtrlNoDelPer");// "Don't have Delete permissions"
            return Json(ViewModel);
        }

        #endregion

        #region "Add version"
        public async Task<IActionResult> UploadVersionAndsave([FromServices] IConfiguration configuration, List<string> getvalues)
        {
            DocumentViewerModel ViewModel = new DocumentViewerModel();
            try
            {
                string keyDocument = getvalues[0];
                int AttachmentNumber = Convert.ToInt32(getvalues[1]);
                ViewModel.documentKey = keyDocument;

                var contentPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath");
                var ServerPath = contentPath + @"\wwwroot\ImportFiles\" + passport.UserId.ToString() + @"\";

                // Dim AttachmentNumber As Integer = Convert.ToInt32(Request.Form("AttachmentsDDL"))
                if ((!System.IO.Directory.Exists(ServerPath)))
                    System.IO.Directory.CreateDirectory(ServerPath);

                if ((httpContext.Request.Form.Files.Count > 0))
                {
                    var ChckOutVersion = CheckIfIsCheckOut(AttachmentNumber, keyDocument);
                    if ((ChckOutVersion == 0))
                    {
                        var queryString = keyDocument; // Request.RawUrl.ToString()
                                                       // queryString = Uri.UnescapeDataString(queryString)
                                                       // queryString = queryString.Replace("%2f", "/")
                        queryString = queryString.Substring(queryString.IndexOf("?") + 1);
                        queryString = Common.DecryptURLParameters(queryString);
                        string[] values = queryString.Split("&");
                        var oTableName = values[1].Split("=")[1];
                        var oTableId = values[0].Split("=")[1];
                        var URLAttachment = AttachmentNumber; // values(2).Split("=")(1)
                        for (int i = 0; i <= httpContext.Request.Form.Files.Count - 1; i++)
                        {
                            var PostedFile = httpContext.Request.Form.Files[i];
                            if (PostedFile.Length > 0)
                            {
                                var FileName = ServerPath + PostedFile.FileName;
                                using (var stream = System.IO.File.Create(FileName))
                                {
                                    await PostedFile.CopyToAsync(stream);
                                }
                                AddAVersion(oTableName, oTableId, FileName, string.Empty, AttachmentNumber);
                            }
                        }
                    }
                    else
                        ViewModel.ErrorMsg = Languages.get_Translation("msgHTML5CtrlFileCheckedOutNoNewVersion");// "File checked out, you can't add a new versions"
                    GetListOfAttachments(configuration, ViewModel, -1);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return PartialView("_DocviewerInit", ViewModel);
        }

        public async Task<IActionResult> UploadAssemblyVersion([FromServices] IConfiguration configuration, List<string> getvalues)
        {
            var call = new ApiCalls(_config);
            var directory = await call.DocumentServices.GetCacheLocation();//ContextService.AppSetting("lt.Cache.Directory", configuration);
            DocumentViewerModel ViewModel = new DocumentViewerModel();
            // prepare veriables
            string keyDocument = getvalues[0];
            ViewModel.documentKey = keyDocument;
            int AttachmentNumber = Convert.ToInt32(getvalues[1]);
            int versionNumber = Convert.ToInt32(getvalues[3]);

            var filePath = directory + "/" + getvalues[2];
            var queryString = keyDocument;
            queryString = queryString.Substring(queryString.IndexOf("?") + 1);
            queryString = Common.DecryptURLParameters(queryString);
            string[] values = queryString.Split("&");
            var oTableName = values[1].Split("=")[1];
            var oTableId = values[0].Split("=")[1];

            // undo checkin before you add version 
            // this is a temporary solution for web checkin process.
            var @params = new List<string>();
            @params.Add(keyDocument);
            @params.Add(AttachmentNumber.ToString());
            @params.Add(versionNumber.ToString());
            UndoCheckOut(@params);

            AddAVersion(oTableName, oTableId, filePath, string.Empty, AttachmentNumber);
            GetListOfAttachments(configuration, ViewModel, -1);
            return PartialView("_DocviewerInit", ViewModel);
        }

        private async void AddAVersion(string oTableName, string oTableId, string filePath, string name, int AttachmentNumber)
        {
            var call = new ApiCalls(_config);
            var _iSystem = new RepositoryVB.Repositories<TabFusionRMS.Models.System>();
            var Server = ContextService.GetSessionValueByKey("Server", httpContext);
            var databaseName = ContextService.GetSessionValueByKey("DatabaseName", httpContext);
            var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", Server, databaseName), oTableName, oTableId).ToString();
            var oDefaultOutputSetting = _iSystem.All().FirstOrDefault()?.DefaultOutputSettingsId;
            var info = await call.DocumentServices.GetCodecInfoFromFile(filePath, System.IO.Path.GetExtension(filePath));//Common.GetCodecInfoFromFile(filePath, System.IO.Path.GetExtension(filePath));
            if (info == null)
                Smead.RecordsManagement.Imaging.Attachments.AddAVersion(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", Server, databaseName), oTableName, oTableId, AttachmentNumber, 0, oDefaultOutputSetting, filePath, filePath, false, Path.GetExtension(filePath), false, false, 0, 0, 0, 0);
            else
                Smead.RecordsManagement.Imaging.Attachments.AddAVersion(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", Server, databaseName), oTableName, oTableId, AttachmentNumber, 0, oDefaultOutputSetting, filePath, filePath, false, Path.GetExtension(filePath), false, true, info.TotalPages, info.Height, info.Width, info.SizeDisk);
        }

        #endregion

        #region "Rename attachment"
        public IActionResult RenameAttachment([FromServices] IConfiguration configuration, List<string> variables)
        {
            DocumentViewerModel ViewModel = new DocumentViewerModel();
            try
            {
                var _iPCFilePointer = new RepositoryVB.Repositories<PCFilesPointer>();
                var _iImagePointer = new RepositoryVB.Repositories<ImagePointer>();
                var _iUserLink = new RepositoryVB.Repositories<Userlink>();
                ViewModel.documentKey = variables[0];
                var queryString = variables[0];
                queryString = queryString.Substring(queryString.IndexOf("?") + 1);
                queryString = Common.DecryptURLParameters(queryString);
                string[] values = queryString.Split("&");
                var oTableName = values[1].Split("=")[1].ToString();
                var oTableId = values[0].Split("=")[1];
                var oAttachmentNumber = Convert.ToInt32(variables[1]);

                var Server = ContextService.GetSessionValueByKey("Server", httpContext);
                var databaseName = ContextService.GetSessionValueByKey("DatabaseName", httpContext);

                var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", Server, databaseName), oTableName, oTableId).ToString();
                var pcFilePointer = _iPCFilePointer.All().AsEnumerable();
                var userLink = _iUserLink.All().AsEnumerable();
                var imagePointer = _iImagePointer.All().AsEnumerable();
                var oTableIdString = oTableId;

                if (!Navigation.FieldIsAString(oTableName, passport.StaticConnection()))
                    oTableIdString = TrackingServices.ZeroPaddedString(oTableId);

                var pcFileId = (from p in pcFilePointer
                                join u in userLink on p.TrackablesId equals u.TrackablesId
                                where u.AttachmentNumber == Convert.ToInt32(oAttachmentNumber) & u.IndexTable == oTableName & u.IndexTableId == oTableIdString
                                select p.Id).ToList();
                var imageId = (from i in imagePointer
                               join u in userLink on i.TrackablesId equals u.TrackablesId
                               where u.AttachmentNumber == Convert.ToInt32(oAttachmentNumber) & u.IndexTable == oTableName & u.IndexTableId == oTableIdString
                               select i.Id).ToList();

                var NewAttachmentName = variables[2];
                if (pcFileId.Count() != (Int32)0)
                {
                    foreach (var a in pcFileId)
                    {
                        // check raju
                        bool oRename = Smead.RecordsManagement.Imaging.Attachments.RenameAttachment(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", Server, databaseName), oTableName, oTableId, oAttachmentNumber, a.ToString(), false, NewAttachmentName);
                    }
                }
                if ((imageId.Count() != (Int32)0))
                {
                    foreach (var a in imageId)
                    {
                        // check raju
                        bool oRename = Smead.RecordsManagement.Imaging.Attachments.RenameAttachment(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", Server, databaseName), oTableName, oTableId, oAttachmentNumber, a.ToString(), true, NewAttachmentName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            GetListOfAttachments(configuration, ViewModel, -1);
            return PartialView("_DocviewerInit", ViewModel);
        }
        #endregion

        private int CheckIfIsCheckOut(int attachmentNum, string documentKey)
        {
            try
            {
                var queryString = documentKey;
                // queryString = Uri.UnescapeDataString(queryString)
                // 'queryString = queryString.Replace("%2f", "/")
                // queryString = queryString.Substring(queryString.IndexOf("?") + 1)
                queryString = Common.DecryptURLParameters(queryString);
                string[] values = queryString.Split("&");
                var id = values[0].Split("=")[1];
                var tableName = values[1].Split("=")[1];
                var AttachmentsDDLVal = attachmentNum; // AttachmentsDDL.SelectedItem.Value()
                int CheckOutVar = Smead.RecordsManagement.Imaging.Attachments.IsCheckedOutPublic(tableName, TrackingServices.ZeroPaddedString(id), AttachmentsDDLVal, passport.Connection());
                return CheckOutVar;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        #region "CHECKOUT\IN CONCEPT"
        private int CheckIfIsCheckOutToMe(int attachmentNum, string documentKey)
        {
            try
            {
                var queryString = documentKey;
                // queryString = Uri.UnescapeDataString(queryString)
                // 'queryString = queryString.Replace("%2f", "/")
                queryString = queryString.Substring(queryString.IndexOf("?") + 1);
                queryString = Common.DecryptURLParameters(queryString);

                string[] values = queryString.Split("&");

                var id = System.Net.WebUtility.UrlDecode(values[0].Split("=")[1]);
                var tableName = values[1].Split("=")[1];
                var Attachmentsnum = attachmentNum;
                int CheckOutVar = Smead.RecordsManagement.Imaging.Attachments.IsCheckedOutToMePublic(passport.UserId, tableName, TrackingServices.ZeroPaddedString(id), Attachmentsnum, passport.Connection());
                return CheckOutVar;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public JsonResult CheckBothIfcheckoutTomeAndcheckout(List<string> getVariables)
        {
            DocumentViewerModel ViewModel = new DocumentViewerModel();
            string documentKey = getVariables[0];
            int attachmentNum = Convert.ToInt32(getVariables[1]);
            try
            {
                ViewModel.isCheckout = CheckIfIsCheckOut(attachmentNum, documentKey);
                ViewModel.isCheckoutTome = CheckIfIsCheckOutToMe(attachmentNum, documentKey);
                ViewModel.isCheckoutDesktop = isCheckoutInDesktop(attachmentNum, documentKey);
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMsg = ex.Message + "couldn't get values";
            }

            return Json(ViewModel);
        }

        private int isCheckoutInDesktop(int attachmentNum, string documentKey)
        {
            var queryString = documentKey;
            queryString = queryString.Substring(queryString.IndexOf("?") + 1);
            queryString = Common.DecryptURLParameters(queryString);
            string[] values = queryString.Split("&");
            var id = values[0].Split("=")[1];
            var tableName = values[1].Split("=")[1];
            var Attachmentsnum = attachmentNum;
            int isCheckoutDesktop = 0;

            string sqlcmd = "select b.persistedcheckout from Userlinks a join Trackables b on a.TrackablesId = b.Id where IndexTable = @tableName and IndexTableId = @tableId and AttachmentNumber = @AttachmentNumber";
            using (SqlCommand cmd = new SqlCommand(sqlcmd, passport.Connection()))
            {
                cmd.Parameters.AddWithValue("@tableId", id);
                cmd.Parameters.AddWithValue("@tableName", tableName);
                cmd.Parameters.AddWithValue("@AttachmentNumber", Attachmentsnum);

                var adp = new SqlDataAdapter(cmd);
                var dTable = new DataTable();
                var datat = adp.Fill(dTable);
                if(dTable.Rows.Count == 0)
                {
                    isCheckoutDesktop = 0;
                }
                else
                {
                    isCheckoutDesktop = dTable.Rows[0][0].IntValue();
                }
                
            }

            return isCheckoutDesktop;
        }

        public void UndoCheckOut(List<string> getVariables)
        {
            try
            {
                var queryString = getVariables[0];
                queryString = Common.DecryptURLParameters(queryString);
                var values = queryString.Split("&");
                var oTableId = values[0].Split("=")[1];
                var oTableName = values[1].Split("=")[1];
                // Dim URLAttachment = values(2).Split("=")(1)
                var oAttachmentNumber = getVariables[1];
                var oVersionNumber = getVariables[2];

                var Server = ContextService.GetSessionValueByKey("Server", httpContext);
                var databaseName = ContextService.GetSessionValueByKey("DatabaseName", httpContext);

                Smead.RecordsManagement.Imaging.Attachment oAttachment = null/* TODO Change to default(_) if this is not a reference type */;
                var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", Server, databaseName), oTableName, oTableId).ToString();
                oAttachment = Smead.RecordsManagement.Imaging.Attachments.UndoCheckOut(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", Server, databaseName), oTableName, oTableId, oAttachmentNumber.IntValue(), oVersionNumber.IntValue(), false);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async void CheckOutAttachment(List<string> getVariables)
        {
            var call = new ApiCalls(_config);
            try
            {
                var queryString = getVariables[0];
                string filePath = getVariables[1];
                queryString = Common.DecryptURLParameters(queryString);
                string[] values = queryString.Split("&");
                var oTableName = values[1].Split("=")[1];
                var oTableId = values[0].Split("=")[1];


                var Server = ContextService.GetSessionValueByKey("Server", httpContext);
                var databaseName = ContextService.GetSessionValueByKey("DatabaseName", httpContext);

                var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", Server, databaseName), oTableName, oTableId).ToString();
                int AttachmentNumber = getVariables[2].IntValue();
                int VersionNumber = getVariables[3].IntValue();
                var PageNumber = getVariables[4];
                // Dim URLAttachment = values(2).Split("=")(1)
                var info = await call.DocumentServices.GetCodecInfoFromFile(filePath, Path.GetExtension(filePath)); //Common.GetCodecInfoFromFile(filePath, Path.GetExtension(filePath));
                if (info == null)
                {
                    // check raju
                    var attachment = Smead.RecordsManagement.Imaging.Attachments.CheckOut(ticket, passport.UserId, string.Format(@"{0}\{1}", Server, databaseName), oTableName, oTableId, AttachmentNumber, VersionNumber, string.Empty, Keys.GetClientIpAddress(httpContext), string.Empty, false, false);
                }
                else
                {
                    Smead.RecordsManagement.Imaging.Attachment attachment = Smead.RecordsManagement.Imaging.Attachments.CheckOut(ticket, passport.UserId, string.Format(@"{0}\{1}", Server, databaseName), oTableName, oTableId, AttachmentNumber, VersionNumber, string.Empty, Keys.GetClientIpAddress(httpContext), string.Empty, false, info.TotalPages);
                    // check raju
                    if ((attachment) is Smead.RecordsManagement.Imaging.ErrorAttachment)
                    {
                        if (((Smead.RecordsManagement.Imaging.ErrorAttachment)attachment).Message.ToLower().StartsWith("could not load file or assembly 'leadtools"))
                        {
                            if (info.TotalPages > 1)
                            {
                                Leadtools.Codecs.RasterCodecs codec = new Leadtools.Codecs.RasterCodecs();

                                for (int i = 1; i <= info.TotalPages; i++)
                                {
                                    using (RasterImage img = codec.Load(filePath, 0, CodecsLoadByteOrder.RgbOrGray, i, i))
                                    {
                                        codec.Save(img, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_" + i.ToString() + Path.GetExtension(filePath), img.OriginalFormat, img.BitsPerPixel);
                                    }
                                }
                            }
                        }
                    }
                    else if (info.TotalPages > 1)
                    {
                        Leadtools.Codecs.RasterCodecs codec = new Leadtools.Codecs.RasterCodecs();

                        for (int i = 1; i <= info.TotalPages; i++)
                        {
                            using (RasterImage img = codec.Load(filePath, 0, CodecsLoadByteOrder.RgbOrGray, i, i))
                            {
                                codec.Save(img, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_" + i.ToString() + Path.GetExtension(filePath), img.OriginalFormat, img.BitsPerPixel);
                            }
                        }
                    }
                }
            }


            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        #endregion

        #region "DOWNLOADAttachments"
        // check raju
        public IActionResult DownloadFiles(List<FileDownloads> @params, int viewid)
        {
            var viewName = Navigation.GetViewName(viewid, passport);
            if (passport.CheckPermission(viewName, Smead.Security.SecureObject.SecureObjectType.View, Smead.Security.Permissions.Permission.Export))
            {
                if (@params.Count == 0)
                {
                }
                FileDownloads obj = new FileDownloads();

                var contentPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath");
                var ServerPath = contentPath + @"\wwwroot\Downloads\";

                var filesCol = obj.GetFiles(@params, passport, ServerPath).ToList();

                // For Each info As AttachmentsFileInfo In filesCol
                // info.FilePath = IsfileCreatedInDesktopDownload(variables, info.FilePath)
                // Next
                httpContext.Response.Clear();
                //Response.Buffer = false;   // check raju
                Response.Headers.Add("content-disposition", "attachment;filename=Attachments.zip");
                Response.Headers.Add("Content-Type", "application/x-zip-compressed");
                // check raju
                //using (var ziparchive = new ZipArchive(new ZipStreamWrapper(httpContext.Response.OutputStream), ZipArchiveMode.Create))
                //{
                //    for (int i = 0; i <= filesCol.Count - 1; i++)
                //    {
                //        var entry = ziparchive.CreateEntry(filesCol[i].FileName, CompressionLevel.Optimal);
                //        using (var entryStream = entry.Open())
                //        {
                //            using (var fileStream = System.IO.File.OpenRead(filesCol[i].FilePath))
                //            {
                //                fileStream.CopyTo(entryStream);
                //            }
                //        }
                //    }
                //}
                //Response.End();
                // delete temp files
                foreach (var del in obj.deleteTempFile)
                {
                    try
                    {
                        System.IO.File.Delete(del);
                        //My.Computer.FileSystem.DeleteFile(del);  // check raju
                    }
                    catch (Exception ex)
                    {
                    }
                }
                return Ok();
            }
            else
                return Ok();
        }
        #endregion

        #region "CREAT TEMP FILE IN CACHE"
        private List<string> GetFilesPerAttachment(List<string> variables)
        {
            // if file created in desktop compose file and return cach temp file
            // otherwise show the file.
            // this method check if file has note and annotation as well.
            List<string> pathString = new List<string>();
            var AttachmentNumber = variables[1];
            var VarsionNumber = variables[2];
            string queryString = variables[0];
            string documentId = "";
            queryString = queryString.Substring(queryString.IndexOf("?") + 1);
            queryString = Common.DecryptURLParameters(queryString);

            string[] values = queryString.Split("&");

            var id = values[0].Split("=")[1];
            var tableName = values[1].Split("=")[1];
            var attachmentId = values[2].Split("=")[1];

            using (SqlCommand cmd = new SqlCommand("SP_RMS_GetFilesPaths", passport.Connection()))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@tableId", id);
                cmd.Parameters.AddWithValue("@tableName", tableName);
                cmd.Parameters.AddWithValue("@AttachmentNumber", AttachmentNumber);
                cmd.Parameters.AddWithValue("@RecordVersion", VarsionNumber);

                var adp = new SqlDataAdapter(cmd);
                var dTable = new DataTable();
                var datat = adp.Fill(dTable);
                foreach (DataRow row in dTable.Rows)
                {
                    var getpath = row["FullPath"].ToString();
                    // Dim pointerid = Convert.ToInt32(row("pointerId"))
                    pathString.Add(getpath);
                }
            }

            return pathString;
        }

        public async Task<JsonResult> IsfileCreatedInDesktop(List<string> variables)
        {
            var api = new ApiCalls(_config);
            string documentId = "";
            List<string> pathString = GetFilesPerAttachment(variables);
            try
            {
                if (pathString.Count > 1)
                {
                    documentId = await api.APIPOST($"{api.DocumentServices.SaveTempFileToCacheURL}", pathString);//SaveTempFileTocache(pathString);
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, api, 0, passport.DatabaseName);
            }
            return Json(new { documentid = documentId, pageCount = pathString.Count });
        }

        //private async Task<string> APIPOST(string url, object obj)
        //{

        //    string value = "";
        //    var content = JsonConvert.SerializeObject(obj);
        //    var reqContent = new StringContent(content, Encoding.UTF8, "application/json");
        //    var client = new HttpClient();
        //    var call = client.PostAsync($"{url}", reqContent);
        //    if (call.IsCompletedSuccessfully)
        //    {
        //        return await call.Result.Content.ReadAsStringAsync();
        //    }
        //    else
        //    {
        //        return await call.Result.Content.ReadAsStringAsync();
        //    }
        //}
        //private string SaveTempFileTocache(List<string> pathString)
        //{
            
        //    string documentId = null;
          
        //    var cache = Cache;

        //    // Dim today = DateTime.Now
        //    // Dim policies As New CacheItemPolicy
        //    // policies.AbsoluteExpiration = today.AddDays(2)

        //    // Create a New document 
        //    CreateDocumentOptions createOptions = new CreateDocumentOptions();
        //    createOptions.Cache = cache;
        //    createOptions.UseCache = true;
        //    //moti mashiah leadtools 
        //    using (Leadtools.Document.LEADDocument document = DocumentFactory.Create(createOptions))
        //    {
        //        document.Name = "TempDocument";
        //        document.AutoDeleteFromCache = false;
        //        document.AutoDisposeDocuments = true;

        //        foreach (var item in pathString)
        //        {
        //            LoadDocumentOptions loadOptions = new LoadDocumentOptions();
        //            loadOptions.Cache = cache;
        //            var child = DocumentFactory.LoadFromFile(item, loadOptions);
        //            child.AutoDeleteFromCache = false;
        //            child.AutoDisposeDocuments = true;
        //            child.SaveToCache();
        //            // loop through each file and build document
        //            foreach (var page in child.Pages)
        //                document.Pages.Add(page);
        //        }

        //        document.SaveToCache();


        //        documentId = document.DocumentId;
        //    }

        //    return documentId;
        //}
        #endregion

        #region "Add to attachment cart"

        [HttpPost]
        public JsonResult AddAttachmenToShoppingCart(string keydocument, List<string> filesList)
        {
            string msg = "true";
            DocumentViewerModel ViewModel = new DocumentViewerModel();
            List<string> buildAttachmetList = new List<string>();
            var userID = passport.UserId;
            string queryString = keydocument;
            queryString = queryString.Substring(queryString.IndexOf("?") + 1);
            queryString = Common.DecryptURLParameters(queryString);
            // first check if file exist in the attachment cart already.
            string command1 = "select * from s_AttachmentCart where UserID = @userID";
            using (SqlCommand cmd = new SqlCommand(command1, passport.Connection()))
            {
                cmd.Parameters.AddWithValue("@userID", userID);
                var adp = new SqlDataAdapter(cmd);
                var dTable = new DataTable();
                var datat = adp.Fill(dTable);
                foreach (DataRow row in dTable.Rows)
                    buildAttachmetList.Add(row["filePath"].ToString());
            }
            string[] values = queryString.Split("&");
            var Id = values[0].Split("=")[1];
            var tableName = values[1].Split("=")[1];
            var attachmentId = values[2].Split("=")[1];
            string record = string.Format("{0} {1}", tableName, Id);
            if (!Navigation.FieldIsAString(tableName, passport))
                record = string.Format("{0} {1}", tableName, Convert.ToInt32(Id));
            try
            {
                foreach (var F in filesList)
                {
                    var prop = F.Split(",");
                    var getfile = Common.DecryptURLParameters(prop[0]);
                    string filePath = System.Net.WebUtility.UrlDecode(getfile);
                    string fileName = prop[1];
                    int attachmentNum = Convert.ToInt32(prop[2]);
                    int versionNumber = Convert.ToInt32(prop[3]);
                    var checkIfExist = buildAttachmetList.Where(a => a == filePath).Count();
                    if (checkIfExist == 0)
                    {
                        string command = "insert into s_AttachmentCart([UserId],[Username], [Record], [filePath], [fileName], [attachmentNumber], [versionNumber]) values(@userID, 'userName', @record, @filePath, @fileName, @attachmentNum, @versionNumber)";
                        using (SqlCommand cmd = new SqlCommand(command, passport.Connection()))
                        {
                            cmd.Parameters.AddWithValue("@userID", userID);
                            cmd.Parameters.AddWithValue("@record", record);
                            cmd.Parameters.AddWithValue("@filePath", filePath);
                            cmd.Parameters.AddWithValue("@fileName", fileName);
                            cmd.Parameters.AddWithValue("@attachmentNum", attachmentNum);
                            cmd.Parameters.AddWithValue("@versionNumber", versionNumber);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return Json(msg);
        }
        private DocumentViewerModel GetAttachmentCartData()
        {
            var userID = passport.UserId;
            DocumentViewerModel ViewModel = new DocumentViewerModel();
            try
            {
                string command = "select * from s_AttachmentCart where UserID = @userID";
                using (SqlCommand cmd = new SqlCommand(command, passport.Connection()))
                {
                    cmd.Parameters.AddWithValue("@userID", userID);
                    var adp = new SqlDataAdapter(cmd);
                    var dTable = new DataTable();
                    var datat = adp.Fill(dTable);
                    foreach (DataRow row in dTable.Rows)
                        ViewModel.AttachmentCartList.Add(new AttachmentCart(row["Id"].IntValue(), row["UserId"].IntValue(), row["Record"].ToString(), row["filePath"].ToString(), row["fileName"].ToString(), row["attachmentNumber"].IntValue(), row["versionNumber"].IntValue()));
                    ViewModel.ErrorMsg = "false";
                    return ViewModel;
                }
            }
            catch (Exception ex)
            {
                ViewModel.ErrorMsg = ex.Message;
                return ViewModel;
            }
        }
        public JsonResult GetListOfAttachmentUI()
        {
            var getData = GetAttachmentCartData();
            return Json(getData);
        }
        public JsonResult RemoveAttachmentFromCart(List<string> ListId)
        {
            DocumentViewerModel getData = null/* TODO Change to default(_) if this is not a reference type */;
            try
            {
                foreach (var id in ListId)
                {
                    string command = "delete from s_AttachmentCart where Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(command, passport.Connection()))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                getData = GetAttachmentCartData();
                getData.ErrorMsg = "false";
            }
            catch (Exception ex)
            {
                getData.ErrorMsg = ex.Message;
            }
            return Json(getData);
        }

        #endregion

        public JsonResult OCRSearchIndataBase(List<string> variables)
        {
            var AttachNumber = Convert.ToInt32(variables[1]);
            var Vernumber = Convert.ToInt32(variables[2]);
            var Search = variables[3];
            var getListOfpages = new List<SLIndexer>();
            // get the encryption key
            try
            {
                string queryString = variables[0];
                queryString = queryString.Substring(queryString.IndexOf("?") + 1);
                queryString = Common.DecryptURLParameters(queryString);
                string[] values = queryString.Split("&");
                var id = Convert.ToString(values[0].Split("=")[1]);
                var tableName = values[1].Split("=")[1];

                var db = new TABFusionRMSContext(passport.ConnectionString);
                getListOfpages = db.SLIndexers.Where(a => a.IndexType == 4 || a.IndexType == 5 || a.IndexType == 6 && a.IndexTableName == tableName & a.IndexTableId == id & a.IndexData.Contains(Search) & a.AttachmentNumber == AttachNumber & a.RecordVersion == Vernumber).OrderBy(a => a.PageNumber).ToList();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

            return Json(getListOfpages);
        }

        //'Note: add delete 

        public JsonResult GetListofNotes(List<string> variables)
        {
            var pointerID = variables[0];
            var _annotations = new RepositoryVB.Repositories<Annotation>();

            var getList = _annotations.Where(a => a.TableId == pointerID).ToList();
            var lst = new List<AnnotationModel>();
            foreach (var an in getList)
            {
                var prop = new AnnotationModel();
                prop.Id = an.Id;
                prop.Annotation1 = an.Annotation1;
                prop.UserName = an.UserName;
                prop.NoteDateTime = Keys.get_ConvertCultureDate(an.NoteDateTime.ToString(), false.ToString(), httpContext);
                lst.Add(prop);
            }
            // String.Format(Languages.Translation("lblTrackingBy"), Keys.ConvertCultureDate(getList.NoteDateTime.ToString(), True))
            return Json(lst);
        }

        public JsonResult DeleteNote(List<string> variables)
        {
            var id = System.Convert.ToInt32(variables[0]);
            var pointerID = variables[1];
            var _annotations = new RepositoryVB.Repositories<Annotation>();
            var anodel = _annotations.Where(a => a.Id == id).FirstOrDefault();
            _annotations.Delete(anodel);
            var getList = _annotations.Where(a => a.TableId == pointerID).ToList();
            return Json(getList);
        }

        public JsonResult AddNewNote(List<string> variables)
        {
            var pointerID = variables[0];
            var username = variables[1];
            var text = variables[2];
            var recordType = variables[3];
            if (recordType == "5")
                recordType = "pcfilespointers";
            else
                recordType = "imagepointers";
            var _annotations = new RepositoryVB.Repositories<Annotation>();
            Annotation ant = new Annotation();
            ant.TableId = pointerID;
            ant.Table = recordType;
            ant.UserName = username;
            ant.DeskOf = ""; // DisplayName
            ant.Annotation1 = text;
            ant.NoteDateTime = DateTime.Now;//DateTime.Now.ToString("yyyy MMM dd hh:mm:ss"); // check raju
            _annotations.Add(ant);

            var getList = _annotations.Where(a => a.Id == ant.Id).ToList();
            return Json(getList);
        }

        public JsonResult EditNewNote(List<string> variables)
        {
            var id = Convert.ToInt32(variables[0]);
            var text = variables[1];
            var _annotations = new RepositoryVB.Repositories<Annotation>();
            var ano = _annotations.Where(a => a.Id == id).FirstOrDefault();
            ano.Annotation1 = text;
            _annotations.Update(ano);
            return Json("saved");
        }

        public JsonResult GenerateDate()
        {
            var TodayDate = Keys.get_ConvertCultureDate(DateTime.Now.ToString(), false.ToString(), httpContext);
            return Json(TodayDate);
        }

        public JsonResult deleteCookie()
        {
            //HttpCookie searchCookie = new HttpCookie("searchInput", "");
            //searchCookie.Expires = DateTime.Now.AddSeconds(2);
            //searchCookie.HttpOnly = false;
            //Response.Cookies.Add(searchCookie);

            ContextService.RemoveCookie("searchInput", httpContext);

            return Json("cookie successfuly expired!!");
        }
        // scanner functions
        [HttpPost]
        public async Task<JsonResult> SaveTempfileForScanning()
        {
            var lst = new FilesScane();
            try
            {
                for (int i = 0; i <= httpContext.Request.Form.Files.Count - 1; i++)
                {
                    var PostedFile = httpContext.Request.Form.Files[i];
                    if (PostedFile.Length > 0)
                    {
                        string FileName = System.Guid.NewGuid().ToString();
                        FileName = FileName + Path.GetExtension(PostedFile.FileName);

                        var contentPath = (string)AppDomain.CurrentDomain.GetData("ContentRootPath");
                        var ServerPath = contentPath + @"\wwwroot\ImportFiles\";

                        using (var stream = System.IO.File.Create(ServerPath + FileName))
                        {
                            await PostedFile.CopyToAsync(stream);
                        }

                        lst.fileServerPath.Add(ServerPath + FileName);
                        lst.fileEncryptPath.Add(Common.EncryptURLParameters(ServerPath + FileName));
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
            return Json(lst);
        }
        [HttpPost]
        public string DeleteTempfileForScanning(List<string> ListOftempFiles)
        {
            var filepaths = ListOftempFiles;
            string msg = "";
            try
            {
                for (int i = 0; i <= filepaths.Count - 1; i++)
                {
                    var path = Common.DecryptURLParameters(filepaths[i]);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                msg = "deleted successfuly";
                return msg;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return msg;
            }
        }

        // Check Download Permission
        [HttpPost]
        public JsonResult DownloadDocPermission(int viewid)
        {
            try
            {
                var viewName = Navigation.GetViewName(viewid, passport);
                if (passport.CheckPermission(viewName, Smead.Security.SecureObject.SecureObjectType.View, Smead.Security.Permissions.Permission.Export))
                {
                    Keys.ErrorMessage = "";
                    Keys.ErrorType = "s";
                }
                else
                {
                    Keys.ErrorMessage = "";
                    Keys.ErrorType = "w";
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

    }

    class AnnotationModel
    {
        public int Id { get; set; }
        public string Annotation1 { get; set; }
        public string DeskOf { get; set; }
        public string NewAnnotation { get; set; }
        public bool NewAnnotationComplete { get; set; }
        public string NoteDateTime { get; set; }
        public string Table { get; set; }
        public string TableId { get; set; }
        public string UserName { get; set; }
    }

    class FilesScane
    {
        public FilesScane()
        {
            fileEncryptPath = new List<string>();
            fileServerPath = new List<string>();
        }
        public List<string> fileEncryptPath { get; set; }
        public List<string> fileServerPath { get; set; }
    }

}
