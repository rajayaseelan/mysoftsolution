using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace SharedCache.Notify
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			bool firstInstance;
			Mutex mutex = new Mutex(false, "Local\\MergeSystemIndexusNotify", out firstInstance);

			if(firstInstance)
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm());
			}
			else
			{
				System.Windows.Forms.MessageBox.Show(@"Info: You cannot run more then one instance. SharedCache Notify is already running.", "Shared Cache Notifier", MessageBoxButtons.OK);
			}
		}
	}
}