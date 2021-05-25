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