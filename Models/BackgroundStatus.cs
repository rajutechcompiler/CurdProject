using System.Data.SqlClient;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic
using Smead.RecordsManagement;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
using Smead.Security;

namespace TabFusionRMS.WebCS
{
    public sealed partial class BackgroundStatus
    {
        public static IRepository<SLServiceTask> s_SLServiceTask { get; set; }

        //public static bool InsertDataSecond(ref string errorMessage, string description, int taskType, int userId, int recordCount, HttpContext httpContext, string path = null, string exportReportPath = null, string DestTableName = null, string DestId = null, string DueBackDate = null, bool Reconciliation = false)
        //{
        //    bool IsSuccess = false;
        //    var passport = ContextService.GetObjectFromJson<Passport>("passport", httpContext);
        //    var levelManager = ContextService.GetObjectFromJson<LevelManager>("LevelManager", httpContext);
        //    IRepository<SLServiceTask> s_SLServiceTask = new Repositories<SLServiceTask>();
        //    IRepository<SLServiceTaskItem> s_SLServiceTaskItem = new Repositories<SLServiceTaskItem>();
        //    var selectedItemList = levelManager.ActiveLevel.SelectedItems;
        //    string oTableName = levelManager.ActiveLevel.Parameters.TableName;
        //    s_SLServiceTask.BeginTransaction();
        //    var slServiceTask = new SLServiceTask();
        //    try
        //    {
        //        try
        //        {
        //            slServiceTask.Type = description;
        //            slServiceTask.EMailAddress = Navigation.GetUserEmail(passport);
        //            slServiceTask.TaskType = taskType;
        //            slServiceTask.UserName = Keys.CurrentUserName.ToString();
        //            slServiceTask.UserId = userId;
        //            slServiceTask.Status = Enum.GetName(typeof(Enums.BackgroundTaskStatus), 1);
        //            slServiceTask.RecordCount = recordCount;
        //            slServiceTask.ViewId = levelManager.ActiveLevel.Parameters.ViewId;
        //            slServiceTask.CreateDate = DateTime.Now;
        //            slServiceTask.DestinationTableName = DestTableName;
        //            slServiceTask.DestinationTableId = DestId;
        //            slServiceTask.IsNotification = false;
        //            if (!string.IsNullOrEmpty(DueBackDate))
        //            {
        //                slServiceTask.DueBackDate = DateTime.Parse(DueBackDate);
        //            }
        //            slServiceTask.Reconciliation = Reconciliation;
        //            if (taskType is (int)Enums.BackgroundTaskType.Export)
        //            {
        //                slServiceTask.DownloadLocation = path;
        //                slServiceTask.ReportLocation = exportReportPath;
        //            }
        //            else
        //            {
        //                slServiceTask.ReportLocation = path;
        //            }
        //            s_SLServiceTask.Add(slServiceTask);
        //        }
        //        catch (Exception ex)
        //        {
        //            s_SLServiceTask.RollBackTransaction();
        //            errorMessage = ex.InnerException.Message;
        //            IsSuccess = false;
        //        }
        //        var result = slServiceTask.Id;
        //        if (result == slServiceTask.Id)
        //        {
        //            s_SLServiceTaskItem.BeginTransaction();
        //            var slSLServiceTaskItem = new SLServiceTaskItem();
        //            foreach (var oItem in selectedItemList)
        //            {
        //                slSLServiceTaskItem.TableId = oItem;
        //                slSLServiceTaskItem.SLServiceTaskId = result;
        //                slSLServiceTaskItem.TableName = oTableName;
        //                s_SLServiceTaskItem.Add(slSLServiceTaskItem);
        //            }
        //            s_SLServiceTask.CommitTransaction();
        //            s_SLServiceTaskItem.CommitTransaction();
        //            IsSuccess = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        s_SLServiceTask.RollBackTransaction();
        //        s_SLServiceTaskItem.RollBackTransaction();
        //        errorMessage = ex.InnerException.Message;
        //        IsSuccess = false;
        //    }
        //    return IsSuccess;
        //}


