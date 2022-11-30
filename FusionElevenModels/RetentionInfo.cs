 
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Smead.RecordsManagement;
using static Smead.RecordsManagement.Navigation;
using static Smead.RecordsManagement.Retention;
using Smead.Security;
using TabFusionRMS.DataBaseManagerVB;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.Resource;

namespace TabFusionRMS.WebCS
{
   
    // RETENTION INFO
   public class RetentionInfo : BaseModel
    {
        public RetentionInfo(string id, int viewid, Passport passport, HttpContext httpContext)
        {
            _passport = passport;
            _httpContext = httpContext;
            DropdownRetentionCode = new List<dropdownCode>();
            RetentionStatus = new retentionStatus();
            RetentionArchive = new retentionArchive();
            RetentionInfoInactivityDate = new retentionInactive();
            ListOfHeader = new List<string>();
            ListOfRows = new List<List<string>>();
            rowid = id;
            this.viewid = viewid;
        }
        public RetentionInfo(retentionInfoUIparams props, Passport? paspport, HttpContext httpContext)
        {
            _passport = paspport;
            _httpContext = httpContext;
            DropdownRetentionCode = new List<dropdownCode>();
            RetentionStatus = new retentionStatus();
            RetentionArchive = new retentionArchive();
            RetentionInfoInactivityDate = new retentionInactive();
            ListOfHeader = new List<string>();
            ListOfRows = new List<List<string>>();
            rowid = props.rowid;
            viewid = props.viewid;
            RetentionDescription = props.RetDescription;
            selectedItemText = props.RetentionItemText;
        }
        public RetentionInfo(Passport passport, HttpContext httpContext)
        {
            _passport = passport;
            _httpContext = httpContext;
            ReturnOnerow = new List<List<string>>();
        }
        public List<dropdownCode> DropdownRetentionCode { get; set; }
        public string RetentionDescription { get; set; }
        public string RetentionItem { get; set; }
        public retentionStatus RetentionStatus { get; set; }
        public retentionInactive RetentionInfoInactivityDate { get; set; }
        public string lblRetentionArchive { get; set; }
        public retentionArchive RetentionArchive { get; set; }
        private string rowid { get; set; }
        private int viewid { get; set; }
        internal Parameters @params { get; set; }
        private DataRow retentionItemRow { get; set; }
        private string selectedItemText { get; set; }
        private List<string> HoldingTable { get; set; }
        public List<List<string>> ListOfRows { get; set; }
        public List<string> ListOfHeader { get; set; }
        public bool BtnAdd { get; set; }
        public bool BtnDelete { get; set; }
        public bool BtnEdit { get; set; }
        public bool DDLDrop { get; set; }
        public string TableName { get; set; }
        public bool Disposed { get; set; }
        public int DispositionType { get; set; }
        public List<List<string>> ReturnOnerow { get; set; }
        public int SldestructionCertid { get; set; }

