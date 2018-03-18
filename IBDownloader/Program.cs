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
			taskHandler.Begin();

			var elasticsearch = new DataStorage.ElasticsearchStorage(new DataStorage.Processors.StockQuoteProcessor());
			taskHandler.OnTaskResult += elasticsearch.ProcessTaskResultAsync;

			//taskHandler.AddTask(new IBDTaskInstruction("TestTask"));
			Console.WriteLine("Waiting for tasks...");
			taskHandler.AddTask(new IBDTaskInstruction("ListOptionContracts"){ Symbol = "SPY" });

			Console.ReadLine();
        }
	}
}
