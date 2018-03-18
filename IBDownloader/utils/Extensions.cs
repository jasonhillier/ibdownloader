using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace IBDownloader
{
	static class Extensions
	{
		public static string ToISOString(this DateTime value)
		{
			return value.ToUniversalTime().ToString("o");
		}

		public static T ParseElse<T>(this string value, T defaultValue)
		{
			//TODO: deal with potential upcast


			if (defaultValue is DateTime)
			{
				DateTime result;
				if (DateTime.TryParse(value, out result))
					return (T)(object)result;
			}
			else if (defaultValue is TimeSpan)
			{
				TimeSpan result;
				if (TimeSpan.TryParse(value, out result))
					return (T)(object)result;
			}
			else if (defaultValue is Enum)
			{
				object result;
				if (Enum.TryParse(typeof(T), value, out result))
					return (T)result;
			}
			else if (defaultValue is int)
			{
				int result;
				if (int.TryParse(value, out result))
					return (T)(object)result;
			}
			else if (defaultValue is double)
			{
				double result;
				if (double.TryParse(value, out result))
					return (T)(object)result;
			}
			else if (defaultValue is float)
			{
				float result;
				if (float.TryParse(value, out result))
					return (T)(object)result;
			}
			else if (defaultValue is decimal)
			{
				decimal result;
				if (decimal.TryParse(value, out result))
					return (T)(object)result;
			}

			return defaultValue;
		}

		public static string ToDescription(this Enum en) //ext method
		{
			Type type = en.GetType();
			MemberInfo[] memInfo = type.GetMember(en.ToString());

			if (memInfo != null && memInfo.Length > 0)
			{
				object[] attrs = memInfo[0].GetCustomAttributes(
											  typeof(DescriptionAttribute),
											  false);

				if (attrs != null && attrs.Length > 0)
					return ((DescriptionAttribute)attrs[0]).Description;

			}

			return en.ToString();

		}
	}
}
