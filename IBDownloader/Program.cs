using System;
using System.Threading;

namespace IBDownloader
{
    class Program
    {
		

		static void Main(string[] args)
        {
			var controller = new IBController();
			controller.Connect();

			var taskHandler = new IBDTaskHandler(controller);

			var storage = new DataStorage.ElasticsearchStorage(new DataStorage.Processors.OptionsQuoteProcessor());
			taskHandler.OnTaskResult += storage.ProcessTaskResult;

			//taskHandler.AddTask(new IBDTaskInstruction("TestTask"));
			//taskHandler.AddTask(new IBDTaskInstruction("ListOptionContracts"){ Symbol = "SPY" });
			taskHandler.AddTask(new IBDTaskInstruction("DownloadOptionHistoricalData")
			{
				contract = {ConId= 308142771, Exchange = "SMART"}
			});

			taskHandler.BeginAsync().Wait();
			storage.FlushAsync().Wait();

			Console.WriteLine("Processing complete.");
#if DEBUG
			Console.ReadLine();
#endif
		}
	}
}