        //public static bool InsertData(ref string errorMessage, string description, int taskType, int userId, int recordCount, string sql, bool selectAllData, string path = null, string exportReportPath = null, string DestTableName = null, string DestId = null, string DueBackDate = null, bool Reconciliation = false)
        //{
        //    bool IsSuccess = true;
        //    IRepository<SLServiceTask> s_SLServiceTask = new Repositories<SLServiceTask>();
        //    IRepository<SLServiceTaskItem> s_SLServiceTaskItem = new Repositories<SLServiceTaskItem>();
        //    var levelManager = ContextService.GetObjectFromJson<LevelManager>("LevelManager");
        //    var passport = ContextService.GetObjectFromJson<Passport>("passport");
        //    string tableName = levelManager.ActiveLevel.Parameters.TableName;
        //    var slServiceTask = new SLServiceTask();

        //    s_SLServiceTask.BeginTransaction();

        //    try
        //    {
        //        try
        //        {
        //            slServiceTask.Type = description;
        //            slServiceTask.EMailAddress = Navigation.GetUserEmail(passport);
        //            slServiceTask.TaskType = taskType;
        //            slServiceTask.UserName = Keys.CurrentUserName.ToString();
        //            slServiceTask.UserId = userId;
        //            slServiceTask.Status = Enum.GetName(typeof(Enums.BackgroundTaskStatus), 1);
        //            slServiceTask.RecordCount = recordCount;
        //            slServiceTask.ViewId = levelManager.ActiveLevel.Parameters.ViewId;
        //            slServiceTask.CreateDate = DateTime.Now;
        //            slServiceTask.DestinationTableName = DestTableName;
        //            slServiceTask.DestinationTableId = DestId;
        //            slServiceTask.IsNotification = false;
        //            slServiceTask.Reconciliation = Reconciliation;
        //            if (!string.IsNullOrEmpty(DueBackDate))
        //                slServiceTask.DueBackDate = DateTime.Parse(DueBackDate);

        //            if (taskType is (int)Enums.BackgroundTaskInDetail.ExportCSV or (int)Enums.BackgroundTaskInDetail.ExportTXT)
        //            {
        //                slServiceTask.DownloadLocation = path;
        //                slServiceTask.ReportLocation = exportReportPath;
        //            }
        //            else
        //            {
        //                slServiceTask.ReportLocation = path;
        //            }

        //            s_SLServiceTask.Add(slServiceTask);
        //        }
        //        catch (Exception ex)
        //        {
        //            s_SLServiceTask.RollBackTransaction();
        //            errorMessage = ex.InnerException.Message;
        //            IsSuccess = false;
        //        }

        //        if (IsSuccess && slServiceTask.Id != 0)
        //        {
        //            var returnVal = default(int);

        //            try
        //            {
        //                var sqlQuery = new StringBuilder();
        //                Parameters @params = levelManager.ActiveLevel.Parameters;

        //                using (SqlConnection conn = passport.StaticConnection())
        //                {
        //                    if (selectAllData)
        //                    {
        //                        string sqlViewName = Query.GetSQLViewName(@params, false);
        //                        int indexOf = sql.ToLower().IndexOf(string.Format(" from {0} ", sqlViewName));
        //                        if (indexOf < 0)
        //                            indexOf = sql.ToLower().IndexOf(string.Format(" from [{0}] ", sqlViewName));
        //                        if (indexOf < 0)
        //                            indexOf = sql.ToLower().IndexOf(string.Format(" from {0}", sqlViewName));
        //                        if (indexOf < 0)
        //                            indexOf = sql.ToLower().IndexOf(string.Format(" from [{0}]", sqlViewName));
        //                        if (indexOf < 0)
        //                            indexOf = 0;

        //                        sql = string.Format("SELECT {0}, '{1}', [{2}] {3}", slServiceTask.Id, tableName, @params.KeyField, sql.Substring(indexOf));
        //                        sqlQuery.Append(string.Format("INSERT INTO SLServiceTaskItems ([SLServiceTaskId], [TableName], [TableId]) {0}", sql));
        //                    }
        //                    else
        //                    {
        //                        string value = string.Empty;

        //                        if (@params.IdFieldDataType.FullName.ToLower().Contains("system.int"))
        //                        {
        //                            value = string.Join(",", levelManager.ActiveLevel.SelectedItems);
        //                        }
        //                        else if (@params.IdFieldDataType.FullName.ToLower().Contains("system.string"))
        //                        {
        //                            value = "'" + string.Join("','", levelManager.ActiveLevel.SelectedItems) + "'";
        //                        }

