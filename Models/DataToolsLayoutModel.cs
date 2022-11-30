using System.Collections.Generic;

public partial class DataToolsLayoutModel
{
    public DataToolsLayoutModel()
    {
        ListLocalization = new List<Localizations>();
        objLocalization = new Localizations();
    }
    public string Title { get; set; }
    public List<Localizations> ListLocalization { get; set; }
    public Localizations objLocalization { get; set; }
}

public partial class ChangePassword
{
    public string OldPass { get; set; }
    public string NewPass1 { get; set; }
    public string NewPass2 { get; set; }
    public string errorMessage { get; set; }
}

public partial class Localizations
{
    public Localizations()
    {

    }
    public Localizations(string LookupTypeValue, string LookupTypeCode)
    {
        this.LookupTypeValue = LookupTypeValue;
        this.LookupTypeCode = LookupTypeCode;
    }
    public string LookupTypeValue { get; set; }
    public string LookupTypeCode { get; set; }
    public string pLocData { get; set; }
    public object resouceObjectLenguage { get; set; }
    public string SelectedCountry { get; set; }
    public string LanguageSelected { get; set; }
    public string dateFormatSelected { get; set; }
}