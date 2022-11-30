class PagingFunctions {
    constructor(SelectedIndex, PageSize, TotalRecords, PerPageRecords) {
        this.SelectedIndex = SelectedIndex;
        this.PageSize = PageSize
        this.VisibleIndexsNo = 3
        this.TotalRecords = TotalRecords
        this.PerPageRecords = PerPageRecords
        this.GotoTextFieldId = "#inputGotoPageNum"
        this.BtnSpanGotoPage = "#spanGotoPage"
        this.InvalidPageNoMsg = "Invalid Page Number"
        this.SelectingIndexId = "#paging-div ul .liIndex"
    }
    // this function are using for load data
    LoadRecord() {
        // this is override function
    }
    // this function are using load next record
    Next() {
        if (this.SelectedIndex == this.PageSize || this.PageSize == 0) {
            return false;
        }

        this.SetActiveIndex(this.SelectedIndex, (this.SelectedIndex + 1))
        this.SelectedIndex = this.SelectedIndex + 1;
        $(this.GotoTextFieldId).val(this.SelectedIndex)
        this.ResetPaginationAndLoadData()
    }
    // this function are using for load previous record
    Previous() {
        if (this.SelectedIndex == 1 || this.PageSize == 0) {
            return false;
        }
        this.SetActiveIndex(this.SelectedIndex, (this.SelectedIndex - 1))
        this.SelectedIndex = this.SelectedIndex - 1;
        $(this.GotoTextFieldId).val(this.SelectedIndex)
        this.ResetPaginationAndLoadData()

    }
    //this function are using for go to page number and get reocrd
    GoToPage() {
        var pageNum = $(this.GotoTextFieldId).val();

        if (pageNum == this.SelectedIndex) {
            return false;
        }

        if (pageNum.length > 0) {
            if (parseInt(pageNum) <= this.PageSize) {
                this.SetActiveIndex(this.SelectedIndex, parseInt(pageNum))
                this.SelectedIndex = parseInt(pageNum);
                this.ResetPaginationAndLoadData()
            }
        }
    }
    // this function are using go to first page
    FirstPage() {

        if (this.SelectedIndex == 1) {
            return false;
        }

        if (this.PageSize > 0) {
            this.SetActiveIndex(this.SelectedIndex, 1)
            this.SelectedIndex = 1;
            $(this.GotoTextFieldId).val(this.SelectedIndex)
            this.ResetPaginationAndLoadData()
        }
    }
    // this function are using for go to last page
    LastPage() {
        if (this.SelectedIndex == this.PageSize) {
            return false;
        }
        if (this.PageSize > 0) {
            this.SetActiveIndex(this.SelectedIndex, this.PageSize)
            this.SelectedIndex = this.PageSize;
            $(this.GotoTextFieldId).val(this.SelectedIndex)
            this.ResetPaginationAndLoadData()
        }
    }
    // It highlight the selected page.
    SetActiveIndex(remove, add) {
        $("#paging-container ul [index='" + remove + "']").removeClass("active");
        $("#paging-container ul [index='" + add + "']").addClass("active");
    }
    GetRecord(obj) {
        try {
            var PageNumber = parseInt($(obj).attr("index"));
            if (PageNumber == this.SelectedIndex) {
                return false;
            }
            this.SelectedIndex = PageNumber;
            $(this.GotoTextFieldId).val(this.SelectedIndex)
            this.ResetPaginationAndLoadData()
        } catch (ex) {
            console.log(ex)
        }
    }
    //this function are call the two function that are load the handsontable data
    //and second function (VisibleIndexs) set reset the pagination UI indexs
    ResetPaginationAndLoadData() {
        this.LoadRecord();
        this.VisibleIndexs()
    }
    SetTextFieldValues() {
        $("#divTotalRecords span").html(this.TotalRecords)
        $("#divPerPageRecord span").html(this.PerPageRecords)
        $("#spanTotalPage").html(this.PageSize)
        //$(this.BtnSpanGotoPage).hide();
        $(this.GotoTextFieldId).val(this.SelectedIndex)
        this.AddCommaToTotalRecords()
    }
    AddCommaToTotalRecords() {
        $("#divTotalRecords span").html(this.TotalRecords.toLocaleString())
    }
    VisibleIndexs() {
        if (this.PageSize > this.VisibleIndexsNo) {
            for (var i = 1; i <= this.PageSize; i = i + this.VisibleIndexsNo) {
                if (this.SelectedIndex >= i && this.SelectedIndex < (i + this.VisibleIndexsNo)) {
                    var elementIndex = 0, end = i + this.VisibleIndexsNo;
                    if (i + this.VisibleIndexsNo > this.PageSize) {
                        end = this.PageSize
                    }

                    for (var j = i; j <= end; j++) {
                        if (j == this.SelectedIndex) {
                            $(this.SelectingIndexId).eq(elementIndex).show().addClass("active").attr("index", j).children("a").html(j)
                        } else {
                            $(this.SelectingIndexId).eq(elementIndex).show().removeClass("active").attr("index", j).children("a").html(j)
                        }
                        elementIndex = elementIndex + 1;
                    }

                    if (this.VisibleIndexsNo - elementIndex > 0) {
                        for (var k = elementIndex; k <= this.VisibleIndexsNo - 1; k++) {
                            $(this.SelectingIndexId).eq(k).hide();
                        }
                    }
                    break;
                }
            }
        } else {
            if (this.PageSize == 0) { return false; }
            var elementIndex = 0;
            for (var j = 0; j < this.PageSize; j++) {
                if (j + 1 == this.SelectedIndex) { $(this.SelectingIndexId).eq(j).show().addClass("active").attr("index", j + 1).children("a").html(j + 1) }
                else { $(this.SelectingIndexId).eq(j).show().removeClass("active").attr("index", j + 1).children("a").html(j + 1) }
                elementIndex = elementIndex + 1;
            }
            if (this.VisibleIndexsNo - elementIndex > 0) {
                for (var k = elementIndex; k <= this.VisibleIndexsNo - 1; k++) {
                    $(this.SelectingIndexId).eq(k).hide();
                }
            }
        }
    }
    GotoPageNoChangeEvt(evt) {
        var goto = $(this.GotoTextFieldId).val();
        if (goto > this.PageSize || goto < 1) {
            showAjaxReturnMessage(this.InvalidPageNoMsg, 'w');
            $(this.GotoTextFieldId).val(this.SelectedIndex);
            return false;
        }
        $(this.BtnSpanGotoPage).click();
        return true;
    }
    GotoPageNoKeypressEvt(evt) {
        var ASCIICode = (evt.which) ? evt.which : evt.keyCode
        var goto = $(this.GotoTextFieldId).val();

        if ((goto > this.PageSize || goto < 1) && goto != "") {
            showAjaxReturnMessage(this.InvalidPageNoMsg, 'w');
            $(this.GotoTextFieldId).val(this.SelectedIndex);
            return false;
        }

        if (ASCIICode == 13) {
            $(this.BtnSpanGotoPage).click();
            return false;
        }
        if (ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57)) {
            return false;
        }
        return true;
    }
}

//app share properties
var prop = {
    Dialogbox: {
        Dialogboxid: "#Dialogboxid",
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
            LoadPartialView: "LoadPartialView",
            Content: "Content"
        },
        DlgbodyId: "#dlgbodyId",
        DlgArrow: "#arrowUpDown"
    },
    Dialogboxmsg:{
        Dialogboxmsgid: "#Dialogboxmsgid",
        DlgBsContentmsg: "#dlgBsContentmsg",
        DlgBsTitlemsg: "#dlgBsTitlemsg",
        Dlg: {
            DialogMsg: {
                DialogMsgTxtmsg: "#dialogMsgTxtmsg"
            },
            DialogMsgConfirm: {
                DialogMsgTxtmsg: "#dialogMsgTxtmsg",
                DialogYesmsg: "#dialogYesmsg",
                ListItemmsg: "#listItemmsg"
            },
        },
        Type: {
            PartialView: "PartialView",
            LoadPartialView: "LoadPartialView",
            Content: "Content"
        },
        DlgbodyIdmsg: "#dlgbodyIdmsg",
    },
    DialogboxAudit: {
        DialogboxAuditid: "#DialogboxAuditid",
        DlgBsContentAudit: "#dlgBsContentAudit",
        DlgBsTitleAudit: "#dlgBsTitleAudit",
        Type: {
            PartialView: "PartialView",
            LoadPartialView: "LoadPartialView",
            Content: "Content"
        },
        DlgArrowAudit: "#arrowUpDownAudit"
    },
    ajax: {
        Type: {
            Post: "POST",
            Get: "GET",
            Put: "PUT",
            Delete: "DELETE"
        },
        DataType: {
            Json: "json",
            Html: "html",
            Text: "text",
            String: "string"
        },
        ContentType: {
            Utf8: "application/json; charset=utf-8",
            AppJson: "application/json",
            False: false,
            True: true
        },
        ProcessData: {
            False: false,
            True: true
        },
        Async: {
            False: false,
            True: true
        },
        Cache: {
            False: false,
            True: true
        }
    },
    SpinningWheel: {
        Main: "#spinningWheel",
        Grid: "#spingrid",
    },
    language: JSON.parse(localStorage.getItem("language"))
}

//Dialogbox events and functions 
class DialogBox {
    constructor(title, contentHtml, type) {
        this._title = title;
        this._content = contentHtml;
        this.isconfirm = "";
        this.Type = type;
    }
    ShowDialog() {
        var _this = this;
        var isDialogExist = document.querySelector(prop.Dialogbox.Dialogboxid);
        if (isDialogExist === null) {
            this.CreateDialogHtml();
        }
        document.querySelector(prop.Dialogbox.DlgBsTitle).innerHTML = _this._title;
        if (this.Type === prop.Dialogbox.Type.PartialView) {
            return new Promise((resolve, reject) => {
                this.PatialViewDialog().then((data) => {
                    resolve(data);
                });
            });
        } else if (this.Type === prop.Dialogbox.Type.LoadPartialView) {
            return new Promise((resolve, reject) => {
                this.LoadPatialViewDialog().then((data) => {
                    resolve(data)
                });
            });

        } else if (this.Type === prop.Dialogbox.Type.Content) {
            return new Promise((resolve, reject) => {
                this.LoadContentDialog().then(() => {
                    resolve(true);
                });
            });

        }
    }
    LoadContentDialog() {
        //dont need to pass teh dialog model-body class as we build it for you.
        return new Promise((resolve, reject) => {
            $(prop.Dialogbox.DlgBsContent).html("<div id='dlgbodyId' class='modal-body'>" + this._content + "</div>");
            $(prop.Dialogbox.Dialogboxid).show();
            $('.modal-dialog').draggable({
                handle: ".modal-header",
                stop: function (event, ui) {
                }
            });
            resolve(true);
        });
    }
    PatialViewDialog() {
        var _this = this;
        //option: to return direct partial view from the server. 
        return new Promise((resolve, reject) => {
            //$(prop.Dialogbox.DlgBsContent).load(_this._content, function () {
            $(prop.Dialogbox.DlgBsContent).html(_this._content);
            $(prop.Dialogbox.Dialogboxid).show();
            $('.modal-dialog').draggable({
                handle: ".modal-header",
                stop: function (event, ui) {
                }
            });
            resolve(true);
        });
    }
    LoadPatialViewDialog() {
        var _this = this;
        return new Promise((resolve, reject) => {
            $(prop.Dialogbox.DlgBsContent).load(_this._content, () => {
                $(prop.Dialogbox.Dialogboxid).show();
                $('.modal-dialog').draggable({
                    handle: ".modal-header",
                    stop: function (event, ui) {
                    }
                });
            });
            resolve(true)
        });
    }
    CreateDialogHtml() {
        $("body").append(`<div Id="Dialogboxid" style="display: none">
                            <div class="modal modalblock" tabindex="-1">
                                <div class="modal-dialog">
                                    <div class="modal-content">
                                        <div class="modal-header cursor-drag-icon">
                                            <button type="button" onclick="$(prop.Dialogbox.Dialogboxid).hide()" class="close cancelQueryWindow" data-dismiss="modal" aria-label="Close">
                                                <i class="fa fa-close theme-color"></i>
                                            </button>
                                            <button id="arrowUpDown" type="button" class="close cancelQueryWindow">
                                                <i class="fa fa-angle-up"></i>
                                            </button>
                                            <h4 class="modal-title">
                                                <label Id="dlgBsTitle" class="theme_color"></label>
                                            </h4>
                                        </div>
                                        <div id="dlgBsContent">
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>`)
    }
    ArrowUpDown(e) {
        if (e.currentTarget.children[0].className === "fa fa-angle-up") {
            e.currentTarget.children[0].className = "fa fa-angle-down";
            $(prop.Dialogbox.DlgBsContent).hide();
        } else {
            e.currentTarget.children[0].className = "fa fa-angle-up";
            $(prop.Dialogbox.DlgBsContent).show();
        }
    }
}

