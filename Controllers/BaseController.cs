using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Smead.RecordsManagement;
using System.Data;
using System.Data.SqlClient;
using Smead.Security;
using Newtonsoft.Json;
using TabFusionRMS.RepositoryVB;
using Microsoft.AspNetCore.Mvc.Filters;
using TabFusionRMS.Resource;
using System.Diagnostics;

namespace TabFusionRMS.WebCS.Controllers
{
    [TypeFilter(typeof(OnExceptionFilter))]
    public class BaseController : Controller
    {
        public HttpContext httpContext { get; set; }
        IHttpContextAccessor httpContextAccessor;
       
        public BaseController()
        {
            httpContextAccessor = new HttpContextAccessor();
            httpContext = httpContextAccessor?.HttpContext;
           
            passport = ContextService.GetObjectFromJson<Passport>("passport", httpContext);
            if (passport == null)
            {
                httpContext.Response.Redirect("Login/Index");
                return;
            }
            else
            {
                var strConn = passport.ConnectionString;
                if (!strConn.Contains("MultipleActiveResultSets"))
                {
                    strConn = strConn + ";MultipleActiveResultSets=True";
                }
                RepositoryKeys.StrConnection = strConn;
            }
            
            //SetMetaTag();
        }
        public Passport passport { get; init; }
        public List<string> GetAllDataIds(int viewid)
        {
            var pr = new Parameters(viewid, passport);
            var lst = new List<string>();
            string query = ContextService.GetSessionValueByKey("HoldTotalRowQuery", httpContext);
            string command = string.Format("SELECT [{0}] {1}", pr.KeyField, Strings.Right(query, query.Length - Strings.InStr(query, " FROM ", CompareMethod.Text)));
            var dt = new DataTable();

            if (passport != null)
            {
                using (var cmd = new SqlCommand(command, passport.Connection()))
                {
                    var adp = new SqlDataAdapter();
                    adp.SelectCommand = cmd;
                    adp.Fill(dt);
                }
            }

            foreach (DataRow row in dt.Rows)
                lst.Add(row[pr.KeyField].ToString());

            return lst;
        }

        public void SetMetaTag()
        {
            if (httpContext.Session is not null)
            {
                string orgurl = "";
                //check moti (where do you want to redirect) data.aspx does not exist
                //if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("retention") | HttpContext.Current.Request.Url.ToString().ToLower().Contains("scanner") | HttpContext.Current.Request.Url.ToString().ToLower().Contains("import"))
                //{
                //    orgurl = "../data.aspx";
                //}
                //else
                //{
                //    orgurl = "data.aspx";
                //}
                if (string.IsNullOrEmpty(ContextService.GetSessionValueByKey("Passport", httpContext)))
                {
                    if (passport.AutoSignOutSeconds() == 0)
                    {
                        // check raju (how to reset timeout in asp.net core)
                        //HttpContext.Session..Timeout = 1440;
                        ViewBag.TimeOutSecounds = 1440 * 60;
                    }
                    else
                    {
                        int AutoSignOutSeconds = passport.AutoSignOutSeconds();
                        // check raju (how to reset timeout in asp.net core)
                        //HttpContext.Current.Session.Timeout = AutoSignOutSeconds / 60d;
                        ViewBag.TimeOutSecounds = passport.AutoSignOutSeconds();
                    } // changed by GANESH
                    ViewBag.AutoRedirectURL = string.Format("{0}?timeout=1", orgurl);
                }
            }

        }
        public ActionResult GetRefereshDetails(string strPageName)
        {

            return Json(Keys.get_RefereshDetails(strPageName, passport, httpContext));
        }
        public new RedirectToRouteResult RedirectToAction(string action, string controller)
        {
            return RedirectToAction(action, controller);
        }
        // in vb code this function name is "BeginExecuteCore"
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var pCurrentCulture = Keys.GetCultureCookies(httpContext);
            Thread.CurrentThread.CurrentCulture = pCurrentCulture;
            Thread.CurrentThread.CurrentUICulture = pCurrentCulture;
            if (passport == null)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Login",
                    action = "Index"
                }));
            }
        }
    }

    public class OnExceptionFilter : IExceptionFilter
    {
        private readonly IHostEnvironment _hostEnvironment;
        public HttpContext httpContext { get; set; }

        public OnExceptionFilter(IHostEnvironment hostEnvironment) =>
            _hostEnvironment = hostEnvironment;

        public void OnException(ExceptionContext context)
        {
            var httpContextAccessor = new HttpContextAccessor();
            httpContext = httpContextAccessor?.HttpContext;

            var passport = ContextService.GetObjectFromJson<Passport>("passport", httpContext);
            
            if (context.ExceptionHandled)
            {
                context.ExceptionHandled = true;
            }
            if (passport == null)
            {
                httpContext.Response.Redirect("/");
            }
        }
    }
}
