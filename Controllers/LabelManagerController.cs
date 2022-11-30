
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Web;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using TabFusionRMS.DataBaseManagerVB;
using Newtonsoft.Json;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing.Text;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using TabFusionRMS.Resource;
using Newtonsoft.Json.Linq;
using Smead.Security;

namespace TabFusionRMS.WebCS.Controllers
{
    public class LabelManagerController : BaseController
    {
        private IDBManager _IDBManager = new DBManager();
        // Private Property _IDBManager As IDBManager
        private IRepository<OneStripJob> _iOneStripJob
        {
            get;
            set;
        }
        private IRepository<OneStripJobField> _iOneStripJobsField
        {
            get;
            set;
        }
        private IRepository<OneStripForm> _iOneStripForms
        {
            get;
            set;
        }
        private IRepository<Table> _iTables
        {
            get;
            set;
        }
        private IRepository<Databas> _iDatabas
        {
            get;
            set;
        }
       
        // IDBManager As IDBManager, 
        public LabelManagerController(IRepository<OneStripJob> iOneStripJob, IRepository<OneStripJobField> iOneStripJobField, IRepository<OneStripForm> iOneStripForms, IRepository<Table> iTable, IRepository<Databas> iDatabase)
        {
            // _IDBManager = IDBManager
            _iOneStripJob = iOneStripJob;
            _iOneStripJobsField = iOneStripJobField;
            _iOneStripForms = iOneStripForms;
            _iTables = iTable;
            _iDatabas = iDatabase;
        }

        //public LabelManagerController(IRepository<OneStripJob> iOneStripJob, IRepository<OneStripJobField> iOneStripJobField, IRepository<OneStripForm> iOneStripForms, IRepository<Table> iTable, IRepository<Databas> iDatabase, IHttpContextAccessor httpContext) : base(httpContext)
        //{
        //    // _IDBManager = IDBManager
        //    _iOneStripJob = iOneStripJob;
        //    _iOneStripJobsField = iOneStripJobField;
        //    _iOneStripForms = iOneStripForms;
        //    _iTables = iTable;
        //    _iDatabas = iDatabase;
        //}

        public ActionResult Index()
        {
			// Keys.iLabelRefId = Keys.GetUserId
			httpContext.Session.SetString("iLabelRefId", passport.UserId.ToString());
            return View();
        }

        public PartialViewResult LoadAddEditLabel()
        {
            return PartialView("_AddBarCodePartial");
        }

		public ActionResult GetFirstValue(string table, string field, string SQL_String)
		{
			var sString = "";
			var sSql = "";
			DataSet loutput;
			var oBarCodePrefix = "";
			var oDBManager = new DBManager();

			try
			{
				var pTable = _iTables.All().Where(x => x.TableName.Trim().ToLower().Equals(table.Trim().ToLower()) && !string.IsNullOrEmpty(x.DBName.Trim().ToLower())).FirstOrDefault();

				var oTable = _iTables.All().Where(x => x.TableName.Trim().ToLower().Equals(table.Trim().ToLower())).FirstOrDefault();

				oBarCodePrefix = oTable.BarCodePrefix;

				Databas pDatabaseEntity = null;
				if (pTable != null)
				{
					pDatabaseEntity = _iDatabas.Where(x => x.DBName.Trim().ToLower().Equals(pTable.DBName.Trim().ToLower())).FirstOrDefault();
					oDBManager.ConnectionString = Keys.get_GetDBConnectionString(pDatabaseEntity);
				}
				else

					oDBManager.ConnectionString = Keys.get_GetDBConnectionString(pDatabaseEntity);

				var finalSQL = SQL_String.Split(new string[] { "WHERE" }, StringSplitOptions.None)[0];

				var dateColumn = true;
				if (field.ToString().ToLower().Contains("date"))
				{
					sSql = "SELECT TOP 1 *, CONVERT(VARCHAR(19)," + field + ",121) As DateColumn, " + finalSQL.Substring(7);
					// sSql = "SELECT TOP 1 *,CONVERT(VARCHAR(19)," + field + ",121) As DateColumn FROM " + table
					dateColumn = true;
				}
				else
				{
					sSql = "SELECT TOP 1 " + finalSQL.Substring(7);
					// sSql = "SELECT TOP 1 * FROM " + table
					dateColumn = false;
				}

				// sSql = "SELECT TOP 1 [" + field + "] FROM " + table + " ORDER BY [" + field + "] ASC"

				loutput = oDBManager.ExecuteDataSet(System.Data.CommandType.Text, sSql);

				if (loutput.Tables[0].Rows.Count == 0)
					sString = field;
				else if (dateColumn)
					// 'Changed by Hasmukh on 06/15/2016 for date format changes
					sString = Keys.get_ConvertCultureDate(loutput.Tables[0].Rows[0]["DateColumn"].ToString(), httpContext, true, false); // Date.Parse(loutput.Tables(0).Rows(0).Item("DateColumn").ToString()).ToString(Keys.GetCultureCookies().DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture)
				else
					sString = loutput.Tables[0].Rows[0][field].ToString();

				oDBManager.CommitTransaction();
			}
			catch (Exception)
			{
				sString = field;
				oDBManager.CommitTransaction();
				Keys.ErrorType = "e";
				Keys.ErrorMessage = Keys.UnableToConnect("");
			}
			return Json(new
			{
				value = sString,
				barCodePrefix = oBarCodePrefix,
				errortype = Keys.ErrorType,
				message = Keys.ErrorMessage
			});
		}

