using System;
using System.Collections.Generic;
using System.Text;
using IBDownloader.messages;
using IBApi;
using IBDownloader.Tasks;
using System.Collections;

namespace IBDownloader.DataStorage.Processors
{
	public class OptionsQuoteProcessor : IDataProcessor, IFrameworkLoggable
	{
		public virtual bool CheckIfSupported(TaskResultData taskResult)
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

		public virtual IEnumerable<IDataRow> Convert(TaskResultData taskResult)
		{
			if (!CheckIfSupported(taskResult))
				return null;

			List<IDataRow> rows = new List<IDataRow>();
			foreach(var quote in (IEnumerable<HistoricalDataMessage>)taskResult.Data)
			{
				//can only process option quotes
				if (taskResult.Instruction.contract.SecType == "OPT")
				{
					object metaUnderlying;
					if (!taskResult.Instruction.metadata.TryGetValue("underlying", out metaUnderlying))
					{
						this.Log("Missing underlying contract definition! Make sure the task sets required instruction metadata.");
						continue;
					}
					Contract underlying = (Contract)metaUnderlying;

					rows.Add(new OptionQuote(taskResult.Instruction.contract, underlying, quote));
				}
			}

			return rows;
		}

		//BID_ASK quote
		public class OptionQuote : IDataRow
		{
			public OptionQuote() { }
			public OptionQuote(Contract OptionContract, Contract Underlying, HistoricalDataMessage Quote)
			{
				this.symbol = OptionContract.LocalSymbol;
				this.id = this.symbol + " " + this.date.ToString("yyyy-MM-ddTHH:mm:ssZ"); //fixed-length ISO string
				this.type = OptionContract.SecType;
				this.contractId = OptionContract.ConId;
				this.date = DateTime.Parse(Quote.Date);
				this.expiry = (DateTime)Framework.ParseDateTz(OptionContract.LastTradeDateOrContractMonth, DateTime.Now);
				this.strike = OptionContract.Strike;
				this.right = OptionContract.Right;
				this.bid = Quote.Open;
				this.maxAsk = Quote.High;
				this.lowBid = Quote.Low;
				this.ask = Quote.Close;
				this.baseSymbol = Underlying.Symbol;
				this.baseType = Underlying.SecIdType;
			}

			public string id { get; set; }
			public string symbol { get; set; }
			public string type { get; set; } //TODO: need to have normalized type names
			public int contractId { get; set; }

			public DateTime date { get; set; }
			public DateTime expiry { get; set; }

			public double strike { get; set; }
			public string right { get; set; }

			public double bid { get; set; }

			public double maxAsk { get; set; }

			public double lowBid { get; set; }
			public double ask { get; set; }

			public double spread
			{
				get { return Math.Round(ask - bid, 2); }
			}
			public double mid
			{
				get { return Math.Round(bid + (spread / 2), 2); }
			}

			public string baseSymbol { get; set; }
			public string baseType { get; set; }
		}
	}
}
