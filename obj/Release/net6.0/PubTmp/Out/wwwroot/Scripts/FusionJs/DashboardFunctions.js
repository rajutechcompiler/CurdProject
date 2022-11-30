var Language = []

const WIDGET_TYPE = {
    TASKS: { NAME: "TASKS", HEADING: "TASK LIST" },
    DATA: { NAME: "DATA", HEADING: "DATA GRID" },
    BAR: { NAME: "BAR", HEADING: "BAR CHART" },
    PIE: { NAME: "PIE", HEADING: "PIE CHART" },
    CHART_1: { NAME: "CHART_1", HEADING: "BAR CHART OBJECTS TRACKED BY DAY" },
    CHART_2: { NAME: "CHART_2", HEADING: "BAR CHART USER OPERATIONS BY DAY" },
    CHART_3: { NAME: "CHART_3", HEADING: "BAR CHART TIME SERIES" }
}

const CHART = {
    PIE: {
        NAME: "pie",
        BG: ["#36688D", "#F3CD05", "#A37C27", "#00743F", "#F49F05", "#563838", "#192E5B", "#F18904", "#A79674", "#132226", "#BDA589",
            "#6465A5", "#525B56", "#A7414A", "#F05837", "#A3586D", "#282726", "#ABA6BF", "#C0334D", "#6A8A82", "#595775", "#03353E"
        ]
    },

    BAR: {
        NAME: "bar",
        BG: ["#36688D", "#F3CD05", "#A37C27", "#00743F", "#F49F05", "#563838", "#192E5B",
            "#F18904", "#A79674", "#132226", "#BDA589", "#6465A5", "#525B56", "#A7414A", "#F05837",
            "#A3586D", "#282726", "#ABA6BF", "#C0334D", "#6A8A82", "#595775", "#03353E"
        ]
    }
}

var WidgetList = [];
var DashboardList = [];
var WidgetRefObjList = new Map();
var HandsontableInstance = new Map();
var PaginationInstance = new Map();
// do not set this value null or 0.
var ChartDataTooLarge = 100000

class DashboardFunc {
    constructor() {
    }


