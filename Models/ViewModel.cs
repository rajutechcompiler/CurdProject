using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Smead.Security;
using TabFusionRMS.Models;
using TabFusionRMS.Resource;

namespace TabFusionRMS.WebCS
{

    public sealed class ViewModel
    {

        //public static JSTreeView.TreeView GetBindTreeControlView(IQueryable<Table> lTableEntities, IQueryable<View> lViewEntities)
        //{
        //    var treeView = new JSTreeView.TreeView();
        //    var treeNode = new JSTreeView.TreeNode();
        //    var rootNode = new JSTreeView.ListItem("Root node 1", "node1");
        //    var dataJsTree = new JSTreeView.DataJsTree();
        //    // dataJsTree.Icon = "glyphicon glyphicon-folder-open"
        //    // dataJsTree.Icon = "fa fa-minus-square-o"
        //    // dataJsTree.Icon = "/Images/icons/EMPLOYEE.ICO"
        //    dataJsTree.Opened = false;

        //    var passport = ContextService.GetObjectFromJson<Passport>("passport");

        //    foreach (Table pTables in lTableEntities)
        //    {
        //        if (!CollectionsClass.IsEngineTable(pTables.TableName))
        //        {
                   
        //            if (passport.CheckPermission(pTables.TableName, Smead.Security.SecureObject.SecureObjectType.Table, Smead.Security.Permissions.Permission.View))
        //            {
        //                var TablesNode = new JSTreeView.ListItem(pTables.UserName, Guid.NewGuid().ToString() + "_rootTables_" + pTables.TableId.ToString(), className: "jstree-close");
        //                rootNode.Nodes.Add(TablesNode);

        //                var lViews = lViewEntities.Where(x => (x.TableName.Trim().ToLower()) == (pTables.TableName.Trim().ToLower())).OrderBy(x => x.ViewOrder);

        //                foreach (var pView in lViews)
        //                {
        //                    if ((pView.Printable == false) && (passport.CheckPermission(pView.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.View, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Configure)) == true)
                                
        //                    {
        //                        var ViewNode = new JSTreeView.ListItem(pView.ViewName, Guid.NewGuid().ToString() + "_childViews_" + pView.Id.ToString(), className: "jstree-close");
        //                        TablesNode.Nodes.Add(ViewNode);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    treeNode.ListItems.Add(rootNode);
        //    treeView.Nodes.Add(treeNode);
        //    return treeView;
        //}

        public static string GetBindViewMenus(string root, IQueryable<Table> lTableEntities, IQueryable<View> lViewEntities, Passport _passport)
        {
            //string strViewMenu = "";
            string strALId = "";
            StringBuilder strViewMenu = new StringBuilder();
            foreach (Table pTables in lTableEntities)
            {
                if (!CollectionsClass.IsEngineTable(pTables.TableName))
                {
                    if (_passport.CheckPermission(pTables.TableName, Smead.Security.SecureObject.SecureObjectType.Table, Smead.Security.Permissions.Permission.View))
                    {
                        strALId = string.Format("AL_{0}_{1}", pTables.TableName, pTables.TableId.ToString());
                        strViewMenu.Append("<li>");

                        strViewMenu.Append("<a href='#' id='" + strALId.ToString().Trim() + "' onclick=RootItemClick('" + strALId.ToString().Trim() + "')>");
                        strViewMenu.Append(pTables.UserName + "</a>");
                        var lViews = lViewEntities.Where(x => (x.TableName.Trim().ToLower() ?? "") == (pTables.TableName.Trim().ToLower() ?? "")).OrderBy(x => x.ViewOrder);
                        if (lViews.Count() > 0)
                        {
                            strViewMenu.Append("<ul>");
                            foreach (var pView in lViews)
                            {
                                if ((pView.Printable != true) && (_passport.CheckPermission(pView.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.View, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Configure)))
                                {
                                    // Dim ViewNode = New JSTreeView.ListItem(pView.ViewName, Guid.NewGuid.ToString() + "_childViews_" + pView.Id.ToString(), className:="jstree-close")
                                    // TablesNode.Nodes.Add(ViewNode)
                                    strViewMenu.Append(string.Format("<li><a id='FAL_{0}' onclick=ChildItemClick('{2}','{3}','FAL_{0}')>{1}</a></li>", pView.Id.ToString().Trim(), pView.ViewName, root, strALId));
                                }
                            }
                            strViewMenu.Append("</ul>");
                        }
                        strViewMenu.Append("</li>");
                    }
                }
            }


            // treeNode.ListItems.Add(rootNode)
            // treeView.Nodes.Add(treeNode)

            // Dim strViewMenu = "<li>" +
            // "<a href='#' id='AL010'>Clients</a>" +
            // "<ul><li> <a href='#' id='FAL0100'>All Clients 1</a></li>" +
            // "<li><a href='#' id='FAL0101'>All Clients 2</a></li>" +
            // "</ul></li>" +
            // "<li Class='hasSubs'><a  href='#' id='AL010'>Clients1</a><ul><li>" +
            // "<a  href='#' id='FAL0100'>All Clients 11</a></li>" +
            // "<li> <a  href='#' id='FAL0101'>All Clients 21</a></li></ul></li>"

            return strViewMenu.ToString();
        }

        public static void GetFieldTypeAndSize(Table oTables, IQueryable<Databas> lDatabas, string sFieldName, ref string msFieldType, ref string msFieldSize)
        {

            string sTableName;
            var csADOConn = new ADODB.Connection();

            if (Strings.InStr(sFieldName, ".") > 1)
            {
                sTableName = Strings.Left(sFieldName, Strings.InStr(sFieldName, ".") - 1);
            }
            else
            {
                sTableName = oTables.TableName;
            }

            if (DatabaseMap.RemoveTableNameFromField(sFieldName) == "SLTrackedDestination" | DatabaseMap.RemoveTableNameFromField(sFieldName) == "SLFileRoomOrder")
            {
                msFieldType = Common.FT_TEXT;
                msFieldSize = Common.FT_MEMO_SIZE;
                return;
            }

            var pDatabaseEntity = lDatabas.Where(x => x.DBName.Trim().ToLower().Equals(oTables.DBName.Trim().ToLower())).FirstOrDefault();
            csADOConn = DataServices.DBOpen(pDatabaseEntity);
            BindTypeAndSize(csADOConn, sFieldName, sTableName, ref msFieldType, ref msFieldSize, oTables);
            return;
        }

        public static void BindTypeAndSize(ADODB.Connection csADOConn, string sFieldName, string sTableName, ref string msFieldType, ref string msFieldSize, Table oTables = null)
        {
            ADODB.Field oFields;
            var rsADO = new ADODB.Recordset();

            string argsSql = "Select * FROM [" + sTableName + "] WHERE 0 = 1";
            string arglError = "";
            rsADO = DataServices.GetADORecordSet(ref argsSql, csADOConn, lError: ref arglError);
            oFields = ViewModel.FieldWithOrWithoutTable(sFieldName, rsADO.Fields, false);
            if (rsADO.State != (int)Enums.ObjectStateEnum.rmStateClosed)
            {
                if (oFields is not null)
                {
                    if (DataServices.IsADateType((Enums.DataTypeEnum)oFields.Type))
                    {
                        msFieldType = Common.FT_DATE;
                        msFieldSize = Common.FT_DATE_SIZE;
                    }
                    else if (DataServices.IsAStringType((Enums.DataTypeEnum)oFields.Type))
                    {
                        if (oFields.DefinedSize <= 0 | oFields.DefinedSize >= 2000000)
                        {
                            msFieldType = Common.FT_MEMO;
                            msFieldSize = Common.FT_MEMO_SIZE;
                        }
                        else
                        {
                            msFieldType = Common.FT_TEXT;
                            msFieldSize = oFields.DefinedSize.ToString();
                        }
                    }
                    else
                    {
                        switch (oFields.Type)
                        {
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBoolean:
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedTinyInt:
                                {
                                    msFieldType = Common.FT_BOOLEAN;
                                    msFieldSize = Common.FT_BOOLEAN_SIZE;
                                    break;
                                }
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmDouble:
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmCurrency:
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmDecimal:
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmNumeric:
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmSingle:
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmVarNumeric:
                                {
                                    msFieldType = Common.FT_DOUBLE;
                                    msFieldSize = Common.FT_DOUBLE_SIZE;
                                    break;
                                }
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBigInt:
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedBigInt:
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmInteger:
                                {
                                    if (Convert.ToBoolean(oFields.Properties["IsAutoIncrement"].Value))
                                    {
                                        msFieldType = Common.FT_AUTO_INCREMENT;
                                        msFieldSize = Common.FT_AUTO_INCREMENT_SIZE;
                                    }
                                    else if (oTables is not null)
                                    {
                                        if (!string.IsNullOrEmpty(oTables.CounterFieldName) & Strings.StrComp(DatabaseMap.RemoveTableNameFromField(oFields.Name), DatabaseMap.RemoveTableNameFromField(oTables.IdFieldName), Constants.vbTextCompare) == 0)
                                        {
                                            msFieldType = Common.FT_SMEAD_COUNTER;
                                            msFieldSize = Common.FT_SMEAD_COUNTER_SIZE;
                                        }
                                        else
                                        {
                                            msFieldType = Common.FT_LONG_INTEGER;
                                            msFieldSize = Common.FT_LONG_INTEGER_SIZE;
                                        }
                                    }

                                    break;
                                }
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmBinary:
                                {
                                    msFieldType = Common.FT_BINARY;
                                    msFieldSize = Common.FT_MEMO_SIZE;
                                    break;
                                }
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmSmallInt:
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmTinyInt:
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedInt:
                            case (ADODB.DataTypeEnum)Enums.DataTypeEnum.rmUnsignedSmallInt:
                                {
                                    msFieldType = Common.FT_SHORT_INTEGER;
                                    msFieldSize = Common.FT_SHORT_INTEGER_SIZE;
                                    break;
                                }
                        }
                    }
                }
                rsADO.Close();
            }
            rsADO = null;
            oFields = null;

        }

