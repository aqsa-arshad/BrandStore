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
                    return new List<TrackingInformation>();
                }

                else
                {
                    lstTrackingInformation.Add(new TrackingInformation()
                    {
                        OrderNumber = order.OrderNumber.ToString(),
                        TrackingNumber = order.ShippingTrackingNumber,
                        CarrierCode = string.Empty,
                        ShippingMethod = order.ShippingMethod,
                        TrackingURL = GetTrackingURL(order.ShippingTrackingNumber, order.ShippingMethod)
                    });
                }
            }
            else if (ValidateJSON(ref lstTrackingInformation, order.CartItems.FirstOrDefault().Notes))
            {
                lstTrackingInformation.ForEach(x => x.ShippingMethod = GetShippingMethod(x.CarrierCode));
                lstTrackingInformation.ForEach(x => x.TrackingURL = GetTrackingURL(x.TrackingNumber, x.ShippingMethod));
            }
            else
            {
                SysLog.LogMessage(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString() + " :: " + System.Reflection.MethodBase.GetCurrentMethod().Name,
                    "Invalid JSON", MessageTypeEnum.GeneralException, MessageSeverityEnum.Message);
                return new List<TrackingInformation>();
            }

            return lstTrackingInformation;
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
                    if (shippingMethod.Contains(listItem))
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