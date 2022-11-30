using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Smead;
using Smead.RecordsManagement;
using Smead.Security;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.Resource;
using TabFusionRMS.WebCS;
using TabFusionRMS.WebCS.FusionElevenModels;

public partial class DashboardModel : BaseModel
{
    public TasksBar Taskbar { get; set; }
    public string DashboardListHtml { get; set; }
    public string DashboardListJsonS { get; set; }
    public string LanguageCulture { get; set; }

    private List<SLUserDashboard> DashboardList;
    public string ErrorMessage { get; set; }

    public DashboardModel(Passport passport, HttpContext httpContext)
    {
        this._passport = passport;
        Taskbar = new TasksBar(this._passport);
        _httpContext = httpContext;
    }


    public void ExecuteDashboard()
    {
        GetDashboardList();
        GetDashboardHtml();
        GetClientResourceKeys();
        // Me.GetDashboardJsonString()
    }

    public void ExecuteDashboardList()
    {
        GetDashboardList();
        GetDashboardHtml();
    }

    private void GetDashboardHtml()
    {
        var htmlDashboard = new StringBuilder();
        foreach (var item in this.DashboardList)
        {
            if (Convert.ToBoolean(item.IsFav))
            {
                htmlDashboard.Append(string.Format("<li class='hasSubs' dashbard-id='{0}'><a><xmp class='xmpDashbaordNames'>{1}</xmp></a><i class='fa fa-star staricon' aria-hidden='true'></i></li>", item.ID, item.Name));
            }
            else
            {
                htmlDashboard.Append(string.Format("<li class='hasSubs' dashbard-id='{0}'><a><xmp class='xmpDashbaordNames'>{1}</xmp></a></li>", item.ID, item.Name));
            }
        }
        this.DashboardListHtml = htmlDashboard.ToString();
    }

    private void GetClientResourceKeys()
    {
        var pCurrentCulture = Keys.GetCultureCookies;
        var resouceObject = Languages.get_GetValuesByModule(Languages.ResModule.clients, pCurrentCulture(_httpContext).Name);

        // #Region "Add keys in Dictionary"
        var resourceSetCommon = new Dictionary<string, string>();
        resourceSetCommon = Languages.get_GetValuesByModule(Languages.ResModule.dashboard, pCurrentCulture(_httpContext).Name);

        foreach (var resource in resourceSetCommon)
        {
            if (!resouceObject.ContainsKey(resource.Key))
            {
                resouceObject.Add(resource.Key, resource.Value);
            }
        }
        resouceObject.Add("cultureKey", pCurrentCulture(_httpContext).Name);
        resouceObject.Add("currentUserName", Keys.CurrentUserName.ToString());
        LanguageCulture = JsonConvert.SerializeObject(resouceObject);
    }

    private void GetDashboardJsonString()
    {
        this.DashboardListJsonS = JsonConvert.SerializeObject(this.DashboardList.Select(x => new DashboardJsonModel() { ID = x.ID, Name = x.Name, Json = x.Json }));
    }

    private void GetDashboardList()
    {
        var obj = new DashboardDataModel(_passport, _httpContext);
        var lis = obj.GetDashbaordList();
        DashboardList = lis;
    }

}

public partial class DashboardJsonModel
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Json { get; set; }
}
public partial class DashboardDropdown : BaseModel
{
    public DashboardDropdown()
    {
        WorkGroup = new List<WorkGroupItem>();
        Table = new List<TableItem>();
    }
    public List<WorkGroupItem> WorkGroup;
    public List<TableItem> Table;
    public List<ViewItem> View;
}

public partial class CommonModel
{
    public int Id;
    public string Name;
    public string UserName;
}


public partial class CommonDropdown : CommonModel
{
    public string SId;
    public string FieldName;
}

public partial class ViewModal : CommonModel
{
}

public partial class ColumnModel : CommonModel
{

}

public partial class DashboardDataModel : BaseModel
{
    public string ViewId;
    public string FieldName;
    public string TableName;
    private string Query;
    private HttpContext _httpContext;

    private string ChartQuery = "select [FieldName] As [X],  count(*) as [Y] from [TableName] Group by [FieldName]";
    private string ChartQueryCount = "select count(*) as [Y] from [TableName]";

    // Private PeriodQuery = "DECLARE @currentDate DATE,@fromdate DATE, @todate DATE, @period INT
    // SELECT @currentDate = GETDATE(), @period = [Period]
    // IF(@period = 1) SELECT @fromdate = CONVERT(varchar, DATEADD(DAY,-14, @currentDate), 1), @todate = CONVERT(varchar, DATEADD(DAY,-7, @currentDate), 1)
    // ELSE IF(@period = 2) SELECT @fromdate = CONVERT(varchar, DATEADD(MONTH,-2, @currentDate), 1),  @todate = CONVERT(varchar, DATEADD(MONTH,-1, @currentDate), 1)
    // ELSE SELECT @fromdate = CONVERT(varchar, DATEADD(MONTH, -6, @currentDate), 1), @todate = CONVERT(varchar, DATEADD(MONTH, -3, @currentDate), 1)                 "

