
using System.Data.SqlClient;
using System.Text;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic
using Smead.RecordsManagement;
using Smead.Security;
using Microsoft.AspNetCore.Mvc;
using TabFusionRMS.Resource;
using Newtonsoft.Json;
using TabFusionRMS.WebCS;

namespace TabFusionRMS.WebCS.Controllers
{
    public partial class ExporterController : BaseController
    {
        //public ExporterController(IHttpContextAccessor httpContext) : base(httpContext)
        //{ }
        // EXPORT CSV Or TXT SELECTED
        [HttpPost]
        public JsonResult DialogConfirmExportCSVorTXT([FromBody]ExporterJsonModelReqModel req)
        {
            var model = new Exporter(passport);
            model.isRequireBtn = true;
            if (req.paramss.IsSelectAllData)
            {
                req.paramss.RecordCount = Query.TotalQueryRowCount(ContextService.GetSessionValueByKey("HoldTotalRowQuery", httpContext), passport.Connection());
            }
            try
            {

                if (passport.CheckPermission(req.paramss.viewName, SecureObject.SecureObjectType.View,  Permissions.Permission.Export))
                {

                    var type = CheckIfBackgroungProcessing(req.paramss);
                    switch (type)
                    {
                        case var @case when @case == Enums.BackgroundTaskProcess.Normal:
                            {
                                model.lblTitle = req.paramss.IsCSV ? Languages.get_Translation("lblTitleExportCSV") : Languages.get_Translation("lblTitleExportTXT");
                                model.HtmlMessage = string.Format("<p>" + Languages.get_Translation("lblDataTABExport") + "</p><p>{2}</p>", req.paramss.RecordCount, "TAB FusionRMS", Languages.get_Translation("msgDoUWishToContinue"));
                               // Session("objHolder") = @params;
                                ContextService.SetSessionValue("objHolder", JsonConvert.SerializeObject(req.paramss), httpContext);
                                model.IsBackgroundProcessing = false;
                                break;
                            }
                        case var case1 when case1 == Enums.BackgroundTaskProcess.Background:
                            {
                                model.lblTitle = req.paramss.IsCSV ? Languages.get_Translation("lblTitleExportCSV") : Languages.get_Translation("lblTitleExportTXT");
                                model.HtmlMessage = string.Format("<p>" + Languages.get_Translation("lblDataTABExportBackground") + "</p><p>{2}</p>", req.paramss.RecordCount, "TAB FusionRMS", Languages.get_Translation("msgDoUWishToContinue"));
                                req.paramss.IsBackgroundProcessing = true;
                                model.IsBackgroundProcessing = req.paramss.IsBackgroundProcessing;
                                req.paramss.TaskType = 4;
                                
                                ContextService.SetSessionValue("objHolder", JsonConvert.SerializeObject(req.paramss), httpContext);
                                break;
                            }
                        case var case2 when case2 == Enums.BackgroundTaskProcess.ExceedMaxLimit:
                            {
                                model.lblTitle = Languages.get_Translation("lblMSGWarningMessage");
                                model.HtmlMessage = string.Format("<p>" + Languages.get_Translation("msgFnMaxLimitExceeded") + "</p>");
                                model.isRequireBtn = false;
                                break;
                            }
                        case var case3 when case3 == Enums.BackgroundTaskProcess.ServiceNotEnabled:
                            {
                                model.lblTitle = Languages.get_Translation("lblMSGWarningMessage");
                                model.HtmlMessage = string.Format("<p>" + Languages.get_Translation("msgFnServiceNotEnabled") + "</p>");
                                model.isRequireBtn = false;
                                break;
                            }
                    }
                }
                else
                {
                    model.lblTitle = Languages.get_Translation("lblTitleExportCSV");
                    model.HtmlMessage = Languages.get_Translation("msgFnCountDataCSV");
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }
        // raju check imp
        [HttpGet]
        public async void ExportCSVOrTXT()
        {
            var model = new Exporter(passport);
            ExporterJsonModel @params = ContextService.GetObjectFromJson<ExporterJsonModel>("objHolder", httpContext);
            // written for security that if background processing setup to true we shouldn run BackGroundProcessing
            if (@params.IsBackgroundProcessing)
                return;
            if (passport.CheckPermission(@params.viewName, SecureObject.SecureObjectType.View, Permissions.Permission.Export))
            {
                var sb = new StringBuilder();
                try
                {
                    Response.Clear();
                    HttpContext.Response.Headers.Clear();

                    string extension = string.Empty;
                    if (@params.IsCSV)
                    {
                        Response.ContentType = "text/csv";
                        extension = "csv";
                    }
                    else
                    {
                        Response.ContentType = "text/txt";
                        extension = "txt";
                    }
                    Response.Headers.Add("content-disposition", string.Format("attachment;filename={0}.{1}", @params.tableName, extension));
                    BuildHeader(sb, @params);
                    sb.Append(Environment.NewLine);
                    BuildRows(sb, @params);
                    await Response.WriteAsync(sb.ToString());
                    await Response.Body.FlushAsync();
                }
                catch (Exception ex)
                {
                    DataErrorHandler.ErrorHandler(ex, model, @params.viewId, passport.DatabaseName);
                }
            }
        }
        // Export CSV or TXT SELECTED (Reports)
        [HttpPost]
        public JsonResult DialogConfirmExportCSVorTXTReport([FromBody] ExporterReportJsonModelReq req)
        {
            var model = new Exporter(passport);
            model.isRequireBtn = true;
            ExporterReportJsonModel @params = req.paramss;
            try
            {

                if (@params.viewId > 0)
                {
                    if (!passport.CheckPermission(@params.viewName, SecureObject.SecureObjectType.Reports, Permissions.Permission.Export))
                    {
                        model.lblTitle = Languages.get_Translation("lblTitleExportCSV");
                        model.HtmlMessage = Languages.get_Translation("msgFnCountDataCSV");
                        model.Permission = false;
                        return Json(model);
                    }
                    else if (@params.IsSelectAllData)
                    {
                        @params.RecordCount = CountSelectAllData(@params.viewId);
                    }
                }
                else
                {
                    @params.RecordCount = @params.DataRows.Count;
                }

                Enums.BackgroundTaskProcess type;
                if (@params.ReportType == "19")
                {
                    type = CheckIfBackgroungProcessing(@params);
                }
                else
                {
                    type = Enums.BackgroundTaskProcess.Normal;
                }

                switch (type)
                {
                    case var @case when @case == Enums.BackgroundTaskProcess.Normal:
                        {
                            model.lblTitle = @params.IsCSV ? Languages.get_Translation("lblTitleExportCSV") : Languages.get_Translation("lblTitleExportTXT");
                            model.HtmlMessage = string.Format("<p>" + Languages.get_Translation("lblDataTABExport") + "</p><p>{2}</p>", @params.RecordCount, "TAB FusionRMS", Languages.get_Translation("msgDoUWishToContinue"));
                            ContextService.SetSessionValue("objHolder",JsonConvert.SerializeObject(@params), httpContext);
                            model.IsBackgroundProcessing = false;
                            break;
                        }
                    case var case1 when case1 == Enums.BackgroundTaskProcess.Background:
                        {
                            model.lblTitle = @params.IsCSV ? Languages.get_Translation("lblTitleExportCSV") : Languages.get_Translation("lblTitleExportTXT");
                            model.HtmlMessage = string.Format("<p>" + Languages.get_Translation("lblDataTABExportBackground") + "</p><p>{2}</p>", @params.RecordCount, "TAB FusionRMS", Languages.get_Translation("msgDoUWishToContinue"));
                            @params.IsBackgroundProcessing = true;
                            model.IsBackgroundProcessing = @params.IsBackgroundProcessing;
                            @params.TaskType = 4;
                            
                            ContextService.SetSessionValue("objHolder", JsonConvert.SerializeObject(@params), httpContext);
                            break;
                        }
                    case var case2 when case2 == Enums.BackgroundTaskProcess.ExceedMaxLimit:
                        {
                            model.lblTitle = Languages.get_Translation("lblMSGWarningMessage");
                            model.HtmlMessage = string.Format("<p>" + Languages.get_Translation("msgFnMaxLimitExceeded") + "</p>");
                            model.isRequireBtn = false;
                            break;
                        }
                    case var case3 when case3 == Enums.BackgroundTaskProcess.ServiceNotEnabled:
                        {
                            model.lblTitle = Languages.get_Translation("lblMSGWarningMessage");
                            model.HtmlMessage = string.Format("<p>" + Languages.get_Translation("msgFnServiceNotEnabled") + "</p>");
                            model.isRequireBtn = false;
                            break;
                        }
                }
            }


            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }

        [HttpGet]
        public async void ExportCSVOrTXTReport()
        {
            var model = new Exporter(passport);
            ExporterJsonModel @params = ContextService.GetObjectFromJson<ExporterJsonModel>("objHolder", httpContext);
            // written for security that if background processing setup to true we shouldn run BackGroundProcessing
            if (@params.IsBackgroundProcessing)
                return;
            if (@params.viewId > 0)
            {
                if (!passport.CheckPermission(@params.viewName, SecureObject.SecureObjectType.Reports, Permissions.Permission.Export))
                {
                    return;
                }
            }

            var sb = new StringBuilder();
            try
            {
                Response.Clear();
                Response.Headers.Clear();
                string extension = string.Empty;
                if (@params.IsCSV)
                {
                    Response.ContentType = "text/csv";
                    extension = "csv";
                }
                else
                {
                    Response.ContentType = "text/txt";
                    extension = "txt";
                }
                Response.Headers.Add("content-disposition", string.Format("attachment;filename={0}.{1}", @params.tableName, extension));
                BuildHeader(sb, @params);
                sb.Append(Environment.NewLine);
                BuildRows(sb, @params);
                await Response.WriteAsync(sb.ToString());
                await Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessageJS();
                DataErrorHandler.ErrorHandler(ex, model, @params.viewId, passport.DatabaseName);
            }

        }
        //EXPORT CSV OR TXT ALL
       [HttpGet]
        public async void ExportCSVorTXTAll()
        {
            ExporterJsonModel @params = ContextService.GetObjectFromJson<ExporterJsonModel>("objHolder", httpContext);

            if (@params.IsBackgroundProcessing)
                return;
            if (passport.CheckPermission(@params.viewName, SecureObject.SecureObjectType.View, Permissions.Permission.Export))
            {
                var sb = new StringBuilder();
                try
                {
                    Response.Clear();
                    Response.Headers.Clear();
                    string extension = string.Empty;
                    if (@params.IsCSV)
                    {
                        Response.ContentType = "text/csv";
                        extension = "csv";
                    }
                    else
                    {
                        Response.ContentType = "text/txt";
                        extension = "txt";
                    }
                    Response.Headers.Add("content-disposition", string.Format("attachment;filename={0}.{1}", @params.tableName, extension));
                    var model = new ExportAll(passport, @params, (string)ContextService.GetSessionValueByKey("HoldTotalRowQuery", httpContext), @params.crumbLevel, httpContext);
                    var strbuilder = model.BuildString();
                    await Response.WriteAsync(model.BuildString().ToString());
                    await Response.Body.FlushAsync();
                }
                catch (Exception)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();
                }
            }
        }

        //EXPORT CSV OR TXT ALL Report
       [HttpGet]
        public async void ExportCSVorTXTAllReport()
        {
            ExporterReportJsonModel @params = ContextService.GetObjectFromJson<ExporterReportJsonModel>("objHolder", httpContext);

            if (@params.IsBackgroundProcessing)
                return;
            if (passport.CheckPermission(@params.viewName, SecureObject.SecureObjectType.View, Permissions.Permission.Export))
            {
                var sb = new StringBuilder();
                try
                {
                    Response.Clear();
                    Response.Headers.Clear();
                    string extension = string.Empty;
                    if (@params.IsCSV)
                    {
                        Response.ContentType = "text/csv";
                        extension = "csv";
                    }
                    else
                    {
                        Response.ContentType = "text/txt";
                        extension = "txt";
                    }
                    Response.Headers.Add("content-disposition", string.Format("attachment;filename={0}.{1}", @params.tableName, extension));
                    var model = new ExportAll(passport, @params, httpContext);
                    await Response.WriteAsync(model.BuildString().ToString());
                    await Response.Body.FlushAsync();
                    HttpContext.Response.StatusCode = StatusCodes.Status200OK;
                }
                catch (Exception)
                {
                    Keys.ErrorType = "e";
                    Keys.ErrorMessage = Keys.ErrorMessageJS();

                }
            }
        }
        private int CountSelectAllData(int viewid)
        {
            int Rowscount = 0;
            string command = string.Format("select count(*) from view__{0}", viewid);
            using (var cmd = new SqlCommand(command, passport.Connection()))
            {
                Rowscount = Conversions.ToInteger(cmd.ExecuteScalar());
            }
            return Rowscount;
        }
        private int CountSelectAllDataReport(ExporterReportJsonModel paramsRep)
        {
            int Rowscount = 0;
            var @params = new TabFusionRMS.WebCS.ReportingJsonModel();
            // 0 to 8 types are Tracking Report & Requestor Reports
            // 9 to 17 are Retention reports
            // 18 for Audit Report
            if (Int32.Parse(paramsRep.ReportType) >= 0 && Int32.Parse(paramsRep.ReportType) <= 8)
            {
                var model = new TabFusionRMS.WebCS.ReportsModels(passport, @params, httpContext);
                model.ExecuteReportingPagination((ReportsType)Convert.ToInt32(paramsRep.ReportType));
                Rowscount = model.Paging.TotalRecord;
            }
            else if ((Int32.Parse(paramsRep.ReportType) >= 9 && Int32.Parse(paramsRep.ReportType) <= 12) || (Int32.Parse(paramsRep.ReportType) >= 14 && Int32.Parse(paramsRep.ReportType) <= 17))
            {
                var model = new TabFusionRMS.WebCS.RetentionReportModel(passport, @params, httpContext);
                model.ExecuteRetentionReportsPagination((ReportsType)@params.reportType);
                Rowscount = model.Paging.TotalRecord;
            }
            else if (Int32.Parse(paramsRep.ReportType) == 18)
            {
                var model = new TabFusionRMS.WebCS.AuditReportSearch(passport, httpContext);
                model.Paging.PageNumber = @params.pageNumber;
                model.RunQuery(paramsRep.ReportAuditFilterProperties, true);
                Rowscount = model.Paging.TotalRecord;
            }
            return Rowscount;
        }

        // call background processing
        [HttpGet]
        public JsonResult BackGroundProcessing([FromServices] IWebHostEnvironment webHostEnvironment)
        {
            var model = new Exporter(passport);
            try
            {
                var datapro = new DataProcessingModel();
                ExporterJsonModel @params =  ContextService.GetObjectFromJson<ExporterJsonModel> ("objHolder", httpContext);
                datapro.ErrorMessage = @params.ErrorMessage;
                datapro.viewId = @params.viewId;

                datapro.Reconciliation = @params.Reconciliation;
                datapro.RecordCount = @params.RecordCount;
                datapro.TaskType = @params.TaskType;
                datapro.IsSelectAllData = @params.IsSelectAllData;
                datapro.DueBackDate = null;
                datapro.DestinationTableName = "";
                datapro.DestinationTableId = "";

                datapro.ListofselectedIds = @params.ListofselectedIds;
                // If datapro.IsSelectAllData Then datapro.ListofselectedIds = GetAllDataIds(params.viewId)

                string extension = "txt";
                if (@params.IsCSV)
                    extension = "csv";

                string errorMessage = string.Empty;
                datapro.FileName = string.Format("{0}_Export_{1}_{2}", @params.tableName, extension.ToUpper(), Guid.NewGuid().ToString());
                @params.FileName = datapro.FileName;
                datapro.Path = string.Format("{0}{1}.{2}", webHostEnvironment.ContentRootPath + "/BackgroundFiles/", datapro.FileName, extension);
                datapro.ExportReportPath = string.Format("{0}{1}_Report.txt", webHostEnvironment.ContentRootPath + "/BackgroundFiles/", datapro.FileName);
                bool output = BackgroundStatus.InsertData(datapro, httpContext.Session.GetString("HoldTotalRowQuery")??"", passport, httpContext);
                model = BackgroungMessage(output, @params);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(model);
        }
        private Enums.BackgroundTaskProcess CheckIfBackgroungProcessing(ExporterJsonModel @params)
        {
            try
            {
                // RepositoryVB.IRepository s_Setting = new Repositories<TabFusionRMS.Models.Setting>();
                var s_Setting = new TabFusionRMS.RepositoryVB.Repositories<TabFusionRMS.Models.Setting>();
                var expMinVal = s_Setting.Where(x => x.Section == "BackgroundExport" && x.Item == "MinValue").FirstOrDefault()?.ItemValue;
                var expMaxVal = s_Setting.Where(x => x.Section == "BackgroundExport" && x.Item == "MaxValue").FirstOrDefault()?.ItemValue;
                if (@params.RecordCount < Convert.ToInt32(expMinVal))
                {
                    return Enums.BackgroundTaskProcess.Normal;
                }
                else if (@params.RecordCount > Convert.ToInt32(expMaxVal))
                {
                    return Enums.BackgroundTaskProcess.ExceedMaxLimit;
                }
                else if ( Convert.ToBoolean(ContextService.GetSessionValueByKey("ServiceManagerEnabled", httpContext)))
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
        private void BuildHeader(StringBuilder sb, ExporterJsonModel @params)
        {
            // build header
            int counter = 0;
            foreach (HeaderProps head in @params.Headers)
            {
                counter = counter + 1;
                if (@params.Headers.Count == counter)
                {
                    sb.Append(head.HeaderName);
                }
                else
                {
                    sb.Append(head.HeaderName + ",");
                }
            }
        }
        private void BuildRows(StringBuilder sb, ExporterJsonModel @params)
        {
            int counter = 0;
            string holdeData = string.Empty;
            for (int index = 0, loopTo = @params.DataRows.Count - 1; index <= loopTo; index++)
            {
                foreach (string loopdata  in @params.DataRows[index])
                {
                    string data = loopdata;
                    if (data == null)
                    {
                        data = "";
                    }
                    counter = counter + 1;
                    if (@params.Headers[counter - 1].DataType == "DateTime")
                    {
                        holdeData = Keys.get_ConvertCultureDate(data, httpContext, true);
                    }
                    else if (@params.Headers[counter - 1].DataType == "Boolean")
                    {
                        if (data == "False")
                        {
                            holdeData = 0.ToString();
                        }
                        else
                        {
                            holdeData = 1.ToString();
                        }
                    }
                    else
                    {

                        // ' if data contains NewLine/Enter in that case need to add into string.
                        if (data.Contains(Constants.vbCrLf))
                        {
                            data = "\"" + data + "\"";
                        }

                        if (data.Contains(","))
                        {
                            holdeData = data.Replace(",", "");
                        }
                        else
                        {
                            holdeData = data;
                        }
                    }
                    if (@params.IsCSV)
                    {
                        if (counter == @params.DataRows[index].Count)
                        {
                            sb.Append(holdeData);
                        }
                        else
                        {
                            sb.Append(holdeData + ",");
                        }
                    }
                    else if (counter == @params.DataRows[index].Count)
                    {
                        sb.Append("\"" + holdeData + "\"");
                    }
                    else
                    {
                        sb.Append("\"" + holdeData + "\"" + ",");
                    }

                }
                sb.Append(Environment.NewLine);
                counter = 0;
            }
        }
        private Exporter BackgroungMessage(bool output, ExporterJsonModel @params)
        {
            var msg = new StringBuilder();
            var model = new Exporter(passport);
            if (output)
            {
                // If params.FileName.Contains("Transfer") Then
                // msg.Append(String.Format("<p>" + Languages.get_Translation("lblMSGTransferBackgroundSuccess") + "</p>", "<b>" + Languages.get_Translation("lblMSGExportBackgroundStatus") + "</b>"))
                // Else
                // msg.Append(String.Format("<p>" + Languages.get_Translation("lblMSGExportBackgroundSuccess") + "</p>", Languages.get_Translation("lblMSGExportBackgroundStatus")))
                // End If
                msg.Append(string.Format("<p>" + Languages.get_Translation("lblMSGExportBackgroundSuccess") + "</p>", Languages.get_Translation("lblMSGExportBackgroundStatus")));
                msg.Append(string.Format("<p>" + Languages.get_Translation("lblMSGExportBackgroundFileName") + "</p>", "<b>" + @params.FileName + "</b>"));
                msg.Append(string.Format("<p>" + Languages.get_Translation("lblMSGExportBackgroundSelectedRows") + "</p>", "<b>" + @params.RecordCount.ToString() + "</b>"));
                msg.Append(string.Format("<p>" + Languages.get_Translation("lblMSGExportBackgroundCurrentStatus") + "</p>", "<b>" + Enum.GetName(typeof(Enums.BackgroundTaskStatus), 1) + "</b>"));
                model.HtmlMessage = msg.ToString();
                model.lblTitle = Languages.get_Translation("lblMSGSuccessMessage");
            }
            else
            {
                model.lblTitle = Languages.get_Translation("lblMSGErrorMessage");
                msg.Append(string.Format("<p>{0}</p>", @params.ErrorMessage));
                model.HtmlMessage = msg.ToString();
            }
            return model;
        }

    }
}