using System;
using System.Collections.Generic;

namespace Org.Json
{

	public class JSONObject
	{
		private const double NegativeZero = -0d;
		public static readonly object Null = new ObjectAnonymousInnerClass();

		private class ObjectAnonymousInnerClass : object
		{
			public override bool Equals(object o)
			{
				return o == this || o == null;
			}

			public override int GetHashCode()
			{
				return -1;
			}

			public override string ToString()
			{
				return "null";
			}
		}

		private readonly IDictionary<string, object> _nameValuePairs;

		public JSONObject()
		{
			_nameValuePairs = new Dictionary<string, object>();
		}

		public JSONObject(System.Collections.IDictionary copyFrom) : this()
		{
			var contentsTyped = copyFrom as IDictionary<object, object>;
			foreach (KeyValuePair<object, object> entry in contentsTyped.SetOfKeyValuePairs())
			{

				string key = (string) entry.Key;
				if (ReferenceEquals(key, null))
				{
					throw new NullReferenceException("key == null");
				}
				_nameValuePairs[key] = Wrap(entry.Value);
			}
		}

		public JSONObject(JSONTokener readFrom)
		{

			object @object = readFrom.NextValue();
			if (@object is JSONObject)
			{
				_nameValuePairs = ((JSONObject) @object)._nameValuePairs;
			}
			else
			{
				throw JSON.TypeMismatch(@object, "JSONObject");
			}
		}

		public JSONObject(string json) : this(new JSONTokener(json))
		{
		}

		public JSONObject(JSONObject copyFrom, string[] names) : this()
		{
			foreach (string name in names)
			{
				object value = copyFrom.Opt(name);
				if (value != null)
				{
					_nameValuePairs[name] = value;
				}
			}
		}

		public virtual int Length()
		{
			return _nameValuePairs.Count;
		}

		public virtual JSONObject Put(string name, bool value)
		{
			_nameValuePairs[CheckName(name)] = value;
			return this;
		}

		public virtual JSONObject Put(string name, double value)
		{
			_nameValuePairs[CheckName(name)] = JSON.CheckDouble(value);
			return this;
		}

		public virtual JSONObject Put(string name, int value)
		{
			_nameValuePairs[CheckName(name)] = value;
			return this;
		}

		public virtual JSONObject Put(string name, long value)
		{
			_nameValuePairs[CheckName(name)] = value;
			return this;
		}

		public virtual JSONObject Put(string name, object value)
		{
			if (value == null)
			{
				_nameValuePairs.Remove(name);
				return this;
			}
			if (NumberHelper.IsNumber(value))
			{
				JSON.CheckDouble(Convert.ToDouble(value));
			}
			_nameValuePairs[CheckName(name)] = value;
			return this;
		}

		public virtual JSONObject PutOpt(string name, object value)
		{
			if (ReferenceEquals(name, null) || value == null)
			{
				return this;
			}
			return Put(name, value);
		}

		public virtual JSONObject Accumulate(string name, object value)
		{
			object current = _nameValuePairs[CheckName(name)];
			if (current == null)
			{
				return Put(name, value);
			}
			
			if (NumberHelper.IsNumber(value))
			{
				JSON.CheckDouble(Convert.ToDouble(value));
			}

			if (current is JSONArray)
			{
				JSONArray array = (JSONArray) current;
				array.Put(value);
			}
			else
			{
				JSONArray array = new JSONArray();
				array.Put(current);
				array.Put(value);
				_nameValuePairs[name] = array;
			}
			return this;
		}

		internal virtual string CheckName(string name)
		{
			if (ReferenceEquals(name, null))
			{
				throw new JSONException("Names must be non-null");
			}
			return name;
		}
		
		public virtual object Remove(string name)
		{
			return _nameValuePairs.Remove(name);
		}
		
		public virtual bool IsNull(string name)
		{
			object value = _nameValuePairs[name];
			return value == null || value == Null;
		}
		
		public virtual bool Has(string name)
		{
			return _nameValuePairs.ContainsKey(name);
		}

		public virtual object Get(string name)
		{
			object result = _nameValuePairs[name];
			if (result == null)
			{
				throw new JSONException("No value for " + name);
			}
			return result;
		}
		
		public virtual object Opt(string name)
		{
			return _nameValuePairs[name];
		}

		public virtual bool GetBoolean(string name)
		{
			object @object = Get(name);
			bool? result = JSON.ToBoolean(@object);
			if (result == null)
			{
				throw JSON.TypeMismatch(name, @object, "boolean");
			}
			return result.Value;
		}

		public virtual bool OptBoolean(string name)
		{
			return OptBoolean(name, false);
		}

		public virtual bool OptBoolean(string name, bool fallback)
		{
			object @object = Opt(name);
			bool? result = JSON.ToBoolean(@object);
			return result != null ? result.Value : fallback;
		}

		public virtual double GetDouble(string name)
		{
			object @object = Get(name);
			double? result = JSON.ToDouble(@object);
			if (result == null)
			{
				throw JSON.TypeMismatch(name, @object, "double");
			}
			return result.Value;
		}
		
		public virtual double OptDouble(string name)
		{
			return OptDouble(name, Double.NaN);
		}
		
		public virtual double OptDouble(string name, double fallback)
		{
			object @object = Opt(name);
			double? result = JSON.ToDouble(@object);
			return result != null ? result.Value : fallback;
		}

		public virtual int GetInt(string name)
		{
			object @object = Get(name);
			int? result = JSON.ToInteger(@object);
			if (result == null)
			{
				throw JSON.TypeMismatch(name, @object, "int");
			}
			return result.Value;
		}
		
