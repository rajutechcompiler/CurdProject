using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic
using Newtonsoft.Json;
using Smead.RecordsManagement;
using TabFusionRMS.DataBaseManagerVB;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.Resource;

namespace TabFusionRMS.WebCS.Controllers
{

    public partial class RetentionController : BaseController
    {
        private IRepository<vwGridSetting> _ivwGridSetting { get; set; }
        private IRepository<vwTablesAll> _ivwTablesAll { get; set; }

        private IRepository<SLRetentionCode> _iRetentionCode { get; set; }
        private IRepository<SLRetentionCitation> _iCitationCode { get; set; }
        private IRepository<Table> _iTable { get; set; }
        private IRepository<TabFusionRMS.Models.System> _iSystem { get; set; }

        private IRepository<SLRetentionCitaCode> _iRetentionCitaCode { get; set; }
        private IRepository<Databas> _iDatabas { get; set; }

        // Private Property _IDBManager As IDBManager
        private IDBManager _IDBManager = new DBManager();
        // IDBManager As IDBManager, 
        public RetentionController(IRepository<vwTablesAll> ivwTablesAll, IRepository<vwGridSetting> ivwGridSetting, IRepository<SLRetentionCode> iRetentionCode, IRepository<SLRetentionCitation> iCitationCode, IRepository<SLRetentionCitaCode> iRetentionCitaCode, IRepository<Table> iTable, IRepository<TabFusionRMS.Models.System> iSystem, IRepository<Databas> iDatabas)
        {
            // _IDBManager = IDBManager
            _ivwTablesAll = ivwTablesAll;
            _ivwGridSetting = ivwGridSetting;

            _iRetentionCode = iRetentionCode;
            _iCitationCode = iCitationCode;
            _iRetentionCitaCode = iRetentionCitaCode;
            _iTable = iTable;
            _iDatabas = iDatabas;
            _iSystem = iSystem;
        }

        public IActionResult Index()
        {

            return View();
        }

        // 
        // GET: /Retention

        public ActionResult RetentionCodeMaintenance()
        {
            // Keys.iRetentionRefId = Keys.GetUserId
            ContextService.SetSessionValue("iRetentionRefId", passport.UserId.ToString(), httpContext);
            return View();
        }
        // <HttpPost>
        // <ValidateAntiForgeryToken>
        public IActionResult ReassignRetentionCode()
        {
            return PartialView("_ReassignRetentionCodePartial");
        }

        #region Retention
        #region Retention Code Maintenance

        public PartialViewResult LoadRetentionCodeView()
        {
            return PartialView("AddRetentionCode");
        }

        // Get the screen to show details of citation code.
        public IActionResult DetailedCitationCode()
        {
            return PartialView("DetailedCitationCode");
        }

        // Get the view for Citation codes remained to assign.
        public ActionResult GetAssignCitationCode()
        {
            return PartialView("AssignCitationCode");
        }

        // Get the list of Retention Codes.
        public JsonResult GetRetentionCodes(string sidx, string sord, int page, int rows)
        {

            var pRetentionCodesEntities = from t in _iRetentionCode.All()
                                          select new { t.SLRetentionCodesId, t.Id, t.Description, t.Notes };

            var jsonData = pRetentionCodesEntities.GetJsonListForGrid(sord, page, rows, "Id");

            return Json(jsonData);

        }

        // Get the list of Retention codes based on Citation Codes.
        [HttpPost]
        public IActionResult GetCitationCodesByRetenton([FromForm]string pRetentionCodeId)
        {
            var lstCitationCodes = new List<string>();

            var lstRetentionCodeEntity = _iRetentionCitaCode.All().Where(x => x.SLRetentionCodesId == pRetentionCodeId).ToList();
            foreach (SLRetentionCitaCode item in lstRetentionCodeEntity)
                lstCitationCodes.Add(item.SLRetentionCitationsCitation);

            var pRetentionCodeEntities = from t in _iCitationCode.All()
                                         where lstCitationCodes.Contains(t.Citation)
                                         select new { t.SLRetentionCitationId, t.Citation, t.Subject };

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            var jsonObject = JsonConvert.SerializeObject(pRetentionCodeEntities, Formatting.Indented, Setting);

            return Json(jsonObject);

        }

