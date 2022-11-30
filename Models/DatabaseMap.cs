using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using TabFusionRMS.DataBaseManagerVB;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;

namespace TabFusionRMS.WebCS
{

    public sealed class DatabaseMap
    {
        public static JSTreeView.TreeView GetBindTreeControl(IQueryable<TabFusionRMS.Models.System> lSystemEntities, IQueryable<Table> lTableEntities, IQueryable<TableTab> lTabletabEntities, IQueryable<TabSet> lTabsetsEntities, IQueryable<RelationShip> lRelationShipEntities)
        {

            var treeView = new JSTreeView.TreeView();

            var treeNode = new JSTreeView.TreeNode();
            var rootNode = new JSTreeView.ListItem("Root node 1", "node1");

            var dataJsTree = new JSTreeView.DataJsTree();
            // dataJsTree.Icon = "glyphicon glyphicon-folder-open"
            // dataJsTree.Icon = "/Images/icons/EMPLOYEE.ICO"
            dataJsTree.Opened = true;

            if (lSystemEntities.Count() > 0)
            {
                foreach (TabFusionRMS.Models.System pSystemEntity in lSystemEntities)
                {
                    var DatabaseNode = new JSTreeView.ListItem(pSystemEntity.UserName, Guid.NewGuid().ToString() + "_root_" + pSystemEntity.Id.ToString() + "_0_1", className: "jstree-open", dataJsTree: dataJsTree);

                    int iTabsetsCount = lTabsetsEntities.Count();
                    int iTabsetsLoop = 0;
                    if (lTabsetsEntities.Count() > 0)
                    {
                        foreach (TabSet pTabsetsEntity in lTabsetsEntities.OrderBy(x => x.Id))
                        {
                            dataJsTree = new JSTreeView.DataJsTree();
                            if (!string.IsNullOrEmpty(pTabsetsEntity.Picture))
                            {
                                dataJsTree.Icon = "/Images/icons/" + pTabsetsEntity.Picture;
                            }
                            else
                            {
                                // dataJsTree.Icon = "glyphicon glyphicon-folder-open"
                                dataJsTree.Icon = "/Images/icons/FOLDERS.ICO";
                            }
                            dataJsTree.Opened = true;

                            var TabsetNode = new JSTreeView.ListItem(pTabsetsEntity.UserName, Guid.NewGuid().ToString() + "_Tabsets_" + pTabsetsEntity.Id.ToString() + "_" + iTabsetsLoop.ToString() + "_" + iTabsetsCount.ToString(), className: "jstree-open", dataJsTree: dataJsTree);
                            iTabsetsLoop = iTabsetsLoop + 1;
                            // DatabaseNode.Nodes.Add(New JSTreeView.ListItem(pTabsetsEntity.UserName, "level1_" + pTabsetsEntity.Id.ToString(), className:=""))
                            DatabaseNode.Nodes.Add(TabsetNode);

                            var lTabletabSelected = lTabletabEntities.Where(x => x.TabSet == pTabsetsEntity.Id).OrderBy(x => x.TabOrder);
                            int iTabletabCount = lTabletabSelected.Count();
                            int iTabletabLoop = 0;
                            if (lTabletabSelected.Count() > 0)
                            {
                                foreach (TableTab pTabletabEntity in lTabletabSelected)
                                {
                                    var pTableTabTableEntity = lTableEntities.Where(x => x.TableName.Trim().ToLower().Equals(pTabletabEntity.TableName.Trim().ToLower())).FirstOrDefault();
                                    // Dim pUserName = lTableEntities.Where(Function(x) x.TableName.Trim().ToLower().Equals(pTabletabEntity.TableName.Trim().ToLower())).FirstOrDefault().UserName
                                    if (pTableTabTableEntity is not null)
                                    {
                                        if (!string.IsNullOrEmpty(pTableTabTableEntity.UserName))
                                        {
                                            dataJsTree = new JSTreeView.DataJsTree();
                                            if (!string.IsNullOrEmpty(pTableTabTableEntity.Picture))
                                            {
                                                dataJsTree.Icon = "/Images/icons/" + pTableTabTableEntity.Picture;
                                            }
                                            else
                                            {
                                                // dataJsTree.Icon = "glyphicon glyphicon-folder-open"
                                                dataJsTree.Icon = "/Images/icons/FOLDERS.ICO";
                                            }
                                            dataJsTree.Opened = true;

                                            var TableTabNode = new JSTreeView.ListItem(pTableTabTableEntity.UserName, Guid.NewGuid().ToString() + "_Tabletabs_" + pTableTabTableEntity.TableId.ToString() + "_" + iTabletabLoop.ToString() + "_" + iTabletabCount.ToString(), className: "jstree-open", dataJsTree: dataJsTree);
                                            iTabletabLoop = iTabletabLoop + 1;
                                            TabsetNode.Nodes.Add(TableTabNode);

                                            var lRelationShipSelected = lRelationShipEntities.Where(x => x.UpperTableName.Trim().ToLower().Equals(pTabletabEntity.TableName.Trim().ToLower())).OrderBy(x => x.TabOrder);
                                            int iRelationShipCount = lRelationShipSelected.Count();
                                            int iRelationShipLoop = 0;
                                            foreach (RelationShip pRelationShipEntity in lRelationShipSelected)
                                            {
                                                var pRelTableEntity = lTableEntities.Where(x => x.TableName.Trim().ToLower().Equals(pRelationShipEntity.LowerTableName.Trim().ToLower())).FirstOrDefault();
                                                // pUserName = lTableEntities.Where(Function(x) x.TableName.Trim().ToLower().Equals(pRelationShipEntity.LowerTableName.Trim().ToLower())).FirstOrDefault().UserName
                                                if (pRelTableEntity is not null)
                                                {
                                                    if (!string.IsNullOrEmpty(pRelTableEntity.UserName))
                                                    {

                                                        dataJsTree = new JSTreeView.DataJsTree();
                                                        if (!string.IsNullOrEmpty(pRelTableEntity.Picture))
                                                        {
                                                            dataJsTree.Icon = "/Images/icons/" + pRelTableEntity.Picture;
                                                        }
                                                        else
                                                        {
                                                            // dataJsTree.Icon = "glyphicon glyphicon-folder-open"
                                                            dataJsTree.Icon = "/Images/icons/FOLDERS.ICO";
                                                        }
                                                        dataJsTree.Opened = true;

                                                        var RelationShipNode = new JSTreeView.ListItem(pRelTableEntity.UserName, Guid.NewGuid().ToString() + "_RelShips_" + pRelTableEntity.TableId.ToString() + "_" + iRelationShipLoop.ToString() + "_" + iRelationShipCount.ToString(), className: "jstree-open", dataJsTree: dataJsTree);
                                                        iRelationShipLoop = iRelationShipLoop + 1;
                                                        TableTabNode.Nodes.Add(RelationShipNode);

                                                        var pChildCheck = lRelationShipEntities.Where(x => x.UpperTableName.Trim().ToLower().Equals(pRelationShipEntity.LowerTableName.Trim().ToLower()));
                                                        if (pChildCheck.Count() > 0)
                                                        {
                                                            SetChildRecords(RelationShipNode, pRelationShipEntity.LowerTableName, lRelationShipEntities, lTableEntities);
                                                        }

                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                    rootNode.Nodes.Add(DatabaseNode);
                }
            }

            treeNode.ListItems.Add(rootNode);
            treeView.Nodes.Add(treeNode);
            return treeView;
        }

        public static void SetChildRecords(JSTreeView.ListItem pListItem, string pTableName, IQueryable<RelationShip> lRelationEntities, IQueryable<Table> lTableEntities)
        {

            var dataJsTree = new JSTreeView.DataJsTree();
            // dataJsTree.Icon = "glyphicon glyphicon-folder-open"
            // dataJsTree.Opened = True
            var lRelationShipEntities = lRelationEntities.Where(x => x.UpperTableName.Trim().ToLower().Equals(pTableName.Trim().ToLower())).OrderBy(x => x.TabOrder);
            int iRelationShipCount = lRelationShipEntities.Count();
            int iRelationShipLoop = 0;
            if (lRelationShipEntities.Count() > 0)
            {
                foreach (RelationShip pRelationShipEntity in lRelationShipEntities)
                {
                    var pRelTableEntity = lTableEntities.Where(x => x.TableName.Trim().ToLower().Equals(pRelationShipEntity.LowerTableName.Trim().ToLower())).FirstOrDefault();
                    // Dim pUserName = lTableEntities.Where(Function(x) x.TableName.Trim().ToLower().Equals(pRelationShipEntity.LowerTableName.Trim().ToLower())).FirstOrDefault().UserName
                    if (pRelTableEntity is not null)
                    {
                        dataJsTree = new JSTreeView.DataJsTree();
                        if (!string.IsNullOrEmpty(pRelTableEntity.Picture))
                        {
                            dataJsTree.Icon = "/Images/icons/" + pRelTableEntity.Picture;
                        }
                        else
                        {
                            // dataJsTree.Icon = "glyphicon glyphicon-folder-open"
                            dataJsTree.Icon = "/Images/icons/FOLDERS.ICO";
                        }
                        dataJsTree.Opened = true;

                        var RelationShipChildNode = new JSTreeView.ListItem(pRelTableEntity.UserName, Guid.NewGuid().ToString() + "_RelShips_" + pRelTableEntity.TableId.ToString() + "_" + iRelationShipLoop.ToString() + "_" + iRelationShipCount.ToString(), className: "jstree-open", dataJsTree: dataJsTree);
                        iRelationShipLoop = iRelationShipLoop + 1;
                        pListItem.Nodes.Add(RelationShipChildNode);

                        var pChildCheck = lRelationEntities.Where(x => x.UpperTableName.Trim().ToLower().Equals(pRelationShipEntity.LowerTableName.Trim().ToLower()));

                        if (pChildCheck.Count() > 0)
                        {
                            SetChildRecords(RelationShipChildNode, pRelationShipEntity.LowerTableName, lRelationEntities, lTableEntities);
                        }
                    }
                }

            }

        }

        public static string RemoveTableNameFromField(string sFieldName)
        {
            string RemoveTableNameFromFieldRet = default;
            int i;
            RemoveTableNameFromFieldRet = sFieldName;
            i = Strings.InStr(sFieldName, ".");
            if (i > 1)
            {
                RemoveTableNameFromFieldRet = Strings.Mid(sFieldName, i + 1);
            }
            RemoveTableNameFromFieldRet = Strings.Trim(RemoveTableNameFromFieldRet);
            return RemoveTableNameFromFieldRet;

        }

        public static string RemoveTableNameFromFieldIfNotCurrentTable(string sFieldName, string currentTableName)
        {
            string RemoveTableNameFromFieldIfNotCurrentTableRet = default;
            string sFieldTableName;

            RemoveTableNameFromFieldIfNotCurrentTableRet = sFieldName;
            sFieldTableName = sFieldName;
            if (Strings.InStr(sFieldTableName, ".") > 1)
            {
                sFieldTableName = Strings.Left(sFieldTableName, Strings.InStr(sFieldTableName, ".") - 1);
                if (Conversions.ToBoolean(Strings.StrComp(sFieldTableName, currentTableName, Constants.vbTextCompare)))
                {
                    RemoveTableNameFromFieldIfNotCurrentTableRet = Strings.Mid(sFieldName, Strings.InStr(sFieldName, ".") + 1);
                }
            }

            return RemoveTableNameFromFieldIfNotCurrentTableRet;
        }

        public static string RemoveFieldNameFromField(string sFieldName)
        {
            string RemoveFieldNameFromFieldRet = default;
            int i;
            RemoveFieldNameFromFieldRet = sFieldName;
            i = Strings.InStr(sFieldName, ".");
            if (i > 1)
            {
                RemoveFieldNameFromFieldRet = Strings.Trim(Strings.Left(sFieldName, i - 1));
            }
            return RemoveFieldNameFromFieldRet;
        }

        public static string GetConnectionString(Databas DBToOpen, bool includeProvider)
        {
            string sConnect = string.Empty;

            if (includeProvider)
            {
                sConnect += "Provider=" + DBToOpen.DBProvider + ";";
            }

            sConnect += "Data Source=" + DBToOpen.DBServer + "; Initial Catalog=" + DBToOpen.DBDatabase + "; "; // Persist Security Info=true; "

            if (!string.IsNullOrEmpty(Strings.Trim(DBToOpen.DBUserId)))
            {
                sConnect += "User Id=" + DBToOpen.DBUserId + "; Password=" + DBToOpen.DBPassword + ";";
            }
            else
            {
                sConnect += "Integrated Security=SSPI;";
            }

            return sConnect;
        }


        private static int miUserLinkIndexTableIdSize = 0;

        public static int UserLinkIndexTableIdSize
        {
            get
            {
                if (miUserLinkIndexTableIdSize == 0)
                {
                    // Get UserLinks.IndexTableId field length...
                    var sADOConn = DataServices.DBOpen(Enums.eConnectionType.conDefault);
                    var pSchemaInfo = SchemaInfoDetails.GetSchemaInfo("USERLINKS", sADOConn, "INDEXTABLEID");
                    if (pSchemaInfo.Count > 0)
                    {
                        try
                        {
                            miUserLinkIndexTableIdSize = pSchemaInfo[0].CharacterMaxLength;
                        }
                        catch (Exception)
                        {
                            miUserLinkIndexTableIdSize = 30;
                        }
                    }
                    else
                    {
                        miUserLinkIndexTableIdSize = 30;
                    }

                    pSchemaInfo = null;
                }

                return miUserLinkIndexTableIdSize;
            }
        }

        // Create new table in database
        public static string CreateNewTables(string pTableName, string pIdFieldName, Enums.meFieldTypes pFieldType, int pFieldSize, Databas pDatabaseEntity, Table pParentTableEntity, Repositories<Databas> _iDatabas)
        {
            bool pOutPut = false;
            string pOutPutStr = string.Empty;
            bool bHaveParent;
            string sSQLStr;
            var rsADO = new ADODB.Recordset();
            string sErrMsg = string.Empty;
            List<SchemaColumns> oSchemaColumns;
            var sADOConn = new ADODB.Connection();
            var pADOConn = new ADODB.Connection();

            // Dim sConnString As String = Keys.DefaultConnectionString(True)
            if (pDatabaseEntity is not null)
            {
                sADOConn = DataServices.DBOpen(pDatabaseEntity);
            }
            else
            {
                sADOConn = DataServices.DBOpen(Enums.eConnectionType.conDefault, null);
            }

            bHaveParent = pParentTableEntity is not null;

            try
            {
                // create the new field:
                sSQLStr = "CREATE TABLE [" + pTableName + "] ";
                sSQLStr = sSQLStr + "([" + pIdFieldName + "] ";

                switch (pFieldType)
                {
                    case Enums.meFieldTypes.ftLong:
                    case Enums.meFieldTypes.ftSmeadCounter:
                        {
                            sSQLStr = sSQLStr + "INT NOT NULL";
                            sSQLStr = sSQLStr + " CONSTRAINT [" + pTableName + "_PrimaryKey] PRIMARY KEY ([" + pIdFieldName + "])";
                            break;
                        }
                    case Enums.meFieldTypes.ftCounter:
                        {
                            sSQLStr = sSQLStr + "INT IDENTITY(1,1) NOT NULL";
                            sSQLStr = sSQLStr + " CONSTRAINT [" + pTableName + "_PrimaryKey] PRIMARY KEY ([" + pIdFieldName + "])";
                            break;
                        }
                    case Enums.meFieldTypes.ftText:
                        {
                            sSQLStr = sSQLStr + "VARCHAR(" + pFieldSize + ") NOT NULL";
                            sSQLStr = sSQLStr + " CONSTRAINT [" + pTableName + "_PrimaryKey] PRIMARY KEY ([" + pIdFieldName + "])";
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }

                if (bHaveParent)
                {
                    sSQLStr = sSQLStr + ", [" + pParentTableEntity.TableName + RemoveTableNameFromField(pParentTableEntity.IdFieldName) + "]";
                    pADOConn = DataServices.DBOpen(_iDatabas.All().Where(x => x.DBName.Trim().ToLower().Equals(pParentTableEntity.DBName.Trim().ToLower())).FirstOrDefault());
                    oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pParentTableEntity.TableName, pADOConn, RemoveTableNameFromField(pParentTableEntity.IdFieldName));
                    if (oSchemaColumns[0].IsString)
                    {
                        sSQLStr = sSQLStr + " VARCHAR(" + oSchemaColumns[0].CharacterMaxLength + ")";
                    }
                    else if (oSchemaColumns[0].IsADate)
                    {
                        sSQLStr = sSQLStr + " DATETIME";
                    }
                    else
                    {
                        switch (oSchemaColumns[0].DataType)
                        {
                            case Enums.DataTypeEnum.rmInteger:
                                {
                                    if (oSchemaColumns[0].CharacterMaxLength == 2)
                                    {
                                        sSQLStr = sSQLStr + " SHORT INT";
                                    }
                                    else
                                    {
                                        sSQLStr = sSQLStr + " INT";
                                    }

                                    break;
                                }

                            default:
                                {
                                    Interaction.MsgBox("Unsupported Type IdFieldNameType");
                                    break;
                                }
                        }
                    }

                    oSchemaColumns = null;
                }

                sSQLStr = sSQLStr + ");";
                pOutPut = DataServices.ProcessADOCommand(ref sSQLStr, sADOConn, false);

                if (pOutPut)
                {
                    string argsSql = "SELECT TOP 1 * FROM " + pTableName + "";
                    rsADO = DataServices.GetADORecordSet(ref argsSql, sADOConn, ref sErrMsg);
                    if (sErrMsg.ToLower().Contains("incorrect syntax near the keyword"))
                    {
                        throw new Exception(sErrMsg);
                    }
                    if (pFieldType == Enums.meFieldTypes.ftSmeadCounter)
                    {
                        sSQLStr = "ALTER TABLE System";
                        sSQLStr = sSQLStr + " ADD [" + pTableName + "Counter] INT NULL";

                        pOutPut = DataServices.ProcessADOCommand(ref sSQLStr, sADOConn, false);

                        List<SchemaColumns> oSchemaColumn;
                        oSchemaColumn = SchemaInfoDetails.GetSchemaInfo("System", sADOConn, pTableName + "Counter");

                        if (oSchemaColumn.Count > 0)
                        {
                            IncrementCounter(pTableName + "Counter", sADOConn);
                            pOutPut = true;
                        }
                    }
                    else
                    {
                        pOutPut = true;
                    }
                }
                if (pOutPut == false)
                    pOutPutStr = "False";
                return pOutPutStr;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Enter data in "Tables" database table
        public static bool SetTablesEntity(string sTableName, string sUserName, string sIdFieldName, string sDatabaseName, Enums.meFieldTypes eFieldType, IRepository<Table> _iTable, ref int iTableId)
        {

            var pMaxSearchOrder = _iTable.All().Max(x => x.SearchOrder);

            var pTables = new Table();

            pTables.TableName = sTableName;
            pTables.UserName = sUserName;

            pTables.AddGroup = 0;
            pTables.DelGroup = 0;
            pTables.EditGroup = 0;
            pTables.MgrGroup = 0;
            pTables.ViewGroup = 0;
            pTables.PCFilesEditGrp = 0;
            pTables.PCFilesNVerGrp = 0;
            pTables.Attachments = false;
            pTables.AttributesID = 1;

            if (Strings.StrComp(sDatabaseName, "db_engine", Constants.vbTextCompare) != 0)
            {
                pTables.DBName = sDatabaseName;
            }

            pTables.IdFieldName = sTableName + "." + sIdFieldName;
            pTables.IdFieldName2 = null;
            pTables.TrackingTable = (short?)0;
            pTables.Trackable = false;

            pTables.Picture = null;
            pTables.BarCodePrefix = null;

            if (eFieldType == Enums.meFieldTypes.ftSmeadCounter)
            {
                pTables.CounterFieldName = sTableName + "Counter";
            }
            else
            {
                pTables.CounterFieldName = null;
            }

            pTables.DefaultDescriptionField = null;
            pTables.DefaultDescriptionText = null;
            pTables.DefaultRetentionId = null;
            pTables.DescFieldNameOne = null;
            pTables.DescFieldNameTwo = null;
            pTables.DescFieldPrefixOne = null;
            pTables.DescFieldPrefixOneTable = null;
            pTables.DescFieldPrefixOneWidth = (short?)0;
            pTables.DescFieldPrefixTwo = null;
            pTables.DescFieldPrefixTwoTable = null;
            pTables.DescFieldPrefixTwoWidth = (short?)0;
            pTables.DescRelateTable1 = null;
            pTables.DescRelateTable2 = null;
            pTables.IdMask = null;
            pTables.IdStripChars = null;
            pTables.MaxRecordsAllowed = (short?)0;
            pTables.OutTable = (short?)0;
            pTables.RestrictAddToTable = (short?)0;
            pTables.RuleDateField = null;
            pTables.TrackingACTIVEFieldName = null;
            pTables.TrackingOUTFieldName = null;
            pTables.TrackingStatusFieldName = null;

            pTables.ADOServerCursor = false;
            pTables.ADOQueryTimeout = 30;
            pTables.ADOCacheSize = 1;
            pTables.DeleteAttachedGroup = 9999;
            pTables.SearchOrder = pMaxSearchOrder + 1;
            pTables.Trackable = false;
            pTables.AllowBatchRequesting = false;

            _iTable.Add(pTables);
            iTableId = pTables.TableId;
            return true;

        }

        // Enter data in "Views" database table
        public static bool SetViewsEntity(string sTableName, string sViewName, IRepository<View> _iView, ref int iViewId)
        {

            var pView = new View();

            pView.Printable = false;
            pView.AltViewId = 0;
            pView.DeleteGridAvail = false;
            pView.FiltersActive = false;
            pView.Picture = "";
            pView.ReportStylesId = "All Requests Report";
            pView.RowHeight = (short?)0;
            pView.SQLStatement = "SELECT * FROM [" + sTableName + "]";
            pView.SearchableView = true;
            pView.TableName = sTableName;
            pView.TablesDown = "";
            pView.TablesId = 0;
            pView.UseExactRowCount = false;
            pView.VariableColWidth = true;
            pView.VariableFixedCols = false;
            pView.VariableRowHeight = true;
            pView.ViewGroup = 0;
            pView.ViewName = sViewName;
            pView.ViewOrder = 1;
            pView.ViewType = (short?)0;
            pView.Visible = true;
            pView.WorkFlow1 = "";
            pView.WorkFlow1Pic = "";
            pView.WorkFlowDesc1 = "";
            pView.WorkFlow2 = "";
            pView.WorkFlow2Pic = "";
            pView.WorkFlowDesc2 = "";
            pView.WorkFlow3 = "";
            pView.WorkFlow3Pic = "";
            pView.WorkFlowDesc3 = "";
            pView.WorkFlow4 = "";
            pView.WorkFlow4Pic = "";
            pView.WorkFlowDesc4 = "";
            pView.WorkFlow5 = "";
            pView.WorkFlow5Pic = "";
            pView.WorkFlowDesc5 = "";
            // 25 is the new default as it is only used for Web Access 
            // and 5000 for desktop.  RVW 03/06/2019
            pView.MaxRecsPerFetch = pView.MaxRecsPerFetch;
            pView.MaxRecsPerFetchDesktop = pView.MaxRecsPerFetchDesktop;

            _iView.Add(pView);

            iViewId = pView.Id;
            return true;

        }

        // Enter data in "ViewColumn" database table
        public static bool SetViewColumnEntity(int iViewId, string sTableName, string sIdFieldName, int sIdFieldType, IRepository<ViewColumn> _iViewColumn, Table pParentTable)
        {

            var pViewColumn = new ViewColumn();
            bool bEditAllowed;
            if (sIdFieldType == (int)Enums.meFieldTypes.ftSmeadCounter || sIdFieldType == (int)Enums.meFieldTypes.ftCounter)
            {
                bEditAllowed = false;
            }
            else
            {
                bEditAllowed = true;
            }

            pViewColumn = DatabaseMap.CreateViewColumnEntity(iViewId, bEditAllowed, Enums.geViewColumnDisplayType.cvAlways, sTableName, sIdFieldName, Enums.geViewColumnsLookupType.ltDirect, 0, 1, 1);
            _iViewColumn.Add(pViewColumn);
            pViewColumn = null;
            if (pParentTable is not null)
            {
                pViewColumn = new ViewColumn();

                pViewColumn = DatabaseMap.CreateViewColumnEntity(iViewId, bEditAllowed, Enums.geViewColumnDisplayType.cvSmartColumns, sTableName, pParentTable.TableName + RemoveTableNameFromField(pParentTable.IdFieldName), Enums.geViewColumnsLookupType.ltDirect, 1, 0, 0);
                _iViewColumn.Add(pViewColumn);
            }

            return true;

        }

        // Mehtod for create ViewColumn Entity attributes
        private static ViewColumn CreateViewColumnEntity(int iViewId, bool bEditAllowed, Enums.geViewColumnDisplayType eColumnOrder, string sTableName, string sIdFieldName, Enums.geViewColumnsLookupType eLookupType, int iColumnNum, int iFreezeOrder, int iSortOrder)
        {
            var pViewColumn = new ViewColumn();

            pViewColumn.ViewsId = iViewId;
            pViewColumn.ColumnNum = (short?)iColumnNum;
            pViewColumn.ColumnVisible = false; // True is capslock
            pViewColumn.ColumnWidth = (short?)3000;
            pViewColumn.ColumnOrder = Conversions.ToShort(eColumnOrder);  // Visible Flag


            pViewColumn.EditAllowed = bEditAllowed;

            pViewColumn.FieldName = sTableName + "." + sIdFieldName;
            pViewColumn.Heading = sIdFieldName;
            pViewColumn.FilterField = true;
            pViewColumn.FreezeOrder = iFreezeOrder;
            pViewColumn.SortOrder = iSortOrder;
            pViewColumn.LookupType = Conversions.ToShort(eLookupType);
            pViewColumn.SortableField = true;

            pViewColumn.AlternateFieldName = "";
            pViewColumn.ColumnStyle = (short?)0;
            pViewColumn.DefaultLookupValue = "";
            pViewColumn.DropDownFilterIdField = "";
            pViewColumn.DropDownFilterMatchField = "";
            pViewColumn.DropDownFlag = (short?)0;
            pViewColumn.DropDownReferenceColNum = (short?)0;
            pViewColumn.DropDownReferenceValue = "";
            pViewColumn.DropDownTargetField = "";
            pViewColumn.EditMask = "";
            pViewColumn.FormColWidth = 0;
            pViewColumn.InputMask = "";
            pViewColumn.LookupIdCol = (short?)0;
            pViewColumn.MaskClipMode = false;
            pViewColumn.MaskInclude = false; // True '3.0.214 - MEF 08/11/2000 - Fix to wrong default
            pViewColumn.MaskPromptChar = "_";
            pViewColumn.MaxPrintLines = 1;
            pViewColumn.PageBreakField = false;
            pViewColumn.Picture = "";
            pViewColumn.PrinterColWidth = 0;
            pViewColumn.SortOrderDesc = false;
            pViewColumn.SuppressDuplicates = false;
            pViewColumn.SuppressPrinting = false;
            pViewColumn.VisibleOnForm = true;
            pViewColumn.VisibleOnPrint = true;
            pViewColumn.CountColumn = false;
            pViewColumn.SubtotalColumn = false;
            pViewColumn.PrintColumnAsSubheader = false;
            pViewColumn.RestartPageNumber = false;
            pViewColumn.LabelJustify = (int?)1L;
            pViewColumn.LabelLeft = (int?)-1L;
            pViewColumn.LabelTop = (int?)-1L;
            pViewColumn.LabelWidth = (int?)-1L;
            pViewColumn.LabelHeight = (int?)-1L;
            pViewColumn.ControlLeft = (int?)-1L;
            pViewColumn.ControlTop = (int?)-1L;
            pViewColumn.ControlWidth = (int?)-1L;
            pViewColumn.ControlHeight = (int?)-1L;
            pViewColumn.TabOrder = (int?)-1L;

            return pViewColumn;
        }

        // Enter data in "Relationships" database table
        public static bool SetRelationshipsEntity(string sTableName, Table pParentTableEntity, IRepository<RelationShip> _iRelationShip, ref int iRelId, string sExtraStr = "", string sIdFieldName = "")
        {

            var pRelationShip = new RelationShip();
            int iHiTab;

            if (pParentTableEntity is not null)
            {
                iHiTab = 0;

                var lRelationShipEntities = _iRelationShip.All().Where(x => x.UpperTableName.Trim().ToLower().Equals(pParentTableEntity.TableName.Trim().ToLower()));

                // For Each oRelationships In lRelationShipEntities
                // If (oRelationships.TabOrder > iHiTab) Then
                // iHiTab = oRelationships.TabOrder
                // End If
                // Next
                // iHiTab = iHiTab + 1
                if (lRelationShipEntities.Count() > 0)
                {
                    iHiTab = (int)lRelationShipEntities.Max(x => x.TabOrder);
                }

                iHiTab = iHiTab + 1;
                // iHiTab = lRelationShipEntities.Count() + 1
                pRelationShip.DrillDownViewGroup = 0;
                pRelationShip.IdTypes = (short?)0;
                if (!string.IsNullOrEmpty(sIdFieldName))
                {
                    pRelationShip.LowerTableFieldName = sTableName + "." + sIdFieldName;
                }
                else
                {
                    pRelationShip.LowerTableFieldName = sTableName + "." + pParentTableEntity.TableName + RemoveTableNameFromField(pParentTableEntity.IdFieldName) + sExtraStr;
                }

                pRelationShip.LowerTableName = sTableName;
                pRelationShip.TabOrder = (short?)iHiTab;
                pRelationShip.UpperTableFieldName = pParentTableEntity.TableName + "." + RemoveTableNameFromField(pParentTableEntity.IdFieldName);
                pRelationShip.UpperTableName = pParentTableEntity.TableName;

                _iRelationShip.Add(pRelationShip);
                iRelId = pRelationShip.Id;
                pRelationShip = null;
            }

            return true;

        }

        // Enter data in "TabSet" database table
        public static bool SetTabSetEntity(int iTabSetId, int lViewId, string sTableName, IRepository<TableTab> _iTabletab, ref int iTableTabId)
        {
            var pTabletab = new TableTab();
            long lHighestOrder;
            var lTabletabEntities = _iTabletab.All();

            var pTabletabEntity = lTabletabEntities.Where(x => x.TabSet == iTabSetId && x.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())).FirstOrDefault();

            var flag = true;

            if (pTabletabEntity != null)
            {
                if(pTabletabEntity.Id > 0)
                {
                    if (pTabletabEntity.TopTab == true)
                    {
                        pTabletabEntity.TopTab = true;
                        _iTabletab.Add(pTabletabEntity);
                        iTableTabId = pTabletabEntity.Id;
                    }
                    pTabletab = null;
                    flag = false;
                }
            }

            // flag true means pTabletabEntity is null or not grater then 0
            if (flag)
            {
                lHighestOrder = lTabletabEntities.Where(x => x.TabSet == iTabSetId).Count();
                lHighestOrder = lHighestOrder + 1L;
                pTabletab.BaseView = lViewId;
                pTabletab.TableName = sTableName;
                pTabletab.TabOrder = (short?)lHighestOrder;
                pTabletab.TabSet = (short?)iTabSetId;
                pTabletab.TopTab = true;
                _iTabletab.Add(pTabletab);
                iTableTabId = pTabletab.Id;
                pTabletab = null;
            }

            return true;
        }

        // Enter new field into table
        public static bool CreateNewField(string sFieldName, Enums.DataTypeEnum eFieldType, long lSize, string sTableName, ADODB.Connection sADOConn)
        {
            string sSQLStr;

            // create the new field:
            sSQLStr = "ALTER TABLE [" + sTableName + "]";
            sSQLStr = sSQLStr + " ADD [" + sFieldName + "] ";

            if (DataServices.IsAStringType(eFieldType))
            {
                sSQLStr = sSQLStr + " VARCHAR(" + lSize + ") NULL";
            }
            else if (DataServices.IsADateType(eFieldType))
            {
                sSQLStr = sSQLStr + " SMALLDATETIME NULL";
            }
            else
            {
                switch (eFieldType)
                {
                    // 3.0.214 - MEF 08/11/2000 - Fixed all types to have a SQL and access type
                    case Enums.DataTypeEnum.rmTinyInt:
                    case Enums.DataTypeEnum.rmUnsignedTinyInt:
                        {
                            sSQLStr = sSQLStr + " TINYINT NULL DEFAULT 0";
                            break;
                        }
                    case Enums.DataTypeEnum.rmSmallInt:
                    case Enums.DataTypeEnum.rmUnsignedSmallInt:
                        {
                            sSQLStr = sSQLStr + " SMALLINT NULL";
                            break;
                        }
                    case Enums.DataTypeEnum.rmInteger:
                    case Enums.DataTypeEnum.rmBigInt:
                    case Enums.DataTypeEnum.rmUnsignedInt:
                    case Enums.DataTypeEnum.rmUnsignedBigInt:
                        {
                            sSQLStr = sSQLStr + " INT NULL";
                            break;
                        }
                    case Enums.DataTypeEnum.rmSingle:
                        {
                            sSQLStr = sSQLStr + " REAL NULL";
                            break;
                        }
                    case Enums.DataTypeEnum.rmDouble:
                        {
                            sSQLStr = sSQLStr + " FLOAT NULL";
                            break;
                        }

                    default:
                        {
                            Interaction.MsgBox("Unsupported Type IdFieldNameType");
                            break;
                        }
                }
            }

            return DataServices.ProcessADOCommand(ref sSQLStr, sADOConn, false);

        }

        // Drop table from Database
        public static bool DropTable(string pTableName, Databas pDatabaseEntity)
        {
            var sADOConn = DataServices.DBOpen(Enums.eConnectionType.conDefault, null);
            if (pDatabaseEntity is not null)
                sADOConn = DataServices.DBOpen(Enums.eConnectionType.conDatabase, pDatabaseEntity);

            return DatabaseMap.DropTable(pTableName, sADOConn);
        }

        public static bool DropTable(string pTableName, ADODB.Connection oADOConn)
        {
            bool rtn;
            string sSQLStr;

            try
            {
                sSQLStr = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[" + pTableName.Trim() + "]') AND type in (N'U')) ";
                sSQLStr = sSQLStr + "DROP TABLE [" + pTableName.Trim() + "];";
                rtn = DataServices.ProcessADOCommand(ref sSQLStr, oADOConn, false);
            }
            catch (Exception)
            {
                rtn = false;
            }

            if (rtn)
            {
                try
                {
                    sSQLStr = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[" + pTableName.Trim() + "]') AND type in (N'V')) ";
                    sSQLStr = sSQLStr + "DROP VIEW [" + pTableName.Trim() + "];";
                    DataServices.ProcessADOCommand(ref sSQLStr, oADOConn, false);
                }
                catch (Exception)
                {
                }
            }

            return rtn;
        }

        // Get all child and parent tables for selected table 
        public static List<string> GetChildTableIds(string pTableName, IDBManager pIDBManager, HttpContext httpContext)
        {
            var lTableNames = new List<string>();
            pIDBManager.ConnectionString = Keys.get_GetDBConnectionString();
            pIDBManager.ExecuteReader(CommandType.Text, "SELECT * FROM dbo.FNGetAllChildParentTableList('" + pTableName + "');");
            while (pIDBManager.DataReader.Read())
            {
                if (!pIDBManager.DataReader.IsDBNull(pIDBManager.DataReader.GetOrdinal("TableName")))
                {
                    lTableNames.Add(pIDBManager.DataReader.GetString(pIDBManager.DataReader.GetOrdinal("TableName")));
                }
            }
            pIDBManager.Dispose();
            return lTableNames;
        }

        // Get all child and parent tables dependency during un attach
        public static void GetTableDependency(IQueryable<Table> lTableEntities, Repositories<View> _iView, Repositories<ViewColumn> _iViewColumn, Table pUpperTableEntity, Table pLowerTableEntity, ref string sViewMessage)
        {


            var lViewEntities = _iView.All();
            var lViewColumnEntities = _iViewColumn.All();
            // sViewMessage = sViewMessage + "<ul>"

            // See if in use in a ViewColumn
            foreach (var oTables in lTableEntities)
            {
                var lLoopViewEntities = lViewEntities.Where(x => x.TableName.Trim().ToLower() == oTables.TableName.Trim().ToLower());
                foreach (var oViews in lLoopViewEntities)
                {
                    var lInViewColumnEntities = lViewColumnEntities.Where(x => x.ViewsId == oViews.Id);
                    foreach (var oViewColumns in lInViewColumnEntities)
                    {
                        switch (oViewColumns.LookupType)
                        {
                            case 1:
                                {
                                    if (Strings.InStr(oViewColumns.FieldName, ".") > 1)
                                    {
                                        if (ReferenceEquals(oTables, pLowerTableEntity))
                                        {
                                            if (Strings.StrComp(Strings.Left(oViewColumns.FieldName, Strings.InStr(oViewColumns.FieldName, ".") - 1), pUpperTableEntity.TableName, Constants.vbTextCompare) == 0)
                                            {
                                                if (Strings.InStr(sViewMessage, "  Used in View" + Constants.vbTab + "\"" + oViews.ViewName + "\"" + Constants.vbCrLf) == 0)
                                                {
                                                    sViewMessage = sViewMessage + "<li> Used in View " + Constants.vbTab + "\"" + oViews.ViewName + "\"</li>";
                                                }
                                            }
                                        }
                                    }

                                    break;
                                }

                            case 12:
                            case 13:
                            case 14:
                            case 15:
                                {
                                    if ((bool)(-1 is var arg15 && oViewColumns.LookupIdCol is { } arg14 ? arg14 == arg15 : (bool?)null))
                                    {
                                        if (Strings.InStr(oViewColumns.FieldName, ".") > 1)
                                        {
                                            if (ReferenceEquals(oTables, pUpperTableEntity))
                                            {
                                                if (Strings.StrComp(Strings.Left(oViewColumns.FieldName, Strings.InStr(oViewColumns.FieldName, ".") - 1), pLowerTableEntity.TableName, Constants.vbTextCompare) == 0)
                                                {
                                                    if (Strings.InStr(sViewMessage, "  Used in View" + Constants.vbTab + "\"" + oViews.ViewName + "\"" + Constants.vbCrLf) == 0)
                                                    {
                                                        sViewMessage = sViewMessage + "<li> Used in View " + Constants.vbTab + "\"" + oViews.ViewName + "\"</li>";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (Strings.InStr(oViewColumns.FieldName, ".") > 1)
                                    {
                                        if (Strings.StrComp(Strings.Left(oViewColumns.FieldName, Strings.InStr(oViewColumns.FieldName, ".") - 1), pUpperTableEntity.TableName, Constants.vbTextCompare) == 0)
                                        {
                                            if(((oViewColumns.LookupIdCol >= 0) && (oViewColumns.LookupIdCol <= lInViewColumnEntities.Count())))
                                            {
                                                var oTmpViewColumns = lInViewColumnEntities.Where(x => x.LookupIdCol == oViewColumns.LookupIdCol + 1).FirstOrDefault();
                                                if (oTmpViewColumns is not null)
                                                {
                                                    if (Strings.StrComp(Strings.Left(oTmpViewColumns.FieldName, Strings.InStr(oTmpViewColumns.FieldName, ".") - 1), pLowerTableEntity.TableName, Constants.vbTextCompare) == 0)
                                                    {
                                                        if (Strings.InStr(sViewMessage, "  Used in View" + Constants.vbTab + "\"" + oViews.ViewName + "\"" + Constants.vbCrLf) == 0)
                                                        {
                                                            sViewMessage = sViewMessage + "<li> Used in View " + Constants.vbTab + "\"" + oViews.ViewName + "\"</li>";
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    break;
                                }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(sViewMessage))
            {
                sViewMessage = "<ul>" + sViewMessage + "</ul>";
            }
            // Return sViewMessage
        }

        // Get all child and parent tables for selected table 
        public static List<string> GetAttachTableFieldsList(Table pTableEntity, Table pParentTableEntity, Repositories<Databas> _iDatabas)
        {

            var pDatabaseEntity = new Databas();
            var rsADO = new ADODB.Recordset();
            var psADOConn = new ADODB.Connection();
            var csADOConn = new ADODB.Connection();
            var iFieldType = default(int);
            var lFieldSize = default(long);
            List<SchemaColumns> oSchemaColumns;
            var lColumnNames = new List<string>();

            if (pParentTableEntity is not null)
            {
                pDatabaseEntity = _iDatabas.Where(x => x.DBName.Trim().ToLower().Equals(pParentTableEntity.DBName.Trim().ToLower())).FirstOrDefault();
                psADOConn = DataServices.DBOpen(pDatabaseEntity);
                string argsSql = "SELECT TOP 1 * FROM [" + pParentTableEntity.TableName + "]";
                string arglError = "";
                rsADO = DataServices.GetADORecordSet(ref argsSql, psADOConn, lError: ref arglError);
            }

            if (rsADO is not null)
            {
                lFieldSize = (Int64)rsADO.Fields[RemoveTableNameFromField(pParentTableEntity.IdFieldName)].DefinedSize;
                rsADO.Close();
                rsADO = null;
            }
            pDatabaseEntity = null;
            pDatabaseEntity = _iDatabas.Where(x => x.DBName.Trim().ToLower().Equals(pTableEntity.DBName.Trim().ToLower())).FirstOrDefault();
            csADOConn = DataServices.DBOpen(pDatabaseEntity);
            string argsSql1 = "SELECT TOP 1 * FROM [" + pTableEntity.TableName + "]";
            string arglError1 = "";
            rsADO = DataServices.GetADORecordSet(ref argsSql1, csADOConn, lError: ref arglError1);

            if (rsADO is not null)
            {
                oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(pParentTableEntity.TableName, psADOConn, RemoveTableNameFromField(pParentTableEntity.IdFieldName));

                if (oSchemaColumns.Count > 0)
                {
                    iFieldType = (int)oSchemaColumns[0].DataType;
                    oSchemaColumns = null;
                }

                // Load all fields from the Table
                oSchemaColumns = SchemaInfoDetails.GetSchemaInfo(Strings.Trim(pTableEntity.TableName), csADOConn);

                foreach (var oSchema in oSchemaColumns)
                {
                    if (!SchemaInfoDetails.IsSystemField(oSchema.ColumnName))
                    {
                        if ((int)oSchema.DataType == iFieldType & lFieldSize == (long)rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oSchema.ColumnName)].DefinedSize)
                        {
                            lColumnNames.Add(Strings.Trim(oSchema.ColumnName));
                        }
                    }
                }

                oSchemaColumns = null;
                rsADO.Close();
                rsADO = null;
            }

            return lColumnNames;
        }

        public static IQueryable<Table> GetAttachExistingTableList(IQueryable<Table> lTablesEntities, int iParentTableId, int iTabSetId, Repositories<TableTab> _iTabletab, Repositories<RelationShip> _iRelationShip, IDBManager _IDBManager, HttpContext httpContext)
        {
            string sTableName;
            var lEngineTablesList = CollectionsClass.EngineTablesList;
            var lEngineTablesNotNeededList = CollectionsClass.EngineTablesNotNeededList;
            var lTableList = new List<string>();
            lTablesEntities = from q in lTablesEntities
                              where !lEngineTablesList.Any(x => (x.Trim().ToLower() ?? "") == (q.TableName.Trim().ToLower() ?? ""))
                              select q;
            lTablesEntities = from q in lTablesEntities
                              where !lEngineTablesNotNeededList.Any(x => (x.Trim().ToLower() ?? "") == (q.TableName.Trim().ToLower() ?? ""))
                              select q;

            if (iParentTableId == 0)
            {
                var lTableTabsEntities = _iTabletab.All();
                var lTableTabsTableList = from q in lTableTabsEntities.Where(x => x.TabSet == iTabSetId) 
                                          select q.TableName;
                foreach (var sTableNameLoop in lTableTabsTableList)
                    lTableList.Add(sTableNameLoop);
                lTablesEntities = from q in lTablesEntities
                                  where !lTableList.Any(x => (x.Trim().ToLower() ?? "") == (q.TableName.Trim().ToLower() ?? ""))
                                  select q;
            }
            else
            {
                // sTableName = lTablesEntities.Where(Function(x) x.UserName.Trim().ToLower().Equals(sParentTable.Trim().ToLower())).FirstOrDefault().TableName
                sTableName = lTablesEntities.Where(x => x.TableId == iParentTableId).FirstOrDefault().TableName;
                var lRel = _iRelationShip.All();
                lTablesEntities = from q in lTablesEntities
                                  where !q.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())
                                  select q;
                lTableList = new List<string>();
                foreach (var sTableNameLoop in GetChildTableIds(sTableName, _IDBManager, httpContext))
                    lTableList.Add(sTableNameLoop);
                // lTablesEntities = From q In lTablesEntities Where Not (DatabaseMap.GetChildTableIds(sTableName, _IDBManager).Any(Function(x) x.Trim().ToLower() = q.TableName.Trim().ToLower())) Select q
                lTablesEntities = from q in lTablesEntities
                                  where !lTableList.Any(x => (x.Trim().ToLower() ?? "") == (q.TableName.Trim().ToLower() ?? ""))
                                  select q;
            }

            return lTablesEntities;

        }

        public static bool IncrementCounter(string sCounterName, ADODB.Connection sADOConn, string NewCounter = "0")
        {
            var bIsNull = default(bool);
            string counter = string.Empty;
            int lCount;
            var lErrorCount = default(int);
            int lError;
            string sErrMsg = string.Empty;
            string sSQL;
            string sQuote = string.Empty;
            ADODB.Recordset rstADO;
            SchemaColumns oSchemaColumn;
            int pCounter;
            ;

            oSchemaColumn = SchemaInfoDetails.GetSchemaInfo("System", sADOConn, sCounterName).SingleOrDefault();
            if (oSchemaColumn is not null && oSchemaColumn.IsString)
                sQuote = "'";

            lbl_IncrementCounter_Restart:
            ;

            sSQL = string.Format("SELECT [{0}] FROM System", sCounterName);
            string arglError = "";
            rstADO = DataServices.GetADORecordSet(ref sSQL, sADOConn, lError: ref arglError); // , RMAG.CursorTypeEnum.rmOpenForwardOnly, RMAG.LockTypeEnum.rmLockReadOnly)

            if (rstADO.EOF)
            {
                pCounter = 1;
            }
            else if (rstADO.Fields[sCounterName] is DBNull)
            {
                bIsNull = true;
                pCounter = 1;
            }
            else
            {
                pCounter = Conversions.ToInteger(rstADO.Fields[(object)sCounterName].ToString());
            }

            counter = pCounter.ToString(); // IncrementCounter.CounterValue(sCounterName)
            rstADO.Close();
            // Update the field to turn on edit mode (and hopefully lock the record!)
            if (Conversions.ToDouble(counter) >= Conversions.ToDouble(NewCounter))
                NewCounter = (Conversions.ToDouble(counter) + 1d).ToString();
            sSQL = string.Format("UPDATE [System] SET [{0}] = {1}{2}{1} WHERE ", sCounterName, sQuote, NewCounter);

            if (bIsNull)
            {
                sSQL += string.Format("[{0}] IS NULL", sCounterName);
            }
            else
            {
                sSQL += string.Format("[{0}] = {1}{2}{1}", sCounterName, sQuote, counter);
            }

            lError = 0;
            sErrMsg = string.Empty;
            lCount = DataServices.ProcessADOCommand(ref sSQL, sADOConn, true, ref lError, ref sErrMsg);

            if (sErrMsg.ToLower().Contains("overflow"))
            {
                throw new OverflowException(string.Format("The value {0} is too large to fit into the System.{1} field.  Please contact your system administrator.", NewCounter, sCounterName));
            }

            if (lCount != 1)
            {
                lErrorCount = lErrorCount + 1;

                if (lErrorCount < 1000)
                {
                    goto lbl_IncrementCounter_Restart;
                }

            }

            return true;

        lbl_IncrementCounter:
            ;

            if (sErrMsg.ToLower().Contains("overflow"))
            {
                throw new OverflowException(string.Format("The value {0} is too large to fit into the System.{1} field.  Please contact your system administrator.", NewCounter, sCounterName));
            }

            lErrorCount = lErrorCount + 1;

            if (lErrorCount < 1000)
            {
                ;
            }

        }


    }
}