    ChartDraw(id, chartType) {
        $("#spinningWheel").show();
        var canvas = document.createElement('canvas');
        canvas.setAttribute("id", "mychart" + id);
        canvas.setAttribute("height", "300px");
        canvas.setAttribute("width", "100%");
        var jsonObj;
        for (let i = 0; i < WidgetList.length; i++) {
            if (WidgetList[i].Id == id) {
                jsonObj = WidgetList[i];
                break;
            }
        }
        var ctx = canvas.getContext('2d');
        dashboardfunc.EnlargeViewBarChartDataCall(ctx, jsonObj);
        return canvas;
    }
    /* to retrieve the bar chart widget object from database*/
    EnlargeViewBarChartDataCall(ctx, jsonObj) {
        $('#widgetContent').css({ "overflow": "hidden" });
        $("#spinnerDiv_widgetContent").show();
        var url = jsonObj.WidgetType == WIDGET_TYPE.CHART_1.NAME ? urls.GetTrackedData :
            jsonObj.WidgetType == WIDGET_TYPE.CHART_2.NAME ? urls.GetOperationsData :
                jsonObj.WidgetType == WIDGET_TYPE.CHART_3.NAME ? urls.GetTimeSeriesChartData : urls.GetChartData;

        $.post(url, { widgetObjectJson: JSON.stringify(jsonObj) })
            .done(function (response) {
                if (response.isError === false) {
                    var widgetObj = JSON.parse(response.JsonString)
                    if (jsonObj.WidgetType == WIDGET_TYPE.CHART_2.NAME) {
                        dashboardfunc.StackBarChart(ctx, response.Data, widgetObj);
                    }
                    else if (jsonObj.WidgetType == WIDGET_TYPE.PIE.NAME) {
                        dashboardfunc.PieChart(ctx, response.Data, widgetObj);
                    } else {
                        dashboardfunc.BarChart(ctx, response.Data, widgetObj);
                        dashboardfunc.AppendTitle("Enlarge", widgetObj);
                    }
                    $('#widgetContent').removeClass("hidden-e");
                    $("#spinningWheel").hide();
                    //setTimeout(() => { $("#openModal1 .modal-content canvas").css("width", "690"); }, 200)
                } else {
                    $("#spinningWheel").hide();
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                    //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: response.Msg });
                }
            })
            .fail(function (xhr, status, error) {

            });
    }
    /*To enlarge the widget*/
    Enlarge(id, widgetType) {
        var enlarge;
        this.Clean();
        $("#widgetEnlargeName").html($("#" + id + " #spanWidgetHeading xmp").html())
        $("#hiddenEnlargeType").val(widgetType)
        if (widgetType == WIDGET_TYPE.TASKS.NAME) {
            var clone = $(".task-clone").clone();
            enlarge = clone[0].innerHTML;
        }

        else if (widgetType == WIDGET_TYPE.DATA.NAME) {
            var jsonObj = dashboardfunc.GetWidgetJson(id);
            $("#ulEnlargeGrid .liIndex").hide();
            dashboardfunc.EnlargeDataGrid(jsonObj, "enlargeChart", true, 1);
            return;
        }
        else {
            enlarge = this.ChartDraw(id, widgetType);
        }
        $("#spinnerDiv_widgetContent").hide();
        $('#enlargeChart').append(enlarge);
    }
    /*Clean enlarge content */
    Clean() {
        $("#enlargeChartTitle").empty();
        $("#enlargeChart").empty();
        $("#enlargeChart").removeAttr("style");
        $("#enlargeChart").css({ "height": "95%", "width": "100%" });
        $("#paging").hide();
        $("#openModal1 #widgetContent").css("height", "300").css("width", "100%")

        $("#openModal1 .modal-content").css("height", "381")
        $("#openModal1 .modal-content").css("width", "746")
        $("#openModal1").css({ "padding-right": "", "left": "", "top": "", })
    }
    //Event to resize Enlarged View
    ResizableEnlargeWidget() {
        $('.modal-content').resizable({
            minWidth: 746,
            minHeight: 381,
            handles: "n, e, s",
            resize: function (e, ui) {
                var hie = ($("#openModal1 .modal-content").height() - 100)
                var type = $("#hiddenEnlargeType").val();
                if (type == WIDGET_TYPE.DATA.NAME) {
                    dashboardfunc.UpdateHandsontableEnlarge()

                } else {
                    if (hie > 300) {
                        $("#openModal1 #widgetContent").css("height", hie)
                        $("#openModal1 #widgetContent").css("width", "100%")
                    } else {
                        $("#openModal1 #widgetContent").css("height", 300)
                        $("#openModal1 #widgetContent").css("width", "100%")
                    }
                }

            }
        });
    }
    /*Generate new template for widget*/
    GenerateWidget(id, WidgetType, WidgetName, WidgetWidth, WidgetHeight) {

        var htmlString = '          <div id="' + id + '" class="panel panel-default col-lg-4 col-sm-12 col-md-12 no_padding no-margin widget clone" style="width: ' + WidgetWidth + ';height: ' + WidgetHeight + '">                                                                                                   ' +
            '          <div class="row" style = "height:100%" >                                                                                                                                                                                                                                         ' +
            '                <div class="panel-heading col-xs-12 no_padding no-margin top_action_header nav-bar">                                                                                                                                                                                       ' +
            '                    <div class="col-xs-8 col-sm-8 top_action_header_block no-margin widget-name">                                                                                                                                                                                          ' +
            '                        <input type="text" style="display:none" value="' + WidgetName + '" />                                                                                                                                                                                                ' +
            '                        <a class="customTooltip" gloss="' + WidgetName + '" href="#" style="z-index:999;"><h1 id="spanWidgetHeading" class="threedots"><xmp class="threedots">' + WidgetName + '</xmp></h1></a>                                                                                         ' +
            '                    </div>                                                                                                                                                                                                                                                                 ' +
            '                    <div class="col-xs-2 col-sm-2 top_action_header_block no_padding no-margin options">                                                                                                                                                                                   ' +
            '                        <img src="/Content/themes/TAB/css/images/refresh30px.png" onclick="dashboardfunc.RefreshWidget(' + id + ',\'' + WidgetType + '\')" class="icons authorized-img" style="width:20px;" />                                                                                                ' +
            '                        <img src="/Content/themes/TAB/css/images/arrows-fullscreen.png" data-toggle="modal" data-focus="false" data-backdrop="static" data-keyboard="false" data-target="#openModal1" onclick="dashboardfunc.Enlarge(' + id + ',\'' + WidgetType + '\')" class="icons authorized-img"/>   ' +
            '                        <img src="/Content/themes/TAB/css/images/three-dots-vertical.png" onclick="dashboardfunc.EditModal(\'' + WidgetType + '\', ' + id + ')" class="icons authorized-img" />                                                                                                             ' +
            '                        <img src="/Content/themes/TAB/css/images/x-lg.png" onclick="dashboardfunc.WidgetConf(' + id + ')" class="icons widget-cross" />                                                                                                                                                  ' +
            '                    </div>                                                                                                                                                                                                                                                                 ' +
            '                </div>                                                                                                                                                                                                                                                                     ' +
            '                <div class="panel-collapse col-xs-12 no_padding top_action_content widget-content" aria-expanded="true">                                                                                                                                                                   ' +
            '                    <div class="col-xs-12 col-sm-12 hidden-e task-clone" id="Task_' + id + '" style="width: 100%; height: 100%; overflow: hidden;">                                                                                                                                         ' +
            '                                 <ul id="tasks-list_' + id + '" class="tastk_list"> </ul>                                                                                                                                                                                                    ' +
            '                    </div>                                                                                                                                                                                                                                                                 ' +
            '                    <div class="col-xs-12 col-sm-12 hidden-e grid-clone" id="Grid_' + id + '" style="width: 100%; height: 100%;overflow-y:hidden;">                                                                                                                                         ' +
            '                        <div class="col-sm-12 div-h" style="margin-top: 2px; display: block;overflow:hidden;"><div id="Table_' + id + '" class="table1" style="display:none;"> </div></div>                                                                                                    ' +
            '                      <div style="margin-top: 3px; display: block;text-align:center" class="col-sm-12">                                                                                                                                                                                    ' +
            '                      <nav aria-label="Page navigation example" class="page-navigation">                                                                                                                                                                                                   ' +
            '                          <ul class="pagination pagi" style="justify-content: center" id="ul_' + id + '" >                                                                                                                                                                                     ' +
            '                              <li Class="page-item first " onclick="dashboardfunc.GetRecord(\'' + "First" + '\', this, \'' + "" + '\')"><a class="page-link">First</a></li>                                                                                                                      ' +
            '                              <li Class="#" onclick="dashboardfunc.GetRecord(\'' + "Previous" + '\', this, \'' + "" + '\')"><a>&laquo;</a></li>                                                                                                                                             ' +
            '                              <li Class="liIndex" style="display:none;" index="0" onclick="dashboardfunc.GetRecord(\'' + "Index" + '\', this, \'' + "" + '\')"><a> </a></li>                                                                                                                ' +
            '                              <li Class="liIndex" style="display:none;" index="0" onclick="dashboardfunc.GetRecord(\'' + "Index" + '\', this, \'' + "" + '\')"><a> </a></li>                                                                                                                ' +
            '                              <li Class="liIndex" style="display:none;" index="0" onclick="dashboardfunc.GetRecord(\'' + "Index" + '\', this, \'' + "" + '\')"><a> </a></li>                                                                                                                ' +
            '                              <li onclick="dashboardfunc.GetRecord(\'' + "Next" + '\', this, \'' + "" + '\')"><a>&raquo;</a></li>                                                                                                                                                           ' +
            '                              <li Class="page-item last" onclick="dashboardfunc.GetRecord(\'' + "Last" + '\', this, \'' + "" + '\')"><a class="page-link">Last</a></li>                                                                                                                     ' +
            '                          </ul>                                                                                                                                                                                                                                                            ' +
            '                      </nav>                                                                                                                                                                                                                                                               ' +
            '                    </div>                                                                                                                                                                                                                                                                 ' +
            '                    </div>                                                                                                                                                                                                                                                                 ' +
            '                    <div class="col-xs-12 col-sm-12 hidden-e" id="CanvasDiv_' + id + '" style="width: 100%; height: 100%; overflow: hidden;">                                                                                                                                               ' +
            '                        <div class="col-xs-12 col-sm-12" id="CanvasTitle_' + id + '" style="width: 100%; height: 5%;">asdf</div><div class="col-xs-12 col-sm-12" style="width: 100%; height: 95%;"><canvas id="Canvas_' + id + '"></canvas></div>                                                                                                                                                                                                                            ' +
            '                    </div>                                                                                                                                                                                                                                                                 ' +
            '                    <div class="col-xs-12 col-sm-12" id="spinnerDiv_' + id + '" style="width: 100%; height: 100%; overflow: hidden;">                                                                                                                                                       ' +
            '                        <i class="fa fa-refresh fa-spin spinfresh"></i>                                                                                                                                                                                                                    ' +
            '                    </div>                                                                                                                                                                                                                                                                 ' +
            '                    <div class="col-xs-12 col-sm-12" id="unauthorized_' + id + '" style="width: 100%; height: 100%; overflow: hidden;display:none;text-align:center;">                                                                                                                     ' +
            '                        <span style="position: absolute;top: 50%;left: 20%;transform: translate(-14%, -50%);font-weight: bold;">Unfortunately, one or more of your settings are now unavailable. Please contact administrator, or remove gadget by clicking delete from the title bar above</span>           ' +
            '                    </div>                                                                                                                                                                                                                                                                 ' +
            '                </div>                                                                                                                                                                                                                                                                     ' +
            '            </div>                                                                                                                                                                                                                                                                         ' +
            '        </div>                                                                                                                                                                                                                                                                             ';


        $('.sortable').append(htmlString);

    }
    GetWidgetDefaultWidth() {
        var width = $("#sortable").width();

        if (width / 2 <= 400) {
            width = (width - 25).toString() + 'px'//'500px'
        } else {
            width = (width / 2 - 25).toString() + 'px';
        }

        return width;
    }
    WidgetResetDefaultSize() {

        var width = dashboardfunc.GetWidgetDefaultWidth()

        let id = $("#hiddenDashboardId").val();
        if (id.length < 1) {
            return false
        }
        $("#spinningWheel").show();
        for (let i = 0; i < WidgetList.length; i++) {
            WidgetList[i].WidgetWidth = width;
            WidgetList[i].WidgetHeight = '257px';
        }

        let json = {
            DashboardId: id,
            JsonString: JSON.stringify(WidgetList)
        };

        $.post(urls.SetDashboardJson, json)
            .done(function (response) {
                dashboardfunc.UpdateLocalDashboardList(id, WidgetList);
                $("[dashbard-id=" + $("#hiddenDashboardId").val() + "]").click()
                $("#spinningWheel").hide();
            })
            .fail(function (xhr, status, error) {
                console.log(error);
                $("#spinningWheel").hide();
            });
    }
    ReadWidgetJson(arr) {
        arr.sort(function (a, b) {
            return (a.Order < b.Order ? -1 : 1);
        });

        $.post(urls.ValidatePermission, { widgetList: JSON.stringify(arr) })
            .done(function (response) {

                if (response.isError === false) {
                    arr = JSON.parse(response.JsonString);
                    for (var i = 0; i < arr.length; i++) {
                        var index = WidgetList.findIndex(({ Id }) => Id === arr[i].Id);
                        if (arr[i].permission) {
                            try {
                                dashboardfunc.GenerateWidget(arr[i].Id, arr[i].WidgetType, arr[i].WidgetName, arr[i].WidgetWidth, arr[i].WidgetHeight);
                                dashboardfunc.ReloadEvent(arr[i].Id);
                                dashboardfunc.ReadWidgetData(arr[i]);
                            } catch (ex) {
                                console.log("Error fu(ReadWidgetJson): ", ex.message, arr[i]);
                            }
                            WidgetList[index].permission = true;
                        } else {
                            var index = WidgetList.findIndex(({ Id }) => Id === arr[i].Id);
                            WidgetList[index].permission = false;
                            dashboardfunc.GenerateWidget(arr[i].Id, arr[i].WidgetType, arr[i].WidgetName, arr[i].WidgetWidth, arr[i].WidgetHeight);
                            dashboardfunc.ReloadEvent(arr[i].Id);
                            dashboardfunc.WidgetUnauthorized(arr[i].Id);
                        }
                    }
                    var dashboardId = parseInt(getCookie('lastDashboardID'));
                    dashboardfunc.UpdateLocalDashboardList(dashboardId, WidgetList);
                } else {
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                }
            })
            .fail(function (xhr, status, error) {
            });
    }
    WidgetUnauthorized(id) {
        $("#spinnerDiv_" + id).hide();
        $("#unauthorized_" + id).show();
        $("#CanvasDiv_" + id).addClass("hidden-e");
        $("#Grid_" + id).addClass("hidden-e");
        $("#" + id + " .authorized-img").addClass("hidden-e")
    }
    WidgetError(id) {
        $("#spinnerDiv_" + id).hide();
        $("#unauthorized_" + id).hide();
        $("#CanvasDiv_" + id).addClass("hidden-e");
        $("#Grid_" + id).addClass("hidden-e");
    }
    WidgetAuthorized(wid) {
        $("#spinnerDiv_" + wid).hide();
        $("#unauthorized_" + wid).hide();
        $("#CanvasDiv_" + wid).removeClass("hidden-e");
        $("#" + wid + " .authorized-img").removeClass("hidden-e")
    }
    ShowWidgetLoader(wid) {
        $("#spinnerDiv_" + wid).show();
        $("#unauthorized_" + wid).hide();
    }
    ResetWidgetCanvas(jsonObj) {
        $("#CanvasDiv_" + jsonObj.Id).removeClass("hidden-e");
        $("#CanvasDiv_" + jsonObj.Id).addClass("hidden-e");
        $("#Canvas_" + jsonObj.Id).remove();
        $("#CanvasDiv_" + jsonObj.Id).html('<div class="col-xs-12 col-sm-12" id="CanvasTitle_' + jsonObj.Id + '" style="width: 100%; height: 5%;"></div><div class="col-xs-12 col-sm-12" style="width: 100%; height: 95%;"><canvas id="Canvas_' + jsonObj.Id + '"></canvas></div>')

    }
    BarChartPostCall(jsonObj) {

        $.post(urls.GetChartData, { widgetObjectJson: JSON.stringify(jsonObj) })
            .done(function (response) {
                var widgetObj = JSON.parse(response.JsonString)
                if (response.isError === false) {
                    if (response.Permission === true) {
                        dashboardfunc.GenerateChart(widgetObj, response.Data)
                        dashboardfunc.WidgetAuthorized(widgetObj.Id);
                        dashboardfunc.ResetObjectTimeout(widgetObj, "")
                    } else {
                        dashboardfunc.GenerateChart(widgetObj, [])
                        dashboardfunc.WidgetUnauthorized(widgetObj.Id);
                    }
                } else {
                    $("#spinnerDiv_" + widgetObj.Id).hide();
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                }
            })
            .fail(function (xhr, status, error) {
                dashboardfunc.ShowErrorMsg(error, 'e')
            });
    }
    TaskListPostCall(jsonObj) {
        $.post(urls.GetTaskList, { widgetObjectJson: JSON.stringify(jsonObj) })
            .done(function (response) {
                var widgetObj = JSON.parse(response.JsonString)
                if (response.isError === false) {

                    $("#spinnerDiv_" + widgetObj.Id).hide();
                    $("#Task_" + widgetObj.Id).removeClass("hidden-e");
                    $("#tasks-list_" + widgetObj.Id).html(response.Data)

                    dashboardfunc.ResetObjectTimeout(widgetObj, "")
                } else {
                    $("#spinnerDiv_" + widgetObj.Id).hide();
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                }
            })
            .fail(function (xhr, status, error) { });
    }
    TrackedPostCall(jsonObj) {
        $.post(urls.GetTrackedData, { widgetObjectJson: JSON.stringify(jsonObj) })
            .done(function (response) {
                var widgetObj = JSON.parse(response.JsonString);
                if (response.isError === false) {
                    if (response.Permission === true) {
                        dashboardfunc.GenerateChart(widgetObj, response.Data)
                        dashboardfunc.WidgetAuthorized(widgetObj.Id);
                        dashboardfunc.ResetObjectTimeout(widgetObj, "");
                    } else {
                        dashboardfunc.GenerateChart(widgetObj, []);
                        dashboardfunc.WidgetUnauthorized(widgetObj.Id);
                    }
                } else {
                    $("#spinnerDiv_" + widgetObj.Id).hide();
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                }
            })
            .fail(function (xhr, status, error) {

            });
    }
    OperationsDataPostCall(jsonObj) {
        $.post(urls.GetOperationsData, { widgetObjectJson: JSON.stringify(jsonObj) })
            .done(function (response) {
                var widgetObj = JSON.parse(response.JsonString)
                if (response.isError === false) {
                    if (response.Permission === true) {
                        dashboardfunc.GenerateChart(widgetObj, response.Data)
                        dashboardfunc.WidgetAuthorized(widgetObj.Id);
                        dashboardfunc.ResetObjectTimeout(widgetObj, "")
                    } else {
                        dashboardfunc.GenerateChart(widgetObj, []);
                        dashboardfunc.WidgetUnauthorized(widgetObj.Id);
                    }
                } else {
                    $("#spinnerDiv_" + widgetObj.Id).hide();
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                }
            })
            .fail(function (xhr, status, error) {

            });
    }
    TimeSeriesPostCall(jsonObj) {
        $.post(urls.GetTimeSeriesChartData, { widgetObjectJson: JSON.stringify(jsonObj) })
            .done(function (response) {
                var widgetObj = JSON.parse(response.JsonString)
                if (response.isError === false) {
                    if (response.Permission === true) {
                        dashboardfunc.GenerateChart(widgetObj, response.Data)
                        dashboardfunc.WidgetAuthorized(widgetObj.Id);
                        dashboardfunc.ResetObjectTimeout(widgetObj, "")
                    } else {
                        dashboardfunc.GenerateChart(widgetObj, []);
                        dashboardfunc.WidgetUnauthorized(widgetObj.Id);
                    }
                } else {
                    $("#spinnerDiv_" + widgetObj.Id).hide();
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                }
            })
            .fail(function (xhr, status, error) {

            });
    }
    GetRecord(page, obj, parentId) {
        var id = "";
        if (parentId.length > 0) {
            id = parentId
        } else {
            var id = $(obj).parent().attr("id").split("_")[1]
        }

        if (id.length == 0) {
            return false;
        }
        var pagingInstance = PaginationInstance.get(id);
        if (pagingInstance == false) {
            return false;
        }
        switch (page) {
            case "First":
                pagingInstance.FirstPage()
                break;
            case "Last":
                pagingInstance.LastPage()
                break;
            case "Next":
                pagingInstance.Next()
                break;
            case "Previous":
                pagingInstance.Previous()
                break;
            case "Index":
                pagingInstance.GetRecord(obj)
                break;
        }
    }
    LoadPagination(jsonObj, paramss, isEnlarge) {
        var data = JSON.stringify({ paramsUI: paramss })
        var call = new DataAjaxCall(urls.GetTotalrowsForGrid, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "");
        call.Send().then((res) => {
            var inf = res.split('|');
            var SelectedIndex = paramss.pageNum;
            var TotalRecords = parseInt(inf[0])
            var PageSize = parseInt(inf[1])
            var PerPageRecords = parseInt(inf[2])
            var Id = jsonObj.Id;
            var obj;
         
            if (isEnlarge == true) {
                obj = new DashboardPaging(SelectedIndex, PageSize, TotalRecords, PerPageRecords, "ulEnlargeGrid", isEnlarge, Id)
                dashboardfunc.SetPaginationInstance(obj, "ulEnlargeGrid");
            } else {
                obj = new DashboardPaging(SelectedIndex, PageSize, TotalRecords, PerPageRecords, Id, isEnlarge, Id)
                dashboardfunc.SetPaginationInstance(obj, Id);
            }
            obj.VisibleIndexs();
                  }).catch(ex => {
            console.log(ex)
        })
    }
    SetPaginationInstance(instance, Id) {
        // first delete already exist key
        PaginationInstance.delete(Id.toString());
        // set instance of datagrid pagination
        PaginationInstance.set(Id.toString(), instance);
    }
    DataGridPostCall(jsonObj, pageNumber, loadPaging) {
        $("#Grid_" + jsonObj.Id).removeClass("hidden-e");
        $("#Grid_" + jsonObj.Id).addClass("hidden-e");
        var paramss = {
            ViewId: parseInt(jsonObj.ParentView),
            pageNum: parseInt(pageNumber),
            preTableName: '',
            Childid: ''
        }
        var data = JSON.stringify({
            paramss: paramss, searchQuery: []
        });
        new Promise((resolve, reject) => {
            var call = new DataAjaxCall(urls.GetGridData, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
            call.Send().then((response) => {
                try {
                    dashboardfunc.BuildHandsonTable(response, 'Grid_' + jsonObj.Id + ' .table1', jsonObj.Id, true);
                    dashboardfunc.UpdateHandsontable(jsonObj.Id);
                    $("#spinnerDiv_" + jsonObj.Id).hide();
                    $("#Grid_" + jsonObj.Id).removeClass("hidden-e");
                    dashboardfunc.ResetObjectTimeout(jsonObj, "")
                    if (loadPaging == true) {
                        dashboardfunc.LoadPagination(jsonObj, paramss, false);
                    }
                } catch (ex) {
                    console.log(ex)
                }
            });
        });
    }
    EnlargeDataGrid(jsonObj, handsontableId, loadPaging, pageNum) {
        $("#spinnerDiv_widgetContent").show();
        $("#spinningWheel").show();
        $("#widgetContent").hide();
        $("#paging").hide();
        var viewID = jsonObj.ParentView;
        var pageNum = parseInt(pageNum);
        var paramss = {
            ViewId: parseInt(viewID),
            pageNum: pageNum,
            preTableName: '',
            Childid: ''
        }
        var data = JSON.stringify({
            paramss: paramss, searchQuery: []
        });
        new Promise((resolve, reject) => {
            var call = new DataAjaxCall(urls.GetGridData, ajax.Type.Post, ajax.DataType.Json, data, ajax.ContentType.Utf8, "", "", "")
            call.Send().then((response) => {
                try {
                    dashboardfunc.BuildHandsonTable(response, handsontableId, jsonObj.Id, false);
                    $("#spinningWheel").hide();
                    $("#paging").show();
                    $("#spinnerDiv_widgetContent").hide();
                    $("#widgetContent").show();
                    setTimeout(() => { dashboardfunc.UpdateHandsontableEnlarge() }, 200)
                    if (loadPaging == true) {
                        dashboardfunc.LoadPagination(jsonObj, paramss, true);
                    }
                } catch (ex) {
                    console.log(ex)
                }
            })
                .fail(function (xhr, status, error) {
                    dashboardfunc.ShowErrorMsg(error.message, 'e')
                });
        });
    }
    GetWidgetJsonIdBase(id) {
        var jsonObj;
        for (var i = 0; i < WidgetList.length; i++) {
            if (WidgetList[i].Id == id) {
                jsonObj = WidgetList[i];
                break;
            }
        }
        return jsonObj;
    }
    BuildHandsonTable(data, Id, widgetId, localAdd) {
        var listOfHeaders = [], listOfData = [];
        var setOfHeaders = new Set(["pkey", "drilldown", "attachment"]);
        var datarows = data.ListOfDatarows, headers = data.ListOfHeaders;
        for (let i = 0; i < datarows.length; i++) {
            var row = []
            for (let j = 0; j < datarows[i].length; j++) {
                if (setOfHeaders.has(headers[j].HeaderName)) {
                    continue;
                }
                row.push(datarows[i][j])
                if (i == 0) { listOfHeaders.push(headers[j].HeaderName) }
            }
            listOfData.push(row);
        }
        var container = document.querySelector("#" + Id);
        container.style.display = "block";
        var hotSettings = {
            data: listOfData,
            rowHeaders: false,
            filters: false,
            dropdownMenu: false,// ['filter_by_condition', 'filter_action_bar'],
            outsideClickDeselects: false,
            disableVisualSelection: false,
            sortIndicator: true,
            columnSorting: true,
            colHeaders: listOfHeaders,
            stretchH: 'all',
            //fillHandle: false,
            //manualRowResize: true,
            //manualColumnResize: true,
            search: !localAdd,
            headerTooltips: true,
            //comments: true,
            autoRowSize: false,
            autoWrapCol: false,
            autoWrapRow: false,
            //manualRowMove: true,
            //manualColumnMove: true,
            licenseKey: "non-commercial-and-evaluation",
            afterDropdownMenuShow: (col, TH) => {
                $(".htCore input[type=text]:eq(0)").unbind('focusout');
                $(".htCore input[type=text]:eq(0)").focusout(function () {
                    $(".htCore input[type=text]:eq(0)").focus()
                });
            }
        };
        var hot = new Handsontable(container, hotSettings);
        hot.updateSettings({
            cells: function (row, col) {
                var cellProperties = {};
                cellProperties.readOnly = true;
                return cellProperties;
            },
            height: 300
        });
        if (localAdd == true) {
            dashboardfunc.AddObjectHandsontable(widgetId, hot)
        } else {
            dashboardfunc.AddObjectHandsontable("Enlarge", hot)
        }
        setTimeout(function () { hot.render(); }, 100);
    }
    AddObjectHandsontable(widgetId, hot) {
        // Make string of WidgetId
        var key = widgetId.toString();
        // first remove already exist
        HandsontableInstance.delete();
        // Add key
        HandsontableInstance.set(key, hot);
    }
    UpdateHandsontable(UId) {
        try {
            // get height of widget in pixel
            var gridh = $("#Grid_" + UId).height() - 70;
            // get object of handsontable
            var gethot = HandsontableInstance.get(UId.toString());
            // update handsontable properties
            gethot.updateSettings({ height: gridh });
        } catch (ex) {
            console.log("UpdateHandsontable fun:-", ex)
        }
    }
    UpdateHandsontableEnlarge() {
        try {
            // get height of widget in pixel
            var gridh = $("#openModal1 .modal-content").height() - 150

            // get height of widget in pixel
            var gridw = $("#openModal1 .modal-content").width() - 50;
            // get object of handsontable
            var gethot = HandsontableInstance.get("Enlarge");
            // update handsontable properties
            if (gethot !== undefined) {
                $("#openModal1 #widgetContent").css("height", gridh.toString() + "px")
                $("#openModal1 #widgetContent").css("width", gridw.toString() + "px")
                var colh = 42
                var rows = gethot.countRows()
                if (((colh * rows) + 32) < gridh) {
                    gridh = colh * rows;
                }

                gethot.updateSettings({ height: gridh, width: "100%" });
            }
        } catch (ex) {
            console.log("UpdateHandsontable fun:-", ex)
        }
    }
    ReadWidgetData(jsonObj) {
        dashboardfunc.ShowWidgetLoader(jsonObj.Id);
        if (jsonObj.WidgetType == WIDGET_TYPE.TASKS.NAME) {
            $("#tasks-list_" + jsonObj.Id).empty()
            dashboardfunc.TaskListPostCall(jsonObj)
        }
        else if (jsonObj.WidgetType == WIDGET_TYPE.BAR.NAME || jsonObj.WidgetType == WIDGET_TYPE.PIE.NAME) {
            dashboardfunc.ResetWidgetCanvas(jsonObj);
            dashboardfunc.BarChartPostCall(jsonObj);
        }
        else if (jsonObj.WidgetType == WIDGET_TYPE.CHART_1.NAME) {
            dashboardfunc.ResetWidgetCanvas(jsonObj);
            dashboardfunc.TrackedPostCall(jsonObj);
        }
        else if (jsonObj.WidgetType == WIDGET_TYPE.CHART_2.NAME) {
            dashboardfunc.ResetWidgetCanvas(jsonObj);
            dashboardfunc.OperationsDataPostCall(jsonObj);
        }
        else if (jsonObj.WidgetType == WIDGET_TYPE.CHART_3.NAME) {
            dashboardfunc.ResetWidgetCanvas(jsonObj);
            dashboardfunc.TimeSeriesPostCall(jsonObj);
        }
        else if (jsonObj.WidgetType == WIDGET_TYPE.DATA.NAME) {
            $.post(urls.CheckDataGridPermission, { widgetObjectJson: JSON.stringify(jsonObj) })
                .done(function (response) {
                    var widgetObj = JSON.parse(response.JsonString)
                    if (response.isError === false) {
                        if (response.Permission === true) {
                            dashboardfunc.DataGridPostCall(widgetObj, 1, true);
                        } else {
                            dashboardfunc.WidgetUnauthorized(widgetObj.Id);
                        }
                    } else {
                        $("#spinnerDiv_" + widgetObj.Id).hide();
                        dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                    }
                })
                .fail(function (xhr, status, error) {
                    dashboardfunc.ShowErrorMsg(error, 'e')
                });
        }
    }
    RefreshWidget(id, widgetType) {
        var i = 0;
        for (i = 0; i < DashboardList.length; i++) {
            if (DashboardList[i].ID == $("#hiddenDashboardId").val()) {
                break;
            }
        }
        var hindt = JSON.parse(DashboardList[i].Json);
        var j = 0;
        for (j = 0; j < hindt.length; j++) {
            if (hindt[j].Id == id) {
                break;
            }
        }
        dashboardfunc.ReadWidgetData(hindt[j]);
    }
    /*Fire an event on widget add*/
    ReloadEvent(UId) {
        this.WidgetResizableEvent(UId);
        this.WidgetDblClickEvent(UId);
        this.WidgetKeyPressEvent(UId);
        this.WidgetBlurEvent(UId);
    }
    GetWidgetJson(UId) {
        var jsonObj;
        for (var i = 0; i < WidgetList.length; i++) {
            if (WidgetList[i].Id == UId) {
                jsonObj = WidgetList[i];
                break;
            }
        }
        return jsonObj;
    }
    WidgetResizableEvent(UId) {
        $("#" + UId).resizable({
            handles: "e, s",
            minHeight: 257,
            minWidth: 335,
            stop: function (e, ui) {

                for (let i = 0; i < WidgetList.length; i++) {
                    if (WidgetList[i].Id == UId) {
                        WidgetList[i].WidgetWidth = ui.size.width.toString() + 'px';
                        WidgetList[i].WidgetHeight = ui.size.height.toString() + 'px';
                        break;
                    }
                }

                let id = $("#hiddenDashboardId").val();
                let json = {
                    DashboardId: id,
                    JsonString: JSON.stringify(WidgetList)
                };

                dashboardfunc.UpdateHandsontable(UId)
                $.post(urls.SetDashboardJson, json)
                    .done(function (response) {
                        dashboardfunc.UpdateLocalDashboardList(id, WidgetList);
                    })
                    .fail(function (xhr, status, error) {
                        console.log(error);
                    });
            }
        });
    }
    WidgetDblClickEvent(UId) {
        $("#" + UId + " h1").dblclick(function () {
            $("#" + UId + " input").css({ "display": "block" });
            $("#" + UId + " input").focus();
            $("#" + UId + " h1").addClass("hidden-e");
        })
    }
    WidgetKeyPressEvent(UId) {
        $("#" + UId + " input").keypress(function () {
            $(this).attr('maxLength', 25);
        })
    }
    WidgetBlurEvent(UId) {
        $("#" + UId + " input").blur(function () {
            $("#spinningWheel").show();
            var inputText = $(this).val();
            $("#" + UId + " input").css({ "display": "none" });
            $("#" + UId + " h1").removeClass("hidden-e");
            $("#" + UId + " h1 xmp").html(inputText);
            $("#" + UId + " a").attr('gloss', inputText);
            var json = "";
            for (let i = 0; i < WidgetList.length; i++) {
                if (WidgetList[i].Id == UId) {
                    WidgetList[i].WidgetName = inputText;
                    json = JSON.stringify(WidgetList[i]);
                    break;
                }
            }

            var jsonObject = {
                DashboardId: parseInt($("#hiddenDashboardId").val()),
                JsonString: JSON.stringify(WidgetList),
            };

            $.post(urls.SetDashboardJson, jsonObject)
                .done(function (response) {
                    $("#spinningWheel").hide();
                    dashboardfunc.ShowErrorMsg(inputText + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"], 's')
                    dashboardfunc.UpdateLocalDashboardList(jsonObject.DashboardId, WidgetList);
                })
                .fail(function (xhr, status, error) {
                    dashboardfunc.ShowErrorMsg(error.message, 'e')
                });
        })
    }
    ReloadModalSortable() {
        $(".modal").draggable({
            handle: ".modal-header"
        });
    }
    /*To create chart*/
    GenerateChart(widgetObj, data) {

        var canvas = document.getElementById("Canvas_" + widgetObj.Id);
        var ctx = canvas.getContext('2d');

        if (widgetObj.WidgetType == WIDGET_TYPE.CHART_2.NAME) {
            this.StackBarChart(ctx, data, widgetObj);
        }
        else if (widgetObj.WidgetType != WIDGET_TYPE.PIE.NAME) {
            this.BarChart(ctx, data, widgetObj);
            this.AppendTitle("", widgetObj);
        } else {
            this.PieChart(ctx, data, widgetObj);
        }
    }
    AppendTitle(forTitle, widgetObj) {
        if (widgetObj.WidgetType == 'CHART_1') {
            var displayStr = widgetObj.TableName
            var len = 30
            if (widgetObj.TableName.length > len) {
                displayStr = widgetObj.TableName.substr(0, len) + '...'
            }
            if (forTitle == "Enlarge") {
                $("#enlargeChartTitle").prepend('<div style="display:block;text-align:center"><a class="customTooltip" gloss="' + widgetObj.TableName + '" href="#" style="color:black">' + displayStr + '</a></div>')
            } else {
                $("#CanvasTitle_" + widgetObj.Id).append('<div style="display:block;text-align:center"><a class="customTooltip" gloss="' + widgetObj.TableName + '" href="#" style="color:black">' + displayStr + '</a></div>')
            }
        }
    }
    /*Create Pie Chart*/
    PieChart(ctx, data, widgetObj) {

        var cLabels = []
        var cData = [];
        var legend = true;
        var chartTitle = ""
        var titleDisplay = true
        var cLabelWithLabels = [];
        chartTitle = widgetObj.DisplayColumn == undefined ? "" : "Column : " + widgetObj.DisplayColumn;
        // if legend.data length > 5 then hide legend
        if (data.length > 0) {
            if (data.length > 5) {
                legend = false

            }
            cLabels = data.map(a => a.X);
            cData = data.map(a => a.Y);

            // convert true value to Active and false to Inactive
            cData.forEach(function (entry, index) {
                var la = cLabels[index] == null ? "" : cLabels[index].toString()
                la = la == "true" ? "Active" : la == "false" ? "Inactive" : la == "" ? "No Record" : la
                cLabelWithLabels.push(la + "-" + cData[index]);
            });
        }
        var myChart = new Chart(ctx, {

            type: CHART.PIE.NAME,
            data: {
                labels: cLabelWithLabels,
                datasets: [{
                    label: '#Sample',
                    data: cData,
                    backgroundColor: CHART.PIE.BG
                }]
            },
            options: {
                color: "black",
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    datalabels: {
                        formatter: function (value, context) {
                            var sum = context.dataset.data.reduce((a, b) => a + b, 0)
                            var each = (context.dataset.data[context.dataIndex] * 100);
                            return (each / sum).toFixed(2) + '%';
                        }
                    },
                    legend: {
                        display: legend,
                    },
                    tooltip: {
                        callbacks: {
                            title: function (tooltipItem) {
                                return tooltipItem[0].label
                            },
                            label: function (context) {
                                var sum = context.dataset.data.reduce((a, b) => a + b, 0)
                                var each = (context.dataset.data[context.dataIndex] * 100);
                                return (each / sum).toFixed(2) + '%';

                            }
                        },

                    },
                    title: {
                        align: 'center',
                        color: '0x565656',
                        position: 'top',
                        padding: 10,
                        display: titleDisplay,
                        text: chartTitle
                    },
                }
            },
            //plugins: [ChartDataLabels],
        });

    }
    /*Create Bar Chart*/
    BarChart(ctx, data, widgetObj) {
        var cLabels = [];
        var cData = [];
        var titleDisplay = false;
        var chartTitle = ''
        //ttile text can be added for other ChartTypes as well (Column/TableName)
        var chartlabel = '';
        if (widgetObj.WidgetType == 'BAR') {
            chartlabel = "Column : " + widgetObj.DisplayColumn;
        }
        else if (widgetObj.WidgetType == 'CHART_1') {
            chartlabel = "Tracked Objects filtered by " + widgetObj.Filter;
            chartTitle = "Table(s):-" + widgetObj.TableName;
        } else if (widgetObj.WidgetType == 'CHART_2') {
            chartlabel = "Audit User Operation filtered by " + widgetObj.Filter;
        }
        else if (widgetObj.WidgetType == 'CHART_3') {
            chartlabel = "Column : " + widgetObj.DisplayColumn;
        }
        if (data.length > 0) {
            cLabels = data.map(a => {
                if (a.X == null) {
                    return "Unknown";
                } else {
                    return a.X;
                }
            });
            cData = data.map(a => a.Y);
        }
        var config = {
            type: CHART.BAR.NAME,
            data: {
                labels: cLabels,
                datasets: [{
                    label: chartlabel,
                    data: cData,
                    backgroundColor: CHART.BAR.BG[1],
                    borderColor: '0x292929',
                    borderWidth: .5,
                    barPercentage: 1,
                    barThickness: 20,
                    maxBarThickness: 20,
                    minBarLength: 2,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    tooltip: {
                        enabled: true
                    },
                    legend: {
                        display: true,
                        legendText: 'Current',
                        onClick: function (e, legendItem, legend) { }
                    },
                    title: {
                        align: 'center',
                        color: '0x565656',
                        position: 'top',
                        padding: 10,
                        display: titleDisplay,
                        text: chartTitle
                    },
                },
                scales: {
                    yAxes: [{
                        ticks: {
                            callback: function (value, index, values) {
                                return (6 - value);
                            }
                        }
                    }]
                }
            }
        }

        setTimeout(dashboardfunc.RenderChart, 100, ctx, config)
    }

    StackBarChart(ctx, data, widgetObj) {
        var cLabels = [];
        var dataSets = [];
        var legend = true;
        if (widgetObj.WidgetType != WIDGET_TYPE.CHART_2.NAME) {
            return false
        }
        //ttile text can be added for other ChartTypes as well (Column/TableName)
        var chartTitle = "Audit User Operation filtered by " + widgetObj.Filter;
        if (data.length > 0) {
            if (data.length > 5) {
                legend = false
            }
            if (data[0].Data.length > 0) {

                cLabels = data[0].Data.map(a => a.X);
            }

            for (var ds = 0; ds < data.length; ds++) {
                dataSets.push({
                    label: data[ds].AuditType,
                    data: data[ds].Data.map(a => a.Y),
                    backgroundColor: CHART.BAR.BG[ds],
                    borderWidth: .5,
                    barPercentage: 1,
                    barThickness: 20,
                    maxBarThickness: 20,
                    //minBarLength: 2,
                })

            }

        }

        var config = {
            type: CHART.BAR.NAME,
            data: {
                labels: cLabels,
                datasets: dataSets
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: legend,
                        legendText: 'Current',
                        onClick: function (e, legendItem, legend) { }
                    },
                    title: {
                        align: 'center',
                        color: '0x565656',
                        position: 'top',
                        padding: 10,
                        display: true,
                        text: chartTitle
                    },
                },
                scales: {
                    y: {
                        stacked: true,
                        grid: {
                            display: true,

                        }
                    },
                    x: {
                        stacked: true,
                        grid: {
                            display: true
                        }
                    }
                }
            }
        }

        setTimeout(dashboardfunc.RenderChart, 100, ctx, config)
    }

