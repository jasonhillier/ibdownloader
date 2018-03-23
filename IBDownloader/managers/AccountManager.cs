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
    public class AccountManager : BaseManager
	{
		internal AccountManager(IBController Controller, IBClient ibClient)
			: base(Controller, ibClient)
		{
			_ibClient.AccountSummary += this.AppendPendingRequestData;
			_ibClient.AccountSummaryEnd += this.HandleEndMessage;
		}

		public async Task<List<AccountSummaryMessage>> GetAccountSummary()
		{
			var accountSummary = await this.Dispatch<AccountSummaryMessage>((requestId) =>
			{
				this.Log("Requesting Account Summary...");
				_ibClient.ClientSocket.reqAccountSummary(requestId, "All", AccountSummaryTags.GetAllTags());
				return true;
			});

			//we don't want a subscription
			if (accountSummary.Count > 0)
				_ibClient.ClientSocket.cancelAccountSummary(accountSummary[0].RequestId);

			return accountSummary;
		}
    }
}
