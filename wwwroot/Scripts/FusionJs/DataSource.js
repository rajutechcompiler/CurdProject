var app = {
    data: {
        ChildKeyname: "string",
        HasAttachmentcolumn: "bool",
        HasDrillDowncolumn: "bool",
        IdFieldDataType: "string",
        ItemDescription: "string",
        ListOfAttachmentLinks: "array",
        ListOfBreadCrumbs: "array",
        ListOfBreadCrumbsRightClick: {
            viewId: "integer",
            viewName: "string"
        },
        ListOfDatarows: "array",
        ListOfHeaders: [{
            Allownull: "bool",
            ColumnName: "string",
            DataType: "string",
            DataTypeFullName: "string",
            HeaderName: "string",
            IsPrimarykey: "bool",
            Issort: "bool",
            MaxLength: "integer",
            columnOrder: "integer",
            editMask: "string",
            isDropdown: "bool",
            isEditable: "bool"
        }],
        ListOfdrilldownLinks: "string",
        ListOfdropdownColumns: [{
            colorder: "integer",
            display: "string",
            value: "string"
        }],
        ListofEditableHeader: [{
            Allownull: "bool",
            ColumnName: "string",
            DataType: "string",
            DataTypeFullName: "string",
            HeaderName: "string",
            IsPrimarykey: "bool",
            Issort: "bool",
            MaxLength: "integer",
            columnOrder: "integer",
            editMask: "string",
            isDropdown: "bool",
            isEditable: "bool"
        }],
        Message: "string",
        Msg: "string",
        PageNumber: "integer",
        RightClickToolBar: "object",
        RowPerPage: "integer",
        ShowTrackableTable: "bool",
        TableName: "string",
        TesterErrMsg: "string",
        ToolBarHtml: "string",
        TotalPagesNumber: "integer",
        TotalRows: "integer",
        ViewId: "integer",
        ViewName: "string",
        crumbLevel: "integer",
        dateFormat: "string",
        errorNumber: "integer",
        fvList: "array",
        hasPermission: "bool",
        isError: "bool",
        sortableFields: []
    },
    language: "Language data comes from server and build object in _LayoutData partial view",
    url: {
        server: {
            DataGridView: "/Data/DataGridView",
            NewFavorite: "/Data/NewFavorite",
            AddNewFavorite: "/Data/AddNewFavorite",
            LoadQueryWindow: "/Data/LoadQueryWindow",
            LoadquerytoGrid: "/Data/LoadquerytoGrid",
            RunQuery: "/Data/RunQuery",
            GetTrackbaleDataPerRow: "/Data/GetTrackbaleDataPerRow",
            ReturnFavoritTogrid: "/Data/ReturnFavoritTogrid",
            DeleteFavorite: "/Data/DeleteFavorite",
            LinkscriptButtonClick: "/Data/LinkscriptButtonClick",
            LinkscriptEvents: "/Data/LinkscriptEvents",
            FlowButtonsClickEvent: "/Data/FlowButtonsClickEvent",
            SaveNewsURL: "/Data/SaveNewsURL",
            AddToFavorite: "/Data/AddToFavorite",
            StartDialogAddToFavorite: "/data/StartDialogAddToFavorite",
            CheckBeforeAddTofavorite: "/Data/CheckBeforeAddTofavorite",
            UpdateFavorite: "/Data/UpdateFavorite",
            DeleteFavoriteRecord: "/Data/DeleteFavoriteRecord",
            SaveNewQuery: "/Data/SaveNewQuery",
            LoadControls: "/DataToolsLayout/LoadControls",
            BtnSaveLocData_Click: "/DataToolsLayout/btnSaveLocData_Click",
            ChangePassword_click: "/DataToolsLayout/ChangePassword_click",
            DeleteQuery: "/Data/DeleteQuery",
            UpdateQuery: "/Data/UpdateQuery",
            RunglobalSearch: "/Data/RunglobalSearch",
            GlobalSearchClick: "/Data/GlobalSearchClick",
            GlobalSearchAllClick: "/Data/GlobalSearchAllClick",
            SetDatabaseChanges: "/Data/SetDatabaseChanges",
            CheckForduplicateId: "/Data/CheckForduplicateId",
            CheckForduplicateId: "/Data/CheckForduplicateId",
            DeleteRowsFromGrid: "/Data/DeleteRowsFromGrid",
            Drilldown: "/Data/Drilldown",
            BreadCrumbClick: "/Data/BreadCrumbClick",
            LoadFlyoutPartial: "/Common/LoadFlyoutPartial",
            LazyLoadPopupAttachments: "../Common/LazyLoadPopupAttachments",
            AddNewAttachment: "/Common/AddNewAttachment",
            TaskBarClick: "/Data/TaskBarClick",
            Reporting: "/data/Reporting",
            ReportingCount: "/data/ReportingCount",
            GetTotalrowsForGrid: "/data/GetTotalrowsForGrid",
            GetauditReportView: "/data/GetauditReportView",
            RunAuditSearch: "/data/RunAuditSearch",
            GetRetentionInfo: "/data/GetRetentionInfo",
            OnDropdownChange: "/data/OnDropdownChange",
            RetentionInfoUpdate: "/data/RetentionInfoUpdate",
            RemoveRetentioninfoRows: "/data/RemoveRetentioninfoRows",
            RetentionInfoHolde: "/data/RetentionInfoHolde",
            DialogConfirmExportCSVorTXT: "/Exporter/DialogConfirmExportCSVorTXT",
            ExportCSVOrTXT: "/Exporter/ExportCSVOrTXT",
            ExportCSVorTXTAll: "/Exporter/ExportCSVorTXTAll",
            BackGroundProcessing: "/Exporter/BackGroundProcessing",
            DialogMsgConfirmDelete: "/data/DialogMsgConfirmDelete",
            InitiateBarcodePopup: "/PrintBarcode/InitiateBarcodePopup",
            SetDefaultBarcodeForm: "/PrintBarcode/SetDefaultBarcodeForm",
            GenerateBarcodeOnchange: "/PrintBarcode/GenerateBarcodeOnchange",
            DeleteLabelsFiles: "/PrintBarcode/DeleteLabelsFiles",
            OrphanPartial: "/Vault/OrphanPartial",
            LoadFlyoutOrphansPartial: "/Vault/LoadFlyoutOrphansPartial",
            LinkOrphanAttachment: "/Vault/LinkOrphanAttachment",
            DeleteAttachments: "/Vault/DeleteAttachments",
            GetCountAfterFilter: "/Vault/GetCountAfterFilter",
            GetMovePopup: "/MoveRows/GetMovePopup",
            LoadDestinationItems: "/MoveRows/LoadDestinationItems",
            FilterItemsList: "/MoveRows/FilterItemsList",
            BtnMoveItems: "/MoveRows/BtnMoveItems",
            CheckDownloadable: "/Vault/CheckDownloadable",
            StartTransfering: "/Transfer/StartTransfering",
            GetTransferItems: "/Transfer/GetTransferItems",
            BtnTransfer: "/Transfer/BtnTransfer",
            CountAllTransferData: "/Transfer/CountAllTransferData",
            GetRequsterpopup: "/Requester/GetRequsterpopup",
            SearchEmployees: "/Requester/SearchEmployees",
            SubmitRequest: "/Requester/SubmitRequest",
            DeleteTrackingRequest: "/Requester/DeleteTrackingRequest",
            GetRequestDetails: "/Requester/GetRequestDetails",
            GetImportFavoritpopup: "/ImportFavorite/GetImportFavoritpopup",
            UploadingFile: "/ImportFavorite/UploadingFile",
            ImportFavoriteDDLChange: "/ImportFavorite/ImportFavoriteDDLChange",
            BtnImportfavorite: "/ImportFavorite/BtnImportfavorite",
            DialogConfirmExportCSVorTXTGridReport: "/Exporter/DialogConfirmExportCSVorTXTReport",
            ExportCSVOrTXTGridReport: "/Exporter/ExportCSVOrTXTReport",
            LoadNewRecordForm: "/data/LoadNewRecordForm",
            CheckAttachmentMaxSize: "/Common/CheckAttachmentMaxSize",
            UpdateRequest: "/Requester/UpdateRequest"
        },
        Html: {
            DialogMsghtml: "ViewsHtml/DialogMsg.html",
            DialogMsgDeleteFavoriteConfirm: "ViewsHtml/DialogMsgDeleteFavoriteConfirm.html",
            DialogMsgConfirmSaveNewrow: "ViewsHtml/DialogMsgConfirmSaveNewrow.html",
            Reporting: "ViewsHtml/Reporting.html",
            //Addnewrow: "ViewsHtml/AddNewRow.html"
        }
    },
    domMap: {
        ToolBarButtons: {
            BtnDeleterow: "#btndeleterow",
            BtnTabquikId: "#tabquikId",
            BtnPrint: "#btnPrint",
            BtnBlackWhite: "#btnBlackWhite",
            BtnExportCSVSelected: "#btnExportCSV",
            BtnExportCSVAll: "#btnExportCSVAll",
            BtnExportTXTSelected: "#btnExportTXT",
            BtnExportTXTAll: "#btnExportTXTAll",
            BtnRequest: "#btnRequest",
            BtnTransfer: "#btnTransfer",
            BtnTransferAll: "#btnTransferAll",
            BtnMoverows: "#btnMoverows",
            BtnTracking: "#btnTracking",
            BtnDeleteCeriteria: "#btnDeleteCeriteria",
            BtnNewFavorite: "#btnAddFavourite",
            BtnUpdateFavourite: "#btnUpdateFavourite",
            BtnremoveFromFavorite: "#removeFromFavorite",
            BtnImportFavourite: "#btnImportFavourite",
            BtnQuery: "#btnQuery",
            BtnSaveRow: "#saveRow",
            DivFavOptions: "#divFavOptions",
            BtnImportFavourite: "#btnImportFavourite",
            btnExportCSVGridReport: "#btnExportCSVGridReport",
        },
        DataGrid: {
            ContentWrapper: "#page-content-wrapper",
            ToolsBarDiv: "#ToolBar",
            MainDataContainerDiv: "#mainDataContainer",
            TrackingStatusDiv: "#TrackingStatusDiv",
            HandsOnTableContainer: "#handsOnTableContainer",
            Paging: "#paging"
        },
        NewsFeed: {
            BtnSaveNewsURL: "#btnSaveNewsURL",
            TabNewsTable: "#TabNewsTable",
            TxtNewsUrl: "#txtNewsURL",
            NewsFrame: "#NewsFrame",
            MainDataContainer: "#mainDataContainer",
            IsTabfeed: "#isTabfeed",
            FooterTask: "#FooterTask",
            FooterlblAttampt: "#lblAttempt",
            FooterlblService: "#lblService"
        },
        Dialogboxbasic: {
            BasicDialog: "#BasicDialog",
            DlgBsContent: "#dlgBsContent",
            DlgBsTitle: "#dlgBsTitle",
            Dlg: {
                DialogMsg: {
                    DialogMsgTxt: "#dialogMsgTxt"
                },
                DialogMsgConfirm: {
                    DialogMsgTxt: "#dialogMsgTxt",
                    DialogYes: "#dialogYes",
                    ListItem: "#listItem"
                },
            },
            Type: {
                PartialView: "PartialView",
                Content: "Content"
            },
            DlgbodyId: "#dlgbodyId",
            BtnAddNewRow: "#BtnAddNewRow" //just for addnewrow
        },
        DialogQuery: {
            MainDialogQuery: "#MainDialogQuery",
            QuerylblTitle: "#QuerylblTitle",
            QuerySaveInput: "#QuerySaveInput",
            ChekBasicQuery: "#chekBasicQuery",
            OkQuery: "#OkQuery",
            BtnCancel: "#btnCancel",
            BtnQueryApply: "#btnQueryApply",
            BtnSaveQuery: "#btnSaveQuery",
            QueryContentDialog: "#QueryContentDialog",
            Querytableid: "#querytableid",
            UpdateQuery: "#updateQuery"
        },
        DialogRecord: {
            DialogRecordSaveId: "#DialogRecordSaveId",
            DialogRecordMsg: "#DialogRecordMsg"
        },
        DialogAttachment: {
            Openformattachment: "#dialog-form-attachment",
            AttachmentModalBody: "#AttachmentModalBody",
            ThumbnailDetails: "#ThumbnailDetails",
            BtnAddAttachment: "#ModalAddAttachment",
            BtnopenAttachViewer: "#openAttachViewer",
            FileUploadForAddAttach: "#FileUploadForAddAttach",
            GotoScanner: "#gotoScanner",
            StringQuery: "#stringQuery",
            EmptydropDiv: "#emptydrop",
            StringEncript: "#stringEncript",
            paging: {
                ListThumbnailDetailsImg: "#ThumbnailDetails img",
                TotalRecCount: "#totalRecCount",
                ResultDisplay: "#resultDisplay",
                SPageIndex: "#sPageIndex",
                SPageSize: "#sPageSize"
            }
        },
        Favorite: {
            NewFavorite: {
                Ok: "#favoriveOk",
                ErrorMsg: "#Dialog_Favourites_lblError",
                FavNewInputBox: "#newfavinput"
            },
            AddToFavorite: {
                AddtofavoriteOK: "#AddtofavoriteOK",
                DDLfavorite: "#DDLfavorite"
            },
            RemoveFavorite: {
                RemoveConfimed: "#removeRecord"
            },
            ImportFavorite: {
                fileSource: "#fileSource",
                InputFileName: "#InputFileName",
                btnImportFavRecordOk: "#btnImportFavRecordOk",
                ddlFavoriteList: "#ddlFavoriteList",
                listoffieldsbody: "#listoffieldsbody",
                chk1RowFieldNames: "#chk1RowFieldNames",
                rblSourceCol1: "#rblSourceCol1",
                ImportFavoritesAReport: "#ImportFavoritesAReport"
            }
        },
        Layout: {
            MenuNavigation: {
                LeftSideMenuContainer: "#mCSB_1_container",
                MyQureyClickMenu: "#MyQueryClickMenu",
                MyFavClickMenu: "#MyfavClickMenu",
                GoHome: "goHome",
                goUp: "goUp",
                ToolsDialog: "#ToolsDialog",
                ScrollMenu: "#mCSB_1_dragger_vertical"
            }
        },
        Myqury: {
            BtnDeleteMyquery: "#btnDeletemyquery",
        },
        GlobalSearching: {
            ChkAttachments: "#chkAttachments",
            ChkCurrentTable: "#chkCurrentTable",
            ChkUnderRow: "#chkUnderRow",
            TxtSearch: "#txtSearch",
            BtnSearch: "#btnSearch",
            DialogSearchInput: "#DialogSearchInput",
            DialogchkAttachments: "#DialogchkAttachments",
            DialogCurrenttable: "#DialogCurrenttable",
            DialogUnderthisrow: "#DialogUnderthisrow",
            DialogSearchButton: "#dialogSearchButton"
        },
        Breadcrumb: {
            olContainer: "#breadcrumbs"
        },
        Reporting: {
            ReportTitle: "#ReportTitle",
            PrintReport: "#printReport",
            itemDescription: "#itemDescription",
            HandsOnTableContainer: "#handsOnTableContainer",
            ShadowBox: "#shadowBox",
            Paging: {
            }
        },
        AuditReport: {
            ddlUser: "#ddlUser",
            ddlObject: "#ddlObject",
            txtObjectId: "#txtObjectId",
            dtStartDate: "#dtStartDate",
            dtEndDate: "#dtEndDate",
            chkSuccessLogin: "#chkSuccessLogin",
            chkAddEditDelL: "#chkAddEditDel",
            chkFailedLogin: "#chkFailedLogin",
            chkConfidential: "#chkConfidential",
            chkChildTable: "#chkChildTable",
            BtnauditRepoOk: "#auditReportOk",
            CheckboxMsg: "#CheckboxMsg"
        },
        Retentioninfo: {
            ddlRetentionCode: "#ddlRetentionCode",
            RetinCodeDesc: "#RetinCodeDesc",
            RetinItemName: "#RetinItemName",
            RetinStatus: "#RetinStatus",
            RetinInactiveDate: "#RetinInactiveDate",
            lblRetinArchive: "#lblRetinArchive",
            RetinArchive: "#RetinArchive",
            btnAddRetin: "#btnAddRetin",
            btnEditRetin: "#btnEditRetin",
            btnRemoveRetin: "#btnRemoveRetin",
            btnOkRetin: "#btnOkRetin",
            btnCancelRetin: "#btnCancelRetin",
            handsOnTableRetinfo: "#handsOnTableRetinfo"
        },
        RetentioninfoHold: {
            chkHoldTypeRetention: "#chkHoldTypeRetention",
            chkHoldTypeLegal: "#chkHoldTypeLegal",
            holdReason: "#holdReason",
            btnSnooze: "#btnSnooze",
            btnHoldOk: "#btnHoldOk",
            btnAddHoldeCancel: "#btnAddHoldeCancel",
            txtSnoozeDate: "#txtSnoozeDate",
            Dialog: {
                RetentionHoldingDialog: "#RetentionHoldingDialog",
                RetentionHoldingContent: "#RetentionHoldingContent",
                DlgHoldingTitle: "#DlgHoldingTitle"
            }
        },
        ExportFiles: {
            ExportSelectedToCSVorTXT: "#ExportToCSVorTXT",
            ExportToCSVorTXTAll: "#ExportToCSVorTXTAll",
            ExportSelectedToCSVorTXTGridReport: "#ExportToCSVorTXTGridReport"
        },
        PrintBarcod: {
            labelDesign: "#labelDesign",
            labelForm: "#labelForm",
            setDefault: "#setDefault",
            BarcodeBtnUp: "#BarcodeBtnUp",
            BarcodeBtnDown: "#BarcodeBtnDown",
            totalLabels: "#totalLabels",
            totalPages: "#totalPages",
            totalSkipped: "#totalSkipped",
            labelOutline: "#labelOutline",
            previewData: "#previewData",
            previewContainer: "#previewContainer",
            strtPrinting: "#strtPrinting"
        },
        MovingRows: {
            rblDestinationItemsContainer: "#rblDestinationItemsContainer",
            rblDestinationTableContainer: "#rblDestinationTableContainer",
            scroller: "#scrolldiv",
            txtFilter: "#txtFilter",
            btnMove: "#btnMove"
        },
        Transfer: {
            TransferItems: "#TransferItems",
            txtDueBack: "#txtDueBack",
            DivDueBack: "#DivDueBack",
            tstxtFilter: "#tstxtFilter",
            rblLocationType: "#rblLocationType",
            tslLocations: "#tslLocations",
            lblError: "#lblError",
            btnTransferItems: "#btnTransferItems",
            btnCancel: "#btnCancel",
            lsTaransferTypeTableContainer: "#lsTaransferTypeTableContainer",
            trlLocationType: "#trlLocationType",
            trlDestinationItemsContainer: "#trlDestinationItemsContainer",
            tsscrolldiv: "#tsscrolldiv",
            lblError: "#lblError",
            lblTransfer: "#lblTransfer"
        },
        Request: {
            txtreqDue: "#txtreqDue",
            txtInstructions: "#txtInstructions",
            reqtxtFilter: "#reqtxtFilter",
            btnSubmitRequest: "#btnSubmitRequest",
            employeeContainer: "#employeeContainer",
            emscrolldiv: "#emscrolldiv",
            chkWaitList: "#chkWaitList"
        },
        Autosave: {
            dialogNo: "#dialogNo",
            saverowdialogYes: "#saverowdialogYes",
            autosavebtn: "#autosavebtn"
        },
        Paging: {
            pageCurrentNumber: "#pageCurrentNumber"
        }
    },
    //ajax: {
    //    Type: {
    //        Post: "POST",
    //        Get: "GET",
    //        Put: "PUT",
    //        Delete: "DELETE"
    //    },
    //    DataType: {
    //        Json: "json",
    //        Html: "html",
    //        Text: "text",
    //        String: "string",
    //        TextCsv: "text/csv"
    //    },
    //    ContentType: {
    //        Utf8: "application/json; charset=utf-8",
    //        AppJson: "application/json",
    //        False: false,
    //        True: true
    //    },
    //    ProcessData: {
    //        False: false,
    //        True: true
    //    },
    //    Async: {
    //        False: false,
    //        True: true
    //    },
    //    Cache: {
    //        False: false,
    //        True: true
    //    }
    //},
    ServerDataReturn: {
        QueryData: "Query data set from the server",
        hasMyQueryAceess: "set from server"
    },
    globalverb: {
        //BuildRowFromCells: [],
        GCurrentRowHolder: { previous_row: -1, previous_col: -1 },
        IsServerProcessing: false,
        SaveRowDialog: "",
        //LastRowSelected: null,
        LastLeftmenuClick: "",
        Isnewrow: false,
        beforePast: "",
        IsBackgroundProcessing: false,
        ids: [],
        DialogType: "",
        Gviewid: "",
        GTransferToitem: { tableName: "", Tableid: "-1", isDueBack: "" },
        GTransferHtmlHolder: "",
        GIsTransferAllData: false,
        GTransferRowcounter: 0,
        Grequest: { employid: "-1", priority: "Standard" },
        GIsstring: "",
        GisDrillDownCall: false,
        GTopHeightDrillDown: 0,
        GrowKey: "",
        GForceRefresh: false,
        GLabelDesignId: 0
    },
    SpinningWheel: {
        Main: "#spinningWheel",
        Grid: "#spingrid",
        Total: "#spinTotal"
    },
    BreadCrumb: {
        CrumbContextmenu: "#CrumbContextmenu",
    },
    Enums: {
        Crumbs: {
            AuditHistoryPerRow: 0,
            TrackingHistoryPerRow: 1,
            ContentsPerRow: 2,
            Vault: 3
        },
        Error: {
            DuplicatedId: 2627,
            ConversionFailed: 245
        },
        ViewType: {
            FusionView: 0,
            Favorite: 1,
            GlobalSearch: 2
        },
        location: {
            LeftMenuView: 0,
            FavoritView: 1,
            MyQueryView: 2,
            DrilldownView: 3,
            ClickcrumbView: 4,
            CrumbRightClickView: 5,
            GlobalView: 6,
            TaskBarView: 7
        }
    },
    DomLocation: {
        Current: "",
        Previous: "",
        CurrnetCrumb: "",
        PreviousCrumb: ""
    },
    //Linkscript: {
    //    isBeforeAdd: false,
    //    isAfterAdd: false,
    //    isBeforeEdit: false,
    //    isAfterEdit: false,
    //    isBeforeDelete: false,
    //    isAfterDelete: false,
    //    ScriptDone: false,
    //    id: "",
    //    keyvalue: ""
    //},
}
//call dialog box
class DialogBoxBasic {
    constructor(title, contentHtml, type) {
        this._title = title;
        this._content = contentHtml;
        this.isconfirm = "";
        this.Type = type;
    }
    ShowDialog() {
        var _this = this;
        return new Promise((resolve, reject) => {
            document.querySelector(app.domMap.Dialogboxbasic.DlgBsTitle).innerHTML = _this._title;
            if (this.Type === app.domMap.Dialogboxbasic.Type.PartialView) {
                this.LoadPatialViewDialog().then((data) => {
                    resolve(data);
                });
            } else if (this.Type === app.domMap.Dialogboxbasic.Type.Content) {
                this.LoadContentDialog().then(() => {
                    resolve(true);
                });
            }
        });
    }
    LoadPatialViewDialog() {
        //need to pass the dialog model-body class
        //option: to return direct partial view from the server. 
        return new Promise((resolve, reject) => {
            $(app.domMap.Dialogboxbasic.DlgBsContent).load(this._content, function () {
                $(app.domMap.Dialogboxbasic.BasicDialog).show();
                $('.modal-dialog').draggable({
                    handle: ".modal-header",
                    stop: function (event, ui) {
                    }
                });
                resolve(true);
            });
        });
    }
    LoadContentDialog() {
        //dont need to pass the dialog model-body class as we build it for you.
        return new Promise((resolve, reject) => {
            $(app.domMap.Dialogboxbasic.DlgBsContent).html("<div id='dlgbodyId' class='modal-body'>" + this._content + "</div>");
            $(app.domMap.Dialogboxbasic.BasicDialog).show();
            $('.modal-dialog').draggable({
                handle: ".modal-header",
                stop: function (event, ui) {
                }
            });
            resolve(true);
        });
    }
    CloseDialog(isConfirmed) {
        //if (isConfirmed === undefined) return alert("Developer, pass true or false arguments in DialogBoxBasic object #Function: closeDialog()")
        //this.isconfirm = isConfirmed === undefined ? "" : isConfirmed;
        $(app.domMap.Dialogboxbasic.BasicDialog).hide();
        if (app.globalverb.DialogType === "barcode") {
            $.get(app.url.server.DeleteLabelsFiles).fail((e) => {
                showAjaxReturnMessage("something went wrong after closing the popup window, please,contact your administrator for more info", "e")
            })
        }
        app.globalverb.DialogType = "";
    }
}

