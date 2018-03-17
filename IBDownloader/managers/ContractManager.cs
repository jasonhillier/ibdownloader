using IBDownloader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IBDownloader.messages;
using IBApi;

namespace IBDownloader.managers
{
	class ContractManager : BaseManager
	{
		public ContractManager(IBClient ibClient)
			: base(ibClient)
		{
			_ibClient.ContractDetails += _ibClient_ContractDetails;
			_ibClient.ContractDetailsEnd += _ibClient_ContractDetailsEnd;
		}

		private void _ibClient_ContractDetailsEnd(int RequestId)
		{
			this.MarkCompleted(RequestId);
		}

		private void _ibClient_ContractDetails(ContractDetailsMessage obj)
		{
			this.SetPendingRequestData<ContractDetails>(obj.RequestId, obj.ContractDetails);
		}

		public async Task<ContractDetails> GetContractDetails(Contract Contract)
		{
			int requestId = this.GetNextTaskId();
			var job = this.Dispatch<ContractDetails>(requestId).GetAwaiter();

			Framework.Log("Requesting Contract Details...");
			_ibClient.ClientSocket.reqContractDetails(requestId, Contract);

			while (!job.IsCompleted)
			{
				await Task.Delay(100);
			}

			return job.GetResult();
		}
	}
}
