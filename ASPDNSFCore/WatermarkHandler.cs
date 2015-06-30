// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Drawing.Imaging;
using System.Web;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for WatermarkHandler
	/// </summary>
	public class WatermarkHandler : IHttpHandler
	{
		public WatermarkHandler() { }

		#region IHttpHandler Members

		public bool IsReusable
		{
			get { return true; }
		}

		public void ProcessRequest(HttpContext context)
		{			
			if(!AppLogic.AppConfigBool("Watermark.Enabled"))
				return;

			string querystringImageURL = CommonLogic.QueryStringCanBeDangerousContent("imgurl");
			if(string.IsNullOrEmpty(querystringImageURL))
			{
				return;
			}

			context.Response.CacheControl = "private";
			context.Response.Expires = 0;
			context.Response.AddHeader("pragma", "no-cache");
			
			string mappedImageUrl = HttpContext.Current.Request.MapPath(querystringImageURL);				
			bool sourceImageExists = CommonLogic.FileExists(mappedImageUrl) && !mappedImageUrl.Contains("nopicture.gif") && !mappedImageUrl.Contains("nopictureicon.gif");
			if(sourceImageExists)
			{
				string imgSize = CommonLogic.QueryStringCanBeDangerousContent("size");
				string copyrightText = AppLogic.AppConfig("Watermark.CopyrightText");
				string copyrightImageUrl = string.Empty;

				if(string.IsNullOrEmpty(copyrightText))
				{
					copyrightText = AppLogic.AppConfig("StoreName");
				}

				switch(imgSize.ToLower())
				{
					case "icon":
						copyrightImageUrl = AppLogic.AppConfig("Watermark.CopyrightImage.Icon");
						break;
					case "medium":
						copyrightImageUrl = AppLogic.AppConfig("Watermark.CopyrightImage.Medium");
						break;
					case "large":
						copyrightImageUrl = AppLogic.AppConfig("Watermark.CopyrightImage.Large");
						break;
					default:
						copyrightImageUrl = AppLogic.AppConfig("Watermark.CopyrightImage.Icon");
						break;
				}

				if(!CommonLogic.IsStringNullOrEmpty(copyrightImageUrl))
				{
					copyrightImageUrl = CommonLogic.IIF(copyrightImageUrl.StartsWith("/"), copyrightImageUrl.Remove(0, 1), copyrightImageUrl);
				}



				// TODO: Image must be fully qualified (System.Drawing.Image) to support VB conversion 
				// During conversion System.Drawing will be lost...must be re-added
				System.Drawing.Image imgPhoto = CommonLogic.LoadImage(mappedImageUrl);

				if(!string.IsNullOrEmpty(copyrightText) || !string.IsNullOrEmpty(copyrightImageUrl))
				{
					//If a copyright image is used and exists on disk, clear copyright text
					if(!string.IsNullOrEmpty(copyrightImageUrl) && CommonLogic.FileExists(copyrightImageUrl))
					{
						copyrightText = string.Empty;
					}
					try
					{
						imgPhoto = CommonLogic.AddWatermark(imgPhoto, copyrightText, copyrightImageUrl);
					}
					catch(Exception ex)
					{
						SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
					}
				}

				if(mappedImageUrl.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase))
				{
					context.Response.ContentType = "image/jpeg";
					EncoderParameters encoderParameters = new EncoderParameters();
					encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
					imgPhoto.Save(context.Response.OutputStream, ImageCodecInfo.GetImageEncoders()[1], encoderParameters);
				}
				if(mappedImageUrl.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase))
				{
					context.Response.ContentType = "image/jpg";
					EncoderParameters encoderParameters = new EncoderParameters();
					encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
					imgPhoto.Save(context.Response.OutputStream, ImageCodecInfo.GetImageEncoders()[1], encoderParameters);
				}
				if(mappedImageUrl.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase))
				{
					context.Response.ContentType = "image/gif";
					imgPhoto.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Gif);
				}
				if(mappedImageUrl.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
				{
					context.Response.ContentType = "image/png";
					imgPhoto.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Gif);
				}
				imgPhoto.Dispose();
			}
		}

		#endregion
	}
}
