using System;
using Xunit;
using IBDownloader.DataStorage;

namespace IBDownloader.Tests
{
    public class ElasticSearchTest
    {
        [Fact]
        public void Basic()
        {
			ElasticsearchStorage es = new ElasticsearchStorage(new DataStorage.Processors.OptionsQuoteProcessor());

		}
    }
}
