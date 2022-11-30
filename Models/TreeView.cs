using System.Collections.Generic;

namespace TabFusionRMS.WebCS.JSTreeView
{

    public class TreeView
    {
        public TreeView()
        {
            Nodes = new List<TreeNode>();
        }

        public List<TreeNode> Nodes
        {
            get
            {
                return m_Nodes;
            }
            set
            {
                m_Nodes = value;
            }
        }
        private List<TreeNode> m_Nodes;
    }

    public interface INodeItem
    {
        List<INodeItem> Nodes { get; set; }
        // Function All() As List(Of T)
    }

    public class TreeNode
    {
        public TreeNode()
        {
            ListItems = new List<INodeItem>();
        }

        public List<INodeItem> ListItems
        {
            get
            {
                return m_ListItems;
            }
            set
            {
                m_ListItems = value;
            }
        }
        private List<INodeItem> m_ListItems;
    }

    public class DataJsTree
    {
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }
        private string m_Name;
        public string Icon
        {
            get
            {
                return m_Icon;
            }
            set
            {
                m_Icon = value;
            }
        }
        private string m_Icon;
        public bool Opened
        {
            get
            {
                return m_Opened;
            }
            set
            {
                m_Opened = value;
            }
        }
        private bool m_Opened;
        public bool Selected
        {
            get
            {
                return m_Selected;
            }
            set
            {
                m_Selected = value;
            }
        }
        private bool m_Selected;
        public bool Disabled
        {
            get
            {
                return m_Disabled;
            }
            set
            {
                m_Disabled = value;
            }
        }
        private bool m_Disabled;
    }

    public class ListItem : INodeItem
    {
        public string Id
        {
            get
            {
                return m_Id;
            }
            set
            {
                m_Id = value;
            }
        }
        private string m_Id;
        public string Parent
        {
            get
            {
                return m_Parent;
            }
            set
            {
                m_Parent = value;
            }
        }
        private string m_Parent;
        public string Class
        {
            get
            {
                return m_Class;
            }
            set
            {
                m_Class = value;
            }
        }
        private string m_Class;
        public string Text
        {
            get
            {
                return m_Text;
            }
            set
            {
                m_Text = value;
            }
        }
        private string m_Text;
        public ListItemAHref Href
        {
            get
            {
                return m_Href;
            }
            set
            {
                m_Href = value;
            }
        }
        private ListItemAHref m_Href;
        public DataJsTree DataJsTree
        {
            get
            {
                return m_DataJsTree;
            }
            set
            {
                m_DataJsTree = value;
            }
        }
        private DataJsTree m_DataJsTree;
        // Public Property Type() As String
        // Get
        // Return m_Type
        // End Get
        // Set(value As String)
        // m_Type = value
        // End Set
        // End Property
        // Private m_Type As String

        public List<INodeItem> Nodes
        {
            get
            {
                return m_Nodes;
            }
            set
            {
                m_Nodes = value;
            }
        }
        private List<INodeItem> m_Nodes;

        public ListItem(string text, string id = "", string parent = "#", string className = "", ListItemAHref ahref = null, DataJsTree dataJsTree = null)
        {
            if (id is not null)
            {
                Id = id;
            }

            Parent = parent;
            Text = text;
            Class = className;

            if (ahref is not null)
            {
                Href = ahref;
            }
            else
            {
                Href = new ListItemAHref();
            }

            if (dataJsTree is not null)
            {
                DataJsTree = dataJsTree;
            }
            else
            {
                DataJsTree = new DataJsTree();
            }

            Nodes = new List<INodeItem>();
        }
    }

    public class ListItemAHref
    {
        public string Href
        {
            get
            {
                return m_Href;
            }
            set
            {
                m_Href = value;
            }
        }
        private string m_Href;
        public string Class
        {
            get
            {
                return m_Class;
            }
            set
            {
                m_Class = value;
            }
        }
        private string m_Class;

        public ListItemAHref(string href = "#", string cssClass = "")
        {
            Href = href;
            Class = cssClass;
        }
    }

}