class DialogBoxMsg {
    constructor(title, contentHtml, type) {
        this._title = title;
        this._content = contentHtml;
        this.isconfirm = "";
        this.Type = type;
    }
    ShowDialog() {
        var _this = this;
        var isDialogExist = document.querySelector(prop.Dialogboxmsg.Dialogboxmsgid);
        if (isDialogExist === null) {
            this.CreateDialogHtml();
        }
        document.querySelector(prop.Dialogboxmsg.DlgBsTitlemsg).innerHTML = _this._title;
        if (this.Type === prop.Dialogboxmsg.Type.PartialView) {
            return new Promise((resolve, reject) => {
                this.PatialViewDialog().then((data) => {
                    resolve(data);
                });
            });
        } else if (this.Type === prop.Dialogboxmsg.Type.LoadPartialView) {
            return new Promise((resolve, reject) => {
                this.LoadPatialViewDialog().then((data) => {
                    resolve(data)
                });
            });

        } else if (this.Type === prop.Dialogboxmsg.Type.Content) {
            return new Promise((resolve, reject) => {
                this.LoadContentDialog().then(() => {
                    resolve(true);
                });
            });

        }
    }
    LoadContentDialog() {
        //dont need to pass teh dialog model-body class as we build it for you.
        return new Promise((resolve, reject) => {
            $(prop.Dialogboxmsg.DlgBsContentmsg).html("<div id='dlgbodyId' class='modal-body'>" + this._content + "</div>");
            $(prop.Dialogboxmsg.Dialogboxmsgid).show();
            $('.modal-dialog').draggable({
                handle: ".modal-header",
                stop: function (event, ui) {
                }
            });
            resolve(true);
        });
    }
    PatialViewDialog() {
        var _this = this;
        //option: to return direct partial view from the server. 
        return new Promise((resolve, reject) => {
            //$(prop.Dialogbox.DlgBsContent).load(_this._content, function () {
            $(prop.Dialogboxmsg.DlgBsContentmsg).html(_this._content);
            $(prop.Dialogboxmsg.Dialogboxmsgid).show();
            $('.modal-dialog').draggable({
                handle: ".modal-header",
                stop: function (event, ui) {
                }
            });
            resolve(true);
        });
    }
    LoadPatialViewDialog() {
        var _this = this;
        return new Promise((resolve, reject) => {
            $(prop.Dialogboxmsg.DlgBsContentmsg).load(_this._content, () => {
                $(prop.Dialogboxmsg.Dialogboxmsgid).show();
                $('.modal-dialog').draggable({
                    handle: ".modal-header",
                    stop: function (event, ui) {
                    }
                });
            });
            resolve(true)
        });
    }
    CreateDialogHtml() {
        $("body").append(`<div Id="Dialogboxmsgid" style="display: none">
        <div class="modal modalblock" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header cursor-drag-icon">
                        <button type="button" onclick="$(prop.Dialogboxmsg.Dialogboxmsgid).hide()" class="close cancelQueryWindow" data-dismiss="modal" aria-label="Close">
                            <i class="fa fa-close theme-color"></i>
                        </button>
                        <h4 class="modal-title">
                            <label Id="dlgBsTitlemsg" class="theme_color"></label>
                        </h4>
                    </div>
                    <div id="dlgBsContentmsg">
                    </div>
                </div>
            </div>
        </div>
    </div>`)
    }
}

class DialogBoxAudit {
    constructor(title, contentHtml, type) {
        this._title = title;
        this._content = contentHtml;
        this.isconfirm = "";
        this.Type = type;
    }
    ShowDialog() {
        var _this = this;
        var isDialogExist = document.querySelector(prop.DialogboxAudit.DialogboxAuditid);
        if (isDialogExist === null) {
            this.CreateDialogHtml();
        }
        document.querySelector(prop.DialogboxAudit.DlgBsTitleAudit).innerHTML = _this._title;
        if (this.Type === prop.DialogboxAudit.Type.PartialView) {
            return new Promise((resolve, reject) => {
                this.PatialViewDialog().then((data) => {
                    resolve(data);
                });
            });
        } else if (this.Type === prop.DialogboxAudit.Type.LoadPartialView) {
            return new Promise((resolve, reject) => {
                this.LoadPatialViewDialog().then((data) => {
                    resolve(data)
                });
            });

        } else if (this.Type === prop.DialogboxAudit.Type.Content) {
            return new Promise((resolve, reject) => {
                this.LoadContentDialog().then(() => {
                    resolve(true);
                });
            });

        }
    }
    LoadContentDialog() {
        //dont need to pass teh dialog model-body class as we build it for you.
        return new Promise((resolve, reject) => {
            $(prop.DialogboxAudit.DlgBsContentAudit).html("<div id='dlgbodyId' class='modal-body'>" + this._content + "</div>");
            $(prop.DialogboxAudit.DialogboxAuditid).show();
            $('.modal-dialog').draggable({
                handle: ".modal-header",
                stop: function (event, ui) {
                }
            });
            resolve(true);
        });
    }
    PatialViewDialog() {
        var _this = this;
        //option: to return direct partial view from the server. 
        return new Promise((resolve, reject) => {
            //$(prop.Dialogbox.DlgBsContent).load(_this._content, function () {
            $(prop.DialogboxAudit.DlgBsContentAudit).html(_this._content);
            $(prop.DialogboxAudit.DialogboxAuditid).show();
            $('.modal-dialog').draggable({
                handle: ".modal-header",
                stop: function (event, ui) {
                }
            });
            resolve(true);
        });
    }
    LoadPatialViewDialog() {
        var _this = this;
        return new Promise((resolve, reject) => {
            $(prop.DialogboxAudit.DlgBsContentAudit).load(_this._content, () => {
                $(prop.DialogboxAudit.DialogboxAuditid).show();
                $('.modal-dialog').draggable({
                    handle: ".modal-header",
                    stop: function (event, ui) {
                    }
                });
            });
            resolve(true)
        });
    }
    CreateDialogHtml() {
        $("body").append(`<div Id="DialogboxAuditid" style="display: none">
            <div class="modal modalblock" tabindex="-1">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header cursor-drag-icon">
                            <button type="button" onclick="$(prop.DialogboxAudit.DialogboxAuditid).hide()" class="close cancelQueryWindow" data-dismiss="modal" aria-label="Close">
                                <i class="fa fa-close theme-color"></i>
                            </button>
                            <button id="arrowUpDownAudit" type="button" class="close cancelQueryWindow">
                                <i class="fa fa-angle-up"></i>
                            </button>
                            <h4 class="modal-title">
                                <label Id="dlgBsTitleAudit" class="theme_color"></label>
                            </h4>
                        </div>
                            <div id="dlgBsContentAudit">
                            </div>
                    </div>
                </div>
            </div>
        </div>`)
    }
    ArrowUpDown(e) {
        if (e.currentTarget.children[0].className === "fa fa-angle-up") {
            e.currentTarget.children[0].className = "fa fa-angle-down";
            $(prop.DialogboxAudit.DlgBsContentAudit).hide();
        } else {
            e.currentTarget.children[0].className = "fa fa-angle-up";
            $(prop.DialogboxAudit.DlgBsContentAudit).show();
        }
    }
}


$('body').on("click", prop.Dialogbox.DlgArrow, (e) => {
    if (e.currentTarget.children[0].className === "fa fa-angle-up") {
        e.currentTarget.children[0].className = "fa fa-angle-down";
        $(prop.Dialogbox.DlgBsContent).hide();
    } else {
        e.currentTarget.children[0].className = "fa fa-angle-up";
        $(prop.Dialogbox.DlgBsContent).show();
    }
});

$('body').on("click", prop.DialogboxAudit.DlgArrowAudit, (e) => {
    if (e.currentTarget.children[0].className === "fa fa-angle-up") {
        e.currentTarget.children[0].className = "fa fa-angle-down";
        $(prop.DialogboxAudit.DlgBsContentAudit).hide();
    } else {
        e.currentTarget.children[0].className = "fa fa-angle-up";
        $(prop.DialogboxAudit.DlgBsContentAudit).show();
    }
});

//end Dialogbox events and functions 

class printTable {
    constructor(headers, rows, id, printtitle, HasHiddenField) {
        this.Rows = rows == "" ? "" : rows;
        this.Headers = headers == "" ? "" : headers;
        this.Id = id == "" ? "" : id;
        this.PrintTitle = printtitle == "" ? "" : printtitle;
        this.HasHiddenField = HasHiddenField;
    }
    PrintTable() {
        //print handsontable pass handsontable rows and headers
        $(prop.SpinningWheel.Main).show();
        this.CheckIfTableHasHiddenColumn();
        var Databuilder = {};
        var printData = [];

        for (var i = 0; i < this.Rows.length; i++) {
            for (var j = 0; j < this.Headers.length; j++) {
                Databuilder[this.Headers[j]] = this.Rows[i][j];
            }
            printData.push(Databuilder);
            Databuilder = {};
        }

        setTimeout(() => {
            printJS({
                printable: printData,
                properties: this.Headers,
                header: "Record count: " + this.Rows.length,
                headerStyle: "font-weight: 50px; font-size:20px",
                documentTitle: this.PrintTitle,
                gridHeaderStyle: "font-size: 22px;font-weight: bold; border: 1px solid black;",
                gridStyle: "font-size: 20px; border: 1px solid black",
                type: 'json'
            });
            $(prop.SpinningWheel.Main).hide();
        }, 50)
    }
    CheckIfTableHasHiddenColumn() {
        //if there is an hidden field we remove the 0 column out from the print.
        console.log(this.HasHiddenField)
        if (this.HasHiddenField === 0) {
            this.Headers.shift();
            this.Rows.findIndex(firstColumnRemove => {
                firstColumnRemove.shift();
            });
        }
    }
}

function HandsOnGetrowsSelected(hot) {
    if (hot.getSelected() === undefined) return;
    var rowsSelected = [];
    if (hot.getSelected().length === 1) {
        var start = hot.getSelected()[0][0];
        var end = hot.getSelected()[0][2];
        if (start < end) {
            for (var i = start; i < end + 1; i++) {
                rowsSelected.push(i);
            }
        } else {
            for (var i = end; i < start + 1; i++) {
                rowsSelected.push(i);
            }
        }
    } else {
        var len = hot.getSelected().length;
        for (var i = 0; i < len; i++) {
            var rowNumber = hot.getSelected()[i][0]
            rowsSelected.push(rowNumber);
        }
    }
    return rowsSelected;
}

