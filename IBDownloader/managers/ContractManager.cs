using IBDownloader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IBDownloader.messages;
using IBApi;
using System.Linq;

namespace IBDownloader.Managers
{
	enum SecurityType
	{
		STK,
		OPT,
		FUT,
		IND,
		FOP,
		CASH,
		BAG,
		WAR,
		BOND,
		CMDTY,
		NEWS,
		FUND
	}
	class ContractManager : BaseManager
	{
		public ContractManager(IBController Controller, IBClient ibClient)
			: base(Controller, ibClient)
		{
			_ibClient.ContractDetails += this.AppendPendingRequestData;
			_ibClient.ContractDetailsEnd += this.HandleEndMessage;
		}

		/// <summary>
		/// Get contract definitions by criteria
		/// </summary>
		public async Task<List<Contract>> GetContracts(SecurityType secType, string symbol, string currency = "USD", string exchange = "SMART")
		{
			var contractSearch = new Contract()
			{
				Symbol = symbol,
				SecType = secType.ToString(),
				Currency = currency,
				Exchange = exchange
			};

			var contractDetails = await GetContractDetails(contractSearch);
			return contractDetails.ConvertAll<Contract>((c) =>
			{
				return c.Summary;
			});
		}

		/// <summary>
		/// Get detailed info about a contract
		/// </summary>
		public async Task<List<ContractDetails>> GetContractDetails(Contract Contract)
		{
			var messages = await this.Dispatch<ContractDetailsMessage>((requestId) =>
			{
				this.Log("Requesting Contract Details...");
				_ibClient.ClientSocket.reqContractDetails(requestId, Contract);
				return true;
			});

			return messages.ConvertAll((i) =>
			{
				return i.ContractDetails;
			});
		}
	}
}