    RenderChart(ctx, config) {
        new Chart(ctx, config);
    }
    /*Open widget delete confirmation */
    WidgetConf(id) {
        $("#modalDeleteWidget").modal("show");
        $("#hiddenWidgetID").val(id);
    }
    /*Delete widget after confirm*/
    DeleteWidget() {
        $("#" + Number($("#hiddenWidgetID").val())).remove();
        //To delete from database
        DeleteWidgetFromDb($("#hiddenWidgetID").val());
        ResetBtnCheck();
    }
    /*Using for modal open of Bar Chart, Pie Chart, Data grid*/
    OpenModalChart(val, id) {
        $("#hiddenChartVal").val(val);
        this.CloseWidgetListModal()
        setTimeout(() => {
            LoadChart(id);
        }, 500);
        $("#lstTypeChart").val(val);
    }
    /*Using for modal open of Bar Chart Objects Tracked by Day*/
    OpenModalTracked(val, id) {
        this.CloseWidgetListModal()
        setTimeout(() => {
            LoadTracked(id);
        }, 500);

    }
    /*Using for modal open of Bar Chart User Operations by Day*/
    OpenModalOperation(val, id) {
        this.CloseWidgetListModal()
        setTimeout(() => { LoadOperation(id); }, 500);
    }
    /*Using for modal open of Bar Chart Time Series*/
    OpenModalSeries(val, id) {
        this.CloseWidgetListModal()
        setTimeout(() => {
            LoadSeries(id);
        }, 500);

    }
    /*Using for modal open of Task List*/
    OpenModalTask(val, id) {
        $("#openModal32").modal("hide");
        $('body').removeClass('modal-open');
        $('.modal-backdrop').remove();
        setTimeout(() => {
            LoadTask(id);
        }, 500);
    }
    /*Close add widget modal*/
    CloseWidgetListModal() {
        $("#openModal32").modal("hide");
        $('body').removeClass('modal-open');
        $('.modal-backdrop').remove();
    }
    /*Open edit option modal*/
    EditModal(val, id) {
        if (val == WIDGET_TYPE.TASKS.NAME) {
            this.OpenModalTask(val, id);
        } else if (val == WIDGET_TYPE.CHART_1.NAME) {
            this.OpenModalTracked(val, id);
        } else if (val == WIDGET_TYPE.CHART_2.NAME) {
            this.OpenModalOperation(val, id);
        } else if (val == WIDGET_TYPE.CHART_3.NAME) {
            this.OpenModalSeries(val, id);
        } else {
            this.OpenModalChart(val, id)
        }
    }
    ResetObjectTimeout(widgetObj, clear) {
        // this condition are using for clear all the intervals 
        if (clear == "All") {
            WidgetRefObjList.forEach(element => clearTimeout(element));
            WidgetRefObjList.clear();
            return true
        }
        // this condion are remove old time interal of the widget
        if (WidgetRefObjList.get(widgetObj.Id.toString()) != undefined) {
            clearTimeout(WidgetRefObjList.get(widgetObj.Id.toString()))
            WidgetRefObjList.delete(widgetObj.Id.toString());
        }
        // this condition are regenerate chart within time period that are selected by the user
        if (widgetObj.Interval != "0" && widgetObj.Interval != "") {
            var interval = parseInt(widgetObj.Interval);
            interval = interval * 1000 * 60;
            let setTime = setTimeout(dashboardfunc.ReadWidgetData, interval, widgetObj);
            WidgetRefObjList.set(widgetObj.Id.toString(), setTime);
        }
    }
    PreventEnter() {
        $("input[type=text]").on("keypress", function (event) {
            if (event.key === "Enter") {
                event.preventDefault();
            }
        })
    }
    ReloadSelect() {
        let currentDashID = getCookie('lastDashboardID');
        if (currentDashID != "") {

            var index = $("[dashbard-id=" + currentDashID + "]").index()
            $("#ulDashboardList li:eq(" + index + ")").click();
        }
    }
    UpdateLocalDashboardList(DashboardId, widgetList) {
        for (let i = 0; i < DashboardList.length; i++) {
            if (DashboardList[i].ID == DashboardId) {
                DashboardList[i].Json = JSON.stringify(widgetList)
                break;
            }
        }
    }
    UpdateFavouriteDashboardStatus(json) {
        $.post(urls.UpdateFavouriteDashboard, json)
            .done(function (response) {
                if (response.isError === false) {
                    for (let i = 0; i < DashboardList.length; i++) {
                        if (DashboardList[i].ID == json.DashboardId) {
                            DashboardList[i].IsFav = json.IsFav;
                            break;
                        }
                    }
                    $("#ulDashboardList").empty()
                    window.BindDashboardList(response.DashboardListHtml)
                    $("[dashbard-id=" + json.DashboardId + "] a").addClass("active")
                    $("#spinningWheel").hide();

                } else {
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                }
            })
            .fail(function (xhr, status, error) {
                $("#spinningWheel").hide();
            });
    }
    ScrollDownSet() {
        var element = document.getElementById("sortable");
        element.scrollTop = element.scrollHeight - element.clientHeight;
    }
    ShowErrorMsg(msg, type) {
        showAjaxReturnMessage("<xmp style='white-space: pre-wrap'>" + msg + "</xmp>", type);
    }
}
class DashboardPaging {

