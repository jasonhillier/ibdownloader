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
	public class StockOptionQuoteProcessor : OptionsQuoteProcessor
	{
		public override bool CheckIfSupported(TaskResultData taskResult)
		{
			return (
				base.CheckIfSupported(taskResult) &&
				taskResult.Instruction.metadata.ContainsKey(IBDMetadataType.underlyingData)
			);
		}

		public override IEnumerable<IDataRow> Convert(TaskResultData taskResult)
		{
			if (!CheckIfSupported(taskResult))
				return null;

			 var underlyingData = (Dictionary<DateTime, HistoricalDataMessage>)taskResult.Instruction.metadata[IBDMetadataType.underlyingData];

			List<IDataRow> rows = new List<IDataRow>();
			foreach (var quote in (IEnumerable<HistoricalDataMessage>)taskResult.Data)
			{
				//can only process option quotes
				if (taskResult.Instruction.contract.SecType == "OPT")
				{
					object metaUnderlying;
					if (!taskResult.Instruction.metadata.TryGetValue(IBDMetadataType.underlying, out metaUnderlying))
					{
						this.Log("Missing underlying contract definition! Make sure the task sets required instruction metadata.");
						continue;
					}
					Contract underlying = (Contract)metaUnderlying;
					var quoteDate = (DateTime)Framework.ParseDateTz(quote.Date);

					if (underlyingData.ContainsKey(quoteDate))
					{
						rows.Add(new StockOptionQuote(
							taskResult.Instruction.contract,
							underlying,
							quote,
							underlyingData[quoteDate]
						));
					}
					else
					{
						this.Log("Missing underlying quote at {0}", quoteDate);
					}
				}
			}

			return rows;
		}

		public class StockOptionQuote : OptionQuote
		{
			public StockOptionQuote() : base() { }
			public StockOptionQuote(Contract OptionContract, Contract Underlying, HistoricalDataMessage OptionQuote, HistoricalDataMessage StockQuote)
				: base(OptionContract, Underlying, OptionQuote)
			{
				this.basePrice = StockQuote.Close;
			}

			public double basePrice { get; set; }
			public double dist
			{
				get { return Math.Abs(strike - basePrice); }
			}

			//In-the-money
			public bool itm
			{
				get
				{
					if (right == "C" &&
						this.strike < this.basePrice)
					{
						return true;
					}
					else if (right == "P" &&
						this.strike > this.basePrice)
					{
						return true;
					}

					return false;
				}
			}

			//extrinsic value
			public double extr
			{
				get
				{
					//otm
					if (!this.itm)
						return this.mid > 0 ? this.mid : 0;

					//itm
					double result = this.mid - Math.Abs(this.strike - this.basePrice);

					return result > 0 ? result : 0;
				}
			}
		}
	}
}
