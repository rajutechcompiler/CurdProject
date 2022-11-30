//using Newtonsoft.Json;
using System.Text.Json;
using System.Data;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.AspNetCore.Http.Features;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting.Server.Features;

public class ContextService
{
    #region static members
    /// <summary>
    /// gettting physical path of dick without back forward slash at last
    /// Ex: E:\Project\TabFusionRMS.WebCS
    /// </summary>
    //private static string basedirectory;
    //private static IConfiguration configuration;
    #endregion

    #region getting paths of projects
    /// <summary>
    /// getting the path of wwwroot folder.
    /// Ex:- E:\Project\TabFusionRMS.WebCS\wwwroot
    /// </summary>
    //public static string GetWebRootPath()
    //{
    //    return basedirectory + "/wwwroot";
    //}
    #endregion

    #region Cookies getting and setting fuctions
    /// <summary>
    /// Setting multiple values to the single cookie
    /// </summary>
    public static void CreateCookies(string key, string value, HttpContext httpContext, [Optional] double expire, [Optional] bool https)
    {
        CookieOptions obj = new CookieOptions();
        obj.Expires = DateTime.Now.AddDays(180d);

        if (https)
            obj.HttpOnly = true;

        if (expire > 0)
            obj.Expires = DateTime.Now.AddDays(expire);

        httpContext.Response.Cookies.Append(key, value, obj);
    }

    /// <summary>
    /// Setting multiple values to the single cookie
    /// </summary>
    public static void CreateCookies(string key, IDictionary<string, string> value, HttpContext httpContext,[Optional] double expire, [Optional] bool https)
    {
        CookieOptions obj = new CookieOptions();
        obj.Expires = DateTime.Now.AddDays(180d);

        if (https)
            obj.HttpOnly = true;

        if (expire > 0)
            obj.Expires = DateTime.Now.AddDays(expire);

        httpContext.Response.Cookies.Append(key, DictionaryToString(value), obj);
    }

    /// <summary>
    /// Setting cookies value as null
    /// </summary>
    public static void SetCookieNull(string key, HttpContext httpContext)
    {
        httpContext.Response.Cookies.Append(key, null);
    }

    /// <summary>
    /// getting cookie value
    /// </summary>
    public static string GetCookieVal(string key, HttpContext httpContext)
    {
        return httpContext.Request.Cookies[key];
    }

    /// <summary>
    /// remove cookie
    /// </summary>
    public static void RemoveCookie(string key, HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(key);
    }

    /// <summary>
    /// converting multiple value of single cookie into dictionary formate
    /// </summary>
    public static IDictionary<string, string> FromLegacyCookieString(string legacyCookie)
    {
        try
        {
            return legacyCookie.Split('&').Select(s => s.Split('=')).ToDictionary(kvp => kvp[0], kvp => kvp[1]);
        }
        catch (Exception)
        {
            return new Dictionary<string, string>();
        }
    }
    public static IDictionary<int, int> FromLegacyCookieStringToDicInt(string legacyCookie)
    {
        try
        {
            return legacyCookie.Split('&').Select(s => s.Split('=')).ToDictionary(kvp => Convert.ToInt32(kvp[0]), kvp => Convert.ToInt32(kvp[1]));
        }
        catch (Exception)
        {
            return new Dictionary<int, int>();
        }
    }
    /// <summary>
    /// Get value from multiple value base cookies
    /// </summary>
    public static string GetCookiesValueFromMultiple(string cookiesName, string key, HttpContext httpContext)
    {
        try
        {
            var val = ContextService.GetCookieVal(cookiesName, httpContext);
            var dic = ContextService.FromLegacyCookieString(val);

            return dic[key];
        }
        catch(Exception ex)
        {
            return "";
        }
    }

    public static IDictionary<string, string> GetCookieValuesDicStr(string key, HttpContext httpContext)
    {
        var val = ContextService.GetCookieVal(key, httpContext);
        return ContextService.FromLegacyCookieString(val);
    }


    #endregion

    #region Session getting and setting functions
    /// <summary>
    /// getting session value and convert into Object or list object etc...
    /// </summary>
    public static T GetObjectFromJson<T>(string key, HttpContext httpContext)
    {
        try
        {
            var value = httpContext.Session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
        catch (Exception)
        {
            return default(T);
        }
    }
    /// <summary>
    /// setting session value into string formate
    /// </summary>

    public static void SetSessionValue(string key, string val, HttpContext httpContext)
    {
        httpContext.Session.SetString(key, val);
    }
    public static string GetSessionValueByKey(string key, HttpContext httpContext)
    {
        try
        {
            return httpContext.Session.GetString(key);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// getting session value into string formate
    /// </summary>
    public static void RemoveSession(string key, HttpContext httpContext)
    {
        httpContext.Session.Remove(key);
    }
    #endregion

    #region helping functions

    /// <summary>
    /// getting dictionary "IDictionary<string, string> " to "string" 
    /// </summary>
    public static string DictionaryToString(IDictionary<string, string> dict)
    {
        return string.Join("&", dict.Select(kvp => string.Join("=", kvp.Key, kvp.Value)));
    }

    #endregion

    #region Getting Appsetting.json value 
    /// <summary>
    /// getting value from appsetting.json file
    /// </summary>
    public static string AppSetting(string key, IConfiguration configuration)
    {
        return configuration.GetValue<string>(key);
    }
    #endregion

    public static bool HttpContextIsNull(HttpContext httpContext)
    {
        if (httpContext is null)
            return true;
        else
            return false;
    }

    public static bool SessionIsNull(HttpContext httpContext)
    {
        try
        {
            if (httpContext.Session is null)
                return true;
            else
                return false;
        }
        catch (Exception)
        {
            return true;
        }
    }

    public static string UserLanguages(HttpContext httpContext)
    {
        try
        {
            return httpContext.Request.Headers["AcceptLanguage"].ToString();
        }
        catch (Exception)
        {
            return "en-US";
        }
    }

    public static string GetServerVariablesFeature(string ServerVariable, HttpContext httpContext)
    {
        try
        {
            var svf = httpContext.Features.Get<IServerVariablesFeature>();
            if (svf == null)
            {
                return "";
            }
            else
            {
                var hosteName = svf[ServerVariable];
                return hosteName ?? "";
            }
        }
        catch (Exception)
        {
            return "";
        }
    }
    public static string GetServerAddressesFeature(HttpContext httpContext)
    {
        try
        {
            var svf = httpContext.Features.Get<IServerAddressesFeature>().Addresses;
            if (svf == null)
            {
                return "";
            }
            else
            {
                return svf.ToString();
            }
        }
        catch (Exception)
        {
            return "";
        }
    }

}