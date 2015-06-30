// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using AspDotNetStorefrontCore;
using GatewayAuthorizeNet.AuthorizeNetApi;

namespace GatewayAuthorizeNet
{
	public class DataUtility
	{
		public static Int64 GetProfileId(int customerId)
		{
			return DB.GetSqlNLong(String.Format("select top 1 CIM_ProfileId as N from customer where CustomerID = {0}", customerId));
		}

		public static void SaveProfileId(int customerId, Int64 profileId)
		{
			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(SqlCommand command = new SqlCommand("update customer set CIM_ProfileId = @profileId where CustomerID = @customerId", connection))
				{
					command.Parameters.AddRange(new[] {
						new SqlParameter("@profileId", profileId),
						new SqlParameter("@customerId", customerId),
					});

					command.ExecuteNonQuery();
				}
			}
		}

		public static AddressPaymentProfileMap GetPaymentProfile(int customerId, Int64 paymentProfileId)
		{
			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(SqlCommand command = new SqlCommand("select top 1 AuthorizeNetProfileId, CardType, ExpirationMonth, ExpirationYear, AddressId from CIM_AddressPaymentProfileMap where CustomerId = @customerId and AuthorizeNetProfileID = @paymentProfileId", connection))
				{
					command.Parameters.AddRange(new[]
					{
						new SqlParameter("@customerId", customerId),
						new SqlParameter("@paymentProfileId", paymentProfileId),
					});

					using(var reader = command.ExecuteReader())
					{
						if(!reader.Read())
							return null;

						return new AddressPaymentProfileMap(
							reader.GetInt64(reader.GetOrdinal("AuthorizeNetProfileId")),
							reader.GetString(reader.GetOrdinal("CardType")),
							reader.GetString(reader.GetOrdinal("ExpirationMonth")),
							reader.GetString(reader.GetOrdinal("ExpirationYear")),
							reader.GetInt32(reader.GetOrdinal("AddressId")));
					}
				}
			}
		}

		public static void DeletePaymentProfile(int customerId, Int64 paymentProfileId)
		{
			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(SqlCommand command = new SqlCommand("delete from CIM_AddressPaymentProfileMap where CustomerId = @customerId and AuthorizeNetProfileId = @authorizeNetProfileId", connection))
				{
					command.Parameters.AddRange(new[] {
						new SqlParameter("@customerId", customerId),
						new SqlParameter("@authorizeNetProfileId", paymentProfileId),
					});

					command.ExecuteNonQuery();
				}
			}
		}

		public static void SavePaymentProfile(int customerid, int addressId, Int64 paymentProfileId, string expirationMonth, string expirationYear, string cardType)
		{
			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(SqlCommand command = new SqlCommand(
					@"if exists(select * from CIM_AddressPaymentProfileMap where CustomerId = @customerId and AuthorizeNetProfileId = @authorizeNetProfileId) 
						update CIM_AddressPaymentProfileMap set AddressId = @addressId, ExpirationMonth = @expirationMonth, ExpirationYear = @expirationYear where CustomerId = @customerId and AuthorizeNetProfileId = @authorizeNetProfileId
					else
						insert into CIM_AddressPaymentProfileMap (CustomerId, AuthorizeNetProfileId, CardType, AddressId, ExpirationMonth, ExpirationYear) values (@customerId, @authorizeNetProfileId, @cardType, @addressId, @expirationMonth, @expirationYear)", connection))
				{
					command.Parameters.AddRange(new[] {
						new SqlParameter("@customerId", customerid),
						new SqlParameter("@authorizeNetProfileId", paymentProfileId),
						new SqlParameter("@cardType", cardType),
						new SqlParameter("@addressId", addressId),
						new SqlParameter("@expirationMonth", expirationMonth),
						new SqlParameter("@expirationYear", expirationYear),
					});

					command.ExecuteNonQuery();
				}
			}
		}

		public static void SetPrimaryPaymentProfile(int customerId, Int64 paymentProfileId)
		{
			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(SqlCommand command = new SqlCommand("update CIM_AddressPaymentProfileMap set [Primary] = case when AuthorizeNetProfileId = @authorizeNetProfileId then 1 else 0 end where CustomerId = @customerId", connection))
				{
					command.Parameters.AddRange(new[] {
						new SqlParameter("@customerId", customerId),
						new SqlParameter("@authorizeNetProfileId", paymentProfileId),
					});

					command.ExecuteNonQuery();
				}
			}
		}

		public static PaymentProfileWrapper GetPaymentProfileWrapper(int customerId, string email, Int64 paymentProfileId)
		{
			long profileID = DataUtility.GetProfileId(customerId);
			var profileMgr = new ProfileManager(customerId, email, profileID);
			var paymentProfile = profileMgr.GetPaymentProfile(paymentProfileId);
			var creditCardMasked = (CreditCardMaskedType)paymentProfile.payment.Item;
			var cimPaymentProfile = DataUtility.GetPaymentProfile(customerId, paymentProfileId);

			if (creditCardMasked != null && cimPaymentProfile != null)
			{
				return new PaymentProfileWrapper()
				{
					CreditCardNumberMasked = creditCardMasked.cardNumber.Replace("XXXX", "****"),
					CardType = cimPaymentProfile.CardType,
					ExpirationMonth = cimPaymentProfile.ExpirationMonth,
					ExpirationYear = cimPaymentProfile.ExpirationYear,
					CustomerId = customerId,
					ProfileId = profileID
				};
			}
			return null;
		}

		public static IEnumerable<PaymentProfileWrapper> GetPaymentProfiles(int customerId, string email)
		{
			Int64 profileId = DataUtility.GetProfileId(customerId);

			if(profileId <= 0)
				yield break;

			var profileMgr = new ProfileManager(customerId, email, profileId);

			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(SqlCommand command = new SqlCommand("select AuthorizeNetProfileId, CardType, ExpirationMonth, ExpirationYear from CIM_AddressPaymentProfileMap where CustomerId = @customerId", connection))
				{
					command.Parameters.AddRange(new[] {
						new SqlParameter("@customerId", customerId),
					});

					using(SqlDataReader reader = command.ExecuteReader())
					{
						int authorizeNetProfileIdColumn = reader.GetOrdinal("AuthorizeNetProfileId");
						int cardTypeColumn = reader.GetOrdinal("CardType");
						int expirationMonthColumn = reader.GetOrdinal("ExpirationMonth");
						int expirationYearColumn = reader.GetOrdinal("ExpirationYear");

						while(reader.Read())
						{
							long authorizeNetProfileId = reader.GetInt64(authorizeNetProfileIdColumn);
							string cardType = reader.GetString(cardTypeColumn);
							string expirationMonth = reader.GetString(expirationMonthColumn);
							string expirationYear = reader.GetString(expirationYearColumn);

							var cimProfile = profileMgr.GetPaymentProfile(authorizeNetProfileId);
							if(cimProfile == null)
								continue;

							CreditCardMaskedType maskedCard = (CreditCardMaskedType)cimProfile.payment.Item;

							yield return new PaymentProfileWrapper()
							{
								CreditCardNumberMasked = string.Format("**** **** **** {0}", maskedCard.cardNumber.Substring(4)),
								CardType = cardType,
								ExpirationMonth = expirationMonth,
								ExpirationYear = expirationYear,
								CustomerId = customerId,
								ProfileId = authorizeNetProfileId,
							};
						}
					}
				}
			}
		}

		public static CustomerAddressType GetCustomerAddressFromAddress(int addressId)
		{
			using(SqlConnection connection = new SqlConnection(DB.GetDBConn()))
			{
				connection.Open();

				using(SqlCommand command = new SqlCommand("select top 1 FirstName, LastName, Company, Address1, City, State, Zip, Country, Phone from Address where AddressID = @addressId", connection))
				{
					command.Parameters.AddRange(new[] {
						new SqlParameter("@addressId", addressId),
					});

					using(SqlDataReader reader = command.ExecuteReader())
					{
						if(!reader.Read())
							return null;

						return new GatewayAuthorizeNet.AuthorizeNetApi.CustomerAddressType()
						{
							firstName = reader.GetString(reader.GetOrdinal("FirstName")),
							lastName = reader.GetString(reader.GetOrdinal("LastName")),
							company = reader.GetString(reader.GetOrdinal("Company")),
							address = reader.GetString(reader.GetOrdinal("Address1")),
							city = reader.GetString(reader.GetOrdinal("City")),
							state = reader.GetString(reader.GetOrdinal("State")),
							zip = reader.GetString(reader.GetOrdinal("Zip")),
							country = reader.GetString(reader.GetOrdinal("Country")),
							phoneNumber = reader.GetString(reader.GetOrdinal("Phone")),
						};
					}
				}
			}
		}
	}
}
