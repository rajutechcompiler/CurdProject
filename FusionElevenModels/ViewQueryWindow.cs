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
using Smead.RecordsManagement;
//using Smead.RecordsManagement.Navigation
using Smead.Security;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.WebCS.FusionElevenModels;
using System.Data;
using TabFusionRMS.Resource;

// QUERY WINDOW MODEL RETURN DOM OBJECT
public class ViewQueryWindow : BaseModel
{
    public ViewQueryWindow(Passport passport, int crumblevel, string childKeyField, HttpContext httpContext)
    {
        this._passport = passport;
        _httpContext = httpContext;
        // Me._LevelManager = LevelManger
        this.ListOfRows = new List<string>();
        this.MyqueryList = new List<Myquery.queryList>();
        this.listMyqueryDatatype = new List<string>();
        this.crumblevel = crumblevel;
        ChildKeyField = string.IsNullOrEmpty(childKeyField) ? String.Empty : childKeyField;
    }
    private Query _query
    {
        get;
        set;
    }
    private Parameters _Params
    {
        get;
        set;
    }
    public List<string> ListOfRows
    {
        get;
        set;
    }
    public List<Myquery.queryList> MyqueryList
    {
        get;
        set;
    }
    public List<string> listMyqueryDatatype
    {
        get;
        set;
    }
    public bool hasMyQueryAceess
    {
        get;
        set;
    } = false;
    public string dateFormat
    {
        get;
        set;
    }
    private int crumblevel
    {
        get;
        set;
    }
    public string ChildKeyField
    {
        get;
        set;
    }
    public void DrawQuery(int viewID)
    {
        // check if my query
        // If ceriteriaId > 0 Then Me.GetMyqueryList(ceriteriaId)

        if (_passport.CheckPermission(Common.SECURE_MYQUERY,
                                    Smead.Security.SecureObject.SecureObjectType.Application,
                                    Permissions.Permission.Access))
            hasMyQueryAceess = true;
        this._query = new Query(this._passport);
        this._Params = new Parameters(viewID, this._passport);
        this._Params.QueryType = queryTypeEnum.Schema;
        this._Params.Culture = Keys.GetCultureCookies(_httpContext);
        this._Params.Scope = ScopeEnum.Table;
        this._Params.ParentField = ChildKeyField;
        this._Params.Culture.DateTimeFormat.ShortDatePattern = Keys.GetCultureCookies(_httpContext).DateTimeFormat.ShortDatePattern;
        this.dateFormat = Keys.GetUserPreferences.sPreferedDateFormat.ToString().Trim().ToUpper();
        this._query.FillData(_Params);

        foreach (System.Data.DataColumn dc in _Params.Data.Columns)
        {
            if (GridDataBinding.ShowColumn(dc, this.crumblevel, this._Params.ParentField) == true)
            {
                // don't show column if the lookuptyp is 1 and it is not a dropdown.
                if (Convert.ToInt32(dc.ExtendedProperties["lookuptype"]) == 1
                    && !Convert.ToBoolean(dc.ExtendedProperties["dropdownflag"]) == true) { }
                else
                {
                    string buildRow = "<tr>" + BuildHeader(dc) + GetOperators(dc, dataType: dc.DataType.Name) + BuildTextBoxes(dc) + "</tr>";
                    ListOfRows.Add(buildRow);
                    listMyqueryDatatype.Add(dc.DataType.FullName);
                }
            }
        }
    }
    private string BuildHeader(DataColumn dc)
    {
        // aspNetDisabled form-control formWindowTextBox
        string ColumnName;
        string Header = dc.ExtendedProperties["heading"].ToString() + ":";
        string Title = dc.ExtendedProperties["heading"].ToString();
        string dataType = dc.DataType.FullName;
        bool isdropDown = System.Convert.ToBoolean(dc.ExtendedProperties["dropdownflag"]);
        if (System.Convert.ToBoolean(dc.ExtendedProperties["dropdownflag"]) == true & dc.ExtendedProperties["LookupData"] != null)
            ColumnName = Navigation.MakeSimpleField(dc.ExtendedProperties["LookupData"].ToString());
        else
            ColumnName = dc.ColumnName;

        return string.Format("<td dropdown=\"{4}\" DataType=\"{2}\" ColumnName=\"{3}\" title=\"{0}\" style=\"width:30%;text-align:left;\">{1}</td>", Title, Header, dataType, ColumnName, isdropDown);
    }
    private string GetOperators(DataColumn dc, string dataType = null)
    {
        StringBuilder ListOfOperators = new StringBuilder();
        if (Common.BOOLEAN_TYPE == dataType.ToLower() || Convert.ToInt32(dc.ExtendedProperties["lookuptype"]) == 1)
        {
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", " ", " "));
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", "=", Languages.get_Translation("queryPopupDDLEqualsto")));
        }
        else
        {
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", " ", " "));
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", "=", Languages.get_Translation("queryPopupDDLEqualsto")));
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", "<>", Languages.get_Translation("queryPopupDDLNotequalsto")));
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", ">", Languages.get_Translation("queryPopupDDLGreaterthan")));
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", ">=", Languages.get_Translation("queryPopupDDLGreaterthanequalsto")));
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", "<", Languages.get_Translation("queryPopupDDLLessthan")));
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", "<=", Languages.get_Translation("queryPopupDDLLessthanequalsto")));
            if (Common.dataType.Contains(dataType.ToLower()))
                ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", "Between", Languages.get_Translation("queryPopupDDLBetween")));
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", "BEG", Languages.get_Translation("queryPopupDDLBEG")));
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", "Ends with", Languages.get_Translation("queryPopupDDLEndWith")));
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", "Contains", Languages.get_Translation("queryPopupDDLContains")));
            ListOfOperators.Append(string.Format("<option value=\"{0}\">{1}</option>", "Not contains", Languages.get_Translation("queryPopupDDLNotContains")));
        }
        string returnOperators = string.Format("<td style=\"width:30%;text-align:center;\"><select class=\"form-control\" onchange=\"obJquerywindow.OperatorCondition(this)\" style=\"color:Black;border-color:Silver;border-width:1px;border-style:Solid;font-size:9pt;font-weight:bold;\">{0}</select></td>", ListOfOperators.ToString());
        return returnOperators;
    }
    private string BuildTextBoxes(object dc1)
    {
        var dc = (DataColumn)dc1;
        string buildInput = "";
        string placeHoldersValue = string.Empty; 
        string HeaderId = dc.ExtendedProperties["heading"].ToString().Trim();
        var filedName = dc.ExtendedProperties["FieldName"];
        switch (dc.DataType.Name.ToString().ToLower())
        {
            case "string":
                {
                    if (Convert.ToBoolean(dc.ExtendedProperties["dropdownflag"]))
                        buildInput = string.Format("<td onchange=\"obJquerywindow.WhenChangeValue(event)\" style=\"width:40%;\"><select type=\"text\" placeholder=\"{1}\" class=\"form-control\">{0}</select></td>", BuildDropDown(dc), placeHoldersValue);
                    else
                        buildInput = string.Format("<td onkeyup=\"obJquerywindow.WhenChangeValue(event)\" style=\"width:40%;\"><input type=\"text\" placeholder=\"{0}\" class=\"form-control\" ></td>", placeHoldersValue);
                    break;
                }

            case "boolean":
                {
                    buildInput = "<td onclick=\"obJquerywindow.WhenChangeValue(event)\" class=\"datacell\" style=\"border-width:0px;width:40%;text-align:left\"><input class=\"modal-checkbox\" type=\"checkbox\"></td>";
                    break;
                }

            case "int16":
            case "int32":
            case "int64":
            case "decimal":
                {
                    if (Convert.ToBoolean(dc.ExtendedProperties["dropdownflag"]))
                        buildInput = string.Format("<td onchange=\"obJquerywindow.WhenChangeValue(event)\" style=\"width:40%;\"><select type=\"text\" placeholder=\"{1}\" class=\"form-control\">{0}</select></td>", BuildDropDown(dc), placeHoldersValue);
                    else
                        buildInput = string.Format("<td onkeyup=\"obJquerywindow.WhenChangeValue(event)\" obJquerywindow.WhenChangeValue(event)\" style=\"width:40%;\"><input id=\"singelNumber\" type=\"number\" placeholder=\"{0}\" class=\"form-control\" ></td>", placeHoldersValue);
                    break;
                }

            case "double":
                {
                    buildInput = string.Format("<td onkeyup=\"obJquerywindow.WhenChangeValue(event)\" style=\"width:40%;\"><input type=\"number\" placeholder=\"{0}\" class=\"form-control\" ></td>", placeHoldersValue);
                    break;
                }

            case "datetime":
                {
                    var dateFormat = Keys.GetUserPreferences.sPreferedDateFormat.ToString().Trim().ToUpper();
                    buildInput = string.Format("<td onchange=\"obJquerywindow.WhenChangeValue(event)\" style=\"width:40%;\"><input id=\"{0}\" placeholder=\"{1}\" autocomplete=\"off\" name=\"tabdatepicker\" class=\"form-control\" ></td>", HeaderId, dateFormat);
                    break;
                }

            default:
                {
                    break;
                }
        }
        return buildInput;
    }
    // Private Function ShowColumn(ByVal col As DataColumn) As Boolean
    // 'If CBoolean(col.ExtendedProperties("columnvisible")) = False Then Return False
    // Select Case CInt(col.ExtendedProperties("columnvisible"))
    // Case 3  'Not visible
    // Return False
    // Case 1  'Visible on level 1 only
    // 'If Me._LevelManager.CurrentLevel <> 0 Then Return False
    // Case 2  'Visible on level 2 and below only
    // 'If Me._LevelManager.CurrentLevel < 1 Then Return False
    // Case 4  'Smart column- not visible in a drill down when it's the parent.
    // If Me._Params.ParentField.ToLower = col.ColumnName.ToLower Then
    // Return False
    // End If
    // End Select
    // If col.ColumnName.ToLower = "formattedid" Then Return False
    // If col.ColumnName.ToLower = "pkey" Then Return False
    // If col.ColumnName.ToLower = "attachments" Then Return False
    // If col.ColumnName.ToLower = "rowtimestamp" Then Return False
    // If col.ColumnName.ToLower = "itemname" Then Return False
    // If col.ColumnName.ToLower = "dispositionstatus" Then Return False
    // 'If col.ColumnName.ToLower = "processeddescfieldnameone" Then Return False
    // 'If col.ColumnName.ToLower = "processeddescfieldnametwo" Then Return False
    // 'If col.ColumnName.ToLower = "rownum" Then Return False
    // Return True
    // End Function

