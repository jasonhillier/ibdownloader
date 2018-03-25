using IBDownloader.DataStorage;
using IBDownloader.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace IBDownloader.Tests
{
    public class MultiTaskTest
    {
        [Fact]
        public async void ImportCsvData()
        {
			var controller = new IBController();
            Assert.True(controller.Connect(), "Connection setup failed!");

            var taskHandler = new IBDTaskHandler(controller);

			var instruction = new IBDTaskInstruction("ImportCsv");
			instruction.parameters["FilePathName"] = "/Users/jason/Downloads/cboe";
            taskHandler.AddTask(instruction);

			ElasticsearchStorage es = new ElasticsearchStorage(new DataStorage.Processors.StockOptionQuoteProcessor());
			taskHandler.OnTaskResult += es.ProcessTaskResult;

			await taskHandler.BeginAsync();
			await es.FlushAsync();
        }
    }
}
