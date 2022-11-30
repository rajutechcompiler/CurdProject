using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic
using Microsoft.VisualBasic.FileIO; // Install-Package Microsoft.VisualBasic
using Smead.RecordsManagement;
using Smead.Security;
using TabFusionRMS.Models;
using System.Data;
using TabFusionRMS.WebCS.FusionElevenModels;
using TabFusionRMS.Resource;
using TabFusionRMS.WebCS;

namespace TabFusionRMS.WebCS.Controllers
{
    public partial class ImportFavoriteController : BaseController
    {
        //public ImportFavoriteController(IHttpContextAccessor httpContext) : base(httpContext)
        //{ }
        // GET: ImportFavorite
        [HttpGet]
        public ActionResult GetImportFavoritpopup()
        {
            var model = new ImportFavoriteModel();
            try
            {
                model.DropDownlist = BindDropdownList();
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }
            return PartialView("_importfavorites", model);
        }
        [HttpPost]
        public async Task<JsonResult> UploadingFileAsync([FromServices] IWebHostEnvironment webHostEnvironment,List<string> param)
        {
            var model = new ImportFavoriteModel();
            model.isFilesupported = true;
            try
            {
                var File = httpContext.Request.Form.Files[0];
                var ext = File.FileName.Split(".")[1];
                // check if file supported server side
                if (!(ext == "txt") && !(ext == "xls") && !(ext == "xlsx") && !(ext == "csv"))
                {
                    model.isFilesupported = false;
                    return Json(model);
                }
                string directorypath = webHostEnvironment.ContentRootPath + "/wwwroot" + string.Format("/ImportFiles/ImportFavorite/{0}", passport.UserId);
                // create directory per user if not exist
                if (!System.IO.Directory.Exists(directorypath))
                {
                    System.IO.Directory.CreateDirectory(directorypath);
                }
                // after creating directory delete all the files inside the directory
                if (System.IO.Directory.GetFiles(directorypath).Length > 0)
                {
                    var info = new DirectoryInfo(directorypath);
                    foreach (FileInfo f in info.GetFiles())
                        f.Delete();
                }

                string savedFilepath = webHostEnvironment.ContentRootPath + "/wwwroot" + string.Format("/ImportFiles/ImportFavorite/{0}/{1}", passport.UserId, File.FileName);
                using (var stream = System.IO.File.Create(savedFilepath))
                {
                    await File.CopyToAsync(stream);
                }

                //File.SaveAs(savedFilepath);
                ContextService.SetSessionValue("savedFilepath", savedFilepath, httpContext);
                ContextService.SetSessionValue("dirctorypath", directorypath, httpContext);
                 
                // create columns from file
                if (File == null)
                {
                    return Json(model);
                }

                model.ListOfcolumns = GetcolumnFromfile(File, savedFilepath, Convert.ToBoolean(param[0].ToString()));
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model);
        }
        [HttpPost]
        public JsonResult ImportFavoriteDDLChange([FromBody] ImportFavoriteReqModel @params)
        {
            string viewid = @params.ImportFavorite.SelectedDropdown;
            var model = new ImportFavoriteModel();
            model.isfieldsExist = true;
            try
            {
                if (!(Convert.ToDouble(viewid) == 0d))
                {
                    string strQuery = string.Format("Select (SUBSTRING(FieldName, charindex('.',FieldName)+1,LEN(FieldName))) as FieldName,id from ViewColumns where viewsid={0} and LookupType = {1}", viewid, (int)Enums.geViewColumnsLookupType.ltDirect);

                    var dsFields = new DataTable();
                    using (SqlConnection conn = passport.Connection())
                    {
                        using (var cmd = new SqlCommand(strQuery, conn))
                        {
                            using (var da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(dsFields);
                            }
                        }
                    }

                    var p = new Parameters(Int32.Parse(viewid), passport);

                    if (dsFields.Rows.Count > 0)
                    {
                        if (dsFields.Rows.Count > 0)
                        {
                            foreach (DataRow row in dsFields.Rows)
                            {
                                Type fieldType = Navigation.GetFieldType(p.TableName, row["FieldName"].ToString(), passport);
                                model.ListOfFieldName.Add(new ListOfFieldName() { text = row["FieldName"].ToString(), value = fieldType.Name });
                            }
                        }
                    }
                    else
                    {
                        model.isfieldsExist = false;
                    }
                }
                else
                {
                    model.isfieldsExist = false;
                }
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, 0, passport.DatabaseName);
            }

