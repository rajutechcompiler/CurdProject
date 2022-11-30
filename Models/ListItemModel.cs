using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabFusionRMS.Models
{
    public class ListItemModel
    {
        public ListItemModel(string text, string value)
        {
            Text = text;
            Value = value;
        }

        private string text;

        private string value;
        public string Text
        {
            get
            {
                if (text != null)
                {
                    return text;
                }

                if (value != null)
                {
                    return value;
                }

                return string.Empty;
            }
            set
            {
                text = value;
            }
        }

        public string Value
        {
            get
            {
                if (value != null)
                {
                    return value;
                }

                if (text != null)
                {
                    return text;
                }

                return string.Empty;
            }
            set
            {
                this.value = value;
            }
        }
    }
}
