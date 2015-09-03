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

namespace AspDotNetStorefrontLayout.Behaviors
{
    public interface IData
    {
    }

    public interface IBehavior
    {
        string ID { get; set; }
        string Name { get; }
        LayoutPanel Container { get; set; }
        LayoutHost Host { get; set; }
        bool IsFirstLoad { get; set; }
        void Initialize();
    }

    public interface IBehaviorData<T> : IBehavior where T : IData
    {
        T Data { get; set; }
    }
}



