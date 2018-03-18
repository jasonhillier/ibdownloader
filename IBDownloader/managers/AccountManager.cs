﻿using IBApi;
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
		public AccountManager(IBController Controller, IBClient ibClient)
			: base(Controller, ibClient)
		{
			_ibClient.AccountSummary += this.AppendPendingRequestData;
			_ibClient.AccountSummaryEnd += this.HandleEndMessage;
		}

		public async Task<List<AccountSummaryMessage>> GetAccountSummary()
		{
			var accountSummary = await this.Dispatch<AccountSummaryMessage>((requestId) =>
			{
				Framework.Log("== Requesting Account Info ==");
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
