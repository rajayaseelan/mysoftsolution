using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace SharedCache.Registration
{
	public partial class Register : Form
	{
		public Register()
		{
			InitializeComponent();
			this.txtCompanyName.Focus();
		}

		private void BtnClose_Click(object sender, EventArgs e)
		{
			DialogResult res = MessageBox.Show("Are you sure you like to Exit registration?", "Exit Registration", MessageBoxButtons.OKCancel);
			if (res == DialogResult.OK)
			{
				this.Close();
			}			
		}

		private void BtnRegister_Click(object sender, EventArgs e)
		{
			try
			{
				bool show = false;
				if(string.IsNullOrEmpty(this.txtFullName.Text.Trim()))
				{
					show = true;
				}
				if(string.IsNullOrEmpty(this.txtCompanyName.Text.Trim()))
				{
					show = true;
				}
				if (string.IsNullOrEmpty(this.txtEmail.Text.Trim()))
				{
					show = true;
				}

				if (show)
				{
					MessageBox.Show("Ensure you entered all needed data. " + Environment.NewLine + "Company name, your name and your Email address is must.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
				Registration.Registration srv = new SharedCache.Registration.Registration.Registration();
				srv.Url = "http://sharedcache.indexus.net/Registration.asmx";
				
				srv.SetInstallRegistration(
					Environment.MachineName,
					this.txtFullName.Text.Trim(),
					this.txtCompanyName.Text.Trim(),
					this.txtEmail.Text.Trim(),
					this.cmbUsage.SelectedItem != null ? this.cmbUsage.SelectedItem.ToString() : string.Empty ,
					this.cmbInfo.SelectedItem != null ? this.cmbInfo.SelectedItem.ToString() : string.Empty,
					this.ckbUpdate.Checked
				);

				this.SetRegistryValue();
				
				MessageBox.Show("Thank you for registration." + Environment.NewLine + "You able now to use our notifier without any limitations.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			catch (Exception ex)
			{
				
			}

			this.Close();
			this.Cursor = System.Windows.Forms.Cursors.Default;
		}

		private void SetRegistryValue()
		{
			try
			{
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software", true);
				// Add one more sub key
				Microsoft.Win32.RegistryKey newkey = key.CreateSubKey("SharedCache");
				// Set value of sub key
				newkey.SetValue("RegisterForSharedCache.com", "True");
			}
			catch (Exception)
			{

			}
		}
	}
}
