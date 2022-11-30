using Microsoft.VisualBasic;

namespace TabFusionRMS
{
    public class FoundBarCode
    {

        private Models.Table moTable;
        private string msTypedText;
        private string msId;
        private string msUserLinkId;
        private string msUserDisplay;
        private bool mbHaveOut;
        private bool mbIsOut;
        private bool mbIsActive;
        private string msEmailAddress;
        private string msDescription;
        private int mlDueBackDays;

        public new string Id
        {
            get
            {
                return msId;
            }
            set
            {
                msId = Strings.Trim(value);
            }
        }

        public Models.Table oTable
        {
            get
            {
                return moTable;
            }
            set
            {
                moTable = value;
            }
        }

        public string TypedText
        {
            get
            {
                return msTypedText;
            }
            set
            {
                msTypedText = Strings.Trim(value);
            }
        }

        public string UserLinkId
        {
            get
            {
                return msUserLinkId;
            }
            set
            {
                msUserLinkId = Strings.Trim(value);
            }
        }

        public string UserDisplay
        {
            get
            {
                return msUserDisplay;
            }
            set
            {
                msUserDisplay = Strings.Trim(value);
            }
        }

        public string EmailAddress
        {
            get
            {
                return msEmailAddress;
            }
            set
            {
                msEmailAddress = Strings.Trim(value);
            }
        }

        public string Description
        {
            get
            {
                return msDescription;
            }
            set
            {
                msDescription = Strings.Trim(value);
            }
        }

        public bool HaveOut
        {
            get
            {
                return mbHaveOut;
            }
            set
            {
                mbHaveOut = value;
            }
        }

        public bool IsOut
        {
            get
            {
                return mbIsOut;
            }
            set
            {
                mbIsOut = value;
            }
        }

        public bool IsActive
        {
            get
            {
                return mbIsActive;
            }
            set
            {
                mbIsActive = value;
            }
        }

        public int DueBackDays
        {
            get
            {
                return mlDueBackDays;
            }
            set
            {
                mlDueBackDays = value;
            }
        }
    }
}