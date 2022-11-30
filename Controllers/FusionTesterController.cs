using System.Data.SqlClient;
using Smead.RecordsManagement;
using Smead.Security;
using Microsoft.AspNetCore.Mvc;
using TabFusionRMS.WebCS.FusionElevenModels;
using System.Data;
namespace TabFusionRMS.WebCS.Controllers
{
    public partial class FusionTesterController : BaseController
    {

        //public FusionTesterController(IHttpContextAccessor httpContext) : base(httpContext)
        //{ }

        // GET: FusionTester
        [HttpPost]
        public async Task<JsonResult> RunQuery([FromBody]searchQueryModel.Searchparams @params)
        {
            // create grid object      lookonetime
            var tst = new FusionTesterModel();
            string password = "@FusionTab$TesterAccess.com";
            if (!(password == @params.password))
            {
                tst.messages = "badpass";
                return new JsonResult(tst);
            }
            GridDataBinding model = default;
            try
            {
                model = new GridDataBinding(passport, @params.ViewId, @params.pageNum, @params.crumbLevel, (int)ViewType.FusionView, httpContext);
                model.ExecuteGridData();
                var asdf = model.@params.TableInfo["AuditUpdate"].ToString();
                Getgrid(model, tst);
                tst.RelationshipTable = GetTableRelationshit(model.TableName);
                tst.AuditOnOff = Convert.ToBoolean(model.@params.TableInfo["AuditUpdate"].ToString());
                tst.ListOfSettings = GetSettings();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, model.ViewId, passport.DatabaseName);
                tst.isError = model.isError;
                tst.TesterErrMsg = model.TesterErrMsg;
            }
            //var jsonresult = Json(tst);
            //jsonresult.MaxJsonLength = int.MaxValue;
            //return jsonresult;

            return new JsonResult(tst);
        }
        [HttpGet]
        public JsonResult DropDownViews(string UIpassword)
        {
            string password = "@FusionTab$TesterAccess.com";
            if (!((password ?? "") == (UIpassword ?? "")))
            {
                return Json("badpass");
            }
            var views = Navigation.GetAllUserViews(passport);
            return Json(views);
        }
        private List<Settings> GetSettings()
        {
            var lst = new List<Settings>();
            var dt = new DataTable();
            using (var cmd = new SqlCommand("select * from Settings", passport.Connection()))
            {
                var adp = new SqlDataAdapter(cmd);
                adp.Fill(dt);
            }
            foreach (DataRow row in dt.Rows)
                lst.Add(new Settings() { Section = row["Section"].ToString(), Item = row["Item"].ToString(), ItemValue = row["ItemValue"].ToString() });
            return lst;
        }
        private List<RelationshipTable> GetTableRelationshit(string tableName)
        {
            var lst = new List<RelationshipTable>();
            var dt = new DataTable();
            using (var cmd = new SqlCommand("select * from RelationShips where UpperTableName = @tablename or LowerTableName = @tablename", passport.Connection()))
            {
                cmd.Parameters.AddWithValue("@tablename", tableName);
                var adp = new SqlDataAdapter(cmd);
                adp.Fill(dt);
            }
            foreach (DataRow row in dt.Rows)
                lst.Add(new RelationshipTable()
                {
                    LowerTableFieldName = row["LowerTableFieldName"].ToString(),
                    LowerTableName = row["LowerTableName"].ToString(),
                    UpperTableFieldName = row["UpperTableFieldName"].ToString(),
                    UpperTableName = row["UpperTableName"].ToString()
                });
            return lst;
        }
        private void Getgrid(GridDataBinding model, FusionTesterModel tst)
        {
            tst.ListOfDatarows = model.ListOfDatarows;
            tst.ListOfHeaders = model.ListOfHeaders;
            tst.HasAttachmentcolumn = model.HasAttachmentcolumn;
            tst.HasDrillDowncolumn = model.HasDrillDowncolumn;
            tst.ShowTrackableTable = model.ShowTrackableTable;
            tst.TableName = model.TableName;
            tst.ViewId = model.ViewId;
            tst.ViewName = model.ViewName;
        }
    }


}