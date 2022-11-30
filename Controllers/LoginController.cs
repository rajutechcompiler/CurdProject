using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Smead.RecordsManagement;
using System.Xml;
using TabFusionRMS.Resource;
using Smead.Security;
using Newtonsoft.Json;
using System.Text;
using System.Reflection;
using System.Net;

namespace TabFusionRMS.WebCS.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpContext hc;
        private readonly IConfiguration _configuration;
        public LoginController(IWebHostEnvironment env, IHttpContextAccessor httpContext, IConfiguration configuration)
        {
            hc = httpContext.HttpContext;
            Languages.ContentRootPath = env.ContentRootPath;
            _configuration = configuration;
        }


        public IActionResult Index([FromServices] IWebHostEnvironment env)
        {
            if (hc.Request.Query["ad"].Count > 0)
            {
                if (hc.Request.Query["ad"][0] == "1" && !string.IsNullOrEmpty(hc.Session.GetString("authToken")))
                {
                    // check raju
                    //SignIn();
                }
            }

            string userName = hc.Session.GetString("UserName");

            if (string.IsNullOrWhiteSpace(userName) && hc.Request.Cookies["lastUsername"] is not null)
                hc.Session.SetString("UserName", hc.Request.Cookies["lastUsername"].ToString());

            string fileName = null;
            if (hc.Request.Query["fileName"].Count > 0)
                fileName = hc.Request.Query["fileName"][0];

            if (fileName == null)
            {
                if (hc.Session.Get("passport") is not null)
                {
                    return Redirect("~/data");
                }
            }

            Keys.LastLoginAttempt = null;
            //--Start Login warning message :Added by Nikunj Patel--------------------------------
            string showMessage = string.Empty;
            string path = System.IO.Path.Combine(Common.GetBaseFolder(), @"ImportFiles\WarningMessageXML.xml");

            if (System.IO.File.Exists(path))
            {
                XmlReader document = new XmlTextReader(path);
                while (document.Read())
                {
                    var type = document.NodeType;
                    if (type == XmlNodeType.Element)
                    {
                        if (document.Name == "ShowMessage")
                            showMessage = document.ReadInnerXml().ToString();
                    }
                }
                document.Close();
            }

            var goLoging = Convert.ToBoolean(hc.Session.GetString("goLogin"));

            if (showMessage == "Yes" & goLoging != true)
                Response.Redirect("SignInMessage.aspx");
            //--End Login warning message :Added by Nikunj Patel--------------------------------

            ProcessDirectSignOut();
            LoadLastUserName();
            DrawLastSignInInfoText();

            if (Convert.ToBoolean(_configuration["MySettings:IsCloud"]))
            {
                ViewBag.panCloud = false;
                ViewBag.panCustomer = true;
                LoadFakeCustomerKey();
                //TryCloudAutoSignIn();
            }
            else
            {
                var Databaselist = LoadDatabases(new slimShared.CSlimAPI("iAccess", true));
                ViewBag.Database = Databaselist;
                ViewBag.panCloud = true;
                ViewBag.panCustomer = false;

                var selectedDb = Databaselist.Where(x => x.Selected == true).FirstOrDefault();
                AzureADModel azureADModel = null;
                string ADEnabled = null;

                if (selectedDb !=null)
                {
                    azureADModel = LoadADSettings(Convert.ToInt32(selectedDb.Value), selectedDb.Text);
                }
                
                if (azureADModel != null)
                {
                    if(!string.IsNullOrEmpty(azureADModel.ADEnabled))
                    {
                        ADEnabled = azureADModel.ADEnabled;
                        VisibleCtrBasedOnADSettings(Convert.ToBoolean(azureADModel.ADEnabled));
                    }
                    else
                    {
                        VisibleCtrBasedOnADSettings(false);
                        TryAutoSignIn(selectedDb.Text);
                    }
                }
                else
                {
                    VisibleCtrBasedOnADSettings(false);
                    if (selectedDb != null)
                    {
                        TryAutoSignIn(selectedDb.Text);
                    }
                }
               
                //if(azureADModel !=null && ADEnabled !=null)
                //{
                //    VisibleCtrBasedOnADSettings(false);
                //    if(selectedDb !=null)
                //    {
                //        TryAutoSignIn(selectedDb.Text);
                //    }
                //}
            }

            return View();
        }
        [HttpPost]
        public IActionResult SignInUser(string ddlDatabase, string txtUserName, string txtPassword, 
                                        string txtCustomerKey, string chkSaveAuth, string hdnSSOEnabled, string hdnSSODBSelected)
        {
            // check raju IMP Moti discuss
            //If btnSignIn.CommandArgument.Equals("LOGIN") Then
            if (ddlDatabase != "")
            {
                if (ddlDatabase != "-1")
                {
                    if (hc.Request.Query["fileName"].Count > 0)
                    {
                        hc.Session.SetString("fileName", hc.Request.Query["fileName"][0]);
                    }
                    else
                    {
                        hc.Session.Remove("fileName");
                    }

                    hc.Session.SetString("WorkGroup", "");

                    if(hdnSSODBSelected.IntValue() == 1)
                    {
                        // check raju (Page.User.Identity.IsAuthenticated)
                        //        If Page.User.Identity.IsAuthenticated Then
                        //            txtUserName.Text = Page.User.Identity.Name.Substring(Page.User.Identity.Name.IndexOf("\") + 1)
                        //            txtPassword.Text = "3kszs932ksdjjdjwqp00qkksj"
                        //        End If
                    }

                    var checkbox = chkSaveAuth == "on" ? true : false;

                    var res = SignIn(txtCustomerKey,txtUserName,txtPassword, checkbox, ddlDatabase).Result;

                    if(Navigation.CBoolean(ContextService.GetCookieVal("Islogin", hc)))
                    {
                        try
                        {
                            if(!string.IsNullOrWhiteSpace(ContextService.GetCookieVal("WebApi", hc)))
                            {
                                return Redirect(ContextService.GetCookieVal("WebApi", hc));
                            }
                        }
                        catch(Exception)
                        {
                            //do nothing if there is no cookie. just continue to login
                        }
                    }

                    if(res)
                    {
                        if (!string.IsNullOrEmpty(ContextService.GetSessionValueByKey("mustChangePassword", hc)))
                        {
                            bool changePass = Convert.ToBoolean(ContextService.GetSessionValueByKey("mustChangePassword", hc));
                            if (changePass)
                            {
                                ContextService.CreateCookies("mustChangePassword", changePass.ToString(), hc);
                                return Redirect("~/ChangePassword");
                            }
                        }

                        if (!string.IsNullOrEmpty(ContextService.GetSessionValueByKey("FileName", hc)))
                            return Redirect("~/admin");

                        return Redirect("~/data");
                    }
                    else
                    {
                        
                        return Redirect("~/Login");
                    }
                }
                else
                {
                    TempData["lblError"] = Languages.get_Translation("msgSignInPlzSelDataBase");
                    ContextService.RemoveCookie("lastDatabaseIndex", hc);
                    return Redirect("~/Login");
                }
            }
            else
            {
                if (hc.Request.Query["fileName"].Count > 0)
                {
                    hc.Session.SetString("fileName", hc.Request.Query["fileName"][0]);
                }
                else
                {
                    hc.Session.Remove("fileName");
                }

                hc.Session.SetString("WorkGroup", "");
                SignIn(txtCustomerKey, txtUserName, txtPassword, Convert.ToBoolean(chkSaveAuth), ddlDatabase);
                return Redirect("~/Login");
            }
        }

        private void TryAutoSignIn(string SelectedValue)
        {
            if( ! string.IsNullOrEmpty(SelectedValue) && hc.Request.Query["out"].Count == 0 )
            {
                var lastPass = ContextService.GetCookieVal("lastPassword", hc);

                if (!string.IsNullOrEmpty(lastPass))
                {
                    ContextService.RemoveCookie("lastPassword", hc);
                    ContextService.CreateCookies("lastPassword", lastPass, hc, expire: 10d);
                    ViewBag.txtPassword = System.Net.WebUtility.HtmlEncode(Encrypt.IPDecrypt(lastPass, GetUserIP()));
                }
            }
        }
        
        private void VisibleCtrBasedOnADSettings(bool IsAdEnabled)
        {
           if(IsAdEnabled)
                {
                ViewBag.usernameidVisible = false;
                ViewBag.passwordidVisible = false;
                ViewBag.txtUserNameText = String.Empty;
                // check Moti CommandArgument
                ViewBag.btnSignInCommandArgument = "ADLOGIN";
            }
            else
                {
                ViewBag.usernameidVisible = true;
                ViewBag.passwordidVisible = true;
                // check Moti CommandArgument
                ViewBag.btnSignInCommandArgument = "LOGIN";
            }
        }

        private void ProcessDirectSignOut()
        {
            if (hc.Request.Query.Count > 0)
            {
                if (hc.Request.Query["out"][0] == "1")
                {
                    ContextService.SetCookieNull("lastPassword", hc);
                    LoadFakeCustomerKey();
                }
            }
        }
        public void LoadLastUserName()
        {
            if (ContextService.GetCookieVal("lastUsername", hc) is not null)
            {
                // set lastusername to html
                ViewBag.txtUserName = System.Net.WebUtility.HtmlEncode(ContextService.GetCookieVal("lastUsername", hc));
            }
        }

        private void DrawLastSignInInfoText()
        {
            var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            ViewBag.lblService = String.Format(Languages.get_Translation("lblDbVersion"), version??"".ToString().Trim());

            if(ContextService.GetCookieVal("lastAttempt", hc) != null & ContextService.GetCookieVal("lastAttemptUserName", hc) != null)
            {
                string lastDate = "";
                DateTime lastDateAndTime;
                try
                {
                    lastDateAndTime = DateTime.Parse(System.Net.WebUtility.HtmlEncode(ContextService.GetCookieVal("lastAttempt", hc)));
                    lastDate = lastDateAndTime.ToClientDateFormat();
                }
                catch(Exception)
                {
                    lastDate = "";
                }

                Keys.LastLoginUser = System.Net.WebUtility.HtmlEncode(ContextService.GetCookieVal("lastAttemptUserName", hc));

                if(string.IsNullOrEmpty(lastDate))
                {
                    ViewBag.lblAttempt = string.Format(Languages.get_Translation("lblSignInLastSignInAttemptBy"), System.Net.WebUtility.HtmlEncode(ContextService.GetCookieVal("lastAttemptUserName", hc)));
                }
                else
                {
                    ViewBag.lblAttempt = string.Format(Languages.get_Translation("lblSignInLastSignInAttemptOn"), lastDate, System.Net.WebUtility.HtmlEncode(ContextService.GetCookieVal("lastAttemptUserName", hc)));
                }
            }

        }
        public void LoadFakeCustomerKey()
        {
            if (!string.IsNullOrEmpty(hc.Request.Cookies["CustomerKey"]))
            {
                ViewBag.txtCustomerKey = hc.Request.Cookies["CustomerKey"]??"".ToString();
            }
        }

        private void TryCloudAutoSignIn()
        {
            if (hc.Request.Query["out"].Count > 0)
            {
                if (string.IsNullOrEmpty(ContextService.GetCookieVal("CustomerKey", hc))) return;

                if (!string.IsNullOrEmpty(ContextService.GetCookieVal("lastPassword", hc))) return;

                ViewBag.txtPassword = System.Net.WebUtility.HtmlEncode(Encrypt.AesDecrypt(ContextService.GetCookieVal("lastPassword", hc)));
                // cheeck raju
                //SignIn();
            }
        }

        private AzureADModel LoadADSettings(int SelectedIndex, string SelectedText)
        {
            if (SelectedIndex > 0)
            {
                var azureADService = new AzureADService();
                var azureADModel = azureADService.GetAzureADSettings(hc, SelectedText);

                return azureADModel;
            }
            return null;
        }
        private List<DatabaseModel> LoadDatabases(slimShared.CSlimAPI Databases)
        {
            try
            {
                var Databaselist = new List<DatabaseModel>();
                Databaselist.Add(new DatabaseModel { Text = "Select Database", Value = "-1" });
                ViewBag.hdnSSODBSelected = "";

                for (int i = 0, loopTo = Databases.DatabaseCount - 1; i <= loopTo; i++)
                {
                    if (Databases.GetDatabase(i) is not null)
                    {
                        var Database = new DatabaseModel();
                        Database.Value = i.ToString();
                        Database.Text = Databases.GetDatabase(i, true).Name;
                        // SSO concept 
                        if (Databases.GetDatabase(i, true).WebAccessSSOEnabled)
                        {

                            Database.Text = Databases.GetDatabase(i, true).Name;

                            ContextService.RemoveCookie("hdnSSOEnabled", hc);
                            ContextService.CreateCookies("hdnSSOEnabled", "1", hc);

                            if (Databases.DatabaseCount == 1)
                                ViewBag.hdnSSODBSelected = "1";
                        }
                        else
                        {
                            Database.Text = Databases.GetDatabase(i, true).Name;
                            ContextService.RemoveCookie("hdnSSOEnabled", hc);
                            ContextService.CreateCookies("hdnSSOEnabled", "0", hc);

                            if (Databases.DatabaseCount == 1)
                                ViewBag.hdnSSODBSelected = "0";
                        }

                        if (ContextService.GetCookieVal("lastDatabaseIndex", hc) is not null)
                        {
                            if(ContextService.GetCookieVal("lastDatabaseIndex", hc).ToString() == i.ToString())
                            {
                                Database.Selected = true;
                            }
                        }
                        else
                        {
                            Database.Selected = false;
                        }

                        Databaselist.Add(Database);

                    }
                }

                // TODO When we upgrade to .net 4.7 or greater remove the if and just use samesite=strict
                string hdnsso = ContextService.GetCookieVal("hdnSSOEnabled", hc);
                ContextService.RemoveCookie("hdnSSOEnabled", hc);
                
                if(hdnsso !=null)
                {
                    if (hc.Request.IsHttps)
                        ContextService.CreateCookies("hdnSSOEnabled", hdnsso, hc, https: true, expire: 1d);
                    else
                        ContextService.CreateCookies("hdnSSOEnabled", hdnsso, hc, https: false, expire: 1d);
                }

                //if (ddlDatabase.SelectedIndex == 0 && ddlDatabase.Items.Count > 0)
                //    ddlDatabase.Items(0).Selected = true;

                return Databaselist;

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(Languages.get_Translation("msgUnableToConnSlim"), Constants.vbCrLf, ex.Message));
            }
        }

        private async Task<bool> SignIn(string txtCustomerKey, string txtUserName, string txtPassword, bool chkSaveAuth, string ddlDatabase)
        {
            ContextService.SetSessionValue("mustChangePassword", false.ToString(), hc);

            ContextService.CreateCookies("mustChangePassword", false.ToString(), hc);
            ContextService.CreateCookies("lastAttempt", DateTime.Now.ToString(), hc);

            var passport = new Passport();
            slimShared.CSlimAPI slim;

            try
            {
                slim = new slimShared.CSlimAPI("iAccess", true);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(Languages.get_Translation("msgUnableToConnSlim"), Constants.vbCrLf, ex.Message));
            }

            slimShared.CSLIMDatabase? db = default;

            if (Convert.ToBoolean(_configuration["MySettings:IsCloud"]))
            {
                string customerKey = string.Empty;

                try
                {
                    customerKey = ContextService.GetCookieVal("CustomerKey", hc).ToString();
                }
                catch (Exception)
                {
                    customerKey = string.Empty;
                }

                if (!string.IsNullOrEmpty(txtCustomerKey) && txtCustomerKey != "XXXXXXXXXX")
                {
                    ContextService.CreateCookies("CustomerKey", txtCustomerKey, hc);
                    customerKey = txtCustomerKey;
                }

                for (int i = 0; i <= slim.DatabaseCount - 1; i++)
                {
                    if (slim.GetDatabase(i) is not null)
                    {
                        if (customerKey == slim.GetDatabase(i, true).Name)
                        {
                            db = slim.GetDatabase(i);
                            break;
                        }
                    }
                }
            }
            else if (AzureADSettings.isADEnabled(hc))
            {
                try
                {
                    int ADDB = 0;
                    int databaseIndex = Convert.ToInt32(ContextService.GetCookieVal("lastDatabaseIndex", hc));
                    if (databaseIndex > 0)
                    {
                        ADDB = databaseIndex;
                    }
                    // txtUserName.Text = Context.User.Identity.Name
                    db = slim.GetDatabase(ADDB, true);
                }
                catch (InvalidCastException)
                {
                    db = slim.GetDatabase(0, true);
                }
            }
            else
            {
                try
                {
                    db = slim.GetDatabase(Convert.ToInt32(ddlDatabase), true);
                }
                catch (InvalidCastException)
                {
                    db = slim.GetDatabase(0, true);
                }
            }

            if (db == null)
            {
                if (Convert.ToBoolean(_configuration["MySettings:IsCloud"]))
                {
                    TempData["lblError"] = Languages.get_Translation("lblSignInInvalidCustKey");
                }
                else
                {
                    TempData["lblError"] = Languages.get_Translation("lblSignInProbToConnDBFrmSlim");
                }
                return false;
            }

            var userName = ContextService.GetSessionValueByKey("UserName", hc);

            string strUserName = Convert.ToString(AzureADSettings.isADEnabled(hc) ? userName: txtUserName);
            ContextService.CreateCookies("lastAttemptUserName", strUserName, hc);

            try
            {
                passport.SignIn(strUserName, txtPassword, String.Empty, db.Server, db.Database, db.UserName, db.Password);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                if (ex.Message.Contains("invalid object name 'secureuser'"))
                {
                    TempData["lblError"] = Languages.get_Translation("lblSignInIncompatibleDB");
                }
                else
                {
                    TempData["lblError"] = Languages.get_Translation("SystemError") + ": " + ex.Message;
                }
                if (AzureADSettings.isADEnabled(hc))
                    Response.Redirect("~/logout.aspx", true);

                return false;
            }
            catch (Exception ex)
            {
                if (ex.Message == Languages.get_Translation("msgSignInPassChangeReq"))
                {
                    ContextService.SetSessionValue("mustChangePassword", true.ToString(),hc);
                    ContextService.CreateCookies("mustChangePassword", true.ToString(),hc);
                }
                else
                {
                    ContextService.CreateCookies("lastPassword", "",hc);
                }
                if (db == null)
                {
                    TempData["lblError"] = Languages.get_Translation("lblSignInInvalidCustKey");
                }
                else
                {
                    TempData["lblError"] = ex.Message;
                }
            }

            if (passport.SignedIn)
            {
                ContextService.SetSessionValue("ServiceManagerEnabled", db.ServiceManagerEnabled.ToString(),hc);

                // check raju
                //var levelmgr = new LevelManager(passport);
                //ContextService.SetSessionValue("LevelManager", JsonConvert.SerializeObject(levelmgr), hc);
                ContextService.SetSessionValue("passport", JsonConvert.SerializeObject(passport),hc);
                ContextService.SetSessionValue("ConnectionString", passport.ConnectionString,hc);
                //just testing the get Moti Mashiah;
                //var test = ContextService.GetObjectFromJson<Passport>("passport");

                ContextService.SetSessionValue("DatabaseName", db.Database,hc);
                ContextService.SetSessionValue("DatabaseDisplayName", db.Name,hc);
                ContextService.SetSessionValue("Server", db.Server,hc);

                if (!AzureADSettings.isADEnabled(hc))
                {
                    ContextService.SetSessionValue("UserName", txtUserName,hc);
                }

                ContextService.CreateCookies("lastUsername", strUserName,hc);
                ContextService.RemoveCookie("lastDatabaseIndex",hc);
                ContextService.CreateCookies("lastDatabaseIndex", ddlDatabase, hc);
                
                DrawLastSignInInfoText();
                ContextService.CreateCookies("Islogin", true.ToString(),hc);

                if (chkSaveAuth)
                {
                    ContextService.CreateCookies("lastPassword", Smead.Security.Encrypt.IPEncrypt(txtPassword, GetUserIP()), hc);
                }

                return true;

            }

            return false;
        }
        private string GetUserIP()
        {

            try
            {
               var IpAddress =  hc.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
                return IpAddress ?? "";
            }
            catch (Exception)
            {
                return "";
            }

            //if (string.IsNullOrEmpty(ContextService.httpContext.Request.ServerVariables("HTTP_X_FORWARDED_FOR")))
            //{
            //    return Request.UserHostAddress;
            //}
            //else
            //{
            //    return HttpContext.Current.Request.ServerVariables("HTTP_X_FORWARDED_FOR").Split(",").First().Trim();
            //}
        }
    }

}

public class DatabaseModel
{
    public string Value { get; set; }
    public string Text { get; set; }
    public bool Selected { get; set; } = false;
}
