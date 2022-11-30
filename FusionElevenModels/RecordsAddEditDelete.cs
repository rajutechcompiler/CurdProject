
using System.Collections.Generic;
using Smead.RecordsManagement;
using Smead.Security;
using SecureObject = Smead.Security.SecureObject;
using TabFusionRMS.Resource;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.Models;
using System.Text.Json.Serialization;
using System.Data;

namespace TabFusionRMS.WebCS.FusionElevenModels
{

    // SAVE NEW ROW, EDIT ROW FUNCTIONS RETURN MESSAGE TO DOM
    public class RecordsAddEditDelete
    {
    }
    public class Saverows : BaseModel
    {
        public Saverows(Passport passport, paramsUI paramsUi, string clientIpAddress, List<RowsparamsUI> rowdata, string keyvalue, HttpContext httpContext)
        {
            _passport = passport;
            _viewId = paramsUi.ViewId;
            crumbLevel = paramsUi.crumbLevel;
            _beforeChange = paramsUi.BeforeChange;
            _afterChange = paramsUi.AfterChange;
            this.clientIpAddress = clientIpAddress;
            scriptReturn = new LinkScriptModel();
            RowDataFields = rowdata;
            this.keyvalue = keyvalue;
            scriptDone = paramsUi.scriptDone;
            _httpContext = httpContext;
        }
        private int crumbLevel { get; set; }
        private string clientIpAddress { get; set; }
        private int _viewId { get; set; }
        private string _beforeChange { get; set; }
        private string _afterChange { get; set; }
        private bool _isSavefails { get; set; }
        public LinkScriptModel scriptReturn { get; set; }
        public List<RowsparamsUI> RowDataFields { get; set; }
        public bool IsNewRecord { get; set; }
        public string keyvalue { get; set; }
        public bool scriptDone { get; set; }

        //[JsonIgnore]
        //public IRepository<View> _iView { get; set; }

        // Public Property ListOfDatarows As List(Of List(Of String))
        // Public Property ToolBarHtml As String
        // Public Property ListOfHeaders As List(Of TableHeadersProperty)
        // Public Property HasDrillDowncolumn As Boolean
        public GridDataBinding gridDatabinding { get; set; }
        public void AddNewRow(string childkeyfield)
        {
            // Dim _query = New Query(_passport)
            var param = new Parameters(_viewId, _passport);
            if (_passport.CheckPermission(param.ViewName, SecureObject.SecureObjectType.View, Permissions.Permission.Add))
            {
                param.Scope = ScopeEnum.Table;
                param.NewRecord = true;
                param.AfterData = _afterChange;
                IsNewRecord = true;
                param.RequestedRows = 1;
                param.Culture = Keys.GetCultureCookies(_httpContext);
                // linkscript before
                // check if isBeforeSaveLinkScript finished skip
                ScriptReturn result = null;
                if (scriptDone == false)
                {
                    LinkScriptRunBeforeAdd(result, param);
                    if (scriptReturn.isBeforeAddLinkScript)
                    {
                        return;
                    }
                }
                else
                {
                    result = new ScriptReturn(true, "", 0.ToString(), false);
                }

                Query.Save(param, "", param.KeyField, "", DataFieldValues(), _passport, result);

                if(string.IsNullOrWhiteSpace(keyvalue) && !string.IsNullOrWhiteSpace(param.KeyValue))
                {
                    param.AfterData += string.Format("{1}: {2}{0}", Environment.NewLine, param.KeyField, param.KeyValue);
                }

                keyvalue = param.KeyValue;
                // save audit
                {
                    var withBlock = AuditType.WebAccess;
                    withBlock.TableName = param.TableName;
                    withBlock.TableId = param.KeyValue;
                    withBlock.ClientIpAddress = clientIpAddress;
                    withBlock.ActionType = AuditType.WebAccessActionType.AddRecord;
                    withBlock.AfterData = param.AfterDataTrimmed;
                    withBlock.BeforeData = string.Empty;
                }

                Auditing.AuditUpdates(AuditType.WebAccess, _passport);
                // linkscript After
                LinkScriptRunAfterAdd(result, param);

                string retentionCode = Query.SetRetentionCode(param.TableName, param.TableInfo, param.KeyValue, _passport);
                DataRow row = Navigation.GetSingleRow(param.TableInfo, param.KeyValue, param.KeyField, _passport);
                Tracking.SetRetentionInactiveFlag(param.TableInfo, row, retentionCode, _passport);

                gridDatabinding = GetLastRowadded(param, childkeyfield);
                Msg = "success";
            }
            // Me.ListOfDatarows = returnValueonrow.ListOfDatarows
            // Me.ToolBarHtml = returnValueonrow.ToolBarHtml
            // Me.ListOfHeaders = returnValueonrow.ListOfHeaders
            // Me.HasDrillDowncolumn = returnValueonrow.HasDrillDowncolumn
            else
            {
                Msg = Languages.get_Translation("msgNoPermission2Add");
                isError = true;
            }
        }

