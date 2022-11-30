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
