// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace AspDotNetStorefrontCore
{

	/// <summary>
	/// Download item
	/// </summary>
	public class DownloadItem
	{
		public enum DownloadItemStatus
		{
			Pending,
			Available,
			Expired
		}
		public int ShoppingCartRecordId { get; private set; }
		public int OrderNumber { get; private set; }
		public int CustomerId { get; private set; }
		public string DownloadName { get; private set; }
		public string DownloadLocation { get; private set; }
		public string DownloadCategory { get; private set; }
		public DownloadItemStatus Status { get; private set; }
		public DateTime PurchasedOn { get; private set; }
		public DateTime ReleasedOn { get; private set; }
		public DateTime ExpiresOn { get; private set; }
		public int ValidDays { get; private set; }
		public string ContentType { get; private set; }
		public string CopyToDirectory { get; private set; }

		public void Load(int shoppingCartRecordId)
		{
			List<SqlParameter> sqlParams = new List<SqlParameter>();
			sqlParams.Add(DB.CreateSQLParameter("@ShoppingCartRecID", SqlDbType.Int, 4, shoppingCartRecordId, ParameterDirection.Input));

			using (SqlConnection dbconn = DB.dbConn())
			{
				dbconn.Open();
				using (IDataReader dr = DB.GetRS("SELECT OrderNumber, CustomerId, OrderedProductVariantName, OrderedProductName, DownloadStatus, DownloadLocation, DownloadValidDays, DownloadCategory, DownloadReleasedOn, CreatedOn FROM Orders_ShoppingCart with (NOLOCK) WHERE ShoppingCartRecID = @ShoppingCartRecID", sqlParams.ToArray(), dbconn))
				{
					if (dr.Read())
					{
						ShoppingCartRecordId = shoppingCartRecordId;
						OrderNumber = DB.RSFieldInt(dr, "OrderNumber");
						CustomerId = DB.RSFieldInt(dr, "CustomerId");
                        DownloadName = DB.RSFieldByLocale(dr, "OrderedProductVariantName", Localization.GetDefaultLocale()).Length > 0 ? string.Format("{0} - {1}", DB.RSFieldByLocale(dr, "OrderedProductName", Localization.GetDefaultLocale()), DB.RSFieldByLocale(dr, "OrderedProductVariantName", Localization.GetDefaultLocale())) : DB.RSFieldByLocale(dr, "OrderedProductName", Localization.GetDefaultLocale());
						DownloadLocation = DB.RSField(dr, "DownloadLocation") ?? string.Empty;
						DownloadCategory = DB.RSField(dr, "DownloadCategory") ?? string.Empty;
						Status = (DownloadItemStatus)DB.RSFieldInt(dr, "DownloadStatus");
						ValidDays = DB.RSFieldInt(dr, "DownloadValidDays");
						PurchasedOn = DB.RSFieldDateTime(dr, "CreatedOn");
						ReleasedOn = DB.RSFieldDateTime(dr, "DownloadReleasedOn");

						if (Status != DownloadItemStatus.Pending && ValidDays > 0)
						{
							ExpiresOn = ReleasedOn.AddDays(ValidDays);

							if (DateTime.Now > ReleasedOn.AddDays(ValidDays))
								Status = DownloadItemStatus.Expired;
						}
						ContentType = DownloadLocation.Length > 0 ? GetMimeType(Path.GetExtension(DownloadLocation)) : string.Empty;

					}
				}
			}
		}

		public void Create(int orderNumber, CartItem c)
		{
			ProductVariant variant = new ProductVariant(c.VariantID);

			using (SqlConnection cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using (SqlCommand cmd = new SqlCommand(@"update orders_ShoppingCart set 								
									DownloadCategory=@DownloadCategory, 
									DownloadValidDays=@DownloadValidDays,
									DownloadLocation=@DownloadLocation,
									DownloadStatus=@DownloadStatus
									where OrderNumber=@OrderNumber and ShoppingCartRecID=@ShoppingCartRecID", cn))
				{
					cmd.Parameters.Add(new SqlParameter("@DownloadCategory", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@DownloadValidDays", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@DownloadLocation", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@DownloadStatus", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@OrderNumber", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@ShoppingCartRecID", SqlDbType.Int));

					cmd.Parameters["@DownloadCategory"].Value = AppLogic.GetFirstProductEntity(AppLogic.LookupHelper("Category", 0), c.ProductID, false, Localization.GetDefaultLocale());
					cmd.Parameters["@DownloadValidDays"].Value = variant.DownloadValidDays;
					cmd.Parameters["@DownloadLocation"].Value = variant.DownloadLocation;
					cmd.Parameters["@DownloadStatus"].Value = (int)DownloadItemStatus.Pending;
					cmd.Parameters["@OrderNumber"].Value = orderNumber;
					cmd.Parameters["@ShoppingCartRecID"].Value = c.ShoppingCartRecordID;

					cmd.ExecuteNonQuery();
				}
			}
		}

		public void Release(bool releaseMaxMindDelay)
		{
			Customer customer = new Customer(this.CustomerId);

			if (this.DownloadLocation == null || this.DownloadLocation.Length == 0)
			{
				string emailSubject = string.Format(AppLogic.GetString("notification.downloaddelayed.3", customer.SkinID, customer.LocaleSetting), AppLogic.AppConfig("StoreName"));
				string emailBody = string.Format(AppLogic.GetString("notification.downloaddelayed.4", customer.SkinID, customer.LocaleSetting), this.DownloadName, this.OrderNumber, this.CustomerId);

				NotifyAdminDelayedDownload(customer, emailSubject, emailBody);

				return;
			}

			string finalDownloadLocation = this.DownloadLocation;

			if (AppLogic.AppConfigBool("MaxMind.Enabled"))
			{
				Order order = new Order(this.OrderNumber);
				if (!releaseMaxMindDelay && order.MaxMindFraudScore >= AppLogic.AppConfigNativeDecimal("MaxMind.DelayDownloadThreshold"))
				{
					string emailSubject = string.Format(AppLogic.GetString("notification.downloaddelayed.1", customer.SkinID, customer.LocaleSetting), AppLogic.AppConfig("StoreName"));
					string emailBody = string.Format(AppLogic.GetString("notification.downloaddelayed.2", customer.SkinID, customer.LocaleSetting), this.DownloadName, this.OrderNumber, this.CustomerId);

					NotifyAdminDelayedDownload(customer, emailSubject, emailBody);
					return;
				}
			}

			if (AppLogic.AppConfigBool("Download.CopyFileForEachOrder") && !this.DownloadLocation.Contains("http:") && !this.DownloadLocation.Contains("https:"))
			{
				try
				{
					string downloadPath = CommonLogic.SafeMapPath(this.DownloadLocation);
					string filename = Path.GetFileName(downloadPath);
					string orderDownloadLocation = string.Format("~/orderdownloads/{0}_{1}", this.OrderNumber, this.CustomerId);
					string orderDownloadDirectory = CommonLogic.SafeMapPath(orderDownloadLocation);

					if (!Directory.Exists(orderDownloadDirectory))
						Directory.CreateDirectory(orderDownloadDirectory);

					string orderDownloadPath = string.Format("{0}/{1}", orderDownloadDirectory, filename);

					File.Copy(downloadPath, orderDownloadPath, true);

					finalDownloadLocation = string.Format("{0}/{1}", orderDownloadLocation, filename);
				}
				catch (Exception ex)
				{
					SysLog.LogException(ex, MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
					return;
				}
			}
			using (SqlConnection cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using (SqlCommand cmd = new SqlCommand(@"update orders_ShoppingCart set 								
									DownloadReleasedOn=@DownloadReleasedOn,
									DownloadStatus=@DownloadStatus,
									DownloadLocation=@DownloadLocation
									where ShoppingCartRecID=@ShoppingCartRecID", cn))
				{
					cmd.Parameters.Add(new SqlParameter("@DownloadReleasedOn", SqlDbType.DateTime));
					cmd.Parameters.Add(new SqlParameter("@DownloadStatus", SqlDbType.Int));
					cmd.Parameters.Add(new SqlParameter("@DownloadLocation", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ShoppingCartRecID", SqlDbType.Int));

					cmd.Parameters["@DownloadReleasedOn"].Value = DateTime.Now;
					cmd.Parameters["@DownloadStatus"].Value = (int)DownloadItemStatus.Available;
					cmd.Parameters["@DownloadLocation"].Value = finalDownloadLocation;
					cmd.Parameters["@ShoppingCartRecID"].Value = this.ShoppingCartRecordId;

					cmd.ExecuteNonQuery();
				}
			}
		}

		public void SendDownloadEmailNotification()
		{
			Customer customer = new Customer(this.CustomerId);
			string runtimeParams = string.Format("ShoppingCartRecID={0}", this.ShoppingCartRecordId);
			string subject = string.Format(AppLogic.GetString("notification.downloadreleased.1", customer.SkinID, customer.LocaleSetting), AppLogic.AppConfig("StoreName"));
			string result = AppLogic.RunXmlPackage("notification.downloadreleased.xml.config", null, customer, customer.SkinID, string.Empty, runtimeParams, false, true);
			AppLogic.SendMail(subject, result, true, AppLogic.AppConfig("GotOrderEMailFrom"), AppLogic.AppConfig("GotOrderEMailFromName"), customer.EMail, customer.FullName(), string.Empty, AppLogic.MailServer());
		}

		public void NotifyAdminDelayedDownload(Customer customer, string emailSubject, string emailBody)
		{
			if (!AppLogic.AppConfigBool("TurnOffStoreAdminEMailNotifications"))
			{
				String SendToList = AppLogic.AppConfig("GotOrderEMailTo").Replace(",", ";");
				if (SendToList.IndexOf(';') != -1)
				{
					foreach (String s in SendToList.Split(';'))
					{
						AppLogic.SendMail(emailSubject,
							emailBody + AppLogic.AppConfig("MailFooter"),
							true,
							AppLogic.AppConfig("GotOrderEMailFrom"),
							AppLogic.AppConfig("GotOrderEMailFromName"),
							s.Trim(),
							s.Trim(),
							String.Empty,
							AppLogic.MailServer());
					}
				}
				else
				{
					AppLogic.SendMail(emailSubject,
							emailBody + AppLogic.AppConfig("MailFooter"),
							true,
							AppLogic.AppConfig("GotOrderEMailFrom"),
							AppLogic.AppConfig("GotOrderEMailFromName"),
							SendToList.Trim(),
							SendToList.Trim(),
							String.Empty,
							AppLogic.MailServer());
				}
			}
			else
			{
				SysLog.LogMessage(emailSubject, emailBody, MessageTypeEnum.Informational, MessageSeverityEnum.Alert);
			}
		}

		public void UpdateDownloadLocation(string downloadLocation)
		{
			this.DownloadLocation = downloadLocation;
			using (SqlConnection cn = new SqlConnection(DB.GetDBConn()))
			{
				cn.Open();
				using (SqlCommand cmd = new SqlCommand(@"update orders_ShoppingCart set 								
									DownloadLocation=@DownloadLocation
									where ShoppingCartRecID=@ShoppingCartRecID", cn))
				{
					cmd.Parameters.Add(new SqlParameter("@DownloadLocation", SqlDbType.NText));
					cmd.Parameters.Add(new SqlParameter("@ShoppingCartRecID", SqlDbType.Int));

					cmd.Parameters["@DownloadLocation"].Value = downloadLocation;
					cmd.Parameters["@ShoppingCartRecID"].Value = this.ShoppingCartRecordId;

					cmd.ExecuteNonQuery();
				}
			}
		}

		private string GetMimeType(string fileExtension)
		{
			Dictionary<string, string> mimeTypeMappings = new Dictionary<string, string>();
			mimeTypeMappings.Add(".7z", "application/x-7z-compressed");
			mimeTypeMappings.Add(".avi", "video/x-msvideo");
			mimeTypeMappings.Add(".bmp", "image/bmp");
			mimeTypeMappings.Add(".css", "text/css");
			mimeTypeMappings.Add(".csv", "text/csv");
			mimeTypeMappings.Add(".doc", "application/msword");
			mimeTypeMappings.Add(".docm", "application/vnd.ms-word.document.macroEnabled.12");
			mimeTypeMappings.Add(".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
			mimeTypeMappings.Add(".exe", "application/octet-stream");
			mimeTypeMappings.Add(".flv", "video/x-flv");
			mimeTypeMappings.Add(".gif", "image/gif");
			mimeTypeMappings.Add(".htm", "text/html");
			mimeTypeMappings.Add(".html", "text/html");
			mimeTypeMappings.Add(".ico", "image/x-icon");
			mimeTypeMappings.Add(".jpe", "image/jpeg");
			mimeTypeMappings.Add(".jpeg", "image/jpeg");
			mimeTypeMappings.Add(".jpg", "image/jpeg");
			mimeTypeMappings.Add(".js", "application/x-javascript");
			mimeTypeMappings.Add(".mfp", "application/x-shockwave-flash");
			mimeTypeMappings.Add(".mid", "audio/mid");
			mimeTypeMappings.Add(".midi", "audio/mid");
			mimeTypeMappings.Add(".mod", "video/mpeg");
			mimeTypeMappings.Add(".mov", "video/quicktime");
			mimeTypeMappings.Add(".movie", "video/x-sgi-movie");
			mimeTypeMappings.Add(".mp2", "video/mpeg");
			mimeTypeMappings.Add(".mp2v", "video/mpeg");
			mimeTypeMappings.Add(".mp3", "audio/mpeg");
			mimeTypeMappings.Add(".mp4", "video/mp4");
			mimeTypeMappings.Add(".mp4v", "video/mp4");
			mimeTypeMappings.Add(".mpa", "video/mpeg");
			mimeTypeMappings.Add(".mpe", "video/mpeg");
			mimeTypeMappings.Add(".mpeg", "video/mpeg");
			mimeTypeMappings.Add(".mpf", "application/vnd.ms-mediapackage");
			mimeTypeMappings.Add(".mpg", "video/mpeg");
			mimeTypeMappings.Add(".mpv2", "video/mpeg");
			mimeTypeMappings.Add(".pdf", "application/pdf");
			mimeTypeMappings.Add(".pic", "image/pict");
			mimeTypeMappings.Add(".pict", "image/pict");
			mimeTypeMappings.Add(".png", "image/png");
			mimeTypeMappings.Add(".pnz", "image/png");
			mimeTypeMappings.Add(".pps", "application/vnd.ms-powerpoint");
			mimeTypeMappings.Add(".ppsm", "application/vnd.ms-powerpoint.slideshow.macroEnabled.12");
			mimeTypeMappings.Add(".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow");
			mimeTypeMappings.Add(".ppt", "application/vnd.ms-powerpoint");
			mimeTypeMappings.Add(".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12");
			mimeTypeMappings.Add(".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
			mimeTypeMappings.Add(".psd", "application/octet-stream");
			mimeTypeMappings.Add(".pub", "application/x-mspublisher");
			mimeTypeMappings.Add(".qtif", "image/x-quicktime");
			mimeTypeMappings.Add(".rtf", "application/rtf");
			mimeTypeMappings.Add(".swf", "application/x-shockwave-flash");
			mimeTypeMappings.Add(".tif", "image/tiff");
			mimeTypeMappings.Add(".txt", "text/plain");
			mimeTypeMappings.Add(".wav", "audio/wav");
			mimeTypeMappings.Add(".wave", "audio/wav");
			mimeTypeMappings.Add(".zip", "application/x-zip-compressed");

			return mimeTypeMappings.ContainsKey(fileExtension.ToLower()) ? mimeTypeMappings[fileExtension.ToLower()] : "text/plain";
		}

	}
}
