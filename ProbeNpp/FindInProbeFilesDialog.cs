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

namespace ProbeNpp
{
	internal partial class FindInProbeFilesDialog : Form
	{
		private string _searchText = string.Empty;
		private FindInProbeFilesMethod _method = ProbeNppPlugin.Instance.Settings.FindInProbeFiles.Method;
		private bool _matchCase = ProbeNppPlugin.Instance.Settings.FindInProbeFiles.MatchCase;
		private bool _matchWholeWord = ProbeNppPlugin.Instance.Settings.FindInProbeFiles.MatchWholeWord;
		private bool _onlyProbeFiles = ProbeNppPlugin.Instance.Settings.FindInProbeFiles.OnlyProbeFiles;
		private Regex _regex = null;

		private const int k_maxMru = 10;

		public FindInProbeFilesDialog()
		{
			InitializeComponent();
		}

		private void FindInProbeFilesDialog_Load(object sender, EventArgs e)
		{
			try
			{
				foreach (var mru in LoadMru()) cmbSearchText.Items.Add(mru);
				cmbSearchText.Text = _searchText;
				cmbMethod.InitForEnum<FindInProbeFilesMethod>(ProbeNppPlugin.Instance.Settings.FindInProbeFiles.Method);
				chkMatchCase.Checked = _matchCase;
				chkMatchWholeWord.Checked = _matchWholeWord;
				chkOnlyProbeFiles.Checked = _onlyProbeFiles;

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

				_method = cmbMethod.GetEnumValue<FindInProbeFilesMethod>();
				_matchCase = chkMatchCase.Checked;
				_matchWholeWord = chkMatchWholeWord.Checked;
				_onlyProbeFiles = chkOnlyProbeFiles.Checked;

				switch (_method)
				{
					case FindInProbeFilesMethod.RegularExpression:
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
					case FindInProbeFilesMethod.CodeFriendly:
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

		public Regex SearchRegex
		{
			get { return _regex; }
		}

		public FindInProbeFilesMethod Method
		{
			get { return _method; }
		}

		public bool MatchCase
		{
			get { return _matchCase; }
		}

		public bool MatchWholeWord
		{
			get { return _matchWholeWord; }
		}

		public bool OnlyProbeFiles
		{
			get { return _onlyProbeFiles; }
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
