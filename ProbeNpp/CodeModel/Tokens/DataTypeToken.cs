using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class DataTypeToken : Token, IGroupToken
    {
        private Token[] _tokens;

        public DataTypeToken(Token parent, Scope scope, IEnumerable<Token> tokens)
            : base(parent, scope, new Span(tokens.First().Span.Start, tokens.Last().Span.End))
        {
            _tokens = tokens.ToArray();
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("DataType");
            foreach (var token in _tokens) token.DumpTree(xml);
            xml.WriteEndElement();
        }

        IEnumerable<Token> IGroupToken.SubTokens
        {
            get { return _tokens; }
        }

        public static DataTypeToken CreateFromDefinedWord(Token parent, Scope scope, Span span, string text)
        {
            return new DataTypeToken(parent, scope, new Token[] { new IdentifierToken(parent, scope, span, text) });
        }

        public static Token Parse(Token parent, Scope scope, DataTypeKeywordToken firstToken)
        {
            var startPos = scope.File.Position;

            if (firstToken.Text == "enum")
            {
                var tokens = new List<Token>();
                tokens.Add(firstToken);
                while (true)
                {
                    var token = scope.File.ParseSingleToken(parent, scope);
                    if (token == null) break;
                    else if (token.GetType() == typeof(DataTypeKeywordToken) && (token as DataTypeKeywordToken).Text == "proto") tokens.Add(token);
                    else if (token.GetType() == typeof(DataTypeKeywordToken) && (token as DataTypeKeywordToken).Text == "nowarn") tokens.Add(token);
                    else if (token.GetType() == typeof(BracesToken)) tokens.Add(token);
                    else
                    {
                        scope.File.Position = tokens.Last().Span.End;
                        break;
                    }
                }
                return new DataTypeToken(parent, scope, tokens);
            }
            else if (firstToken.Text == "numeric")
            {
                var tokens = new List<Token>();
                tokens.Add(firstToken);

                var token = scope.File.ParseSingleToken(parent, scope);
                if (token == null) return firstToken;
                if (token.GetType() == typeof(BracketsToken))
                {
                    tokens.Add(token);
                    token = scope.File.ParseSingleToken(parent, scope);
                    if (token == null) return new DataTypeToken(parent, scope, tokens);
                }

                var done = false;
                while (!done)
                {
                    if (token.GetType() == typeof(DataTypeKeywordToken))
                    {
                        switch ((token as DataTypeKeywordToken).Text)
                        {
                            case "unsigned":
                            case "currency":
                            case "local_currency":
                            case "LEADINGZEROS":
                            case "PROBE":
                                tokens.Add(token);
                                break;
                            default:
                                scope.File.Position = tokens.Last().Span.End;
                                done = true;
                                break;
                        }
                    }
                    else if (token.GetType() == typeof(IdentifierToken))
                    {
                        switch ((token as IdentifierToken).Text)
                        {
                            case "PROBE":
                                tokens.Add(token);
                                break;
                            default:
                                scope.File.Position = tokens.Last().Span.End;
                                done = true;
                                break;
                        }
                    }
                    else if (token.GetType() == typeof(StringLiteralToken))
                    {
                        tokens.Add(token);
                    }
                    else
                    {
                        scope.File.Position = tokens.Last().Span.End;
                        done = true;
                    }

                    if (!done)
                    {
                        token = scope.File.ParseSingleToken(parent, scope);
                        if (token == null) return new DataTypeToken(parent, scope, tokens);
                    }
                }

                return new DataTypeToken(parent, scope, tokens);
            }
            else if (firstToken.Text == "char")
            {
                var tokens = new List<Token>();
                tokens.Add(firstToken);

                var token = scope.File.ParseSingleToken(parent, scope);
                if (token == null) return firstToken;

                if (token.GetType() == typeof(BracketsToken))
                {
                    tokens.Add(token);
                    token = scope.File.ParseSingleToken(parent, scope);
                    if (token == null) return new DataTypeToken(parent, scope, tokens);
                }

                if (token.GetType() == typeof(StringLiteralToken))
                {
                    tokens.Add(token);
                }
                else scope.File.Position = tokens.Last().Span.End;

                return new DataTypeToken(parent, scope, tokens);
            }
            else if (firstToken.Text == "date")
            {
                var tokens = new List<Token>();
                tokens.Add(firstToken);

                var token = scope.File.ParseSingleToken(parent, scope);
                if (token == null) return token;

                if (token.GetType() == typeof(NumberToken))
                {
                    tokens.Add(token);
                    token = scope.File.ParseSingleToken(parent, scope);
                    if (token == null) return new DataTypeToken(parent, scope, tokens);
                }

                var done = false;
                while (!done)
                {
                    if (token.GetType() == typeof(StringLiteralToken))
                    {
                        tokens.Add(token);
                    }
                    else if (token.GetType() == typeof(DataTypeKeywordToken))
                    {
                        switch ((token as DataTypeKeywordToken).Text)
                        {
                            case "shortform":
                            case "longform":
                            case "alternate":
                            case "PROBE":
                                tokens.Add(token);
                                break;
                            default:
                                scope.File.Position = tokens.Last().Span.End;
                                done = true;
                                break;
                        }
                    }
                    else
                    {
                        scope.File.Position = tokens.Last().Span.End;
                        done = true;
                    }

                    if (!done)
                    {
                        token = scope.File.ParseSingleToken(parent, scope);
                        if (token == null) break;
                    }
                }

                return new DataTypeToken(parent, scope, tokens);
            }
            else if (firstToken.Text == "like")
            {
                var tableNameToken = scope.File.ParseSingleToken(parent, scope);
                if (tableNameToken.GetType() != typeof(IdentifierToken))
                {
                    scope.File.Position = startPos;
                    return firstToken;
                }

                var dotToken = scope.File.ParseSingleToken(parent, scope);
                if (dotToken.GetType() != typeof(DotToken))
                {
                    scope.File.Position = startPos;
                    return firstToken;
                }

                var fieldNameToken = scope.File.ParseSingleToken(parent, scope);
                if (fieldNameToken.GetType() != typeof(IdentifierToken))
                {
                    scope.File.Position = startPos;
                    return firstToken;
                }

                return new DataTypeToken(parent, scope, new Token[] { firstToken, tableNameToken, dotToken, fieldNameToken });
            }

            return firstToken;
        }
    }
}
