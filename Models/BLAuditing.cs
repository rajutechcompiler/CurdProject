using System.Collections.Generic;
using System.Data;
using TabFusionRMS.DataBaseManagerVB;

namespace TabFusionRMS.WebCS
{

    public sealed class BLAuditing
    {

        public static List<int> GetChildTableIds(string pTableName, IDBManager pIDBManager, HttpContext httpContext)
        {
            var lTableIds = new List<int>();
            pIDBManager.ConnectionString = Keys.get_GetDBConnectionString();
            pIDBManager.ExecuteReader(CommandType.Text, "SELECT * FROM dbo.FNGetChildTables('" + pTableName + "');");
            while (pIDBManager.DataReader.Read())
            {
                if (!pIDBManager.DataReader.IsDBNull(pIDBManager.DataReader.GetOrdinal("TableId")))
                {
                    lTableIds.Add(pIDBManager.DataReader.GetInt32(pIDBManager.DataReader.GetOrdinal("TableId")));
                }
            }
            pIDBManager.Dispose();
            return lTableIds;
        }

    }
}