//dom properties
let rowselected = []
var repapp = {
    hot: "",
    dom: {
        ReportTitle: "#ReportTitle",
        Btn: {
            print: "#btnprint",
            exportcsv: "#btnExportCSVReport",
            exportcsvAll: "#btnExportCSVAllReport",
            SendcheckedItems: "#sendcheckedItems",
            DDLRequestSelected: "#DDLRequestSelected",
            DDLCertifiedPosionSelected: "#DDLCertifiedPosionSelected",
            setinactive: "#setInactive",
            DDLfinaldisposition: "#DDLfinaldisposition",
            SubmitFordestructionPopup: "#SubmitFordestructionPopup",
            SubmitForPurgePopup: "#submitforpurgePopup",
            SubmitForArchivePopup: "#submitforarchivePopup",
            NewpurgeReport: "#newpurgeReport",
            NewDestructionReport: "#newdestructionReport",
            NewpermanentActive: "#newpermanentActive",
            ddlArchivedDescription: "#ddlArchivedDescription",
            btnleftButton: "#btnleft button"
        },
        ExportFiles: {
            ExportSelectedToCSVorTXT: "#ExportToCSVorTXT",
            ExportToCSVorTXTAll: "#ExportToCSVorTXTAll"
        },
        btnContainer: "#btnContainer",
        btnleft: "#btnleft",
        ReportTitle: "#ReportTitle",
        itemDescription: "#itemDescription",
        HandsOnTableContainer: "#handsOnTableContainer",
        CurrentMenuElemnt: "",
        Paging: {
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
        QueryWindow: {
            QuerylblTitle: "#QuerylblTitle",
            QuerySaveInput: "#QuerySaveInput",
            ChekBasicQuery: "#chekBasicQuery",
            OkQuery: "#OkQuery",
            BtnCancel: "#btnCancel",
            BtnQueryApply: "#btnQueryApply",
            BtnSaveQuery: "#btnSaveQuery",
            QueryContentDialog: "#QueryContentDialog",
            Querytableid: "#querytableid",
            UpdateQuery: "#updateQuery",
            SaveContent: "",
            buildQuery: []
        },
        setInactive: {
            Inactivedate: "#Inactivedate",
            InactiveUserName: "#InactiveUserName",
            inactivepullDDL: "#inactivepullDDL",
            btnsetinactive: "#btnsetinactive"
        },
        SubmitPopup: {
            submitDate: "#submitDate",
            submitUserName: "#submitUserName",
            submitDropdown: "#submitDropdown",
            btnSubmit: "#btnSubmit"
        }
    },
    url: {
        LoadQueryWindow: "/Reports/LoadQueryWindow",
        RunQuery: "/Reports/RunQuery",
        GetauditReportView: "/Reports/GetauditReportView",
        RunAuditSearch: "/Reports/RunAuditSearch",
        InitiateReports: "/Reports/InitiateReports",
        BtnSendRequestToThePullList: "/Reports/BtnSendRequestToThePullList",
        InitiateRetentionReport: "/Reports/InitiateRetentionReport",
        GetInactivePullList: "/Reports/GetInactivePullList",
        BtnSubmitInactive: "/Reports/BtnSubmitInactive",
        GetSubmitForm: "/Reports/GetSubmitForm",
        BtnSubmitDisposition: "/Reports/BtnSubmitDisposition",
        BtnCreateNewPurged: "/Reports/BtnCreateNewPurged",
        BtnCreateNewDestruction: "/Reports/BtnCreateNewDestruction",
        BtnCreateNewArchived: "/Reports/BtnCreateNewArchived",
        PagingPartialView: "/Reports/PagingPartialView",
        InitiateRetentionReportPagination: "/Reports/InitiateRetentionReportPagination",
        InitiateReportsPagination: "/Reports/InitiateReportsPagination",
        RunAuditReportPagination: "/Reports/RunAuditReportPagination",
        GetTotalrowsForGrid: "/data/GetTotalrowsForGrid",
        DialogConfirmExportCSVorTXT: "/Exporter/DialogConfirmExportCSVorTXTReport",
        BackGroundProcessing: "/Exporter/BackGroundProcessing",
        ExportCSVOrTXT: "/Exporter/ExportCSVOrTXTReport",
        ExportCSVorTXTAll: "/Exporter/ExportCSVorTXTAll",
    },
    enum: {
        PastDueTrackableItemsReport: 0,
        ObjectOut: 1,
        ObjectsInventory: 2,
        RequestNew: 3,
        RequestNewBatch: 4,
        RequestPullList: 5,
        RequestException: 6,
        RequestInProcess: 7,
        RequestWaitList: 8,
        RetentionFinalDisposition: 9,
        RetentionCertifieDisposition: 10,
        RetentionInactivePullList: 11,
        RetentionInactiveRecords: 12,
        RetentionRecordsOnHold: 13,
        RetentionCitations: 14,
        RetentionCitationsWithRetCodes: 15,
        RetentionCodes: 16,
        RetentionCodesWithCitations: 17,
        AuditReport: 18,
        CustomReport: 19
    },
    ob: {
        pageNumber: -1,
        reportType: -1,
        tableName: "",
        id: "",
        isQueryFromDDL: false,
        isCountRecord: false
    },
    Global: {
    },
    data: ""
}
//EXPORT CSV OR TXT
$('body').on("click", repapp.dom.Btn.exportcsv, function () {
    if (repapp.hot == '') {
        showAjaxReturnMessage("There is no data to export.", 'w')
        return false;
    }
    else if (repapp.hot.getData().length == 0) {
        showAjaxReturnMessage("There is no data to export.", 'w')
        return false;
    }
    else {
        exportData.ExportCSVorTXTCurrent(true);
    }
})
//EXPORT CSV OR TXT - ALL
$('body').on("click", repapp.dom.Btn.exportcsvAll, function () {
    if (repapp.ob.reportType == repapp.enum.CustomReport) {
        if (repapp.hot == '') {
            showAjaxReturnMessage("There is no data to export.", 'w')
            return false;
        }
        else if (repapp.hot.getData().length == 0) {
            showAjaxReturnMessage("There is no data to export.", 'w')
            return false;
        }
        else {
            exportData.ExportCSVorTXTAll(true);
        }
    }
})
$('body').on('click', repapp.dom.ExportFiles.ExportSelectedToCSVorTXT, () => {
    if (reports.IsBackgroundProcessing) {
        $.get(repapp.url.BackGroundProcessing).done((response) => {
            if (response.isError === false) {
                var dlg = new DialogBox(response.lblTitle, response.HtmlMessage, prop.Dialogbox.Type.Content);
                dlg.ShowDialog().then(() => { })
            } else {
                showAjaxReturnMessage(response.Msg, "e");
            }
        });
    } else {
        location.href = repapp.url.ExportCSVOrTXT;
    }
    $("#Dialogboxmsgid").hide();
});
$('body').on('click', repapp.dom.ExportFiles.ExportToCSVorTXTAll, () => {
    if (reports.IsBackgroundProcessing) {
        $.get(repapp.url.BackGroundProcessing).done((response) => {
            if (response.isError === false) {
                var dlg = new DialogBox(response.lblTitle, response.HtmlMessage, prop.Dialogbox.Type.Content);
                dlg.ShowDialog().then(() => { })
            } else {
                showAjaxReturnMessage(response.Msg, "e");
            }
        });
    } else {
        location.href = repapp.url.ExportCSVorTXTAll;
    }
    $(prop.Dialogbox.Dialogboxid).hide();
});
//reports events
$('body').on("click", repapp.dom.Btn.print, () => {
    if (repapp.hot.getColHeader === undefined) { return }
    var print = new printTable();
    print.Headers = repapp.hot.getColHeader();
    print.Rows = repapp.hot.getData();
    print.Id = ""
    print.PrintTitle = document.querySelector(repapp.dom.ReportTitle).innerText;
    print.HasHiddenField = HandsOnTableReports.HasHiddenField;
    print.PrintTable();
});
//send checked items from new request
$('body').on("click", repapp.dom.Btn.SendcheckedItems, (e) => {
    if (repapp.hot.getSelected() === undefined) {
        //showAjaxReturnMessage("Please, select a row!", "w");
        showAjaxReturnMessage("Please select a row/s to send into Pull List", "w");
        return;
    }
    reports.SendcheckedItemsToPullList(e);
});
//pull request dll on change
$('body').on("change", repapp.dom.Btn.DDLRequestSelected, (e) => {
    repapp.ob.reportType = repapp.enum.RequestPullList;
    repapp.ob.pageNumber = 1;
    repapp.ob.id = e.currentTarget.value;
    repapp.ob.isQueryFromDDL = true;
    reports.RunReport(repapp.ob, "no")
});
//set inactive bring up dialog view 
$('body').on("click", repapp.dom.Btn.setinactive, () => {
    if (HandsOnGetrowsSelected(repapp.hot) === undefined) { return showAjaxReturnMessage("select at least one row!", "w") }
    var dlg = new DialogBox(prop.language["Retention"], repapp.url.GetInactivePullList, prop.Dialogbox.Type.LoadPartialView)
    dlg.ShowDialog().then((data) => {
        if (data.isError) {
            showAjaxReturnMessage(data.Msg, "e");
            $(prop.SpinningWheel.Main).hide();
            return;
        }
    });

});
//set inactive submit button
$('body').on("click", repapp.dom.setInactive.btnsetinactive, () => {
    if (HandsOnGetrowsSelected(repapp.hot) === undefined) { return showAjaxReturnMessage("select at least one row!", "w") }
    var m = new BtnSetInactive()
    m.SubmitInactive();
});
//ceretificate dll for disposition
$('body').on('change', repapp.dom.Btn.DDLCertifiedPosionSelected, (e) => {
    repapp.ob.reportType = repapp.enum.RetentionCertifieDisposition;
    repapp.ob.pageNumber = 1;
    repapp.ob.id = e.currentTarget.value;
    repapp.ob.isQueryFromDDL = true;
    retentionreport.RunRetentionReport(repapp.ob, "no").then(() => {
        repapp.ob.isQueryFromDDL = false;
    })
});
//retention final disposition
////submit view, bring up popup window
$('body').on("click", repapp.dom.Btn.SubmitFordestructionPopup, () => {
    if (HandsOnGetrowsSelected(repapp.hot) === undefined) { return showAjaxReturnMessage("Please select a record/s for submitting to disposition", "w") }
    var dlg = new DialogBox(prop.language["Retention"], repapp.url.GetSubmitForm + "/?submitType=destruction", prop.Dialogbox.Type.LoadPartialView)
    dlg.ShowDialog().then((data) => {
        if (data.isError) {
            showAjaxReturnMessage(rdata.Msg, "e");
            $(prop.SpinningWheel.Main).hide();
            return;
        }
    });
});

$('body').on("click", repapp.dom.Btn.SubmitForPurgePopup, () => {
    if (HandsOnGetrowsSelected(repapp.hot) === undefined) { return showAjaxReturnMessage("Please select a record/s for submitting to disposition", "w") }
    var dlg = new DialogBox(prop.language["Retention"], repapp.url.GetSubmitForm + "/?submitType=purge", prop.Dialogbox.Type.LoadPartialView)
    dlg.ShowDialog().then((data) => {
        if (data.isError) {
            showAjaxReturnMessage(rdata.Msg, "e");
            $(prop.SpinningWheel.Main).hide();
            return;
        }
    });
});

$('body').on("click", repapp.dom.Btn.SubmitForArchivePopup, () => {
    if (HandsOnGetrowsSelected(repapp.hot) === undefined) { return showAjaxReturnMessage("Please select a record/s for submitting to disposition", "w") }
    var dlg = new DialogBox(prop.language["Retention"], repapp.url.GetSubmitForm + "/?submitType=archived", prop.Dialogbox.Type.LoadPartialView)
    dlg.ShowDialog().then(() => { });
});

$('body').on("click", repapp.dom.SubmitPopup.btnSubmit, (e) => {
    if (HandsOnGetrowsSelected(repapp.hot) === undefined) { return showAjaxReturnMessage("select at least one row!", "w") }
    var m = new BtnSubmitReport(e.currentTarget.getAttribute("dataset.submittype"));
    switch (e.currentTarget.getAttribute("dataset.submittype")) {
        case "destruction":
            m.SubmitReport();
            break;
        case "purge":
            m.SubmitReport();
            break;
        case "archive":
            m.SubmitReport();
            break;
        default:
    }
});

//create new reports: purge. destruction and archived
$('body').on("click", repapp.dom.Btn.NewpurgeReport, () => {
    var call = new DataAjaxCall(repapp.url.BtnCreateNewPurged, prop.ajax.Type.Get, prop.ajax.DataType.Json, "", "", "", "", "");
    call.Send().then((rdata) => {
        if (rdata.isError) {
            showAjaxReturnMessage(rdata.Msg, 'e')
            return;
        }
        if (rdata.iscreated === 0) {
            showAjaxReturnMessage("No data available for creating Purge report", 'w')
        } else {
            repapp.dom.CurrentMenuElemnt.click();
        }
    });
});

$('body').on("click", repapp.dom.Btn.NewDestructionReport, () => {
    var call = new DataAjaxCall(repapp.url.BtnCreateNewDestruction, prop.ajax.Type.Get, prop.ajax.DataType.Json, "", "", "", "", "");
    call.Send().then((rdata) => {
        if (rdata.errortype === 'e') {
            showAjaxReturnMessage(rdata.Msg, 'e')
            return;
        }
        if (rdata.iscreated === 0) {
            showAjaxReturnMessage("No data available for creating Destruction report", 'w')
        } else {
            repapp.dom.CurrentMenuElemnt.click();
        }

    });
});
$('body').on("click", repapp.dom.Btn.NewpermanentActive, () => {
    var call = new DataAjaxCall(repapp.url.BtnCreateNewArchived, prop.ajax.Type.Get, prop.ajax.DataType.Json, "", "", "", "", "");
    call.Send().then((rdata) => {

        if (rdata.errortype === 'e') {
            showAjaxReturnMessage(rdata.Msg, 'e')
            return;
        }
        if (rdata.iscreated === 0) {
            showAjaxReturnMessage("No data available for creating Archive report", 'w')
        } else {
            repapp.dom.CurrentMenuElemnt.click();
        }

    });
});
$('body').on("change", repapp.dom.Btn.DDLfinaldisposition, (e) => {
    var btnDisposition = document.getElementsByName("dispositionType")
    //run finaldisposition method to get the new Id and text 
    var getType = e.currentTarget.options[e.currentTarget.options.selectedIndex].text.split(" ");
    retentionreport.FinalDispositionType(getType[getType.length - 1])
    btnDisposition[0].value = retentionreport.dispositionText;
    btnDisposition[0].id = retentionreport.dispositionid;
    //set up properties
    repapp.ob.reportType = repapp.enum.RetentionFinalDisposition;
    repapp.ob.pageNumber = 1;
    repapp.ob.id = e.currentTarget.value;
    repapp.ob.isQueryFromDDL = true;
    retentionreport.RunRetentionReport(repapp.ob, 0).then(() => {
        repapp.ob.isQueryFromDDL = false;
    })
});

$('body').on("click", repapp.dom.Btn.btnleftButton, (e) => {
    if (repapp.ob.reportType == repapp.enum.CustomReport) {
        $("#btnExportCSVAllReport").removeClass("disableAllCsvBtn")
    } else {
        $("#btnExportCSVAllReport").addClass("disableAllCsvBtn")
    }
})

//open report directly from main grid
function OpenReportDirectly(passTypeOfReport) {
    var menu = document.querySelector("#mCSB_1_container")
    switch (passTypeOfReport) {
        case 'newRequest':
            var reqElem = document.getElementById("newrequest");
            menu.children[0].children[1].children[0].children[1].children[3].children[0].click()
            reports.RequestorReport(reqElem, 'new', 1)
            break;
        case "exceptions":
            var reqElem = document.getElementById("exceptions");
            menu.children[0].children[1].children[0].children[1].children[3].children[0].click()
            reports.RequestorReport(reqElem, 'exception', 1)
            break;
        default:
    }
}

//audit report events
var auditEvents = {
    BtnOkClick: (event) => {
        if (auditEvents.Validation() === 0) {
            reports.RunAuditReport();
        }
    },
    userChange: () => {
        auditEvents.CheckBoxConditions();
    },
    ObjectChange: (event) => {
        auditEvents.CheckBoxConditions();
        var txtid = document.querySelector(repapp.dom.AuditReport.txtObjectId)
        if (event.options[event.selectedIndex].dataset.isIdstring == 'false') {
            txtid.type = 'number';
        } else {
            txtid.type = 'text';
        }
    },
    CheckBoxConditions: (event) => {
        var m = auditEvents.properties();
        if (m.UserName.value === "-1" && m.ObjectId.value === "-1") {
            m.SuccessLogin.disabled = false;
            m.FailedLogin.disabled = false;
            m.AddEditDelete.disabled = true;
            m.AddEditDelete.checked = false;
            m.ConfDataAccess.disabled = true;
            m.ConfDataAccess.checked = false;
            m.ChildTable.disabled = true;
            m.ChildTable.checked = false;
            m.Id.value = "";
            m.Id.disabled = true;
        } else if (m.UserName.value !== "-1" && m.ObjectId.value === "-1") {
            m.SuccessLogin.disabled = false;
            m.FailedLogin.disabled = false;
            m.AddEditDelete.disabled = false;
            m.ConfDataAccess.disabled = false;
            m.ChildTable.disabled = false;
            m.Id.value = "";
            m.Id.disabled = true;
        } else if (m.UserName.value === "-1" && m.ObjectId.value !== "-1" && parseInt(m.ObjectId.value.split("|")[1]) === 0) {
            m.SuccessLogin.disabled = true;
            m.FailedLogin.disabled = true;
            m.AddEditDelete.disabled = false;
            m.ConfDataAccess.disabled = false;
            m.ChildTable.disabled = true;
            m.ChildTable.checked = false;
            m.Id.disabled = false;
        } else if (m.UserName.value === "-1" && m.ObjectId.value !== "-1" && parseInt(m.ObjectId.value.split("|")[1]) > 0) {
            m.SuccessLogin.disabled = true;
            m.SuccessLogin.checked = false;
            m.FailedLogin.disabled = true;
            m.FailedLogin.checked = false;
            m.AddEditDelete.disabled = false;
            m.ConfDataAccess.disabled = false;
            m.ChildTable.disabled = false;
            m.Id.disabled = false;
        } else if (m.UserName.value !== "-1" && m.ObjectId.value !== "-1" && parseInt(m.ObjectId.value.split("|")[1]) > 0) {
            m.SuccessLogin.disabled = true;
            m.SuccessLogin.checked = false;
            m.FailedLogin.disabled = true;
            m.FailedLogin.checked = false;
            m.AddEditDelete.disabled = false;
            m.ConfDataAccess.disabled = false;
            m.ChildTable.disabled = false;
            m.Id.disabled = false
        } else if (m.UserName.value !== "-1" && m.ObjectId.value !== "-1" && parseInt(m.ObjectId.value.split("|")[1]) === 0) {
            m.SuccessLogin.disabled = true;
            m.SuccessLogin.checked = false;
            m.FailedLogin.disabled = true;
            m.FailedLogin.checked = false;
            m.AddEditDelete.disabled = false;
            m.ConfDataAccess.disabled = false;
            m.ChildTable.disabled = true;
            m.ChildTable.checked = false;
            m.Id.disabled = false;
        } else if (m.ObjectId.value == "All") {
            m.SuccessLogin.disabled = true;
            m.SuccessLogin.checked = false;
            m.FailedLogin.disabled = true;
            m.FailedLogin.checked = false;
            m.AddEditDelete.disabled = false;
            m.ConfDataAccess.disabled = false;
            m.ChildTable.disabled = false;
            m.Id.disabled = true;
        }

        m.Id.setAttribute("placeholder", "");
        m.Id.style.border = "1px solid black";
    },
    Validation() {
        var isNotValid = 0;
        var m = auditEvents.properties();
        //checkboxes
        var checkbox = [m.SuccessLogin.checked, m.FailedLogin.checked, m.ConfDataAccess.checked, m.AddEditDelete.checked, m.ChildTable.checked];
        if (!checkbox.includes(true)) {
            document.querySelector(repapp.dom.AuditReport.CheckboxMsg).innerHTML = prop.language["msgSelectchkAuditReport"];
            isNotValid++;
        } else if (m.ChildTable.checked && m.AddEditDelete.checked === false && m.ConfDataAccess.checked === false){
            document.querySelector(repapp.dom.AuditReport.CheckboxMsg).innerHTML = prop.language["msgSelectchkChildTable"];
            isNotValid++;
        } else {
            document.querySelector(repapp.dom.AuditReport.CheckboxMsg).innerHTML = "";
        }
        //checkbox childtable if checked it must checked with add/edit/delete or confidential data access checkbox.
        //if (m.ChildTable.checked && m.AddEditDelete.checked === false && m.ConfDataAccess.checked === false) {
        //    document.querySelector(repapp.dom.AuditReport.CheckboxMsg).innerHTML = prop.language["msgSelectchkChildTable"];
        //    isNotValid++;
        //} else {
        //    document.querySelector(repapp.dom.AuditReport.CheckboxMsg).innerHTML = "";
        //}

        //dateStart
        if (m.StartDate.value === "") {
            m.StartDate.parentElement.parentElement.children[0].firstElementChild.style.display = "initial";
            isNotValid++;
        } else {
            m.StartDate.parentElement.parentElement.children[0].firstElementChild.style.display = "none";
        }
        //dateEnd
        if (m.EndDate.value === "") {
            m.EndDate.parentElement.parentElement.children[0].firstElementChild.style.display = "initial";
            isNotValid++;
        } else {
            m.EndDate.parentElement.parentElement.children[0].firstElementChild.style.display = "none";
        }
        //id text

        //if (m.ObjectId.value !== "-1" && m.Id.value === "") {
        //    m.Id.setAttribute("placeholder", "Required!");
        //    m.Id.style.border = "2px solid red";
        //    isNotValid++
        //} else {
        //    m.Id.setAttribute("placeholder", "");
        //    m.Id.style.border = "1px solid black";
        //}

        return isNotValid;
    },
    properties: () => {
        var model = {};
        model.UserName = document.querySelector(repapp.dom.AuditReport.ddlUser);
        model.ObjectId = document.querySelector(repapp.dom.AuditReport.ddlObject);
        model.Id = document.querySelector(repapp.dom.AuditReport.txtObjectId);
        model.StartDate = document.querySelector(repapp.dom.AuditReport.dtStartDate);
        model.EndDate = document.querySelector(repapp.dom.AuditReport.dtEndDate);
        model.AddEditDelete = document.querySelector(repapp.dom.AuditReport.chkAddEditDelL);
        model.SuccessLogin = document.querySelector(repapp.dom.AuditReport.chkSuccessLogin);
        model.ConfDataAccess = document.querySelector(repapp.dom.AuditReport.chkConfidential);
        model.FailedLogin = document.querySelector(repapp.dom.AuditReport.chkFailedLogin);
        model.ChildTable = document.querySelector(repapp.dom.AuditReport.chkChildTable);
        return model;
    }
}

//reports functions 
class Reports {
    constructor() {
        this.title = "";
        this.text = "";
        this.ViewId = 0;
        this.pageNum = 0;
        this.CurrentElem = "";
        this.isNewrequestReport = false;
        this.TableName = "";
        this.ViewName = "";
        this.IsBackgroundProcessing = false;
    }
    LoadCustomReport(elem, viewid) {
        var _this = this;
        _this.MarkSelctedMenu(elem);
        _this.CurrentElem = elem;
        _this.ViewId = viewid;
        var q = new reportsQuery(viewid);
        q.LoadQueryWindow(-1).then((data) => { });
    }
    GenerateCustomeReportHeader(bobj, headerList, data) {
        var cell = this.StartInCell(data)
        bobj.ListOfHeader = [];
        for (var i = 0; i < headerList.length; i++) {
            if (i >= cell) {
                bobj.ListOfHeader.push(headerList[i].HeaderName);
            }
        }
    }
    GenerateCustomeReportRows(bobj, rowsList, data) {
        var cell = this.StartInCell(data)
        bobj.ListOfRows = [];
        for (var i = 0; i < rowsList.length; i++) {
            var rowbuild = [];
            for (var j = 0; j < rowsList[i].length; j++) {
                if (j >= cell) {
                    rowbuild.push(rowsList[i][j])
                }
            }
            bobj.ListOfRows.push(rowbuild);
        }
    }
    RunCustomReport(pageNum) {
        var _this = this;
        _this.isNewrequestReport = false;
        _this.pageNum = pageNum;
        repapp.ob.pageNumber = pageNum
        repapp.ob.reportType = repapp.enum.CustomReport
        _this.CreateBaiscButtons(repapp.ob.reportType);
        $(prop.SpinningWheel.Main).show();
        var data = JSON.stringify({ paramss: _this, searchQuery: repapp.dom.QueryWindow.buildQuery });
        var call = new DataAjaxCall(repapp.url.RunQuery, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "", "")
        call.Send().then((data) => {
            if (data.isError === false) {
                this.TableName = data.TableName;
                this.ViewName = data.ViewName;
                repapp.data = data;
                document.querySelector(repapp.dom.ReportTitle).innerHTML = _this.CurrentElem.innerHTML;
                var buildObj = {};
                reports.GenerateCustomeReportHeader(buildObj, data.ListOfHeaders, data);
                reports.GenerateCustomeReportRows(buildObj, data.ListOfDatarows, data);
                HandsOnTableReports.buildHandsonTable(buildObj);
                $(prop.SpinningWheel.Main).hide();
                pagingsectionObj.PerPageRecords = parseInt(data.RowPerPage) == 0 ? 100 : parseInt(data.RowPerPage);
                pagingsectionObj.SelectedIndex = data.PageNumber
                this.LoadCustomReportPaging();
            }
            else {
                $(prop.SpinningWheel.Main).hide();
                showAjaxReturnMessage(data.Msg, 'e');
            }
        });
    }
    // this function are using only table data
    RunCustomReportHandsonData(pageNum) {
        var _this = this;
        _this.pageNum = pageNum;
        $(prop.SpinningWheel.Main).show();
        var data = JSON.stringify({ paramss: _this, searchQuery: repapp.dom.QueryWindow.buildQuery });
        var call = new DataAjaxCall(repapp.url.RunQuery, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "", "")
        call.Send().then((data) => {
            $(prop.SpinningWheel.Main).hide();
            if (data.isError === false) {
                var buildObj = {};
                reports.GenerateCustomeReportHeader(buildObj, data.ListOfHeaders, data);
                reports.GenerateCustomeReportRows(buildObj, data.ListOfDatarows, data);
                HandsOnTableReports.buildHandsonTable(buildObj);
            }
            else {
                showAjaxReturnMessage(data.Msg, 'e');
            }
        });
    }
    LoadCustomReportPaging() {
        var _this = this
        var data = JSON.stringify({ paramsUI: _this, searchQuery: repapp.dom.QueryWindow.buildQuery })
        var call = new DataAjaxCall(repapp.url.GetTotalrowsForGrid, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "");
        call.Send().then((total) => {
            //if (rdata.isError) {
            //    showAjaxReturnMessage(rdata.Msg, "e");
            //    $(prop.SpinningWheel.Main).hide();
            //    return;
            //}

            try {
                var inf = total.split('|')
                pagingsectionObj.TotalRecords = parseInt(inf[0]);
                pagingsectionObj.PageSize = parseInt(inf[0]) > 0 && parseInt(inf[1]) == 0 ? 1 : parseInt(inf[1])
                pagingsectionObj.SetTextFieldValues()
                pagingsectionObj.VisibleIndexs()
            } catch (ex) {
                console.log(ex)
            }
        });
    }
    //Audit report
    LoadAuditReport(elem) {
        reports.MarkSelctedMenu(elem)
        var dlg = new DialogBoxAudit(prop.language["mnuWorkGroupMenuControlAuditReport"], repapp.url.GetauditReportView, prop.DialogboxAudit.Type.LoadPartialView);
        DestroyPickDays();
        dlg.ShowDialog().then((istrue) => {

        });
    }
    RunAuditReport() {
        this.isNewrequestReport = false;
        this.CreateBasicButtonAndFilter();
        var model = auditEvents.properties();
        var m = {};
        m.UserName = model.UserName.options[model.UserName.selectedIndex].innerText.trim();
        m.UserDDLId = model.UserName.value;
        m.ObjectId = model.ObjectId.value;
        m.ObjectName = model.ObjectId.options[model.ObjectId.selectedIndex].innerText.trim();
        m.SuccessLogin = model.SuccessLogin.checked;
        m.FailedLogin = model.FailedLogin.checked;
        m.AddEditDelete = model.AddEditDelete.checked;
        m.ChildTable = model.ChildTable.checked;
        m.ConfDataAccess = model.ConfDataAccess.checked;
        m.EndDate = model.EndDate.value;
        m.StartDate = model.StartDate.value;
        m.Id = model.Id.value;
        m.PageNumber = 1
        repapp.ob.reportType = repapp.enum.AuditReport
        //run the spinner
        $(prop.SpinningWheel.Main).show();

        //var data = JSON.stringify({ params: m });
        var data = JSON.stringify(m);
        repapp.auditObj = m
        var call = new DataAjaxCall(repapp.url.RunAuditSearch, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                repapp.data = rdata
                $(prop.Dialogbox.DialogboxAuditid).hide()
                document.querySelector(repapp.dom.ReportTitle).innerHTML = prop.language["mnuWorkGroupMenuControlAuditReport"]
                document.querySelector(repapp.dom.itemDescription).innerHTML = rdata.SubTitle;
                HandsOnTableReports.buildHandsonTable(rdata);
                $(prop.SpinningWheel.Main).hide();
                reports.RunAuditReportPagination();
                $(prop.DialogboxAudit.DialogboxAuditid).hide();
            } else {
                $(prop.SpinningWheel.Main).hide();
                showAjaxReturnMessage(rdata.Msg, "e");
            }
        });
    }
    LoadAuditfilter() {
        $(prop.DialogboxAudit.DialogboxAuditid).show();
    }
    RunAuditReportHandsondataData(pageNumber) {
        repapp.auditObj.PageNumber = pageNumber
        repapp.ob.reportType = repapp.enum.AuditReport
        //run the spinner
        $(prop.SpinningWheel.Main).show();
        var data = JSON.stringify(repapp.auditObj);
        var call = new DataAjaxCall(repapp.url.RunAuditSearch, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                repapp.data = rdata;
                HandsOnTableReports.buildHandsonTable(rdata);
                $(prop.SpinningWheel.Main).hide();
            } else {
                $(prop.SpinningWheel.Main).hide();
                showAjaxReturnMessage(rdata.Msg, "e");
            }
        });
    }
    RunAuditReportPagination() {
        $(prop.SpinningWheel.Main).show();
        return new Promise((resolve, reject) => {
            var data = JSON.stringify(repapp.auditObj); 
            var call = new DataAjaxCall(repapp.url.RunAuditReportPagination, prop.ajax.Type.Post, prop.ajax.DataType.Html, data, prop.ajax.ContentType.Utf8, "", "", "", "");
            call.Send().then((rdata) => {

                if (rdata.isError) {
                    showAjaxReturnMessage(rdata.Msg, "e");
                    $(prop.SpinningWheel.Main).hide();
                    reject();
                }

                $('#paging-container').html(rdata);
                $(prop.SpinningWheel.Main).hide();
                resolve(rdata);
            }).catch((ex) => {
                alert(ex)
                $(prop.SpinningWheel.Main).hide();
                reject();
            });
        });
    }
    //tracking report
    TrackableReport(elem, report, pageNumber) {
        this.isNewrequestReport = false;
        $(prop.SpinningWheel.Main).show();
        reports.MarkSelctedMenu(elem);
        repapp.ob.pageNumber = pageNumber
        switch (report) {
            case 0:
                repapp.ob.reportType = repapp.enum.PastDueTrackableItemsReport;
                this.RunReport(repapp.ob, "no")
                break;
            case 1:
                repapp.ob.reportType = repapp.enum.ObjectOut;
                this.RunReport(repapp.ob, "no");
                break;
            default:
                repapp.ob.reportType = repapp.enum.ObjectsInventory;
                repapp.ob.tableName = report;
                this.RunReport(repapp.ob, "no");
        }
        reports.CreateBaiscButtons(repapp.ob.reportType);
    }
    //Requestor report
    RequestorReport(elem, report, pageNumber) {
        repapp.dom.CurrentMenuElemnt = elem;
        this.MarkSelctedMenu(elem);
        this.isNewrequestReport = false;
        repapp.ob.pageNumber = pageNumber
        $(repapp.dom.btnContainer).trigger("click")
        switch (report) {
            case "new":
                this.RequestNew();
                break;
            case "newbatch":
                repapp.ob.reportType = repapp.enum.RequestNewBatch;
                repapp.ob.isQueryFromDDL = false;
                this.RequestNewBatch(repapp.ob);
                break;
            case "pulllist":
                repapp.ob.reportType = repapp.enum.RequestPullList;
                repapp.ob.isQueryFromDDL = false;
                this.PullList(repapp.ob);
                break;
            case "exception":
                repapp.ob.reportType = repapp.enum.RequestException;
                this.RunReport(repapp.ob, "no")
                break;
            case "inprocess":
                repapp.ob.reportType = repapp.enum.RequestInProcess
                this.RunReport(repapp.ob, "no")
                break;
            case "waitlist":
                repapp.ob.reportType = repapp.enum.RequestWaitList;
                this.RunReport(repapp.ob, "no")
                break;
            default:
        }
        this.CreateBaiscButtons(repapp.ob.reportType);
    }
    RequestNew() {
        //this variable created to indicate if the button send item to pull comes from batch or new request 
        this.isNewrequestReport = true;
        repapp.ob.reportType = repapp.enum.RequestNew;
        this.RunReport(repapp.ob, 0).then(() => {
            if (repapp.hot !== "") {
                if (repapp.hot.countRows() !== 0) {
                    $(repapp.dom.btnleft).append(` <input type="button" id="sendcheckedItems" class="btn btn-secondary tab_btn" value="${prop.language["lblHTMLReportsSendCheckedItmLst"]}" style="min-width: 70px;">`)
                } else {
                    $(repapp.dom.ReportTitle).text(prop.language["titleNoNewRequestReports"])
                }
            }
        })
    }
    RequestNewBatch(ob) {
        this.RunReport(ob, 0).then((data) => {
            if (repapp.hot !== "") {
                if (repapp.hot.countRows() !== 0) {
                    $(repapp.dom.btnleft).append(` <input type="button" id="sendcheckedItems" class="btn btn-secondary tab_btn" value="${prop.language["lblHTMLReportsSendCheckedItmLst"]}" style="min-width: 70px;">`)
                    this.BindDropDowntoDom(data, "DDLRequestSelected")
                } else {
                    $(repapp.dom.ReportTitle).text(prop.language["titleNoBatchRequestReports"])
                }
            }
        })
    }
    PullList(ob) {
        this.RunReport(ob, "no").then((data) => {
            if (repapp.hot !== "") {
                if (repapp.hot.countRows() !== 0 || data.ddlSelectReport.length !== 0) {
                    this.BindDropDowntoDom(data, "DDLRequestSelected")
                }
            }
        })
    }
    BindDropDowntoDom(data, id) {
        //var createHtml = `<div class="col-md-7"><div style="display:-webkit-inline-box; padding-left:90px;"><label style="margin-top: 8px;">Select List: </label><select id="${id}" class="form-control" style="width:100%; width:425px;">`
        var createHtml = `<div class="col-md-7" style="padding:0"><div style="float:right;"><label style="margin-top: 8px;width:104px">Select List: </label><select id="${id}" class="form-control" style="width:100%;">`
        data.ddlSelectReport.forEach((v, i) => {
            if (id === "DDLRequestSelected") {
                var counter = data.ddlSelectReport.length - i;
                createHtml += `<option value="${v.value.trim()}">${counter}| ${v.text}</option>`
            } else {
                createHtml += `<option value="${v.value.trim()}">${v.Id}| ${v.text}</option>`
            }
        })
        $(repapp.dom.btnContainer).append(createHtml + "</div></div>")
    }
    RunReport(ob, isHiddenField) {
        $(prop.SpinningWheel.Main).show();
        return new Promise((resolve, reject) => {
            var data = JSON.stringify({ paramss: ob });
            var call = new DataAjaxCall(repapp.url.InitiateReports, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "", "");
            call.Send().then((rdata) => {
                if (rdata.isError === false) {
                    document.querySelector(repapp.dom.ReportTitle).innerHTML = rdata.lblTitle;
                    document.querySelector(repapp.dom.itemDescription).innerHTML = rdata.lblSubtitle;
                    HandsOnTableReports.buildHandsonTable(rdata, isHiddenField);
                    reports.InitiateReportsPagination()
                    $(prop.SpinningWheel.Main).hide();
                    repapp.data = rdata
                    resolve(rdata);
                } else {
                    $(prop.SpinningWheel.Main).hide();
                    showAjaxReturnMessage(rdata.Msg, "e");
                }
            }).catch(() => {
                $(prop.SpinningWheel.Main).hide();
                reject()
            })
        });
    }
    ResetPagination(obj) {
        pagingsectionObj.PerPageRecords = obj.PerPageRecord;
        pagingsectionObj.TotalRecords = obj.TotalRecord;
        pagingsectionObj.SelectedIndex = obj.PageNumber
        pagingsectionObj.PageSize = obj.TotalPage
        pagingsectionObj.SetTextFieldValues()
        pagingsectionObj.VisibleIndexs()
    }

    RunReportHandsondataData(ob, isHiddenField) {
        $(prop.SpinningWheel.Main).show();
        return new Promise((resolve, reject) => {
            var data = JSON.stringify({ paramss: ob });
            var call = new DataAjaxCall(repapp.url.InitiateReports, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "", "");
            call.Send().then((rdata) => {
                if (rdata.isError === false) {
                    HandsOnTableReports.buildHandsonTable(rdata, isHiddenField);
                    $(prop.SpinningWheel.Main).hide();
                    resolve(rdata);
                } else {
                    $(prop.SpinningWheel.Main).hide();
                    showAjaxReturnMessage(rdata.Msg, "e");
                }
            }).catch(() => {
                $(prop.SpinningWheel.Main).hide();
                reject()
            })
        });
    }

    //this function are using for load only pagination for Retention Report
    InitiateReportsPagination() {
        $(prop.SpinningWheel.Main).show();
        return new Promise((resolve, reject) => {
            var data = JSON.stringify({ paramss: repapp.ob });
            var call = new DataAjaxCall(repapp.url.InitiateReportsPagination, prop.ajax.Type.Post, prop.ajax.DataType.Html, data, prop.ajax.ContentType.Utf8, "", "", "", "");
            call.Send().then((rdata) => {

                if (rdata.isError) {
                    showAjaxReturnMessage(rdata.Msg, "e");
                    $(prop.SpinningWheel.Main).hide();
                    reject();
                }

                $('#paging-container').html(rdata);
                $(prop.SpinningWheel.Main).hide();
                resolve(rdata);
            }).catch((ex) => {
                alert(ex)
                $(prop.SpinningWheel.Main).hide();
                reject();
            });
        });
    }

    //mark selected row
    MarkSelctedMenu(elemnt) {
        //select the menu clicked.
        $('.divMenu').find('a.selectedMenu').removeClass('selectedMenu');
        elemnt.className = 'selectedMenu';
    }
    StartInCell(data) {
        var cell = 0; //never be zero
        if (data.HasAttachmentcolumn && data.HasDrillDowncolumn) {
            cell = 3;
        } else if (data.HasAttachmentcolumn === false && data.HasDrillDowncolumn === false) {
            cell = 1;
        } else if (data.HasAttachmentcolumn === true && data.HasDrillDowncolumn === false) {
            cell = 2;
        } else if (data.HasAttachmentcolumn === false && data.HasDrillDowncolumn === true) {
            cell = 2
        }
        return cell;
    }
    CreateBaiscButtons(reportType) {
        $(repapp.dom.btnContainer).html("");
        var createHtml = "";
        if (reportType == repapp.enum.CustomReport) {
            createHtml = `<div id="btnleft" class="col-md-5" style="padding:0;"><div class="btn-group">
            <button class="btn btn-secondary dropdown-toggle tab_btn" data-toggle="dropdown" type="button" aria-expanded="false">
                <i class="fa fa-file-text-o fa-fw"></i>
                <i class="fa fa-angle-down"></i>
            </button>
            <ul class="dropdown-menu btn_menu">
                <li><a id="btnprint">Print</a></li>
                <li><a id="btnExportCSVReport">Export All Rows(CSV)</a></li>
            </ul>
        </div></div>`
        } else {
            createHtml = `<div id="btnleft" class="col-md-5" style="padding:0;"><div class="btn-group">
            <button class="btn btn-secondary dropdown-toggle tab_btn" data-toggle="dropdown" type="button" aria-expanded="false">
                <i class="fa fa-file-text-o fa-fw"></i>
                <i class="fa fa-angle-down"></i>
            </button>
            <ul class="dropdown-menu btn_menu">
                <li><a id="btnprint">Print</a></li>
                <li><a id="btnExportCSVReport">Export Current Page(csv)</a></li>
            </ul>
        </div></div>`
        }
        $(repapp.dom.btnContainer).append(createHtml)
    }
    CreateBasicButtonAndFilter() {
        $(repapp.dom.btnContainer).html("");
        var createhtml = `<div id="btnContainer" class="col-md-12" style="margin-top:30px; margin-left:-15px;">
                          <button class="btn btn-secondary dropdown-toggle tab_btn" data-toggle="dropdown" type="button" aria-expanded="false">
                              <i class="fa fa-file-text-o fa-fw"></i>
                              <i class="fa fa-angle-down"></i>
                          </button>
                          <ul class="dropdown-menu btn_menu">
                              <li><a id="btnprint">Print</a></li>
                              <li><a id="btnExportCSVReport">Export Current Page(csv)</a></li>
                          </ul>
                          <div class="btn-group">
                             <button onclick="reports.LoadAuditfilter(this)" style="width:56px;height:34px" title="Filters" class="fa fa-filter btn btn-default"></button>
                          </div></div>`
        $(repapp.dom.btnContainer).append(createhtml)
    }
    SendcheckedItemsToPullList() {
        var items = []
        for (var i = 0; i < HandsOnGetrowsSelected(repapp.hot).length; i++) {
            var row = HandsOnGetrowsSelected(repapp.hot)[i]
            items.push({ tableid: repapp.hot.getDataAtCell(row, 0).split("||")[0], tableName: repapp.hot.getDataAtCell(row, 0).split("||")[1] })
        }
        var obs = {};
        obs.ListofPullItem = items;
        obs.id = document.querySelector(repapp.dom.Btn.DDLRequestSelected) == null ? "" : document.querySelector(repapp.dom.Btn.DDLRequestSelected)[0].value;
        obs.isBatchRequest = this.isNewrequestReport ? false : true;
        var data = JSON.stringify({ params: obs });
        var call = new DataAjaxCall(repapp.url.BtnSendRequestToThePullList, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                //click on pull list: if it is a new request report go down two childeren 
                if (this.isNewrequestReport) {
                    repapp.dom.CurrentMenuElemnt.parentElement.nextElementSibling.children[0].click()
                } else {
                    repapp.dom.CurrentMenuElemnt.parentElement.nextElementSibling.children[0].click()
                }
            } else {
                showAjaxReturnMessage(rdata.Msg, "e");
            }
        })
    }
}
class reportsQuery {
    constructor(viewid, viewtitle) {
        this.viewId = viewid;
        this.viewTitle = viewtitle;
        this.isQueryStoped = false;
    }
    LoadQueryWindow(IsmyQuerycall) {
        var _this = this;
        DestroyPickDays();
        return new Promise((resolve, reject) => {
            var data = { viewId: this.viewId, ceriteriaId: IsmyQuerycall };
            var call = new DataAjaxCall(repapp.url.LoadQueryWindow, prop.ajax.Type.Get, prop.ajax.DataType.Html, data, "", "", "", "");
            call.Send().then((data) => {
                if (data.isError) {
                    showAjaxReturnMessage(data.Msg, "e");
                    $(prop.SpinningWheel.Main).hide();
                    reject();
                }
                var dlg = new DialogBox("Query", data, prop.Dialogbox.Type.PartialView);
                dlg.ShowDialog().then(() => {
                    repapp.dom.QueryWindow.SaveContent = data;
                    resolve(data);
                    GetPickaDayCalendar(dateFormat)
                });
            })
        })
    }
    ShowPopupWindow() {
        $(repapp.dom.QueryWindow.MainDialogQuery).show();
        $('.modal-dialog').draggable({
            handle: ".modal-header",
            stop: function (event, ui) {
            }
        });
        //arrow condition up and down.
        if ($("#arrowUpDown").hasClass("fa fa-angle-down")) {
            $("#arrowUpDown").removeClass("fa fa-angle-down");
            $("#arrowUpDown").addClass("fa fa-angle-up");
        }
        //setup checkbox basic query
        var check = document.querySelector(repapp.dom.QueryWindow.ChekBasicQuery);
        this.StartQuery = getCookie('startQuery');
        if (this.StartQuery == "True") {
            check.checked = true;
        } else {
            check.checked = false;
        }
        //create my query events
        obJmyquery.CreateMyQueryEvents();
    }
    QueryOkButton() {
        this.SubmmitQuery();
        document.querySelector(repapp.dom.itemDescription).innerHTML = "";
        if (this.isQueryStoped) {
            this.isQueryStoped = false;
            return;
        } else {
            $(prop.Dialogbox.Dialogboxid).hide();
        }
    }
    QueryApplybutton() {
        this.SubmmitQuery();
    }
    SubmmitQuery(isSaveQueryCall = false) {
        var _this = this;
        repapp.dom.QueryWindow.buildQuery = [];
        var table = document.querySelector(repapp.dom.QueryWindow.Querytableid);
        for (var i = 0; i < table.rows.length; i++) {
            var controlerType = table.rows[i].children[2].children[0].type;
            var columnName = table.rows[i].cells[0].getAttribute("columnname");
            var ColumnType = table.rows[i].cells[0].getAttribute("datatype");
            var operators = table.rows[i].cells[1].children[0].value;
            var values = table.rows[i].cells[2].children[0].value;

            if (operators == "Between") {
                var start = table.rows[i].cells[2].children[0].value;
                var end = table.rows[i].cells[2].children[1].value;
                values = start + "|" + end;
            } else if (controlerType == "select-one") {
                var selected = table.rows[i].children[2].children[0].selectedIndex;
                values = table.rows[i].children[2].children[0].item(selected).value;
            } else if (table.rows[i].children[2].children[0].className == "modal-checkbox") {
                values = table.rows[i].children[2].children[0].checked.toString().toLowerCase();
            } else {
                values = table.rows[i].cells[2].children[0].value;
            }
            repapp.dom.QueryWindow.buildQuery.push({ columnName: columnName, ColumnType: ColumnType, operators: operators, values: values, islevelDownProp: false });
        }
        //check condition before apply the search
        if (_this.CheckQueryConditions() > 0) {
            _this.isQueryStoped = true;
            return;
        } else {
            _this.isQueryStoped = false;
        }

        reports.RunCustomReport(1);
    }
    CheckQueryConditions() {
        var _this = this;
        var errCounter = 0;
        var table = document.querySelector("#querytableid")
        for (var i = 0; i < table.rows.length; i++) {
            var dataType = table.rows[i].children[0].attributes.datatype.value;
            var isBetween = table.rows[i].children[1].children[0].value
            //if is in between
            if (isBetween === "Between") {
                var startInput = table.rows[i].children[2].children[0]
                var endInput = table.rows[i].children[2].children[1]
                //if is number
                if (_this.CheckBetweeinInteger(dataType, startInput, endInput) > 0) {
                    errCounter++;
                }
                //if is date
                if (_this.CheckBetweenDates(dataType, startInput, endInput) > 0) {
                    errCounter++;
                }
            }

            if (dataType === "System.DateTime" && isBetween !== "Between") {
                var dateField = table.rows[i].children[2].children[0]
                if (dateField.value !== "" && dateField.value !== " ") {
                    if (!checkDateFormat(dateField.value, dateFormat)) {
                        dateField.style.border = "1px solid red";
                        errCounter++
                    } else {
                        dateField.style.border = "";
                    }
                }
            }
        }
        return errCounter;
    }
    CheckBetweeinInteger(dataType, startInput, endInput) {
        var counter = 0;
        if (dataType === "System.Int32" || dataType === "System.Int16" || dataType === "System.Double") {
            if (startInput.value === '') {
                startInput.style.border = "1px solid red";
                counter++
            } else {
                if (dataType === "System.Int32" || dataType === "System.Int16" || dataType === "System.Double") {
                    startInput.style.border = "";
                }
            }

            if (endInput.value === '') {
                endInput.style.border = "1px solid red";
                counter++
            } else {
                if (dataType === "System.Int32" || dataType === "System.Int16" || dataType === "System.Double") {
                    endInput.style.border = "";
                }
            }
        }
        return counter;
    }
    CheckBetweenDates(dataType, startInput, endInput) {
        var counter = 0;

        if (dataType === "System.DateTime" && !checkDateFormat(startInput.value, dateFormat)) {
            startInput.style.border = "1px solid red";
            counter++
        } else {
            if (dataType === "System.DateTime") {
                startInput.style.border = "";
            }
        }

        if (dataType === "System.DateTime" && !checkDateFormat(endInput.value, dateFormat)) {
            endInput.style.border = "1px solid red";
            counter++
        } else {
            if (dataType === "System.DateTime") {
                endInput.style.border = "";
            }

        }
        return counter;
    }
    ClearInputs() {
        $(prop.Dialogbox.DlgBsContent).html("");
        $(prop.Dialogbox.DlgBsContent).html(repapp.dom.QueryWindow.SaveContent);
    }

