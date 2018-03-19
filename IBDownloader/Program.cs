using IBDownloader.DataStorage;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace IBDownloader
{
    class Program
    {
		static void Main(string[] args)
        {
			var controller = new IBController();
			controller.Connect();

			var taskHandler = new IBDTaskHandler(controller);

			BaseDataStorage storage;

			string commandArg = (args.Length > 0) ? args[0] : "default";
			switch(commandArg)
			{
				case "runtaskfile":
					storage = RunTaskFile(taskHandler, args[1]);
					break;
				case "optiontasks":
					storage = BuildOptionDownloadTasks(taskHandler, args[1]);
					break;
				default:
					throw new Exception("No commands specified! Try runtaskfile or optiontasks");
					break;
			}

			taskHandler.BeginAsync().Wait();
			storage.FlushAsync().Wait();

			Framework.Log("Processing complete.");
#if DEBUG
			Console.ReadLine();
#endif
		}

		static BaseDataStorage RunTaskFile(IBDTaskHandler TaskHandler, string FilePathName)
		{
			using (StreamReader file = new StreamReader(File.Open(FilePathName, FileMode.Open)))
			{
				string taskListJson = file.ReadToEnd();

				var tasks = JsonConvert.DeserializeObject<List<IBDTaskInstruction>>(taskListJson);
				Framework.Log("Preparing {0} tasks...", tasks.Count);

				foreach (var task in tasks)
				{
					TaskHandler.AddTask(task);
				}
			}

			return new ElasticsearchStorage(new DataStorage.Processors.OptionsQuoteProcessor());
		}

		static BaseDataStorage BuildOptionDownloadTasks(IBDTaskHandler TaskHandler, string FilePathName)
		{
			var storage = new DataStorage.JSONFile(FilePathName);
			//var storage = new DataStorage.ElasticsearchStorage(new DataStorage.Processors.OptionsQuoteProcessor());
			TaskHandler.OnTaskResult += storage.ProcessTaskResult;

			//taskHandler.AddTask(new IBDTaskInstruction("TestTask"));
			TaskHandler.AddTask(new IBDTaskInstruction("BuildOptionDownloadTasks") { Symbol = "SPY" });
			//taskHandler.AddTask(new IBDTaskInstruction("DownloadOptionHistoricalData") { ConId = 308142771 });

			return storage;
		}
	}
}
