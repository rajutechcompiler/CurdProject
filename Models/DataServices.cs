using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using ADODB;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using TabFusionRMS.Models;

namespace TabFusionRMS.WebCS
{
    public class DataServices
    {

        public static ADODB.Connection DBOpen(Table pTableEntity, IQueryable<Databas> oDatabase)
        {
            try
            {
                string DBConnString = "";
                string DbName;
                Databas pDatabaseEntity = null;
                string pDatabaseName = "";
                if (pTableEntity is not null)
                {
                    if (pTableEntity.DBName is not null)
                    {
                        pDatabaseEntity = oDatabase.Where(x => x.DBName.Trim().ToLower().Equals(pTableEntity.DBName.Trim().ToLower())).FirstOrDefault();
                    }
                }
                if (pDatabaseEntity is not null)
                {
                    DbName = Convert.ToString(pDatabaseEntity.DBName);
                    if (string.IsNullOrEmpty(DbName))
                    {
                        DBConnString = Keys.get_DefaultConnectionString(true);
                    }
                    else
                    {
                        DBConnString = GetConnectionString((Databas)pDatabaseEntity);
                    }
                }
                else
                {
                    DBConnString = Keys.get_DefaultConnectionString(true);
                }

                var Cnxn = new ADODB.Connection();
                Cnxn.Open(DBConnString);
                return Cnxn;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static ADODB.Connection DBOpen(Databas pDatabaseEntity)
        {
            try
            {
                string DBConnString = "";
                string DbName;
                if (pDatabaseEntity is not null)
                {
                    DbName = pDatabaseEntity.DBName;
                    if (string.IsNullOrEmpty(DbName))
                    {
                        DBConnString = Keys.get_DefaultConnectionString(true);
                    }
                    else
                    {
                        DBConnString = GetConnectionString(pDatabaseEntity);
                    }
                }
                else
                {
                    DBConnString = Keys.get_DefaultConnectionString(true);
                }

                var Cnxn = new ADODB.Connection();
                Cnxn.Open(DBConnString);
                return Cnxn;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static ADODB.Connection DBOpen(Enums.eConnectionType DBConType = Enums.eConnectionType.conDefault, Databas pDatabaseEntity = null)
        {
            try
            {
                string DBConnString;
                if (DBConType == Enums.eConnectionType.conDefault)
                {
                    DBConnString = Keys.get_DefaultConnectionString(true);
                }
                else
                {
                    DBConnString = GetConnectionString(pDatabaseEntity);
                }

                var Cnxn = new ADODB.Connection();
                Cnxn.Open(DBConnString);
                return Cnxn;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetConnectionString(Databas DBToOpen)
        {
            return GetConnectionString(DBToOpen, true);
        }

        public static string GetConnectionString(Databas DBToOpen, bool includeProvider)
        {
            string sConnect = string.Empty;
            if (includeProvider)
                sConnect = string.Format("Provider={0}; ", DBToOpen.DBProvider);

            sConnect += string.Format("Data Source={0}; Initial Catalog={1}; ", DBToOpen.DBServer, DBToOpen.DBDatabase);

            if (!string.IsNullOrEmpty(DBToOpen.DBUserId))
            {
                sConnect += string.Format("User Id={0}; Password={1};", DBToOpen.DBUserId, DBToOpen.DBPassword);
            }
            else
            {
                sConnect += "Persist Security Info=True;Integrated Security=SSPI;";
                // sConnect = Keys.DefaultConnectionString(True, DBToOpen.DBDatabase)
            }

            return sConnect;
        }

        public static bool ProcessADOCommand(ref string sSQL, ADODB.Connection Cnxn, bool bDoNoCount = false)
        {
            bool ProcessADOCommandRet = default;
            object oRecsAffected;
            ADODB.Recordset rsADO;
            ProcessADOCommandRet = Convert.ToBoolean(-1);

            try
            {
                if (Cnxn is not null)
                {
                    if (bDoNoCount)
                    {
                        sSQL = "SET NOCOUNT OFF;" + sSQL + ";SET NOCOUNT ON";
                    }

                    var cmADOCmd = new ADODB.Command();
                    cmADOCmd.CommandTimeout = 30;
                    cmADOCmd.ActiveConnection = Cnxn;
                    cmADOCmd.ActiveConnection.CursorLocation = ADODB.CursorLocationEnum.adUseClient;
                    cmADOCmd.CommandType = ADODB.CommandTypeEnum.adCmdText;
                    cmADOCmd.CommandText = sSQL;

                    oRecsAffected = new object();
                    rsADO = cmADOCmd.Execute(out oRecsAffected);
                    if ((int)oRecsAffected >= 0)
                    {
                        ProcessADOCommandRet = Convert.ToBoolean((int)oRecsAffected);
                    }

                    if (rsADO is not null)
                    {
                        if (rsADO.State != (int)Enums.ObjectStateEnum.rmStateClosed)
                        {
                            rsADO.Close();
                        }

                        rsADO = default;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsSysAdmin(string sTableName, Table pTableEntity = null, 
            IQueryable<Databas> lDatabaseEntities = null,
             Databas pDatabases = null)
        {
            bool IsSysAdminRet = default;
            bool bDBPassedIn;
            int lError = 0;
            string sErrMsg = "";
            var sADOConn = new ADODB.Connection();
            var rsADO = new ADODB.Recordset();
            if (Strings.Len(sTableName) == 0)
                return true;

            if (pDatabases is null)
            {
                pDatabases = GetDBObjForTable(pTableEntity, lDatabaseEntities);
            }
            else
            {
                bDBPassedIn = true;
            }
            sADOConn = DBOpen(pDatabases);

            if (sADOConn is not null)
            {
                // check moti (parameters)
                rsADO = DataServices.GetADORecordset("SELECT IS_SRVROLEMEMBER ('sysadmin')", pTableEntity, lDatabaseEntities,Enums.CursorTypeEnum.rmOpenKeyset ,Enums.LockTypeEnum.rmLockBatchOptimistic , Enums.CommandTypeEnum.rmCmdText, MaxRecords : 0 ,bAccessSpecialServerSide: false,bReturnError: false, lError: ref lError, sErrorMessage: ref sErrMsg);

                if (rsADO is not null)
                {
                    if (!rsADO.BOF & !rsADO.EOF)
                        IsSysAdminRet = (Int32)rsADO.Fields[0].Value != 0;
                    rsADO.Close();
                    rsADO = null;
                }

                if (IsSysAdminRet)
                    return true;

                int arglError1 = 0;
                string argsErrorMessage1 = "";
                // check moti (parametes)
                rsADO = DataServices.GetADORecordset("SELECT IS_MEMBER ('db_owner')", pTableEntity, lDatabaseEntities, bAccessSpecialServerSide: false, bReturnError: false, lError: ref arglError1, sErrorMessage: ref argsErrorMessage1, bCheckIfDisconnected: false);

                if (rsADO is not null)
                {
                    if (!rsADO.BOF & !rsADO.EOF)
                        IsSysAdminRet = (Int32)rsADO.Fields[0].Value != 0;
                    rsADO.Close();
                    rsADO = null;
                }
            }

            return IsSysAdminRet;
        }

        public static Databas GetDBObjForTable(Table oTable, IQueryable<Databas> lDatabaseEntities, bool bCheckIfDisconnected = false)
        {
            try
            {
                var oDatabase = new Databas();
                if (oTable is not null)
                {
                    if (!string.IsNullOrEmpty(oTable.DBName))
                    {
                        oDatabase = lDatabaseEntities.Where(m => m.DBName.Trim().ToLower().Equals(oTable.DBName.Trim().ToLower())).FirstOrDefault();
                    }
                    else
                    {
                        var dbEngine = new DatabaseEngine();
                        oDatabase.DBName = dbEngine.DBName;
                        oDatabase.DBType = dbEngine.DBType;
                        oDatabase.DBConnectionText = dbEngine.DBConnectionText;
                        oDatabase.DBConnectionTimeout = dbEngine.DBConnectionTimeout;
                        oDatabase.DBDatabase = dbEngine.DBDatabase;
                        oDatabase.DBPassword = dbEngine.DBPassword;
                        oDatabase.DBProvider = dbEngine.DBProvider;
                        oDatabase.DBServer = dbEngine.DBServer;
                        oDatabase.DBUseDBEngineUIDPWD = dbEngine.DBUseDBEngineUIDPWD;
                        oDatabase.DBUserId = dbEngine.DBUserId;
                    }
                }
                else
                {
                    var dbEngine = new DatabaseEngine();
                    oDatabase.DBName = dbEngine.DBName;
                    oDatabase.DBType = dbEngine.DBType;
                    oDatabase.DBConnectionText = dbEngine.DBConnectionText;
                    oDatabase.DBConnectionTimeout = dbEngine.DBConnectionTimeout;
                    oDatabase.DBDatabase = dbEngine.DBDatabase;
                    oDatabase.DBPassword = dbEngine.DBPassword;
                    oDatabase.DBProvider = dbEngine.DBProvider;
                    oDatabase.DBServer = dbEngine.DBServer;
                    oDatabase.DBUseDBEngineUIDPWD = dbEngine.DBUseDBEngineUIDPWD;
                    oDatabase.DBUserId = dbEngine.DBUserId;
                }
                return oDatabase;
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }


        // Public Shared Function GetDBObjForTable(ByVal pTableEntity As Table, lDatabaseEntities As IQueryable(Of Databas), Optional ByVal bCheckIfDisconnected As Boolean = False) As Databas
        // Dim pDatabaseEntity = New Databas

        // If Not (pTableEntity Is Nothing) Then
        // If Not (lDatabaseEntities Is Nothing) Then
        // If pTableEntity.DBName Is Nothing Then
        // pDatabaseEntity = Nothing
        // Else
        // pDatabaseEntity = lDatabaseEntities.Where(Function(x) x.DBName.Trim().ToLower().Equals(pTableEntity.DBName.Trim().ToLower())).FirstOrDefault()
        // End If

        // End If
        // Else
        // pDatabaseEntity = Nothing
        // End If

        // Return pDatabaseEntity
        // End Function

        // Public Shared Function RefineTrackableValue(moTable As Table, SecureObject As IQueryable(Of SecureObject), SecureObjectPermission As IQueryable(Of SecureObjectPermission)) As Boolean
        // Try
        // Dim SecureTableObject As SecureObject
        // Dim boolVal = False
        // If (Not moTable Is Nothing) Then
        // If ((Not (SecureObject Is Nothing)) And (Not (String.IsNullOrEmpty(moTable.TableName)))) Then
        // SecureTableObject = SecureObject.Where(Function(m) m.Name.Trim.ToLower.Equals(moTable.TableName.Trim.ToLower) And m.SecureObjectTypeID = Enums.SecureObjectType.Table).FirstOrDefault
        // If ((Not SecureObjectPermission Is Nothing) And (Not SecureTableObject Is Nothing)) Then
        // Dim TrackableEntity = SecureObjectPermission.Where(Function(m) m.SecureObjectID = SecureTableObject.SecureObjectID And m.PermissionID = Enums.PassportPermissions.Transfer).FirstOrDefault
        // If (Not TrackableEntity Is Nothing) Then
        // Return True
        // Exit Function
        // End If
        // End If
        // End If
        // End If
        // Return boolVal
        // Catch ex As Exception
        // Throw ex.InnerException
        // Return False
        // End Try
        // End Function

        public static ADODB.Recordset GetADORecordset(string sSQL, Table pTableEntity,
            IQueryable<Databas> lDatabaseEntities,
            [Optional, DefaultParameterValue(Enums.CursorTypeEnum.rmOpenKeyset)] Enums.CursorTypeEnum eCursorType ,
           [Optional, DefaultParameterValue(Enums.LockTypeEnum.rmLockBatchOptimistic)] Enums.LockTypeEnum eLockType  ,
            [Optional, DefaultParameterValue( Enums.CommandTypeEnum.rmCmdText)] Enums.CommandTypeEnum eCommandType ,
            [Optional, DefaultParameterValue(0)] int MaxRecords,
            [Optional, DefaultParameterValue(true)] bool bAccessSpecialServerSide,
           [Optional, DefaultParameterValue(false)] bool bReturnError ,
            [Optional, DefaultParameterValue(0)] ref int lError,
            [Optional, DefaultParameterValue("")] ref string sErrorMessage,
           [Optional, DefaultParameterValue(false)] bool bCheckIfDisconnected )
        {
            Databas oDatabase;
            Table oTables;
            var rsADO = new ADODB.Recordset();
            // Dim cmADOCmd = New ADODB.Command
            ADODB.Connection Cnxn;

            try
            {
                // sTableName = Trim(sTableName)
                oDatabase = GetDBObjForTable(pTableEntity, lDatabaseEntities, bCheckIfDisconnected);
                Cnxn = DBOpen(oDatabase);

                if (oDatabase is not null)
                {
                    oTables = pTableEntity;

                    {
                        //ref var withBlock = ref rsADO;
                        if (oTables is not null)
                        {
                            Cnxn.CommandTimeout = (int)oTables.ADOQueryTimeout;
                        }
                        else
                        {
                            Cnxn.CommandTimeout = 30;
                        }

                        rsADO.ActiveConnection = Cnxn;
                        rsADO.CursorType = (ADODB.CursorTypeEnum)eCursorType;
                        rsADO.MaxRecords = MaxRecords;

                        if (oTables is not null)
                        {
                            if (oTables.ADOCacheSize > 0)
                            {
                                rsADO.CacheSize = (int)oTables.ADOCacheSize;
                            }
                        }
                        else
                        {
                            rsADO.CacheSize = 1;
                        }

                        if (eCursorType == Enums.CursorTypeEnum.rmOpenForwardOnly)
                        {
                            // If we set Forward Only we are loading the data so get it the quickest way possible with the least overhead
                            rsADO.LockType = (ADODB.LockTypeEnum)Enums.LockTypeEnum.rmLockReadOnly;
                            Cnxn.CursorLocation = (ADODB.CursorLocationEnum)Enums.CursorLocationEnum.rmUseServer;
                            rsADO.CursorLocation = (ADODB.CursorLocationEnum)Enums.CursorLocationEnum.rmUseServer;
                            rsADO.ActiveConnection = Cnxn;
                        }
                        else
                        {
                            rsADO.LockType = (ADODB.LockTypeEnum)eLockType;
                            // Is Access?
                            if (oTables is not null)
                            {
                                if ((bool)oTables.ADOServerCursor)
                                {
                                    Cnxn.CursorLocation = (ADODB.CursorLocationEnum)Enums.CursorLocationEnum.rmUseServer;
                                    rsADO.CursorLocation = (ADODB.CursorLocationEnum)Enums.CursorLocationEnum.rmUseServer;
                                }
                                else
                                {
                                    Cnxn.CursorLocation = (ADODB.CursorLocationEnum)Enums.CursorLocationEnum.rmUseClient;
                                    rsADO.CursorLocation = (ADODB.CursorLocationEnum)Enums.CursorLocationEnum.rmUseClient;
                                }
                            }
                            else
                            {
                                Cnxn.CursorLocation = (ADODB.CursorLocationEnum)Enums.CursorLocationEnum.rmUseClient;
                                rsADO.CursorLocation = (ADODB.CursorLocationEnum)Enums.CursorLocationEnum.rmUseClient;
                            }
                            if ((int)rsADO.CursorLocation == (int)Enums.CursorLocationEnum.rmUseServer)
                            {
                                if (Cnxn is null)
                                {
                                    Cnxn = new ADODB.Connection();
                                }

                                if (Cnxn.State == (int)Enums.ObjectStateEnum.rmStateClosed)
                                {
                                    var db = GetConnectionString(oDatabase);
                                    Cnxn.ConnectionString = db;
                                    Cnxn.Open();
                                }

                                Cnxn.CursorLocation = Cnxn.CursorLocation;
                                rsADO.ActiveConnection = Cnxn;
                            }
                            else
                            {
                                rsADO.ActiveConnection = Cnxn;
                            }
                        }

                        rsADO.Open(sSQL,Options: (int)eCommandType);
                        // This code forces deletes to only update the table we care about in multi-table joined record sets
                        if (oTables is not null)
                        {
                            if (Strings.Len(oTables.TableName) > 0)
                            {
                                rsADO.Properties["Unique Table"].Value = oTables.TableName;
                            }
                        }
                    }
                }

                if (rsADO is null)
                {
                    return null;
                }
                else if (rsADO.State != (int)Enums.ObjectStateEnum.rmStateClosed)
                {
                    return rsADO;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                lError = ex.HResult;
                sErrorMessage = Information.Err().Description;

                if (rsADO.State == (int)Enums.ObjectStateEnum.rmStateOpen)
                {
                    rsADO.Close();
                }

                rsADO = null;
                return null;
            }
        }

        public static bool IsAStringType(Enums.DataTypeEnum eDataType)
        {
            switch (eDataType)
            {
                case Enums.DataTypeEnum.rmBSTR:
                case Enums.DataTypeEnum.rmChar:
                case Enums.DataTypeEnum.rmVarChar:
                case Enums.DataTypeEnum.rmLongVarChar:
                case Enums.DataTypeEnum.rmWChar:
                case Enums.DataTypeEnum.rmVarWChar:
                case Enums.DataTypeEnum.rmLongVarWChar:
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        public static bool IsADateType(Enums.DataTypeEnum eDataType)
        {
            switch (eDataType)
            {
                case Enums.DataTypeEnum.rmDate:
                case Enums.DataTypeEnum.rmDBDate:
                case Enums.DataTypeEnum.rmDBTimeStamp:
                case Enums.DataTypeEnum.rmDBTime:
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        public static bool IsANumericType(Enums.DataTypeEnum eDataType)
        {
            switch (eDataType)
            {
                case Enums.DataTypeEnum.rmTinyInt:
                case Enums.DataTypeEnum.rmUnsignedTinyInt:
                case Enums.DataTypeEnum.rmSmallInt:
                case Enums.DataTypeEnum.rmUnsignedSmallInt:
                case Enums.DataTypeEnum.rmInteger:
                case Enums.DataTypeEnum.rmBigInt:
                case Enums.DataTypeEnum.rmUnsignedInt:
                case Enums.DataTypeEnum.rmUnsignedBigInt:
                case Enums.DataTypeEnum.rmSingle:
                case Enums.DataTypeEnum.rmDouble:
                case Enums.DataTypeEnum.rmDecimal:
                case Enums.DataTypeEnum.rmNumeric:
                case Enums.DataTypeEnum.rmVarNumeric:
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        public static ADODB.Recordset GetADORecordSet(ref string sSql, ADODB.Connection Cnxn, [Optional, DefaultParameterValue("")] ref string lError)
        {
            try
            {
                var recordSet = new ADODB.Recordset();
                object oRecsAffected;
                if (Cnxn is not null)
                {
                    var cmADOCmd = new ADODB.Command();
                    cmADOCmd.CommandTimeout = 30;
                    cmADOCmd.ActiveConnection = Cnxn;
                    cmADOCmd.ActiveConnection.CursorLocation = ADODB.CursorLocationEnum.adUseClient;
                    cmADOCmd.CommandType = ADODB.CommandTypeEnum.adCmdText;
                    cmADOCmd.CommandText = sSql;
                    oRecsAffected = new object();
                    recordSet = cmADOCmd.Execute(out oRecsAffected);
                    // recordSet.Open(sSql, Cnxn)
                    if (recordSet is null)
                    {
                        return default;
                    }
                    else if (recordSet.State != Convert.ToInt32(Enums.ObjectStateEnum.rmStateClosed))
                    {
                        return recordSet;
                    }
                    else
                    {
                        return default;
                    }
                }
                return recordSet;
            }
            catch (Exception ex)
            {
                lError = ex.Message;
                sSql = lError;
                return default;
                // Throw ex.InnerException
            }

        }


        public static bool IsSysAdmin(string tableName, ADODB.Connection Cnxn, bool bDBOwnerOK = true)
        {
            bool IsSysAdminRet = default;
            ADODB.Recordset rsADO;
            if (Strings.Len(tableName) == 0)
            {
                return IsSysAdminRet == true;
            }
            if (Cnxn is not null)
            {
                string argsSql = "SELECT IS_SRVROLEMEMBER ('sysadmin')";
                string arglError = "";
                rsADO = GetADORecordSet(ref argsSql, Cnxn, lError: ref arglError);
                if (rsADO is not null)
                {
                    if (!rsADO.BOF & !rsADO.EOF)
                    {
                        IsSysAdminRet = Convert.ToInt32(rsADO.Fields[0].Value) != 0;
                        rsADO.Close();
                        rsADO = null;
                    }
                }
                if (IsSysAdminRet | !bDBOwnerOK)
                {
                    string argsSql1 = "SELECT IS_MEMBER ('db_owner')";
                    string arglError1 = "";
                    rsADO = GetADORecordSet(ref argsSql1, Cnxn, lError: ref arglError1);
                    if (rsADO is not null)
                    {
                        if (!rsADO.BOF & !rsADO.EOF)
                        {
                            IsSysAdminRet = Convert.ToInt32(rsADO.Fields[0].Value)!= 0;
                            rsADO.Close();
                            rsADO = null;
                        }
                    }
                }
            }
            return IsSysAdminRet;
        }

        public static object IsContainField(ADODB.Connection sADOConn, string tableName, List<SchemaColumns> schemaColumnList, string fieldName, List<KeyValuePair<string, string>> DDList)
        {
            var bHasAField = default(bool);
            bool bIsSystemAdmin;
            foreach (SchemaColumns schemaColumnObj in schemaColumnList)
            {
                if (schemaColumnObj.ColumnName.Trim().ToLower().Equals(fieldName.Trim().ToLower()))
                {
                    bHasAField = true;
                    break;
                }
            }
            bIsSystemAdmin = IsSysAdmin(tableName, sADOConn);
            if (!bHasAField & bIsSystemAdmin)
            {
                DDList.Add(new KeyValuePair<string, string>(fieldName, fieldName));
            }
            foreach (SchemaColumns schemaColumnObj in schemaColumnList)
            {
                if (!SchemaInfoDetails.IsSystemField(schemaColumnObj.ColumnName))
                {
                    switch (schemaColumnObj.DataType)
                    {
                        case Enums.DataTypeEnum.rmBoolean:
                        case Enums.DataTypeEnum.rmSmallInt:
                        case Enums.DataTypeEnum.rmUnsignedSmallInt:
                        case Enums.DataTypeEnum.rmTinyInt:
                        case Enums.DataTypeEnum.rmUnsignedTinyInt:
                            {
                                DDList.Add(new KeyValuePair<string, string>(schemaColumnObj.ColumnName, schemaColumnObj.ColumnName));
                                break;
                            }

                        default:
                            {
                                break;
                            }

                    }
                }
            }
            return DDList;
        }

        public static object IsContainStringField(ADODB.Connection sADOConn, string tableName, List<SchemaColumns> schemaColumnList, string fieldName, List<KeyValuePair<string, string>> DDList)
        {
            var bHasAField = default(bool);
            bool eIsSystemAdmin;
            foreach (SchemaColumns schemaColumnObj in schemaColumnList)
            {
                if (schemaColumnObj.ColumnName.Trim().ToLower().Equals(fieldName.Trim().ToLower()))
                {
                    bHasAField = true;
                    break;
                }
            }
            eIsSystemAdmin = IsSysAdmin(tableName, sADOConn);
            if (!bHasAField & eIsSystemAdmin)
            {
                DDList.Add(new KeyValuePair<string, string>(fieldName, fieldName));
                bHasAField = false;
            }

            foreach (SchemaColumns oSchemaColumnObj in schemaColumnList)
            {
                if (!SchemaInfoDetails.IsSystemField(oSchemaColumnObj.ColumnName) & oSchemaColumnObj.IsString)
                {
                    DDList.Add(new KeyValuePair<string, string>(oSchemaColumnObj.ColumnName, oSchemaColumnObj.ColumnName));
                }
            }
            return DDList;
        }

        public static bool RemoveTrackingStatusField(ADODB.Connection sADOConn, string tableName, string fieldName)
        {
            try
            {
                var schemaIndexList = SchemaIndex.GetTableIndexes(tableName, sADOConn);
                var schemaColumnList = SchemaInfoDetails.GetSchemaInfo(tableName, sADOConn);
                bool boolProcessSQL = false;
                foreach (SchemaIndex schemaIndexObj in schemaIndexList)
                {
                    if (schemaIndexObj.IndexName.Trim().ToLower().Equals(fieldName.Trim().ToLower()))
                    {

                        string indexDropSQL = "Drop Index [" + tableName + "]." + schemaIndexObj.IndexName;
                        boolProcessSQL = ProcessADOCommand(ref indexDropSQL, sADOConn, false);
                        break;
                    }
                }
                foreach (SchemaColumns schemaInfoObj in schemaColumnList)
                {
                    if (schemaInfoObj.ColumnName.Trim().ToLower().Equals(fieldName.Trim().ToLower()))
                    {

                        string columnDropSQL = "ALTER TABLE " + tableName + " DROP COLUMN " + fieldName;
                        boolProcessSQL = ProcessADOCommand(ref columnDropSQL, sADOConn, false);
                        break;
                    }
                }
                return boolProcessSQL;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IdFieldIsString(ADODB.Connection sADOConn, string tablename, string idfield)
        {
            try
            {
                var schemacolumnlist = SchemaInfoDetails.GetSchemaInfo(tablename, sADOConn, DatabaseMap.RemoveTableNameFromField(idfield));
                if (schemacolumnlist is not null)
                {
                    return schemacolumnlist[0].IsString;
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

        public static string IDQueryValue(bool IfIdFieldIsString, string sID)
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

        public static int ProcessADOCommand(ref string sSQL, ADODB.Connection Cnxn, [Optional, DefaultParameterValue(false)] bool bDoNoCount  , [Optional, DefaultParameterValue(-1)] ref int lError, [Optional, DefaultParameterValue("")] ref string sErrorMsg)
        {
            int ProcessADOCommandRet = default;
            object oRecsAffected = -1;
            ADODB.Recordset rsADO;
            try
            {
                ProcessADOCommandRet = -1;
                if (Cnxn is not null)
                {
                    if (bDoNoCount)
                    {
                        sSQL = "SET NOCOUNT OFF;" + sSQL + ";SET NOCOUNT ON";
                    }

                    var cmADOCmd = new ADODB.Command();
                    cmADOCmd.CommandTimeout = 30;
                    cmADOCmd.ActiveConnection = Cnxn;
                    cmADOCmd.ActiveConnection.CursorLocation = ADODB.CursorLocationEnum.adUseClient;
                    cmADOCmd.CommandType = ADODB.CommandTypeEnum.adCmdText;
                    cmADOCmd.CommandText = sSQL;

                    oRecsAffected = new object();
                    rsADO = cmADOCmd.Execute(out oRecsAffected);
                    if ((int)oRecsAffected >= 0)
                    {
                        ProcessADOCommandRet = (int)oRecsAffected;
                    }

                    if (rsADO is not null)
                    {
                        if (rsADO.State != (int)Enums.ObjectStateEnum.rmStateClosed)
                        {
                            rsADO.Close();
                        }

                        rsADO = default;
                    }
                }
                return ProcessADOCommandRet;
            } 
            catch(Exception)
            {
                if (lError != -1)
                {
                    lError = Information.Err().Number;
                    sErrorMsg = Information.Err().Description;
                }

                //if (rsADO is not null)
                //{
                //    if (rsADO.State != (int)Enums.ObjectStateEnum.rmStateClosed)
                //    {
                //        rsADO.Close();
                //    }
                //    rsADO = default;
                //}

                return -1;
            }
        }


        public static string InjectWhereIntoSQL(string sSQL, string sNewWhere, string sOperator = "AND")
        {
            string InjectWhereIntoSQLRet = default;
            string sInitWhere = string.Empty;
            string sInitOrderBy = string.Empty;
            string sInitSelect = string.Empty;
            string sRetVal = sSQL;
            int iPos;
            ;

            sSQL = NormalizeString(sSQL);
            iPos = Strings.InStr(sSQL, " WHERE ", CompareMethod.Text);

            if (iPos > 0)
            {
                sInitSelect = Strings.Trim(Strings.Left(sSQL, iPos));
                sInitWhere = Strings.Trim(Strings.Mid(sSQL, iPos + Strings.Len(" WHERE "), Strings.Len(sSQL)));

                iPos = Strings.InStr(sInitWhere, " ORDER BY ", CompareMethod.Text);
                if (iPos > 0)
                {
                    sInitOrderBy = Strings.Trim(Strings.Mid(sInitWhere, iPos + Strings.Len(" ORDER BY "), Strings.Len(sInitWhere)));
                    sInitWhere = Strings.Trim(Strings.Left(sInitWhere, iPos));
                }
            }
            else
            {
                iPos = Strings.InStr(sSQL, " ORDER BY ", CompareMethod.Text);
                if (iPos > 0)
                {
                    sInitOrderBy = Strings.Trim(Strings.Mid(sSQL, iPos + Strings.Len(" ORDER BY "), Strings.Len(sSQL)));
                    sInitSelect = Strings.Trim(Strings.Left(sSQL, iPos));
                }
                else
                {
                    sInitSelect = Strings.Trim(sSQL);
                }
            }

            sRetVal = sInitSelect;

            if (!string.IsNullOrEmpty(sInitWhere))
            {
                if (!string.IsNullOrEmpty(Strings.Trim(sNewWhere)))
                {
                    sRetVal += " WHERE (" + ParenEncloseStatement(sInitWhere) + " " + sOperator + " " + ParenEncloseStatement(sNewWhere) + ")";
                }
                else
                {
                    sRetVal += " WHERE " + ParenEncloseStatement(sInitWhere);
                }
            }
            else if (!string.IsNullOrEmpty(Strings.Trim(sNewWhere)))
            {
                sRetVal += " WHERE " + ParenEncloseStatement(sNewWhere);
            }

            if (!string.IsNullOrEmpty(sInitOrderBy))
                sRetVal += " ORDER BY " + sInitOrderBy;
            InjectWhereIntoSQLRet = sRetVal;
            return InjectWhereIntoSQLRet;

        lbl_InjectWhereIntoSQL:
            ;

            if (Interaction.MsgBox("Error " + Strings.Format((object)Information.Err().Number) + " " + Strings.Left(Information.Err().Description, Strings.Len(Strings.Trim(Information.Err().Description)) - 1) + Strings.Replace(Information.Err().Description, ".", "", Strings.Len(Strings.Trim(Information.Err().Description)), 1) + ".  (GUIUtils.InjectWhereIntoSQL)", Constants.vbRetryCancel | Constants.vbExclamation) == Constants.vbRetry)
            {
               // resume in vb
            }
            else
            {
                //resume next in vb
            }
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

        //internal static Recordset? GetADORecordset(string sSql, object value1, IQueryable<Databas> iDatabaseQuery, object value2, object value3, object value4, object value5, bool v1, bool v2, ref int lError, ref string sErrorMessage, bool bCheckIfDisconnected)
        //{
        //    throw new NotImplementedException();
        //}


        public static string ParenEncloseStatement(string sSQL)
        {
            string ParenEncloseStatementRet = default;
            long iParenCount;
            long iMaxParenCount;
            int iIndex;
            bool bDoEnclose;
            bool bInString;
            string sCurChar;
            ;

            bDoEnclose = false;
            bInString = false;
            iIndex = 1;
            iParenCount = 0L;
            iMaxParenCount = 0L;
            while (iIndex <= Strings.Len(sSQL) & !bDoEnclose)
            {
                sCurChar = Strings.Mid(sSQL, iIndex, 1);
                if (sCurChar == "\"")
                {
                    bInString = !bInString;
                }

                if (!bInString)
                {
                    if (sCurChar == "(")
                    {
                        iParenCount = iParenCount + 1L;
                    }

                    if (sCurChar == ")")
                    {
                        iParenCount = iParenCount - 1L;
                    }
                }

                if (iParenCount > iMaxParenCount)
                {
                    iMaxParenCount = iParenCount;
                }

                if (iParenCount == 0L & iIndex > 1 & iIndex < Strings.Len(sSQL) & iMaxParenCount > 0L)
                {
                    bDoEnclose = true;
                }

                iIndex = iIndex + 1;
            }

            if (iMaxParenCount == 0L)
            {
                bDoEnclose = true;
            }

            if (bDoEnclose)
            {
                ParenEncloseStatementRet = "(" + sSQL + ")";
            }
            else
            {
                ParenEncloseStatementRet = sSQL;
            }

            return ParenEncloseStatementRet;

        lbl_ParenEncloseStatement:
            ;

            if (Interaction.MsgBox("Error " + Strings.Format((object)Information.Err().Number) + " " + Strings.Left(Information.Err().Description, Strings.Len(Strings.Trim(Information.Err().Description)) - 1) + Strings.Replace(Information.Err().Description, ".", "", Strings.Len(Strings.Trim(Information.Err().Description)), 1) + ".  (GUIUtils.ParenEncloseStatement)", Constants.vbRetryCancel | Constants.vbExclamation) == Constants.vbRetry)
            {
                //   Resume
            }
            else
            {
                //Resume Next
            }
        }

    }

}