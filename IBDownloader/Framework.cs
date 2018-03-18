using System;
using System.Collections.Generic;
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
			foreach (var property in @object.GetType().GetProperties())
			{
				var value = property.GetValue(@object, null);
				if (value != null)
					Console.WriteLine(property.Name + ": " + value.ToString());
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