        //                        sqlQuery.Append(string.Format("INSERT INTO SLServiceTaskItems ([SLServiceTaskId], [TableName], [TableId]) SELECT {0}, '{1}', {2} FROM {3} WHERE {4} IN ({5})", slServiceTask.Id, tableName, @params.KeyField, tableName, @params.KeyField, value));
        //                    }

        //                    using (var cmd = new System.Data.SqlClient.SqlCommand(sqlQuery.ToString(), conn))
        //                    {
        //                        cmd.CommandTimeout = 20 * 60; // 20 minutes 
        //                        returnVal = cmd.ExecuteNonQuery();
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                s_SLServiceTask.RollBackTransaction();
        //                errorMessage = ex.Message;
        //                IsSuccess = false;
        //            }

        //            if (returnVal <= 0)
        //            {
        //                if (IsSuccess == true)
        //                    s_SLServiceTask.RollBackTransaction();
        //                if (string.IsNullOrWhiteSpace(errorMessage))
        //                    errorMessage = "Error";
        //                IsSuccess = false;
        //            }
        //            else
        //            {
        //                s_SLServiceTask.CommitTransaction();
        //                IsSuccess = true;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (IsSuccess == true)
        //        {
        //            s_SLServiceTask.RollBackTransaction();
        //            s_SLServiceTaskItem.RollBackTransaction();
        //        }

        //        errorMessage = ex.Message;
        //        IsSuccess = false;
        //    }

        //    return IsSuccess;
        //}

        public static object ChangeNotification(int userId)
        {
            var backgroundStatusNotification = s_SLServiceTask.All().Where(x => x.UserId == userId && x.IsNotification == true).ToList();
            if (backgroundStatusNotification.Count() > 0)
            {
                s_SLServiceTask.BeginTransaction();
                try
                {
                    var slServiceTask = new SLServiceTask();
                    foreach (var item in backgroundStatusNotification)
                    {
                        item.IsNotification = false;
                        s_SLServiceTask.Update(item);
                    }
                    s_SLServiceTask.CommitTransaction();
                }
                catch (Exception)
                {
                    s_SLServiceTask.RollBackTransaction();
                }
            }

            return "True";
        }

        public static object ChangeNotification(int userId, IRepository<SLServiceTask> _s_SLServiceTask)
        {
            s_SLServiceTask = _s_SLServiceTask;
            return ChangeNotification(userId);
        }

