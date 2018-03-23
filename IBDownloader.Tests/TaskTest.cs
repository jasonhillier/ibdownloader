using IBDownloader.DataStorage;
using IBDownloader.Managers;
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

			taskHandler.AddTask(new IBDTaskInstruction("DownloadHistoricalData")
			{
				contract = new IBApi.Contract() { Symbol = "VIX", SecType = "IND", Exchange = "CBOE" }
			});

			var storage = new JSONFile();
			taskHandler.OnTaskResult += storage.ProcessTaskResult;

			await taskHandler.BeginAsync();
			await storage.FlushAsync();
		}

		[Fact]
		public async void GenerateOptionHistoricalDataTasks()
		{
			var controller = new IBController();
			Assert.True(controller.Connect(), "Connection setup failed!");

			var taskHandler = new IBDTaskHandler(controller);

			var instruction = new IBDTaskInstruction("BuildOptionDownloadTasks") { Symbol = "VIX", SecType = "OPT" };
			instruction.parameters["filter.expirytype"] = OptionChain.Expiration.Type.any.ToString();
			taskHandler.AddTask(instruction);

			var storage = new JSONFile();
			taskHandler.OnTaskResult += storage.ProcessTaskResult;

			await taskHandler.BeginAsync();
			await storage.FlushAsync();
		}
	}
}
