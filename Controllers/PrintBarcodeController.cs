using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic
using Newtonsoft.Json;
using PrinterCOM;
using Smead.RecordsManagement;
//using Smead.RecordsManagement.Navigation;
using Smead.Security;
using TabFusionRMS.Resource;
using TabFusionRMS.WebCS;
using TabFusionRMS.WebCS.FusionElevenModels;

namespace TabFusionRMS.WebCS.Controllers
{
    public partial class PrintBarcodeController : BaseController
    {
        
        private Printer_obj moPrinter = new Printer_obj();
        //private string pathValue { get; set; } 
        
        [HttpPost]
        public IActionResult InitiateBarcodePopup([FromServices] IConfiguration configuration,[FromServices] IWebHostEnvironment webHostEnvironment,[FromBody] UserInterfaceJsonReqModel req)

        {
            if (req.paramss.ids is null | req.paramss.ids.Count > 100)
                return default;
            var model = new PrintBarcode();
            try
            {
                if (req.paramss.ids.Count == 0)
                {
                    model.Msg = Languages.get_Translation("msgJsDataSelectOneRow");
                    return Json(model);
                }
                var p = new Parameters(req.paramss.ViewId, passport);


                string inClause = string.Empty;

                if (!String.Join(',', req.paramss.ids.ToArray()).Equals(""))
                {
                    inClause = " IN (" + String.Join(',', req.paramss.ids.ToArray()) + ")";
                }
                else
                {
                    model.Msg = Languages.get_Translation("msgSQLNoDataReturns");
                    return Json(model);
                }

                int labelId = 0;
                string labelName = string.Empty;
                var dtForms = new DataTable();
                var dtJobs = new DataTable();
                var dtFormat = new DataTable("LabelDataFields");
                var dtData = new DataTable("LabelData");

                using (SqlConnection conn = passport.Connection())
                {
                    using (var cmd = new SqlCommand("SELECT * FROM [OneStripJobs] WHERE [TableName] = @tableName AND [InPrint] = 0", conn))
                    {
                        cmd.Parameters.AddWithValue("@TableName", p.TableName);
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dtJobs);
                            foreach (DataRow item in dtJobs.Rows)
                                model.labelDesign.Add(new dropdown() { text = item["Name"].Text(), value = item["Id"].Text() });
                        }
                    }

                    using (var cmd = new SqlCommand("SELECT * FROM [OneStripForms] WHERE [Inprint] = @Inprint", conn))
                    {
                        cmd.Parameters.AddWithValue("@Inprint", 0);
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dtForms);
                            foreach (DataRow item in dtForms.Rows)
                                model.labelForm.Add(new dropdown() { text = item["Name"].Text(), value = item["Id"].Text() });
                        }
                    }

                    labelId = dtJobs.Rows[0]["Id"].IntValue();
                    model.formSelectionid = dtJobs.Rows[0]["OneStripFormsId"].Text();


