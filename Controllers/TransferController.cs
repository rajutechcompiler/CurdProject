using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Smead.RecordsManagement;
using static Smead.RecordsManagement.Navigation;
using static Smead.RecordsManagement.Tracking;
using Smead.Security;
using TabFusionRMS.Resource;
using TabFusionRMS.WebCS.Controllers;
using Microsoft.AspNetCore.Mvc;
using TabFusionRMS.WebCS;
using Microsoft.AspNetCore.Http;
using TabFusionRMS.WebCS.FusionElevenModels;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS;

namespace Controllers
{
    public class TransferController : BaseController
    {
        private IWebHostEnvironment _webHostEnvironment { get; set; }
        public TransferController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        // GET: Transfer
        [HttpPost]
        public ActionResult StartTransfering([FromBody] UserInterfaceJsonReqModel req)
        {
            var model = new Transfers();
            if (passport.CheckPermission(req.paramss.TableName, SecureObject.SecureObjectType.Table, Permissions.Permission.Transfer))
            {
                var type = CheckIfBackgroungProcessing(req.paramss);
                ContextService.SetSessionValue("IsBackground", false.ToString(), httpContext);
                switch (type)
                {
                    case Enums.BackgroundTaskProcess.Normal:
                        {
                            model.lblTitle = "Normal";
                            GeneratepopupView(model, req.paramss);
                            break;
                        }
                    case Enums.BackgroundTaskProcess.Background:
                        {
                            model.lblTitle = "background";
                            ContextService.SetSessionValue("IsBackground", true.ToString(), httpContext);
                            GeneratepopupView(model, req.paramss);
                            break;
                        }
                    case Enums.BackgroundTaskProcess.ExceedMaxLimit:
                        {
                            model.lblTitle = "exceededmaxlimit";
                            break;
                        }
                    case Enums.BackgroundTaskProcess.ServiceNotEnabled:
                        {
                            model.lblTitle = "serviceisnotenable";
                            break;
                        }
                }
            }

            return PartialView("_transfer", model);
        }
        [HttpPost]
        public JsonResult GetTransferItems([FromBody] UserInterfaceJsonReqModel req)
        {
            var model = new Transfers();
            var pr = new Parameters(Convert.ToInt32(req.paramss.Transfer.ContainerViewid), passport);
            model.isDueBack = isDueback(pr);
            model.trDestinationsItem = this.GetListofItems(pr, req.paramss.Transfer.TextFilter);

            return Json(model);
        }