//validation fields
class ValidationFields {
    //to use validator you have to pass the app.daa.listOfheader object and the value
    constructor(value, ListOfHeaders) {
        this.value = value;
        this.objc = ListOfHeaders;
        this.errSmallIntMaxValue = false;
        this.errBigIntMaxValue = false;
        this.errStringMaxLength = false;
        this.errDatePattern = false;
        this.errBoolean = false;
        this.errDouble = false;
        this.errIsAllowEmpty = false;
    }
    Validator() {
        //this.value = this.value !== null ? this.value : "";
        if (this.objc.Allownull === false && this.objc.isCounterField === false) {
            if (this.value === null || this.value === "") {
                this.errIsAllowEmpty = true;
            }
        }
        if (this.value !== null) {
            if (this.objc.DataTypeFullName === "System.Boolean") return;
            //check if allow empty
            switch (this.objc.DataTypeFullName) {
                case "System.String":
                    if (this.objc.MaxLength < this.value.toString().length) {
                        this.fieldType = "System.String";
                        this.errStringMaxLength = true;
                    }
                    break;
                case "System.Int16":
                    if (parseInt(this.value) > 32767) {
                        this.fieldType = "System.Int16"
                        this.errSmallIntMaxValue = true;
                    }
                    break;
                case "System.Int32":
                    if (parseInt(this.value) > 2147483647) {
                        this.fieldType = "System.Int32";
                        this.errBigIntMaxValue = true;
                    }
                    break;
                case "System.Double":
                    this.fieldType = "System.Double"
                    this.errDouble = true;
                    break;
                case "System.DateTime":
                    this.fieldType = "DateTime"
                    if (this.value !== "") {
                        if (!moment(this.value, app.data.dateFormat, true).isValid()) {
                            this.errDatePattern = true;
                        }
                    }
                    break;
                default:
            }
        }
    }
}
//get key ids for each row
function GetrowsKeyids() {
    var ids = [];
    for (var i = 0; i < rowselected.length; i++) {
        var cellid = hot.getDataAtCell(rowselected[i], 0)
        if (cellid !== null) {
            ids.push(cellid);
        }
    }
    return ids;
}
//check autosave button if true or false 
function CheckrowsAutosave(type) {
    var saveauto = localStorage.getItem("rowautosave");
    var elem = document.querySelector("#autosavebtn");

    if (type === "click") {
        if (saveauto == "false") {
            localStorage.setItem("rowautosave", true);
            elem.checked = 1;
        } else {
            localStorage.setItem("rowautosave", false);
            elem.checked = 0;
        }
        //app.DomLocation.Current.click();
    } else {
        if (saveauto == "true") {
            localStorage.setItem("rowautosave", true);
            elem.checked = 1;
        } else {
            localStorage.setItem("rowautosave", false);
            elem.checked = 0;
        }
    }
}
//resizing calculation for grid.
function CalculateHeightGridSize() {
    var pagingHeight = 74;
    var toolBarHeight = 44
    var TrackingHeight = $('#TrackingStatusDiv').height();
    var TaskContainerHeight = $('#TaskContainer').height();
    var pageContent = $("#page-content-wrapper").height();
    return pageContent - (pagingHeight + toolBarHeight + TrackingHeight + TaskContainerHeight);
}

var dlgClose = new DialogBoxBasic();





