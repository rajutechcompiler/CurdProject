using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Smead.RecordsManagement;
using Smead.Security;
using TabFusionRMS.Resource;

namespace TabFusionRMS.WebCS.Controllers
{
    public class BarcodeTrackerController : BaseController
    {
        //public BarcodeTrackerController(IHttpContextAccessor httpContext) :base(httpContext)
        //{
           
        //}
        // 
        BarcodeTrackerModel Barcodemodel = new BarcodeTrackerModel();
        private enum LockTypes
        {
            Locked,
            HoldDetect,
            Unlocked
        }
        private TableItem _objItem;
        private TableItem _destItem;

        public IActionResult Index()
        {
            Barcodemodel.hdnPrefixes = LoadPrefixes();
            Barcodemodel.additionalField1Type = "";
            Barcodemodel.lblAdditional1 = "";
            Barcodemodel.lblAdditional2 = "";
            Barcodemodel.additionalField2 = "";

            string additionalField1 = Navigation.GetSystemSetting("TrackingAdditionalField1Desc", passport);
            if (!string.IsNullOrEmpty(additionalField1))
            {
                string additionalField1Type = Navigation.GetSystemSetting("TrackingAdditionalField1Type", passport);
                if (!string.IsNullOrEmpty(additionalField1Type))
                {
                    Barcodemodel.additionalField1Type = additionalField1Type;
                    Barcodemodel.lblAdditional1 = additionalField1 + ":";
                    // Barcodemodel.chekAdditionSystemseting = 1
                }
            }
            else
            {
                Barcodemodel.additionalField1Type = "";
            }
            // 'memo additional field
            string additionalField2 = Navigation.GetSystemSetting("TrackingAdditionalField2Desc", passport.Connection());
            if (!string.IsNullOrEmpty(additionalField2))
            {
                Barcodemodel.lblAdditional2 = additionalField2 + ":";
                Barcodemodel.additionalField2 = additionalField2;
                // Barcodemodel.chekAdditionSystemseting = 1
            }

            return View(Barcodemodel);
        }

        // additional dropdownList helper
        public IActionResult Dropdownlist()
        {
            var conn = passport.Connection();
            var ls = new List<SelectListItem>();
            var dtSuggest = Tracking.GetTrackingSelectData(conn);
            foreach (DataRow row in dtSuggest.Rows)
                ls.Add(new SelectListItem() { Text = Convert.ToString(row["Id"]), Value = Convert.ToString(row["Id"]) });
            return Json(ls);
        }
        // click on the first textbox
        public JsonResult ClickBarcodeTextDestination(string txtDestination, string txtObject, string hdnPrefixes)
        {
            Barcodemodel.serverErrorMsg = "";
            try
            {
                if (string.IsNullOrEmpty(txtDestination))
                {
                    if (!IsDestination(_destItem))
                        throw new Exception(string.Format(Languages.get_Translation("msgBarCodeTrackingIsrequired"), txtDestination));
                }
                else
                {
                    using (var conn = passport.Connection())
                    {
                        _objItem = Tracking.TranslateBarcode(txtObject, false, conn);
                        _destItem = Tracking.TranslateBarcode(txtDestination, true, conn);
                        if (!IsDestination(_destItem))
                            throw new Exception(string.Format(Languages.get_Translation("msgBarCodeTrackingIsNotValidDest"), txtDestination));

                        if (_destItem is not null)
                        {
                            Barcodemodel.getDestination = Navigation.GetItemName(_destItem.TableName, _destItem.ID, passport, conn);
                            Barcodemodel.CheckgetDestination = true;
                        }
                        else
                        {
                            Barcodemodel.getDestination = string.Format(Languages.get_Translation("msgBarCodeTrackingNotFound"), txtDestination);
                            Barcodemodel.CheckgetDestination = false;
                        }
                        SetDueBackDate(_destItem, txtDestination);
                    }
                }
            }
            catch (Exception ex)
            {
                Barcodemodel.serverErrorMsg = ex.Message;
            }
            return Json(Barcodemodel);
        }