                    labelName = dtJobs.Rows[0]["Name"].Text();
                    dtData = LoadPrintData(conn, inClause, dtJobs, labelId, Orderby(req.paramss), model);
                    dtFormat = LoadPrintDataFields(conn, labelId, model);
                }

                ContextService.SetSessionValue("dtForms", JsonConvert.SerializeObject(dtForms), httpContext);
                ContextService.SetSessionValue("dtJobs", JsonConvert.SerializeObject(dtJobs), httpContext);

                model.oneStripJob = dtJobs.Rows[0];
                model.oneStripForms = dtForms.Select("Id = " + model.formSelectionid).ToList()[0];
                model.oneStripJobFields = dtFormat.Select().ToList();
                model.gridData = dtData.Select().ToList();

                if (moPrinter.PrinterSetup(0, ContextService.AppSetting("PrinterDriver", configuration), gePrinterSetupTypes.psPrinterRetrieve, false))
                {
                    PrintData(webHostEnvironment, model, configuration);
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
                model.Msg = "failed";
            }

            // DeleteLabelsPCFiles(HttpContext.Server.MapPath("LabelData/"))
            return PartialView("_getbarcode", model);
        }

        [HttpPost]
        public JsonResult GenerateBarcodeOnchange([FromServices] IConfiguration configuration, [FromServices] IWebHostEnvironment webHostEnvironment,[FromBody] UserInterfaceJsonReqModel req)
        {
            if (req.paramss.ids is null)
                return default;

            var model = new PrintBarcode();
            var m = new PrintBarcodeCommonModel();
            model.labelOutline = req.paramss.PrintBarcode.labelOutline;
            model.strtPrinting = req.paramss.PrintBarcode.strtPrinting;
            
            try
            {
                if (req.paramss.ids.Count == 0)
                {
                    model.Msg = Languages.get_Translation("msgJsDataSelectOneRow");
                    return Json(model);
                }

                string inClause = string.Empty;

                if (!String.Join(",", req.paramss.ids.ToArray()).Equals(""))
                {
                    inClause = " IN (" + String.Join(",", req.paramss.ids.ToArray()) + ")";
                }
                else
                {
                    model.Msg = Languages.get_Translation("msgSQLNoDataReturns");
                    return Json(model);
                }

                DataTable dtForms = ContextService.GetObjectFromJson<DataTable>("dtForms", httpContext);
                DataTable dtJobs = ContextService.GetObjectFromJson<DataTable>("dtJobs", httpContext);

                int labelId = req.paramss.PrintBarcode.labelDesignSelectedValue;

                if (req.paramss.PrintBarcode.isLabelDropdown)
                {
                    model.formSelectionid = dtJobs.Rows[req.paramss.PrintBarcode.labelIndex]["OneStripFormsId"].ToString();
                }
                else
                {
                    model.formSelectionid = req.paramss.PrintBarcode.labelFormSelectedValue.ToString();
                }

                var dtFormat = new DataTable("LabelDataFields");
                var dtData = new DataTable("LabelData");

                using (SqlConnection conn = passport.Connection())
                {
                    dtData = LoadPrintData(conn, inClause, dtJobs, labelId, Orderby(req.paramss), model);
                    dtFormat = LoadPrintDataFields(conn, labelId, model);
                }

                model.oneStripJob = dtJobs.Rows[req.paramss.PrintBarcode.labelIndex];
                model.oneStripForms = dtForms.Select("Id = " + model.formSelectionid).ToList()[0];
                model.oneStripJobFields = dtFormat.Select().ToList();
                model.gridData = dtData.Select().ToList();

                if (moPrinter.PrinterSetup(0, ContextService.AppSetting("PrinterDriver", configuration), gePrinterSetupTypes.psPrinterRetrieve, false))
                    PrintData(webHostEnvironment,model, configuration);
            }
            catch (Exception ex)
            {
                model.Msg = "failed";
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            m.labelFileName = model.labelFileName;
            m.labelFileDynamicpath = model.labelFileDynamicpath;
            m.formSelectionid = Convert.ToInt32(model.formSelectionid);
            return Json(m);
        }

        [HttpGet]
        public async void DownloadbarcodefileAsync([FromServices] IWebHostEnvironment webHostEnvironment, string labelUniqueName, int labelId, string ids)
        {

            
            LabelPrintUpdate(labelId, ids);
            var serverPath = webHostEnvironment.ContentRootPath + "wwwroot\\" + "LabelData\\" + passport.UserId.ToString() + "\\" + labelUniqueName;
            string Header = "attachment;filename=" + labelUniqueName;
            FileStream sourceFile = new FileStream(serverPath, FileMode.Open);
            float fileSize = 0;
            fileSize = sourceFile.Length;
            byte[] getContent = new byte[Convert.ToInt32(Math.Truncate(fileSize))];
            sourceFile.Read(getContent, 0, Convert.ToInt32(sourceFile.Length));
            sourceFile.Close();
            HttpContext.Response.Clear();
            HttpContext.Response.Headers.Clear();
            Response.ContentType = "application/pdf";
            Response.Headers.Add("Content-Length", getContent.Length.ToString());
            Response.Headers.Add("Content-Disposition", "attachment; filename=" + labelUniqueName);
            Response.Body.WriteAsync(getContent);
            Response.Body.Flush();

            //HttpContext.Response.ContentType = "application/pdf";//"application/octet-stream";
            //string Header = "attachment;filename=" + labelUniqueName;
            //HttpContext.Response.Headers.Append("content-disposition", Header);
            ////var serverPath = HttpContext.Server.MapPath("~/LabelData/" + _passport.UserId.ToString() + "/" + labelUniqueName);
            //var serverPath = webHostEnvironment.ContentRootPath + "wwwroot\\" + "LabelData\\" + passport.UserId.ToString() + "\\" + labelUniqueName;
            //var Dfile = new FileInfo(serverPath);
            //await HttpContext.Response.WriteAsync();
            //await HttpContext.Response.Body.FlushAsync();
            //Ok();

            //var stream = new FileStream(serverPath, FileMode.Open);
            //return new FileStreamResult(stream, "application/pdf");

        }

        [HttpPost]
        public JsonResult SetDefaultBarcodeForm([FromBody] UserInterfaceJsonReqModel req)
        {
            var model = new PrintBarcodeCommonModel();

            try
            {
                int frmId = req.paramss.PrintBarcode.labelFormSelectedValue;
                var labelId = req.paramss.PrintBarcode.labelDesignSelectedValue;
                DataTable dtForms = ContextService.GetObjectFromJson<DataTable>("dtForms", httpContext);
                var frmObject = dtForms.Select("Id = " + frmId)[0];
                var dtJobs = new DataTable();

                var labelHeight = frmObject["LabelHeight"];
                var labelWidth = frmObject["LabelWidth"];

                using (SqlConnection conn = passport.Connection())
                {
                    using (var cmd = new SqlCommand("Update [OneStripJobs] SET [OneStripFormsId] = @frmId, [LabelHeight] = @lblHeight, [LabelWidth] = @lblWidth WHERE [Id] = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@frmId", frmId);
                        cmd.Parameters.AddWithValue("@lblHeight", labelHeight);
                        cmd.Parameters.AddWithValue("@lblWidth", labelWidth);
                        cmd.Parameters.AddWithValue("@id", labelId);
                        cmd.ExecuteScalar();
                    }

                    using (var cmd = new SqlCommand("SELECT * FROM [OneStripJobs] WHERE [TableName] = @tableName AND [InPrint] = 0", conn))
                    {
                        cmd.Parameters.AddWithValue("@TableName", req.paramss.TableName);
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dtJobs);
                        }
                    }
                }

                ContextService.SetSessionValue("dtJobs", JsonConvert.SerializeObject(dtJobs), httpContext);
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }

       
        private DataTable LoadPrintData(SqlConnection conn, string inClause, DataTable dtJobs, int labelId, string orderby, PrintBarcode model)
        {
            var dtData = new DataTable("LabelData");

            try
            {
                DataRow[] row = dtJobs.Select(string.Format("Id = {0}", labelId));
                string query = Navigation.NormalizeString(row[0]["SQLString"].ToString());
                if (query.EndsWith(";"))
                    query = query.Substring(0, query.Length - 1).Trim();

                query = query.Replace("= %ID%", inClause);
                query = query.Replace("=%ID%", inClause);
                query = query.Replace("='%ID%'", inClause);
                query = query.Replace("= '%ID%'", inClause);

                using (var cmd = new SqlCommand(string.Format("{0} {1}", query, orderby), conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtData);
                    }
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return dtData;
        }

        private DataTable LoadPrintDataFields(SqlConnection conn, int labelId, PrintBarcode model)
        {
            var dtData = new DataTable("LabelDataFields");

            try
            {
                using (var cmd = new SqlCommand("SELECT * FROM [OneStripJobFields] WHERE [OneStripJobsID] = @JobID", conn))
                {
                    cmd.Parameters.AddWithValue("@JobID", labelId);
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtData);
                    }
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return dtData;
        }

        private void LabelPrintUpdate(int labelId, string inClause)
        {
            using (SqlConnection conn = passport.Connection())
            {
                string sqlUpdate;

                using (var cmd = new SqlCommand("SELECT [SQLUpdateString] FROM [OneStripJobs] WHERE [Id] = @labelId", conn))
                {
                    cmd.Parameters.AddWithValue("@labelId", labelId);

                    try
                    {
                        sqlUpdate = cmd.ExecuteScalar().ToString();
                    }
                    catch (Exception)
                    {
                        sqlUpdate = string.Empty;
                    }
                }

                if (string.IsNullOrEmpty(sqlUpdate))
                    return;

                sqlUpdate = Navigation.NormalizeString(sqlUpdate);
                inClause = string.Format("IN ({0})", inClause);

                using (var cmd = new SqlCommand(Strings.Replace(sqlUpdate, "= %ID%", inClause), conn))
                {
                    cmd.CommandText = Strings.Replace(cmd.CommandText, "=%ID%", inClause);
                    cmd.CommandText = Strings.Replace(cmd.CommandText, "='%ID%'", inClause);
                    cmd.CommandText = Strings.Replace(cmd.CommandText, "= '%ID%'", inClause);
                    cmd.ExecuteScalar();
                }
            }
        }

        private string Orderby(UserInterfaceJsonModel @params)
        {
            string str = string.Empty;
            string ascdesc = string.Empty;
            int counter = 0;
            if (@params.PrintBarcode == null)
                return "";
            foreach (SortableFileds item in @params.PrintBarcode.sortableFields)
            {
                counter = counter + 1;
                if (item.SortOrderDesc == false || item.SortOrderDesc == null)
                {
                    ascdesc = "asc";
                }
                else
                {
                    ascdesc = "desc";
                }
                if (counter == @params.PrintBarcode.sortableFields.Count)
                {
                    str += item.FieldName + " " + ascdesc;
                }
                else
                {
                    str += item.FieldName + " " + ascdesc + ",";
                }
            }

            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            else
            {
                return "order by " + str;
            }
        }
        [HttpGet]
        public void DeleteLabelsFiles([FromServices] IWebHostEnvironment webHostEnvironment)
        {
            var ServerPath = webHostEnvironment.ContentRootPath + "/wwwroot" + "/LabelData/" + passport.UserId.ToString();
            if (Directory.Exists(ServerPath))
            {
                var di = new DirectoryInfo(ServerPath);
                foreach (FileInfo file in di.GetFiles())
                    file.Delete();
            }
        }
        private void PrintData(IWebHostEnvironment webHostEnvironment, PrintBarcode model, IConfiguration configuration)
        {
            bool bPrintedOnPage;
            bool bPrintingLabel;
            float fOffsetShiftX;
            float fOffsetShiftY;
            float fRatio;
            float fX1;
            float fX2;
            float fY1;
            float fY2;
            var fXPtrOffset = default(float);
            var fYPtrOffset = default(float);
            long lCount;
            long lCounter;
            long lCurrLabelsAcross;
            long lCurrLabelsDown;
            long lLabelsToPrint;
            long lLabelsToPrintTemp;
            long lLabelStart;
            long lLabelsSkipped;
            long lPagesToPrint;
            string sTemp;
            string sFieldName;
            string sFieldValue;
            string extensionValue;
            try
            {
                string sGUID = Guid.NewGuid().ToString();
                extensionValue = ContextService.AppSetting("PrinterDriverExtension", configuration);
                model.labelFileName = sGUID + "." + extensionValue.Trim().ToLower();
                model.labelFileDynamicpath = "./LabelData/" + passport.UserId.ToString() + "/" + model.labelFileName;

                var ServerPath = webHostEnvironment.ContentRootPath + "wwwroot" +  "\\LabelData\\" + passport.UserId.ToString() + "\\";
                if (!Directory.Exists(ServerPath))
                {
                    Directory.CreateDirectory(ServerPath);
                }
                
                moPrinter.StartDoc(ServerPath + model.labelFileName);

                bPrintedOnPage = false;
                lLabelStart = model.strtPrinting - 1;

                lCounter = 1L;
                lLabelsSkipped = 0L;
                lCurrLabelsDown = 0L;
                lCurrLabelsAcross = 0L;
                fX1 = 0f;
                fY1 = 0f;
                fX2 = 0f;
                fY2 = 0f;

                model.totalSkipped = Strings.Format(lLabelsSkipped, "#,#") + " ";
                var labelwidth = model.oneStripForms["LabelWidth"].DoubleValue();

                fOffsetShiftX = (float)((model.oneStripForms["LabelWidth"].DoubleValue() - model.oneStripJob["LabelWidth"].DoubleValue()) / 2.0d);
                fOffsetShiftY = (float)((model.oneStripForms["LabelHeight"].DoubleValue() - model.oneStripJob["LabelHeight"].DoubleValue()) / 2.0d);

                if (Conversions.ToBoolean(model.gridData.Count))
                {
                    lLabelsToPrint = model.gridData.Count;
                }
                else
                {
                    lLabelsToPrint = 1L;
                }

                model.totalLabels = Strings.Format(lLabelsToPrint, "#,0") + " ";

                if ((model.oneStripForms["LabelsAcross"].DoubleValue() * model.oneStripForms["LabelsDown"].DoubleValue())>0)
                {
                    lLabelsToPrintTemp = lLabelsToPrint + (model.strtPrinting - 1);
                    lPagesToPrint = (Int32)(lLabelsToPrintTemp / (model.oneStripForms["LabelsAcross"].DoubleValue() * model.oneStripForms["LabelsDown"].DoubleValue()));
                    if (lLabelsToPrintTemp % (model.oneStripForms["LabelsAcross"].DoubleValue() * model.oneStripForms["LabelsDown"].DoubleValue())>0)
                    {
                        lPagesToPrint = lPagesToPrint + 1L;
                    }
                }
                else
                {
                    lPagesToPrint = 0L;
                }
                if (lPagesToPrint == 0L)
                {
                    lPagesToPrint = 1L;
                }
                model.totalPages = Strings.Format(lPagesToPrint, "#,0") + " ";
                lCurrLabelsAcross = 0L;
                lCurrLabelsDown = 0L;
                fRatio = 1f;
                lCounter = 1L;
                bPrintingLabel = true;

                var loopTo = lLabelStart;
                for (lCount = 1L; lCount <= loopTo; lCount++)
                {
                    if (bPrintingLabel)
                    {
                        lCurrLabelsAcross = lCurrLabelsAcross + 1L;
                    }

                    if (lCurrLabelsAcross >= model.oneStripForms["LabelsAcross"].DoubleValue())
                    {
                        lCurrLabelsDown = lCurrLabelsDown + 1L;
                        lCurrLabelsAcross = 0L;
                    }
                }

                lLabelStart = 0L;

                foreach (var gridDataRow in model.gridData)
                {
                    if (!bPrintedOnPage)
                    {
                        moPrinter.StartPage();
                        bPrintedOnPage = true;
                    }
                    SetLabelPosition(ref fX1, ref fY1, ref fX2, ref fY2, false, lCurrLabelsAcross, lCurrLabelsDown, model.oneStripForms, fXPtrOffset, fYPtrOffset);
                    if (model.labelOutline == true)
                    {
                        DrawBox(fX1, fY1, fX2, fY2);
                    }

                    SetLabelPosition(ref fX1, ref fY1, ref fX2, ref fY2, true, lCurrLabelsAcross, lCurrLabelsDown, model.oneStripForms);
                    moPrinter.SelectRegion(true, fX1, fY1, fX2, fY2, 1);

                    foreach (var moOneStripJobFields in model.oneStripJobFields)
                    {
                        switch (moOneStripJobFields["Type"].Text())
                        {
                            case "S": // String
                                {
                                    sTemp = (string)moOneStripJobFields["Format"];
                                    moPrinter.PrintRotated(true, sTemp, moOneStripJobFields["XPos"].IntValue(), moOneStripJobFields["YPos"].IntValue(), 
                                        fRatio, moOneStripJobFields["Alignment"].IntValue(), moOneStripJobFields["FontOrientation"].IntValue(), 
                                        (float)moOneStripJobFields["BCWidth"].DoubleValue(), (float)moOneStripJobFields["BCHeight"].DoubleValue(),
                                              moOneStripJobFields["ForeColor"].IntValue(), moOneStripJobFields["BackColor"].IntValue(),
                                              moOneStripJobFields["FontName"].Text(), moOneStripJobFields["FontSize"].IntValue(),
                                              Convert.ToBoolean( moOneStripJobFields["FontBold"]), Convert.ToBoolean(moOneStripJobFields["FontItalic"]),
                                               Convert.ToBoolean(moOneStripJobFields["FontUnderline"]), Convert.ToBoolean(moOneStripJobFields["FontStrikeThru"]),
                                               Convert.ToBoolean(moOneStripJobFields["FontTransparent"]),moOneStripJobFields["StartChar"].IntValue(),
                                               moOneStripJobFields["MaxLen"].IntValue(),moOneStripJobFields["Format"].Text(), 
                                               (float)model.oneStripForms["LabelOffsetX"].DoubleValue(),(float)model.oneStripForms["LabelOffsetY"].DoubleValue(),
                                               0, 0, fOffsetShiftX, fOffsetShiftY,lCurrLabelsAcross, lCurrLabelsDown,
                                               (float)model.oneStripForms["LabelWidthEdgeToEdge"].DoubleValue(),(float)model.oneStripForms["LabelHeightEdgeToEdge"].DoubleValue());
                                    break;
                                }

                            default:
                                {
                                    sFieldName = (string)moOneStripJobFields["FieldName"];
                                    sFieldValue = "";

                                    sFieldName = RemoveTableNameFromField((string)moOneStripJobFields["FieldName"]);

                                    if (gridDataRow.Table.Columns.Contains(sFieldName))
                                    {
                                        sFieldValue = gridDataRow[sFieldName].Text();
                                    }
                                    else
                                    {
                                        sFieldValue = sFieldName;
                                    }

                                    sTemp = sFieldValue;

                                    switch (moOneStripJobFields["Type"])
                                    {
                                        case "B":   // Barcode
                                            {
                                                moPrinter.PrintBarCode(sTemp, moOneStripJobFields["Format"].ToString(), moOneStripJobFields["BackColor"].IntValue(), 
                                                    moOneStripJobFields["ForeColor"].IntValue(), moOneStripJobFields["BCStyle"].IntValue(), 
                                                    moOneStripJobFields["BCBarWidth"].IntValue(), moOneStripJobFields["BCWidth"].IntValue(),
                                                    (int)moOneStripJobFields["BCHeight"].DoubleValue(), moOneStripJobFields["BCDirection"].IntValue(), 
                                                    moOneStripJobFields["Alignment"].IntValue(), moOneStripJobFields["BCUPCNotches"].IntValue(), 0, 0, fOffsetShiftX, fOffsetShiftY,
                                                    (int)lCurrLabelsAcross, (int)lCurrLabelsDown, fRatio, (float)model.oneStripForms["LabelOffsetX"].DoubleValue(), 
                                                    (float)model.oneStripForms["LabelOffsetY"].DoubleValue(), (float)moOneStripJobFields["XPos"].DoubleValue(), 
                                                    (float)moOneStripJobFields["YPos"].DoubleValue(),(float)model.oneStripForms["LabelWidthEdgeToEdge"].DoubleValue(), 
                                                    (float)model.oneStripForms["LabelHeightEdgeToEdge"].DoubleValue());
                                                break;
                                            }
                                        case "Q":   // QRCode
                                            {
                                                moPrinter.PrintQRCode(sTemp, moOneStripJobFields["BackColor"].IntValue(), moOneStripJobFields["ForeColor"].IntValue(),
                                                    moOneStripJobFields["Alignment"].IntValue(), moOneStripJobFields["Format"].ToString(),
                                                    moOneStripJobFields["BCWidth"].IntValue(), moOneStripJobFields["BCDirection"].IntValue(),
                                                    moOneStripJobFields["BCUPCNotches"].IntValue(), moOneStripJobFields["BCStyle"].IntValue(),
                                                    model.oneStripForms["LabelOffsetX"].IntValue(), model.oneStripForms["LabelOffsetY"].IntValue(),
                                                    (int)fOffsetShiftX, (int)fOffsetShiftY, moOneStripJobFields["XPos"].IntValue(), moOneStripJobFields["YPos"].IntValue(), 0,
                                                    (int)lCurrLabelsAcross, (int)lCurrLabelsDown, model.oneStripForms["LabelWidthEdgeToEdge"].IntValue(),
                                                    model.oneStripForms["LabelHeightEdgeToEdge"].IntValue());
                                                break;
                                            }
                                        case "T":   // Text
                                            {
                                                moPrinter.PrintRotated(false, sTemp, moOneStripJobFields["XPos"].IntValue(),
                                                    moOneStripJobFields["YPos"].IntValue(), fRatio, moOneStripJobFields["Alignment"].IntValue(),
                                                    moOneStripJobFields["FontOrientation"].IntValue(), (float)moOneStripJobFields["BCWidth"].DoubleValue(),
                                                    (float)moOneStripJobFields["BCHeight"].DoubleValue(), moOneStripJobFields["ForeColor"].IntValue(),
                                                    moOneStripJobFields["BackColor"].IntValue(), moOneStripJobFields["FontName"].Text(),
                                                    moOneStripJobFields["FontSize"].IntValue(), Convert.ToBoolean(moOneStripJobFields["FontBold"]),
                                                    Convert.ToBoolean(moOneStripJobFields["FontItalic"]), Convert.ToBoolean(moOneStripJobFields["FontUnderline"]),
                                                    Convert.ToBoolean(moOneStripJobFields["FontStrikeThru"]),Convert.ToBoolean(moOneStripJobFields["FontTransparent"]),
                                                    moOneStripJobFields["StartChar"].IntValue(), moOneStripJobFields["MaxLen"].IntValue(), moOneStripJobFields["Format"].ToString(),
                                                    (float)model.oneStripForms["LabelOffsetX"].DoubleValue(), (float)model.oneStripForms["LabelOffsetY"].DoubleValue(),
                                                    0, 0, fOffsetShiftX, fOffsetShiftY, lCurrLabelsAcross, lCurrLabelsDown, 
                                                    (float)model.oneStripForms["LabelWidthEdgeToEdge"].DoubleValue(), (float)model.oneStripForms["LabelHeightEdgeToEdge"].DoubleValue());
                                                break;
                                            }

                                        default:
                                            {
                                                break;
                                            }
                                    }

                                    break;
                                }
                        }
                    }
                    moPrinter.DeleteRegion();

                    lLabelsToPrint = lLabelsToPrint - 1L;

                    lLabelsToPrintTemp = lLabelsToPrint + (model.strtPrinting - 1);
                    var vall = Conversion.Int(((double)lLabelsToPrintTemp / (model.oneStripForms["LabelsAcross"].IntValue() * model.oneStripForms["LabelsDown"].IntValue())));
                    lPagesToPrint = (long)vall;
                    
                    if (Convert.ToBoolean((lLabelsToPrintTemp % (model.oneStripForms["LabelsAcross"].IntValue() * model.oneStripForms["LabelsDown"].IntValue()))))
                    {
                        lPagesToPrint = lPagesToPrint + 1L;
                    }

                    if (!bPrintingLabel)
                    {
                        lLabelsSkipped = lLabelsSkipped + 1L;
                        model.totalSkipped = Strings.Format(lLabelsSkipped, "#,#") + " ";
                    }

                    if (bPrintingLabel)
                    {
                        lCurrLabelsAcross = lCurrLabelsAcross + 1L;
                    }

                    if (lCurrLabelsAcross >= model.oneStripForms["LabelsAcross"].IntValue())
                    {
                        lCurrLabelsDown = lCurrLabelsDown + 1L;
                        lCurrLabelsAcross = 0L;
                    }

                    if (lCurrLabelsDown >= model.oneStripForms["LabelsDown"].IntValue())
                    {
                        lCurrLabelsDown = 0L;
                        if (bPrintedOnPage)
                        {
                            moPrinter.EndPage();
                            bPrintedOnPage = false;
                        }
                    }
                }
                moPrinter.EndDoc();
                moPrinter = null;
            }

            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        private void SetLabelPosition(ref float fX1, ref float fY1, ref float fX2, ref float fY2, bool bDisplay, long lCurrLabelsAcross, long lCurrLabelsDown, DataRow moOneStripForms, float fXPtrOffset = 0f, float fYPtrOffset = 0f, float fOffsetShiftX = 0f, float fOffsetShiftY = 0f, bool bSecondLabel = false)
        {
            float fLabelOffsetX;
            float fLabelOffsetY;
            float fLabelWidth;
            float fLabelHeight;
            float fLabelWidthEdgeToEdge;
            float fLabelHeightEdgeToEdge;
            var fTQOffsetX = default(float);
            var fTQOffsetY = default(float);
            var fTQHeightEdgeToEdge = default(float);

            fLabelOffsetX = (float)(moOneStripForms["LabelOffsetX"].DoubleValue()) + fTQOffsetX;
            fLabelOffsetY = (float)moOneStripForms["LabelOffsetY"].DoubleValue() + fTQOffsetY;
            fLabelHeightEdgeToEdge = (float)moOneStripForms["LabelHeightEdgeToEdge"].DoubleValue() + fTQHeightEdgeToEdge;
            fLabelWidth = (float)moOneStripForms["LabelWidth"].DoubleValue();
            fLabelHeight = (float)moOneStripForms["LabelHeight"].DoubleValue();
            fLabelWidthEdgeToEdge = (float)moOneStripForms["LabelWidthEdgeToEdge"].DoubleValue();

            fX1 = fLabelOffsetX + fXPtrOffset + lCurrLabelsAcross * fLabelWidthEdgeToEdge + fOffsetShiftX;
            fY1 = fLabelOffsetY + fYPtrOffset + lCurrLabelsDown * fLabelHeightEdgeToEdge + fOffsetShiftY;
            fX2 = fLabelOffsetX + fXPtrOffset + lCurrLabelsAcross * fLabelWidthEdgeToEdge + fLabelWidth + fOffsetShiftX;
            fY2 = fLabelOffsetY + fYPtrOffset + lCurrLabelsDown * fLabelHeightEdgeToEdge + fLabelHeight + fOffsetShiftY;
            return;
        }
        private void DrawBox(float fX1, float fY1, float fX2, float fY2)
        {
            long lColor;
            lColor = Information.RGB(0, 0, 0);
            moPrinter.PrintLine((int)fX1, (int)fX2, (int)-fY1, true, 1, 1, (int)lColor);
            moPrinter.PrintLine((int)fX1, (int)fX2, (int)-fY2, true, 1, 1, (int)lColor);
            moPrinter.PrintLine((int)fX1, (int)-fY2, (int)-fY1, false, 1, 1, (int)lColor);
            moPrinter.PrintLine((int)fX2, (int)-fY2, (int)-fY1, false, 1, 1, (int)lColor);
        }
        private string RemoveTableNameFromField(string sFieldName)
        {
            string RemoveTableNameFromFieldRet = default;
            int i;

            RemoveTableNameFromFieldRet = sFieldName;
            i = Strings.InStr(sFieldName, ".");

            if (i > 1)
            {
                RemoveTableNameFromFieldRet = Strings.Mid(sFieldName, i + 1);
            }
            RemoveTableNameFromFieldRet = Strings.Trim(RemoveTableNameFromFieldRet);
            return RemoveTableNameFromFieldRet;
        }

    }


    public partial class PrintBarcode : BaseModel
    {
        public PrintBarcode()
        {
            labelDesign = new List<dropdown>();
            labelForm = new List<dropdown>();
            oneStripJobFields = new List<DataRow>();
            gridData = new List<DataRow>();
        }
        public List<dropdown> labelDesign { get; set; }
        public List<dropdown> labelForm { get; set; }
        public string totalLabels { get; set; }
        public string totalPages { get; set; }
        public string totalSkipped { get; set; }
        public DataRow oneStripJob { get; set; }
        public DataRow oneStripForms { get; set; }
        public List<DataRow> oneStripJobFields { get; set; }
        public List<DataRow> gridData { get; set; }
        public int strtPrinting { get; set; }
        public string labelFileName { get; set; }
        public string labelFileDynamicpath { get; set; }
        public bool labelOutline { get; set; }
        public string formSelectionid { get; set; }
    }
    public partial class dropdown
    {
        public string value { get; set; }
        public string text { get; set; }
    }
    public partial class PrintBarcodeCommonModel : BaseModel
    {
        public string labelFileDynamicpath { get; set; }
        public string labelFileName { get; set; }
        public string msg { get; set; }
        public int formSelectionid { get; set; }
    }
}