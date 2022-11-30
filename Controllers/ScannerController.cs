using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Smead.RecordsManagement.Imaging;
using TabFusionRMS.DataBaseManagerVB;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using Microsoft.VisualBasic;

namespace TabFusionRMS.WebCS.Controllers
{

    public class ScannerController : BaseController
    {

        private IRepository<ScanRule> _iScanRule { get; set; }
        private IRepository<OutputSetting> _iOutputSetting { get; set; }
        private IRepository<Volume> _iVolume { get; set; }
        private IRepository<SystemAddress> _iSystemAddress { get; set; }
        private IRepository<Table> _iTable { get; set; }
        private IRepository<SecureObject> _iSecureObject { get; set; }
        private IRepository<Databas> _iDatabas { get; set; }

        private IDBManager _IDBManager = new DBManager();

        // private BaseWebPage myBasePage = new BaseWebPage();
        private Smead.Security.SecureObject oSecureObject;
        
        public ScannerController(IRepository<ScanRule> iScanRule, IRepository<OutputSetting> iOutputSetting, IRepository<Volume> iVolume, IRepository<SystemAddress> iSystemAddress, IRepository<Table> iTable, IRepository<SecureObject> iSecureObject, IRepository<Databas> iDatabas) : base()
        {
            
            _iScanRule = iScanRule;
            _iOutputSetting = iOutputSetting;
            _iVolume = iVolume;
            _iSystemAddress = iSystemAddress;
            _iTable = iTable;
            _iSecureObject = iSecureObject;
            _iDatabas = iDatabas;
            oSecureObject = new Smead.Security.SecureObject(passport);
        }

        public IActionResult Index()
        {
            // Keys.iScannerRefId = Keys.GetUserId

            ContextService.SetSessionValue("iScannerRefId", passport.UserId.ToString(), httpContext);
            var lScanRule = _iScanRule.All();
            var lFinalScanRule = new List<ScanRule>();
            if (passport is not null)
            {
                foreach (ScanRule oScanRule in lScanRule)
                {
                    if (passport.CheckPermission(oScanRule.Id, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.ScanRules, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access))
                    {
                        lFinalScanRule.Add(oScanRule);
                    }
                }
            }
            this.ViewBag.ScanRuleList = lFinalScanRule.CreateSelectListFromList("ScanRulesId", "Id", default);


            var pOutputSettingsEntities = _iSecureObject.All();
            var pOutputSettingsList = new List<SecureObject>();
            var pSecureObjectID = pOutputSettingsEntities.Where(x => x.Name.Trim().ToLower().Equals("output settings")).FirstOrDefault().SecureObjectID;
            pOutputSettingsEntities = pOutputSettingsEntities.Where(x => x.BaseID == pSecureObjectID);
            foreach (var oOutputSettings in pOutputSettingsEntities)
            {
                if (passport.CheckPermission(oOutputSettings.Name, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.OutputSettings, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access))
                {
                    pOutputSettingsList.Add(oOutputSettings);
                }
            }

            var lTableEntities = _iTable.All();

            this.ViewBag.OutputSettingsList = pOutputSettingsList.CreateSelectListFromList("Name", "Name", default);
            this.ViewBag.TablesList = lTableEntities.Where(x => x.Attachments == true).CreateSelectList("TableName", "UserName", default);

            return this.View();
        }

