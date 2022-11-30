using System;
using System.Collections.Generic;
using Smead.RecordsManagement;
using static Smead.RecordsManagement.Navigation;
using static Smead.RecordsManagement.Tracking;
using Smead.Security;
using TabFusionRMS.Resource;

namespace TabFusionRMS.WebCS
{

    // TRACKING MODEL RETURN DATA AND DOM OBJECT
    public class TrackingModel : BaseModel
    {
        public TrackingModel(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }
        public TrackingModel(Passport passport, int viewId, int trackrowid, string tablename, HttpContext httpContext)
        {
            _passport = passport;
            _httpContext = httpContext;
            ViewId = viewId;
            TrackRowId = trackrowid;
            ListofRequests = new List<Requestlist>();
            TableName = tablename;
        }
        private Parameters @params { get; set; }
        private int ViewId { get; set; }
        private int TrackRowId { get; set; }
        public string lblTracking { get; set; }
        public string lblTrackTime { get; set; }
        public string lblDueBack { get; set; }
        public List<Requestlist> ListofRequests { get; set; }
        public string TableName { get; set; }
        public bool isDeleteAllow { get; set; } = false;

        public void ExecuteTrackingPerRow()
        {
            GetTrackingInfo();
            GetRequestWaitlist();
        }
        private void GetTrackingInfo()
        {
            @params = new Parameters(ViewId, _passport);
            Dictionary<string, string> argidsByTable = null;
            var tracks = GetTrackableStatus(GetViewTableName(ViewId, _passport), TrackRowId.ToString(), _passport, idsByTable: ref argidsByTable);
            if (tracks is null | @params.NewRecord)
            {
                lblTracking = Languages.get_Translation("lblTrackingNeverTracked");
            }
            else
            {
                lblTrackTime = string.Format(Languages.get_Translation("lblTrackingBy"), Keys.get_ConvertCultureDate(tracks[0].TransactionDate.ToString(), _httpContext, true), tracks[0].UserName.ToString());
                foreach (Container cont in tracks[0].Containers)
                {
                    lblTracking += GetItemName(cont.Type, cont.ID.ToString(), _passport, true) + "<br>";
                    switch (cont.OutType)
                    {
                        case 0:
                            {
                                string trackingField = GetTableInfo(cont.Type, _passport)["TrackingOutFieldName"].ToString();
                                if (!string.IsNullOrEmpty(trackingField) && CBoolean(GetSingleFieldValue(cont.Type, cont.ID, trackingField, _passport)[0]))
                                {
                                    lblDueBack = Languages.get_Translation("Out").ToUpper();
                                    if (CBoolean(GetSystemSetting("DateDueOn", _passport)))
                                    {
                                        if (!tracks[0].DateDue.Equals(new DateTime()))
                                            lblDueBack += string.Format(Languages.get_Translation("msgTrackingDueBackOn"), " - ", Keys.get_ConvertCultureDate(tracks[0].DateDue.ToString(), _httpContext));
                                    }
                                }
                                else
                                {
                                    lblDueBack = Languages.get_Translation("lblIN").ToUpper();
                                }

                                break;
                            }
                        case 1:
                            {
                                lblDueBack = Languages.get_Translation("Out").ToUpper() + " ";
                                if (CBoolean(GetSystemSetting("DateDueOn", _passport)))
                                {
                                    if (!tracks[0].DateDue.Equals(new DateTime()))
                                        lblDueBack += string.Format(Languages.get_Translation("msgTrackingDueBackOn"), " - ", Keys.get_ConvertCultureDate(tracks[0].DateDue.ToString(), _httpContext));
                                }

                                break;
                            }
                        case 2:
                            {
                                lblDueBack = Languages.get_Translation("lblIN").ToUpper();
                                break;
                            }
                    }
                }
            }
        }
        private void GetRequestWaitlist()
        {
            var requests = new Requesting();
            string trckid = PrepPad(TrackRowId.ToString());
            foreach (Request req in requests.GetActiveRequests(ViewId, trckid, _passport))
            {
                var model = new Requestlist();
                model.DateRequested = Keys.get_ConvertCultureDate(req.DateRequested.ToString(), _httpContext);
                model.EmployeeName = req.Name;
                model.DateNeeded = Keys.get_ConvertCultureDate(req.DateNeeded.ToString(), _httpContext);
                model.Status = req.Status;
                model.reqid = req.RequestID.ToString();
                ListofRequests.Add(model);

                if (_passport.CheckPermission(" Requestor", SecureObject.SecureObjectType.Reports, Permissions.Permission.Configure))
                {
                    isDeleteAllow = true;
                }

            }
            // Dim dt As New DataTable
            // Dim trckid As String = Navigation.PrepPad(Me.TrackRowId)
            // Dim txtcommnad As String = "select id, DateRequested, EmployeeId, DateNeeded, [Status] from SLRequestor where TableId = @rowid and [status] = 'WaitList'"
            // Using cmd As New SqlCommand(txtcommnad, _passport.Connection())
            // cmd.Parameters.AddWithValue("@rowid", trckid)
            // Dim adp = New SqlDataAdapter(cmd)
            // adp.Fill(dt)
            // End Using
            // 'DateRequested, EmployeeId, DateNeeded, [Status]
            // For Each row As DataRow In dt.Rows
            // Dim employName As String = Navigation.GetItemName(Me.TableName, row("EmployeeId").ToString, _passport)
            // ListofRequests.Add(New Requestlist With {.DateRequested = row("DateRequested").ToString, .EmployeeName = employName, .DateNeeded = row("DateNeeded").ToString, .Status = row("Status").ToString, .reqid = row("id")})
            // Next

        }
        public class trackableUiParams
        {
            public int ViewId { get; set; }
            public int RowKeyid { get; set; }
            public string TableName { get; set; }
        }

        public class Requestlist
        {
            public string DateRequested { get; set; }
            public string EmployeeName { get; set; }
            public string DateNeeded { get; set; }
            public string Status { get; set; }
            public string reqid { get; set; }
        }
    }
}