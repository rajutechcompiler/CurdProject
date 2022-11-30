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
using TabFusionRMS.ContextHelp;

namespace TabFusionRMS.WebCS.Controllers
{
    public class LogoutController1 : Controller
    {
        public IActionResult Index()
        {
            // Session.Abandon()
            ContextService.httpContext.Session.Remove("Passport");
            // get Api request
            var apireq = ContextService.GetSessionValueByKey("ApiRequest");

            // written for web api moti mashiah
            //ContextService.CreateCookies("WebApi", apireq, https: false, expireDateTime: DateTime.Now.AddSeconds(60));

            // end
            string userName = ContextService.GetSessionValueByKey("UserName");
            var adModel = ContextService.GetSessionValueByKey("azureADModelSesssion") != null ? ContextService.GetObjectFromJson<AzureADModel>("azureADModelSesssion") : null;
            var adToken = string.Empty;
            if (ContextService.GetSessionValueByKey("authToken") != null)
                adToken = ContextService.GetSessionValueByKey("authToken").ToString();
            // check raju
            //System.Web.UI.Page.Session.Clear();
            if (!string.IsNullOrEmpty(userName))
                ContextService.SetSessionValue("authToken", userName);

            // remove cookies value 
            ContextService.RemoveCookie("Islogin");

            ContextService.CreateCookies("Islogin", false.ToString());

            //if (adModel != null)
            //{
            //    if(adModel.ADEnabled  !=null && string.IsNullOrEmpty(adToken))
            //    {
            //        string redirectURL = System.Web.UI.Page.Request.Url.GetLeftPart(UriPartial.Authority) + System.Web.UI.Page.Response.ApplyAppPathModifier("~/signin.aspx?out=1");
            //        var signOutURL = string.Format(AzureADSettings.SignOutURL, AzureADSettings.MultiTenantId, redirectURL);
            //        Response.Redirect(signOutURL);
            //    }
            //}
            //else
                Response.Redirect("Login/Index");

            return View();
        }
    }
}
