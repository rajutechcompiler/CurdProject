//calcualte divs after collapse
//$(document).on("click", function () {
//    $(domMap.DataGrid.MainDataContainerDiv).height($("#page-content-wrapper").height() - $('#TaskContainer').height() - $(domMap.DataGrid.TrackingStatusDiv).height());
//});
//handling handsonTable when user click out of the grid
//$(document).click(function () {
//    if (hot !== undefined) {
//        hot.selectRows(0)
//        gridfunc.StartInCell()
//    }
//})
//$('body').on('click', app.domMap.DataGrid.HandsOnTableContainer, function () {
//    event.stopPropagation();
//});
//End handling handsonTable when user click out of the grid
//autosave for row button
//localStorage.setItem("rowautosave", false);

//left menu close and open 
$("body").on("click", "#left-panel-arrow", (e) => {
    if (hot === undefined) return; 
    //var currentWidth = $(app.domMap.DataGrid.HandsOnTableContainer).width();
    //if (elem.target.parentElement.parentElement.clientWidth > 0) {
    //    $(app.domMap.DataGrid.HandsOnTableContainer).width(currentWidth + 250);
    //} else {
    //    $(app.domMap.DataGrid.HandsOnTableContainer).width(currentWidth - 250);
    //}
    setTimeout(() => {
        hot.render()
    }, 500)
})




//save edit row button
$("body").on("click", app.domMap.ToolBarButtons.BtnSaveRow, () => {

    app.globalverb.Isnewrow = false;
    var s = hot.getSelected();
    var rowStart = s[0][0];
    var rowEnd = s[0][2];
    var corStart = s[0][1];
    if (rowStart !== rowEnd) {
        showAjaxReturnMessage(`you can't save changes on more than one row`, "w");
        return;
    }

    if (obJgridfunc.BuildRowFromCells.length === 0) {
        //showAjaxReturnMessage(`there are no changes in row ${rowStart}`, "w")        
        showAjaxReturnMessage(app.language["msgwarningequest"], "w")

    } else {
        app.globalverb.GCurrentRowHolder.previous_row = rowStart;
        obJgridfunc.BuildRowFromCells.push({ DataTypeFullName: "", columnName: "", value: hot.getDataAtCol(0)[rowStart] });
        obJgridfunc.SaveRowAfterBuild().then(() => {
            obJgridfunc.AfterChange = [];
            obJgridfunc.BeforeChange = [];
            obJgridfunc.BuildRowFromCells = [];
            obJgridfunc.LastRowSelected = null;
            console.log("button save finished after return saverowafterbuild fired")
        })
    }
})

$('body').on("click", app.domMap.Dialogboxbasic.BtnAddNewRow, () => {
    obJaddnewrecord.SaveNewRecord();
})

//DIALOG AUTO SAVE EVENTS
$('body').on("click", app.domMap.Autosave.autosavebtn, (e) => {
    CheckrowsAutosave("click")
})
//confirm save row after changes
$('body').on("click", app.domMap.Autosave.saverowdialogYes, () => {
    $(app.domMap.Dialogboxbasic.BasicDialog).hide();
    app.globalverb.Isnewrow = false;
    obJgridfunc.SaveRowAfterBuild().then(() => {
        dlgClose.CloseDialog();
        obJgridfunc.AfterChange = [];
        obJgridfunc.BeforeChange = [];
        obJgridfunc.BuildRowFromCells = [];
    });
});

