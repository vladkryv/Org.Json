using System.Collections.Generic;
using System.Text;

namespace Org.Json
{
	public class JSONStringer
	{
		internal readonly StringBuilder Out = new StringBuilder();

		internal enum Scope
		{
			EmptyArray,
			NonemptyArray,
			EmptyObject,
			DanglingKey,
			NonemptyObject,
			Null
		}

		private readonly IList<Scope> _stack = new List<Scope>();
		private readonly string _indent;

		public JSONStringer()
		{
			_indent = null;
		}

		internal JSONStringer(int indentSpaces)
		{
			char[] indentChars = new char[indentSpaces];
			Arrays.Fill(indentChars, ' ');
			_indent = new string(indentChars);
		}

		public virtual JSONStringer Array()
		{
			return Open(Scope.EmptyArray, "[");
		}

		public virtual JSONStringer EndArray()
		{
			return Close(Scope.EmptyArray, Scope.NonemptyArray, "]");
		}

		public virtual JSONStringer Object()
		{
			return Open(Scope.EmptyObject, "{");
		}

		public virtual JSONStringer EndObject()
		{
			return Close(Scope.EmptyObject, Scope.NonemptyObject, "}");
		}

		internal virtual JSONStringer Open(Scope empty, string openBracket)
		{
			if (_stack.Count == 0 && Out.Length > 0)
			{
				throw new JSONException("Nesting problem: multiple top-level roots");
			}
			BeforeValue();
			_stack.Add(empty);
			Out.Append(openBracket);
			return this;
		}

		internal virtual JSONStringer Close(Scope empty, Scope nonempty, string closeBracket)
		{
			Scope context = Peek();
			if (context != nonempty && context != empty)
			{
				throw new JSONException("Nesting problem");
			}

			_stack.RemoveAt(_stack.Count - 1);
			if (context == nonempty)
			{
				Newline();
			}
			Out.Append(closeBracket);
			return this;
		}

		private Scope Peek()
		{
			if (_stack.Count == 0)
			{
				throw new JSONException("Nesting problem");
			}
			return _stack[_stack.Count - 1];
		}

		private void ReplaceTop(Scope topOfStack)
		{
			_stack[_stack.Count - 1] = topOfStack;
		}

		public virtual JSONStringer Value(object value)
		{
			if (_stack.Count == 0)
			{
				throw new JSONException("Nesting problem");
			}

			if (value is JSONArray)
			{
				((JSONArray) value).WriteTo(this);
				return this;
			}

			if (value is JSONObject)
			{
				((JSONObject) value).WriteTo(this);
				return this;
			}

			BeforeValue();

			if (value == null || value is bool || value == JSONObject.Null)
			{
				Out.Append(value);

			}
			else if (NumberHelper.IsNumber(value))
			{
				Out.Append(JSONObject.HideNumberToString(value));
			}
			else
			{
				String(value.ToString());
			}

			return this;
		}

		public virtual JSONStringer Value(bool value)
		{
			if (_stack.Count == 0)
			{
				throw new JSONException("Nesting problem");
			}
			BeforeValue();
			Out.Append(value);
			return this;
		}

		public virtual JSONStringer Value(double value)
		{
			if (_stack.Count == 0)
			{
				throw new JSONException("Nesting problem");
			}
			BeforeValue();
			Out.Append(JSONObject.NumberToString(value));
			return this;
		}

		public virtual JSONStringer Value(long value)
		{
			if (_stack.Count == 0)
			{
				throw new JSONException("Nesting problem");
			}
			BeforeValue();
			Out.Append(value);
			return this;
		}

		private void String(string value)
		{
			Out.Append("\"");
			for (int i = 0, length = value.Length; i < length; i++)
			{
				char c = value[i];
				switch (c)
				{
					case '"':
					case '\\':
					case '/':
						Out.Append('\\').Append(c);
						break;

					case '\t':
						Out.Append("\\t");
						break;

					case '\b':
						Out.Append("\\b");
						break;

					case '\n':
						Out.Append("\\n");
						break;

					case '\r':
						Out.Append("\\r");
						break;

					case '\f':
						Out.Append("\\f");
						break;

					default:
						if (c <= (char)0x1F)
						{
							Out.Append(string.Format("\\u{0:x4}", (int) c));
						}
						else
						{
							Out.Append(c);
						}
						break;
				}

			}
			Out.Append("\"");
		}

		private void Newline()
		{
			if (ReferenceEquals(_indent, null))
			{
				return;
			}

			Out.Append("\n");
			for (int i = 0; i < _stack.Count; i++)
			{
				Out.Append(_indent);
			}
		}

		public virtual JSONStringer Key(string name)
		{
			if (ReferenceEquals(name, null))
			{
				throw new JSONException("Names must be non-null");
			}
			BeforeKey();
			String(name);
			return this;
		}

		private void BeforeKey()
		{
			Scope context = Peek();
			if (context == Scope.NonemptyObject)
			{
				Out.Append(',');
			}
			else if (context != Scope.EmptyObject)
			{
				throw new JSONException("Nesting problem");
			}
			Newline();
			ReplaceTop(Scope.DanglingKey);
		}

		private void BeforeValue()
		{
			if (_stack.Count == 0)
			{
				return;
			}

			Scope context = Peek();
			if (context == Scope.EmptyArray)
			{
				ReplaceTop(Scope.NonemptyArray);
				Newline();
			}
			else if (context == Scope.NonemptyArray)
			{
				Out.Append(',');
				Newline();
			}
			else if (context == Scope.DanglingKey)
			{
				Out.Append(ReferenceEquals(_indent, null) ? ":" : ": ");
				ReplaceTop(Scope.NonemptyObject);
			}
			else if (context != Scope.Null)
			{
				throw new JSONException("Nesting problem");
			}
		}

		public override string ToString()
		{
			return Out.Length == 0 ? null : Out.ToString();
		}
	}
}