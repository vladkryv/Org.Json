using System;
using System.Text;

namespace Org.Json
{
	public class JSONTokener
	{
		private readonly string _in;
		private int _pos;

		public JSONTokener(string @in)
		{

			if (!ReferenceEquals(@in, null) && @in.StartsWith("\ufeff", StringComparison.Ordinal))
			{
				@in = @in.Substring(1);
			}
			_in = @in;
		}

		public virtual object NextValue()
		{
			int c = NextCleanInternal();
			switch (c)
			{
				case -1:
					throw SyntaxError("End of input");

				case '{':
					return ReadObject();

				case '[':
					return ReadArray();

				case '\'':
				case '"':
					return NextString((char) c);

				default:
					_pos--;
					return ReadLiteral();
			}
		}

		private int NextCleanInternal()
		{
			while (_pos < _in.Length)
			{
				int c = _in[_pos++];
				switch (c)
				{
					case '\t':
					case ' ':
					case '\n':
					case '\r':
						continue;

					case '/':
						if (_pos == _in.Length)
						{
							return c;
						}

						char peek = _in[_pos];
						switch (peek)
						{
							case '*':
								_pos++;
								int commentEnd = _in.IndexOf("*/", _pos, StringComparison.Ordinal);
								if (commentEnd == -1)
								{
									throw SyntaxError("Unterminated comment");
								}
								_pos = commentEnd + 2;
								continue;

							case '/':
								_pos++;
								SkipToEndOfLine();
								continue;

							default:
								return c;
						}

					case '#':

						SkipToEndOfLine();
						continue;

					default:
						return c;
				}
			}

			return -1;
		}
		
		private void SkipToEndOfLine()
		{
			for (; _pos < _in.Length; _pos++)
			{
				char c = _in[_pos];
				if (c == '\r' || c == '\n')
				{
					_pos++;
					break;
				}
			}
		}

		public virtual string NextString(char quote)
		{

			StringBuilder builder = null;


			int start = _pos;

			while (_pos < _in.Length)
			{
				int c = _in[_pos++];
				if ((char)c == quote)
				{
					if (builder == null)
					{

						return _in.Substring(start, (_pos - 1) - start);
					}
					else
					{
						builder.Append(_in, start, _pos - 1);
						return builder.ToString();
					}
				}

				if (c == '\\')
				{
					if (_pos == _in.Length)
					{
						throw SyntaxError("Unterminated escape sequence");
					}
					if (builder == null)
					{
						builder = new StringBuilder();
					}
					builder.Append(_in, start, _pos - 1);
					builder.Append(ReadEscapeCharacter());
					start = _pos;
				}
			}

			throw SyntaxError("Unterminated string");
		}

		private char ReadEscapeCharacter()
		{
			char escaped = _in[_pos++];
			switch (escaped)
			{
				case 'u':
					if (_pos + 4 > _in.Length)
					{
						throw SyntaxError("Unterminated escape sequence");
					}
					string hex = _in.Substring(_pos, 4);
					_pos += 4;
					return (char) Convert.ToInt32(hex, 16);

				case 't':
					return '\t';

				case 'b':
					return '\b';

				case 'n':
					return '\n';

				case 'r':
					return '\r';

				case 'f':
					return '\f';

				default:
					return escaped;
			}
		}

		private object ReadLiteral()
		{
			string literal = NextToInternal("{}[]/\\:,=;# \t\f");

			if (literal.Length == 0)
			{
				throw SyntaxError("Expected literal value");
			}

			if ("null".Equals(literal, StringComparison.OrdinalIgnoreCase))
			{
				return JSONObject.Null;
			}

