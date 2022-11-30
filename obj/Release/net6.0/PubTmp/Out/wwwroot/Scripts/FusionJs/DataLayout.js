var ids = {
    None: 0,
    Password: 1,
    Index: 2,
    RequestDetails: 3,
    BatchRequest: 4,
    Tools: 5,
    FinalDisposition: 6,
    Localize: 7
};
class ToolsLayoutModel {
    DialogRun(id) {
        var _this = this;
        var data = { id: id };
        var call = new DataAjaxCall(app.url.server.LoadControls, ajax.Type.Get, ajax.DataType.Html, data, "", "", "", "");
        call.Send().then((data) => {
            //place content in content div
            $("#ContentDialog").html(data);
            //show dialog
            _this.DialogShow();
            _this.DialogOpenType(id);
        })
    }
    DialogShow() {
        $(app.domMap.Layout.MenuNavigation.ToolsDialog).show();
        var title = $("#titleid").val();
        $("#lblTitle").html(title);
        $('.modal-dialog').draggable({
            handle: ".modal-header",
            stop: function (event, ui) {
            }
        });
    }
    DialogClose() {
        $(app.domMap.Layout.MenuNavigation.ToolsDialog).hide();
    }
    DialogOpenType(id) {
        switch (id) {
            case ids.Password: break;
            case ids.Localize: this.LocalizationSetup(); break;
            case ids.Index: break;
            case ids.BatchRequest: break;
            case ids.Tools: this.ToolsSetup(); break;
            case ids.FinalDisposition: break;
            default:
        }
    }
    ToolsSetup() {
        var changePassword = document.getElementById("lbChangePassword");
        var betchRequest = document.getElementById("lbBatchRequest");
        var localization = document.getElementById("lnkLocalize");
        changePassword.innerHTML = app.language["ChangePassword"];
        betchRequest.innerHTML = app.language["BatchRequest"];
        localization.innerHTML = app.language["lnkToolsLanguageAndRegion"];
    }

    LocalizationSetup() {
        var ResLanguage = document.getElementById("Reslanguages").value;
        var selectCountry = document.getElementById("selectedCountry").value;
        var pDateChosen = document.getElementById("pDateChosen").value;
        $('#userLangSelect').flagStrap({
            countries: JSON.parse(ResLanguage), reload_change: false,
            selectedCountry: selectCountry
        });

        $('.chosen-select').chosen({ enable_split_word_search: false });
        $('select').val(pDateChosen);
        $('select').trigger('chosen:updated');
    }
    LocalizationSaveClick() {
        var item = $("#optdatetime").val();
        if (item === "-1") {
            $("#optdatetimeRequired").show();
            return false;
        }

        var model = {
            LanguageSelected: "",
            dateFormatSelected: $('.chosen-select')[0].selectedIndex
        };
        var options = $("select")[0];
        for (var i = 0; i < options.length; i++) {
            var checkOpt = options[i];
            if (checkOpt.defaultSelected === true) {
                model.LanguageSelected = options[i].value;
            }
        }
        var call = new DataAjaxCall(app.url.server.BtnSaveLocData_Click, ajax.Type.Post, ajax.DataType.Json, model, "", "", "", "");
        call.Send().then((data) => {
            if (data) {
                showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["msgLocalizeSave"], "s")
                toolsmodel.DialogClose();
                setTimeout(function () { window.location.reload() }, 2000);
               
            }
        })
    }
    PasswordSaveClick() {
        var model = {
            OldPass: document.getElementById("txtOldPass").value,
            NewPass1: document.getElementById("txtNewPass1").value,
            NewPass2: document.getElementById("txtNewPass2").value
        };
        var call = new DataAjaxCall(app.url.server.ChangePassword_click, ajax.Type.Post, ajax.DataType.Json, model, "", "", "", "");
        call.Send().then((data) => {
            var msg = $("#passMsg");
            msg.show();
            if (data.errorMessage !== "0") {
                msg.css("color", "red");
                msg.html(data.errorMessage);
            } else {
                //msg.css("color", "green");
                //msg.html("Password changed successfuly!");
                //msg.fadeOut(3000);
                showAjaxReturnMessage(JSON.parse(localStorage.getItem('language'))["PasswordChangedSucessMSG"], "s")
                toolsmodel.DialogClose()
            }
        });
    }
};

const toolsmodel = new ToolsLayoutModel();


$('#lbAboutUs').click(function () {
    $.ajax({
        type: "POST",
        url: '/Admin/OpenAboutUs',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,
        success: function (response) {
            var msg = response;
            $('#divAboutInfo').html(msg);
            $('#dialog-form-AboutUs').modal('show');
            $("#dialog-form-AboutUs .modal-dialog").draggable({ disabled: true });
        },
        failure: function (response) {
            return false;
        }
    });
});






