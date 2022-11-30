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
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using System.Data.OleDb;
using Smead.RecordsManagement;
using System.Data;
using TabFusionRMS.Resource;
using TabFusionRMS.WebCS;

public sealed class SavedCriteria
{
    public static Int32 SaveSavedCriteria(Int32 userId, ref string pErrorMessage, string FavouriteName, Int32 pViewId, IRepository<s_SavedCriteria> _is_SavedCriteria)
    {
        // Dim _is_SavedCriteria As IRepository(Of s_SavedCriteria) = New RepositoryVB.Repositories(Of s_SavedCriteria)
        s_SavedCriteria ps_SavedCriteria = new s_SavedCriteria();
        try
        {
            ps_SavedCriteria.UserId = userId;
            ps_SavedCriteria.SavedName = FavouriteName;
            ps_SavedCriteria.SavedType = (int)Enums.SavedType.Favorite;
            ps_SavedCriteria.ViewId = pViewId;
            _is_SavedCriteria.Add(ps_SavedCriteria);
        }
        catch (Exception ex)
        {
            pErrorMessage = ex.Message;
            return -1;
        }
        return ps_SavedCriteria.Id;
    }

    public static bool SaveSavedChildrenFavourite(ref string pErrorMessage, bool isNewRecord, Int32 ps_SavedCriteriaId, Int32 pViewId, List<string> lSelectedItemList, IRepository<s_SavedCriteria> _is_SavedCriteria, IRepository<s_SavedChildrenFavorite> _is_SavedChildrenFavorite)
    {
        // Dim _is_SavedCriteria As IRepository(Of s_SavedCriteria) = New RepositoryVB.Repositories(Of s_SavedCriteria)
        // Dim _is_SavedChildrenFavorite As IRepository(Of s_SavedChildrenFavorite) = New RepositoryVB.Repositories(Of s_SavedChildrenFavorite)
        var IsSuccess = false;
        try
        {
            List<s_SavedChildrenFavorite> ls_SavedChildrenFavorite = new List<s_SavedChildrenFavorite>();
            var Lists_SavedChildrenFavorite = _is_SavedChildrenFavorite.All().ToList();
            var Lists_SavedCriteria = _is_SavedCriteria.All().ToList();
            _is_SavedCriteria.Where(a => a.SavedName == "").FirstOrDefault();

            var finalOutPut = from child in Lists_SavedChildrenFavorite
                              join par in Lists_SavedCriteria
                              on child.SavedCriteriaId equals par.Id
                              where par.Id == Convert.ToInt32(child.SavedCriteriaId)
                              select new { par.ViewId, child.TableId, par.Id };

            foreach (string tableId in lSelectedItemList)
            {
                if (isNewRecord | !(finalOutPut.Any(x => x.TableId == tableId && x.ViewId == pViewId && x.Id == ps_SavedCriteriaId)))
                {
                    s_SavedChildrenFavorite ps_SavedChildrenFavorite = new s_SavedChildrenFavorite();
                    ps_SavedChildrenFavorite.SavedCriteriaId = ps_SavedCriteriaId;
                    ps_SavedChildrenFavorite.TableId = tableId;
                    ls_SavedChildrenFavorite.Add(ps_SavedChildrenFavorite);
                }
            }

            _is_SavedChildrenFavorite.AddList(ls_SavedChildrenFavorite);

            IsSuccess = true;
        }
        catch (Exception ex)
        {
            pErrorMessage = ex.Message;
            IsSuccess = false;
        }
        return IsSuccess;
    }

