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
	/// Summary description for remove.
	/// </summary>
	public partial class remove : SkinBase
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			if(CommonLogic.QueryStringCanBeDangerousContent("id").Length != 0)
			{
                AppLogic.CheckForScriptTag(CommonLogic.QueryStringCanBeDangerousContent("id"));
                DB.ExecuteSQL("update customer set OKToEMail=0 where customerguid=" + DB.SQuote(CommonLogic.QueryStringCanBeDangerousContent("id")));
			}
			litRemoveEmailCompleteMessage.Text = "AppConfig.MailingMgr.RemoveEMailCompleteMessage".StringResource();
		}
	}
}
