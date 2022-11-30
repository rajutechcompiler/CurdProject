/*Using this function for open modal of new Dashboard & Bind event of dashboard*/
function LoadDashboard(json) {
    $("#spinningWheel").show();
    $('#divModalLoad').empty();
    $('#divModalLoad').load(urls.AddEditDashboardPartial, function () {
        $('#modalAddNewDashboard').ShowModel();
        dashboardfunc.ReloadModalSortable();
      
        $("#spinningWheel").hide();
        /*This Event are using for create new Dashboard*/
        $("#btnSaveDashboardName").click(() => {

            $("#spinningWheel").show();

            var Name = $("#txtNewDashboardName").val();

            // check validate jquery
            ValidateAddDashboard()

            var $form = $('#frmAddEditDashboard');
            var serializedForm = $form.serialize();
            var newID;

            if ($form.valid()) {
                $.post(urls.SetDashboardDetails, serializedForm)
                    .done(function (response) {
                        $("#spinningWheel").hide();
                        if (response.isError === false) {
                            if (response.ErrorMessage == "Already exists name.") {
                                dashboardfunc.ShowErrorMsg('"' + Name + '" ' + JSON.parse(localStorage.getItem('language'))["alreadyexistsname"], 'w')
                                //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-warning', message: '"' + Name + '" ' + JSON.parse(localStorage.getItem('language'))["alreadyexistsname"] });
                            }
                            else {
                                $("#modalAddNewDashboard").modal("hide");
                                BindDashboardList("<li class='hasSubs' dashbard-id='" + response.ud.ID + "'><a><xmp class='xmpDashbaordNames'>" + response.ud.Name + "</xmp></a></li>");
                                var dashboard_name = $("[dashbard-id=" + response.ud.ID + "] a xmp").html();
                                
                                if (json == undefined) {
                                    dashboardfunc.ShowErrorMsg(dashboard_name + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"], 's')
                                }
                                DashboardList.push(response.ud);
                                newID = response.ud.ID;
                                setCookie('lastDashboardID', response.ud.ID);
                                dashboardfunc.ReloadSelect();
                                if (json != undefined) {
                                    var DashboardJson = {
                                        DashboardId: newID,
                                        JsonString: json
                                    };
                                    $("#spinningWheel").show();
                                    $.post(urls.SetDashboardJson, DashboardJson)
                                        .done(function (response) {
                                            $("#spinningWheel").hide();
                                            dashboardfunc.ShowErrorMsg(dashboard_name + " " + JSON.parse(localStorage.getItem('language'))["clonedSuccessfully"], 's')
                                            DashboardList[DashboardList.length - 1].Json = json;
                                            var arrJsonWidget = JSON.parse(json)
                                            if (arrJsonWidget != null) {
                                                WidgetList = arrJsonWidget
                                                dashboardfunc.ReadWidgetJson(arrJsonWidget);
                                            }
                                            ResetBtnCheck(WidgetList);
                                        })
                                        .fail(function (xhr, status, error) {
                                            console.log("Failed ", error);
                                            $("#spinningWheel").hide();
                                            dashboardfunc.ShowErrorMsg(error.message, 'e')
                                        });
                                }
                                $("#ulDashboardList").empty()
                                window.BindDashboardList(response.DashboardListHtml)
                                $("[dashbard-id=" + newID + "] a").addClass("active")
                            }
                        }
                        else
                        {
                            $("#modalAddNewDashboard").modal("hide");
                            dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                        }
                    })
                    .fail(function (xhr, status, error) {
                        $("#spinningWheel").hide();
                        $("#modalAddNewDashboard").modal("hide");
                        dashboardfunc.ShowErrorMsg(error.message, 'e')
                    });
            } else {
                $("#spinningWheel").hide();
            }
        });
        if (json == undefined) {
            $('#myModalmodalAddNewDashboard').html(Language["addNewDashboard"]);
        }
        else {
            $("#btnSaveDashboardName").html(Language["clone"])
            $('#myModalmodalAddNewDashboard').html(Language["cloneDashboard"]);
        }
    });
}
function ValidateAddDashboard() {
    $('#frmAddEditDashboard').validate({
        rules: {
            Name: {
                required: true,
                rangelength: [2, 60]
            }
        },
        ignore: ":hidden:not(select)",
        highlight: function (element) {
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-error');
        },
        errorElement: 'span',
        errorClass: 'help-block',
        errorPlacement: function (error, element) {
            if (element.parent('.input-group').length) {
                error.insertAfter(element.parent());
            } else {
                error.insertAfter(element);
            }
        }
    });
}
/*this funciton loads all dashboard into left side nav*/
function BindDashboardList(html) {
    $("#ulDashboardList").append(html)
    $("#ulDashboardList li").unbind()
    BindUlDashboardListClickEvent();
}
function BindUlDashboardListClickEvent() {
    $("#ulDashboardList li").click(function (e) {
        $("#spinningWheel").show();
        $(".nav a").removeClass('active');
        $(this).children().addClass('active');
        //$(this).children().children().css('color','white');

        $("#del-dash").removeClass("hidden-e");
        $("#new-widget").removeClass("hidden-e");
        $("#rename-dash").removeClass("hidden-e");
        $("#btnClone").removeClass("hidden-e");
        $("#btnReset").removeClass("hidden-e");

        WidgetList = []

        $("#sortable").empty();
        dashboardfunc.ResetObjectTimeout({}, "All");
        var attr = e.currentTarget.attributes["dashbard-id"];
        var Id = parseInt(attr.value);
        $("#hiddenDashboardId").val(Id);
        setCookie('lastDashboardID', Id);
       
        var DashboardJson = DashboardList.find(c => c.ID == Id);
        if (DashboardJson != undefined) {
            if (DashboardJson.Json != "") {
                var arrJsonWidget = JSON.parse(DashboardJson.Json)
            }
            else {
                var arrJsonWidget = null
            }
            IsFavDashboard(DashboardJson.IsFav);

            $("#spanWidgetHeading xmp").html(DashboardJson.Name)
            if (arrJsonWidget != null) {
                WidgetList = arrJsonWidget
                dashboardfunc.ReadWidgetJson(arrJsonWidget);
            }
            
            ResetBtnCheck(WidgetList);
            $("#spinningWheel").hide();

        } else {
            var json = {
                DashboardId: parseInt($("#hiddenDashboardId").val())
            }
            $.get(urls.GetDashboardDetails, json)
                .done(function (response) {
                    if (response.isError === false) {
                        if (response.ud.Json != "")
                            var arrJsonWidget = JSON.parse(response.ud.Json)
                        else
                            var arrJsonWidget = null
                        DashboardList.push(response.ud)
                        IsFavDashboard(response.ud.IsFav);


                        $("#spanWidgetHeading xmp").html(response.ud.Name)
                        if (arrJsonWidget != null) {
                            WidgetList = arrJsonWidget
                            dashboardfunc.ReadWidgetJson(arrJsonWidget);
                        }
                        ResetBtnCheck(WidgetList);
                    } else {
                        dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: response.Msg });
                    }
                    $("#spinningWheel").hide();
                })
                .fail(function (xhr, status, error) {
                    $("#spinningWheel").hide();
                });
        }
    })
}
function ResetBtnCheck(WidgetList) {
    try {
        if (WidgetList.length == 0) { $("#btnReset").addClass("hidden-e"); }
        else { $("#btnReset").removeClass("hidden-e"); }

    } catch (ex) {
        console.log(ex);
    }
}
function IsFavDashboard(val) {
    if (val === true) {
        $('#btnFavBlank').addClass('hidden-e');
        $('#btnFavSolid').removeClass('hidden-e');
    }
    else {
        $('#btnFavSolid').addClass('hidden-e');
        $('#btnFavBlank').removeClass('hidden-e');
    }
}
function SaveWidget(modalId) {
    $("#spinningWheel").show();

    var json = {
        DashboardId: parseInt($("#hiddenDashboardId").val()),
        JsonString: JSON.stringify(WidgetList)
    }

    $.post(urls.SetDashboardJson, json)
        .done(function (response) {
            $("#spinningWheel").hide();
            $("#" + modalId).modal("hide");
            if (response.isError === false) {
                dashboardfunc.ShowErrorMsg(JSON.parse(localStorage.getItem('language'))["widgetSave"], 's')
                //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: JSON.parse(localStorage.getItem('language'))["widgetSave"] });
            } else {
                dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message:response.Msg });
            }
        })
        .fail(function (xhr, status, error) {
            $("#spinningWheel").hide();
            $("#" + modalId).modal("hide");
            dashboardfunc.ShowErrorMsg(error.message, 'e')
            //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: error.message });
        });
}
/*Using this funciton for open task widget modal & Bind event of this modal*/
function LoadTask(id) {
    $("#spinningWheel").show();
    $('#divModalLoad').empty();
    $('#divModalLoad').load(urls.AddEditTaskListPartial, function () {
        $('#modalTask').ShowModel();
        dashboardfunc.ReloadModalSortable();
        if (id != undefined) {
            $("#titleAddEditTask").html(Language["editTaskList"])
            $("#btnTaskChart").html(Language["updateButton"])
            SetTaskListVal(id)
        }
        $("#spinningWheel").hide();

        BindLoadTaskClickEvent();
    });
}
function BindLoadTaskClickEvent() {
    /*Add Task List widget*/
    $("#btnTaskChart").click(() => {

        $("#spinningWheel").show();

        var WidgetName = $("#taskWidgetName").val();
        var Interval = $("#taskRefreshTime").val();
        var DashboardId = $("#hiddenDashboardId").val();
        var id = $("#hiddenTaskWidgetId").val()
        var $form = $('#frmAddEditTask');
        var serializedForm = $form.serialize();

        ValidateFormTask();
        
        if ($form.valid()) {

            var obj = WidgetTaskJson(new Date().getTime().toString(), -1, WidgetName, Interval, '571px', '257px');

            if (id == 0) {
                obj.Order = (typeof WidgetList.length == 'undefined' ? 0 : WidgetList.length);
                WidgetList.push(obj);
            }

            else {
                for (let i = 0; i < WidgetList.length; i++) {
                    if (WidgetList[i].Id == id) {
                        obj.Order = i;
                        obj.WidgetHeight = WidgetList[i].WidgetHeight;
                        obj.WidgetWidth = WidgetList[i].WidgetWidth;
                        WidgetList[i] = obj
                    }
                }
            }

            var json = {
                DashboardId: parseInt($("#hiddenDashboardId").val()),
                JsonString: JSON.stringify(WidgetList)
            }

            $.post(urls.SetDashboardJson, json)
                .done(function (response) {

                    if (id == 0) {
                        dashboardfunc.ReadWidgetJson([obj]);
                    }
                    else {
                        $("#" + id + " #spanWidgetHeading").html(obj.WidgetName)
                        dashboardfunc.ReadWidgetData(obj)
                    }

                    $("#spinningWheel").hide();
                    $("#modalTask").modal("hide");

                    if (id == 0) {
                        dashboardfunc.ShowErrorMsg(WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"] , 's')
                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"] });
                    }
                    else {
                        dashboardfunc.ShowErrorMsg(WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"], 's')
                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"] });
                    }
                    dashboardfunc.ScrollDownSet()
                    
                    dashboardfunc.UpdateLocalDashboardList(DashboardId, WidgetList);
                })
                .fail(function (xhr, status, error) {
                    $("#spinningWheel").hide();
                    $("#modalTask").modal("hide");
                    dashboardfunc.ShowErrorMsg(error.message, 'e')
                    //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: error.message });
                });
        } else {
            $("#spinningWheel").hide();
        }
    })
}
function ValidateFormTask(){
    $('#frmAddEditTask').validate({
        rules: {
            taskWidgetName: {
                required: true,
                rangelength: [2, 50]
            }
        },
        ignore: ":hidden:not(select)",
        highlight: function (element) {
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-error');
        },
        errorElement: 'span',
        errorClass: 'help-block',
        errorPlacement: function (error, element) {
            if (element.parent('.input-group').length) {
                error.insertAfter(element.parent());
            } else {
                error.insertAfter(element);
            }
        }
    });
}
function SetTaskListVal(id) {

    var dashboardID = parseInt($("#hiddenDashboardId").val());
    var dashboardJson = DashboardList.find(c => c.ID == dashboardID)
    var parsedJson = JSON.parse(dashboardJson.Json)
    var widgetJson = parsedJson.find(c => c.Id == id)
    $("#taskWidgetName").val(widgetJson.WidgetName);
    $("#hiddenTaskWidgetId").val(id)

    var selectize = $("#taskRefreshTime").selectize()
    var sel = selectize[0].selectize
    sel.setValue(widgetJson.Interval)

}
/*Using this funciton for open bar, pie and data grid modal & Bind event of this modal*/
function LoadChart(id) {
    $("#spinningWheel").show();
    $('#divModalLoad').empty();
    $('#divModalLoad').load(urls.AddEditChartPartial, function () {
        $('#modal13').ShowModel();
        dashboardfunc.ReloadModalSortable();

        var type1 = $("#hiddenChartVal").val();
        if (type1 == "BAR") {
            $("#titleAddEditChart").html(Language["addBarChart"])

        } else if (type1 == "PIE") {
            $("#titleAddEditChart").html(Language["addPieChart"])

        } else {
            $("#titleAddEditChart").html(Language["addDataGrid"])
        }

        $("#lstTypeChart").val(type1);

        if (type1 == "DATA") {
            $("#divChartlistCol").hide()
            $("#chartlstCol").val("-")
            BindLoadChartEvent()
            ValidateAddChart(false)
        } else {
            BindLoadChartEvent()
            BindLoadChartEvent2()
            ValidateAddChart(true)
        }

        if (id != undefined) {
            if (type1 == "BAR") {
                $("#titleAddEditChart").html(Language["editBarChart"])

            } else if (type1 == "PIE") {
                $("#titleAddEditChart").html(Language["editPieChart"])

            } else {
                $("#titleAddEditChart").html(Language["editDataGrid"])
            }
            $("#btnAddChart").html(Language["updateButton"])
            SetChartVal(id)
        }

        $("#spinningWheel").hide();

    });
}
function SetChartVal(id) {

    var dashboardID = parseInt($("#hiddenDashboardId").val());
    var dashboardJson = DashboardList.find(c => c.ID == dashboardID)
    var parsedJson = JSON.parse(dashboardJson.Json)
    var widgetJson = parsedJson.find(c => c.Id == id)
    $("#chartWidgetName").val(widgetJson.WidgetName);
    $("#hiddenChartWidgetId").val(id)

    var selectize = $("#chartRefreshTime").selectize()
    var sel = selectize[0].selectize
    sel.setValue(widgetJson.Interval)

    $("#hiddenchartListTable").val(widgetJson.TableName)
    $("#hiddenchartlstView").val(widgetJson.ParentView)
    $("#hiddenchartlstCol").val(widgetJson.Column)

    var selectize = $("#chartlstWorkGroup").selectize()
    var sel = selectize[0].selectize
    sel.setValue(widgetJson.WorkGroup)

    if (widgetJson.WidgetType == WIDGET_TYPE.DATA.NAME) {
        $("#chartlstCol").val(widgetJson.Column)
    }
}
function BindLoadChartEventChange() {
    /*Set values for table attributel*/
    $("#chartlstWorkGroup").change(function () {
        $("#spinningWheel").show();

        ClearAndAddOption("chartListTable")
        ClearAndAddOption("chartlstView")
        ClearAndAddOption("chartlstCol")
        var json = {
            workGroupId: parseInt($(this).val())
        }
        $.get(urls.GetWorkGroupTableMenu, json)
            .done(function (response) {
                if (response.isError) {
                    dashboardfunc.ShowErrorMsg(response.ErrorMessage, 'e')
                    $("#spinningWheel").hide();
                    return;
                }
                $("#spinningWheel").hide();

                var $select = $("#chartListTable").selectize()
                var selectize = $select[0].selectize;
                for (let r of response.WorkGroupMenu) {
                    selectize.addOption({ value: r.TableName, text: r.UserName });
                }

                if ($("#hiddenchartListTable").val() != "") {
                    selectize.setValue($("#hiddenchartListTable").val())
                }

                RemoveValidatorChart("frmAddEditChart","chartlstWorkGroup")
            })
            .fail(function (xhr, status, error) {
                $("#spinningWheel").hide();
            });
    })
}
function BindLoadChartEventClick() {
    /*Pie and Bar Chart, View Grid Modal*/
    $("#btnAddChart").click(() => {

        $("#spinningWheel").show();
        var WidgetName = $("#modal13 #chartWidgetName").val();
        var Interval = $("#modal13 #chartRefreshTime").val();
        var Type = $("#modal13 #lstTypeChart").val();
        var View = $("#modal13 #chartlstView").val();
        var Column = $("#modal13 #chartlstCol").val();
        var DisplayColumn = $("#modal13 #chartlstCol").text();
        var WorkGroup = $("#modal13 #chartlstWorkGroup").val();
        var Table = $("#modal13 #chartListTable").val();
        var DashboardId = $("#hiddenDashboardId").val();
        var id = $("#hiddenChartWidgetId").val();

        var $form = $('#frmAddEditChart');
        var serializedForm = $form.serialize();

        if ($form.valid()) {
            var obj;
            var Id = new Date().getTime().toString();
            var Value = WIDGET_TYPE[Type].NAME;
            const InitialWidth = dashboardfunc.GetWidgetDefaultWidth()
            const InitialHeight = '257px';

            if (Value == WIDGET_TYPE.BAR.NAME) {
                obj = WidgetBarJson(Id, -1, WidgetName, View, Column, Interval, InitialWidth, InitialHeight, Table, WorkGroup, DisplayColumn, true);
            } else if (Value == WIDGET_TYPE.PIE.NAME) {
                obj = WidgetPieJson(Id, -1, WidgetName, View, Column, Interval, InitialWidth, InitialHeight, Table, WorkGroup, true, DisplayColumn);
            } else if (Value == WIDGET_TYPE.DATA.NAME) {
                obj = WidgetGridJson(Id, -1, WidgetName, View, Column, Interval, InitialWidth, InitialHeight, Table, WorkGroup);
            }

            if (id == 0) {
                obj.Order = (typeof WidgetList.length == 'undefined' ? 0 : WidgetList.length);
                WidgetList.push(obj);
            }
            else {
                for (let i = 0; i < WidgetList.length; i++) {
                    if (WidgetList[i].Id == id) {
                        obj.Order = i; obj.WidgetHeight = WidgetList[i].WidgetHeight; obj.WidgetWidth = WidgetList[i].WidgetWidth; obj.Id = id; WidgetList[i] = obj
                    }
                }
            }

            var json = {DashboardId: parseInt($("#hiddenDashboardId").val()), JsonString: JSON.stringify(WidgetList)}
            var jsonCount = {widgetObjectJson: JSON.stringify(obj),}

            $.post(urls.CountBarChartData, jsonCount)
                .done(function (res) {
                    if (res.isError === false) {
                        if (res.Count > ChartDataTooLarge && obj.WidgetType !="DATA") {
                            TooLargeData("#modal13",id,obj)
                        } else {
                            $.post(urls.SetDashboardJson, json)
                .done(function (response) {
                    if (id == 0) {
                        dashboardfunc.ReadWidgetJson([obj]);
                        dashboardfunc.ShowErrorMsg(WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"], 's')
                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"] });
                    }
                    else {

                        $("#" + id + " #spanWidgetHeading xmp").html(obj.WidgetName)
                        $("#" + id + " input").val(obj.WidgetName);
                        $("#" + id + " .customTooltip").attr("gloss", obj.WidgetName)
                        dashboardfunc.ReadWidgetData(obj);
                        dashboardfunc.ShowErrorMsg(WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"], 's')
                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"] });
                    }
                    $("#spinningWheel").hide();
                    $("#modal13").modal("hide");
                    ResetBtnCheck(WidgetList);
                    dashboardfunc.UpdateLocalDashboardList(DashboardId, WidgetList);
                })
                .fail(function (xhr, status, error) {
                    $("#spinningWheel").hide();
                    $("#modal13").modal("hide");
                    dashboardfunc.ShowErrorMsg(error.message , 'e')
                    //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: error.message });
                });
                        }
                    } else {
                        dashboardfunc.ShowErrorMsg(JSON.parse(localStorage.getItem('language'))["somethingwentwrong"], 'e')
                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: JSON.parse(localStorage.getItem('language'))["somethingwentwrong"] });
                    }
                })
                .fail(function (xhr, status, error) {

                });
        } else {
            $("#spinningWheel").hide();
        }
    })

}
function BindLoadChartEvent2() {
    /*Set values for coloum attribute needs to be updated*/
    $("#chartlstView").change(function () {
        $("#spinningWheel").show();
        ClearAndAddOption("chartlstCol")

        var json = {
            viewId: parseInt($(this).val())
        }
        $.get(urls.GetViewColumnMenu, json)
            .done(function (response) {
                if (response.isError) {
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                    $("#spinningWheel").hide();
                    return;
                }
                $("#spinningWheel").hide();
                var $select = $("#chartlstCol").selectize()
                var selectize = $select[0].selectize;
                for (let r of response.ViewColumnEntity) {
                    selectize.addOption({ value: r.FieldName, text: r.Name });
                }

                if ($("#hiddenchartlstCol").val() != "") {
                    selectize.setValue($("#hiddenchartlstCol").val())
                    $("#hiddenchartlstCol").val("")
                }
                RemoveValidatorChart("frmAddEditChart", "chartlstView")

            })
            .fail(function (xhr, status, error) {
                $("#spinningWheel").hide();
            });
    })
    $("#chartlstCol").change(function () {
        RemoveValidatorChart("frmAddEditChart","chartlstCol")
    })
}
function BindLoadChartEvent() {
    BindLoadChartEventClick();
    BindLoadChartEventChange();
    BindLoadChartTableEventChange();
    BindSearableDropdown("chartlstWorkGroup");
    BindSearableDropdown("chartListTable");
    BindSearableDropdown('chartlstView')
    BindSearableDropdown('chartlstCol')
    BindSearableDropdown('chartRefreshTime')
    $("#chartlstView").change(function () {
        RemoveValidatorChart("frmAddEditChart","chartlstView")
    })
}
function BindLoadChartTableEventChange() {
    /*Set values for views attributel*/
    $("#chartListTable").change(function () {
        $("#spinningWheel").show();

        ClearAndAddOption("chartlstView")
        ClearAndAddOption("chartlstCol")
        var json = {
            tableName: $(this).val()
        }
        $.get(urls.GetViewMenu, json)
            .done(function (response) {
                if (response.isError) {
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                    $("#spinningWheel").hide();
                    return;
                }
                $("#spinningWheel").hide();
                var $select = $("#chartlstView").selectize()
                var selectize = $select[0].selectize;
                for (let r of response.ViewsByTableName) {
                    selectize.addOption({ value: r.Id, text: r.ViewName });
                }

                if ($("#hiddenchartlstView").val() != "") {
                    selectize.setValue($("#hiddenchartlstView").val())
                }
                RemoveValidatorChart("frmAddEditChart","chartListTable")
            })
            .fail(function (xhr, status, error) {
                $("#spinningWheel").hide();
            });
    })
}
function ValidateAddChart(chartlstViewReq) {
    $('#frmAddEditChart').validate({
        debug:true,
        rules: {
            chartWidgetName: {
                required: true,
                rangelength: [2, 50]
            },
            chartlstView: {
                required: true
            },
            chartlstCol: {
                required: chartlstViewReq
            },
            chartlstWorkGroup: {
                required: true
            },
            chartListTable: {
                required: true
            }
        },
        ignore: ":hidden:not(select)",
        highlight: function (element) {
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-error');
        },
        errorElement: 'span',
        errorClass: 'help-block',
        errorPlacement: function (error, element) {
            if (element.parent('.input-group').length) {
                error.insertAfter(element.parent());
            } else {
                error.insertAfter(element);
            }
        }
    });
}
/*Using this funciton for open Bar Chart Objects Tracked By Day modal & Bind event of this modal*/
function LoadTracked(id) {
    
        $("#spinningWheel").show();
        $('#divModalLoad').empty();
        $('#divModalLoad').load(urls.AddEditTrackedPartial, function (data) {

            $('#modalTracked').ShowModel();
            dashboardfunc.ReloadModalSortable();
            /*for multi-select drop-down*/
            $('.example-getting-started').multiselect({
                includeSelectAllOption: true,
                maxHeight: 118
            });

            if (id != undefined) {
                $("#titleAddEditTracked").html(Language["editBarChartObjByday"])
                $("#btnObjectsTrackedChart").html(Language["updateButton"])
                SetTrackedVal(id)
            }
            $("#spinningWheel").hide();
            BindTrackedEvent();
            ValidateTracked();
        });
    
}
function BindTrackedEvent() {
    BindTrackedClickEvent();
    $("#trackedListTable").change(function () {
        RemoveValidatorChart("frmAddEditTracked","trackedListTable")
    })
}
function ValidateTracked() {
    $('#frmAddEditTracked').validate({
        rules: {
            trackedWidgetName: {
                required: true,
                rangelength: [2, 50]
            },
            trackedListTable: {
                required: true
            },
            trackedlstPeriod: {
                required: true
            },
            trackedlstfilter: {
                required: true
            }
        },
        ignore: ":hidden:not(select)",
        highlight: function (element) {
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-error');
        },
        errorElement: 'span',
        errorClass: 'help-block',
        errorPlacement: function (error, element) {
            if (element.parent('.input-group').length) {
                error.insertAfter(element.parent());
            } else {
                error.insertAfter(element);
            }
        }
    });
}
function BindTrackedClickEvent() {
    /*Bar Chart Objects Tracked By Day*/
    $("#btnObjectsTrackedChart").click(() => {

        $("#spinningWheel").show();

        var WidgetName = $("#modalTracked #trackedWidgetName").val();
        var Interval = $("#modalTracked #trackedRefreshTime").val();
        var Object = $("#modalTracked #trackedListTable").val();
        var Filter = $("#modalTracked #trackedlstfilter").val();
        var Period = $("#modalTracked #trackedlstPeriod").val();
        var DashboardId = $("#hiddenDashboardId").val();
        var id = $("#hiddenTrackedWidgetId").val();
        var TableName = '';
        var InitialWidth = dashboardfunc.GetWidgetDefaultWidth()

        $("#modalTracked #trackedListTable :selected").each(function (index) {
            if (index == 0) {
                TableName = $(this).text()
            } else {
                TableName = TableName + ',' + $(this).text()
            }
        })
        
        var $form = $('#frmAddEditTracked');
        var serializedForm = $form.serialize();

        if ($form.valid()) {
            var Id = new Date().getTime().toString();
            var obj = WidgetTrackedJson(Id, -1, WidgetName, Object, Period, Interval, InitialWidth, '257px', Filter, TableName, true);

            if (id == 0) { 
                obj.Order = (typeof WidgetList.length == 'undefined' ? 0 : WidgetList.length);
                WidgetList.push(obj);
            }
            else {
                for (let i = 0; i < WidgetList.length; i++) {
                    if (WidgetList[i].Id == id) {
                        obj.Order = i; obj.WidgetHeight = WidgetList[i].WidgetHeight; obj.WidgetWidth = WidgetList[i].WidgetWidth; obj.Id = id; WidgetList[i] = obj
                    }
                }
            }

            var json = { DashboardId: parseInt($("#hiddenDashboardId").val()), JsonString: JSON.stringify(WidgetList)}

            var jsonCount = { widgetObjectJson: JSON.stringify(obj) }

            $.post(urls.CountTrackedData, jsonCount)
                .done(function (res) {
                    if (res.isError === false) {
                        if (res.Count > ChartDataTooLarge) {
                            TooLargeData("#modalTracked", id, obj)
                        } else {
                            $.post(urls.SetDashboardJson, json)
                                .done(function (response) {

                                    if (id == 0) {
                                        dashboardfunc.ReadWidgetJson([obj]);
                                        dashboardfunc.ShowErrorMsg(WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"], 's')
                                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"] });
                                    }
                                    else {
                                        $("#" + id + " #spanWidgetHeading").html(obj.WidgetName)
                                        dashboardfunc.ReadWidgetData(obj)
                                        dashboardfunc.ShowErrorMsg(WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"], 's')
                                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"] });
                                    }
                                    $("#spinningWheel").hide();
                                    $("#modalTracked").modal("hide");
                                    dashboardfunc.ScrollDownSet();
                                    dashboardfunc.UpdateLocalDashboardList(DashboardId, WidgetList);
                                })
                                .fail(function (xhr, status, error) {
                                    $("#spinningWheel").hide();
                                    $("#modalTracked").modal("hide");
                                    dashboardfunc.ShowErrorMsg(error.message, 'e')
                                    //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: error.message });
                                });
                        }
                    } else {
                        dashboardfunc.ShowErrorMsg(JSON.parse(localStorage.getItem('language'))["somethingwentwrong"], 'e')
                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: JSON.parse(localStorage.getItem('language'))["somethingwentwrong"] });
                    }
                })
                .fail(function (xhr, status, error) {

                });
            
        } else {
            $("#spinningWheel").hide();
        }

    })
}
function SetTrackedVal(id) {
    var dashboardID = parseInt($("#hiddenDashboardId").val());
    var dashboardJson = DashboardList.find(c => c.ID == dashboardID)
    var parsedJson = JSON.parse(dashboardJson.Json)
    var widgetJson = parsedJson.find(c => c.Id == id)


    $("#trackedListTable").val(widgetJson.Objects)
    $("#trackedListTable").multiselect("refresh");

    $("#trackedWidgetName").val(widgetJson.WidgetName);
    $("#trackedlstfilter").val(widgetJson.Filter);
    $("#trackedlstPeriod").val(widgetJson.Period);
    $("#hiddenTrackedWidgetId").val(id)

    var selectize = $("#trackedRefreshTime").selectize()
    var sel = selectize[0].selectize
    sel.setValue(widgetJson.Interval)

}
/*Using this funciton for open Bar Chart Objects User Operations By Day modal & Bind event of this modal*/
function LoadOperation(id) {
        $("#spinningWheel").show();
        $('#divModalLoad').empty();
        $('#divModalLoad').load(urls.AddEditOperationPartial, function () {
            $('#modalOperation').ShowModel();
            dashboardfunc.ReloadModalSortable();
            /*for multi-select drop-down*/
            $('.example-getting-started').multiselect({
                includeSelectAllOption: true,
                maxHeight: 118

            });
            /*Bar Chart Objects User Operations By Day*/
            if (id != undefined) {
                $("#titleAddEditOperation").html(Language["editBarChartObjUserOperationByday"])
                $("#btnUserOperationsChart").html(Language["updateButton"])
                SetOperationVal(id)
            }
            $("#spinningWheel").hide();
            BindOperationChartEvent();
            ValidateOperationForm();
        });
}
function BindOperationChartEvent() {
    BindOperationChartAddClickEvent()
    BindOperationChartObjectChangeEvent()
    BindOperationChartUserChangeEvent()
    //BindSearableDropdown("lstAuditType");
    BindSearableDropdown("lstUsers");
}
function BindOperationChartUserChangeEvent() {
    $("#lstUsers").change(function () {
        RemoveValidatorChart("frmAddEditOperation","lstUsers")
    })
}
function BindOperationChartObjectChangeEvent() {
    $("#lstObject").change(function () {
        RemoveValidatorChart("frmAddEditOperation","lstObject")
    })
}
function BindOperationChartAddClickEvent(){
    $("#btnUserOperationsChart").click(() => {

        $("#spinningWheel").show();

        var WidgetName = $("#modalOperation #bcoWidgetName").val();
        var Interval = $("#modalOperation #operationRefreshTime").val();
        var Object = $("#modalOperation #lstObject").val();
        var Filter = $("#modalOperation #operationlstfilter").val();
        var Period = $("#modalOperation #operationlstPeriod").val();
        var Users = $("#modalOperation #lstUsers").val().split();
        var AuditType = $("#modalOperation #lstAuditType").val();
        var DashboardId = $("#hiddenDashboardId").val();
        var id = $("#hiddenOperationWidgetId").val();
        var TableName = '';
        var InitialWidth = dashboardfunc.GetWidgetDefaultWidth()
        $("#modalOperation #lstObject option:selected").each(function (index) {
            if (index == 0) {
                TableName = $(this).text()
            } else {
                TableName = TableName + ',' + $(this).text()
            }
        })

        var $form = $('#frmAddEditOperation');
        var serializedForm = $form.serialize();
        if ($form.valid()) {
            var Id = new Date().getTime().toString();
            var obj = WidgetObjectJson(Id, -1, WidgetName, Users, AuditType, Object, Period, Interval, InitialWidth, '257px', TableName, Filter, true);

            if (id == 0) {
                obj.Order = (typeof WidgetList.length == 'undefined' ? 0 : WidgetList.length);
                WidgetList.push(obj);
            }
            else {
                for (let i = 0; i < WidgetList.length; i++) {
                    if (WidgetList[i].Id == id) {
                        obj.Order = i; obj.WidgetHeight = WidgetList[i].WidgetHeight; obj.WidgetWidth = WidgetList[i].WidgetWidth; obj.Id = id;WidgetList[i] = obj
                    }
                }
            }

            var json = { DashboardId: parseInt($("#hiddenDashboardId").val()), JsonString: JSON.stringify(WidgetList) }

            var jsonCount = { widgetObjectJson: JSON.stringify(obj)}

            $.post(urls.CountOperationsData, jsonCount)
                .done(function (res) {
                    if (res.isError === false) {
                        if (res.Count > ChartDataTooLarge) {
                            TooLargeData("#modalOperation",id,obj)
                        } else {
                            $.post(urls.SetDashboardJson, json)
                                .done(function (response) {
                                    if (id == 0) {
                                        dashboardfunc.ReadWidgetJson([obj]);
                                        $("#spinningWheel").hide();
                                        $("#modalOperation").modal("hide");
                                        dashboardfunc.ShowErrorMsg(WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"], 's')
                                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"] });
                                    }
                                    else {
                                        $("#" + id + " #spanWidgetHeading").html(obj.WidgetName)
                                        dashboardfunc.ReadWidgetData(obj)
                                        $("#spinningWheel").hide();
                                        $("#modalOperation").modal("hide");
                                        dashboardfunc.ShowErrorMsg(WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"], 's')
                                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"] });
                                    }

                                    dashboardfunc.ScrollDownSet()
                                    dashboardfunc.UpdateLocalDashboardList(DashboardId, WidgetList);
                                })
                                .fail(function (xhr, status, error) {
                                    $("#spinningWheel").hide();
                                    $("#modalOperation").modal("hide");
                                    dashboardfunc.ShowErrorMsg(error.message, 'e')
                                    //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: error.message });
                                });
                        }
                    } else {
                        dashboardfunc.ShowErrorMsg(JSON.parse(localStorage.getItem('language'))["somethingwentwrong"], 'e')
                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: JSON.parse(localStorage.getItem('language'))["somethingwentwrong"] });
                    }
                })
                .fail(function (xhr, status, error) {

                });
            
        } else {
            $("#spinningWheel").hide();
        }

    })
}
function ValidateOperationForm() {
    // check validate jquery
    $('#frmAddEditOperation').validate({
        debug:true,
        rules: {
            bcoWidgetName: {
                required: true,
                rangelength: [2, 50]
            },
            lstObject: {
                required: true
            },
            lstUsers: {
                required: true
            },
            lstAuditType: {
                required: false
            },
            lstView: {
                required: true
            },
            operationlstfilter: {
                required: true
            },
            operationlstPeriod: {
                required: true
            }
        },
        ignore: ":hidden:not(select)",
        highlight: function (element) {
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-error');
        },
        errorElement: 'span',
        errorClass: 'help-block',
        errorPlacement: function (error, element) {
            if (element.parent('.input-group').length) {
                error.insertAfter(element.parent());
            } else {
                error.insertAfter(element);
            }
        }
    });
}
function SetOperationVal(id) {

    var dashboardID = parseInt($("#hiddenDashboardId").val());
    var dashboardJson = DashboardList.find(c => c.ID == dashboardID)
    var parsedJson = JSON.parse(dashboardJson.Json)
    var widgetJson = parsedJson.find(c => c.Id == id)
    $("#bcoWidgetName").val(widgetJson.WidgetName);

    $("#lstObject").val(widgetJson.Objects)
    $("#lstObject").multiselect("refresh");

    //$("#lstUsers").val(widgetJson.Users[0])
    //$("#lstUsers").multiselect("refresh");

    var selectize = $("#lstUsers").selectize()
    var sel = selectize[0].selectize
    sel.setValue(widgetJson.Users[0])

    $("#lstAuditType").val(widgetJson.AuditType)
    $("#lstAuditType").multiselect("refresh");

    $("#operationlstfilter").val(widgetJson.Filter);
    $("#operationlstPeriod").val(widgetJson.Period);
    $("#lstAuditType").val(widgetJson.AuditType);
    $("#hiddenOperationWidgetId").val(id)

    var selectize = $("#operationRefreshTime").selectize()
    var sel = selectize[0].selectize
    sel.setValue(widgetJson.Interval)

}
/*Using this funciton for open Bar Chart Time Series modal & Bind event of this modal*/
function LoadSeries(id) {
        $("#spinningWheel").show();
        $('#divModalLoad').empty();
        $('#divModalLoad').load(urls.AddEditSeriesPartial, function () {
            $('#modalSeries').ShowModel();
            dashboardfunc.ReloadModalSortable();

            BindLoadSeriesEvent()

            if (id != undefined) {
                $("#titleAddEditSeries").html(Language["editBarChartTimeSeries"])
                $("#btnTimeSeriesChart").html(Language["updateButton"])
                SetSeriesVal(id)
            }

            $("#spinningWheel").hide();
            ValidateFormSeries();
        });
}
function BindLoadSeriesEvent() {

    BindLoadSeriesEventClick();
    BindLoadSeriesEventChange();
    BindLoadSeriesTableEventChange();
    BindLoadSeriesViewEventChange();
}
function BindLoadSeriesViewEventChange() {
    /*Set values for coloum attribute needs to be updated*/
    $("#serieslstView").change(function () {
        $("#spinningWheel").show();
        var colList = $("#serieslstCol");
        colList.empty()
        colList.append($("<option></option>").val('').html(''));
        var json = {
            viewId: parseInt($(this).val()),
            tableName: $("#seriesListTable").val()
        }
        $.get(urls.GetViewColumnOnlyDate, json)
            .done(function (response) {
                if (response.isError) {
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                    $("#spinningWheel").hide();
                    return;
                }
                $("#spinningWheel").hide(); 
                for (let r of response.ViewColumnEntity) {
                    colList.append($("<option></option>").val(r.FieldName).html(r.Name))
                }

                if ($("#hiddenserieslstCol").val() != "") {
                    $("#serieslstCol").val($("#hiddenserieslstCol").val())
                    $("#hiddenserieslstCol").val("")
                }
            })
            .fail(function (xhr, status, error) {
                $("#spinningWheel").hide();
            });
    })
}
function BindLoadSeriesTableEventChange() {
    /*Set values for views attributel*/
    $("#seriesListTable").change(function () {
        $("#spinningWheel").show();
        var viewList = $("#serieslstView");
        viewList.empty()
        $("#serieslstCol").empty()
        viewList.append($("<option></option>").val('').html(''));
        $("#serieslstCol").append($("<option></option>").val('').html(''));
        var json = {
            tableName: $(this).val()
        }
        $.get(urls.GetViewMenu, json)
            .done(function (response) {
                if (response.isError) {
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                    $("#spinningWheel").hide();
                    return;
                }
                $("#spinningWheel").hide();
                for (let r of response.ViewsByTableName) {
                    viewList.append($("<option></option>").val(r.Id).html(r.ViewName))
                }

                if ($("#hiddenserieslstView").val() != "") {
                    $("#serieslstView").val($("#hiddenserieslstView").val()).change()
                    $("#hiddenserieslstView").val("")
                }
            })
            .fail(function (xhr, status, error) {
                $("#spinningWheel").hide();
            });
    })

}
function BindLoadSeriesEventChange() {
    /*Set values for table attributel*/
    $("#serieslstWorkGroup").change(function () {
        $("#spinningWheel").show();
        var tableList = $("#seriesListTable");
        tableList.empty()
        $("#serieslstView").empty()
        $("#serieslstCol").empty()
        tableList.append($("<option></option>").val('').html(''));
        $("#serieslstView").append($("<option></option>").val('').html(''));
        $("#serieslstCol").append($("<option></option>").val('').html(''));
        var json = {
            workGroupId: parseInt($(this).val())
        }
        $.get(urls.GetWorkGroupTableMenu, json)
            .done(function (response) {
                if (response.isError) {
                    dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                    $("#spinningWheel").hide();
                    return;
                }
                $("#spinningWheel").hide();
                for (let r of response.WorkGroupMenu) {
                    tableList.append($("<option></option>").val(r.TableName).html(r.UserName))
                }

                if ($("#hiddenseriesListTable").val() != "") {
                    $("#seriesListTable").val($("#hiddenseriesListTable").val()).change()
                    $("#hiddenseriesListTable").val("")
                }
            })
            .fail(function (xhr, status, error) {
                $("#spinningWheel").hide();
            });
    })
}
function BindLoadSeriesEventClick() {
    /*Bar Chart Time Series*/
    $("#btnTimeSeriesChart").click(() => {
        console.log("series add")
        $("#spinningWheel").show();

        var WidgetName = $("#modalSeries #bcsdWidgetName").val();
        var WorkGroup = $("#modalSeries #serieslstWorkGroup").val();
        var Table = $("#modalSeries #seriesListTable").val()
        var Interval = $("#modalSeries #seriesRefreshTime").val();
        var View = $("#modalSeries #serieslstView").val();
        var Filter = $("#modalSeries #serieslstfilter").val();
        var Column = $("#modalSeries #serieslstCol").val();
        var DisplayColumn = $("#modalSeries #serieslstCol  option:selected").text();
        var Period = $("#modalSeries #serieslstPeriod").val();
        var DashboardId = $("#hiddenDashboardId").val();
        var id = $("#hiddenSeriesWidgetId").val();
        const InitialWidth = dashboardfunc.GetWidgetDefaultWidth()

        var $form = $('#frmAddEditSeries');
        var serializedForm = $form.serialize();
        if ($form.valid()) {
            var Id = new Date().getTime().toString();
            var obj = WidgetSeriesJson(Id, -1, WidgetName, View, Column, Period, Interval, InitialWidth, '257px', Table, WorkGroup, Filter, DisplayColumn, true);

            if (id == 0) {
                obj.Order = (typeof WidgetList.length == 'undefined' ? 0 : WidgetList.length);
                WidgetList.push(obj);
            }
            else {
                for (let i = 0; i < WidgetList.length; i++) {
                    if (WidgetList[i].Id == id) {
                        obj.Order = i; obj.WidgetHeight = WidgetList[i].WidgetHeight; obj.WidgetWidth = WidgetList[i].WidgetWidth; obj.Id = id; WidgetList[i] = obj
                    }
                }
            }

            var json = { DashboardId: parseInt($("#hiddenDashboardId").val()), JsonString: JSON.stringify(WidgetList)}

            var jsonCount = { widgetObjectJson: JSON.stringify(obj) }

            $.post(urls.CountTimeSeriesChartData, jsonCount)
                .done(function (res) {
                    if (res.isError === false) {
                        if (res.Count > ChartDataTooLarge) {
                            TooLargeData("#modalSeries", id, obj)
                        } else {
                            $.post(urls.SetDashboardJson, json)
                                .done(function (response) {
                                    if (id == 0) {
                                        console.log([obj]);
                                        dashboardfunc.ReadWidgetJson([obj]);
                                    }
                                    else {
                                        $("#" + id + " #spanWidgetHeading").html(obj.WidgetName)
                                        dashboardfunc.ReadWidgetData(obj)
                                    }
                                    $("#spinningWheel").hide();
                                    $("#modalSeries").modal("hide");
                                    if (id == 0) {
                                        dashboardfunc.ShowErrorMsg(WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"], 's')
                                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"] });
                                    }
                                    else {
                                        dashboardfunc.ShowErrorMsg(WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"], 's')
                                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"] });
                                    }
                                    dashboardfunc.ScrollDownSet()
                                    dashboardfunc.UpdateLocalDashboardList(DashboardId, WidgetList);
                                })
                                .fail(function (xhr, status, error) {
                                    $("#spinningWheel").hide();
                                    $("#modalSeries").modal("hide");
                                    dashboardfunc.ShowErrorMsg(error.message, 'e')
                                    //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: error.message });
                                });
                        }
                    } else {
                        dashboardfunc.ShowErrorMsg(JSON.parse(localStorage.getItem('language'))["somethingwentwrong"], 'e')
                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: JSON.parse(localStorage.getItem('language'))["somethingwentwrong"] });
                    }
                })
                .fail(function (xhr, status, error) {

                });
            
        } else {
            $("#spinningWheel").hide();
        }
    })
}
function SetSeriesVal(id) {

    var dashboardID = parseInt($("#hiddenDashboardId").val());
    var dashboardJson = DashboardList.find(c => c.ID == dashboardID)
    var parsedJson = JSON.parse(dashboardJson.Json)
    var widgetJson = parsedJson.find(c => c.Id == id)
    $("#bcsdWidgetName").val(widgetJson.WidgetName);
    $("#seriesRefreshTime").val(widgetJson.Interval);
    $("#serieslstfilter").val(widgetJson.Filter);
    $("#serieslstPeriod").val(widgetJson.Period);
    $("#hiddenSeriesWidgetId").val(id)

    $("#hiddenseriesListTable").val(widgetJson.TableName)
    $("#hiddenserieslstView").val(widgetJson.ParentView);
    $("#hiddenserieslstCol").val(widgetJson.Column);

    $("#serieslstWorkGroup").val(widgetJson.WorkGroup).change();
}
/*Fuction to remove widget from database*/
function DeleteWidgetFromDb(id) {

    $("#spinningWheel").show();
    var DashboardId = parseInt($("#hiddenDashboardId").val());
    let widgetName;
    for (let i = 0; i < WidgetList.length; i++) {
        if (WidgetList[i].Id == id) {
            widgetName = WidgetList[i].WidgetName ;
            WidgetList.splice(i, 1);
            break;
        }
    }

    var json = {
        DashboardId: DashboardId,
        JsonString: JSON.stringify(WidgetList)
    }

    $.post(urls.SetDashboardJson, json)
        .done(function (response) {
            dashboardfunc.UpdateLocalDashboardList(DashboardId, WidgetList);
            $("#spinningWheel").hide();
            ResetBtnCheck(WidgetList);
            dashboardfunc.ShowErrorMsg(widgetName + " " + JSON.parse(localStorage.getItem('language'))["deleteSuccess"], 's')
            //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: widgetName + " " + JSON.parse(localStorage.getItem('language'))["deleteSuccess"]});
        })
        .fail(function (xhr, status, error) {
            dashboardfunc.ShowErrorMsg(error.message, 'e')
            //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: error.message });
        });
}
function ValidateForm(formId) {
    var $form = $('#' + formId);
    $form.valid();
}
function ClearAndAddOption(id) {
    UnbindSelectize(id)
    var viewList = $("#" + id);
    viewList.empty()
    viewList.append($("<option></option>").val('').html(''));
    BindSearableDropdown(id)
}
function ValidateFormSeries() {
    // check validate jquery
    $('#frmAddEditSeries').validate({
        debug:true,
        rules: {
            bcsdWidgetName: {
                required: true,
                rangelength: [2, 50]
            },
            serieslstCol: {
                required: true
            },
            serieslstView: {
                required: true
            },
            serieslstPeriod: {
                required: true
            },
            seriesListTable: {
                required: true
            },
            serieslstWorkGroup: {
                required: true
            },
            serieslstfilter: {
                required: true
            }
        },
        ignore: ":hidden:not(select)",
        highlight: function (element) {
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-error');
        },
        errorElement: 'span',
        errorClass: 'help-block',
        errorPlacement: function (error, element) {
            if (element.parent('.input-group').length) {
                error.insertAfter(element.parent());
            } else {
                error.insertAfter(element);
            }
        }
    });

}
function UnbindSelectize(id) {
    try {
        $('#' + id)[0].selectize.destroy();
    } catch { }
}
function BindSearableDropdown(id){
    try {
        $('#'+id).selectize.destroy();
        $('#' + id).selectize({
            //sortField: 'text'
        });
    }
    catch
    {
        $('#' + id).selectize({
            //sortField: 'text'
        });
    }
}
function RemoveValidatorChart(formId,id) {
    $('#' + formId+' [for=' + id + ']').closest('.form-group').removeClass('has-error');
    $('#' + formId +' span[for=' + id + ']').css("display", "none")
}
/*for all widget common fun*/
function SaveWidgetAfterConfirmation() {
    var DashboardId = parseInt($("#hiddenDashboardId").val())
    var json = {
        DashboardId: DashboardId,
        JsonString: JSON.stringify(WidgetList),
    }
    var obj = JSON.parse($("#hiddenObjChart").val())
    var id = $("#hiddenObjChartWidgetId").val()
    $.post(urls.SetDashboardJson, json)
        .done(function (response) {
            if (id == 0) {
                dashboardfunc.ReadWidgetJson([obj]);
                dashboardfunc.ShowErrorMsg(obj.WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"], 's')
                //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: obj.WidgetName + " " + JSON.parse(localStorage.getItem('language'))["addSuccess"] });
            }
            else {
                $("#" + id + " #spanWidgetHeading xmp").html(obj.WidgetName)
                $("#" + id + " .customTooltip").attr("gloss", obj.WidgetName)
                dashboardfunc.ReadWidgetData(obj)
                dashboardfunc.ShowErrorMsg(obj.WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"], 'e')
                //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: obj.WidgetName + " " + JSON.parse(localStorage.getItem('language'))["updateSuccess"] });
            }
            $("#spinningWheel").hide();
            $("#modaConfirmToSaveChart").modal("hide")
            ResetBtnCheck(WidgetList);
            dashboardfunc.UpdateLocalDashboardList(DashboardId, WidgetList);
        })
        .fail(function (xhr, status, error) {
            $("#spinningWheel").hide();
            $("#modaConfirmToSaveChart").modal("hide")
            dashboardfunc.ShowErrorMsg(error.message, 'e')
            //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: error.message });
        });
}
function TooLargeData(modalId, id, obj) {
    $("#spinningWheel").hide();
    $(modalId).modal("hide");
    $("#modaConfirmToSaveChart").modal("show")
    $("#hiddenObjChart").val(JSON.stringify(obj));
    $("#hiddenObjChartWidgetId").val(id)
}