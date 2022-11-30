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
 
namespace TabFusionRMS.WebCS
{
    public class ImportFavoriteModel : BaseModel
    {
        public ImportFavoriteModel()
        {
            ListOfFieldName = new List<ListOfFieldName>();
            ListOfcolumns = new List<string>();
            Targetfileds = new List<ListOfFieldName>();
            lsRecExists = new List<string>();
            lsRecDuplicate = new List<string>();
            lsRecNotExists = new List<string>();
            lsRecAlreadyImported = new List<string>();
            lstcoldata = new List<string>();
        }
        public List<ImDropdownprops> DropDownlist
        {
            get;
            set;
        }
        public List<ListOfFieldName> ListOfFieldName
        {
            get;
            set;
        }
        public bool isfieldsExist
        {
            get;
            set;
        }
        public List<string> ListOfcolumns
        {
            get;
            set;
        }
        public bool isFilesupported
        {
            get;
            set;
        }
        public string UserMsg
        {
            get;
            set;
        }
        public string sourceFile
        {
            get;
            set;
        }
        public bool isRightColumnChk
        {
            get;
            set;
        }
        public bool isLeftColumnChk
        {
            get;
            set;
        }
        public string favoritListSelectorid
        {
            get;
            set;
        }
        public bool chk1RowFieldNames
        {
            get;
            set;
        }
        public List<ListOfFieldName> Targetfileds
        {
            get;
            set;
        }
        public string ColumnSelect
        {
            get;
            set;
        }
        public bool isValidate
        {
            get;
            set;
        }
        public int ViewId
        {
            get;
            set;
        }
        public bool IscolString
        {
            get;
            set;
        }
        public string AReportUrl
        {
            get;
            set;
        }
        public List<string> lsRecExists
        {
            get;
            set;
        }
        public List<string> lsRecNotExists
        {
            get;
            set;
        }
        public List<string> lsRecDuplicate
        {
            get;
            set;
        }
        public List<string> lsRecAlreadyImported
        {
            get;
            set;
        }
        public List<string> lstcoldata
        {
            get;
            set;
        }
        public string errorType
        {
            get;
            set;
        }
    }

    public class ImDropdownprops
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

    public class ListOfFieldName
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
