using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Hosting.Internal;
//using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Smead.RecordsManagement;
using Smead.Security;
using TabFusionRMS.Resource;

namespace TabFusionRMS.WebCS
{

    // TABQUIK API RETURN NEW PAGE, AND DOM OBJECT
    public class TabquikApi : BaseModel
    {
        public TabquikApi(Passport passport, Parameters @params, string rowsSelected)
        {
            _passport = passport;
            _params = @params;
            _rowsSelected = rowsSelected;
        }

        private Parameters _params;
        private string _rowsSelected;
        private string CustomerID;
        private string ContactID;
        public string ErrorMsg { get; set; }
        public string srcUrl { get; set; }
        public string DataTQ { get; set; }
        private DataTable dtData = new DataTable("LabelData");
        private DataTable dtJobs = new DataTable();
        private DataTable dtFormat = new DataTable("Formats");
        private DataTable dtClone;

        public void GetLicense()
        {
            // get the license
            var key = Navigation.GetSetting("TABQUIK", "Key", _passport).Split('-');

            if (key is not null)
            {
                if (key.Length > 1)
                {
                    CustomerID = key[0];
                    ContactID = key[1];
                }
                else
                {
                    CustomerID = key[0];
                }
            }
        }
        public void GetTabquikData(IWebHostEnvironment webHostEnvironment)
        {
            string inClause = System.String.Format(" IN ({0})", _rowsSelected);

            using (var conn = _passport.Connection())
            {
                using (var cmd = new SqlCommand("SELECT * FROM OneStripJobs WHERE TableName = @tableName AND InPrint = 5", conn))
                {
                    cmd.Parameters.AddWithValue("@TableName", _params.TableName);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtJobs);
                    }
                }

                if (dtJobs.Rows.Count == 0)
                {
                    ErrorMsg = Languages.get_Translation("msgNoLabelsIntegrated");
                    return;
                }
                var rep = dtJobs.Rows[0]["SQLString"].ToString().Replace("= %ID%", inClause);
                using (var cmd = new SqlCommand(rep, conn))
                {
                    cmd.CommandText = cmd.CommandText.Replace("=%ID%", inClause);
                    cmd.CommandText = cmd.CommandText.Replace("='%ID%'", inClause);
                    cmd.CommandText = cmd.CommandText.Replace("= '%ID%'", inClause);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtData);
                    }
                }

                if (dtData.Rows.Count == 0)
                {
                    ErrorMsg = Languages.get_Translation("msgSQLNoDataReturns");
                    return;
                }

                using (var cmd = new SqlCommand("SELECT * FROM OneStripJobFields WHERE OneStripJobsID = @JobID", conn))
                {
                    cmd.Parameters.AddWithValue("@JobID", dtJobs.AsEnumerable().ElementAtOrDefault(0)["Id"]);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtFormat);
                    }
                }

                if (dtFormat.Rows.Count == 0)
                {
                    ErrorMsg = Languages.get_Translation("msgEvtbtnTabQuick_ClickNoLanFrmtFields");
                    return;
                }

                dtClone = dtData.Clone();

                foreach (DataColumn col in dtClone.Columns)
                    col.DataType = typeof(string);

                foreach (DataRow row in dtData.Rows)
                    dtClone.ImportRow(row);
            }

            dtClone.WriteXml( webHostEnvironment.ContentRootPath + "LabelData" + @"\" + CustomerID + ContactID + ".xml", XmlWriteMode.WriteSchema, true);
        }
        
        public void CreateSecureLink(IWebHostEnvironment webHostEnvironment)
        {
            string baseUrl = "https://www.tabquik.com";
            string baseMVCURL = "https://mvc.tabquik.com";
            string[] signIn;

            if (System.Configuration.ConfigurationManager.AppSettings["TabQuikBase"] is not null)
            {
                baseUrl = System.Configuration.ConfigurationManager.AppSettings["TabQuikBase"];
            }
            if (System.Configuration.ConfigurationManager.AppSettings["TabQuikMVCBase"] is not null)
            {
                baseMVCURL = System.Configuration.ConfigurationManager.AppSettings["TabQuikMVCBase"];
            }

            try
            {
                signIn = Navigation.GetSetting("TABQUIK", "Key", _passport.Connection()).Split('-');
            }
            catch
            {
                ErrorMsg = "No license";
                return;
            }

            string isMvc = string.Empty;
            var webClient = new WebClient();

            try
            {
                isMvc = webClient.DownloadString(string.Format("{0}/TabQuik/IsMVCUser?contactID={1}", baseMVCURL, signIn[1]));
            }
            catch (Exception ex)
            {
                var model = new ErrorBaseModel();
                DataErrorHandler.ErrorHandler(ex, model, 0, _passport.DatabaseName);
            }
            // if is MVC
            if (string.Compare(isMvc, "True", true) == 0)
            {
                string data;

                try
                {
                    string line = string.Empty;
                    var dtXML = new DataTable();

                    dtXML.ReadXml(webHostEnvironment.ContentRootPath + "LabelData" + @"\" + CustomerID + ContactID + ".xml");

                    foreach (DataRow row in dtXML.Rows)
                    {
                        for (int x = 0, loopTo = row.Table.Columns.Count - 1; x <= loopTo; x++)
                            line += row[x].ToString().Replace("~", "") + "~";

                        line = line.Substring(0, line.Length - 1) + "*!*";
                    }

                    data = line.Substring(0, line.Length - 3);
                    data = data.Replace(@"\", @"\\");
                    data = data.Replace("'", @"\'");
                }
                catch (Exception ex)
                {
                    data = "Cannot read file";
                }

                DataTQ = data;
                var tqEncrypt = new TabQUIKEncrypt();
                srcUrl = baseMVCURL + "/?k=" + System.Net.WebUtility.HtmlEncode(tqEncrypt.Encrypt(CustomerID + "," + ContactID + ",1"));
            }
            else
            {
                srcUrl = baseUrl + "/Design.aspx?t1=" + (Convert.ToDouble(signIn[0]) * 6654d).ToString() + "&t2=" + (Convert.ToDouble(signIn[0]) * Convert.ToDouble(signIn[1])).ToString();
            }
        }
    }

    public class TabQuikRowsSelected
    {
        public string Id { get; set; }
    }
}