		public virtual int OptInt(string name)
		{
			return OptInt(name, 0);
		}

		public virtual int OptInt(string name, int fallback)
		{
			object @object = Opt(name);
			int? result = JSON.ToInteger(@object);
			return result != null ? result.Value : fallback;
		}

		public virtual long GetLong(string name)
		{
			object @object = Get(name);
			long? result = JSON.ToLong(@object);
			if (result == null)
			{
				throw JSON.TypeMismatch(name, @object, "long");
			}
			return result.Value;
		}
		
		public virtual long OptLong(string name)
		{
			return OptLong(name, 0L);
		}
		
		public virtual long OptLong(string name, long fallback)
		{
			object @object = Opt(name);
			long? result = JSON.ToLong(@object);
			return result != null ? result.Value : fallback;
		}

		public virtual string GetString(string name)
		{
			object @object = Get(name);
			string result = JSON.ToString(@object);
			if (ReferenceEquals(result, null))
			{
				throw JSON.TypeMismatch(name, @object, "String");
			}
			return result;
		}
		
		public virtual string OptString(string name)
		{
			return OptString(name, "");
		}
		
		public virtual string OptString(string name, string fallback)
		{
			object @object = Opt(name);
			string result = JSON.ToString(@object);
			return !ReferenceEquals(result, null) ? result : fallback;
		}

		public virtual JSONArray GetJsonArray(string name)
		{
			object @object = Get(name);
			if (@object is JSONArray)
			{
				return (JSONArray) @object;
			}

			throw JSON.TypeMismatch(name, @object, "JSONArray");
		}

		public virtual JSONArray OptJsonArray(string name)
		{
			object @object = Opt(name);
			return @object is JSONArray ? (JSONArray) @object : null;
		}

		public virtual JSONObject GetJsonObject(string name)
		{
			object @object = Get(name);
			if (@object is JSONObject)
			{
				return (JSONObject) @object;
			}

			throw JSON.TypeMismatch(name, @object, "JSONObject");
		}

		public virtual JSONObject OptJsonObject(string name)
		{
			object @object = Opt(name);
			return @object is JSONObject ? (JSONObject) @object : null;
		}

		public virtual JSONArray ToJsonArray(JSONArray names)
		{
			JSONArray result = new JSONArray();
			if (names == null)
			{
				return null;
			}
			
			int length = names.Length();
			if (length == 0)
			{
				return null;
			}
			
			for (int i = 0; i < length; i++)
			{
				string name = JSON.ToString(names.Opt(i));
				result.Put(Opt(name));
			}
			return result;
		}

		public virtual System.Collections.IEnumerator Keys()
		{
			return _nameValuePairs.Keys.GetEnumerator();
		}
		
		public virtual JSONArray Names()
		{
			return _nameValuePairs.Count == 0 ? null : new JSONArray(new List<String>(_nameValuePairs.Keys));
		}

		public override string ToString()
		{
			try
			{
				JSONStringer stringer = new JSONStringer();
				WriteTo(stringer);
				return stringer.ToString();
			}
			catch (JSONException)
			{
				return null;
			}
		}

		public virtual string ToString(int indentSpaces)
		{
			JSONStringer stringer = new JSONStringer(indentSpaces);
			WriteTo(stringer);
			return stringer.ToString();
		}

		internal virtual void WriteTo(JSONStringer stringer)
		{
			stringer.Object();
			foreach (KeyValuePair<string, object> entry in _nameValuePairs.SetOfKeyValuePairs())
			{
				stringer.Key(entry.Key).Value(entry.Value);
			}
			stringer.EndObject();
		}

		public static string NumberToString<T>(T number) where T : struct, IComparable, IConvertible
		{
			return HideNumberToString(number);
		}

		internal static string HideNumberToString(object number)
		{
			if (!NumberHelper.IsNumber(number))
			{
				throw new JSONException("Values must be non-number");
			}

			double doubleValue = Convert.ToDouble(number);
			JSON.CheckDouble(doubleValue);
			
			if (number.Equals(NegativeZero))
			{
				return "-0";
			}

			long longValue = Convert.ToInt64(number);
			if (doubleValue == longValue)
			{
				return Convert.ToString(longValue);
			}

			return number.ToString();
		}

		public static string Quote(string data)
		{
			if (ReferenceEquals(data, null))
			{
				return "\"\"";
			}
			try
			{
				JSONStringer stringer = new JSONStringer();
				stringer.Open(JSONStringer.Scope.Null, "");
				stringer.Value(data);
				stringer.Close(JSONStringer.Scope.Null, JSONStringer.Scope.Null, "");
				return stringer.ToString();
			}
			catch (JSONException)
			{
				throw new InvalidOperationException();
			}
		}

		public static object Wrap(object o)
		{
			if (o == null)
			{
				return Null;
			}
			if (o is JSONArray || o is JSONObject)
			{
				return o;
			}
			if (o.Equals(Null))
			{
				return o;
			}
			try
			{
				if (o is System.Collections.ICollection)
				{
					return new JSONArray((System.Collections.ICollection) o);
				}

				if (o.GetType().IsArray)
				{
					return new JSONArray(o);
				}
				
				if (o is bool || o is sbyte || o is char || o is double || o is float || o is int || o is long || o is short || o is string)
				{
					return o;
				}

				string name = o.GetType().Assembly.GetName().Name;
				if (name != null && name.StartsWith("System."))
				{
					return o.ToString();
				}
			}
			catch (Exception)
			{
				// ignored
			}

			return null;
		}
	}
}