        public enum meFinalDisposition
        {
            fdNone = 0,
            fdPermanentArchive = 1,
            fdDestruction = 2,
            fdPurge = 3
        }
        // retention init
        public void GetRetentionInfoPerRow()
        {
            try
            {
                @params = new Parameters(viewid, _passport);
                RetentionItem = "Record Details";
                SetActiveItem();
                BuildDropdwonList();
                SetRetentionStatus();
                RetentionArchiveinfo();
                GenerateHoldingTable();
                CheckIfRetentionAssign();
            }

            catch (Exception ex)
            {
                string x = ex.Message;
            }

        }
        // dropdown change
        public void OnDropdownChange()
        {
            try
            {
                @params = new Parameters(viewid, _passport);
                RetentionItem = "Record Details";
                SetActiveItem();
                var tableInfo = GetTableInfo(@params.TableName, _passport);
                var codeRow = GetRetentionCode(selectedItemText, _passport);
                RetentionArchiveDate(tableInfo, codeRow);
                RetentionInactiveDate(tableInfo, codeRow);
                SetRetentionStatus(tableInfo, codeRow);
                RetentionArchiveinfo();
            }
            catch (Exception ex)
            {
                string x = ex.Message;
            }
        }
        // functions for init and dropdown change
        private void SetActiveItem()
        {
            IDBManager IDBManager = new DBManager();
            IDBManager.ConnectionString =  Keys.get_GetDBConnectionString();
            string query = string.Empty;
            if (FieldIsAString(@params.TableInfo, @params.KeyField, _passport))
            {
                query = string.Format("Select * from {0} where {1} = '{2}'", @params.TableName, @params.KeyField, rowid);
            }
            else
            {
                query = string.Format("Select * from {0} where {1} = {2}", @params.TableName, @params.KeyField, rowid);
            }

            var loutput = IDBManager.ExecuteDataSet(CommandType.Text, query);
            retentionItemRow = loutput.Tables[0].Rows[0];
        }
        private void BuildDropdwonList()
        {
            selectedItemText = "";
            RetentionDescription = "";
            var dtRetention = GetRetentionCodes(_passport);
            string retentionField = @params.TableInfo["RetentionFieldName"].ToString();
            foreach (DataRow retRow in dtRetention.Rows)
            {
                if (string.Compare(retentionItemRow[retentionField].ToString(), retRow["id"].ToString(), true) == 0)
                {
                    RetentionDescription = retRow["description"].ToString();
                    selectedItemText = retRow["id"].ToString().ToUpper();
                    DropdownRetentionCode.Add(new dropdownCode() { value = retRow["description"].ToString(), text = retRow["id"].ToString().ToUpper(), selected = true });
                }
                else
                {
                    DropdownRetentionCode.Add(new dropdownCode() { value = retRow["description"].ToString(), text = retRow["id"].ToString().ToUpper(), selected = false });
                }
            }
            var tableInfo = GetTableInfo(@params.TableName, _passport);
            var codeRow = GetRetentionCode(selectedItemText, _passport);
            RetentionArchiveDate(tableInfo, codeRow);
            RetentionInactiveDate(tableInfo, codeRow);
        }
        private void SetRetentionStatus()
        {
            if (!string.IsNullOrWhiteSpace(RetentionDescription))
            {
                var tableInfo = GetTableInfo(@params.TableName, _passport);
                var codeRow = GetRetentionCode(selectedItemText, _passport);
                SetRetentionStatus(tableInfo, codeRow);
            }
            else
            {
                checkRetentionStatus();
            }
        }
        private void SetRetentionStatus(DataRow tableinfo, DataRow codeRow)
        {
            if (!string.IsNullOrWhiteSpace(RetentionDescription))
            {
                if (CBoolean(codeRow["RetentionLegalHold"]))
                {
                    RetentionStatus.text = Languages.get_Translation("lblRetentionInfoOnHold");
                }
                else
                {
                    RetentionStatus.text = Languages.get_Translation("lblRetentionInfoStatus");
                }
            }

            checkRetentionStatus();
        }
        private void checkRetentionStatus()
        {
            var destCert = GetDescCertRow(@params.TableName, rowid, _passport);

            if (destCert is not null)
            {
                bool disposed = false;
                var dispositiontype = meFinalDisposition.fdNone;
                bool isHold = CBoolean(destCert["RetentionHold"]) | CBoolean(destCert["LegalHold"]);
                if (destCert["DispositionDate"] is not null && !string.IsNullOrEmpty(destCert["DispositionDate"].ToString()))
                    disposed = true;
                if (destCert["DispositionType"] is not null)
                    dispositiontype = (meFinalDisposition)Conversions.ToInteger(destCert["DispositionType"]);
                Disposed = disposed;
                DispositionType = (int)dispositiontype;

                if (isHold & dispositiontype == 0)
                {
                    RetentionStatus.text = Languages.get_Translation("lblRetentionInfoOnHold");
                }
                else if (dispositiontype == meFinalDisposition.fdDestruction)
                {
                    if (Conversions.ToInteger(destCert["SLDestructionCertsId"]) == 0)
                    {
                        RetentionStatus.text = Languages.get_Translation("lblRetentionInformationDestroyedParentDisposed");
                        RetentionStatus.color = "red";
                    }
                    else if (disposed)
                    {
                        var cert = GetDescCert(Conversions.ToInteger(destCert["SLDestructionCertsId"]), _passport);
                        // lblStatus.Text = "Destroyed [" + cert("Id").ToString + " - " + CDate(cert("DateCreated")).ToClientDateFormat + "]"
                        RetentionStatus.text = string.Format(Languages.get_Translation("lblRetentionInformationDestroyedX-X"), cert["Id"].ToString(), Conversions.ToDate(cert["DateCreated"]).ToClientDateFormat());
                        RetentionStatus.color = "red";
                    }
                    else
                    {
                        var cert = GetDescCert(Conversions.ToInteger(destCert["SLDestructionCertsId"]), _passport);
                        // lblStatus.Text = "Eligible to be Destroyed [" + cert("Id").ToString + " - " + CDate(cert("DateCreated")).ToClientDateFormat + "]"
                        RetentionStatus.text = string.Format(Languages.get_Translation("lblRetentionInformationEligible2BDestroyedX-X"), cert["Id"].ToString(), Conversions.ToDate(cert["DateCreated"]).ToClientDateFormat());
                    }
                }
                else if (dispositiontype == meFinalDisposition.fdPermanentArchive)
                {
                    if (Conversions.ToInteger(destCert["SLDestructionCertsId"]) == 0)
                    {
                        RetentionStatus.text = Languages.get_Translation("lblRetentionInformationArchivedParentDisposed");
                        RetentionStatus.color = "red";
                    }
                    else if (disposed)
                    {
                        var cert = GetDescCert(Conversions.ToInteger(destCert["SLDestructionCertsId"]), _passport);
                        // lblStatus.Text = "Archived [" + cert("Id").ToString + " - " + CDate(cert("DateCreated")).ToClientDateFormat + "]"
                        RetentionStatus.text = string.Format(Languages.get_Translation("lblRetentionInformationArchivedX-X"), cert["Id"].ToString(), Conversions.ToDate(cert["DateCreated"]).ToClientDateFormat());
                        RetentionStatus.color = "red";
                    }
                    else
                    {
                        var cert = GetDescCert(Conversions.ToInteger(destCert["SLDestructionCertsId"]), _passport);
                        RetentionStatus.text = string.Format(Languages.get_Translation("lblRetentionInformationEligible2BArchivedX-X"), cert["Id"].ToString(), Conversions.ToDate(cert["DateCreated"]).ToClientDateFormat());
                    }
                }
                else
                {
                    try
                    {
                        var cert = GetDescCert(Conversions.ToInteger(destCert["SLDestructionCertsId"]), _passport);
                        dispositiontype = (meFinalDisposition)Conversions.ToInteger(cert["RetentionDispositionType"]);

                        if (dispositiontype == meFinalDisposition.fdPermanentArchive)
                        {
                            RetentionStatus.text = string.Format(Languages.get_Translation("lblRetentionInformationEligible2BArchivedX-X"), cert["Id"].ToString(), Conversions.ToDate(cert["DateCreated"]).ToClientDateFormat());
                        }
                        else if (dispositiontype == meFinalDisposition.fdDestruction)
                        {
                            RetentionStatus.text = string.Format(Languages.get_Translation("lblRetentionInformationEligible2BDestroyedX-X"), cert["Id"].ToString(), Conversions.ToDate(cert["DateCreated"]).ToClientDateFormat());
                        }
                        else if (dispositiontype == meFinalDisposition.fdPurge)
                        {
                            RetentionStatus.text = string.Format(Languages.get_Translation("lblRetentionInformationEligible2BPurgingX-X"), cert["Id"].ToString(), Conversions.ToDate(cert["DateCreated"]).ToClientDateFormat());
                        }
                        else
                        {
                            RetentionStatus.text = Languages.get_Translation("lblRetentionInfoStatus");
                        }
                    }
                    catch (Exception)
                    {
                        RetentionStatus.text = Languages.get_Translation("lblRetentionInfoStatus");
                    }
                }
            }
        }
        private void RetentionArchiveinfo()
        {
            if (retentionItemRow is null)
                return;
            IRepository<TabFusionRMS.Models.Table> table = new Repositories<TabFusionRMS.Models.Table>();
            var objTables = table.All().FirstOrDefault(x => (x.TableName ?? "") == (@params.TableName ?? ""));

            if (objTables is not null)
            {
                if (1 is var arg2 && objTables.RetentionFinalDisposition is { } arg1 && arg1 == arg2)
                {
                    lblRetentionArchive = Languages.get_Translation("lblRetentionInfoArchiveDate");
                }
                else if (2 is var arg4 && objTables.RetentionFinalDisposition is { } arg3 && arg3 == arg4)
                {
                    lblRetentionArchive = Languages.get_Translation("lblRetentionInformationDestructionDate");
                }
                else if (3 is var arg6 && objTables.RetentionFinalDisposition is { } arg5 && arg5 == arg6)
                {
                    lblRetentionArchive = Languages.get_Translation("lblRetentionInformationPurgeDate");
                }
                else
                {
                    lblRetentionArchive = Languages.get_Translation("lblRetentionInfoArchiveDate");
                }
            }
            else
            {
                lblRetentionArchive = Languages.get_Translation("lblRetentionInfoArchiveDate");
            }

            // ceSnoozeDate.Format = Keys.GetCultureCookies().DateTimeFormat.ShortDatePattern
            // RetentionArchive = "Record Details"
        }
        private void RetentionArchiveDate(DataRow tableInfo, DataRow codeRow)
        {
            meFinalDisposition dispositionType;

            if (!CBoolean(tableInfo["RetentionPeriodActive"]))
            {
                RetentionArchive.text = Languages.get_Translation("lblRetentionInformationNA");
                RetentionArchive.color = "black";
                return;
            }

            try
            {
                dispositionType = (meFinalDisposition)Conversions.ToInteger(retentionItemRow["%slRetentionDispositionStatus"]);
            }
            catch (Exception)
            {
                dispositionType = meFinalDisposition.fdNone;
            }

            if (dispositionType != meFinalDisposition.fdNone)
            {
                RetentionArchive.text = Languages.get_Translation("lblRetentionInformationNA");
                RetentionArchive.color = "black";
                return;
            }

            string dateField = string.Empty;

            switch (codeRow["RetentionEventType"].ToString() ?? "")
            {
                case "Date Opened":
                    {
                        dateField = tableInfo["RetentionDateOpenedField"].ToString();
                        break;
                    }
                case "Date Created":
                    {
                        dateField = tableInfo["RetentionDateCreateField"].ToString();
                        break;
                    }
                case "Date Closed":
                    {
                        dateField = tableInfo["RetentionDateClosedField"].ToString();
                        break;
                    }
                case "Date Other":
                    {
                        dateField = tableInfo["RetentionDateOtherField"].ToString();
                        break;
                    }
            }

            try
            {
                if (!string.IsNullOrEmpty(dateField))
                {
                    var dDispositionDate = ApplyYearEndToDate(Conversions.ToDate(retentionItemRow[dateField]), Conversions.ToDouble(codeRow["RetentionPeriodTotal"]), CBoolean(codeRow["RetentionPeriodForceToEndOfYear"]), _passport);
                    RetentionArchive.text = dDispositionDate.ToClientDateFormat();
                    RetentionArchive.color = Interaction.IIf(DateTime.Parse(Conversions.ToString(RetentionArchive.text)) > DateTime.Today, "black", "red");
                }
                else
                {
                    RetentionArchive.text = Languages.get_Translation("lblRetentionInformationNA");
                    RetentionArchive.color = "black";
                }
            }
            catch
            {
                RetentionArchive.text = Languages.get_Translation("lblRetentionInformationNODATEENTERED");
                RetentionArchive.color = "red";
            }
        }
        private void RetentionInactiveDate(DataRow tableInfo, DataRow codeRow)
        {
            meFinalDisposition dispositionType;

            if (!CBoolean(tableInfo["RetentionInactivityActive"]))
            {
                RetentionInfoInactivityDate.text = Languages.get_Translation("lblRetentionInformationNA");
                RetentionInfoInactivityDate.color = "black";
                return;
            }

            try
            {
                dispositionType = (meFinalDisposition)Conversions.ToInteger(retentionItemRow["%slRetentionDispositionStatus"]);
            }
            catch (Exception)
            {
                dispositionType = meFinalDisposition.fdNone;
            }

            if (dispositionType != meFinalDisposition.fdNone)
            {
                RetentionInfoInactivityDate.text = Languages.get_Translation("lblRetentionInformationNA");
                RetentionInfoInactivityDate.color = "black";
                return;
            }

            string dateField = string.Empty;

            switch (codeRow["InactivityEventType"].ToString() ?? "")
            {
                case "Date Opened":
                    {
                        dateField = tableInfo["RetentionDateOpenedField"].ToString();
                        break;
                    }
                case "Date Created":
                    {
                        dateField = tableInfo["RetentionDateCreateField"].ToString();
                        break;
                    }
                case "Date Closed":
                    {
                        dateField = tableInfo["RetentionDateClosedField"].ToString();
                        break;
                    }
                case "Date Other":
                    {
                        dateField = tableInfo["RetentionDateOtherField"].ToString();
                        break;
                    }
            }

            try
            {
                if (!string.IsNullOrEmpty(dateField))
                {
                    var dDispositionDate = ApplyYearEndToDate(Conversions.ToDate(retentionItemRow[dateField]), Conversions.ToDouble(codeRow["InactivityPeriod"]), CBoolean(codeRow["InactivityForceToEndOfYear"]), _passport);
                    RetentionInfoInactivityDate.text = dDispositionDate.ToClientDateFormat();
                    RetentionInfoInactivityDate.color = Conversions.ToString(Interaction.IIf(DateTime.Parse(RetentionInfoInactivityDate.text) > DateTime.Today, "black", "red"));
                }
                else
                {
                    RetentionInfoInactivityDate.text = Languages.get_Translation("lblRetentionInformationNA");
                    RetentionInfoInactivityDate.color = "red";
                }
            }
            catch (Exception)
            {
                RetentionInfoInactivityDate.text = Languages.get_Translation("lblRetentionInformationNODATEENTERED");
                RetentionInfoInactivityDate.color = "red";
            }
        }
        private void GenerateHoldingTable()
        {
            ListOfHeader.Add(Languages.get_Translation("tiRetentionInformationHoldType"));
            ListOfHeader.Add(Languages.get_Translation("btnRetentionInfoSnooze"));
            ListOfHeader.Add(Languages.get_Translation("tiRetentionInformationReason"));
            IRepository<SLDestructCertItem> SLDestructCertItem = new Repositories<SLDestructCertItem>();
            var result = SLDestructCertItem.Where(x => x.TableName == @params.TableName && x.TableId == rowid  && (x.RetentionHold == true || x.LegalHold == true)).OrderBy(x => x.Id).ToList();
            if (result.Count == 0)
            {
                var certid = SLDestructCertItem.Where(x => x.TableName == @params.TableName && x.TableId == rowid).OrderBy(x => x.Id).FirstOrDefault();
                if (certid is not null)
                    SldestructionCertid = (int)certid.SLDestructionCertsId;
            }

            for (int index = 0, loopTo = result.Count - 1; index <= loopTo; index++)
            {
                var cell = new List<string>();
                var objNewSLDestructCertItem = new NewSLDestructCertItem();
                if (true is var arg12 && result[index].LegalHold is { } arg11 && arg11 == arg12)
                {
                    cell.Add("Legal");
                }
                else if (true is var arg14 && result[index].RetentionHold is { } arg13 && arg13 == arg14)
                {
                    cell.Add("Retention");
                }
                if (!string.IsNullOrEmpty(Convert.ToString(result[index].SnoozeUntil)))
                {
                    cell.Add( Keys.get_ConvertCultureDate(result[index].SnoozeUntil.ToString(), _httpContext,false));
                }
                else
                {
                    cell.Add("");
                }
                cell.Add(result[index].HoldReason);
                ListOfRows.Add(cell);

                // TableHoldingTemp.Add(New holdingTableprop With {.Id = result(index).Id, .HoldReason = result(index).HoldReason, .LegalHold = result(index).LegalHold, .RetentionCode = result(index).RetentionCode, .RetentionHold = result(index).RetentionHold, .SLDestructionCertsId = result(index).SLDestructionCertsId, .SnoozeUntil = result(index).SnoozeUntil, .TableId = result(index).TableId, .TableName = result(index).TableName})
            }
        }
        private void CheckIfRetentionAssign()
        {
            // check if disable the dropdown in case there is no retention policy 
            if (@params.TableInfo["RetentionAssignmentMethod"] is DBNull)
            {
                DDLDrop = false;
            }
            else if (Conversions.ToInteger(@params.TableInfo["RetentionAssignmentMethod"]) == 0)
            {
                try
                {
                    DDLDrop = Conversions.ToInteger(retentionItemRow["%slRetentionDispositionStatus"]) == 0;
                }
                catch (Exception)
                {
                    DDLDrop = true;
                }
            }
            else
            {
                DDLDrop = false;
            }
        }
        // updating information
        public void StartUpdating(retentionInfoUIparams param)
        {
            try
            {
                UpdateRetentionCodeInTableRecord(param);
                DeleteDestructCertItem(param);
                UpdateDestructCertItem(param);
                if(_passport is not null)
                {
                    if(_passport.Connection() != null)
                    {
                        using (var conn = _passport.Connection())
                        {
                            if (GetDestructionCertChildrenCount(param.SldestructionCertId, conn) == 0)
                                DeleteDestructionCertRecord(param.SldestructionCertId, conn);
                        }
                    }
                }
            }

            catch (Exception)
            {
                errorNumber = 1;
                // write to event viewer
            }

        }
        private void UpdateRetentionCodeInTableRecord(retentionInfoUIparams param)
        {
            string query = "UPDATE [{0}] SET [{1}] = @retentionCode WHERE [{2}] = @tableID";
            using (var cmd = new SqlCommand(string.Format(query, param.TableName, GetRetentionFieldName(param.TableName, _passport.Connection()), GetPrimaryKeyFieldName(param.TableName, _passport.Connection())), _passport.Connection()))
            {
                cmd.Parameters.AddWithValue("@retentionCode", param.RetentionItemCode);
                cmd.Parameters.AddWithValue("@tableID", param.rowid);
                cmd.ExecuteNonQuery();
            }
        }
        private void DeleteDestructCertItem(retentionInfoUIparams param)
        {
            IRepository<SLDestructCertItem> sLDestructCertItem = new Repositories<SLDestructCertItem>();
            var deleteRecord = sLDestructCertItem.All().Where(x => (x.TableId ?? "") == (param.rowid ?? "") & (x.TableName ?? "") == (param.TableName ?? "")).ToList();
            sLDestructCertItem.DeleteRange(deleteRecord);
        }
        private void UpdateDestructCertItem(retentionInfoUIparams param)
        {
            // UpdateRetentionCodeInTableRecord(param)
            // Dim sLDestructCertItem As IRepository(Of SLDestructCertItem) = New Repositories(Of SLDestructCertItem)()
            // Dim deleteRecord = sLDestructCertItem.All.Where(Function(x) x.TableId = param.rowid And x.TableName = param.TableName).ToList()
            // sLDestructCertItem.DeleteRange(deleteRecord)
            if (param.RetTableHolding.Count == 0)
                return;
            for (int index = 0, loopTo = param.RetTableHolding.Count - 1; index <= loopTo; index++)
            {
                IRepository<SLDestructCertItem> repSLDestructCertItem = new Repositories<SLDestructCertItem>();
                var objSLDestructCertItem = new SLDestructCertItem();

                try
                {
                    objSLDestructCertItem.ScheduledDestruction =  Keys.get_ConvertStringToCultureDate(param.RetnArchive, _httpContext);
                }
                catch (Exception)
                {
                    objSLDestructCertItem.ScheduledDestruction = default;
                }

                try
                {
                    objSLDestructCertItem.ScheduledInactivity = Keys.get_ConvertStringToCultureDate(param.RetnInactivityDate, _httpContext);
                }
                catch (Exception)
                {
                    objSLDestructCertItem.ScheduledInactivity = default;
                }

                objSLDestructCertItem.RetentionCode = !string.IsNullOrEmpty(param.RetentionItemCode) ? param.RetentionItemCode : null;
                objSLDestructCertItem.RetentionHold = Conversions.ToBoolean(param.RetTableHolding[index].RetentionHold);
                objSLDestructCertItem.LegalHold = Conversions.ToBoolean(param.RetTableHolding[index].LegalHold);
                objSLDestructCertItem.SnoozeUntil = param.RetTableHolding[index].SnoozeUntil;
                objSLDestructCertItem.TableId = param.RetTableHolding[index].TableId;
                objSLDestructCertItem.TableName = param.RetTableHolding[index].TableName;
                objSLDestructCertItem.SLDestructionCertsId = Conversions.ToInteger(param.RetTableHolding[index].SLDestructionCertsId);
                objSLDestructCertItem.HoldReason = param.RetTableHolding[index].HoldReason;

                if ((param.RetTableHolding[index].SnoozeUntil is null | (DateTime.Today is var arg16 && param.RetTableHolding[index].SnoozeUntil is { } arg15 ? arg15 > arg16 : (bool?)null)) == true)
                {
                    repSLDestructCertItem.Add(objSLDestructCertItem);
                }

            }
        }
        // Public Sub RemoveRetentioninfoRows(listids As List(Of Integer))
        // Dim sLDestructCertItem As IRepository(Of SLDestructCertItem) = New Repositories(Of SLDestructCertItem)()
        // Try
        // For Each id In listids
        // 'Dim objSLDestructCertItem = sLDestructCertItem.FirstOrDefault(Function(x) x.Id = id)
        // Dim objSLDestructCertItem = sLDestructCertItem.Where(Function(a) a.Id = id)
        // If Not objSLDestructCertItem Is Nothing Then
        // sLDestructCertItem.DeleteRange(objSLDestructCertItem)
        // End If
        // Next
        // Catch ex As Exception
        // Me.errorNumber = 1
        // End Try

