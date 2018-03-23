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
	public class OptionChain
	{
		public OptionChain(Contract Underlying)
		{
			this.Underlying = Underlying;
			this.Expirations = new SortedDictionary<DateTime, Expiration>();
		}

		public Contract Underlying { get; private set; }
		public SortedDictionary<DateTime, Expiration> Expirations { get; private set; }

		public void AddOption(Contract Option)
		{
			DateTime? expirationDateParsed = Framework.ParseDateTz(Option.LastTradeDateOrContractMonth);
			//check if valid option contract
			if (expirationDateParsed == null || Option.Strike <= 0)
			{
				return; //ignore it
			}

			DateTime expirationDate = (DateTime)expirationDateParsed;

			if (!Expirations.ContainsKey(expirationDate))
				this.Expirations.Add(expirationDate, new Expiration(expirationDate));

			if (Option.Right == "C")
				this.Expirations[expirationDate].Calls.Add(Option.Strike, Option);
			else
				this.Expirations[expirationDate].Puts.Add(Option.Strike, Option);
		}

		public class Expiration
		{
			public enum Type { weekly, monthly, any };
			public Expiration(DateTime ExpirationDate)
			{
				Date = ExpirationDate;
				Puts = new SortedList<double, Contract>();
				Calls = new SortedList<double, Contract>();

				//TODO: make this smarter
				if (ExpirationDate.DayOfWeek == DayOfWeek.Friday)
					ExpType = Type.monthly;
				else if (ExpirationDate.DayOfWeek == DayOfWeek.Monday)
					ExpType = Type.weekly;
				else
					ExpType = Type.any;
			}

			public Type ExpType { get; set; }
			public DateTime Date { get; private set; }
			public SortedList<double, Contract> Puts { get; private set; }
			public SortedList<double, Contract> Calls { get; private set; }
		}
	}

	public class OptionManager : BaseManager
	{
		internal OptionManager(IBController Controller, IBClient ibClient)
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
				Currency = Contract.Currency != null ? Contract.Currency : "USD",
				Exchange = Contract.Exchange != null ? Contract.Exchange : "SMART"
			};

			this.Log("Loading option contract chain for {0}...", contractSearch.Symbol);

			var foundContracts = await _Controller.ContractManager.GetContractDetails(contractSearch);

			this.Log("Found {0} option contracts", foundContracts.Count);

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
