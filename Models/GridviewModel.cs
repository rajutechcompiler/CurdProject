
public partial class GridviewModel
{
    public GridviewModel()
    {
    }

    public string ErrorMessage { get; set; }
    public string jsonDataFields { get; set; }
    public bool chkIfAttachment { get; set; }

}

public partial class GridRowsCells
{
    public string tableRowsCellsdata { get; set; }
    public string textCell { get; set; }


}


public partial class GridHeaderCell
{
    public string tableHeaderCelldata { get; set; }
}