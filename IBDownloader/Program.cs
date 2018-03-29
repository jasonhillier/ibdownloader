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
				case "optionwithstock":
					storage = AddStockToOptions(taskHandler, args[1]);
					break;
				case "optiontasks":
					storage = BuildOptionDownloadTasks(taskHandler, args[1], args[2]);
					break;
				case "importcsv":
					storage = ImportCsv(taskHandler, args[1]);
					break;
				case "downloadoptions":
					string secType = (args.Length > 2) ? args[3] : "STK";
					storage = DownloadOptions(taskHandler, args[1], secType);
					break;
				case "cronfile":
					storage = CronFile(taskHandler, args[1]);
					break;
				default:
					throw new Exception("No commands specified! Try runtaskfile, optionwithstock, optiontasks, importcsv, downloadoptions, cronfile");
			}

			taskHandler.BeginAsync().Wait();
			storage.FlushAsync().Wait();

			Framework.Log("Processing complete.");
#if DEBUG
			Console.ReadLine();
#endif
		}

		static BaseDataStorage CronFile(IBDTaskHandler TaskHandler, string FilePathName)
		{
			ElasticsearchStorage es = new ElasticsearchStorage(new DataStorage.Processors.StockOptionQuoteProcessor());
			TaskHandler.OnTaskResult += es.ProcessTaskResult;
			TaskHandler.OnTaskResult += TaskHandler.AddTasks; //enable task chaining

			TaskHandler.Begin();

			var cronSchedule = new CronScheduler(TaskHandler);
			cronSchedule.RunFileAsync(FilePathName).Wait();

			return es;
		}

		static BaseDataStorage DownloadOptions(IBDTaskHandler TaskHandler, string Symbol, string SecurityType)
		{
			TaskHandler.AddTask(new IBDTaskInstruction("BuildOptionDownloadTasks") { Symbol = Symbol, SecType = SecurityType });
			TaskHandler.OnTaskResult += TaskHandler.AddTasks; //enable task chaining

			ElasticsearchStorage es = new ElasticsearchStorage(new DataStorage.Processors.StockOptionQuoteProcessor());
			TaskHandler.OnTaskResult += es.ProcessTaskResult;

			return es;
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
					//taskHandler.AddTask(new IBDTaskInstruction("DownloadOptionHistoricalData") { ConId = 308142771 });
					TaskHandler.AddTask(task);
				}
			}

			var storage = new ElasticsearchStorage(new DataStorage.Processors.StockOptionQuoteProcessor());
			TaskHandler.OnTaskResult += storage.ProcessTaskResult;

			return storage;
		}

		static BaseDataStorage ImportCsv(IBDTaskHandler TaskHandler, string FilePathName)
		{
			var instruction = new IBDTaskInstruction("ImportCsv");
			instruction.parameters["FilePathName"] = FilePathName;
			TaskHandler.AddTask(instruction);

			ElasticsearchStorage es = new ElasticsearchStorage(new DataStorage.Processors.StockOptionQuoteProcessor());
			TaskHandler.OnTaskResult += es.ProcessTaskResult;

			return es;
		}

		static BaseDataStorage BuildOptionDownloadTasks(IBDTaskHandler TaskHandler, string pSymbol, string FilePathName)
		{
			var storage = new DataStorage.JSONFile(FilePathName);
			//var storage = new DataStorage.ElasticsearchStorage(new DataStorage.Processors.OptionsQuoteProcessor());
			TaskHandler.OnTaskResult += storage.ProcessTaskResult;

			//taskHandler.AddTask(new IBDTaskInstruction("TestTask"));
			TaskHandler.AddTask(new IBDTaskInstruction("BuildOptionDownloadTasks") { Symbol = pSymbol, SecType = "STK" });

			return storage;
		}

		static BaseDataStorage AddStockToOptions(IBDTaskHandler TaskHandler, string pSymbol)
		{
			TaskHandler.AddTask(new IBDTaskInstruction("DownloadHistoricalData") { Symbol = pSymbol, SecType = "STK" });

			var storage = new ElasticsearchStorage(new DataStorage.Processors.StockOptionQuoteConverter());
			TaskHandler.OnTaskResult += storage.ProcessTaskResult;

			return storage;
		}
	}
}
