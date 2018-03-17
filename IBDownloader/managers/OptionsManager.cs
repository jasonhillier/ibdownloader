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
	class OptionManager : BaseManager
	{
		public OptionManager(IBClient ibClient)
			: base(ibClient)
		{
			_ibClient.SecurityDefinitionOptionParameter += this.AppendPendingRequestData;
			_ibClient.SecurityDefinitionOptionParameterEnd += this.HandleEndMessage;
		}

		/// <summary>
		/// Gets option profile for a security (e.g. Expiration dates, Strikes, etc)
		/// </summary>
		public async Task<List<SecurityDefinitionOptionParameterMessage>> GetOptionProfiles(Contract Contract)
		{
			return await this.Dispatch<SecurityDefinitionOptionParameterMessage>((requestId) =>
			{
				Framework.Log("Requesting option contracts for {0}...", Contract.Symbol);
				_ibClient.ClientSocket.reqSecDefOptParams(requestId, Contract.Symbol, Contract.Exchange, Contract.SecType, Contract.ConId);
				return true;
			});
		}
	}
}
