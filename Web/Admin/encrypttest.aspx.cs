// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{

    public partial class encrypttest : AdminPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!ThisCustomer.IsAdminSuperUser)
            {
                Response.Redirect(AppLogic.AdminLinkUrl("default.aspx"));
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Label1.Text = Security.MungeString(TextBox1.Text);
            Label2.Text = Security.UnmungeString(Label1.Text);
        }
        protected void Button2_Click(object sender, EventArgs e)
        {
            Label3.Text = Security.MungeStringOld(TextBox2.Text);
            Label4.Text = Security.UnmungeStringOld(Label3.Text);
        }
        protected void Button3_Click(object sender, EventArgs e)
        {
            Label5.Text = Security.UnmungeStringOld(TextBox3.Text);
            Label6.Text = Security.MungeString(Label5.Text);
            Label7.Text = Security.UnmungeString(Label6.Text);
        }
}
}

//admin.title.encrypttest = Encrypt Test
//admin.encrypttest.V2Encryption = V2 Encryption:
//admin.encrypttest.Encrypt = String To Encrypt:
//admin.common.EncryptIt = Encrypt It!
//admin.encrypttest.EncryptedValue = Encrypted Value:
//admin.encrypttest.DecryptedValue = Decrypted Value:
//admin.encrypttest.V1Encryption = V1 Encryption:
//admin.encrypttest.StringToEncrypt = String To Encrypt:
//admin.encrypttest.TextAgain = V1 Encrypted -&gt; Unencrypt -&gt; V2 -&gt; Encrypt -&gt; Plain Text Again:
//admin.encrypttest.V1Encryption2 = V1 Encrypted Value:
//admin.encrypttest.V1Plain = V1 Plain Text:
//admin.encrypttest.V2Encryption2 = V2 Encrypted Value:
//admin.encrypttest.V2Decrypted = V2 Decrypted Value:
//admin.common.DoIt = Do It!


