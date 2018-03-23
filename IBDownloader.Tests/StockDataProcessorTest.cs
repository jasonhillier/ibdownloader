using IBDownloader.DataStorage;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace IBDownloader.Tests
{
    public class StockAndOptionDataProcessorTest
    {
		[Fact]
		public async void ProcessStockAndOptionQuote()
		{
			var controller = new IBController();
			Assert.True(controller.Connect(), "Connection setup failed!");

			var taskHandler = new IBDTaskHandler(controller);

			//try getting data for a VIX option contract
			taskHandler.AddTask(new IBDTaskInstruction("DownloadStockAndOptionHistoricalData") { ConId = 308395806 });

			var storage = new Stdout(new DataStorage.Processors.StockOptionQuoteProcessor());
			taskHandler.OnTaskResult += storage.ProcessTaskResult;

			await taskHandler.BeginAsync();
			await storage.FlushAsync();
		}
    }
}
