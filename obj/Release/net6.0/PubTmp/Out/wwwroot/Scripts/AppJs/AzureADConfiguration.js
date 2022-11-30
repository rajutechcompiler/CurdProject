$(function () {

    $.get(urls.AzureAD.AzureADGetConfiguration, function (data) {
        var tabSettings = JSON.parse(data);
        $.each(tabSettings, function (code, setting) {
            if (setting.Item === "ClientId")
                $("#AzureADClientId").val(setting.ItemValue);
            if (setting.Item === "ClientSecret")
                $("#AzureADClientSecret").val(setting.ItemValue);
            if (setting.Item === "EnabledAzureAD") {
                $("#chkAzureADEnabled").prop("checked", setting.ItemValue === "true" ? true : false);
            }
        });
    });
});

$("#btnApplyAzureAD").on('click', function () {
    var AzureADClientId = $("#AzureADClientId").val().trim();
    var AzureADClientSecret = $("#AzureADClientSecret").val().trim();
    var chkAzureADEnabled = $("#chkAzureADEnabled").is(":checked") ? true : false;
    if (validateAzureAD()) {
        $.post(urls.AzureAD.AzureADSetConfiguration, { clientId: AzureADClientId, clientSecret: AzureADClientSecret, isAdEnabled: chkAzureADEnabled }).done(function (response) {
                showAjaxReturnMessage(response.message, response.errortype);
        }).fail(function (xhr, status, error) {
            ShowErrorMessge();
        });
    }
});

function validateAzureAD() {
    if ($("#AzureADClientId").val().trim() === "") {
        showAjaxReturnMessage(vrApplicationRes["msgJsAzureADClientIdValidation"], "w");
        return false;
    }
    if ($("#AzureADClientSecret").val().trim() === "") {
        showAjaxReturnMessage(vrApplicationRes["msgJsAzureADClientSecretValidation"], "w");
        return false;
    }
    return true;
}  