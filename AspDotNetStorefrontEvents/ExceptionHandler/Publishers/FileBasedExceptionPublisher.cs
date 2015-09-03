// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontEventHandlers.ExceptionHandlers.Publishers
{
    public class FileBasedExceptionPublisher : ExceptionPublisher
    {
        public override void Publish(string errorCode, string error)
        {
            string dumpDirectory = CommonLogic.SafeMapPath("~/images/errors");
            if (!Directory.Exists(dumpDirectory))
            {
                Directory.CreateDirectory(dumpDirectory);
            }

            string filePath = CommonLogic.SafeMapPath(
                                string.Format("{0}/{1}_{2}.txt",
                                    dumpDirectory,
                                    DateTime.Now.ToString("MM-dd-yyy_hhmmss"),
                                    errorCode));

            File.WriteAllText(filePath, error);
        }
    }
}
