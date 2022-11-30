using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc;
using Smead.Security;
using Smead.RecordsManagement;
using TabFusionRMS.WebCS.FusionElevenModels;
using static Smead.RecordsManagement.Retention;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
//using Smead.RecordsManagement.Navigation;
//using Smead.RecordsManagement.Retention;
//using Smead.RecordsManagement.Tracking;

namespace TabFusionRMS.WebCS.Controllers
{
    public class ReportsController : BaseController
    {
        //public ReportsController(IHttpContextAccessor httpContext) : base(httpContext)
        //{ }
        // GET: Reports
        public IActionResult Index()
        {
            var model = new ReportingMenu(passport, httpContext);
            model.GenerateMenu();
            return View(model);
        }
        [HttpPost]
        public IActionResult RunQuery([FromBody] SearchQueryRequestModal req)
        {
            // create grid object
            var model = new GridDataBinding(passport, req.paramss.ViewId, req.paramss.pageNum, req.paramss.crumbLevel,(int)ViewType.FusionView,"", httpContext);
            // get list of query fields
            try
            {
                if (req.searchQuery != null)
                    model.fvList = GridDataBinding.CreateQuery(req.searchQuery,httpContext);
                // execute the grid - query
                model.ExecuteGridData();
                httpContext.Session.SetString("HoldTotalRowQuery", model.TotalRowsQuery.Substring(9));
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            var jsonresult = Json(model);
            //jsonresult.MaxJsonLength = int.MaxValue;
            return jsonresult;
        }
        // AUDIT REPORTS
        [HttpPost]
        public JsonResult RunAuditSearch([FromBody] AuditReportSearch.UIproperties paramss)
        {

            var model = new AuditReportSearch(passport, httpContext);
            try
            {
                model.RunQuery(paramss, false);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(model);
        }
        // AUDIT REPORTS PAGINATION
        [HttpPost]
        public PartialViewResult RunAuditReportPagination([FromBody] AuditReportSearch.UIproperties paramss)
        {
            var model = new AuditReportSearch(passport, httpContext);
            try
            {
                model.Paging.PageNumber =Convert.ToInt32(paramss.PageNumber);
                model.RunQuery(paramss, true);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return PartialView("_PagingFooter", model.Paging);
        }
        // REPORTS INITIATOR
        [HttpPost]
        public JsonResult InitiateReports([FromBody] ReportingJsonModelReq req)
        {
            var model = new ReportsModels(passport, req.paramss,httpContext);
            try
            {
                model.ExecuteReporting((ReportsType)req.paramss.reportType);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(model);
        }
        // REPORTS INITIATOR PAGINATION
        [HttpPost]
        public PartialViewResult InitiateReportsPagination([FromBody] ReportingJsonModelReq req)
        {
            var model = new ReportsModels(passport, req.paramss, httpContext);
            try
            {
                model.ExecuteReportingPagination((ReportsType)req.paramss.reportType);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return PartialView("_PagingFooter", model.Paging);
        }
        // RETENTION REPORT INITIATOR
        [HttpPost]
        public JsonResult InitiateRetentionReport([FromBody] ReportingJsonModelReq req)
        {
            var model = new RetentionReportModel(passport, req.paramss, httpContext);
            try
            {
                model.ExecuteRetentionReports((ReportsType)req.paramss.reportType);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(model);
        }

        [HttpPost]
        public PartialViewResult InitiateRetentionReportPagination([FromBody] ReportingJsonModelReq req)
        {
            var model = new RetentionReportModel(passport, req.paramss, httpContext);
            try
            {
                model.ExecuteRetentionReportsPagination((ReportsType)req.paramss.reportType);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return PartialView("_PagingFooter", model.Paging);
        }

        public PartialViewResult PagingPartialView(int TotalPage, int TotalRecord, int PerPageRecord, int PageNumber)
        {
            var @params = new PagingModel();
            @params.PerPageRecord = PerPageRecord;
            @params.TotalPage = TotalPage;
            @params.TotalRecord = TotalRecord;
            @params.PageNumber = PageNumber;
            return PartialView("_PagingFooter", @params);
        }
        // BTN REQUEST - SEND REQUEST LIST
        [HttpPost]
        public JsonResult BtnSendRequestToThePullList([FromBody] ReportingJsonModelReq req)
        {
            var model = new ReportsModels(passport, httpContext);
            try
            {
                model.SendRequestToThePullList(req.paramss);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(model);
        }
        // BTN RETENTION - SUBMIT INACTIVE
        [HttpPost]
        public JsonResult BtnSubmitInactive([FromBody] ReportingJsonModelReq param)
        {
            var model = new ReportCommonModel();
            try     
            {
                var location = Smead.RecordsManagement.Tracking.GetLocationsTableName(passport);
                SetListInactive(param.paramss.ids, location, param.paramss.ddlSelected, passport);
            }
            catch (Exception ex)
            {
                // return error message
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(model);
        }
        // BTN SUBMIT disposition - DESTRUCTION, PURGE AND ARCHIVED
        [HttpPost]
        public JsonResult BtnSubmitDisposition([FromBody] ReportingJsonModelReq req)
        {
            var model = new ReportCommonModel();
            if (req.paramss.submitType != null/* TODO Change to default(_) if this is not a reference type */ )
            {
                var location = string.Empty;
                var locationId = string.Empty;
                try
                {
                    FinalDisposition dispositionType = FinalDisposition.None;
                    switch (req.paramss.submitType)
                    {
                        case "destruction":
                            {
                                dispositionType = FinalDisposition.Destruction;
                                break;
                            }

                        case "archive":
                            {
                                location = Smead.RecordsManagement.Tracking.GetLocationsTableName(passport);
                                locationId = req.paramss.locationId;
                                dispositionType = FinalDisposition.PermanentArchive;
                                break;
                            }

                        case "purge":
                            {
                                dispositionType = FinalDisposition.Purge;
                                break;
                            }
                    }
                    if (dispositionType != FinalDisposition.None)
                    {     
                        DateTime dispositionDate = DateTime.Parse(req.paramss.udate);
                        if (ApplyDispositionToList((List<string>)req.paramss.ids, dispositionType, Keys.GetClientIpAddress(httpContext), dispositionDate, passport, location, locationId))
                            ApproveDestruction(req.paramss.reportId.ToString(), dispositionDate, passport);
                    }
                }
                catch (Exception ex)
                {
                    // need to return error message to the eventlog and UI
                    DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                }
            }
            else
                model.Msg = "error violation";

            return Json(model);
        }
        // BTN CREATE NEW REPORTS - DESTRUCTION, PURGE AND ARCHIVED
        [HttpGet]
        public JsonResult BtnCreateNewArchived()
        {
            var model = new ReportCommonModel();
            int isCreated = 0;
            try
            {
                isCreated = CreateEligibleRecordsForReport(FinalDisposition.PermanentArchive, passport);
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            model.errortype = Keys.ErrorType;
            model.iscreated = isCreated;
            return Json(model);
        }
        [HttpGet]
        public JsonResult BtnCreateNewPurged()
        {
            var model = new ReportCommonModel();
            int isCreated = 0;
            try
            {
                isCreated = CreateEligibleRecordsForReport(FinalDisposition.Purge, passport);
            }
            catch (Exception ex)
            {
                // Keys.ErrorType = "e"
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            // model.errortype = Keys.ErrorType
            model.iscreated = isCreated;
            return Json(model);
        }
        [HttpGet]
        public JsonResult BtnCreateNewDestruction()
        {
            var model = new ReportCommonModel();
            int isCreated = 0;
            try
            {
                isCreated = CreateEligibleRecordsForReport(FinalDisposition.Destruction, passport);
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            model.errortype = Keys.ErrorType;
            model.iscreated = isCreated;
            return Json(model);
        }
        // partial view return
        [HttpGet]
        public ActionResult LoadQueryWindow(int viewId, int ceriteriaId)
        {
            var model = new ViewQueryWindow(passport, 0, "", httpContext);
            try
            {
                model.DrawQuery(viewId);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return PartialView("_ReportQuery", model);
        }
        [HttpGet]
        public ActionResult GetauditReportView()
        {
            var model = new AuditReportSearch(passport, httpContext);
            try
            {
                model.loadDialogReportSearch();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return PartialView("_AuditReport", model);
        }
        [HttpGet]
        public ActionResult GetInactivePullList()
        {
            var model = new RetentionButtons(passport);
            try 
            {
               model.GetInactivePopup();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return PartialView("_Inactivepulllist", model);
        }
        [HttpGet]
        public ActionResult GetSubmitForm(string submitType)
        {
            var model = new RetentionButtons(passport);
            try
            {
                 model.GetSubmitForm(submitType);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return PartialView("_submitform", model);
        }
    }
    public class ReportCommonModel : BaseModel
    {
        public string errortype { get; set; }
        public int iscreated { get; set; }
        public string message { get; set; }
    }
}