            return Json(model );
        }


        [HttpPost]
        public JsonResult BtnImportfavorite([FromBody]UserInterfaceJsonModel @params)
        {
            var model = new ImportFavoriteModel();
            model.chk1RowFieldNames = @params.ImportFavorite.chk1RowFieldNames;
            model.ColumnSelect = @params.ImportFavorite.ColumnSelect;
            model.favoritListSelectorid = @params.ImportFavorite.favoritListSelectorid;
            model.isLeftColumnChk = @params.ImportFavorite.isLeftColumnChk;
            model.isRightColumnChk = @params.ImportFavorite.isRightColumnChk;
            model.Targetfileds = @params.ImportFavorite.Targetfileds;
            model.ViewId = @params.ViewId;
            model.IscolString = @params.ImportFavorite.IscolString;
            // model.sourceFile = Session("savedFilepath")
            if (!object.ReferenceEquals(@params.ImportFavorite.sourceFile, "") & @params.ImportFavorite.sourceFile is not null)
            {
                model.sourceFile =ContextService.GetSessionValueByKey("savedFilepath", httpContext);
            }

            if (!validation(model))
            {
                return Json(model);
            }

            try
            {
                StartInsertDataToFavorite(model);
                model.UserMsg = Languages.get_Translation("msgImportFavoriteSuccess");
            }
            catch (Exception ex)
            {
                DataErrorHandler.ErrorHandler(ex, model, model.ViewId, passport.DatabaseName);
                if (ex.Message.Contains("Conversion failed when converting the varchar value"))
                {
                    model.errorType = "w";
                    model.UserMsg = string.Format(Languages.get_Translation("msgSelectTartgetFieldMismatch"));
                }
                else
                {
                    model.UserMsg = ex.Message;
                }

            }
           
            return Json(model );
        }
        private void StartInsertDataToFavorite(ImportFavoriteModel m)
        {
            // Find the table name from view based on the selected Favorite
            var arrayFields = new List<string>();
            var dsTableName = new DataSet();
            string strTableName = string.Empty;
            string strPKField = string.Empty;
            var FileData = new DataTable();
            using (SqlConnection conn = passport.Connection())
            {
                using (var cmd = new SqlCommand(string.Format("select v.TableName, (select case when t.IdFieldName is null then '' else (SUBSTRING(t.IdFieldName,charindex('.',t.IdFieldName)+1,LEN(t.IdFieldName))) end " + "from Tables t where ltrim(rtrim(t.TableName))=ltrim(rtrim(v.TableName)))as PKField from Views v where v.ID={0}", m.ViewId), conn))
                {
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dsTableName);
                    }
                }
            }
            if (dsTableName.Tables.Count > 0)
            {
                if (dsTableName.Tables[0].Rows.Count > 0)
                {
                    strTableName = dsTableName.Tables[0].Rows[0]["TableName"].ToString();
                    strPKField = dsTableName.Tables[0].Rows[0]["PKField"].ToString();
                }
            }
            string sqlquery = BuildQuery(m, strTableName, strPKField);
            CheckIfValueExist(m, strTableName, strPKField);
            SaveFavorite(m, sqlquery);
        }
        private void CheckIfValueExist(ImportFavoriteModel model, string tablename, string pkey)
        {
            foreach (string value in model.lstcoldata)
            {
                var dt = new DataTable();
                string whereClause = string.Empty;
                int counter = 1;
                foreach (ListOfFieldName f in model.Targetfileds)
                {
                    if (counter == model.Targetfileds.Count)
                    {
                        whereClause += string.Format(" {0} = {1}", f.text, value);
                    }
                    else
                    {
                        whereClause += string.Format(" {0} = {1} or ", f.text, value);
                    }
                    counter = counter + 1;
                }

                string query = string.Format("select {0} from {1} where {2}", pkey, tablename, whereClause);
                using (var cmd = new SqlCommand(query, passport.Connection()))
                {
                    var adp = new SqlDataAdapter(cmd);
                    adp.Fill(dt);
                }
                if (dt.Rows.Count == 0)
                {
                    model.lsRecNotExists.Add(value);
                }
                else
                {
                    model.lsRecExists.Add(value);
                }
            }
        }
        private void SaveFavorite(ImportFavoriteModel m, string sqlquery)
        {
            var dt = new DataTable();
            using (var cmd = new SqlCommand(sqlquery, passport.Connection()))
            {
                var adp = new SqlDataAdapter(cmd);
                adp.Fill(dt);
            }

            var  _is_SavedChildrenFavorite = new TabFusionRMS.RepositoryVB.Repositories<s_SavedChildrenFavorite>();
            SavedCriteria.SaveImportFavouriteMVC(m, dt, _is_SavedChildrenFavorite);
            WriteStatusReport(m);
        }
        private string BuildQuery(ImportFavoriteModel m, string table, string pkey)
        {
            var lsttrg = new List<string>();
            // lsttrg.Add(String.Format("[{0}]", pkey))
            // prepare select query
            string selectquery = string.Format("select [{0}] from [{1}]", pkey, table);
            foreach (DataRow col in GetdataFromfile(m).Rows)
            {
                string datacol = string.Format("'{0}'", col[Convert.ToInt32(m.ColumnSelect)].ToString());
                m.lstcoldata.Add(datacol);
            }
            // get fields for clause
            foreach (var f in m.Targetfileds)
                // If Not f.text = pkey Then
                // End If
                lsttrg.Add(string.Format("[{0}]", f.text));

            string wheretrg = string.Empty;
            int counter = 1;
            foreach (string trg in lsttrg)
            {
                if (counter == lsttrg.Count)
                {
                    wheretrg += string.Format("{0} in ({1})", trg, string.Join(",", m.lstcoldata));
                }
                else
                {
                    wheretrg += string.Format("{0} in ({1}) OR ", trg, string.Join(",", m.lstcoldata));
                }
                counter = counter + 1;
            }

            return string.Format("{0} where {1}", selectquery, wheretrg);
        }

        private DataTable GetdataFromfile(ImportFavoriteModel m)
        {
            var dtFileDataTable = new DataTable();
            string filepath = ContextService.GetSessionValueByKey("savedFilepath", httpContext);
            string extention = Path.GetExtension(filepath);
            extention = Conversions.ToString(Interaction.IIf(extention.Contains("."), extention.Replace(".", ""), extention));
            switch (extention.ToLower() ?? "")
            {
                case "xls":
                case "xlsx":
                    {
                        dtFileDataTable = SavedCriteria.getDataFromFile(extention, m.chk1RowFieldNames, filepath);
                        break;
                    } 
                case "csv":
                    {
                        dtFileDataTable = GetDataTabletFromCSVFile(filepath, m.chk1RowFieldNames);
                        break;
                    }
                case "txt":
                    {
                        var strLines = System.IO.File.ReadAllLines(filepath);
                        string strColName = string.Empty;
                        DataRow dtRow;

                        // Add column in datatable
                        for (int index = 1, loopTo = strLines.First().Split(ContextService.GetSessionValueByKey("Delmiter", httpContext)).Count(); index <= loopTo; index++)
                        {
                            strColName = SavedCriteria.getColumnLetterFromColumnIndex(index);
                            dtFileDataTable.Columns.Add(strColName);
                        }

                        // Fill data in datatable
                        int intRec = 0;
                        foreach (var line in strLines)
                        {
                            if (intRec == 0 & m.chk1RowFieldNames)
                            {
                                intRec += 1;
                                continue;
                            }
                            dtRow = dtFileDataTable.Rows.Add();
                            dtRow.ItemArray = line.Split(ContextService.GetSessionValueByKey("Delmiter", httpContext));
                        }

                        break;
                    }
            }
            return dtFileDataTable;
        }
        private static DataTable GetDataTabletFromCSVFile(string csv_file_path, bool chk1RowFieldNames)
        {
            var csvData = new DataTable();
            try
            {
                using (var csvReader = new TextFieldParser(csv_file_path))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    var colFields = csvReader.ReadFields();

                    // For Each column As String In colFields
                    // Dim datecolumn As DataColumn = New DataColumn(column)
                    // datecolumn.AllowDBNull = True
                    // csvData.Columns.Add(datecolumn)
                    // Next

                    // Add column in datatable
                    string strColName = string.Empty;
                    for (int index = 1, loopTo = colFields.Count(); index <= loopTo; index++)
                    {
                        strColName = SavedCriteria.getColumnLetterFromColumnIndex(index);
                        csvData.Columns.Add(strColName);
                    }

                    if (!chk1RowFieldNames)
                    {
                        for (int i = 0, loopTo1 = colFields.Length - 1; i <= loopTo1; i++)
                        {
                            if (string.IsNullOrEmpty(colFields[i]))
                            {
                                colFields[i] = null;
                            }
                        }
                        csvData.Rows.Add(colFields);
                    }

                    while (!csvReader.EndOfData)
                    {
                        var fieldData = csvReader.ReadFields();
                        for (int i = 0, loopTo2 = fieldData.Length - 1; i <= loopTo2; i++)
                        {
                            if (string.IsNullOrEmpty(fieldData[i]))
                            {
                                fieldData[i] = null;
                            }
                        }
                        csvData.Rows.Add(fieldData);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return csvData;
        }
        private bool validation(ImportFavoriteModel model)
        {
            model.isValidate = true;
            if (model.favoritListSelectorid == "0")
            {
                model.UserMsg = Languages.get_Translation("msgSelectFavoriteListField");
                model.isValidate = false;
            }
            else if (model.sourceFile is null) // Session("savedFilepath") Is Nothing Then
            {
                model.UserMsg = Languages.get_Translation("msgSelectASourceFile");
                model.isValidate = false;
            }
            else if (!model.isLeftColumnChk)
            {
                model.UserMsg = Languages.get_Translation("msgSelectSourceColumn");
                model.isValidate = false;
            }
            else if (!model.isRightColumnChk)
            {
                model.UserMsg = Languages.get_Translation("msgSelectTartgetField");
                model.isValidate = false;

            }
            return model.isValidate;
        }
        private List<string> GetcolumnFromfile(IFormFile file, string filepath, bool chkfirstRow)
        {
            var lst = new List<string>();
            string extention = "";
            string strDelmiter = "";
            extention = file.FileName.Split(".")[1].ToLower();
            switch (extention ?? "")
            {
                case "txt":
                case "csv":
                    {
                        if (extention == "csv")
                        {
                            strDelmiter = ",";
                        }
                        else
                        {
                            strDelmiter = Constants.vbTab;
                        }
                        using (var reader = new StreamReader(filepath))
                        {
                            for (int index = 1, loopTo = reader.ReadLine().Split(strDelmiter).Count(); index <= loopTo; index++)
                                lst.Add(SavedCriteria.getColumnLetterFromColumnIndex(index));
                        }

                        break;
                    }
                case "xls":
                case "xlsx":
                    {
                        lst = SavedCriteria.getColumnFromFile(extention, chkfirstRow, filepath, true);
                        break;
                    }
            }
            return lst;
        }
        private List<ImDropdownprops> BindDropdownList()
        {
            var lst = new List<ImDropdownprops>();
            var dsCriteria = new DataTable();
            string strQuery = string.Format("select SavedName,(CAST(id as varchar(20))+'|'+cast(isnull(ViewId,'0') AS varchar(20))) as ViewId from s_SavedCriteria where SavedType = {0} and UserId = {1}", (int)Enums.SavedType.Favorite, passport.UserId);
            using (SqlConnection conn = passport.Connection())
            {
                using (var cmd = new SqlCommand(strQuery, conn))
                {
                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dsCriteria);
                    }
                }
            }

            foreach (DataRow row in dsCriteria.Rows)
                lst.Add(new ImDropdownprops() { text = (string)row["SavedName"], value = (string)row["ViewId"] });
            return lst;
        }
        private void WriteStatusReport(ImportFavoriteModel m)
        {
            string recordString = "";

            var serverfilepaths =  ContextService.GetSessionValueByKey("dirctorypath", httpContext) + "/" + "Report.html";

            var fs = System.IO.File.Create(serverfilepaths);
            fs.Close();
            m.AReportUrl = string.Format("/ImportFiles/ImportFavorite/{0}/Report.html", passport.UserId);
            using (var writer = System.IO.File.AppendText(serverfilepaths))
            {
                writer.WriteLine(string.Format("<h2 style=\"text-align:center; margin-top:20px;\"> " + Languages.get_Translation("msgImportFavoriteStatusReport") + " </h2><hr  style=\"width: 37%;\">", "TAB FusionRMS"));
                // writer.WriteLine(String.Format(Languages.Translation("msgImportFavoriteStatusReport"), "                      TAB FusionRMS"))
                // writer.WriteLine("===========================================================================")
                // writer.WriteLine("===========================================================================")
            }

            using (var writer = System.IO.File.AppendText(serverfilepaths))
            {
                // writer.WriteLine()
                // writer.WriteLine()

                if (m.lsRecExists.Count > 0) // add rows for records exists
                {
                    writer.WriteLine(string.Format("<h3 style=\"color:slategrey;padding-left: 15px; padding-right: 15px; \">" + Languages.get_Translation("tiSuccessfullyImported") + "</h3>", m.lsRecExists.Count));
                    foreach (var row in m.lsRecExists)
                        recordString += string.Format("{0}, ", row);
                    writer.Write("<p style=\"width:100%; padding-left: 15px; padding-right: 15px;\">" + recordString.Substring(0, recordString.Length - 2) + "</p>");
                    // writer.WriteLine(String.Format(Languages.Translation("tiSuccessfullyImported"), m.lsRecExists.Count))
                    // writer.WriteLine("===========================================================================")
                    // For Each row In m.lsRecExists
                    // recordString += String.Format("{0}, ", row)
                    // Next
                    // writer.Write(recordString.Substring(0, recordString.Length - 2))
                    // writer.WriteLine()
                    // writer.WriteLine()
                    // writer.WriteLine()
                }

                if (m.lsRecNotExists.Count > 0) // add rows for records not exists
                {
                    writer.WriteLine("<h3 style=\"color:Red; padding-left: 15px; padding-right: 15px;\">" + string.Format(Languages.get_Translation("tiNotExistsInDestinationTable") + "</h3>", m.lsRecNotExists.Count));
                    recordString = string.Empty;
                    for (int i = 0, loopTo = m.lsRecNotExists.Count - 1; i <= loopTo; i++)
                        recordString += string.Format("{0}, ", m.lsRecNotExists[i]);
                    writer.Write("<p style=\" padding-left: 15px; padding-right: 15px;\">" + recordString.Substring(0, recordString.Length - 2) + "</p>");
                    // writer.WriteLine(String.Format(Languages.Translation("tiNotExistsInDestinationTable"), m.lsRecNotExists.Count))
                    // writer.WriteLine("===========================================================================")
                    // recordString = String.Empty
                    // For i = 0 To m.lsRecNotExists.Count - 1
                    // recordString += String.Format("{0}, ", m.lsRecNotExists(i))
                    // Next
                    // writer.Write(recordString.Substring(0, recordString.Length - 2))
                    // writer.WriteLine()
                    // writer.WriteLine()
                    // writer.WriteLine()
                }

                if (m.lsRecDuplicate.Count > 0) // add rows for records not exists
                {
                    writer.WriteLine("<h3 style=\"color:slategrey\">" + string.Format(Languages.get_Translation("tiDuplicateInDestinationTable") + "</h3>", m.lsRecDuplicate.Count));
                    recordString = string.Empty;
                    for (int i = 0, loopTo1 = m.lsRecDuplicate.Count - 1; i <= loopTo1; i++)
                        recordString += string.Format("{0}, ", m.lsRecDuplicate[i]);
                    writer.Write("<p style=\"width:100%\">" + recordString.Substring(0, recordString.Length - 2) + "</p>");
                    // writer.WriteLine(String.Format(Languages.Translation("tiDuplicateInDestinationTable"), m.lsRecDuplicate.Count))
                    // writer.WriteLine("===========================================================================")
                    // recordString = String.Empty
                    // For i = 0 To m.lsRecDuplicate.Count - 1
                    // recordString += String.Format("{0}, ", m.lsRecDuplicate(i))
                    // Next
                    // writer.Write(recordString.Substring(0, recordString.Length - 2))
                }
            }
        }
    }
}