        [HttpPost]
        public async Task<JsonResult> AttachDocuments(object file)
        {
            var context = HttpContext;
            var _file = httpContext.Request.Form.Files;
            DataSet dsTable = null;
            DataTable dtTable = null;
            Databas dbObj = default;
            Table pSelectedTbl = null;

            //var BaseWebPageMain = new BaseWebPage();

            string pOutputSettings = HttpContext.Request.Form["OutPutSettings"];
            string pTableName = HttpContext.Request.Form["TableName"];
            string pTableId = HttpContext.Request.Form["TableId"];

            try
            {
               

                // Added by Ganesh to fix bug #66 from spreadsheet. - 23/02/2016.
                pSelectedTbl = _iTable.All().Where(m => m.TableName == pTableName).FirstOrDefault();
                string pTableField = DatabaseMap.RemoveTableNameFromField(pSelectedTbl.IdFieldName.Trim().ToLower());

                dbObj = _iDatabas.All().Where(m => m.DBName.Trim().ToLower().Equals(pSelectedTbl.DBName.Trim().ToLower())).FirstOrDefault();
                _IDBManager.ConnectionString = Keys.get_GetDBConnectionString(dbObj);


                var oOutputSettings = _iOutputSetting.All().Where(x => x.Id.Trim().ToLower().Equals(pOutputSettings.Trim().ToLower())).FirstOrDefault();
                if (oOutputSettings is null)
                {
                    return this.Json(new { errortype = "e", message = TabFusionRMS.Resource.Languages.get_Translation("msgScannerControllerInvalidDirPath") });
                }
                var oVolumns = _iVolume.All().Where(x => x.Id == oOutputSettings.VolumesId).FirstOrDefault();
                if (oVolumns is null)
                {
                    return this.Json(new { errortype = "e", message = TabFusionRMS.Resource.Languages.get_Translation("msgScannerControllerInvalidDirPath") });
                }
                var oSystemAddress = _iSystemAddress.All().Where(x => x.Id == oVolumns.SystemAddressesId).FirstOrDefault();
                if (oSystemAddress is null)
                {
                    return this.Json(new { errortype = "e", message = TabFusionRMS.Resource.Languages.get_Translation("msgScannerControllerInvalidDirPath") });
                }

                if (!System.IO.Directory.Exists(oSystemAddress.PhysicalDriveLetter))
                {
                    return this.Json(new { errortype = "e", message = TabFusionRMS.Resource.Languages.get_Translation("msgScannerControllerInvalidDirPath") });
                }


                var strqry = "SELECT [" + pTableField + "] FROM [" + pSelectedTbl.TableName + "] ;";
                dsTable = _IDBManager.ExecuteDataSet(CommandType.Text, strqry);
                dtTable = dsTable.Tables[0];


                var foundRows = dtTable.Select(pTableField + "= " + pTableId);
                if (pSelectedTbl.Attachments == true)
                {
                    if (foundRows.Length > 0)
                    {
                        foreach (var pfilePath in _file)
                        {
                            var pfile = pfilePath;

                            // Dim filepath As String = context.Server.MapPath("~/ImportFiles/" + pfile.FileName)
                            //string filepath = context.Server.MapPath("~/ImportFiles/" + pfilePath);
                            //pfile.SaveAs(filepath);
                            string filepath = AppDomain.CurrentDomain.GetData("ContentRootPath") + string.Format("ImportFiles/" + pfile.FileName);
                            using (var stream = System.IO.File.Create(filepath))
                            {
                                await pfile.CopyToAsync(stream);
                            }
                            string ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", ContextService.GetSessionValueByKey("Server", httpContext).ToString(), ContextService.GetSessionValueByKey("databaseName", httpContext).ToString()), pTableName, pTableId).ToString();

                            var attach = Attachments.AddAttachment(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", ContextService.GetSessionValueByKey("Server", httpContext).ToString(), ContextService.GetSessionValueByKey("databaseName", httpContext).ToString()), pTableName, pTableId, 0, pOutputSettings, filepath, filepath, Path.GetExtension(pfile.FileName), false, string.Empty);

                            if (System.IO.File.Exists(filepath))
                            {
                                System.IO.File.Delete(filepath);
                            }
                        }

                        Keys.ErrorType = "s";
                        Keys.ErrorMessage = TabFusionRMS.Resource.Languages.get_Translation("msgScannerControllerFilesAttachsucc");
                    }
                    else
                    {
                        Keys.ErrorType = "e";
                        Keys.ErrorMessage = TabFusionRMS.Resource.Languages.get_Translation("msgScannerControllerRecIdNFound");
                    }
                }
                else
                {
                    Keys.ErrorType = "w";
                    Keys.ErrorMessage = string.Format(TabFusionRMS.Resource.Languages.get_Translation("msgScannerControllerSelTable"), pSelectedTbl.UserName);
                }
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message.ToString();
            }
            finally
            {
                // _IDBManager.Dispose()
                dsTable = null;
                dtTable = null;
            }

            return this.Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        [HttpPost]
        public IActionResult SetScanRule(string pScanRuleName, int pScanRuleId, char pAction)
        {
           
            var lScanRule = _iScanRule.All();
            try
            {

                switch (pAction)
                {
                    case 'N':
                        {
                            if (lScanRule.Any(x => x.Id.Trim().ToLower() == pScanRuleName.Trim().ToLower()) == false)
                            {
                                var oScanRule = new ScanRule();
                                ScannerModel.InitiateScanRule(ref oScanRule);
                                oScanRule.ScanRulesId = 0;
                                oScanRule.Id = pScanRuleName;
                                _iScanRule.Add(oScanRule);
                                oSecureObject.Register(pScanRuleName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.ScanRules, (int)Enums.SecureObjects.ScanRules);
                                oScanRule = default;

                                Keys.ErrorType = "s";
                                Keys.ErrorMessage = Keys.SaveSuccessMessage();
                            }
                            else
                            {
                                Keys.ErrorType = "w";
                                Keys.ErrorMessage = Keys.AlreadyExistMessage(pScanRuleName);
                            }

                            break;
                        }
                    case 'D':
                        {
                            int lSecurableID;
                            string pOrgScanRule = "";
                            var oScanRule = _iScanRule.All().Where(x => x.ScanRulesId == pScanRuleId).FirstOrDefault();
                            pOrgScanRule = oScanRule.Id;
                            if (oScanRule is not null)
                            {
                                _iScanRule.Delete(oScanRule);
                                lSecurableID = Convert.ToInt32(oSecureObject.GetSecureObjectID(pOrgScanRule, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.ScanRules));
                                if (lSecurableID > 0)
                                {
                                    oSecureObject.UnRegister(lSecurableID);
                                }
                            }
                            oScanRule = default;
                            Keys.ErrorType = "s";
                            Keys.ErrorMessage = Keys.DeleteSuccessMessage();
                            break;
                        }
                    case 'C':
                        {
                            if (lScanRule.Any(x => x.Id.Trim().ToLower() == pScanRuleName.Trim().ToLower()) == false)
                            {
                                var cloneScanRule = new ScanRule();
                                var oScanRule = _iScanRule.All().Where(x => x.ScanRulesId == pScanRuleId).FirstOrDefault();
                                ScannerModel.InitiateCloneScanRule(ref cloneScanRule, oScanRule);
                                cloneScanRule.Id = pScanRuleName;
                                cloneScanRule.ScanRulesId = 0;
                                _iScanRule.Add(cloneScanRule);
                                oSecureObject.Register(pScanRuleName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.ScanRules, (int)Enums.SecureObjects.ScanRules);
                                oScanRule = default;
                                cloneScanRule = default;
                                Keys.ErrorType = "s";
                                Keys.ErrorMessage = Keys.SaveSuccessMessage();
                            }
                            else
                            {
                                Keys.ErrorType = "w";
                                Keys.ErrorMessage = Keys.AlreadyExistMessage(pScanRuleName);
                            }

                            break;
                        }
                    case 'R':
                        {
                            if (lScanRule.Any(x => x.Id.Trim().ToLower()== pScanRuleName.Trim().ToLower() && ( x.ScanRulesId == pScanRuleId) == false))
                            {

                                string sSQL;
                                string sRuleMessage;
                                var iBatchCount = default(int);
                                var rsADORecordset = new ADODB.Recordset();
                                var psADOConn = new ADODB.Connection();
                                psADOConn.Open(Keys.get_DefaultConnectionString(true));

                                var oScanRule = _iScanRule.All().Where(x => x.ScanRulesId == pScanRuleId).FirstOrDefault();

                                sSQL = "SELECT COUNT(ScanRulesIdUsed) FROM [ScanBatches] WHERE [ScanRulesIdUsed] = '" + Strings.Replace(oScanRule.Id, "'", "''") + "'";
                                string arglError = "";
                                rsADORecordset = DataServices.GetADORecordSet(ref sSQL, psADOConn, lError: ref arglError);

                                if (rsADORecordset is not null)
                                {
                                    if (!rsADORecordset.EOF)
                                    {
                                        iBatchCount = rsADORecordset.Fields[0].IntValue();
                                    }

                                    rsADORecordset.Close();
                                    rsADORecordset = null;
                                }

                                sRuleMessage = string.Format(TabFusionRMS.Resource.Languages.get_Translation("msgScannerControllerThisWillUpdt"), (object)iBatchCount) + Constants.vbCrLf + TabFusionRMS.Resource.Languages.get_Translation("msgScannerControllerDoUWishToCon");
                                if (iBatchCount > 20)
                                {
                                    sRuleMessage = sRuleMessage + Constants.vbCrLf + Constants.vbCrLf + TabFusionRMS.Resource.Languages.get_Translation("msgScannerControllerThisCldTakeWhile");
                                }


                                oScanRule = default;
                                Keys.ErrorType = "s";
                                Keys.ErrorMessage = sRuleMessage;
                            }
                            else
                            {
                                Keys.ErrorType = "w";
                                Keys.ErrorMessage = Keys.AlreadyExistMessage(pScanRuleName);
                            }

                            break;
                        }

                    default:
                        {
                            Keys.ErrorType = "e";
                            Keys.ErrorMessage = Keys.ErrorMessageJS();
                            break;
                        }
                }
            }

            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message.ToString();
            } // Keys.ErrorMessageJS()
            finally
            {
                lScanRule = null;
            }

            return this.Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public IActionResult RenameRule(string pScanRuleName, int pScanRuleId)
        {

            try
            {

                var rsADORecordset = new ADODB.Recordset();
                var psADOConn = new ADODB.Connection();
                psADOConn.Open(Keys.get_DefaultConnectionString(true));

                string sSQL = "";
                string pOrgScanRule = "";
                var oScanRule = _iScanRule.All().Where(x => x.ScanRulesId == pScanRuleId).FirstOrDefault();
                pOrgScanRule = oScanRule.Id;

                oScanRule.Id = pScanRuleName;
                _iScanRule.Update(oScanRule);


                sSQL = "UPDATE [ScanBatches] SET [ScanRulesIdUsed] = '" + Strings.Replace(pScanRuleName, "'", "''") + "'";
                sSQL = sSQL + " WHERE [ScanRulesIdUsed] = '" + Strings.Replace(oScanRule.Id, "'", "''") + "'";
                string arglError = "";
                rsADORecordset = DataServices.GetADORecordSet(ref sSQL, psADOConn, lError: ref arglError);

                // oSecureObject.RenameSecurable(oScanRule.Id, Enums.SecureObjects.ScanRules, pScanRuleName)

                if (Strings.StrComp(pOrgScanRule, pScanRuleName, Constants.vbTextCompare) != 0)
                {
                    oSecureObject.Rename(pOrgScanRule, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.ScanRules, pScanRuleName);
                }

                oScanRule = default;

                Keys.ErrorType = "s";
                Keys.ErrorMessage = TabFusionRMS.Resource.Languages.get_Translation("msgScannerControllerRuleReSucc");
            }

            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();

            }
            return this.Json(new
            {
                errortype = Keys.ErrorType,
                message = Keys.ErrorMessage
            });
        }

        public IActionResult UpdateScanRulesDropDown()
        {

            var lScanRule = _iScanRule.All();
            var lFinalScanRule = new List<ScanRule>();
            if (passport is not null)
            {
                foreach (ScanRule oScanRule in lScanRule)
                {
                    if (passport.CheckPermission(oScanRule.Id, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.ScanRules, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access))
                    {
                        lFinalScanRule.Add(oScanRule);
                    }
                }
            }

            var Result = from p in lFinalScanRule.OrderBy(x => x.Id)
                         select new { p.ScanRulesId, p.Id };

            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
            string jsonObject = JsonConvert.SerializeObject(Result, Formatting.Indented, Setting);

            return this.Json(jsonObject, "application/json; charset=utf-8");
        }

        public PartialViewResult LoadDiskSourceInputSettingPartial()
        {
            return this.PartialView("_DiskSourcePartial");
        }

    }
}