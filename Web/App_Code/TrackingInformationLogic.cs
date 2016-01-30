using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AspDotNetStorefrontCore;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;

namespace AspDotNetStorefront
{
    /// <summary>
    /// TrackingInformationLogic Static Class for Get TrackingInformation for an Order
    /// </summary>
    public static class TrackingInformationLogic
    {
        public static List<TrackingInformation> GetTrackingInformation(int orderNumber)
        {
            Order order = new Order(orderNumber);
            List<TrackingInformation> lstTrackingInformation = new List<TrackingInformation>();

            if (order == null || order.CartItems.Count == 0)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "Invalid Order or Order Items.", MessageTypeEnum.GeneralException, MessageSeverityEnum.Message);

                return new List<TrackingInformation>();
            }


            if (string.IsNullOrEmpty(order.CartItems.FirstOrDefault().Notes))
            {
                if (string.IsNullOrEmpty(order.ShippingTrackingNumber))
                {
                    SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "ShippingTrackingNumber is empty.", MessageTypeEnum.GeneralException, MessageSeverityEnum.Message);
                    lstTrackingInformation.Add(new TrackingInformation()
                    {
                        OrderNumber = order.OrderNumber.ToString(),
                        TrackingNumber = string.Empty,
                        CarrierCode = string.Empty,
                        ShippingMethod = order.ShippingMethod,
                        ShippingStatus = GetShippingStatus(order.OrderNumber, order.ShippedOn.ToString()),
                        TrackingURL = string.Empty
                    });
                }

                else
                {
                    lstTrackingInformation.Add(new TrackingInformation()
                    {
                        OrderNumber = order.OrderNumber.ToString(),
                        TrackingNumber = order.ShippingTrackingNumber,
                        CarrierCode = string.Empty,
                        ShippingMethod = order.ShippingMethod,
                        ShippingStatus = GetShippingStatus(order.OrderNumber, order.ShippedOn.ToString()),
                        TrackingURL = GetTrackingURL(order.ShippingTrackingNumber, order.ShippingMethod)
                    });
                }
            }
            else if (ValidateJSON(ref lstTrackingInformation, order.CartItems.FirstOrDefault().Notes))
            {
                lstTrackingInformation.ForEach(x => x.ShippingMethod = GetShippingMethod(x.CarrierCode));

                lstTrackingInformation.ForEach(x => x.ShippingStatus = GetShippingStatus(order.OrderNumber, order.ShippedOn.ToString()));

                lstTrackingInformation.ForEach(x => x.TrackingURL = GetTrackingURL(x.TrackingNumber, x.ShippingMethod));

                if (!string.IsNullOrEmpty(order.ShippingTrackingNumber))
                {
                    lstTrackingInformation.Add(new TrackingInformation()
                    {
                        OrderNumber = order.OrderNumber.ToString(),
                        TrackingNumber = order.ShippingTrackingNumber,
                        CarrierCode = string.Empty,
                        ShippingMethod = order.ShippingMethod,
                        ShippingStatus = GetShippingStatus(order.OrderNumber, order.ShippedOn.ToString()),
                        TrackingURL = GetTrackingURL(order.ShippingTrackingNumber, order.ShippingMethod)
                    });
                }
                var tempLstTrackingInformation = new List<TrackingInformation>();
                tempLstTrackingInformation.Add(lstTrackingInformation[lstTrackingInformation.Count - 1]);
                var lstTrackingInformationCount = 0;
                foreach (var trackingInfo in lstTrackingInformation)
                {
                    if (lstTrackingInformationCount > lstTrackingInformation.Count - 1)
                        break;
                    tempLstTrackingInformation.Add(trackingInfo);
                    lstTrackingInformationCount++;
                }
                lstTrackingInformation = tempLstTrackingInformation;
            }
            else
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "Invalid JSON", MessageTypeEnum.GeneralException, MessageSeverityEnum.Message);
                return new List<TrackingInformation>();
            }

            return lstTrackingInformation;
        }

        static string GetShippingStatus(int OrderNumber, string ShippedOn)
        {
            String ShippingStatus = String.Empty;
            if (AppLogic.OrderHasShippableComponents(OrderNumber))
            {
                if (ShippedOn == "1/1/0001 12:00:00 AM" || string.IsNullOrEmpty(ShippedOn))
                    ShippingStatus = AppLogic.GetString("account.aspx.52", 3, "en-US");
                else
                {
                    ShippingStatus = AppLogic.GetString("account.aspx.48", 3, "en-US");
                }
            }
            if (AppLogic.OrderHasDownloadComponents(OrderNumber, true))
            {
                ShippingStatus += string.Format("<div><a href=\"downloads.aspx\">{0}</a></div>", AppLogic.GetString("download.aspx.1", 3, "en-US"));
            }
            if (ShippingStatus.Contains("downloads.aspx") && ShippingStatus.Contains("Not Yet Shipped"))
                ShippingStatus = "Not Yet Shipped, Downloadable";
            else if (ShippingStatus.Contains("downloads.aspx"))
            {
                ShippingStatus = "Downloadable";
            }
            return ShippingStatus;
        }

        private static bool ValidateJSON(ref List<TrackingInformation> lstTrackingInformation, string notes)
        {
            bool flag = false;
            try
            {
                lstTrackingInformation = JsonConvert.DeserializeObject<List<TrackingInformation>>(notes);
                flag = true;
            }
            catch (Exception ex)
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    ex.Message + ((ex.InnerException != null && string.IsNullOrEmpty(ex.InnerException.Message)) ? " :: " + ex.InnerException.Message : ""),
                    MessageTypeEnum.GeneralException, MessageSeverityEnum.Error);
            }
            return flag;
        }

        private static string GetShippingMethod(string CarrierCode)
        {
            string shippingMethod = string.Empty;

            using (var conn = DB.dbConn())
            {
                conn.Open();
                var query = "select Name from ShippingMethod where ShippingMethodCode = '" + CarrierCode.Trim() + "'";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    IDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        shippingMethod = reader["Name"].ToString();
                    }
                }
            }

            if (string.IsNullOrEmpty(shippingMethod))
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "Shipping Method Name is not found.", MessageTypeEnum.GeneralException, MessageSeverityEnum.Message);
            }
            return shippingMethod;
        }

        private static string GetTrackingURL(string trackingNumber, string shippingMethod)
        {
            if (string.IsNullOrEmpty(shippingMethod))
                return string.Empty;

            string trackingURL = string.Empty;

            if (AppLogic.AppConfig("RTShipping.ActiveCarrier") != null)
            {
                var carrierList = AppLogic.AppConfig("RTShipping.ActiveCarrier").Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);


                foreach (var listItem in carrierList)
                {
                    if (shippingMethod.ToUpper().Contains(listItem.ToUpper()))
                        if (!string.IsNullOrEmpty(trackingNumber))
                        {
                            trackingURL = string.Format(AppLogic.AppConfig("ShippingTrackingURL." + listItem), trackingNumber);
                        }
                }
            }

            if (string.IsNullOrEmpty(trackingURL))
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "Tracking URL is invalid.", MessageTypeEnum.GeneralException, MessageSeverityEnum.Message);
            }
            return trackingURL;
        }
    }
}