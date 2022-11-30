public sealed class AzureADSettings
{
    private static string aadInstance
    {
        get
        {
            try
            {
                var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                return EnsureTrailingSlash(ContextService.AppSetting("ida:AADInstance", configuration));
            }
            catch(Exception)
            {
                return "";
            }
        }
    }
    public static readonly string Authority = aadInstance + "common";
    //// Private graphResourceId As String = "https://graph.windows.net"
    public static string GraphResourceURL = "https://graph.microsoft.com";
    public static string SignOutURL = "https://login.microsoftonline.com/{0}/oauth2/logout?post_logout_redirect_uri={1}";
    public static string MultiTenantId = "common";
    public static string urlListGroups = "https://graph.microsoft.com/v1.0/groups";
    public static string urlGroupMembers = "https://graph.microsoft.com/v1.0/groups/{0}/members?$select=userPrincipalName,displayName,id"; // For Paggination required to append "&$top=1"

    public static int dbSelected = 0;
    public static string secureUserType = "Z";
    public static int userDefaultGroupId = -1;

    public static bool isADEnabled(HttpContext httpContext)
    {
        try
        {
            bool bIsADEnabled = false;

            if (httpContext.Session != null)
            {
                var isAdenabled = httpContext.Session.GetString("isADEnabled");
                if (isAdenabled != null)
                {
                    bIsADEnabled = Convert.ToBoolean(isAdenabled);
                }
            }
            return bIsADEnabled;
        }
        catch(Exception)
        {
            return false;
        }
    }

    public static string authToken(HttpContext httpContext)
    {
        try
        {
            string strAuthToken = string.Empty;
            if (httpContext is not null)
            {
                var authToken = httpContext.Session.GetString("authToken");
                if (authToken is not null)
                {
                    strAuthToken = authToken;
                }
            }
            return strAuthToken;
        }
        catch(Exception)
        {
            return String.Empty;
        }
    }
    public static string EnsureTrailingSlash(string value)
    {
        if (value == null)
        {
            value = string.Empty;
        }

        if (!value.EndsWith("/", StringComparison.Ordinal))
        {
            return value + "/";
        }

        return value;
    }
}