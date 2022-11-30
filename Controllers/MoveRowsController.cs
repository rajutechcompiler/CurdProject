using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Smead.RecordsManagement;
using static Smead.RecordsManagement.Navigation;
using Smead.Security;
using TabFusionRMS.Resource;
using TabFusionRMS.WebCS.Controllers;
using Microsoft.AspNetCore.Mvc;
using TabFusionRMS.WebCS;

namespace Controllers
  {
    public class MoveRowsController : BaseController
    {
        //public MoveRowsController(IHttpContextAccessor httpContext) : base(httpContext)
        //{ }
        // GET: MoveRows
        [HttpPost]
            public IActionResult GetMovePopup([FromBody] UserInterfaceJsonReqModel req)
            {
                if (req.paramss.ids.Count == 0)
                    return null;
                var model = new Moving();
                try
                {
                    LoadDestinationTables(req.paramss, model);
                    // model.rbDestinationsItem = LoadDestinationItems(params.MoveRecords.MoveViewid)
                    model.itemsTobeMove = LoadItemsTobeMove(req.paramss);
                    model.moveView = req.paramss.MoveRecords.MoveViewid.ToString();
                }
                catch (Exception ex)
                {
                    DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                }
                return PartialView("_moverows", model);
            }
            [HttpPost]
            public IActionResult LoadDestinationItems([FromBody] UserInterfaceJsonReqModel req)
            {
                var model = new Moving();
                try
                {
                    model.rbDestinationsItem = this.DestinationItemsList(req.paramss.MoveRecords.MoveViewid);
                }
                catch (Exception ex)
                {
                    DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                }
                return this.Json(model);
            }
            [HttpPost]
            public IActionResult FilterItemsList([FromBody] UserInterfaceJsonReqModel req)
            {
                // If params.MoveRecords.TextFilter.Length < 3 Then Return Nothing

                var model = new MoveradioBoxModel();
                try
                {
                    var qry = new Query(passport);
                    var localparam = new Parameters(req.paramss.MoveRecords.MoveViewid, passport);
                    localparam.QueryType = queryTypeEnum.LikeItemName;
                    localparam.Paged = true;
                    localparam.IsMVCCall = true;
                    localparam.Text = req.paramss.MoveRecords.TextFilter;
                    qry.FillData(localparam);
                    var lst = new List<MoveradioBox>();
                    foreach (DataRow row in localparam.Data.Rows)
                        lst.Add(new MoveradioBox() { text = row["ItemName"].ToString(), value = row["pkey"].ToString() });
                    model.Data = lst;
                }
                catch (Exception ex)
                {
                    DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                }

                return this.Json(model);
            }
            [HttpPost]
            public IActionResult BtnMoveItems([FromBody] BtnMoveItemsReqModel req)
            {
                if (!passport.CheckPermission(req.paramsUI.TableName, SecureObject.SecureObjectType.Table, Permissions.Permission.Move))
                    return null;

                var @params = new Parameters(req.paramsUI.ViewId, passport);
                var parentParams = new Parameters(req.paramsUI.MoveRecords.MoveViewid, passport);
                var model = new Moving();

                try
                {
                    using (var conn = passport.Connection())
                    {
                        string currentValue;
                        foreach (string tableId in req.paramsUI.ids)
                        {
                            string sql = string.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = @tableId", Navigation.MakeSimpleField(req.paramsUI.MoveRecords.fieldName), @params.TableName, MakeSimpleField(@params.KeyField));

                            using (var cmd = new SqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@tableId", tableId);

                                try
                                {
                                    currentValue = cmd.ExecuteScalar().ToString();
                                }
                                catch (Exception)
                            {
                                    currentValue = string.Empty;
                                }
                            }

                            Navigation.UpdateSingleField(@params.TableName, tableId, req.paramsUI.MoveRecords.fieldName, req.paramsUI.MoveRecords.fieldItemValue, conn);

                            if (CBoolean(@params.TableInfo["AuditUpdate"]))
                            {
                                {
                                    var withBlock = AuditType.WebAccess;
                                    withBlock.TableName = @params.TableName;
                                    withBlock.TableId = tableId;
                                    withBlock.ClientIpAddress = Keys.GetClientIpAddress(httpContext);
                                    withBlock.ActionType = AuditType.WebAccessActionType.MoveRecord;
                                    withBlock.AfterData = string.Format("{0}: {1}", Navigation.MakeSimpleField(req.paramsUI.MoveRecords.fieldName), req.paramsUI.MoveRecords.fieldItemValue);
                                    if (!string.IsNullOrWhiteSpace(currentValue))
                                        withBlock.BeforeData = string.Format("{0}: {1}", Navigation.MakeSimpleField(req.paramsUI.MoveRecords.fieldName), currentValue);
                                }

                                string action = string.Format("Move Record: {0}: {1} from {2}: {3} to {2}: {4}", @params.TableInfo["UserName"].ToString(), tableId, parentParams.TableInfo["UserName"].ToString(), currentValue, req.paramsUI.MoveRecords.fieldItemValue);
                                Auditing.AuditUpdates(AuditType.WebAccess, action, passport);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                }
                return this.Json(model);
            }

            private List<string> LoadItemsTobeMove(UserInterfaceJsonModel @params)
            {
                var lst = new List<string>();
                foreach (var id in @params.ids)
                    lst.Add(Navigation.GetItemName(@params.TableName, id, passport));
                return lst;
            }

            private void LoadDestinationTables(UserInterfaceJsonModel @params, Moving model)
            {
                foreach (DataRow item in Navigation.GetUpperRelationships(@params.TableName, passport).Rows)
                {
                    model.rbDestinationTable.Add(item[0].ToString());
                    var lst = new List<MoveradioBox>();
                    foreach (ViewItem View in GetViewsByTableName(item[0].ToString(), passport))
                        lst.Add(new MoveradioBox() { text = View.ViewName, viewId = View.Id, value = item[2].ToString(), selected = false });
                    model.rbDestinationViews.Add(lst);
                }
            }

            private List<MoveradioBox> LoadDestinationItems(int Moveviewid)
            {
                return DestinationItemsList(Moveviewid);
            }

            private List<MoveradioBox> DestinationItemsList(int Moveviewid)
            {
                var qry = new Query(passport);
                var @params = new Parameters(Moveviewid, passport);
                @params.QueryType = queryTypeEnum.OpenTable;
                @params.Paged = true;
                @params.IsMVCCall = true;
                qry.FillData(@params);
                var lst = new List<MoveradioBox>();
                foreach (DataRow row in @params.Data.Rows)
                    lst.Add(new MoveradioBox() { text = row["ItemName"].ToString(), value = row["pkey"].ToString() });
                return lst;
            }

            // not in use. Attachment will be another feature in the future. (approved by Evan)
            private List<MoveradioBox> LoadAttachments(UserInterfaceJsonModel @params)
            {
                var dt = Navigation.GetAttachments(@params.TableName, @params.ids[0].ToString(), passport);
                var lst = new List<MoveradioBox>();
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow item in dt.Rows)
                        lst.Add(new MoveradioBox() { text = string.Format(Languages.get_Translation("msgMoveControlAddAttachment"), item["AttachmentNumber"].ToString()), value = item["AttachmentNumber"].ToString(), selected = false });
                }
                return lst;
            }
        }
        public class MoveradioBoxModel : BaseModel
        {
            public MoveradioBoxModel()
            {
                Data = new List<MoveradioBox>();
            }
            public List<MoveradioBox> Data { get; set; } = new List<MoveradioBox>();
        }
    public class Moving : BaseModel
    {
        public Moving()
        {
            rbDestinationsItem = new List<MoveradioBox>();
            rbDestinationTable = new List<string>();
            rbDestinationViews = new List<List<MoveradioBox>>();
            itemsTobeMove = new List<string>();
        }

        public List<MoveradioBox> rbDestinationsItem { get; set; }
        public List<string> rbDestinationTable { get; set; }
        public List<List<MoveradioBox>> rbDestinationViews { get; set; }
        public List<string> itemsTobeMove { get; set; }
        public string moveView { get; set; }
    }

    public class MoveradioBox
    {
        public string value { get; set; }
        public string text { get; set; }
        public bool selected { get; set; }
        public string ViewName { get; set; }
        public int viewId { get; set; }
        public string upperTable { get; set; }
    }
}

