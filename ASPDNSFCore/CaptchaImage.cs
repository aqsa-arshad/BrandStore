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

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for CaptchaImage.
	/// </summary>
	public class CaptchaImage
	{
		// Public properties (all read-only).
		public string Text
		{
            get { return m_text; }
		}
		public Bitmap Image
		{
            get { return m_image; }
		}
		public int Width
		{
            get { return m_width; }
		}
		public int Height
		{
            get { return m_height; }
		}

		// Internal properties.
		private string m_text;
        private int m_width;
        private int m_height;
        private string m_familyName;
        private Bitmap m_image;

		// For generating random numbers.
		private Random random = new Random();

		// ====================================================================
		// Initializes a new instance of the CaptchaImage class using the
		// specified text, width and height.
		// ====================================================================
		public CaptchaImage(string s, int width, int height)
		{
            m_text = s;
			this.SetDimensions(width, height);
			this.GenerateImage();
		}

		// ====================================================================
		// Initializes a new instance of the CaptchaImage class using the
		// specified text, width, height and font family.
		// ====================================================================
		public CaptchaImage(string s, int width, int height, string familyName)
		{
            m_text = s;
			this.SetDimensions(width, height);
			this.SetFamilyName(familyName);
			this.GenerateImage();
		}

		// ====================================================================
		// This member overrides Object.Finalize.
		// ====================================================================
		~CaptchaImage()
		{
			Dispose(false);
		}

		// ====================================================================
		// Releases all resources used by this object.
		// ====================================================================
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			this.Dispose(true);
		}

		// ====================================================================
		// Custom Dispose method to clean up unmanaged resources.
		// ====================================================================
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Dispose of the bitmap.
                m_image.Dispose();
			}
		}

		// ====================================================================
		// Sets the image width and height.
		// ====================================================================
		private void SetDimensions(int width, int height)
		{
			// Check the width and height.
			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException("width", width, "Argument out of range, must be greater than zero.");
			}
			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException("height", height, "Argument out of range, must be greater than zero.");
			}
            m_width = width;
            m_height = height;
		}

		// ====================================================================
		// Sets the font used for the image text.
		// ====================================================================
		private void SetFamilyName(string familyName)
		{
			// If the named font is not installed, default to a system font.
			try
			{
                Font font = new Font(m_familyName, 12F);
                m_familyName = familyName;
				font.Dispose();
			}
			catch
			{
                m_familyName = System.Drawing.FontFamily.GenericSerif.Name;
			}
		}

		// ====================================================================
		// Creates the bitmap image.
		// ====================================================================
		private void GenerateImage()
		{
			// Create a new 32-bit bitmap image.
            Bitmap bitmap = new Bitmap(m_width, m_height, PixelFormat.Format32bppArgb);

			// Create a graphics object for drawing.
			Graphics g = Graphics.FromImage(bitmap);
			g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, m_width, m_height);

			// Fill in the background.
			HatchBrush hatchBrush = new HatchBrush(HatchStyle.SmallConfetti, Color.LightGray, Color.White);
			g.FillRectangle(hatchBrush, rect);

			// Set up the text font.
			SizeF size;
			float fontSize = rect.Height + 1;
			Font font;
			// Adjust the font size until the text fits within the image.
			do
			{
				fontSize--;
                font = new Font(m_familyName, fontSize, FontStyle.Bold);
                size = g.MeasureString(m_text, font);
			} while (size.Width > rect.Width);

			// Set up the text format.
			StringFormat format = new StringFormat();
			format.Alignment = StringAlignment.Center;
			format.LineAlignment = StringAlignment.Center;

			// Create a path using the text and warp it randomly.
			GraphicsPath path = new GraphicsPath();
            path.AddString(m_text, font.FontFamily, (int)font.Style, font.Size, rect, format);
			float v = 4F;
			PointF[] points =
			{
				new PointF(this.random.Next(rect.Width) / v, this.random.Next(rect.Height) / v),
				new PointF(rect.Width - this.random.Next(rect.Width) / v, this.random.Next(rect.Height) / v),
				new PointF(this.random.Next(rect.Width) / v, rect.Height - this.random.Next(rect.Height) / v),
				new PointF(rect.Width - this.random.Next(rect.Width) / v, rect.Height - this.random.Next(rect.Height) / v)
			};
			Matrix matrix = new Matrix();
			matrix.Translate(0F, 0F);
			path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);

			// Draw the text.
			hatchBrush = new HatchBrush(HatchStyle.LargeConfetti, Color.LightGray, Color.DarkGray);
			g.FillPath(hatchBrush, path);

			// Add some random noise.
			int m = Math.Max(rect.Width, rect.Height);
			for (int i = 0; i < (int) (rect.Width * rect.Height / 30F); i++)
			{
				int x = this.random.Next(rect.Width);
				int y = this.random.Next(rect.Height);
				int w = this.random.Next(m / 50);
				int h = this.random.Next(m / 50);
				g.FillEllipse(hatchBrush, x, y, w, h);
			}

			// Clean up.
			font.Dispose();
			hatchBrush.Dispose();
			g.Dispose();

			// Set the image.
            m_image = bitmap;
		}
	}
}