    public static bool DeleteSavedCriteria(Int32 id, string SavedCriteriaType)
    {
        TabFusionRMS.RepositoryVB.IRepository<TabFusionRMS.Models.s_SavedCriteria> s_SavedCriteria = new TabFusionRMS.RepositoryVB.Repositories<TabFusionRMS.Models.s_SavedCriteria>();
        TabFusionRMS.RepositoryVB.IRepository<s_SavedChildrenFavorite> _is_SavedChildrenFavorite = new TabFusionRMS.RepositoryVB.Repositories<s_SavedChildrenFavorite>();
        TabFusionRMS.RepositoryVB.IRepository<TabFusionRMS.Models.s_SavedChildrenQuery> s_SavedChildrenQuery = new TabFusionRMS.RepositoryVB.Repositories<TabFusionRMS.Models.s_SavedChildrenQuery>();
        try
        {
            s_SavedCriteria.BeginTransaction();
            var savedCriteria = s_SavedCriteria.Where(x => x.Id == id).FirstOrDefault();
            if (savedCriteria != null)
            {
                s_SavedCriteria.Delete(savedCriteria);
                if (SavedCriteriaType == "1")
                {
                    _is_SavedChildrenFavorite.BeginTransaction();
                    var s_s_SavedChildrenFavoriteList = _is_SavedChildrenFavorite.Where(x => x.SavedCriteriaId == id).ToList();
                    if (s_s_SavedChildrenFavoriteList != null)
                        _is_SavedChildrenFavorite.DeleteRange(s_s_SavedChildrenFavoriteList);
                    _is_SavedChildrenFavorite.CommitTransaction();
                }
                else
                {
                    s_SavedChildrenQuery.BeginTransaction();
                    TabFusionRMS.Models.s_SavedChildrenQuery savedChildrenQuery = new TabFusionRMS.Models.s_SavedChildrenQuery();
                    var odjdel = s_SavedChildrenQuery.Where(x => x.SavedCriteriaId == id).ToList();
                    if (odjdel != null)
                        s_SavedChildrenQuery.DeleteRange(odjdel);
                    s_SavedChildrenQuery.CommitTransaction();
                }
                s_SavedCriteria.CommitTransaction();
            }
            return true;
        }
        catch (Exception ex)
        {
            if (SavedCriteriaType == "1")
                _is_SavedChildrenFavorite.RollBackTransaction();
            else
                s_SavedChildrenQuery.RollBackTransaction();
            s_SavedCriteria.RollBackTransaction();
            return false;
        }
    }
    public static bool DeleteFavouriteRecords(List<string> ids, Int32 savedCriteriaId)
    {
        TabFusionRMS.RepositoryVB.IRepository<s_SavedChildrenFavorite> _is_SavedChildrenFavorite = new TabFusionRMS.RepositoryVB.Repositories<s_SavedChildrenFavorite>();
        try
        {
            var SavedChildrenFavoriteList = _is_SavedChildrenFavorite.Where(m => m.SavedCriteriaId == savedCriteriaId && ids.Contains(m.TableId));
            if (SavedChildrenFavoriteList != null)
                _is_SavedChildrenFavorite.DeleteRange(SavedChildrenFavoriteList);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static string SetDelimiterById(string DelimiterId)
    {
        string Delimiter = string.Empty;
        switch (DelimiterId)
        {
            case "comma":
                {
                    Delimiter = ",";
                    break;
                }

            case "tab":
                {
                    Delimiter = Constants.vbTab;
                    break;
                }

            case "semicolon":
                {
                    Delimiter = ";";
                    break;
                }

            case "space":
                {
                    Delimiter = Strings.Space(1);
                    break;
                }

            default:
                {
                    Delimiter = DelimiterId;
                    break;
                }
        }
        return Delimiter;
    }

    public static string GetConnStringForOLEDB(string extension, bool headerFlag, char Delimiter = ' ', string uplodedFilePath = "")
    {
        string connString = string.Empty;
        try
        {
            string strHeaderFlag = headerFlag == true ? "YES" : "NO";

            System.Data.OleDb.OleDbConnectionStringBuilder sb = new System.Data.OleDb.OleDbConnectionStringBuilder();
            switch (extension)
            {
                case "xls":
                case "xlsx":
                    {
                        if ((Strings.StrComp(extension, "xls", Constants.vbTextCompare) == 0))
                        {
                            sb.Provider = "Microsoft.Jet.OLEDB.4.0";
                            sb.DataSource = uplodedFilePath;
                            sb.Add("Extended Properties", string.Format("Excel 8.0;HDR={0};IMEX=1", strHeaderFlag));
                        }
                        else
                        {
                            sb.Provider = "Microsoft.ACE.OLEDB.12.0";
                            sb.DataSource = uplodedFilePath;
                            sb.Add("Extended Properties", string.Format("Excel 12.0;HDR={0};IMEX=1", strHeaderFlag));
                        }
                        connString = sb.ToString();
                        break;
                    }

                case "txt":
                case "csv":
                    {
                        if ((Delimiter == ','))
                            connString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR={0};FMT=CsvDelimited';", strHeaderFlag);
                        else if ((Delimiter == Convert.ToChar(Strings.Space(1))))
                            connString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR={0};IMEX=1;FMT=Delimited('" + Strings.Space(1) + "')\';", strHeaderFlag);
                        else if ((Delimiter == Convert.ToChar(Constants.vbTab)))
                            connString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR={0};IMEX=1;FMT=TabDelimited';", strHeaderFlag);
                        else if ((Delimiter == ';'))
                            connString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR={0};IMEX=1;FMT=Delimited(';')\";", strHeaderFlag);
                        else
                            connString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties='Text;HDR={0};IMEX=1;FMT=Delimited(" + Delimiter + ")\';", strHeaderFlag); connString = string.Format(connString, uplodedFilePath);
                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
        return connString;
    }

    public static List<string> getColumnFromFile(string extention, bool hasFirstRowFieldNames, string prFilePath, bool OnlyFirstRow = true)
    {
        List<string> colArray = new List<string>();

        string connString = null;
        string objectName = "";

        DataTable dtExcelData = new DataTable();
        connString = SavedCriteria.GetConnStringForOLEDB(extention, hasFirstRowFieldNames, ' ', prFilePath);
        using (OleDbConnection excel_con = new OleDbConnection(connString))
        {
            excel_con.Open();
            var count = excel_con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows.Count;
            if ((count > 0))
            {
                objectName = excel_con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["TABLE_NAME"].ToString();
                var query = string.Format("SELECT {0} * FROM [{1}]", Interaction.IIf(OnlyFirstRow, "TOP 1", ""), objectName);
                using (OleDbDataAdapter oda = new OleDbDataAdapter(query, excel_con))
                {
                    oda.Fill(dtExcelData);
                }
            }
        }
        if ((dtExcelData != null))
        {
            if ((dtExcelData.Columns.Count > 0))
            {
                for (var index = 1; index <= dtExcelData.Columns.Count; index++)
                    colArray.Add(getColumnLetterFromColumnIndex(index));
            }
        }
        return colArray;
    }
    public static DataTable getDataFromFile(string extention, bool hasFirstRowFieldNames, string prFilePath)
    {
        string connString = null;
        string objectName = "";

        DataTable dtExcelData = new DataTable();

        switch (extention.ToLower())
        {
            case "xls":
            case "xlsx":
                {
                    connString = SavedCriteria.GetConnStringForOLEDB(extention, hasFirstRowFieldNames, ' ', prFilePath);
                    using (OleDbConnection excel_con = new OleDbConnection(connString))
                    {
                        excel_con.Open();
                        var count = excel_con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows.Count;
                        if ((count > 0))
                        {
                            objectName = excel_con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null).Rows[0]["TABLE_NAME"].ToString();
                            var query = string.Format("SELECT * FROM [{0}]", objectName);
                            using (OleDbDataAdapter oda = new OleDbDataAdapter(query, excel_con))
                            {
                                oda.Fill(dtExcelData);
                            }
                        }
                    }

                    break;
                }

            case "csv":
            case "txt":
                {
                    break;
                }
        }
        return dtExcelData;
    }

    public static string getColumnLetterFromColumnIndex(int colIndex)
    {
        int div = colIndex;
        string colLetter = string.Empty;
        int modnum = 0;

        while (div > 0)
        {
            modnum = (div - 1) % 26;
            colLetter = Strings.Chr(65 + modnum) + colLetter;
            div = System.Convert.ToInt32((div - modnum) / 26);
        }

        return string.Format("Column {0}", colLetter);
    }

    public static bool SaveImportFavourite(int prId, DataTable prDtRecExists, IRepository<s_SavedChildrenFavorite> _is_SavedChildrenFavorite)
    {
        var IsSuccess = false;
        try
        {
            var lstExistingTableIds = _is_SavedChildrenFavorite.All().Where(w => w.SavedCriteriaId == prId).ToList();

            foreach (s_SavedChildrenFavorite item in lstExistingTableIds)
            {
                if (prDtRecExists != null && prDtRecExists.Rows.Count > 0)
                {
                    foreach (DataRow dr in prDtRecExists.Select())
                    {
                        if (dr[0] == item.TableId)
                            dr.Delete();
                    }
                }
            }

            List<s_SavedChildrenFavorite> ls_SavedChildrenFavorite = new List<s_SavedChildrenFavorite>();

            if (prDtRecExists != null && prDtRecExists.Rows.Count > 0)
            {
                s_SavedChildrenFavorite ps_SavedChildrenFavorite;
                for (var index = 0; index <= prDtRecExists.Rows.Count - 1; index++)
                {
                    var asdf = prDtRecExists.Rows[index];
                    ps_SavedChildrenFavorite = new s_SavedChildrenFavorite();
                    ps_SavedChildrenFavorite.SavedCriteriaId = prId;
                    ps_SavedChildrenFavorite.TableId = prDtRecExists.Rows[index]["ids"].ToString();
                    ls_SavedChildrenFavorite.Add(ps_SavedChildrenFavorite);
                }
                _is_SavedChildrenFavorite.AddList(ls_SavedChildrenFavorite);
            }
        }
        catch (Exception ex)
        {
            IsSuccess = false;
        }
        return IsSuccess;
    }

    public static bool SaveImportFavouriteMVC(ImportFavoriteModel m, DataTable prDtRecExists, IRepository<s_SavedChildrenFavorite> _is_SavedChildrenFavorite)
    {
        var IsSuccess = false;
        try
        {
            var lstExistingTableIds = _is_SavedChildrenFavorite.All().Where(w => w.SavedCriteriaId == Convert.ToInt32(m.favoritListSelectorid)).ToList();

            foreach (s_SavedChildrenFavorite item in lstExistingTableIds)
            {
                if (prDtRecExists != null && prDtRecExists.Rows.Count > 0)
                {
                    foreach (DataRow dr in prDtRecExists.Select())
                    {
                        if (dr[0] == item.TableId)
                            prDtRecExists.Rows.Remove(dr);
                    }
                }
            }

            List<s_SavedChildrenFavorite> ls_SavedChildrenFavorite = new List<s_SavedChildrenFavorite>();

            if (prDtRecExists != null && prDtRecExists.Rows.Count > 0)
            {
                s_SavedChildrenFavorite ps_SavedChildrenFavorite;
                for (var index = 0; index <= prDtRecExists.Rows.Count - 1; index++)
                {
                    // m.lsRecExists.Add(prDtRecExists.Rows(index).ItemArray(0).ToString)
                    ps_SavedChildrenFavorite = new s_SavedChildrenFavorite();
                    ps_SavedChildrenFavorite.SavedCriteriaId = Convert.ToInt32(m.favoritListSelectorid);
                    ps_SavedChildrenFavorite.TableId = prDtRecExists.Rows[index].ItemArray[0].ToString();
                    ls_SavedChildrenFavorite.Add(ps_SavedChildrenFavorite);
                }
                _is_SavedChildrenFavorite.AddList(ls_SavedChildrenFavorite);
            }
        }
        catch (Exception ex)
        {
            IsSuccess = false;
        }
        return IsSuccess;
    }

    public static List<FieldValue> GatherFieldsWithValues(string myQueryValue)
    {
        List<FieldValue> list = new List<FieldValue>();
        IRepository<s_SavedChildrenQuery> s_SavedChildrenQuery = new TabFusionRMS.RepositoryVB.Repositories<s_SavedChildrenQuery>();
        var tempQueryValue = System.Convert.ToInt32(myQueryValue);
        var savedChildrenQuery = s_SavedChildrenQuery.All().Where(x => x.SavedCriteriaId == tempQueryValue).ToList();
        foreach (var item in savedChildrenQuery)
        {
            FieldValue fv = new FieldValue(item.ColumnName, item.GetType().ToString());
            fv.Operate = item.Operator;
            fv.value = item.CriteriaValue;
            list.Add(fv);
        }

        return list;
    }

    public static string GetMyQueMyFavName(HttpContext httpContext)
    {
        Int32 savedCriteriaId = 0;
        string savedCriteriaName = string.Empty;

        if (httpContext.Request.Cookies["myQueryValue"] !=null)
        {
            var myQueryValue = string.IsNullOrEmpty(httpContext.Request.Cookies["myQueryValue"].ToString()) ? 0 : Convert.ToInt32(httpContext.Request.Cookies["myQueryValue"].ToString());
            if (myQueryValue > 0 | myQueryValue == -1)
            {
                if (httpContext.Request.Cookies["SavedCriteriaType"] != null)
                {
                    savedCriteriaId = System.Convert.ToInt32(httpContext.Request.Cookies["myQueryValue"].ToString().Split("_")[0]);
                    if (savedCriteriaId > 0)
                    {
                        TabFusionRMS.RepositoryVB.IRepository<TabFusionRMS.Models.s_SavedCriteria> _is_SavedCriteria = new TabFusionRMS.RepositoryVB.Repositories<TabFusionRMS.Models.s_SavedCriteria>();
                        var SavedCriteria = _is_SavedCriteria.Where(x => x.Id == savedCriteriaId).FirstOrDefault();
                        if (SavedCriteria != null)
                            savedCriteriaName = SavedCriteria.SavedName;
                    }
                }
            }
        }
        return savedCriteriaName;
    }

    /// <summary>
    ///     ''' MyQuery Validation at Query Pop Up
    ///     ''' </summary>
    ///     ''' <param name="name"></param>
    ///     ''' <param name="id"></param>
    ///     ''' <returns></returns>
    public static string ValidateMyQueries(string name, string isTitleChanged, int id, HttpContext httpContext, int userId)
    {
        dynamic savedCriteria;
        Keys.IsValidQuerySet(true,httpContext);
        if (isTitleChanged == "false")
            return "";
        if (id > 0)
        {
            if (string.IsNullOrEmpty(name))
            {
                Keys.IsValidQuerySet(false, httpContext);
                var message = Languages.get_Translation("msgMyQueryNotBlank");
                return message.Replace(Constants.vbTab, "\t").Replace(Constants.vbCrLf, "\r\n");
            }
        }
        if (name.Length > 60)
        {
            Keys.IsValidQuerySet(false, httpContext);
            var message = string.Format(Languages.get_Translation("msgMyQuerySavedNameMaxLenght"), name);
            return message.Replace(Constants.vbTab, "\t").Replace(Constants.vbCrLf, "\r\n");
        }
        if (!string.IsNullOrEmpty(name))
        {
            IRepository<s_SavedCriteria> s_SavedCriteria = new TabFusionRMS.RepositoryVB.Repositories<s_SavedCriteria>();
            if (id > 0)
            {
                savedCriteria = s_SavedCriteria.Where(x => x.Id == id & x.SavedType == (int)Enums.SavedType.Query).FirstOrDefault();
                if (savedCriteria != null && savedCriteria.SavedName == name)
                    return "";
                else
                {
                    savedCriteria = s_SavedCriteria.Where(x => x.SavedName == name && x.SavedType == (int)Enums.SavedType.Query && x.UserId == userId).FirstOrDefault();
                    if (savedCriteria != null)
                    {
                        Keys.IsValidQuerySet(false, httpContext);
                        var message = string.Format(Languages.get_Translation("msgMyQueryUniqueSavedName"), name);
                        return message.Replace(Constants.vbTab, @"\t").Replace(Constants.vbCrLf, @"\r\n");
                    }
                    else
                        return "";
                }
            }
            else
            {
                savedCriteria = s_SavedCriteria.Where(x => x.SavedName == name && x.SavedType == (int)Enums.SavedType.Query && x.UserId == userId).FirstOrDefault();
                if (savedCriteria != null)
                {
                    Keys.IsValidQuerySet(false, httpContext);
                    var message = string.Format(Languages.get_Translation("msgMyQueryUniqueSavedName"), name);
                    return message.Replace(Constants.vbTab, @"\t").Replace(Constants.vbCrLf, @"\r\n");
                }
                else
                    return "";
            }
        }
        else
            return "";
    }
}