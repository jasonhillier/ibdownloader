using IBApi;
using IBDownloader;
using IBDownloader.messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IBDownloader.Managers
{
    class AccountManager : BaseManager
	{
		public AccountManager(IBClient ibClient)
			: base(ibClient)
		{
			_ibClient.AccountSummary += this.AppendPendingRequestData;
			_ibClient.AccountSummaryEnd += this.HandleEndMessage;
		}

		public async Task<List<AccountSummaryMessage>> GetAccountSummary()
		{
			int requestId = this.GetNextTaskId();
			var job = this.Dispatch<AccountSummaryMessage>(requestId).GetAwaiter();

			Framework.Log("== Requesting Account Info ==");
			_ibClient.ClientSocket.reqAccountSummary(requestId, "All", AccountSummaryTags.GetAllTags());

			while (!job.IsCompleted)
			{
				await Task.Delay(100);
			}

			return job.GetResult();
		}
    }
}