        private GridDataBinding GetLastRowadded(Parameters param, string childkeyfield)
        {
            var _query = new Query(_passport);
            var q = new GridDataBinding(_passport, _viewId, 1, crumbLevel, (int)ViewType.FusionView, childkeyfield, _httpContext);
            q.IsWhereClauseRequest = true;
            if (Navigation.IsAStringType(param.IdFieldDataType) || Navigation.IsADateType(param.IdFieldDataType))
            {
                q.WhereClauseStr = string.Format("SELECT [{0}] FROM [{1}] where [{0}] = '{2}'", Navigation.MakeSimpleField(param.KeyField), param.TableName, param.KeyValue.Replace("'", "''"));
            }
            else
            {
                q.WhereClauseStr = string.Format("SELECT [{0}] FROM [{1}] where [{0}] = {2}", Navigation.MakeSimpleField(param.KeyField), param.TableName, param.KeyValue);
            }
            q.ExecuteGridData();
            if (q.ListOfDatarows.Count == 0)
            {
                return q;
            }
            int counter = 0;
            foreach (TableHeadersProperty f in q.ListOfHeaders)
            {
                string value = q.ListOfDatarows[0][counter];
                if (f.DataTypeFullName == "System.DateTime" && !string.IsNullOrWhiteSpace(value))
                {
                    q.ListOfDatarows[0][counter] = Keys.get_ConvertStringToCultureDate(value, _httpContext).ToString(Keys.GetCultureCookies(_httpContext).DateTimeFormat.ShortDatePattern);
                }
                counter = counter + 1;
            }

            return q;
        }
        private void LinkScriptRunBeforeAdd(ScriptReturn result, Parameters param)
        {
            result = ScriptEngine.RunScriptBeforeAdd(param.TableName, _passport);
            scriptReturn.Successful = result.Successful;
            scriptReturn.GridRefresh = result.GridRefresh;
            scriptReturn.ReturnMessage = result.ReturnMessage;
            // check if there is a script and if script is not finished
            // check if script doesn't need any user interaction.
            if (result.Engine is not null)
            {
                LinkScriptSession = result.Engine;
                if (result.Engine.ShowPromptBool)
                {
                    scriptReturn.isBeforeAddLinkScript = true;
                    scriptReturn.ScriptName = result.Engine.ScriptName;
                }
                else
                {
                    scriptReturn.isBeforeAddLinkScript = false;
                    scriptReturn.ScriptName = "";
                }
            }

            if (!scriptReturn.Successful)
                return;
        }
        private void LinkScriptRunAfterAdd(ScriptReturn result, Parameters param)
        {
            result = ScriptEngine.RunScriptAfterAdd(param.TableName, param.KeyValue, _passport);
            scriptReturn.Successful = result.Successful;
            scriptReturn.GridRefresh = result.GridRefresh;
            scriptReturn.ReturnMessage = result.ReturnMessage;
            scriptReturn.keyValue = param.KeyValue;
            if (result.Engine is not null)
            {
                LinkScriptSession = result.Engine;
                if (result.Engine.ShowPromptBool)
                {
                    scriptReturn.isAfterAddLinkScript = true;
                    scriptReturn.ScriptName = result.Engine.ScriptName;
                }
            }
        }
        public void EditRow(string childkeyfield)
        {
            // Dim _query = New Query(_passport)
            var param = new Parameters(_viewId, _passport);
            if (_passport.CheckPermission(param.ViewName, SecureObject.SecureObjectType.View, Permissions.Permission.Edit))
            {
                param.Scope = ScopeEnum.Table;
                param.KeyValue = keyvalue;
                param.NewRecord = false;
                param.BeforeData = _beforeChange;
                param.AfterData = _afterChange;
                IsNewRecord = false;
                param.Culture = Keys.GetCultureCookies(_httpContext);

                // linkscript before
                ScriptReturn result = null;
                if (scriptDone == false)
                {
                    LinkScriptRunBeforeEdit(result, param);
                    if (scriptReturn.isBeforeEditLinkScript)
                    {
                        return;
                    }
                }
                else
                {
                    result = new ScriptReturn(true, "", 0.ToString(), false);
                }
                // save row
                Query.Save(param, "", param.KeyField, keyvalue, DataFieldValues(), _passport, result);
                keyvalue = param.KeyValue;
                // save audit
                {
                    var withBlock = AuditType.WebAccess;
                    withBlock.TableName = param.TableName;
                    withBlock.TableId = param.KeyValue;
                    withBlock.ClientIpAddress = clientIpAddress;
                    withBlock.ActionType = AuditType.WebAccessActionType.UpdateRecord;
                    withBlock.AfterData = param.AfterDataTrimmed;
                    withBlock.BeforeData = param.BeforeDataTrimmed;
                }

                Auditing.AuditUpdates(AuditType.WebAccess, _passport);

                LinkScriptRunAfterEdit(result, param);

                string retentionCode = Query.SetRetentionCode(param.TableName, param.TableInfo, param.KeyValue, _passport);
                DataRow row = Navigation.GetSingleRow(param.TableInfo, param.KeyValue, param.KeyField, _passport);
                Smead.RecordsManagement.Tracking.SetRetentionInactiveFlag(param.TableInfo, row, retentionCode, _passport);
                gridDatabinding = GetLastRowadded(param, childkeyfield);
            }
            else
            {
                Msg = Languages.get_Translation("msgNoPermission2Edit");
                isError = true;
                return;
            }
            Msg = "success";
        }
        private void LinkScriptRunBeforeEdit(ScriptReturn result, Parameters param)
        {
            result = ScriptEngine.RunScriptBeforeEdit(param.TableName, param.KeyValue, _passport);
            scriptReturn.Successful = result.Successful;
            scriptReturn.GridRefresh = result.GridRefresh;
            scriptReturn.ReturnMessage = result.ReturnMessage;
            // check if there is a script and if script is not finished
            // check if script doesn't need any user interaction.
            if (result.Engine is not null)
            {
                LinkScriptSession = result.Engine;
                if (result.Engine.ShowPromptBool)
                {
                    scriptReturn.isBeforeEditLinkScript = true;
                    scriptReturn.ScriptName = result.Engine.ScriptName;
                }
                else
                {
                    scriptReturn.isBeforeEditLinkScript = false;
                    scriptReturn.ScriptName = "";
                }
            }
            else
            {
                scriptReturn.isBeforeEditLinkScript = false;
                scriptReturn.ScriptName = "";
            }
            if (!scriptReturn.Successful)
                return;

        }
        private void LinkScriptRunAfterEdit(ScriptReturn result, Parameters param)
        {
            result = ScriptEngine.RunScriptAfterEdit(param.TableName, param.KeyValue, _passport);
            scriptReturn.Successful = result.Successful;
            scriptReturn.GridRefresh = result.GridRefresh;
            scriptReturn.ReturnMessage = result.ReturnMessage;
            if (result.Engine is not null)
            {
                LinkScriptSession = result.Engine;
                if (result.Engine.ShowPromptBool)
                {
                    scriptReturn.isAfterEditLinkScript = true;
                    scriptReturn.ScriptName = result.Engine.ScriptName;
                }
            }
        }

