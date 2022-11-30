using System.Collections.Generic;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace TabFusionRMS.WebCS
{
    // Imports System.Data.SqlClient

    public class SchemaColumns
    {
        protected string msTableCatalog;
        protected string msTableSchema;
        protected string msTableName;
        protected string msColumnName;
        protected string msColumnGuid;
        protected string msColumnPropid;
        protected string msOrdinalPosition;
        protected string msColumnHasDefault;
        protected string msColumnDefault;
        protected string msColumnFlag;
        protected string msIsNullable;
        protected Enums.DataTypeEnum meDataType;
        protected string msTypeGuid;
        protected int miCharacterMaxLength;
        protected string msCharacterOctetLength;
        protected string msNumericPrecision;
        protected string msNumericScale;
        protected string msDataTimePrecision;
        protected string msCharacterSetCatalog;
        protected string msCharacterSetSchema;
        protected string msCharacterSetName;
        protected string msCollationCatalog;
        protected string msCollationSchema;
        protected string msCollationName;
        protected string msDomainCatalog;
        protected string msDomainSchema;
        protected string msDomainName;
        protected string msDescription;
        protected string msOrdinal;

        public string TableCatalog
        {
            get
            {
                return msTableCatalog;
            }
            set
            {
                msTableCatalog = Strings.Trim(value);
            }
        }

        public string TableSchema
        {
            get
            {
                return msTableSchema;
            }
            set
            {
                msTableSchema = Strings.Trim(value);
            }
        }

        public string TableName
        {
            get
            {
                return msTableName;
            }
            set
            {
                msTableName = Strings.Trim(value);
            }
        }

        public string ColumnName
        {
            get
            {
                return msColumnName;
            }
            set
            {
                msColumnName = Strings.Trim(value);
            }
        }

        public string ColumnGuid
        {
            get
            {
                return msColumnGuid;
            }
            set
            {
                msColumnGuid = Strings.Trim(value);
            }
        }

        public string ColumnPropid
        {
            get
            {
                return msColumnPropid;
            }
            set
            {
                msColumnPropid = Strings.Trim(value);
            }
        }

        public string OrdinalPosition
        {
            get
            {
                return msOrdinalPosition;
            }
            set
            {
                msOrdinalPosition = Strings.Trim(value);
            }
        }

        public string ColumnHasDefault
        {
            get
            {
                return msColumnHasDefault;
            }
            set
            {
                msColumnHasDefault = Strings.Trim(value);
            }
        }

        public string ColumnDefault
        {
            get
            {
                return msColumnDefault;
            }
            set
            {
                msColumnDefault = Strings.Trim(value);
            }
        }

        public string ColumnFlag
        {
            get
            {
                return msColumnFlag;
            }
            set
            {
                msColumnFlag = Strings.Trim(value);
            }
        }

        public string IsNullable
        {
            get
            {
                return msIsNullable;
            }
            set
            {
                msIsNullable = Strings.Trim(value);
            }
        }

        public Enums.DataTypeEnum DataType
        {
            get
            {
                return meDataType;
            }
            set
            {
                meDataType = value;
            }
        }

        public string TypeGuid
        {
            get
            {
                return msTypeGuid;
            }
            set
            {
                msTypeGuid = Strings.Trim(value);
            }
        }

        public int CharacterMaxLength
        {
            get
            {
                return miCharacterMaxLength;
            }
            set
            {
                miCharacterMaxLength = value;
            }
        }

        public string CharacterOctetLength
        {
            get
            {
                return msCharacterOctetLength;
            }
            set
            {
                msCharacterOctetLength = Strings.Trim(value);
            }
        }

        public string NumericPrecision
        {
            get
            {
                return msNumericPrecision;
            }
            set
            {
                msNumericPrecision = Strings.Trim(value);
            }
        }

        public string NumericScale
        {
            get
            {
                return msNumericScale;
            }
            set
            {
                msNumericScale = Strings.Trim(value);
            }
        }

        public string DataTimePrecision
        {
            get
            {
                return msDataTimePrecision;
            }
            set
            {
                msDataTimePrecision = Strings.Trim(value);
            }
        }

        public string CharacterSetCatalog
        {
            get
            {
                return msCharacterSetCatalog;
            }
            set
            {
                msCharacterSetCatalog = Strings.Trim(value);
            }
        }

        public string CharacterSetSchema
        {
            get
            {
                return msCharacterSetSchema;
            }
            set
            {
                msCharacterSetSchema = Strings.Trim(value);
            }
        }

        public string CharacterSetName
        {
            get
            {
                return msCharacterSetName;
            }
            set
            {
                msCharacterSetName = Strings.Trim(value);
            }
        }

        public string CollationCatalog
        {
            get
            {
                return msCollationCatalog;
            }
            set
            {
                msCollationCatalog = Strings.Trim(value);
            }
        }

        public string CollationSchema
        {
            get
            {
                return msCollationSchema;
            }
            set
            {
                msCollationSchema = Strings.Trim(value);
            }
        }

        public string CollationName
        {
            get
            {
                return msCollationName;
            }
            set
            {
                msCollationName = Strings.Trim(value);
            }
        }

        public string DomainCatalog
        {
            get
            {
                return msDomainCatalog;
            }
            set
            {
                msDomainCatalog = Strings.Trim(value);
            }
        }

        public string DomainSchema
        {
            get
            {
                return msDomainSchema;
            }
            set
            {
                msDomainSchema = Strings.Trim(value);
            }
        }

        public string DomainName
        {
            get
            {
                return msDomainName;
            }
            set
            {
                msDomainName = Strings.Trim(value);
            }
        }

        public string Description
        {
            get
            {
                return msDescription;
            }
            set
            {
                msDescription = Strings.Trim(value);
            }
        }

        public string Ordinal
        {
            get
            {
                return msOrdinal;
            }
            set
            {
                msOrdinal = Strings.Trim(value);
            }
        }

        public bool IsString
        {
            get
            {
                return SchemaInfoDetails.IsAStringType(meDataType);
            }
        }

        public bool IsADate
        {
            get
            {
                return SchemaInfoDetails.IsADateType(meDataType);
            }
        }

    }

    public sealed class SchemaTable
    {

        private string msTableCatalog;
        public string TableCatalog
        {
            get
            {
                return msTableCatalog;
            }
            set
            {
                msTableCatalog = Strings.Trim(value);
            }
        }

        private string msTableSchema;
        public string TableSchema
        {
            get
            {
                return msTableSchema;
            }
            set
            {
                msTableSchema = Strings.Trim(value);
            }
        }

        private string msTableName;
        public string TableName
        {
            get
            {
                return msTableName;
            }
            set
            {
                msTableName = Strings.Trim(value);
            }
        }

        private string msTableType;
        public string TableType
        {
            get
            {
                return msTableType;
            }
            set
            {
                msTableType = Strings.Trim(value);
            }
        }

        private string msTableGuid;
        public string TableGuid
        {
            get
            {
                return msTableGuid;
            }
            set
            {
                msTableGuid = Strings.Trim(value);
            }
        }

        private string msDescription;
        public string Description
        {
            get
            {
                return msDescription;
            }
            set
            {
                msDescription = Strings.Trim(value);
            }
        }

        private string msTablePropId;
        public string TablePropId
        {
            get
            {
                return msTablePropId;
            }
            set
            {
                msTablePropId = Strings.Trim(value);
            }
        }

        private string msDateCreated;
        public string DateCreated
        {
            get
            {
                return msDateCreated;
            }
            set
            {
                msDateCreated = Strings.Trim(value);
            }
        }

        private string msDateModified;
        public string DateModified
        {
            get
            {
                return msDateModified;
            }
            set
            {
                msDateModified = Strings.Trim(value);
            }
        }

        public static object GetSchemaTable(ADODB.Connection Cnxn, Enums.geTableType eTableType, string sTableName = "")
        {
            ADODB.Recordset rsADO;
            var schemaList = new List<SchemaTable>();
            if (!string.IsNullOrEmpty(Strings.Trim(sTableName)))
            {
                rsADO = Cnxn.OpenSchema((ADODB.SchemaEnum)Enums.SchemaEnum.rmSchemaTables, new object[] { null, null, sTableName, Enums.CvtTableTypeToString(ref eTableType) });
            }
            else
            {
                rsADO = Cnxn.OpenSchema((ADODB.SchemaEnum)Enums.SchemaEnum.rmSchemaTables, new object[] { null, null, null, Enums.CvtTableTypeToString(ref eTableType) });
            }

            if (rsADO is not null)
            {
                while (!rsADO.EOF)
                {
                    var objSchema = new SchemaTable();
                    objSchema.TableCatalog = rsADO.Fields[0].Value.Text();
                    objSchema.TableSchema = rsADO.Fields[1].Value.Text();
                    objSchema.TableName = rsADO.Fields[2].Value.Text();
                    objSchema.TableType = rsADO.Fields[3].Value.Text();
                    objSchema.TableGuid = rsADO.Fields[4].Value.Text();
                    objSchema.Description = rsADO.Fields[5].Value.Text();
                    objSchema.TablePropId = rsADO.Fields[6].Value.Text();
                    objSchema.DateCreated = rsADO.Fields[7].Value.Text();
                    objSchema.DateModified = rsADO.Fields[8].Value.Text();
                    schemaList.Add(objSchema);
                    rsADO.MoveNext();
                }
                rsADO.Close();
            }
            return schemaList;
        }

    }

    public sealed class SchemaIndex
    {
        private string msTableCatalog;
        public string TableCatalog
        {
            get
            {
                return msTableCatalog;
            }
            set
            {
                msTableCatalog = Strings.Trim(value);
            }
        }

        private string msTableSchema;
        public string TableSchema
        {
            get
            {
                return msTableSchema;
            }
            set
            {
                msTableSchema = Strings.Trim(value);
            }
        }

        private string msTableName;
        public string TableName
        {
            get
            {
                return msTableName;
            }
            set
            {
                msTableName = Strings.Trim(value);
            }
        }

        private string msIndexCatalog;
        public string IndexCatalog
        {
            get
            {
                return msIndexCatalog;
            }
            set
            {
                msIndexCatalog = Strings.Trim(value);
            }
        }

        private string msIndexSchema;
        public string IndexSchema
        {
            get
            {
                return msIndexSchema;
            }
            set
            {
                msIndexSchema = Strings.Trim(value);
            }
        }

        private string msIndexName;
        public string IndexName
        {
            get
            {
                return msIndexName;
            }
            set
            {
                msIndexName = Strings.Trim(value);
            }
        }

        private string msPrimaryKey;
        public string PrimaryKey
        {
            get
            {
                return msPrimaryKey;
            }
            set
            {
                msPrimaryKey = Strings.Trim(value);
            }
        }

        private string msUnique;
        public string Unique
        {
            get
            {
                return msUnique;
            }
            set
            {
                msUnique = Strings.Trim(value);
            }
        }

        private string msClustered;
        public string Clustered
        {
            get
            {
                return msClustered;
            }
            set
            {
                msClustered = Strings.Trim(value);
            }
        }

        private string msIndexType;
        public string IndexType
        {
            get
            {
                return msIndexType;
            }
            set
            {
                msIndexType = Strings.Trim(value);
            }
        }

        private string msFillFactor;
        public string FillFactor
        {
            get
            {
                return msFillFactor;
            }
            set
            {
                msFillFactor = Strings.Trim(value);
            }
        }

        private string msInitialSize;
        public string InitialSize
        {
            get
            {
                return msInitialSize;
            }
            set
            {
                msInitialSize = Strings.Trim(value);
            }
        }

        private string msNulls;
        public string Nulls
        {
            get
            {
                return msNulls;
            }
            set
            {
                msNulls = Strings.Trim(value);
            }
        }

        private string msSortBookmarks;
        public string SortBookmarks
        {
            get
            {
                return msSortBookmarks;
            }
            set
            {
                msSortBookmarks = Strings.Trim(value);
            }
        }

        private string msAutoUpdate;
        public string AutoUpdate
        {
            get
            {
                return msAutoUpdate;
            }
            set
            {
                msAutoUpdate = Strings.Trim(value);
            }
        }

        private string msNullCollation;
        public string NullCollation
        {
            get
            {
                return msNullCollation;
            }
            set
            {
                msNullCollation = Strings.Trim(value);
            }
        }

        private string msOrdinalPosition;
        public string OrdinalPosition
        {
            get
            {
                return msOrdinalPosition;
            }
            set
            {
                msOrdinalPosition = Strings.Trim(value);
            }
        }

        private string msColumnName;
        public string ColumnName
        {
            get
            {
                return msColumnName;
            }
            set
            {
                msColumnName = Strings.Trim(value);
            }
        }

        private string msColumnGuid;
        public string ColumnGuid
        {
            get
            {
                return msColumnGuid;
            }
            set
            {
                msColumnGuid = Strings.Trim(value);
            }
        }

        private string msColumnPropid;
        public string ColumnPropid
        {
            get
            {
                return msColumnPropid;
            }
            set
            {
                msColumnPropid = Strings.Trim(value);
            }
        }

        private string msCollation;
        public string Collation
        {
            get
            {
                return msCollation;
            }
            set
            {
                msCollation = Strings.Trim(value);
            }
        }

        private string msCardinality;
        public string Cardinality
        {
            get
            {
                return msCardinality;
            }
            set
            {
                msCardinality = Strings.Trim(value);
            }
        }

        private string msPages;
        public string Pages
        {
            get
            {
                return msPages;
            }
            set
            {
                msPages = Strings.Trim(value);
            }
        }

        private string msFilterCondition;
        public string FilterCondition
        {
            get
            {
                return msFilterCondition;
            }
            set
            {
                msFilterCondition = Strings.Trim(value);
            }
        }

        private string msIntegrated;
        public string Integrated
        {
            get
            {
                return msIntegrated;
            }
            set
            {
                msIntegrated = Strings.Trim(value);
            }
        }

        public static List<SchemaIndex> GetTableIndexes(string sTableName, ADODB.Connection Cnxn)
        {
            ADODB.Recordset rsADO;
            var lSchemaIndexEntities = new List<SchemaIndex>();
            rsADO = Cnxn.OpenSchema((ADODB.SchemaEnum)Enums.SchemaEnum.rmSchemaIndexes, new object[] { null, null, null, null, sTableName });
            if (rsADO is not null)
            {
                while (!rsADO.EOF)
                {
                    var objSchemaIndexes = new SchemaIndex();
                    objSchemaIndexes.TableCatalog = rsADO.Fields[0].Value.Text();
                    objSchemaIndexes.TableSchema = rsADO.Fields[1].Value.Text();
                    objSchemaIndexes.TableName = rsADO.Fields[2].Value.Text();
                    objSchemaIndexes.IndexCatalog = rsADO.Fields[3].Value.Text();
                    objSchemaIndexes.IndexSchema = rsADO.Fields[4].Value.Text();
                    objSchemaIndexes.IndexName = rsADO.Fields[5].Value.Text();
                    objSchemaIndexes.PrimaryKey = rsADO.Fields[6].Value.Text();
                    objSchemaIndexes.Unique = rsADO.Fields[7].Value.Text();
                    objSchemaIndexes.Clustered = rsADO.Fields[8].Value.Text();
                    objSchemaIndexes.IndexType = rsADO.Fields[9].Value.Text();
                    objSchemaIndexes.FillFactor = rsADO.Fields[10].Value.Text();
                    objSchemaIndexes.InitialSize = rsADO.Fields[11].Value.Text();
                    objSchemaIndexes.Nulls = rsADO.Fields[12].Value.Text();
                    objSchemaIndexes.SortBookmarks = rsADO.Fields[13].Value.Text();
                    objSchemaIndexes.AutoUpdate = rsADO.Fields[14].Value.Text();
                    objSchemaIndexes.NullCollation = rsADO.Fields[15].Value.Text();
                    objSchemaIndexes.OrdinalPosition = rsADO.Fields[16].Value.Text();
                    objSchemaIndexes.ColumnName = rsADO.Fields[17].Value.Text();
                    objSchemaIndexes.ColumnGuid = rsADO.Fields[18].Value.Text();
                    objSchemaIndexes.ColumnPropid = rsADO.Fields[19].Value.Text();
                    objSchemaIndexes.Collation = rsADO.Fields[20].Value.Text();
                    objSchemaIndexes.Cardinality = rsADO.Fields[21].Value.Text();
                    objSchemaIndexes.Pages = rsADO.Fields[22].Value.Text();
                    objSchemaIndexes.FilterCondition = rsADO.Fields[23].Value.Text();
                    objSchemaIndexes.Integrated = rsADO.Fields[24].Value.Text();
                    lSchemaIndexEntities.Add(objSchemaIndexes);
                    rsADO.MoveNext();
                }
            }
            return lSchemaIndexEntities;
        }

    }

    public sealed class SchemaInfoDetails : SchemaColumns
    {


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

        public static bool IsSystemField(string sFieldName)
        {
            return sFieldName.Substring(0, 1) == "%";
        }



        public static List<SchemaColumns> GetSchemaInfo(string sTableName, ADODB.Connection Cnxn, string sIdFieldName = "")
        {
            ADODB.Recordset rsADO;

            var lSchemaColumnList = new List<SchemaColumns>();
            if (string.IsNullOrEmpty(Strings.Trim(sIdFieldName)))
            {
                rsADO = Cnxn.OpenSchema((ADODB.SchemaEnum)Enums.SchemaEnum.rmSchemaColumns, new object[] { null, null, sTableName, null });
            }
            else if (!string.IsNullOrEmpty(sTableName) && !string.IsNullOrEmpty(sIdFieldName))
            {
                rsADO = Cnxn.OpenSchema((ADODB.SchemaEnum)Enums.SchemaEnum.rmSchemaColumns, new object[] { null, null, sTableName, sIdFieldName });
            }
            else
            {
                rsADO = Cnxn.OpenSchema((ADODB.SchemaEnum)Enums.SchemaEnum.rmSchemaColumns, new object[] { null, null, null, sIdFieldName });
            }

            if (rsADO is not null)
            {
                while (!rsADO.EOF)
                {
                    var objSchema = new SchemaColumns();
                    objSchema.TableCatalog = rsADO.Fields[0].Value.Text();
                    objSchema.TableSchema = rsADO.Fields[1].Value.Text();
                    objSchema.TableName = rsADO.Fields[2].Value.Text();
                    objSchema.ColumnName = rsADO.Fields[3].Value.Text();
                    objSchema.ColumnGuid = rsADO.Fields[4].Value.Text();
                    objSchema.ColumnPropid = rsADO.Fields[5].Value.Text();
                    objSchema.OrdinalPosition = rsADO.Fields[6].Value.Text();
                    objSchema.ColumnHasDefault = rsADO.Fields[7].Value.Text();
                    objSchema.ColumnDefault = rsADO.Fields[8].Value.Text();
                    objSchema.ColumnFlag = rsADO.Fields[9].Value.Text();
                    objSchema.IsNullable = rsADO.Fields[10].Value.Text();
                    objSchema.DataType = (Enums.DataTypeEnum)Convert.ToInt32(rsADO.Fields[11].Value.ToString());
                    objSchema.TypeGuid = rsADO.Fields[12].Value.Text();
                    objSchema.CharacterMaxLength = rsADO.Fields[13].Value.IntValue();
                    objSchema.CharacterOctetLength = rsADO.Fields[14].Value.Text();
                    objSchema.NumericPrecision = rsADO.Fields[15].Value.Text();
                    objSchema.NumericScale = rsADO.Fields[16].Value.Text();
                    objSchema.DataTimePrecision = rsADO.Fields[17].Value.Text();
                    objSchema.CharacterSetCatalog = rsADO.Fields[18].Value.Text();
                    objSchema.CharacterSetSchema = rsADO.Fields[19].Value.Text();
                    objSchema.CharacterSetName = rsADO.Fields[20].Value.Text(); ;
                    objSchema.CollationCatalog = rsADO.Fields[21].Value.Text();
                    objSchema.CollationSchema = rsADO.Fields[22].Value.Text();
                    objSchema.CollationName = rsADO.Fields[23].Value.Text();
                    objSchema.DomainCatalog = rsADO.Fields[24].Value.Text();
                    objSchema.DomainSchema = rsADO.Fields[25].Value.Text();
                    objSchema.DomainName = rsADO.Fields[26].Value.Text();
                    objSchema.Description = rsADO.Fields[27].Value.Text();
                    objSchema.Ordinal = rsADO.Fields[28].Value.Text();
                    lSchemaColumnList.Add(objSchema);
                    rsADO.MoveNext();
                }
            }
            return lSchemaColumnList;
        }

        public static List<SchemaColumns> GetTableSchemaInfo(string sTableName, ADODB.Connection Cnxn)
        {
            ADODB.Recordset rsADO;
            var lstObj = new List<SchemaColumns>();
            SchemaColumns objSchema;

            rsADO = Cnxn.OpenSchema((ADODB.SchemaEnum)Enums.SchemaEnum.rmSchemaColumns, new object[] { null, null, sTableName, null });

            if (rsADO is not null)
            {
                while (!rsADO.EOF)
                {
                    objSchema = new SchemaColumns();

                    objSchema.TableCatalog = rsADO.Fields[0].Value.Text();
                    objSchema.TableSchema = rsADO.Fields[1].Value.Text();
                    objSchema.TableName = rsADO.Fields[2].Value.Text();
                    objSchema.ColumnName = rsADO.Fields[3].Value.Text();
                    objSchema.ColumnGuid = rsADO.Fields[4].Value.Text();
                    objSchema.ColumnPropid = rsADO.Fields[5].Value.Text();
                    objSchema.OrdinalPosition = rsADO.Fields[6].Value.Text();
                    objSchema.ColumnHasDefault = rsADO.Fields[7].Value.Text();
                    objSchema.ColumnDefault = rsADO.Fields[8].Value.Text();
                    objSchema.ColumnFlag = rsADO.Fields[9].Value.Text();
                    objSchema.IsNullable = rsADO.Fields[10].Value.Text();
                    objSchema.DataType = (Enums.DataTypeEnum)rsADO.Fields[11].Value.IntValue();
                    objSchema.TypeGuid = rsADO.Fields[12].Value.Text();
                    objSchema.CharacterMaxLength = rsADO.Fields[13].Value.IntValue();
                    objSchema.CharacterOctetLength = rsADO.Fields[14].Value.Text();
                    objSchema.NumericPrecision = rsADO.Fields[15].Value.Text();
                    objSchema.NumericScale = rsADO.Fields[16].Value.Text();
                    objSchema.DataTimePrecision = rsADO.Fields[17].Value.Text();
                    objSchema.CharacterSetCatalog = rsADO.Fields[18].Value.Text();
                    objSchema.CharacterSetSchema = rsADO.Fields[19].Value.Text();
                    objSchema.CharacterSetName = rsADO.Fields[20].Value.Text();
                    objSchema.CollationCatalog = rsADO.Fields[21].Value.Text();
                    objSchema.CollationSchema = rsADO.Fields[22].Value.Text();
                    objSchema.CollationName = rsADO.Fields[23].Value.Text();
                    objSchema.DomainCatalog = rsADO.Fields[24].Value.Text();
                    objSchema.DomainSchema = rsADO.Fields[25].Value.Text();
                    objSchema.DomainName = rsADO.Fields[26].Value.Text();
                    objSchema.Description = rsADO.Fields[27].Value.Text();
                    objSchema.Ordinal = rsADO.Fields[28].Value.Text();

                    lstObj.Add(objSchema);
                    rsADO.MoveNext();
                }

                rsADO.Close();
            }

            return lstObj;

        }

    }
}