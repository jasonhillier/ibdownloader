using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace IBDownloader
{
	interface IFrameworkLoggable {}

    static class Framework
    {
		static Framework()
		{
			Framework.Settings = new SettingsData();
		}

		public static SettingsData Settings { get; private set; }

		#region Date Handlers

		/// <summary>
		/// Parse a date-time string (with an ambigous tz) according to a predefined tz
		/// </summary>
		public static DateTime? ParseDateTz(string DateValue, DateTime? defaultValue = null)
		{
			DateTime date;
			bool success = false;

			if (string.IsNullOrEmpty(DateValue))
				return defaultValue;

			if (DateValue.Length == 18)
			{
				//20180316  14:30:00
				success = DateTime.TryParseExact(DateValue, "yyyyMMdd  HH:mm:ss", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out date);
			}
			else if (DateValue.Length == 8)
			{
				success = DateTime.TryParseExact(DateValue, "yyyyMMdd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out date);
			}
			else if (DateValue.Length == 6)
			{
				success = DateTime.TryParseExact(DateValue, "yyyyMM", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out date);
			}
			else
			{
				success = DateTime.TryParse(DateValue, out date);
			}

			return (success ? date : defaultValue);
		}

		#endregion

		#region Logging

		//extension
		public static void LogError(this IFrameworkLoggable caller, string format, params object[] args)
		{
			Log("[" + caller.GetType().Name + "] ERROR " + format, args);
		}
		public static void LogError(string format, params object[] args)
		{
			Log("ERROR " + format, args);
		}

		//extension
		public static void LogWarn(this IFrameworkLoggable caller, string format, params object[] args)
		{
			Log("[" + caller.GetType().Name + "] WARN " + format, args);
		}
		public static void LogWarn(string format, params object[] args)
		{
			Log("WARN " + format, args);
		}

		//extension
		public static void Log(this IFrameworkLoggable caller, string format, params object[] args)
		{
			Log("[" + caller.GetType().Name + "] " + format, args);
		}

		public static void Log(string format, params object[] args)
		{
			if (format.StartsWith("["))
				format = "<" + DateTime.UtcNow.ToString("o") + "> " + format;

			try
			{
				Console.WriteLine(format, args);
			}
			catch
			{
				Console.WriteLine(format);
			}
		}

		public static void DebugPrint(object @object)
		{
			if (@object == null)
				return;

			if (@object is IEnumerable)
			{
				foreach(var item in (IEnumerable)@object)
				{
					DebugPrint(item);
				}

				return;
			}

			var properties = @object.GetType().GetProperties();
			if (properties.Length == 0)
			{
				//for scalars
				Console.WriteLine(@object.ToString());
			}
			else
			{
				//for objects
				foreach (var property in properties)
				{
					var value = property.GetValue(@object, null);
					if (value != null)
						Console.WriteLine(property.Name + ": " + value.ToString());
				}
			}
		}

		#endregion
	}

	class SettingsData
	{
		//TODO: command-line parser

		public string this[string key]
		{
			get
			{
				return Environment.GetEnvironmentVariable(key.ToUpper());
			}
		}

		public string Get(string key, string defaultValue)
		{
			var value = this[key];
			if (String.IsNullOrEmpty(value))
				value = defaultValue;

			return value;
		}

		public int Get(string key, int defaultValue)
		{
			var value = this[key];

			int result;
			if (int.TryParse(value, out result))
				return result;
			else
				return defaultValue;
		}
	}
}
