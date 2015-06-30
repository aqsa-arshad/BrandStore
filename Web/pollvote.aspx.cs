// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{
	/// <summary>
	/// Summary description for pollvote.
	/// </summary>
	public partial class pollvote : System.Web.UI.Page
	{
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
			Response.CacheControl="private";
			Response.Expires=0;
			Response.AddHeader("pragma", "no-cache");

            Customer ThisCustomer = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;
			ThisCustomer.RequireCustomerRecord();

			int CustomerID = ThisCustomer.CustomerID;
            int PollID = CommonLogic.QueryStringNativeInt("PollID");
            int PollAnswerID = CommonLogic.QueryStringNativeInt("Poll_" + PollID.ToString());

			if(PollID != 0 && CustomerID != 0 && PollAnswerID != 0)
			{
				// record the vote:
				try
				{
                    DB.ExecuteSQL("INSERT INTO PollVotingRecord(PollID,CustomerID,PollAnswerID) VALUES(" + PollID.ToString() + "," + CustomerID.ToString() + "," + PollAnswerID.ToString() + ")");
				}
				catch {}
			}

			Response.Redirect("polls.aspx");
		}

	}
}
