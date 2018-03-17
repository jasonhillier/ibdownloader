using System;
using System.Collections.Generic;
using System.Text;

namespace IBDownloader
{
    static class Framework
    {
		static Framework()
		{

		}

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
	}
}
