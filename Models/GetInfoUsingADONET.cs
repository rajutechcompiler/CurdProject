using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using TabFusionRMS.DataBaseManagerVB;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.Resource;
using Smead.Security;

namespace TabFusionRMS.WebCS
{

    public class GetInfoUsingADONET
    {
        private const string SL_USERNAME = "@@SL_UserName";
        public const int TRACKING_LOCATION = 1;
        public const int TRACKING_EMPLOYEE = 2;
        // Public Shared _IDBManager As IDBManager = New ImportDBManager
        private static int miBatchNum = 0;

        #region Check DataType
        public static bool IsADate(string oDataType)
        {
            switch (oDataType.Trim().ToLower() ?? "")
            {
                case "system.datetime":
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        public static bool IsAString(string oDataType)
        {
            switch (oDataType.Trim().ToLower() ?? "")
            {
                case "system.string":
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        public static bool IsANumeric(string oDataType)
        {
            switch (oDataType.Trim().ToLower() ?? "")
            {
                case "system.int32":
                case "system.decimal":
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        public static bool IsADateForSchema(string oDataType)
        {
            switch (oDataType ?? "")
            {
                case "datetime":
                case "smalldatetime":
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        public static bool IsAStringForSchema(string oDataType)
        {
            switch (oDataType ?? "")
            {
                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        public static bool IsANumericForSchema(string oDataType)
        {
            switch (oDataType ?? "")
            {
                case "tinyint":
                case "smallint":
                case "identity":
                case "int":
                case "bigint":
                case "real":
                case "float":
                case "decimal":
                case "numeric":
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }
        #endregion

        #region SQL Process Function Using Default DBManager
        // Process SQL query using default connection so we do not need to open connection every time
        public static bool ProcessADONETRecord(string sSql, IDBManager CommonIDBManager, [Optional, DefaultParameterValue("")] ref string sErrMsg, [Optional, DefaultParameterValue(0)] ref int lError)
        {
            try
            {
                if (CommonIDBManager.Connection.State == ConnectionState.Closed)
                {
                    CommonIDBManager.Open();
                }
                CommonIDBManager.ExecuteNonQuery(CommandType.Text, sSql);
                return true;
            }
            catch (Exception ex)
            {
                sErrMsg = ex.Message;
                lError = 1;
                return false;
            }
        }

        // Process SQL query and get data using default connection so we do not need to open connection every time
        public static DataTable GetADONETRecord(string sSql, IDBManager CommonIDBManager, [Optional, DefaultParameterValue("")] ref string sErrMsg, [Optional, DefaultParameterValue(0)] ref int lError, string TableName = "")
        {
            try
            {
                if (CommonIDBManager.Connection.State == ConnectionState.Closed)
                {
                    CommonIDBManager.Open();
                }
                var lOutput = CommonIDBManager.ExecuteDataSet(CommandType.Text, sSql, false, TableName);
                return lOutput.Tables[0];
            }
            catch (Exception ex)
            {
                sErrMsg = ex.Message;
                lError = 1;
                return null;
            }
        }

        // Process SQL query and get scalar result using default connection so we do not need to open connection every time
        public static object GetScalarUsingADONET(string sSql, IDBManager CommonIDBManager)
        {
            try
            {
                if (CommonIDBManager.Connection.State == ConnectionState.Closed)
                {
                    CommonIDBManager.Open();
                }
                var lOutput = CommonIDBManager.ExecuteScalar(CommandType.Text, sSql);
                return lOutput;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Get schema info using default connection so we do not need to open connection every time
        public static DataTable GetSchemaInfo(IDBManager CommonIDBManager, string sTableName, string sIdFieldName = "")
        {
            try
            {
                if (CommonIDBManager.Connection.State == ConnectionState.Closed)
                {
                    CommonIDBManager.Open();
                }
                string sSQL = "SELECT * FROM INFORMATION_SCHEMA.COLUMNS";
                var lSchemaColumnList = new List<SchemaColumns>();
                if (!string.IsNullOrEmpty(sTableName) && string.IsNullOrEmpty(sIdFieldName))
                {
                    sSQL = sSQL + " where TABLE_NAME= '" + sTableName + "'";
                }
                else if (!string.IsNullOrEmpty(sTableName) && !string.IsNullOrEmpty(sIdFieldName))
                {
                    sSQL = sSQL + " where TABLE_NAME= '" + sTableName + "' AND  COLUMN_NAME='" + sIdFieldName + "'";
                }
                else if (string.IsNullOrEmpty(sTableName) && !string.IsNullOrEmpty(sIdFieldName))
                {
                    sSQL = sSQL + " where COLUMN_NAME='" + sIdFieldName + "'";
                }
                var lOutPut = CommonIDBManager.ExecuteDataSet(CommandType.Text, sSQL, false);
                return lOutPut.Tables[0];
            }
            catch (Exception)
            {
                throw;
            }

        }
        #endregion

        #region Tracking
        public static string AssignRetentionCode(Table oTable, string sID, List<Databas> data_iDatabas, List<Table> data_iTables,
            List<RelationShip> data_iRelationship,HttpContext httpContext, IDBManager _IDBManagerDefault = null, IDBManager _IDBManager = null)
        {
            Table oRetentionParentTable;
            RelationShip oRelationships;
            DataTable rsCurrentRecord;
            string sParentID = string.Empty;
            string sRetentionCode = string.Empty;
            string sSQL = string.Empty;
            IDBManager commonDbManager = null;
            try
            {
                if (oTable is not null)
                {
                    if (!string.IsNullOrEmpty(oTable.DBName))
                    {
                        var oDatabase = data_iDatabas.Where(m => m.DBName.Trim().ToLower().Equals(oTable.DBName.Trim().ToLower())).FirstOrDefault();
                        _IDBManager.ConnectionString = Keys.get_GetDBConnectionString(oDatabase);
                        _IDBManager.Open();
                        commonDbManager = _IDBManager;
                    }
                    else
                    {
                        commonDbManager = _IDBManagerDefault;
                    }
                    if ((bool)(oTable.RetentionPeriodActive | oTable.RetentionInactivityActive))
                    {
                        if ((bool)(Convert.ToInt32(Enums.meRetentionCodeAssignment.rcaRelatedTable) is var arg2 && oTable.RetentionAssignmentMethod is { } arg1 ? arg1 == arg2 : (bool?)null))
                        {
                            // Retention Code is assigned by related table
                            var retentionTable = data_iTables.Where(m => m.TableName.Trim().ToLower().Equals(oTable.RetentionRelatedTable.Trim().ToLower())).FirstOrDefault();
                            oRetentionParentTable = retentionTable;

                            if (oRetentionParentTable is not null)
                            {
                                var ulRelation = data_iRelationship.Where(m => m.UpperTableName.Trim().ToLower().Equals(oRetentionParentTable.TableName.Trim().ToLower()) & m.LowerTableName.Trim().ToLower().Equals(oTable.TableName.Trim().ToLower())).OrderBy(m => m.TabOrder).FirstOrDefault();
                                oRelationships = ulRelation;

                                if (oRelationships is not null)
                                {
                                    sSQL = string.Format("SELECT * FROM [{0}] WHERE [{1}] = {2}", oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), IDQueryValue(sID, commonDbManager, oTable));
                                    string argsErrMsg = "";
                                    int arglError = 0;
                                    rsCurrentRecord = GetADONETRecord(sSQL, commonDbManager, sErrMsg: ref argsErrMsg, lError: ref arglError);

                                    if (rsCurrentRecord is not null)
                                    {
                                        if (rsCurrentRecord.Rows.Count > 0)
                                        {
                                            if (rsCurrentRecord.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.RetentionFieldName)] is DBNull)
                                            {
                                                sParentID = Convert.ToString(rsCurrentRecord.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oRelationships.LowerTableFieldName)]);
                                                sRetentionCode = GetRetentionCodeValueUsingADONET(oTable, oRetentionParentTable, sParentID, false, data_iDatabas, commonDbManager, httpContext);
                                                sSQL = string.Format("UPDATE [{0}] SET [{1}] = '{2}' WHERE [{3}] = {4}", oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.RetentionFieldName), sRetentionCode, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), IDQueryValue(sID, commonDbManager, oTable));
                                                string argsErrMsg1 = "";
                                                int arglError1 = 0;
                                                ProcessADONETRecord(sSQL, commonDbManager, sErrMsg: ref argsErrMsg1, lError: ref arglError1);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Retention Code is assigned by default code stored in table   (Manual or By Table)
                            sRetentionCode = GetRetentionCodeValueUsingADONET(oTable, null, sParentID, false, data_iDatabas, commonDbManager, httpContext);
                            sSQL = string.Format("UPDATE [{0}] SET [{1}] = '{2}' WHERE [{3}] = {4}", oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.RetentionFieldName), sRetentionCode, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), IDQueryValue(sID, commonDbManager, oTable));
                            string argsErrMsg2 = "";
                            int arglError2 = 0;
                            ProcessADONETRecord(sSQL, commonDbManager, sErrMsg: ref argsErrMsg2, lError: ref arglError2);
                        }
                    }
                }
                return sRetentionCode;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_IDBManager.Connection is not null)
                {
                    _IDBManager.Close();
                }
            }
        }

        public static string GetRetentionCodeValueUsingADONET(Table oCurrentTable, Table oParentTable, string sParentID, bool bFromDropDown, List<Databas> data_iDatabase, IDBManager IDBManagerDefault, HttpContext httpContext)
        {
            DataTable rsParentRecord;
            string sSQL;
            IDBManager IDBManager = null;
            try
            {
                if (oCurrentTable is not null)
                {
                    if ((bool)(Convert.ToInt32(Enums.meRetentionCodeAssignment.rcaRelatedTable) is var arg4 && oCurrentTable.RetentionAssignmentMethod is { } arg3 ? arg3 == arg4 : (bool?)null))
                    {
                        if (Strings.StrComp(oParentTable.TableName, oCurrentTable.RetentionRelatedTable, Constants.vbTextCompare) == 0)
                        {
                            if (oParentTable is not null)
                            {
                                if (!string.IsNullOrEmpty(oParentTable.DBName))
                                {
                                    var oDatabase = data_iDatabase.Where(m => m.DBName.Trim().ToLower().Equals(oParentTable.DBName.Trim().ToLower())).FirstOrDefault();
                                    IDBManager = new ImportDBManager();
                                    IDBManager.ConnectionString = Keys.get_GetDBConnectionString(oDatabase);
                                    IDBManager.Open();
                                    sSQL = string.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = '{3}'", oParentTable.RetentionFieldName, oParentTable.TableName, DatabaseMap.RemoveTableNameFromField(oParentTable.IdFieldName), IDQueryValue(sParentID, IDBManager, oParentTable));
                                    string argsErrMsg = "";
                                    int arglError = 0;
                                    rsParentRecord = GetADONETRecord(sSQL, IDBManager, sErrMsg: ref argsErrMsg, lError: ref arglError);
                                }
                                else
                                {
                                    sSQL = string.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = '{3}'", oParentTable.RetentionFieldName, oParentTable.TableName, DatabaseMap.RemoveTableNameFromField(oParentTable.IdFieldName), IDQueryValue(sParentID, IDBManagerDefault, oParentTable));
                                    string argsErrMsg1 = "";
                                    int arglError1 = 0;
                                    rsParentRecord = GetADONETRecord(sSQL, IDBManagerDefault, sErrMsg: ref argsErrMsg1, lError: ref arglError1);
                                }
                                // If (Not String.IsNullOrEmpty(oParentTable.DBName)) Then
                                // sSQL = String.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = {3}", oParentTable.RetentionFieldName, oParentTable.TableName, DatabaseMap.RemoveTableNameFromField(oParentTable.IdFieldName), GetInfoUsingADONET.IDQueryValue(sParentID, IDBManager, oParentTable))
                                // rsParentRecord = GetInfoUsingADONET.GetADONETRecord(sSQL, IDBManager)
                                // Else
                                // sSQL = String.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = {3}", oParentTable.RetentionFieldName, oParentTable.TableName, DatabaseMap.RemoveTableNameFromField(oParentTable.IdFieldName), GetInfoUsingADONET.IDQueryValue(sParentID, IDBManagerDefault, oParentTable))
                                // rsParentRecord = GetInfoUsingADONET.GetADONETRecord(sSQL, IDBManagerDefault)
                                // End If

                                if (rsParentRecord is not null)
                                {
                                    if (rsParentRecord.Rows.Count > 0)
                                    {
                                        if (!(rsParentRecord.AsEnumerable().ElementAtOrDefault(0)[oParentTable.RetentionFieldName] is DBNull))
                                        {
                                            return Convert.ToString(rsParentRecord.AsEnumerable().ElementAtOrDefault(0)[oParentTable.RetentionFieldName]);
                                        }
                                        else
                                        {
                                            return string.Empty;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return oCurrentTable.DefaultRetentionId;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (IDBManager is not null)
                {
                    if (IDBManager.Connection is not null)
                    {
                        IDBManager.Close();
                        IDBManager.Dispose();
                    }
                }
            }

            return string.Empty;
        }

        // check renuka
        public static FoundBarCode BarCodeLookup(Smead.Security.Passport passport,
            List<Databas> iDatabaseQuery, List<Table> iTableQuery,
            List<TabFusionRMS.Models.System> iSystemQuery, List<ScanList> iScanListQuery,
            List<View> iViewQuery, List<TableTab> iTableTabQuery, List<TabSet> iTabSetQuery,
            List<RelationShip> iRelationShipQuery, string LookupString,
            HttpContext httpContext, ref string sTabsetIds,
            [Optional] ref Table oCurrentTable,
            [Optional] ref bool bSecurityCheck,
            [Optional] IDBManager IDBManagerDefault,
            [Optional] bool IsDestination,
            [Optional] ref string sReturnMessage)
        {
            FoundBarCode oFoundBarCode;

            if (sTabsetIds is not null && sTabsetIds.Length > 0)
            {
                var cTabsetIds = new Collection();
                var asTabsetIds = Strings.Split(sTabsetIds, ",");

                foreach (string s in asTabsetIds)
                    cTabsetIds.Add(null, s);
                oFoundBarCode = BarCodeLookup(passport, iDatabaseQuery, iTableQuery, iSystemQuery, iScanListQuery, iViewQuery, iTableTabQuery, iTabSetQuery, iRelationShipQuery, LookupString, httpContext, ref cTabsetIds, ref oCurrentTable, ref bSecurityCheck, IDBManagerDefault, IsDestination, ref sReturnMessage);
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
                oFoundBarCode = BarCodeLookup(passport, iDatabaseQuery, iTableQuery, iSystemQuery, iScanListQuery, iViewQuery, iTableTabQuery, iTabSetQuery,
                    iRelationShipQuery, LookupString,httpContext, ref argcTabsetIds, ref oCurrentTable, ref bSecurityCheck, IDBManagerDefault, IsDestination, ref sReturnMessage);
            }

            return oFoundBarCode;
        }

        public static FoundBarCode BarCodeLookup(Smead.Security.Passport passport,
            List<Databas> iDatabaseQuery, List<Table> iTableQuery,
            List<TabFusionRMS.Models.System> iSystemQuery,
            List<ScanList> iScanListQuery, List<View> iViewQuery,
            List<TableTab> iTableTabQuery, List<TabSet> iTabSetQuery,
            List<RelationShip> iRelationShipQuery, string LookupString,
            HttpContext httpContext,
            [Optional, DefaultParameterValue(null)] ref Collection cTabsetIds,
            [Optional, DefaultParameterValue(null)] ref Table oCurrentTable,
            [Optional, DefaultParameterValue(false)] ref bool bSecurityCheck,
            [Optional, DefaultParameterValue(null)] IDBManager IDBManagerDefault,
            [Optional, DefaultParameterValue(false)] bool IsDestination,
            [Optional, DefaultParameterValue("")] ref string sReturnMessage)
        {
            FoundBarCode BarCodeLookupRet = default;
            // Dim sADOConBase As ADODB.Connection = DataServices.DBOpen()
            IDBManager CommonIDBManager = null;
            IDBManager IDBManager = null;
            // Dim d1 As DateTime = DateTime.Now
            // Dim s1 As String = d1.ToString("yyyy/MM/dd-HH/mm/ss.fff")
            // TrackingServices.WriteTxtFile("BarCodeLookup Start       " + s1)
            try
            {
                int iCnt;
                string sLookupStr = string.Empty;
                bool bGoAhead;
                bool bOrigSecurity;
                bool bDidIdFldName;
                string sDescription = string.Empty;
                ScanList oScanList = null;
                DataRow oSchemaColumns = null;
                Table oTable = null;
                TabSet oTabSets;
                DataTable dtADONET = null;
                BarCodeLookupRet = null;
                bOrigSecurity = bSecurityCheck;
                // Lookup for Barcode Prefix Matches
                // Dim passport = New Smead.Security.Passport
                // Dim oTableObj = iTableQuery
                var oTableObjJoin = (from T in iTableQuery
                                     join SL in iScanListQuery
                                     on T.TableName.Trim().ToUpper()
                                     equals SL.TableName.Trim().ToUpper()
                                     where T.BarCodePrefix != null
                                     orderby SL.ScanOrder
                                     select new { T, SL }).ToList();

                var loopTo = oTableObjJoin.Count - 1;
                for (iCnt = 0; iCnt <= loopTo; iCnt++)
                {
                    if (oCurrentTable is null)
                    {
                        oTable = oTableObjJoin[iCnt].T;
                    }
                    // oTable = DirectCast(oTableObj.AsEnumerable.ElementAt(iCnt), Table)
                    else
                    {
                        oTable = oCurrentTable;
                    }

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
                        if (!string.IsNullOrEmpty(oTable.DBName))
                        {
                            var oDatabase = iDatabaseQuery.Where(m => m.DBName.Trim().ToLower().Equals(oTable.DBName.Trim().ToLower())).FirstOrDefault();
                            if (IDBManager is null)
                            {
                                IDBManager = new ImportDBManager();
                            }
                            IDBManager.ConnectionString = Keys.get_GetDBConnectionString(oDatabase);
                            IDBManager.Open();
                            CommonIDBManager = IDBManager;
                        }
                        else
                        {
                            CommonIDBManager = IDBManagerDefault;
                        }
                        bool IfIdFieldIsString = IdFieldIsString(CommonIDBManager, oTable.TableName, oTable.IdFieldName);
                        if (bGoAhead)
                        {
                            // if dest is numeric & lookupstr is a string, don't attempt lookup
                            if (!IfIdFieldIsString & !Information.IsNumeric(sLookupStr))
                                bGoAhead = false;
                        }

                        if (bGoAhead)
                        {
                            if ((bool)(IsDestination == true & (0 is var arg6 && oTable.TrackingTable is { } arg5 ? arg5 > arg6 : null) | IsDestination == false & (true is var arg8 && oTable.Trackable is { } arg7 ? arg7 == arg8 : null)))
                            {
                                if (IfIdFieldIsString & Strings.Len(oTable.IdMask) > 0)
                                    sLookupStr = Strings.Format(sLookupStr, oTable.IdMask);
                                dtADONET = FindUserRecord(oTable.TableName, oTable.IdFieldName, oTable.IdFieldName2, oTable.TrackingACTIVEFieldName, oTable.TrackingOUTFieldName, oTable.TrackingEmailFieldName, oTable.TrackingDueBackDaysFieldName, oTable.OperatorsIdField, oTable.IdFieldName, IfIdFieldIsString, sLookupStr, iTableQuery, iDatabaseQuery, CommonIDBManager);

                                if (dtADONET is not null)
                                {
                                    if (dtADONET.Rows.Count > 0)
                                    {
                                        if (VerifyViewSecurity(passport, iViewQuery, iDatabaseQuery, oTable, sLookupStr, oTable.IdFieldName, IfIdFieldIsString, CommonIDBManager, bOrigSecurity) > 0)
                                        {
                                            bSecurityCheck = true;
                                            break;
                                        }
                                        else
                                        {
                                            bSecurityCheck = false;
                                        }
                                    }
                                }
                                dtADONET = null;
                            }
                            else
                            {
                                sReturnMessage = Languages.get_Translation("msgTransfersNotAuthToTransfer");
                            }
                        }
                    }

                    if (oCurrentTable is not null)
                        break;
                    if (IDBManager is not null)
                    {
                        if (IDBManager.Connection is not null)
                        {
                            IDBManager.Close();
                            IDBManager.Dispose();
                        }
                    }
                }

                if (dtADONET is null)
                {
                    bDidIdFldName = false;
                    var oScanListObj = iScanListQuery.OrderBy(m => m.Id).ToList();
                    var loopTo1 = oScanListObj.Count;
                    for (iCnt = 0; iCnt <= loopTo1; iCnt++)
                    {
                        if (iCnt == oScanListObj.Count)
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
                                oTable = iTableQuery.Where(m => m.TableName.Trim().ToLower().Equals(oScanList.TableName.Trim().ToLower())).FirstOrDefault();
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
                            if (oTable.DBName is not null)
                            {
                                var oDatabase = iDatabaseQuery.Where(m => m.DBName.Trim().ToLower().Equals(oTable.DBName.Trim().ToLower())).FirstOrDefault();
                                if (IDBManager is null)
                                {
                                    IDBManager = new ImportDBManager();
                                }
                                IDBManager.ConnectionString = Keys.get_GetDBConnectionString(oDatabase);
                                IDBManager.Open();
                                CommonIDBManager = IDBManager;
                            }
                            else
                            {
                                CommonIDBManager = IDBManagerDefault;
                            }
                            sLookupStr = LookupString;
                            sLookupStr = Strings.Replace(sLookupStr, oScanList.IdStripChars, "", Compare: CompareMethod.Text);
                            if (Strings.Len(oTable.BarCodePrefix) > 0 & Strings.StrComp(Strings.Left(sLookupStr, Strings.Len(oTable.BarCodePrefix)), oTable.BarCodePrefix, CompareMethod.Text) == 0)
                            {
                                sLookupStr = Strings.Mid(sLookupStr, Strings.Len(oTable.BarCodePrefix) + 1);
                            }

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
                            var oSchemaColumnsTable = GetInfoUsingADONET.GetSchemaInfo(CommonIDBManager, oTable.TableName, DatabaseMap.RemoveTableNameFromField(oScanList.FieldName));
                            if (oSchemaColumnsTable is not null)
                            {
                                if (oSchemaColumnsTable.Rows.Count != 0)
                                {
                                    oSchemaColumns = oSchemaColumnsTable.Rows[0];
                                }
                            }
                            if (oSchemaColumns is not null)
                            {
                                bool IsFieldString = IsAStringForSchema(Convert.ToString(oSchemaColumns["DATA_TYPE"]));
                                // if dest is numeric & lookupstr is a string, don't attempt lookup
                                bGoAhead = true;
                                if (!IsFieldString & !Information.IsNumeric(sLookupStr))
                                    bGoAhead = false;

                                if (bGoAhead)
                                {
                                    if ((bool)(IsDestination == true & (0 is var arg10 && oTable.TrackingTable is { } arg9 ? arg9 > arg10 : null) | IsDestination == false & (true is var arg12 && oTable.Trackable is { } arg11 ? arg11 == arg12 : null)))
                                    {
                                        if (IsFieldString)
                                        {
                                            if (!string.IsNullOrEmpty(Strings.Trim(oScanList.IdMask)))
                                                sLookupStr = Strings.Format(sLookupStr, oScanList.IdMask);
                                        }
                                        dtADONET = FindUserRecord(oScanList.TableName, oTable.IdFieldName, oTable.IdFieldName2, oTable.TrackingACTIVEFieldName, oTable.TrackingOUTFieldName, oTable.TrackingEmailFieldName, oTable.TrackingDueBackDaysFieldName, oTable.OperatorsIdField, oScanList.FieldName, IsFieldString, sLookupStr, iTableQuery, iDatabaseQuery, CommonIDBManager);
                                        if (dtADONET is not null)
                                        {
                                            if (dtADONET.Rows.Count > 0)
                                            {
                                                if (VerifyViewSecurity(passport, iViewQuery, iDatabaseQuery, oTable, sLookupStr, oScanList.FieldName, IsFieldString, CommonIDBManager, bOrigSecurity) > 0)
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
                                            // rsADO.Close()
                                        }

                                        dtADONET = null;
                                    }
                                    else
                                    {
                                        sReturnMessage = Languages.get_Translation("msgTransfersNotAuthToTransfer");
                                    }
                                }
                            }
                        }

                        oScanList = null;
                        if (IDBManager is not null)
                        {
                            if (IDBManager.Connection is not null)
                            {
                                IDBManager.Close();
                                IDBManager.Dispose();
                            }
                        }
                    }
                }

                if (dtADONET is not null)
                {
                    if (dtADONET.Rows.Count > 0)
                    {
                        if (cTabsetIds is not null && cTabsetIds.Count > 0)
                        {
                            // If matching to the a list of Tabsets, validate it here.
                            oTabSets = GetTabset(passport, iTableTabQuery, iTabSetQuery, iRelationShipQuery, iTableQuery, ref oTable, cTabsetIds);

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
                        if (!(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)] is DBNull))
                        {
                            BarCodeLookupRet.Id = Convert.ToString(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName)]);
                            sReturnMessage = string.Empty;
                        }
                        BarCodeLookupRet.UserLinkId = BarCodeLookupRet.Id;
                        bool IfIdFieldIsString = IdFieldIsString(CommonIDBManager, oTable.TableName, oTable.IdFieldName);
                        if (!IfIdFieldIsString)
                        {
                            int oUserLinkTableIdSize = UserLinkIndexTableIdSize(IDBManagerDefault);
                            BarCodeLookupRet.UserLinkId = Strings.Right(new string('0', oUserLinkTableIdSize) + BarCodeLookupRet.UserLinkId, oUserLinkTableIdSize);
                        }
                        var IDueBackDays = -1;
                        DataTable dtADONet = null;
                        bool beof=false;
                        BarCodeLookupRet.UserDisplay = GetInfoUsingADONET.GetUserDisplayString(iDatabaseQuery, iSystemQuery, oTable, BarCodeLookupRet.Id,ref dtADONet, ref beof, sDescriptionOnly: ref sDescription,ref IDueBackDays, false, CommonIDBManager);
                        BarCodeLookupRet.Description = sDescription;
                        BarCodeLookupRet.HaveOut = !string.IsNullOrEmpty(Strings.Trim(oTable.TrackingOUTFieldName));
                        // 3.0.214 MEF 8/11/2000 - Fix to Not using OutTable
                        BarCodeLookupRet.IsOut = false;

                        if (BarCodeLookupRet.HaveOut)
                        {
                            if (!(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.TrackingOUTFieldName)] is DBNull))
                            {
                                BarCodeLookupRet.IsOut = Convert.ToBoolean(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.TrackingOUTFieldName)]);
                            }
                        }
                        else
                        {
                            if ((bool)(Convert.ToInt32(Enums.geTrackingOutType.otAlwaysOut) is var arg14 && oTable.OutTable is { } arg13 ? arg13 == arg14 : (bool?)null))
                            {
                                BarCodeLookupRet.HaveOut = true;
                                BarCodeLookupRet.IsOut = true;
                            }

                            if ((bool)(Convert.ToInt32(Enums.geTrackingOutType.otAlwaysIn) is var arg16 && oTable.OutTable is { } arg15 ? arg15 == arg16 : (bool?)null))
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
                            if (!(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.TrackingACTIVEFieldName)] is DBNull))
                            {
                                BarCodeLookupRet.IsActive = Convert.ToBoolean(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.TrackingACTIVEFieldName)]);
                            }
                        }
                        if (Strings.Len(oTable.TrackingDueBackDaysFieldName) > 0)
                        {
                            if (!(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.TrackingDueBackDaysFieldName)] is DBNull))
                            {
                                BarCodeLookupRet.DueBackDays = Convert.ToInt32(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.TrackingDueBackDaysFieldName)]);
                            }
                        }
                        if (BarCodeLookupRet.DueBackDays <= 0)
                            BarCodeLookupRet.DueBackDays = (int)iSystemQuery.OrderBy(m => m.Id).FirstOrDefault().DefaultDueBackDays;
                        if (!string.IsNullOrEmpty(oTable.TrackingEmailFieldName))
                        {
                            BarCodeLookupRet.EmailAddress = Convert.ToString(Interaction.IIf(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.TrackingEmailFieldName)] is DBNull, string.Empty, dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.TrackingEmailFieldName)]));
                            // BarCodeLookup.EmailAddress = dtADONET(0)(DatabaseMap.RemoveTableNameFromField(oTable.TrackingEmailFieldName))