    OperatorCondition(elem) {
        var dataType = elem.parentElement.parentElement.firstElementChild.getAttribute("datatype");
        switch (dataType) {
            case "System.String":
                break;
            case "System.Int32":
                this.OperatorNumericSetup(elem);
                break;
            case "System.Double":
                this.OperatorNumericSetup(elem);
                break;
            case "System.Boolean":
                break;
            case "System.DateTime":
                this.OperatorDateSetup(elem);
                break;
            default:
        }
    }
    OperatorNumericSetup(elem) {
        var OprValue = elem.options.item(elem.selectedIndex).value;
        if (OprValue == "Between") {
            elem.parentElement.nextElementSibling.innerHTML = `<input id="numberFiled_start" type="number" style="width:49%;margin-top:5px" placeholder="Start" type="text" class="form-control formWindowTextBox"><input id="numberFiled_end" type="number" style="display:block;width:49%;margin-left:3px;margin-top:5px" placeholder="End" class="form-control formWindowTextBox">`;
        } else {
            var input = document.getElementById("singelNumber");
            if (input === null) {
                elem.parentElement.nextElementSibling.innerHTML = `<input id="singelNumber" type="number" placeholder="" class="form-control">`;
            } else {
                if (input.value === "") {
                    elem.parentElement.nextElementSibling.innerHTML = `<input id="singelNumber" type="number" placeholder="" class="form-control">`;
                }
            }
        }
    }
    OperatorDateSetup(elem) {
        DestroyPickDays();
        var OprValue = elem.options.item(elem.selectedIndex).value;
        if (OprValue == "Between") {
            elem.parentElement.nextElementSibling.innerHTML = '<input id="dateFiled_start" name="tabdatepicker" style="width:49%;" autocomplete="off" placeholder="Start" type="text" onfocus="obJquerywindow.OperatorDateFocuse(this)" onblur="obJquerywindow.OperatorDateBlur(this)" class="form-control formWindowTextBox"><input id="dateFiled_end" autocomplete="off" name="tabdatepicker" style="display:block;width:49%;margin-left:3px;margin-top:5px" placeholder="End" onfocus="obJquerywindow.OperatorDateFocuse(this)" onblur="obJquerywindow.OperatorDateBlur(this)" class="form-control formWindowTextBox">';
        } else {
            elem.parentElement.nextElementSibling.innerHTML = '<input  id="dateFiled" autocomplete="off" name="tabdatepicker" class="form-control">';
        }
        GetPickaDayCalendar(dateFormat)
    }
    OperatorDateFocuse(elem) {
        //ClearPickDaysElements();
        // GetPickaDayCalendar(dateFormat)
        //elem.type = 'date';
        //elem.style.marginTop = "5px";
    }
    OperatorDateBlur(elem) {
        //elem.type = "text";
        //elem.value = elem.value;
    }
    CheckboxQueryClick(elem) {
        if (elem.checked == true) {
            setCookie('startQuery', "True");
        } else {
            setCookie('startQuery', "False");
        }
    }
    WhenChangeValue(elem) {
        var oprator = elem.target.parentElement.parentElement.children[1].children[0];
        switch (elem.target.type) {
            case "checkbox":
                if (elem.target.checked !== true) {
                    oprator.selectedIndex = 0;
                } else {
                    oprator.selectedIndex = 1;
                }
                break;
            //case "number":
            //    if (oprator.options[oprator.selectedIndex].value !== "Between") {
            //        if (oprator.selectedIndex === 0 && elem.target.value !== "") {
            //            oprator.selectedIndex = 1;
            //        } else if (oprator.selectedIndex !== 0 && elem.target.value === "") {
            //            oprator.selectedIndex = 0;
            //        }
            //    }
            //    break;
            default:
                if (elem.target.parentElement.childElementCount === 1) {
                    if (oprator.selectedIndex === 0 && elem.target.value !== "") {
                        oprator.selectedIndex = 1;
                    } else if (oprator.selectedIndex !== 0 && elem.target.value === "") {
                        oprator.selectedIndex = 0;
                    }
                }
        }
    }
}
class RetentionReport extends Reports {
    constructor() {
        super();
        this.dispositionText = "";
        this.dispositionid = "";
    }
    RetentionReport(elem, report, pageNumber) {
        repapp.dom.CurrentMenuElemnt = elem;
        this.MarkSelctedMenu(elem);
        this.CreateBaiscButtons(0);
        this.isNewrequestReport = false;
        repapp.ob.pageNumber = pageNumber
        $(repapp.dom.btnContainer).trigger("click")
        switch (report) {
            case "finaldisposition":
                this.FinalDisposition()
                break;
            case "certifiedisposition":
                this.Certifiedisposition();
                break;
            case "inactivepulllist":
                this.InactivePulllist()
                break;
            case "inactivereport":
                repapp.ob.reportType = repapp.enum.RetentionInactiveRecords;
                this.RunRetentionReport(repapp.ob, "no")
                break;
            case "recordonhold":
                repapp.ob.reportType = repapp.enum.RetentionRecordsOnHold
                // countRcord key is a flag for allow to count the records.
                repapp.ob.isCountRecord = true
                this.RunRetentionReport(repapp.ob, "no")
                break;
            case "citations":
                repapp.ob.reportType = repapp.enum.RetentionCitations;
                this.RunRetentionReport(repapp.ob, "no")
                break;
            case "citationwithretcode":
                repapp.ob.reportType = repapp.enum.RetentionCitationsWithRetCodes;
                this.RunRetentionReport(repapp.ob, "no")
                break;
            case "codes":
                repapp.ob.reportType = repapp.enum.RetentionCodes;
                this.RunRetentionReport(repapp.ob, "no")
                break;
            case "codewithcitations":
                repapp.ob.reportType = repapp.enum.RetentionCodesWithCitations;
                this.RunRetentionReport(repapp.ob, "no")
                break;
            default:
        }
    }
    FinalDisposition() {
        var btnSubmit = ""
        repapp.ob.reportType = repapp.enum.RetentionFinalDisposition;
        this.RunRetentionReport(repapp.ob, 0).then((data) => {
            //if dropdown return empty then don't create dropdown and disposition button
            if (data.ddlSelectReport.length > 0) {
                var reportLst = data.ddlSelectReport[0].text.split(" ");
                var submitType = reportLst[reportLst.length - 1]
                this.FinalDispositionType(submitType);
                //if disposition true show disposition for distroy, purge and archive.
                if (data.SubmitDisposition.isBtnVisibal) {
                    var btnSubmit = `<input type="button" name="dispositionType" id="${this.dispositionid}" class="btn btn-secondary tab_btn" value="${this.dispositionText}" style="min-width: 70px;">`
                }
                this.BindDropDowntoDom(data, "DDLfinaldisposition");
            }
            //create functional button for final disposition
            $(repapp.dom.btnleft).append(` <div class="btn-group">
                <button class="btn btn-secondary dropdown-toggle tab_btn" data-toggle="dropdown" type="button" aria-expanded="false">
                    <i class="">${prop.language["btnNew"]}</i>
                    <i class="fa fa-angle-down"></i>
                </button>
                <ul class="dropdown-menu btn_menu">
                    ${data.Purge.isBtnVisibal ? '<li><a id="newpurgeReport">' + data.Purge.btnText + '</a></li>' : ''}
                    ${data.Destruction.isBtnVisibal ? '<li><a id="newdestructionReport">' + data.Destruction.btnText + '</a></li>' : ''}
                    ${data.PermanentArchive.isBtnVisibal ? '<li><a id="newpermanentActive">' + data.PermanentArchive.btnText + '</a></li>' : ''}
                </ul>
            </div> ${btnSubmit == undefined ? "" : btnSubmit}`)

        });
    }
    FinalDispositionType(submitType) {
        //written to change the submit button for any disposition type dynamicly. 
        switch (submitType.toLowerCase()) {
            case "Archived".toLowerCase():
                this.dispositionText = "Submit for Archived";
                this.dispositionid = "submitforarchivePopup"
                break;
            case "Destruction".toLowerCase():
                this.dispositionText = "Submit for Destruction"
                this.dispositionid = "SubmitFordestructionPopup"
                break;
            case "Purged".toLowerCase():
                this.dispositionText = "Submit for Purged"
                this.dispositionid = "submitforpurgePopup";
                break;
            default:
        }
    }
    Certifiedisposition() {
        repapp.ob.reportType = repapp.enum.RetentionCertifieDisposition;
        this.RunRetentionReport(repapp.ob, "no").then((data) => {
            if (repapp.hot !== "") {
                if (repapp.hot.countRows() !== 0) {
                    this.BindDropDowntoDom(data, "DDLCertifiedPosionSelected")
                } else {
                    $(repapp.dom.ReportTitle).text(prop.language["titleNoCertOfDispReports"])
                }
            }
        });
    }
    InactivePulllist() {
        repapp.ob.reportType = repapp.enum.RetentionInactivePullList;
        this.RunRetentionReport(repapp.ob, 0).then(() => {
            if (repapp.hot !== "") {
                if (repapp.hot.countRows() !== 0) {
                    $(repapp.dom.btnleft).append(` <input type="button" id="setInactive" class="btn btn-secondary tab_btn" value="${prop.language["btnHTMLReportsSetInactive"]}" style="min-width: 70px;">`)
                }
            }
        })
    }

