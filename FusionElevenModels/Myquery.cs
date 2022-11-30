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
using Smead.Security;
using TabFusionRMS.Models;
using TabFusionRMS.RepositoryVB;
// MY QUERY MODEL RETURN DOM OBJECT
namespace TabFusionRMS.WebCS.FusionElevenModels
{
    public class Myquery : BaseModel
    {
        public Myquery(Passport passport, Myquery.queryList @params)
        {
            this._passport = passport;
            this.Params = @params;
        }
        public Myquery(Passport passport, queryList @params, List<queryList> list)
        {
            this._passport = passport;
            this.list = new List<queryList>();
            this.list = list;
            this.Params = @params;
        }
        private List<queryList> list
        {
            get;
            set;
        }
        private queryList Params
        {
            get;
            set;
        }
        public bool isNameExist = false;
        public string uiparam
        {
            get;
            set;
        }
        private bool CheckIfQueryNameExist(string typecheck)
        {
            IRepository<s_SavedCriteria> s_SavedCriteria;
            s_SavedCriteria = new RepositoryVB.Repositories<s_SavedCriteria>();
            if (typecheck == "Insert")
            {
                var check = s_SavedCriteria.Where(x => x.SavedName == this.Params.SaveName & x.UserId == this._passport.UserId).FirstOrDefault();
                if (check == null)
                    return false;
            }

            return true;
        }
        public void DeleteQuery()
        {
            this.Msg = "success";
            IRepository<s_SavedCriteria> s_SavedCriteria;
            IRepository<s_SavedChildrenQuery> s_SavedChildrenQuery;
            s_SavedCriteria = new RepositoryVB.Repositories<s_SavedCriteria>();
            s_SavedChildrenQuery = new RepositoryVB.Repositories<s_SavedChildrenQuery>();
            try
            {
                var delparent = s_SavedCriteria.Where(x => x.Id == this.Params.SavedCriteriaid & x.UserId == this._passport.UserId).FirstOrDefault();
                s_SavedCriteria.Delete(delparent);
                var delchild = s_SavedChildrenQuery.Where(x => x.Id == this.Params.SavedCriteriaid).ToList();
                s_SavedChildrenQuery.DeleteRange(delchild);
            }
            catch (Exception ex)
            {
                this.Msg = ex.Message;
            }
        }
        public void InsertNewQuery()
        {
            if (this.CheckIfQueryNameExist("Insert"))
            {
                this.isNameExist = true;
                return;
            }
            IRepository<s_SavedCriteria> s_SavedCriteria;
            IRepository<s_SavedChildrenQuery> s_SavedChildrenQuery;
            s_SavedCriteria = new RepositoryVB.Repositories<s_SavedCriteria>();
            s_SavedChildrenQuery = new RepositoryVB.Repositories<s_SavedChildrenQuery>();
            this.Msg = "success";

            try
            {
                s_SavedCriteria.BeginTransaction();

                // first save in parent table
                s_SavedCriteria savedCriteria = new s_SavedCriteria();
                savedCriteria.SavedName = this.Params.SaveName;
                savedCriteria.SavedType = (int)Enums.SavedType.Query;
                savedCriteria.UserId = this._passport.UserId;
                savedCriteria.ViewId = this.Params.ViewId;
                s_SavedCriteria.Add(savedCriteria);
                // after parent created check if the insert saved.
                var result = s_SavedCriteria.Where(x => x.SavedName == this.Params.SaveName & x.UserId == this._passport.UserId).FirstOrDefault();
                // insert to child
                if (result != null)
                {
                    s_SavedChildrenQuery.BeginTransaction();
                    s_SavedChildrenQuery savedChildrenQuery = new s_SavedChildrenQuery();
                    foreach (queryList item in list)
                    {
                        savedChildrenQuery.SavedCriteriaId = result.Id;
                        savedChildrenQuery.Operator = item.operators;
                        savedChildrenQuery.ColumnName = item.columnName;
                        savedChildrenQuery.CriteriaValue = item.values == null ? "" : item.values;
                        s_SavedChildrenQuery.Add(savedChildrenQuery);
                    }
                    s_SavedChildrenQuery.CommitTransaction();
                }
                s_SavedCriteria.CommitTransaction();
                // for Ui return
                this.uiparam = string.Format("{0}_{1}_{2}", this.Params.ViewId, result.Id, 0);
            }
            catch (Exception ex)
            {
                this.Msg = ex.Message;
                s_SavedChildrenQuery.RollBackTransaction();
                s_SavedCriteria.RollBackTransaction();
            }
        }
        public void UpdateQuery()
        {
            IRepository<s_SavedCriteria> s_SavedCriteria;
            IRepository<s_SavedChildrenQuery> s_SavedChildrenQuery;
            s_SavedCriteria = new RepositoryVB.Repositories<s_SavedCriteria>();
            s_SavedChildrenQuery = new RepositoryVB.Repositories<s_SavedChildrenQuery>();
            var savedCriteria = s_SavedCriteria.Where(a => a.Id == this.Params.SavedCriteriaid).FirstOrDefault();
            // Dim savedCriteria As New s_SavedCriteria
            savedCriteria.SavedName = this.Params.SaveName;
            savedCriteria.SavedType = (int)Enums.SavedType.Query;
            savedCriteria.UserId = this._passport.UserId;
            savedCriteria.ViewId = this.Params.ViewId;
            s_SavedCriteria.Update(savedCriteria);
            var savedChildrenQuery = s_SavedChildrenQuery.Where(a => a.SavedCriteriaId == this.Params.SavedCriteriaid).ToList();
            // Dim savedChildrenQuery As New s_SavedChildrenQuery
            int index = 0;
            foreach (var prop in savedChildrenQuery)
            {
                queryList item = list[index];
                prop.SavedCriteriaId = Params.SavedCriteriaid;
                prop.Operator = item.operators;
                prop.ColumnName = item.columnName;
                prop.CriteriaValue = item.values == null ? "" : item.values;
                s_SavedChildrenQuery.Update(prop);
                index += 1;
            }
        }
        public class queryList
        {
            public string operators
            {
                get;
                set;
            }
            public string columnName
            {
                get;
                set;
            }
            public string values
            {
                get;
                set;
            }
            public string SaveName
            {
                get;
                set;
            }
            public int ViewId
            {
                get;
                set;
            }
            public int SavedCriteriaid
            {
                get;
                set;
            }
            public int type
            {
                get;
                set;
            }
            public string ColumnType
            {
                get;
                set;
            }
        }


       
        }
    public class SaveNewUpdateDeleteQueryReqModel
    {
        public Myquery.queryList paramss { get; set; }
        public List<Myquery.queryList> Querylist { get; set; }

    }
}