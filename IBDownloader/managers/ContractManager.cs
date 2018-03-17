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
		public ContractManager(IBClient ibClient)
			: base(ibClient)
		{
			_ibClient.ContractDetails += this.AppendPendingRequestData;
			_ibClient.ContractDetailsEnd += this.HandleEndMessage;
		}

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

		public async Task<List<ContractDetails>> GetContractDetails(Contract Contract)
		{
			var messages = await this.Dispatch<ContractDetailsMessage>((requestId) =>
			{
				Framework.Log("Requesting Contract Details...");
				_ibClient.ClientSocket.reqContractDetails(requestId, Contract);
				return true;
			});

			return messages.ConvertAll<ContractDetails>((i) =>
			{
				return i.ContractDetails;
			});
		}
	}
}
