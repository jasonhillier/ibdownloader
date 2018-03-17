using System;
using System.Collections.Generic;
using System.Text;

namespace IBDownloader
{
    static class Framework
    {
		static Framework()
		{
			Framework.Settings = new SettingsData();
		}

		public static SettingsData Settings { get; private set; }

		#region Logging

		public static void LogError(string format, params object[] args)
		{
			Log("ERROR " + format, args);
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
				return Environment.GetEnvironmentVariable(key);
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
