using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.WebCS;
using Microsoft.AspNetCore.Mvc.Rendering;
using Smead.Security;
using slimShared;
using Leadtools.Codecs;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using ADODB;

using Leadtools;

public class Enums
{
    public enum FormTypes
    {
        ftAttachments,
        ftOrphans,
        ftPagesOnly
    }

    public enum geSaveAction
    {
        saNewAttachment,
        saNewVersion,
        saNewPage
    }

    public enum geTrackableType
    {
        tkNone = 0,
        tkImage = 1,
        tkFax = 2,
        tkCOLD = 3,
        tkWPDoc = 5
    }
    public enum geRequestConfirmationTypes
    {
        geRequestConfirmationTypes_rctConfirmAllField = 1,
        geRequestConfirmationTypes_rctConfirmNeverField = 2,
        geRequestConfirmationTypes_rctConfirmOnceField = 0
    }
    public enum geEmailConfirmationTypes
    {
        ecConfirmIgnoreAll = 0,
        ecConfirmPromptL1IgnoreL2,
        ecConfirmPromptL2IgnoreL1,
        ecConfirmPromptAll,
        ecConfirmWarnL1IgnoreL2,
        ecConfirmWarnL1PromptL2 = 6,
        ecConfirmWarnL2IgnoreL1 = 8,
        ecConfirmWarnL2PromptL1 = 9,
        ecConfirmWarnAll = 12
    }
    public enum geEmailType
    {
        etDelivery = 0x1,
        etWaitList = 0x2,
        etException = 0x4,
        etCheckedOut = 0x8,
        etRequest = 0x10,
        etPastDue = 0x20,
        etSimple = 0x40
    }

    public enum geEmailValidationType
    {
        evNotDefined,
        evBlank,
        evInvalid,
        evValid,
        evRecordNotFound
    }

    public enum geTrackingOriginatedFrom
    {
        tofUnknown = 0,
        tofManualTransfer = 1,
        tofBarcodeTracking = 2,
        tofImports = 4,
        tofAutoAssign = 8
    }

    public enum geFTSTypes
    {
        tsNotDefined,
        tsFileName,
        tsImage,
        tsPCFile,
        tsCOLD,
        tsImageContent,
        tsPCFileContent,
        tsCOLDContent,
        tsFieldName
    }

    public enum meFinalDispositionStatusType
    {
        /// <summary>
        /// Indicates the current disposition status.
        ///    Active: Has not reached the retention point.
        /// Archived: Item has been archived or transferred to an "Archive" location.
        /// Destroyed: Item has been destoryed.
        /// Purged is not needed because the record is deleted from the system and never flagged as purged.
        /// </summary>
        fdstActive = 0,
        fdstArchived = 1,
        fdstDestroyed = 2
    }


    public enum meFinalDisposition
    {
        /// <summary>
        /// Indicates what happens to the record when it reaches the end of its retention period.
        ///        None: Retention is not active. Nothing happens.
        ///     Archive: Records (including child records) are marked as archived, but no data or attachments are removed from the
        ///              system. Items are transferred to a ArchiveLocation. Can only be viewed by a user with security permissions
        ///              to review "archived" items.
        /// Destruction: Records (including child records) are marked as destroyed. All attachments are permanently deleted. The data
        ///              is preserved, but can only be viewed by a user with security permissions to review "destroyed" items.
        ///      Purged: Records (including child records) and attachments are permanently deleted. The information is unrecoverable
        ///              once purged.
        /// </summary>
        /// <remarks></remarks>
        fdNone = 0,
        fdPermanentArchive = 1,
        fdDestruction = 2,
        fdPurge = 3
    }

    public enum SelectionTypes
    {
        stSelection,
        stSuggestion,
        stText
    }

    public enum geTrackingOutType
    {
        otUseOutField,
        otAlwaysOut,
        otAlwaysIn
    }

    public enum geRunTypes
    {
        irNormal = 0,
        irAuto = 1,
        irMonitor = 2
    }

    public enum meRetentionCodeAssignment
    {
        rcaManual = 0,
        rcaCurrentTable = 1,
        rcaRelatedTable = 2
    }

    public enum SecureObjects
    {
        Application = Smead.Security.SecureObject.SecureObjectType.Application,
        Table = Smead.Security.SecureObject.SecureObjectType.Table,
        View = Smead.Security.SecureObject.SecureObjectType.View,
        Annotations = Smead.Security.SecureObject.SecureObjectType.Annotations,
        WorkGroup = Smead.Security.SecureObject.SecureObjectType.WorkGroup,
        Attachments = Smead.Security.SecureObject.SecureObjectType.Attachments,
        Reports = Smead.Security.SecureObject.SecureObjectType.Reports,
        Retention = Smead.Security.SecureObject.SecureObjectType.Retention,
        LinkScript = Smead.Security.SecureObject.SecureObjectType.LinkScript,
        ScanRules = Smead.Security.SecureObject.SecureObjectType.ScanRules,
        Volumes = Smead.Security.SecureObject.SecureObjectType.Volumes,
        OutputSettings = Smead.Security.SecureObject.SecureObjectType.OutputSettings,
        Orphans = Smead.Security.SecureObject.SecureObjectType.Orphans
    }

    public enum PassportPermissions
    {
        Access = Smead.Security.Permissions.Permission.Access,
        Execute = Smead.Security.Permissions.Permission.Execute,
        View = Smead.Security.Permissions.Permission.View,
        Add = Smead.Security.Permissions.Permission.Add,
        Edit = Smead.Security.Permissions.Permission.Edit,
        Delete = Smead.Security.Permissions.Permission.Delete,
        Destroy = Smead.Security.Permissions.Permission.Destroy,
        Transfer = Smead.Security.Permissions.Permission.Transfer,
        Request = Smead.Security.Permissions.Permission.Request,
        RequestHigh = Smead.Security.Permissions.Permission.RequestHigh,
        Configure = Smead.Security.Permissions.Permission.Configure,
        PrintLabel = Smead.Security.Permissions.Permission.PrintLabel,
        Scanning = Smead.Security.Permissions.Permission.Scanning,
        Index = Smead.Security.Permissions.Permission.Index,
        Move = Smead.Security.Permissions.Permission.Move,
        Copy = Smead.Security.Permissions.Permission.Copy,
        Redact = Smead.Security.Permissions.Permission.Redact,
        Versioning = Smead.Security.Permissions.Permission.Versioning,
        Import = Smead.Security.Permissions.Permission.Import,
        Export = Smead.Security.Permissions.Permission.Export,
        Email = Smead.Security.Permissions.Permission.Email,
        Print = Smead.Security.Permissions.Permission.Print,
        RequestOnBehalf = Smead.Security.Permissions.Permission.RequestOnBehalf
    }

    public enum geOfficialRecordConversonType
    {
        orcNoConversion = 0,
        orcFirstVersionConversion = 1,
        orcLastVersionConversion = 2,
        orcConversionToNothing = 4
    }

    public enum geTableType
    {
        SystemTables,
        UserTables,
        Views,
        AllTableTypes
    }

    public enum geViewType
    {
        vtMaster,
        vtVLibr1,
        vtVLibr2,
        vtVLibr3,
        vtVLibr4,
        vtVLibr5
    }

    public enum meFieldTypes
    {
        ftLong = 1,
        ftCounter = 2,
        ftText = 3,
        ftSmeadCounter = 4
    }

    public enum meTableFieldTypes
    {
        ftLong = 0,
        ftCounter = 1,
        ftText = 2,
        ftInteger = 3,
        ftBoolean = 4,
        ftDouble = 5,
        ftDate = 6,
        ftMemo = 7,
        ftSmeadCounter = 8
    }

    public enum eNodeLevel
    {
        ndDatabase = 0,
        ndTabSets = 1,
        ndTableTabRel = 2
    }

    public enum eConnectionType
    {
        conDefault = 1,
        conDatabase = 2
    }

    public enum geViewColumnDisplayType
    {
        cvAlways,
        cvBaseTab,
        cvPopupTab,
        cvNotVisible,
        cvSmartColumns,
        cvTrackingSmartColumns,
        cvBasedOnProperty,
        cvNeverVisible
    }

    public enum geViewColumnsLookupType
    {
        ltUndefined = -1,
        ltDirect,
        ltLookup,
        ltImageFlag,
        ltFaxFlag,
        ltCOLDFlag,
        ltPCFilesFlag,
        ltReserved,
        ltChildrenFlag,
        ltRowNumber,
        ltAnyFlag,
        ltTrackingStatus,
        ltTrackingLocation,
        ltChildLookdownCommaDisplayDups,
        ltChildLookdownLFDisplayDups,
        ltChildLookdownCommaHideDups,
        ltChildLookdownLFHideDups,
        ltChildrenCounts,
        ltChildLookdownTotals,
        ltTableUserName,
        ltTableGetUserNameLookup,
        ltRetentionFlag,
        ltAltTableAnyFlag,
        ltCurrentlyAtDisplay,
        ltCurrentlyAtTableName,
        ltSignature,
        ltUserDisplayNameLookup
    }