			if ("true".Equals(literal, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			if ("false".Equals(literal, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			
			if (literal.IndexOf('.') == -1)
			{
				int @base = 10;
				string number = literal;
				if (number.StartsWith("0x", StringComparison.Ordinal) || number.StartsWith("0X", StringComparison.Ordinal))
				{
					number = number.Substring(2);
					@base = 16;
				}
				else if (number.StartsWith("0", StringComparison.Ordinal) && number.Length > 1)
				{
					number = number.Substring(1);
					@base = 8;
				}
				try
				{
					long longValue = Convert.ToInt64(number, @base);
					if (longValue <= int.MaxValue && longValue >= int.MinValue)
					{
						return (int) longValue;
					}

					return longValue;
				}
				catch (FormatException)
				{
					// ignored
				}
			}
			
			try
			{
				return Convert.ToDouble(literal);
			}
			catch (FormatException)
			{
				// ignored
			}

			return literal;
		}

		private string NextToInternal(string excluded)
		{
			int start = _pos;
			for (; _pos < _in.Length; _pos++)
			{
				char c = _in[_pos];
				if (c == '\r' || c == '\n' || excluded.IndexOf(c) != -1)
				{
					return _in.Substring(start, _pos - start);
				}
			}
			return _in.Substring(start);
		}

		private JSONObject ReadObject()
		{
			JSONObject result = new JSONObject();


			int first = NextCleanInternal();
			if (first == '}')
			{
				return result;
			}

			if (first != -1)
			{
				_pos--;
			}

			while (true)
			{
				object name = NextValue();
				if (!(name is string))
				{
					if (name == null)
					{
						throw SyntaxError("Names cannot be null");
					}

					throw SyntaxError("Names must be strings, but " + name + " is of type " + name.GetType().FullName);
				}


				int separator = NextCleanInternal();
				if (separator != ':' && separator != '=')
				{
					throw SyntaxError("Expected ':' after " + name);
				}
				if (_pos < _in.Length && _in[_pos] == '>')
				{
					_pos++;
				}

				result.Put((string) name, NextValue());

				switch (NextCleanInternal())
				{
					case '}':
						return result;
					case ';':
					case ',':
						continue;
					default:
						throw SyntaxError("Unterminated object");
				}
			}
		}

		private JSONArray ReadArray()
		{
			JSONArray result = new JSONArray();
			bool hasTrailingSeparator = false;

			while (true)
			{
				switch (NextCleanInternal())
				{
					case -1:
						throw SyntaxError("Unterminated array");
					case ']':
						if (hasTrailingSeparator)
						{
							result.Put(null);
						}
						return result;
					case ',':
					case ';':

						result.Put(null);
						hasTrailingSeparator = true;
						continue;
					default:
						_pos--;
					break;
				}

				result.Put(NextValue());

				switch (NextCleanInternal())
				{
					case ']':
						return result;
					case ',':
					case ';':
						hasTrailingSeparator = true;
						continue;
					default:
						throw SyntaxError("Unterminated array");
				}
			}
		}

		public virtual JSONException SyntaxError(string message)
		{
			return new JSONException(message + this);
		}

		public override string ToString()
		{

			return " at character " + _pos + " of " + _in;
		}

		public virtual bool More()
		{
			return _pos < _in.Length;
		}

		public virtual char Next()
		{
			return _pos < _in.Length ? _in[_pos++] : '\0';
		}

		public virtual char Next(char c)
		{
			char result = Next();
			if (result != c)
			{
				throw SyntaxError("Expected " + c + " but was " + result);
			}
			return result;
		}

		public virtual char NextClean()
		{
			int nextCleanInt = NextCleanInternal();
			return nextCleanInt == -1 ? '\0' : (char) nextCleanInt;
		}

		public virtual string Next(int length)
		{
			if (_pos + length > _in.Length)
			{
				throw SyntaxError(length + " is out of bounds");
			}
			string result = _in.Substring(_pos, length);
			_pos += length;
			return result;
		}

		public virtual string NextTo(string excluded)
		{
			if (ReferenceEquals(excluded, null))
			{
				throw new NullReferenceException("excluded == null");
			}
			return NextToInternal(excluded).Trim();
		}

		public virtual string NextTo(char excluded)
		{
			return NextToInternal(excluded.ToString()).Trim();
		}

		public virtual void SkipPast(string thru)
		{
			int thruStart = _in.IndexOf(thru, _pos, StringComparison.Ordinal);
			_pos = thruStart == -1 ? _in.Length : (thruStart + thru.Length);
		}

		public virtual char SkipTo(char to)
		{
			int index = _in.IndexOf(to, _pos);
			if (index != -1)
			{
				_pos = index;
				return to;
			}

			return '\0';
		}

		public virtual void Back()
		{
			if (--_pos == -1)
			{
				_pos = 0;
			}
		}

		public static int Dehexchar(char hex)
		{
			if (hex >= '0' && hex <= '9')
			{
				return hex - '0';
			}

			if (hex >= 'A' && hex <= 'F')
			{
				return hex - 'A' + 10;
			}

			if (hex >= 'a' && hex <= 'f')
			{
				return hex - 'a' + 10;
			}

			return -1;
		}
	}
}