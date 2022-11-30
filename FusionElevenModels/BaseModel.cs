using System.Collections.Generic;
using System.Text.Json.Serialization;
using Smead.RecordsManagement;
using Smead.Security;

public abstract class BaseModel
{
    [JsonIgnore]
    internal Passport? _passport { get; set; }
    [JsonIgnore]
    public HttpContext _httpContext { get; set; }
    public int errorNumber { get; set; }
    public string Msg { get; set; }
    public bool isError { get; set; } = false;
    public string TesterErrMsg { get; set; }
    public bool isWarning { get; set; }
    public bool isDuplicated { get; set; }
    public InternalEngine LinkScriptSession { get; set; }
    public int linkscriptSequence { get; set; }

}

public class DataProcessingModel
{
    public string FileName { get; set; }
    public int TaskType { get; set; }
    public int RecordCount { get; set; }
    public int viewId { get; set; }
    public bool Reconciliation { get; set; }
    public string Path { get; set; }
    public string ExportReportPath { get; set; }
    public string ErrorMessage { get; set; }
    public List<string> ListofselectedIds { get; set; }
    public bool IsSelectAllData { get; set; }
    public string DueBackDate { get; set; }
    public string DestinationTableName { get; set; }
    public string DestinationTableId { get; set; }
}