using System;
using System.Collections.Generic;
using System.Text;
using IBDownloader.messages;
using IBApi;
using IBDownloader.Tasks;
using System.Collections;
using IBDownloader.Managers;
using System.Linq;

namespace IBDownloader.DataStorage.Processors
{
	public class StockOptionQuoteConverter : OptionsQuoteProcessor, IDataPreProcessor
	{
		public override bool CheckIfSupported(TaskResultData taskResult)
		{
			if (!(taskResult.Data is IEnumerable<HistoricalDataMessage>))
			{
				return false;
			}

			if (taskResult.Instruction.SecType == SecurityType.STK.ToString() ||
					taskResult.Instruction.SecType == SecurityType.IND.ToString() ||
					taskResult.Instruction.SecType == SecurityType.FUT.ToString() ||
					taskResult.Instruction.SecType == SecurityType.FUT.ToString())
			{
				return true;
			}

			return false;
		}

		public void PreConvert(BaseDataStorage storage, TaskResultData taskResult)
		{
			//TODO: make generic interface
			if (storage is ElasticsearchStorage)
			{
				taskResult.Metadata["OptionQuotes"] = ((ElasticsearchStorage)storage).FetchQuotes(taskResult.Instruction.Symbol, DateTime.Now, DateTime.Now).Result;
			}
		}

        public override IEnumerable<IDataRow> Convert(TaskResultData taskResult)
		{
			if (!CheckIfSupported(taskResult))
				return null;

			//fallback
			if (taskResult.Instruction.contract.SecType == "OPT")
				return base.Convert(taskResult);

			List<HistoricalDataMessage> stockQuotes = (List<HistoricalDataMessage>)taskResult.Data;
			//TODO: we might want to reconcile this
			//index stock quotes by date
			Dictionary<DateTime, HistoricalDataMessage> stockQuotesIndexed = new Dictionary<DateTime, HistoricalDataMessage>();
			stockQuotes.All((q) =>
			{
				var quoteDate = DateTime.Parse(q.Date);
				stockQuotesIndexed[quoteDate] = q;
				return true;
			});

			//else we are updating a stock quote into prior downloaded options
			List<OptionQuote> optionQuotes = (List<OptionQuote>)taskResult.Metadata["OptionQuotes"];
			List<StockOptionQuote> stockOptionQuotes = optionQuotes.ConvertAll((q) =>
			{
				var iQuote = q as StockOptionQuote;
				if (stockQuotesIndexed.ContainsKey(iQuote.date))
				{
					iQuote.StockQuote = stockQuotesIndexed[iQuote.date];
				}
				else
				{
					this.LogWarn("Skipping option quote {0} because no underlying quote found for {1}", iQuote.symbol, iQuote.date);
				}

				return iQuote;
			});
            
            return stockOptionQuotes;
        }
        
        public class StockOptionQuote : OptionQuote
		{
            public StockOptionQuote(Contract OptionContract, Contract Underlying, HistoricalDataMessage OptionQuote, HistoricalDataMessage StockQuote)
                : base(OptionContract, Underlying, OptionQuote)
			{
				this.StockQuote = StockQuote;
            }

			public HistoricalDataMessage StockQuote {get;set;}
        }
    }
}