//HANDSONTABLS OBJECT, EVENTS AND FUNCTIONS 
//handsontable review object
let pagingModel = {};
var HandsOnTableViews = {
    container: "",
    hotSettings: "",
    change: "",
    colWidthArray: [],
    buildHandsonTable: function (data) {
        var _this = this;
        this.typeColumnEvent = "";
        this.container = document.querySelector(app.domMap.DataGrid.HandsOnTableContainer);
        this.container.style.display = "block";
        this.hotSettings = {
            data: data.ListOfDatarows,
            copyPasteEnabled: true,
            colWidths: function (i) {
                if (i > obJgridfunc.StartInCell() - 1) {
                    return 200;
                }
            },
            maxCols: app.data.ListOfHeaders.length,
            beforeColumnMove: function (columnsMoving, target) {
                if (app.data.HasDrillDowncolumn && target === 1) {
                    return false;
                }
                if (app.data.HasAttachmentcolumn && target === 2) {
                    return false;
                }
            },
            outsideClickDeselects: false,
            disableVisualSelection: false,
            sortIndicator: true,
            columnSorting: true,
            colHeaders: function (col) {
                var checkboxUrl;
                var drilldown;
                var attach;
                //check 3 first columns for drilldown, attachment
                switch (col) {
                    case 0:
                        return "pkay";
                    case 1:
                        if (data.HasDrillDowncolumn) {
                            var drill = "<span class='fa-stack theme_color' title='" + app.language["dataGridMenu"] + "'><i class='fa fa-circle-thin fa-stack-2x'></i><i class='fa fa-list fa-stack-1x' style='top:8px;'></i></span>";
                            return drill;
                        } else if (data.HasDrillDowncolumn == false && data.HasAttachmentcolumn == true) {
                            drilldown = '<i class="fa fa-paperclip fa-flip-horizontal fa-2x theme_color"></i>';
                            return drilldown;
                        }
                        break;
                    case 2:
                        if (data.HasAttachmentcolumn && data.HasDrillDowncolumn) {
                            attach = '<i class="fa fa-paperclip fa-flip-horizontal fa-2x theme_color"></i>';
                            return attach;
                        }
                        break;
                    default:
                }

                //loop through dynamic header
                var headers = data.ListOfHeaders;
                for (var i = 0; i < headers.length; i++) {
                    if (col === i) {
                        if (headers[i].isEditable === false || headers[i].IsPrimarykey) {
                            return "<div style='display:inline-flex'><span>" + headers[i].HeaderName + "</span> " + '<i class="fa fa-lock formLock" style="color:#002949;font-weight:600;margin-top:-9.9px; margin-left:3px"></i></div>';
                        } else {
                            return headers[i].HeaderName;
                        }
                    }
                }
            },
            stretchH: 'all',
            fillHandle: false,
            manualColumnResize: true,
            renderAllRows: false,
            comments: true,
            contextMenu: ['copy'],//['copy', 'freeze_column'],
            autoRowSize: false,
            autoWrapCol: false,
            autoWrapRow: false,
            manualRowMove: true,
            manualColumnMove: true,
            licenseKey: "a1fe9-3411a-c6714-e4504-e7930",
            selectionMode: 'multiple',           
            currentRowClassName: 'currentRow',
            allowInvalid: true,
            
            cells: function (row, col, prop) {
                var CurrentYear = new Date()
                var cellProperties = {};
                cellProperties.allowInvalid = true;
                cellProperties.invalidCellClassName = '';
                var datalist = data.ListOfHeaders;
                for (var i = 0; i < datalist.length; i++) {
                    //check if checkbox || drilldown || attachment
                    if (col === datalist[i].columnOrder) {
                        if (datalist[i].DataType === "none" && datalist[i].HeaderName === "pkey") {
                            cellProperties.type = "text";
                            cellProperties.copyable = false;
                        } else if (datalist[i].DataType === "none" && datalist[i].HeaderName === "drilldown") {
                            cellProperties.renderer = _this.RowRenderDrilldown;
                            cellProperties.readOnly = 'true';
                            cellProperties.copyable = false;
                        } else if (datalist[i].DataType === "none" && datalist[i].HeaderName === "attachment") {
                            cellProperties.renderer = _this.RowRendererattachment;
                            cellProperties.readOnly = 'true';
                            cellProperties.copyable = false;
                        } else {
                            //return cells data to the grid.
                            switch (datalist[i].DataTypeFullName) {
                                case "System.String":
                                    if (datalist[i].isEditable === false || datalist[i].IsPrimarykey) cellProperties.readOnly = 'true';
                                    if (datalist[i].isDropdown) {
                                        cellProperties.type = "dropdown";
                                        cellProperties.skipColumnOnPaste = true;
                                    } else {
                                        cellProperties.type = "text";
                                        cellProperties.placeholder = datalist[i].editMask;
                                        cellProperties.renderer = _this.RowTexttruncated;
                                    }
                                    if (cellProperties.readOnly === 'true') {
                                        cellProperties.allowEmpty = true;
                                    } else {
                                        cellProperties.allowEmpty = datalist[i].Allownull;
                                    }
                                    break;
                                case "System.Int64":
                                    if (datalist[i].isEditable === false || datalist[i].IsPrimarykey) cellProperties.readOnly = 'true';
                                    if (datalist[i].isDropdown) {
                                        cellProperties.type = "dropdown";
                                        cellProperties.skipColumnOnPaste = true;
                                        //cellProperties.source = [];//fill up on mouse over
                                    } else {
                                        cellProperties.type = "numeric";
                                        // cellProperties.placeholder = datalist[i].editMask;
                                    }
                                    if (cellProperties.readOnly === 'true') {
                                        cellProperties.allowEmpty = true;
                                    } else {
                                        cellProperties.allowEmpty = datalist[i].Allownull;
                                    }
                                    break;
                                case "System.Int32":
                                    if (datalist[i].isEditable === false || datalist[i].IsPrimarykey) cellProperties.readOnly = 'true';
                                    if (datalist[i].isDropdown) {
                                        cellProperties.type = "dropdown";
                                        cellProperties.skipColumnOnPaste = true;
                                        //cellProperties.source = [];//fill up on mouse over
                                    } else {
                                        cellProperties.type = "numeric";
                                        //cellProperties.placeholder = datalist[i].editMask;
                                    }
                                    if (cellProperties.readOnly === 'true') {
                                        cellProperties.allowEmpty = true;
                                    } else {
                                        cellProperties.allowEmpty = datalist[i].Allownull;
                                    }
                                    break;
                                case "System.Int16":
                                    if (datalist[i].isEditable === false || datalist[i].IsPrimarykey) cellProperties.readOnly = 'true';
                                    if (datalist[i].isDropdown) {
                                        cellProperties.type = "dropdown";
                                        cellProperties.skipColumnOnPaste = true;
                                    } else {
                                        cellProperties.type = "numeric";
                                        //cellProperties.placeholder = datalist[i].editMask;
                                    }
                                    if (cellProperties.readOnly === 'true') {
                                        cellProperties.allowEmpty = true;
                                    } else {
                                        cellProperties.allowEmpty = datalist[i].Allownull;
                                    }
                                    break;
                                case "System.Boolean":
                                    //cellProperties.allowEmpty = datalist[i].Allownull;
                                    if (datalist[i].isEditable === false || datalist[i].IsPrimarykey) cellProperties.readOnly = 'true';
                                    cellProperties.type = "checkbox";
                                    cellProperties.skipColumnOnPaste = true;
                                    cellProperties.className = 'htCenter';
                                    cellProperties.maxLength = datalist[i].MaxLength
                                    break;
                                case "System.Double":
                                    if (datalist[i].isEditable === false || datalist[i].IsPrimarykey) cellProperties.readOnly = 'true';
                                    if (datalist[i].isDropdown) {
                                        cellProperties.type = "dropdown";
                                        cellProperties.skipColumnOnPaste = true;
                                    } else {
                                        cellProperties.type = "numeric";
                                        //cellProperties.placeholder = datalist[i].editMask;
                                    }
                                    if (cellProperties.readOnly === 'true') {
                                        cellProperties.allowEmpty = true;
                                    } else {
                                        cellProperties.allowEmpty = datalist[i].Allownull;
                                    }
                                    break;
                                case "System.DateTime":
                                    if (datalist[i].isEditable === false || datalist[i].IsPrimarykey) cellProperties.readOnly = 'true';
                                    cellProperties.type = "date";
                                    cellProperties.dateFormat = app.data.dateFormat;
                                    cellProperties.placeholder = app.data.dateFormat;
                                    cellProperties.validator = (value, callback) => {
                                        callback(true);
                                    }
                                    const minYear = CurrentYear.getFullYear() - 200;
                                    const maxYear = CurrentYear.getFullYear();
                                    cellProperties.renderer = _this.getCalendar;
                                    cellProperties.datePickerConfig = {
                                        yearRange: [minYear, maxYear],
                                    }
                                    if (cellProperties.readOnly === 'true') {
                                        cellProperties.allowEmpty = true;
                                    } else {
                                        cellProperties.allowEmpty = datalist[i].Allownull;
                                    }
                                    break;
                                default:
                            }
                        }
                    }
                }

                return cellProperties;
            }
        };
        hot = new Handsontable(this.container, this.hotSettings);
        hot.addHook('beforeChange', function (changes, source) {
            var change = {
                row: changes[0][0],
                cell: changes[0][1],
                Bchange: changes[0][2],
                Achange: changes[0][3],
            }
            if (source !== "newrecord") {
                if (obJgridfunc.CheckEditRowConditions(change) === false) {
                    hot.selectCell(change.row, change.cell);
                    changes[0][3] = change.Bchange;
                    //hot.setDataAtCell(change.row, change.cell, change.Bchange)
                }
            }
        });
        hot.addHook('afterChange', function (changes, source) {
            _this.change = {
                row: changes[0][0],
                cell: changes[0][1],
                Bchange: changes[0][2],
                Achange: changes[0][3],
            }
           
            if (changes[0][3] == null) {
                hot.setDataAtCell(_this.change.row, _this.change.cell, "");
            }
            if (source === "newrecord") return;
            if (app.data.ListOfHeaders[_this.change.cell].isEditable !== false && !app.data.ListOfHeaders[_this.change.cell].IsPrimarykey) {
                var columnName = app.data.ListOfHeaders[_this.change.cell].ColumnName
                var DataTypeFullName = app.data.ListOfHeaders[_this.change.cell].DataTypeFullName
                if (_this.change.Bchange != _this.change.Achange && _this.change.cell !== 0) {
                    var found = false;
                    var afterChange = _this.change.Achange !== null ? _this.change.Achange : "";
                    var beforeChange = _this.change.Bchange !== null ? _this.change.Bchange : "";

                    //get the value from dropdown if the field is dropdown
                    if (app.data.ListOfHeaders[_this.change.cell].isDropdown) {
                        afterChange = _this.GetDropDownValue(_this.change.cell, afterChange)
                    }
                    for (var i = 0; i < obJgridfunc.BuildRowFromCells.length; i++) {
                        if (obJgridfunc.BuildRowFromCells[i].columnName === columnName) {
                            found = true;
                            obJgridfunc.AfterChange[i] = { columnName: columnName, value: columnName + ": " + afterChange };
                            obJgridfunc.BuildRowFromCells[i] = { value: afterChange, columnName: columnName, DataTypeFullName: DataTypeFullName };
                            //obJgridfunc.BeforeChange.splice(i, 1);
                            break;
                        }
                    }

                    if (found === false) {
                        obJgridfunc.BuildRowFromCells.push({ value: afterChange, columnName: columnName, DataTypeFullName: DataTypeFullName });
                        obJgridfunc.BeforeChange.push({ columnName: columnName, value: columnName + ": " + beforeChange });
                        obJgridfunc.AfterChange.push({ columnName: columnName, value: columnName + ": " + afterChange })
                    }
                }
            }
        });
        hot.addHook('beforePaste', function (data, coords) {
            if (coords[0].startRow !== coords[0].endRow) {
                showAjaxReturnMessage(app.language["msgYouCantpasteInMultipleCells"], "e");
                return false;
            }
        });
        hot.addHook('beforeCopy', function (data, coords) {
            if (coords[0].startRow !== coords[0].endRow) {
                //showAjaxReturnMessage("You can't copy cells in multiple rows (needs to add the language file)", "e");
                showAjaxReturnMessage(app.language["msgCannotCopyMultipleRows"], "e");
                return false;
            }
        });
        hot.addHook('beforeOnCellMouseDown', function (event, coords, TD, blockCalculations) {
            //check requirments fields and control keys.
            //obJgridfunc.GridControlKeys(event, coords);
            //if (obJgridfunc.isConditionPass === false) {
            //    event.stopImmediatePropagation();
            //    return;
            //}
            //block
            //var colorder = app.data.ListOfHeaders.find(p => p.IsPrimarykey == true).columnOrder;
            //if (coords.col == colorder) {
            //    blockCalculations.cells = true;
            //    blockCalculations.column = true;
            //}

            //don't show blue mark on the cells when  columns are - drildown || attachments
            if (app.data.HasAttachmentcolumn == true && app.data.HasDrillDowncolumn == true) {
                if (coords.col <= 2) {
                    blockCalculations.cell = true;
                    blockCalculations.column = true;
                    hot.selectCell(coords.row, 3)
                }
            } else if (app.data.HasAttachmentcolumn == true && app.data.HasDrillDowncolumn == false) {
                if (coords.col <= 1) {
                    blockCalculations.cell = true;
                    blockCalculations.column = true;
                    hot.selectCell(coords.row, 2)
                }
            } else if (app.data.HasAttachmentcolumn == false && app.data.HasDrillDowncolumn == true) {
                if (coords.col <= 1) {
                    blockCalculations.cell = true;
                    blockCalculations.column = true;
                    hot.selectCell(coords.row, 2)
                }
            } else {
                if (coords.col <= 0) {
                    blockCalculations.cell = true;
                    blockCalculations.column = true;
                }
            }

        });
        hot.addHook('beforeKeyDown', function (event) {
            //check requirments fields and control keys.
            /* obJgridfunc.GridControlKeys(event);*/
            var selected = this.getSelected()[0];
            var row = selected[0];
            var col = selected[1];
            var endrow = selected[2];

            //key control grid
            _this.GridKeysControll(row, endrow, event, col);


            var cellProp = this.getCellMeta(row, col);

            //cells conditions and format
            switch (cellProp.type) {
                case "numeric":
                    var regNumeric = new RegExp(/[0-9]|\./);
                    if (_this.AllowKeysIntegerCell(event) === false) {
                        if (!regNumeric.test(event.key)) {
                            event.returnValue = false;
                        }
                    }
                    break;
                case "text":
                    break;
                case "date":
                    break;
                case "dropdown":
                    event.returnValue = false;
                    break;
                default:
            }
        });
        hot.addHook('afterSelection', function (row, column, row2, column2, preventScrolling, selectionLayerLevel) {
            //concept to get dropdown item list from server
            if (row === -1) return; //condition for sorting issue
            var itemsList = [];
            itemsList.push("");
            var cellProp = this.getCellMeta(row, column);
            if (cellProp.type == "dropdown") {
                data.ListOfdropdownColumns.forEach(function (prop, i) {
                    if (column == prop.colorder) {
                        var item = prop.display.split(",");
                        item.splice(-1, 1);
                        item.forEach(function (item, i) {
                            itemsList.push(item);
                        });
                    }
                });
                cellProp.source = itemsList;
            }
        });
        hot.addHook('afterSelectionEnd', function (rowstart, colstart, rowend, colend) {
            //build row after selection end - cover edit and add new row
            /* if (obJgridfunc.isConditionPass) {*/
            obJgridfunc.BuildOneRowBeforeSave(rowstart, colstart).then((isdone) => {
                if (isdone) {
                    obJgridfunc.AfterChange = [];
                    obJgridfunc.BeforeChange = [];
                    obJgridfunc.LastRowSelected = rowstart;
                    obJgridfunc.BuildRowFromCells = [];
                }
            });
            //} else if (obJgridfunc.isConditionPass === false) {

            //}

            //build rows array for later use.
            if (hot.getSelected().length === 1) {
                //if get selected method comes with one row it means it is starting new row
                //so in this case I clear the array and start all over again.
                rowselected = [];
            }
            if (rowstart < rowend) {
                for (var i = rowstart; i < rowend + 1; i++) {
                    if (!obJgridfunc.CheckForRowDuplication(i)) {
                        rowselected.push(i);
                    }

                }
            } else if (rowstart > rowend) {
                for (var j = rowend; j < rowstart + 1; j++) {
                    if (!obJgridfunc.CheckForRowDuplication(j)) {
                        rowselected.push(j);
                    }
                }
            } else if (rowstart === rowend) {
                if (!obJgridfunc.CheckForRowDuplication(rowstart)) {
                    rowselected.push(rowstart);
                }
            }
            //check if there is -1 in the array if yes delete it) handsontable bug when you select all CTR+A
            if (rowselected[0] === -1) rowselected.shift();
            //return rows selected to the user
            if (rowselected.length !== 0) {
                document.getElementById("rowcounter").innerText = rowselected.length;
            }

            //check if the table is trackable; if yes return data into the trackble container
            obJgridfunc.RowKeyid = hot.getDataAtRow(rowstart)[0];
            if (obJgridfunc._isTableTrackble == true) {
                obJgridfunc.GetRowTrackTableData();
            }
        });
        hot.addHook('beforeOnCellMouseOver', function (event, coords, TD, blockCalculations) {
            // wrote to prevent selecting clips and attachment if exist
            if (coords.col < obJgridfunc.StartInCell()) {
                event.stopImmediatePropagation();
            }

            if (data.HasDrillDowncolumn == true && coords.col == 1 && coords.row != -1) {
                //drildownclick.Childid = hot.getDataAtCell(coords.row, 0);
                //var createNewdrillLinks = "";
                //createNewdrillLinks = '<a class="dropdown-toggle" data-toggle="dropdown" aria-expanded="true"><span class="fa-stack theme_color" title="drilldown"><i class="fa fa-circle-thin fa-stack-2x"></i><i class="fa fa-list fa-stack-1x" style="top: 8px;"></i></span></a>'
                //    + data.ListOfdrilldownLinks

                //TD.innerHTML = createNewdrillLinks
            }
            //} else if (TD.children[1] != undefined && coords.col === 1) {

            //}

        });
        hot.addHook('afterUndo', function (action) {

        });
        hot.addHook('afterColumnSort', function (currentSortConfig, destinationSortConfigs) {
            if (destinationSortConfigs.length === 0 || destinationSortConfigs === undefined) {
                app.data.sortableFields[0].SortOrderDesc = false;
                return;
            }
            var colName = app.data.ListOfHeaders[destinationSortConfigs[0].column].ColumnName;
            var sortorder = "";
            if (destinationSortConfigs[0].sortOrder == "asc") {
                sortorder = false;
            } else {
                sortorder = true;
            }
            app.data.sortableFields = [];
            app.data.sortableFields.push({ FieldName: colName, SortOrder: "", SortOrderDesc: sortorder })
        });
        hot.updateSettings({
            hiddenColumns: {
                columns: [0],
                indicators: true
            },
            //calculate row to get a dynamic height 34px is the row high + 68 is the header hight 
            height: () => {
                //var sizeHight = hot.countRows() * 34 + 68; option
                return CalculateHeightGridSize();
            }
        });
        //search ocr
        /*var srheighLight = document.getElementById("searchInpage")*/;
        //srheighLight.style.display = "none";
        //Handsontable.dom.addEvent(srheighLight, 'keyup', function (event) {
        //    var search = hot.getPlugin('search');
        //    if (this.value.length > 2) {
        //        var queryResult = search.query(this.value);
        //        console.log(queryResult.length);
        //    }
        //    if (this.value.length === 0) {
        //        search.query("");
        //    }
        //    hot.render();
        //});
    },
    RunhandsOnTable: function (data, type) {
        if (hot === undefined) {
            this.buildHandsonTable(data);
        } else {
            hot.destroy();
            this.buildHandsonTable(data);
        }
        setTimeout(() => {
            hot.render();
        }, 500)
        this.ClearObjectsForNewview();
        if (type === "newrecord") return;
        this.Pagination(data);
    },
    Pagination: function (data) {
        pagingfun.SelectedIndex = data.PageNumber;
        pagingfun.PerPageRecords = data.RowPerPage;
        if (obJgridfunc.isPagingClick === true) {
            pagingfun.SetTextFieldValues()
            pagingfun.VisibleIndexs()
        }
        else {
            //get the total rows later written that way to prevent slowness with big data.
            this.GetTotalRows();
        }
    },
    GetTotalRows: function () {
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
    },
    ClearObjectsForNewview: function () {
        this.rowcallLinks = [];
    },
    RowRendererCheckBox: function (instance, td, row, col, prop, value, cellProperties) {
        Handsontable.renderers.HtmlRenderer.apply(this, arguments);
        td.className = "text-center action-icon";
        //td.innerHTML = '<input type="checkbox" checked="checked" onchange="test(this)" data-row="' + row + '">';
        td.innerHTML = '<input type="checkbox" onchange="ListOfCheckboxArray(this)" data-row="' + row + '">';
    },
    RowRenderDrilldown: function (instance, td, row, col, prop, value, cellProperties) {
        Handsontable.renderers.HtmlRenderer.apply(this, arguments);
        td.className = "Column2 text-center action-icon open";
        td.innerHTML = '<a onclick="BuildDrillDownList(event)" class="dropdown-toggle" data-toggle="dropdown" aria-expanded="true"><span class="fa-stack theme_color" title="Menu"><i class="fa fa-circle-thin fa-stack-2x"></i><i class="fa fa-list fa-stack-1x" style="top:8px;"></i></span></a>';
        //td.innerHTML = '<a class="dropdown-toggle" data-toggle="dropdown" aria-expanded="true"><span class="fa-stack theme_color" title="Menu"><i class="fa fa-circle-thin fa-stack-2x"></i><i class="fa fa-list fa-stack-1x" style="top:8px;"></i></span></a>';
    },
    RowRendererattachment: function (instance, td, row, col, prop, value, cellProperties) {
        Handsontable.renderers.HtmlRenderer.apply(this, arguments);
        td.style.textAlign = 'center';
        if (value > 0) {
            td.innerHTML = '<i style="cursor: pointer" onclick="obJattachmentsview.StartAttachmentDialog()" class="fa fa-paperclip fa-flip-horizontal fa-2x theme_color"></i>';
        } else {
            td.innerHTML = '<span style="cursor: pointer" onclick="obJattachmentsview.StartAttachmentDialog()" class="fa-stack theme_color"><i class="fa fa-paperclip fa-flip-horizontal fa-stack-2x"></i><i class="fa fa-plus fa-stack-1x" style="top:16px;left:10px;"></i></span>';
        }
    },
    RowTexttruncated: function (instance, td, row, col, prop, value, cellProperties) {
        Handsontable.renderers.TextRenderer.apply(this, arguments);
        td.innerHTML = `<div class="truncated">${value}<span style="color:blue;"></div>`
    },
    ColumnInteger: function (instance, td, row, col, prop, value, cellProperties) {
        Handsontable.renderers.HtmlRenderer.apply(this, arguments);
    },
    ColumnString: function (instance, td, row, col, prop, value, cellProperties) {
        Handsontable.renderers.HtmlRenderer.apply(this, arguments);
    },
    ColumnDouble: function (instance, td, row, col, prop, value, cellProperties) {
        Handsontable.renderers.HtmlRenderer.apply(this, arguments);
    },
    ColumnDateTime: function (instance, td, row, col, prop, value, cellProperties) {
        Handsontable.renderers.HtmlRenderer.apply(this, arguments);
    },
    AllowKeysIntegerCell: function (event) {
        var isAllow = false;
        switch (event.keyCode) {
            case 8: //Backspace
                isAllow = true;
                break;
            case 46: //Delete
                isAllow = true;
                break;
            case 37: //arrow left
                isAllow = true;
                break;
            case 39: //arrow right
                isAllow = true;
                break;
            case 40: //arrow down
                isAllow = true;
                break;
            case 38:
                isAllow = true; //arrow up
                break;
            case 32: //space
                isAllow = true;
                break;
            default:
        }
        return isAllow;
    },
    GridKeysControll: function (row, endrow, event, cell) {
        //delete key
        if (row !== endrow && event.keyCode === 46) {
            event.stopImmediatePropagation()
            showAjaxReturnMessage("You can't delete cells in multiple rows", "e");
        }

        //backspace
        if (row !== endrow && event.keyCode === 8) {
            event.stopImmediatePropagation()
            showAjaxReturnMessage("You can't delete cells in multiple rows", "e");
        }
        //left arrow
        if (event.keyCode === 37) {
            var index = obJgridfunc.StartInCell();
            var cellStart = hot.getSelected()[0][1]
            var cellEnd = hot.getSelected()[0][3]
            if (cellStart > cellEnd) {
                if (cellEnd <= index) {
                    event.stopImmediatePropagation();
                    hot.scrollViewportTo(0, 0)
                }
            } else if (cellEnd > cellStart) {
                if (cellStart <= index) {
                    event.stopImmediatePropagation();
                    hot.scrollViewportTo(0, 0)
                }
            } else {
                if (cellStart <= index) {
                    event.stopImmediatePropagation();
                    hot.scrollViewportTo(0, 0)
                }
            }
        }

    },
    GetDropDownValue: function (cell, afterChange) {
        var colprop = app.data.ListOfdropdownColumns.find(a => a.colorder == app.data.ListOfHeaders[cell].columnOrder);
        var dropList = colprop.display.split(",");
        var index = dropList.findIndex(name => name == afterChange)
        var values = colprop.value.split(",")
        return values[index];
    }
};
var HandsOnTableReports = {
    buildHandsonTable: function (data) {
        this.container = document.querySelector(app.domMap.Reporting.HandsOnTableContainer);
        this.container.style.display = "block";
        this.hotSettings = {
            data: data.ListOfRows,
            rowHeaders: false,
            //dropdownMenu: ['remove_col', '---------', 'alignment'],
            filters: true,
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
            //currentRowClassName: 'currentRow',
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
        hot = new Handsontable(this.container, this.hotSettings);
        hot.updateSettings({
            cells: function (row, col) {
                var cellProperties = {};
                cellProperties.readOnly = true;
                return cellProperties;
            },
            //calculate row to get a dynamic height 34px is the row high + 68 is the header hight 
            height: hot.countRows() * 34 + 68,
            //colWidths: this.colWidthArray
        });
    }

}
var HandsOnTableRetentionRowInfo = {
    buildHandsonTable: function (data) {
        this.container = document.querySelector(app.domMap.Retentioninfo.handsOnTableRetinfo);
        this.container.style.display = "block";
        this.hotSettings = {
            data: data.ListOfRows.length == 0 ? [["#####", "#####", "#####"]] : data.ListOfRows,
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
            //search: true,
            headerTooltips: true,
            currentRowClassName: 'currentRow',
            //renderAllRows: false,
            //afterDeselect: true,
            //colWidths: [0, 10,18],
            //fixedRowsTop: 1,
            //manualColumnFreeze: true,F
            comments: true,
            //contextMenu: ['row_above', 'row_below', 'remove_row'],
            contextMenu: obJtoolbarmenufunc.GetMenu(),
            autoRowSize: false,
            autoWrapCol: false,
            autoWrapRow: false,
            //manualRowMove: true,
            //manualColumnMove: true,
            licenseKey: "a1fe9-3411a-c6714-e4504-e7930",
        };
        hotRetention = new Handsontable(this.container, this.hotSettings);
        hotRetention.updateSettings({
            cells: function (row, col) {
                var cellProperties = {};
                cellProperties.readOnly = true;
                return cellProperties;
            },
            //calculate row to get a dynamic height 34px is the row high + 68 is the header hight 
            height: hotRetention.countRows() * 34 + 68,
            //colWidths: this.colWidthArray
        });
        hotRetention.addHook('afterSelectionEnd', function (rowstart, colstart, rowend, colend) {
            //get rows selected
            if (hot.getSelected().length === 1) {
                rowselectedRetention = [];
            }
            if (rowstart < rowend) {
                for (var i = rowstart; i < rowend + 1; i++) {
                    rowselectedRetention.push(i);
                }
            } else if (rowstart > rowend) {
                for (var j = rowend; j < rowstart + 1; j++) {
                    rowselectedRetention.push(j);
                }
            } else if (rowstart === rowend) {
                rowselectedRetention.push(rowstart);
            }
        });
    }
}
