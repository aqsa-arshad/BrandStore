// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Net;

namespace AspDotNetStorefrontCommon
{
    public class FtpClient
    {
        private string m_hostname = string.Empty;
        private string m_username = string.Empty;
        private string m_password = string.Empty;
        private string m_currentDirectory = string.Empty;


        //Constructor
        public FtpClient(string hostname, string username, string pwd)
        {
            m_hostname = hostname;
            m_username = username;
            m_password = pwd;
        }

        public string Upload(string localFileName, string targetFilename)
        {
            if (!File.Exists(localFileName))
            {
                throw new Exception("File " + localFileName + " not found");
            }

            FileInfo fi = new FileInfo(localFileName);

            return Upload(fi, targetFilename);

        }
        public string Upload(FileInfo fi, string targetFilename)
        {
            string retVal = "File Uploaded Successfully";
            string target = string.Empty;
            if (targetFilename.Trim().Length == 0)
            {
                target = this.CurrentDirectory + fi.Name;
            }
            else if (targetFilename.Contains("/"))
            {
                target = AdjustDir(targetFilename);
            }
            else
            {
                target = this.CurrentDirectory + targetFilename;
            }

            string URI = this.HostName + target;
            FtpWebRequest ftp = (FtpWebRequest)WebRequest.Create(URI);
            if (this.Username.Length > 0)
            {
                ftp.Credentials = new NetworkCredential(this.Username, this.Password);
            }
            ftp.Method = WebRequestMethods.Ftp.UploadFile;
            ftp.UseBinary = true;
            ftp.ContentLength = fi.Length;

            const int BufferSize = 2048;
            Byte[] content = new Byte[BufferSize];
            int dataRead;

            using (FileStream fs = fi.OpenRead())
            {
                try
                {
                    using (Stream rs = ftp.GetRequestStream())
                    {
                        do
                        {
                            dataRead = fs.Read(content, 0, BufferSize);
                            rs.Write(content, 0, dataRead);
                        } while (dataRead == BufferSize);
                        rs.Close();
                    }
                }
                catch (Exception ex) { retVal = ex.Message;  }
                finally
                {
                    fs.Close();
                }
            }

            return retVal;
        }



        //Public Properties
        public string HostName
        {
            get
            {
                if (m_hostname.StartsWith("ftp://", StringComparison.InvariantCultureIgnoreCase))
                {
                    return m_hostname;
                }
                else
                {
                    return "ftp://" + m_hostname;
                }
            }

            set { m_hostname = value; }
        }
        public string Username
        {
            get
            {
                return m_username == string.Empty ? "anonymous" : m_username;
            }
            set
            {
                m_username = value;
            }
        }
        public string Password
        {
            get
            {
                return m_password;
            }
            set
            {
                m_password = value;
            }
        }
        public string CurrentDirectory
        {
            get
            {
                return m_currentDirectory + (m_currentDirectory.EndsWith("/") ? string.Empty : "/");
            }
            set
            {
                if (!m_currentDirectory.StartsWith("/"))
                {
                    throw new Exception("Directory should start with /");
                }
                else
                {
                    m_currentDirectory = value;
                }
            }
        }




        //Private Functions
        private string AdjustDir(string path)
        {
            return (path.StartsWith("/") ? string.Empty : "/") + path;
        }

    }
}
