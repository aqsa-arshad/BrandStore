// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AspDotNetStorefrontCore;
using System.Data.SqlClient;
using Telerik.Web.UI;

namespace AspDotNetStorefrontAdmin
{
	public partial class topics : AdminPageBase
	{
		#region Event Handlers
		protected void Page_Load(object sender, EventArgs e)
		{
			Response.CacheControl = "private";
			Response.Expires = 0;
			Response.AddHeader("pragma", "no-cache");

			BindData();
		}
		protected void TopicEditor_TopicCopiedToStore(object sender, TopicEditEventArgs e)
		{
			loadTree(e.TopicId);
			TopicEditor.LoadTopic(e.TopicId);
		}
		protected void TopicEditor_TopicAdded(object sender, TopicEditEventArgs e)
		{
			loadTree(e.TopicId);
			TopicEditor.LoadTopic(e.TopicId);
		}
		protected void TopicEditor_TopicSaved(object sender, TopicEditEventArgs e)
		{
			if (e.NameChanged)
				loadTree(e.TopicId);
			TopicEditor.LoadTopic(e.TopicId);
		}
		protected void TopicEditor_TopicDeleted(object sender, TopicEditEventArgs e)
		{
			loadTree();
		}
		protected void TopicEditor_TopicNuked(object sender, TopicEditEventArgs e)
		{
			loadTree();
		}
		protected void btnAdd_Click(object sender, EventArgs e)
		{
			TopicEditor.Visible = true;
			TopicEditor.LoadTopic(0);
		}
		protected void treeMain_SelectedNodeChanged(object sender, EventArgs e)
		{
			TopicEditor.Visible = true;
			int topicid;
			if (int.TryParse(treeMain.SelectedNode.Value, out topicid))
			{
				TopicEditor.LoadTopic(topicid);
			}
		}

		protected void ddlPageLocales_SelectedIndexChanged(object sender, EventArgs e)
		{
			TopicEditor.LocaleSetting = ddlPageLocales.SelectedValue;
		}

		#endregion

