/*****************************************************************************
   Copyright 2021 The vladkryv@github.com. All Rights Reserved.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
       http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Org.Json
{
	public class JSONArray
	{
		private readonly IList<object> _values;
		
		public JSONArray()
		{
			_values = new List<object>();
		}

		public JSONArray(System.Collections.ICollection copyFrom) : this()
		{
			if (copyFrom != null)
			{
				for (System.Collections.IEnumerator it = copyFrom.GetEnumerator(); it.MoveNext();)
				{
					Put(JSONObject.Wrap(it.Current));
				}
			}
		}

		public JSONArray(JSONTokener readFrom)
		{
			object @object = readFrom.NextValue();
			if (@object is JSONArray)
			{
				_values = ((JSONArray) @object)._values;
			}
			else
			{
				throw JSON.TypeMismatch(@object, "JSONArray");
			}
		}

		public JSONArray(string json) : this(new JSONTokener(json))
		{
		}

		public JSONArray(object array)
		{
			if (!array.GetType().IsArray)
			{
				throw new JSONException("Not a primitive array: " + array.GetType());
			}
			
			int length = ((Array) array).Length;
			_values = new List<object>(length);
			for (int i = 0; i < length; ++i)
			{
				Put(JSONObject.Wrap(((Array) array).GetValue(i)));
			}
		}
		
		public virtual int Length()
		{
			return _values.Count;
		}
		
		public virtual JSONArray Put(bool value)
		{
			_values.Add(value);
			return this;
		}

		public virtual JSONArray Put(double value)
		{
			_values.Add(JSON.CheckDouble(value));
			return this;
		}

		public virtual JSONArray Put(int value)
		{
			_values.Add(value);
			return this;
		}

		public virtual JSONArray Put(long value)
		{
			_values.Add(value);
			return this;
		}

		public virtual JSONArray Put(object value)
		{
			_values.Add(value);
			return this;
		}

		public virtual JSONArray Put(int index, bool value)
		{
			return Put(index, ((bool?) value).Value);
		}

		public virtual JSONArray Put(int index, double value)
		{
			return Put(index, ((double?) value).Value);
		}

		public virtual JSONArray Put(int index, int value)
		{
			return Put(index, ((int?) value).Value);
		}

		public virtual JSONArray Put(int index, long value)
		{
			return Put(index, ((long?) value).Value);
		}

		public virtual JSONArray Put(int index, object value)
		{
			if (NumberHelper.IsNumber(value))
			{
				JSON.CheckDouble(Convert.ToDouble(value));
			}
			while (_values.Count <= index)
			{
				_values.Add(null);
			}
			_values[index] = value;
			return this;
		}

		public virtual bool IsNull(int index)
		{
			object value = Opt(index);
			return value == null || value == JSONObject.Null;
		}

		public virtual object Get(int index)
		{
			try
			{
				object value = _values[index];
				if (value == null)
				{
					throw new JSONException("Value at " + index + " is null.");
				}
				return value;
			}
			catch (IndexOutOfRangeException)
			{
				throw new JSONException("Index " + index + " out of range [0.." + _values.Count + ")");
			}
		}
		
		public virtual object Opt(int index)
		{
			if (index < 0 || index >= _values.Count)
			{
				return null;
			}
			return _values[index];
		}
		
		public virtual object Remove(int index)
		{
			if (index < 0 || index >= _values.Count)
			{
				return null;
			}
			object oldValue = _values[index];
			_values.RemoveAt(index);
			return oldValue;
		}

		public virtual bool GetBoolean(int index)
		{
			object @object = Get(index);
			bool? result = JSON.ToBoolean(@object);
			if (result == null)
			{
				throw JSON.TypeMismatch(index, @object, "boolean");
			}
			return result.Value;
		}

		public virtual bool OptBoolean(int index)
		{
			return OptBoolean(index, false);
		}

		public virtual bool OptBoolean(int index, bool fallback)
		{
			object @object = Opt(index);
			bool? result = JSON.ToBoolean(@object);
			return result != null ? result.Value : fallback;
		}
		
		public virtual double GetDouble(int index)
		{
			object @object = Get(index);
			double? result = JSON.ToDouble(@object);
			if (result == null)
			{
				throw JSON.TypeMismatch(index, @object, "double");
			}
			return result.Value;
		}

		public virtual double OptDouble(int index)
		{
			return OptDouble(index, Double.NaN);
		}
		
		public virtual double OptDouble(int index, double fallback)
		{
			object @object = Opt(index);
			double? result = JSON.ToDouble(@object);
			return result != null ? result.Value : fallback;
		}

		public virtual int GetInt(int index)
		{
			object @object = Get(index);
			int? result = JSON.ToInteger(@object);
			if (result == null)
			{
				throw JSON.TypeMismatch(index, @object, "int");
			}
			return result.Value;
		}

		public virtual int OptInt(int index)
		{
			return OptInt(index, 0);
		}

		public virtual int OptInt(int index, int fallback)
		{
			object @object = Opt(index);
			int? result = JSON.ToInteger(@object);
			return result != null ? result.Value : fallback;
		}

		public virtual long GetLong(int index)
		{
			object @object = Get(index);
			long? result = JSON.ToLong(@object);
			if (result == null)
			{
				throw JSON.TypeMismatch(index, @object, "long");
			}
			return result.Value;
		}

		public virtual long OptLong(int index)
		{
			return OptLong(index, 0L);
		}
		
		public virtual long OptLong(int index, long fallback)
		{
			object @object = Opt(index);
			long? result = JSON.ToLong(@object);
			return result != null ? result.Value : fallback;
		}

		public virtual string GetString(int index)
		{
			object @object = Get(index);
			string result = JSON.ToString(@object);
			if (ReferenceEquals(result, null))
			{
				throw JSON.TypeMismatch(index, @object, "String");
			}
			return result;
		}


		public virtual string OptString(int index)
		{
			return OptString(index, "");
		}
		
		public virtual string OptString(int index, string fallback)
		{
			object @object = Opt(index);
			string result = JSON.ToString(@object);
			return result ?? fallback;
		}

		public virtual JSONArray GetJsonArray(int index)
		{
			object @object = Get(index);
			if (@object is JSONArray)
			{
				return (JSONArray) @object;
			}
			else
			{
				throw JSON.TypeMismatch(index, @object, "JSONArray");
			}
		}

		public virtual JSONArray OptJsonArray(int index)
		{
			object @object = Opt(index);
			return @object is JSONArray ? (JSONArray) @object : null;
		}

		public virtual JSONObject GetJsonObject(int index)
		{
			object @object = Get(index);
			if (@object is JSONObject)
			{
				return (JSONObject) @object;
			}
			
			throw JSON.TypeMismatch(index, @object, "JSONObject");
		}
		
		public virtual JSONObject OptJsonObject(int index)
		{
			object @object = Opt(index);
			return @object is JSONObject ? (JSONObject) @object : null;
		}

		public virtual JSONObject ToJsonObject(JSONArray names)
		{
			JSONObject result = new JSONObject();
			int length = Math.Min(names.Length(), _values.Count);
			if (length == 0)
			{
				return null;
			}
			for (int i = 0; i < length; i++)
			{
				string name = JSON.ToString(names.Opt(i));
				result.Put(name, Opt(i));
			}
			return result;
		}

		public virtual string Join(string separator)
		{
			JSONStringer stringer = new JSONStringer();
			stringer.Open(JSONStringer.Scope.Null, "");
			for (int i = 0, size = _values.Count; i < size; i++)
			{
				if (i > 0)
				{
					stringer.Out.Append(separator);
				}
				stringer.Value(_values[i]);
			}
			
			stringer.Close(JSONStringer.Scope.Null, JSONStringer.Scope.Null, "");
			return stringer.Out.ToString();
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
			stringer.Array();
			foreach (object value in _values)
			{
				stringer.Value(value);
			}
			stringer.EndArray();
		}

		public override bool Equals(object o)
		{
			return o is JSONArray && ((JSONArray) o)._values.SequenceEqual(_values);
		}

		public override int GetHashCode()
		{
			return _values.GetHashCode();
		}
	}
}