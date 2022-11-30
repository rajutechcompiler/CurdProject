using Microsoft.AspNetCore.Mvc;
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

namespace TabFusionRMS.WebCS.Controllers
{
    public class LogoutController : Controller
    {
        private IHttpContextAccessor accessor { get; set; }
        public LogoutController(IHttpContextAccessor httpContext)
        {
            accessor = httpContext;
        }
        public IActionResult Index()
        {
            // Session.Abandon()
            accessor.HttpContext.Session.Remove("passport");
            // get Api request
            var apireq = ContextService.GetSessionValueByKey("ApiRequest", accessor.HttpContext);

            // written for web api moti mashiah
            //ContextService.CreateCookies("WebApi", apireq, https: false, expireDateTime: DateTime.Now.AddSeconds(60));

            // end
            string userName = ContextService.GetSessionValueByKey("UserName", accessor.HttpContext);
            var adModel = ContextService.GetSessionValueByKey("azureADModelSesssion",accessor.HttpContext) != null ? ContextService.GetObjectFromJson<AzureADModel>("azureADModelSesssion", accessor.HttpContext) : null;
            var adToken = string.Empty;
            if (ContextService.GetSessionValueByKey("authToken", accessor.HttpContext) != null)
                adToken = ContextService.GetSessionValueByKey("authToken", accessor.HttpContext).ToString();
            //accessor.HttpContext.Session.Clear();
            if (!string.IsNullOrEmpty(userName))
                ContextService.SetSessionValue("authToken", userName, accessor.HttpContext);

            // remove cookies value 
            ContextService.RemoveCookie("Islogin", accessor.HttpContext);

            ContextService.CreateCookies("Islogin", false.ToString(), accessor.HttpContext);

            if (adModel != null)
            {
                if (adModel.ADEnabled != null && string.IsNullOrEmpty(adToken))
                {
                    string redirectURL = (accessor.HttpContext.Request.IsHttps ? "https://" : "http://") + accessor.HttpContext.Request.Host + "/Login/Index?out=1";
                    var signOutURL = string.Format(AzureADSettings.SignOutURL, AzureADSettings.MultiTenantId, redirectURL);
                    Response.Redirect(signOutURL);
                }
            }
            else
                Response.Redirect("Login/Index");

            return View();
        }
    }
}