    //this function load Handsontable and Pagination data.
    RunRetentionReport(ob, isHiddenField) {
        $(prop.SpinningWheel.Main).show();
        return new Promise((resolve, reject) => {
            var data = JSON.stringify({ paramss: ob });
            var call = new DataAjaxCall(repapp.url.InitiateRetentionReport, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "", "");
            call.Send().then((rdata) => {
                if (rdata.isError === false) {
                    document.querySelector(repapp.dom.ReportTitle).innerHTML = rdata.lblTitle;
                    document.querySelector(repapp.dom.itemDescription).innerHTML = rdata.lblSubtitle;
                    HandsOnTableReports.buildHandsonTable(rdata, isHiddenField);
                    if (repapp.ob.isCountRecord == true && repapp.ob.reportType == repapp.enum.RetentionRecordsOnHold) {
                        reports.ResetPagination(rdata.Paging)
                    } else {
                        retentionreport.InitiateRetentionReportPagination()
                    }
                    $(prop.SpinningWheel.Main).hide();
                    resolve(rdata);
                } else {
                    $(prop.SpinningWheel.Main).hide();
                    showAjaxReturnMessage(rdata.Msg, "e");
                }
            }).catch(() => {
                $(prop.SpinningWheel.Main).hide();
                reject();
            });
        });
    }
    //this funcion load only handsontable data. It is not load pagination properties
    RunRetentionReportHandsondataData(ob, isHiddenField) {
        $(prop.SpinningWheel.Main).show();
        return new Promise((resolve, reject) => {
            var data = JSON.stringify({ paramss: ob });
            var call = new DataAjaxCall(repapp.url.InitiateRetentionReport, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "", "");
            call.Send().then((rdata) => {
                if (rdata.isError === false) {
                    HandsOnTableReports.buildHandsonTable(rdata, isHiddenField);
                    $(prop.SpinningWheel.Main).hide();
                    resolve(rdata);
                } else {
                    $(prop.SpinningWheel.Main).hide();
                    showAjaxReturnMessage(rdata.Msg, "e");
                }
            }).catch(() => {
                $(prop.SpinningWheel.Main).hide();
                reject();
            });
        });
    }

