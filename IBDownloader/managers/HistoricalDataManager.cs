using IBApi;
using IBDownloader;
using IBDownloader.messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace IBDownloader.Managers
{
	enum HistoricalDataType
	{
		TRADES,
		MIDPOINT,
		BID_ASK
	}
	enum BarSize
	{
		[DescriptionAttribute("15 mins")]
		M15,
		[DescriptionAttribute("30 mins")]
		M30,
		[DescriptionAttribute("1 hour")]
		H1,
		[DescriptionAttribute("4 hours")]
		H4,
		[DescriptionAttribute("1 day")]
		D1
	};

	class HistoricalDataManager : BaseManager
	{
		public HistoricalDataManager(IBClient ibClient)
			: base(ibClient)
		{
			_ibClient.HistoricalData += this.AppendPendingRequestData;
			_ibClient.HistoricalDataEnd += this.HandleEndMessage;
		}

		/// <summary>
		/// Get historical time-series data from IB
		/// </summary>
		public async Task<List<HistoricalDataMessage>> GetHistoricalData(
			Contract Contract, DateTime EndTime, BarSize BarSize,
			TimeSpan? Duration = null, HistoricalDataType DataType = HistoricalDataType.TRADES, bool UseRTH = false
			)
		{
			return await this.Dispatch<HistoricalDataMessage>((requestId) =>
			{
				Framework.Log("Requesting Historical Data...");
				_ibClient.ClientSocket.reqHistoricalData(
					requestId,
					Contract,
					EndTime.ToString("yyyyMMdd HH:mm:ss"),
					"1 D", //Duration
					BarSize.ToDescription(),
					DataType.ToString(),
					UseRTH ? 1 : 0,
					1,
					false,
					new List<TagValue>()
					);
				return true;
			});
		}
	}
}