    /// <summary>
    /// ADO wrapper classes.
    /// </summary>
    /// <remarks></remarks>
    public enum ObjectStateEnum
    {
        rmStateClosed = ADODB.ObjectStateEnum.adStateClosed,
        rmStateConnecting = ADODB.ObjectStateEnum.adStateConnecting,
        rmStateExecuting = ADODB.ObjectStateEnum.adStateExecuting,
        rmStateFetching = ADODB.ObjectStateEnum.adStateFetching,
        rmStateOpen = ADODB.ObjectStateEnum.adStateOpen
    }
    public enum AffectEnum
    {
        rmAffectAll = ADODB.AffectEnum.adAffectAll,
        rmAffectAllChapters = ADODB.AffectEnum.adAffectAllChapters,
        rmAffectCurrent = ADODB.AffectEnum.adAffectCurrent,
        rmAffectGroup = ADODB.AffectEnum.adAffectGroup
    }
    public enum BookmarkEnum
    {
        rmBookmarkCurrent = ADODB.BookmarkEnum.adBookmarkCurrent,
        rmBookmarkFirst = ADODB.BookmarkEnum.adBookmarkFirst,
        rmBookmarkLast = ADODB.BookmarkEnum.adBookmarkLast
    }
    public enum CompareEnum
    {
        rmCompareEqual = ADODB.CompareEnum.adCompareEqual,
        rmCompareGreaterThan = ADODB.CompareEnum.adCompareGreaterThan,
        rmCompareLessThan = ADODB.CompareEnum.adCompareLessThan,
        rmCompareNotComparable = ADODB.CompareEnum.adCompareNotComparable,
        rmCompareNotEqual = ADODB.CompareEnum.adCompareNotEqual
    }
    public enum CommandTypeEnum
    {
        rmCmdFile = ADODB.CommandTypeEnum.adCmdFile,
        rmCmdStoredProc = ADODB.CommandTypeEnum.adCmdStoredProc,
        rmCmdTable = ADODB.CommandTypeEnum.adCmdTable,
        rmCmdTableDirect = ADODB.CommandTypeEnum.adCmdTableDirect,
        rmCmdText = ADODB.CommandTypeEnum.adCmdText,
        rmCmdUnknown = ADODB.CommandTypeEnum.adCmdUnknown,
        rmCmdUnspecified = ADODB.CommandTypeEnum.adCmdUnspecified
    }
    public enum ConnectModeEnum
    {
        rmModeRead = ADODB.ConnectModeEnum.adModeRead,
        rmModeReadWrite = ADODB.ConnectModeEnum.adModeReadWrite,
        rmModeRecursive = ADODB.ConnectModeEnum.adModeRecursive,
        rmModeShareDenyNone = ADODB.ConnectModeEnum.adModeShareDenyNone,
        rmModeShareDenyRead = ADODB.ConnectModeEnum.adModeShareDenyRead,
        rmModeShareDenyWrite = ADODB.ConnectModeEnum.adModeShareDenyWrite,
        rmModeShareExclusive = ADODB.ConnectModeEnum.adModeShareExclusive,
        rmModeUnknown = ADODB.ConnectModeEnum.adModeUnknown,
        rmModeWrite = ADODB.ConnectModeEnum.adModeWrite
    }
    public enum CursorLocationEnum
    {
        rmUseClient = ADODB.CursorLocationEnum.adUseClient,
        rmUseClientBatch = ADODB.CursorLocationEnum.adUseClientBatch,
        rmUseNone = ADODB.CursorLocationEnum.adUseNone,
        rmUseServer = ADODB.CursorLocationEnum.adUseServer
    }
    public enum CursorOptionEnum
    {
        rmAddNew = ADODB.CursorOptionEnum.adAddNew,
        rmApproxPosition = ADODB.CursorOptionEnum.adApproxPosition,
        rmBookmark = ADODB.CursorOptionEnum.adBookmark,
        rmDelete = ADODB.CursorOptionEnum.adDelete,
        rmFind = ADODB.CursorOptionEnum.adFind,
        rmHoldRecords = ADODB.CursorOptionEnum.adHoldRecords,
        rmIndex = ADODB.CursorOptionEnum.adIndex,
        rmMovePrevious = ADODB.CursorOptionEnum.adMovePrevious,
        rmNotify = ADODB.CursorOptionEnum.adNotify,
        rmResync = ADODB.CursorOptionEnum.adResync,
        rmSeek = ADODB.CursorOptionEnum.adSeek,
        rmUpdate = ADODB.CursorOptionEnum.adUpdate,
        rmUpdateBatch = ADODB.CursorOptionEnum.adUpdateBatch
    }
    public enum CursorTypeEnum
    {
        rmOpenDynamic = ADODB.CursorTypeEnum.adOpenDynamic,
        rmOpenForwardOnly = ADODB.CursorTypeEnum.adOpenForwardOnly,
        rmOpenKeyset = ADODB.CursorTypeEnum.adOpenKeyset,
        rmOpenStatic = ADODB.CursorTypeEnum.adOpenStatic,
        rmUnspecified = ADODB.CursorTypeEnum.adOpenUnspecified
    }
    public enum DataTypeEnum
    {
        rmArray = ADODB.DataTypeEnum.adArray,
        rmBigInt = ADODB.DataTypeEnum.adBigInt,
        rmBinary = ADODB.DataTypeEnum.adBinary,
        rmBoolean = ADODB.DataTypeEnum.adBoolean,
        rmBSTR = ADODB.DataTypeEnum.adBSTR,
        rmChapter = ADODB.DataTypeEnum.adChapter,
        rmChar = ADODB.DataTypeEnum.adChar,
        rmCurrency = ADODB.DataTypeEnum.adCurrency,
        rmDate = ADODB.DataTypeEnum.adDate,
        rmDBDate = ADODB.DataTypeEnum.adDBDate,
        rmDBTime = ADODB.DataTypeEnum.adDBTime,
        rmDBTimeStamp = ADODB.DataTypeEnum.adDBTimeStamp,
        rmDecimal = ADODB.DataTypeEnum.adDecimal,
        rmDouble = ADODB.DataTypeEnum.adDouble,
        rmEmpty = ADODB.DataTypeEnum.adEmpty,
        rmError = ADODB.DataTypeEnum.adError,
        rmFileTime = ADODB.DataTypeEnum.adFileTime,
        rmGUID = ADODB.DataTypeEnum.adGUID,
        rmIDispatch = ADODB.DataTypeEnum.adIDispatch,
        rmInteger = ADODB.DataTypeEnum.adInteger,
        rmIUnknown = ADODB.DataTypeEnum.adIUnknown,
        rmLongVarBinary = ADODB.DataTypeEnum.adLongVarBinary,
        rmLongVarChar = ADODB.DataTypeEnum.adLongVarChar,
        rmLongVarWChar = ADODB.DataTypeEnum.adLongVarWChar,
        rmNumeric = ADODB.DataTypeEnum.adNumeric,
        rmPropVariant = ADODB.DataTypeEnum.adPropVariant,
        rmSingle = ADODB.DataTypeEnum.adSingle,
        rmSmallInt = ADODB.DataTypeEnum.adSmallInt,
        rmTinyInt = ADODB.DataTypeEnum.adTinyInt,
        rmUnsignedBigInt = ADODB.DataTypeEnum.adUnsignedBigInt,
        rmUnsignedInt = ADODB.DataTypeEnum.adUnsignedInt,
        rmUnsignedSmallInt = ADODB.DataTypeEnum.adUnsignedSmallInt,
        rmUnsignedTinyInt = ADODB.DataTypeEnum.adUnsignedTinyInt,
        rmUserDefined = ADODB.DataTypeEnum.adUserDefined,
        rmVarBinary = ADODB.DataTypeEnum.adVarBinary,
        rmVarChar = ADODB.DataTypeEnum.adVarChar,
        rmVariant = ADODB.DataTypeEnum.adVariant,
        rmVarNumeric = ADODB.DataTypeEnum.adVarNumeric,
        rmVarWChar = ADODB.DataTypeEnum.adVarWChar,
        rmWChar = ADODB.DataTypeEnum.adWChar
    }
    public enum EditModeEnum
    {
        rmEditAdd = ADODB.EditModeEnum.adEditAdd,
        rmEditDelete = ADODB.EditModeEnum.adEditDelete,
        rmEditInProgress = ADODB.EditModeEnum.adEditInProgress,
        rmEditNone = ADODB.EditModeEnum.adEditNone
    }
    public enum FieldAttributeEnum
    {
        rmFldCacheDeferred = ADODB.FieldAttributeEnum.adFldCacheDeferred,
        rmFldFixed = ADODB.FieldAttributeEnum.adFldFixed,
        rmFldIsChapter = ADODB.FieldAttributeEnum.adFldIsChapter,
        rmFldIsCollection = ADODB.FieldAttributeEnum.adFldIsCollection,
        rmFldIsDefaultStream = ADODB.FieldAttributeEnum.adFldIsDefaultStream,
        rmFldIsNullable = ADODB.FieldAttributeEnum.adFldIsNullable,
        rmFldIsRowURL = ADODB.FieldAttributeEnum.adFldIsRowURL,
        rmFldKeyColumn = ADODB.FieldAttributeEnum.adFldKeyColumn,
        rmFldLong = ADODB.FieldAttributeEnum.adFldLong,
        rmFldMayBeNull = ADODB.FieldAttributeEnum.adFldMayBeNull,
        rmFldMayDefer = ADODB.FieldAttributeEnum.adFldMayDefer,
        rmFldNegativeScale = ADODB.FieldAttributeEnum.adFldNegativeScale,
        rmFldRowID = ADODB.FieldAttributeEnum.adFldRowID,
        rmFldRowVersion = ADODB.FieldAttributeEnum.adFldRowVersion,
        rmFldUnknownUpdatable = ADODB.FieldAttributeEnum.adFldUnknownUpdatable,
        rmFldUnspecified = ADODB.FieldAttributeEnum.adFldUnspecified,
        rmFldUpdatable = ADODB.FieldAttributeEnum.adFldUpdatable
    }
    public enum FieldStatusEnum
    {
        rmFieldAlreadyExists = ADODB.FieldStatusEnum.adFieldAlreadyExists,
        rmFieldBadStatus = ADODB.FieldStatusEnum.adFieldBadStatus,
        rmFieldCannotComplete = ADODB.FieldStatusEnum.adFieldCannotComplete,
        rmFieldCannotDeleteSource = ADODB.FieldStatusEnum.adFieldCannotDeleteSource,
        rmFieldCantConvertValue = ADODB.FieldStatusEnum.adFieldCantConvertValue,
        rmFieldCantCreate = ADODB.FieldStatusEnum.adFieldCantCreate,
        rmFieldDataOverflow = ADODB.FieldStatusEnum.adFieldDataOverflow,
        rmFieldDefault = ADODB.FieldStatusEnum.adFieldDefault,
        rmFieldDoesNotExist = ADODB.FieldStatusEnum.adFieldDoesNotExist,
        rmFieldIgnore = ADODB.FieldStatusEnum.adFieldIgnore,
        rmFieldIntegrityViolation = ADODB.FieldStatusEnum.adFieldIntegrityViolation,
        rmFieldInvalidURL = ADODB.FieldStatusEnum.adFieldInvalidURL,
        rmFieldIsNull = ADODB.FieldStatusEnum.adFieldIsNull,
        rmFieldOK = ADODB.FieldStatusEnum.adFieldOK,
        rmFieldOutOfSpace = ADODB.FieldStatusEnum.adFieldOutOfSpace,
        rmFieldPendingChange = ADODB.FieldStatusEnum.adFieldPendingChange,
        rmFieldPendingDelete = ADODB.FieldStatusEnum.adFieldPendingDelete,
        rmFieldPendingInsert = ADODB.FieldStatusEnum.adFieldPendingInsert,
        rmFieldPendingUnknown = ADODB.FieldStatusEnum.adFieldPendingUnknown,
        rmFieldPendingUnknownDelete = ADODB.FieldStatusEnum.adFieldPendingUnknownDelete,
        rmFieldPermissionDenied = ADODB.FieldStatusEnum.adFieldPermissionDenied,
        rmFieldReadOnly = ADODB.FieldStatusEnum.adFieldReadOnly,
        rmFieldResourceExists = ADODB.FieldStatusEnum.adFieldResourceExists,
        rmFieldResourceLocked = ADODB.FieldStatusEnum.adFieldResourceLocked,
        rmFieldResourceOutOfScope = ADODB.FieldStatusEnum.adFieldResourceOutOfScope,
        rmFieldSchemaViolation = ADODB.FieldStatusEnum.adFieldSchemaViolation,
        rmFieldSignMismatch = ADODB.FieldStatusEnum.adFieldSignMismatch,
        rmFieldTruncated = ADODB.FieldStatusEnum.adFieldTruncated,
        rmFieldUnavailable = ADODB.FieldStatusEnum.adFieldUnavailable,
        rmFieldVolumeNotFound = ADODB.FieldStatusEnum.adFieldVolumeNotFound
    }
    public enum FilterGroupEnum
    {
        rmFilterAffectedRecords = ADODB.FilterGroupEnum.adFilterAffectedRecords,
        rmFilterConflictingRecords = ADODB.FilterGroupEnum.adFilterConflictingRecords,
        rmFilterFetchedRecords = ADODB.FilterGroupEnum.adFilterFetchedRecords,
        rmFilterNone = ADODB.FilterGroupEnum.adFilterNone,
        rmFilterPendingRecords = ADODB.FilterGroupEnum.adFilterPendingRecords,
        rmFilterPredicate = ADODB.FilterGroupEnum.adFilterPredicate
    }
    public enum IsolationLevelEnum
    {
        rmXactBrowse = ADODB.IsolationLevelEnum.adXactBrowse,
        rmXactChaos = ADODB.IsolationLevelEnum.adXactChaos,
        rmXactCursorStability = ADODB.IsolationLevelEnum.adXactCursorStability,
        rmXactIsolated = ADODB.IsolationLevelEnum.adXactIsolated,
        rmXactReadCommitted = ADODB.IsolationLevelEnum.adXactReadCommitted,
        rmXactReadUncommitted = ADODB.IsolationLevelEnum.adXactReadUncommitted,
        rmXactRepeatableRead = ADODB.IsolationLevelEnum.adXactRepeatableRead,
        rmXactSerializable = ADODB.IsolationLevelEnum.adXactSerializable,
        rmXactUnspecified = ADODB.IsolationLevelEnum.adXactUnspecified
    }
    public enum LockTypeEnum
    {
        rmLockBatchOptimistic = ADODB.LockTypeEnum.adLockBatchOptimistic,
        rmLockOptimistic = ADODB.LockTypeEnum.adLockOptimistic,
        rmLockPessimistic = ADODB.LockTypeEnum.adLockPessimistic,
        rmLockReadOnly = ADODB.LockTypeEnum.adLockReadOnly,
        rmLockUnspecified = ADODB.LockTypeEnum.adLockUnspecified
    }
    public enum MarshalOptionsEnum
    {
        rmMarshalAll = ADODB.MarshalOptionsEnum.adMarshalAll,
        rmMarshalModifiedOnly = ADODB.MarshalOptionsEnum.adMarshalModifiedOnly
    }
    public enum ParameterDirectionEnum
    {
        rmParamInput = ADODB.ParameterDirectionEnum.adParamInput,
        rmParamInputOutput = ADODB.ParameterDirectionEnum.adParamInputOutput,
        rmParamOutput = ADODB.ParameterDirectionEnum.adParamOutput,
        rmParamReturnValue = ADODB.ParameterDirectionEnum.adParamReturnValue,
        rmParamUnknown = ADODB.ParameterDirectionEnum.adParamUnknown
    }
    public enum PersistFormatEnum
    {
        rmPersistADTG = ADODB.PersistFormatEnum.adPersistADTG,
        rmPersistXML = ADODB.PersistFormatEnum.adPersistXML
    }
    public enum PositionEnum
    {
        rmPosBOF = ADODB.PositionEnum.adPosBOF,
        rmPosEOF = ADODB.PositionEnum.adPosEOF,
        rmPosUnknown = ADODB.PositionEnum.adPosUnknown
    }
    public enum RecordStatusEnum
    {
        rmRecCanceled = ADODB.RecordStatusEnum.adRecCanceled,
        rmRecCantRelease = ADODB.RecordStatusEnum.adRecCantRelease,
        rmRecConcurrencyViolation = ADODB.RecordStatusEnum.adRecConcurrencyViolation,
        rmRecDBDeleted = ADODB.RecordStatusEnum.adRecDBDeleted,
        rmRecDeleted = ADODB.RecordStatusEnum.adRecDeleted,
        rmRecIntegrityViolation = ADODB.RecordStatusEnum.adRecIntegrityViolation,
        rmRecInvalid = ADODB.RecordStatusEnum.adRecInvalid,
        rmRecMaxChangesExceeded = ADODB.RecordStatusEnum.adRecMaxChangesExceeded,
        rmRecModified = ADODB.RecordStatusEnum.adRecModified,
        rmRecMultipleChanges = ADODB.RecordStatusEnum.adRecMultipleChanges,
        rmRecNew = ADODB.RecordStatusEnum.adRecNew,
        rmRecObjectOpen = ADODB.RecordStatusEnum.adRecObjectOpen,
        rmRecOK = ADODB.RecordStatusEnum.adRecOK,
        rmRecOutOfMemory = ADODB.RecordStatusEnum.adRecOutOfMemory,
        rmRecPendingChanges = ADODB.RecordStatusEnum.adRecPendingChanges,
        rmRecPermissionDenied = ADODB.RecordStatusEnum.adRecPermissionDenied,
        rmRecSchemaViolation = ADODB.RecordStatusEnum.adRecSchemaViolation,
        rmRecUnmodified = ADODB.RecordStatusEnum.adRecUnmodified
    }
    public enum ResyncEnum
    {
        rmResyncAllValues = ADODB.ResyncEnum.adResyncAllValues,
        rmResyncUnderlyingValues = ADODB.ResyncEnum.adResyncUnderlyingValues
    }
    public enum SearchDirectionEnum
    {
        rmSearchBackward = ADODB.SearchDirectionEnum.adSearchBackward,
        rmSearchForward = ADODB.SearchDirectionEnum.adSearchForward
    }
    public enum SeekEnum
    {
        rmSeekAfter = ADODB.SeekEnum.adSeekAfter,
        rmSeekAfterEQ = ADODB.SeekEnum.adSeekAfterEQ,
        rmSeekBefore = ADODB.SeekEnum.adSeekBefore,
        rmSeekBeforeEQ = ADODB.SeekEnum.adSeekBeforeEQ,
        rmSeekFirstEQ = ADODB.SeekEnum.adSeekFirstEQ,
        rmSeekLastEQ = ADODB.SeekEnum.adSeekLastEQ
    }
    public enum SchemaEnum
    {
        rmSchemaActions = ADODB.SchemaEnum.adSchemaActions,
        rmSchemaAsserts = ADODB.SchemaEnum.adSchemaAsserts,
        rmSchemaCatalogs = ADODB.SchemaEnum.adSchemaCatalogs,
        rmSchemaCharacterSets = ADODB.SchemaEnum.adSchemaCharacterSets,
        rmSchemaCheckConstraints = ADODB.SchemaEnum.adSchemaCheckConstraints,
        rmSchemaCollations = ADODB.SchemaEnum.adSchemaCollations,
        rmSchemaColumnPrivileges = ADODB.SchemaEnum.adSchemaColumnPrivileges,
        rmSchemaColumns = ADODB.SchemaEnum.adSchemaColumns,
        rmSchemaColumnsDomainUsage = ADODB.SchemaEnum.adSchemaColumnsDomainUsage,
        rmSchemaCommands = ADODB.SchemaEnum.adSchemaCommands,
        rmSchemaConstraintColumnUsage = ADODB.SchemaEnum.adSchemaConstraintColumnUsage,
        rmSchemaConstraintTableUsage = ADODB.SchemaEnum.adSchemaConstraintTableUsage,
        rmSchemaCubes = ADODB.SchemaEnum.adSchemaCubes,
        rmSchemaDBInfoKeywords = ADODB.SchemaEnum.adSchemaDBInfoKeywords,
        rmSchemaDBInfoLiterals = ADODB.SchemaEnum.adSchemaDBInfoLiterals,
        rmSchemaDimensions = ADODB.SchemaEnum.adSchemaDimensions,
        rmSchemaForeignKeys = ADODB.SchemaEnum.adSchemaForeignKeys,
        rmSchemaFunctions = ADODB.SchemaEnum.adSchemaFunctions,
        rmSchemaHierarchies = ADODB.SchemaEnum.adSchemaHierarchies,
        rmSchemaIndexes = ADODB.SchemaEnum.adSchemaIndexes,
        rmSchemaKeyColumnUsage = ADODB.SchemaEnum.adSchemaKeyColumnUsage,
        rmSchemaLevels = ADODB.SchemaEnum.adSchemaLevels,
        rmSchemaMeasures = ADODB.SchemaEnum.adSchemaMeasures,
        rmSchemaMembers = ADODB.SchemaEnum.adSchemaMembers,
        rmSchemaPrimaryKeys = ADODB.SchemaEnum.adSchemaPrimaryKeys,
        rmSchemaProcedureColumns = ADODB.SchemaEnum.adSchemaProcedureColumns,
        rmSchemaProcedureParameters = ADODB.SchemaEnum.adSchemaProcedureParameters,
        rmSchemaProcedures = ADODB.SchemaEnum.adSchemaProcedures,
        rmSchemaProperties = ADODB.SchemaEnum.adSchemaProperties,
        rmSchemaProviderSpecific = ADODB.SchemaEnum.adSchemaProviderSpecific,
        rmSchemaProviderTypes = ADODB.SchemaEnum.adSchemaProviderTypes,
        rmSchemaReferentialConstraints = ADODB.SchemaEnum.adSchemaReferentialConstraints,
        rmSchemaReferentialContraints = ADODB.SchemaEnum.adSchemaReferentialContraints,
        rmSchemaSchemata = ADODB.SchemaEnum.adSchemaSchemata,
        rmSchemaSets = ADODB.SchemaEnum.adSchemaSets,
        rmSchemaSQLLanguages = ADODB.SchemaEnum.adSchemaSQLLanguages,
        rmSchemaStatistics = ADODB.SchemaEnum.adSchemaStatistics,
        rmSchemaTableConstraints = ADODB.SchemaEnum.adSchemaTableConstraints,
        rmSchemaTablePrivileges = ADODB.SchemaEnum.adSchemaTablePrivileges,
        rmSchemaTables = ADODB.SchemaEnum.adSchemaTables,
        rmSchemaTranslations = ADODB.SchemaEnum.adSchemaTranslations,
        rmSchemaTrustees = ADODB.SchemaEnum.adSchemaTrustees,
        rmSchemaUsagePrivileges = ADODB.SchemaEnum.adSchemaUsagePrivileges,
        rmSchemaViewColumnUsage = ADODB.SchemaEnum.adSchemaViewColumnUsage,
        rmSchemaViews = ADODB.SchemaEnum.adSchemaViews,
        rmSchemaViewTableUsage = ADODB.SchemaEnum.adSchemaViewTableUsage
    }
    public enum StringFormatEnum
    {
        rmClipString = ADODB.StringFormatEnum.adClipString
    }