        [HttpPost]
        public JsonResult BtnTransfer([FromBody] UserInterfaceJsonReqModel req)
        {
            if (!this.checkIfContainerExist(req.paramss.Transfer.ContainerTableName, req.paramss.Transfer.ContainerItemValue))
                return null;

            var model = new Transfers();
            var dateStr = new DateTime();
            model.isBackground = false;
            model.isWarning = false;
            model.userMsg = Languages.get_Translation("MsgTransfersuccessfuly");
            var pr = new Parameters(Convert.ToInt32(req.paramss.Transfer.ContainerViewid), passport);
            try
            {
                if (TrackingServices.IsOutDestination(req.paramss.Transfer.ContainerTableName, req.paramss.Transfer.ContainerItemValue, httpContext))
                {
                    if (isDueback(pr))
                    {
                        if (string.IsNullOrEmpty(req.paramss.Transfer.TxtDueBack))
                        {
                            model.userMsg = Languages.get_Translation("msgBarCodeTrackingDueBackDateReq");
                            model.isWarning = true;
                            return this.Json(model);
                        }

                        dateStr = DateTime.Parse(req.paramss.Transfer.TxtDueBack, CultureInfo.CurrentCulture);
                        if (DateTime.Parse(DateTime.Now.ToShortDateString()) > DateTime.Parse(dateStr.ToShortDateString()))
                        {
                            model.userMsg = Languages.get_Translation("DueBackDateLessThanCurrent");
                            model.isWarning = true;
                            return this.Json(model);
                        }
                    }
                }

                if (req.paramss.Transfer.IsSelectAllData)
                    req.paramss.ids = this.GetAllDataIds(req.paramss.ViewId);
                if (Convert.ToBoolean(ContextService.GetSessionValueByKey("IsBackground", httpContext)))
                {
                    if (TransferInBackground(req.paramss, model))
                    {
                        model.isBackground = true;
                    }
                    else
                    {
                        model.isWarning = true;
                    }
                }
                else
                {
                    this.SubmitTransferdata(req.paramss.ids, req.paramss);
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return this.Json(model);
        }
        [HttpPost]
        public JsonResult CountAllTransferData(int viewid, List<searchQueryModel> searchQuery)
        {
            int TotalRows = Query.TotalQueryRowCount(Convert.ToString(ContextService.GetSessionValueByKey("HoldTotalRowQuery", httpContext)), passport.Connection());
            return this.Json(TotalRows);
        }
       
        private void SubmitTransferdata(List<string> idlst, UserInterfaceJsonModel @params)
        {
            foreach (string tableId in idlst)
            {
                if (string.IsNullOrEmpty(@params.Transfer.TxtDueBack))
                {
                    TrackingServices.PrepareDataForTransfer(@params.TableName, tableId, @params.Transfer.ContainerTableName, @params.Transfer.ContainerItemValue, default(DateTime), passport.UserName, passport, httpContext);
                }
                else
                {
                    TrackingServices.PrepareDataForTransfer(@params.TableName, tableId, 
                        @params.Transfer.ContainerTableName, 
                        @params.Transfer.ContainerItemValue, Convert.ToDateTime(@params.Transfer.TxtDueBack), passport.UserName, passport, httpContext);
                }
            }
        }

        private void GeneratepopupView(Transfers model, UserInterfaceJsonModel @params)
        {
            this.GetContainerTypes(model, @params.ViewId);

            if (@params.Transfer.IsSelectAllData)
            {
                model.lblTransfer = string.Format(Languages.get_Translation("lblTransfersTransAllTheFollowing"), @params.ViewName);
            }
            else
            {
                model.lblTransfer = string.Format(Languages.get_Translation("lblTransfersTransTheFollowing"), @params.RecordCount, @params.ViewName);
                model.itemsTobeTransfer = LoadItemsTobeTransfer(@params);
            }
        }

        private bool TransferInBackground(UserInterfaceJsonModel @params, Transfers model)
        {
            var datapro = new DataProcessingModel();
            datapro.TaskType = (int)Enums.BackgroundTaskInDetail.Transfer;
            datapro.RecordCount = @params.RecordCount;
            datapro.IsSelectAllData = @params.Transfer.IsSelectAllData;
            datapro.ListofselectedIds = @params.ids;
            datapro.viewId = @params.ViewId;
            datapro.DueBackDate = @params.Transfer.TxtDueBack;
            datapro.ErrorMessage = string.Empty;
            datapro.DestinationTableName = @params.Transfer.ContainerTableName;
            datapro.DestinationTableId = @params.Transfer.ContainerItemValue;

            datapro.FileName = string.Format("{0}_Transfer_{1}", @params.TableName, Guid.NewGuid().ToString());
            datapro.Path = string.Format("{0}{1}.txt", (_webHostEnvironment.WebRootPath + "/BackgroundFiles/"), datapro.FileName);
            bool output = BackgroundStatus.InsertData(datapro, httpContext.Session.GetString("HoldTotalRowQuery")??"", passport, httpContext);
            if (!output)
            {
                model.userMsg = datapro.ErrorMessage;
            }
            else
            {
                model.userMsg = string.Format("{0} % {1} % {2} % {3}", Languages.get_Translation("lblMSGTransferBackgroundSuccess"), Languages.get_Translation("lblMSGExportBackgroundStatus"), datapro.FileName, datapro.RecordCount);
            }

            return output;
        }
        private bool checkIfContainerExist(string containerName, string tableid)
        {
            // check if container id exist if not (possible hacking...user change the id manualy)
            bool result = false;
            try
            {
                string pkey = GetPrimaryKeyFieldName(containerName, passport);
                string command = string.Format("select count({0}) from {1} where {0} = {2}", pkey, containerName, tableid);
                using (var cmd = new SqlCommand(command, passport.Connection()))
                {
                    result = Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
            catch (Exception)
            {
                result = false;
            }
            if (result == false)
            {
                slimShared.SlimShared.logMessage(Languages.get_Translation("MsgSecurityValuation"), false, EventLogEntryType.Error, "TAB FusionRMS Web Access");
            }
            return result;
        }

        private List<TransferRadioBox> GetListofItems(Parameters pr, string txtFilter)
        {
            var qry = new Query(passport);
            var lst = new List<TransferRadioBox>();
            pr.QueryType = queryTypeEnum.LikeItemName;
            pr.Paged = true;
            pr.IsMVCCall = true;
            if (!string.IsNullOrEmpty(txtFilter))
            {
                pr.Text = txtFilter;
            }
            qry.FillData(pr);

            foreach (DataRow item in pr.Data.Rows)
            {
                string id = PrepPad(pr.TableName, Convert.ToString(item["pkey"]), passport);
                lst.Add(new TransferRadioBox() { text = Convert.ToString(item["ItemName"]), ContainerTableName = pr.TableName, value = id });
            }
            return lst;
        }


        private void GetContainerTypes(Transfers model, int viewid)
        {
            var pr = new Parameters(viewid, passport);
            int trackLevel = Convert.ToInt32(pr.TableInfo["TrackingTable"]);
            if (trackLevel == 0)
                trackLevel = int.MaxValue;
            foreach (var item in GetTrackableContainerTypes(passport))
            {
                if (passport.CheckPermission(item.Type, SecureObject.SecureObjectType.Table, Permissions.Permission.View))
                {
                    if (item.Level < trackLevel)
                    {
                        model.lsTaransferType.Add(item.Type.ToString());
                        var lst = new List<TransferRadioBox>();
                        foreach (var v in GetViewsByTableName(item.Type, passport))
                            lst.Add(new TransferRadioBox() { text = v.ViewName, ContainerTableName = item.Type, ContainerViewid = v.Id });
                        model.lsViews.Add(lst);
                    }
                }
            }
        }

        private Enums.BackgroundTaskProcess CheckIfBackgroungProcessing(UserInterfaceJsonModel @params)
        {
            try
            {
                IRepository<TabFusionRMS.Models.Setting> s_Setting = new Repositories<TabFusionRMS.Models.Setting>();
                string expMinVal = s_Setting.Where(x => x.Section == "BackgroundTransfer" & x.Item == "MinValue").FirstOrDefault().ItemValue;
                string expMaxVal = s_Setting.Where(x => x.Section == "BackgroundTransfer" & x.Item == "MaxValue").FirstOrDefault().ItemValue;
                if ((double)@params.RecordCount < Convert.ToDouble(expMinVal))
                {
                    return Enums.BackgroundTaskProcess.Normal;
                }
                else if ((double)@params.RecordCount > Convert.ToDouble(expMaxVal))
                {
                    return Enums.BackgroundTaskProcess.ExceedMaxLimit;
                }
                else if (Convert.ToBoolean(ContextService.GetSessionValueByKey("ServiceManagerEnabled", httpContext)))
                {
                    return Enums.BackgroundTaskProcess.Background;
                }
                else
                {
                    return Enums.BackgroundTaskProcess.ServiceNotEnabled;
                }
            }
            catch (Exception)
            {
                return Enums.BackgroundTaskProcess.Normal;
            }
        }

        private bool isDueback(Parameters pr)
        {
            bool _dueDateOn = CBoolean(GetSystemSetting("DateDueOn", passport));
            bool rtn = true;
            if (!_dueDateOn || pr.TableInfo["TrackingDueBackDaysFieldName"] is DBNull)
                rtn = false;
            return rtn;
        }
        private List<string> LoadItemsTobeTransfer(UserInterfaceJsonModel @params)
        {
            var lst = new List<string>();
            foreach (var id in @params.ids)
                lst.Add(Navigation.GetItemName(@params.TableName, id, passport));
            return lst;
        }
    }

    public class Transfers : BaseModel
    {
        public Transfers()
        {
            lsTaransferType = new List<string>();
            lsViews = new List<List<TransferRadioBox>>();
            trDestinationsItem = new List<TransferRadioBox>();
            itemsTobeTransfer = new List<string>();
        }
        public bool isDueBack { get; set; }
        public List<string> lsTaransferType { get; set; }
        public List<List<TransferRadioBox>> lsViews { get; set; }
        public List<TransferRadioBox> trDestinationsItem { get; set; }
        public List<string> itemsTobeTransfer { get; set; }
        public string userMsg { get; set; }
        public string HtmlMessage { get; set; }
        public string lblTitle { get; set; }
        public bool isBackground { get; set; }
        public string lblTransfer { get; set; }
    }

    public class TransferRadioBox
    {
        public string text { get; set; }
        public string value { get; set; }
        public string ContainerTableName { get; set; }
        public int ContainerViewid { get; set; }
    }

}