        // Written for MVC model Moti Mashiah (since the data processing model written specific for aspx objec and controller MVC can't use it).
        public static bool InsertData(DataProcessingModel prop, string rowQuery, Passport passport, HttpContext httpContext)
        {
            bool IsSuccess = true;
            IRepository<SLServiceTask> s_SLServiceTask = new Repositories<SLServiceTask>();
            IRepository<SLServiceTaskItem> s_SLServiceTaskItem = new Repositories<SLServiceTaskItem>();
            var slServiceTask = new SLServiceTask();

            s_SLServiceTask.BeginTransaction();
            
            try
            {
                try
                {
                    slServiceTask.Type = prop.FileName;
                    slServiceTask.EMailAddress = Navigation.GetUserEmail(passport);
                    slServiceTask.TaskType = prop.TaskType;
                    slServiceTask.UserName = Keys.CurrentUserName.ToString();
                    slServiceTask.UserId = passport.UserId;
                    slServiceTask.Status = Enum.GetName(typeof(Enums.BackgroundTaskStatus), 1);
                    slServiceTask.RecordCount = prop.RecordCount;
                    slServiceTask.ViewId = prop.viewId;
                    slServiceTask.CreateDate = DateTime.Now;
                    slServiceTask.DestinationTableName = prop.DestinationTableName;
                    slServiceTask.DestinationTableId = prop.DestinationTableId;
                    slServiceTask.IsNotification = false;
                    slServiceTask.Reconciliation = prop.Reconciliation;
                    if (!string.IsNullOrEmpty(prop.DueBackDate))
                        slServiceTask.DueBackDate = DateTime.Parse(prop.DueBackDate);

                    if (prop.TaskType is (int)Enums.BackgroundTaskInDetail.ExportCSV or (int)Enums.BackgroundTaskInDetail.ExportTXT)
                    {
                        slServiceTask.DownloadLocation = prop.Path;
                        slServiceTask.ReportLocation = prop.ExportReportPath;
                    }
                    else
                    {
                        slServiceTask.ReportLocation = prop.Path;
                    }

                    s_SLServiceTask.Add(slServiceTask);
                }
                catch (Exception ex)
                {
                    s_SLServiceTask.RollBackTransaction();
                    prop.ErrorMessage = ex.Message;
                    if (ex.InnerException is not null)
                        prop.ErrorMessage += string.Format("{0}InnerException: {1}", Constants.vbCrLf, ex.InnerException.Message);
                    IsSuccess = false;
                    throw;
                }

                if (IsSuccess && slServiceTask.Id != 0)
                {
                    int returnVal;

                    try
                    {
                        
                        var @params = new Parameters(prop.viewId, passport);

                        using (SqlConnection conn = passport.Connection())
                        {
                            string sqlQuery = string.Empty;
                            string selectSql = string.Format("SELECT {0}, '{1}', [{2}]", slServiceTask.Id, @params.TableName, @params.KeyField);

                            if (prop.IsSelectAllData)
                            {
                                if (string.IsNullOrEmpty(rowQuery))
                                {
                                    string sqlViewName = Query.GetSQLViewName(@params, false);
                                    sqlQuery = string.Format("INSERT INTO [SLServiceTaskItems] ([SLServiceTaskId], [TableName], [TableId]) {0} FROM [{1}]", selectSql, sqlViewName);
                                }
                                else
                                {
                                    int fromIndex = rowQuery.ToUpper().IndexOf(" FROM ");

                                    if (fromIndex > 0)
                                    {
                                        sqlQuery = string.Format("INSERT INTO [SLServiceTaskItems] ([SLServiceTaskId], [TableName], [TableId]) {0} {1}", selectSql, rowQuery.Substring(fromIndex));
                                    }
                                    else
                                    {
                                        string sqlViewName = Query.GetSQLViewName(@params, false);
                                        sqlQuery = string.Format("INSERT INTO [SLServiceTaskItems] ([SLServiceTaskId], [TableName], [TableId]) {0} FROM [{1}]", selectSql, sqlViewName);
                                    }
                                }
                            }
                            else
                            {
                                string value = string.Empty;
                                string sqlViewName = Query.GetSQLViewName(@params, false);

                                if (@params.IdFieldDataType.FullName.ToLower().Contains("system.int"))
                                {
                                    value = string.Join(",", prop.ListofselectedIds);
                                }
                                else if (@params.IdFieldDataType.FullName.ToLower().Contains("system.string"))
                                {
                                    value = string.Format("'{0}'", string.Join("','", prop.ListofselectedIds));
                                }

                                sqlQuery = string.Format("INSERT INTO [SLServiceTaskItems] ([SLServiceTaskId], [TableName], [TableId]) {0} FROM [{1}] WHERE [{2}] IN ({3})", selectSql, sqlViewName, @params.KeyField, value);
                            }

                            using (var cmd = new SqlCommand(sqlQuery, conn))
                            {
                                cmd.CommandTimeout = 20 * 60; // 20 minutes 
                                returnVal = cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        s_SLServiceTask.RollBackTransaction();
                        prop.ErrorMessage = ex.Message;
                        IsSuccess = false;
                        throw;
                    }

                    if (returnVal <= 0)
                    {
                        if (IsSuccess)
                            s_SLServiceTask.RollBackTransaction();
                        if (string.IsNullOrWhiteSpace(prop.ErrorMessage))
                            prop.ErrorMessage = "Error";
                        IsSuccess = false;
                    }
                    else
                    {
                        s_SLServiceTask.CommitTransaction();
                        IsSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsSuccess)
                {
                    s_SLServiceTask.RollBackTransaction();
                    s_SLServiceTaskItem.RollBackTransaction();

                }
                prop.ErrorMessage = ex.Message;
                IsSuccess = false;
                throw;
            }

            return IsSuccess;
        }
    }
}
