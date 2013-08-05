using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
	internal abstract class Token
	{
		private Token _parent;
		private Scope _scope;
		private Span _span;

		public abstract void DumpTree(System.Xml.XmlWriter xml);

		public Token(Token parent, Scope scope, Span span)
		{
#if DEBUG
			if (parent == null && this.GetType() != typeof(CodeFile)) throw new ArgumentNullException("parent");
#endif

			_parent = parent;
			_scope = scope;
			_span = span;
		}

		public Token Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		public Scope Scope
		{
			get { return _scope; }
		}

		public CodeFile File
		{
			get { return _scope.File; }
		}

		public Span Span
		{
			get { return _span; }
			set { _span = value; }
		}

		public Token FindToken(Position pos)
		{
			if (!_span.Contains(pos)) return null;
			if (typeof(IGroupToken).IsAssignableFrom(GetType()))
			{
				var group = this as IGroupToken;
				foreach (var token in group.SubTokens)
				{
					var t = token.FindToken(pos);
					if (t != null) return t;
				}
				return this;
			}
			else
			{
				return this;
			}
		}

		public Token FindTokenOfType(Position pos, Type type)
		{
			if (!_span.Contains(pos)) return null;
			if (typeof(IGroupToken).IsAssignableFrom(GetType()))
			{
				var group = this as IGroupToken;
				foreach (var token in group.SubTokens)
				{
					var t = token.FindTokenOfType(pos, type);
					if (t != null) return t;
				}
				return type.IsAssignableFrom(GetType()) ? this : null;
			}
			else if (type.IsAssignableFrom(GetType()))
			{
				return this;
			}
			else
			{
				return null;
			}
		}

		public Token FindNearbyTokenOfType(Position pos, Type type)
		{
			if (!_span.ContainsNearby(pos)) return null;
			if (typeof(IGroupToken).IsAssignableFrom(GetType()))
			{
				var group = this as IGroupToken;
				foreach (var token in group.SubTokens)
				{
					var t = token.FindNearbyTokenOfType(pos, type);
					if (t != null) return t;
				}
				return type.IsAssignableFrom(GetType()) ? this : null;
			}
			else if (type.IsAssignableFrom(GetType()))
			{
				return this;
			}
			else
			{
				return null;
			}
		}

		public virtual IEnumerable<Span> HiddenRegions
		{
			get
			{
				if (typeof(IGroupToken).IsAssignableFrom(GetType()))
				{
					foreach (var token in (this as IGroupToken).SubTokens)
					{
						foreach (var region in token.HiddenRegions)
						{
							yield return region;
						}
					}
				}
			}
		}

		public virtual IEnumerable<FunctionToken> LocalFunctions
		{
			get
			{
				if (typeof(IGroupToken).IsAssignableFrom(GetType()))
				{
					foreach (var token in (this as IGroupToken).SubTokens)
					{
						foreach (var func in token.LocalFunctions)
						{
							yield return func;
						}
					}
				}
			}
		}

		public virtual IEnumerable<AutoCompletionItem> GetAutoCompletionItems(Position pos)
		{
			if (typeof(IAutoCompletionSource).IsAssignableFrom(GetType()))
			{
				foreach (var item in (this as IAutoCompletionSource).AutoCompletionItems) yield return item;
			}

			if (typeof(IGroupToken).IsAssignableFrom(GetType()))
			{
				foreach (var token in (this as IGroupToken).SubTokens)
				{
					if (!token.Scope.Hint.HasFlag(ScopeHint.NotOnRoot) || token.Span.Contains(pos))
					{
						foreach (var item in token.GetAutoCompletionItems(pos)) yield return item;
					}
				}
			}
		}

		public virtual IEnumerable<AutoCompletionItem> GetGlobalAutoCompletionItems()
		{
			if (_scope.Hint.HasFlag(ScopeHint.NotOnRoot)) yield break;

			if (typeof(IAutoCompletionSource).IsAssignableFrom(GetType()))
			{
				foreach (var item in (this as IAutoCompletionSource).AutoCompletionItems) yield return item;
			}

			if (typeof(IGroupToken).IsAssignableFrom(GetType()))
			{
				foreach (var token in (this as IGroupToken).SubTokens)
				{
					if (!token._scope.Hint.HasFlag(ScopeHint.NotOnRoot))
					{
						foreach (var item in token.GetGlobalAutoCompletionItems()) yield return item;
					}
				}
			}
		}

		public virtual string Text
		{
			get { return _scope.File.GetText(_span); }
		}

		public virtual bool BreaksStatement
		{
			get { return false; }
		}

		public virtual string NormalizedText
		{
			get
			{
				var text = Text;
				var sb = new StringBuilder(text.Length);
				var lastWhiteSpace = true;

				foreach (var ch in text)
				{
					if (char.IsWhiteSpace(ch))
					{
						if (!lastWhiteSpace)
						{
							sb.Append(" ");
							lastWhiteSpace = true;
						}
					}
					else
					{
						sb.Append(ch);
						lastWhiteSpace = false;
					}
				}

				return sb.ToString().TrimEnd();
			}
		}

		public IEnumerable<Token> FindTokens(Position pos)
		{
			if (!_span.Contains(pos)) yield break;

			if (typeof(IGroupToken).IsAssignableFrom(GetType()))
			{
				yield return this;

				foreach (var token in (this as IGroupToken).SubTokens)
				{
					foreach (var token2 in token.FindTokens(pos))
					{
						yield return token2;
					}
				}
			}
			else
			{
				yield return this;
			}
		}

		public virtual IEnumerable<FunctionSignature> GetFunctionSignatures()
		{
			if (typeof(IFunctionSignatureSource).IsAssignableFrom(GetType()))
			{
				foreach (var func in (this as IFunctionSignatureSource).FunctionSignatures)
				{
					yield return func;
				}
			}
			else if (typeof(IGroupToken).IsAssignableFrom(GetType()))
			{
				foreach (var token in (this as IGroupToken).SubTokens)
				{
					foreach (var func in token.GetFunctionSignatures())
					{
						yield return func;
					}
				}
			}
		}

		public virtual IEnumerable<IncludeToken.IncludeDef> GetUnprocessedIncludes()
		{
			if (typeof(IGroupToken).IsAssignableFrom(GetType()))
			{
				foreach (var token in (this as IGroupToken).SubTokens)
				{
					foreach (var includeDef in token.GetUnprocessedIncludes())
					{
						yield return includeDef;
					}
				}
			}
		}
	}
}
