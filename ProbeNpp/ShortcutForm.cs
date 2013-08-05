using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProbeNpp
{
	public partial class ShortcutForm : Form
	{
		private class ShortcutAction
		{
			public Keys key;
			public string description;
			public Action action;
		}
		private List<ShortcutAction> _actions = new List<ShortcutAction>();

		public Action SelectedAction { get; set; }

		public ShortcutForm()
		{
			InitializeComponent();
		}

		public void AddAction(Keys key, string description, Action action)
		{
			var sa = new ShortcutAction { key = key, description = description, action = action };
			_actions.Add(sa);

			var lvi = new ListViewItem(key.ToString());
			lvi.SubItems.Add(description);
			lvi.Tag = sa;
			lstActions.Items.Add(lvi);
		}

		private void lstActions_ItemActivate(object sender, EventArgs e)
		{
			try
			{
				var sa = (from l in lstActions.SelectedItems.Cast<ListViewItem>() select l.Tag as ShortcutAction).FirstOrDefault();
				if (sa != null) ExecuteAction(sa);
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void ExecuteAction(ShortcutAction sa)
		{
			if (sa != null)
			{
				SelectedAction = sa.action;
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		private void ShortcutForm_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				CheckKey(e.KeyCode);
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void lstActions_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				CheckKey(e.KeyCode);
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void CheckKey(Keys key)
		{
			if (key == Keys.Escape)
			{
				DialogResult = DialogResult.Cancel;
				Close();
			}

			var sa = (from a in _actions where a.key == key select a).FirstOrDefault();
			if (sa != null) ExecuteAction(sa);
		}

	}
}
