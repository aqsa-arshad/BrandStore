// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.Profile;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for monthlymaintenance
    /// </summary>
    public partial class monthlymaintenance : AdminPageBase
    {
        #region "Properties"
        private string Status
        {
            get { return (string)Session["Status"]; }
            set{Session["Status"] = value;}
        }
        private DateTime StartTime
        {
            get { return Session["StartTime"] == null ? new DateTime(0) : (DateTime)Session["StartTime"]; }
            set{Session["StartTime"] = value;}

        }
        private DateTime EndTime
        {
            get { return Session["EndTime"] == null ? new DateTime(0) : (DateTime)Session["EndTime"]; }
            set { Session["EndTime"] = value; }
        }
        private TimeSpan runningTime
        {
            get
            {
                if (StartTime == new DateTime(0))
                    new TimeSpan(0);
                if (EndTime != new DateTime(0))
                {
                    return new TimeSpan(EndTime.Ticks - StartTime.Ticks);
                }
                else
                {
                    return new TimeSpan(DateTime.Now.Ticks - StartTime.Ticks);
                }
            }
        }
        private string formattedRunningTime
        {
            get
            {
                if (runningTime == TimeSpan.Zero)
                    return string.Empty;
                else
                {
                    return string.Format("{2}:{1}:{0}",
                        (runningTime.Seconds % 60).ToString("0#"),
                        (runningTime.Minutes % 60).ToString("0#"),
                        (runningTime.Hours).ToString("#0")
                        );
                }
            }
        }
        private bool ControlsEnabled
        {
            set
            {
                GOButton.Enabled = value;
                ClearAllShoppingCarts.Enabled = value;
                ClearAllWishLists.Enabled = value;
                ClearAllGiftRegistries.Enabled = value;
                EraseOrderCreditCards.Enabled = value;
                EraseSQLLog.Enabled = value;
                EraseProfileLog.Enabled = value;
                ClearProductViewsOlderThan.Enabled = value;
                InvalidateUserLogins.Enabled = value;
                PurgeAnonUsers.Enabled = value;
                EraseAddressCreditCards.Enabled = value;
                PurgeDeletedRecords.Enabled = value;
                TuneIndexes.Enabled = value;
                SaveSettings.Enabled = value;
            }
        }
        #endregion
        
        protected void Page_Load(object sender, System.EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            Server.ScriptTimeout = 10000; // these could run quite a long time!
            if (!IsPostBack)
            {
                divRunning.Attributes.CssStyle["visibility"] = "hidden";
                String SavedSettings = AppLogic.AppConfig("System.SavedMonthlyMaintenance", 0, true);
                if (SavedSettings.Length != 0)
                {
                    foreach (String s in SavedSettings.Split(','))
                    {
                        if (s.Trim().Length != 0)
                        {
                            String[] token = s.Trim().Split('=');
                            String ParmName = token[0].ToUpper(CultureInfo.InvariantCulture).Trim();
                            String ParmValue = token[1].ToUpper(CultureInfo.InvariantCulture).Trim();
                            switch (ParmName)
                            {
                                case "INVALIDATEUSERLOGINS":
                                    InvalidateUserLogins.Checked = (ParmValue == "TRUE");
                                    break;
                                case "PURGEANONUSERS":
                                    PurgeAnonUsers.Checked = (ParmValue == "TRUE");
                                    break;
                                case "CLEARALLSHOPPINGCARTS":                                    
                                    ClearAllShoppingCarts.SelectedValue = ParmValue;
                                    break;
                                case "CLEARALLWISHLISTS":
                                    ClearAllWishLists.SelectedValue = ParmValue;
                                    break;
                                case "CLEARALLGIFTREGISTRIES":                                    
                                    ClearAllGiftRegistries.SelectedValue = ParmValue;
                                    break;
                                case "ERASEORDERCREDITCARDS":
                                    EraseOrderCreditCards.SelectedValue = ParmValue;
                                    break;
                                case "ERASEADDRESSCREDITCARDS":
                                    EraseAddressCreditCards.Checked = (ParmValue == "TRUE");
                                    break;
                                case "ERASESQLLOG":
                                    EraseSQLLog.SelectedValue = ParmValue;
                                    break;
                                case "CLEARPRODUCTVIEWSOLDERTHAN":
                                    ClearProductViewsOlderThan.SelectedValue = ParmValue;
                                    break;
                                case "TUNEINDEXES":
                                    TuneIndexes.Checked = (ParmValue == "TRUE");
                                    break;
                                case "SAVESETTINGS":
                                    SaveSettings.Checked = (ParmValue == "TRUE");
                                    break;
                            }
                        }
                    }
                }
            }

            if (Status != null && Status == "Processing")
            {
                lblStatus.Text = formattedRunningTime;
                ShowDialog();
            }
            Page.Form.DefaultButton = GOButton.UniqueID;
        }
        private int GetIndex(DropDownList l, String TheVal)
        {
            int i = 0;
            foreach (ListItem ix in l.Items)
            {
                if (ix.Text.Equals(TheVal, StringComparison.InvariantCultureIgnoreCase))                
                {
                    break;
                }
                i++;
            }
            return i;
        }
        private static int maintenanceTimeout
        {
            get
            {
                if (AppLogic.AppConfig("MonthlyMaintenanceTimeout") != string.Empty)
                {
                    return int.Parse(AppLogic.AppConfig("MonthlyMaintenanceTimeout"));
                }
                else
                {
                    return 120;
                }
            }
        }


        #region Maintenance Delegate
        Action<SqlParameter[]> actRunMaintenance = (paramset) =>
        {
            using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
            {
                SqlCommand cmdMaintenance = new SqlCommand("dbo.aspdnsf_MonthlyMaintenance");
                cmdMaintenance.CommandType = CommandType.StoredProcedure;
                cmdMaintenance.Parameters.AddRange(paramset);
                con.Open();
                cmdMaintenance.Connection = con;
                cmdMaintenance.CommandTimeout = maintenanceTimeout * 1000;
                cmdMaintenance.ExecuteNonQuery();
            }
        };
        #endregion
        
        
        protected void ShowTime_Tick(object sender, EventArgs e)
        {
            if (Status != "Complete")
            {
                lblStatus.Text = formattedRunningTime;
                lblNotice.Visible = false;
            }
            else
            {
                lblStatus.Text = AppLogic.GetString("admin.monthlymaintenance.Done", SkinID, LocaleSetting);
                tmrMain.Enabled = false;
                HideDialog();
            }
        }

        protected void MaintenanceCallback(IAsyncResult res)
        {

            if (res.IsCompleted)
            {
                Status = "Complete";
                EndTime = DateTime.Now;
                Session.Remove("EndTime");
                Session.Remove("StartTime");

            }
        }

        private void HideDialog()
        {
            container.Attributes.CssStyle.Clear();
            divRunning.Attributes.CssStyle["visibility"] = "hidden";
            ControlsEnabled = true;
        }
        private void ShowDialog()
        {
            tmrMain.Enabled = true;
            container.Attributes.CssStyle["opacity"] = "0.7";
            container.Attributes.CssStyle["background-color"] = "Gray";
            container.Attributes.CssStyle["filter"] = "alpha(opacity = 70)";
            lblStatus.Visible = true;
            divRunning.Style["visibility"] = "visible";
            ControlsEnabled = false;
            
            
        }

        protected void GOButton_Click(object sender, EventArgs e)
        {

            SqlParameter[] spa = { 
                DB.CreateSQLParameter("@InvalidateCustomerCookies", SqlDbType.TinyInt, 1, CommonLogic.IIF(InvalidateUserLogins.Checked, 1, 0), ParameterDirection.Input), 
                DB.CreateSQLParameter("@PurgeAnonCustomers", SqlDbType.TinyInt, 1, CommonLogic.IIF(PurgeAnonUsers.Checked, 1, 0), ParameterDirection.Input),
                DB.CreateSQLParameter("@CleanShoppingCartsOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(ClearAllShoppingCarts.SelectedValue), ParameterDirection.Input),
                DB.CreateSQLParameter("@CleanWishListsOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(ClearAllWishLists.SelectedValue), ParameterDirection.Input),
                DB.CreateSQLParameter("@CleanGiftRegistriesOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(ClearAllGiftRegistries.SelectedValue), ParameterDirection.Input),
                DB.CreateSQLParameter("@EraseCCFromAddresses", SqlDbType.TinyInt, 1, CommonLogic.IIF(EraseAddressCreditCards.Checked, 1, 0), ParameterDirection.Input),
                DB.CreateSQLParameter("@EraseSQLLogOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(EraseSQLLog.SelectedValue), ParameterDirection.Input),
                DB.CreateSQLParameter("@ClearProductViewsOrderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(ClearProductViewsOlderThan.SelectedValue), ParameterDirection.Input),
                DB.CreateSQLParameter("@EraseCCFromOrdersOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(EraseOrderCreditCards.SelectedValue), ParameterDirection.Input),
                DB.CreateSQLParameter("@DefragIndexes", SqlDbType.TinyInt, 1, CommonLogic.IIF(TuneIndexes.Checked, 1, 0), ParameterDirection.Input),
                DB.CreateSQLParameter("@PurgeDeletedRecords", SqlDbType.TinyInt, 1, CommonLogic.IIF(PurgeDeletedRecords.Checked, 1, 0), ParameterDirection.Input),
                DB.CreateSQLParameter("@RemoveRTShippingDataOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(dlClearRTShippingData.SelectedValue), ParameterDirection.Input),
                DB.CreateSQLParameter("@ClearSearchLogOlderThan", SqlDbType.SmallInt, 2, Convert.ToInt16(dlClearSearchData.SelectedValue), ParameterDirection.Input)
            };

            if (Status == null || Status != "Processing")
            {
                
                Status = "Processing";
                StartTime = DateTime.Now;
                Session.Remove("EndTime");
                
                ShowDialog();
                actRunMaintenance.BeginInvoke(spa, new AsyncCallback(MaintenanceCallback), null);
            }
            

            if (EraseProfileLog.SelectedIndex != 0)
            {
                using (SqlConnection con = new SqlConnection(DB.GetDBConn()))
                {
                    con.Open();

                    string str = "select Date = dateadd(dd,-" + EraseProfileLog.SelectedValue.ToString() + ",GETDATE())";
                    if (EraseProfileLog.SelectedIndex == 1)
                    {
                        str = "select Date = dateadd(dd,-1,GETDATE())";
                    }
                    else
                    {
                        str = "select Date = dateadd(dd,-" + EraseProfileLog.SelectedValue.ToString() + ",GETDATE())";
                    }

                    using (IDataReader rs = DB.GetRS(str, con))
                    {
                        if (rs.Read())
                        {
                            ProfileManager.DeleteInactiveProfiles(ProfileAuthenticationOption.All, DB.RSFieldDateTime(rs, "Date"));
                        }
                    }
                }
            }

            if (SaveSettings.Checked)
            {
                StringBuilder tmpS = new StringBuilder(1024);
                tmpS.Append("InvalidateUserLogins=");
                tmpS.Append(InvalidateUserLogins.Checked);
                tmpS.Append(",");
                tmpS.Append("PurgeAnonUsers=");
                tmpS.Append(PurgeAnonUsers.Checked);
                tmpS.Append(",");
                tmpS.Append("ClearAllShoppingCarts=");
                tmpS.Append(ClearAllShoppingCarts.Items[ClearAllShoppingCarts.SelectedIndex].Value);
                tmpS.Append(",");
                tmpS.Append("ClearAllWishLists=");
                tmpS.Append(ClearAllWishLists.Items[ClearAllWishLists.SelectedIndex].Value);
                tmpS.Append(",");
                tmpS.Append("ClearAllGiftRegistries=");
                tmpS.Append(ClearAllGiftRegistries.Items[ClearAllGiftRegistries.SelectedIndex].Value);
                tmpS.Append(",");
                tmpS.Append("EraseOrderCreditCards=");
                tmpS.Append(EraseOrderCreditCards.Items[EraseOrderCreditCards.SelectedIndex].Value);
                tmpS.Append(",");
                tmpS.Append("EraseAddressCreditCards=");
                tmpS.Append(EraseAddressCreditCards.Checked);
                tmpS.Append(",");
                tmpS.Append("EraseSQLLog=");
                tmpS.Append(EraseSQLLog.Items[EraseSQLLog.SelectedIndex].Value);
                tmpS.Append(",");
                tmpS.Append("ClearProductViewsOlderThan=");
                tmpS.Append(ClearProductViewsOlderThan.Items[ClearProductViewsOlderThan.SelectedIndex].Value);
                tmpS.Append(",");
                tmpS.Append("TuneIndexes=");
                tmpS.Append(TuneIndexes.Checked);
                tmpS.Append(",");
                tmpS.Append("SaveSettings=");
                tmpS.Append(SaveSettings.Checked);
                AppLogic.SetAppConfig("System.SavedMonthlyMaintenance", tmpS.ToString(), 0);
            }
            //resetError("Maintenance complete.", false);
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">" + AppLogic.GetString("admin.common.Notice", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
            {
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
            }

            if (error.Length > 0)
            {
                str += error + "";
            }
            else
            {
                str = "";
            }

            ltError.Text = str;
        }
    }
}
