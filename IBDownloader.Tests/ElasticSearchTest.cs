using System;
using Xunit;
using IBDownloader.DataStorage;
using System.Linq;

namespace IBDownloader.Tests
{
    public class ElasticSearchTest
    {
        [Fact]
        public void Basic()
        {
			ElasticsearchStorage es = new ElasticsearchStorage(new DataStorage.Processors.OptionsQuoteProcessor());
			var data = es.FetchQuotes<DataStorage.Processors.StockOptionQuoteConverter.StockOptionQuote>("SPY", DateTime.Now.AddDays(-7), DateTime.Now).Result;
			Assert.True(data.Count > 1, "No data found!");
		}
    }
}
