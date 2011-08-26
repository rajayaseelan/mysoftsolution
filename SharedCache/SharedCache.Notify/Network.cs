#region Copyright (c) Roni Schuetz - All Rights Reserved
// * --------------------------------------------------------------------- *
// *                              Roni Schuetz                             *
// *              Copyright (c) 2008 All Rights reserved                   *
// *                                                                       *
// * Shared Cache high-performance, distributed caching and    *
// * replicated caching system, generic in nature, but intended to         *
// * speeding up dynamic web and / or win applications by alleviating      *
// * database load.                                                        *
// *                                                                       *
// * This Software is written by Roni Schuetz (schuetz AT gmail DOT com)   *
// *                                                                       *
// * This library is free software; you can redistribute it and/or         *
// * modify it under the terms of the GNU Lesser General Public License    *
// * as published by the Free Software Foundation; either version 2.1      *
// * of the License, or (at your option) any later version.                *
// *                                                                       *
// * This library is distributed in the hope that it will be useful,       *
// * but WITHOUT ANY WARRANTY; without even the implied warranty of        *
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU      *
// * Lesser General Public License for more details.                       *
// *                                                                       *
// * You should have received a copy of the GNU Lesser General Public      *
// * License along with this library; if not, write to the Free            *
// * Software Foundation, Inc., 59 Temple Place, Suite 330,                *
// * Boston, MA 02111-1307 USA                                             *
// *                                                                       *
// *       THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.        *
// * --------------------------------------------------------------------- *
#endregion

// *************************************************************************
//
// Name:      Network.cs
// 
// Created:   04-01-2008 SharedCache.com, rschuetz
// Modified:  04-01-2008 SharedCache.com, rschuetz : Creation
// ************************************************************************* 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;

using COM = SharedCache.WinServiceCommon;
using CACHE = SharedCache.WinServiceCommon.Provider.Cache.IndexusDistributionCache;

// http://www.c-sharpcorner.com/UploadFile/prasadh/TreeViewControlInWinFormsPSD11182005235448PM/TreeViewControlInWinFormsPSD.aspx
namespace SharedCache.Notify
{
	public partial class Network : Form
	{
		private delegate void DisplayServerNodes();
		private delegate void UpdateStats();
		private delegate void DelegateDisplayNodeSelectionResult(object sender, EventArgs e);
		private delegate void DelegateSearchRegEx(object sender, EventArgs e);
		private delegate void DelegateBtnSearch(object sender, EventArgs e);
		private delegate void DelegateClearLbxServerNodes();
		private delegate void DelegateClearLbxNodeKey();
		private delegate void DelegateClearTxtSearchRegEx();
		private delegate void DelegateClearTxtSearchKey();

		private Thread worker = null;
		private Thread stats = null;

		public Network()
		{
			InitializeComponent();
			this.StartUpThread();
			this.LblClrVersion.Text = Environment.Version.ToString();
			this.LblScVersion.Text = new AssemblyInfo().Version;
			this.LoadServerClrVersions();

		}

		private void SetToolTips()
		{
			Cursor.Current = Cursors.WaitCursor;
			this.TtCacheNodes.SetToolTip(this.LblCacheNodes, "If you can't see all your Cache Nodes check your configuraiton file.");
			this.TtCacheNodeKeys.SetToolTip(this.LblAvailableKeys, "A list with all selecte node key's - you able to select more then one server at once.");
			this.TtRegularExpression.SetToolTip(this.TxtSearchRegEx, "If you're using Prefixes for your Key's you can simple get all Items based on regular expressions (e.g.: 'Prefix = ClientRelated_Xxxx then you search for 'ClientRelated_*.')");
			this.TtSearch.SetToolTip(this.TxtSearchKey, "If you search a specific Key you can find it easly with this search form");
			Cursor.Current = Cursors.Default;
		}

		public void BindStats()
		{
			if (this.InvokeRequired)
			{
				UpdateStats inv = new UpdateStats(this.BindStats);
				this.Invoke(inv, new object[] { });
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;
				COM.IndexusStatistic stat = CACHE.SharedCache.GetStats();
				// Put the words data in a DataTable so that column sorting works.
				DataTable dataTable = new DataTable();
				dataTable.Columns.Add("IP/Name", typeof(string));
				dataTable.Columns.Add("Amount", typeof(long));
				dataTable.Columns.Add("Size KB", typeof(long));
				foreach (COM.ServerStats st in stat.NodeDate)
				{
					dataTable.Rows.Add(new object[] { st.Name, st.AmountOfObjects, st.CacheSize / 1024 });
				}
				this.GvStats.DataSource = dataTable;
				Cursor.Current = Cursors.Default;
			}
		}

