using System;
using System.Collections.Generic;
using System.Text;
using IBDownloader.messages;
using IBApi;
using IBDownloader.Tasks;
using System.Collections;

namespace IBDownloader.DataStorage.Processors
{
	class StockOptionQuoteProcessor : OptionsQuoteProcessor, IDataPreProcessor
	{
		public override bool CheckIfSupported(TaskResultData taskResult)
		{
			if (taskResult.Data is IEnumerable<HistoricalDataMessage>)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public void PreConvert(BaseDataStorage storage, TaskResultData taskResult)
		{
			//TODO: make generic interface
			if (storage is ElasticsearchStorage)
			{
				taskResult.Metadata["OptionQuotes"] = ((ElasticsearchStorage)storage).FetchQuotes(DateTime.Now, DateTime.Now).Result;
			}
		}

        public override IEnumerable<IDataRow> Convert(TaskResultData taskResult)
		{
			if (!CheckIfSupported(taskResult))
				return null;

			//fallback
			if (taskResult.Instruction.contract.SecType == "OPT")
				return base.Convert(taskResult);
			
			
			//else we are updating a stock quote into prior downloaded options
			List<OptionsQuoteProcessor.OptionQuote> optionQuotes = (List<OptionsQuoteProcessor.OptionQuote>)taskResult.Metadata["OptionQuotes"];
			var stockOptionQuotes = optionQuotes.ConvertAll<StockOptionQuote>((q)=>
			{
				var iQuote =  q as StockOptionQuote;
				iQuote.StockQuote = ;
				return iQuote;
			});
            
            return stockOptionQuotes;
        }
        
        public class StockOptionQuote : OptionsQuoteProcessor.OptionQuote
		{
            public StockOptionQuote(Contract OptionContract, Contract Underlying, HistoricalDataMessage OptionQuote, HistogramDataMessage StockQuote)
                : base(OptionContract, Underlying, OptionQuote)
			{
				this.StockQuote = StockQuote;
            }

			public HistogramDataMessage StockQuote {get;set;}
        }
    }
}