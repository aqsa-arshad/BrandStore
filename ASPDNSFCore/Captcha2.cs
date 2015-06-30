// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text.RegularExpressions;

namespace AspDotNetStorefrontCore
{
    /// <summary>
    /// Summary description for Captcha2.
    /// </summary>
    public class Captcha2
    {
        #region Properties

        // private captcha properties
        private Bitmap m_cImage;
        private String m_SecurityCode;
        private int m_width;
        private int m_height;
        private Color m_cImageForeColor;
        private Color m_cImageBackColor;
        private Color m_textForeColor;
        private Color m_textBackColor;
        private Color m_horizontalColor;
        private Color m_verticalColor;

        // For generating random numbers.
        private Random rnd = new Random();

        // public captcha properties (read-only)
        public Bitmap Image
        {
            get { return m_cImage; }
        }

        #endregion

        #region Contructors

        /// <summary>
        /// Generate captcha image with specified security code
        /// </summary>
        /// <param name="iSecurityCode">Text for image</param>
        /// <param name="iWidth">Width of image</param>
        /// <param name="iHeight">Height of image</param>
        /// <param name="isPostBack">Whether or not the captcha is being generated as a result of a page postback</param>
        public Captcha2(String iSecurityCode, int iWidth, int iHeight, Boolean isPostBack)
        {
            m_SecurityCode = iSecurityCode;

            m_width = iWidth;
            m_height = iHeight;

            InitializeProperties();
            GenerateColorCaptcha();
        }

        /// <summary>
        /// Generate captcha image with a random security code
        /// </summary>
        /// <param name="iSecurityCode">Text for image</param>
        /// <param name="iWidth">Width of image</param>
        /// <param name="iHeight">height of image</param>
        public Captcha2(out String iSecurityCode, int iWidth, int iHeight)
        {
            m_SecurityCode = iSecurityCode = GenerateSecurityCode();

            m_width = iWidth;
            m_height = iHeight;

            InitializeProperties();
            GenerateColorCaptcha();
        }


        #endregion

        #region Methods

        /// <summary>
        /// Generate captcha image with colors and random noise
        /// </summary>
        private void GenerateColorCaptcha()
        {
            Bitmap bmp = new Bitmap(m_width, m_height, PixelFormat.Format32bppArgb);

            Graphics grph = Graphics.FromImage(bmp);
            grph.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, m_width, m_height);

            HatchBrush hb = new HatchBrush(HatchStyle.SmallGrid, m_cImageForeColor, m_cImageBackColor);
            GraphicsPath gPath = new GraphicsPath();
            
            float fontSize = rect.Height - 10;
            Font captchaFont = new Font("Century", fontSize, FontStyle.Bold);
            SizeF textSize = grph.MeasureString(m_SecurityCode, captchaFont);
            StringFormat strformat = new StringFormat();
            strformat.Alignment = StringAlignment.Center;
            strformat.LineAlignment = StringAlignment.Center;

            grph.FillRectangle(hb, rect);

            // generate random horizontal alpha-fading colored lines for noise
            for (int i = 0; i < 6; i++)
            {
                try
                {
                    int xPoint1 = rnd.Next(m_width);
                    int xPoint2 = rnd.Next(m_width);
                    int yPoint = rnd.Next(m_height);

                    LinearGradientBrush lgb = new LinearGradientBrush(
                        new PointF(xPoint1, yPoint),
                        new PointF(xPoint2, yPoint),
                        Color.FromArgb(125, m_horizontalColor),
                        Color.FromArgb(10, m_horizontalColor));

                    Pen pen1 = new Pen(lgb, 5);

                    grph.DrawLine(pen1, new Point(xPoint1, yPoint), new Point(xPoint2, yPoint));
                    
                    lgb.Dispose();
                    pen1.Dispose();
                }
                catch { }
            }

            // generate random vertical alpha-fading colored lines for noise
            for (int j = 0; j < 6; j++)
            {
                try
                {
                    int yPoint1 = rnd.Next(m_height);
                    int yPoint2 = rnd.Next(m_height);
                    int xPoint = rnd.Next(m_width);

                    LinearGradientBrush lgb = new LinearGradientBrush(
                        new PointF(xPoint, yPoint1),
                        new PointF(xPoint, yPoint2),
                        Color.FromArgb(125, m_verticalColor),
                        Color.FromArgb(10, m_verticalColor));

                    Pen pen1 = new Pen(lgb, 5);

                    grph.DrawLine(pen1, new Point(xPoint, yPoint1), new Point(xPoint, yPoint2));

                    lgb.Dispose();
                    pen1.Dispose();
                }
                catch { }
            }

            float wDif = textSize.Width - rect.Width;
            int k = 0;

            while(wDif > 0.0F)
            {
                k++;
                captchaFont = new Font("Century", fontSize - k, FontStyle.Bold);
                wDif = grph.MeasureString(m_SecurityCode, captchaFont).Width - rect.Width;
            }


            gPath.AddString(m_SecurityCode, captchaFont.FontFamily, (int)captchaFont.Style, captchaFont.Size, rect, strformat);
            
            PointF[] transformPoints =
			{
				new PointF(rnd.Next(m_width) / 4, rnd.Next(m_height) / 4),
				new PointF(rect.Width - rnd.Next(rect.Width) / 4, rnd.Next(rect.Height) / 4),
				new PointF(rnd.Next(rect.Width) / 4, rect.Height - rnd.Next(rect.Height) / 4),
				new PointF(rect.Width - rnd.Next(rect.Width) / 4, rect.Height - rnd.Next(rect.Height) / 4)
			};