        private List<FieldValue> DataFieldValues()
        {
            var lst = new List<FieldValue>();
            foreach (var row in RowDataFields)
            {
                if (!string.IsNullOrEmpty(row.columnName) && !string.IsNullOrEmpty(row.DataTypeFullName))
                {
                    // If param.KeyField = row.columnName Then
                    // param.KeyValue = row.value
                    // End If
                    var field = new FieldValue(row.columnName, row.DataTypeFullName);
                    if (row.value is null)
                    {
                        field.value = "";
                    }
                    else if (row.DataTypeFullName == "System.DateTime")
                    {
                        field.value = Keys.get_ConvertCultureDate(row.value,"E", _httpContext);
                    }
                    else
                    {
                        field.value = row.value;

                    }
                    lst.Add(field);
                }
            }
            return lst;
        }
        public class paramsUI
        {
            public int ViewId { get; set; }
            public string BeforeChange { get; set; }
            public string AfterChange { get; set; }
            public string Tablename { get; set; }
            public string KeyValue { get; set; }
            public string PrimaryKeyname { get; set; }
            public bool scriptDone { get; set; }
            public bool IsNewRow { get; set; }
            public int crumbLevel { get; set; }
            public string childkeyfield { get; set; }

        }
        public class RowsparamsUI
        {
            public string value { get; set; }
            public string columnName { get; set; }
            public string DataTypeFullName { get; set; }
        }


