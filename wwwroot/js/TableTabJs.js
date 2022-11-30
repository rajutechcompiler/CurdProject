var signatureVar;
var TableName = "";
var TableId = "";
var targetId = "tableGeneral";
var DispositionTable = null;
var DispositionType = null;
var dispositionFlag = false;
var IsTrackable = false;
var trackableFlag = false;
var trackableValue = false;
var DispositionTableAttach = null;

$(function () {
    /* Secuirty Integration - Added by Ganesh*/
    if (!IS_ADMIN) {
        $('#tabRetention').remove();
    }
    /* End Security Integration */
    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        targetId = $(e.target).attr("id");
        //Added by Hasmukh for fix : FUS-2317
        TableId = $("#hdnTableId").val();
        BindTableTabInfo(targetId);
    });
});
function loadTabData(e, linkId) {
    e.stopPropagation();
    TableName = $(linkId).attr("id");
    TableId = $(linkId).attr("name");
    $("#ulTable li a").each(function (a, b) {
        $(this).removeClass('selectedMenu');
    });
    $('.divMenu').find('a.selectedMenu').removeClass('selectedMenu');
    linkId.addClass('selectedMenu');
    $('#tableLabel').html($(linkId).text());
    var currentTab = $("#divTableTab").find('ul').find('li.active > a').attr('id');
    if (currentTab == undefined)
        targetId = "tableGeneral";
    BindTableTabInfo(targetId);
    TableName = $(linkId).attr("id");
    $("#divTableTab").show();
    $("#ulTable").find('li').find('.fa-play-circle').removeClass('fa-play-circle').addClass('fa-circle-o');
    linkId.find('i').removeClass('fa-circle-o').addClass('fa-play-circle');
    setCookie('hdnAdminMnuIds', ('Tables|' + TableName), 1);
    /* Hasmukh : Added on 04/04/2016 - Security check */
    CheckModuleLevelAccess("liTables");
}

//Code for Retention Tab
function BindTableTabInfo(targetId) {
    switch (targetId) {
        case "tableGeneral":
            GetTableGeneralData(TableName);
            break;
        case "tabTableFields":
            LoadFieldsView(TableName);
            break;
        case "tableTracking":
            GetTableTrackingData(TableName);
            break;
        case "tabRetention":
            loadTableRetentionInformation();
            break;
        case "tabFileRoomOrder":
            LoadFileRoomOrderView(TableName);
            break;
        case "tableAdvanced":
            LoadTableAdvancedData(TableName);
            break;
    }
}
function LoadOutField(DDLListId, outFieldJSONList) {
    $(DDLListId).empty();
    $(outFieldJSONList).each(function (i, item) {
        $(DDLListId).append("<option value=" + item.Key + ">" + item.Value + "</option>");
    });
}
function loadTableRetentionInformation() {
    if ($('#LoadTabContent').length == 0) {
        RedirectOnAccordian(urls.TableTracking.LoadTableTab);
        $('#title, #navigation').text(vrCommonRes['mnuTables']);
    }
    var IsRelatedTblEnabled = false;

    $.ajax({
        url: urls.Retention.LoadTablesRetentionView,
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html'
    }).done(function (result) {
            $('#LoadTabContent').empty();
            $('#LoadTabContent').html(result);

            $.post(urls.Retention.GetRetentionPropertiesData, { pTableId: TableId })
                    .done(function (data) {
                        if (data.errortype == 's') {
                            debugger;
                            var tableEntityJson = $.parseJSON(data.tableEntity);

                            //console.log("TrackingTable value: " + tableEntityJson);
                            //check if location exist
                            if (data.isThereLocation.length === 0) {
                                $("#archiveLocationmsg").show();
                            } else {
                                $("#archiveLocationmsg").hide();
                            }
                            if (tableEntityJson.TrackingTable != 1) {
                                $("#parentTblRet").show();
                                $('#lblLevel1Container').text("");

                                // start Issue FUS-7916 
                                data.ArchiveLocationField = data.ArchiveLocationField == null ? "" : data.ArchiveLocationField
                                if (data.ArchiveLocationField.length > 0) {
                                    $("#divArchiveLocationField").hide();
                                } else {
                                    $("#divArchiveLocationField").show();
                                }
                                // end issue fixed

                                var pRetentionCodes = $.parseJSON(data.pRetentionCodes);
                                GetRetentionCodeList(pRetentionCodes);
                                //Set Permanent Archive based on Trackable status.
                                //console.log("IsTrackable Status in TableRetention.js: " + IsTrackable);
                                $("#lblPermanentArchive").val("");
                                if (data.bTrackable) {//Changed from tableEntityJson.Trackable to based on 'Transfer' permission. - 15/12/2015 By Ganesh.
                                    $("#lblPermanentArchive").text(vrApplicationRes['lblJsTableRetenntionPartialStartFieldPerArch']);
                                    $("input[name=disposition][value= '1']").removeAttr('disabled');
                                }
                                else {
                                    $("#lblPermanentArchive").text(vrApplicationRes['lblJsTableRetenntionPartialStartFieldPerArchTrc']);
                                    $("input[name=disposition][value= '1']").attr('disabled', 'disabled');
                                }                                
                                //Change made on 02/03/2016.
                                if (tableEntityJson.RetentionPeriodActive || tableEntityJson.RetentionInactivityActive)
                                    $("input[name=disposition][value=" + tableEntityJson.RetentionFinalDisposition + "]").attr('checked', 'checked');

                                $("input[name=assignment][value=" + tableEntityJson.RetentionAssignmentMethod + "]").prop('checked', true).change();
                                $("#chkInactivity").prop("checked", tableEntityJson.RetentionInactivityActive);
                                $("#lstRetentionCodes").val(tableEntityJson.DefaultRetentionId);

                                var pRetIdsObject = $.parseJSON(data.RetIdFieldsList);
                                var pRetDateObject = $.parseJSON(data.RetDateFieldsList);

                                $('#lstFieldRetentionCode').empty();
                                $("#lstFieldDateOpened").empty();
                                $("#lstFieldDateClosed").empty();
                                $("#lstFieldDateCreated").empty();
                                $("#lstFieldOtherDate").empty();
                                $('#lstRelatedTables').empty();

                                var jsonRelatedTblObj = $.parseJSON(data.relatedTblObj);
                                //$('#lstRelatedTables').append($("<option>", { value: "", html: "" }));

                                $(jsonRelatedTblObj).each(function (i, v) {
                                    $('#lstRelatedTables').append($("<option>", { value: jsonRelatedTblObj[i].TableName, html: jsonRelatedTblObj[i].UserName }));
                                    IsRelatedTblEnabled = true;
                                });
                                //Added by Ganesh - SEP 24
                                if (IsRelatedTblEnabled)
                                    $("input[name=assignment][value= '2']").removeAttr('disabled');


                                if (!$("#chkInactivity").is(":checked") & $('input:radio[name=disposition]:checked').val() == 0) {
                                    //console.log('Initial Inactivity click...');
                                    disableRetentionPropFields();
                                }
                                else {
                                    enableRetentionPropFields();
                                }

                                if ($("input:radio[name='assignment']:checked").val() == 2) {
                                    $("#lstRelatedTables").removeAttr('disabled', 'disabled');
                                    $("#lstRetentionCodes").attr('disabled', 'disabled');
                                }
                                //Changes made on 15/12/2015.
                                if (($("input[name=assignment][value= '0']").is(':enabled') && $("input[name=assignment][value= '1']").is(':enabled')) & ($("input:radio[name='assignment']:checked").val() == 0 || $("input:radio[name='assignment']:checked").val() == 1)) {
                                    $("#lstRelatedTables").attr('disabled', 'disabled');
                                    $("#lstRetentionCodes").removeAttr('disabled', 'disabled');
                                }

                                if (data.lstRetentionCode != "") {
                                    $('#lstFieldRetentionCode').append($("<option>", { value: "* RetentionCodesId", html: data.lstRetentionCode }));
                                    $('#lstFieldRetentionCode option:first').attr('selected', 'selected');

                                }

                                if (data.lstDateOpened != "") {
                                    $('#lstFieldDateOpened').append($("<option>", { value: data.lstDateOpened, html: data.lstDateOpened }));
                                    $('#lstFieldDateOpened option:first').attr('selected', 'selected');
                                }

                                if (data.lstDateClosed != "") {
                                    $('#lstFieldDateClosed').append($("<option>", { value: data.lstDateClosed, html: data.lstDateClosed }));
                                    $('#lstFieldDateClosed option:first').attr('selected', 'selected');
                                }

                                if (data.lstDateCreated != "") {
                                    $('#lstFieldDateCreated').append($("<option>", { value: data.lstDateCreated, html: data.lstDateCreated }));
                                    $('#lstFieldDateCreated option:first').attr('selected', 'selected');
                                }

                                if (data.lstDateOther != "") {
                                    $('#lstFieldOtherDate').append($("<option>", { value: data.lstDateOther, html: data.lstDateOther }));
                                    $('#lstFieldOtherDate option:first').attr('selected', 'selected');
                                }

                                //Show and Hide Star Field Label
                                if (!data.bFootNote)
                                    $("#lblStarField").hide();

                                $(pRetIdsObject).each(function (i, v) {
                                    $('#lstFieldRetentionCode').append($("<option>", { value: pRetIdsObject[i], html: pRetIdsObject[i] }));
                                });

                                $(pRetDateObject).each(function (i, v) {
                                    $('#lstFieldDateOpened').append($("<option>", { value: pRetDateObject[i], html: pRetDateObject[i] }));
                                    $('#lstFieldDateClosed').append($("<option>", { value: pRetDateObject[i], html: pRetDateObject[i] }));
                                    $('#lstFieldDateCreated').append($("<option>", { value: pRetDateObject[i], html: pRetDateObject[i] }));
                                    $('#lstFieldOtherDate').append($("<option>", { value: pRetDateObject[i], html: pRetDateObject[i] }));
                                });

                                $("#lstRelatedTables").val(tableEntityJson.RetentionRelatedTable);

                                if (tableEntityJson.RetentionFieldName !== null)
                                    $("#lstFieldRetentionCode").val(tableEntityJson.RetentionFieldName);
                                if (tableEntityJson.RetentionDateOpenedField !== null)
                                    $("#lstFieldDateOpened").val(tableEntityJson.RetentionDateOpenedField);
                                if (tableEntityJson.RetentionDateCreateField !== null)
                                    $("#lstFieldDateCreated").val(tableEntityJson.RetentionDateCreateField);
                                if (tableEntityJson.RetentionDateClosedField !== null)
                                    $("#lstFieldDateClosed").val(tableEntityJson.RetentionDateClosedField);
                                if (tableEntityJson.RetentionDateOtherField !== null)
                                    $("#lstFieldOtherDate").val(tableEntityJson.RetentionDateOtherField);

                                $("input:radio[name=disposition]").click(function () {
                                    //console.log("Inside disposistion type clicked...");
                                    var SelectConfirmValueVar = parseInt($('input:radio[name=disposition]:checked').val());
                                    dispositionFlag = true;
                                    DispositionTable = $('#tableLabel').text();
                                    DispositionType = SelectConfirmValueVar;
                                });

                                //Handle None and InActivity Flag.
                                $("input[name='disposition']").on("change", function () {
                                    //console.log("Inside disposistion type change...");
                                    var boolInActivity = $("#chkInactivity").is(":checked");

                                    if (this.value == 1 || this.value == 2 || this.value == 3) {
                                        enableRetentionPropFields();
                                    }
                                    else if (this.value == 0 & boolInActivity == false) {
                                        disableRetentionPropFields();
                                    }
                                });

                                $("input[name='assignment']").on("change", function () {
                                    switch (this.value) {
                                        case "0":
                                            if ($("#lstRetentionCodes option[value='0']").length == 0) {
                                                //console.log("Inside If...");
                                                ($("<option>", { value: "0", html: "" })).prependTo('#lstRetentionCodes');
                                            }
                                            $("#lstRelatedTables").attr('disabled', 'disabled');
                                            $("#lstRetentionCodes").removeAttr('disabled', 'disabled');
                                            break;
                                        case "1":

                                            if ($("#lstRetentionCodes option[value='0']").length) {
                                                $('#lstRetentionCodes option[value="0"]').remove();
                                            }
                                            $("#lstRelatedTables").attr('disabled', 'disabled');
                                            $("#lstRetentionCodes").removeAttr('disabled', 'disabled');
                                            break;
                                        case "2":
                                            $("#lstRelatedTables").removeAttr('disabled', 'disabled');
                                            $("#lstRetentionCodes").attr('disabled', 'disabled');
                                            break;
                                        default:
                                            $("#lstRelatedTables").attr('disabled', 'disabled');
                                            break;
                                    }
                                });

                                $('#chkInactivity').change(function () {
                                    //console.log("Inside InActivity type change...");
                                    if ($(this).is(":checked")) {
                                        enableRetentionPropFields();
                                    }
                                    else if (!$(this).is(":checked") & $('input:radio[name=disposition]:checked').val() == 0) {
                                        disableRetentionPropFields();
                                    }

                                });

                                $("#btnSaveRetentionInfo").on('click', function (e) {

                                    var pInActivity = $("#chkInactivity").is(":checked");;
                                    var pDisposition = $('input:radio[name=disposition]:checked').val();
                                    var pAssignment = $('input:radio[name=assignment]:checked').val();
                                    var pDefaultRetentionId = $('#lstRetentionCodes').val();
                                    var pRelatedTable = $("#lstRelatedTables").val();

                                    var pRetentionCode = $("#lstFieldRetentionCode").val();
                                    var pDateOpened = $("#lstFieldDateOpened").val();
                                    var pDateClosed = $("#lstFieldDateClosed").val();
                                    var pDateCreated = $("#lstFieldDateCreated").val();
                                    var pOtherDate = $("#lstFieldOtherDate").val();

                                    if (TableId != "")
                                        setRetentionData(TableId, pInActivity, pAssignment, pDisposition, pDefaultRetentionId, pRelatedTable, pRetentionCode, pDateOpened, pDateClosed, pDateCreated, pOtherDate);
                                });

                            }//End of IF
                            else {
                                $("#parentTblRet").hide();
                                $('#lblLevel1Container').text(vrApplicationRes['lblJsTableRetenntionPartialReteCantSetLvl1']);
                            }
                        }
                        setTimeout(function () {
                            var s = $(".sticker");
                            $('.content-wrapper').scroll(function () {
                                if ($(this).scrollTop() + $(this).innerHeight() + 60 >= $(this)[0].scrollHeight) {
                                    s.removeClass("stick", 10);
                                }
                                else {
                                    s.addClass("stick");
                                }
                            });
                        }, 800);
                    });
        })
        .fail(function (xhr, status) {
            ShowErrorMessge();
        });
}