		public ActionResult AddLabel(OneStripJob pOneStripJobs, bool pDrawLabels)
		{
			int? labelId = 0;
			string onestripjobs = null;
			string onestripjobfields = null;
			int lError = 0;
			string sErrorMessage = null;
			string sMessage = string.Empty;
			ADODB.Recordset rsTestSQL = new ADODB.Recordset();
			bool ValidateSQL = true;
			string oBarCodePrefix = null;

			var Setting = new JsonSerializerSettings();
			Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

			var oTable = _iTables.All().Where(x => x.TableName == pOneStripJobs.TableName).FirstOrDefault();

			oBarCodePrefix = oTable.BarCodePrefix;

			var sSQL = pOneStripJobs.SQLString.Replace("%ID%", "0");

			var uSQL = pOneStripJobs.SQLUpdateString;

			if (((sSQL).Trim().Length == 0))
				ValidateSQL = true;
			else
			{
				  rsTestSQL = DataServices.GetADORecordset(sSQL, oTable, _iDatabas.All(), lError: ref lError,sErrorMessage: ref sErrorMessage);

				if (lError != 0)
				{
					if ((lError == -2147217865))
					{
						// sMessage = sMessage & " contains an Invalid Table Name."
						Keys.ErrorType = "e";
						Keys.ErrorMessage = Languages.get_Translation("msgLabelManagerCtrlSQLStrHvInvalidTblName");
					}
					else
					{
						// sMessage = sMessage & " is Invalid."
						Keys.ErrorType = "e";
						Keys.ErrorMessage = Languages.get_Translation("msgLabelManagerCtrlSQLStatementInvalid");
					}
					// Fail
					return Json(new
					{
						errortype = Keys.ErrorType,
						message = Keys.ErrorMessage
					});
				}
				else
					ValidateSQL = true;

				if ((!(rsTestSQL == null)))
				{
					rsTestSQL.Close();
					rsTestSQL = null /* TODO Change to default(_) if this is not a reference type */ ;
				}

				if ((uSQL != null))
				{
                    uSQL = uSQL.Replace("%ID%", "0");
                    uSQL = DataServices.InjectWhereIntoSQL(uSQL, "0=1");
					// check by raju
					rsTestSQL = DataServices.GetADORecordset(uSQL, oTable, _iDatabas.All(), default, default , default , default  , default  , default ,ref lError,ref sErrorMessage);

                    if (lError != 0)
					{
						if ((lError == -2147217865))
						{
							// sMessage = sMessage & " contains an Invalid Table Name."
							Keys.ErrorType = "e";
							Keys.ErrorMessage = Languages.get_Translation("msgLabelManagerCtrlSQLStrHvInvalidTblName");
						}
						else
						{
							// sMessage = sMessage & " is Invalid."
							Keys.ErrorType = "e";
							Keys.ErrorMessage = Languages.get_Translation("msgLabelManagerCtrlSQLUpdateStrInvalid");
						}
						// Fail
						return Json(new
						{
							errortype = Keys.ErrorType,
							message = Keys.ErrorMessage
						});
					}
					else
						ValidateSQL = true;

					if ((!(rsTestSQL == null)))
					{
						rsTestSQL.Close();
						rsTestSQL = null /* TODO Change to default(_) if this is not a reference type */ ;
					}
				}
			}

			if (pOneStripJobs.Id > 0)
			{
				var tempOneStripJobs = _iOneStripJob.All().Where(x => x.Name.Trim().ToLower().Equals(pOneStripJobs.Name.Trim().ToLower()) && x.Id != pOneStripJobs.Id).FirstOrDefault();
				if (tempOneStripJobs == null)
				{
					pOneStripJobs.Inprint = 0;
					pOneStripJobs.UserUnits = 0;
					pOneStripJobs.LastCounter = 0;
					pOneStripJobs.DrawLabels = pDrawLabels;
					_iOneStripJob.Update(pOneStripJobs);
					labelId = _iOneStripJob.All().Where(x => x.Name.Trim().ToLower().Equals(pOneStripJobs.Name.Trim().ToLower())).FirstOrDefault()?.Id;
					var oOneStripJobs = _iOneStripJob.All().Where(x => x.Name.Trim().ToLower().Equals(pOneStripJobs.Name.Trim().ToLower())).FirstOrDefault();
					var oOneStripJobFields = _iOneStripJobsField.All().Where(x => x.OneStripJobsId == pOneStripJobs.Id);
					onestripjobfields = JsonConvert.SerializeObject(oOneStripJobFields, Formatting.Indented, Setting);
					onestripjobs = JsonConvert.SerializeObject(oOneStripJobs, Formatting.Indented, Setting);
					Keys.ErrorType = "s";
					Keys.ErrorMessage = Languages.get_Translation("msgLabelManagerCtrlLblUpdatedSuccessfully");
				}
				else
				{
					Keys.ErrorType = "w";
					Keys.ErrorMessage = Languages.get_Translation("msgLabelManagerCtrlLblNameExists");
				}
			}
			else
			{
				var pOneStripJob = _iOneStripJob.All().Where(x => x.Name.Trim().ToLower().Equals(pOneStripJobs.Name.Trim().ToLower())).FirstOrDefault();
				if (pOneStripJob == null)
				{
					pOneStripJobs.Inprint = 0;
					pOneStripJobs.UserUnits = 0;
					pOneStripJobs.LastCounter = 0;
					pOneStripJobs.DrawLabels = pDrawLabels;
					_iOneStripJob.Add(pOneStripJobs);
					labelId = _iOneStripJob.All().Where(x => x.Name.Trim().ToLower().Equals(pOneStripJobs.Name.Trim().ToLower())).FirstOrDefault()?.Id;

					Keys.ErrorType = "s";
					Keys.ErrorMessage = Languages.get_Translation("msgLabelManagerCtrlLblCreatedSuccessfully");
				}
				else
				{
					labelId = 0;
					Keys.ErrorType = "w";
				}
			}

			var tableName = pOneStripJobs.TableName.ToString();

			var rowCount = GetCount(tableName);

			return Json(new
			{
				errortype = Keys.ErrorType,
				message = Keys.ErrorMessage,
				onestripjob = onestripjobs,
				onestripjobfields = onestripjobfields,
				labelId = labelId==null ? 0 : labelId,
				count = rowCount,
				barCodePrefix = oBarCodePrefix
			});
		}

