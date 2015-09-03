// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
#region Using Directives

using AspDotNetStorefront.Promotions;
using System.Collections.Generic;
using System.Configuration;

#endregion

namespace AspDotNetStorefront.Promotions.Data
{

    partial class PromotionStore
    {
    }

    public partial class EntityContextDataContext
    {
        public EntityContextDataContext()
            : this(ConfigurationManager.AppSettings["DBConn"])
        {
        }
    }

    partial class PromotionUsage : IPromotionUsage
    {
    }
    partial class Promotion : IPromotion
    {
        #region Properties

        public IEnumerable<PromotionStore> MappedStores
        {
            get
            {
                return this.PromotionStores;
            }
        }

        public List<PromotionRuleBase> PromotionRules
        {
            get { return SerializationController.DeserializeObject<List<PromotionRuleBase>>(this.PromotionRuleData); }
            set { this.PromotionRuleData = SerializationController.SerializeObject(value); }
        }

        public List<PromotionDiscountBase> PromotionDiscounts
        {
            get { return SerializationController.DeserializeObject<List<PromotionDiscountBase>>(this.PromotionDiscountData); }
            set { this.PromotionDiscountData = SerializationController.SerializeObject(value); }
        }

        #endregion
    }

    
}
