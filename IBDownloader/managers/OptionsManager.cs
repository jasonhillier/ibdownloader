using IBApi;
using IBDownloader;
using IBDownloader.messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;

namespace IBDownloader.Managers
{
	class OptionChain
	{
		public OptionChain(Contract Underlying)
		{
			this.Underlying = Underlying;
			this.Expirations = new Dictionary<DateTime, Expiration>();
		}

		public Contract Underlying { get; private set; }
		public Dictionary<DateTime, Expiration> Expirations { get; private set; }

		public void AddOption(Contract Option)
		{
			//check if valid option contract
			if (Option.Strike <= 0 ||
				String.IsNullOrEmpty(Option.LastTradeDateOrContractMonth))
			{
				return; //ignore it
			}

			DateTime expirationDate;
			if (Option.LastTradeDateOrContractMonth.Length>6)
			{
				if (!DateTime.TryParseExact(Option.LastTradeDateOrContractMonth, "yyyyMMdd", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out expirationDate))
					return;
			}
			else
			{
				if (!DateTime.TryParseExact(Option.LastTradeDateOrContractMonth, "yyyyMM", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out expirationDate))
					return;
			}

			if (!Expirations.ContainsKey(expirationDate))
				this.Expirations.Add(expirationDate, new Expiration(expirationDate));

			if (Option.Right == "C")
				this.Expirations[expirationDate].Calls.Add(Option.Strike, Option);
			else
				this.Expirations[expirationDate].Puts.Add(Option.Strike, Option);
		}

		public class Expiration
		{
			public Expiration(DateTime ExpirationDate)
			{
				Date = ExpirationDate;
				Puts = new SortedList<double, Contract>();
				Calls = new SortedList<double, Contract>();
			}

			public DateTime Date { get; private set; }
			public SortedList<double, Contract> Puts { get; private set; }
			public SortedList<double, Contract> Calls { get; private set; }
		}
	}

	class OptionManager : BaseManager
	{
		public OptionManager(IBController Controller, IBClient ibClient)
			: base(Controller, ibClient)
		{
			_ibClient.SecurityDefinitionOptionParameter += this.AppendPendingRequestData;
			_ibClient.SecurityDefinitionOptionParameterEnd += this.HandleEndMessage;
		}

		/// <summary>
		/// Gets option profile for a security (e.g. Expiration dates, Strikes, etc)
		/// </summary>
		public async Task<OptionChain> GetOptionChain(Contract Contract)
		{
			var contractSearch = new Contract()
			{
				Symbol = Contract.Symbol,
				SecType = "OPT",
				Currency = Contract.Currency,
				Exchange = Contract.Exchange
			};

			Framework.Log("Loading option contract chain for {0}...", contractSearch.Symbol);

			var foundContracts = await _Controller.ContractManager.GetContractDetails(contractSearch);

			OptionChain chain = new OptionChain(Contract);
			foundContracts.All((optionContract) =>
			{
				chain.AddOption(optionContract.Summary);
				return true;
			});

			return chain;
		}
	}
}
