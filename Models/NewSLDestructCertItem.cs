using TabFusionRMS.Models;

public class NewSLDestructCertItem : SLDestructCertItem
{
    private string _holdType;
    public string HoldType
    {
        get
        {
            return _holdType;
        }
        set
        {
            _holdType = value;
        }
    }

    private string _snoozeDate;
    public string SnoozeDate
    {
        get
        {
            return _snoozeDate;
        }
        set
        {
            _snoozeDate = value;
        }
    }
}