function setRetentionData(vTableId, vInActivity, vAssignment, vDisposition, vDefaultRetentionId, vRelatedTable, vRetentionCode, vDateOpened, vDateClosed, vDateCreated, vOtherDate) {
    $.post(urls.Retention.SetRetentionTblPropData, { pTableId: vTableId, pInActivity: vInActivity, pAssignment: vAssignment, pDisposition: vDisposition, pDefaultRetentionId: vDefaultRetentionId, pRelatedTable: vRelatedTable, pRetentionCode: vRetentionCode, pDateOpened: vDateOpened, pDateClosed: vDateClosed, pDateCreated: vDateCreated, pOtherDate: vOtherDate })
           .done(function (response) {
               if (response.errortype == 's') {

                   if (response.msgVerifyRetDisposition != "") {

                       //Made change on 18/02/2016 - By Ganesh to fix bug #64  from spreadsheet.
                       $(this).confirmModal({
                           confirmTitle: 'TAB FusionRMS',
                           confirmMessage: response.msgVerifyRetDisposition,
                           confirmOk: vrCommonRes['Ok'],
                           confirmStyle: 'default',
                           confirmOnlyOk: true,                           
                           confirmCallback: ReloadRetentionData                           
                       });                       
                       //showAjaxReturnMessage('Record saved successfully.', 's');
                   }
                   else {
                       //showAjaxReturnMessage(vrApplicationRes['msgJsRequestorRecSaveSuccessfully'], 's');
                       showAjaxReturnMessage(vrTablesRes['msgAdminCtrlRecordUpdatedSuccessfully'], 's');
                       loadTableRetentionInformation();
                   }

               }
           })
           .fail(function (xhr, status, error) {
               ShowErrorMessge();
           });
}

function ReloadRetentionData() {
    loadTableRetentionInformation();
}

function enableRetentionPropFields() {
    //$('input[name="assignment"]').removeAttr('disabled', 'disabled');
    //console.log("Inside enableRetentionPropFields() method..." + $('#lstRelatedTables option').length);
    var vAssignmentSelection = $("input:radio[name='assignment']:checked").val();
    $("#btnSaveRetentionProp").removeAttr('disabled', 'disabled');

    $("input[name=assignment][value= 0]").removeAttr('disabled', 'disabled');
    $("input[name=assignment][value= 1]").removeAttr('disabled', 'disabled');
    $("#lstRetentionCodes").removeAttr('disabled', 'disabled');
    if (vAssignmentSelection != 0 && vAssignmentSelection != 1)
        $("#lstRelatedTables").removeAttr('disabled', 'disabled');

    if ($('#lstRelatedTables option').length >= 1) {
        //$("#lstRelatedTables").removeAttr('disabled', 'disabled');        
        $("input[name=assignment][value= 2]").removeAttr('disabled', 'disabled');
    }

    $("#lstFieldRetentionCode").removeAttr('disabled', 'disabled');
    $("#lstFieldDateOpened").removeAttr('disabled', 'disabled');
    $("#lstFieldDateClosed").removeAttr('disabled', 'disabled');
    $("#lstFieldDateCreated").removeAttr('disabled', 'disabled');
    $("#lstFieldOtherDate").removeAttr('disabled', 'disabled');
}


function disableRetentionPropFields() {
    //$('input[name="assignment"]').attr('disabled', 'disabled');
    //console.log("Inside disableRetentionPropFields() method...");
    $("#btnSaveRetentionProp").attr('disabled', 'disabled');
    $("#lstRelatedTables").attr('disabled', 'disabled');
    $("input[name=assignment][value= 0]").attr('disabled', 'disabled');
    $("input[name=assignment][value= 1]").attr('disabled', 'disabled');
    $("input[name=assignment][value= 2]").attr('disabled', 'disabled');
    $("#lstRetentionCodes").attr('disabled', 'disabled');

    if ($('#lstRelatedTables option').length >= 1) {
        //$("#lstRelatedTables").attr('disabled', 'disabled');
        $("input[name=assignment][value= 2]").attr('disabled', 'disabled');
    }

    $("#lstFieldRetentionCode").attr('disabled', 'disabled');
    $("#lstFieldDateOpened").attr('disabled', 'disabled');
    $("#lstFieldDateClosed").attr('disabled', 'disabled');
    $("#lstFieldDateCreated").attr('disabled', 'disabled');
    $("#lstFieldOtherDate").attr('disabled', 'disabled');
}

//function GetRetentionCodeList() {
//    alert("GetRetentionCodeList");
//    $.ajax({
//        url: urls.Retention.GetRetentionCodeList,
//        dataType: "json",
//        type: "GET",
//        contentType: 'application/json; charset=utf-8',
//        async: false,
//        processData: false,
//        cache: false,
//        success: function (data) {
//            alert("Inside SUccess");
//            debugger;
//            var pRetentionObject = $.parseJSON(data);

//            $('#lstRetentionCodes').empty();

//            $('#lstRetentionCodes').append($("<option>", { value: "0", html: "" }));
//            $(pRetentionObject).each(function (i, v) {
//                $('#lstRetentionCodes').append($("<option>", { value: v.Id, html: v.Id }));
//            });
//        },
//        error: function (xhr, status, error) {
//            //console.log("Error: " + error);
//            ShowErrorMessge();
//        }
//    });
//}


function GetRetentionCodeList(pRetentionObject) {
    $('#lstRetentionCodes').empty();
    $('#lstRetentionCodes').append($("<option>", { value: "0", html: "" }));
    $(pRetentionObject).each(function (i, v) {
        $('#lstRetentionCodes').append($("<option>", { value: v.Id, html: v.Id }));
    });
}


//Code For File Room Order Grid.
jQuery.fn.gridLoadFileRoomOrder = function (getUrl, caption, tableName) {
    var gridobject = $(this);
    //alert("Table Name in gridLoadFileRoomOrder: "+tableName);    
    $.post(urls.Common.GetGridViewSettings, { pGridName: "grdFileRoomOrder" })
                .done(function (response) {
                    BindFileRoomOrderGrid(gridobject, getUrl, $.parseJSON(response), caption, tableName);
                })
                .fail(function (xhr, status, error) {
                    ShowErrorMessge();
                });
}

jQuery.fn.getSelectedRowsIds = function () {
    var selectedrows = $(this).jqGrid('getGridParam', 'selarrrow');
    return selectedrows;
};

jQuery.fn.refreshJqGrid = function () {
    $(this).trigger('reloadGrid');
};

jQuery.fn.setGridColumnsOrder = function () {
    var pDatabaseGridName = $(this).attr('id');
    $.post(urls.Common.SetGridOrders, { pGridName: pDatabaseGridName })
                .done(function (response) {
                    //if (!response.MSGWarnning) {

                    //}
                    //showAjaxReturnMessage(response.message, (response.MSGWarnning ? 'w' : 's'));
                })
                .fail(function (xhr, status, error) {
                    //ShowErrorMessge();
                });
};

function BindFileRoomOrderGrid(gridobject, getUrl, arrColSettings, caption, tableName) {    
    //var IsCheckbox = true;
    var pDatabaseGridName = gridobject.attr('id');
    var pPagerName = '#' + pDatabaseGridName + '_pager';

    $("#filterButton").click(function (event) {
        event.preventDefault();
        filterGrid(gridobject);
    });

    var arryDisplayCol = [];
    var arryColSettings = [];
    var globalColumnOrder = [];

    arrColSettings.sort(function (a, b) {
        if (a.srno < b.srno) return -1;
        if (b.srno < a.srno) return 1;
        return 0;
    });
    var sortColumnsName = '';

    for (var i = 0; i < arrColSettings.length; i++) {        
        arryDisplayCol.push([arrColSettings[i].displayName]);
        globalColumnOrder.push([arrColSettings[i].srno]);
        if (arrColSettings[i].srno == -1) {
            arryColSettings.push({ key: true, hidden: true, name: arrColSettings[i].name, index: arrColSettings[i].name, sortable: arrColSettings[i].sortable });
        }
        else if (arrColSettings[i].name == "StartFromFront")
        {            
            arryColSettings.push({ key: false, align: "center", formatter: "checkbox", editable: false, edittype: 'checkbox', editoptions: { value: "1:0" }, name: arrColSettings[i].name, index: arrColSettings[i].name, sortable: arrColSettings[i].sortable });
        }
        else {
            arryColSettings.push({ key: false, name: arrColSettings[i].name, index: arrColSettings[i].name, sortable: arrColSettings[i].sortable });
        }
        if (arrColSettings[i].srno != -1 && sortColumnsName == "") {
            sortColumnsName = arrColSettings[i].name;
        }
    }
    
    gridobject.jqGrid({
        url: getUrl,
        datatype: 'json',
        mtype: 'Get',
        //data: gridData,
        postData: { pTableName: tableName },
        colNames: arryDisplayCol,
        colModel: arryColSettings,
        pager: jQuery(pPagerName),
        rowNum: 20,
        rowList: [20, 40, 80, 100],
        height: '100%',
        viewrecords: true,
        loadonce: false,
        grouping: false,
        caption: caption,
        emptyrecords: vrCommonRes["NoRecordsToDisplay"],
        jsonReader: {
            root: "rows",
            page: "page",
            total: "total",
            records: "records",
            repeatitems: false,
            Id: "0"
        },
        sortable: {
            update: function (relativeColumnOrder) {
                var grid = gridobject;
                var currentColModel = grid.getGridParam('colModel');
                $.ajax({
                    url: urls.Common.ArrangeGridOrder,
                    type: 'POST',
                    data: JSON.stringify(currentColModel),
                    contentType: 'application/json; charset=utf-8',
                    success: function (response) {

                    },
                    error: function (xhr, status, error) {
                    }
                });
            }
        },
        sortname: sortColumnsName,
        autowidth: true,
        shrinkToFit: true,
        //width: $('#MainWrapper').width(),
        multiselect: true//IsCheckbox//,
    }).navGrid(pPagerName, { edit: false, add: false, del: false, search: false, refresh: true, refreshtext: vrCommonRes["Refresh"] }
    );//.trigger("reloadGrid", [{ current: true, page: 1 }]);

    //hide Select all checkbox in 'File Room Order' grid
    $('#jqgh_grdFileRoomOrder_cb').hide();
}

function filterGrid(grid) {
    var postDataValues = grid.jqGrid('getGridParam', 'postData');
    $(".filterItem").each(function (index, item) {
        postDataValues[$(item).attr('id')] = $(item).val();
    });
    grid.jqGrid().setGridParam({ postData: postDataValues, page: 1 }).trigger('reloadGrid');
}

function sortResults(arr, prop, asc) {
    arr = arr.sort(function (a, b) {
        if (asc) return (a[prop] > b[prop]) ? 1 : ((a[prop] < b[prop]) ? -1 : 0);
        else return (b[prop] > a[prop]) ? 1 : ((b[prop] < a[prop]) ? -1 : 0);
    });
    return arr;
}

jQuery.fn.sort = function () {
    return this.pushStack([].sort.apply(this, arguments), []);
};

//End Grid Code.

