using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using IBDownloader;
using IBDownloader.Managers;

namespace IBDownloader.Tests
{
    public class QuoteTest
    {
		[Fact]
		public async void Connect()
		{
			var controller = new IBController();
			Assert.True(controller.Connect(), "Connection setup failed!");

			var contracts = await controller.ContractManager.GetContracts(SecurityType.STK, "SPY");
			Assert.True(contracts.Count > 0, "No contracts found!");

			var result = await controller.HistoricalDataManager.GetPriceRange(contracts[0], 1);
			Assert.True(result.Item1 > 1 && result.Item2 < 1000, "SPY quote not quite right...");
		}
    }
}
