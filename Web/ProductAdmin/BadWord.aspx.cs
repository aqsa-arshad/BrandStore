// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web.UI;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    public partial class BadWord : AdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            SectionTitle = AppLogic.GetString("admin.sectiontitle.BadWord", SkinID, LocaleSetting);

            Page.Form.DefaultButton = btnSubmit.UniqueID;   
        }
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (txtWord.Text.Trim().Length > 0)
            {
                string word = txtWord.Text.Trim();
                // see if this name is already there:
                if (AppLogic.badWord(word) != null)
                {
                    if (AppLogic.badWord(word).Word.Length > 0)
                    {
                        resetError(AppLogic.GetString("admin.BadWord.ExistingBadWord", SkinID, LocaleSetting), true);
                        return;
                    }
                }
                try
                {
                    AppLogic.BadWordTable.Add(word,LocaleSetting);
                        resetError("Item added.", false);
                    ViewState["IsInsert"] = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format(AppLogic.GetString("admin.BadWord.UpdateBadWordError", SkinID, LocaleSetting), ex.ToString()));
                }
            }
            else
            {
                resetError(AppLogic.GetString("admin.BadWord.EntryRequired", SkinID, LocaleSetting), true);
            }
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">" + AppLogic.GetString("admin.common.Notice", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">" + AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting) + "</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ltError.Text = str;
        }

}
}