function LoadFileRoomOrderView(TableName) {
    if ($('#LoadTabContent').length == 0) {
        RedirectOnAccordian(urls.TableTracking.LoadTableTab);
        $('#title, #navigation').text(vrCommonRes['mnuTables']);
    }
    $.ajax({
        url: urls.TableFileRoomOrder.LoadTablesFileRoomOrderView,
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html'
    }).done(function (result) {
        $('#LoadTabContent').empty();
        $('#LoadTabContent').html(result);

        //$("#grdFileRoomOrder").jqGrid("unload");
        $("#grdFileRoomOrder").gridLoadFileRoomOrder(urls.TableFileRoomOrder.GetListOfFileRoomOrders, vrTablesRes["mnuTableTabPartialFileRoomOrder"], TableName);

        $("#gridAdd").off().on('click', function () {
            $("#myModalLabel").empty();
            $('#myModalLabel').append(String.format(vrTablesRes["tiTableFileRoomOrderFileRoomOrdrAddLn"]));
            ShowFileRoomOrderPopup(TableName);
        });

        $("#gridEdit").off().on('click', function () {
            $("#myModalLabel").empty();
            $('#myModalLabel').append(String.format(vrTablesRes["tiTableFileRoomOrderFileRoomOrdrEditLn"]));
            var OperationFlag = "EDIT";

            var selRowId = $("#grdFileRoomOrder").jqGrid('getGridParam', 'selrow');
            var celValue = $("#grdFileRoomOrder").jqGrid('getCell', selRowId, 'Id');

            var selectedrows = $("#grdFileRoomOrder").getSelectedRowsIds();

            if (selectedrows.length > 1 || selectedrows.length == 0) {
                showAjaxReturnMessage(vrCommonRes["PlzSelOnlyOneRow"], 'w');
                return;
            }
            else {

                $.post(urls.TableFileRoomOrder.EditFileRoomOrderRecord, $.param({ pRowSelected: selectedrows, pRecordId: celValue }, true), function (data) {
                    if (data) {
                        var result = $.parseJSON(data);
                        ShowFileRoomOrderPopup(TableName, OperationFlag, result);
                    }
                });
            }
        });

        $("#gridRemove").off().on('click', function () {
            var selRows = $('#grdFileRoomOrder').getSelectedRowsIds();
            if (selRows.length > 1 || selRows.length == 0)
            {
                showAjaxReturnMessage(vrCommonRes["PlzSelOnlyOneRow"], 'w');
                return;
            }
            $(this).confirmModal({
                confirmTitle: 'TAB FusionRMS',
                confirmMessage: vrTablesRes['msgFileRoomOrderTabRecDelete'],
                confirmOk: vrCommonRes['Yes'],
                confirmCancel: vrCommonRes['No'],
                confirmStyle: 'default',
                confirmCallback: delFileRoomOrder
            });
        });
    })
    .fail(function (xhr, status) {
        ShowErrorMessge();
    });
}
function delFileRoomOrder() {
    var selRowId = $("#grdFileRoomOrder").jqGrid('getGridParam', 'selrow');
    var celValue = $("#grdFileRoomOrder").jqGrid('getCell', selRowId, 'Id');

    var selectedrows = $("#grdFileRoomOrder").getSelectedRowsIds();

    if (selectedrows.length > 1 || selectedrows.length == 0) {
        showAjaxReturnMessage(vrCommonRes["PlzSelOnlyOneRow"], 'w');
        return;
    }
    else {
        $.post(urls.TableFileRoomOrder.RemoveFileRoomOrderRecord, $.param({ pRowSelected: selectedrows, pRecordId: celValue }, true), function (data) {
            $("#grdFileRoomOrder").refreshJqGrid();
            showAjaxReturnMessage(data.message, data.errortype);
        });
    }
}
function ShowFileRoomOrderPopup(TableName, OperationFlag, result)
{    
    $('#frmTableFileRoomOrder').resetControls();
    $('#mdlFileRoomOrder').ShowModel();

    $("#btnSave").off().on('click', function () {
        saveFileRoomOrderRecord(TableName);
    });

    //ForceNumericOnly
    $("#txtStartingPosition").OnlyNumeric();
    $("#txtNumOfChars").OnlyNumeric();

    $.getJSON(urls.TableFileRoomOrder.GetListOfFieldNames, $.param({ pTableName: TableName }, true), function (data) {

        var lstFieldNameObject = $.parseJSON(data.lstFieldNames);

        $('#lstFields').empty();
        $("#chkStartFromEnd").prop("checked", true);
        $(lstFieldNameObject).each(function (i, v) {
            $('#lstFields').append($("<option>", { value: lstFieldNameObject[i].substr(0, lstFieldNameObject[i].indexOf('(')).trim(), html: lstFieldNameObject[i] }));
        });

        $("#lstFields").off().on('change', function () {            
            EnableDisableStartFromEndChk();
        });

        if (OperationFlag == "EDIT") {
                $("#Id").val(result.Id);                
                $("#lstFields").val(result.FieldName);
                EnableDisableStartFromEndChk();
                $("#chkStartFromEnd").prop("checked", result.StartFromFront);
                $("#txtStartingPosition").val(result.StartingPosition);
                $("#txtNumOfChars").val(result.NumberofCharacters);
        }
       else
            EnableDisableStartFromEndChk();
    });
}

function saveFileRoomOrderRecord(vTableName)
{
    var $form = $('#frmTableFileRoomOrder');    
    $('#chkStartFromEnd').removeAttr('disabled');
    var serializedForm = $form.serialize() + "&pStartFromFront=" + $("#chkStartFromEnd").is(':checked');
    $.post(urls.TableFileRoomOrder.SetFileRoomOrderRecord + "?pTableName=" + vTableName, serializedForm)
            .done(function (response) {
                showAjaxReturnMessage(response.message, response.errortype);
                
                if (response.errortype == 's') {
                    $('#mdlFileRoomOrder').HideModel();
                    $("#grdFileRoomOrder").refreshJqGrid();                    
                }
            })
            .fail(function (xhr, status, error) {
                //console.log(error);
                ShowErrorMessge();
            });    
}

function EnableDisableStartFromEndChk()
{
    //console.log("EnableDisableStartFromEndChk and sel text: " + $("#lstFields :selected").text());
    if ($("#lstFields :selected").text().search("(Numeric: padded with leading zeros)") != -1)
        $("#chkStartFromEnd").attr("disabled", "disabled");
    else
        $("#chkStartFromEnd").removeAttr("disabled", "disabled");
}
var StatusField = "";
$(function () {
    $('#TrackingStatusFieldName').bind('cut paste', function (e) {
        e.preventDefault();
    });

    
    $("#Trackable").on('change', function () {
        var tableName = $('#TableName').val();
        var IsTrackable = $('#Trackable').is(':checked');
        var requestval = $('#AllowBatchRequesting').is(':checked');
        if (!IsTrackable)
            $('#AllowBatchRequesting').attr('checked', false);
        GetIntialDestination(tableName, true, IsTrackable, requestval);

        trackableFlag = true;
        DispositionTableAttach = $('#tableLabel').text();
        trackableValue = $('#Trackable').is(':checked');
        if (DispositionTable != null && DispositionType != null) {
            if (DispositionTable == $('#tableLabel').text()) {
                if ((DispositionType == 1)) {
                    confirmTitle = 'TAB FusionRMS';
                    confirmMessage = vrTablesRes["msgJsTableTrackingTblMustBTrackingObj"] + "\n " + vrTablesRes["msgJsTableTrackingIfUWouldLike2Chang"];
                    confirmOk = vrCommonRes["Ok"].toUpperCase();
                    ShowWarning(confirmTitle, confirmMessage, confirmOk);
                    $('#Trackable').attr('checked', true);
                }
            }
        }

    });

    $('#TrackingTable').change(function () {
        var containerLevel = parseInt($('#TrackingTable :selected').val());
        if (containerLevel == 0)
            $('#TrackingStatusFieldName').val("");
        else
            $('#TrackingStatusFieldName').val(StatusField);
        var OutTypeVar = parseInt($('#OutTable :selected').val());
        DisableControlOnCLevel(containerLevel);
        DisableDueBackDays(containerLevel, OutTypeVar);
            var boolSignature = (signatureVar && containerLevel);
            DisableSigntureRequired(boolSignature);
        if (containerLevel == 1) {
            if (DispositionTable != null && DispositionType != null) {
                if (DispositionTable == $('#tableLabel').text()) {
                    if ((DispositionType == 1)) {
                        confirmTitle = 'TAB FusionRMS';
                        confirmMessage = vrTablesRes["msgJsTableTrackingTblMustBTrackingObj"] + "\n " + vrTablesRes["msgJsTableTrackingIfUWouldLike2Chang"];
                        confirmOk = vrCommonRes["Ok"].toUpperCase();
                        ShowWarning(confirmTitle, confirmMessage, confirmOk);
                    }
                }
            }
        }
    });

    $('#OutTable').change(function () {
        var OutTypeVar = parseInt($('#OutTable :selected').val());
        var containerLevel = parseInt($('#TrackingTable :selected').val());
        DisableOutField(OutTypeVar);
        DisableDueBackDays(containerLevel, OutTypeVar);
    });


    $('#AutoAssignOnAdd').on('change', function () {
        var trackableObj = $('#Trackable').is(':checked');
        var destinationVar = $('#DefaultTrackingId option').length;
        var autoAssignVar = $('#AutoAssignOnAdd').is(':checked');
        DisableAutoTracking(trackableObj, destinationVar, autoAssignVar);
    });

    //$('#DefaultTrackingDetails').change(function () {
    //    var DefaultId = parseInt($('#DefaultTrackingDetails :selected').attr('Id'));
    //    //var DefaultVal = $('#DefaultTrackingDetails :selected').val();
    //    var DefaultTable = $('#lblDestination').text();
    //    $('#DefaultTrackingId').val(DefaultId);
    //    $('#DefaultTrackingTable').val(DefaultTable);
    //});

    $('#btnApply').on('click', function () {
        var tableVar = $('#TableName').val();
        var cLevelInfo = $('#TrackingTable :selected').val();
        var statusFieldInfo = $('#TrackingStatusFieldName').val();
        var outFieldStatus = $('#TrackingOUTFieldName').is(':disabled');
        var outTypeInfoVal = parseInt($('#OutTable :selected').val());
        var outTypeInfoText = $('#OutTable :selected').text();
        var outFieldInfoVal = parseInt($('#TrackingOUTFieldName :selected').val());
        var outFieldInfoText = $('#TrackingOUTFieldName :selected').text();
        var activeFieldInfoVal = parseInt($('#TrackingACTIVEFieldName :selected').val());
        var activeFieldInfoText = $('#TrackingACTIVEFieldName :selected').text();
        var requestableInfoVal = parseInt($('#TrackingRequestableFieldName :selected').val());
        var requestableInfoText = $('#TrackingRequestableFieldName :selected').text();
        var inactiveStorageVal = parseInt($('#InactiveLocationField :selected').val());
        var inactiveStorageText = $('#InactiveLocationField :selected').text();
        var archiveFieldVal = parseInt($('#ArchiveLocationField :selected').val());
        var archiveFieldText = $('#ArchiveLocationField :selected').text();
        var signatureFieldVal = parseInt($('#SignatureRequiredFieldName :selected').val());
        var signatureFieldText = $('#SignatureRequiredFieldName :selected').text();
        var autoAssignStatus = ($('#AutoAssignOnAdd').is(':checked')) && !($('#AutoAssignOnAdd').is(':disabled'));
        var destinationVar = $('#DefaultTrackingId').val();
        if (statusFieldInfo !== null)
            statusFieldInfo = $('#TrackingStatusFieldName').val();
        else
            statusFieldInfo = null;

        if (cLevelInfo > 0 && statusFieldInfo == "") {
            showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingTrackingStatusReq"], "w");
            return false;
        }
        if (statusFieldInfo !== null) {
            var firstCharCode = statusFieldInfo.charCodeAt(0);
            if ((48 <= firstCharCode && firstCharCode <= 57) || (firstCharCode == 95)) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingFieldCantBeginWithUScore"], "w");
                return false;
            }
        }

        if ((!outFieldStatus) && (cLevelInfo > 0) && (outFieldInfoVal == 0 && outFieldInfoText.trim() == "{No Out Field}")) {
            showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingOutFldMustSelected"], "w");
            return false;
        }
        if ((outTypeInfoVal == 0) && (outFieldInfoText.trim() !== "{No Out Field}")) {
            if (activeFieldInfoText.trim() !== "{No Active Field}") {
                if ($('#TrackingACTIVEFieldName').val() == $('#TrackingOUTFieldName').val()) {
                    showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingOutAndActiveMustDiff"], "w");
                    return false;
                }
            }
            if (requestableInfoText.trim() !== "{No Requestable Field}") {
                if ($('#TrackingRequestableFieldName').val() == $('#TrackingOUTFieldName').val()) {
                    showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingOutAndReqMustDiff"], "w");
                    return false;
                }
            }
            if (inactiveStorageText.trim() !== "{No Inactive Storage Field}") {
                if ($('#InactiveLocationField').val() == $('#TrackingOUTFieldName').val()) {
                    showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingOutAndInActiveMustDiff"], "w");
                    return false;
                }
            }
            if (archiveFieldText.trim() !== "{No Archive Storage Field}") {
                if ($('#ArchiveLocationField').val() == $('#TrackingOUTFieldName').val()) {
                    showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingOutAndArchiveStrgMustDiff"], "w");
                    return false;
                }
            }
            if (signatureFieldText.trim() !== "{No Signature Required Field}") {
                if ($('#SignatureRequiredFieldName').val() == $('#TrackingOUTFieldName').val()) {
                    showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingOutAndSignReqMustDiff"], "w");
                    return false;
                }
            }
        }
        if ((requestableInfoText.trim() !== "{No Requestable Field}") && (activeFieldInfoText.trim() !== "{No Active Field}")) {
            if ($('#TrackingRequestableFieldName').val() == $('#TrackingACTIVEFieldName').val()) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingTrackReqAndActiveMustDiff"], "w");
                return false;
            }
        }
        if ((inactiveStorageText.trim() !== "{No Inactive Storage Field}") && (activeFieldInfoText.trim() !== "{No Active Field}")) {
            if ($('#InactiveLocationField').val() == $('#TrackingACTIVEFieldName').val()) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingInActiveStAndActiveMustDiff"], "w");
                return false;
            }
        }
        if ((archiveFieldText.trim() !== "{No Archive Storage Field}") && (activeFieldInfoText.trim() !== "{No Active Field}")) {
            if ($('#ArchiveLocationField').val() == $('#TrackingACTIVEFieldName').val()) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingArchiveAndActiveMustDiff"], "w");
                return false;
            }
        }
        if ((signatureFieldText.trim() !== "{No Signature Required Field}") && (activeFieldInfoText.trim() !== "{No Active Field}")) {
            if ($('#SignatureRequiredFieldName').val() == $('#TrackingACTIVEFieldName').val()) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingSignReqAndActiveMustDiff"], "w");
                return false;
            }
        }
        if (( requestableInfoText.trim() !== "{No Requestable Field}") && (inactiveStorageText.trim() !== "{No Inactive Storage Field}")) {
            if ($('#TrackingRequestableFieldName').val() == $('#InactiveLocationField').val()) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingTraReqAndInActiveMustDiff"], "w");
                return false;
            }
        }
        if ((requestableInfoText.trim() !== "{No Requestable Field}") && (archiveFieldText.trim() !== "{No Archive Storage Field}")) {
            if ($('#TrackingRequestableFieldName').val() == $('#ArchiveLocationField').val()) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingTraReqAndArchiveMustDiff"], "w");
                return false;
            }
        }
        if (( signatureFieldText.trim() !== "{No Signature Required Field}") && (inactiveStorageText.trim() !== "{No Inactive Storage Field}")) {
            if ($('#SignatureRequiredFieldName').val() == $('#InactiveLocationField').val()) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingSignReqAndInActiveMustDiff"], "w");
                return false;
            }
        }
        if ((archiveFieldText.trim() !== "{No Archive Storage Field}") && (inactiveStorageText.trim() !== "{No Inactive Storage Field}")) {
            if ($('#ArchiveLocationField').val() == $('#InactiveLocationField').val()) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingInActAndArchiveStrgMustDiff"], "w");
                return false;
            }
        }
        if ((signatureFieldText.trim() !== "{No Signature Required Field}") && (archiveFieldText.trim() !== "{No Archive Storage Field}")) {
            if ($('#SignatureRequiredFieldName').val() == $('#ArchiveLocationField').val()) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingSignReqAndArchiveStrgMustDiff"], "w");
                return false;
            }
        }
        if ((signatureFieldText.trim() !== "{No Signature Required Field}") && (requestableInfoText.trim() !== "{No Requestable Field}")) {
            if ($('#SignatureRequiredFieldName').val() == $('#TrackingRequestableFieldName').val()) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingReqAndSignReqMustDiff"], "w");
                return false;
            }
        }
        if (autoAssignStatus && (destinationVar == null || destinationVar == "")) {
            showAjaxReturnMessage(vrTablesRes["msgJsTableTrackingIniTrackingDestMustSel"], "w");
            return false;
        }

        $.get(urls.TableTracking.GetTableEntity, $.param({ containerInfo: cLevelInfo, tableName: tableVar, statusFieldText: statusFieldInfo }), function (data) {
            if (data) {
                if (data.errortype == "w") {
                    showAjaxReturnMessage(data.message, data.errortype);
                    return false;
                }else{
                    if (data.errortype == "r") {
                        $(this).confirmModal({
                            confirmTitle: 'TAB FusionRMS',
                            confirmMessage: data.message,
                            confirmOk: vrCommonRes['Yes'],
                            confirmCancel: vrCommonRes['No'],
                            confirmStyle: 'default',
                            confirmCallback: RemoveStatusFieldYes,
                            confirmCallbackCancel: RemoveStatusFieldNo

                        });
                    } else if (data.errortype == "s") {
                        RemoveStatusField(false);
                    }
                }
            }
            return true;
        });
        return true;
    });
});


