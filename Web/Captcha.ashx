<%@ WebHandler Language="C#" Class="Captcha" %>

using System;
using System.Web;
using System.Web.SessionState;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text.RegularExpressions;
using AspDotNetStorefrontCommon;
using AspDotNetStorefrontCore;

public class Captcha : IHttpHandler, IRequiresSessionState
{
    
    public void ProcessRequest (HttpContext hCtxt) {
        
        try
        {
            int id = Int32.Parse(hCtxt.Request.QueryString["id"]);

            String SecurityCode = String.Empty;

            Captcha2 c2;

            if (id == 2)
            {
                SecurityCode = hCtxt.Session["SecurityCode"].ToString();
                c2 = new Captcha2(SecurityCode, 200, 50, true);
            }
            else
            {
                c2 = new Captcha2(out SecurityCode, 200, 50);
                hCtxt.Session["SecurityCode"] = SecurityCode;
            }
            
            Image cImage = c2.Image;

            hCtxt.Response.ContentType = "image/jpeg";
            cImage.Save(hCtxt.Response.OutputStream, ImageFormat.Jpeg);
        }
        catch
        {
            hCtxt.Response.ContentType = "text/plaintext";
            hCtxt.Response.Write("");
        }
        
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }
    
    

}