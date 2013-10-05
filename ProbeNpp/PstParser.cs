using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NppSharp;
using ProbeNpp.TokenParser;

namespace ProbeNpp
{
	internal class PstParser
	{
		private Parser _parser;
		private List<ProbeField> _fields;

		public IEnumerable<ProbeField> Fields
		{
			get
			{
				if (_fields == null) throw new InvalidOperationException("PST has not been processed.");
				return _fields;
			}
		}

		public void Process(string tableName)
		{
			var procOutput = new StringOutput();
			var proc = new ProcessRunner();
			
			proc.CaptureOutput = true;
			proc.CaptureError = false;
			var ret = proc.CaptureProcess("pst", "/v", ProbeEnvironment.ExeDir, procOutput);
			if (ret != 0) throw new ProbeException(string.Concat("PST returned error code ", ret, "."));

			_parser = new TokenParser.Parser(StripImplicitComments(procOutput.Text));
			while (!_parser.Read())
			{
				if (_parser.TokenText == "create")
				{
					if (ReadCreateTable())
					{
					}
					else
					{
						// Unknown syntax for 'create'
					}
				}
			}
		}

		private string StripImplicitComments(string source)
		{
			var insideCreateTable = false;

			var sb = new StringBuilder(source.Length);
			using (var reader = new StringReader(source))
			{
				while (true)
				{
					var line = reader.ReadLine();
					if (line == null) break;
					if (line.StartsWith("create table"))
					{
						insideCreateTable = true;
						sb.Append(line);
					}
					else if (insideCreateTable && line.StartsWith("//"))
					{
						var str = line.Substring(2);
						if (str.Trim() != "implicit column(s):")
						{
							sb.AppendLine(str);
						}
					}
					else if (!line.StartsWith("//"))
					{
						sb.AppendLine(line);
					}
				}
			}

			return sb.ToString();
		}

		private Regex _rxName = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$");

		private bool ReadCreateTable()
		{
			var startPos = _parser.Position;
			var ret = false;
			try
			{
				if (!_parser.Read() || _parser.TokenText != "table") return false;

				// Table name
				if (!_parser.Read() || !_rxName.IsMatch(_parser.TokenText)) return false;
				var tableName = _parser.TokenText;

				// Table number
				if (!_parser.Read() || _parser.TokenType != TokenType.Number) throw new ProbeException("Expected table number to follow 'create table'.");
				var tableNumber = int.Parse(_parser.TokenText);
				var tableNumber2 = -1;

				var pos = _parser.Position;
				if (!_parser.Read()) return false;
				if (_parser.TokenType == TokenType.Number) tableNumber2 = int.Parse(_parser.TokenText);
				else _parser.Position = pos;

				var createDone = false;
				while (!createDone)
				{
					if (!_parser.Read()) throw new ProbeException("Unexpected end of file in create table statement.");
					switch (_parser.TokenText)
					{
						case "(":
							createDone = true;
							break;
						case "updates":
							break;
						case "database":
							if (!_parser.Read() || _parser.TokenType != TokenType.Number) throw new ProbeException("Expected database number to follow 'database'.");
							break;
						case "display":
							break;
						case "modal":
							break;
						case "nopick":
							break;
						case "pick":
							break;
						case "snapshot":
							if (!_parser.Read() || _parser.TokenType != TokenType.Number) throw new ProbeException("Expected frequency to follow 'snapshot'.");
							break;
						case "prompt":
							if (!_parser.Read() || _parser.TokenType != TokenType.StringLiteral) throw new ProbeException("Expected string to follow 'prompt'.");
							break;
						case "comment":
							if (!_parser.Read() || _parser.TokenType != TokenType.StringLiteral) throw new ProbeException("Expected string to follow 'comment'.");
							break;
						default:
							throw new ProbeException(string.Format("Unexpected token '{0}' in create table statement.", _parser.TokenText));
					}
				}

				var tableDone = false;
				while (!tableDone)
				{
					if (!_parser.Read()) throw new ProbeException("Unexpected end of file in create table statement.");
					if (_parser.TokenText == ")") { tableDone = true; break; }

					if (!_rxName.IsMatch(_parser.TokenText)) throw new ProbeException("Expected column name.");
					var fieldName = _parser.TokenText;

					if (!ReadDataType()) throw new ProbeException("Expected data type after column name.");

					// Mask
					string mask = null;
					if (_parser.Peek(out pos) && _parser.TokenType == TokenType.StringLiteral)
					{
						mask = _parser.TokenText;
						_parser.Position = pos;
					}

					// Attributes

					// Optional parameters.

					var fieldDone = false;
					while (!fieldDone && _parser.Read())
					{
						switch (_parser.TokenText)
						{
						}
					}
				}

				return ret = true;
			}
			catch (ProbeException ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Warning, ex.ToString());
				return ret = false;
			}
			finally
			{
				if (!ret) _parser.Position = startPos;
			}
		}

		private bool ReadDataType()
		{
			// TODO
			return false;
		}
	}
}
