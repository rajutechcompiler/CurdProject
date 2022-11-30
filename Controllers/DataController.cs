using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Smead.RecordsManagement;
using Smead.Security;
using TabFusionRMS.Models;
using TabFusionRMS.Models.Mapping;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.Resource;
using TabFusionRMS.WebCS.FusionElevenModels;
using System.Web;
using System.Net;
using Microsoft.AspNetCore.Http.Features;
using ADAL = Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Extensions.Caching.Memory;

namespace TabFusionRMS.WebCS.Controllers
{
    public class DataController : BaseController
    {
        public IActionResult Index([FromServices] IConfiguration configuration)
        {
            if (passport is null)
                return Redirect("/");

            var model = new LayoutModel(passport, httpContext);
            model.NewsFeed.configuration = configuration;
            try
            {
                model.Layout.ExecuteLayout();
                model.Taskbar.ExecuteTasksbar();
                model.NewsFeed.ExecuteNewsFeed();
                model.Footer.ExecuteFooter();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return View(model);
        }

        //private LevelManager _levelManager
        //{
        //    get
        //    {
        //        return ContextService.GetObjectFromJson<LevelManager>("LevelManager", httpContext);
        //    }
        //}

        // APP START | API CALL
        public ActionResult Authorize([FromServices] IMemoryCache cache)
        {
            var authCode = httpContext.Request.Form["code"];
            if (authCode.Count == 0)
            {
                return Redirect("~/Logout");
            }

            var idId = cache.Get("dbID").Text();
            ADAL.AuthenticationContext authContext = new ADAL.AuthenticationContext(AzureADSettings.Authority);
            string redirectUri = Url.Action("Authorize", "Data", null , httpContext?.Request.Scheme);

            ContextService.CreateCookies("", idId.ToString(), httpContext, expire: 180);

            AzureADService azureADService = new AzureADService();
            var adSettings = azureADService.GetAzureADSettings(httpContext);
            if (adSettings == null)
                return Redirect("~/Logout");

            ContextService.SetSessionValue("isADEnabled", string.IsNullOrEmpty(adSettings.ADEnabled) ? false.ToString() : adSettings.ADEnabled.ToString(), httpContext);
            ContextService.SetSessionValue("azureADModelSesssion", JsonConvert.SerializeObject(adSettings), httpContext);

            ADAL.ClientCredential credential = new ADAL.ClientCredential(adSettings.ADClientId, adSettings.ADClientSecret);
            try
            {
                var authResult = authContext.AcquireTokenByAuthorizationCodeAsync(authCode, new Uri(redirectUri), credential, AzureADSettings.GraphResourceURL).Result;
                ContextService.SetSessionValue("authToken", authResult.AccessToken, httpContext);
                ContextService.SetSessionValue("UserName", authResult.UserInfo.DisplayableId, httpContext);
                return Redirect("~/Login/Index?ad=1");
            }
            catch (Exception ex)
            {
                return Redirect("~/Login/Index");
            }
        }

        //// NEWS FEED | API CALL
        [HttpPost]
        public IActionResult SaveNewsURL(string NewUrl)
        {
            bool isSuccess = true;
            try
            {
               Navigation.SetSetting("News", "NewsURL", NewUrl, passport);
            }
            catch (Exception)
            {
                isSuccess = false;
            }
            return Json(isSuccess );
        }
        //// LOAD QUERY WINDOW | API CALL
        [HttpGet]
        public ActionResult LoadQueryWindow(int viewId, int ceriteriaId, int crumblevel, string childKeyField)
        {
            var model = new ViewQueryWindow(passport, crumblevel, childKeyField, httpContext);
            try
            {
                model.DrawQuery(viewId);
                if (ceriteriaId > 0)
                {
                    model.GetMyqueryList(ceriteriaId);
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, viewId, passport.DatabaseName);
            }
            return PartialView("_Query", model);
        }

        [HttpGet]
        public JsonResult LoadquerytoGrid(int viewId, int ceriteriaId, int crumblevel, string childKeyField)
        {
            var model = new ViewQueryWindow(passport, crumblevel, childKeyField, httpContext);
            try
            {
                model.DrawQuery(viewId);
                model.GetMyqueryList(ceriteriaId);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, viewId, passport.DatabaseName);
            }
            return Json(model.MyqueryList);
        }

