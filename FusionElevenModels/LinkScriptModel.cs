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
using System.Windows.Forms;
using Smead.RecordsManagement;
using Smead.Security;

// LINKSCRIPT FUNCTIONS MODEL RETURN DOM OBJECT AND DATA
public class LinkScriptModel : BaseModel
{
    public LinkScriptModel()
    {
        ButtonsList = new List<Button>();
        ControllerList = new List<CreateController>();
    }
    public LinkScriptModel(Passport passport)
    {
        //TextboxList = new List<TextBox>();
        ButtonsList = new List<Button>();
        ControllerList = new List<CreateController>();
    }

    public string ErrorMsg
    {
        get;
        set;
    }
    public List<CreateController> ControllerList { get; set; }
    public List<TextBox> TextboxList
    {
        get;
        set;
    }
    public List<Label> LabelList
    {
        get;
        set;
    }
    public List<Button> ButtonsList
    {
        get;
        set;
    }
    public string lblHeading
    {
        get;
        set;
    }
    public string Title
    {
        get;
        set;
    }
    public string ReturnMessage
    {
        get;
        set;
    }
    public bool ContinuetoAnotherDialog
    {
        get;
        set;
    }
    public bool UnloadPromptWindow
    {
        get;
        set;
    }
    public bool Showprompt
    {
        get;
        set;
    }
    public bool isSuccessful
    {
        get;
        set;
    }
    public bool isBeforeAddLinkScript
    {
        get;
        set;
    }
    public bool isAfterAddLinkScript
    {
        get;
        set;
    }
    public bool isBeforeEditLinkScript
    {
        get;
        set;
    }
    public bool isAfterEditLinkScript
    {
        get;
        set;
    }
    public bool isBeforeDeleteLinkScript
    {
        get;
        set;
    }
    public bool isAfterDeleteLinkScript
    {
        get;
        set;
    }
    public string ScriptName
    {
        get;
        set;
    }
    public bool GridRefresh
    {
        get;
        set;
    }
    public bool Successful
    {
        get;
        set;
    }
    public string keyValue
    {
        get;
        set;
    }

