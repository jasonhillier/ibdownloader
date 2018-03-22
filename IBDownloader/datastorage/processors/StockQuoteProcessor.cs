using System;
using System.Collections.Generic;
using System.Text;
using IBDownloader.messages;
using IBApi;
using IBDownloader.Tasks;

namespace IBDownloader.DataStorage.Processors
{
    public class StockQuoteProcessor : IDataProcessor
    {
		public bool CheckIfSupported(TaskResultData taskResult)
		{
			return false;
		}

		public IEnumerable<IDataRow> Convert(TaskResultData taskResult)
		{
			throw new NotImplementedException();
		}
	}
}