$('#TrackingStatusFieldName').SpecialCharactersSpaceNotAllowed();

function RemoveStatusFieldYes() {
    RemoveStatusField(true);
}

function RemoveStatusFieldNo() {
    RemoveStatusField(false);
}
function RemoveStatusField(FieldFlag) {
    var $form = $('#frmTableTracking');
    if ($form.valid()) {

        var serializedForm = $form.serialize() + "&FieldFlag=" + FieldFlag + "&pAutoAddNotification=" + $("#AutoAddNotification").is(':checked') + "&pAllowBatchRequesting=" + $("#AllowBatchRequesting").is(':checked') + "&pTrackable=" + $("#Trackable").is(':checked');

        $.ajax({
            url: urls.TableTracking.SetTableTrackingDetails,
            type: "POST",
            dataType: 'json',
            async: false,
            data:  serializedForm ,
            success: function (response) {
                if (response.warnMsgJSON !== "") {
                    $(this).confirmModal({
                        confirmTitle: 'TAB FusionRMS',
                        confirmMessage: response.warnMsgJSON,
                        confirmOk: vrCommonRes['Ok'].toUpperCase(),
                        confirmStyle: 'default',
                        confirmOnlyOk: true
                    });

                } else {
                    showAjaxReturnMessage(response.message, response.errortype);
                }
                var tableVar = $('#TableName').val();
                GetTableTrackingData(tableVar);
            }
        });
    }
}

function GetTableTrackingData(tblName) {
    if ($('#LoadTabContent').length == 0) {
        RedirectOnAccordian(urls.TableTracking.LoadTableTab);
        $('#title, #navigation').text(vrCommonRes['mnuTables']);
    }
    $.ajax({
        url: urls.TableTracking.LoadTableTracking,
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html',
        async:false
    }).done(function (result) {
         $('#LoadTabContent').empty();
         $('#LoadTabContent').html(result);
         $.post(urls.TableTracking.GetTableTrackingProperties, $.param({ tableName: tblName }), function (data) {
             if (data) {
                 if (data.errortype == 's') {
                     var pSystemObject = $.parseJSON(data.systemObject);
                      oTrackingOutOn = pSystemObject.TrackingOutOn;
                      oDateDueOutOn = pSystemObject.DateDueOn;
                     if (data.lblDestinationJSON !== "null" && data.lblDestinationJSON !== "" && data.lblDestinationJSON !== undefined) {
                         var lblDestinationJSON = $.parseJSON(data.lblDestinationJSON);
                         $('#lblDestination').text(lblDestinationJSON + ":");
                     }                   
                     signatureVar = pSystemObject.SignatureCaptureOn;
                     var containerJSONList = $.parseJSON(data.containerJSONList);
                     var tableObject = $.parseJSON(data.oneTableObj);
                     var outFieldJSONList = $.parseJSON(data.outFieldJSONList);
                     var dueBackJSONList = $.parseJSON(data.dueBackJSONList);
                     var activeFieldJSONList = $.parseJSON(data.activeFieldJSONList);
                     var emailAddressJSONList = $.parseJSON(data.emailAddressJSONList);
                     var requestFieldJSONList = $.parseJSON(data.requestFieldJSONList);
                     var inactiveJSONList = $.parseJSON(data.inactiveJSONList);
                     var archiveJSONList = $.parseJSON(data.archiveJSONList);
                     var userFieldJSONList = $.parseJSON(data.userFieldJSONList);
                     var phoneFieldJSONList = $.parseJSON(data.phoneFieldJSONList);
                     var mailStopJSONList = $.parseJSON(data.mailStopJSONList);
                     var signatureJSONList = $.parseJSON(data.signatureJSONList);
                     LoadOutField('#TrackingTable', containerJSONList);
                     LoadOutField('#TrackingOUTFieldName', outFieldJSONList);
                     LoadOutField('#TrackingDueBackDaysFieldName', dueBackJSONList);
                     LoadOutField('#TrackingACTIVEFieldName', activeFieldJSONList);
                     LoadOutField('#TrackingEmailFieldName', emailAddressJSONList);
                     LoadOutField('#TrackingRequestableFieldName', requestFieldJSONList);
                     LoadOutField('#InactiveLocationField', inactiveJSONList);
                     LoadOutField('#ArchiveLocationField', archiveJSONList);
                     LoadOutField('#OperatorsIdField', userFieldJSONList);
                     LoadOutField('#TrackingPhoneFieldName', phoneFieldJSONList);
                     LoadOutField('#TrackingMailStopFieldName', mailStopJSONList);
                     LoadOutField('#SignatureRequiredFieldName', signatureJSONList);
                     $('#TableName').val(tableObject.TableName.trim());
                     $('#TrackingTable option[value=' + tableObject.TrackingTable + ']').attr('selected', true);
                     if (tableObject.TrackingStatusFieldName !== null && tableObject.TrackingStatusFieldName !== "") {
                          StatusField = tableObject.TrackingStatusFieldName.trim();
                         $('#TrackingStatusFieldName').val(tableObject.TrackingStatusFieldName.trim());
                     }
                     else {
                         $('#TrackingStatusFieldName').val("");
                     }
                 
                     $('#OutTable option[value=' + tableObject.OutTable + ']').attr('selected', true);
                     if (tableObject.TrackingDueBackDaysFieldName !== '' && tableObject.TrackingDueBackDaysFieldName !== null)
                         $('[name=TrackingDueBackDaysFieldName] option').filter(function () {
                             return ($(this).val() == tableObject.TrackingDueBackDaysFieldName);
                         }).prop('selected', true);
                     if (tableObject.TrackingOUTFieldName !== '' && tableObject.TrackingOUTFieldName !== null)
                         $('[name=TrackingOUTFieldName] option').filter(function () {
                             return ($(this).val() == tableObject.TrackingOUTFieldName);
                         }).prop('selected', true);
                     if (tableObject.TrackingACTIVEFieldName != '' && tableObject.TrackingACTIVEFieldName !== null)
                         $('[name=TrackingACTIVEFieldName] option').filter(function () {
                             return ($(this).val() == tableObject.TrackingACTIVEFieldName);
                         }).prop('selected', true);
                     if (tableObject.TrackingEmailFieldName !== '' && tableObject.TrackingEmailFieldName !== null)
                         $('[name=TrackingEmailFieldName] option').filter(function () {
                             return ($(this).val() == tableObject.TrackingEmailFieldName);
                         }).prop('selected', true);
                     if (tableObject.TrackingRequestableFieldName !== '' && tableObject.TrackingRequestableFieldName !== null)
                         $('[name=TrackingRequestableFieldName] option').filter(function () {
                             return ($(this).val() == tableObject.TrackingRequestableFieldName);
                         }).prop('selected', true);
                     if (tableObject.TrackingPhoneFieldName !== '' && tableObject.TrackingPhoneFieldName !== null)
                         $('[name=TrackingPhoneFieldName] option').filter(function () {
                             return ($(this).val() == tableObject.TrackingPhoneFieldName);
                         }).prop('selected', true);
                     if (tableObject.InactiveLocationField !== '' && tableObject.InactiveLocationField !== null)
                         $('[name=InactiveLocationField] option').filter(function () {
                             return ($(this).val() == tableObject.InactiveLocationField);
                         }).prop('selected', true);
                     if (tableObject.TrackingMailStopFieldName !== '' && tableObject.TrackingMailStopFieldName !== null)
                         $('[name=TrackingMailStopFieldName] option').filter(function () {
                             return ($(this).val() == tableObject.TrackingMailStopFieldName);
                         }).prop('selected', true);
                     if (tableObject.ArchiveLocationField !== '' && tableObject.ArchiveLocationField !== null)
                         $('[name=ArchiveLocationField] option').filter(function () {
                             return ($(this).val() == tableObject.ArchiveLocationField);
                         }).prop('selected', true);
                     if (tableObject.OperatorsIdField !== '' && tableObject.OperatorsIdField !== null)
                         $('[name=OperatorsIdField] option').filter(function () {
                             return ($(this).val() == tableObject.OperatorsIdField);
                         }).prop('selected', true);
                     if (tableObject.SignatureRequiredFieldName !== '' && tableObject.SignatureRequiredFieldName !== null)
                         $('[name=SignatureRequiredFieldName] option').filter(function () {
                             return ($(this).val() == tableObject.SignatureRequiredFieldName);
                         }).prop('selected', true);
                     DisableOutSetup(oTrackingOutOn, tableObject.OutTable)
                     //DisableOutField(tableObject.OutTable);
                     DisableDueBackDays(tableObject.TrackingTable, tableObject.OutTable);
                     var boolSignature = (signatureVar && tableObject.TrackingTable);
                     DisableSigntureRequired(boolSignature);
                     DisableControlOnCLevel(tableObject.TrackingTable);
                     GetIntialDestination(tblName,false,false,false);
                     if (tableObject.AutoAddNotification)
                         $('#AutoAddNotification').attr('checked', true);
                     else
                         $('#AutoAddNotification').removeAttr('checked', true);
              
                     if (!(dispositionFlag && (DispositionTable == $('#tableLabel').text()))) {
                         dispositionFlag = false;
                         DispositionTable = $('#tableLabel').text();
                         DispositionType = tableObject.RetentionFinalDisposition;
                     }
                 }
                 setTimeout(function () {
                     var s = $(".sticker");
                     $('.content-wrapper').scroll(function () {
                         if ($(this).scrollTop() + $(this).innerHeight() + 60 >= $(this)[0].scrollHeight) {
                             s.removeClass("stick", 10);
                         }
                         else {
                             s.addClass("stick");
                         }
                     });
                 }, 800);
             }
             else {
                 ShowErrorMessge();
             }
         });

     })
.fail(function (xhr, status) {
    ShowErrorMessge();
});
}