    constructor(SelectedIndex, PageSize, TotalRecords, PerPageRecords, Id, IsEnlarge, WidgetId) {
        this.SelectedIndex = SelectedIndex;
        this.PageSize = PageSize
        this.VisibleIndexsNo = 3
        this.TotalRecords = TotalRecords
        this.PerPageRecords = PerPageRecords
        this.GotoTextFieldId = "#inputGotoPageNum"
        this.ParentIndexId = Id
        this.IsEnlarge = IsEnlarge
        this.WidgetId = WidgetId
    }

    // this function are using for load handsontable data
    LoadRecord() {
        var jsonObj = dashboardfunc.GetWidgetJson(parseInt(this.WidgetId))
        if (this.IsEnlarge == true) {
            dashboardfunc.EnlargeDataGrid(jsonObj, "widgetContent", false, this.SelectedIndex);
        } else {
            $("#spinnerDiv_" + jsonObj.Id).show();
            dashboardfunc.DataGridPostCall(jsonObj, this.SelectedIndex, false);
        }
    }
    // this function are using load next record
    Next() {
        if (this.SelectedIndex == this.PageSize) {
            return false;
        }

        this.SetActiveIndex(this.SelectedIndex, (this.SelectedIndex + 1))
        this.SelectedIndex = this.SelectedIndex + 1;
        $(this.GotoTextFieldId).val(this.SelectedIndex)
        this.ResetPaginationAndLoadData()
    }
    // this function are using for load previous record
    Previous() {
        if (this.SelectedIndex == 1) {
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
        $("#" + this.ParentIndexId + "[index='" + remove + "']").removeClass("active");
        $("#" + this.ParentIndexId + "[index='" + add + "']").addClass("active");
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
        $(this.GotoTextFieldId).val(this.SelectedIndex)
        this.AddCommaToTotalRecords()
    }
    AddCommaToTotalRecords() {
        $("#divTotalRecordsspan").html(this.TotalRecords.toLocaleString())
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
                            $("#" + this.ParentIndexId + " .liIndex").eq(elementIndex).show().addClass("active").attr("index", j).children("a").html(j)
                        } else {
                            $("#" + this.ParentIndexId + " .liIndex").eq(elementIndex).show().removeClass("active").attr("index", j).children("a").html(j)
                        }
                        elementIndex = elementIndex + 1;
                    }

