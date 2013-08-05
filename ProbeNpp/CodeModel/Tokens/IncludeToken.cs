using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProbeNpp.CodeModel.Tokens
{
	internal class IncludeToken : Token
	{
		private PreprocessorToken _prepToken;
		private string _fileName;
		private bool _processed = false;
		private bool _searchFileDir = false;

		public class IncludeDef
		{
			public string SourceFileName { get; set; }
			public string FileName { get; set; }
			public bool SearchFileDir { get; set; }
		}

		private IncludeToken(Token parent, Scope scope, Span span, PreprocessorToken prepToken, string fileName, bool searchFileDir)
			: base(parent, scope, span)
		{
			_prepToken = prepToken;
			_fileName = fileName;
			_searchFileDir = searchFileDir;
		}

		private static Regex _rxAngleBrackets = new Regex(@"^\<([^>]+)\>");
		private static Regex _rxQuotes = new Regex(@"^""([^""]+)""");

		public static IncludeToken Parse(Token parent, Scope scope, PreprocessorToken prepToken)
		{
			var startPos = scope.File.Position;

			scope.File.SeekEndOfLine();
			var lineText = scope.File.GetText(new Span(startPos, scope.File.Position)).Trim();

			var fileName = "";
			var searchFileDir = false;

			var match = _rxAngleBrackets.Match(lineText);
			if (match.Success)
			{
				fileName = match.Groups[1].Value.Trim();
			}
			else if ((match = _rxQuotes.Match(lineText)).Success)
			{
				fileName = match.Groups[1].Value.Trim();
				searchFileDir = true;
			}

			return new IncludeToken(parent, scope, new Span(prepToken.Span.Start, scope.File.Position), prepToken, fileName, searchFileDir);
		}

		public override bool BreaksStatement
		{
			get
			{
				return true;
			}
		}

		public override void DumpTree(System.Xml.XmlWriter xml)
		{
			xml.WriteStartElement("Include");
			xml.WriteAttributeString("fileName", _fileName);
			xml.WriteAttributeString("span", Span.ToString());
			xml.WriteEndElement();
		}

		public bool Processed
		{
			get { return _processed; }
		}

		public override IEnumerable<IncludeDef> GetUnprocessedIncludes()
		{
			if (!_processed && !string.IsNullOrEmpty(_fileName))
			{
				return new IncludeDef[] { new IncludeDef { SourceFileName = Scope.File.FileName, FileName = _fileName, SearchFileDir = _searchFileDir } };
			}
			else
			{
				return new IncludeDef[0];
			}
		}
	}
}