function GetIntialDestination(tblName, ConfigureTransfer, TransferValue, RequestVal) {
    
    $.post(urls.TableTracking.GetTrackingDestination, $.param({ tableName: tblName, ConfigureTransfer: ConfigureTransfer, TransferValue: TransferValue, RequestVal: RequestVal }), function (response) {
        if (response) {
            if (response.errortype !== "e") {
                var bRequestPermissionJSON = $.parseJSON(response.bRequestPermissionJSON);
                var bTransferPermissionJSON = $.parseJSON(response.bTransferPermissionJSON);
                //trackableFlag = bTransferPermissionJSON;
                var tableObject = $.parseJSON(response.tableObjectJSON);
                trackableValue = bTransferPermissionJSON;
                $('#DefaultTrackingId').empty();
                if (trackableFlag == true && DispositionTableAttach == $('#tableLabel').text()) {
                    trackableFlag = true;
                    DispositionTableAttach=$('#tableLabel').text();
                    $('#Trackable').attr('checked', trackableValue);
                }
                else {
                    trackableFlag = false;
                    DispositionTableAttach =null;
                    $('#Trackable').attr('checked', trackableValue);
                }
                //Check - uncheck based on permission
                $('#AllowBatchRequesting').attr('checked', bRequestPermissionJSON);
                //if (bRequestPermissionJSON)
                //    $('#AllowBatchRequesting').attr('checked', true);
                //else
                //    $('#AllowBatchRequesting').removeAttr('checked', true);

                if (response.errortype == "w") {
                    var sNoRecordMsgJSON = $.parseJSON(response.sNoRecordMsgJSON);
                    $('#InitialListDiv').hide();
                    $('#IntialLabelDiv').show();
                    $('#noRecordLabel').text(sNoRecordMsgJSON);
                }
                else if (response.errortype == "s") {
                    $('#InitialListDiv').show();
                    $('#IntialLabelDiv').hide();
                    var sRecordJSON = $.parseJSON(response.sRecordJSON);
                    var colDataFieldJSON = $.parseJSON(response.colDataFieldJSON);
                    var col1DataFieldJSON = $.parseJSON(response.col1DataFieldJSON);
                    var col2DataFieldJSON = $.parseJSON(response.col2DataFieldJSON);
                    //var lblDestinationJSON = $.parseJSON(response.lblDestinationJSON);
                    var colVisibleJSON = $.parseJSON(response.colVisibleJSON);
                    var col1VisibleJSON = $.parseJSON(response.col1VisibleJSON);
                    var col2VisibleJSON = $.parseJSON(response.col2VisibleJSON);
                    //$('#lblDestination').text(lblDestinationJSON+":");
                    //$('#DefaultTrackingId').append("<option value=''> </option>");
                    
                    if (col1VisibleJSON == true && col2VisibleJSON == true) {
                        $('#DefaultTrackingId').append("<option value='' >" + col1DataFieldJSON + "+" + col2DataFieldJSON + "</option>");
                        $(sRecordJSON).each(function (i, item) {
                            $('#DefaultTrackingId').append("<option value='" + sRecordJSON[i][colDataFieldJSON] + "' >" + sRecordJSON[i][col1DataFieldJSON] + "+" + sRecordJSON[i][col2DataFieldJSON] + "</option>");
                        });
                    } else if (col1VisibleJSON == true && col2VisibleJSON == false) {
                        $('#DefaultTrackingId').append("<option value='' >" + col1DataFieldJSON + "+" + colDataFieldJSON + "</option>");
                        $(sRecordJSON).each(function (i, item) {
                            $('#DefaultTrackingId').append("<option value='" + sRecordJSON[i][colDataFieldJSON] + "' >" + sRecordJSON[i][col1DataFieldJSON] + "+" + sRecordJSON[i][colDataFieldJSON] + " </option>");
                        });
                    } else if (col1VisibleJSON == false && col2VisibleJSON == true) {
                        $('#DefaultTrackingId').append("<option value='' >" + colDataFieldJSON + "+" + col2DataFieldJSON + "</option>");
                        $(sRecordJSON).each(function (i, item) {
                            $('#DefaultTrackingId').append("<option value='" + sRecordJSON[i][colDataFieldJSON] + "' >" + sRecordJSON[i][colDataFieldJSON] + "+" + sRecordJSON[i][col2DataFieldJSON] + "</option>");
                        });
                    } else {
                        $('#DefaultTrackingId').append("<option value='' >" + colDataFieldJSON + "+" + "</option>");
                        $(sRecordJSON).each(function (i, item) {
                            $('#DefaultTrackingId').append("<option value='" + sRecordJSON[i][colDataFieldJSON] + "'>[" + sRecordJSON[i][colDataFieldJSON] + "] </option>");
                        });
                    }

                    var spacesToAdd = 5;
                    var firstLength = secondLength = 0;
                    $("select#DefaultTrackingId option").each(function (i, v) {
                            var parts = $(this).text().split('+');
                            var len = parts[0].length;
                            if (len > firstLength) {
                                firstLength = len;
                            }

                            if (parts[1]) {
                                var len1 = parts[1].length;
                                if (len1 > secondLength) {
                                    secondLength = len1;
                                }
                            }
                    });

                    var padLength = firstLength + spacesToAdd;
                    var padLength1 = secondLength + spacesToAdd;
                    
                    $("select#DefaultTrackingId option").each(function (i, v) {
                        var parts = $(this).text().split('+');
                        var strLength = parts[0].length;

                        for (var x = 0; x < (padLength - strLength) ; x++) {
                            parts[0] = parts[0] + ' ';
                        }

                        if (parts[1]) {
                            $(this).text(parts[0].replace(/ /g, '\u00a0') + parts[1]);
                        } else {
                            $(this).text(parts[0].replace(/ /g, '\u00a0'));
                        }

                    });

                    if (tableObject.DefaultTrackingId !== null) {
                        $('#DefaultTrackingId option[value=' + tableObject.DefaultTrackingId + ']').attr('selected', true);
                        $('#AutoAssignOnAdd').attr('checked', 'checked');
                    } else {
                        $('#DefaultTrackingId option:first').attr('selected', true);
                        $('#AutoAssignOnAdd').removeAttr('checked', 'checked');
                    }
                }
                var autoAssignVar=$('#AutoAssignOnAdd').is(':checked');
                if ($('#DefaultTrackingId').length == 0)
                    DisableAllowRequest(bTransferPermissionJSON, autoAssignVar, false);
                else 
                    DisableAllowRequest(bTransferPermissionJSON, autoAssignVar, true);
            }
        } else {
            ShowErrorMessge();
        }

    });
}

function DisableAllowRequest(trackableObj,autoAssignVar, destinationVar)
{
    if (trackableObj) {
            $('#AllowBatchRequesting').removeAttr('disabled', 'disabled');
            $('#AutoAssignOnAdd').removeAttr('disabled', 'disabled');
        }
    else {
        //$('#AllowBatchRequesting').attr('checked', false);
            $('#AllowBatchRequesting').attr('disabled', 'disabled');
            $('#AutoAssignOnAdd').attr('disabled', 'disabled');
            $('#AutoAssignOnAdd').attr('checked', false);
        }
            DisableAutoTracking(trackableObj, destinationVar, autoAssignVar);
}

function DisableAutoTracking(trackableObj, destinationVar, autoAssignVar)
{
    if (trackableObj && destinationVar && autoAssignVar) {
        $('#DefaultTrackingId').removeAttr('disabled', 'disabled');
        $('#AutoAddNotification').removeAttr('disabled', 'disabled');
    }
    else {
        $('#DefaultTrackingId').attr('disabled', 'disabled');
        $('#AutoAddNotification').attr('disabled', 'disabled');
        $('#AutoAddNotification').attr('checked', false);
    }
}

function DisableSigntureRequired(boolSignature) {
    if (boolSignature)
        $('#SignatureRequiredFieldName').removeAttr('disabled','disabled');
    else
        $('#SignatureRequiredFieldName').attr('disabled', 'disabled');
}

function DisableDueBackDays(containerLevel, OutTypeVar) {
    if (oTrackingOutOn == true && oDateDueOutOn == true) {
        if ((containerLevel > 0) && ((OutTypeVar == 0) || (OutTypeVar == 1)))
            $('#TrackingDueBackDaysFieldName').removeAttr('disabled', 'disabled');
        else
            $('#TrackingDueBackDaysFieldName').attr('disabled', 'disabled');
    } else {
        $('#TrackingDueBackDaysFieldName').attr('disabled', 'disabled');
    }
}

function DisableOutField(OutTypeVar) {
    if (OutTypeVar == 0 || OutTypeVar == null) {
        $('#TrackingOUTFieldName').removeAttr('disabled', 'disabled');
    }
    else {
        $('#TrackingOUTFieldName').attr('disabled', 'disabled');
    }

}

function DisableControlOnCLevel(containerLevel) {
    switch (containerLevel) {
        case 0:
            $('#Trackable').removeAttr('disabled', 'disabled');
            $('#TrackingStatusFieldName').val('');
            $('#TrackingRequestableFieldName').attr('disabled', 'disabled');
            $('#InactiveLocationField').attr('disabled', 'disabled');
            $('#ArchiveLocationField').attr('disabled', 'disabled');
            $('#TrackingPhoneFieldName').attr('disabled', 'disabled');
            $('#TrackingMailStopFieldName').attr('disabled', 'disabled');
            $('#OperatorsIdField').attr('disabled', 'disabled');
            $('#TrackingStatusFieldName').attr('disabled', 'disabled');
            break;
        case 1:
            $('#Trackable').attr('checked', false);
            $('#Trackable').attr('disabled', 'disabled');
            $('#TrackingRequestableFieldName').removeAttr('disabled', 'disabled');
            $('#InactiveLocationField').removeAttr('disabled', 'disabled');
            $('#ArchiveLocationField').removeAttr('disabled', 'disabled');
            $('#TrackingPhoneFieldName').attr('disabled', 'disabled');
            $('#TrackingMailStopFieldName').attr('disabled', 'disabled');
            $('#OperatorsIdField').attr('disabled', 'disabled');
            $('#TrackingStatusFieldName').removeAttr('disabled', 'disabled');
            var trackableObj = $('#Trackable').is(':checked');
            var destinationVar = $('#DefaultTrackingId option').length;
            var autoAssignVar = $('#AutoAssignOnAdd').is(':checked');
            DisableAllowRequest(trackableObj, autoAssignVar, destinationVar);
            break;
        case 2:
            $('#Trackable').removeAttr('disabled', 'disabled');
            $('#TrackingRequestableFieldName').attr('disabled', 'disabled');
            $('#InactiveLocationField').attr('disabled', 'disabled');
            $('#ArchiveLocationField').attr('disabled', 'disabled');
            $('#TrackingPhoneFieldName').removeAttr('disabled', 'disabled');
            $('#TrackingMailStopFieldName').removeAttr('disabled', 'disabled');
            $('#OperatorsIdField').removeAttr('disabled', 'disabled');
            $('#TrackingStatusFieldName').removeAttr('disabled', 'disabled');
            break;
        default:
            $('#TrackingRequestableFieldName').attr('disabled', 'disabled');
            $('#InactiveLocationField').attr('disabled', 'disabled');
            $('#ArchiveLocationField').attr('disabled', 'disabled');
            $('#TrackingPhoneFieldName').attr('disabled', 'disabled');
            $('#TrackingMailStopFieldName').attr('disabled', 'disabled');
            $('#OperatorsIdField').attr('disabled', 'disabled');
            $('#TrackingStatusFieldName').removeAttr('disabled', 'disabled');
            $('#Trackable').removeAttr('disabled', 'disabled');
            break;
    }
}