    public enum SecureObjectType
    {
        Application = 1,
        Table = 2,
        View = 3,
        Annotations = 4,
        WorkGroup = 5,
        Attachments = 6,
        Reports = 7,
        Retention = 8,
        LinkScript = 9,
        ScanRules = 10,
        Volumes = 11,
        OutputSettings = 12,
        Orphans = 13
    }

    public enum SavedType
    {
        Query = 0,
        Favorite = 1,
        Valt = 2
    }

    /// <summary>
    /// Background Process Task Types for SLServiceTask Table(Column: TaskType)
    /// </summary>
    public enum BackgroundTaskType
    {
        Normal = 2,
        Transfer,
        Export
    }

    /// <summary>
    /// Background Process Task Types in Detail
    /// </summary>
    public enum BackgroundTaskInDetail
    {
        Normal = 2,
        Transfer,
        ExportCSV,
        ExportTXT
    }

    public enum BackgroundTaskProcess
    {
        Normal = 1,
        Background,
        ExceedMaxLimit,
        ServiceNotEnabled,
        NoSelection
    }

    /// <summary>
    /// Background Process for Service Task Status
    /// </summary>
    public enum BackgroundTaskStatus
    {
        [Description("Pending")]
        Pending = 1,
        [Description("InProgress")]
        InProgress,
        [Description("Completed")]
        Completed,
        [Description("Error")]
        Error,
        [Description("InQue")]
        InQue
    }