//TOOLBAR EVENTS
//PRINT
$('body').on("click", app.domMap.ToolBarButtons.BtnPrint, function () {
    obJtoolbarmenufunc.PrintTable("")
});
$('body').on("click", app.domMap.ToolBarButtons.BtnQuery, function () {
    obJquerywindow.LoadQueryWindow(true)
});
//PRINT BARCODE BLACK AND WHITE
$('body').on("click", app.domMap.ToolBarButtons.BtnBlackWhite, () => {
    app.globalverb.ids = [];
    for (var i = 0; i < rowselected.length; i++) {
        var cellid = hot.getDataAtCell(rowselected[i], 0)
        if (cellid !== null) {
            if (app.data.IdFieldDataType == "System.String") {
                app.globalverb.ids.push(`'${cellid}'`);
            } else {
                app.globalverb.ids.push(cellid);
            }
            
        }
    }
    if (app.globalverb.ids.length === 0) {
        showAjaxReturnMessage(app.language["msgJsDataSelectOneRow"], "w")
        return;
    }

    obJtoolbarmenufunc.GetPrintBarcodePopup();
});
$('body').on("click", app.domMap.PrintBarcod.setDefault, (e) => {
    var objc = {
        PrintBarcode: {
            labelFormSelectedValue: parseInt(document.querySelector(app.domMap.PrintBarcod.labelForm).selectedOptions[0].value),
            labelDesignSelectedValue: parseInt(document.querySelector(app.domMap.PrintBarcod.labelDesign).selectedOptions[0].value)
        }
    }
    objc.TableName = app.data.TableName;
    var data = JSON.stringify({ paramss: objc })
    var call = new DataAjaxCall(app.url.server.SetDefaultBarcodeForm, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
    call.Send().then((rdata) => {
        if (rdata.isError) {
            showAjaxReturnMessage(app.language["GenErrorMessage"], "e")
        } else {
            showAjaxReturnMessage(`Default stock has been changed successfully to "${document.querySelector(app.domMap.PrintBarcod.labelForm).selectedOptions[0].innerText}"`, "s")
        }
    })
});
$('body').on("click", app.domMap.PrintBarcod.previewData, (e) => {
    if (e.target.checked) {
        $(app.domMap.PrintBarcod.previewContainer).show();
    } else {
        $(app.domMap.PrintBarcod.previewContainer).hide();
    }
});
$('body').on("click", app.domMap.PrintBarcod.labelOutline, (e) => {
    obJtoolbarmenufunc.GetPrintBarcodeOnchange(false);
});
$('body').on("change", app.domMap.PrintBarcod.labelDesign, (e) => {
    obJtoolbarmenufunc.GetPrintBarcodeOnchange(true).then((formId) => {
        var opt = document.querySelector(app.domMap.PrintBarcod.labelForm);
        for (var i = 0; i < opt.options.length; i++) {
            var val = opt.options[i].value;
            if (parseInt(val) === parseInt(formId)) {
                var indx = opt.options[i].index;
                opt.options.selectedIndex = indx;
            }
        }
    });
});
$('body').on("change", app.domMap.PrintBarcod.labelForm, (e) => {
    obJtoolbarmenufunc.GetPrintBarcodeOnchange(false);
});
$('body').on("click", app.domMap.PrintBarcod.BarcodeBtnUp, (e) => {
    var lblNumber = parseInt(document.querySelector(app.domMap.PrintBarcod.strtPrinting).value);
    if (document.querySelector(app.domMap.PrintBarcod.strtPrinting).maxLength === lblNumber) return
    lblNumber = lblNumber + 1
    document.querySelector(app.domMap.PrintBarcod.strtPrinting).value = lblNumber
    obJtoolbarmenufunc.GetPrintBarcodeOnchange(false);
});
$('body').on("click", app.domMap.PrintBarcod.BarcodeBtnDown, (e) => {
    var lblNumber = parseInt(document.querySelector(app.domMap.PrintBarcod.strtPrinting).value);
    if (lblNumber === 1) return;
    lblNumber = lblNumber - 1
    document.querySelector(app.domMap.PrintBarcod.strtPrinting).value = lblNumber
    obJtoolbarmenufunc.GetPrintBarcodeOnchange(false);
});
$('body').on("keyup", app.domMap.PrintBarcod.strtPrinting, (e) => {
    var maxvalue = document.querySelector(app.domMap.PrintBarcod.strtPrinting).maxLength;
    var currentvalue = parseInt(document.querySelector(app.domMap.PrintBarcod.strtPrinting).value);
    if (isNaN(currentvalue)) return;
    if (maxvalue < currentvalue) {
        document.querySelector(app.domMap.PrintBarcod.strtPrinting).value = maxvalue;
    }
    obJtoolbarmenufunc.GetPrintBarcodeOnchange(false);
});


//EXPORT CSV OR TXT
$('body').on("click", app.domMap.ToolBarButtons.BtnExportCSVSelected, function () {
    obJtoolbarmenufunc.ExportCSVorTXTSelected(true);
});
$('body').on("click", app.domMap.ToolBarButtons.btnExportCSVGridReport, function () {
    if (app.data.ListOfRows.length == 0) {
        showAjaxReturnMessage("There is no data to export.", "w");
    } else {
        obJtoolbarmenufunc.ExportCSVorTXTCurrentGridReport(true);
    }
});
$('body').on("click", app.domMap.ToolBarButtons.BtnExportTXTSelected, function () {
    obJtoolbarmenufunc.ExportCSVorTXTSelected(false);
});
$('body').on('click', app.domMap.ExportFiles.ExportSelectedToCSVorTXT, () => {
    if (app.globalverb.IsBackgroundProcessing) {
        $.get(app.url.server.BackGroundProcessing).done((response) => {
            if (response.isError === false) {
                var dlg = new DialogBoxBasic(response.lblTitle, response.HtmlMessage, app.domMap.Dialogboxbasic.Type.Content);
                dlg.ShowDialog().then(() => { })
            } else {
                showAjaxReturnMessage(response.Msg, "e");
            }
        });
    } else {
        location.href = app.url.server.ExportCSVOrTXT;
    }
    $(app.domMap.Dialogboxbasic.BasicDialog).hide();

});
$('body').on('click', app.domMap.ExportFiles.ExportSelectedToCSVorTXTGridReport, () => {
    location.href = app.url.server.ExportCSVOrTXTGridReport;
    $(app.domMap.Dialogboxbasic.BasicDialog).hide();
});
//EXPORT CSV OR TXT - ALL
$('body').on("click", app.domMap.ToolBarButtons.BtnExportCSVAll, function () {
    obJtoolbarmenufunc.ExportCSVorTXTAll(true);
})
$('body').on("click", app.domMap.ToolBarButtons.BtnExportTXTAll, function () {
    obJtoolbarmenufunc.ExportCSVorTXTAll(false);
})
$('body').on('click', app.domMap.ExportFiles.ExportToCSVorTXTAll, () => {
    if (app.globalverb.IsBackgroundProcessing) {
        $.get(app.url.server.BackGroundProcessing).done((response) => {
            if (response.isError === false) {
                var dlg = new DialogBoxBasic(response.lblTitle, response.HtmlMessage, app.domMap.Dialogboxbasic.Type.Content);
                dlg.ShowDialog().then(() => { })
            } else {
                showAjaxReturnMessage(response.Msg, "e");
            }
        });
    } else {
        location.href = app.url.server.ExportCSVorTXTAll;
    }
    $(app.domMap.Dialogboxbasic.BasicDialog).hide();
});
//REQUEST
$('body').on("click", app.domMap.ToolBarButtons.BtnRequest, function () {
    app.globalverb.Grequest.employid = "-1";
    obJtoolbarmenufunc.GetRequesterPopup();
});
$('body').on("click", "input[name = 'rdlEmployees']", (e) => {
    app.globalverb.Grequest.employid = e.target.value;
    var row = e.target.parentElement.parentElement;
    e.target.parentElement.parentElement.remove();
    $(app.domMap.Request.employeeContainer).prepend(row)
    document.querySelector(app.domMap.Request.emscrolldiv).scrollTo(0, 0);
});
$('body').on("click", "span[name = 'rdlEmployees']", (e) => {
    e.target.previousSibling.click();
});
$('body').on("keyup", app.domMap.Request.reqtxtFilter, (e) => {
    //if (e.target.value.length < 3) return
    if (e.keyCode === 13) {
        $(app.SpinningWheel.Main).show();
        var empcon = document.querySelector(app.domMap.Request.employeeContainer);
        var objc = { Request: {} };
        objc.ViewId = app.data.ViewId;
        objc.TableName = app.data.TableName;
        objc.ids = GetrowsKeyids();
        objc.ViewName = app.data.ViewName
        objc.Request.TextFilter = e.target.value;


        FETCHPOST(app.url.server.SearchEmployees, objc).then(rdata => {
            empcon.innerHTML = "";
            for (var i = 0; i < rdata.rdemployddlist.length; i++) {
                var emp = rdata.rdemployddlist[i]
                var tr = document.createElement("tr");
                if (emp.disable) {
                    tr.innerHTML = `<td><input disabled="disabled" type="radio" name="rdlEmployees" value="${emp.value}"><span style="cursor:pointer;font-size:14px">${emp.text}</span></td>`
                } else {
                    tr.innerHTML = `<td><input type="radio" name="rdlEmployees" value="${emp.value}"><span style="cursor:pointer;font-size:14px">${emp.text}</span></td>`
                }
                
                empcon.appendChild(tr);
            }
            $(app.SpinningWheel.Main).hide();
        }).catch((err) => {
            $(app.SpinningWheel.Main).hide();
            showAjaxReturnMessage(err, "e")
        })
    }
});
//$('body').on("click", "input[name = 'priority']", (e) => {
//    app.globalverb.Grequest.priority = e.target.value;
//});

$('body').on("click", "#rdhigh", (e) => {
    app.globalverb.Grequest.priority = e.currentTarget.firstChild.value
    e.currentTarget.firstChild.checked = true;
});
$('body').on("click", "#rdstandard", (e) => {
    app.globalverb.Grequest.priority = e.currentTarget.firstChild.value
    e.currentTarget.firstChild.checked = true;
})

//save request by row
$("body").on("click", "#cbException" ,(e) => {
    var comment = document.getElementById("txtComment")
    if (e.currentTarget.checked) {
        comment.disabled = false;
    } else {
        comment.disabled = true;
        comment.value = "";
    }
});

$('body').on("click", app.domMap.Request.btnSubmitRequest, () => {
    var objc = { Request: {} };
    objc.Request.Employeeid = app.globalverb.Grequest.employid;
    objc.Request.Priotiry = app.globalverb.Grequest.priority;
    objc.Request.Instruction = document.querySelector(app.domMap.Request.txtInstructions).value;
    objc.ids = GetrowsKeyids();
    objc.ViewId = app.data.ViewId
    objc.RecordCount = 0;
    objc.TableName = app.data.TableName
    objc.Request.ReqDate = document.querySelector(app.domMap.Request.txtreqDue).value;
    objc.Request.ischeckWaitlist = document.querySelector(app.domMap.Request.chkWaitList) == null ? false : document.querySelector(app.domMap.Request.chkWaitList).checked
    var msg = "Please make selection of " + '<b> Date needed by</b>'+ " from above";
    if (objc.Request.ReqDate == "") {
        showAjaxReturnMessage(msg, "w");
        return;
    }
    if (objc.Request.Employeeid == "-1") {
        showAjaxReturnMessage("Please make a Selection above to Request", "w")
        return;
    }
    $(app.SpinningWheel.Main).show();
    FETCHPOST(app.url.server.SubmitRequest, objc).then(data => {
        if (data.isError) {
            showAjaxReturnMessage(data.Msg, "e");
            $(app.SpinningWheel.Main).hide();
            return;
        }
        //showAjaxReturnMessage("Requested successfuly!", "s")
        showAjaxReturnMessage(app.language["transferrequestsuccess"], "s")
        dlgClose.CloseDialog();
        obJgridfunc.GetRowTrackTableData()
        $(app.SpinningWheel.Main).hide();
    }).catch(err => {
        $(app.SpinningWheel.Main).hide();
        console.log("developer error: " + err)
    });
});
//TRANSFER
$('body').on("click", app.domMap.ToolBarButtons.BtnTransfer, function () {
    app.globalverb.GIsTransferAllData = false;
    app.globalverb.GTransferRowcounter = rowselected.length;
    obJtoolbarmenufunc.StartTransferItems();
});

$('body').on("click", app.domMap.ToolBarButtons.BtnTransferAll, function () {
    app.globalverb.GIsTransferAllData = true;
    var data = JSON.stringify({ viewid: app.data.ViewId, searchQuery: buildQuery });
    var call = new DataAjaxCall(app.url.server.CountAllTransferData, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
    call.Send().then((rowcount) => {
        app.globalverb.GTransferRowcounter = rowcount
        obJtoolbarmenufunc.StartTransferItems();
    });

});

$("body").on('click', "#ApproveTransfer", () => {
    obJtoolbarmenufunc.GetTransferPopup();
});

$('body').on("click", "input[name = 'trlLocationType']", (e) => {
    app.globalverb.Gviewid = e.target.dataset.viewid;
    var objc = { Transfer: {} }
    objc.Transfer.ContainerViewid = e.target.dataset.viewid;
    obJtoolbarmenufunc.GetTransferItemContainers(objc);
});
$('body').on("click", "span[name = 'trlLocationType']", (e) => {
    e.target.previousSibling.click()
});


$('body').on('keyup', app.domMap.Transfer.tstxtFilter, (e) => {
    //if (e.target.value.length < 3) return
    if (app.globalverb.Gviewid === '') {
        $(app.domMap.Transfer.trlDestinationItemsContainer).html("<span style='color:red;'>Please, select transfer type before searching!</span>");
        return;
    }
    if (e.keyCode === 13) {
        var objc = { Transfer: {} }
        objc.Transfer.ContainerViewid = app.globalverb.Gviewid;
        objc.Transfer.TextFilter = e.target.value;
        obJtoolbarmenufunc.GetTransferItemContainers(objc)
    }
});

$('body').on("click", app.domMap.Transfer.btnTransferItems, () => {
    if (app.globalverb.GTransferToitem.Tableid == "-1") return showAjaxReturnMessage("Please make a Selection above to Transfer", "w")
    var objc = { Transfer: {} };
    objc.Transfer.ContainerTableName = app.globalverb.GTransferToitem.tableName;
    objc.Transfer.ContainerItemValue = app.globalverb.GTransferToitem.Tableid;
    objc.Transfer.ContainerViewid = app.globalverb.Gviewid;
    objc.ids = GetrowsKeyids();
    objc.TableName = app.data.TableName;
    objc.ViewId = app.data.ViewId;
    objc.Transfer.IsSelectAllData = app.globalverb.GIsTransferAllData;
    if (objc.Transfer.IsSelectAllData) {
        objc.RecordCount = app.globalverb.GTransferRowcounter;
    } else {
        objc.RecordCount = rowselected.length;
    }

    if (app.globalverb.GTransferToitem.isDueBack) {
        objc.Transfer.TxtDueBack = document.querySelector(app.domMap.Transfer.txtDueBack).value;
    } else {
        objc.Transfer.TxtDueBack = "";
    }
    $(app.SpinningWheel.Main).show();
    var data = JSON.stringify({ paramss: objc });
    var call = new DataAjaxCall(app.url.server.BtnTransfer, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
    call.Send().then((rdata) => {
        if (rdata.isWarning) {
            showAjaxReturnMessage(rdata.userMsg, "w")
            //dlgClose.CloseDialog(false);
            $(app.SpinningWheel.Main).hide();
            return;
        } else if (rdata.isBackground) {
            var msgprops = rdata.userMsg.split("%");
            var htmlMsg = `<p>${msgprops[0].replace("{0}", "<b>" + msgprops[1] + "</b>")}</p><p>${app.language["lblMSGExportBackgroundFileName"].replace("{0}", "<b>" + msgprops[2] + "</b>")}</p><p>${app.language["lblMSGExportBackgroundSelectedRows"].replace("{0}", "<b>" + msgprops[3] + "</b>")}</p>${app.language["lblMSGExportBackgroundCurrentStatus"].replace("{0}", "<b>Pending</b>")}<p></p>`
            var dlg = new DialogBoxBasic(app.language["lblMSGSuccessMessage"], htmlMsg, app.domMap.Dialogboxbasic.Type.Content)
            dlg.ShowDialog().then(() => { })
        } else {
            showAjaxReturnMessage(rdata.userMsg, "s")
            dlgClose.CloseDialog(false);
            hot.selectCell(rowselected[0], obJgridfunc.StartInCell())
        }
        app.globalverb.GTransferToitem.Tableid = "-1"
        $(app.SpinningWheel.Main).hide();
    });
});
$('body').on('click', "input[name = 'trDestinationsItem']", (e) => {
    app.globalverb.GTransferToitem.Tableid = e.target.value;
    app.globalverb.GTransferToitem.tableName = e.target.dataset.tablename;
    var row = e.target.parentElement.parentElement;
    e.target.parentElement.parentElement.remove();
    $(app.domMap.Transfer.trlDestinationItemsContainer).prepend(row)
    document.querySelector(app.domMap.Transfer.tsscrolldiv).scrollTo(0, 0);
});

$('body').on('click', "span[name = 'trDestinationsItem']", (e) => {
    e.target.previousSibling.click()
});


//MOVE ROW BUTTON.
$('body').on("click", app.domMap.ToolBarButtons.BtnMoverows, function () {
    obJtoolbarmenufunc.GetMovePopup();
});
$('body').on("click", "input[name = 'rblDestinations']", (e) => {
    var objc = { MoveRecords: {} };
    objc.MoveRecords.MoveViewid = e.target.dataset.viewid
    var data = JSON.stringify({ paramss: objc })
    var call = new DataAjaxCall(app.url.server.LoadDestinationItems, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
    $(app.SpinningWheel.Main).show();
    call.Send().then((rdata) => {
        if (rdata.isError === false) {
            $(app.domMap.MovingRows.rblDestinationItemsContainer).html("")
            if (rdata.rbDestinationsItem.length > 0) {
                for (var i = 0; i < rdata.rbDestinationsItem.length; i++) {
                    var text = rdata.rbDestinationsItem[i].text;
                    var value = rdata.rbDestinationsItem[i].value;
                    $(app.domMap.MovingRows.rblDestinationItemsContainer).append(`<tr><td><input type="radio" name="rbDestinationsItem" value="${value}"><span name="rbDestinationsItem" style="font-size:14px;cursor:pointer">${text}</span></td></tr>`)
                }
                gMoveView = e.target.dataset.viewid
                $(app.domMap.MovingRows.txtFilter).val("");
            } else {
                showAjaxReturnMessage("No row found in this view", "w")
            }
        } else {
            showAjaxReturnMessage(rdata.Msg, 'e');
        }
        $(app.SpinningWheel.Main).hide();
    });
});
$('body').on("click", "span[name = 'rblDestinations']", (e) => {
    e.target.previousSibling.click();
});


$('body').on("click", "input[name = rbDestinationsItem]", (e) => {
    var row = e.target.parentElement.parentElement;
    e.target.parentElement.parentElement.remove();
    $(app.domMap.MovingRows.rblDestinationItemsContainer).prepend(row)
    document.querySelector(app.domMap.MovingRows.scroller).scrollTo(0, 0);
});

$('body').on("click", "span[name = rbDestinationsItem]", (e) => {
    e.target.previousSibling.click();
});

$('body').on("keyup", app.domMap.MovingRows.txtFilter, (e) => {
    //if (e.target.value.length < 3) return
    var objc = { MoveRecords: {} };
    if (e.keyCode === 13) {
        e.currentTarget.disabled = true;
        objc.MoveRecords.MoveViewid = gMoveView;
        objc.MoveRecords.TextFilter = e.target.value;
        var data = JSON.stringify({ paramss: objc })
        var call = new DataAjaxCall(app.url.server.FilterItemsList, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        $(app.SpinningWheel.Main).show();
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                $(app.domMap.MovingRows.rblDestinationItemsContainer).html("");
                var lis = rdata.Data;
                for (var i = 0; i < lis.length; i++) {
                    $(app.domMap.MovingRows.rblDestinationItemsContainer).append(`<tr><td><input type="radio" name="rbDestinationsItem" value="${lis[i].value}"><span style="font-size:14px">${lis[i].text}</span></td></tr>`)
                }
                $(app.SpinningWheel.Main).hide();
                e.currentTarget.disabled = false;
                e.currentTarget.focus()
            } else {
                $(app.SpinningWheel.Main).hide();
                showAjaxReturnMessage(rdata.Msg, 'e');
            }
        });
    }
});
$('body').on("click", app.domMap.MovingRows.btnMove, () => {
    var objc = { MoveRecords: {} };

    if ($("input[name = rbDestinationsItem]")[0]?.checked) {
        objc.MoveRecords.fieldItemValue = $("input[name = rbDestinationsItem]")[0].value;
        objc.MoveRecords.fieldName = getMovefieldName();
        objc.MoveRecords.MoveViewid = getMoveParentViewId();
        objc.ids = GetrowsKeyids();
        objc.ViewId = app.data.ViewId;
        objc.TableName = app.data.TableName;
        var data = JSON.stringify({ paramsUI: objc });
        var call = new DataAjaxCall(app.url.server.BtnMoveItems, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                //showAjaxReturnMessage("Moved successfuly!", "s")
                showAjaxReturnMessage(app.language["MovedSuccessfulyMessage"], "s")

                obJgridfunc.LoadViewTogrid(app.data.ViewId, pagingfun.SelectedIndex).then(() => {
                    dlgClose.CloseDialog()
                });
            } else {
                showAjaxReturnMessage(rdata.Msg, "e")
            }
        });
    } else {
        showAjaxReturnMessage("Please make a Selection above to Move", "w")
    }
});
function getMovefieldName() {
    var lst = $("input[name = rblDestinations]")
    for (var i = 0; i < lst.length; i++) {
        if (lst[i].checked) {
            return lst[i].value
        }
    }
}
function getMoveParentViewId() {
    var lst = $("input[name = rblDestinations]")
    for (var i = 0; i < lst.length; i++) {
        if (lst[i].checked) {
            return lst[i].dataset.viewid
        }
    }
}

