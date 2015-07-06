// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Globalization;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// To be able to mapp a payment method to a specific shipping method. This is to avoid situations where customers
    /// can choose a payment method that is NOT supported for choosen shipping method. For example you don´t want customers to be able 
    /// to choose to pay by credit card when the cusotmer had choosen the shipping option COD (Cash on delivery). 
    /// We want to make it easy for the customer and therfor only show the supported payment method for each shipping option.
    /// 
    /// </summary>
    public partial class MapShippingMethodToPaymentMethod : AdminPageBase
    {
        string strAvailiblePaymentMethods = AppLogic.AppConfig("PaymentMethods");

        /// <summary>
        /// Runs when the user hit the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            //If the page is not in postback then we do this
            if (!IsPostBack)
            {
                //Select all Shipping methods from database and add them to the listbox

                using (SqlConnection dbconn = DB.dbConn())
                {
                    dbconn.Open();
                    using (IDataReader rsReferenceShippingMethods = DB.GetRS("SELECT ShippingMethodID , Name FROM ShippingMethod", dbconn))
                    {
                        while (rsReferenceShippingMethods.Read())
                        {
                            ListBoxAvailShippingMethods.Items.Add(new ListItem(DB.RSFieldByLocale(rsReferenceShippingMethods, "Name", "en-US") + " ( ID=" + rsReferenceShippingMethods.GetValue(0).ToString() + " ) ", rsReferenceShippingMethods.GetValue(0).ToString()));
                        }
                    }
                }

                //Here we PreSelect the 1:st Item in the Listbox
                ListBoxAvailShippingMethods.SelectedIndex = 0;

                //Update the payment options for selected shipping method.
                UpdateSelectedPayments(int.Parse(ListBoxAvailShippingMethods.SelectedValue));
            }
            Page.Form.DefaultButton = btnUpdateShippingToPaymentMethod.UniqueID;
        }


        /// <summary>
        /// Build the new supported payments STRING and save this into the database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnUpdateShippingToPaymentMethod_Click(object sender, EventArgs e)
        {
            int intSelectedShippingMethodID = int.Parse(ListBoxAvailShippingMethods.SelectedValue.ToString());
            string strSelectedPM = "";
            int intSelectedPaymentMethods = ListBoxSelectedPaymentMethods.Items.Count;

            if (intSelectedPaymentMethods > 0)
            {
                int i = 0;
                for (i = 0; i < intSelectedPaymentMethods; i++)
                {
                    strSelectedPM += ListBoxSelectedPaymentMethods.Items[i].Value.ToString().Trim();
                    strSelectedPM += ",";
                }

                if (strSelectedPM.EndsWith(","))
                {
                    int intSelectedPMLength = strSelectedPM.Length;
                    strSelectedPM = strSelectedPM.Remove(intSelectedPMLength - 1);
                }

                DB.ExecuteSQL("UPDATE ShippingMethod SET MappedPM=" + DB.SQuote(strSelectedPM) + " WHERE ShippingMethodID=" + intSelectedShippingMethodID.ToString());
            }

        }

        /// <summary>
        /// When the user select another item in the Listbox this event fires and call the function 
        /// to update the listbox for payment options for selected shipping method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ListBoxAvailShippingMethods_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBoxSelectedPaymentMethods.Items.Clear();
            UpdateSelectedPayments(int.Parse(ListBoxAvailShippingMethods.SelectedValue));
        }

        /// <summary>
        /// Update the listbox for payment options for selected shipping method.
        /// </summary>
        /// <param name="intSelectedShippingMethodID"></param>
        private void UpdateSelectedPayments(int intSelectedShippingMethodID)
        {
            string strCurrentMappingsInDB = "";

            ListBoxAvailPaymentMethods.Items.Clear();
            ListBoxSelectedPaymentMethods.Items.Clear();

            //Get the information from database of current payment options for selected shipping option.

            using (SqlConnection dbconn = DB.dbConn())
            {
                dbconn.Open();
                using (IDataReader rsCurrentMappings = DB.GetRS("SELECT MappedPM FROM ShippingMethod WHERE ShippingMethodID=" + intSelectedShippingMethodID.ToString(), dbconn))
                {
                    while (rsCurrentMappings.Read())
                    {
                        strCurrentMappingsInDB = DB.RSField(rsCurrentMappings, "MappedPM");
                    }
                }
            }

            Hashtable hashCurrentMappingsInDB = new Hashtable();

            if (strCurrentMappingsInDB.Length > 0)
            {

                string[] strSplittedCurrentMappingsInDB = strCurrentMappingsInDB.Split(new char[] { ',' });

                foreach (string strPMinDB in strSplittedCurrentMappingsInDB)
                {
                    hashCurrentMappingsInDB.Add(strPMinDB, strPMinDB);
                    ListBoxSelectedPaymentMethods.Items.Add(new ListItem(strPMinDB, strPMinDB));

                }

            }

            //ALL Possible payment options
            //CREDIT CARD,PAYPAL,PAYPALEXPRESS,REQUEST QUOTE,PURCHASE ORDER,CHECK BY MAIL,C.O.D.,ECHECK, MICROPAY

            string[] strSplittedAvailiblePaymentMethods = strAvailiblePaymentMethods.Split(new char[] { ',' });

            //Loop through all availible payment options and select those option in listbox that is
            //allready selected in the database.
            foreach (string strPM in strSplittedAvailiblePaymentMethods)
            {
                if (hashCurrentMappingsInDB.ContainsKey(strPM))
                {

                }
                else
                {
                    ListBoxAvailPaymentMethods.Items.Add(new ListItem(strPM, strPM));
                }

            }

            if (ListBoxSelectedPaymentMethods.Items.Count > 0)
            {
                ListBoxSelectedPaymentMethods.Items[0].Text = ListBoxSelectedPaymentMethods.Items[0].Value + "(Show by default)";

            }
            UpdateSaveToDBTextFiled();
        }


        /// <summary>
        /// Move a selected payment options one step up in the listbox. The top Item will alsyas be the
        /// payemnt option that the store show as default (expanded) to the user at checkout.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnMovePaymentUp_Click(object sender, EventArgs e)
        {
            int intSelectedIndex;
            string strTempSwapText;
            string strTempSwapValue;


            intSelectedIndex = ListBoxSelectedPaymentMethods.SelectedIndex;

            if (intSelectedIndex > 0)
            {
                strTempSwapText = ListBoxSelectedPaymentMethods.Items[intSelectedIndex - 1].Text.ToString();
                strTempSwapValue = ListBoxSelectedPaymentMethods.Items[intSelectedIndex - 1].Value.ToString();


                ListBoxSelectedPaymentMethods.Items[intSelectedIndex - 1].Value = ListBoxSelectedPaymentMethods.Items[intSelectedIndex].Value;
                ListBoxSelectedPaymentMethods.Items[intSelectedIndex - 1].Text = ListBoxSelectedPaymentMethods.Items[intSelectedIndex].Text;

                ListBoxSelectedPaymentMethods.Items[intSelectedIndex].Value = strTempSwapValue;
                ListBoxSelectedPaymentMethods.Items[intSelectedIndex].Text = strTempSwapText;
                ListBoxSelectedPaymentMethods.SelectedIndex = intSelectedIndex - 1;
            }

            ListBoxSelectedPaymentMethods.Items[0].Text = ListBoxSelectedPaymentMethods.Items[0].Value + " (Show by default)";
            ListBoxSelectedPaymentMethods.Items[1].Text = ListBoxSelectedPaymentMethods.Items[1].Value;

            UpdateSaveToDBTextFiled();
        }

        /// <summary>
        /// Move the selected itme from the Avail list to the selected list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnSelectOne_Click(object sender, EventArgs e)
        {
            int intSelectedIndex;
            //Get the selected Item in the UnSelected ListBox
            intSelectedIndex = ListBoxAvailPaymentMethods.SelectedIndex;

            //If there are a selected Item then move the Item
            if (ListBoxAvailPaymentMethods.Items.Count >= 1 && intSelectedIndex != -1)
            {
                //Make the Move
                ListBoxSelectedPaymentMethods.Items.Add(ListBoxAvailPaymentMethods.Items[intSelectedIndex]);
                ListBoxAvailPaymentMethods.Items.Remove(ListBoxAvailPaymentMethods.Items[intSelectedIndex]);

                //Take away the selection from the ListBox
                ListBoxSelectedPaymentMethods.SelectedIndex = -1;
            }

            if (ListBoxSelectedPaymentMethods.Items.Count > 0)
            {
                ListBoxSelectedPaymentMethods.Items[0].Text = ListBoxSelectedPaymentMethods.Items[0].Value + " (Show by default)";
                UpdateSaveToDBTextFiled();
            }

            btnMovePaymentUp.Enabled = ListBoxSelectedPaymentMethods.Items.Count > 1;
        }


        /// <summary>
        /// Move the selected itme from the selected list to the avail list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnDeSelectOne_Click(object sender, EventArgs e)
        {

            int intSelectedIndex;


            //Get the selected Index from the ListBox
            intSelectedIndex = ListBoxSelectedPaymentMethods.SelectedIndex;

            //If there are a Item in the 
            if (intSelectedIndex != -1)
            {

                //If there are a selected Item in the ListBox then make the move
                if (ListBoxSelectedPaymentMethods.Items.Count >= 1 && intSelectedIndex != -1)
                {
                    ListBoxAvailPaymentMethods.Items.Add(new ListItem(ListBoxSelectedPaymentMethods.Items[intSelectedIndex].Value, ListBoxSelectedPaymentMethods.Items[intSelectedIndex].Value));
                    ListBoxSelectedPaymentMethods.Items.Remove(ListBoxSelectedPaymentMethods.Items[intSelectedIndex]);
                    ListBoxAvailPaymentMethods.SelectedIndex = -1;
                }

            }
            else
            {
                
            }

            if (ListBoxSelectedPaymentMethods.Items.Count > 0)
            {
                ListBoxSelectedPaymentMethods.Items[0].Text = ListBoxSelectedPaymentMethods.Items[0].Value + " (Show by default)";
                UpdateSaveToDBTextFiled();
            }

        }


        /// <summary>
        /// Move ALL itmes from the Avail list to the selected list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnSelectAll_Click(object sender, EventArgs e)
        {

            int intListBoxItems;
            int i = 0;

            //Get the selected Item in the UnSelected ListBox
            intListBoxItems = ListBoxAvailPaymentMethods.Items.Count;

            //If there are Item then move the Item
            if (intListBoxItems > 0)
            {
                for (i = 0; i < intListBoxItems; i++)
                {
                    //Make the Move
                    ListBoxSelectedPaymentMethods.Items.Add(ListBoxAvailPaymentMethods.Items[0]);
                    ListBoxAvailPaymentMethods.Items.Remove(ListBoxAvailPaymentMethods.Items[0]);
                }
            }

            if (ListBoxSelectedPaymentMethods.Items.Count > 0)
            {
                ListBoxSelectedPaymentMethods.Items[0].Text = ListBoxSelectedPaymentMethods.Items[0].Value + " (Show by default)";
                UpdateSaveToDBTextFiled();
            }

        }

        /// <summary>
        /// Move ALL itmes from the selected list to the avail list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnDeSelectALL_Click(object sender, EventArgs e)
        {

            int intListBoxItems;
            int i = 0;

            intListBoxItems = ListBoxSelectedPaymentMethods.Items.Count;

            //Make the move
            if (intListBoxItems > 0)
            {
                for (i = 0; i < intListBoxItems; i++)
                {
                    ListBoxAvailPaymentMethods.Items.Add(new ListItem(ListBoxSelectedPaymentMethods.Items[0].Value, ListBoxSelectedPaymentMethods.Items[0].Value));
                    ListBoxSelectedPaymentMethods.Items.Remove(ListBoxSelectedPaymentMethods.Items[0]);
                }
            }
            txtSaveToDBInfo.Text = "";

        }

        /// <summary>
        /// Update (for display only) the textbox so it get more "clear" which information that is
        /// saved to the databse for mapping payments
        /// </summary>
        private void UpdateSaveToDBTextFiled()
        {
            string strSelectedPM = "";

            int intSelectedPaymentMethods = ListBoxSelectedPaymentMethods.Items.Count;

            int i = 0;

            for (i = 0; i < intSelectedPaymentMethods; i++)
            {
                strSelectedPM += ListBoxSelectedPaymentMethods.Items[i].Value.ToString().Trim();
                strSelectedPM += ",";
            }

            if (strSelectedPM.EndsWith(","))
            {
                int intSelectedPMLength = strSelectedPM.Length;
                strSelectedPM = strSelectedPM.Remove(intSelectedPMLength - 1);
            }
            txtSaveToDBInfo.Text = strSelectedPM;
        }
    }
}