    //this function are using for load only pagination for Retention Report
    InitiateRetentionReportPagination() {
        $(prop.SpinningWheel.Main).show();
        return new Promise((resolve, reject) => {
            var data = JSON.stringify({ paramss: repapp.ob });
            var call = new DataAjaxCall(repapp.url.InitiateRetentionReportPagination, prop.ajax.Type.Post, prop.ajax.DataType.Html, data, prop.ajax.ContentType.Utf8, "", "", "", "");
            call.Send().then((rdata) => {

                if (rdata.isError) {
                    showAjaxReturnMessage(rdata.Msg, "e");
                    $(prop.SpinningWheel.Main).hide();
                    reject();
                }

                $('#paging-container').html(rdata);
                $(prop.SpinningWheel.Main).hide();
                resolve(rdata);
            }).catch((ex) => {
                alert(ex)
                $(prop.SpinningWheel.Main).hide();
                reject();
            });
        });
    }
    LoadPaging(obj) {
        $(prop.SpinningWheel.Main).show();
        var data = { PerPageRecord: obj.PerPageRecord, TotalPage: obj.TotalPage, TotalRecord: obj.TotalRecord, PageNumber: obj.PageNumber };
        $.get(repapp.url.PagingPartialView, data, function (data) {
            $(prop.SpinningWheel.Main).hide();
            $('#paging-container').html(data);
        });
    }
}
class BtnSetInactive {
    constructor() {
        this.ids = [];
        this.udate = "";
        this.username = "";
    }
    SubmitInactive() {
        this.GatherProperties();
        var data = JSON.stringify({ paramss: this })
        $(prop.SpinningWheel.Main).show();
        var call = new DataAjaxCall(repapp.url.BtnSubmitInactive, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                $(prop.Dialogbox.Dialogboxid).hide();
                $(prop.SpinningWheel.Main).hide();
            } else {
                $(prop.Dialogbox.Dialogboxid).hide();
                $(prop.SpinningWheel.Main).hide();
                showAjaxReturnMessage(rdata.Msg, "e");
            }
        });
    }
    GatherProperties() {
        var lst = HandsOnGetrowsSelected(repapp.hot)
        for (var i = 0; i < lst.length; i++) {
            var val = repapp.hot.getDataAtCell(lst[i], 0).split("||");
            var params = val[1] + "|" + val[0];
            this.ids.push(params)
        }
        this.udate = document.querySelector(repapp.dom.setInactive.Inactivedate).value;
        this.username = document.querySelector(repapp.dom.setInactive.InactiveUserName).value;
        var selected = document.querySelector(repapp.dom.setInactive.inactivepullDDL).options;
        this.ddlSelected = selected[selected.selectedIndex].value;
    }
}
class BtnSubmitReport {
    constructor(subType) {
        this.ids = [];
        this.udate = "";
        this.locationId = "";
        this.submitType = subType;
        this.reportId = 0;
    }
    SubmitReport() {
        var lst = this.GatherProperties();
        $(prop.SpinningWheel.Main).show();
        var data = JSON.stringify({ paramss: this })
        var call = new DataAjaxCall(repapp.url.BtnSubmitDisposition, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                this.AfterDisposition(lst);
            } else {
                $(prop.SpinningWheel.Main).hide();
                showAjaxReturnMessage(rdata.Msg, "e")
            }
            $(prop.Dialogbox.Dialogboxid).hide();
        });
    }
    AfterDisposition(lst) {
        //delete rows from UI
        for (var i = 0; i < lst.length; i++) {
            repapp.hot.alter('remove_row', lst[i]);
        }
        //message for user
        switch (this.submitType) {
            case "destruction":
                showAjaxReturnMessage("Selected Record/s has been destroyed successfully", "s");
                break;
            case "purge":
                showAjaxReturnMessage("Selected Record/s has been purged successfully", "s");
                break;
            case "archive":
                showAjaxReturnMessage("Selected Record/s has been archived successfully", "s");
                break;
            default:
        }

        setTimeout(() => {
            $(prop.SpinningWheel.Main).hide();
            repapp.dom.CurrentMenuElemnt.click()
        }, 2000)
    }
    GatherProperties() {
        var lst = HandsOnGetrowsSelected(repapp.hot)
        for (var i = 0; i < lst.length; i++) {
            var val = repapp.hot.getDataAtCell(lst[i], 0).split("||");
            var params = val[1] + "|" + val[0];
            this.ids.push(params)
        }
        this.reportId = document.querySelector(repapp.dom.Btn.DDLfinaldisposition).value;
        this.udate = document.querySelector(repapp.dom.SubmitPopup.submitDate).value;
        if (this.submitType === "archive") {
            if (document.querySelector(repapp.dom.SubmitPopup.submitDropdown) !== null) {
                this.locationId = document.querySelector(repapp.dom.SubmitPopup.submitDropdown).value;
            }

        }
        return lst;
    }

}
class PagingSection extends PagingFunctions {
    constructor(SelectedIndex, PageSize, TotalRecords, PerPageRecords) {
        super(SelectedIndex, PageSize, TotalRecords, PerPageRecords)
        this.PagingContainer = "#paging-div"
        if (repapp.ob.reportType >= 0) {
            $(this.PagingContainer).css({ 'display': 'block' });
        } else {
            $(this.PagingContainer).css({ 'display': 'none' });
        }
    }
    // override method
    // this function are using for load handsontable data
    LoadRecord() {
        if (repapp.ob.reportType === repapp.enum.RetentionFinalDisposition) {
            repapp.ob.pageNumber = this.SelectedIndex
            retentionreport.RunRetentionReportHandsondataData(repapp.ob, 0)
        } else if (repapp.ob.reportType === repapp.enum.RetentionCertifieDisposition) {
            repapp.ob.pageNumber = this.SelectedIndex
            retentionreport.RunRetentionReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType === repapp.enum.RetentionCitations) {
            repapp.ob.pageNumber = this.SelectedIndex
            retentionreport.RunRetentionReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType === repapp.enum.RetentionCitationsWithRetCodes) {
            repapp.ob.pageNumber = this.SelectedIndex
            retentionreport.RunRetentionReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType === repapp.enum.RetentionCodes) {
            repapp.ob.pageNumber = this.SelectedIndex
            retentionreport.RunRetentionReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType === repapp.enum.RetentionCodesWithCitations) {
            repapp.ob.pageNumber = this.SelectedIndex
            retentionreport.RunRetentionReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType == repapp.enum.RetentionInactivePullList) {
            repapp.ob.pageNumber = this.SelectedIndex
            retentionreport.RunRetentionReportHandsondataData(repapp.ob, 0)
        } else if (repapp.ob.reportType == repapp.enum.RetentionInactiveRecords) {
            repapp.ob.pageNumber = this.SelectedIndex
            retentionreport.RunRetentionReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType == repapp.enum.RequestNew) {
            repapp.ob.pageNumber = this.SelectedIndex
            reports.RunReportHandsondataData(repapp.ob, 0)
        } else if (repapp.ob.reportType == repapp.enum.RequestNewBatch) {
            repapp.ob.pageNumber = this.SelectedIndex
            reports.RunReportHandsondataData(repapp.ob, 0)
        } else if (repapp.ob.reportType == repapp.enum.RequestPullList) {
            repapp.ob.pageNumber = this.SelectedIndex
            reports.RunReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType == repapp.enum.RequestException) {
            repapp.ob.pageNumber = this.SelectedIndex
            reports.RunReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType == repapp.enum.RequestInProcess) {
            repapp.ob.pageNumber = this.SelectedIndex
            reports.RunReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType == repapp.enum.RequestWaitList) {
            repapp.ob.pageNumber = this.SelectedIndex
            reports.RunReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType == repapp.enum.RetentionRecordsOnHold) {
            repapp.ob.isCountRecord = false
            repapp.ob.pageNumber = this.SelectedIndex
            retentionreport.RunRetentionReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType == repapp.enum.ObjectsInventory) {
            repapp.ob.pageNumber = this.SelectedIndex
            reports.RunReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType == repapp.enum.ObjectOut) {
            repapp.ob.pageNumber = this.SelectedIndex
            reports.RunReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType == repapp.enum.PastDueTrackableItemsReport) {
            repapp.ob.pageNumber = this.SelectedIndex
            reports.RunReportHandsondataData(repapp.ob, "no")
        } else if (repapp.ob.reportType == repapp.enum.AuditReport) {
            repapp.ob.pageNumber = this.SelectedIndex
            reports.RunAuditReportHandsondataData(repapp.ob.pageNumber)
        } else if (repapp.ob.reportType == repapp.enum.CustomReport) {
            repapp.ob.pageNumber = this.SelectedIndex
            reports.RunCustomReportHandsonData(repapp.ob.pageNumber)
        }
    }

