using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Smead.RecordsManagement;
using clsNavigation = Smead.RecordsManagement.Navigation;
using clsTracking = Smead.RecordsManagement.Tracking;
using TabFusionRMS.DataBaseManagerVB;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.Resource;
using Smead.Security;

namespace TabFusionRMS.WebCS
{
    public class TrackingServices
    {

        private const string SL_USERNAME = "@@SL_UserName";

        private IDBManager _IDBManager { get; set; }
        private IRepository<Table> _iTables { get; set; }
        private IRepository<Databas> _iDatabas { get; set; }
        private IRepository<ScanList> _iScanList { get; set; }
        private IRepository<TabFusionRMS.Models.System> _iSystem { get; set; }
        private IRepository<TabSet> _iTabSet { get; set; }
        private IRepository<TableTab> _iTableTab { get; set; }
        private IRepository<RelationShip> _iRelationship { get; set; }
        private IRepository<View> _iView { get; set; }
        private IRepository<TrackingHistory> _iTrackingHistory { get; set; }
        public const int TRACKING_LOCATION = 1;
        public const int TRACKING_EMPLOYEE = 2;
        private static int miBatchNum = 0;

        public static string msSignatureFile;
        public static string SignatureFile
        {
            get
            {
                return msSignatureFile;
            }
            set
            {
                msSignatureFile = value;
            }
        }

        public static DateTime StartTime
        {
            get
            {
                return msStartTime;
            }
            set
            {
                msStartTime = value;
            }
        }
        public static DateTime msStartTime;

        public static bool TelxonModeOn
        {
            get
            {
                return mbTelxonModeOn;
            }
            set
            {
                mbTelxonModeOn = value;
            }
        }
        public static bool mbTelxonModeOn;

        public static DateTime ScanDateTime
        {
            get
            {
                return mdScanDateTime;
            }
            set
            {
                mdScanDateTime = value;
            }
        }
        public static DateTime mdScanDateTime;

        public static string TelxonUserName
        {
            get
            {
                return msTelxonUserName;
            }
            set
            {
                msTelxonUserName = value;
            }
        }
        public static string msTelxonUserName;


        public static bool FromEXE
        {
            get
            {
                return mbFromEXE;
            }
            set
            {
                mbFromEXE = value;
            }
        }
        public static bool mbFromEXE { get; set; }

        public TrackingServices(IDBManager IDBManager, IRepository<Table> iTable, IRepository<Databas> iDatabase, IRepository<ScanList> iScanList, IRepository<TabSet> iTabSet, IRepository<TableTab> iTableTab, IRepository<RelationShip> iRelationship, IRepository<View> iView, IRepository<TabFusionRMS.Models.System> iSystem, IRepository<TrackingHistory> iTrackingHistory) : base()
        {
            _IDBManager = IDBManager;
            _iTables = iTable;
            _iDatabas = iDatabase;
            _iScanList = iScanList;
            _iTabSet = iTabSet;
            _iTableTab = iTableTab;
            _iRelationship = iRelationship;
            _iView = iView;
            _iSystem = iSystem;
            _iTrackingHistory = iTrackingHistory;
        }

        public static FoundBarCode BarCodeLookup(Smead.Security.Passport passport, IQueryable<Databas> iDatabaseQuery, IQueryable<Table> iTableQuery, IQueryable<TabFusionRMS.Models.System> iSystemQuery, IQueryable<ScanList> iScanListQuery, IQueryable<View> iViewQuery, IQueryable<TableTab> iTableTabQuery, IQueryable<TabSet> iTabSetQuery, IQueryable<RelationShip> iRelationShipQuery, string LookupString, ref string sTabsetIds, [Optional, DefaultParameterValue(null)] ref Table oCurrentTable, [Optional, DefaultParameterValue(false)] ref bool bSecurityCheck)
        {
            FoundBarCode oFoundBarCode;

            if (sTabsetIds is not null && sTabsetIds.Length > 0)
            {
                var cTabsetIds = new Collection();
                var asTabsetIds = Strings.Split(sTabsetIds, ",");

                foreach (string s in asTabsetIds)
                    cTabsetIds.Add(null, s);
                oFoundBarCode = BarCodeLookup(passport, iDatabaseQuery, iTableQuery, iSystemQuery, iScanListQuery, iViewQuery, iTableTabQuery, iTabSetQuery, iRelationShipQuery, LookupString, ref cTabsetIds, ref oCurrentTable, ref bSecurityCheck);
                sTabsetIds = "";

                for (int i = 0, loopTo = cTabsetIds.Count - 1; i <= loopTo; i++)
                {
                    sTabsetIds += cTabsetIds[i].ToString();
                    if (i < cTabsetIds.Count - 1)
                        sTabsetIds += ",";
                }
            }
            else
            {
                var argcTabsetIds = new Collection();
                oFoundBarCode = BarCodeLookup(passport, iDatabaseQuery, iTableQuery, iSystemQuery, iScanListQuery, iViewQuery, iTableTabQuery, iTabSetQuery, iRelationShipQuery, LookupString, ref argcTabsetIds, ref oCurrentTable, ref bSecurityCheck);
            }

            return oFoundBarCode;
        }

        //[ComVisible(false)]
        public static FoundBarCode BarCodeLookup(Smead.Security.Passport passport, IQueryable<Databas> iDatabaseQuery, IQueryable<Table> iTableQuery, IQueryable<TabFusionRMS.Models.System> iSystemQuery, IQueryable<ScanList> iScanListQuery, IQueryable<View> iViewQuery, IQueryable<TableTab> iTableTabQuery, IQueryable<TabSet> iTabSetQuery, IQueryable<RelationShip> iRelationShipQuery, string LookupString, [Optional, DefaultParameterValue(null)] ref Collection cTabsetIds, [Optional, DefaultParameterValue(null)] ref Table oCurrentTable, [Optional, DefaultParameterValue(false)] ref bool bSecurityCheck)
        {
            FoundBarCode BarCodeLookupRet = default;
            int iCnt;
            string sLookupStr = string.Empty;
            bool bGoAhead;
            bool bOrigSecurity;
            bool bDidIdFldName;
            string sDescription = string.Empty;
            ScanList oScanList = null;
            SchemaColumns oSchemaColumns = null;
            Table oTable = null;
            TabSet oTabSets;
            ADODB.Recordset rsADO = null;
            BarCodeLookupRet = null;
            bOrigSecurity = bSecurityCheck;
            // Lookup for Barcode Prefix Matches
            // Dim passport = New Smead.Security.Passport
            var oTableObj = iTableQuery;
            var loopTo = oTableObj.Count() - 1;
            for (iCnt = 0; iCnt <= loopTo; iCnt++)
            {
                if (oCurrentTable is null)
                {
                    oTable = oTableObj.AsEnumerable().ElementAt(iCnt);
                }
                else
                {
                    oTable = oCurrentTable;
                }
                var sADOCon = DataServices.DBOpen(oTable, iDatabaseQuery);
                if (Strings.Len(oTable.BarCodePrefix) > 0 | oCurrentTable is not null)
                {
                    sLookupStr = Strings.Replace(LookupString, oTable.IdStripChars, "", Compare: CompareMethod.Text);

                    if (Strings.Len(oTable.BarCodePrefix) > 0 & Strings.StrComp(Strings.Left(sLookupStr, Strings.Len(oTable.BarCodePrefix)), oTable.BarCodePrefix, CompareMethod.Text) == 0)
                    {
                        sLookupStr = Strings.Mid(sLookupStr, Strings.Len(oTable.BarCodePrefix) + 1);
                        sLookupStr = Strings.Replace(sLookupStr, oTable.IdStripChars, "", Compare: CompareMethod.Text);
                        bGoAhead = true;
                    }
                    else if (oCurrentTable is not null)
                    {
                        sLookupStr = LookupString;
                        bGoAhead = true;
                    }
                    else
                    {
                        bGoAhead = false;
                    }

                    bool IfIdFieldIsString = DataServices.IdFieldIsString(sADOCon, oTable.TableName, oTable.IdFieldName);
                    if (bGoAhead)
                    {
                        // if dest is numeric & lookupstr is a string, don't attempt lookup
                        if (!IfIdFieldIsString & !Information.IsNumeric(sLookupStr))
                            bGoAhead = false;
                    }

                    if (bGoAhead)
                    {
                        if (IfIdFieldIsString & Strings.Len(oTable.IdMask) > 0)
                            sLookupStr = Strings.Format(sLookupStr, oTable.IdMask);
                        rsADO = FindUserRecord(oTable.TableName, oTable.IdFieldName, oTable.IdFieldName2, oTable.TrackingACTIVEFieldName, oTable.TrackingOUTFieldName, oTable.TrackingEmailFieldName, oTable.TrackingDueBackDaysFieldName, oTable.OperatorsIdField, oTable.IdFieldName, IfIdFieldIsString, sLookupStr, iTableQuery, iDatabaseQuery);

                        if (rsADO is not null)
                        {
                            int count = rsADO.RecordCount;
                            if (!rsADO.EOF)
                            {
                                if (VerifyViewSecurity(passport, iViewQuery, iDatabaseQuery, oTable, sLookupStr, oTable.IdFieldName, IfIdFieldIsString, bOrigSecurity) > 0)
                                {
                                    bSecurityCheck = true;
                                    break;
                                }
                                else
                                {
                                    bSecurityCheck = false;
                                }
                            }

                            rsADO.Close();
                        }

                        rsADO = null;
                    }
                }

                if (oCurrentTable is not null)
                    break;
            }

            if (rsADO is null)
            {

                bDidIdFldName = false;
                IQueryable<ScanList> oScanListObj = iScanListQuery.OrderBy(m => m.Id);
                var loopTo1 = oScanListObj.Count();
                for (iCnt = 0; iCnt <= loopTo1; iCnt++)
                {
                    if (iCnt == oScanListObj.Count())
                    {
                        oTable = null;

                        if (oCurrentTable is not null)
                        {
                            if (!bDidIdFldName)
                            {
                                var oScanNewObj = new ScanList();
                                oTable = oCurrentTable;
                                oScanList = oScanNewObj;
                                oScanList.FieldName = oTable.IdFieldName;
                                oScanList.TableName = oTable.TableName;
                                oScanList.IdMask = oTable.IdMask;
                                oScanList.IdStripChars = oTable.IdStripChars;
                            }
                        }
                    }
                    else
                    {
                        oScanList = oScanListObj.AsEnumerable().ElementAt(iCnt);
                        if (oCurrentTable is null)
                        {
                            oTable = oTableObj.Where(m => m.TableName.Trim().ToLower().Equals(oScanList.TableName.Trim().ToLower())).FirstOrDefault();
                        }
                        else
                        {
                            oTable = null;
                            if (Strings.StrComp(oScanList.TableName, oCurrentTable.TableName, CompareMethod.Text) == 0)
                                oTable = oCurrentTable;
                        }
                    }

                    if (oTable is not null)
                    {
                        // Dim DbObj = iDatabaseQuery.Where(Function(m) m.DBName.Trim.ToLower.Equals(oTable.DBName.Trim.ToLower)).FirstOrDefault
                        var sADOCon = DataServices.DBOpen(oTable, iDatabaseQuery);
                        sLookupStr = LookupString;
                        sLookupStr = Strings.Replace(sLookupStr, oScanList.IdStripChars, "", Compare: CompareMethod.Text);

                        if (Strings.Len(oTable.BarCodePrefix) > 0 & Strings.StrComp(Strings.Left(sLookupStr, Strings.Len(oTable.BarCodePrefix)), oTable.BarCodePrefix, CompareMethod.Text) == 0)
                        {
                            sLookupStr = Strings.Mid(sLookupStr, Strings.Len(oTable.BarCodePrefix) + 1);
                        }
                        var oSchemaColumnsList = SchemaInfoDetails.GetSchemaInfo(oTable.TableName, sADOCon, DatabaseMap.RemoveTableNameFromField(oScanList.FieldName));
                        if (oSchemaColumnsList.Count != 0)
                        {
                            oSchemaColumns = oSchemaColumnsList[0];
                        }

                        // oSchemaColumns = oTable.ColumnSchema(oScanList.FieldName)

                        if (Strings.StrComp(DatabaseMap.RemoveTableNameFromField(oScanList.FieldName), DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), CompareMethod.Text) == 0)
                        {
                            if (Strings.StrComp(oScanList.TableName, oTable.TableName, CompareMethod.Text) == 0)
                            {
                                if (Strings.StrComp(oScanList.IdMask, oTable.IdMask, CompareMethod.Text) == 0)
                                {
                                    if (Strings.StrComp(oScanList.IdStripChars, oTable.IdStripChars, CompareMethod.Text) == 0)
                                        bDidIdFldName = true;
                                }
                            }
                        }

                        if (oSchemaColumns is not null)
                        {
                            // if dest is numeric & lookupstr is a string, don't attempt lookup
                            bGoAhead = true;
                            if (!oSchemaColumns.IsString & !Information.IsNumeric(sLookupStr))
                                bGoAhead = false;

                            if (bGoAhead)
                            {
                                if (oSchemaColumns.IsString)
                                {
                                    if (!string.IsNullOrEmpty(Strings.Trim(oScanList.IdMask)))
                                        sLookupStr = Strings.Format(sLookupStr, oScanList.IdMask);
                                }
                                rsADO = TrackingServices.FindUserRecord(oScanList.TableName,
                                    oTable.IdFieldName,
                                    oTable.IdFieldName2, 
                                    oTable.TrackingACTIVEFieldName,
                                    oTable.TrackingOUTFieldName,
                                    oTable.TrackingEmailFieldName,
                                    oTable.TrackingDueBackDaysFieldName,
                                    oTable.OperatorsIdField,
                                    oScanList.FieldName, 
                                    oSchemaColumns.IsString,
                                    sLookupStr, iTableQuery,
                                    iDatabaseQuery);
                                if (rsADO is not null)
                                {
                                    if (!rsADO.EOF)
                                    {
                                        if (TrackingServices.VerifyViewSecurity(passport, iViewQuery, iDatabaseQuery, oTable, sLookupStr, oScanList.FieldName, oSchemaColumns.IsString, bOrigSecurity) > 0)
                                        {
                                            bSecurityCheck = true;
                                            oScanList = null;
                                            break;
                                        }
                                        else
                                        {
                                            bSecurityCheck = false;
                                        }
                                    }

                                    rsADO.Close();
                                }

                                rsADO = null;
                            }
                        }
                    }

                    oScanList = null;
                }
            }

            if (rsADO is not null)
            {
                if (cTabsetIds is not null && cTabsetIds.Count > 0)
                {
                    // If matching to the a list of Tabsets, validate it here.
                    oTabSets = GetTabset(passport,
                        iTableTabQuery,
                        iTabSetQuery,
                        iRelationShipQuery,
                        iTableQuery,
                        ref oTable,
                        cTabsetIds);

                    if (oTabSets is null)
                    {
                        BarCodeLookupRet = null;
                        return BarCodeLookupRet;
                    }
                }
                else if (oCurrentTable is not null)
                {
                    if (Strings.StrComp(oCurrentTable.TableName, oTable.TableName, CompareMethod.Text) != 0)
                    {
                        // If matching to the Current Table, validate it here.
                        BarCodeLookupRet = null;
                        return BarCodeLookupRet;
                    }
                }

                BarCodeLookupRet = new FoundBarCode();
                BarCodeLookupRet.oTable = oTable;
                BarCodeLookupRet.TypedText = LookupString;
                BarCodeLookupRet.Id = rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)].Value.ToString();
                BarCodeLookupRet.UserLinkId = BarCodeLookupRet.Id;

                // If (Not oTable.IdFieldIsString) Then
                // Append values...
                // Dim DbObj = iDatabaseQuery.Where(Function(m) m.DBName.Trim.ToLower.Equals(oTable.DBName.Trim.ToLower)).FirstOrDefault
                var sADOCon = DataServices.DBOpen(oTable, iDatabaseQuery);
                bool IfIdFieldIsString = DataServices.IdFieldIsString(sADOCon, oTable.TableName, oTable.IdFieldName);
                if (!IfIdFieldIsString)
                {
                    BarCodeLookupRet.UserLinkId = Strings.Right(new string('0', DatabaseMap.UserLinkIndexTableIdSize) + BarCodeLookupRet.UserLinkId, DatabaseMap.UserLinkIndexTableIdSize);
                }

                int arglDueBackDays = -1;
                bool bEOF = false;
                //iDatabaseQuery, iSystemQuery, oTable, BarCodeLookup.Id, , , sDescription
                BarCodeLookupRet.UserDisplay = TrackingServices.GetUserDisplayString(iDatabaseQuery, iSystemQuery, oTable, BarCodeLookupRet.Id, ref rsADO, ref bEOF,sDescriptionOnly: ref sDescription, lDueBackDays: ref arglDueBackDays);

                BarCodeLookupRet.Description = sDescription;
                BarCodeLookupRet.HaveOut = !string.IsNullOrEmpty(Strings.Trim(oTable.TrackingOUTFieldName));
                // 3.0.214 MEF 8/11/2000 - Fix to Not using OutTable 
                BarCodeLookupRet.IsOut = false;

