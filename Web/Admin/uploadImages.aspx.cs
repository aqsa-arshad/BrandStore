// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI.WebControls;
using System.IO;
using System.Collections;
using System.Web;
using AspDotNetStorefrontCore;

namespace AspDotNetStorefrontAdmin
{
    /// <summary>
    /// Summary description for uploadimages.
    /// </summary>
    public partial class uploadImages : AdminPageBase
    {
        protected Customer cust;
        private String SFP = CommonLogic.SafeMapPath("../images/spacer.gif").Replace("images\\spacer.gif", "images\\upload");

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.CacheControl = "private";
            Response.Expires = 0;
            Response.AddHeader("pragma", "no-cache");

            cust = ((AspDotNetStorefrontPrincipal)Context.User).ThisCustomer;

            if (!IsPostBack)
            {
                buildGridData(buildGridData());
            }
            Page.Form.DefaultButton = btnUpload.UniqueID;
            Page.Form.DefaultFocus = fuMain.UniqueID;
        }

        protected void loadScript(bool load)
        {
            if (load)
            {

            }
            else
            {

            }
        }

        protected DataSet buildGridData()
        {

            // create an array to hold the list of files
            ArrayList fArray = new ArrayList();

            // get information about our initial directory
            DirectoryInfo dirInfo = new DirectoryInfo(SFP);

            // retrieve array of files & subdirectories
            FileSystemInfo[] myDir = dirInfo.GetFileSystemInfos();

            DataSet ds = new DataSet();
            ds.Tables.Add();
            ds.Tables[0].Columns.Add("Path");
            ds.Tables[0].Columns.Add("FileName");
            ds.Tables[0].Columns.Add("SRC");
            ds.Tables[0].Columns.Add("Dimensions");
            ds.Tables[0].Columns.Add("Size");
            ds.Tables[0].Columns.Add("Image");

            for (int i = 0; i < myDir.Length; i++)
            {
                // check the file attributes

                // if a subdirectory, add it to the sArray    
                // otherwise, add it to the fArray
                if (((Convert.ToUInt32(myDir[i].Attributes) & Convert.ToUInt32(FileAttributes.Directory)) > 0))
                {
                    //sArray.Add(Path.GetFileName(myDir[i].FullName));  
                }
                else
                {
                    bool skipit = false;
                    if (myDir[i].FullName.StartsWith("_") || (!myDir[i].FullName.EndsWith("jpg", StringComparison.InvariantCultureIgnoreCase) && !myDir[i].FullName.EndsWith("gif", StringComparison.InvariantCultureIgnoreCase) && !myDir[i].FullName.EndsWith("png", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        skipit = true;
                    }
                    if (!skipit)
                    {
                        fArray.Add(Path.GetFileName(myDir[i].FullName));
                    }
                }
            }

            if (fArray.Count != 0)
            {
                // sort the files alphabetically
                fArray.Sort(0, fArray.Count, null);
                for (int i = 0; i < fArray.Count; i++)
                {
                    DataRow dr = ds.Tables[0].NewRow();

                    String src = "../images/upload/" + fArray[i].ToString();
                    System.Drawing.Size size = CommonLogic.GetImagePixelSize(src);
                    long s = CommonLogic.GetImageSize(src);

                    dr["FileName"] = fArray[i].ToString();
                    dr["SRC"] = "../images/upload/" + fArray[i].ToString();
                    dr["Dimensions"] = size.Width.ToString() + "x" + size.Height.ToString();
                    dr["Size"] = (s / 1000).ToString();
                    dr["Image"] = "<img border=\"0\" src=\"" + src + "?" + CommonLogic.GetRandomNumber(1, 1000000).ToString() + "\"" + CommonLogic.IIF(size.Height > 50, " height=\"50\"", "") + "/>";
                    dr["Path"] = fArray[i].ToString();

                    ds.Tables[0].Rows.Add(dr);
                }
            }

            return ds;
        }

        protected void buildGridData(DataSet ds)
        {
            gMain.DataSource = ds;
            gMain.DataBind();
            ds.Dispose();
        }

        protected void resetError(string error, bool isError)
        {
            string str = "<font class=\"noticeMsg\">NOTICE:</font>&nbsp;&nbsp;&nbsp;";
            if (isError)
                str = "<font class=\"errorMsg\">ERROR:</font>&nbsp;&nbsp;&nbsp;";

            if (error.Length > 0)
                str += error + "";
            else
                str = "";

            ltError.Text = str;
        }

        protected void gMain_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ViewState["IsInsert"] = false;
            resetError("", false);
            gMain.PageIndex = e.NewPageIndex;
            gMain.EditIndex = -1;
            buildGridData(buildGridData());
        }

        protected void gMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ImageButton ib = (ImageButton)e.Row.FindControl("imgDelete");
                ib.Attributes.Add("onClick", "javascript: return confirm('Confirm Delete?')");

                e.Row.Cells[5].Text = ((Literal)e.Row.FindControl("ltImage")).Text.Replace("&gt;", ">").Replace("&lt;", "<");
            }
        }

        protected void gMain_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            resetError("", false);

            if (e.CommandName == "DeleteItem")
            {
                ViewState["IsInsert"] = false;
                gMain.EditIndex = -1;
                string iden = e.CommandArgument.ToString();
                deleteRow(iden);
            }
        }
        protected void deleteRow(string iden)
        {
            // delete the image:
            System.IO.File.Delete(SFP + "/" + iden);
            buildGridData(buildGridData());
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            // handle upload if any also:
            HttpPostedFile Image1File = fuMain.PostedFile;
            if (Image1File.ContentLength != 0)
            {
                String tmp = Image1File.FileName;
                if (tmp.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || 
                    tmp.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) || 
                    tmp.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (tmp.LastIndexOf('\\') != -1)
                    {
                        tmp = tmp.Substring(tmp.LastIndexOf('\\') + 1);
                    }
                    String fn = SFP + "/" + tmp;
                    Image1File.SaveAs(fn);

                    resetError("Image uploaded.", false);
                }
            }

            buildGridData(buildGridData());
        }
    }
}