        public static ADODB.Field FieldWithOrWithoutTable(string sFieldName, ADODB.Fields oFields, bool bWarnUser = true)
        {
            ADODB.Field FieldWithOrWithoutTableRet = default;
            FieldWithOrWithoutTableRet = null;
            try
            {
                if (oFields is not null)
                {
                    if (!string.IsNullOrEmpty(Strings.Trim(sFieldName)) & Information.IsNumeric(sFieldName))
                    {
                        FieldWithOrWithoutTableRet = oFields[Convert.ToString(sFieldName)];
                    }
                    else if (!string.IsNullOrEmpty(Strings.Trim(sFieldName)))
                    {
                        if (Strings.InStr(sFieldName, ".") > 1L)
                        {
                            FieldWithOrWithoutTableRet = oFields[DatabaseMap.RemoveTableNameFromField(sFieldName)];
                            if (Information.Err().Number != 0)
                            {
                                FieldWithOrWithoutTableRet = oFields[sFieldName];
                            }
                        }
                        else
                        {
                            FieldWithOrWithoutTableRet = oFields[sFieldName];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                FieldWithOrWithoutTableRet = null;
            }

            return FieldWithOrWithoutTableRet;


        }

        public static bool DataLocked(string sFieldName, ADODB.Recordset rsADO)
        {
            bool DataLockedRet = default;
            ADODB.Field oField = null;
            bool IsLock = false;
            try
            {
                if (!string.IsNullOrEmpty(Strings.Trim(sFieldName)))
                {
                    // Turn off editing if we know for sure we are not allowed to
                    oField = ViewModel.FieldWithOrWithoutTable(sFieldName, rsADO.Fields, false);

                    if (oField is not null)
                    {
                        if (Convert.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(Operators.AndObject(oField.Attributes, Enums.FieldAttributeEnum.rmFldUpdatable), 0, false), Operators.ConditionalCompareObjectEqual(Operators.AndObject(oField.Attributes, Enums.FieldAttributeEnum.rmFldUnknownUpdatable), 0, false))))
                        {
                            IsLock = true;
                        }
                        // See if an auto increment field
                        if (Convert.ToBoolean(oField.Properties["ISAUTOINCREMENT"].Value))
                            DataLockedRet = true;
                    }
                    else
                    {
                        IsLock = true;
                    }
                    oField = null;
                }
            }
            catch (Exception)
            {
                IsLock = false;
            }
            return IsLock;
        }

        public static List<GridColumns> GetColumnsData(IQueryable<View> lView, IQueryable<ViewColumn> lViewColumns, IQueryable<Databas> lDatabas, IQueryable<Table> lTables, int intViewsId, string sAction)
        {
            var GridColumnEntities = new List<GridColumns>();
            try
            {
                if (!string.IsNullOrEmpty(sAction) && sAction.Trim().ToUpper().Equals("E"))
                {
                    var oViews = lView.Where(x => x.Id == intViewsId).FirstOrDefault();
                    // Dim lViewColumns = lViewColumn.All()
                    // Dim lDatabas = lDatabas

                    if (oViews is not null)
                    {
                        string sTableName = oViews.TableName;
                        int iViewsId = Convert.ToInt32(oViews.Id);
                        var oTable = lTables.Where(x => x.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())).FirstOrDefault();
                        var olViewColumns = lViewColumns.Where(x => x.ViewsId == iViewsId).OrderBy(x => x.ColumnNum);

                        if (olViewColumns is not null)
                        {
                            if (olViewColumns.Count() == 0)
                            {
                                // Dim oViewsFirst = lView.Where(Function(x) x.TableName.Trim().ToLower().Equals(oViews.TableName.Trim().ToLower())).OrderBy(Function(x) x.ViewOrder).FirstOrDefault()
                                // olViewColumns = lViewColumns.Where(Function(x) x.ViewsId = oViewsFirst.Id).OrderBy(Function(x) x.ColumnNum)

                                // Taken columns from AltView, If view does not have any columns - FUS- 5704
                                var oAltView = lView.Where(x => x.Id == oViews.AltViewId).FirstOrDefault();
                                olViewColumns = lViewColumns.Where(x => x.ViewsId == oAltView.Id).OrderBy(x => x.ColumnNum);
                            }
                        }

                        if (olViewColumns is not null)
                        {

                            foreach (ViewColumn column in olViewColumns)
                            {
                                var GridColumnEntity = new GridColumns();
                                GridColumnEntity.ColumnSrNo = column.Id;
                                GridColumnEntity.ColumnId = (int)column.ColumnNum;
                                GridColumnEntity.ColumnName = column.Heading;
                                string sFieldType = "";
                                string sFieldSize = "";
                                GetFieldTypeAndSize(oTable, lDatabas, column.FieldName, ref sFieldType, ref sFieldSize);
                                GridColumnEntity.ColumnDataType = sFieldType;
                                GridColumnEntity.ColumnMaxLength = sFieldSize;
                                GridColumnEntity.IsPk = false;
                                GridColumnEntity.AutoInc = (bool)column.FilterField;
                                GridColumnEntities.Add(GridColumnEntity);
                            }

                        }

                    }
                }
                return GridColumnEntities;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }

        }

        // ByVal bIsString As Boolean, Optional ByVal bLookup As Boolean = False, _
        // Optional ByVal bYesNo As Boolean = False, Optional ByVal bDate As Boolean = False

