using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using slimShared;
using Smead.RecordsManagement;
using Smead.RecordsManagement.Imaging;
using Smead.Security;
using System.Data;
//using Leadtools;
//using Leadtools.Codecs;
//using Leadtools.Drawing;
using TabFusionRMS.WebCS.FusionElevenModels;

namespace TabFusionRMS.WebCS.Controllers
{
    public class VaultController : BaseController
    {
        private IWebHostEnvironment _webHostEnvironment { get; set; }
        private readonly IConfiguration _config;
        public VaultController(IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _webHostEnvironment = webHostEnvironment;
            _config = config;
        }
        // GET: Vault
        public IActionResult Index()
        {
            var model = new OrphanModel(passport);
            try
            {
                if (httpContext.Session.GetString("VaultType") == default)
                {
                    httpContext.Session.SetString("VaultType", "Grid");
                }

                model = OrphanList("", httpContext.Session.GetString("VaultType")??"");

                model.GenerateMenuWithOutSubmenu();
                return View(model);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult OrphanPartial(string docdata, string ViewType)
        {
            var model = new OrphanModel(passport);
            try
            {
                model = OrphanList(docdata, ViewType);
                return PartialView("../Vault/_OrphanContent", model);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return PartialView("../Vault/_OrphanContent", model);
            }
        }


        private OrphanModel OrphanList(string docdata, string ViewType)
        {
            var model = new OrphanModel(passport);
            try
            {
                model.OrphanPerPageRecord();
                var flyoutModel = LoadAttachmentOrphanData(docdata, 1, model.DefaultPerPageRecord, "Orphans", "");
                var dt = Attachments.GetAllOrphansCount(passport, "");
                var TotalRecords = dt.Rows[0].ItemArray[0];
                model.SetValues(docdata, flyoutModel.Result);
                model.TotalRecords = Convert.ToInt32(TotalRecords);
                model.SelectedViewType = ViewType;
                model.DeletePermission = passport.CheckPermission(" Orphans ", Smead.Security.SecureObject.SecureObjectType.Orphans, Smead.Security.Permissions.Permission.Delete);
                return model;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return model;
            }
        }

        public PartialViewResult LoadFlyoutOrphansPartial(string docdata, bool isMVC, string ViewType, string Filter, int PageIndex)
        {
            httpContext.Session.SetString("VaultType", ViewType);
            var model = new OrphanModel(passport);
            model.OrphanPerPageRecord();
            var flyoutModel = LoadAttachmentOrphanData(docdata, PageIndex, model.DefaultPerPageRecord, "Orphans", Filter);
            if (ViewType.Equals("Grid"))
            {
                return PartialView("_FlyoutPartialOrphansGrid", flyoutModel.Result);
            }
            else
            {
                return PartialView("_FlyoutPartialOrphansList", flyoutModel.Result);
            }
        }

        [HttpPost]
        public JsonResult LinkOrphanAttachment(string TrackableIds, string TableName, string RecordId)
        {
            var model = new VaultCommonModel();
            try
            {
                if (Attachments.CheckOrphanDeleted(TrackableIds, passport.DatabaseName))
                {
                    model.Status = false;
                    model.IsAlreadyDeleted = true;
                    //return Json(model, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet);
                    return Json(model);
                }
                else
                {
                    var res = default(bool);

                    foreach (string Id in TrackableIds.Split(","))
                    {
                        int TrackableId = Convert.ToInt32(Id);
                        res = Smead.RecordsManagement.Imaging.Attachments.LinkOrphanAttachmnet(TrackableId, TableName, Convert.ToInt32(RecordId), passport.DatabaseName, passport.UserId);
                    }
                    if (res)
                    {
                        model.Status = true;
                        model.IsAlreadyDeleted = false;
                        //return Json(model, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet);
                        return Json(model);
                    }
                    else
                    {
                        //return Json(model, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet);
                        return Json(model);
                    }
                }
            }
            catch (Exception ex)
            {
                model.Status = false;
                model.IsAlreadyDeleted = false;
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                //return Json(model, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet);
                return Json(model);
            }
        }
        public async Task<FlyoutModel> LoadAttachmentOrphanData(string docdata, int PageIndex, int PageSize, string viewName, string filter)
        {
            var call = new ApiCalls(_config);
            var flyoutModel = new FlyoutModel();
            flyoutModel.filter = filter;
            try
            {
                var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", passport.ServerName, passport.DatabaseName), Smead.RecordsManagement.Imaging.Attachments.OrphanName, "").ToString();

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

                foreach (DataRow oDataRow in orphanList.Rows)
                {
                    bool isFormatValid = true;
                    string filePath = Path.Combine(Path.GetDirectoryName(oDataRow["FullPath"].ToString()), CachedFlyouts, string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(oDataRow["FullPath"].ToString()), Smead.RecordsManagement.Imaging.Export.Output.Format.Jpg.ToString().ToLower()));
                    bool validAttachment = System.IO.File.Exists(filePath);
                    if (!validAttachment)
                        fullPath = oDataRow["FullPath"].ToString();
                    var flyOutDetails = new FlyOutDetails();

                    try
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }
                    catch (Exception ex)
                    {
                        DataErrorHandler.ErrorHandler(ex, flyoutModel, 0, passport.DatabaseName);
                        SlimShared.logInformation(string.Format("Info {0} in CommonController.LoadAttachmentData", ex.Message));
                    }

                    flyOutDetails.attchVersion = Convert.ToInt32(oDataRow["TrackablesRecordVersion"].ToString());
                    flyOutDetails.recordType = Convert.ToInt32(oDataRow["RecordType"].ToString());
                    flyOutDetails.sOrgFilePath = Common.EncryptURLParameters(System.Net.WebUtility.UrlDecode(oDataRow["FullPath"].Text()));

                    // written for handling invalid format exception usually happening with kinda of xlsx files
                    FileStreamResult fileStreamResult = default;
                    try
                    {
                        fileStreamResult = await call.DocumentServices.APIGETStreamFlyOutFirstPage(filePath, fullPath, validAttachment);//CommonFunctions.GetSubImageFlyOut(filePath, fullPath, validAttachment);
                    }
                    catch (Exception ex)
                    {
                        // If "Invalid file format".ToLower() = ex.Message.ToLower() Then
                        // Dim fileReplace = HttpContext.Server.MapPath("~/resources/images/InvalidFormat.PNG")
                        // fileStreamResult = GetSubImageFlyOut(fileReplace, fileReplace, validAttachment)
                        // isFormatValid = False
                        // End If
                        try
                        {
                            DataErrorHandler.ErrorHandler(ex, flyoutModel, 0, passport.DatabaseName);
                            var fileReplace = _webHostEnvironment.ContentRootPath + "resources\\images\\InvalidFormat.PNG";
                            fileStreamResult = await call.DocumentServices.APIGETStreamFlyOutFirstPage(filePath, fullPath, validAttachment);//CommonFunctions.GetSubImageFlyOut(fileReplace, fileReplace, validAttachment);
                            isFormatValid = false;
                        }
                        catch(Exception exx){}
                        
                    }

                    var buffer = new byte[(int)(0)];

                    if (fileStreamResult != null)
                    {
                        long filesize = fileStreamResult.FileStream.Length;
                        buffer = new byte[(int)(filesize + 1)];
                        fileStreamResult.FileStream.Read(buffer, 0, Convert.ToInt32(filesize));
                    }

                    flyOutDetails.sAttachmentName = string.IsNullOrEmpty(oDataRow["OrgFileName"].Text())? Path.GetFileName(oDataRow["orgfullpath"].Text()): oDataRow["OrgFileName"].ToString();
                    flyOutDetails.sFlyoutImages = buffer;
                    flyOutDetails.sAttachId = Convert.ToInt32(oDataRow["TrackablesId"].ToString());
                    // flyOutDetails.scanDateTime = IIf(String.IsNullOrEmpty(oDataRow("ScanDateTime")), "-", oDataRow("ScanDateTime").ToString())
                    flyOutDetails.scanDateTime = oDataRow["ScanDateTime"].Text() == ""? String.Empty: oDataRow["ScanDateTime"].Text();

                    var encryptURL = Common.EncryptURLParameters("pointerId=" + Convert.ToString(oDataRow["PointerId"]) + "&Id=" + Convert.ToString(oDataRow["TrackablesId"]) + "&Rtype=" + Convert.ToString(oDataRow["RecordType"]) + "&itempath=" + Convert.ToString(oDataRow["FullPath"] + "&itemname=" + Convert.ToString(oDataRow["OrgFileName"])));

                    flyOutDetails.sViewerLink = "/DocumentViewer/Orphan?documentKey=" + encryptURL;
                    flyOutDetails.downloadEncryptAttachment = "";
                    flyOutDetails.sRowNum = Convert.ToInt32(oDataRow["RowNum"].ToString());

                    // security fix for view permission in voliumn level
                    lstFlyOutDetails.Add(flyOutDetails);
                    // Dim VolumName = Convert.ToString(oDataRow("VolumnName"))
                    // If BaseWebPage.Passport.CheckPermission(VolumName, Smead.Security.SecureObject.SecureObjectType.Volumes, Smead.Security.Permissions.Permission.View) Then
                    // lstFlyOutDetails.Add(flyOutDetails)
                    // End If

                }
                flyoutModel.stringQuery = "";
                flyoutModel.FlyOutDetails = lstFlyOutDetails;
                flyoutModel.downloadPermission = true;
                // If BaseWebPage.Passport.CheckPermission(viewName, Smead.Security.SecureObject.SecureObjectType.View, Smead.Security.Permissions.Permission.Export) Then
                // flyoutModel.downloadPermission = True
                // Else
                // flyoutModel.downloadPermission = False
                // End If


                return flyoutModel;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, flyoutModel, 0, passport.DatabaseName);
                SlimShared.logError(string.Format("Error {0} in CommonController.LoadAttachmentOrphanData", ex.Message));
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult DeleteAttachments(string TrackableIds)
        {
            var model = new VaultCommonModel();
            try
            {
                if (passport.CheckPermission(" Orphans ", Smead.Security.SecureObject.SecureObjectType.Orphans, Smead.Security.Permissions.Permission.Delete) == false)
                {
                    model.Status = false;
                    model.IsAlreadyMaped = false;
                    model.IsDeletePermision = false;
                    //return Json(model, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet);
                    return Json(model);
                }

                if (Attachments.CheckOrphanAvailable(TrackableIds, passport.DatabaseName))
                {
                    // Return Json(New With {.Status = False, .Error = False, .IsAlreadyMaped = True}, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet)
                    model.Status = false;
                    model.IsAlreadyMaped = true;
                    //return Json(model, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet);
                    return Json(model);
                }
                else
                {
                    var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", passport.ServerName, passport.DatabaseName), Attachments.OrphanName, "").ToString();

                    foreach (string item in TrackableIds.Split(","))
                    {
                        int tId = Convert.ToInt32(item);
                        Attachments.DeleteOrphan(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, passport.DatabaseName, tId);
                    }
                    model.Status = true;
                    model.IsAlreadyMaped = false;
                    //return Json(model, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet);
                    return Json(model);
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                SlimShared.logError(string.Format("Error {0} in CommonController.DeleteAttachments", ex.Message));
                throw ex;
            }

        }

        [HttpPost]
        public JsonResult CheckDownloadable(string TrackableIds)
        {
            var model = new VaultCommonModel();
            model.Status = true;
            try
            {
                if (Attachments.CheckOrphanAvailable(TrackableIds, passport.DatabaseName))
                {
                    model.IsAlreadyMaped = true;
                    //return Json(model, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet);
                    return Json(model);
                }
                else
                {
                    model.IsAlreadyMaped = false;
                    //return Json(model, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet);
                    return Json(model);
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                SlimShared.logError(string.Format("Error {0} in VaultController.CheckDownloadable", ex.Message));
                throw ex;
            }

        }

        [HttpPost]
        public JsonResult GetCountAfterFilter(string filter)
        {
            var model = new VaultCommonModel();
            model.TotalRecords = 0;
            try
            {
                var model1 = new OrphanModel(passport);
                model1.OrphanPerPageRecord();
                var dt = Attachments.GetAllOrphansCount(passport, filter);
                model.TotalRecords = Convert.ToInt32(dt.Rows[0].ItemArray[0]);
                model.PerPageRecord = model1.DefaultPerPageRecord;
                //return Json(model, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet);
                return Json(model);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                //return Json(model, "application/json; charset=utf-8", JsonRequestBehavior.AllowGet);
                return Json(model);
            }
        }

    }

    public partial class VaultCommonModel : BaseModel
    {
        public bool Status { get; set; }
        public bool IsAlreadyMaped { get; set; }
        public bool IsAlreadyDeleted { get; set; }
        public bool IsDeletePermision { get; set; } = true;
        public int TotalRecords { get; set; }
        public int PerPageRecord { get; set; }
    }
}