        [HttpPost]
        public IActionResult RunQuery([FromBody] SearchQueryRequestModal req)
        {
            // create grid object
            var @params = req.paramss;
            GridDataBinding model = new GridDataBinding(passport, @params.ViewId, @params.pageNum, @params.crumbLevel, @params.ViewType, req.paramss.ChildKeyField, httpContext);
            try
            {
                model.ItemDescription = Navigation.GetItemName(@params.preTableName, @params.Childid, passport);
                // get list of query fields
                if (req.searchQuery != null)
                    model.fvList = GridDataBinding.CreateQuery(req.searchQuery, httpContext);
                // check for a view type
                CheckForViewState(@params.ViewType, model);
                // execute the grid query
                model.ExecuteGridData();
                // holding pages return in session for later call 
                ContextService.SetSessionValue("HoldTotalRowQuery", model.TotalRowsQuery, httpContext);

                // holding editable cell session for later save edit validation
                ContextService.SetSessionValue("EditableCells", JsonConvert.SerializeObject(model.ListofEditableHeader), httpContext);

                // Check raju Level Manager
                // setup levelManager | written for aspx 
                //LevelManager levelmanager = new LevelManager(passport);
                //Level level = new Level(passport);
                //levelmanager.CurrentLevel = 0;
                //levelmanager.Levels.Add(level);
                //levelmanager.ActiveLevel.Parameters = model.@params;
                //ContextService.SetSessionValue("LevelManager", JsonConvert.SerializeObject(levelmanager), httpContext);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, model.ViewId, passport.DatabaseName);
            }

            var jsonresult = Json(model);
            //jsonresult.Value = int.MaxValue;
            return jsonresult;
        }
        [HttpPost]
        public IActionResult GetTotalrowsForGrid([FromBody] SearchQueryRequestModal req)
        {
            IRepository<View> _iView = new RepositoryVB.Repositories<View>();
            int TotalPages=0;
            int TotalRows=0;
            Parameters @params = new Parameters(req.paramsUI.ViewId, passport);
            var model = new ErrorBaseModel();
            try
            {
                // get list of query fields
                if (req.searchQuery != null)
                    @params.FilterList = GridDataBinding.CreateQuery(req.searchQuery, httpContext);
                var RequestedRows = _iView.All().Where(x => x.Id == req.paramsUI.ViewId).FirstOrDefault();
                @params.RequestedRows = (int)RequestedRows.MaxRecsPerFetch;
                var asdf = HttpContext.Session.GetString("HoldTotalRowQuery");
                var conn = passport.Connection();
                TotalRows = Query.TotalQueryRowCount(ContextService.GetSessionValueByKey("HoldTotalRowQuery", httpContext), passport.Connection());

                if (TotalRows / (double)@params.RequestedRows > 0 & TotalRows / (double)@params.RequestedRows < 1)
                    TotalPages = 1;
                else if (TotalRows % @params.RequestedRows == 0)
                    TotalPages = (int)(TotalRows / (double)@params.RequestedRows);
                else
                {
                    int tp = (int)(TotalRows / (double)@params.RequestedRows);
                    TotalPages = tp + 1;
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.paramsUI.ViewId, passport.DatabaseName);
            }
            model.totalrowsForGrid = TotalRows + "|" + TotalPages + "|" + @params.RequestedRows;
            return Json(model.totalrowsForGrid);
        }
        private void CheckForViewState(int viewTypes, GridDataBinding m)
        {
            var model = ContextService.GetObjectFromJson<QueryHoldingProps>("modelParams", httpContext);
            switch (viewTypes)
            {
                case (int)ViewType.FusionView:
                        m.IsWhereClauseRequest = false;
                        m.WhereClauseStr = "";
                        break;
                case (int)ViewType.Favorite:
                        m.IsWhereClauseRequest = model.IsWhereClauseRequest;
                        m.WhereClauseStr = model.WhereClauseStr;
                        break;
                case (int)ViewType.GlobalSearch:
                        m.GsIsGlobalSearch = model.GsIsGlobalSearch;
                        m.GsSearchText = model.GsSearchText;
                        m.GsIncludeAttchment = model.GsIncludeAttchment;
                        m.GsIsAllGlobalRequest = model.GsIsAllGlobalRequest;
                        break;
            }
        }
        //// LINKSCRIPT | API CALL
        [HttpPost]
        public JsonResult LinkscriptButtonClick([FromBody] linkscriptPropertiesUI @params)
        {
            var model = new LinkScriptModel();
            var _param = new Parameters(@params.ViewId, passport);
            try
            {
                var scriptflow = ScriptEngine.RunScriptWorkFlow(@params.WorkFlow, _param.TableName, @params.TableId, @params.ViewId, passport, @params.Rowids);

                if (scriptflow.ReturnMessage != string.Empty)
                    model.ErrorMsg = scriptflow.ReturnMessage;
                else if ((scriptflow.Engine != null) && scriptflow.Engine.ShowPromptBool)
                {
                    ContextService.SetSessionValue("LinkScriptEngineMvc", JsonConvert.SerializeObject(scriptflow.Engine), httpContext);

                    model.ErrorMsg = "";
                    model.Showprompt = true;
                    if (scriptflow.ScriptControlDictionary != null)
                        model.BuiltControls(scriptflow);
                }
                model.GridRefresh = scriptflow.GridRefresh;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, @params.ViewId, passport.DatabaseName);
            }