    // override 
    VisibleIndexs() {
        super.VisibleIndexs();
        if (repapp.ob.reportType >= 0) {
            $(this.PagingContainer).css({ 'display': 'block' });
        }
    }
}

//reports handsontable
var HandsOnTableReports = {
    HasHiddenField: false,
    buildHandsonTable: function (data, TableHasHiddenField = "no") {
        repapp.data = data;
        this.HasHiddenField = TableHasHiddenField;
        document.querySelector(repapp.dom.HandsOnTableContainer).innerHTML = "";
        this.container = document.querySelector(repapp.dom.HandsOnTableContainer);
        this.container.style.display = "block";
        this.hotSettings = {
            data: data.ListOfRows,
            rowHeaders: false,
            //dropdownMenu: ['remove_col', '---------', 'alignment'],
            //filters: true,
            //dropdownMenu: ['filter_by_condition', 'filter_action_bar'],
            //height: 700,
            outsideClickDeselects: false,
            disableVisualSelection: false,
            sortIndicator: true,
            columnSorting: true,
            colHeaders: data.ListOfHeader,
            stretchH: 'all',
            fillHandle: false,
            manualRowResize: true,
            manualColumnResize: true,
            search: true,
            headerTooltips: true,
            currentRowClassName: 'currentRow',
            //renderAllRows: false,
            //afterDeselect: true,
            //colWidths: [0, 10,18],
            //fixedRowsTop: 1,
            //manualColumnFreeze: true,
            comments: true,
            //contextMenu: ['row_above', 'row_below', 'remove_row'],
            //contextMenu: toolbarmenufunc.GetMenu(),
            autoRowSize: false,
            autoWrapCol: false,
            autoWrapRow: false,
            manualRowMove: true,
            manualColumnMove: true,
            licenseKey: "a1fe9-3411a-c6714-e4504-e7930",
        };
        repapp.hot = new Handsontable(this.container, this.hotSettings);
        repapp.hot.addHook('afterSelectionEnd', function (rowstart, colstart, rowend, colend) {
            //build rows array for later use.
            if (repapp.hot.getSelected().length === 1) {
                //if get selected method comes with one row it means it is starting new row
                //so in this case I clear the array and start all over again.
                rowselected = [];
            }
            if (rowstart < rowend) {
                for (var i = rowstart; i < rowend + 1; i++) {
                    rowselected.push(i);
                }
            } else if (rowstart > rowend) {
                for (var j = rowend; j < rowstart + 1; j++) {
                    rowselected.push(j);
                }
            } else if (rowstart === rowend) {
                rowselected.push(rowstart);
            }
        });
        repapp.hot.updateSettings({
            cells: function (row, col) {
                var cellProperties = {};
                cellProperties.readOnly = true;
                return cellProperties;
            },
            hiddenColumns: {
                columns: [TableHasHiddenField],
                indicators: true
            },
            //calculate row to get a dynamic height 34px is the row high + 68 is the header hight 
            height: repapp.hot.countRows() * 34 + 68,
            //colWidths: this.colWidthArray
        });
    }
}