    public void SetUpcontrolsValues(List<linkscriptUidata> uidata, InternalEngine Engine)
    {
        foreach (var ctr in uidata)
        {
            switch (ctr.type)
            {
                case "textarea":
                case "text":
                    {
                        var _conkey = Engine.ScriptControlDictionary[ctr.id];
                        _conkey.SetProperty(ScriptControls.ControlProperties.cpText, ctr.value);
                        Engine.ScriptControlDictionary[ctr.id] = _conkey;
                        break;
                    }

                case "select-one":
                    {
                        var _conkey = Engine.ScriptControlDictionary[ctr.id];
                        var val = ctr.value.Split("%&&&%")[0];
                        var text = ctr.value.Split("%&&&%")[2];
                        _conkey.SetProperty(ScriptControls.ControlProperties.cpText, text);
                        _conkey.SetProperty(ScriptControls.ControlProperties.cpItemData, val);
                        Engine.ScriptControlDictionary[ctr.id] = _conkey;
                        break;
                    }

                case "checkbox":
                    {
                        var _conkey = Engine.ScriptControlDictionary[ctr.id];
                        _conkey.SetProperty(ScriptControls.ControlProperties.cpValue, ctr.value);
                        Engine.ScriptControlDictionary[ctr.id] = _conkey;
                        break;
                    }

                case "radio":
                    {
                        var _conkey = Engine.ScriptControlDictionary[ctr.id];
                        _conkey.SetProperty(ScriptControls.ControlProperties.cpValue, ctr.value);
                        Engine.ScriptControlDictionary[ctr.id] = _conkey;
                        break;
                    }
            }
        }
    }
    private void SetHeadingAndTitle(ScriptReturn scriptresult)
    {
        this.lblHeading = scriptresult.Engine.Heading;
        this.Title = scriptresult.Engine.Title;
    }
    public void BuiltControls(ScriptReturn scriptresult)
    {
        SetHeadingAndTitle(scriptresult);
        UnloadPromptWindow = scriptresult.ScriptControlDictionary.Count == 0;

        foreach (var item in scriptresult.ScriptControlDictionary)
        {
            switch (item.Value.ControlType)
            {
                case ScriptControls.ControlTypes.ctTextBox:
                    CreateController text = new CreateController();
                    if (!string.IsNullOrEmpty(item.Value.GetProperty(ScriptControls.ControlProperties.cpText).ToString()))
                        text.Text = item.Value.GetProperty(ScriptControls.ControlProperties.cpText).ToString();

                    text.Id = item.Key;
                    text.Css = "form-control";
                    text.ControlerType = "textbox";
                    ControllerList.Add(text);
                    break;

                case ScriptControls.ControlTypes.ctLabel:
                    CreateController label = new CreateController();
                    label.Text = item.Value.GetProperty(ScriptControls.ControlProperties.cpCaption).ToString();
                    label.Id = item.Key;
                    label.Css = "control-label";
                    label.ControlerType = "label";
                    ControllerList.Add(label);
                    break;
                case ScriptControls.ControlTypes.ctComboBox:
                    CreateController dropdown = new CreateController();
                    int j = 0;
                    foreach (var _item in item.Value.ItemList)
                    {
                        dropdownprop prop = new dropdownprop();
                        prop.text = _item;
                        prop.value = item.Value.ItemDataList[j];
                        // dropdown.Text = _item
                        // If j < item.Value.ItemDataList.Count Then _item = item.Value.ItemDataList(j)
                        j = j + 1;
                        // dropdown.Items.Add(listitem)
                        dropdown.dropdownItems.Add(prop);
                    }
                    dropdown.Id = item.Key;
                    dropdown.Css = "form-control";
                    dropdown.ControlerType = "dropdown";
                    dropdown.dropIndex = Convert.ToInt32(item.Value.GetProperty(ScriptControls.ControlProperties.cpListindex));
                    ControllerList.Add(dropdown);
                    break;

                case ScriptControls.ControlTypes.ctOption:
                    CreateController radiobutton = new CreateController();
                    radiobutton.Text = item.Value.GetProperty(ScriptControls.ControlProperties.cpCaption).ToString();
                    radiobutton.Groupname = "LinkScriptRadioButtons";
                    radiobutton.Id = item.Key;
                    radiobutton.ControlerType = "radiobutton";
                    ControllerList.Add(radiobutton);
                    break;
                case ScriptControls.ControlTypes.ctCheck:
                    CreateController checkbox = new CreateController();
                    checkbox.Text = item.Value.GetProperty(ScriptControls.ControlProperties.cpCaption).ToString();
                    checkbox.Id = item.Key;
                    checkbox.ControlerType = "checkbox";
                    ControllerList.Add(checkbox);
                    break;

                case ScriptControls.ControlTypes.ctListBox:
                    CreateController listBox = new CreateController();
                    //var j = 0;
                    foreach (var _item in item.Value.ItemList)
                    {
                        listBox prop = new listBox();
                        prop.text = _item;
                        prop.value = item.Value.ItemDataList[0];
                        listBox.listboxItems.Add(prop);
                    }
                    listBox.rowCounter = 4.ToString();
                    listBox.Id = item.Key;
                    listBox.Css = "form-control";
                    listBox.ControlerType = "listBox";
                    listBox.dropIndex = Convert.ToInt32(item.Value.GetProperty(ScriptControls.ControlProperties.cpListindex));
                    ControllerList.Add(listBox);
                    break;
                case ScriptControls.ControlTypes.ctButton:
                    Button button = new Button();
                    if (item.Value.GetProperty(ScriptControls.ControlProperties.cpCaption) != string.Empty)
                        button.Text = Convert.ToString(item.Value.GetProperty(ScriptControls.ControlProperties.cpCaption));
                    else
                        button.Text = item.Key;

                    if (button.Text.Contains("&"))
                    {
                        button.Text = button.Text.Replace("&&", "!!!!!!ampersandescape!!!!!!!");
                        button.Text = button.Text.Replace("&", "");
                        button.Text = button.Text.Replace("!!!!!!ampersandescape!!!!!!!", "&");
                    }

                    button.Css = "btn btn-success text-uppercase";
                    button.Id = item.Key;
                    // AddHandler button.Click, AddressOf FlowButton_Click
                    ButtonsList.Add(button);
                    break;
                case ScriptControls.ControlTypes.ctMemoBox:
                    CreateController tx = new CreateController();
                    tx.Text = item.Value.GetProperty(ScriptControls.ControlProperties.cpText).ToString();
                    tx.Id = item.Key;
                    tx.Css = "form-control";
                    tx.ControlerType = "textarea";
                    ControllerList.Add(tx);
                    break;
            }
        }
    }
    public class CreateController
    {
        public CreateController()
        {
            dropdownItems = new List<dropdownprop>();
            listboxItems = new List<listBox>();
        }
        public string Text
        {
            get;
            set;
        }
        public string Id
        {
            get;
            set;
        }
        public string Css
        {
            get;
            set;
        }
        public List<dropdownprop> dropdownItems
        {
            get;
            set;
        }
        public List<listBox> listboxItems
        {
            get;
            set;
        }
        public string Groupname
        {
            get;
            set;
        }
        public string ControlerType
        {
            get;
            set;
        }
        public int dropIndex
        {
            get;
            set;
        }
        public string rowCounter
        {
            get;
            set;
        }
    }
    public class Button
    {
        public string Text
        {
            get;
            set;
        }
        public string Id
        {
            get;
            set;
        }
        public string Css
        {
            get;
            set;
        }
    }
    public class dropdownprop
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
    public class listBox
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

public class linkscriptPropertiesUI
{
    public string WorkFlow
    {
        get;
        set;
    }
    public int ViewId
    {
        get;
        set;
    }
    public string[] Rowids
    {
        get;
        set;
    }
    public string TableId
    {
        get;
        set;
    }
}

public class linkscriptUidata
{
    public string id
    {
        get;
        set;
    }
    public string value
    {
        get;
        set;
    }
    public string type
    {
        get;
        set;
    }
}

public class linkscriptParams
{
    public string WorkFlow
    {
        get;
        set;
    }
    public int ViewId
    {
        get;
        set;
    }
    public string[] Rowids
    {
        get;
        set;
    }
    public string Tableid
    {
        get;
        set;
    }
}

public class linkscriptResult
{
    public string ReturnMessage
    {
        get;
        set;
    }
    public bool ContinuetoAnotherDialog
    {
        get;
        set;
    }
    public void SetUpcontrolsValues(List<linkscriptUidata> uidata, InternalEngine Engine)
    {
        foreach (var ctr in uidata)
        {
            switch (ctr.type)
            {
                case "text":
                    {
                        var _conkey = Engine.ScriptControlDictionary[ctr.id];
                        _conkey.SetProperty(ScriptControls.ControlProperties.cpText, ctr.value);
                        Engine.ScriptControlDictionary[ctr.id] = _conkey;
                        break;
                    }

                case "select-one":
                    {
                        var _conkey = Engine.ScriptControlDictionary[ctr.id];
                        var val = ctr.value.Split("%&&&%")[0];
                        var text = ctr.value.Split("%&&&%")[2];
                        _conkey.SetProperty(ScriptControls.ControlProperties.cpText, text);
                        _conkey.SetProperty(ScriptControls.ControlProperties.cpItemData, val);
                        Engine.ScriptControlDictionary[ctr.id] = _conkey;
                        break;
                    }

                case "checkbox":
                    {
                        var _conkey = Engine.ScriptControlDictionary[ctr.id];
                        _conkey.SetProperty(ScriptControls.ControlProperties.cpValue, ctr.value);
                        Engine.ScriptControlDictionary[ctr.id] = _conkey;
                        break;
                    }

                case "radio":
                    {
                        var _conkey = Engine.ScriptControlDictionary[ctr.id];
                        _conkey.SetProperty(ScriptControls.ControlProperties.cpValue, ctr.value);
                        Engine.ScriptControlDictionary[ctr.id] = _conkey;
                        break;
                    }
            }
        }
    }
}