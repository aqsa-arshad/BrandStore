// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Globalization;

namespace AspDotNetStorefrontCore
{
	/// <summary>
	/// Summary description for SMS.
	/// </summary>
	public class SMS
	{
		public SMS() {}

		public static String MakeProperPhoneFormat2(String PhoneNumber)
		{
			if(PhoneNumber.Length == 0)
			{
				return String.Empty;
			}
			if(PhoneNumber.Substring(0,1) != "1")
			{
				PhoneNumber = "1" + PhoneNumber;
			}
			String newS = String.Empty;
			String validDigits = "0123456789";
			for(int i = 1; i<= PhoneNumber.Length; i++)
			{
				if(validDigits.IndexOf(PhoneNumber[i-1]) != -1)
				{
					newS = newS + PhoneNumber[i-1];
				}
			}
			return newS;
		}

        static public void Send(Order order, String FromEMailAddress, String MailServer, Customer ViewingCustomer)
		{
				// change in v6.1, the Dispatch_ToPhoneNumber must now be the full cell message address (e.g. 4805551212@mmode.com) or whatever
				// is needed. You can also dispatch to multiple cells by separating them with a comma.
				String SMSTo = AppLogic.AppConfig("Dispatch_ToPhoneNumber");
				if(SMSTo.Length != 0)
				{
					Decimal OrderThreshold = System.Decimal.Zero;
					if(AppLogic.AppConfig("Dispatch_OrderThreshold").Length != 0)
					{
						try
						{
							String s = AppLogic.AppConfig("Dispatch_OrderThreshold").Replace("$",""); // strip the $ out if present
							OrderThreshold = Localization.ParseUSDecimal(s);
						}
						catch {}
					}
					if(order.Total(true) >= OrderThreshold)
					{
						try
						{
							SMSTo = SMSTo.Replace(";",",");
							String SMSSubject = AppLogic.AppConfig("Dispatch_SiteName");
							if(SMSSubject.Length != 0)
							{
								SMSSubject = AppLogic.GetString("sms.cs.1",1,Localization.GetDefaultLocale());
							}
                           	String PackageName = AppLogic.AppConfig("XmlPackage.NewOrderAdminSMSNotification");
                            if (PackageName.Length != 0)
                            {
                                String SMSBody = AppLogic.RunXmlPackage(PackageName, null, null, order.SkinID, String.Empty, "OrderNumber=" + order.OrderNumber.ToString(), false, false);
                                if (SMSBody.Length > AppLogic.AppConfigUSInt("Dispatch_MAX_SMS_MSG_LENGTH"))
                                {
                                    SMSBody = SMSBody.Substring(0, AppLogic.AppConfigUSInt("Dispatch_MAX_SMS_MSG_LENGTH"));
                                }
                                string s = String.Empty;
                                string[] sAry = SMSTo.Split(',');
                                for (int i = 0; i < sAry.Length; i++)
                                {
                                    String s2 = sAry[i].Trim();
                                    if (s2.Length != 0)
                                    {
                                        AppLogic.SendMail(SMSSubject, SMSBody, false, FromEMailAddress, FromEMailAddress, s2, s2, "", MailServer);
                                    }
                                }
                            }
						}
						catch {}
					}
			}
		}
	}
}
