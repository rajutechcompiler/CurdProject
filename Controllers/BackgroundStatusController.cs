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
using TabFusionRMS.WebCS;

namespace TABFusionRMS.Web.Controllers
{
    public partial class BackgroundStatusController : BaseController
    {
        private readonly IHostEnvironment _webHostEnvironment;
        private IRepository<Table> _iTable { get; set; }
        private IRepository<Databas> _iDatabas { get; set; }
        private IRepository<SLServiceTask> _iSLServiceTask { get; set; }
        private IDBManager _IDBManager = new DBManager();
        public BackgroundStatusController(IRepository<SLServiceTask> iSLServiceTask, IRepository<Table> iTable, IRepository<Databas> iDatabas,
            IHostEnvironment webHostEnvironment)
        {
            _iSLServiceTask = iSLServiceTask;
            _iTable = iTable;
            _iDatabas = iDatabas;
            _webHostEnvironment = webHostEnvironment;
        }

        //public BackgroundStatusController(IRepository<SLServiceTask> iSLServiceTask, IRepository<Table> iTable, IRepository<Databas> iDatabas,
        //    IHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        //{
        //    _iSLServiceTask = iSLServiceTask;
        //    _iTable = iTable;
        //    _iDatabas = iDatabas;
        //    _webHostEnvironment = webHostEnvironment;
        //}

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetBackgroundStatusList1(string sidx, string sord, int page, int rows)
        {

            DataTable dtRecords1;
            int totalRecords = 0;
            _IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
            _IDBManager.CreateParameters(6);
            _IDBManager.AddParameters(0, "@TableName", "SLServiceTasks");
            _IDBManager.AddParameters(1, "@PageNo", page);
            _IDBManager.AddParameters(2, "@PageSize", rows);
            _IDBManager.AddParameters(3, "@DataAndColumnInfo", true);
            _IDBManager.AddParameters(4, "@ColName", sidx);
            _IDBManager.AddParameters(5, "@Sort", sord);
            var loutput = _IDBManager.ExecuteDataSetWithSchema(System.Data.CommandType.StoredProcedure, "SP_RMS_GetTableData");
            _IDBManager.Dispose();
            dtRecords1 = loutput.Tables[0];
            var list = loutput.Tables[0].Rows.Cast<DataRow>().ToList();
            var lBackgroundServiceTask = new List<BackgroundServiceTask>();
            if (dtRecords1 is null)
            {
                return Json(Languages.get_Translation("msgAdminCtrlNullValue"));
            }
            else
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                DataTable dtRecords = (DataTable)dtRecords1;
                if (dtRecords.Columns.Contains("TotalCount"))
                {
                    if (dtRecords.Rows.Count != 0)
                    {
                        totalRecords = Convert.ToInt16(dtRecords.Rows[0]["TotalCount"]);
                    }
                    dtRecords.Columns.Remove("TotalCount");

                }
                if (dtRecords.Columns.Contains("ROWNUM"))
                {
                    dtRecords.Columns.Remove("ROWNUM");
                }
                foreach (var rowData in list)
                {
                    var objJobServiceTask = new BackgroundServiceTask();

                    switch (rowData["Status"].ToString() ?? "")
                    {
                        case var @case when @case == (Enum.GetName(typeof(Enums.BackgroundTaskStatus), 1).ToString() ?? ""): // --Pending
                            {
                                if (String.IsNullOrEmpty(rowData["Status"].ToString()))
                                {
                                    objJobServiceTask.Status = "";
                                }
                                else
                                {
                                    objJobServiceTask.Status = "<span style='color:red'>" + rowData["Status"].ToString() + "</span>";
                                }
                                objJobServiceTask.ReportLocation = "-";
                                objJobServiceTask.DownloadLocation = "-";
                                break;
                            }
                        case var case1 when case1 == (Enum.GetName(typeof(Enums.BackgroundTaskStatus), 2).ToString() ?? ""): // --In-Progress
                            {
                                if (String.IsNullOrEmpty(rowData["Status"].ToString()))
                                {
                                    objJobServiceTask.Status = "";
                                }
                                else
                                {
                                    objJobServiceTask.Status = "<span style='color:darkgoldenrod'>" + rowData["Status"].ToString() + "</span>";
                                }
                                objJobServiceTask.ReportLocation = "-";
                                objJobServiceTask.DownloadLocation = "-";
                                break;
                            }
                        case var case2 when case2 == (Enum.GetName(typeof(Enums.BackgroundTaskStatus), 4).ToString() ?? ""): // --Error
                            {
                                if (String.IsNullOrEmpty(rowData["Status"].ToString()))
                                {
                                    objJobServiceTask.Status = "";
                                }
                                else
                                {
                                    objJobServiceTask.Status = "<span style='color:red'>" + rowData["Status"].ToString() + "</span>";
                                }


                                objJobServiceTask.DownloadLocation = "-";
                                string mainPath = null;
                                if (!string.IsNullOrEmpty(Convert.ToString(rowData["ReportLocation"])))
                                {
                                    if (System.IO.File.Exists(Convert.ToString(rowData["ReportLocation"])))
                                    {
                                        string path = Convert.ToString(rowData["ReportLocation"]);

                                        var new_path= path.Split("/");

                                        mainPath = Convert.ToString(Operators.AddObject(@"\BackgroundFiles\", new_path.Last()));
                                        objJobServiceTask.ReportLocation = "<a href='" + Url.Action("DownloadBackgroundStatus", "BackgroundStatus", new { url = Common.EncryptURLParameters(mainPath) }) + "'><i class='fa fa-eye' aria-hidden='True'></i></a>";
                                    }
                                    else
                                    {
                                        objJobServiceTask.ReportLocation = "-";
                                    }
                                }
                                else
                                {
                                    objJobServiceTask.ReportLocation = "-";
                                }

                                break;
                            }
                        case var case3 when case3 == (Enum.GetName(typeof(Enums.BackgroundTaskStatus), 3).ToString() ?? ""): // --Completed
                            {
                                objJobServiceTask.Status = Conversions.ToString(string.IsNullOrEmpty(Conversions.ToString(rowData["Status"])) ? "" : Operators.AddObject(Operators.AddObject("<span style='color:green'>", rowData["Status"]), "</span>"));
                                string mainPath = null;
                                if (Operators.ConditionalCompareObjectEqual(rowData["TaskType"], Enums.BackgroundTaskType.Export, false))
                                {
                                    if (!string.IsNullOrEmpty(Conversions.ToString(rowData["DownloadLocation"])))
                                    {
                                        if (System.IO.File.Exists(Conversions.ToString(rowData["DownloadLocation"])))
                                        {
                                            var path = rowData["DownloadLocation"].ToString().Split("/");
                                            mainPath = Conversions.ToString(Operators.AddObject(@"\BackgroundFiles\", path.Last()));
                                            objJobServiceTask.DownloadLocation = "<a href='" + Url.Action("DownloadBackgroundStatus", "BackgroundStatus", new { url = Common.EncryptURLParameters(mainPath) }) + "'><i class='fa fa-download' aria-hidden='True'></i></a>";
                                        }
                                        else
                                        {
                                            objJobServiceTask.DownloadLocation = "-";
                                        }
                                    }
                                    else
                                    {
                                        objJobServiceTask.DownloadLocation = "-";
                                    }
                                    objJobServiceTask.ReportLocation = "-";
                                }
                                else
                                {
                                    objJobServiceTask.DownloadLocation = "-";
                                    if (!string.IsNullOrEmpty(Conversions.ToString(rowData["ReportLocation"])))
                                    {
                                        if (System.IO.File.Exists(Conversions.ToString(rowData["ReportLocation"])))
                                        {
                                            var path = rowData["ReportLocation"].ToString().Split(@"/");
                                            mainPath = Conversions.ToString(Operators.AddObject(@"\BackgroundFiles\", path.Last()));
                                            objJobServiceTask.ReportLocation = "<a href='" + Url.Action("DownloadBackgroundStatus", "BackgroundStatus", new { url = Common.EncryptURLParameters(mainPath) }) + "'><i class='fa fa-eye' aria-hidden='True'></i></a>";
                                        }
                                        else
                                        {
                                            objJobServiceTask.ReportLocation = "-";
                                        }
                                    }
                                    else
                                    {
                                        objJobServiceTask.ReportLocation = "-";
                                    }
                                }

                                break;
                            }
                    }
                    objJobServiceTask.Id = Conversions.ToInteger(rowData["Id"]);
                    objJobServiceTask.StartDate = Conversions.ToString(string.IsNullOrEmpty(rowData["StartDate"].ToString()) ? "-" : rowData["StartDate"]);
                    objJobServiceTask.EndDate = Conversions.ToString(string.IsNullOrEmpty(rowData["EndDate"].ToString()) ? "-" : rowData["EndDate"]);
                    objJobServiceTask.Type = Conversions.ToString(rowData["Type"]);
                    objJobServiceTask.RecordCount = Conversions.ToString(rowData["RecordCount"]);
                    lBackgroundServiceTask.Add(objJobServiceTask);
                }
            }

            int totalPages = (int)Math.Round(Math.Truncate(Math.Ceiling(totalRecords / (float)rows)));
            var jsonData = new { total = totalPages, page, records = totalRecords, rows = lBackgroundServiceTask };
            return Json(jsonData);
        }

        public JsonResult GetBackgroundStatusList(string sidx, string sord, int page, int rows)
        {
            try
            {
                int totalRecords = 0;
                BackgroundStatus.ChangeNotification(passport.UserId, _iSLServiceTask);
                List<SLServiceTask> recordsList;
                
                var tranfer = (int)Enums.BackgroundTaskType.Transfer;
                var csv = (int)Enums.BackgroundTaskInDetail.ExportCSV;
                var etxt = (int)Enums.BackgroundTaskInDetail.ExportTXT;

                if (passport.IsAdmin())
                {
                    recordsList = _iSLServiceTask.All().Where(x => x.TaskType == tranfer || x.TaskType== csv || x.TaskType== etxt).ToList();
                }
                else
                {
                    recordsList = _iSLServiceTask.All().Where(x => x.UserId == passport.UserId  && x.TaskType == tranfer || x.TaskType == csv || x.TaskType == etxt).ToList();
                }

                totalRecords = recordsList.Count;

                switch (sidx ?? "")
                {
                    case "CreateDate":
                        {
                            if (sord.ToLower() == "asc")
                            {
                                recordsList = recordsList.OrderBy(y => y.CreateDate).Skip(rows * (page - 1)).Take(rows).ToList();
                            }
                            else
                            {
                                recordsList = recordsList.OrderByDescending(y => y.CreateDate).Skip(rows * (page - 1)).Take(rows).ToList();
                            }

                            break;
                        }
                    case "StartDate":
                        {
                            if (sord.ToLower() == "asc")
                            {
                                recordsList = recordsList.OrderBy(y => y.StartDate).Skip(rows * (page - 1)).Take(rows).ToList();
                            }
                            else
                            {
                                recordsList = recordsList.OrderByDescending(y => y.StartDate).Skip(rows * (page - 1)).Take(rows).ToList();
                            }

                            break;
                        }
                    case "EndDate":
                        {
                            if (sord.ToLower() == "asc")
                            {
                                recordsList = recordsList.OrderBy(y => y.StartDate).Skip(rows * (page - 1)).Take(rows).ToList();
                            }
                            else
                            {
                                recordsList = recordsList.OrderByDescending(y => y.StartDate).Skip(rows * (page - 1)).Take(rows).ToList();
                            }

                            break;
                        }
                    case "Type":
                        {
                            if (sord.ToLower() == "asc")
                            {
                                recordsList = recordsList.OrderBy(y => y.EndDate).Skip(rows * (page - 1)).Take(rows).ToList();
                            }
                            else
                            {
                                recordsList = recordsList.OrderByDescending(y => y.EndDate).Skip(rows * (page - 1)).Take(rows).ToList();
                            }

                            break;
                        }
                    case "Status":
                        {
                            if (sord.ToLower() == "asc")
                            {
                                recordsList = recordsList.OrderBy(y => y.Status).Skip(rows * (page - 1)).Take(rows).ToList();
                            }
                            else
                            {
                                recordsList = recordsList.OrderByDescending(y => y.Status).Skip(rows * (page - 1)).Take(rows).ToList();
                            }

                            break;
                        }
                    case "UserName":
                        {
                            if (sord.ToLower() == "asc")
                            {
                                recordsList = recordsList.OrderBy(y => y.UserName).Skip(rows * (page - 1)).Take(rows).ToList();
                            }
                            else
                            {
                                recordsList = recordsList.OrderByDescending(y => y.UserName).Skip(rows * (page - 1)).Take(rows).ToList();
                            }

                            break;
                        }

                    default:
                        {
                            if (sord.ToLower() == "asc")
                            {
                                recordsList = recordsList.OrderBy(y => y.StartDate).Skip(rows * (page - 1)).Take(rows).ToList();
                            }
                            else
                            {
                                recordsList = recordsList.OrderByDescending(y => y.StartDate).Skip(rows * (page - 1)).Take(rows).ToList();
                            }

                            break;
                        }
                }

                // totalRecords = _iSLServiceTask.All().Where(Function(x) x.TaskType = Enums.BackgroundTaskType.Transfer Or x.TaskType = Enums.BackgroundTaskType.Export).Count()
                if (totalRecords <= 0)
                {
                    return Json(Languages.get_Translation("msgAdminCtrlNullValue"));
                }
                else
                {
                    var lBackgroundServiceTask = new List<BackgroundServiceTask>();
                    foreach (var rowData in recordsList)
                    {
                        var objJobServiceTask = new BackgroundServiceTask();
                        switch (rowData.Status)
                        {
                            case var @case when @case == Enum.GetName(typeof(Enums.BackgroundTaskStatus), 1).ToString(): // --Pending
                                {
                                    objJobServiceTask.Status = rowData.Status is null ? "" : "<span style='color:red'><b>" + rowData.Status + "</b></span>";
                                    objJobServiceTask.ReportLocation = "-";
                                    objJobServiceTask.DownloadLocation = "-";
                                    break;
                                }
                            case var case1 when case1 == Enum.GetName(typeof(Enums.BackgroundTaskStatus), 2).ToString(): // --In-Progress
                                {
                                    objJobServiceTask.Status = rowData.Status is null ? "" : "<span style='color:darkgoldenrod'><b>" + rowData.Status + "</b></span>";
                                    objJobServiceTask.ReportLocation = "-";
                                    objJobServiceTask.DownloadLocation = "-";
                                    break;
                                }
                            case var case2 when case2 == Enum.GetName(typeof(Enums.BackgroundTaskStatus), 4).ToString(): // --Error
                                {
                                    objJobServiceTask.Status = rowData.Status is null ? "" : "<span style='color:red'><b>" + rowData.Status + "</b></span>";
                                    objJobServiceTask.DownloadLocation = "-";
                                    string mainPath = null;
                                    if (rowData.ReportLocation is not null)
                                    {
                                        if (System.IO.File.Exists(rowData.ReportLocation))
                                        {
                                            var path = rowData.ReportLocation.Split(@"\");
                                            mainPath = @"\BackgroundFiles\" + path.Last();
                                            objJobServiceTask.ReportLocation = "<a href='" + Url.Action("DownloadBackgroundStatus", "BackgroundStatus", new { url = Common.EncryptURLParameters(mainPath) }) + "'><i class='fa fa-eye' aria-hidden='True'></i></a>";
                                        }
                                        else
                                        {
                                            objJobServiceTask.ReportLocation = "-";
                                        }
                                    }
                                    else
                                    {
                                        objJobServiceTask.ReportLocation = "-";
                                    }

                                    break;
                                }
                            case var case3 when case3 == Enum.GetName(typeof(Enums.BackgroundTaskStatus), 3).ToString(): // --Completed
                                {
                                    objJobServiceTask.Status = rowData.Status is null ? "" : "<span style='color:green'><b>" + rowData.Status + "</b></span>";
                                    string mainPath = null;
                                    if (rowData.TaskType is (int)Enums.BackgroundTaskInDetail.ExportCSV or (int)Enums.BackgroundTaskInDetail.ExportTXT)
                                    {
                                        if (rowData.DownloadLocation is not null)
                                        {
                                            if (System.IO.File.Exists(rowData.DownloadLocation))
                                            {
                                                var path = rowData.DownloadLocation.Split(@"\");
                                                //mainPath = @"\BackgroundFiles\" + path.Last();
                                                mainPath = path.Last();
                                                objJobServiceTask.DownloadLocation = "<a href='" + Url.Action("DownloadBackgroundStatus", "BackgroundStatus", new { url = Common.EncryptURLParameters(mainPath) }) + "'><i class='fa fa-download' aria-hidden='True'></i></a>";
                                            }
                                            else
                                            {
                                                objJobServiceTask.DownloadLocation = "-";
                                            }
                                        }
                                        else
                                        {
                                            objJobServiceTask.DownloadLocation = "-";
                                        }
                                        objJobServiceTask.ReportLocation = "-";
                                    }
                                    else
                                    {
                                        objJobServiceTask.DownloadLocation = "-";
                                        if (rowData.ReportLocation is not null)
                                        {
                                            if (System.IO.File.Exists(rowData.ReportLocation))
                                            {
                                                var path = rowData.ReportLocation.Split(@"\");
                                                mainPath = @"\BackgroundFiles\" + path.Last();
                                                objJobServiceTask.ReportLocation = "<a href='" + Url.Action("DownloadBackgroundStatus", "BackgroundStatus", new { url = Common.EncryptURLParameters(mainPath) }) + "'><i class='fa fa-eye' aria-hidden='True'></i></a>";
                                            }
                                            else
                                            {
                                                objJobServiceTask.ReportLocation = "-";
                                            }
                                        }
                                        else
                                        {
                                            objJobServiceTask.ReportLocation = "-";
                                        }

                                    }

                                    break;
                                }
                            case var case4 when case4 == Enum.GetName(typeof(Enums.BackgroundTaskStatus), 5).ToString(): // --In Que
                                {
                                    objJobServiceTask.Status = rowData.Status is null ? "" : "<span style='color:navy'><b>" + rowData.Status + "</b></span>";
                                    objJobServiceTask.ReportLocation = "-";
                                    objJobServiceTask.DownloadLocation = "-";
                                    break;
                                }
                        }
                        objJobServiceTask.Id = rowData.Id;
                        objJobServiceTask.CreateDate = rowData.CreateDate is null ? "-" : Keys.get_ConvertCultureDate(rowData.CreateDate.Value,httpContext, bDetectTime: true);
                        objJobServiceTask.StartDate = rowData.StartDate is null ? "-" : Keys.get_ConvertCultureDate(rowData.StartDate.Value, httpContext, bDetectTime: true);
                        objJobServiceTask.EndDate = rowData.EndDate is null ? "-" : Keys.get_ConvertCultureDate(rowData.EndDate.Value, httpContext, bDetectTime: true);
                        objJobServiceTask.Type = rowData.Type;
                        objJobServiceTask.RecordCount = rowData.RecordCount.ToString();
                        objJobServiceTask.UserName = rowData.UserName;
                        lBackgroundServiceTask.Add(objJobServiceTask);
                    }

                    int totalPages = (int)Math.Round(Math.Truncate(Math.Ceiling(totalRecords / (float)rows)));
                    var jsonData = new { total = totalPages, page, records = totalRecords, rows = lBackgroundServiceTask };
                    return Json(jsonData);
                }

            }
            catch (Exception)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = Keys.ErrorMessage;
                return Json(new
                {
                    errortype = Keys.ErrorType,
                    message = Keys.ErrorMessage,
                }, "application/json; charset=utf-8");
            }
        }

        [HttpGet]
        public FileResult DownloadBackgroundStatus([FromServices] IWebHostEnvironment webHostEnvironment, string url)
        {
            string contentType = string.Empty;
            string rootPath = string.Empty;
            rootPath = webHostEnvironment.ContentRootPath + Common.DecryptURLParameters(url);
            new FileExtensionContentTypeProvider().TryGetContentType(rootPath, out contentType);
            var fileName = Path.GetFileName(rootPath);
            var filedata = System.IO.File.ReadAllBytes(rootPath);
            var cd = new System.Net.Mime.ContentDisposition();
            cd.FileName = Path.GetFileName(rootPath);
            cd.Inline = false;
            httpContext.Response.Headers.Add("Content-Disposition", cd.ToString());
            return File(filedata, contentType, fileName);
        }
    }
    public partial class BackgroundServiceTask
    {
        public int Id;
        public string CreateDate;
        public string StartDate;
        public string EndDate;
        public string Type;
        public string Status;
        public string RecordCount;
        public string ReportLocation;
        public string DownloadLocation;
        public string UserName;
    }
}
