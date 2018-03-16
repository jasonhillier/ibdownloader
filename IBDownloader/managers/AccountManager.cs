using IBSampleApp;
using IBSampleApp.messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IBDownloader.managers
{
    class AccountManager
    {
		protected IBClient _ibClient;
		private const int ACCOUNT_ID_BASE = 50000000;
		private const int ACCOUNT_SUMMARY_ID = ACCOUNT_ID_BASE + 1;
		private const string ACCOUNT_SUMMARY_TAGS = "AccountType,NetLiquidation,TotalCashValue,SettledCash,AccruedCash,BuyingPower,EquityWithLoanValue,PreviousEquityWithLoanValue,"
			 + "GrossPositionValue,ReqTEquity,ReqTMargin,SMA,InitMarginReq,MaintMarginReq,AvailableFunds,ExcessLiquidity,Cushion,FullInitMarginReq,FullMaintMarginReq,FullAvailableFunds,"
			 + "FullExcessLiquidity,LookAheadNextChange,LookAheadInitMarginReq ,LookAheadMaintMarginReq,LookAheadAvailableFunds,LookAheadExcessLiquidity,HighestSeverity,DayTradesRemaining,Leverage";

		private TypeMergerPolicy _merge = new TypeMergerPolicy();
		private int _lastRequestIdCompleted;
		private object _lastRequestData;

		public AccountManager(IBClient ibClient)
		{
			_ibClient = ibClient;

			_ibClient.AccountSummary += _ibClient_AccountSummary;
			_ibClient.AccountSummaryEnd += _ibClient_AccountSummaryEnd;
		}

		private void _ibClient_AccountSummaryEnd(AccountSummaryEndMessage obj)
		{
			_lastRequestIdCompleted = obj.RequestId;
		}

		public async Task<Dictionary<string, AccountSummaryMessage>> GetAccountSummary()
		{
			var job = Dispatch<AccountSummaryMessage>(ACCOUNT_SUMMARY_ID);

			_ibClient.ClientSocket.reqAccountSummary(ACCOUNT_SUMMARY_ID, "All", ACCOUNT_SUMMARY_TAGS);

			return await job.GetAwaiter().GetResult();
		}

		private async Task<Dictionary<string, T>> Dispatch<T>(int pendingTaskId)
		{
			Dictionary<string, T> data = new Dictionary<string, T>();
			_lastRequestData = data;
			//_lastRequestIdCompleted = 0;
			//TODO: mutex/interlock
			while (_lastRequestIdCompleted != pendingTaskId)
			{
				await Task.Delay(1000);
			}


			return data;
		}

		private void _ibClient_AccountSummary(AccountSummaryMessage obj)
		{
			var data = (Dictionary<string, AccountSummaryMessage>)_lastRequestData;
			data[obj.Tag] = obj;
		}
    }
}
