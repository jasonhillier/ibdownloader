using IBDownloader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IBDownloader.messages;
using IBApi;

namespace IBDownloader.Managers
{
	class ContractManager : BaseManager
	{
		public ContractManager(IBClient ibClient)
			: base(ibClient)
		{
			_ibClient.ContractDetails += this.AppendPendingRequestData;
			_ibClient.ContractDetailsEnd += this.HandleEndMessage;
		}

		public async Task<List<Contract>> GetContracts(string secType, string symbol, string currency, string exchange)
		{
			Contract[] contracts = await _ibClient.ResolveContractAsync(secType, symbol, currency, exchange);
			return new List<Contract>(contracts);
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