                            if (string.IsNullOrEmpty(BarCodeLookupRet.EmailAddress))
                            {
                                BarCodeLookupRet.EmailAddress = GetEmailAddressFromOperator(iTableQuery, iDatabaseQuery, dtADONET, oTable, CommonIDBManager);
                            }
                        }
                        else
                        {
                            BarCodeLookupRet.EmailAddress = "<<Undefined>>";
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (IDBManager is not null)
                {
                    if (IDBManager.Connection is not null)
                    {
                        IDBManager.Close();
                        IDBManager.Dispose();
                    }
                }
            }

            return BarCodeLookupRet;

            // Dim d2 As DateTime = DateTime.Now
            // Dim s2 As String = d2.ToString("yyyy/MM/dd-HH/mm/ss.fff")
            // TrackingServices.WriteTxtFile("BarCodeLookup End       " + s2)
        }

        public static DataTable FindUserRecord(string sTableName, string sIdFieldName, string sIdFieldName2, string sActiveField, string sOutField, string sEmailAddress, string sDueBackDays, string sOperatorIdField, string sFindByFieldName, bool bFindFieldIsString, string sLookupStr, List<Table> iTableQuery, List<Databas> iDatabaseQuery, IDBManager CommonIDBManager)
        {
            string sSQL;
            string sError = string.Empty;
            try
            {
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
                string argsErrMsg = "";
                int arglError = 0;
                return GetADONETRecord(sSQL, CommonIDBManager, sErrMsg: ref argsErrMsg, lError: ref arglError);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public static int VerifyViewSecurity(Smead.Security.Passport passport, List<View> iViewQuery, List<Databas> iDatabaseQuery, Table oTable, string sLookup, string sFindByFieldName, bool bFindFieldIsString, IDBManager CommonIDBManager, bool bSecurityCheck = true)
        {
            int iPos;
            string sFrom;
            string sSQL;
            string sTemp;
            DataTable rsADOCount;

            var DBEngineObj = new DatabaseEngine();
            // Dim sADOCon As ADODB.Connection
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
                var lLoopViewEntities = iViewQuery.Where(x => x.TableName.Trim().ToLower()== oTable.TableName.Trim().ToLower() && Convert.ToInt32(Enums.geViewType.vtMaster) == x.ViewType);
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

                    sSQL = "";
                    sSQL = Strings.Replace(sSQL, string.Format("'{0}'", SL_USERNAME), string.Format("'{0}'", DBEngineObj.DBUserId.Replace("'", "''")), Compare: CompareMethod.Text);
                    string argsErrMsg = "";
                    int arglError = 0;
                    rsADOCount = GetADONETRecord(sSQL, CommonIDBManager, sErrMsg: ref argsErrMsg, lError: ref arglError);
                    if (rsADOCount is not null)
                    {
                        if (rsADOCount.Rows.Count > 0)
                        {
                            if (Operators.ConditionalCompareObjectGreater(rsADOCount.AsEnumerable().ElementAtOrDefault(0)["RCount"], 0, false))
                            {
                                // rsADOCount.Close()
                                rsADOCount = null;
                                return oView.Id;
                            }
                        }

                        // rsADOCount.Close()
                        rsADOCount = null;
                    }
                }
            }
            return 0;
        }

        public static string GetUserDisplayString(List<Databas> iDatabaseQuery,
            List<TabFusionRMS.Models.System> iSystemQuery, 
            Table oTable,
            string sLookupStr, 
            [Optional, DefaultParameterValue(null)] ref DataTable dtADONET,
            [Optional, DefaultParameterValue(false)] ref bool bEOF,
            [Optional, DefaultParameterValue("")] ref string sDescriptionOnly,
            [Optional, DefaultParameterValue(-1)] ref int lDueBackDays,
            [Optional, DefaultParameterValue(false)]  bool bCompleteDescription,
            [Optional, DefaultParameterValue(null)] IDBManager CommonIDBManager)
        {
            string GetUserDisplayStringRet = default;
            var bHadADO = default(bool);
            GetUserDisplayStringRet = "";
            try
            {
                if (dtADONET is not null)
                {
                    if (dtADONET.Rows.Count > 0)
                    {
                        bHadADO = true;
                    }
                }

                // If (sConStr Is Nothing) Then
                // Dim oDatabase = iDatabaseQuery.Where(Function(m) m.DBName.Trim.ToLower.Equals(oTable.DBName.Trim.ToLower)).FirstOrDefault
                // sConStr = Keys.GetDBConnectionString(oDatabase)
                // End If

                bool IfIdFieldIsString = IdFieldIsString(CommonIDBManager, oTable.TableName, oTable.IdFieldName);
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
                        sSQL += string.Format(" FROM [{0}] WHERE [{1}] = {2}", oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), IDQueryValue(IfIdFieldIsString, sLookupStr));

                        if (!bHadADO)
                        {
                            string argsErrMsg = "";
                            int arglError = 0;
                            dtADONET = GetADONETRecord(sSQL, CommonIDBManager, sErrMsg: ref argsErrMsg, lError: ref arglError);
                        }

                        if (lDueBackDays != -1)
                            lDueBackDays = GetDueBackDays(iDatabaseQuery, iSystemQuery, ref oTable, sLookupStr, ref dtADONET, CommonIDBManager);

                        if (dtADONET.Rows.Count == 0)
                        {
                            if (!bHadADO)
                            {
                                // rsADO.Close()
                                dtADONET = null;
                            }
                            bEOF = true;
                            return TrackingServices.StripLeadingZeros(sLookupStr);
                        }

                        if (!(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameOne)] is DBNull))
                        {
                            if (Strings.Len(oTable.DescFieldPrefixOne) > 0)
                                GetUserDisplayStringRet += Strings.Trim(oTable.DescFieldPrefixOne) + " ";
                            GetUserDisplayStringRet += Strings.Trim(Convert.ToString(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameOne)]));

                            if (Strings.StrComp(DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameOne), DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), Constants.vbTextCompare) != 0)
                            {
                                sDescriptionOnly += Strings.Trim(Convert.ToString(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameOne)]));
                            }
                            else if (Strings.StrComp(DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo), DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), Constants.vbTextCompare) != 0 & bCompleteDescription)
                            {
                                sDescriptionOnly += Strings.Trim(Convert.ToString(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameOne)]));
                            }
                        }

                        if (Strings.Len(oTable.DescFieldNameTwo) > 0)
                        {
                            if (!(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo)] is DBNull))
                            {
                                if (Strings.Len(oTable.DescFieldPrefixTwo) > 0)
                                    GetUserDisplayStringRet += " " + Strings.Trim(oTable.DescFieldPrefixTwo);
                                GetUserDisplayStringRet += " " + Strings.Trim(Convert.ToString(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo)]));

                                if (Strings.StrComp(DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo), DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), Constants.vbTextCompare) != 0 | bCompleteDescription)
                                {
                                    if (Strings.Len(sDescriptionOnly) > 0)
                                    {
                                        if (Strings.Len(oTable.DescFieldPrefixTwo) > 0)
                                        {
                                            sDescriptionOnly += " " + Strings.Trim(oTable.DescFieldPrefixTwo) + " " + Strings.Trim(Convert.ToString(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo)]));
                                        }
                                        else
                                        {
                                            sDescriptionOnly += " - " + Strings.Trim(Convert.ToString(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo)]));
                                        }
                                    }
                                    else
                                    {
                                        sDescriptionOnly = Strings.Trim(Convert.ToString(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.DescFieldNameTwo)]));
                                    }
                                }
                            }
                        }
                    }
                    else if (lDueBackDays != -1)
                    {
                        lDueBackDays = GetInfoUsingADONET.GetDueBackDays(iDatabaseQuery, iSystemQuery, ref oTable, sLookupStr, ref CommonIDBManager);
                    }

                    if (string.IsNullOrEmpty(GetUserDisplayStringRet))
                        GetUserDisplayStringRet = TrackingServices.StripLeadingZeros(sLookupStr);
                }

                if (!bHadADO && dtADONET is not null)
                {
                    // rsADO.Close()
                    dtADONET = null;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return GetUserDisplayStringRet;
        }

        private static int GetDueBackDays(List<Databas> iDatabaseQuery, List<TabFusionRMS.Models.System> iSystemQuery, ref Table oTable, string sLookupStr, ref IDBManager commonIDBManager)
        {
            throw new NotImplementedException();
        }

        public static int GetDueBackDays(List<Databas> iDatabaseQuery, List<TabFusionRMS.Models.System> iSystemQuery, ref Table oTable, string sLookupStr, [Optional, DefaultParameterValue(null)] ref DataTable dtADONET, IDBManager CommonIDBManager = null)
        {
            int lDueBackDays = -1;
            string sFieldName;

            if (oTable is not null && Strings.Len(oTable.TrackingDueBackDaysFieldName) > 0)
            {
                sFieldName = DatabaseMap.RemoveTableNameFromField(oTable.TrackingDueBackDaysFieldName);
                if (dtADONET is not null)
                {
                    if (Strings.Len(sFieldName) > 0 && dtADONET.Rows.Count > 0)
                        lDueBackDays = Convert.ToInt32(dtADONET.AsEnumerable().ElementAtOrDefault(0)[sFieldName]);
                }
                else if (Strings.Len(Strings.Trim(sLookupStr)) > 0 & Strings.Len(sFieldName) > 0)
                {
                    bool IfIdFieldIsString = IdFieldIsString(CommonIDBManager, oTable.TableName, oTable.IdFieldName);
                    string sSQL = string.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = {3}", sFieldName, oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), DataServices.IDQueryValue(IfIdFieldIsString, sLookupStr));
                    string argsErrMsg = "";
                    int arglError = 0;
                    dtADONET = GetADONETRecord(sSQL, CommonIDBManager, sErrMsg: ref argsErrMsg, lError: ref arglError);
                    if (dtADONET is not null)
                    {
                        if (!(dtADONET.Rows.Count > 0))
                        {
                            if (dtADONET.AsEnumerable().ElementAtOrDefault(0)[sFieldName] is DBNull)
                            {
                                lDueBackDays = Convert.ToInt32(dtADONET.AsEnumerable().ElementAtOrDefault(0)[sFieldName]);
                            }
                        }
                        // rsADO.Close()
                    }
                }
            }
            if (lDueBackDays <= 0)
                lDueBackDays = (int)iSystemQuery.OrderBy(m => m.Id).FirstOrDefault().DefaultDueBackDays;
            return lDueBackDays;
        }

        public static string GetEmailAddressFromOperator(List<Table> iTableQuery, List<Databas> iDatabaseQuery, DataTable dtADONET, Table oTable, IDBManager CommonIDBManager)
        {
            if (string.IsNullOrEmpty(oTable.OperatorsIdField))
                return string.Empty;
            if (string.IsNullOrEmpty(Convert.ToString(dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.OperatorsIdField)])))
                return string.Empty;
            var tableObj = iTableQuery.Where(m => m.TableName.Equals("SecureUser")).FirstOrDefault();
            string accountType = "A";
            string sql = string.Format("SELECT TOP 1 Email FROM SecureUser WHERE UserName = '{0}' AND AccountType <> '{1}' AND AccountType IS NOT NULL", dtADONET.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oTable.OperatorsIdField)].ToString().Replace("'", "''"), accountType);
            // sADOCon = DataServices.DBOpen(oTable, iDatabaseQuery)
            string argsErrMsg = "";
            int arglError = 0;
            var dtUser = GetADONETRecord(sql, CommonIDBManager, sErrMsg: ref argsErrMsg, lError: ref arglError);
            if (dtUser is null)
                return string.Empty;

            try
            {
                if (dtUser.Rows.Count > 0)
                    return Convert.ToString(dtUser.AsEnumerable().ElementAtOrDefault(0)[0]);
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            finally
            {
                // rsUser.Close()
                dtUser = null;
            }
        }

        public static string ValidateTracking(List<Databas> data_iDatabas,
            List<Table> data_iTables, List<TabFusionRMS.Models.System> data_iSystem,
            List<ScanList> data_iScanList, List<View> data_iView, List<TableTab> data_iTableTab,
            List<TabSet> data_iTabSet, List<RelationShip> data_iRelationship,
            ref FoundBarCode oTrackObject, ref FoundBarCode oTrackDestination,
            string sDestination, bool bUseMaster, Passport passport, HttpContext httpContext,
            [Optional, DefaultParameterValue(null)] FoundBarCode moTrackDestination,
            [Optional, DefaultParameterValue(0)] ref int errNum,
            IDBManager IDBManagerDefault = null, bool IsDestination = false)
        {
            string ValidateTrackingRet = default;
            if (bUseMaster)
            {
                oTrackDestination = moTrackDestination;
            }
            else
            {
                Collection argcTabsetIds = null;
                Table argoCurrentTable = null;
                bool argbSecurityCheck = false;
                string argsReturnMessage = "";
                oTrackDestination = BarCodeLookup(null, data_iDatabas, data_iTables, data_iSystem, data_iScanList, data_iView, data_iTableTab, data_iTabSet, data_iRelationship, sDestination, httpContext, cTabsetIds: ref argcTabsetIds, oCurrentTable: ref argoCurrentTable, bSecurityCheck: ref argbSecurityCheck, IDBManagerDefault, IsDestination, sReturnMessage: ref argsReturnMessage);
            }

            if (oTrackDestination is null)
            {
                ValidateTrackingRet = string.Format(Languages.get_Translation("msgImportCtrlTrackingDestination"), sDestination);
                // ValidateTracking = "Tracking Destination " & """" & sDestination & """" & " is not a valid Container."
                errNum = 28;
            }
            else if ((bool)(0 is var arg22 && oTrackDestination.oTable.TrackingTable is { } arg21 ? arg21 == arg22 : (bool?)null))
            {
                ValidateTrackingRet = string.Format(Languages.get_Translation("msgImportCtrlTrackingDestination"), sDestination);
                errNum = 28;
            }
            else
            {

                bool oTableTrackable = passport.CheckPermission(oTrackObject.oTable.TableName.Trim(), (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Transfer);
                if (!oTableTrackable)
                {
                    ValidateTrackingRet = string.Format(Languages.get_Translation("msgImportCtrlTrackingObjectNotConfig"), oTrackObject.oTable.UserName);
                    errNum = 29;
                }
                else if (oTrackDestination.IsActive)
                {
                    if ((bool)((0 is var arg24 && oTrackObject.oTable.TrackingTable is { } arg23 ? arg23 != arg24 : (bool?)null) & (oTrackObject.oTable.TrackingTable is var arg25 && oTrackDestination.oTable.TrackingTable is { } arg26 && arg25.HasValue ? arg25 <= arg26 : (bool?)null)))
                    {
                        ValidateTrackingRet = string.Format(Languages.get_Translation("msgImportCtrlTrackingObjectNotFit"), oTrackObject.oTable.UserName, sDestination);
                        errNum = 30;
                    }
                    else
                    {
                        var argoTable = oTrackObject.oTable;
                        string argsId = oTrackObject.Id;
                        ValidateTrackingRet = GetInfoUsingADONET.ValidateRetention(null, data_iDatabas, ref argoTable, ref argsId, IDBManagerDefault, httpContext);
                        oTrackObject.oTable = argoTable;
                        oTrackObject.Id = argsId;
                    }
                }
                else
                {
                    ValidateTrackingRet = string.Format(Languages.get_Translation("msgImportCtrlIsCurrentlyInActive"), sDestination);
                    errNum = 31;
                }
            }

            return ValidateTrackingRet;
        }

        public static int UserLinkIndexTableIdSize(IDBManager CommonIDBManager)
        {
            int miUserLinkIndexTableIdSize;
            var pSchemaInfo = GetSchemaInfo(CommonIDBManager, "USERLINKS", "INDEXTABLEID");
            if (pSchemaInfo.Rows.Count > 0)
            {
                try
                {
                    miUserLinkIndexTableIdSize = Convert.ToInt32(pSchemaInfo.AsEnumerable().ElementAtOrDefault(0)["CHARACTER_MAXIMUM_LENGTH"]);
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
            return miUserLinkIndexTableIdSize;
        }

        public static string ValidateRetention(Smead.Security.Passport passport, List<Databas> iDatabaseQuery, ref Table oTable, ref string sId, IDBManager IDBManagerDefault, HttpContext httpContext)
        {
           
            if (oTable is not null & Strings.Len(sId) > 0)
            {
                if (IsInactiveRecord(oTable, sId, IDBManagerDefault, iDatabaseQuery, httpContext))
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
                if (GetInfoUsingADONET.IsArchivedOrDestroyed(oTable, sId, Enums.meFinalDispositionStatusType.fdstArchived, IDBManagerDefault, iDatabaseQuery, httpContext))
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
                if (GetInfoUsingADONET.IsArchivedOrDestroyed(oTable, sId, Enums.meFinalDispositionStatusType.fdstDestroyed, IDBManagerDefault, iDatabaseQuery, httpContext))
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

        public static bool IsInactiveRecord(Table oTable, string sId, IDBManager IDBManagerDefault, List<Databas> iDatabaseQuery, HttpContext httpContext)
        {
            IDBManager IDbManager = null;
            try
            {
                if (!string.IsNullOrEmpty(oTable.RetentionFieldName) && oTable.RetentionInactivityActive.GetValueOrDefault())
                {
                    IDBManager CommonDBManager = null;
                    if (!string.IsNullOrEmpty(oTable.DBName))
                    {
                        var oDatabase = iDatabaseQuery.Where(m => m.DBName.Trim().ToLower().Equals(oTable.DBName.Trim().ToLower())).FirstOrDefault();
                        IDbManager.ConnectionString = Keys.get_GetDBConnectionString(oDatabase);
                        IDbManager.Open();
                        CommonDBManager = IDbManager;
                    }
                    else
                    {
                        CommonDBManager = IDBManagerDefault;
                    }
                    bool IfIdFieldIsString = IdFieldIsString(CommonDBManager, oTable.TableName, oTable.IdFieldName);
                    string sSQL = string.Format("SELECT [%slRetentionInactiveFinal] FROM [{0}] WHERE [{1}] = {2}", oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), DataServices.IDQueryValue(IfIdFieldIsString, sId));

                    string argsErrMsg = "";
                    int arglError = 0;
                    var rs = GetADONETRecord(sSQL, CommonDBManager, sErrMsg: ref argsErrMsg, lError: ref arglError);

                    try
                    {
                        if (rs is not null && rs.Rows.Count > 0)
                        {
                            return Convert.ToBoolean(rs.Rows[0][0]);
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (IDbManager is not null)
                {
                    if (IDbManager.Connection is not null)
                    {
                        IDbManager.Close();
                        IDbManager.Dispose();
                    }
                }
            }

            return false;
        }

        public static bool IsArchivedOrDestroyed(Table oTable, string sId, Enums.meFinalDispositionStatusType eCompareType, IDBManager IDBManagerDefault, List<Databas> iDatabaseQuery, HttpContext httpContext)
        {
            IDBManager IDbManager = null;
            bool rtn = false;
            try
            {
                if (!string.IsNullOrEmpty(oTable.RetentionFieldName) && oTable.RetentionPeriodActive.GetValueOrDefault())
                {
                    IDBManager CommonDBManager = null;
                    if (!string.IsNullOrEmpty(oTable.DBName))
                    {
                        var oDatabase = iDatabaseQuery.Where(m => m.DBName.Trim().ToLower().Equals(oTable.DBName.Trim().ToLower())).FirstOrDefault();
                        IDbManager.ConnectionString = Keys.get_GetDBConnectionString(oDatabase);
                        IDbManager.Open();
                        CommonDBManager = IDbManager;
                    }
                    else
                    {
                        CommonDBManager = IDBManagerDefault;
                    }
                    bool IfIdFieldIsString = IdFieldIsString(CommonDBManager, oTable.TableName, oTable.IdFieldName);
                    Enums.meFinalDispositionStatusType eStatusType;
                    string sSQL = string.Format("SELECT [%slRetentionDispositionStatus] FROM [{0}] WHERE [{1}] = {2}", oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), DataServices.IDQueryValue(IfIdFieldIsString, sId));

                    string argsErrMsg = "";
                    int arglError = 0;
                    var rs = GetADONETRecord(sSQL, CommonDBManager, sErrMsg: ref argsErrMsg, lError: ref arglError);

                    if (rs is not null && rs.Rows.Count > 0)
                    {
                        try
                        {
                            if (!(rs.Rows[0][0] is DBNull))
                            {
                                eStatusType = (Enums.meFinalDispositionStatusType)rs.Rows[0][0];
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
                        }
                        catch (Exception)
                        {
                            rtn = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (IDbManager is not null)
                {
                    if (IDbManager.Connection is not null)
                    {
                        IDbManager.Close();
                        IDbManager.Dispose();
                    }
                }
            }

            return rtn;
        }

        public static Table MakeSureIsALoadedTable(string sTableName, IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject, IDBManager _IDBManagerDefault, string sUserName = "")
        {
            var oTables = new Table();
            var dbEngine = new DatabaseEngine();
            oTables.TableName = sTableName.Trim();
            oTables.IdFieldName2 = "";
            if (dbEngine.DBName != "DB_Engine")
            {
                oTables.DBName = dbEngine.DBName;
            }
            else
            {
                oTables.DBName = "";
            }

            switch (sTableName.ToUpper() ?? "")
            {
                case "SYSNEXTTRACKABLE":
                    {
                        oTables.IdFieldName = "NextTrackablesId";
                        break;
                    }
                case "LITIGATIONSUPPORT":
                    {
                        oTables.IdFieldName = "CaseNumber";
                        break;
                    }
                case "LINKSCRIPT":
                    {
                        oTables.IdFieldName = "ScriptName";
                        oTables.IdFieldName2 = "ScriptSequence";
                        break;
                    }
                case "TRACKABLES":
                    {
                        oTables.IdFieldName = "Id";
                        oTables.IdFieldName2 = "RecordVersion";
                        break;
                    }
                case "DATABASES":
                    {
                        oTables.IdFieldName = "DBName";
                        break;
                    }
                case "TABLES":
                    {
                        oTables.IdFieldName = "TableName";
                        break;
                    }
                case "SLRETENTIONCODES":
                    {
                        oTables.TableName = "SLRetentionCodes";
                        oTables.IdFieldName = "Id";
                        oTables.DescFieldPrefixOne = "Retention Code";
                        oTables.DescFieldNameOne = "Id";
                        oTables.DescFieldPrefixTwo = "Retention Desc";
                        oTables.DescFieldNameTwo = "Description";
                        if (Strings.Len(sUserName) == 0)
                            sUserName = "Retention Codes";

                        if (!GetInfoUsingADONET.CheckIfObjectRegister(sTableName, Enums.SecureObjectType.Table, _iSecureObject))
                        {
                            GetInfoUsingADONET.SetRegisterTable(sTableName, Enums.SecureObjectType.Table, _iSecureObject, _IDBManagerDefault);
                        }

                        break;
                    }
                // SLLOGGEDINUSERS is no longer being used.  RVW 08/23/2006
                case "SLRETENTIONCITATIONS":
                    {
                        oTables.TableName = "SLRetentionCitations";
                        oTables.IdFieldName = "Citation";
                        if (Strings.Len(sUserName) == 0)
                            sUserName = "Citations Codes";

                        if (!GetInfoUsingADONET.CheckIfObjectRegister(sTableName, Enums.SecureObjectType.Table, _iSecureObject))
                        {
                            GetInfoUsingADONET.SetRegisterTable(sTableName, Enums.SecureObjectType.Table, _iSecureObject, _IDBManagerDefault);
                        }

                        break;
                    }
                case "SLRETENTIONCITACODES":
                    {
                        oTables.TableName = "SLRetentionCitaCodes";
                        oTables.IdFieldName = "Id";
                        if (Strings.Len(sUserName) == 0)
                            sUserName = "Retention Citations Cross Reference";
                        if (!GetInfoUsingADONET.CheckIfObjectRegister(sTableName, Enums.SecureObjectType.Table, _iSecureObject))
                        {
                            GetInfoUsingADONET.SetRegisterTable(sTableName, Enums.SecureObjectType.Table, _iSecureObject, _IDBManagerDefault);
                        }

                        break;
                    }
                case "OPERATORS":
                    {
                        oTables.IdFieldName = "UserName";
                        break;
                    }
                case "SECUREGROUP":
                    {
                        oTables.IdFieldName = "GroupId";
                        break;
                    }
                case "SECUREOBJECT":
                    {
                        oTables.IdFieldName = "SecureObjectId";
                        break;
                    }
                case "SECUREOBJECTPERMISSION":
                    {
                        oTables.IdFieldName = "SecureObjectPermissionId";
                        break;
                    }
                case "SECUREPERMISSION":
                    {
                        oTables.IdFieldName = "PermissionId";
                        break;
                    }
                case "SECUREPERMISSIONDESCRIPTION":
                    {
                        oTables.IdFieldName = "SecureObjectId";
                        oTables.IdFieldName2 = "PermissionId";
                        break;
                    }
                case "SECUREUSER":
                    {
                        oTables.IdFieldName = "UserId";
                        break;
                    }

                default:
                    {
                        oTables.IdFieldName = "Id";
                        break;
                    }
            }

            oTables.OutTable = Convert.ToInt16(Enums.geTrackingOutType.otUseOutField);
            oTables.IdFieldName = sTableName + "." + oTables.IdFieldName;
            oTables.ADOQueryTimeout = 30;
            oTables.ADOCacheSize = 1;
            oTables.ADOServerCursor = false;
            if (Strings.Len(sUserName) > 0)
            {
                oTables.UserName = sUserName;
            }
            else
            {
                oTables.UserName = sTableName;

            }

            return oTables;

        }

        public static bool CheckIfObjectRegister(string objName, Enums.SecureObjectType objType, IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject)
        {
            try
            {
                bool returnFlag = false;
                if (objName is not null & !string.IsNullOrEmpty(objName))
                {
                    var objId = _iSecureObject.Where(m => m.SecureObjectTypeID == (int)objType & m.Name.Trim().ToLower().Equals(objName.Trim().ToLower()));
                    if (objId is null)
                    {
                        returnFlag = false;
                    }
                    else
                    {
                        returnFlag = true;
                    }

                }
                return returnFlag;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void SetRegisterTable(string objName, Enums.SecureObjectType objType,
            IRepository<TabFusionRMS.Models.SecureObject> _iSecureObject, IDBManager _IDBManagerDefault)
        {
            try
            {
                if (objName is not null & !string.IsNullOrEmpty(objName))
                {
                    var SecureObjEntity = new TabFusionRMS.Models.SecureObject();
                    var TableTypeId = _iSecureObject.All().Where(m => m.Name == "Tables").FirstOrDefault();
                    if (TableTypeId is not null)
                    {
                        SecureObjEntity.SecureObjectTypeID = TableTypeId.SecureObjectTypeID;
                        SecureObjEntity.BaseID = TableTypeId.SecureObjectTypeID;
                    }
                    SecureObjEntity.Name = objName.Trim();
                    _iSecureObject.Add(SecureObjEntity);
                    _iSecureObject.CommitTransaction();
                    var pSecureObject = _iSecureObject.All().Where(x => x.Name.Trim().ToLower().Equals(objName.Trim().ToLower())).FirstOrDefault();
                    string sSql = "INSERT INTO SecureObjectPermission (GroupID, SecureObjectID, PermissionID) SELECT GroupID," + pSecureObject.SecureObjectID + " AS SecureObjectId, PermissionID FROM SecureObjectPermission AS SecureObjectPermission WHERE     (SecureObjectID = " + pSecureObject.BaseID + ") AND (PermissionID IN (SELECT     PermissionID FROM          SecureObjectPermission AS SecureObjectPermission_1 WHERE (SecureObjectID = " + ((int)objType).ToString() + ") AND (GroupID = 0)))";
                    _IDBManagerDefault.ExecuteNonQuery(CommandType.Text, sSql);
                }
            }

            catch (Exception)
            {
                throw;
            }
        }

        public static string PrepareSQLUsingList(bool bDoAddNew, bool bFound,
            List<ClsFieldWithVal> lstField, Table oTable,
            [Optional, DefaultParameterValue("")] string strId,
            [Optional, DefaultParameterValue(false)] bool IsIdFieldString,
            [Optional, DefaultParameterValue(false)] bool IsIdAutoIncrement,
            [Optional, DefaultParameterValue("")] string UpdateByFieldName,
            [Optional, DefaultParameterValue("")] string UpdateByFieldVal,
            [Optional, DefaultParameterValue("")] ref string sReturnMessage)
        {
            string sqlFinalInsertUpdate = string.Empty;
            string colName = string.Empty;
            string colVal = string.Empty;
            string IdFieldName = DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName);
            try
            {
                if (bFound == true & bDoAddNew == true)
                {
                    if (lstField.Count != 0)
                    {
                        foreach (ClsFieldWithVal oObject in lstField)
                        {
                            colName += string.Format("[{0}],", oObject.colName);
                            switch (oObject.colType.FullName.ToString().ToLower() ?? "")
                            {
                                case "system.int32":
                                case "system.int16":
                                case "system.int64":
                                case "system.double":
                                case "system.decimal":
                                    {
                                        if (oObject.colVal is null)
                                        {
                                            colVal += string.Format("'{0}',", oObject.colVal);
                                        }
                                        else
                                        {
                                            colVal += string.Format("{0},", oObject.colVal);
                                        }

                                        break;
                                    }
                                case "system.boolean":
                                    {
                                        if (oObject.colVal is not null)
                                        {
                                            if (oObject.colVal.ToLower() == "true" | oObject.colVal.ToLower() == "false")
                                            {
                                                colVal += string.Format("'{0}',", oObject.colVal);
                                            }
                                            else
                                            {
                                                colVal += string.Format("{0},", oObject.colVal);
                                            }
                                        }
                                        else
                                        {
                                            colVal += string.Format("'{0}',", oObject.colVal);
                                        }

                                        break;
                                    }
                                case "system.datetime":
                                    {
                                        if (oObject.colVal is not null)
                                        {
                                            colVal += string.Format("'{0}',", oObject.colVal);
                                        }
                                        else
                                        {
                                            colVal += string.Format("{0},", "null");
                                        }

                                        break;
                                    }

                                default:
                                    {
                                        colVal += string.Format("'{0}',", oObject.colVal.Replace("'", "''"));
                                        break;
                                    }
                            }
                        }

                        colName = colName.Substring(0, colName.Length - 1);
                        colVal = colVal.Substring(0, colVal.Length - 1);
                        if (IsIdAutoIncrement == true)
                        {
                            if (IsIdFieldString)
                            {
                                sqlFinalInsertUpdate = string.Format("Insert into [{0}] ({1}) values ({2});Select * from [{0}] where [{3}] = 'SCOPE_IDENTITY()'", oTable.TableName, colName, colVal, IdFieldName);
                            }
                            else
                            {
                                sqlFinalInsertUpdate = string.Format("Insert into [{0}] ({1}) values ({2});Select * from [{0}] where [{3}] = SCOPE_IDENTITY()", oTable.TableName, colName, colVal, IdFieldName);
                            }
                        }
                        else
                        {
                            sqlFinalInsertUpdate = string.Format("Insert into [{0}] ({1}) values ({2});Select * from [{0}] where [{3}] = '{4}'", oTable.TableName, colName, colVal, IdFieldName, strId);
                        }
                    }
                    else if (IsIdAutoIncrement == false)
                    {
                        if (IsIdFieldString)
                        {
                            sqlFinalInsertUpdate = string.Format("Insert into [{0}] ({1}) values ('{2}');Select * from [{0}] where [{1}] = 'SCOPE_IDENTITY()'", oTable.TableName, IdFieldName, strId);
                        }
                        else
                        {
                            sqlFinalInsertUpdate = string.Format("Insert into [{0}] ({1}) values ({2});Select * from [{0}] where [{1}] = SCOPE_IDENTITY()", oTable.TableName, IdFieldName, strId);
                        }
                    }
                    else
                    {
                        sReturnMessage = Languages.get_Translation("msgImportCtrlEmptyRowCanNotBeInserted");
                    }
                }
                else if (lstField.Count != 0)
                {
                    sqlFinalInsertUpdate = string.Format("Update [{0}] Set ", oTable.TableName);
                    foreach (ClsFieldWithVal oObject in lstField)
                    {
                        if (!(IsIdAutoIncrement == true & oObject.colName.Trim().ToLower().Equals(IdFieldName.Trim().ToLower())))
                        {
                            switch (oObject.colType.FullName.ToString().ToLower() ?? "")
                            {
                                case "system.int32":
                                case "system.int16":
                                case "system.int64":
                                case "system.double":
                                case "system.decimal":
                                    {
                                        if (oObject.colVal is null)
                                        {
                                            sqlFinalInsertUpdate += string.Format("[{0}]='{1}',", oObject.colName, oObject.colVal);
                                        }
                                        else
                                        {
                                            sqlFinalInsertUpdate += string.Format("[{0}]={1},", oObject.colName, oObject.colVal);
                                        }

                                        break;
                                    }

                                case "system.boolean":
                                    {
                                        if (oObject.colVal is not null)
                                        {
                                            if (oObject.colVal.ToLower() == "true" | oObject.colVal.ToLower() == "false")
                                            {
                                                sqlFinalInsertUpdate += string.Format("[{0}]='{1}',", oObject.colName, oObject.colVal);
                                            }
                                            else
                                            {
                                                sqlFinalInsertUpdate += string.Format("[{0}]={1},", oObject.colName, oObject.colVal);
                                            }
                                        }
                                        else
                                        {
                                            sqlFinalInsertUpdate += string.Format("[{0}]='{1}',", oObject.colName, oObject.colVal);
                                        }

                                        break;
                                    }
                                case "system.datetime":
                                    {
                                        if (oObject.colVal is not null)
                                        {
                                            sqlFinalInsertUpdate += string.Format("[{0}]='{1}',", oObject.colName, oObject.colVal);
                                        }
                                        else
                                        {
                                            sqlFinalInsertUpdate += string.Format("[{0}]={1},", oObject.colName, "null");
                                        }

                                        break;
                                    }

                                default:
                                    {
                                        sqlFinalInsertUpdate += string.Format("[{0}]='{1}',", oObject.colName, oObject.colVal);
                                        break;
                                    }
                            }
                        }
                    }
                    sqlFinalInsertUpdate = sqlFinalInsertUpdate.Substring(0, sqlFinalInsertUpdate.Length - 1);
                    if (IsIdFieldString)
                    {
                        sqlFinalInsertUpdate += string.Format(" where [{3}] = '{4}';Select * from [{2}] where [{0}] = '{1}'", IdFieldName, strId, oTable.TableName, UpdateByFieldName, UpdateByFieldVal);
                    }
                    else
                    {
                        sqlFinalInsertUpdate += string.Format(" where [{3}] = {4};Select * from [{2}] where [{0}] = {1}", IdFieldName, strId, oTable.TableName, UpdateByFieldName, UpdateByFieldVal);
                    }
                }
                else if (IsIdAutoIncrement == false)
                {
                    sqlFinalInsertUpdate = string.Format("Update [{0}] Set ", oTable.TableName);
                    if (IsIdFieldString)
                    {
                        sqlFinalInsertUpdate += string.Format("[{0}]='{1}';", IdFieldName, strId);
                    }
                    else
                    {
                        sqlFinalInsertUpdate += string.Format("[{0}]={1};", IdFieldName, strId);
                    }
                }
                else if (IsIdFieldString)
                {
                    sqlFinalInsertUpdate += string.Format("Select * from [{2}] where [{0}] = '{1}'", IdFieldName, strId, oTable.TableName);
                }
                else
                {
                    sqlFinalInsertUpdate += string.Format("Select * from [{2}] where [{0}] = {1}", IdFieldName, strId, oTable.TableName);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return sqlFinalInsertUpdate;
        }

        public static bool UpdateFullTextIndexerFieldType(string sId, Table oTable, DataTable oDatatable, bool bAddMode, List<SLTextSearchItem> _iSLTextSearchItems, IRepository<SLIndexer> _iSLIndexer)
        {
            bool UpdateFullTextIndexerFieldTypeRet = default;
            string sSQL = "";
            string sValue = "";
            SLIndexer oSLIndexer;
            // Dim oField As ADODB.Field
            bool HaveATableWithFullTextSearch = false;
            try
            {
                UpdateFullTextIndexerFieldTypeRet = true;
                if (!string.IsNullOrEmpty(oTable.TableName))
                {
                    var slTextSearchItem = _iSLTextSearchItems.Where(m => m.IndexTableName.Trim().ToLower().Equals(oTable.TableName.Trim().ToLower()));
                    if (slTextSearchItem is not null)
                    {
                        HaveATableWithFullTextSearch = slTextSearchItem.Count() > 0;
                        if (HaveATableWithFullTextSearch)
                        {
                            foreach (DataColumn oColumn in oDatatable.Columns)
                            {
                                foreach (var oSLTextSearchItem in slTextSearchItem)
                                {
                                    if (Strings.StrComp(oColumn.ColumnName, oSLTextSearchItem.IndexFieldName, CompareMethod.Text) == 0)
                                    {
                                        if (!(oDatatable.AsEnumerable().ElementAtOrDefault(0)[oColumn] is DBNull))
                                        {
                                            sValue = Strings.Replace(Convert.ToString(oDatatable.AsEnumerable().ElementAtOrDefault(0)[oColumn]), "'", "''");
                                        }
                                        else
                                        {
                                            sValue = "";
                                        }
                                        if (!string.IsNullOrEmpty(sValue))
                                        {
                                            var slIndexerObj = new SLIndexer();
                                            oSLIndexer = slIndexerObj;

                                            {
                                                ref var withBlock = ref oSLIndexer;
                                                withBlock.IndexType = (int)Enums.geFTSTypes.tsFieldName;
                                                withBlock.IndexTableName = oTable.TableName;
                                                withBlock.IndexTableId = sId;
                                                withBlock.IndexFieldName = oSLTextSearchItem.IndexFieldName;
                                            }

                                            if (bAddMode)
                                            {
                                                oSLIndexer.OrphanType = 0;
                                                oSLIndexer.IndexData = sValue;
                                                if (oSLIndexer is not null)
                                                {
                                                    _iSLIndexer.Add(oSLIndexer);
                                                    oSLIndexer = oSLIndexer;
                                                }
                                            }
                                            else
                                            {
                                                oSLIndexer.OrphanType = 0;
                                                var oSLIndexerList = _iSLIndexer.Where(x =>    x.IndexType      == (int)Enums.geFTSTypes.tsFieldName
                                                                                            && x.IndexTableName == oTable.TableName
                                                                                            && x.IndexFieldName == oSLTextSearchItem.IndexFieldName
                                                                                            && x.IndexTableId   == sId
                                                                                            && x.OrphanType     == oSLIndexer.OrphanType
                                                                                      );
                                                if (oSLIndexerList.Count() != 0)
                                                {
                                                    oSLIndexer = oSLIndexerList.First();
                                                    oSLIndexer.IndexData = sValue;
                                                    if (oSLIndexer is not null)
                                                    {
                                                        _iSLIndexer.Update(oSLIndexer);
                                                    }
                                                }
                                                else
                                                {
                                                    oSLIndexer = new SLIndexer();
                                                    oSLIndexer.IndexType = (int)Enums.geFTSTypes.tsFieldName;
                                                    oSLIndexer.IndexTableName = oTable.TableName;
                                                    oSLIndexer.IndexTableId = sId;
                                                    oSLIndexer.IndexFieldName = oSLTextSearchItem.IndexFieldName;
                                                    oSLIndexer.IndexData = sValue;
                                                    if (oSLIndexer is not null)
                                                    {
                                                        _iSLIndexer.Add(oSLIndexer);
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
                }
            }
            catch (Exception)
            {
                throw;
            }

            return UpdateFullTextIndexerFieldTypeRet;
        }

        public static TabSet GetTabset(Smead.Security.Passport passport, List<TableTab> iTableTabQuery, List<TabSet> iTabSetQuery, List<RelationShip> iRelationShipQuery, List<Table> iTableQuery, ref Table oTables, Collection cTabsetIds = null, bool bSecurityCheck = false)
        {
            TabSet GetTabsetRet = default;
            Collection cTabsets;
            int iCnt;
            int lCount;
            GetTabsetRet = null;
            bool @bool;
            try
            {
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
                        if ((bool)(Strings.StrComp(oTableTabs.TableName, oTables.TableName, CompareMethod.Text) == 0 & (Convert.ToInt32(cTabsets[iCnt]) is var arg36 && oTableTabs.TabSet is { } arg35 ? arg35 == arg36 : null)))
                        {
                            GetTabsetRet = tabSetObj1.AsEnumerable().ElementAt(iCnt);
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
            }
            catch (Exception)
            {
                throw;
            }

            return GetTabsetRet;
        }

        public static TabSet GetParentTabSet(Smead.Security.Passport passport, ref Table oTables, int lLevel, int lTabsetId, bool bSecurityCheck, List<TableTab> iTableTabQuery, List<TabSet> iTabSetQuery, List<RelationShip> iRelationShipQuery, List<Table> iTableQuery)
        {
            Table oUpTables;
            var oFindTabSets = new TabSet();

            if (lLevel > 100)
            {
                // A little recursive???
                return null;
            }
            var tableTabObj = iTableTabQuery.OrderBy(m => m.Id);
            var tabSetObj = iTabSetQuery.OrderBy(m => m.Id);
            string oTableDbName = oTables.TableName.Trim().ToLower();
            var oParentTableList = iRelationShipQuery.Where(m => m.LowerTableName.Trim().ToLower().Equals(oTableDbName));
            bool @bool;
            foreach (var oRelationships in oParentTableList)
            {
                if (Strings.StrComp(oRelationships.LowerTableName, oTables.TableName, CompareMethod.Text) == 0)
                {
                    foreach (var oTableTabs in tableTabObj)
                    {
                        if ((Strings.StrComp(oTableTabs.TableName, oRelationships.UpperTableName, CompareMethod.Text) == 0) && (oTableTabs.TabSet == lTabsetId))
                        {
                            oFindTabSets = (TabSet)tabSetObj.Where(m => m.Id == oTableTabs.TabSet);
                            if (oFindTabSets is not null && bSecurityCheck)
                            {
                                if (passport is null)
                                {
                                    @bool = passport.CheckPermission(oFindTabSets.UserName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.WorkGroup, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                                }
                                else
                                {
                                    @bool = passport.CheckPermission(oFindTabSets.UserName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.WorkGroup, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                                }
                                if (@bool)
                                {
                                    oFindTabSets = null;
                                }
                            }
                            return oFindTabSets;
                        }
                    }
                }
                // Recursively Check upstream
                oUpTables = iTableQuery.Where(m => m.TableName.Trim().ToLower().Equals(oRelationships.UpperTableName.Trim().ToLower())).FirstOrDefault();
                if (oUpTables is not null)
                {
                    oFindTabSets = GetParentTabSet(passport, ref oUpTables, lLevel + 1, lTabsetId, bSecurityCheck, iTableTabQuery, iTabSetQuery, iRelationShipQuery, iTableQuery);

                    if (oFindTabSets is not null && bSecurityCheck)
                    {
                        if (passport is null)
                        {
                            @bool = passport.CheckPermission(oFindTabSets.UserName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.WorkGroup, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                        }
                        else
                        {
                            @bool = passport.CheckPermission(oFindTabSets.UserName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjectType.WorkGroup, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Access);
                        }
                        if (@bool)
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

        public static SystemCustModel IncrementCounter(string sCounterName, IDBManager IDBManagerDefault, string NewCounter = "0")
        {
            SystemCustModel IncrementCounterRet = default;
            var bIsNull = default(bool);
            bool lBoolean;
            string counter = string.Empty;
            var lError = default(int);
            string sErrMsg = string.Empty;
            string sSQL;
            string sQuote = string.Empty;
            DataTable rstADO;
            try
            {
                IncrementCounterRet = new SystemCustModel();
                var SchemaColTable = GetSchemaInfo(IDBManagerDefault, "System", sCounterName);
                if (SchemaColTable is not null && SchemaColTable.Rows.Count > 0)
                {
                    if (IsAString(Convert.ToString(SchemaColTable.AsEnumerable().ElementAtOrDefault(0)["DATA_TYPE"])))
                    {
                        sQuote = "'";
                    }
                }
                sSQL = string.Format("SELECT [{0}] FROM System", sCounterName);
                string argsErrMsg = "";
                int arglError = 0;
                rstADO = GetADONETRecord(sSQL, IDBManagerDefault, sErrMsg: ref argsErrMsg, lError: ref arglError);
                if (rstADO.Rows.Count == 0)
                {
                    IncrementCounterRet.set_CounterValue(sCounterName, "1");
                }
                else if (string.IsNullOrEmpty(Convert.ToString(rstADO.AsEnumerable().ElementAtOrDefault(0)[sCounterName])))
                {
                    bIsNull = true;
                    IncrementCounterRet.set_CounterValue(sCounterName, "1");
                }
                else
                {
                    IncrementCounterRet.set_CounterValue(sCounterName, Convert.ToString(rstADO.AsEnumerable().ElementAtOrDefault(0)[sCounterName]));
                }
                counter = IncrementCounterRet.get_CounterValue(sCounterName);

                // Update the field to turn on edit mode (and hopefully lock the record!)
                if (Convert.ToDouble(counter) >= Convert.ToDouble(NewCounter))
                    NewCounter = (Convert.ToDouble(counter) + 1d).ToString();
                sSQL = string.Format("UPDATE [System] SET [{0}] = {1}{2}{1} WHERE ", sCounterName, sQuote, NewCounter);

                if (bIsNull)
                {
                    sSQL += string.Format("[{0}] IS NULL", sCounterName);
                }
                else
                {
                    sSQL += string.Format("[{0}] = {1}{2}{1}", sCounterName, sQuote, counter);
                }
                lBoolean = ProcessADONETRecord(sSQL, IDBManagerDefault, ref sErrMsg, ref lError);
                if (sErrMsg.ToLower().Contains("overflow"))
                {
                    throw new OverflowException(string.Format(Languages.get_Translation("msgImportCtrlTheValueIs2Lrg2FitIn2TheSystem"), NewCounter, sCounterName));
                    // Throw New OverflowException(String.Format("The value {0} is too large to fit into the System.{1} field.  Please contact your system administrator.", NewCounter, sCounterName))
                }
            }
            catch (Exception)
            {
                throw;
            }

            return IncrementCounterRet;

        }

        public static void WalkRelationshipsForAuditUpdates(string sId, ref SLAuditUpdate oSLAuditUpdates, Table oOrgTables, 
            Table oTable, int lLevel, DateTime dNow, List<Databas> data_iDatabas, List<RelationShip> data_iRelationship, List<Table> data_iTables, 
            IRepository<SLAuditUpdChildren> _iSLAuditUpdChildren, HttpContext httpContext, IDBManager? _IDBManagerDefault = null, IDBManager _IDBManager = null)
        {
            bool bHadADO = false;
            string sComma = string.Empty;
            string sSQL = string.Empty;
            RelationShip oRelationships = null;
            SLAuditUpdChildren oSLAuditUpdChildren = null;
            Table oUpTables = null;
            DataTable rsADO = null;
            string CachedId = "";
            var CachedDateTime = DateTime.FromOADate(0d);
            IDBManager commonDBManager = null;
            try
            {
                if (lLevel > 100)
                {
                    // A little too recursive???
                    return;
                }
                if (oTable is not null)
                {
                    if (!string.IsNullOrEmpty(oTable.DBName))
                    {
                        var odatabase = data_iDatabas.Where(m => m.DBName.Trim().ToLower().Equals(oTable.DBName.Trim().ToLower())).FirstOrDefault();
                        _IDBManager.ConnectionString = Keys.get_GetDBConnectionString(odatabase);
                        commonDBManager = _IDBManager;
                    }
                    else
                    {
                        commonDBManager = _IDBManagerDefault;
                    }
                }
                if (sId is null || sId.Trim().Length == 0)
                    return;
                bool IfIdFieldIsString = IdFieldIsString(commonDBManager, oTable.TableName, oTable.IdFieldName);
                if (!IfIdFieldIsString)
                {
                    if (!Information.IsNumeric(sId))
                        return;
                    sId = TrackingServices.StripLeadingZeros(sId);
                    if (string.IsNullOrEmpty(sId))
                        return;
                }

                // If (rsADOIn Is Nothing) Then
                var ParentTablesRelations = data_iRelationship.Where(m => m.LowerTableName.Trim().ToLower().Equals(oTable.TableName.Trim().ToLower()));
                if (ParentTablesRelations.Count() > 0)
                {
                    sComma = "";
                    sSQL = "";

                    foreach (var currentORelationships in ParentTablesRelations)
                    {
                        oRelationships = currentORelationships;
                        var AuditUpdateFlagOnBranch = data_iTables.Where(m => m.TableName.Trim().ToLower().Equals(oRelationships.UpperTableName.Trim().ToLower())).FirstOrDefault().AuditUpdate;
                        if ((bool)AuditUpdateFlagOnBranch)
                        {
                            if (Strings.StrComp(sId, CachedId, CompareMethod.Text) == 0 & CachedDateTime == dNow)
                            {
                                return;
                            }
                            else
                            {
                                sSQL = sSQL + sComma + "[" + DatabaseMap.RemoveTableNameFromField(oRelationships.LowerTableFieldName) + "]";
                                sComma = ", ";
                            }
                        }
                    }

                    if (Strings.Len(sSQL) > 0)
                    {
                        sSQL = string.Format("SELECT {0} FROM [{1}] WHERE [{2}] = '{3}'", sSQL, oTable.TableName, DatabaseMap.RemoveTableNameFromField(oTable.IdFieldName), IDQueryValue(sId, commonDBManager, oTable));
                        string argsErrMsg = "";
                        int arglError = 0;
                        rsADO = GetADONETRecord(sSQL, commonDBManager, sErrMsg: ref argsErrMsg, lError: ref arglError);
                    }
                }
                // Else
                // bHadADO = True
                // rsADO = rsADOIn
                // End If

                var SLAuditUpdChildrenObj = new SLAuditUpdChildren();
                oSLAuditUpdChildren = SLAuditUpdChildrenObj;

                {
                    ref var withBlock = ref oSLAuditUpdChildren;
                    withBlock.SLAuditUpdatesId = oSLAuditUpdates.Id;
                    withBlock.TableName = oTable.TableName;
                    withBlock.TableId = ZeroPaddedString(oTable, sId, _IDBManagerDefault);
                }
                if (oSLAuditUpdChildren is not null)
                {
                    _iSLAuditUpdChildren.Add(oSLAuditUpdChildren);
                    oSLAuditUpdChildren = oSLAuditUpdChildren;
                }

                if (rsADO is not null)
                {
                    if (rsADO.Rows.Count > 0)
                    {
                        // Dim ParentTablesRelations = data_iRelationship.Where(Function(m) m.LowerTableName.Trim.ToLower.Equals(oTable.TableName.Trim.ToLower))
                        foreach (var currentORelationships1 in ParentTablesRelations)
                        {
                            oRelationships = currentORelationships1;
                            var AuditUpdateFlagOnBranch = data_iTables.Where(m => m.TableName.Trim().ToLower().Equals(oRelationships.UpperTableName.Trim().ToLower())).FirstOrDefault().AuditUpdate;
                            if ((bool)AuditUpdateFlagOnBranch)
                            {
                                var sTable = data_iTables.Where(m => m.TableName.Trim().ToLower().Equals(oRelationships.UpperTableName.Trim().ToLower())).FirstOrDefault();
                                oUpTables = sTable;
                                if (oUpTables is not null)
                                {
                                    // Recursively Check upstream
                                    if (!(rsADO.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oRelationships.LowerTableFieldName)] is DBNull))
                                    {
                                        WalkRelationshipsForAuditUpdates(Convert.ToString(rsADO.AsEnumerable().ElementAtOrDefault(0)[DatabaseMap.RemoveTableNameFromField(oRelationships.LowerTableFieldName)]), 
                                            ref oSLAuditUpdates, oOrgTables, oUpTables, lLevel + 1, dNow, data_iDatabas, data_iRelationship, data_iTables,
                                            _iSLAuditUpdChildren,httpContext, _IDBManagerDefault, _IDBManager);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (_IDBManager.Connection is not null)
                {
                    _IDBManager.Close();
                }
            }
        }

        #endregion

        #region General
        // Contain default connection for DBManager created during code optimization
        public static string ZeroPaddedString(Table oTable, object oId, IDBManager IDBManagerDefault)
        {
            if (oId is null || oId.ToString().Length == 0)
            {
                return "";
            }
            else if (oTable is null)
            {
                return ZeroPaddedString(oId, IDBManagerDefault);
            }
            else
            {
                bool IfIdFieldIsString = IdFieldIsString(IDBManagerDefault, oTable.TableName, oTable.IdFieldName);
                if (!IfIdFieldIsString)
                {
                    return ZeroPaddedString(oId, IDBManagerDefault);
                }
                else
                {
                    return oId.ToString().Trim();
                }
            }
        }

        // Contain default connection for DBManager created during code optimization
        public static string ZeroPaddedString(Table oTable, object oId, IDBManager IDBManagerDefault, bool IfIdFieldIsString)
        {
            if (oId is null || oId.ToString().Length == 0)
            {
                return "";
            }
            else if (oTable is null)
            {
                return ZeroPaddedString(oId, IDBManagerDefault);
            }
            else if (!IfIdFieldIsString)
            {
                return ZeroPaddedString(oId, IDBManagerDefault);
            }
            else
            {
                return oId.ToString().Trim();
            }
        }

        // Contain default connection for DBManager created during code optimization
        public static string ZeroPaddedString(object oId, IDBManager IDBManagerDefault)
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
                    sId = TrackingServices.StripLeadingZeros(sId);
                    return sId.PadLeft(UserLinkIndexTableIdSize(IDBManagerDefault), '0');
                }
                else
                {
                    return sId.Trim();
                }
            }
        }

        // Check if id field is string or not using default connection 
        public static bool IdFieldIsString(IDBManager CommonIDBManager, string sTableName, string sIdFieldName)
        {
            try
            {
                if (CommonIDBManager.Connection.State == ConnectionState.Closed)
                {
                    CommonIDBManager.Open();
                }
                var oDatatable = GetInfoUsingADONET.GetSchemaInfo(CommonIDBManager, sTableName, DatabaseMap.RemoveTableNameFromField(sIdFieldName));
                if (oDatatable is not null)
                {
                    return IsAStringForSchema(Convert.ToString(oDatatable.AsEnumerable().ElementAtOrDefault(0)["DATA_TYPE"]));
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        // Use default connection to process
        public static string IDQueryValue(string sID, IDBManager CommonIDBManager, Table oTable)
        {
            try
            {
                if (CommonIDBManager.Connection.State == ConnectionState.Closed)
                {
                    CommonIDBManager.Open();
                }
                bool IfIdFieldIsString = IdFieldIsString(CommonIDBManager, oTable.TableName, oTable.IdFieldName);
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
            catch (Exception)
            {
                throw;
            }

        }

        // Use default connection to process
        public static string IDQueryValue(bool IfIdFieldIsString, string sID)
        {
            try
            {
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
            catch (Exception)
            {
                throw;
            }

        }
        #endregion

    }

    public class ClsFieldWithVal
    {
        private string m_colName;
        public string colName
        {
            get
            {
                return m_colName;
            }
            set
            {
                m_colName = value;
            }
        }

        private string m_colVal;
        public string colVal
        {
            get
            {
                return m_colVal;
            }
            set
            {
                m_colVal = value;
            }
        }

        private Type m_colType;
        public Type colType
        {
            get
            {
                return m_colType;
            }
            set
            {
                m_colType = value;
            }
        }

    }
}