using IBApi;
using IBDownloader;
using IBDownloader.messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IBDownloader.managers
{
    class AccountManager : BaseManager
	{
		public AccountManager(IBClient ibClient)
			: base(ibClient)
		{
			_ibClient.AccountSummary += _ibClient_AccountSummary;
			_ibClient.AccountSummaryEnd += _ibClient_AccountSummaryEnd;
		}

		private void _ibClient_AccountSummaryEnd(AccountSummaryEndMessage obj)
		{
			this.MarkCompleted(obj.RequestId);
		}

		public async Task<ConcurrentDictionary<string, AccountSummaryMessage>> GetAccountSummary()
		{
			int requestId = this.GetNextTaskId();
			var job = this.Dispatch<ConcurrentDictionary<string, AccountSummaryMessage>>(requestId).GetAwaiter();

			Framework.Log("== Requesting Account Info ==");
			_ibClient.ClientSocket.reqAccountSummary(requestId, "All", AccountSummaryTags.GetAllTags());

			while (!job.IsCompleted)
			{
				await Task.Delay(100);
			}

			return job.GetResult();
		}

		private void _ibClient_AccountSummary(AccountSummaryMessage message)
		{
			if (this.CheckRequestStillPending(message.RequestId))
			{
				var data = this.GetPendingRequestData<ConcurrentDictionary<string, AccountSummaryMessage>>(message.RequestId);
				data[message.Tag] = message;
			}
		}
    }
}
