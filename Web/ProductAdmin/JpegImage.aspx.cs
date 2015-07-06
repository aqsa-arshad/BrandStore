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
using System.Drawing.Imaging;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
	public partial class JpegImage : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Create a CAPTCHA image using the text stored in the Session object.
            CaptchaImage ci = new CaptchaImage(Session["SecurityCode"].ToString(), 200, 50, "Century Schoolbook");
			// Change the response headers to output a JPEG image.
			Response.Clear();
			Response.ContentType = "image/jpeg";
			// Write the image to the response stream in JPEG format.
			ci.Image.Save(Response.OutputStream, ImageFormat.Jpeg);
			// Dispose of the CAPTCHA image object.
			ci.Dispose();
		}

	}
}
