using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;
using Smead.RecordsManagement;
using static Smead.RecordsManagement.Navigation;
using Smead.Security;
using TabFusionRMS.WebCS.FusionElevenModels;
using System.Data.SqlClient;
public class Exporter : BaseModel
{
    public Exporter(Passport passport)
    {
        _passport = passport;
        Permission = true;
    }
    public string lblTitle { get; set; }
    public string HtmlMessage { get; set; }
    public bool isRequireBtn { get; set; }
    public bool IsBackgroundProcessing { get; set; }
    public bool Permission { get; set; }
}

public class ExporterJsonModel
{
    public string tableName { get; set; } = "";
    public string viewName { get; set; } = "";
    public int viewId { get; set; } = 0;
    public List<HeaderProps> Headers { get; set; } 
    public List<string> ListofselectedIds { get; set; }
    public List<List<string>> DataRows { get; set; }
    public bool IsBackgroundProcessing { get; set; } = false;
    public int TaskType { get; set; } = 0;
    public string FileName { get; set; }
    public int RecordCount { get; set; } = 0;
    public bool IsSelectAllData { get; set; } = false;
    public bool Reconciliation { get; set; } = false;
    public string ErrorMessage { get; set; }
    public string Path { get; set; }
    public string ExportReportPath { get; set; }
    public bool IsCSV { get; set; } = false;
    public int crumbLevel { get; set; } = 0;

}

public class ExporterJsonModelReqModel
{
    public ExporterJsonModel  paramss {get; set;}
}
public class ExporterReportJsonModel : ExporterJsonModel
{
    public TabFusionRMS.WebCS.AuditReportSearch.UIproperties ReportAuditFilterProperties { get; set; }
    public string ReportType { get; set; }
}

public class ExporterReportJsonModelReq
{
    public  ExporterReportJsonModel paramss  { get; set; }
}

public class HeaderProps
{
    public string ColumnName { get; set; }
    public string DataType { get; set; }
    public string HeaderName { get; set; }
}

public class ExportAll : BaseModel
{
    public ExportAll(Passport passport, ExporterJsonModel @params, string allquery, int curlevel, HttpContext httpContext)
    {
        _passport = passport;
        this.@params = @params;
        AllQuery = allquery;
        CurrentLevel = curlevel;
        _httpContext = httpContext;
    }
    public ExportAll(Passport passport, ExporterJsonModel @params, HttpContext httpContext)
    {
        _passport = passport;
        this.@params = @params;
        _httpContext = httpContext;
    }
    private ExporterJsonModel @params { get; set; }
    public string AllQuery { get; set; }
    public int CurrentLevel { get; set; }
    public StringBuilder BuildString()
    {
        var _query = new Query(_passport);
        var p = new Parameters(@params.viewId, _passport);
        p.Paged = false;
        p.RequestedRows = -1;
        string strQuery = NormalizeString(AllQuery);
        if (strQuery.ToUpper().Contains(" WHERE "))
        {
            string indx = strQuery.ToUpper().IndexOf(" WHERE ").ToString();
            strQuery = strQuery.Substring((int)Math.Round(Conversions.ToDouble(indx) + 7d));
            p.WhereClause = strQuery;
        }

        _query.FillData(p);
        // Dim viewcol As RecordsManage.ViewColumnsDataTable = Navigation.GetViewColumns(params.viewId, _passport)
        var sb = new StringBuilder();
        BuildHeaderAll(sb, p);
        sb.Append(Environment.NewLine);
        BuildRowsAll(sb, p);
        return sb;
    }
    private void BuildHeaderAll(StringBuilder sb, Parameters p)
    {
        int counter = 0;
        foreach (DataColumn head in p.Data.Columns)
        {
            counter = counter + 1;
            if (GridDataBinding.ShowColumn(head, CurrentLevel, p.ParentField))
            {
                if (counter == p.Data.Columns.Count)
                {
                    sb.Append(head.ExtendedProperties["heading"].ToString());
                }
                else
                {
                    sb.Append(head.ExtendedProperties["heading"].ToString() + ",");
                }
            }
        }
    }
    private void BuildRowsAll(StringBuilder sb, Parameters p)
    {
        int counter = 0;
        foreach (DataRow row in p.Data.Rows)
        {
            foreach (DataColumn col in p.Data.Columns)
            {
                counter = counter + 1;
                if (GridDataBinding.ShowColumn(col, CurrentLevel, p.ParentField))
                {
                    string fieldData = string.Empty;
                    string fieldName = col.ColumnName;
                    if (string.IsNullOrEmpty(row[fieldName].ToString()))
                    {
                        fieldData = "";
                    }
                    else
                    {
                        fieldData = Conversions.ToString(row[fieldName]);
                    }

                    fieldData = CheckDataType(col, row, fieldName);

                    if (fieldData.Contains(","))
                    {
                        fieldData = fieldData.Replace(",", "");
                    }
                    FillDateInRow(counter, p.Data.Columns.Count, fieldData, sb);

                }
            }
            sb.Append(Environment.NewLine);
            counter = 0;
        }

    }
    private string CheckDataType(object col, DataRow row, string fieldName)
    {
        var datatype = row[fieldName].GetType();
        if (datatype.Name == "DateTime")
        {
            return Keys.get_ConvertCultureDate(row[fieldName].ToString(), _httpContext, bDetectTime: true);
        }
        if (datatype.Name == "Boolean")
        {
            if (Conversions.ToBoolean(row[fieldName]))
            {
                return "1";
            }
            else
            {
                return "0";
            }
        }
        return row[fieldName].ToString();
    }
    private void FillDateInRow(int counter, int colcount, string fieldData, StringBuilder sb)
    {
        if (counter == colcount)
        {
            sb.Append(fieldData);
        }
        else
        {
            sb.Append(fieldData + ",");
        }

        // If counter = viewcol.Rows.Count Then
        // sb.Append(fieldData)
        // Else
        // sb.Append(fieldData & ",")
        // End If
    }
    private string GetTablesJoinKeys(string upperTable, string LowerTable)
    {
        var td = new DataTable();
        string command = string.Format("select * from RelationShips where UpperTableName = '{0}' and LowerTableName = '{1}'", upperTable, LowerTable);
        using (var conn = _passport.Connection())
        {
            using (var cmd = new SqlCommand(command, conn))
            {
                using (var adp = new SqlDataAdapter(cmd))
                {
                    adp.Fill(td);
                }
            }
        }
        return td.Rows[0]["LowerTableFieldName"].ToString().Split('.')[1] + "||" + td.Rows[0]["UpperTableFieldName"].ToString().Split('.')[1];
    }
    private string GetLookupValue(string upperTable, string upperTableFieldName, string upperkey, string value)
    {
        var td = new DataTable();
        string command = string.Format("select [{3}] from {0} where {1} =  '{2}'", upperTable, upperkey, value, upperTableFieldName);
        using (var conn = _passport.Connection())
        {
            using (var cmd = new SqlCommand(command, conn))
            {
                using (var adp = new SqlDataAdapter(cmd))
                {
                    adp.Fill(td);
                }
            }
        }
        if (td.Rows.Count == 0)
        {
            return "";
        }
        else
        {
            return td.Rows[0].ItemArray[0].ToString();
        }
    }
}