        // Add/Update the Retention Code record from system.
        [HttpPost]
        public IActionResult SetRetentionCode(SLRetentionCode PRetentionCode)
        {
            string jsonRetCodeObj = string.Empty;

            try
            {
                bool PRetentionLegalHold;
                bool PRetentionPeriodForceToEndOfYear;
                bool PInactivityForceToEndOfYear;

                if (PRetentionCode.SLRetentionCodesId > 0)
                {
                    if (_iRetentionCode.All().Any(x => x.Id.Trim().ToLower() == PRetentionCode.Id.Trim().ToLower() && x.SLRetentionCodesId != PRetentionCode.SLRetentionCodesId) == false)
                    {
                        PRetentionLegalHold = Convert.ToBoolean(Interaction.IIf(Request.Form["RetentionLegalHold"] == "on", true, false));
                        PRetentionPeriodForceToEndOfYear = Convert.ToBoolean(Interaction.IIf(Request.Form["RetentionPeriodForceToEndOfYear"] == "on", true, false));
                        PInactivityForceToEndOfYear = Convert.ToBoolean(Interaction.IIf(Request.Form["chkForceToEndOfTear"] == "on", true, false));

                        PRetentionCode.RetentionLegalHold = PRetentionLegalHold;
                        PRetentionCode.InactivityForceToEndOfYear = PInactivityForceToEndOfYear;
                        PRetentionCode.RetentionPeriodForceToEndOfYear = PRetentionPeriodForceToEndOfYear;
                        PRetentionCode.RetentionPeriodOther = 0; // Reported by Dhaval on 23rd June.
                        PRetentionCode.InactivityEventType = Request.Form["InactivityEventType"] == "" || Request.Form["InactivityEventType"] == "" ? "N/A" : Request.Form["InactivityEventType"].ToString();
                        PRetentionCode.RetentionEventType = Request.Form["RetentionEventType"] == "" || Request.Form["RetentionEventType"] == "" ? "N/A" : Request.Form["RetentionEventType"].ToString();

                        IRepository<SLRetentionCode> iRetentionCode = new Repositories<SLRetentionCode>();
                        var retentionCode = iRetentionCode.Where(x=> x.SLRetentionCodesId == PRetentionCode.SLRetentionCodesId).FirstOrDefault();

                        _iRetentionCode.Update(PRetentionCode);

                        List<Table> pTableEntity = _iTable.All().ToList();

                        if (pTableEntity is not null)
                        {
                            string sSQL = string.Empty;
                            ADODB.Connection oADOConn = default;

                            foreach (Table table in pTableEntity)
                            {
                                if (Convert.ToBoolean(table.RetentionPeriodActive) && retentionCode.Id != PRetentionCode.Id)
                                {
                                    if (oADOConn is null)
                                        oADOConn = DataServices.DBOpen(Enums.eConnectionType.conDefault, default);
                                    sSQL = "UPDATE [" + table.TableName + "] SET [" + DatabaseMap.RemoveTableNameFromField(table.RetentionFieldName) + "] = '" + PRetentionCode.Id + "' WHERE [" + DatabaseMap.RemoveTableNameFromField(table.RetentionFieldName) + "] = '" + retentionCode.Id + "'";
                                    DataServices.ProcessADOCommand(ref sSQL, oADOConn, false);
                                }

                                if (PRetentionLegalHold)
                                {
                                    if (oADOConn is null)
                                        oADOConn = DataServices.DBOpen(Enums.eConnectionType.conDefault, default);
                                    sSQL = string.Format("DELETE FROM [SLDestructCertItems] WHERE ([TableName] = '{0}' AND [RetentionCode] = '{1}' AND [DispositionDate] IS NULL)", table.TableName, PRetentionCode.Id);
                                    DataServices.ProcessADOCommand(ref sSQL, oADOConn, false);
                                }
                            }

                            if (oADOConn is not null)
                            {
                                oADOConn.Close();
                            }
                        }

                        jsonRetCodeObj = GetRetentionCodeId(PRetentionCode.Id);

                        // Dim pRetentionCitaCodes = _iRetentionCitaCode.All().Where(Function(x) x.SLRetentionCodesId = PRetentionCode)
                        // _iRetentionCitaCode.DeleteRange(pRetentionCitaCodes)

                        // Dim pRetentionCitaCodes As SLRetentionCitaCode = New SLRetentionCitaCode()
                        // pRetentionCitaCodes = _iRetentionCitaCode.All().Where(Function(x) x.SLRetentionCodesId = "HUM3000")
                        // pRetentionCitaCodes.SLRetentionCodesId = "HUM123"
                        // _iRetentionCitaCode.UpdateRange(pRetentionCitaCodes)

                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Languages.get_Translation("msgRetentionCodeMaintenanceUpdate"); // Fixed : FUS-6114
                    }
                    else
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Languages.get_Translation("msgRetentionControllerThisRetCodeHasAlreadyBeenDefined");
                    }
                }
                else if (_iRetentionCode.All().Any(x => x.Id.Trim().ToLower() == PRetentionCode.Id.Trim().ToLower()) == false)
                {
                    PRetentionLegalHold = Conversions.ToBoolean(Interaction.IIf(Request.Form["RetentionLegalHold"] == "on", true, false));
                    PRetentionPeriodForceToEndOfYear = Conversions.ToBoolean(Interaction.IIf(Request.Form["RetentionPeriodForceToEndOfYear"] == "on", true, false));
                    PInactivityForceToEndOfYear = Conversions.ToBoolean(Interaction.IIf(Request.Form["chkForceToEndOfTear"] == "on", true, false));

                    PRetentionCode.RetentionLegalHold = PRetentionLegalHold;
                    PRetentionCode.InactivityForceToEndOfYear = PInactivityForceToEndOfYear;
                    PRetentionCode.RetentionPeriodForceToEndOfYear = PRetentionPeriodForceToEndOfYear;
                    PRetentionCode.RetentionPeriodOther = 0; // Reported by Dhaval on 23rd June.
                    PRetentionCode.InactivityEventType = Request.Form["InactivityEventType"].ToString() == "" || Request.Form["InactivityEventType"].ToString() == "" ? "N/A" : Request.Form["InactivityEventType"].ToString();
                    PRetentionCode.RetentionEventType = Request.Form["RetentionEventType"].ToString() == "" || Request.Form["RetentionEventType"].ToString() == "" ? "N/A" : Request.Form["RetentionEventType"].ToString();

                    _iRetentionCode.Add(PRetentionCode);

                    if (PRetentionLegalHold)
                    {
                        ADODB.Connection oADOConn = default;
                        List<Table> pTableEntity = _iTable.All().ToList();

                        if (pTableEntity is not null)
                        {
                            foreach (Table table in pTableEntity)
                            {
                                if (oADOConn is null)
                                    oADOConn = DataServices.DBOpen(Enums.eConnectionType.conDefault, default);
                                string sSQL = string.Format("DELETE FROM [SLDestructCertItems] WHERE ([TableName] = '{0}' AND [RetentionCode] = '{1}' AND [DispositionDate] IS NULL)", table.TableName, PRetentionCode.Id);
                                DataServices.ProcessADOCommand(ref sSQL, oADOConn, false);
                            }
                        }

                        oADOConn.Close();
                    }

                    jsonRetCodeObj = GetRetentionCodeId(PRetentionCode.Id);

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgRetentionCodeMaintenanceSave"); // Fixed : FUS-6113
                }
                else
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Languages.get_Translation("msgRetentionControllerThisRetCodeHasAlreadyBeenDefined");
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
                message = Keys.ErrorMessage,
                jsonRetObj = jsonRetCodeObj
            });
        }

        // Return the fields to edit for retention code.
        [HttpPost]
        public IActionResult EditRetentionCode([FromForm] string[] pRowSelected, [FromForm] string pRetentionCode)
        {

            if (pRowSelected is null)
            {
                return Json(new { success = false });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { success = false });
            }

            var pRetentionCodeEntity = _iRetentionCode.All().Where(x => x.Id == pRetentionCode).FirstOrDefault();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            var jsonObject = JsonConvert.SerializeObject(pRetentionCodeEntity, Formatting.Indented, Setting);

            return Json(jsonObject);

        }


        // Remove the requested retention code record from system.
        [HttpPost]
        public IActionResult RemoveRetentionCodeEntity([FromForm] string[] pRowSelected, [FromForm] string pRetentionCode)
        {

            if (pRowSelected is null)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgRetentionControllerNullValFound") });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgRetentionControllerNullValFound") });
            }

            try
            {
                // If Not IsRetentionCodeInUse(pRetentionCode) Then
                _iRetentionCode.BeginTransaction();
                _iRetentionCitaCode.BeginTransaction();

                var pRetentionCodesEntity = _iRetentionCode.All().Where(x => x.Id == pRetentionCode).FirstOrDefault();

                _iRetentionCode.Delete(pRetentionCodesEntity);

                var pRetentionCitaCodes = _iRetentionCitaCode.All().Where(x => x.SLRetentionCodesId.ToString().Trim().ToLower().Equals(pRetentionCode.Trim().ToLower()));
                _iRetentionCitaCode.DeleteRange(pRetentionCitaCodes);

                _iRetentionCode.CommitTransaction();
                _iRetentionCitaCode.CommitTransaction();

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgRetentionCodeMaintenanceDelete"); // Fixed: FUS-6115
            }
            // Else
            // Keys.ErrorType = "e"
            // Keys.ErrorMessage = "This Retention Code is currently assigned to records and cannot be deleted."
            // End If
            catch (Exception)
            {

                _iRetentionCode.RollBackTransaction();
                _iRetentionCitaCode.RollBackTransaction();

                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });

        }
        // Check if retention code is in use.
        [HttpPost]
        public JsonResult IsRetentionCodeInUse([FromForm]string pRetentionCode)
        {
            bool bRetCodeUsed = false;
            string SQL = "";

            var sADOConn = new ADODB.Connection();
            ADODB.Recordset rsADO;

            try
            {

                foreach (var oTable in _iTable.All())
                {
                    if (Convert.ToBoolean(oTable.RetentionPeriodActive))
                    {
                        SQL = "SELECT COUNT(" + DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName) + ") AS TotalCount FROM [" + oTable.TableName + "] WHERE [" + DatabaseMap.RemoveTableNameFromField(oTable.RetentionFieldName) + "] = '" + pRetentionCode + "'";
                        if (oTable.TableName != "Operators")
                        {
                            string lError = "";
                            sADOConn = DataServices.DBOpen(oTable, _iDatabas.All());
                            rsADO = DataServices.GetADORecordSet(ref SQL, sADOConn, ref lError);

                            if (!(rsADO == null))
                            {
                                if (!rsADO.EOF)
                                    bRetCodeUsed = (rsADO.Fields["TotalCount"].Value.IntValue() > 0);
                                rsADO.Close();
                                rsADO = default;
                            }
                        }
                    }
                    if (bRetCodeUsed)
                    {
                        Keys.ErrorType = "e";
                        Keys.ErrorMessage = Languages.get_Translation("msgRetentionControllerThisRetCodeIsCurAss2Record");
                        break;
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
                message = Keys.ErrorMessage,
                IsRetCodeUsed = bRetCodeUsed
            });
        }
        // Assign the Citation code to Retention code.
        [HttpPost]
        public IActionResult AssignCitationToRetention([FromForm] string pRetentionCodeId, [FromForm] string pCitationCodeId)
        {

            try
            {

                var pRetentionCitaCode = new SLRetentionCitaCode();

                _iRetentionCitaCode.BeginTransaction();

                pRetentionCitaCode.SLRetentionCodesId = pRetentionCodeId;
                pRetentionCitaCode.SLRetentionCitationsCitation = pCitationCodeId;
                _iRetentionCitaCode.Add(pRetentionCitaCode);

                _iRetentionCitaCode.CommitTransaction();

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Keys.SaveSuccessMessage();
            }

            catch (Exception)
            {
                _iRetentionCitaCode.RollBackTransaction();

                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Retrieve the Citation's code list which are not assigned to passed Retention code.
        public JsonResult GetCitationsCodeToAdd(string sidx, string sord, int page, int rows, string pRetentionCodeId)
        {

            var lstCitationIds = new List<string>();

            var lstRetentionCitaCodes = _iRetentionCitaCode.All().Where(x => x.SLRetentionCodesId == pRetentionCodeId).ToList();

            foreach (SLRetentionCitaCode item in lstRetentionCitaCodes)
                lstCitationIds.Add(item.SLRetentionCitationsCitation);

            var pCitationCodesEntities = from t in _iCitationCode.All()
                                         where !lstCitationIds.Contains(t.Citation)
                                         select new { t.SLRetentionCitationId, t.Citation, t.Subject };

            var jsonData = pCitationCodesEntities.GetJsonListForGrid(sord, page, rows, "Citation");

            return Json(jsonData);

        }

        public string GetRetentionCodeId(string pRetentionCode)
        {
            // Dim pRetentionCodesEntity = From t In _iRetentionCode.All()
            // Where t.Id = pRetentionCode
            // Select t.SLRetentionCodesId, t.Id

            var pRetentionCodeEntity = _iRetentionCode.All().Where(x => x.Id == pRetentionCode).FirstOrDefault();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            var jsonObject = JsonConvert.SerializeObject(pRetentionCodeEntity, Formatting.Indented, Setting);

            return jsonObject;

        }

        public JsonResult CheckRetentionCodeExists(string pRetentionCode)
        {
            try
            {

                if (_iRetentionCode.All().Any(x => x.Id.Trim().ToLower() == pRetentionCode.Trim().ToLower()) == true)
                {
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgRetentionControllerThisRetCodeHasAlreadyBeenDefined");
                }
                else
                {
                    Keys.ErrorType = "e";
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
        // Copy Citations from one Retention code to another. Added on 26th August 2015.
        [HttpPost]
        public IActionResult ReplicateCitationForRetentionOnSaveAs([FromForm] string pCopyFromRetCode, [FromForm] string pCopyToRetCode)

        {
            var pNewRetCitaCodeEntity = new SLRetentionCitaCode();

            try
            {
                _iRetentionCitaCode.BeginTransaction();
                var pRetentionCitaCodesEntities = _iRetentionCitaCode.All().Where(x => x.SLRetentionCodesId == pCopyFromRetCode);

                foreach (var pRetCitaEntity in pRetentionCitaCodesEntities)
                {
                    pNewRetCitaCodeEntity.SLRetentionCitationsCitation = pRetCitaEntity.SLRetentionCitationsCitation;
                    pNewRetCitaCodeEntity.SLRetentionCodesId = pCopyToRetCode;
                    _iRetentionCitaCode.Add(pNewRetCitaCodeEntity);
                }
                _iRetentionCitaCode.CommitTransaction();
            }
            catch (Exception)
            {
                _iRetentionCitaCode.RollBackTransaction();

                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }
        // Get the Label to show on ADD retention code screen.
        public IActionResult GetRetentionYearEndValue()

        {
            DateTime dYearEnd;
            var pSystemEntity = _iSystem.All().FirstOrDefault();
            string lblRetentionYrEnd = Languages.get_Translation("lblRetentionControllerRetentionYrEndsDec31");

            try
            {
                if (pSystemEntity.RetentionYearEnd > System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMonth(DateTime.Now))
                {
                    dYearEnd = new DateTime(DateTime.Now.Year, pSystemEntity.RetentionYearEnd ?? 0 + 1, 0);
                }
                else
                {
                    dYearEnd = new DateTime(DateTime.Now.Year + 1, pSystemEntity.RetentionYearEnd ?? 0 + 1, 0);
                }

                lblRetentionYrEnd = string.Format(Languages.get_Translation("lblRetentionControllerRetentionYrEnds"), Strings.Format((object)dYearEnd, "MMMM dd"));
            }
            catch (Exception)
            {

            }

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                lblRetentionYrEnd,
                citaStatus = pSystemEntity.RetentionTurnOffCitations               // Added by Hemin on 12/20/2016 for bug fix
            });
        }
        #endregion

        #region Citation Maintenance
        public IActionResult CitationMaintenance()
        {
            if (IsRetentionTurnOffCitations() == false)
            {
                return View();
            }
            return View("RetentionCodeMaintenance");
        }

        public PartialViewResult LoadAddCitationCodeView()
        {
            return PartialView("AddCitationCode");
        }

        // Get the list of Citation Codes.
        public JsonResult GetCitationCodes(string sidx, string sord, int page, int rows)
        {
            var pCitationCodesEntities = from t in _iCitationCode.All()
                                         select new { t.SLRetentionCitationId, t.Citation, t.Subject };

            var jsonData = pCitationCodesEntities.GetJsonListForGrid(sord, page, rows, "Citation");

            return Json(jsonData);
        }

        // Get the list of Retention codes based on Citation Codes.
        public JsonResult GetRetentionCodesByCitation(string pCitationCodeId)
        {

            var lstRetentionCodes = new List<string>();

            var pRetentionCitaEntities = from t in _iRetentionCitaCode.All()
                                         select t.SLRetentionCodesId;

            var lstCitationCodeEntity = _iRetentionCitaCode.All().Where(x => x.SLRetentionCitationsCitation == pCitationCodeId).ToList();

            foreach (SLRetentionCitaCode item in lstCitationCodeEntity)
                lstRetentionCodes.Add(item.SLRetentionCodesId);

            var pRetentionCodesEntities = from t in _iRetentionCode.All()
                                          where lstRetentionCodes.Contains(t.Id)
                                          select new { t.SLRetentionCodesId, t.Id, t.Description };


            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            var jsonObject = JsonConvert.SerializeObject(pRetentionCodesEntities, Formatting.Indented, Setting);

            return Json(jsonObject);

        }

        // Add/Update the Citation Code record from system.
        [HttpPost]
        public IActionResult SetCitationCode([FromForm]SLRetentionCitation PCitationCode)
        {

            try
            {
                if (PCitationCode.SLRetentionCitationId > 0)
                {

                    if (_iCitationCode.All().Any(x => x.Citation.Trim().ToLower() == PCitationCode.Citation.Trim().ToLower() && x.SLRetentionCitationId != PCitationCode.SLRetentionCitationId) == false)
                    {

                        // Dim pRetentionCitaCode = _iRetentionCitaCode.All.Where(Function(x) x.SLRetentionCitationsCitation)
                        _iCitationCode.Update(PCitationCode);
                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = Keys.EditRetentionSaveSuccessMessage();
                    }

                    else
                    {
                        Keys.ErrorType = "w";
                        Keys.ErrorMessage = Languages.get_Translation("msgRetentionControllerThisCitationCodeHasAlrdyDefined");
                    }
                }
                else if (_iCitationCode.All().Any(x=> x.Citation.Trim().ToLower() == PCitationCode.Citation.Trim().ToLower()) == false)
                {
                    _iCitationCode.Add(PCitationCode);

                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Keys.RetentionSaveSuccessMessage();
                }
                else
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = Languages.get_Translation("msgRetentionControllerThisCitationCodeHasAlrdyDefined");

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

        // Return the fields to edit for Citation code.
        [HttpPost]
        public IActionResult EditCitationCode([FromForm]string[] pRowSelected, [FromForm]int pCitationCodeId)
        {

            if (pRowSelected is null)
            {
                return Json(new { success = false });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { success = false });
            }

            var pRetentionCodeEntity = _iCitationCode.All().Where(x => x.SLRetentionCitationId == pCitationCodeId).FirstOrDefault();

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            var jsonObject = JsonConvert.SerializeObject(pRetentionCodeEntity, Formatting.Indented, Setting);

            return Json(jsonObject);

        }

        // Remove the Citation code Assigned to Retention code.
        [HttpPost]
        public IActionResult RemoveAssignedCitationCode([FromForm] string pRetentionCodeId, [FromForm] string pCitationCodeId)
        {

            try
            {
                var pRetentionCitaCodeEntity = _iRetentionCitaCode.All().Where(x => x.SLRetentionCitationsCitation == pCitationCodeId & x.SLRetentionCodesId == pRetentionCodeId).FirstOrDefault();
                _iRetentionCitaCode.Delete(pRetentionCitaCodeEntity);
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

        // Get the count of Retention Codes for Citation code.
        public JsonResult GetCountOfRetentionCodesForCitation(string pCitationCodeId)
        {

            int retentionCodeCount = 0;
            int RetentionCount = 0;
            var pRetentionCitaCode = _iRetentionCitaCode.All();

            retentionCodeCount = (from rc in pRetentionCitaCode
                                  where rc.SLRetentionCitationsCitation == pCitationCodeId
                                  select rc.SLRetentionCodesId).Count();

            return Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage,
                retentionCodeCount
            });
        }

        // Remove the requested citation code record from system.
        public IActionResult RemoveCitationCodeEntity(string[] pRowSelected, string pCitationCodeId)
        {

            if (pRowSelected is null)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgRetentionControllerNullValFound") });
            }
            if (pRowSelected.Length == 0)
            {
                return Json(new { errortype = "e", message = Languages.get_Translation("msgRetentionControllerNullValFound") });
            }

            try
            {
                _iCitationCode.BeginTransaction();
                _iRetentionCitaCode.BeginTransaction();

                var pCitationCodesEntity = _iCitationCode.All().Where(x => x.Citation == pCitationCodeId).FirstOrDefault();
                _iCitationCode.Delete(pCitationCodesEntity);

                var pRetentionCitaCodesEntity = _iRetentionCitaCode.All().Where(x => x.SLRetentionCitationsCitation == pCitationCodeId);
                _iRetentionCitaCode.DeleteRange(pRetentionCitaCodesEntity);

                _iCitationCode.CommitTransaction();
                _iRetentionCitaCode.CommitTransaction();

                Keys.ErrorType = "s";
                Keys.ErrorMessage = Keys.DeleteSuccessMessage();
            }
            catch (Exception)
            {
                _iCitationCode.RollBackTransaction();
                _iRetentionCitaCode.RollBackTransaction();

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

        #region Reassign a Retention Code

        // Replace old retention code with new on click of OK.
        public JsonResult ReplaceRetentionCode(int pTableId, string pNewRetentionCode, string pOldRetentionCode)
        {
            return ReplaceRetentionCodeInternal(pTableId, pNewRetentionCode, pOldRetentionCode, false);
        }

        public JsonResult ReplaceRetentionCodeIncludingDisposed(int pTableId, string pNewRetentionCode, string pOldRetentionCode)
        {
            return ReplaceRetentionCodeInternal(pTableId, pNewRetentionCode, pOldRetentionCode, true);
        }

        private JsonResult ReplaceRetentionCodeInternal(int pTableId, string pNewRetentionCode, string pOldRetentionCode, bool updateDisposedRecords)
        {
            try
            {
                bool bSuccess = false;
                var pTableEntity = _iTable.All().Where(m => m.TableId == pTableId).FirstOrDefault();
                ADODB.Connection adoConn = DataServices.DBOpen(Enums.eConnectionType.conDefault, default);
                Retention.UpdateRetentionData(pTableEntity.TableName, pNewRetentionCode, pOldRetentionCode, passport);

                string sSQL = string.Format("UPDATE [{0}] SET [{1}] = '{2}' WHERE [{1}] = '{3}'", pTableEntity.TableName, DatabaseMap.RemoveTableNameFromField(pTableEntity.RetentionFieldName), pNewRetentionCode, pOldRetentionCode);
                if (Strings.Len(pOldRetentionCode) == 0)
                    sSQL += string.Format(" OR [{0}] IS NULL", DatabaseMap.RemoveTableNameFromField(pTableEntity.RetentionFieldName));
                if (!updateDisposedRecords)
                    sSQL += " AND ([%slRetentionDispositionStatus] IS NULL OR [%slRetentionDispositionStatus] = 0)";
                bSuccess = DataServices.ProcessADOCommand(ref sSQL, adoConn, false);

                if (bSuccess)
                {
                    Keys.ErrorType = "s";
                    Keys.ErrorMessage = Languages.get_Translation("msgRetentionControllerTheRetCodeUpdateHasCompleted");
                }
            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
            }

            return Json(new
            {
                Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        // Get the list of names for Retention tables.
        public JsonResult GetRetentionTablesList()
        {
            var lstRetentionCode = new List<Table>();

            var pTables = _iTable.All().Where(x => x.RetentionPeriodActive == true || x.RetentionInactivityActive == true && !String.IsNullOrEmpty(x.RetentionFieldName));
            // (x.RetentionInactivityActive = 1 Or x.RetentionPeriodActive = 1) AndAlso
            foreach (var oTable in pTables)
            {
                if (passport.CheckPermission(oTable.TableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, Smead.Security.Permissions.Permission.View))
                {
                    lstRetentionCode.Add(oTable);
                }
            }

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            // Dim jsonObject = JsonConvert.SerializeObject(pTables, Formatting.Indented, Setting)
            var jsonObject = JsonConvert.SerializeObject(lstRetentionCode, Formatting.Indented, Setting);

            return Json(jsonObject);
        }

        // Get the list of all retention codes from system.
        public JsonResult GetRetentionCodeList()
        {

            var pRetentionCodes = _iRetentionCode.All().OrderBy(x => x.Id);

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            var jsonObject = JsonConvert.SerializeObject(pRetentionCodes, Formatting.Indented, Setting);

            return Json(jsonObject);

        }

        #endregion
        #endregion

        public static bool IsRetentionTurnOffCitations()
        {
            bool RetentionTurnOffCitations = false;
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                var pSystemEntity = _iSystem.All().OrderBy(x => x.Id).FirstOrDefault();
                RetentionTurnOffCitations = (bool)pSystemEntity.RetentionTurnOffCitations;
            }
            catch (Exception)
            {

            }

            return RetentionTurnOffCitations;
        }
    }
}