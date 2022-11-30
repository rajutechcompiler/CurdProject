using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Smead.Security;
using TabFusionRMS.Models;
using TabFusionRMS.Resource;

namespace TabFusionRMS.WebCS
{

    public sealed class ReportsModel
    {

        public static Collection mcFieldName = new Collection();
        public static Collection mcRelationships = new Collection();
        public static Collection mcLevel = new Collection();
        public const string TRACKED_LOCATION_NAME = "SLTrackedDestination";

        public static string GetBindReportsMenus(string root, IQueryable<Table> lTableEntities, IQueryable<View> lViewEntities, 
            IQueryable<ReportStyle> lReportStyleEntities, Passport _passport, HttpContext httpContext)
        {
            string strALId = "";
            StringBuilder strViewMenu = new StringBuilder();

            
            bool mbMgrGroup = _passport.CheckAdminPermission((Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
            int iCntRpt = Convert.ToInt32(httpContext.Session.GetString("iCntRpts"));

            if (mbMgrGroup | iCntRpt > 0)
            {

                strALId = "ALRPT_1";
                strViewMenu.Append("<li>");

                strViewMenu.Append(string.Format("<a href='#' id='{0}' onclick=ReportRootItemClick('{1}') class='ReportDefinitions'>{2}</a>", strALId.ToString().Trim(), strALId.ToString().Trim(), Languages.get_Translation("mnuReportDefinitions")));

                strViewMenu.Append("<ul>");
                foreach (var oTable in lTableEntities)
                {
                    if (!CollectionsClass.IsEngineTable(oTable.TableName))
                    {
                        if (_passport.CheckPermission(oTable.TableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.View))
                        {
                            var lTableViewList = lViewEntities.Where(x => (x.TableName.Trim().ToLower() ?? "") == (oTable.TableName.Trim().ToLower() ?? ""));
                            foreach (var oView in lTableViewList)
                            {
                                if ((bool)oView.Printable)
                                {
                                    if (_passport.CheckPermission(oView.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Reports, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Configure))
                                    {
                                        if (_passport.CheckPermission(oView.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Reports, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.View))
                                        {
                                            bool pCheckSubViewExist = true;
                                            if (oView.SubViewId is not null)
                                            {
                                                var oCheckSubViewExist = lViewEntities.Where(x => x.SubViewId ==oView.Id).FirstOrDefault();
                                                if (oCheckSubViewExist is not null)
                                                {
                                                    pCheckSubViewExist = false;
                                                }
                                            }

                                            if (pCheckSubViewExist)
                                            {
                                                // Dim ChildReportsNode = New JSTreeView.ListItem(oView.ViewName & " (" & oTable.UserName & ")", Guid.NewGuid.ToString() + "_childReports_" + oView.Id.ToString(), className:="jstree-open", dataJsTree:=dataJsTree)
                                                // ReportNode.Nodes.Add(ChildReportsNode)

                                                strViewMenu.Append(string.Format("<li><a id='FALRPT_{0}' onclick=ReportChildItemClick('{2}','{3}','FALRPT_{0}')>{1} ({4})</a></li>", oView.Id.ToString().Trim(), oView.ViewName, root, strALId, oTable.UserName));

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                strViewMenu.Append("</ul></li>");
            }
            // '''''''''''''''''''''''''''''''''''''''''
            // Load the Report Styles
            // '''''''''''''''''''''''''''''''''''''''''
            if (_passport.CheckPermission(Keys.SECURE_REPORT_STYLES, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Application, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access))
            {
                // Dim ReportStypeNode = New JSTreeView.ListItem("Report Styles", Guid.NewGuid.ToString() + "_rootReportStyle_-2", className:="jstree-open", dataJsTree:=dataJsTree)
                // rootNode.Nodes.Add(ReportStypeNode)
                strALId = "ALRPTSTL_2";
                strViewMenu.Append("<li>");
                strViewMenu.Append(string.Format("<a href='#' id='{0}' onclick=ReportStyleRootItemClick('{1}') class='ReportStyles'>{2}</a>", strALId.ToString().Trim(), strALId.ToString().Trim(), Languages.get_Translation("mnuReportStyles")));
                strViewMenu.Append("<ul>");

                foreach (var oReportStyles in lReportStyleEntities)
                    // Dim ChildReportStypeNode = New JSTreeView.ListItem(oReportStyles.Id, Guid.NewGuid.ToString() + "_childReportStyle_" + oReportStyles.ReportStylesId.ToString(), className:="jstree-open", dataJsTree:=dataJsTree)
                    // ReportStypeNode.Nodes.Add(ChildReportStypeNode)
                    strViewMenu.Append(string.Format("<li><a id='FALRPTSTL_{0}' onclick=ReportStyleChildItemClick('{2}','{3}','FALRPTSTL_{0}')>{1}</a></li>", oReportStyles.ReportStylesId.ToString().Trim(), oReportStyles.Id, root, strALId));
                strViewMenu.Append("</ul></li>");
            }
            // End If

            // treeNode.ListItems.Add(rootNode)
            // treeView.Nodes.Add(treeNode)
            return strViewMenu.ToString();
        }

        public static void FillViewColField(IQueryable<Table> tableObjList, IQueryable<RelationShip> relationObjList, ref List<KeyValuePair<string, string>> FieldNameList, ADODB.Connection sADOConn, Table orgTable, IQueryable<RelationShip> relationShipEntity, bool bDoUpper, int iLevel, bool bNumericOnly)
        {
            try
            {
                Table tableEntity;
                var schemaColumn = new List<SchemaColumns>();
                string sFieldName = "";
                if (relationShipEntity is not null)
                {
                    if (iLevel == 1)
                    {
                        mcFieldName.Clear();
                        mcRelationships.Clear();
                        mcLevel.Clear();
                    }

                    foreach (RelationShip relationObj in relationShipEntity)
                    {
                        if (bDoUpper)
                        {
                            tableEntity = tableObjList.Where(m => m.TableName.Trim().ToLower().Equals(relationObj.UpperTableName.Trim().ToLower())).FirstOrDefault();
                        }
                        else
                        {
                            tableEntity = tableObjList.Where(m => m.TableName.Trim().ToLower().Equals(relationObj.LowerTableName.Trim().ToLower())).FirstOrDefault();
                        }
                        if (tableEntity is not null)
                        {
                            if (!tableEntity.TableName.Trim().ToLower().Equals(orgTable.TableName.Trim().ToLower()))
                            {
                                schemaColumn = SchemaInfoDetails.GetTableSchemaInfo(tableEntity.TableName, sADOConn);
                            }
                            int schemaColCount = schemaColumn.Count;
                            if (schemaColumn is not null)
                            {
                                for (int icol = 0, loopTo = schemaColumn.Count - 1; icol <= loopTo; icol++)
                                {
                                    var schemaColumnItem = schemaColumn[icol];
                                    if (bDoUpper)
                                    {
                                        if (!DatabaseMap.RemoveTableNameFromField(tableEntity.IdFieldName).Equals(DatabaseMap.RemoveTableNameFromField(schemaColumnItem.ColumnName)))
                                        {
                                            sFieldName = "";
                                            if (mcFieldName.Count != 0)
                                            {
                                                string UpperTableName = relationObj.UpperTableName;
                                                string schemaColVar = schemaColumnItem.ColumnName;
                                                if (mcFieldName.Contains(Strings.Trim(Strings.UCase(relationObj.UpperTableName) + "." + Strings.UCase(schemaColumnItem.ColumnName))))
                                                {
                                                    sFieldName = Conversions.ToString(mcFieldName[Strings.Trim(Strings.UCase(relationObj.UpperTableName) + "." + Strings.UCase(schemaColumnItem.ColumnName))]);
                                                }
                                            }
                                            if (string.IsNullOrEmpty(sFieldName))
                                            {
                                                if (!bNumericOnly | SchemaInfoDetails.IsANumericType(schemaColumnItem.DataType))
                                                {
                                                    if (!SchemaInfoDetails.IsSystemField(schemaColumnItem.ColumnName))
                                                    {
                                                        FieldNameList.Add(new KeyValuePair<string, string>(Strings.Trim(relationObj.UpperTableName + "." + schemaColumnItem.ColumnName), relationObj.UpperTableName + "." + schemaColumnItem.ColumnName));
                                                        // mcFieldName.Add(Trim(UCase(relationObj.UpperTableName) + "." + UCase(schemaColumnItem.ColumnName)), Trim(UCase(relationObj.UpperTableName) + "." + UCase(schemaColumnItem.ColumnName)))
                                                        mcFieldName.Add(relationObj.LowerTableFieldName, Strings.Trim(Strings.UCase(relationObj.UpperTableName) + "." + Strings.UCase(schemaColumnItem.ColumnName)));
                                                        mcRelationships.Add(relationObj, Strings.Trim(Strings.UCase(relationObj.UpperTableName) + "." + Strings.UCase(schemaColumnItem.ColumnName)));
                                                        mcLevel.Add(iLevel, Strings.Trim(Strings.UCase(relationObj.UpperTableName) + "." + Strings.UCase(schemaColumnItem.ColumnName)));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else if (!DatabaseMap.RemoveTableNameFromField(relationObj.LowerTableFieldName).Equals(DatabaseMap.RemoveTableNameFromField(schemaColumnItem.ColumnName)))
                                    {
                                        if (mcFieldName.Count != 0)
                                        {
                                            if (mcFieldName.Contains(Strings.Trim(Strings.UCase(relationObj.LowerTableName) + "." + Strings.UCase(schemaColumnItem.ColumnName))))
                                            {
                                                sFieldName = Conversions.ToString(mcFieldName[Strings.UCase(relationObj.LowerTableName) + "." + Strings.UCase(schemaColumnItem.ColumnName)]);
                                            }

                                        }
                                        if (string.IsNullOrEmpty(sFieldName))
                                        {
                                            if (!bNumericOnly | SchemaInfoDetails.IsANumericType(schemaColumnItem.DataType))
                                            {
                                                if (!SchemaInfoDetails.IsSystemField(schemaColumnItem.ColumnName))
                                                {
                                                    FieldNameList.Add(new KeyValuePair<string, string>(Strings.Trim(relationObj.LowerTableName + "." + schemaColumnItem.ColumnName), relationObj.LowerTableName + "." + schemaColumnItem.ColumnName));
                                                    // mcFieldName.Add(Trim(UCase(relationObj.LowerTableName) + "." + UCase(schemaColumnItem.ColumnName)), Trim(UCase(relationObj.LowerTableName) + "." + UCase(schemaColumnItem.ColumnName)))
                                                    mcFieldName.Add(relationObj.UpperTableFieldName, Strings.Trim(Strings.UCase(relationObj.LowerTableName) + "." + Strings.UCase(schemaColumnItem.ColumnName)));
                                                    mcRelationships.Add(relationObj, Strings.Trim(Strings.UCase(relationObj.LowerTableName) + "." + Strings.UCase(schemaColumnItem.ColumnName)));
                                                    mcLevel.Add(iLevel, Strings.Trim(Strings.UCase(relationObj.LowerTableName) + "." + Strings.UCase(schemaColumnItem.ColumnName)));
                                                }
                                            }
                                        }
                                    }
                                }
                                schemaColumn = null;
                            }
                        }
                        if (iLevel < 2)
                        {
                            var recursiveParent = relationObjList.Where(m => m.LowerTableName.Trim().ToLower().Equals(tableEntity.TableName.Trim().ToLower())).OrderBy(m => m.TabOrder);
                            if (recursiveParent is not null)
                            {
                                FillViewColField(tableObjList, relationObjList, ref FieldNameList, sADOConn, orgTable, recursiveParent, true, iLevel + 1, bNumericOnly);
                            }
                        }
                    }

                }
            }

            catch (Exception)
            {
                throw;
            } // ex
        }

        // Public Shared Function GetTrackedLocationName() As String
        // Const TRACKED_LOCATION_NAME As String = "SLTrackedDestination"
        // Return TRACKED_LOCATION_NAME
        // End Function

    }
}