        public class DatabaseChangesReq
        {
            public List<Saverows.RowsparamsUI> Rowdata;
            public Saverows.paramsUI paramss;
        }
    }
    // DELETE ROWS FUNCTIONS RETURN MESSAGE TO DOM
    public class Deleterows : BaseModel
    {
        public Deleterows(Passport passport, List<string> ids, int viewid, string clientIpAddress, bool scriptdone)
        {
            _passport = passport;
            this.ids = ids;
            _viewid = viewid;
            this.clientIpAddress = clientIpAddress;
            scriptDone = scriptdone;
            scriptReturn = new LinkScriptModel();
        }
        private List<string> ids { get; set; }
        private int _viewid { get; set; }
        private string clientIpAddress { get; set; }
        public LinkScriptModel scriptReturn { get; set; }
        public bool scriptDone { get; set; }

        public void DeleteRows()
        {
            var param = new Parameters(_viewid, _passport);
            isError = false;

            if (_passport.CheckPermission(param.ViewName, SecureObject.SecureObjectType.View, Permissions.Permission.Delete))
            {
                Navigation.VerifyLegalDeletion(param.TableName, ids, _passport);
                ScriptReturn result = null;

                foreach (string item in ids)
                {
                    // run linkscript before delete
                    if (scriptDone == false)
                    {
                        LinkScriptRunBeforeDelete(result, param, item);
                        if (scriptReturn.isBeforeDeleteLinkScript)
                            return;
                    }
                    else
                    {
                        result = new ScriptReturn(true, "", 0.ToString(), false);
                    }
                    Query.DeleteTableItem(param.TableName, item, clientIpAddress, false, true, false, _passport);
                    // run linkscript after delete
                    LinkScriptRunAfterDelete(result, param);
                }
            }
            else
            {
                Msg = "You don't have sufficient Permission to Delete";
                isError = true;
            }
        }
        private void LinkScriptRunBeforeDelete(ScriptReturn result, Parameters param, string tableId)
        {
            result = ScriptEngine.RunScriptBeforeDelete(param.TableName, tableId, _passport);
            scriptReturn.Successful = result.Successful;
            scriptReturn.GridRefresh = result.GridRefresh;
            scriptReturn.ReturnMessage = result.ReturnMessage;
            // check if there is a script and if script is not finished
            // check if script doesn't need any user interaction.
            if (result.Engine is not null)
            {
                LinkScriptSession = result.Engine;
                if (result.Engine.ShowPromptBool)
                {
                    scriptReturn.isBeforeDeleteLinkScript = true;
                    scriptReturn.ScriptName = result.Engine.ScriptName;
                }
                else
                {
                    scriptReturn.isBeforeDeleteLinkScript = false;
                    scriptReturn.ScriptName = "";
                }
            }
            else
            {
                scriptReturn.isBeforeDeleteLinkScript = false;
                scriptReturn.ScriptName = "";
            }
            if (!scriptReturn.Successful)
                return;
            // save New ROW 
        }
        private void LinkScriptRunAfterDelete(ScriptReturn result, Parameters param)
        {
            result = ScriptEngine.RunScriptAfterDelete(param.TableName, param.KeyValue, _passport);
            scriptReturn.Successful = result.Successful;
            scriptReturn.GridRefresh = result.GridRefresh;
            scriptReturn.ReturnMessage = result.ReturnMessage;
            if (result.Engine is not null)
            {
                LinkScriptSession = result.Engine;
                if (result.Engine.ShowPromptBool)
                {
                    scriptReturn.isAfterDeleteLinkScript = true;
                    scriptReturn.ScriptName = result.Engine.ScriptName;
                }
            }
        }

        public class RowparamsUI
        {
            public List<string> ids { get; set; }
        }
        public class paramsUI
        {
            public int viewid { get; set; }
            public bool ScriptDone { get; set; }
        }
    }
    public class DeleteRowsFromGridReqModel
    {
        public Deleterows.RowparamsUI rowData { get; set; }
        public Deleterows.paramsUI paramss { get; set; }
    }
}