    private string PeriodQuery = @"DECLARE @currentDate date,@fromdate date,@todate date,@period int
                           SELECT @currentDate = GETDATE(),@period = [Period]
                           IF (@period = 1) SELECT @fromdate = CONVERT(varchar, DATEADD(DAY, -6, @currentDate), 1),@todate = @currentDate
                           ELSE IF (@period = 2) Begin SET DATEFIRST 1 SELECT @fromdate = CONVERT(varchar, DATEADD(DAY, -29, @currentDate), 1), @todate = @currentDate End
                           ELSE SELECT @fromdate = CONVERT(varchar, DATEADD(DAY, -89, @currentDate), 1), @todate = @currentDate";


    private string TimeSeriesChartQuery;

    private string TimeSeriesChartQueryCount;

    private string OperationChartQuery;

    private string OperationChartQueryWeek;

    private string OperationChartQueryCount;

    private string TrackedChartQuery;

    private string TrackedChartQueryWeek;


    private string TrackedChartQueryCount;

    private string GridQuery = "select [Fields] from [TableName]";

    private string DashboardInsertQ = @"Declare @InsertedId table(Id int)
                                          Insert Into SLUserDashboard (Name, UserID, Json) output Inserted.Id into @InsertedId  values(@Name, @UserID, @Json)  
                                          select Id from @InsertedId";

    private string DashboardUpdateJsonQ = @"Declare @UpdatedId table(Id int)
                                              update SLUserDashboard set Json = @Json output Inserted.Id into @UpdatedId where Id = @DashboardId
                                              select Id from @UpdatedId";

    private string DashboardUpdateNameQ = @"Declare @UpdatedId table(Id int)
                                              update SLUserDashboard set Name = @Name output Inserted.Id into @UpdatedId where Id = @DashboardId
                                              select Id from @UpdatedId";

    private string DashboardGetIdBaseQ = "select ID, Name, Json, IsFav from SLUserDashboard where ID = @DashboardId";

    private string DashboardGetListQ = "select ID, Name, Json, isnull(IsFav, 0) as IsFav from SLUserDashboard where UserID=@UserId order by isnull(IsFav, 0) desc, Name";

    private string DashboardDeleteQ = @"Declare @InsertedId table(Id int)
                                          Delete SLUserDashboard output Deleted.Id into @InsertedId  where ID = @DashboardId  
                                          select Id from @InsertedId";

    private string TrackingTableQ = @"select t.TableId, t.TableName,  t.UserName from tables as t
                                        inner join vwTablesAll as v
                                        on t.TableName = v.TABLE_NAME COLLATE Latin1_General_CI_AS
                                        where t.trackable = 1";

    private string AuditTableQ = @"select t.TableId, t.TableName,  t.UserName from tables as t
                                        inner join vwTablesAll as v
                                        on t.TableName = v.TABLE_NAME COLLATE Latin1_General_CI_AS
                                        where t.AuditUpdate = 1";

    private string ViewColumnQ = @"select Id, Heading as Name, FieldName, ViewsId, ColumnNum from ViewColumns where viewsid = @ViewId
                                     order by ColumnNum";

    private string UserListQ = "select UserId As Id, UserName As SID, FullName as Name from SecureUser where UserId > 0";