            return Json(model);
        }
        public JsonResult LinkscriptEvents(linkscriptPropertiesUI @params)
        {
            var model = new LinkScriptModel();
            Smead.RecordsManagement.InternalEngine engine = ContextService.GetObjectFromJson<Smead.RecordsManagement.InternalEngine>("LinkScriptEngineMvc", httpContext);


            try
            {
                // Dim scriptflow = ScriptEngine.RunScriptWorkFlow(params.WorkFlow, _param.TableName, params.TableId, params.ViewId, _passport, params.Rowids)
                ScriptReturn scriptflow = ScriptEngine.RunScript(engine.ScriptName, engine.CurrentTableName, engine.RecordId, engine.ViewId, passport, passport.Connection(), engine.Caller, engine.GetSelectedRowIds);

                if (scriptflow.ReturnMessage != string.Empty)
                {
                    model.ErrorMsg = scriptflow.ReturnMessage;
                }
                else if (scriptflow.Engine is not null && scriptflow.Engine.ShowPromptBool)
                {
                    ContextService.SetSessionValue("LinkScriptEngineMvc", JsonConvert.SerializeObject(scriptflow.Engine), httpContext);

                    model.ErrorMsg = "";
                    model.Showprompt = true;
                    if (scriptflow.ScriptControlDictionary is not null)
                    {
                        model.BuiltControls(scriptflow);
                    }
                }

                model.GridRefresh = scriptflow.GridRefresh;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, @params.ViewId, passport.DatabaseName);
            }

