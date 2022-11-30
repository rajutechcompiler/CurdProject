using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic
using Smead.RecordsManagement.Imaging;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.Resource;
using TabFusionRMS.WebCS.Controllers;

public partial class UploadController : BaseController
{
    private IRepository<OutputSetting> _iOutputSetting { get; set; }
    private IRepository<Table> _iTable { get; set; }
    private IRepository<Volume> _iVolume { get; set; }
    private IRepository<SystemAddress> _iSystemAddress { get; set; }



    public UploadController(IRepository<TabFusionRMS.Models.System> iSystem, IRepository<OutputSetting> iOutputSetting, IRepository<Volume> iVolume, IRepository<SystemAddress> iSystemAddress, IRepository<Table> iTable) : base()
    {
        _iOutputSetting = iOutputSetting;
        _iTable = iTable;
        _iVolume = iVolume;
        _iSystemAddress = iSystemAddress;
    }

    public IActionResult Index()
    {

        var lOutputSettings = _iOutputSetting.All();
        var lTableEntities = _iTable.All();

        ViewBag.OutputSettingsList = lOutputSettings.Where(x => Operators.ConditionalCompareObjectEqual(x.InActive, false, false)).CreateSelectList("Id", "Id", default);
        ViewBag.TablesList = lTableEntities.Where(x => Operators.ConditionalCompareObjectEqual(x.Attachments, true, false)).CreateSelectList("TableName", "UserName", default);

        return View();
    }

    public ActionResult AttachDocument(string pTableName, string pTableId, string pOutPutSetting)
    {
        if (ContextService.GetSessionValueByKey("fileName", httpContext) is null && string.IsNullOrEmpty(pTableName) && string.IsNullOrEmpty(pTableId) && string.IsNullOrEmpty(pOutPutSetting))
        {
            return Json(new
            {
                errortype = "e",
                message = Keys.ErrorMessageJS()
            });
        }
        do
        {
            try
            {

                var oOutputSettings = _iOutputSetting.All().Where(x => x.Id.Trim().ToLower().Equals(pOutPutSetting.Trim().ToLower())).FirstOrDefault();
                if (oOutputSettings is null)
                {
                    return Json(new { errortype = "w", message = Languages.get_Translation("msgUploadControllerInvalidDirPath") });
                }
                var oVolumns = _iVolume.All().Where(x => Operators.ConditionalCompareObjectEqual(x.Id, oOutputSettings.VolumesId, false)).FirstOrDefault();
                if (oVolumns is null)
                {
                    return Json(new { errortype = "w", message = Languages.get_Translation("msgUploadControllerInvalidDirPath") });
                }
                var oSystemAddress = _iSystemAddress.All().Where(x => Operators.ConditionalCompareObjectEqual(x.Id, oVolumns.SystemAddressesId, false)).FirstOrDefault();
                if (oSystemAddress is null)
                {
                    return Json(new { errortype = "w", message = Languages.get_Translation("msgUploadControllerInvalidDirPath") });
                }

                if (!System.IO.Directory.Exists(oSystemAddress.PhysicalDriveLetter))
                {
                    return Json(new { errortype = "w", message = Languages.get_Translation("msgUploadControllerInvalidDirPath") });
                }

                var pFileName = ContextService.GetSessionValueByKey("fileName",httpContext).ToString();
                //var BaseWebPageMain = new BaseWebPage();

                var filepath = Path.GetTempPath() + pFileName;  // Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\Temp\" + pFileName

                if (!System.IO.File.Exists(filepath))
                {
                    filepath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Temp\" + pFileName;
                }


                if (!System.IO.File.Exists(filepath))
                {
                    return Json(new
                    {
                        errortype = "e",
                        message = Languages.get_Translation("msgUploadControllerFilenExiOnPath")
                    });
                }


                var ticket = passport.get_CreateTicket(string.Format(@"{0}\{1}", ContextService.GetSessionValueByKey("Server",httpContext).ToString(), ContextService.GetSessionValueByKey("DatabaseName",httpContext).ToString()), pTableName, pTableId).ToString();

                Attachment attach = Attachments.AddAttachment(Keys.GetClientIpAddress(httpContext), ticket, passport.UserId, string.Format(@"{0}\{1}", ContextService.GetSessionValueByKey("Server",httpContext).ToString(), ContextService.GetSessionValueByKey("DatabaseName",httpContext).ToString()), pTableName, pTableId, 0, pOutPutSetting, filepath, filepath, Path.GetExtension(pFileName), false, string.Empty);
                ContextService.RemoveSession("fileName",httpContext);
                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                }
                Keys.ErrorType = "s";
                Keys.ErrorMessage = Languages.get_Translation("msgUploadControllerDocAttachSucc"); // DirectCast(attach, Smead.RecordsManagement.Imaging.ErrorAttachment).Message 
                break;
            }
            catch (Exception ex)
            {
                Keys.ErrorType = "e";
                Keys.ErrorMessage = ex.Message.ToString();
            }
        }
        while (false);

        return Json(new
        {
            errortype = Keys.ErrorType,
            message = Keys.ErrorMessage
        });
    }

}