        public IActionResult DetectDestinationChange(string txtDestination, string txtObject, string hdnPrefixes)
        {

            using (var conn = passport.Connection())
            {
                try
                {
                    _destItem = Tracking.TranslateBarcode(txtDestination, true, conn);
                    _objItem = Tracking.TranslateBarcode(txtObject, true, conn);
                    Barcodemodel.detectDestination = !DestinationIsHigher(_destItem, _objItem);
                }
                catch (Exception ex)
                {
                    Barcodemodel.serverErrorMsg = ex.Message;
                    Barcodemodel.detectDestination = false;
                }
            }
            return Json(Barcodemodel);
        }
        //// click on the second textbox
        public IActionResult ClickBarckcodeTextTolistBox(string txtDestination, string txtObject, string hdnPrefixes, string txtDueBackDate, string? additional1 = null, string? additional2 = null)
        {
            var dateStr = new DateTime();
            Barcodemodel.serverErrorMsg = "";
            try
            {
                if (string.IsNullOrEmpty(txtObject))
                {
                    throw new Exception(string.Format(Languages.get_Translation("msgObjectBarCodeTrackingIsrequired"), txtObject));
                }
                if (string.IsNullOrEmpty(additional1))
                    additional1 = " ";
                if (string.IsNullOrEmpty(additional2))
                    additional2 = " ";

                using (var conn = passport.Connection())
                {
                    _objItem = Tracking.TranslateBarcode(txtObject, false, conn);
                    _destItem = Tracking.TranslateBarcode(txtDestination, true, conn);
                }
                if (string.IsNullOrEmpty(hdnPrefixes))
                    LoadPrefixes();
                if (_objItem is not null)
                {
                    if (IsDestination(_destItem) && !DestinationIsHigher(_destItem, _objItem))
                        throw new Exception(string.Format(Languages.get_Translation("msgImportCtrlTrackingObjectNotFit"), _objItem.TableName, txtDestination));
                    var user = new User(passport, true);
                    StartTransfer(_destItem, _objItem, txtObject, txtDestination, txtDueBackDate, additional1, additional2);
                    Barcodemodel.returnDestination = Navigation.GetItemName(_destItem.TableName, _destItem.ID,  passport);
                    Barcodemodel.returnObjectItem = "  └─► " + Navigation.GetItemName(_objItem.TableName, _objItem.ID, passport);
                }
                else
                {
                    Barcodemodel.serverErrorMsg = string.Format(Languages.get_Translation("msgBarCodeTrackingNotFound"), txtObject);
                }
            }
            catch (Exception ex)
            {
                Barcodemodel.serverErrorMsg = ex.Message;
            }
            return Json(Barcodemodel);
        }

        private bool IsDestination(TableItem _destItem)
        {
            try
            {
                if (_destItem is not null)
                {
                    return _destItem.TrackingTable > 0;
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool DestinationIsHigher(TableItem _destItem, TableItem _objItem)
        {
            try
            {
                if (_destItem is not null & _objItem is not null)
                {
                    if (_objItem.TrackingTable != -1)
                    {
                        return _destItem.TrackingTable < _objItem.TrackingTable;
                    }
                    else if (_destItem.TrackingTable != -1)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void StartTransfer(TableItem destItem, TableItem objItem, string txtObject, string txtDestination, string txtDueBackDate, string? additional1 = null, string? additional2 = null)
        {
            if (string.IsNullOrEmpty(txtObject))
            {
                txtObject = "";
            }
            if (string.IsNullOrEmpty(txtDestination))
            {
                txtDestination = "";
            }
            var dateStr = new DateTime();
            if (TrackingServices.IsOutDestination(destItem.TableName, destItem.ID, httpContext))
            {
                if (txtDueBackDate is not null)
                {
                    if (txtDueBackDate.Trim().Length == 0)
                    {
                        Barcodemodel.serverErrorMsg = Languages.get_Translation("msgBarCodeTrackingDueBackDateReq");
                        return;
                    }

                    if (!Information.IsDate(txtDueBackDate))
                    {
                        Barcodemodel.serverErrorMsg = Languages.get_Translation("DueBackDateInvalid");
                        return;
                    }
                    dateStr = DateTime.Parse(txtDueBackDate.Trim(), CultureInfo.CurrentCulture);
                    if (DateTime.Parse(DateTime.Now.ToShortDateString()) > DateTime.Parse(dateStr.ToShortDateString()))
                    {
                        Barcodemodel.serverErrorMsg = Languages.get_Translation("DueBackDateLessThanCurrent");
                        return;
                    }
                }
            }
            try
            {
                var user = new User(passport, true);
                TrackingServices.PrepareDataForTransfer(objItem.TableName, objItem.ID, destItem.TableName, destItem.ID, dateStr, user.UserName,passport,httpContext, additional1, additional2);
            }
            catch (Exception ex)
            {
                Barcodemodel.serverErrorMsg = ex.Message;
            }
        }

        private string LoadPrefixes()
        {
            var sb = new StringBuilder();
          
            using (var conn = passport.Connection())
            {
                var dt = Tracking.GetTrackingContainerTypes(conn);
                foreach (DataRow row in dt.Rows)
                {
                    if (string.IsNullOrEmpty(row["BarCodePrefix"].ToString()))
                    {
                        sb.Append(" ,");
                    }
                    else
                    {
                        sb.Append(string.Format("{0},", row["BarCodePrefix"].ToString().ToUpper()));
                    }
                }
            }
            return sb.ToString();
        }


        private void SetDueBackDate(TableItem item, string text)
        {
            Barcodemodel.chkDueBackDate = TrackingServices.IsOutDestination(item.TableName, item.ID, httpContext);
            Barcodemodel.formatDueBackDate = Keys.GetCultureCookies(httpContext).DateTimeFormat.ShortDatePattern;
            if (Convert.ToBoolean(Barcodemodel.chkDueBackDate))
            {
                Barcodemodel.DueBackDateText = Keys.get_ConvertCultureDate(Convert.ToString(TrackingServices.GetDueBackDate(item.TableName, item.ID, httpContext)), httpContext);
            }
            else
            {
                Barcodemodel.DueBackDateText = Languages.get_Translation("txtBarCodeTrackingNone");
            }
        }
    }
}