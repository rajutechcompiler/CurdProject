using System;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.VisualBasic;
using slimShared;
using TabFusionRMS.Resource;

// ###### requirments for using this error class: 
// ###### you have to create two properties in your model 
// ###### 1. int errorNumber, 2. string Msg 
class DataErrorHandler
{
    private static int errorNumber;
    public static void ErrorHandler(Exception ex, BaseModel model, int viewid, string databaseName)
    {
        model.isError = true;
        if (ex is SqlException)
            errorNumber = ((SqlException)ex).Number;
        model.errorNumber = errorNumber;

        if (ex.TargetSite is null)
        {
            model.TesterErrMsg = string.Format("{0} - ViewId = {1} - DatabaseName = {2}", ex.Message, viewid, databaseName);
        }
        else
        {
            model.TesterErrMsg = string.Format("{0} - ViewId = {1} - DatabaseName = {2} {3}TargetSite: {4}", ex.Message, viewid, databaseName, Constants.vbCrLf, ex.TargetSite);
        }

        SlimShared.logMessage(model.TesterErrMsg, false, EventLogEntryType.Error, "TAB FusionRMS Web Access");
        BuildUsersCustomMessage(ex, model);
    }
    private static void BuildUsersCustomMessage(Exception ex, BaseModel model)
    {
        switch (errorNumber)
        {
            case 2627:
                {
                    model.Msg = "The unique value is duplicated, the change will not save, Please, change the value in order to Add this row.";
                    break;
                }
            case 248:
                {
                    model.Msg = "The value is too long";
                    break;
                }
            case 515:
                {
                    model.Msg = "can't insert required field!";
                    break;
                }
            case 245:
                {
                    model.Msg = "can't Convert the ID, change will not save, Please change the ID or contact system administrator";
                    break;
                }

            default:
                {
                    if (ex.InnerException is not null && ex.InnerException.Message == "-1")
                    {
                        model.Msg = ex.Message;
                    }
                    else if (ex.Message.Contains("Failure sending mail"))
                    {
                        model.Msg = Languages.get_Translation("TransferSMTPWarningMessage");
                    }
                    else
                    {
                        model.Msg = ex.Message;
                    } // "something went wrong, contact the administrator"

                    break;
                }
        }
    }
}

class ErrorBaseModel :BaseModel
{
    public bool isSuccess { get; set; } = true;
    public string totalrowsForGrid { get; set; }
}