//TRACKING HIDE AND SHOW
$('body').on("click", app.domMap.ToolBarButtons.BtnTracking, function () {
    if ($(this).html() === "Hide Tracking") {
        $(this).html("Show Tracking")
        $(app.domMap.DataGrid.TrackingStatusDiv).hide();
    } else {
        $(this).html("Hide Tracking")
        $(app.domMap.DataGrid.TrackingStatusDiv).show();
    }
})
//COLLAPS
$('body').on('shown.bs.collapse', function () {
    $(app.domMap.DataGrid.HandsOnTableContainer).height(CalculateHeightGridSize());
    setTimeout(() => {
        if (hot === undefined) return;
        hot.render()
    }, 500)
});
$('body').on('hidden.bs.collapse', function () {
    $(app.domMap.DataGrid.HandsOnTableContainer).height(CalculateHeightGridSize());
    setTimeout(() => {
        if (hot === undefined) return;
        hot.render()
    }, 500)
});
$(document).ready(function () {
    $(window).resize(function () {
        if (hot !== undefined) {
            $(app.domMap.DataGrid.HandsOnTableContainer).height(CalculateHeightGridSize());
        }
        if (hot === undefined) return;
        setTimeout(() => {
            hot.render()
        }, 500)
    });
})
//TRACKING HIDE AND SHOW FUNCTIONS
function TrackingHideShowClick(id) {
    var result = $(id).text().split(" ");
    if (result[0] === "Hide") {
        $(id).text(app.language["Show"] + " [+]");
    } else {
        $(id).text(app.language["Hide"] + " [-]");
    }
}
function TaskListSetHideShowText(sel) {
    var result = $(sel).text().split(" ");
    if (result[result.length - 1] == "[+]") {
        $(sel).text(app.language["Hide"] + " [-]");
    } else {
        $(sel).text(app.language["Show"] + " [+]");
    }
}
$("body").on("click", app.domMap.NewsFeed.BtnSaveNewsURL, function () {
    runfeed.SaveNewUrl();
    window.location.reload();
});
//CLICK ON TAVQUIK BUTTON
$("body").on('click', app.domMap.ToolBarButtons.BtnTabquikId, function () {
    if (rowselected.length < 1) {
        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-warning', message: app.language["msgJsDataSelectOneRow"], timeout: 3000 });
        showAjaxReturnMessage(app.language["msgJsDataSelectOneRow"], "w");
    } else {
        var rowids = [];
        for (var i = 0; i < rowselected.length; i++) {
            let rowid = hot.getDataAtRow(rowselected[i])[0];
            if (app.data.IdFieldDataType == "System.String") {
                rowids.push(`'${rowid}'`);
            } else {
                rowids.push(rowid);
            }
        }
        setCookie('TabQuikViewRowselected', obJgridfunc.ViewId + "^&&^" + rowids, { expires: 1 });
        let url = window.location.origin;
        window.open(url + '/Data/TabQuik', '_blank');
    }
});
//DELETE ROW FROM GRID
$("body").on('click', app.domMap.ToolBarButtons.BtnDeleterow, function (e) {
    obJtoolbarmenufunc.BeforeDeleteRow();
});
//FAVORIRTE
$("body").on("click", app.domMap.ToolBarButtons.BtnDeleteCeriteria, function () {
    obJfavorite.DeleteCeriteria();
})
$("body").on("click", app.domMap.ToolBarButtons.BtnNewFavorite, function () {
    obJfavorite.LoadNewFavorite();
});
$("body").on("click", app.domMap.Favorite.NewFavorite.Ok, function (e) {
    var favName = document.querySelector(app.domMap.Favorite.NewFavorite.FavNewInputBox).value;
    //if (favName == "") { return showAjaxReturnMessage("please, insert favorite name (needs to add the language file)(needs to add the language file)", "w") }
    if (favName == "") { return showAjaxReturnMessage(app.language["msgFavoriteName"], "w") }
    obJfavorite.AddNewFavorite(favName);
});
$("body").on("click", app.domMap.ToolBarButtons.BtnUpdateFavourite, function () {
    obJfavorite.BeforeAddToFavorite();
});
$("body").on("click", app.domMap.Favorite.AddToFavorite.AddtofavoriteOK, function () {
    favName = document.querySelector(app.domMap.Favorite.AddToFavorite.DDLfavorite).value
    if (favName == "" || favName == "0") { return showAjaxReturnMessage(app.language["msgFavoriteName"], "w") }
    obJfavorite.UpdateFavorite();
});
$("body").on("click", app.domMap.ToolBarButtons.BtnremoveFromFavorite, function () {
    if (rowselected.length <= 0) return showAjaxReturnMessage(app.language["msgFavoriteRecordSelection"], "w")
    var dlg = new DialogBoxBasic("<h3>" + app.language["FavoriteDeleteConfBox"] + "</h3>", app.url.Html.DialogMsgDeleteFavoriteConfirm, app.domMap.Dialogboxbasic.Type.PartialView)
    dlg.ShowDialog().then(() => {
        document.querySelector(app.domMap.Dialogboxbasic.Dlg.DialogMsgConfirm.DialogMsgTxt).innerHTML = app.language["FavoriteDeleteConfBoxMessage"];
        document.querySelector(app.domMap.Dialogboxbasic.Dlg.DialogMsgConfirm.DialogYes).id = "removeRecord";
    })
});
$("body").on("click", app.domMap.Favorite.RemoveFavorite.RemoveConfimed, function () {
    obJfavorite.DeleteFavoriteRecords();
})
//IMPORT FAVORITE
$("body").on("click", app.domMap.ToolBarButtons.BtnImportFavourite, () => {
    $(app.SpinningWheel.Main).show();
    FETCHGET(app.url.server.GetImportFavoritpopup, "html").then(data => {
        var dlg = new DialogBoxBasic("Import Favorite", data, app.domMap.Dialogboxbasic.Type.Content)
        dlg.ShowDialog().then(() => {
            $(app.SpinningWheel.Main).hide();
        });
    });
});
$('body').on("change", app.domMap.Favorite.ImportFavorite.ddlFavoriteList, (e) => {
    var fieldsbody = document.querySelector(app.domMap.Favorite.ImportFavorite.listoffieldsbody);
    fieldsbody.innerHTML = "";
    var objc = { ImportFavorite: {} }
    objc.ImportFavorite.SelectedDropdown = e.target.value.split("|")[1];
    FETCHPOST(app.url.server.ImportFavoriteDDLChange, objc).then(data => {

        if (data.isError) {
            showAjaxReturnMessage(data.Msg, "e");
            return;
        }

        //if (data.isfieldsExist === false) {
        //    fieldsbody.innerHTML = "";
        //    return;
        //}
        for (var i = 0; i < data.ListOfFieldName.length; i++) {
            var field = data.ListOfFieldName[i].text;
            var fieldType = data.ListOfFieldName[i].value;
            var tr = document.createElement("tr");
            tr.innerHTML = `<td><label><input value="${fieldType}" name="fieldsChk" type="checkbox" RepeatDirection="Vertical" Class="viewFields" /><span>${field}</span> - <span style="color:lightgray">${fieldType}</span></label></td>`
            fieldsbody.append(tr);
        }

    });
});
$('body').on("change", app.domMap.Favorite.ImportFavorite.fileSource, (e) => {
    var sourceCols = document.querySelector(app.domMap.Favorite.ImportFavorite.rblSourceCol1);
    sourceCols.innerHTML = "";
    var formdata = new FormData();
    var file = e.currentTarget.files[0];
    var ext = file.name.split(".")[1];
    //check if file supported UI side
    if (ext !== "txt" && ext !== "xls" && ext !== "xlsx" && ext !== "csv") {
        showAjaxReturnMessage("Only txt, csv, xls or xlsx files are supported", "w")
        e.target.value = "";
        return;
    }

    $(app.domMap.Favorite.ImportFavorite.InputFileName).val(file.name);
    e.target.value = "";
    formdata.append(file.name, file)
    formdata.append("param", document.querySelector(app.domMap.Favorite.ImportFavorite.chk1RowFieldNames).checked);

    var call = new DataAjaxCall(app.url.server.UploadingFile, ajax.Type.Post, ajax.DataType.Json, formdata, ajax.ContentType.False, ajax.ProcessData.False, "", "")
    call.Send().then((rdata) => {

        if (rdata.isError) {
            showAjaxReturnMessage(rdata.Msg, "e");
            e.target.value = "";
            return;
        }

        if (rdata.isFilesupported == false) {
            showAjaxReturnMessage("Only txt, csv, xls or xlsx files are supported", "w")
            e.target.value = "";
            return;
        }

        for (var i = 0; i < rdata.ListOfcolumns.length; i++) {
            var colstr = rdata.ListOfcolumns[i];
            var tr = document.createElement("tr");
            tr.innerHTML = `<td><label><input value="${i}" type="radio" name="radiocols" RepeatDirection="Vertical" Class="viewFields" /><span>${colstr}</span></label></td>`
            sourceCols.append(tr);
        }
    });

});
$('body').on("click", app.domMap.Favorite.ImportFavorite.btnImportFavRecordOk, (e) => {
    var btnReport = document.querySelector(app.domMap.Favorite.ImportFavorite.ImportFavoritesAReport);
    btnReport.style.display = "none";
    //set up properties
    var data = { ImportFavorite: {} };
    data.ViewId = document.querySelector(app.domMap.Favorite.ImportFavorite.ddlFavoriteList).value.split("|")[1]
    data.ImportFavorite.chk1RowFieldNames = document.querySelector(app.domMap.Favorite.ImportFavorite.chk1RowFieldNames).checked;
    data.ImportFavorite.sourceFile = document.querySelector(app.domMap.Favorite.ImportFavorite.InputFileName).value;
    data.ImportFavorite.isRightColumnChk = false;
    data.ImportFavorite.Targetfileds = [];
    var TargetFields = document.getElementsByName("fieldsChk")
    for (var i = 0; i < TargetFields.length; i++) {
        if (TargetFields[i].checked) {
            data.ImportFavorite.isRightColumnChk = true;
            data.ImportFavorite.Targetfileds.push({ value: TargetFields[i].value, text: TargetFields[i].nextElementSibling.innerText })
        }
    }
    data.ImportFavorite.isLeftColumnChk = false;
    var columns = document.getElementsByName("radiocols");

    for (var i = 0; i < columns.length; i++) {
        if (columns[i].checked) {
            data.ImportFavorite.isLeftColumnChk = true;
            data.ImportFavorite.ColumnSelect = columns[i].value;
        }
    }
    data.ImportFavorite.favoritListSelectorid = document.querySelector(app.domMap.Favorite.ImportFavorite.ddlFavoriteList).value.split("|")[0]
    data.ImportFavorite.IscolString = app.globalverb.GIsstring == "" ? false : app.globalverb.GIsstring;
    //call server
    FETCHPOST(app.url.server.BtnImportfavorite, data).then(rdata => {
        if (!rdata.isValidate) {
            showAjaxReturnMessage(rdata.UserMsg, "w");
            return;
        }
        if (rdata.isError) {
            if (rdata.errorType == "w") {
                showAjaxReturnMessage(rdata.UserMsg, "w")
            }
            else {
                showAjaxReturnMessage(rdata.UserMsg, "e")
            }
            return;
        } else {
            showAjaxReturnMessage(rdata.UserMsg, "s")
            btnReport.style.display = "block";
            btnReport.href = rdata.AReportUrl;

        }

    }).catch(err => {

    });
});
$('body').on("click", "input[name = fieldsChk]", (e) => {
    var type = e.currentTarget.value
    var listOfFields = document.getElementsByName("fieldsChk")
    if (e.currentTarget.checked) {
        if (type == "String" || type == "DateTime") {
            app.globalverb.GIsstring = true;
            for (var i = 0; i < listOfFields.length; i++) {
                var field = listOfFields[i];
                if (field.value == "Int32" || field.value == "Int16" || field.value == "Boolean" || field.value == "Decimal") {
                    field.checked = false;
                }
            }
        }
        if (type == "Int32" || type == "Int16" || type == "Decimal") {
            app.globalverb.GIsstring = false;
            for (var i = 0; i < listOfFields.length; i++) {
                var field = listOfFields[i];
                if (field.value == "String" || field.value == "DateTime" || field.value == "Boolean") {
                    field.checked = false;
                }
            }
        }

        if (type == "Boolean") {
            app.globalverb.GIsstring = true;
            for (var i = 0; i < listOfFields.length; i++) {
                var field = listOfFields[i];
                if (field.value == "String" || field.value == "DateTime" || field.value == "Int32" || field.value == "Int16" || field.value == "Decimal") {
                    field.checked = false;
                }
            }
        }
    }
});
//MY QUERY
$("body").on("click", app.domMap.Myqury.BtnDeleteMyquery, function () {
    obJmyquery.DeleteQuery();
});
//GLOBAL SEARCH
$("body").on("click", app.domMap.GlobalSearching.BtnSearch, function () {
    var attachment = document.querySelector(app.domMap.GlobalSearching.ChkAttachments).checked;
    var currentTable = document.querySelector(app.domMap.GlobalSearching.ChkCurrentTable).checked;
    var currentrow = document.querySelector(app.domMap.GlobalSearching.ChkUnderRow).checked;
    var value = document.querySelector(app.domMap.GlobalSearching.TxtSearch).value;

    obJglobalsearch.RunSearch(false, value, attachment, currentTable, currentrow);
});
$("body").on("keypress", app.domMap.GlobalSearching.TxtSearch, function (e) {
    if (e.keyCode === 13) {
        var attachment = document.querySelector(app.domMap.GlobalSearching.ChkAttachments).checked;
        var currentTable = document.querySelector(app.domMap.GlobalSearching.ChkCurrentTable).checked;
        var currentrow = document.querySelector(app.domMap.GlobalSearching.ChkUnderRow).checked;
        var value = document.querySelector(app.domMap.GlobalSearching.TxtSearch).value;
        obJglobalsearch.RunSearch(false, value, attachment, currentTable, currentrow);
    }
});
$("body").on("keypress", app.domMap.GlobalSearching.DialogSearchInput, function (e) {
    if (e.keyCode === 13) {
        var value = document.querySelector(app.domMap.GlobalSearching.DialogSearchInput).value;
        var isattach = document.querySelector(app.domMap.GlobalSearching.DialogchkAttachments).checked;
        var isCurTable = document.querySelector(app.domMap.GlobalSearching.DialogCurrenttable).checked;
        var isRow = document.querySelector(app.domMap.GlobalSearching.DialogUnderthisrow).checked;
        obJglobalsearch.RunSearch(true, value, isattach, isCurTable, isRow);
    }
});
$("body").on("click", app.domMap.GlobalSearching.DialogchkAttachments, function () {
    var value = document.querySelector(app.domMap.GlobalSearching.DialogSearchInput).value;
    var isattach = this.checked;
    var isCurTable = document.querySelector(app.domMap.GlobalSearching.DialogCurrenttable).checked;
    var isRow = document.querySelector(app.domMap.GlobalSearching.DialogUnderthisrow).checked;
    obJglobalsearch.RunSearch(true, value, isattach, isCurTable, isRow);

});
$("body").on("click", app.domMap.GlobalSearching.DialogSearchButton, function () {
    var value = document.querySelector(app.domMap.GlobalSearching.DialogSearchInput).value;
    var isattach = document.querySelector(app.domMap.GlobalSearching.DialogchkAttachments).checked;
    var isCurTable = document.querySelector(app.domMap.GlobalSearching.DialogCurrenttable).checked;
    var isRow = document.querySelector(app.domMap.GlobalSearching.DialogUnderthisrow).checked;
    obJglobalsearch.RunSearch(true, value, isattach, isCurTable, isRow);
});
$("body").on("click", app.domMap.GlobalSearching.DialogCurrenttable, function () {
    var value = document.querySelector(app.domMap.GlobalSearching.DialogSearchInput).value;
    var isattach = document.querySelector(app.domMap.GlobalSearching.DialogchkAttachments).checked;
    var isCurTable = document.querySelector(app.domMap.GlobalSearching.DialogCurrenttable).checked;
    var isRow = document.querySelector(app.domMap.GlobalSearching.DialogUnderthisrow).checked;
    obJglobalsearch.RunSearch(true, value, isattach, isCurTable, isRow);
});
$("body").on("click", app.domMap.GlobalSearching.DialogUnderthisrow, function () {
    var value = document.querySelector(app.domMap.GlobalSearching.DialogSearchInput).value;
    var isattach = document.querySelector(app.domMap.GlobalSearching.DialogchkAttachments).checked;
    var isCurTable = document.querySelector(app.domMap.GlobalSearching.DialogCurrenttable).checked;
    var isRow = document.querySelector(app.domMap.GlobalSearching.DialogUnderthisrow).checked;
    obJglobalsearch.RunSearch(true, value, isattach, isCurTable, isRow);
});
//ATTACHMENT POPUP
$("body").on("click", app.domMap.DialogAttachment.BtnAddAttachment, function (e) {
    e.stopPropagation();
    $(app.domMap.DialogAttachment.FileUploadForAddAttach).off().on('change', function (e) {
        oFileUpload = document.querySelector(app.domMap.DialogAttachment.FileUploadForAddAttach);
        FlyoutUploderFiles = oFileUpload.files;
        obJattachmentsview.UploadAttachmentOnNewAdd(FlyoutUploderFiles);
    });
    $(app.domMap.DialogAttachment.FileUploadForAddAttach).val("");
    $(app.domMap.DialogAttachment.FileUploadForAddAttach).click();

});
$("body").on("click", app.domMap.DialogAttachment.GotoScanner, function (e) {
    scannerLink = window.location.origin + "/DocumentViewer/Index?documentKey=" + document.querySelector(app.domMap.DialogAttachment.StringQuery).value;
    localStorage.setItem("callFromScanner", 1);
    var url = scannerLink;
    window.open(url, "_blank");
});
$("body").on("click", app.domMap.DialogAttachment.BtnopenAttachViewer, function (e) {
    scannerLink = window.location.origin + "/DocumentViewer/Index?documentKey=" + document.querySelector(app.domMap.DialogAttachment.StringQuery).value;
    localStorage.setItem("callFromScanner", 0);
    var url = scannerLink;
    window.open(url, "_blank");
});