    public static object CvtTableTypeToString(ref geTableType eTableType)
    {
        object CvtTableTypeToStringRet = default;
        switch (eTableType)
        {
            case geTableType.SystemTables:
                {
                    CvtTableTypeToStringRet = "SYSTEM TABLE";
                    break;
                }
            case geTableType.UserTables:
                {
                    CvtTableTypeToStringRet = "TABLE";
                    break;
                }
            case geTableType.Views:
                {
                    CvtTableTypeToStringRet = "VIEW";
                    break;
                }

            default:
                {
                    CvtTableTypeToStringRet = null;
                    break;
                }
        }

        return CvtTableTypeToStringRet;
    }

}

public sealed class Common
{
    public static string[] dataType = new string[] { "datetime", "decimal", "double", "int16", "int32", "int64" };
    public const string BOOLEAN_TYPE = "boolean";
    // Data types and sizes constants
    public const string FT_BINARY = "Binary";
    public const string FT_LONG_INTEGER = "Long Integer";
    public const string FT_LONG_INTEGER_SIZE = "4";
    public const string FT_AUTO_INCREMENT = "Automatic Counter";
    public const string FT_AUTO_INCREMENT_SIZE = "4";
    public const string FT_TEXT = "Text";
    public const string FT_SHORT_INTEGER = "Short Integer";
    public const string FT_SHORT_INTEGER_SIZE = "2";
    public const string FT_BOOLEAN = "Yes/No";
    public const string FT_BOOLEAN_SIZE = "1";
    public const string FT_DOUBLE = "Floating Number (Double)";
    public const string FT_DOUBLE_SIZE = "8";
    public const string FT_DATE = "Date";
    public const string FT_DATE_SIZE = "8";
    public const string FT_MEMO = "Memo";
    public const string FT_MEMO_SIZE = "N/A";
    public const string FT_SMEAD_COUNTER = "Counter";
    public const string FT_SMEAD_COUNTER_SIZE = "4";
    public const string FT_UNKNOWN = "Unknown";

    public const string SECURE_APPLICATION = "Applications";
    // IMPORTANT! Space is required at the beginning of SECURE_REPORTS* constants
    public const string SECURE_REPORTS_AUDIT = " Auditing";
    public const string SECURE_REPORTS_REQUEST = " Requestor";
    public const string SECURE_REPORTS_RETENTION = " Retention";
    public const string SECURE_REPORTS_TRACKING = " Tracking";
    public const string SECURE_LABEL_SETUP = "Label Integration";
    public const string SECURE_SQL_SCRIPT = "Database Scripting";
    public const string SECURE_IMPORT_SETUP = "Import Setup";
    public const string SECURE_IPUBLISH = "Snapshot";
    public const string SECURE_ORPHANS = " Orphans";
    public const string SECURE_OPTIONS = "Options";
    public const string SECURE_RETENTION_DISPO = "Disposition";
    public const string SECURE_RETENTION_SETUP = "Code Maintenance";
    public const string SECURE_RETENTION_ARCHIVE = "View Archived Records";
    public const string SECURE_RETENTION_DESTROY = "View Destroyed Records";
    public const string SECURE_RETENTION_INACTIVE = "View Inactive Records";
    public const string SECURE_REPORT_STYLES = "Report Styles";
    public const string SECURE_SCANNER = "Scanner";
    public const string SECURE_SECURITY = "Security Configuration";
    public const string SECURE_SECURITY_USER = "Security Users";
    public const string SECURE_STORAGE = "Storage Configuration";
    public const string SECURE_TRACKING = "Tracking";
    public const string SECURE_MYQUERY = "My Queries";
    public const string SECURE_MYFAVORITE = "My Favorites";
    private const string EncryptionKey = "MAKV2SPBNI99212";
    public const string SECURE_DASHBOARD = "Dashboard";
    public const string SECURE_REPORTS = "Reports";
    private Common()
    {
    }
    public static Array GetGridSelectedRowsIds(Array pRowSelected)
    {

        return pRowSelected;
    }

    public static IEnumerable<SelectListItem> DefaultDropdownSelectionValue
    {
        get
        {
            return Enumerable.Repeat(new SelectListItem()
            {
                Value = "-1",
                Text = "--Select--"
            }, count: 1);
        }
    }

    public static IEnumerable<SelectListItem> DefaultDropdownSelectionBlank
    {
        get
        {
            return Enumerable.Repeat(new SelectListItem()
            {
                Value = 0.ToString(),
                Text = ""
            }, count: 1);
        }
    }

    public static object ConvertDTToJQGridResult(DataTable dtRecords, int totalRecords, string sidx, string sord, int page, int rows)
    {
        int totalPages = (int)Math.Round(Math.Truncate(Math.Ceiling(totalRecords / (float)rows)));
        int pageIndex = Convert.ToInt32(page) - 1;

        var Dv = dtRecords.AsDataView();
        if (!string.IsNullOrEmpty(sidx))
        {
            Dv.Sort = sidx + " " + sord;
        }

        var objListOfEmployeeEntity = new List<object>();
        foreach (DataRowView dRow in Dv)
        {
            var hashtable = new Hashtable();
            foreach (DataColumn column in dtRecords.Columns)
                hashtable.Add(column.ColumnName, dRow[column.ColumnName].ToString());
            objListOfEmployeeEntity.Add(hashtable);
        }
        var jsonData = new { total = totalPages, page, records = totalRecords, rows = objListOfEmployeeEntity };
        return jsonData;
    }

    public static object ConvertDataTableToJQGridResult(DataTable dtRecords, string sidx, string sord, int page, int rows)
    {
        var totalRecords = default(int);
        var totalPages = default(int);
        int pageIndex;
        object lFinalResult = null;

        try
        {
            totalRecords = dtRecords.Rows.Count;
            totalPages = (int)Math.Round(Math.Truncate(Math.Ceiling(totalRecords / (float)rows)));
            pageIndex = Convert.ToInt32(page) - 1;
            var Dv = dtRecords.AsDataView();
            if (!string.IsNullOrEmpty(sidx))
            {
                Dv.Sort = sidx + " " + sord;
            }

            var objListOfEmployeeEntity = new List<object>();
            foreach (DataRowView dRow in Dv)
            {
                var hashtable = new Hashtable();
                foreach (DataColumn column in dtRecords.Columns)
                    hashtable.Add(column.ColumnName, dRow[column.ColumnName].ToString());
                objListOfEmployeeEntity.Add(hashtable);
            }

            lFinalResult = objListOfEmployeeEntity.Skip(pageIndex * rows).Take(rows);
        }
        catch (Exception ex)
        {
            SlimShared.logError(string.Format("Fields error{0}", ex.Message));
        }

        object jsonData = new { total = totalPages, page, records = totalRecords, rows = lFinalResult };
        return jsonData;
    }

    public static List<object> ConvertDataViewToList(DataView dsProducts)
    {
        var objListOfEmployeeEntity = new List<object>();
        foreach (DataRowView dRow in dsProducts)
        {
            var hashtable = new Hashtable();
            foreach (DataColumn column in dsProducts.Table.Columns)
            {
                if (column.ColumnName.IndexOf("%") == -1)
                {
                    // If column.DataType = GetType(Integer) Then
                    // hashtable.Add(column.ColumnName, Integer.Parse(dRow(column.ColumnName.ToString())))
                    // Else
                    hashtable.Add(column.ColumnName, dRow[column.ColumnName].ToString());
                    // End If
                }
            }
            objListOfEmployeeEntity.Add(hashtable);
        }
        return objListOfEmployeeEntity;

    }

