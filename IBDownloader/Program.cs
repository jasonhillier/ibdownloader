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

			var stdout = new DataStorage.JSONFile();
			taskHandler.OnTaskResult += stdout.ProcessTaskResult;

			//taskHandler.AddTask(new IBDTaskInstruction("TestTask"));
			Console.WriteLine("Waiting for tasks...");
			//taskHandler.AddTask(new IBDTaskInstruction("ListOptionContracts"){ Symbol = "SPY" });
			taskHandler.AddTask(new IBDTaskInstruction("DownloadOptionHistoricalData")
			{
				contract = {ConId= 308142771, Exchange = "SMART"}
			});

			Console.ReadLine();
        }
	}
}