                if (BarCodeLookupRet.HaveOut)
                {

                    var tempTrackOut = rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.TrackingOUTFieldName)];
                    if (!(tempTrackOut is DBNull))
                    {
                        BarCodeLookupRet.IsOut = Convert.ToBoolean(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.TrackingOUTFieldName)]);
                    }
                }

                else
                {
                    if ((bool)(Convert.ToInt32(Enums.geTrackingOutType.otAlwaysOut) is var arg2 && oTable.OutTable is { } arg1 ? arg1 == arg2 : (bool?)null))
                    {
                        BarCodeLookupRet.HaveOut = true;
                        BarCodeLookupRet.IsOut = true;
                    }

                    if ((bool)(Convert.ToInt32(Enums.geTrackingOutType.otAlwaysIn) is var arg4 && oTable.OutTable is { } arg3 ? arg3 == arg4 : (bool?)null))
                    {
                        BarCodeLookupRet.HaveOut = true;
                        BarCodeLookupRet.IsOut = false;
                    }
                }

                BarCodeLookupRet.IsActive = string.IsNullOrEmpty(Strings.Trim(oTable.TrackingACTIVEFieldName));
                BarCodeLookupRet.EmailAddress = "";
                BarCodeLookupRet.DueBackDays = 0;

                if (!BarCodeLookupRet.IsActive)
                {
                    var tempActive = rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.TrackingACTIVEFieldName)];
                    if (!(tempActive is DBNull))
                    {
                        BarCodeLookupRet.IsActive = Convert.ToBoolean(tempActive);
                    }
                }
                if (Strings.Len(oTable.TrackingDueBackDaysFieldName) > 0)
                    BarCodeLookupRet.DueBackDays = Convert.ToInt32(rsADO.Fields[(DatabaseMap.RemoveTableNameFromField(oTable.TrackingDueBackDaysFieldName))].Value);
                if (BarCodeLookupRet.DueBackDays <= 0)
                    BarCodeLookupRet.DueBackDays = (int)iSystemQuery.OrderBy(m => m.Id).FirstOrDefault().DefaultDueBackDays;

                if (Strings.Len(oTable.TrackingEmailFieldName) > 0)
                {
                    BarCodeLookupRet.EmailAddress = rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.TrackingEmailFieldName)].ToString();

                    if (string.IsNullOrEmpty(BarCodeLookupRet.EmailAddress))
                    {
                        BarCodeLookupRet.EmailAddress = GetEmailAddressFromOperator(iTableQuery, iDatabaseQuery, rsADO, oTable);
                    }
                }
                else
                {
                    BarCodeLookupRet.EmailAddress = "<<Undefined>>";
                }
            }
            return BarCodeLookupRet;
        }

        public static ADODB.Recordset FindUserRecord(string sTableName, string sIdFieldName, string sIdFieldName2, string sActiveField, string sOutField, string sEmailAddress, string sDueBackDays, string sOperatorIdField, string sFindByFieldName, bool bFindFieldIsString, string sLookupStr, IQueryable<Table> iTableQuery, IQueryable<Databas> iDatabaseQuery)
        {
            string sSQL;
            string sError = string.Empty;

            // Build SQL Statement We only need the IdField and Active Fields
            sSQL = "SELECT [" + sIdFieldName.Replace(".", "].[");
            if (Strings.Len(sIdFieldName2) > 0)
                sSQL = sSQL + "], [" + sIdFieldName2.Replace(".", "].[");
            if (Strings.Len(sActiveField) > 0)
                sSQL = sSQL + "], [" + sActiveField.Replace(".", "].[");
            if (Strings.Len(sOutField) > 0)
                sSQL = sSQL + "], [" + sOutField.Replace(".", "].[");
            if (Strings.Len(sEmailAddress) > 0)
                sSQL = sSQL + "], [" + sEmailAddress.Replace(".", "].[");
            if (Strings.Len(Strings.Trim(sDueBackDays)) > 0)
                sSQL = sSQL + "], [" + sDueBackDays.Replace(".", "].[");
            if (Strings.Len(sOperatorIdField) > 0)
                sSQL = sSQL + "], [" + sOperatorIdField.Replace(".", "].[");

            sSQL = sSQL + "] FROM [" + sTableName + "] WHERE [" + DatabaseMap.RemoveTableNameFromField(sFindByFieldName) + "] = ";

            if (bFindFieldIsString)
            {
                sSQL = sSQL + "'" + Strings.Replace(sLookupStr, "'", "''") + "'";
            }
            else
            {
                sSQL = sSQL + sLookupStr;
            }
            var oTable = iTableQuery.Where(m => m.TableName.Trim().ToLower().Equals(sTableName.Trim().ToLower())).FirstOrDefault();
            // return the RecordSet
            var Cnxn = DataServices.DBOpen(oTable, iDatabaseQuery);
            // If (Not oTable Is Nothing) Then
            // Dim oDatabase = iDatabaseQuery.Where(Function(m) m.DBName.Trim.ToLower.Equals(oTable.DBName.Trim.ToLower)).FirstOrDefault
            // Cnxn = DataServices.DBOpen(oDatabase)
            // End If
            string arglError = "";
            return DataServices.GetADORecordSet(ref sSQL, Cnxn, lError: ref arglError);
            // Dim recordSet = DataServices.GetADORecordset(sSQL, oTable, _iDatabas.All(), Enums.CursorTypeEnum.rmOpenStatic, Enums.LockTypeEnum.rmLockReadOnly, Enums.CommandTypeEnum.rmCmdText, 0, lError:=lError, sErrorMessage:=sError)

        }

        public static TabSet GetTabset(Smead.Security.Passport passport, IQueryable<TableTab> iTableTabQuery, IQueryable<TabSet> iTabSetQuery, IQueryable<RelationShip> iRelationShipQuery, IQueryable<Table> iTableQuery, ref Table oTables, Collection cTabsetIds = null, bool bSecurityCheck = false)
        {
            TabSet GetTabsetRet = default;
            Collection cTabsets;
            int iCnt;
            int lCount;
            GetTabsetRet = null;
            bool @bool;
            var tableTabObj = iTableTabQuery.OrderBy(m => m.Id);
            if (cTabsetIds is not null)
            {
                cTabsets = cTabsetIds;
            }
            else
            {
                cTabsets = new Collection();
                var tabSetObj = iTabSetQuery.OrderBy(m => m.Id);
                var loopTo = tabSetObj.Count() - 1;
                for (lCount = 0; lCount <= loopTo; lCount++)
                    cTabsets.Add(null, tabSetObj.ElementAtOrDefault(lCount).Id.ToString());
            }
            var tabSetObj1 = iTabSetQuery.OrderBy(m => m.Id);
            var loopTo1 = cTabsets.Count - 1;
            for (iCnt = 0; iCnt <= loopTo1; iCnt++)
            {
                foreach (var oTableTabs in tableTabObj)
                {
                    if ((bool)(Strings.StrComp(oTableTabs.TableName, oTables.TableName, CompareMethod.Text) == 0 & (Convert.ToInt32(cTabsets[iCnt]) is var arg6 && oTableTabs.TabSet is { } arg5 ? arg5 == arg6 : null)))
                    {
                        GetTabsetRet = tabSetObj1.AsEnumerable().ElementAt(iCnt);
                        // GetTabset = DirectCast(_, Tabset)

                        if (GetTabsetRet is not null && bSecurityCheck)
                        {
                            if (passport is null)
                            {
                                @bool = passport.CheckPermission(GetTabsetRet.UserName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.WorkGroup, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                            }
                            else
                            {
                                @bool = passport.CheckPermission(GetTabsetRet.UserName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.WorkGroup, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                            }
                            if (@bool)
                            {
                                return GetTabsetRet;
                            }
                        }
                        else
                        {
                            return GetTabsetRet;
                        }
                    }
                }

                GetTabsetRet = GetParentTabSet(passport, ref oTables, 1, Convert.ToInt32(cTabsets[iCnt]), bSecurityCheck, iTableTabQuery, iTabSetQuery, iRelationShipQuery, iTableQuery);
                if (GetTabsetRet is not null)
                    break;
            }

            return GetTabsetRet;
        }

        public static TabSet GetParentTabSet(Smead.Security.Passport passport, ref Table oTables, int lLevel, int lTabsetId, bool bSecurityCheck, IQueryable<TableTab> iTableTabQuery, IQueryable<TabSet> iTabSetQuery, IQueryable<RelationShip> iRelationShipQuery, IQueryable<Table> iTableQuery)
        {
            Table oUpTables;
            TabSet oFindTabSets = default;

            if (lLevel > 100)
            {
                // A little recursive???
                return default;
            }
            var tableTabObj = iTableTabQuery.OrderBy(m => m.Id);
            var tabSetObj = iTabSetQuery.OrderBy(m => m.Id);
            var oTableDbName = oTables.TableName.Trim().ToLower();
            var oParentTableList = iRelationShipQuery.Where(m => m.LowerTableName.Trim().ToLower().Equals(oTableDbName));
            bool @bool;
            foreach (RelationShip oRelationships in oParentTableList)
            {
                if (Strings.StrComp(oRelationships.LowerTableName, oTables.TableName, CompareMethod.Text) == 0)
                {
                    foreach (TableTab oTableTabs in tableTabObj)
                    {
                        if (Strings.StrComp(oTableTabs.TableName, oRelationships.UpperTableName, CompareMethod.Text) == 0 & oTableTabs.TabSet == lTabsetId)
                        {
                            // oFindTabSets = DirectCast(tabSetObj(oTableTabs.TabSet.ToString), Tabset)
                            oFindTabSets = (TabSet)tabSetObj.Where(m => Operators.ConditionalCompareObjectEqual(m.Id, oTableTabs.TabSet, false));
                            if (oFindTabSets is not null && bSecurityCheck)
                            {
                                var wg = (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.WorkGroup;
                                var ac = (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access;

                                @bool = passport.CheckPermission(oFindTabSets.UserName, wg, ac);

                                //if (passport is null)
                                //{
                                //    @bool =passport.CheckPermission(oFindTabSets.UserName, wg, ac);
                                //}
                                //else
                                //{
                                //    @bool = passport.CheckPermission(oFindTabSets.UserName, wg, ac);
                                //}
                                if (@bool)
                                {
                                    oFindTabSets = default;
                                }
                            }
                            return oFindTabSets;
                        }
                    }
                }
                // Recursively Check upstream
                // oUpTables = DirectCast(moApp.Tables.Item(oRelationships.UpperTableName), Table)
                oUpTables = (Table)iTableQuery.Where(m => m.TableName.Trim().ToLower().Equals(oRelationships.UpperTableName.Trim().ToLower())).FirstOrDefault();
                if (oUpTables is not null)
                {
                    oFindTabSets = GetParentTabSet(passport, ref oUpTables, lLevel + 1, lTabsetId, bSecurityCheck, iTableTabQuery, iTabSetQuery, iRelationShipQuery, iTableQuery);

                    if (oFindTabSets is not null && bSecurityCheck)
                    {
                        Boolean boole;
                        var wg = (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.WorkGroup;
                        var ac = (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access;

                        if (passport is null)
                        {

                            boole = passport.CheckPermission(oFindTabSets.UserName, wg, ac);
                        }
                        else
                        {
                            boole = passport.CheckPermission(oFindTabSets.UserName, wg, ac);
                        }
                        if (boole)
                        {
                            return oFindTabSets;
                        }
                    }
                    else
                    {
                        return oFindTabSets;
                    }
                }
            }

            return oFindTabSets;
        }

        public static string GetUserDisplayString(IQueryable<Databas> iDatabaseQuery, 
            IQueryable<TabFusionRMS.Models.System> iSystemQuery, Table oTable,
            string sLookupStr,
            [Optional, DefaultParameterValue(null)] ref ADODB.Recordset rsADO,
            [Optional, DefaultParameterValue(false)] ref bool bEOF, 
            [Optional] ref string sDescriptionOnly,
            [Optional, DefaultParameterValue(-1)] ref int lDueBackDays,
            [Optional, DefaultParameterValue(false)] bool bCompleteDescription)
        {
            string GetUserDisplayStringRet = default;
            var bHadADO = default(bool);

            sDescriptionOnly = string.Empty;
            GetUserDisplayStringRet = string.Empty;
            if (rsADO is not null)
                bHadADO = true;
            var sADOCon = DataServices.DBOpen(oTable, iDatabaseQuery);

            bool IfIdFieldIsString = DataServices.IdFieldIsString(sADOCon, oTable.TableName, oTable.IdFieldName);
            if (!IfIdFieldIsString & !Information.IsNumeric(sLookupStr))
                sLookupStr = "0";

            if (Strings.Len(Strings.Trim(sLookupStr)) == 0)
            {
                if (lDueBackDays != -1)
                    lDueBackDays = (int)iSystemQuery.OrderBy(m => m.Id).FirstOrDefault().DefaultDueBackDays;
            }
            else
            {
                if (Strings.Len(oTable.DescFieldNameOne) > 0)
                {
                    string sSQL = "SELECT [" + DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameOne) + "]";
                    // Build SQL Statement; we only need the description fields
                    if (Strings.Len(oTable.DescFieldNameTwo) > 0)
                        sSQL += ", [" + DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo) + "]";
                    if (Strings.Len(oTable.TrackingDueBackDaysFieldName) > 0 & lDueBackDays != -1)
                        sSQL += ", [" + DatabaseMap.RemoveTableNameFromField(oTable.TrackingDueBackDaysFieldName) + "]";
                    sSQL += string.Format(" FROM [{0}] WHERE [{1}] = {2}", oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), DataServices.IDQueryValue(IfIdFieldIsString, sLookupStr));

                    if (!bHadADO)
                    {
                        string arglError = "";
                        rsADO = DataServices.GetADORecordSet(ref sSQL, sADOCon, lError: ref arglError);
                        // Create RecordSet
                        // rsADO = DataServices.GetADORecordset(sSQL, oTable, _iDatabas.All(), Enums.CursorTypeEnum.rmOpenStatic, Enums.LockTypeEnum.rmLockReadOnly, Enums.CommandTypeEnum.rmCmdText, 0)
                    }

                    if (lDueBackDays != -1)
                        lDueBackDays = GetDueBackDays(iDatabaseQuery, iSystemQuery, ref oTable, sLookupStr, ref rsADO);

                    if (rsADO.EOF)
                    {
                        if (!bHadADO)
                        {
                            rsADO.Close();
                            rsADO = null;
                        }

                        bEOF = true;
                        return StripLeadingZeros(sLookupStr);
                    }

                    if (Strings.Len(Strings.Trim(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameOne)].ToString())) > 0)
                    {
                        if (Strings.Len(oTable.DescFieldPrefixOne) > 0)
                            GetUserDisplayStringRet += Strings.Trim(oTable.DescFieldPrefixOne) + " ";
                        GetUserDisplayStringRet += Strings.Trim(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameOne)].ToString());

                        if (Strings.StrComp(DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameOne), DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), Constants.vbTextCompare) != 0)
                        {
                            sDescriptionOnly += Strings.Trim(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameOne)].ToString());
                        }
                        else if (Strings.StrComp(DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo), DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), Constants.vbTextCompare) != 0 & bCompleteDescription)
                        {
                            sDescriptionOnly += Strings.Trim(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameOne)].ToString());
                        }
                    }

                    if (Strings.Len(oTable.DescFieldNameTwo) > 0)
                    {
                        if (Strings.Len(Strings.Trim(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo)].ToString())) > 0)
                        {
                            if (Strings.Len(oTable.DescFieldPrefixTwo) > 0)
                                GetUserDisplayStringRet += " " + Strings.Trim(oTable.DescFieldPrefixTwo);
                            GetUserDisplayStringRet += " " + Strings.Trim(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo)].ToString());

                            // If (StrComp(RemoveTableNameFromField(oTable.DescFieldNameTwo), RemoveTableNameFromField(oTable.IdFieldName), CompareMethod.Text) <> 0) Then
                            if (Strings.StrComp(DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo), DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), Constants.vbTextCompare) != 0 | bCompleteDescription)
                            {
                                if (Strings.Len(sDescriptionOnly) > 0)
                                {
                                    if (Strings.Len(oTable.DescFieldPrefixTwo) > 0)
                                    {
                                        sDescriptionOnly += " " + Strings.Trim(oTable.DescFieldPrefixTwo) + " " + Strings.Trim(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo)].ToString());
                                    }
                                    else
                                    {
                                        sDescriptionOnly += " - " + Strings.Trim(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo)].ToString());
                                    }
                                }
                                else
                                {
                                    sDescriptionOnly = Strings.Trim(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo)].ToString());
                                }
                            }
                        }
                    }
                }
                else if (lDueBackDays != -1)
                {
                    ADODB.Recordset argrsADO = null;
                    lDueBackDays = GetDueBackDays(iDatabaseQuery, iSystemQuery, ref oTable, sLookupStr, rsADO: ref argrsADO);
                }

                if (string.IsNullOrEmpty(GetUserDisplayStringRet))
                    GetUserDisplayStringRet = StripLeadingZeros(sLookupStr);
            }

            if (!bHadADO && rsADO is not null)
            {
                rsADO.Close();
                rsADO = null;
            }

            return GetUserDisplayStringRet;

            // Exit Function

            // If (lDueBackDays <> -1) Then lDueBackDays = iSystemQuery.OrderBy(Function(m) m.Id).FirstOrDefault.DefaultDueBackDays
            // GetUserDisplayString = StripLeadingZeros(sLookupStr)
            // 'AppMsgBox("Error - Attempting to Find RecordSet (LookupServices.GetUserDisplayString)[" & CStr(sLookupStr) & "]: " & CStr(Err.Number) & " " & Err.Description & "", MsgBoxStyle.Critical, System.Reflection.Assembly.GetExecutingAssembly.GetName.Name)

            // If (Not bHadADO AndAlso rsADO IsNot Nothing) Then
            // rsADO.Close()
            // rsADO = Nothing
            // End If
        }

        public static string StripLeadingZeros(string stripThis)
        {
            if (string.IsNullOrEmpty(stripThis))
                return string.Empty;
            if (!Information.IsNumeric(stripThis))
                return stripThis;

            while (stripThis.Trim().Length > 0)
            {
                if (string.Compare(stripThis.Substring(0, 1), "0") != 0)
                    return stripThis.Trim();
                stripThis = stripThis.Substring(1);
            }

            return stripThis.Trim();
        }

        public static string NormalizeString(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            s = s.Replace(Constants.vbTab, " ");
            s = s.Replace(Constants.vbCr, " ");
            s = s.Replace(Constants.vbLf, " ");

            while (s.Contains("  "))
                s = s.Replace("  ", " ");

            return s;
        }

        public static int GetDueBackDays(IQueryable<Databas> iDatabaseQuery, IQueryable<TabFusionRMS.Models.System> iSystemQuery, ref Table oTable, string sLookupStr, [Optional, DefaultParameterValue(null)] ref ADODB.Recordset rsADO)
        {
            int lDueBackDays = -1;
            string sFieldName;
            ADODB.Connection sADOCon;
            if (oTable is not null && Strings.Len(oTable.TrackingDueBackDaysFieldName) > 0)
            {
                sFieldName = DatabaseMap.RemoveTableNameFromField(oTable.TrackingDueBackDaysFieldName);

                if (rsADO is not null)
                {
                    if (Strings.Len(sFieldName) > 0 && !rsADO.EOF)
                        lDueBackDays = Convert.ToInt32(rsADO.Fields[sFieldName].Value);
                }
                else if (Strings.Len(Strings.Trim(sLookupStr)) > 0 & Strings.Len(sFieldName) > 0)
                {
                    string oTableDbName = oTable.DBName.Trim().ToLower();
                    sADOCon = DataServices.DBOpen(oTable, iDatabaseQuery);
                    bool IfIdFieldIsString = DataServices.IdFieldIsString(sADOCon, oTable.TableName, oTable.IdFieldName);
                    string sSQL = string.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = {3}", sFieldName, oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), DataServices.IDQueryValue(IfIdFieldIsString, sLookupStr));
                    // rsADO = DataServices.GetADORecordset(sSQL, oTable, _iDatabas.All(), Enums.CursorTypeEnum.rmOpenStatic, Enums.LockTypeEnum.rmLockReadOnly, Enums.CommandTypeEnum.rmCmdText)
                    string arglError = "";
                    rsADO = DataServices.GetADORecordSet(ref sSQL, sADOCon, lError: ref arglError);
                    if (rsADO is not null)
                    {
                        if (!rsADO.EOF)
                            lDueBackDays = Convert.ToInt32(rsADO.Fields[sFieldName].Value);
                        rsADO.Close();
                    }
                }
            }
            if (lDueBackDays <= 0)
                lDueBackDays = (int)iSystemQuery.OrderBy(m => m.Id).FirstOrDefault().DefaultDueBackDays;
            return lDueBackDays;
        }

        public static int VerifyViewSecurity(Smead.Security.Passport passport,
                IQueryable<View> iViewQuery, IQueryable<Databas> iDatabaseQuery,
                Table oTable, string sLookup, string sFindByFieldName,
                bool bFindFieldIsString, bool bSecurityCheck = true)
        {
            int iPos;
            string sFrom;
            string sSQL;
            string sTemp;
            ADODB.Recordset rsADOCount;
            ADODB.Connection sADOCon;
            bool boolVal;
            if (!bSecurityCheck)
            {
                // If we're not doing a security check then we don't care what the View ID is. It just can't be zero.
                return 1;
            }

            sFindByFieldName = "[" + oTable.TableName + "].[" + DatabaseMap.RemoveTableNameFromField(sFindByFieldName) + "]";
            if (passport is null)
            {
                boolVal = passport.CheckPermission(oTable.TableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.Table, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.View);
            }
            else
            {
                boolVal = passport.CheckPermission(oTable.TableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.Table, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.View);
            }
            if (boolVal)
            {
                var lLoopViewEntities = iViewQuery.Where(x => x.TableName.Trim().ToLower() == oTable.TableName.Trim().ToLower() && x.ViewType == (int)Enums.geViewType.vtMaster);
                foreach (var oView in lLoopViewEntities)
                {
                    sTemp = TrackingServices.NormalizeString(oView.SQLStatement);
                    iPos = Strings.InStr(1, sTemp, " FROM ", CompareMethod.Text);

                    if (iPos > 0)
                    {
                        sFrom = Strings.Mid(sTemp, iPos + 6);
                    }
                    else
                    {
                        sFrom = sTemp;
                    }

                    iPos = Strings.InStr(1, sFrom, " WHERE ", CompareMethod.Text);

                    if (iPos > 0)
                    {
                        sTemp = " WHERE ((" + Strings.Mid(sFrom, iPos + 7) + ") AND (" + sFindByFieldName + " = ";
                        sFrom = sFrom.Substring(0, iPos);
                    }
                    else
                    {
                        sTemp = " WHERE " + sFindByFieldName + " = ";
                        sFrom = "[" + oTable.TableName + "]";
                    }

                    if (bFindFieldIsString)
                    {
                        sSQL = "SELECT COUNT(" + sFindByFieldName + ") AS RCount FROM " + sFrom + " " + sTemp + "'" + Strings.Replace(sLookup, "'", "''") + "'";
                    }
                    else
                    {
                        sSQL = "SELECT COUNT(" + sFindByFieldName + ") AS RCount FROM " + sFrom + " " + sTemp + sLookup;
                    }

                    if (iPos > 0)
                    {
                        sSQL = sSQL + "))";
                    }
                    var DBEngineObj = new DatabaseEngine();
                    sSQL = "";
                    sSQL = Strings.Replace(sSQL, string.Format("'{0}'", SL_USERNAME), string.Format("'{0}'", DBEngineObj.DBUserId.Replace("'", "''")), Compare: CompareMethod.Text);
                    // If (Not oTable Is Nothing) Then
                    // Dim DbObj = iDatabaseQuery.Where(Function(m) m.DBName.Trim().ToLower().Equals(oTable.DBName.Trim.ToLower)).FirstOrDefault
                    sADOCon = DataServices.DBOpen(oTable, iDatabaseQuery);
                    // End If

                    string arglError = "";
                    rsADOCount = DataServices.GetADORecordSet(ref sSQL, (ADODB.Connection)oTable, lError: ref arglError);
                    // rsADOCount = DataServices.GetADORecordset(sSQL, oTable, Enums.CursorTypeEnum.rmOpenStatic, Enums.LockTypeEnum.rmLockReadOnly, , , , True, iPos, sTemp)
                    // rsADOCount = DataServices.GetADORecordset(sSQL, oTable, _iDatabas.All(), Enums.CursorTypeEnum.rmOpenStatic, Enums.LockTypeEnum.rmLockReadOnly, , , , iPos, sTemp)
                    if (rsADOCount is not null)
                    {
                        if (!rsADOCount.EOF)
                        {
                            if (Convert.ToInt32(rsADOCount.Fields["RCount"].Value) > 0)
                            {
                                rsADOCount.Close();
                                rsADOCount = null;
                                return oView.Id;
                            }
                        }

                        rsADOCount.Close();
                        rsADOCount = null;
                    }
                }
            }
            return 0;
        }

        public static string GetEmailAddressFromOperator(IQueryable<Table> iTableQuery, IQueryable<Databas> iDatabaseQuery, ADODB.Recordset rsADO, Table oTable)
        {
            if (string.IsNullOrEmpty(oTable.OperatorsIdField))
                return string.Empty;
            if (string.IsNullOrEmpty(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.OperatorsIdField)].ToString()))
                return string.Empty;
            var tableObj = iTableQuery.Where(m => m.TableName.Equals("SecureUser")).FirstOrDefault();
            ADODB.Connection sADOCon;
            string accountType = "A";
            // If moApp.oSDLKini.UsingActiveDirectoryCached Then accountType = "S"

            string sql = string.Format("SELECT TOP 1 Email FROM SecureUser WHERE UserName = '{0}' AND AccountType <> '{1}' AND AccountType IS NOT NULL", rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oTable.OperatorsIdField)].ToString().Replace("'", "''"), accountType);
            // If (Not oTable Is Nothing) Then
            // Dim DbObj = iDatabaseQuery.Where(Function(m) m.DBName.Trim().ToLower().Equals(oTable.DBName.Trim.ToLower)).FirstOrDefault
            sADOCon = DataServices.DBOpen(oTable, iDatabaseQuery);
            // End If
            string arglError = "";
            var rsUser = DataServices.GetADORecordSet(ref sql, sADOCon, lError: ref arglError);

            // Dim rsUser As ADODB.Recordset = DataServices.GetADORecordset(sql, tableObj, _iDatabas.All(), Enums.CursorTypeEnum.rmOpenForwardOnly, Enums.LockTypeEnum.rmLockReadOnly, , , True)
            if (rsUser is null)
                return string.Empty;

            try
            {
                if (!rsUser.EOF)
                    return rsUser.Fields[0].ToString();
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            finally
            {
                rsUser.Close();
                rsUser = null;
            }
        }

        public static bool DeleteExtraTrackingHistory(IQueryable<TabFusionRMS.Models.System> iSystemQuery, Repositories<TrackingHistory> _iTrackingHistory, string sTableName, string sId, [Optional, DefaultParameterValue("")] ref string KeysType)
        {
            try
            {
                _iTrackingHistory.BeginTransaction();
                var pSystem = iSystemQuery.OrderBy(m => m.Id).FirstOrDefault();
                int pSystem1 = Convert.ToInt32(pSystem.MaxHistoryItems);
                var trackHistory = _iTrackingHistory.All();
                int totalVra = _iTrackingHistory.All().Count();
                // Dim sSqlExtra = (From tMain In trackHistory
                // Where tMain.TrackedTable = sTableName.Trim() And tMain.TrackedTableId.Trim().ToLower() = sId.Trim().ToLower() AndAlso
                // Not (From tSub In trackHistory
                // Where tSub.TrackedTable.Trim().ToLower() = sTableName.Trim().ToLower() And tSub.TrackedTableId.Trim().ToLower() = sId.Trim().ToLower()
                // Order By tSub.TransactionDateTime Descending
                // Select tSub.Id Take pSystem.MaxHistoryItems).Contains(tMain.Id))
                var sSqlExtra = from tMain in trackHistory
                                where (tMain.TrackedTable ?? "") == (sTableName.Trim() ?? "") & (tMain.TrackedTableId.Trim().ToLower() ?? "") == (sId.Trim().ToLower() ?? "") && !(from tSub in trackHistory
                                                                                                                                                                                   where (tSub.TrackedTable.Trim().ToLower() ?? "") == (sTableName.Trim().ToLower() ?? "") & (tSub.TrackedTableId.Trim().ToLower() ?? "") == (sId.Trim().ToLower() ?? "")
                                                                                                                                                                                   orderby tSub.TransactionDateTime descending
                                                                                                                                                                                   select tSub.Id).Take(pSystem1).Contains(tMain.Id)
                                select tMain;
                for (int index = 1; index <= 2; index++)
                {
                    var sSqlTotal = (from tMain in trackHistory
                                     where (tMain.TrackedTable ?? "") == (sTableName ?? "") && (tMain.TrackedTableId ?? "") == (sId ?? "")
                                     group tMain by new { tMain.TrackedTableId, tMain.TrackedTable } into tGroup
                                     let groupName = tGroup.Key
                                     let TotalCount = tGroup.Count()
                                     select new { TotalCount }).ToList();

                    if (sSqlTotal is not null)
                    {
                        if (!(sSqlTotal.Count == 0))
                        {
                            foreach (var totalVar in sSqlTotal)
                            {
                                if ((bool)(totalVar.TotalCount is var arg15 && pSystem.MaxHistoryItems is { } arg16 ? arg15 >= arg16 : (bool?)null))
                                {
                                    _iTrackingHistory.DeleteRange(sSqlExtra);
                                    // For Each record In sSqlExtra
                                    // _iTrackingHistory.Delete(record)
                                    // Next
                                }
                            }
                            KeysType = "s";
                        }
                        else
                        {
                            KeysType = "w";
                        }
                    }
                }
                _iTrackingHistory.CommitTransaction();
                return true;
            }
            catch (Exception)
            {
                _iTrackingHistory.RollBackTransaction();
                return false;
            }
        }

        public static bool InnerTruncateTrackingHistory(IQueryable<TabFusionRMS.Models.System> iSystemQuery, IRepository<TrackingHistory> _iTrackingHistory, string sTableName, [Optional, DefaultParameterValue("")] string sId , [Optional, DefaultParameterValue("")] ref string KeysType)
        {
            bool returnFlag = true;
            try
            {
                var pSystem = iSystemQuery.OrderBy(m => m.Id).FirstOrDefault();
                if ((bool)(0 is var arg18 && pSystem.MaxHistoryDays is { } arg17 ? arg17 > arg18 : (bool?)null))
                {
                    var dMaxDate = DateTime.FromOADate((double)(DateTime.Now.ToOADate() - pSystem.MaxHistoryDays - 1));
                    var dUTC = dMaxDate.ToUniversalTime();
                    if (Operators.CompareString(sTableName, "", false) > 0)
                    {
                        // Dim pTrackingHistory = _iTrackingHistory.All.Where(Function(m) m.TransactionDateTime < dUTC AndAlso (m.TrackedTable.Trim().ToLower().Equals(sTableName.Trim().ToLower()) AndAlso m.TrackedTableId.Trim().ToLower().Equals(sId.Trim.ToLower())))
                        var pTrackingHistory = _iTrackingHistory.All().Where(m => m.TransactionDateTime < dUTC && (m.TrackedTable.Trim().ToLower().Equals(sTableName.Trim().ToLower()) && m.TrackedTableId.Trim().ToLower().Equals(sId.Trim().ToLower())));
                        if (pTrackingHistory.Count() == 0)
                        {
                            KeysType = "w";
                        }
                        else
                        {
                            _iTrackingHistory.DeleteRange(pTrackingHistory);
                            // For Each entity As TrackingHistory In pTrackingHistory.ToList()
                            // _iTrackingHistory.Delete(entity)
                            // Next
                            KeysType = "s";
                        }
                    }
                    else
                    {
                        var pTrackingHistory = _iTrackingHistory.All().Where(m => m.TransactionDateTime < dUTC).Take(100);

                        if (pTrackingHistory.Count() != 0)
                        {
                            _iTrackingHistory.DeleteRange(pTrackingHistory);
                            // For Each entity As TrackingHistory In pTrackingHistory.ToList()
                            // _iTrackingHistory.Delete(entity)
                            // Next
                            KeysType = "s";
                        }
                        else
                        {
                            KeysType = "w";
                        }
                    }
                }

                if ((bool)(0 is var arg26 && pSystem.MaxHistoryItems is { } arg25 ? arg25 > arg26 : (bool?)null))
                {
                    if (string.IsNullOrEmpty(sTableName))
                    {
                        var trackHistory = _iTrackingHistory.All();
                        var sSQL = (from tq in trackHistory
                                    group tq by new { tq.TrackedTable, tq.TrackedTableId } into tGroup
                                    let groupName = tGroup.Key
                                    let TableIdCount = tGroup.Count()
                                    orderby groupName.TrackedTableId, groupName.TrackedTable descending
                                    select new { TableIdCount, groupName.TrackedTable, groupName.TrackedTableId }).ToList();
                        if (sSQL is not null)
                        {
                            foreach (var Tracking in sSQL)
                            {
                                if ((bool)(Tracking.TableIdCount is var arg27 && pSystem.MaxHistoryItems is { } arg28 ? arg27 < arg28 : (bool?)null))
                                {
                                }
                                // KeysType = "w"
                                else
                                {
                                    returnFlag = DeleteExtraTrackingHistory(iSystemQuery, (Repositories<TrackingHistory>)_iTrackingHistory, Tracking.TrackedTable, Tracking.TrackedTableId, ref KeysType);
                                }
                            }
                        }
                    }
                    else
                    {
                        returnFlag = DeleteExtraTrackingHistory(iSystemQuery, (Repositories<TrackingHistory>)_iTrackingHistory, sTableName, sId, ref KeysType);
                    }
                }
                return returnFlag;
            }
            catch (Exception)
            {
                returnFlag = false;
                return returnFlag;
            }
        }

        public static string BuildTrackingLocationSQL(IQueryable<Table> itableQuery, IQueryable<Databas> idatabaseQuery, string sCurrentSQL, ref Table oTables)
        {
            string BuildTrackingLocationSQLRet = default;
            int iFromPos = 0;
            int iWherePos = 0;
            Tables oTmpTables = null;
            List<Table> cUsedTables;
            SchemaColumns oSchemaColumns = null;
            string sPreFrom = string.Empty;
            string sFrom = string.Empty;
            string sWhere = string.Empty;
            string sReturnSQL = string.Empty;
            string sNewFields = string.Empty;
            string sJoinTables = string.Empty;

            sCurrentSQL = DataServices.NormalizeString(sCurrentSQL);
            BuildTrackingLocationSQLRet = sCurrentSQL;

            if (oTables is null)
            {
                return BuildTrackingLocationSQLRet;
            }

            sWhere = "";
            iFromPos = Strings.InStr(1, sCurrentSQL, " FROM ", CompareMethod.Text);

            if (iFromPos > 0)
            {
                sPreFrom = Strings.Left(sCurrentSQL, iFromPos - 1);
                sFrom = Strings.Mid(sCurrentSQL, iFromPos, Strings.Len(sCurrentSQL));

                iWherePos = Strings.InStr(1, sFrom, " WHERE ", CompareMethod.Text);
                if (iWherePos > 0)
                {
                    sWhere = Strings.Mid(sFrom, iWherePos, Strings.Len(sCurrentSQL));
                    sFrom = Strings.Left(sFrom, iWherePos - 1);
                }

                if (!ValidateFromOneTable(ref sFrom))
                {
                    // Sorry, current SQL is too complex for Me!
                    return BuildTrackingLocationSQLRet;
                }
            }
            else
            {
                // no FROM in current SQL?? Bail out!
                return BuildTrackingLocationSQLRet;
            }
            // Set up a collection of tables objects that represent the actual
            // tracking tables (containers) that are visible from this view table
            cUsedTables = ViewModel.GetUsedTrackingTables(oTables, itableQuery.Where(m => m.TrackingTable > 0));
            // cUsedTables = GetUsedTrackingTables(oTables)

            if (cUsedTables.Count > 0)
            {
                if (!ViewModel.CreateCoalesceFields(cUsedTables, idatabaseQuery, ref sNewFields, false))
                    return BuildTrackingLocationSQLRet;
                // If (Not CreateCoalesceFields(cUsedTables, sNewFields)) Then Exit Function
                if (!CreateJoinTables(ref cUsedTables, ref sJoinTables))
                    return BuildTrackingLocationSQLRet;

                sReturnSQL = FixupFieldNames(sPreFrom, oTables.TableName) + ", " + sNewFields;
                sReturnSQL += " FROM " + new string('(', cUsedTables.Count + 1) + "[" + oTables.TableName + "]";
                sReturnSQL += " LEFT JOIN TrackingStatus ON ((TrackingStatus.TrackedTable = '" + oTables.TableName + "') AND ";
                sReturnSQL += "([" + oTables.TableName + "].[" + DatabaseMap.RemoveTableNameFromField(oTables.IdFieldName) + "] = TrackingStatus.TrackedTableId))) ";
                sReturnSQL += sJoinTables;

                if (!string.IsNullOrEmpty(Strings.Trim(sWhere)))
                    sReturnSQL += sWhere;
                BuildTrackingLocationSQLRet = sReturnSQL;
            }

            cUsedTables = null;
            return BuildTrackingLocationSQLRet;
        }

        public static string FixupFieldNames(string sSelect, string sTableName)
        {
            string FixupFieldNamesRet = default;
            bool bSimpleField = false;
            bool bBogus = false;
            int iSelectPos = 0;
            int iCurLoc = 0;
            string sFields = string.Empty;
            string sField = string.Empty;
            string sResult = string.Empty;
            FixupFieldNamesRet = sSelect;
            sResult = "";

            iSelectPos = Strings.InStr(1, sSelect, "SELECT ", CompareMethod.Text);

            if (iSelectPos == 1)
            {
                sFields = Strings.Trim(Strings.Mid(sSelect, Strings.Len("SELECT ") + 1, Strings.Len(sSelect)));
                iCurLoc = 1;

                while (FindNextFieldName(sFields, ref sField, ref bSimpleField, ref bBogus, ref iCurLoc) == true)
                {
                    if (bBogus)
                    {
                        // ahhhh crap - now what?
                        return FixupFieldNamesRet;
                    }
                    // deal with complex field types
                    if (bSimpleField)
                    {
                        if (Strings.InStr(1, sField, ".", CompareMethod.Text) > 0)
                        {
                            sResult = sResult + sField + ", ";
                        }
                        else if (string.Compare(sField, "*") == 0)
                        {
                            sResult = sResult + "[" + sTableName + "]." + sField + ", ";
                        }
                        else
                        {
                            sResult = sResult + "[" + sTableName + "].[" + sField + "], ";
                        }
                    }
                    else
                    {
                        sResult = sResult + sField + ", ";
                    }
                }
            }

            sResult = Strings.Left(sResult, Strings.Len(sResult) - 2);
            FixupFieldNamesRet = "SELECT " + sResult;

            return FixupFieldNamesRet;
        }

        public static bool FindNextFieldName(string sFields, ref string sField, ref bool bSimpleField, ref bool bBogus, ref int iCurLoc)
        {
            bool FindNextFieldNameRet = default;
            int iIndex;
            int iLen;
            int iParCount;
            string sChar;
            bool bDone;
            bool bInQuote;

            FindNextFieldNameRet = true;

            sFields = sFields + ",";
            iLen = Strings.Len(sFields);
            iIndex = iCurLoc;
            bDone = false;
            bInQuote = false;
            iParCount = 0;
            bBogus = false;
            bSimpleField = true;
            sField = "";

            if (iCurLoc > iLen)
            {
                FindNextFieldNameRet = false;
                return FindNextFieldNameRet;
            }

            while (!bDone & iIndex <= iLen)
            {
                sChar = Strings.Mid(sFields, iIndex, 1);

                if (sChar == "'")
                {
                    bInQuote = !bInQuote;
                    sField = sField + sChar;
                    bSimpleField = false;
                }
                else if (sChar == "(")
                {
                    iParCount = iParCount + 1;
                    sField = sField + sChar;
                    bSimpleField = false;
                }
                else if (sChar == ")")
                {
                    iParCount = iParCount - 1;
                    sField = sField + sChar;
                    bSimpleField = false;
                }
                else if (sChar == ",")
                {
                    if (!bInQuote & iParCount == 0)
                    {
                        bDone = true;
                    }
                    else
                    {
                        sField = sField + sChar;
                        bSimpleField = false;
                    }
                }
                else
                {
                    sField = sField + sChar;
                }

                iIndex = iIndex + 1;
            }

            iCurLoc = iIndex;
            if (!bDone)
                bBogus = true;
            sField = Strings.Trim(sField);
            return FindNextFieldNameRet;
        }

        public static bool CreateJoinTables(ref List<Table> oTables, ref string sJoins)
        {
            bool CreateJoinTablesRet = default;
            CreateJoinTablesRet = false;
            sJoins = "";

            foreach (var oTmpTables in oTables)
                sJoins = sJoins + " LEFT JOIN [" + oTmpTables.TableName + "] ON (TrackingStatus." + oTmpTables.TrackingStatusFieldName + " = [" + oTmpTables.TableName + "].[" + DatabaseMap.RemoveTableNameFromField(oTmpTables.IdFieldName) + "]))";

            if (!string.IsNullOrEmpty(Strings.Trim(sJoins)))
            {
                CreateJoinTablesRet = true;
            }
            return CreateJoinTablesRet;
        }

        public static bool ValidateFromOneTable(ref string sFrom)
        {
            bool ValidateFromOneTableRet = default;
            ValidateFromOneTableRet = true;

            sFrom = Strings.Trim(Strings.Mid(sFrom, Strings.Len(" FROM ") + 1));

            if (Strings.InStr(1, sFrom, ",", CompareMethod.Text) > 0)
            {
                ValidateFromOneTableRet = false;
            }

            if (Strings.InStr(1, sFrom, " ", CompareMethod.Text) > 0)
            {
                ValidateFromOneTableRet = false;
            }

            if (Strings.InStr(1, sFrom, " JOIN ", CompareMethod.Text) > 0)
            {
                ValidateFromOneTableRet = false;
            }

            return ValidateFromOneTableRet;
        }

        public static string ValidateRetention(Smead.Security.Passport passport, IQueryable<Databas> iDatabaseQuery, ref Table oTable, ref string sId)
        {
          
            var sADOCon = DataServices.DBOpen(oTable, iDatabaseQuery);
            if (oTable is not null & Strings.Len(sId) > 0)
            {
                if (TrackingServices.IsInactiveRecord(oTable, sId, sADOCon))
                {
                    bool @bool;
                    if (passport is null)
                    {
                        @bool = passport.CheckPermission(Common.SECURE_RETENTION_INACTIVE, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Retention, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                    }
                    else
                    {
                        @bool = passport.CheckPermission(Common.SECURE_RETENTION_INACTIVE, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Retention, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                    }
                    if (!@bool)
                    {
                        return Languages.get_Translation("msgImportCtrlTrackingObjectNotExist");
                    }
                }
                if (TrackingServices.IsArchivedOrDestroyed(oTable, sId, Enums.meFinalDispositionStatusType.fdstArchived, sADOCon))
                {
                    bool @bool;
                    if (passport is null)
                    {
                        @bool = passport.CheckPermission(Common.SECURE_RETENTION_ARCHIVE, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Retention, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                    }
                    else
                    {
                        @bool = passport.CheckPermission(Common.SECURE_RETENTION_ARCHIVE, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Retention, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                    }
                    if (!@bool)
                    {
                        return Languages.get_Translation("msgImportCtrlTrackingObjectNotExist");
                    }
                }
                if (TrackingServices.IsArchivedOrDestroyed(oTable, sId, Enums.meFinalDispositionStatusType.fdstDestroyed, sADOCon))
                {
                    bool @bool;
                    if (passport is null)
                    {
                        @bool = passport.CheckPermission(Common.SECURE_RETENTION_DESTROY, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Retention, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                    }
                    else
                    {
                        @bool = passport.CheckPermission(Common.SECURE_RETENTION_DESTROY, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Retention, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                    }
                    if (!@bool)
                    {
                        return Languages.get_Translation("msgImportCtrlTrackingObjectNotExist");
                    }
                }
            }

            return string.Empty;
        }

        public static bool IsInactiveRecord(Table oTable, string sId, ADODB.Connection sADOCon)
        {
            bool IfIdFieldIsString = DataServices.IdFieldIsString(sADOCon, oTable.TableName, oTable.IdFieldName);
            if (!string.IsNullOrEmpty(oTable.RetentionFieldName) && oTable.RetentionInactivityActive.GetValueOrDefault())
            {
                string sSQL = string.Format("SELECT [%slRetentionInactiveFinal] FROM [{0}] WHERE [{1}] = {2}", oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), DataServices.IDQueryValue(IfIdFieldIsString, sId));

                string arglError = "";
                var rs = DataServices.GetADORecordSet(ref sSQL, sADOCon, lError: ref arglError);

                try
                {
                    if (rs is not null && !rs.EOF)
                        return Convert.ToBoolean(rs.Fields[0]);
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    if (rs is not null)
                        rs.Close();
                }
            }

            return false;
        }

        public static bool IsArchivedOrDestroyed(Table oTable, string sId, Enums.meFinalDispositionStatusType eCompareType, ADODB.Connection sADOCon)
        {
            bool rtn = false;
            bool IfIdFieldIsString = DataServices.IdFieldIsString(sADOCon, oTable.TableName, oTable.IdFieldName);
            if (!string.IsNullOrEmpty(oTable.RetentionFieldName) && oTable.RetentionPeriodActive.GetValueOrDefault())
            {
                Enums.meFinalDispositionStatusType eStatusType;
                string sSQL = string.Format("SELECT [%slRetentionDispositionStatus] FROM [{0}] WHERE [{1}] = {2}", oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), DataServices.IDQueryValue(IfIdFieldIsString, sId));

                string arglError = "";
                var rs = DataServices.GetADORecordSet(ref sSQL, sADOCon, lError: ref arglError);

                if (rs is not null)
                {
                    if (!rs.EOF)
                    {
                        try
                        {
                            eStatusType = (Enums.meFinalDispositionStatusType)rs.Fields[0].Value;
                            switch (eStatusType)
                            {
                                case Enums.meFinalDispositionStatusType.fdstArchived:
                                case Enums.meFinalDispositionStatusType.fdstDestroyed:
                                    {
                                        rtn = (eStatusType & eCompareType) == eStatusType;
                                        break;
                                    }
                            }
                        }
                        catch (Exception)
                        {
                            rtn = false;
                        }
                    }

                    rs.Close();
                }
            }

            return rtn;
        }

        public static string ZeroPaddedString(Table oTable, object oId, IQueryable<Databas> iDatabaseQuery)
        {
            if (oId is null || oId.ToString().Length == 0)
            {
                return "";
            }
            else if (oTable is null)
            {
                return ZeroPaddedString(oId);
            }
            else
            {
                var sADOCon = DataServices.DBOpen(oTable, iDatabaseQuery);
                // If (Not oTable Is Nothing) Then
                // If (Not String.IsNullOrEmpty(oTable.DBName)) Then
                // Dim oDatabase = iDatabaseQuery.Where(Function(m) m.DBName.Trim.ToLower.Equals(oTable.DBName.Trim.ToLower)).FirstOrDefault
                // sADOCon = DataServices.DBOpen(oDatabase)
                // End If
                // End If
                bool IfIdFieldIsString = DataServices.IdFieldIsString(sADOCon, oTable.TableName, oTable.IdFieldName);
                if (!IfIdFieldIsString)
                {
                    return ZeroPaddedString(oId);
                }
                else
                {
                    return oId.ToString().Trim();
                }
            }
        }

        public static string ZeroPaddedString(object oId)
        {
            if (oId is null || oId.ToString().Length == 0)
            {
                return "";
            }
            else
            {
                string sId = oId.ToString();

                if (Information.IsNumeric(sId))
                {
                    sId = StripLeadingZeros(sId);
                    return sId.PadLeft(DatabaseMap.UserLinkIndexTableIdSize, '0');
                }
                else
                {
                    return sId.Trim();
                }
            }
        }

        public static void Save(IQueryable<Table> iTableQuery, IQueryable<Databas> iDatabaseQuery, CustomTrackingStatus trackingStatus = null, CustomTrackingHistory trackingHistory = null, CustomAssetStatus assetObj = null)
        {
            ADODB.Recordset recordSet = null;
            string sql = "";
            try
            {
                int IError = 0;
                string sErrorMessage = "";
                if (trackingStatus is not null)
                {
                    sql = string.Format("SELECT * FROM [TrackingStatus] WHERE [Id] = {0}", trackingStatus.TrackingStatus.Id);
                }
                else if (trackingHistory is not null)
                {
                    sql = string.Format("SELECT * FROM [TrackingHistory] WHERE [Id] = {0}", trackingHistory.TrackingHistory.Id);
                }
                else if (assetObj is not null)
                {
                    sql = string.Format("SELECT * FROM [AssetStatus] WHERE [Id] = {0}", assetObj.AssetStatus.Id);
                }
                int arglError = 0;
                string argsErrorMessage = "";
                recordSet = DataServices.GetADORecordset(sql, null, iDatabaseQuery, lError: ref arglError, sErrorMessage: ref argsErrorMessage);
                if (trackingStatus is not null)
                {
                    LoadTrackingStatusFields(iTableQuery, trackingStatus, ref recordSet);
                    recordSet = TrackingServices.UpdateBatchADORecordSet(iDatabaseQuery, recordSet, null, false, ref IError, ref sErrorMessage, "TrackingStatus");
                }
                else if (trackingHistory is not null)
                {
                   
                    LoadTrackingHistoryFields(iTableQuery, trackingHistory, recordSet);
                    recordSet = TrackingServices.UpdateBatchADORecordSet(iDatabaseQuery, recordSet, null, false, ref IError, ref sErrorMessage,sTableName: "TrackingHistory");
                }
                else if (assetObj is not null)
                {
                    LoadAssetStatusProperties(iTableQuery, ref assetObj, recordSet);
                    recordSet = TrackingServices.UpdateBatchADORecordSet(iDatabaseQuery, recordSet, null, false, ref IError, ref sErrorMessage,  "AssetStatus");
                }
            }
            // recordSet = moApp.Data.UpdateBatchADORecordSet(recordSet, sTableName:=TableName)
            finally
            {
                recordSet.Close();
            }
        }

        public static void SaveNew(IQueryable<Table> iTableQuery, 
            IQueryable<Databas> iDatabaseQuery,
            string sTableName,
            [Optional, DefaultParameterValue(null)] ref CustomTrackingStatus trackingStatus,
            [Optional, DefaultParameterValue(null)] ref CustomTrackingHistory trackingHistory,
            [Optional, DefaultParameterValue(null)] ref CustomAssetStatus assetStatus)
        {
            ADODB.Recordset recordSet = null;
            try
            {
                string sSql = string.Format("SELECT * FROM [{0}] WHERE 0 = 1", sTableName.Trim());
                int arglError = 0;
                string argsErrorMessage = "";
                recordSet = DataServices.GetADORecordset(sSql, null, iDatabaseQuery,bAccessSpecialServerSide: true,bReturnError: true, lError: ref arglError, sErrorMessage: ref argsErrorMessage, bCheckIfDisconnected: false);
                recordSet.AddNew();
                if (trackingStatus is not null)
                {
                    LoadTrackingStatusFields(iTableQuery, trackingStatus, ref recordSet);
                    int arglError1 = 0;
                    string argsErrorMessage1 = "";
                    recordSet = UpdateBatchADORecordSet(iDatabaseQuery, recordSet, null, lError: ref arglError1, sErrorMessage: ref argsErrorMessage1);
                    CustomTrackingHistory argtrackingHisrtory = null;
                    CustomAssetStatus argassetObj = null;
                    SLRequestor argslRequestor = null;
                    LoadObject(iTableQuery, ref recordSet, ref trackingStatus, trackingHisrtory: ref argtrackingHisrtory, assetObj: ref argassetObj, slRequestor: ref argslRequestor);
                }
                else if (trackingHistory is not null)
                {
                    LoadTrackingHistoryFields(iTableQuery, trackingHistory, recordSet);
                    int arglError2 = 0;
                    string argsErrorMessage2 = "";

                    recordSet = UpdateBatchADORecordSet(iDatabaseQuery, recordSet, null, lError: ref arglError2, sErrorMessage: ref argsErrorMessage2);
                    CustomAssetStatus argassetObj1 = null;
                    SLRequestor argslRequestor1 = null;
                    
                    TrackingServices.LoadObject(iTableQuery, ref recordSet, ref trackingStatus, trackingHisrtory: ref trackingHistory, assetObj: ref argassetObj1, slRequestor: ref argslRequestor1);
                }
                else if (assetStatus is not null)
                {
                    LoadAssetStatusFields(iTableQuery, assetStatus, ref recordSet);
                    int arglError3 = 0;
                    string argsErrorMessage3 = "";
                
                    recordSet = UpdateBatchADORecordSet(iDatabaseQuery, recordSet, null, lError: ref arglError3, sErrorMessage: ref argsErrorMessage3);
                    SLRequestor argslRequestor2 = null;
                    TrackingServices.LoadObject(iTableQuery, ref recordSet, ref trackingStatus, trackingHisrtory: ref trackingHistory, assetObj: ref assetStatus, slRequestor: ref argslRequestor2);
                }
                else
                {
                    throw new ArgumentNullException("A business object is required.");
                }
            }

            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                recordSet.Close();
            }
        }

        public static void LoadTrackingStatusFields(IQueryable<Table> iTableQuery, CustomTrackingStatus businessObject, ref ADODB.Recordset recordset)
        {
            {
                var withBlock = businessObject;
                // recordset.Fields("Id").Value = .Id 'Auto Number field...
                if (withBlock.TrackingStatus.TrackedTableId is not null & !string.IsNullOrEmpty(withBlock.TrackingStatus.TrackedTableId))
                {
                    recordset.Fields["TrackedTableId"].Value = withBlock.TrackingStatus.TrackedTableId;
                }
                else
                {
                    recordset.Fields["TrackedTableId"].Value = DBNull.Value;
                }
                if (withBlock.TrackingStatus.TrackedTable is not null & !string.IsNullOrEmpty(withBlock.TrackingStatus.TrackedTable))
                {
                    recordset.Fields["TrackedTable"].Value = withBlock.TrackingStatus.TrackedTable;
                }
                else
                {
                    recordset.Fields["TrackedTable"].Value = DBNull.Value;
                }
                if (withBlock.TrackingStatus.DateDue is not null)
                {
                    recordset.Fields["DateDue"].Value = withBlock.TrackingStatus.DateDue;
                }
                else
                {
                    recordset.Fields["DateDue"].Value = DBNull.Value;
                }
                if (withBlock.TrackingStatus.UserName is not null & !string.IsNullOrEmpty(withBlock.TrackingStatus.UserName))
                {
                    recordset.Fields["UserName"].Value = withBlock.TrackingStatus.UserName;
                }
                else
                {
                    recordset.Fields["UserName"].Value = DBNull.Value;
                }
                if (withBlock.TrackingStatus.TrackingAdditionalField1 is not null & !string.IsNullOrEmpty(withBlock.TrackingStatus.TrackingAdditionalField1))
                {
                    recordset.Fields["TrackingAdditionalField1"].Value = withBlock.TrackingStatus.TrackingAdditionalField1;
                }
                else
                {
                    recordset.Fields["TrackingAdditionalField1"].Value = DBNull.Value;
                }
                if (withBlock.TrackingStatus.TrackingAdditionalField2 is not null & !string.IsNullOrEmpty(withBlock.TrackingStatus.TrackingAdditionalField2))
                {
                    recordset.Fields["TrackingAdditionalField2"].Value = withBlock.TrackingStatus.TrackingAdditionalField2;
                }
                else
                {
                    recordset.Fields["TrackingAdditionalField2"].Value = DBNull.Value;
                }
                if (withBlock.TrackingStatus.TransactionDateTime is not null)
                {
                    recordset.Fields["TransactionDateTime"].Value = withBlock.TrackingStatus.TransactionDateTime;
                }
                else
                {
                    recordset.Fields["TransactionDateTime"].Value = DBNull.Value;
                }
                if (withBlock.TrackingStatus.ProcessedDateTime is not null)
                {
                    recordset.Fields["ProcessedDateTime"].Value = withBlock.TrackingStatus.ProcessedDateTime;
                }
                else
                {
                    recordset.Fields["ProcessedDateTime"].Value = DBNull.Value;
                }
                recordset.Fields["Out"].Value = withBlock.TrackingStatus.Out;
                var TrackingContainter = iTableQuery.Where(m => m.TrackingTable > 0).ToList();
                int TrackingCount = 0;
                if (TrackingContainter is not null)
                {
                    TrackingCount = TrackingContainter.Count;
                }
                for (int i = 0, loopTo = TrackingCount - 1; i <= loopTo; i++)
                {
                    if (withBlock.get_ContainerColumns(TrackingContainter[i].TrackingStatusFieldName) is not null & !string.IsNullOrEmpty(withBlock.get_ContainerColumns(TrackingContainter[i].TrackingStatusFieldName)))
                    {
                        recordset.Fields[DatabaseMap.RemoveTableNameFromField(TrackingContainter[i].TrackingStatusFieldName)].Value = withBlock.get_ContainerColumns(TrackingContainter[i].TrackingStatusFieldName);
                    }
                    else
                    {
                        recordset.Fields[DatabaseMap.RemoveTableNameFromField(TrackingContainter[i].TrackingStatusFieldName)].Value = DBNull.Value;
                    }

                }
            }
        }

        public static void LoadAssetStatusFields(IQueryable<Table> iTableQuery, CustomAssetStatus businessObject, ref ADODB.Recordset recordset)
        {
            {
                var withBlock = businessObject;
                // recordset.Fields("Id").Value = .Id 'Auto Number field...
                if (withBlock.AssetStatus.TrackedTableId is not null & !string.IsNullOrEmpty(withBlock.AssetStatus.TrackedTableId))
                {
                    recordset.Fields["TrackedTableId"].Value = withBlock.AssetStatus.TrackedTableId;
                }
                else
                {
                    recordset.Fields["TrackedTableId"].Value = DBNull.Value;
                }
                if (withBlock.AssetStatus.TrackedTable is not null & !string.IsNullOrEmpty(withBlock.AssetStatus.TrackedTable))
                {
                    recordset.Fields["TrackedTable"].Value = withBlock.AssetStatus.TrackedTable;
                }
                else
                {
                    recordset.Fields["TrackedTable"].Value = DBNull.Value;
                }
                if (withBlock.AssetStatus.DateDue is not null)
                {
                    recordset.Fields["DateDue"].Value = withBlock.AssetStatus.DateDue;
                }
                else
                {
                    recordset.Fields["DateDue"].Value = DBNull.Value;
                }
                if (withBlock.AssetStatus.UserName is not null & !string.IsNullOrEmpty(withBlock.AssetStatus.UserName))
                {
                    recordset.Fields["UserName"].Value = withBlock.AssetStatus.UserName;
                }
                else
                {
                    recordset.Fields["UserName"].Value = DBNull.Value;
                }
                if (withBlock.AssetStatus.TrackingAdditionalField1 is not null & !string.IsNullOrEmpty(withBlock.AssetStatus.TrackingAdditionalField1))
                {
                    recordset.Fields["TrackingAdditionalField1"].Value = withBlock.AssetStatus.TrackingAdditionalField1;
                }
                else
                {
                    recordset.Fields["TrackingAdditionalField1"].Value = DBNull.Value;
                }
                if (withBlock.AssetStatus.TrackingAdditionalField2 is not null & !string.IsNullOrEmpty(withBlock.AssetStatus.TrackingAdditionalField2))
                {
                    recordset.Fields["TrackingAdditionalField2"].Value = withBlock.AssetStatus.TrackingAdditionalField2;
                }
                else
                {
                    recordset.Fields["TrackingAdditionalField2"].Value = DBNull.Value;
                }
                if (withBlock.AssetStatus.TransactionDateTime is not null)
                {
                    recordset.Fields["TransactionDateTime"].Value = withBlock.AssetStatus.TransactionDateTime;
                }
                else
                {
                    recordset.Fields["TransactionDateTime"].Value = DBNull.Value;
                }
                if (withBlock.AssetStatus.ProcessedDateTime is not null)
                {
                    recordset.Fields["ProcessedDateTime"].Value = withBlock.AssetStatus.ProcessedDateTime;
                }
                else
                {
                    recordset.Fields["ProcessedDateTime"].Value = DBNull.Value;
                }
                recordset.Fields["Out"].Value = withBlock.AssetStatus.Out;
                var TrackingContainter = iTableQuery.Where(m => m.TrackingTable > 0).ToList();
                int TrackingCount = 0;
                if (TrackingContainter is not null)
                {
                    TrackingCount = TrackingContainter.Count;
                }
                for (int i = 0, loopTo = TrackingCount - 1; i <= loopTo; i++)
                {
                    if (withBlock.get_ContainerColumns(TrackingContainter[i].TrackingStatusFieldName) is not null & !string.IsNullOrEmpty(withBlock.get_ContainerColumns(TrackingContainter[i].TrackingStatusFieldName)))
                    {
                        recordset.Fields[DatabaseMap.RemoveTableNameFromField(TrackingContainter[i].TrackingStatusFieldName)].Value = withBlock.get_ContainerColumns(TrackingContainter[i].TrackingStatusFieldName);
                    }
                    else
                    {
                        recordset.Fields[DatabaseMap.RemoveTableNameFromField(TrackingContainter[i].TrackingStatusFieldName)].Value = DBNull.Value;
                    }

                }
            }
        }


        public static void LoadTrackingHistoryFields(IQueryable<Table> iTableQuery, CustomTrackingHistory businessObject, ADODB.Recordset recordset)
        {
            {
                var withBlock = businessObject;
                // recordset.Fields("Id").Value = .Id 'Auto Number field...
                if (withBlock.TrackingHistory.TrackedTableId is not null & !string.IsNullOrEmpty(withBlock.TrackingHistory.TrackedTableId))
                {
                    recordset.Fields["TrackedTableId"].Value = withBlock.TrackingHistory.TrackedTableId;
                }
                else
                {
                    recordset.Fields["TrackedTableId"].Value = DBNull.Value;
                }
                if (withBlock.TrackingHistory.TrackedTable is not null & !string.IsNullOrEmpty(withBlock.TrackingHistory.TrackedTable))
                {
                    recordset.Fields["TrackedTable"].Value = withBlock.TrackingHistory.TrackedTable;
                }
                else
                {
                    recordset.Fields["TrackedTable"].Value = DBNull.Value;
                }
                if (withBlock.TrackingHistory.UserName is not null & !string.IsNullOrEmpty(withBlock.TrackingHistory.UserName))
                {
                    recordset.Fields["UserName"].Value = withBlock.TrackingHistory.UserName;
                }
                else
                {
                    recordset.Fields["UserName"].Value = DBNull.Value;
                }
                if (withBlock.TrackingHistory.TrackingAdditionalField1 is not null & !string.IsNullOrEmpty(withBlock.TrackingHistory.TrackingAdditionalField1))
                {
                    recordset.Fields["TrackingAdditionalField1"].Value = withBlock.TrackingHistory.TrackingAdditionalField1;
                }
                else
                {
                    recordset.Fields["TrackingAdditionalField1"].Value = DBNull.Value;
                }
                if (withBlock.TrackingHistory.TrackingAdditionalField2 is not null & !string.IsNullOrEmpty(withBlock.TrackingHistory.TrackingAdditionalField2))
                {
                    recordset.Fields["TrackingAdditionalField2"].Value = withBlock.TrackingHistory.TrackingAdditionalField2;
                }
                else
                {
                    recordset.Fields["TrackingAdditionalField2"].Value = DBNull.Value;
                }

                recordset.Fields["IsActualScan"].Value = withBlock.TrackingHistory.IsActualScan;
                recordset.Fields["BatchId"].Value = withBlock.TrackingHistory.BatchId;
                if (withBlock.TrackingHistory.TransactionDateTime is not null)
                {
                    recordset.Fields["TransactionDateTime"].Value = withBlock.TrackingHistory.TransactionDateTime;
                }
                var TrackingContainter = iTableQuery.Where(m => m.TrackingTable > 0).ToList();
                int TrackingCount = 0;
                if (TrackingContainter is not null)
                {
                    TrackingCount = TrackingContainter.Count;
                }
                for (int i = 0, loopTo = TrackingCount - 1; i <= loopTo; i++)
                {
                    if (withBlock.get_ContainerColumns(TrackingContainter[i].TrackingStatusFieldName) is not null & !string.IsNullOrEmpty(withBlock.get_ContainerColumns(TrackingContainter[i].TrackingStatusFieldName)))
                    {
                        recordset.Fields[DatabaseMap.RemoveTableNameFromField(TrackingContainter[i].TrackingStatusFieldName)].Value = withBlock.get_ContainerColumns(TrackingContainter[i].TrackingStatusFieldName);
                    }
                    else
                    {
                        recordset.Fields[DatabaseMap.RemoveTableNameFromField(TrackingContainter[i].TrackingStatusFieldName)].Value = DBNull.Value;
                    }
                }
            }
        }

        public static void LoadObject(IQueryable<Table> iTableQuery,
            ref ADODB.Recordset recordset,
            [Optional, DefaultParameterValue(null)] ref CustomTrackingStatus trackingObj,
            [Optional, DefaultParameterValue(null)] ref CustomTrackingHistory trackingHisrtory,
            [Optional, DefaultParameterValue(null)] ref CustomAssetStatus assetObj,
            [Optional, DefaultParameterValue(null)] ref SLRequestor slRequestor)
        {

            if (recordset is null || recordset.EOF || recordset.BOF)
            {
                recordset = null;
            }
            else if (trackingObj is not null)
            {
                LoadTrackingStatusProperties(iTableQuery, ref trackingObj, recordset);
            }
            else if (trackingHisrtory is not null)
            {
                LoadTrackingHistoryProperties(iTableQuery, ref trackingHisrtory, recordset);
            }
            else if (assetObj is not null)
            {
                LoadAssetStatusProperties(iTableQuery, ref assetObj, recordset);
            }
            else
            {
                throw new ArgumentNullException("A business object is required.");
            }
        }

        public static void LoadTrackingStatusProperties(IQueryable<Table> iTableQuery, ref CustomTrackingStatus trackingObj, ADODB.Recordset recordset)
        {

            if (!recordset.EOF)
            {
                trackingObj.TrackingStatus = new TrackingStatu();
                trackingObj.TrackingStatus.Id = Convert.ToInt32(recordset.Fields["Id"].Value);
                trackingObj.TrackingStatus.TrackedTableId = recordset.Fields["TrackedTableId"].ToString();
                trackingObj.TrackingStatus.TrackedTable = recordset.Fields["TrackedTable"].ToString();
                trackingObj.TrackingStatus.UserName = recordset.Fields["UserName"].ToString();
                trackingObj.TrackingStatus.TrackingAdditionalField1 = recordset.Fields["TrackingAdditionalField1"].ToString();
                trackingObj.TrackingStatus.TrackingAdditionalField2 = recordset.Fields["TrackingAdditionalField2"].ToString();
                if (!(recordset.Fields["TransactionDateTime"].Value is DBNull))
                {
                    trackingObj.TrackingStatus.TransactionDateTime = Convert.ToDateTime(recordset.Fields["TransactionDateTime"].ToString());
                }
                if (!(recordset.Fields["ProcessedDateTime"].Value is DBNull))
                {
                    trackingObj.TrackingStatus.ProcessedDateTime = Convert.ToDateTime(recordset.Fields["ProcessedDateTime"].Value);
                }
                if (!(recordset.Fields["DateDue"].Value is DBNull))
                {
                    trackingObj.TrackingStatus.DateDue = Convert.ToDateTime(recordset.Fields["DateDue"].Value);
                }
                if (!(recordset.Fields["Out"].Value is DBNull))
                {
                    trackingObj.TrackingStatus.Out = Convert.ToBoolean(recordset.Fields["Out"].Value);
                }
                string s;
                var TrackingContainer = iTableQuery.Where(m => m.TrackingTable > 0).ToList();
                int TrackingCount = 0;
                if (TrackingContainer is not null)
                {
                    TrackingCount = TrackingContainer.Count;
                }
                for (int i = 0, loopTo = TrackingCount - 1; i <= loopTo; i++)
                {
                    s = TrackingContainer[i].TrackingStatusFieldName;
                    trackingObj.set_ContainerColumns(s, recordset.Fields[DatabaseMap.RemoveTableNameFromField(s)].ToString());
                }
            }
        }

        public static void LoadAssetStatusProperties(IQueryable<Table> iTableQuery, ref CustomAssetStatus assetObj, ADODB.Recordset recordset)
        {

            if (!recordset.EOF)
            {
                assetObj.AssetStatus = new AssetStatu();
                assetObj.AssetStatus.Id = Convert.ToInt32(recordset.Fields["Id"].Value);
                assetObj.AssetStatus.TrackedTableId = recordset.Fields["TrackedTableId"].ToString();
                assetObj.AssetStatus.TrackedTable = recordset.Fields["TrackedTable"].ToString();
                assetObj.AssetStatus.UserName = recordset.Fields["UserName"].ToString();
                assetObj.AssetStatus.TrackingAdditionalField1 = recordset.Fields["TrackingAdditionalField1"].ToString();
                assetObj.AssetStatus.TrackingAdditionalField2 = recordset.Fields["TrackingAdditionalField2"].ToString();
                if (!(recordset.Fields["TransactionDateTime"] is DBNull))
                {
                    assetObj.AssetStatus.TransactionDateTime = Convert.ToDateTime(recordset.Fields["TransactionDateTime"]);
                }
                if (!(recordset.Fields["ProcessedDateTime"] is DBNull))
                {
                    assetObj.AssetStatus.ProcessedDateTime = Convert.ToDateTime(recordset.Fields["ProcessedDateTime"]);
                }
                if (!(recordset.Fields["DateDue"] is DBNull))
                {
                    assetObj.AssetStatus.DateDue = Convert.ToDateTime(recordset.Fields["DateDue"]);
                }
                if (!(recordset.Fields["Out"] is DBNull))
                {
                    assetObj.AssetStatus.Out = Convert.ToBoolean(recordset.Fields["Out"].Value);
                }
                string s;
                var TrackingContainer = iTableQuery.Where(m => m.TrackingTable > 0).ToList();
                int TrackingCount = 0;
                if (TrackingContainer is not null)
                {
                    TrackingCount = TrackingContainer.Count;
                }
                for (int i = 0, loopTo = TrackingCount - 1; i <= loopTo; i++)
                {
                    s = TrackingContainer[i].TrackingStatusFieldName;
                    assetObj.set_ContainerColumns(s, recordset.Fields[DatabaseMap.RemoveTableNameFromField(s)].ToString());
                }
            }
        }

        public static void LoadTrackingHistoryProperties(IQueryable<Table> iTableQuery, ref CustomTrackingHistory trackingHistory, ADODB.Recordset recordset)
        {
            if (!recordset.EOF)
            {
                trackingHistory.TrackingHistory = new TrackingHistory();
                trackingHistory.TrackingHistory.Id = (int)recordset.Fields["Id"].Value;
                trackingHistory.TrackingHistory.TrackedTableId = recordset.Fields["TrackedTableId"].ToString();
                trackingHistory.TrackingHistory.TrackedTable = recordset.Fields["TrackedTable"].ToString();
                trackingHistory.TrackingHistory.UserName = recordset.Fields["UserName"].ToString();
                trackingHistory.TrackingHistory.TrackingAdditionalField1 = recordset.Fields["TrackingAdditionalField1"].ToString();
                trackingHistory.TrackingHistory.TrackingAdditionalField2 = recordset.Fields["TrackingAdditionalField2"].ToString();
                if (!(recordset.Fields["TransactionDateTime"].Value is DBNull))
                {
                    trackingHistory.TrackingHistory.TransactionDateTime = Convert.ToDateTime(recordset.Fields["TransactionDateTime"].Value);
                }
                if (!(recordset.Fields["IsActualScan"].Value is DBNull))
                {
                    trackingHistory.TrackingHistory.IsActualScan = Convert.ToBoolean(recordset.Fields["IsActualScan"].Value);
                }
                trackingHistory.TrackingHistory.BatchId = (int?)recordset.Fields["BatchId"].Value;
                var TrackingContainer = iTableQuery.Where(m => m.TrackingTable > 0).ToList();
                int TrackingCount = 0;
                if (TrackingContainer is not null)
                {
                    TrackingCount = TrackingContainer.Count;
                }
                for (int i = 0, loopTo = TrackingCount - 1; i <= loopTo; i++)
                    trackingHistory.set_ContainerColumns(TrackingContainer[i].TrackingStatusFieldName, recordset.Fields[DatabaseMap.RemoveTableNameFromField(TrackingContainer[i].TrackingStatusFieldName)].ToString());
            }
        }

        public static ADODB.Recordset UpdateBatchADORecordSet(IQueryable<Databas> iDatabaseQuery,
            ADODB.Recordset rsADO,
            Table sTableObj,
            [Optional, DefaultParameterValue(false)] bool bReturnError ,
            [Optional, DefaultParameterValue(0)] ref int lError,
            [Optional, DefaultParameterValue("")] ref string sErrorMessage,
            [Optional] string sTableName 
            )
        {
            Databas oDatabase = null;
            bool bHadConnection = false;
            Table oTables;
            try
            {
                var sADOCon = DataServices.DBOpen();

                bHadConnection = true;
                // Resync the connection
                if (!string.IsNullOrEmpty(Strings.Trim(sTableName)))
                {
                    oTables = sTableObj;
                    oDatabase = DataServices.GetDBObjForTable(oTables, iDatabaseQuery);
                    sADOCon = DataServices.DBOpen(oDatabase);
                    if (rsADO.ActiveConnection is null)
                    {
                        if (oDatabase is not null)
                        {
                            bHadConnection = false;

                            // If (Not (oTables Is Nothing)) Then
                            // oDatabase.cnDBADO.CommandTimeout = oTables.ADOQueryTimeout
                            // Else
                            // oDatabase.cnDBADO.CommandTimeout = 30
                            // End If

                            rsADO.ActiveConnection = sADOCon;

                            if (oTables is not null)
                            {
                                if (oTables.ADOCacheSize > 0)
                                {
                                    rsADO.CacheSize = (int)oTables.ADOCacheSize;
                                }
                            }
                        }

                        if (!TrackingServices.SQLStatementContainsJoin(rsADO.Source.ToString()))
                        {
                            rsADO.Resync((ADODB.AffectEnum)Enums.AffectEnum.rmAffectCurrent, (ADODB.ResyncEnum)Enums.ResyncEnum.rmResyncUnderlyingValues);
                        }
                    }
                }
                else if (rsADO.ActiveConnection is null)
                {
                    rsADO.ActiveConnection = DataServices.DBOpen();
                }

                // Update the record...
                // Not sure why this is required, but the adAffectAll was not working in all cases -Dan
                // Commented out; may be causing "Multiple-step" errors.  But back if it causes more problems. RVW 01/13/2009
                // rsADO.UpdateBatch(RMAG.AffectEnum.rmAffectCurrent)
                rsADO.UpdateBatch((ADODB.AffectEnum)Enums.AffectEnum.rmAffectAll);

                if (!bHadConnection)
                {
                    if (rsADO.State != (int)Enums.ObjectStateEnum.rmStateClosed)
                    {
                        rsADO.ActiveConnection = null;
                    }

                    bHadConnection = true;
                }
                // Return updated RecordSet...
                return rsADO;
            }
            catch(Exception)
            {
                if (bReturnError)
                {
                    // Errors while saving but all seems fine. This needs to be looked into!
                    // If (err.Number <> -2147217887) Then 'This error occurs if in access if updatebatch does not use adAffectAll
                    lError = Information.Err().Number;
                    sErrorMessage = Information.Err().Description;
                    // End If
                    // End If
                }

                if (!bHadConnection)
                {
                    if (rsADO.State != (int)Enums.ObjectStateEnum.rmStateClosed)
                    {
                        rsADO.ActiveConnection = null;
                    }
                }
                return rsADO;
            }           
        }

        public static bool SQLStatementContainsJoin(string sSQL)
        {
            int iPos;
            string sTemp;
            string[] asTemp;
            try
            {
                iPos = Strings.InStr(1, sSQL, " join ", CompareMethod.Text);

                if (iPos == 0)
                {
                    return false;
                }
                else
                {
                    while (iPos > 0)
                    {
                        sTemp = Strings.Left(sSQL, iPos - 1);
                        asTemp = Strings.Split(sTemp, "'");

                        iPos = -1;
                        iPos = Information.UBound(asTemp);

                        if (iPos % 2 <= 0)
                        {
                            asTemp = null;
                            return true;
                        }

                        sSQL = Strings.Mid(sSQL, iPos);
                        iPos = Strings.InStr(1, sSQL, " join ", CompareMethod.Text);
                    }
                }

                asTemp = null;
                return false;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        public static string ReplaceTicks(string keyValue)
        {
            return string.Format("'{0}'", Strings.Replace(keyValue, "'", "''"));
        }

        public static CustomTrackingStatus GetByTrackedTableAndIDTrackingStatus(IQueryable<Table> itableQuery, IQueryable<Databas> iDatabaseQuery, string TrackedTable, string TrackedTableID)
        {
            var trackingObj = new CustomTrackingStatus();
            string argwhereClause = BuildTrackedTableAndIDWhereClause(TrackedTable, TrackedTableID);
            CustomAssetStatus argassetObj = null;
            GetByForTrackingAsset(itableQuery, iDatabaseQuery, ref argwhereClause, ref argassetObj, ref trackingObj);
            return trackingObj;
        }


        public static CustomAssetStatus GetByTrackedTableAndIDAssetStatus(IQueryable<Table> itableQuery, IQueryable<Databas> iDatabaseQuery, string TrackedTable, string TrackedTableID)
        {
            var assetObj = new CustomAssetStatus();
            string argwhereClause = BuildTrackedTableAndIDWhereClause(TrackedTable, TrackedTableID);
            CustomTrackingStatus argtrackingObj = null;
            GetByForTrackingAsset(itableQuery, iDatabaseQuery, ref argwhereClause, ref assetObj, ref argtrackingObj);
            return assetObj;
        }

        public static string BuildTrackedTableAndIDWhereClause(string TrackedTable, string TrackedTableID)
        {
            return string.Format("WHERE TrackedTableId = {0} AND TrackedTable = {1}", ReplaceTicks(TrackedTableID), ReplaceTicks(TrackedTable));
        }

        public static void GetByForTrackingAsset(IQueryable<Table> iTableQuery, IQueryable<Databas> iDatabaseQuery, ref string whereClause, [Optional, DefaultParameterValue(null)] ref CustomAssetStatus assetObj, [Optional, DefaultParameterValue(null)] ref CustomTrackingStatus trackingObj)
        {
            ADODB.Recordset recordset = null;
            string tableName = "";
            if (assetObj is not null)
            {
                tableName = "AssetStatus";
            }
            else if (trackingObj is not null)
            {
                tableName = "TrackingStatus";
            }
            try
            {
                string sql = string.Format("SELECT TOP 1 * FROM [{0}] {1}", tableName, whereClause);
                string arglError = "";
                recordset = DataServices.GetADORecordSet(ref sql, DataServices.DBOpen(), lError: ref arglError);
                // recordset = DataServices.GetADORecordset(sql, Nothing, iDatabaseQuery, Enums.CursorTypeEnum.rmOpenForwardOnly)
                if (assetObj is not null)
                {
                    CustomTrackingStatus argtrackingObj = null;
                    CustomTrackingHistory argtrackingHisrtory = null;
                    SLRequestor argslRequestor = null;
                    LoadObject(iTableQuery, ref recordset, ref argtrackingObj, ref argtrackingHisrtory, ref assetObj, slRequestor: ref argslRequestor);
                }
                else if (trackingObj is not null)
                {
                    CustomTrackingHistory argtrackingHisrtory1 = null;
                    CustomAssetStatus argassetObj = null;
                    SLRequestor argslRequestor1 = null;
                    LoadObject(iTableQuery, ref recordset, ref trackingObj, ref argtrackingHisrtory1, ref argassetObj, slRequestor: ref argslRequestor1);
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                if (recordset is not null && recordset.State == (int)Enums.ObjectStateEnum.rmStateOpen)
                    recordset.Close();
            }
        }

        public static ADODB.Recordset GetRecordSetByContainer(IQueryable<Databas> iDatabaseQuery, string containerFieldName, string containerColumnValue, Enums.CursorTypeEnum cursorType = Enums.CursorTypeEnum.rmOpenKeyset)
        {
            // Need to validate params.

            string sql = string.Format("SELECT * FROM [AssetStatus]");
            if (0 < containerFieldName.Trim().Length)
            {
                sql += string.Format(" WITH (INDEX({0})) WHERE [{0}] = '{1}'", DatabaseMap.RemoveTableNameFromField(containerFieldName), containerColumnValue);
            }

            int arglError = 0;
            string argsErrorMessage = "";
            return DataServices.GetADORecordset(sql, null, iDatabaseQuery, cursorType, lError: ref arglError, sErrorMessage: ref argsErrorMessage);
        }

        public static bool SaveSignatureToDB(IQueryable<Databas> _iDatabas, string sTable, int iTableID, string sTempFile)
        {
            bool SaveSignatureToDBRet = default;
            ADODB.Recordset rsImageTable;

            SaveSignatureToDBRet = false;
            int arglError = 0;
            string argsErrorMessage = "";
            rsImageTable = DataServices.GetADORecordset("Select * from [SLSignature] WHERE [Table] = '" + sTable + "' AND [TableId] = " + iTableID, null, _iDatabas, lError: ref arglError, sErrorMessage: ref argsErrorMessage);

            if (rsImageTable is not null)
            {
                try
                {
                    if (rsImageTable.BOF | rsImageTable.EOF)
                    {
                        rsImageTable.AddNew();
                    }

                    System.IO.FileStream strFile;

                    strFile = new System.IO.FileStream(sTempFile, System.IO.FileMode.Open);
                    var arrImage = new byte[(int)strFile.Length + 1];

                    strFile.Read(arrImage, 0, (int)strFile.Length);
                    strFile.Close();
                    
                    rsImageTable.Fields["Table"].Value = sTable;
                    rsImageTable.Fields["TableID"].Value = iTableID;
                    rsImageTable.Fields["Signature"].Value = arrImage;
                    rsImageTable = TrackingServices.UpdateBatchADORecordSet(_iDatabas, rsImageTable, null,false, lError: ref arglError, sErrorMessage: ref argsErrorMessage, sTableName: sTable);

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    rsImageTable.Close();
                }
            }

            return SaveSignatureToDBRet;
        }

        private static void RemoveRequests(IQueryable<Table> _iTables, IQueryable<Databas> _iDatabase, IRepository<SLRequestor> _iSLRequestor, dynamic oTrackingStatus, string oTrackedTable, string oTrackedTableId)
        {
            try
            {
                string sEmployeeId = string.Empty;
                Table oEmployeeTable;
                _iSLRequestor.BeginTransaction();
                oEmployeeTable = _iTables.Where(m => m.TrackingTable == TRACKING_EMPLOYEE).FirstOrDefault();
                // oEmployeeTable = moApp.TrackingTables.Item(TRACKING_EMPLOYEE - 1)

                if (oEmployeeTable is not null)
                {
                    sEmployeeId = Convert.ToString(oTrackingStatus.ContainerColumns(DatabaseMap.RemoveTableNameFromField(oEmployeeTable.TrackingStatusFieldName)));
                    // If the new location is to an Employee then mark any requests by that employee for this object as fulfilled
                    if (sEmployeeId.Trim().Length > 0)
                    {
                        var SLRequestorEntity = _iSLRequestor.All().Where(m => (m.TableId ?? "") == (oTrackedTableId ?? "") & m.TableName.Trim().ToLower().Equals(oTrackedTable.Trim().ToLower()) & m.Status.Trim() != "Deleted" & m.Status.Trim() != "Fulfilled").OrderBy(m => m.Priority).ThenBy(m => m.DateRequested);
                        foreach (var oSLRequestor in SLRequestorEntity)
                        {
                            if ((oSLRequestor.EmployeeId ?? "") == (sEmployeeId ?? ""))
                            {
                                oSLRequestor.Status = "Fulfilled";
                                oSLRequestor.DateReceived = DateTime.Now;
                                _iSLRequestor.Update(oSLRequestor);
                            }
                            else if (oSLRequestor.Status == "New")
                            {
                                oSLRequestor.Status = "WaitList";
                                _iSLRequestor.Update(oSLRequestor);
                            }
                            // moApp.SLRequestor.Save(oSLRequestor)
                            else if (oSLRequestor.Status == "In Process")
                            {
                                oSLRequestor.Status = "WaitList";
                                oSLRequestor.SLPullListsId = 0;
                                oSLRequestor.DatePulled = new DateTime(); // System.Convert.ToDateTimeTime(0)
                                _iSLRequestor.Update(oSLRequestor);
                                // moApp.SLRequestor.Save(oSLRequestor)
                            }
                        }
                    }
                    string argsAPIError = "\0";
                    UpgradeMostEligibleRequest(_iTables, _iDatabase, oTrackedTable, oTrackedTableId, sAPIError: ref argsAPIError);
                    _iSLRequestor.CommitTransaction();
                }
            }
            catch(Exception)
            {
                _iSLRequestor.RollBackTransaction();
            }


            return;
        }

        public static void UpgradeMostEligibleRequest(IQueryable<Table> _iTables, IQueryable<Databas> _iDatabase, string sTrackedTable, string sTrackedTableId, [Optional, DefaultParameterValue(Constants.vbNullChar)] ref string sAPIError)
        {
            string sSQL;
            Table oLocationsTable;
            Databas oDatabase;
            var sADOCon = DataServices.DBOpen();
            oLocationsTable = _iTables.Where(m => m.TrackingTable == TRACKING_LOCATION).FirstOrDefault();

            // oLocationsTable = moApp.TrackingTables.Item(TRACKING_LOCATION - 1)
            // Switch the most eligible item to New (If any)
            sSQL = "UPDATE [SLRequestor] SET [Status] = 'New' WHERE ";
            sSQL += "(";
            sSQL += "([Id] in ";
            sSQL += "(";
            sSQL += " SELECT TOP 1 [SLRequestor].[Id] FROM ([SLRequestor] INNER JOIN [TrackingStatus]";
            sSQL += " ON ([SLRequestor].[TableName] = [TrackingStatus].[TrackedTable] AND [SLRequestor].[TableId] = [TrackingStatus].[TrackedTableId] ))";

            if (!string.IsNullOrEmpty(Strings.Trim(oLocationsTable.TrackingRequestableFieldName)))
            {
                sSQL += " INNER JOIN [" + oLocationsTable.TableName + "]";
                sSQL += " ON ([TrackingStatus].[" + DatabaseMap.RemoveTableNameFromField(oLocationsTable.TrackingStatusFieldName) + "] =";
                bool IfIdFieldIsString = DataServices.IdFieldIsString(sADOCon, oLocationsTable.TableName, oLocationsTable.IdFieldName);
                if (IfIdFieldIsString)
                {
                    sSQL += " [" + oLocationsTable.TableName + "].[" + DatabaseMap.RemoveTableNameFromField(oLocationsTable.IdFieldName) + "])";
                }
                else
                {
                    oDatabase = DataServices.GetDBObjForTable(oLocationsTable, _iDatabase);
                    sSQL += " RIGHT('" + new string('0', DatabaseMap.UserLinkIndexTableIdSize) + "' + Convert(VARCHAR(" + DatabaseMap.UserLinkIndexTableIdSize.ToString() + "), [" + oLocationsTable.TableName + "].[" + DatabaseMap.RemoveTableNameFromField(oLocationsTable.IdFieldName) + "]), " + DatabaseMap.UserLinkIndexTableIdSize.ToString() + "))";
                }
            }

            sSQL += " WHERE (([SLRequestor].[TableName] = '" + sTrackedTable + "')";
            sSQL += " AND ([SLRequestor].[TableId] = '" + Strings.Replace(sTrackedTableId, "'", "''") + "')";
            sSQL += " AND ([SLRequestor].[Status] = 'WaitList')";
            sSQL += " AND ([TrackingStatus].[" + DatabaseMap.RemoveTableNameFromField(oLocationsTable.TrackingStatusFieldName) + "] IS NOT NULL)";

            if (!string.IsNullOrEmpty(Strings.Trim(oLocationsTable.TrackingRequestableFieldName)))
            {
                sSQL += " AND ([" + oLocationsTable.TableName + "].[" + DatabaseMap.RemoveTableNameFromField(oLocationsTable.TrackingRequestableFieldName) + "] <> 0)";
            }

            sSQL += ") ORDER BY [SLRequestor].[Priority], [SLRequestor].[DateRequested]";
            sSQL += ")";
            sSQL += ")";
            sSQL += " AND ";
            sSQL += "(";
            sSQL += " (SELECT Count([SLRequestor].[Id]) FROM SLRequestor";
            sSQL += " WHERE ";
            sSQL += "(";
            sSQL += "([TableName] = '" + sTrackedTable + "')";
            sSQL += " AND ([TableId] = '" + Strings.Replace(sTrackedTableId, "'", "''") + "')";
            sSQL += " AND ";
            sSQL += "(";
            sSQL += " ([Status] = 'New')";
            sSQL += " OR ([Status] = 'New Batch')";
            sSQL += " OR ([Status] = 'In Process')";
            sSQL += " OR ([Status] = 'Exception')";
            sSQL += ")";
            sSQL += ")";
            sSQL += ") = 0)";
            sSQL += ")";
            DataServices.ProcessADOCommand(ref sSQL, DataServices.DBOpen(), true);
            return;

        }

        //public static string UpdateTrackingData(Smead.Security.Passport passport,
        //    IQueryable<Databas> _iDatabas,
        //    IQueryable<TabFusionRMS.Models.System> _iSystem,
        //    IQueryable<Table> _iTables,
        //    IQueryable<SecureUser> _iSecureUser, 
        //    IQueryable<SLRetentionCode> _iSLRetentionCode,
        //    IRepository<SLDestructCertItem> _iSLDestructCertItem,
        //    IRepository<SLRequestor> _iSLRequestor,
        //    IRepository<TrackingHistory> _iTrackingHistory,
        //    int hWnd, FoundBarCode oObject,
        //    FoundBarCode oDestination, bool bIsContainer,
        //    bool bDoRecon, string sDueBackDate,
        //    bool bCheckForDups,
        //    [Optional, DefaultParameterValue(false)] bool bReturnErrors,
        //    [Optional, DefaultParameterValue("")] ref string sTrackingAdditionalField1,
        //    [Optional, DefaultParameterValue("")] ref string sTrackingAdditionalField2,
        //    [Optional, DateTimeConstant(315569088000000000L/* #1/1/1001# */)] ref DateTime dtScanDateTime,
        //    [Optional, DefaultParameterValue(Constants.vbNullChar)] ref string sAPIError,
        //    [Optional, DefaultParameterValue(Enums.geTrackingOriginatedFrom.tofUnknown)] Enums.geTrackingOriginatedFrom eTrackingOrigination ,
        //    [Optional, DefaultParameterValue(false)] bool bUseExtMsgBox , 
        //    [Optional, DefaultParameterValue(false)] ref bool bCheckForDupsReturn)
        //{
        //    string UpdateTrackingDataRet = default;
        //    UpdateTrackingDataRet = "";
        //    bool bCurrOut;
        //    bool bCurrentScan;
        //    bool bDoChildren;
        //    bool bDoUpdate;
        //    bool bFoundTrackStatus;
        //    bool bFoundPriorHistoryRec;
        //    bool bInTheContainer;
        //    bool bUpLocs;
        //    bool bValid;
        //    DateTime dTime;
        //    var dScanDateTime = ScanDateTime;
        //    int iCnt;
        //    string sSQL = string.Empty;
        //    string sUserName;
        //    string sTempStr;
        //    object vReturnDate;
        //    Table oTable = null;
        //    var oTrackingHistory = new CustomTrackingHistory();
        //    oTrackingHistory.TrackingHistory = new TrackingHistory();
        //    dynamic oTmpTrackingHistory;
        //    dynamic oTrackingStatus;
        //    dynamic oNewTrackingStat;
        //    dynamic oDestTrackStat;
        //    CustomTrackingStatus oUpTrackStat;
        //    dynamic oChildTrackStat;
        //    ADODB.Recordset rsTrackingChildren = null;
        //    ADODB.Recordset rsTrackingHistory = null;
        //    ADODB.Recordset rsTrackingStatus = null;
        //    var oDestTableCon = DataServices.DBOpen();
        //    var oObjectTableCon = DataServices.DBOpen();
        //    string currentUserName = string.Empty;
        //    int sUserId = 0;
        //    if (passport is not null)
        //    {
        //        sUserId = passport.UserId;
        //    }
        //    else
        //    {
        //        Passport pass = ContextService.GetObjectFromJson<Passport>("passport");
        //        sUserId = pass.UserId;
        //    }
        //    if (sUserId != 0)
        //    {
        //        currentUserName = _iSecureUser.Where(m => m.UserID == sUserId).FirstOrDefault().UserName;
        //    }

        //    if (oDestination.oTable is not null)
        //    {
        //        oDestTableCon = DataServices.DBOpen(oDestination.oTable, _iDatabas);
        //    }
        //    if (oObject.oTable is not null)
        //    {
        //        oObjectTableCon = DataServices.DBOpen(oObject.oTable, _iDatabas);
        //    }

        //    var oSystem = _iSystem.OrderBy(m => m.Id).FirstOrDefault();

        //    var TrackingContainer = _iTables.Where(m => m.TrackingTable > 0).OrderBy(m => m.TrackingTable).ToList();
        //    int TrackingCount = 0;
        //    if (TrackingContainer is not null)
        //    {
        //        TrackingCount = TrackingContainer.Count;
        //    }

        //    UpdateTrackingDataRet = "";
        //    // Decide what user name to use
        //    if (TelxonModeOn)
        //    {
        //        sUserName = TelxonUserName;
        //        bCurrentScan = false;
        //    }
        //    else
        //    {
        //        sUserName = currentUserName;
        //        bCurrentScan = true;
        //    }
        //    if (bDoRecon)
        //    {
        //        oDestTrackStat = new CustomAssetStatus();
        //        oDestTrackStat.AssetStatus = new AssetStatu();
        //        oDestTrackStat.AssetStatus.TrackedTable = oDestination.oTable.TableName;
        //        oDestTrackStat.AssetStatus.TrackedTableId = oDestination.UserLinkId;
        //        oDestTrackStat = TrackingServices.GetByTrackedTableAndIDAssetStatus(_iTables, _iDatabas, oDestination.oTable.TableName, oDestination.UserLinkId);
        //        bUpLocs = oDestTrackStat.AssetStatus is not null;
        //    }
        //    else
        //    {
        //        oDestTrackStat = new CustomTrackingStatus();
        //        oDestTrackStat.TrackingStatus = new TrackingStatu();
        //        oDestTrackStat.TrackingStatus.TrackedTable = oDestination.oTable.TableName;
        //        oDestTrackStat.TrackingStatus.TrackedTableId = oDestination.UserLinkId;
        //        oDestTrackStat = TrackingServices.GetByTrackedTableAndIDTrackingStatus(_iTables, _iDatabas, oDestination.oTable.TableName, oDestination.UserLinkId);
        //        bUpLocs = oDestTrackStat.TrackingStatus is not null;
        //    }
        //    if (bDoRecon)
        //    {
        //        oTrackingStatus = new CustomAssetStatus();
        //        oTrackingStatus.AssetStatus = new AssetStatu();
        //        oTrackingStatus.AssetStatus.TrackedTable = oObject.oTable.TableName;
        //        oTrackingStatus.AssetStatus.TrackedTableId = oObject.UserLinkId;
        //        oTrackingStatus = TrackingServices.GetByTrackedTableAndIDAssetStatus(_iTables, _iDatabas, oObject.oTable.TableName, oObject.UserLinkId);
        //        bFoundTrackStatus = oTrackingStatus.AssetStatus is not null;
        //    }
        //    else
        //    {
        //        oTrackingStatus = new CustomTrackingStatus();
        //        oTrackingStatus.TrackingStatus = new TrackingStatu();
        //        oTrackingStatus.TrackingStatus.TrackedTable = oObject.oTable.TableName;
        //        oTrackingStatus.TrackingStatus.TrackedTableId = oObject.UserLinkId;
        //        oTrackingStatus = TrackingServices.GetByTrackedTableAndIDTrackingStatus(_iTables, _iDatabas, oObject.oTable.TableName, oObject.UserLinkId);
        //        bFoundTrackStatus = oTrackingStatus.TrackingStatus is not null;
        //    }
        //    bDoChildren = false;

        //    // See if this is a container

        //    if ((bool)((1 is var arg56 && oObject.oTable.TrackingTable is { } arg55 ? arg55 > arg56 : (bool?)null) & (oObject.oTable.TrackingTable is var arg57 && TrackingContainer[TrackingCount - 1].TrackingTable is { } arg58 && arg57.HasValue ? arg57 <= arg58 : (bool?)null)))
        //    {
        //        if (!string.IsNullOrEmpty(Strings.Trim(oObject.oTable.TrackingStatusFieldName)))
        //        {
        //            bDoChildren = true;
        //        }
        //    }
        //    // If it is a container, does it currently have childern? Create a recordset of all children
        //    if (bDoChildren)
        //    {
        //        if (bDoRecon)
        //        {
        //            oChildTrackStat =  new CustomAssetStatus();
        //            oChildTrackStat.AssetStatus = new AssetStatu();
        //        }
        //        else
        //        {
        //            oChildTrackStat = new CustomTrackingStatus();
        //            oChildTrackStat.TrackingStatus = new TrackingStatu();
        //        }

        //        oChildTrackStat.ContainerColumns[oObject.oTable.TrackingStatusFieldName] = oObject.UserLinkId;

        //        if (bDoRecon)
        //        {
        //            // moApp.TrackingStatus
        //            rsTrackingChildren = TrackingServices.GetRecordSetByContainer(_iDatabas, oObject.oTable.TrackingStatusFieldName, oObject.UserLinkId, Enums.CursorTypeEnum.rmOpenKeyset);
        //        }
        //        else
        //        {
        //            if (!bCurrentScan)
        //            {
        //                sSQL = "SELECT TrackedTableId, TrackedTable, " + oObject.oTable.TrackingStatusFieldName + Constants.vbCrLf;
        //                sSQL += "   FROM TrackingHistory ";
        //                sSQL += " WITH (INDEX(" + oObject.oTable.TrackingStatusFieldName + ")) " + Constants.vbCrLf;
        //                sSQL += "   WHERE " + oObject.oTable.TrackingStatusFieldName + " = '" + oObject.UserLinkId + "'" + Constants.vbCrLf + "   GROUP BY TrackedTableId, TrackedTable, " + oObject.oTable.TrackingStatusFieldName + " " + Constants.vbCrLf;
        //                sSQL += " UNION ";
        //            }

        //            sSQL += "SELECT TrackedTableId, TrackedTable, " + oObject.oTable.TrackingStatusFieldName + Constants.vbCrLf;
        //            sSQL += "    FROM TrackingStatus ";
        //            sSQL += " WITH (INDEX(" + oObject.oTable.TrackingStatusFieldName + ")) " + Constants.vbCrLf;
        //            sSQL += "    WHERE " + oObject.oTable.TrackingStatusFieldName + " = '" + oObject.UserLinkId + "'" + Constants.vbCrLf + "    GROUP BY TrackedTableId, TrackedTable, " + oObject.oTable.TrackingStatusFieldName + " ";

        //            int arglError = 0;
        //            string argsErrorMessage = "";
        //            rsTrackingChildren = DataServices.GetADORecordset(sSQL, null, _iDatabas, lError: ref arglError, sErrorMessage: ref argsErrorMessage);
        //        }
        //        bValid = rsTrackingChildren is not null;
        //        if (!bValid)
        //        {
        //            return UpdateTrackingDataRet;
        //        }
        //    }
        //    // Set the time Note it uses the same for all records created
        //    if (TelxonModeOn)
        //    {
        //        dTime = dScanDateTime;
        //    }
        //    else
        //    {
        //        dTime = DateTime.Now;
        //    }
        //    // Get a TrackingStatus Buffer to save

        //    bDoUpdate = true;
        //    // add new status record or reuse old one
        //    if (bFoundTrackStatus)
        //    {
        //        object oTrackingStatusDateTime;
        //        object oTrackingStatusFieldName;
        //        if (bDoRecon)
        //        {
        //            oNewTrackingStat = new CustomAssetStatus();
        //            oNewTrackingStat.AssetStatus = new AssetStatu();
        //            oNewTrackingStat.AssetStatus.Id = oTrackingStatus.AssetStatus.Id;
        //            oNewTrackingStat.AssetStatus.TrackedTable = oTrackingStatus.AssetStatus.TrackedTable;
        //            oNewTrackingStat.AssetStatus.TrackedTableId = oTrackingStatus.AssetStatus.TrackedTableId;
        //            oTrackingStatusDateTime = oTrackingStatus.AssetStatus.TransactionDateTime;
        //            oTrackingStatusFieldName = oTrackingStatus.AssetStatus.UserName;
        //        }
        //        else
        //        {
        //            oNewTrackingStat = new CustomTrackingStatus();
        //            oNewTrackingStat.TrackingStatus = new TrackingStatu();
        //            oNewTrackingStat.TrackingStatus.Id = oTrackingStatus.TrackingStatus.Id;
        //            oNewTrackingStat.TrackingStatus.TrackedTable = oTrackingStatus.TrackingStatus.TrackedTable;
        //            oNewTrackingStat.TrackingStatus.TrackedTableId = oTrackingStatus.TrackingStatus.TrackedTableId;
        //            oTrackingStatusDateTime = oTrackingStatus.TrackingStatus.TransactionDateTime;
        //            oTrackingStatusFieldName = oTrackingStatus.TrackingStatus.UserName;
        //        }

        //        // Copy the Old One
        //        if (bCheckForDups)
        //        {
        //            if (Strings.StrComp(Convert.ToString(oTrackingStatus.ContainerColumns(oDestination.oTable.TrackingStatusFieldName)), oDestination.UserLinkId, CompareMethod.Text) == 0)
        //            {
        //                sTempStr = "This Object has already been tracked";
        //                sTempStr = sTempStr + " on " + Strings.Format(oTrackingStatusDateTime, DateTime.Now.ToString());
        //                // 1290:               sTempStr = sTempStr & " on " & Format(oTrackingStatus.TrackingStatus.TransactionDateTime, LocaleInfo.GetDefaultDateAndTimeFormat)
        //                sTempStr = Convert.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(sTempStr + " by ", oTrackingStatusFieldName), Constants.vbCrLf), Constants.vbCrLf));
        //                sTempStr = sTempStr + "Object:" + Constants.vbTab + Constants.vbTab + "\"" + oObject.UserDisplay + "\"" + Constants.vbCrLf;
        //                sTempStr = sTempStr + "Destination:" + Constants.vbTab + "\"" + oDestination.UserDisplay + "\"";

        //                if (!bReturnErrors)
        //                {
        //                    sTempStr = sTempStr + Constants.vbCrLf + Constants.vbCrLf + "Would you like to update the Tracking Information with the current date and time?";
        //                    bCheckForDupsReturn = true;
        //                    // UpdateTrackingData = sTempStr
        //                    return sTempStr;
        //                }
        //                // If (Not bDoUpdate) Then
        //                // UpdateTrackingData = sTempStr
        //                // End If
        //                else
        //                {
        //                    UpdateTrackingDataRet = sTempStr;
        //                    bDoUpdate = false;
        //                }
        //            }
        //        }
        //    }
        //    else if (bDoRecon)
        //    {
        //        oNewTrackingStat = new CustomAssetStatus();
        //        oNewTrackingStat.AssetStatus = new AssetStatu();
        //        oNewTrackingStat.AssetStatus.TrackedTableId = oObject.UserLinkId;
        //        oNewTrackingStat.AssetStatus.TrackedTable = oObject.oTable.TableName;
        //    }
        //    else
        //    {
        //        oNewTrackingStat = new CustomTrackingStatus();
        //        oNewTrackingStat.TrackingStatus = new TrackingStatu();
        //        oNewTrackingStat.TrackingStatus.TrackedTableId = oObject.UserLinkId;
        //        oNewTrackingStat.TrackingStatus.TrackedTable = oObject.oTable.TableName;

        //    }

            
        //    object oNewDateDue = null;
        //    object oNewOut = null;
        //    object oNewUserName;
        //    object oNewTransactDateTime;
        //    object oNewProceedDateTime = null;
        //    object oNewField1;
        //    object oNewField2;
        //    if (bDoUpdate)
        //    {

        //        // If have Container status record move its locations across
        //        if (bUpLocs)
        //        {
        //            var loopTo = TrackingCount - 1;
        //            for (iCnt = 0; iCnt <= loopTo; iCnt++)
        //            {
        //                if (oDestTrackStat.ContainerColumns(TrackingContainer[iCnt].TrackingStatusFieldName) is not null & !string.IsNullOrEmpty(Convert.ToString(oDestTrackStat.ContainerColumns(TrackingContainer[iCnt].TrackingStatusFieldName))))
        //                {
        //                    oNewTrackingStat.ContainerColumns[TrackingContainer[iCnt].TrackingStatusFieldName] = oDestTrackStat.ContainerColumns(TrackingContainer[iCnt].TrackingStatusFieldName);
        //                }
        //                else
        //                {
        //                    oNewTrackingStat.ContainerColumns[TrackingContainer[iCnt].TrackingStatusFieldName] = null;
        //                }

        //            }
        //        }
        //        else
        //        {
        //            var loopTo1 = TrackingCount - 1;
        //            for (iCnt = 0; iCnt <= loopTo1; iCnt++)
        //                oNewTrackingStat.ContainerColumns[TrackingContainer[iCnt].TrackingStatusFieldName] = null;
        //        }

        //        vReturnDate = DBNull.Value;
        //        bCurrOut = false;

        //        if ((bool)(oSystem.TrackingOutOn & bDoRecon == false))
        //        {
        //            // Set out field if is going to an "OUT"
        //            if (oDestination.HaveOut)
        //            {
        //                bCurrOut = oDestination.IsOut;
        //            }
        //            else
        //            {
        //                bCurrOut = false; // assume is "IN" if not in something that is an out
        //                if ((bool)(1 is var arg60 && oDestination.oTable.TrackingTable is { } arg59 ? arg59 > arg60 : (bool?)null))
        //                {
        //                    for (iCnt = (int)(oDestination.oTable.TrackingTable - 1); iCnt >= 1; iCnt -= 1)
        //                    {
        //                        oTable = null;
        //                        // On Error Resume Next
        //                        oTable = TrackingContainer[iCnt - 1];
        //                        ;

        //                        if (oTable is not null)
        //                        {
        //                            if ((bool)(Convert.ToInt32(Enums.geTrackingOutType.otAlwaysOut) is var arg62 && oTable.OutTable is { } arg61 ? arg61 == arg62 : (bool?)null))
        //                            {
        //                                bCurrOut = true;
        //                            }
        //                            else if ((bool)(Convert.ToInt32(Enums.geTrackingOutType.otAlwaysIn) is var arg64 && oTable.OutTable is { } arg63 ? arg63 == arg64 : (bool?)null))
        //                            {
        //                                bCurrOut = false;
        //                            }
        //                            // Go find the upper destination record (Assume
        //                            else if (!string.IsNullOrEmpty(Strings.Trim(oTable.TrackingOUTFieldName))) // Use Record Setting
        //                            {
        //                                if (bUpLocs)
        //                                {
        //                                    if (!string.IsNullOrEmpty(Strings.Trim(Convert.ToString(oDestTrackStat.ContainerColumns(oTable.TrackingStatusFieldName)))))
        //                                    {
        //                                        oUpTrackStat = new CustomTrackingStatus();
        //                                        oUpTrackStat.TrackingStatus = new TrackingStatu();
        //                                        oUpTrackStat.TrackingStatus.TrackedTableId = Strings.Trim(Convert.ToString(oDestTrackStat.ContainerColumns(oTable.TrackingStatusFieldName)));
        //                                        oUpTrackStat = TrackingServices.GetByTrackedTableAndIDTrackingStatus(_iTables, _iDatabas, oUpTrackStat.TrackingStatus.TrackedTable, oUpTrackStat.TrackingStatus.TrackedTableId);
        //                                        if (oUpTrackStat.TrackingStatus is not null)
        //                                        {
        //                                            bCurrOut = (bool)oUpTrackStat.TrackingStatus.Out;
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            object oDestDateDue = null;
        //            if (bDoRecon)
        //            {
        //                oNewTrackingStat.AssetStatus.Out = bCurrOut;
        //                if (bUpLocs)
        //                    oDestDateDue = oDestTrackStat.AssetStatus.DateDue;
        //            }
        //            else
        //            {
        //                oNewTrackingStat.TrackingStatus.Out = bCurrOut;
        //                if (bUpLocs)
        //                    oDestDateDue = oDestTrackStat.TrackingStatus.DateDue;
        //            }

        //            if ((bool)oSystem.DateDueOn)
        //            {
        //                // if going "IN" then remove datedue
        //                if (bCurrOut)
        //                {
        //                    if (Information.IsDate(sDueBackDate))
        //                    {
        //                        vReturnDate = Convert.ToDateTime(sDueBackDate);

        //                        if (bUpLocs)
        //                        {
        //                            // Keep the closest in date
        //                            if (!(vReturnDate is DBNull) & vReturnDate is not null & !(oDestDateDue is DBNull) & oDestDateDue is not null)
        //                            {
        //                                if (Operators.ConditionalCompareObjectGreater(Convert.ToDateTime(vReturnDate), oDestDateDue, false))
        //                                {
        //                                    vReturnDate = oDestDateDue;
        //                                }
        //                            }
        //                        }

        //                        if (!(vReturnDate is DBNull) & vReturnDate is not null)
        //                        {
        //                            oNewDateDue = Convert.ToDateTime(vReturnDate);
        //                        }
        //                        else
        //                        {
        //                            oNewDateDue = null;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        oNewDateDue = null;
        //                    }
        //                }
        //                else
        //                {
        //                    oNewDateDue = null;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            oNewOut = false;
        //            oNewDateDue = null;
        //        }
        //        oNewUserName = sUserName;
        //        oNewTrackingStat.ContainerColumns[oDestination.oTable.TrackingStatusFieldName] = oDestination.UserLinkId;
        //        if (dtScanDateTime > DateTime.Parse("1001-01-02"))
        //        {
        //            oNewTransactDateTime = dtScanDateTime;
        //        }
        //        else
        //        {
        //            oNewTransactDateTime = dTime;
        //        }
        //        // oNewProceedDateTime = New Date
        //        oNewField1 = sTrackingAdditionalField1;
        //        oNewField2 = sTrackingAdditionalField2;
                
        //        if (bDoRecon)
        //        {
        //            oNewTrackingStat.AssetStatus.DateDue = oNewDateDue;
        //            oNewTrackingStat.AssetStatus.Out = oNewOut;
        //            oNewTrackingStat.AssetStatus.UserName = oNewUserName;
        //            oNewTrackingStat.AssetStatus.TransactionDateTime = oNewTransactDateTime;
        //            oNewTrackingStat.AssetStatus.ProcessedDateTime = oNewProceedDateTime;
        //            oNewTrackingStat.AssetStatus.TrackingAdditionalField1 = oNewField1;
        //            oNewTrackingStat.AssetStatus.TrackingAdditionalField2 = oNewField2;
        //        }
        //        else
        //        {
        //            oNewTrackingStat.TrackingStatus.DateDue = oNewDateDue;
        //            oNewTrackingStat.TrackingStatus.Out = oNewOut;
        //            oNewTrackingStat.TrackingStatus.UserName = oNewUserName;
        //            oNewTrackingStat.TrackingStatus.TransactionDateTime = oNewTransactDateTime;
        //            oNewTrackingStat.TrackingStatus.ProcessedDateTime = oNewProceedDateTime;
        //            oNewTrackingStat.TrackingStatus.TrackingAdditionalField1 = oNewField1;
        //            oNewTrackingStat.TrackingStatus.TrackingAdditionalField2 = oNewField2;
        //        }
        //        if (bFoundTrackStatus)
        //        {
        //            if (bDoRecon)
        //            {
        //                // If (TypeOf oNewTrackingStat Is CustomAssetStatus) Then
        //                Save(_iTables, _iDatabas, null, null, (CustomAssetStatus)oNewTrackingStat);
        //            }
        //            // Else
        //            // Save(_iTables, _iDatabas, oNewTrackingStat, Nothing, Nothing)
        //            // End If
        //            // If (TypeOf oNewTrackingStat Is CustomAssetStatus) Then
        //            // If (oNewTrackingStat.AssetStatus.TransactionDateTime > oTrackingStatus.AssetStatus.TransactionDateTime) Then
        //            // Save(_iTables, _iDatabas, Nothing, Nothing, oNewTrackingStat)
        //            // End If
        //            // Else
        //            else if (Operators.ConditionalCompareObjectGreater(oNewTrackingStat.TrackingStatus.TransactionDateTime, oTrackingStatus.TrackingStatus.TransactionDateTime, false))
        //            {
        //                Save(_iTables, _iDatabas, (CustomTrackingStatus)oNewTrackingStat);
        //                // End If

        //            }
        //        }
        //        else if (bDoRecon)
        //        {
        //            CustomTrackingStatus argtrackingStatus = null;
        //            CustomTrackingHistory argtrackingHistory = null;
        //            CustomAssetStatus argassetStatus = (CustomAssetStatus)oNewTrackingStat;
        //            TrackingServices.SaveNew(_iTables, _iDatabas, "AssetStatus",ref argtrackingStatus, trackingHistory: ref argtrackingHistory, assetStatus: ref argassetStatus);
        //            oNewTrackingStat = argassetStatus;
        //        }
        //        // oNewTrackingStat = DirectCast(moApp.AssetStatus.SaveNew(oNewTrackingStat), TrackingStatus)
        //        else
        //        {
        //            // If (oNewTrackingStat.TrackingStatus.TransactionDateTime > oTrackingStatus.TrackingStatus.TransactionDateTime) Then
        //            CustomTrackingStatus argtrackingStatus = (CustomTrackingStatus)oNewTrackingStat;
        //            CustomTrackingHistory argtrackingHistory = null;
        //            CustomAssetStatus argassetStatus1 = null;
        //            SaveNew(_iTables, _iDatabas, "TrackingStatus", ref argtrackingStatus, trackingHistory: ref argtrackingHistory, assetStatus: ref argassetStatus1);
        //            oNewTrackingStat = argtrackingStatus;
        //            // End If
        //        }
        //        if (Operators.CompareString(msSignatureFile, "", false) > 0)
        //        {
        //            if (bDoRecon)
        //            {
        //                SaveSignatureToDB(_iDatabas, "AssetStatus", Convert.ToInt32(oNewTrackingStat.AssetStatus.Id), msSignatureFile);
        //            }
        //            else
        //            {
        //                SaveSignatureToDB(_iDatabas, "TrackingStatus", Convert.ToInt32(oNewTrackingStat.TrackingStatus.Id), msSignatureFile);
        //            }

        //        }

        //        if (oNewTrackingStat is null)
        //        {
        //            if (bDoRecon)
        //            {
        //                UpdateTrackingDataRet = "AssetStatus SQL Update Error. Not Updated!";
        //            }
        //            else
        //            {
        //                UpdateTrackingDataRet = "TrackingStatus SQL Update Error. Not Updated!";
        //            }
        //        }

        //        if (oNewTrackingStat is not null)
        //        {
        //            if (bDoRecon == false)
        //            {
        //                RemoveRequests(_iTables, _iDatabas, _iSLRequestor, oNewTrackingStat, Convert.ToString(oNewTrackingStat.TrackingStatus.TrackedTable), Convert.ToString(oNewTrackingStat.TrackingStatus.TrackedTableId));
        //                oTrackingHistory.TrackingHistory.TrackedTable = Convert.ToString(oNewTrackingStat.TrackingStatus.TrackedTable);
        //                oTrackingHistory.TrackingHistory.TrackedTableId = Convert.ToString(oNewTrackingStat.TrackingStatus.TrackedTableId);

        //                // add original history record
        //                // oTrackingHistory = DirectCast(New CustomTrackingHistory, CustomTrackingHistory)
        //                // oTrackingHistory.TrackingHistory = New TrackingHistory
        //                oTrackingHistory.TrackingHistory.BatchId = miBatchNum;
        //                oTrackingHistory.TrackingHistory.IsActualScan = true;

        //                var loopTo2 = TrackingCount - 1;
        //                for (iCnt = 0; iCnt <= loopTo2; iCnt++)
        //                    oTrackingHistory.set_ContainerColumns(TrackingContainer[iCnt].TrackingStatusFieldName, Convert.ToString(oNewTrackingStat.ContainerColumns(TrackingContainer[iCnt].TrackingStatusFieldName)));

        //                oTrackingHistory.TrackingHistory.TransactionDateTime = dTime;
        //                oTrackingHistory.TrackingHistory.UserName = sUserName;
        //                oTrackingHistory.TrackingHistory.TrackingAdditionalField1 = sTrackingAdditionalField1;
        //                oTrackingHistory.TrackingHistory.TrackingAdditionalField2 = sTrackingAdditionalField2;
        //                if (!FromEXE)
        //                {
        //                    oTrackingHistory.TrackingHistory.BatchId = miBatchNum;
        //                }
        //                CustomTrackingStatus argtrackingStatus1 = null;
        //                CustomAssetStatus argassetStatus2 = null;
        //                SaveNew(_iTables, _iDatabas, "TrackingHistory", ref argtrackingStatus1, ref oTrackingHistory, assetStatus: ref argassetStatus2);
        //                oTrackingHistory = oTrackingHistory;

        //                if (Operators.CompareString(msSignatureFile, "", false) > 0)
        //                {
        //                    TrackingServices.SaveSignatureToDB(_iDatabas, "TrackingHistory", oTrackingHistory.TrackingHistory.Id, msSignatureFile);
        //                }

        //                if (oTrackingHistory is null)
        //                {
        //                    UpdateTrackingDataRet = "TrackingHistory SQL Update Error. Not Updated!";
        //                }
        //                // Get the Id of the last record to use as batch number if we need one
        //                else if (miBatchNum == 0)
        //                {
        //                    miBatchNum = oTrackingHistory.TrackingHistory.Id;
        //                    oTrackingHistory.TrackingHistory.BatchId = miBatchNum;
        //                    Save(_iTables, _iDatabas, null, oTrackingHistory);

        //                    if (oTrackingHistory is null)
        //                    {
        //                        UpdateTrackingDataRet = "TrackingHistory (BatchId) SQL Update Error. Not Updated!";
        //                    }
        //                }
        //                // Dim trackingServiceObj As New TrackingServices(_IDBManager, _iTables, _iDatabas, _iScanList, _iTabSet, _iTableTab, _iRelationship, _iView, _iSystem, _iTrackingHistory)
        //                string argKeysType = "";
        //                bool catchFlag = TrackingServices.InnerTruncateTrackingHistory(_iSystem, _iTrackingHistory, oTrackingHistory.TrackingHistory.TrackedTable, oTrackingHistory.TrackingHistory.TrackedTableId, KeysType: ref argKeysType);
        //                // TruncateTrackingHistory(oTrackingHistory.TrackedTable, oTrackingHistory.TrackedTableId)
        //            }

        //            if (bDoChildren)
        //            {
        //                if (rsTrackingChildren is not null)
        //                {
        //                    if (!rsTrackingChildren.EOF & !rsTrackingChildren.BOF)
        //                    {
        //                        // Remember the rsTrackingChildren includes any child ever in the box. Use TrackingHistory to determine if it was when this scan happened. (By looking at the TrackingHistory record just prior to this scan)
        //                        rsTrackingChildren.MoveFirst();

        //                        object oNewTransactDateTimeChange;
        //                        object oNewDateDueChange;
        //                        object oNewOutChange;
        //                        while (!rsTrackingChildren.EOF)
        //                        {
        //                            if (bDoRecon)
        //                            {
        //                                oNewTransactDateTimeChange = oNewTrackingStat.AssetStatus.TransactionDateTime;
        //                                oNewDateDue = oNewTrackingStat.AssetStatus.DateDue;
        //                                oNewOutChange = oNewTrackingStat.AssetStatus.Out;
        //                            }
        //                            else
        //                            {
        //                                oNewTransactDateTimeChange = oNewTrackingStat.TrackingStatus.TransactionDateTime;
        //                                oNewDateDue = oNewTrackingStat.TrackingStatus.DateDue;
        //                                oNewOutChange = oNewTrackingStat.TrackingStatus.Out;
        //                            }
        //                            sSQL = Convert.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT TOP 1 * From TrackingHistory WHERE TrackingHistory.TrackedTableId = '" + rsTrackingChildren.Fields["TrackedTableId"].Value + "' AND TrackingHistory.TrackedTable = '" + rsTrackingChildren.Fields["TrackedTable"].ToString() + "' AND TrackingHistory.TransactionDateTime <= '", oNewTrackingStat.TrackingStatus.TransactionDateTime), "' ORDER BY TrackingHistory.TransactionDateTime DESC"));
        //                            int arglError1 = 0;
        //                            string argsErrorMessage1 = "";
        //                            rsTrackingHistory = DataServices.GetADORecordset(sSQL, null, _iDatabas, lError: ref arglError1, sErrorMessage: ref argsErrorMessage1);

        //                            if (rsTrackingHistory is not null)
        //                            {
        //                                if (!rsTrackingHistory.EOF & !rsTrackingHistory.BOF)
        //                                {
        //                                    var trackingHistoryObj = new CustomTrackingHistory();
        //                                    CustomTrackingStatus argtrackingObj = null;
        //                                    CustomAssetStatus argassetObj = null;
        //                                    SLRequestor argslRequestor = null;
        //                                    LoadObject(_iTables, ref rsTrackingHistory, ref argtrackingObj, ref trackingHistoryObj, assetObj: ref argassetObj, slRequestor: ref argslRequestor);
        //                                    oTrackingHistory = trackingHistoryObj;
        //                                    bInTheContainer = bDoRecon;

        //                                    if (!bInTheContainer)
        //                                    {
        //                                        if (Operators.ConditionalCompareObjectLessEqual(oTrackingHistory.TrackingHistory.TransactionDateTime, oNewTransactDateTimeChange, false))
        //                                        {
        //                                            if (Strings.StrComp(oTrackingHistory.get_ContainerColumns(oObject.oTable.TrackingStatusFieldName), oObject.UserLinkId, CompareMethod.Text) == 0)
        //                                            {
        //                                                bInTheContainer = true;
        //                                            }
        //                                        }
        //                                    }

        //                                    if (bInTheContainer)
        //                                    {
        //                                        // All children records are currently in this location
        //                                        // add children data
        //                                        sSQL = "SELECT * From TrackingStatus WHERE TrackingStatus.TrackedTableId = '" + oTrackingHistory.TrackingHistory.TrackedTableId + "' AND TrackingStatus.TrackedTable = '" + oTrackingHistory.TrackingHistory.TrackedTable + "' ORDER BY TrackingStatus.TransactionDateTime";
        //                                        int arglError2 = 0;
        //                                        string argsErrorMessage2 = "";
        //                                        rsTrackingStatus = DataServices.GetADORecordset(sSQL, null, _iDatabas, lError: ref arglError2, sErrorMessage: ref argsErrorMessage2);

        //                                        if (rsTrackingStatus is not null)
        //                                        {
        //                                            while (!rsTrackingStatus.EOF)
        //                                            {
        //                                                // add upstream status record for children
        //                                                var cTrackingStatus = new CustomTrackingStatus();
        //                                                CustomTrackingHistory argtrackingHisrtory = null;
        //                                                CustomAssetStatus argassetObj1 = null;
        //                                                SLRequestor argslRequestor1 = null;
        //                                                LoadObject(_iTables, ref rsTrackingStatus, ref cTrackingStatus, trackingHisrtory: ref argtrackingHisrtory, assetObj: ref argassetObj1, slRequestor: ref argslRequestor1);
        //                                                oChildTrackStat = cTrackingStatus;

        //                                                var loopTo3 = TrackingCount - 1;
        //                                                for (iCnt = 0; iCnt <= loopTo3; iCnt++)
        //                                                {
        //                                                    if ((bool)(TrackingContainer[iCnt].TrackingTable is var arg65 && oObject.oTable.TrackingTable is { } arg66 && arg65.HasValue ? arg65 < arg66 : (bool?)null))
        //                                                    {
        //                                                        oChildTrackStat.ContainerColumns[TrackingContainer[iCnt].TrackingStatusFieldName] = oNewTrackingStat.ContainerColumns(TrackingContainer[iCnt].TrackingStatusFieldName);
        //                                                    }
        //                                                    else if ((bool)(TrackingContainer[iCnt].TrackingTable is var arg67 && oObject.oTable.TrackingTable is { } arg68 && arg67.HasValue ? arg67 == arg68 : (bool?)null))
        //                                                    {
        //                                                        oChildTrackStat.ContainerColumns[TrackingContainer[iCnt].TrackingStatusFieldName] = oObject.UserLinkId;
        //                                                    }
        //                                                }

        //                                                if ((bool)oSystem.TrackingOutOn)
        //                                                {
        //                                                    oChildTrackStat.TrackingStatus.Out = oNewOutChange;

        //                                                    if ((bool)oSystem.DateDueOn)
        //                                                    {
        //                                                        // if the parent is null let stand whatever it had
        //                                                        if (!(oNewDateDue is DBNull) & !Equals(oNewDateDue, new DateTime()))
        //                                                        {
        //                                                            if (oChildTrackStat.TrackingStatus.DateDue is DBNull | object.Equals(oChildTrackStat.TrackingStatus.DateDue, new DateTime()))
        //                                                            {
        //                                                                oChildTrackStat.TrackingStatus.DateDue = oNewDateDue;
        //                                                            }
        //                                                            // Keep the earliest date
        //                                                            else if (Operators.ConditionalCompareObjectGreater(oChildTrackStat.TrackingStatus.DateDue, oNewDateDue, false))
        //                                                            {
        //                                                                oChildTrackStat.TrackingStatus.DateDue = oNewDateDue;
        //                                                            }
        //                                                        }
        //                                                    }
        //                                                }

        //                                                oChildTrackStat.TrackingStatus.TransactionDateTime = dTime;
        //                                                oChildTrackStat.TrackingStatus.UserName = sUserName;
        //                                                oChildTrackStat.TrackingStatus.ProcessedDateTime = null;
        //                                                LoadTrackingStatusFields(_iTables, (CustomTrackingStatus)oChildTrackStat, ref rsTrackingStatus);
        //                                                Save(_iTables, _iDatabas, (CustomTrackingStatus)oChildTrackStat);
        //                                                // rsTrackingStatus.UpdateBatch(Enums.AffectEnum.rmAffectCurrent)

        //                                                if (oChildTrackStat is null)
        //                                                {
        //                                                    UpdateTrackingDataRet = "TrackingStatus (Children) SQL Update Error. Not Updated!";
        //                                                }
        //                                                if (oChildTrackStat is not null)
        //                                                {
        //                                                    if (bDoRecon == false)
        //                                                    {
        //                                                        CustomTrackingStatus argoChildTrackStat = (CustomTrackingStatus)oChildTrackStat;
        //                                                        UpdateTrackingDataRet = SaveHistoryRecord(_iTables, _iDatabas, _iSystem, _iTrackingHistory, _iSLRequestor, ref argoChildTrackStat, ref sUserName, ref dTime, ref sTrackingAdditionalField1, ref sTrackingAdditionalField2, ref bReturnErrors);
        //                                                        oChildTrackStat = argoChildTrackStat;
        //                                                    }
        //                                                }

        //                                                rsTrackingStatus.MoveNext();
        //                                            }

        //                                            rsTrackingStatus.Close();
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        bFoundPriorHistoryRec = false;
        //                                        oTmpTrackingHistory = new CustomTrackingHistory();

        //                                        while (!rsTrackingHistory.EOF)
        //                                        {
        //                                            // Find First Transaction prior to the Transfer Date
        //                                            var pTrckingHistory = new CustomTrackingHistory();
        //                                            CustomTrackingStatus argtrackingObj1 = null;
        //                                            CustomAssetStatus argassetObj2 = null;
        //                                            SLRequestor argslRequestor2 = null;
        //                                            LoadObject(_iTables, ref rsTrackingHistory, ref argtrackingObj1, ref pTrckingHistory, assetObj: ref argassetObj2, slRequestor: ref argslRequestor2);
        //                                            oTmpTrackingHistory = pTrckingHistory;

        //                                            if (Operators.ConditionalCompareObjectLess(oTmpTrackingHistory.TrackingHistory.TransactionDateTime, oNewTransactDateTimeChange, false))
        //                                            {
        //                                                bFoundPriorHistoryRec = true;

        //                                                if (Strings.StrComp(oTmpTrackingHistory.get_ContainerColumns(oObject.oTable.TrackingStatusFieldName), oObject.UserLinkId, CompareMethod.Text) == 0)
        //                                                {
        //                                                    if (!bDoRecon)
        //                                                    {
        //                                                        oChildTrackStat = new CustomTrackingStatus();

        //                                                        oChildTrackStat.TrackingStatus.TrackedTable = oTmpTrackingHistory.TrackingHistory.TrackedTable;
        //                                                        oChildTrackStat.TrackingStatus.TrackedTableId = oTmpTrackingHistory.TrackingHistory.TrackedTableId;

        //                                                        var loopTo4 = TrackingCount - 1;
        //                                                        for (iCnt = 0; iCnt <= loopTo4; iCnt++)
        //                                                        {
        //                                                            if ((bool)(TrackingContainer[iCnt].TrackingTable is var arg69 && oObject.oTable.TrackingTable is { } arg70 && arg69.HasValue ? arg69 < arg70 : (bool?)null))
        //                                                            {
        //                                                                oChildTrackStat.ContainerColumns[TrackingContainer[iCnt].TrackingStatusFieldName] = oNewTrackingStat.ContainerColumns(TrackingContainer[iCnt].TrackingStatusFieldName);
        //                                                            }
        //                                                            else if ((bool)(TrackingContainer[iCnt].TrackingTable is var arg71 && oObject.oTable.TrackingTable is { } arg72 && arg71.HasValue ? arg71 == arg72 : (bool?)null))
        //                                                            {
        //                                                                oChildTrackStat.ContainerColumns[TrackingContainer[iCnt].TrackingStatusFieldName] = oObject.UserLinkId;
        //                                                            }
        //                                                        }

        //                                                        CustomTrackingStatus argoChildTrackStat1 = (CustomTrackingStatus)oChildTrackStat;
        //                                                        UpdateTrackingDataRet = SaveHistoryRecord(_iTables, _iDatabas,_iSystem, _iTrackingHistory, _iSLRequestor, ref argoChildTrackStat1, ref sUserName, ref dTime, ref sTrackingAdditionalField1, ref sTrackingAdditionalField2, ref bReturnErrors);
        //                                                        oChildTrackStat = argoChildTrackStat1;
        //                                                    }
        //                                                }
        //                                            }

        //                                            rsTrackingHistory.MoveNext();
        //                                        }

        //                                        oTmpTrackingHistory = null;

        //                                        if (!bFoundPriorHistoryRec)
        //                                        {
        //                                            // If we did not find a prior history record assume is in the box because his current TrackingStatus record says so. (Via the Union query above)
        //                                            if (!bDoRecon)
        //                                            {
        //                                                oChildTrackStat = new CustomTrackingStatus();
        //                                                oChildTrackStat.TrackingStatus.TrackedTable = oTrackingHistory.TrackingHistory.TrackedTable;
        //                                                oChildTrackStat.TrackingStatus.TrackedTableId = oTrackingHistory.TrackingHistory.TrackedTableId;
        //                                                oChildTrackStat.ContainerColumns[_iTables.Where(m => m.TableName.Trim().ToLower().Equals(oObject.oTable.TableName.Trim().ToLower())).FirstOrDefault().TrackingStatusFieldName] = oObject.UserLinkId;

        //                                                var loopTo5 = TrackingCount - 1;
        //                                                for (iCnt = 0; iCnt <= loopTo5; iCnt++)
        //                                                {
        //                                                    if ((bool)(TrackingContainer[iCnt].TrackingTable is var arg73 && oObject.oTable.TrackingTable is { } arg74 && arg73.HasValue ? arg73 < arg74 : (bool?)null))
        //                                                    {
        //                                                        oChildTrackStat.ContainerColumns[TrackingContainer[iCnt].TrackingStatusFieldName] = oNewTrackingStat.ContainerColumns(TrackingContainer[iCnt].TrackingStatusFieldName);
        //                                                    }
        //                                                    else if ((bool)(TrackingContainer[iCnt].TrackingTable is var arg75 && oObject.oTable.TrackingTable is { } arg76 && arg75.HasValue ? arg75 == arg76 : (bool?)null))
        //                                                    {
        //                                                        oChildTrackStat.ContainerColumns[TrackingContainer[iCnt].TrackingStatusFieldName] = oObject.UserLinkId;
        //                                                    }
        //                                                }

        //                                                CustomTrackingStatus argoChildTrackStat2 = (CustomTrackingStatus)oChildTrackStat;
        //                                                UpdateTrackingDataRet = SaveHistoryRecord(_iTables, _iDatabas, _iSystem, _iTrackingHistory, _iSLRequestor, ref argoChildTrackStat2, ref sUserName, ref dTime, ref sTrackingAdditionalField1, ref sTrackingAdditionalField2, ref bReturnErrors);
        //                                                oChildTrackStat = argoChildTrackStat2;
        //                                            }
        //                                        }
        //                                    }
        //                                }

        //                                rsTrackingHistory.Close();
        //                            }

        //                            rsTrackingChildren.MoveNext();
        //                        }

        //                        rsTrackingChildren.Close();
        //                        rsTrackingChildren = null;
        //                    }
        //                }
        //            }
        //        }

        //        if ((bool)(Operators.CompareString(oObject.oTable.RetentionFieldName, "", false) > 0 && (oObject.oTable.RetentionPeriodActive | oObject.oTable.RetentionInactivityActive).GetValueOrDefault()))
        //        {
        //            bool bArchiveLocation = false;
        //            bool bInactiveLocation = false;
        //            var bInactiveEligible = default(bool);

        //            if (Operators.CompareString(oDestination.oTable.InactiveLocationField, "", false) > 0)
        //            {
        //                ADODB.Recordset rsLocation;
        //                var sADOCon = DataServices.DBOpen(oTable, _iDatabas);
        //                // Dim IfoDestIdIsString = DataServices.IdFieldIsString(oDestTableCon, oDestination.oTable.TableName, oDestination.oTable.IdFieldName)
        //                sSQL = "SELECT * FROM [" + oDestination.oTable.TableName + "]" + " WHERE [" + DatabaseMap.RemoveTableNameFromField(oDestination.oTable.IdFieldName) + "] = " + TrackingServices.IDQueryValue(oDestination.Id, oDestTableCon, oDestination.oTable);
        //                int arglError3 = 0;
        //                string argsErrorMessage3 = "";
        //                rsLocation = DataServices.GetADORecordset(sSQL, oDestination.oTable, _iDatabas, lError: ref arglError3, sErrorMessage: ref argsErrorMessage3);

        //                if (rsLocation is not null)
        //                {
        //                    if (rsLocation.RecordCount > 0)
        //                    {
        //                        if (Operators.CompareString(oDestination.oTable.InactiveLocationField, string.Empty, false) > 0)
        //                        {
        //                            if (!(rsLocation.Fields[oDestination.oTable.InactiveLocationField] is DBNull))
        //                            {
        //                                bInactiveLocation = Convert.ToBoolean(rsLocation.Fields[oDestination.oTable.InactiveLocationField]);
        //                            }
        //                        }
        //                        if (Operators.CompareString(oDestination.oTable.ArchiveLocationField, string.Empty, false) > 0)
        //                        {
        //                            if (!(rsLocation.Fields[oDestination.oTable.ArchiveLocationField] is DBNull))
        //                            {
        //                                bArchiveLocation = Convert.ToBoolean(rsLocation.Fields[oDestination.oTable.ArchiveLocationField]);
        //                            }
        //                        }
        //                    }
        //                    rsLocation.Close();
        //                    rsLocation = null;
        //                }
        //            }
        //            // If the object is transferred from an "Inactive" Location into an "Active" Location then flag the object based on the Retention Info.

        //            ADODB.Recordset rsADO;
        //            bool IfoObjectIdIsString = DataServices.IdFieldIsString(oObjectTableCon, oObject.oTable.TableName, oObject.oTable.IdFieldName);
        //            sSQL = "SELECT * FROM [" + oObject.oTable.TableName + "]" + " WHERE [" + DatabaseMap.RemoveTableNameFromField(oObject.oTable.IdFieldName) + "] = " + TrackingServices.IDQueryValue(oObject.Id, oObjectTableCon, oObject.oTable);
        //            int arglError4 = 0;
        //            string argsErrorMessage4 = "";
        //            rsADO = DataServices.GetADORecordset(sSQL, oObject.oTable, _iDatabas, lError: ref arglError4, sErrorMessage: ref argsErrorMessage4);

        //            if (rsADO is not null)
        //            {
        //                if (rsADO.RecordCount > 0)
        //                {
        //                    bInactiveEligible = TrackingServices.SetRetentionInactiveFlag(_iDatabas, _iSystem, _iTables, _iSLRetentionCode, _iSLDestructCertItem, oObject.oTable, oObject.Id, rsADO.Fields[oObject.oTable.RetentionFieldName].ToString(), bArchiveLocation);
        //                }
        //                rsADO.Close();
        //                rsADO = null;
        //            }

        //            sSQL = "UPDATE [" + oObject.oTable.TableName + "] SET [%slRetentionInactiveFinal] = " + Math.Abs(Convert.ToInt32(bInactiveLocation)) + "," + " [%slRetentionInactive] = " + Math.Abs(Convert.ToInt32(bInactiveEligible)) + " WHERE [" + DatabaseMap.RemoveTableNameFromField(oObject.oTable.IdFieldName) + "] = " + TrackingServices.IDQueryValue(oObject.Id, oObjectTableCon, oObject.oTable);
        //            DataServices.ProcessADOCommand(ref sSQL, oObjectTableCon, false);
        //            TrackingServices.SetOrRemoveArchiveFlagIfNeeded(_iDatabas, _iSLDestructCertItem, sUserName, oObject, oDestination, oDestinationCon: oDestTableCon, oObjectCon: oObjectTableCon);
        //        }
        //    }

        //    // 6110:       If (miTransferPage = miTotalPages) Then
        //    if (Operators.CompareString(msSignatureFile, "", false) > 0)
        //    {
        //        FileSystem.Kill(msSignatureFile);
        //        msSignatureFile = "";
        //    }
        //    // 6160:       End If

        //    return UpdateTrackingDataRet;
        //}


        private static void SetOrRemoveArchiveFlagIfNeeded(IQueryable<Databas> _iDatabase, IRepository<SLDestructCertItem> _iSLDestCertItem, string sUserName, FoundBarCode oObject, FoundBarCode oDestination, ADODB.Connection oDestinationCon = null, ADODB.Connection oObjectCon = null)
        {
            bool bArchiveLocation;
            Enums.meFinalDispositionStatusType iDispositionStatus;
            Table oCurrentTable;
            string sSQL;

            bArchiveLocation = false;

            oCurrentTable = oObject.oTable;

            if (oCurrentTable is not null)
            {
                if ((bool)(Convert.ToInt32(Enums.meFinalDisposition.fdPermanentArchive) is var arg80 && oCurrentTable.RetentionFinalDisposition is { } arg79 ? arg79 == arg80 : (bool?)null))
                {
                    if (oDestination is not null)
                    {
                        if (oDestination.oTable is not null)
                        {
                            if (Operators.CompareString(oDestination.oTable.ArchiveLocationField, "", false) > 0)
                            {
                                ADODB.Recordset rsLocation;

                                sSQL = "SELECT * FROM [" + oDestination.oTable.TableName + "]" + " WHERE [" + DatabaseMap.RemoveTableNameFromField(oDestination.oTable.IdFieldName) + "] = " + TrackingServices.IDQueryValue(oDestination.Id, oDestinationCon, oDestination.oTable);
                                int arglError = 0;
                                string argsErrorMessage = "";
                                rsLocation = DataServices.GetADORecordset(sSQL, oDestination.oTable, _iDatabase, lError: ref arglError, sErrorMessage: ref argsErrorMessage);

                                if (rsLocation is not null)
                                {
                                    if (rsLocation.RecordCount > 0)
                                    {
                                        if (!(rsLocation.Fields[oDestination.oTable.ArchiveLocationField].Value is DBNull))
                                        {
                                            if (Convert.ToBoolean(rsLocation.Fields[oDestination.oTable.ArchiveLocationField]))
                                            {
                                                bArchiveLocation = true;
                                            }
                                        }

                                    }
                                    rsLocation.Close();
                                    rsLocation = null;
                                }

                                if (bArchiveLocation)
                                {
                                    // If the object is transferred into an "Archive" Location then flag the object as "Archived".
                                    iDispositionStatus = Enums.meFinalDispositionStatusType.fdstArchived;
                                }
                                else
                                {
                                    // If the object is transferred into a non-"Archive" Location then remove the "Archived" flag from the object.
                                    iDispositionStatus = Enums.meFinalDispositionStatusType.fdstActive;
                                }

                                sSQL = "UPDATE [" + oObject.oTable.TableName + "] SET [%slRetentionDispositionStatus] = " + Math.Abs(Convert.ToInt32(bArchiveLocation)) + " WHERE [" + DatabaseMap.RemoveTableNameFromField(oObject.oTable.IdFieldName) + "] = " + TrackingServices.IDQueryValue(oObject.Id, oObjectCon, oObject.oTable);

                                try
                                {
                                    DataServices.ProcessADOCommand(ref sSQL, oObjectCon, false);
                                    var oslDestructCertItem = _iSLDestCertItem.All().Where(m => m.TableName.Trim().ToLower().Equals(oObject.oTable.TableName) & (m.TableId ?? "") == (oObject.Id ?? "")).FirstOrDefault();
                                    // oslDestructCertItem = DirectCast(moApp.SLDestructCertItems.GetByTableNameAndId(oObject.oTable.TableName, oObject.Id), SRME.SLDestructCertItems)

                                    if (oslDestructCertItem.Id == 0)
                                    {
                                        if (bArchiveLocation)
                                        {
                                            oslDestructCertItem.TableName = oObject.oTable.TableName;
                                            oslDestructCertItem.TableId = oObject.Id;
                                            oslDestructCertItem.SLDestructionCertsId = 0;
                                            oslDestructCertItem.LegalHold = false;
                                            oslDestructCertItem.RetentionHold = false;
                                            oslDestructCertItem.HoldReason = string.Empty;
                                            oslDestructCertItem.EventDate = DateTime.Today;

                                            oslDestructCertItem.DispositionFlag = true;
                                            oslDestructCertItem.DispositionDate = DateTime.Today;
                                            oslDestructCertItem.DispositionType = oCurrentTable.RetentionFinalDisposition;
                                            oslDestructCertItem.ApprovedBy = sUserName;
                                            _iSLDestCertItem.Update(oslDestructCertItem);
                                        }
                                    }
                                    else if (bArchiveLocation)
                                    {
                                        oslDestructCertItem.DispositionFlag = true;
                                        oslDestructCertItem.DispositionDate = DateTime.Today;
                                        oslDestructCertItem.DispositionType = oCurrentTable.RetentionFinalDisposition;
                                        oslDestructCertItem.ApprovedBy = sUserName;
                                        _iSLDestCertItem.Update(oslDestructCertItem);
                                    }
                                    else
                                    {
                                        _iSLDestCertItem.Delete(oslDestructCertItem);
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                }
            }
        }

        public static bool SetRetentionInactiveFlag(IQueryable<Databas> _iDatabase, IQueryable<TabFusionRMS.Models.System> _iSystem, IQueryable<Table> _iTable, IQueryable<SLRetentionCode> _iSLRetentionCode, IRepository<SLDestructCertItem> _iSLDestCertItem, Table oCurrentTable, string sID, string sRetentionCode, bool bArchiving)
        {
            bool SetRetentionInactiveFlagRet = default;
            bool bActiveStorage;
            var dInactiveDate = DateTime.MinValue;
            var iCount = default(int);
            CustomTrackingStatus oTrackingStatus = null;
            ADODB.Recordset rsDestructionItems;
            ADODB.Recordset rsLocation;
            ADODB.Recordset rsADO;
            string sSQL;
            var oCurrentTableCon = DataServices.DBOpen();
            if (!string.IsNullOrEmpty(oCurrentTable.DBName))
            {
                var oDatabase = _iDatabase.Where(m => m.DBName.Trim().ToLower().Equals(oCurrentTable.DBName.Trim().ToLower())).FirstOrDefault();
                oCurrentTableCon = DataServices.DBOpen(oDatabase);
            }
            // Update Retention Destruction Code records
            SetRetentionInactiveFlagRet = false;

            if ((bool)(!bArchiving & (Convert.ToInt32(Enums.meFinalDisposition.fdNone) is var arg82 && oCurrentTable.RetentionFinalDisposition is { } arg81 ? arg81 == arg82 : (bool?)null)))
            {
                var oSLDestructCertItem = _iSLDestCertItem.All().Where(m => m.TableName.Trim().ToLower().Equals(oCurrentTable.TableName.Trim().ToLower()) & (m.TableId ?? "") == (sID ?? "")).FirstOrDefault();
                if (oSLDestructCertItem is not null)
                {
                    if (oSLDestructCertItem.Id > 0 && (0 is var arg84 && oSLDestructCertItem.SLDestructionCertsId is { } arg83 ? arg83 > arg84 : (bool?)null).GetValueOrDefault())
                    {
                        sSQL = "SELECT COUNT(Id) AS TotalCount FROM [SLDestructCertItems] WHERE [SLDestructionCertsId] = " + oSLDestructCertItem.SLDestructionCertsId;
                        int arglError = 0;
                        string argsErrorMessage = "";
                        rsDestructionItems = DataServices.GetADORecordset(sSQL, null, _iDatabase, lError: ref arglError, sErrorMessage: ref argsErrorMessage);

                        if (rsDestructionItems is not null)
                        {
                            if (!rsDestructionItems.EOF)
                                iCount = iCount + Convert.ToInt32(rsDestructionItems.Fields["TotalCount"]);
                            rsDestructionItems.Close();
                            rsDestructionItems = null;
                        }

                        if (iCount == 1)
                        {
                            _iSLDestCertItem.Delete(oSLDestructCertItem);
                        }
                    }

                    if (string.Compare(sRetentionCode, oSLDestructCertItem.RetentionCode, true) != 0)
                    {
                        if ((bool)((false is var arg88 && oSLDestructCertItem.LegalHold is { } arg87 ? arg87 == arg88 : null) & (false is var arg90 && oSLDestructCertItem.RetentionHold is { } arg89 ? arg89 == arg90 : null)))
                        {
                            // If the code has been changed and the Destruction Cert Item is not on hold, then delete the record.
                            _iSLDestCertItem.Delete(oSLDestructCertItem);
                        }
                        // otherwise if the code has changed, update the item
                        else if (Operators.CompareString(sRetentionCode, "", false) > 0)
                        {
                            oSLDestructCertItem.RetentionCode = sRetentionCode;
                            oSLDestructCertItem.SLDestructionCertsId = 0;
                            _iSLDestCertItem.Update(oSLDestructCertItem);
                        }
                    }
                }


                oSLDestructCertItem = null;
            }
            // Set the Retention Inactivity Flag
            sSQL = string.Format("SELECT * FROM [{0}] WHERE [{1}] = {2}", oCurrentTable.TableName, DatabaseMap.RemoveTableNameFromField(oCurrentTable.IdFieldName), TrackingServices.IDQueryValue(sID, oCurrentTableCon, oCurrentTable));
            int arglError1 = 0;
            string argsErrorMessage1 = "";
            rsADO = DataServices.GetADORecordset(sSQL, oCurrentTable, _iDatabase, lError: ref arglError1, sErrorMessage: ref argsErrorMessage1);

            if (rsADO is not null)
            {
                if (!rsADO.EOF)
                {
                    bool IfIdFieldIsString = DataServices.IdFieldIsString(oCurrentTableCon, oCurrentTable.TableName, oCurrentTable.IdFieldName);
                    if (!string.IsNullOrEmpty(rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oCurrentTable.RetentionFieldName)].ToString()))
                    {
                        string TempVal = rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oCurrentTable.RetentionFieldName)].ToString();
                        var oRetentionCode = _iSLRetentionCode.Where(m => m.Id.Trim().Equals(TempVal)).FirstOrDefault();
                        // Dim oRetentionCode As SLRetentionCodes = DirectCast(moApp.SLRetentionCodes.GetByKey(rsADO.Fields(moApp.Data.RemoveTableNameFromField(oCurrentTable.RetentionFieldName)).Value.ToString), SLRetentionCodes)

                        if (oRetentionCode is not null)
                        {
                            if (oRetentionCode.InactivityEventType == "Date Opened")
                            {
                                if (!(rsADO.Fields[oCurrentTable.RetentionDateOpenedField] is DBNull))
                                    dInactiveDate = Convert.ToDateTime(rsADO.Fields[oCurrentTable.RetentionDateOpenedField]);
                            }
                            else if (oRetentionCode.InactivityEventType == "Date Closed")
                            {
                                if (!(rsADO.Fields[oCurrentTable.RetentionDateClosedField] is DBNull))
                                    dInactiveDate = Convert.ToDateTime(rsADO.Fields[oCurrentTable.RetentionDateClosedField]);
                            }
                            else if (oRetentionCode.InactivityEventType == "Date Created")
                            {
                                if (!(rsADO.Fields[oCurrentTable.RetentionDateCreateField] is DBNull))
                                    dInactiveDate = Convert.ToDateTime(rsADO.Fields[oCurrentTable.RetentionDateCreateField]);
                            }
                            else if (oRetentionCode.InactivityEventType == "Date Other")
                            {
                                if (!(rsADO.Fields[oCurrentTable.RetentionDateOtherField] is DBNull))
                                    dInactiveDate = Convert.ToDateTime(rsADO.Fields[oCurrentTable.RetentionDateOtherField]);
                            }
                            else if (oRetentionCode.InactivityEventType == "Date Last Tracked")
                            {
                                if (IfIdFieldIsString)
                                {
                                    oTrackingStatus = TrackingServices.GetByTrackedTableAndIDTrackingStatus(_iTable, _iDatabase, oCurrentTable.TableName, rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oCurrentTable.IdFieldName)].ToString());
                                }
                                else
                                {
                                    oTrackingStatus = GetByTrackedTableAndIDTrackingStatus(_iTable, _iDatabase, oCurrentTable.TableName, TrackingServices.ZeroPaddedString(oCurrentTable, rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oCurrentTable.IdFieldName)].ToString(), _iDatabase));
                                }

                                if (oTrackingStatus.TrackingStatus.Id != 0)
                                {
                                    dInactiveDate = (DateTime)oTrackingStatus.TrackingStatus.TransactionDateTime;
                                }
                                else
                                {
                                    oTrackingStatus = null;
                                }
                            }

                            if (dInactiveDate != DateTime.MinValue)
                            {
                                bActiveStorage = true;
                                var oLocation = _iTable.Where(m => m.TrackingTable == TrackingServices.TRACKING_LOCATION).FirstOrDefault();
                                var oLocationCon = DataServices.DBOpen();

                                if (Operators.CompareString(oLocation.InactiveLocationField, "", false) > 0 & Operators.CompareString(oLocation.TrackingStatusFieldName, "", false) > 0)
                                {
                                    if (oTrackingStatus is null)
                                    {
                                        if (IfIdFieldIsString)
                                        {
                                            oTrackingStatus = TrackingServices.GetByTrackedTableAndIDTrackingStatus(_iTable, _iDatabase, oCurrentTable.TableName, rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oCurrentTable.IdFieldName)].ToString());
                                        }
                                        else
                                        {
                                            oTrackingStatus = GetByTrackedTableAndIDTrackingStatus(_iTable, _iDatabase, oCurrentTable.TableName, TrackingServices.ZeroPaddedString(oCurrentTable, rsADO.Fields[DatabaseMap.RemoveTableNameFromField(oCurrentTable.IdFieldName)].ToString(), _iDatabase));
                                        }
                                        if (oTrackingStatus.TrackingStatus is not null)
                                        {
                                            if (oTrackingStatus.TrackingStatus.Id == 0)
                                                oTrackingStatus = null;
                                        }

                                    }

                                    if (oTrackingStatus is not null)
                                    {
                                        if (Operators.CompareString(oTrackingStatus.get_ContainerColumns(oLocation.TrackingStatusFieldName), "", false) > 0)
                                        {
                                            sSQL = string.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = {3}", oLocation.InactiveLocationField, oLocation.TableName, oLocation.IdFieldName.Replace(".", "].["), TrackingServices.IDQueryValue(oTrackingStatus.get_ContainerColumns(oLocation.TrackingStatusFieldName), oLocationCon, oLocation));
                                            int arglError2 = 0;
                                            string argsErrorMessage2 = "";
                                            rsLocation = DataServices.GetADORecordset(sSQL, oLocation, _iDatabase, lError: ref arglError2, sErrorMessage: ref argsErrorMessage2);

                                            if (rsLocation is not null)
                                            {
                                                if (!rsLocation.EOF)
                                                    bActiveStorage = !Convert.ToBoolean(rsLocation.Fields[0]);
                                                rsLocation.Close();
                                                rsLocation = null;
                                            }
                                        }
                                    }
                                }

                                oTrackingStatus = null;
                                // If (bActiveStorage) Then
                                if (dInactiveDate != DateTime.MinValue)
                                {
                                    dInactiveDate = ApplyYearEndToDate(_iSystem, dInactiveDate, (double)oRetentionCode.InactivityPeriod, (bool)oRetentionCode.InactivityForceToEndOfYear);
                                    SetRetentionInactiveFlagRet = dInactiveDate.CompareTo(DateTime.Now) < 0 | !bActiveStorage;
                                }
                            }

                            oRetentionCode = null;
                        }
                    }


                    rsADO.Close();
                    rsADO = null;
                }
            }

            return SetRetentionInactiveFlagRet;
        }

        public static string IDQueryValue(string sID, ADODB.Connection sADOCon, Table oTable)
        {

            bool IfIdFieldIsString = DataServices.IdFieldIsString(sADOCon, oTable.TableName, oTable.IdFieldName);
            if (IfIdFieldIsString)
            {
                if (sID is null)
                    sID = string.Empty;
                return "'" + sID.Replace("'", "''") + "'";
            }

            if (sID is null)
                sID = "0";
            return sID;
        }

        public static DateTime ApplyYearEndToDate(IQueryable<TabFusionRMS.Models.System> _iSystem, DateTime ApplyToDate, double YearsToAdd, bool ForceToEndOfYear)
        {
            var workingDate = ApplyToDate.AddMonths((int)Math.Round(YearsToAdd * 12d));
            return clsNavigation.ApplyYearEndToDate(workingDate, ForceToEndOfYear, (int)_iSystem.OrderBy(m => m.Id).FirstOrDefault().RetentionYearEnd);
        }

        private static string SaveHistoryRecord(IQueryable<Table> _iTables, IQueryable<Databas> _iDatabase, IQueryable<TabFusionRMS.Models.System> _iSystem, IRepository<TrackingHistory> _iTrackingHistory, IRepository<SLRequestor> _iSLRequestor, ref CustomTrackingStatus oChildTrackStat, ref string sUserName, ref DateTime dTime, ref string sTrackingAdditionalField1, ref string sTrackingAdditionalField2, ref bool bReturnErrors)
        {
            string SaveHistoryRecordRet = default;
            int iCnt;
            CustomTrackingHistory oTrackingHistory;
            var TrackingContainer = _iTables.Where(m => m.TrackingTable > 0).ToList();
            int TrackingCount = 0;
            if (TrackingContainer is not null)
            {
                TrackingCount = TrackingContainer.Count;
            }
            // On Error GoTo lbl_Error

            SaveHistoryRecordRet = string.Empty;
            TrackingServices.RemoveRequests(_iTables, _iDatabase, _iSLRequestor, oChildTrackStat, oChildTrackStat.TrackingStatus.TrackedTable, oChildTrackStat.TrackingStatus.TrackedTableId);
            // Dim mTrackingHistory As New CustomTrackingHistory
            oTrackingHistory = new CustomTrackingHistory();
            oTrackingHistory.TrackingHistory = new TrackingHistory();
            oTrackingHistory.TrackingHistory.BatchId = miBatchNum;
            oTrackingHistory.TrackingHistory.IsActualScan = false;
            oTrackingHistory.TrackingHistory.TrackedTableId = oChildTrackStat.TrackingStatus.TrackedTableId;
            oTrackingHistory.TrackingHistory.TrackedTable = oChildTrackStat.TrackingStatus.TrackedTable;

            var loopTo = TrackingCount - 1;
            for (iCnt = 0; iCnt <= loopTo; iCnt++)
                oTrackingHistory.set_ContainerColumns(TrackingContainer[iCnt].TrackingStatusFieldName, oChildTrackStat.get_ContainerColumns(TrackingContainer[iCnt].TrackingStatusFieldName));

            oTrackingHistory.TrackingHistory.TransactionDateTime = dTime;
            oTrackingHistory.TrackingHistory.UserName = sUserName;
            oTrackingHistory.TrackingHistory.TrackingAdditionalField1 = sTrackingAdditionalField1;
            oTrackingHistory.TrackingHistory.TrackingAdditionalField2 = sTrackingAdditionalField2;
            CustomTrackingStatus argtrackingStatus = null;
            CustomAssetStatus argassetStatus = null;
            SaveNew(_iTables, _iDatabase, "TrackingHistory", ref argtrackingStatus, ref oTrackingHistory, assetStatus: ref argassetStatus);
            // oTrackingHistory = DirectCast(oTrackingHistory, CustomTrackingHistory)

            if (oTrackingHistory is null)
            {
                SaveHistoryRecordRet = Languages.get_Translation("msgImportCtrlTrackingHistoryError");
            }
            // If (Not bReturnErrors) Then
            // AppMsgBox("TrackingHistory (Children) SQL Update Error. Not Updated!", MsgBoxStyle.OkOnly Or MsgBoxStyle.Exclamation, System.Reflection.Assembly.GetExecutingAssembly.GetName.Name)
            // End If
            else
            {
                // Dim trackingServiceObj As New TrackingServices(_IDBManager, _iTables, _iDatabas, _iScanList, _iTabSet, _iTableTab, _iRelationship, _iView, _iSystem, _iTrackingHistory)
                string argKeysType = "";
                bool catchFlag = TrackingServices.InnerTruncateTrackingHistory(_iSystem, _iTrackingHistory, oTrackingHistory.TrackingHistory.TrackedTable, oTrackingHistory.TrackingHistory.TrackedTableId, KeysType: ref argKeysType);
                // TruncateTrackingHistory(oTrackingHistory.TrackedTable, oTrackingHistory.TrackedTableId)
            }

            return SaveHistoryRecordRet;
        }

        #region Transfer/BarCode Tracking/Import

        public static bool IsOutDestination(string oDestinationTable, string oDestinationId, HttpContext httpContext)
        {
            IDBManager _IDBManagerDestination = null;
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                IRepository<Table> _iTables = new Repositories<Table>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                var oSystem = _iSystem.All().FirstOrDefault();
                var oDestTable = _iTables.Where(m => m.TableName.Trim().ToLower().Equals(oDestinationTable.Trim().ToLower())).FirstOrDefault();
                var outType = default(bool);
                if (oSystem.TrackingOutOn == true && oSystem.DateDueOn == true)
                {
                    switch (oDestTable.OutTable)
                    {
                        case 0:
                            {
                                try
                                {
                                    _IDBManagerDestination = new ImportDBManager();
                                    if (oDestTable.DBName is not null)
                                    {
                                        var oDatabase = _iDatabas.Where(m => m.DBName.Trim().ToLower().Equals(oDestTable.DBName.Trim().ToLower())).FirstOrDefault();
                                        _IDBManagerDestination.ConnectionString = Keys.get_GetDBConnectionString(oDatabase);
                                        _IDBManagerDestination.Open();
                                    }
                                    else
                                    {
                                        _IDBManagerDestination.ConnectionString = Keys.get_GetDBConnectionString();
                                        _IDBManagerDestination.Open();
                                    }
                                    string sSQL = string.Format("SELECT [{0}] FROM [{1}] WHERE [{2}]='{3}'", oDestTable.TrackingOUTFieldName, oDestTable.TableName, DatabaseMap.RemoveTableNameFromField(oDestTable.IdFieldName), oDestinationId);
                                    outType = Convert.ToBoolean(GetInfoUsingADONET.GetScalarUsingADONET(sSQL, _IDBManagerDestination));
                                }
                                catch
                                {
                                    outType = false;
                                }  // No out field, assume always in

                                break;
                            }

                        case 1:
                            {
                                outType = true;
                                break;
                            }

                        case 2:
                            {
                                outType = false;
                                break;
                            }
                    }
                }
                else
                {
                    outType = false;
                }
                return outType;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_IDBManagerDestination is not null)
                {
                    if (_IDBManagerDestination.Connection is not null)
                    {
                        _IDBManagerDestination.Dispose();
                    }
                }
            }
        }

        public static DateTime GetDueBackDate(string oDestinationTable, string oDestinationId, HttpContext httpContext)
        {
            IDBManager _IDBManagerDestination = null;
            try
            {
                IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
                IRepository<Table> _iTables = new Repositories<Table>();
                IRepository<Databas> _iDatabas = new Repositories<Databas>();
                var oDestTable = _iTables.Where(m => m.TableName.Trim().ToLower().Equals(oDestinationTable.Trim().ToLower())).FirstOrDefault();
                int oDueBackDaysInt = 0;
                if (Strings.Len(oDestTable.TrackingDueBackDaysFieldName) > 0)
                {
                    _IDBManagerDestination = new ImportDBManager();
                    if (oDestTable.DBName is not null)
                    {
                        var oDatabase = _iDatabas.Where(m => m.DBName.Trim().ToLower().Equals(oDestTable.DBName.Trim().ToLower())).FirstOrDefault();
                        _IDBManagerDestination.ConnectionString = Keys.get_GetDBConnectionString( oDatabase);
                        _IDBManagerDestination.Open();
                    }
                    else
                    {
                        _IDBManagerDestination.ConnectionString = Keys.get_GetDBConnectionString();
                        _IDBManagerDestination.Open();
                    }
                    string sSQL = string.Format("SELECT [{0}] FROM [{1}] WHERE [{2}]='{3}'", oDestTable.TrackingDueBackDaysFieldName, oDestTable.TableName, DatabaseMap.RemoveTableNameFromField(oDestTable.IdFieldName), oDestinationId);
                    var oDueBackDays = GetInfoUsingADONET.GetScalarUsingADONET(sSQL, _IDBManagerDestination);
                    if (!(oDueBackDays is DBNull))
                    {
                        oDueBackDaysInt = Convert.ToInt32(oDueBackDays);
                    }
                }
                if (oDueBackDaysInt <= 0)
                    oDueBackDaysInt = (int)_iSystem.All().OrderBy(m => m.Id).FirstOrDefault().DefaultDueBackDays;
                return DateTime.Now.AddDays(oDueBackDaysInt);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_IDBManagerDestination is not null)
                {
                    if (_IDBManagerDestination.Connection is not null)
                    {
                        _IDBManagerDestination.Dispose();
                    }
                }
            }
        }

        public static void PrepareDataForTransfer(string trackableType, string trackableID, string destinationType, 
            string destinationID, DateTime DueBackDate, string userName, Passport passport, HttpContext httpContext, string trackingAdditionalField1 = null, string trackingAdditionalField2 = null)
        {

            IRepository<Table> _iTables = new Repositories<Table>();
            IRepository<Databas> _iDatabas = new Repositories<Databas>();
            IRepository<TrackingStatu> _iTrackingStatu = new Repositories<TrackingStatu>();
            IRepository<AssetStatu> _iAssetStatu = new Repositories<AssetStatu>();
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            IDBManager _IDBManagerDefault = new ImportDBManager();
            IDBManager _IDBManagerObject = null;
            IDBManager _IDBManagerDestination = null;
            string oDestinationId = null;
            string oObjectId = null;
            Table objectTable = null;
            Table destTable = null;
            try
            {
                _IDBManagerDefault.ConnectionString = Keys.get_GetDBConnectionString();
                _IDBManagerDefault.Open();

                objectTable = _iTables.Where(m => m.TableName.Trim().ToLower().Equals(trackableType.Trim().ToLower())).FirstOrDefault();
                destTable = _iTables.Where(m => m.TableName.Trim().ToLower().Equals(destinationType.Trim().ToLower())).FirstOrDefault();
                if (objectTable.DBName is not null)
                {
                    var oDatabase = _iDatabas.Where(m => m.DBName.Trim().ToLower().Equals(objectTable.DBName.Trim().ToLower())).FirstOrDefault();
                    _IDBManagerObject = new ImportDBManager();
                    _IDBManagerObject.ConnectionString = Keys.get_GetDBConnectionString(oDatabase);
                    _IDBManagerObject.Open();
                }
                else
                {
                    _IDBManagerObject = _IDBManagerDefault;
                }
                if (destTable.DBName is not null)
                {
                    var oDatabase = _iDatabas.Where(m => m.DBName.Trim().ToLower().Equals(destTable.DBName.Trim().ToLower())).FirstOrDefault();
                    _IDBManagerDestination = new ImportDBManager();
                    _IDBManagerDestination.ConnectionString = Keys.get_GetDBConnectionString(oDatabase);
                    _IDBManagerDestination.Open();
                }
                else
                {
                    _IDBManagerDestination = _IDBManagerDefault;
                }
                bool IfObjIdFieldIsString = GetInfoUsingADONET.IdFieldIsString(_IDBManagerObject, objectTable.TableName, objectTable.IdFieldName);
                if (!IfObjIdFieldIsString)
                {
                    int oUserLinkTableIdSize = GetInfoUsingADONET.UserLinkIndexTableIdSize(_IDBManagerDefault);
                    oObjectId = Strings.Right(new string('0', oUserLinkTableIdSize) + trackableID, oUserLinkTableIdSize);
                }
                else
                {
                    oObjectId = trackableID;
                }

                bool IfDestIdFieldIsString = GetInfoUsingADONET.IdFieldIsString(_IDBManagerDestination, destTable.TableName, destTable.IdFieldName);
                if (!IfDestIdFieldIsString)
                {
                    int oUserLinkTableIdSize = GetInfoUsingADONET.UserLinkIndexTableIdSize(_IDBManagerDefault);
                    oDestinationId = Strings.Right(new string('0', oUserLinkTableIdSize) + destinationID, oUserLinkTableIdSize);
                }
                else
                {
                    oDestinationId = destinationID;
                }


                DoTransfer(trackableType, oObjectId, destinationType, oDestinationId, false, DueBackDate, DateTime.Now, trackingAdditionalField1, trackingAdditionalField2, userName, _iTables, _iTrackingStatu, _iAssetStatu, _iSystem, _iDatabas, _IDBManagerDefault, passport);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_IDBManagerDefault.Connection is not null)
                {
                    _IDBManagerDefault.Dispose();
                }
                if (objectTable.DBName is not null)
                {
                    _IDBManagerObject.Dispose();
                }
                if (destTable.DBName is not null)
                {
                    _IDBManagerDestination.Dispose();
                }
            }
        }

        public static void PrepareTransferDataForImport(IDBManager DefaultIDBManager, 
            IRepository<TabFusionRMS.Models.System> iSystem,
            IRepository<SecureUser> iSecureUser,
            IRepository<Table> iTables,
            IRepository<TrackingStatu> iTrackingStatu,
            IRepository<AssetStatu> iAssetStatu,
            IRepository<Databas> iDatabase,
            FoundBarCode oObject, FoundBarCode oDestination,
            bool bDoRecon, DateTime sDueBackDate, Passport passport,
            [Optional, DefaultParameterValue("")] string sTrackingAdditionalField1,
            [Optional, DefaultParameterValue("")] string sTrackingAdditionalField2,
            [Optional, DateTimeConstant(315569088000000000L/* #1/1/1001# */)] DateTime dtScanDateTime, 
            [Optional, DefaultParameterValue("")] ref string sTrackingReturn)
        {
            // UpdateTrackingData = ""
            try
            {
                bool bCurrOut;
                DateTime dTime;
                var dScanDateTime = ScanDateTime;
                string sUserName = null;
                int sUserId = 0;
                var oSystem = iSystem.All().FirstOrDefault();

                var IsObjectTrackable = iTables.Where(m => m.TableName.Trim().ToLower().Equals(oObject.oTable.TableName.Trim().ToLower())).FirstOrDefault().Trackable;
                if ((bool)!IsObjectTrackable)
                {
                    sTrackingReturn = string.Format(Languages.get_Translation("msgTransfersNotAuthToTransfer"));
                    return;
                }

                var DestContainerOrder = iTables.Where(m => m.TableName.Trim().ToLower().Equals(oDestination.oTable.TableName.Trim().ToLower())).FirstOrDefault().TrackingTable;
                if ((bool)(0 is var arg100 && DestContainerOrder is { } arg99 ? arg99 <= arg100 : (bool?)null))
                {
                    sTrackingReturn = string.Format(Languages.get_Translation("msgImportCtrlNotValidContainer"));
                    return;
                }
                // Decide what user name to use
                if (TelxonModeOn)
                {
                    sUserName = TelxonUserName;
                    dTime = dScanDateTime;
                }
                else
                {
                    dTime = DateTime.Now;
                    sUserId = passport.UserId;
                    if (sUserId != 0)
                    {
                        sUserName = iSecureUser.Where(m => m.UserID == sUserId).FirstOrDefault().UserName;
                    }
                }
                DateTime oNewTransactDateTime = default;
                bCurrOut = false;
                if ((bool)(oSystem.TrackingOutOn & bDoRecon == false))
                {
                    if (oDestination.HaveOut)
                    {
                        bCurrOut = oDestination.IsOut;
                    }
                    if ((bool)oSystem.DateDueOn)
                    {
                        // if going "IN" then remove datedue
                        if (bCurrOut == false)
                        {
                            sDueBackDate = default;
                        }
                    }
                    else
                    {
                        sDueBackDate = default;
                    }
                }
                else
                {
                    sDueBackDate = default;
                }

                if (dtScanDateTime > DateTime.Parse("1001-01-02"))
                {
                    oNewTransactDateTime = dtScanDateTime;
                }
                else
                {
                    oNewTransactDateTime = dTime;
                }

                TrackingServices.DoTransfer(oObject.oTable.TableName, oObject.UserLinkId, oDestination.oTable.TableName, oDestination.UserLinkId, bDoRecon, sDueBackDate, oNewTransactDateTime, sTrackingAdditionalField1, sTrackingAdditionalField2, sUserName, iTables, iTrackingStatu, iAssetStatu, iSystem, iDatabase, DefaultIDBManager, passport);
            }
            catch (Exception)
            {
                throw;
            }

        }

        private static void DoTransfer(string strObjectTableName, string strObjectTableId, string strDestinationTableName, string strDestinationTableId, bool bIsReconciliationOn, DateTime? dtDueDate, DateTime? dtTransactionDateTime, string strTrackingAdditionalField1, string strTrackingAdditionalField2, string strUserName, IRepository<Table> iTables, IRepository<TrackingStatu> iTrackingStatu, IRepository<AssetStatu> iAssetStatu, IRepository<TabFusionRMS.Models.System> iSystem, IRepository<Databas> iDatabase, IDBManager DefaultIDBManager, Passport passport)
        {
            try
            {
                clsTracking.Transfer(strObjectTableName, strObjectTableId, strDestinationTableName, strDestinationTableId, (DateTime)dtDueDate, strUserName, passport, strTrackingAdditionalField1, strTrackingAdditionalField2, (DateTime)dtTransactionDateTime);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void EmailDelivery(IRepository<Table> iTables, IRepository<TabFusionRMS.Models.System> iSystem, Smead.Security.Passport passport, bool IsWaitListMail, string trackableType, string destinationType, string trackableId, string moDestinationId, DateTime DueBackDate = default, string instruction = "")
        {
            try
            {
                string message = string.Empty;
                string title = string.Empty;
                bool IsAllow;
                if (IsWaitListMail)
                {
                    IsAllow = (bool)iSystem.All().FirstOrDefault().EMailWaitListEnabled;
                    message = "The following item(s) have been requested by: ";
                    title = "Wait List Notification";
                }
                else
                {
                    IsAllow = (bool)iSystem.All().FirstOrDefault().EMailDeliveryEnabled;
                    message = "The following item(s) are en route to: ";
                    title = "Delivery Notification";
                }
                if (IsAllow == true)
                {
                    var RequestorTable = iTables.Where(m => m.TrackingTable == 2).FirstOrDefault();
                    if (RequestorTable is not null)
                    {
                        if ((destinationType ?? "") == (RequestorTable.TableName.Trim() ?? ""))
                        {

                            string employeeID = moDestinationId;
                            var employee = new Smead.RecordsManagement.Employee(passport.Connection());
                            employee.LoadByID(employeeID, passport);
                            if (IsWaitListMail)
                            {
                                message = message + employee.Description + Environment.NewLine + Environment.NewLine + "Current Location: ";
                                string tempTrackableId = Strings.Right(new string('0', DatabaseMap.UserLinkIndexTableIdSize) + trackableId, DatabaseMap.UserLinkIndexTableIdSize);
                                message = message + clsTracking.GetTrackedItemLocationDescription(passport, trackableType, tempTrackableId) + Environment.NewLine + Environment.NewLine;
                            }
                            else
                            {
                                message = message + employee.Description + Environment.NewLine + Environment.NewLine;
                            }

                            message = message + "Item             Description                       Due Back" + Environment.NewLine;
                            message = message + "-----------------------------------------------------------" + Environment.NewLine;
                            string item = trackableType + ": " + StripLeadingZeros(trackableId);
                            string description = clsNavigation.GetItemName(trackableType, trackableId, passport);
                            string dueBack = DueBackDate.ToShortDateString();
                            int firstlength = 17 - item.Length;
                            int secondlength = 0;
                            if (firstlength < 0)
                            {
                                secondlength = 50 - (description.Length + 17 - firstlength);
                                firstlength = 0;
                            }
                            else
                            {
                                secondlength = 50 - (description.Length + 17);
                            }
                            if (secondlength < 0)
                                secondlength = 0;
                            message = message + item + new string(' ', firstlength) + description + new string(' ', secondlength) + dueBack;
                            if (IsWaitListMail)
                            {
                                if (!string.IsNullOrEmpty(instruction))
                                {
                                    message = message + Environment.NewLine + "Request Instructions: " + instruction;
                                }
                            }
                            else
                            {
                                var waitLists = Requesting.GetActiveRequests(trackableType, trackableId, passport);
                                if (waitLists.Count > 0)
                                {
                                    string waitListMessage = Environment.NewLine + Environment.NewLine + "                WaitList                                      Requested";
                                    waitListMessage = waitListMessage + Environment.NewLine + "                -------------------------------------------------------";
                                    foreach (Request req in waitLists)
                                    {
                                        waitListMessage = waitListMessage + Environment.NewLine + new string(' ', 16);
                                        string employeeDesc = clsNavigation.GetItemName(destinationType, req.EmployeeID, passport);
                                        string reqDate = req.DateRequested.ToShortDateString();
                                        int length = 55 - employeeDesc.Length - reqDate.Length;
                                        if (length < 0)
                                            length = 1;
                                        waitListMessage = waitListMessage + employeeDesc + new string(' ', length) + reqDate;
                                    }
                                    message = message + waitListMessage;
                                }
                            }
                            try
                            {
                                clsNavigation.SendEmail(message, clsNavigation.GetEmployeeEmailByID(employeeID, passport), clsNavigation.GetUserEmail(passport), title, "", passport.Connection());
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                    }
                }
            }

            catch (Exception)
            {
                throw;
            }
        }

        public static DataSet UpdateTrackingStatus(string strObjectTableName, string strObjectTableId, string strDestinationTableName, 
            string strDestinationTableId, bool bIsReconciliationOn, DateTime? dtDueDate, DateTime? dtTransactionDateTime, bool intOutField,
            string strTrackingAdditionalField1, string strTrackingAdditionalField2, string strUserName, IRepository<Table> _iTables,
            IRepository<TrackingStatu> _iTrackingStatus, IRepository<AssetStatu> _iAssetStatus, IDBManager DefaultIDBManager, HttpContext httpContext)
        {
            try
            {
                string strSQL = "";
                // Create temp table for strore object ids as well as existing record in tracking status table for current object
                string strCreateTempTableSQL = "";
                strCreateTempTableSQL += string.Format(@"IF OBJECT_ID('tempdb..#InputRecordDataListForHistory') IS NOT NULL
            DROP TABLE #InputRecordDataListForHistory
        CREATE TABLE #InputRecordDataListForHistory 
        (
            ObjectTableId VARCHAR(50) COLLATE DATABASE_DEFAULT,
            ObjectTable VARCHAR(50) COLLATE DATABASE_DEFAULT
        )
        INSERT INTO #InputRecordDataListForHistory 
        VALUES ('{0}','{1}'); ", strObjectTableId, strObjectTableName) + Constants.vbCrLf;

                // get all trackable tables
                var oTrackableTables = _iTables.Where(x => x.TrackingTable > 0).ToList();
                // Generate batch Id for tracking history 
                int intIdentityKeyValue = RepositoryKeys.GetNextIdentityForBatchNumber("TrackingHistory") + 1;

                // Find destination table field name to exclude from build update tracking field string
                string strDestinationTableFieldName = "";
                var oDestinationTable = oTrackableTables.Where(x => (x.TableName.Trim() ?? "") == (strDestinationTableName.Trim() ?? "")).FirstOrDefault();
                if (oDestinationTable is null)
                {
                    return null;
                }
                strDestinationTableFieldName = oDestinationTable.TrackingStatusFieldName.ToString().Trim();

                // Prepare variables for dynamic fields which eligible to update in child effected records into tracking status
                var TrackableFieldNameList = oTrackableTables.Where(m => !string.IsNullOrEmpty(m.TrackingStatusFieldName)).OrderBy(x => x.TrackingTable).Select(x => x.TrackingStatusFieldName).ToList();
                string strTrackingColumnsDeclare = "";
                string strTrackingColumnsForUpdate = "";
                string strTrackingColumnsForUpdateValues = "";
                string strTrackingColumnsForInsert = "";
                string strTrackingColumnsForTrackingHistory = "";
                string strTrackingColumnsForUpdateFromVariables = "";
                int iCount = 0;
                // Loop to concate fields eligible to update for current object
                foreach (string strFieldName in TrackableFieldNameList)
                {
                    iCount = iCount + 1;
                    if ((strFieldName.Trim().ToLower() ?? "") != (strDestinationTableFieldName.Trim().ToLower() ?? ""))
                    {
                        strTrackingColumnsDeclare += Constants.vbCrLf + string.Format("DECLARE @v_{0} AS VARCHAR(50)", Query.ReplaceInvalidParameterCharacters(strFieldName));
                        strTrackingColumnsForUpdate += " @v_" + Query.ReplaceInvalidParameterCharacters(strFieldName) + " = [" + strFieldName + "], ";
                        strTrackingColumnsForUpdateFromVariables += " [" + strFieldName + "] = @v_" + Query.ReplaceInvalidParameterCharacters(strFieldName) + ", ";
                        strTrackingColumnsForUpdateValues += " @v_" + Query.ReplaceInvalidParameterCharacters(strFieldName) + ", ";
                        strTrackingColumnsForInsert += " [" + strFieldName + "], ";
                    }
                    // Prepared string with all trackable fields for history table updates
                    strTrackingColumnsForTrackingHistory += " [" + strFieldName + "], ";
                    // Removed extra comma from concated fields string
                    if (iCount == TrackableFieldNameList.Count)
                    {
                        strTrackingColumnsForUpdate = strTrackingColumnsForUpdate.Trim().TrimEnd(',');
                        strTrackingColumnsForUpdateFromVariables = strTrackingColumnsForUpdateFromVariables.Trim().TrimEnd(',');
                        strTrackingColumnsForUpdateValues = strTrackingColumnsForUpdateValues.Trim().TrimEnd(',');
                        strTrackingColumnsForInsert = strTrackingColumnsForInsert.Trim().TrimEnd(',');
                        strTrackingColumnsForTrackingHistory = strTrackingColumnsForTrackingHistory.Trim().TrimEnd(',');
                    }
                }
                // Set SQL string for get destination existing value from database into dynamic tracking columns
                string strSQLSetDestinationValueInTracableColumns = "";
                if (bIsReconciliationOn)
                {
                    strSQLSetDestinationValueInTracableColumns = strTrackingColumnsDeclare + Constants.vbCrLf + string.Format("Select TOP 1 {0} FROM [dbo].[AssetStatus] WHERE TrackedTableId = '{1}' AND TrackedTable = '{2}'", strTrackingColumnsForUpdate, strDestinationTableId, strDestinationTableName) + Constants.vbCrLf;
                }
                else
                {
                    strSQLSetDestinationValueInTracableColumns = strTrackingColumnsDeclare + Constants.vbCrLf + string.Format("Select TOP 1 {0} FROM [dbo].[TrackingStatus] WHERE TrackedTableId = '{1}' AND TrackedTable = '{2}'", strTrackingColumnsForUpdate, strDestinationTableId, strDestinationTableName) + Constants.vbCrLf;
                }

                // Date Due manipulation and calculation before insert into table
                string strDateDueConditionForNull = "";
                string strDateDue = null;
                if (dtDueDate is not null && (DateTime.MinValue is var arg106 && dtDueDate is { } arg105 ? arg105 != arg106 : (bool?)null).GetValueOrDefault())
                {
                    DateTime? dtDateDueDestCheck;
                    dtDateDueDestCheck = _iTrackingStatus.Where(x => (x.TrackedTableId ?? "") == (strDestinationTableId ?? "") & (x.TrackedTable ?? "") == (strDestinationTableName ?? "")).Select(x => x.DateDue).FirstOrDefault();
                    if (dtDateDueDestCheck is not null)
                    {
                        if ((bool)(dtDateDueDestCheck is var arg109 && dtDueDate is { } arg110 && arg109.HasValue ? arg109 < arg110 : (bool?)null))
                        {
                            dtDueDate = dtDateDueDestCheck;
                        }
                    }
                    strDateDue = " '" + Keys.get_ConvertStringtoDateWithTime((DateTime)dtDueDate, httpContext) + "' ";
                    strDateDueConditionForNull += string.Format(@" DateDue = CASE WHEN DateDue > CAST('{0}' AS DATETIME) THEN  
                                                        {1}
                                                        ELSE DateDue END ", Keys.get_ConvertStringtoDateWithTime((DateTime)dtDueDate, httpContext), strDateDue.ToString());
                }
                else
                {
                    // strDateDueConditionForNull += " DateDue = NULL "
                    strDateDueConditionForNull += " DateDue = CASE WHEN DateDue = NULL THEN NULL ELSE DateDue END ";
                }

                // Set Transaction date if passed
                string strTransactionDateTime = null;
                if (dtTransactionDateTime is not null)
                {
                    strTransactionDateTime = " '" + Keys.get_ConvertStringtoDateWithTime((DateTime)dtTransactionDateTime, httpContext) + "' ";
                }

                // Tracking history only needed if we do not selected Reconciliation mode
                string strSQLObjectDest = "";
                string strSQLTrackingHistory = "";
                if (!bIsReconciliationOn) // Start : bIsReconciliationOn
                {
                    string strTrackableFieldNameforObjectTable = "";
                    // Get container field name for Object table
                    var oObjectTable = oTrackableTables.Where(x => (x.TableName.Trim() ?? "") == (strObjectTableName.Trim() ?? "")).FirstOrDefault();
                    if (oObjectTable is not null) // Start : oObjectTable
                    {
                        strTrackableFieldNameforObjectTable = oObjectTable.TrackingStatusFieldName.ToString().Trim();
                        if (!string.IsNullOrEmpty(strTrackableFieldNameforObjectTable))
                        {
                            // Query set for add existing tracking for object table into temp table
                            strSQLObjectDest += Constants.vbCrLf + string.Format(@"Declare @tmpTblObjectDestUpdateIds AS TABLE (Id INT);
        INSERT INTO @tmpTblObjectDestUpdateIds
        Select Id 
        FROM TrackingStatus
        WHERE {0} = '{1}'", strTrackableFieldNameforObjectTable.Trim(), strObjectTableId) + Constants.vbCrLf;
                            // Update tracking status child records
                            strSQLObjectDest += Constants.vbCrLf + string.Format(@";UPDATE TS SET {0} = '{1}',TransactionDateTime = {2},{3},[Out] = {4} 
        FROM dbo.TrackingStatus TS 
        INNER JOIN @tmpTblObjectDestUpdateIds tmpID ON tmpID.Id = TS.Id 
        WHERE TransactionDateTime <= CAST('{5}' AS DATETIME);
        ", strDestinationTableFieldName, strDestinationTableId, Interaction.IIf(string.IsNullOrEmpty(strTransactionDateTime), "NULL", strTransactionDateTime), strDateDueConditionForNull, Interaction.IIf(intOutField.ToString().ToLower() == "true", "1", "0"), Keys.get_ConvertStringtoDateWithTime((DateTime)dtTransactionDateTime, httpContext)) + Constants.vbCrLf;
                            // Insert child effected records into temp created tables into SQL
                            strSQLObjectDest += @"INSERT INTO #InputRecordDataListForHistory 
        Select TrackedTableId,TrackedTable 
        FROM TrackingStatus TS 
        INNER JOIN @tmpTblObjectDestUpdateIds tmpID On tmpID.Id = TS.Id;";

                            // Update child record in tracking status
                            string strTrackingColumnsBasedonTrackingOrder = "";
                            var intObjectTableTrackingOrder = oObjectTable.TrackingTable;
                            var oTrackingTablesBasedOnTrackingOrder = oTrackableTables.Where(x => x.TrackingTable < intObjectTableTrackingOrder && x.TableName.Trim() != strDestinationTableName.Trim()).ToList();
                            if (oTrackingTablesBasedOnTrackingOrder is not null)
                            {
                                iCount = 0;
                                var lstringTracableFieldsNameByTrackingOrder = oTrackingTablesBasedOnTrackingOrder.Where(m => !string.IsNullOrEmpty(m.TrackingStatusFieldName)).OrderBy(x => x.TrackingTable).Select(x => x.TrackingStatusFieldName).ToList();
                                foreach (string strFieldName in lstringTracableFieldsNameByTrackingOrder)
                                {
                                    iCount = iCount + 1;
                                    strTrackingColumnsBasedonTrackingOrder = strTrackingColumnsBasedonTrackingOrder + " [" + strFieldName + "] = @v_" + Query.ReplaceInvalidParameterCharacters(strFieldName) + ", ";
                                    if (iCount == lstringTracableFieldsNameByTrackingOrder.Count)
                                    {
                                        strTrackingColumnsBasedonTrackingOrder = strTrackingColumnsBasedonTrackingOrder.Trim().TrimEnd(',');
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(strTrackingColumnsBasedonTrackingOrder))
                            {
                                strSQLObjectDest += Constants.vbCrLf + string.Format("UPDATE TS SET {0} FROM TrackingStatus TS INNER JOIN @tmpTblObjectDestUpdateIds tmpID ON tmpID.Id = TS.Id; ", strTrackingColumnsBasedonTrackingOrder);
                            }
                        }
                    } // End : oObjectTable

                    strSQLTrackingHistory += Constants.vbCrLf + string.Format(@"INSERT INTO TrackingHistory (TrackedTableId, TrackedTable, TransactionDateTime, IsActualScan, BatchId, UserName, TrackingAdditionalField1, TrackingAdditionalField2, {0}) 
        SELECT TrackedTableId, TrackedTable, TransactionDateTime, 1,CAST({1} AS VARCHAR(20)),UserName,TrackingAdditionalField1,TrackingAdditionalField2,{2} 
        FROM TrackingStatus 
        WHERE TrackedTableId= '{3}' AND TrackedTable= '{4}'; ", strTrackingColumnsForTrackingHistory, intIdentityKeyValue.ToString(), strTrackingColumnsForTrackingHistory, strObjectTableId, strObjectTableName);

                    if (!string.IsNullOrEmpty(strTrackableFieldNameforObjectTable))
                    {
                        strSQLTrackingHistory += Constants.vbCrLf + string.Format(@"INSERT INTO TrackingHistory (TrackedTableId, TrackedTable, TransactionDateTime, IsActualScan, BatchId, UserName, TrackingAdditionalField1, TrackingAdditionalField2, {0})
        SELECT TrackedTableId, TrackedTable, TransactionDateTime, 0,CAST({1} AS VARCHAR(20)),UserName,TrackingAdditionalField1,
        TrackingAdditionalField2, {2} 
        FROM TrackingStatus TS 
        INNER JOIN @tmpTblObjectDestUpdateIds tmpID ON tmpID.Id = TS.Id; ", strTrackingColumnsForTrackingHistory, intIdentityKeyValue.ToString(), strTrackingColumnsForTrackingHistory);
                    }

                } // End : bIsReconciliationOn

                if (bIsReconciliationOn)
                {
                    if (_iAssetStatus.All().Any(x => (x.TrackedTableId ?? "") == (strObjectTableId ?? "") & (x.TrackedTable.Trim() ?? "") == (strObjectTableName.Trim() ?? "")))
                    {
                        strSQL += strSQLSetDestinationValueInTracableColumns + Constants.vbCrLf;
                        strSQL = Convert.ToString(strSQL + Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(@"UPDATE [dbo].[AssetStatus]
            						    SET [TrackedTableId] = '" + strObjectTableId + @"' 
            						    ,[TrackedTable] = '" + strObjectTableName + @"'
            						    ,[TransactionDateTime] = ", Interaction.IIf(string.IsNullOrEmpty(strTransactionDateTime), "NULL", strTransactionDateTime)), @"
            						    ,[Out] = "), Interaction.IIf(intOutField.ToString().ToLower() == "true", "1", "0")), @"  
            						    ,[TrackingAdditionalField1] = "), Interaction.IIf(string.IsNullOrEmpty(strTrackingAdditionalField1), "NULL", "'" + strTrackingAdditionalField1 + "'")), @"
            						    ,[TrackingAdditionalField2] = "), Interaction.IIf(string.IsNullOrEmpty(strTrackingAdditionalField2), "NULL", "'" + strTrackingAdditionalField2 + "'")), @"  
            						    ,[UserName] = "), Interaction.IIf(string.IsNullOrEmpty(strUserName), "NULL", "'" + strUserName + "'")), @"
            						    ,[DateDue] = "), Interaction.IIf(string.IsNullOrEmpty(strDateDue), "NULL", strDateDue)), @"
            						    ,["), strDestinationTableFieldName), "] = '"), strDestinationTableId), @"'
            						    , "), strTrackingColumnsForUpdateFromVariables), " WHERE TrackedTableId= '"), strObjectTableId), "' AND TrackedTable='"), strObjectTableName), "'; "));
                    }
                    else
                    {
                        strSQL += strSQLSetDestinationValueInTracableColumns + Constants.vbCrLf;
                        strSQL = Convert.ToString(strSQL + Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(@"INSERT INTO [dbo].[AssetStatus] ([TrackedTableId], [TrackedTable], [Out], [TrackingAdditionalField1], [TrackingAdditionalField2], 
                                            [UserName], [DateDue], [TransactionDateTime],[" + strDestinationTableFieldName + "]," + strTrackingColumnsForInsert + @") 
            	                        VALUES
            	                        ('" + strObjectTableId + "', '" + strObjectTableName + "', ", Interaction.IIf(intOutField.ToString().ToLower() == "true", "1", "0")), @" , 
                                        "), Interaction.IIf(string.IsNullOrEmpty(strTrackingAdditionalField1), "NULL", "'" + strTrackingAdditionalField1 + "'")), @" , 
                                        "), Interaction.IIf(string.IsNullOrEmpty(strTrackingAdditionalField2), "NULL", "'" + strTrackingAdditionalField2 + "'")), @" , 
                                        "), Interaction.IIf(string.IsNullOrEmpty(strUserName), "NULL", "'" + strUserName + "'")), @" , 
                                        "), Interaction.IIf(string.IsNullOrEmpty(strDateDue), "NULL", strDateDue)), @" , 
                                        "), Interaction.IIf(string.IsNullOrEmpty(strTransactionDateTime), "NULL", "" + strTransactionDateTime + "")), @" , 
                                        '"), strDestinationTableId), "', "), strTrackingColumnsForUpdateValues), "); "));
                    }
                }
                else
                {
                    var TrackingStatusChkExists = _iTrackingStatus.Where(x => (x.TrackedTableId ?? "") == (strObjectTableId ?? "") & (x.TrackedTable ?? "") == (strObjectTableName ?? "")).FirstOrDefault();
                    // If (_iTrackingStatus.All().Any(Function(x) x.TrackedTableId = strObjectTableId And x.TrackedTable = strObjectTableName)) Then
                    if (TrackingStatusChkExists is not null)
                    {
                        strSQL += strSQLSetDestinationValueInTracableColumns + Constants.vbCrLf;

                        strSQL = Convert.ToString(strSQL + Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(@"UPDATE [dbo].[TrackingStatus]
            						    SET [TrackedTableId] = '" + strObjectTableId + @"'
            						    ,[TrackedTable] = '" + strObjectTableName + @"'
            						    ,[TransactionDateTime] = ", Interaction.IIf(string.IsNullOrEmpty(strTransactionDateTime), "NULL", strTransactionDateTime)), @"
            						    ,[Out] = "), Interaction.IIf(intOutField.ToString().ToLower() == "true", "1", "0")), @"  
            						    ,[TrackingAdditionalField1] = "), Interaction.IIf(string.IsNullOrEmpty(strTrackingAdditionalField1), "NULL", "'" + strTrackingAdditionalField1 + "'")), @"
            						    ,[TrackingAdditionalField2] = "), Interaction.IIf(string.IsNullOrEmpty(strTrackingAdditionalField2), "NULL", "'" + strTrackingAdditionalField2 + "'")), @"  
            						    ,[UserName] = "), Interaction.IIf(string.IsNullOrEmpty(strUserName), "NULL", "'" + strUserName + "'")), @"
            						    ,[DateDue] = "), Interaction.IIf(string.IsNullOrEmpty(strDateDue), "NULL", strDateDue)), @"
            						    ,["), strDestinationTableFieldName), "] = '"), strDestinationTableId), @"'
            						    , "), strTrackingColumnsForUpdateFromVariables), @"
            						    WHERE TrackedTableId= '"), strObjectTableId), "' AND TrackedTable='"), strObjectTableName), @"'
            						        And TransactionDateTime < CAST('"), Keys.get_ConvertStringtoDateWithTime((DateTime)dtTransactionDateTime, httpContext)), "' AS DATETIME); "), Constants.vbCrLf));

                        strSQL += strSQLObjectDest + Constants.vbCrLf;
                        strSQL += strSQLTrackingHistory + Constants.vbCrLf;
                    }
                    else
                    {
                        strSQL += strSQLSetDestinationValueInTracableColumns + Constants.vbCrLf;

                        strSQL = Convert.ToString(strSQL + Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(Operators.AddObject(@"INSERT INTO [dbo].[TrackingStatus] ([TrackedTableId], [TrackedTable], [Out], [TrackingAdditionalField1], [TrackingAdditionalField2], [UserName], [DateDue], [TransactionDateTime],
                                            [" + strDestinationTableFieldName + "]," + strTrackingColumnsForInsert + @") 
            	                        VALUES
            	                        ('" + strObjectTableId + "', '" + strObjectTableName + "', ", Interaction.IIf(intOutField.ToString().ToLower() == "true", "1", "0")), @" , 
                                        "), Interaction.IIf(string.IsNullOrEmpty(strTrackingAdditionalField1), "NULL", "'" + strTrackingAdditionalField1 + "'")), @" , 
                                        "), Interaction.IIf(string.IsNullOrEmpty(strTrackingAdditionalField2), "NULL", "'" + strTrackingAdditionalField2 + "'")), @" , 
                                        "), Interaction.IIf(string.IsNullOrEmpty(strUserName), "NULL", "'" + strUserName + "'")), @" , 
                                        "), Interaction.IIf(string.IsNullOrEmpty(strDateDue), "NULL", strDateDue)), @"
                                        , "), Interaction.IIf(string.IsNullOrEmpty(strTransactionDateTime), "NULL", "" + strTransactionDateTime + "")), @" , 
                                        '"), strDestinationTableId), "', "), strTrackingColumnsForUpdateValues), "); "), Constants.vbCrLf));

                        strSQL += strSQLObjectDest + Constants.vbCrLf;
                        strSQL += strSQLTrackingHistory + Constants.vbCrLf;
                    }
                }

                strSQL = strCreateTempTableSQL + " " + strSQL + " ;";


                strSQL = Convert.ToString(strSQL + Operators.AddObject(Constants.vbCrLf, UpdateRequestsQuery(oTrackableTables, strObjectTableName, strObjectTableId, DefaultIDBManager)));
                strSQL = Convert.ToString(strSQL + Operators.AddObject(Constants.vbCrLf, DeleteTrackingHistoryQuery()));


                // strSQL += vbCrLf + " ;SELECT * FROM #EffectedRecordForRemoveRequest "

                int output = DefaultIDBManager.ExecuteNonQuery(CommandType.Text, strSQL);
                object dsEffectedTrackingRec = null;
                return (DataSet)dsEffectedTrackingRec;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static object UpdateRequestsQuery(List<Table> oTrackableTables, string oTrackedTable, string oTrackedTableId, IDBManager DefaultIDBManager)
        {
            string strUpdateRequestSQL = "";
            string strUpdateMostEligibleRequestSQL = "";

            strUpdateRequestSQL += @"IF OBJECT_ID('tempdb..#EffectedRecordForRemoveRequest') IS NOT NULL
            DROP TABLE #EffectedRecordForRemoveRequest
        CREATE TABLE #EffectedRecordForRemoveRequest
        (
            ObjectTableId VARCHAR(50) COLLATE DATABASE_DEFAULT,
            ObjectTable VARCHAR(50) COLLATE DATABASE_DEFAULT,
            SecoundContainerId VARCHAR(50) COLLATE DATABASE_DEFAULT
        ); " + Constants.vbCrLf;

            var oSecoundLevelContainer = oTrackableTables.Where(x => x.TrackingTable == Keys.TRACKING_EMPLOYEE).FirstOrDefault();
            string strSecoundLevelContainerName = "";
            if (oSecoundLevelContainer is not null)
            {
                strSecoundLevelContainerName = oSecoundLevelContainer.TrackingStatusFieldName.Trim();
            }

            strUpdateRequestSQL += Constants.vbCrLf + @"INSERT INTO #EffectedRecordForRemoveRequest 
        SELECT IDH.ObjectTableId, IDH.ObjectTable, TS." + strSecoundLevelContainerName + @" 
        FROM TrackingStatus TS
        INNER JOIN #InputRecordDataListForHistory IDH ON IDH.ObjectTableId=TS.TrackedTableId AND IDH.ObjectTable=TS.TrackedTable " + Constants.vbCrLf;

            strUpdateRequestSQL += Constants.vbCrLf + "UPDATE SLR SET Status =  CASE WHEN SLR.EmployeeId = ER.SecoundContainerId THEN 'Fulfilled' WHEN Status IN ('New','In Process') THEN 'WaitList' Else Status End ," + "DateReceived = CASE WHEN SLR.EmployeeId = ER.SecoundContainerId THEN GETDATE() Else DateReceived End ," + "SLPullListsId = CASE WHEN Status = 'In Process' THEN 0 Else SLPullListsId End ," + "DatePulled =  CASE WHEN Status = 'In Process' THEN GETDATE() Else DatePulled End " + "From SLRequestor SLR " + "INNER JOIN #EffectedRecordForRemoveRequest ER ON SLR.TableId = ER.ObjectTableId And SLR.TableName = ER.ObjectTable" + " WHERE SLR.Status Not IN ('Deleted','Fulfilled')" + Constants.vbCrLf;


            var oLocationsTable = oTrackableTables.Where(x => x.TrackingTable == Keys.TRACKING_LOCATION).FirstOrDefault();


            strUpdateMostEligibleRequestSQL += Constants.vbCrLf + @"SELECT ROW_NUMBER() OVER(PARTITION BY SR.TableName,SR.TableId ORDER BY SR.[Priority], SR.[DateRequested] ) AS RowNum,
        		SR.Id  
        		FROM SLRequestor SR
        		INNER JOIN [TrackingStatus] TS ON SR.[TableName] = TS.[TrackedTable] AND SR.[TableId] = TS.[TrackedTableId] AND (TS.[" + DatabaseMap.RemoveTableNameFromField(oLocationsTable.TrackingStatusFieldName) + "] IS NOT NULL)";
            // WHERE SR.TableId = {0} AND SR.TableName = {1}", oTrackedTableId, oTrackedTable)
            // INNER JOIN @InputRecordTable AS INREC ON SR.TableId = INREC.ObjectTableId AND SR.TableName = INREC.ObjectTable "

            if (oLocationsTable is not null)
            {
                if (!string.IsNullOrEmpty(Strings.Trim(oLocationsTable.TrackingRequestableFieldName)))
                {
                    strUpdateMostEligibleRequestSQL += Constants.vbCrLf + " INNER JOIN [" + oLocationsTable.TableName + "] TSL ON TS.[" + DatabaseMap.RemoveTableNameFromField(oLocationsTable.TrackingStatusFieldName) + "] = ";

                    bool IfIdFieldIsString = GetInfoUsingADONET.IdFieldIsString(DefaultIDBManager, oLocationsTable.TableName, oLocationsTable.IdFieldName);
                    if (IfIdFieldIsString)
                    {
                        strUpdateMostEligibleRequestSQL += " TSL.[" + DatabaseMap.RemoveTableNameFromField(oLocationsTable.IdFieldName) + "]";
                    }
                    else
                    {
                        strUpdateMostEligibleRequestSQL += " RIGHT('" + new string('0', DatabaseMap.UserLinkIndexTableIdSize) + "' + Convert(VARCHAR(" + DatabaseMap.UserLinkIndexTableIdSize.ToString() + "), TSL.[" + DatabaseMap.RemoveTableNameFromField(oLocationsTable.IdFieldName) + "]), " + DatabaseMap.UserLinkIndexTableIdSize.ToString() + ")";
                    }

                }
            }
            strUpdateMostEligibleRequestSQL += Constants.vbCrLf + string.Format(@"WHERE SR.[Status] = 'WaitList' 
        		AND NOT EXISTS (SELECT TOP 1 Id FROM SLRequestor SRSUB WHERE SRSUB.TableName = '{1}' AND SRSUB.TableId = '{0}'
        						AND SRSUB.[Status] IN ('New','New Batch','In Process','Exception'))
                 AND SR.TableId = '{0}' AND SR.TableName = '{1}'", oTrackedTableId, oTrackedTable);

            if (!string.IsNullOrEmpty(Strings.Trim(oLocationsTable.TrackingRequestableFieldName)))
            {
                strUpdateMostEligibleRequestSQL += " AND (TSL.[" + DatabaseMap.RemoveTableNameFromField(oLocationsTable.TrackingRequestableFieldName) + "] <> 0)";
            }

            strUpdateRequestSQL += Constants.vbCrLf + @";WITH cteEligibleRequest AS
        		(
        		" + strUpdateMostEligibleRequestSQL + @"
        		)  UPDATE SR SET SR.[Status] = 'New' FROM SLRequestor SR  
        		INNER JOIN cteEligibleRequest ER ON ER.Id = SR.Id WHERE ER.RowNum < 2;";

            return strUpdateRequestSQL;
        }

        private static object DeleteTrackingHistoryQuery()
        {
            string strDelTrackingHistory = "";
            IRepository<TabFusionRMS.Models.System> _iSystem = new Repositories<TabFusionRMS.Models.System>();
            var oSystem = _iSystem.All().FirstOrDefault();
            if (oSystem is not null)
            {
                if (oSystem.MaxHistoryDays > 0)
                {
                    var dMaxDate = DateTime.FromOADate((double)(DateTime.Now.ToOADate() - oSystem.MaxHistoryDays - 1));
                    var dUTC = dMaxDate.ToUniversalTime();
                    // String.Format("DELETE FROM [TrackingHistory] Where TransactionDateTime < {0} AND [TrackedTable] = '{1}' AND [TrackedTableId] = '{2}'", dUTC, sTableName.Trim(), sId.Trim())
                    strDelTrackingHistory += Constants.vbCrLf + string.Format(@"DELETE	TH
        			FROM TrackingHistory TH
        			INNER JOIN	#InputRecordDataListForHistory RT ON RT.ObjectTable = TH.TrackedTable AND RT.ObjectTableId = TH.TrackedTableId
        			WHERE TH.TransactionDateTime < CAST('{0}' AS DATETIME) ", dUTC);

                }
                if (oSystem.MaxHistoryItems > 0)
                {
                    strDelTrackingHistory += Constants.vbCrLf + @";WITH cteDeleteHistory AS
        			(
        				SELECT		ROW_NUMBER() OVER(PARTITION BY TH.TrackedTable,TH.TrackedTableId ORDER BY TH.TransactionDateTime DESC) AS RowNum, TH.Id
        				FROM		TrackingHistory TH
        				INNER JOIN	(SELECT		TrackedTableId,TrackedTable
        							FROM		TrackingHistory
        							GROUP BY	TrackedTableId,TrackedTable
        							HAVING		COUNT(ID) > " + oSystem.MaxHistoryItems.ToString() + @") AS AST ON AST.TrackedTableId = TH.TrackedTableId AND AST.TrackedTable = TH.TrackedTable
        				INNER JOIN	#InputRecordDataListForHistory RT ON RT.ObjectTable = TH.TrackedTable AND RT.ObjectTableId = TH.TrackedTableId
        			)
        			DELETE TH
        			FROM TrackingHistory TH
        			INNER JOIN cteDeleteHistory CDH on CDH.Id = TH.Id
        			WHERE CDH.RowNum > " + oSystem.MaxHistoryItems.ToString() + ";";
                }
            }

            return strDelTrackingHistory;
        }
    }
    #endregion
    
}



