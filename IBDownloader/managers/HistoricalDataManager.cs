using IBApi;
using IBDownloader;
using IBDownloader.messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

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
		public HistoricalDataManager(IBController Controller, IBClient ibClient)
			: base(Controller, ibClient)
		{
			_ibClient.HistoricalData += this.AppendPendingRequestData;
			_ibClient.HistoricalDataEnd += this.HandleEndMessage;

			_ibClient.HeadTimestamp += this.AppendPendingRequestData;
			_ibClient.HeadTimestamp += this.HandleEndMessage;
		}

		public async Task<DateTime> GetEarliestDataTime(Contract Contract, bool UseRTH = false)
		{
			var messages = await this.Dispatch<HeadTimestampMessage>((requestId) =>
			{
				_ibClient.ClientSocket.reqHeadTimestamp(
					requestId,
					Contract,
					"BID_ASK",
					UseRTH ? 1 : 0,
					1);
				return true;
			});

			if (messages.Count == 1)
			{
				return (DateTime)Framework.ParseDateTz(messages[0].HeadTimestamp, DateTime.Now);
			}
			else
			{
				return DateTime.Now;
			}
		}

		/// <summary>
		/// Get historical time-series data from IB
		/// </summary>
		public async Task<List<HistoricalDataMessage>> GetHistoricalData(
			Contract Contract, DateTime EndTime, BarSize BarSize,
			TimeSpan? Duration = null, HistoricalDataType DataType = HistoricalDataType.TRADES, bool UseRTH = false
			)
		{
			var dataBars = await this.Dispatch<HistoricalDataMessage>((requestId) =>
			{
				this.Log("Requesting Historical Data...");
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

			dataBars.All((bar) =>
			{
				//convert the datetime from IB format into normalized ISO time format
				bar.Date = Framework.ParseDateTz(bar.Date, DateTime.Now).Value.ToISOString();
				return true;
			});

			return dataBars.OrderBy(bar => bar.Date).ToList();
		}
	}
}