function DisableOutSetup(TrackingOutOn, IsOutField) {
    if (TrackingOutOn == true) {
        $("#OutSetupDiv").removeAttr('disabled', 'disabled');
        DisableOutField(IsOutField);
        $('#DisabledOutId').hide();
    } else {
        $("#OutSetupDiv").attr('disabled', 'disabled');
        $('#DisabledOutId').show();
    }

}


//Break the line in label
$.fn.multiline = function (text) {
    this.text(text);
    this.html(this.html().replace(/\\n/g, '<br/>'));
    return this;
}//Break the line in label

var miOfficialRecordConversion = 0;
$(function () {

    $('#BarCodePrefix').keypress(function (event) {
        var key = event.which;
        if (!((key >= 65 && key <= 90) || (key >= 97 && key <= 122) || (key == 8) || (key != 9) || (event.keyCode == 37) || (event.keyCode == 38) || (event.keyCode == 39) || (event.keyCode == 40))) {
            event.preventDefault();
            return true;
        }
        if (key >= 97 && key <= 122) {
            event.preventDefault();
            key = key - 32;
            var barCode = $('#BarCodePrefix').val();
            if (barCode.length < 10) {
                $("#BarCodePrefix").val($('#BarCodePrefix').val() + String.fromCharCode(key));
                return true;
            }
            return false;
        } else {
            return false;
        }
    });

    $('#OfficialRecordHandling').on('change', function () {
        var officialRecord = $('#OfficialRecordHandling').is(':checked');
        var SelectedTable = $('#TableName').val();
        $.post(urls.TableGeneral.OfficialRecordWarning, $.param({ recordStatus: officialRecord, tableName: SelectedTable }), function (data) {
            if (data) {
                if (data.errortype == "w") {
                    if (officialRecord == true) {
                        $('#warningLabel').multiline(data.message);
                        $('#fourTrueBtn').show();
                        $('#threeFalseBtn').hide();
                        $('#mdlOfficialRecord').ShowModel();
                        $('#btnFirst').on('click', function () {
                            miOfficialRecordConversion = 1;
                        });
                        $('#btnLast').on('click', function () {
                            miOfficialRecordConversion = 2;
                        });
                        $('#btnNotSet').on('click', function () {
                            miOfficialRecordConversion = 0;
                        });
                        $('#btnCancel').on('click', function () {
                            $('#OfficialRecordHandling').attr('checked', true);
                        });

                    }
                    else {
                        $('#warningLabel').multiline(data.message);
                        $('#fourTrueBtn').hide();
                        $('#threeFalseBtn').show();
                        $('#mdlOfficialRecord').ShowModel();
                        $('#btnRemove').on('click', function () {
                            miOfficialRecordConversion = 4;
                        });
                        $('#btnCancel').on('click', function () {
                            $('#OfficialRecordHandling').attr('checked', false);
                        });
                    }

                }
            }
        });

    });

    $('#BarCodePrefix').bind('cut paste', function (e) {
        e.preventDefault();
    });

    $('#AttachmentCheck').on('change', function () {
        var attachmentState = $('#AttachmentCheck').is(':checked');
        DisableAttachment(attachmentState);
    });

    ValidateNumeric('#DescFieldPrefixOneWidth');
    ValidateNumeric('#DescFieldPrefixTwoWidth');
    ValidateNumeric('#ADOQueryTimeout');
    ValidateNumeric('#ADOCacheSize');
    ValidateNumeric('#MaxRecsOnDropDown');

    $('#btnChange').on('click', function () {
        var SelectedTable = $('#TableName').val();
        $('#mdlIconSetting').ShowModel();
        $.post(urls.TableGeneral.LoadIconWindow, { TableName: SelectedTable }, function (data) {
            if (data) {
                if (data.errortype == "s") {
                    var IconListJSON = $.parseJSON(data.IconListJSON);
                    var PictureNameJSON = $.parseJSON(data.PictureNameJSON);
                    if (PictureNameJSON == null || PictureNameJSON == "")
                        PictureNameJSON = "FOLDERS.ICO";
                    $("#selectable").empty();
                    $(IconListJSON).each(function (i, item) {
                        if (item.Value.toLowerCase().trim() == PictureNameJSON.toLowerCase().trim())
                            $("#selectable").append("<li id=" + item.Value + " class='highlightClass'><a class='dd-option'><img class='dd-option-image' src=" + item.Key + ">&nbsp;&nbsp;&nbsp;&nbsp;" + item.Value + "</a></li>");
                        else
                            $("#selectable").append("<li id=" + item.Value + "><a class='dd-option'><img class='dd-option-image' src=" + item.Key + ">&nbsp;&nbsp;&nbsp;&nbsp;" + item.Value + "</a></li>");
                    });

                } else {
                    showAjaxReturnMessage(data.message, data.errortype);
                }
            }
        });


        $("#selectable").selectable({
            selected: function (event, ui) {
                if ($(ui.selected).hasClass('ui-selected')) {
                    $("#selectable li.highlightClass").removeClass('highlightClass');
                    $(ui.selected).closest('li').addClass('highlightClass');
                } else {
                    $(ui.selected).closest('li').removeClass('highlightClass');
                }
            }
        });
    });

    $('#btnSave').on('click', function () {
        var pictureId = $('.highlightClass').attr('id');
        if (pictureId != undefined) {
            var imgPath = "/Images/icons/" + pictureId;
            $("#iconDefault").attr('src', imgPath);
            $('#Picture').val(pictureId);
        }

    });

    $('#btnApply').on('click', function () {
        var $form = $('#frmTableGeneral');
        if ($form.valid()) {
            var serverCursor = $('input:radio[name=ADOServerCursor]:checked').val();
            var maxRecords = parseInt($('#MaxRecsOnDropDown').val());
            if (maxRecords < 100 && maxRecords !== 0) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableGeneralMaxDrpDwnRecs100OrHigh"], "w");
                return false;
            }
            //debugger;
            //var maxSQLIntVal = 2147483647;
            //var maxRecords = parseInt($('#ADOQueryTimeout').val());
            //if(maxRecords > maxSQLIntVal)
            //{
            //    showAjaxReturnMessage("The value should be between 0 to 2,147,483,647", "w");
            //    return false;
            //}

            if (serverCursor == 'client')
                $('input:radio[name=ADOServerCursor]:checked').val(false);
            else
                $('input:radio[name=ADOServerCursor]:checked').val(true);
            var serializedForm = $form.serialize() + "&Attachments=" + $('#AttachmentCheck').is(':checked') + "&miOfficialRecord=" + miOfficialRecordConversion;
            $.post(urls.TableGeneral.SetGeneralDetails, serializedForm)
                .done(function (response) {
                    if (response.warnMsgJSON !== "") {
                        $(this).confirmModal({
                            confirmTitle: 'TAB FusionRMS',
                            confirmMessage: response.warnMsgJSON,
                            confirmOk: vrCommonRes['Ok'].toUpperCase(),
                            confirmStyle: 'default',
                            confirmOnlyOk: true
                        });
                    } else {
                        showAjaxReturnMessage(response.message, response.errortype);
                    }
                    var searchValue = $.parseJSON(response.searchValueJSON);
                    $.post(urls.TableGeneral.SetSearchOrder, function (data) {
                        if (data) {
                            if (data.errortype == "s") {
                                var searchOrderList = $.parseJSON(data.searchOrderListJSON);
                                LoadOutField('#SearchOrder', searchOrderList);
                                $('#SearchOrder option[value=' + searchValue + ']').attr('selected', true);
                            }
                        }
                    });
                    // showAjaxReturnMessage(response.message, response.errortype);
                })
                .fail(function (xhr, status, error) {
                    ShowErrorMessge();
                });
        }
        return true;
    });

});



function GetTableGeneralData(tblName) {
    if ($('#divTableTab').length == 0) {
        RedirectOnAccordian(urls.TableTracking.LoadTableTab);
        $('#title, #navigation').text(vrCommonRes['mnuTables']);
    }
    $.ajax({
        url: urls.TableGeneral.LoadGeneralTab,
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html',
        async: false
    }).done(function (result) {
        $('#LoadTabContent').html('');
        $('#LoadTabContent').html(result);
        $.post(urls.TableGeneral.GetGeneralDetails, $.param({ tableName: tblName }), function (data) {
            if (data) {
                if (data.errortype == 's') {
                    var cursorFlag = $.parseJSON(data.cursorFlagJSON);
                    var auditflag = $.parseJSON(data.auditflagJSON);
                    var tableGeneralData = $.parseJSON(data.pSelectTableJSON);
                    var displayFieldList = $.parseJSON(data.displayFieldListJSON);
                    var ServerPath = $.parseJSON(data.ServerPathJSON);
                    var DBUserNameJSON = $.parseJSON(data.DBUserNameJSON);

                    var UserTableIcon = $.parseJSON(data.UserTableIconJSON);
                    //Added by Hasmukh for fix : FUS-2317
                    $("#hdnTableId").val(tableGeneralData.TableId);

                    if (UserTableIcon == "false")
                        $("#UserTableIcon").hide();
                    else
                        $("#UserTableIcon").show();

                    LoadOutField('#DescFieldNameOne', displayFieldList);
                    LoadOutField('#DescFieldNameTwo', displayFieldList);
                    $("#DescFieldNameOne").prepend("<option value='' selected='selected'></option>");
                    $("#DescFieldNameTwo").prepend("<option value='' selected='selected'></option>");
                    if (tableGeneralData.TableName !== null && tableGeneralData.TableName !== '')
                        $('#TableName').val(tableGeneralData.TableName.trim());
                    if (DBUserNameJSON !== null && DBUserNameJSON !== '')
                        $('#DBName').val(DBUserNameJSON.trim());
                    if (tableGeneralData.BarCodePrefix !== null && tableGeneralData.BarCodePrefix !== '')
                        $('#BarCodePrefix').val(tableGeneralData.BarCodePrefix.trim());
                    if (tableGeneralData.IdStripChars !== null && tableGeneralData.IdStripChars !== '')
                        $('#IdStripChars').val(tableGeneralData.IdStripChars.trim());
                    else
                        $('#IdStripChars').val("");
                    if (tableGeneralData.IdMask !== null && tableGeneralData.IdMask !== '')
                        $('#IdMask').val(tableGeneralData.IdMask.trim());
                    else
                        $('#IdMask').val("");
                    if (tableGeneralData.DescFieldPrefixOne !== null && tableGeneralData.DescFieldPrefixOne !== '')
                        $('#DescFieldPrefixOne').val(tableGeneralData.DescFieldPrefixOne.trim());
                    else
                        $('#DescFieldPrefixOne').val("");
                    if (tableGeneralData.DescFieldPrefixTwo !== null && tableGeneralData.DescFieldPrefixTwo !== '')
                        $('#DescFieldPrefixTwo').val(tableGeneralData.DescFieldPrefixTwo.trim());
                    else
                        $('#DescFieldPrefixTwo').val("");
                    if (tableGeneralData.DescFieldPrefixOneWidth !== null)
                        $('#DescFieldPrefixOneWidth').val(tableGeneralData.DescFieldPrefixOneWidth);
                    else
                        $('#DescFieldPrefixOneWidth').val("");
                    if (tableGeneralData.DescFieldPrefixTwoWidth !== null)
                        $('#DescFieldPrefixTwoWidth').val(tableGeneralData.DescFieldPrefixTwoWidth);
                    else
                        $('#DescFieldPrefixTwoWidth').val("");
                    if (tableGeneralData.Attachments)
                        $('#AttachmentCheck').attr('checked', true);
                    else
                        $('#AttachmentCheck').attr('checked', false);
                    if (tableGeneralData.OfficialRecordHandling)
                        $('#OfficialRecordHandling').attr('checked', true);
                    else
                        $('#OfficialRecordHandling').attr('checked', false);
                    if (tableGeneralData.CanAttachToNewRow)
                        $('#CanAttachToNewRow').attr('checked', true);
                    else
                        $('#CanAttachToNewRow').attr('checked', false);
                    if (tableGeneralData.Picture !== null && tableGeneralData.Picture !== '')
                        $('#Picture').val(tableGeneralData.Picture.trim());
                    if (tableGeneralData.ADOQueryTimeout !== null)
                        $('#ADOQueryTimeout').val(tableGeneralData.ADOQueryTimeout);
                    else
                        $('#ADOQueryTimeout').val("");
                    if (tableGeneralData.ADOCacheSize !== null)
                        $('#ADOCacheSize').val(tableGeneralData.ADOCacheSize);
                    else
                        $('#ADOCacheSize').val("");
                    if (tableGeneralData.MaxRecsOnDropDown !== null)
                        $('#MaxRecsOnDropDown').val(tableGeneralData.MaxRecsOnDropDown);
                    else
                        $('#MaxRecsOnDropDown').val("");
                    if (tableGeneralData.ADOServerCursor)
                        $('input:radio[name=ADOServerCursor][value=server]').attr('checked', 'checked');
                    else
                        $('input:radio[name=ADOServerCursor][value=client]').attr('checked', 'checked');
                    if (tableGeneralData.AuditAttachments)
                        $('#AuditAttachments').attr('checked', true);
                    else
                        $('#AuditAttachments').attr('checked', false);
                    if (tableGeneralData.AuditConfidentialData)
                        $('#AuditConfidentialData').attr('checked', true);
                    else
                        $('#AuditConfidentialData').attr('checked', false);
                    if (tableGeneralData.AuditUpdate)
                        $('#AuditUpdate').attr('checked', true);
                    else
                        $('#AuditUpdate').attr('checked', false);
                    if (tableGeneralData.Picture !== null && tableGeneralData.Picture !== '')
                        $('#iconDefault').attr('src', ServerPath + tableGeneralData.Picture);
                    else
                        $('#iconDefault').attr('src', ServerPath + 'FOLDERS.ICO');
                    if (tableGeneralData.DescFieldNameOne !== null && tableGeneralData.DescFieldNameOne !== '')
                        $('#DescFieldNameOne option[value=' + tableGeneralData.DescFieldNameOne + ']').attr('selected', true);
                    if (tableGeneralData.DescFieldNameTwo !== null && tableGeneralData.DescFieldNameTwo !== '')
                        $('#DescFieldNameTwo option[value=' + tableGeneralData.DescFieldNameTwo + ']').attr('selected', true);
                    if (cursorFlag)
                        $('#cursor_location *').prop("disabled", true);
                    else
                        $('#cursor_location *').prop("disabled", false);
                    if (auditflag)
                        $('#AuditConfidentialData').attr('disabled', false);
                    else
                        $('#AuditConfidentialData').attr('disabled', true);
                    DisableAttachment(tableGeneralData.Attachments);

                    $.post(urls.TableGeneral.SetSearchOrder, function (data) {
                        if (data) {
                            if (data.errortype == "s") {
                                var searchOrderList = $.parseJSON(data.searchOrderListJSON);
                                LoadOutField('#SearchOrder', searchOrderList);
                                if (tableGeneralData.SearchOrder !== null)
                                    $('#SearchOrder option[value=' + tableGeneralData.SearchOrder + ']').attr('selected', true);
                            }
                        }
                    });
                    setTimeout(function () {
                        var s = $(".sticker");
                        $('.content-wrapper').scroll(function () {
                            if ($(this).scrollTop() + $(this).innerHeight() + 40 >= $(this)[0].scrollHeight) {
                                s.removeClass("stick", 10);
                            }
                            else {
                                s.addClass("stick");
                            }
                        });
                    }, 800);
                }
                else {
                    showAjaxReturnMessage(data.message, data.errortype);
                }
            }
        });
    })
.fail(function (xhr, status) {
    ShowErrorMessge();
});
}