    public static object ConvertDataTableToList(DataTable dtRecords, string sidx, string sord, int page, int rows)
    {

        int totalRecords = dtRecords.Rows.Count;
        int totalPages = (int)Math.Round(Math.Truncate(Math.Ceiling(totalRecords / (float)rows)));
        int pageIndex = Convert.ToInt32(page) - 1;

        var objListOfEmployeeEntity = new List<object>();
        foreach (DataRow dRow in dtRecords.Rows)
        {
            var hashtable = new Hashtable();
            foreach (DataColumn column in dtRecords.Columns)
            {
                if (column.ColumnName.IndexOf("%") == -1)
                {
                    // If column.DataType = GetType(Integer) Then
                    // hashtable.Add(column.ColumnName, Integer.Parse(dRow(column.ColumnName.ToString())))
                    // Else
                    hashtable.Add(column.ColumnName, dRow[column.ColumnName].ToString());
                    // End If
                }
            }
            objListOfEmployeeEntity.Add(hashtable);
        }


        var jsonData = new { total = totalPages, page, records = totalRecords, rows = objListOfEmployeeEntity };


        return jsonData;

    }

    internal static string EncryptURLParameters(string clearText)
    {
        try
        {
            var clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (var encryptor = new AesCryptoServiceProvider())
            {
                using (var pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }))
                {
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.FlushFinalBlock();
                        }

                        clearText = Strings.Chr(225) + Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }
        catch (Exception)
        {
            // do nothing 'HeyReggie
        }
        return clearText;
    }

    internal static string DecryptURLParameters(string cipherText)
    {
        try
        {
            if (!cipherText.StartsWith(Conversions.ToString(Strings.Chr(225))))
                return cipherText;
            cipherText = DecryptURLParameters(cipherText.Substring(1)).Replace(" ", "+");
            var cipherBytes = Convert.FromBase64String(cipherText);

            using (var encryptor = new AesCryptoServiceProvider())
            {
                using (var pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 }))
                {
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
        }
        catch (Exception)
        {
            // Return "whatever"
        }

        return cipherText;
    }

    //public static CodecsImageInfo GetCodecInfoFromFile(string fileName, string extension)
    //{
    //    try
    //    {
    //        var codec = new RasterCodecs();
    //        var info = codec.GetInformation(fileName, true);
    //        if (info.Document.IsDocumentFile)
    //            return null;
    //        if (IsAPCFile(info.Format, extension))
    //            return null;
    //        return info;
    //    }
    //    catch (Exception)
    //    {
    //        return null;
    //    }
    //}

    public static bool IsAPCFile(RasterImageFormat Format, string extension)
    {
        switch (Format)
        {
            case RasterImageFormat.RasPdf:
            case RasterImageFormat.RasPdfG31Dim:
            case var @case when @case == RasterImageFormat.RasPdfG31Dim:
            case RasterImageFormat.RasPdfG32Dim:
            case RasterImageFormat.RasPdfG4:
            case RasterImageFormat.RasPdfJbig2:
            case RasterImageFormat.RasPdfJpeg:
            case RasterImageFormat.RasPdfJpeg411:
            case RasterImageFormat.RasPdfJpeg422:
            case RasterImageFormat.RasPdfLzw:
            case RasterImageFormat.PdfLeadMrc:
            case RasterImageFormat.Eps:
            case RasterImageFormat.EpsPostscript:
            case RasterImageFormat.Postscript:
            case RasterImageFormat.RtfRaster:
            case RasterImageFormat.Unknown:
                {
                    return true;
                }
        }

        if (extension.StartsWith("."))
            extension = extension.Substring(1);

        switch (extension.ToLower() ?? "")
        {
            case "pdf":
            case "fdf":
                {
                    return true;
                }
            case "xps":
                {
                    return true;
                }

            default:
                {
                    return false;
                }
        }
    }

    // --Start MyQueries :Added by Milan Patel--------------------------------

    public static string SetMenuIds(string strMenuId)
    {
        var objMenuId = strMenuId.Split('|');
        if (objMenuId.Count() > 2)
        {
            string midPart = objMenuId[1];
            int midIntPart = int.Parse(Regex.Replace(midPart, @"[^\d]", "")) + 1;
            midPart = string.Format("{0}{1}", "AL", midIntPart);
            int midCharCount = midPart.Length + 1;

            string lastPart = objMenuId[2];
            string newLastPart = lastPart.Substring(midCharCount);
            lastPart = string.Format("{0}{1}{2}", "F", midPart, newLastPart);
            return string.Format("{0}|{1}|{2}", objMenuId[0], midPart, lastPart);
        }
        else
        {
            return strMenuId;
        }
    }
    // --End MyQueries :Added by Milan Patel--------------------------------
    // --Start Get base folder path Login warning message :Added by Nikunj Patel--------------------------------
    public static string GetBaseFolder()
    {
        try
        {
            return (string)AppDomain.CurrentDomain.GetData("ContentRootPath");
            //return web.ContentRootPath;
        }
        catch (Exception)
        {
            //if (HttpContext.Current is not null && HttpContext.Current.Request is not null)
            //{
            //    return HttpContext.Current.Request.PhysicalApplicationPath;
            //}
            return ".";
        }
    }
    // --End Login warning message :Added by Nikunj Patel--------------------------------

    // Validate SQL Statement
    public static string ValidateSQLStatement(string SQLString, TabFusionRMS.Models.Table oTable, IQueryable<Databas> lDatabaseEntities, [Optional, DefaultParameterValue(0)] ref int lError)
    {
        string sReturnErrorMessage = string.Empty;
        string sReturnMessage = string.Empty;
        try
        {
            SQLString = DataServices.NormalizeString(SQLString);

            var sADOConn = DataServices.DBOpen();
            var rsADO = new ADODB.Recordset();
            rsADO =  DataServices.GetADORecordset(SQLString, oTable, lDatabaseEntities, bReturnError: true, lError: ref lError, sErrorMessage: ref sReturnMessage);

            if (lError == 0)
            {
                sReturnErrorMessage = string.Empty;
            }
            else
            {
                // sReturnErrorMessage = String.Format("{0} {1} {1} {2}", Languages.Translation("msgAdminCtrlSQLStatementInValid"), vbCrLf, sReturnMessage)
                sReturnErrorMessage = sReturnMessage;
            }
            sADOConn.Close();
        }
        catch (Exception ex)
        {
            throw ex.InnerException;
        }

        return sReturnErrorMessage;
    }

    public static Uri GetAbsoluteUri(HttpContext httpContext)
    {
        var request = httpContext.Request;
        UriBuilder uriBuilder = new UriBuilder();
        uriBuilder.Scheme = request.Scheme;
        uriBuilder.Host = request.Host.Host;
        uriBuilder.Path = request.Path.ToString();
        uriBuilder.Query = request.QueryString.ToString();
        return uriBuilder.Uri;
    }
}

public class BaseEntity
{
    public BaseEntity()
    {
    }
    #region JavaScript Serializer
    public string ToJSON()
    {
        // var lJavaScriptSerializer = new JavaScriptSerializer();
        // return lJavaScriptSerializer.Serialize(this);
        string lJSON = JsonConvert.SerializeObject(this, new Newtonsoft.Json.Converters.IsoDateTimeConverter());
        return lJSON;
    }

    public object FromJSON(string lValue)
    {
        //var lJavaScriptSerializer = new JavaScriptSerializer();
        //lJavaScriptSerializer.Deserialize(lValue, GetType());
        return JsonConvert.DeserializeObject<object>(lValue); 
    }
    #endregion

}
// Public NotInheritable Class EntityExtensions
[HideModuleName()]
public static class EntityExtensions
{

    public static object GetJsonListForGrid<TSource>(this IQueryable<TSource> pEntityList, string pSort, int pPage, int pPageSize, string ShortPropertyName)
    {
        int pageIndex = Convert.ToInt32(pPage) - 1;
        // int pageSize = rows;
        int totalRecords = pEntityList.Count();
        int totalPages = (int)Math.Round(Math.Truncate(Math.Ceiling(totalRecords / (float)pPageSize)));

        if (pSort.ToUpper() == "DESC")
        {
            pEntityList = pEntityList.OrderByField(ShortPropertyName, false);
            pEntityList = pEntityList.Skip(pageIndex * pPageSize).Take(pPageSize);
        }
        else
        {
            pEntityList = pEntityList.OrderByField(ShortPropertyName, true);
            pEntityList = pEntityList.Skip(pageIndex * pPageSize).Take(pPageSize);
        }
        var jsonData = new
        {
            total = totalPages,
            page = pPage,
            records = totalRecords,
            rows = pEntityList
        };

        return jsonData;
    }

    public static object GetJsonListForGrid1<TSource>(this List<TSource> pEntityList, string pSort, int pPage, int pPageSize)
    {
        int pageIndex = Convert.ToInt32(pPage) - 1;
        // int pageSize = rows;
        int totalRecords = pEntityList.Count;
        int totalPages = (int)Math.Round(Math.Truncate(Math.Ceiling(totalRecords / (float)pPageSize)));

        var jsonData = new
        {
            total = totalPages,
            page = pPage,
            records = totalRecords,
            rows = pEntityList
        };

        return jsonData;
    }

    public static IQueryable<T> OrderByField<T>(this IQueryable<T> q, string SortField, bool Ascending)
    {
        var param = Expression.Parameter(typeof(T), "p");
        var prop = Expression.Property(param, SortField);
        var exp = Expression.Lambda(prop, param);
        string method = Ascending ? "OrderBy" : "OrderByDescending";
        var types = new Type[] { q.ElementType, exp.Body.Type };
        var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
        return q.Provider.CreateQuery<T>(mce);
    }

    public static SelectList CreateSelectListFromList<T>(this List<T> pEntityList, string pIdField, string pNameField, int? pIntSelectValue)
    {
        return pEntityList.CreateSelectListFromList(pIdField, pNameField, pIntSelectValue, pNameField, ListSortDirection.Ascending);
    }

