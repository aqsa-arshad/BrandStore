// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using AspDotNetStorefrontCore;
using System;
using System.Text;

namespace AspDotNetStorefrontAdmin
{
    public partial class Admin_asyncstatus : AdminPageBase
    {
        /// <summary>
        /// Uses the ID parameter to retrieve async process status from a static hashtable and reports the status here
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            string sessID = CommonLogic.QueryStringCanBeDangerousContent("id");

            if (AsyncDataStore.RetrieveRecord(sessID) != null)
            {
                String[] Status = AsyncDataStore.RetrieveRecord(sessID).ToString().Split(',');

                if (Status != null && Status[0] == Status[1])
                {

                    ltlTimeRemaining.Text = AppLogic.GetString("admin.asyncstatus.Complete", ThisCustomer.LocaleSetting);
                    ltlNumeric.Text = String.Empty;
                    ltlPercent.Text = String.Empty;
                    ltlEstRemaining.Text = String.Empty;
                    imgWaiting.Visible = false;
                }
                else
                {
                    //Not completed.  Set the page to refresh
                    Response.AddHeader("Refresh", "5");

                    decimal sent = Convert.ToDecimal(Status[0]);
                    decimal total = Convert.ToDecimal(Status[1]);
                    DateTime startTime = Convert.ToDateTime(Status[2]);

                    decimal percentComplete = Math.Round(((sent / total) * 100), 2);

                    DateTime currentTime = DateTime.Now;
                    decimal elapsed = (decimal)currentTime.Subtract(startTime).TotalSeconds;
                    decimal opsPerSecond = ((decimal)elapsed / (decimal)sent);

                    TimeSpan remaining = TimeSpan.FromSeconds((double)(opsPerSecond * (total - sent)));

                    StringBuilder displayTime = new StringBuilder();

                    if (remaining.Days > 0)
                    {
                        displayTime.Append(remaining.Days.ToString() + " " + AppLogic.GetString("admin.common.days", ThisCustomer.LocaleSetting));
                        displayTime.Append(" ");
                    }
                    if (remaining.Hours > 0)
                    {
                        displayTime.Append(remaining.Hours.ToString() + " " + AppLogic.GetString("admin.common.Hours", ThisCustomer.LocaleSetting));
                        displayTime.Append(" ");
                    }
                    if (remaining.Minutes > 0)
                    {
                        displayTime.Append(remaining.Minutes.ToString() + " " + AppLogic.GetString("admin.common.Minutes", ThisCustomer.LocaleSetting));
                        displayTime.Append(" ");
                    }
                    if (remaining.Seconds > 0)
                    {
                        displayTime.Append(remaining.Seconds.ToString() + " " + AppLogic.GetString("admin.common.Seconds", ThisCustomer.LocaleSetting));
                        displayTime.Append(" ");
                    }


                    ltlNumeric.Text = String.Format(AppLogic.GetString("admin.asyncstatus.SendingXofY", ThisCustomer.LocaleSetting), Status[0], Status[1]);
                    ltlPercent.Text = percentComplete.ToString() + "%";
                    ltlTimeRemaining.Text = displayTime.ToString(); ;
                }


            }
        }
    }
}
