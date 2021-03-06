﻿using System;
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
		private string _tableName;
		private Parser _parser;
		private List<ProbeTable> _tables = new List<ProbeTable>();
		private List<ProbeRelInd> _relInds = new List<ProbeRelInd>();

		public IEnumerable<ProbeTable> Tables
		{
			get { return _tables; }
		}

		public IEnumerable<ProbeRelInd> RelInds
		{
			get { return _relInds; }
		}

#if DEBUG
		public string Source
		{
			get { return _parser.Source; }
		}
#endif

		public void Process(string tableName)
		{
			_tableName = tableName;
			_tables.Clear();

			var procOutput = new StringOutput();
			var proc = new ProcessRunner();
			
			proc.CaptureOutput = true;
			proc.CaptureError = false;
			var ret = proc.CaptureProcess("pst", "/v " + tableName, ProbeEnvironment.ExeDir, procOutput);
			if (ret != 0) throw new ProbeException(string.Concat("PST returned error code ", ret, "."));

			_parser = new TokenParser.Parser(StripImplicitComments(procOutput.Text));
			while (_parser.Read())
			{
				if (_parser.TokenText == "create")
				{
					if (ReadCreateTable()) { }
					else if (ReadCreateIndex()) { }
					else if (ReadCreateRelationship()) { }
					else if (ReadCreateTimeRelationship()) { }
					else
					{
						ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Warning, "Unknown token '{0}' in PST output for table '{1}'.", _parser.TokenText, tableName);
					}
				}
			}
		}

		private string StripImplicitComments(string source)
		{
			var rxImplicit = new Regex(@"^//\s+implicit\s+.+\:\s*$");
			var rxCtSt = new Regex(@"^//\s+(server|client)\s+.+\:\s*$");
			var stripComments = false;

			var sb = new StringBuilder(source.Length);
			using (var reader = new StringReader(source))
			{
				while (true)
				{
					var line = reader.ReadLine();
					if (line == null) break;
					if (rxImplicit.IsMatch(line))
					{
						stripComments = true;
					}
					else if (rxCtSt.IsMatch(line))
					{
						stripComments = false;
					}
					else if (line.StartsWith("//"))
					{
						if (stripComments) sb.AppendLine(line.Substring(2));
						else sb.AppendLine(line);
					}
					else
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

				string tablePrompt = string.Empty;

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
							tablePrompt = Parser.StringLiteralToString(_parser.TokenText);
							break;
						case "comment":
							if (!_parser.Read() || _parser.TokenType != TokenType.StringLiteral) throw new ProbeException("Expected string to follow 'comment'.");
							break;
						default:
							throw new ProbeException(string.Format("Unexpected token '{0}' in create table statement.", _parser.TokenText));
					}
				}

				var table1 = new ProbeTable(tableNumber, tableName, tablePrompt);
				var table2 = tableNumber2 > 0 ? new ProbeTable(tableNumber2, tableName + "2", tablePrompt) : null;
				var tables = table2 == null ? new ProbeTable[] { table1 } : new ProbeTable[] { table1, table2 };
				foreach (var table in tables) table.SetFieldsLoaded();

				ReadTableFields(")", tables);

				_tables.AddRange(tables);
				return ret = true;
			}
			catch (ProbeException ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Warning, string.Format("Exception when processing 'create table' for table '{0}': {1}", _tableName, ex.ToString()));
				return ret = false;
			}
			finally
			{
				if (!ret) _parser.Position = startPos;
			}
		}

		private void ReadTableFields(string endToken, IEnumerable<ProbeTable> tables)
		{
			Position pos;
			string str;

			var tableDone = false;
			while (!tableDone)
			{
				if (!_parser.Read()) throw new ProbeException("Unexpected end of file in create table statement.");
				if (_parser.TokenText == endToken) { tableDone = true; break; }

				if (!_rxName.IsMatch(_parser.TokenText)) throw new ProbeException("Expected column name.");
				var fieldName = _parser.TokenText;

				string fieldDataType = ReadDataType();
				if (string.IsNullOrEmpty(fieldDataType)) throw new ProbeException(string.Format("Expected data type after column name '{0}'.", fieldName));

				// Mask
				string mask = null;
				if (_parser.Peek(out pos) && _parser.TokenType == TokenType.StringLiteral)
				{
					mask = _parser.TokenText;
					_parser.Position = pos;
				}

				var fieldPrompt = "";
				var fieldComment = "";

				// Attributes and optional parameters.
				var fieldDone = false;
				while (!fieldDone && _parser.Read())
				{
					if (_parser.TokenText == endToken)
					{
						fieldDone = tableDone = true;
						break;
					}

					switch (_parser.TokenText)
					{
						case ",":
							fieldDone = true;
							break;

						case "ALLCAPS":
						case "AUTOCAPS":
						case "LEADINGZEROS":
						case "NOCHANGE":
						case "NODISPLAY":
						case "NOECHO":
						case "NOINPUT":
						case "NOPICK":
						case "NOUSE":
						case "REQUIRED":
						case "form":
						case "formonly":
						case "zoom":
						case "endgroup":
							break;

						case "col":
						case "row":
							str = _parser.TokenText;
							if (!_parser.Read()) throw new ProbeException(string.Format("Unexpected end of file after '{0}'.", str));
							if (_parser.TokenText == "+" || _parser.TokenText == "-")
							{
								if (_parser.Read() && _parser.TokenType != TokenType.Number) throw new ProbeException(string.Format("Expected coordinate after '{0}'.", str));
							}
							else if (_parser.TokenType != TokenType.Number) throw new ProbeException(string.Format("Expected coordinate after '{0}'.", str));
							break;

						case "cols":
						case "rows":
							str = _parser.TokenText;
							if (!_parser.Read() && _parser.TokenType != TokenType.Number) throw new ProbeException(string.Format("Expected number after '{0}'.", str));
							break;

						case "prompt":
						case "comment":
						case "group":
							str = _parser.TokenText;
							if (!_parser.Read()) throw new ProbeException(string.Format("Unexpected end of file after '{0}'.", str));
							if (_parser.TokenType != TokenType.StringLiteral) throw new ProbeException(string.Format("Expected string literal after '{0}'.", str));
							if (str == "prompt") fieldPrompt = Parser.StringLiteralToString(_parser.TokenText);
							else if (str == "comment") fieldComment = Parser.StringLiteralToString(_parser.TokenText);
							break;

						default:
							throw new ProbeException(string.Format("Unrecognized token '{0}' in field definition.", _parser.TokenText));
					}
				}

				foreach (var table in tables)
				{
					table.AddField(new ProbeField(table, fieldName, fieldPrompt, fieldComment, fieldDataType));
				}
			}
		}

		private string ReadDataType()
		{
			var startPos = _parser.Position;
			string ret = null;
			try
			{
				Position pos;
				var sb = new StringBuilder();

				if (!_parser.Read()) return null;
				sb.Append(_parser.TokenText);

				switch (_parser.TokenText)
				{
					case "date":
						{
							// date [width] [shortform | longform | alternate | alternate longform | "mask"] [PROBE]

							if (!_parser.Peek(out pos)) return ret = sb.ToString();

							if (_parser.TokenType == TokenType.Number)
							{
								// Width
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
								if (!_parser.Peek(out pos)) return ret = sb.ToString();
							}

							if (_parser.TokenText == "shortform" || _parser.TokenText == "longform")
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
								if (!_parser.Peek(out pos)) return ret = sb.ToString();
							}
							else if (_parser.TokenText == "alternate")
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
								if (!_parser.Peek(out pos)) return ret = sb.ToString();
								if (_parser.TokenText == "longform")
								{
									sb.Append(" ");
									sb.Append(_parser.TokenText);
									_parser.Position = pos;
									if (!_parser.Peek(out pos)) return ret = sb.ToString();
								}
							}
							else if (_parser.TokenType == TokenType.StringLiteral)
							{
								// Mask
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
								if (!_parser.Peek(out pos)) return ret = sb.ToString();
							}

							if (_parser.TokenText == "PROBE")
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
							}
						}
						break;

					case "enum":
						{
							// enum [proto [nowarn]] { item[, ...] }

							if (!_parser.Read()) throw new ProbeException("Unexpected end of file in enum.");

							if (_parser.TokenText == "proto")
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								if (!_parser.Read()) throw new ProbeException("Unexpected end of file in enum.");
							}

							if (_parser.TokenText == "required")
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								if (!_parser.Read()) throw new ProbeException("Unexpected end of file in enum.");
							}

							if (_parser.TokenText == "nowarn")
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								if (!_parser.Read()) throw new ProbeException("Unexpected end of file in enum.");
							}

							if (_parser.TokenText != "{") throw new ProbeException("Expected '{' after enum.");
							sb.Append(" ");
							sb.Append("{");

							var needDelim = false;
							while (true)
							{
								if (!_parser.Read()) throw new ProbeException("Unexpected end of file in enum list.");
								if (_parser.TokenText == "}")
								{
									sb.Append(" }");
									break;
								}

								if (needDelim)
								{
									if (_parser.TokenText != ",") throw new ProbeException("Expected ',' in enum list.");
									sb.Append(",");
									needDelim = false;
								}
								else
								{
									if (_parser.TokenType != TokenType.StringLiteral && _parser.TokenType != TokenType.Word) throw new ProbeException("Expected identifier or string literal in enum list.");
									sb.Append(" ");
									sb.Append(_parser.TokenText);
									needDelim = true;
								}
							}
						}
						break;

					case "numeric":
						{
							// numeric( precision[,scale] ) [unsigned ["mask"] | currency | local_currency] [LEADINGZEROS] [PROBE]

							if (!_parser.Read() || _parser.TokenText != "(") return null;
							sb.Append("(");
							if (!_parser.Read() || _parser.TokenType != TokenType.Number) return null;
							sb.Append(_parser.TokenText);

							if (!_parser.Read()) return null;
							if (_parser.TokenText == ",")
							{
								sb.Append(_parser.TokenText);
								if (!_parser.Read() || _parser.TokenType != TokenType.Number) return null;
								sb.Append(_parser.TokenText);
								_parser.Read();
							}

							if (_parser.TokenText != ")") return null;
							sb.Append(_parser.TokenText);

							// Attributes
							if (!_parser.Peek(out pos)) return ret = sb.ToString();
							if (_parser.TokenText == "unsigned" || _parser.TokenText == "currency" || _parser.TokenText == "local_currency")
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
								if (!_parser.Peek(out pos)) return ret = sb.ToString();
							}

							// Mask
							if (_parser.TokenType == TokenType.StringLiteral)
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
								if (!_parser.Peek(out pos)) return ret = sb.ToString();
							}

							if (_parser.TokenText == "LEADINGZEROS")
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
								if (!_parser.Peek(out pos)) return ret = sb.ToString();
							}

							if (_parser.TokenText == "PROBE")
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
								if (!_parser.Peek(out pos)) return ret = sb.ToString();
							}
						}
						break;

					case "char":
						{
							// char( width ) [ "mask" ]

							if (!_parser.Peek(out pos)) return ret = sb.ToString();
							if (_parser.TokenText == "(")
							{
								sb.Append("(");
								_parser.Position = pos;
								if (!_parser.Read() || _parser.TokenType != TokenType.Number) return null;
								sb.Append(_parser.TokenText);
								if (!_parser.Read() || _parser.TokenText != ")") return null;
								sb.Append(_parser.TokenText);
								if (!_parser.Peek(out pos)) return ret = sb.ToString();
							}

							// Mask
							if (_parser.TokenType == TokenType.StringLiteral)
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
							}
						}
						break;

					case "time":
						{
							// time [width] [PROBE]

							if (!_parser.Peek(out pos)) return ret = sb.ToString();
							if (_parser.TokenType == TokenType.Number)
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
								if (!_parser.Peek(out pos)) return ret = sb.ToString();
							}

							if (_parser.TokenText == "PROBE")
							{
								sb.Append(" ");
								sb.Append(_parser.TokenText);
								_parser.Position = pos;
							}
						}
						break;

					case "graphic":
						{
							// graphic rows columns bytes

							if (!_parser.Read() || _parser.TokenType != TokenType.Number) throw new ProbeException("Expected rows after 'graphic'.");
							if (!_parser.Read() || _parser.TokenType != TokenType.Number) throw new ProbeException("Expected columns after 'graphic'.");
							if (!_parser.Read() || _parser.TokenType != TokenType.Number) throw new ProbeException("Expected bytes after 'graphic'.");
						}
						break;
				}

				return ret = sb.ToString();
			}
			finally
			{
				if (ret == null) _parser.Position = startPos;
			}
		}

		private bool ReadCreateIndex()
		{
			var startPos = _parser.Position;
			var ret = false;
			try
			{
				if (!_parser.Read()) return false;

				if (_parser.TokenText == "unique")
				{
					if (!_parser.Read()) return false;
				}
				else if (_parser.TokenText == "autosequence")
				{
					if (!_parser.Read()) return false;
				}

				if (_parser.TokenText != "index") return false;

				if (!_parser.Read() || _parser.TokenType != TokenType.Word) throw new ProbeException("Expected index name after 'create index'.");
				var indexName = _parser.TokenText;

				if (!_parser.Read() || _parser.TokenText != "on") throw new ProbeException("Expected 'on' after index name.");
				if (!_parser.Read() || _parser.TokenType != TokenType.Word) throw new ProbeException("Expected table name after create index 'on'.");
				if (!_parser.Read() || _parser.TokenText != "(") throw new ProbeException("Expected '(' after create index table name.");

				var needDelim = false;
				while (true)
				{
					if (!_parser.Read()) throw new ProbeException("Unexpected end of file in create index column list.");
					if (_parser.TokenText == ")") break;
					if (needDelim)
					{
						if (_parser.TokenText != ",") throw new ProbeException("Expected comma delimiter after create index column name.");
						needDelim = false;
					}
					else
					{
						if (_parser.TokenType != TokenType.Word) throw new ProbeException("Expected column name in create index column list.");
						needDelim = true;
					}
				}

				_relInds.Add(new ProbeRelInd { Name = indexName });
				return ret = true;
			}
			catch (ProbeException ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Warning, string.Format("Exception when processing 'create index' for table '{0}': {1}", _tableName, ex.ToString()));
				return ret = false;
			}
			finally
			{
				if (!ret) _parser.Position = startPos;
			}
		}

		private bool ReadCreateRelationship()
		{
			var startPos = _parser.Position;
			var ret = false;
			try
			{
				if (!_parser.Read() || _parser.TokenText != "relationship") return false;

				if (!_parser.Read() || _parser.TokenType != TokenType.Word) throw new ProbeException("Expected relationship name after 'create relationship'.");
				var relName = _parser.TokenText;

				if (!_parser.Read() || _parser.TokenType != TokenType.Number) throw new ProbeException("Expected relationship number after name.");
				var relNumber = int.Parse(_parser.TokenText);

				Position pos;
				string relPrompt = "";
				string relComment = "";
				if (_parser.Peek(out pos) && _parser.TokenText == "prompt")
				{
					_parser.Position = pos;
					if (!_parser.Read() || _parser.TokenType != TokenType.StringLiteral) throw new ProbeException("Expected string literal after 'prompt'.");
					relPrompt = Parser.StringLiteralToString(_parser.TokenText);
				}

				if (_parser.Peek(out pos) && _parser.TokenText == "comment")
				{
					_parser.Position = pos;
					if (!_parser.Read() || _parser.TokenType != TokenType.StringLiteral) throw new ProbeException("Expected string literal after 'comment'.");
					relComment = Parser.StringLiteralToString(_parser.TokenText);
				}

				if (!_parser.Read() || (_parser.TokenText != "one" && _parser.TokenText != "many")) throw new ProbeException("Expected 'one' or 'many' after relationship number.");
				if (!_parser.Read() || _parser.TokenType != TokenType.Word) throw new ProbeException("Expected parent table name for relationship.");
				if (!_parser.Read() || _parser.TokenText != "to") throw new ProbeException("Expected 'to' after relationship parent table name.");
				if (!_parser.Read() || (_parser.TokenText != "one" && _parser.TokenText != "many")) throw new ProbeException("Expected 'one' or 'many' after relationship 'to'.");
				if (!_parser.Read() || _parser.TokenType != TokenType.Word) throw new ProbeException("Expected child table name for relationship.");

				if (_parser.Peek(out pos) && _parser.TokenText == "order")
				{
					_parser.Position = pos;
					if (!_parser.Read() || _parser.TokenText != "by") throw new ProbeException("Expected 'by' after 'order' in relationship.");
					if (_parser.Peek(out pos) && _parser.TokenText == "unique")
					{
						_parser.Position = pos;
					}

					while (true)
					{
						if (!_parser.Read()) throw new ProbeException("Unexpected end of file in relationship column list.");
						if (_parser.TokenText == "(") break;
						if (_parser.TokenType != TokenType.Word) throw new ProbeException("Expected column name in relationship column list.");
					}
				}
				else
				{
					if (!_parser.Read() || _parser.TokenText != "(") throw new ProbeException("Expected '(' after relationship.");
				}

				var table = new ProbeTable(relNumber, relName, relPrompt);
				ReadTableFields(")", new ProbeTable[] { table });
				table.SetFieldsLoaded();
				_tables.Add(table);
				_relInds.Add(new ProbeRelInd { Name = relName });
				return ret = true;
			}
			catch (ProbeException ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Warning, string.Format("Exception when processing 'create relationship' for table '{0}': {1}", _tableName, ex.ToString()));
				return ret = false;
			}
			finally
			{
				if (!ret) _parser.Position = startPos;
			}
		}

		private bool ReadCreateTimeRelationship()
		{
			var startPos = _parser.Position;
			var ret = false;
			try
			{
				Position pos;

				if (!_parser.Read() || _parser.TokenText != "time") return false;
				if (!_parser.Read() || _parser.TokenText != "relationship") return false;

				if (!_parser.Read() || _parser.TokenType != TokenType.Word) throw new ProbeException("Expected time relationship name.");
				var relName = _parser.TokenText;

				if (!_parser.Read() || _parser.TokenType != TokenType.Number) throw new ProbeException("Expected time relationship number.");

				if (_parser.Peek(out pos) && _parser.TokenText == "prompt")
				{
					_parser.Position = pos;
					if (!_parser.Read() || _parser.TokenType != TokenType.StringLiteral) throw new ProbeException("Expected string literal after 'prompt'.");
				}

				if (_parser.Peek(out pos) && _parser.TokenText == "comment")
				{
					_parser.Position = pos;
					if (!_parser.Read() || _parser.TokenType != TokenType.StringLiteral) throw new ProbeException("Expected string literal after 'comment'.");
				}

				if (!_parser.Read() || _parser.TokenType != TokenType.Word) throw new ProbeException("Expected time relationship master table name.");
				if (!_parser.Read() || _parser.TokenText != "to") throw new ProbeException("Expected 'to' after time relationship master table.");
				if (!_parser.Read() || _parser.TokenType != TokenType.Word) throw new ProbeException("Expected time relationship transactions table name.");

				if (_parser.Peek(out pos) && _parser.TokenText == "order")
				{
					_parser.Position = pos;
					if (!_parser.Read() || _parser.TokenText != "by") throw new ProbeException("Expected 'by' after 'order'.");

					while (true)
					{
						if (!_parser.Read()) throw new ProbeException("Unexpected end of file in time relationship order by list.");
						if (_parser.TokenText == "(") break;
						if (_parser.TokenType != TokenType.Word) throw new ProbeException("Expected column name in time relationship order by list.");
					}
				}
				else
				{
					if (!_parser.Read() || _parser.TokenText != "(") throw new ProbeException("Expected '(' after time relationship.");
				}

				if (!_parser.Read() || _parser.TokenText != ")") throw new ProbeException("Expected ')' at end of time relationship.");

				_relInds.Add(new ProbeRelInd { Name = relName });
				return ret = true;
			}
			catch (ProbeException ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Warning, string.Format("Exception when processing 'create time relationship' for table '{0}': {1}", _tableName, ex.ToString()));
				return ret = false;
			}
			finally
			{
				if (!ret) _parser.Position = startPos;
			}
		}
	}
}
