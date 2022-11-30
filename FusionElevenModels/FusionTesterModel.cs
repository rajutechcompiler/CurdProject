using Smead.RecordsManagement;
using TabFusionRMS.WebCS.FusionElevenModels;

public class FusionTesterModel
{
    public FusionTesterModel()
    {
    }
    public List<TableHeadersProperty> ListOfHeaders { get; set; }
    public List<List<string>> ListOfDatarows { get; set; }
    public List<RelationshipTable> RelationshipTable { get; set; }
    public List<Settings> ListOfSettings { get; set; }
    public List<UserSecureGroup> ListOfSecureGroups { get; set; }
    public bool HasAttachmentcolumn { get; set; }
    public bool HasDrillDowncolumn { get; set; }
    public bool ShowTrackableTable { get; set; }
    public int ViewId { get; set; }
    public string ViewName { get; set; }
    public string TableName { get; set; }
    public string messages { get; set; }
    public bool AuditOnOff { get; set; }
    public bool isError { get; set; }
    public string TesterErrMsg { get; set; }
    public List<Navigation.ListOfviews> ListOfviews { get; set; }
}


public class RelationshipTable
{
    public string UpperTableName { get; set; }
    public string UpperTableFieldName { get; set; }
    public string LowerTableName { get; set; }
    public string LowerTableFieldName { get; set; }
}

public class Settings
{
    public string Section { get; set; }
    public string Item { get; set; }
    public string ItemValue { get; set; }
}

public class UserSecureGroup
{

}