    private StringBuilder BuildDropDown(DataColumn col1)
    {
        //DataTable col = (DataTable)col1;
        var count = ((DataTable)col1.ExtendedProperties["LookupData"]).Rows.Count;

        var listItem = new StringBuilder(count);
        listItem.Append("<option value=\"\"> </option>");
        foreach (DataRow row in ((DataTable)col1.ExtendedProperties["LookupData"]).Rows)
        {
            if (((DataTable)col1.ExtendedProperties["LookupData"]).Columns.Count > 1)
            {
                listItem.Append(string.Format("<option value=\"{0}\">{1}</option>", row["Value"].ToString(), row["Display"].ToString()));
            }
            else
            {
                listItem.Append(string.Format("<option value=\"{0}\">{1}</option>", row["Display"].ToString(), row["Display"].ToString()));
            }
        }
        return listItem;
    }

    //private StringBuilder BuildDropDown(object col1)
    //{
    //    var col = (DataColumn)col1;
    //    var adsfasdf = col("LookupData");
    //    var listItem = new StringBuilder(col["LookupData"]);
    //    listItem.Append("<option value=\"\"> </option>");
    //    foreach (DataRow row in (DataTable)col.ExtendedProperties["LookupData"].Rows)
    //    {
    //        if ((DataTable)col.ExtendedProperties("LookupData").Columns.Count > 1)
    //            listItem.Append(string.Format("<option value=\"{0}\">{1}</option>", row("Value").ToString, row("Display").ToString));
    //        else
    //            listItem.Append(string.Format("<option value=\"{0}\">{1}</option>", row("Display").ToString, row("Display").ToString));
    //    }
    //    return listItem;
    //}
    public void GetMyqueryList(int ceriteriaId)
    {
        IRepository<s_SavedCriteria> s_SavedCriteria;
        IRepository<s_SavedChildrenQuery> s_SavedChildrenQuery;
        s_SavedCriteria = new TabFusionRMS.RepositoryVB.Repositories<s_SavedCriteria>();
        s_SavedChildrenQuery = new TabFusionRMS.RepositoryVB.Repositories<s_SavedChildrenQuery>();

        var id = s_SavedCriteria.Where(a => a.UserId == _passport.UserId & a.Id == ceriteriaId).FirstOrDefault().Id;
        var getlist = s_SavedChildrenQuery.Where(a => a.SavedCriteriaId == id).ToList();
        int index = 0;
        foreach (var itm in getlist)
        {
            var myq = new Myquery.queryList();
            myq.ColumnType = listMyqueryDatatype[index];
            myq.columnName = itm.ColumnName;
            myq.operators = itm.Operator;
            myq.values = itm.CriteriaValue;
            index += 1;
            this.MyqueryList.Add(myq);
        }
    }
}

public class QueryHoldingProps
{
    public bool IsWhereClauseRequest
    {
        get;
        set;
    }
    public string WhereClauseStr
    {
        get;
        set;
    }
    public bool GsIsGlobalSearch
    {
        get;
        set;
    }
    public string GsSearchText
    {
        get;
        set;
    }
    public bool GsIncludeAttchment
    {
        get;
        set;
    }
    public bool GsIsAllGlobalRequest
    {
        get;
        set;
    }
    public string HoldTotalRowQuery
    {
        get;
        set;
    }
    public List<TableEditableHeader> EditableCells
    {
        get;
        set;
    }
}