        public static List<KeyValuePair<string, string>> FillOperatorsDropDownOnChange(ref Dictionary<string, bool> filterControls, IQueryable<View> lView, 
            IQueryable<Table> lTable, IQueryable<Databas> lDatabase, int iColumnNum, HttpContext httpContext)
        {
            bool bIsString = true;
            var bLookup = default(bool);
            bool bDate = false;
            bool bYesNo = false;
            var lOperatorItems = new List<KeyValuePair<string, string>>();
            if (mcFilterColumns.Count != 0)
            {
                var oFilterColumns = mcFilterColumns.Where(m => m.Value.ColumnNum == iColumnNum).FirstOrDefault().Value;
                if (oFilterColumns is not null)
                {
                    bLookup = (bool)(Convert.ToInt32(Enums.geViewColumnsLookupType.ltUndefined) is var arg14 && oFilterColumns.LookupType is { } arg13 ? arg13 != arg14 : (bool?)null);
                    if (!bLookup)
                    {
                        if (oFilterColumns is not null)
                        {
                            string tableName = "";
                            if (string.IsNullOrEmpty(tableName))
                            {
                                if ((long)Strings.InStr(oFilterColumns.FieldName, ".") > 1L)
                                {
                                    tableName = DatabaseMap.RemoveFieldNameFromField(oFilterColumns.FieldName);
                                }
                                else if ((oFilterColumns.ViewsId is null | (0 is var arg16 && oFilterColumns.ViewsId is { } arg15 ? arg15 == arg16 : (bool?)null)) == true)
                                {
                                    tableName = httpContext.Session.GetString("viewTableName") ?? "";
                                }
                                else
                                {
                                    tableName = lView.Where(m => m.Id == oFilterColumns.ViewsId).FirstOrDefault()?.TableName??"";

                                }
                            }

                            if (!string.IsNullOrEmpty(tableName))
                            {
                                var tableObj = lTable.Where(m => m.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower())).FirstOrDefault();
                                var sADOConn = DataServices.DBOpen(tableObj, lDatabase);
                                var oSchemaColumnList = SchemaInfoDetails.GetSchemaInfo(tableName, sADOConn, DatabaseMap.RemoveTableNameFromField(oFilterColumns.FieldName));
                                var oSchemaColumn = new SchemaColumns();
                                foreach (var currentOSchemaColumn in oSchemaColumnList)
                                {
                                    oSchemaColumn = currentOSchemaColumn;
                                    if (Strings.StrComp(oSchemaColumn.ColumnName, DatabaseMap.RemoveTableNameFromField(oFilterColumns.FieldName), Constants.vbTextCompare) == 0)
                                    {
                                        break;
                                    }
                                }

                                if (oSchemaColumn is not null)
                                {
                                    bYesNo = false;
                                    bDate = false;
                                    switch (oSchemaColumn.DataType)
                                    {
                                        case Enums.DataTypeEnum.rmBoolean:
                                        case Enums.DataTypeEnum.rmSmallInt:
                                        case Enums.DataTypeEnum.rmUnsignedSmallInt:
                                        case Enums.DataTypeEnum.rmTinyInt:
                                        case Enums.DataTypeEnum.rmUnsignedTinyInt:
                                            {
                                                bYesNo = true;
                                                break;
                                            }

                                        case Enums.DataTypeEnum.rmDate:
                                        case Enums.DataTypeEnum.rmDBDate:
                                        case Enums.DataTypeEnum.rmDBTime:
                                            {
                                                bDate = true;
                                                break;
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            filterControls.Add("FieldDDL", bLookup);
            filterControls.Add("chkYesNoField", bYesNo);
            filterControls.Add("FieldTextBox", !bLookup & !bYesNo);
            FillOperatorsDropDown(ref lOperatorItems, true, bLookup, bYesNo, bDate);
            return lOperatorItems;
        }

        public static void FillOperatorsDropDown(ref List<KeyValuePair<string, string>> lOperatorItems, bool bIsString, bool bLookup = false, bool bYesNo = false, bool bDate = false)
        {
            try
            {
                lOperatorItems.Clear();
                lOperatorItems.Add(new KeyValuePair<string, string>("=", "="));
                if (!bLookup & !bYesNo)
                {
                    lOperatorItems.Add(new KeyValuePair<string, string>(">", ">"));
                    lOperatorItems.Add(new KeyValuePair<string, string>("<", "<"));
                    lOperatorItems.Add(new KeyValuePair<string, string>(">=", ">="));
                    lOperatorItems.Add(new KeyValuePair<string, string>("<=", "<="));
                }
                if (!bYesNo & !bDate)
                {
                    lOperatorItems.Add(new KeyValuePair<string, string>("<>", "<>"));
                    if (bIsString & !bLookup)
                    {
                        lOperatorItems.Add(new KeyValuePair<string, string>("In", "In"));
                        lOperatorItems.Add(new KeyValuePair<string, string>("BEG", "BEG"));
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public static Dictionary<int, ViewColumn> mcFilterColumns = new Dictionary<int, ViewColumn>();

        public static Dictionary<int, ViewColumn> FillFilterFieldNames(List<ViewColumn> lViewColumn)
        {
            try
            {
                mcFilterColumns.Clear();
                if (lViewColumn is not null)
                {
                    int iCount = 1;
                    foreach (ViewColumn viewColObj in lViewColumn)
                    {
                        if ((bool)viewColObj.FilterField)
                        {
                            viewColObj.LookupIdCol = (short?)iCount; // LookupIdCol=IdColumnNum
                            if (Convert.ToBoolean(viewColObj.DropDownFlag.Value))
                            {
                                viewColObj.LookupType = Convert.ToInt16(Enums.geViewColumnsLookupType.ltDirect);
                                viewColObj.TabOrder = iCount;    // TabOrder=DisplayColumn
                            }
                            else
                            {
                                viewColObj.LookupType = Convert.ToInt16(Enums.geViewColumnsLookupType.ltUndefined);
                                viewColObj.TabOrder = -1;
                            }   // TabOrder=DisplayColumn
                            mcFilterColumns.Add(iCount, viewColObj);
                        }
                        else if (viewColObj.LookupType == (int)Enums.geViewColumnsLookupType.ltLookup && viewColObj.DropDownFlag>0)
                        {
                            var lookupRecord = lViewColumn.Where(m => m.ColumnNum == viewColObj.LookupIdCol).FirstOrDefault();
                            if (lookupRecord is not null)
                            {
                                if ((bool)lookupRecord.FilterField)
                                {
                                    viewColObj.LookupType = Convert.ToInt16(Enums.geViewColumnsLookupType.ltLookup);
                                    viewColObj.LookupIdCol = viewColObj.LookupIdCol;
                                    viewColObj.TabOrder = iCount;
                                    mcFilterColumns.Add(iCount, viewColObj);
                                }
                            }
                        }
                        iCount = iCount + 1;
                    }
                }
                return mcFilterColumns;
            }
            catch (Exception)
            {
                throw;
            } // ex

        }

        public static void CreateViewsEntity(View oldViews, ref View newViews)
        {
            newViews.TableName = oldViews.TableName;
            newViews.ViewName = oldViews.ViewName;
            newViews.IncludeFileRoomOrder = oldViews.IncludeFileRoomOrder;
            newViews.IncludeTrackingLocation = oldViews.IncludeTrackingLocation;
            newViews.SearchableView = oldViews.SearchableView;
            newViews.MaxRecsPerFetch = oldViews.MaxRecsPerFetch;
            newViews.MaxRecsPerFetchDesktop = oldViews.MaxRecsPerFetchDesktop;

            if (string.IsNullOrEmpty(oldViews.SQLStatement))
            {
                newViews.SQLStatement = "Select * From [" + oldViews.TableName + "]";
            }
            else
            {
                newViews.SQLStatement = oldViews.SQLStatement;
            }

            newViews.InTaskList = oldViews.InTaskList;
            newViews.TaskListDisplayString = oldViews.TaskListDisplayString;
            newViews.ReportStylesId = "Default";
            if (oldViews.ViewOrder is not null)
            {
                if (0 is var arg25 && oldViews.ViewOrder is { } arg24 && arg24 != arg25)
                {
                    newViews.ViewOrder = oldViews.ViewOrder;
                }
            }
            if (newViews.Id == 0)
            {
                // newViews.TablesId = 0
                newViews.Visible = true;
                // newViews.ViewGroup = 0
                // newViews.VariableColWidth = 0
                // newViews.VariableRowHeight = 0
                // newViews.VariableFixedCols = 0
                newViews.RowHeight = (short?)0;
                // newViews.AddAllowed = 0
                newViews.ViewType = (short?)0;
                // newViews.UseExactRowCount = 0
                // newViews.Printable = 0
                // newViews.GrandTotal = 0
                // newViews.LeftIndent = 0
                // newViews.RightIndent = 0
                // newViews.SubViewId = 0
                // newViews.PrintWithoutChildren = 0
                // newViews.SuppressHeader = 0
                // newViews.SuppressFooter = 0
                // newViews.PrintFrozenOnly = 0
                // newViews.TrackingEverContained = 0
                // newViews.PrintImages = 0
                // newViews.PrintImageFullPage = 0
                // newViews.PrintImageFirstPageOnly = 0
                // newViews.PrintImageRedlining = 0
                // newViews.PrintImageLeftMargin = 0
                // newViews.PrintImageRightMargin = 0
                // newViews.PrintImageAllVersions = 0
                // newViews.ChildColumnHeaders = 0
                // newViews.SuppressImageDataRow = 0
                // newViews.SuppressImageFooter = 0
                newViews.DisplayMode = 1;
                // newViews.AutoRotateImage = 0
                // newViews.GrandTotalOnSepPage = 0
                // newViews.AltViewId = 0
                // newViews.DeleteGridAvail = 0
                // newViews.FiltersActive = 0
                // newViews.PrintAttachments = 0
                // newViews.MultiParent = 0
                // newViews.CustomFormView = 0
            }
        }

        // Public Shared Sub UpdateViewsEntity(ByVal oldViews As View, ByRef newViews As View)
        // newViews.TableName = oldViews.TableName
        // newViews.ViewName = oldViews.ViewName
        // newViews.IncludeFileRoomOrder = oldViews.IncludeFileRoomOrder
        // newViews.IncludeTrackingLocation = oldViews.IncludeTrackingLocation
        // newViews.SearchableView = oldViews.SearchableView
        // newViews.MaxRecsPerFetch = oldViews.MaxRecsPerFetch
        // If (String.IsNullOrEmpty(oldViews.SQLStatement)) Then
        // newViews.SQLStatement = "Select * From [" + oldViews.TableName + "]"
        // Else
        // newViews.SQLStatement = oldViews.SQLStatement
        // End If
        // newViews.InTaskList = oldViews.InTaskList
        // newViews.TaskListDisplayString = oldViews.TaskListDisplayString
        // newViews.ReportStylesId = "Default"
        // If Not oldViews.ViewOrder Is Nothing Then
        // If oldViews.ViewOrder <> 0 Then
        // newViews.ViewOrder = oldViews.ViewOrder
        // End If
        // End If

        // End Sub

        private static bool ParseDate(ref string sDateLine, string sDate, string sTableFieldName, string sOperator)
        {
            bool ParseDateRet = default;
            bool bValid = true;
            var bWildQuestion = default(bool);
            var bWildAsterisk = default(bool);
            int iIndex;
            int iPos;
            int iPos1;
            int iTemp;
            string sDateSep = "-";
            string sHour;
            string sOriginalDate;
            string sTemp;
            string sTimeSep = ":";
            string sTime;
            string sYear;

            var aiDate = new int[3, 2];
            var aiTime = new int[4, 2];

            sDateLine = string.Empty;
            if (sDate is null)
                sDate = string.Empty;
            sOriginalDate = sDate;

            if (sDate.Length > 0)
            {
                bWildAsterisk = sDate.IndexOf("*") > -1;
                bWildQuestion = sDate.IndexOf("?") > -1;

                if (!bWildAsterisk && !bWildQuestion)
                {
                    ;
                    bValid = false;
                    bValid = Information.IsDate(sDate);

                    if (bValid)
                    {
                        sDate = Strings.Format(Convert.ToDateTime(sDate), "MM/dd/yyyy");
                        sDateLine = sTableFieldName;

                        if (sOperator == "=")
                        {
                            sDateLine = "CONVERT(VARCHAR(" + sDate.Length.ToString() + "), " + sTableFieldName + ",120)";

                            if (Strings.Format(Convert.ToDateTime(sDate), "yyyy-MM-dd HH:mm:ss").IndexOf("00:00:00") > -1)
                            {
                                sDateLine = sDateLine + " LIKE '" + Strings.Format(Convert.ToDateTime(sDate), "yyyy-MM-dd") + "%'";
                            }
                            else
                            {
                                sDateLine = sDateLine + " LIKE '" + Strings.Format(Convert.ToDateTime(sDate), "yyyy-MM-dd HH:mm:ss") + "%'";
                            }
                        }
                        else if (sOperator == "BEG")
                        {
                            sDateLine = "CONVERT(VARCHAR(" + sDate.Length.ToString() + "), " + sTableFieldName + ",120)";

                            if (Strings.Format(Convert.ToDateTime(sDate), "yyyy-MM-dd HH:mm:ss").IndexOf("00:00:00") > -1)
                            {
                                sDateLine = sDateLine + " LIKE '" + Strings.Format(Convert.ToDateTime(sDate), "yyyy-MM-dd") + "%'";
                            }
                            else
                            {
                                sDateLine = sDateLine + " LIKE '" + Strings.Format(Convert.ToDateTime(sDate), "yyyy-MM-dd HH:mm:ss") + "%'";
                            }
                        }
                        else if (sOperator == "<>")
                        {
                            sDateLine = "CONVERT(VARCHAR(" + sDate.Length.ToString() + "), " + sTableFieldName + ",120)";

                            if (Strings.Format(Convert.ToDateTime(sDate), "yyyy-MM-dd HH:mm:ss").IndexOf("00:00:00") > -1)
                            {
                                sDateLine = sDateLine + " Not LIKE '" + Strings.Format(Convert.ToDateTime(sDate), "yyyy-MM-dd") + "%'";
                            }
                            else
                            {
                                sDateLine = sDateLine + " Not LIKE '" + Strings.Format(Convert.ToDateTime(sDate), "yyyy-MM-dd HH:mm:ss") + "%'";
                            }
                        }
                        else
                        {
                            sDateLine = sDateLine + " " + sOperator + " '" + sDate + "'";
                        }

                        return bValid;
                    }
                    else
                    {
                        bValid = true;
                    }
                }

                if (bWildAsterisk)
                    sDate = sDate.Substring(0, sDate.IndexOf("*"));

                if (bWildQuestion)
                {
                    sDate = sDate.Replace("??", "01");
                    sDate = sDate.Replace("0?", "01");
                    sDate = sDate.Replace("?", "0");
                }

                iPos = sDate.IndexOf(" ");
                if (iPos > -1)
                    sDate = sDate.Substring(iPos).Trim();

                if (Information.IsNumeric(sDate))
                {
                    iTemp = Convert.ToInt32(sDate);

                    if (iTemp > 31)
                    {
                        sTemp = ParseUnusualDate(iTemp, sDate, ref aiDate);

                        if (string.IsNullOrEmpty(sTemp))
                        {
                            sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }

                        sOriginalDate = sTemp;
                    }
                    else if (iTemp > 12)
                    {
                        aiDate[1, 0] = 1;
                        aiDate[1, 1] = sDate.Length;

                        if (aiDate[1, 1] > 2)
                        {
                            sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }
                    }
                    else
                    {
                        aiDate[0, 0] = 1;
                        aiDate[0, 1] = sDate.Length;
                        if (aiDate[0, 1] > 2)
                        {
                            sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }
                    }
                }
                else
                {
                    // at least one non-numeric character
                    iPos = 1;
                    iPos1 = 1;
                    sTemp = sDate;

                    while (iPos <= sDate.Length)
                    {
                        if (!Information.IsNumeric(Strings.Mid(sDate, iPos, 1)))
                        {
                            if (iPos1 != iPos)
                            {
                                ;
                                iTemp = Convert.ToInt32(Strings.Mid(sDate, iPos1, iPos - iPos1));

                                if (Information.Err().Number == 13) // type mismatch
                                {
                                    sDateLine = "Date must be in a numeric format.  (i.e. 01/01/2001)";
                                    ParseDateRet = false;
                                    return ParseDateRet;
                                }

                                if (!LoadDateTimeArray(ref aiDate, iTemp, iPos1, iPos - iPos1, 31, 12, false))
                                {
                                    ParseDateRet = false;
                                    return ParseDateRet;
                                }

                                iPos1 = iPos + 1;
                            }
                            else
                            {
                                iPos1 = iPos - 1;
                            }
                        }

                        iPos = iPos + 1;
                    }

                    if (iPos1 == 0)
                    {
                        sDateLine = "Date must be in a numeric format.  (i.e. 01/01/2001)";
                        ParseDateRet = false;
                        return ParseDateRet;
                    }

                    if (Strings.Len(Strings.Mid(sDate, iPos1)) > 0)
                    {
                        if (Information.IsNumeric(Strings.Mid(sDate, iPos1)))
                        {
                            ;
                            iTemp = Convert.ToInt32(Strings.Mid(sTemp, iPos1));

                            if (Information.Err().Number == 13) // type mismatch
                            {
                                sDateLine = "Date must be in a numeric format.  (i.e. 01/01/2001)";
                                ParseDateRet = false;
                                return ParseDateRet;
                            }

                            if (!LoadDateTimeArray(ref aiDate, iTemp, iPos1, iPos - iPos1, 31, 12, false))
                            {
                                ParseDateRet = false;
                                return ParseDateRet;
                            }
                        }
                    }

                    sDate = string.Empty;
                    ;

                    for (iIndex = 0; iIndex <= 2; iIndex++)
                    {
                        if (aiDate[iIndex, 0] > 0)
                            sDate = sDate + Strings.Mid(sTemp, aiDate[iIndex, 0], aiDate[iIndex, 1]) + sDateSep;

                        if (Information.Err().Number != 0)
                        {
                            sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }
                    }

                    sDate = sDate.Substring(0, sDate.Length - 1);

                    if (Information.Err().Number != 0)
                    {
                        sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                        ParseDateRet = false;
                        return ParseDateRet;
                    }

                    bValid = Information.IsDate(sDate);
                }

                if (!bValid)
                {
                    ParseDateRet = false;
                    return ParseDateRet;
                }

                sTime = string.Empty;
                iPos = sOriginalDate.IndexOf(" ");
                if (iPos > -1)
                    sTime = sOriginalDate.Substring(iPos);

                bWildAsterisk = bWildAsterisk | sTime.IndexOf("*") > -1;
                bWildQuestion = bWildQuestion | sTime.IndexOf("?") > -1;
                if (bWildAsterisk & sTime.Length > 0)
                    sTime = sTime.Substring(0, sTime.IndexOf("*"));

                if (bWildQuestion)
                {
                    sTime = sTime.Replace("??", "01");
                    sTime = sTime.Replace("?", "0");
                }

                if (Information.IsNumeric(sTime))
                {
                    iTemp = Convert.ToInt32(sTime);

                    if (iTemp > 59)
                    {
                        ParseDateRet = false;
                        return ParseDateRet;
                    }
                    else if (iTemp > 23)
                    {
                        aiTime[1, 0] = 1;
                        aiTime[1, 1] = sTime.Length;
                    }
                    else
                    {
                        aiTime[0, 0] = 1;
                        aiTime[0, 1] = sTime.Length;
                    }
                }
                else if (sTime.Length > 0)
                {
                    // at least one non-numeric character
                    iPos = 1;
                    iPos1 = 1;
                    sTemp = sTime;

                    while (iPos <= sTime.Length)
                    {
                        if (!Information.IsNumeric(Strings.Mid(sTime, iPos, 1)))
                        {
                            if (iPos1 != iPos)
                            {
                                ;
                                iTemp = Convert.ToInt32(Strings.Mid(sTime, iPos1, iPos - iPos1));

                                if (Information.Err().Number == 0)
                                {
                                    if (!LoadDateTimeArray(ref aiTime, iTemp, iPos1, iPos - iPos1, 59, 23, true))
                                    {
                                        ParseDateRet = false;
                                        return ParseDateRet;
                                    }

                                    iPos1 = iPos + 1;
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                                iPos1 = iPos - 1;
                            }
                        }

                        iPos = iPos + 1;
                    }

                    if (Strings.Len(Strings.Mid(sTime, iPos1)) > 0)
                    {
                        if (Information.IsNumeric(Strings.Mid(sTime, iPos1)))
                        {
                            iTemp = Convert.ToInt32(Strings.Mid(sTime, iPos1));

                            if (!LoadDateTimeArray(ref aiTime, iTemp, iPos1, iPos - iPos1, 59, 23, true))
                            {
                                ParseDateRet = false;
                                return ParseDateRet;
                            }
                        }
                        else
                        {
                            aiTime[3, 0] = iPos1;
                            aiTime[3, 1] = iPos - iPos1;
                        }
                    }

                    sTime = string.Empty;
                    ;

                    for (iIndex = 0; iIndex <= 3; iIndex++)
                    {
                        if (aiTime[iIndex, 0] > 0)
                        {
                            sTime = sTime + Strings.Mid(sTemp, aiTime[iIndex, 0], aiTime[iIndex, 1]);

                            if (Information.Err().Number != 0)
                            {
                                sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                                ParseDateRet = false;
                                return ParseDateRet;
                            }

                            if (iIndex < 2)
                                sTime = sTime + sTimeSep;
                        }
                    }

                    if (string.Compare(Strings.Right(sTime, 1), sTimeSep) == 0)
                        sTime = sTime.Substring(0, sTime.Length - 1);

                    if (Information.Err().Number != 0)
                    {
                        sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                        ParseDateRet = false;
                        return ParseDateRet;
                    }

                    bValid = Information.IsDate(sTime);
                }

                if (!bValid)
                {
                    ParseDateRet = false;
                    return ParseDateRet;
                }
            }

            sDate = sOriginalDate;
            sTime = string.Empty;

            if (sDate.Length == 0)
            {
                sDateLine = sTableFieldName;

                switch (sOperator ?? "")
                {
                    case "=":
                    case "<":
                    case "<=":
                        {
                            sDateLine = sDateLine + " Is NULL";
                            break;
                        }
                    case "<>":
                    case ">":
                        {
                            sDateLine = sDateLine + " Is Not NULL";
                            break;
                        }
                    case ">=":
                        {
                            sDateLine = string.Empty;
                            ParseDateRet = true;
                            return ParseDateRet;
                        }

                    default:
                        {
                            sDateLine = "Filter Data is required when using the \"IN\" or \"BEG\" Operators.";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }
                }
            }
            else
            {
                if (bWildAsterisk | bWildQuestion)
                {
                    if (string.Compare(sOperator, "=") != 0 & string.Compare(sOperator, "<>") != 0)
                    {
                        sDateLine = "The wildcards \"*\" and \"?\" can only be used with the \"=\" or \"<>\" Operators.";
                        ParseDateRet = false;
                        return ParseDateRet;
                    }
                }

                iPos = sDate.IndexOf(" ");

                if (iPos == -1)
                {
                    sDate = string.Empty;
                    sYear = string.Empty;
                    sTemp = sOriginalDate;

                    bWildAsterisk = sTemp.IndexOf("*") > -1;
                    bWildQuestion = sTemp.IndexOf("?") > -1;
                    if (bWildAsterisk)
                        sTemp = sTemp.Substring(0, sTemp.IndexOf("*"));
                    ;

                    for (iIndex = 0; iIndex <= 2; iIndex++)
                    {
                        if (aiDate[iIndex, 0] > 0)
                            sDate = sDate + Strings.Mid(sTemp, aiDate[iIndex, 0], aiDate[iIndex, 1]) + sDateSep;

                        if (Information.Err().Number != 0)
                        {
                            sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }
                    }

                    sDate = sDate.Substring(0, sDate.Length - 1);

                    if (Information.Err().Number != 0)
                    {
                        sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                        ParseDateRet = false;
                        return ParseDateRet;
                    }

                    sYear = string.Empty;

                    if (aiDate[2, 0] > 0 & aiDate[2, 1] != 4)
                    {
                        ;
                        sYear = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetYear(Convert.ToDateTime(sDate)).ToString();
                    }

                    sTemp = sOriginalDate;
                    if (bWildAsterisk)
                        sTemp = sTemp.Substring(0, sTemp.IndexOf("*"));
                    if (bWildQuestion)
                        sTemp = sTemp.Replace("?", "_");
                    sDate = string.Empty;

                    if (sYear.Length > 0)
                    {
                        sDate = sYear + sDateSep;
                    }
                    else if (aiDate[2, 0] > 0)
                    {
                        ;
                        sDate = Strings.Left("0000", 4 - aiDate[2, 1]) + Strings.Mid(sTemp, aiDate[2, 0], aiDate[2, 1]) + sDateSep;

                        if (Information.Err().Number != 0)
                        {
                            sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }
                    }

                    else
                    {
                        sDate = "____" + sDateSep;
                    };

                    for (iIndex = 0; iIndex <= 1; iIndex++)
                    {
                        if (aiDate[iIndex, 0] > 0)
                        {
                            sDate = sDate + Strings.Left("00", 2 - aiDate[iIndex, 1]) + Strings.Mid(sTemp, aiDate[iIndex, 0], aiDate[iIndex, 1]) + sDateSep;
                        }
                        else
                        {
                            sDate = sDate + "__" + sDateSep;
                        }

                        if (Information.Err().Number != 0)
                        {
                            sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }
                    }

                    sDate = sDate.Substring(0, sDate.Length - 1);

                    if (Information.Err().Number != 0)
                    {
                        sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                        ParseDateRet = false;
                        return ParseDateRet;
                    }
                }

                else
                {
                    sDate = string.Empty;
                    sYear = string.Empty;
                    sTemp = sOriginalDate;

                    bWildAsterisk = sTemp.IndexOf("*") > -1;
                    bWildQuestion = sTemp.IndexOf("?") > -1;
                    if (bWildAsterisk)
                        sTemp = sTemp.Substring(0, sTemp.IndexOf("*"));
                    ;

                    for (iIndex = 0; iIndex <= 2; iIndex++)
                    {
                        if (aiDate[iIndex, 0] > 0)
                            sDate = sDate + Strings.Mid(sTemp, aiDate[iIndex, 0], aiDate[iIndex, 1]) + sDateSep;

                        if (Information.Err().Number != 0)
                        {
                            sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }
                    }

                    sDate = sDate.Substring(0, sDate.Length - 1);

                    if (Information.Err().Number != 0)
                    {
                        sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                        ParseDateRet = false;
                        return ParseDateRet;
                    }

                    sYear = string.Empty;

                    if (aiDate[2, 0] > 0 & aiDate[2, 1] != 4)
                    {
                        ;
                        sYear = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetYear(Convert.ToDateTime(sDate)).ToString();
                    }

                    sTemp = sOriginalDate;
                    if (bWildAsterisk)
                        sTemp = sTemp.Substring(0, sTemp.IndexOf("*"));
                    if (bWildQuestion)
                        sTemp = sTemp.Replace("?", "_");
                    sDate = string.Empty;

                    if (Strings.Len(sYear) > 0)
                    {
                        sDate = sYear + sDateSep;
                    }
                    else if (aiDate[2, 0] > 0)
                    {
                        ;
                        sDate = Strings.Left("0000", 4 - aiDate[2, 1]) + Strings.Mid(sTemp, aiDate[2, 0], aiDate[2, 1]) + sDateSep;

                        if (Information.Err().Number != 0)
                        {
                            sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }
                    }

                    else
                    {
                        sDate = "____" + sDateSep;
                    };
                    for (iIndex = 0; iIndex <= 1; iIndex++)
                    {
                        if (aiDate[iIndex, 0] > 0)
                        {
                            sDate = sDate + Strings.Left("00", 2 - aiDate[iIndex, 1]) + Strings.Mid(sTemp, aiDate[iIndex, 0], aiDate[iIndex, 1]) + sDateSep;
                        }
                        else
                        {
                            sDate = sDate + "__" + sDateSep;
                        }

                        if (Information.Err().Number != 0)
                        {
                            sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }
                    }

                    sDate = sDate.Substring(0, sDate.Length - 1);

                    if (Information.Err().Number != 0)
                    {
                        sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                        ParseDateRet = false;
                        return ParseDateRet;
                    }


                    sTime = string.Empty;
                    sTemp = Strings.Mid(sOriginalDate, iPos + 1);

                    bWildAsterisk = sTemp.IndexOf("*") > -1;
                    bWildQuestion = sTemp.IndexOf("?") > -1;
                    if (bWildAsterisk)
                        sTemp = sTemp.Substring(0, sTemp.IndexOf("*"));
                    if (bWildQuestion)
                        sTemp = sTemp.Replace("?", "_");
                    ;

                    for (iIndex = 0; iIndex <= 3; iIndex++)
                    {
                        if (aiTime[iIndex, 0] > 0)
                        {
                            sTime = sTime + Strings.Mid(sTemp, aiTime[iIndex, 0], aiTime[iIndex, 1]);

                            if (Information.Err().Number != 0)
                            {
                                sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                                ParseDateRet = false;
                                return ParseDateRet;
                            }

                            if (iIndex < 2)
                                sTime = sTime + sTimeSep;
                        }
                    }

                    if (string.Compare(Strings.Right(sTime, 1), sTimeSep) == 0)
                        sTime = sTime.Substring(0, sTime.Length - 1);

                    if (Information.Err().Number != 0)
                    {
                        sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                        ParseDateRet = false;
                        return ParseDateRet;
                    }

                    sHour = string.Empty;

                    if (Strings.InStr(sTime, sTimeSep) > 0)
                    {
                        ;
                        sHour = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetHour(Convert.ToDateTime(sTime)).ToString();
                    };

                    if (Strings.Len(sHour) > 0)
                    {
                        sTime = Strings.Left("00", 2 - Strings.Len(sHour)) + sHour;
                    }
                    else
                    {
                        sTime = Strings.Left("00", 2 - aiTime[0, 1]) + Strings.Mid(sTemp, aiTime[0, 0], aiTime[0, 1]);
                    }

                    if (Information.Err().Number != 0)
                    {
                        sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                        ParseDateRet = false;
                        return ParseDateRet;
                    }

                    for (iIndex = 1; iIndex <= 2; iIndex++)
                    {
                        if (aiTime[iIndex, 0] > 0)
                            sTime = sTime + sTimeSep + Strings.Left("00", 2 - aiTime[iIndex, 1]) + Strings.Mid(sTemp, aiTime[iIndex, 0], aiTime[iIndex, 1]);

                        if (Information.Err().Number != 0)
                        {
                            sDateLine = "Date must be in a valid format.  (i.e. 01/01/2001)";
                            ParseDateRet = false;
                            return ParseDateRet;
                        }
                    }
                    if (Strings.Len(sTime) > 0)
                        sDate = sDate + " " + sTime;
                }

                sDateLine = sDateLine + "CONVERT(VARCHAR(" + sDate.Length.ToString() + "), ";
                sDateLine = sDateLine + sTableFieldName;
                sDateLine = sDateLine + ",120)";

                if (sOperator == "=")
                {
                    sDateLine = sDateLine + " LIKE ";
                }
                else if (sOperator == "BEG")
                {
                    sDateLine = sDateLine + " LIKE ";
                }
                else if (sOperator == "<>")
                {
                    sDateLine = sDateLine + " Not LIKE ";
                }
                else
                {
                    sDateLine = sDateLine + " " + sOperator + " ";
                }

                sDateLine = sDateLine + "'" + sDate + "'";
            }

            // Debug.Print sDateLine
            ParseDateRet = bValid;
            return ParseDateRet;

        }

        private static bool LoadDateTimeArray(ref int[,] aiDateTime, int iValue, int iPos, int iLength, int iInvalid1, int iInvalid2, bool bInvalid1IsAlwaysInvalid)
        {
            bool LoadDateTimeArrayRet = default;

            if (iValue > iInvalid1)
            {
                if (bInvalid1IsAlwaysInvalid)
                {
                    LoadDateTimeArrayRet = false;
                    return LoadDateTimeArrayRet;
                }

                if (aiDateTime[2, 0] > 0)
                {
                    // too many values larger than iInvalid1 to be a valid date/time
                    LoadDateTimeArrayRet = false;
                    return LoadDateTimeArrayRet;
                }

                aiDateTime[2, 0] = iPos;
                aiDateTime[2, 1] = iLength;
            }
            else if (iValue > iInvalid2)
            {
                if (aiDateTime[1, 0] > 0)
                {
                    if (aiDateTime[2, 0] > 0)
                    {
                        // too many values larger than iInvalid2 to be a valid date/time
                        LoadDateTimeArrayRet = false;
                        return LoadDateTimeArrayRet;
                    }
                    else
                    {
                        aiDateTime[2, 0] = iPos;
                        aiDateTime[2, 1] = iLength;
                    }
                }
                else
                {
                    aiDateTime[1, 0] = iPos;
                    aiDateTime[1, 1] = iLength;
                }
            }
            else if (aiDateTime[0, 0] > 0)
            {
                if (aiDateTime[1, 0] > 0)
                {
                    aiDateTime[2, 0] = iPos;
                    aiDateTime[2, 1] = iLength;
                }
                else
                {
                    aiDateTime[1, 0] = iPos;
                    aiDateTime[1, 1] = iLength;
                }
            }
            else
            {
                aiDateTime[0, 0] = iPos;
                aiDateTime[0, 1] = iLength;
            }

            LoadDateTimeArrayRet = true;
            return LoadDateTimeArrayRet;
        }

        private static string ParseUnusualDate(int iDate, string sDate, ref int[,] aiDate)
        {
            if (iDate > 1753 && sDate.Length == 4 || sDate.Length == 2)
            {
                // assume year (1753 is the same year SQL uses as the floor for a datetime value)
                aiDate[2, 0] = 1;
                aiDate[2, 1] = 4;
                return Convert.ToDateTime(string.Format("01/01/{0}", sDate)).Year.ToString();
            }

            var tempDate = default(DateTime);
            var standardFormat = default(bool);
            string separator = System.Globalization.DateTimeFormatInfo.CurrentInfo.DateSeparator;

            switch (sDate.Length)
            {
                case 3:
                    {
                        if (Information.IsDate(string.Format("0{0}/{1}", sDate.Substring(0, 1), sDate.Substring(1, 2))))
                        {
                            aiDate[0, 0] = 1;
                            aiDate[0, 1] = 1;
                            aiDate[1, 0] = 2;
                            aiDate[1, 1] = 2;
                        }
                        else if (Information.IsDate(string.Format("{0}/0{1}", sDate.Substring(0, 2), sDate.Substring(2, 1))))
                        {
                            aiDate[0, 0] = 1;
                            aiDate[0, 1] = 2;
                            aiDate[1, 0] = 2;
                            aiDate[1, 1] = 1;
                        }
                        else if (Information.IsDate(string.Format("{0}/{1}/{2}", sDate.Substring(0, 1), sDate.Substring(1, 1), sDate.Substring(2, 1))))
                        {
                            standardFormat = DateTime.TryParse(string.Format("{0}/{1}/{2}", sDate.Substring(0, 1), sDate.Substring(1, 1), sDate.Substring(2, 1)), out tempDate);
                        }

                        break;
                    }
                case 4:
                    {
                        if (Information.IsDate(string.Format("{0}/{1}", sDate.Substring(0, 2), sDate.Substring(2, 2))))
                        {
                            aiDate[0, 0] = 1;
                            aiDate[0, 1] = 2;
                            aiDate[1, 0] = 3;
                            aiDate[1, 1] = 2;
                        }
                        else if (Information.IsDate(string.Format("{0}/{1}/{2}", sDate.Substring(0, 1), sDate.Substring(1, 2), sDate.Substring(3, 1))))
                        {
                            standardFormat = DateTime.TryParse(string.Format("{0}/{1}/{2}", sDate.Substring(0, 1), sDate.Substring(1, 2), sDate.Substring(3, 1)), out tempDate);
                        }
                        else if (Information.IsDate(string.Format("{0}/{1}/{2}", sDate.Substring(0, 2), sDate.Substring(2, 1), sDate.Substring(3, 1))))
                        {
                            standardFormat = DateTime.TryParse(string.Format("{0}/{1}/{2}", sDate.Substring(0, 2), sDate.Substring(2, 1), sDate.Substring(3, 1)), out tempDate);
                        }
                        else
                        {
                            aiDate[2, 0] = 1;
                            aiDate[2, 1] = 4;
                        }

                        break;
                    }
                case 5:
                    {
                        if (Information.IsDate(string.Format("{0}/{1}/{2}", sDate.Substring(0, 1), sDate.Substring(1, 2), sDate.Substring(3, 2))))
                        {
                            standardFormat = DateTime.TryParse(string.Format("{0}/{1}/{2}", sDate.Substring(0, 1), sDate.Substring(1, 2), sDate.Substring(3, 2)), out tempDate);
                        }
                        else if (Information.IsDate(string.Format("{0}/{1}/{2}", sDate.Substring(0, 2), sDate.Substring(3, 1), sDate.Substring(3, 2))))
                        {
                            standardFormat = DateTime.TryParse(string.Format("{0}/{1}/{2}", sDate.Substring(0, 2), sDate.Substring(3, 1), sDate.Substring(3, 2)), out tempDate);
                        }
                        else
                        {
                            standardFormat = DateTime.TryParse(string.Format("{0}/{1}/{2}", sDate.Substring(0, 1), sDate.Substring(1, 1), sDate.Substring(2, 3)), out tempDate);
                        }

                        break;
                    }

                default:
                    {
                        if (Information.IsDate(string.Format("{0}/{1}/{2}", sDate.Substring(0, 2), sDate.Substring(2, 2), sDate.Substring(4, sDate.Length - 4))))
                        {
                            standardFormat = DateTime.TryParse(string.Format("{0}/{1}/{2}", sDate.Substring(0, 2), sDate.Substring(2, 2), sDate.Substring(4, sDate.Length - 4)), out tempDate);
                        }
                        else
                        {
                            sDate = string.Empty;
                        }

                        break;
                    }
            }

            if (!standardFormat)
                return sDate;

            aiDate[0, 0] = 1;
            aiDate[0, 1] = 2;
            aiDate[1, 0] = 3;
            aiDate[1, 1] = 2;
            aiDate[2, 0] = 5;
            aiDate[2, 1] = 4;
            return tempDate.ToString("MM/dd/yyyy").Replace(separator, string.Empty);
        }

        private static bool ConvertFilterData(ref string sFilterData, ref string sCharEscape, ref bool bConvertLiterals)
        {
            bool ConvertFilterDataRet = default;
            int iIndex = 0;
            string sCharBackslash = string.Empty;
            string sCharAsterisk = string.Empty;
            string sCharQuestionMark = string.Empty;


            for (iIndex = 1; iIndex <= 250; iIndex++)
            {
                if (iIndex != 92) // don't use backslash(92) for ESCAPE character
                {
                    if (Strings.InStr(1, sFilterData, Convert.ToString(Strings.Chr(iIndex)), CompareMethod.Binary) == 0)
                    {
                        if (Strings.Len(sCharEscape) == 0)
                        {
                            sCharEscape = Convert.ToString(Strings.Chr(iIndex));
                        }
                        else if (Strings.Len(sCharBackslash) == 0)
                        {
                            sCharBackslash = Convert.ToString(Strings.Chr(iIndex));
                        }
                        else if (Strings.Len(sCharAsterisk) == 0)
                        {
                            sCharAsterisk = Convert.ToString(Strings.Chr(iIndex));
                        }
                        else if (Strings.Len(sCharQuestionMark) == 0)
                        {
                            sCharQuestionMark = Convert.ToString(Strings.Chr(iIndex));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            if (Strings.Len(sCharEscape) == 0)
            {
                sCharEscape = Convert.ToString(Strings.Chr(251));
            }
            if (Strings.Len(sCharBackslash) == 0)
            {
                sCharBackslash = Convert.ToString(Strings.Chr(252));
            }
            if (Strings.Len(sCharAsterisk) == 0)
            {
                sCharAsterisk = Convert.ToString(Strings.Chr(253));
            }
            if (Strings.Len(sCharQuestionMark) == 0)
            {
                sCharQuestionMark = Convert.ToString(Strings.Chr(254));
            }

            sFilterData = Strings.Replace(sFilterData, "_", sCharEscape + "_");
            sFilterData = Strings.Replace(sFilterData, "%", sCharEscape + "%");

            bConvertLiterals = Strings.InStr(sFilterData, "_") > 0 | Strings.InStr(sFilterData, "%") > 0;

            sFilterData = Strings.Replace(sFilterData, @"\\", sCharBackslash);
            sFilterData = Strings.Replace(sFilterData, @"\*", sCharAsterisk);
            sFilterData = Strings.Replace(sFilterData, @"\?", sCharQuestionMark);

            ConvertFilterDataRet = Strings.InStr(sFilterData, "*") > 0 | Strings.InStr(sFilterData, "?") > 0;

            sFilterData = Strings.Replace(sFilterData, "*", "%");
            sFilterData = Strings.Replace(sFilterData, "?", "_");

            sFilterData = Strings.Replace(sFilterData, sCharAsterisk, "*");
            sFilterData = Strings.Replace(sFilterData, sCharBackslash, @"\");
            sFilterData = Strings.Replace(sFilterData, sCharQuestionMark, "?");
            return ConvertFilterDataRet;

        }

        private static int ComputeParenCount(ref string sParenString)
        {
            int ComputeParenCountRet = default;
            int iStart;
            int iPos;


            ComputeParenCountRet = 0;
            // Add Open Parens
            iStart = 1;

            do
            {
                iPos = Strings.InStr(iStart, sParenString, "(");

                if (iPos > 0)
                {
                    ComputeParenCountRet = ComputeParenCountRet + 1;
                    iStart = iPos + 1;
                }
                else
                {
                    break;
                }
            }
            while (true);
            // Subtract Closed Parens
            iStart = 1;

            do
            {
                iPos = Strings.InStr(iStart, sParenString, ")");

                if (iPos > 0)
                {
                    ComputeParenCountRet = ComputeParenCountRet - 1;
                    iStart = iPos + 1;
                }
                else
                {
                    break;
                }
            }
            while (true);

            return ComputeParenCountRet;

        }

        public static List<Table> GetUsedTrackingTables(Table oTables, IQueryable<Table> trackingTable)
        {
            var cUsedTables = new List<Table>();
            if (oTables is not null)
            {
                foreach (Table oTmpTables in trackingTable)
                {
                    // If container then only parent Containers should display
                    if ((bool)(0 is var arg27 && oTables.TrackingTable is { } arg26 ? arg26 > arg27 : (bool?)null))
                    {
                        if ((bool)(oTables.TrackingTable is var arg28 && oTmpTables.TrackingTable is { } arg29 && arg28.HasValue ? arg28 > arg29 : (bool?)null))
                        {
                            cUsedTables.Add(oTmpTables);
                        }
                    }
                    else if ((bool)(0 is var arg31 && oTmpTables.TrackingTable is { } arg30 ? arg30 > arg31 : (bool?)null))
                    {
                        cUsedTables.Add(oTmpTables);
                    }
                }
            }

            return cUsedTables;
        }

        public static bool CreateCoalesceFields(List<Table> cTables, IQueryable<Databas> cDatabase, ref string sFields, bool bIncludeAS = true)
        {
            bool CreateCoalesceFieldsRet = default;
            SchemaColumns oSchemaColumns;
            // Dim aSimpleFields() As String
            // Dim aCompleteFields() As String
            // Dim iIndex As Integer
            string sPrefixOne;
            string sFieldNameOne;
            string sPrefixTwo;
            string sFieldNameTwo;
            ADODB.Connection sADOCon;
            sFields = "";
            CreateCoalesceFieldsRet = false;
            if (cTables is null)
                return CreateCoalesceFieldsRet;

            foreach (Table oTmpTables in cTables)
            {
                sPrefixOne = oTmpTables.DescFieldPrefixOne;
                sFieldNameOne = oTmpTables.DescFieldNameOne;
                sPrefixTwo = oTmpTables.DescFieldPrefixTwo;
                sFieldNameTwo = oTmpTables.DescFieldNameTwo;
                sADOCon = DataServices.DBOpen(oTmpTables, cDatabase);
                if (string.IsNullOrEmpty(sFieldNameOne))
                    sFieldNameOne = DatabaseMap.RemoveTableNameFromField(oTmpTables.IdFieldName);
                if (!string.IsNullOrEmpty(sPrefixOne))
                    sFields += "'" + sPrefixOne + " ' + ";
                oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(oTmpTables.TableName, sADOCon, sFieldNameOne).Where(m => m.ColumnName.Trim().ToLower().Equals(sFieldNameOne.Trim().ToLower())).FirstOrDefault();
                // oSchemaColumns = oTmpTables.ColumnSchema(RemoveTableNameFromField(sFieldNameOne))
                if (oSchemaColumns.IsString)
                {
                    sFields += oTmpTables.TableName + "." + sFieldNameOne;
                }
                else
                {
                    sFields += "CAST(" + oTmpTables.TableName + "." + sFieldNameOne + " AS VARCHAR)";
                }

                oSchemaColumns = null;

                if (!string.IsNullOrEmpty(sFieldNameTwo))
                {
                    sFields += " + ' " + sPrefixTwo + " ' + ";
                    oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(oTmpTables.TableName, sADOCon, sFieldNameTwo).Where(m => m.ColumnName.Trim().ToLower().Equals(sFieldNameTwo.Trim().ToLower())).FirstOrDefault();
                    // oSchemaColumns = oTmpTables.ColumnSchema(RemoveTableNameFromField(sFieldNameTwo))
                    if (oSchemaColumns.IsString)
                    {
                        sFields += oTmpTables.TableName + "." + sFieldNameTwo + ", ";
                    }
                    else
                    {
                        sFields += "CAST(" + oTmpTables.TableName + "." + sFieldNameTwo + " AS VARCHAR), ";
                    }

                    oSchemaColumns = null;
                }
                else
                {
                    sFields += ", ";
                }
            }

            if (!string.IsNullOrEmpty(sFields))
            {
                sFields = string.Format("COALESCE({0}, 'Never Tracked')", sFields.Substring(0, sFields.Length - 2));
                if (bIncludeAS)
                    sFields += " AS " + ReportsModel.TRACKED_LOCATION_NAME;
                CreateCoalesceFieldsRet = true;
            }

            return CreateCoalesceFieldsRet;

        }

        public static string ProcessFilter(List<ViewFilter> cViewFilters, List<ViewColumn> cViewColumns, IQueryable<Table> cTables, IQueryable<Databas> cDatabase, View oViews, Table oTable, bool bActiveFilters, ref string sReturnStr, bool bQBEFilter, bool bConvertMemoField = true)
        {
            string ProcessFilterRet = default;
            bool bContainsWildcards = false;
            bool bConvertLiterals = false;
            bool bDidOne = false;
            bool bIsCheckBoxCol = false;
            bool bIsDateCol = false;
            bool bIsMemoCol = false;
            bool bIsStringCol = false;
            bool bUseFieldOnly = false;
            bool bValid = false;
            List<Table> cUsedTables;
            int iLastConnectorPos = 0;
            int iParenCount = 0;
            int lIndex = 0;
            string sCharEscape = string.Empty;
            string sEachLine = string.Empty;
            string sEachTableFieldName = string.Empty;
            string sFieldName = string.Empty;
            string sLastConnector = string.Empty;
            string sLookupTableName = string.Empty;
            string sFilterData = string.Empty;
            string sTempFieldName = string.Empty;
            SchemaColumns oSchemaColumns = null;
            Table oTables = null;
            ViewColumn oViewColumns = null;
            ADODB.Connection sADOCon;

            sReturnStr = "";
            ProcessFilterRet = "";
            if (bActiveFilters)
            {
                if (oViews is not null)
                {
                    iParenCount = 0;
                    bDidOne = false;
                    sLastConnector = "";
                    do  // do loop used for convenient bail point (using exit do)
                    {
                        bool exitDo5 = false;
                        bool exitDo6 = false;
                        bool exitDo7 = false;
                        bool exitDo8 = false;
                        bool exitDo9 = false;
                        bool exitDo10 = false;
                        bool exitDo11 = false;
                        bool exitDo12 = false;
                        bool exitDo13 = false;
                        bool exitDo14 = false;
                        bool exitDo15 = false;
                        foreach (var oViewFilters in cViewFilters)
                        {
                            bUseFieldOnly = false;
                            sReturnStr = sReturnStr + oViewFilters.OpenParen;

                            if ((bool)oViewFilters.Active)
                            {

                                if (bDidOne & string.IsNullOrEmpty(sLastConnector))
                                {
                                    ProcessFilterRet = "Missing Join Connector";
                                    sReturnStr = "";
                                    exitDo5 = true;
                                    break;
                                }
                                bDidOne = true;
                                sLastConnector = "";

                                if (oViewFilters.ColumnNum > -1)
                                {
                                    oViewColumns = cViewColumns.FirstOrDefault(m => m.ColumnNum == oViewFilters.ColumnNum);
                                    if (oViewColumns is not null)
                                    {
                                        sFieldName = DatabaseMap.RemoveTableNameFromField(oViewColumns.FieldName);
                                        sLookupTableName = oViewColumns.FieldName;
                                        if (Strings.InStr(sLookupTableName, ".") > 1)
                                        {
                                            sLookupTableName = Strings.Left(sLookupTableName, Strings.InStr(sLookupTableName, ".") - 1);
                                            if (sLookupTableName.StartsWith("["))
                                                sLookupTableName = sLookupTableName.Substring(1, sLookupTableName.Length - 1);
                                            if (sLookupTableName.EndsWith("]"))
                                                sLookupTableName = sLookupTableName.Substring(0, sLookupTableName.Length - 1);
                                        }
                                        else if (oTable is not null)
                                        {
                                            sLookupTableName = oTable.TableName;
                                        }
                                        else
                                        {
                                            ProcessFilterRet = "Missing Default Table Object";
                                            sReturnStr = "";
                                            exitDo6 = true;
                                            break;
                                        }
                                        oTables = cTables.Where(m => m.TableName.Trim().ToLower().Equals(sLookupTableName.Trim().ToLower())).FirstOrDefault();
                                        sADOCon = DataServices.DBOpen(oTables, cDatabase);

                                        if (oTables is not null)
                                        {
                                            oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(oTables.TableName, sADOCon, sFieldName).Where(m => m.ColumnName.Trim().ToLower().Equals(sFieldName.Trim().ToLower())).FirstOrDefault();

                                            if (oSchemaColumns is not null)
                                            {
                                                bIsStringCol = oSchemaColumns.IsString;
                                                bIsDateCol = oSchemaColumns.IsADate;
                                                bIsMemoCol = false;
                                                bIsCheckBoxCol = oSchemaColumns.DataType == Enums.DataTypeEnum.rmBoolean | (oSchemaColumns.DataType == Enums.DataTypeEnum.rmTinyInt | oSchemaColumns.DataType == Enums.DataTypeEnum.rmUnsignedTinyInt) & Strings.StrComp(oViewColumns.EditMask, "Yes/No", CompareMethod.Text) == 0;
                                                if (bConvertMemoField)
                                                {
                                                    bIsMemoCol = bIsStringCol & (oSchemaColumns.CharacterMaxLength <= 0 | oSchemaColumns.CharacterMaxLength > 8000);
                                                }
                                            }
                                            else if (Strings.StrComp(DatabaseMap.RemoveTableNameFromField(sFieldName), ReportsModel.TRACKED_LOCATION_NAME, Constants.vbTextCompare) == 0)
                                            {
                                                bIsStringCol = true;
                                                bIsDateCol = false;
                                                bIsMemoCol = false;
                                                bIsCheckBoxCol = false;
                                                bUseFieldOnly = true;
                                                sLookupTableName = "";
                                                sFieldName = oViewColumns.AlternateFieldName;
                                                cUsedTables = ViewModel.GetUsedTrackingTables(oTables, cTables.Where(m => m.TrackingTable > 0));
                                                if (CreateCoalesceFields(cUsedTables, cDatabase, ref sTempFieldName, false))
                                                {
                                                    sFieldName = sTempFieldName;
                                                }
                                                cUsedTables = null;
                                            }

                                            else if (Strings.Len(oViewColumns.AlternateFieldName) > 0)
                                            {
                                                bIsStringCol = true;
                                                bIsDateCol = false;
                                                bIsMemoCol = false;
                                                bIsCheckBoxCol = false;
                                                bUseFieldOnly = true;
                                                sLookupTableName = "";
                                                sFieldName = oViewColumns.AlternateFieldName;
                                            }
                                            else
                                            {
                                                ProcessFilterRet = "Invalid Column name: [" + sFieldName + "] In Table: [" + sLookupTableName + "]";
                                                sReturnStr = "";
                                                exitDo7 = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            ProcessFilterRet = "Invalid Table: [" + sLookupTableName + "]";
                                            sReturnStr = "";
                                            exitDo8 = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        ProcessFilterRet = "Invalid ViewColumns Reference";
                                        sReturnStr = "";
                                        exitDo9 = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    sADOCon = DataServices.DBOpen(oTable, cDatabase);
                                    var oTableSchemaColumn = SchemaInfoDetails.GetSchemaInfo(oTable.TableName, sADOCon, oTable.IdFieldName).Where(m => m.ColumnName.Trim().ToLower().Equals(oTable.IdFieldName.Trim().ToLower())).FirstOrDefault();
                                    if (oTableSchemaColumn is not null)
                                    {
                                        bIsStringCol = DataServices.IsAStringType(oTableSchemaColumn.DataType);
                                        bIsDateCol = DataServices.IsADateType(oTableSchemaColumn.DataType);
                                    }

                                    bIsMemoCol = false;
                                    bIsCheckBoxCol = false;
                                    sLookupTableName = "";
                                    sFieldName = DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName);
                                }

                                if (!bUseFieldOnly & Strings.Len(sLookupTableName) > 0)
                                {
                                    sEachTableFieldName = "[" + sLookupTableName + "].";
                                }
                                else
                                {
                                    sEachTableFieldName = "";
                                }

                                if (bUseFieldOnly)
                                {
                                    sEachTableFieldName = sEachTableFieldName + sFieldName;
                                }
                                else
                                {
                                    sEachTableFieldName = sEachTableFieldName + "[" + sFieldName + "]";
                                }

                                if (bIsDateCol)
                                {
                                    if (!ParseDate(ref sEachLine, oViewFilters.FilterData, sEachTableFieldName, oViewFilters.Operator))
                                    {
                                        if (Strings.Len(sEachLine) > 0)
                                        {
                                            ProcessFilterRet = sEachLine + Constants.vbCrLf + Constants.vbCrLf + "Data: [" + oViewFilters.FilterData + "]";
                                        }
                                        else
                                        {
                                            ProcessFilterRet = "Bad Date In Data: [" + oViewFilters.FilterData + "]";
                                        }

                                        sReturnStr = "";
                                        exitDo10 = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    sEachLine = sEachTableFieldName;

                                    bool exitDo1 = false;
                                    bool exitDo2 = false;
                                    bool exitDo3 = false;
                                    bool exitDo4 = false;
                                    switch (oViewFilters.Operator ?? "")
                                    {
                                        case "LIST":
                                            {
                                                if (Strings.Len(oViewFilters.FilterData) > 0)
                                                {
                                                    if (bIsMemoCol)
                                                    {
                                                        sEachLine = "CONVERT(VARCHAR(8000), " + sEachTableFieldName + ")";
                                                    }

                                                    sEachLine = sEachLine + " IN  (" + oViewFilters.FilterData + ")";
                                                }

                                                break;
                                            }
                                        case "IN":
                                        case "BEG":
                                            {
                                                if (Strings.Len(oViewFilters.FilterData) == 0)
                                                {
                                                    ProcessFilterRet = Languages.get_Translation("msgViewModelFilterDataIsRequiredWhenUsingInBeg");
                                                    sReturnStr = "";
                                                    exitDo11 = exitDo1 = true;
                                                    break;
                                                }

                                                sEachLine = sEachLine + " LIKE ";
                                                sFilterData = Strings.Replace(oViewFilters.FilterData, "'", "''");
                                                bContainsWildcards = ConvertFilterData(ref sFilterData, ref sCharEscape, ref bConvertLiterals);

                                                if (Strings.Right(sFilterData, 1) == "%")
                                                {
                                                    sFilterData = Strings.Left(sFilterData, Strings.Len(sFilterData) - 1);
                                                }

                                                if (bConvertLiterals)
                                                {
                                                    if (Strings.StrComp(oViewFilters.Operator, "BEG", CompareMethod.Text) == 0)
                                                    {
                                                        sEachLine = sEachLine + "'" + sFilterData + "%' ESCAPE '" + sCharEscape + "'";
                                                    }
                                                    else
                                                    {
                                                        sEachLine = sEachLine + "'%" + sFilterData + "%' ESCAPE '" + sCharEscape + "'";
                                                    }
                                                }
                                                else if (Strings.StrComp(oViewFilters.Operator, "BEG", CompareMethod.Text) == 0)
                                                {
                                                    sEachLine = sEachLine + "'" + sFilterData + "%'";
                                                }
                                                else
                                                {
                                                    sEachLine = sEachLine + "'%" + sFilterData + "%'";
                                                }

                                                break;
                                            }

                                        default:
                                            {
                                                if (bIsMemoCol)
                                                {
                                                    sEachLine = "CONVERT(VARCHAR(8000), " + sEachTableFieldName + ")";
                                                }

                                                if (Strings.Len(oViewFilters.FilterData) == 0)
                                                {
                                                    bool exitDo = false;
                                                    switch (oViewFilters.Operator ?? "")
                                                    {
                                                        case ">":
                                                        case "<>":
                                                            {
                                                                if (bIsStringCol)
                                                                {
                                                                    sEachLine = "(" + sEachLine + " > '')";
                                                                }
                                                                else if (!bIsCheckBoxCol)
                                                                {
                                                                    sEachLine = "(" + sEachLine + " " + oViewFilters.Operator + " 0)";
                                                                }

                                                                break;
                                                            }
                                                        case ">=":
                                                            {
                                                                if (bIsStringCol)
                                                                {
                                                                    sEachLine = "((" + sEachLine + " Is NULL) OR (" + sEachTableFieldName + " > ''))";
                                                                }
                                                                else
                                                                {
                                                                    sEachLine = "((" + sEachLine + " Is NULL) OR (" + sEachTableFieldName + " >= 0))";
                                                                }

                                                                break;
                                                            }
                                                        case "=":
                                                        case "<=":
                                                            {
                                                                if (bIsStringCol)
                                                                {
                                                                    sEachLine = "((" + sEachLine + " Is NULL) OR (" + sEachTableFieldName + " <= ''))";
                                                                }
                                                                else
                                                                {
                                                                    sEachLine = "((" + sEachLine + " Is NULL) OR (" + sEachTableFieldName + " " + oViewFilters.Operator + " 0))";
                                                                }

                                                                break;
                                                            }
                                                        case "<":
                                                            {
                                                                if (bIsStringCol)
                                                                {
                                                                    ProcessFilterRet = Languages.get_Translation("msgViewModelFilterDataIsRequiredWhenUsingLessThanOp");
                                                                    sReturnStr = "";
                                                                    exitDo12 = exitDo2 = exitDo = true;
                                                                    break;
                                                                }
                                                                else
                                                                {
                                                                    sEachLine = "(" + sEachLine + " " + oViewFilters.Operator + " 0)";
                                                                }

                                                                break;
                                                            }

                                                        default:
                                                            {
                                                                break;
                                                            }
                                                    }

                                                    if (exitDo)
                                                    {
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    sFilterData = Strings.Replace(oViewFilters.FilterData, "'", "''");
                                                    bContainsWildcards = ConvertFilterData(ref sFilterData, ref sCharEscape, ref bConvertLiterals);

                                                    if (bContainsWildcards)
                                                    {
                                                        if (Strings.StrComp(oViewFilters.Operator, "=") == 0)
                                                        {
                                                            sEachLine = sEachLine + " LIKE ";
                                                        }
                                                        else if (Strings.StrComp(oViewFilters.Operator, "<>") == 0)
                                                        {
                                                            sEachLine = sEachLine + " Not LIKE ";
                                                        }
                                                        else
                                                        {
                                                            ProcessFilterRet = Languages.get_Translation("msgViewModelWildCardscanOnlyBeUsedWithOp");
                                                            sReturnStr = "";
                                                            exitDo13 = exitDo3 = true;
                                                            break;
                                                        }
                                                    }
                                                    else if (!bIsCheckBoxCol)
                                                    {
                                                        if (bConvertLiterals)
                                                        {
                                                            sEachLine = sEachLine + " LIKE ";
                                                        }
                                                        else
                                                        {
                                                            sEachLine = sEachLine + " " + oViewFilters.Operator + " ";
                                                        }
                                                    }

                                                    if (bIsStringCol)
                                                    {
                                                        if (bConvertLiterals)
                                                        {
                                                            sEachLine = sEachLine + "'" + sFilterData + "' ESCAPE '" + sCharEscape + "'";
                                                        }
                                                        else
                                                        {
                                                            sEachLine = sEachLine + "'" + sFilterData + "'";
                                                        }
                                                    }
                                                    else if (bIsCheckBoxCol)
                                                    {
                                                        if (oViewFilters.FilterData == "0")
                                                        {
                                                            sEachLine = "((" + sEachLine + " Is NULL) OR (" + sEachTableFieldName + " = 0))";
                                                        }
                                                        else
                                                        {
                                                            sEachLine = "(" + sEachLine + " <> 0)";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Added to handle wildcards in numeric fields RVW 08/28/2001 - Start
                                                        bValid = false;
                                                        sFilterData = oViewFilters.FilterData;

                                                        if (!Information.IsNumeric(Strings.Trim(sFilterData)))
                                                        {
                                                            var loopTo = Strings.Len(sFilterData);
                                                            for (lIndex = 1; lIndex <= loopTo; lIndex++)
                                                            {
                                                                if (Strings.InStr(1, "1234567890.", Strings.Mid(sFilterData, lIndex, 1), CompareMethod.Text) > 0)
                                                                {
                                                                    bValid = true;
                                                                }
                                                                else if (Strings.InStr(1, "*?", Strings.Mid(sFilterData, lIndex, 1), CompareMethod.Text) > 0)
                                                                {
                                                                    bContainsWildcards = true;
                                                                }
                                                                else
                                                                {
                                                                    bValid = false;
                                                                    bContainsWildcards = false;
                                                                    break;
                                                                }
                                                            }

                                                            if (bContainsWildcards)
                                                            {
                                                                sFilterData = Strings.Replace(sFilterData, "?", "_");
                                                                sFilterData = "'" + Strings.Replace(sFilterData, "*", "%") + "'";
                                                            }
                                                            // Added to handle wildcards in numeric fields RVW 08/28/2001 - End
                                                            else if (!bValid)
                                                            {
                                                                ProcessFilterRet = string.Format(Languages.get_Translation("msgViewModelIsNumericFieldMustCantainsNumbersOrWild"), sFieldName);
                                                                sReturnStr = "";
                                                                exitDo14 = exitDo4 = true;
                                                                break;
                                                            }
                                                        }

                                                        sEachLine = Strings.Trim(sEachLine) + " " + sFilterData + "";
                                                    }
                                                }

                                                break;
                                            }
                                    }

                                    if (exitDo1)
                                    {
                                        break;
                                    }

                                    if (exitDo2)
                                    {
                                        break;
                                    }

                                    if (exitDo3)
                                    {
                                        break;
                                    }

                                    if (exitDo4)
                                    {
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                sEachLine = "";
                            }

                            if (Strings.Len(sEachLine) > 0)
                            {
                                sReturnStr = sReturnStr + sEachLine;
                                iParenCount = ComputeParenCount(ref sReturnStr);

                                if (iParenCount < 0)
                                {
                                    ProcessFilterRet = Languages.get_Translation("msgViewModelTooManyRightParens");
                                    sReturnStr = "";
                                    exitDo15 = true;
                                    break;
                                }

                                sReturnStr = sReturnStr + oViewFilters.CloseParen;

                                if ((bool)oViewFilters.Active)
                                {
                                    if (!string.IsNullOrEmpty(oViewFilters.JoinOperator))
                                    {
                                        sLastConnector = oViewFilters.JoinOperator;
                                        iLastConnectorPos = Strings.Len(sReturnStr);
                                        sReturnStr = sReturnStr + " " + Strings.UCase(oViewFilters.JoinOperator) + " ";
                                    }
                                }
                            }
                            else if (Strings.Right(sReturnStr, 4) == "AND ")
                            {
                                sReturnStr = Strings.Left(sReturnStr, Strings.Len(sReturnStr) - 4);
                                sReturnStr = Strings.Trim(sReturnStr) + oViewFilters.CloseParen + " AND ";
                            }
                            else if (Strings.Right(sReturnStr, 3) == "OR ")
                            {
                                sReturnStr = Strings.Left(sReturnStr, Strings.Len(sReturnStr) - 3);
                                sReturnStr = Strings.Trim(sReturnStr) + oViewFilters.CloseParen + " OR ";
                            }
                            else
                            {
                                sReturnStr = Strings.Trim(sReturnStr) + oViewFilters.CloseParen;
                            }
                        }

                        if (exitDo5)
                        {
                            break;
                        }

                        if (exitDo6)
                        {
                            break;
                        }

                        if (exitDo7)
                        {
                            break;
                        }

                        if (exitDo8)
                        {
                            break;
                        }

                        if (exitDo9)
                        {
                            break;
                        }

                        if (exitDo10)
                        {
                            break;
                        }

                        if (exitDo11)
                        {
                            break;
                        }

                        if (exitDo12)
                        {
                            break;
                        }

                        if (exitDo13)
                        {
                            break;
                        }

                        if (exitDo14)
                        {
                            break;
                        }

                        if (exitDo15)
                        {
                            break;
                            // End If
                        }
                        // Remove extra connector at the end
                        if (bDidOne & !string.IsNullOrEmpty(sLastConnector))
                        {
                            sReturnStr = Strings.Left(sReturnStr, iLastConnectorPos) + Strings.Mid(sReturnStr, iLastConnectorPos + Strings.Len(sLastConnector) + 3);
                        }
                        iParenCount = ComputeParenCount(ref sReturnStr);

                        if (iParenCount < 0)
                        {
                            ProcessFilterRet = Languages.get_Translation("msgViewModelTooManyRightParens");
                            sReturnStr = "";
                            break;
                        }
                        if (iParenCount > 0)
                        {
                            ProcessFilterRet = Languages.get_Translation("msgViewModelTooManyLeftParens");
                            sReturnStr = "";
                            break;
                        }

                        break;
                    }
                    while (true);
                }
                else
                {
                    ProcessFilterRet = "No View";
                    sReturnStr = "";
                }
            }

            if (sReturnStr is not null && sReturnStr.Length > 0)
            {
                if (Strings.Left(sReturnStr, 1) != "(" | Strings.Right(sReturnStr, 1) != ")")
                {
                    sReturnStr = "(" + sReturnStr + ")";
                }

                iLastConnectorPos = Strings.InStr(sReturnStr, "()");

                while (iLastConnectorPos > 0)
                {
                    sReturnStr = Strings.Replace(sReturnStr, "()", "");
                    iLastConnectorPos = Strings.InStr(sReturnStr, "()");
                }
            }

            return ProcessFilterRet;
        }

        public static void SQLViewDelete(int Id, Passport passport)
        {
            // Dim sql As String = "IF OBJECT_ID('view__@ViewID1', 'V') IS NOT NULL DROP VIEW [view__@ViewID] ;"
            string sql = string.Format("IF OBJECT_ID('view__{0}', 'V') IS NOT NULL DROP VIEW [view__{0}]", Id.ToString());
            try
            {
                using (var cmd = new SqlCommand(sql, passport.Connection()))
                {
                    // cmd.Parameters.AddWithValue("@ViewID", Id.ToString)
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
               slimShared.SlimShared.logWarningAsType(ex.Message, EventLogEntryType.Error);
            }
        }
    }

    public partial class TestingList
    {
        public List<Testing> NameList { get; set; }
    }

    public partial class Testing
    {
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }
        private string m_Name;
    }
}