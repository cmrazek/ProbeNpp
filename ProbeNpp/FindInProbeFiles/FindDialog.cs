using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace ProbeNpp.FindInProbeFiles
{
	internal partial class FindDialog : Form
	{
		private string _searchText = string.Empty;
		private FindMethod _method = ProbeNppPlugin.Instance.Settings.FindInProbeFiles.Method;
		private bool _matchCase = ProbeNppPlugin.Instance.Settings.FindInProbeFiles.MatchCase;
		private bool _matchWholeWord = ProbeNppPlugin.Instance.Settings.FindInProbeFiles.MatchWholeWord;
		private bool _onlyProbeFiles = ProbeNppPlugin.Instance.Settings.FindInProbeFiles.OnlyProbeFiles;
		private Regex _regex = null;
		private string _includeExtensions = ProbeNppPlugin.Instance.Settings.FindInProbeFiles.IncludeExtensions;
		private string _excludeExtensions = ProbeNppPlugin.Instance.Settings.FindInProbeFiles.ExcludeExtensions;

		private const int k_maxMru = 10;

		public FindDialog()
		{
			InitializeComponent();

			if (string.IsNullOrWhiteSpace(_includeExtensions)) _includeExtensions = "*";
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		private void FindInProbeFilesDialog_Load(object sender, EventArgs e)
		{
			try
			{
				foreach (var mru in LoadMru()) cmbSearchText.Items.Add(mru);
				cmbSearchText.Text = _searchText;
				cmbMethod.InitForEnum<FindMethod>(ProbeNppPlugin.Instance.Settings.FindInProbeFiles.Method);
				chkMatchCase.Checked = _matchCase;
				chkMatchWholeWord.Checked = _matchWholeWord;
				chkOnlyProbeFiles.Checked = _onlyProbeFiles;
				txtIncludeExtensions.Text = _includeExtensions;
				txtExcludeExtensions.Text = _excludeExtensions;

				EnableControls();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void EnableControls()
		{
			btnOk.Enabled = !string.IsNullOrEmpty(cmbSearchText.Text);
			//chkMatchWholeWord.Enabled = cmbMethod.GetEnumValue<FindInProbeFilesMethod>() != FindInProbeFilesMethod.RegularExpression;
		}

		private string[] LoadMru()
		{
			try
			{
				var xml = ProbeNppPlugin.Instance.Settings.FindInProbeFiles.MRU;
				if (string.IsNullOrWhiteSpace(xml)) return new string[] { };
				return XmlUtil.Deserialize<string[]>(xml);
			}
			catch (Exception)
			{
				return new string[] { };
			}
		}

		private void SaveMru(string[] values)
		{
			ProbeNppPlugin.Instance.Settings.FindInProbeFiles.MRU = XmlUtil.Serialize(values);
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			try
			{
				_searchText = cmbSearchText.Text;
				if (string.IsNullOrEmpty(_searchText)) return;

				_method = cmbMethod.GetEnumValue<FindMethod>();
				_matchCase = chkMatchCase.Checked;
				_matchWholeWord = chkMatchWholeWord.Checked;
				_onlyProbeFiles = chkOnlyProbeFiles.Checked;
				_includeExtensions = string.IsNullOrWhiteSpace(txtIncludeExtensions.Text) ? "*" : txtIncludeExtensions.Text;
				_excludeExtensions = txtExcludeExtensions.Text;

				switch (_method)
				{
					case FindMethod.RegularExpression:
						try
						{
							_regex = new Regex(_searchText, chkMatchCase.Checked ? RegexOptions.None : RegexOptions.IgnoreCase);
						}
						catch (Exception ex)
						{
							Errors.Show(this, ex, "Invalid regular expression.");
							_regex = null;
							return;
						}
						break;
					case FindMethod.CodeFriendly:
						_regex = GenerateCodeFriendlyRegex();
						break;
					default:
						_regex = null;
						break;
				}

				var mru = new List<string>();
				mru.Add(_searchText);
				foreach (string item in cmbSearchText.Items)
				{
					if (item != _searchText && mru.Count < k_maxMru) mru.Add(item);
				}
				SaveMru(mru.ToArray());

				ProbeNppPlugin.Instance.Settings.FindInProbeFiles.Method = _method;
				ProbeNppPlugin.Instance.Settings.FindInProbeFiles.MatchCase = _matchCase;
				ProbeNppPlugin.Instance.Settings.FindInProbeFiles.MatchWholeWord = _matchWholeWord;
				ProbeNppPlugin.Instance.Settings.FindInProbeFiles.OnlyProbeFiles = _onlyProbeFiles;
				ProbeNppPlugin.Instance.Settings.FindInProbeFiles.IncludeExtensions = _includeExtensions;
				ProbeNppPlugin.Instance.Settings.FindInProbeFiles.ExcludeExtensions = _excludeExtensions;

				DialogResult = DialogResult.OK;
				Close();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			try
			{
				DialogResult = DialogResult.Cancel;
				Close();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void cmbSearchText_TextUpdate(object sender, EventArgs e)
		{
			try
			{
				EnableControls();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void cmbMethod_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				EnableControls();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void cmbSearchText_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				EnableControls();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		#region Search Option Properties
		public string SearchText
		{
			get { return _searchText; }
			set { _searchText = value; }
		}

		public FindArgs CreateFindArgs(ResultsPanel panel)
		{
			return new FindInProbeFiles.FindArgs
			{
				SearchText = _searchText,
				SearchRegex = _regex,
				Method = _method,
				MatchCase = _matchCase,
				MatchWholeWord = _matchWholeWord,
				ProbeFilesOnly = _onlyProbeFiles,
				IncludeExtensions = _includeExtensions,
				ExcludeExtensions = _excludeExtensions,
				Panel = panel
			};
		}
		#endregion

		#region Code Friendly Regex
		private enum CharClass
		{
			WhiteSpace,
			Word,
			Symbol
		}

		private Regex GenerateCodeFriendlyRegex()
		{
			var sb = new StringBuilder();
			var chClass = CharClass.WhiteSpace;
			var lastClass = CharClass.WhiteSpace;
			var lastNonWhiteClass = CharClass.WhiteSpace;

			foreach (var ch in _searchText)
			{
				switch (chClass = GetCharClass(ch))
				{
					case CharClass.Word:
						if (lastClass == CharClass.Word)
						{
							// Part of the same word.
						}
						else if (lastClass == CharClass.Symbol)
						{
							sb.Append("\\s*");
						}
						else if (lastClass == CharClass.WhiteSpace)
						{
							if (lastNonWhiteClass == CharClass.Word)
							{
								sb.Append("\\s+");
							}
							else if (lastNonWhiteClass == CharClass.Symbol)
							{
								sb.Append("\\s*");
							}
						}
						sb.Append(EscapeCodeFriendlyChar(ch));
						break;

					case CharClass.Symbol:
						if (lastClass == CharClass.Symbol || lastClass == CharClass.Word ||
							lastNonWhiteClass == CharClass.Symbol || lastNonWhiteClass == CharClass.Word)
						{
							sb.Append("\\s*");
						}
						sb.Append(EscapeCodeFriendlyChar(ch));
						break;
				}

				lastClass = chClass;
				if (lastClass != CharClass.WhiteSpace) lastNonWhiteClass = lastClass;
			}

			return new Regex(sb.ToString(), (_matchCase ? RegexOptions.None : RegexOptions.IgnoreCase) | RegexOptions.Singleline);
		}

		private CharClass GetCharClass(char ch)
		{
			if (Char.IsWhiteSpace(ch)) return CharClass.WhiteSpace;
			if (Char.IsLetterOrDigit(ch) || ch == '_') return CharClass.Word;
			return CharClass.Symbol;
		}

		private string EscapeCodeFriendlyChar(char ch)
		{
			switch (ch)
			{
				case '[':
				case ']':
				case '\\':
				case '^':
				case '$':
				case '.':
				case '|':
				case '?':
				case '*':
				case '+':
				case '(':
				case ')':
				case '{':
				case '}':
					return string.Concat("\\", ch);
				default:
					return ch.ToString();
			}
		}
		#endregion

	}
}