            return Json(model);
        }

        [HttpPost]
        public JsonResult FlowButtonsClickEvent([FromServices] IConfiguration configuration, List<linkscriptUidata> linkscriptUidata)
        {
            // set engine on session
            var model = new LinkScriptModel();
            Smead.RecordsManagement.InternalEngine engine = ContextService.GetObjectFromJson<Smead.RecordsManagement.InternalEngine>("LinkScriptEngineMvc", httpContext);
            try
            {
                if (engine != null)
                    engine.ShowPromptBool = true;
                var Button = linkscriptUidata.Where(a => a.type == "button").FirstOrDefault().id;
                var _control = engine.ScriptControlDictionary[Button];
                _control.SetProperty(ScriptControls.ControlProperties.cpValue, "TRUE");
                engine.ScriptControlDictionary[Button] = _control;

                // setup all value which come from the UI to linkscript proprties.
                model.SetUpcontrolsValues(linkscriptUidata, engine);
                string[] selctedrow = engine.GetSelectedRowIds;
                // run the script
                var successful = ScriptEngine.RunScript(ref engine, 
                    engine.ScriptName, engine.CurrentTableName, engine.RecordId, engine.ViewId, 
                    passport, engine.Caller, ref selctedrow);
                engine.GetSelectedRowIds = selctedrow;
                var result = new ScriptReturn(successful, engine);
                model.GridRefresh = result.GridRefresh;
                model.ReturnMessage = result.ReturnMessage;
                model.Successful = result.Successful;

                if (result.Successful)
                    ContextService.SetSessionValue("LinkScriptEngineMvc", JsonConvert.SerializeObject(engine), httpContext);
              // Session("LinkScriptEngineMvc") = engine;

                if (result.Engine != null)
                {
                    if (result.Engine.ShowPromptBool)
                    {
                       // Session("LinkScriptEngineMvc") = result.Engine;
                        ContextService.SetSessionValue("LinkScriptEngineMvc", JsonConvert.SerializeObject(result.Engine), httpContext);
                        model.BuiltControls(result);
                        model.ContinuetoAnotherDialog = result.Engine.ShowPromptBool;
                    }
                    else
                        model.UnloadPromptWindow = true;
                }

                // Below condition used for SharePoint Move functionality
                if (configuration["SPURL"] != null)
                {
                    // these are closed in key
                    Keys.RemoveDocument(configuration);
                   Keys.IsMovedFromSP = "";
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, engine.ViewId, passport.DatabaseName);
            }

            return Json(model);
        }
        //// TABQUIK | API CALL
        [HttpGet]
        public IActionResult TabQuik(IWebHostEnvironment webHostEnvironment)
        {
            var getValues = ContextService.GetCookieVal("TabQuikViewRowselected", httpContext);
            var viewId = getValues.Split("^&&^")[0].IntValue();
            var rowsSelected = getValues.Split("^&&^")[2].ToString();
            var param = new Parameters(viewId, passport);
            var model = new TabquikApi(passport, param, rowsSelected);
            model.GetLicense();
            model.GetTabquikData(webHostEnvironment);
            model.CreateSecureLink(webHostEnvironment);
            return View(model);
        }
        //// TRACKABLE | API CALL
        [HttpGet]
        public JsonResult GetTrackbaleDataPerRow(TrackingModel.trackableUiParams track)
        {
            var model = new TrackingModel(passport, track.ViewId, track.RowKeyid, track.TableName, httpContext);
            model.ExecuteTrackingPerRow();
            return Json(model);
        }
        //// FAVORITE: ADD - DELETE - UPDATE | API CALL
        [HttpPost]
        public JsonResult ReturnFavoritTogrid([FromBody] ReturnFavoritTogridReqModel req)
        {
            IRepository<View> _iView = new Repositories<View>();
            var model = new GridDataBinding(passport, req.paramss.ViewId, req.paramss.pageNum, 0, (int)ViewType.Favorite, "", httpContext);
            model.IsWhereClauseRequest = true;
            model.WhereClauseStr = string.Format("select [TableId] from s_SavedChildrenFavorite where SavedCriteriaId = {0}", req.paramss.FavCriteriaid);
            if (req.searchQuery != null)
                model.fvList = GridDataBinding.CreateQuery(req.searchQuery, httpContext);
            model.ExecuteGridData();

            var qmodel = new QueryHoldingProps();
            qmodel.IsWhereClauseRequest = model.IsWhereClauseRequest;
            qmodel.WhereClauseStr = model.WhereClauseStr;
           
            ContextService.SetSessionValue("modelParams", JsonConvert.SerializeObject(qmodel), httpContext);
            ContextService.SetSessionValue("HoldTotalRowQuery", model.TotalRowsQuery, httpContext);
            ContextService.SetSessionValue("EditableCells", JsonConvert.SerializeObject(model.ListofEditableHeader), httpContext);
            // model.PageNumber = params.pageNum
            var jsonresult = Json(model);
            //jsonresult.Value = int.MaxValue;
            return jsonresult;
        }
        [HttpPost]
        public JsonResult DeleteFavorite([FromBody] DeleteFavoriteRecordReqModel req)
        {
            var model = new MyFavorite();
            try
            {
                var isdeleted = SavedCriteria.DeleteSavedCriteria(req.paramss.FavCriteriaid, "1");
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.paramss.ViewId, passport.DatabaseName);
            }
            return Json(model);
        }
        [HttpPost]
        public JsonResult AddNewFavorite([FromBody] FavoriteRecordReqModel req)

        {
            var listOfKeys = new List<string>();
            var model = new MyFavorite();
            try
            {
                IRepository<s_SavedCriteria> _is_SavedCriteria = new RepositoryVB.Repositories<s_SavedCriteria>();
                IRepository<s_SavedCriteria> ceritriaName = new Repositories<s_SavedCriteria>();
                IRepository<s_SavedChildrenFavorite> _is_SavedChildrenFavorite = new RepositoryVB.Repositories<s_SavedChildrenFavorite>();
                if (ceritriaName.Where(a => a.SavedName.Trim() == req.paramss.NewFavoriteName.Trim()).ToList().Count > 0)
                {
                    model.isWarning = true;
                    model.Msg = string.Format(Languages.get_Translation("msgFavoriteDuplicate"), req.paramss.NewFavoriteName);
                }
                else
                {
                    foreach (var rec in req.recordkeys)
                        listOfKeys.Add(rec.rowKeys);
                    var msg = "";
                    var ps_SavedCriteriaId = SavedCriteria.SaveSavedCriteria(passport.UserId, ref msg, req.paramss.NewFavoriteName, req.paramss.ViewId, _is_SavedCriteria);
                    if (ps_SavedCriteriaId != default)
                    {
                        var pOutPut = SavedCriteria.SaveSavedChildrenFavourite(ref msg, true, ps_SavedCriteriaId, req.paramss.ViewId, listOfKeys, _is_SavedCriteria, _is_SavedChildrenFavorite);
                        model.SaveCriteriaId = ps_SavedCriteriaId;
                    }
                }
            }

            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.paramss.ViewId, passport.DatabaseName);
            }
            return Json(model);
        }

        [HttpPost]
        public JsonResult UpdateFavorite([FromBody] FavoriteRecordReqModel req)
        {
            var listOfKeys = new List<string>();
            foreach (var rec in req.recordkeys)
                listOfKeys.Add(rec.rowKeys);
            var model = new MyFavorite();
            try
            {
                IRepository<s_SavedCriteria> _is_SavedCriteria = new RepositoryVB.Repositories<s_SavedCriteria>();
                IRepository<s_SavedChildrenFavorite> _is_SavedChildrenFavorite = new RepositoryVB.Repositories<s_SavedChildrenFavorite>();
                var msg = "";
                SavedCriteria.SaveSavedChildrenFavourite(ref msg, true, req.paramss.FavCriteriaid, req.paramss.ViewId, listOfKeys, _is_SavedCriteria, _is_SavedChildrenFavorite);
                model.Msg = msg;

                model.SaveCriteriaId = req.paramss.FavCriteriaid;
            }
            catch (Exception ex)
            {
                model.Msg = ex.Message;
            }
            return Json(model);
        }
        [HttpPost]
        public JsonResult DeleteFavoriteRecord([FromBody]FavoriteRecordReqModel req)
        {
            var lst = new List<string>();
            var model = new MyFavorite();
            foreach (var d in req.recordkeys)
                lst.Add(d.rowKeys);
            try
            {
                SavedCriteria.DeleteFavouriteRecords(lst, req.paramss.FavCriteriaid);
            }
            catch (Exception ex)
            {
                model.Msg = ex.Message;
            }
            return Json(model);
        }
        //// QUERY: ADD - DELETE - UPDATE | API CALL 
        [HttpPost]
        public JsonResult SaveNewQuery([FromBody] SaveNewUpdateDeleteQueryReqModel req)
        {
            if (!passport.CheckPermission(Common.SECURE_MYQUERY, Smead.Security.SecureObject.SecureObjectType.Application,
                Permissions.Permission.Access))
                return null /* TODO Change to default(_) if this is not a reference type */ ;
            var model = new Myquery(passport, req.paramss, req.Querylist);
            try
            {
                model.InsertNewQuery();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.paramss.ViewId, passport.DatabaseName);
            }

            return Json(model);
        }
        public object HTMLencoder(object model)
        {
            HttpUtility.HtmlEncode(model);
            return model;
        }


        [HttpPost]
        public JsonResult UpdateQuery([FromBody] SaveNewUpdateDeleteQueryReqModel req)
        {
            var model = new Myquery(passport, req.paramss, req.Querylist);
            try
            {
                model.UpdateQuery();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.paramss.ViewId, passport.DatabaseName);
            }

            return Json(model);
        }
        [HttpPost]
        public JsonResult DeleteQuery([FromBody] SaveNewUpdateDeleteQueryReqModel req)
        {
            var model = new Myquery(passport, req.paramss);
            try
            {
                model.DeleteQuery();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.paramss.ViewId, passport.DatabaseName);
            }

            return Json(model);
        }
        //// GLOBAL SEARCH | API CALL---- 
        [HttpPost]
        public JsonResult RunglobalSearch(GlobalSearch.globalSearchUI @params)
        {
            var model = new GlobalSearch(passport, @params.ViewId, @params.SearchInput, @params.ChkAttch, @params.ChkcurTable, @params.ChkUnderRow, @params.TableName, @params.Currentrow, httpContext);
            model.HtmlReturn();
            return Json(model);
        }
        [HttpPost]
        public JsonResult GlobalSearchClick(GlobalSearch.globalSearchUI @params)
        {
            var model = new GridDataBinding(passport, @params.ViewId, 1, @params.crumbLevel, (int)ViewType.GlobalSearch, "", httpContext);

            try
            {
                model.GsIsGlobalSearch = true;
                model.GsKeyvalue = WebUtility.UrlDecode(@params.KeyValue).Replace("'", "''");
                model.GsSearchText = @params.SearchInput;
                model.GsIsAllGlobalRequest = false;
                model.ExecuteGridData();
               // Session("HoldTotalRowQuery") = model.TotalRowsQuery;
                ContextService.SetSessionValue("HoldTotalRowQuery", model.TotalRowsQuery, httpContext);
                //Session("EditableCells") = model.ListofEditableHeader;
                ContextService.SetSessionValue("EditableCells", JsonConvert.SerializeObject(model.ListofEditableHeader), httpContext);

                // setup levelManager | written for aspx 
                LevelManager levelmanager = new LevelManager(passport);
                Level level = new Level(passport);
                levelmanager.CurrentLevel = 0;
                levelmanager.Levels.Add(level);
                levelmanager.ActiveLevel.Parameters = model.@params;
              //  Session("LevelManager") = levelmanager;
                ContextService.SetSessionValue("LevelManager", JsonConvert.SerializeObject(levelmanager), httpContext);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, @params.ViewId, passport.DatabaseName);
            }

            var jsonresult = Json(model);
            //jsonresult.Value = int.MaxValue;
            return jsonresult;
        }
        [HttpPost]
        public JsonResult GlobalSearchAllClick(GlobalSearch.globalSearchUI @params)
        {
            var model = new GridDataBinding(passport, @params.ViewId, 1, @params.crumbLevel, (int)ViewType.GlobalSearch, "", httpContext);

            try
            {
                model.GsIsGlobalSearch = true;
                model.GsSearchText = @params.SearchInput;
                model.GsIncludeAttchment = @params.IncludeAttchment;
                model.GsIsAllGlobalRequest = true;
                model.ExecuteGridData();
                var qmodel = new QueryHoldingProps();
                qmodel.GsIsGlobalSearch = true;
                qmodel.GsSearchText = @params.SearchInput;
                qmodel.GsIncludeAttchment = @params.IncludeAttchment;
                qmodel.GsIsAllGlobalRequest = true;
                 
                ContextService.SetSessionValue("modelParams", JsonConvert.SerializeObject(qmodel), httpContext);
                ContextService.SetSessionValue("HoldTotalRowQuery", model.TotalRowsQuery, httpContext);
                ContextService.SetSessionValue("EditableCells", JsonConvert.SerializeObject(model.ListofEditableHeader), httpContext);

                //Session("modelParams") = qmodel;
                //Session("HoldTotalRowQuery") = model.TotalRowsQuery;
                //Session("EditableCells") = model.ListofEditableHeader;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, @params.ViewId, passport.DatabaseName);
            }

            var jsonresult = Json(model);
            //jsonresult.Value = int.MaxValue;
            return jsonresult;
        }
        //// GRIG: ADD - DELETE - EDIT | API CALL
        [HttpPost]
        public IActionResult SetDatabaseChanges([FromBody] Saverows.DatabaseChangesReq req)
        {
            IRepository<View> _iView = new Repositories<View>();
            // check fields and validate again (written for hacking possibility).
            if (!ValidateFieldsBeforeSave(req.Rowdata))
            {
                slimShared.SlimShared.logMessage(Languages.get_Translation("MsgSecurityValuation"), false, EventLogEntryType.Error, "TAB FusionRMS Web Access");
                return null /* TODO Change to default(_) if this is not a reference type */ ;
            }

            var pkeyId = req.Rowdata[req.Rowdata.Count - 1].value;
            Saverows model = null /* TODO Change to default(_) if this is not a reference type */ ;
            try
            {
                model = new Saverows(passport, req.paramss, Keys.GetClientIpAddress(httpContext), req.Rowdata, pkeyId, httpContext);
                if ((string.IsNullOrEmpty(pkeyId)))
                    // insert new row
                    model.AddNewRow(req.paramss.childkeyfield);
                else
                    // save edit row
                    model.EditRow(req.paramss.childkeyfield);
                if (model.LinkScriptSession != null)
                   // Session("LinkScriptEngineMvc") = model.LinkScriptSession;
                ContextService.SetSessionValue("LinkScriptEngineMvc", JsonConvert.SerializeObject(model.LinkScriptSession), httpContext);

                model.LinkScriptSession = null;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.paramss.ViewId, passport.DatabaseName);
            }
            return Json(model);
        }
        private bool ValidateFieldsBeforeSave(List<Saverows.RowsparamsUI> rowdata)
        {
            var Fields = ContextService.GetObjectFromJson<List<TableEditableHeader>>("EditableCells", httpContext);
            int index = 0;
            try
            {
                foreach (Saverows.RowsparamsUI f in rowdata)
                {
                    var field = Fields.Where(a => a.ColumnName == f.columnName).FirstOrDefault();
                    if (!(field == null))
                    {
                        if (!(field.DataTypeFullName == "System.Boolean"))
                        {
                            if (!field.Allownull && !field.isCounterField)
                            {
                                if (string.IsNullOrEmpty(f.value))
                                {
                                    return false;
                                }
                            }
                            if (field.DataTypeFullName == "System.Int16" && System.Convert.ToInt32(f.value) > 32767)
                            {
                                return false;
                            }

                            if (field.DataTypeFullName == "System.Int32" && System.Convert.ToInt32(f.value) > 2147483647)
                            {
                                return false;
                            }

                            if (field.DataTypeFullName == "System.DateTime")
                            {

                                if (Keys.get_ConvertCultureDate(f.value, "E", httpContext) == null)
                                {
                                    return false;
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }


            return true;
        }

        [HttpPost]
        public JsonResult CheckForduplicateId([FromBody] Saverows.DatabaseChangesReq req)
        {
            var model = new ErrorBaseModel();
            try
            {
                model.isDuplicated = Navigation.CheckIfDuplicatePrimaryKey(passport, req.paramss.Tablename, req.paramss.PrimaryKeyname, req.paramss.KeyValue);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.paramss.ViewId, passport.DatabaseName);
            }
            return Json(model);
        }
        [HttpPost]
        public JsonResult DeleteRowsFromGrid([FromBody] DeleteRowsFromGridReqModel req)
        {
            var model = new Deleterows(passport, req.rowData.ids, req.paramss.viewid, Keys.GetClientIpAddress(httpContext), req.paramss.ScriptDone);
            try
            {
                model.DeleteRows();
                if (model.LinkScriptSession != null)
                    //Session("LinkScriptEngineMvc") = model.LinkScriptSession;
                    ContextService.SetSessionValue("LinkScriptEngineMvc", JsonConvert.SerializeObject(model.LinkScriptSession), httpContext);

                model.LinkScriptSession = null;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.paramss.viewid, passport.DatabaseName);
            }

            return Json(model);
        }
        //// TASKBAR CLICK AND RETURN TABLE | API CALL
        [HttpGet]
        public JsonResult TaskBarClick(int viewId)
        {
            var model = new GridDataBinding(passport, viewId, 1, 0, (int)ViewType.FusionView, "", httpContext);
            try
            {
                model.IsOpenWhereClause = true;
                model.IsWhereClauseRequest = true;
                model.WhereClauseStr = "";
                model.ExecuteGridData();
                //Session("HoldTotalRowQuery") = model.TotalRowsQuery;
                ContextService.SetSessionValue("HoldTotalRowQuery", model.TotalRowsQuery, httpContext);

            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, viewId, passport.DatabaseName);
            }

            var JsonResult = Json(model);
            //JsonResult.Value = int.MaxValue;
            return JsonResult;
        }
        //// REPORTING | API CALL
        [HttpPost]
        public JsonResult Reporting([FromBody] ReportingReqModel req)
        {
            var report = new ReportingPerRow(req.paramss.Tableid, req.paramss.tableName, req.paramss.viewId, req.paramss.pageNumber, passport, httpContext);

            try
            {
                report.ExecuteReporting((ReportingPerRow.Reports)req.paramss.reportNum);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, report, req.paramss.viewId, passport.DatabaseName);
            }

            var JsonResult = Json(report);
            //JsonResult.Value = int.MaxValue;
            return JsonResult;
        }
        [HttpPost]
        public JsonResult ReportingCount([FromBody] ReportingReqModel req)
        {
            var report = new ReportingPerRow(req.paramss.Tableid, req.paramss.tableName, req.paramss.viewId, req.paramss.pageNumber, passport, httpContext);
            try
            {
                report.ExecuteReportingCount((ReportingPerRow.Reports)req.paramss.reportNum);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, report, req.paramss.viewId, passport.DatabaseName);
            }
            return Json(report.Paging);
        }
        //// REPORTING AUDIT SEARCH
        [HttpPost]
        public JsonResult RunAuditSearch(AuditReportSearch.UIproperties @params)
        {
            var model = new AuditReportSearch(passport, httpContext);
            try
            {
                model.RunQuery(@params);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return Json(model);
        }
        //// RETENTION INFORMATION
        [HttpPost]
        public JsonResult OnDropdownChange([FromBody] RetentionInfoUpdateReqModel req)
        {
            var model = new RetentionInfo(req.props, passport, httpContext);
            try
            {
                model.OnDropdownChange();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.props.viewid, passport.DatabaseName);
            }
            return Json(model);
        }
        [HttpPost]
        public JsonResult RetentionInfoUpdate([FromBody] RetentionInfoUpdateReqModel req)
        {
            var model = new RetentionInfo(passport, httpContext);
            try
            {
                model.StartUpdating(req.props);
                // optional use to return row and update the grid (dynamic return)
                model.ReturnOnerow = ReturnOnerow(req.props.viewid, req.props.rowid);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.props.viewid, passport.DatabaseName);
            }
            return Json(model);
        }
        //// PARTIAL VIEW RETURNS
        [HttpGet]
        public ActionResult DataGridView()
        {
            return PartialView("_DataGrid");
        }
        [HttpGet]
        public ActionResult ReportingReturn()
        {
            return PartialView("_Reporting");
        }
        [HttpGet]
        public ActionResult NewFavorite()
        {
            return PartialView("_NewFavorite");
        }
        [HttpGet]
        public JsonResult CheckBeforeAddTofavorite(int viewid)
        {
            try
            {
                IRepository<s_SavedCriteria> _is_SavedCriteria = new RepositoryVB.Repositories<s_SavedCriteria>();
                bool hasList = false;
                var criteriaList = _is_SavedCriteria.Where(a => a.ViewId == viewid & a.SavedType == 1).ToList();
                if (criteriaList.Count > 0)
                    hasList = true;
                return Json(hasList);
            }
            catch(Exception)
            {
                return Json(false);
            }
        }
        [HttpGet]
        public ActionResult StartDialogAddToFavorite(int viewid)
        {
            var model = new MyFavorite();
            model.placeholder = Languages.get_Translation("FavoriteListDdl");
            model.label = Languages.get_Translation("lblAddFavourite");
            try
            {
                IRepository<s_SavedCriteria> _is_SavedCriteria = new RepositoryVB.Repositories<s_SavedCriteria>();
                var criteriaList = _is_SavedCriteria.Where(a => a.ViewId == viewid & a.SavedType == 1).ToList();
                if (criteriaList.Count > 0)
                {
                    foreach (var lst in criteriaList)
                    {
                        var ddl = new MyFavorite.FavoritedropdownList();
                        var name = lst.SavedName;
                        ddl.text = name;
                        ddl.value = lst.Id.ToString();
                        model.ListAddtoFavorite.Add(ddl);
                    }
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, viewid, passport.DatabaseName);
                model.Msg = ex.Message;
            }

            return PartialView("_AddtoFavorite", model);
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
        public ActionResult GetRetentionInfo(string id, int viewId)
        {
            id = HttpUtility.HtmlEncode(id);
            var model = new RetentionInfo(id, viewId, passport, httpContext);
            try
            {
                model.GetRetentionInfoPerRow();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, viewId, passport.DatabaseName);
            }
            return PartialView("_RetentionInfo", model);
        }
        [HttpGet]
        public ActionResult RetentionInfoHolde()
        {
            return PartialView("_RetentionInfoHolde");
        }
        [HttpPost]  //model UserInterfaceJsonModel
        public ActionResult DialogMsgConfirmDelete([FromBody]DialogMsgConfirmDeleteReqModel req)
        {
            var lst = new List<string>();
            try
            {
                foreach (string id in req.paramss.ids)
                    lst.Add(Navigation.GetItemName(req.paramss.TableName, id, passport));
            }
            catch (Exception) { }
            ViewBag.ItemList = lst;
            return PartialView("_DialogMsgConfirmDelete");
        }
        [HttpPost]
        public ActionResult LoadNewRecordForm()
        {
            return PartialView("_LoadNewRecordForm");
        }

        //// SHARED METHODS
        public List<List<string>> ReturnOnerow(int viewid, string rowid)
        {
            var param = new Parameters(viewid, passport);
            var q = new GridDataBinding(passport, viewid, 1, 0, (int)ViewType.FusionView, "", httpContext);
            q.IsWhereClauseRequest = true;
            if (Navigation.IsAStringType(param.IdFieldDataType) || Navigation.IsADateType(param.IdFieldDataType))
                q.WhereClauseStr = string.Format("SELECT [{0}] FROM [{1}] where [{0}] = '{2}'", Navigation.MakeSimpleField(param.KeyField), param.TableName, rowid.Replace("'", "''"));
            else
                q.WhereClauseStr = string.Format("SELECT [{0}] FROM [{1}] where [{0}] = {2}", Navigation.MakeSimpleField(param.KeyField), param.TableName, rowid);
            q.ExecuteGridData();
            return q.ListOfDatarows;
        }

    }

    
}
 


 