		public IActionResult SetLableObjects(string jsonArray,int id)
		{

			var Setting = new JsonSerializerSettings();
			//Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
			var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonArray);

			var LabelObjectType = new DataTable();
			LabelObjectType.Columns.Add("Id", typeof(int));
			LabelObjectType.Columns.Add("OneStripJobsId", typeof(int));
			LabelObjectType.Columns.Add("FieldName", typeof(string));
			LabelObjectType.Columns.Add("Format", typeof(string));
			LabelObjectType.Columns.Add("Type", typeof(string));
			LabelObjectType.Columns.Add("XPos", typeof(double));
			LabelObjectType.Columns.Add("YPos", typeof(double));
			LabelObjectType.Columns.Add("BCStyle", typeof(int));
			LabelObjectType.Columns.Add("BCWidth", typeof(double));
			LabelObjectType.Columns.Add("BCHeight", typeof(double));
			LabelObjectType.Columns.Add("Order", typeof(int));
			LabelObjectType.Columns.Add("ForeColor", typeof(string));
			LabelObjectType.Columns.Add("BackColor", typeof(string));
			LabelObjectType.Columns.Add("FontSize", typeof(double));
			LabelObjectType.Columns.Add("FontName", typeof(string));
			LabelObjectType.Columns.Add("FontBold", typeof(bool));
			LabelObjectType.Columns.Add("FontItalic", typeof(bool));
			LabelObjectType.Columns.Add("FontUnderline", typeof(bool));
			LabelObjectType.Columns.Add("FontStrikeThru", typeof(bool));
			LabelObjectType.Columns.Add("FontTransparent", typeof(bool));
			LabelObjectType.Columns.Add("FontOrientation", typeof(int));
			LabelObjectType.Columns.Add("Alignment", typeof(int));
			LabelObjectType.Columns.Add("BCBarWidth", typeof(double));
			LabelObjectType.Columns.Add("BCDirection", typeof(int));
			LabelObjectType.Columns.Add("BCUPCNotches", typeof(int));
			LabelObjectType.Columns.Add("StartChar", typeof(int));
			LabelObjectType.Columns.Add("MaxLen", typeof(int));
			LabelObjectType.Columns.Add("SpecialFunctions", typeof(int));