function ValidateNumeric(fieldId) {
    $(fieldId).keypress(function (event) {
        if (event.keyCode != 37 && event.keyCode != 39 && event.keyCode != 38 && event.keyCode != 40 && event.keyCode != 9) {
            if ((event.which != 8 && isNaN(String.fromCharCode(event.which))) || (event.which == 32)) {
                event.preventDefault();
                return false;
            }
            return true;
        } else {
            return false;
        }
    });
    $(fieldId).bind('cut copy paste', function (e) {
        e.preventDefault();
    });
}


function DisableAttachment(attachmentState) {

    if (attachmentState) {
        $('#OfficialRecordHandling').removeAttr('disabled');
        $('#CanAttachToNewRow').removeAttr('disabled');
        $('#AuditAttachments').removeAttr('disabled');
    }
    else {
        $('#OfficialRecordHandling').attr('disabled', 'disabled');
        $('#CanAttachToNewRow').attr('disabled', 'disabled');
        $('#AuditAttachments').attr('disabled', 'disabled');
    }
}

$(function () {
    $('#grdTablesField').jqGrid('setLabel', 'Field_Name', 'Field Name', { 'text-align': 'left' });
});

//JQuery Code for Print Function

// Create a jquery plugin that prints the given element.
jQuery.fn.print = function (pTableName) {
    //alert("Table Name: "+pTableName);
    // NOTE: We are trimming the jQuery collection down to the
    // first element in the collection.
    if (this.length > 1) {
        this.eq(0).print();
        return;
    } else if (!this.length) {
        return;
    }
    // ASSERT: At this point, we know that the current jQuery
    // collection (as defined by THIS), contains only one
    // printable element.

    // Create a random name for the print frame.
    var strFrameName = ("printer-" + (new Date()).getTime());

    // Create an iFrame with the new name.
    var jFrame = $("<iframe name='" + strFrameName + "'>");

    // Hide the frame (sort of) and attach to the body.
    jFrame
    .css("width", $(document).width())
    .css("height", "0px")
    .css("position", "absolute")
    .css("bottom", "0px")
    .css("left", "0px")
    .appendTo($("body:first"))
    ;

    // Get a FRAMES reference to the new frame.
    var objFrame = window.frames[strFrameName];

    // Get a reference to the DOM in the new frame.
    var objDoc = objFrame.document;

    // Grab all the style tags and copy to the new
    // document so that we capture look and feel of
    // the current document.

    // Create a temp document DIV to hold the style tags.
    // This is the only way I could find to get the style
    // tags into IE.
    var jStyleDiv = $("<div>").append(
    $("style").clone()
    );

    jStyleDiv = $("<div style=' border-radius: 25px;'>").append("<center><p><h2>TAB FusionRMS</b></h2></center>")
                .append("<center><p><u><b>" + String.format(vrTablesRes["tiJsTableFieldFieldListing"], pTableName) + "</b></u></p></center>");

    //alert(this.html());
    //$('.exclude_pager').remove();
    var htmlData = this.html();
    // Write the HTML for the document. In this, we will
    // write out the HTML of the current element.
    objDoc.open();
    objDoc.write("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
    objDoc.write("<html>");
    objDoc.write("<body>");
    objDoc.write("<head>");
    objDoc.write("<title>");
    objDoc.write(document.title);
    objDoc.write("</title>");
    objDoc.write(jStyleDiv.html());
    objDoc.write("</head>");
    objDoc.write(this.html());
    objDoc.write("</body>");
    objDoc.write("</html>");
    objDoc.close();

    $(objDoc).find(".ui-jqgrid-title").remove();
    $(objDoc).find("input:checkbox").remove();
    $(objDoc).find("th").css("text-align", "left");

    // Print the document.    
    objFrame.focus();
    objFrame.print();

    // Have the frame remove itself in about a minute so that
    // we don't build up too many of these frames.

    //Modified by Hemin for Bug Fixes on 11/02/2016
    //setTimeout(
    //function () {
    //    jFrame.remove();
    //},
    //(60 * 1000)
    //);
    jFrame.remove();
}
//End Of Print Function

//Code For File Room Order Grid.
jQuery.fn.gridLoadFields = function (getUrl, caption, tableName) {
    var gridobject = $(this);
    //alert("Table Name in gridLoadFileRoomOrder: "+tableName);

    $.post(urls.Common.GetGridViewSettings, { pGridName: "grdTablesField" })
                .done(function (response) {
                    BindFieldsGrid(gridobject, getUrl, $.parseJSON(response), caption, tableName);
                })
                .fail(function (xhr, status, error) {
                    ShowErrorMessge();
                });
}

jQuery.fn.getSelectedRowsIds = function () {
    var selectedrows = $(this).jqGrid('getGridParam', 'selarrrow');
    return selectedrows;
};

jQuery.fn.refreshJqGrid = function () {
    $(this).trigger('reloadGrid');
};

jQuery.fn.setGridColumnsOrder = function () {
    var pDatabaseGridName = $(this).attr('id');
    $.post(urls.Common.SetGridOrders, { pGridName: pDatabaseGridName })
                .done(function (response) {
                    //if (!response.MSGWarnning) {

                    //}
                    //showAjaxReturnMessage(response.message, (response.MSGWarnning ? 'w' : 's'));
                })
                .fail(function (xhr, status, error) {
                    //ShowErrorMessge();
                });
};

function BindFieldsGrid(gridobject, getUrl, arrColSettings, caption, tableName) {
    //var IsCheckbox = true;
    var pDatabaseGridName = gridobject.attr('id');
    var pPagerName = '#' + pDatabaseGridName + '_pager';

    $("#filterButton").click(function (event) {
        event.preventDefault();
        filterGrid(gridobject);
    });

    var arryDisplayCol = [];
    var arryColSettings = [];
    var globalColumnOrder = [];

    arrColSettings.sort(function (a, b) {
        if (a.srno < b.srno) return -1;
        if (b.srno < a.srno) return 1;
        return 0;
    });
    //$("#1").attr("disabled", "disabled");
    for (var i = 0; i < arrColSettings.length; i++) {
        arryDisplayCol.push([arrColSettings[i].displayName]);
        globalColumnOrder.push([arrColSettings[i].srno]);
        if (arrColSettings[i].srno == -1) {
            arryColSettings.push({ key: true, hidden: true, name: arrColSettings[i].name, index: arrColSettings[i].name, sortable: arrColSettings[i].sortable, align: 'left' });
        }
        else {
            arryColSettings.push({ key: false, name: arrColSettings[i].name, index: arrColSettings[i].name, sortable: arrColSettings[i].sortable, align: 'left' });
        }
    }

    gridobject.jqGrid({
        url: getUrl,
        datatype: 'json',
        mtype: 'Get',
        //data: gridData,
        postData: { pTableName: tableName },
        colNames: arryDisplayCol,
        colModel: arryColSettings,
        pager: jQuery(pPagerName),
        rowNum: 20,
        rowList: [20, 40, 80, 100],
        height: '100%',
        viewrecords: true,
        loadonce: false,
        grouping: false,
        caption: caption,
        onSelectRow: function (rowid, status, e) {
            //if(rowid == 1)
            //{
            //    $("#gridEdit").attr("disabled", "disabled");
            //    $("#gridRemove").attr("disabled", "disabled");
            //}
            //else
            //{
            //    $("#gridEdit").removeAttr("disabled", "disabled");
            //    $("#gridRemove").removeAttr("disabled", "disabled");
            //}
        },
        emptyrecords: vrCommonRes["NoRecordsToDisplay"],
        jsonReader: {
            root: "rows",
            page: "page",
            total: "total",
            records: "records",
            repeatitems: false,
            Id: "0"
        },
        autowidth: true,
        shrinkToFit: true,
        //width: $('#MainWrapper').width(),
        multiselect: true//IsCheckbox//,
    }).navGrid(pPagerName, { edit: false, add: false, del: false, search: false, refresh: true, refreshtext: vrCommonRes["Refresh"] }
    );//.trigger("reloadGrid", [{ current: true, page: 1 }]);

    //display select all checkbox in Fields grid
    $('#jqgh_grdTablesField_cb').hide();
}

function filterGrid(grid) {
    var postDataValues = grid.jqGrid('getGridParam', 'postData');
    $(".filterItem").each(function (index, item) {
        postDataValues[$(item).attr('id')] = $(item).val();
    });
    grid.jqGrid().setGridParam({ postData: postDataValues, page: 1 }).trigger('reloadGrid');
}

function sortResults(arr, prop, asc) {
    arr = arr.sort(function (a, b) {
        if (asc) return (a[prop] > b[prop]) ? 1 : ((a[prop] < b[prop]) ? -1 : 0);
        else return (b[prop] > a[prop]) ? 1 : ((b[prop] < a[prop]) ? -1 : 0);
    });
    return arr;
}

jQuery.fn.sort = function () {
    return this.pushStack([].sort.apply(this, arguments), []);
};
//End Grid Code.


function LoadFieldsView(TableName) {
    if ($('#LoadTabContent').length == 0) {
        RedirectOnAccordian(urls.TableTracking.LoadTableTab);
        $('#title, #navigation').text(vrCommonRes['mnuTables']);
    }
    $.ajax({
        url: urls.TableFields.LoadFieldsTab,
        contentType: 'application/html; charset=utf-8',
        type: 'GET',
        dataType: 'html'
    }).done(function (result) {
        $('#LoadTabContent').empty();
        $('#LoadTabContent').html(result);

        var Operation = "";

        //$("#grdFileRoomOrder").jqGrid("unload");
        //Modified by Hemin
        //$("#grdTablesField").gridLoadFields(urls.TableFields.LoadFieldData, "Fields", TableName);
        $("#grdTablesField").gridLoadFields(urls.TableFields.LoadFieldData, vrTablesRes["mnuTableTabPartialFields"], TableName);

        $("#gridAdd").off().on('click', function () {
            Operation = "ADD";
            ShowFieldsPopup(TableName, Operation);
        });

        $("#gridEdit").off().on('click', function () {
            Operation = "EDIT";
            ShowFieldsPopup(TableName, Operation);
        });

        $("#gridRemove").off().on('click', function () {
            //urls.TableFields.RemoveFieldFromTable
            var selRowId = $("#grdTablesField").jqGrid('getGridParam', 'selrow');
            var celValue = $("#grdTablesField").jqGrid('getCell', selRowId, 'Field_Name');

            var selectedrows = $("#grdTablesField").getSelectedRowsIds();

            if (selectedrows.length > 1 || selectedrows.length == 0) {
                showAjaxReturnMessage(vrCommonRes["msgSelectOneRowToRemove"], 'w');
                return;
            }
            else {

                if (selRowId == 1) {
                    showAjaxReturnMessage(vrTablesRes["msgJsTableFieldSrryUCantRemvField"], 'w');
                    return;
                }
                else {
                    $.post(urls.TableFields.CheckBeforeRemoveFieldFromTable, $.param({ pTableName: TableName, pFieldName: celValue }, true), function (data) {
                        //bDeleteIndexes
                        //$("#grdTablesField").refreshJqGrid();
                        //showAjaxReturnMessage(data.message, data.errortype);
                        var combinval = TableName + "#" + celValue + "#" + data.bDeleteIndexes;
                        //console.log("Combine value in CheckBeforeRemoveFieldFromTable: "+combinval);
                        if (data.bDeleteIndexes == true) {
                            //show error message here.
                            $(this).confirmModal({
                                confirmTitle: 'TAB FusionRMS',
                                confirmMessage: data.message,
                                confirmOk: vrCommonRes['Ok'],
                                confirmStyle: 'default',
                                confirmOnlyOk: true
                            });
                        }
                        else {
                            //    //Show error msg and show confirmation box also.
                            $(this).confirmModal({
                                confirmTitle: 'TAB FusionRMS',
                                confirmMessage: data.message,
                                confirmOk: vrCommonRes['Yes'],
                                confirmCancel: vrCommonRes['No'],
                                confirmStyle: 'default',
                                //confirmOnlyOk: true,
                                confirmObject: combinval,
                                confirmCallback: DeleteTableField
                            });
                        }
                    });
                }
            }

        });

        $("#gridPrint").on('click', function () {
            $("#gview_grdTablesField").print(TableName);
        });
    })
    .fail(function (xhr, status) {
        ShowErrorMessge();
    });
}

//Delete the Table Field
function DeleteTableField(combinval) {
    var data = [];

    if (combinval != null) {
        data = combinval.split("#");

        $.post(urls.TableFields.RemoveFieldFromTable, $.param({ pTableName: data[0], pFieldName: data[1], pDeleteIndexes: data[2] }, true), function (data) {
            showAjaxReturnMessage(data.message, data.errortype);

            if (data.errortype == "s") {
                $('#mdlAddTablesField').HideModel();
                $("#grdTablesField").refreshJqGrid();
            }
        });
    }
}

function ShowFieldsPopup(TableName, Operation) {

    var vOriginalInternalName = "";
    var vOriginalFieldSize = 0;
    var vOriginalFieldType = 0;
    var selRowId;
    var celValue;

    $("#txtInternalName").SpecialCharactersSpaceNotAllowed();
    $("#txtFieldLength").OnlyNumeric();

    if (Operation == "EDIT") {

        selRowId = $("#grdTablesField").jqGrid('getGridParam', 'selrow');
        celValue = $("#grdTablesField").jqGrid('getCell', selRowId, 'Field Name');

        var selectedrows = $("#grdTablesField").getSelectedRowsIds();

        if (selectedrows.length > 1 || selectedrows.length == 0) {
            showAjaxReturnMessage(vrTablesRes["msgJsTableFieldPlzSelOnlyOneRow"], 'w');
            return;
        }
        else {
            if (selRowId == 1) {
                showAjaxReturnMessage(vrTablesRes["msgJsTableFieldSryUCantEditField"], 'w');
                return;
            }
            else {
                //CheckFieldBeforeEdit
                vOriginalInternalName = $("#grdTablesField").jqGrid('getCell', selRowId, 'Field_Name');
                vOriginalFieldSize = $("#grdTablesField").jqGrid('getCell', selRowId, 'Field_Size');
                vOriginalFieldType = $("#grdTablesField").jqGrid('getCell', selRowId, 'Field_Type');

                $.post(urls.TableFields.CheckFieldBeforeEdit, $.param({ pTableName: TableName, sFieldName: vOriginalInternalName }, true), function (data) {

                    if (data.IndexMsg != "") {
                        $(this).confirmModal({
                            confirmTitle: 'TAB FusionRMS',
                            confirmMessage: data.IndexMsg,
                            confirmOk: vrCommonRes['Ok'],
                            confirmStyle: 'default',
                            confirmOnlyOk: true
                        });
                    }
                    else {
                        if (data.Message != "")
                            $("#txtInternalName").attr("disabled", "disabled");
                        else
                            $("#txtInternalName").removeAttr("disabled", "disabled");

                        $('#mdlAddTablesField').ShowModel();
                        LoadFieldTypes(TableName);

                        $("#txtInternalName").val("");
                        $("#txtFieldLength").val("");

                        $('#lblTableName').text(String.format(vrTablesRes["lblJsTableFieldModify"], vOriginalInternalName, TableName));

                        $("#txtInternalName").val(vOriginalInternalName);
                        if (vOriginalFieldType == "Binary")
                            $("#lstFieldTypes").val("3").change();
                        else
                            $("#lstFieldTypes option:contains(" + vOriginalFieldType + ")").attr('selected', 'selected').change();

                        $("#txtFieldLength").val(vOriginalFieldSize);
                    }
                });
            }
        }
    }
    else {
        $('#mdlAddTablesField').ShowModel();

        //New Added on date - May 29th 2015 - Reported by Dhaval.
        $("#txtInternalName").removeAttr("disabled", "disabled");
        $("#txtInternalName").val("");
        $("#txtFieldLength").val("");

        LoadFieldTypes(TableName);

        $('#lblTableName').text(String.format(vrTablesRes["lblJsTableFieldCreateNewFieldIn"], TableName));
    }

    $("#lstFieldTypes").on('change', function () {
        var selectedVal = $("#lstFieldTypes").val();
        //console.log("selectedVal: " + selectedVal);

        switch (selectedVal) {
            case "2":
                $("#txtFieldLength").val("10");// Bug FUS-6072: Fixed by Nikunj.
                $("#txtFieldLength").removeAttr("disabled", "disabled");
                break;
            case "0":
                $("#txtFieldLength").attr("disabled", "disabled");
                $("#txtFieldLength").val("4");
                break;
            case "3":
                $("#txtFieldLength").attr("disabled", "disabled");
                $("#txtFieldLength").val("2");
                break;
            case "4":
                $("#txtFieldLength").attr("disabled", "disabled");
                $("#txtFieldLength").val("1");
                break;
            case "5":
                $("#txtFieldLength").attr("disabled", "disabled");
                $("#txtFieldLength").val("8");
                break;
            case "6":
                $("#txtFieldLength").attr("disabled", "disabled");
                $("#txtFieldLength").val("8");
                break;
            case "7":
                $("#txtFieldLength").attr("disabled", "disabled");
                $("#txtFieldLength").val("N/A");
                break;
            case "8":
                $("#txtFieldLength").attr("disabled", "disabled");
                $("#txtFieldLength").val("4");
                break;
            default:
                break;
        }
    });

    $("#btnOk").off().on('click', function () {
        var vNewInternalName = $("#txtInternalName").val();
        var vFieldType = $("#lstFieldTypes").val();
        var vFieldSize = $("#txtFieldLength").val();

        vOriginalFieldType = $("#grdTablesField").jqGrid('getCell', selRowId, 'Field_Type');
        vOriginalFieldType = $("#lstFieldTypes option:contains(" + vOriginalFieldType + ")").val();

        if (vOriginalFieldType === undefined) {
            vOriginalFieldType = "0";
        }
        var combinval = Operation + "#" + TableName + "#" + vNewInternalName + "#" + vOriginalInternalName + "#" + vFieldType + "#" + vOriginalFieldType + "#" + vFieldSize + "#" + vOriginalFieldSize;

        if ($("#txtInternalName").val() == "") {
            showAjaxReturnMessage(vrTablesRes["msgJsTableFieldIntNameReq"], 'w');
            return;
        }
        if ($("#txtFieldLength").val() == "") {
            showAjaxReturnMessage(vrTablesRes["msgJsTableFieldFieldLenCanNotEmpty"], 'w');
            return;
        }
        if ($("#txtFieldLength").val() < 1 || $("#txtFieldLength").val() > 8000) {
            showAjaxReturnMessage(vrTablesRes["msgJsTableFieldFieldSizeBet1To8000"], 'w');
            return;
        }
        if (Operation == "EDIT") {
            if ((parseInt(vFieldType) != 7) && ((parseInt(vOriginalFieldSize) > parseInt(vFieldSize)) || (parseInt(vOriginalFieldType) != parseInt(vFieldType)))) {
                var sMessage =String.format(vrTablesRes['msgJsTableFieldUChoseChangeTypeOrDecSize'] , vNewInternalName);

                $(this).confirmModal({
                    confirmTitle: 'TAB FusionRMS',
                    confirmMessage: sMessage,
                    confirmOk: vrCommonRes['Yes'],
                    confirmCancel: vrCommonRes['No'],
                    confirmStyle: 'default',
                    //confirmOnlyOk: true,
                    confirmObject: combinval,
                    confirmCallback: AddEditTablesField
                });
            }
            else if (parseInt(vOriginalFieldType) == 6 && vFieldType == 7) {
                //Added on 11/01/2016.
                if(vFieldType !=2)
                {
                    var msgtoshow = String.format(vrTablesRes['msgJsTableFieldCantUpdateThe'] , TableName );

                    $(this).confirmModal({
                        confirmTitle: 'TAB FusionRMS',
                        confirmMessage: msgtoshow,
                        confirmOk: vrCommonRes['Ok'],
                        confirmStyle: 'default',
                        confirmOnlyOk: true
                    });
                }
            }
            else {
                AddEditTablesField(combinval);
            }
        }
        else {
            AddEditTablesField(combinval);
        }

    });
}

function AddEditTablesField(combinval) {
    var data = [];

    var Operation = "";
    var TableName = "";
    var vNewInternalName = "";
    var vOriginalInternalName = "";
    var vFieldType = "";
    var vOriginalFieldType = "";
    var vFieldSize = "";
    var vOriginalFieldSize = "";

    if (combinval != null) {
        data = combinval.split("#");
        Operation = data[0];
        TableName = data[1];
        vNewInternalName = data[2];
        vOriginalInternalName = data[3];
        vFieldType = data[4];
        vOriginalFieldType = data[5];
        vFieldSize = data[6];
        vOriginalFieldSize = data[7];
    }

    $.post(urls.TableFields.AddEditField, $.param({ pOperationName: Operation, pTableName: TableName, pNewInternalName: vNewInternalName, pOriginalInternalName: vOriginalInternalName, pFieldType: vFieldType, pOriginalFieldType: vOriginalFieldType, pFieldSize: vFieldSize, pOriginalFieldSize: vOriginalFieldSize }, true), function (data) {

        showAjaxReturnMessage(data.message, data.errortype);

        if (data.errortype == 's') {
            $('#mdlAddTablesField').HideModel();
            $("#grdTablesField").refreshJqGrid();
        }
    });

}

//Load list of FieldTypes
function LoadFieldTypes(TableName) {
    $.ajax({
        url: urls.TableFields.GetFieldTypeList,
        dataType: "json",
        type: "POST",
        data: JSON.stringify({ pTableName: TableName }),
        contentType: 'application/json; charset=utf-8',
        async: false,
        processData: false,
        cache: false,
        success: function (data) {
            //console.log("Data: " + $.parseJSON(data.lstFieldTypesJson));
            var fieldTypesJSONList = $.parseJSON(data.lstFieldTypesJson);
            if (data.errortype == "s") {
                $("#lstFieldTypes").empty();

                $(fieldTypesJSONList).each(function (i, item) {
                    $("#lstFieldTypes").append("<option value=" + item.Key + ">" + item.Value + "</option>");
                });
                $("#lstFieldTypes option[value='1']").remove();
                $("#lstFieldTypes option:contains('Text')").attr('selected', 'selected').change();
                $("#txtFieldLength").val("10");// Bug: Fixed reported by Dhaval 29 May 2015.
            }
        },
        error: function (xhr, status, error) {
            //console.log("Error: " + error);
            ShowErrorMessge();
        }
    });
}