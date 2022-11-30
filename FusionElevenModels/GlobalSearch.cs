
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualBasic.CompilerServices;
using Smead.RecordsManagement;
using static Smead.RecordsManagement.Navigation;
using Smead.Security;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;

namespace TabFusionRMS.WebCS
{

    // GLOBAL SEARCH RETURN DOM OBJECT AND PARTIAL VIEW
    public class GlobalSearch : BaseModel
    {
        public GlobalSearch(Passport passport, int viewid, string inputtxt, bool chkAttch, bool chkcurTable, bool chkUnderRow, string tableName, int rowid, HttpContext httpContext)
        {
            _passport = passport;
            _viewId = viewid;
            _InputTxt = inputtxt;
            _chkAttch = chkAttch;
            _chkCurTable = chkcurTable;
            _chkUnderRow = chkUnderRow;
            _tableName = tableName;
            _rowid = rowid;
            _httpContext = httpContext;
        }
        private LevelManager _levelmanager;
        private int _viewId;
        private string _tableName;
        private int _rowid;
        private string _InputTxt;
        private bool _chkAttch;
        private bool _chkCurTable;
        private bool _chkUnderRow;
        public string HTMLSearchResults;

        public void HtmlReturn()
        {
            if (_InputTxt is null || _InputTxt.Length == 0)
                return;
            var qry = new Query(_passport);
            var @params = new Parameters(_passport);
            var savedSearches = new List<string>();

            CheckBoxesconditions(@params);
            @params.Text = _InputTxt;

            @params.IncludeAttachments = _chkAttch;

            @params.QueryType = queryTypeEnum.Text;
            // params.RequestedRows = 500
            IRepository<Setting> _iSettings = new Repositories<Setting>();
            int globalSearchMaxVal = 25;
            string sMaxVal = _iSettings.All().Where(x => x.Section == "GlobalSearch" & x.Item == "MaxRecordsFetch").Select(y => y.ItemValue).FirstOrDefault();
            if (!string.IsNullOrEmpty(sMaxVal))
            {
                globalSearchMaxVal = Convert.ToInt32(sMaxVal);
            }
            @params.RequestedRows = globalSearchMaxVal;

            // 'If _levelManager Is Nothing Then _levelManager = CType(Session("LevelManager"), LevelManager)
            // _levelManager.DatabaseSearchParams = params
            // 'check if view searchable
            try
            {
                // If Not IsTableHasActiveFulltextIndex() Then
                // Me.Msg = Languages.Translation("msgSearchControlFullTextCatalogAndIndexNotSet")
                // Me.isError = True
                // Return
                // End If
                @params.IsMVCCall = true;
                qry.Search(@params);
                HTMLSearchResults = @params.HTMLSearchResults;
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, this, @params.ViewId, _passport.DatabaseName);
                Msg = ex.Message;
            }
        }
        private void CheckBoxesconditions(dynamic @params)
        {
            if (_chkUnderRow)
            {
                @params.Scope = ScopeEnum.Node;
                if (_tableName is not null && !string.IsNullOrEmpty(_tableName))
                {
                    @params.ChangeViewID(_viewId);
                }
                @params.CursorValue = _rowid;
            }
            else if (_chkCurTable)
            {
                @params.Scope = ScopeEnum.Table;
                if (_tableName is not null && !string.IsNullOrEmpty(_tableName))
                {
                    @params.ChangeViewID(GetTableFirstSearchableViewId(_tableName, _passport));
                }
            }
            else
            {
                @params.Scope = ScopeEnum.Database;
            }
        }

        private bool IsTableHasActiveFulltextIndex()
        {
            string SQLQuery = "";
            int returnValue;
            bool TableHasActiveFulltextIndex = false;
            DataBaseManagerVB.IDBManager _IDBManager = new DataBaseManagerVB.DBManager();
            _IDBManager.ConnectionString =  Keys.get_GetDBConnectionString();
            try
            {
                SQLQuery = "SELECT OBJECTPROPERTY ( object_id('SLIndexer'), 'TableHasActiveFulltextIndex') As TableHasActiveFulltextIndex";
                returnValue = Conversions.ToInteger(_IDBManager.ExecuteScalar(CommandType.Text, SQLQuery));

                if (returnValue > 0)
                {
                    TableHasActiveFulltextIndex = true;
                }
            }
            catch (Exception)
            {
                TableHasActiveFulltextIndex = false;
            }

            return TableHasActiveFulltextIndex;
        }
        public class globalSearchUI
        {
            public int ViewId { get; set; }
            public string TableName { get; set; }
            public int Currentrow { get; set; }
            public string SearchInput { get; set; }
            public bool ChkAttch { get; set; }
            public bool ChkcurTable { get; set; }
            public bool ChkUnderRow { get; set; }
            public string KeyValue { get; set; }
            public bool IncludeAttchment { get; set; }
            public int crumbLevel { get; set; }
        }
    }


    public class GlobalSearchReqModel
    {
        public GlobalSearch.globalSearchUI paramss { get; set; }
    }
    
}