			var OneStripJobsId = id;

			try
            {
				for (int i = 0; i <= jsonObject.Count; i += 1)
				{
					if (!string.IsNullOrEmpty(jsonObject[i.ToString()].ToString()))
					{
						string FieldName;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["FieldName"].ToString()))

							FieldName = jsonObject[i.ToString()]["FieldName"].ToString();
						else
							FieldName = null;

						string Format;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["Format"].ToString()))
							Format = jsonObject[i.ToString()]["Format"].ToString();
						else
							Format = null;

						var Type = jsonObject[i.ToString()]["Type"].ToString();

						var XPos = double.Parse(jsonObject[i.ToString()]["XPos"].ToString());
						var YPos = double.Parse(jsonObject[i.ToString()]["YPos"].ToString());

						string ForeColor = null;

						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["ForeColor"].ToString()))
							ForeColor = jsonObject[i.ToString()]["ForeColor"].ToString();

						string BackColor = null;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["BackColor"].ToString()))
							BackColor = jsonObject[i.ToString()]["BackColor"].ToString();

						Int32 BCStyle;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["BCStyle"].ToString()))

							BCStyle = int.Parse(jsonObject[i.ToString()]["BCStyle"].ToString());
						else
							BCStyle = 0;

						double BCWidth;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["BCWidth"].ToString()))
							BCWidth = double.Parse(jsonObject[i.ToString()]["BCWidth"].ToString());
						else
							BCWidth = 0;

						double BCHeight;

						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["BCHeight"].ToString()))
							BCHeight = double.Parse(jsonObject[i.ToString()]["BCHeight"].ToString());
						else
							BCHeight = 0;

						Int32 objID = 0;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["Id"].ToString()))

							objID = int.Parse(jsonObject[i.ToString()]["Id"].ToString());
						else
							objID = 0;

						double fontSize;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["FontSize"].ToString()))
							fontSize = double.Parse(jsonObject[i.ToString()]["FontSize"].ToString());
						else
							fontSize = 0;

						string fontName;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["FontName"].ToString()))
							fontName = jsonObject[i.ToString()]["FontName"].ToString();
						else
							fontName = null;

						var fontBold = false;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["FontBold"].ToString()))
							fontBold = bool.Parse(jsonObject[i.ToString()]["FontBold"].ToString());

						var fontItalic = false;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["FontItalic"].ToString()))
							fontItalic = bool.Parse(jsonObject[i.ToString()]["FontItalic"].ToString());

						var fontUnderline = false;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["FontUnderline"].ToString()))
							fontUnderline = bool.Parse(jsonObject[i.ToString()]["FontUnderline"].ToString());

						var fontStrike = false;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["FontStrikeThru"].ToString()))
							fontStrike = bool.Parse(jsonObject[i.ToString()]["FontStrikeThru"].ToString());

						var fontTrans = false;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["FontTransparent"].ToString()))
							fontTrans = bool.Parse(jsonObject[i.ToString()]["FontTransparent"].ToString());

						Int32 orientation;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["FontOrientation"].ToString()))
							orientation = int.Parse(jsonObject[i.ToString()]["FontOrientation"].ToString());
						else
							orientation = 0;

						Int32? align = null;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["Alignment"].ToString()))
							align = int.Parse(jsonObject[i.ToString()]["Alignment"].ToString());

						double BCBarWidth;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["BCBarWidth"].ToString()))
							BCBarWidth = double.Parse(jsonObject[i.ToString()]["BCBarWidth"].ToString());
						else
							BCBarWidth = 0;

						Int32 BCDirection;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["BCDirection"].ToString()))
							BCDirection = int.Parse(jsonObject[i.ToString()]["BCDirection"].ToString());
						else
							BCDirection = 0;

						Int32 BCUPCNotches;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["BCDirection"].ToString()))
							BCUPCNotches = int.Parse(jsonObject[i.ToString()]["BCUPCNotches"].ToString());
						else
							BCUPCNotches = 0;

						Int32 StartChar;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["StartChar"].ToString()))
							StartChar = int.Parse(jsonObject[i.ToString()]["StartChar"].ToString());
						else
							StartChar = 0;

						Int32 MaxLen;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["MaxLen"].ToString()))
							MaxLen = int.Parse(jsonObject[i.ToString()]["MaxLen"].ToString());
						else
							MaxLen = 0;

						Int32 SpecialFunctions = 0;
						if (!string.IsNullOrEmpty(((JObject)jsonObject[i.ToString()])["SpecialFunctions"].ToString()))
							SpecialFunctions = int.Parse(jsonObject[i.ToString()]["SpecialFunctions"].ToString());
						else
							SpecialFunctions = 0;

						var Order = int.Parse(jsonObject[i.ToString()]["Order"].ToString());

						LabelObjectType.Rows.Add(objID, OneStripJobsId, FieldName, Format, Type, XPos, YPos, BCStyle, BCWidth, BCHeight, Order, ForeColor, BackColor, fontSize, fontName, fontBold, fontItalic, fontUnderline, fontStrike, fontTrans, orientation, align, BCBarWidth, BCDirection, BCUPCNotches, StartChar, MaxLen, SpecialFunctions);
					}
				}
			}
			catch(Exception ex)
            {

            }
			

			_IDBManager.ConnectionString = Keys.get_GetDBConnectionString();
			_IDBManager.CreateParameters(2);
			_IDBManager.AddParameters(0, "LabelObjectType", LabelObjectType);
			_IDBManager.AddParameters(1, "OneStripJobsId", OneStripJobsId);

			var loutput = _IDBManager.ExecuteNonQuery(System.Data.CommandType.StoredProcedure, "SP_RMS_AddEditLabelObjectDetails");
			_IDBManager.Dispose();

			var pOneStripJobFields = _iOneStripJobsField.All().Where(x => x.OneStripJobsId == id);

			var oneStripJobFieldObject = JsonConvert.SerializeObject(pOneStripJobFields, Formatting.Indented, Setting);

			return Json(new
			{
				errortype = Keys.ErrorType,
				message = Keys.ErrorMessage,
				oneStripJobFieldObject = oneStripJobFieldObject
			});
		}

		public IActionResult GetAllLabelList()
        {
            var pOneStripJob = _iOneStripJob.All().Where(x => x.Inprint == 0);
            var Setting = new JsonSerializerSettings();
            Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
        var jsonObject = JsonConvert.SerializeObject(pOneStripJob, Formatting.Indented, Setting);

            return Json(pOneStripJob);
        }

		public ActionResult GetFormList()
		{
			var pOneStripForm = _iOneStripForms.All().Where(x => x.Inprint == 0);

			var Setting = new JsonSerializerSettings();
			Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
			var jsonObject = JsonConvert.SerializeObject(pOneStripForm, Formatting.Indented, Setting);

			return Json(jsonObject);
		}

		public ActionResult GetLabelDetails(string name)
		{
			var pOneStripJobs = _iOneStripJob.All().Where(x => x.Name.Trim().ToLower().Equals(name.Trim().ToLower())).FirstOrDefault();

			var jobsId = _iOneStripJob.All().Where(x => x.Name.Trim().ToLower().Equals(name.Trim().ToLower())).FirstOrDefault().Id;

			var pOneStripJobField = _iOneStripJobsField.All().Where(x => x.OneStripJobsId == jobsId);

			var oTable = _iTables.All().Where(x => x.TableName == pOneStripJobs.TableName).FirstOrDefault();

			var pOneStripForm = _iOneStripForms.All().Where(x => x.Id == pOneStripJobs.OneStripFormsId).FirstOrDefault();

			var oBarCodePrefix = "";
			var tableName = "";
			var rowCount = 0;

			if (oTable != null)
			{
				oBarCodePrefix = oTable.BarCodePrefix;
				tableName = pOneStripJobs.TableName;
				rowCount = GetCount(tableName);
			}

			var Setting = new JsonSerializerSettings();
			Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
			var onestripjobfields = JsonConvert.SerializeObject(pOneStripJobField, Formatting.Indented, Setting);
			var onestripjobs = JsonConvert.SerializeObject(pOneStripJobs, Formatting.Indented, Setting);
			var onestripform = JsonConvert.SerializeObject(pOneStripForm, Formatting.Indented, Setting);

			return Json(new
			{
				errortype = Keys.ErrorType,
				message = Keys.ErrorMessage,
				onestripjob = onestripjobs,
				onestripform = onestripform,
				barCodePrefix = oBarCodePrefix,
				count = rowCount,
				onestripjobfields = onestripjobfields
			});
		}

		public ActionResult DeleteLable(string name)
		{
			try
			{
				var pOneStripJob = _iOneStripJob.All().Where(x => x.Name.Trim().ToLower().Equals(name.Trim().ToLower())).FirstOrDefault();

				var jobsId = _iOneStripJob.All().Where(x => x.Name.Trim().ToLower().Equals(name.Trim().ToLower())).FirstOrDefault().Id;

				var pOneStripJobField = _iOneStripJobsField.All().Where(x => x.OneStripJobsId == jobsId);

				_iOneStripJob.Delete(pOneStripJob);

				_iOneStripJobsField.DeleteRange(pOneStripJobField);

				Keys.ErrorType = "s";
				Keys.ErrorMessage = Languages.get_Translation("msgLabelManagerCtrlLblDeletedSuccessfully");
			}
			catch (Exception)
			{
				Keys.ErrorType = "e";
				Keys.ErrorMessage = Keys.ErrorMessageJS();
			}

			return Json(new
			{
				errortype = Keys.ErrorType,
				message = Keys.ErrorMessage
			});
		}

		public ActionResult CreateSQLString(string tableName)
		{
			var pTable = _iTables.All().Where(x => x.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower()) && !string.IsNullOrEmpty(x.DBName.Trim().ToLower())).FirstOrDefault();
			DBManager dbManager = new DBManager();

			Databas pDatabaseEntity = null;
			if (pTable != null)
			{
				pDatabaseEntity = _iDatabas.Where(x => x.DBName.Trim().ToLower().Equals(pTable.DBName.Trim().ToLower())).FirstOrDefault();
				dbManager.ConnectionString = Keys.get_GetDBConnectionString(pDatabaseEntity);
			}
			else
				dbManager.ConnectionString = Keys.get_GetDBConnectionString(pDatabaseEntity);

			var sSql = "SELECT * FROM [" + tableName + "]";
			var loutput = dbManager.ExecuteDataSetWithSchema(System.Data.CommandType.Text, sSql);

			string pColumn = null;

			if (loutput.Tables[0].PrimaryKey.Length == 0)
			{
				for (int i = 0; i <= loutput.Tables[0].Columns.Count - 1; i += 1)
				{
					if (loutput.Tables[0].Columns[i].Caption.Trim().ToLower().Equals("id"))
						pColumn = loutput.Tables[0].Columns[i].Caption;

					if (pColumn == null & loutput.Tables[0].Columns.Count > 0)
						pColumn = loutput.Tables[0].Columns[0].Caption;
				}
			}
			else
				pColumn = loutput.Tables[0].PrimaryKey[0].ToString();

			string SQLString = null;

			if (pColumn == null)
				SQLString = "SELECT [" + tableName + "].* FROM [" + tableName + "]"; // WHERE " + tableName + "." + pColumn + " = '%ID%'"
			else
				SQLString = "SELECT [" + tableName + "].* FROM [" + tableName + "] WHERE " + tableName + "." + pColumn + " = '%ID%'";

			return Json(new
			{
				value = SQLString,
				errortype = Keys.ErrorType,
				message = Keys.ErrorMessage
			});
		}

		public int GetCount(string tableName)
		{
			var pTable = _iTables.All().Where(x => x.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower()) && !string.IsNullOrEmpty(x.DBName.Trim().ToLower())).FirstOrDefault();
			DBManager dbManager = new DBManager();

			Databas pDatabaseEntity = null;
			if (pTable != null)
			{
				pDatabaseEntity = _iDatabas.Where(x => x.DBName.Trim().ToLower().Equals(pTable.DBName.Trim().ToLower())).FirstOrDefault();
				dbManager.ConnectionString = Keys.get_GetDBConnectionString(pDatabaseEntity);
			}
			else
				dbManager.ConnectionString = Keys.get_GetDBConnectionString(pDatabaseEntity);

			var countQuery = "SELECT COUNT(1) FROM " + tableName;
			var loutput = dbManager.ExecuteScalar(System.Data.CommandType.Text, countQuery);
			return System.Convert.ToInt32(loutput);
		}

		public ActionResult GetNextRecord(int rowNo, string tableName, string SQL_String)
		{
			var pTable = _iTables.All().Where(x => x.TableName.Trim().ToLower().Equals(tableName.Trim().ToLower()) && !string.IsNullOrEmpty(x.DBName.Trim().ToLower())).FirstOrDefault();
			DBManager dbManager = new DBManager();

			Databas pDatabaseEntity = null;
			if (pTable != null)
			{
				pDatabaseEntity = _iDatabas.Where(x => x.DBName.Trim().ToLower().Equals(pTable.DBName.Trim().ToLower())).FirstOrDefault();
				dbManager.ConnectionString = Keys.get_GetDBConnectionString(pDatabaseEntity);
			}
			else
				dbManager.ConnectionString = Keys.get_GetDBConnectionString(pDatabaseEntity);

			string finalSQL = SQL_String.Split(new string[] { "WHERE" }, StringSplitOptions.None)[0];

			// Dim countQuery As String = "SELECT * FROM ( SELECT *, ROW_NUMBER() OVER (ORDER BY (SELECT '')) AS  RowNum FROM " + tableName + " ) T1 WHERE T1.RowNum = " + rowNo.ToString()

			string countQuery = "Select * from (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT '')) AS  RowNum, " + finalSQL.Substring(7) + ") T1 WHERE T1.RowNum = " + rowNo.ToString();

			DataSet loutput = null;
			try
			{
				loutput = dbManager.ExecuteDataSet(System.Data.CommandType.Text, countQuery);
			}
			catch (Exception)
			{
				loutput = null;
			}

			var Setting = new JsonSerializerSettings();
			Setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

			string rowData = null;
			if (loutput != null)
				rowData = JsonConvert.SerializeObject(loutput.Tables[0], Formatting.Indented, Setting);
			else
				rowData = "[]";

			return Json(new
			{
				errortype = Keys.ErrorType,
				message = Keys.ErrorMessage,
				rowdata = rowData
			});
		}

		public ActionResult SetAsDefault(int oneStripJobsId, int oneStripFormsId)
		{
			try
			{
				var oONeStripJobs = _iOneStripJob.All().Where(x => x.Id == oneStripJobsId).FirstOrDefault();
				var oOneStripForms = _iOneStripForms.All().Where(x => x.Id == oneStripFormsId).FirstOrDefault();

				oONeStripJobs.OneStripFormsId = oneStripFormsId;
				oONeStripJobs.LabelHeight = oOneStripForms.LabelHeight;
				oONeStripJobs.LabelWidth = oOneStripForms.LabelWidth;

				_iOneStripJob.Update(oONeStripJobs);
				Keys.ErrorType = "s";
				Keys.ErrorMessage = "";
			}
			catch (Exception)
			{
				Keys.ErrorType = "e";
				Keys.ErrorMessage = "";
			}

			return Json(new
			{
				errortype = Keys.ErrorType,
				message = Keys.ErrorMessage
			});
		}
	}
}