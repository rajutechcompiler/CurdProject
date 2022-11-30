
$(document).ready(function () {

    /*for multi-select drop-down*/
    $('.example-getting-started').multiselect();

    /*Make list sortable:*/
    $(function () {
        $('.sortable').sortable(
            {
                tolerance: "pointer",
                update : function (event, ui) {

                    $('#sortable .widget').each(function (index) {
                        var widgetId = $('#sortable .widget:nth-child(' + (index + 1)+ ')').attr("id")
                        for (let i = 0; i < WidgetList.length; i++) {
                            if (WidgetList[i].Id == widgetId) {
                                WidgetList[i].Order = index;
                            }
                        }
                    })  

                    let dashID = parseInt($("#hiddenDashboardId").val());
                    let json = {
                        DashboardId: dashID,
                        JsonString: JSON.stringify(WidgetList)
                    };

                    $.post(urls.SetDashboardJson, json)
                        .done(function (response) {
                            dashboardfunc.UpdateLocalDashboardList(dashID, WidgetList);
                        })
                        .fail(function (xhr, status, error) {
                            console.log(error);
                        });
                } 
            }
        );
    });

});

/*This Event are using for delete dashboard*/
$("#btnDeleteDashboard").click(() => {

    $("#spinningWheel").show();

    var id = parseInt($("#hiddenDashboardId").val())
    var json = {
        DashboardId : id
    }

    $.post(urls.DeleteDashboard, json)
        .done(function (response) {
            $("#spinningWheel").hide();
            $("#modalDeleteDashboard").modal("hide");
            if (response.isError === false) {
                dashboardfunc.ShowErrorMsg($("[dashbard-id=" + id + "] a xmp").html() + ' ' + JSON.parse(localStorage.getItem('language'))["deleteSuccess"], 's')
                //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: $("[dashbard-id=" + id + "] a xmp").html() + ' '+ JSON.parse(localStorage.getItem('language'))["deleteSuccess"] });
                $("[dashbard-id=" + id + "]").remove();
                $('#sortable').empty();
                $('#rename-dash').addClass('hidden-e');
                $('#del-dash').addClass('hidden-e');
                $('#new-widget').addClass('hidden-e');
                $('#btnClone').addClass('hidden-e');
                $('#btnFavBlank').removeClass('hidden-e').addClass('hidden-e');
                $('#btnFavSolid').removeClass('hidden-e').addClass('hidden-e');
                $('#btnReset').addClass('hidden-e');
                $("#spanWidgetHeading xmp").html("DASHBOARD")
                $("#hiddenDashboardId").val(0)
                $("#hiddenChartVal").val(0)
                setCookie("lastDashboardID", "")
            } else {
                dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: response.Msg });
            }
        })
        .fail(function (xhr, status, error) {
            $("#spinningWheel").hide();
            dashboardfunc.ShowErrorMsg(error.message, 'e')
            //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: error.message });
        });
});

/*Event to rename dashboard*/
$('#rename-dash').click(() => {

    $("#spinningWheel").show();
    var id = parseInt($("#hiddenDashboardId").val())
    var dashboardName = $("[dashbard-id=" + id + "] a xmp").html() ;  

    $('#divModalLoad').empty();
    $('#divModalLoad').load(urls.AddEditDashboardPartial, function () {

        $('#modalAddNewDashboard').ShowModel();
        $('#myModalmodalAddNewDashboard').html(Language["renameDashboard"]);
        $("#txtNewDashboardName").val(dashboardName);

        $("#spinningWheel").hide();

        /*This Event are using for create new Dashboard*/
        $("#btnSaveDashboardName").click(() => {

            $("#spinningWheel").show();

            var Name = $("#txtNewDashboardName").val();


            // check validate jquery
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

            var json = {
                DashboardId: id,
                Name: Name
            }

            var $form = $('#frmAddEditDashboard');
            if ($form.valid()) {
                $.post(urls.RenameDashboard, json)
                    .done(function (response) {
                        $("#spinningWheel").hide();
                        if (response.isError === false) {
                            if (response.ErrorMessage == "Already exists name.") {
                                dashboardfunc.ShowErrorMsg('"' + Name + '" ' + JSON.parse(localStorage.getItem('language'))["alreadyexistsname"], 'w')
                                //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-warning', message: '"' + Name + '" ' + JSON.parse(localStorage.getItem('language'))["alreadyexistsname"] });
                            } else {
                                $("#modalAddNewDashboard").modal("hide");
                                dashboardfunc.ShowErrorMsg(JSON.parse(localStorage.getItem('language'))["dashboardrenamedsuccessfully"], 's')
                                //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-success', message: JSON.parse(localStorage.getItem('language'))["dashboardrenamedsuccessfully"] });
                                $("[dashbard-id=" + id + "] a xmp").html(Name);
                                $("#spanWidgetHeading xmp").html(Name)
                                var index = DashboardList.findIndex(x => { return x.ID == id });
                                DashboardList[index].Name = Name;

                                $("#ulDashboardList").empty()
                                window.BindDashboardList(response.DashboardListHtml)
                                $("[dashbard-id=" + json.DashboardId + "] a").addClass("active")
                            }
                        } else {
                            $("#modalAddNewDashboard").modal("hide");
                            dashboardfunc.ShowErrorMsg(response.Msg, 'e')
                            //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: response.Msg });
                        }
                    })
                    .fail(function (xhr, status, error) {
                        $("#spinningWheel").hide();
                        $("#modalAddNewDashboard").modal("hide");
                        dashboardfunc.ShowErrorMsg(error.message, 'e')
                        //$('#toast-container').fnAlertMessage({ title: '', msgTypeClass: 'toast-error', message: error.message });
                    });
            } else {
                $("#spinningWheel").hide();
            }

        });
    });
});

$('#btnClone').click(() => {

    let id =  parseInt($("#hiddenDashboardId").val());
    let DashboardJson;
    for (let i = 0; i < DashboardList.length; i++) {
        if (DashboardList[i].ID==id) {
            DashboardJson = DashboardList[i].Json;
            break;
        }
    }

    LoadDashboard(DashboardJson);

});

$('#btnFavBlank').click(() => {
    $("#spinningWheel").show();
    if ($("#hiddenDashboardId").val() == "") {
        $("#spinningWheel").hide();
    }
    else {
        var json = {
            DashboardId: parseInt($("#hiddenDashboardId").val()),
            IsFav: true
        }

        dashboardfunc.UpdateFavouriteDashboardStatus(json);
        $('#btnFavBlank').addClass('hidden-e');
        $('#btnFavSolid').removeClass('hidden-e');
    }
})

$('#btnFavSolid').click(() => {
    $("#spinningWheel").show();
    var json = {
        DashboardId: parseInt($("#hiddenDashboardId").val()),
        IsFav: false
    }

    dashboardfunc.UpdateFavouriteDashboardStatus(json);
    $('#btnFavSolid').addClass('hidden-e');
    $('#btnFavBlank').removeClass('hidden-e');
})

/*Add New Dashboard & Refresh Dashboard Modal*/
$("#new-dashboard").click(function () {
    LoadDashboard();
});