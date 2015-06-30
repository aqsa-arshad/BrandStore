// --------------------------------------------------------------------------------
// Copyright AspDotNetStorefront.com. All Rights Reserved.
// http://www.aspdotnetstorefront.com
// For details on this license please visit the product homepage at the URL above.
// THE ABOVE NOTICE MUST REMAIN INTACT. 
// --------------------------------------------------------------------------------
using System;
// it's required for reading/writing into the registry:
using Microsoft.Win32;      
using System.Globalization;

namespace AspDotNetStorefrontCommon
{
	/// <summary>
	/// An useful class to read/write/delete/count registry keys
	/// </summary>
	public class WindowsRegistry
	{
		private bool m_ShowError = false;

		/// <summary>
		/// A property to show or hide error messages 
		/// (default = false)
		/// </summary>
		public bool ShowError
		{
			get { return m_ShowError; }
			set	{ m_ShowError = value; }
		}

		private string m_SubKey = String.Empty;

		/// <summary>
		/// A property to set the SubKey value
		/// (default = "SOFTWARE\\" + Application.ProductName.ToUpperInvariant())
		/// </summary>
		public string SubKey
		{
			get { return m_SubKey; }
			set	{ m_SubKey = value; }
		}

		private RegistryKey m_BaseRegistryKey = Registry.LocalMachine;

		/// <summary>
		/// A property to set the BaseRegistryKey value.
		/// (default = Registry.LocalMachine)
		/// </summary>
		public RegistryKey BaseRegistryKey
		{
			get { return m_BaseRegistryKey; }
			set	{ m_BaseRegistryKey = value; }
		}

		/* **************************************************************************
		 * **************************************************************************/

		public WindowsRegistry(String KeyLocation)
		{
			m_SubKey = KeyLocation;
		}

		/// <summary>
		/// To read a registry key.
		/// input: KeyName (string)
		/// output: value (string) 
		/// </summary>
		public string Read(string KeyName)
		{
			// Opening the registry key
			RegistryKey rk = m_BaseRegistryKey ;
			// Open a subKey as read-only
			RegistryKey sk1 = rk.OpenSubKey(m_SubKey);
			// If the RegistrySubKey doesn't exist -> (null)
			if ( sk1 == null )
			{
				return null;
			}
			else
			{
				try 
				{
					// If the RegistryKey exists I get its value
					// or null is returned.
					return (string)sk1.GetValue(KeyName.ToUpperInvariant());
				}
				catch (Exception e)
				{
					ShowErrorMessage(e, "Reading registry " + KeyName.ToUpperInvariant());
					return null;
				}
			}
		}	

		/* **************************************************************************
		 * **************************************************************************/

		/// <summary>
		/// To write into a registry key.
		/// input: KeyName (string) , Value (object)
		/// output: true or false 
		/// </summary>
		public bool Write(string KeyName, object Value)
		{
			try
			{
				// Setting
				RegistryKey rk = m_BaseRegistryKey ;
				// I have to use CreateSubKey 
				// (create or open it if already exits), 
				// 'cause OpenSubKey open a subKey as read-only
				RegistryKey sk1 = rk.CreateSubKey(m_SubKey);
				// Save the value
				sk1.SetValue(KeyName.ToUpperInvariant(), Value);

				return true;
			}
			catch (Exception e)
			{
				
				ShowErrorMessage(e, "Writing registry " + KeyName.ToUpperInvariant());
				return false;
			}
		}

		/* **************************************************************************
		 * **************************************************************************/

		/// <summary>
		/// To delete a registry key.
		/// input: KeyName (string)
		/// output: true or false 
		/// </summary>
		public bool DeleteKey(string KeyName)
		{
			try
			{
				// Setting
				RegistryKey rk = m_BaseRegistryKey ;
				RegistryKey sk1 = rk.CreateSubKey(m_SubKey);
				// If the RegistrySubKey doesn't exists -> (true)
				if ( sk1 == null )
					return true;
				else
					sk1.DeleteValue(KeyName);

				return true;
			}
			catch (Exception e)
			{
				
				ShowErrorMessage(e, "Deleting SubKey " + m_SubKey);
				return false;
			}
		}

		/* **************************************************************************
		 * **************************************************************************/

		/// <summary>
		/// To delete a sub key and any child.
		/// input: void
		/// output: true or false 
		/// </summary>
		public bool DeleteSubKeyTree()
		{
			try
			{
				// Setting
				RegistryKey rk = m_BaseRegistryKey ;
				RegistryKey sk1 = rk.OpenSubKey(m_SubKey);
				// If the RegistryKey exists, I delete it
				if ( sk1 != null )
					rk.DeleteSubKeyTree(m_SubKey);

				return true;
			}
			catch (Exception e)
			{
				
				ShowErrorMessage(e, "Deleting SubKey " + m_SubKey);
				return false;
			}
		}

		/* **************************************************************************
		 * **************************************************************************/

		/// <summary>
		/// Retrive the count of subkeys at the current key.
		/// input: void
		/// output: number of subkeys
		/// </summary>
		public int SubKeyCount()
		{
			try
			{
				// Setting
				RegistryKey rk = m_BaseRegistryKey ;
				RegistryKey sk1 = rk.OpenSubKey(m_SubKey);
				// If the RegistryKey exists...
				if ( sk1 != null )
					return sk1.SubKeyCount;
				else
					return 0; 
			}
			catch (Exception e)
			{
				
				ShowErrorMessage(e, "Retriving subkeys of " + m_SubKey);
				return 0;
			}
		}

		/* **************************************************************************
		 * **************************************************************************/

		/// <summary>
		/// Retrive the count of values in the key.
		/// input: void
		/// output: number of keys
		/// </summary>
		public int ValueCount()
		{
			try
			{
				// Setting
				RegistryKey rk = m_BaseRegistryKey ;
				RegistryKey sk1 = rk.OpenSubKey(m_SubKey);
				// If the RegistryKey exists...
				if ( sk1 != null )
					return sk1.ValueCount;
				else
					return 0; 
			}
			catch (Exception e)
			{
				
				ShowErrorMessage(e, "Retriving keys of " + m_SubKey);
				return 0;
			}
		}

		private void ShowErrorMessage(Exception e, string Title)
		{
		}
	}
}