		#region Private Methods
		private void BindData()
		{
			loadDD();
			loadTree();
		}
		private void loadDD()
		{
			if (!IsPostBack)
			{
				using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
				{
					conn.Open();
					using (IDataReader thisReader = DB.GetRS("select Name from LocaleSetting with (NOLOCK) order by DisplayOrder,Description", conn))
					{
						while (thisReader.Read())
						{
							ddlPageLocales.Items.Add(new ListItem(DB.RSField(thisReader, "Name"), DB.RSField(thisReader, "Name")));
						}
					}
				}
				ddlPageLocales.Items.FindByValue(Localization.GetDefaultLocale()).Selected = true;
				divPageLocale.Visible = ddlPageLocales.Items.Count > 1;

				List<Store> storeList = Store.GetStoreList();
				ddStores.DataSource = storeList;
				ddStores.DataTextField = "Name";
				ddStores.DataValueField = "StoreID";
				ddStores.DataBind();

				ddStores.Items.Insert(0, new ListItem("All Stores", "0"));
			}
		}
		private void loadTree() { loadTree(0); }
		private void loadTree(int selectedTopic)
		{
			List<Store> storeList = Store.GetStoreList();
			Dictionary<int, String> storeNames = new Dictionary<int, string>();
			foreach (Store s in storeList)
				storeNames.Add(s.StoreID, s.Name);

			try
			{
				treeMain.Nodes.Clear();
                fileTreeMain.Nodes.Clear();

				//DATABASE TOPICS
				List<SqlParameter> spa = new List<SqlParameter>();
				spa.Add(new SqlParameter("@Published", ddPublished.SelectedValue));
				spa.Add(new SqlParameter("@StoreId", ddStores.SelectedValue));

				String sql = String.Empty;

				sql = "select * from Topic with (NOLOCK) where deleted=0";
				if (ddPublished.SelectedValue != "Both")
				{
					sql += " AND Published = @Published";
				}
				if (!chkShowAllTopics.Checked)
				{
					sql += " And IsFrequent = 1 ";
				}
				if (Store.StoreCount > 1 && ddStores.SelectedValue != "0")
				{
					sql += " And StoreId = @StoreId";
				}

				sql += " order by Name ASC ";

				using (SqlConnection conn = new SqlConnection(DB.GetDBConn()))
				{
					conn.Open();
					using (IDataReader rs = DB.GetRS(sql, spa.ToArray(), conn))
					{
						while (rs.Read())
						{
							string name = string.Empty;
							name = DB.RSFieldByLocale(rs, "Name", ThisCustomer.LocaleSetting);
							int StoreID = DB.RSFieldInt(rs, "StoreID");
							TreeNode myNode = new TreeNode();
							myNode.Text = CommonLogic.IIF(name.Equals(string.Empty), "[Not Set for this Locale]", name);
							if (Store.StoreCount > 1)
							{
								if (StoreID == 0)
									myNode.Text += " (All Stores)";
								else if (storeNames.ContainsKey(StoreID))
									myNode.Text += " (" + storeNames[StoreID] + ")";
								else
									myNode.Text += " (" + StoreID.ToString() + ")";
							}
							int tid = DB.RSFieldInt(rs, "TopicID");
							myNode.Value = tid.ToString();
							myNode.ImageUrl = AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/icons/dot.gif");
							myNode.Selected = tid == selectedTopic;
							treeMain.Nodes.Add(myNode);
						}
					}
				}

                // FILE BASED TOPICS:
                string appdir = HttpContext.Current.Request.PhysicalApplicationPath;
                string rootUrl = Path.Combine(appdir, string.Format("Topics\\"));
                ArrayList fArray = new ArrayList();

                //Skin specific first
                foreach (String skinId in AppLogic.FindAllSkins().Split(','))
                {
                    string skinUrl = Path.Combine(appdir, string.Format("App_Templates\\Skin_{0}\\Topics", skinId));

                    //See if there are any files there
                    DirectoryInfo dirInfo = new DirectoryInfo(skinUrl);
                    if (dirInfo != null)
                    {
                        FileSystemInfo[] myDir = dirInfo.GetFileSystemInfos();

                        for (int i = 0; i < myDir.Length; i++)
                        {
                            // check the file attributes, skip subdirs:
                            if (!((Convert.ToUInt32(myDir[i].Attributes) & Convert.ToUInt32(FileAttributes.Directory)) > 0))
                            {
                                if (myDir[i].FullName.EndsWith("htm", StringComparison.InvariantCultureIgnoreCase) || myDir[i].FullName.EndsWith("html", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string filenameWithoutExtension = myDir[i].FullName.Substring(0, myDir[i].FullName.IndexOf(".htm"));
                                    fArray.Add(Path.GetFileName(filenameWithoutExtension));
                                }
                            }
                        }

                        if (fArray.Count != 0)
                        {
                            // sort the files alphabetically
                            fArray.Sort(0, fArray.Count, null);
                            for (int i = 0; i < fArray.Count; i++)
                            {
                                TreeNode myNode = new TreeNode();
                                myNode.Value = SE.MakeDriverLink(XmlCommon.GetLocaleEntry(fArray[i].ToString(), ThisCustomer.LocaleSetting, true));
                                myNode.Text = string.Format("<a target='_blank' href='../{0}'>{1} (Skin {2})</a>", myNode.Value, fArray[i].ToString(), skinId);
                                myNode.ImageUrl = AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/icons/dot.gif");
                                fileTreeMain.Nodes.Add(myNode);
                            }
                        }
                        
                        fArray.Clear();
                    }
                }

                //Root folder
                //See if there are any files there
                DirectoryInfo rootDirInfo = new DirectoryInfo(rootUrl);

                if (rootDirInfo != null)
                {
                    FileSystemInfo[] rootFiles = rootDirInfo.GetFileSystemInfos();

                    for (int i = 0; i < rootFiles.Length; i++)
                    {
                        // check the file attributes, skip subdirs:
                        if (!((Convert.ToUInt32(rootFiles[i].Attributes) & Convert.ToUInt32(FileAttributes.Directory)) > 0))
                        {
                            if (rootFiles[i].FullName.EndsWith("htm", StringComparison.InvariantCultureIgnoreCase) || rootFiles[i].FullName.EndsWith("html", StringComparison.InvariantCultureIgnoreCase))
                            {
                                string filenameWithoutExtension = rootFiles[i].FullName.Substring(0, rootFiles[i].FullName.IndexOf(".htm"));
                                fArray.Add(Path.GetFileName(filenameWithoutExtension));
                            }
                        }
                    }

                    if (fArray.Count != 0)
                    {
                        // sort the files alphabetically
                        fArray.Sort(0, fArray.Count, null);
                        for (int i = 0; i < fArray.Count; i++)
                        {
                            TreeNode myNode = new TreeNode();
                            myNode.Value = SE.MakeDriverLink(XmlCommon.GetLocaleEntry(fArray[i].ToString(), ThisCustomer.LocaleSetting, true));
                            myNode.Text = string.Format("<a target='_blank' href='../{0}'>{1} (All skins)</a>", myNode.Value, fArray[i].ToString());
                            myNode.ImageUrl = AppLogic.LocateImageURL("~/App_Themes/Admin_Default/images/icons/dot.gif");
                            fileTreeMain.Nodes.Add(myNode);
                        }
                    }
                }
			}
			catch (Exception ex)
			{
				resetError(ex.ToString(), true);
			}
		}

		protected void resetError(string error, bool isError)
		{
			string str = AppLogic.GetString("admin.topic.notice", SkinID, LocaleSetting);
			if (isError)
			{
				str = AppLogic.GetString("admin.common.Error", SkinID, LocaleSetting);
			}
			if (error.Length > 0)
			{
				str += error + "";
			}
			else
			{
				str = "";
			}
			ltError.Text = str;
		}
		#endregion
	}
}
