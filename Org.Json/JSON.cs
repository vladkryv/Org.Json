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

namespace Org.Json
{
	internal class JSON
	{
		internal static double CheckDouble(double d)
		{
			if (double.IsInfinity(d) || double.IsNaN(d))
			{
				throw new JSONException("Forbidden numeric value: " + d);
			}
			return d;
		}

		internal static bool? ToBoolean(object value)
		{
			if (value is Boolean)
			{
				return (bool?) value;
			}
			if (value is String)
			{
				string stringValue = (string) value;
				if ("true".Equals(stringValue, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if ("false".Equals(stringValue, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}
			return null;
		}

		internal static double? ToDouble(object value)
		{
			if (value is Double)
			{
				return (double?) value;
			}
			if (NumberHelper.IsNumber(value))
			{
				return Convert.ToDouble(value);
			}
			if (value is String)
			{
				try
				{
					return Convert.ToDouble((string) value);
				}
				catch (FormatException)
				{
					// ignored
				}
			}
			return null;
		}

		internal static int? ToInteger(object value)
		{
			if (value is Int32)
			{
				return (int?) value;
			}
			if (NumberHelper.IsNumber(value))
			{
				return Convert.ToInt32(value);
			}
			if (value is string)
			{
				try
				{
					return (int) double.Parse((string) value);
				}
				catch (FormatException)
				{
					// ignored
				}
			}
			return null;
		}

		internal static long? ToLong(object value)
		{
			if (value is Int64)
			{
				return (long?) value;
			}
			if (NumberHelper.IsNumber(value))
			{
				return Convert.ToInt64(value);
			}
			if (value is String)
			{
				try
				{
					return (long) double.Parse((string) value);
				}
				catch (FormatException)
				{
					// ignored
				}
			}
			return null;
		}

		internal static string ToString(object value)
		{
			if (value is String)
			{
				return (string) value;
			}
			if (value != null)
			{
				return value.ToString();
			}
			return null;
		}

		public static JSONException TypeMismatch(object indexOrName, object actual, string requiredType)
		{
			if (actual == null)
			{
				throw new JSONException("Value at " + indexOrName + " is null.");
			}
			throw new JSONException("Value " + actual + " at " + indexOrName + " of type " + actual.GetType().FullName + " cannot be converted to " + requiredType);
		}

		public static JSONException TypeMismatch(object actual, string requiredType)
		{
			if (actual == null)
			{
				throw new JSONException("Value is null.");
			}
			throw new JSONException("Value " + actual + " of type " + actual.GetType().FullName + " cannot be converted to " + requiredType);
		}
	}
}