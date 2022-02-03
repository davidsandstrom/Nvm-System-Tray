using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Nvm.Forms
{
	internal class NvmAppContext : ApplicationContext
	{
		private readonly NotifyIcon notifyIcon;
		private ContextMenuStrip menu;

		private NvmWrapper nvm;

		public NvmAppContext()
		{
			nvm = new NvmWrapper();

			menu = new ContextMenuStrip();
			menu.Opening += Menu_Opening;
			
			var label = new ToolStripLabel("Versions");
			menu.Items.Add(label);

			var separator = new ToolStripSeparator();
			menu.Items.Add(separator);

			var exit = new ToolStripMenuItem("Exit", null, OnExitClick);
			menu.Items.Add(exit);

			using Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NvmSystemTray.nvm.ico");

			notifyIcon = new NotifyIcon()
			{
				Icon = new System.Drawing.Icon(iconStream),
				Visible = true,
				ContextMenuStrip = menu
			};
		}

		private ToolStripItem[] GetVersions()
		{
			var list = new List<ToolStripItem>();
			var items = nvm.Versions();

			var indent = new Padding(30, 0, 0, 0);

			foreach (var item in items)
			{
				var version = new ToolStripMenuItem()
				{
					Text = item.Value,
					Name = item.Key,
					Padding = indent,
					Tag = "version"
				};
				
				version.Click += OnVersionClick;

				list.Add(version);
			}

			return list.ToArray();
		}

		private void UpdateVersions()
		{
			for (var i=0;i<menu.Items.Count;i++)
			{
				ToolStripItem item = menu.Items[i];
				if (item.Tag == "version")
				{
					menu.Items.Remove(item);
					i--;
				}
			}

			var versions = GetVersions();
			Array.Reverse(versions);

			foreach (ToolStripItem version in versions)
			{
				menu.Items.Insert(1, version);
			}
		}

		private void Menu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			UpdateVersions();
		}

		private void OnVersionClick(object sender, EventArgs e)
		{
			var menuItem = (ToolStripMenuItem)sender;
			nvm.Use(menuItem.Name);

			UpdateVersions();
			/*var versions = nvm.Versions();
			
			foreach(ToolStripItem item in menu.Items)
			{
				if(item.GetType() == typeof(ToolStripMenuItem)) {
					foreach (var version in versions)
					{
						if (item.Name == version.Key)
						{
							item.Text = version.Value;
						}
					}
				}
			}
			*/
		}

		private void OnExitClick(object sender, EventArgs e)
		{
			notifyIcon.Visible = false;

			Application.Exit();
		}

		
	}
}