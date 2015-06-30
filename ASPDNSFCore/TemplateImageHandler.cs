// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace AspDotNetStorefrontCore
{
    public class TemplateImageHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (this.ImageGuid == new Guid() || ImageExtension == null)
                return;
            context.Response.ContentType = string.Format("image/{0}", ImageExtension.ToUpperInvariant());
            WriteImage(context);


        }

        private SqlCommand _cmdRetrieve;
        private SqlCommand cmdRetrieve
        {
            get
            {
                if (_cmdRetrieve != null)
                    return _cmdRetrieve;
                _cmdRetrieve = new SqlCommand(sqlRetrieve);
                _cmdRetrieve.Parameters.Add(
                    new SqlParameter("@GUID", ImageGuid)
                    );

                return _cmdRetrieve;
            }
        }

        private void WriteImage(HttpContext context)
        {
            byte[] buffer = new byte[BufferSize];
            long readLen = (long)BufferSize;
            int position = 0;
            using (SqlConnection xCon = new SqlConnection(DB.GetDBConn()))
            {
                cmdRetrieve.Connection = xCon;
                xCon.Open();
                
                SqlDataReader xRdr = cmdRetrieve.ExecuteReader(CommandBehavior.SequentialAccess);
                if (!xRdr.HasRows)
                    return;
                xRdr.Read();
                while (readLen == BufferSize)
                {
                    readLen = xRdr.GetBytes(0, position, buffer, 0, BufferSize);
                    context.Response.BinaryWrite(buffer);
                    position += BufferSize;
                }
            }
        }

        #region Properties



        private Guid ImageGuid
        {
            get
            {
                try
                {
                    if (CommonLogic.QueryStringCanBeDangerousContent("ImageGUID") == string.Empty)
                        throw new InvalidOperationException("Template Image Handler requires a GUID parameter");
                    return new Guid(CommonLogic.QueryStringCanBeDangerousContent("ImageGUID"));
                }
                catch
                {
                    return new Guid();
                }
            }
        }

        private string _extension;

        private string TableName
        {
            get
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("type") == string.Empty)
                    return "StaticTemplateImages";
                else
                    return CommonLogic.QueryStringCanBeDangerousContent("type").Split('-')[0];
            }
        }
        private string ImageField
        {     
            get
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("type") == string.Empty)
                    return "IMAGE";
                else
                    return CommonLogic.QueryStringCanBeDangerousContent("type").Split('-')[1];
            }
        }
        private string ImageExtensionsField
        {
            get
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("type") == string.Empty)
                    return "Extension";
                else
                    return ImageField + "Extension";
            }
        }
        private string GUIDField
        {
            get
            {
                if (CommonLogic.QueryStringCanBeDangerousContent("gf") == string.Empty)
                    return "GUID";
                else
                    return CommonLogic.QueryStringCanBeDangerousContent("gf");
            }
        }


        //"SELECT {0} FROM {1} WHERE {2} = @GUID"
        private string sqlRetrieve
        {
            get
            {
                return string.Format("SELECT {0} FROM {1} WHERE {2} = @GUID", new object[]
                {
                    ImageField, 
                    TableName, 
                    GUIDField
                });
            }
        }
        private string sqlGetExtensions
        {
            get
            {
                return string.Format("SELECT {0} FROM {1} WHERE {2} = @GUID", new object[]
                {
                    ImageExtensionsField, 
                    TableName, 
                    GUIDField
                });
            }
        }

        private string ImageExtension
        {
            get
            {
                if (_extension != null)
                    return _extension;
                SqlCommand getExtension = new SqlCommand(sqlGetExtensions);
                getExtension.Parameters.Add(
                     new SqlParameter("@GUID", ImageGuid)
                     );

                using (SqlConnection xCon = new SqlConnection(DB.GetDBConn()))
                {
                    getExtension.Connection = xCon;
                    xCon.Open();
                    _extension = (string)getExtension.ExecuteScalar();
                }
                return _extension;

            }
        }

        private int BufferSize
        {
            get { return 1024 * 10; }
        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #endregion
    }
}