//RETENTION INFORMATION
$('body').on('change', app.domMap.Retentioninfo.ddlRetentionCode, (e) => {
    var getddl = e.currentTarget.options[e.currentTarget.selectedIndex];
    var elem = {};
    elem.description = document.querySelector(app.domMap.Retentioninfo.RetinCodeDesc);
    elem.retentionItem = document.querySelector(app.domMap.Retentioninfo.RetinItemName);
    elem.status = document.querySelector(app.domMap.Retentioninfo.RetinStatus);
    elem.inArchiveDate = document.querySelector(app.domMap.Retentioninfo.RetinInactiveDate);
    elem.lblArchive = document.querySelector(app.domMap.Retentioninfo.lblRetinArchive);
    elem.retArchive = document.querySelector(app.domMap.Retentioninfo.RetinArchive);

    if (getddl.value === "") {
        elem.description.innerText = "N/A";
        elem.retentionItem.innerText = "Record Details";
        elem.status.innerText = "N/A";
        elem.inArchiveDate.innerText = "N/A";
        elem.retArchive.innerText = "N/A";
        return;
    }
    objc = {};
    objc.rowid = obJgridfunc.RowKeyid
    objc.viewid = obJgridfunc.ViewId;
    objc.RetDescription = getddl.value;
    objc.RetentionItemText = getddl.innerText;
    var data = JSON.stringify({ props: objc })
    var call = new DataAjaxCall(app.url.server.OnDropdownChange, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
    call.Send().then((model) => {
        obJretentioninfo.DropdownReturnAfterChange(model, elem)
    });

});
$('body').on("click", app.domMap.Retentioninfo.btnOkRetin, () => {
    obJretentioninfo.UpdateRecordInfo();
});
$('body').on("click", app.domMap.Retentioninfo.btnRemoveRetin, () => {
    //jira 11303
    if (hotRetention.getSelected() === undefined) {
        showAjaxReturnMessage(app.language["MsgRetentionInformationHoldSelectRowRemove"], "w")
    }
    else {
        obJretentioninfo.RemoveRows();
    }
});
$('body').on('click', app.domMap.Retentioninfo.btnCancelRetin, () => {
    var dlgClose = new DialogBoxBasic();
    dlgClose.CloseDialog();
});
//POPUP HOLDING TABLE TO ADD MORE ROWS
$('body').on('click', app.domMap.Retentioninfo.btnAddRetin, () => {
    obJretentioninfo.StartAddingRow();
});
//RETENTION HOLDING 
//BTN SNOOZ BUTTON
$('body').on('click', app.domMap.RetentioninfoHold.btnSnooze, () => {
    var snooz = document.querySelector(app.domMap.RetentioninfoHold.txtSnoozeDate);
    var retType = document.querySelector(app.domMap.RetentioninfoHold.chkHoldTypeRetention);
    var LegType = document.querySelector(app.domMap.RetentioninfoHold.chkHoldTypeLegal);
    var reason = document.querySelector(app.domMap.RetentioninfoHold.holdReason);
    if (snooz.disabled === false) {
        IncreaseDateByoneMonth(snooz);
    } else {
        if (!LegType.checked) {
            retType.checked = true;
        }
        reason.disabled = false;
        snooz.disabled = false;
    }
});
//BTN RETENTION TYPE CHECKBOX
$('body').on('click', app.domMap.RetentioninfoHold.chkHoldTypeRetention, () => {
    obJretentioninfo.HoldingConditions("retention");
});
//BTN LEGAL CHECKBOX
$('body').on('click', app.domMap.RetentioninfoHold.chkHoldTypeLegal, () => {
    obJretentioninfo.HoldingConditions("legal");
});
//BTN OK BUTTON
$('body').on('click', app.domMap.RetentioninfoHold.btnHoldOk, () => {
    if (obJretentioninfo.isEditMode) {
        obJretentioninfo.EditHoldingRow();
    } else {
        obJretentioninfo.AddnewHolding();
    }
    //hide holding dialog after Ok button click
    // $(app.domMap.RetentioninfoHold.Dialog.RetentionHoldingDialog).hide();
});
$('body').on('click', app.domMap.Retentioninfo.btnEditRetin, () => {
    if (hotRetention.getSelected() === undefined) {
        //showAjaxReturnMessage("Please, select row to edit!", "w") 
        showAjaxReturnMessage(app.language["MsgRetentionInformationHoldSelectRow"], "w")
    } else if (obJretentioninfo.GetrowsSelected().length > 1) {
        showAjaxReturnMessage("You can't edit more than one row!", "w")
    } else {
        $(app.domMap.RetentioninfoHold.Dialog.RetentionHoldingDialog).show();
        $(app.domMap.RetentioninfoHold.Dialog.DlgHoldingTitle).html(app.language["lblRetentionInformationHoldInfo"]);
        $(app.domMap.RetentioninfoHold.Dialog.RetentionHoldingContent).load(app.url.server.RetentionInfoHolde, () => {
            obJretentioninfo.StartEditHoldingRow();
        })
    }

});


var clicked = 0;
$("body").on("mouseleave", "#drill1", () => {
    //check if drill down is open if yes close it. 
    var drill = document.getElementById("drill1");
    if (drill != null) {
        drill.remove();
        clicked = 0;
    }
});
function BuildDrillDownList(e) {
    var drill = document.getElementById("drill1");
    if (clicked == 0) {
        if (drill != null) drill.remove(); //check if there is any drill open (drill must remove before creating a new one)
        var div = document.createElement("ul");
        div.id = "drill1";
        div.style.top = e.target.parentElement.parentElement.parentElement.parentElement.offsetTop + 25
        div.className = "Customdrilldown"
        div.innerHTML = app.data.ListOfdrilldownLinks;
        e.target.parentElement.before(div);
        if (div.offsetTop + div.offsetHeight > window.innerHeight) {
            div.style.top = div.offsetTop - div.offsetHeight - 25;
        }
        clicked = 1;
    } else {
        if (drill != null) drill.remove(); //check if there is any drill open (drill must remove before creating a new one)
        clicked = 0;
    }
}



////for drilldown
//(function () {
//    // hold onto the drop down menu
//    var dropdownfix;
//    // and when you show it, move it to the body
//    $("body").on('show.bs.dropdown', function (e) {
//        // grab the menu
//        dropdownfix = $(e.target).find('.grid_drildown');

//        // detach it and append it to the body
//        $('body').append(dropdownfix.detach());

//        // grab the new offset position
//        var eOffset = $(e.target).offset();

//        // make sure to place it where it would normally go (this could be improved)
//        dropdownfix.css({
//            'display': 'block',
//            'top': eOffset.top + $(e.target).outerHeight(),
//            'left': eOffset.left
//        });
//    });

//    // and when you hide it, reattach the drop down, and hide it normally
//    $(window).on('hide.bs.dropdown', function (e) {
//        if (dropdownfix != undefined) {
//            $(e.target).append(dropdownfix.detach());
//            dropdownfix.hide();
//        }
//    });
//    $('input[type="checkbox"]').on('change', function () {
//        console.log($(this).data('row'));
//    })
//})();








