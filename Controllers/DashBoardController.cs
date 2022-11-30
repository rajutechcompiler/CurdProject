using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
using Smead.RecordsManagement;
using Smead.Security;
using TabFusionRMS.Models;
using TabFusionRMS.Models.Mapping;
using TabFusionRMS.RepositoryVB;


namespace TabFusionRMS.WebCS.Controllers
{
    public partial class DashBoardController : BaseController
    {
        // GET: DashBoard
        public IActionResult Index()
        {
            var model = new DashboardModel(passport, httpContext);
            try
            {
                // Session.Remove("workgrouplist")
                model.ExecuteDashboard();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = Keys.ErrorMessageJS();
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return View("~/Views/DashBoard/index.cshtml", model);
        }

        // 'Start Chart Model Partial Views
        public IActionResult AddEditDashboardPartial()
        {
            return PartialView("_AddEditDashboard");
        }
        public IActionResult AddEditTaskListPartial()
        {
            return PartialView("_AddEditTaskList");
        }
        public IActionResult AddEditChartPartial()
        {
            var dropM = new DashboardDropdown();
            dropM.WorkGroup = Smead.RecordsManagement.Navigation.GetWorkGroups(passport).OrderBy(x => x.WorkGroupName).ToList();
            httpContext.Session.SetString("workgrouplist", JsonConvert.SerializeObject(dropM.WorkGroup));
            return PartialView("_AddEditChart", dropM);
        }
        public IActionResult AddEditTrackedPartial()
        {
            var dm = new DashboardDataModel(passport, httpContext);
            try
            {
                var response = dm.TrackableTable();
                ViewBag.TrackableTable = response;
                return PartialView("_AddEditTracked");
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, dm, 0, passport.DatabaseName);
                return PartialView("_AddEditTracked");
            }
        }
        public IActionResult AddEditOperationPartial()
        {
            var dm = new DashboardDataModel(passport, httpContext);
            try
            {
                ViewBag.UsersList = dm.Users().OrderBy(x => x.Name).ToList();
                ViewBag.AuditTable = dm.AuditTable().OrderBy(x => x.UserName).ToList();
                var auditTypeList = new Auditing().GetAuditTypeList();
                auditTypeList.Sort((x, y) => x.Name.ToLower().CompareTo(y.Name.ToLower()));
                ViewBag.AuditType = auditTypeList;

                return PartialView("_AddEditOperation");
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, dm, 0, passport.DatabaseName);
                return PartialView("_AddEditOperation");
            }
        }
        public IActionResult AddEditSeriesPartial()
        {
            var dropM = new DashboardDropdown();
            try
            {
                dropM.WorkGroup = Navigation.GetWorkGroups(passport).OrderBy(x => x.WorkGroupName).ToList();
                httpContext.Session.SetString("workgrouplist", JsonConvert.SerializeObject(dropM.WorkGroup));
                return PartialView("_AddEditSeries", dropM);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, dropM, 0, passport.DatabaseName);
                return PartialView("_AddEditSeries", dropM);
            }
        }

        // 'Chart Dropdown data
        public IActionResult GetWorkGroupTableMenu(Int16 workGroupId)
        {
            var model = new DashboardCommonModel();
            try
            {
                model.WorkGroupMenu = Navigation.GetWorkGroupMenu(workGroupId, passport).OrderBy(x => x.UserName).ToList();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(model);
        }
        public IActionResult GetViewMenu(string tableName)
        {
            var model = new DashboardCommonModel();
            try
            {
                model.ViewsByTableName = Navigation.GetViewsByTableName(tableName, passport).OrderBy(x => x.ViewName).ToList();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(model);
        }
        public JsonResult GetViewColumnMenu(int viewId)
        {
            var dm = new DashboardDataModel(passport, httpContext);
            try
            {
                dm.ViewColumnEntity = dm.ViewColumns(viewId).OrderBy(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, dm, viewId, passport.DatabaseName);
            }
            return Json(dm);
        }
        public JsonResult GetViewColumnOnlyDate(int viewId, string tableName)
        {
            var dm = new DashboardDataModel(passport, httpContext);
            try
            {
                dm.ViewColumnEntity = dm.GetViewColumnOnlyWithType(tableName, viewId).OrderBy(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, dm, viewId, passport.DatabaseName);
            }
            return Json(dm);
        }

        // 'Dashboard Rename, Update, delete , add in favourite dashboard, Get Dashboard Detail,  Set Dashboard details functionality

        //Todo
        //[ValidateInput(false)]//Todo
        public IActionResult SetDashboardDetails(string Name)
        {
            Name = System.Net.WebUtility.HtmlEncode(Name);
            var dm = new DashboardDataModel(passport, httpContext);
            var model = new DashboardCommonModel();
            try
            {
                if (dm.CheckDashboardNameDuplicate(0, Name) == true)
                {
                    model.ErrorMessage = "Already exists name.";
                }
                else
                {
                    var resId = dm.InsertDashbaord(Name, "");
                    if (resId > 0)
                    {
                        model.ErrorMessage = "Added successfully";
                        model.ud = dm.GetDashbaordId(resId);
                        model.DashboardListHtml = dm.GetDashboardListString();
                    }
                    else
                    {
                        model.ErrorMessage = "Fail to add new dashboard";
                    }
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = Keys.ErrorMessageJS();
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(model);
        }
        public IActionResult GetDashboardDetails(int DashboardId)
        {
            var dm = new DashboardDataModel(passport, httpContext);
            var model = new DashboardCommonModel();
            try
            {
                model.ud = dm.GetDashbaordId(DashboardId);
            }

            catch (Exception ex)
            {
                model.ErrorMessage = Keys.ErrorMessageJS();
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);

        }
        public IActionResult GetDashboardList()
        {
            var model = new DashboardModel(passport, httpContext);
            var returnModel = new DashboardListModel();
            try
            {
                model.ExecuteDashboard();
                returnModel.DashboardListHtml = model.DashboardListHtml;
            }
            catch (Exception ex)
            {
                model.ErrorMessage = Keys.ErrorMessageJS();
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(returnModel);
        }

        //[ValidateInput(false)]//Todo
        public IActionResult SetDashboardJson(int DashboardId, string JsonString)
        {
            DashboardDataModel dm = default;
            var model = new DashboardCommonModel();

            try
            {
                dm = new DashboardDataModel(passport, httpContext);
                var resId = dm.UpdateDashbaordJson(JsonString, DashboardId);
                if (resId > 0)
                {
                    model.ErrorMessage = "Update successfully";
                    model.ud = dm.GetDashbaordId(resId);
                }
                else
                {
                    model.ErrorMessage = "Fail to update";
                }
            }

            catch (Exception ex)
            {
                model.ErrorMessage = Keys.ErrorMessageJS();
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }

        //[ValidateInput(false)]//Todo//Todo
        public IActionResult RenameDashboardName(int DashboardId, string Name)
        {
            var model = new DashboardCommonModel();
            try
            {
                var dm = new DashboardDataModel(passport, httpContext);
                Name = System.Net.WebUtility.HtmlEncode(Name);
                if (dm.CheckDashboardNameDuplicate(DashboardId, Name) == true)
                {
                    model.ErrorMessage = "Already exists name.";
                }
                else
                {
                    var resId = dm.UpdateDashbaordName(Name, DashboardId);
                    if (resId > 0)
                    {
                        model.ErrorMessage = "Update successfully";
                        model.ud = dm.GetDashbaordId(resId);
                        model.DashboardListHtml = dm.GetDashboardListString();
                    }
                    else
                    {
                        model.ErrorMessage = "Fail to add new dashboard";
                    }
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = Keys.ErrorMessageJS();
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }
        public IActionResult DeleteDashboard(int DashboardId)
        {
            var model = new DashboardCommonModel();
            DashboardDataModel dm = default;
            try
            {
                dm = new DashboardDataModel(passport, httpContext);
                var resId = dm.DeleteDashbaord(DashboardId);
                if (resId > 0)
                {
                    model.ErrorMessage = "Delete Successfully";
                }
                else
                {
                    model.ErrorMessage = "Fail to delete dashboard";
                }
            }

            catch (Exception ex)
            {
                model.ErrorMessage = Keys.ErrorMessageJS();
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);

        }
        public IActionResult UpdateFavouriteDashboard(int DashboardId, bool IsFav)
        {
            var model = new DashboardCommonModel();
            var model1 = new DashboardModel(passport,httpContext);
            try
            {
                IRepository<SLUserDashboard> _iUserDashboard = new Repositories<SLUserDashboard>();
                model.ud = _iUserDashboard.All().Where(x => x.ID.Equals(DashboardId)).FirstOrDefault();
                model.ud.IsFav = IsFav;
                _iUserDashboard.Update(model.ud);

                model1.ExecuteDashboardList();
                model.DashboardListHtml = model1.DashboardListHtml;

                model.ErrorMessage = "Updated Successfully";
            }
            catch (Exception ex)
            {
                model.ErrorMessage = Keys.ErrorMessageJS();
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);

        }

        // 'Check Permission of dashboard widget list
        //[ValidateInput(false)]//Todo
        public JsonResult ValidatePermission(string widgetList)
        {
            var dm = new DashboardDataModel(passport, httpContext);
            var model = new ChartDataResModel();
            model.JsonString = widgetList;
            var updatedWidgetList = new List<object>();
            try
            {
                var parseJsonList = JsonConvert.DeserializeObject<List<object>>(widgetList);
                IRepository<View> _iView = new Repositories<View>();

                foreach (JObject parseJson in parseJsonList)
                {
                    var asdf = parseJson[""];
                    if (parseJson["WidgetType"].ToString() == "CHART_1" || parseJson["WidgetType"].ToString() == "CHART_2")
                    {
                        string TableIds = this.ConvertArrayToString((JArray)(parseJson["Objects"]));
                        var TableNames = dm.GetTableNames(TableIds);
                        bool Permission = true;
                        foreach (TableModel item in TableNames)
                        {
                            if (passport.CheckPermission(item.TableName, Smead.Security.SecureObject.SecureObjectType.Table, Permissions.Permission.View) == false)
                            {
                                Permission = false;
                                break;
                            }
                        }
                        parseJson["permission"] = Permission;
                    }
                    else
                    {
                        // check table permission 
                        string TableName = parseJson["TableName"].ToString();
                        int ParentView = Convert.ToInt32(parseJson["ParentView"].ToString());
                        int WorkGroupId = Convert.ToInt32(parseJson["WorkGroup"].ToString());
                        bool permission = true;
                        var ViewName = _iView.All().Where(x => x.Id == ParentView).Select(x => x.ViewName).FirstOrDefault();
                        // 'It is already getting all permission workgroup list so do not need to check permission
                        if (Smead.RecordsManagement.Navigation.GetWorkGroups(passport).Select(x => x.ID== WorkGroupId).ToList().Count() == 0)
                            permission = false;

                        if (permission == true)
                        {
                            if (string.IsNullOrEmpty(TableName) == false)
                            {
                                permission = passport.CheckPermission(parseJson["TableName"].ToString(), Smead.Security.SecureObject.SecureObjectType.Table, Permissions.Permission.View);
                                if (permission == true)
                                {
                                    if (string.IsNullOrEmpty(ViewName) == false)
                                    {
                                        permission = passport.CheckPermission(ViewName, Smead.Security.SecureObject.SecureObjectType.View, Permissions.Permission.View);
                                    }
                                }
                            }
                            else if (string.IsNullOrEmpty(ViewName) == false)
                            {
                                permission = passport.CheckPermission(ViewName, Smead.Security.SecureObject.SecureObjectType.View, Permissions.Permission.View);
                            }
                        }
                        parseJson["permission"] = permission;
                    }
                    updatedWidgetList.Add(parseJson);
                }
                model.JsonString = JsonConvert.SerializeObject(updatedWidgetList);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(model);
        }

        // 'Get Chart Data
        //[ValidateInput(false)]//Todo
        public JsonResult GetChartData(string widgetObjectJson)
        {
            DashboardDataModel dm = default;
            var model = new ChartDataResModel();
            model.JsonString = widgetObjectJson;
            model.Permission = true;
            var JsonResult = Json(model);
            //JsonResult.MaxJsonLength = int.MaxValue;
            try
            {
                IRepository<View> _iView = new Repositories<View>();
                dm = new DashboardDataModel(passport, httpContext);

                var parseJson = JsonConvert.DeserializeObject<JObject>(widgetObjectJson);

                var ParentView = Convert.ToInt16(parseJson["ParentView"].ToString());
                int workgroupId = Convert.ToInt32(parseJson["WorkGroup"].ToString());

                var workgroupname = GetWorkGroupListSession().Where(x => x.ID == workgroupId).Select(n => n.WorkGroupName).FirstOrDefault();

                if (workgroupname == default)
                {
                    model.Permission = false;
                    return JsonResult;
                }

                // check workgroup permission
                if (!passport.CheckPermission(workgroupname, Smead.Security.SecureObject.SecureObjectType.WorkGroup, Permissions.Permission.Access))
                {
                    model.Permission = false;
                    return JsonResult;
                }
                // check table permission 
                if (!passport.CheckPermission(parseJson["TableName"].ToString(), Smead.Security.SecureObject.SecureObjectType.Table, Permissions.Permission.View))
                {
                    model.Permission = false;
                    return JsonResult;
                }
                // check View permission 
                var ViewName = _iView.All().Where(x => x.Id== ParentView).Select(x => x.ViewName).FirstOrDefault();
                if (string.IsNullOrEmpty(ViewName) == false)
                {
                    var ress = passport.CheckPermission(ViewName, Smead.Security.SecureObject.SecureObjectType.View, Permissions.Permission.View);
                    if (ress == false)
                    {
                        model.Permission = false;
                        return JsonResult;
                    }
                }
                else
                {
                    return JsonResult;
                }

                string TableName = parseJson["TableName"].ToString();
                var ColumnName = parseJson["Column"].ToString();
                IRepository<ViewColumn> _iViewCol = new Repositories<ViewColumn>();

                // 'check column exist or not into view
                var col = _iViewCol.Where(x => x.FieldName == ColumnName && x.ViewsId == ParentView).ToList();
                if (col.Count == 0)
                {
                    model.Permission = false;
                    return JsonResult;
                }

                var list = dm.GetBarPieChartData(TableName, Convert.ToInt32(ParentView), ColumnName);
                // Dim list = dm.GetBarPieChartData(TableName, ColumnName)
                model.Data = list;
                return JsonResult;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return JsonResult;
            }

        }
        //[ValidateInput(false)]//Todo
        public JsonResult GetTrackedData(string widgetObjectJson)
        {
            var dm = new DashboardDataModel(passport, httpContext);
            var model = new ChartDataResModel();
            model.JsonString = widgetObjectJson;
            try
            {
                var parseJson = JsonConvert.DeserializeObject<JObject>(widgetObjectJson);
                string TableIds = this.ConvertArrayToString((JArray)parseJson["Objects"]);

                var TableNames = dm.GetTableNames(TableIds);
                // Dim Permission As Boolean = True
                foreach (TableModel item in TableNames)
                {
                    if (passport.CheckPermission(item.TableName, Smead.Security.SecureObject.SecureObjectType.Table, Permissions.Permission.View) == false)
                    {
                        model.Permission = false;
                        break;
                    }
                }

                if (model.Permission == false)
                {
                    return Json(model);
                }

                var Filter = parseJson["Filter"].ToString();
                var pe = parseJson["Period"];
                var list = dm.GetTrackedChartData(TableIds, Filter, pe);
                model.Data = list;
                return Json(model);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return Json(model);
            }

        }
        //[ValidateInput(false)]//Todo
        public JsonResult GetOperationsData(string widgetObjectJson)
        {
            var model = new OperationChartDataResModel();
            model.JsonString = widgetObjectJson;
            try
            {
                var dm = new DashboardDataModel(passport, httpContext);
                var parseJson = JsonConvert.DeserializeObject<JObject>(widgetObjectJson);
                string TableIds = this.ConvertArrayToString((JArray)parseJson["Objects"]);
                var TableNames = dm.GetTableNames(TableIds);
                model.Permission = true;
                foreach (TableModel item in TableNames)
                {
                    if (passport.CheckPermission(item.TableName, Smead.Security.SecureObject.SecureObjectType.Table, Permissions.Permission.View) == false)
                    {
                        model.Permission = false;
                        break;
                    }
                }

                if (model.Permission == false)
                {
                    return Json(model);
                }

                string UserIds = this.ConvertArrayToString((JArray)parseJson["Users"]);
                string AuditTypeId = this.ConvertArrayToString((JArray)parseJson["AuditType"]);
                var Period = parseJson["Period"].ToString();
                var Filter = parseJson["Filter"].ToString();
                var list = dm.GetOperationChartData(TableIds, UserIds, AuditTypeId, Period, Filter);

                model.Data = list;
                return Json(model);
            }
            catch (Exception ex)
            {

                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return Json(model);
            }

        }
        //[ValidateInput(false)]//Todo
        public JsonResult GetTimeSeriesChartData(string widgetObjectJson)
        {
            DashboardDataModel dm = default;
            var model = new ChartDataResModel();
            model.JsonString = widgetObjectJson;
            try
            {
                IRepository<View> _iView = new Repositories<View>();

                var parseJson = JsonConvert.DeserializeObject<JObject>(widgetObjectJson);
                dm = new DashboardDataModel(passport, httpContext);
                var ParentView = Convert.ToInt16(parseJson["ParentView"].ToString());
                var ViewName = _iView.All().Where(x => x.Id == ParentView).Select(x => x.ViewName).FirstOrDefault();

                int workgroupId = Convert.ToInt32(parseJson["WorkGroup"].ToString());
                var ColumnName = parseJson["Column"].ToString();
                var workgroupname = GetWorkGroupListSession().Where(x => x.ID == workgroupId).Select(n => n.WorkGroupName).FirstOrDefault();

                if (workgroupname == default)
                {
                    model.Permission = false;
                    return Json(model);
                }

                // check workgroup permission
                if (!passport.CheckPermission(workgroupname, Smead.Security.SecureObject.SecureObjectType.WorkGroup, Permissions.Permission.Access))
                {
                    model.Permission = false;
                    return Json(model);
                }
                // check table permission 
                if (!passport.CheckPermission(parseJson["TableName"].ToString(), Smead.Security.SecureObject.SecureObjectType.Table, Permissions.Permission.View))
                {
                    model.Permission = false;
                    return Json(model);
                }

                // check view permission
                if (ViewName.Length > 0)
                {
                    model.Permission = passport.CheckPermission(ViewName, Smead.Security.SecureObject.SecureObjectType.View, Permissions.Permission.View);
                    if (model.Permission == false)
                    {
                        return Json(model);
                    }
                }
                else
                {
                    return Json(model);
                }
                IRepository<ViewColumn> _iViewCol = new Repositories<ViewColumn>();

                // 'check column exist or not into view
                var col = _iViewCol.Where(x => x.FieldName==ColumnName && x.ViewsId ==ParentView).ToList();
                if (col.Count == 0)
                {
                    model.Permission = false;
                    return Json(model);
                }

                var list = dm.GetTimeSeriesChartData(parseJson["TableName"].ToString(), ColumnName, Convert.ToInt32(ParentView), parseJson["Period"].ToString(), parseJson["Filter"].ToString());
                model.Data = list;
                return Json(model);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return Json(model);
            }

        }
        //[ValidateInput(false)]//Todo
        public JsonResult GetTaskList(string widgetObjectJson)
        {
            var dm = new DashboardModel(passport,httpContext);
            var model = new ChartDataResModel();
            model.JsonString = widgetObjectJson;
            try
            {
                dm.Taskbar.ExecuteDashboardTasksbar();
                model.TaskList = dm.Taskbar.TaskList;
                return Json(model);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                model.TaskList = dm.Taskbar.TaskList;
                return Json(model);
            }
        }
        private string ConvertArrayToString(JArray arr)
        {
            var model = new DashboardCommonModel();
            try
            {
                var appendSt = new StringBuilder();
                for (int a = 0; a <= arr.Count() - 1; a++)
                {
                    if ((arr.Count() - 1).Equals(a))
                    {
                        appendSt.Append("'").Append(arr[a]).Append("'");
                    }
                    else
                    {
                        appendSt.Append("'").Append(arr[a]).Append("'").Append(",");
                    }
                }
                string finalSt = Convert.ToString(appendSt);
                return finalSt;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return "";
            }

        }

        // 'Count Chart Data
        //[ValidateInput(false)]//Todo
        public JsonResult CountBarChartData(string widgetObjectJson)
        {
            DashboardDataModel dm = default;
            var model = new ChartDataResModel();
            model.JsonString = widgetObjectJson;
            var JsonResult = Json(model);
            //JsonResult.MaxJsonLength = int.MaxValue;
            try
            {
                dm = new DashboardDataModel(passport, httpContext);
                var parseJson = JsonConvert.DeserializeObject<JObject>(widgetObjectJson);
                var TableName = parseJson["TableName"].ToString();
                var ColumnName = parseJson["Column"].ToString();
                var list = dm.GetBarPieChartDataCount(TableName, ColumnName);
                model.Count = list;
                return JsonResult;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return JsonResult;
            }

        }
        //[ValidateInput(false)]//Todo
        public JsonResult CountTimeSeriesChartData(string widgetObjectJson)
        {
            DashboardDataModel dm = default;
            var model = new ChartDataResModel();
            model.JsonString = widgetObjectJson;
            try
            {
                IRepository<View> _iView = new Repositories<View>();

                var parseJson = JsonConvert.DeserializeObject<JObject>(widgetObjectJson);
                dm = new DashboardDataModel(passport, httpContext);
                var ParentView = Convert.ToInt16(parseJson["ParentView"].ToString());
                var ViewName = _iView.All().Where(x => x.Id== ParentView).Select(x => x.ViewName).FirstOrDefault();
                if (ViewName.Length > 0)
                {
                    var ress = passport.CheckPermission(ViewName, Smead.Security.SecureObject.SecureObjectType.View, Permissions.Permission.View);
                    if (ress == false)
                    {
                        return Json(model);
                    }
                }
                else
                {
                    return Json(model);
                }

                var TableName = parseJson["TableName"].ToString();
                var ColumnName = parseJson["Column"].ToString();
                var Period = parseJson["Period"].ToString();
                var Filter = parseJson["Filter"].ToString();

                var Count = dm.GetTimeSeriesChartDataCount(TableName, ColumnName, Period, Filter);
                model.Count = Count;
                return Json(model);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return Json(model);
            }

        }
        //[ValidateInput(false)]//Todo
        public JsonResult CountOperationsData(string widgetObjectJson)
        {
            var model = new OperationChartDataResModel();
            model.JsonString = widgetObjectJson;
            try
            {
                var dm = new DashboardDataModel(passport, httpContext);
                var parseJson = JsonConvert.DeserializeObject<JObject>(widgetObjectJson);
                string TableIds = this.ConvertArrayToString((JArray)parseJson["Objects"]);
                var TableNames = dm.GetTableNames(TableIds);
                bool Permission = true;
                foreach (TableModel item in TableNames)
                {
                    if (passport.CheckPermission(item.TableName, Smead.Security.SecureObject.SecureObjectType.Table, Permissions.Permission.View) == false)
                    {
                        Permission = false;
                        break;
                    }
                }

                if (Permission == false)
                {
                    return Json(model);
                }

                string UserIds = this.ConvertArrayToString((JArray)parseJson["Users"]);
                string AuditTypeId = this.ConvertArrayToString((JArray)parseJson["AuditType"]);
                var Period = parseJson["Period"].ToString();
                var Filter = parseJson["Filter"].ToString();
                var list = dm.GetOperationChartDataCount(TableIds, UserIds, AuditTypeId, Period, Filter);

                model.Count = list;
                return Json(model);
            }
            catch (Exception ex)
            {

                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return Json(model);
            }

        }
        //[ValidateInput(false)]//Todo
        public JsonResult CountTrackedData(string widgetObjectJson)
        {
            var dm = new DashboardDataModel(passport, httpContext);
            var model = new ChartDataResModel();
            model.JsonString = widgetObjectJson;
            try
            {
                var parseJson = JsonConvert.DeserializeObject<JObject>(widgetObjectJson);
                string TableIds = this.ConvertArrayToString((JArray)parseJson["Objects"]);

                var TableNames = dm.GetTableNames(TableIds);
                bool Permission = true;
                foreach (TableModel item in TableNames)
                {
                    if (passport.CheckPermission(item.TableName, Smead.Security.SecureObject.SecureObjectType.Table, Permissions.Permission.View) == false)
                    {
                        Permission = false;
                        break;
                    }
                }

                if (Permission == false)
                {
                    return Json(model);
                }

                var Filter = parseJson["Filter"].ToString();
                var pe = parseJson["Period"].ToString();
                var Count = dm.GetTrackedChartDataCount(TableIds, Filter, pe);
                model.Count = Count;
                return Json(model);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return Json(model);
            }

        }

        // 'Check Data Grid Permissin 
        //[ValidateInput(false)]//Todo
        public JsonResult CheckDataGridPermission(string widgetObjectJson)
        {
            DashboardDataModel dm = default;
            var model = new ChartDataResModel();
            model.JsonString = widgetObjectJson;
            model.Permission = true;
            var JsonResult = Json(model);
            //JsonResult.MaxJsonLength = int.MaxValue;
            try
            {
                dm = new DashboardDataModel(passport, httpContext);

                var parseJson = JsonConvert.DeserializeObject<JObject>(widgetObjectJson);
                var ParentView = parseJson["ParentView"].ToString();
                int workgroupId = Convert.ToInt32(parseJson["WorkGroup"].ToString());

                var workgroupname = GetWorkGroupListSession().Where(x => x.ID== workgroupId).Select(n => n.WorkGroupName).FirstOrDefault();

                if (workgroupname == default)
                {
                    model.Permission = false;
                    return JsonResult;
                }

                // check workgroup permission
                if (!passport.CheckPermission(workgroupname, Smead.Security.SecureObject.SecureObjectType.WorkGroup, Permissions.Permission.Access))
                {
                    model.Permission = false;
                    return JsonResult;
                }
                // check table permission 
                if (!passport.CheckPermission(parseJson["TableName"].ToString(), Smead.Security.SecureObject.SecureObjectType.Table, Permissions.Permission.View))
                {
                    model.Permission = false;
                    return JsonResult;
                }

                return JsonResult;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                return JsonResult;
            }

        }

        // 'Get workgroup list from session
        public List<WorkGroupItem> GetWorkGroupListSession()
        {
            var workgrouplist = ContextService.GetObjectFromJson<List<WorkGroupItem>>("workgrouplist", httpContext);
            if (workgrouplist == null)
            {
                var list = Smead.RecordsManagement.Navigation.GetWorkGroups(passport).OrderBy(x => x.WorkGroupName).ToList();
                ContextService.SetSessionValue("workgrouplist", JsonConvert.SerializeObject(list), httpContext);
                httpContext.Session.SetString("workgrouplist", JsonConvert.SerializeObject(list));
                return list;
            }
            else
            {
                return workgrouplist;
            }

            return workgrouplist;
        }
    }
}


public partial class DashboardCommonModel : BaseModel
{
    public DashboardCommonModel()
    {
        WorkGroupMenu = new List<TableItem>();
        ViewsByTableName = new List<ViewItem>();

    }

    public string ErrorMessage { get; set; }
    public string DashboardListHtml { get; set; }
    public SLUserDashboard ud { get; set; }
    public List<ViewItem> ViewsByTableName { get; set; }
    public List<TableItem> WorkGroupMenu { get; set; }



}
public partial class DashboardListModel : BaseModel
{
    public string ErrorMessage { get; set; }
    public string DashboardListHtml { get; set; }

}
public partial class ChartDataResModel : BaseModel
{
    public ChartDataResModel()
    {
        Data = new List<ChartModel>();
        JsonString = "";
    }
    public List<ChartModel> Data { get; set; }
    public string JsonString { get; set; }
    public string TaskList { get; set; }
    public bool Permission { get; set; } = true;
    public int Count { get; set; }
}

public partial class OperationChartDataResModel : BaseModel
{
    public OperationChartDataResModel()
    {
        Data = new List<ChartOperatinModelRes>();
        JsonString = "";
    }
    public List<ChartOperatinModelRes> Data { get; set; }
    public string JsonString { get; set; }
    public string TaskList { get; set; }
    public bool Permission { get; set; } = true;
    public int Count { get; set; }

}