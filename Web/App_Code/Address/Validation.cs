// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Reflection;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefront
{

        /********************************
        * 
        *   This is a partial class file, look for related files in this subdirectory.
        *   One such file is uspsValidation.cs, you can use that as a model to add 
        *   your own AddressVerification provider if you'd like.
        * 
        ********************************/

    public partial class AddressValidation
    {
        public AddressValidation()
        {
        }

        /// <summary>
        /// Validate Address using API specified in AppConfig VerifyAddressesProvider.
        /// <para>This static public method is to be used without requiring the declaration
        /// of an AddressValidation instance.</para>
        /// </summary>
        /// <param name="EnteredAddress">The address as entered by a customer</param>
        /// <param name="ResultAddress">The resulting validated address</param>
        /// <returns>String,
        /// ro_OK => ResultAddress = EnteredAddress proceed with no further user review,
        /// 'some message' => address requires edit or verification by customer
        /// </returns>
        static public String RunValidate(Address EnteredAddress, out Address ResultAddress)
        {
            AddressValidation av = new AddressValidation();
            return av.Validate(EnteredAddress, out ResultAddress);
        }

        /// <summary>
        /// Validate Address using API specified in AppConfig VerifyAddressesProvider.
        /// <seealso cref="RunValidate"/>
        /// </summary>
        /// <param name="EnteredAddress">The address as entered by a customer</param>
        /// <param name="ResultAddress">The resulting validated address</param>
        /// <returns>String,
        /// ro_OK => ResultAddress = EnteredAddress proceed with no further user review,
        /// 'some message' => address requires edit or verification by customer
        /// </returns>
        public String Validate(Address EnteredAddress, out Address ResultAddress)
        {
            ResultAddress = new Address();
            String result = AppLogic.ro_OK;

            string strService = AppLogic.AppConfig("VerifyAddressesProvider").ToLowerInvariant();

            if (strService.Length == 0)
            {
                ResultAddress.LoadFromDB(EnteredAddress.AddressID);
                return result;
            }

            strService += "Validate";
            try
            {
                Type thisType = this.GetType();
                MethodInfo theMethod = thisType.GetMethod(strService);

                if (theMethod == null || (!CheckMethod(theMethod)))
                {
                    return "Error: " + strService + " not supported.";
                }

                object[] parms = new object[2];
                parms[0] = EnteredAddress;
                parms[1] = ResultAddress;
                result = (string)theMethod.Invoke(this, parms);
                ResultAddress = (Address)parms[1];

            }
            catch 
            {
                // This exception is from strService not existing
                return "Error: " + strService + " does not exist.";
            }
            return result;
        }

        // This method makes sure that the method being called
        // is Public, Not Static and Not Inherited. This is all done
        // by checking properties of the reflection method description
        // that is encapsulated in the MethodInfo object.
        private bool CheckMethod(MethodInfo method)
        {
            if (!method.IsPublic)
            {
                return false;
            }
            if (method.IsStatic)
            {
                return false;
            }
            if (method.DeclaringType != this.GetType())
            {
                return false;
            }
            return true;
        }


    }
}
