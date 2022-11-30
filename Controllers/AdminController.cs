using System.Collections;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Smead.RecordsManagement;
using Smead.Security;
using TabFusionRMS.DataBaseManagerVB;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.Resource;
using Attribute = TabFusionRMS.Models.Attribute;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Resources;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TabFusionRMS.WebCS.Controllers
{
    //<AuthenticationFilter()> _
    public class AdminController : BaseController
    {

        private IDBManager _IDBManager = new DBManager();
       
        // <SkipMyGlobalActionFilter> _
        public IActionResult Index()
        {
            if (ContextService.GetSessionValueByKey("fileName", httpContext) is not null)
            {
                return RedirectToAction("Index", "Upload");
            }
            // Keys.iAdminRefId = Keys.GetUserId
            ContextService.SetSessionValue("iAdminRefId", passport.UserId.ToString(), httpContext);
            return View();
        }

        public IActionResult BindAccordian()
        {
            // Added by Ganesh for Security fix.
            IRepository<vwTablesAll> _ivwTablesAll = new Repositories<vwTablesAll>();
            var pTablesEntities = _ivwTablesAll.All().OrderByField("UserName", true);
            var lstDataTbl = new Dictionary<string, string>();

            foreach (vwTablesAll Table in pTablesEntities)
            {
                if (CollectionsClass.IsEngineTable(Table.TABLE_NAME))
                {
                    Table.UserName = Table.UserName + "*";
                    lstDataTbl.Add(Table.TABLE_NAME, Table.UserName);
                }
                else if (passport.CheckPermission(Table.TABLE_NAME, Smead.Security.SecureObject.SecureObjectType.Table, Permissions.Permission.View))
                {
                    Table.UserName = Table.UserName;
                    lstDataTbl.Add(Table.TABLE_NAME, Table.UserName);
                }
            }

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.None;
            string jsonObject = JsonConvert.SerializeObject(lstDataTbl, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        //#region Application

        #region Email Notification
        public IActionResult LoadEmailNotificationView()
        {
            return PartialView("_EmailNotificationPartial");
        }

        public IActionResult EmailSettingPartialView()
        {
            return PartialView("_EmailSettingPartial");
        }

        private enum EmailType
        {
            etDelivery = 0x1,
            etWaitList = 0x2,
            etException = 0x4,
            etCheckedOut = 0x8,
            etRequest = 0x10,
            etPastDue = 0x20,
            etSimple = 0x40,
            etBackground = 0x80
        }

        [HttpPost]
        public IActionResult SetEmailDetails(TabFusionRMS.Models.System systemEmail, bool pEMailDeliveryEnabled, bool pEMailWaitListEnabled, bool pEMailExceptionEnabled, bool pEMailBackgroundEnabled, bool pSMTPAuthentication)
        {
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                EmailType eNotificationEnabled = default;
                var pSystemEntity = _iSystem.All().OrderBy(x => x.Id).FirstOrDefault();
                pSystemEntity.EMailDeliveryEnabled = pEMailDeliveryEnabled;
                pSystemEntity.EMailWaitListEnabled = pEMailWaitListEnabled;
                pSystemEntity.EMailExceptionEnabled = pEMailExceptionEnabled;

                if (pEMailDeliveryEnabled)
                    eNotificationEnabled = (EmailType)(eNotificationEnabled += (int)EmailType.etDelivery);
                if (pEMailWaitListEnabled)
                    eNotificationEnabled = (EmailType)(eNotificationEnabled + (int)EmailType.etWaitList);
                if (pEMailExceptionEnabled)
                    eNotificationEnabled = (EmailType)(eNotificationEnabled + (int)EmailType.etException);
                if (pEMailBackgroundEnabled)
                    eNotificationEnabled = (EmailType)(eNotificationEnabled + (int)EmailType.etBackground);
                pSystemEntity.NotificationEnabled = Convert.ToInt32(eNotificationEnabled);

                pSystemEntity.SMTPServer = systemEmail.SMTPServer;
                if (systemEmail.SMTPPort is not null)
                {
                    pSystemEntity.SMTPPort = systemEmail.SMTPPort;
                }
                else
                {
                    pSystemEntity.SMTPPort = 25;
                }
                if (systemEmail.EMailConfirmationType is not null)
                {
                    pSystemEntity.EMailConfirmationType = systemEmail.EMailConfirmationType;
                }
                else
                {
                    pSystemEntity.EMailConfirmationType = 0;
                }
                if (systemEmail.SMTPUserAddress is null | systemEmail.SMTPUserPassword is null)
                {
                    pSystemEntity.SMTPUserPassword = pSystemEntity.SMTPUserPassword;
                    pSystemEntity.SMTPUserAddress = pSystemEntity.SMTPUserAddress;
                }
                else
                {
                    pSystemEntity.SMTPUserAddress = systemEmail.SMTPUserAddress;
                    string encrypted = Tables.GenerateKey(Convert.ToBoolean(1), systemEmail.SMTPUserPassword, null);
                    pSystemEntity.SMTPUserPassword = encrypted;
                }
                pSystemEntity.SMTPAuthentication = pSMTPAuthentication;
                _iSystem.Update(pSystemEntity);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRecUpdatedSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public IActionResult GetSMTPDetails(bool flagSMTP = false)
        {
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                var pSystemEntity = _iSystem.All().OrderBy(x => x.Id).FirstOrDefault();
                if (flagSMTP)
                {
                    if (pSystemEntity.SMTPUserPassword is not null)
                    {
                        var byteArray = Encoding.Default.GetBytes(pSystemEntity.SMTPUserPassword);
                        string encrypted = Tables.GenerateKey(Convert.ToBoolean(0), null, byteArray);
                        pSystemEntity.SMTPUserPassword = encrypted;
                    }
                }
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string jsonObject = JsonConvert.SerializeObject(pSystemEntity, Newtonsoft.Json.Formatting.Indented, Setting);
                return Json(jsonObject);
            }
            catch (Exception)
            {
                return Json(new
                {
                    errortype = "e",
                    message = Keys.ErrorMessageJS()
                });
            }
        }

        #endregion

        #region Requestor
        public IActionResult LoadRequestorView()
        {
            return PartialView("_RequestorPartial");
        }

        public IActionResult RemoveRequestorEntity(string statusVar)
        {
            IRepository<SLRequestor> _iSLRequestor = new Repositories<SLRequestor>();
            try
            {
                var pSLRequestorEntity = _iSLRequestor.All().Where(m => m.Status.Trim().ToLower().Equals(statusVar.Trim().ToLower()));
                if (pSLRequestorEntity.Count() != 0)
                {
                    _iSLRequestor.DeleteRange(pSLRequestorEntity);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Keys.DeleteSuccessMessage();
                }
                else
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlNoData2Purge");
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public IActionResult ResetRequestorLabel(string tableName)
        {
            IRepository<OneStripJob> _iOneStripJob = new Repositories<OneStripJob>();
            try
            {
                _iOneStripJob.BeginTransaction();
                var pOneStripJob = _iOneStripJob.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                if (pOneStripJob is null)
                {
                    var rStripJob = new OneStripJob();
                    rStripJob.Name = "Requestor Default Label";
                    rStripJob.Inprint = (short?)0;
                    rStripJob.TableName = "SLRequestor";
                    rStripJob.OneStripFormsId = 101;
                    rStripJob.UserUnits = (short?)0;
                    rStripJob.LabelWidth = 5040;
                    rStripJob.LabelHeight = 1620;
                    rStripJob.DrawLabels = false;
                    rStripJob.LastCounter = 0;
                    rStripJob.SQLString = "SELECT * FROM [SLRequestor] WHERE [Id] = %ID%";
                    rStripJob.SQLUpdateString = "";
                    rStripJob.LSAfterPrinting = "";
                    _iOneStripJob.Add(rStripJob);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRecAddedSuccessfully");
                }
                else
                {
                    pOneStripJob.Name = "Requestor Default Label";
                    pOneStripJob.Inprint = (short?)0;
                    pOneStripJob.TableName = "SLRequestor";
                    pOneStripJob.OneStripFormsId = 101;
                    pOneStripJob.UserUnits = (short?)0;
                    pOneStripJob.LabelWidth = 5040;
                    pOneStripJob.LabelHeight = 1620;
                    pOneStripJob.DrawLabels = false;
                    pOneStripJob.LastCounter = 0;
                    pOneStripJob.SQLString = "SELECT * FROM [SLRequestor] WHERE [Id] = %ID%";
                    pOneStripJob.SQLUpdateString = "";
                    pOneStripJob.LSAfterPrinting = "";
                    _iOneStripJob.Update(pOneStripJob);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRecUpdatedSuccessfully");
                }

                var pOneStripJobId = _iOneStripJob.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                if (pOneStripJob is null)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlNoRec4DRL");
                }
                else
                {
                    object pDatabaseEntity = null;
                    _IDBManager.ConnectionString = Keys.get_GetDBConnectionString((Databas)pDatabaseEntity);
                    _IDBManager.CreateParameters(1);
                    _IDBManager.AddParameters(0, "@JobsId", pOneStripJobId.Id);
                    int loutput = _IDBManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_RMS_AddRequestorJobFields");
                    _IDBManager.Dispose();
                }
                _iOneStripJob.CommitTransaction();
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });

        }

        // //[HttpPost]
        public IActionResult SetRequestorSystemEntity(int pConfirmType, int pPrintMethod, int pIdType, bool pAllowList, bool pPopupList, int pPrintCopies, int pPrintInterval)
        {
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                var pSystemEntity = _iSystem.All().OrderBy(x => x.Id).FirstOrDefault();
                pSystemEntity.RequestConfirmation = pConfirmType;
                pSystemEntity.ReqAutoPrintMethod = pPrintMethod;
                pSystemEntity.ReqAutoPrintIDType = pIdType;
                pSystemEntity.AllowWaitList = pAllowList;
                pSystemEntity.PopupWaitList = pPopupList;
                pSystemEntity.ReqAutoPrintCopies = pPrintCopies;
                pSystemEntity.ReqAutoPrintInterval = pPrintInterval;
                _iSystem.Update(pSystemEntity);
                Keys.ErrorMessage = Keys.SaveSuccessMessage();
                Keys.ErrorType = "s";
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public IActionResult GetRequestorSystemEntity()
        {
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                var pSystemEntity = _iSystem.All().OrderBy(x => x.Id).FirstOrDefault();
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string jsonObject = JsonConvert.SerializeObject(pSystemEntity, Newtonsoft.Json.Formatting.Indented, Setting);
                return Json(jsonObject);
            }
            catch (Exception)
            {
                return Json(new
                {
                    errortype = "e",
                    message = Keys.ErrorMessageJS()
                });
            }

        }
        #endregion

        #region Retention

        // GET the view for Admin control of Retention.
        public IActionResult AdminRetentionPartial()
        {
            return PartialView("_RetentionPartial");
        }

        // Load view for retention properties.
        public IActionResult LoadRetentionPropView()
        {
            return PartialView("_RetentionPropertiesPartial");
        }

        public IActionResult GetRetentionPeriodTablesList()
        {

            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<SLServiceTask> _iSLServiceTasks = new Repositories<SLServiceTask>();
                var lTableEntites = from t in _iTable.All().OrderBy(m => m.TableName)
                                    select new { t.TableName, t.UserName, t.RetentionPeriodActive, t.RetentionInactivityActive };

                var lSystem = from x in _iSystem.All()
                              select new { x.RetentionTurnOffCitations, x.RetentionYearEnd };

                var lSLServiceTasks = from y in _iSLServiceTasks.All()
                                      select new { y.Type, y.Interval };

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string jsonObject = JsonConvert.SerializeObject(lTableEntites, Newtonsoft.Json.Formatting.Indented, Setting);

                string systemJsonObject = JsonConvert.SerializeObject(lSystem, Newtonsoft.Json.Formatting.Indented, Setting);
                string serviceJsonObject = JsonConvert.SerializeObject(lSLServiceTasks, Newtonsoft.Json.Formatting.Indented, Setting);

                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage,
                    ltablelist = jsonObject,
                    lsystemlist = systemJsonObject,
                    lservicelist = serviceJsonObject
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, errortype = "e", message = Keys.ErrorMessageJS() });
            }

        }

        //         [HttpPost]
        public IActionResult RemoveRetentionTableFromList(Array pTableIds)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                foreach (string item in pTableIds)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        int pTableId = Convert.ToInt32(item);
                        var pTableEntity = _iTable.All().Where(x => x.TableId.Equals(pTableId)).FirstOrDefault();
                        pTableEntity.RetentionPeriodActive = false;
                        pTableEntity.RetentionInactivityActive = false;

                        _iTable.Update(pTableEntity);
                    }
                }
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlTblMoveSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        //         [HttpPost]
        public IActionResult SetRetentionTblPropData(int pTableId, bool pInActivity, int pAssignment, int pDisposition, string pDefaultRetentionId, string pRelatedTable, string pRetentionCode, string pDateOpened, string pDateClosed, string pDateCreated, string pOtherDate)
        {

            string msgVerifyRetDisposition = "";
            string sSQL = "";
            ADODB.Connection sADOConn;
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<View> _iView = new Repositories<View>();
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                var pTableEntites = _iTable.All().Where(x => x.TableId.Equals(pTableId)).FirstOrDefault();
                var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableEntites.TableName.Trim().ToLower())).FirstOrDefault();
                sADOConn = DataServices.DBOpen(oTables, _iDatabas.All());

                var oViews = _iView.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableEntites.TableName.Trim().ToLower())).FirstOrDefault();

                object pDatabaseEntity = null;
                if (pTableEntites is not null)
                {
                    if (pTableEntites.DBName is not null)
                    {
                        pDatabaseEntity = _iDatabas.Where(x => x.DBName.Trim().ToLower().Equals(pTableEntites.DBName.Trim().ToLower())).FirstOrDefault();
                    }
                }
                // _IDBManager.ConnectionString = Keys.GetDBConnectionString(pDatabaseEntity)

                if (pDisposition != 0 || pInActivity)
                {
                    pTableEntites.RetentionAssignmentMethod = pAssignment;
                    pTableEntites.DefaultRetentionId = pDefaultRetentionId;
                    pTableEntites.RetentionRelatedTable = pRelatedTable;
                }

                pTableEntites.RetentionPeriodActive = pDisposition != 0;
                pTableEntites.RetentionInactivityActive = pInActivity;
                pTableEntites.RetentionFinalDisposition = pDisposition;

                // Field Defination fields are allowed to save when None Disposition is selected.
                if (!string.IsNullOrEmpty(pRetentionCode))
                {
                    if (pRetentionCode.Substring(0, 1) == "*")
                    {
                        Tables.SaveNewFieldToTable(pTableEntites.TableName, pRetentionCode.Substring(1).Trim(), Enums.DataTypeEnum.rmVarWChar, (Databas)pDatabaseEntity, oViews.Id, passport);

                        pTableEntites.RetentionFieldName = pRetentionCode.Substring(1).Trim();
                    }
                    else
                    {
                        pTableEntites.RetentionFieldName = pRetentionCode;
                    }
                }

                if (!string.IsNullOrEmpty(pDateOpened))
                {
                    if (pDateOpened.Substring(0, 1) == "*")
                    {
                        Tables.SaveNewFieldToTable(pTableEntites.TableName, pDateOpened.Substring(1).Trim(), Enums.DataTypeEnum.rmDate, (Databas)pDatabaseEntity, oViews.Id, passport);

                        pTableEntites.RetentionDateOpenedField = pDateOpened.Substring(1).Trim();
                    }
                    else
                    {
                        pTableEntites.RetentionDateOpenedField = pDateOpened;
                    }
                }

                if (!string.IsNullOrEmpty(pDateClosed))
                {
                    if (pDateClosed.Substring(0, 1) == "*")
                    {
                        Tables.SaveNewFieldToTable(pTableEntites.TableName, pDateClosed.Substring(1).Trim(), Enums.DataTypeEnum.rmDate, (Databas)pDatabaseEntity, oViews.Id, passport);

                        pTableEntites.RetentionDateClosedField = pDateClosed.Substring(1).Trim();
                    }
                    else
                    {
                        pTableEntites.RetentionDateClosedField = pDateClosed;
                    }
                }

                if (!string.IsNullOrEmpty(pDateCreated))
                {
                    if (pDateCreated.Substring(0, 1) == "*")
                    {
                        Tables.SaveNewFieldToTable(pTableEntites.TableName, pDateCreated.Substring(1).Trim(), Enums.DataTypeEnum.rmDate, (Databas)pDatabaseEntity, oViews.Id, passport);

                        pTableEntites.RetentionDateCreateField = pDateCreated.Substring(1).Trim();
                    }
                    else
                    {
                        pTableEntites.RetentionDateCreateField = pDateCreated;
                    }
                }

                if (!string.IsNullOrEmpty(pOtherDate))
                {
                    if (pOtherDate.Substring(0, 1) == "*")
                    {
                        Tables.SaveNewFieldToTable(pTableEntites.TableName, pOtherDate.Substring(1).Trim(), Enums.DataTypeEnum.rmDate, (Databas)pDatabaseEntity, oViews.Id, passport);

                        pTableEntites.RetentionDateOtherField = pOtherDate.Substring(1).Trim();
                    }
                    else
                    {
                        pTableEntites.RetentionDateOtherField = pOtherDate;
                    }
                }

                _iTable.Update(pTableEntites);

                msgVerifyRetDisposition = VerifyRetentionDispositionTypesForParentAndChildren(pTableEntites.TableId);

                // Add the Retention Status fields
                sSQL = "ALTER TABLE [" + pTableEntites.TableName + "]";
                sSQL = sSQL + " ADD [%slRetentionInactive] BIT DEFAULT 0";
                DataServices.ProcessADOCommand(ref sSQL, sADOConn, false);
                sSQL = "";

                sSQL = "ALTER TABLE [" + pTableEntites.TableName + "]";
                sSQL = sSQL + " ADD [%slRetentionInactiveFinal] BIT DEFAULT 0";
                DataServices.ProcessADOCommand(ref sSQL, sADOConn, false);
                sSQL = "";

                // Add the Retention Disposition Status field
                sSQL = "ALTER TABLE [" + pTableEntites.TableName + "]";
                sSQL = sSQL + " ADD [%slRetentionDispositionStatus] INT DEFAULT 0";
                DataServices.ProcessADOCommand(ref sSQL, sADOConn, false);
                sSQL = "";

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Keys.SaveSuccessMessage();
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                msgVerifyRetDisposition,
                message = Keys.ErrorMessage
            });

        }

        public string VerifyRetentionDispositionTypesForParentAndChildren(int pTableId)
        {
            Table oTable;

            string sMessage = string.Empty;
            IRepository<Table> _iTable = new Repositories<Table>();
            var pTableEntites = _iTable.All().Where(x => x.TableId.Equals(pTableId)).FirstOrDefault();
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            // Dim lstRelatedTables = _iRelationShip.All().Where(Function(x) x.UpperTableName = pTableEntites.TableName).ToList()
            var lstRelatedTables = _iRelationShip.All().Where(x => (x.LowerTableName) == (pTableEntites.TableName)).ToList();
            var lstRelatedChildTable = _iRelationShip.All().Where(x => (x.UpperTableName) == (pTableEntites.TableName)).ToList();

            try
            {
                if (pTableEntites.RetentionFinalDisposition != 0)
                {

                    foreach (var lTableName in lstRelatedTables)
                    {

                        oTable = _iTable.All().Where(x => x.TableName.Equals(lTableName.UpperTableName)).FirstOrDefault();

                        if (oTable is not null)
                        {
                            if (((oTable.RetentionPeriodActive == true) || (oTable.RetentionInactivityActive == true)) && (oTable.RetentionFinalDisposition != 0))
                            {
                                if (oTable.RetentionFinalDisposition != pTableEntites.RetentionFinalDisposition)
                                    sMessage = Constants.vbTab + Constants.vbTab + oTable.UserName + Constants.vbCrLf;
                            }
                            oTable = null;
                        }

                    }

                    foreach (var lTableName in lstRelatedChildTable)
                    {

                        oTable = _iTable.All().Where(x => x.TableName.Equals(lTableName.LowerTableName)).FirstOrDefault();

                        if (oTable is not null)
                        {
                            if (((oTable.RetentionPeriodActive == true) || (oTable.RetentionInactivityActive == true)) && (oTable.RetentionFinalDisposition != 0))
                            {
                                if ((oTable.RetentionFinalDisposition != pTableEntites.RetentionFinalDisposition))
                                    sMessage = Constants.vbTab + Constants.vbTab + oTable.UserName + Constants.vbCrLf;
                            }
                            oTable = null;
                        }

                    }

                    if (Operators.CompareString(sMessage, "", false) > 0)
                    {
                        // sMessage = "<b>WARNING:</b>  The following related tables have a retention disposition " & vbCrLf & _
                        // "set differently than this table:</br></br>" & _
                        // sMessage & vbCrLf & _
                        // "</br></br>This could give different results than expected." & vbCrLf & _
                        // "</br></br>Please correct the appropriate table if this is not what is intended."
                        sMessage = string.Format(Languages.get_Translation("msgAdminCtrlWarningL1"), Constants.vbNewLine, sMessage);
                    }
                }
            }
            catch (Exception)
            {
                sMessage = string.Empty;
            }

            return sMessage;
        }

        public IActionResult GetRetentionPropertiesData(int pTableId)
        {

            var lstRetCodeFields = new List<string>();
            var lstDateFields = new List<string>();
            var lstRelatedTable = new List<string>();
            var bFootNote = default(bool);
            string lstRetentionCode = "";
            string lstDateClosed = "";
            string lstDateCreated = "";
            string lstDateOpened = "";
            string lstDateOther = "";
            ADODB.Connection conObj;
            // var BaseWebPage = new BaseWebPage();
            bool bTrackable = false;
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            IRepository<SLRetentionCode> _iSLRetentionCode = new Repositories<SLRetentionCode>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            if (pTableId == -1)
            {
                return Json(new { success = false, errortype = "e", message = Keys.ErrorMessageJS() });
            }
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                var pTableEntites = _iTable.All().Where(x => x.TableId.Equals(pTableId)).FirstOrDefault();
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Keys.SaveSuccessMessage();

                bTrackable = passport.CheckPermission(pTableEntites.TableName.Trim(), (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Transfer);

                if (pTableEntites is null)
                {
                    return Json(new { success = false, errortype = "e", message = Languages.get_Translation("msgAdminCtrlRecordNotFound") });
                }

                // Dim conObj As ADODB.Connection = DataServices.DBOpen(Enums.eConnectionType.conDefault)
                var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableEntites.TableName.Trim().ToLower())).FirstOrDefault();
                conObj = DataServices.DBOpen(oTables, _iDatabas.All());

                var dbRecordSet = SchemaInfoDetails.GetTableSchemaInfo(pTableEntites.TableName, conObj);

                if (!dbRecordSet.Exists(x => x.ColumnName == "RetentionCodesId"))
                {
                    lstRetentionCode = "* RetentionCodesId";
                    bFootNote = true;
                }

                if (!dbRecordSet.Exists(x => x.ColumnName == "DateOpened"))
                {
                    lstDateOpened = "* DateOpened";
                    bFootNote = true;
                }

                if (!dbRecordSet.Exists(x => x.ColumnName == "DateClosed"))
                {
                    lstDateClosed = "* DateClosed";
                    bFootNote = true;
                }

                if (!dbRecordSet.Exists(x => x.ColumnName == "DateCreated"))
                {
                    lstDateCreated = "* DateCreated";
                    bFootNote = true;
                }

                if (!dbRecordSet.Exists(x => x.ColumnName == "DateOther"))
                {
                    lstDateOther = "* DateOther";
                    bFootNote = true;
                }

                foreach (var oSchemaColumn in dbRecordSet)
                {

                    if (!SchemaInfoDetails.IsSystemField(oSchemaColumn.ColumnName))
                    {
                        if (oSchemaColumn.IsADate)
                        {
                            lstDateFields.Add(oSchemaColumn.ColumnName);
                        }
                        else if (oSchemaColumn.IsString && oSchemaColumn.CharacterMaxLength == 20)
                        {
                            lstRetCodeFields.Add(oSchemaColumn.ColumnName);
                        }
                    }
                }

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

                lstRetCodeFields.Sort();
                lstDateFields.Sort();

                string RetCodeFieldsObject = JsonConvert.SerializeObject(lstRetCodeFields, Newtonsoft.Json.Formatting.Indented, Setting);
                string DateFields = JsonConvert.SerializeObject(lstDateFields, Newtonsoft.Json.Formatting.Indented, Setting);

                var lstRelatedTables = _iRelationShip.All().Where(x => (x.LowerTableName) == (pTableEntites.TableName)).ToList();

                foreach (RelationShip item in lstRelatedTables)
                    lstRelatedTable.Add(item.UpperTableName);

                var lstTables = _iTable.All().Where(x => x.RetentionPeriodActive == true && x.RetentionFinalDisposition != 0 && lstRelatedTable.Contains(x.TableName));
                var pRetentionCodes = _iSLRetentionCode.All().OrderBy(x => x.Id);

                string relatedTblObj = JsonConvert.SerializeObject(lstTables, Newtonsoft.Json.Formatting.Indented, Setting);
                string tableObj = JsonConvert.SerializeObject(pTableEntites, Newtonsoft.Json.Formatting.Indented, Setting);
                string pRetentionCodesJSON = JsonConvert.SerializeObject(pRetentionCodes, Newtonsoft.Json.Formatting.Indented, Setting);
                var isThereLocation = Tracking.GetArchiveLocations(passport);
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage,
                    tableEntity = tableObj,
                    bTrackable,
                    RetIdFieldsList = RetCodeFieldsObject,
                    RetDateFieldsList = DateFields,
                    lstRetentionCode,
                    lstDateCreated,
                    lstDateClosed,
                    lstDateOpened,
                    lstDateOther,
                    bFootNote,
                    relatedTblObj,
                    pRetentionCodes = pRetentionCodesJSON,
                    pTableEntites.ArchiveLocationField,
                    isThereLocation
                });
            }

            catch (Exception)
            {
                return Json(new { success = false, errortype = "e", message = Keys.ErrorMessageJS() });
            }
        }

        // Set the parameters for Retention admin screen.
        public IActionResult SetRetentionParameters(bool pIsUseCitaions, int pYearEnd, int pInactivityPeriod)
        {
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                IRepository<SLServiceTask> _iSLServiceTasks = new Repositories<SLServiceTask>();
                _iSystem.BeginTransaction();
                _iSLServiceTasks.BeginTransaction();

                var pSystemEntity = _iSystem.All().OrderBy(x => x.Id).FirstOrDefault();

                pSystemEntity.RetentionTurnOffCitations = pIsUseCitaions;
                pSystemEntity.RetentionYearEnd = pYearEnd;
                _iSystem.Update(pSystemEntity);

                var pServiceTasks = _iSLServiceTasks.All().OrderBy(x => x.Id).FirstOrDefault();
                pServiceTasks.Interval = pInactivityPeriod;
                _iSLServiceTasks.Update(pServiceTasks);

                _iSystem.CommitTransaction();
                _iSLServiceTasks.CommitTransaction();

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgJsRetentionPropOnApply"); // Keys.SaveSuccessMessage()
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        #endregion

        #region Tracking
        public IActionResult LoadTrackingView()
        {
            return PartialView("_TrackingPartial");
        }

        public IActionResult TrackingFieldPartialView()
        {
            return PartialView("_TrackingFieldPartial");
        }

        public IActionResult GetReconciliation()
        {
            try
            {
                IRepository<AssetStatu> _iAssetStatus = new Repositories<AssetStatu>();
                var pAssetNumber = _iAssetStatus.All().OrderBy(m => m.Id);
                int totalRecord = pAssetNumber.Count();
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string jsonObject = JsonConvert.SerializeObject(totalRecord, Newtonsoft.Json.Formatting.Indented, Setting);
                return Json(jsonObject);
            }
            catch (Exception)
            {
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                Keys.ErrorType = "e";
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }

        }

        // Public Function RemoveReconciliation() As IActionResult
        // Try
        // Dim pAssetEntity = _iAssetStatus.All()
        // If pAssetEntity.Count() = 0 Then
        // Keys.ErrorType = "w"
        // Keys.ErrorMessage = Languages.Translation("msgAdminCtrlNoRecordFoundInSystem")
        // Else
        // For Each entity As AssetStatu In pAssetEntity.ToList()
        // _iAssetStatus.Delete(entity)
        // Next
        // Keys.ErrorType = "s"
        // Keys.ErrorMessage = Keys.DeleteSuccessMessage()
        // End If
        // Catch ex As Exception
        // Keys.ErrorType = "e"
        // Keys.ErrorMessage = Keys.ErrorMessageJS()
        // End Try
        // Return Json(New With {
        // Key .errortype = Keys.ErrorType,
        // Key .message = Keys.ErrorMessage
        // })
        // End Function

        // [HttpPost]
        public IActionResult SetTrackingSystemEntity(TabFusionRMS.Models.System systemTracking, bool pDateDueOn, bool pTrackingOutOn)
        {
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                var pSystemEntity = _iSystem.All().OrderBy(x => x.Id).FirstOrDefault();
                pSystemEntity.TrackingOutOn = pTrackingOutOn;
                pSystemEntity.DateDueOn = pDateDueOn;
                pSystemEntity.TrackingAdditionalField1Desc = systemTracking.TrackingAdditionalField1Desc;
                pSystemEntity.TrackingAdditionalField2Desc = systemTracking.TrackingAdditionalField2Desc;
                pSystemEntity.TrackingAdditionalField1Type = systemTracking.TrackingAdditionalField1Type;
                pSystemEntity.SignatureCaptureOn = systemTracking.SignatureCaptureOn;
                if (systemTracking.MaxHistoryDays is null)
                {
                    pSystemEntity.MaxHistoryDays = 0;
                }
                else
                {
                    pSystemEntity.MaxHistoryDays = systemTracking.MaxHistoryDays;
                }
                if (systemTracking.MaxHistoryItems is null)
                {
                    pSystemEntity.MaxHistoryItems = 0;
                }
                else
                {
                    pSystemEntity.MaxHistoryItems = systemTracking.MaxHistoryItems;
                }
                if ((systemTracking.DefaultDueBackDays == null)  || (systemTracking.DefaultDueBackDays == 0))
                {
                    pSystemEntity.DefaultDueBackDays = (short?)1;
                }
                else
                {
                    pSystemEntity.DefaultDueBackDays = systemTracking.DefaultDueBackDays;
                }
                _iSystem.Update(pSystemEntity);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminApplicationTracking");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public IActionResult GetTrackingSystemEntity()
        {
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                var pSystemEntity = _iSystem.All().OrderBy(m => m.Id).FirstOrDefault();
                var setting = new JsonSerializerSettings();
                setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string jsonObject = JsonConvert.SerializeObject(pSystemEntity, Newtonsoft.Json.Formatting.Indented, setting);
                return Json(jsonObject);
            }
            catch (Exception)
            {
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                Keys.ErrorType = "e";
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
        }

        public IActionResult GetTrackingFieldList(string sord, int page, int rows)
        {
            IRepository<SLTrackingSelectData> _iSLTrackingSelectData = new Repositories<SLTrackingSelectData>();
            var pTrackingEntity = _iSLTrackingSelectData.All();
            if (pTrackingEntity is null)
            {
                return Json(new object());
            }
            else
            {
                var jsonData = pTrackingEntity.GetJsonListForGrid(sord, page, rows, "Id");
                return Json(jsonData);
            }
        }

        public IActionResult GetTrackingField(Array pRowSelected)
        {
            IRepository<SLTrackingSelectData> _iSLTrackingSelectData = new Repositories<SLTrackingSelectData>();
            if (pRowSelected is null)
            {
                return Json(new { success = false });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { success = false });
            }

            int pTrackingFieldId = Convert.ToInt32(pRowSelected.GetValue(0));
            if (pTrackingFieldId == 0)
            {
                return Json(new { success = false });
            }

            var pTrackingFieldEntity = _iSLTrackingSelectData.Where(x => x.SLTrackingSelectDataId == pTrackingFieldId).FirstOrDefault();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(pTrackingFieldEntity, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        //        [HttpPost]
        public IActionResult SetTrackingField(SLTrackingSelectData pSLTrackingData)
        {
            IRepository<SLTrackingSelectData> _iSLTrackingSelectData = new Repositories<SLTrackingSelectData>();
            try
            {
                pSLTrackingData.Id = pSLTrackingData.Id.Trim();
                if (pSLTrackingData.SLTrackingSelectDataId > 0)
                {
                    if (_iSLTrackingSelectData.All().Any(x => x.Id.Trim().ToLower() == pSLTrackingData.Id.Trim().ToLower() && x.SLTrackingSelectDataId != pSLTrackingData.SLTrackingSelectDataId) == false)
                    {
                        _iSLTrackingSelectData.Update(pSLTrackingData);
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgJsTrackingAdditionalFieldEdit");
                    }
                    else
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Keys.AlreadyExistMessage(Languages.get_Translation("msgAdminCtrlAddTrackField"));
                    }
                }
                else if (_iSLTrackingSelectData.All().Any(x => (x.Id.Trim().ToLower()) == (pSLTrackingData.Id.Trim().ToLower())) == false)
                {
                    _iSLTrackingSelectData.Add(pSLTrackingData);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgJSTrackingAdditionalTrackFieldAdd");
                }
                else
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Keys.AlreadyExistMessage(Languages.get_Translation("msgAdminCtrlAddTrackField"));
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        public IActionResult RemoveTrackingField([FromForm] string[] pRowSelected)
        {
            IRepository<SLTrackingSelectData> _iSLTrackingSelectData = new Repositories<SLTrackingSelectData>();
            try
            {
                if (pRowSelected is null)
                {
                    return Json(new { success = false });
                }
                if (pRowSelected.Length == 0)
                {
                    return Json(new { success = false });
                }

                int pTrackingFieldId = Convert.ToInt32(pRowSelected.GetValue(0));
                if (pTrackingFieldId == 0)
                {
                    return Json(new { success = false });
                }
                var pTrackingFieldEntity = _iSLTrackingSelectData.Where(x => x.SLTrackingSelectDataId == pTrackingFieldId).FirstOrDefault();
                if (pTrackingFieldEntity is not null)
                {
                    _iSLTrackingSelectData.Delete(pTrackingFieldEntity);
                    Keys.ErrorMessage = Languages.get_Translation("msgJsTrackingAdditionalFieldDel"); // Keys.DeleteSuccessMessage()
                    Keys.ErrorType = "s";
                }
                else
                {
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlNoRecordFoundInSystem");
                    Keys.ErrorType = "e";
                }
            }
            catch (Exception)
            {
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                Keys.ErrorType = "e";
            }


            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });

        }

        [HttpPost]
        public IActionResult SetTrackingHistoryData(TabFusionRMS.Models.System systemTracking)
        {
            IRepository<TrackingHistory> _iTrackingHistory = new Repositories<TrackingHistory>();
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                var pSystemEntity = _iSystem.All().OrderBy(x => x.Id).FirstOrDefault();
                if (systemTracking.MaxHistoryDays is null)
                {
                    pSystemEntity.MaxHistoryDays = 0;
                }
                else
                {
                    pSystemEntity.MaxHistoryDays = systemTracking.MaxHistoryDays;
                }
                if (systemTracking.MaxHistoryItems is null)
                {
                    pSystemEntity.MaxHistoryItems = 0;
                }
                else
                {
                    pSystemEntity.MaxHistoryItems = systemTracking.MaxHistoryItems;
                }
                _iSystem.Update(pSystemEntity);

                bool catchFlag;

                string KeysType = "";
                // Dim trackingServiceObj As New TrackingServices(_IDBManager, _iTable, _iDatabase, _iScanList, _iTabSet, _iTableTab, _iRelationship, _iView, _iSystem, _iTrackingHistory)
                catchFlag = TrackingServices.InnerTruncateTrackingHistory(_iSystem.All(), _iTrackingHistory, "", "", ref KeysType);
                if (catchFlag == true)
                {
                    if (KeysType == "s")
                    {
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("HistoryHasBeenTruncated");
                    }
                    else
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Languages.get_Translation("NoMoreHistoryToTruncate");
                    }
                }

                else
                {
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                    Keys.ErrorType = "e";
                }
            }

            catch (Exception)
            {
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                Keys.ErrorType = "e";
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        #endregion

        #region Appearance
        public IActionResult LoadApplicationView()
        {
            return PartialView("_ApplicationAppearancePartial");
        }

        public IActionResult GetSystemList()
        {
            IRepository<Setting> _iSetting = new Repositories<Setting>();
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            var pSystemEntities = from t in _iSystem.All()
                                  select new { t.AlternateRowColors, t.GridBackColorEven, t.GridBackColorOdd, t.GridForeColorEven, t.GridForeColorOdd, t.FormViewMinLines, t.ReportGridColor, t.UseTableIcons };

            var pSettingEntities = from t in _iSetting.All()
                                   select new { t.Section, t.ItemValue };

            string theme = pSettingEntities.Where(x => x.Section.Trim().ToLower().Equals("usetheme")).First().ItemValue;

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(pSystemEntities, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);

        }

        //        [HttpPost]
        public IActionResult SetSystemDetails(TabFusionRMS.Models.System systemAppearance)
        {
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            if (systemAppearance.Id > 0)
            {
                _iSystem.Update(systemAppearance);
            }
            else
            {
                _iSystem.Add(systemAppearance);
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        #endregion

        #region TABQUIK

        public IActionResult LoadTABQUIKIntegrationKeyView()
        {
            return PartialView("_AddTABQUIKKey");
        }

        // [HttpGet]
        public IActionResult GetTabquikKey()
        {
            IRepository<Setting> _iSetting = new Repositories<Setting>();
            var tabquikkey = _iSetting.All().Where(s => s.Item.Equals("Key") & s.Section.Equals("TABQUIK")).FirstOrDefault();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(tabquikkey, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }
        //        [HttpPost]
        public IActionResult SetTabquikKey(string pTabquikkey)
        {
            try
            {
                IRepository<Setting> _iSetting = new Repositories<Setting>();
                var tabquiksetting = _iSetting.All().Where(s => s.Item.Equals("Key") & s.Section.Equals("TABQUIK")).FirstOrDefault();

                if (tabquiksetting == null)
                {
                    tabquiksetting = new Setting();
                    tabquiksetting.Section = "TABQUIK";
                    tabquiksetting.Item = "Key";
                    tabquiksetting.ItemValue = pTabquikkey;
                    _iSetting.Add(tabquiksetting);
                }
                else
                {
                    tabquiksetting.ItemValue = pTabquikkey;
                    _iSetting.Update(tabquiksetting);
                }

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgJsTabquikSaveSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        #endregion
        #region TABQUIK -> Field Mapping

        public IActionResult LoadTABQUIKFieldMappingPartial()
        {
            // Set "FLDColumns" session to Nothing, when returnning job list.
            ContextService.RemoveSession("FLDColumns", httpContext);
            return PartialView("_TABQUIKLabelList");
        }

        public IActionResult TABQUIKFieldMappingPartial(int pTabquikId)
        {
            IRepository<OneStripJob> _iOneStripJob = new Repositories<OneStripJob>();
            if (pTabquikId != 0)
            {
                var oOneStripJob = _iOneStripJob.All().Where(x => x.Id == pTabquikId && x.Inprint == 5).FirstOrDefault();
                TempData["sTableName"] = oOneStripJob.TableName;
                TempData["sSQLUpdateString"] = oOneStripJob.SQLUpdateString;
            }
            else
            {
                TempData["sTableName"] = "";
                TempData["sSQLUpdateString"] = "";
            }
            return PartialView("_TABQUIKFieldMapping");
        }

        public IActionResult GetTABQUIKMappingGrid(string sTableName, string sOperation, string sJobName, string sord, int page, int rows)
        {
            IRepository<OneStripJobField> _iOneStripJobField = new Repositories<OneStripJobField>();
            IRepository<OneStripJob> _iOneStripJob = new Repositories<OneStripJob>();
            try
            {
                var oDatatable = new DataTable();
                string sFLDColumns = "";
                var splitColumns = new List<string>();
                var lstAllColumnNames = new List<string>();

                oDatatable.Columns.Add(new DataColumn("TABQUIKField"));
                oDatatable.Columns.Add(new DataColumn("TABFusionRMSField"));
                oDatatable.Columns.Add(new DataColumn("Manual"));
                oDatatable.Columns.Add(new DataColumn("Format"));

                if (sOperation.Equals("Add"))
                {
                    if (!(ContextService.GetSessionValueByKey("FLDColumns", httpContext) == null))
                    {
                        sFLDColumns = ContextService.GetSessionValueByKey("FLDColumns", httpContext).ToString();
                        splitColumns = sFLDColumns.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        foreach (string sTABQUIKFieldName in splitColumns)
                        {
                            var dr = oDatatable.NewRow();
                            dr["TABQUIKField"] = sTABQUIKFieldName;
                            oDatatable.Rows.Add(dr);
                        }
                    }
                }
                else if (sOperation.Equals("Edit"))
                {
                    if (!(ContextService.GetSessionValueByKey("lstAllColumnNames", httpContext) == null))
                    {
                        var lstAllColumnNamesobj = ContextService.GetObjectFromJson<List<string>>("lstAllColumnNames", httpContext);
                        lstAllColumnNames = lstAllColumnNamesobj;
                    }

                    var oOneStripJob = _iOneStripJob.All().Where(x => x.Name == sJobName && x.TableName == sTableName && x.Inprint == 5).FirstOrDefault();
                    var oOneStripJobGeneral = _iOneStripJob.All().Where(x => x.Name == sJobName && x.Inprint == 5).FirstOrDefault();
                    IEnumerable<OneStripJobField> oOneStripJobFields = null;

                    if (oOneStripJob is not null)
                    {
                        oOneStripJobFields = _iOneStripJobField.All().Where(x => x.OneStripJobsId == oOneStripJob.Id);
                    }

                    if (!(oOneStripJobGeneral == null))
                    {
                        if (!string.IsNullOrEmpty(oOneStripJobGeneral.FLDFieldNames))
                        {
                            sFLDColumns = oOneStripJobGeneral.FLDFieldNames.ToString();
                            splitColumns = sFLDColumns.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }
                        else if (!(ContextService.GetSessionValueByKey("FLDColumns", httpContext) == null))
                        {
                            sFLDColumns = ContextService.GetSessionValueByKey("FLDColumns", httpContext).ToString();
                            splitColumns = sFLDColumns.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }
                    }

                    if (!(oOneStripJobFields == null))
                    {
                        foreach (string sTABQUIKFieldName in splitColumns)
                        {
                            var oOneStripJobField = oOneStripJobFields.Where(x => (x.FontName) == (sTABQUIKFieldName)).FirstOrDefault();
                            var dr = oDatatable.NewRow();
                            dr["TABQUIKField"] = sTABQUIKFieldName;
                            if (!(oOneStripJobField == null))
                            {
                                if (Strings.InStr(oOneStripJobField.FieldName, Constants.vbNullChar) != 0L)
                                {
                                    string sManualField = "";
                                    sManualField = Strings.Mid(oOneStripJobField.FieldName, (int)(Strings.InStr(oOneStripJobField.FieldName, Constants.vbNullChar) + 1L));
                                    dr["TABFusionRMSField"] = "";
                                    dr["Manual"] = sManualField;
                                }
                                else
                                {
                                    // Check if field is from current table.
                                    if (oOneStripJobField.FieldName.Contains(sTableName + "."))
                                    {
                                        dr["TABFusionRMSField"] = lstAllColumnNames.IndexOf(DatabaseMap.RemoveTableNameFromField(oOneStripJobField.FieldName));
                                    }
                                    else
                                    {
                                        dr["TABFusionRMSField"] = lstAllColumnNames.IndexOf(">" + oOneStripJobField.FieldName);
                                    }
                                    dr["Manual"] = "";
                                }

                                dr["Format"] = oOneStripJobField.Format;
                            }
                            else
                            {
                                dr["TABFusionRMSField"] = "";
                                dr["Manual"] = "";
                                dr["Format"] = "";
                            }
                            oDatatable.Rows.Add(dr);
                        }
                    }
                    else
                    {
                        foreach (string sTABQUIKFieldName in splitColumns)
                        {
                            var dr = oDatatable.NewRow();
                            dr["TABQUIKField"] = sTABQUIKFieldName;
                            oDatatable.Rows.Add(dr);
                        }
                    }
                }

                // Kept in session for 'Auto Fill' function, to know the columns data
                ContextService.SetSessionValue("dtTABQuikMapping", JsonConvert.SerializeObject(oDatatable), httpContext);
                return Json(Common.ConvertDataTableToJQGridResult(oDatatable, "", sord, page, rows));
            }

            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }
        public IActionResult GetTABQUIKMappingGridWithAutoFill(string sOperation, string sord, int page, int rows)
        {
            var oDatatable = new DataTable();
            var lstAllColumnNames = new List<string>();

            try
            {
                if (!(ContextService.GetSessionValueByKey("dtTABQuikMapping", httpContext) == null) & !(ContextService.GetSessionValueByKey("lstAllColumnNames", httpContext) == null))
                {
                    ContextService.SetSessionValue("dtTABQuikMapping", JsonConvert.SerializeObject(oDatatable), httpContext);
                    var lstAllColumnNamesobj = ContextService.GetObjectFromJson<List<string>>("lstAllColumnNames", httpContext);
                    lstAllColumnNames = lstAllColumnNamesobj;

                    // Find matches between TABQUIK fields and TABFusionRMS fields
                    foreach (DataRow row in oDatatable.Rows)
                    {
                        string tabquikFieldName = row["TABQUIKField"].ToString();
                        if (lstAllColumnNames.Contains(tabquikFieldName))
                        {
                            // row("TABFusionRMSField") = tabquikFieldName
                            row["TABFusionRMSField"] = lstAllColumnNames.IndexOf(tabquikFieldName);
                        }
                        else
                        {
                            row["TABFusionRMSField"] = "0";
                        } // Set it to blank
                    }
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(Common.ConvertDataTableToJQGridResult(oDatatable, "", sord, page, rows));
        }

        public JsonResult GetOneStripJobs()
        {
            IRepository<OneStripJob> _iOneStripJob = new Repositories<OneStripJob>();
            try
            {
                var oOneStripJob = _iOneStripJob.All().Where(x => x.Inprint == 5).Select(y => new { Value = y.Id, y.Name, y.TableName });
                return Json(oOneStripJob);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        public IActionResult RemoveSelectedJob(int pTabquikJobId)
        {
            IRepository<OneStripJobField> _iOneStripJobField = new Repositories<OneStripJobField>();
            IRepository<OneStripJob> _iOneStripJob = new Repositories<OneStripJob>();
            try
            {
                _iOneStripJob.BeginTransaction();
                _iOneStripJobField.BeginTransaction();
                var oOneStripJob = _iOneStripJob.All().Where(x => x.Id == pTabquikJobId).FirstOrDefault();
                var oOneStripJobFields = _iOneStripJobField.All().Where(x => x.OneStripJobsId == oOneStripJob.Id);

                _iOneStripJobField.DeleteRange(oOneStripJobFields);
                _iOneStripJob.Delete(oOneStripJob);

                _iOneStripJobField.CommitTransaction();
                _iOneStripJob.CommitTransaction();

                Keys.ErrorType = "s";
                Keys.ErrorMessage = "";
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        //        [HttpPost]
        public async Task<IActionResult> UploadColorLabelFiles([FromServices] IWebHostEnvironment webHostEnvironment,[FromQuery] object files)
        {
            string serverPath = "";
            string filePath = "";
            string sAllFLDColumns = "";
            string sOperation = httpContext.Request.Form["sOperation"][0].ToString();
            try
            {
                var file = httpContext.Request.Form.Files;
                string sJobName = file[0].FileName.Split('.')[0];

                if (!IsJobAlreadyExists(sJobName) || sOperation.Equals("Edit")) 
                {
                    string sGUID = Guid.NewGuid().ToString();
                    serverPath = webHostEnvironment.ContentRootPath + "/LabelData/"; 
                    httpContext.Session.Remove("FLDColumns");

                    foreach (var pfilePath in file)
                    {

                        var pfile = pfilePath;
                        var pfileNameArr = pfilePath.FileName.Split(".");

                        if (pfileNameArr[1].ToLower().Equals("fld"))
                        {
                            string pNewFileName = pfileNameArr[0] + "_" + sGUID + "." + pfileNameArr[1];
                            filePath = System.IO.Path.Combine(serverPath, pNewFileName);
                            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await pfile.CopyToAsync(fileStream);
                            }
                        }
                    }

                    // Read TABQUIK column names from FLD file
                    using (var sr = new System.IO.StreamReader(filePath))
                    {
                        string line = "";
                        string sTABQUIKFieldName = "";

                        while (!sr.EndOfStream)
                        {
                            line = sr.ReadLine();
                            sTABQUIKFieldName = Strings.Trim(Strings.Left(line, 20));
                            sAllFLDColumns += sTABQUIKFieldName + "~";
                        }
                    }

                    // Delete uploaded design files once we retrieved TABQUIK Field names.            
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    sAllFLDColumns = Strings.Left(sAllFLDColumns, (int)(Strings.Len(sAllFLDColumns) - 1L));
                    ContextService.SetSessionValue("FLDColumns", JsonConvert.SerializeObject(sAllFLDColumns), httpContext);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = "";
                }
                else
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgTABQUIKDuplicateJob"), sJobName);
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public bool IsJobAlreadyExists(string jobName)
        {
            bool jobExists = false;
            try
            {
                IRepository<OneStripJob> _iOneStripJob = new Repositories<OneStripJob>();
                // Check if JOB already exists for TABQUIK label
                if (_iOneStripJob.All().Any(x => x.Name == jobName && x.Inprint == 5))
                {
                    jobExists = true;
                }
            }
            catch (Exception)
            {

            }

            return jobExists;
        }

        public JsonResult IsFLDNamesExists(string jobName)
        {
            bool fldNamesExists = false;
            OneStripJob oOneStripJob;
            IRepository<OneStripJob> _iOneStripJob = new Repositories<OneStripJob>();
            try
            {
                oOneStripJob = _iOneStripJob.All().Where(x => x.Name == jobName && x.Inprint == 5).FirstOrDefault();

                if (!string.IsNullOrEmpty(oOneStripJob.FLDFieldNames))
                {
                    fldNamesExists = true;
                }
                Keys.ErrorType = "s";
                Keys.ErrorMessage = "";
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                fldNamesExists
            });
        }

        public IActionResult GetTableFieldsListAndParentTableFields(string pTableName)
        {
            var lstAllColumnNames = new List<string>();
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            try
            {

                var lstColumnNames = new List<string>();
                var oParentTableList = _iRelationShip.All().Where(x => (x.LowerTableName) == (pTableName)).Select(y => new { TableName = y.UpperTableName }).ToList();

                // Insert first value blank for empty field selection
                lstColumnNames.Add("");
                lstColumnNames.AddRange(GetColumnListOfTable(pTableName));
                lstAllColumnNames.AddRange(lstColumnNames);

                if (oParentTableList is not null)
                {
                    foreach (var oTable in oParentTableList)
                    {
                        lstColumnNames = GetColumnListOfTable(oTable.TableName, true);
                        lstAllColumnNames.AddRange(lstColumnNames);
                    }
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            ContextService.SetSessionValue("lstAllColumnNames", JsonConvert.SerializeObject(lstAllColumnNames), httpContext);
            string jsonObject = JsonConvert.SerializeObject(lstAllColumnNames, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        public List<string> GetColumnListOfTable(string pTableName, bool IncludeTableNameInField = false)
        {
            var lstColumnNames = new List<string>();
            string sNewColumnName = "";
            SchemaIndex PrimaryKeyField = null;
            string sPrimaryKeyField = "";
            try
            {
                if (!string.IsNullOrEmpty(pTableName))
                {
                    var conObj = DataServices.DBOpen(Enums.eConnectionType.conDefault);

                    var pSchemaIndexList = new List<SchemaIndex>();
                    pSchemaIndexList = SchemaIndex.GetTableIndexes(pTableName, conObj);
                    // Get the primary key of table, avoid loading this in parent table field list.
                    PrimaryKeyField = pSchemaIndexList.Where(x => x.PrimaryKey == "True").FirstOrDefault();
                    if (!(PrimaryKeyField == null))
                    {
                        sPrimaryKeyField = PrimaryKeyField.ColumnName;
                    }

                    var oCurrentTableSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pTableName, conObj);

                    foreach (var oSchemaColumn in oCurrentTableSchemaColumns)
                    {
                        if (!SchemaInfoDetails.IsSystemField(oSchemaColumn.ColumnName))
                        {
                            if (!IncludeTableNameInField)
                            {
                                lstColumnNames.Add(oSchemaColumn.ColumnName);
                            }
                            else if (!oSchemaColumn.ColumnName.Equals(sPrimaryKeyField))
                            {
                                sNewColumnName = ">" + oSchemaColumn.TableName + "." + oSchemaColumn.ColumnName;
                                lstColumnNames.Add(sNewColumnName);
                            }
                        }
                    }
                    conObj.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }

            return lstColumnNames;
        }

        //        [HttpPost]
        public IActionResult ValidateTABQUIKSQLStatments(string SQLStatement, string sTableName, bool IsSelectStatement)
        {
            string responseMessage = string.Empty;
            int lError = 0;
            string sSQLStatement = string.Empty;
            bool IsBasicSyntaxCorrect = true;
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                sSQLStatement = SQLStatement;
                sSQLStatement = Strings.Trim(sSQLStatement);
                // Check basic conditions on SQL statements
                if (Strings.StrComp("SELECT ", Strings.Left(sSQLStatement, Strings.Len("SELECT ")), Constants.vbTextCompare) != 0L & IsSelectStatement)
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Languages.get_Translation("errormsgSQLSelectKeyword");
                    IsBasicSyntaxCorrect = false;
                }
                else if (Strings.StrComp("UPDATE ", Strings.Left(sSQLStatement, Strings.Len("UPDATE ")), Constants.vbTextCompare) != 0L & Strings.StrComp("DELETE ", Strings.Left(sSQLStatement, Strings.Len("DELETE ")), Constants.vbTextCompare) != 0L & !IsSelectStatement)
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Languages.get_Translation("errorMsgSQLUpdatedelete");
                    IsBasicSyntaxCorrect = false;
                }

                if (!IsSelectStatement & Strings.InStr(1, sSQLStatement, "<YourTable>", Constants.vbTextCompare) != 0)
                {
                    sSQLStatement = "";
                }

                var oTable = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())).FirstOrDefault();

                if (IsBasicSyntaxCorrect & !string.IsNullOrEmpty(sSQLStatement))
                {
                    if (!IsSelectStatement)
                    {
                        sSQLStatement = Strings.Replace(sSQLStatement, "%ID%", "0", 1, (int)-1L, Constants.vbTextCompare);
                        sSQLStatement = DataServices.InjectWhereIntoSQL(sSQLStatement, "0=1");
                    }
                    else if (IsSelectStatement)
                    {
                        sSQLStatement = Strings.Replace(sSQLStatement, "%ID%", "0", 1, (int)-1L, Constants.vbTextCompare);
                    }

                    responseMessage = Common.ValidateSQLStatement(sSQLStatement, oTable, _iDatabas.All(), ref lError);

                    if (string.IsNullOrEmpty(responseMessage))
                    {
                        Keys.ErrorType = "s";
                    }
                    else if (lError == -2147217865 & IsSelectStatement)
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Languages.get_Translation("msgLabelManagerCtrlSQLStrHvInvalidTblName") + Constants.vbCrLf + Constants.vbCrLf + Languages.get_Translation("lblBackgroundProcessStatusError") + ": " + responseMessage;
                    }
                    else if (lError == -2147217865 & !IsSelectStatement)
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Languages.get_Translation("msgTABQUIKInvalidaUpdateSQL") + Constants.vbCrLf + Constants.vbCrLf + Languages.get_Translation("lblBackgroundProcessStatusError") + ": " + responseMessage;
                    }
                    else if (!IsSelectStatement)
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Languages.get_Translation("msgLabelManagerCtrlSQLUpdateStrInvalid") + Constants.vbCrLf + Constants.vbCrLf + Languages.get_Translation("lblBackgroundProcessStatusError") + ": " + responseMessage;
                    }
                    else
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = responseMessage;
                    }
                }
            }

            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        //        [HttpPost]
        public IActionResult FormTABQUIKSelectSQLStatement(string pTableName, string dtTABQUIKData)
        {
            string strSQLStatement = "SELECT ";
            string sTmpParentTable = "";
            var lstAllColumnNames = new List<string>();
            var lstParentTables = new List<string>();
            int index = 0;
            int lParentPos = 0;
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                var oTable = _iTable.All().Where(x => (x.TableName) == (pTableName)).FirstOrDefault();

                var lstTABQUIKModels = JsonConvert.DeserializeObject<List<TABQUIKModel>>(dtTABQUIKData);

                lstAllColumnNames = ContextService.GetObjectFromJson<List<string>>("lstAllColumnNames", httpContext);

                foreach (var tabquikmodel in lstTABQUIKModels)
                {
                    string sTABFusionRMSField = "";

                    if (!string.IsNullOrEmpty(tabquikmodel.TABFusionRMSField))
                    {
                        sTABFusionRMSField = lstAllColumnNames[Convert.ToInt32(tabquikmodel.TABFusionRMSField)];
                    }

                    string sManualField = tabquikmodel.Manual;

                    if (!string.IsNullOrEmpty(sTABFusionRMSField) & Strings.InStr(1, sTABFusionRMSField, ">") == 0L)
                    {
                        strSQLStatement = strSQLStatement + "[" + pTableName + "].[" + sTABFusionRMSField + "], ";
                    }
                    else if (!string.IsNullOrEmpty(sTABFusionRMSField))
                    {
                        sTmpParentTable = Strings.Mid(sTABFusionRMSField, 2, (int)(Strings.InStr(1, sTABFusionRMSField, ".") - 2L));
                        if (!lstParentTables.Contains(Strings.Trim(sTmpParentTable)))
                        {
                            lstParentTables.Add(Strings.Trim(sTmpParentTable));
                        }
                        strSQLStatement = strSQLStatement + "[" + Strings.Replace(Strings.Mid(sTABFusionRMSField, 2, Strings.Len(sTABFusionRMSField)), ".", "].[") + "], ";
                    }
                    // Check for any alias fields
                    if (!string.IsNullOrEmpty(sManualField))
                    {
                        strSQLStatement = strSQLStatement + "(" + sManualField + ") AS ManualField" + index + ", ";
                    }
                    index = index + 1;
                }

                if (Strings.StrComp(Strings.Trim(strSQLStatement), "SELECT", Constants.vbTextCompare) == 0L)
                {
                    strSQLStatement = strSQLStatement + "*  ";
                }

                strSQLStatement = Strings.Left(strSQLStatement, (int)(Strings.Len(strSQLStatement) - 2L)) + " ";
                strSQLStatement = strSQLStatement + "FROM ";
                lParentPos = Strings.Len(strSQLStatement);
                strSQLStatement = strSQLStatement + pTableName + " ";

                // Append LEFT JOIN condition
                foreach (var sParentTable in lstParentTables)
                {
                    var oRelationShips = _iRelationShip.All().Where(x => (x.LowerTableName) == (pTableName) & (x.UpperTableName) == (sParentTable)).FirstOrDefault();

                    strSQLStatement = Strings.Left(strSQLStatement, lParentPos) + "(" + Strings.Mid(strSQLStatement, (int)(lParentPos + 1L));
                    strSQLStatement = strSQLStatement + "LEFT JOIN " + sParentTable + " ON ";
                    strSQLStatement = strSQLStatement + oRelationShips.LowerTableFieldName + " = " + oRelationShips.UpperTableFieldName + ") ";
                }
                // Append WHERE clause
                strSQLStatement = strSQLStatement + "WHERE " + oTable.IdFieldName + " = ";

                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString(null);
                _IDBManager.Open();
                bool IsIdFieldIdFieldIsString = GetInfoUsingADONET.IdFieldIsString(_IDBManager, oTable.TableName, oTable.IdFieldName);
                _IDBManager.Close();
                if (IsIdFieldIdFieldIsString)
                {
                    strSQLStatement = strSQLStatement + "'%ID%'";
                }
                else
                {
                    strSQLStatement = strSQLStatement + "%ID%";
                }

                Keys.ErrorType = "s";
                Keys.ErrorMessage = "";
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                throw ex.InnerException;
            }
            finally
            {
                if (_IDBManager.Connection is not null)
                {
                    _IDBManager.Dispose();
                }
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                SQLStatement = strSQLStatement
            });
        }

        //        [HttpPost]
        public IActionResult SaveTABQUIKFields(string sOperation, string JobName, string TableName, string SQLSelectString, string SQLUpdateString, string dtTABQUIKData)
        {
            int index = 0;
            IRepository<OneStripJobField> _iOneStripJobField = new Repositories<OneStripJobField>();
            IRepository<OneStripJob> _iOneStripJob = new Repositories<OneStripJob>();
            try
            {
                var lstTABQUIKModels = JsonConvert.DeserializeObject<List<TABQUIKModel>>(dtTABQUIKData);

                if (sOperation.Equals("Add"))
                {
                    if (!IsJobAlreadyExists(JobName))
                    {
                        var oOneStripJob = new OneStripJob();
                        oOneStripJob.Name = JobName;
                        oOneStripJob.TableName = TableName;
                        oOneStripJob.Inprint = (short?)5;
                        oOneStripJob.SQLString = SQLSelectString;
                        oOneStripJob.SQLUpdateString = SQLUpdateString;
                        oOneStripJob.Sampling = 0;
                        if (!(ContextService.GetSessionValueByKey("FLDColumns",httpContext) == null))
                        {
                            oOneStripJob.FLDFieldNames = httpContext.Session.GetString("FLDColumns");
                        }
                        _iOneStripJob.Add(oOneStripJob);

                        oOneStripJob = _iOneStripJob.All().Where(x => x.Name == JobName && x.Inprint == 5).FirstOrDefault();
                        this.AddOneStripJobFields(TableName, lstTABQUIKModels, oOneStripJob.Id);
                    }
                }
                else if (sOperation.Equals("Edit") & !string.IsNullOrEmpty(JobName))
                {
                    var oOneStripJob = new OneStripJob();
                    IEnumerable<OneStripJobField> oOneStripJobFields;
                    int oneStripJobId = 0;
                    oOneStripJob = _iOneStripJob.All().Where(x => x.Name == JobName && x.Inprint == 5).FirstOrDefault();
                    oOneStripJob.TableName = TableName;
                    oOneStripJob.SQLString = SQLSelectString;
                    oOneStripJob.SQLUpdateString = SQLUpdateString;
                    oOneStripJob.Sampling = 0;
                    if (!(httpContext.Session.GetString("FLDColumns") == null))
                    {
                        oOneStripJob.FLDFieldNames = httpContext.Session.GetString("FLDColumns").ToString();
                    }
                    _iOneStripJob.Update(oOneStripJob);

                    oneStripJobId = oOneStripJob.Id;
                    // Delete existing field mapping for this JOB
                    oOneStripJobFields = _iOneStripJobField.All().Where(x => x.OneStripJobsId == oneStripJobId);
                    _iOneStripJobField.DeleteRange(oOneStripJobFields);

                    this.AddOneStripJobFields(TableName, lstTABQUIKModels, oOneStripJob.Id);
                }
                httpContext.Session.Remove("FLDColumns");
                httpContext.Session.Remove("dtTABQuikMapping");
                httpContext.Session.Remove("lstAllColumnNames");

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgColorLabelSave");
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public void AddOneStripJobFields(string sTableName, List<TABQUIKModel> lstTABQUIKModels, int oneStripJobId)
        {
            int index = 0;
            string sTABFusionRMSField = "";
            var lstAllColumnNames = new List<string>();
            IRepository<OneStripJobField> _iOneStripJobField = new Repositories<OneStripJobField>();
            if (!(httpContext.Session.GetString("lstAllColumnNames") == null))
            {
                ContextService.GetObjectFromJson<List<string>>("lstAllColumnNames", httpContext);
            }

            try
            {
                foreach (var tabquikModel in lstTABQUIKModels)
                {
                    if (!string.IsNullOrEmpty(tabquikModel.TABFusionRMSField))
                    {
                        var oOneStripJobField = new OneStripJobField();
                        oOneStripJobField.OneStripJobsId = oneStripJobId;
                        oOneStripJobField.FontName = tabquikModel.TABQUIKField;
                        sTABFusionRMSField = lstAllColumnNames[Convert.ToInt32(tabquikModel.TABFusionRMSField)];
                        if (!sTABFusionRMSField.Contains("."))
                        {
                            sTABFusionRMSField = sTableName + "." + sTABFusionRMSField;
                        }
                        else if (sTABFusionRMSField.Contains(".") & sTABFusionRMSField.Contains(">"))
                        {
                            sTABFusionRMSField = sTABFusionRMSField.Remove(0, 1);
                        }
                        oOneStripJobField.FieldName = sTABFusionRMSField;
                        oOneStripJobField.Format = string.IsNullOrEmpty(tabquikModel.Format) ? null : tabquikModel.Format;
                        oOneStripJobField.Type = "T";
                        oOneStripJobField.SetNum = (short?)0;
                        oOneStripJobField.XPos = 0;
                        oOneStripJobField.YPos = 0;
                        oOneStripJobField.FontSize = 0;
                        _iOneStripJobField.Add(oOneStripJobField);
                    }
                    else if (!string.IsNullOrEmpty(tabquikModel.Manual)) // Add Manual Field
                    {
                        var oOneStripJobField = new OneStripJobField();
                        oOneStripJobField.OneStripJobsId = oneStripJobId;
                        oOneStripJobField.FontName = tabquikModel.TABQUIKField;
                        oOneStripJobField.FieldName = "ManualField" + index + Constants.vbNullChar + tabquikModel.Manual;
                        oOneStripJobField.Format = string.IsNullOrEmpty(tabquikModel.Format) ? null : tabquikModel.Format;
                        oOneStripJobField.Type = "T";
                        oOneStripJobField.SetNum = (short?)0;
                        oOneStripJobField.XPos = 0;
                        oOneStripJobField.YPos = 0;
                        oOneStripJobField.FontSize = 0;
                        _iOneStripJobField.Add(oOneStripJobField);
                    }
                    index = index + 1;
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        #endregion


        #region Attachments
        public PartialViewResult LoadAttachmentParticalView()
        {
            IRepository<vwGetOutputSetting> _ivwGetOutputSetting = new Repositories<vwGetOutputSetting>();
            var pFilterdVolums = new List<vwGetOutputSetting>();
            var pVolumeList = _ivwGetOutputSetting.All().Where(x => x.Active == true).ToList();
            foreach (var pvwGetOutputSetting in pVolumeList)
            {
                if (passport.CheckPermission(pvwGetOutputSetting.Name, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Volumes, Permissions.Permission.Access))
                {
                    pFilterdVolums.Add(pvwGetOutputSetting);
                }
            }
            this.ViewBag.OutputSettingList = pFilterdVolums.CreateSelectListFromList("VolumesId", "OutputPath", null);
            return PartialView("_AttachmentsPartial");
        }

        public IActionResult GetOutputSettingList()
        {
            IRepository<OutputSetting> _iOutputSetting = new Repositories<OutputSetting>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            var pOutputSettingsEntities = from t in _iSecureObject.All() select new { t.SecureObjectID, t.Name, t.BaseID };
            var pOutputSettingsList = new List<string>();


            int pSecureObjectID = pOutputSettingsEntities.Where(x => x.Name.Trim().ToLower().Equals("output settings")).FirstOrDefault().SecureObjectID;
            // Added by Ganesh for Security Integration fix.
            pOutputSettingsEntities = pOutputSettingsEntities.Where(x => x.BaseID == pSecureObjectID);
            foreach (var oOutputSettings in pOutputSettingsEntities)
            {
                // Change on 09/08/2016 - Tejas
                var objOutputSetting = _iOutputSetting.All().Where(x => x.Id.ToString().Trim().ToLower().Equals(oOutputSettings.Name.ToString().Trim().ToLower())).FirstOrDefault();
                if (objOutputSetting is not null)
                {
                    if (passport.CheckPermission(oOutputSettings.Name, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.OutputSettings, (Permissions.Permission)Enums.PassportPermissions.Access))
                    {
                        pOutputSettingsList.Add(oOutputSettings.Name);
                    }
                }
            }

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

            string jsonObject = JsonConvert.SerializeObject(pOutputSettingsList, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);


        }

        public IActionResult EditAttachmentSettingsEntity()
        {
            try
            {
                IRepository<Setting> _iSetting = new Repositories<Setting>();
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                var pSystemEntity = _iSystem.All().OrderBy(x => x.Id).FirstOrDefault();
                var lSettingsEntities = _iSetting.All();
                var pSettingsEntityiAccessLocation = lSettingsEntities.Where(x => x.Section.Trim().ToLower().Equals("imageservice") && x.Item.Trim().ToLower().Equals("iaccesslocation")).FirstOrDefault();
                var pSettingsEntityLocation = lSettingsEntities.Where(x => x.Section.Trim().ToLower().Equals("imageservice") && x.Item.Trim().ToLower().Equals("location")).FirstOrDefault();
                string DefaultSettingId = "";
                bool PrintingFooter = false;
                bool RenameOnScan = false;
                string SlimLocation = "";
                string CustomeLocation = "";
                if (pSystemEntity is not null)
                {
                    DefaultSettingId = pSystemEntity.DefaultOutputSettingsId;
                    PrintingFooter = (bool)pSystemEntity.PrintImageFooter;
                    RenameOnScan = (bool)pSystemEntity.RenameOnScan;
                }
                if (pSettingsEntityiAccessLocation is not null)
                {
                    SlimLocation = pSettingsEntityiAccessLocation.ItemValue;
                }
                if (pSettingsEntityLocation is not null)
                {
                    CustomeLocation = pSettingsEntityLocation.ItemValue;
                }
                Keys.ErrorType = "s";
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage,
                    defaultsettingid = DefaultSettingId,
                    printimgfooter = PrintingFooter,
                    renameonscan = RenameOnScan,
                    slimlocation = SlimLocation,
                    customelocation = CustomeLocation
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    errortype = "e",
                    message = Keys.ErrorMessageJS()
                });
            }
        }

        [HttpPost]
        public IActionResult SetAttachmentSettingsEntity(string pDefaultOpSettingsId, bool pPrintImageFooter, string pCustomeLocation, string pImgSlimLocationAddress, bool pRenameOnScan)
        {
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            IRepository<OutputSetting> _iOutputSetting = new Repositories<OutputSetting>();
            IRepository<Setting> _iSetting = new Repositories<Setting>();
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            do
            {
                try
                {

                    var pOutputSettings = _iOutputSetting.All().Where(x => x.Id.Equals(pDefaultOpSettingsId, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    if (true is var arg44 && pOutputSettings.InActive is { } arg43 && arg43 == arg44)
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlOptSettingInActive");
                        break;
                    }

                    var pSystemEntity = _iSystem.All().OrderBy(x => x.Id).FirstOrDefault();
                    var pDefaultOutputSettingName = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals(pDefaultOpSettingsId.Trim().ToLower())).FirstOrDefault();

                    if (pDefaultOutputSettingName is not null)
                    {
                        pSystemEntity.DefaultOutputSettingsId = pDefaultOutputSettingName.Name;
                    }
                    pSystemEntity.RenameOnScan = pRenameOnScan;
                    pSystemEntity.PrintImageFooter = pPrintImageFooter;
                    _iSystem.Update(pSystemEntity);
                    var lSettingsEntities = _iSetting.All();

                    var pSettingsEntityiAccessLocation = lSettingsEntities.Where(x => x.Section.Trim().ToLower().Equals("imageservice") && x.Item.Trim().ToLower().Equals("iaccesslocation")).FirstOrDefault();
                    if (pSettingsEntityiAccessLocation is not null)
                    {
                        pSettingsEntityiAccessLocation.ItemValue = pImgSlimLocationAddress;
                        _iSetting.Update(pSettingsEntityiAccessLocation);
                    }
                    else
                    {
                        pSettingsEntityiAccessLocation = new Setting();
                        pSettingsEntityiAccessLocation.Section = "ImageService";
                        pSettingsEntityiAccessLocation.Item = "iAccessLocation";
                        pSettingsEntityiAccessLocation.ItemValue = pImgSlimLocationAddress;
                        _iSetting.Add(pSettingsEntityiAccessLocation);
                    }


                    var pSettingsEntityLocation = lSettingsEntities.Where(x => x.Section.Trim().ToLower().Equals("imageservice") && x.Item.Trim().ToLower().Equals("location")).FirstOrDefault();
                    if (pSettingsEntityLocation is not null)
                    {
                        pSettingsEntityLocation.ItemValue = pCustomeLocation;
                        _iSetting.Update(pSettingsEntityLocation);
                    }
                    else
                    {
                        pSettingsEntityLocation = new Setting();
                        pSettingsEntityLocation.Section = "ImageService";
                        pSettingsEntityLocation.Item = "Location";
                        pSettingsEntityLocation.ItemValue = pCustomeLocation;
                        _iSetting.Add(pSettingsEntityLocation);
                    }
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminAttachmentSettingsSave"); // Keys.ErrorMessage = Keys.SaveSuccessMessage() ' Fixed : FUS-5948
                }
                catch (Exception)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        public IActionResult SetOutputSettingsEntity(OutputSetting pOutputSettingEntity, string DirName, bool pInActive)
        {
            IRepository<OutputSetting> _iOutputSetting = new Repositories<OutputSetting>();
            IRepository<TabFusionRMS.Models.Directory> _iDirectory = new Repositories<TabFusionRMS.Models.Directory>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            do
            {
                try
                {
                    string eMsg = string.Format(Languages.get_Translation("msgAdminCtrlAvailableDocNo"), Strings.Format(int.MaxValue, "#,###"));
                    if (pOutputSettingEntity.NextDocNum is not null)
                    {
                        int NextdocNum = Convert.ToInt32(pOutputSettingEntity.NextDocNum);
                        if (NextdocNum <= 0.0d | NextdocNum > (double)int.MaxValue)
                        {
                            return Json(new
                            {
                                errortype = "e",
                                message = eMsg
                            });
                        }
                    }
                    else
                    {
                        return Json(new
                        {
                            errortype = "e",
                            message = eMsg
                        });
                    }

                    //var BaseWebPageMain = new BaseWebPage();
                    var oSecureObjectMain = new Smead.Security.SecureObject(passport);

                    _iOutputSetting.BeginTransaction();
                    _iSecureObject.BeginTransaction();
                    int pSecureObjectID = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals("output settings")).FirstOrDefault().SecureObjectID;

                    if (pOutputSettingEntity.DefaultOutputSettingsId > 0)
                    {
                        int countSecureObject = _iSecureObject.All().Where(x => (x.Name.Trim().ToLower()) == (DirName.Trim().ToLower()) && x.BaseID.Equals(pSecureObjectID)).Count();
                        if (countSecureObject > 1)
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = Keys.AlreadyExistMessage(DirName);
                            break;
                        }
                        else if (_iOutputSetting.All().Any(x => (x.FileNamePrefix) == (pOutputSettingEntity.FileNamePrefix) && x.DefaultOutputSettingsId != pOutputSettingEntity.DefaultOutputSettingsId))
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlThePrefix"), pOutputSettingEntity.FileNamePrefix);
                            break;
                        }
                        else
                        {
                            if (pOutputSettingEntity.DirectoriesId is null)
                            {
                                pOutputSettingEntity.DirectoriesId = 0;
                            }
                            pOutputSettingEntity.Id = DirName.Trim();
                            if (pInActive == true)
                            {
                                pOutputSettingEntity.InActive = false;
                            }
                            else
                            {
                                pOutputSettingEntity.InActive = true;
                            }
                            _iOutputSetting.Update(pOutputSettingEntity);
                        }
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminUpdateOutputSettings"); // Fixed : FUS-5949
                    }
                    else
                    {
                        if (_iSecureObject.All().Any(x => (x.Name.Trim().ToLower()) == (DirName.Trim().ToLower()) && x.BaseID.Equals(pSecureObjectID)) == false)
                        {
                            if (_iOutputSetting.All().Any(x => (x.FileNamePrefix) == (pOutputSettingEntity.FileNamePrefix)))
                            {
                                Keys.ErrorType = "w";
                                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlThePrefix"), pOutputSettingEntity.FileNamePrefix);
                                break;
                            }
                            else
                            {

                                oSecureObjectMain.Register(DirName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.OutputSettings, (int)Enums.SecureObjects.OutputSettings);


                                // Dim pSecureObjectEntity As New SecureObject

                                // pSecureObjectEntity.Name = DirName
                                // pSecureObjectEntity.SecureObjectTypeID = pSecureObjectID
                                // pSecureObjectEntity.BaseID = pSecureObjectID
                                // _iSecureObject.Add(pSecureObjectEntity)

                                // pOutputSettingEntity.DefaultOutputSettingsId = pSecureObjectEntity.SecureObjectID
                                var pDirectoryEntities = _iDirectory.All();
                                int pDirectoriesId = 0;
                                pDirectoryEntities = pDirectoryEntities.Where(x => x.VolumesId == pOutputSettingEntity.VolumesId);
                                if (pDirectoryEntities.Count() > 0)
                                {
                                    pDirectoriesId = pDirectoryEntities.FirstOrDefault().Id;
                                }

                                pOutputSettingEntity.DirectoriesId = pDirectoriesId;
                                pOutputSettingEntity.Id = DirName.Trim();
                                if (pInActive == true)
                                {
                                    pOutputSettingEntity.InActive = false;
                                }
                                else
                                {
                                    pOutputSettingEntity.InActive = true;
                                }
                                _iOutputSetting.Add(pOutputSettingEntity);
                            }
                        }
                        else
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = Keys.AlreadyExistMessage(DirName);
                            break;
                        }
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminSaveOutputSettings");
                    } // Keys.ErrorMessage = Keys.SaveSuccessMessage() ' Fixed : FUS-5947
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                            Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                    _iOutputSetting.RollBackTransaction();
                    _iSecureObject.RollBackTransaction();
                }
            }
            while (false);
            _iOutputSetting.CommitTransaction();
            _iSecureObject.CommitTransaction();
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        public IActionResult EditOutputSettingsEntity([FromForm] string[] pRowSelected)
        {
            if (pRowSelected is null)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
            }

            string pOutputSettingsId = pRowSelected.GetValue(0).ToString();
            if (string.IsNullOrWhiteSpace(pOutputSettingsId))
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
            }
            IRepository<OutputSetting> _iOutputSetting = new Repositories<OutputSetting>();
            var pOutputSettingsEntity = _iOutputSetting.All().Where(x => x.Id.ToString().Trim().ToLower().Equals(pOutputSettingsId.Trim().ToLower())).FirstOrDefault();
            var Setting = new JsonSerializerSettings();
            string jsonObject = "";
            string pFileName = "";
            if (pOutputSettingsEntity is not null)
            {
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                if (false is var arg48 && pOutputSettingsEntity.InActive is { } arg47 && arg47 == arg48)
                {
                    pOutputSettingsEntity.InActive = true;
                }
                else
                {
                    pOutputSettingsEntity.InActive = false;
                }

                jsonObject = JsonConvert.SerializeObject(pOutputSettingsEntity, Newtonsoft.Json.Formatting.Indented, Setting);
                pFileName = Convert.ToString(SetExampleFileName(Convert.ToString(pOutputSettingsEntity.NextDocNum.Value), pOutputSettingsEntity.FileNamePrefix, pOutputSettingsEntity.FileExtension));
            }
            Keys.ErrorType = "s";
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                jsonObject,
                fileName = pFileName
            });
        }

        [HttpPost]
        public IActionResult RemoveOutputSettingsEntity([FromForm] string[] pRowSelected)
        {
            if (pRowSelected is null)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
            }

            int lSecureObjectId;
            var oSecureObjectMain = new Smead.Security.SecureObject(passport);
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();

            try
            {

                string pOutputSettingsId = pRowSelected.GetValue(0).ToString();
                if (string.IsNullOrWhiteSpace(pOutputSettingsId))
                {
                    return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
                }

                var pSystemEntity = _iSystem.All().OrderBy(x => x.Id).FirstOrDefault();
                if (pSystemEntity is not null)
                {
                    var pSecureObjectEntity = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals(pOutputSettingsId.ToString().Trim().ToLower())).FirstOrDefault();
                    // Dim pSecureObjectEntity = _iSecureObject.All().Where(Function(x) x.SecureObjectID = pOutputSettingsId).FirstOrDefault()

                    if (pSystemEntity.DefaultOutputSettingsId.Equals(pOutputSettingsId, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlOptSettingCantBeUsed"), pSecureObjectEntity.Name);
                    }
                    else
                    {
                        IRepository<OutputSetting> _iOutputSetting = new Repositories<OutputSetting>();
                        var pOutputSettingsEntity = _iOutputSetting.All().Where(x => x.Id.Trim().ToLower().Equals(pOutputSettingsId.ToString().Trim().ToLower())).FirstOrDefault();
                        if (pOutputSettingsEntity is not null)
                        {
                            _iOutputSetting.Delete(pOutputSettingsEntity);
                        }
                        if (pSecureObjectEntity is not null)
                        {
                            lSecureObjectId = oSecureObjectMain.GetSecureObjectID(pSecureObjectEntity.Name, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.OutputSettings);
                            if (lSecureObjectId != 0)
                                oSecureObjectMain.UnRegister(lSecureObjectId);
                            // _iSecureObject.Delete(pSecureObjectEntity)
                        }

                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminDeleteOutputSettings");
                    } // Keys.ErrorMessage = Keys.DeleteSuccessMessage() ' Fixed: FUS-5950
                }
                else
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlNoRecordFoundInSystem");
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public object SetExampleFileName(string pNextDocNum, string pFileNamePrefix, string pFileExtension)
        {
            string sBase36;
            if (Convert.ToDouble(pNextDocNum) >= 0.0d & Convert.ToDouble(pNextDocNum) <= int.MaxValue)
            {
                sBase36 = Convert10to36(Convert.ToDouble(pNextDocNum));
            }
            else
            {
                sBase36 = "";
            }
            return Strings.Trim(pFileNamePrefix) + Strings.Trim(sBase36) + "." + Strings.Trim(pFileExtension.TrimStart('.'));
        }

        public string Convert10to36(double dValue)
        {
            string Convert10to36Ret = default;
            double dRemainder;
            string sResult;
            sResult = "";
            dValue = Math.Abs(dValue);
            do
            {
                dRemainder = dValue - 36d * Conversion.Int(dValue / 36d);
                sResult = Strings.Mid("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", (int)Math.Round(dRemainder + 1d), 1) + sResult;
                dValue = Conversion.Int(dValue / 36d);
            }
            while (dValue > 0d);
            // Convert10to36 = InStr(6 - Len(sResult), "0") & sResult
            Convert10to36Ret = sResult.PadLeft(6, '0');
            return Convert10to36Ret;
        }

        #endregion

        #region Auditing
        public PartialViewResult LoadAuditingView()
        {
            return PartialView("_AuditingPartial");
        }

        public IActionResult GetTablesForLabel()
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                var lTableEntites = from t in _iTable.All().OrderBy(m => m.TableName)
                                    select new { t.AuditUpdate, t.UserName, t.AuditAttachments, t.AuditConfidentialData };

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string jsonObject = JsonConvert.SerializeObject(lTableEntites, Newtonsoft.Json.Formatting.Indented, Setting);

                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage,
                    ltablelist = jsonObject
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, errortype = "e", message = Keys.ErrorMessageJS() });
            }
        }

        public IActionResult GetAuditPropertiesData(int pTableId)
        {
            if (pTableId == -1)
            {
                return Json(new { success = false, errortype = "e", message = Languages.get_Translation("msgAdminCtrlTblIdNotFound") });
            }
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                var pTableEntites = _iTable.All().Where(x => x.TableId == pTableId).FirstOrDefault();
                if (pTableEntites is null)
                {
                    return Json(new { success = false, errortype = "e", message = Languages.get_Translation("msgAdminCtrlTblRecNotFound") });
                }
                var pRelationShipEntites = _iRelationShip.All().Where(x => x.UpperTableName.Trim().ToLower().Equals(pTableEntites.TableName.Trim().ToLower()));
                if (pRelationShipEntites is null)
                {
                    return Json(new { success = false, errortype = "e", message = Languages.get_Translation("msgAdminCtrlRelationRecNotFound") });
                }
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlSuccess");
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage,
                    isconfchecked = pTableEntites.AuditConfidentialData,
                    isupdatechecked = pTableEntites.AuditUpdate,
                    isattachchecked = pTableEntites.AuditAttachments,
                    confenabled = pRelationShipEntites.Count() > 0? false: true,
                    attachenabled = (pTableEntites.Attachments == true)? false: true
                });
            }
            catch (Exception)
            {
                return Json(new { success = false, errortype = "e", message = Keys.ErrorMessageJS() });
            }
        }

        [HttpPost]
        public IActionResult SetAuditPropertiesData(int pTableId, bool pAuditConfidentialData, bool pAuditUpdate, bool pAuditAttachments, bool pIsChild)
        {
            var lTableIds = new List<int>();
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                var pTableEntity = _iTable.All().Where(x => x.TableId.Equals(pTableId)).FirstOrDefault();
                pTableEntity.AuditConfidentialData = pAuditConfidentialData;
                pTableEntity.AuditUpdate = pAuditUpdate;
                pTableEntity.AuditAttachments = pAuditAttachments;
                _iTable.Update(pTableEntity);
                if (pIsChild)
                {
                    foreach (var pTableIdeach in BLAuditing.GetChildTableIds(pTableEntity.TableName, _IDBManager, httpContext))
                    {
                        pTableEntity = _iTable.All().Where(x => x.TableId.Equals(pTableIdeach)).FirstOrDefault();
                        if(pTableEntity.AuditConfidentialData == false && pTableEntity.AuditUpdate == false && pTableEntity.AuditAttachments == false)
                        {
                            pTableEntity.AuditUpdate = pAuditUpdate;
                            _iTable.Update(pTableEntity);
                            lTableIds.Add(pTableIdeach);
                        }
                    }
                }

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAuditdualboxSuccess");
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                ltableids = lTableIds
            });
        }

        [HttpPost]
        public IActionResult RemoveTableFromList([FromForm] string[] pTableIds)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                foreach (string item in pTableIds)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        int pTableId = Convert.ToInt32(item);
                        var pTableEntity = _iTable.All().Where(x => x.TableId.Equals(pTableId)).FirstOrDefault();
                        pTableEntity.AuditConfidentialData = false;
                        pTableEntity.AuditUpdate = false;
                        pTableEntity.AuditAttachments = false;
                        _iTable.Update(pTableEntity);
                    }
                }
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlMoveTblSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        public IActionResult PurgeAuditData(DateTime pPurgeDate, bool pUpdateData, bool pConfData, bool pSuccessLoginData, bool pFailLoginData)
        {
            IRepository<SLAuditUpdate> _iSLAuditUpdate = new Repositories<SLAuditUpdate>();
            IRepository<SLAuditUpdChildren> _iSLAuditUpdChildren = new Repositories<SLAuditUpdChildren>();
            IRepository<SLAuditConfData> _iSLAuditConfData = new Repositories<SLAuditConfData>();
            IRepository<SLAuditFailedLogin> _iSLAuditFailedLogin = new Repositories<SLAuditFailedLogin>();
            IRepository<SLAuditLogin> _iSLAuditLogin = new Repositories<SLAuditLogin>();
            try
            {
                bool bRecordExist = false;

                if (pUpdateData == true)
                {
                    var lSLAuditUpdateEntities = _iSLAuditUpdate.All().Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.UpdateDateTime) < pPurgeDate);

                    var ids = new HashSet<int>(lSLAuditUpdateEntities.Select(x => x.Id));

                    var lSLAuditUpdChildrenEntities = _iSLAuditUpdChildren.All().Where(x => ids.Contains((int)x.SLAuditUpdatesId));
                    if (lSLAuditUpdChildrenEntities is not null)
                    {
                        if (lSLAuditUpdChildrenEntities.Count() > 0)
                        {
                            bRecordExist = true;
                            _iSLAuditUpdChildren.DeleteRange(lSLAuditUpdChildrenEntities);
                            _iSLAuditUpdate.DeleteRange(lSLAuditUpdateEntities);
                        }
                    }
                }

                if (pConfData == true)
                {
                    var lSLAuditConfDataEntities = _iSLAuditConfData.All().Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.AccessDateTime) < pPurgeDate);
                    if (lSLAuditConfDataEntities is not null)
                    {
                        if (lSLAuditConfDataEntities.Count() > 0)
                        {
                            bRecordExist = true;
                            _iSLAuditConfData.DeleteRange(lSLAuditConfDataEntities);
                        }
                    }
                }

                if (pSuccessLoginData == true)
                {
                    var lSLAuditLoginEntities = _iSLAuditLogin.All().Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.LoginDateTime) < pPurgeDate);
                    if (lSLAuditLoginEntities is not null)
                    {
                        if (lSLAuditLoginEntities.Count() > 0)
                        {
                            bRecordExist = true;
                            _iSLAuditLogin.DeleteRange(lSLAuditLoginEntities);
                        }
                    }
                }

                if (pFailLoginData == true)
                {
                    var lSLAuditFailedLoginEntities = _iSLAuditFailedLogin.All().Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.LoginDateTime) < pPurgeDate);
                    if (lSLAuditFailedLoginEntities is not null)
                    {
                        if (lSLAuditFailedLoginEntities.Count() > 0)
                        {
                            bRecordExist = true;
                            _iSLAuditFailedLogin.DeleteRange(lSLAuditFailedLoginEntities);
                        }
                    }
                }


                if (bRecordExist == true)
                {
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Keys.PurgeSuccessMessage();
                }
                else
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlAuditPurgeNodata");
                }
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public IActionResult CheckChildTableExist(int pTableId)
        {
            bool bChildExist = false;
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                var oTable = _iTable.All().Where(x => x.TableId == pTableId).FirstOrDefault();
                if (oTable is not null)
                {
                    if (BLAuditing.GetChildTableIds(oTable.TableName.Trim(), _IDBManager, httpContext).Count > 0)
                    {
                        bChildExist = true;
                    }
                }

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Keys.SaveSuccessMessage();
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                childexist = bChildExist
            });
        }
        #endregion

        #region Bar Code Search Order

        public PartialViewResult LoadBarCodeSearchView()
        {
            this.ViewBag.BarCodeTableList = Common.DefaultDropdownSelectionBlank;
            return PartialView("_BarCodeSearchPartial");
        }

        public JsonResult GetBarCodeList(string sord, int page, int rows)
        {
            IRepository<ScanList> _iScanList = new Repositories<ScanList>();
            var pBarCideEntities = _iScanList.All();
            IRepository<Table> _iTable = new Repositories<Table>();
            var pTable = _iTable.All();

            var oBarCodeEntities = new List<ScanList>();

            foreach (ScanList scan in pBarCideEntities)
            {
                if (!string.IsNullOrEmpty(scan.TableName))
                {
                    oBarCodeEntities.Add(scan);
                }
            }
            var q = (from sc in oBarCodeEntities
                     join ta in pTable.ToList()
                   on sc.TableName.Trim().ToLower() equals ta.TableName.Trim().ToLower()
                     select new
                     {
                         sc.Id,
                         sc.IdMask,
                         sc.IdStripChars,
                         sc.ScanOrder,
                         sc.TableName,
                         sc.FieldName,
                         sc.FieldType,
                         ta.UserName
                     }
                    ).AsQueryable();

            //Select ta.UserName, sc



            pBarCideEntities = pBarCideEntities.OrderBy(x => x.ScanOrder);

            var jsonData = q.GetJsonListForGrid(sord, page, rows, "ScanOrder");

            return Json(jsonData);
        }

        [HttpPost]
        public IActionResult RemoveBarCodeSearchEntity([FromForm] int pId, [FromForm] int scan)
        {

            IRepository<ScanList> _iScanList = new Repositories<ScanList>();
            try
            {
                var pBarCodeRemovedEntity = _iScanList.All().Where(x => x.Id == pId).FirstOrDefault();
                _iScanList.Delete(pBarCodeRemovedEntity);

                var pScanListEntityGreater = _iScanList.All().Where(x => x.ScanOrder > scan);

                if (pScanListEntityGreater.Count() == 0 == false)
                {
                    foreach (ScanList pScanList in pScanListEntityGreater.ToList())
                    {
                        pScanList.ScanOrder = (short?)(pScanList.ScanOrder - 1);
                        _iScanList.Update(pScanList);
                    }
                }

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminSearchOrderDelete"); // Fixed: FUS-5998
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        public IActionResult SetbarCodeSearchEntity([FromForm] int Id, [FromForm] string FieldName, [FromForm] int scanOrder,
            [FromForm] string TableName, [FromForm] string IdStripChars, [FromForm] string IdMask)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<ScanList> _iScanList = new Repositories<ScanList>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            var pBarCodeSearchEntity = new ScanList()
            {
                Id = Id,
                FieldName = FieldName,
                TableName = TableName,
                IdStripChars = IdStripChars,
                IdMask = IdMask
            };


            var conObj = DataServices.DBOpen(Enums.eConnectionType.conDefault);

            var oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pBarCodeSearchEntity.TableName, conObj, pBarCodeSearchEntity.FieldName);

            if (oSchemaColumns.Count == 0)
            {
                var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pBarCodeSearchEntity.TableName.Trim().ToLower())).FirstOrDefault();
                conObj = DataServices.DBOpen(oTables, _iDatabas.All());
                oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pBarCodeSearchEntity.TableName, conObj, pBarCodeSearchEntity.FieldName);
            }

            try
            {
                if (pBarCodeSearchEntity.Id > 0)
                {
                    if (_iScanList.All().Any(x => (x.TableName) == (pBarCodeSearchEntity.TableName) && (x.FieldName) == (pBarCodeSearchEntity.FieldName) && x.Id != pBarCodeSearchEntity.Id))
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Keys.AlreadyExistMessage(pBarCodeSearchEntity.TableName);
                    }
                    else
                    {
                        var pSystemEntity = _iScanList.All().Where(x => x.Id == pBarCodeSearchEntity.Id).FirstOrDefault();
                        pSystemEntity.TableName = pBarCodeSearchEntity.TableName;
                        pSystemEntity.FieldName = pBarCodeSearchEntity.FieldName;
                        pSystemEntity.FieldType = Convert.ToInt16(oSchemaColumns[0].DataType);
                        pSystemEntity.IdStripChars = pBarCodeSearchEntity.IdStripChars;
                        pSystemEntity.IdMask = pBarCodeSearchEntity.IdMask;
                        _iScanList.Update(pSystemEntity);
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminSearchOrderEdit");
                    }  // Fixed: FUS-5997
                }
                else if (_iScanList.All().Any(x => (x.TableName) == (pBarCodeSearchEntity.TableName) && (x.FieldName) == (pBarCodeSearchEntity.FieldName)))
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Keys.AlreadyExistMessageAddBarCode(pBarCodeSearchEntity.TableName);
                }
                else
                {
                    pBarCodeSearchEntity.ScanOrder = (short?)(scanOrder + 1);
                    pBarCodeSearchEntity.FieldType = Convert.ToInt16(oSchemaColumns[0].DataType);
                    _iScanList.Add(pBarCodeSearchEntity);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminSearchOrderSave");
                }  // Fixed: FUS-5996
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        #endregion

        #region Background Process

        public PartialViewResult LoadBackgroundProcessView()
        {
            return PartialView("_BackgroundProcessPartial");
        }

        public JsonResult GetBackgroundProcess(string sord, int page, int rows)
        {
            IRepository<LookupType> _iLookupType = new Repositories<LookupType>();
            try
            {
                var oDataTable = new DataTable();
                var BackgroundOption = from p in _iLookupType.Where(m => m.LookupTypeForCode.Trim().ToUpper().Equals("BGPCS".Trim()))
                                       select p.LookupTypeValue;
                var oBackGroundOption = BackgroundOption.ToList();
                if (BackgroundOption.Count() > 0)
                {
                    IRepository<Setting> _iSetting = new Repositories<Setting>();
                    var oSettingList = from s in _iSetting.All()
                                       where oBackGroundOption.Contains(s.Section)
                                       select s;
                    oDataTable.Columns.Add(new DataColumn("Id"));
                    oDataTable.Columns.Add(new DataColumn("Section"));
                    oDataTable.Columns.Add(new DataColumn("MinValue"));
                    oDataTable.Columns.Add(new DataColumn("MaxValue"));
                    if (oSettingList is not null)
                    {
                        int oId = 0;
                        DataRow[] foundRows;
                        foreach (var oItem in oSettingList.ToList())
                        {
                            if (oDataTable.Rows.Count != 0)
                            {
                                foundRows = oDataTable.Select("Section = '" + oItem.Section.Trim() + "'");
                                if (foundRows.Length != 0)
                                {
                                    foundRows[0][oItem.Item] = oItem.ItemValue;
                                }
                                else
                                {
                                    var dr = oDataTable.NewRow();
                                    oId = oId + 1;
                                    dr["Id"] = oId;
                                    dr["Section"] = oItem.Section;
                                    dr[oItem.Item] = oItem.ItemValue;
                                    oDataTable.Rows.Add(dr);
                                }
                            }
                            else
                            {
                                var dr = oDataTable.NewRow();
                                oId = oId + 1;
                                dr["Id"] = oId;
                                dr["Section"] = oItem.Section;
                                dr[oItem.Item] = oItem.ItemValue;
                                oDataTable.Rows.Add(dr);
                            }
                        }
                    }
                }
                // Dim oSettingList = _iSetting.Where(Function(m) m.Section.Trim().ToLower().Equals(("BackgroundTransfer").Trim().ToLower()) Or m.Section.Trim().ToLower().Equals(("BackgroundExport").Trim().ToLower()))

                return Json(Common.ConvertDataTableToJQGridResult(oDataTable, "", sord, page, rows));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public JsonResult GetBackgroundOptions()
        {
            string BackgroundOptionJSON = "";
            IRepository<LookupType> _iLookupType = new Repositories<LookupType>();
            try
            {
                var BackgroundOption = _iLookupType.Where(m => m.LookupTypeForCode.Trim().ToUpper().Equals("BGPCS".Trim()));
                var lstBackgroundItems = new List<KeyValuePair<string, string>>();
                var BackgroundOptionList = BackgroundOption.ToList();
                foreach (LookupType oitem in BackgroundOptionList)
                    lstBackgroundItems.Add(new KeyValuePair<string, string>(oitem.LookupTypeValue, oitem.LookupTypeValue));
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                BackgroundOptionJSON = JsonConvert.SerializeObject(lstBackgroundItems, Newtonsoft.Json.Formatting.Indented, Setting);
                Keys.ErrorType = "s";
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
            }
            return Json(new
            {
                BackgroundOptionJSON,
                errorType = Keys.ErrorType
            });
        }

        [HttpPost]
        //[ValidateInput(false)]
        public JsonResult SetBackgroundData()
        {
            string ReturnMessage = string.Empty;
            Keys.ErrorType = "s";
            IRepository<Setting> _iSetting = new Repositories<Setting>();
            do
            {
                try
                {
                    var forms = httpContext.Request.Form;
                    string data = forms["x01"];
                    var jsonObject = JsonConvert.DeserializeObject<JObject>(data);
                    string oId = jsonObject.GetValue("Id").ToString();
                    string oSection = jsonObject.GetValue("Section").ToString();
                    int oMinValue = Convert.ToInt32(jsonObject.GetValue("MinValue").ToString());
                    if (oMinValue == 0)
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Languages.get_Translation("msgBackgroundMinValGreaterZero");
                        break;
                    }
                    int oMaxValue = Convert.ToInt32(jsonObject.GetValue("MaxValue").ToString());
                    var oMinValueItem = _iSetting.Where(m => m.Section.Trim().ToLower().Equals(oSection.Trim().ToLower()) & m.Item.Trim().ToLower().Equals("minvalue")).FirstOrDefault();
                    if (oMinValueItem is not null)
                    {
                        if (oId.Contains("jqg"))
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgBackgroundUpdateAlreadyAddedRow"), oMinValueItem.Section);
                            break;
                        }
                        else
                        {
                            oMinValueItem.ItemValue = oMinValue.ToString();
                            _iSetting.Update(oMinValueItem);
                        }
                    }
                    else
                    {
                        oMinValueItem = new Setting();
                        oMinValueItem.Section = oSection.Trim();
                        oMinValueItem.Item = "MinValue";
                        oMinValueItem.ItemValue = oMinValue.ToString();
                        _iSetting.Add(oMinValueItem);
                    }

                    var oMaxValueItem = _iSetting.Where(m => m.Section.Trim().ToLower().Equals(oSection.Trim().ToLower()) & m.Item.Trim().ToLower().Equals("maxvalue")).FirstOrDefault();
                    if (oMaxValueItem is not null)
                    {
                        if (oId.Contains("jqg"))
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgBackgroundUpdateAlreadyAddedRow"), oMinValueItem.Section);
                            break;
                        }
                        else
                        {
                            oMaxValueItem.ItemValue = oMaxValue.ToString();
                            _iSetting.Update(oMaxValueItem);
                        }
                    }
                    else
                    {
                        oMaxValueItem = new Setting();
                        oMaxValueItem.Section = oSection.Trim();
                        oMaxValueItem.Item = "MaxValue";
                        oMaxValueItem.ItemValue = oMaxValue.ToString();
                        _iSetting.Add(oMaxValueItem);
                    }
                    if (Keys.ErrorType == "s")
                        Keys.ErrorMessage = Languages.get_Translation("msgBackgroundUpdateSuccessMsg");
                }
                catch (Exception ex)
                {
                    Keys.ErrorType = "e";
                    if (ex.Message == "Value was either too large or too small for an Int32.")
                    {
                        Keys.ErrorMessage = Languages.get_Translation("msgBackgroundLargeValueErrorMsg");
                    }
                    else
                    {
                        Keys.ErrorMessage = ex.Message;
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

        public JsonResult RemoveBackgroundSection(string SectionArrayObject)
        {
            try
            {
                IRepository<Setting> _iSetting = new Repositories<Setting>();
                var SectionArrayObjectDes = JsonConvert.DeserializeObject<object>(SectionArrayObject);
                foreach (string oStr in (IEnumerable)SectionArrayObjectDes)
                {
                    var oSetting = _iSetting.Where(m => m.Section.Trim().ToLower().Equals(oStr.Trim().ToLower()));
                    _iSetting.DeleteRange(oSetting);
                }
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgBackgroundSectionDltSuccess");
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
        public IActionResult DeleteBackgroundProcessTasks(DateTime pBGEndDate, bool pchkBGStatusCompleted, bool pchkBGStatusError)
        {
            try
            {
                var lstOfStatus = new List<string>();
                if (pchkBGStatusCompleted == true)
                {
                    lstOfStatus.Add("Completed");
                }
                else if (pchkBGStatusError == true)
                {
                    lstOfStatus.Add("Error");
                }

                if (lstOfStatus is not null)
                {
                    string status = "'" + string.Join("','", lstOfStatus) + "'";
                    status = status.Replace("\"", "");
                    string endDate = "'" + pBGEndDate.ToString("yyyy-MM-dd") + "'";

                    string conString = Keys.get_GetDBConnectionString();
                    // added by moti mashiah need to discuss with Reggie.
                    using (var con1 = new SqlConnection(conString))
                    {
                        string qsqlpath = "select ReportLocation, DownloadLocation from SLServiceTasks WHERE Convert(Date, StartDate, 101) <= " + endDate + " AND Status IN (" + status + ")";
                        var command = new SqlCommand(qsqlpath, con1);
                        var adp = new SqlDataAdapter(command);
                        var dTable = new DataTable();
                        int datat = adp.Fill(dTable);
                        foreach (DataRow row in dTable.Rows)
                        {
                            string transferFile = row["ReportLocation"].ToString();
                            if (!string.IsNullOrEmpty(transferFile))
                            {
                                System.IO.File.Delete(transferFile);
                            }
                            string CsvFile = row["DownloadLocation"].ToString();
                            if (!string.IsNullOrEmpty(CsvFile))
                            {
                                System.IO.File.Delete(CsvFile);
                            }

                        }
                    }
                    using (var con = new SqlConnection(conString))
                    {
                        con.Open();
                        if (Convert.ToBoolean(con.State))
                        {
                            string sSql = "DELETE From SLServiceTaskItems WHERE SLServiceTaskId In (SELECT Id FROM SLServiceTasks WHERE Convert(Date, EndDate, 101) <= " + endDate + " AND Status IN (" + status + ")); DELETE From SLServiceTasks WHERE Convert(Date, EndDate, 101) <= " + endDate + " AND Status IN (" + status + ")";
                            _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
                            _IDBManager.ExecuteNonQuery(CommandType.Text, sSql);
                            _IDBManager.Dispose();
                        }
                        con.Close();
                    }
                }

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminBGCtrlRecDeletedSuccessfully");
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        #endregion


        #region Database

        #region External Database
        public PartialViewResult LoadExternalDBView()
        {
            return PartialView("_ExternalDBPartial");
        }

        public PartialViewResult LoadAddDBView()
        {
            return PartialView("_AddNewDatabasePartial");
        }

        public IActionResult GetAllSQLInstance()
        {

            // Retrieve the installed providers and factories.
            //DataTable table = System.Data.SqlClient.SqlClientFactory.Instance.DbProviderFactories.GetFactoryClasses();

            // Display each row and column value.
            //foreach (DataRow row in table.Rows)
            //{
            //    foreach (DataColumn column in table.Columns)
            //    {
            //        Console.WriteLine(row[column]);
            //    }
            //}
            

            //DataTable tablee = Microsoft.SqlServer.Management.Smo.SmoApplication.EnumAvailableSqlServers(true);
            //string ServerName = Environment.MachineName;
            //foreach (DataRow row in tablee.Rows)
            //{
            //    Console.WriteLine(ServerName + "\\" + row["InstanceName"].ToString());
            //}

            // check raju getDataSourceInstacne

            Microsoft.Data.SqlClient.SqlClientFactory newFactory = Microsoft.Data.SqlClient.SqlClientFactory.Instance;
            System.Data.Common.DbDataAdapter cmd = newFactory.CreateDataAdapter();

            DataTable dataTable = new DataTable();
            //https://stackoverflow.com/questions/67577933/the-name-sqldatasourceenumerator-does-not-exist-in-the-current-context
            System.Data.Common.DbProviderFactory factory = SqlClientFactory.Instance;
            if (factory.CanCreateDataSourceEnumerator)
            {
                System.Data.Common.DbDataSourceEnumerator instance =
                    factory.CreateDataSourceEnumerator();
                DataTable table = instance.GetDataSources();

                foreach (DataRow row in table.Rows)
                {
                    Console.WriteLine("{0}\\{1}",
                        row["ServerName"], row["InstanceName"]);
                }
            }

            var lstSQLInstances = new string[dataTable.Rows.Count + 1];
            int i = 1;
            lstSQLInstances[0] = "";
            foreach (DataRow dr in dataTable.Rows)
            {
                lstSQLInstances[i] = string.Concat(dr["ServerName"], @"\", dr["InstanceName"]);
                i = i + 1;
            }

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(lstSQLInstances, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);

        }

        public bool IsUserDatabase(string DataBaseName)
        {
            bool IsUserDatabaseRet = default;
            IsUserDatabaseRet = false;

            if (DataBaseName.Trim().ToLower().Equals("master"))
            {
                IsUserDatabaseRet = true;
            }
            if (DataBaseName.Trim().ToLower().Equals("msdb"))
            {
                IsUserDatabaseRet = true;
            }
            if (DataBaseName.Trim().ToLower().Equals("model"))
            {
                IsUserDatabaseRet = true;
            }
            if (DataBaseName.Trim().ToLower().Equals("tempdb"))
            {
                IsUserDatabaseRet = true;
            }

            return IsUserDatabaseRet;
        }

        public IActionResult GetDatabaseList(string instance, string userID, string pass)
        {
            var list = new List<string>();
            string conString;
            var Setting = new JsonSerializerSettings();
            // Open connection to the database
            if (userID.Equals("") | pass.Equals(""))
            {
                conString = "Data Source=" + instance + ";Persist Security Info=True;Integrated Security=SSPI;";
            }
            else
            {
                conString = "Data Source=" + instance + ";User ID=" + userID + ";Password=" + pass + ";Persist Security Info=True;MultipleActiveResultSets=True;";
            }
            using (var con = new SqlConnection(conString))
            {
                try
                {
                    con.Open();
                    // Set up a command with the given query and associate
                    // this with the current connection.
                    using (var cmd = new SqlCommand("SELECT name from sys.databases", con))
                    {
                        using (IDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                if (IsUserDatabase(dr[0].ToString()) == false)
                                {
                                    list.Add(dr[0].ToString());
                                }
                            }
                        }
                    }
                    Keys.ErrorType = "s";
                }
                // con.Close()

                catch (Exception)
                {
                    if (userID.Equals("") & pass.Equals(""))
                    {
                        Keys.ErrorType = "e";
                        Keys.ErrorMessage = Keys.UnableToConnect(instance);
                    }
                    else
                    {
                        Keys.ErrorType = "e";
                        Keys.ErrorMessage = Keys.LoginFailed();
                    }
                    return Json(new
                    {
                        errortype = Keys.ErrorType,
                        message = Keys.ErrorMessage
                    });
                }
            }
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(new
            {
                jsonObject,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public IActionResult AddNewDB([FromServices] IConfiguration configuration,  Databas pDatabas)
        {
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            _iDatabas.BeginTransaction();
            bool mAdd;

            var pDatabaseEntity = _iDatabas.All().Where(x => (x.DBName.Trim().ToLower()) == (pDatabas.DBName.Trim().ToLower())).FirstOrDefault();

            if (pDatabas.Id > 0)
            {
                mAdd = false;
            }
            else
            {
                mAdd = true;
            }

            int connTime;
            if (pDatabas.DBConnectionTimeout != 0)
            {
                connTime = (int)pDatabas.DBConnectionTimeout;
            }
            else
            {
                connTime = 10;
            }

            if (mAdd)
            {
                if (_iDatabas.All().Any(x => (x.DBName.Trim().ToLower()) == (pDatabas.DBName.Trim().ToLower())))
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlADBConnNamed"), pDatabas.DBName);
                }
                else
                {
                    string conText = "UID=" + pDatabas.DBUserId + ";PWD=" + pDatabas.DBPassword + ";DATABASE=" + pDatabas.DBDatabase + ";Server=" + pDatabas.DBServer;
                    pDatabas.DBConnectionText = conText;
                    pDatabas.DBProvider = Keys.DBProvider;  
                    pDatabas.DBConnectionTimeout = connTime;
                    pDatabas.DBUseDBEngineUIDPWD = false; 
                    _iDatabas.Add(pDatabas);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminExternalDBSave");
                } // FUS-6020
            }
            else if (_iDatabas.All().Any(x => (x.DBName.Trim().ToLower()) == (pDatabas.DBName.Trim().ToLower()) & x.Id != pDatabas.Id))
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlADBConnNamed"), pDatabas.DBName);
            }
            else
            {
                var pDatabase = _iDatabas.All().Where(x => x.Id == pDatabas.Id).FirstOrDefault();
                string instance = pDatabas.DBServer;
                string userID = pDatabas.DBUserId;
                string pass = pDatabas.DBPassword;
                object conString;
                bool valid = true;

                if (userID is null & pass is null)
                {
                    conString = "Data Source=" + instance + ";Persist Security Info=True;Integrated Security=SSPI;";
                }
                else
                {
                    conString = "Data Source=" + instance + ";User ID=" + userID + ";Password=" + pass + ";Persist Security Info=True;MultipleActiveResultSets=True;";
                }
                using (var con = new SqlConnection(Convert.ToString(conString)))
                {
                    try
                    {
                        con.Open();
                    }
                    catch (Exception)
                    {
                        valid = false;
                    }
                }

                if (valid)
                {
                    string conText = "UID=" + pDatabas.DBUserId + ";PWD=" + pDatabas.DBPassword + ";DATABASE=" + pDatabas.DBDatabase + ";Server=" + pDatabas.DBServer;
                    pDatabase.DBName = pDatabas.DBName;
                    pDatabase.DBConnectionText = conText;
                    pDatabase.DBProvider = Keys.DBProvider;
                    pDatabase.DBConnectionTimeout = connTime;
                    pDatabase.DBUseDBEngineUIDPWD =false;
                    pDatabase.DBUserId = pDatabas.DBUserId;
                    pDatabase.DBPassword = pDatabas.DBPassword;
                    pDatabase.DBServer = pDatabas.DBServer;
                    pDatabase.DBDatabase = pDatabas.DBDatabase;

                    _iDatabas.Update(pDatabase);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminExternalDBEdit"); // FUS-6021
                }
                else
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.LoginFailed();
                }

            }
            _iDatabas.CommitTransaction();
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public IActionResult GetAllSavedInstances()
        {
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            var pDatabase = _iDatabas.All();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(pDatabase, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        public IActionResult DisconnectDBCheck(string connName)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            string table = null;
            if (_iTable.All().Any(x => x.DBName.Trim().ToLower().Equals(connName.Trim().ToLower())))
            {

                var pTable = _iTable.All().Where(x => x.DBName.Trim().ToLower().Equals(connName.Trim().ToLower()));

                if (pTable.Count() == 0 == false)
                {
                    foreach (Table tables in pTable.ToList())
                        table = table + "Tables: " + tables.UserName + Constants.vbCrLf;
                }
                Keys.ErrorType = "w";
                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlExtDBIsInUse"), connName, Constants.vbNewLine, table);
                table = null;
            }
            else
            {
                Keys.ErrorType = "s";
                Keys.ErrorMessage = "";
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public IActionResult DisconnectDB(string connName)
        {
            try
            {
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                var pDatabases = _iDatabas.All().Where(x => x.DBName.Trim().ToLower().Equals(connName.Trim().ToLower())).FirstOrDefault();
                _iDatabas.Delete(pDatabases);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminExternalDBDisconnect"); // FUS-6022
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public bool CheckIfDateChanged(Databas pDatabase)
        {
            bool Changed;
            bool mAdd;
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            if (pDatabase.Id > 0)
            {
                mAdd = false;
            }
            else
            {
                mAdd = true;
            }

            if (mAdd)
            {
                if (string.IsNullOrEmpty(pDatabase.DBName) & string.IsNullOrEmpty(pDatabase.DBDatabase) & string.IsNullOrEmpty(pDatabase.DBServer) & string.IsNullOrEmpty(pDatabase.DBPassword) & string.IsNullOrEmpty(pDatabase.DBUserId))
                {
                    Changed = false;
                }
                else
                {
                    Changed = true;
                }
            }

            else
            {
                var mDatabases = _iDatabas.All().Where(x => x.Id == pDatabase.Id).FirstOrDefault();

                Changed = !pDatabase.DBName.Trim().ToLower().Equals(mDatabases.DBName.Trim().ToLower());

                if (mDatabases.DBUseDBEngineUIDPWD == true)
                {
                    Changed = Changed | !pDatabase.DBUserId.Trim().ToLower().Equals(mDatabases.DBUserId.Trim().ToLower());
                    Changed = Changed | !pDatabase.DBPassword.Trim().ToLower().Equals(mDatabases.DBPassword.Trim().ToLower());
                }

                Changed = Changed | !pDatabase.DBDatabase.Trim().ToLower().Equals(mDatabases.DBDatabase.Trim().ToLower());
                Changed = Changed | !pDatabase.DBServer.Trim().ToLower().Equals(mDatabases.DBServer.Trim().ToLower());

                int connTime;
                if (pDatabase.DBConnectionTimeout != 0)
                {
                    connTime = (int)pDatabase.DBConnectionTimeout;
                }
                else
                {
                    connTime = 10;
                }

                if (connTime != mDatabases.DBConnectionTimeout)
                {
                    Changed = true;
                }

                return Changed;
            }
            return Changed;
        }
        #endregion

        #region Map
        // Load Database -> Map screen view
        public PartialViewResult LoadMapView()
        {
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<TabSet> _iTabset = new Repositories<TabSet>();
            IRepository<LookupType> _iLookupType = new Repositories<LookupType>();
            IRepository<TableTab> _iTabletab = new Repositories<TableTab>();
            var lSystemEntities = _iSystem.All();
            var lTableEntities = _iTable.All();
            var lTabletabEntities = _iTabletab.All();
            var lTabsetsEntities = _iTabset.All();
            var lRelationShipEntities = _iRelationShip.All();

            var treeView = DatabaseMap.GetBindTreeControl(lSystemEntities, lTableEntities, lTabletabEntities, lTabsetsEntities, lRelationShipEntities);

            var Items = Enumerable.Repeat(new SelectListItem()
            {
                Value = lSystemEntities.FirstOrDefault().UserName,
                Text = lSystemEntities.FirstOrDefault().UserName,
                Selected = true
            }, count: 1);
            //check renuka
            this.ViewBag.DatabaseList = Items.Concat(_iDatabas.All().CreateSelectList<Databas>("DBName", "DBName", null));
            this.ViewBag.FieldTypeList = _iLookupType.All().Where(x => x.LookupTypeForCode.Trim().ToUpper().Equals("FLDSZ")).CreateSelectList<LookupType>("LookupTypeCode", "LookupTypeValue", default(int?), "SortOrder", System.ComponentModel.ListSortDirection.Ascending);

            this.ViewBag.ExistingTablesList = Common.DefaultDropdownSelectionBlank;
            this.ViewBag.FieldsList = Common.DefaultDropdownSelectionBlank;

            return PartialView("_MapPartial", treeView);
        }

        // Add Workgroup in database
        public IActionResult SetNewWorkgroup(string pWorkGroupName, int pTabsetsId)
        {
            IRepository<TabSet> _iTabset = new Repositories<TabSet>();
            do
            {
                try
                {

                    var oSecureObjectMain = new Smead.Security.SecureObject(passport);

                    // Dim pOutputSettingsEntities = From t In _iSecureObject.All()
                    // Find workgroup from secure object table
                    // Dim pSecureObjectID = pOutputSettingsEntities.Where(Function(x) x.Name.Trim().ToLower().Equals("workgroups")).FirstOrDefault().SecureObjectID
                    _iTabset.BeginTransaction();
                    // _iSecureObject.BeginTransaction()

                    // Duplidate check
                    if (_iTabset.All().Any(x => (x.UserName.Trim().ToLower()) == (pWorkGroupName.Trim().ToLower())) == false)
                    {
                        // Add new workgroup if not already exist
                        var pTabsetEntity = new TabSet();
                        pTabsetEntity.Id = (short)(_iTabset.All().Max(x => x.Id) + 1);
                        pTabsetEntity.UserName = pWorkGroupName.Trim();
                        pTabsetEntity.ViewGroup = 0;
                        pTabsetEntity.StartupTabset = false;
                        pTabsetEntity.TabFontBold = false;
                        _iTabset.Add(pTabsetEntity);
                        pTabsetsId = pTabsetEntity.Id;
                        // Dim pSecureObjectEntity = New SecureObject()
                        // pSecureObjectEntity.BaseID = pSecureObjectID
                        // pSecureObjectEntity.SecureObjectTypeID = pSecureObjectID
                        // pSecureObjectEntity.Name = pWorkGroupName.Trim()
                        // _iSecureObject.Add(pSecureObjectEntity)

                        oSecureObjectMain.Register(pWorkGroupName.Trim(), (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.WorkGroup, (int)Enums.SecureObjects.WorkGroup);
                    }
                    else
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlTheWGName"), pWorkGroupName.Trim());
                        break;
                    }
                    _iTabset.CommitTransaction();
                    // _iSecureObject.CommitTransaction()

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Keys.SaveWorkSuccessMessage();
                }
                catch (Exception)
                {
                    _iTabset.RollBackTransaction();
                    // _iSecureObject.RollBackTransaction()
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                tabsetId = Guid.NewGuid().ToString() + "_Tabsets_" + pTabsetsId.ToString()
            });
        }

        // Edit data load for workgroup in Map
        public IActionResult EditNewWorkgroup(int pTabsetsId)
        {
            IRepository<TabSet> _iTabset = new Repositories<TabSet>();
            if (!(pTabsetsId > 0))
            {
                return Json(new { success = false });
            }

            var pTabsetsEntity = _iTabset.All().Where(x => x.Id == pTabsetsId).FirstOrDefault();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(pTabsetsEntity, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        // Remove Workgroup
        public IActionResult RemoveNewWorkgroup(int pTabsetsId)
        {
            IRepository<TabSet> _iTabset = new Repositories<TabSet>();
            IRepository<TableTab> _iTabletab = new Repositories<TableTab>();
            do
            {
                try
                {
                    // Find secure object type
                    int lSecureObjectId;
                    var oSecureObjectMain = new Smead.Security.SecureObject(passport);

                    string sUserName = "";
                    // Dim pOutputSettingsEntities = From t In _iSecureObject.All()
                    // Dim pSecureObjectID = pOutputSettingsEntities.Where(Function(x) x.Name.Trim().ToLower().Equals("workgroups")).FirstOrDefault().SecureObjectID
                    _iTabset.BeginTransaction();
                    // _iSecureObject.BeginTransaction()
                    // Edit workgroup if already exist
                    if (pTabsetsId > 0)
                    {
                        var pTabletab = _iTabletab.All().Where(x => x.TabSet == pTabsetsId);
                        if (pTabletab.Count() > 0)
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlCantRemoveWG");
                            break;
                        }
                        var pTabsetEntity = _iTabset.All().Where(x => x.Id == pTabsetsId).FirstOrDefault();
                        sUserName = pTabsetEntity.UserName;
                        if (pTabsetEntity is not null)
                        {
                            _iTabset.Delete(pTabsetEntity);

                            lSecureObjectId = oSecureObjectMain.GetSecureObjectID(sUserName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.WorkGroup);
                            if (lSecureObjectId != 0)
                                oSecureObjectMain.UnRegister(lSecureObjectId);
                            // Dim pSecureObjectEntity = pOutputSettingsEntities.Where(Function(x) x.Name.Trim().ToLower().Equals(sUserName.Trim().ToLower())).FirstOrDefault()
                            // If Not pSecureObjectEntity Is Nothing Then
                            // _iSecureObject.Delete(pSecureObjectEntity)
                            // End If
                        }
                    }
                    _iTabset.CommitTransaction();
                    // _iSecureObject.CommitTransaction()

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Keys.DeleteWorkSuccessMessage();
                }
                catch (Exception)
                {
                    _iTabset.RollBackTransaction();
                    // _iSecureObject.RollBackTransaction()
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Create new table
        public IActionResult SetNewTable(int pParentNodeId, string pDatabaseName, string pTableName, string pUserName, string pIdFieldName, int pFieldType, int pFieldSize, int pNodeLevel)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<TabSet> _iTabset = new Repositories<TabSet>();
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            IRepository<TableTab> _iTabletab = new Repositories<TableTab>();
            if (pFieldType == (int)Enums.meFieldTypes.ftText)
            {
                int iMaxFieldSize = DatabaseMap.UserLinkIndexTableIdSize;
                if (pFieldSize < 1 | pFieldSize > iMaxFieldSize)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldSizeValue"), iMaxFieldSize);
                    return Json(new
                    {
                        errortype = Keys.ErrorType,
                        message = Keys.ErrorMessage
                    });
                }
            }

            string sDatabaseName = Keys.DBName;
            Databas pDatabaseEntity = _iDatabas.All().Where(x => x.DBName.Trim().ToLower().Equals(pDatabaseName.Trim().ToLower())).FirstOrDefault();
            Table pParentTableEntity = null;
            bool bSysAdmin;
            var lViewEntities = _iView.All();
            Table pCurrentTableEntity;
            string sViewName = "";
            TabSet pTabSetEntity;
            int iTabSetId = 0;
            string sNewNodeId = 0.ToString();
            int iTableTabId = 0;
            int iRelId = 0;
            int iTableId = 0;
            int viewIdTemp = 0;
            int lSecureObjectId;
            var oSecureObjectMain = new Smead.Security.SecureObject(passport);

            var sADOConn = DataServices.DBOpen(pDatabaseEntity);
            if (sADOConn is null)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlTheServerL1"), pDatabaseEntity.DBServer, pDatabaseEntity.DBDatabase, pDatabaseEntity.DBName);
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
            else
            {
                List<SchemaColumns> oSchemaColumns;
                oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pTableName, sADOConn, pIdFieldName);
                if (oSchemaColumns.Count > 0)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlInternalNameExists");
                    return Json(new
                    {
                        errortype = Keys.ErrorType,
                        message = Keys.ErrorMessage
                    });
                }

                bSysAdmin = DataServices.IsSysAdmin("", null, null, pDatabases: pDatabaseEntity);

                if (bSysAdmin)
                {
                    try
                    {
                        if (pNodeLevel == (int)Enums.eNodeLevel.ndTabSets)
                        {
                            pTabSetEntity = _iTabset.All().Where(x => x.Id == pParentNodeId).FirstOrDefault();
                            if (pTabSetEntity is not null)
                            {
                                iTabSetId = pTabSetEntity.Id;
                            }
                        }
                        else if (pNodeLevel == (int)Enums.eNodeLevel.ndTableTabRel)
                        {
                            pParentTableEntity = _iTable.All().Where(x => x.TableId == pParentNodeId).FirstOrDefault();
                        }

                        string sParentTableName = "";
                        bool SaveData = false;

                        _iTable.BeginTransaction();
                        _iView.BeginTransaction();
                        _iRelationShip.BeginTransaction();
                        _iTabletab.BeginTransaction();

                        if (pDatabaseEntity is not null)
                        {
                            sDatabaseName = pDatabaseEntity.DBName;
                        }

                        if (pParentTableEntity is not null)
                        {
                            sParentTableName = pParentTableEntity.TableName;
                        }

                        // Enums.meFieldTypes.ftCounter
                        // Create table in apropriate database
                        string pOutPutStr = DatabaseMap.CreateNewTables(pTableName, pIdFieldName, (Enums.meFieldTypes)pFieldType, pFieldSize, pDatabaseEntity, pParentTableEntity, (Repositories<Databas>)_iDatabas);
                        if (string.IsNullOrEmpty(pOutPutStr))
                        {
                            // Set entry of this record in database "Tables" table
                            if (DatabaseMap.SetTablesEntity(pTableName, pUserName, pIdFieldName, sDatabaseName, (Enums.meFieldTypes)pFieldType, _iTable, ref iTableId))
                            {
                                // Recently created table entity record
                                pCurrentTableEntity = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableName.Trim().ToLower())).FirstOrDefault();
                                // View name 
                                if (lViewEntities.Any(x => (x.ViewName.Trim().ToLower()) == (pUserName.Trim().ToLower())) == false)
                                {
                                    sViewName = "All " + Strings.Trim(pUserName); // changed by Nikunj for FUS-5812
                                }
                                else
                                {
                                    sViewName = "All " + Strings.Trim(pUserName) + " " + (lViewEntities.Where(x => x.ViewName.Trim().ToLower().Contains(pUserName.Trim().ToLower())).Count() + 1);
                                } // changed by Nikunj for FUS-5812
                                  // Insert into views table
                                int iViewId = 0;
                                if (DatabaseMap.SetViewsEntity(pTableName, sViewName, _iView, ref iViewId))
                                {
                                    viewIdTemp = iViewId;
                                    // Insert into view columns 
                                    if (DatabaseMap.SetViewColumnEntity(iViewId, pTableName, pIdFieldName, pFieldType, _iViewColumn, pParentTableEntity))
                                    {
                                        // If (iTabSetId <> 0) Then
                                        if (!string.IsNullOrEmpty(Strings.Trim(sParentTableName)))
                                        {
                                            // Insert record in relationship
                                            if (DatabaseMap.SetRelationshipsEntity(pTableName, pParentTableEntity, _iRelationShip, ref iRelId))
                                            {
                                                SaveData = true;
                                            }
                                        }
                                        else if (iTabSetId != 0)
                                        {
                                            // Insert record in TabSet
                                            if (DatabaseMap.SetTabSetEntity(iTabSetId, iViewId, pTableName, _iTabletab, ref iTableTabId))
                                            {
                                                SaveData = true;
                                            }
                                        }
                                        // End If
                                    }

                                }

                            }
                        }
                        else if (!ReferenceEquals(pOutPutStr, "False"))
                            throw new Exception(pOutPutStr);

                        if (SaveData)
                        {
                            if (iRelId != 0)
                            {
                                sNewNodeId = iTableId.ToString(); // Guid.NewGuid.ToString() + "_RelShips_" + iTableId.ToString()
                            }
                            else if (iTableTabId != 0)
                            {
                                sNewNodeId = Guid.NewGuid().ToString() + "_Tabletabs_" + iTableId.ToString();
                            }

                            lSecureObjectId = oSecureObjectMain.Register(pTableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (int)Enums.SecureObjects.Table);
                            oSecureObjectMain.Register(sViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.View, lSecureObjectId);

                            // Added by Ganesh - To fix Bug #806 - 12/01/2016
                            var reqAndTransferPermissions = _iSecureObjectPermission.All().Where(x => x.SecureObjectID == lSecureObjectId & (x.PermissionID == (int)Enums.PassportPermissions.Request | x.PermissionID == (int)Enums.PassportPermissions.RequestOnBehalf | x.PermissionID == (int)Enums.PassportPermissions.RequestHigh | x.PermissionID == (int)Enums.PassportPermissions.Transfer));
                            _iSecureObjectPermission.BeginTransaction();
                            _iSecureObjectPermission.DeleteRange(reqAndTransferPermissions);
                            _iSecureObjectPermission.CommitTransaction();
                            // END of fix Bug #806

                            _iTable.CommitTransaction();
                            _iView.CommitTransaction();
                            _iRelationShip.CommitTransaction();
                            _iTabletab.CommitTransaction();
                        }
                        else
                        {
                            _iTable.RollBackTransaction();
                            _iView.RollBackTransaction();
                            _iRelationShip.RollBackTransaction();
                            _iTabletab.RollBackTransaction();
                            DatabaseMap.DropTable(pTableName, pDatabaseEntity);

                            Keys.ErrorType = "e";
                            Keys.ErrorMessage = Keys.ErrorMessageJS();
                        }

                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgMapTableCreateSuccess");
                    }
                    catch (Exception ex)
                    {

                        _iTable.RollBackTransaction();
                        _iView.RollBackTransaction();
                        _iRelationShip.RollBackTransaction();
                        _iTabletab.RollBackTransaction();
                        DatabaseMap.DropTable(pTableName, pDatabaseEntity);

                        Keys.ErrorType = "e";
                        Keys.ErrorMessage = ex.Message;
                        if (ex.Message.ToLower().Contains("incorrect syntax near the keyword"))
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgMapTableCreateInValid"), pTableName);
                    }
                }
                else
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlNotSufficiantPermission"), pDatabaseName);
                }
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                nodeId = sNewNodeId,
                viewIdTemp
            });
        }

        // Add Workgroup in database
        public IActionResult RenameTreeNode(string pPrevNodeName, string pNewNodeName, int pId, string pRenameOperation)
        {
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            IRepository<TabSet> _iTabset = new Repositories<TabSet>();
            IRepository<Table> _iTable = new Repositories<Table>();
            do
            {
                try
                {
                    var oSecureObjectMain = new Smead.Security.SecureObject(passport);

                    bool exitTry = false;
                    bool exitTry1 = false;
                    bool exitTry2 = false;
                    bool exitTry3 = false;
                    switch (pRenameOperation.Trim().ToUpper())
                    {
                        case "A":
                            {
                                var lSystemEntities = _iSystem.All();
                                if (lSystemEntities.Any(x => (x.UserName.Trim().ToLower()) == (pNewNodeName.Trim().ToLower()) && x.Id != pId) == false)
                                {
                                    _iSystem.BeginTransaction();
                                    // Dim pSystemEntity = lSystemEntities.Where(Function(x) x.Id = pId AndAlso x.UserName.Trim().ToLower() = pPrevNodeName.Trim().ToLower()).FirstOrDefault()
                                    var pSystemEntity = lSystemEntities.Where(x => x.Id == pId).FirstOrDefault();
                                    if (pSystemEntity is not null)
                                    {
                                        pSystemEntity.UserName = pNewNodeName.Trim();
                                        _iSystem.Update(pSystemEntity);
                                    }
                                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRenameAppNameSuccessfully");
                                    _iSystem.CommitTransaction();
                                }
                                else
                                {
                                    Keys.ErrorType = "w";
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlApplicationName"), pNewNodeName.Trim());
                                    exitTry = true;
                                    break;
                                }

                                break;
                            }

                        case "W":
                            {
                                var pOutputSettingsEntities = _iSecureObject.All();

                                // Duplidate check
                                if (_iTabset.All().Any(x => (x.UserName.Trim().ToLower()) == (pNewNodeName.Trim().ToLower()) && x.Id != pId) == false)
                                {
                                    _iTabset.BeginTransaction();
                                    // _iSecureObject.BeginTransaction()
                                    var pTabsetEntity = _iTabset.All().Where(x => x.Id == pId).FirstOrDefault();
                                    string pOldWGName = "";
                                    if (pTabsetEntity is not null)
                                    {
                                        pOldWGName = pTabsetEntity.UserName;
                                        pTabsetEntity.UserName = pNewNodeName.Trim();
                                        _iTabset.Update(pTabsetEntity);

                                        oSecureObjectMain.Rename(pOldWGName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.WorkGroup, pNewNodeName.Trim());

                                        // Dim pSecureObjectEntity = pOutputSettingsEntities.Where(Function(x) x.Name.Trim().ToLower().Equals(pPrevNodeName.Trim().ToLower())).FirstOrDefault()
                                        // If Not pSecureObjectEntity Is Nothing Then
                                        // pSecureObjectEntity.Name = pNewNodeName.Trim()
                                        // _iSecureObject.Update(pSecureObjectEntity)
                                        // End If
                                    }
                                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRenameWGSuccessfully");
                                    _iTabset.CommitTransaction();
                                }
                                // _iSecureObject.CommitTransaction()
                                else
                                {
                                    Keys.ErrorType = "w";
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlTheWGName"), pNewNodeName.Trim());
                                    exitTry1 = true;
                                    break;
                                }

                                break;
                            }

                        case "T":
                            {

                                var lTableEntities = _iTable.All();
                                if (lTableEntities.Any(x => (x.UserName.Trim().ToLower()) == (pNewNodeName.Trim().ToLower()) && x.TableId != pId) == false)
                                {
                                    _iTable.BeginTransaction();
                                    var pTableEntity = lTableEntities.Where(x => x.TableId == pId && (x.UserName.Trim().ToLower()) == (pPrevNodeName.Trim().ToLower())).FirstOrDefault();
                                    if (pTableEntity is not null)
                                    {
                                        pTableEntity.UserName = pNewNodeName.Trim();
                                        _iTable.Update(pTableEntity);
                                    }
                                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRenameTblSuccessfully");
                                    _iTable.CommitTransaction();
                                }
                                else
                                {
                                    Keys.ErrorType = "w";
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlTheTblName"), pNewNodeName.Trim());
                                    exitTry2 = true;
                                    break;
                                }

                                break;
                            }

                        default:
                            {
                                Keys.ErrorType = "e";
                                Keys.ErrorMessage = Keys.ErrorMessageJS();
                                exitTry3 = true;
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

                    if (exitTry3)
                    {
                        break;
                    }
                    Keys.ErrorType = "s";
                }
                catch (Exception)
                {
                    switch (pRenameOperation.Trim().ToUpper())
                    {
                        case "A":
                            {
                                _iSystem.RollBackTransaction();
                                break;
                            }
                        case "W":
                            {
                                _iTabset.RollBackTransaction();
                                break;
                            }
                        // _iSecureObject.RollBackTransaction()
                        case "T":
                            {
                                _iTable.RollBackTransaction();
                                break;
                            }

                        default:
                            {
                                _iSystem.RollBackTransaction();
                                _iTable.RollBackTransaction();
                                _iTabset.RollBackTransaction();
                                _iSecureObject.RollBackTransaction();
                                break;
                            }
                    }

                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Load Dropdown Data into Attach table
        public IActionResult GetAttachTableList(int iParentTableId, int iTabSetId)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<TableTab> _iTabletab = new Repositories<TableTab>();
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();

            var lTablesEntities = _iTable.All();

            lTablesEntities = DatabaseMap.GetAttachExistingTableList(lTablesEntities, iParentTableId, iTabSetId, (Repositories<TableTab>)_iTabletab,
                (Repositories<RelationShip>)_iRelationShip, _IDBManager, httpContext);


            var Result = from p in lTablesEntities.OrderBy(x => x.TableName)
                         select new { p.TableName, p.TableId, p.UserName };

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(Result, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        // Attach time Exiting fields confirmation
        public IActionResult ConfirmationForAlreadyExistColumn(int iParentTableId, int iTableId, bool ConfAns)
        {
            var pParentTableEntity = new Table();
            IRepository<Table> _iTable = new Repositories<Table>();
            var lTableEntities = _iTable.All();
            var bCreateNew = default(bool);
            var csADOConn = new ADODB.Connection();
            var psADOConn = new ADODB.Connection();
            List<SchemaColumns> oSchemaColumns;
            var pTableEntity = new Table();
            string sExtraStr;
            int iExtraValue;
            bool SaveData = false;
            int iRelId = 0;
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            try
            {
                sExtraStr = "";
                iExtraValue = 1;
                pTableEntity = lTableEntities.Where(x => x.TableId == iTableId).FirstOrDefault();
                pParentTableEntity = lTableEntities.Where(x => x.TableId == iParentTableId).FirstOrDefault();
                csADOConn = DataServices.DBOpen(_iDatabas.All().Where(x => x.DBName.Trim().ToLower().Equals(pTableEntity.DBName.Trim().ToLower())).FirstOrDefault());
                oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pTableEntity.TableName, csADOConn, pParentTableEntity.TableName + DatabaseMap.RemoveTableNameFromField(pParentTableEntity.IdFieldName));

                bCreateNew = true;
                if (ConfAns)
                {
                    bCreateNew = false;
                }
                else
                {
                    do
                    {
                        sExtraStr = Strings.Format(iExtraValue);
                        iExtraValue = iExtraValue + 1;
                        oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pTableEntity.TableName, csADOConn, pParentTableEntity.TableName + DatabaseMap.RemoveTableNameFromField(pParentTableEntity.IdFieldName) + sExtraStr);
                    }
                    while (oSchemaColumns.Count != 0);
                }
                oSchemaColumns = null;
                psADOConn = DataServices.DBOpen(_iDatabas.All().Where(x => x.DBName.Trim().ToLower().Equals(pParentTableEntity.DBName.Trim().ToLower())).FirstOrDefault());
                oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pParentTableEntity.TableName, psADOConn, DatabaseMap.RemoveTableNameFromField(pParentTableEntity.IdFieldName));
                _iRelationShip.BeginTransaction();
                if (bCreateNew)
                {
                    csADOConn.BeginTrans();
                    if (DatabaseMap.CreateNewField(pParentTableEntity.TableName + DatabaseMap.RemoveTableNameFromField(pParentTableEntity.IdFieldName) + sExtraStr, (Enums.DataTypeEnum)Convert.ToInt32(Interaction.IIf(oSchemaColumns.Count > 0, oSchemaColumns[0].DataType, Enums.DataTypeEnum.rmInteger)), Convert.ToInt64("0" + Strings.Trim(oSchemaColumns[0].CharacterMaxLength.ToString())), pTableEntity.TableName, csADOConn) == true)
                    {
                        if (DatabaseMap.SetRelationshipsEntity(pTableEntity.TableName, pParentTableEntity, _iRelationShip, ref iRelId, sExtraStr) == true)
                        {
                            SaveData = true;
                        }
                    }
                }
                else if (DatabaseMap.SetRelationshipsEntity(pTableEntity.TableName, pParentTableEntity, _iRelationShip, ref iRelId, sExtraStr) == true)
                {
                    SaveData = true;
                }

                if (bCreateNew)
                {
                    _iRelationShip.CommitTransaction();
                    csADOConn.CommitTrans();
                }
                else
                {
                    _iRelationShip.CommitTransaction();
                }
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Keys.SaveSuccessMessage();
            }
            catch (Exception)
            {
                if (bCreateNew)
                {
                    _iRelationShip.RollBackTransaction();
                    csADOConn.RollbackTrans();
                }
                else
                {
                    _iRelationShip.RollBackTransaction();
                }
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            oSchemaColumns = null;
            pParentTableEntity = null;
            pTableEntity = null;
            psADOConn.Close();
            csADOConn.Close();

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Attach Table in Map Node
        public IActionResult SetAttachTableDetails(int iParentTableId, int iTableId, int iTabSetId)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            var lTableEntities = _iTable.All();
            var lViewEntities = _iView.All();
            var pTableEntity = new Table();
            var pParentTableEntity = new Table();
            var pViewEntity = new View();
            bool SaveData = false;
            List<SchemaColumns> oSchemaColumns;
            var csADOConn = new ADODB.Connection();
            var psADOConn = new ADODB.Connection();
            var bCreateNew = default(bool);
            // Dim sExtraStr As String
            // Dim iExtraValue As Integer
            int iTableTabId = 0;
            int iRelId = 0;
            IRepository<TableTab> _iTabletab = new Repositories<TableTab>();
            do
            {
                try
                {
                    pTableEntity = lTableEntities.Where(x => x.TableId == iTableId).FirstOrDefault();
                    if (pTableEntity is not null)
                    {
                        pViewEntity = lViewEntities.Where(x => x.TableName.Trim().ToLower().Equals(pTableEntity.TableName.Trim().ToLower())).FirstOrDefault();
                    }

                    if (pViewEntity is not null)
                    {
                        if (pViewEntity.Id != 0)
                        {
                            if (iParentTableId != 0)
                            {
                                // sExtraStr = ""
                                // iExtraValue = 1
                                pParentTableEntity = lTableEntities.Where(x => x.TableId == iParentTableId).FirstOrDefault();
                                csADOConn = DataServices.DBOpen(_iDatabas.All().Where(x => x.DBName.Trim().ToLower().Equals(pTableEntity.DBName.Trim().ToLower())).FirstOrDefault());
                                oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pTableEntity.TableName, csADOConn, pParentTableEntity.TableName + DatabaseMap.RemoveTableNameFromField(pParentTableEntity.IdFieldName));
                                bCreateNew = true;
                                if (oSchemaColumns.Count > 0)
                                {
                                    Keys.ErrorType = "c";
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlReuseThisColumn"), pParentTableEntity.TableName + DatabaseMap.RemoveTableNameFromField(pParentTableEntity.IdFieldName));
                                    break;
                                }
                                else
                                {
                                    oSchemaColumns = null;
                                    psADOConn = DataServices.DBOpen(_iDatabas.All().Where(x => x.DBName.Trim().ToLower().Equals(pParentTableEntity.DBName.Trim().ToLower())).FirstOrDefault());
                                    oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pParentTableEntity.TableName, psADOConn, DatabaseMap.RemoveTableNameFromField(pParentTableEntity.IdFieldName));
                                    _iRelationShip.BeginTransaction();
                                    csADOConn.BeginTrans();
                                    if (DatabaseMap.CreateNewField(pParentTableEntity.TableName + DatabaseMap.RemoveTableNameFromField(pParentTableEntity.IdFieldName), (Enums.DataTypeEnum)Convert.ToInt32(Interaction.IIf(oSchemaColumns.Count > 0, oSchemaColumns[0].DataType, Enums.DataTypeEnum.rmInteger)), Convert.ToInt64("0" + Strings.Trim(oSchemaColumns[0].CharacterMaxLength.ToString())), pTableEntity.TableName, csADOConn) == true)
                                    {
                                        if (DatabaseMap.SetRelationshipsEntity(pTableEntity.TableName, pParentTableEntity, _iRelationShip, ref iRelId) == true)
                                        {
                                            SaveData = true;
                                        }
                                    }
                                }
                            }

                            else
                            {
                                // smsg += " => Tabletab value save"
                                _iTabletab.BeginTransaction();
                                if (DatabaseMap.SetTabSetEntity(iTabSetId, pViewEntity.Id, pTableEntity.TableName, _iTabletab, ref iTableTabId) == true)
                                {
                                    SaveData = true;
                                }
                            }
                        }
                    }

                    if (SaveData)
                    {
                        if (iParentTableId != 0)
                        {
                            if (bCreateNew)
                            {
                                _iRelationShip.CommitTransaction();
                                csADOConn.CommitTrans();
                            }
                            else
                            {
                                _iRelationShip.CommitTransaction();
                                // csADOConn.CommitTrans()
                            }
                        }
                        else
                        {
                            _iTabletab.CommitTransaction();
                        }
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgMapTableAttachedSuccess");
                    }
                }
                catch (Exception)
                {
                    if (iParentTableId != 0)
                    {
                        if (bCreateNew)
                        {
                            _iRelationShip.RollBackTransaction();
                            csADOConn.RollbackTrans();
                        }
                        else
                        {
                            _iRelationShip.RollBackTransaction();
                        }
                    }
                    else
                    {
                        _iTabletab.RollBackTransaction();
                    }
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);

            oSchemaColumns = null;
            pParentTableEntity = null;
            pTableEntity = null;
            pViewEntity = null;

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public IActionResult DeleteTableFromTableTab(int iTabSetId, int iNewTableSetId)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<TableTab> _iTabletab = new Repositories<TableTab>();
            var lTableEntities = _iTable.All();
            bool IsUnattached = true;
            try
            {
                var pTableTabTableEntity = lTableEntities.Where(x => x.TableId == iTabSetId).FirstOrDefault();
                var pTableTabsEntity = _iTabletab.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableTabTableEntity.TableName.Trim().ToLower()) && x.TabSet == iNewTableSetId).FirstOrDefault();
                if (pTableTabsEntity.Id != 0)
                {
                    _iTabletab.Delete(pTableTabsEntity);
                }
                pTableTabsEntity = null;

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlUnAttachTblSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                isunattached = IsUnattached
            });
        }

        public IActionResult DeleteTableFromRelationship(int iParentTableId, int iTableId)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            var lTableEntities = _iTable.All();
            var pUpperTableEntity = new Table();
            var pLowerTableEntity = new Table();
            var lRelationshipsEntites = _iRelationShip.All();

            bool IsUnattached = true;
            try
            {
                pUpperTableEntity = lTableEntities.Where(x => x.TableId == iParentTableId).FirstOrDefault();
                pLowerTableEntity = lTableEntities.Where(x => x.TableId == iTableId).FirstOrDefault();

                var oRelationships = lRelationshipsEntites.Where(x => x.UpperTableName.Trim().ToLower().Equals(pUpperTableEntity.TableName.Trim().ToLower()) && x.LowerTableName.Trim().ToLower().Equals(pLowerTableEntity.TableName.Trim().ToLower())).FirstOrDefault();
                if (oRelationships.Id != 0)
                {
                    _iRelationShip.Delete(oRelationships);
                }

                oRelationships = null;
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlUnAttachTblSuccessfully");
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            lTableEntities = null;
            pUpperTableEntity = null;
            pLowerTableEntity = null;
            lRelationshipsEntites = null;

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                isunattached = IsUnattached,
                iTableId
            });
        }

        public IActionResult GetDeleteTableNames(int iParentTableId, int iTableId)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<TabSet> _iTabset = new Repositories<TabSet>();
            var lTableEntities = _iTable.All();
            var lTabsetEntities = _iTabset.All();
            var pUpperTableEntity = new Table();
            var pLowerTableEntity = new TabSet();
            string sUpperTableName = "";
            string sLowerTableName = "";
            try
            {
                if (iParentTableId != 0 && iTableId != 0)
                {
                    pUpperTableEntity = lTableEntities.Where(x => x.TableId == iTableId).FirstOrDefault();
                    pLowerTableEntity = lTabsetEntities.Where(x => x.Id == iParentTableId).FirstOrDefault();
                    sUpperTableName = Strings.Trim(pUpperTableEntity.UserName);
                    sLowerTableName = Strings.Trim(pLowerTableEntity.UserName);
                    Keys.ErrorType = "s";
                }
                else
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            } // ex.Message()

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                parentTable = Strings.Trim(pUpperTableEntity.UserName),
                childTable = Strings.Trim(pLowerTableEntity.UserName),
                childTableId = Strings.Trim(pLowerTableEntity.Id.ToString())
            });
        }

        public IActionResult DeleteTable(int iParentTableId, int iTableId)
        {
            string sViewMessage = "";
            bool IsUnattached = true;
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            do
            {
                try
                {
                    var lTableEntities = _iTable.All();

                    var pUpperTableEntity = lTableEntities.Where(x => x.TableId == iParentTableId).FirstOrDefault();
                    var pLowerTableEntity = lTableEntities.Where(x => x.TableId == iTableId).FirstOrDefault();
                    IRepository<View> _iView = new Repositories<View>();
                    DatabaseMap.GetTableDependency(lTableEntities, (Repositories<View>)_iView, (Repositories<ViewColumn>)_iViewColumn, pUpperTableEntity, pLowerTableEntity, ref sViewMessage);
                    if (!string.IsNullOrEmpty(sViewMessage))
                    {
                        sViewMessage = string.Format(Languages.get_Translation("msgAdminCtrlTblExistsOnSomePlacesL1"), pLowerTableEntity.UserName, Strings.Trim(pUpperTableEntity.UserName), sViewMessage);
                        // MsgBox(sViewMessage, MsgBoxStyle.OkOnly, "Worning Message")
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = "<html>" + sViewMessage + "</html>";
                        pUpperTableEntity = null;
                        pLowerTableEntity = null;
                        IsUnattached = false;
                        break;
                    }
                    else
                    {
                        Keys.ErrorType = "c";
                        Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlRuSure2RemoveAttachment"), Strings.Trim(pLowerTableEntity.UserName), Strings.Trim(pUpperTableEntity.UserName));
                    }
                }
                // End If
                catch (Exception)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                isunattached = IsUnattached
            });
        }

        // Attach Existing Table in Map Node
        public IActionResult SetAttachExistingTableDetails(int iParentTableId, int iTableId, string sIdFieldName)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<View> _iView = new Repositories<View>();
            var lTableEntities = _iTable.All();
            var lViewEntities = _iView.All();
            var pTableEntity = new Table();
            var pParentTableEntity = new Table();
            var pViewEntity = new View();
            bool SaveData = true;
            int iRelId = 0;
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            do
            {
                try
                {
                    pTableEntity = lTableEntities.Where(x => x.TableId == iTableId).FirstOrDefault();
                    if (pTableEntity is not null)
                    {
                        pViewEntity = lViewEntities.Where(x => x.TableName.Trim().ToLower().Equals(pTableEntity.TableName.Trim().ToLower())).FirstOrDefault();
                    }

                    if (pViewEntity is not null)
                    {
                        if (pViewEntity.Id != 0)
                        {
                            if (iParentTableId != 0)
                            {
                                pParentTableEntity = lTableEntities.Where(x => x.TableId == iParentTableId).FirstOrDefault();
                                _iRelationShip.BeginTransaction();
                                if (DatabaseMap.SetRelationshipsEntity(pTableEntity.TableName, pParentTableEntity, _iRelationShip, ref iRelId, sIdFieldName: sIdFieldName) == true)
                                {
                                    SaveData = true;
                                }
                                pParentTableEntity = null;
                            }
                        }
                    }
                    pTableEntity = null;
                    pViewEntity = null;
                    if (iParentTableId != 0)
                    {
                        _iRelationShip.CommitTransaction();
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Keys.SaveSuccessMessage();
                        break;
                    }
                }
                catch (Exception)
                {
                    if (iParentTableId != 0)
                    {
                        _iRelationShip.RollBackTransaction();
                    }
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Get tables Id fields list
        public IActionResult GetAttachTableFieldsList(int iParentTableId, int iTableId, string sCurrIdFieldName)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            var lTablesEntities = _iTable.All();
            var lTables = new List<string>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            do
            {
                try
                {
                    var pTableEntity = lTablesEntities.Where(x => x.TableId == iTableId).FirstOrDefault();
                    var pParentTableEntity = lTablesEntities.Where(x => x.TableId == iParentTableId).FirstOrDefault();
                    lTables = DatabaseMap.GetAttachTableFieldsList(pTableEntity, pParentTableEntity, (Repositories<Databas>)_iDatabas);
                    if (lTables.Count == 0)
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlTblDoesNotContainAnyMatchingFields"), pTableEntity.UserName, sCurrIdFieldName);
                        break;
                    }
                    Keys.ErrorType = "s";
                }
                catch (Exception)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(lTables, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                jsonObject
            });

            // Return Json(jsonObject)
        }

        // Get tables Id fields list
        public IActionResult LoadAttachExistingTableScreen(int iParentTableId, int iTabSetId)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            IRepository<TableTab> _iTabletab = new Repositories<TableTab>();
            var lTablesEntities = _iTable.All();
            var pTableNameEntity = lTablesEntities.Where(x => x.TableId == iParentTableId).FirstOrDefault();
            lTablesEntities = DatabaseMap.GetAttachExistingTableList(lTablesEntities, iParentTableId, iTabSetId, (Repositories<TableTab>)_iTabletab, 
                (Repositories<RelationShip>)_iRelationShip, _IDBManager, httpContext);
            var ResultTables = from p in lTablesEntities.OrderBy(x => x.TableName)
                               select new { p.TableName, p.TableId, p.UserName };

            // Dim pTableEntity = lTablesEntities.Where(Function(x) x.TableId = ResultTables.FirstOrDefault().TableId).FirstOrDefault()
            // Dim lTableColumns = DatabaseMap.GetAttachTableFieldsList(pTableEntity, pTableNameEntity, _iDatabas)

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObjectTableColumns = ""; // JsonConvert.SerializeObject(lTableColumns, Newtonsoft.Json.Formatting.Indented, Setting)
            string jsonObjectTables = JsonConvert.SerializeObject(ResultTables, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(new
            {
                tableName = pTableNameEntity.TableName,
                tableIdColumn = DatabaseMap.RemoveTableNameFromField(pTableNameEntity.IdFieldName),
                tableIdColumnList = jsonObjectTableColumns,
                tablesList = jsonObjectTables
            });

            // Return Json(jsonObject)
        }

        // Button Up & Down in Tree
        public IActionResult ChangeNodeOrder(int pUpperTableId, string pTableName, int pTableId, char pAction)
        {
            try
            {
                int vOldOrder;
                int vNewOrder;
                Table oTable;
                Table oUpperTable;
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<TabSet> _iTabset = new Repositories<TabSet>();
                IRepository<TableTab> _iTabletab = new Repositories<TableTab>();
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                oTable = _iTable.All().Where(x => x.TableId == pTableId).FirstOrDefault();
                if (oTable is null)
                {
                    return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlObjectNotFound") });
                }
                oUpperTable = _iTable.All().Where(x => x.TableId == pUpperTableId).FirstOrDefault();

                switch (pTableName.ToLower())
                {
                    case "tabsets":
                        {
                            var oOldTabset = _iTabset.All().Where(x => x.Id == pTableId).FirstOrDefault();
                            if (oOldTabset is null)
                            {
                                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlObjectNotFound") });
                            }
                            vOldOrder = oOldTabset.Id;

                            var oNewTabset = new TabSet();
                            if (Convert.ToString(pAction) == "U")
                            {
                                oNewTabset = _iTabset.All().Where(x => x.Id < pTableId).OrderByDescending(x => x.Id).FirstOrDefault();
                            }
                            else if (Convert.ToString(pAction) == "D")
                            {
                                oNewTabset = _iTabset.All().Where(x => x.Id > pTableId).OrderBy(x => x.Id).FirstOrDefault();
                            }
                            if (oNewTabset is null)
                            {
                                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlObjectNotFound") });
                            }
                            vNewOrder = oNewTabset.Id;

                            _IDBManager.ConnectionString = Keys.get_GetDBConnectionString(null);

                            string sSql = "UPDATE Tabsets SET Id = 9999 WHERE UserName = '" + oOldTabset.UserName + "'";
                            int iOutPut = _IDBManager.ExecuteNonQuery(CommandType.Text, sSql);
                            if (iOutPut > 0)
                            {
                                sSql = "UPDATE Tabsets SET Id = " + vOldOrder.ToString() + " WHERE UserName = '" + oNewTabset.UserName + "'";
                                iOutPut = _IDBManager.ExecuteNonQuery(CommandType.Text, sSql);
                                if (iOutPut > 0)
                                {
                                    sSql = "UPDATE Tabsets SET Id = " + vNewOrder.ToString() + " WHERE UserName = '" + oOldTabset.UserName + "'";
                                    iOutPut = _IDBManager.ExecuteNonQuery(CommandType.Text, sSql);
                                }
                            }


                            var lNewTabletab = _iTabletab.All().Where(x => x.TabSet == vNewOrder).ToList();
                            foreach (TableTab oTabletab in lNewTabletab)
                            {
                                oTabletab.TabSet = (short?)9999;
                                _iTabletab.Update(oTabletab);
                            }

                            var lOldTableTabIds = new List<int>();
                            var lOldTabletab = _iTabletab.All().Where(x => x.TabSet == vOldOrder);
                            foreach (TableTab oTabletab in lOldTabletab.ToList())
                            {
                                lOldTableTabIds.Add(oTabletab.Id);
                                oTabletab.TabSet = (short?)vNewOrder;
                                _iTabletab.Update(oTabletab);
                            }

                            lNewTabletab = _iTabletab.All().Where(x => x.TabSet == vNewOrder || x.TabSet == 9999).ToList();
                            foreach (TableTab oTabletab in lNewTabletab)
                            {
                                if (!lOldTableTabIds.Any(x => x == oTabletab.Id))
                                {
                                    oTabletab.TabSet = (short?)vOldOrder;
                                    _iTabletab.Update(oTabletab);
                                }
                            }

                            _IDBManager.Dispose();

                            if (iOutPut <= 0)
                            {
                                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlErrorWhileChangeOrder") });
                            }

                            break;
                        }

                    // oOldTabset.Id = 9999
                    // _iTabset.Update(oOldTabset)

                    // oNewTabset.Id = vOldOrder
                    // _iTabset.Update(oNewTabset)

                    // oOldTabset.Id = vNewOrder
                    // _iTabset.Update(oOldTabset)

                    case "tabletabs":
                        {
                            // Dim oOldTabletab = _iTabletab.All().Where(Function(x) x.TableName.ToLower().Equals(oTable.TableName.ToLower())).FirstOrDefault()
                            var oOldTabletab = _iTabletab.All().Where(x => x.TableName.ToLower().Equals(oTable.TableName.ToLower()) && x.TabSet == pUpperTableId).FirstOrDefault();
                            if (oOldTabletab is null)
                            {
                                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlObjectNotFound") });
                            }
                            vOldOrder = (int)oOldTabletab.TabOrder;

                            var oNewTabletab = new TableTab();
                            if (Convert.ToString(pAction) == "U")
                            {
                                oNewTabletab = _iTabletab.All().Where(x => x.TabSet == pUpperTableId && x.TabOrder < oOldTabletab.TabOrder).OrderByDescending(x => x.TabOrder).FirstOrDefault();
                            }
                            else if (Convert.ToString(pAction) == "D")
                            {
                                oNewTabletab = _iTabletab.All().Where(x => x.TabSet == pUpperTableId && x.TabOrder > oOldTabletab.TabOrder).OrderBy(x => x.TabOrder).FirstOrDefault();
                            }
                            if (oNewTabletab is null)
                            {
                                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlObjectNotFound") });
                            }
                            vNewOrder = (int)oNewTabletab.TabOrder;

                            oOldTabletab.TabOrder = (short?)vNewOrder;
                            _iTabletab.Update(oOldTabletab);

                            oNewTabletab.TabOrder = (short?)vOldOrder;
                            _iTabletab.Update(oNewTabletab);
                            break;
                        }

                    case "relships":
                        {

                            var oOldRelationShips = _iRelationShip.All().Where(x => x.LowerTableName.ToLower().Equals(oTable.TableName.ToLower()) && x.UpperTableName.ToLower().Equals(oUpperTable.TableName.ToLower())).FirstOrDefault();
                            if (oUpperTable is null | oOldRelationShips is null)
                            {
                                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlObjectNotFound") });
                            }
                            vOldOrder = (int)oOldRelationShips.TabOrder;

                            var oNewRelationShips = new RelationShip();
                            if (Convert.ToString(pAction) == "U")
                            {
                                oNewRelationShips = _iRelationShip.All().Where(x => x.UpperTableName.ToLower().Equals(oUpperTable.TableName.ToLower()) && x.TabOrder < oOldRelationShips.TabOrder).OrderByDescending(x => x.TabOrder).FirstOrDefault();
                            }
                            else if (Convert.ToString(pAction) == "D")
                            {
                                oNewRelationShips = _iRelationShip.All().Where(x => x.UpperTableName.ToLower().Equals(oUpperTable.TableName.ToLower()) && x.TabOrder > oOldRelationShips.TabOrder).OrderBy(x => x.TabOrder).FirstOrDefault();
                            }
                            if (oNewRelationShips is null)
                            {
                                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlObjectNotFound") });
                            }
                            vNewOrder = (int)oNewRelationShips.TabOrder;

                            oOldRelationShips.TabOrder = (short?)vNewOrder;
                            _iRelationShip.Update(oOldRelationShips);

                            oNewRelationShips.TabOrder = (short?)vOldOrder;
                            _iRelationShip.Update(oNewRelationShips);
                            break;
                        }

                    default:
                        {
                            break;
                        }
                        // Keys.ErrorType = "w"
                        // Keys.ErrorMessage = "Not able to reorder selected table."
                }

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Keys.ChangeOrderSuccessMessage();
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }


            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        #endregion

        #region Table Registration
        // Load TableRegisterartial partial view
        public PartialViewResult LoadTableRegisterView()
        {
            return PartialView("_TableRegisterPartial");
        }

        // Get data from 'Table' table and load Unregister table list
        public IActionResult LoadRegisterList()
        {
            IRepository<TableTab> _iTabletab = new Repositories<TableTab>();
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                var registerList = new List<KeyValuePair<string, string>>();
                bool bDoNotAdd;
                var pTableEntity = _iTable.All().OrderBy(m => m.TableId);
                var pRelationEntity = _iRelationShip.All().OrderBy(m => m.Id);
                var pTableTabEntity = _iTabletab.All().OrderBy(m => m.Id);
                foreach (Table tableObj in pTableEntity.ToList())
                {
                    bDoNotAdd = (bool)tableObj.Attachments;
                    if (!bDoNotAdd)
                    {
                        bDoNotAdd = CollectionsClass.IsEngineTable(tableObj.TableName.Trim().ToLower());
                        if (!bDoNotAdd)
                        {
                            bDoNotAdd = CollectionsClass.EngineTablesNotNeededList.Contains(tableObj.TableName.Trim().ToLower());
                        }
                        if (!bDoNotAdd)
                        {
                            bool lowerTable = pRelationEntity.Any(m => m.LowerTableName.Trim().ToLower().Equals(tableObj.TableName.Trim().ToLower()));
                            bool upperTable = pRelationEntity.Any(m => m.UpperTableName.Trim().ToLower().Equals(tableObj.TableName.Trim().ToLower()));
                            bDoNotAdd = lowerTable | upperTable;
                            if (!bDoNotAdd)
                            {
                                foreach (TableTab tableTabeVar in pTableTabEntity.ToList())
                                {
                                    bDoNotAdd = tableTabeVar.TableName.Trim().ToLower().Equals(tableObj.TableName.Trim().ToLower());
                                    if (bDoNotAdd)
                                    {
                                        break;
                                    }
                                }
                                if (!bDoNotAdd)
                                {
                                    if (tableObj.DBName is not null)
                                    {
                                        registerList.Add(new KeyValuePair<string, string>(tableObj.TableName, tableObj.DBName));
                                    }
                                    else
                                    {
                                        registerList.Add(new KeyValuePair<string, string>(tableObj.TableName, "TabFusionRMSDemoServer"));
                                    }
                                }
                            }
                        }
                    }
                }
                if (registerList is null)
                {
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlNotRegAnyTable");
                    Keys.ErrorType = "w";
                }
                else
                {
                    Keys.ErrorMessage = Keys.SaveSuccessMessage();
                    Keys.ErrorType = "s";
                }
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string RegisterBlock = JsonConvert.SerializeObject(registerList, Newtonsoft.Json.Formatting.Indented, Setting);
                return Json(new
                {
                    RegisterBlock,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
            catch (Exception)
            {
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                Keys.ErrorType = "e";
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
        }

        // Get list of added external database from 'Database' table and load 'Available database' list.
        public IActionResult GetAvailableDatabase()
        {
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                var pSystemDatabase = _iSystem.All().OrderBy(m => m.Id).FirstOrDefault();
                var pAvailableDatabase = from t in _iDatabas.All()
                                         select new { t.DBName, t.DBServer };
                var systemDBList = new List<KeyValuePair<string, string>>();
                systemDBList.Add(new KeyValuePair<string, string>(pSystemDatabase.UserName, "TabFusionRMSDemoServer"));
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string systemDB = JsonConvert.SerializeObject(systemDBList, Newtonsoft.Json.Formatting.Indented, Setting);
                string ExternalDB = JsonConvert.SerializeObject(pAvailableDatabase, Newtonsoft.Json.Formatting.Indented, Setting);
                return Json(new
                {
                    systemDB,
                    ExternalDB,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
            catch (Exception)
            {
                throw;
            } // ex
        }

        // Get list of all table names from 'Table' table which is not register yet and load 'Available table' list.
        public IActionResult GetAvailableTable(string dbName, string server)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            var unregisterList = new List<KeyValuePair<string, string>>();
            var schemaTableInfoList = new List<SchemaTable>();
            var schemaViewList = new List<SchemaTable>();
            var pTableEntity = _iTable.All().OrderBy(m => m.TableId);
            bool flag = false;
            bool bDoNotAdd;
            ADODB.Connection sADOConn = null;
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            try
            {
                var pDatabaseEntity = _iDatabas.All().Where(m => m.DBServer.Trim().ToLower().Equals(server.Trim().ToLower()) && m.DBName.Trim().ToLower().Equals(dbName.Trim().ToLower())).FirstOrDefault();
                sADOConn = DataServices.DBOpen(pDatabaseEntity);

                schemaTableInfoList = (List<SchemaTable>)SchemaTable.GetSchemaTable(sADOConn, Enums.geTableType.UserTables, "");
                schemaViewList = (List<SchemaTable>)SchemaTable.GetSchemaTable(sADOConn, Enums.geTableType.Views, "");
                if (schemaViewList is not null)
                {
                    foreach (SchemaTable schemaTableObj in schemaViewList)
                        schemaTableInfoList.Add(schemaTableObj);
                }
                if (schemaTableInfoList is not null)
                {
                    foreach (SchemaTable schemaTableObj in schemaTableInfoList.ToList())
                    {
                        bDoNotAdd = CollectionsClass.IsEngineTable(schemaTableObj.TableName.Trim().ToLower());
                        if (!bDoNotAdd)
                        {
                            bDoNotAdd = CollectionsClass.EngineTablesNotNeededList.Contains(schemaTableObj.TableName.Trim().ToLower());
                        }
                        if (!bDoNotAdd)
                        {
                            if (schemaTableObj.TableType.Trim().ToLower().Equals("view"))
                            {
                                bool vwVar = Strings.Left(schemaTableObj.TableName.Trim().ToLower(), Strings.Len("view_")).Equals("view_");
                                // Dim viewVar = (Left(schemaTableObj.TableName.Trim.ToLower, Len("vw")).Equals("vw"))
                                // bDoNotAdd = vwVar Or viewVar
                                bDoNotAdd = vwVar;
                            }
                        }
                        if (!bDoNotAdd)
                        {
                            foreach (Table tableObj in pTableEntity.ToList())
                            {
                                bDoNotAdd = tableObj.TableName.Trim().ToLower().Equals(schemaTableObj.TableName.Trim().ToLower());
                                if (bDoNotAdd == true)
                                {
                                    break;
                                }
                            }
                            if (!bDoNotAdd)
                            {
                                var tempVar = new SchemaTable();
                                if (Strings.Len(schemaTableObj.TableName.Trim().ToLower()) > 20)
                                {
                                    flag = true;
                                    tempVar = null;
                                }
                                else
                                {
                                    if (flag == true)
                                    {
                                        flag = true;
                                    }
                                    else
                                    {
                                        flag = false;
                                    }
                                    tempVar = schemaTableObj;

                                }
                                if (tempVar is not null)
                                {
                                    if (!server.Trim().ToLower().Equals("TabFusionRMSDemoServer".Trim().ToLower()))
                                    {
                                        unregisterList.Add(new KeyValuePair<string, string>(tempVar.TableName, dbName));
                                    }
                                    else
                                    {
                                        unregisterList.Add(new KeyValuePair<string, string>(tempVar.TableName, server));
                                    }
                                }
                            }
                        }
                    }
                }
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string unregisterListJSON = JsonConvert.SerializeObject(unregisterList, Newtonsoft.Json.Formatting.Indented, Setting);
                if (unregisterList.Count == 0)
                {
                    Keys.ErrorType = "w";
                }
                else
                {
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlTblBindSuccessfully");
                }
                return Json(new
                {
                    unregisterListJSON,
                    flag,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();

                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }

        }

        // Get primary or unique field of selected table and load 'Primary field'
        public IActionResult GetPrimaryField(string TableName, string ConName)
        {
            var PrimaryFieldList = new List<KeyValuePair<string, string>>();
            var pSchemaIndexList = new List<SchemaIndex>();
            var pSchemaIndexEntity = new SchemaIndex();
            var pSchemaColumnList = new List<SchemaColumns>();
            bool bUniqueKey = false;
            long lColumnSize = 0L;
            long lUserLinkIndexId = 30L;
            ADODB.Connection sADOConn = null;
            Databas pconDBObj;
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            try
            {
                pconDBObj = _iDatabas.All().Where(m => m.DBName.Trim().ToLower().Equals(ConName.Trim().ToLower())).FirstOrDefault();
                sADOConn = DataServices.DBOpen(pconDBObj);
                // First check for a Single Field, Unique, Primary Key
                pSchemaIndexList = SchemaIndex.GetTableIndexes(TableName, sADOConn);
                for (int iCount = 0, loopTo = pSchemaIndexList.Count - 1; iCount <= loopTo; iCount++)
                {
                    pSchemaIndexEntity = pSchemaIndexList[iCount];
                    if (Convert.ToBoolean(pSchemaIndexEntity.PrimaryKey) == true & Convert.ToBoolean(pSchemaIndexEntity.Unique) == true)
                    {
                        if (iCount < pSchemaIndexList.Count - 1)
                        {
                            if ((pSchemaIndexEntity.IndexName) != (pSchemaIndexList[iCount + 1].IndexName))
                            {
                                PrimaryFieldList.Add(new KeyValuePair<string, string>(pSchemaIndexEntity.ColumnName, pSchemaIndexEntity.TableName));
                                bUniqueKey = true;
                                break;
                            }
                        }
                        else
                        {
                            PrimaryFieldList.Add(new KeyValuePair<string, string>(pSchemaIndexEntity.ColumnName, pSchemaIndexEntity.TableName));
                            bUniqueKey = true;
                        }
                    }
                }

                // Second check for a Single Field, Unique Key (since the Primary key didn't qualify)
                if (!bUniqueKey)
                {
                    for (int iCount = 0, loopTo1 = pSchemaIndexList.Count - 1; iCount <= loopTo1; iCount++)
                    {
                        pSchemaIndexEntity = pSchemaIndexList[iCount];
                        if (Convert.ToBoolean(pSchemaIndexEntity.Unique) == true)
                        {
                            if (iCount < pSchemaIndexList.Count - 1)
                            {
                                if ((pSchemaIndexEntity.IndexName) != (pSchemaIndexList[iCount].IndexName))
                                {
                                    PrimaryFieldList.Add(new KeyValuePair<string, string>(pSchemaIndexEntity.ColumnName, pSchemaIndexEntity.TableName));
                                    bUniqueKey = true;
                                    break;
                                }
                            }
                            else
                            {
                                PrimaryFieldList.Add(new KeyValuePair<string, string>(pSchemaIndexEntity.ColumnName, pSchemaIndexEntity.TableName));
                                bUniqueKey = true;
                            }
                        }
                    }
                }

                // If there are no Single field, Unique Keys then list the columns
                pSchemaColumnList = SchemaInfoDetails.GetSchemaInfo(TableName, sADOConn);
                if (!bUniqueKey)
                {
                    foreach (SchemaColumns pSchemaColumnObj in pSchemaColumnList.ToList())
                    {
                        lColumnSize = (long)Convert.ToUInt64("0" + pSchemaColumnObj.CharacterMaxLength);
                        if (lColumnSize == 0L)
                        {
                            lColumnSize = (long)Convert.ToUInt64("0" + pSchemaColumnObj.NumericPrecision);
                        }
                        if (lColumnSize == 0L)
                        {
                            if (pSchemaColumnObj.IsADate)
                            {
                                lColumnSize = lUserLinkIndexId;
                            }
                        }
                        if (lColumnSize > 0L & lColumnSize <= lUserLinkIndexId)
                        {
                            PrimaryFieldList.Add(new KeyValuePair<string, string>(pSchemaColumnObj.ColumnName, pSchemaColumnObj.TableName));
                        }
                    }
                }
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string fieldListJSON = JsonConvert.SerializeObject(PrimaryFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                if (fieldListJSON.Count() == 0)
                {
                    Keys.ErrorType = "w";
                }
                else
                {
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlTblBindSuccessfully");
                }
                return Json(new
                {
                    fieldListJSON,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }

        }

        // Selected table will be register by adding record in 'Table' table
        public IActionResult SetRegisterTable(string dbName, string tbName, string fldName)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<ReportStyle> _iReportStyle = new Repositories<ReportStyle>();
            IRepository<TabFusionRMS.Models.Attribute> _iAttribute = new Repositories<TabFusionRMS.Models.Attribute>();
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();

            do
            {
                try
                {
                    _iView.BeginTransaction();
                    _iViewColumn.BeginTransaction();
                    _iTable.BeginTransaction();
                    _iSecureObject.BeginTransaction();
                    _iReportStyle.BeginTransaction();
                    var ReportStyle = _iReportStyle.All().OrderBy(m => m.Id).FirstOrDefault();
                    var Attribute = _iAttribute.All().OrderBy(m => m.Id).FirstOrDefault();
                    var TableTypeId = _iSecureObject.All().Where(m => m.Name == "Tables").FirstOrDefault();
                    var ViewTypeId = _iSecureObject.All().Where(m => m.Name == "Views").FirstOrDefault();
                    var moSecureObjectTable = _iSecureObject.All().Where(m => m.Name.Trim().ToLower().Equals(tbName.Trim().ToLower()) & m.SecureObjectTypeID.Equals(TableTypeId.SecureObjectTypeID)).FirstOrDefault();
                    var moSecureObjectView = _iSecureObject.All().Where(m => m.Name.Trim().Trim().ToLower().Equals(("All " + tbName).Trim().ToLower()) & m.SecureObjectTypeID.Equals(ViewTypeId.SecureObjectTypeID)).FirstOrDefault();
                    if (moSecureObjectTable is not null & moSecureObjectView is not null)
                    {
                        var moTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tbName.Trim().ToLower())).FirstOrDefault();
                        var moView = _iView.All().Where(m => m.TableName.Trim().ToLower().Equals(tbName.Trim().ToLower())).FirstOrDefault();
                        if (moTable is not null)
                        {
                            _iTable.Delete(moTable);
                        }
                        if (moView is not null)
                        {
                            _iView.Delete(moView);
                            var moViewColumn = _iViewColumn.All().Where(m => m.ViewsId == moView.Id);
                            if (moViewColumn is not null)
                            {
                                _iViewColumn.DeleteRange(moViewColumn);
                            }
                        }
                        _iSecureObject.Delete(moSecureObjectView);
                        _iSecureObject.Delete(moSecureObjectTable);

                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlSecObjNameAlreadyReg");
                        _iView.CommitTransaction();
                        _iViewColumn.CommitTransaction();
                        _iTable.CommitTransaction();
                        _iSecureObject.CommitTransaction();
                        break;
                    }



                    var viewEntity = new View();
                    var tableEntity = new Table();
                    var viewColumnEntity = new ViewColumn();
                    var tableObjectEntity = new TabFusionRMS.Models.SecureObject();
                    var pMaxSearchOrder = _iTable.All().Max(x => x.SearchOrder);
                    int pSecureObjectID;
                    int pSecureBaseID;

                    if (Attribute is null)
                    {
                        tableEntity.AttributesID = 0;
                    }
                    else
                    {
                        tableEntity.AttributesID = Attribute.Id;
                    }
                    if (!dbName.Trim().ToLower().Equals("TabFusionRMSDemoServer".Trim().ToLower()))
                    {
                        tableEntity.DBName = dbName;
                    }
                    tableEntity.TableName = tbName;
                    tableEntity.UserName = tbName;
                    tableEntity.TrackingStatusFieldName = null;
                    tableEntity.CounterFieldName = null;
                    tableEntity.AddGroup = 0;
                    tableEntity.DelGroup = 0;
                    tableEntity.EditGroup = 0;
                    tableEntity.MgrGroup = 0;
                    tableEntity.ViewGroup = 0;
                    tableEntity.PCFilesEditGrp = 0;
                    tableEntity.PCFilesNVerGrp = 0;
                    tableEntity.Attachments = false;
                    tableEntity.IdFieldName = tbName + "." + fldName;
                    tableEntity.TrackingTable = (short?)0;
                    tableEntity.Trackable = false;
                    tableEntity.DescFieldPrefixOneWidth = (short?)0;
                    tableEntity.DescFieldPrefixTwoWidth = (short?)0;
                    tableEntity.MaxRecordsAllowed = (short?)0;
                    tableEntity.OutTable = (short?)0;
                    tableEntity.RestrictAddToTable = (short?)0;
                    tableEntity.MaxRecsOnDropDown = 0;
                    tableEntity.ADOServerCursor = Convert.ToBoolean(0);
                    tableEntity.ADOQueryTimeout = 30;
                    tableEntity.ADOCacheSize = 1;
                    tableEntity.DeleteAttachedGroup = 9999;
                    tableEntity.RecordManageMgmtType = 0;
                    tableEntity.SearchOrder = pMaxSearchOrder + 1;
                    _iTable.Add(tableEntity);
                    if (ReportStyle is not null)
                    {
                        viewEntity.ReportStylesId = ReportStyle.Id;
                    }
                    viewEntity.AltViewId = 0;
                    viewEntity.DeleteGridAvail = false;
                    viewEntity.SearchableView = true;
                    viewEntity.FiltersActive = false;
                    viewEntity.SQLStatement = "SELECT * FROM [" + tbName + "]";
                    viewEntity.SearchableView = true;
                    viewEntity.RowHeight = (short?)0;
                    viewEntity.TableName = tbName;
                    viewEntity.TablesId = 0;
                    viewEntity.UseExactRowCount = false;
                    viewEntity.VariableColWidth = true;
                    viewEntity.VariableFixedCols = false;
                    viewEntity.VariableRowHeight = true;
                    viewEntity.ViewGroup = 0;
                    viewEntity.ViewName = "All " + tbName;
                    viewEntity.ViewOrder = 1;
                    viewEntity.ViewType = (short?)0;
                    viewEntity.Visible = true;
                    // 25 is the new default as it is only used for Web Access 
                    // and new default of 5000 for desktop.  RVW 03/06/2019
                    viewEntity.MaxRecsPerFetch = viewEntity.MaxRecsPerFetch;
                    viewEntity.MaxRecsPerFetchDesktop = viewEntity.MaxRecsPerFetchDesktop;
                    _iView.Add(viewEntity);

                    bool editAllowed = true;
                    DataSet loutput = null;
                    if (!string.IsNullOrWhiteSpace(fldName))
                    {
                        string strqry = "SELECT [" + fldName + "] FROM [" + tbName + "] Where 0=1;";
                        _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
                        loutput = _IDBManager.ExecuteDataSetWithSchema(CommandType.Text, strqry);
                        _IDBManager.Dispose();
                    }
                    if (loutput is not null)
                    {
                        if (loutput.Tables[0].Columns[0].AutoIncrement)
                            editAllowed = false;
                    }

                    var View = _iView.All().Where(m => (m.ViewName) == ("All " + tbName)).FirstOrDefault();
                    viewColumnEntity.ViewsId = View.Id;
                    viewColumnEntity.ColumnNum = (short?)0;
                    viewColumnEntity.ColumnWidth = (short?)3000;
                    viewColumnEntity.EditAllowed = editAllowed;
                    viewColumnEntity.FieldName = tbName + "." + fldName;
                    viewColumnEntity.Heading = fldName;
                    viewColumnEntity.FilterField = true;
                    viewColumnEntity.FreezeOrder = 1;
                    viewColumnEntity.SortOrder = 1;
                    viewColumnEntity.LookupType = (short?)0;
                    viewColumnEntity.SortableField = true;
                    viewColumnEntity.ColumnStyle = (short?)0;
                    viewColumnEntity.DropDownFlag = (short?)0;
                    viewColumnEntity.DropDownReferenceColNum = (short?)0;
                    viewColumnEntity.FormColWidth = 0;
                    viewColumnEntity.LookupIdCol = (short?)0;
                    viewColumnEntity.MaskClipMode = false;
                    viewColumnEntity.MaskInclude = false;
                    viewColumnEntity.MaskPromptChar = "_";
                    viewColumnEntity.MaxPrintLines = 1;
                    viewColumnEntity.PageBreakField = false;
                    viewColumnEntity.PrinterColWidth = 0;
                    viewColumnEntity.SortOrderDesc = false;
                    viewColumnEntity.SuppressDuplicates = false;
                    viewColumnEntity.SuppressPrinting = false;
                    viewColumnEntity.VisibleOnForm = true;
                    viewColumnEntity.VisibleOnPrint = true;
                    viewColumnEntity.CountColumn = false;
                    viewColumnEntity.SubtotalColumn = false;
                    viewColumnEntity.PrintColumnAsSubheader = false;
                    viewColumnEntity.RestartPageNumber = false;
                    viewColumnEntity.LabelJustify = 1;
                    viewColumnEntity.LabelLeft = -1;
                    viewColumnEntity.LabelTop = -1;
                    viewColumnEntity.LabelWidth = -1;
                    viewColumnEntity.LabelHeight = -1;
                    viewColumnEntity.ControlLeft = -1;
                    viewColumnEntity.ControlTop = -1;
                    viewColumnEntity.ControlWidth = -1;
                    viewColumnEntity.ControlHeight = -1;
                    viewColumnEntity.TabOrder = -1;
                    viewColumnEntity.ColumnOrder = (short?)0;
                    _iViewColumn.Add(viewColumnEntity);

                    if (TableTypeId is not null)
                    {
                        tableObjectEntity.SecureObjectTypeID = TableTypeId.SecureObjectTypeID;
                        tableObjectEntity.BaseID = TableTypeId.SecureObjectTypeID;
                    }
                    tableObjectEntity.Name = tbName;
                    _iSecureObject.Add(tableObjectEntity);
                    _iSecureObject.CommitTransaction();
                    // Added by Ganesh on 22nd Sep 2015
                    pSecureObjectID = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals(tbName)).FirstOrDefault().SecureObjectID;
                    pSecureBaseID = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals(tbName)).FirstOrDefault().BaseID;
                    AddSecureObjectPermissionsBySecureObjectType(pSecureObjectID, pSecureBaseID, (int)Enums.SecureObjects.Table);
                    pSecureObjectID = default;
                    _iSecureObject.BeginTransaction();


                    var RecentTableBaseId = _iSecureObject.All().Where(m => (m.Name) == (tbName)).FirstOrDefault();
                    var viewObjectEntity = new TabFusionRMS.Models.SecureObject();

                    if (ViewTypeId is not null)
                    {
                        viewObjectEntity.SecureObjectTypeID = ViewTypeId.SecureObjectTypeID;
                    }

                    if (RecentTableBaseId is not null)
                    {
                        viewObjectEntity.BaseID = RecentTableBaseId.SecureObjectID;
                    }
                    viewObjectEntity.Name = "All " + tbName;
                    _iSecureObject.Add(viewObjectEntity);
                    _iSecureObject.CommitTransaction();
                    // Added by Ganesh on 22nd Sep 2015
                    pSecureObjectID = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals("All " + tbName)).FirstOrDefault().SecureObjectID;
                    pSecureBaseID = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals("All " + tbName)).FirstOrDefault().BaseID;
                    AddSecureObjectPermissionsBySecureObjectType(pSecureObjectID, pSecureBaseID, (int)Enums.SecureObjects.View);

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Keys.SaveSuccessMessage();
                    // Fill the secure permissions dataset after SetRegisterTable
                    CollectionsClass.ReloadPermissionDataSet(passport);

                    _iView.CommitTransaction();
                    _iViewColumn.CommitTransaction();
                    _iTable.CommitTransaction();

                    _iReportStyle.CommitTransaction();
                }

                catch (Exception)
                {
                    _iView.RollBackTransaction();
                    _iViewColumn.RollBackTransaction();
                    _iTable.RollBackTransaction();
                    _iSecureObject.RollBackTransaction();
                    _iReportStyle.RollBackTransaction();
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Selected table will be unregister by removing record from 'Table' table but no data will be lost
        public IActionResult UnRegisterTable(string dbName, string tbName)
        {
            Table tableEntity;
            int vSecureObjectId;
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();

            do
            {
                try
                {
                    _iView.BeginTransaction();
                    _iViewColumn.BeginTransaction();
                    _iTable.BeginTransaction();
                    _iSecureObject.BeginTransaction();
                    // Added By Ganesh
                    _iSecureObjectPermission.BeginTransaction();
                    // Delete views entry from 'Views','ViewColumns' and 'Secure Object' table in order to unregister a table
                    var oTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tbName.Trim().ToLower())).FirstOrDefault();
                    if (oTable is not null)
                    {
                        if (oTable.TrackingTable != 0)
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgJsTableCanNotBeUnregister"), oTable.TableName);
                            break;
                        }
                    }
                    var viewsEntity = _iView.All().Where(m => (m.TableName.Trim().ToLower()) == (tbName.Trim().ToLower()));
                    foreach (View viewVar in viewsEntity.ToList())
                    {
                        _iView.Delete(viewVar);
                        var viewColumnsEntity = _iViewColumn.All().Where(m => m.ViewsId == viewVar.Id);
                        foreach (ViewColumn viewColumnVar in viewColumnsEntity.ToList())
                            _iViewColumn.Delete(viewColumnVar);
                        var secureViewObject = _iSecureObject.All().Where(m => (m.Name.Trim().ToLower()) == (viewVar.ViewName.Trim().ToLower())).FirstOrDefault();
                        if (secureViewObject is not null)
                        {
                            _iSecureObject.Delete(secureViewObject);
                        }

                    }

                    // Delete table entry from 'Table' and 'Secure Object' table in order to unregister a table
                    if (dbName.Trim().ToLower().Equals("TabFusionRMSDemoServer".Trim().ToLower()))
                    {
                        tableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tbName.Trim().ToLower())).FirstOrDefault();
                    }
                    else
                    {
                        tableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tbName.Trim().ToLower()) && m.DBName.Trim().ToLower().Equals(dbName.Trim().ToLower())).FirstOrDefault();
                    }

                    if (tableEntity is not null)
                    {
                        _iTable.Delete(tableEntity);
                        var secureTableObject = _iSecureObject.All().Where(m => (m.Name.Trim().ToLower()) == (tbName.Trim().ToLower())).FirstOrDefault();
                        // Added by Ganesh
                        vSecureObjectId = _iSecureObject.All().Where(m => (m.Name.Trim().ToLower()) == (tbName.Trim().ToLower())).FirstOrDefault().SecureObjectID;
                        var pSecureObjPermissions = _iSecureObjectPermission.All().Where(x => x.SecureObjectID == vSecureObjectId);
                        if (secureTableObject is not null)
                        {
                            _iSecureObject.Delete(secureTableObject);
                            _iSecureObject.CommitTransaction();
                            _iSecureObjectPermission.DeleteRange(pSecureObjPermissions);
                        }

                    }

                    // Fill the secure permissions dataset after UnRegisterTable
                    CollectionsClass.ReloadPermissionDataSet(passport);

                    _iView.CommitTransaction();
                    _iViewColumn.CommitTransaction();
                    _iTable.CommitTransaction();
                    // _iSecureObject.CommitTransaction()
                    _iSecureObjectPermission.CommitTransaction();
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgJsTableRegisterTblUnRegSuccessfully");
                }
                catch (Exception)
                {
                    _iView.RollBackTransaction();
                    _iViewColumn.RollBackTransaction();
                    _iTable.RollBackTransaction();
                    _iSecureObject.RollBackTransaction();
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Table will be permannetly drop from table containing database
        public IActionResult DropTable(string dbName, string tbName)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<SLIndexer> _iSLIndexer = new Repositories<SLIndexer>();
            IRepository<SLIndexerCache> _iSLIndexerCache = new Repositories<SLIndexerCache>();
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            do
            {
                try
                {
                    var oTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tbName.Trim().ToLower())).FirstOrDefault();

                    if ((oTable != null) && (oTable.TrackingTable != 0))
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = string.Format(Languages.get_Translation("msgJsTableCanNotBeDropped"), oTable.TableName);
                        break;
                    }

                    JsonResult jsonVar = (JsonResult)UnRegisterTable(dbName, tbName);
                    string Serialvalue = JsonConvert.SerializeObject(jsonVar.Value);
                    bool unregisterSuccessful = Serialvalue.ToLower().Contains(Languages.get_Translation("msgJsTableRegisterTblUnRegSuccessfully").ToLower());

                    var bFoundCounter = default(bool);
                    string DatabaseName = string.Empty;
                    string ServerInstance = string.Empty;
                    var sADOConn = new ADODB.Connection();
                    var pDatabaseEntity = new Databas();
                    string conStr = Keys.get_GetDBConnectionString(); // System.Configuration.ConfigurationManager.ConnectionStrings("TABFusionRMSRMSDemoContext").ConnectionString

                    _iSLIndexer.BeginTransaction();
                    _iSLIndexerCache.BeginTransaction();
                    _iSystem.BeginTransaction();

                    if (unregisterSuccessful)
                    {
                        pDatabaseEntity = _iDatabas.All().Where(m => m.DBName.Trim().ToLower().Equals(dbName.Trim().ToLower())).FirstOrDefault();
                        sADOConn = DataServices.DBOpen(pDatabaseEntity);
                        var conStrList = new List<KeyValuePair<string, string>>();
                        conStrList = Keys.GetParamFromConnString(httpContext,pDatabaseEntity);

                        foreach (KeyValuePair<string, string> conList in conStrList)
                        {
                            if (conList.Key.Equals("DBDatabase"))
                            {
                                DatabaseName = conList.Value;
                            }
                            else if (conList.Key.Equals("DBServer"))
                            {
                                ServerInstance = conList.Value;
                            }
                        }

                        if (DatabaseMap.DropTable(tbName, sADOConn))
                        {
                            var indexerEntity = _iSLIndexer.All().Where(m => m.IndexTableName.Trim().ToLower().Equals(tbName.Trim().ToLower()));
                            _iSLIndexer.DeleteRange(indexerEntity);
                            var indexerCacheEntity = _iSLIndexerCache.All().Where(m => m.IndexTableName.Trim().ToLower().Equals(tbName.Trim().ToLower()));
                            _iSLIndexerCache.DeleteRange(indexerCacheEntity);
                            var schemaInfoEntity = SchemaInfoDetails.GetSchemaInfo("System", DataServices.DBOpen());

                            if (schemaInfoEntity is not null)
                            {
                                for (int iCount = 0, loopTo = schemaInfoEntity.Count - 1; iCount <= loopTo; iCount++)
                                {
                                    bFoundCounter = schemaInfoEntity[iCount].ColumnName.Trim().ToLower().Equals((tbName + "Counter").Trim().ToLower());
                                    if (bFoundCounter)
                                        break;
                                }

                                if (bFoundCounter)
                                {
                                    using (var con = new SqlConnection(conStr))
                                    {
                                        try
                                        {
                                            con.Open();
                                            if (Convert.ToBoolean(con.State))
                                            {
                                                string sSql = "ALTER TABLE System DROP COLUMN [" + tbName + "Counter]";
                                                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString(null);
                                                _IDBManager.ExecuteNonQuery(CommandType.Text, sSql);
                                                _IDBManager.Dispose();
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            if (Convert.ToBoolean(~con.State))
                                            {
                                                Keys.ErrorType = "e";
                                                Keys.ErrorMessage = Keys.UnableToConnect(ServerInstance);
                                            }
                                            else
                                            {
                                                Keys.ErrorType = "e";
                                                Keys.ErrorMessage = Keys.ErrorMessageJS();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlTblDropSuccessfully");
                    _iSLIndexer.CommitTransaction();
                    _iSLIndexerCache.CommitTransaction();
                    _iSystem.CommitTransaction();
                }
                catch (Exception)
                {
                    _iSLIndexer.RollBackTransaction();
                    _iSLIndexerCache.RollBackTransaction();
                    _iSystem.RollBackTransaction();
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        #endregion

        #endregion

        #region Data Module

        public PartialViewResult LoadDataView()
        {
            return PartialView("_DataPartial");
        }

        public JsonResult GetDataList(string pTabledName, string sidx, string sord, int page, int rows)
        {

            DataTable dtRecords = null;
            int totalRecords = 0;
            IRepository<Table> _iTable = new Repositories<Table>();
            var pTableEntity = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pTabledName.Trim().ToLower())).FirstOrDefault();
            object pDatabaseEntity = null;
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            if (pTableEntity is not null)
            {
                if (pTableEntity.DBName is not null)
                {
                    pDatabaseEntity = _iDatabas.Where(x => x.DBName.Trim().ToLower().Equals(pTableEntity.DBName.Trim().ToLower())).FirstOrDefault();
                }
                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString((Databas)pDatabaseEntity);
            }
            else
            {
                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
            }
            _IDBManager.CreateParameters(6);
            _IDBManager.AddParameters(0, "@TableName", pTabledName);
            _IDBManager.AddParameters(1, "@PageNo", page);
            _IDBManager.AddParameters(2, "@PageSize", rows);
            _IDBManager.AddParameters(3, "@DataAndColumnInfo", true);
            _IDBManager.AddParameters(4, "@ColName", sidx);
            _IDBManager.AddParameters(5, "@Sort", sord);
            var loutput = _IDBManager.ExecuteDataSetWithSchema(CommandType.StoredProcedure, "SP_RMS_GetTableData");
            _IDBManager.Dispose();
            dtRecords = loutput.Tables[0];

            if (dtRecords is null)
            {
                return Json(Languages.get_Translation("msgAdminCtrlNullValue"));
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                dtRecords = dtRecords;
                if (dtRecords.Columns.Contains("TotalCount"))
                {
                    if (dtRecords.Rows.Count != 0)
                    {
                        totalRecords = Convert.ToInt32(dtRecords.AsEnumerable().ElementAtOrDefault(0)["TotalCount"]);
                    }
                    dtRecords.Columns.Remove("TotalCount");

                }
                if (dtRecords.Columns.Contains("ROWNUM"))
                {
                    dtRecords.Columns.Remove("ROWNUM");
                }
            }

            return Json(Common.ConvertDTToJQGridResult(dtRecords, totalRecords, sidx, sord, page, rows));
        }

        [HttpPost]
        public IActionResult DeleteSelectedRows(string tablename, string rows, string col)
        {

            var RowID = new DataTable();
            var row = rows.Split(',');
            RowID.Columns.Add("ID", typeof(string));
            RowID.Columns.Add("Col", typeof(string));
            int i = 0;
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            foreach (string value in row)
            {
                RowID.Rows.Add(row[i], col);
                i = i + 1;
            }
            // For value As String = row.First To row.Last
            // RowID.Rows.Add(row(value))
            // Next

            var parameter = new SqlParameter("@ID", SqlDbType.Structured);
            parameter.Value = RowID;
            parameter.TypeName = "dbo.RowID";

            var pTable = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(tablename.Trim().ToLower()) && !string.IsNullOrEmpty(x.DBName.Trim().ToLower())).FirstOrDefault();

            object pDatabaseEntity = null;
            if (pTable is not null)
            {
                pDatabaseEntity = _iDatabas.Where(x => x.DBName.Trim().ToLower().Equals(pTable.DBName.Trim().ToLower())).FirstOrDefault();
                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString((Databas)pDatabaseEntity);
            }
            else
            {
                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
            }

            _IDBManager.CreateParameters(3);
            _IDBManager.AddParameters(0, "@TableType", RowID);
            _IDBManager.AddParameters(1, "@TableName", tablename);
            _IDBManager.AddParameters(2, "@ColName", col);
            int loutput = _IDBManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_RMS_DeleteDataRecords");
            _IDBManager.Dispose();

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });

        }

        [HttpPost]
        //   [ValidateInput(false)]
        public string ProcessRequest()
        {

            var forms = httpContext.Request.Form;

            string data = forms["x01"];

            string tableName = forms["x02"];
            string colName = forms["x03"];
            string colType = forms["x04"];
            string columnName = forms["x05"];
            string pkValue = forms["x07"];
            string columnValue;
            // Dim IDBManagar As DBManager
            IRepository<Databas> _iDatabas = new Repositories<Databas>();

            IRepository<Table> _iTable = new Repositories<Table>();
            var jsonObject = JsonConvert.DeserializeObject<JObject>(data);
            var jsonType = JsonConvert.DeserializeObject<JObject>(colType);
            var strOperation = jsonObject.GetValue("oper");
            if (columnName.Trim().ToLower().Equals("id"))
            {
                if (forms["x06"].Contains("<"))
                {
                    columnValue = jsonObject.GetValue(columnName).ToString();
                }
                else
                {
                    columnValue = forms["x06"];
                }
            }

            // columnValue = jsonObject.GetValue(columnName).ToString()
            else if (forms["x06"].Equals(""))
            {
                columnValue = null;
            }
            else
            {
                columnValue = forms["x06"];
                // columnValue = jsonObject.GetValue(columnName).ToString()
            }

            var AddEditType = new DataTable();
            AddEditType.Columns.Add("Col_Name", typeof(string));
            AddEditType.Columns.Add("Col_Data", typeof(string));

            var cNames = colName.Split(',');

            object val = "";

            for (int value = 0, loopTo = cNames.Length - 1; value <= loopTo; value++)
            {
                var types = jsonType.GetValue(cNames[value]);

                object type = null;
                string incremented = "true";
                string readOnlye = "false";

                if (Convert.ToBoolean(Operators.ConditionalCompareObjectNotEqual(types, null, false)))
                {
                    type = types.ToString().Split(',')[0];
                    incremented = types.ToString().Split(',')[1];
                    readOnlye = types.ToString().Split(',')[2];
                }

                if (readOnlye != "true")
                {
                    switch (type)
                    {
                        case "String":
                            {
                                string str = jsonObject.GetValue(cNames[value]).ToString();
                                // Change on 09/08/2016 - Tejas
                                // If String.IsNullOrEmpty(str) Then

                                if (str.IndexOf("'") > -1)
                                {
                                    str = str.Replace("'", Convert.ToString(ControlChars.Quote));
                                }

                                val = str;
                                break;
                            }

                        // Else
                        // val = jsonObject.GetValue(cNames(value)).ToString
                        // End If
                        case "Int32":
                        case "Int64":
                        case "Int16":
                            {
                                if ((cNames[value]) != (columnName))
                                {
                                    string intr = jsonObject.GetValue(cNames[value]).ToString();
                                    if (string.IsNullOrEmpty(intr))
                                    {
                                        val = jsonObject.GetValue(intr);
                                    }
                                    else
                                    {
                                        decimal round = Math.Round(decimal.Parse(jsonObject.GetValue(cNames[value]).ToString()));
                                        val = int.Parse(round.ToString());
                                    }
                                }
                                else if (incremented.Equals("false"))
                                {
                                    if (!string.IsNullOrEmpty(jsonObject.GetValue(cNames[value]).ToString()))
                                    {
                                        val = int.Parse(jsonObject.GetValue(cNames[value]).ToString());
                                    }

                                }

                                break;
                            }
                        case "Double":
                            {
                                string str = jsonObject.GetValue(cNames[value]).ToString();
                                if (string.IsNullOrEmpty(str))
                                {
                                    val = jsonObject.GetValue(str);
                                }
                                else
                                {
                                    val = jsonObject.GetValue(cNames[value]).ToString();
                                }

                                break;
                            }
                        case "Decimal":
                            {
                                string str = jsonObject.GetValue(cNames[value]).ToString();
                                if (string.IsNullOrEmpty(str))
                                {
                                    val = jsonObject.GetValue(str);
                                }
                                else
                                {
                                    val = jsonObject.GetValue(cNames[value]).ToString();
                                }

                                break;
                            }
                        case "DateTime":
                            {
                                string dates = jsonObject.GetValue(cNames[value]).ToString();
                                if (string.IsNullOrEmpty(dates))
                                {
                                    val = jsonObject.GetValue(dates);
                                }
                                else
                                {
                                    var argresult = new DateTime();
                                    if (DateTime.TryParse(dates, out argresult))
                                    {
                                        if (dates.IndexOf(":") > -1)
                                        {
                                            val = DateTime.Parse(dates).ToString(CultureInfo.InvariantCulture);
                                        }
                                        // val = Date.ParseExact(dates, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy HH:mm")
                                        else
                                        {
                                            val = DateTime.Parse(dates).ToString("MM/dd/yyyy");
                                        }
                                    }
                                    // val = Date.ParseExact(jsonObject.GetValue(cNames(value)).ToString(), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy")
                                    else
                                    {
                                        val = DateTime.ParseExact(dates, "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy HH:mm");
                                        // val = DateTime.Parse(dates)
                                        // val = Date.ParseExact(jsonObject.GetValue(cNames(value)).ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("MM/dd/yyyy")
                                    }
                                }

                                break;
                            }
                        case "Byte[]":
                            {
                                val = Constants.vbByte;
                                break;
                            }
                        case "Boolean":
                            {
                                val = jsonObject.GetValue(cNames[value]);
                                break;
                            }

                        default:
                            {
                                val = jsonObject.GetValue(cNames[value]);
                                break;
                            }
                    }
                    if (Convert.ToBoolean(Operators.AndObject(Operators.AndObject(Operators.ConditionalCompareObjectNotEqual(type, "Byte[]", false), (cNames[value]) != (columnName)), Operators.ConditionalCompareObjectNotEqual(type, null, false))))
                    {
                        if (!(strOperation.ToString().Equals("edit") & val is null))
                        {
                            AddEditType.Rows.Add(cNames[value], val);
                        }
                    }
                    else if (cNames[value].Equals(columnName) & incremented.Equals("false"))
                    {
                        AddEditType.Rows.Add(cNames[value], val);
                    }
                }
            }

            int n;

            var pTable = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower()) && !string.IsNullOrEmpty(x.DBName.Trim().ToLower())).FirstOrDefault();

            object pDatabaseEntity = null;
            if (pTable is not null)
            {
                pDatabaseEntity = _iDatabas.Where(x => x.DBName.Trim().ToLower().Equals(pTable.DBName.Trim().ToLower())).FirstOrDefault();
                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString((Databas)pDatabaseEntity);
            }
            else
            {
                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
            }
            if (strOperation.ToString().Equals("add"))
            {
                n = 2;
                _IDBManager.CreateParameters(n);
                _IDBManager.AddParameters(0, "@TableType", AddEditType);
                _IDBManager.AddParameters(1, "@TableName", tableName);
            }
            else
            {
                n = 4;
                _IDBManager.CreateParameters(n);
                _IDBManager.AddParameters(0, "@TableType", AddEditType);
                _IDBManager.AddParameters(1, "@TableName", tableName);
                _IDBManager.AddParameters(2, "@ColName", columnName);
                if (pkValue is null)
                {
                    _IDBManager.AddParameters(3, "@ColVal", columnValue);
                }
                else
                {
                    _IDBManager.AddParameters(3, "@ColVal", pkValue);
                }

            }
            try
            {
                if (strOperation.ToString().Equals("add"))
                {
                    int loutput = _IDBManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_RMS_AddDataRecords");
                    _IDBManager.Dispose();
                    return Languages.get_Translation("RecordSavedSuccessfully");
                }
                else
                {
                    int loutput = _IDBManager.ExecuteNonQuery(CommandType.StoredProcedure, "SP_RMS_EditDataRecords");
                    _IDBManager.Dispose();
                    return Languages.get_Translation("RecordUpdatedSuccessfully");
                }
            }
            catch (SqlException sql)
            {
                _IDBManager.Dispose();
                return string.Format(Languages.get_Translation("msgAdminCtrlCantInsertDupKeyInObjXDulKeyValIsX"), columnName, columnValue);
            }
            catch (Exception ex)
            {
                _IDBManager.Dispose();
                return string.Format(Languages.get_Translation("msgAdminCtrlError"), ex.Message);
            }
        }

        #endregion

        #region Directories Module
        public PartialViewResult LoadDirectoriesView()
        {
            return PartialView("_DirectoriesPartial");
        }

        #region Drive Details
        public JsonResult GetSystemAddressList(string sidx, string sord, int page, int rows)
        {
            IRepository<SystemAddress> _iSystemAddress = new Repositories<SystemAddress>();
            var pSystemAddressEntities = from t in _iSystemAddress.All()
                                         select new { t.Id, t.DeviceName, t.PhysicalDriveLetter };

            var jsonData = pSystemAddressEntities.GetJsonListForGrid(sord, page, rows, "DeviceName");
            return Json(jsonData);
        }

        public PartialViewResult LoadDriveView()
        {
            return PartialView("_AddDirectoriesPartial");
        }

        [HttpPost]
        public IActionResult SetSystemAddressDetails(SystemAddress pSystemAddress)
        {

            do
            {
                try
                {
                    IRepository<SystemAddress> _iSystemAddress = new Repositories<SystemAddress>();
                    if (pSystemAddress.Id > 0)
                    {
                        if (_iSystemAddress.All().Any(x => (x.DeviceName.Trim().ToLower()) == (pSystemAddress.DeviceName.Trim().ToLower()) && x.Id != pSystemAddress.Id) == false)
                        {
                            pSystemAddress.PhysicalDriveLetter = pSystemAddress.PhysicalDriveLetter.ToUpper();
                            _iSystemAddress.Update(pSystemAddress);
                        }
                        else
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlDeviceIsAlreadyInUseL1"), pSystemAddress.DeviceName);
                            break;
                        }
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminDriveUpdateSuccess"); // Keys.SaveSuccessMessage()
                    }
                    else
                    {
                        if (_iSystemAddress.All().Any(x => (x.DeviceName.Trim().ToLower()) == (pSystemAddress.DeviceName.Trim().ToLower())) == false)
                        {
                            pSystemAddress.PhysicalDriveLetter = pSystemAddress.PhysicalDriveLetter.ToUpper();
                            _iSystemAddress.Add(pSystemAddress);
                        }
                        else
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlDeviceIsAlreadyInUseL1"), pSystemAddress.DeviceName);
                            break;
                        }
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminDriveSaveSuccess");
                    } // Keys.SaveSuccessMessage()
                }

                catch (Exception)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        public IActionResult EditSystemAddress([FromForm] string[] pRowSelected)
        {

            if (pRowSelected is null)
            {
                return Json(new { success = false });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { success = false });
            }

            int pSystemAddressId = Convert.ToInt32(pRowSelected.GetValue(0));
            if (pSystemAddressId == 0)
            {
                return Json(new { success = false });
            }
            IRepository<SystemAddress> _iSystemAddress = new Repositories<SystemAddress>();
            var pSystemAddressEntity = _iSystemAddress.All().Where(x => x.Id == pSystemAddressId).FirstOrDefault();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(pSystemAddressEntity, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        public JsonResult IsValidFilename(string PhysicalDriveLetter)
        {
            if (PhysicalDriveLetter.Contains("hasu"))
            {
                return Json(new { Data = false });
            }
            return Json(new { Data = true });
        }

        public IActionResult DeleteSystemAddress([FromForm] string[] pRowSelected)
        {
            if (pRowSelected is null)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
            }

            int iSystemAddressId;
            try
            {
                IRepository<SystemAddress> _iSystemAddress = new Repositories<SystemAddress>();
                string pSystemAddressId = pRowSelected.GetValue(0).ToString();
                if (string.IsNullOrWhiteSpace(pSystemAddressId))
                {
                    return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
                }
                iSystemAddressId = Convert.ToInt32(pSystemAddressId);
                var oSystemAddressEntity = _iSystemAddress.All().Where(x => x.Id == iSystemAddressId).FirstOrDefault();
                if (oSystemAddressEntity is not null)
                {
                    IRepository<Volume> _iVolume = new Repositories<Volume>();
                    var oVolumns = _iVolume.All().Where(x => x.SystemAddressesId == iSystemAddressId).FirstOrDefault();


                    if (oVolumns is not null)
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRowHasValAssigned");
                    }
                    else
                    {
                        _iSystemAddress.Delete(oSystemAddressEntity);

                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminDriveDeleteSuccess");
                    }
                }
                else
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlNoRecForDelete");
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        #endregion

        #region Volumes Details

        public JsonResult GetVolumesList(string sidx, string sord, int page, int rows, string pId)
        {
            IRepository<Volume> _iVolume = new Repositories<Volume>();
            var pVolumeEntities = from t in _iVolume.All()
                                  select new { t.Id, t.Name, t.PathName, t.DirDiskMBLimitation, t.DirCountLimitation, t.Active, t.ImageTableName, t.SystemAddressesId };

            if (!string.IsNullOrEmpty(pId))
            {
                int intpId = Convert.ToInt32(pId);
                pVolumeEntities = pVolumeEntities.Where(p => p.SystemAddressesId == intpId);
            }

            var jsonData = pVolumeEntities.GetJsonListForGrid(sord, page, rows, "Name");

            return Json(jsonData);
        }

        public PartialViewResult LoadVolumeView()
        {
            return PartialView("_VolumesPartial");
        }

        [HttpPost]
        public IActionResult SetVolumeDetails(Volume pVolume, bool pActive)
        {
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
            do
            {
                try
                {
                    IRepository<Volume> _iVolume = new Repositories<Volume>();
                    var oSecureObject = new Smead.Security.SecureObject(passport);
                    pVolume.Active = pActive;
                    pVolume.Active = pActive;


                    if (pVolume.Id > 0)
                    {
                        var pVolumnEntity = _iVolume.All().Where(x => x.Id == pVolume.Id).FirstOrDefault();
                        string oldVolumnName = pVolumnEntity.Name;

                        if (_iVolume.All().Any(x => (x.Name.Trim().ToLower()) == (pVolume.Name.Trim().ToLower()) && x.Id != pVolume.Id) == false)
                        {
                            if (pVolume.PathName.Substring(0, 1) != @"\")
                            {
                                pVolume.PathName = @"\" + pVolume.PathName;
                            }
                            pVolumnEntity.Name = pVolume.Name;
                            pVolumnEntity.PathName = pVolume.PathName;
                            pVolumnEntity.DirDiskMBLimitation = pVolume.DirDiskMBLimitation;
                            pVolumnEntity.DirCountLimitation = pVolume.DirCountLimitation;
                            pVolumnEntity.Active = pVolume.Active;
                            // pVolumnEntity.OfflineLocation = pVolume.OfflineLocation
                            pVolumnEntity.ImageTableName = pVolume.ImageTableName;

                            if (Strings.StrComp(oldVolumnName, pVolume.Name, Constants.vbTextCompare) != 0)
                            {
                                var oSecureObjectOld = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals(oldVolumnName.Trim().ToLower())).FirstOrDefault();
                                if (oSecureObjectOld is not null)
                                {
                                    oSecureObjectOld.Name = pVolume.Name;
                                }
                                _iSecureObject.Update(oSecureObjectOld);
                                // oSecureObject.Rename(oldVolumnName, Enums.SecureObjects.Volumes, pVolume.Name)
                            }
                            _iVolume.Update(pVolumnEntity);
                            Keys.ErrorType = "s";
                            Keys.ErrorMessage = Languages.get_Translation("msgAdminVolumeUpdate");
                        }
                        else
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlValumeAlreadyInUseL1"), pVolume.Name);
                            break;
                        }
                    }
                    else if (_iVolume.All().Any(x => (x.Name.Trim().ToLower()) == (pVolume.Name.Trim().ToLower())) == false)
                    {
                        if (pVolume.PathName.Substring(0, 1) != @"\")
                        {
                            pVolume.PathName = @"\" + pVolume.PathName;
                        }

                        int lCounter;
                        lCounter = oSecureObject.GetSecureObjectID(pVolume.Name, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Volumes);
                        if (lCounter == 0L)
                            lCounter = oSecureObject.Register(pVolume.Name, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Volumes, (int)Enums.SecureObjects.Volumes);
                        var oSecureObjectPermission = new SecureObjectPermission();
                        oSecureObjectPermission.GroupID = -1;
                        oSecureObjectPermission.SecureObjectID = Convert.ToInt32(lCounter.ToString());
                        oSecureObjectPermission.PermissionID = 3;
                        if (_iSecureObjectPermission.All().Any(x => x.GroupID == oSecureObjectPermission.GroupID & x.SecureObjectID == oSecureObjectPermission.SecureObjectID & x.PermissionID == oSecureObjectPermission.PermissionID) == false)
                        {
                            _iSecureObjectPermission.Add(oSecureObjectPermission);
                        }
                        _iVolume.Add(pVolume);
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminVolumeSave");
                    }
                    else
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlValumeAlreadyInUseL1"), pVolume.Name);
                        break;
                    }
                }

                catch (Exception)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });

        }
        [HttpPost]
        public IActionResult EditVolumeDetails([FromForm] string[] pRowSelected)
        {
            if (pRowSelected is null)
            {
                return Json(new { success = false });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { success = false });
            }

            int pVolumeId = Convert.ToInt32(pRowSelected.GetValue(0));
            if (pVolumeId == 0)
            {
                return Json(new { success = false });
            }
            IRepository<Volume> _iVolume = new Repositories<Volume>();
            var pVolumeEntity = _iVolume.All().Where(x => x.Id == pVolumeId).FirstOrDefault();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(pVolumeEntity, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        public IActionResult DeleteVolumesEntity([FromForm] string[] pRowSelected)
        {
            if (pRowSelected is null)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
            }
            int iVolumnId;
            IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
            IRepository<ImagePointer> _iImagePointer = new Repositories<ImagePointer>();
            IRepository<PCFilesPointer> _iPCFilesPointer = new Repositories<PCFilesPointer>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            IRepository<TabFusionRMS.Models.Directory> _iDirectory = new Repositories<TabFusionRMS.Models.Directory>();
            try
            {
                string pVolumnId = pRowSelected.GetValue(0).ToString();
                if (string.IsNullOrWhiteSpace(pVolumnId))
                {
                    return Json(new { errortype = "e", message = Languages.get_Translation("msgAdminCtrlNullValueFound") });
                }
                IRepository<OutputSetting> _iOutputSetting = new Repositories<OutputSetting>();
                iVolumnId = Convert.ToInt32(pVolumnId);

                bool lOutputSettings = _iOutputSetting.All().Any();
                if (_iOutputSetting.All().Any(x => x.VolumesId == iVolumnId) == true)
                {
                    return Json(new { errortype = "w", message = Languages.get_Translation("msgAdminCtrlVolumeAlreadyUsedNotRemove") });
                }
                IRepository<Volume> _iVolume = new Repositories<Volume>();
                var oVolumeEntity = _iVolume.All().Where(x => x.Id == iVolumnId).FirstOrDefault();
                if (oVolumeEntity is not null)
                {
                    var oDirectory = _iDirectory.All().Where(x => x.VolumesId == iVolumnId).FirstOrDefault();

                    object oImagePointers = null;
                    object oPCFilesPointer = null;
                    if (oDirectory is not null)
                    {
                        oImagePointers = _iImagePointer.All().Where(x => x.ScanDirectoriesId == oDirectory.Id).FirstOrDefault();
                        oPCFilesPointer = _iPCFilesPointer.All().Where(x => x.ScanDirectoriesId == oDirectory.Id).FirstOrDefault();
                    }

                    if (oImagePointers is not null | oPCFilesPointer is not null)
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRowHasAttachmentAssigned");
                    }
                    else
                    {
                        _iVolume.Delete(oVolumeEntity);

                        // Added by Ganesh 06/01/2016 to fix bug #812.
                        _iSecureObject.BeginTransaction();
                        _iSecureObjectPermission.BeginTransaction();
                        var oSecureObjEntity = _iSecureObject.All().Where(m => (m.Name) == (oVolumeEntity.Name) & m.SecureObjectTypeID == (int)Enums.SecureObjectType.Volumes).FirstOrDefault();
                        int SecureObjectId = oSecureObjEntity.SecureObjectID;
                        _iSecureObject.Delete(oSecureObjEntity);
                        _iSecureObject.CommitTransaction();

                        var oSecureObjPermissions = _iSecureObjectPermission.All().Where(m => m.SecureObjectID == SecureObjectId);

                        _iSecureObjectPermission.DeleteRange(oSecureObjPermissions);
                        _iSecureObjectPermission.CommitTransaction();

                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminVolumeDelete");
                    }
                }
                else
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlNoRecForDelete");
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        #endregion
        #endregion

        #region Table
        public IActionResult LoadAccordianTable()
        {
            var pTablesList = new List<Table>();
            IRepository<vwTablesAll> _ivwTablesAll = new Repositories<vwTablesAll>();
            IRepository<Table> _iTable = new Repositories<Table>();
            var pTablesEntities = _iTable.All().Where(m => (m.TableName.Trim().ToLower()) != ("Operators".Trim().ToLower())).OrderBy(m => m.TableName);
            var lAllTables = _ivwTablesAll.All().Select(x => x.TABLE_NAME).ToList();
            pTablesEntities = (IOrderedQueryable<Table>)pTablesEntities.Where(x => lAllTables.Contains(x.TableName));

            foreach (var oTable in pTablesEntities)
            {
                if (passport.CheckPermission(oTable.TableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Configure))
                {
                    pTablesList.Add(oTable);
                }
            }

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(pTablesList.OrderBy(m => m.UserName), Newtonsoft.Json.Formatting.Indented, Setting);
            return Json(jsonObject);

        }

        public PartialViewResult LoadTableTab()
        {
            return PartialView("_TableTabPartial");
        }

        #region General

        public PartialViewResult LoadGeneralTab()
        {
            return PartialView("_TableGeneralPartial");
        }

        [HttpPost]
        //   [ValidateInput(false)]
        public IActionResult GetGeneralDetails(string tableName)
        {
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                var pTableEntity = _iTable.All();
                var pSelectTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                object DBUserName;
                if (pSelectTable.DBName is null)
                {

                    DBUserName = _iSystem.All().OrderBy(m => m.Id).FirstOrDefault().UserName;
                }
                else
                {
                    DBUserName = pSelectTable.DBName;
                }
                var sAdoConn = DataServices.DBOpen();
                Databas dbObj = null;
                var auditFlag = default(bool);
                var cursorFlag = default(bool);
                var displayFieldList = new List<KeyValuePair<string, string>>();
                object DatabaseName = null;
                DataSet loutput = null;
                var schemaColumnList = new List<SchemaColumns>();

                // Get ADO connection name
                if (pSelectTable is not null)
                {
                    if (!string.IsNullOrEmpty(pSelectTable.DBName))
                    {
                        dbObj = _iDatabas.All().Where(m => m.DBName.Trim().ToLower().Equals(pSelectTable.DBName.Trim().ToLower())).FirstOrDefault();
                        if (dbObj is null)
                        {
                            Keys.ErrorType = "e";
                            Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlSomethingWrongInExtDBConf");
                            return Json(new
                            {
                                errortype = Keys.ErrorType,
                                message = Keys.ErrorMessage
                            });
                        }
                        sAdoConn = DataServices.DBOpen(pSelectTable, _iDatabas.All());
                    }
                }

                // Load Display Field Select List
                schemaColumnList = SchemaInfoDetails.GetSchemaInfo(pSelectTable.TableName, sAdoConn);
                if (schemaColumnList is not null)
                {
                    // Dim bAddColumn As Boolean = False
                    foreach (SchemaColumns colObject in schemaColumnList)
                    {
                        bool bAddColumn = false;
                        if (!SchemaInfoDetails.IsSystemField(colObject.ColumnName))
                        {
                            if (!string.IsNullOrWhiteSpace(pSelectTable.RetentionFieldName))
                            {
                                if (Convert.ToBoolean(DatabaseMap.RemoveTableNameFromField(Convert.ToString(pSelectTable.RetentionFieldName.Trim().ToLower().Equals(colObject.ColumnName.Trim().ToLower())))))
                                {
                                    bAddColumn = true;
                                }
                                else
                                {
                                    bAddColumn = true;
                                }
                            }
                            else
                            {
                                bAddColumn = true;
                            }
                            if (bAddColumn)
                            {
                                bool bIsMemoCol = colObject.IsString & (colObject.CharacterMaxLength <= 0 | colObject.CharacterMaxLength > 8000);
                                if (!bIsMemoCol)
                                {
                                    displayFieldList.Add(new KeyValuePair<string, string>(colObject.ColumnName, colObject.ColumnName));
                                }
                            }
                        }
                    }
                }
                // Get Current URI and icon name
                string ServerPath = Common.GetAbsoluteUri(httpContext).ToString();
                ServerPath = this.Url.Content("~/Images/icons/");
                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString(dbObj);

                if (!string.IsNullOrWhiteSpace(pSelectTable.IdFieldName))
                {
                    string strqry = "SELECT [" + DatabaseMap.RemoveTableNameFromField(pSelectTable.IdFieldName.Trim().ToLower()) + "] FROM [" + pSelectTable.TableName + "] Where 0=1;";
                    loutput = _IDBManager.ExecuteDataSetWithSchema(CommandType.Text, strqry);
                    _IDBManager.Dispose();
                }
                if (loutput is not null)
                {
                    bool IdentityVal = loutput.Tables[0].Columns[0].AutoIncrement;
                    if (IdentityVal)
                    {
                        cursorFlag = true;
                    }
                    else
                    {
                        cursorFlag = false;
                    }
                }

                // Check whether selected table has any child table or not
                var relationObject = _iRelationShip.All().Where(m => m.UpperTableName.Trim().ToLower().Equals(pSelectTable.TableName.Trim().ToLower()));
                if (relationObject is not null)
                {
                    if (relationObject.Count() <= 0)
                    {
                        auditFlag = false;
                    }
                    else
                    {
                        auditFlag = true;
                    }
                }

                string UserTableIcon = Convert.ToString(_iSystem.All().FirstOrDefault().UseTableIcons);

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string cursorFlagJSON = JsonConvert.SerializeObject(cursorFlag, Newtonsoft.Json.Formatting.Indented, Setting);
                string auditflagJSON = JsonConvert.SerializeObject(auditFlag, Newtonsoft.Json.Formatting.Indented, Setting);
                string pSelectTableJSON = JsonConvert.SerializeObject(pSelectTable, Newtonsoft.Json.Formatting.Indented, Setting);
                string displayFieldListJSON = JsonConvert.SerializeObject(displayFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                string ServerPathJSON = JsonConvert.SerializeObject(ServerPath, Newtonsoft.Json.Formatting.Indented, Setting);
                string DBUserNameJSON = JsonConvert.SerializeObject(DBUserName, Newtonsoft.Json.Formatting.Indented, Setting);
                string UserTableIconJSON = JsonConvert.SerializeObject(UserTableIcon.ToLower(), Newtonsoft.Json.Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlAllDataGetSuccessfully");
                return Json(new
                {
                    cursorFlagJSON,
                    auditflagJSON,
                    pSelectTableJSON,
                    displayFieldListJSON,
                    ServerPathJSON,
                    UserTableIconJSON,
                    DBUserNameJSON,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
        }

        [HttpPost]
        //   [ValidateInput(false)]
        public IActionResult LoadIconWindow([FromServices] IWebHostEnvironment webHostEnvironment,string TableName)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            try
            {
                var IconList = new List<KeyValuePair<string, string>>();
                string FileName = Common.GetAbsoluteUri(httpContext).ToString();
                FileName = this.Url.Content("~/Images/icons");
                string LocalPath = webHostEnvironment.WebRootPath;
                LocalPath = LocalPath + @"\Images\icons";

                var FileDirectory = new System.IO.DirectoryInfo(LocalPath);
                var FileIco = FileDirectory.GetFiles("*.ICO");
                var FileJpg = FileDirectory.GetFiles("*.jpg");
                var FileGif = FileDirectory.GetFiles("*.gif");
                var FileBmp = FileDirectory.GetFiles("*.bmp");
                foreach (System.IO.FileInfo File in FileIco)
                    IconList.Add(new KeyValuePair<string, string>(FileName + "/" + File.Name, File.Name));
                foreach (System.IO.FileInfo File in FileJpg)
                    IconList.Add(new KeyValuePair<string, string>(FileName + "/" + File.Name, File.Name));
                foreach (System.IO.FileInfo File in FileGif)
                    IconList.Add(new KeyValuePair<string, string>(FileName + "/" + File.Name, File.Name));
                foreach (System.IO.FileInfo File in FileBmp)
                    IconList.Add(new KeyValuePair<string, string>(FileName + "/" + File.Name, File.Name));
                var oTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(TableName.Trim().ToLower())).FirstOrDefault();
                string PictureName = oTable.Picture;
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string IconListJSON = JsonConvert.SerializeObject(IconList, Newtonsoft.Json.Formatting.Indented, Setting);
                string PictureNameJSON = JsonConvert.SerializeObject(PictureName, Newtonsoft.Json.Formatting.Indented, Setting);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlTableLoadSuccessfully");
                return Json(new
                {
                    IconListJSON,
                    PictureNameJSON,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessage;
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });

            }
        }

        public IActionResult SetGeneralDetails(Table tableForm, bool Attachments, int miOfficialRecord)
        {
            string warnMsgJSON = "'";
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                var tableObj = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tableForm.TableName.Trim().ToLower())).FirstOrDefault();
                var tableEntity = _iTable.All().OrderBy(m => m.SearchOrder);
                int LimitVar = (int)tableForm.SearchOrder;
                var SearchOrderList = new List<Table>();
                bool flagSecure = false;
                var SecureAnnotation = _iSecureObject.All().Where(m => m.Name.Trim().ToLower().Equals(tableForm.TableName.Trim().ToLower()) & m.SecureObjectTypeID.Equals((int)Enums.SecureObjects.Annotations)).FirstOrDefault();
                var SecureAttachment = _iSecureObject.All().Where(m => m.Name.Trim().ToLower().Equals(tableForm.TableName.Trim().ToLower()) & m.SecureObjectTypeID.Equals((int)Enums.SecureObjects.Attachments)).FirstOrDefault();
                if (tableForm.Attachments == true)
                {
                    if (SecureAnnotation is null)
                    {
                        flagSecure = Convert.ToBoolean(this.RegisterSecureObject(tableForm.TableName, Enums.SecureObjects.Annotations));
                        // Added by Ganesh for Security Fix
                        this.RegisterSecureObject(tableForm.TableName, Enums.SecureObjects.Attachments);
                    }
                }
                else if (SecureAnnotation is not null)
                {
                    flagSecure = this.UnRegisterSecureObject(SecureAnnotation);
                    // Added by Ganesh for Security Fix
                    this.UnRegisterSecureObject(SecureAttachment);
                }

                UpdateOfficialRecord(miOfficialRecord, tableObj.TableName);
                tableObj.Picture = tableForm.Picture;
                tableObj.BarCodePrefix = tableForm.BarCodePrefix;
                tableObj.IdStripChars = tableForm.IdStripChars;
                tableObj.IdMask = tableForm.IdMask;
                tableObj.DescFieldPrefixOne = tableForm.DescFieldPrefixOne;
                tableObj.DescFieldPrefixTwo = tableForm.DescFieldPrefixTwo;
                tableObj.DescFieldNameOne = tableForm.DescFieldNameOne;
                tableObj.DescFieldNameTwo = tableForm.DescFieldNameTwo;
                if (tableForm.DescFieldPrefixOneWidth is null)
                {
                    tableObj.DescFieldPrefixOneWidth = (short?)0;
                }
                else
                {
                    tableObj.DescFieldPrefixOneWidth = tableForm.DescFieldPrefixOneWidth;
                }
                if (tableForm.DescFieldPrefixTwoWidth is null)
                {
                    tableObj.DescFieldPrefixTwoWidth = (short?)0;
                }
                else
                {
                    tableObj.DescFieldPrefixTwoWidth = tableForm.DescFieldPrefixTwoWidth;
                }
                tableObj.Attachments = Attachments;
                tableObj.OfficialRecordHandling = tableForm.OfficialRecordHandling;
                tableObj.CanAttachToNewRow = tableForm.CanAttachToNewRow;
                tableObj.AuditAttachments = tableForm.AuditAttachments;
                tableObj.AuditConfidentialData = tableForm.AuditConfidentialData;
                tableObj.AuditUpdate = tableForm.AuditUpdate;
                if (tableForm.MaxRecsOnDropDown is null)
                {
                    tableObj.MaxRecsOnDropDown = 0;
                }
                else
                {
                    tableObj.MaxRecsOnDropDown = tableForm.MaxRecsOnDropDown;
                }
                if (tableForm.ADOQueryTimeout is null)
                {
                    tableObj.ADOQueryTimeout = 0;
                }
                else
                {
                    tableObj.ADOQueryTimeout = tableForm.ADOQueryTimeout;
                }
                if (tableForm.ADOCacheSize is null)
                {
                    tableObj.ADOCacheSize = 0;
                }
                else
                {
                    tableObj.ADOCacheSize = tableForm.ADOCacheSize;
                }
                tableObj.ADOServerCursor = tableForm.ADOServerCursor;


                if (LimitVar != 0 && tableObj.SearchOrder != tableForm.SearchOrder)
                {

                    foreach (Table tb in tableEntity.Where(m => m.SearchOrder <= LimitVar))
                    {
                        if (tb.SearchOrder is not null)
                        {
                            if ((tb.SearchOrder <= LimitVar) && (!tableObj.TableName.Trim().ToLower().Equals(tb.TableName.Trim().ToLower())))
                            {
                                SearchOrderList.Add(tb);
                            }
                        }
                        else
                        {
                            SearchOrderList.Add(tb);
                        }
                    }

                    if (tableObj.SearchOrder < LimitVar)
                    {
                        SearchOrderList.Add(tableObj);
                    }
                    else
                    {
                        var LastObject = SearchOrderList.Last();
                        SearchOrderList.RemoveAt(SearchOrderList.Count - 1);
                        SearchOrderList.Add(tableObj);
                        SearchOrderList.Add(LastObject);
                    }
                    foreach (Table tb in tableEntity.Where(m => m.SearchOrder > LimitVar))
                    {
                        if (tb.SearchOrder is not null)
                        {
                            if ((tb.SearchOrder > LimitVar) && (!tableObj.TableName.Trim().ToLower().Equals(tb.TableName.Trim().ToLower())))
                            {
                                SearchOrderList.Add(tb);
                            }
                        }
                    }

                    int iLevel = 1;

                    foreach (Table tb in SearchOrderList)
                    {
                        tb.SearchOrder = iLevel;
                        iLevel = iLevel + 1;
                    }

                    foreach (Table tb in SearchOrderList)
                        _iTable.Update(tb);
                }

                else
                {
                    _iTable.Update(tableObj);
                }
                warnMsgJSON = VerifyRetentionDispositionTypesForParentAndChildren(tableObj.TableId);
                var searchValue = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tableForm.TableName.Trim().ToLower())).FirstOrDefault().SearchOrder;
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string searchValueJSON = JsonConvert.SerializeObject(searchValue, Newtonsoft.Json.Formatting.Indented, Setting);

                // Reload the permissions dataset after updation of permissions.
                passport.FillSecurePermissions();

                Keys.ErrorType = "s";
                // Modified By Hemin For bug fix on 11-07-2016
                // Keys.ErrorMessage = Languages.Translation("RecordUpdatedSuccessfully")
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRecordUpdatedSuccessfully");

                // Reload the permissions dataset after updation of permissions. [Fix for FUS-5398]
                // CollectionsClass.ReloadPermissionDataSet()

                return Json(new
                {
                    searchValueJSON,
                    warnMsgJSON,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
        }

        public IActionResult SetSearchOrder()
        {
            try
            {
                var sADOConnDefault = DataServices.DBOpen();
                var searchOrderList = new List<KeyValuePair<int, string>>();
                string argsSql = "SELECT DISTINCT t.TableName, t.UserName, t.SearchOrder, s.IndexTableName FROM [Tables] t LEFT OUTER JOIN SLTextSearchItems s ON s.IndexTableName = t.TableName ORDER BY t.SearchOrder";
                string arglError = "";
                ADODB.Recordset searchRecords = DataServices.GetADORecordSet(ref argsSql, sADOConnDefault, lError: ref arglError);
                if (searchRecords is not null)
                {
                    while (!searchRecords.EOF)
                    {
                        string sSql = "[] ";
                        var textValue = searchRecords.Fields[3];

                        if (searchRecords.Fields[3].Value.ToString().Trim().Equals(""))
                        {
                            sSql = "[not part of Full Text Index]";
                        }
                        else
                        {
                            sSql = " ";
                        }
                        var tableStr = "(" + searchRecords.Fields[2].Value.ToString() + ")" + "    " + searchRecords.Fields[1].Value.ToString() + "   " + sSql;
                        if (searchRecords.Fields[2] is DBNull)
                        {
                            searchOrderList.Add(new KeyValuePair<int, string>(0, Convert.ToString(tableStr)));
                        }
                        else
                        {
                            searchOrderList.Add(new KeyValuePair<int, string>(Convert.ToInt32(searchRecords.Fields[2].Value), Convert.ToString(tableStr)));
                        }
                        searchRecords.MoveNext();
                    }
                }
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string searchOrderListJSON = JsonConvert.SerializeObject(searchOrderList, Newtonsoft.Json.Formatting.Indented, Setting);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlAllDataGetSuccessfully");
                return Json(new
                {
                    searchOrderListJSON,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
        }

        public IActionResult OfficialRecordWarning(bool recordStatus, string tableName)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            do
            {
                try
                {
                    var tableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                    var sADOConnDefault = DataServices.DBOpen();
                    string argsSql = "SELECT TOP 1 * FROM [UserLinks] WHERE [IndexTable] ='" + tableName + "'";
                    string arglError = "";
                    var rs = DataServices.GetADORecordSet(ref argsSql, sADOConnDefault, lError: ref arglError);
                    if (recordStatus == true)
                    {
                        if (tableEntity.OfficialRecordHandling == false)
                        {
                            if (rs.RecordCount > 0)
                            {
                                Keys.ErrorType = "w";
                                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlWouldULikeToSetOffcialRecL2"), tableName, @"\n\n");
                                break;
                            }
                        }
                    }
                    else if (tableEntity.OfficialRecordHandling == true)
                    {
                        if (rs.RecordCount > 0)
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlWouldULikeToRemoveOffcialRec"), tableName, @"\n\n");
                            break;
                        }
                    }
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlYouHaveNoRecord");
                }
                catch (Exception)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public bool UpdateOfficialRecord(int miOfficialRecord, string tableName)
        {
            try
            {
                var sADOConnDefault = DataServices.DBOpen();
                var miOfficialRecordConversion = Enums.geOfficialRecordConversonType.orcNoConversion;
                switch (miOfficialRecord)
                {
                    case 0:
                        {
                            miOfficialRecordConversion = Enums.geOfficialRecordConversonType.orcNoConversion;
                            break;
                        }
                    case 1:
                        {
                            miOfficialRecordConversion = Enums.geOfficialRecordConversonType.orcFirstVersionConversion;
                            break;
                        }
                    case 2:
                        {
                            miOfficialRecordConversion = Enums.geOfficialRecordConversonType.orcLastVersionConversion;
                            break;
                        }
                    case 4:
                        {
                            miOfficialRecordConversion = Enums.geOfficialRecordConversonType.orcConversionToNothing;
                            break;
                        }

                    default:
                        {
                            miOfficialRecordConversion = Enums.geOfficialRecordConversonType.orcNoConversion;
                            break;
                        }
                }
                if (miOfficialRecordConversion != Enums.geOfficialRecordConversonType.orcNoConversion)
                {
                    string sSQL = null;
                    if (miOfficialRecordConversion == Enums.geOfficialRecordConversonType.orcFirstVersionConversion)
                    {
                        sSQL = "UPDATE [Trackables] SET [OfficialRecord] = 1 FROM [Trackables] INNER JOIN [UserLinks] ON ([UserLinks].[TrackablesId] = [Trackables].[Id]) WHERE [UserLinks].[IndexTable] ='" + tableName + "' AND [Trackables].[RecordVersion] = 1";
                        DataServices.ProcessADOCommand(ref sSQL, sADOConnDefault, false);
                    }
                    else if (miOfficialRecordConversion == Enums.geOfficialRecordConversonType.orcLastVersionConversion)
                    {
                        sSQL = " UPDATE [Trackables] SET [OfficialRecord] = 1 FROM [Trackables] a INNER JOIN (SELECT [id], MAX([RecordVersion]) AS MaxVersion FROM [Trackables] GROUP BY [Id]) b ON (a.Id = b.Id AND a.RecordVersion = b.MaxVersion) INNER JOIN [Userlinks] ON ([Userlinks].[TrackablesId] = [a].[Id]) WHERE [Userlinks].[IndexTable] ='" + tableName + "'";
                        DataServices.ProcessADOCommand(ref sSQL, sADOConnDefault, false);
                    }
                    else if (miOfficialRecordConversion == Enums.geOfficialRecordConversonType.orcConversionToNothing)
                    {
                        sSQL = " UPDATE [Trackables] SET [OfficialRecord] = 0 FROM [Trackables] a INNER JOIN (SELECT [id], MAX([RecordVersion]) AS MaxVersion FROM [Trackables] GROUP BY [Id]) b ON (a.Id = b.Id AND a.RecordVersion = b.MaxVersion) INNER JOIN [Userlinks] ON ([Userlinks].[TrackablesId] = [a].[Id]) WHERE [Userlinks].[IndexTable] ='" + tableName + "'";
                        DataServices.ProcessADOCommand(ref sSQL, sADOConnDefault, false);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int RegisterSecureObject(string tableName, Enums.SecureObjects secureObjTypeId)
        {
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            try
            {
                var secureObjEntity = new TabFusionRMS.Models.SecureObject();
                int returnSecureObjId = 0;
                if (tableName is not null)
                {
                    var baseId = _iSecureObject.All().Where(m => m.SecureObjectTypeID.Equals((int)Enums.SecureObjects.Table) & m.Name.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                    if (baseId is not null)
                    {
                        secureObjEntity.Name = tableName;
                        secureObjEntity.SecureObjectTypeID = (int)secureObjTypeId;
                        secureObjEntity.BaseID = baseId.SecureObjectID;
                        _iSecureObject.Add(secureObjEntity);
                        returnSecureObjId = _iSecureObject.All().Where(m => m.SecureObjectTypeID.Equals((int)secureObjTypeId) & m.Name.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault().SecureObjectID;
                        // Need to take care for first insertion of permissions by default. - Added by Ganesh
                        AddSecureObjectPermissionsBySecureObjectType(returnSecureObjId, (int)secureObjTypeId, (int)secureObjTypeId);
                    }
                }
                return returnSecureObjId;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public bool UnRegisterSecureObject(TabFusionRMS.Models.SecureObject secureObjId)
        {
            try
            {
                IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
                IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
                if (secureObjId is not null)
                {
                    _iSecureObject.Delete(secureObjId);
                    // Remove permissions against registered secure object. - Added by Ganesh.
                    var secureObjPermissions = _iSecureObjectPermission.All().Where(x => x.SecureObjectID == secureObjId.SecureObjectID);
                    _iSecureObjectPermission.DeleteRange(secureObjPermissions);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // 'Add new permission in secure permission table
        private void AddNewSecureObjectPermission(int secureObjectId, int securePermissionId)
        {
            IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
            var secoreObjPermissionObj = new SecureObjectPermission();
            secoreObjPermissionObj.GroupID = 0;
            secoreObjPermissionObj.SecureObjectID = secureObjectId;
            secoreObjPermissionObj.PermissionID = securePermissionId;
            _iSecureObjectPermission.Add(secoreObjPermissionObj);
            if (securePermissionId == 8 | securePermissionId == 9)
            {
                UpdateTablesTrackingObject("A", secureObjectId, securePermissionId);
            }
        }

        // 'Keep sync Tables -> Tracking tab (Tracking Object and Allow Requiresting checkboxs) and Security Securables tab
        // 'Removed permission updates in Tables table
        private void UpdateTablesTrackingObject(string action, int secureObjectId, int securePermissionId)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            var SecureObject = _iSecureObject.All().Where(m => m.SecureObjectID == secureObjectId).FirstOrDefault();
            if (SecureObject is not null)
            {
                var Tables = _iTable.Where(m => m.TableName.Trim().ToLower().Equals(SecureObject.Name.Trim().ToLower())).FirstOrDefault();
                if (Tables is not null)
                {
                    if (securePermissionId == 8)
                    {
                        Tables.Trackable = (bool?)Interaction.IIf(action == "A", true, false);
                    }
                    if (securePermissionId == 9)
                    {
                        Tables.AllowBatchRequesting = (bool?)Interaction.IIf(action == "A", true, false);
                    }
                    _iTable.Update(Tables);
                }
            }
        }

        // 'Add Tracking object permission into security tables 
        private bool AddSecureObjectPermission(int secureObjId, Enums.PassportPermissions SecurePermissionId)
        {
            try
            {
                IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
                if (!_iSecureObjectPermission.All().Any(x => (x.GroupID == 0 && x.SecureObjectID == secureObjId) & x.PermissionID == (int)SecurePermissionId))
                {
                    AddNewSecureObjectPermission(secureObjId, (int)SecurePermissionId);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RemoveSecureObjectPermission(SecureObjectPermission secureObjPermission)
        {
            try
            {
                if (secureObjPermission is not null)
                {
                    IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
                    _iSecureObjectPermission.Delete(secureObjPermission);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public SecureObjectPermission GetSecureObjPermissionId(int secureObjId, Enums.PassportPermissions SecurePermissionId)
        {
            try
            {
                IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
                var secoreObjPermissionObj = _iSecureObjectPermission.All().Where(m => m.SecureObjectID == secureObjId & m.PermissionID == (int)SecurePermissionId).FirstOrDefault();
                return secoreObjPermissionObj;
            }
            catch (Exception)
            {
                throw;
            } // ex
        }
        #endregion

        #region Fields
        public PartialViewResult LoadFieldsTab()
        {
            return PartialView("_TableFields");
        }

        // Create and Edit new field for Table.
        [HttpPost]
        public IActionResult AddEditField([FromForm] string pOperationName, [FromForm] string pTableName, [FromForm] string pNewInternalName, [FromForm] string pOriginalInternalName,
            [FromForm] string pFieldType, [FromForm] string pOriginalFieldType, [FromForm] string pFieldSize, [FromForm] string pOriginalFieldSize)
        {
            string sSQLStr;
            // Dim sTempSQL As String
            int iFieldMaxSize;
            string ErrMsg = "";
            bool isError = false;
            bool bFieldCreated;
            bool bFieldUpdate;
            ADODB.Connection sADOConn;
            string FieldName;
            int iFieldSize;
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<Table> _iTable = new Repositories<Table>();

            try
            {
                FieldName = pNewInternalName.Trim();

                if (Strings.StrComp(pFieldSize, Common.FT_MEMO_SIZE, Constants.vbTextCompare) == 0)
                {
                    iFieldSize = 0;
                }
                else
                {
                    iFieldSize = Convert.ToInt32("0" + pFieldSize);
                }

                if (string.IsNullOrEmpty(FieldName))
                {
                    ErrMsg = Languages.get_Translation("msgJsTableFieldIntNameReq");
                    isError = true;
                }

                if (Strings.InStr("_0123456789%", Strings.Left(FieldName, 1)) > 0 & isError == false)
                {
                    ErrMsg = Languages.get_Translation("msgAdminCtrlInternalNotBeginWith");
                    isError = true;
                }

                if ((Strings.StrComp(FieldName, "SLFileRoomOrder", Constants.vbBinaryCompare) == 0 | Strings.StrComp(FieldName, "SLTrackedDestination", Constants.vbTextCompare) == 0) & isError == false)
                {
                    ErrMsg = Languages.get_Translation("msgAdminCtrlInternalNotBe");
                    isError = true;
                }

                if (Convert.ToDouble(pFieldType) == (double)Enums.meTableFieldTypes.ftText)
                {
                    iFieldMaxSize = 8000;

                    if ((iFieldSize < 0 | iFieldSize > iFieldMaxSize) & isError == false)
                    {
                        ErrMsg = Languages.get_Translation("msgAdminCtrlFieldSizeBtw1ToMaxSize");
                        isError = true;
                    }
                }

                var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableName.Trim().ToLower())).FirstOrDefault();
                sADOConn = DataServices.DBOpen(oTables, _iDatabas.All());

                var dbRecordSet = SchemaInfoDetails.GetTableSchemaInfo(pTableName, sADOConn);

                // Added on 21/12/2015 to fix Bug #703.
                if ((pNewInternalName ?? "".ToLower()) != (pOriginalInternalName ?? "".ToLower()) & string.IsNullOrEmpty(ErrMsg) & pOperationName == "EDIT")
                {
                    if (dbRecordSet.Exists(x => x.ColumnName.ToLower().Equals(pNewInternalName.ToLower())))
                    {
                        ErrMsg = Languages.get_Translation("msgAdminCtrlInternalNameAlreadyExists");
                        isError = true;
                    }
                }
                if (dbRecordSet.Exists(x => x.ColumnName.ToLower().Equals(pNewInternalName.ToLower())) & string.IsNullOrEmpty(ErrMsg) & pOperationName == "ADD")
                {
                    ErrMsg = Languages.get_Translation("msgAdminCtrlInternalNameAlreadyExists");
                    isError = true;
                }

                if (isError == false & pOperationName == "ADD")
                {
                    // create the new field
                    sSQLStr = "ALTER TABLE [" + pTableName + "]";

                    sSQLStr = sSQLStr + " ADD [" + pNewInternalName + "] ";

                    switch (pFieldType)
                    {
                        case "0":
                        case "8":
                            {
                                sSQLStr = sSQLStr + "INT NULL";
                                break;
                            }

                        case "1":
                            {
                                sSQLStr = sSQLStr + "INT IDENTITY(1,1) NOT NULL";
                                break;
                            }

                        case "2":
                            {
                                sSQLStr = sSQLStr + "VARCHAR(" + iFieldSize + ") NULL";
                                break;
                            }

                        case "3":
                            {
                                sSQLStr = sSQLStr + "SMALLINT NULL";
                                break;
                            }

                        case "4":
                            {
                                sSQLStr = sSQLStr + "BIT NULL";
                                // If (Not bTemporary) Then sSQLStr = sSQLStr & " DEFAULT 0"
                                sSQLStr = sSQLStr + " CONSTRAINT DF_" + pTableName + "_" + pNewInternalName + " DEFAULT (0) WITH VALUES";
                                break;
                            }

                        case "5":
                            {
                                sSQLStr = sSQLStr + "FLOAT NULL";
                                break;
                            }

                        case "6":
                            {
                                sSQLStr = sSQLStr + "DATETIME NULL";
                                break;
                            }

                        case "7":
                            {
                                sSQLStr = sSQLStr + "TEXT NULL";
                                break;
                            }

                        default:
                            {
                                break;
                            }
                    }

                    bFieldCreated = DataServices.ProcessADOCommand(ref sSQLStr, sADOConn, false);

                    if (bFieldCreated)
                    {
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlFieldCreatedSuccessfully");
                        DeleteSQLViewWithNoViewColumnExists(pTableName);
                    }
                    else
                    {
                        Keys.ErrorType = "e";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlIssuesWithCreatingNewField");
                    }
                }
                else if (isError == false & pOperationName == "EDIT")
                {

                    // If (dbRecordSet.Exists(Function(x) x.ColumnName = pNewInternalName)) And ErrMsg = "" Then
                    // ErrMsg = "Internal Name Already Exists."
                    // isError = True
                    // Else
                    bFieldUpdate = UpdateNewField(pNewInternalName, pOriginalInternalName, pTableName, (Enums.meTableFieldTypes)Convert.ToInt32(pOriginalFieldType), iFieldSize, Convert.ToInt32(pFieldType));
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlFieldUpdatedSuccessfully");
                }
                // End If
                else
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = ErrMsg;
                }
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public JsonResult CheckBeforeUpdate(string pFieldName, int pNewFieldSize, int pNewFieldType, int pOrigFieldSize, int pOrigFieldType)
        {
            string sMessage = "";

            if (pNewFieldType != (int)Enums.meTableFieldTypes.ftMemo & (pOrigFieldSize > pNewFieldSize | pOrigFieldType != pNewFieldType))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlChangeTheTypeL1"), pFieldName, Constants.vbNewLine);
            }

            return Json(new { sMessage });
        }

        private bool UpdateNewField(string sNewFieldName, string sOldFieldName, string sTableName, Enums.meTableFieldTypes eFieldType, int iFieldSize, int iNewFieldType)
        {
            string sSQLStr;
            // Dim sModifyColSQLStr As String
            // Dim lStatus As Long
            string sFieldType = "";
            string eStrFieldType = "";
            string FieldType = "";
            ADODB.Connection sADOConn;
            bool bFieldUpdate = false;
            int lError = 0;
            string sErrorMsg = "";
            string sSQLAddToTEMP = "";
            string sSQLCopyToTEMP = "";
            string sSQLDropOriginal = "";
            string sSQLCreateNew = "";
            string sSQLAddToNew = "";
            string sSQLDropTEMP = "";
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<Table> _iTable = new Repositories<Table>();

            // Dim sADOConn = DataServices.DBOpen(Enums.eConnectionType.conDefault, Nothing)
            var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())).FirstOrDefault();
            sADOConn = DataServices.DBOpen(oTables, _iDatabas.All());

            switch (iNewFieldType)
            {
                case (int)Enums.meTableFieldTypes.ftLong:
                case (int)Enums.meTableFieldTypes.ftSmeadCounter:
                    {
                        sFieldType = "INT";
                        break;
                    }
                case (int)Enums.meTableFieldTypes.ftCounter:
                    {
                        sFieldType = "INT";
                        break;
                    }
                case (int)Enums.meTableFieldTypes.ftText:
                    {
                        sFieldType = "VARCHAR(" + iFieldSize + ")";
                        break;
                    }
                case (int)Enums.meTableFieldTypes.ftInteger:
                    {
                        sFieldType = "SMALLINT";
                        break;
                    }
                case (int)Enums.meTableFieldTypes.ftBoolean:
                    {
                        sFieldType = "BIT";
                        break;
                    }
                case (int)Enums.meTableFieldTypes.ftDouble:
                    {
                        sFieldType = "FLOAT";
                        break;
                    }
                case (int)Enums.meTableFieldTypes.ftDate:
                    {
                        sFieldType = "DATETIME";
                        break;
                    }
                case (int)Enums.meTableFieldTypes.ftMemo:
                    {
                        sFieldType = "TEXT";
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            if ((sOldFieldName) != (sNewFieldName))
            {
                sSQLStr = "EXEC SP_RENAME '" + sTableName + "." + sOldFieldName + "','" + sNewFieldName + "'," + "'COLUMN'";
                bFieldUpdate = DataServices.ProcessADOCommand(ref sSQLStr, sADOConn, false);

                if (bFieldUpdate)
                {
                    sOldFieldName = sNewFieldName;
                }
            }

            // ALTER TABLE TableName ADD tmp text NULL
            // GO()
            // UPDATE TableName SET tmp = ColumnName
            // GO()
            // ALTER TABLE TableName DROP COLUMN ColumnName
            // GO()
            // ALTER TABLE TableName ADD ColumnName ntext NULL
            // GO()
            // UPDATE TableName SET ColumnName = tmp
            // GO()
            // ALTER TABLE TableName DROP COLUMN tmp
            sSQLAddToTEMP = "ALTER TABLE [" + sTableName + "] " + "ADD TEMP___ ";

            switch (eFieldType)
            {
                case Enums.meTableFieldTypes.ftLong:
                case Enums.meTableFieldTypes.ftSmeadCounter:
                    {
                        sSQLAddToTEMP = sSQLAddToTEMP + "INT NULL";
                        break;
                    }
                case Enums.meTableFieldTypes.ftCounter:
                    {
                        sSQLAddToTEMP = sSQLAddToTEMP + "INT IDENTITY(1,1) NOT NULL";
                        break;
                    }
                case Enums.meTableFieldTypes.ftText:
                    {
                        sSQLAddToTEMP = sSQLAddToTEMP + "VARCHAR(" + iFieldSize + ") NULL";
                        break;
                    }
                case Enums.meTableFieldTypes.ftInteger:
                    {
                        sSQLAddToTEMP = sSQLAddToTEMP + "SMALLINT NULL";
                        break;
                    }
                case Enums.meTableFieldTypes.ftBoolean:
                    {
                        sSQLAddToTEMP = sSQLAddToTEMP + "BIT NULL";
                        break;
                    }
                case Enums.meTableFieldTypes.ftDouble:
                    {
                        sSQLAddToTEMP = sSQLAddToTEMP + "FLOAT NULL";
                        break;
                    }
                case Enums.meTableFieldTypes.ftDate:
                    {
                        sSQLAddToTEMP = sSQLAddToTEMP + "DATETIME NULL";
                        break;
                    }
                case Enums.meTableFieldTypes.ftMemo:
                    {
                        sSQLAddToTEMP = sSQLAddToTEMP + "TEXT NULL";
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            DataServices.ProcessADOCommand(ref sSQLAddToTEMP, sADOConn, true, ref lError, ref sErrorMsg);

            sSQLCopyToTEMP = "UPDATE [" + sTableName + "] " + "SET TEMP___ = [" + sOldFieldName + "]";
            DataServices.ProcessADOCommand(ref sSQLCopyToTEMP, sADOConn, true, ref lError, ref sErrorMsg);

            //sSQLDropOriginal = "ALTER TABLE [" + sTableName + "] " + "DROP COLUMN [" + sOldFieldName + "] ";
            sSQLDropOriginal = "ALTER TABLE [" + sTableName + "] " + "DROP COLUMN [" + sOldFieldName + "] ";

            DataServices.ProcessADOCommand(ref sSQLDropOriginal, sADOConn, true, ref lError, ref sErrorMsg);

            sSQLCreateNew = "ALTER TABLE [" + sTableName + "] " + "ADD [" + sOldFieldName + "] " + sFieldType;
            DataServices.ProcessADOCommand(ref sSQLCreateNew, sADOConn, true, ref lError, ref sErrorMsg);

            sSQLAddToNew = "UPDATE [" + sTableName + "] " + "SET [" + sOldFieldName + "] =" + "[TEMP___]";
            int bUpdate = DataServices.ProcessADOCommand(ref sSQLAddToNew, sADOConn, true, ref lError, ref sErrorMsg);

            sSQLDropTEMP = "ALTER TABLE [" + sTableName + "] " + "DROP COLUMN [TEMP___]";
            DataServices.ProcessADOCommand(ref sSQLDropTEMP, sADOConn, true, ref lError, ref sErrorMsg);

            // sModifyColSQLStr = "ALTER TABLE [" & sTableName & "] "
            // sModifyColSQLStr = sModifyColSQLStr & "ALTER COLUMN [" & sOldFieldName & "] " & sFieldType

            // Dim bUpdate = DataServices.ProcessADOCommand(sModifyColSQLStr, sADOConn, True, lError, sErrorMsg)

            // sSQLStr = sSQLStr & "[" & sNewFieldName & "] = CONVERT(" & sFieldType & ", [" & sOldFieldName & "])"

            // Select Case eFieldType
            // Case Enums.meTableFieldTypes.ftLong, Enums.meTableFieldTypes.ftCounter, Enums.meTableFieldTypes.ftInteger, Enums.meTableFieldTypes.ftBoolean, Enums.meTableFieldTypes.ftDouble
            // sSQLStr = sSQLStr & " WHERE ISNUMERIC([" & sOldFieldName & "]) = 1"
            // Case Enums.meTableFieldTypes.ftDate
            // sSQLStr = sSQLStr & " WHERE ISDATE([" & sOldFieldName & "]) = 1"
            // End Select
            // ALTER TABLE [Documents] ALTER COLUMN [DocumentDescription] VARCHAR(300)

            return Convert.ToBoolean(bUpdate);
        }

        [HttpPost]
        public IActionResult CheckFieldBeforeEdit([FromForm] string pTableName, [FromForm] string sFieldName)
        {
            string sIndexMessage = "";
            string sMessage = "";

            sMessage = CheckIfInUse(pTableName, sFieldName);

            sIndexMessage = CheckIfIndexesExist(pTableName, sFieldName, false);

            return Json(new
            {
                Message = sMessage,
                IndexMsg = sIndexMessage
            });
        }

        [HttpPost]
        public IActionResult RemoveFieldFromTable([FromForm] string pTableName, [FromForm] string pFieldName, [FromForm] bool pDeleteIndexes)
        {
            List<SchemaIndex> oSchemaList;
            string sSQL;
            bool bSuccess;
            ADODB.Connection sAdoConn;
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<SLTableFileRoomOrder> _iSLTableFileRoomOrder = new Repositories<SLTableFileRoomOrder>();
            var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableName.Trim().ToLower())).FirstOrDefault();
            sAdoConn = DataServices.DBOpen(oTables, _iDatabas.All());
            try
            {
                if (pDeleteIndexes)
                {
                    oSchemaList = SchemaIndex.GetTableIndexes(pTableName, sAdoConn);

                    foreach (var oSchema in oSchemaList)
                    {
                        if (this.FieldsMatch(pTableName, pFieldName, oSchema.ColumnName))
                        {
                            sSQL = "DROP INDEX [" + pTableName + "].[" + Strings.Trim(Strings.UCase(oSchema.IndexName)) + "]";
                            DataServices.ProcessADOCommand(ref sSQL, sAdoConn, false);
                            // theApp.Data.ProcessADOCommand(sSQL, msTableName, True, Nothing, lError, sErrorMsg)
                        }
                    }

                }

                sSQL = "ALTER TABLE [" + pTableName + "] DROP COLUMN [" + pFieldName + "] ";
                bSuccess = DataServices.ProcessADOCommand(ref sSQL, sAdoConn, false);

                var pSLTableFileRoomOrderEntity = _iSLTableFileRoomOrder.All().Where(x => (x.TableName) == (pTableName) & (x.FieldName) == (pFieldName));
                _iSLTableFileRoomOrder.DeleteRange(pSLTableFileRoomOrderEntity);

                if (bSuccess == true)
                {
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlFieldRemoveSuccessfully");
                    DeleteSQLViewWithNoViewColumnExists(pTableName);
                }
                else
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlSorryCantRemoveField");
                }
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        public IActionResult CheckBeforeRemoveFieldFromTable([FromForm] string pTableName, [FromForm] string pFieldName)
        {

            var bDeleteIndexes = default(bool);
            string bOKToDeleteMsg = "";
            string sMessage = "";
            Table pTableEntity;
            List<SchemaIndex> oSchemaList;
            string FieldName;
            ADODB.Connection sAdoConn;
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();


            try
            {
                FieldName = pFieldName.Trim();
                sMessage = CheckIfInUse(pTableName, FieldName);

                pTableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(pTableName)).FirstOrDefault();

                // If (sMessage <> "") Then
                // sMessage = "The """ & FieldName & """ field is in use in the following places and cannot be removed from the """ & Trim$(pTableEntity.UserName) & """ table:</br>" & sMessage
                // End If

                if (string.IsNullOrEmpty(sMessage))
                {
                    sMessage = CheckIfIndexesExist(pTableName, FieldName, true);
                }

                // Dim sAdoConn = DataServices.DBOpen(Enums.eConnectionType.conDefault, Nothing)
                var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableName.Trim().ToLower())).FirstOrDefault();
                sAdoConn = DataServices.DBOpen(oTables, _iDatabas.All());

                oSchemaList = SchemaIndex.GetTableIndexes(pTableName, sAdoConn);

                if (!string.IsNullOrEmpty(sMessage))
                {
                    sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldIsInUseForThe"), FieldName, Strings.Trim(pTableEntity.UserName), sMessage);
                    bDeleteIndexes = true;
                }
                else
                {
                    sMessage = string.Format(Languages.get_Translation("msgAdminCtrlRUSureUWant2Remove"), pFieldName, Strings.Trim(pTableEntity.UserName));
                }
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessageJS();
            }

            if (!string.IsNullOrEmpty(sMessage))
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = sMessage;
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                bDeleteIndexes,
                message = Keys.ErrorMessage
            });
        }

        public JsonResult LoadFieldData(string pTableName, string sidx, string sord, int page, int rows)
        {
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            bool bAddColumn;
            int lIndex;
            var fieldsDT = new DataTable();
            DataColumn column;
            DataRow row;
            Table pTableEntity;
            ADODB.Connection sADOConn;
            // Dim tableName As String = "Folders"
            ADODB.Recordset rsADO;
            // Dim sFieldName As String
            string sFieldSize = "";
            string sFieldType = "";
            var lDatabase = _iDatabas.All();

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "Field_Name";
            fieldsDT.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "Field_Type";
            fieldsDT.Columns.Add(column);

            column = new DataColumn();
            column.DataType = Type.GetType("System.String");
            column.ColumnName = "Field_Size";
            fieldsDT.Columns.Add(column);
            IRepository<Table> _iTable = new Repositories<Table>();
            var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableName.Trim().ToLower())).FirstOrDefault();
            sADOConn = DataServices.DBOpen(oTables, _iDatabas.All());



            if (!string.IsNullOrEmpty(pTableName))
            {

                pTableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(pTableName)).FirstOrDefault();

                string argsSql = "SELECT * FROM [" + pTableName + "] WHERE 0 = 1";
                string arglError = "";
                rsADO = DataServices.GetADORecordSet(ref argsSql, sADOConn, lError: ref arglError);

                // Dim oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pTableName, sADOConn)
                object pDatabaseEntity = null;
                if (pTableEntity is not null)
                {
                    if (pTableEntity.DBName is not null)
                    {
                        pDatabaseEntity = lDatabase.Where(x => x.DBName.Trim().ToLower().Equals(pTableEntity.DBName.Trim().ToLower())).FirstOrDefault();
                    }
                }

                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString((Databas)pDatabaseEntity);
                var dt = _IDBManager.ExecuteDataSetWithSchema(CommandType.Text, "SELECT * FROM [" + pTableName + "] WHERE 0 = 1");
                _IDBManager.Dispose();
                // Dim columnName = dt.Tables(0).Columns(0)..ColumnName
                // Dim columnCount = dt.Tables(0).Columns.Count
                // Dim identityVal = dt.Tables(0).Columns(0).AutoIncrement

                var loopTo = rsADO.Fields.Count - 1;
                for (lIndex = 0; lIndex <= loopTo; lIndex++)
                {
                    string test = dt.Tables[0].Columns[lIndex].ColumnName;
                    string NameTest = rsADO.Fields[lIndex].Name;
                    var testDataType = dt.Tables[0].Columns[lIndex].DataType;
                    bool testBoolean = dt.Tables[0].Columns[lIndex].AutoIncrement;

                    bAddColumn = !SchemaInfoDetails.IsSystemField(rsADO.Fields[lIndex].Name);

                    if (bAddColumn)
                    {
                        if (SchemaInfoDetails.IsADateType((Enums.DataTypeEnum)rsADO.Fields[lIndex].Type))
                        {
                            sFieldType = Common.FT_DATE;
                            sFieldSize = Common.FT_DATE_SIZE;
                        }
                        else if (SchemaInfoDetails.IsAStringType((Enums.DataTypeEnum)rsADO.Fields[lIndex].Type))
                        {
                            if (rsADO.Fields[lIndex].DefinedSize <= 0 | rsADO.Fields[lIndex].DefinedSize >= 2000000)
                            {
                                sFieldType = Common.FT_MEMO;
                                sFieldSize = Common.FT_MEMO_SIZE;
                            }
                            else if (!string.IsNullOrEmpty(pTableEntity.CounterFieldName) & Strings.StrComp(DatabaseMap.RemoveTableNameFromField(rsADO.Fields[lIndex].Name), DatabaseMap.RemoveTableNameFromField(pTableEntity.IdFieldName), Constants.vbTextCompare) == 0)
                            {
                                sFieldType = Common.FT_SMEAD_COUNTER;

                                if ((long)rsADO.Fields[lIndex].DefinedSize < Convert.ToInt64(Common.FT_SMEAD_COUNTER_SIZE))
                                {
                                    sFieldSize = Common.FT_SMEAD_COUNTER_SIZE;
                                }
                                else
                                {
                                    sFieldSize = rsADO.Fields[lIndex].DefinedSize.ToString();
                                }
                            }
                            else
                            {
                                sFieldType = Common.FT_TEXT;
                                sFieldSize = rsADO.Fields[lIndex].DefinedSize.ToString();
                            }
                        }
                        else
                        {
                            switch (rsADO.Fields[lIndex].Type)
                            {
                                // Enums.DataTypeEnum.rmInteger
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBoolean:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedTinyInt:
                                    {
                                        sFieldType = Common.FT_BOOLEAN;
                                        sFieldSize = Common.FT_BOOLEAN_SIZE;
                                        break;
                                    }
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmDouble:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmCurrency:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmDecimal:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmNumeric:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmSingle:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmVarNumeric:
                                    {
                                        sFieldType = Common.FT_DOUBLE;
                                        sFieldSize = Common.FT_DOUBLE_SIZE;
                                        break;
                                    }
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBigInt:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedBigInt:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmInteger:
                                    {
                                        if (dt.Tables[0].Columns[lIndex].AutoIncrement) // rsADO.Fields(lIndex).Properties("ISAUTOINCREMENT").Value
                                        {
                                            sFieldType = Common.FT_AUTO_INCREMENT;
                                            sFieldSize = Common.FT_AUTO_INCREMENT_SIZE;
                                        }
                                        else if (!string.IsNullOrEmpty(pTableEntity.CounterFieldName) & Strings.StrComp(DatabaseMap.RemoveTableNameFromField(rsADO.Fields[lIndex].Name), DatabaseMap.RemoveTableNameFromField(pTableEntity.IdFieldName), Constants.vbTextCompare) == 0)
                                        {
                                            sFieldType = Common.FT_SMEAD_COUNTER;
                                            sFieldSize = Common.FT_SMEAD_COUNTER_SIZE;
                                        }
                                        else
                                        {
                                            sFieldType = Common.FT_LONG_INTEGER;
                                            sFieldSize = Common.FT_LONG_INTEGER_SIZE;
                                        }

                                        break;
                                    }
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBinary:
                                    {
                                        sFieldType = Common.FT_BINARY;
                                        sFieldSize = Common.FT_MEMO_SIZE;
                                        break;
                                    }
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmSmallInt:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmTinyInt:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedInt:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedSmallInt:
                                    {
                                        sFieldType = Common.FT_SHORT_INTEGER;
                                        sFieldSize = Common.FT_SHORT_INTEGER_SIZE;
                                        break;
                                    }
                            }
                        }

                        // Put Fields in DataTable.
                        if (!string.IsNullOrEmpty(sFieldType) & !string.IsNullOrEmpty(sFieldSize))
                        {
                            row = fieldsDT.NewRow();
                            row["Field_Name"] = rsADO.Fields[lIndex].Name;
                            row["Field_Type"] = sFieldType;
                            row["Field_Size"] = sFieldSize;
                            fieldsDT.Rows.Add(row);
                        }

                    }
                }
            }

            return Json(Common.ConvertDataTableToJQGridResult(fieldsDT, sidx, sord, page, rows));

        }

        private string CheckIfInUse(string pTableName, string sFieldName)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<OneStripJobField> _iOneStripJobField = new Repositories<OneStripJobField>();
            IRepository<ImportLoad> _iImportLoad = new Repositories<ImportLoad>();
            IRepository<ImportField> _iImportField = new Repositories<ImportField>();
            IRepository<SLTextSearchItem> _iSLTextSearchItem = new Repositories<SLTextSearchItem>();
            string sMessage = "";
            string sViewMessage = "";
            List<ImportLoad> oImportLoads;
            List<ImportField> oImportFields;
            List<OneStripJob> oOneStripJobs;
            List<OneStripJobField> oOneStripJobFields;

            List<SLTextSearchItem> oSLTextSearchItem;
            IRepository<OneStripJob> _iOneStripJob = new Repositories<OneStripJob>();
            var moTable = _iTable.All().Where(x => x.TableName.Equals(pTableName)).FirstOrDefault();
            oOneStripJobs = _iOneStripJob.All().Where(x => x.TableName.Equals(pTableName)).ToList();
            oOneStripJobFields = _iOneStripJobField.All().ToList();
            oImportFields = _iImportField.All().ToList();
            oImportLoads = _iImportLoad.All().Where(x => x.FileName.Equals(pTableName)).ToList();
            oSLTextSearchItem = _iSLTextSearchItem.All().Where(x => x.IndexTableName.Equals(pTableName)).ToList();
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            // First check to see if the Field to be deleted is being used anyplace
            if (FieldsMatch(pTableName, sFieldName, moTable.CounterFieldName))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldCounterFieldName"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.DefaultDescriptionField))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldDefaultDescriptionField"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.DescFieldNameOne))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldDescFieldNameOne"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.DescFieldNameTwo))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldDescFieldNameTwo"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.IdFieldName))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldIdFieldName"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.IdFieldName2))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldIdFieldName2"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.RuleDateField))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldRuleDateField"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.TrackingACTIVEFieldName))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldTrackingActiveFieldName"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.TrackingOUTFieldName))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldTrackingOutFieldName"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.InactiveLocationField))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlInactiveLocationField"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.RetentionFieldName))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldRetentionFieldName"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.RetentionDateOpenedField))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlRetentionOpenDateField"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.RetentionDateCreateField))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlRetentionCreateDateField"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.RetentionDateClosedField))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlRetentionDateClosedField"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.RetentionDateOtherField))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlRetentionOtherDateField"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.TrackingPhoneFieldName))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldTrackingPhoneFieldName"), moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.TrackingMailStopFieldName))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldTrackingMailStopFieldName"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.TrackingRequestableFieldName))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldTrackingRequestableFieldName"), sMessage, moTable.UserName);
            }

            if (FieldsMatch(pTableName, sFieldName, moTable.OperatorsIdField))
            {
                sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldOperatorsIdField"), sMessage, moTable.UserName);
            }

            if (!string.IsNullOrEmpty(sMessage))
                sMessage = sMessage + "</br>";

            var lstRelatedTables = _iRelationShip.All().Where(x => (x.LowerTableName) == (moTable.TableName)).ToList();
            var lstRelatedChildTable = _iRelationShip.All().Where(x => (x.UpperTableName) == (moTable.TableName)).ToList();

            foreach (var oParentRelationships in lstRelatedTables)
            {
                if (FieldsMatch(pTableName, sFieldName, oParentRelationships.UpperTableFieldName, oParentRelationships.UpperTableName))
                {
                    sMessage = string.Format(Languages.get_Translation("msgAdminCtrlDownToTable"), sMessage, Constants.vbTab, oParentRelationships.LowerTableName, Strings.StrConv(DatabaseMap.RemoveTableNameFromField(oParentRelationships.LowerTableFieldName), Constants.vbProperCase));
                }
                // Added by Ganesh - 07/01/2016
                if (FieldsMatch(pTableName, sFieldName, oParentRelationships.LowerTableFieldName, oParentRelationships.LowerTableName))
                {
                    sMessage = string.Format(Languages.get_Translation("msgAdminCtrlUpToTable"), sMessage, Constants.vbTab, oParentRelationships.UpperTableName, Strings.StrConv(DatabaseMap.RemoveTableNameFromField(oParentRelationships.UpperTableFieldName), Constants.vbProperCase), Constants.vbCrLf);
                }
            }

            foreach (var oParentRelationships in lstRelatedChildTable)
            {
                if (FieldsMatch(pTableName, sFieldName, oParentRelationships.LowerTableFieldName, oParentRelationships.LowerTableName))
                {
                    sMessage = string.Format(Languages.get_Translation("msgAdminCtrlDownToTable"), sMessage, Constants.vbTab, oParentRelationships.UpperTableName, Strings.StrConv(DatabaseMap.RemoveTableNameFromField(oParentRelationships.UpperTableFieldName), Constants.vbProperCase));
                }
            }

            foreach (var oOneStripJob in oOneStripJobs)
            {
                if (Strings.StrComp(pTableName, oOneStripJob.TableName, Constants.vbTextCompare) == 0)
                {

                    foreach (var oOneStripJobField in oOneStripJobFields)
                    {
                        if (FieldsMatch(pTableName, sFieldName, oOneStripJobField.FieldName, oOneStripJob.TableName))
                        {
                            sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldUsedInOneStripJobFields"), sMessage);
                            //oOneStripJobField = null;
                            break;
                        }
                    }
                }
            }

            foreach (var oImportLoad in oImportLoads)
            {
                if (Strings.StrComp(pTableName, oImportLoad.FileName, Constants.vbTextCompare) == 0)
                {

                    foreach (var oImportField in oImportFields)
                    {
                        if (FieldsMatch(pTableName, sFieldName, oImportField.FieldName, oImportLoad.FileName))
                        {
                            sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldUsedInImportFields"), sMessage);
                            //oImportField = null;
                            break;
                        }
                    }
                }
            }
            var lViewEntities = _iView.All();
            var lViewColumnEntities = _iViewColumn.All();

            var lLoopViewEntities = lViewEntities.Where(x => (x.TableName.Trim().ToLower()) == (moTable.TableName.Trim().ToLower()));
            foreach (var oViews in lLoopViewEntities)
            {
                var lInViewColumnEntities = lViewColumnEntities.Where(x => x.ViewsId == oViews.Id);
                foreach (var oViewColumns in lInViewColumnEntities)
                {

                    if (this.FieldsMatch(pTableName, sFieldName, oViewColumns.FieldName, moTable.TableName))
                    {
                        // If (Not oViewColumns.Deleted) Then
                        // sMessage = sMessage & "  Used in View" & vbTab & """" & oViews.ViewName & """</br>"
                        // Else
                        if (oViewColumns.Id != 0)
                        {
                            sMessage = string.Format(Languages.get_Translation("msgAdminCtrlUsedInView"), sMessage, Constants.vbTab, oViews.ViewName);
                        }
                        else
                        {
                            sMessage = string.Format(Languages.get_Translation("msgAdminCtrlUsedInView"), sMessage, Constants.vbTab, oViews.ViewName);
                        }
                        // End If
                    }
                }
            }

            // For Each oSLTextSrchItem In oSLTextSearchItem
            // If (FieldsMatch(pTableName, sFieldName, oSLTextSrchItem.IndexFieldName, oSLTextSrchItem.IndexTableName)) Then
            // If (goSDLKini.LicensedFullTextSearchEnabled) Then
            // sMessage = sMessage & "  Used in ""Full Text Search""" & vbCrLf
            // Else
            // sMessage = sMessage & "  Used in ""Global Field Search""" & vbCrLf
            // End If

            // oSLTextSearchItem = Nothing
            // Exit For
            // End If
            // Next

            // For Each oViews In oTables.Views(geViewType_vtMaster)
            // For Each oViewColumns In oViews.ViewColumns
            // If (FieldsMatch(msTableName, sFieldName, oViewColumns.FieldName, oTables.TableName)) Then
            // If (Not oViewColumns.Deleted) Then
            // sViewMessage = sViewMessage & "  Used in View" & vbTab & """" & oViews.ViewName & """" & vbCrLf
            // Else
            // If (oViewColumns.Id <> 0) Then
            // sViewMessage = sViewMessage & "  Used in View" & vbTab & """" & oViews.ViewName & """" & vbCrLf
            // End If
            // End If
            // End If
            // Next
            // Next
            // Next

            // If ((sMessage <> "") And (bRelationship)) Then sMessage = sMessage & vbCrLf
            // If (sViewMessage <> "") Then sMessage = sMessage & sViewMessage & vbCrLf
            // theApp.OneStripJobs.Load()


            // For Each oSLTextSearchItem In moTable.TextSearchItems
            // If (FieldsMatch(msTableName, sFieldName, oSLTextSearchItem.IndexFieldName, oSLTextSearchItem.IndexTableName)) Then
            // If (goSDLKini.LicensedFullTextSearchEnabled) Then
            // sMessage = sMessage & "  Used in ""Full Text Search""" & vbCrLf
            // Else
            // sMessage = sMessage & "  Used in ""Global Field Search""" & vbCrLf
            // End If

            // oSLTextSearchItem = Nothing
            // Exit For
            // End If
            // Next

            // CheckIfInUse = sMessage
            return sMessage;

        }

        // Check if Indexes exists for field been operated.
        public string CheckIfIndexesExist(string sTableName, string sFieldName, bool bAsk)
        {
            string sMessage = "";
            List<SchemaIndex> oSchemaList;
            ADODB.Connection sAdoConn;
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();

            var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())).FirstOrDefault();
            sAdoConn = DataServices.DBOpen(oTables, _iDatabas.All());
            oSchemaList = SchemaIndex.GetTableIndexes(sTableName, sAdoConn);

            foreach (var oSchema in oSchemaList)
            {
                if (this.FieldsMatch(sTableName, sFieldName, oSchema.ColumnName))
                {
                    sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldIsPartOfAtLeast1Index"), sFieldName);
                    if (bAsk)
                    {
                        sMessage = string.Format(Languages.get_Translation("msgAdminCtrlFieldRemovingField"), sMessage, Constants.vbNewLine, Constants.vbCrLf);
                    }
                    else
                    {
                        sMessage = string.Format(Languages.get_Translation("msgAdminCtrlCannotBModified"), sMessage);
                    }
                    //oSchema = null;
                }
            }
            return sMessage;
        }

        private bool FieldsMatch(string sFileName, string sFieldName, string sCompareName, string sCompareTable = "")
        {
            bool FieldsMatchRet = default;

            FieldsMatchRet = false;

            sFileName = Strings.Trim(sFileName);
            sFieldName = Strings.Trim(sFieldName);
            sCompareName = Strings.Trim(sCompareName);

            if (Strings.InStr(sCompareName, ".") > 0)
            {
                if (Strings.StrComp(sCompareName, sFileName + "." + sFieldName, Constants.vbTextCompare) == 0)
                    FieldsMatchRet = true;
            }
            else if (string.IsNullOrEmpty(sCompareTable) | Strings.StrComp(sCompareTable, sFileName, Constants.vbTextCompare) == 0)
            {
                if (Strings.StrComp(sCompareName, sFieldName, Constants.vbTextCompare) == 0)
                    FieldsMatchRet = true;
            }

            return FieldsMatchRet;
        }

        [HttpPost]
        public JsonResult GetFieldTypeList([FromBody] GetFieldTypeListReqModel req)
        {
            var lstFieldTypes = new List<KeyValuePair<string, string>>();
            bool bAutoCompensator;
            var bHasAutoIncrement = default(bool);
            string lstFieldTypesJsonList = "";
            int lCounter;
            Table pTableEntity;
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            try
            {
                object pDatabaseEntity = null;
                pTableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(req.pTableName)).FirstOrDefault();
                if (pTableEntity is not null)
                {
                    if (pTableEntity.DBName is not null)
                    {
                        pDatabaseEntity = _iDatabas.Where(x => x.DBName.Trim().ToLower().Equals(pTableEntity.DBName.Trim().ToLower())).FirstOrDefault();
                    }
                }

                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString((Databas)pDatabaseEntity);


                var dt = _IDBManager.ExecuteDataSetWithSchema(CommandType.Text, "SELECT * FROM [" + req.pTableName + "] WHERE 0 = 1");
                _IDBManager.Dispose();
                var loopTo = dt.Tables[0].Columns.Count - 1;
                for (lCounter = 0; lCounter <= loopTo; lCounter++)
                {
                    if (dt.Tables[0].Columns[lCounter].AutoIncrement)
                    {
                        bHasAutoIncrement = true;
                        break;
                    }
                }

                bAutoCompensator = true;

                if (!bHasAutoIncrement)
                {
                    bAutoCompensator = false;
                    lstFieldTypes.Add(new KeyValuePair<string, string>(((int)Enums.meTableFieldTypes.ftCounter).ToString(), Common.FT_AUTO_INCREMENT));
                }

                // If ((Not bHasAutoIncrement) Or (rsADO.Fields.Item(msFieldName).Properties.Item("IsAutoIncrement").GetValue)) Then
                // bAutoCompensator = False
                // cboFieldType.AddItem FT_AUTO_INCREMENT
                // cboFieldType.ItemData(cboFieldType.NewIndex) = ftCounter
                // End If
                lstFieldTypes.Add(new KeyValuePair<string, string>(((int)Enums.meTableFieldTypes.ftLong).ToString(), Common.FT_LONG_INTEGER));
                lstFieldTypes.Add(new KeyValuePair<string, string>(((int)Enums.meTableFieldTypes.ftText).ToString(), Common.FT_TEXT));
                lstFieldTypes.Add(new KeyValuePair<string, string>(((int)Enums.meTableFieldTypes.ftInteger).ToString(), Common.FT_SHORT_INTEGER));
                lstFieldTypes.Add(new KeyValuePair<string, string>(((int)Enums.meTableFieldTypes.ftBoolean).ToString(), Common.FT_BOOLEAN));
                lstFieldTypes.Add(new KeyValuePair<string, string>(((int)Enums.meTableFieldTypes.ftDouble).ToString(), Common.FT_DOUBLE));
                lstFieldTypes.Add(new KeyValuePair<string, string>(((int)Enums.meTableFieldTypes.ftDate).ToString(), Common.FT_DATE));
                lstFieldTypes.Add(new KeyValuePair<string, string>(((int)Enums.meTableFieldTypes.ftMemo).ToString(), Common.FT_MEMO));

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                lstFieldTypesJsonList = JsonConvert.SerializeObject(lstFieldTypes, Newtonsoft.Json.Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlFieldRetreivedData");
            }
            catch (Exception)
            {

            }

            return Json(new
            {
                lstFieldTypesJson = lstFieldTypesJsonList,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });

        }
        // Delete the temp view of table, if no viewcolumns exists.
        public void DeleteSQLViewWithNoViewColumnExists(string tableName)
        {
            IRepository<View> _iView = new Repositories<View>();
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            try
            {
                if (!string.IsNullOrEmpty(tableName))
                {
                    var tableViews = _iView.All().Where(x => (x.TableName) == (tableName));
                    foreach (var vView in tableViews)
                    {
                        var viewColumns = _iViewColumn.All().Where(x => x.ViewsId == vView.Id);
                        if (!(viewColumns == null))
                        {
                            if (viewColumns.Count() == 0)
                            {
                                ViewModel.SQLViewDelete(vView.Id, passport);
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
        #endregion

        #region Tracking
        public PartialViewResult LoadTableTracking()
        {
            return PartialView("_TableTrackingPartial");
        }

        [HttpPost]
        public IActionResult GetTableTrackingProperties([FromForm] string tableName)
        {
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                var pTableEntity = _iTable.All();
                var pContainerTables = pTableEntity.Where(m => m.TrackingTable > 0).OrderBy(m => m.TrackingTable);
                var pSystemEntities = _iSystem.All().OrderBy(m => m.Id).FirstOrDefault();
                var pRelationShipEntity = _iRelationShip.All().OrderBy(m => m.Id);
                var pSelectTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                var Container1Table = _iTable.All().Where(m => m.TrackingTable == 1).FirstOrDefault();
                string DBConName = pSelectTable.DBName;
                var sADOConn = DataServices.DBOpen();
                var schemaColumnList = SchemaInfoDetails.GetSchemaInfo(tableName, sADOConn);
                var containerList = new List<KeyValuePair<int, string>>();
                var OutFieldList = new List<KeyValuePair<string, string>>();
                var DueBackFieldList = new List<KeyValuePair<string, string>>();
                var ActiveFieldList = new List<KeyValuePair<string, string>>();
                var EmailAddressList = new List<KeyValuePair<string, string>>();
                var RequesFieldList = new List<KeyValuePair<string, string>>();
                var InactiveFieldList = new List<KeyValuePair<string, string>>();
                var ArchiveFieldList = new List<KeyValuePair<string, string>>();
                var UserIdFieldList = new List<KeyValuePair<string, string>>();
                var PhoneFieldList = new List<KeyValuePair<string, string>>();
                var MailSTopFieldList = new List<KeyValuePair<string, string>>();
                var SignatureFieldList = new List<KeyValuePair<string, string>>();
                var defaultTracking = new List<KeyValuePair<string, string>>();
                string lblDestination = null;
                // Get ADO connection name
                if (pSelectTable is not null)
                {
                    sADOConn = DataServices.DBOpen(pSelectTable, _iDatabas.All());
                }
                containerList.Add(new KeyValuePair<int, string>(0, "{ Not a container }"));
                OutFieldList.Add(new KeyValuePair<string, string>("0", "{No Out Field}"));
                DueBackFieldList.Add(new KeyValuePair<string, string>("0", "{No Due Back Days Field}"));
                ActiveFieldList.Add(new KeyValuePair<string, string>("0", "{No Active Field}"));
                EmailAddressList.Add(new KeyValuePair<string, string>("0", "{No Email Address Field}"));
                RequesFieldList.Add(new KeyValuePair<string, string>("0", "{No Requestable Field}"));
                InactiveFieldList.Add(new KeyValuePair<string, string>("0", "{No Inactive Storage Field}"));
                ArchiveFieldList.Add(new KeyValuePair<string, string>("0", "{No Archive Storage Field}"));
                UserIdFieldList.Add(new KeyValuePair<string, string>("0", "{No User Id Field}"));
                PhoneFieldList.Add(new KeyValuePair<string, string>("0", "{No Phone Field}"));
                MailSTopFieldList.Add(new KeyValuePair<string, string>("0", "{No MailStop Field}"));
                SignatureFieldList.Add(new KeyValuePair<string, string>("0", "{No Signature Required Field}"));
                defaultTracking.Add(new KeyValuePair<string, string>("0", ""));
                // Fill Container Level DropDown List And Selected table data
                if (!(pContainerTables.Count() == 0))
                {
                    foreach (Table tableObj in pContainerTables.ToList())
                    {
                        string containerVal = Convert.ToString(tableObj.TrackingTable) + " (" + tableObj.UserName + ")";
                        containerList.Add(new KeyValuePair<int, string>((int)tableObj.TrackingTable, containerVal));
                    }
                }
                int countValue = pContainerTables.Count() + 1;
                containerList.Add(new KeyValuePair<int, string>(countValue, Convert.ToString(countValue) + " { Unused }"));

                if (schemaColumnList is not null)
                {
                    // Out Field DropDown List
                    OutFieldList = (List<KeyValuePair<string, string>>)DataServices.IsContainField(sADOConn, tableName, schemaColumnList, "Out", OutFieldList);

                    // Due Back Days Field
                    var bHasAField = default(bool);
                    bool bIsSystemAdmin;
                    foreach (SchemaColumns schemaColumnObj in schemaColumnList)
                    {
                        if (schemaColumnObj.ColumnName.Trim().ToLower().Equals("DueBackDays".Trim().ToLower()))
                        {
                            bHasAField = true;
                            break;
                        }
                    }
                    bIsSystemAdmin = DataServices.IsSysAdmin(tableName, sADOConn);
                    if (!bHasAField & bIsSystemAdmin)
                    {
                        DueBackFieldList.Add(new KeyValuePair<string, string>("DueBackDays", "DueBackDays"));
                        bHasAField = false;
                    }
                    foreach (SchemaColumns oSchemaColumnObj in schemaColumnList)
                    {
                        switch (oSchemaColumnObj.DataType)
                        {
                            case Enums.DataTypeEnum.rmInteger:
                            case Enums.DataTypeEnum.rmUnsignedInt:
                            case Enums.DataTypeEnum.rmBigInt:
                            case Enums.DataTypeEnum.rmUnsignedBigInt:
                            case Enums.DataTypeEnum.rmSingle:
                            case Enums.DataTypeEnum.rmDouble:
                                {
                                    bHasAField = oSchemaColumnObj.ColumnName.Trim().ToLower().Equals(DatabaseMap.RemoveTableNameFromField(pSelectTable.IdFieldName.Trim().ToLower()));
                                    if (!bHasAField)
                                    {
                                        foreach (RelationShip oRelationshipObj in pRelationShipEntity)
                                        {
                                            if (oSchemaColumnObj.ColumnName.Trim().ToLower().Equals(DatabaseMap.RemoveTableNameFromField(oRelationshipObj.UpperTableFieldName.Trim().ToLower())))
                                            {
                                                bHasAField = true;
                                                break;
                                            }
                                        }

                                        if (!bHasAField)
                                        {
                                            foreach (RelationShip oRelationshipObj in pRelationShipEntity)
                                            {
                                                if (oSchemaColumnObj.ColumnName.Trim().ToLower().Equals(DatabaseMap.RemoveTableNameFromField(oRelationshipObj.LowerTableFieldName.Trim().ToLower())))
                                                {
                                                    bHasAField = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (!bHasAField)
                                    {
                                        DueBackFieldList.Add(new KeyValuePair<string, string>(oSchemaColumnObj.ColumnName, oSchemaColumnObj.ColumnName));
                                    }

                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                        }
                    }


                    // Active Field List
                    ActiveFieldList = (List<KeyValuePair<string, string>>)DataServices.IsContainField(sADOConn, tableName, schemaColumnList, "Active", ActiveFieldList);

                    // Email Address List
                    EmailAddressList = (List<KeyValuePair<string, string>>)DataServices.IsContainStringField(sADOConn, tableName, schemaColumnList, "EmailAddress", EmailAddressList);

                    // Requestable Field List
                    RequesFieldList = (List<KeyValuePair<string, string>>)DataServices.IsContainField(sADOConn, tableName, schemaColumnList, "Requestable", RequesFieldList);

                    // Inactive Storage Field List
                    InactiveFieldList = (List<KeyValuePair<string, string>>)DataServices.IsContainField(sADOConn, tableName, schemaColumnList, "InactiveStorage", InactiveFieldList);

                    // Archive Storage Field List
                    ArchiveFieldList = (List<KeyValuePair<string, string>>)DataServices.IsContainField(sADOConn, tableName, schemaColumnList, "ArchiveStorage", ArchiveFieldList);

                    // User Id Field List
                    var bHasAUserField = default(bool);
                    bool userIdIsSysAdmin;
                    foreach (SchemaColumns oSchemaColumnObj in schemaColumnList)
                    {
                        if (oSchemaColumnObj.ColumnName.Trim().ToLower().Equals("OperatorsId".Trim().ToLower()))
                        {
                            bHasAUserField = true;
                        }
                        else if (oSchemaColumnObj.ColumnName.Trim().ToLower().Equals("UserId".Trim().ToLower()))
                        {
                            bHasAUserField = true;
                        }

                        if (bHasAUserField)
                        {
                            break;
                        }
                    }

                    userIdIsSysAdmin = DataServices.IsSysAdmin(tableName, sADOConn);
                    if (!bHasAUserField & userIdIsSysAdmin)
                    {
                        UserIdFieldList.Add(new KeyValuePair<string, string>("UserId", "UserId"));
                    }

                    foreach (SchemaColumns oSchemaColumnObj in schemaColumnList)
                    {
                        if (!SchemaInfoDetails.IsSystemField(oSchemaColumnObj.ColumnName) & oSchemaColumnObj.IsString & oSchemaColumnObj.CharacterMaxLength == 30)
                        {
                            UserIdFieldList.Add(new KeyValuePair<string, string>(oSchemaColumnObj.ColumnName, oSchemaColumnObj.ColumnName));
                        }
                    }

                    // Phone Field List
                    PhoneFieldList = (List<KeyValuePair<string, string>>)DataServices.IsContainStringField(sADOConn, tableName, schemaColumnList, "Phone", PhoneFieldList);

                    // Mail Stop Field List
                    MailSTopFieldList = (List<KeyValuePair<string, string>>)DataServices.IsContainStringField(sADOConn, tableName, schemaColumnList, "MailStop", MailSTopFieldList);

                    // Signature Required Field LIst
                    SignatureFieldList = (List<KeyValuePair<string, string>>)DataServices.IsContainField(sADOConn, tableName, schemaColumnList, "SignatureRequired", SignatureFieldList);

                }
                if (Container1Table is not null)
                {
                    lblDestination = Container1Table.UserName;
                }

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string containerJSONList = JsonConvert.SerializeObject(containerList, Newtonsoft.Json.Formatting.Indented, Setting);
                string systemObject = JsonConvert.SerializeObject(pSystemEntities, Newtonsoft.Json.Formatting.Indented, Setting);
                string oneTableObj = JsonConvert.SerializeObject(pSelectTable, Newtonsoft.Json.Formatting.Indented, Setting);
                string outFieldJSONList = JsonConvert.SerializeObject(OutFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                string dueBackJSONList = JsonConvert.SerializeObject(DueBackFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                string activeFieldJSONList = JsonConvert.SerializeObject(ActiveFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                string emailAddressJSONList = JsonConvert.SerializeObject(EmailAddressList, Newtonsoft.Json.Formatting.Indented, Setting);
                string requestFieldJSONList = JsonConvert.SerializeObject(RequesFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                string inactiveJSONList = JsonConvert.SerializeObject(InactiveFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                string archiveJSONList = JsonConvert.SerializeObject(ArchiveFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                string userFieldJSONList = JsonConvert.SerializeObject(UserIdFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                string phoneFieldJSONList = JsonConvert.SerializeObject(PhoneFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                string mailStopJSONList = JsonConvert.SerializeObject(MailSTopFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                string signatureJSONList = JsonConvert.SerializeObject(SignatureFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
                string lblDestinationJSON = JsonConvert.SerializeObject(lblDestination, Newtonsoft.Json.Formatting.Indented, Setting);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlAllDataGetSuccessfully");
                return Json(new
                {
                    containerJSONList,
                    systemObject,
                    oneTableObj,
                    outFieldJSONList,
                    dueBackJSONList,
                    activeFieldJSONList,
                    emailAddressJSONList,
                    requestFieldJSONList,
                    inactiveJSONList,
                    archiveJSONList,
                    userFieldJSONList,
                    phoneFieldJSONList,
                    mailStopJSONList,
                    signatureJSONList,
                    lblDestinationJSON,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }

            catch (Exception)
            {
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                Keys.ErrorType = "e";
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }

        }
        [HttpPost]
        public IActionResult SetTableTrackingDetails([FromForm] Table trackingForm, [FromForm] bool FieldFlag,
            [FromForm] bool pAutoAddNotification, [FromForm] bool pAllowBatchRequesting,
            [FromForm] bool pTrackable)
        {
            string warnMsgJSON = "";
            IRepository<AssetStatu> _iAssetStatus = new Repositories<AssetStatu>();
            IRepository<TrackingHistory> _iTrackingHistory = new Repositories<TrackingHistory>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<TrackingStatu> _iTrackingStatus = new Repositories<TrackingStatu>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                _iTrackingHistory.BeginTransaction();
                _iAssetStatus.BeginTransaction();
                _iTrackingStatus.BeginTransaction();
                var ptableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(trackingForm.TableName.Trim().ToLower())).FirstOrDefault();
                var ptableByLevel = _iTable.All().Where(m => m.TrackingTable > 0).OrderBy(m => m.TrackingTable);
                // Dim ptableEntity = _iTable.All.Where(Function(m) m.TableName.ToLower.Trim.Equals(trackingForm.TableName)).FirstOrDefault
                var modifyTable = new List<Table>();
                var sADOConnDefault = DataServices.DBOpen();
                short newLevel;
                int UserLinkIndexIdSize = 30;
                var oTrackingTable = _iTable.All().Where(m => m.TrackingTable == 1).FirstOrDefault();
                var mbADOCon = DataServices.DBOpen(ptableEntity, _iDatabas.All());
                trackingForm.AutoAddNotification = pAutoAddNotification;
                trackingForm.AllowBatchRequesting = pAllowBatchRequesting;
                trackingForm.Trackable = pTrackable;
                // ReOrder of container level 
                if (trackingForm.TrackingTable > 0)
                {
                    newLevel = (short)trackingForm.TrackingTable;
                }
                else
                {
                    newLevel = 0;
                }
                if (ptableEntity.TrackingTable != trackingForm.TrackingTable)
                {
                    ptableEntity.TrackingTable = default;
                    foreach (Table tbObject in ptableByLevel)
                    {
                        if (tbObject.TrackingTable <= (short)newLevel && tbObject.TableName != trackingForm.TableName)
                        {
                            modifyTable.Add(tbObject);
                        }
                    }
                    if (Convert.ToBoolean(!Operators.ConditionalCompareObjectEqual(newLevel, 0, false)))
                    {
                        modifyTable.Add(ptableEntity);
                    }
                    else
                    {
                        ptableEntity.TrackingTable = (short?)0;
                    }
                    foreach (Table tbObject in ptableByLevel)
                    {
                        if (tbObject.TrackingTable > newLevel && tbObject.TrackingTable != trackingForm.TrackingTable)
                        {
                            modifyTable.Add(tbObject);
                        }
                    }
                    int iLevel = 1;
                    foreach (Table tbObject in modifyTable)
                    {
                        tbObject.TrackingTable = (short?)iLevel;
                        iLevel = iLevel + 1;
                    }
                    foreach (Table tbObject in modifyTable)
                    {
                        if (!tbObject.TableName.Trim().ToLower().Equals(trackingForm.TableName.Trim().ToLower()))
                        {
                            _iTable.Update(tbObject);
                        }
                    }
                }

                bool mbIsSysAdmin = DataServices.IsSysAdmin(ptableEntity.TableName, mbADOCon);

                // added by kirti'
                if (mbIsSysAdmin)
                {
                    if (trackingForm.TrackingOUTFieldName is not null & trackingForm.TrackingOUTFieldName != "0")
                    {
                        AddFieldIfNeeded(trackingForm.TrackingOUTFieldName, ptableEntity, "BIT");
                    }
                    if (trackingForm.TrackingDueBackDaysFieldName is not null & trackingForm.TrackingDueBackDaysFieldName != "0")
                    {
                        AddFieldIfNeeded(trackingForm.TrackingDueBackDaysFieldName, ptableEntity, "INT");
                    }
                    if (trackingForm.TrackingACTIVEFieldName is not null & trackingForm.TrackingACTIVEFieldName != "0")
                    {
                        AddFieldIfNeeded(trackingForm.TrackingACTIVEFieldName, ptableEntity, "BIT");
                    }
                    if (trackingForm.TrackingRequestableFieldName is not null & trackingForm.TrackingRequestableFieldName != "0")
                    {
                        AddFieldIfNeeded(trackingForm.TrackingRequestableFieldName, ptableEntity, "BIT");
                    }
                    if (trackingForm.InactiveLocationField is not null & trackingForm.InactiveLocationField != "0")
                    {
                        AddFieldIfNeeded(trackingForm.InactiveLocationField, ptableEntity, "BIT");
                    }
                    if (trackingForm.ArchiveLocationField is not null & trackingForm.ArchiveLocationField != "0")
                    {
                        AddFieldIfNeeded(trackingForm.ArchiveLocationField, ptableEntity, "BIT");
                    }
                    if (trackingForm.OperatorsIdField is not null & trackingForm.OperatorsIdField != "0")
                    {
                        AddFieldIfNeeded(trackingForm.OperatorsIdField, ptableEntity, "VARCHAR(30)");
                    }
                    if (trackingForm.TrackingPhoneFieldName is not null & trackingForm.TrackingPhoneFieldName != "0")
                    {
                        AddFieldIfNeeded(trackingForm.TrackingPhoneFieldName, ptableEntity, "VARCHAR(30)");
                    }
                    if (trackingForm.TrackingMailStopFieldName is not null & trackingForm.TrackingMailStopFieldName != "0")
                    {
                        AddFieldIfNeeded(trackingForm.TrackingMailStopFieldName, ptableEntity, "VARCHAR(30)");
                    }
                    if (trackingForm.TrackingEmailFieldName is not null & trackingForm.TrackingEmailFieldName != "0")
                    {
                        AddFieldIfNeeded(trackingForm.TrackingEmailFieldName, ptableEntity, "VARCHAR(320)");
                    }
                    if (trackingForm.SignatureRequiredFieldName is not null & trackingForm.SignatureRequiredFieldName != "0")
                    {
                        AddFieldIfNeeded(trackingForm.SignatureRequiredFieldName, ptableEntity, "VARCHAR(320)");
                    }

                }

                // Delete/Modify/Add Tracking Status Field
                bool IsSysAdminTracking = DataServices.IsSysAdmin("TrackingStatus", sADOConnDefault);
                IsSysAdminTracking = IsSysAdminTracking & DataServices.IsSysAdmin("AssetStatus", sADOConnDefault);
                IsSysAdminTracking = IsSysAdminTracking & DataServices.IsSysAdmin("TrackingHistory", sADOConnDefault);
                if (!string.IsNullOrEmpty(trackingForm.TrackingStatusFieldName))
                {
                    if (!string.IsNullOrEmpty(ptableEntity.TrackingStatusFieldName))
                    {
                        if (IsSysAdminTracking & !trackingForm.TrackingStatusFieldName.Trim().ToLower().Equals(ptableEntity.TrackingStatusFieldName.Trim().ToLower()))
                        {
                            bool boolSQLVal;
                            string indexStatusSQL = "EXEC sp_rename N'TrackingStatus." + ptableEntity.TrackingStatusFieldName + "',N'" + trackingForm.TrackingStatusFieldName + "',N'INDEX'";
                            boolSQLVal = DataServices.ProcessADOCommand(ref indexStatusSQL, sADOConnDefault, false);
                            if (Convert.ToBoolean(boolSQLVal))
                            {
                                string indexHistorySQL = "EXEC sp_rename N'TrackingHistory." + ptableEntity.TrackingStatusFieldName + "',N'" + trackingForm.TrackingStatusFieldName + "',N'INDEX'";
                                boolSQLVal = DataServices.ProcessADOCommand(ref indexHistorySQL, sADOConnDefault, false);
                            }
                            if (Convert.ToBoolean(boolSQLVal))
                            {
                                string indexAssetSQL = "EXEC sp_rename N'AssetStatus." + ptableEntity.TrackingStatusFieldName + "',N'" + trackingForm.TrackingStatusFieldName + "',N'INDEX'";
                                boolSQLVal = DataServices.ProcessADOCommand(ref indexAssetSQL, sADOConnDefault, false);
                            }
                            if (Convert.ToBoolean(boolSQLVal))
                            {
                                string updateStatusSQL = "EXEC sp_rename N'TrackingStatus." + ptableEntity.TrackingStatusFieldName + "',N'" + trackingForm.TrackingStatusFieldName + "',N'COLUMN'";
                                boolSQLVal = DataServices.ProcessADOCommand(ref updateStatusSQL, sADOConnDefault, false);
                            }
                            if (Convert.ToBoolean(boolSQLVal))
                            {
                                string updateHistorySQL = "EXEC sp_rename N'TrackingHistory." + ptableEntity.TrackingStatusFieldName + "',N'" + trackingForm.TrackingStatusFieldName + "',N'COLUMN'";
                                boolSQLVal = DataServices.ProcessADOCommand(ref updateHistorySQL, sADOConnDefault, false);
                            }
                            if (Convert.ToBoolean(boolSQLVal))
                            {
                                string updateAssetSQL = "EXEC sp_rename N'AssetStatus." + ptableEntity.TrackingStatusFieldName + "',N'" + trackingForm.TrackingStatusFieldName + "',N'COLUMN'";
                                boolSQLVal = DataServices.ProcessADOCommand(ref updateAssetSQL, sADOConnDefault, false);
                            }
                            if (Convert.ToBoolean(!boolSQLVal))
                            {
                                Keys.ErrorType = "e";
                                Keys.ErrorMessage = Keys.ErrorMessageJS();
                            }

                        }
                    }
                    else if (IsSysAdminTracking)
                    {
                        object boolProcessSQL;
                        string trackingStatusSQL = "ALTER TABLE [TrackingStatus] ADD [" + trackingForm.TrackingStatusFieldName + "] VARCHAR(30) NULL";
                        boolProcessSQL = DataServices.ProcessADOCommand(ref trackingStatusSQL, sADOConnDefault, false);
                        if (Convert.ToBoolean(boolProcessSQL))
                        {
                            string trackingHistorySQL = "ALTER TABLE [TrackingHistory] ADD [" + trackingForm.TrackingStatusFieldName + "] VARCHAR(30) NULL";
                            boolProcessSQL = DataServices.ProcessADOCommand(ref trackingHistorySQL, sADOConnDefault, false);
                        }
                        if (Convert.ToBoolean(boolProcessSQL))
                        {
                            string assetStatusSQL = "ALTER TABLE [AssetStatus] ADD [" + trackingForm.TrackingStatusFieldName + "] VARCHAR(30) NULL";
                            boolProcessSQL = DataServices.ProcessADOCommand(ref assetStatusSQL, sADOConnDefault, false);
                        }
                        if (Convert.ToBoolean(boolProcessSQL))
                        {
                            string iStatusSQL = "CREATE UNIQUE INDEX " + trackingForm.TrackingStatusFieldName + " ON [TrackingStatus] ([" + trackingForm.TrackingStatusFieldName + "], [Id])";
                            boolProcessSQL = DataServices.ProcessADOCommand(ref iStatusSQL, sADOConnDefault, false);
                        }
                        if (Convert.ToBoolean(boolProcessSQL))
                        {
                            string iHistorySQL = "CREATE UNIQUE INDEX " + trackingForm.TrackingStatusFieldName + " ON [TrackingHistory] ([" + trackingForm.TrackingStatusFieldName + "], [Id])";
                            boolProcessSQL = DataServices.ProcessADOCommand(ref iHistorySQL, sADOConnDefault, false);
                        }
                        if (Convert.ToBoolean(boolProcessSQL))
                        {
                            string iAssetSQL = "CREATE UNIQUE INDEX " + trackingForm.TrackingStatusFieldName + " ON [AssetStatus] ([" + trackingForm.TrackingStatusFieldName + "], [Id])";
                            boolProcessSQL = DataServices.ProcessADOCommand(ref iAssetSQL, sADOConnDefault, false);
                        }
                        if (!Convert.ToBoolean(boolProcessSQL))
                        {
                            Keys.ErrorType = "e";
                            Keys.ErrorMessage = Keys.ErrorMessageJS();
                        }

                    }
                }

                else if (FieldFlag)
                {
                    if (IsSysAdminTracking & ptableEntity.TrackingStatusFieldName is not null)
                    {
                        object boolProcessSQL;
                        boolProcessSQL = DataServices.RemoveTrackingStatusField(sADOConnDefault, "TrackingStatus", ptableEntity.TrackingStatusFieldName);
                        if (Convert.ToBoolean(boolProcessSQL))
                        {
                            boolProcessSQL = DataServices.RemoveTrackingStatusField(sADOConnDefault, "TrackingHistory", ptableEntity.TrackingStatusFieldName);
                        }
                        if (Convert.ToBoolean(boolProcessSQL))
                        {
                            boolProcessSQL = DataServices.RemoveTrackingStatusField(sADOConnDefault, "AssetStatus", ptableEntity.TrackingStatusFieldName);
                        }
                        if (Convert.ToBoolean(boolProcessSQL))
                        {
                            Keys.ErrorType = "e";
                            Keys.ErrorMessage = Keys.ErrorMessageJS();
                        }
                    }

                }
                if (trackingForm.TrackingTable == 0)
                {
                    ptableEntity.TrackingTable = 0;
                }
                ptableEntity.TrackingStatusFieldName = trackingForm.TrackingStatusFieldName;
                if (trackingForm.OutTable is null)
                {
                    ptableEntity.OutTable = 0; // Set Default Use out Field
                }
                else
                {
                    ptableEntity.OutTable = (short)trackingForm.OutTable;
                }
                // ' Condition changed by hasmukh for fix [FUS-1914]
                if (!string.IsNullOrEmpty(trackingForm.TrackingDueBackDaysFieldName) & trackingForm.TrackingDueBackDaysFieldName != "0")
                {
                    ptableEntity.TrackingDueBackDaysFieldName = trackingForm.TrackingDueBackDaysFieldName;
                }
                else if (trackingForm.TrackingDueBackDaysFieldName == "0")
                {
                    ptableEntity.TrackingDueBackDaysFieldName = null;
                }


                if (!string.IsNullOrEmpty(trackingForm.TrackingOUTFieldName) & trackingForm.TrackingOUTFieldName != "0")
                {
                    ptableEntity.TrackingOUTFieldName = trackingForm.TrackingOUTFieldName;
                }
                else if (trackingForm.TrackingOUTFieldName == "0")
                {
                    ptableEntity.TrackingOUTFieldName = null;
                }

                if (!string.IsNullOrEmpty(trackingForm.TrackingACTIVEFieldName) & trackingForm.TrackingACTIVEFieldName != "0")
                {
                    ptableEntity.TrackingACTIVEFieldName = trackingForm.TrackingACTIVEFieldName;
                }
                else if (trackingForm.TrackingACTIVEFieldName == "0")
                {
                    ptableEntity.TrackingACTIVEFieldName = null;
                }

                if (!string.IsNullOrEmpty(trackingForm.TrackingEmailFieldName) & trackingForm.TrackingEmailFieldName != "0")
                {
                    ptableEntity.TrackingEmailFieldName = trackingForm.TrackingEmailFieldName;
                }
                else if (trackingForm.TrackingEmailFieldName == "0")
                {
                    ptableEntity.TrackingEmailFieldName = null;
                }

                if (!string.IsNullOrEmpty(trackingForm.TrackingRequestableFieldName) & trackingForm.TrackingRequestableFieldName != "0")
                {
                    ptableEntity.TrackingRequestableFieldName = trackingForm.TrackingRequestableFieldName;
                }
                else if (trackingForm.TrackingRequestableFieldName == "0")
                {
                    ptableEntity.TrackingRequestableFieldName = null;
                }

                if (!string.IsNullOrEmpty(trackingForm.TrackingPhoneFieldName) & trackingForm.TrackingPhoneFieldName != "0")
                {
                    ptableEntity.TrackingPhoneFieldName = trackingForm.TrackingPhoneFieldName;
                }
                else if (trackingForm.TrackingPhoneFieldName == "0")
                {
                    ptableEntity.TrackingPhoneFieldName = null;
                }

                if (!string.IsNullOrEmpty(trackingForm.InactiveLocationField) & trackingForm.InactiveLocationField != "0")
                {
                    ptableEntity.InactiveLocationField = trackingForm.InactiveLocationField;
                }
                else if (trackingForm.InactiveLocationField == "0")
                {
                    ptableEntity.InactiveLocationField = null;
                }

                if (!string.IsNullOrEmpty(trackingForm.TrackingMailStopFieldName) & trackingForm.TrackingMailStopFieldName != "0")
                {
                    ptableEntity.TrackingMailStopFieldName = trackingForm.TrackingMailStopFieldName;
                }
                else if (trackingForm.TrackingMailStopFieldName == "0")
                {
                    ptableEntity.TrackingMailStopFieldName = null;
                }

                if (!string.IsNullOrEmpty(trackingForm.ArchiveLocationField) & trackingForm.ArchiveLocationField != "0")
                {
                    ptableEntity.ArchiveLocationField = trackingForm.ArchiveLocationField;
                }
                else if (trackingForm.ArchiveLocationField == "0")
                {
                    ptableEntity.ArchiveLocationField = null;
                }

                if (!string.IsNullOrEmpty(trackingForm.OperatorsIdField) & trackingForm.OperatorsIdField != "0")
                {
                    ptableEntity.OperatorsIdField = trackingForm.OperatorsIdField;
                }
                else if (trackingForm.OperatorsIdField == "0")
                {
                    ptableEntity.OperatorsIdField = null;
                }

                if (!string.IsNullOrEmpty(trackingForm.SignatureRequiredFieldName) & trackingForm.SignatureRequiredFieldName != "0")
                {
                    ptableEntity.SignatureRequiredFieldName = trackingForm.SignatureRequiredFieldName;
                }
                else if (trackingForm.SignatureRequiredFieldName == "0")
                {
                    ptableEntity.SignatureRequiredFieldName = null;
                }

                if (trackingForm.DefaultTrackingId is not null)
                {
                    ptableEntity.DefaultTrackingId = trackingForm.DefaultTrackingId;
                    if (oTrackingTable is not null)
                    {
                        ptableEntity.DefaultTrackingTable = oTrackingTable.TableName.Trim();
                    }
                }
                else
                {
                    ptableEntity.DefaultTrackingTable = null;
                    ptableEntity.DefaultTrackingId = null;
                }
                var bRequestObj = _iSecureObject.All().Where(m => m.Name.Trim().ToLower().Equals(trackingForm.TableName.Trim().ToLower()) & m.SecureObjectTypeID == (int)Enums.SecureObjects.Table).FirstOrDefault();
                var bTransferObj = _iSecureObject.All().Where(m => m.Name.Trim().ToLower().Equals(trackingForm.TableName.Trim().ToLower()) & m.SecureObjectTypeID == (int)Enums.SecureObjects.Table).FirstOrDefault();
                if ((bool)trackingForm.Trackable)
                {
                    this.AddSecureObjectPermission(bTransferObj.SecureObjectID, Enums.PassportPermissions.Transfer);
                }
                else
                {
                    var bTransferPermissionId = this.GetSecureObjPermissionId(bTransferObj.SecureObjectID, Enums.PassportPermissions.Transfer);
                    RemoveSecureObjectPermission(bTransferPermissionId);
                }
                ptableEntity.Trackable = trackingForm.Trackable;
                ptableEntity.AllowBatchRequesting = trackingForm.AllowBatchRequesting;
                if ((bool)trackingForm.AllowBatchRequesting)
                {
                    this.AddSecureObjectPermission(bRequestObj.SecureObjectID, Enums.PassportPermissions.Request);
                }
                else
                {
                    var bRequestPermissionId = this.GetSecureObjPermissionId(bRequestObj.SecureObjectID, Enums.PassportPermissions.Request);
                    RemoveSecureObjectPermission(bRequestPermissionId);
                }
                passport.FillSecurePermissions();
                ptableEntity.AutoAddNotification = trackingForm.AutoAddNotification;
                _iTable.Update(ptableEntity);
                AddTrackableInScanList(ptableEntity);
                warnMsgJSON = VerifyRetentionDispositionTypesForParentAndChildren(ptableEntity.TableId);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRecordUpdatedSuccessfully");
                _iTrackingHistory.CommitTransaction();
                _iTrackingStatus.CommitTransaction();
                _iAssetStatus.CommitTransaction();
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessage;
                _iTrackingHistory.RollBackTransaction();
                _iAssetStatus.RollBackTransaction();
                _iTrackingStatus.RollBackTransaction();
            }

            return Json(new
            {
                warnMsgJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public void AddTrackableInScanList(Table ptableEntity)
        {
            IRepository<ScanList> _iScanList = new Repositories<ScanList>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            try
            {
                _iScanList.BeginTransaction();
                if (ptableEntity.Trackable == true || ptableEntity.TrackingTable > 0)
                {
                    var oScanList = _iScanList.All();
                    string oTableIdFieldName = DatabaseMap.RemoveTableNameFromField(ptableEntity.IdFieldName.Trim().ToLower());
                    bool containScanObj = oScanList.Any(m => m.TableName.Trim().ToLower().Equals(ptableEntity.TableName.Trim().ToLower()) & m.FieldName.Trim().ToLower().Equals(oTableIdFieldName));

                    if (!containScanObj)
                    {
                        var oScanObject = new ScanList();
                        oScanObject.TableName = ptableEntity.TableName.Trim();
                        oScanObject.FieldName = DatabaseMap.RemoveTableNameFromField(ptableEntity.IdFieldName);
                        var conObj = DataServices.DBOpen(ptableEntity, _iDatabas.All());
                        var oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(ptableEntity.TableName, conObj, oScanObject.FieldName);
                        oScanObject.FieldType = Convert.ToInt16(oSchemaColumns[0].DataType);
                        oScanObject.IdMask = ptableEntity.IdMask;
                        oScanObject.IdStripChars = ptableEntity.IdStripChars;
                        if (oScanList.Count() > 0)
                        {
                            oScanObject.ScanOrder = (short?)(oScanList.Count() + 1);
                        }
                        else
                        {
                            oScanObject.ScanOrder = (short?)1;
                        }
                        _iScanList.Add(oScanObject);
                    }
                }
                else
                {
                    var oScanListOfCurrentTable = _iScanList.Where(m => m.TableName.Trim().ToLower().Equals(ptableEntity.TableName.Trim().ToLower()));
                    if (oScanListOfCurrentTable is not null)
                    {
                        _iScanList.DeleteRange(oScanListOfCurrentTable);
                    }
                    var oScanListAll = _iScanList.All().OrderBy(x => x.ScanOrder);
                    int oScanOrder = 1;
                    foreach (ScanList oScanObj in oScanListAll)
                    {
                        oScanObj.ScanOrder = (short?)oScanOrder;
                        oScanOrder = oScanOrder + 1;
                        _iScanList.Update(oScanObj);
                    }
                }
                _iScanList.CommitTransaction();
            }
            catch (Exception)
            {
                _iScanList.RollBackTransaction();
                throw;
            }
        }

        private void AddFieldIfNeeded(string fieldName, Table ptableEntity, string Dtype)
        {
            string sSQLStr;
            bool FieldExist;
            // Dim sADOConnDefault As ADODB.Connection = DataServices.DBOpen() Default Connection
            // local + external db connection'
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            var sADOConn = DataServices.DBOpen();
            if (ptableEntity is not null)
            {
                sADOConn = DataServices.DBOpen(ptableEntity, _iDatabas.All());
            }

            var schemaColumnList = SchemaInfoDetails.GetSchemaInfo(ptableEntity.TableName, sADOConn);
            FieldExist = schemaColumnList.Any(x => (x.ColumnName) == (fieldName));
            if (!FieldExist)
            {
                sSQLStr = "ALTER TABLE [" + ptableEntity.TableName + "]";
                sSQLStr = sSQLStr + " ADD [" + Strings.Trim(fieldName) + "] " + Dtype + " NULL";
                switch (Dtype)
                {
                    case "BIT":
                    case "TINYINT":
                        {
                            sSQLStr = sSQLStr + " DEFAULT 0";
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }
                bool boolSQLVal = DataServices.ProcessADOCommand(ref sSQLStr, sADOConn, false);

            }
            return;
        }

        public IActionResult GetTableEntity(int containerInfo, string tableName, string statusFieldText = "")
        {
            object tableObject = null;
            IRepository<Table> _iTable = new Repositories<Table>();
            do
            {
                try
                {
                    var sADOConnDefault = DataServices.DBOpen();
                    var tableEntity = _iTable.All().OrderBy(m => m.TableId);
                    var schemaColumnList = new List<SchemaColumns>();
                    var oTable = tableEntity.Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = "";

                    if (containerInfo.Equals(0))
                    {
                        if (oTable.TrackingStatusFieldName is not null)
                        {
                            Keys.ErrorType = "r";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlRemove"), oTable.TrackingStatusFieldName);
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(statusFieldText))
                    {
                        bool IsSameOrNot = false;
                        bool exitTry = false;
                        switch (Strings.UCase(Strings.Trim(statusFieldText)))
                        {
                            case "USERNAME":
                            case "DATEDUE":
                            case "ID":
                            case "TRACKEDTABLEID":
                            case "TRACKEDTABLE":
                            case "TRANSACTIONDATETIME":
                            case "PROCESSEDDATETIME":
                            case "OUT":
                            case "TRACKINGADDITIONALFIELD1":
                            case "TRACKINGADDITIONALFIELD2":
                            case "ISACTUALSCAN":
                            case "BATCHID":
                                {
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlSystemFieldNotUsed"), statusFieldText);
                                    Keys.ErrorType = "w";
                                    exitTry = true;
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                        }

                        if (exitTry)
                        {
                            break;
                        }
                        var tsTable = _iTable.All().Where(m => m.TrackingStatusFieldName.Trim().ToLower().Equals(statusFieldText.Trim().ToLower())).FirstOrDefault();
                        if (tsTable is not null)
                        {
                            if (!tsTable.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower()))
                            {
                                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlAlreadyUsedAsTrackingStatusField"), statusFieldText, tsTable.UserName);
                                Keys.ErrorType = "w";
                            }
                        }
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

        [HttpPost]
        public IActionResult GetTrackingDestination([FromForm] string tableName, [FromForm] bool ConfigureTransfer, [FromForm] bool TransferValue,
           [FromForm] bool RequestVal)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                var tableTracking = new Table();
                var tableEntity = new Table();
                //passport = new BaseWebPage();
                // Dim bHasTrackPermission As Boolean
                var bRequestPermission = default(bool);
                var bTransferPermission = default(bool);
                bool bOrderByField;
                string sSQL;
                string sNoRecordMsg;
                string sOrderByFieldName = string.Empty;
                var sADOConn = DataServices.DBOpen();
                ADODB.Recordset rs;
                tableTracking = _iTable.All().Where(m => m.TrackingTable == 1).FirstOrDefault();
                if (!string.IsNullOrEmpty(tableName))
                {
                    tableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                }
                // Get ADO Connection
                if (tableTracking is not null)
                {
                    sADOConn = DataServices.DBOpen(tableTracking, _iDatabas.All());
                }
                if (ConfigureTransfer)
                {
                    bRequestPermission = RequestVal;
                    bTransferPermission = TransferValue;
                }
                else if (passport is not null)
                {
                    bRequestPermission = passport.CheckSetting(tableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Request);
                    bTransferPermission = passport.CheckSetting(tableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Transfer);
                }

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string bRequestPermissionJSON = JsonConvert.SerializeObject(bRequestPermission, Newtonsoft.Json.Formatting.Indented, Setting);
                string bTransferPermissionJSON = JsonConvert.SerializeObject(bTransferPermission, Newtonsoft.Json.Formatting.Indented, Setting);

                if (tableEntity is not null)
                {
                    if (tableEntity.TrackingTable != 1)
                    {
                        if (bTransferPermission)
                        {
                            if (tableTracking is not null)
                            {

                                if (string.IsNullOrEmpty(tableTracking.DescFieldNameOne) & string.IsNullOrEmpty(tableTracking.DescFieldNameTwo))
                                {
                                    bOrderByField = true;
                                }
                                else
                                {
                                    bOrderByField = false;
                                }
                                sSQL = "Select * from [" + tableTracking.TableName + "]";
                                string arglError = "";
                                rs = DataServices.GetADORecordSet(ref sSQL, sADOConn, lError: ref arglError);
                                if (!rs.EOF)
                                {
                                    if (!string.IsNullOrEmpty(tableTracking.TrackingACTIVEFieldName))
                                    {
                                        sSQL = "Select * From [" + tableTracking.TableName + "] Where [" + DatabaseMap.RemoveTableNameFromField(tableTracking.TrackingACTIVEFieldName) + "] <> 0";
                                    }
                                    else
                                    {
                                        sSQL = "Select * from [" + tableTracking.TableName + "]";
                                    }
                                    if (bOrderByField)
                                    {
                                        sOrderByFieldName = DatabaseMap.RemoveTableNameFromField(tableTracking.IdFieldName);
                                    }
                                    else if (!string.IsNullOrEmpty(tableTracking.DescFieldNameOne))
                                    {
                                        sOrderByFieldName = DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameOne);
                                    }
                                    else if (!string.IsNullOrEmpty(tableTracking.DescFieldNameTwo))
                                    {
                                        sOrderByFieldName = DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameTwo);
                                    }
                                    sSQL = sSQL + " Order By [" + sOrderByFieldName + "]";
                                    string arglError1 = "";
                                    rs = DataServices.GetADORecordSet(ref sSQL, sADOConn, lError: ref arglError1);
                                }

                                if (!rs.EOF)
                                {
                                    bool colVisible = false;
                                    bool col1Visible = false;
                                    bool col2Visible = false;
                                    string col1DataField = "";
                                    string col2DataField = "";
                                    var table = new DataTable();
                                    string colDataField = rs.Fields[DatabaseMap.RemoveTableNameFromField(tableTracking.IdFieldName)].Name;
                                    string colDataFieldJSON = "";
                                    string col1DataFieldJSON = "";
                                    string col2DataFieldJSON = "";
                                    colVisible = string.IsNullOrEmpty(col1DataField);

                                    if (tableTracking.IdFieldName is not null & !string.IsNullOrEmpty(tableTracking.IdFieldName))
                                    {
                                        if (!string.IsNullOrEmpty(tableTracking.DescFieldNameOne))
                                        {
                                            col1Visible = !(Strings.StrComp(DatabaseMap.RemoveTableNameFromField(tableTracking.IdFieldName), DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameOne)) == 0);
                                            if (col1Visible)
                                            {
                                                col1DataField = rs.Fields[DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameOne)].Name;
                                                if (!string.IsNullOrEmpty(tableTracking.DescFieldNameTwo))
                                                {
                                                    col2Visible = !(Strings.StrComp(DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameTwo), DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameOne)) == 0);
                                                    if (col2Visible)
                                                    {
                                                        col2Visible = !(Strings.StrComp(DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameTwo), DatabaseMap.RemoveTableNameFromField(tableTracking.IdFieldName)) == 0);
                                                        if (col2Visible)
                                                        {
                                                            col2DataField = rs.Fields[DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameTwo)].Name;
                                                        }
                                                    }
                                                }
                                            }
                                            else if (!string.IsNullOrEmpty(tableTracking.DescFieldNameTwo))
                                            {
                                                col2Visible = !(Strings.StrComp(DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameTwo), DatabaseMap.RemoveTableNameFromField(tableTracking.IdFieldName)) == 0);
                                                if (col2Visible)
                                                {
                                                    col2DataField = rs.Fields[DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameTwo)].Name;
                                                }
                                            }
                                        }
                                        else if (!string.IsNullOrEmpty(tableTracking.DescFieldNameTwo))
                                        {
                                            col2Visible = !(Strings.StrComp(DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameTwo), DatabaseMap.RemoveTableNameFromField(tableTracking.IdFieldName)) == 0);
                                            if (col2Visible)
                                            {
                                                col2DataField = rs.Fields[DatabaseMap.RemoveTableNameFromField(tableTracking.DescFieldNameTwo)].Name;
                                            }
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(colDataField))
                                    {
                                        table.Columns.Add(new DataColumn(colDataField));
                                        if (col1Visible)
                                        {
                                            if (!table.Columns.Contains(col1DataField))
                                            {
                                                table.Columns.Add(new DataColumn(col1DataField));
                                            }
                                        }
                                        if (col2Visible)
                                        {
                                            if (!table.Columns.Contains(col2DataField))
                                            {
                                                table.Columns.Add(new DataColumn(col2DataField));
                                            }
                                        }
                                    }

                                    while (!rs.EOF)
                                    {
                                        var rowObj = table.NewRow();
                                        if (!string.IsNullOrEmpty(colDataField))
                                        {
                                            if ((int)rs.Fields[colDataField].Type == (int)Enums.DataTypeEnum.rmBinary)
                                            {
                                                rowObj[colDataField] = "";
                                            }
                                            else if (rs.Fields[colDataField].Value is DBNull)
                                            {
                                                rowObj[colDataField] = "";
                                            }
                                            else
                                            {
                                                rowObj[colDataField] = rs.Fields[colDataField].Value;
                                            }
                                        }
                                        if (col1Visible)
                                        {
                                            if ((int)rs.Fields[col1DataField].Type == (int)Enums.DataTypeEnum.rmBinary)
                                            {
                                                rowObj[col1DataField] = "";
                                            }
                                            else if (rs.Fields[col1DataField].Value is DBNull)
                                            {
                                                rowObj[col1DataField] = "";
                                            }
                                            else
                                            {
                                                rowObj[col1DataField] = rs.Fields[col1DataField].Value;
                                            }
                                        }
                                        if (col2Visible)
                                        {
                                            if ((int)rs.Fields[col2DataField].Type == (int)Enums.DataTypeEnum.rmBinary)
                                            {
                                                rowObj[col2DataField] = "";
                                            }
                                            else if (rs.Fields[col2DataField] is DBNull)
                                            {
                                                rowObj[col2DataField] = "";
                                            }
                                            else
                                            {
                                                rowObj[col2DataField] = rs.Fields[col2DataField];
                                            }
                                        }
                                        table.Rows.Add(rowObj);
                                        rs.MoveNext();
                                    }

                                    if (!string.IsNullOrEmpty(colDataField))
                                    {
                                        colDataFieldJSON = JsonConvert.SerializeObject(colDataField, Newtonsoft.Json.Formatting.Indented, Setting);

                                    }
                                    if (col1Visible)
                                    {
                                        col1DataFieldJSON = JsonConvert.SerializeObject(col1DataField, Newtonsoft.Json.Formatting.Indented, Setting);

                                    }
                                    if (col2Visible)
                                    {
                                        col2DataFieldJSON = JsonConvert.SerializeObject(col2DataField, Newtonsoft.Json.Formatting.Indented, Setting);

                                    }
                                    // rs.Fields(DatabaseMap.RemoveTableNameFromField(tableTracking.IdFieldName))

                                    string colVisibleJSON = JsonConvert.SerializeObject(colVisible, Newtonsoft.Json.Formatting.Indented, Setting);
                                    string col1VisibleJSON = JsonConvert.SerializeObject(col1Visible, Newtonsoft.Json.Formatting.Indented, Setting);
                                    string col2VisibleJSON = JsonConvert.SerializeObject(col2Visible, Newtonsoft.Json.Formatting.Indented, Setting);
                                    string sRecordJSON = JsonConvert.SerializeObject(table, Newtonsoft.Json.Formatting.Indented, Setting);
                                    string tableObjectJSON = JsonConvert.SerializeObject(tableEntity, Newtonsoft.Json.Formatting.Indented, Setting);
                                    // End If
                                    Keys.ErrorMessage = "";
                                    Keys.ErrorType = "s";
                                    var returnJson = Json(new
                                    {
                                        sRecordJSON,
                                        colVisibleJSON,
                                        col1VisibleJSON,
                                        col2VisibleJSON,
                                        colDataFieldJSON,
                                        col1DataFieldJSON,
                                        col2DataFieldJSON,
                                        bRequestPermissionJSON,
                                        bTransferPermissionJSON,
                                        tableObjectJSON,
                                        errortype = Keys.ErrorType,
                                        message = Keys.ErrorMessage
                                    });
                                    //returnJson.MaxJsonLength = int.MaxValue; // FUS-2304 fixes.
                                    return returnJson;
                                }
                                else
                                {
                                    Keys.ErrorType = "w";
                                    Keys.ErrorMessage = "";
                                    sNoRecordMsg = Languages.get_Translation("msgAdminCtrlLvl1TrackingTblHvNoRecords");
                                    string sNoRecordMsgJSON = JsonConvert.SerializeObject(sNoRecordMsg, Newtonsoft.Json.Formatting.Indented, Setting);
                                    return Json(new
                                    {
                                        sNoRecordMsgJSON,
                                        bRequestPermissionJSON,
                                        bTransferPermissionJSON,
                                        errortype = Keys.ErrorType,
                                        message = Keys.ErrorMessage
                                    });
                                }
                            }
                        }
                    }
                }
                Keys.ErrorMessage = "";
                Keys.ErrorType = "r";
                return Json(new
                {
                    bRequestPermissionJSON,
                    bTransferPermissionJSON,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
            catch (Exception)
            {
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                Keys.ErrorType = "e";
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }

        }
        #endregion

        #region Retention
        // Load View for Tables Retention tab.
        public PartialViewResult LoadTablesRetentionView()
        {
            return PartialView("_TablesRetentionPartial");
        }
        #endregion

        #region File Room Order

        // Load View for Tables Retention tab.
        public PartialViewResult LoadTablesFileRoomOrderView()
        {
            return PartialView("_TableFileRoomOrder");
        }

        // Get the records for File Room Order
        public JsonResult GetListOfFileRoomOrders(string sidx, string sord, int page, int rows, string pTableName)
        {
            IRepository<SLTableFileRoomOrder> _iSLTableFileRoomOrder = new Repositories<SLTableFileRoomOrder>();
            var pFileRoomOrderEntities = from t in _iSLTableFileRoomOrder.All().Where(x => (x.TableName) == (pTableName))
                                         select new { t.Id, t.FieldName, t.StartFromFront, t.StartingPosition, t.NumberofCharacters };

            var jsonData = pFileRoomOrderEntities.GetJsonListForGrid(sord, page, rows, "FieldName");

            return Json(jsonData);
        }

        // Get list of Field Name's
        public IActionResult GetListOfFieldNames(string pTableName)
        {

            var lstFieldNames = new List<string>();
            ADODB.Connection sADOConn;
            IRepository<Table> _iTable = new Repositories<Table>();
            // Dim sADOConn = DataServices.DBOpen(Enums.eConnectionType.conDefault, Nothing)
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableName.Trim().ToLower())).FirstOrDefault();
            sADOConn = DataServices.DBOpen(oTables, _iDatabas.All());
            var schemaColumnList = SchemaInfoDetails.GetSchemaInfo(pTableName, sADOConn);

            foreach (SchemaColumns schemaColumnObj in schemaColumnList)
            {
                if (!SchemaInfoDetails.IsSystemField(schemaColumnObj.ColumnName))
                {
                    if (SchemaInfoDetails.IsAStringType(schemaColumnObj.DataType))
                    {
                        lstFieldNames.Add(schemaColumnObj.ColumnName + " (String: padded with spaces)");
                    }
                    else if (SchemaInfoDetails.IsADateType(schemaColumnObj.DataType))
                    {
                        lstFieldNames.Add(schemaColumnObj.ColumnName + " (Date: mm/dd/yyyy)");
                    }
                    else if (SchemaInfoDetails.IsANumericType(schemaColumnObj.DataType))
                    {
                        lstFieldNames.Add(schemaColumnObj.ColumnName + " (Numeric: padded with leading zeros)");
                    }
                }
            }

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

            string lstFieldNamesObj = JsonConvert.SerializeObject(lstFieldNames, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(new { lstFieldNames = lstFieldNamesObj });

        }

        // Get the details for Edit Operation of File Room Order.
        [HttpPost]
        public IActionResult EditFileRoomOrderRecord([FromForm] string[] pRowSelected, [FromForm] int pRecordId)
        {

            if (pRowSelected is null)
            {
                return Json(new { success = false });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { success = false });
            }
            IRepository<SLTableFileRoomOrder> _iSLTableFileRoomOrder = new Repositories<SLTableFileRoomOrder>();
            var pFileRoomOrderEntity = _iSLTableFileRoomOrder.All().Where(x => x.Id == pRecordId).FirstOrDefault();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(pFileRoomOrderEntity, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        // Save the file room order record.
        [HttpPost]
        public IActionResult SetFileRoomOrderRecord([FromForm] SLTableFileRoomOrder pFileRoomOrder, string pTableName, [FromForm] bool pStartFromFront)
        {
            int pFieldLength = 0;
            string ErrMsg = "";
            bool IsDateField = false;
            bool ErrStatus = false;
            ADODB.Connection sADOConn;
            IRepository<SLTableFileRoomOrder> _iSLTableFileRoomOrder = new Repositories<SLTableFileRoomOrder>();

            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(pTableName.Trim().ToLower())).FirstOrDefault();
                sADOConn = DataServices.DBOpen(oTables, _iDatabas.All());

                var schemaColumnList = SchemaInfoDetails.GetSchemaInfo(pTableName, sADOConn, pFileRoomOrder.FieldName);

                if (schemaColumnList.Count > 0)
                {
                    if (schemaColumnList[0].IsADate)
                    {
                        pFieldLength = 10;
                        IsDateField = true;
                    }
                    else if (schemaColumnList[0].IsString)
                    {
                        if (schemaColumnList[0].CharacterMaxLength <= 0 | schemaColumnList[0].CharacterMaxLength >= 2000000)
                        {
                            pFieldLength = 30;
                        }
                        else
                        {
                            pFieldLength = schemaColumnList[0].CharacterMaxLength;
                        }
                    }
                    else
                    {
                        pFieldLength = 30;
                    }
                }
                pFileRoomOrder.StartFromFront = pStartFromFront;
                if (pFileRoomOrder.StartingPosition == null)
                {
                    ErrMsg = string.Format(Languages.get_Translation("msgAdminCtrlStartingPositionMustBe"), Strings.Format(pFieldLength));
                    ErrStatus = true;
                }
                if (pFileRoomOrder.NumberofCharacters == null & string.IsNullOrEmpty(ErrMsg))
                {
                    ErrMsg = string.Format(Languages.get_Translation("msgAdminCtrlNoOfCharMustBe"), Strings.Format(pFieldLength));
                    ErrStatus = true;
                }

                if (!(pFileRoomOrder.StartingPosition == null) & !(pFileRoomOrder.NumberofCharacters == null) & string.IsNullOrEmpty(ErrMsg))
                {
                    if (pFileRoomOrder.StartingPosition < 1 || pFileRoomOrder.StartingPosition > pFieldLength)
                    {
                        ErrMsg = string.Format(Languages.get_Translation("msgAdminCtrlStartingPositionMustBe"), Strings.Format(pFieldLength));
                        ErrStatus = true;
                    }

                    if (pFileRoomOrder.NumberofCharacters < 1 || pFileRoomOrder.NumberofCharacters > pFieldLength)
                    {
                        ErrMsg = string.Format(Languages.get_Translation("msgAdminCtrlNoOfCharMustBe"), Strings.Format(pFieldLength));
                        ErrStatus = true;
                    }
                }

                if (pFileRoomOrder.StartFromFront == true)
                {
                    if (((pFileRoomOrder.StartingPosition + pFileRoomOrder.NumberofCharacters) > pFieldLength + 1) && (ErrMsg == ""))
                    {
                        ErrMsg = Languages.get_Translation("msgAdminCtrlNoOfCharMustBLessEqualStartPosition");
                        ErrStatus = true;
                    }
                }
                if ((pFileRoomOrder.StartingPosition > pFieldLength) && ErrMsg == "")
                {
                    ErrMsg = string.Format(Languages.get_Translation("msgAdminCtrlNoOfCharPlusStartingPosition"), Strings.Format(pFieldLength));
                    ErrStatus = true;
                }

                // Validation for Date Field
                if (IsDateField == true)
                {
                    if ((pFileRoomOrder.StartingPosition > pFieldLength) && ErrMsg == "")
                    {
                        ErrMsg = string.Format(Languages.get_Translation("msgAdminCtrlStartPosNotExceed"), Strings.Format(pFieldLength));
                        ErrStatus = true;
                    }

                    if (pFileRoomOrder.StartFromFront == false)
                    {
                        if ((pFileRoomOrder.StartingPosition + pFileRoomOrder.NumberofCharacters) > (pFieldLength + 1) && ErrMsg == "")
                        {
                            ErrMsg = string.Format(Languages.get_Translation("msgAdminCtrlStartPosPlusLenNoExceed"), Strings.Format(pFieldLength + 1));
                            ErrStatus = true;
                        }
                    }
                }

                // Save File Room Order Record.
                if (!(ErrStatus == true))
                {
                    if (pFileRoomOrder.Id > 0)
                    {
                        _iSLTableFileRoomOrder.BeginTransaction();

                        pFileRoomOrder.TableName = pTableName;
                        _iSLTableFileRoomOrder.Update(pFileRoomOrder);

                        _iSLTableFileRoomOrder.CommitTransaction();

                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Keys.SaveSuccessMessage();
                    }
                    else
                    {
                        pFileRoomOrder.TableName = pTableName;
                        _iSLTableFileRoomOrder.Add(pFileRoomOrder);

                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Keys.SaveSuccessMessage();
                    }
                }
                else
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = ErrMsg;
                }
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Remove the file room order record.
        [HttpPost]
        public IActionResult RemoveFileRoomOrderRecord([FromForm] string pRowSelected, [FromForm] int pRecordId)
        {
            IRepository<SLTableFileRoomOrder> _iSLTableFileRoomOrder = new Repositories<SLTableFileRoomOrder>();
            try
            {
                if (pRowSelected is null)
                {
                    return Json(new { success = false });
                }
                if (pRowSelected.Length == 0)
                {
                    return Json(new { success = false });
                }

                var pSLTableFileRoomOrderEntity = _iSLTableFileRoomOrder.All().Where(x => x.Id == pRecordId).FirstOrDefault();
                _iSLTableFileRoomOrder.Delete(pSLTableFileRoomOrderEntity);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Keys.DeleteSuccessMessage();
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }


            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        #endregion

        #region Advanced
        public PartialViewResult LoadAdvancedTab()
        {
            return PartialView("_TableAdvancedPartial");
        }

        [HttpPost]
        public IActionResult GetAdvanceDetails([FromForm] string tableName)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                var tableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                if (tableEntity.RecordManageMgmtType is null)
                {
                    tableEntity.RecordManageMgmtType = 0;
                }
                var relationshipEntity = _iRelationShip.All().Where(m => m.LowerTableName.Trim().ToLower().Equals(tableName.Trim().ToLower()));
                bool flag = false;
                var parentFolderList = LoadAdvancedLevelList(tableEntity.TableName, relationshipEntity);
                var parentDocList = LoadAdvancedLevelList(tableEntity.TableName, relationshipEntity);
                if (parentFolderList.Count == 0)
                {
                    flag = false;
                }
                else
                {
                    flag = true;
                }
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string tableEntityJSON = JsonConvert.SerializeObject(tableEntity, Newtonsoft.Json.Formatting.Indented, Setting);
                string parentFolderListJSON = JsonConvert.SerializeObject(parentFolderList, Newtonsoft.Json.Formatting.Indented, Setting);
                string parentDocListJSON = JsonConvert.SerializeObject(parentDocList, Newtonsoft.Json.Formatting.Indented, Setting);
                string flagJSON = JsonConvert.SerializeObject(flag, Newtonsoft.Json.Formatting.Indented, Setting);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlAllDataGetSuccessfully");
                return Json(new
                {
                    tableEntityJSON,
                    parentFolderListJSON,
                    parentDocListJSON,
                    flagJSON,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }

        }

        public List<KeyValuePair<string, string>> LoadAdvancedLevelList(string tableName, IQueryable<RelationShip> relationshipEntity)
        {
            var parentDDList = new List<KeyValuePair<string, string>>();
            IRepository<Table> _iTable = new Repositories<Table>();
            var tableEntity = _iTable.All().OrderBy(m => m.TableId);
            if (relationshipEntity is not null & tableEntity is not null)
            {
                foreach (RelationShip relationOBj in relationshipEntity)
                {
                    foreach (Table tableObj in tableEntity)
                    {
                        if (relationOBj.UpperTableName.Trim().ToLower().Equals(tableObj.TableName.Trim().ToLower()))
                        {
                            parentDDList.Add(new KeyValuePair<string, string>(tableObj.TableName, tableObj.UserName));
                        }
                    }
                }
            }
            return parentDDList;
        }

        public IActionResult SetAdvanceDetails(Table advanceform)
        {
            string warnMsgJSON = "";
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                var tableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(advanceform.TableName.Trim().ToLower())).FirstOrDefault();
                if (advanceform.RecordManageMgmtType is null)
                {
                    tableEntity.RecordManageMgmtType = 0;
                }
                else
                {
                    tableEntity.RecordManageMgmtType = advanceform.RecordManageMgmtType;
                }
                if (advanceform.RecordManageMgmtType is not null)
                {
                    if (!(advanceform.ParentFolderTableName is null))
                    {
                        tableEntity.ParentDocTypeTableName = null;
                        tableEntity.ParentFolderTableName = null;
                    }
                }
                if (advanceform.ParentDocTypeTableName is not null)
                {
                    tableEntity.ParentDocTypeTableName = advanceform.ParentDocTypeTableName;
                }
                if (advanceform.ParentFolderTableName is not null)
                {
                    tableEntity.ParentFolderTableName = advanceform.ParentFolderTableName;
                }
                _iTable.Update(tableEntity);
                warnMsgJSON = VerifyRetentionDispositionTypesForParentAndChildren(tableEntity.TableId);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRecordUpdatedSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new
            {
                warnMsgJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });

        }

        [HttpPost]
        public IActionResult CheckParentForder([FromForm] string parentFolderVar, [FromForm] string selectedTableVar)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                var tableEntity = _iTable.All().OrderBy(m => m.TableId);
                bool flagEqual = false;
                string parentTableName = _iTable.All().Where(m => m.UserName.Trim().ToLower().Equals(parentFolderVar.Trim().ToLower())).FirstOrDefault().TableName;
                string ConfigTable = null;
                if (tableEntity is not null)
                {
                    foreach (Table tableObj in tableEntity)
                    {
                        if (tableObj.ParentFolderTableName is not null)
                        {
                            if (tableObj.ParentFolderTableName.Trim().ToLower().Equals(parentTableName.Trim().ToLower()))
                            {
                                if (!tableObj.TableName.Trim().ToLower().Equals(selectedTableVar.Trim().ToLower()))
                                {
                                    flagEqual = true;
                                    ConfigTable = tableObj.UserName;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (flagEqual == true)
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlIsAlreadyConfigured"), parentTableName, ConfigTable);
                }
                else
                {
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = "";
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();

            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        #endregion

        #endregion

        #region Views

        public PartialViewResult LoadViewsList()
        {
            return PartialView("_ViewsListPartial");
        }

        public PartialViewResult LoadViewsSettings(int pViewId, string sAction)
        {
            var oViews = new View();
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            // Dim oViewFilterList As New List(Of ViewFilter)

            httpContext.Session.SetString("tmpViewId", JsonConvert.SerializeObject(pViewId));

            if (!string.IsNullOrEmpty(sAction) && sAction.Trim().ToUpper().Equals("E"))
            {
                oViews = _iView.All().Where(x => x.Id == pViewId).FirstOrDefault();
            }
            else
            {
                var tempViews = _iTable.All().Where(x => x.TableId == pViewId).FirstOrDefault();

                var pViewNameObject = _iView.All().Where(x => x.TableName.Trim().ToLower().Equals(tempViews.TableName.Trim().ToLower()) && x.ViewName.Trim().ToLower().Contains(("All " + tempViews.UserName).Trim().ToLower()));
                int MaxCount = 1000;
                int NextCount = 1;


                int intMyInteger1;
                for (int index = 1; index <= 1000; index++)
                {
                    bool status = false;
                    foreach (View item in pViewNameObject)
                    {
                        var items = item.ViewName.Split(' ');
                        int.TryParse(items[items.Count() - 1], out intMyInteger1);
                        if (index == intMyInteger1)
                        {
                            status = true;

                            break;
                        }
                    }
                    if (status == false)
                    {
                        NextCount = index;
                        break;
                    }
                }

                int intMyInteger;
                foreach (View item in pViewNameObject)
                {
                    var items = item.ViewName.Split(' ');
                    int.TryParse(items[items.Count() - 1], out intMyInteger);
                    if (intMyInteger > MaxCount)
                    {
                        MaxCount = intMyInteger;
                        if (MaxCount == NextCount)
                        {
                            NextCount = NextCount + 1;
                        }

                    }
                }
                oViews.ViewName = Convert.ToString(Operators.AddObject("All " + tempViews.UserName + " ", Interaction.IIf(NextCount == 0, "", NextCount.ToString())));
                oViews.SearchableView = true;
                oViews.SQLStatement = "SELECT * FROM [" + tempViews.TableName + "]";
                oViews.TableName = tempViews.TableName;
                oViews.MaxRecsPerFetch = oViews.MaxRecsPerFetch;
                pViewId = 0;
            }

            httpContext.Session.SetString("viewTableName", oViews.TableName);

            var oViewsCustModel = new ViewsCustomModel();
            oViewsCustModel.ViewsModel = oViews;
            var GridColumnEntities = ViewModel.GetColumnsData(_iView.All(), _iViewColumn.All(), _iDatabas.All(), _iTable.All(), oViews.Id, sAction);
            ViewBag.ViewColumnList = GridColumnEntities.Where(x => x.AutoInc == true).ToList().CreateSelectListFromList("ColumnId", "ColumnName", default(int?));
            TempData.Clear();
            //TempData["ColumnsData"] =  GridColumnEntities;
            TempData.Set<List<GridColumns>>("ColumnsData", GridColumnEntities);

            ViewBag.ColumnGrdCount = GridColumnEntities.Count.ToString();

            var olViewColumns = _iViewColumn.All().Where(x => x.ViewsId == pViewId).OrderBy(x => x.ColumnNum).ToList();

            if (pViewId != 0)
            {
                if (olViewColumns is not null)
                {
                    if (olViewColumns.Count == 0)
                    {
                        // Taken columns from AltView, If view does not have any columns - FUS- 5704
                        var oAltView = _iView.All().Where(x => x.Id == oViews.AltViewId).FirstOrDefault();
                        olViewColumns = _iViewColumn.All().Where(x => x.ViewsId == oAltView.Id).OrderBy(x => x.ColumnNum).ToList();
                    }
                }
            }

            //TempData["TempViewColumns_" + pViewId] = olViewColumns;
            TempData.Set<List<ViewColumn>>("TempViewColumns_" + pViewId, olViewColumns);
            TempData.Remove("ViewFilterUpdate");
            return PartialView("_ViewsSettingsPartial", oViewsCustModel);
        }

        public ContentResult ViewTreePartial(string root)
        {
            IRepository<vwTablesAll> _ivwTablesAll = new Repositories<vwTablesAll>();
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<View> _iView = new Repositories<View>();
            var lTableEntities = _iTable.All().OrderBy(m => m.UserName);
            var lAllTables = _ivwTablesAll.All().Select(x => x.TABLE_NAME).ToList();
            lTableEntities = (IOrderedQueryable<Table>)lTableEntities.Where(x => lAllTables.Contains(x.TableName));
            var lViewsEntities = _iView.All();

            // Dim treeView = ViewModel.GetBindTreeControlView(lTableEntities, lViewsEntities)
            string treeView = ViewModel.GetBindViewMenus(root, lTableEntities, lViewsEntities, passport);

            return this.Content(treeView);
        }


        public JsonResult GetViewColumnsList(string sidx, string sord, int page, int rows, int intViewsId, string sAction)
        {
            var GridColumnEntities = new List<GridColumns>();
            GridColumnEntities = (TempData.GetPeek<List<GridColumns>>("ColumnsData")).OrderBy(x => x.ColumnId).ToList();
            ViewBag.ColumnGrdCount = GridColumnEntities.Count.ToString();
            var jsonData = GridColumnEntities.GetJsonListForGrid1(sord, page, rows);
            return Json(jsonData);
        }

        public JsonResult GetViewsRelatedData(string sTableName, int pViewId)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<ViewFilter> _iViewFilter = new Repositories<ViewFilter>();
            IRepository<SLTableFileRoomOrder> _iSLTableFileRoomOrder = new Repositories<SLTableFileRoomOrder>();
            var oTables = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())).FirstOrDefault();
            var oViews = _iView.All().Where(x => x.Id == pViewId).FirstOrDefault();
            bool bTaskList = false;
            bool bInFileRoomOrder = false;
            bool bIncludeTrackingLocation = false;
            int maxRecsPerFetch;

            if (oViews is not null)
            {
                bTaskList = Convert.ToBoolean(Interaction.IIf(oViews.InTaskList is null, false, oViews.InTaskList));
                bInFileRoomOrder = Convert.ToBoolean(Interaction.IIf(oViews.IncludeFileRoomOrder is null, false, oViews.IncludeFileRoomOrder));
                bIncludeTrackingLocation = Convert.ToBoolean(Interaction.IIf(oViews.IncludeTrackingLocation is null, false, oViews.IncludeTrackingLocation));
                maxRecsPerFetch = (int)oViews.MaxRecsPerFetch;
            }
            else // if no ViewId passed in then we get the default from the model
            {
                oViews = new View();
                maxRecsPerFetch = (int)oViews.MaxRecsPerFetch;
                oViews = null;
            }

            var oViewFilter = _iViewFilter.All().Where(x => x.ViewsId == pViewId);
            int SLTableFileRoomOrderCount = 0;
            int ViewFilterCount = 0;
            bool bTrackable = false;
            //var myBasePage = new BaseWebPage();
            bool mbCanModifyColumns = true;
            bool btnColumnAdd = true;
            bool bSearchableView = false;
            var ShouldEnableMoveFilter = default(bool);
            // Dim BaseWebPage = New BaseWebPage()

            var TempViewFilterList = new List<ViewFilter>();
            if (TempData.ContainsKey("ViewFilterUpdate"))
            {
                TempData.Remove("ViewFilterUpdate");
            }
            TempViewFilterList = oViewFilter.ToList();
            //TempData["ViewFilterUpdate"] = TempViewFilterList;
            TempData.Set<List<ViewFilter>>("ViewFilterUpdate", TempViewFilterList);

            try
            {
                if (oTables is not null)
                {
                    bTrackable = passport.CheckPermission(oTables.TableName.Trim(), (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Transfer);
                    // bTrackable = oTables.Trackable
                    var oSLTableFileRoomOrder = _iSLTableFileRoomOrder.All().Where(x => x.TableName.Trim().ToLower().Equals(oTables.TableName.Trim().ToLower()));
                    if (oSLTableFileRoomOrder is not null)
                    {
                        SLTableFileRoomOrderCount = oSLTableFileRoomOrder.Count();
                    }
                    if (oViews is not null)
                    {
                        if (oViews.AltViewId > 0)
                        {
                            mbCanModifyColumns = false;
                            var oAltView = _iView.All().Where(x => x.AltViewId == oViews.AltViewId).FirstOrDefault();
                            if (oAltView is not null)
                            {
                                mbCanModifyColumns = passport.CheckPermission(oAltView.ViewName, Smead.Security.SecureObject.SecureObjectType.View, Permissions.Permission.Configure);
                                btnColumnAdd = mbCanModifyColumns;
                                // lblColumnsPermission.Caption = Replace$(lblColumnsPermission.Caption, "???", vbCrLf & vbCrLf & oAltView.Id & ":  " & oAltView.ViewName)
                                oAltView = null;

                            }
                        }
                        bSearchableView = Convert.ToBoolean(Interaction.IIf(oViews.SearchableView is null, false, oViews.SearchableView));
                        if (oViewFilter is not null)
                        {
                            if (oViewFilter.Count() != 0)
                            {
                                if (oViewFilter.Any(m => m.Active == true))
                                {
                                    ShouldEnableMoveFilter = true;
                                }
                                else
                                {
                                    ShouldEnableMoveFilter = false;
                                }
                            }
                            else
                            {
                                ShouldEnableMoveFilter = false;
                            }

                        }
                    }

                }
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Keys.SaveSuccessMessage();
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message.ToString();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                errorMessage = Keys.ErrorMessage,
                btnColumnAdd,
                sltableFileRoomOrderCount = SLTableFileRoomOrderCount,
                ShouldEnableMoveFilter,
                bSearchableView,
                bTrackable,
                bTaskList,
                bInFileRoomOrder,
                bIncludeTrackingLocation,
                MaxRecsPerFetch = maxRecsPerFetch
            });

        }

        public object UpdateGridSortOrder(List<int> ids, string sGridName)
        {
            IRepository<ScanList> _iScanList = new Repositories<ScanList>();
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            IRepository<ViewFilter> _iViewFilter = new Repositories<ViewFilter>();
            try
            {
                if (ids.Count > 0)
                {
                    switch (sGridName.Trim().ToUpper())
                    {
                        case "GRDVIEWCOLUMNS":
                            {
                                // Dim lViewColumn = _iViewColumn.All()
                                int iViewColumnId = 0;
                                int iViewId = Convert.ToInt32(httpContext.Session.GetString("tmpViewId"));
                                // Dim iViewId As Integer = 0
                                // Dim oViewColumns = Nothing
                                // For i As Integer = 0 To ids.Count - 1
                                // iViewColumnId = ids(i)
                                // oViewColumns = lViewColumn.Where(Function(x) x.Id = iViewColumnId).FirstOrDefault()
                                // If (oViewColumns IsNot Nothing) Then
                                // Exit For
                                // End If
                                // Next

                                // iViewId = oViewColumns.ViewsId
                                var lstViewColumn = new List<ViewColumn>();
                                //lstViewColumn = (List<ViewColumn>)this.TempData.Peek("TempViewColumns_" + iViewId);   // does not work in core
                                lstViewColumn = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + iViewId);        

                                var dictionary = new Dictionary<int, int>();
                                for (int i = 0, loopTo = ids.Count - 1; i <= loopTo; i++)
                                {
                                    iViewColumnId = ids[i];
                                    var oViewColumnsDictionary = lstViewColumn.Where(x => x.Id == iViewColumnId).FirstOrDefault();
                                    if (iViewColumnId > 0)
                                    {
                                        dictionary.Add(iViewColumnId, (int)oViewColumnsDictionary.ColumnNum);
                                    }
                                }
                                httpContext.Session.SetString("OrgViewColumnIds", JsonConvert.SerializeObject(dictionary));

                                var lstTEMPViewColumns = new List<ViewColumn>();
                                //lstTEMPViewColumns = (List<ViewColumn>)this.TempData.Peek("TempViewColumns_" + iViewId);   // does not work in asp.net core 
                                lstTEMPViewColumns = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + iViewId);   
                                var dicUpdatedColNums = new Dictionary<int, int>();
                                for (int i = 0, loopTo1 = ids.Count - 1; i <= loopTo1; i++)
                                {
                                    iViewColumnId = ids[i];
                                    var pViewColumns = lstTEMPViewColumns.Where(x => x.Id == iViewColumnId).FirstOrDefault();
                                    if (pViewColumns is not null)
                                    {
                                        pViewColumns.ColumnNum = (short?)i;
                                        dicUpdatedColNums.Add(pViewColumns.Id, i);
                                    }
                                }
                                httpContext.Session.SetString("UpViewColumnIds", JsonConvert.SerializeObject(dicUpdatedColNums));

                                SetLookupId(iViewId);
                                // TempData("TempViewColumns_" & orgViewId) = lstTEMPViewColumns

                                var lViewFilter = _iViewFilter.All();

                                var lViewFiltersUpdated = new List<ViewFilter>();

                                foreach (ViewFilter pViewFilter in lViewFilter.Where(x => x.ViewsId == iViewId).ToList())
                                {
                                    int iViewColumn = dictionary.Where(x => x.Value == pViewFilter.ColumnNum).FirstOrDefault().Key;
                                    var op = lstTEMPViewColumns.Where(x => x.Id == iViewColumn).FirstOrDefault();
                                    if (pViewFilter.ColumnNum is var arg223 && op.ColumnNum is { } arg224 && arg223.HasValue && arg223 != arg224)
                                    {
                                        pViewFilter.ColumnNum = op.ColumnNum;
                                        // _iViewFilter.Update(pViewFilter)
                                    }
                                    lViewFiltersUpdated.Add(pViewFilter);
                                }

                                //this.TempData["ViewFilterUpdate"] = lViewFiltersUpdated;
                                TempData.Set<List<ViewFilter>>("ViewFilterUpdate", lViewFiltersUpdated);

                                break;
                            }
                        case "GRDBARCODE":
                            {

                                var lScanList = _iScanList.All();
                                int iScanListID = 0;

                                ScanList pScanList;
                                for (int i = 0, loopTo2 = ids.Count - 1; i <= loopTo2; i++)
                                {
                                    iScanListID = ids[i];
                                    pScanList = _iScanList.All().Where(x => x.Id == iScanListID).FirstOrDefault();
                                    if (pScanList is not null)
                                    {
                                        pScanList.ScanOrder = ((short?)(i + 1));
                                    }
                                    _iScanList.Update(pScanList);
                                }

                                break;
                            }

                        default:
                            {
                                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRecordNotFound");
                                break;
                            }
                    }

                }
            }
            catch (Exception ex)
            {
                Keys.ErrorMessage = ex.Message.ToString();
            }

            return Json(Keys.ErrorMessage);
        }

        [HttpPost]
        public IActionResult ValidateSqlStatement(ViewsCustomModel lViewsCustomModelEntites, bool pIncludeFileRoomOrder, bool pIncludeTrackingLocation, bool pInTaskList)
        {
            int lError = 0;
            string sReturnMessage = "";
            var oTable = new Table();
            string sSql = string.Empty;
            string sSQLWithTL = string.Empty;
            var pViewEntity = new View();
            var lViewsData = lViewsCustomModelEntites.ViewsModel;
            int viewIdVar = lViewsData.Id;
            string ViewIdJSON = "";
            string SendMessage = string.Empty;
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            oTable = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(lViewsData.TableName.Trim().ToLower())).FirstOrDefault();
            if (!string.IsNullOrEmpty(lViewsData.SQLStatement))
            {
                sSql = DataServices.NormalizeString(lViewsData.SQLStatement);
                sSql = DataServices.InjectWhereIntoSQL(sSql, "0=1");
                var sADOConn = DataServices.DBOpen();
                sADOConn = DataServices.DBOpen(oTable, _iDatabas.All());

                var rsADO = new ADODB.Recordset();
                rsADO = DataServices.GetADORecordset(sSql, oTable, _iDatabas.All() ,bReturnError: true,lError: ref lError,sErrorMessage: ref sReturnMessage);
                if (lError == 0)
                {
                    Keys.ErrorType = "s";
                }
                else
                {
                    Keys.ErrorType = "w";
                    if (Strings.InStr(1, sSql, " TOP ", Constants.vbTextCompare) > 0L)
                    {
                        SendMessage = string.Format(Languages.get_Translation("msgAdminCtrlLimitingSpecificNoOfRec"), Constants.vbCrLf);
                    }
                    else
                    {
                        SendMessage = string.Format("{0} {1} {1} {2}", Languages.get_Translation("msgAdminCtrlSQLStatementInValid"), Constants.vbCrLf, sReturnMessage);
                    }
                }

                if (string.IsNullOrEmpty(SendMessage) & pIncludeTrackingLocation == true)
                {
                    sSql = DataServices.NormalizeString(lViewsData.SQLStatement);
                    sSQLWithTL = TrackingServices.BuildTrackingLocationSQL(_iTable.All(), _iDatabas.All(), sSql, ref oTable);
                    if (Strings.StrComp(sSql, sSQLWithTL, Constants.vbTextCompare) == 0)
                    {
                        Keys.ErrorType = "w";
                        SendMessage = Languages.get_Translation("msgAdminCtrlViewContainsSQLStatement");
                    }
                }
                Keys.ErrorMessage = SendMessage;
            }
            else
            {
                Keys.ErrorType = "s";
            }
            return Json(new
            {
                ViewIdJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });

        }

        // <HttpPost> _
        // Public Function ValidateSqlStatement(lViewsCustomModelEntites As ViewsCustomModel) As IActionResult

        // Dim lError As String = ""
        // Dim sReturnMessage = ""
        // Dim oTable = New Table()
        // Dim sSql As String
        // lError = 0

        // Dim pViewEntity = New View()
        // Dim lViewsData As View = DirectCast(lViewsCustomModelEntites.ViewsModel, View)
        // Dim viewIdVar = lViewsData.Id
        // Dim ViewIdJSON As String = ""
        // oTable = _iTable.All().Where(Function(x) x.TableName.Trim().ToLower().Equals(lViewsData.TableName.Trim().ToLower())).FirstOrDefault()


        // If (Not lViewsData.SQLStatement() Is Nothing) Then

        // Dim sADOConn As ADODB.Connection = DataServices.DBOpen()
        // sADOConn = DataServices.DBOpen(oTable, _iDatabas.All())

        // Dim rsADO As New ADODB.Recordset
        // sSql = lViewsData.SQLStatement()
        // rsADO = DataServices.GetADORecordset(sSql, oTable, _iDatabas.All(), , , , , , True, lError, sReturnMessage)
        // If (Not lError.ToString().Equals("0")) Then
        // Keys.ErrorType = "e"
        // Else
        // Keys.ErrorType = "s"
        // End If
        // End If
        // Keys.ErrorMessage = sReturnMessage
        // Return Json(New With { _
        // Key .ViewIdJSON = ViewIdJSON, _
        // Key .errortype = Keys.ErrorType, _
        // Key .message = Keys.ErrorMessage _
        // })

        // End Function

        public void SetLookupId(int oViewId)
        {
            try
            {
                //List<ViewColumn> lViewColumns = (List<ViewColumn>)this.TempData.Peek("TempViewColumns_" + oViewId);
                List<ViewColumn> lViewColumns = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + oViewId);

                lViewColumns = lViewColumns.OrderBy(m => m.ColumnNum).ToList();
                var ViewColumnObj = new ViewColumn();
                // Dim pViewColumnList = _iViewColumn.All().Where(Function(x) x.ViewsId = oViewId).ToList() '.OrderBy(Function(x) x.ColumnNum)

                var tempViewColumns = new List<ViewColumn>();
                var dictionary = ContextService.FromLegacyCookieStringToDicInt(httpContext.Session.GetString("OrgViewColumnIds"));
                var dicUpdatedColNums = ContextService.FromLegacyCookieStringToDicInt(httpContext.Session.GetString("UpViewColumnIds"));
                bool bLookup0Updated = false;
                if (lViewColumns is not null)
                {
                    foreach (ViewColumn pViewColObj in lViewColumns)
                    {
                        if (lViewColumns.Any(m => m.Id == pViewColObj.Id))
                        {
                            var tempViewCol = lViewColumns.Where(m => m.Id == pViewColObj.Id).FirstOrDefault();

                            // ' Update Lookup Id values
                            if (dictionary is not null && dicUpdatedColNums is not null)
                            {
                                if ((pViewColObj.LookupIdCol > 0) || (pViewColObj.LookupIdCol == 0) && (pViewColObj.LookupType == 1))
                                {
                                    var pOldLookupIdCol = pViewColObj.LookupIdCol;
                                    int pOldViewColumnId = dictionary.Where(x => x.Value == pOldLookupIdCol).FirstOrDefault().Key;
                                    int pViewColumnsXYZ = dicUpdatedColNums.Where(x => x.Key == pOldViewColumnId).FirstOrDefault().Value;
                                    // Dim pViewColumnsXYZ = lViewColumnsOld.Where(Function(m) m.Id = pOldViewColumnId).FirstOrDefault()
                                    tempViewCol.LookupIdCol = (short?)pViewColumnsXYZ;
                                }
                            }
                            if (tempViewCol.LookupIdCol is null)
                            {
                                tempViewCol.LookupIdCol = pViewColObj.LookupIdCol;
                            }
                            tempViewColumns.Add(tempViewCol);
                        }
                    }
                    this.TempData["TempViewColumns_" + oViewId] = tempViewColumns;
                }
            }
            catch (Exception)
            {

            }
        }

        [HttpPost]
        public IActionResult SetViewsDetails(ViewsCustomModel lViewsCustomModelEntites, bool pIncludeFileRoomOrder, bool pIncludeTrackingLocation, bool pInTaskList)
        {
            string ViewIdJSON = "";
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            IRepository<ViewFilter> _iViewFilter = new Repositories<ViewFilter>();
            do
            {
                try
                {
                    _iView.BeginTransaction();
                    _iViewColumn.BeginTransaction();
                    _iViewFilter.BeginTransaction();
                    var pViewEntity = new View();
                    var lViewsData = lViewsCustomModelEntites.ViewsModel;
                    lViewsData.IncludeFileRoomOrder = pIncludeFileRoomOrder;
                    lViewsData.IncludeTrackingLocation = pIncludeTrackingLocation;
                    lViewsData.InTaskList = pInTaskList;
                    int viewIdVar = lViewsData.Id;
                    string oldViewName = "";
                    if (lViewsData.Id > 0)
                    {
                        var viewEn = _iView.All().Where(x => x.Id == lViewsData.Id).FirstOrDefault();
                        pViewEntity = viewEn;
                        oldViewName = pViewEntity.ViewName;
                        ViewModel.CreateViewsEntity(lViewsData, ref pViewEntity);
                    }
                    else
                    {
                        lViewsData.ViewOrder = _iView.All().Where(x => x.TableName.Trim().ToLower().Equals(lViewsData.TableName.Trim().ToLower())).Max(x => x.ViewOrder) + 1;
                        ViewModel.CreateViewsEntity(lViewsData, ref pViewEntity);
                    }

                    var lViewFiltersData = new List<ViewFilter>();
                    lViewFiltersData = lViewsCustomModelEntites.ViewFilterList;
                    int lSecureObjectId;
                    // var BaseWebPage = new BaseWebPage();
                    var oSecureObject = new Smead.Security.SecureObject(passport);
                    List<ViewColumn> lViewColumns = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + lViewsData.Id); //(List<ViewColumn>)this.TempData.Peek("TempViewColumns_" + lViewsData.Id);
                    if (lViewColumns is not null)
                    {
                        if (lViewColumns.Count == 0)
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrl1ColIncluded2SaveView");
                            break;
                        }
                    }
                    var oTable = new Table();
                    if (lViewsData is not null)
                    {
                        oTable = _iTable.All().Where(x => x.TableName.Trim().ToLower().Equals(lViewsData.TableName.Trim().ToLower())).FirstOrDefault();
                    }

                    var lViewColumn = _iViewColumn.All().Where(x => x.ViewsId == lViewsData.Id).ToList();
                    // Start : Set Views details

                    if (lViewsData is not null)
                    {

                        if (lViewsData.Id > 0)
                        {
                            // To solve Bug No. 1166(There are same view name(Pipeline Report) with different folder records in 'Views' table.)(Identify unique record using ViewId,ViewName and TableName)
                            bool con1 = _iView.All().Any(x => (x.ViewName.Trim().ToLower()) == (lViewsData.ViewName.Trim().ToLower()) && x.Id != lViewsData.Id && x.TableName.Trim().ToLower().Equals(lViewsData.TableName.Trim().ToLower())) == false;
                            bool con2 = lViewsData.ViewName.Trim().ToLower().Equals("Purchase Orders".Trim().ToLower());
                            if (con1 | con2)
                            {
                                if (Strings.StrComp(oldViewName, lViewsData.ViewName, Constants.vbTextCompare) != 0)
                                {
                                    oSecureObject.Rename(oldViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.View, lViewsData.ViewName);
                                }
                                _iView.Update(pViewEntity);
                                viewIdVar = pViewEntity.Id;
                            }
                            else
                            {
                                Keys.ErrorType = "w";
                                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlTheViewNameL1"), lViewsData.ViewName);
                                break;
                            }
                        }
                        else if (_iView.All().Any(x => (x.ViewName.Trim().ToLower()) == (lViewsData.ViewName.Trim().ToLower())) == false)
                        {

                            lSecureObjectId = oSecureObject.GetSecureObjectID(lViewsData.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.View);
                            if (lSecureObjectId != 0)
                                oSecureObject.UnRegister(lSecureObjectId);

                            lSecureObjectId = oSecureObject.GetSecureObjectID(oTable.TableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table);
                            if (lSecureObjectId == 0L)
                                lSecureObjectId = (int)Enums.SecureObjects.View;

                            oSecureObject.Register(lViewsData.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.View, lSecureObjectId);

                            _iView.Add(pViewEntity);
                            viewIdVar = pViewEntity.Id;
                        }
                        else
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlTheViewNameL1"), lViewsData.ViewName);
                            break;
                        }

                        // Start : Set View Filters
                        if (this.TempData is not null)
                        {
                            if (this.TempData.ContainsKey("ViewFilterUpdate"))
                            {

                                List<ViewFilter> lViewFiltersDataTemp = TempData.Get<List<ViewFilter>>("ViewFilterUpdate"); //(List<ViewFilter>)this.TempData["ViewFilterUpdate"];
                                var viewFilterList = _iViewFilter.All();
                                if (lViewFiltersDataTemp is not null)
                                {
                                    foreach (ViewFilter pViewFilter in lViewFiltersDataTemp)
                                    {
                                        if (pViewFilter.ColumnNum is not null)
                                        {
                                            var viewColObj = lViewColumns.Where(m => m.ColumnNum == pViewFilter.ColumnNum).FirstOrDefault();
                                            pViewFilter.Sequence = 0;
                                            pViewFilter.PartOfView = true;
                                            if ((pViewFilter.ViewsId != (-1)))
                                            {
                                                if (viewFilterList.Any(x => x.ViewsId == pViewFilter.ViewsId && x.Id == pViewFilter.Id))
                                                {
                                                    _iViewFilter.Update(pViewFilter);
                                                }
                                                else
                                                {
                                                    pViewFilter.Id = 0;
                                                    pViewFilter.ViewsId = viewIdVar;
                                                    _iViewFilter.Add(pViewFilter);
                                                }
                                            }
                                            else if (pViewFilter.Id > 0)
                                            {
                                                var deleteViewFilter = _iViewFilter.Where(m => m.Id == pViewFilter.Id).FirstOrDefault();
                                                if (deleteViewFilter is not null)
                                                {
                                                    _iViewFilter.Delete(deleteViewFilter);
                                                }
                                            }
                                        }
                                    }

                                    if (lViewFiltersDataTemp.Count != 0)
                                    {
                                        var oViewUpdate = _iView.All().Where(m => m.Id == viewIdVar).FirstOrDefault();
                                        oViewUpdate.FiltersActive = true;
                                        _iView.Update(oViewUpdate);
                                    }
                                }
                            }
                        }

                        int preViewId = pViewEntity.Id;
                        var pViewColumnList = _iViewColumn.All().Where(x => x.ViewsId == pViewEntity.Id).ToList(); // .OrderBy(Function(x) x.ColumnNum)

                        if (pViewColumnList is not null)
                        {
                            if (pViewColumnList.Count() == 0 && lViewsData.Id > 0)
                            {
                                // Dim oViewsFirst = _iView.All().Where(Function(x) x.TableName.Trim().ToLower().Equals(pViewEntity.TableName.Trim().ToLower())).OrderBy(Function(x) x.ViewOrder).FirstOrDefault()
                                // pViewColumnList = _iViewColumn.All().Where(Function(x) x.ViewsId = oViewsFirst.Id).OrderBy(Function(x) x.ColumnNum).ToList()
                                // preViewId = oViewsFirst.Id

                                // Taken columns from AltView, If view does not have any columns - FUS- 5704
                                var oAltView = _iView.All().Where(x => x.Id == pViewEntity.AltViewId).FirstOrDefault();
                                pViewColumnList = _iViewColumn.All().Where(x => x.ViewsId == oAltView.Id).OrderBy(x => x.ColumnNum).ToList();
                                preViewId = oAltView.Id;
                            }
                        }
                        else
                        {
                            preViewId = pViewEntity.Id;
                        }

                        int iColumnNum = 0;
                        if (pViewColumnList is not null)
                        {
                            foreach (ViewColumn pViewColObj in pViewColumnList)
                            {
                                pViewColObj.ColumnNum = (short?)(iColumnNum - 1);
                                _iViewColumn.Update(pViewColObj);
                                iColumnNum = iColumnNum - 1;
                            }
                        }

                        var ViewColumnObj = new ViewColumn();

                        var dictionary = new Dictionary<int, int>();
                        //dictionary = (Dictionary<int, int>)this.Session["OrgViewColumnIds"];
                        dictionary = ContextService.GetObjectFromJson<Dictionary<int, int>>("OrgViewColumnIds", httpContext);
                        var dicUpdatedColNums = new Dictionary<int, int>();
                        dicUpdatedColNums = ContextService.GetObjectFromJson<Dictionary<int, int>>("UpViewColumnIds", httpContext);
                        bool bLookup0Updated = false;
                        if (lViewColumns is not null)
                        {
                            foreach (ViewColumn pViewColObj in pViewColumnList)
                            {
                                if (lViewColumns.Any(m => m.Id == pViewColObj.Id))
                                {
                                    var tempViewCol = lViewColumns.Where(m => m.Id == pViewColObj.Id).FirstOrDefault();

                                    // ' Update Lookup Id values
                                    // If Not dictionary Is Nothing AndAlso Not dicUpdatedColNums Is Nothing Then
                                    // If (pViewColObj.LookupIdCol > 0) Or (pViewColObj.LookupIdCol = 0 AndAlso pViewColObj.LookupType = 1) Then
                                    // Dim pOldLookupIdCol = pViewColObj.LookupIdCol
                                    // Dim pOldViewColumnId = dictionary.Where(Function(x) x.Value = pOldLookupIdCol).FirstOrDefault().Key
                                    // Dim pViewColumnsXYZ = dicUpdatedColNums.Where(Function(x) x.Key = pOldViewColumnId).FirstOrDefault().Value
                                    // 'Dim pViewColumnsXYZ = lViewColumnsOld.Where(Function(m) m.Id = pOldViewColumnId).FirstOrDefault()
                                    // tempViewCol.LookupIdCol = pViewColumnsXYZ
                                    // End If
                                    // End If
                                    // If tempViewCol.LookupIdCol Is Nothing Then
                                    // tempViewCol.LookupIdCol = pViewColObj.LookupIdCol
                                    // End If
                                    var pviewcol = pViewColObj;
                                    AddUpdateViewColumn(ref pviewcol, tempViewCol);
                                    _iViewColumn.Update(pviewcol);
                                    lViewColumns.Remove(tempViewCol);
                                }
                                else
                                {
                                    _iViewColumn.Delete(pViewColObj);
                                }
                            }

                            foreach (ViewColumn pViewColumns in lViewColumns)
                            {
                                if (lViewColumns.Any(m => m.Id == pViewColumns.Id))
                                {
                                    pViewColumns.Id = 0;
                                    pViewColumns.ViewsId = preViewId;
                                    // If (pViewColumns.LookupType = Enums.geViewColumnsLookupType.ltDirect) Then
                                    // pViewColumns.FieldName = DatabaseMap.RemoveTableNameFromField(pViewColumns.FieldName)
                                    // End If
                                    _iViewColumn.Add(pViewColumns);
                                }
                            }

                        }

                    }
                    // Remove Existing view form sql
                    ViewModel.SQLViewDelete(viewIdVar, passport);

                    // Set flag for 'Move Filter into SQL' to make button enable/disable
                    var vwFilterData = _iViewFilter.All().Where(m => m.ViewsId == viewIdVar);
                    _iView.CommitTransaction();
                    _iViewColumn.CommitTransaction();
                    _iViewFilter.CommitTransaction();
                    var Setting = new JsonSerializerSettings();
                    Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    ViewIdJSON = JsonConvert.SerializeObject(viewIdVar, Newtonsoft.Json.Formatting.Indented, Setting);
                    List<ViewColumn> lViewColumnsList;

                    if (pViewEntity.AltViewId != 0)
                    {
                        var altViewId = Convert.ToInt32(ViewIdJSON);
                        string altViewIdJSON = JsonConvert.SerializeObject(pViewEntity.AltViewId, Newtonsoft.Json.Formatting.Indented, Setting);
                        lViewColumnsList = _iViewColumn.All().Where(x => x.ViewsId == altViewId).OrderBy(x => x.ColumnNum).ToList();
                    }
                    else
                    {
                        var viewId = Convert.ToInt32(ViewIdJSON);
                        lViewColumnsList = _iViewColumn.All().Where(x => x.ViewsId == viewId).OrderBy(x => x.ColumnNum).ToList();
                    }
                    TempData.Set<List<ViewColumn>>("TempViewColumns_" + ViewIdJSON, lViewColumnsList);
                    //TempData["TempViewColumns_" + ViewIdJSON] = lViewColumnsList;

                    httpContext.Session.Remove("OrgViewColumnIds");
                    httpContext.Session.Remove("UpViewColumnIds");
                    // If lViewsData.Id <= 0 Then
                    passport.FillSecurePermissions();
                    // End If
                    if (pViewEntity.AltViewId != 0)
                    {
                        RefreshViewColGrid((int)pViewEntity.AltViewId, pViewEntity.TableName);
                    }
                    else
                    {
                        RefreshViewColGrid(viewIdVar, pViewEntity.TableName);
                    }

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminViewsSaveSuccess"); // Keys.SaveSuccessMessage()
                }
                catch (DbEntityValidationException dbEx)
                {
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            Trace.TraceInformation(Languages.get_Translation("msgAdminCtrlProperty"), validationError.PropertyName, validationError.ErrorMessage);
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlProperty"), validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                    Keys.ErrorType = "e";
                    _iView.RollBackTransaction();
                    _iViewColumn.RollBackTransaction();
                    _iViewFilter.RollBackTransaction();
                }
            }
            while (false);


            return Json(new
            {
                ViewIdJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public JsonResult GetSortedColumnList(int pViewId)
        {
            var lViewColumnsList = new List<ViewColumn>();
                        
            if (TempData.ContainsKey("TempViewColumns_" + pViewId))
            {
                lViewColumnsList = (TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + pViewId)).OrderBy(x => x.SortOrder).ToList();
            }

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(lViewColumnsList, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        [HttpPost]
        public IActionResult SaveSortedColumnToList(string[] pTableIds, int pViewId)
        {
            try
            {
                var lViewColumnsList = new List<ViewColumn>();
                if (TempData.ContainsKey("TempViewColumns_" + pViewId))
                {
                    lViewColumnsList = (TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + pViewId)).OrderBy(x => x.SortOrder).ToList();
                }

                foreach (ViewColumn item in lViewColumnsList)
                {
                    item.SortOrder = 0;
                    item.SortOrderDesc = false;
                }

                if (pTableIds is not null)
                {
                    int sortOrder = 1;
                    foreach (string item in pTableIds)
                    {
                        var splitValue = item.Split('|');
                        if (splitValue.Length > 1)
                        {
                            var columns = lViewColumnsList.Single(x => x.Id == Convert.ToInt32(splitValue[0]));
                            columns.SortOrder = sortOrder;
                            columns.SortOrderDesc = Convert.ToBoolean(splitValue[1]);
                            sortOrder = sortOrder + 1;
                        }
                    }
                }

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlTblMoveSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public JsonResult GetOperatorDDLData(int iViewId, int iColumnNum)
        {
            string jsonObjectOperator = string.Empty;
            string jsonFilterControls = string.Empty;
            var filterControls = new Dictionary<string, bool>();
            var oOperatorData = new List<KeyValuePair<string, string>>();
            string sThisFieldHeading = "";
            string sFirstLookupHeading = "";
            string sSecondLookupHeading = "";
            string sRecordJSON = "";
            string sValueFieldNameJSON = "";
            string sLookupFieldJSON = "";
            string sFirstLookupJSON = "";
            string sSecondLookupJSON = "";
            string filterColumnsJSON = "";
            string sValueFieldName = "";
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<View> _iView = new Repositories<View>();
            try
            {
                oOperatorData.Clear();
                filterControls.Clear();
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                oOperatorData = ViewModel.FillOperatorsDropDownOnChange(ref filterControls, _iView.All(), _iTable.All(), _iDatabas.All(), iColumnNum, httpContext);
                if (filterControls["FieldDDL"])
                {
                    if (TempData is not null)
                    {
                        var lstTEMPViewColumns = new List<ViewColumn>();
                        lstTEMPViewColumns = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + iViewId);
                        var oViewFilterColumns = new ViewColumn();
                        if (lstTEMPViewColumns is not null)
                        {
                            oViewFilterColumns = lstTEMPViewColumns.Where(m => m.ColumnNum == iColumnNum).FirstOrDefault();
                        }
                        if (oViewFilterColumns is not null)
                        {
                            var table = FillColumnCombobox(ref sValueFieldName, ref sThisFieldHeading, ref sFirstLookupHeading, ref sSecondLookupHeading, oViewFilterColumns);
                            if (!string.IsNullOrEmpty(sValueFieldName))
                            {
                                sValueFieldNameJSON = JsonConvert.SerializeObject(sValueFieldName, Newtonsoft.Json.Formatting.Indented, Setting);
                            }
                            if (!string.IsNullOrEmpty(sThisFieldHeading))
                            {
                                sLookupFieldJSON = JsonConvert.SerializeObject(sThisFieldHeading, Newtonsoft.Json.Formatting.Indented, Setting);
                            }
                            if (!string.IsNullOrEmpty(sFirstLookupHeading))
                            {
                                sFirstLookupJSON = JsonConvert.SerializeObject(sFirstLookupHeading, Newtonsoft.Json.Formatting.Indented, Setting);
                            }
                            if (!string.IsNullOrEmpty(sSecondLookupHeading))
                            {
                                sSecondLookupJSON = JsonConvert.SerializeObject(sSecondLookupHeading, Newtonsoft.Json.Formatting.Indented, Setting);
                            }
                            sRecordJSON = JsonConvert.SerializeObject(table, Newtonsoft.Json.Formatting.Indented, Setting);
                        }
                    }
                }
                var filterColumns = new List<ViewFilter>();
                GetFilterData(iViewId, ref filterColumns);
                filterColumnsJSON = JsonConvert.SerializeObject(filterColumns, Newtonsoft.Json.Formatting.Indented, Setting);
                jsonObjectOperator = JsonConvert.SerializeObject(oOperatorData, Newtonsoft.Json.Formatting.Indented, Setting);
                jsonFilterControls = JsonConvert.SerializeObject(filterControls, Newtonsoft.Json.Formatting.Indented, Setting);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Keys.SaveSuccessMessage();
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            } // ex.Message.ToString()

            return Json(new
            {
                errortype = Keys.ErrorType,
                errorMessage = Keys.ErrorMessage,
                sLookupFieldJSON,
                sValueFieldNameJSON,
                sFirstLookupJSON,
                sSecondLookupJSON,
                sRecordJSON,
                jsonFilterControls,
                filterColumnsJSON,
                jsonObjectOperator
            });

        }

        public object RefreshViewColGrid(int vViewId, string tableName)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                var GridColumnEntities = new List<GridColumns>();
                var lstTEMPViewColumns = new List<ViewColumn>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                if (TempData is not null)
                {
                    lstTEMPViewColumns = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + vViewId);
                }
                if (lstTEMPViewColumns is not null)
                {
                    var lDatabas = _iDatabas.All().OrderBy(m => m.Id);
                    var oTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                    foreach (ViewColumn column in lstTEMPViewColumns)
                    {
                        var GridColumnEntity = new GridColumns();
                        GridColumnEntity.ColumnSrNo = column.Id;
                        GridColumnEntity.ColumnId = (int)column.ColumnNum;
                        GridColumnEntity.ColumnName = column.Heading;
                        string sFieldType = "";
                        string sFieldSize = "";
                        ViewModel.GetFieldTypeAndSize(oTable, lDatabas, column.FieldName, ref sFieldType, ref sFieldSize);
                        GridColumnEntity.ColumnDataType = sFieldType;
                        GridColumnEntity.ColumnMaxLength = sFieldSize;
                        GridColumnEntity.IsPk = false;
                        GridColumnEntity.AutoInc = (bool)column.FilterField;
                        GridColumnEntities.Add(GridColumnEntity);
                    }
                    TempData.Remove("ColumnsData");
                    TempData.Set<List<GridColumns>>("ColumnsData", GridColumnEntities);
                    //TempData["ColumnsData"] = GridColumnEntities;
                }
                Keys.ErrorType = "s";
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
            }
            return Json(new { errortype = Keys.ErrorType });
        }

        public static List<GridColumns> GetColumnsData(IQueryable<View> lView, IQueryable<ViewColumn> lViewColumns, IQueryable<Databas> lDatabas, IQueryable<Table> lTables, int intViewsId, string sAction)
        {
            var GridColumnEntities = new List<GridColumns>();
            if (!string.IsNullOrEmpty(sAction) && sAction.Trim().ToUpper().Equals("E"))
            {
                var oViews = lView.Where(x => x.Id == intViewsId).FirstOrDefault();
                // Dim lViewColumns = lViewColumn.All()
                // Dim lDatabas = lDatabas

                if (oViews is not null)
                {
                    string sTableName = oViews.TableName;
                    int iViewsId = Convert.ToInt32(oViews.Id);
                    var oTable = lTables.Where(x => x.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())).FirstOrDefault();
                    var olViewColumns = lViewColumns.Where(x => x.ViewsId == iViewsId).OrderBy(x => x.ColumnNum);

                    if (olViewColumns is not null)
                    {
                        if (olViewColumns.Count() == 0)
                        {
                            var oViewsFirst = lView.Where(x => x.TableName.Trim().ToLower().Equals(oViews.TableName.Trim().ToLower())).OrderBy(x => x.ViewOrder).FirstOrDefault();
                            olViewColumns = lViewColumns.Where(x => x.ViewsId == oViewsFirst.Id).OrderBy(x => x.ColumnNum);
                        }
                    }

                    if (olViewColumns is not null)
                    {

                        foreach (ViewColumn column in olViewColumns)
                        {
                            var GridColumnEntity = new GridColumns();
                            GridColumnEntity.ColumnSrNo = column.Id;
                            GridColumnEntity.ColumnId = (int)column.ColumnNum;
                            GridColumnEntity.ColumnName = column.Heading;
                            string sFieldType = "";
                            string sFieldSize = "";
                            ViewModel.GetFieldTypeAndSize(oTable, lDatabas, column.FieldName, ref sFieldType, ref sFieldSize);
                            GridColumnEntity.ColumnDataType = sFieldType;
                            GridColumnEntity.ColumnMaxLength = sFieldSize;
                            GridColumnEntity.IsPk = false;
                            GridColumnEntity.AutoInc = (bool)column.FilterField;
                            GridColumnEntities.Add(GridColumnEntity);
                        }

                    }

                }
            }
            return GridColumnEntities;
        }

        public JsonResult FillColumnsDDL(int oViewId)
        {
            string jsonObjectColumns = string.Empty;
            try
            {
                // Dim filterColumns As New Dictionary(Of Integer, ViewColumn)
                // Dim lstTEMPViewColumns As New List(Of ViewColumn)
                // Dim filterFieldList As New List(Of KeyValuePair(Of String, String))
                // If (Not TempData Is Nothing) Then
                // lstTEMPViewColumns = TempData.Peek("TempViewColumns_" & oViewId)
                // If (Not lstTEMPViewColumns Is Nothing) Then
                // filterColumns = ViewModel.FillFilterFieldNames(lstTEMPViewColumns)
                // End If
                // End If
                // If (Not filterColumns Is Nothing) Then
                // For Each viewCol In filterColumns
                // If (viewCol.Value.LookupType = Enums.geViewColumnsLookupType.ltLookup) Then
                // filterFieldList.Add(New KeyValuePair(Of String, String)(viewCol.Value.Heading, Convert.ToString(viewCol.Value.ColumnNum) + "_" + Convert.ToString(viewCol.Value.LookupIdCol) + "***"))
                // Else
                // filterFieldList.Add(New KeyValuePair(Of String, String)(viewCol.Value.Heading, Convert.ToString(viewCol.Value.ColumnNum) + "_" + Convert.ToString(viewCol.Value.ColumnNum)))
                // End If
                // Next
                // End If
                // Dim Setting = New JsonSerializerSettings
                // Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects
                // jsonObjectColumns = JsonConvert.SerializeObject(filterFieldList, Newtonsoft.Json.Formatting.Indented, Setting)
                jsonObjectColumns = GetColumnsDDL(oViewId);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Keys.SaveSuccessMessage();
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }  // ex.Message.ToString()

            return Json(new
            {
                errortype = Keys.ErrorType,
                errorMessage = Keys.ErrorMessage,
                jsonObjectColumns
            });

        }

        public string GetColumnsDDL(int oViewId)
        {
            string jsonObjectColumns = string.Empty;
            var filterColumns = new Dictionary<int, ViewColumn>();
            var lstTEMPViewColumns = new List<ViewColumn>();
            var tempListViewColumns = new List<ViewColumn>();
            var filterFieldList = new List<KeyValuePair<string, string>>();
            if (this.TempData is not null)
            {
                tempListViewColumns = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + oViewId);
                foreach (ViewColumn ViewColumn in tempListViewColumns)
                {
                    var objectView = new ViewColumn(ViewColumn);
                    lstTEMPViewColumns.Add(objectView);
                }
                if (lstTEMPViewColumns is not null)
                {
                    filterColumns = ViewModel.FillFilterFieldNames(lstTEMPViewColumns);
                }
            }
            if (filterColumns is not null)
            {
                foreach (var viewCol in filterColumns)
                {
                    if (viewCol.Value.LookupType == (short)Enums.geViewColumnsLookupType.ltLookup)
                    {
                        filterFieldList.Add(new KeyValuePair<string, string>(viewCol.Value.Heading, Convert.ToString(viewCol.Value.ColumnNum) + "_" + Convert.ToString(viewCol.Value.LookupIdCol) + "***"));
                    }
                    else
                    {
                        filterFieldList.Add(new KeyValuePair<string, string>(viewCol.Value.Heading, Convert.ToString(viewCol.Value.ColumnNum) + "_" + Convert.ToString(viewCol.Value.ColumnNum)));
                    }
                }
            }
            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            jsonObjectColumns = JsonConvert.SerializeObject(filterFieldList, Newtonsoft.Json.Formatting.Indented, Setting);
            return jsonObjectColumns;

        }

        public DataTable FillColumnCombobox(ref string sValueFieldName, ref string sThisFieldHeading, ref string sFirstLookupHeading, ref string sSecondLookupHeading, ViewColumn oViewColumn)
        {
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<Table> _iTable = new Repositories<Table>();
            try
            {
                string jsonObjectColumns = "";
                var filterFieldList = new List<KeyValuePair<string, string>>();
                string sLookupTableName = "";
                var oParentTable = new Table();
                bool bLookUpById;
                bool bFoundIt;
                string sThisFieldName = "";
                string actualTableName = _iView.All().Where(m => m.Id == oViewColumn.ViewsId).FirstOrDefault().TableName;
                string sSQL = "";
                ADODB.Connection sADOCon;
                var table = new DataTable();
                string sFirstLookupField = "";
                string sSecondLookupField = "";
                string sLookupFieldName = "";

                if (oViewColumn is not null)
                {
                    if (oViewColumn.LookupType == (short)Enums.geViewColumnsLookupType.ltLookup)
                    {
                        sLookupTableName = oViewColumn.FieldName;
                        if (Strings.InStr(sLookupTableName, ".") > 1)
                        {
                            sLookupTableName = Strings.Left(sLookupTableName, Strings.InStr(sLookupTableName, ".") - 1);
                        }
                        else
                        {
                            sLookupTableName = actualTableName;
                        }

                        bLookUpById = false;
                        sThisFieldName = DatabaseMap.RemoveTableNameFromField(oViewColumn.FieldName);
                        sThisFieldHeading = oViewColumn.Heading;
                        if (!string.IsNullOrEmpty(sLookupTableName))
                        {
                            oParentTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(sLookupTableName.Trim().ToLower())).FirstOrDefault();
                        }
                        sLookupFieldName = sThisFieldName;
                        if (oParentTable is not null)
                        {
                            sValueFieldName = DatabaseMap.RemoveTableNameFromField(oParentTable.IdFieldName);
                        }
                        else
                        {
                            sValueFieldName = sThisFieldName;
                        }
                        sValueFieldName = DatabaseMap.RemoveTableNameFromField(sValueFieldName);
                    }
                    else
                    {
                        if (oViewColumn.LookupType == (short)Enums.geViewColumnsLookupType.ltDirect)
                        {
                            bFoundIt = false;
                            var parentRelationShip = _iRelationShip.All().Where(m => m.LowerTableName.Trim().ToLower().Equals(actualTableName.Trim().ToLower()));
                            foreach (RelationShip oRelationObj in parentRelationShip)
                            {
                                if (DatabaseMap.RemoveTableNameFromField(oRelationObj.LowerTableFieldName).Trim().ToLower().Equals(DatabaseMap.RemoveTableNameFromField(oViewColumn.FieldName).Trim().ToLower()))
                                {
                                    bFoundIt = true;
                                    sLookupTableName = oRelationObj.UpperTableName;
                                    break;
                                }
                            }
                            if (!bFoundIt)
                            {
                                var oRelationShip = _iRelationShip.All().OrderBy(m => m.Id);
                                foreach (RelationShip oRelationObj in oRelationShip)
                                {
                                    if (oRelationObj.LowerTableFieldName.Trim().ToLower().Equals(oViewColumn.FieldName.Trim().ToLower()))
                                    {
                                        sLookupTableName = oRelationObj.UpperTableName;
                                        break;
                                    }
                                }
                            }
                            if (string.IsNullOrEmpty(sLookupTableName))
                            {
                                var tempTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(actualTableName.Trim().ToLower())).FirstOrDefault();
                                if (DatabaseMap.RemoveTableNameFromField(tempTable.RetentionFieldName).Trim().ToLower().Equals(DatabaseMap.RemoveTableNameFromField(oViewColumn.FieldName).Trim().ToLower()))
                                {
                                    sLookupTableName = "SLRetentionCodes";
                                }
                                else if (Strings.InStr(sLookupTableName, ".") > 1)
                                {
                                    sLookupTableName = Strings.Left(sLookupTableName, Strings.InStr(sLookupTableName, ".") - 1);
                                }
                                else
                                {
                                    sLookupTableName = actualTableName;
                                }
                            }
                            if (!string.IsNullOrEmpty(sLookupTableName))
                            {
                                oParentTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(sLookupTableName.Trim().ToLower())).FirstOrDefault();
                                sThisFieldName = "Id";
                                sThisFieldHeading = "Id";
                            }
                        }
                        if (oParentTable is not null)
                        {
                            sLookupFieldName = DatabaseMap.RemoveTableNameFromField(oParentTable.IdFieldName);
                        }
                        else
                        {
                            sLookupFieldName = sThisFieldName;
                        }

                    }


                    if (oParentTable is not null)
                    {
                        sFirstLookupField = DatabaseMap.RemoveTableNameFromField(oParentTable.DescFieldNameOne);
                        sSecondLookupField = DatabaseMap.RemoveTableNameFromField(oParentTable.DescFieldNameTwo);
                    }
                    else
                    {
                        sFirstLookupField = "";
                        sSecondLookupField = "";
                    }
                    if (!string.IsNullOrEmpty(sFirstLookupField))
                    {
                        if (!sFirstLookupField.Trim().ToLower().Equals(sLookupFieldName.Trim().ToLower()))
                        {
                            sFirstLookupHeading = oParentTable.DescFieldPrefixOne;
                            if (sFirstLookupHeading is null)
                            {
                                sFirstLookupHeading = sFirstLookupField;
                            }
                            if (!string.IsNullOrEmpty(sSecondLookupField))
                            {
                                if (!sSecondLookupField.Trim().ToLower().Equals(sLookupFieldName.Trim().ToLower()))
                                {
                                    sSecondLookupHeading = oParentTable.DescFieldPrefixTwo;
                                    if (sSecondLookupHeading is null)
                                    {
                                        sSecondLookupHeading = sSecondLookupField;
                                    }
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(sSecondLookupField))
                        {
                            if (!sSecondLookupField.Trim().ToLower().Equals(sLookupFieldName.Trim().ToLower()))
                            {
                                sSecondLookupHeading = oParentTable.DescFieldPrefixTwo;
                                if (sSecondLookupHeading is null)
                                {
                                    sSecondLookupHeading = sSecondLookupField;
                                }
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(sSecondLookupField))
                    {
                        if (!sSecondLookupField.Trim().ToLower().Equals(sLookupFieldName.Trim().ToLower()))
                        {
                            sSecondLookupHeading = oParentTable.DescFieldPrefixTwo;
                            if (sSecondLookupHeading is null)
                            {
                                sSecondLookupHeading = sSecondLookupField;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(sThisFieldHeading))
                    {
                        if (!string.IsNullOrEmpty(sFirstLookupHeading))
                        {
                            if (sFirstLookupHeading.Trim().ToLower().Equals(sThisFieldHeading.Trim().ToLower()))
                            {
                                sFirstLookupHeading = sFirstLookupField;
                                if (!string.IsNullOrEmpty(sSecondLookupHeading))
                                {
                                    if (sSecondLookupHeading.Trim().ToLower().Equals(sThisFieldHeading.Trim().ToLower()))
                                    {
                                        sSecondLookupHeading = sSecondLookupField;
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(sSecondLookupHeading))
                            {
                                if (sSecondLookupHeading.Trim().ToLower().Equals(sThisFieldHeading.Trim().ToLower()))
                                {
                                    sSecondLookupHeading = sSecondLookupField;
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(sSecondLookupHeading))
                        {
                            if (!sSecondLookupHeading.Trim().ToLower().Equals(sThisFieldHeading.Trim().ToLower()))
                            {
                                sSecondLookupHeading = sSecondLookupField;
                            }
                        }
                    }
                    if (oViewColumn.LookupType == Convert.ToInt32(Enums.geViewColumnsLookupType.ltDirect))
                    {
                        sValueFieldName = sThisFieldHeading;
                    }
                    if (oViewColumn.LookupType == Convert.ToInt32(Enums.geViewColumnsLookupType.ltLookup))
                    {
                        bool flag = true;
                        if (!string.IsNullOrEmpty(sValueFieldName))
                        {
                            if (!string.IsNullOrEmpty(sLookupFieldName))
                            {
                                sValueFieldName = sThisFieldHeading;
                                flag = false;
                            }
                            if (!string.IsNullOrEmpty(sFirstLookupField) & flag)
                            {
                                sValueFieldName = sFirstLookupHeading;
                                flag = false;
                            }
                            if (!string.IsNullOrEmpty(sSecondLookupField) & flag)
                            {
                                sValueFieldName = sSecondLookupHeading;
                                flag = false;
                            }
                        }
                    }
                    // If (Not String.IsNullOrEmpty(sValueFieldName)) Then
                    // If (Not String.IsNullOrEmpty(sLookupFieldName)) Then
                    // If (sLookupFieldName.Trim.ToLower.Equals(sValueFieldName.Trim.ToLower)) Then
                    // sValueFieldName = sLookupFieldName
                    // End If
                    // End If
                    // If (Not String.IsNullOrEmpty(sFirstLookupField)) Then

                    // End If
                    // If (Not String.IsNullOrEmpty(sFirstLookupField)) Then

                    // End If
                    // End If
                    if (!string.IsNullOrEmpty(sLookupFieldName))
                    {
                        sSQL = "SELECT [" + sLookupFieldName + "]";
                    }
                    else
                    {
                        sSQL = "SELECT ";
                    }
                    if (!string.IsNullOrEmpty(sFirstLookupHeading))
                    {
                        sSQL = sSQL + ",[" + sFirstLookupField + "]";
                    }
                    else
                    {
                        sSQL = sSQL;
                    }
                    if (!string.IsNullOrEmpty(sSecondLookupHeading))
                    {
                        sSQL = sSQL + ",[" + sSecondLookupField + "] ";
                    }
                    else
                    {
                        sSQL = sSQL;
                    }

                    if (!string.IsNullOrEmpty(sSQL) & oParentTable is not null)
                    {
                        sSQL = sSQL + " FROM [" + oParentTable.TableName + "];";
                        sADOCon = DataServices.DBOpen(oParentTable, _iDatabas.All());
                    }

                    else
                    {
                        sSQL = sSQL + " FROM [" + sLookupTableName + "];";
                        sADOCon = DataServices.DBOpen();

                    }
                    string arglError = "";
                    var rs = DataServices.GetADORecordSet(ref sSQL, sADOCon, lError: ref arglError);
                    if (!(rs.RecordCount == 0))
                    {
                        if (!string.IsNullOrEmpty(sThisFieldHeading))
                        {
                            table.Columns.Add(new DataColumn(sThisFieldHeading));
                        }
                        if (!string.IsNullOrEmpty(sFirstLookupHeading))
                        {
                            table.Columns.Add(new DataColumn(sFirstLookupHeading));
                        }
                        else
                        {
                            sFirstLookupField = "";
                        }
                        if (!string.IsNullOrEmpty(sSecondLookupHeading))
                        {
                            table.Columns.Add(new DataColumn(sSecondLookupHeading));
                        }
                        else
                        {
                            sSecondLookupField = "";
                        }

                        while (!rs.EOF)
                        {
                            var rowObj = table.NewRow();
                            if (!string.IsNullOrEmpty(sThisFieldHeading) & !string.IsNullOrEmpty(sLookupFieldName))
                            {
                                rowObj[sThisFieldHeading] = rs.Fields[sLookupFieldName];
                            }
                            if (!string.IsNullOrEmpty(sFirstLookupHeading) & !string.IsNullOrEmpty(sFirstLookupField))
                            {
                                rowObj[sFirstLookupHeading] = rs.Fields[sFirstLookupField];
                            }
                            if (!string.IsNullOrEmpty(sSecondLookupHeading) & !string.IsNullOrEmpty(sSecondLookupField))
                            {
                                rowObj[sSecondLookupHeading] = rs.Fields[sSecondLookupField];
                            }
                            table.Rows.Add(rowObj);
                            rs.MoveNext();
                        }

                    }
                }
                Keys.ErrorType = "s";
                return table;
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                throw;
            } // ex
        }

        public JsonResult MoveFilterInSQL(ViewsCustomModel lViewsCustomModelEntites)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<ViewFilter> _iViewFilter = new Repositories<ViewFilter>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();

                var lViewsData = lViewsCustomModelEntites.ViewsModel;
                int oViewId = lViewsData.Id;
                var lViewFiltersData = new List<ViewFilter>();
                if (this.TempData.ContainsKey("ViewFilterUpdate"))
                {
                    lViewFiltersData = TempData.GetPeek<List<ViewFilter>>("ViewFilterUpdate");
                    lViewFiltersData = lViewFiltersData.Where(m => m.ViewsId != -1).ToList();
                }
                else
                {
                    lViewFiltersData = _iViewFilter.All().Where(m => m.ViewsId == oViewId).ToList();
                }
                // Dim lViewFiltersData As List(Of ViewFilter) = lViewsCustomModelEntites.ViewFilterList
                var oTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(lViewsData.TableName.Trim().ToLower())).FirstOrDefault();
                var lstViewColumns = new List<ViewColumn>();
                string sError = "";
                string sSQL = "";
                string sTemp = "";
                int iWherePos;
                string jsonSQLState = "";

                if (this.TempData is not null)
                {
                    lstViewColumns = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + oViewId);
                }

                if (lViewFiltersData is not null)
                {
                    var lViewFiltersDataList = lViewFiltersData.Where(m => m.ViewsId != -1).ToList();
                    sError = ViewModel.ProcessFilter(lViewFiltersDataList, lstViewColumns, _iTable.All(), _iDatabas.All(), lViewsData, oTable, true, ref sSQL, false, true);
                }

                if (!string.IsNullOrEmpty(sError))
                {
                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlErrorMovingFilters"), sError);
                    Keys.ErrorType = "w";
                }
                else
                {
                    sTemp = lViewsData.SQLStatement;
                    iWherePos = Strings.InStr(1, sTemp, " WHERE ", Constants.vbTextCompare);

                    if (iWherePos > 0)
                    {
                        sTemp = Strings.Left(sTemp, iWherePos + 6) + "(" + Strings.Mid(sTemp, iWherePos + 7);
                        sTemp = sTemp + " AND " + sSQL + ")";
                    }
                    else
                    {
                        sTemp = sTemp + " WHERE " + sSQL;
                    }
                    var viewFilterData = _iViewFilter.All().Where(m => m.ViewsId == lViewsData.Id);
                    _iViewFilter.DeleteRange(viewFilterData);
                    this.TempData.Remove("ViewFilterUpdate");
                    string SQLState = sTemp;
                    var Setting = new JsonSerializerSettings();
                    Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    jsonSQLState = JsonConvert.SerializeObject(SQLState, Newtonsoft.Json.Formatting.Indented, Setting);


                    Keys.ErrorMessage = "";
                    Keys.ErrorType = "s";
                }
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    errorMessage = Keys.ErrorMessage,
                    jsonSQLState
                });
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        public void GetFilterData(int oViewId, ref List<ViewFilter> filterColumns)
        {
            try
            {
                if (TempData.ContainsKey("ViewFilterUpdate"))
                {
                    filterColumns = TempData.GetPeek<List<ViewFilter>>("ViewFilterUpdate");
                }
                else
                {
                    IRepository<ViewFilter> _iViewFilter = new Repositories<ViewFilter>();
                    filterColumns = _iViewFilter.All().Where(m => m.ViewsId == oViewId).ToList();
                }
            }
            catch (Exception)
            {
            }
        }

        public IActionResult ValidateFilterData(ViewsCustomModel lViewsCustomModelEntites, bool EventFlag)
        {
            string sErrorJSON = "";
            string moveFilterFlagJSON = "";
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();

            do
            {
                try
                {
                    var lViewsData = lViewsCustomModelEntites.ViewsModel;
                    var lstViewColumns = new List<ViewColumn>();
                    var lViewFiltersData = lViewsCustomModelEntites.ViewFilterList;
                    int oViewId = lViewsData.Id;
                    string sError = "";
                    var oTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(lViewsData.TableName.Trim().ToLower())).FirstOrDefault();
                    string sSQL = "";
                    bool moveFilterFlag = false;
                    if (TempData is not null)
                    {
                        lstViewColumns = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + oViewId);
                    }

                    if (lViewFiltersData is not null)
                    {
                        var lViewFiltersDataList = lViewFiltersData.Where(m => m.ViewsId != -1).ToList();

                        if (lViewFiltersDataList.Count != 0)
                        {
                            moveFilterFlag = lViewFiltersDataList.Any(m => m.Active == true);
                            if (moveFilterFlag)
                            {
                                bool exitTry = false;
                                foreach (var lviewFilter in lViewFiltersDataList)
                                {
                                    if (lviewFilter.ColumnNum is null)
                                    {
                                        Keys.ErrorType = "s";
                                        moveFilterFlag = false;
                                        exitTry = true;
                                        break;
                                    }
                                    else
                                    {
                                        moveFilterFlag = true;
                                    }
                                }

                                if (exitTry)
                                {
                                    break;
                                }
                                sError = ViewModel.ProcessFilter(lViewFiltersDataList, lstViewColumns, _iTable.All(), _iDatabas.All(), lViewsData, oTable, true, ref sSQL, false, true);
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                    if (EventFlag)
                    {
                        if (sError.Equals("") & lViewFiltersData.Count != 0)
                        {
                            TempData.Set<List<ViewFilter>>("ViewFilterUpdate", lViewFiltersData);
                            //TempData["ViewFilterUpdate"] = lViewFiltersData;
                        }
                    }

                    var Setting = new JsonSerializerSettings();
                    Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                    sErrorJSON = JsonConvert.SerializeObject(sError, Newtonsoft.Json.Formatting.Indented, Setting);
                    moveFilterFlagJSON = JsonConvert.SerializeObject(moveFilterFlag, Newtonsoft.Json.Formatting.Indented, Setting);
                    Keys.ErrorType = "w";
                }
                catch (Exception)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                } // ex.Message.ToString
            }
            while (false);
            return Json(new
            {
                errortype = Keys.ErrorType,
                errorMessage = Keys.ErrorMessage,
                sErrorJSON,
                moveFilterFlagJSON
            });
        }

        public object GetFilterData1(int oViewId)
        {
            string filterColumnsJSON = "";
            bool parmOpenCloseExist = false;
            bool joinOrOperatorExist = false;
            IRepository<ViewFilter> _iViewFilter = new Repositories<ViewFilter>();
            string jsonColumns = "";
            try
            {
                var filterColumns = new List<ViewFilter>();
                if (this.TempData.ContainsKey("ViewFilterUpdate"))
                {
                    filterColumns = TempData.GetPeek<List<ViewFilter>>("ViewFilterUpdate");
                    filterColumns = filterColumns.Where(m => m.ViewsId != -1).ToList();
                }
                else
                {
                    filterColumns = _iViewFilter.All().Where(m => m.ViewsId == oViewId).ToList();
                }
                foreach (var filterObj in filterColumns)
                {
                    if (filterObj.OpenParen is not null & !string.IsNullOrEmpty(filterObj.OpenParen))
                    {
                        if (filterObj.OpenParen.Contains("("))
                        {
                            parmOpenCloseExist = true;
                            break;
                        }
                        else if (filterObj.CloseParen is not null & !string.IsNullOrEmpty(filterObj.CloseParen))
                        {
                            if (filterObj.CloseParen.Contains(""))
                            {
                                parmOpenCloseExist = true;
                                break;
                            }
                        }

                    }
                    // parmOpenCloseExist = filterColumns.Any(Function(m) m.CloseParen IsNot Nothing Or m.OpenParen IsNot Nothing)

                }
                joinOrOperatorExist = filterColumns.Any(m => m.JoinOperator == "Or");
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                filterColumnsJSON = JsonConvert.SerializeObject(filterColumns, Newtonsoft.Json.Formatting.Indented, Setting);
                jsonColumns = GetColumnsDDL(oViewId);

                Keys.ErrorType = "s";
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            } // ex.Message.ToString

            bool AnyadvancedFeatureCheck = parmOpenCloseExist;
            return Json(new
            {
                errortype = Keys.ErrorType,
                errorMessage = Keys.ErrorMessage,
                filterColumnsJSON,
                jsonColumns,
                AdvancedJsonFlag = AnyadvancedFeatureCheck,
                joinOrOperatorExist
            });
        }

        // DELETE the selected view.
        public JsonResult DeleteView(int pViewId)
        {
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            IRepository<ViewFilter> _iViewFilter = new Repositories<ViewFilter>();
            if (pViewId <= 0)
            {
                return Json(new
                {
                    errortype = "e",
                    message = "Selected view not found."
                });
            }
            IRepository<View> _iView = new Repositories<View>();

            _iView.BeginTransaction();
            _iViewColumn.BeginTransaction();
            _iViewFilter.BeginTransaction();

            // var BaseWebPage = new BaseWebPage();
            var oSecureObject = new Smead.Security.SecureObject(passport);

            try
            {

                var oView = _iView.All().Where(x => x.Id == pViewId).FirstOrDefault();

                int lSecureObjectId = oSecureObject.GetSecureObjectID(oView.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.View);
                if (lSecureObjectId != 0)
                    oSecureObject.UnRegister(lSecureObjectId);

                var lViewFilters = _iViewFilter.All().Where(x => x.ViewsId == pViewId);
                _iViewFilter.DeleteRange(lViewFilters);

                var lViewColumns = _iViewColumn.All().Where(x => x.ViewsId == pViewId);
                _iViewColumn.DeleteRange(lViewColumns);

                _iView.Delete(oView);


                _iViewFilter.CommitTransaction();
                _iViewColumn.CommitTransaction();
                _iView.CommitTransaction();

                passport.FillSecurePermissions();

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlViewDeletedSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                _iViewFilter.RollBackTransaction();
                _iViewColumn.RollBackTransaction();
                _iView.RollBackTransaction();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Up & Down
        public JsonResult ViewsOrderChange(string pAction, int pViewId)
        {
            bool bUpperLast = false;
            bool bLowerLast = false;
            IRepository<View> _iView = new Repositories<View>();
            try
            {
                var lViews = _iView.All();
                var oViews = lViews.Where(x => x.Id == pViewId).FirstOrDefault();
                View oUpperView;
                View oDownView;
                var intUpdatedOrder = default(int);
                var intOrgOrder = default(int);
                int intLastOrder;

                string oTableName = "";
                if (oViews is not null)
                {
                    oTableName = oViews.TableName;
                    intOrgOrder = (int)oViews.ViewOrder;
                }

                lViews = lViews.Where(x => x.TableName.Trim().ToLower().Equals(oTableName.Trim().ToLower()));

                var oViewSortButton = lViews.Where(x => x.Printable == false).OrderByDescending(x => x.ViewOrder).FirstOrDefault();
                intLastOrder = (int)oViewSortButton.ViewOrder;

                if (!string.IsNullOrEmpty(oTableName))
                {

                    if (pAction == "U")
                    {
                        oUpperView = lViews.Where(x => x.ViewOrder < oViews.ViewOrder && x.Printable == false).OrderByDescending(x => x.ViewOrder).FirstOrDefault();

                        intUpdatedOrder = (int)oUpperView.ViewOrder;
                        oUpperView.ViewOrder = intOrgOrder;
                        _iView.Update(oUpperView);
                    }

                    else if (pAction == "D")
                    {
                        oDownView = lViews.Where(x => (x.ViewOrder > oViews.ViewOrder) && (x.Printable == false)).OrderBy(x => x.ViewOrder).FirstOrDefault();

                        intUpdatedOrder = (int)oDownView.ViewOrder;
                        oDownView.ViewOrder = intOrgOrder;
                        _iView.Update(oDownView);
                    }

                    oViews.ViewOrder = intUpdatedOrder;

                    if (intUpdatedOrder == intLastOrder)
                    {
                        bLowerLast = true;
                    }
                    if (intUpdatedOrder == 1)
                    {
                        bUpperLast = true;
                    }
                    _iView.Update(oViews);

                }

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlViewDeletedSuccessfully");
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();

            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                bupperlast = bUpperLast,
                blowerlast = bLowerLast
            });
        }

        #endregion

        #region Reports
        public PartialViewResult ReportListPartial()
        {
            return PartialView("_ReportListPartial");
        }

        public ContentResult ReportsTreePartial(string root)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<ReportStyle> _iReportStyle = new Repositories<ReportStyle>();
            var lTableEntities = _iTable.All().OrderBy(m => m.TableName);
            var lViewsEntities = _iView.All();
            var lReportStyleEntities = _iReportStyle.All();

            string treeView = ReportsModel.GetBindReportsMenus(root, lTableEntities, lViewsEntities, lReportStyleEntities, passport, httpContext);

            return this.Content(treeView);
        }

        public PartialViewResult LoadReportsView()
        {
            return PartialView("_ReportDefinitionsPartial");
        }

        public IActionResult GetReportInformation(int pReportID, int bIsAdd)
        {
            var lstTblNames = new List<KeyValuePair<string, string>>();
            var lstReportStyles = new List<KeyValuePair<string, string>>();
            Table oTable;
            string lstTblNamesList = "";
            string lstReportStylesList = "";
            string lstChildTablesObjStr = "";
            // Dim actualTblName As String = ""
            var lstChildTables = new List<KeyValuePair<string, string>>();
            int lNextReport = 0;
            string sReportName = "";
            bool bFound;
            View pViewEntity;
            View pSubViewEntity;
            var lstRelatedChildTable = new List<RelationShip>();
            // Dim reportName As String = ""
            string tblName = "";
            string sReportStyleId = "";

            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<ReportStyle> _iReportStyle = new Repositories<ReportStyle>();
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            IRepository<View> _iView = new Repositories<View>();
            var tableEntity = _iTable.All().OrderBy(x => x.TableName).ToList();
            var reportStyleEntity = _iReportStyle.All();
            // March 30 2016.
            int subViewId2 = 0;
            int subViewId3 = 0;

            pViewEntity = _iView.All().Where(x => x.Id == pReportID).FirstOrDefault();
            if (!(pViewEntity == null))
            {
                if (pViewEntity.SubViewId > 0 && pViewEntity.SubViewId != 9999)
                {
                    subViewId2 = (int)pViewEntity.SubViewId;
                }
            }

            if (subViewId2 > 0 & subViewId2 != 9999)
            {
                pSubViewEntity = _iView.All().Where(x => x.Id == subViewId2).FirstOrDefault();
                // Added on April 26th 2016.
                if (!(pSubViewEntity == null))
                {
                    subViewId3 = (int)pSubViewEntity.SubViewId;
                }
            }

            try
            {
                // Get the list of Tables for report.
                foreach (var currentOTable in tableEntity)
                {
                    oTable = currentOTable;
                    if (!CollectionsClass.IsEngineTable(oTable.TableName) & passport.CheckPermission(oTable.TableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.View))
                    {
                        lstTblNames.Add(new KeyValuePair<string, string>(oTable.TableName, oTable.UserName));
                    }
                }

                // Get the list of Report Styles
                foreach (var oReportStyle in reportStyleEntity)
                    lstReportStyles.Add(new KeyValuePair<string, string>(oReportStyle.ReportStylesId.ToString(), oReportStyle.Id));

                // Get the list of child tables for Level 2.
                // Commented by Ganesh. - For Bug fixing.
                // If (Not pViewEntity.TableName = "") Then
                // actualTblName = _iTable.All().Where(Function(x) x.UserName = pViewEntity.TableName).FirstOrDefault().TableName
                // End If
                if (pViewEntity is not null)
                {
                    lstRelatedChildTable = _iRelationShip.All().Where(x => (x.UpperTableName) == (pViewEntity.TableName)).ToList();
                    sReportName = pViewEntity.ViewName;
                    tblName = pViewEntity.TableName;
                    sReportStyleId = pViewEntity.ReportStylesId;
                }

                foreach (var lTableName in lstRelatedChildTable)
                {
                    // Change on date- June 18th 2015.
                    oTable = _iTable.All().Where(x => x.UserName.Equals(lTableName.LowerTableName)).FirstOrDefault();

                    if (oTable is not null)
                    {
                        lstChildTables.Add(new KeyValuePair<string, string>(oTable.TableName, oTable.UserName));
                        oTable = null;
                    }
                }

                // Get the report name when clicked on ADD.
                if (Convert.ToBoolean(bIsAdd))
                {
                    do
                    {
                        bFound = false;

                        if (lNextReport == 0)
                        {
                            // Modified by Hemin for bug fix
                            // sReportName = Languages.Translation("tiAdminCtrlNewReport")
                            sReportName = "New Report";
                        }
                        else
                        {
                            // Modified by Hemin
                            // sReportName = String.Format(Languages.Translation("tiAdminCtrlNewReport1"), lNextReport)
                            sReportName = "New Report " + lNextReport;
                        }

                        // For Each oTable In _iTable.All()
                        foreach (var oView in _iView.All())
                        {
                            if (Strings.StrComp(oView.ViewName, sReportName, Constants.vbTextCompare) == 0)
                            {
                                lNextReport = lNextReport + 1;
                                bFound = true;
                            }
                        }
                        // Next oTable

                        if (!bFound)
                        {
                            break;
                        }
                    }
                    while (true);
                }


                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

                lstTblNamesList = JsonConvert.SerializeObject(lstTblNames, Newtonsoft.Json.Formatting.Indented, Setting);
                lstReportStylesList = JsonConvert.SerializeObject(lstReportStyles, Newtonsoft.Json.Formatting.Indented, Setting);
                lstChildTablesObjStr = JsonConvert.SerializeObject(lstChildTables, Newtonsoft.Json.Formatting.Indented, Setting);
            }
            catch (Exception)
            {

            }

            return Json(new
            {
                lstTblNamesList,
                lstReportStylesList,
                lstChildTablesObjStr,
                sReportName,
                tblName,
                sReportStyleId,
                subViewId2,
                subViewId3,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // DELETE the selected report.
        public JsonResult DeleteReport(int pReportID)
        {
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            IRepository<View> _iView = new Repositories<View>();
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
            try
            {
                View pViewEntity;
                object pViewColEnities;
                View pSubViewEntity;
                object pSubViewColEntities;

                // New Added
                // Dim pViewEntity = _iView.All().Where(Function(x) x.Id = pReportID).FirstOrDefault()
                // Dim pInternalTblName = _iTable.All().Where(Function(x) x.UserName = pTableName).FirstOrDefault().TableName
                object pSecureObjEntity;
                object pSecureObjPermisionEntities;
                int pSecureObjectID;

                // Dim pViewID = _iView.All().Where(Function(x) x.TableName = pInternalTblName And x.ViewName = pReportName).FirstOrDefault().Id
                // Dim pSubViewID = _iView.All().Where(Function(x) x.TableName = pInternalTblName And x.ViewName = pReportName).FirstOrDefault().SubViewId

                _iView.BeginTransaction();
                _iViewColumn.BeginTransaction();
                _iSecureObject.BeginTransaction();
                _iSecureObjectPermission.BeginTransaction();
                // DELETE View and ViewColumns for report been deleted.
                pViewEntity = _iView.All().Where(x => x.Id == pReportID).FirstOrDefault();
                _iView.Delete(pViewEntity);

                pViewColEnities = _iViewColumn.All().Where(x => x.ViewsId == pReportID);
                _iViewColumn.DeleteRange((IEnumerable<ViewColumn>)pViewColEnities);

                var pSecureObject = _iSecureObject.All().FirstOrDefault(x => (x.Name) == (pViewEntity.ViewName) & x.SecureObjectTypeID == (int)Enums.SecureObjectType.Reports);
                if (pSecureObject is not null)
                {
                    pSecureObjectID = pSecureObject.SecureObjectID;
                    pSecureObjEntity = _iSecureObject.All().Where(x => x.SecureObjectID == pSecureObjectID).FirstOrDefault();
                    if(pSecureObjEntity!=null)
                    {
                        _iSecureObject.Delete((TabFusionRMS.Models.SecureObject)pSecureObjEntity);

                    }
                    
                    _iSecureObject.CommitTransaction();

                    pSecureObjPermisionEntities = _iSecureObjectPermission.All().Where(x => x.SecureObjectID == pSecureObjectID);
                    _iSecureObjectPermission.DeleteRange((IEnumerable<SecureObjectPermission>)pSecureObjPermisionEntities);
                }

                // If pViewEntity.SubViewId <> 0 Then
                if (pViewEntity.SubViewId != 0)
                {
                    pSubViewEntity = _iView.All().FirstOrDefault(x => x.Id == pViewEntity.SubViewId);

                    // ' Added by Hasmukh - check secound level available or not
                    if (pSubViewEntity is not null)
                    {
                        var pThirdLevelSubViewId = pSubViewEntity.SubViewId;

                        _iView.Delete(pSubViewEntity);

                        pSubViewColEntities = _iViewColumn.All().Where(x => x.ViewsId == pViewEntity.SubViewId);
                        _iViewColumn.DeleteRange((IEnumerable<ViewColumn>)pSubViewColEntities);

                        // ' Added by Hasmukh - check third level available or not
                        // If pThirdLevelSubViewId <> 0 Then
                        if (pThirdLevelSubViewId != 0)
                        {
                            var pSubViewThirdLevelEntity = _iView.All().FirstOrDefault(x => x.Id == pThirdLevelSubViewId);
                            if (pSubViewThirdLevelEntity is not null)
                            {
                                _iView.Delete(pSubViewThirdLevelEntity);

                                var pSubViewThirdLevelColEntities = _iViewColumn.All().Where(x => x.ViewsId == pThirdLevelSubViewId);
                                _iViewColumn.DeleteRange(pSubViewThirdLevelColEntities);
                            }
                        }

                    }
                }

                _iView.CommitTransaction();
                _iViewColumn.CommitTransaction();
                // _iSecureObject.CommitTransaction()
                _iSecureObjectPermission.CommitTransaction();

                // Fill the secure permissions dataset after delete report.
                passport.FillSecurePermissions();

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRptDelSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                _iSecureObjectPermission.RollBackTransaction();
                _iSecureObject.RollBackTransaction();
                _iViewColumn.RollBackTransaction();
                _iView.RollBackTransaction();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public JsonResult DeleteReportsPrintingColumn(int pColumnId, int pIndex)
        {
            List<ViewColumn> lstViewColumns;
            ViewColumn pViewColumn;
            try
            {
                ;
                lstViewColumns = TempData.Get<List<ViewColumn>>("TempViewColumns_" + pIndex);
                pViewColumn = lstViewColumns.FirstOrDefault(x => x.Id == pColumnId);
                var ColumnNumVar = pViewColumn.ColumnNum;
                lstViewColumns.Remove(pViewColumn);
                foreach (ViewColumn viewColObj in lstViewColumns)
                {
                    if (viewColObj.ColumnNum > ColumnNumVar)
                    {
                        viewColObj.ColumnNum = ColumnNumVar;
                        ColumnNumVar = (short?)(ColumnNumVar + 1);
                    }
                }

                TempData.Set<List<ViewColumn>>("TempViewColumns_" + pIndex, lstViewColumns);
                TempData.Peek("TempViewColumns_" + pIndex);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlColRmvSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Get the list of views related to table.
        public JsonResult GetTablesView(string pTableName)
        {
            var lstViews = new List<KeyValuePair<string, string>>();
            Table oTable;
            string lstViewStr = "";
            string lstChildTablesObjStr = "";
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<View> _iView = new Repositories<View>();
            var tableEntity = _iTable.All().OrderBy(x => x.TableName).ToList();
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            var lViewEntities = _iView.All();
            var lViewColumnEntities = _iViewColumn.All();
            var lstChildTables = new List<KeyValuePair<string, string>>();
            // var BaseWebPage = new BaseWebPage();
            // Change made on 19th June 2015.            
            var lLoopViewEntities = lViewEntities.Where(x => (x.TableName.Trim().ToLower()) == (pTableName.ToLower()));
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
            try
            {
                foreach (var oView in lLoopViewEntities)
                {
                    // Added on 06/01/2016
                    // If (Not oView.Printable) And (passport.CheckPermission(oView.ViewName, Enums.SecureObjects.View, Enums.PassportPermissions.View)) Then
                    // lstViews.Add(New KeyValuePair(Of String, String)(oView.Id, oView.ViewName))
                    // End If
                    // Added on 06/01/2016
                    if (NotSubReport(oView, pTableName))
                    {
                        // If oView.Printable Then
                        // If (passport.CheckPermission(oView.ViewName, Enums.SecureObjects.Reports, Enums.PassportPermissions.Configure)) Then
                        if (passport.CheckPermission(oView.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Reports, (Permissions.Permission)Enums.PassportPermissions.View) | passport.CheckPermission(oView.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.View, (Permissions.Permission)Enums.PassportPermissions.View))
                        {
                            // If (_iSecureObject.All().Any(Function(x) x.Name = oView.ViewName)) Then
                            lstViews.Add(new KeyValuePair<string, string>(oView.Id.ToString(), oView.ViewName));
                        }
                        // End If
                        // End If
                        // End If
                    }
                }

                var lstRelatedChildTable = _iRelationShip.All().Where(x => (x.UpperTableName) == (pTableName)).ToList();

                foreach (var lTableName in lstRelatedChildTable)
                {
                    oTable = _iTable.All().Where(x => x.TableName.Equals(lTableName.LowerTableName)).FirstOrDefault();

                    if (oTable is not null)
                    {
                        lstChildTables.Add(new KeyValuePair<string, string>(oTable.TableName, oTable.UserName));
                        oTable = null;
                    }
                }

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

                lstViewStr = JsonConvert.SerializeObject(lstViews, Newtonsoft.Json.Formatting.Indented, Setting);
                lstChildTablesObjStr = JsonConvert.SerializeObject(lstChildTables, Newtonsoft.Json.Formatting.Indented, Setting);
            }

            catch (Exception)
            {

            }

            return Json(new
            {
                lstViewsList = lstViewStr,
                lstChildTablesObjStr,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Get the list of columns for report view.
        [HttpPost]
        public JsonResult GetColumnsForPrinting([FromForm] string pTableName, [FromForm] int pViewId, [FromForm] int pIndex)
        {
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            var viewColumnEntity = _iViewColumn.All().Where(x => x.ViewsId == pViewId).OrderByField("ColumnNum", true);
            string viewColumnEntityObj = "";
            var lstTrackedHistory = new List<KeyValuePair<int, string>>();
            string lstTrackedHisObj = "";
            string lstViewObjStr = "";
            string tableEntityObjStr = "";
            int iCount = 0;
            bool mbHistory;
            Table tableEntity;
            Table oChildTable;
            View oView;
            var lstTEMPViewColumns = new List<ViewColumn>();
            // var BaseWebPage = new BaseWebPage();
            bool bTrackable = false;
            int altViewId = 0;
            IRepository<View> _iView = new Repositories<View>();
            IRepository<Table> _iTable = new Repositories<Table>();

            try
            {
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

                // Added on 24/12/2015 to fix Bug#793
                if (viewColumnEntity.Count() == 0)
                {
                    altViewId = (int)_iView.All().Where(x => x.Id == pViewId).FirstOrDefault().AltViewId;
                    if (altViewId != 0)
                    {
                        viewColumnEntity = _iViewColumn.All().Where(x => x.ViewsId == altViewId).OrderByField("ColumnNum", true);
                    }
                }

                viewColumnEntityObj = JsonConvert.SerializeObject(viewColumnEntity, Newtonsoft.Json.Formatting.Indented, Setting);

                // Added New on date - 23rd June 2015.
                foreach (var objView in viewColumnEntity)
                    lstTEMPViewColumns.Add(objView);

                // TempData("ViewId") = pViewId

                if (lstTEMPViewColumns.Count > 0)
                {
                    TempData.Set<List<ViewColumn>>("TempViewColumns_" + pIndex, lstTEMPViewColumns);
                }

                var ViewsEntity = _iView.All().Where(x => x.Id == pViewId);
                lstViewObjStr = JsonConvert.SerializeObject(ViewsEntity, Newtonsoft.Json.Formatting.Indented, Setting);

                tableEntity = _iTable.All().Where(x => (x.TableName) == (pTableName)).FirstOrDefault();
                oView = _iView.All().Where(x => x.Id == pViewId).FirstOrDefault();

                lstTrackedHistory.Add(new KeyValuePair<int, string>(0, "None"));
                bTrackable = passport.CheckPermission(tableEntity.TableName.Trim(), (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Transfer);

                if ((bTrackable) || (tableEntity.TrackingTable > 0)) // tableEntity.Trackable
                {
                    oChildTable = _iTable.All().Where(x => (x.TableName) == (oView.TableName)).FirstOrDefault();

                    if (oChildTable is not null)
                    {
                        iCount = iCount + 1;
                        bool bChildTrackable = passport.CheckPermission(oChildTable.TableName.Trim(), (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Transfer);
                        if (bChildTrackable) // oChildTable.Trackable
                        {
                            lstTrackedHistory.Add(new KeyValuePair<int, string>(iCount, "History"));
                            mbHistory = true;
                        }
                        else
                        {
                            mbHistory = false;
                        }

                        var lstTrackingtbls = LoadTrackingTables();

                        if (oChildTable.TrackingTable > 0)
                        {
                            bool bContainerTrackable = false;

                            if (oChildTable.TrackingTable < lstTrackingtbls.Count)
                            {
                                foreach (var oContainer in lstTrackingtbls)
                                {
                                    bContainerTrackable = passport.CheckPermission(oContainer.TableName.Trim(), (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Transfer);
                                    if ((oContainer.TrackingTable > oChildTable.TrackingTable) && (bContainerTrackable)) // oContainer.Trackable
                                    {
                                        iCount = iCount + 1;
                                        lstTrackedHistory.Add(new KeyValuePair<int, string>(iCount, oContainer.UserName + " Contained"));
                                    }
                                }
                            }

                            foreach (var oContainer in _iTable.All())
                            {
                                bContainerTrackable = passport.CheckPermission(oContainer.TableName.Trim(), (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Transfer);
                                if ((oContainer.TrackingTable == 0) && (bContainerTrackable)) // oContainer.Trackable
                                {
                                    iCount = iCount + 1;
                                    lstTrackedHistory.Add(new KeyValuePair<int, string>(iCount, oContainer.UserName + " Contained"));
                                }
                            }

                            lstTrackedHistory.Add(new KeyValuePair<int, string>(9999, "All Contents Contained"));
                        }

                        oChildTable = null;
                    }
                }

                lstTrackedHisObj = JsonConvert.SerializeObject(lstTrackedHistory, Newtonsoft.Json.Formatting.Indented, Setting);
                tableEntityObjStr = JsonConvert.SerializeObject(tableEntity, Newtonsoft.Json.Formatting.Indented, Setting);
            }

            catch (Exception)
            {

            }

            return Json(new
            {
                viewColumnEntityObj,
                lstViewObjStr,
                lstTrackedHisObj,
                tableEntityObjStr,
                bTrackable,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

       [HttpPost]
        public IActionResult SetReportDefinitionValues(string pOldReportName, string pReportStyle)
        {

            int ViewId;
            bool bGrandTotal;
            bool bIncludeImages;
            bool bIncludeHeaders;
            bool bIncludeFooters;
            bool bPrintImageFullPage;
            bool bPrintImageFirstPageOnly;
            bool bIncludeAnnotations;
            bool bPrintDataRow;
            bool bIncludeImgFileInfo;
            bool bTrackingEverContained;
            int pLeftIndent;
            int pRightIndent;
            int pLeftMargin;
            int pRightMargin;
            string lblTableName = "";
            string lblReportName = "";
            string parentTblName = "";
            string parentViewName = "";
            // Dim childTblName As String = ""
            // Dim childViewName As String = ""
            string level3ChildTblName = "";
            string level3ViewName = "";
            string parentLevel2TblName = "";
            string parentLevel2ViewName = "";
            bool bIsWarnning = false;
            View pViewObject;
            int BaseSecureObjId = 0;
            int lSecureObjectId = 0;
            int ChildColumnHeaders = 0;
            bool bUpdateState = false;
            Array Ids;
            Array chksState;
            var lstViewColumns = new List<ViewColumn>();
            bool reportExists = false;
            int level1ID = 0;
            // Added on 26th April 2016.
            int trackingHistoryIndex;
            int printColumnHeadersIndex;
            bool viewStatus = false;
            IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            IRepository<View> _iView = new Repositories<View>();

            try
            {

                FormCollection formData = (FormCollection)httpContext.Request.Form;

                lblReportName = formData["ReportName"].ToString();

                for (int pIndex = 1; pIndex <= 3; pIndex++)
                {
                    
                    lstViewColumns = TempData.Get<List<ViewColumn>>("TempViewColumns_" + pIndex);
                    TempData.Keep("TempViewColumns_" + pIndex);

                    ViewId = Convert.ToInt32(formData["ViewID_Level" + pIndex.ToString()]);
                    lblTableName = "";
                    if (formData["TableName_Level" + pIndex].Count != 0)
                    {
                        lblTableName = formData["TableName_Level" + pIndex].ToString();
                    }

                    if (!(ViewId == 0))
                    {

                        if (!string.IsNullOrEmpty(lblTableName))
                        {
                            if (pIndex == 1)
                            {
                                parentTblName = lblTableName;
                                parentViewName = lblReportName;
                            }

                            if (pIndex == 2)
                            {
                                parentLevel2TblName = lblTableName;
                                parentLevel2ViewName = lblReportName;
                            }

                            if (pIndex == 1)
                            {
                                if ((lblReportName) != (pOldReportName))
                                {
                                    reportExists = _iView.All().Any(x => (x.ViewName) == (lblReportName));
                                }
                            }

                            // If _iView.All().Any(Function(x) x.TableName = lblTableName And x.Id = ViewId) = True Then
                            if (!reportExists)
                            {
                                var ViewEntity = _iView.All().Where(x => x.Id == ViewId).FirstOrDefault();

                                bGrandTotal = formData["GrandTotal_Level" + pIndex].Text() == "on"? true: false;
                                bIncludeImages = formData["PrintImages_Level" + pIndex].Text() == "on"? true: false;
                                bIncludeHeaders = formData["SuppressHeader_Level" + pIndex] == "on"? false: true;
                                bIncludeFooters = formData["SuppressFooter_Level" + pIndex] == "on"? false: true;
                                bPrintImageFullPage = formData["PrintImageFullPage_Level" + pIndex] == "on"? true: false;
                                bPrintImageFirstPageOnly = formData["PrintImageFirstPageOnly_Level" + pIndex] == "on"? true: false;
                                bIncludeAnnotations = formData["PrintImageRedlining_Level" + pIndex] == "on"? true: false;
                                bPrintDataRow = formData["SuppressImageDataRow_Level" + pIndex] == "on"? false: true;
                                bIncludeImgFileInfo = formData["SuppressImageFooter_Level" + pIndex] == "on"? false: true;
                                pLeftIndent = string.IsNullOrEmpty(formData["LeftIndent_Level" + pIndex])? 0: formData["LeftIndent_Level" + pIndex].IntValue();
                                pRightIndent = string.IsNullOrEmpty(formData["RightIndent_Level" + pIndex])? 0: formData["RightIndent_Level" + pIndex].IntValue();
                                pLeftMargin = string.IsNullOrEmpty(formData["PrintImageLeftMargin_Level" + pIndex])? 0: formData["PrintImageLeftMargin_Level" + pIndex].IntValue();
                                pRightMargin = string.IsNullOrEmpty(formData["PrintImageRightMargin_Level" + pIndex])? 0: formData["PrintImageRightMargin_Level" + pIndex].IntValue();
                                bTrackingEverContained = formData["TrackingEverContained_Level" + pIndex] == "on" ? true : false;
                                Ids = formData["ViewColList_Level" + pIndex][0].Split(",");
                                chksState = formData["ViewColChkStates_Level" + pIndex][0].Split(",");
                                // Added on 26th April 2016.
                                trackingHistoryIndex = formData["Level" + pIndex + "TrackingHis"].IntValue();
                                printColumnHeadersIndex = Convert.ToInt32(formData["rdPrintColHeaders_Level" + pIndex]);
                                ChildColumnHeaders = Convert.ToInt32(formData["rdPrintColHeaders" + (pIndex + 1)]);

                                if (pIndex == 1)
                                {
                                    if ((pOldReportName) != (lblReportName))
                                    {
                                        var pSecureObject = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals(pOldReportName) & x.SecureObjectTypeID == (int)Enums.SecureObjectType.Reports).FirstOrDefault();
                                        if (pSecureObject is not null)
                                        {
                                            _iSecureObject.BeginTransaction();
                                            // Changes by Ganesh - 12/01/2016
                                            pSecureObject.SecureObjectTypeID = (int)Enums.SecureObjectType.Reports;
                                            pSecureObject.Name = lblReportName;
                                            _iSecureObject.Update(pSecureObject);
                                            _iSecureObject.CommitTransaction();
                                        }
                                    }
                                }

                                ViewEntity.ViewName = lblReportName;
                                ViewEntity.ReportStylesId = pReportStyle;
                                ViewEntity.GrandTotal = bGrandTotal;
                                ViewEntity.PrintImages = bIncludeImages;
                                ViewEntity.SuppressHeader = bIncludeHeaders;
                                ViewEntity.SuppressFooter = bIncludeFooters;
                                ViewEntity.PrintImageFullPage = bPrintImageFullPage;
                                ViewEntity.PrintImageFirstPageOnly = bPrintImageFirstPageOnly;
                                ViewEntity.PrintImageRedlining = bIncludeAnnotations;
                                ViewEntity.SuppressImageDataRow = bPrintDataRow;
                                ViewEntity.SuppressImageFooter = bIncludeImgFileInfo;
                                ViewEntity.LeftIndent = pLeftIndent;
                                ViewEntity.RightIndent = pRightIndent;
                                ViewEntity.PrintImageLeftMargin = pLeftMargin;
                                ViewEntity.PrintImageRightMargin = pRightMargin;
                                ViewEntity.TrackingEverContained = bTrackingEverContained;
                                ViewEntity.ChildColumnHeaders = ChildColumnHeaders;
                                // ViewEntity.SubViewId = 0
                                // ViewEntity.SubTableName = ""

                                // Added on 26th April 2016.
                                if (trackingHistoryIndex > 0)
                                {
                                    ViewEntity.SubTableName = "<<Tracking>>";
                                    // ViewEntity.SubViewId = trackingHistoryIndex
                                }
                                // Added on 29th April 2016.
                                // If trackingHistoryIndex = 0 Then
                                // ViewEntity.SubTableName = ""
                                // 'ViewEntity.SubViewId = trackingHistoryIndex
                                // End If

                                if (pIndex == 2 | pIndex == 3)
                                {
                                    ViewEntity.PrintWithoutChildren = (bool?)Interaction.IIf(formData["PrintWithoutChildren_Level" + pIndex] == "on", true, false);
                                    // Added on 26th April 2016.
                                    // ViewEntity.ChildColumnHeaders = printColumnHeadersIndex
                                }
                                _iView.Update(ViewEntity);

                                // If pIndex = 2 Then
                                // UpdateLevelWiseReportsView(False, "", "", parentTblName, parentViewName, parentLevel2TblName, parentLevel2ViewName)
                                // End If

                                // If pIndex = 3 Then
                                // UpdateLevelWiseReportsView(False, "", "", parentTblName, parentViewName, lblTableName, lblReportName)
                                // End If
                                // Update View Columns
                                bUpdateState = true;
                                SaveViewColumns(Ids, chksState, lstViewColumns, bUpdateState, ViewEntity.Id, formData, pIndex);
                            }
                            else
                            {
                                Keys.ErrorType = "w";
                                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlAlreadyDefinedAsViewOrRpt"), lblReportName, lblTableName);
                                bIsWarnning = true;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(lblTableName))
                    {

                        if (pIndex == 1)
                        {
                            viewStatus = _iView.All().Any(x => (x.ViewName) == (lblReportName));
                        }

                        if (viewStatus == false)
                        {

                            bGrandTotal = formData["GrandTotal_Level" + pIndex] == "on"? true: false;
                            bIncludeImages = formData["PrintImages_Level" + pIndex] == "on"? true: false;
                            bIncludeHeaders = formData["SuppressHeader_Level" + pIndex] == "on"? false: true;
                            bIncludeFooters = formData["SuppressFooter_Level" + pIndex] == "on"? false: true;
                            bPrintImageFullPage = formData["PrintImageFullPage_Level" + pIndex] == "on"? true: false;
                            bPrintImageFirstPageOnly = formData["PrintImageFirstPageOnly_Level" + pIndex] == "on"? true: false;
                            bIncludeAnnotations = formData["PrintImageRedlining_Level" + pIndex] == "on"? true: false;
                            bPrintDataRow = formData["SuppressImageDataRow_Level" + pIndex] == "on"? false: true;
                            bIncludeImgFileInfo = formData["SuppressImageFooter_Level" + pIndex] == "on"? false: true;
                            pLeftIndent = string.IsNullOrEmpty(formData["LeftIndent_Level" + pIndex])? 0: formData["LeftIndent_Level" + pIndex].IntValue();
                            pRightIndent = string.IsNullOrEmpty(formData["RightIndent_Level" + pIndex])? 0: formData["RightIndent_Level" + pIndex].IntValue();
                            pLeftMargin = string.IsNullOrEmpty(formData["PrintImageLeftMargin_Level" + pIndex])? 0: formData["PrintImageLeftMargin_Level" + pIndex].IntValue();
                            pRightMargin = string.IsNullOrEmpty(formData["PrintImageRightMargin_Level" + pIndex])? 0: formData["PrintImageRightMargin_Level" + pIndex].IntValue();
                            bTrackingEverContained = formData["TrackingEverContained_Level" + pIndex] == "on" ? true : false;
                            Ids = formData["ViewColList_Level" + pIndex].ToString().Split(",");
                            chksState = formData["ViewColChkStates_Level" + pIndex].ToString().Split(",");
                            // Added on 26th April 2016.
                            trackingHistoryIndex = formData["Level" + pIndex + "TrackingHis"].IntValue();
                            printColumnHeadersIndex = formData["rdPrintColHeaders_Level" + pIndex].IntValue();
                            ChildColumnHeaders = formData["rdPrintColHeaders" + (pIndex + 1)].IntValue();

                            // Added on 03/03/2016
                            int initialViewId = formData["InitialViewID_Level" + pIndex].IntValue();
                            var initialViewEntity = _iView.All().Where(m => m.Id == initialViewId).FirstOrDefault();

                            pViewObject = new View();
                            pViewObject.TableName = lblTableName;
                            pViewObject.ViewName = lblReportName;
                            pViewObject.ReportStylesId = pReportStyle;
                            pViewObject.GrandTotal = bGrandTotal;
                            pViewObject.PrintImages = bIncludeImages;
                            pViewObject.SuppressHeader = bIncludeHeaders;
                            pViewObject.SuppressFooter = bIncludeFooters;
                            pViewObject.PrintImageFullPage = bPrintImageFullPage;
                            pViewObject.PrintImageFirstPageOnly = bPrintImageFirstPageOnly;
                            pViewObject.PrintImageRedlining = bIncludeAnnotations;
                            pViewObject.SuppressImageDataRow = bPrintDataRow;
                            pViewObject.SuppressImageFooter = bIncludeImgFileInfo;
                            pViewObject.LeftIndent = pLeftIndent;
                            pViewObject.RightIndent = pRightIndent;
                            pViewObject.PrintImageLeftMargin = pLeftMargin;
                            pViewObject.PrintImageRightMargin = pRightMargin;
                            pViewObject.TrackingEverContained = bTrackingEverContained;
                            pViewObject.Printable = true;
                            pViewObject.AltViewId = 0;
                            pViewObject.DeleteGridAvail = false;
                            pViewObject.FiltersActive = false;
                            pViewObject.RowHeight = (short?)0;
                            pViewObject.SQLStatement = "SELECT * FROM " + lblTableName;
                            pViewObject.TablesId = 0;
                            pViewObject.UseExactRowCount = false;
                            pViewObject.VariableColWidth = true;
                            pViewObject.VariableFixedCols = false;
                            pViewObject.VariableRowHeight = true;
                            pViewObject.ViewGroup = 0;
                            pViewObject.ViewOrder = 1;
                            pViewObject.ViewType = (short?)0;
                            pViewObject.Visible = true;
                            pViewObject.ChildColumnHeaders = ChildColumnHeaders;
                            pViewObject.DisplayMode = 1;
                            pViewObject.PrintAttachments = 0;
                            pViewObject.CustomFormView = Convert.ToBoolean(0);
                            // 25 is the new default as it is only used for Web Access 
                            // and new default of 5000 for desktop.  RVW 03/06/2019
                            pViewObject.MaxRecsPerFetch = pViewObject.MaxRecsPerFetch;
                            pViewObject.MaxRecsPerFetchDesktop = pViewObject.MaxRecsPerFetchDesktop;
                            pViewObject.InTaskList = false;
                            pViewObject.SearchableView = true;
                            pViewObject.SubViewId = 0;
                            pViewObject.WorkFlow1 = initialViewEntity.WorkFlow1;
                            pViewObject.RowHeight = initialViewEntity.RowHeight;
                            pViewObject.ViewGroup = initialViewEntity.ViewGroup;
                            pViewObject.MaxRecsPerFetch = initialViewEntity.MaxRecsPerFetch;
                            pViewObject.MaxRecsPerFetchDesktop = initialViewEntity.MaxRecsPerFetchDesktop;
                            pViewObject.WorkFlow1 = initialViewEntity.WorkFlow1;
                            pViewObject.WorkFlowDesc1 = initialViewEntity.WorkFlowDesc1;
                            pViewObject.WorkFlowToolTip1 = initialViewEntity.WorkFlowToolTip1;
                            pViewObject.TablesId = initialViewEntity.TablesId;
                            pViewObject.UseExactRowCount = initialViewEntity.UseExactRowCount;

                            // Added on 26th April 2016.
                            if (trackingHistoryIndex > 0)
                            {
                                pViewObject.SubTableName = "<<Tracking>>";
                            }

                            if (pIndex == 2 | pIndex == 3)
                            {
                                pViewObject.PrintWithoutChildren = formData["PrintWithoutChildren_Level" + pIndex] == "on" ? true : false;
                            }

                            _iView.Add(pViewObject);
                            if (pIndex == 1)
                            {
                                parentTblName = lblTableName;
                                parentViewName = lblReportName;

                                _iSecureObject.BeginTransaction();
                                var pNewSecureObject = new TabFusionRMS.Models.SecureObject();

                                var pSecureObjectData = _iSecureObject.All().FirstOrDefault(x => x.Name.Trim().ToLower().Equals(lblReportName) & x.SecureObjectTypeID == (int)Enums.SecureObjectType.Reports);
                                if (pSecureObjectData is not null)
                                {
                                    var pSecureObjectPermission = _iSecureObjectPermission.All().Where(x => x.SecureObjectID == pSecureObjectData.SecureObjectID);
                                    _iSecureObjectPermission.DeleteRange(pSecureObjectPermission);

                                    _iSecureObject.Delete(pSecureObjectData);
                                }

                                var pSecureObject = _iSecureObject.All().FirstOrDefault(x => x.Name.Trim().ToLower().Equals(lblTableName) & x.SecureObjectTypeID == (int)Enums.SecureObjectType.Table);
                                if (pSecureObject is not null)
                                {
                                    BaseSecureObjId = pSecureObject.SecureObjectID;
                                    pNewSecureObject.BaseID = pSecureObject.SecureObjectID;
                                    pNewSecureObject.SecureObjectTypeID = (int)Enums.SecureObjectType.Reports;
                                    pNewSecureObject.Name = lblReportName;
                                    _iSecureObject.Add(pNewSecureObject);
                                    _iSecureObject.CommitTransaction();

                                    AddSecureObjectPermissionsBySecureObjectType(pNewSecureObject.SecureObjectID, BaseSecureObjId, (int)Enums.SecureObjects.Reports);

                                    bUpdateState = false;
                                    level1ID = pViewObject.Id;
                                    SaveViewColumns(Ids, chksState, lstViewColumns, bUpdateState, pViewObject.Id, formData, pIndex);
                                }
                            }

                            else if (pIndex == 2)
                            {
                                parentLevel2TblName = lblTableName;
                                parentLevel2ViewName = lblReportName;

                                UpdateLevelWiseReportsView(lblReportName, pOldReportName, parentTblName, parentViewName, parentLevel2TblName, parentLevel2ViewName);

                                // Add columns for View
                                bUpdateState = false;
                                SaveViewColumns(Ids, chksState, lstViewColumns, bUpdateState, pViewObject.Id, formData, pIndex);
                            }
                            else if (pIndex == 3)
                            {
                                level3ChildTblName = lblTableName;
                                level3ViewName = lblReportName;

                                UpdateLevelWiseReportsView(lblReportName, pOldReportName, parentLevel2TblName, parentLevel2ViewName, level3ChildTblName, level3ViewName);

                                bUpdateState = false;
                                SaveViewColumns(Ids, chksState, lstViewColumns, bUpdateState, pViewObject.Id, formData, pIndex);
                            }
                        }

                        else
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlAlreadyDefinedAsViewOrRpt"), lblReportName, lblTableName);
                            bIsWarnning = true;
                        }
                    }
                }

                // Fill the secure permissions dataset after new report creation.
                passport.FillSecurePermissions();

                if (bIsWarnning == false)
                {
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlRptSaveSuccessfully");
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                        Trace.TraceInformation(string.Format(Languages.get_Translation("msgAdminCtrlProperty"), validationError.PropertyName, validationError.ErrorMessage));
                }
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                level1ID
            });
        }

        // Save the columns for Priting in ViewColumns.
        private bool SaveViewColumns(Array Ids, Array chksState, List<ViewColumn> lstViewColumns, bool bUpdateState, int vId, FormCollection formData, int iIndex)
        {
            ViewColumn ViewColumnObj;
            int icount = 0;
            IRepository<ViewColumn> _iViewColumn = new Repositories<ViewColumn>();
            _iViewColumn.BeginTransaction();

            // Delete all data of current view.
            var lSViewColEntities = _iViewColumn.All().Where(x => x.ViewsId == vId);
            _iViewColumn.DeleteRange(lSViewColEntities);

            // If bUpdateState Then
            // For Each id In Ids
            // ViewColumnObj = lstViewColumns.Where(Function(item) item.Id = id).FirstOrDefault()
            // ViewColumnObj.Id = 0
            // ViewColumnObj.SuppressPrinting = chksState(icount)
            // ViewColumnObj.ColumnNum = icount
            // ViewColumnObj.ViewsId = vId
            // _iViewColumn.Add(ViewColumnObj)
            // icount = icount + 1
            // Next
            // Else
            foreach (var id in Ids)
            {
                ViewColumnObj = lstViewColumns.Where(item => Operators.ConditionalCompareObjectEqual(item.Id, id, false)).FirstOrDefault();
                ViewColumnObj.Id = 0;
                ViewColumnObj.SuppressPrinting = chksState.GetValue(icount).ToString() == "0" ? false : true;
                ViewColumnObj.ColumnNum = (short?)icount;
                ViewColumnObj.ViewsId = vId;
                _iViewColumn.Add(ViewColumnObj);
                icount = icount + 1;
            }
            // End If
            _iViewColumn.CommitTransaction();

            return true;
        }

        // Add the Permission object for added report
        public bool AddSecureObjectPermissionsBySecureObjectType(int pSecureObjectID, int pBaseSecureObjectID, int pSecureObjectType)
        {
            bool bSucceed = false;
            try
            {
                string sSql = "INSERT INTO SecureObjectPermission (GroupID, SecureObjectID, PermissionID) SELECT GroupID," + pSecureObjectID + " AS SecureObjectId, PermissionID FROM SecureObjectPermission AS SecureObjectPermission WHERE     (SecureObjectID = " + pBaseSecureObjectID + ") AND (PermissionID IN (SELECT     PermissionID FROM          SecureObjectPermission AS SecureObjectPermission_1 WHERE (SecureObjectID = " + pSecureObjectType + ") AND (GroupID = 0)))";

                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
                bSucceed = Convert.ToBoolean(_IDBManager.ExecuteNonQuery(CommandType.Text, sSql));
                _IDBManager.Dispose();
            }
            catch (Exception)
            {
                return false;
            }
            return bSucceed;
        }

        // Remove the currently active tab after REMOVE THIS LEVEL button.
        public IActionResult RemoveActiveLevel(Array pViewIDs)
        {
            IRepository<View> _iView = new Repositories<View>();
            try
            {
                _iView.BeginTransaction();
                foreach (string item in pViewIDs)
                {
                    var parentView = _iView.All().Where(x => x.SubViewId == Convert.ToDouble(item)).FirstOrDefault();
                    if (parentView is not null)
                    {
                        parentView.SubTableName = "";
                        parentView.SubViewId = 0;
                        _iView.Update(parentView);
                    }

                    var viewEntity = _iView.All().Where(x => x.Id == Convert.ToDouble(item)).FirstOrDefault();

                    if (viewEntity is not null)
                    {
                        _iView.Delete(viewEntity);
                    }

                }
                _iView.CommitTransaction();

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlLvlRmvSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                _iView.RollBackTransaction();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public List<Table> LoadTrackingTables()
        {
            int iContainerNum;
            int iLowContainer;
            var lstTables = new List<Table>();
            Table oSaveTable = null;
            iLowContainer = 0;
            IRepository<Table> _iTable = new Repositories<Table>();
            var tableEntity = _iTable.All();

            do
            {
                iContainerNum = 9999;
                oSaveTable = null;

                foreach (Table oTable in tableEntity)
                {
                    if ((oTable.TrackingTable > iLowContainer) && (oTable.TrackingTable < iContainerNum))
                    {
                        iContainerNum = (int)oTable.TrackingTable;
                        oSaveTable = oTable;
                    }
                }

                if (oSaveTable is null)
                    break;
                lstTables.Add(oSaveTable);
                iLowContainer = (int)oSaveTable.TrackingTable;
            }
            while (true);

            return lstTables;

        }

        public bool NotSubReport(View oView, string pTableName)
        {
            bool NotSubReportRet = default;
            IRepository<View> _iView = new Repositories<View>();
            IRepository<Table> _iTable = new Repositories<Table>();
            var tableEntity = _iTable.All();
            View oTempView;
            var lViewEntities = _iView.All();
            object lLoopViewEntities = null;

            NotSubReportRet = true;

            foreach (var oTable in tableEntity)
            {
                lLoopViewEntities = lViewEntities.Where(x => (x.TableName.Trim().ToLower()) == (oTable.TableName));

                if (!(lLoopViewEntities == null))
                {
                    foreach (View currentOTempView in (IEnumerable)lLoopViewEntities)
                    {
                        oTempView = currentOTempView;
                        if (oTempView.SubViewId == oView.Id)
                        {
                            NotSubReportRet = false;
                            break;
                        }
                    }
                }
            }

            oTempView = null;
            return NotSubReportRet;
        }

        public object UpdateLevelWiseReportsView(string reportName, string oldReportName, string parentTblName, string parentViewName, string childTblName, string childViewName)
        {
            IRepository<View> _iView = new Repositories<View>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            try
            {
                // If bAdd Then
                // Dim parentViewEntity = _iView.All().Where(Function(x) x.TableName = parentTblName And x.ViewName = parentViewName).FirstOrDefault()
                // Dim childViewEntity = _iView.All().Where(Function(x) x.TableName = childTblName And x.ViewName = childViewName).FirstOrDefault()
                // If Not childViewEntity Is Nothing Then
                // parentViewEntity.SubTableName = childTblName
                // parentViewEntity.SubViewId = childViewEntity.Id
                // Else
                // parentViewEntity.SubTableName = ""
                // parentViewEntity.SubViewId = 0
                // End If
                // _iView.Update(parentViewEntity)
                // End If

                var parentViewEntity = _iView.All().Where(x => (x.TableName) == (parentTblName) & (x.ViewName) == (parentViewName)).FirstOrDefault();
                var childViewEntity = _iView.All().Where(x => (x.TableName) == (childTblName) & (x.ViewName) == (childViewName)).FirstOrDefault();
                if (childViewEntity is not null)
                {
                    parentViewEntity.SubTableName = childTblName;
                    parentViewEntity.SubViewId = childViewEntity.Id;
                }
                else
                {
                    parentViewEntity.SubTableName = "";
                    parentViewEntity.SubViewId = 0;
                }
                _iView.Update(parentViewEntity);

                var pSecureObject = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals(oldReportName) & x.SecureObjectTypeID == (int)Enums.SecureObjectType.Reports).FirstOrDefault();

                if (oldReportName != reportName)
                {
                    if (pSecureObject is not null)
                    {
                        _iSecureObject.BeginTransaction();
                        int pSecureObjectID = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals(childTblName) & x.SecureObjectTypeID == (int)Enums.SecureObjectType.Table).FirstOrDefault().SecureObjectID;
                        pSecureObject.BaseID = pSecureObjectID;
                        pSecureObject.SecureObjectTypeID = 7;
                        pSecureObject.Name = reportName;
                        _iSecureObject.Update(pSecureObject);
                        _iSecureObject.CommitTransaction();
                    }
                }
            }
            catch (Exception)
            {

            }
            return true;
        }
        #endregion

        #region Report Definitions

        public PartialViewResult LoadViewColumn()
        {
            return PartialView("_AddColumnViewPartial");
        }

        public IActionResult GetDataFromViewColumn(ViewsCustomModel lViewsCustomModelEntites, int viewColumnId, int LevelNum, string currentHeading, int viewId = 0, string tableName = "")
        {
            try
            {
                var viewColumnEntity = new ViewColumn();
                var DisplayStyleData = default(bool);
                var DuplicateType = default(bool);
                var CurrentViewColumn = new List<ViewColumn>();
                var editSetting = new Dictionary<string, bool>();
                if (lViewsCustomModelEntites.ViewsModel is not null)
                {
                    viewId = lViewsCustomModelEntites.ViewsModel.Id;
                }

                if (TempData is not null)
                {
                    if (viewId != 0)
                    {
                        CurrentViewColumn = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + viewId);
                        viewColumnEntity = CurrentViewColumn.Where(item => item.Id == viewColumnId & item.Heading.Trim().ToLower().Equals(currentHeading.Trim().ToLower())).FirstOrDefault();
                    }
                    else
                    {
                        CurrentViewColumn = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + LevelNum);
                        viewColumnEntity = CurrentViewColumn.Where(item => item.Id == viewColumnId & item.Heading.Trim().ToLower().Equals(currentHeading.Trim().ToLower())).FirstOrDefault();
                    }
                    this.ValidateEditSettingsOnEdit(ref editSetting, viewColumnEntity, CurrentViewColumn, tableName, lViewsCustomModelEntites.ViewsModel);
                }


                if (viewColumnEntity.LookupType == Convert.ToInt32(Enums.geViewColumnsLookupType.ltChildLookdownCommaDisplayDups))
                {
                    if (viewColumnEntity.LookupType == Convert.ToInt32(Enums.geViewColumnsLookupType.ltChildLookdownLFDisplayDups))
                    {
                        DisplayStyleData = true;
                        DuplicateType = false;
                    }
                    else if (viewColumnEntity.LookupType == Convert.ToInt32(Enums.geViewColumnsLookupType.ltChildLookdownCommaHideDups))
                    {
                        DisplayStyleData = false;
                        DuplicateType = true;
                    }
                    else if (viewColumnEntity.LookupType == Convert.ToInt32(Enums.geViewColumnsLookupType.ltChildLookdownLFHideDups))
                    {
                        DisplayStyleData = true;
                        DuplicateType = true;
                    }
                }
                else
                {
                    DisplayStyleData = false;
                    DuplicateType = false;
                }
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string viewColumnJSON = JsonConvert.SerializeObject(viewColumnEntity, Newtonsoft.Json.Formatting.Indented, Setting);
                string DisplayStyleDataJSON = JsonConvert.SerializeObject(DisplayStyleData, Newtonsoft.Json.Formatting.Indented, Setting);
                string DuplicateTypeJSON = JsonConvert.SerializeObject(DuplicateType, Newtonsoft.Json.Formatting.Indented, Setting);
                string editSettingsJSON = JsonConvert.SerializeObject(editSetting, Newtonsoft.Json.Formatting.Indented, Setting);
                Keys.ErrorType = "s";
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    DisplayStyleDataJSON,
                    DuplicateTypeJSON,
                    editSettingsJSON,
                    viewColumnJSON
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }

        }

        public IActionResult FillViewColumnControl(string TableName, bool viewFlag, int viewId = 0)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                IRepository<LookupType> _iLookupType = new Repositories<LookupType>();
                var tableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(TableName.Trim().ToLower())).FirstOrDefault();
                string tableVar = tableEntity.TableName;
                //var myBasePage = new BaseWebPage();
                var columnType = new List<KeyValuePair<int, string>>();
                var visualAttribute = new List<KeyValuePair<int, string>>();
                var allignment = new List<KeyValuePair<int, string>>();
                columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltDirect, Languages.get_Translation("ddlAdminCtrlDirect")));
                columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltRowNumber, Languages.get_Translation("ddlAdminCtrlRowNumber")));
                var RelationParentEntity = _iRelationShip.All().Where(m => m.LowerTableName.Trim().ToLower().Equals(tableVar.Trim().ToLower()));
                var RelationChildEntity = _iRelationShip.All().Where(m => m.UpperTableName.Trim().ToLower().Equals(tableVar.Trim().ToLower()));
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                if (RelationParentEntity is not null)
                {
                    foreach (RelationShip relationObj in RelationParentEntity)
                    {
                        string UpperTableVar = relationObj.UpperTableName;
                        var sADOConn = DataServices.DBOpen();
                        // Get ADO connection name
                        if (tableEntity is not null)
                        {
                            sADOConn = DataServices.DBOpen(tableEntity, _iDatabas.All());
                        }
                        var tableSchemaInfo = SchemaInfoDetails.GetTableSchemaInfo(tableVar, sADOConn);
                        if (tableSchemaInfo.Count > 1)
                        {
                            columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltLookup, Languages.get_Translation("ddlAdminCtrlParentLookup")));
                            break;
                        }
                    }
                }

                if (tableEntity is not null)
                {
                    if ((bool)tableEntity.Attachments)
                    {
                        columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltImageFlag, Languages.get_Translation("ddlAdminCtrlImageFlag")));
                        columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltFaxFlag, Languages.get_Translation("ddlAdminCtrlFaxFlag")));
                        columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltPCFilesFlag, Languages.get_Translation("ddlAdminCtrlPCFilesFlag")));
                        columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltAnyFlag, Languages.get_Translation("ddlAdminCtrlAnyAttachmentFlag")));
                    }
                    bool boolval = passport.CheckPermission(tableVar.Trim(), (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Transfer);
                    if (boolval)
                    {
                        columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltTrackingStatus, Languages.get_Translation("ddlAdminCtrlTrackingStatus")));
                        columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltTrackingLocation, Languages.get_Translation("ddlAdminCtrlTrackingLocation")));
                    }
                }

                if (RelationChildEntity is not null)
                {
                    if (RelationChildEntity.Count() > 0)
                    {
                        columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltChildrenFlag, Languages.get_Translation("ddlAdminCtrlChildFlag")));
                        columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltChildrenCounts, Languages.get_Translation("ddlAdminCtrlChildCount")));
                        columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltChildLookdownCommaDisplayDups, Languages.get_Translation("ddlAdminCtrlChildLookdown")));
                        columnType.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnsLookupType.ltChildLookdownTotals, Languages.get_Translation("ddlAdminCtrlChildTotals")));
                    }
                }
                var lookupEntity = _iLookupType.All().Where(m => m.LookupTypeForCode.Trim().ToUpper().Equals("CLMALN".Trim().ToUpper())).OrderBy(m => m.SortOrder);
                visualAttribute.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnDisplayType.cvAlways, Languages.get_Translation("ddlAdminCtrlAlwaysVisible")));
                visualAttribute.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnDisplayType.cvBaseTab, Languages.get_Translation("ddlAdminCtrlVisibleOnLevel1Only")));
                visualAttribute.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnDisplayType.cvPopupTab, Languages.get_Translation("ddlAdminCtrlVisibleOnLevel2AndBelow")));
                visualAttribute.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnDisplayType.cvNotVisible, Languages.get_Translation("ddlAdminCtrlNotVisible")));
                visualAttribute.Add(new KeyValuePair<int, string>((int)Enums.geViewColumnDisplayType.cvSmartColumns, Languages.get_Translation("ddlAdminCtrlSmartColumn")));
                if (lookupEntity is not null)
                {
                    foreach (LookupType lookupObj in lookupEntity)
                        allignment.Add(new KeyValuePair<int, string>(Convert.ToInt32(lookupObj.LookupTypeCode), lookupObj.LookupTypeValue));
                }
                // If (viewFlag = True) Then
                // Dim lstTEMPViewColumns As List(Of ViewColumn) = New List(Of ViewColumn)
                // If (viewId <> 0) Then
                // Dim viewColumnEntity = _iViewColumn.All().Where(Function(x) x.ViewsId = viewId).OrderByField("ColumnNum", True)
                // If (Not TempData.ContainsKey("TempViewColumns_" & viewId)) Then
                // If (Not (viewColumnEntity Is Nothing)) Then
                // For Each viewColObj As ViewColumn In viewColumnEntity
                // lstTEMPViewColumns.Add(viewColObj)
                // Next
                // If lstTEMPViewColumns.Count > 0 Then
                // TempData("TempViewColumns_" & viewId) = lstTEMPViewColumns
                // End If
                // End If
                // End If
                // Else
                // If (Not TempData.ContainsKey("TempViewColumns_" & viewId)) Then
                // TempData("TempViewColumns_" & viewId) = lstTEMPViewColumns
                // End If

                // End If
                // End If

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string columnTypeJSON = JsonConvert.SerializeObject(columnType, Newtonsoft.Json.Formatting.Indented, Setting);
                string visualAttributeJSON = JsonConvert.SerializeObject(visualAttribute, Newtonsoft.Json.Formatting.Indented, Setting);
                string allignmentJSON = JsonConvert.SerializeObject(allignment, Newtonsoft.Json.Formatting.Indented, Setting);
                Keys.ErrorType = "s";
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    columnTypeJSON,
                    allignmentJSON,
                    visualAttributeJSON
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });

            }
        }

        public ActionResult FillInternalFieldName(Enums.geViewColumnsLookupType ColumnTypeVar, string TableName, bool viewFlag, bool IsLocationChecked, string msSQL)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                IRepository<View> _iView = new Repositories<View>();
                var FieldNameList = new List<KeyValuePair<string, string>>();
                var tableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(TableName.Trim().ToLower())).FirstOrDefault();
                string TableVar = tableEntity.TableName;
                string DBConName = "";
                if (tableEntity is not null)
                {
                    DBConName = tableEntity.DBName;
                }
                var sADOConn = DataServices.DBOpen();
                if (tableEntity is not null)
                {
                    sADOConn = DataServices.DBOpen(tableEntity, _iDatabas.All());
                }
                string sSql = "";
                string sErrorMessage = "";
                int lError = 0;
                ADODB.Recordset rsADO = null;
                if (viewFlag)
                {
                    msSQL = msSQL;
                }
                else
                {
                    var oFirsView = _iView.All().Where(m => m.TableName.Trim().ToLower().Equals(TableName.Trim().ToLower()) && m.Printable == false).OrderBy(m => m.ViewOrder).FirstOrDefault();
                    if (oFirsView is not null)
                    {
                        msSQL = oFirsView.SQLStatement;
                    }
                }
                bool bIsAView = false;
                List<SchemaTable> schemaTableVar = (List<SchemaTable>)SchemaTable.GetSchemaTable(sADOConn, Enums.geTableType.AllTableTypes, TableVar);
                if (schemaTableVar is not null)
                {
                    if (schemaTableVar[0].TableType.Trim().ToLower().Equals("Views"))
                    {
                        bIsAView = true;
                    }
                }

                switch (ColumnTypeVar)
                {
                    case Enums.geViewColumnsLookupType.ltDirect:
                        {
                            sSql = DataServices.InjectWhereIntoSQL(msSQL, "0=1");
                            sADOConn = DataServices.DBOpen(tableEntity, _iDatabas.All());
                            rsADO = DataServices.GetADORecordset(sSql, tableEntity, _iDatabas.All(),MaxRecords: 1, bAccessSpecialServerSide:true , bReturnError: true, lError: ref lError, sErrorMessage: ref sErrorMessage);
                            string sBaseTableName = "";
                            string sTableName = "";
                            if (rsADO is not null)
                            {
                                for (int iCol = 0; rsADO.Fields.Count > iCol; iCol++)
                                {
                                    if (!SchemaInfoDetails.IsSystemField(Convert.ToString(rsADO.Fields[iCol].Name)))
                                    {
                                        if (Strings.InStr(Conversions.ToString(rsADO.Fields[iCol].Name), ".") == 0)
                                        {
                                            sBaseTableName = "";

                                            if (!(rsADO.Fields[iCol].Properties["BASETABLENAME"].Value is DBNull))
                                            {
                                                sBaseTableName = Strings.Replace(Strings.Replace(rsADO.Fields[0].Properties["BASETABLENAME"].Value.Text(), "[", ""), "]", "");
                                                sTableName = sBaseTableName;
                                                if (Strings.InStr(sTableName, " ") > 0)
                                                    sTableName = "[" + sTableName + "]";
                                            }

                                            if (Strings.Len(sBaseTableName) > 0L & Strings.StrComp(sBaseTableName, Strings.Replace(Strings.Replace(tableEntity.TableName, "[", ""), "]", ""), Constants.vbTextCompare) != 0)
                                            {
                                                if (!bIsAView)
                                                {
                                                    FieldNameList.Add(new KeyValuePair<string, string>(Strings.Trim(sTableName + "." + Strings.Trim(Convert.ToString(rsADO.Fields[iCol].Name))), Strings.Trim(sTableName + "." + Strings.Trim(Convert.ToString(rsADO.Fields[iCol].Name)))));
                                                }
                                                else
                                                {
                                                    var SchemaInfo = SchemaInfoDetails.GetSchemaInfo(tableEntity.TableName, sADOConn, Strings.Trim(rsADO.Fields[iCol].Name));
                                                    if (SchemaInfo.Count > 0)
                                                    {
                                                        FieldNameList.Add(new KeyValuePair<string, string>(Strings.Trim(tableEntity.TableName) + "." + Strings.Trim(Convert.ToString(rsADO.Fields[iCol].Name)), Strings.Trim(Convert.ToString(rsADO.Fields[iCol].Name))));
                                                    }
                                                    else
                                                    {
                                                        FieldNameList.Add(new KeyValuePair<string, string>(Strings.Trim(sTableName) + "." + Strings.Trim(Conversions.ToString(rsADO.Fields[iCol].Name)), Strings.Trim(sTableName) + "." + Strings.Trim(Convert.ToString(rsADO.Fields[iCol].Name))));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                FieldNameList.Add(new KeyValuePair<string, string>(Strings.Trim(tableEntity.TableName) + "." + Strings.Trim(Conversions.ToString(rsADO.Fields[iCol].Name)), Strings.Trim(Conversions.ToString(rsADO.Fields[iCol].Name))));
                                            }
                                        }
                                        else
                                        {
                                            FieldNameList.Add(new KeyValuePair<string, string>(Strings.Trim(tableEntity.TableName) + "." + Strings.Trim(Conversions.ToString(rsADO.Fields[iCol].Name)), Strings.Trim(Conversions.ToString(rsADO.Fields[iCol].Name))));
                                        }
                                    }
                                }
                                bool ShouldIncludeLocation;
                                if (viewFlag)
                                {
                                    ShouldIncludeLocation = IsLocationChecked;
                                }
                                else
                                {
                                    View ViewEntity;
                                    ViewEntity = _iView.All().Where(m => m.TableName.Trim().ToLower().Equals(TableVar.Trim().ToLower())).OrderBy(m => m.ViewOrder).FirstOrDefault();
                                    ShouldIncludeLocation = Conversions.ToBoolean(Interaction.IIf(ViewEntity.IncludeTrackingLocation is null, false, ViewEntity.IncludeTrackingLocation));
                                }
                                if (ShouldIncludeLocation)
                                {
                                    FieldNameList.Add(new KeyValuePair<string, string>(ReportsModel.TRACKED_LOCATION_NAME, ReportsModel.TRACKED_LOCATION_NAME));
                                }

                            }

                            break;
                        }


                    case Enums.geViewColumnsLookupType.ltLookup:
                        {
                            var relationShipEntity = _iRelationShip.All().Where(m => m.LowerTableName.Trim().ToLower().Equals(TableVar.Trim().ToLower())).OrderBy(m => m.TabOrder);
                            this.LoadFieldTable(ref FieldNameList, sADOConn, tableEntity, relationShipEntity, true, 1, false);
                            break;
                        }
                    case Enums.geViewColumnsLookupType.ltImageFlag:
                    case Enums.geViewColumnsLookupType.ltFaxFlag:
                    case Enums.geViewColumnsLookupType.ltPCFilesFlag:
                    case Enums.geViewColumnsLookupType.ltRowNumber:
                    case Enums.geViewColumnsLookupType.ltAnyFlag:
                        {
                            if (tableEntity.IdFieldName is not null)
                            {
                                string IdName = Strings.Trim(tableEntity.TableName + "." + DatabaseMap.RemoveTableNameFromField(tableEntity.IdFieldName));
                                FieldNameList.Add(new KeyValuePair<string, string>(IdName, tableEntity.IdFieldName));
                            }

                            break;
                        }
                    case Enums.geViewColumnsLookupType.ltChildrenCounts:
                    case Enums.geViewColumnsLookupType.ltChildrenFlag:
                        {
                            if (tableEntity.IdFieldName is not null)
                            {
                                string IdName = Strings.Trim(tableEntity.TableName + "." + DatabaseMap.RemoveTableNameFromField(tableEntity.IdFieldName));
                                FieldNameList.Add(new KeyValuePair<string, string>(IdName, IdName));
                            }
                            var ChildTable = _iRelationShip.All().Where(m => m.UpperTableName.Trim().ToLower().Equals(tableEntity.TableName.Trim().ToLower()));
                            foreach (RelationShip relationObj in ChildTable)
                            {
                                string lowerTableField = Strings.Trim(relationObj.LowerTableName + "." + DatabaseMap.RemoveTableNameFromField(relationObj.LowerTableFieldName));
                                FieldNameList.Add(new KeyValuePair<string, string>(lowerTableField, lowerTableField));
                            }

                            break;
                        }
                    case Enums.geViewColumnsLookupType.ltTrackingStatus:
                        {
                            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                            var systemEntity = _iSystem.All().OrderBy(m => m.Id).FirstOrDefault();
                            FieldNameList.Add(new KeyValuePair<string, string>("TrackingStatus.TransactionDateTime", "TrackingStatus.TransactionDateTime"));
                            if ((bool)systemEntity.TrackingOutOn)
                            {
                                FieldNameList.Add(new KeyValuePair<string, string>("TrackingStatus.Out", "TrackingStatus.Out"));
                                if ((bool)systemEntity.DateDueOn)
                                {
                                    FieldNameList.Add(new KeyValuePair<string, string>("TrackingStatus.DateDue", "TrackingStatus.DateDue"));
                                }
                            }
                            FieldNameList.Add(new KeyValuePair<string, string>("TrackingStatus.UserName", "TrackingStatus.UserName"));
                            break;
                        }
                    case Enums.geViewColumnsLookupType.ltTrackingLocation:
                        {
                            var TrackingTableEntity = _iTable.All().Where(m => m.TrackingTable > 0);
                            foreach (Table trackObj in TrackingTableEntity)
                            {
                                if (trackObj.TrackingTable > tableEntity.TrackingTable)
                                {
                                    string fieldId = Strings.Trim("TrackingStatus." + trackObj.TrackingStatusFieldName);
                                    FieldNameList.Add(new KeyValuePair<string, string>(fieldId, trackObj.TrackingStatusFieldName));
                                }
                                else
                                {
                                    string fieldId = Strings.Trim("TrackingStatus." + trackObj.TrackingStatusFieldName);
                                    FieldNameList.Add(new KeyValuePair<string, string>(fieldId, trackObj.TrackingStatusFieldName));
                                }
                            }

                            break;
                        }
                    case Enums.geViewColumnsLookupType.ltChildLookdownCommaDisplayDups:
                    case Enums.geViewColumnsLookupType.ltChildLookdownLFDisplayDups:
                    case Enums.geViewColumnsLookupType.ltChildLookdownCommaHideDups:
                    case Enums.geViewColumnsLookupType.ltChildLookdownLFHideDups:
                        {
                            var relationShipEntity = _iRelationShip.All().Where(m => m.UpperTableName.Trim().ToLower().Equals(TableVar.Trim().ToLower()));
                            this.LoadFieldTable(ref FieldNameList, sADOConn, tableEntity, relationShipEntity, false, 1, false);
                            break;
                        }
                    case Enums.geViewColumnsLookupType.ltChildLookdownTotals:
                        {
                            var relationShipEntity = _iRelationShip.All().Where(m => m.UpperTableName.Trim().ToLower().Equals(TableVar.Trim().ToLower()));
                            this.LoadFieldTable(ref FieldNameList, sADOConn, tableEntity, relationShipEntity, false, 1, true);
                            break;
                        }
                }

                Keys.ErrorType = "s";
                // Dim FieldName As New List(Of KeyValuePair(Of String, String))
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string FieldNameJSON = JsonConvert.SerializeObject(FieldNameList, Newtonsoft.Json.Formatting.Indented, Setting);
                return this.Json(new
                {
                    errortype = Keys.ErrorType,
                    FieldNameJSON
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                return this.Json(new { errortype = Keys.ErrorType });
            }
        }

        public void LoadFieldTable(ref List<KeyValuePair<string, string>> FieldNameList, ADODB.Connection sADOConn, Table orgTable, IQueryable<RelationShip> relationShipEntity, bool bDoUpper, int iLevel, bool bNumericOnly)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                var tableObjList = _iTable.All();
                var relationObjList = _iRelationShip.All();
                ReportsModel.FillViewColField(tableObjList, relationObjList, ref FieldNameList, sADOConn, orgTable, relationShipEntity, bDoUpper, iLevel, bNumericOnly);
            }
            catch (Exception)
            {

            }

        }

        public IActionResult FillFieldTypeAndSize(string TableVar, string FieldName)
        {
            try
            {
                string sFieldType = "";
                string sFieldSize = "";
                string sTableName = "";
                var sEditMaskLength = default(long);
                var sInputMaskLength = default(long);
                var sADOConn = DataServices.DBOpen();
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                var lDatabas = _iDatabas.All().OrderBy(m => m.Id);
                var oTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(TableVar.Trim().ToLower())).FirstOrDefault();
                if (Strings.InStr(FieldName, ".") > 1)
                {
                    sTableName = Strings.Left(FieldName, Strings.InStr(FieldName, ".") - 1);
                }
                else
                {
                    sTableName = oTable.TableName;
                }
                if (oTable is not null)
                {
                    ViewModel.GetFieldTypeAndSize(oTable, lDatabas, FieldName, ref sFieldType, ref sFieldSize);
                    sADOConn = DataServices.DBOpen(oTable, _iDatabas.All());
                }
                else
                {
                    ViewModel.BindTypeAndSize(sADOConn, FieldName, sTableName, ref sFieldType, ref sFieldSize, null);
                }
                string fieldType = sFieldType;
                // If (fieldType = Common.FT_DOUBLE Or fieldType = Common.FT_SHORT_INTEGER Or fieldType = Common.FT_LONG_INTEGER) Then
                // flagSubTotal = True
                // Else
                // flagSubTotal = False
                // End If
                this.SetMaskLength(sTableName, sADOConn, FieldName, ref sEditMaskLength, ref sInputMaskLength);
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string sFieldTypeJSON = JsonConvert.SerializeObject(sFieldType, Newtonsoft.Json.Formatting.Indented, Setting);
                string sFieldSizeJSON = JsonConvert.SerializeObject(sFieldSize, Newtonsoft.Json.Formatting.Indented, Setting);
                string sEditMaskLengthJSON = JsonConvert.SerializeObject(sEditMaskLength, Newtonsoft.Json.Formatting.Indented, Setting);
                string sInputMaskLengthJSON = JsonConvert.SerializeObject(sInputMaskLength, Newtonsoft.Json.Formatting.Indented, Setting);
                // Dim flagSubTotalJSON = JsonConvert.SerializeObject(flagSubTotal, Newtonsoft.Json.Formatting.Indented, Setting)
                Keys.ErrorType = "s";
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    sFieldTypeJSON,
                    sFieldSizeJSON,
                    sEditMaskLengthJSON,
                    sInputMaskLengthJSON
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                return Json(new { errortype = Keys.ErrorType });
            }
        }

        public void SetMaskLength(string tableName, ADODB.Connection sADOConn, string FieldName, ref long sEditMaskLength, ref long sInputMaskLength)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                string sTableName = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault().TableName;
                var sDefaultADOConn = DataServices.DBOpen();
                var sDataEditLength = default(long);
                var sDataInputLength = default(long);
                List<SchemaColumns> EditSchemaCol;
                EditSchemaCol = SchemaInfoDetails.GetSchemaInfo("ViewColumns", sDefaultADOConn, "EditMask");
                if (EditSchemaCol.Count > 0)
                {
                    sDataEditLength = (long)EditSchemaCol[0].CharacterMaxLength;
                }

                List<SchemaColumns> InputSchemaCol;
                InputSchemaCol = SchemaInfoDetails.GetSchemaInfo("ViewColumns", sDefaultADOConn, "InputMask");
                if (InputSchemaCol.Count > 0)
                {
                    sDataInputLength = (long)InputSchemaCol[0].CharacterMaxLength;
                }

                List<SchemaColumns> FieldSchemaCol;
                string sFieldName = "";

                if (Strings.InStr(FieldName, ".") > 1)
                {
                    int posCar = Strings.InStr(FieldName, ".");
                    sFieldName = FieldName.Substring(posCar);
                }
                sEditMaskLength = sDataEditLength;
                sInputMaskLength = sDataInputLength;
                FieldSchemaCol = SchemaInfoDetails.GetSchemaInfo(sTableName, sADOConn, sFieldName);
                if (FieldSchemaCol.Count > 0)
                {
                    if (FieldSchemaCol[0].IsString)
                    {
                        sEditMaskLength = (long)FieldSchemaCol[0].CharacterMaxLength;
                        sInputMaskLength = (long)FieldSchemaCol[0].CharacterMaxLength;
                    }
                }

                if (sDataEditLength < sEditMaskLength)
                {
                    sEditMaskLength = sDataEditLength;
                }
                if (sDataInputLength < sInputMaskLength)
                {
                    sInputMaskLength = sDataInputLength;
                }
            }
            catch (Exception)
            {
            }

        }

        public IActionResult SetColumnInTempViewCol(ViewColumn formEntity, int DisplayStyleData, int DuplicateType, string TableName, string reportNameOrId, int LevelNum, string SQLString, string? FieldNameTB = null, Enums.geViewColumnsLookupType LookupNumber = default, bool IsReportColumn = false, bool DropDownFlagBool = false, bool pPageBreakField = false, bool pSuppressDuplicates = false, bool pCountColumn = false, bool pSubtotalColumn = false, bool pRestartPageNumber = false, bool pUseAsPrintId = false, bool pSortableField = false, bool pFilterField = false, bool pEditAllowed = false, bool pColumnVisible = false, bool pDropDownSuggestionOnly = false, bool pMaskInclude = false)
        {
            string lstViewColumnJSON = "";
            IRepository<Table> _iTable = new Repositories<Table>();

            do
            {
                try
                {
                    bool sortFieldValue;
                    bool filterFieldValue;
                    if (IsReportColumn)
                    {
                        sortFieldValue = true;
                        filterFieldValue = true;
                    }
                    else
                    {
                        sortFieldValue = pSortableField;
                        filterFieldValue = pFilterField;
                    }
                    formEntity.DropDownFlag = Convert.ToInt16(DropDownFlagBool);
                    formEntity.EditAllowed = pEditAllowed;
                    formEntity.PageBreakField = pPageBreakField;
                    formEntity.SuppressDuplicates = pSuppressDuplicates;
                    formEntity.CountColumn = pCountColumn;
                    formEntity.SubtotalColumn = pSubtotalColumn;
                    formEntity.RestartPageNumber = pRestartPageNumber;
                    formEntity.UseAsPrintId = pUseAsPrintId;
                    formEntity.SortableField = pSortableField;
                    formEntity.FilterField = pFilterField;
                    formEntity.EditAllowed = pEditAllowed;
                    formEntity.ColumnVisible = pColumnVisible;
                    formEntity.DropDownSuggestionOnly = pDropDownSuggestionOnly;
                    formEntity.MaskInclude = pMaskInclude;
                    if (!string.IsNullOrEmpty(FieldNameTB))
                    {
                        formEntity.FieldName = FieldNameTB;
                    }

                    string msSql = "";
                    string sTempFieldName = string.Empty;
                    int iLookupColumn = 0;
                    // If (Not String.IsNullOrEmpty(TableName)) Then
                    // msSql = "SELECT * From [" + TableName + "]"
                    // End If
                    if (!string.IsNullOrEmpty(SQLString))
                    {
                        msSql = SQLString;
                    }

                    var tableEntity = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(TableName.Trim().ToLower())).FirstOrDefault();
                    ValidateAlternateField(formEntity, sortFieldValue, filterFieldValue, msSql, tableEntity, ref sTempFieldName);
                    if (!string.IsNullOrEmpty(sTempFieldName))
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = sTempFieldName;
                        break;
                    }
                    if (!this.TempData.ContainsKey("TempViewColumns_" + LevelNum))
                    {
                        var lViewColumnsList = new List<ViewColumn>();
                        TempData.Set<List<ViewColumn>>("TempViewColumns_" + LevelNum, lViewColumnsList);
                        //TempData["TempViewColumns_" + LevelNum] = lViewColumnsList;
                    }
                    if (TempData is not null)
                    {
                        var CurrentViewColumn = new List<ViewColumn>();
                        long viewColumnId = new long();
                        CurrentViewColumn = TempData.Get<List<ViewColumn>>("TempViewColumns_" + LevelNum);
                        if (!string.IsNullOrEmpty(reportNameOrId))
                        {
                            if (reportNameOrId.Contains("_"))
                            {
                                var viewColArray = reportNameOrId.Split('_');
                                viewColumnId = Convert.ToInt64(viewColArray[1]);
                            }
                        }
                        // Added New on date - 23rd June 2015.
                        if (CurrentViewColumn is not null)
                        {
                            if (IsReportColumn)
                            {
                                formEntity.SortableField = true;
                                formEntity.FilterField = true;
                            }
                            if (viewColumnId != 0L)
                            {
                                foreach (ViewColumn viewColObj in CurrentViewColumn)
                                {
                                    if (viewColObj.Id == viewColumnId)
                                    {
                                        formEntity.Id = (int)viewColumnId;
                                        formEntity.LookupType = Convert.ToInt16(LookupNumber);
                                        formEntity.FieldName = FieldNameTB;
                                        formEntity.ColumnNum = viewColObj.ColumnNum;
                                        formEntity.ColumnWidth = viewColObj.ColumnWidth;
                                        formEntity.SortOrder = viewColObj.SortOrder;
                                        formEntity.SortOrderDesc = viewColObj.SortOrderDesc;
                                        var viewColObjref = viewColObj;
                                        AddUpdateViewColumn(ref viewColObjref, formEntity);
                                        break;
                                    }
                                }
                                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlReUpdatedSuccessfully");
                                Keys.ErrorType = "s";
                            }
                            else
                            {

                                SetChildLookDown(ref formEntity, DisplayStyleData, DuplicateType);
                                SetLookupIdColOnAdd(ref iLookupColumn, formEntity, CurrentViewColumn, TableName, msSql, tableEntity);
                                formEntity.LookupIdCol = (short?)iLookupColumn;
                                int iMinNumber = -1;
                                if (CurrentViewColumn.Count != 0)
                                {
                                    iMinNumber = CurrentViewColumn.Min(x => x.Id);
                                    if (iMinNumber < 0)
                                    {
                                        iMinNumber = iMinNumber - 1;
                                    }
                                    else
                                    {
                                        iMinNumber = -1;
                                    }
                                }
                                formEntity.Id = iMinNumber;
                                if (!string.IsNullOrEmpty(formEntity.FieldName) & !string.IsNullOrEmpty(tableEntity.RetentionFieldName))
                                {
                                    if (formEntity.FieldName.Trim().ToLower().Equals(tableEntity.RetentionFieldName.Trim().ToLower()))
                                    {
                                        formEntity.DropDownFlag = (short?)-1;
                                    }
                                    else if (Convert.ToBoolean(formEntity.DropDownFlag.Value))
                                    {
                                        formEntity.DropDownFlag = (short?)-1;
                                    }
                                    else
                                    {
                                        formEntity.DropDownFlag = (short?)0;
                                    }
                                }
                                else if (Convert.ToBoolean(formEntity.DropDownFlag.Value))
                                {
                                    formEntity.DropDownFlag = (short?)-1;
                                }
                                else
                                {
                                    formEntity.DropDownFlag = (short?)0;
                                }

                                int ColumnCount = CurrentViewColumn.Count;
                                int pColumnNumber = ColumnCount;

                                // If ColumnCount > -1 Then
                                // Dim oView = _iView.All().Where(Function(x) x.TableName.ToLower().Trim().Equals(TableName.Trim().ToLower())).FirstOrDefault()
                                // pColumnNumber = _iViewColumn.All().Where(Function(x) x.ViewsId = oView.Id).Max(Function(x) x.ColumnNum) + 1
                                // End If

                                formEntity.ColumnNum = (short?)pColumnNumber; // ColumnCount
                                                                              // If (formEntity.LookupType = Enums.geViewColumnsLookupType.ltDirect) Then
                                                                              // formEntity.FieldName = DatabaseMap.RemoveTableNameFromField(formEntity.FieldName)
                                                                              // End If
                                formEntity.FieldName = formEntity.FieldName;

                                AddUpdateViewColumn(ref formEntity, formEntity);

                                CurrentViewColumn.Add(formEntity);

                                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlReColumnAddedSuccessfully");
                                Keys.ErrorType = "s";
                            }
                            if (CurrentViewColumn.Count > 0)
                            {
                                TempData.Set<List<ViewColumn>>("TempViewColumns_" + LevelNum, CurrentViewColumn);
                                //this.TempData["TempViewColumns_" + LevelNum] = CurrentViewColumn;
                                // TempData.Keep("TempViewColumns_" & LevelNum)
                            }
                        }


                    }
                    if (this.TempData is not null)
                    {
                        var Setting = new JsonSerializerSettings();
                        Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                        var lstViewColumn = new List<ViewColumn>();
                        lstViewColumn = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + LevelNum);
                        //lstViewColumn = (List<ViewColumn>)this.TempData.Peek("TempViewColumns_" + LevelNum);
                        lstViewColumnJSON = JsonConvert.SerializeObject(lstViewColumn, Newtonsoft.Json.Formatting.Indented, Setting);
                        Keys.ErrorType = "s";
                    }
                }

                catch (Exception)
                {
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                    Keys.ErrorType = "e";
                }
            }
            while (false);
            return Json(new
            {
                lstViewColumnJSON,
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public void ValidateAlternateField(ViewColumn moViewColumn, bool sortFieldValue, bool filterFieldValue, string msSQL, Table moTables, ref string sTempReturnMessage)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                string sSQL;
                int iWherePos;
                string sFieldName;
                string sErrorMessage = string.Empty;
                var lError = default(int);
                ADODB.Recordset rsTestSQL;
                bool bIsMemo;
                var oSchemaColumns = new SchemaColumns();
                var conObj = DataServices.DBOpen(moTables, _iDatabas.All());
                object sTempFieldName;
                if (sortFieldValue | filterFieldValue)
                {
                    sSQL = DataServices.NormalizeString(msSQL);
                    // sSQL = NormalizeString(msSQL)

                    if (Strings.InStr(1, sSQL, "SELECT DISTINCT TOP", Constants.vbTextCompare) <= 0L & Strings.InStr(1, sSQL, "SELECT DISTINCTROW TOP", Constants.vbTextCompare) <= 0L & Strings.InStr(1, sSQL, "SELECT TOP", Constants.vbTextCompare) <= 0L)
                    {
                        if (Strings.InStr(1, sSQL, "SELECT DISTINCT ", Constants.vbTextCompare) > 0L)
                        {
                            sSQL = Strings.Replace(sSQL, "SELECT DISTINCT ", "SELECT DISTINCT TOP " + Strings.Format(1) + " ", 1, 1, Constants.vbTextCompare);
                        }
                        else if (Strings.InStr(1, sSQL, "SELECT DISTINCTROW ", Constants.vbTextCompare) > 0L)
                        {
                            sSQL = Strings.Replace(sSQL, "SELECT DISTINCTROW ", "SELECT DISTINCTROW TOP " + Strings.Format(1) + " ", 1, 1, Constants.vbTextCompare);
                        }
                        else
                        {
                            sSQL = Strings.Replace(sSQL, "SELECT ", "SELECT TOP " + Strings.Format(1) + " ", 1, 1, Constants.vbTextCompare);
                        }
                    }

                    iWherePos = Strings.InStr(1, sSQL, "GROUP BY", Constants.vbTextCompare);
                    if (iWherePos == 0)
                        iWherePos = Strings.InStr(1, sSQL, "ORDER BY", Constants.vbTextCompare);
                    if (iWherePos > 0)
                        sSQL = Strings.Left(sSQL, (int)(iWherePos - 1L));

                    iWherePos = Strings.InStr(1, sSQL, " WHERE ", Constants.vbTextCompare);
                    if (iWherePos > 0)
                    {
                        sSQL = Strings.Left(sSQL, (int)(iWherePos - 1L));
                    }

                    sSQL = Strings.Trim(sSQL) + " WHERE (";
                    sTempFieldName = DatabaseMap.RemoveTableNameFromField(moViewColumn.FieldName);

                    if (Strings.StrComp(Convert.ToString(sTempFieldName), "SLFileRoomOrder", Constants.vbTextCompare) == 0)
                    {
                        sFieldName = "[" + moTables.TableName + "].[" + DatabaseMap.RemoveTableNameFromField(moTables.IdFieldName) + "]";
                        sSQL = sSQL + sFieldName + " IS NULL)";
                    }
                    else if (Strings.StrComp(Convert.ToString(sTempFieldName), ReportsModel.TRACKED_LOCATION_NAME, Constants.vbTextCompare) == 0)
                    {
                        // Dim trackingServiceObj As TrackingServices = New TrackingServices(_IDBManager, _iTable, _iDatabas, _iScanList, _iTabset, _iTabletab, _iRelationShip, _iView, _iSystem, _iTrackingHistory)
                        sSQL = TrackingServices.BuildTrackingLocationSQL(_iTable.All(), _iDatabas.All(), msSQL, ref moTables);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(Strings.Trim(moViewColumn.AlternateFieldName)))
                        {
                            sFieldName = "[" + moTables.TableName + "].[" + DatabaseMap.RemoveTableNameFromField(moViewColumn.FieldName) + "]";
                        }
                        else
                        {
                            sFieldName = moViewColumn.AlternateFieldName;
                        }

                        sSQL = sSQL + sFieldName + " IS NULL)";
                    }

                    rsTestSQL = DataServices.GetADORecordset(sSQL, moTables, _iDatabas.All(), Enums.CursorTypeEnum.rmOpenForwardOnly, MaxRecords: 1 ,bReturnError: true,lError: ref lError,sErrorMessage:ref sErrorMessage);
                    if (lError != 0L)
                    {
                        if (string.IsNullOrEmpty(Strings.Trim(moViewColumn.AlternateFieldName)))
                        {
                            sTempReturnMessage = string.Format(Languages.get_Translation("msgAdminCtrlAltFieldNameIsRequired"), Constants.vbCrLf, sErrorMessage);
                        }
                        else
                        {
                            sTempReturnMessage = string.Format(Languages.get_Translation("msgAdminCtrlAlternateFieldIsInvalid"), Constants.vbCrLf, sErrorMessage);
                        }
                        return;
                    }
                    else if (rsTestSQL is not null)
                    {
                        rsTestSQL.Close();
                        rsTestSQL = null;
                    }
                }
                // Validate the ORDER BY statement
                if (sortFieldValue)
                {
                    if (Strings.StrComp(moViewColumn.FieldName, "SLFileRoomOrder", Constants.vbTextCompare) != 0 & Strings.StrComp(moViewColumn.FieldName, "SLTrackedDestination", Constants.vbTextCompare) != 0)
                    {
                        sSQL = DataServices.NormalizeString(msSQL);

                        if (Strings.InStr(1, sSQL, "SELECT DISTINCT TOP", Constants.vbTextCompare) <= 0L & Strings.InStr(1, sSQL, "SELECT DISTINCTROW TOP", Constants.vbTextCompare) <= 0L & Strings.InStr(1, sSQL, "SELECT TOP", Constants.vbTextCompare) <= 0L)
                        {
                            if (Strings.InStr(1, sSQL, "SELECT DISTINCT ", Constants.vbTextCompare) > 0L)
                            {
                                sSQL = Strings.Replace(sSQL, "SELECT DISTINCT ", "SELECT DISTINCT TOP " + Strings.Format(1) + " ", 1, 1, Constants.vbTextCompare);
                            }
                            else if (Strings.InStr(1, sSQL, "SELECT DISTINCTROW ", Constants.vbTextCompare) > 0L)
                            {
                                sSQL = Strings.Replace(sSQL, "SELECT DISTINCTROW ", "SELECT DISTINCTROW TOP " + Strings.Format(1) + " ", 1, 1, Constants.vbTextCompare);
                            }
                            else
                            {
                                sSQL = Strings.Replace(sSQL, "SELECT ", "SELECT TOP " + Strings.Format(1) + " ", 1, 1, Constants.vbTextCompare);
                            }
                        }

                        if (Strings.InStr(1, sSQL, "ORDER BY", Constants.vbTextCompare) == 0)
                        {
                            iWherePos = Strings.InStr(1, sSQL, " WHERE ", Constants.vbTextCompare);
                            if (iWherePos > 0)
                                sSQL = Strings.Left(sSQL, iWherePos);
                            sSQL = Strings.Trim(sSQL) + " WHERE (0=1)";

                            if (string.IsNullOrEmpty(Strings.Trim(moViewColumn.AlternateFieldName)))
                            {
                                lError = 0;
                                // 01/16/2003 MEF - Added. Fixed SetViewSQL in frmLibrarian to add CONVERT(VARCHAR, to Order by so it can be sortable.
                                bIsMemo = false;
                                if (moTables is not null)
                                {
                                    if (!string.IsNullOrEmpty(moViewColumn.FieldName))
                                    {
                                        var oSchemaColumnsList = SchemaInfoDetails.GetSchemaInfo(moTables.TableName, conObj, DatabaseMap.RemoveTableNameFromField(moViewColumn.FieldName));
                                        if (oSchemaColumnsList.Count != 0)
                                        {
                                            oSchemaColumns = oSchemaColumnsList[0];
                                        }
                                        // oSchemaColumns = oTmpTables.ColumnSchema(RemoveTableNameFromField(txtFieldName.Text))
                                        if (oSchemaColumns is not null)
                                        {
                                            if (oSchemaColumns.IsString & (oSchemaColumns.CharacterMaxLength <= 0 | oSchemaColumns.CharacterMaxLength > 8000))
                                            {
                                                bIsMemo = true;
                                            }

                                            oSchemaColumns = null;
                                        }
                                        if (bIsMemo)
                                        {
                                            sSQL = sSQL + " ORDER BY CONVERT(VARCHAR(8000), [" + Strings.Trim(Strings.Replace(moViewColumn.FieldName, ".", "].[")) + "])";
                                        }
                                        else
                                        {
                                            sSQL = sSQL + " ORDER BY [" + Strings.Trim(Strings.Replace(moViewColumn.FieldName, ".", "].[")) + "]";
                                        }
                                    }


                                }

                                rsTestSQL = DataServices.GetADORecordset(sSQL, moTables, _iDatabas.All(), Enums.CursorTypeEnum.rmOpenForwardOnly,MaxRecords: 1 ,bReturnError: true,lError: ref lError,sErrorMessage: ref sErrorMessage);
                                // rsTestSQL = theApp.Data.GetADORecordset(sSQL, moTables.TableName, CursorTypeEnum_rmOpenForwardOnly, , , 1, , True, lError, sErrorMessage)

                                if (lError != 0L)
                                {
                                    sTempReturnMessage = string.Format(Languages.get_Translation("msgAdminCtrlAltFieldNameIsRequired"), Constants.vbCrLf, sErrorMessage);
                                    return;
                                }
                                else if (rsTestSQL is not null)
                                {
                                    rsTestSQL.Close();
                                    rsTestSQL = null;
                                }
                            }
                            else
                            {
                                lError = 0;
                                sSQL = sSQL + " ORDER BY " + moViewColumn.AlternateFieldName;
                                rsTestSQL = DataServices.GetADORecordset(sSQL, moTables, _iDatabas.All(), Enums.CursorTypeEnum.rmOpenForwardOnly,MaxRecords: 1, bReturnError: true, lError: ref lError, sErrorMessage: ref sErrorMessage);
                                // rsTestSQL = theApp.Data.GetADORecordset(sSQL, moTables.TableName, , , , 1, , True, lError, sErrorMessage)

                                if (lError != 0L)
                                {
                                    // CenterPopupOnForm hWnd
                                    sTempReturnMessage = string.Format(Languages.get_Translation("msgAdminCtrlAlternateFieldIsInvalid"), Constants.vbCrLf, sErrorMessage);
                                    return;
                                }
                                else if (rsTestSQL is not null)
                                {
                                    rsTestSQL.Close();
                                    rsTestSQL = null;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sTempReturnMessage = ex.Message.ToString();
            }

        }

        public void SetChildLookDown(ref ViewColumn formEntity, int DisplayStyleData, int DuplicateType)
        {
            if (formEntity.LookupType == Convert.ToInt32(Enums.geViewColumnsLookupType.ltChildLookdownCommaDisplayDups))
            {
                if (DisplayStyleData == 0 & DuplicateType == 1)
                {
                    formEntity.LookupType = Convert.ToInt16(Enums.geViewColumnsLookupType.ltChildLookdownCommaHideDups);
                }
                else if (DisplayStyleData == 0 & DuplicateType == 0)
                {
                    formEntity.LookupType = Convert.ToInt16(Enums.geViewColumnsLookupType.ltChildLookdownCommaDisplayDups);
                }
                else if (DisplayStyleData == 1 & DuplicateType == 1)
                {
                    formEntity.LookupType = Convert.ToInt16(Enums.geViewColumnsLookupType.ltChildLookdownLFHideDups);
                }
                else if (DisplayStyleData == 1 & DuplicateType == 0)
                {
                    formEntity.LookupType = Convert.ToInt16(Enums.geViewColumnsLookupType.ltChildLookdownLFDisplayDups);
                }
            }
            else
            {
                formEntity.LookupType = formEntity.LookupType;
            }
        }

        public void AddUpdateViewColumn(ref ViewColumn viewColumnEntity, ViewColumn formEntity)
        {
            viewColumnEntity.Id = formEntity.Id;
            if (formEntity.ViewsId is not null)
            {
                viewColumnEntity.ViewsId = formEntity.ViewsId;
            }
            if (formEntity.FieldName is not null)
            {
                viewColumnEntity.FieldName = formEntity.FieldName;
            }
            if (formEntity.Heading is not null)
            {
                viewColumnEntity.Heading = formEntity.Heading;
            }
            if (formEntity.LookupType is not null)
            {
                viewColumnEntity.LookupType = formEntity.LookupType;
            }
            viewColumnEntity.EditMask = formEntity.EditMask;
            viewColumnEntity.AlternateFieldName = formEntity.AlternateFieldName;
            if (formEntity.DropDownFlag is not null)
            {
                viewColumnEntity.DropDownFlag = formEntity.DropDownFlag;
            }
            viewColumnEntity.MaskPromptChar = formEntity.MaskPromptChar;
            if (formEntity.ColumnNum is not null)
            {
                viewColumnEntity.ColumnNum = formEntity.ColumnNum;
            }
            if (formEntity.MaxPrintLines is not null)
            {
                viewColumnEntity.MaxPrintLines = formEntity.MaxPrintLines;
            }
            else
            {
                viewColumnEntity.MaxPrintLines = 0;
            }
            viewColumnEntity.InputMask = formEntity.InputMask;
            if (formEntity.LookupIdCol is not null)
            {
                viewColumnEntity.LookupIdCol = formEntity.LookupIdCol;
            }
            if (formEntity.ColumnWidth is null)
            {
                viewColumnEntity.ColumnWidth = (short?)3000;
            }
            else
            {
                viewColumnEntity.ColumnWidth = formEntity.ColumnWidth;
            }
            viewColumnEntity.ColumnOrder = formEntity.ColumnOrder;
            viewColumnEntity.ColumnStyle = formEntity.ColumnStyle;
            viewColumnEntity.ColumnVisible = formEntity.ColumnVisible;
            viewColumnEntity.SortableField = formEntity.SortableField;
            viewColumnEntity.FilterField = formEntity.FilterField;
            viewColumnEntity.EditAllowed = formEntity.EditAllowed;
            viewColumnEntity.DropDownSuggestionOnly = formEntity.DropDownSuggestionOnly;
            viewColumnEntity.MaskInclude = formEntity.MaskInclude;
            viewColumnEntity.CountColumn = formEntity.CountColumn;
            viewColumnEntity.SubtotalColumn = formEntity.SubtotalColumn;
            viewColumnEntity.PrintColumnAsSubheader = false;
            viewColumnEntity.RestartPageNumber = formEntity.RestartPageNumber;
            viewColumnEntity.UseAsPrintId = formEntity.UseAsPrintId;
            viewColumnEntity.SuppressPrinting = formEntity.SuppressPrinting;
            viewColumnEntity.ValueCount = false;
            viewColumnEntity.DropDownReferenceColNum = (short?)0;
            viewColumnEntity.FormColWidth = 0;
            viewColumnEntity.MaskClipMode = false;
            viewColumnEntity.SortOrderDesc = formEntity.SortOrderDesc;
            viewColumnEntity.SuppressDuplicates = formEntity.SuppressDuplicates;
            viewColumnEntity.VisibleOnForm = true;
            viewColumnEntity.VisibleOnPrint = true;
            viewColumnEntity.PageBreakField = formEntity.PageBreakField;

            viewColumnEntity.FreezeOrder = 0;
            viewColumnEntity.AlternateSortColumn = 0;
            viewColumnEntity.PrinterColWidth = 0;
            viewColumnEntity.SortOrder = formEntity.SortOrder;
            viewColumnEntity.LabelJustify = 0;
            viewColumnEntity.LabelLeft = 0;
            viewColumnEntity.LabelTop = 0;
            viewColumnEntity.LabelWidth = 0;
            viewColumnEntity.LabelHeight = 0;
            viewColumnEntity.ControlLeft = 0;
            viewColumnEntity.ControlTop = 0;
            viewColumnEntity.ControlWidth = 0;
            viewColumnEntity.ControlHeight = 0;
            viewColumnEntity.TabOrder = 0;
            // viewColumnEntity.ColumnWidth = 3000
            viewColumnEntity.SortField = default;

        }

        public IActionResult ValidateViewColEditSetting(ViewsCustomModel viewCustModel, string TableName, Enums.geViewColumnsLookupType LookupType, string FieldName, string FieldType)
        {
            try
            {
                IRepository<Table> _iTable = new Repositories<Table>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                bool mbLookup;
                var editSettings = new Dictionary<string, bool>();
                var lError = default(long);
                ADODB.Recordset rsADO = null;
                string sSql = "";
                ADODB.Connection sADOConn;
                ADODB.Field oFields = null;
                string msSQL = "";
                editSettings.Add("Capslock", true);
                editSettings.Add("Editable", true);
                editSettings.Add("Filterable", true);
                editSettings.Add("Sortable", true);
                editSettings.Add("DropDown", true);
                editSettings.Add("DropDownSuggestionOnly", true);
                editSettings.Add("MaskIncludeDB", true);
                editSettings.Add("SubTotal", true);
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                var oTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(TableName.Trim().ToLower())).FirstOrDefault();
                if (oTable is not null)
                {
                    // editSettings("DropDown") = False
                    // editSettings("DropDownSuggestionOnly") = False
                    var oRelation = _iRelationShip.All().Where(m => m.LowerTableName.Trim().ToLower().Equals(oTable.TableName.Trim().ToLower()));
                    if (oRelation is not null)
                    {
                        mbLookup = false;
                        foreach (RelationShip relationObj in oRelation)
                        {
                            if (DatabaseMap.RemoveTableNameFromField(relationObj.LowerTableFieldName).Trim().ToLower().Equals(DatabaseMap.RemoveTableNameFromField(FieldName).Trim().ToLower()))
                            {
                                mbLookup = true;
                                break;
                            }
                        }
                        editSettings["DropDown"] = mbLookup;
                        editSettings["DropDownSuggestionOnly"] = mbLookup;
                    }
                }
                if (viewCustModel.ViewsModel is null)
                {
                    msSQL = "Select * From [" + TableName + "]";
                }
                else
                {
                    msSQL = viewCustModel.ViewsModel.SQLStatement;
                }
                sSql = DataServices.InjectWhereIntoSQL(msSQL, "0=1");
                sADOConn = DataServices.DBOpen(oTable, _iDatabas.All());
                string sErrorMessage = "";
                int arglError = (int)lError;
                rsADO = DataServices.GetADORecordset(sSql, oTable, _iDatabas.All() , MaxRecords: 1,bAccessSpecialServerSide: true,bReturnError: true,lError: ref arglError,sErrorMessage: ref sErrorMessage, bCheckIfDisconnected: false);
                lError = arglError;
                // rsADO = DataServices.GetADORecordset(sSql, sADOConn)
                if (rsADO is not null)
                {
                    if (rsADO is not null)
                    {
                        oFields = ViewModel.FieldWithOrWithoutTable(FieldName, rsADO.Fields, false);
                    }
                }

                if (lError == 0L)
                {
                    if (oFields is not null)
                    {
                        if (SchemaInfoDetails.IsADateType((Enums.DataTypeEnum)oFields.Type))
                        {
                            editSettings["Capslock"] = false;
                            editSettings["SubTotal"] = false;
                        }
                        else if (SchemaInfoDetails.IsAStringType((Enums.DataTypeEnum)oFields.Type))
                        {
                            editSettings["SubTotal"] = false;
                        }
                        else
                        {
                            editSettings["Capslock"] = false;
                            switch (oFields.Type)
                            {
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBoolean:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedTinyInt:
                                    {
                                        editSettings["SubTotal"] = false;
                                        break;
                                    }
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmDouble:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmCurrency:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmDecimal:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmNumeric:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmSingle:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmVarNumeric:
                                    {
                                        editSettings["SubTotal"] = true;
                                        break;
                                    }
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBigInt:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedBigInt:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmInteger:
                                    {
                                        if (Convert.ToBoolean(oFields.Properties["ISAUTOINCREMENT"].Value))
                                        {
                                            editSettings["Editable"] = false;
                                            editSettings["SubTotal"] = false;
                                        }
                                        else
                                        {
                                            int boolVal = Strings.StrComp(DatabaseMap.RemoveTableNameFromField(oFields.Name), DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), (CompareMethod)Convert.ToInt32(Constants.vbTextCompare == 0));
                                            if (!string.IsNullOrEmpty(oTable.CounterFieldName) & boolVal == 0)
                                            {
                                                editSettings["SubTotal"] = false;
                                            }
                                            else
                                            {
                                                editSettings["SubTotal"] = true;
                                            }
                                        }

                                        break;
                                    }
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBinary:
                                    {
                                        editSettings["Editable"] = false;
                                        editSettings["SubTotal"] = false;
                                        break;
                                    }
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmSmallInt:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmTinyInt:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedInt:
                                case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedSmallInt:
                                    {
                                        editSettings["SubTotal"] = true;
                                        break;
                                    }
                            }
                        }
                        rsADO.Close();
                        rsADO = null;
                    }
                    else
                    {
                        editSettings["SubTotal"] = false;
                    }
                }
                else
                {
                    editSettings["SubTotal"] = false;
                }
                if (LookupType == Enums.geViewColumnsLookupType.ltLookup)
                {
                    lError = 1L;
                    lError = Convert.ToInt64(ReportsModel.mcLevel[FieldName]);
                    if (lError == 1L)
                    {
                        editSettings["DropDown"] = true;
                        editSettings["Editable"] = false;
                    }
                    else
                    {
                        editSettings["DropDown"] = false;
                        editSettings["Editable"] = false;
                    }
                    editSettings["DropDownSuggestionOnly"] = false;
                }
                if (LookupType != Enums.geViewColumnsLookupType.ltDirect)
                {
                    if (LookupType != Enums.geViewColumnsLookupType.ltLookup)
                    {
                        editSettings["Editable"] = false;
                    }
                    editSettings["Sortable"] = false;
                    editSettings["Filterable"] = false;
                    editSettings["MaskIncludeDB"] = false;
                    editSettings["Capslock"] = false;
                }
                if (!string.IsNullOrEmpty(oTable.RetentionFieldName))
                {
                    if (DatabaseMap.RemoveTableNameFromField(oTable.RetentionFieldName).Trim().ToLower().Equals(DatabaseMap.RemoveTableNameFromField(FieldName).Trim().ToLower()))
                    {
                        if (oTable.RetentionAssignmentMethod is not null)
                        {
                            if ((Int32)oTable.RetentionAssignmentMethod == (Int32)Enums.meRetentionCodeAssignment.rcaCurrentTable || (Int32)oTable.RetentionAssignmentMethod == (Int32)Enums.meRetentionCodeAssignment.rcaRelatedTable)
                            {
                                editSettings["Editable"] = false;
                            }
                        }
                    }
                }
                if (editSettings["DropDown"] == false)
                {
                    editSettings["DropDownSuggestionOnly"] = false;
                }
                // FUS-5789
                if (editSettings["DropDown"] == false)
                {
                    if (oTable is not null)
                    {
                        var oRelation = _iRelationShip.All().Where(m => m.LowerTableName.Trim().ToLower().Equals(oTable.TableName.Trim().ToLower()));
                        if (oRelation is not null)
                        {
                            foreach (RelationShip relationObj in oRelation)
                            {
                                if (relationObj.UpperTableFieldName.Split('.')[0].Trim().ToLower().Equals(FieldName.Split('.')[0].Trim().ToLower()))
                                {
                                    editSettings["Editable"] = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                Keys.ErrorType = "s";
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string editSettingsJSON = JsonConvert.SerializeObject(editSettings, Newtonsoft.Json.Formatting.Indented, Setting);
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    editSettingsJSON
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                return Json(new { errortype = Keys.ErrorType });
            }
        }


        public void ValidateEditSettingsOnEdit(ref Dictionary<string, bool> editSettingList, ViewColumn viewColumnEntity, List<ViewColumn> CurrentViewColumn, string tableName, View oView)
        {
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<Table> _iTable = new Repositories<Table>();

            try
            {
                bool bIsSecondLevel;
                var mbLocalLookup = default(bool);
                bool bLocked;
                var sADOConn = new ADODB.Connection();
                editSettingList.Add("Capslock", true);
                editSettingList.Add("Editable", true);
                editSettingList.Add("Filterable", true);
                editSettingList.Add("Sortable", true);
                // editSettingList.Add("DropDown", True)
                // editSettingList.Add("DropDownSuggestionOnly", True)
                editSettingList.Add("MaskIncludeDB", true);
                editSettingList.Add("DropDown", false);
                editSettingList.Add("DropDownSuggestionOnly", false);
                editSettingList.Add("SubTotal", true);
                var moTable = new Table();
                IRepository<View> _iView = new Repositories<View>();
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                if (viewColumnEntity is not null)
                {
                    // Dim tableName = _iView.All.Where(Function(m) m.Id = viewColumnEntity.ViewsId).FirstOrDefault.TableName
                    moTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                    switch (viewColumnEntity.LookupType)
                    {
                        case 1:
                            {
                                bIsSecondLevel = true;
                                if ((viewColumnEntity.LookupIdCol >= 0) && (viewColumnEntity.LookupIdCol < CurrentViewColumn.Count))
                                {
                                    var tempViewCol = CurrentViewColumn.Where(m => m.ColumnNum == viewColumnEntity.LookupIdCol).FirstOrDefault();
                                    // Dim tempViewCol = CurrentViewColumn.Item(viewColumnEntity.LookupIdCol + 1)
                                    if (tempViewCol is not null)
                                    {
                                        bIsSecondLevel = (tempViewCol.LookupType != Convert.ToInt32(Enums.geViewColumnsLookupType.ltDirect));
                                    }
                                }
                                if (!bIsSecondLevel)
                                {
                                    editSettingList["DropDown"] = true;
                                    editSettingList["Editable"] = false;
                                }
                                else
                                {
                                    editSettingList["DropDown"] = false;
                                    editSettingList["Editable"] = false;
                                }

                                break;
                            }

                        case 12:
                        case 14:
                        case 13:
                        case 15:
                        case 17:
                            {
                                var childTable = new Table();
                                if (viewColumnEntity.LookupIdCol > -1)
                                {
                                    if ((viewColumnEntity.LookupIdCol >= 0) && (viewColumnEntity.LookupIdCol < CurrentViewColumn.Count))
                                    {
                                        var tempViewCol = CurrentViewColumn.Where(m => m.ColumnNum == viewColumnEntity.LookupIdCol).FirstOrDefault();
                                        string TempTableName = DatabaseMap.RemoveFieldNameFromField(tempViewCol.FieldName);
                                        childTable = (Table)_iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(TempTableName.Trim().ToLower()));
                                        if (childTable is not null)
                                        {
                                            editSettingList["DropDown"] = true;
                                        }
                                    }
                                }
                                else
                                {
                                    string TempTableName = DatabaseMap.RemoveFieldNameFromField(viewColumnEntity.FieldName);
                                    childTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(TempTableName.Trim().ToLower())).FirstOrDefault();
                                    if (childTable is not null)
                                    {
                                        var ParentTable = _iRelationShip.All().Where(m => m.LowerTableName.Trim().ToLower().Equals(childTable.TableName.Trim().ToLower()));
                                        if (ParentTable is not null)
                                        {
                                            foreach (RelationShip relationObj in ParentTable)
                                            {
                                                if (relationObj.LowerTableFieldName.Trim().ToLower().Equals(viewColumnEntity.FieldName.Trim().ToLower()))
                                                {
                                                    if (!relationObj.UpperTableName.Trim().ToLower().Equals(moTable.TableName.Trim().ToLower()))
                                                    {
                                                        editSettingList["DropDown"] = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (editSettingList["DropDown"])
                                {
                                    if (childTable is not null)
                                    {
                                        if (string.IsNullOrEmpty(Strings.Trim(childTable.CounterFieldName)))
                                        {
                                            // Get ADO connection name
                                            if (childTable is not null)
                                            {
                                                // Dim dbObj = _iDatabas.All.Where(Function(m) m.DBName.Trim.ToLower.Equals(DBConName.Trim.ToLower)).FirstOrDefault
                                                sADOConn = DataServices.DBOpen(childTable, _iDatabas.All());
                                            }
                                            string sSql = "SELECT [" + Strings.Replace(childTable.IdFieldName, ".", "].[") + "] FROM [" + childTable.TableName + "] WHERE 0=1;";
                                            // Dim rsAdo = DataServices.GetADORecordset(sSql, sADOConn)
                                            int arglError = 0;
                                            string argsErrorMessage = "";
                                            var rsAdo = DataServices.GetADORecordset(sSql, childTable, _iDatabas.All(), MaxRecords: 1, bReturnError: true,lError: ref arglError ,sErrorMessage: ref argsErrorMessage);
                                            if (rsAdo is not null)
                                            {
                                                if (!Convert.ToBoolean(rsAdo.Fields[0].Properties["IsAutoIncrement"].Value))
                                                {
                                                    editSettingList["DropDown"] = false;
                                                    editSettingList["Editable"] = false;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    editSettingList["DropDown"] = false;
                                }

                                break;
                            }

                        case 0:
                            {
                                var ParentTable = _iRelationShip.All().Where(m => m.LowerTableName.Trim().ToLower().Equals(moTable.TableName.Trim().ToLower()));

                                if (ParentTable is not null)
                                {
                                    foreach (RelationShip relationObj in ParentTable)
                                    {
                                        if (Strings.StrComp(DatabaseMap.RemoveTableNameFromField(relationObj.LowerTableFieldName), DatabaseMap.RemoveTableNameFromField(viewColumnEntity.FieldName), Constants.vbTextCompare) == 0)
                                        {
                                            mbLocalLookup = true;
                                            break;
                                        }
                                        // If (DatabaseMap.RemoveTableNameFromField(relationObj.LowerTableFieldName).Trim.ToLower.Equals(DatabaseMap.RemoveTableNameFromField(viewColumnEntity.FieldName))) Then
                                        // mbLocalLookup = True
                                        // Exit For
                                        // End If
                                    }
                                    editSettingList["DropDown"] = mbLocalLookup;
                                    // FUS-5789
                                    if (editSettingList["DropDown"] == false)
                                    {
                                        if (moTable is not null)
                                        {
                                            if (ParentTable is not null)
                                            {
                                                foreach (RelationShip relationObj in ParentTable)
                                                {
                                                    if (relationObj.UpperTableFieldName.Split('.')[0].Trim().ToLower().Equals(viewColumnEntity.FieldName.Split('.')[0].Trim().ToLower()))
                                                    {
                                                        editSettingList["Editable"] = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (DatabaseMap.RemoveTableNameFromField(moTable.RetentionFieldName).Trim().ToLower().Equals(DatabaseMap.RemoveTableNameFromField(viewColumnEntity.FieldName).Trim().ToLower()))
                                {
                                    editSettingList["Editable"] = (moTable.RetentionAssignmentMethod != (int)Enums.meRetentionCodeAssignment.rcaCurrentTable) && (moTable.RetentionAssignmentMethod != (int)Enums.meRetentionCodeAssignment.rcaRelatedTable);
                                    editSettingList["DropDown"] = editSettingList["Editable"];
                                }

                                break;
                            }
                    }

                    switch (viewColumnEntity.LookupType)
                    {
                        case 1:
                        case 12:
                        case 13:
                        case 14:
                        case 15:
                        case 17:
                            {
                                bLocked = false;
                                break;
                            }

                        default:
                            {
                                // Get ADO connection name
                                if (moTable is not null)
                                {
                                    // Dim dbObj = _iDatabas.All.Where(Function(m) m.DBName.Trim.ToLower.Equals(DBConName.Trim.ToLower)).FirstOrDefault
                                    sADOConn = DataServices.DBOpen(moTable, _iDatabas.All());
                                }
                                string sSql = "SELECT [" + DatabaseMap.RemoveTableNameFromField(viewColumnEntity.FieldName) + "] FROM [" + moTable.TableName + "] WHERE 0=1;";
                                int arglError1 = 0;
                                string argsErrorMessage1 = "";
                                var rsAdo = DataServices.GetADORecordset(sSql, moTable, _iDatabas.All(), MaxRecords: 1 ,bReturnError: true,lError: ref arglError1 ,sErrorMessage: ref argsErrorMessage1);
                                // Dim rsAdo = DataServices.GetADORecordset(sSql, sADOConn)
                                bLocked = ViewModel.DataLocked(viewColumnEntity.FieldName, rsAdo);
                                break;
                            }
                    }
                    if (bLocked)
                    {
                        editSettingList["Editable"] = false;
                        if ((Int32)viewColumnEntity.LookupType != (Int32)Enums.geViewColumnsLookupType.ltLookup)
                        {
                            editSettingList["DropDown"] = false;
                        }
                    }
                    if ((Int32)viewColumnEntity.LookupType != (Int32)Enums.geViewColumnsLookupType.ltDirect)
                    {
                        if ((Int32)viewColumnEntity.LookupType != (Int32)Enums.geViewColumnsLookupType.ltLookup)
                        {
                            editSettingList["Editable"] = false;
                        }
                        editSettingList["Sortable"] = false;
                        editSettingList["Filterable"] = false;
                        editSettingList["MaskIncludeDB"] = false;
                        editSettingList["Capslock"] = false;
                    }
                    if (editSettingList["DropDown"] & mbLocalLookup)
                    {
                        editSettingList["DropDownSuggestionOnly"] = true;
                    }
                    else
                    {
                        editSettingList["DropDownSuggestionOnly"] = false;
                    }
                    SetEditSettingOnEdit(ref editSettingList, viewColumnEntity, tableName, oView);
                }
            }

            catch (Exception)
            {
            }

        }

        public void SetEditSettingOnEdit(ref Dictionary<string, bool> editSettingList, ViewColumn viewColumnEntity, string sTableName, View oView)
        {
            try
            {
                string msSQL = "";
                IRepository<Table> _iTable = new Repositories<Table>();
                Table oTable = null;
                var lError = default(int);
                ADODB.Recordset rsADO;
                string sErrorMessage = string.Empty;
                ADODB.Field oFields = null;
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                if (!string.IsNullOrEmpty(sTableName))
                {
                    oTable = _iTable.All().Where(m => m.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())).FirstOrDefault();
                }
                if (oView is null)
                {
                    msSQL = "SELECT * FROM [" + sTableName + "]";
                }
                else
                {
                    msSQL = oView.SQLStatement;
                }
                string sSql = string.Empty;
                string sOrderByStr = string.Empty;
                int iWherePos;
                if (DatabaseMap.RemoveTableNameFromField(viewColumnEntity.FieldName).Trim().Equals("SLTrackedDestination") | DatabaseMap.RemoveTableNameFromField(viewColumnEntity.FieldName).Trim().Equals("SLFileRoomOrder"))
                {
                    editSettingList["SubTotal"] = false;
                    editSettingList["Editable"] = false;
                }
                else
                {
                    if (Strings.InStr(viewColumnEntity.FieldName, ".") > 1)
                    {
                        sTableName = DatabaseMap.RemoveFieldNameFromField(viewColumnEntity.FieldName);
                    }

                    if ((bool)(Convert.ToInt32(Enums.geViewColumnsLookupType.ltDirect) is var arg409 && viewColumnEntity.LookupType is { } arg408 ? arg408 == arg409 : (bool?)null))
                    {
                        sSql = DataServices.NormalizeString(msSQL);
                        iWherePos = Strings.InStr(1, sSql, "GROUP BY", Constants.vbTextCompare);
                        if (iWherePos == 0)
                        {
                            iWherePos = Strings.InStr(1, sSql, "ORDER BY", Constants.vbTextCompare);
                        }
                        if (iWherePos > 0)
                        {
                            sOrderByStr = Strings.Mid(sSql, iWherePos);
                            sSql = Strings.Left(sSql, iWherePos - 2);
                        }
                        iWherePos = Strings.InStr(1, sSql, " WHERE ", Constants.vbTextCompare);
                        if (iWherePos > 0)
                        {
                            sSql = Strings.Left(sSql, iWherePos + 6) + " (" + Strings.Mid(sSql, iWherePos + 7) + ")";
                            sSql = sSql + " AND (";
                        }
                        else
                        {
                            sSql = sSql + " WHERE (";
                        }
                        sSql = sSql + "1 = 0)" + sOrderByStr;
                        rsADO = DataServices.GetADORecordset(sSql, oTable, _iDatabas.All(),MaxRecords: 1 ,bReturnError: true,lError: ref lError,sErrorMessage: ref sErrorMessage);
                    }
                    else if (!string.IsNullOrEmpty(viewColumnEntity.AlternateFieldName))
                    {
                        rsADO = DataServices.GetADORecordset("SELECT " + viewColumnEntity.AlternateFieldName + " FROM [" + sTableName + "] WHERE 0 = 1", oTable, _iDatabas.All(),MaxRecords: 1 ,bReturnError: true,lError: ref lError,sErrorMessage: ref sErrorMessage);
                    }
                    else
                    {
                        rsADO = DataServices.GetADORecordset("SELECT [" + Strings.Replace(viewColumnEntity.FieldName.ToString(), ".", "].[") + "] FROM [" + sTableName + "] WHERE 0 = 1", oTable, _iDatabas.All(), MaxRecords: 1 ,bReturnError: true,lError: ref lError,sErrorMessage:ref sErrorMessage);
                    }
                    if (rsADO is not null)
                    {
                        if (rsADO is not null)
                        {
                            oFields = ViewModel.FieldWithOrWithoutTable(viewColumnEntity.FieldName, rsADO.Fields, false);
                        }
                    }
                    if (lError == 0)
                    {
                        if (oFields is not null)
                        {
                            if (SchemaInfoDetails.IsADateType((Enums.DataTypeEnum)oFields.Type))
                            {
                                editSettingList["Capslock"] = false;
                                editSettingList["SubTotal"] = false;
                            }
                            else if (SchemaInfoDetails.IsAStringType((Enums.DataTypeEnum)oFields.Type))
                            {
                                editSettingList["SubTotal"] = false;
                            }
                            else
                            {
                                editSettingList["Capslock"] = false;
                                switch (oFields.Type)
                                {
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBoolean:
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedTinyInt:
                                        {
                                            editSettingList["SubTotal"] = false;
                                            break;
                                        }
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmDouble:
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmCurrency:
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmDecimal:
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmNumeric:
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmSingle:
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmVarNumeric:
                                        {
                                            editSettingList["SubTotal"] = true;
                                            break;
                                        }
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBigInt:
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedBigInt:
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmInteger:
                                        {
                                            if (Convert.ToBoolean(oFields.Properties["ISAUTOINCREMENT"].Value))
                                            {
                                                editSettingList["Editable"] = false;
                                                editSettingList["SubTotal"] = false;
                                            }
                                            else
                                            {
                                                int boolVal = Strings.StrComp(DatabaseMap.RemoveTableNameFromField(oFields.Name), DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), (CompareMethod)Convert.ToInt32(Constants.vbTextCompare == 0));
                                                if (!string.IsNullOrEmpty(oTable.CounterFieldName) & boolVal == 0)
                                                {
                                                    editSettingList["SubTotal"] = false;
                                                }
                                                else
                                                {
                                                    editSettingList["SubTotal"] = true;
                                                }
                                            }

                                            break;
                                        }
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBinary:
                                        {
                                            editSettingList["Editable"] = false;
                                            editSettingList["SubTotal"] = false;
                                            break;
                                        }
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmSmallInt:
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmTinyInt:
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedInt:
                                    case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedSmallInt:
                                        {
                                            editSettingList["SubTotal"] = true;
                                            break;
                                        }
                                }
                            }
                            rsADO.Close();
                            rsADO = null;
                        }
                        else
                        {
                            editSettingList["SubTotal"] = false;
                        }
                    }
                    else
                    {
                        editSettingList["SubTotal"] = false;
                    }
                    oFields = null;
                }

                if ((Int32)viewColumnEntity.LookupType != (Int32)Enums.geViewColumnsLookupType.ltDirect)
                {
                    editSettingList["Sortable"] = false;
                    editSettingList["Filterable"] = false;
                }
            }

            catch (Exception)
            {

            }
        }


        public IActionResult DropDownValidation(bool currentStatus, bool editStatus, string tableName, string lFieldNameVar, Enums.geViewColumnsLookupType lookUpVar)
        {
            try
            {
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                bool chkEditable = editStatus;
                bool chkDropDownSuggest;
                bool mbLocalLookup = false;
                if (!string.IsNullOrEmpty(tableName))
                {
                    var parentTable = _iRelationShip.All().Where(m => m.LowerTableName.Trim().ToLower().Equals(tableName.Trim().ToLower()));
                    if (parentTable.Count() > 0)
                    {
                        foreach (RelationShip relationObj in parentTable)
                        {
                            if (DatabaseMap.RemoveTableNameFromField(relationObj.LowerTableFieldName).Trim().ToLower().Equals(DatabaseMap.RemoveTableNameFromField(lFieldNameVar).Trim().ToLower()))
                            {
                                mbLocalLookup = true;
                                break;
                            }
                        }
                    }
                }
                if (!currentStatus)
                {
                    if (!mbLocalLookup | lookUpVar == Enums.geViewColumnsLookupType.ltLookup)
                    {
                        chkEditable = false;
                    }
                    chkDropDownSuggest = false;
                }
                else
                {
                    if (!mbLocalLookup | lookUpVar == Enums.geViewColumnsLookupType.ltLookup)
                    {
                        chkEditable = true;
                    }
                    chkDropDownSuggest = mbLocalLookup;
                }
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                string chkEditableJSON = JsonConvert.SerializeObject(chkEditable, Newtonsoft.Json.Formatting.Indented, Setting);
                string chkDropDownSuggestJSON = JsonConvert.SerializeObject(chkDropDownSuggest, Newtonsoft.Json.Formatting.Indented, Setting);
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    chkEditableJSON,
                    chkDropDownSuggestJSON
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                return Json(new { errortype = Keys.ErrorType });
            }
        }

        public JsonResult DeleteViewColumn(int vViewId, int vViewColumnId, string arrSerialized)
        {

            List<ViewColumn> lstViewColumns;
            ViewColumn pViewColumn;

            try
            {
                if (this.TempData is not null)
                {
                    bool bInUse = false;
                    lstViewColumns = TempData.GetPeek<List<ViewColumn>>("TempViewColumns_" + vViewId);
                    //lstViewColumns = (List<ViewColumn>)this.TempData.Peek("TempViewColumns_" + vViewId);
                    pViewColumn = lstViewColumns.FirstOrDefault(m => m.Id == vViewColumnId);
                    if (pViewColumn is not null)
                    {

                        foreach (ViewColumn viewColObj in lstViewColumns)
                        {
                            if ((Int32)viewColObj.LookupType != (Int32)Enums.geViewColumnsLookupType.ltDirect)
                            {
                                if (viewColObj.LookupIdCol == pViewColumn.ColumnNum && viewColObj.Id != pViewColumn.Id)
                                {
                                    bInUse = true;
                                    break;
                                }
                            }
                        }
                        if (!bInUse)
                        {
                            var filterColNum = JsonConvert.DeserializeObject<dynamic>(arrSerialized);
                            if (filterColNum is not null)
                            {
                                foreach (string iCount in filterColNum)
                                {
                                    if (!string.IsNullOrEmpty(iCount))
                                    {
                                        if (pViewColumn.ColumnNum == Convert.ToInt16(iCount))
                                        {
                                            bInUse = true;
                                            break;
                                        }
                                    }

                                }
                            }
                        }
                        if (bInUse)
                        {
                            Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlSelectedColUsedAsLookupId");
                            Keys.ErrorType = "w";
                        }
                        else
                        {
                            var ColumnNumVar = pViewColumn.ColumnNum;
                            object tempViewColumn = null;

                            foreach (ViewColumn objViewCol in lstViewColumns)
                            {
                                if (objViewCol.ColumnNum == pViewColumn.LookupIdCol && objViewCol.ColumnNum != 0)
                                {
                                    bool IsUsedByOther = lstViewColumns.Any(m => m.LookupIdCol == objViewCol.ColumnNum);
                                    if (!IsUsedByOther)
                                    {
                                        ColumnNumVar = objViewCol.ColumnNum;
                                        tempViewColumn = objViewCol;
                                    }
                                }
                            }
                            if (tempViewColumn is not null)
                            {
                                lstViewColumns.Remove((ViewColumn)tempViewColumn);
                            }
                            lstViewColumns.Remove(pViewColumn);
                            lstViewColumns.OrderBy(m => m.ColumnNum);
                            ColumnNumVar = (short?)0;
                            var list = new List<KeyValuePair<int, int>>();
                            foreach (ViewColumn viewColObj in lstViewColumns)
                            {
                                bool IfDependent = lstViewColumns.Any(m => m.LookupIdCol == viewColObj.ColumnNum && m.LookupIdCol != 0);
                                if (IfDependent)
                                {
                                    list.Add(new KeyValuePair<int, int>((int)ColumnNumVar, (int)viewColObj.ColumnNum));
                                }
                                viewColObj.ColumnNum = ColumnNumVar;
                                ColumnNumVar = (short?)(ColumnNumVar + 1);
                            }
                            foreach (ViewColumn viewColObj in lstViewColumns)
                            {
                                if (viewColObj.LookupType == 1)
                                {
                                    bool YesNo = list.Any(m => m.Value == viewColObj.LookupIdCol);
                                    if (YesNo)
                                    {
                                        viewColObj.LookupIdCol = (short?)list.Where(m => m.Value == viewColObj.LookupIdCol).FirstOrDefault().Key;
                                    }
                                }
                            }

                            TempData.Set<List<ViewColumn>>("TempViewColumns_" + vViewId, lstViewColumns);
                            //TempData["TempViewColumns_" + vViewId] = lstViewColumns;
                            TempData.Peek("TempViewColumns_" + vViewId);
                            Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlColRmvSuccessfully");
                            Keys.ErrorType = "s";
                        }
                    }
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public void SetLookupIdColOnAdd(ref int iLookupColumn, ViewColumn formEntity, List<ViewColumn> CurrentViewColumn, string currentTableName, string msSql, Table moTables)
        {
            try
            {
                // Dim iLookupColumn As Integer
                RelationShip oRelationships;
                var oParentRelationships = new RelationShip();
                var iLookupColumn2 = default(int);
                ViewColumn oTmpViewColumns;
                string sFieldTableName;
                string sTmpTableName;
                IRepository<RelationShip> _iRelationShip = new Repositories<RelationShip>();
                var ParentTable = _iRelationShip.All().Where(m => m.LowerTableName.Trim().ToLower().Equals(currentTableName.Trim().ToLower()));
                switch (formEntity.LookupType)
                {
                    case 1:
                        {
                            iLookupColumn = -1;
                            foreach (var currentOTmpViewColumns in CurrentViewColumn)
                            {
                                oTmpViewColumns = currentOTmpViewColumns;
                                // If (Not oTmpViewColumns.Deleted) Then
                                if (Operators.ConditionalCompareObjectEqual(ReportsModel.mcLevel[Strings.UCase(Strings.Trim(formEntity.FieldName))], 1, false))
                                {
                                    if ((Int32)oTmpViewColumns.LookupType == (Int32)Enums.geViewColumnsLookupType.ltDirect)
                                    {
                                        if ((Strings.UCase(Strings.Trim(DatabaseMap.RemoveTableNameFromField(oTmpViewColumns.FieldName)))) == (Strings.UCase(Strings.Trim(DatabaseMap.RemoveTableNameFromField(Convert.ToString(ReportsModel.mcFieldName[Strings.UCase(Strings.Trim(formEntity.FieldName))]))))))
                                        {
                                            iLookupColumn = (int)oTmpViewColumns.ColumnNum;
                                            break;
                                        }
                                    }
                                }
                                if (Operators.ConditionalCompareObjectEqual(ReportsModel.mcLevel[Strings.UCase(Strings.Trim(formEntity.FieldName))], 2, false))
                                {
                                    if ((Int32)oTmpViewColumns.LookupType == (Int32)Enums.geViewColumnsLookupType.ltLookup)
                                    {
                                        if ((Strings.UCase(Strings.Trim(oTmpViewColumns.FieldName))) == (Strings.UCase(Strings.Trim(Convert.ToString(ReportsModel.mcFieldName[Strings.UCase(Strings.Trim(formEntity.FieldName))])))))
                                        {
                                            iLookupColumn = CheckForDirectRelationship(formEntity.FieldName, ParentTable, CurrentViewColumn, msSql, moTables, formEntity);
                                            // Private Function CheckForDirectRelationship(ByVal sUpperFieldName As String, ByVal ParentTable As IQueryable(Of RelationShip), currentViewColumn As List(Of ViewColumn), msSql As String, moTables As Table, FormEntity As ViewColumn) As Integer
                                            if (iLookupColumn < 0)
                                            {
                                                iLookupColumn = (int)oTmpViewColumns.ColumnNum;
                                            }

                                            break;
                                        }
                                    }
                                }
                                // End If
                            }
                            oTmpViewColumns = null;
                            if (iLookupColumn < 0)
                            {
                                if (Operators.ConditionalCompareObjectEqual(ReportsModel.mcLevel[Strings.UCase(Strings.Trim(formEntity.FieldName))], 1, false))
                                {

                                    // iLookupColumn = CreateViewColumns(geViewColumnsLookupType_ltDirect, 0, RemoveTableNameFromFieldIfNotCurrentTable(mcFieldName.Item(UCase$(Trim$(cboFieldName.List(cboFieldName.ListIndex))))), "Id For " & cboFieldName.List(cboFieldName.ListIndex), False, False)
                                    iLookupColumn = this.CreateViewColumnsForLookupIdCol(ref CurrentViewColumn, Enums.geViewColumnsLookupType.ltDirect, 0, DatabaseMap.RemoveTableNameFromFieldIfNotCurrentTable(Convert.ToString(ReportsModel.mcFieldName[Strings.UCase(Strings.Trim(formEntity.FieldName))]), currentTableName), "Id For " + formEntity.FieldName, false, false, msSql, moTables, formEntity);
                                }
                                // iLookupColumn = CreateViewColumns(Enums.geViewColumnsLookupType.ltDirect, 0, DatabaseMap.RemoveTableNameFromFieldIfNotCurrentTable(ReportsModel.mcFieldName.Item(UCase$(Trim$(formEntity.FieldName))), currentTableName), "Id For " & formEntity.FieldName, False, False)
                                else
                                {
                                    oRelationships = (RelationShip)ReportsModel.mcRelationships[Strings.UCase(Strings.Trim(formEntity.FieldName))];

                                    foreach (var currentOParentRelationships in ParentTable)
                                    {
                                        oParentRelationships = currentOParentRelationships;
                                        if ((Strings.UCase(Strings.Trim(oParentRelationships.UpperTableName))) == (Strings.UCase(Strings.Trim(oRelationships.LowerTableName))))
                                        {
                                            break;
                                        }
                                    }
                                    if (oParentRelationships is not null)
                                    {
                                        iLookupColumn2 = -1;
                                        foreach (var currentOTmpViewColumns1 in CurrentViewColumn)
                                        {
                                            oTmpViewColumns = currentOTmpViewColumns1;
                                            // If (Not oTmpViewColumns.Deleted) Then
                                            if ((Int32)oTmpViewColumns.LookupType == (Int32)Enums.geViewColumnsLookupType.ltDirect)
                                            {
                                                if ((Strings.UCase(Strings.Trim(DatabaseMap.RemoveTableNameFromField(oTmpViewColumns.FieldName)))) == (Strings.UCase(Strings.Trim(DatabaseMap.RemoveTableNameFromField(oParentRelationships.LowerTableFieldName)))))
                                                {
                                                    iLookupColumn2 = (int)oTmpViewColumns.ColumnNum;
                                                    break;
                                                }
                                            }
                                            // End If
                                        }
                                        oTmpViewColumns = null;
                                        if (iLookupColumn2 < 0)
                                        {
                                            // Private Function CreateViewColumnsForLookupIdCol(ByRef CurrentViewColumn As List(Of ViewColumn), eLookupType As Enums.geViewColumnsLookupType, ByVal iLookupCol As Integer, ByVal sFieldName As String, ByVal sFieldDesc As String, ByVal bDoDropDownFlag As Boolean, ByVal bVisible As Boolean, ByVal msSQL As String, moTables As Table, formEntity As ViewColumn) As Integer
                                            iLookupColumn2 = this.CreateViewColumnsForLookupIdCol(ref CurrentViewColumn, Enums.geViewColumnsLookupType.ltDirect, 0, Strings.Trim(oParentRelationships.LowerTableFieldName), "Id For " + oRelationships.LowerTableFieldName, false, false, msSql, moTables, formEntity);
                                        }
                                        iLookupColumn = this.CreateViewColumnsForLookupIdCol(ref CurrentViewColumn, Enums.geViewColumnsLookupType.ltLookup, iLookupColumn2, Strings.Trim(oRelationships.LowerTableFieldName), "Id For " + formEntity.FieldName, false, false, msSql, moTables, formEntity);
                                        // iLookupColumn = CreateViewColumns(geViewColumnsLookupType_ltLookup, iLookupColumn2, Trim$(oRelationships.LowerTableFieldName), "Id For " & cboFieldName.List(cboFieldName.ListIndex), False, False)
                                    }
                                }
                            }

                            break;
                        }

                    case 12:
                    case 14:
                    case 13:
                    case 15:
                    case 17:
                        {
                            iLookupColumn = -1; // Level One Items still set this as a flag that it is a level one look down not up.
                            string uCaseVar = Strings.UCase(Strings.Trim(formEntity.FieldName));
                            if (Operators.ConditionalCompareObjectEqual(ReportsModel.mcLevel[Strings.UCase(Strings.Trim(formEntity.FieldName))], 2, false))
                            {
                                iLookupColumn = -1;
                                sFieldTableName = Strings.UCase(Strings.Trim(Convert.ToString(ReportsModel.mcFieldName[Strings.UCase(Strings.Trim(formEntity.FieldName))])));
                                if (Strings.InStr(sFieldTableName, ".") > 1)
                                {
                                    sFieldTableName = Strings.Left(sFieldTableName, Strings.InStr(sFieldTableName, ".") - 1);
                                }
                                else
                                {
                                    sFieldTableName = "";
                                }
                                if (!string.IsNullOrEmpty(sFieldTableName))
                                {
                                    foreach (var currentOTmpViewColumns2 in CurrentViewColumn)
                                    {
                                        oTmpViewColumns = currentOTmpViewColumns2;
                                        // If (Not oTmpViewColumns.Deleted) Then
                                        if (oTmpViewColumns.LookupIdCol == -1) // This indicates the look down record we need
                                        {
                                            bool exitFor = false;
                                            switch (oTmpViewColumns.LookupType)
                                            {
                                                case 12:
                                                case 14:
                                                case 13:
                                                case 15:
                                                case 17:
                                                    {
                                                        // Check if table names are the same
                                                        sTmpTableName = Strings.UCase(Strings.Trim(oTmpViewColumns.FieldName));
                                                        if (Strings.InStr(sTmpTableName, ".") > 1)
                                                        {
                                                            sTmpTableName = Strings.Left(sTmpTableName, Strings.InStr(sTmpTableName, ".") - 1);
                                                        }
                                                        else
                                                        {
                                                            sTmpTableName = "";
                                                        }
                                                        if ((sTmpTableName) == (sFieldTableName))
                                                        {
                                                            iLookupColumn = (int)oTmpViewColumns.ColumnNum;
                                                            exitFor = true;
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
                                        }
                                        // End If
                                    }
                                }
                                oTmpViewColumns = null;
                                if (iLookupColumn < 0)
                                {
                                    oRelationships = (RelationShip)ReportsModel.mcRelationships[Strings.UCase(Strings.Trim(formEntity.FieldName))];
                                    iLookupColumn = CreateViewColumnsForLookupIdCol(ref CurrentViewColumn, (Enums.geViewColumnsLookupType)formEntity.LookupType, iLookupColumn2, Strings.Trim(oRelationships.LowerTableFieldName), "Id For " + formEntity.FieldName, false, false, msSql, moTables, formEntity);
                                    // iLookupColumn = CreateViewColumns(eType, -1, Trim$(oRelationships.LowerTableFieldName), "Id For " & cboFieldName.List(cboFieldName.ListIndex), False, False)
                                    oRelationships = null;
                                }
                            }

                            break;
                        }

                    default:
                        {
                            break;
                        }
                }
            }

            // bIsSecondLevel = False

            // If (iLookupColumn > 0) Then
            // If ((moViewColumns.LookupIdCol >= 0) And (iLookupColumn < moViews.ViewColumns.Count)) Then
            // oViewColumns = moViews.ViewColumns.Item(iLookupColumn + 1)
            // If (Not (oViewColumns Is Nothing)) Then
            // bIsSecondLevel = (oViewColumns.LookupType <> geViewColumnsLookupType_ltDirect)
            // End If
            // oViewColumns = Nothing
            // End If
            // End If

            // If (mcLevel.Count > 0) Then
            // iLevel = mcLevel.Item(cboFieldName.List(cboFieldName.ListIndex))
            // End If
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        private int CheckForDirectRelationship(string sUpperFieldName, IQueryable<RelationShip> ParentTable, List<ViewColumn> currentViewColumn, string msSql, Table moTables, ViewColumn FormEntity)
        {
            int CheckForDirectRelationshipRet = default;
            try
            {
                int iCol;
                int iLookupColumn;
                string sLowerFieldName;
                string sUpperTableName;
                ViewColumn oViewColumns;
                ADODB.Recordset rsADO;
                string sSQL;
                var sADOCon = new ADODB.Connection();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                sADOCon = DataServices.DBOpen(moTables, _iDatabas.All());

                iLookupColumn = -1;
                sUpperTableName = DatabaseMap.RemoveFieldNameFromField(sUpperFieldName);

                foreach (var oRelationships in ParentTable)
                {
                    if (Strings.StrComp(oRelationships.UpperTableName, sUpperTableName, Constants.vbTextCompare) == 0)
                    {
                        sLowerFieldName = DatabaseMap.RemoveTableNameFromField(oRelationships.LowerTableFieldName);
                        //oRelationships = null;

                        foreach (var currentOViewColumns in currentViewColumn)
                        {
                            oViewColumns = currentOViewColumns;
                            if (Strings.StrComp(sLowerFieldName, DatabaseMap.RemoveTableNameFromField(oViewColumns.FieldName), Constants.vbTextCompare) == 0)
                            {
                                iLookupColumn = (int)oViewColumns.ColumnNum;
                                break;
                            }
                        }

                        oViewColumns = null;

                        if (iLookupColumn < 0)
                        {
                            sSQL = DataServices.NormalizeString(msSql);

                            if (Strings.InStr(1, sSQL, "SELECT DISTINCT TOP", Constants.vbTextCompare) <= 0L & Strings.InStr(1, sSQL, "SELECT DISTINCTROW TOP", Constants.vbTextCompare) <= 0L & Strings.InStr(1, sSQL, "SELECT TOP", Constants.vbTextCompare) <= 0L)
                            {
                                if (Strings.InStr(1, sSQL, "SELECT DISTINCT ", Constants.vbTextCompare) > 0L)
                                {
                                    sSQL = Strings.Replace(sSQL, "SELECT DISTINCT ", "SELECT DISTINCT TOP " + Strings.Format(1) + " ", 1, 1, Constants.vbTextCompare);
                                }
                                else if (Strings.InStr(1, sSQL, "SELECT DISTINCTROW ", Constants.vbTextCompare) > 0L)
                                {
                                    sSQL = Strings.Replace(sSQL, "SELECT DISTINCTROW ", "SELECT DISTINCTROW TOP " + Strings.Format(1) + " ", 1, 1, Constants.vbTextCompare);
                                }
                                else
                                {
                                    sSQL = Strings.Replace(sSQL, "SELECT ", "SELECT TOP " + Strings.Format(1) + " ", 1, 1, Constants.vbTextCompare);
                                }
                            }

                            string arglError = "";
                            rsADO = DataServices.GetADORecordSet(ref sSQL, sADOCon, lError: ref arglError);

                            var loopTo = rsADO.Fields.Count - 1;
                            for (iCol = 0; iCol <= loopTo; iCol++)
                            {
                                if (Strings.StrComp(sLowerFieldName, rsADO.Fields[iCol].Name, Constants.vbTextCompare) == 0)
                                {
                                    iLookupColumn = this.CreateViewColumnsForLookupIdCol(ref currentViewColumn, Enums.geViewColumnsLookupType.ltDirect, 0, sLowerFieldName, sLowerFieldName, false, false, msSql, moTables, FormEntity);
                                    break;
                                }
                            }

                            rsADO = null;
                        }

                        break;
                    }
                }

                CheckForDirectRelationshipRet = iLookupColumn;
            }
            catch (Exception ex)
            {
                CheckForDirectRelationshipRet = -1;
                throw ex.InnerException;
            }

            return CheckForDirectRelationshipRet;

        }

        private int CreateViewColumnsForLookupIdCol(ref List<ViewColumn> CurrentViewColumn, Enums.geViewColumnsLookupType eLookupType, int iLookupCol, string sFieldName, string sFieldDesc, bool bDoDropDownFlag, bool bVisible, string msSQL, Table moTables, ViewColumn formEntity)
        {
            int CreateViewColumnsForLookupIdColRet = default;
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            try
            {
                string sSQL;
                ADODB.Recordset rsTestSQL;
                var lError = default(long);
                string sErrorMessage = string.Empty;
                int iWherePos;
                string sTestFieldName;
                var oViewColumns = new ViewColumn();
                int iMinNumber = -1;
                int columnNum = 0;

                AddUpdateViewColumn(ref oViewColumns, formEntity);
                if (CurrentViewColumn is not null)
                {
                    if (CurrentViewColumn.Count != 0)
                    {
                        iMinNumber = CurrentViewColumn.Min(x => x.Id);
                        if (iMinNumber < 0)
                        {
                            iMinNumber = iMinNumber - 1;
                        }
                        else
                        {
                            iMinNumber = -1;
                        }
                        columnNum = (int)CurrentViewColumn.Max(x => x.ColumnNum);
                        columnNum = columnNum + 1;

                    }
                }
                oViewColumns.Id = iMinNumber;
                oViewColumns.ViewsId = formEntity.ViewsId;
                oViewColumns.ColumnNum = (short?)columnNum;

                if (bVisible)
                {
                    oViewColumns.ColumnOrder = Convert.ToInt16(Enums.geViewColumnDisplayType.cvAlways); // Visible Flag
                }
                else
                {
                    oViewColumns.ColumnOrder = Convert.ToInt16(Enums.geViewColumnDisplayType.cvNotVisible);
                } // Visible Flag

                oViewColumns.FieldName = sFieldName;
                oViewColumns.Heading = Strings.Left(sFieldDesc, 30);
                oViewColumns.FreezeOrder = 0;
                oViewColumns.SortOrder = 0;
                oViewColumns.LookupType = Convert.ToInt16(eLookupType);
                if ((bool)(oViewColumns.FilterField | oViewColumns.SortableField))
                {
                    sSQL = msSQL;
                    sSQL = Strings.Replace(sSQL, Constants.vbCr, " ");
                    sSQL = Strings.Replace(sSQL, Constants.vbLf, " ");

                    iWherePos = Strings.InStr(1, sSQL, "GROUP BY", Constants.vbTextCompare);
                    if (iWherePos == 0)
                    {
                        iWherePos = Strings.InStr(1, sSQL, "ORDER BY", Constants.vbTextCompare);
                    }

                    if (iWherePos > 0)
                    {
                        sSQL = Strings.Left(sSQL, iWherePos - 2);
                    }

                    iWherePos = Strings.InStr(1, sSQL, " WHERE ", Constants.vbTextCompare);

                    if (iWherePos > 0)
                    {
                        sSQL = Strings.Left(sSQL, iWherePos + 6) + " (" + Strings.Mid(sSQL, iWherePos + 7) + ")";
                        sSQL = sSQL + " AND (";
                    }
                    else
                    {
                        sSQL = sSQL + " WHERE (";
                    }

                    if (Strings.StrComp(DatabaseMap.RemoveTableNameFromField(oViewColumns.FieldName), "SLFileRoomOrder", Constants.vbTextCompare) == 0)
                    {
                        sTestFieldName = "[" + Strings.Replace(moTables.IdFieldName, ".", "].[") + "]";
                    }
                    else if (string.IsNullOrEmpty(Strings.Trim(oViewColumns.AlternateFieldName)))
                    {
                        sTestFieldName = "[" + Strings.Replace(oViewColumns.FieldName, ".", "].[") + "]";
                    }
                    else
                    {
                        sTestFieldName = oViewColumns.AlternateFieldName;
                    }

                    sSQL = sSQL + sTestFieldName + " IS NULL)";
                    int arglError = (int)lError;
                    rsTestSQL = DataServices.GetADORecordset(sSQL, moTables, _iDatabas.All(), MaxRecords: 1,bReturnError: true,lError: ref arglError, sErrorMessage: ref sErrorMessage);
                    lError = arglError;
                    // rsTestSQL = theApp.Data.GetADORecordset(sSQL, moTables.TableName, , , , 1, , True, lError, sErrorMessage)
                    if (Convert.ToBoolean(lError))
                    {
                        oViewColumns.FilterField = false;
                        oViewColumns.SortableField = false;
                    }
                    else if (rsTestSQL is not null)
                    {
                        rsTestSQL.Close();
                        rsTestSQL = null;
                    }
                }
                if (bDoDropDownFlag | (Strings.UCase(moTables.RetentionFieldName)) == (Strings.UCase(oViewColumns.FieldName)))
                {
                    oViewColumns.DropDownFlag = (short?)-1;
                    oViewColumns.EditAllowed = true;
                }
                else
                {
                    oViewColumns.DropDownFlag = (short?)0;
                }
                oViewColumns.MaskPromptChar = "_";
                oViewColumns.MaxPrintLines = 1;
                oViewColumns.LookupIdCol = (short?)iLookupCol;
                oViewColumns.ColumnStyle = (short?)0;
                CurrentViewColumn.Add(oViewColumns);
                CreateViewColumnsForLookupIdColRet = (int)oViewColumns.ColumnNum;
            }
            catch (Exception ex)
            {
                CreateViewColumnsForLookupIdColRet = 0;
                throw ex.InnerException;
            }

            return CreateViewColumnsForLookupIdColRet;

        }

        #endregion

        #region Report Style

        public PartialViewResult LoadReportStyle()
        {
            return PartialView("_ReportStylesPartial");
        }

        public IActionResult GetReportStyles(string sord, int page, int rows)
        {
            IRepository<ReportStyle> _iReportStyle = new Repositories<ReportStyle>();
            var reportEntity = _iReportStyle.All().OrderBy(m => m.ReportStylesId);
            if (reportEntity is null)
            {
                return null;
            }
            else
            {
                var jsonData = reportEntity.GetJsonListForGrid(sord, page, rows, "Id");
                return Json(jsonData);
            }
        }

        public PartialViewResult LoadAddReportStyle()
        {
            return PartialView("_AddReportStylePartial");
        }

        public IActionResult GetReportStylesData(string reportStyleVar, int selectedRowsVar = 0, bool cloneFlag = false)
        {
            object reportStyleEntity;
            object allReportStyle = null;
            IRepository<ReportStyle> _iReportStyle = new Repositories<ReportStyle>();
            if (selectedRowsVar != 0)
            {
                reportStyleEntity = _iReportStyle.All().Where(m => m.Id.Equals(reportStyleVar.Trim()) & m.ReportStylesId.Equals(selectedRowsVar)).FirstOrDefault();
            }
            else
            {
                reportStyleEntity = _iReportStyle.All().Where(m => m.Id.Equals(reportStyleVar.Trim())).FirstOrDefault();
            }

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(reportStyleEntity, Newtonsoft.Json.Formatting.Indented, Setting);
            if (cloneFlag)
            {
                allReportStyle = _iReportStyle.All().OrderBy(m => m.Id);
                bool bFound = false;
                int iNextReport = 0;
                // Modified by Hemin for Bug fix
                // Dim sReportStyleName As String = Languages.Translation("msgAdminCtrlNewReportStyle")
                string sReportStyleName = "New Report Style";
                do
                {
                    bFound = false;
                    if (iNextReport == 0)
                    {
                        // Modified by Hemin for Bug fix
                        // sReportStyleName = Languages.Translation("msgAdminCtrlNewReportStyle")
                        sReportStyleName = "New Report Style";
                    }
                    else
                    {
                        // Modified by Hemin for Bug fix
                        // sReportStyleName = String.Format(Languages.Translation("msgAdminCtrlNewReportStyle1"), iNextReport)
                        sReportStyleName = "New Report Style " + iNextReport;
                    }

                    foreach (ReportStyle oReportStyle in (IEnumerable)allReportStyle)
                    {
                        if (Strings.StrComp(oReportStyle.Id.Trim().ToLower(), sReportStyleName.Trim().ToLower(), Constants.vbTextCompare) == 0)
                        {
                            iNextReport = iNextReport + 1;
                            sReportStyleName = oReportStyle.Id;
                            bFound = true;
                            break;
                        }
                    }
                    if (!bFound)
                    {
                        break;
                    }
                }
                while (true);

                string sReportStyleNameJSON = JsonConvert.SerializeObject(sReportStyleName, Newtonsoft.Json.Formatting.Indented, Setting);
                return Json(new
                {
                    sReportStyleNameJSON,
                    jsonObject
                });
            }
            else
            {
                return Json(jsonObject);
            }
        }

        public IActionResult RemoveReportStyle(int selectedRowsVar, string reportStyleVar)
        {
            IRepository<ReportStyle> _iReportStyle = new Repositories<ReportStyle>();
            try
            {
                var reportStyleEntity = _iReportStyle.All().Where(m => m.Id.Trim().ToLower().Equals(reportStyleVar.Trim().ToLower()) & m.ReportStylesId == selectedRowsVar).FirstOrDefault();
                if (reportStyleEntity is not null)
                {
                    _iReportStyle.Delete(reportStyleEntity);
                }
                Keys.ErrorMessage = Keys.DeleteReportSuccessMessage();
                Keys.ErrorType = "s";
            }
            catch (Exception)
            {
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                Keys.ErrorType = "e";
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        public IActionResult SetReportStylesData([FromForm] ReportStyle formEntity, [FromForm] bool pFixedLines, [FromForm] bool pAltRowShading, [FromForm] bool pReportCentered)
        {
            IRepository<ReportStyle> _iReportStyle = new Repositories<ReportStyle>();
            try
            {
                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                formEntity.FixedLines = pFixedLines;
                formEntity.AltRowShading = pAltRowShading;
                formEntity.ReportCentered = pReportCentered;
                if (formEntity.ReportStylesId > 0)
                {
                    var reportStyleEntity = _iReportStyle.All().Where(m => m.ReportStylesId == formEntity.ReportStylesId).FirstOrDefault();
                    // Dim flagBool = reportStyleEntity.ReportStylesName.Trim.ToLower.Equals(formEntity.ReportStylesName.Trim.ToLower)
                    if (!string.IsNullOrEmpty(reportStyleEntity.Id))
                    {
                        if (!reportStyleEntity.Id.Trim().ToLower().Equals(formEntity.Id.Trim().ToLower()))
                        {
                            var reportStyleAll = _iReportStyle.All().OrderBy(m => m.ReportStylesId);
                            foreach (ReportStyle reportObj in reportStyleAll)
                            {
                                if (reportObj.Id.Trim().ToLower().Equals(formEntity.Id.Trim().ToLower()) & reportObj.ReportStylesId != formEntity.ReportStylesId)
                                {
                                    Keys.ErrorType = "w";
                                    Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlRptStyleExists"), reportObj.Id);
                                    return Json(new
                                    {
                                        errortype = Keys.ErrorType,
                                        message = Keys.ErrorMessage
                                    });
                                }
                            }
                        }
                    }

                    reportStyleEntity = AddReportStyle(reportStyleEntity, formEntity);
                    _iReportStyle.Update(reportStyleEntity);
                    Keys.ErrorType = "s";
                    // Keys.ErrorMessage = Languages.Translation("msgAdminCtrlReUpdatedSuccessfully")
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlReportStyleUpdateSuccessfully");
                }
                else
                {
                    _iReportStyle.Add(formEntity);
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Keys.SaveSuccessMessage();
                }
                var reportBack = _iReportStyle.All().Where(m => m.Id.Trim().ToLower().Equals(formEntity.Id.Trim().ToLower())).FirstOrDefault();
                string reportBackJSON = JsonConvert.SerializeObject(reportBack, Newtonsoft.Json.Formatting.Indented, Setting);

                return Json(new
                {
                    reportBackJSON,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();

                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
        }

        public ReportStyle AddReportStyle(ReportStyle reportStyleEntity, ReportStyle formEntity)
        {
            reportStyleEntity.ReportStylesId = formEntity.ReportStylesId;
            reportStyleEntity.Id = formEntity.Id;
            reportStyleEntity.Description = formEntity.Description;
            reportStyleEntity.Heading1Left = formEntity.Heading1Left;
            reportStyleEntity.Heading1Center = formEntity.Heading1Center;
            reportStyleEntity.Heading1Right = formEntity.Heading1Right;
            reportStyleEntity.Heading2Center = formEntity.Heading2Center;
            reportStyleEntity.FooterLeft = formEntity.FooterLeft;
            reportStyleEntity.FooterCenter = formEntity.FooterCenter;
            reportStyleEntity.FooterRight = formEntity.FooterRight;
            reportStyleEntity.Orientation = formEntity.Orientation;
            reportStyleEntity.HeaderSize = formEntity.HeaderSize;
            reportStyleEntity.ShadowSize = formEntity.ShadowSize;
            reportStyleEntity.MinColumnWidth = formEntity.MinColumnWidth;
            reportStyleEntity.BlankLineSpacing = formEntity.BlankLineSpacing;
            reportStyleEntity.ColumnSpacing = formEntity.ColumnSpacing;
            reportStyleEntity.BoxWidth = formEntity.BoxWidth;
            reportStyleEntity.MaxLines = formEntity.MaxLines;
            reportStyleEntity.FixedLines = formEntity.FixedLines;
            reportStyleEntity.AltRowShading = formEntity.AltRowShading;
            reportStyleEntity.ReportCentered = formEntity.ReportCentered;
            reportStyleEntity.TextForeColor = formEntity.TextForeColor;
            reportStyleEntity.LineColor = formEntity.LineColor;
            reportStyleEntity.ShadeBoxColor = formEntity.ShadeBoxColor;
            reportStyleEntity.ShadowColor = formEntity.ShadowColor;
            reportStyleEntity.ShadedLineColor = formEntity.ShadedLineColor;
            reportStyleEntity.LeftMargin = formEntity.LeftMargin;
            reportStyleEntity.RightMargin = formEntity.RightMargin;
            reportStyleEntity.TopMargin = formEntity.TopMargin;
            reportStyleEntity.BottomMargin = formEntity.BottomMargin;
            reportStyleEntity.HeadingL1FontBold = formEntity.HeadingL1FontBold;
            reportStyleEntity.HeadingL1FontItalic = formEntity.HeadingL1FontItalic;
            reportStyleEntity.HeadingL1FontUnderlined = formEntity.HeadingL1FontUnderlined;
            reportStyleEntity.HeadingL1FontSize = formEntity.HeadingL1FontSize;
            reportStyleEntity.HeadingL1FontName = formEntity.HeadingL1FontName;

            reportStyleEntity.HeadingL2FontBold = formEntity.HeadingL2FontBold;
            reportStyleEntity.HeadingL2FontItalic = formEntity.HeadingL2FontItalic;
            reportStyleEntity.HeadingL2FontUnderlined = formEntity.HeadingL2FontUnderlined;
            reportStyleEntity.HeadingL2FontSize = formEntity.HeadingL2FontSize;
            reportStyleEntity.HeadingL2FontName = formEntity.HeadingL2FontName;


            reportStyleEntity.SubHeadingFontBold = formEntity.SubHeadingFontBold;
            reportStyleEntity.SubHeadingFontItalic = formEntity.SubHeadingFontItalic;
            reportStyleEntity.SubHeadingFontUnderlined = formEntity.SubHeadingFontUnderlined;
            reportStyleEntity.SubHeadingFontName = formEntity.SubHeadingFontName;
            reportStyleEntity.SubHeadingFontSize = formEntity.SubHeadingFontSize;

            reportStyleEntity.ColumnHeadingFontBold = formEntity.ColumnHeadingFontBold;
            reportStyleEntity.ColumnHeadingFontItalic = formEntity.ColumnHeadingFontItalic;
            reportStyleEntity.ColumnHeadingFontUnderlined = formEntity.ColumnHeadingFontUnderlined;
            reportStyleEntity.ColumnHeadingFontName = formEntity.ColumnHeadingFontName;
            reportStyleEntity.ColumnHeadingFontSize = formEntity.ColumnHeadingFontSize;

            reportStyleEntity.ColumnFontBold = formEntity.ColumnFontBold;
            reportStyleEntity.ColumnFontItalic = formEntity.ColumnFontItalic;
            reportStyleEntity.ColumnFontUnderlined = formEntity.ColumnFontUnderlined;
            reportStyleEntity.ColumnFontName = formEntity.ColumnFontName;
            reportStyleEntity.ColumnFontSize = formEntity.ColumnFontSize;

            reportStyleEntity.FooterFontBold = formEntity.FooterFontBold;
            reportStyleEntity.FooterFontItalic = formEntity.FooterFontItalic;
            reportStyleEntity.FooterFontUnderlined = formEntity.FooterFontUnderlined;
            reportStyleEntity.FooterFontName = formEntity.FooterFontName;
            reportStyleEntity.FooterFontSize = formEntity.FooterFontSize;

            return reportStyleEntity;
        }

        public JsonResult LoadFontModel()
        {
            string fontJSON = "";
            string fontStyleJSON = "";
            var fontSize = new string[] { "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72" };
            string fontSizeJson = "";

            // Dim fontBold As String = ""
            // Dim fontItalic As String = ""
            // Dim fontUndeline As String = ""

            try
            {
                var fonts = new InstalledFontCollection();
                var FontList = new List<KeyValuePair<string, string>>();
                var FontStyleList = new List<KeyValuePair<string, string>>();
                var FontSizeList = new List<KeyValuePair<string, string>>();
                foreach (FontFamily font in fonts.Families)

                    FontList.Add(new KeyValuePair<string, string>(font.Name, font.Name));

                FontStyle[] styles;
                styles = (FontStyle[])Enum.GetValues(typeof(FontStyle));
                foreach (var style in styles)
                    // combobox1.items.add(style.ToString)
                    FontStyleList.Add(new KeyValuePair<string, string>(style.ToString(), style.ToString()));
                foreach (var fontSizeElement in fontSize)
                    FontSizeList.Add(new KeyValuePair<string, string>(fontSizeElement, fontSizeElement));

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                fontJSON = JsonConvert.SerializeObject(FontList, Newtonsoft.Json.Formatting.Indented, Setting);
                fontStyleJSON = JsonConvert.SerializeObject(FontStyleList, Newtonsoft.Json.Formatting.Indented, Setting);
                fontSizeJson = JsonConvert.SerializeObject(FontSizeList, Newtonsoft.Json.Formatting.Indented, Setting);
            }

            catch (Exception)
            {

            }
            return Json(new
            {
                fontJSON,
                fontStyleJSON,
                fontSizeJson
            });
        }
        #endregion

        #region Security
        // Load the view for Security
        public PartialViewResult LoadSecurityTab()
        {
            return PartialView("_SecurityTabPartial");
        }

        // Check for module level access of Admin Manager.
        [HttpGet]
        public IActionResult CheckModuleLevelAccess()
        {
            var mdlAccessDictionary = new Dictionary<string, bool>();
            bool bAddTabApplication = false;
            bool bAddTabDatabase = false;
            bool bAddTabDirectories = false;
            bool bAddTabData = false;
            bool bAddTabTables = false;
            bool bAddTabViews = false;
            bool bAddTabReports = false;
            bool bAddTabSecuirty = false;

            bool mbSecuriyGroup = false;
            bool mbMgrGroup = false;
            bool bAtLeastOneTablePermission = false;
            bool bAtLeastOneViewPermission = false;
            bool bAdminPermission = false;
            int iCntRpts = 0;
            IRepository<Table> _iTable = new Repositories<Table>();
            IRepository<View> _iView = new Repositories<View>();

            try
            { 

                var lTableEntities = _iTable.All();
                var lViewEntities = _iView.All();

                mbSecuriyGroup = passport.CheckPermission(Common.SECURE_SECURITY, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Permissions.Permission)Enums.PassportPermissions.Access) | passport.CheckPermission(Common.SECURE_SECURITY_USER, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Permissions.Permission)Enums.PassportPermissions.Access);
                mbMgrGroup = passport.CheckAdminPermission(Permissions.Permission.Access);

                if (mbMgrGroup)
                {
                    bAddTabApplication = true;
                    bAddTabDatabase = true;
                    bAddTabData = true;
                }
                bAddTabDirectories = passport.CheckPermission(Common.SECURE_STORAGE, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Permissions.Permission)Enums.PassportPermissions.Access);

                iCntRpts = CollectionsClass.CheckReportsPermission(_iTable.All(), _iView.All(), passport, httpContext);
                bAddTabReports = mbMgrGroup | iCntRpts > 0 | passport.CheckPermission(Common.SECURE_REPORT_STYLES, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Permissions.Permission)Enums.PassportPermissions.Access);

                if (mbSecuriyGroup)
                {
                    bAddTabSecuirty = true;
                }

                bAtLeastOneTablePermission = CollectionsClass.CheckTablesPermission(lTableEntities, mbMgrGroup, passport, httpContext);
                bAtLeastOneViewPermission = CollectionsClass.CheckViewsPermission(lViewEntities, mbMgrGroup, passport, httpContext);
                bAdminPermission = passport.CheckAdminPermission(Permissions.Permission.Access);

                bAddTabTables = mbMgrGroup | bAtLeastOneTablePermission;
                bAddTabViews = mbMgrGroup | bAtLeastOneViewPermission;

                mdlAccessDictionary.Add("Application", bAddTabApplication);
                mdlAccessDictionary.Add("Database", bAddTabDatabase);
                mdlAccessDictionary.Add("Directories", bAddTabDirectories);
                mdlAccessDictionary.Add("Data", bAddTabData);
                mdlAccessDictionary.Add("Tables", bAddTabTables);
                mdlAccessDictionary.Add("Views", bAddTabViews);
                mdlAccessDictionary.Add("Reports", bAddTabReports);
                mdlAccessDictionary.Add("Security", bAddTabSecuirty);
                mdlAccessDictionary.Add("AdminPermission", bAdminPermission);
            }
            catch (Exception)
            {

            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                mdlAccessDictionary
            });
        }
        [HttpPost]
        public IActionResult ValidateApplicationLink([FromBody] ValidateApplicationLinkReq req)
        {
            IRepository<Table> _iTable = new Repositories<Table>();
            var oTables = _iTable.All();
            bool bHaveRights = false;

            if (passport.CheckPermission(req.pModuleNameStr, Smead.Security.SecureObject.SecureObjectType.Application, Permissions.Permission.Access))
            {
                if (req.pModuleNameStr == "Import Setup")
                {
                    foreach (var oTable in oTables)
                    {
                        if (!CollectionsClass.IsEngineTable(oTable.TableName) | CollectionsClass.IsEngineTableOkayToImport(oTable.TableName))
                        {
                            if (passport.CheckPermission(oTable.TableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Permissions.Permission)Enums.PassportPermissions.Import))
                            {
                                bHaveRights = true;
                                break;
                            }
                        }
                    }

                    if (!bHaveRights)
                    {
                        // If (passport.CheckPermission(Common.SECURE_TRACKING, Enums.SecureObjects.Application, Enums.PassportPermissions.Access) Or passport.CheckAdminPermission(Enums.PassportPermissions.Access)) Then
                        if (passport.CheckPermission(Common.SECURE_TRACKING, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Permissions.Permission)Enums.PassportPermissions.Access))
                        {
                            bHaveRights = true;
                        }
                    }

                    if (bHaveRights)
                    {
                        return Json(1);
                    }
                    else
                    {
                        return Json(2);
                    } // Here 2 indicates permission issues for importing table.
                }
                else
                {
                    return Json(1);
                }
            }
            else
            {
                return Json(0);
            }


        }

        #region Users
        // Load the view of Security->Users
        public PartialViewResult LoadSecurityUsersTab()
        {
            return PartialView("_SecurityUsersPartial");
        }
        // Load the view for users proile.
        public PartialViewResult LoadSecurityUserProfileView()
        {
            return PartialView("_AddSecurityUserProfilePartial");
        }
        // Load user security grid.
        public JsonResult LoadSecurityUserGridData(string sidx, string sord, int page, int rows, string pId)
        {
            IRepository<SecureUser> _iSecureUser = new Repositories<SecureUser>();
            var pSecureUserEntities = from t in _iSecureUser.All()
                                      where t.UserID != -1 & (t.AccountType.ToLower() == "s" | t.AccountType.ToLower() == "z")
                                      select new { t.UserID, t.UserName, t.Email, t.FullName, t.AccountDisabled, t.MustChangePassword };

            var jsonData = pSecureUserEntities.GetJsonListForGrid(sord, page, rows, "UserName");

            return Json(jsonData);
        }
        // Set the user details.
        public IActionResult SetUserDetails(SecureUser pUserEntity)
        {
            IRepository<SecureUser> _iSecureUser = new Repositories<SecureUser>();
            do
            {
                try
                {
                    int pNextUserID = 0;
                    if (pUserEntity.UserID > 0)
                    {
                        if (_iSecureUser.All().Any(x => (x.UserName.Trim().ToLower()) == (pUserEntity.UserName.Trim().ToLower()) && x.UserID != pUserEntity.UserID) == false)
                        {
                            var pUserProfileEntity = _iSecureUser.All().Where(x => x.UserID == pUserEntity.UserID).FirstOrDefault();
                            if (pUserProfileEntity != null)
                            {
                                pUserProfileEntity.UserName = Convert.ToString(Interaction.IIf(pUserEntity.UserName is null, "", pUserEntity.UserName));
                                pUserProfileEntity.FullName = Convert.ToString(Interaction.IIf(pUserEntity.FullName is null, "", pUserEntity.FullName));
                                pUserProfileEntity.Email = Convert.ToString(Interaction.IIf(pUserEntity.Email is null, "", pUserEntity.Email));
                                pUserProfileEntity.Misc1 = pUserEntity.Misc1;
                                pUserProfileEntity.Misc2 = pUserEntity.Misc2;
                                pUserProfileEntity.AccountDisabled = pUserEntity.AccountDisabled;
                            }

                            _iSecureUser.Update(pUserProfileEntity);

                            Keys.ErrorType = "s";
                            Keys.ErrorMessage = Languages.get_Translation("msgAdminEditUsers"); // Fixed FUS-6054
                        }
                        else
                        {
                            Keys.ErrorType = "w";
                            Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlTheUserName"), pUserEntity.UserName);
                            break;
                        }
                    }
                    else if (_iSecureUser.All().Any(x => (x.UserName.Trim().ToLower()) == (pUserEntity.UserName.Trim().ToLower())) == false)
                    {

                        // pNextUserID = _iSecureUser.All().ToList().LastOrDefault().UserID + 1
                        // pUserEntity.PasswordHash = Smead.Security.Encrypt.HashPassword(pNextUserID, "password$")
                        pUserEntity.PasswordHash = "";
                        pUserEntity.AccountType = "S";
                        pUserEntity.PasswordUpdate = DateTime.Now;
                        pUserEntity.MustChangePassword = true;   // Modified by Hemin on 11/18/2016
                        pUserEntity.FullName = Convert.ToString(Interaction.IIf(pUserEntity.FullName is null, "", pUserEntity.FullName));
                        pUserEntity.Email = Convert.ToString(Interaction.IIf(pUserEntity.Email is null, "", pUserEntity.Email));
                        pUserEntity.AccountDisabled = false;
                        pUserEntity.DisplayName = pUserEntity.UserName;

                        _iSecureUser.Add(pUserEntity);
                        pNextUserID = pUserEntity.UserID;
                        pUserEntity.PasswordHash = Smead.Security.Encrypt.HashPassword(pNextUserID, "password$");
                        _iSecureUser.Update(pUserEntity);

                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgAdminAddUsers"); // Fixed FUS-6057
                    }
                    else
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlTheUserName"), pUserEntity.UserName);
                        break;
                    }
                }
                catch (Exception)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
            while (false);

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Get the user details for EDIT purpose.
        [HttpPost]
        public IActionResult EditUserProfile([FromForm] string[] pRowSelected)
        {
            if (pRowSelected is null)
            {
                return Json(new { success = false });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { success = false });
            }

            int pUserId = Convert.ToInt32(pRowSelected.GetValue(0));
            if (pUserId == 0)
            {
                return Json(new { success = false });
            }
            IRepository<SecureUser> _iSecureUser = new Repositories<SecureUser>();
            var pUserProfileEntity = _iSecureUser.All().Where(x => x.UserID == pUserId).FirstOrDefault();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(pUserProfileEntity, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        // DELETE user.
        [HttpPost]
        public IActionResult DeleteUserProfile([FromForm] string[] pRowSelected)
        {
            try
            {
                IRepository<SecureUser> _iSecureUser = new Repositories<SecureUser>();
                IRepository<SecureUserGroup> _iSecureUserGroup = new Repositories<SecureUserGroup>();
                if (pRowSelected is null)
                {
                    return Json(new { success = false });
                }
                if (pRowSelected.Length == 0)
                {
                    return Json(new { success = false });
                }

                int pUserId = Convert.ToInt32(pRowSelected.GetValue(0));
                if (pUserId == 0)
                {
                    return Json(new { success = false });
                }

                _iSecureUser.BeginTransaction();
                _iSecureUserGroup.BeginTransaction();
                var pUserProfileEntity = _iSecureUser.All().Where(x => x.UserID == pUserId).FirstOrDefault();
                var pUserGroupEntities = _iSecureUserGroup.All().Where(x => x.UserID == pUserId);

                _iSecureUser.Delete(pUserProfileEntity);
                _iSecureUser.CommitTransaction();
                _iSecureUserGroup.DeleteRange(pUserGroupEntities);


                _iSecureUserGroup.CommitTransaction();
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminDeleteUsers"); // Fixed FUS-6058
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // SET USER PASSWORD
        [HttpPost]
        public IActionResult SetUserPassword([FromForm] int pUserId, [FromForm] string pUserPassword, [FromForm] bool pCheckedState)
        {
            IRepository<SecureUser> _iSecureUser = new Repositories<SecureUser>();
            var pUserEntity = _iSecureUser.All().Where(x => x.UserID == pUserId).FirstOrDefault();
            try
            {
                pUserEntity.PasswordHash = Smead.Security.Encrypt.HashPassword(pUserId, pUserPassword);
                pUserEntity.MustChangePassword = pCheckedState;
                _iSecureUser.Update(pUserEntity);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlPwdChangedSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        // Assign Groups to user.
        [HttpPost]
        public IActionResult SetGroupsAgainstUser([FromForm] int pUserID, [FromForm] string[] pGroupList)
        {
            var pSecureUserGroup = new SecureUserGroup();
            IRepository<SecureUserGroup> _iSecureUserGroup = new Repositories<SecureUserGroup>();
            try
            {
                _iSecureUserGroup.BeginTransaction();
                var pUserGrpEntities = _iSecureUserGroup.All().Where(x => x.UserID == pUserID);
                _iSecureUserGroup.DeleteRange(pUserGrpEntities);
                if (!pGroupList.GetValue(0).ToString().Equals("None"))
                {
                    if (pGroupList.Length > 0)
                    {
                        foreach (var gid in pGroupList)
                        {
                            pSecureUserGroup.UserID = pUserID;
                            pSecureUserGroup.GroupID = Convert.ToInt32(gid);
                            _iSecureUserGroup.Add(pSecureUserGroup);
                        }
                    }
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminSecurityGroupsToUser");
                }
                else
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Languages.get_Translation("msgPleaseselectonerecord");
                }
                _iSecureUserGroup.CommitTransaction();
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                _iSecureUserGroup.RollBackTransaction();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // GET the list of assigned groups for User.
        [HttpPost]
        public IActionResult GetAssignedGroupsForUser([FromForm] int pUserID)
        {
            var lstGroups = new List<string>();
            object jsonObject = null;
            object pSecureGroupEntities = null;
            List<SecureUserGroup> pSecureGroupUserEntities;
            IRepository<SecureGroup> _iSecureGroup = new Repositories<SecureGroup>();
            IRepository<SecureUserGroup> _iSecureUserGroup = new Repositories<SecureUserGroup>();

            try
            {
                pSecureGroupUserEntities = _iSecureUserGroup.All().Where(x => x.UserID == pUserID).ToList();

                foreach (SecureUserGroup item in pSecureGroupUserEntities)
                    lstGroups.Add(item.GroupID.ToString());

                pSecureGroupEntities = from t in _iSecureGroup.All()
                                       where lstGroups.Contains(t.GroupID.ToString())
                                       orderby t.GroupName ascending
                                       select t;

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                jsonObject = JsonConvert.SerializeObject(pSecureGroupEntities, Newtonsoft.Json.Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlDataRetrieved");
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                jsonObject
            });
        }
        // GET all the list of groups from system.
        [HttpPost]
        public IActionResult GetAllGroupsList([FromForm] int pUserID)
        {
            List<SecureGroup> lstAllGroups;
            object grpJsonObject = null;
            IRepository<SecureGroup> _iSecureGroup = new Repositories<SecureGroup>();
            try
            {
                lstAllGroups = _iSecureGroup.All().Where(x => x.GroupID != -1).OrderBy(x => x.GroupName).ToList();

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                grpJsonObject = JsonConvert.SerializeObject(lstAllGroups, Newtonsoft.Json.Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlDataRetrieved");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                grpJsonObject
            });
        }
        // Unlock the user account.
        [HttpPost]
        public IActionResult UnlockUserAccount([FromForm] string pOperatorId)
        {
            try
            {
                // var BaseWebPage = new BaseWebPage();
                passport.LogFailedLogs("Unlock", pOperatorId);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = string.Format(Languages.get_Translation("msgAdminCtrlUACUnlockedSuccessfully"), pOperatorId);
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        #endregion
        #region Groups
        // Load the view of Security->Groups
        public PartialViewResult LoadSecurityGroupsTab()
        {
            return PartialView("_SecurityGroupsPartial");
        }
        // Load the view for groups proile.
        public PartialViewResult LoadSecurityGroupProfileView()
        {
            return PartialView("_AddSecurityGroupProfilePartial");
        }
        // Load group security grid.
        public JsonResult LoadSecurityGroupGridData(string sidx, string sord, int page, int rows, string pId)
        {
            IRepository<SecureGroup> _iSecureGroup = new Repositories<SecureGroup>();
            // Dim pSecureGroupEntities = From t In _iSecureGroup.All()
            // Select t.GroupID, t.GroupName, t.Description, t.AutoLockSeconds, t.AutoLogOffSeconds
            // Made change to have duration in minutes from DB.
            var pSecureGroupEntities = from t in _iSecureGroup.All()
                                       select new { t.GroupID, t.GroupName, t.Description, AutoLockSeconds = t.AutoLockSeconds / 60d, AutoLogOffSeconds = t.AutoLogOffSeconds / 60d };

            var jsonData = pSecureGroupEntities.GetJsonListForGrid(sord, page, rows, "GroupName");

            return Json(jsonData);
        }
        // Set Group profile
        public IActionResult SetGroupDetails(SecureGroup pGroupEntity)
        {
            IRepository<SecureGroup> _iSecureGroup = new Repositories<SecureGroup>();
            try
            {
                if (pGroupEntity.GroupID > -2)
                {
                    var pGroupProfileEntity = _iSecureGroup.All().Where(x => x.GroupID == pGroupEntity.GroupID).FirstOrDefault();

                    pGroupProfileEntity.GroupName = pGroupEntity.GroupName;
                    pGroupProfileEntity.Description = Convert.ToString(Interaction.IIf(pGroupEntity.Description is null, "", pGroupEntity.Description));
                    pGroupProfileEntity.AutoLockSeconds = Convert.ToInt32(Interaction.IIf(Convert.ToBoolean(pGroupEntity.AutoLockSeconds), pGroupEntity.AutoLockSeconds * 60, 0));
                    pGroupProfileEntity.AutoLogOffSeconds = Convert.ToInt32(Interaction.IIf(Convert.ToBoolean(pGroupEntity.AutoLogOffSeconds), pGroupEntity.AutoLogOffSeconds * 60, 0));

                    _iSecureGroup.Update(pGroupProfileEntity);

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminEditGroups"); // Fixed FUS-6055
                }
                else if (_iSecureGroup.All().Any(x => (x.GroupName.Trim().ToLower()) == (pGroupEntity.GroupName.Trim().ToLower())) == false)
                {
                    pGroupEntity.GroupName = Convert.ToString(Interaction.IIf(pGroupEntity.GroupName is null, "", pGroupEntity.GroupName));
                    pGroupEntity.Description = Convert.ToString(Interaction.IIf(pGroupEntity.Description is null, "", pGroupEntity.Description));
                    pGroupEntity.AutoLockSeconds = Convert.ToInt32(Interaction.IIf(Convert.ToBoolean(pGroupEntity.AutoLockSeconds), pGroupEntity.AutoLockSeconds * 60, 0));
                    pGroupEntity.AutoLogOffSeconds = Convert.ToInt32(Interaction.IIf(Convert.ToBoolean(pGroupEntity.AutoLogOffSeconds), pGroupEntity.AutoLogOffSeconds * 60, 0));
                    pGroupEntity.GroupType = "USERGROUP";

                    _iSecureGroup.Add(pGroupEntity);

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminAddGroups"); // Fixed FUS-6056
                }
                else
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlGnAlreadyDefined");
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        [HttpPost]
        public IActionResult EditGroupProfile([FromForm] string[] pRowSelected)
        {
            if (pRowSelected is null)
            {
                return Json(new { success = false });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { success = false });
            }

            int pGroupId = Convert.ToInt32(pRowSelected.GetValue(0));
            if (pGroupId == -2)
            {
                return Json(new { success = false });
            }
            IRepository<SecureGroup> _iSecureGroup = new Repositories<SecureGroup>();
            var pGroupProfileEntity = _iSecureGroup.All().Where(x => x.GroupID == pGroupId).FirstOrDefault();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(pGroupProfileEntity, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        // DELETE Group.
        [HttpPost]
        public IActionResult DeleteGroupProfile([FromForm] string[] pRowSelected)
        {

            try
            {
                if (pRowSelected is null)
                {
                    return Json(new { success = false });
                }
                if (pRowSelected.Length == 0)
                {
                    return Json(new { success = false });
                }

                int pGroupId = Convert.ToInt32(pRowSelected.GetValue(0));
                if (pGroupId == -2)
                {
                    return Json(new { success = false });
                }
                IRepository<SecureGroup> _iSecureGroup = new Repositories<SecureGroup>();
                IRepository<SecureUserGroup> _iSecureUserGroup = new Repositories<SecureUserGroup>();
                _iSecureGroup.BeginTransaction();
                _iSecureUserGroup.BeginTransaction();
                var pGroupProfileEntity = _iSecureGroup.All().Where(x => x.GroupID == pGroupId).FirstOrDefault();
                var pUserGroupEntities = _iSecureUserGroup.All().Where(x => x.GroupID == pGroupId);

                _iSecureGroup.Delete(pGroupProfileEntity);
                _iSecureGroup.CommitTransaction();
                _iSecureUserGroup.DeleteRange(pUserGroupEntities);

                _iSecureUserGroup.CommitTransaction();
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminDeleteGroups"); // Fixed FUS-6059
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // GET the list of assigned users for Group.
        [HttpPost]
        public IActionResult GetAssignedUsersForGroup([FromForm] int pGroupId)
        {
            var lstUsers = new List<string>();
            object jsonObject = null;
            object pSecureUserEntities = null;
            List<SecureUserGroup> pSecureGroupUserEntities;
            IRepository<SecureUser> _iSecureUser = new Repositories<SecureUser>();
            IRepository<SecureUserGroup> _iSecureUserGroup = new Repositories<SecureUserGroup>();

            try
            {
                pSecureGroupUserEntities = _iSecureUserGroup.All().Where(x => x.GroupID == pGroupId).ToList();

                foreach (SecureUserGroup item in pSecureGroupUserEntities)
                    lstUsers.Add(item.UserID.ToString());

                pSecureUserEntities = from t in _iSecureUser.All()
                                      where lstUsers.Contains(t.UserID.ToString())
                                      orderby t.UserName ascending
                                      select t;

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                jsonObject = JsonConvert.SerializeObject(pSecureUserEntities, Newtonsoft.Json.Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlDataSavedSuccessfully");
            }

            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                jsonObject
            });
        }

        // GET all the list of groups from system.
        public IActionResult GetAllUsersList()
        {
            List<SecureUser> lstAllUsers;
            object usrJsonObject = null;
            IRepository<SecureUser> _iSecureUser = new Repositories<SecureUser>();

            try
            {
                lstAllUsers = _iSecureUser.All().Where(x => x.UserID != -1).OrderBy(x => x.UserName).ToList();

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                usrJsonObject = JsonConvert.SerializeObject(lstAllUsers, Newtonsoft.Json.Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlDataRetrieved");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                usrJsonObject
            });
        }

        // Assign Users to group.
        [HttpPost]
        public IActionResult SetUsersAgainstGroup([FromForm] int pGroupID, [FromForm] string[] pUserList)
        {
            var pSecureUserGroup = new SecureUserGroup();
            IRepository<SecureUserGroup> _iSecureUserGroup = new Repositories<SecureUserGroup>();
            try
            {
                _iSecureUserGroup.BeginTransaction();
                var pUserGrpEntities = _iSecureUserGroup.All().Where(x => x.GroupID == pGroupID);
                _iSecureUserGroup.DeleteRange(pUserGrpEntities);
                if (pUserList is not null)
                {
                    if (pUserList.Length > 0)
                    {
                        foreach (var uid in pUserList)
                        {
                            pSecureUserGroup.GroupID = pGroupID;
                            pSecureUserGroup.UserID = Convert.ToInt32(uid);
                            _iSecureUserGroup.Add(pSecureUserGroup);
                        }
                    }
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminSecurityUsersToGroup");
                }
                else
                {
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgPleaseselectonerecord");
                }
                _iSecureUserGroup.CommitTransaction();
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();

                _iSecureUserGroup.RollBackTransaction();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        #endregion
        #region Securables
        // Load the view for Securables
        public PartialViewResult LoadSecuritySecurablesTab()
        {
            return PartialView("_SecuritySecurablesPartial");
        }
        // Get the list of Securable types.
        public IActionResult GetListOfSecurablesType()
        {
            object pSecureObjectEntity = null;
            object jsonObject = null;
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            try
            {
                pSecureObjectEntity = _iSecureObject.All().Where(x => x.BaseID == 0 & x.SecureObjectID > 0 & x.Name.Substring(0, 1) != " ").OrderBy(x => x.Name).ToList();

                //var Setting = new JsonSerializerSettings();
                //Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                //jsonObject = JsonConvert.SerializeObject(pSecureObjectEntity, Newtonsoft.Json.Formatting.Indented, Setting);
                jsonObject = JsonConvert.SerializeObject(pSecureObjectEntity);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlDataRetrievedSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                jsonObject
            });
        }
        // Get the list of Securable objects list.
        [HttpPost]
        public IActionResult GetListOfSecurableObjects([FromForm] int pSecurableTypeID)
        {
            object pSecureObjectEntity = null;
            object jsonObject = null;
            IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            try
            {
                List<SecureObjectPermission> sopData = null;
                sopData = _iSecureObjectPermission.All().ToList();
                pSecureObjectEntity = from o in _iSecureObject.All()
                                      join v in _iSecureObject.All() on o.BaseID equals v.SecureObjectID into ov
                                      let ParentName = ov.FirstOrDefault().Name
                                      where o.BaseID != 0 & o.SecureObjectTypeID == pSecurableTypeID & !o.Name.StartsWith("slRetention") & !o.Name.StartsWith("security")
                                      orderby o.Name
                                      select new { o.SecureObjectID, o.Name, o.SecureObjectTypeID, o.BaseID, ParentName };

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                jsonObject = JsonConvert.SerializeObject(pSecureObjectEntity, Newtonsoft.Json.Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlDataRetrievedSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                jsonObject
            });
        }
        // Get the permission data for Securable Object.
        [HttpPost]
        public IActionResult GetPermissionsForSecurableObject([FromForm] int pSecurableObjID)
        {
            var dsPermissions = new DataSet();
            var dtPermissions = new DataTable();
            object jsonObject = null;
            try
            {
                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
                _IDBManager.CreateParameters(1);
                _IDBManager.AddParameters(0, "@SecurableObjID", pSecurableObjID);

                dsPermissions = _IDBManager.ExecuteDataSet(CommandType.StoredProcedure, "SP_RMS_GetPermissionInfoForSecurableObj");
                _IDBManager.Dispose();
                dtPermissions = dsPermissions.Tables[0];

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                jsonObject = JsonConvert.SerializeObject(dtPermissions, Newtonsoft.Json.Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlDataRetrievedSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                jsonObject
            });
        }
        // Set the permissions to securable object.
        [HttpPost]
        public IActionResult SetPermissionsToSecurableObject([FromForm] int[] pSecurableObjIds, [FromForm] List<int> pPermisionIds, [FromForm] List<int> pPermissionRvmed)
        {

            object pSecurableObjectList = null;
            var pTempPermissionIds = new List<int>();
            var pPermissionEntity = new SecureObjectPermission();
            var pSecurableIdsForView = new List<TabFusionRMS.Models.SecureObject>();
            IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            try
            {
                if (pSecurableObjIds.Length > 0)
                {
                    _iSecureObjectPermission.BeginTransaction();
                    foreach (int pSecurableID in pSecurableObjIds)
                    {

                        // Add all permission Id's which are new.
                        if (!(pPermisionIds == null))
                        {
                            if (pPermisionIds.Count > 0)
                            {
                                foreach (var pPermissionId in pPermisionIds)
                                {
                                    if (!_iSecureObjectPermission.All().Any(x => (x.GroupID == 0 && x.SecureObjectID == pSecurableID) & x.PermissionID == pPermissionId))
                                    {
                                        // 'Assigned new permission
                                        AddNewSecureObjectPermission(pSecurableID, pPermissionId);
                                    }
                                }
                            }
                        }

                        // Remove permission ids
                        if (!(pPermissionRvmed == null))
                        {
                            if (pPermissionRvmed.Count > 0)
                            {
                                // Remove all associated secure object permission for Views - added by Ganesh 12/01/2016.
                                pSecurableIdsForView = _iSecureObject.All().Where(x => x.BaseID == pSecurableID & x.SecureObjectTypeID == (int)Enums.SecureObjectType.View).ToList();
                                if (pSecurableIdsForView.Count > 0)
                                {
                                    foreach (var SecureObj in pSecurableIdsForView)
                                    {
                                        int SecureObjID = SecureObj.SecureObjectID;
                                        foreach (var pPermissionId in pPermissionRvmed)
                                        {
                                            // Changed by Ganesh - 06/01/2016.
                                            if (!(pPermissionId == (int)Enums.PassportPermissions.Configure))
                                            {
                                                if (_iSecureObjectPermission.All().Any(x => x.SecureObjectID == SecureObj.SecureObjectID & x.PermissionID == pPermissionId))
                                                {
                                                    var SecureObjectPerEntities = _iSecureObjectPermission.All().Where(x => x.SecureObjectID == SecureObj.SecureObjectID & x.PermissionID == pPermissionId);
                                                    _iSecureObjectPermission.DeleteRange(SecureObjectPerEntities);
                                                }
                                            }
                                        }
                                    }
                                    // Else
                                    // For Each pPermissionId In pPermissionRvmed
                                    // If _iSecureObjectPermission.All().Any(Function(x) x.SecureObjectID = pSecurableID And x.PermissionID = pPermissionId) Then
                                    // Dim SecureObjectPerEntities = _iSecureObjectPermission.All().Where(Function(x) x.SecureObjectID = pSecurableID And x.PermissionID = pPermissionId)
                                    // _iSecureObjectPermission.DeleteRange(SecureObjectPerEntities)
                                    // End If
                                    // Next
                                }

                                // Remove all associated secure object permission for Annotation and Attachments - added by Ganesh 12/01/2016.
                                var selectedSecureObj = _iSecureObject.All().Where(x => x.SecureObjectID == pSecurableID).FirstOrDefault();
                                // Execute this code if current selected Secure Object is from TABLE.
                                if (selectedSecureObj.SecureObjectTypeID == (int)Enums.SecureObjectType.Table)
                                {
                                    var relatedAnnotationObj = _iSecureObject.All().Where(x => (x.Name) == (selectedSecureObj.Name) & (x.SecureObjectTypeID == (int)Enums.SecureObjectType.Annotations | x.SecureObjectTypeID == (int)Enums.SecureObjectType.Attachments)).ToList();

                                    if (relatedAnnotationObj.Count > 0)
                                    {
                                        foreach (var relatedObj in relatedAnnotationObj)
                                        {
                                            foreach (var pPermissionId in pPermissionRvmed)
                                            {
                                                if (_iSecureObjectPermission.All().Any(x => x.SecureObjectID == relatedObj.SecureObjectID & x.PermissionID == pPermissionId))
                                                {
                                                    var SecureObjectPerEntities = _iSecureObjectPermission.All().Where(x => x.SecureObjectID == relatedObj.SecureObjectID & x.PermissionID == pPermissionId);
                                                    _iSecureObjectPermission.DeleteRange(SecureObjectPerEntities);
                                                }
                                            }
                                        }
                                    }
                                }
                                foreach (var pPermissionId in pPermissionRvmed)
                                {
                                    if (_iSecureObjectPermission.All().Any(x => x.SecureObjectID == pSecurableID & x.PermissionID == pPermissionId))
                                    {
                                        var SecureObjectPerEntities = _iSecureObjectPermission.All().Where(x => x.SecureObjectID == pSecurableID & x.PermissionID == pPermissionId);
                                        _iSecureObjectPermission.DeleteRange(SecureObjectPerEntities);
                                        // 'Keep sync Tables -> Tracking tab Tracking Object and Allow Requiresting checkboxs and Security Securables tab
                                        // 'Removed permission updates in Tables table
                                        if (pPermissionId == 8 | pPermissionId == 9)
                                        {
                                            UpdateTablesTrackingObject("D", pSecurableID, pPermissionId);
                                        }
                                    }

                                    // START: Delete entries for My query and My Fav
                                    var SecureObject = _iSecureObject.All().Where(x => x.SecureObjectID == pSecurableID).FirstOrDefault();
                                    if (SecureObject.Name.Equals(Common.SECURE_MYQUERY))
                                    {
                                        this.RemovePreviousDataForMyQueryOrFavoriate(Common.SECURE_MYQUERY);
                                    }
                                    else if (SecureObject.Name.Equals(Common.SECURE_MYFAVORITE))
                                    {
                                        this.RemovePreviousDataForMyQueryOrFavoriate(Common.SECURE_MYFAVORITE);
                                    }
                                    // END: Delete entries for My query and My Fav
                                }
                            }
                        }
                    }
                    _iSecureObjectPermission.CommitTransaction();
                    // Reload the permissions dataset after updation of permissions.
                    passport.FillSecurePermissions();


                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlPermissionSavedSuccessfully");
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                _iSecureObjectPermission.RollBackTransaction();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Remove previous data for My query and My Favorites.
        private void RemovePreviousDataForMyQueryOrFavoriate(string typeOfFunctionality, int pGroupId = 0)
        {
            IRepository<s_SavedChildrenQuery> _is_SavedChildrenQuery = new Repositories<s_SavedChildrenQuery>();
            IRepository<s_SavedCriteria> _is_SavedCriteria = new Repositories<s_SavedCriteria>();
            IRepository<s_SavedChildrenFavorite> _is_SavedChildrenFavorite = new Repositories<s_SavedChildrenFavorite>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
            if (pGroupId == 0)
            {
                // Handle removing data if action performed on "Securable" section.
                if (typeOfFunctionality.Equals(Common.SECURE_MYQUERY))
                {
                    var allSavedCritriaForMyQuery = _is_SavedCriteria.All().Where(x => x.SavedType == 0);

                    foreach (var savedCritria in allSavedCritriaForMyQuery)
                    {
                        var savedChildrenQuery = _is_SavedChildrenQuery.All().Where(q => q.SavedCriteriaId == savedCritria.Id);
                        _is_SavedChildrenQuery.DeleteRange(savedChildrenQuery);
                    }

                    _is_SavedCriteria.DeleteRange(allSavedCritriaForMyQuery);
                }
                else if (typeOfFunctionality.Equals(Common.SECURE_MYFAVORITE))
                {
                    var allSavedCritriaForMyFav = _is_SavedCriteria.All().Where(x => x.SavedType == 1);

                    foreach (var savedCritria in allSavedCritriaForMyFav)
                    {
                        var savedChildrenFav = _is_SavedChildrenFavorite.All().Where(q => q.SavedCriteriaId == savedCritria.Id);
                        _is_SavedChildrenFavorite.DeleteRange(savedChildrenFav);
                    }

                    _is_SavedCriteria.DeleteRange(allSavedCritriaForMyFav);
                }
            }
            else
            {
                IRepository<SecureUserGroup> _iSecureUserGroup = new Repositories<SecureUserGroup>();

                // 'Handle remove data if action performed on "Permissions" section.
                var lstUsersUnderGrpBeingDel = _iSecureUserGroup.All().Where(x => x.GroupID == pGroupId).Select(y => y.UserID).ToList();
                var allSavedCriteria = _is_SavedCriteria.All().Select(x => x.UserId).Distinct().ToList();
                int secureObjectIdForMyQuery = _iSecureObject.All().Where(x => (x.Name) == Common.SECURE_MYQUERY).Select(y => y.SecureObjectID).FirstOrDefault();
                int secureObjectIdForMyFav = _iSecureObject.All().Where(x => (x.Name) == Common.SECURE_MYFAVORITE).Select(y => y.SecureObjectID).FirstOrDefault();
                bool IsMyQueryForEveryone = _iSecureObjectPermission.All().Any(x => x.GroupID == -1 & x.SecureObjectID == secureObjectIdForMyQuery);
                bool IsMyFavForEveryone = _iSecureObjectPermission.All().Any(x => x.GroupID == -1 & x.SecureObjectID == secureObjectIdForMyFav);
                int cntOfGrpUserPartOf = 0;
                string SQLQuery;

                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();

                foreach (var userid in lstUsersUnderGrpBeingDel)
                {
                    if (allSavedCriteria.Contains(userid))
                    {
                        if (typeOfFunctionality.Equals(Common.SECURE_MYQUERY))
                        {
                            SQLQuery = string.Format(@"SELECT COUNT(SUG.GroupId) as cntGroups FROM SecureUser SU
                                                                INNER JOIN SecureUserGroup SUG ON SU.UserID = SUG.UserID
                                                                INNER JOIN SecureObjectPermission SOG ON SUG.GroupID = SOG.GroupID
                                                                WHERE SU.UserID = {0} AND SecureObjectID = (SELECT SecureObjectID FROM SecureObject WHERE Name = '{1}')", userid, Common.SECURE_MYQUERY);

                            cntOfGrpUserPartOf = Convert.ToInt32(_IDBManager.ExecuteScalar(CommandType.Text, SQLQuery));
                            _IDBManager.Dispose();

                            if (!IsMyQueryForEveryone)
                            {
                                // If user is not part of other group with 'My Queries' permission, then DELETE "Saved Queries" for that user.
                                if (cntOfGrpUserPartOf == 0)
                                {
                                    // Delete entries of my query for user under current group
                                    var allSavedCritriaForMyQuery = _is_SavedCriteria.All().Where(x => x.SavedType == 0 && x.UserId == userid);

                                    foreach (var savedCritria in allSavedCritriaForMyQuery)
                                    {
                                        var savedChildrenQuery = _is_SavedChildrenQuery.All().Where(q => q.SavedCriteriaId == savedCritria.Id);
                                        _is_SavedChildrenQuery.DeleteRange(savedChildrenQuery);
                                    }
                                    _is_SavedCriteria.DeleteRange(allSavedCritriaForMyQuery);
                                }
                            }
                        }
                        else if (typeOfFunctionality.Equals(Common.SECURE_MYFAVORITE))
                        {
                            try
                            {
                                SQLQuery = string.Format(@"SELECT COUNT(SUG.GroupId) as cntGroups FROM SecureUser SU
                                                                INNER JOIN SecureUserGroup SUG ON SU.UserID = SUG.UserID
                                                                INNER JOIN SecureObjectPermission SOG ON SUG.GroupID = SOG.GroupID
                                                                WHERE SU.UserID = {0} AND SecureObjectID = (SELECT SecureObjectID FROM SecureObject WHERE Name = '{1}')", userid, Common.SECURE_MYFAVORITE);

                                cntOfGrpUserPartOf = Convert.ToInt32(_IDBManager.ExecuteScalar(CommandType.Text, SQLQuery));
                                _IDBManager.Dispose();
                                if (!IsMyFavForEveryone)
                                {
                                    // If user is not part of other group with 'My Favorites' permission, then DELETE "Saved Favorites" for that user.
                                    if (cntOfGrpUserPartOf == 0)
                                    {
                                        // Delete entries of my favorites for user under current group
                                        var allSavedCritriaForMyFav = _is_SavedCriteria.All().Where(x => x.SavedType == 1 && x.UserId == userid);
                                        foreach (var savedCritria in allSavedCritriaForMyFav)
                                        {
                                            var savedChildrenFav = _is_SavedChildrenFavorite.All().Where(q => q.SavedCriteriaId == savedCritria.Id);
                                            _is_SavedChildrenFavorite.DeleteRange(savedChildrenFav);
                                        }
                                        _is_SavedCriteria.DeleteRange(allSavedCritriaForMyFav);
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                Keys.ErrorType = "e";
                                Keys.ErrorMessage = Keys.ErrorMessageJS();
                            }
                        }
                    }
                    cntOfGrpUserPartOf = 0;
                    SQLQuery = "";
                }
                // 'Handle Scenario where users don't have My Query/My Fav permission with assigned group(s), but everyone group had permission.
                if (typeOfFunctionality.Equals(Common.SECURE_MYQUERY))
                {
                    if (!IsMyQueryForEveryone & pGroupId == -1)
                    {
                        var dsUserIds = new DataSet();
                        var dtUserIds = new DataTable();

                        SQLQuery = string.Format(@"
                                SELECT Distinct UserId FROM s_SavedCriteria 
                                WHERE UserId NOT IN (
                                        SELECT SU.UserID FROM SecureUser SU
                                        INNER JOIN SecureUserGroup SUG ON SU.UserID = SUG.UserID
                                        INNER JOIN SecureObjectPermission SOG ON SUG.GroupID = SOG.GroupID
        	                            INNER JOIN SecureGroup SG ON SUG.GroupID = SG.GroupID    
        	                            AND SecureObjectID = (SELECT SecureObjectID FROM SecureObject WHERE Name = '{0}')
                                )", Common.SECURE_MYQUERY);

                        dsUserIds = _IDBManager.ExecuteDataSet(CommandType.Text, SQLQuery);
                        dtUserIds = dsUserIds.Tables[0];
                        _IDBManager.Dispose();

                        foreach (DataRow useridRow in dtUserIds.Rows)
                        {
                            int userid = Convert.ToInt32(useridRow["UserId"]);
                            var allSavedCritriaForMyQuery = _is_SavedCriteria.All().Where(x => x.SavedType == 1 && x.UserId == userid);

                            foreach (var savedCritria in allSavedCritriaForMyQuery)
                            {
                                var savedChildrenQuery = _is_SavedChildrenQuery.All().Where(q => q.SavedCriteriaId == savedCritria.Id);
                                _is_SavedChildrenQuery.DeleteRange(savedChildrenQuery);
                            }
                            _is_SavedCriteria.DeleteRange(allSavedCritriaForMyQuery);
                        }
                    }
                }
                else if (typeOfFunctionality.Equals(Common.SECURE_MYFAVORITE))
                {

                    if (!IsMyFavForEveryone & pGroupId == -1)
                    {
                        var dsUserIds = new DataSet();
                        var dtUserIds = new DataTable();

                        SQLQuery = string.Format(@"
                                SELECT Distinct UserId FROM s_SavedCriteria 
                                WHERE UserId NOT IN (
                                        SELECT SU.UserID FROM SecureUser SU
                                        INNER JOIN SecureUserGroup SUG ON SU.UserID = SUG.UserID
                                        INNER JOIN SecureObjectPermission SOG ON SUG.GroupID = SOG.GroupID
        	                            INNER JOIN SecureGroup SG ON SUG.GroupID = SG.GroupID    
        	                            AND SecureObjectID = (SELECT SecureObjectID FROM SecureObject WHERE Name = '{0}')
                                )", Common.SECURE_MYFAVORITE);

                        dsUserIds = _IDBManager.ExecuteDataSet(CommandType.Text, SQLQuery);
                        dtUserIds = dsUserIds.Tables[0];
                        _IDBManager.Dispose();

                        foreach (DataRow useridRow in dtUserIds.Rows)
                        {
                            int userid = Convert.ToInt32(useridRow["UserId"]);
                            var allSavedCritriaForMyFav = _is_SavedCriteria.All().Where(x => x.SavedType == 1 && x.UserId == userid);

                            foreach (var savedCritria in allSavedCritriaForMyFav)
                            {
                                var savedChildrenFav = _is_SavedChildrenQuery.All().Where(q => q.SavedCriteriaId == savedCritria.Id);
                                _is_SavedChildrenQuery.DeleteRange(savedChildrenFav);
                            }
                            _is_SavedCriteria.DeleteRange(allSavedCritriaForMyFav);
                        }
                    }
                }
            }
        }

        #endregion
        #region Permissions
        // Load the view for Permissions.
        public PartialViewResult LoadSecurityPermissionsTab()
        {
            return PartialView("_SecurityPermissionsPartial");
        }

        // Get the list of groups
        public IActionResult GetPermisionsGroupList()
        {
            object jsonObject = null;
            object pSecureGroupEntity = null;
            IRepository<SecureGroup> _iSecureGroup = new Repositories<SecureGroup>();
            try
            {
                pSecureGroupEntity = _iSecureGroup.All().Where(x => x.GroupID != 0).OrderBy(x => x.GroupName);

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                jsonObject = JsonConvert.SerializeObject(pSecureGroupEntity, Newtonsoft.Json.Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlDataRetrievedSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                jsonObject
            });
        }

        // Get the list of Securable objects list.
        [HttpPost]
        public IActionResult GetListOfSecurableObjForPermissions([FromForm] int pSecurableTypeID)
        {
            var dsSecurables = new DataSet();
            var dtSecurables = new DataTable();
            object jsonObject = null;
            try
            {
                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
                _IDBManager.CreateParameters(2);
                _IDBManager.AddParameters(0, "@GroupID", 0);
                _IDBManager.AddParameters(1, "@SecurableTypeID", pSecurableTypeID);

                dsSecurables = _IDBManager.ExecuteDataSet(CommandType.StoredProcedure, "SP_RMS_GetListOfSecurablesById");
                _IDBManager.Dispose();
                dtSecurables = dsSecurables.Tables[0];

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                jsonObject = JsonConvert.SerializeObject(dtSecurables, Newtonsoft.Json.Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlDataRetrievedSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                jsonObject
            });
        }

        // Get the permission data for Securable Object.
        [HttpPost]
        public IActionResult GetPermissionsBasedOnGroupId([FromForm] int pGroupID, [FromForm] int pSecurableObjID)
        {
            var dsPermissions = new DataSet();
            var dtPermissions = new DataTable();
            object jsonObject = null;
            try
            {
                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
                _IDBManager.CreateParameters(2);
                _IDBManager.AddParameters(0, "@GroupID", pGroupID);
                _IDBManager.AddParameters(1, "@SecurableObjID", pSecurableObjID);

                dsPermissions = _IDBManager.ExecuteDataSet(CommandType.StoredProcedure, "SP_RMS_GetPermissionInfoObjBasedOnGroup");
                _IDBManager.Dispose();
                dtPermissions = dsPermissions.Tables[0];

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
                jsonObject = JsonConvert.SerializeObject(dtPermissions, Newtonsoft.Json.Formatting.Indented, Setting);

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlDataRetrievedSuccessfully");
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                jsonObject
            });
        }

        // Set the permisions for group.
        [HttpPost]
        public IActionResult SetGroupPermissions([FromForm] int[] pGroupIds, [FromForm] int[] pSecurableObjIds, [FromForm] List<int> pPermisionIds)
        {
            List<SecureObjectPermission> pSecurableObjectList = new List<SecureObjectPermission>();
            var pTempPermissionIds = new List<int>();
            var pPermissionEntity = new SecureObjectPermission();
            IRepository<SecureObjectPermission> _iSecureObjectPermission = new Repositories<SecureObjectPermission>();
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject = new Repositories<TabFusionRMS.Models.SecureObject>();
            try
            {
                if (pSecurableObjIds.Length > 0)
                {
                    foreach (int pGroupId in pGroupIds)
                    {
                        foreach (int pSecurableID in pSecurableObjIds)
                        {

                            pSecurableObjectList = _iSecureObjectPermission.All().Where(x => x.SecureObjectID == pSecurableID & x.GroupID == pGroupId).ToList();
                            if (!(pPermisionIds == null))
                            {
                                pTempPermissionIds.Clear();
                                pTempPermissionIds.AddRange(pPermisionIds);
                            }

                            // Check for new permission ids and remove from list if exists system already.
                            foreach (var pSecurableObject in pSecurableObjectList)
                            {
                                _iSecureObjectPermission.BeginTransaction();
                                if (pTempPermissionIds.Count > 0)
                                {
                                    var resfind = pTempPermissionIds.Find((x) => Convert.ToInt32(x) == pSecurableObject.PermissionID);
                                    if (resfind > 0)
                                    {
                                        pTempPermissionIds.Remove(resfind);
                                    }
                                    else
                                    {
                                        _iSecureObjectPermission.Delete(pSecurableObject);
                                    }
                                }
                                else if (pTempPermissionIds.Count == 0)
                                {

                                    _iSecureObjectPermission.Delete(pSecurableObject);
                                }
                                _iSecureObjectPermission.CommitTransaction();
                                // START: Delete entries for My query and My Fav
                                var SecureObject = _iSecureObject.All().Where(x => x.SecureObjectID == pSecurableID).FirstOrDefault();
                                if (SecureObject.Name.Equals(Common.SECURE_MYQUERY))
                                {
                                    this.RemovePreviousDataForMyQueryOrFavoriate(Common.SECURE_MYQUERY, pGroupId);
                                }
                                else if (SecureObject.Name.Equals(Common.SECURE_MYFAVORITE))
                                {
                                    this.RemovePreviousDataForMyQueryOrFavoriate(Common.SECURE_MYFAVORITE, pGroupId);
                                }
                                // END: Delete entries for My query and My Fav
                            }
                            // Get the new permissions ids and Insert those into system.
                            foreach (var pPermissionId in pTempPermissionIds)
                            {
                                _iSecureObjectPermission.BeginTransaction();
                                pPermissionEntity.GroupID = pGroupId;
                                pPermissionEntity.SecureObjectID = pSecurableID;
                                pPermissionEntity.PermissionID = pPermissionId;
                                _iSecureObjectPermission.Add(pPermissionEntity);
                                _iSecureObjectPermission.CommitTransaction();
                            }
                        }
                    }

                    // Reload the permissions dataset after updation of permissions.
                    passport.FillSecurePermissions();

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAdminCtrlPermissionSavedSuccessfully");
                }
            }
            catch (Exception)
            {

            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        #endregion
        #region Azure AD
        // Load the view for users proile.
        public PartialViewResult LoadSecurityAzureADView()
        {
            var mappedGroupModel = new AzureADSecurityModel();
            IRepository<SecureGroup> _iSecureGroup = new Repositories<SecureGroup>();
            var secureGroups = _iSecureGroup.Where(x => x.GroupID != -1).OrderBy(x => x.GroupName).ToList();
            var secureGroupsSelectList = secureGroups.CreateSelectListFromList("GroupID", "GroupName", default(int?));
            mappedGroupModel.FusionGroups = secureGroupsSelectList;
            IRepository<Setting> _iSetting = new Repositories<Setting>();
            var azureADSettings = _iSetting.All().Where(s => s.Section.Equals("AzureAD")).ToList();
            var azureADAdmin = azureADSettings.Where(s => s.Item.Equals("AzureADAdmin")).FirstOrDefault();
            var azureADEnabled = azureADSettings.Where(s => s.Item.Equals("EnabledAzureAD")).FirstOrDefault();
            if (azureADEnabled is not null)
            {
                mappedGroupModel.ADEnabled = Convert.ToBoolean(Interaction.IIf(!string.IsNullOrEmpty(azureADEnabled.ItemValue), Convert.ToBoolean(azureADEnabled.ItemValue), false));
            }
            if (azureADAdmin is not null)
            {
                mappedGroupModel.ADAdminUser = Convert.ToString(Interaction.IIf(azureADAdmin is null, string.Empty, azureADAdmin.ItemValue));
            }
            var groups = new List<AzureADGroupsModel>();
            var mappedGroups = new List<GroupsMappingModel>();
            if (httpContext.Session.GetString("authToken") is not null && !string.IsNullOrEmpty(httpContext.Session.GetString("authToken")))
            {
                var _azureADConfiguration = new AzureADConfiguration();
                groups = _azureADConfiguration.GetGroups(httpContext.Session.GetString("authToken"));
                mappedGroups = _azureADConfiguration.GetAzureADGroupMappings();
            }
            mappedGroupModel.ADGroups = groups.CreateSelectListFromList("Id", "DisplayName", default(int?));
            mappedGroupModel.MappedGroups = mappedGroups.CreateSelectListFromList("GroupID", "GroupName", default(int?));

            return PartialView("_AddSecurityAzureADPartial", mappedGroupModel);
        }

        [HttpGet]
        public JsonResult GetAzureADAdminConfiguration()
        {
            IRepository<Setting> _iSetting = new Repositories<Setting>();
            var azureADConfigurations = _iSetting.All().Where(s => s.Section.Equals("AzureAD")).ToList();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(azureADConfigurations, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        [HttpGet]
        public JsonResult GetAzureMappedGroups()
        {
            var _azureADConfiguration = new AzureADConfiguration();
            var mappedGroups = _azureADConfiguration.GetAzureADGroupMappings();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(mappedGroups, Newtonsoft.Json.Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        [HttpPost]
        public JsonResult SetAzureAdmin(string azureAdAdminUser, int fusionGroupId, bool isAdEnabled)
        {
            var _azureADConfiguration = new AzureADConfiguration();
            try
            {
                bool azureDetailValidate = true;
                IRepository<Setting> _iSetting = new Repositories<Setting>();
                var azureADSettings = _iSetting.All().Where(s => s.Section.Equals("AzureAD"));
                if (!azureADSettings.Where(x => x.Item.Equals("ClientId") | x.Item.Equals("ClientSecret")).Count().Equals(2))
                {
                    azureDetailValidate = false;
                }
                if (azureDetailValidate)
                {
                    _azureADConfiguration.SaveAzureADAdminSettings(azureAdAdminUser, isAdEnabled);
                    if (isAdEnabled)
                    {
                        int userId = _azureADConfiguration.SaveMappedUser(azureAdAdminUser, azureAdAdminUser);
                        if (userId > 0)
                        {
                            _azureADConfiguration.SaveUserGroups(userId, fusionGroupId);
                        }
                    }

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAzureADAdminSaveSuccessfully");
                }
                else
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Languages.get_Translation("msgAzureADAdminSaveValidateOtherDetails");
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
        public JsonResult SaveGroupMapping(int fusionRMSGroupId, string adGroupName)
        {
            var _azureADConfiguration = new AzureADConfiguration();
            var mappedGroups = new List<GroupsMappingModel>();
            try
            {
                _azureADConfiguration.SaveMappedGroups(fusionRMSGroupId, adGroupName);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAzureADGroupMappingSuccessfully");
                mappedGroups = _azureADConfiguration.GetAzureADGroupMappings();
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message;
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                mappedgroups = mappedGroups
            });
        }

        [HttpPost]
        public JsonResult DeleteGroupMapping(int fusionRMSGroupId)
        {
            var _azureADConfiguration = new AzureADConfiguration();
            var mappedGroups = new List<GroupsMappingModel>();
            try
            {
                _azureADConfiguration.DeleteMappedGroups(fusionRMSGroupId);
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgAzureADGroupDeleteSuccessfully");
                mappedGroups = _azureADConfiguration.GetAzureADGroupMappings();
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message;
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                mappedgroups = mappedGroups
            });
        }

        [HttpPost]
        public JsonResult SynchronizeGroups()
        {
            if (string.IsNullOrEmpty(AzureADSettings.authToken(httpContext)))
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Languages.get_Translation("msgAzureADInvalidAuthToken");
            }

            var _azureADConfiguration = new AzureADConfiguration();
            try
            {
                bool output = _azureADConfiguration.SyncUsers(AzureADSettings.authToken(httpContext));
                if (output)
                {
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgAzureADSyncUsersSuccessfully");
                }
                else
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Languages.get_Translation("msgAzureADSyncUsersError");
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
        #endregion
        #endregion


        #region Localize
        public PartialViewResult LoadLocalizePartial()
        {
            return PartialView("_LocalizePartial");
        }

        public IActionResult GetAvailableLang()
        {
            IRepository<LookupType> _iLookupType = new Repositories<LookupType>();
            try
            {

                var pDateForm = _iLookupType.All().Where(m => m.LookupTypeForCode.Trim().ToUpper().Equals("DTFRM".Trim().ToUpper())).OrderBy(m => m.SortOrder);

                var Setting = new JsonSerializerSettings();
                Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

                // ' Fill languages dropdown
                var resouceObject = new Dictionary<string, string>();
                resouceObject = Languages.GetXMLLanguages();

                string pLang = JsonConvert.SerializeObject(resouceObject, Newtonsoft.Json.Formatting.Indented, Setting);
                string pDateFormS = JsonConvert.SerializeObject(pDateForm, Newtonsoft.Json.Formatting.Indented, Setting);
                string pLocData = "0";
                if (pDateForm is not null)
                {
                    var pLookupType = pDateForm.Where(x => x.LookupTypeValue.Trim().ToLower().Equals(Keys.GetUserPreferences.sPreferedDateFormat.ToString().Trim().ToLower())).FirstOrDefault();
                    if (pLookupType is not null)
                    {
                        pLocData = pLookupType.LookupTypeCode;
                    }
                }

                var strCulture = ContextService.GetCookiesValueFromMultiple(Keys.CurrentUserName, "PreferedLanguage", httpContext);
                return Json(new
                {
                    pLangS = resouceObject,
                    cultureCode = strCulture,
                    pDateFormS,
                    pLocData,
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });
            }
            catch (Exception)
            {
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                Keys.ErrorType = "e";
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage
                });

            }

        }

        public IActionResult SetLocalizeData(string pPreferedLanguage, string pPreferedDateFormat)
        {
            IRepository<LookupType> _iLookupType = new Repositories<LookupType>();
            try
            {
                if (string.IsNullOrEmpty(pPreferedDateFormat))
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Languages.get_Translation("MsgLocalizeDateValidate");
                }
                else
                {
                    var pLookupType = _iLookupType.All().Where(m => m.LookupTypeForCode.Trim().ToUpper().Equals("DTFRM".Trim().ToUpper()) && m.LookupTypeCode.Equals(pPreferedDateFormat)).FirstOrDefault();

                    IDictionary<string, string> cookiesDic = new Dictionary<string, string>();

                    if (ContextService.GetCookieVal(Keys.CurrentUserName.ToString(), httpContext) is not null)
                    {
                        cookiesDic = ContextService.FromLegacyCookieString(httpContext.Request.Cookies[Keys.CurrentUserName.ToString()]);
                        httpContext.Response.Cookies.Delete(Keys.CurrentUserName.ToString());
                    }

                    bool keyExists = cookiesDic.ContainsKey("PreferedLanguage");
                    if (pPreferedLanguage is not null)
                    {
                        if (keyExists)
                            cookiesDic["PreferedLanguage"] = pPreferedLanguage;
                        else
                            cookiesDic.Add("PreferedLanguage", pPreferedLanguage);
                    }
                    else
                    {
                        if (keyExists)
                            cookiesDic["PreferedLanguage"] = "en-US";
                        else
                            cookiesDic.Add("PreferedLanguage", "en-US");

                    }
                    if (pLookupType is not null)
                    {
                        if (keyExists)
                            cookiesDic["PreferedDateFormat"] = pLookupType.LookupTypeValue;
                        else
                            cookiesDic.Add("PreferedDateFormat", pLookupType.LookupTypeValue);
                    }
                    else
                    {
                        if (keyExists)
                            cookiesDic["PreferedDateFormat"] = pLookupType.LookupTypeValue;
                        else
                            cookiesDic.Add("PreferedDateFormat", new System.Globalization.CultureInfo(cookiesDic["PreferedLanguage"]).DateTimeFormat.ShortDatePattern);

                    }
                    ContextService.RemoveCookie(Keys.CurrentUserName.ToString(), httpContext);
                    ContextService.CreateCookies(Keys.CurrentUserName.ToString(), cookiesDic, httpContext, 180d);

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgLocalizeSave");
                } // Fixed: FUS-6229
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });

        }
        #endregion

        #region Before Login warning message

        public PartialViewResult LoadWarningMessageView()
        {
            return PartialView("_LoginWarningMsgPartial");
        }

        [HttpGet]
        public JsonResult GetWarningMessage([FromServices] IWebHostEnvironment webHostEnvironment)
        {
            string showMessage = string.Empty;
            string warningMessage = string.Empty;
            string path = System.IO.Path.Combine(Convert.ToString(webHostEnvironment.WebRootPath), @"ImportFiles\WarningMessageXML.xml");
            if (System.IO.File.Exists(path))
            {
                XmlReader document = new XmlTextReader(path);
                while (document.Read())
                {
                    var type = document.NodeType;
                    if (type == XmlNodeType.Element)
                    {
                        if (document.Name == "ShowMessage")
                            showMessage = document.ReadInnerXml().ToString();
                        if (document.Name == "WarningMessage")
                            warningMessage = document.ReadInnerXml().ToString();
                    }
                }
                document.Close();
            }
            warningMessage = warningMessage.Remove(0, 1); // remove first last character of a string
            warningMessage = warningMessage.Remove(warningMessage.Length - 1); // remove last character of a string
            string data = showMessage + "||" + warningMessage;
            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, Setting);
            return Json(jsonObject);
        }

        [HttpPost]
        public JsonResult SetWarningMessage([FromServices] IWebHostEnvironment webHostEnvironment,string pWarningMessage, string pShowMessage)
        {

            try
            {
                if (pShowMessage.Trim().ToLower() == "yes" && string.IsNullOrEmpty(pWarningMessage))
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Languages.get_Translation("lblValShowLoginWarningMsg");
                }
                else
                {
                    if (!string.IsNullOrEmpty(pWarningMessage))
                    {
                        if (Convert.ToString(pWarningMessage.TrimStart()[0]) != "\"")
                            pWarningMessage = "\"" + pWarningMessage; // firstLetter
                        if (Convert.ToString(pWarningMessage.Last()) != "\"")
                            pWarningMessage = pWarningMessage + "\""; // lastLetter
                    }

                    var settings = new XmlWriterSettings();
                    settings.Indent = true;
                    string path = System.IO.Path.Combine(Convert.ToString(webHostEnvironment.WebRootPath), @"ImportFiles\WarningMessageXML.xml");
                    var XmlWrt = XmlWriter.Create(path, settings);
                    // Write the Xml declaration.
                    XmlWrt.WriteStartDocument();
                    // Write a comment.
                    XmlWrt.WriteComment("Before login Warning Message Data.");
                    // Write the root element.
                    XmlWrt.WriteStartElement("Data");
                    // Write element.
                    XmlWrt.WriteStartElement("ShowMessage");
                    XmlWrt.WriteString(pShowMessage);
                    XmlWrt.WriteEndElement();
                    XmlWrt.WriteStartElement("WarningMessage");
                    XmlWrt.WriteString(pWarningMessage);
                    XmlWrt.WriteEndElement();
                    // Close the XmlTextWriter.
                    XmlWrt.WriteEndDocument();
                    XmlWrt.Close();
                    Keys.ErrorType = "s";
                    if (pShowMessage.Trim().ToLower() == "no")
                    {
                        Keys.ErrorMessage = Languages.get_Translation("successMessageLoginWarningNo");
                    }
                    else
                    {
                        Keys.ErrorMessage = Languages.get_Translation("successMessageLoginWarning");
                    }
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }
            return Json(new { errortype = Keys.ErrorType, message = Keys.ErrorMessage });
        }
        #endregion

        public async Task<IActionResult> OpenAboutUs([FromServices] ICompositeViewEngine viewEngine)
        {
            try
            {
                ResourceManager rm = new ResourceManager("TabFusionRMS.WebCS.Resource", Assembly.GetExecutingAssembly());
                String tokens = rm.GetString("about");

                var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                var title = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;
                var copyright = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
                tokens = Strings.Replace(tokens, "VersionToken", version);
                tokens = Strings.Replace(tokens, "DescriptionToken", string.Format(Languages.get_Translation("lblAboutUsDescriptionToken"), "TAB FusionRMS")); // My.Application.Info.Description
                tokens = Strings.Replace(tokens, "TradeMarkToken", string.Format(Languages.get_Translation("lblAboutUsTradeMarkToken"), "TAB FusionRMS", "TAB")); // My.Application.Info.Trademark
                tokens = Strings.Replace(tokens, "CopyrightToken", copyright.Replace("2012", System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetYear(DateTime.Now).ToString()));
                tokens = Strings.Replace(tokens, "CurrentYearToken", System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetYear(DateTime.Now).ToString());
                tokens = Strings.Replace(tokens, "AboutToken", string.Format(Languages.get_Translation("lblAboutAbout"), title));
                tokens = Strings.Replace(tokens, "WarningTxtTkn", Languages.get_Translation("lblAboutWarning"));
                tokens = Strings.Replace(tokens, "TechSupportTxtTkn", string.Format(Languages.get_Translation("lblAboutTechSupport"), "(877) 306-8875"));
                tokens = Strings.Replace(tokens, "VersionTxtTkn", string.Format("{0} {1}", Languages.get_Translation("lblAboutVersion"), version));

                string returnView = await RenderPartialViewToString("_AddAboutPartial", viewEngine);
                string sb = string.Format(returnView, string.Format(Languages.get_Translation("lblAboutAbout"), title), tokens);
                return Json(sb);
            }
            catch (Exception)
            {
                return Json("");
            }
        }

        private async Task<string> RenderPartialViewToString(string viewName, ICompositeViewEngine viewEngine)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;


            using (var writer = new StringWriter())
            {
                ViewEngineResult viewResult =
                    viewEngine.FindView(ControllerContext, viewName, false);

                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }

        public class GetFieldTypeListReqModel
        {
            public string pTableName { get; set; }
        }
    }

    public class ValidateApplicationLinkReq{
        public string pModuleNameStr { get; set; }
    }
}

