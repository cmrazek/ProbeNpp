using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class ReplaceSetToken : Token, IGroupToken
    {
        private List<Token> _tokens = new List<Token>();
        private ReplaceToken _replaceToken = null;
        private ReplaceToken _withToken = null;
        private ReplaceToken _endReplaceToken = null;
        private List<Token> _oldTokens = new List<Token>();
        private List<Token> _newTokens = new List<Token>();

        private ReplaceSetToken(Token parent, Scope scope, Span span)
            : base(parent, scope, span)
        {
        }

        public static ReplaceSetToken Parse(Token parent, Scope scope, PreprocessorToken replaceToken)
        {
#if DEBUG
            if (replaceToken == null) throw new ArgumentNullException("replaceToken");
#endif

            var file = scope.File;
            var startPos = replaceToken.Span.Start;

            var scopeIndent = scope.Indent();

            var ret = new ReplaceSetToken(parent, scope, new Span(startPos, startPos));
            ret._replaceToken = new ReplaceToken(ret, scope, replaceToken.Span, ret);
            ret._tokens.Add(ret._replaceToken);

            var done = false;

            while (!file.EndOfFile && !done)
            {
                var token = file.ParseToken(ret, scopeIndent);
                if (token == null) break;
                if (token.GetType() == typeof(PreprocessorToken))
                {
                    if ((token as PreprocessorToken).Text == "#with" && ret._withToken == null)
                    {
                        ret._withToken = new ReplaceToken(ret, scope, token.Span, ret);
                        ret._tokens.Add(ret._withToken);
                    }
                    else if ((token as PreprocessorToken).Text == "#endreplace")
                    {
                        ret._endReplaceToken = new ReplaceToken(ret, scope, token.Span, ret);
                        ret._tokens.Add(ret._endReplaceToken);
                        done = true;
                    }
                    else
                    {
                        ret._tokens.Add(token);
                        if (ret._withToken == null) ret._oldTokens.Add(token);
                        else ret._newTokens.Add(token);
                    }
                }
                else
                {
                    ret._tokens.Add(token);
                    if (ret._withToken == null) ret._oldTokens.Add(token);
                    else ret._newTokens.Add(token);
                }
            }

            ret.Span = new Span(startPos, file.Position);
            return ret;
        }

        IEnumerable<Token> IGroupToken.SubTokens
        {
            get { return _tokens; }
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("ReplaceSet");
            xml.WriteAttributeString("span", Span.ToString());
            foreach (var t in _tokens) t.DumpTree(xml);
            xml.WriteEndElement();
        }

        public override IEnumerable<Span> HiddenRegions
        {
            get
            {
                if (_replaceToken != null && _withToken != null && _replaceToken.Span.End.LineNum + 1 < _withToken.Span.Start.LineNum)
                {
                    yield return new Span(_replaceToken.Span.End, Scope.File.FindEndOfPreviousLine(_withToken.Span.Start));
                }
                if (_withToken != null && _endReplaceToken != null && _withToken.Span.End.LineNum + 1 < _endReplaceToken.Span.Start.LineNum)
                {
                    yield return new Span(_withToken.Span.End, Scope.File.FindEndOfPreviousLine(_endReplaceToken.Span.Start));
                }
            }
        }

        public override bool BreaksStatement
        {
            get
            {
                return true;
            }
        }

        internal class ReplaceToken : Token, IBraceMatchingToken
        {
            private ReplaceSetToken _replaceSet;

            public ReplaceToken(Token parent, Scope scope, Span span, ReplaceSetToken replaceSet)
                : base(parent, scope, span)
            {
                _replaceSet = replaceSet;
            }

            IEnumerable<Token> IBraceMatchingToken.BraceMatchingTokens
            {
                get
                {
                    var ret = new List<Token>();
                    ret.Add(_replaceSet._replaceToken);
                    if (_replaceSet._withToken != null) ret.Add(_replaceSet._withToken);
                    if (_replaceSet._endReplaceToken != null) ret.Add(_replaceSet._endReplaceToken);
                    return ret;
                }
            }

            public override void DumpTree(System.Xml.XmlWriter xml)
            {
                xml.WriteStartElement("Replace");
                xml.WriteAttributeString("span", Span.ToString());
                xml.WriteEndElement();
            }

            public override bool BreaksStatement
            {
                get
                {
                    return true;
                }
            }
        }
    }
}