    public static SelectList CreateSelectListFromList<T>(this List<T> pEntityList, string pIdField, string pNameField, int? pIntSelectValue, string pNameFieldSort, ListSortDirection pListSortDirection)
    {

        var result = (from e in pEntityList.AsEnumerable()
                      select new
                      {
                          Id = e.GetType().GetProperty(pIdField, BindingFlags.Instance | BindingFlags.Public).GetValue(e, null),
                          Name = e.GetType().GetProperty(pNameField, BindingFlags.Instance | BindingFlags.Public).GetValue(e, null),
                          Sequence = e.GetType().GetProperty(pNameFieldSort, BindingFlags.Instance | BindingFlags.Public).GetValue(e, null)
                      }).Distinct().ToList();

        if (pListSortDirection == ListSortDirection.Ascending)
        {
            result = result.OrderBy(p => p.Sequence).ToList();
        }
        else
        {
            result = result.OrderByDescending(p => p.Sequence).ToList();
        }

        var oSelectList = !pIntSelectValue.HasValue ? new SelectList(result, "Id", "Name") : new SelectList(result, "Id", "Name", pIntSelectValue.Value);
        return oSelectList;
    }

    public static SelectList CreateSelectList<T>(this List<T> pEntityList, string pIdField, string pNameField, int? pIntSelectValue) where T : BaseEntity
    {
        return pEntityList.CreateSelectList(pIdField, pNameField, pIntSelectValue, pNameField, ListSortDirection.Ascending);
    }
    public static SelectList CreateSelectList<T>(this List<T> pEntityList, string pIdField, string pNameField, int? pIntSelectValue, string pNameFieldSort, ListSortDirection pListSortDirection) where T : BaseEntity
    {

        var result = (from e in pEntityList.AsEnumerable()
                      select new
                      {
                          Id = e.GetType().GetProperty(pIdField, BindingFlags.Instance | BindingFlags.Public).GetValue(e, null),
                          Name = e.GetType().GetProperty(pNameField, BindingFlags.Instance | BindingFlags.Public).GetValue(e, null),
                          Sequence = e.GetType().GetProperty(pNameFieldSort, BindingFlags.Instance | BindingFlags.Public).GetValue(e, null)
                      }).Distinct().ToList();

        if (pListSortDirection == ListSortDirection.Ascending)
        {
            result = result.OrderBy(p => p.Sequence).ToList();
        }
        else
        {
            result = result.OrderByDescending(p => p.Sequence).ToList();
        }

        var oSelectList = !pIntSelectValue.HasValue ? new SelectList(result, "Id", "Name") : new SelectList(result, "Id", "Name", pIntSelectValue.Value);
        return oSelectList;
    }

    public static SelectList CreateSelectList<T>(this IQueryable<T> pEntityList, string pIdField, string pNameField, int? pIntSelectValue)
    {
        return pEntityList.CreateSelectList(pIdField, pNameField, pIntSelectValue, pNameField, ListSortDirection.Ascending);
    }
    public static SelectList CreateSelectList<T>(this IQueryable<T> pEntityList, string pIdField, string pNameField, int? pIntSelectValue, string pNameFieldSort, ListSortDirection pListSortDirection)
    {

        var result = (from e in pEntityList.AsEnumerable()
                      select new
                      {
                          Id = e.GetType().GetProperty(pIdField, BindingFlags.Instance | BindingFlags.Public).GetValue(e, null),
                          Name = e.GetType().GetProperty(pNameField, BindingFlags.Instance | BindingFlags.Public).GetValue(e, null),
                          Sequence = e.GetType().GetProperty(pNameFieldSort, BindingFlags.Instance | BindingFlags.Public).GetValue(e, null)
                      }).Distinct().ToList();



        if (pListSortDirection == ListSortDirection.Ascending)
        {
            result = result.OrderBy(p => p.Sequence).ToList();
        }
        else
        {
            result = result.OrderByDescending(p => p.Sequence).ToList();
        }

        var oSelectList = !pIntSelectValue.HasValue ? new SelectList(result, "Id", "Name") : new SelectList(result, "Id", "Name", pIntSelectValue.Value);
        return oSelectList;
    }

    private static StringBuilder _htmlStringBuilder = new StringBuilder();

    private static string RenderListItem(TabFusionRMS.WebCS.JSTreeView.INodeItem treeItem)
    {
        foreach (TabFusionRMS.WebCS.JSTreeView.ListItem item in treeItem.Nodes)
        {
            var listItem = item;
            var dataJsTree = listItem.DataJsTree;

            _htmlStringBuilder.AppendFormat("<li class='{0}' id='{1}' data-jstree='{6} \"opened\":{2}, \"selected\":{3}, \"disabled\":{4}, \"icon\":\"{5}\" {7}'> {8}", listItem.Class, listItem.Id, dataJsTree.Opened.ToString().ToLower(), dataJsTree.Selected.ToString().ToLower(), dataJsTree.Disabled.ToString().ToLower(), dataJsTree.Icon, "{", "}", listItem.Text);

            if (item.Nodes.Count > 0)
            {
                _htmlStringBuilder.Append("<ul>");
                EntityExtensions.RenderListItem(item);
                _htmlStringBuilder.Append("</ul>");
            }

            _htmlStringBuilder.Append("</li>");
        }

        return _htmlStringBuilder.ToString();
    }

    public static string RenderJsTreeNodes(this IHtmlHelper helper, List<TabFusionRMS.WebCS.JSTreeView.TreeNode> treeNodes)
    {
        _htmlStringBuilder = new StringBuilder();

        var stringBuilder = new StringBuilder();

        foreach (TabFusionRMS.WebCS.JSTreeView.TreeNode treeNodeItem in treeNodes)
        {
            foreach (TabFusionRMS.WebCS.JSTreeView.ListItem listItem in treeNodeItem.ListItems)
                stringBuilder.Append(EntityExtensions.RenderListItem(listItem));
        }
        return stringBuilder.ToString();
    }

    public static string Text(this object pObjValue)
    {
        try
        {
            if (object.ReferenceEquals(pObjValue, DBNull.Value))
            {
                return "";
            }
            else
            {
                return pObjValue.ToString();
            }
        }
        catch (Exception)
        {
            return "";
        }
    }