        // End Sub
    }

    public class retentionInfoUIparams
    {
        public retentionInfoUIparams()
        {
            RetTableHolding = new List<holdingTableprop>();
        }
        public string rowid { get; set; }
        public int viewid { get; set; }
        public string RetDescription { get; set; }
        public string RetentionItemText { get; set; }
        public string TableName { get; set; }
        public string RetentionItemCode { get; set; }
        public string RetnArchive { get; set; }
        public string RetnInactivityDate { get; set; }
        public List<holdingTableprop> RetTableHolding { get; set; }
        public int SldestructionCertId { get; set; }
    }

    public class dropdownCode
    {
        public string value { get; set; }
        public string text { get; set; }
        public bool selected { get; set; }
    }

    public class retentionStatus
    {
        public string text { get; set; }
        public string color { get; set; }
    }

    public class retentionArchive
    {
        public object text { get; set; }
        public object color { get; set; }
    }

    public class retentionInactive
    {
        public string text { get; set; }
        public string color { get; set; }
    }

    public class holdingTableprop
    {
        public string RetentionCode { get; set; }
        public bool RetentionHold { get; set; }
        public DateTime? SnoozeUntil { get; set; }
        public string TableId { get; set; }
        public string TableName { get; set; }
        public int SLDestructionCertsId { get; set; }
        public string HoldReason { get; set; }
        public bool? LegalHold { get; set; }
        public int? Id { get; set; }
    }
    public class RetentionInfoUpdateReqModel
    {
        public retentionInfoUIparams props { get; set; }
    }
}