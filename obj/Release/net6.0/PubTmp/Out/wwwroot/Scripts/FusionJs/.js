// Vault Operations
class VaultFunction {
    constructor() {
        this.trackableIdMap = new Map();
    }
    LoadViewValt(docdata, obj) {
        bc.innerHTML = "";
        $(app.SpinningWheel.Main).show();
        var data = { 'docdata': docdata, ViewType: OrphanSelectedView }
        var call = new DataAjaxCall(app.url.server.OrphanPartial, app.ajax.Type.Get, app.ajax.DataType.Html, data, "", "", "", "")
        return new Promise((resolve, reject) => {
            call.Send().then((partialData) => {
                $("#mainContainer").empty()
                $("#mainContainer").html(partialData)
                $(app.SpinningWheel.Main).hide();
                resolve(true)
            }).catch(() => {
                reject();
            });
        });
    }
    CheckInputBoxes() {
        $("#AttachmentModalBody input[type=checkbox]").each(function (e, i) {
            var id = $(i).attr('trackableId');
            if (obJvaultfunction.trackableIdMap.has(id)) {
                $(i).prop('checked', true);
            }
        })
    }
    AttachOrphanRecord() {
        var _this = this;
        if (rowselected.length > 1) {
            showAjaxReturnMessage(app.language["msgLinkOnlyOneRecord"], 'w')
        }
        else if (hot.getDataAtRow(rowselected[0])[0] == null) {
            showAjaxReturnMessage(app.language["msgCantAttachForEmptyRow"], 'w')
        }
        else {

            var docdata = app.data.TableName + ',' + hot.getDataAtCell(rowselected[0], 0) + ',' + app.data.ViewName + ',' + app.data.ViewId
            this.LoadViewValt(docdata, this).then(() => {
                _this.SetBreadcrumbs();
            })
        }
    }
    ChangeViewType(type) {
        $("#fileSearchOrphan").val("")
        OrphanSelectedView = OrphanViewType[type]
        obJvaultfunction.LoadOrphansAttachment()
    }
    SelectCheckBoxDiv(eleObj) {
        var checkBoxObj;

        if ($(eleObj).prop("tagName") == "INPUT") {
            checkBoxObj = $(eleObj);
        } else {
            checkBoxObj = $(eleObj).find("input[type=checkbox]");
        }

        if (checkBoxObj.prop("checked")) {
            checkBoxObj.prop("checked", false);
        } else {
            checkBoxObj.prop("checked", true);
        }

        this.ShowButtons();
    }
    UncheckCheckbox() {
        $(".Thmbnail-main input[type=checkbox]").prop("checked", false)
    }
    ShowButtons() {
        //adds trackable id to map
        $("#AttachmentModalBody input[type=checkbox]").each(function (e, i) {
            if ($(i).prop('checked')) {
                var arr = [i, $("#Download_" + $(i).attr('trackableId')).attr("href")];
                obJvaultfunction.trackableIdMap.set($(i).attr('trackableId'), arr);
            } else {
                obJvaultfunction.trackableIdMap.delete($(i).attr('trackableId'));
            }
        })
        //shows buttons when attachment is clicked
        if (obJvaultfunction.trackableIdMap.size > 0) {
            $("#divDeleteAttachment").show();
            $("#divDownloadAttachment").show();
            $("#btnMoveAttachments").show();
            $("#divSelectedRecords").show();
            //Selected records: x functionality on _OrphanContent
            /*$("#lblSelectedRecords").text(app.language["selectedRecords"] + " : " + obJvaultfunction.trackableIdMap.size);*/
            return false;
        }
        else {
            $("#divDeleteAttachment").hide();
            $("#divDownloadAttachment").hide();
            $("#btnMoveAttachments").hide();
            $("#divSelectedRecords").hide();
        }
    }
    DownloadAttachment() {
        $("#spinningWheel").show();
        var trackableIdIterator = obJvaultfunction.trackableIdMap[Symbol.iterator]();
        for (var i of trackableIdIterator) {
            var url = i[1][1];
            $.ajax({
                url: url,
                method: 'GET',
                beforeSend: function () {
                    //$("#spinningWheel").show();
                },
                xhrFields: {
                    responseType: 'blob'
                },
                success: function (data) {
                    var reqUrl = this.url
                    var fileName = reqUrl.substring(reqUrl.indexOf("fileName=") + 9, reqUrl.length)
                    var a = document.createElement('a');
                    var url = window.URL.createObjectURL(data);
                    a.href = url;
                    a.download = decodeURI(fileName);
                    document.body.append(a);
                    a.click();
                    a.remove();
                    window.URL.revokeObjectURL(url);
                },
                fail: function (err) {

                }
            });
        }
        trackableIdIterator = obJvaultfunction.trackableIdMap[Symbol.iterator]();
        for (var i of trackableIdIterator) {
            $(i[1][0]).prop('checked', false);
        }
        obJvaultfunction.trackableIdMap.clear();
        obJvaultfunction.ShowButtons();
        $("#spinningWheel").hide();
    }
    LoadOrphansAttachment() {
        $(app.SpinningWheel.Main).show();
        var pageNum = vaultPagingfunc.SelectedIndex //parseInt(document.getElementsByClassName('page-number')[0].value);
        var filter = $("#fileSearchOrphan").val();
        var data = {
            'docdata': { 'tableName': "", }, 'isMVC': true, ViewType: OrphanSelectedView, Filter: filter, PageIndex: pageNum
        }
        var call = new DataAjaxCall(app.url.server.LoadFlyoutOrphansPartial, app.ajax.Type.Get, app.ajax.DataType.Html, data, "", "", "", "")
        call.Send().then((partialData) => {
            $("#AttachmentModalBody").empty()
            $("#AttachmentModalBody").html(partialData)
            $(app.SpinningWheel.Main).hide();
            this.CheckInputBoxes();
            this.ShowButtons();
        });
    }
    FilterOnEnter() {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        var filter = $("#fileSearchOrphan").val();
        if (keycode == 13) {
            if (filter != "") {
                this.ResetOrphanPagginationProperties()
            }
        } else if (filter == "") {
            this.ResetOrphanPagginationProperties()
        }

    }
    FilterOnSearch() {
        var filter = $("#fileSearchOrphan").val();
        if (filter != "") {
            this.ResetOrphanPagginationProperties()
        }
    }
    ResetOrphanPagginationProperties() {
        var filter = $("#fileSearchOrphan").val();
        var data = {
            'filter': filter
        }
        var call = new DataAjaxCall(app.url.server.GetCountAfterFilter, app.ajax.Type.Post, app.ajax.DataType.Json, data, "", "", "", "")
        call.Send().then((res) => {
            if (res.isError === false) {
                vaultPagingfunc.SelectedIndex = 1
                vaultPagingfunc.TotalRecords = res.TotalRecords;
                vaultPagingfunc.PageSize = Math.ceil(res.TotalRecords / parseInt(res.PerPageRecord));
                vaultPagingfunc.VisibleIndexs();
                vaultPagingfunc.AddCommaToTotalRecords();
                vaultPagingfunc.SetTextFieldValues();
                obJvaultfunction.LoadOrphansAttachment();
            } else {
                showAjaxReturnMessage(res.Msg, 'e');
            }
        });
    }
    MoveAttachments() {
        var TrackableIds = ""
        var trackableIdIterator = obJvaultfunction.trackableIdMap[Symbol.iterator]();
        for (var i of trackableIdIterator) {
            if (TrackableIds.length > 0) {
                TrackableIds = TrackableIds + "," + i[0];
            } else {
                TrackableIds = i[0];
            }
        }

        var TableName = $("#hiddenTableName").val()
        var RecordId = $("#hiddenRecordId").val()

        if (TrackableIds.length < 1) {
            return false
        }
        if (TableName.length < 1 && RecordId.length < 1) {
            return false
        }

        var json = {
            TrackableIds: TrackableIds,
            TableName: TableName,
            RecordId: RecordId
        }

        $.post(app.url.server.LinkOrphanAttachment, json)
            .done(function (response) {
                $("#spinningWheel").hide();
                $("#modalDelete").modal("hide")
                if (response.isError === false) {
                    if (response.IsAlreadyDeleted == true) {
                        showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["alreadyDeletedWarning"], "w")
                        obJvaultfunction.trackableIdMap.clear();
                    }
                    else {
                        obJvaultfunction.ResetOrphanPagginationProperties();
                        obJvaultfunction.trackableIdMap.clear();
                    }
                } else {
                    showAjaxReturnMessage(response.Msg, 'e');
                }
            })
            .fail(function (xhr, status, error) {
                obJvaultfunction.trackableIdMap.clear();
                $("#spinningWheel").hide();
                $('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: error.message });
            });
    }
    RefreshAttachments(elemnt) {

        $("#vaultLabel").addClass('selectedMenu');
        vaultPagingfunc.SelectedIndex = 1
        obJvaultfunction.trackableIdMap.clear();
        $("#fileSearchOrphan").val("")
        //window.location.reload()
        this.ResetOrphanPagginationProperties();
    }
    BrowseFile() {
        $('#OrphanFileUploadForAddAttach').click()

    }
    DeleteAttachmentConfim() {
        $("#modalDelete").modal("show")
    }
    DeleteAttachment() {
        var TrackableIds = this.GetTrackableIds()

        var json = {
            TrackableIds: TrackableIds,
        }

        if (TrackableIds.length < 1) {
            return false;
        }

        $("#spinningWheel").show();

        $.post(app.url.server.DeleteAttachments, json)
            .done(function (response) {
                $("#spinningWheel").hide();
                if (response.isError === false) {
                    if (response.IsAlreadyMaped == true) {
                        $("#modalDelete").modal("hide")
                        showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["alreadyAttachedWarning"], "w")
                    } else {
                        $("#modalDelete").modal("hide")
                        showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["msgAttachmentDeletedSuccessfully"], "s")
                        obJvaultfunction.ResetOrphanPagginationProperties()
                        obJvaultfunction.trackableIdMap.clear();
                    }
                } else {
                    showAjaxReturnMessage(response.Msg, 'e');
                }
            })
            .fail(function (xhr, status, error) {
                $("#spinningWheel").hide();
                obJvaultfunction.trackableIdMap.clear();
                showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["msgAttachmentDeleteFailed"], "w")
            });
    }
    GetTrackableIds() {
        var TrackableIds = "";
        var trackableIdIterator = obJvaultfunction.trackableIdMap[Symbol.iterator]();
        for (var i of trackableIdIterator) {
            if (TrackableIds.length > 0) {
                TrackableIds = TrackableIds + "," + i[0];
            } else {
                TrackableIds = i[0];
            }
        }
        return TrackableIds
    }
    BackToView() {
        var viewId = $("#hiddenViewId").val();
        if (viewId.length > 0) {
            obJgridfunc.LoadView(this, viewId)
        }
    }
    CheckDownloadable() {
        var TrackableIds = this.GetTrackableIds()

        var json = {
            TrackableIds: TrackableIds,
        }

        if (TrackableIds.length < 1) {
            return false;
        }

        $("#spinningWheel").show();

        $.post(app.url.server.CheckDownloadable, json)
            .done(function (response) {
                $("#spinningWheel").hide();
                if (response.isError === false) {
                    if (response.IsAlreadyMaped == true) {
                        showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["alreadyAttachedDownloadFailed"], "w")
                    } else {
                        obJvaultfunction.DownloadAttachment();
                    }
                } else {

                }
            })
            .fail(function (xhr, status, error) {
                $("#spinningWheel").hide();
                showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["msgAttachmentDownloadFailed"], "w")
            });
    }
    UploadAttachmentOnNewAddOrphan(FlyoutUploderFiles) {
        var isfilesSupported = true;
        var formdata = new FormData();
        formdata.append("tabName", 'Orphans');
        formdata.append("tableId", '');
        formdata.append("viewId", '');
        for (var i = 0; i < FlyoutUploderFiles.length; i++) {
            var format = FlyoutUploderFiles[i].name.toString();
            format = format.split(".");
            if (format[0].length > 50) {
                showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["msgFileNameTooLong"], "w");
                isfilesSupported = false;
                break;
            }
            var cFileFormat = obJattachmentsview.IsSupportedFile(format[format.length - 1]);
            if (cFileFormat) {
                formdata.append(FlyoutUploderFiles[i].name, FlyoutUploderFiles[i]);
            } else {
                showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["invalidOrphanFormat"], "w")
                isfilesSupported = false;
                $('#OrphanFileUploadForAddAttach').val('');
                break;
            }
        }
        if (isfilesSupported === true) {
            $('#OrphanFileUploadForAddAttach').val('');
            obJvaultfunction.SaveNewAttachmentOrphan(formdata);
        } else {
            return;
        }

    }
    DragAnddropNewFileOrphan() {
        var _this = this;
        document.querySelector(app.domMap.DialogAttachment.EmptydropDiv)
        var newFile = document.querySelector(app.domMap.DialogAttachment.AttachmentModalBody);
        newFile.ondrop = function (e) {
            e.preventDefault();
            obJvaultfunction.UploadAttachmentOnNewAddOrphan(e.dataTransfer.files);
            newFile.style.border = "1px solid white";
        };
        newFile.ondragover = function () {
            newFile.style.border = "5px dashed #cccccc";
            return false;
        };
        newFile.ondragleave = function () {
            newFile.style.border = "1px solid white";
            return false;
        };
    }
    ChangeBrowseFile(e) {
        this.UploadAttachmentOnNewAddOrphan(e.files);
    }
    SaveNewAttachmentOrphan(formdata) {
        $(app.SpinningWheel.Main).show();
        var call = new DataAjaxCall(app.url.server.AddNewAttachment, app.ajax.Type.Post, app.ajax.DataType.Json, formdata, app.ajax.ContentType.False, app.ajax.ProcessData.False, "", "")
        call.Send().then((model) => {
            switch (model.checkConditions) {
                case "success":
                    // Remove Filter and Set page number 1
                    $("#fileSearchOrphan").val("");
                    obJvaultfunction.ResetOrphanPagginationProperties()
                    // below functin uncheck all the checkbox
                    obJvaultfunction.UncheckCheckbox();
                    // hide the delete and download buttons
                    obJvaultfunction.ShowButtons();
                    showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["msgAttachmentSuccessfullyAdded"], "s")
                    break;
                case "permission":
                    showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["msgDocViewerVolumePermission"], "w")
                    break;
                case "maxsize":
                    showAjaxReturnMessage(model.WarringMsg, 'w');
                    break;
                case "error":
                    showAjaxReturnMessage(model.WarringMsg, 'e');
                    break;
                default:
            }

            $(app.SpinningWheel.Main).hide();
        });
    }
    SetBreadcrumbs() {
        bc.innerHTML = "";
        app.data.ItemDescription = $("#displayname").val();
        obJbreadcrumb.CreateHtmlCrumbLink(hot.getDataAtRow(rowselected)[0], obJdrildownclick.ChildKeyField, obJreportsrecord.ChildKeyType, obJgridfunc.ViewId, obJgridfunc.ViewName, buildQuery);
        obJbreadcrumb.CreateCrumbHeader(obJdrildownclick.childViewName, app.Enums.Crumbs.Vault);
    }
    CheckOrphanAvailabel(e) {
        var TrackableIds = $(e).attr("AttachId")
        var link = $(e).attr("link");

        var json = {
            TrackableIds: TrackableIds,
        }

        if (TrackableIds.length < 1) {
            return false;
        }

        $("#spinningWheel").show();

        $.post(app.url.server.CheckDownloadable, json)
            .done(function (response) {
                $("#spinningWheel").hide();
                if (response.isError === false) {
                    if (response.IsAlreadyMaped == true) {
                        showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["alreadyAttachedDownloadFailed"], "w")
                    } else {
                        window.open(link);
                    }
                } else {

                }
            })
            .fail(function (xhr, status, error) {
                $("#spinningWheel").hide();
                showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["msgAttachmentDownloadFailed"], "w")
            });
    }
}

// This constraint are using for Change grid type only for Valt page
const OrphanViewType = {
    G: "Grid",
    L: "List"
}

// set default view type for orphan View
var OrphanSelectedView = OrphanViewType.G

class VaultPagingfunc extends PagingFunctions {
    constructor(SelectedIndex, PageSize, TotalRecords, PerPageRecords) {
        super(SelectedIndex, PageSize, TotalRecords, PerPageRecords)
    }
    // override method
    // this function are using for load attachment data
    LoadRecord() {
        obJvaultfunction.LoadOrphansAttachment()
    }
}



var obJvaultfunction = new VaultFunction();