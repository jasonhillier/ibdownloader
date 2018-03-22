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
			private Contract _Contract;
			private Contract _Underlying;
			private HistoricalDataMessage _Quote;

			public OptionQuote(Contract OptionContract, Contract Underlying, HistoricalDataMessage Quote)
			{
				_Contract = OptionContract;
				_Underlying = Underlying;
				_Quote = Quote;
			}

			public string id
			{
				get
				{
					return this.symbol + " " + this.date.ToString("yyyy-MM-ddTHH:mm:ssZ"); //fixed-length ISO string
				}
			}

			public string symbol { get { return _Contract.LocalSymbol; } }
			public string type { get { return _Contract.SecType; } } //TODO: need to have normalized type names
			public int contractId {  get { return _Contract.ConId; } }

			public DateTime date
			{
				get { return DateTime.Parse(_Quote.Date); }
			}

			public DateTime expiry
			{
				get { return (DateTime)Framework.ParseDateTz(_Contract.LastTradeDateOrContractMonth, DateTime.Now); }
			}

			public double strike
			{
				get { return _Contract.Strike; }
			}

			public string right
			{
				get { return _Contract.Right; }
			}

			public double bid
			{
				get { return _Quote.Open; }
			}

			public double maxAsk
			{
				get { return _Quote.High; }
			}

			public double lowBid
			{
				get { return _Quote.Low; }
			}

			public double ask
			{
				get { return _Quote.Close; }
			}

			public string baseSymbol
			{
				get { return _Underlying.Symbol; }
			}

			public string baseType
			{
				get { return _Underlying.SecType; }
			}
		}
	}
}
