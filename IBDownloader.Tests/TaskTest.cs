using IBDownloader.DataStorage;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace IBDownloader.Tests
{
    public class TaskTest
    {
		[Fact]
		public async void DownloadHistoricalData()
		{
			var controller = new IBController();
			Assert.True(controller.Connect(), "Connection setup failed!");

			var taskHandler = new IBDTaskHandler(controller);

			taskHandler.AddTask(new IBDTaskInstruction("DownloadHistoricalData") { Symbol = "VXX", SecType = "STK" });

			var storage = new JSONFile();
			taskHandler.OnTaskResult += storage.ProcessTaskResult;

			await taskHandler.BeginAsync();
			await storage.FlushAsync();
		}
    }
}
