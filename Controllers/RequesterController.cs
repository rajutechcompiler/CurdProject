using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Smead.RecordsManagement;
using static Smead.RecordsManagement.Navigation;
using static Smead.RecordsManagement.Tracking;
using Smead.Security;
using TabFusionRMS.Resource;
using TabFusionRMS.WebCS;
using Microsoft.AspNetCore.Mvc;
using TabFusionRMS.WebCS.Controllers;



namespace Controllers
{
    public class RequesterController : BaseController
    {
        //public RequesterController(IHttpContextAccessor httpContext) : base(httpContext)
        //{ }

        [HttpPost]

        public IActionResult GetRequsterpopup([FromBody] UserInterfaceJsonReqModel req)
        {
            var model = new Requesters();
            try
            {
                model.rdemployddlist = DrawRequestorList(req.paramss);
                model.itemsTobeRequest = ItemsRequest(req.paramss, model);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.paramss.ViewId, passport.DatabaseName);
            }

            return this.PartialView("_getrequest", model);
        }
        [HttpPost]
        public IActionResult SearchEmployees([FromBody]UserInterfaceJsonModel @params)
        {
            // If params.Request.TextFilter.Length < 3 Then Return Nothing
            var model = new Requesters();
            try
            {
                model.rdemployddlist = DrawRequestorList(@params);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, @params.ViewId, passport.DatabaseName);
            }
            return this.Json(model);
        }
        [HttpPost]
        public IActionResult SubmitRequest([FromBody] UserInterfaceJsonModel req)
        {
            var model = new Requesters();
            var message = new StringBuilder();
            var requests = new Requesting();
            if (string.IsNullOrEmpty(req.Request.Instruction))
                req.Request.Instruction = "";
            try
            {
                var reqList = requests.GetActiveRequests(req.ViewId, req.ids[0], passport);
                var tableInfo = Navigation.GetTableInfo(req.TableName, passport);
                Requesting.MakeRequest(ref message, req.ids, req.TableName, req.Request.Employeeid, req.Request.Instruction, req.Request.Priotiry, DateTime.Parse(req.Request.ReqDate, Keys.GetCultureCookies(httpContext)), req.Request.ischeckWaitlist, passport, tableInfo);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, req.ViewId, passport.DatabaseName);
            }

            if (message.Length > 0)
            {
                model.isError = true;
                model.Msg = message.ToString();
            }

            return this.Json(model);
        }
        [HttpGet]
        public IActionResult DeleteTrackingRequest(int id)
        {
            var model = new TrackingModel(httpContext);
            if (passport.CheckPermission(" Requestor", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.Configure))
            {
                model.isDeleteAllow = true;
                try
                {
                    Requesting.DeleteRequest(id, passport);
                }
                catch (Exception ex)
                {
                    DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                }
            }
            return this.Json(model);
        }
        [HttpGet]
        public IActionResult GetRequestDetails(int id, string tableName)
        {
            var requestInfo = Requesting.GetRequest(id, tableName, passport);
            var model = new Requesters();
            model.RequestID = id.ToString();
            model.Priority = requestInfo.Priority;
            model.DateRequested = Keys.get_ConvertCultureDate(requestInfo.DateRequested, httpContext); // Query.ConvertUSDateToLocal(requestInfo.DateRequested, LevelManager.ActiveLevel.Parameters.Culture)
            model.DateNeeded = Keys.get_ConvertCultureDate(requestInfo.DateNeeded, httpContext); // Query.ConvertUSDateToLocal(requestInfo.DateNeeded, LevelManager.ActiveLevel.Parameters.Culture)
            model.Status = requestInfo.Status;
            model.Instruction = requestInfo.Instructions;
            model.Fulfilled = string.Compare(requestInfo.Status, "Fulfilled", true) == 0;
            // If Not String.IsNullOrEmpty(requestInfo.Instructions.Trim()) Then
            // model.Fulfilled = True
            // Else
            // model.Fulfilled = False
            // End If

            var employeeTableInfo = GetRequestorTableInfo(passport);
            var requestedFor = new Smead.RecordsManagement.Employee(employeeTableInfo);
            var requestedBy = new Smead.RecordsManagement.Employee(employeeTableInfo);
            requestedFor.LoadByID(requestInfo.EmployeeID, passport);
            model.txtFor = requestedFor.Description;
            model.Phone = requestedFor.Phone;
            model.Mail = requestedFor.MailStop;

            if (!string.IsNullOrEmpty(requestInfo.RequestedBy))
            {
                if (requestedBy.LoadByName(requestInfo.RequestedBy, passport))
                {
                    model.By = requestedBy.Description;
                }
                else
                {
                    model.By = ShowUserName(requestInfo.RequestedBy, passport);
                }
            }
            else
            {
                model.By = Languages.get_Translation("txtRequestDetailsBy");
            }

            if (requestInfo.PullListID != 0)
            {
                var pullInfo = GetPullList(requestInfo.PullListID, passport);
                model.PullList = requestInfo.PullListID.ToString();
                model.PullDate = Keys.get_ConvertCultureDate(pullInfo["DateCreated"].ToString(), httpContext);
                model.PullOperator = pullInfo["OperatorsId"].ToString();
            }

            model.ExceptionAllowed = false;
            model.isException = string.Compare(requestInfo.Status, "Exception", true) == 0;

            if (passport.CheckPermission(" Requestor", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.Configure))
            {
                model.ExceptionAllowed = "|Exception|In Process|New|New Batch|".IndexOf(string.Format("|{0}|", requestInfo.Status)) >= 0;
            }

            if (model.ExceptionAllowed)
                model.isComment = true;
            model.txtComment = requestInfo.ExceptionComments;

            return this.PartialView("_requestdetails", model);
        }

        [HttpPost]
        public IActionResult UpdateRequest(Requesters param)
        {
            var model = new TrackingModel(httpContext);
            if (passport.CheckPermission(" Requestor", Smead.Security.SecureObject.SecureObjectType.Reports, Permissions.Permission.Configure))
            {
                try
                {
                    Requesting.UpdateRequest(Convert.ToInt32(param.RequestID), param.Fulfilled, param.isException, param.txtComment, passport);
                }
                catch (Exception ex)
                {
                    DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                }
            }

            return this.Json(model);
        }

        private List<string> ItemsRequest(UserInterfaceJsonModel @params, Requesters model)
        {
            bool allowWaitList = CBoolean(GetSystemSetting("AllowWaitList", passport));
            var requests = new Requesting();
            var reqList = requests.GetActiveRequests(@params.ViewId, @params.ids[0], passport);
            model.chkWaitList = false;
            var lst = new List<string>();
            foreach (string id in @params.ids)
            {
                bool atAnEmployee = Tracking.AlreadyAtAnEmployee(@params.TableName, id, passport);
                if (allowWaitList && atAnEmployee | reqList.Count > 0)
                {
                    model.chkWaitList = true;
                }
                else if (!allowWaitList && atAnEmployee)
                {
                    model.lblError = Languages.get_Translation("lblRequestsRequestedCheckedOut");
                }
                string ItemName = Navigation.GetItemName(@params.TableName, id, passport);
                lst.Add(ItemName);
            }
            return lst;
        }

        private List<RequstRadioBox> DrawRequestorList(UserInterfaceJsonModel @params)
        {
            var li = new List<RequstRadioBox>();
            var lil = new List<RequstRadioBox>();
            bool _requestOnBehalf = passport.CheckPermission(@params.TableName, Smead.Security.SecureObject.SecureObjectType.Table, Permissions.Permission.RequestOnBehalf);
            string userName = string.Empty;
            int count = 0;
            var requestorRow = GetRequestorTableInfo(passport);
            string requestorTableName = requestorRow["TableName"].ToString();
            userName = GetEmployeeRequestorName(passport);
            var TrackingStatus = Tracking.TrackingStatusByTableAndId(@params.TableName, "", passport);
            var rq = new Requesting();
            bool allowWaitList = CBoolean(GetSystemSetting("AllowWaitList", passport));

            using (var conn = passport.Connection())
            {
                foreach (var req in rq.GetActiveRequests(@params.ViewId, @params.ids[0], passport, conn))
                    lil.Add(new RequstRadioBox() { text = req.Name, value = req.EmployeeID });
            }
            foreach (Container item in Tracking.GetContainersByType(requestorTableName, @params.Request.TextFilter, _requestOnBehalf, passport))
            {
                var TrackingRows = TrackingStatus.Select(string.Format("{0}='{1}'", requestorRow["TrackingStatusFieldName"].ToString(), item.ID));
                var objli = new RequstRadioBox();
                if (!(TrackingRows.Count() > 0))
                {
                    objli.text = item.Name;
                    objli.value = item.ID;
                    objli.disable = false;

                    foreach (RequstRadioBox re in lil)
                    {
                        if ((re.value ?? "") == (objli.value ?? ""))
                        {
                            objli.disable = true;
                            objli.text += " - " + Languages.get_Translation("liTxtRequestsAlreadyRequested");
                        }
                    }

                    // Else
                    // 'li.Selected = Not _requestOnBehalf OrElse String.Compare(userName, item.Name, True) = 0
                    // End If
                }
                li.Add(objli);
            }
            return li;
        }

    }


    public class Requesters : BaseModel
    {
        public List<RequstRadioBox> rdemployddlist { get; set; }
        public List<string> itemsTobeRequest { get; set; }
        public bool chkWaitList { get; set; }
        public string lblError { get; set; }
        public string RequestID { get; set; }
        public string Priority { get; set; }
        public string DateRequested { get; set; }
        public string DateNeeded { get; set; }
        public string Status { get; set; }
        public string Instruction { get; set; }
        public bool Fulfilled { get; set; }
        public string txtFor { get; set; }
        public string Phone { get; set; }
        public string Mail { get; set; }
        public string By { get; set; }
        public string PullList { get; set; }
        public string PullDate { get; set; }
        public string PullOperator { get; set; }
        public bool ExceptionAllowed { get; set; }
        public string txtComment { get; set; }
        public bool isComment { get; set; }
        public bool isException { get; set; }
    }

    public class RequstRadioBox
    {
        public string value { get; set; }
        public string text { get; set; }
        public bool disable { get; set; }
    }
}