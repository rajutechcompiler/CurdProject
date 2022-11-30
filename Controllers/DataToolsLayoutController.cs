using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Smead.RecordsManagement;
using Smead.Security;
using TabFusionRMS.Resource;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using Newtonsoft.Json;

namespace TabFusionRMS.WebCS.Controllers
{
    public partial class DataToolsLayoutController : BaseController
    {

        //public DataToolsLayoutController(IHttpContextAccessor httpContext) : base(httpContext)
        //{ }

        // GET: DataToolsLayout
        private IRepository<LookupType> _iLookupType = new Repositories<LookupType>();
        //public LevelManager LevelManager
        //{
        //    get
        //    {
                
        //       return  ContextService.GetObjectFromJson<LevelManager>("LevelManager", httpContext);
        //    }
        //}
        public enum ids
        {
            None,
            Password,
            Index,
            RequestDetails,
            BatchRequest,
            Tools,
            FinalDisposition,
            Localize
        }
        [HttpGet]
        public ActionResult LoadControls(int id)
        {
            var model = new DataToolsLayoutModel();
            try
            {
                switch (id)
                {
                    case (int)ids.Password:
                        {
                            model.Title = Languages.get_Translation("ChangePassword");
                            return PartialView("_password", model);
                        }
                    case (int)ids.Localize:
                        {
                            model.Title = Languages.get_Translation("lnkToolsLanguageAndRegion");
                            showLocalization(ref model);
                            return PartialView("_localization", model);
                        }
                    case (int)ids.Index:
                        {
                            model.Title = Languages.get_Translation("tiDialogBoxIndexAttachment");
                            break;
                        }
                    case (int)ids.RequestDetails:
                        {
                            model.Title = Languages.get_Translation("RequestDetails");
                            break;
                        }
                    case (int)ids.BatchRequest:
                        {
                            model.Title = Languages.get_Translation("BatchRequest");
                            return PartialView("_batchRequest", model);
                        }
                    case (int)ids.Tools:  // Done
                        {
                            model.Title = Languages.get_Translation("tiDialogBoxTools");
                            return PartialView("_tools", model);
                        }
                }
            }
            catch (Exception)
            {

            }

            return PartialView("", model);
        }
        private void showLocalization(ref DataToolsLayoutModel model)
        {
            try
            {
                var pDateForm = _iLookupType.All().Where(m => m.LookupTypeForCode.Trim().ToUpper().Equals("DTFRM".Trim().ToUpper())).OrderBy(m => m.SortOrder).ToList();
                // ' Fill languages dropdown
                var resouceObject = new Dictionary<string, string>();
                resouceObject = Languages.GetXMLLanguages();
                string pLocData = "0";
                if (pDateForm is not null)
                {
                    var pLookupType = pDateForm.Where(x => x.LookupTypeValue.Trim().ToLower().Equals(Keys.GetUserPreferences.sPreferedDateFormat.ToString().Trim().ToLower())).FirstOrDefault();
                    if (pLookupType is not null)
                    {
                        pLocData = pLookupType.LookupTypeCode;
                    }
                }

                var data = ContextService.FromLegacyCookieString(httpContext.Request.Cookies[Keys.CurrentUserName]);

                var strCulture = data["PreferedLanguage"].ToString();
                model.ListLocalization.Add(new Localizations(Languages.get_Translation("lblLocalizePartialChooseDateFormat"), "-1"));
                foreach (var item in pDateForm)
                    model.ListLocalization.Add(new Localizations(item.LookupTypeValue, item.LookupTypeCode));


                var res = JsonConvert.SerializeObject(resouceObject);
                model.objLocalization.pLocData = pLocData;
                model.objLocalization.resouceObjectLenguage = res;
                model.objLocalization.SelectedCountry = strCulture.ToString();
            }
            catch (Exception)
            {

            }
        }
        // Dialog Events
        [HttpPost]
        public JsonResult ChangePassword_click(ChangePassword model)
        {
            model.errorMessage = "0";
            try
            {
                if (string.Compare(model.OldPass, model.NewPass1) == 0)
                {
                    throw new Exception(Languages.get_Translation("msgErrorMsgPasswordChange"));
                }
                var user = new Smead.Security.User(passport, true);
                user.ChangePassword(model.OldPass, model.NewPass1, model.NewPass2);
                ContextService.SetSessionValue("MustChangePassword", "false", httpContext);
                
                ContextService.RemoveCookie("mustChangePassword", httpContext);
                ContextService.CreateCookies("mustChangePassword", false.ToString(), httpContext, 180d);
            }
            catch (Exception ex)
            {
                model.errorMessage = ex.Message;
            }
            return Json(model);
        }
        [HttpPost]
        public JsonResult btnSaveLocData_Click(Localizations model)
        {
            bool isError = true;
            try
            {
                string pPreferedLanguage;
                if (model.LanguageSelected == "")
                {
                    pPreferedLanguage = Keys.GetCultureCookies(httpContext).ToString(); // GetCultureCookies().Name;
                }
                else
                {
                    pPreferedLanguage = model.LanguageSelected;
                }
                var pLookupType = _iLookupType.All().Where(m => m.LookupTypeForCode.Trim().ToUpper().Equals("DTFRM".Trim().ToUpper()) && m.LookupTypeCode.Equals(model.dateFormatSelected)).FirstOrDefault();

                IDictionary<string, string> cookiDic = new Dictionary<string, string>();

                if (ContextService.GetCookieVal(Keys.CurrentUserName.ToString(), httpContext) is not null)
                {
                    cookiDic = ContextService.FromLegacyCookieString(ContextService.GetCookieVal(Keys.CurrentUserName.ToString(), httpContext));//this.Request.Cookies[Keys.CurrentUserName.ToString()];
                    ContextService.RemoveCookie(Keys.CurrentUserName.ToString(), httpContext);
                }

                cookiDic.Remove("PreferedLanguage");
                cookiDic.Remove("PreferedDateFormat");

                if (pPreferedLanguage is not null)
                    cookiDic.Add("PreferedLanguage", pPreferedLanguage);
                else
                    cookiDic.Add("PreferedLanguage", "en-US");

                if (!(model.dateFormatSelected == "0"))
                    cookiDic.Add("PreferedDateFormat", pLookupType.LookupTypeValue);
                else
                    cookiDic.Add("PreferedDateFormat", Keys.GetCultureCookies(httpContext).DateTimeFormat.ShortDatePattern);

                ContextService.RemoveCookie(Keys.CurrentUserName.ToString(), httpContext);
                ContextService.CreateCookies(Keys.CurrentUserName.ToString(), cookiDic, httpContext, expire: 180d);
            }
            catch (Exception)
            {
                isError = false;
            }
            return Json(isError);
        }

    }
}