    private string TableNameQ = "select TableName from tables where tableId in ([TableIds])";
    public List<CommonDropdown> ViewColumnEntity { get; set; }
    public DashboardDataModel(Passport passport, HttpContext httpContext)
    {

        TimeSeriesChartQuery = PeriodQuery + @" SELECT [Filter] AS [X], COUNT(*) AS [Y] FROM [TableName] WHERE 
                                                            [FieldName] >= @fromdate AND [FieldName] <= @todate GROUP BY [Filter]";
        TimeSeriesChartQueryCount = PeriodQuery + @" SELECT COUNT(*) AS [Y] FROM [TableName] WHERE 
                                                                [FieldName] >= @fromdate AND [FieldName] <= @todate ";
        OperationChartQuery = PeriodQuery + @" SELECT  [Filter] AS [X], COUNT(*)  AS [Y], sl.actiontype AS AuditType FROM SLAuditUpdates  AS sl INNER JOIN Tables AS t
                                            ON sl.TableName = t.TableName WHERE t.TableId IN ([TableIds]) AND sl.OperatorsId IN ([UserIds]) [AuditType]
                                            AND (CONVERT(varchar , UpdateDateTime, 1) >= @fromdate AND CONVERT(varchar , UpdateDateTime, 1) <= @todate) 
                                            GROUP BY [Filter] , sl.ActionType order by [Filter] desc";
        OperationChartQueryWeek = PeriodQuery + @" Declare @temptbl table (X nvarchar(12), Y int, WK int, AuditType int);
                                                Insert Into @temptbl(X,Y,WK, AuditType)
                                                SELECT
                                                  CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday,UpdateDateTime)+1,UpdateDateTime), 6)  AS X,
                                                  COUNT(*) AS [Y],
                                                  DATEPART(WEEK,UpdateDateTime) AS WK,
                                                  sl.actiontype AS AuditType
                                                FROM
                                                  SLAuditUpdates AS sl
                                                  INNER JOIN Tables AS t ON sl.TableName = t.TableName
                                                WHERE
                                                  t.TableId IN ([TableIds])
                                                  AND sl.OperatorsId IN ([UserIds])
                                                  [AuditType] 
                                                  AND (
                                                    CONVERT(varchar, UpdateDateTime, 1) >= @fromdate
                                                    AND CONVERT(varchar, UpdateDateTime, 1) <= @todate
                                                  )
                                                GROUP BY
                                                  CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday,UpdateDateTime)+1,UpdateDateTime), 6),
                                                  DATEPART(WEEK,UpdateDateTime),
                                                  sl.actiontype
                                                order by
                                                  DATEPART(WEEK,UpdateDateTime) desc
                                                
                                                
                                                Declare @WeekFrom nvarchar(12) = CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday,@fromdate)+1,@fromdate), 6)
                                                Declare @WeekTo nvarchar(12) = CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday,@todate)+1,@todate), 6)
                                                                                             
                                                Select  Case X  when @WeekFrom  then  CONVERT(nvarchar, @fromdate, 6) 
                                                when @WeekTo then CONVERT(nvarchar, @todate, 6) else X end As X, Y, AuditType
                                                from @temptbl order by WK desc";

        OperationChartQueryCount = PeriodQuery + @" SELECT  COUNT(*)  AS [Y] FROM SLAuditUpdates  AS sl INNER JOIN Tables AS t ON sl.TableName = t.TableName WHERE t.TableId IN ([TableIds]) AND sl.OperatorsId IN ([UserIds]) [AuditType]
                                            AND (CONVERT(varchar , UpdateDateTime, 1) >= @fromdate AND CONVERT(varchar , UpdateDateTime, 1) <= @todate) ";

        TrackedChartQuery = PeriodQuery + @" SELECT [Filter] AS X, COUNT(*) AS Y FROM TrackingHistory  AS th
                                           INNER JOIN tables t ON t.tableName = th.TrackedTable WHERE t.tableId IN ([TableIds]) AND 
                                           (CONVERT(varchar , TransactionDateTime, 1) >= @fromdate AND CONVERT(varchar , TransactionDateTime, 1) <= @todate)
                                           GROUP BY [Filter] order by [Filter] desc";
        TrackedChartQueryWeek = PeriodQuery + @" Declare @temptbl table (X nvarchar(12), Y int, WK int);
                                             Insert Into @temptbl(X,Y,WK)
                                             Select
                                               CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday,TransactionDateTime)+1,TransactionDateTime), 6)  AS X,
                                               COUNT(*) AS Y, DATEPART(WEEK,TransactionDateTime) AS WK
                                             From TrackingHistory AS th INNER JOIN tables t ON t.tableName = th.TrackedTable
                                             Where t.tableId IN ([TableIds])
                                               AND ( CONVERT(varchar, TransactionDateTime, 1) >= @fromdate AND CONVERT(varchar, TransactionDateTime, 1) <= @todate )
                                             Group By CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday,TransactionDateTime)+1,TransactionDateTime), 6), DATEPART(WEEK,TransactionDateTime)
                                             order by DATEPART(WEEK,TransactionDateTime) desc
                                             
                                             Declare @WeekFrom nvarchar(12) = CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday,@fromdate)+1,@fromdate), 6)
                                             Declare @WeekTo nvarchar(12) = CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday,@todate)+1,@todate), 6)
                                             
                                             Select  Case X  when @WeekFrom  then  CONVERT(nvarchar, @fromdate, 6) 
                                             		when @WeekTo then CONVERT(nvarchar, @todate, 6) else X end As X, Y
                                             from @temptbl order by WK desc";
        TrackedChartQueryCount = PeriodQuery + " SELECT  COUNT(*) AS Y FROM TrackingHistory  AS th   INNER JOIN tables t ON t.tableName = th.TrackedTable WHERE t.tableId IN ([TableIds]) AND (CONVERT(varchar , TransactionDateTime, 1) >= @fromdate AND CONVERT(varchar , TransactionDateTime, 1) <= @todate) ";
        this._passport = passport;
        _httpContext = httpContext;
        ViewColumnEntity = new List<CommonDropdown>();
    }
    public string GetDashboardListString()
    {
        var model1 = new DashboardModel(_passport, _httpContext);
        model1.ExecuteDashboardList();
        return model1.DashboardListHtml;
    }
    public List<ChartModel> GetBarPieChartData(string tableName, object columnName)
    {

        bool isDateTime = CheckIsDatetimeColumn(Convert.ToString(tableName), Convert.ToString(columnName));

        string col = AddSquareBracket(Convert.ToString(columnName));
        if (isDateTime == true)
        {
            // Convert(nvarchar (12), DateHired, 111 )
            this.Query = ChartQuery.Replace("[FieldName]", " Convert(nvarchar (12), " + columnName + ", 111 ) ").Replace("[TableName]", tableName);
        }
        else
        {
            Query = ChartQuery.Replace("[FieldName]", col).Replace("[TableName]", tableName);
        }
        var dt = ExecureSelectQuery(Query);
        if (dt.Rows.Count > 0)
        {
            var stringObj = JsonConvert.SerializeObject(dt);
            return JsonConvert.DeserializeObject<List<ChartModel>>(stringObj).ToList();
        }
        else
        {
            return new List<ChartModel>();
        }
    }

    public List<ChartModel> GetBarPieChartData(string tableName, int viewId, string columnName)
    {

        var _query = new Query(_passport);
        var @params = new Parameters(viewId, _passport);
        @params.fromChartReq = true;
        @params.IsMVCCall = true;
        _query.RefineSQL(@params);

        bool isDateTime = CheckIsDatetimeColumn(Convert.ToString(tableName), Convert.ToString(columnName));

        if (columnName.ToString().Contains(tableName))
        {
            columnName = "tbl.[" + columnName.ToString().Split(".")[1] + "]";
        }
        else
        {
            columnName = "tbl.[" + columnName + "]";
        }

        if (isDateTime)
        {
            columnName = "Convert(nvarchar (12), " + columnName + ", 111 )";
        }

        var sql = "select " + columnName + " As [X],  count(*) as [Y] from (" + @params.SQL + ") as tbl Group by " + columnName;


        var dt = ExecureSelectQuery(Convert.ToString(sql));


        if (dt.Rows.Count > 0)
        {
            string stringObj = JsonConvert.SerializeObject(dt);
            return JsonConvert.DeserializeObject<List<ChartModel>>(stringObj).ToList();
        }
        else
        {
            return new List<ChartModel>();
        }

    }

    public int GetBarPieChartDataCount(string tableName, string columnName)
    {
        string col = AddSquareBracket(Convert.ToString(columnName));
        Query = ChartQueryCount.Replace("[FieldName]", col).Replace("[TableName]", Convert.ToString(tableName));
        var dt = ExecureSelectQuery(Query);
        if (dt.Rows.Count > 0)
        {
            return dt.Rows.Count;
        }
        else
        {
            return 0;
        }
    }

    private string AddSquareBracket(string column)
    {
        string col;
        string[] strArr;
        strArr = column.Split(".");

        if (strArr.Length > 1)
        {
            col = "[" + strArr[0] + "].[" + strArr[1] + "]";
        }
        else
        {
            col = "[" + strArr[0] + "]";
        }
        return col;
    }

    public List<TableModel> GetTableNames(object tableIds)
    {
        Query = TableNameQ.Replace("[TableIds]", Convert.ToString(tableIds));
        var dt = ExecureSelectQuery(Query);
        if (dt.Rows.Count > 0)
        {
            var stringObj = JsonConvert.SerializeObject(dt);
            return JsonConvert.DeserializeObject<List<TableModel>>(stringObj).ToList();
        }
        else
        {
            return new List<TableModel>();
        }
    }

    public List<ChartModel> GetTimeSeriesChartData(string tableName, string columnName, string period, string filter)
    {
        Query = TimeSeriesChartQuery.Replace("[FieldName]", Convert.ToString(columnName)).Replace("[TableName]", Convert.ToString(tableName)).Replace("[Period]", Convert.ToString(period));

        if (filter.Equals("hour"))
        {
            this.Query = Query.Replace("[Filter]", "Convert(varchar , " + columnName + ", 1) + ' ' +cast(datepart(hour," + columnName + ") as nvarchar) + ':00'");
        }
        else if (filter.Equals("day"))
        {
            this.Query = Query.Replace("[Filter]", "CONVERT(varchar , " + columnName + ", 1)");
        }
        else if (filter.Equals("week"))
        {
            this.Query = Query.Replace("[Filter]", "cast(month(" + columnName + ") as nvarchar) + '/' +  cast(year(" + columnName + ") as nvarchar)");
        }
        else
        {
            this.Query = Query.Replace("[Filter]", "cast(month(" + columnName + ") as nvarchar) + '/' +  cast(year(" + columnName + ") as nvarchar)");
        }

        var dt = ExecureSelectQuery(Query);
        if (dt.Rows.Count > 0)
        {
            var stringObj = JsonConvert.SerializeObject(dt);
            return JsonConvert.DeserializeObject<List<ChartModel>>(stringObj).ToList();
        }
        else
        {
            return new List<ChartModel>();
        }
    }

    public List<ChartModel> GetTimeSeriesChartData(string tableName, string columnName, int viewId, string period, string filter)
    {

        string filterSql = "";
        var _query = new Query(_passport);
        var @params = new Parameters(viewId, _passport);
        @params.fromChartReq = true;
        @params.IsMVCCall = true;
        _query.RefineSQL(@params);

        if (columnName.ToString().Contains(tableName))
        {
            columnName = "[" + columnName.ToString().Split(".")[1] + "]";
        }

        if (filter.Equals("hour"))
        {
            filterSql = "Convert(varchar , " + columnName + ", 1) + ' ' +cast(datepart(hour," + columnName + ") as nvarchar) + ':00'";
        }
        else if (filter.Equals("day"))
        {
            filterSql = "CONVERT(varchar , " + columnName + ", 1)";
        }
        // ElseIf filter.Equals("week") Then
        // filterSql = "DATENAME(week," + columnName + ") + 'th week/' + DATENAME(YEAR," + columnName + ")"
        else
        {
            filterSql = "cast(month(" + columnName + ") as nvarchar) + '/' + cast(year(" + columnName + ") as nvarchar)";
        }

        PeriodQuery = PeriodQuery.Replace("[Period]", period.ToString());
        string query = "";
        if (filter.Equals("week"))
        {

            query = PeriodQuery + @" Declare @temptbl table (X nvarchar(12), Y int, WK int);
                                        Insert Into @temptbl(X,Y,WK)
                                        SELECT CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday," + columnName + ")+1," + columnName + "), 6)  AS [X], COUNT(*) AS [Y],DATEPART(WEEK," + columnName + ") AS WK FROM (" + @params.SQL + @") as tbl 
                                        WHERE " + columnName + @">=@fromdate 
                                        AND " + columnName + "<= @todate GROUP BY CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday," + columnName + ")+1," + columnName + "), 6),DATEPART(WEEK," + columnName + ") order by DATEPART(WEEK," + columnName + @") desc
                                        Declare @WeekFrom nvarchar(12) = CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday,@fromdate)+1,@fromdate), 6)
                                        Declare @WeekTo nvarchar(12) = CONVERT(nvarchar, DATEAdd(day, -DATEPART(weekday,@todate)+1,@todate), 6)
                                                                                     
                                        Select  Case X  when @WeekFrom  then  CONVERT(nvarchar, @fromdate, 6) 
                                        when @WeekTo then CONVERT(nvarchar, @todate, 6) else X end As X, Y
                                        from @temptbl order by WK desc";


        }
        else
        {
            query = PeriodQuery + " SELECT " + filterSql + " AS [X], COUNT(*) AS [Y] FROM (" + @params.SQL + @") as tbl 
                                    WHERE " + columnName + @">=@fromdate 
                                    AND " + columnName + "<= @todate GROUP BY " + filterSql + " order by " + filterSql + " asc";
        }

        var dt = this.ExecureSelectQuery(query);
        if (dt.Rows.Count > 0)
        {
            var stringObj = JsonConvert.SerializeObject(dt);
            return JsonConvert.DeserializeObject<List<ChartModel>>(stringObj).ToList();
        }
        else
        {
            return new List<ChartModel>();
        }
    }

    public int GetTimeSeriesChartDataCount(string tableName, string columnName, string period, string filter)
    {
        this.Query = TimeSeriesChartQueryCount.Replace("[FieldName]", "[" + columnName + "]").Replace("[TableName]", "[" + tableName + "]").Replace("[Period]", period);

        //Query = TimeSeriesChartQueryCount.Replace("[Filter]", "cast(month(UpdateDateTime) as nvarchar) + '/' +  cast(year(UpdateDateTime) as nvarchar)");
        var dt = ExecureSelectQuery(Query);
        if (dt.Rows.Count > 0)
        {
            return dt.Rows.Count;
        }
        else
        {
            return 0;
        }
    }

    public bool CheckDashboardNameDuplicate(int DId, string Name)
    {
        Query = "select * from SLUserdashboard Where name = @name and Id <> @id and UserId = @userid";
        var p = new List<SqlParameter>();
        p.Add(new SqlParameter() { ParameterName = "@name", Value = Name });
        p.Add(new SqlParameter() { ParameterName = "@id", Value = DId });
        p.Add(new SqlParameter() { ParameterName = "@userid", Value = _passport.UserId });
        var dt = ExecuteSelectQuery(Query, p);

        if (dt.Rows.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public List<ChartOperatinModelRes> GetOperationChartData(string tableIds, string usersIds, string AuditTypeIds, string period, string filter)
    {
        string AuditTypeQuery = "";
        if (!string.IsNullOrEmpty(Convert.ToString(AuditTypeIds)))
        {
            AuditTypeQuery = " and sl.ActionType in (" + AuditTypeIds + ") ";
        }

        if (filter.Equals("hour"))
        {
            this.Query = OperationChartQuery.Replace("[TableIds]", tableIds).Replace("[UserIds]", usersIds).Replace("[AuditType]", AuditTypeQuery).Replace("[Period]", period);
            this.Query = this.Query.Replace("[Filter]", "Convert(varchar , UpdateDateTime, 1) + ' ' +cast(datepart(hour,UpdateDateTime) as nvarchar) + ':00'");
        }
        else if (filter.Equals("day"))
        {
            this.Query = OperationChartQuery.Replace("[TableIds]", tableIds).Replace("[UserIds]", usersIds).Replace("[AuditType]", AuditTypeQuery).Replace("[Period]", period);
            this.Query = this.Query.Replace("[Filter]", "CONVERT(varchar , UpdateDateTime, 1)");
        }
        else if (filter.Equals("week"))
        {
            // Me.Query = Me.Query.Replace("[Filter]", "DATENAME(week,UpdateDateTime) + 'th week/' + DATENAME(YEAR,UpdateDateTime)")
            this.Query = OperationChartQueryWeek.Replace("[TableIds]", tableIds).Replace("[UserIds]", usersIds).Replace("[AuditType]", AuditTypeQuery).Replace("[Period]", period);
        }
        else
        {
            this.Query = OperationChartQuery.Replace("[TableIds]", tableIds).Replace("[UserIds]", usersIds).Replace("[AuditType]", AuditTypeQuery).Replace("[Period]", period);
            this.Query = this.Query.Replace("[Filter]", "cast(month(UpdateDateTime) as nvarchar) + '/' +  cast(year(UpdateDateTime) as nvarchar)");
        }

        var dt = this.ExecureSelectQuery(this.Query);
        if (dt.Rows.Count > 0)
        {
            var auditTypelst = new Auditing().GetAuditTypeList();
            var stringObj = JsonConvert.SerializeObject(dt);
            var res = JsonConvert.DeserializeObject<List<ChartOperatinModel>>(stringObj).ToList();
            var xValue = (from i in res
                          select i.X).Distinct().ToList();
            var cAuditType = (from i in res
                              select i.AuditType).Distinct().ToList();
            var cAuditTypelst = (from c in cAuditType
                                 join au in auditTypelst on c equals au.Value
                                 select new EnumModel() { Value = c, Name = au.Name }).Distinct().ToList();

            var retChart = new List<ChartOperatinModelRes>();
            for (int a = 0; a <= cAuditTypelst.Count()-1; a++)
            {
                var firstStact = new ChartOperatinModelRes();
                firstStact.AuditType = cAuditTypelst[a].Name;
                for (int x = 0; x <= xValue.Count()-1; x++)
                {
                    var chartData = new ChartModel();
                    chartData.X = xValue[x];
                    chartData.Y = 0;
                    firstStact.Data.Add(chartData);
                    foreach (ChartOperatinModel item in res)
                    {
                        if (item.X == xValue[x] & item.AuditType == cAuditTypelst[a].Value)
                        {
                            firstStact.Data[x].Y = item.Y;
                        }
                    }
                }
                retChart.Add(firstStact);
            }
            return retChart;
        }
        else
        {
            return new List<ChartOperatinModelRes>();
        }
    }

    public int GetOperationChartDataCount(string tableIds, string usersIds, string AuditTypeIds, string period, string filter)
    {
        string AuditTypeQuery = "";
        if (!string.IsNullOrEmpty(Convert.ToString(AuditTypeIds)))
        {
            AuditTypeQuery = " and sl.ActionType in (" + AuditTypeIds + ") ";
        }

        this.Query = OperationChartQueryCount.Replace("[TableIds]", Convert.ToString(tableIds)).Replace("[UserIds]", Convert.ToString(usersIds)).Replace("[AuditType]", AuditTypeQuery).Replace("[Period]", Convert.ToString(period));


        var dt = this.ExecureSelectQuery(this.Query);
        if (dt.Rows.Count > 0)
        {
            return dt.Rows.Count;
        }
        else
        {
            return 0;
        }

    }


    public List<ChartModel> GetTrackedChartData(object tableIds, object filter, object period)
    {
        if (filter.Equals("hour"))
        {
            Query = TrackedChartQuery.Replace("[Filter]", "Convert(varchar , TransactionDateTime, 1) + ' ' +cast(datepart(hour,TransactionDateTime)as nvarchar) + ':00'").Replace("[TableIds]", Convert.ToString(tableIds)).Replace("[Period]", Convert.ToString(period));
        }
        else if (filter.Equals("day"))
        {
            Query = TrackedChartQuery.Replace("[Filter]", "CONVERT(varchar , TransactionDateTime, 1)").Replace("[TableIds]", Convert.ToString(tableIds)).Replace("[Period]", Convert.ToString(period));
        }
        else if (filter.Equals("week"))
        {
            // Me.Query = Me.TrackedChartQuery.Replace("[Filter]", "DATENAME(week,TransactionDateTime) + 'th week/' + DATENAME(YEAR,TransactionDateTime)").Replace("[TableIds]", tableIds).Replace("[Period]", period)
            Query = TrackedChartQueryWeek.Replace("[TableIds]", Convert.ToString(tableIds)).Replace("[Period]", Convert.ToString(period));
        }
        else
        {
            Query = TrackedChartQuery.Replace("[Filter]", "cast(month(TransactionDateTime) as nvarchar) + '/' +  cast(year(TransactionDateTime) as nvarchar)").Replace("[TableIds]", Convert.ToString(tableIds)).Replace("[Period]", Convert.ToString(period));
        }

        var dt = ExecureSelectQuery(Query);
        if (dt.Rows.Count > 0)
        {
            var stringObj = JsonConvert.SerializeObject(dt);
            return JsonConvert.DeserializeObject<List<ChartModel>>(stringObj).ToList();
        }
        else
        {
            return new List<ChartModel>();
        }
    }

    public int GetTrackedChartDataCount(string tableIds, string filter, string period)
    {
        Query = TrackedChartQueryCount.Replace("[TableIds]", Convert.ToString(tableIds)).Replace("[Period]", Convert.ToString(period));
        var dt = ExecureSelectQuery(Query);
        if (dt.Rows.Count > 0)
        {
            return dt.Rows.Count;
        }
        else
        {
            return 0;
        }
    }

    // this function are using for create new dashbaord and it is return inserted Dashboard Id 
    public int InsertDashbaord(string Name, string Json)
    {
        var data = new DataTable();
        int DashboardId = 0;
        using (SqlConnection conn = _passport.Connection())
        {
            using (var cmd = new SqlCommand(DashboardInsertQ, conn))
            {
                cmd.Parameters.AddWithValue("@Name", Name);
                cmd.Parameters.AddWithValue("@UserID", this._passport.UserId);
                cmd.Parameters.AddWithValue("@Json", Json);
                // cmd.ExecuteNonQuery()
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(data);
                }
            }
        }

        if (data.Rows.Count > 0)
        {
            DashboardId = Convert.ToInt16(data.Rows[0][0].ToString());
        }

        return DashboardId;

    }

    public int UpdateDashbaordName(string Name, int Id)
    {
        var data = new DataTable();
        int DashboardId = 0;
        using (SqlConnection conn = _passport.Connection())
        {
            using (var cmd = new SqlCommand(DashboardUpdateNameQ, conn))
            {
                cmd.Parameters.AddWithValue("@Name", Name);
                cmd.Parameters.AddWithValue("@DashboardId", Id);
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(data);
                }
            }
        }

        if (data.Rows.Count > 0)
        {
            DashboardId = Convert.ToInt16(data.Rows[0][0].ToString());
        }

        return DashboardId;
    }

    public int UpdateDashbaordJson(string Json, int Id)
    {
        var data = new DataTable();
        int DashboardId = 0;
        using (SqlConnection conn = _passport.Connection())
        {
            using (var cmd = new SqlCommand(DashboardUpdateJsonQ, conn))
            {
                cmd.Parameters.AddWithValue("@Json", Json);
                cmd.Parameters.AddWithValue("@DashboardId", Id);
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(data);
                }
            }
        }

        if (data.Rows.Count > 0)
        {
            DashboardId = Convert.ToInt16(data.Rows[0][0].ToString());
        }

        return DashboardId;

    }
    public SLUserDashboard GetDashbaordId(int Id)
    {
        var data = new DataTable();
        var ud = new SLUserDashboard();
        using (SqlConnection conn = _passport.Connection())
        {
            using (var cmd = new SqlCommand(DashboardGetIdBaseQ, conn))
            {
                cmd.Parameters.AddWithValue("@DashboardId", Id);
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(data);
                }
            }
        }

        // Datatable convered into SLUserDashboard List
        if (data.Rows.Count > 0)
        {
            var stringObj = JsonConvert.SerializeObject(data);
            ud = JsonConvert.DeserializeObject<List<SLUserDashboard>>(stringObj).FirstOrDefault();
        }

        return ud;

    }

    public List<SLUserDashboard> GetDashbaordList()
    {
        var data = new DataTable();
        var ud = new List<SLUserDashboard>();

        this.DashboardGetListQ = this.DashboardGetListQ;

        using (SqlConnection conn = _passport.Connection())
        {
            using (var cmd = new SqlCommand(this.DashboardGetListQ, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", _passport.UserId);
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(data);
                }
            }
        }

        // Datatable convered into SLUserDashboard List
        if (data.Rows.Count > 0)
        {
            var stringObj = JsonConvert.SerializeObject(data);
            ud = JsonConvert.DeserializeObject<List<SLUserDashboard>>(stringObj);
        }
        return ud;

    }

    public int DeleteDashbaord(int Id)
    {
        var data = new DataTable();
        int DashboardId = 0;
        using (SqlConnection conn = _passport.Connection())
        {
            using (var cmd = new SqlCommand(DashboardDeleteQ, conn))
            {
                cmd.Parameters.AddWithValue("@DashboardId", Id);
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(data);
                }
            }
        }

        if (data.Rows.Count > 0)
        {
            DashboardId = (int)data.Rows[0][0];
        }

        return DashboardId;

    }


    public List<CommonDropdown> TrackableTable()
    {
        var pTablesList = new List<CommonDropdown>();
        var data = ExecureSelectQuery(TrackingTableQ);
        if (data.Rows.Count > 0)
        {
            foreach (DataRow row in data.Rows)
            {
                // If _passport.CheckPermission(row("TableName"), Enums.SecureObjects.Table, Enums.PassportPermissions.Configure) Then
                if (_passport.CheckPermission(row["TableName"].ToString(), Smead.Security.SecureObject.SecureObjectType.Table, Smead.Security.Permissions.Permission.View))
                {
                    var ob = new CommonDropdown();
                    ob.Id = Convert.ToInt32(row["TableId"].ToString());
                    ob.Name = row["TableName"].ToString();
                    ob.UserName = row["UserName"].ToString();
                    pTablesList.Add(ob);
                }
            }
        }
        return pTablesList;
    }

    public List<CommonDropdown> AuditTable()
    {
        var pTablesList = new List<CommonDropdown>();
        var data = this.ExecureSelectQuery(this.AuditTableQ);
        if (data.Rows.Count > 0)
        {
            foreach (DataRow row in data.Rows)
            {
                // If _passport.CheckPermission(row("TableName"), Enums.SecureObjects.Table, Enums.PassportPermissions.Configure) Then

                var tablename = row["TableName"].ToString();

                if (_passport.CheckPermission(tablename.ToString(), Smead.Security.SecureObject.SecureObjectType.Table, Smead.Security.Permissions.Permission.View))
                {
                    var ob = new CommonDropdown();
                    ob.Id = (int)row["TableId"];
                    ob.Name = row["TableName"].ToString();
                    ob.UserName = row["UserName"].ToString();
                    pTablesList.Add(ob);
                }
            }
        }
        return pTablesList;
    }

    public List<CommonDropdown> ViewColumns(int ViewId)
    {
        var cList = new List<CommonDropdown>();
        var _query = new Query(this._passport);
        var @params = new Parameters(ViewId, this._passport);
        var lis = new List<CommonDropdown>();
        @params.QueryType = queryTypeEnum.Schema;
        @params.Culture = Keys.GetCultureCookies(_httpContext);
        @params.Scope = ScopeEnum.Table;
        @params.Culture.DateTimeFormat.ShortDatePattern = Keys.GetCultureCookies(_httpContext).DateTimeFormat.ShortDatePattern;
        // Me.dateFormat = Keys.GetUserPreferences().sPreferedDateFormat.ToString().Trim().ToUpper
        _query.FillData(@params);

        foreach (var dc in @params.Data.Columns)
        {

            if (GridDataBinding.ShowColumn((DataColumn)dc, 0, @params.ParentField) == true)
            {
                var data_col = (DataColumn)dc;
                // don't show column if the lookuptyp is 1 and it is not a dropdown.
                if (Convert.ToBoolean(data_col.ExtendedProperties["lookuptype"]) == true && Convert.ToBoolean(data_col.ExtendedProperties["dropdownflag"]) == true)
                {
                   
                }
                // don't show column
                else
                {
                    string name = data_col.ExtendedProperties["heading"].ToString();
                    int Id = 0;
                    string fieldName = "";
                    foreach (RecordsManage.ViewColumnsRow viewColumn in @params.ViewColumns.Rows)
                    {
                        if ((viewColumn.Heading ?? "") == (name ?? ""))
                        {
                            var obj = new CommonDropdown();
                            obj.Id = viewColumn.Id;
                            obj.Name = viewColumn.Heading; // dc.ExtendedProperties("heading").ToString
                            obj.FieldName = viewColumn.FieldName;
                            lis.Add(obj);
                            break;
                        }
                    }
                }
            }
        }

        return lis;
    }


    public List<CommonDropdown> Users()
    {
        var cList = new List<CommonDropdown>();
        var data = ExecureSelectQuery(UserListQ);
        if (data.Rows.Count > 0)
        {
            string stringObj = JsonConvert.SerializeObject(data);
            cList = JsonConvert.DeserializeObject<List<CommonDropdown>>(stringObj);
        }
        return cList;
    }
    private DataTable GetViewColumns(int ViewId)
    {
        var data = new DataTable();
        using (var conn = this._passport.Connection())
        {
            using (var cmd = new SqlCommand(ViewColumnQ, conn))
            {
                cmd.Parameters.AddWithValue("@ViewId", ViewId);
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(data);
                }
            }
        }
        return data;
    }

    private DataTable ExecureSelectQuery(string query)
    {
        return ExecuteSelectQuery(query, new List<SqlParameter>());
    }

    private DataTable ExecuteSelectQuery(string query, List<SqlParameter> @params)
    {
        var data = new DataTable();
        using (var conn = this._passport.Connection())
        {
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddRange(@params.ToArray());
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(data);
                }
            }
        }
        return data;
    }



    private bool CheckIsDatetimeColumn(string tableName, string column)
    {
        try
        {
            string query = @"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE 
                               TABLE_NAME = @tablename AND COLUMN_NAME = @column";

            var data = new DataTable();
            using (var conn = this._passport.Connection())
            {
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@tablename", tableName);
                    cmd.Parameters.AddWithValue("@column", column);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(data);
                    }
                }
            }
            if (data.Rows[0][0].ToString() == "datetime")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }


    public List<CommonDropdown> GetViewColumnOnlyWithType(string tableName, int viewId)
    {
        var DateTypeColumns = new List<CommonDropdown>();
        var dm = new DashboardDataModel(this._passport, _httpContext);

        foreach (CommonDropdown column in ViewColumns(viewId))
        {
            var GridColumnEntity = new CommonDropdown();
            string sFieldType = "";

            // sometime column not found so try catch are handle this exception
            try
            {
                sFieldType = Navigation.GetFieldDataType(tableName, column.FieldName, this._passport);
            }
            catch (Exception)
            {

            }

            if (sFieldType.Equals("System.DateTime"))
            {
                GridColumnEntity.Id = column.Id;
                GridColumnEntity.Name = column.Name;
                GridColumnEntity.FieldName = column.FieldName;
                DateTypeColumns.Add(GridColumnEntity);
            }
        }

        return DateTypeColumns;

    }
}

public partial class ChartModel
{
    public string X;
    public int Y;
}

public partial class ChartOperatinModel
{
    public string X;
    public int Y;
    public int AuditType;
    public string AuditTypeValue;
}

public partial class ChartOperatinModelRes
{
    public ChartOperatinModelRes()
    {
        Data = new List<ChartModel>();
    }
    public string AuditType;
    public List<ChartModel> Data;
}

public partial class TableModel
{
    public string TableName;
}