    public static int IntValue(this object pObjValue)
    {
        try
        {
            return Convert.ToInt32(pObjValue);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public static double DoubleValue(this object pObjValue)
    {
        try
        {
            return Convert.ToDouble(pObjValue);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    //public static string ToAbsoluteUrl(this string relativeUrl)
    //{
    //    if (string.IsNullOrEmpty(relativeUrl))
    //    {
    //        return relativeUrl;
    //    }

    //    if (HttpContext.Current is null)
    //    {
    //        return relativeUrl;
    //    }

    //    if (relativeUrl.StartsWith("/"))
    //    {
    //        relativeUrl = relativeUrl.Insert(0, "~");
    //    }
    //    if (!relativeUrl.StartsWith("~/"))
    //    {
    //        relativeUrl = relativeUrl.Insert(0, "~/");
    //    }

    //    var url = HttpContext.Current.Request.Url;
    //    string port = url.Port != 80 ? ":" + Convert.ToString(url.Port) : string.Empty;

    //    return string.Format("{0}://{1}{2}{3}", url.Scheme, url.Host, port, VirtualPathUtility.ToAbsolute(relativeUrl));
    //}

}

public class GridSettingsColumns
{
    public bool hidden
    {
        get
        {
            return m_hidden;
        }
        set
        {
            m_hidden = value;
        }
    }
    private bool m_hidden;
    public string index
    {
        get
        {
            return m_index;
        }
        set
        {
            m_index = value;
        }
    }
    private string m_index;
    public bool key
    {
        get
        {
            return m_key;
        }
        set
        {
            m_key = value;
        }
    }
    private bool m_key;
    public string name
    {
        get
        {
            return m_name;
        }
        set
        {
            m_name = value;
        }
    }
    private string m_name;
    public bool resizable
    {
        get
        {
            return m_resizable;
        }
        set
        {
            m_resizable = value;
        }
    }
    private bool m_resizable;
    public bool sortable
    {
        get
        {
            return m_sortable;
        }
        set
        {
            m_sortable = value;
        }
    }
    private bool m_sortable;
    public bool title
    {
        get
        {
            return m_title;
        }
        set
        {
            m_title = value;
        }
    }
    private bool m_title;
    public int width
    {
        get
        {
            return m_width;
        }
        set
        {
            m_width = value;
        }
    }
    private int m_width;
    public int widthOrg
    {
        get
        {
            return m_widthOrg;
        }
        set
        {
            m_widthOrg = value;
        }
    }
    private int m_widthOrg;
}

public class GridColumns
{
    public int ColumnSrNo
    {
        get
        {
            return _ColumnSrNo;
        }
        set
        {
            _ColumnSrNo = value;
        }
    }
    private int _ColumnSrNo;
    public int ColumnId
    {
        get
        {
            return _ColumnId;
        }
        set
        {
            _ColumnId = value;
        }
    }
    private int _ColumnId = 0;
    public string ColumnName
    {
        get
        {
            return _ColumnName;
        }
        set
        {
            _ColumnName = value;
        }
    }
    private string _ColumnName;
    public string ColumnDisplayName
    {
        get
        {
            return _ColumnDisplayName;
        }
        set
        {
            _ColumnDisplayName = value;
        }
    }
    private string _ColumnDisplayName;
    public string ColumnDataType
    {
        get
        {
            return _ColumnDataType;
        }
        set
        {
            _ColumnDataType = value;
        }
    }
    private string _ColumnDataType;
    public string ColumnMaxLength
    {
        get
        {
            return _ColumnMaxLength;
        }
        set
        {
            _ColumnMaxLength = value;
        }
    }
    private string _ColumnMaxLength;
    public bool IsPk
    {
        get
        {
            return _IsPk;
        }
        set
        {
            _IsPk = value;
        }
    }
    private bool _IsPk;
    public bool AutoInc
    {
        get
        {
            return _AutoInc;
        }
        set
        {
            _AutoInc = value;
        }
    }
    private bool _AutoInc;
    public bool IsNull
    {
        get
        {
            return _IsNull;
        }
        set
        {
            _IsNull = value;
        }
    }
    private bool _IsNull;
    public bool ReadOnlye
    {
        get
        {
            return _ReadOnlye;
        }
        set
        {
            _ReadOnlye = value;
        }
    }
    private bool _ReadOnlye;
}

public class JqGridDataHeading
{
    private string m_name;
    private string m_index;
    public string name
    {
        get
        {
            return m_name;
        }
        set
        {
            m_name = value;
        }
    }
    public string index
    {
        get
        {
            return m_index;
        }
        set
        {
            m_index = value;
        }
    }
}

public class JqGridData
{
    private int m_total;
    private int m_page;
    private int m_records;
    private List<object> m_rows;
    private List<string> m_rowshead;

    public int total
    {
        get
        {
            return m_total;
        }
        set
        {
            m_total = value;
        }
    }

    public int page
    {
        get
        {
            return m_page;
        }
        set
        {
            m_page = value;
        }
    }

    public int records
    {
        get
        {
            return m_records;
        }
        set
        {
            m_records = value;
        }
    }

    public List<object> rows
    {
        get
        {
            return m_rows;
        }
        set
        {
            m_rows = value;
        }
    }

    public List<string> rowsHead
    {
        get
        {
            return m_rowshead;
        }
        set
        {
            m_rowshead = value;
        }
    }


    public List<object> rowsM
    {
        get
        {
            return m_rowsM;
        }
        set
        {
            m_rowsM = value;
        }
    }

    private List<object> m_rowsM;
}

public class CollectionsClass
{

    public static List<string> mcEngineTablesList;
    public static List<string> mcEngineTablesOkayToImportList;
    public static List<string> mcEngineTablesNotNeededList;

    public static bool IsEngineTable(string sTableName)
    {
        int iIndex;
        int iCount = EngineTablesList.Count - 1;

        var loopTo = iCount;
        for (iIndex = 0; iIndex <= loopTo; iIndex++)
        {
            if (EngineTablesList[iIndex].Trim().ToLower().Equals(sTableName.Trim().ToLower()))
                return true;
        }
        return false;
    }
    // Added by Ganesh - 13/01/2016
    public static bool IsEngineTableOkayToImport(string sTableName)
    {
        int iIndex;
        int iCount = EngineTablesOkayToImportList.Count - 1;

        var loopTo = iCount;
        for (iIndex = 0; iIndex <= loopTo; iIndex++)
        {
            if (EngineTablesOkayToImportList[iIndex].Trim().ToLower().Equals(sTableName.Trim().ToLower()))
                return true;
        }
        return false;
    }

    public static List<string> EngineTablesList
    {
        get
        {
            if (mcEngineTablesList is null)
            {
                mcEngineTablesList = new List<string>();
                mcEngineTablesOkayToImportList = new List<string>();
                mcEngineTablesNotNeededList = new List<string>();
            }

            if (mcEngineTablesList.Count == 0)
            {
                mcEngineTablesList.Add("AddInReports");
                mcEngineTablesList.Add("Annotations");
                mcEngineTablesList.Add("AssetStatus");
                mcEngineTablesList.Add("Attributes");
                mcEngineTablesList.Add("COMMLabels");
                mcEngineTablesList.Add("COMMLabelLines");
                mcEngineTablesList.Add("CoverLetterLines");
                mcEngineTablesList.Add("CoverLetters");
                mcEngineTablesList.Add("Databases");
                mcEngineTablesList.Add("DBVersion");
                mcEngineTablesList.Add("DestCertDetail");
                mcEngineTablesList.Add("DestCerts");
                mcEngineTablesList.Add("Directories");
                mcEngineTablesList.Add("FaxAddresses");
                mcEngineTablesList.Add("FaxConfigurations");
                mcEngineTablesList.Add("FaxesInBound");
                mcEngineTablesList.Add("FaxesOutBound");
                mcEngineTablesList.Add("FieldDefinitions");
                mcEngineTablesList.Add("ImagePointers");
                mcEngineTablesList.Add("ImageTablesList");
                mcEngineTablesList.Add("ImportFields");
                mcEngineTablesList.Add("ImportJobs");
                mcEngineTablesList.Add("ImportLoads");
                mcEngineTablesList.Add("LinkScript");
                mcEngineTablesList.Add("LinkScriptFeatures");
                mcEngineTablesList.Add("LinkScriptHeader");
                mcEngineTablesList.Add("LitigationSupport");
                mcEngineTablesList.Add("LitigationTrackables");
                mcEngineTablesList.Add("OfficeDocTypes");
                mcEngineTablesList.Add("OneStripForms");
                mcEngineTablesList.Add("OneStripJobFields");
                mcEngineTablesList.Add("OneStripJobs");
                mcEngineTablesList.Add("OutputSettings");
                mcEngineTablesList.Add("PCFilesPointers");
                mcEngineTablesList.Add("RecordTypes");
                mcEngineTablesList.Add("RelationShips");
                mcEngineTablesList.Add("ReportStyles");
                mcEngineTablesList.Add("Retention");
                mcEngineTablesList.Add("RetentionLists");
                mcEngineTablesList.Add("ScanBatches");
                mcEngineTablesList.Add("ScanFormLines");
                mcEngineTablesList.Add("ScanForms");
                mcEngineTablesList.Add("ScanList");
                mcEngineTablesList.Add("ScanRules");
                mcEngineTablesList.Add("SDLKFolderTypes");
                mcEngineTablesList.Add("SDLKPullLists");
                mcEngineTablesList.Add("SDLKRequestor");
                mcEngineTablesList.Add("SDLKStatus");
                mcEngineTablesList.Add("SDLKStatusHistory");
                mcEngineTablesList.Add("Settings");
                mcEngineTablesList.Add("SLColdArchives");
                mcEngineTablesList.Add("SLColdPointers");
                mcEngineTablesList.Add("SLColdSetupCols");
                mcEngineTablesList.Add("SLColdSetupForms");
                mcEngineTablesList.Add("SLColdSetupRows");
                mcEngineTablesList.Add("SLIndexWizard");
                mcEngineTablesList.Add("SLIndexWizardCols");
                mcEngineTablesList.Add("SLPullLists");
                mcEngineTablesList.Add("SLRequestor");
                mcEngineTablesList.Add("SLRetentionCitations");
                mcEngineTablesList.Add("SLRetentionCitaCodes");
                mcEngineTablesList.Add("SLRetentionCodes");
                mcEngineTablesList.Add("SLRetentionInactive");
                mcEngineTablesList.Add("SLDestructionCerts");
                mcEngineTablesList.Add("SLDestructCertItems");
                mcEngineTablesList.Add("SLTableFileRoomOrder");
                mcEngineTablesList.Add("SLTrackingSelectData");
                mcEngineTablesList.Add("SLAuditConfData");
                mcEngineTablesList.Add("SLAuditFailedLogins");
                mcEngineTablesList.Add("SLAuditLogins");
                mcEngineTablesList.Add("SLAuditUpdChildren");
                mcEngineTablesList.Add("SLAuditUpdates");
                mcEngineTablesList.Add("SLBatchRequests");
                mcEngineTablesList.Add("SysNextTrackable");
                mcEngineTablesList.Add("System");
                mcEngineTablesList.Add("SystemAddresses");
                mcEngineTablesList.Add("Tables");
                mcEngineTablesList.Add("Tabletabs");
                mcEngineTablesList.Add("Tabsets");
                mcEngineTablesList.Add("TrackableHeaders");
                mcEngineTablesList.Add("Trackables");
                mcEngineTablesList.Add("TrackingHistory");
                mcEngineTablesList.Add("TrackingStatus");
                mcEngineTablesList.Add("Userlinks");
                mcEngineTablesList.Add("ViewColumns");
                mcEngineTablesList.Add("ViewFilters");
                mcEngineTablesList.Add("ViewParms");
                mcEngineTablesList.Add("Views");
                mcEngineTablesList.Add("Volumes");
                mcEngineTablesList.Add("SLTextSearchItems");
                mcEngineTablesList.Add("SLGrabberFunctions");
                mcEngineTablesList.Add("SLGrabberControls");
                mcEngineTablesList.Add("SLGrabberFields");
                mcEngineTablesList.Add("SLGrabberFldParts");
                mcEngineTablesList.Add("SLServiceTasks");
                mcEngineTablesList.Add("SLIndexer");
                mcEngineTablesList.Add("SLIndexerCache");
                mcEngineTablesList.Add("SLCollections");
                mcEngineTablesList.Add("SLCollectionItems");
                mcEngineTablesList.Add("SLSignature");
                mcEngineTablesList.Add("SecureGroup");
                mcEngineTablesList.Add("SecureObject");
                mcEngineTablesList.Add("SecureObjectPermission");
                mcEngineTablesList.Add("SecurePermission");
                mcEngineTablesList.Add("SecureUser");
                mcEngineTablesList.Add("SecureUserGroup");
                mcEngineTablesList.Add("SecurePermissionDescription");
                mcEngineTablesList.Add("LookupType");
                mcEngineTablesList.Add("GridSettings");
                mcEngineTablesList.Add("GridColumn");
                mcEngineTablesList.Add("MobileDetails");
                // Add ViewTable in list
                mcEngineTablesList.Add("vwColumnsAll");
                mcEngineTablesList.Add("vwGetOutputSetting");
                mcEngineTablesList.Add("vwGridSettings");
                mcEngineTablesList.Add("vwTablesAll");
                // new system tables for 10.1.x  RVW 09/10/2017
                mcEngineTablesList.Add("s_SavedCriteria");
                mcEngineTablesList.Add("s_SavedChildrenQuery");
                mcEngineTablesList.Add("s_SavedChildrenFavorite");
                // new system tables for 10.1.x  RVW 02/15/2018
                mcEngineTablesList.Add("SLServiceTaskItems");
                // new system tables for 10.2.x  RVW 03/08/2019
                mcEngineTablesList.Add("s_AttachmentCart");
                // new system tables for 11.0.x  RVW 09/22/2021
                mcEngineTablesList.Add("SLUserDashboard");

                mcEngineTablesOkayToImportList.Add("slretentioncodes");
                mcEngineTablesOkayToImportList.Add("slretentioncitations");
                mcEngineTablesOkayToImportList.Add("slretentioncitacodes");

                mcEngineTablesNotNeededList.Add("coverletterlines");
                mcEngineTablesNotNeededList.Add("coverletters");
                mcEngineTablesNotNeededList.Add("destcertdetail");
                mcEngineTablesNotNeededList.Add("destcerts");
                mcEngineTablesNotNeededList.Add("faxaddresses");
                mcEngineTablesNotNeededList.Add("faxconfigurations");
                mcEngineTablesNotNeededList.Add("faxesinbound");
                mcEngineTablesNotNeededList.Add("faxesoutbound");
                mcEngineTablesNotNeededList.Add("fielddefinitions");
                mcEngineTablesNotNeededList.Add("litigationsupport");
                mcEngineTablesNotNeededList.Add("litigationtrackables");
                mcEngineTablesNotNeededList.Add("retention");
                mcEngineTablesNotNeededList.Add("retentionlists");
                mcEngineTablesNotNeededList.Add("scanformlines");
                mcEngineTablesNotNeededList.Add("scanforms");
                mcEngineTablesNotNeededList.Add("sdlkfoldertypes");
                mcEngineTablesNotNeededList.Add("sdlkpulllists");
                mcEngineTablesNotNeededList.Add("sdlkrequestor");
                mcEngineTablesNotNeededList.Add("sdlkstatus");
                mcEngineTablesNotNeededList.Add("sdlkstatushistory");
                mcEngineTablesNotNeededList.Add("trackableheaders");
                mcEngineTablesNotNeededList.Add("viewparms");
                mcEngineTablesNotNeededList.Add("slloggedinusers");
                mcEngineTablesNotNeededList.Add("devicetype");
                mcEngineTablesNotNeededList.Add("members");
                mcEngineTablesNotNeededList.Add("securitygroups");
                mcEngineTablesNotNeededList.Add("operators");
                mcEngineTablesNotNeededList.Add("operators_back");
            }

            return mcEngineTablesList;
        }
    }

    public static List<string> EngineTablesOkayToImportList
    {
        get
        {
            if (mcEngineTablesOkayToImportList is null)
            {
                mcEngineTablesOkayToImportList = new List<string>();
            }

            if (mcEngineTablesOkayToImportList.Count == 0)
            {
                mcEngineTablesOkayToImportList.Add("slretentioncodes");
                mcEngineTablesOkayToImportList.Add("slretentioncitations");
                mcEngineTablesOkayToImportList.Add("slretentioncitacodes");
            }

            return mcEngineTablesOkayToImportList;
        }

    }

    public static List<string> EngineTablesNotNeededList
    {
        get
        {
            if (mcEngineTablesNotNeededList is null)
            {
                mcEngineTablesNotNeededList = new List<string>();
            }

            if (mcEngineTablesNotNeededList.Count == 0)
            {
                mcEngineTablesNotNeededList.Add("coverletterlines");
                mcEngineTablesNotNeededList.Add("coverletters");
                mcEngineTablesNotNeededList.Add("destcertdetail");
                mcEngineTablesNotNeededList.Add("destcerts");
                mcEngineTablesNotNeededList.Add("faxaddresses");
                mcEngineTablesNotNeededList.Add("faxconfigurations");
                mcEngineTablesNotNeededList.Add("faxesinbound");
                mcEngineTablesNotNeededList.Add("faxesoutbound");
                mcEngineTablesNotNeededList.Add("fielddefinitions");
                mcEngineTablesNotNeededList.Add("litigationsupport");
                mcEngineTablesNotNeededList.Add("litigationtrackables");
                mcEngineTablesNotNeededList.Add("retention");
                mcEngineTablesNotNeededList.Add("retentionlists");
                mcEngineTablesNotNeededList.Add("scanformlines");
                mcEngineTablesNotNeededList.Add("scanforms");
                mcEngineTablesNotNeededList.Add("sdlkfoldertypes");
                mcEngineTablesNotNeededList.Add("sdlkpulllists");
                mcEngineTablesNotNeededList.Add("sdlkrequestor");
                mcEngineTablesNotNeededList.Add("sdlkstatus");
                mcEngineTablesNotNeededList.Add("sdlkstatushistory");
                mcEngineTablesNotNeededList.Add("trackableheaders");
                mcEngineTablesNotNeededList.Add("viewparms");
                mcEngineTablesNotNeededList.Add("slloggedinusers");
                mcEngineTablesNotNeededList.Add("devicetype");
                mcEngineTablesNotNeededList.Add("members");
                mcEngineTablesNotNeededList.Add("securitygroups");
                mcEngineTablesNotNeededList.Add("operators");
                mcEngineTablesNotNeededList.Add("operators_back");
            }

            return mcEngineTablesNotNeededList;
        }

    }

    public static bool IsAuditingEnabled(string tableName)
    {
        IRepository<Table> oTable = new Repositories<Table>();
        var OTableRow = oTable.Where(x => x.TableName == tableName).FirstOrDefault();
        if (OTableRow.AuditUpdate == true || OTableRow.AuditConfidentialData == true || OTableRow.AuditAttachments == true)
        {
            return true;
        }
        return false;
    }

    // Check for Tables if any table has permission of Configure for a user.
    public static bool CheckTablesPermission(IQueryable<Table> lTableEntities, bool mbMgrGroup, Passport passport, HttpContext http)
    {
        bool bAtLeastOneTablePermission = false;
        http.Session.Remove("TablesPermission");
        try
        {
            if (!string.IsNullOrEmpty(http.Session.GetString("TablesPermission")))
            {
                if (!mbMgrGroup)
                {
                    foreach (var oTable in lTableEntities)
                    {
                        if (!IsEngineTable(oTable.TableName))
                        {
                            if (passport.CheckPermission(oTable.TableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Configure))
                            {
                                bAtLeastOneTablePermission = true;
                                break;
                            }
                        }
                    }
                }
                http.Session.SetString("TablesPermission", bAtLeastOneTablePermission.ToString());
            }
            else
            {
                if (string.IsNullOrEmpty(http.Session.GetString("TablesPermission")))
                {
                    bAtLeastOneTablePermission = false;
                }
                //bAtLeastOneTablePermission = Conversions.ToBoolean(http.Session.GetString("TablesPermission"));
            }
        }
        catch (Exception)
        {

        }

        return bAtLeastOneTablePermission;
    }


    //// Check for Views if any view has permission of Configure for a user.
    public static bool CheckViewsPermission(IQueryable<View> lViewEntities, bool mbMgrGroup, Passport passport, HttpContext httpContext)
    {
        bool bAtLeastOneViewPermission = false;
        httpContext.Session.Remove("ViewPermission");
        try
        {
            if (!string.IsNullOrEmpty(httpContext.Session.GetString("ViewPermission")))
            {
                if (!mbMgrGroup)
                {
                    foreach (var oView in lViewEntities)
                    {
                        if (passport.CheckPermission(oView.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.View, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Configure))
                        {
                            bAtLeastOneViewPermission = true;
                            break;
                        }
                    }
                }

                httpContext.Session.SetString("TablesPermission", bAtLeastOneViewPermission.ToString());
            }
            else
            {
                if (string.IsNullOrEmpty(httpContext.Session.GetString("TablesPermission")))
                {
                    bAtLeastOneViewPermission = false;
                }
                else
                {
                    bAtLeastOneViewPermission = Conversions.ToBoolean(httpContext.Session.GetString("TablesPermission"));
                }
                
            }
        }
        catch (Exception)
        {

        }

        return bAtLeastOneViewPermission;
    }
    //// Check for Reports if any Report has permission access for a user in group
    public static int CheckReportsPermission(IQueryable<Table> lTableEntities, IQueryable<View> lViewEntities, Passport passport, HttpContext httpContext)
    {
        int iCntRpts = 0;
        httpContext.Session.Remove("iCntRpts");
        var iCntRpt = httpContext.Session.GetString("iCntRpts");
        if (!string.IsNullOrEmpty(iCntRpt))
        {
            foreach (var oTable in lTableEntities)
            {
                if (!IsEngineTable(oTable.TableName))
                {
                    if (passport.CheckPermission(oTable.TableName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Table, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.View))
                    {
                        var lTableViewList = lViewEntities.Where(x => (x.TableName.Trim().ToLower() ?? "") == (oTable.TableName.Trim().ToLower() ?? ""));
                        foreach (var oView in lTableViewList)
                        {
                            if ((bool)oView.Printable)
                            {
                                if (passport.CheckPermission(oView.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Reports, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.Configure))
                                {
                                    if (passport.CheckPermission(oView.ViewName, (Smead.Security.SecureObject.SecureObjectType)Enums.SecureObjects.Reports, (Smead.Security.Permissions.Permission)Enums.PassportPermissions.View))
                                    {
                                        if (NotSubReport(lTableEntities, lViewEntities, oView, oTable.TableName))
                                        {
                                            iCntRpts = iCntRpts + 1;
                                            httpContext.Session.SetString("iCntRpts", iCntRpts.ToString());
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (iCntRpts > 0)
                            break;
                    }
                }
            }
        }
        return Convert.ToInt32(httpContext.Session.GetString("iCntRpts"));
    }

    public static bool NotSubReport(IQueryable<Table> lTableEntities, IQueryable<View> IqViewEntities, View oView, string pTableName)
    {
        bool NotSubReportRet = default;
        var tableEntity = lTableEntities;
        View oTempView;
        var lViewEntities = IqViewEntities;
        var lLoopViewEntities = lViewEntities.Where(x => x.TableName.Trim().ToLower() == pTableName);

        NotSubReportRet = true;

        foreach (var oTable in tableEntity.ToList())
        {
            foreach (var currentOTempView in lLoopViewEntities)
            {
                oTempView = currentOTempView;
                if (oTempView.SubViewId == oView.Id)
                {
                    NotSubReportRet = false;
                    break;
                }
            }
        }

        oTempView = null;
        return NotSubReportRet;
    }

    public static void ReloadPermissionDataSet(Passport passport)
    {
        // Passport Get Global 
        //var passport = ContextService.GetObjectFromJson<Passport>("passport");
        passport.FillSecurePermissions();
    }
}

public class UserPreferences
{
    private string p_sPreferedLanguage;
    private string p_sPreferedDateFormat;

    public string sPreferedLanguage
    {
        get
        {
            return p_sPreferedLanguage;
        }
        set
        {
            p_sPreferedLanguage = value;
        }
    }

    public string sPreferedDateFormat
    {
        get
        {
            return p_sPreferedDateFormat;
        }
        set
        {
            p_sPreferedDateFormat = value;
        }
    }
}

public static class TempDataExtensions
{
    public static void Set<T>(this ITempDataDictionary tempData, string key, T value)
    {
        string json = JsonConvert.SerializeObject(value);
        if (tempData.ContainsKey(key))
        {
            tempData[key] = json;
        }
        else
        {
            tempData.Add(key, json);
        }
    }

    public static T Get<T>(this ITempDataDictionary tempData, string key)
    {
        if (!tempData.ContainsKey(key)) return default(T);

        var value = tempData[key] as string;

        return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }

    public static T GetPeek<T>(this ITempDataDictionary tempData, string key)
    {
        
        if (!tempData.ContainsKey(key)) return default(T);

        var value = tempData.Peek(key) as string;

        return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
    }

}