		private void StartUpThread()
		{
			this.SetToolTips();
			this.worker = new Thread(new ThreadStart(this.FullResetForm));
			this.worker.Start();
			this.BindStats();
			this.UpdateLblAmount();
		}

		private void LoadServerClrVersions()
		{
			bool normalHashingIsOk = true;
			IDictionary<string, string> serverNodeVersionsSharedCache = CACHE.SharedCache.ServerNodeVersionSharedCache();
			IDictionary<string, string> serverNodeVersionsClr = CACHE.SharedCache.ServerNodeVersionClr();
			
			foreach (string srv in CACHE.SharedCache.Servers)
			{
				if (
						serverNodeVersionsClr.ContainsKey(srv) &&
						serverNodeVersionsSharedCache.ContainsKey(srv))
				{
					if (!serverNodeVersionsClr[srv].Equals(Environment.Version.ToString(), StringComparison.InvariantCultureIgnoreCase))
					{
						normalHashingIsOk = false;
					}

					Common.ComboBoxItem item = new Common.ComboBoxItem(
						string.Format("Node:{0} - Shared Cache Ver.:{1} - CLR Ver.: {2}", srv, serverNodeVersionsSharedCache[srv], serverNodeVersionsClr[srv]),
						0
					);
					this.LbxServerClrVersion.Items.Add(item);
				}
			}
			this.LbxServerClrVersion.Sorted = true;
		}

		private void DisplayNodeSelectionResult(object sender, EventArgs e)
		{
			if (this.InvokeRequired)
			{
				DelegateDisplayNodeSelectionResult inv = new DelegateDisplayNodeSelectionResult(this.DisplayNodeSelectionResult);
				this.Invoke(inv, new object[] { sender, e });
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;
				string msg = "Selected node(s) does not contain any key's." + Environment.NewLine;
				int nodeCounter = 1;
				if(this.LbxNodeKey.Items.Count > 0) this.LbxNodeKey.Items.Clear();

				ListBox.SelectedObjectCollection coll = this.LbxServerNodes.SelectedItems;

				foreach (Common.ComboBoxItem node in coll)
				{
					List<string> items = CACHE.SharedCache.GetAllKeys(node.Name);
					if (items.Count == 0)
					{
						msg += Environment.NewLine +nodeCounter.ToString()+ " - [ " + node + " ]";
						nodeCounter++;
					}
					foreach (string key in items)
					{
						this.LbxNodeKey.Items.Add(new Common.ComboBoxItem(key, -1));
					}
				}

				this.LbxNodeKey.Sorted = true;

				if (this.LbxNodeKey.Items.Count == 0)
				{
					MessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}

				Cursor.Current = Cursors.Default;
				this.UpdateLblAmount();
				this.UpdateLblAmount();
				this.BindStats();
			}
		}

