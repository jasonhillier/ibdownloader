using System;
using System.Collections.Generic;
using System.Text;
using IBDownloader.messages;
using IBApi;
using IBDownloader.Tasks;

namespace IBDownloader.DataStorage.Processors
{
    class StockQuoteProcessor : IDataProcessor
    {
		public bool CheckIfSupported(TaskResultData taskResult)
		{
			return false;
		}

		public IDataRow Convert(TaskResultData taskResult)
		{
			throw new NotImplementedException();
		}
	}
}
