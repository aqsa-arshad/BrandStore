// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.ServiceModel;

namespace AspDotNetStorefrontLayout.Behaviors.DataContracts
{
    [DataContract,
    Serializable]
    public class DescriptionBehaviorData : IData
    {
        public DescriptionBehaviorData()
        {
            EntityID = -1;
            EntityType = string.Empty;
        }

        [DataMember]
        public int EntityID { get; set; }

        [DataMember]
        public string EntityType { get; set; }
    }
}