		private void StartForm()
		{
			try
			{
				if (this.InvokeRequired)
				{
					DisplayServerNodes utvd = new DisplayServerNodes(this.StartForm);
					this.Invoke(utvd, new object[] { });
				}
				else
				{
					Cursor.Current = Cursors.WaitCursor;
					foreach (string node in CACHE.SharedCache.Servers)
					{
						this.LbxServerNodes.Items.Add(new Common.ComboBoxItem(node, -1));
					}
					Cursor.Current = Cursors.Default;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}		
		}

		private void FullResetForm()
		{
			Cursor.Current = Cursors.WaitCursor;
			if (this.LbxServerNodes.Items.Count > 0) this.ClearLbxServerNodes();
			if (this.LbxNodeKey.Items.Count > 0) this.ClearLbxNodeKey();
			if (!string.IsNullOrEmpty(this.TxtSearchRegEx.Text)) this.ClearTxtSearchRegEx();
			if (!string.IsNullOrEmpty(this.TxtSearchKey.Text)) this.ClearTxtSearchKey();

			this.StartForm();
			Cursor.Current = Cursors.Default;
		}
		
		private void ClearLbxServerNodes()
		{
			if (this.InvokeRequired)
			{
				DelegateClearLbxServerNodes inv = new DelegateClearLbxServerNodes(this.ClearLbxServerNodes);
				this.Invoke(inv, new object[] { });
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;
				this.LbxServerNodes.Items.Clear();
				Cursor.Current = Cursors.Default;
			}
		}

		
		private void ClearLbxNodeKey()
		{
			if (this.InvokeRequired)
			{
				DelegateClearLbxNodeKey inv = new DelegateClearLbxNodeKey(this.ClearLbxNodeKey);
				this.Invoke(inv, new object[] { });
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;
				this.LbxNodeKey.Items.Clear();
				Cursor.Current = Cursors.Default;
			}
		}		
		private void ClearTxtSearchRegEx()
		{
			if (this.InvokeRequired)
			{
				DelegateClearTxtSearchRegEx inv = new DelegateClearTxtSearchRegEx(this.ClearTxtSearchRegEx);
				this.Invoke(inv, new object[] { });
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;
				this.TxtSearchRegEx.Text = string.Empty;
				Cursor.Current = Cursors.Default;
			}
		}
		private void ClearTxtSearchKey()
		{
			if (this.InvokeRequired)
			{
				DelegateClearTxtSearchKey inv = new DelegateClearTxtSearchKey(this.ClearTxtSearchKey);
				this.Invoke(inv, new object[] { });
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;
				this.TxtSearchKey.Text = string.Empty;
				Cursor.Current = Cursors.Default;
			}
		}
		
		private void BtnRegularExpressionSearch_Click(object sender, EventArgs e)
		{			
			if (this.InvokeRequired)
			{
				DelegateSearchRegEx inv = new DelegateSearchRegEx(this.BtnRegularExpressionSearch_Click);
				this.Invoke(inv, new object [] {sender, e} );
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;

				if (this.LbxNodeKey.Items.Count > 0) this.LbxNodeKey.Items.Clear();

				if (string.IsNullOrEmpty(this.TxtSearchRegEx.Text))
				{
					MessageBox.Show("Please enter search pattern.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
					this.TxtSearchRegEx.Focus();
					this.UnselectServerList();
					Cursor.Current = Cursors.Default;
					return;
				}

				try
				{
					Regex objNotNaturalPattern1 = new Regex(this.TxtSearchRegEx.Text);
				}
				catch (Exception ex)
				{
					MessageBox.Show("RegEx Parser Error: " + ex.Message + Environment.NewLine + "Please re-check you pattern and search again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
					this.TxtSearchRegEx.Focus();
					this.UnselectServerList();
					Cursor.Current = Cursors.Default;
					return;
				}

				IDictionary<string, byte[]> dict = CACHE.SharedCache.RegexGet(this.TxtSearchRegEx.Text);

				if (dict != null && dict.Count > 0)
				{
					if (this.LbxNodeKey.Items.Count > 0) this.LbxNodeKey.Items.Clear();

					foreach (KeyValuePair<string, byte[]> item in dict)
					{
						this.LbxNodeKey.Items.Add(
								new Common.ComboBoxItem(item.Key, -1)
							);
					}
					this.LbxNodeKey.Sorted = true;
				}
				else
				{
					MessageBox.Show("Your search term does not return any result, please revalidate your term: '" + this.TxtSearchRegEx.Text + "'",
						"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					this.UnselectServerList();
					Cursor.Current = Cursors.Default;
					return;
				}
				Cursor.Current = Cursors.Default;

				this.BindStats();
				this.UnselectServerList();
				this.UpdateLblAmount();
			}
		}

		private void BtnSearch_Click(object sender, EventArgs e)
		{
			if (this.InvokeRequired)
			{
				DelegateBtnSearch inv = new DelegateBtnSearch(this.BtnSearch_Click);
				this.Invoke(inv, new object[] { sender, e });
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;
				if (this.LbxNodeKey.Items.Count > 0) this.LbxNodeKey.Items.Clear();

				if (string.IsNullOrEmpty(this.TxtSearchKey.Text))
				{
					MessageBox.Show("Please enter search pattern.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
					this.TxtSearchKey.Focus();
					this.UnselectServerList();
					Cursor.Current = Cursors.Default;
					return;
				}

				List<string> keys = CACHE.SharedCache.GetAllKeys();

				if (keys != null && keys.Count > 0)
				{
					if (this.LbxNodeKey.Items.Count > 0) this.LbxNodeKey.Items.Clear();

					foreach (string item in keys)
					{
						if(item.Equals(this.TxtSearchKey.Text, StringComparison.InvariantCultureIgnoreCase))
						this.LbxNodeKey.Items.Add(
								new Common.ComboBoxItem(item, -1)
							);
					}					
				}
				else
				{
					MessageBox.Show("Your search term does not return any result, please revalidate your term: '" + this.TxtSearchKey.Text + "'",
						"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Cursor.Current = Cursors.Default;
					this.UnselectServerList();
					return;
				}
				Cursor.Current = Cursors.Default;
				this.UnselectServerList();
				this.BindStats();
				this.UpdateLblAmount();
			}
		}

		private delegate void DelegateUnselectServerList();
		private void UnselectServerList()
		{
			if (this.InvokeRequired)
			{
				DelegateUnselectServerList inv = new DelegateUnselectServerList(this.UnselectServerList);
				this.Invoke(inv, new object[] { });
			}
			else
			{
				this.LbxServerNodes.SelectedItem = null;
			}		
		}

		private void BtnResetForm_Click(object sender, EventArgs e)
		{
			this.StartUpThread();
		}

		private void BtnClearSelectedKeys_Click(object sender, EventArgs e)
		{			
			if (this.InvokeRequired)
			{
				DelegateDisplayNodeSelectionResult inv = new DelegateDisplayNodeSelectionResult(this.DisplayNodeSelectionResult);
				this.Invoke(inv, new object[] { sender, e });
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;
				string msg = "Please select single or multible key's you wish to delete" + Environment.NewLine;
				
				ListBox.SelectedObjectCollection coll = this.LbxNodeKey.SelectedItems;
				
				if (coll.Count > 0)
				{
					List<string> keysToDelete = new List<string>();
					foreach (Common.ComboBoxItem node in coll)
					{
						if (!string.IsNullOrEmpty(node.Name))
						{
							keysToDelete.Add(node.Name);
						}
					}

					CACHE.SharedCache.MultiDelete(keysToDelete);
					List<string> a = CACHE.SharedCache.GetAllKeys();

					if (a != null && a.Count > 0)
					{
						foreach (string key in keysToDelete)
						{
							CACHE.SharedCache.Remove(key);
						}
					}
					
					
					this.FullResetForm();
					this.UpdateLblAmount();
					this.BindStats();
					MessageBox.Show("All requested key(s) have been successfully deleted.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
					return;
				}
				else
				{
					MessageBox.Show(msg, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}

				Cursor.Current = Cursors.Default;
			}
		}

		// LblAmount
		private delegate void DelegateUpdateLblAmount();
		private void UpdateLblAmount()
		{
			if (this.InvokeRequired)
			{
				DelegateUpdateLblAmount inv = new DelegateUpdateLblAmount(this.UpdateLblAmount);
				this.Invoke(inv, new object[] { });
			}
			else
			{
				int amount = this.LbxNodeKey.Items.Count;
				if (amount == 0)
					this.LblAmount.Text = string.Empty;
				else
					this.LblAmount.Text = amount.ToString();
			}
		}

		private void SetFocusToDelete(object sender, EventArgs e)
		{
			this.LbxNodeKey.Focus();
		}

		private delegate void DelegateBtnClearCache(object sender, EventArgs e);

		private void BtnClearCache_Click(object sender, EventArgs e)
		{
			if (this.InvokeRequired)
			{
				DelegateBtnClearCache inv = new DelegateBtnClearCache(this.BtnClearCache_Click);
				this.Invoke(inv, new object[] { sender, e} );
			}
			else
			{
				DialogResult result = MessageBox.Show("Are you sure you want to delete all items in configured Shared Cache nodes?", 
					"Your Attention is needed!", 
					MessageBoxButtons.YesNo);
				switch (result)
				{
					case DialogResult.Yes:
						{
							CACHE.SharedCache.Clear();
							this.FullResetForm();
							this.UpdateLblAmount();
							this.BindStats();
							break;
						}						
					case DialogResult.Abort:
					case DialogResult.Cancel:
					case DialogResult.Ignore:
					case DialogResult.No:
					case DialogResult.None:
					case DialogResult.OK:
					case DialogResult.Retry:
					default:
						break;
				}

			}
		}
	}
}