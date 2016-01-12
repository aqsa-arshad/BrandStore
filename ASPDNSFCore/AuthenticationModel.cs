using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspDotNetStorefrontCore
{
    public class UserAuthenticationModel
    {
        public string username { get; set; }
        public string password { get; set; }
    }
    public class SessionModel
    {
        public string id { get; set; }
        public string userId { get; set; }
        public bool mfaActive { get; set; }
    }

    public class UserModel
    {
        public string id { get; set; }
        public string status { get; set; }
        public DateTime? created { get; set; }
        public DateTime? activated { get; set; }
        public DateTime? statusChanged { get; set; }
        public DateTime? lastLogin { get; set; }
        public DateTime lastUpdated { get; set; }
        public DateTime? passwordChanged { get; set; }
        public string transitioningToStatus { get; set; }
        public Profile profile { get; set; }
    }

    public class UserLogin
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class Profile
    {
        public string login { get; set; }
        public string email { get; set; }
        public string secondEmail { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string honorificPrefix { get; set; }
        public string honorificSuffix { get; set; }
        public string title { get; set; }
        public string displayName { get; set; }
        public string nickName { get; set; }
        public string profileUrl { get; set; }
        public string primaryPhone { get; set; }
        public string mobilePhone { get; set; }
        public string streetAddress { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zipCode { get; set; }
        public string countryCode { get; set; }
        public string postalAddress { get; set; }
        public string preferredLanguage { get; set; }
        public string locale { get; set; }
        public string timezone { get; set; }
        public string userType { get; set; }
        public string employeeNumber { get; set; }
        public string costCenter { get; set; }
        public string organization { get; set; }
        public string division { get; set; }
        public string department { get; set; }
        public string managerId { get; set; }
        public string manager { get; set; }
        public string sfid { get; set; }
    }

    public class SFDCDealerUser
    {
        public string Customer_Number__c { get; set; }
        public string TrueBLUStatus__c { get; set; }
        public string Region__c { get; set; }
        public string Co_op_budget__c { get; set; }
        public string Display_Funds__c { get; set; }
        public string Literature_Funds__c { get; set; }
        public string POP_Funds__c { get; set; }
        public string Direct_Marketing_Funds__c { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Contact_Roles__c { get; set; }
    }

    public class SFDCInternalUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string IsActive { get; set; }
        public string Sales_Rep_ID__c { get; set; }
        public string SOF__c { get; set; }
        public string Billing_GL__c { get; set; }
    }

    public class CustomerFund
    {
        public int CustomerID { get; set; }
        public int FundID { get; set; }
        public string FundName { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountUsed { get; set; }
    }

    public class BudgetPercentageRatio
    {
        public int BudgetPercentageRatioID { get; set; }
        public int CustomerLevelID { get; set; }
        public int CategoryID { get; set; }
        public int BudgetPercentageValue { get; set; }
        public bool IsActive{ get; set; }
    }
}
