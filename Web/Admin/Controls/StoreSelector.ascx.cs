// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

public partial class StoreSelector : System.Web.UI.UserControl
{
    private bool m_autopostback;
    private bool m_showdefaultforallstores;
    private RepeatDirection m_listrepeatdirection;
    private StoreSelectedMode m_selectmode;

    public StoreSelector()
    {
        AutoPostBack = false;
    }

    protected override void OnInit(EventArgs e)
    {
        GetData();
        base.OnInit(e);
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        
    }

    private void GetData()
    {
        List<Store> xList = Store.GetStoreList();

        switch (SelectMode)
        {
            case StoreSelectedMode.MultiCheckList:
                lstMultiSelect.RepeatDirection = ListRepeatDirection;
                lstMultiSelect.AutoPostBack = this.AutoPostBack;
                lstMultiSelect.DataSource = xList;
                lstMultiSelect.DataBind();
                break;
            case StoreSelectedMode.SingleRadioList:
                lstSingleSelect.RepeatDirection = ListRepeatDirection;
                lstSingleSelect.AutoPostBack = this.AutoPostBack;
                lstSingleSelect.DataSource = xList;
                lstSingleSelect.DataBind();
                if (m_showdefaultforallstores)
                {
                    cmbSingleList.Items.Insert(0, new ListItem(AppLogic.GetString("admin.storeselector.default", ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer.LocaleSetting), "0"));
                }
                break;
            case StoreSelectedMode.SingleDropDown:
                cmbSingleList.DataSource = xList;
                cmbSingleList.AutoPostBack = this.AutoPostBack;
                cmbSingleList.DataBind();
                if (m_showdefaultforallstores)
                {
                    cmbSingleList.Items.Insert(0, new ListItem(AppLogic.GetString("admin.storeselector.default", ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer.LocaleSetting), "0"));
                }
                break;
        }
        if (xList.Count > 1)
        {
            lstMultiSelect.Visible = SelectMode == StoreSelectedMode.MultiCheckList;
            lstSingleSelect.Visible = SelectMode == StoreSelectedMode.SingleRadioList;
            cmbSingleList.Visible = SelectMode == StoreSelectedMode.SingleDropDown;
        }
        else
        {
            lblText.Visible = false;
            lstMultiSelect.Visible = false;
            lstSingleSelect.Visible = false;
            cmbSingleList.Visible = false;
        }

    }

    public enum StoreSelectedMode
    {
        SingleRadioList,
        SingleDropDown,
        MultiCheckList
    }

    /// <summary>
    /// Whether or not the text property of the control is visible
    /// </summary>
    public bool ShowText
    {
        get
        {
            return lblText.Visible;
        }
        set
        {
            lblText.Visible = value;
        }
    }
    /// <summary>
    /// Text to display describing the control
    /// </summary>
    public string Text
    {
        get
        {
            return lblText.Text;
        }
        set
        {
            lblText.Text = value;
        }
    }

    /// <summary>
    /// Total number of stores retrieved by the control
    /// </summary>
    public int StoreCount
    { 
        get 
        {
            return Store.StoreCount;
        } 
    }

    
    /// <summary>
    /// Weather or not to show an item "Default for all stores" -> storeid 0
    /// </summary>
    public bool ShowDefaultForAllStores
    {
        get { return m_showdefaultforallstores; }
        set { m_showdefaultforallstores = value; }
    }

    /// <summary>
    /// Weather or not the store control events triggers a post back
    /// </summary>
    public bool AutoPostBack
    {
        get { return m_autopostback; }
        set { m_autopostback = value; }
    }

    /// <summary>
    /// The repeat direction for the checklist or radio list
    /// </summary>
    public RepeatDirection ListRepeatDirection
    {
        get { return m_listrepeatdirection; }
        set { m_listrepeatdirection = value; }
    }

    /// <summary>
    /// The mode in which the control displays
    /// </summary>
    public StoreSelectedMode SelectMode
    {
        get { return m_selectmode; }
        set { m_selectmode = value; }
    }

    /// <summary>
    /// The selected index of the store (not valid in single select mode
    /// </summary>
    public int SelectedIndex
    {
        get
        {
            if (SelectMode == StoreSelectedMode.SingleDropDown)
                return cmbSingleList.SelectedIndex;
            if (SelectMode == StoreSelectedMode.SingleRadioList)
                return lstSingleSelect.SelectedIndex;
            return -1;
        }
        set
        {
            if (SelectMode == StoreSelectedMode.SingleDropDown)
                cmbSingleList.SelectedIndex = value;
            if (SelectMode == StoreSelectedMode.SingleRadioList)
                lstSingleSelect.SelectedIndex = value;
            
        }
    }

    private EventHandler _SelectedIndexChanged;
    /// <summary>
    /// The selected index changed event when in single-select mode
    /// </summary>
    public event EventHandler SelectedIndexChanged
    {
        add{
            _SelectedIndexChanged += value;
        }
        remove
        {
            _SelectedIndexChanged -= value;
        }
    }

    /// <summary>
    /// The ID of the selected store (valid in single select only)
    /// </summary>
    public int SelectedStoreID
    {
        get
        {
            if (SelectMode == StoreSelectedMode.SingleDropDown)
            {
                return int.Parse(cmbSingleList.SelectedValue);
            }
            else if (SelectMode == StoreSelectedMode.SingleRadioList)
            {
                return int.Parse(lstSingleSelect.SelectedValue);
            }

            return -1;           
        }
        set
        {
            if (SelectMode == StoreSelectedMode.SingleDropDown)
            {
                cmbSingleList.SelectedValue = value.ToString();
            }
            else if (SelectMode == StoreSelectedMode.SingleRadioList)
            {
                lstSingleSelect.SelectedValue = value.ToString();
            }
        }
    }
    /// <summary>
    /// All the stores in the control which are selected
    /// (Only valid if in a MultiSelect mode)
    /// </summary>
    public int[] SelectedStoreIDs
    {
        get
        {
            List<int> selectedStores = new List<int>();
            foreach (ListItem xItm in lstMultiSelect.Items)
            {
                if (xItm.Selected)
                {
                    selectedStores.Add(int.Parse(xItm.Value));
                }
            }
            return selectedStores.ToArray();            
        }
        set
        {
            List<int> storeList = new List<int>(value);
            foreach (ListItem xItm in lstMultiSelect.Items)
            {
                xItm.Selected = false;
            }
            foreach (ListItem xItm in lstMultiSelect.Items)
            {
                foreach (int store in storeList)
                {
                    if (int.Parse(xItm.Value) == store)
                    {
                        xItm.Selected = true;
                        storeList.Remove(store);
                        break;
                    }
                }
            }
        }
    }
    /// <summary>
    /// All the stores in the control which are not selected
    /// (Only valid if in a MultiSelect mode)
    /// </summary>
    public int[] UnSelectedStoreIDs
    {
        get
        {
            List<int> selectedStores = new List<int>();
            foreach (ListItem xItm in lstMultiSelect.Items)
            {
                if (! xItm.Selected)
                {
                    selectedStores.Add(int.Parse(xItm.Value));
                }
            }
            return selectedStores.ToArray();      
        }
    }

    protected void lstSingleSelect_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_SelectedIndexChanged != null)
        {
            _SelectedIndexChanged(this, e);
        }
    }
}
