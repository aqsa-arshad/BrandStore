﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using AspDotNetStorefront;
using System.Collections.Generic;

public partial class controls_TrueBlueUserInfo : System.Web.UI.UserControl
{
    Customer ThisCustomer;
    List<CustomerFund> lstCustomerFund = new List<CustomerFund>();

    protected void Page_Load(object sender, EventArgs e)
    {
        String WelcomeHeading = String.Empty;
        if (ThisCustomer == null)
        {
            ThisCustomer = (Page as AspDotNetStorefront.SkinBase).ThisCustomer;
        }

        WelcomeHeading = " Hi," + " " + ThisCustomer.FirstName.Trim() + " " + ThisCustomer.LastName.Trim();
        WelcomeHeadingAfterUserLogin.InnerText = WelcomeHeading;
        ExpandCustomerfund();
    }
    private void ExpandCustomerfund()
    {
        CustomerFund cf = new CustomerFund();
        int customerLevelId = (int)UserType.BLUUNLIMITED;
        lstCustomerFund = AuthenticationSSO.GetCustomerFund(ThisCustomer.CustomerID);
        lblCustomerLevel.Text = "Level: " + ((ThisCustomer.CustomerLevelID == customerLevelId) ? "Partners" : ThisCustomer.CustomerLevelName);
        lblDealerLevel.Text = ((ThisCustomer.CustomerLevelID == customerLevelId) ? "Partners" : ThisCustomer.CustomerLevelName.Replace("BLU", ""));

        if (ThisCustomer.CustomerLevelID == (int)UserType.POTENTIAL)
        {
            lstCustomerFund.RemoveAll(x => x.FundID == (int)FundType.BLUBucks);
            hBluBucks.Visible = false;
        }
        else if (ThisCustomer.CustomerLevelID == (int)UserType.HOMEDEPOT || ThisCustomer.CustomerLevelID == (int)UserType.MENARDS || ThisCustomer.CustomerLevelID == (int)UserType.LOWES)
        {
            lstCustomerFund.RemoveAll(x => x.FundID == (int)FundType.BLUBucks);
            hBluBucks.Visible = false;
            dLogoBox.Visible = false;
        }
        else
        {
            dLogoBox.Visible = true;
            hBluBucks.Visible = true;
        }
        lstCustomerFund.RemoveAll(x => x.FundID == (int)FundType.SOFFunds);
        cf = lstCustomerFund.SingleOrDefault(x => x.FundID == (int)FundType.BLUBucks);
        //if (ThisCustomer.CustomerLevelID == (int)UserType.BLUUNLIMITED)
        //{
        //    if (cf != null)
        //    {
        //        lstCustomerFund.Clear();
        //        lstCustomerFund.Add(cf);
        //        rptCustomerFunds.DataSource = lstCustomerFund;
        //        rptCustomerFunds.DataBind();
        //    }
        //    else
        //    {
        //        lstCustomerFund.Clear();
        //        rptCustomerFunds.DataSource = lstCustomerFund;
        //        rptCustomerFunds.DataBind();
        //    }
        //    ExpandFunds.Visible = false;
        //    lnkHideFunds.Visible = false;
        //    return;
        //}
        if (cf != null)
        {
            lstCustomerFund.Remove(cf);
            GetFilteredCustomerFund(lstCustomerFund);
            lstCustomerFund.Clear();
            lstCustomerFund.Add(cf);
            rptCustomerFunds.DataSource = lstCustomerFund;
            rptCustomerFunds.DataBind();
        }
        else
        {
            GetFilteredCustomerFund(lstCustomerFund);
        }
    }
    private void GetFilteredCustomerFund(List<CustomerFund> lstCustomerFund)
    {
        try
        {
            foreach (CustomerFund item in lstCustomerFund.ToList())
            {
                if (item != null)
                {
                    if (item.AmountAvailable <= 0)
                    {
                        lstCustomerFund.Remove(item);
                    }
                }
            }
            if (lstCustomerFund.Count > 0)
            {
                rptAllCustomerFunds.DataSource = lstCustomerFund;
                rptAllCustomerFunds.DataBind();
                ExpandFunds.Visible = true;
            }
            else
            {
                ExpandFunds.Visible = false;
                lnkHideFunds.Visible = false;
            }

        }
        catch (Exception ex)
        {
            SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
            ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
            MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
        }
    }




}