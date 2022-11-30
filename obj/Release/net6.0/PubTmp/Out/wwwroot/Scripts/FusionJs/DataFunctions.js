/*dictionary
#showAjaxReturnMessage(msg, type) = toast popup object for messaging.
---------------------------------------------------------------------
#rowselected = selected rows (build for handsontable).
---------------------------------------------------------------------
#app = global object that control the frontend
---------------------------------------------------------------------
#buildQuery = build query object from the query window object
---------------------------------------------------------------------
#hot = handsontable global object
---------------------------------------------------------------------
#bc = object builder for breadcrumbs
---------------------------------------------------------------------
*/
let dlg;
let rowselected = [];
var buildQuery = [];
var buildQueryParents = [{ parent: "", crumblevel: "" }]
var buildAddNewParent = [{ value: "", columnName: "", DataTypeFullName: "" }]
let LinkScriptObjective;
let hot;
let hotRetention
var bc = document.querySelector(app.domMap.Breadcrumb.olContainer);


//GRID FUNCTIONS
class GridFunc {
    constructor() {
        this.StartQuery = getCookie('startQuery');
        this.SaveQueryContent = "";
        this.ViewId = "";
        this.ViewName = "";
        this.TableName = "";
        this.pageNum = "";
        this.ViewTitle = "";
        this.RowKeyid = "";
        this.FavCeriteriaid = "";
        this.FavCriteriaType = "";
        this._isTableTrackble = "";
        this.KeyValue = "";
        this.AfterChange = [];
        this.BeforeChange = [];
        this.LastRowSelected = null;
        this.BuildRowFromCells = [];
        this.LastLeftmenuClick = "";
        this.isPagingClick = false;
        this.ViewType = "";
        this.isConditionsPass = true;
        this.isRefresh = false;
        this.childkeyfield = "";
    }
    LoadView(elemnt, viewId) {
        app.DomLocation.Previous = app.DomLocation.Current;
        app.DomLocation.Current = elemnt;
        this.isPagingClick = false;
        obJbreadcrumb.crumbLevel = 0;
        //reset and empty objects 
        //obJbreadcrumb = new BreadCrumb();
        //obJdrildownclick = new DrilldownClick();
        //buildQuery = [];
        //by default set startquery cookie to rtue
        if (getCookie("startQuery") === null) setCookie('startQuery', "True");
        this.ViewId = viewId;
        this.ViewName = elemnt.dataset.viewname;
        this.ViewTitle = elemnt.innerText;
        this.StartQuery = getCookie('startQuery');
        this.ViewType = app.Enums.ViewType.FusionView;

        //check if requeired open query window
        if (this.StartQuery == "True" && obJbreadcrumb.isClickoncrumbs == false && obJgridfunc.isRefresh == false) {
            obJreportsrecord.isCustomeReportCall = false;
            //obJlastquery.CheckIfLastQuery(this.ViewId);
            obJquerywindow.LoadQueryWindow(0);
        } else {
            bc.innerHTML = "";
            obJbreadcrumb.isClickoncrumbs = false;
            obJgridfunc.isRefresh = false;
            this.LoadViewTogrid(viewId, 1);
        }
        //obJleftmenu.ResetMenu();
        this.MarkSelctedMenu(elemnt)
    }
    MarkSelctedMenu(elemnt) {
        //select the menu clicked.
        // elemnt.parentElement.parentElement.parentElement.firstChild.click()
        $('.divMenu').find('a.selectedMenu').removeClass('selectedMenu');
        elemnt.className = 'selectedMenu';
    }
    UnMarkSelectionMenu(elemnt) {
        $('.divMenu').find('a.selectedMenu').removeClass('selectedMenu');
        elemnt.className = '';
    }
    LoadViewTogrid(viewId, PageNum) {
        var _this = this;
        this.pageNum = PageNum;
        var querybuilder = obJlastquery.CheckIfLastQuery(_this.ViewId);
        this.crumbLevel = obJbreadcrumb.crumbLevel;
        var tempViewType = this.ViewType;
        if (this.crumbLevel > 0) {
            this.ViewType = app.Enums.ViewType.FusionView;
        }
        var data = JSON.stringify({ paramss: this, searchQuery: querybuilder });
        return new Promise((resolve, reject) => {
            var call = new DataAjaxCall(app.url.server.RunQuery, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
            $(app.SpinningWheel.Main).show();
            call.Send().then((data) => {
                if (data.isError) {
                    showAjaxReturnMessage(data.Msg, "e");
                    $(app.SpinningWheel.Main).hide();
                    reject();
                    return;
                }
                _this.ViewType = tempViewType;
                _this.LoadGridpartialView(data).then(function () {
                    $(app.SpinningWheel.Main).hide();
                    resolve(true);
                });
            });
        });
    }
    //main call to return grid data to the dom;
    LoadGridpartialView(data) {
        //clear array builder for query.
        app.DomLocation.Previous = app.DomLocation.Current;
        var _this = this;
        _this.ViewTitle = data.ViewName;
        //_this.TableName = data.TableName;
        //_this.ViewName = data.ViewName
        return new Promise((resolve, reject) => {
            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e");
                $(app.SpinningWheel.Main).hide();
                reject();
                return;
            }
            $("#mainContainer").load(app.url.server.DataGridView, function () {
                app.data = data;
                $("#spanTotalRecords").hide();
                $("#spinTotal").show();
                resolve();
                _this.AfterLoadPartialToGrid(data);
            });
        });
    }
    AfterLoadPartialToGrid(data) {
        var _this = this;
        //written for document viewer
        localStorage.setItem("viewId", data.ViewId);
        obJgridfunc.ViewId = data.ViewId;
        //build toolbar for view
        $(app.domMap.DataGrid.ToolsBarDiv).html(data.ToolBarHtml);
        //check if there are attach, drilldown row if yes? give static width
        //obJHst.colWidthArray = _this.CalcFirstThreeColsWidth(data);
        if (obJlastquery.isQueryApplied) {
            document.querySelector(app.domMap.ToolBarButtons.BtnQuery).style.backgroundColor = "#F0F8FF";
        } else {
            document.querySelector(app.domMap.ToolBarButtons.BtnQuery).style.backgroundColor = "white";
        }

        //create the first crumb
        obJbreadcrumb.CreateFirstCrumb(data);

        //check autosave
        CheckrowsAutosave("gridload");
        if (data.ListOfDatarows.length === 0) {
            this.OnEmptyGrid();
            if (obJgridfunc.ViewType !== 1) {
                $("#emptymsg").show();
            }
            return;
        } else {
            HandsOnTableViews.RunhandsOnTable(data);
        }

        //check condition before showing the TrackingStatusDiv
        _this._isTableTrackble = data.ShowTrackableTable;
        data.ShowTrackableTable != true ? $(app.domMap.DataGrid.TrackingStatusDiv).hide() : $(app.domMap.DataGrid.TrackingStatusDiv).show();
        //if (obJmyquery.updatemode) {
        //    $(app.domMap.ToolBarButtons.DivFavOptions).after(app.domBuilder.BtnRemoveMyquery);
        //}
        //checkboxes condition for global search
        document.querySelector(app.domMap.GlobalSearching.ChkCurrentTable).parentElement.style.display = "block";
        document.querySelector(app.domMap.GlobalSearching.ChkUnderRow).parentElement.style.display = "block";
        //clear the app.globalverb.SaveBuildRow array to start new array before saving
        obJgridfunc.BuildRowFromCells = [];
        //select the first column and start in the first available cell
        hot.selectCell(0, _this.StartInCell());
        //click on the dom after new view loaded(the purpose of doing it is because we want to refresh the drill down link)
        var MainContainer = document.querySelector(app.domMap.DataGrid.MainDataContainerDiv);
        MainContainer.click();
    }
    OnEmptyGrid() {
        var msg = document.getElementById("mainDataContainer")
        var paging = document.getElementById("paging-div");
        paging.style.display = "none";
        //msg.style.top = "115px";
        //msg.style.left = "20%";
        //msg.style.fontSize = "22px"
        //msg.innerHTML = "No record found (Temp message:  Need to discuss with Evan.)";
    }
    HideQueryBody(elem) {
        var arrow = $(elem);
        if (arrow.hasClass("fa fa-angle-up")) {
            $("#modelBody").hide().next().hide();
            arrow.removeClass("fa fa-angle-up");
            arrow.addClass("fa fa-angle-down");
        } else {
            $("#modelBody").show().next().show();
            arrow.removeClass("fa fa-angle-down");
            arrow.addClass("fa fa-angle-up");
        }
    }
    GetRowTrackTableData() {
        var call = new DataAjaxCall(app.url.server.GetTrackbaleDataPerRow, ajax.Type.Get, ajax.DataType.Json, this, "", "", "", "")
        call.Send().then((data) => {

            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e")
                $(app.SpinningWheel.Main).hide();
                return;
            }

            document.getElementById("lblTrackTime").innerHTML = data.lblTrackTime;
            document.getElementById("lblTracking").innerHTML = data.lblTracking;
            document.getElementById("lblDueBack").innerHTML = data.lblDueBack;
            var rowcontainer = document.getElementById("trackingbody")
            rowcontainer.innerHTML = "";
            for (var i = 0; i < data.ListofRequests.length; i++) {
                var num = `${i + 1}.`
                var item = data.ListofRequests[i]
                var tr = document.createElement("tr");
                tr.innerHTML = `<tr><td>${num}</td><td></td><td>${item.DateRequested}</td><td>${item.EmployeeName}</td><td>${item.DateNeeded}</td><td>${item.Status}</td><td><img src="images/notepad.jpg" style="width:18px;"></td><td><a data-reqid="${item.reqid}" onclick="objRowtrackable.DeleteRequest(event)">Delete Request</a></td><td><a data-reqid="${item.reqid}" onclick="objRowtrackable.RequestDetails(event)">Request Details</a></td></tr>`;
                rowcontainer.appendChild(tr);
            }
        });

    }
    GetDataPaging() {
        var _this = this;
        //reset the row counter
        //ajax call
        return new Promise((resolve, reject) => {
            var data = JSON.stringify({ paramss: this, searchQuery: buildQuery });
            var call = new DataAjaxCall(app.url.server.RunQuery, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "", "")
            $(app.SpinningWheel.Main).show();
            call.Send().then((data) => {
                app.data = data;
                HandsOnTableViews.buildHandsonTable(data);
                $(app.SpinningWheel.Main).hide();
                //select the first column and start in the first available cell
                hot.selectCell(0, _this.StartInCell());
            })
        });
    }
    GetSelectedRowskey() {
        var RowKeys = [];
        rowselected.sort(function (a, b) { return a - b })
        for (var i = 0; i < rowselected.length; i++) {
            var rowkey = hot.getDataAtRow(rowselected[i])[0]
            RowKeys.push({ rowKeys: rowkey, })
        }
        app.globalverb.GrowKey = RowKeys;
        return RowKeys
    }
    CheckEditRowConditions(change) {

        var passValidation = true;
        var column = app.data.ListOfHeaders[change.cell];
        var valid = new ValidationFields(change.Achange, column);
        valid.Validator();
        if (valid.errIsAllowEmpty == true) {
            showAjaxReturnMessage(`<p style="color:blue;font-weight: bold">Column: ${column.HeaderName}</p>
                                       <p>can't be empty, we will revert the change back</p>
                                       <p style="color:gray;font-weight: bold">To: ${change.Bchange}</span>`, "w");
            passValidation = false;
        } else {
            if (valid.errSmallIntMaxValue == true) {
                showAjaxReturnMessage(`<p style="color:blue;font-weight: bold">Column: ${column.HeaderName}</p>
                                           <p style="color:black">Max value cannot be greater than 32,767 we will revert the change back</p>
                                           <p style="color:gray;font-weight: bold">To: ${change.Bchange}</p>`, "w");
                passValidation = false;
            }
            if (valid.errBigIntMaxValue == true) {
                showAjaxReturnMessage(`<p style="color:blue;font-weight: bold">Column: ${column.HeaderName}</p>
                                           <p style="color:black">Max value cannot be greater than 2,147,483,647 we will revert the change back</p>
                                           <p style="color:gray;font-weight: bold"> To: ${change.Bchange}</p>`, "w");
                passValidation = false;
            }
            if (valid.errStringMaxLength == true) {
                showAjaxReturnMessage(`<p style="color:red;font-weight: bold">Warning</p>
                                               <p style="color:white">Maximum Length for the selected Column/Field is ${column.MaxLength}</p>
                                               <p style="color:white">Changes made on the Record has been reverted back</p>
                                               <p style="color:white;font-weight: bold">To: ${change.Bchange}</p>`, "w");
                passValidation = false;
            }
            if (valid.errDatePattern == true) {
                showAjaxReturnMessage(`<p style="color:blue;font-weight: bold">Column: ${column.HeaderName}</p>
                                               <p style="color:red">The date format is not correct. The correct format is ${app.data.dateFormat}</p>,
                                               <p style="color:black">we will revert the change back</p>
                                               <p style="color:gray;font-weight: bold">To: ${change.Bchange}</p>`, "w");
                passValidation = false;
            }
            if (valid.errDouble == true) {
                //add to the messages
            }
        }


        return passValidation
    }
    GetPrimaryKeyColumn() {
        const primaryKeyIndex = app.data.ListOfHeaders.findIndex(eachColumnHeader => eachColumnHeader.IsPrimarykey);
        return primaryKeyIndex;
    }
    BuildOneRowBeforeSave(currentrow, currentcol) {
        //reset the prperties from afterchange hook
        this._BuildRowFromCell = obJgridfunc.BuildRowFromCells;
        this.CurrentRow = this.LastRowSelected
        return new Promise((resolve, reject) => {
            if (obJgridfunc.LastRowSelected !== null && obJgridfunc.LastRowSelected !== currentrow) {
                if (this._BuildRowFromCell.length > 0) {
                    this.pkey = hot.getDataAtRow(obJgridfunc.LastRowSelected)[0];
                    //build row parentID
                    //if (app.data.fvList.length > 0) {
                    //    var dt = app.data.fvList[0];
                    //    this._BuildRowFromCell.push({ value: dt.value, columnName: dt.Field, DataTypeFullName: dt.DataType })
                    //}
                    //build row pkey
                    this._BuildRowFromCell.push({ value: this.pkey, columnName: "", DataTypeFullName: "" })
                    //hold the previous row and column
                    app.globalverb.GCurrentRowHolder = { previous_row: obJgridfunc.LastRowSelected, previous_col: currentcol };
                    //check if confirm required
                    if (localStorage.getItem("rowautosave") === "false") {
                        obJgridfunc.LastRowSelected = currentrow;
                        this.ConfirmEditRow();
                        resolve(false);
                    } else {
                        //fire save method
                        app.globalverb.Isnewrow = false;
                        this.SaveRowAfterBuild().then(() => { resolve(true) });
                    }

                } else {
                    obJgridfunc.LastRowSelected = currentrow;
                    this.CurrentRow = obJgridfunc.LastRowSelected;
                }
            } else {
                obJgridfunc.LastRowSelected = currentrow;
                this.CurrentRow = obJgridfunc.LastRowSelected;
            }
        })
    }
    ModifyObject(data) {
        var arrObj = data.Rowdata.map(one => { one.value = one.value.toString(); return one })
        return { "Rowdata": arrObj, "paramss": data.paramss };
    }
    SaveRowAfterBuild() {
        var _this = this;
        var params = {};
        params.ViewId = _this.ViewId;
        params.crumbLevel = app.data.crumbLevel;
        params.BeforeChange = _this.BeforeChange.map((elem) => { return elem.value }).join("\r\n") + "\r\n";
        params.AfterChange = _this.AfterChange.map((elem) => { return elem.value }).join("\r\n") + "\r\n";
        params.scriptDone = obJlinkscript.ScriptDone;
        params.childkeyfield = obJgridfunc.childkeyfield

        var data = { Rowdata: this._BuildRowFromCell, paramss: params };
        var ModifyData = obJgridfunc.ModifyObject(data);
        data = JSON.stringify(ModifyData);

        $(app.SpinningWheel.Main).show();
        var call = new DataAjaxCall(app.url.server.SetDatabaseChanges, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        return new Promise(resolve => {
            call.Send().then((data) => {
                if (data.isError) {
                    showAjaxReturnMessage(data.Msg, "e");
                    if (data.Msg === app.language["msgDestroyedRowsReadOnly"] || data.Msg === app.language["msgNoPermission2Edit"]) {
                        _this.DiscardChangesOnEditRow();
                    }

                    $(app.SpinningWheel.Main).hide();
                    obJgridfunc.BeforeChange = [];
                    obJgridfunc.AfterChange = [];
                    obJgridfunc._BuildRowFromCell = [];
                    app.globalverb.Isnewrow = false;
                    resolve(false);
                    return;
                }
                if (data.scriptReturn.isBeforeAddLinkScript && obJlinkscript.ScriptDone == false) {
                    obJlinkscript.isBeforeAdd = data.scriptReturn.isBeforeAddLinkScript
                    obJlinkscript.id = data.scriptReturn.ScriptName;
                    $(app.SpinningWheel.Grid).hide();
                    $("#BasicDialog").hide()
                    //run linkscript
                    obJlinkscript.LinkscriptEventCall(obJlinkscript);
                } else if (data.scriptReturn.isBeforeEditLinkScript && obJlinkscript.ScriptDone == false) {
                    obJlinkscript.isBeforeEdit = data.scriptReturn.isBeforeEditLinkScript;
                    obJlinkscript.id = data.scriptReturn.ScriptName;
                    $(app.SpinningWheel.Grid).hide();
                    //run linkscript
                    obJlinkscript.LinkscriptEventCall(obJlinkscript);
                } else {
                    _this.AfterSaveRow(data)
                    obJgridfunc._BuildRowFromCell = [];
                    obJgridfunc.BeforeChange = [];
                    obJgridfunc.AfterChange = [];
                    resolve(true);
                    $(app.SpinningWheel.Main).hide();
                }
            });
        })
    }
    ConfirmEditRow() {
        var title = "";
        var isnew = false;
        if (obJgridfunc.RowKeyid === null) {
            title = "These are the following changes that has been made on the record, Do you wish to continue?"
            isnew = true;
        } else {
            // title = "are you sure you wanna edit this row?"
            title = "These are the following changes that has been made on the record, Do you wish to continue?"
        }
        var dlg = new DialogBoxBasic("<label>" + title + "</label>", app.url.Html.DialogMsgConfirmSaveNewrow, app.domMap.Dialogboxbasic.Type.PartialView)
        dlg.ShowDialog().then(() => {
            if (isnew === true) {
                $("#listItem").append(`<li>After Change: ${this.AfterChange.map((elem) => { return elem.value }).join(" | ")}</li>`)
            } else {
                $("#listItem").append(`<li><span style="color:blue;"> Before Change: </span> ${this.BeforeChange.map((elem) => { return elem.value }).join(" | ")}</li><li><span style="color:blue;">After Change: </span>${this.AfterChange.map((elem) => { return elem.value }).join(" | ")}</li>`)
            }
        });
    }
    DiscardChangesOnEditRow() {
        $("#BasicDialog").hide();
        obJgridfunc.BuildRowFromCells = [];
        this.BeforeChange = [];
        this.AfterChange = [];

        obJgridfunc.LoadViewTogrid("", pagingfun.SelectedIndex).then(() => {
            hot.selectCell(app.globalverb.GCurrentRowHolder.previous_row, app.globalverb.GCurrentRowHolder.previous_col)
        })

    }
    AfterSaveRow(data) {
        //check for error
        if (this.ErrorHandler(data)) { return }
        //compare the two keys after return from the server (concept for if user change the pkey in the field)
        //if (this.pkey !== data.keyvalue) {
        //    for (var i = 0; i < app.data.ListOfHeaders.length; i++) {
        //        var isempty = hot.getDataAtRow(this.CurrentRow)[i];
        //        if (app.data.ListOfHeaders[i].IsPrimarykey)
        //            hot.setDataAtCell(this.CurrentRow, i, data.keyvalue);
        //    }
        //}
        //var rownum = this.CurrentRow;
        //showAjaxReturnMessage('Row - ' + this.CurrentRow + ' Id: ' + this.pkey + ' saved successfuly!', 's')
        if (app.globalverb.Isnewrow === true) {
            showAjaxReturnMessage('A New record created successfully', 's')
            obJaddnewrecord.data = data;
        } else {
            //showAjaxReturnMessage('Update successfully', 's')
            showAjaxReturnMessage('Record has been updated successfully', 's')
            if (data.gridDatabinding.ListOfDatarows.length > 0) {
                for (var i = 0; i < data.gridDatabinding.ListOfDatarows[0].length; i++) {
                    if (i >= obJgridfunc.StartInCell()) {
                        var value = data.gridDatabinding.ListOfDatarows[0][i];
                        var db = app.data.ListOfHeaders[i];
                        if (db.DataType !== 'DateTime' && db.Allownull === true) {
                            hot.setDataAtCell(app.globalverb.GCurrentRowHolder.previous_row, i, value, "");
                        }
                    }
                }
            }
        }
        obJlinkscript.AfterLinkScript(data);

    }
    ErrorHandler(data) {
        this.isError = data.isError;
        if (data.isError) {
            switch (data.errorNumber) {
                case app.Enums.Error.DuplicatedId:
                    hot.selectCell(this.CurrentRow, this.GetPrimaryKeyColumn())
                    hot.setDataAtCell(this.CurrentRow, this.GetPrimaryKeyColumn(), "");
                    showAjaxReturnMessage(data.Msg, "e");
                    break;
                case app.Enums.Error.ConversionFailed:
                    hot.selectCell(this.CurrentRow, this.cell)
                    hot.setDataAtCell(this.CurrentRow, this.cell, "");
                    showAjaxReturnMessage(data.Msg, "e");
                    break;
                default:
                    hot.selectCell(this.CurrentRow, this.cell);
                    showAjaxReturnMessage(data.Msg, "e");
            }
        };
        return this.isError;
    }
    StartInCell() {
        var cell = 0; //never be zero
        if (app.data.HasAttachmentcolumn && app.data.HasDrillDowncolumn) {
            cell = 3;
        } else if (app.data.HasAttachmentcolumn === false && app.data.HasDrillDowncolumn === false) {
            cell = 1;
        } else if (app.data.HasAttachmentcolumn === true && app.data.HasDrillDowncolumn === false) {
            cell = 2;
        } else if (app.data.HasAttachmentcolumn === false && app.data.HasDrillDowncolumn === true) {
            cell = 2
        }
        return cell;
    }
    CalcFirstThreeColsWidth(data) {
        if (data.HasAttachmentcolumn && app.data.HasDrillDowncolumn) {
            return [0, 10, 17];
        } else if (data.HasAttachmentcolumn === false && app.data.HasDrillDowncolumn === false) {
            return []
        } else if (data.HasAttachmentcolumn === true && app.data.HasDrillDowncolumn === false) {
            return [0, 5];
        } else if (data.HasAttachmentcolumn === false && app.data.HasDrillDowncolumn === true) {
            return [0, 5];
        }
    }
    CheckForRowDuplication(rownum) {
        var arrposition = rowselected.indexOf(rownum);
        return arrposition != -1 ? true : false;
    }
    RefreshGrid(elem) {
        var _this = this;
        this.isRefresh = true;
        if (obJbreadcrumb.crumbLevel == 0) {
            app.DomLocation.Current.click()
        } else {
            var querybuilder = obJlastquery.CheckIfLastQuery(this.ViewId);
            var data = JSON.stringify({ paramss: this, searchQuery: querybuilder });
            $(app.SpinningWheel.Main).show();
            var call = new DataAjaxCall(app.url.server.RunQuery, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
            call.Send().then((data) => {
                obJgridfunc.LoadGridpartialView(data).then(() => {
                    _this.isRefresh = false;
                });
                $(app.SpinningWheel.Main).hide();
            });
        }

    }
}

class AddNewRecord {
    constructor() {
        this.data = "";
    }
    LoadNewRowDialog() {
        if (!this.IsThereEditbleFields()) {
            showAjaxReturnMessage(app.language["msgNotEditable"], "w")
            return;
        }
        DestroyPickDays()
        var addrow;
        var rowHTML = "";
        var data = JSON.stringify({ paramss: app.data.ListOfHeaders })
        var call = new DataAjaxCall(app.url.server.LoadNewRecordForm, ajax.Type.Post, ajax.DataType.Html, data, ajax.ContentType.Utf8, "", "", "")
        call.Send().then((htmlpage) => {
            var dlg = new DialogBoxBasic("Add New Row", htmlpage, app.domMap.Dialogboxbasic.Type.Content)
            dlg.ShowDialog().then(() => {
                var bd = document.getElementById("addFields");
                for (var i = 0; i < app.data.ListofEditableHeader.length; i++) {
                    var prop = app.data.ListofEditableHeader[i];
                    //if (prop.isEditable == "True") {
                    addrow = "<tr>";
                    addrow += this.BuildNewRowDialogFields(prop);
                    addrow += "</tr>";
                    rowHTML += addrow;
                    //}
                }
                bd.innerHTML = rowHTML;
                GetPickaDayCalendar(app.data.dateFormat)
            });
        })

    }
    IsThereEditbleFields() {
        for (var i = 0; i < app.data.ListofEditableHeader.length; i++) {
            if (app.data.ListofEditableHeader[0].isEditable) {
                return true;
            }
        }
        return false;
    }
    BuildNewRowDialogFields(prop) {
        var model = JSON.stringify(prop)
        var dv = app.data.ListOfdropdownColumns.find(x => x.colorder == prop.columnOrder);
        //first TD
        var row = `<td>${prop.HeaderName}</td>`;
        //second TD
        if (prop.isDropdown) {
            row += `<td data-columnMeta='${model}'>`
            row += `<select onchange="obJaddnewrecord.OnInputChange(event)" class="form-control"><option></option>`

            var display = dv.display.split(",");
            var values = dv.value.split(",");
            for (var i = 0; i < display.length; i++) {
                row += `<option value="${values[i]}">${display[i]}</option>`
            }
            row += `</select></td>`
        } else {
            switch (prop.DataType) {
                case "String":
                    if (prop.MaxLength > 1000000) {//create text area
                        row += `<td data-columnMeta='${model}'">
                        <textarea onmouseleave="obJaddnewrecord.OnInputFiled(event)" onkeyup="obJaddnewrecord.OnInputFiled(event)"
                        class="form-control" placeholder="Max Length: ${prop.MaxLength}" maxlength="${prop.MaxLength}"></textarea></td>`
                    } else {
                        row += `<td data-columnMeta='${model}'">
                        <input type="text" value="${prop.DefaultRetentionId}" onmouseleave="obJaddnewrecord.OnInputFiled(event)" onkeyup="obJaddnewrecord.OnInputFiled(event)"
                        class="form-control" placeholder="Max Length: ${prop.MaxLength}" maxlength="${prop.MaxLength}"/></td>`
                    }
                    break;
                case "Int32":
                    row += `<td data-columnMeta='${model}'>
                        <input type="number" onmouseleave="obJaddnewrecord.OnInputFiled(event)" onkeyup="obJaddnewrecord.OnInputFiled(event)"
                       class="form-control" placeholder="Max Value: 2,147,483,647"/></td>`
                    break;
                case "Int16":
                    row += `<td data-columnMeta='${model}'>
                        <input type="number" onmouseleave="obJaddnewrecord.OnInputFiled(event)" onkeyup="obJaddnewrecord.OnInputFiled(event)"
                       class="form-control" placeholder="Max Value: 32,767"/></td>`
                    break;
                case "Double":
                    row += `<td data-columnMeta='${model}'>
                        <input type="number" onmouseleave="obJaddnewrecord.OnInputFiled(event)" onkeyup="obJaddnewrecord.OnInputFiled(event)"
                       class="form-control" /></td>`
                    break;
                case "Boolean":
                    row += `<td data-columnMeta='${model}'><input type="checkbox" /></td>`
                    break;
                case "DateTime":
                    row += `<td data-columnMeta='${model}'>
                           <input onmouseleave="obJaddnewrecord.OnInputFiled(event)" onkeyup="obJaddnewrecord.OnInputFiled(event)" autocomplete="off" placeholder="${app.data.dateFormat}" name="tabdatepicker" class="form-control" /></td>`
                    break;
                default:
            }

        }

        //third TD

        if (prop.Allownull || prop.DataType == "Boolean" || prop.isCounterField) {
            row += `<td><span style="color:red"></span></td>`
        } else {
            row += `<td><span style="color:red">*</span></td>`
        }

        return row;
    }
    OnInputFiled(event) {
        var data = JSON.parse(event.target.parentElement.dataset.columnmeta);
        if (data.Allownull == true || data.isCounterField == true) return;
        if (event.target.value == "") {
            event.target.parentElement.nextElementSibling.innerHTML = "*"
            event.target.parentElement.nextElementSibling.style.color = "red"
        } else {
            event.target.parentElement.nextElementSibling.innerHTML = ""
        }
    }
    OnInputChange(event) {
        var data = JSON.parse(event.target.parentElement.dataset.columnmeta);
        if (data.Allownull == true || data.isCounterField == true) return;
        if (event.target.value == "") {
            event.target.parentElement.nextElementSibling.innerHTML = "*"
            event.target.parentElement.nextElementSibling.style.color = "red"
        } else {
            event.target.parentElement.nextElementSibling.innerHTML = ""
        }
    }
    SaveNewRecord() {
        var _this = this;
        var fields = document.getElementById("addFields")
        app.globalverb.Isnewrow = true;
        //check pkey first
        if (this.CheckIfPriamryKey(fields)) {
            this.CheckPrimaryKeyFieldDuplication().then((isduplicate) => {
                if (isduplicate) {
                    showAjaxReturnMessage(`<p>${this.TempcolumnHeader} is a primary key field</p> <p>The value ${this.Tempvalue} is duplicated, Please, change the primary key value</p>`, "e")
                    return;
                } else {
                    if (this.GatherProperties(fields) === true) {
                        obJgridfunc.SaveRowAfterBuild().then(() => {
                            _this.AfterSaveNewRecord();
                        });
                    } else {
                        showAjaxReturnMessage(this.msgs, "w")
                    }
                }
            });
        } else {
            if (this.GatherProperties(fields) === true) {
                obJgridfunc.SaveRowAfterBuild().then(() => {
                    _this.AfterSaveNewRecord();
                });
            } else {
                showAjaxReturnMessage(this.msgs, "w")
            }
        }
    }
    GatherProperties(fields) {
        //check the rest of the field
        obJgridfunc._BuildRowFromCell = [];
        obJgridfunc.BeforeChange = [];
        obJgridfunc.AfterChange = [];

        var passValidation = true;
        this.msgs = '<h4 style="color:red">Please make changes to following Fields</h4>';
        for (var i = 0; i < fields.children.length; i++) {
            var value = fields.children[i].children[1].children[0].value;
            var data = JSON.parse(fields.children[i].children[1].dataset.columnmeta);
            //call validation class
            var valid = new ValidationFields(value, data);
            valid.Validator();
            if (valid.errIsAllowEmpty == true) {
                this.msgs += `<p style="color:black">${data.HeaderName}: - Required!</p>`;
                passValidation = false;
            }
            if (valid.errSmallIntMaxValue == true) {
                this.msgs += `<p style="color:black">${data.HeaderName}: Max value cannot be greater than 32,767`
                passValidation = false;
            }
            if (valid.errBigIntMaxValue == true) {
                this.msgs += `<p style="color:white">${data.HeaderName}: Max value for the field cannot be greater than 2,147,483,647`
                passValidation = false;
            }
            if (valid.errStringMaxLength == true) {
                this.msgs += `<p style="color:black">${data.HeaderName}: Max length is - ${data.MaxLength}`
                passValidation = false;
            }
            if (valid.errDatePattern == true) {
                this.msgs += `<p style="color:black">${data.HeaderName}: formar is incorrect the correct format is: ${app.data.dateFormat}`
                passValidation = false;
            }
            if (valid.errDouble == true) {
                //add to the messages
            }
            //check boolean
            if (data.DataTypeFullName == "System.Boolean") 
                value = fields.children[i].children[1].children[0].checked;

            obJgridfunc._BuildRowFromCell.push({ value: value, columnName: data.ColumnName, DataTypeFullName: data.DataTypeFullName })

            if (((data.DataTypeFullName == "System.Boolean") && (value == true)) || ((data.DataTypeFullName !== "System.Boolean") && (value.length > 0)))
                obJgridfunc.AfterChange.push({ columnName: data.HeaderName, value: data.HeaderName + ": " + value })
        }
        if (obJbreadcrumb.crumbLevel > 0) {
            var parent = buildAddNewParent.find(a => a.crumblevel == obJbreadcrumb.crumbLevel);
            obJgridfunc._BuildRowFromCell.push({ value: parent.value, columnName: parent.columnName, DataTypeFullName: parent.DataTypeFullName })
            //obJgridfunc._BuildRowFromCell.push(buildAddNewParent[buildAddNewParent.length - 1]);
        }
        //must have the last row as empty to indicate this row is new (written for the server level)
        obJgridfunc._BuildRowFromCell.push({ value: "", columnName: "", DataTypeFullName: "" })
        return passValidation;
    }
    CheckIfPriamryKey(fields) {
        var hasPkey = false;
        for (var i = 0; i < fields.children.length; i++) {
            var data = JSON.parse(fields.children[i].children[1].dataset.columnmeta);
            var pk = app.data.ListOfHeaders.find(pk => pk.ColumnName == data.ColumnName);
            if (pk.IsPrimarykey) {
                this.TempcolumnName = pk.ColumnName;
                this.TempcolumnHeader = fields.children[i].children[0].innerText;
                this.Tempvalue = fields.children[i].children[1].children[0].value;
                hasPkey = true;
                break;
            }
        }
        return hasPkey;
    }
    CheckPrimaryKeyFieldDuplication() {
        var objc = {};
        objc.Tablename = app.data.TableName;
        objc.PrimaryKeyname = this.TempcolumnName;
        objc.KeyValue = this.Tempvalue;
        var data = JSON.stringify({ paramss: objc });
        return new Promise((resolve, reject) => {
            var call = new DataAjaxCall(app.url.server.CheckForduplicateId, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
            call.Send().then((data) => {
                if (data.isError) {
                    showAjaxReturnMessage(data.Msg, "e");
                    $(app.SpinningWheel.Main).hide();
                    reject();
                }
                resolve(data.isDuplicated);
            });
        });
    }
    AfterSaveNewRecord() {
        //check if table is empty
        if (app.data.ListOfDatarows.length === 0) {
            this.InsertIntoEmptygrid();
        } else {
            this.InsertIntoGrid();
        }

        $("#BasicDialog").hide()
        hot.selectCell(0, obJgridfunc.StartInCell())
        obJgridfunc._BuildRowFromCell = [];
        app.globalverb.Isnewrow = false;

    }
    InsertIntoEmptygrid() {
        //app.data.ListOfDatarows.push(this.data.ListOfDatarows[0]);
        //app.data.ListOfHeaders.push(this.data.ListOfHeaders);
        //app.data.HasDrillDowncolumn = this.data.HasDrillDowncolumn;
        app.data = this.data.gridDatabinding;
        document.getElementById("ToolBar").innerHTML = this.data.gridDatabinding.ToolBarHtml
        HandsOnTableViews.RunhandsOnTable(app.data, "newrecord");
        //show paging after new row return(on empty grid);
        var paging = document.getElementById("paging-div");
        paging.style.display = "block";
        //document.getElementById("inputGotoPageNum").value = 1;
        //document.getElementById("spanTotalRecords").style.display = ""
        //document.getElementById("spanTotalRecords").innerHTML = 1;
        //document.getElementById("spanTotalPage").innerHTML = 1;
        //document.getElementById("divPerPageRecord").children[0].innerHTML = pagingfun.PerPageRecords;
        //$("#spinTotal").hide();
        obJaddnewrecord.GetTotalRowsViews(1);
        CheckrowsAutosave("gridload");
        if (app.data.ShowTrackableTable) {
            document.getElementById("TrackingStatusDiv").style.display = "block"
        }
        obJlastquery.CheckIfLastQuery(obJgridfunc.ViewId);
        //obJgridfunc.LoadViewTogrid(obJgridfunc.ViewId, 1)
    }
    InsertIntoGrid(Values) {
        hot.alter('insert_row', 0, 1);
        for (var i = 0; i < this.data.gridDatabinding.ListOfDatarows[0].length; i++) {
            var value = this.data.gridDatabinding.ListOfDatarows[0][i]
            hot.setDataAtCell(0, i, value, "newrecord");
        }
        var total = document.getElementById("spanTotalRecords");
        pagingfun.TotalRecords = pagingfun.TotalRecords + 1
        total.innerHTML = pagingfun.TotalRecords.toLocaleString();
    }
    GetTotalRowsViews(pageNumber) {
        obJgridfunc.pageNum = 1;
        obJgridfunc.crumbLevel = obJbreadcrumb.crumbLevel;
        app.data.crumbLevel = obJbreadcrumb.crumbLevel
        var data = JSON.stringify({ paramsUI: obJgridfunc, searchQuery: buildQuery })
        var call = new DataAjaxCall(app.url.server.GetTotalrowsForGrid, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((total) => {
            pagingfun.TotalRecords = parseInt(total.split("|")[0])
            pagingfun.PageSize = parseInt(total.split("|")[1])
            pagingfun.PerPageRecords = parseInt(total.split("|")[2])
            pagingfun.SetTextFieldValues()
            pagingfun.VisibleIndexs()
            $("#spanTotalRecords").show();
            $("#spinTotal").hide();
        });
    }
}

class LeftMenu {
    constructor() {

    }
    ResetMenu() {
        var gohome = document.getElementsByClassName(app.domMap.Layout.MenuNavigation.GoHome);
        var goup = document.getElementsByClassName(app.domMap.Layout.MenuNavigation.goUp);

        for (var i = goup.length - 1; i >= 0; i--) {
            goup[i].click();
        }

        for (var i = gohome.length - 1; i >= 0; i--) {
            gohome[i].click();
        }
    }
    ReopenLastmenu() {
        // if (obJbreadcrumb.crumbLevel > 0) return;
        this.ResetMenu();
        if (app.DomLocation.Current === '' || app.DomLocation.Current.dataset.location == 7) {
            return;
        }

        if (app.DomLocation.Current.dataset.location == 1 || app.DomLocation.Current.dataset.location == 2) {
            app.DomLocation.Current.parentElement.parentElement.parentElement.parentElement.parentElement.firstChild.click()
        } else {
            app.DomLocation.Current.parentElement.parentElement.parentElement.firstChild.click();
        }
        obJgridfunc.MarkSelctedMenu(app.DomLocation.Current);
    }
}

class Rowtrackable {
    constructor() {
    }
    DeleteRequest(event) {
        FETCHGET(`${app.url.server.DeleteTrackingRequest}?id=${event.target.dataset.reqid}`).then(data => {
            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e")
            } else if (data.isDeleteAllow == false) {
                showAjaxReturnMessage("you don't have permission to delete requests.", "w")
            } else {
                obJgridfunc.GetRowTrackTableData()
                dlgClose.CloseDialog();
            }
        });
    }
    RequestDetails(event) {
        var reqid = event.target.dataset.reqid
        FETCHGET(`${app.url.server.GetRequestDetails}?id=${reqid}&tableName=${app.data.TableName}`, "html").then(data => {
            var d = new DialogBoxBasic("Request Details", data, app.domMap.Dialogboxbasic.Type.Content)
            d.ShowDialog().then(() => { });
        });
    }
    UpdateRequestDetails(requestid) {
        var objc = {}
        objc.RequestID = requestid;
        objc.Fulfilled = document.getElementById("cbFulfilled").checked;
        objc.isException = document.getElementById("cbException").checked;
        objc.txtComment = document.getElementById("txtComment").value;
        if (objc.isException) {
            if (objc.txtComment == "") {
                showAjaxReturnMessage("Comment required!", "w")
                return;
            }
        }

        FETCHPOST(app.url.server.UpdateRequest, objc).then((data) => {
            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e")
                return;
            }
            obJgridfunc.GetRowTrackTableData()
            $("#BasicDialog").hide();

        });
    }
}


//moti
//DRILLDOWN FUNCTION
class DrilldownClick {
    constructor() {
    }
    Run(elem, childtablename, childKeyfield, childviewid, childusername, index, Parentviewid, viewname, ChildKeyType, calltype = "") {
        //keep the previous viewid and viewname after the call to server end in line 576
        app.DomLocation.CurrnetCrumb = elem;
        var preViewid = obJgridfunc.ViewId;
        var preViewname = obJgridfunc.ViewName
        obJgridfunc.ViewId = childviewid;
        obJgridfunc.ViewName = viewname;
        //var prevQuery = Array.from(buildQuery);
        if (obJbreadcrumb.crumbLevel == 0) {
            buildQueryParents = [];
            buildAddNewParent = [];
        }
        obJbreadcrumb.crumbLevel = parseInt(bc.children[bc.children.length - 1].dataset.CrumbOrder) + 1;
        //prepare params to send to the API RunQuery
        var objc = {};
        buildQuery = [];
        objc.ViewId = childviewid;
        objc.pageNum = 1;
        objc.preTableName = app.data.TableName;
        objc.Childid = hot.getDataAtCell(rowselected, 0);
        objc.ViewType = app.Enums.ViewType.FusionView;
        objc.crumbLevel = obJbreadcrumb.crumbLevel;
        objc.childKeyfield = childKeyfield;
        obJgridfunc.childkeyfield = childKeyfield;
        //reset the query button to white
        obJlastquery.isQueryApplied = false;

        //build array parent for add new row
        this.BuildAddnewParent(childKeyfield, ChildKeyType);
        //build search array parent for later use/ keep the parentrowid for later refresh.

        var data = JSON.stringify({ paramss: objc, searchQuery: this.BuildSearchParent(childKeyfield, ChildKeyType) })
        var call = new DataAjaxCall(app.url.server.RunQuery, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((data) => {
            obJgridfunc.LoadGridpartialView(data).then(() => {
                obJbreadcrumb.RightClickList = data.ListOfBreadCrumbsRightClick;
                //build crumbs
                obJgridfunc.ViewType = 0;
                bc.children[bc.children.length - 1].remove()
                obJbreadcrumb.CreateHtmlCrumbLink(hot.getDataAtCell(rowselected, 0), childKeyfield, ChildKeyType, preViewid, preViewname);
                obJbreadcrumb.CreateCrumbHeader(viewname, "");
            });
        });
    }
    BuildSearchParent(childKeyfield, ChildKeyType) {
        var Parentreturn = [];
        if (obJbreadcrumb.crumbLevel == 1) {
            buildQueryParents = [];
            buildQueryParents.push({ parent: [{ columnName: childKeyfield, ColumnType: ChildKeyType, operators: "=", values: obJgridfunc.GetSelectedRowskey()[0].rowKeys, islevelDownProp: true }], crumblevel: 1 })
            Parentreturn = buildQueryParents.find(a => a.crumblevel == 1).parent
        } else {
            var index = buildQueryParents.findIndex(a => a.crumblevel == obJbreadcrumb.crumbLevel)
            if (index === -1) {
                buildQueryParents.push({ parent: [{ columnName: childKeyfield, ColumnType: ChildKeyType, operators: "=", values: obJgridfunc.GetSelectedRowskey()[0].rowKeys, islevelDownProp: true }], crumblevel: obJbreadcrumb.crumbLevel })
                Parentreturn = buildQueryParents.find(a => a.crumblevel == obJbreadcrumb.crumbLevel).parent;
            } else {
                buildQueryParents[index].parent = [{ columnName: childKeyfield, ColumnType: ChildKeyType, operators: "=", values: obJgridfunc.GetSelectedRowskey()[0].rowKeys, islevelDownProp: true }]
                Parentreturn = buildQueryParents[index].parent
                //queryreturn = buildQueryParents.find(a => a.crumblevel == obJbreadcrumb.crumbLevel && a.values == obJgridfunc.GetSelectedRowskey()[0].rowKeys).parent;
            }
        }
        return Parentreturn;
    }
    BuildAddnewParent(childKeyfield, ChildKeyType) {
        if (obJbreadcrumb.crumbLevel == 1) {
            buildAddNewParent = [];
            buildAddNewParent.push({ value: obJgridfunc.GetSelectedRowskey()[0].rowKeys, columnName: childKeyfield, DataTypeFullName: ChildKeyType, crumblevel: obJbreadcrumb.crumbLevel })
        } else {
            //var index = buildAddNewParent.findIndex(a => a.value == obJgridfunc.GetSelectedRowskey()[0].rowKeys && a.columnName == childKeyfield && a.DataTypeFullName == ChildKeyType)
            var index = buildAddNewParent.findIndex(a => a.crumblevel == obJbreadcrumb.crumbLevel);
            if (index === -1) {
                buildAddNewParent.push({ value: obJgridfunc.GetSelectedRowskey()[0].rowKeys, columnName: childKeyfield, DataTypeFullName: ChildKeyType, crumblevel: obJbreadcrumb.crumbLevel })
            } else {
                buildAddNewParent[index].value = obJgridfunc.GetSelectedRowskey()[0].rowKeys;
            }
        }
    }
}
//moti
//BREAD CRUMBS FUNCTIONS
class BreadCrumb {
    constructor() {
        this.BreadCrumbsList = [];
        this.RightClickList = [];
        this.brcounter = -1;
        this.rowid = 0;
        this.childkeyField = "";
        this.columnType = "";
        this.CrumbOrder = 0;
        this.previousChildid = "";
        this.crumbLevel = 0;
        this.crumbLocation = { Current: "", Previous: "", };
        this.isClickoncrumbs = false;
        this.viewname = "";
    }
    CreateFirstCrumb(data) {
        //this.rowid = "";
        //this.childkeyField = "";
        if (bc.children.length === 0 || obJmyquery.isFromMyquery) {
            bc.innerHTML += "<li style='font-weight:bold;cursor: pointer' ' data-toggle='tooltip' data-original-title='" + data.ViewName + "'>" + data.ViewName + "</li>"
            $('[data-toggle="tooltip"]').tooltip()
            bc.children[0].dataset.CrumbOrder = 0;
            obJbreadcrumb.crumbLevel = 0;
            obJmyquery.isFromMyquery = false;
        }
        //bc.children[0].dataset.lastQuery = JSON.stringify(buildQuery)
        //bc.innerHTML += "<li style='font-weight:bold' data-toggle='tooltip' data-original-title='" + data.ViewName + "'>" + data.ViewName + "</li>"
        //$('[data-toggle="tooltip"]').tooltip()
    }
    ClickOnFirstBreadCrumbs() {
        app.DomLocation.Current.click()
    }
    CreateCrumbHeader(viewName, calltype) {
        var li = document.createElement("li");
        var span = document.createElement("span");
        li.setAttribute("data-toggle", "tooltip");
        li.dataset.CrumbOrder = bc.children.length;
        span.style.fontWeight = "bold";
        li.append(span);
        bc.append(li);

        switch (calltype) {
            case app.Enums.Crumbs.AuditHistoryPerRow:
                li.setAttribute("data-original-title", "Audit-Report");
                span.innerText = "Audit-Report";
                break;
            case app.Enums.Crumbs.TrackingHistoryPerRow:
                li.setAttribute("data-original-title", "Tracking History");
                span.innerText = "Tracking History";
                break;
            case app.Enums.Crumbs.ContentsPerRow:
                li.setAttribute("data-original-title", "Contents");
                span.innerText = "Contents";
                break;
            case app.Enums.Crumbs.Vault:
                li.setAttribute("data-original-title", "Attch from vault");
                span.innerText = "Vault";
                break;
            default:
                li.setAttribute("data-original-title", viewName);
                li.oncontextmenu = obJbreadcrumb.RightClickBuilder;
                this.viewname = viewName;
                span.innerText = viewName;
        }
        $('[data-toggle="tooltip"]').tooltip();
    }
    CreateHtmlCrumbLink(rowid, childkeyfeild, columType, preViewid, preViewname) {
        //create child fields to use later
        this.rowid = rowid;
        this.childkeyField = childkeyfeild;
        this.columnType = columType;

        var li = document.createElement("li");
        var a = document.createElement("a");
        var span = document.createElement("span");
        var span1 = document.createElement("span");
        a.setAttribute("href", "#");
        a.setAttribute("data-toggle", "tooltip");

        //a.setAttribute("data-original-title", "(" + obJgridfunc.ViewName + ") - row - " + rowid);
        a.setAttribute("data-original-title", app.data.ItemDescription)
        a.innerHTML = "(" + preViewname + ")";
        span.className = "fa fa-level-down";
        span.style.marginLeft = "2px";
        span.style.fontSize = "10px";
        a.innerHTML += " - " + app.data.ItemDescription;
        a.append(span);
        li.append(a);
        span1.className = "fa fa-angle-right";
        span1.style.color = "#3f3f3f";
        span1.style.fontWeight = "bold"
        span1.style.fontSize = "13px";
        span1.style.marginLeft = "3px"
        li.append(span1);
        li.setAttribute("onclick", "obJbreadcrumb.ClickOnCrumb(this)")
        li.dataset.childkeyField = childkeyfeild
        li.dataset.rowid = rowid;
        li.dataset.keyfieldValue = rowid;
        li.dataset.columType = columType;
        li.dataset.viewId = preViewid;
        li.dataset.CrumbOrder = bc.children.length;
        li.dataset.location = 4;
        li.dataset.viewname = preViewname;
        this.crumbLocation.Current = li;
        //li.dataset.lastQuery = JSON.stringify(prevQuery);
        //obJbreadcrumb.crumbLevel = bc.children.length + 1;
        //li.dataset.lastQuery = JSON.stringify(buildQuery);
        //buildQuery = [];
        bc.append(li);
    }
    ClickOnCrumb(event) {
        var _this = this;
        var _event = event;
        this.firstCall = true;
        var liOrder = parseInt(event.dataset.CrumbOrder);
        var crumbChilds = event.parentElement.children.length - 1;
        for (var i = 0; i < crumbChilds; i++) {
            if (i >= liOrder) {
                event.nextElementSibling.remove();
            }
        }
        //obJdrildownclick = new DrilldownClick();
        obJbreadcrumb = new BreadCrumb();

        obJbreadcrumb.crumbLevel = parseInt(event.dataset.CrumbOrder);
        this.crumbLevel = parseInt(event.dataset.CrumbOrder);
        app.data.crumbLevel = parseInt(event.dataset.CrumbOrder);
        //build array for saving row later. 
        //var selectCurrentParent =

        //buildQuery = [];
        //buildQuery = JSON.parse(event.dataset.lastQuery)
        this.ViewId = event.dataset.viewId;
        this.pageNum = 1;
        //check if view exist in the list when you go back to parent;
        //if (obJbreadcrumb.crumbLevel == 0) { 
        //    app.DomLocation.Current.dataset.location = obJgridfunc.ViewType;
        //}

        obJlastquery.RemoveCrumbsHistory(lqb)
        obJgridfunc.ViewName = _event.dataset.viewname;
        obJgridfunc.ViewId = this.ViewId;
        //if (obJbreadcrumb.crumbLevel == 0) { obJquerywindow.isCallfromCrumb = false} 
       
        //if (obJlastquery.isQueryApplied == false && obJbreadcrumb.crumbLevel == 0) {
        if (obJbreadcrumb.crumbLevel == 0) {
            obJbreadcrumb.isClickoncrumbs = true;
            //obJleftmenu.ReopenLastmenu();
            app.DomLocation.Current.click();
            return;
        }

        var querybuilder = obJlastquery.CheckIfLastQuery(this.ViewId)
        this.childkeyField = _event.previousElementSibling.dataset.childkeyField
        var data = JSON.stringify({ paramss: this, searchQuery: querybuilder });
        $(app.SpinningWheel.Main).show();
        var call = new DataAjaxCall(app.url.server.RunQuery, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((data) => {
            obJgridfunc.LoadGridpartialView(data).then(() => {
                obJbreadcrumb.RightClickList = data.ListOfBreadCrumbsRightClick;
                //remove link and create header
                if (parseInt(_event.dataset.CrumbOrder) === 0) {
                    _event.remove();
                    obJbreadcrumb.CreateFirstCrumb(data);
                } else {
                    obJbreadcrumb.rowid = _event.previousElementSibling.dataset.rowid;
                    obJgridfunc.childkeyfield = _event.previousElementSibling.dataset.childkeyField;
                    _event.remove();
                    obJbreadcrumb.CreateCrumbHeader(data.ViewName);
                }
            });
            $(app.SpinningWheel.Main).hide();
        });
    }
    RightClickBuilder(ev) {
        ev.preventDefault();
        //dynamic position the popup right click list
        var Calcli = 0;
        for (var j = 0; j < bc.children.length; j++) {
            if (bc.children.length !== j + 1) {
                Calcli += bc.children[j].offsetWidth;
            }
        }
        var posY = ev.offsetY + 4;
        var posX = Calcli
        //anytime remove the context popup and build it again 
        $(app.BreadCrumb.CrumbContextmenu).remove();
        //build right click list
        ev.target.innerHTML += "<ul id='CrumbContextmenu' class='context-menu-list context-menu-root' style='width: 206px; top: " + posY + "; left: " + posX + "; z-index: 1999;'>"
        for (var i = 0; i < obJbreadcrumb.RightClickList.length; i++) {
            $(app.BreadCrumb.CrumbContextmenu).append("<li class='context-menu-item' data-viewname='" + obJbreadcrumb.RightClickList[i].viewName + "' onclick='obJbreadcrumb.ClickOnrightClickView(this," + obJbreadcrumb.RightClickList[i].viewId + ")' style='cursor: pointer; color:#2f2f2f'>" + obJbreadcrumb.RightClickList[i].viewName + "</li></ul>");
        }

        //bread crumns events
        $("body").on("mouseover", app.BreadCrumb.CrumbContextmenu + " li", (ev) => {
            var context = document.querySelector(app.BreadCrumb.CrumbContextmenu)
            for (var i = 0; i < context.children.length; i++) {
                context.children[i].style.backgroundColor = "white";
            }
            ev.target.style.backgroundColor = "#2980b9"
            //ev.currentTarge.style.background = "#2980b9";
        });

        $("body").on("mouseleave", app.BreadCrumb.CrumbContextmenu, (ev) => {
            $(app.BreadCrumb.CrumbContextmenu).remove();
        });

        return false;
    }
    ClickOnrightClickView(elem, viewid) {
        elem.parentElement.parentElement.firstChild.nodeValue = elem.textContent
        var model = {};
        obJlastquery.isQueryApplied = false;
        model.ViewId = viewid;
        model.pageNum = 1;
        model.ViewType = 0;
        model.crumbLevel = obJbreadcrumb.crumbLevel;
        model.ChildKeyField = obJgridfunc.childkeyfield;
        //var p = buildQuery.find(level => level.islevelDownProp === true)
        //buildQuery = [{ columnName: p.columnName, ColumnType: p.ColumnType, operators: p.operators, values: p.values, islevelDownProp: true }];
        var crumbLevel = obJbreadcrumb.crumbLevel
        obJgridfunc.ViewName = elem.dataset.viewname;
        var querybuilder = obJlastquery.CheckIfLastQuery(viewid);
        //querybuilder = buildQueryParents.find(a => a.crumblevel == crumbLevel).parent;
        var data = JSON.stringify({ paramss: model, searchQuery: querybuilder })
        var call = new DataAjaxCall(app.url.server.RunQuery, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((data) => {
            obJgridfunc.LoadGridpartialView(data).then(() => {

            });
        });
    }
}
//QUERY WINDOW FUNCTIONS
class QueryWindow {
    constructor() {
        this.SaveBasicQuerySchema = "";
        this.isQueryStoped = false;
        this.isCallfromCrumb = "";
    }
    LoadQueryWindow(isCallfromquery) {
        if (obJbreadcrumb.crumbLevel > 0 && isCallfromquery) {
            this.isCallfromCrumb = true;
        } else {
            this.isCallfromCrumb = false;
        }
        
        DestroyPickDays()
        var _this = this;

        //check if glocal search call before, if yes setup the type to fusion view call
        if (obJgridfunc.ViewType === app.Enums.ViewType.GlobalSearch) {
            obJgridfunc.ViewType = app.Enums.ViewType.FusionView;
        }
        //check if clicked on myquery if yes load the query from the myquery object
        if (app.DomLocation.Current.dataset.viewid == "query" && obJbreadcrumb.crumbLevel == 0) {
            obJmyquery.LoadSaveQuery(app.DomLocation.Current, obJmyquery.LoadSaveQueryParams, true);
            return;
        }

        //check if is breadcrumb query
        //if breadcrumbs
        //if (obJbreadcrumb.crumbLevel > 0) {
        //    buildQuery = [];
        //}

        obJmyquery.updatemode = false;
        var title = $(app.domMap.DialogQuery.QuerylblTitle);
        $(app.domMap.DialogQuery.QuerySaveInput).hide();
        title.show();
        title.html(obJgridfunc.ViewTitle);
        var start = performance.now();
     
        return new Promise((resolve, reject) => {
            var data = { viewId: obJgridfunc.ViewId, ceriteriaId: 0, crumblevel: obJbreadcrumb.crumbLevel, childkeyfield: obJgridfunc.childkeyfield };
            var call = new DataAjaxCall(app.url.server.LoadQueryWindow, ajax.Type.Get, ajax.DataType.Html, data, "", "", "", "");
            call.Send().then((data) => {
                var end = performance.now();
                console.log(end - start);
                $(app.domMap.DialogQuery.QueryContentDialog).html(data)
                obJquerywindow.SaveBasicQuerySchema = data;
                _this.ApplyLastQuery();

                _this.ShowPopupWindow();

                GetPickaDayCalendar(app.data.dateFormat);
                resolve(true);
            })
        })
    }
    ApplyLastQuery() {
        var buildLastQuery = []
        //var dom = app.DomLocation.Current.dataset;
        //if (dom.location == 1) obJgridfunc.ViewName = dom.viewname;
        var querybuilder = obJlastquery.CheckIfLastQuery(obJgridfunc.ViewId);
        if (obJbreadcrumb.crumbLevel > 0) {
            //if (buildQuery.length == 0) return;
            //buildQuery.pop();
            for (var i = 0; i < querybuilder.length; i++) {
                if (!querybuilder[i].islevelDownProp) {
                    buildLastQuery.push(querybuilder[i])
                }
            }
        } else {
            buildLastQuery = querybuilder;
        }

        var table = document.querySelector(app.domMap.DialogQuery.Querytableid)

        if (obJlastquery.CheckifViewChange(table, buildLastQuery)) return;
        for (var i = 0; i < buildLastQuery.length; i++) {
            var OperCount = table.tBodies[0].children[i].children[1].children[0].childElementCount;
            if (OperCount === 12)
                obJmyquery.BindOperatorsWithBetween(buildLastQuery[i].operators, table, i, buildLastQuery[i].values)
            else
                obJmyquery.BindOperators(buildLastQuery[i].operators, table, i, buildLastQuery[i].values);
        }
    }
    ShowPopupWindow() {
        $(app.domMap.DialogQuery.MainDialogQuery).show();
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
        var waitForcheckbox = setInterval(() => {
            var check = document.querySelector(app.domMap.DialogQuery.ChekBasicQuery);
            if (check !== null) {
                this.StartQuery = getCookie('startQuery');
                if (this.StartQuery == "True") {
                    check.checked = true;
                } else {
                    check.checked = false;
                }
                //create my query events
                obJmyquery.CreateMyQueryEvents();
                //obJquerywindow.SaveBasicQuerySchema = document.getElementById("modelBody").innerHTML;
                // reset position after query window close
                $("#MainDialogQuery .ui-draggable").css({ "top": "0px", "left": "0px" })
                clearInterval(waitForcheckbox)
            }
        })
    }
    QueryOkButton() {
        this.CheckLevelDownprop();
        this.SubmmitQuery();
        if (this.isQueryStoped) {
            this.isQueryStoped = false;
            return;
        } else {
            $(app.domMap.DialogQuery.MainDialogQuery).hide();
        }
    }
    QueryApplybutton() {
        this.CheckLevelDownprop()
        this.SubmmitQuery();
    }
    CheckLevelDownprop() {
        var index = buildQuery.findIndex(eachone => eachone.islevelDownProp);
        if (index === -1) {
            // buildQuery = [];
        } else {
            var getleveldownProp = buildQuery[index];
            //buildQuery = [];
            buildQuery.push(getleveldownProp);
        }

    }
    SubmmitQuery(isSaveQueryCall = false) {
        //send operators and values to the server.
        //get value and operators from table
        var _this = this;
        obJgridfunc.pageNum = 1;
        obJgridfunc.isPagingClick = false;
        var querybuilder = []
        var table = document.querySelector(app.domMap.DialogQuery.Querytableid);
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
            querybuilder.push({ columnName: columnName, ColumnType: ColumnType, operators: operators, values: values, islevelDownProp: false });
        }
        //check for breadcrumbs history. if you are on level 1 delte all history on level > 1.
        obJlastquery.RemoveCrumbsHistory(lqb);
        //check condition before apply the search
        if (_this.CheckQueryConditions() > 0) {
            _this.isQueryStoped = true;
            return;
        } else {
            _this.isQueryStoped = false;
        }


        if (isSaveQueryCall) {
            return querybuilder;
        }
        if (obJreportsrecord.isCustomeReportCall === true) {
            //obJreportsrecord.GenerateCustomReport();
        } else {
            //reset save query attribute to one
            var BtnSvn = document.querySelector(app.domMap.DialogQuery.BtnSaveQuery);
            if (BtnSvn !== null) {
                BtnSvn.attributes[1].value = 0;
            }

            //concept for if you query from global search.//we have to improve for subquery from gloval search.  
            //if (app.DomLocation.Previous !== "") {
          
            // app.DomLocation.Previous = "";
            //}
            obJlastquery.SetLastQuery(querybuilder);
            if (obJbreadcrumb.crumbLevel == 0 || obJmyquery.isFromMyquery) bc.innerHTML = "";

            obJgridfunc.LoadViewTogrid(0, 1)
        }
    }
    CheckQueryConditions() {
        var _this = this;
        var errCounter = 0;
        var table = document.querySelector(app.domMap.DialogQuery.Querytableid)
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
                    if (!checkDateFormat(dateField.value, app.data.dateFormat)) {
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

        if (dataType === "System.DateTime" && !checkDateFormat(startInput.value, app.data.dateFormat)) {
            startInput.style.border = "1px solid red";
            counter++
        } else {
            if (dataType === "System.DateTime") {
                startInput.style.border = "";
            }
        }

        if (dataType === "System.DateTime" && !checkDateFormat(endInput.value, app.data.dateFormat)) {
            endInput.style.border = "1px solid red";
            counter++
        } else {
            if (dataType === "System.DateTime") {
                endInput.style.border = "";
            }

        }
        return counter;
    }

    CloseQuery() {
        if (app.DomLocation.Previous === '') {
            $("#MainDialogQuery").hide();
            return;
        }
        if (app.data.crumbLevel !== "integer") {
            obJbreadcrumb.crumbLevel = app.data.crumbLevel;
        }

        app.DomLocation.Current = app.DomLocation.Previous;
        // we want to be able to avoid refresh when crumb level greater than 0. May need new navbar library.Moti.Reggie 19/7/22
        //if (obJbreadcrumb.crumbLevel == 0) {
            obJleftmenu.ReopenLastmenu();
        //}
        //setup the call from crumb level in case you want to insert query to localstorage
        //in this case inside the CheckIfLastQuery() the location will be 4 which is crumbs location.
        if (obJbreadcrumb.crumbLevel > 0) {
            obJquerywindow.isCallfromCrumb = true;
        } else {
            obJquerywindow.isCallfromCrumb = false;
        }

        obJgridfunc.ViewId = app.data.ViewId;
        obJgridfunc.ViewTitle = app.data.ViewName;
        obJgridfunc.ViewName = app.data.ViewName;
        obJgridfunc.ViewType = app.Enums.ViewType.FusionView;

        $("#MainDialogQuery").hide();
    }
    ClearInputs() {
        DestroyPickDays();
        $(app.domMap.DialogQuery.QueryContentDialog).html(obJquerywindow.SaveBasicQuerySchema)
        GetPickaDayCalendar(app.data.dateFormat);

        var check = document.querySelector(app.domMap.DialogQuery.ChekBasicQuery);
        if (check !== null) {
            this.StartQuery = getCookie('startQuery');
            if (this.StartQuery == "True") {
                check.checked = true;
            } else {
                check.checked = false;
            }
        }

        if (app.DomLocation.Current.dataset.viewid == "query" && obJmyquery.isFromMyquery == true) {
            var btnUpdate = document.querySelector(app.domMap.DialogQuery.BtnSaveQuery)
            //update the button title to update
            if (btnUpdate !== null)
                btnUpdate.innerHTML = app.language["UpdateQuery"];
            obJmyquery.isFromMyquery = false;
        }
        //document.getElementById("modelBody").innerHTML = obJquerywindow.SaveQuerySchema;
    }

    OperatorCondition(elem) {
        var dataType = elem.parentElement.parentElement.firstElementChild.getAttribute("datatype");
        var isDropdown = elem.parentElement.parentElement.firstElementChild.getAttribute("dropdown");
        switch (dataType) {
            case "System.String":
                break;
            case "System.Int32":
                if (isDropdown == "True") return;
                this.OperatorNumericSetup(elem);
                break;
            case "System.Double":
                if (isDropdown == "True") return;
                this.OperatorNumericSetup(elem);
                break;
            case "System.Boolean":
                break;
            case "System.DateTime":
                this.OperatorDateSetup(elem);
                break;
            default:
        }
        var oprator = elem.parentElement.parentElement.children[1].children[0].value
        if (oprator == ' ') {
            elem.parentElement.parentElement.children[2].children[0].value = ' '
        }
    }
    ChechkBetweenValidation(elem) {
        var stid = "#numberFiled_start";
        var eid = "#numberFiled_end";
        var numberFiled_start = $('#numberFiled_start').val();
        var numberFiled_end = $('#numberFiled_end').val();
        var addCssRed = { "margin-top": "5px", "width": "49%", "border": "1px solid red" }
        var RemoveCssRed = { "margin-top": "5px", "width": "49%", "border": "1px solid #ccc" }

        if (numberFiled_start !== "" && numberFiled_end !== "") {
            $(stid).css(RemoveCssRed);
            $(eid).css(RemoveCssRed);
        } else {
            if (numberFiled_start === "" && numberFiled_end === "") {
                $(stid).css(RemoveCssRed);
                $(eid).css(RemoveCssRed);
            }
            else
                if (numberFiled_start !== "" && numberFiled_end === "") {
                    $(stid).css(RemoveCssRed);
                    $(eid).css(addCssRed);
                }
                else if (numberFiled_start === "" && numberFiled_end !== "") {
                    $(stid).css(addCssRed);
                    $(eid).css(RemoveCssRed);
                }
        }
    }
    OperatorNumericSetup(elem) {
        var OprValue = elem.options.item(elem.selectedIndex).value;
        if (OprValue == "Between") {
            elem.parentElement.nextElementSibling.innerHTML = `<input id="numberFiled_start" type="number" style="width:49%;margin-top:5px" placeholder="Start" type="text" class="form-control formWindowTextBox">
                                                               <input id="numberFiled_end" type="number" style="display:block;width:49%;margin-left:3px;margin-top:5px" placeholder="End" class="form-control formWindowTextBox">`;
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
            //elem.parentElement.nextElementSibling.innerHTML = `<input id="dateFiled_start" style="width:49%;" placeholder="Start" type="text" onfocus="obJquerywindow.OperatorDateFocuse(this)" onblur="obJquerywindow.OperatorDateBlur(this)" class="form-control formWindowTextBox">
            //                                                   <input id="dateFiled_end" style="display:block;width:49%;margin-left:3px;margin-top:5px" placeholder="End" onfocus="obJquerywindow.OperatorDateFocuse(this)" onblur="obJquerywindow.OperatorDateBlur(this)" class="form-control formWindowTextBox">`;

            elem.parentElement.nextElementSibling.innerHTML = `<input id="dateFiled_start" autocomplete="off" name="tabdatepicker" style="width:49%;" placeholder="Start" class="form-control formWindowTextBox">
                                                               <input id="dateFiled_end" autocomplete="off" name="tabdatepicker" style="display:block;width:49%;margin-left:3px;margin-top:0px" placeholder="End" class="form-control formWindowTextBox">`;
        } else {
            elem.parentElement.nextElementSibling.innerHTML = `<input id="dateFiled" autocomplete="off" placeholder="${app.data.dateFormat}" name="tabdatepicker" class="form-control">`;
        }

        GetPickaDayCalendar(app.data.dateFormat)
    }
    OperatorDateFocuse(elem) {
        // ClearPickDaysElements();
        // GetPickaDayCalendar(app.data.dateFormat)
        //elem.type = 'date';
        //elem.style.marginTop = "5px";
    }
    OperatorDateBlur(elem) {
        //ClearPickDaysElements();
        //GetPickaDayCalendar(app.data.dateFormat)
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

//LASTQUERY 
var lqb = localStorage.getItem("lastquerybind");
class LastQuery {
    constructor() {
        this.isQueryApplied = false;
    }
    SetLastQuery(querybuilder) {
        //console.log(`${obJbreadcrumb.crumbLevel} setlastquery call`)
        //if integer means it is the first query
        //if (app.data.ViewId !== "integer") {
        if (app.DomLocation.Current.dataset === undefined) return;
        if (app.DomLocation.Current.dataset.location == app.Enums.location.MyQueryView && obJbreadcrumb.crumbLevel === 0) {
            this.isQueryApplied = false;
            obJmyquery.myQueryholder = querybuilder
            return;
        }
        var domdata = app.DomLocation.Current.dataset;


        var str = localStorage.getItem("lastquerybind");
        if (str === null) {
            var createArr = [{ data: "", viewid: "", query: "", location: "", level: 0 }]
            localStorage.setItem("lastquerybind", JSON.stringify(createArr));
            str = JSON.parse(localStorage.getItem("lastquerybind"));
        } else {
            str = JSON.parse(localStorage.getItem("lastquerybind"));
        }
        var domlocation = obJquerywindow.isCallfromCrumb ? 4 : domdata.location;

        var index = str.findIndex(v => v.viewid == obJgridfunc.ViewId && v.location == domlocation && v.viewname == obJgridfunc.ViewName);
        if (index === -1) {
            if (obJbreadcrumb.crumbLevel > 0) {
                str.push({ data: "", viewid: obJgridfunc.ViewId, query: querybuilder, location: domlocation, viewname: obJgridfunc.ViewName, level: obJbreadcrumb.crumbLevel })
            } else {
                str.push({ data: "", viewid: obJgridfunc.ViewId, query: querybuilder, location: domlocation, viewname: obJgridfunc.ViewName, level: 0 })
            }
            localStorage.setItem("lastquerybind", JSON.stringify(str));
        } else {
            str[index].data = "";
            str[index].viewid = obJgridfunc.ViewId;
            str[index].query = querybuilder;
            str[index].location = domlocation;
            str[index].viewname = obJgridfunc.ViewName;
            if (obJbreadcrumb.crumbLevel > 0) {
                str[index].level = obJbreadcrumb.crumbLevel;
            } else {
                str[index].level = 0;
            }

            if (querybuilder.length !== 0) {
                this.isQueryApplied = true;
                // document.querySelector(app.domMap.ToolBarButtons.BtnQuery).style.backgroundColor = "#F0F8FF";
            }

            localStorage.setItem("lastquerybind", JSON.stringify(str));
        }

    }
    CheckIfLastQuery(viewid) {
        if (app.DomLocation.Current.dataset === undefined) return;
        var domdata = app.DomLocation.Current.dataset;
        if (domdata.location == app.Enums.location.MyQueryView && obJbreadcrumb.crumbLevel === 0) {
            this.isQueryApplied = false;
            return obJmyquery.myQueryholder;
        }

        lqb = localStorage.getItem("lastquerybind");
        if (lqb === null) {
            var createArr = [{ data: "", viewid: "", query: "", location: "", level: 0 }]
            localStorage.setItem("lastquerybind", JSON.stringify(createArr));
            lqb = JSON.parse(localStorage.getItem("lastquerybind"));
        } else {
            lqb = JSON.parse(localStorage.getItem("lastquerybind"));
        }
        var domlocation = obJquerywindow.isCallfromCrumb ? 4 : domdata.location;
        domlocation = obJbreadcrumb.crumbLevel == 0 ? domdata.location : domlocation;
        var index = lqb.findIndex(v => v.viewid == viewid && v.location == domlocation && v.viewname == obJgridfunc.ViewName);
        //var index = lqb.findIndex(v => v.viewid == viewid && v.location == domdata.location);
        var buildqurylast = []
        if (index === -1) {
            buildqurylast = [];
            this.isQueryApplied = false;
        } else if (this.IsEmptyQuery(viewid, domlocation)) {
            buildqurylast = [];
            this.isQueryApplied = false;
            lqb.splice(index, 1)
            localStorage.setItem("lastquerybind", JSON.stringify(lqb));
        } else {
            buildqurylast = lqb[index].query
            this.isQueryApplied = true;
            var qrybtn = document.querySelector(app.domMap.ToolBarButtons.BtnQuery);
            if (qrybtn !== null) {
                document.querySelector(app.domMap.ToolBarButtons.BtnQuery).style.backgroundColor = "#F0F8FF";
            }
        }
        //if crumb level more than 0 we have to setup the parent inside the query.
        if (obJbreadcrumb.crumbLevel > 0 && obJmyquery.isFromMyquery == false) {
            var addParent = buildQueryParents.find(a => a.crumblevel == obJbreadcrumb.crumbLevel).parent[0]
            //var level = obJbreadcrumb.crumbLevel - 1;
            //var addParent = buildQueryParents[level].parent[0];
            buildqurylast.push(addParent)
        } else {
            obJmyquery.isFromMyquery = false;
            //this.RemoveCrumbsHistory(lqb);
        }
        return buildqurylast;
    }
    IsEmptyQuery(viewid, domlocation) {
        //var domdata = app.DomLocation.Current.dataset;
        
        var list = lqb.find(v => v.viewid == viewid && v.location == domlocation && v.viewname == obJgridfunc.ViewName).query;
        for (var i = 0; i < list.length; i++) {
            if (list[i].operators.trim() !== "" && list[i].values.trim() !== "") {
                return false;
            }
        }
        return true;
    }
    RemoveCrumbsHistory() {
        var tempCrumb = obJbreadcrumb.crumbLevel;
        if (obJmyquery.isFromMyquery) tempCrumb = 0;
        var str = JSON.parse(localStorage.getItem("lastquerybind"));
        if (str === null) return;
        var indexes = [];
        for (var i = 0; i < str.length; i++) {
            if (str[i].level > tempCrumb) {
                indexes.push(i);
            }
        }
        for (var i = indexes.length - 1; i >= 0; i--) {
            var r = indexes[i]
            str.splice(r, 1);
        }

        localStorage.setItem("lastquerybind", JSON.stringify(str));
    }
    ResetQueries() {
        localStorage.removeItem("lastquerybind");
        location.reload();
    }
    DeleteOneSavedQuery(viewname, locationid) {
        if (lqb !== null) {
            var templqb = [];
            if (typeof lqb === 'object') {
                templqb = lqb;
            } else {
                var objc = JSON.parse(lqb)
                templqb = objc;
            }
            var index = templqb.findIndex(a => a.viewname == viewname && a.location == locationid)
            if (index !== -1) {
                templqb.splice(index, 1)
                localStorage.setItem("lastquerybind", JSON.stringify(templqb))
            }
        }
    }
    CheckifViewChange(table, lastQuery) {
        //check if view change and if it is changed delete the last query for this view from local storage "lastquerybind"]
        if (table.tBodies.length === 0) return false; //if view returns with no column don't check for changes;
        var isviewChanged = false;
        if (table.tBodies[0].childElementCount !== lastQuery.length) {
            isviewChanged = true;
        } else {
            for (var i = 0; i < lastQuery.length; i++) {
                var domcolname = table.tBodies[0].children[i].children[0].getAttribute("columnname").toLowerCase();
                var domdatatype = table.tBodies[0].children[i].children[0].getAttribute("datatype").toLowerCase();
                var lastcolname = lastQuery[i].columnName.toLowerCase()
                var lastdatatype = lastQuery[i].ColumnType.toLowerCase()
                if (domcolname != lastcolname || domdatatype != lastdatatype) {
                    isviewChanged = true;
                    break;
                }
            }
        }

        if (isviewChanged) {
            if (lqb !== null) {
                var templqb = [];
                if (typeof lqb === 'object') {
                    templqb = lqb;
                } else {
                    var objc = JSON.parse(lqb)
                    templqb = objc;
                }
                var index = templqb.findIndex(a => a.viewid == obJgridfunc.ViewId && a.location == app.DomLocation.Current.dataset.location && a.viewname == obJgridfunc.ViewName)  
                if (index !== -1) {
                    templqb.splice(index, 1)
                    localStorage.setItem("lastquerybind", JSON.stringify(templqb))
                }
            }
        }

        return isviewChanged;
    }
}

//FAVORITE FUNCTIONS
class Favorite {
    constructor() {
        this.FavCriteria = "";
        this.FavCriteriaType = "";
        this.ViewId = "";
        this.NewFavoriteName = "";
        this.pageNum = 0;
    }
    LoadFavoriteTogrid(elem, params, pageNum) {
        app.DomLocation.Previous = app.DomLocation.Current;
        app.DomLocation.Current = elem;

        bc.innerHTML = "";
        obJbreadcrumb.crumbLevel = 0;
        var splitparams = params.split("_");
        var _this = this;
        obJgridfunc.ViewId = splitparams[0];
        obJgridfunc.ViewType = app.Enums.ViewType.Favorite;
        obJgridfunc.ViewName = elem.innerText;
        obJlastquery.RemoveCrumbsHistory(lqb);
        var querybuilder = obJlastquery.CheckIfLastQuery(obJgridfunc.ViewId);

        _this.ViewId = splitparams[0];
        _this.FavCriteriaid = splitparams[1];
        _this.FavCriteriaType = splitparams[2];
        _this.pageNum = pageNum;

        var data = JSON.stringify({ paramss: _this, searchQuery: querybuilder });
        var call = new DataAjaxCall(app.url.server.ReturnFavoritTogrid, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((data) => {
            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e")
                return;
            }
            obJgridfunc.LoadGridpartialView(data).then(() => {
                obJgridfunc.ViewName = elem.innerText;
                //create delete button after fav button
                /*$(app.domMap.ToolBarButtons.DivFavOptions).after('<div style="display:inline-block;" data-toggle="tooltip" data-original-title="Delete My Favorite"><button id="btnDeleteCeriteria" style="height:34px;margin-left:3px" class="btn btn-secondary tab_btn" type="button"><i class="fa fa-trash-o"></i></button></div>')*/
                $(app.domMap.ToolBarButtons.BtnUpdateFavourite).after("<li><a id='removeFromFavorite'>Remove From Favorite</a></li>")
                obJbreadcrumb.CreateFirstCrumb(data)
            });
        })
        obJgridfunc.MarkSelctedMenu(elem);
    }
    DeleteCeriteria(elem, id, viewid) {
        var _this = this;
        //written for last query. remove the last query from local storage if this favorit has last query saved

        // app.DomLocation.Current.dataset.location
        var viewname = elem.parentElement.parentElement.firstChild.children[0].dataset.viewname
        obJlastquery.DeleteOneSavedQuery(viewname, app.Enums.location.FavoritView)

        app.DomLocation.Current = elem;
        var objc = {};
        objc.FavCriteriaid = id;
        objc.ViewId = viewid;
        var data = JSON.stringify({ paramss: objc });
        var call = new DataAjaxCall(app.url.server.DeleteFavorite, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((data) => {
            if (data.isError == true) {
                showAjaxReturnMessage(data.Msg, "e")
            } else {
                app.DomLocation.Current.parentElement.parentElement.remove();
                document.getElementById("MyfavClickMenu").click();
                if (id === parseInt(_this.FavCriteriaid)) {
                    window.location.reload();
                }
            }
        })
    }
    LoadNewFavorite() {
        //if (rowselected.length === 0) return showAjaxReturnMessage("Please, select at least one row (needs to add the language file)", "w");
        if (rowselected.length === 0) return showAjaxReturnMessage(app.language["msgFavoriteRecordSelection"], "w");
        dlg = new DialogBoxBasic("New Favorite", app.url.server.NewFavorite, app.domMap.Dialogboxbasic.Type.PartialView)
        dlg.ShowDialog();
    }
    AddNewFavorite(Favoritename) {
        var _this = this;
        //var getRowKey = [];
        this.ViewId = obJgridfunc.ViewId
        this.NewFavoriteName = Favoritename;
        var data = JSON.stringify({ 'paramss': this, 'recordkeys': obJgridfunc.GetSelectedRowskey() });
        var call = new DataAjaxCall(app.url.server.AddNewFavorite, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then(function (data) {
            if (data.isWarning) {
                showAjaxReturnMessage(data.Msg, "w")
                return;
            }

            if (data.isError === true) {
                showAjaxReturnMessage(data.Msg, "e")
                return;
            }

            var setparam = (_this.ViewId + "_" + data.SaveCriteriaId + "_" + 1).toString();

            //_this.AfterAddNewFavorite(setparam)

            //Replacing the dialog box with toaster Message
            _this.AfterAddNewFavoriteMsg(setparam)
        })
    }
    AfterAddNewFavoriteMsg(getparam) {
        var _this = this;
        showAjaxReturnMessage(app.language["msgMyFavoriteAdd"], "s")
        dlg.CloseDialog(true);

        var li = document.createElement("li");
        var div = document.createElement("div");
        div.className = "row";
        div.innerHTML = `<div class="col-md-10">
                                <a data-location="1" data-viewname="${_this.NewFavoriteName}" data-viewid="favorite" onclick="obJfavorite.LoadFavoriteTogrid(this,'${getparam}', 1)">${_this.NewFavoriteName} <b style='color:green'>New!</b></a>
                                </div><div class="col-md-2">
                                <i onclick="obJfavorite.DeleteCeriteria(this,${getparam.split("_")[1]}, ${obJgridfunc.ViewId})" style="margin-top:11px;margin-left: -6px;" class="fa fa-trash-o"></i>
                                </div>`
        li.appendChild(div)
        var menu = document.querySelector(app.domMap.Layout.MenuNavigation.MyFavClickMenu)
        menu.parentElement.children[1].appendChild(li);
        menu.click();

    }
    AfterAddNewFavorite(getparam) {
        var _this = this;
        dlg.CloseDialog(true);
        var d = new DialogBoxBasic(app.language["lblMSGSuccessMessage"], app.url.Html.DialogMsghtml, app.domMap.Dialogboxbasic.Type.PartialView);
        d.ShowDialog().then(function (solve) {
            if (solve) {

                document.querySelector(app.domMap.Dialogboxbasic.Dlg.DialogMsg.DialogMsgTxt).innerHTML = app.language["msgMyFavoriteAdd"]
                //var x = document.querySelector(app.domMap.MenuNavigation.LeftSideMenu)
                var li = document.createElement("li");
                var div = document.createElement("div");
                div.className = "row";
                // a.setAttribute('onclick', "obJfavorite.LoadFavoriteTogrid(this,'" + getparam + "')");
                div.innerHTML = `<div class="col-md-10">
                                <a data-location="1" data-viewid="favorite" onclick="obJfavorite.LoadFavoriteTogrid(this,'${getparam}', 1)">${_this.NewFavoriteName} <b style='color:green'>New!</b></a>
                                </div><div class="col-md-2">
                                <i onclick="obJfavorite.DeleteCeriteria(this,${getparam.split("_")[1]}, ${obJgridfunc.ViewId})" style="margin-top:11px;margin-left: -6px;" class="fa fa-trash-o"></i>
                                </div>`
                li.appendChild(div)
                //reset menu
                //obJleftmenu.ResetMenu();
                //click on favorite
                var menu = document.querySelector(app.domMap.Layout.MenuNavigation.MyFavClickMenu)
                menu.parentElement.children[1].appendChild(li);
                menu.click();
                //click on the new favorite
            }
        });
    }
    BeforeAddToFavorite() {
        var data = { viewid: obJgridfunc.ViewId }
        var call = new DataAjaxCall(app.url.server.CheckBeforeAddTofavorite, ajax.Type.Get, ajax.DataType.Json, data, "", "", "", "");
        call.Send().then((haslist) => {
            if (rowselected.length === 0) return showAjaxReturnMessage(app.language["msgJsDataSelectOneRowForFav"], "w")
            if (haslist) this.AddToFavoriteStartDialog()
            //else showAjaxReturnMessage("you have to create new favorite for this view first (needs to add the language file)", "w");
            else showAjaxReturnMessage(app.language["MSGCreateNewFavorite"], "w");
        });
    }
    AddToFavoriteStartDialog() {
        var dlg = new DialogBoxBasic("Add To Favorite", app.url.server.StartDialogAddToFavorite + "?viewid=" + obJgridfunc.ViewId, app.domMap.Dialogboxbasic.Type.PartialView);
        dlg.ShowDialog().then(() => {
        });
    }
    UpdateFavorite() {
        var ddl = document.querySelector(app.domMap.Favorite.AddToFavorite.DDLfavorite)
        var _this = this;
        //var getRowKey = [];
        this.ViewId = obJgridfunc.ViewId
        this.FavCriteriaid = ddl.item(ddl.options.selectedIndex).value;
        var data = JSON.stringify({ 'paramss': this, 'recordkeys': obJgridfunc.GetSelectedRowskey() });
        var call = new DataAjaxCall(app.url.server.UpdateFavorite, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then(function (data) {
            if (data.isError === false) { _this.AfterupdateFavorite() } else { showAjaxReturnMessage(data.msg, "e") }
        })
    }
    AfterupdateFavorite() {
        //var dlg = new DialogBoxBasic(app.language["lblMSGSuccessMessage"], app.url.Html.DialogMsghtml, app.domMap.Dialogboxbasic.Type.PartialView);
        //dlg.ShowDialog().then(() => {
        //    document.querySelector(app.domMap.Dialogboxbasic.Dlg.DialogMsg.DialogMsgTxt).innerHTML = app.language["msgMyFavoriteUpdate"];
        //});
        showAjaxReturnMessage(app.language["msgMyFavoriteUpdate"], "s")
        $(app.domMap.Dialogboxbasic.BasicDialog).hide();
    }
    DeleteFavoriteRecords() {
        this.ViewId = obJgridfunc.ViewId
        //call server
        var data = JSON.stringify({ 'paramss': this, 'recordkeys': obJgridfunc.GetSelectedRowskey() });
        var call = new DataAjaxCall(app.url.server.DeleteFavoriteRecord, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((data) => {
            if (data.isError === false) {
                app.DomLocation.Current.click();
                dlgClose.CloseDialog(false);

            }
        });
    }
}
//MY QUERY FUNCTIONS
class MyQuery {
    constructor() {
        this.updatemode = false;
        this.LoadSaveQueryParams = "";
        this.SavedCriteriaid = 0;
        this.myQueryholder = [];
        this.isFromMyquery = false;
        /* this.loadSaveverbs = {};*/
    }
    LoadSaveQuery(elem, stringParam, btnQuerycall = false) {
        //this.loadSaveverbs.elem = elem;
        //this.loadSaveverbs.stringParam = stringParam;
        //this.loadSaveverbs.btnQuerycall = false;

        this.isFromMyquery = true;
        DestroyPickDays()
        var _this = this;
        this.LoadSaveQueryParams = stringParam;
        app.DomLocation.Previous = app.DomLocation.Current;
        app.DomLocation.Current = elem;
        _this.SavedCriteriaid = parseInt(stringParam.split("_")[1]);
        obJgridfunc.MarkSelctedMenu(elem);
        obJgridfunc.ViewId = parseInt(stringParam.split("_")[0]);
        obJgridfunc.ViewType = app.Enums.ViewType.FusionView;
        obJgridfunc.ViewTitle = elem.innerHTML;


        if ((getCookie('startQuery') == "True" && !obJgridfunc.isRefresh && obJbreadcrumb.isClickoncrumbs == false) || btnQuerycall === true) {
            var data = { viewId: obJgridfunc.ViewId, ceriteriaId: _this.SavedCriteriaid, crumblevel: obJbreadcrumb.crumbLevel, childkeyfield: obJgridfunc.childkeyfield };
            var call = new DataAjaxCall(app.url.server.LoadQueryWindow, ajax.Type.Get, ajax.DataType.Html, data, "", "", "", "");
            call.Send().then((data) => {
                $(app.domMap.DialogQuery.QueryContentDialog).html(data)
                obJquerywindow.SaveBasicQuerySchema = data;
                _this.AfterLoadQuery();
                _this.ApplyMySavedquery();
                obJquerywindow.ShowPopupWindow();
                GetPickaDayCalendar(app.data.dateFormat);
                obJgridfunc.isRefresh = false;
                obJbreadcrumb.isClickoncrumbs = false;
            });
        } else {
            bc.innerHTML = "";
            obJgridfunc.isRefresh = false;
            obJbreadcrumb.isClickoncrumbs = false;
            _this.BingQueryDirectTodom(_this.SavedCriteriaid);
        }
    }
    AfterLoadQuery() {
        //var btnremove = document.querySelector(app.domMap.Myqury.BtnDeleteMyquery);
        var BtnSvn = document.querySelector(app.domMap.DialogQuery.BtnSaveQuery);
        var btnUpdate = document.querySelector(app.domMap.DialogQuery.BtnSaveQuery)
        var input = $(app.domMap.DialogQuery.QuerySaveInput);
        var label = $(app.domMap.DialogQuery.QuerylblTitle);
        //update the button title to update
        btnUpdate.innerHTML = app.language["UpdateQuery"];
        //set button attribute to 0 as it is not insert and we don't use this attr here.
        BtnSvn.attributes[1].value = 0;
        //hid the label text element
        label.hide();
        //show the input element and insert the title 
        //check if the title has new html element if yes remove it to show only view title in the input
        var ViewTitle = (obJgridfunc.ViewTitle.includes('<b style="color:green">New!</b>')) === true ? obJgridfunc.ViewTitle.split("<b")[0] : obJgridfunc.ViewTitle;
        input.show().val(ViewTitle);
        //set the query window to update mode to let the even't  
        this.updatemode = true;
    }
    BingQueryDirectTodom(SavedCriteriaid) {
        obJgridfunc.pageNum = 1;
        var querybuilder = [];
        var data = { viewId: obJgridfunc.ViewId, ceriteriaId: SavedCriteriaid, crumblevel: 0 };
        var call = new DataAjaxCall(app.url.server.LoadquerytoGrid, ajax.Type.Get, ajax.DataType.Json, data, "", "", "", "");
        call.Send().then((data) => {
            for (var i = 0; i < data.length; i++) {
                var prop = data[i]
                querybuilder.push({ columnName: prop.columnName, ColumnType: prop.ColumnType, operators: prop.operators, values: prop.values })
            }
            app.DomLocation.Current.dataset.location = app.Enums.location.MyQueryView;
            obJgridfunc.crumbLevel = 0;

            var data = JSON.stringify({ paramss: obJgridfunc, searchQuery: querybuilder });
            var call = new DataAjaxCall(app.url.server.RunQuery, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
            $(app.SpinningWheel.Main).show();
            call.Send().then((data) => {
                if (data.isError) {
                    showAjaxReturnMessage(data.Msg, "e");
                    $(app.SpinningWheel.Main).hide();
                    reject();
                    return;
                }
                obJlastquery.isQueryApplied = false;
                obJgridfunc.LoadGridpartialView(data).then(function () {
                    $(app.SpinningWheel.Main).hide();
                });
            });
            //obJgridfunc.LoadViewTogrid(0, 1).then((isgridLoaded) => { });
        })


    }
    ApplyMySavedquery() {
        var table = document.querySelector(app.domMap.DialogQuery.Querytableid)
        var list = app.ServerDataReturn.QueryData;
        for (var i = 0; i < list.length; i++) {
            var OperCount = table.tBodies[0].children[i].children[1].children[0].childElementCount;
            if (OperCount === 12)
                this.BindOperatorsWithBetween(list[i].operators, table, i, list[i].values)
            else
                this.BindOperators(list[i].operators, table, i, list[i].values);
        }
    }
    BindOperators(Operators, table, index, value) {
        switch (Operators) {
            case "=":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 1;
                if (table.tBodies[0].children[index].children[2].children[0].type == "checkbox") {
                    table.tBodies[0].children[index].children[2].children[0].checked = value == "false" ? false : true;
                } else {
                    table.tBodies[0].children[index].children[2].children[0].value = value;
                }
                break;
            case "<>":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 2;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case ">":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 3;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case ">=":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 4;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "<":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 5;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "<=":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 6;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "BEG":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 7;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "Ends with":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 8;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "Contains":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 9;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "Not contains":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 10;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            default: table.tBodies[0].children[index].children[1].children[0].selectedIndex = "";

        }
    }
    BindOperatorsWithBetween(Operators, table, index, value) {
        switch (Operators) {
            case "=":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 1;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "<>":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 2;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case ">":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 3;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case ">=":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 4;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "<":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 5;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "<=":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 6;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "Between":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 7;
                var elem = table.tBodies[0].children[index].children[1].children[0];
                if (table.tBodies[0].children[index].children[0].getAttribute("datatype") === "System.Int32") {
                    obJquerywindow.OperatorNumericSetup(elem);
                } else {
                    obJquerywindow.OperatorDateSetup(elem);
                }
                var fieldStart = table.tBodies[0].children[index].children[2].firstElementChild;
                var fieldEnd = table.tBodies[0].children[index].children[2].lastElementChild;
                var values = value.split("|");
                fieldStart.value = values[0];
                fieldEnd.value = values[1];
                break
            case "BEG":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 8;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "Ends with":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 9;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "Contains":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 10;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            case "Not contains":
                table.tBodies[0].children[index].children[1].children[0].selectedIndex = 11;
                table.tBodies[0].children[index].children[2].children[0].value = value;
                break;
            default: table.tBodies[0].children[index].children[1].children[0].selectedIndex = "";
        }
    }
    CreateMyQueryEvents() {
        var BtnSvn = document.querySelector(app.domMap.DialogQuery.BtnSaveQuery);
        if (BtnSvn !== null && BtnSvn !== undefined)
            BtnSvn.attributes[1].value = 0;

        $("body").on("click", app.domMap.DialogQuery.QuerylblTitle, function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            obJmyquery.BeforeSave();
        });
        $("body").on("click", app.domMap.DialogQuery.BtnSaveQuery, function (e) {
            e.stopImmediatePropagation();
            if (obJmyquery.updatemode) obJmyquery.UpdateQuery(); else obJmyquery.Savequery();
        });
    }
    Savequery() {
        var input = $(app.domMap.DialogQuery.QuerySaveInput);
        var label = $(app.domMap.DialogQuery.QuerylblTitle);
        var BtnSvn = document.querySelector(app.domMap.DialogQuery.BtnSaveQuery);
        if (input.val() === "") {
            //showAjaxReturnMessage("Enter favorite Name before saving! (needs to add the language file)", "w");
            showAjaxReturnMessage(app.language["msgQueryNametoSavedMyQueries"], "w");
            obJmyquery.BeforeSave();
            return
        }
        if (parseInt(BtnSvn.attributes[1].value) === 1) {
            this.SaveName = input.val();
            this.ViewId = obJgridfunc.ViewId;
            var data = JSON.stringify({ paramss: this, Querylist: obJquerywindow.SubmmitQuery(true) })
            var call = new DataAjaxCall(app.url.server.SaveNewQuery, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
            call.Send().then(function (data) {
                if (data.isError) {
                    showAjaxReturnMessage(data.msg, "e")
                    return;
                }

                if (data.isNameExist === true) {
                    showAjaxReturnMessage(input.val() + " " + app.language["msgMyQueryUniqueSavedName"], "w")
                } else {
                    //showAjaxReturnMessage("Saved query as (needs to add the language file)" + input.val(), "s")
                    showAjaxReturnMessage(input.val() + " " + app.language["msgMyQueryUniqueSavedNameSucessfully"], "s")
                    BtnSvn.attributes[1].value = 0;
                    input.hide();
                    label.show();
                    obJmyquery.AfterSaved(data, input.val())
                }
            });
        } else {
            //showAjaxReturnMessage("Enter Name before saving!(needs to add the language file)", "w");
            showAjaxReturnMessage(app.language["msgQueryNametoSavedMyQueries"], "w");
            obJmyquery.BeforeSave();
        }
    }
    BeforeSave() {
        var title = $(app.domMap.DialogQuery.QuerylblTitle);
        var BtnSvn = document.querySelector(app.domMap.DialogQuery.BtnSaveQuery);
        var input = $(app.domMap.DialogQuery.QuerySaveInput);
        var issaved = parseInt(BtnSvn.attributes[1].value);
        if (issaved === 0) {
            var val = title.text()
            title.hide();
            input.css("borderBottom", "1px solid gray");
            input.show();
            input.val(val);
            input.focus();
            BtnSvn.attributes[1].value = 1;
        } else {
            BtnSvn.attributes[1].value = 0;
        }
        this.title = input.val()
    }
    AfterSaved(data, savedname) {
        //var a = document.createElement("a");
        //a.setAttribute('onclick', "obJmyquery.LoadSaveQuery(this,'" + data.uiparam + "')");
        //a.innerHTML = savedname + "  <b style='color:green'>New!</b>";
        var li = document.createElement("li");
        var div = document.createElement("div");
        div.className = "row";
        div.innerHTML = `<div class="col-md-10">
                                <a data-location="2" data-viewid="query" onclick="obJmyquery.LoadSaveQuery(this, '${data.uiparam}')">${savedname} <b style='color:green'>New!</b></a>
                                </div><div class="col-md-2">
                                <i onclick="obJmyquery.DeleteQuery(this, ${data.uiparam.split("_")[1]})(this,)" style="margin-top:11px;margin-left: -6px;" class="fa fa-trash-o"></i>
                                </div>`
        li.appendChild(div)
        //reset menu
        //obJleftmenu.ResetMenu();
        //click on myquery
        var query = document.querySelector(app.domMap.Layout.MenuNavigation.MyQureyClickMenu);
        query.click();
        query.parentElement.children[1].appendChild(li)
    }
    DeleteQuery(elem, id) {
        this.SavedCriteriaid = id;
        var data = JSON.stringify({ 'paramss': this });
        var call = new DataAjaxCall(app.url.server.DeleteQuery, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((data) => {
            if (data.isError == true) {
                showAjaxReturnMessage(data.Msg, "e")
            } else {
                elem.parentElement.parentElement.remove()
                document.getElementById("MyQueryClickMenu").click();
            }

        });
    }
    UpdateQuery() {
        var input = $(app.domMap.DialogQuery.QuerySaveInput);
        this.SaveName = input.val();
        this.ViewId = obJgridfunc.ViewId;
        buildQuery = [];
        var data = JSON.stringify({ paramss: this, Querylist: obJquerywindow.SubmmitQuery(true) })
        var call = new DataAjaxCall(app.url.server.UpdateQuery, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
        call.Send().then(function (data) {
            //if (data.isNameExist === true) return showAjaxReturnMessage(input.val() + " " + app.language["msgMyQueryUniqueSavedName"], "w")
            app.DomLocation.Current.innerHTML = input.val();
            if (data.isError === true) {
                showAjaxReturnMessage(data.Msg, "e")
            } else {
                //showAjaxReturnMessage("Updated query to (needs to add the language file)" + input.val(), "s")
                showAjaxReturnMessage(input.val() + " " + app.language["msgMyQueryUpdatedSuccess"], "s")

            }
        });
    }
}

//LINKSCRIPT FUNCTIONS
class Linkscript {
    constructor() {
        this.InputControllersList = [];
        this.ScriptParams = {};
        this.isBeforeAdd = false;
        this.isAfterAdd = false;
        this.isBeforeEdit = false;
        this.isAfterEdit = false;
        this.isBeforeDelete = false;
        this.isAfterDelete = false;
        this.ScriptDone = false;
        this.id = "";
        this.keyvalue = "";
    }
    ClickButton(elem) {
        var _this = this;
        this.ScriptParams.WorkFlow = elem.id;
        this.ScriptParams.ViewId = obJgridfunc.ViewId;
        app.globalverb.GForceRefresh = obJlinkscript.isBeforeEdit;
        //concept to return rows id from handsontable
        //work around to get the rows id as handsontable randering the page and duplicate rows.
        if (rowselected.length === 0) {
            this.ScriptParams.Tableid = hot.getDataAtCell(0, 0);
            //this.ScriptParams.Rowids = hot.getDataAtCell(0, 0);

        } else {
            if (app.globalverb.Isnewrow) {
                this.ScriptParams.Tableid = elem.keyvalue;
                this.ScriptParams.Rowids = elem.keyvalue;
            } else {
                this.ScriptParams.Tableid = hot.getDataAtCell(obJgridfunc.LastRowSelected, 0);
                var listids = [];
                if (rowselected.length > 1) {
                    for (var i = 0; i < rowselected.length; i++) {
                        var tbids = hot.getDataAtCell(rowselected[i], 0);
                        listids.push(tbids);
                    }
                    this.ScriptParams.Rowids = listids;
                } else {
                    this.ScriptParams.Rowids = hot.getDataAtCell(obJgridfunc.LastRowSelected, 0);
                }
            }
        }
        $(app.SpinningWheel.Main).show();
        var call = new DataAjaxCall(app.url.server.LinkscriptButtonClick, ajax.Type.Post, ajax.DataType.Json, this.ScriptParams, "", "", "", "");
        call.Send().then((data) => {
            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e")
                setTimeout(() => {
                    window.location.reload();
                }, 2000)
                return;
            }

            LinkScriptObjective = data;
            if (data.Showprompt) {
                _this.OpenDialog(data);
            } else if (data.ErrorMsg !== "" && data.ErrorMsg !== null) {
                showAjaxReturnMessage(data.ErrorMsg, "w")
            } else if (data.GridRefresh) {
                obJgridfunc.RefreshGrid()
            }
            $(app.SpinningWheel.Main).hide();

        })
    }
    LinkscriptEventCall(elem) {
        var _this = this;
        this.ScriptParams.WorkFlow = elem.id;
        this.ScriptParams.ViewId = obJgridfunc.ViewId;
        app.globalverb.GForceRefresh = obJlinkscript.isBeforeEdit;
        //concept to return rows id from handsontable
        //work around to get the rows id as handsontable randering the page and duplicate rows.
        if (rowselected.length === 0) {
            this.ScriptParams.Tableid = hot.getDataAtCell(0, 0);
            //this.ScriptParams.Rowids = hot.getDataAtCell(0, 0);

        } else {
            if (app.globalverb.Isnewrow) {
                this.ScriptParams.Tableid = elem.keyvalue;
                this.ScriptParams.Rowids = elem.keyvalue;
            } else {
                this.ScriptParams.Tableid = hot.getDataAtCell(app.globalverb.GCurrentRowHolder.previous_row, 0);
                var listids = [];
                if (rowselected.length > 1) {
                    for (var i = 0; i < rowselected.length; i++) {
                        var tbids = hot.getDataAtCell(rowselected[i], 0);
                        listids.push(tbids);
                    }
                    this.ScriptParams.Rowids = listids;
                } else {
                    this.ScriptParams.Rowids = hot.getDataAtCell(app.globalverb.GCurrentRowHolder.previous_row, 0);
                }
            }
        }
        $(app.SpinningWheel.Main).show();
        var call = new DataAjaxCall(app.url.server.LinkscriptEvents, ajax.Type.Post, ajax.DataType.Json, this.ScriptParams, "", "", "", "");
        call.Send().then((data) => {
            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e")
                setTimeout(() => {
                    window.location.reload();
                }, 2000)
                return;
            }

            LinkScriptObjective = data;
            if (data.Showprompt) {
                _this.OpenDialog(data);
            } else if (data.ErrorMsg !== "" && data.ErrorMsg !== null) {
                showAjaxReturnMessage(data.ErrorMsg, "w")
            } else if (data.GridRefresh) {
                obJgridfunc.RefreshGrid()
            }
            $(app.SpinningWheel.Main).hide();

        })
    }
    OpenDialog(data) {
        document.getElementById("LSlblTitle").innerHTML = "";
        document.getElementById("LSlblHeading").innerHTML = "";
        document.getElementById("LStblControls").innerHTML = "";
        document.getElementById("LSdivButtons").innerHTML = "";
        $("#LinkScriptDialogBox").show();
        $('.modal-dialog').draggable({
            handle: ".modal-header",
            stop: function (event, ui) {
            }
        });
        this.SetupControllers(data);
    }
    SetupControllers(data) {
        var TitleLocation = document.getElementById("LSlblTitle");
        var HeadingLocation = document.getElementById("LSlblHeading");
        //create title and heading
        TitleLocation.innerHTML = data.Title;
        HeadingLocation.innerHTML = data.lblHeading;
        //create body;
        this.SetupBodyController(data);
        //create buttons in the footer
        this.SetupButtons(data);
    }
    SetupButtons(data) {
        var _this = this;
        var buttonLocation = document.getElementById("LSdivButtons");
        buttonLocation.innerHTML = "";
        for (var i = 0; i < data.ButtonsList.length; i++) {
            var btn = document.createElement("button");
            btn.id = data.ButtonsList[i].Id;
            btn.className = data.ButtonsList[i].Css;
            btn.innerText = data.ButtonsList[i].Text;
            btn.addEventListener("click", _this.WorkFlowButtonClick);
            buttonLocation.append(btn);
        }
    }
    SetupBodyController(data) {
        obJlinkscript.InputControllersList = [];
        var table = document.getElementById("LStblControls");
        var body = document.createElement("tbody");
        table.innerHTML = "";
        var tr;
        for (var i = 0; i < data.ControllerList.length; i++) {
            var ctr = data.ControllerList[i];
            if (i % 2 === 0) {
                tr = document.createElement("tr");
                body.appendChild(tr);
                table.appendChild(body);
            }
            var td = document.createElement("td");
            switch (ctr.ControlerType) {
                case "textbox":
                    var input = document.createElement("input");
                    input.id = ctr.Id;
                    input.value = ctr.Text;
                    input.className = ctr.Css;
                    input.type = "text";
                    td.appendChild(input);
                    tr.appendChild(td);
                    //var afterCreateTextbox = document.getElementById(input.id);
                    obJlinkscript.InputControllersList.push(input);
                    break;
                case "label":
                    var label = document.createElement("label");
                    label.id = ctr.Id;
                    label.innerText = ctr.Text;
                    label.className = ctr.Css;
                    td.appendChild(label);
                    tr.appendChild(td);
                    break;
                case "dropdown":
                    var dropdown = document.createElement("select");
                    for (var d = 0; d < ctr.dropdownItems.length; d++) {
                        var option = document.createElement("option");
                        option.innerText = ctr.dropdownItems[d].text;
                        option.value = ctr.dropdownItems[d].value;
                        dropdown.appendChild(option);
                        dropdown.selectedIndex = ctr.dropIndex;
                    }
                    //dropdown.type = "dropdown";
                    dropdown.id = ctr.Id;
                    dropdown.className = ctr.Css;
                    td.appendChild(dropdown);
                    tr.appendChild(td);
                    //var afterCreateDropdown = document.getElementById(dropdown.id);
                    obJlinkscript.InputControllersList.push(dropdown);
                    break;
                case "listBox":
                    var listbox = document.createElement("select");
                    for (var d = 0; d < ctr.listboxItems.length; d++) {
                        var option = document.createElement("option");
                        option.innerText = ctr.listboxItems[d].text;
                        option.value = ctr.listboxItems[d].value;
                        listbox.appendChild(option);
                        listbox.selectedIndex = ctr.dropIndex;
                    }
                    listbox.size = parseInt(ctr.rowCounter) + 1;
                    listbox.id = ctr.Id;
                    listbox.className = ctr.Css;
                    td.appendChild(listbox);
                    tr.appendChild(td);
                    //var afterCreateDropdown = document.getElementById(listbox.id);
                    obJlinkscript.InputControllersList.push(listbox);
                    break;
                case "radiobutton":
                    var radio = document.createElement("input");
                    radio.id = ctr.Id;
                    radio.name = ctr.Groupname + ctr.Text;
                    radio.innerText = ctr.Text;
                    radio.className = ctr.Css;
                    radio.type = "radio";
                    td.appendChild(radio);
                    tr.appendChild(td);
                    //var afterCreateRadio = document.getElementById(radio.id);
                    obJlinkscript.InputControllersList.push(radio);
                    break;
                case "checkbox":
                    var checkbox = document.createElement("input");
                    checkbox.id = ctr.Id;
                    checkbox.innerText = ctr.Text;
                    checkbox.className = ctr.Css;
                    checkbox.type = "checkbox";
                    td.appendChild(checkbox);
                    tr.appendChild(td);
                    //var afterCreateCheckbox = document.getElementById(checkbox.id);
                    obJlinkscript.InputControllersList.push(checkbox);
                    break;
                case "textarea":
                    var textarea = document.createElement("textarea");
                    textarea.id = ctr.Id;
                    textarea.innerText = ctr.Text;
                    textarea.className = ctr.Css;
                    td.appendChild(textarea);
                    tr.appendChild(td);
                    //var afterCreateTextarea = document.getElementById(textarea.id)
                    obJlinkscript.InputControllersList.push(textarea);
                    break;
                default:
            }
        }
    }
    CloseDialog(reset) {
        $("#LinkScriptDialogBox").hide();

        if (reset) {
            obJlinkscript.ScriptDone = false;
            obJlinkscript.isBeforeAdd = false;
            obJlinkscript.isBeforeEdit = false;
            obJlinkscript.isAfterAdd = false;
            obJlinkscript.isAfterEdit = false;
        }
    }
    WorkFlowButtonClick() {
        var elem = this;
        $(app.SpinningWheel.Main).show();
        var linkscriptUidata = JSON.stringify({ 'linkscriptUidata': obJlinkscript.GatherDataTosendLinkscript(elem) });
        var call = new DataAjaxCall(app.url.server.FlowButtonsClickEvent, ajax.Type.Post, ajax.DataType.Json, linkscriptUidata, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((data) => {
            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e")
                setTimeout(() => {
                    window.location.reload();
                }, 2000)
                return;
            }

            //if data is not successful "true" then the script is broken so stop the process of the script.
            if (!data.Successful) {
                obJlinkscript.CloseDialog(false);
                if (data.ReturnMessage != "") {
                    showAjaxReturnMessage(data.ReturnMessage, "w");
                }
                $(app.SpinningWheel.Main).hide();
                obJlinkscript.RefreshGrideAfterlinkScriptDone(data);
                return
            }
            //if the UnloadPromptWindow is false it means we jump to the next popup
            if (data.UnloadPromptWindow === false) {
                obJlinkscript.OpenDialog(data);
            } else {
                //finally if the UnloadPromptWindow is true means we don't have more dialogs and we are ready to close the dialog.
                obJlinkscript.CloseDialog(false);
                //if callerType id before: delete, edit or add then run obJgridfunc.SaveRowAfterBuild();
                if (obJlinkscript.isBeforeAdd || obJlinkscript.isBeforeEdit) {
                    obJlinkscript.ScriptDone = true;
                    obJlinkscript.isBeforeAdd = false;
                    obJlinkscript.isBeforeEdit = false;
                    obJlinkscript.isAfterAdd = false;
                    obJlinkscript.isAfterEdit = false;
                    obJgridfunc.SaveRowAfterBuild().then((isSuccess) => {
                        //obJlinkscript.ScriptDone = false;
                        obJlinkscript = new Linkscript();
                    });
                }

                if (obJlinkscript.isBeforeDelete) {
                    obJlinkscript.ScriptDone = true;
                    obJlinkscript.isBeforeDelete = false;
                    obJtoolbarmenufunc.DeleteRowsFromServer().then((isSuccess) => {
                        obJlinkscript = new Linkscript();
                    });
                }

                //check if linkscript requirs grid refresh 
                obJlinkscript.RefreshGrideAfterlinkScriptDone(data);
            }
            $(app.SpinningWheel.Main).hide();
        });
    }
    GatherDataTosendLinkscript(elem) {
        var DataArray = [];
        for (var i = 0; i < obJlinkscript.InputControllersList.length; i++) {
            var id = obJlinkscript.InputControllersList[i].id;
            var type = obJlinkscript.InputControllersList[i].type;
            var value = "";
            switch (type) {
                case "text":
                    value = obJlinkscript.InputControllersList[i].value;
                    break;
                case "textarea":
                    value = obJlinkscript.InputControllersList[i].value;
                    break;
                case "radio":
                    value = obJlinkscript.InputControllersList[i].checked;
                    break;
                case "checkbox":
                    var ischecked = obJlinkscript.InputControllersList[i].checked != true ? "0" : "1";
                    value = ischecked;
                    break;
                case "select-one":
                    var ddIndex = obJlinkscript.InputControllersList[i].selectedIndex;
                    if (ddIndex != -1) {
                        value = ddIndex + '%&&&%' + obJlinkscript.InputControllersList[i].options[ddIndex].innerText;
                    } else {
                        value = 0 + '%&&&%' + "";
                    }
                    break;
                default:
            }
            DataArray.push({ id: id, value: value, type: type });
        }
        DataArray.push({ id: elem.id, value: elem.value, type: "button" });
        //DataArray.push({ linkparams: this.Lsparams });
        return DataArray;
    }
    RefreshGrideAfterlinkScriptDone(data) {
        if (data.GridRefresh === true || app.globalverb.GForceRefresh === true) {
            obJgridfunc.LoadViewTogrid(obJgridfunc.ViewId, pagingfun.SelectedIndex).then((istrue) => {
                if (data.ReturnMessage !== "") {
                    showAjaxReturnMessage(data.ReturnMessage, "s")
                }
            }).catch(err => {
                var msg = err;
                if (data.ReturnMessage !== "") {
                    msg + "\r\n" + data.ReturnMessage;
                }
                showAjaxReturnMessage(msg, "e");
            });
        }
    }
    AfterLinkScript(data) {
        //linkscript after
        if (data.scriptReturn.isAfterAddLinkScript) {
            obJlinkscript.isAfterAdd = data.scriptReturn.isAfterAddLinkScript
            obJlinkscript.id = data.scriptReturn.ScriptName;
            obJlinkscript.keyvalue = data.scriptReturn.keyValue;
            //$(app.SpinningWheel.Grid).hide();
            obJlinkscript.LinkscriptEventCall(obJlinkscript);

        } else if (data.scriptReturn.isAfterEditLinkScript) {
            obJlinkscript.isAfterEdit = data.scriptReturn.isAfterEditLinkScript;
            obJlinkscript.id = data.scriptReturn.ScriptName;
            //$(app.SpinningWheel.Grid).hide();
            obJlinkscript.LinkscriptEventCall(obJlinkscript);
        }
        obJlinkscript = new Linkscript();
    }
};
//GLOBAL SEARCH FUNCTIONS
class GlobalSearch {
    constructor() {
        this.SearchInput = "";
        this.ViewId = "";
        this.TableName = "";
        this.rowselected = "";
        this.ChkAttch = "";
        this.ChkcurTable = "";
        this.ChkUnderRow = "";
        this.KeyValue = "";
        this.IncludeAttchment = "";
    }
    RunSearch(isCallFromInputDialog, input, IncludeAttachment, IscurrentTable, IsCurrentRow) {
        //if (input.length < 3) return showAjaxReturnMessage("please, enter at least 3 characters(needs to add the language file)", "w");
        if (input.length < 3) return showAjaxReturnMessage(app.language["msgJsWarningSearch"], "w");
        this.ChkAttch = IncludeAttachment
        this.ChkcurTable = IscurrentTable;
        this.ChkUnderRow = IsCurrentRow;
        this.SearchInput = input;
        if (IscurrentTable || IsCurrentRow) {
            var rowIndex = obJgridfunc.GetSelectedRowskey().length;
            this.Currentrow = parseInt(obJgridfunc.GetSelectedRowskey()[rowIndex - 1].rowKeys);
        }
        this.ViewId = obJgridfunc.ViewId;
        this.TableName = obJgridfunc.TableName;
        var data = JSON.stringify({ 'paramss': this });
        var call = new DataAjaxCall(app.url.server.RunglobalSearch, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
        $(app.SpinningWheel.Main).show();
        call.Send().then((data) => {
            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "w")
                $(app.SpinningWheel.Main).hide();
                return;
            }
            if (data.HTMLSearchResults === null) return;
            if (isCallFromInputDialog === true) {
                document.querySelector(app.domMap.Dialogboxbasic.DlgbodyId).innerHTML = data.HTMLSearchResults;
            } else {
                this.FirstSearchClick(data);
                var dialogcontent = document.querySelector(app.domMap.Dialogboxbasic.DlgBsContent)
                dialogcontent.style.maxHeight = "500px";
                dialogcontent.style.overflow = "auto"
            }

            $(app.SpinningWheel.Main).hide();
            //take seach div scroll up to 0 position
            document.getElementById("dlgBsContent").scrollTo(0, 0)
        })
    }
    FirstSearchClick(data) {
        var dlg = new DialogBoxBasic(this.BuildDialogHeader(), data.HTMLSearchResults, app.domMap.Dialogboxbasic.Type.Content)
        dlg.ShowDialog();
        document.getElementById("closedlg").style.marginTop = "-9px"
        //pass variables to the dialog;
        document.querySelector(app.domMap.GlobalSearching.DialogSearchInput).value = obJglobalsearch.SearchInput;
        document.querySelector(app.domMap.GlobalSearching.DialogchkAttachments).checked = obJglobalsearch.ChkAttch;
        //for the another checkboxes check I check if view id is not empty in case is not show another 2 check boxs in dialog.
        if (obJgridfunc.ViewId !== "") {
            var currentTable = document.querySelector(app.domMap.GlobalSearching.DialogCurrenttable);
            currentTable.parentElement.style.display = "block";
            currentTable.checked = obJglobalsearch.ChkcurTable;

            var chkunder = document.querySelector(app.domMap.GlobalSearching.DialogUnderthisrow);
            chkunder.parentElement.style.display = "block";
            chkunder.checked = obJglobalsearch.ChkUnderRow;
        }
    }
    SearchAllClick(dom, viewid, search, keyword, includeAttchment) {
        var _this = this;
        bc.innerHTML = "";
        obJgridfunc.ViewId = viewid;
        obJgridfunc.ViewType = app.Enums.ViewType.GlobalSearch;
        this.ViewId = viewid;
        this.IncludeAttchment = includeAttchment;
        this.KeyValue = keyword;
        var data = JSON.stringify({ paramss: this })
        var call = new DataAjaxCall(app.url.server.GlobalSearchAllClick, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
        call.Send().then((data) => {
            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e")
                $(app.SpinningWheel.Main).hide();
                return;
            }
            obJlastquery.isQueryApplied = false;
            obJgridfunc.LoadGridpartialView(data).then(() => {
                _this.SelectViewAfterSearch(viewid, dom, data)
            })
        });
    }
    SearchClick(dom, viewid, KeyValue, search) {
        var _this = this;
        bc.innerHTML = "";
        obJgridfunc.ViewId = viewid;
        this.ViewId = viewid;
        this.KeyValue = KeyValue;
        obJgridfunc.ViewType = app.Enums.ViewType.GlobalSearch;
        var data = JSON.stringify({ paramss: this })
        var call = new DataAjaxCall(app.url.server.GlobalSearchClick, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
        call.Send().then((data) => {

            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e")
                $(app.SpinningWheel.Main).hide();
                return;
            }
            obJlastquery.isQueryApplied = false;
            obJgridfunc.LoadGridpartialView(data).then(() => {
                _this.SelectViewAfterSearch(viewid, dom, data)
            })
        });
    }
    Setuplocation(viewid, data) {
        var dom = app.DomLocation.Current;
        dom.dataset.location = 0;
        dom.dataset.viewid = viewid;
        dom.dataset.viewname = data.ViewName;
        app.DomLocation.Current = dom;
    }
    SelectViewAfterSearch(viewid, dom, data) {
        //reset the query main window object
        buildQuery = [];
        var names = document.getElementsByName("viewAccess");
        for (var i = 0; i < names.length; i++) {
            if (names[i].dataset.viewid == viewid) {
                obJleftmenu.ResetMenu()
                names[i].parentElement.parentElement.parentElement.firstChild.click()
                obJgridfunc.MarkSelctedMenu(names[i]);

                dom.dataset.location = 0;
                dom.dataset.viewid = viewid;
                dom.dataset.viewname = data.ViewName;
                app.DomLocation.Current = dom;

                app.DomLocation.Previous = names[i];
                return;
            }
        }
    }
    BuildDialogHeader() {
        return `<p style="margin-top: -13px;">${app.language["msgSearchResults"]}</p><div id = "divCustomSearch1" class="input-group search_box">
               <input type="text" id="DialogSearchInput" autocompletetype="Search" maxlength="256" class="form-control" title="Searches all views in the database for matching words.">
               <span class="input-group-btn">
               <button type="button" id="dialogSearchButton" title="Search" class="btn btn-default glyphicon glyphicon-search"></button></div>
               <div class="input-group checkbox search_checkbox m-t-5" style="display:inline-flex;">
               <span><input type="checkbox" id="DialogchkAttachments" tooltip="Search attachment text"><label onclick="document.getElementById('DialogchkAttachments').click()">Include attachments</label></span>&nbsp;&nbsp;
               <span style="display:none;"><input type="checkbox" id="DialogCurrenttable" tooltip="Search attachment text"><label onclick="document.getElementById('DialogCurrenttable').click()">Current table only</label></span>&nbsp;&nbsp;
               <span style="display:none;"><input type="checkbox" id="DialogUnderthisrow" tooltip="Search attachment text"><label onclick="document.getElementById('DialogUnderthisrow').click()">Under this row only</label></span>&nbsp;&nbsp;
               </div>`
    }
}
//NEWS FEED FUNCTIONS
class NewsFeed {
    constructor() {
        this.IsTabfeed = document.querySelector(app.domMap.NewsFeed.IsTabfeed)
        this.NewsFrame = document.querySelector(app.domMap.NewsFeed.NewsFrame)
        this.TabNewsTable = document.querySelector(app.domMap.NewsFeed.TabNewsTable)
        this.TxtNewsUrl = document.querySelector(app.domMap.NewsFeed.TxtNewsUrl)
    }
    NewFeedSetup() {
        if (this.IsTabfeed.value == 1) {
            this.NewsFrame.style.display = "none";
            this.TabNewsTable.style.display = "unset";
        } else {
            this.TabNewsTable.style.display = "none";
            this.NewsFrame.style.display = "unset";
        }
    }
    SaveNewUrl() {
        var _this = this;
        var data = { NewUrl: this.TxtNewsUrl.value };
        var call = new DataAjaxCall(app.url.server.SaveNewsURL, ajax.Type.Post, ajax.DataType.Json, data, "", "", "", "")
        call.Send().then((data) => {

            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e");
                $(app.SpinningWheel.Main).hide();
                return;
            }

            if (data.isSuccess && this.TxtNewsUrl.value != "") {
                //set new ifram with url
                _this.TabNewsTable.style.display = "none";
                _this.NewsFrame.style.display = "unset";
                _this.NewsFrame.src = _this.TxtNewsUrl.value;
            } else {
                //set tab news
                _this.NewsFrame.style.display = "none";
                _this.TabNewsTable.style.display = "unset";
            }
        })
    }
}

//TASK BAR FUNCTIONS
class TaskBar {
    constructor() {

    }
    TaskbarLinks(viewid, tasks, elem) {
        app.DomLocation.Previous = app.DomLocation.Current;
        app.DomLocation.Current = elem;
        app.DomLocation.Current.dataset.location = app.Enums.location.TaskBarView;
        app.DomLocation.Current.dataset.viewname = elem.innerText
        obJgridfunc.ViewType = app.Enums.location.TaskBarView;
        obJlastquery.CheckIfLastQuery(viewid);

        var data = { viewId: viewid };
        var call = new DataAjaxCall(app.url.server.TaskBarClick, ajax.Type.Get, ajax.DataType.Json, data, "", "", "", "");
        call.Send().then((data) => {
            if (data.isError) {
                showAjaxReturnMessage(data.Msg, "e")
                $(app.SpinningWheel.Main).hide();
                return;
            }
            obJleftmenu.ResetMenu();
            obJgridfunc.UnMarkSelectionMenu(app.DomLocation.Previous)
            //clear bread crumbs
            bc.innerHTML = "";
            app.DomLocation.Current = elem;
            obJgridfunc.LoadGridpartialView(data).then(() => {

            });
        });
    }
}
//ATTCHMENT VIEWER FUNCTIONS DIALOG

class AttachmentsView {
    constructor() {
        this._imgid = 1;
        this._flyOutImage = "";
        this._downloadUrl = "";
        this._attachName = "";
        this._viewerLink = "";
        this._pageSize = "";
        this._pageIndex = "";
        this._displayCount = "";
    }
    StartAttachmentDialog() {
        //chcek if new row (in case is a new row don't popup the attachment dialog)
        if (hot.getDataAtRow(rowselected)[0] === null) return;
        var docdata = app.data.TableName + "," + hot.getDataAtRow(rowselected[0])[0] + "," + app.data.ViewName
        $(app.SpinningWheel.Main).show();
        var data = { 'docdata': docdata, 'isMVC': true }
        var call = new DataAjaxCall(app.url.server.LoadFlyoutPartial, ajax.Type.Get, ajax.DataType.Html, data, "", "", "", "")
        call.Send().then((partialData) => {
            document.querySelector(app.domMap.DialogAttachment.AttachmentModalBody).innerHTML = "";
            document.querySelector(app.domMap.DialogAttachment.AttachmentModalBody).innerHTML = partialData;
            this.OpenDialog()
            this.ScrollerBinding();
            this.DragAnddropNewFile();
            $(app.SpinningWheel.Main).hide();
        });
    }
    OpenDialog() {
        $(app.domMap.DialogAttachment.Openformattachment).show();
        $('.modal-dialog').draggable({
            handle: ".modal-header",
            stop: function (event, ui) {
            }
        });
        this.Paging()
        this.ScrollResizing();
    }
    ScrollerBinding(docdata) {
        var _this = this;
        $(app.domMap.DialogAttachment.AttachmentModalBody).unbind('scroll').bind('scroll',
            function (e) {
                var docdata = app.data.TableName + "," + hot.getDataAtRow(rowselected[0])[0] + "," + app.data.ViewName
                if (Math.ceil($(this).scrollTop() + $(this).innerHeight()) >= $(this)[0].scrollHeight) {
                    _this._displayCount = $(app.domMap.DialogAttachment.paging.ListThumbnailDetailsImg).length;
                    var vTotalCount =
                        parseInt($(app.domMap.DialogAttachment.paging.TotalRecCount).val() == "" ? 0 : $(app.domMap.DialogAttachment.paging.TotalRecCount).val());
                    if (_this._displayCount != vTotalCount) {
                        _this._pageIndex =
                            parseInt($(app.domMap.DialogAttachment.paging.SPageIndex).val() == "" ? 1 : $(app.domMap.DialogAttachment.paging.SPageIndex).val()) + 1;
                        _this._pageSize = parseInt($(app.domMap.DialogAttachment.paging.SPageSize).val() == "" ? 0 : $(app.domMap.DialogAttachment.paging.SPageSize).val());
                        var data = { 'docdata': docdata, 'PageIndex': _this._pageIndex, 'PageSize': _this._pageSize, 'viewName': app.data.ViewName, 'isMVC': true };
                        var call = new DataAjaxCall(app.url.server.LazyLoadPopupAttachments, ajax.Type.Get, ajax.DataType.Json, data, "", "", "", "");
                        call.Send().then((result) => {
                            var pOutputObject = JSON.parse(result.JsonString);
                            var loopObject = pOutputObject.FlyOutDetails;
                            $.each(loopObject,
                                function (key, value) {
                                    _this._flyOutImage = value.sFlyoutImages;
                                    _this._imgid = key;
                                    _this._downloadUrl = _this.HtmlDownLoadLinkBuilder(value, pOutputObject);
                                    _this._attachName = value.sAttachmentName;
                                    _this._viewerLink = value.sViewerLink;
                                    var html = _this.ScrollerHtmlBuilder(_this)
                                    $(app.domMap.DialogAttachment.ThumbnailDetails).append(html);
                                });
                            _this.ScrollAfterLoadReturn(_this);
                        });
                    }
                }
            });
    }
    HtmlDownLoadLinkBuilder(value, pOutputObject) {
        var urlParameterEncode = "filePath=" + encodeURIComponent(value.sOrgFilePath) +
            "&fileName=" + encodeURIComponent(value.sAttachmentName) +
            "&docKey=" + encodeURIComponent(value.downloadEncryptAttachment) +
            "&viewName=" + encodeURIComponent(pOutputObject.viewName);
        console.log("urlParameterEncode: ",urlParameterEncode)
        var url = '/Common/DownloadAttachment?' + urlParameterEncode + "&attchVersion=" + value.attchVersion;
        return url;
    }
    ScrollerHtmlBuilder(_this) {
        //main div
        var mainDiv = document.createElement("div");
        mainDiv.className = "col-lg-4 col-md-6 col-sm-6 col-xs-12";
        //thmbnail main
        var thmbMain = document.createElement("div");
        thmbMain.className = "Thmbnail-main";
        mainDiv.appendChild(thmbMain);
        //thmbnail header
        var thmbHeader = document.createElement("div");
        thmbHeader.className = "Thmbnail-header";
        thmbHeader.innerHTML = _this._attachName;
        thmbMain.appendChild(thmbHeader);
        //documentviewer/index link call
        var ancorLink = document.createElement("a");
        ancorLink.href = _this._viewerLink;
        ancorLink.target = "_blank";
        thmbMain.appendChild(ancorLink);
        //div thbnail-body
        var thmbBody = document.createElement("div");
        thmbBody.className = "Thmbnail-body";
        ancorLink.appendChild(thmbBody);
        //div caption symble
        var divCaption = document.createElement("div");
        divCaption.className = "caption";
        thmbBody.appendChild(divCaption);
        //div caption content
        var captionContact = document.createElement("div");
        captionContact.className = "caption-content";
        divCaption.appendChild(captionContact);
        //i eye symle icon
        var eyeSymble = document.createElement("i");
        eyeSymble.className = "fa fa-eye fa-3x";
        captionContact.appendChild(eyeSymble);
        //image elemnt
        var img = document.createElement("img");
        img.src = "data:image/jpg;base64," + _this._flyOutImage;
        img.id = _this._imgid; //need to pass 
        img.className = "img-responsive";
        img.style.height = "300px";
        img.style.width = "280px";
        thmbBody.appendChild(img);
        //div footer
        var thmbFooter = document.createElement("div");
        thmbFooter.className = "Thmbnail-footer";
        //div col
        var divCol = document.createElement("div");
        divCol.className = "col-md-12 col-sm-12 col-xs-12";
        thmbFooter.appendChild(divCol);
        //span stack
        var spanStack = document.createElement("span");
        spanStack.className = "fa-stack";
        divCol.appendChild(spanStack);
        //ancor download link
        var downloadLink = document.createElement("a");
        downloadLink.href = _this._downloadUrl;
        downloadLink.className = "a-color";
        downloadLink.setAttribute("data-toggle", "tooltip");
        downloadLink.title = "Download";
        spanStack.appendChild(downloadLink);
        //download symble
        var downSymble = document.createElement("span");
        downSymble.className = "fa-stack";
        downloadLink.appendChild(downSymble);
        //download symble items
        var itm1 = document.createElement("i");
        var itm2 = document.createElement("i");
        itm1.className = "fa fa-arrow-down fa-stack-1x";
        itm1.style.top = "7px";
        itm2.className = "fa fa-circle-thin fa-stack-2x";
        downSymble.appendChild(itm1);
        downSymble.appendChild(itm2);
        thmbMain.appendChild(thmbFooter);
        //clearfix div
        var clearfixDiv = document.createElement("div");
        clearfixDiv.className = "clearfix";
        thmbFooter.appendChild(clearfixDiv);
        return mainDiv;
    }
    ScrollAfterLoadReturn(_this) {
        _this.displayCount = $(app.domMap.DialogAttachment.paging.ListThumbnailDetailsImg).length;
        var pageText = String.format(app.language["lblAttachmentPopupPagging"], _this.displayCount.toString(), $(app.domMap.DialogAttachment.paging.TotalRecCount).val());
        $(app.domMap.DialogAttachment.paging.ResultDisplay).text(pageText);
        $(app.domMap.DialogAttachment.paging.SPageIndex).val(_this._pageIndex);
        $(app.domMap.DialogAttachment.paging.SPageSize).val(_this._pageSize);
    }
    Paging() {
        var pdisplayCount = document.querySelectorAll(app.domMap.DialogAttachment.paging.ListThumbnailDetailsImg).length;
        var pageText = String.format(app.language["lblAttachmentPopupPagging"], pdisplayCount.toString(), document.querySelector(app.domMap.DialogAttachment.paging.TotalRecCount).value);
        document.querySelector(app.domMap.DialogAttachment.paging.ResultDisplay).innerText = pageText;
    }
    ScrollResizing() {
        //var attachbody = 
        if (document.querySelector(app.domMap.DialogAttachment.paging.TotalRecCount).value <= 6) {
            document.querySelector(app.domMap.DialogAttachment.AttachmentModalBody).style.maxHeight = "calc(100vh - 273px)";
        } else {
            document.querySelector(app.domMap.DialogAttachment.AttachmentModalBody).style.maxHeight = "500px";
        }
    }
    CloseDialog() {
        $(app.domMap.DialogAttachment.Openformattachment).hide();
    }
    UploadAttachmentOnNewAdd(FlyoutUploderFiles) {
        ;
        //var totalsize = 0;
        //var successFilesforPopup = [];
        var isfilesSupported = true;
        //var failedFilesforPopup = [];
        var formdata = new FormData();
        formdata.append("tabName", app.data.TableName);
        formdata.append("tableId", hot.getDataAtRow(rowselected)[0]);
        formdata.append("viewId", app.data.ViewId);
        for (var i = 0; i < FlyoutUploderFiles.length; i++) {
            var format = FlyoutUploderFiles[i].name.toString();
            format = format.split(".");
            var cFileFormat = obJattachmentsview.IsSupportedFile(format[format.length - 1]);
            if (cFileFormat) {
                //var size = FlyoutUploderFiles[i].size;
                //totalsize = parseInt(totalsize) + parseInt(size);
                //successFilesforPopup.push({ tabName: app.data.TableName, tableId: hot.getDataAtRow(rowselected)[0], viewId: app.data.ViewId})
                formdata.append(FlyoutUploderFiles[i].name, FlyoutUploderFiles[i]);
                //successFilesforPopup.push(FlyoutUploderFiles[i].name.toString());
            } else {
                isfilesSupported = false;
                showAjaxReturnMessage(app.language["msgDocViewerInvalidFileFormatNew"] + "  <b style='color:red'>" + format + "</b>", "w");
                break;
                //failedFilesforPopup.push(FlyoutUploderFiles[i].name.toString());
            }
        }
        if (isfilesSupported === true) {
            obJattachmentsview.SaveNewAttachment(formdata);
        } else {
            return;
        }
    }
    IsSupportedFile(format) {
        var FileFormat = ['abc', 'abic', 'afp', 'ani', 'anz', 'arw', 'bmp', 'cal', 'cin', 'clp'
            , 'cmp', 'cmw', 'cr2', 'crw', 'cur', 'cut', 'dcr', 'dcs', 'dcm', 'dcx'
            , 'dng', 'dxf', 'eps', 'exif', 'fax', 'fit', 'flc', 'fpx', 'gif', 'gtiff'
            , 'hdp', 'ico', 'iff', 'ioca', 'ingr', 'img', 'itg', 'jbg', 'jb2', 'jpg'
            , 'jpeg', 'j2k', 'jp2', 'jpm', 'jpx', 'kdc', 'mac', 'mng', 'mob', 'msp'
            , 'mrc', 'nef', 'nitf', 'nrw', 'orf', 'pbm', 'pcd', 'pcx', 'pdf', 'pgm'
            , 'png', 'pnm', 'ppm', 'ps', 'psd', 'psp', 'ptk', 'ras', 'raf', 'raw'
            , 'rw2', 'sct', 'sff', 'sgi', 'smp', 'snp', 'sr2', 'srf', 'tdb', 'tfx'
            , 'tga', 'tif', 'tifx', 'vff', 'wbmp', 'wfx', 'x9', 'xbm', 'xpm', 'xps'
            , 'xwd', 'cgm', 'cmx', 'dgn', 'drw', 'dxf', 'dwf', 'dwfx', 'dwg', 'e00'
            , 'emf', 'gbr', 'mif', 'nap', 'pcl', 'pcl6', 'pct', 'plt', 'shp', 'svg'
            , 'wmf', 'wmz', 'wpg', 'doc', 'docx', 'eml', 'mobi', 'epub', 'html', 'msg'
            , 'ppt', 'pptx', 'pst', 'rtf', 'svg', 'txt', 'xls', 'xlsx', 'xps'];

        return FileFormat.includes(format.toString().toLowerCase());
    }
    SaveNewAttachment(formdata) {
        $(app.SpinningWheel.Main).show();
        var call = new DataAjaxCall(app.url.server.AddNewAttachment, ajax.Type.Post, ajax.DataType.Json, formdata, ajax.ContentType.False, ajax.ProcessData.False, "", "")
        call.Send().then((model) => {
            switch (model.checkConditions) {
                case "success":
                    obJattachmentsview.AfterAddAttachments();
                    break;
                case "permission":
                    if (model.errorNumber == 0) {
                        showAjaxReturnMessage(app.language["msgDocViewerAddPermission"], 'w');
                    } else if (model.errorNumber == 1) {
                        showAjaxReturnMessage(app.language["msgDocViewerVolumePermission"], 'w');
                    }

                    break;
                case "maxsize":
                    showAjaxReturnMessage(model.WarringMsg, 'w');
                    break;
                case "error":
                    showAjaxReturnMessage(model.WarringMsg, 'w');
                    break;
                default:
            }

            $(app.SpinningWheel.Main).hide();
        });
    }
    AfterAddAttachments() {
        var docdata = app.data.TableName + "," + hot.getDataAtRow(rowselected[0])[0] + "," + app.data.ViewName
        $(app.SpinningWheel.Main).show();
        var data = { 'docdata': docdata, 'isMVC': true }
        var call = new DataAjaxCall(app.url.server.LoadFlyoutPartial, ajax.Type.Get, ajax.DataType.Html, data, "", "", "", "")
        call.Send().then((partialData) => {
            document.querySelector(app.domMap.DialogAttachment.AttachmentModalBody).innerHTML = "";
            document.querySelector(app.domMap.DialogAttachment.AttachmentModalBody).innerHTML = partialData;
            this.ScrollerBinding();
            this.Paging()
            this.ScrollResizing();
            //change the html to clips with plus
            var td = hot.getCell(rowselected[0], obJgridfunc.StartInCell() - 1);
            td.innerHTML = '<i style="cursor: pointer" onclick="obJattachmentsview.StartAttachmentDialog()" class="fa fa-paperclip fa-flip-horizontal fa-2x theme_color"></i>'
            $(app.SpinningWheel.Main).hide();
        });
    }
    DragAnddropNewFile() {
        var _this = this;
        document.querySelector(app.domMap.DialogAttachment.EmptydropDiv)
        var newFile = document.querySelector(app.domMap.DialogAttachment.AttachmentModalBody);
        newFile.ondrop = function (e) {
            e.preventDefault();
            _this.UploadAttachmentOnNewAdd(e.dataTransfer.files);
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

}
//TOOLBAR MENU FUNCTIONS (RIGHT CLICK AND UPPER TOOLBAR)

class ToolBarMenuFunc {
    constructor() {
        this.ids = [];
        this.TableName = [];
    }
    GetMenu() {
        var _this = this;
        return {
            items: {
                "copy": {
                    name: "Copy",
                },
                "exportPrint": {
                    name: "Export/Print",
                    submenu: {
                        items: [
                            {
                                key: "exportPrint:print",
                                name: "Print",
                                callback: function (key, name) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function () {
                                    if (!app.data.RightClickToolBar.Menu1Print) { return true };
                                },

                            },
                            {
                                key: "exportPrint:blackAndWhite",
                                name: "Black & White",
                                callback: function (key, name) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function () {
                                    if (!app.data.RightClickToolBar.Menu1btnBlackWhite) { return true };
                                },
                            },
                            {
                                key: "exportPrint:exportSelectedCsv",
                                name: "Export Selected(csv)",
                                callback: function (key, name) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function () {
                                    if (!app.data.RightClickToolBar.Menu1btnExportCSV) { return true };
                                },
                            },
                            {
                                key: "exportPrint:exportAllCsv",
                                name: "Export All(csv)",
                                callback: function (key, name) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function () {
                                    if (!app.data.RightClickToolBar.Menu1btnExportCSVAll) { return true };
                                },
                            },
                            {
                                key: "exportPrint:exportSelectedTxt",
                                name: "Export Selected(txt)",
                                callback: function (key, name) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function () {
                                    if (!app.data.RightClickToolBar.Menu1btnExportTXT) { return true };
                                },
                            },
                            {
                                key: "exportPrint:exportAllTxt",
                                name: "Export All(txt)",
                                callback: function (key, name) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function () {
                                    if (!app.data.RightClickToolBar.Menu1btnExportTXTAll) { return true };
                                },
                            },
                        ]
                    }
                },
                "transferRequest": {
                    name: "Transfer/Request",
                    submenu: {
                        items: [
                            {
                                key: "transferRequest:Request",
                                name: "Request",
                                callback: function (key, options) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function () {
                                    if (!app.data.RightClickToolBar.Menu2btnRequest) { return true };
                                },
                            },
                            {
                                key: "transferRequest:transferSelected",
                                name: "Transfer Selected",
                                callback: function (key, options) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function (key, options) {
                                    if (!app.data.RightClickToolBar.Menu2btnTransfer) { return true }
                                }
                            },
                            {
                                key: "transferRequest:transferAll",
                                name: "Transfer All",
                                callback: function (key, options) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function (key, options) {
                                    if (!app.data.RightClickToolBar.Menu2btnTransfersTransferAll) { return true }
                                }
                            },
                            {
                                key: "transferRequest:Delete",
                                name: "Delete",
                                callback: function (name, coords, event) {
                                    _this.BeforeDeleteRow();
                                },
                                hidden: function () {
                                    if (!app.data.RightClickToolBar.Menu2delete) { return true };
                                },
                            },
                            {
                                key: "transferRequest:Move",
                                name: "Move",
                                callback: function () {
                                    alert("Developoing in progress!")
                                },
                                hidden: function () {
                                    if (!app.data.RightClickToolBar.Menu2move) { return true };
                                },
                            },

                        ]
                    }
                },
                "favorite": {
                    name: "Favorite",
                    submenu: {
                        items: [
                            {
                                key: "favorite:NewFavorite",
                                name: "New Favorite",
                                callback: function (key, options) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function () {
                                    if (!app.data.RightClickToolBar.Favorive) { return true };
                                },
                            },
                            {
                                key: "favorite:AddToFavorit",
                                name: "Add To Favorit",
                                callback: function (key, options) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function (key, options) {
                                    if (!app.data.RightClickToolBar.Favorive) { return true }
                                }
                            },
                            {
                                key: "favorite:Importintofavorite",
                                name: "Import Into Favorite",
                                callback: function (key, options) {
                                    alert("Developoing in progress!")
                                },
                                hidden: function (key, options) {
                                    if (!app.data.RightClickToolBar.Favorive) { return true }
                                }
                            },
                        ]
                    }
                }
            }
        }
    }
    //delete functions
    BeforeDeleteRow() {
        if (rowselected.length > 200) return showAjaxReturnMessage("You can't delete more than 200 rows at the time!", "w");
        if (rowselected.length === 0) {
            showAjaxReturnMessage(app.language["msgJsDataSelectOneRow"], "w");
            return;
        }

        //check emppty rowselected
        this.DeleteSelectedEmptyRows().then((countRows) => {
            var _this = this;
            if (countRows === 0) {
                obJgridfunc.LoadViewTogrid(obJgridfunc.ViewId, pagingfun.SelectedIndex);
                return;
            } else {
                //clear ids
                this.ids = [];
                for (var i = 0; i < rowselected.length; i++) {
                    var rowids = hot.getDataAtRow(rowselected[i])[0];
                    if (rowids !== null) {
                        this.ids.push(rowids);
                    }
                }
                this.TableName = app.data.TableName;
                var data = JSON.stringify({ paramss: this })
                var call = new DataAjaxCall(app.url.server.DialogMsgConfirmDelete, ajax.Type.Post, ajax.DataType.Html, data, ajax.ContentType.Utf8, "", "", "", "");
                call.Send().then((content) => {
                    var dlg = new DialogBoxBasic("Delete Item", content, app.domMap.Dialogboxbasic.Type.Content)
                    dlg.ShowDialog().then(() => {
                        //var yes = document.querySelector(app.domMap.Dialogboxbasic.Dlg.DialogMsgConfirm.DialogYes)
                        //yes.onclick = _this.DeleteRowsFromServer;
                    });
                });
            }
        });
    }
    DeleteSelectedEmptyRows() {
        //var cloneRows = Array.from(rowselected);
        var countRows = rowselected.length;
        var reduceEmpty = countRows;
        rowselected.sort(function (a, b) { return a - b })
        for (var i = 0; i < countRows; i++) {
            if (hot.getDataAtRow(rowselected[i])[0] === null) {
                hot.alter('remove_row', rowselected[i]);
                reduceEmpty--;
            }
        }
        //call server to bind a new table after delete rows.
        return new Promise((resolve, reject) => {
            resolve(reduceEmpty);
        });
    }
    DeleteRowsFromDom() {
        ///call server to delete records.
        var cloneRows = Array.from(rowselected);
        cloneRows.sort(function (a, b) { return b - a })
        for (var i = 0; i < cloneRows.length; i++) {
            hot.alter('remove_row', cloneRows[i]);
        }
        rowselected = [];
    }
    DeleteRowsFromServer() {
        var _this = this;
        app.globalverb.GCurrentRowHolder.previous_row = rowselected[0];
        var objc = {};
        objc.scriptDone = obJlinkscript.ScriptDone;
        objc.viewid = obJgridfunc.ViewId
        var data = JSON.stringify({ rowData: obJtoolbarmenufunc, paramss: objc });
        $(app.SpinningWheel.Main).show();
        var call = new DataAjaxCall(app.url.server.DeleteRowsFromGrid, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
        return new Promise(resolve => {
            call.Send().then((data) => {
                $(app.domMap.Dialogboxbasic.BasicDialog).hide();
                if (data.isError) {
                    showAjaxReturnMessage(data.Msg, "e");
                    $(app.SpinningWheel.Main).hide();
                    resolve(false);
                    return;
                };
                //before linkscript
                if (data.scriptReturn.isBeforeDeleteLinkScript && obJlinkscript.ScriptDone == false) {
                    obJlinkscript.isBeforeDelete = data.scriptReturn.isBeforeDeleteLinkScript;
                    obJlinkscript.id = data.scriptReturn.ScriptName;
                    $(app.SpinningWheel.Grid).hide();
                    $(app.SpinningWheel.Main).hide();
                    //run linkscript
                    obJlinkscript.LinkscriptEventCall(obJlinkscript);
                } else {
                    obJtoolbarmenufunc.DeleteRowsFromDom();
                    //showAjaxReturnMessage("Delete Successfuly!", "s")
                    showAjaxReturnMessage(app.language["msgDeleteSuccessfuly"], "s")
                    if (data.scriptReturn.isAfterDeleteLinkScript) {
                        obJlinkscript.isAfterDelete = data.scriptReturn.isAfterDeleteLinkScript;
                        obJlinkscript.id = data.scriptReturn.ScriptName;
                        $(app.SpinningWheel.Grid).hide();
                        $(app.SpinningWheel.Main).hide();
                        obJlinkscript.LinkscriptEventCall(obJlinkscript);
                    }
                    if (hot.countRows() == 0) {
                        obJgridfunc.LoadViewTogrid(obJgridfunc.ViewId, 1)
                    } else {
                        hot.selectCell(0, obJgridfunc.StartInCell())
                    }
                    //obJgridfunc.LoadViewTogrid(obJgridfunc.ViewId, 1)
                    $(app.SpinningWheel.Main).hide();
                    resolve(true);
                }
            });
        });
    }
    //print functions
    PrintTable(title) {
        var Tableheader = []
        //get header
        for (var i = 0; i < app.data.ListOfHeaders.length; i++) {
            if (i >= obJgridfunc.StartInCell()) {
                Tableheader.push(app.data.ListOfHeaders[i].HeaderName);
            }
        }
        var rowsData = [];
        var jsonObject = {};
        //get rows
        for (var r = 0; r < rowselected.length; r++) {
            for (var c = 0; c < app.data.ListOfHeaders.length; c++) {
                if (c >= obJgridfunc.StartInCell()) {
                    jsonObject[app.data.ListOfHeaders[c].HeaderName] = hot.getDataAtRow(rowselected[r])[c];
                }
            }
            rowsData.push(jsonObject);
            jsonObject = {};
        }

        //go to print
        printJS({
            printable: rowsData,
            properties: Tableheader,
            header: "Record count: " + rowselected.length + " " + title,
            headerStyle: "font-weight: 50px; font-size:20px",
            documentTitle: app.data.ViewName,
            gridHeaderStyle: "font-size: 22px;font-weight: bold; border: 1px solid black; width:100%;",
            gridStyle: "font-size: 20px; border: 1px solid black",
            type: 'json'
        });
    }
    //export csv selected 
    ExportCSVorTXTSelected(iscsv) {
        var obje = {};
        obje.viewId = app.data.ViewId;
        obje.RecordCount = rowselected.length;
        obje.viewName = app.data.ViewName;
        obje.tableName = app.data.TableName
        obje.IsCSV = iscsv;
        obje.IsSelectAllData = false;
        obje.ListofselectedIds = [];
        obje.DataRows = [];
        obje.Headers = [];
        obje.crumbLevel = app.data.crumbLevel;
        this.ExportCsvBuildObject(obje)
        var data = JSON.stringify({ paramss: obje })
        var call = new DataAjaxCall(app.url.server.DialogConfirmExportCSVorTXT, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                var dlg = new DialogBoxBasic(rdata.lblTitle, rdata.HtmlMessage, app.domMap.Dialogboxbasic.Type.Content);
                dlg.ShowDialog().then((data) => {
                    if (rdata.isRequireBtn) {
                        app.globalverb.IsBackgroundProcessing = rdata.IsBackgroundProcessing;
                        $(app.domMap.Dialogboxbasic.DlgBsContent).append(`<div class="modal-footer"><input type="button" onclick="$('#BasicDialog').hide()" class="btn btn-default" style="float: right;" value="${app.language["No"]}"><input type="button" id="ExportToCSVorTXT" class="btn btn-primary" style="float: right; margin-right:5px;" value="${app.language["Yes"]}"> </div>`)
                    }
                }).catch((data) => {

                });
            } else {
                showAjaxReturnMessage(rdata.Msg, "e");
            }

        });
    }
    ExportCsvBuildObject(obje) {
        //header
        for (var i = 0; i < app.data.ListOfHeaders.length; i++) {
            if (i > obJgridfunc.StartInCell() - 1) {
                var head = app.data.ListOfHeaders[i];
                obje.Headers.push({ ColumnName: head.ColumnName, DataType: head.DataType, HeaderName: head.HeaderName });
            }
        }
        //rows
        var rowbuilder = [];
        for (var i = 0; i < rowselected.length; i++) {
            for (var j = 0; j < hot.countCols(); j++) {
                if (j > obJgridfunc.StartInCell() - 1) {
                    rowbuilder.push(hot.getDataAtCell(rowselected[i], j));
                }
            }
            obje.DataRows.push(rowbuilder);
            obje.ListofselectedIds.push(hot.getDataAtCell(rowselected[i], 0));//strore ids for data processing
            rowbuilder = [];
        }
    }
    ExportCsvBuildObjectGridReport(obje) {
        //header
        for (var i = 0; i < app.data.ListOfHeader.length; i++) {
            if (i > obJgridfunc.StartInCell() - 1) {
                var head = app.data.ListOfHeader[i];
                obje.Headers.push({ ColumnName: head, DataType: "", HeaderName: head });
            }
        }
        //rows
        var rowbuilder = [];

        for (var i = 0; i < hot.countRows(); i++) {
            for (var j = 0; j < hot.countCols(); j++) {
                rowbuilder.push(hot.getDataAtCell(i, j));
            }
            obje.DataRows.push(rowbuilder);
            obje.ListofselectedIds.push(hot.getDataAtCell(i, 0));//strore ids for data processing
            rowbuilder = [];
        }
    }
    ExportCSVorTXTAll(iscsv) {
        var obje = {};
        obje.viewId = app.data.ViewId;
        obje.RecordCount = rowselected.length;
        obje.viewName = app.data.ViewName;
        obje.tableName = app.data.TableName
        obje.IsCSV = iscsv;
        obje.IsSelectAllData = true;
        var data = JSON.stringify({ paramss: obje })
        var call = new DataAjaxCall(app.url.server.DialogConfirmExportCSVorTXT, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                var dlg = new DialogBoxBasic(rdata.lblTitle, rdata.HtmlMessage, app.domMap.Dialogboxbasic.Type.Content);
                dlg.ShowDialog().then((data) => {
                    if (rdata.isRequireBtn) {
                        app.globalverb.IsBackgroundProcessing = rdata.IsBackgroundProcessing;
                        $(app.domMap.Dialogboxbasic.DlgBsContent).append(`<div class="modal-footer"><input type="button" onclick="$('#BasicDialog').hide()" class="btn btn-default" style="float: right;" value="${app.language["No"]}"><input type="button" id="ExportToCSVorTXTAll" class="btn btn-primary" style="float:right;margin-right:5px;" value="${app.language["Yes"]}"> </div>`)
                    }
                }).catch((data) => {
                });
            } else {
                showAjaxReturnMessage(rdata.Msg, "e");
            }
        });
    }
    //print barcode
    GetPrintBarcodePopup() {
        if (app.globalverb.ids.length > 100) {
            showAjaxReturnMessage("You cannot generate more than 100 labels at the time", "w")
            return;
        }
        var objd = { PrintBarcode: {} }
        objd.ViewId = app.data.ViewId;
        objd.ids = app.globalverb.ids;
        objd.PrintBarcode.sortableFields = app.data.sortableFields
        var data = JSON.stringify({ paramss: objd })
        $(app.SpinningWheel.Main).show();
        var call = new DataAjaxCall(app.url.server.InitiateBarcodePopup, ajax.Type.Post, ajax.DataType.Html, data, ajax.ContentType.Utf8, "", "", "")
        call.Send().then((rdata) => {
            var dlg = new DialogBoxBasic(app.language["lblBlackWhiteHeader"], rdata, app.domMap.Dialogboxbasic.Type.Content)
            dlg.ShowDialog().then(() => {
                app.globalverb.DialogType = "barcode";
                this.GetPrintBarcodeShowPdf();
                $(app.SpinningWheel.Main).hide();
            });

            if (labelIserror) {
                dlg.CloseDialog();
                showAjaxReturnMessage(labelmsg, "e")
                $(app.SpinningWheel.Main).hide();
                return;
            }
        });
    }
    GetPrintBarcodeOnchange(isLabelDropdown) {
        var objc = {
            PrintBarcode: {
                labelFormSelectedValue: parseInt(document.querySelector(app.domMap.PrintBarcod.labelForm).selectedOptions[0].value),
                labelDesignSelectedValue: parseInt(document.querySelector(app.domMap.PrintBarcod.labelDesign).selectedOptions[0].value),
                labelOutline: document.querySelector(app.domMap.PrintBarcod.labelOutline).checked,
                strtPrinting: document.querySelector(app.domMap.PrintBarcod.strtPrinting).value,
                labelIndex: parseInt(document.querySelector(app.domMap.PrintBarcod.labelDesign).selectedOptions[0].index),
                isLabelDropdown: isLabelDropdown,
                sortableFields: app.data.sortableFields
            },
            ids: app.globalverb.ids
        }

        app.globalverb.GLabelDesignId = objc.PrintBarcode.labelDesignSelectedValue;

        $(app.SpinningWheel.Main).show();
        var data = JSON.stringify({ paramss: objc });
        return new Promise((resolve, reject) => {
            var call = new DataAjaxCall(app.url.server.GenerateBarcodeOnchange, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
            call.Send().then((rdata) => {
                if (rdata.isError === false) {
                    labelFileDynamicpath = rdata.labelFileDynamicpath
                    document.getElementById("btnbcodeDownload").href = `/PrintBarcode/Downloadbarcodefile?labelUniqueName=${rdata.labelFileName}&labelId=${app.globalverb.GLabelDesignId}&ids=${app.globalverb.ids}`
                    this.GetPrintBarcodeShowPdf()
                    resolve(rdata.formSelectionid)
                } else {
                    showAjaxReturnMessage("something went wrong!", "e")
                    resolve(false)
                }
                $(app.SpinningWheel.Main).hide();
            });
        });
    }
    GetPrintBarcodeShowPdf() {
        //var lbl = labelName.toUpperCase();
        //if (lbl.indexOf('.XPS') != -1) {
        //    $('.printPopup').addClass('ChangePrintWidth');
        //    $('.printleftpanel').removeClass('col-md-6');
        //    $('.printleftpanel').addClass('col-md-12');
        //    $('.previewDataCheckbox').hide();
        //    $('#previewContainer').remove();
        //    return false;
        //}
        //var url = './LabelData/' + labelName;

        PDFJS.workerSrc = './Content/themes/TAB/js/pdf.worker.js';
        setTimeout(() => {
            PDFJS.getDocument(labelFileDynamicpath).then(function getPdfHelloWorld(pdf) {
                pdf.getPage(1).then(function getPageHelloWorld(page) {
                    var desiredWidth = 350;
                    var desireHeight = 400;
                    var viewport = page.getViewport(1);
                    var scaleWidth = desiredWidth / viewport.width;
                    var scaleHeight = desireHeight / viewport.height;
                    var scale = (scaleHeight + scaleWidth) / 2;
                    var scaledViewport = page.getViewport(scale);

                    var canvas = document.getElementById('printPreview');
                    var context = canvas.getContext('2d');

                    canvas.height = scaledViewport.height;
                    canvas.width = scaledViewport.width;

                    $(app.domMap.PrintBarcod.previewContainer).width(canvas.width).height(canvas.height);

                    var renderContext = {
                        canvasContext: context,
                        viewport: scaledViewport
                    };
                    page.render(renderContext);
                });
            });
        }, 1000)
    }
    //requester
    GetRequesterPopup() {
        $(app.SpinningWheel.Main).show();
        var objc = { Request: {} };
        objc.ViewId = app.data.ViewId;
        objc.TableName = app.data.TableName;
        objc.ids = GetrowsKeyids();
        objc.ViewName = app.data.ViewName;
        objc.Request.TextFilter = "";

        var data = JSON.stringify({ paramss: objc })
        var call = new DataAjaxCall(app.url.server.GetRequsterpopup, ajax.Type.Post, ajax.DataType.Html, data, ajax.ContentType.Utf8, "", "", "")
        call.Send().then((rdata) => {
            var dlg = new DialogBoxBasic("Request", rdata, app.domMap.Dialogboxbasic.Type.Content);
            dlg.ShowDialog().then(() => {
                $(app.SpinningWheel.Main).hide();
                document.querySelector(app.domMap.Request.txtreqDue).value = SetCurrentDate();
            });
        });
    }
    //move record
    GetMovePopup() {
        $(app.SpinningWheel.Main).show();
        var objc = {};
        objc.TableName = app.data.TableName;
        objc.ViewId = app.data.ViewId
        objc.HasAttachment = app.data.HasAttachmentcolumn
        objc.ids = GetrowsKeyids();
        var data = JSON.stringify({ paramss: objc })
        var call = new DataAjaxCall(app.url.server.GetMovePopup, ajax.Type.Post, ajax.DataType.Html, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {

            if (rdata.isError) {
                showAjaxReturnMessage(rdata.Msg, "e")
                $(app.SpinningWheel.Main).hide();
                return;
            }

            var dlg = new DialogBoxBasic(app.language["Move"], rdata, app.domMap.Dialogboxbasic.Type.Content);
            dlg.ShowDialog().then(() => {
                $(app.SpinningWheel.Main).hide();
            });
        });
    }
    //transfer selected
    StartTransferItems() {
        $(app.SpinningWheel.Main).show();
        var objc = { Transfer: {} };
        objc.ViewId = app.data.ViewId;
        objc.TableName = app.data.TableName
        objc.ids = GetrowsKeyids();
        objc.ViewName = app.data.ViewName
        objc.Transfer.IsSelectAllData = app.globalverb.GIsTransferAllData
        objc.RecordCount = app.globalverb.GTransferRowcounter;

        var data = JSON.stringify({ paramss: objc });
        var call = new DataAjaxCall(app.url.server.StartTransfering, ajax.Type.Post, ajax.DataType.Html, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            var html = $(rdata);
            var transferType = html[6].value
            this.BeforeTransfer(transferType, rdata)
            $(app.SpinningWheel.Main).hide();
        });
    }
    BeforeTransfer(transferType, transferhtml) {
        var htmlMsg = "";
        app.globalverb.GTransferHtmlHolder = transferhtml;
        switch (transferType) {
            case "Normal":
                this.GetTransferPopup();
                break;
            case "background":
                var msg = app.language["lblDataTABTransferBackground"].replace("{0}", app.globalverb.GTransferRowcounter).replace("{1}", "FusionRMS").split(".")
                htmlMsg = `<p>${msg[0]}</p><p>${msg[1]}</p>`
                var dlg = new DialogBoxBasic(app.language["lblTitleTransferBG"], htmlMsg, app.domMap.Dialogboxbasic.Type.Content)
                dlg.ShowDialog().then(() => {
                    $(app.domMap.Dialogboxbasic.DlgBsContent).append(`<div class="modal-footer"><div class="col-md-12 text-right"><input type="button" class="btn btn-primary" id="ApproveTransfer" value="${app.language["Yes"]}"><input type="button" onclick="$('#BasicDialog').hide()" class="btn btn-default" value="${app.language["No"]}"></div></div>`)
                });
                break;
            case "exceededmaxlimit":
                htmlMsg = `<p>${app.language["msgFnMaxLimitExceeded"]}</p>`
                var dlg = new DialogBoxBasic(app.language["lblMSGWarningMessage"], htmlMsg, app.domMap.Dialogboxbasic.Type.Content)
                dlg.ShowDialog().then(() => { });
                break;
            case "serviceisnotenable":
                htmlMsg = `<p>${app.language["msgFnServiceNotEnabled"]}</p>`;
                var dlg = new DialogBoxBasic(app.language["lblMSGWarningMessage"], htmlMsg, app.domMap.Dialogboxbasic.Type.Content)
                dlg.ShowDialog().then(() => { });
                break;
            default:
        }
    }
    GetTransferPopup() {
        var dlg = new DialogBoxBasic("Transfer Items", app.globalverb.GTransferHtmlHolder, app.domMap.Dialogboxbasic.Type.Content);
        dlg.ShowDialog().then(() => {
            $(app.SpinningWheel.Main).hide();
        });
    }
    GetTransferItemContainers(objc) {
        //reset object before change container
        app.globalverb.GTransferToitem.isDueBack = "";
        app.globalverb.GTransferToitem.tableName = "";
        app.globalverb.GTransferToitem.Tableid = "-1";

        var data = JSON.stringify({ paramss: objc })
        var call = new DataAjaxCall(app.url.server.GetTransferItems, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            $(app.domMap.Transfer.trlDestinationItemsContainer).html("");
            if (rdata.isDueBack) {
                document.querySelector(app.domMap.Transfer.DivDueBack).style.display = "block"
                app.globalverb.GTransferToitem.isDueBack = true;
            } else {
                document.querySelector(app.domMap.Transfer.DivDueBack).style.display = "none"
                app.globalverb.GTransferToitem.isDueBack = false;
            }
            if (rdata.trDestinationsItem.length > 0) {
                for (var i = 0; i < rdata.trDestinationsItem.length; i++) {
                    var text = rdata.trDestinationsItem[i].text;
                    var value = rdata.trDestinationsItem[i].value;
                    var viewid = rdata.trDestinationsItem[i].ContainerViewid;
                    var tableName = rdata.trDestinationsItem[i].ContainerTableName
                    $(app.domMap.Transfer.trlDestinationItemsContainer).append(`<tr><td><input type="radio" name="trDestinationsItem" data-tablename="${tableName}" value="${value}"><span name="trDestinationsItem" style="cursor:pointer;font-size:14px">${text}</span></td></tr>`)
                }
            }
        });
    }
    ExportCSVorTXTCurrentGridReport(iscsv) {
        var obje = {};
        obje.viewId = 0;
        obje.RecordCount = hot.countRows();//rowselected.length;
        obje.viewName = obJtoolbarmenufunc.GetKeyByValue(app.Enums.Crumbs, obJreportsrecord.reportType);
        obje.tableName = obJtoolbarmenufunc.GetKeyByValue(app.Enums.Crumbs, obJreportsrecord.reportType);
        obje.IsCSV = iscsv;
        obje.IsSelectAllData = false;
        obje.ListofselectedIds = [];
        obje.DataRows = [];
        obje.Headers = [];
        obje.ReportType = ""
        //obje.ReportAuditFilterProperties = repapp.auditObj;
        this.ExportCsvBuildObjectGridReport(obje)
        var data = JSON.stringify({ paramss: obje })
        var call = new DataAjaxCall(app.url.server.DialogConfirmExportCSVorTXTGridReport, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.isError === false) {
                var dlg = new DialogBoxBasic(rdata.lblTitle, rdata.HtmlMessage, app.domMap.Dialogboxbasic.Type.Content)
                dlg.ShowDialog().then((data) => {
                    if (rdata.isRequireBtn) {
                        app.globalverb.IsBackgroundProcessing = rdata.IsBackgroundProcessing;
                        if (rdata.Permission === false) {
                            $(app.domMap.Dialogboxbasic.DlgBsContent).append(`<div class="modal-footer"><input type="button" onclick="$('#BasicDialog').hide()" class="btn btn-danger" value="${app.language["Cancel"]}"> </div>`)
                        } else {
                            $(app.domMap.Dialogboxbasic.DlgBsContent).append(`<div class="modal-footer"><input type="button" onclick="$('#BasicDialog').hide()" class="btn btn-default"style="float:right;" value="${app.language["No"]}"><input type="button" id="ExportToCSVorTXTGridReport" class="btn btn-primary" style="float:right; margin-right:5px;"value="${app.language["Yes"]}"> </div>`)
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

//REPORTS
class ReportsRecord {
    constructor() {
        this.title = "";
        this.text = "";
        this.isCustomeReportCall = false;
        this.obj = {}
    }
    //retention report
    AuditHistoryRow() {
        if (hot.getDataAtRow(rowselected)[0] == null || hot.getDataAtRow(rowselected)[0] == '') {
            return false;
        }
        var _this = this;
        _this.reportType = app.Enums.Crumbs.AuditHistoryPerRow;
        _this.printTitle = "Audit History Report";
        _this.reportTitle = "Audit History Report";
        this.GenerateRowReport();
    }
    TrackingHistoryRow() {
        if (hot.getDataAtRow(rowselected)[0] == null || hot.getDataAtRow(rowselected)[0] == '') {
            return false;
        }
        var _this = this;
        _this.reportType = app.Enums.Crumbs.TrackingHistoryPerRow;
        _this.printTitle = "Tracking History Report";
        _this.reportTitle = "Tracking History Report";
        this.GenerateRowReport();
    }
    ContentsPerRow() {
        if (hot.getDataAtRow(rowselected)[0] == null || hot.getDataAtRow(rowselected)[0] == '') {
            return false;
        }
        var _this = this;
        _this.reportType = app.Enums.Crumbs.ContentsPerRow;
        _this.printTitle = app.data.TableName + " " + "Content Report";
        _this.reportTitle = app.data.TableName + " " + "Content Report";
        this.GenerateRowReport();
    }
    GenerateRowReport() {
        //this method required to pass obJreports object with params: reportType, printTitle, reportTitle (pass as an object)
        $(app.SpinningWheel.Main).show();
        var rowSelectedData = hot.getDataAtRow(rowselected)[0];
        //build object data for server params
        var objdata = {}
        objdata.Tableid = hot.getDataAtRow(rowselected)[0];
        objdata.tableName = app.data.TableName;
        objdata.viewId = obJgridfunc.ViewId;
        objdata.reportNum = obJreportsrecord.reportType;
        objdata.pageNumber = 1;
        auditPagingfunc.SelectedIndex = objdata.pageNumber
        this.obj = objdata

        var data = JSON.stringify({ paramss: objdata })
        //call server
        var call = new DataAjaxCall(app.url.server.Reporting, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.hasPermission === false) {
                showAjaxReturnMessage(rdata.Msg, "w");
                $(app.SpinningWheel.Main).hide();
                return
            }
            //breadscrumb call
            bc.children[bc.children.length - 1].remove()
            app.data.ItemDescription = rdata.ItemDescription;
            obJbreadcrumb.CreateHtmlCrumbLink(rowSelectedData, obJdrildownclick.ChildKeyField, obJreportsrecord.ChildKeyType, obJgridfunc.ViewId, obJgridfunc.ViewName, buildQuery);
            obJbreadcrumb.CreateCrumbHeader(obJdrildownclick.childViewName, obJreportsrecord.reportType);
            //load reporting partial view with data
            obJreportsrecord.text = rowSelectedData;
            $("#mainContainer").load(app.url.Html.Reporting, () => {
                document.querySelector(app.domMap.Reporting.ReportTitle).innerHTML = obJreportsrecord.reportTitle;
                document.querySelector(app.domMap.Reporting.itemDescription).innerHTML = rdata.ItemDescription;
                document.querySelector(app.domMap.Reporting.PrintReport).addEventListener("click", () => {
                    obJreportsrecord.PrintReport();
                })
                app.data = rdata;
                HandsOnTableReports.buildHandsonTable(rdata);
                // Load Pagination
                obJreportsrecord.GenerateRowReportCount(data);
            });
            $(app.SpinningWheel.Main).hide();
        });
    }
    PrintReport(model) {
        var _this = this;
        $(app.SpinningWheel.Main).show();
        var Databuilder = {};
        var printData = [];
        var RowsData = hot.getData();
        var Header = hot.getColHeader();

        for (var i = 0; i < RowsData.length; i++) {
            for (var j = 0; j < Header.length; j++) {
                Databuilder[Header[j]] = RowsData[i][j];
            }
            printData.push(Databuilder);
            Databuilder = {};
        }

        printJS({
            printable: printData,
            properties: Header,
            header: "Record count: " + hot.getData().length + " - Id: " + _this.text,
            headerStyle: "font-weight: 50px; font-size:20px",
            documentTitle: _this.printTitle,
            gridHeaderStyle: "font-size: 22px;font-weight: bold; border: 1px solid black;",
            gridStyle: "font-size: 20px; border: 1px solid black",
            type: 'json'
        });
        $(app.SpinningWheel.Main).hide();
    }
    GenerateRowReportCount(data) {
        var callp = new DataAjaxCall(app.url.server.ReportingCount, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        callp.Send().then((dt) => {
            auditPagingfunc.SelectedIndex = 1
            auditPagingfunc.TotalRecords = dt.TotalRecord;
            auditPagingfunc.PageSize = dt.TotalPage;
            auditPagingfunc.PerPageRecords = dt.PerPageRecord
            auditPagingfunc.VisibleIndexs();
            auditPagingfunc.AddCommaToTotalRecords();
            auditPagingfunc.SetTextFieldValues();
            $("#spanTotalRecords").show();
            $("#spinTotal").hide();
        })
    }
    GenerateRowReportUpdateData() {
        //this method required to pass obJreports object with params: reportType, printTitle, reportTitle (pass as an object)
        $(app.SpinningWheel.Main).show();
        // this obj value are set when first time upload page
        this.obj.pageNumber = auditPagingfunc.SelectedIndex;

        var data = JSON.stringify({ paramss: this.obj })
        //call server
        var call = new DataAjaxCall(app.url.server.Reporting, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((rdata) => {
            if (rdata.hasPermission === false) {
                showAjaxReturnMessage(rdata.Msg, "w");
                return
            }
            HandsOnTableReports.buildHandsonTable(rdata);
            $(app.SpinningWheel.Main).hide();
        });
    }
}
class RetentionInfo {
    constructor() {
        this.rowNumber = "";
        this.isEditMode = false;
    }
    GetInfo() {
        var url = app.url.server.GetRetentionInfo + `?id=${hot.getDataAtRow(rowselected[0])[0]}&viewId=${obJgridfunc.ViewId}`;
        var call = new DialogBoxBasic(app.language["tiRetentionInfoRetenInfo"], url, app.domMap.Dialogboxbasic.Type.PartialView);
        call.ShowDialog().then(() => {
            HandsOnTableRetentionRowInfo.buildHandsonTable(retinfodata)
        });
    }
    DropdownReturnAfterChange(model, elem) {
        elem.description.innerText = model.RetentionDescription
        //item
        elem.retentionItem.innerText = model.RetentionItem
        //status
        elem.status.innerText = model.RetentionStatus.text
        model.RetentionStatus.color == "red" ? elem.status.style.color = "red" : elem.status.style.color = "black";
        //inactivedate
        elem.inArchiveDate.innerText = model.RetentionInfoInactivityDate.text;
        model.RetentionInfoInactivityDate.color == "red" ? elem.inArchiveDate.style.color = "red" : elem.inArchiveDate.style.color = "black";
        //archive
        elem.lblArchive.innerText = model.lblRetentionArchive;
        elem.retArchive.innerText = model.RetentionArchive.text;
        model.RetentionArchive.color == "red" ? elem.retArchive.style.color = "red" : elem.retArchive.style.color = "black";
    }
    //updating
    UpdateRecordInfo() {
        var da = { props: obJretentioninfo.GatherProperties() };
        if (da.props.RetTableHolding != undefined && da.props.RetTableHolding != null) {
            for (var i = 0; i < da.props.RetTableHolding.length; i++) {
                if (da.props.RetTableHolding[i].SnoozeUntil == "") {
                    da.props.RetTableHolding[i].SnoozeUntil = null;
                }
                else {
                    da.props.RetTableHolding[i].SnoozeUntil = new Date(da.props.RetTableHolding[i].SnoozeUntil);
                }
            }
        }        
        var data = JSON.stringify(da);
        var call = new DataAjaxCall(app.url.server.RetentionInfoUpdate, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((model) => {
            if (model.isError) {
                showAjaxReturnMessage("something went wrong!", "e");
            } else {
                var d = new DialogBoxBasic();
                d.CloseDialog();
                //for dynamic use
                for (var i = 0; i < model.ReturnOnerow[0].length; i++) {
                    if (i >= obJgridfunc.StartInCell()) {
                        var data = model.ReturnOnerow[0][i];
                        var db = app.data.ListOfHeaders[i];
                        if (db.DataType !== 'DateTime' && db.Allownull === true) {
                            hot.setDataAtCell(rowselected[0], i, data)
                        }
                    }
                }
                //var rowBeforeReturn = rowselected[0];
                //obJgridfunc.LoadViewTogrid(0, pagingfun.SelectedIndex).then(() => {
                //    hot.selectCell(rowBeforeReturn, obJgridfunc.StartInCell())
                //});
            }
        });
    }
    GatherProperties() {
        var obj = {};
        obj.rowid = obJgridfunc.RowKeyid
        obj.RetentionItemText = "Record Details";
        obj.TableName = app.data.TableName;
        var retcode = document.querySelector(app.domMap.Retentioninfo.ddlRetentionCode);
        obj.RetentionItemCode = retcode.options[retcode.selectedIndex].innerText;
        obj.RetnArchive = document.querySelector(app.domMap.Retentioninfo.RetinArchive).innerText;
        obj.RetnInactivityDate = document.querySelector(app.domMap.Retentioninfo.RetinInactiveDate).innerText;
        //the retinfodata object comes from the partial view
        obj.RetTableHolding = this.GetTableRows(obj)
        obj.viewid = obJgridfunc.ViewId;
        obj.SldestructionCertid = retinfodata.SldestructionCertid
        return obj;
    }
    GetTableRows(obj) {
        var arr = [];
        if (hotRetention.getData().length === 1 && hotRetention.getData()[0][0] === "#####") {
            arr = 0
            return;
        }
        for (var i = 0; i < hotRetention.getData().length; i++) {
            var holdType = hotRetention.getData()[i][0];
            var setdata = {};
            if (holdType === "Retention") {
                setdata.RetentionHold = true;
                setdata.LegalHold = false;
            } else {
                setdata.RetentionHold = false;
                setdata.LegalHold = true;
            }
            setdata.SnoozeUntil = hotRetention.getData()[i][1];
            setdata.HoldReason = hotRetention.getData()[i][2];
            setdata.RetentionCode = obj.RetentionItemCode;
            setdata.SLDestructionCertsId = 0;
            setdata.TableId = obj.rowid;
            setdata.TableName = obj.TableName;
            arr.push(setdata);
        }
        return arr;
    }
    RemoveRows() {
        var rowselected = this.GetrowsSelected()
        for (var i = 0; i < rowselected.length; i++) {
            hotRetention.alter('remove_row', rowselected[i]);
        }
        this.OnEmptyTable();
    }
    OnEmptyTable() {
        //on empty table create one row; !!Handsonetale will not show header if there is no rows; 
        var btnremove = document.querySelector(app.domMap.Retentioninfo.btnRemoveRetin);
        var btnedit = document.querySelector(app.domMap.Retentioninfo.btnEditRetin);
        if (hotRetention.getData().length === 0) {
            var data = {};
            data.ListOfHeader = [app.language["tiRetentionInformationHoldType"], app.language["btnRetentionInfoSnooze"], app.language["tiRetentionInformationReason"]]
            data.ListOfRows = [["#####", "#####", "#####"]]
            HandsOnTableRetentionRowInfo.buildHandsonTable(data)
            btnremove.disabled = true;
            btnedit.disabled = true;
        }
    }
    GetrowsSelected() {
        var rowsSelected = [];
        if (hotRetention.getSelected().length === 1) {
            var start = hotRetention.getSelected()[0][0];
            var end = hotRetention.getSelected()[0][2];
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
            var len = hotRetention.getSelected().length;
            for (var i = 0; i < len; i++) {
                var rowNumber = hotRetention.getSelected()[i][0]
                rowsSelected.push(rowNumber);
            }
        }
        return rowsSelected;
    }
    //holding
    HoldingConditions(btnType) {
        var retType = document.querySelector(app.domMap.RetentioninfoHold.chkHoldTypeRetention);
        var legType = document.querySelector(app.domMap.RetentioninfoHold.chkHoldTypeLegal);
        var reason = document.querySelector(app.domMap.RetentioninfoHold.holdReason);
        var snooz = document.querySelector(app.domMap.RetentioninfoHold.txtSnoozeDate);
        if (legType.checked || retType.checked) {
            if (btnType === 'retention') {
                legType.checked = false;
            } else {
                retType.checked = false;
            }
            reason.disabled = false;
        } else {
            reason.disabled = true;
        }
        if (legType.checked === false && retType.checked === false) {
            snooz.disabled = true;
            reason.disabled = true;
        }
    }
    StartAddingRow() {
        this.isEditMode = false;
        $(app.domMap.RetentioninfoHold.Dialog.RetentionHoldingDialog).show();
        $(app.domMap.RetentioninfoHold.Dialog.DlgHoldingTitle).html(app.language["lblRetentionInformationHoldInfo"]);
        $(app.domMap.RetentioninfoHold.Dialog.RetentionHoldingContent).load(app.url.server.RetentionInfoHolde, () => {
            //set up snooz for next month by default
            document.querySelector(app.domMap.RetentioninfoHold.txtSnoozeDate).value = SetNextMonthDate();
        })
    }
    AddnewHolding() {
        var retType = document.querySelector(app.domMap.RetentioninfoHold.chkHoldTypeRetention);
        var legType = document.querySelector(app.domMap.RetentioninfoHold.chkHoldTypeLegal);
        var snooz = document.querySelector(app.domMap.RetentioninfoHold.txtSnoozeDate);
        var reason = document.querySelector(app.domMap.RetentioninfoHold.holdReason);
        var btnremove = document.querySelector(app.domMap.Retentioninfo.btnRemoveRetin);
        var btnedit = document.querySelector(app.domMap.Retentioninfo.btnEditRetin);
        //delete empty row if there is
        if (hotRetention.getDataAtCell(0, 1) === "#####") {
            hotRetention.alter('remove_row', 0);
        }
        //add new row
        if (retType.checked || legType.checked) {
            hotRetention.alter("insert_row", 0, 1);
            if (retType.checked) {
                hotRetention.setDataAtCell(0, 0, "Retention");
            } else {
                hotRetention.setDataAtCell(0, 0, "Legal");
            }
            if (snooz.disabled === false) {
                var formatDate = snooz.value.split("-");
                var snoozDate = formatDate[1] + "/" + formatDate[2] + "/" + formatDate[0]
                if (snooz.value === "") {
                    hotRetention.setDataAtCell(0, 1, "");

                } else {
                    hotRetention.setDataAtCell(0, 1, snoozDate);

                }

            } else {
                hotRetention.setDataAtCell(0, 1, "");
            }
            hotRetention.setDataAtCell(0, 2, reason.value);
            $(app.domMap.RetentioninfoHold.Dialog.RetentionHoldingDialog).hide();
        }
        else {
            showAjaxReturnMessage(app.language["MsgRetentionInformationHoldTypeSelect"], "w")
        }

        //after add: enable remove and edit buttons in case they are disabled
        btnremove.disabled = false;
        btnedit.disabled = false;
    }
    StartEditHoldingRow() {
        this.isEditMode = true;
        this.rowNumber = this.GetrowsSelected()[0];
        var tbtype = hotRetention.getDataAtRow(this.rowNumber)[0];
        var tbSnooz = hotRetention.getDataAtRow(this.rowNumber)[1];
        var tbreason = hotRetention.getDataAtRow(this.rowNumber)[2];
        if (tbtype == "Retention") {
            document.querySelector(app.domMap.RetentioninfoHold.chkHoldTypeRetention).checked = true;
            document.querySelector(app.domMap.RetentioninfoHold.chkHoldTypeLegal).checked = false;
        } else {
            document.querySelector(app.domMap.RetentioninfoHold.chkHoldTypeRetention).checked = false;
            document.querySelector(app.domMap.RetentioninfoHold.chkHoldTypeLegal).checked = true;
        }
        //set snoz
        if (tbSnooz == "" || tbSnooz == null) {
            document.querySelector(app.domMap.RetentioninfoHold.txtSnoozeDate).value = SetNextMonthDate();
        } else {
            var year = tbSnooz.split("/")[2];
            var month = tbSnooz.split("/")[0].toString();
            if (month.length == 1) {
                month = `0${month}`
            }
            var date = tbSnooz.split("/")[1];
            var dateFormat = `${year}-${month}-${date}`
            document.querySelector(app.domMap.RetentioninfoHold.txtSnoozeDate).disabled = false;
            document.querySelector(app.domMap.RetentioninfoHold.txtSnoozeDate).value = dateFormat;
        }

        if (!tbreason == "" || !tbreason == null) {
            document.querySelector(app.domMap.RetentioninfoHold.holdReason).disabled = false;
            document.querySelector(app.domMap.RetentioninfoHold.holdReason).innerText = tbreason;
        }
    }
    EditHoldingRow() {
        var rettype = document.querySelector(app.domMap.RetentioninfoHold.chkHoldTypeRetention);
        var snooz = document.querySelector(app.domMap.RetentioninfoHold.txtSnoozeDate);
        var holdReason = document.querySelector(app.domMap.RetentioninfoHold.holdReason);
        holdReason.disabled = false;

        if (rettype.checked) {
            hotRetention.setDataAtCell(this.rowNumber, 0, "Retention");
        } else {
            hotRetention.setDataAtCell(this.rowNumber, 0, "Legal");
        }

        if (snooz.disabled) {
            hotRetention.setDataAtCell(this.rowNumber, 1, "");
        } else {
            var year = snooz.value.split("-")[0]
            var month = snooz.value.split("-")[1];
            var date = snooz.value.split("-")[2];
            var formatDate = `${month}/${date}/${year}`
            if (snooz.value === "") {
                hotRetention.setDataAtCell(this.rowNumber, 1, "");
            } else {
                hotRetention.setDataAtCell(this.rowNumber, 1, formatDate);
            }
        }

        hotRetention.setDataAtCell(this.rowNumber, 2, holdReason.value);
        $(app.domMap.RetentioninfoHold.Dialog.RetentionHoldingDialog).hide();

    }
}

// Vault Operations
class VaultFunction {
    constructor() {
        this.trackableIdMap = new Map();
    }
    LoadViewValt(docdata, obj) {
        bc.innerHTML = "";
        $(app.SpinningWheel.Main).show();
        var data = { 'docdata': docdata, ViewType: OrphanSelectedView }
        var call = new DataAjaxCall(app.url.server.OrphanPartial, ajax.Type.Get, ajax.DataType.Html, data, "", "", "", "")
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
        obJvaultfunction.trackableIdMap.clear();
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
            }
            else {
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
        var call = new DataAjaxCall(app.url.server.LoadFlyoutOrphansPartial, ajax.Type.Get, ajax.DataType.Html, data, "", "", "", "")
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
        var filter = $("#fileSearchOrphan").val().toString();
        var data = {
            'filter': filter
        }
        var call = new DataAjaxCall(app.url.server.GetCountAfterFilter, ajax.Type.Post, ajax.DataType.Json, data, "", "", "", "")
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
                $(app.SpinningWheel.Main).hide();
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
                        showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["MoveSuccessMSg"], "s")
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
                if (response.isError === false) {
                    if (response.IsDeletePermision == false) {
                        $("#spinningWheel").hide();
                        $("#modalDelete").modal("hide")
                        showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["dontpermisiondeletevault"], "w")
                    }
                    else if (response.IsAlreadyMaped == true) {
                        $("#modalDelete").modal("hide")
                        showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["alreadyAttachedWarning"], "w")
                        $("#spinningWheel").hide();
                    } else {
                        $("#modalDelete").modal("hide")
                        showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["msgAttachmentDeletedSuccessfully"], "s")
                        obJvaultfunction.ResetOrphanPagginationProperties()
                        obJvaultfunction.trackableIdMap.clear();
                    }
                } else {
                    $("#spinningWheel").hide();
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
        //var formdata2 = new FormData();
        var filesizes = '';
        formdata.append("tabName", 'Orphans');
        formdata.append("tableId", '');
        formdata.append("viewId", '');
        //formdata2 = formdata;
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
                var fsize = FlyoutUploderFiles[i].size;
                // store all the file size for checking file size permission.
                filesizes = filesizes != '' ? filesizes + ',' + fsize : fsize;
                formdata.append(FlyoutUploderFiles[i].name, FlyoutUploderFiles[i]);
            } else {
                showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["invalidOrphanFormat"], "w")
                isfilesSupported = false;
                $('#OrphanFileUploadForAddAttach').val('');
                break;
            }
        }
        if (isfilesSupported === true) {
            var objc = {};
            objc.tabName = 'Orphans'
            objc.filesizeMB = filesizes.toString();
            //formdata2.append("filesizeMB", filesizes);
            $('#OrphanFileUploadForAddAttach').val('');
            // first check file size permission then save file to server.
            obJvaultfunction.CheckAttachmentMaxSize(objc).then(function () {
                obJvaultfunction.SaveNewAttachmentOrphan(formdata);
            })
        } else {
            return;
        }
    }
    CheckAttachmentMaxSize(objcjson) {
        return new Promise(function (resolve, reject) {
            $(app.SpinningWheel.Main).show();
            var ajaxOptions = {};
            ajaxOptions.url = app.url.server.CheckAttachmentMaxSize;
            ajaxOptions.type = "POST";
            ajaxOptions.dataType = "json"
            ajaxOptions.contentType = "application/json; charset=utf-8";            
            ajaxOptions.data = JSON.stringify({ paramss: objcjson });
            obJvaultfunction.ajaxCall = $.ajax(ajaxOptions).done(function (data) {
                switch (data.checkConditions) {
                    case "success":
                        resolve(data);
                        break;
                    case "permission":
                        showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["msgDocViewerVolumePermission"], "w")
                        $(app.SpinningWheel.Main).hide();
                        reject();
                        break;
                    case "maxsize":
                        showAjaxReturnMessage(data.WarringMsg, 'w');
                        $(app.SpinningWheel.Main).hide();
                        reject();
                        break;
                    case "error":
                        showAjaxReturnMessage(data.WarringMsg, 'e');
                        $(app.SpinningWheel.Main).hide();
                        reject();
                        break;
                    default:
                        $(app.SpinningWheel.Main).hide();
                        reject();
                }
            }).fail(function (jqXHR, textStatus) {
                alert(jqXHR);
                alert(textStatus);
                $(app.SpinningWheel.Main).hide();
                reject();
            });
        })
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
        var ajaxOptions = {};
        ajaxOptions.cache = false;
        ajaxOptions.url = app.url.server.AddNewAttachment;
        //ajaxOptions.enctype = 'multipart/form-data';
        ajaxOptions.type = "POST";
        ajaxOptions.dataType = "json";
        ajaxOptions.contentType = false;
        ajaxOptions.processData = false;
        ajaxOptions.data = formdata;
        ajaxOptions.xhr = function () {
            var xhr = $.ajaxSettings.xhr();
            xhr.onprogress = function (e) {
            },
                xhr.upload.onprogress = function (e) {
                    if (e.lengthComputable) {
                        var prec = Math.floor((e.loaded / e.total) * 100);
                        var total = obJvaultfunction.CalculateFileSize(e.total);
                        var loaded = obJvaultfunction.CalculateFileSize(e.loaded);
                        var mbleft = parseInt(total - loaded);
                        obJvaultfunction.StartingProgressBar(prec, mbleft, total, e.total);
                    }
                };
            return xhr;
        };

        obJvaultfunction.ajaxCall = $.ajax(ajaxOptions).done(function (data) {
            switch (data.checkConditions) {
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
                    $(app.SpinningWheel.Main).hide();
                    break;
                case "maxsize":
                    showAjaxReturnMessage(data.WarringMsg, 'w');
                    $(app.SpinningWheel.Main).hide();
                    break;
                case "error":
                    showAjaxReturnMessage(data.WarringMsg, 'e');
                    $(app.SpinningWheel.Main).hide();
                    break;
                default:
                    $(app.SpinningWheel.Main).hide();
            }

            //$(app.SpinningWheel.Main).hide();
        }).fail(function (jqXHR, textStatus) {
            alert(jqXHR);
            alert(textStatus);
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
    StartingProgressBar(percentage, mbleft, Filetotal, bytes) {
        var _this = this;
        var Unit = obJvaultfunction.GetfileUnit(bytes);
        $("#spinningWheel").hide();
        if (Filetotal > 50 && Unit == "MB" || Unit == "GB" || Unit == "TB") {
            $("#ProgressloadingDialog").modal('show');
            const degree = percentage * 3.6;
            if (degree <= 180) {
                $(".progress-right .progress-bar").css('transform', `rotate(${degree}deg)`);
                $(".progress-left .progress-bar").css('transform', 'rotate(0deg)');
            } else {
                $(".progress-right .progress-bar").css('transform', `rotate(180deg)`);
                $(".progress-left .progress-bar").css('transform', `rotate(${180 + degree}deg)`);
            }
            //var fileSize = _this.CalculateFileSize(totalbytes)

            $("#ProgressloadingDialog").find('label').text("File: " + Filetotal + Unit + " Left: " + mbleft + Unit);
            $("#progressPercentage").text(percentage + "%");
            if (percentage == 100) {
                setTimeout(function () { $("#ProgressloadingDialog").modal('hide'); }, 100);
                percentage = 0;
            }
        }
    }
    GetfileUnit(bytes) {
        var sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
        var i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)));
        return sizes[i];
    }
    CalculateFileSize(bytes) {
        var i = parseInt(Math.floor(Math.log(bytes) / Math.log(1024)));
        return Math.round(bytes / Math.pow(1024, i), 2);
    }
}

// This constraint are using for Change grid type only for Valt page
const OrphanViewType = {
    G: "Grid",
    L: "List"
}

// set default view type for orphan View
var OrphanSelectedView = OrphanViewType.G

class DataPagingfunc extends PagingFunctions {
    constructor(SelectedIndex, PageSize, TotalRecords, PerPageRecords) {
        super(SelectedIndex, PageSize, TotalRecords, PerPageRecords)
    }
    // override method
    // this function are using for load handsontable data
    LoadRecord() {
        obJgridfunc.pageNum = this.SelectedIndex;
        obJgridfunc.GetDataPaging()
    }
}

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
class AuditPagingfunc extends PagingFunctions {
    constructor(SelectedIndex, PageSize, TotalRecords, PerPageRecords) {
        super(SelectedIndex, PageSize, TotalRecords, PerPageRecords)
    }
    // override method
    // this function are using for load data
    LoadRecord() {
        obJreportsrecord.pageNumber = auditPagingfunc.SelectedIndex
        obJreportsrecord.GenerateRowReportUpdateData()
    }
}

//FIRST OBJECT CREATED ON THE FIRST RUN.
const obJgridfunc = new GridFunc();
const obJfavorite = new Favorite();
const obJquerywindow = new QueryWindow();
var obJlinkscript = new Linkscript();
var obJmyquery = new MyQuery();
const obJglobalsearch = new GlobalSearch();
const obJattachmentsview = new AttachmentsView();
const obJtoolbarmenufunc = new ToolBarMenuFunc();
const obJretentioninfo = new RetentionInfo();
var obJbreadcrumb = new BreadCrumb();
var obJdrildownclick = new DrilldownClick();
var obJreportsrecord = new ReportsRecord();
var obJvaultfunction = new VaultFunction();
var taskbar = new TaskBar();
var pagingfun = new DataPagingfunc(0, 0, 0, 0);
var vaultPagingfunc = new VaultPagingfunc(0, 0, 0, 0);
var auditPagingfunc = new AuditPagingfunc(0, 0, 0, 0);
var objRowtrackable = new Rowtrackable();
var obJleftmenu = new LeftMenu();
var obJaddnewrecord = new AddNewRecord();
var obJlastquery = new LastQuery();