            gPath.Warp(transformPoints, rect);

            hb = new HatchBrush(HatchStyle.Percent90, m_textForeColor, m_textBackColor);
            grph.FillPath(hb, gPath);

            // generate random beziers for noise
            Pen bezierPen = new Pen(Color.FromArgb(175, m_cImageForeColor), .5F);

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    PointF[] bezierPoints =
		            {
			            new PointF(rnd.Next(m_width),rnd.Next(m_height)),
			            new PointF(rnd.Next(m_width),rnd.Next(m_height)),
			            new PointF(rnd.Next(m_width),rnd.Next(m_height)),
			            new PointF(rnd.Next(m_width),rnd.Next(m_height))
		            };

                    grph.DrawBeziers(bezierPen, bezierPoints);
                }
                catch { }
            }

            bezierPen.Dispose();
            captchaFont.Dispose();
            strformat.Dispose();
            gPath.Dispose();
            grph.Dispose();
            hb.Dispose();

            m_cImage = bmp;
        }

        /// <summary>
        /// Initialize Captcha2 properties
        /// </summary>
        private void InitializeProperties()
        {
            if (AppLogic.AppConfig("Captcha.ImageBackColor").IndexOf("#") != -1)
            {
                m_cImageBackColor = ColorTranslator.FromHtml(AppLogic.AppConfig("Captcha.ImageBackColor"));
            }
            else
            {
                m_cImageBackColor = Color.FromName(AppLogic.AppConfig("Captcha.ImageBackColor"));
            }

            if (AppLogic.AppConfig("Captcha.ImageForeColor").IndexOf("#") != -1)
            {
                m_cImageForeColor = ColorTranslator.FromHtml(AppLogic.AppConfig("Captcha.ImageForeColor"));
            }
            else
            {
                m_cImageForeColor = Color.FromName(AppLogic.AppConfig("Captcha.ImageForeColor"));
            }

            if (AppLogic.AppConfig("Captcha.TextBackColor").IndexOf("#") != -1)
            {
                m_textBackColor = ColorTranslator.FromHtml(AppLogic.AppConfig("Captcha.TextBackColor"));
            }
            else
            {
                m_textBackColor = Color.FromName(AppLogic.AppConfig("Captcha.TextBackColor"));
            }

            if (AppLogic.AppConfig("Captcha.TextForeColor").IndexOf("#") != -1)
            {
                m_textForeColor = ColorTranslator.FromHtml(AppLogic.AppConfig("Captcha.TextForeColor"));
            }
            else
            {
                m_textForeColor = Color.FromName(AppLogic.AppConfig("Captcha.TextForeColor"));
            }

            if (AppLogic.AppConfig("Captcha.HorizontalColor").IndexOf("#") != -1)
            {
                m_horizontalColor = ColorTranslator.FromHtml(AppLogic.AppConfig("Captcha.HorizontalColor"));
            }
            else
            {
                m_horizontalColor = Color.FromName(AppLogic.AppConfig("Captcha.HorizontalColor"));
            }

            if (AppLogic.AppConfig("Captcha.VerticalColor").IndexOf("#") != -1)
            {
                m_verticalColor = ColorTranslator.FromHtml(AppLogic.AppConfig("Captcha.VerticalColor"));
            }
            else
            {
                m_verticalColor = Color.FromName(AppLogic.AppConfig("Captcha.VerticalColor"));
            }
        }

        /// <summary>
        /// Generate a random security code
        /// </summary>
        /// <returns>An alpha-numeric string</returns>
        public String GenerateSecurityCode()
        {
            int maxTries = 1000;
            int i = 0;

            String randomCode = String.Empty;
            Random rnd = new Random();
            int maxAscii = AppLogic.AppConfigNativeInt("Captcha.MaxAsciiValue");
            int rndAscii;

            Regex rex = new Regex(CommonLogic.IIF(AppLogic.AppConfig("Captcha.AllowedCharactersRegex").Length == 0, "@[0-9]", AppLogic.AppConfig("Captcha.AllowedCharactersRegex")));

            int codeLength = AppLogic.AppConfigNativeInt("Captcha.NumberOfCharacters");

            if (codeLength < 6)
            {
                codeLength = 6;
            }

            if (codeLength > 20)
            {
                codeLength = 20;
            }

            if (maxAscii == 0)
            {
                maxAscii = 126;
            }

            try
            {
                do
                {
                    // get a random ascii value
                    rndAscii = rnd.Next(33, maxAscii);

                    // determine if the character is allowed via the regex
                    if (rex.Match(char.ConvertFromUtf32(rndAscii)).Success)
                    {
                        randomCode += char.ConvertFromUtf32(rndAscii);
                    }

                    i++;
                }
                while (randomCode.Length < codeLength && i < maxTries);
            }
            catch
            {
                // default it to something in case the Captcha.AllowedCharactersRegex
                // causes the match to break;
                randomCode = "934JWC";
            }
            finally
            {
                // double check the length in case Captcha.AllowedCharactersRegex
                // is configured in such a way that no characters are ever matched
                // or the match has reached the maxTries
                if (randomCode.Length < codeLength)
                {
                    randomCode = "934JWC";
                }
            }

            return randomCode;
        }

        #endregion
    }

}
