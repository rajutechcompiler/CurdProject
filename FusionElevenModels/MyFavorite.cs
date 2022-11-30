

// MY FAVORITE MODEL RETURN DOM OBJECT
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

namespace TabFusionRMS.WebCS.FusionElevenModels
{

    public class MyFavorite : BaseModel
    {
        public MyFavorite()
        {
            ListAddtoFavorite = new List<FavoritedropdownList>();
        }
        public int SaveCriteriaId
        {
            get;
            set;
        }
        public List<FavoritedropdownList> ListAddtoFavorite
        {
            get;
            set;
        }
        public string placeholder
        {
            get;
            set;
        }
        public string label
        {
            get;
            set;
        }
        public class UiParams
        {
            public int ViewId
            {
                get;
                set;
            }
            public int FavCriteriaid
            {
                get;
                set;
            }
            public string FavCriteriaType
            {
                get;
                set;
            }
            public string NewFavoriteName
            {
                get;
                set;
            }
            public List<UirowsList> RowsSelected
            {
                get;
                set;
            }
            public int pageNum
            {
                get;
                set;
            }
        }
        public class UirowsList
        {
            public string rowKeys
            {
                get;
                set;
            }
        }
        public class FavoritedropdownList
        {
            public string text
            {
                get;
                set;
            }
            public string value
            {
                get;
                set;
            }
        }
    }
   
    public class FavoriteRecordReqModel
        {
        public MyFavorite.UiParams paramss { get; set; }
        public List<MyFavorite.UirowsList> recordkeys { get; set; }
    }
    public class ReturnFavoritTogridReqModel
    {
        public MyFavorite.UiParams paramss { get; set; }

        public List<searchQueryModel> searchQuery { get; set; }
    }

    public class DeleteFavoriteRecordReqModel
    {
        public MyFavorite.UiParams paramss { get; set; }
    }

    }
