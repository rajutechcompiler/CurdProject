using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
public class AzureADService
{
    public AzureADModel GetAzureADSettings(HttpContext httpContext,string dbName = "")
    {

        // Get the host contains the database
        if (string.IsNullOrEmpty(dbName))
        {
            string url = httpContext.Request.Host.Host;
            string[] splitURL;
            // Dim dbName As String = String.Empty
            if (!string.IsNullOrEmpty(url))
            {
                if (url.Contains(".") && !url.Contains("127.0.0.1"))
                {
                    splitURL = url.Split('.');
                    if (splitURL.Length > 1)
                    {
                        dbName = splitURL[0];
                    }
                }
            }
        }

        slimShared.CSlimAPI slim;
        try
        {
            slim = new slimShared.CSlimAPI("iAccess", true);
        }
        catch (Exception ex)
        {
            // String.Format(Languages.Translation("msgUnableToConnSlim"), vbCrLf, ex.Message)
            throw new Exception(ex.Message);
        }

        slimShared.CSLIMDatabase db = null;
        short dbIndex = 0;
        // If host name have the database name than init the database and get that database object from the slim
        if (!string.IsNullOrEmpty(dbName))
        {
            for (int i = 0, loopTo = slim.DatabaseCount - 1; i <= loopTo; i++)
            {
                if (slim.GetDatabase(i) is not null)
                {
                    if (dbName.Trim().ToLower().Equals(slim.GetDatabase(i).Name.Trim().ToLower()))
                    {
                        dbIndex = (short)i;
                        db = slim.GetDatabase(i, true);
                        break;
                    }
                }
            }
        }
        else // If not exists than init object for the first database
        {
            try
            {
                if (httpContext.Request.Cookies["lastDatabaseIndex"] is not null)
                {
                    short databaseIndex = (short)Conversions.ToInteger(httpContext.Request.Cookies["lastDatabaseIndex"]);
                    dbIndex = databaseIndex;
                    db = slim.GetDatabase(databaseIndex, true);
                }
                else
                {
                    db = slim.GetDatabase(0, true);
                }
            }
            catch (Exception)
            {
                db = slim.GetDatabase(0, true);
            }
        }

        if (db is null)
        {
            return null;
        }
        var azureADService = new AzureADService();
        return GetAzureADSettings(db, dbIndex);
    }
    public AzureADModel GetAzureADSettings(slimShared.CSLIMDatabase db, short dbIndex)
    {
        if (db is null)
        {
            return null;
        }

        var builder = new SqlConnectionStringBuilder();
        builder.DataSource = db.Server;
        builder.InitialCatalog = db.Database;
        if (string.IsNullOrEmpty(db.UserName))
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = db.UserName;
            builder.Password = db.Password;
            builder.PersistSecurityInfo = true;
        }

        builder.MaxPoolSize = 500;
        builder.ConnectTimeout = 60;

        string strSql = "SELECT Item,ItemValue FROM Settings WHERE Section='AzureAD'";
        var dtb = new DataTable();
        using (var conn = new SqlConnection(builder.ConnectionString))
        {
            try
            {
                conn.Open();
                using (var dad = new SqlDataAdapter(strSql, conn))
                {
                    dad.Fill(dtb);
                }
                conn.Close();
            }
            catch(Exception ex)
            {

            }
        }
        var azureADModel = new AzureADModel();
        foreach (DataRow row in dtb.Rows)
        {
            if (row["Item"].ToString() ==  "ClientId")
            {
                azureADModel.ADClientId = row["ItemValue"].ToString();
            }
            else if (row["Item"].ToString() == "ClientSecret")
            {
                azureADModel.ADClientSecret = row["ItemValue"].ToString();
            }
            else if (row["Item"].ToString() == "EnabledAzureAD")
            {
                azureADModel.ADEnabled = Convert.ToString(Convert.ToBoolean(row["ItemValue"]));
            }
            else if (row["Item"].ToString() == "ClientIdAndroid")
            {
                azureADModel.ADClientIdAndroid = Convert.ToString(row["ItemValue"]);
            }
        }
        azureADModel.DBIndex = dbIndex;
        return azureADModel;
    }
}

public class AzureADModel
{
    private string _adClientId;
    public string ADClientId
    {
        get
        {
            return _adClientId;
        }
        set
        {
            _adClientId = Strings.Trim(value);
        }
    }

    private string _adClientSecret;
    public string ADClientSecret
    {
        get
        {
            return _adClientSecret;
        }
        set
        {
            _adClientSecret = Strings.Trim(value);
        }
    }

    private string _adTenantId;
    public string ADTenantId
    {
        get
        {
            return _adTenantId;
        }
        set
        {
            _adTenantId = Strings.Trim(value);
        }
    }

    private string _adEnabled;
    public string ADEnabled
    {
        get
        {
            return _adEnabled;
        }
        set
        {
            _adEnabled = Strings.Trim(value);
        }
    }

    private short _dbIndex;
    public short DBIndex
    {
        get
        {
            return _dbIndex;
        }
        set
        {
            _dbIndex = value;
        }
    }

    private string _adClientIdAndroid;
    public string ADClientIdAndroid
    {
        get
        {
            return _adClientIdAndroid;
        }
        set
        {
            _adClientIdAndroid = Strings.Trim(value);
        }
    }
}

public class AzureADGroupsModel
{
    private string _id;
    public string Id
    {
        get
        {
            return _id;
        }
        set
        {
            _id = Strings.Trim(value);
        }
    }

    private string _groupId;
    public string GroupId
    {
        get
        {
            return _groupId;
        }
        set
        {
            _groupId = Strings.Trim(value);
        }
    }

    private string _displayName;
    public string DisplayName
    {
        get
        {
            return _displayName;
        }
        set
        {
            _displayName = Strings.Trim(value);
        }
    }

}

public class AzureADGroupUsersModel
{
    private string _id;
    public string Id
    {
        get
        {
            return _id;
        }
        set
        {
            _id = Strings.Trim(value);
        }
    }

    private string _displayName;
    public string DisplayName
    {
        get
        {
            return _displayName;
        }
        set
        {
            _displayName = Strings.Trim(value);
        }
    }

    private string _email;
    public string Email
    {
        get
        {
            return _email;
        }
        set
        {
            _email = Strings.Trim(value);
        }
    }
}

public class GroupsMappingModel
{
    private int _groupID;
    public int GroupID
    {
        get
        {
            return _groupID;
        }
        set
        {
            _groupID = value;
        }
    }

    private string _groupName;
    public string GroupName
    {
        get
        {
            return _groupName;
        }
        set
        {
            _groupName = Strings.Trim(value);
        }
    }

}

public class AzureADSecurityModel
{

    private SelectList _fusionGroups;
    public SelectList FusionGroups
    {
        get
        {
            return _fusionGroups;
        }
        set
        {
            _fusionGroups = value;
        }
    }

    private SelectList _aDGroups;
    public SelectList ADGroups
    {
        get
        {
            return _aDGroups;
        }
        set
        {
            _aDGroups = value;
        }
    }
    private bool _adEnabled;
    public bool ADEnabled
    {
        get
        {
            return _adEnabled;
        }
        set
        {
            _adEnabled = value;
        }
    }
    private string _aDAdminUser;
    public string ADAdminUser
    {
        get
        {
            return _aDAdminUser;
        }
        set
        {
            _aDAdminUser = Strings.Trim(value);
        }
    }
    private SelectList _mappedGroups;
    public SelectList MappedGroups
    {
        get
        {
            return _mappedGroups;
        }
        set
        {
            _mappedGroups = value;
        }
    }
}