<%@ Page Language="C#" AutoEventWireup="false" EnableTheming="false" %>

<html>
    <head runat="server" visible="false">

<script runat="server">

private const string DotFeedPublicKey = "<RSAKeyValue><Modulus>2792gV8Hyld7hYNdouEcEfaKquKEZzPMv6iFJIYm0Va4XbXecTEHXKY/sdv03+lxANRc9EbZ0unJHNrSfTDkeRDCgbokce7Yzc0IIOVMHgjwLoVrCjFyWW0mXteBKm65Rqvjm2FGqjOCcPHoBG3G01sKw40aaQB+FmjNfD8OSvE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
	private const string AccessKeyKey = "DotFeed.AccessKey";
	private readonly TimeSpan AuthWindow = new TimeSpan(0, 1, 0);

    private string _ConfiguredAccessKey;
	private string ConfiguredAccessKey
    {
        get { return _ConfiguredAccessKey; }
        set { _ConfiguredAccessKey = value; }
    }

    private string _RawAuthDateParam;
	private string RawAuthDateParam
    { 
        get { return _RawAuthDateParam; }
        set { _RawAuthDateParam = value; }
    }

    private DateTime _AuthDate;
	private DateTime AuthDate
	{
        get { return _AuthDate; }
        set { _AuthDate = value; }
    }

    private byte[] _ExtendedAuthToken;
    private byte[] ExtendedAuthToken
	{
        get { return _ExtendedAuthToken; }
        set { _ExtendedAuthToken = value; }
    }

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);

		DateTime requestDate = DateTime.Now;

        Response.Clear();
        
		VerifyConfiguration();
		VerifyParameters();

        string contents;
        using (System.IO.StreamReader reader = new System.IO.StreamReader(Request.InputStream))
        {
            contents = reader.ReadToEnd();
            Request.InputStream.Position = 0;

            VerifyAuthentication(requestDate, contents);

            string result = ProcessXmlPackage();
            Response.Write(result);
        }
       
        Response.End();
	}

	private void VerifyConfiguration()
	{
        if (!AppConfigExists(AccessKeyKey))
        {
            CreateAppConfig(AccessKeyKey, "The key you provide to DotFeed to allow it to access your site data.", String.Empty, "DOTFEED", false);
            
            if (StoreID() != null)
                ExecuteSql("UPDATE AppConfig SET StoreID=0 WHERE Name = " + QuoteSql(AccessKeyKey));
        }

        string configuredAccessKeyValue = (GetAppConfig(AccessKeyKey) ?? String.Empty).Trim();

		if(String.IsNullOrEmpty(configuredAccessKeyValue))
		{
			ReturnErrorMessage("DotFeed has not been enabled on this store");
			Response.End();
		}

		ConfiguredAccessKey = configuredAccessKeyValue;
	}

    public bool AppConfigExists(string key)
    {
        if (StoreID() != null)
            return GetSqlN("select count(*) as N from AppConfig where Name ='" + key + "' and (StoreId = 0 or StoreId = " + StoreID() + ")") > 0;
        else
            return GetSqlN("select count(*) as N from AppConfig where Name = '" + key + "'") > 0;
    }

    public string GetAppConfig(string key)
    {
        if (StoreID() != null)
            return GetSqlS("select top 1 ConfigValue as S from AppConfig where Name ='" + key + "' and (StoreId = 0 or StoreId = " + StoreID() + ") order by StoreID DESC");
        else
            return GetSqlS("select top 1 ConfigValue as S from AppConfig where Name = '" + key + "'");
    }

    public String GetSqlS(String query)
    {
        Type dbType = GetType("DB");
        System.Reflection.MethodInfo method = dbType.GetMethod("GetSqlS", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new Type[] { typeof(String) }, null);

        Object result = method.Invoke(null, new Object[] { query });
        if (result == null)
            return null;
        else
            return result.ToString();
    }

    public String QuoteSql(String query)
    {
        Type dbType = GetType("DB");
        System.Reflection.MethodInfo method = dbType.GetMethod("SQuote", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new Type[] { typeof(String) }, null);

        Object result = method.Invoke(null, new Object[] { query });
        if (result == null)
            return null;
        else
            return result.ToString();
    }

    public void ExecuteSql(String query)
    {
        Type dbType = GetType("DB");
        System.Reflection.MethodInfo method = dbType.GetMethod("ExecuteSQL", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new Type[] { typeof(String) }, null);
        method.Invoke(null, new Object[] { query });
    }

    public Int32 GetSqlN(String query)
    {
        Type dbType = GetType("DB");
        System.Reflection.MethodInfo method = dbType.GetMethod("GetSqlN", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new Type[] { typeof(String) }, null);
        return (Int32)method.Invoke(null, new Object[] { query });
    }

    public void CreateAppConfig(String name, String description, String value, String group, Boolean superOnly)
    {
        System.Reflection.MethodInfo method = GetType("AppConfig").GetMethod("Create", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new Type[] { typeof(String), typeof(String), typeof(String), typeof(String), typeof(Boolean) }, null);
        method.Invoke(null, new Object[] { name, description, value, group, superOnly });
    }
    
    public Type GetType(String className)
    {
        if (System.IO.File.Exists(Server.MapPath("Bin\\AspDotNetStorefrontCore.dll")))
        {
            foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.FullName.Contains("AspDotNetStorefrontCore"))
                    return assembly.GetType(String.Format("AspDotNetStorefrontCore.{0}", className), false, true);
        }
        else
        {
            foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.FullName.Contains("AspDotNetStorefrontCommon"))
                    return assembly.GetType(String.Format("AspDotNetStorefrontCommon.{0}", className), false, true);
        }

        return null;
    }

    public String TransformString(String xmlPackagePath)
    {
        Type xmlPackageType = GetType("XmlPackage2");
        Object xmlPackage = Activator.CreateInstance(xmlPackageType, new Object[] { xmlPackagePath });

        System.Reflection.MethodInfo method = xmlPackageType.GetMethod("TransformString");
        
        return method.Invoke(xmlPackage, null).ToString();
    }

    public static int? StoreID()
    {
        if (HttpContext.Current != null &&
            HttpContext.Current.Items["StoreId"] != null)
        {
            return Convert.ToInt32(HttpContext.Current.Items["StoreId"]);
        }
        return null;
    }
    
	private void VerifyParameters()
	{
        if (Request.QueryString.Count == 0)
        {
            ReturnErrorMessage("DotFeed Request Acknowledged. See DotFeed Panel for more information and validation.");
            Response.End();
        }
        
        if (Request.QueryString["AccessKey"] != ConfiguredAccessKey)
        {
            ReturnErrorMessage("Invalid access key provided");
            Response.End();
        }

        if (Request.RequestType.ToUpperInvariant() != "POST")
        {
            ReturnErrorMessage("This page only accepts HTTP POST");
            Response.End();
        }

        DateTime authDateValue;
        if (!DateTime.TryParse(Request.QueryString["AuthDate"], out authDateValue))
        {
            ReturnErrorMessage("AuthDate is not a valid DateTime");
            Response.End();
        }

        RawAuthDateParam = Request.Params["AuthDate"];
        AuthDate = authDateValue;

        try
        {
            ExtendedAuthToken = Convert.FromBase64String((Request.QueryString["ExtendedAuthToken"] ?? String.Empty).Replace(" ", "+"));
        }
        catch (Exception)
        {
            ReturnErrorMessage("ExtendedAuthToken is not a valid base64 encoded string");
            Response.End();
        }

        if (Request.ContentLength == 0)
        {
            ReturnErrorMessage("No XmlPackage provided");
            Response.End();
        }
    }

    private void VerifyAuthentication(DateTime requestDate, string requestContents)
    {
        // Make sure authDate is within window
        if (requestDate - AuthDate > AuthWindow || requestDate - AuthDate < -AuthWindow)
        {
            ReturnErrorMessage("The request authorization window has expired");
            Response.End();
        }
        // Verify signature
        string source = ConfiguredAccessKey + RawAuthDateParam + requestContents;
        byte[] data = Encoding.Unicode.GetBytes(source);

        System.Security.Cryptography.RSACryptoServiceProvider rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
        rsa.FromXmlString(DotFeedPublicKey);

        bool verified = rsa.VerifyData(data, "SHA1", ExtendedAuthToken);

        if (!verified)
        {
            ReturnErrorMessage("The provided ExtendedAuthToken is invalid");
            Response.End();
        }
    }

	private string ProcessXmlPackage()
	{
		string xmlPackagePath = null;
		string result = null;

		try
		{
			xmlPackagePath = SaveXmlPackage();
            result = TransformString(xmlPackagePath);
		}
		catch(Exception exception)
		{
			ReturnErrorMessage("An exception occurred: " + exception.ToString());
			Response.End();
		}
		finally
		{
			if(xmlPackagePath != null)
				CleanUpXmlPackage(xmlPackagePath);
		}

		return result;
	}

	private string SaveXmlPackage()
	{
		string xmlPackagePath;

		do
		{
			string filename = String.Format("DotFeed-{0}.xml.config", Guid.NewGuid());
			xmlPackagePath = Server.MapPath("~/images/" + filename);
		} while(System.IO.File.Exists(xmlPackagePath));

        System.IO.FileInfo file = new System.IO.FileInfo(xmlPackagePath);


            using (System.IO.FileStream fileStream = file.Create())
		{
			byte[] buffer = new byte[32768];
			int read;
			while((read = Request.InputStream.Read(buffer, 0, buffer.Length)) > 0)
				fileStream.Write(buffer, 0, read);

			fileStream.Close();
		}
        

		return xmlPackagePath;
	}

	private void CleanUpXmlPackage(string xmlPackagePath)
	{
		System.IO.File.Delete(xmlPackagePath);
	}

	private void ReturnErrorMessage(string message)
	{
        Response.Write(@"<error><message>" + Server.HtmlEncode(message) + @"</message></error>");
	}
</script>
    </head>
    <body />
</html>
