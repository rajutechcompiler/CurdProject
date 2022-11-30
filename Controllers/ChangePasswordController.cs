using Microsoft.AspNetCore.Mvc;
using Smead.Security;
using TabFusionRMS.Resource;

namespace TabFusionRMS.WebCS.Controllers
{
    public class ChangePasswordController : Controller
    {
        private Passport passport { get; set; }
        public ChangePasswordController()
        {
            var httpContext = new HttpContextAccessor().HttpContext;
            passport = ContextService.GetObjectFromJson<Passport>("passport", httpContext);
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UpdatePass([FromBody] ChangePassportReq req)
        {
            try
            {
                if (string.IsNullOrEmpty((req.txtOldPass ?? "").Trim())  || 
                    string.IsNullOrEmpty((req.txtNewPass1 ?? "").Trim()) || 
                    string.IsNullOrEmpty((req.txtNewPass2 ?? "").Trim()))
                {
                    return Json(new ChangePassportRes { Msg = "Fill all mandatory fields." });
                }

                if (string.Compare(req.txtOldPass, req.txtNewPass1) == 0)
                {
                    return Json(new ChangePassportRes { Msg = Languages.get_Translation("msgErrorMsgPasswordChange") });
                }

                var user = new Smead.Security.User(passport, true);
                user.ChangePassword(req.txtOldPass, req.txtNewPass1, req.txtNewPass2);
                ContextService.SetSessionValue("mustChangePassword", false.ToString(), HttpContext);
                ContextService.CreateCookies("mustChangePassword", false.ToString(), HttpContext);

                return Json(new ChangePassportRes { Msg = Languages.get_Translation("msgErrorMsgPasswordChange"), isSuccess = true, redirect = "data" });
            }
            catch(Exception ex)
            {
                return Json(new ChangePassportRes { Msg = "Something went wrong", isError = true, });
            }
        }
    }

    public class ChangePassportReq
    {
        public string txtOldPass { get; set; } = "";
        public string txtNewPass1 { get; set; } = "";
        public string txtNewPass2 { get; set; } = "";
    }

    public class ChangePassportRes : BaseModel
    {
        public bool isSuccess { get; set; } = false;
        public string redirect { get; set; } = "";
    }
}