class ExportData {
    ExportCSVorTXTCurrent(iscsv) {
        var obje = {};
        obje.viewId = repapp.ob.reportType == repapp.enum.CustomReport ? parseInt(reports.ViewId) : 0;
        obje.RecordCount = repapp.hot.countRows();//rowselected.length;
        obje.viewName = repapp.ob.reportType == repapp.enum.CustomReport ? reports.ViewName : "Report";
        obje.tableName = repapp.ob.reportType == repapp.enum.CustomReport ? reports.TableName : this.GetKeyByValue(repapp.enum, repapp.ob.reportType);
        obje.IsCSV = iscsv;
        obje.IsSelectAllData = false;
        obje.ListofselectedIds = [];
        obje.DataRows = [];
        obje.Headers = [];
        obje.ReportType = repapp.ob.reportType.toString();
        obje.ReportAuditFilterProperties = repapp.auditObj;
        this.ExportCsvBuildObject(obje)
        var data = JSON.stringify({ paramss: obje })
        var call = new DataAjaxCall(repapp.url.DialogConfirmExportCSVorTXT, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                var dlg = new DialogBoxMsg(rdata.lblTitle, rdata.HtmlMessage, prop.Dialogbox.Type.Content)
                dlg.ShowDialog().then((data) => {
                    if (rdata.isRequireBtn) {
                        reports.IsBackgroundProcessing = rdata.IsBackgroundProcessing;
                        if (rdata.Permission === false) {
                            $(prop.Dialogboxmsg.DlgBsContentmsg).append(`<div class="modal-footer"><input type="button" onclick="$('#Dialogboxmsgid').hide()" class="btn btn-default" value="${prop.language["Cancel"]}"> </div>`)
                        } else {
                            $(prop.Dialogboxmsg.DlgBsContentmsg).append(`<div class="modal-footer"><input type="button" onclick="$('#Dialogboxmsgid').hide()" style="float: right; margin-left:5px" class="btn btn-default" value="${prop.language["No"]}"> <input type="button" id="ExportToCSVorTXT" class="btn btn-primary" style="float: right;" value="${prop.language["Yes"]}"></div>`)
                        }
                    }
                }).catch((data) => {
                    console.log(data);
                });
            } else {
                showAjaxReturnMessage(rdata.Msg, "e");
            }
        });
    }
    ExportCsvBuildObject(obje) {
        //header
        var ListOfHeaders = repapp.data.ListOfHeader;
        var startIndex = HandsOnTableReports.HasHiddenField == '0' ? 1 : 0;
        if (repapp.ob.reportType === repapp.enum.CustomReport) {
            for (var i = startIndex; i < ListOfHeaders.length; i++) {
                if (i > reports.StartInCell(repapp.data) - 1) {
                    var head = ListOfHeaders[i];
                    obje.Headers.push({ ColumnName: head, DataType: "", HeaderName: head });
                    //obje.Headers.push({ ColumnName: head.ColumnName, DataType: head.DataType, HeaderName: head.HeaderName });
                }
            }
        } else {
            for (var i = startIndex; i < ListOfHeaders.length; i++) {
                if (i > reports.StartInCell(repapp.data) - 1) {
                    var head = ListOfHeaders[i];
                    obje.Headers.push({ ColumnName: head, DataType: "", HeaderName: head });
                }
            }
        }
        //rows
        var rowbuilder = [];

        for (var i = 0; i < repapp.hot.countRows(); i++) {
            for (var j = startIndex; j < repapp.hot.countCols(); j++) {
                rowbuilder.push(repapp.hot.getDataAtCell(i, j));
            }
            obje.DataRows.push(rowbuilder);
            obje.ListofselectedIds.push(repapp.hot.getDataAtCell(i, 0));//strore ids for data processing
            rowbuilder = [];
        }
    }
    ExportCSVorTXTAll(iscsv) {
        var obje = {};
        obje.viewId = repapp.ob.reportType == repapp.enum.CustomReport ? parseInt(reports.ViewId) : 0;
        obje.RecordCount = rowselected.length;
        obje.viewName = repapp.ob.reportType == repapp.enum.CustomReport ? reports.ViewName : "Report";
        obje.tableName = repapp.ob.reportType == repapp.enum.CustomReport ? reports.TableName : "Report";
        obje.IsCSV = iscsv;
        obje.IsSelectAllData = true;
        obje.ReportType = repapp.ob.reportType.toString();
        obje.ReportAuditFilterProperties = repapp.auditObj;
        console.log("asdf",obje);
        var data = JSON.stringify({ paramss: obje })

        var call = new DataAjaxCall(repapp.url.DialogConfirmExportCSVorTXT, prop.ajax.Type.Post, prop.ajax.DataType.Json, data, prop.ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                var dlg = new DialogBox(rdata.lblTitle, rdata.HtmlMessage, prop.Dialogbox.Type.Content)
                dlg.ShowDialog().then((data) => {
                    if (rdata.isRequireBtn) {
                        reports.IsBackgroundProcessing = rdata.IsBackgroundProcessing;
                        if (rdata.Permission === false) {
                            $(prop.Dialogbox.DlgBsContent).append(`<div class="modal-footer"><input type="button" onclick="$('#Dialogboxid').hide()" class="btn btn-default" value="${prop.language["Cancel"]}"> </div>`)
                        } else {
                            $(prop.Dialogbox.DlgBsContent).append(`<div class="modal-footer"><input type="button" id="ExportToCSVorTXTAll" class="btn btn-primary" style="float: left;" value="${prop.language["Yes"]}"><input type="button" onclick="$('#Dialogboxid').hide()" class="btn btn-default" style="float: right;" value="${prop.language["No"]}"> </div>`)
                        }
                    }
                }).catch((data) => {
                    console.log(data);
                });
            } else {
                showAjaxReturnMessage(rdata.Msg, "e");
            }
        });
    }
    GetKeyByValue(object, value) {
        return Object.keys(object).find(key => object[key] === value);
    }
}
var reports = new Reports();
var obJquerywindow = new reportsQuery();
var retentionreport = new RetentionReport();
var pagingsection = new PagingSection();
var exportData = new ExportData();