                    if (this.VisibleIndexsNo - elementIndex > 0) {
                        for (var k = elementIndex; k <= this.VisibleIndexsNo - 1; k++) {
                            $("#" + this.ParentIndexId + " .liIndex").eq(k).hide();
                        }
                    }

                    break;
                }
            }
        } else {
            if (this.PageSize == 0) { return false; }
            var elementIndex = 0;
            for (var j = 0; j < this.PageSize; j++) {
                if (j + 1 == this.SelectedIndex) { $("#" + this.ParentIndexId + " .liIndex").eq(j).show().addClass("active").attr("index", j + 1).children("a").html(j + 1) }
                else { $("#" + this.ParentIndexId + " .liIndex").eq(j).show().removeClass("active").attr("index", j + 1).children("a").html(j + 1) }
                elementIndex = elementIndex + 1;
            }
            if (this.VisibleIndexsNo - elementIndex > 0) {
                for (var k = elementIndex; k <= this.VisibleIndexsNo - 1; k++) {
                    $("#" + this.ParentIndexId + " .liIndex").eq(k).hide();
                }
            }
        }
    }
}


const WidgetTaskJson = (id, or, wn, iv, ww, wh) => ({ Id: id, Order: or, WidgetName: wn, Interval: iv, WidgetWidth: ww, WidgetHeight: wh, WidgetType: WIDGET_TYPE.TASKS.NAME });
const WidgetBarJson = (id, or, wn, pv, col, iv, ww, wh, tn, wg, disCol, pr) => ({ Id: id, Order: or, WidgetName: wn, ParentView: pv, Column: col, Interval: iv, WidgetWidth: ww, WidgetHeight: wh, WidgetType: WIDGET_TYPE.BAR.NAME, TableName: tn, WorkGroup: wg, Data: [], DisplayColumn: disCol, permission: pr });
const WidgetPieJson = (id, or, wn, pv, col, iv, ww, wh, tn, wg, pr, disCol) => ({ Id: id, Order: or, WidgetName: wn, ParentView: pv, Column: col, Interval: iv, WidgetWidth: ww, WidgetHeight: wh, WidgetType: WIDGET_TYPE.PIE.NAME, TableName: tn, WorkGroup: wg, Data: [], permission: pr, DisplayColumn: disCol });
const WidgetGridJson = (id, or, wn, pv, col, iv, ww, wh, tn, wg) => ({ Id: id, Order: or, WidgetName: wn, ParentView: pv, Column: col, Interval: iv, WidgetWidth: ww, WidgetHeight: wh, WidgetType: WIDGET_TYPE.DATA.NAME, TableName: tn, WorkGroup: wg });
const WidgetTrackedJson = (id, or, wn, os, p, iv, ww, wh, ftr, tn, pr) => ({ Id: id, Order: or, WidgetName: wn, Objects: os, Period: p, Interval: iv, WidgetWidth: ww, WidgetHeight: wh, WidgetType: WIDGET_TYPE.CHART_1.NAME, Data: [], Filter: ftr, TableName: tn, permission: pr });
const WidgetObjectJson = (id, or, wn, us, at, os, p, iv, ww, wh, tn, ftr, pr) => ({ Id: id, Order: or, WidgetName: wn, Objects: os, Users: us, AuditType: at, Period: p, Interval: iv, WidgetWidth: ww, WidgetHeight: wh, WidgetType: WIDGET_TYPE.CHART_2.NAME, TableName: tn, Data: [], Filter: ftr, permission: pr });
const WidgetSeriesJson = (id, or, wn, pv, col, p, iv, ww, wh, tn, wg, ftr, disCol, pr) => ({ Id: id, Order: or, WidgetName: wn, ParentView: pv, Column: col, Period: p, Interval: iv, WidgetWidth: ww, WidgetHeight: wh, WidgetType: WIDGET_TYPE.CHART_3.NAME, TableName: tn, WorkGroup: wg, Data: [], Filter: ftr, DisplayColumn: disCol, permission: pr });

//FIRST OBJECT CREATED ON THE FIRST RUN.
const dashboardfunc = new DashboardFunc();
var pagingObj = new DashboardPaging(0, 0, 0, 0, false, "");