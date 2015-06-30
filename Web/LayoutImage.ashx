<%@ WebHandler Language="C#" Class="LayoutImage" %>

using System;
using System.Web;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using AspDotNetStorefrontCore;
using AspDotNetStorefrontLayout;

public class LayoutImage : IHttpHandler
{
    private LayoutField m_thisfield;
    public LayoutField ThisField
    {
        get { return m_thisfield; }
        set { m_thisfield = value; }
    }

    public int Width
    {
        get
        {
            var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(ThisField.LayoutFieldAttributes
                .SingleOrDefault(lf => lf.Name.Equals("width", StringComparison.OrdinalIgnoreCase)),
                new LayoutFieldAttribute { Name = "width", Value = "250" });

            return int.Parse(lfa.Value);
        }
    }

    public int Height
    {
        get
        {
            var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(ThisField.LayoutFieldAttributes
                .SingleOrDefault(lf => lf.Name.Equals("height", StringComparison.OrdinalIgnoreCase)),
                new LayoutFieldAttribute { Name = "height", Value = "250" });

            return int.Parse(lfa.Value);
        }
    }
    
    public String ImageID
    {
        get
        {
            var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(ThisField.LayoutFieldAttributes
                .SingleOrDefault(lf => lf.Name.Equals("id", StringComparison.OrdinalIgnoreCase)),
                new LayoutFieldAttribute { Name = "defaultid", Value = "img" });

            return lfa.Value;
        }
    }
    
    public String Source
    {
        get
        {
            var lfa = (LayoutFieldAttribute)CommonLogic.IsNull(ThisField.LayoutFieldAttributes
                .SingleOrDefault(lf => lf.Name.Equals("source", StringComparison.OrdinalIgnoreCase)),
                new LayoutFieldAttribute { Name = "defaultsource", Value = "grey.png" });

            return lfa.Value;

        }
    }
    
    public void ProcessRequest (HttpContext context)
    {
        Image img;

        int imgWidth = 0;
        int imgHeight = 0;
        
        String imgContentType = "image/png";
        ImageFormat imgFormat = ImageFormat.Png;
        String imgText = "Default Image\nimg1\nwidth x height";
        
        int lfID = CommonLogic.QueryStringUSInt("layoutfieldid");

        if (lfID == 0)
        {
            img = Image.FromFile(CommonLogic.SafeMapPath("~/images/grey.png"));

            imgWidth = 250;
            imgHeight = 250;
            
            ThisField = null;
        }
        else
        {
            LayoutField lf = new LayoutField(lfID);
            
            ThisField = lf;
            String fName = CommonLogic.SafeMapPath("~/images/layouts/" + ThisField.LayoutID.ToString() + "/" + this.Source);

            if (CommonLogic.FileExists(fName))
            {
                img = Image.FromFile(CommonLogic.SafeMapPath("~/images/layouts/" + ThisField.LayoutID.ToString() + "/" + this.Source));

                imgWidth = img.Width;
                imgHeight = img.Height;
            }
            else
            {
                img = Image.FromFile(CommonLogic.SafeMapPath("~/images/grey.png"));

                imgWidth = this.Width;
                imgHeight = this.Height;
            }
            
            imgText = this.ImageID;
            imgText += "\n" + this.Source;
            imgText += "\n" + imgWidth.ToString() + " x " + imgHeight.ToString();
        }

        using (Bitmap b = new Bitmap(img, imgWidth, imgHeight))
        {

            using (Graphics grPhoto = Graphics.FromImage(b))
            {
                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.SmoothingMode = SmoothingMode.HighQuality;
                grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
                grPhoto.CompositingQuality = CompositingQuality.HighQuality;

                Font crFont = null;
                SizeF crSize = new SizeF();

                int[] sizes = new int[] { 16, 14, 12, 10, 8, 6, 4 };

                for (int i = 0; i < 7; i++)
                {
                    crFont = new Font("arial", sizes[i], FontStyle.Bold);
                    crSize = grPhoto.MeasureString(imgText, crFont);

                    if ((ushort)crSize.Width < (ushort)imgWidth)
                    {
                        break;
                    }
                }

                int OffsetPercentage = 55;

                int yPixlesFromBottom = (int)(imgHeight * (OffsetPercentage / 100.0));

                float yPosFromBottom = ((imgHeight - yPixlesFromBottom) - (crSize.Height / 2.0F));

                float xCenterOfImg = (imgWidth / 2);

                StringFormat StrFormat = new StringFormat();
                StrFormat.Alignment = StringAlignment.Center;

                SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));

                grPhoto.DrawString(imgText,
                    crFont,
                    semiTransBrush,
                    new PointF(xCenterOfImg, yPosFromBottom),
                    StrFormat);

                context.Response.ContentType = imgContentType;
                b.Save(context.Response.OutputStream, imgFormat);

                grPhoto.Dispose();
            }
            
            b.Dispose();
        }
        
    }
 
    public bool IsReusable
    {
        get { return true; }
    }

}