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
	public enum HistoricalDataType
	{
		TRADES,
		MIDPOINT,
		BID_ASK
	}
	public enum BarSize
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
		Day,
		[DescriptionAttribute("1 week")]
		Week,
		[DescriptionAttribute("1 month")]
		Month
	};

	public class HistoricalDataManager : BaseManager
	{
		internal HistoricalDataManager(IBController Controller, IBClient ibClient)
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
		/// Get high, low price for a security
		/// </summary>
		public async Task<Tuple<double, double>> GetPriceRange(Contract contract, int DurationMonths=1)
		{
			var bars = await GetHistoricalData(contract, DateTime.Now.EndOfDay(), BarSize.Month, DurationMonths + " M");

			double high = 0;
			double low = 0;
			bars.All((bar) =>
			{
				if (bar.High > high)
					high = bar.High;
				if (bar.Low < low || low==0)
					low = bar.Low;
				return true;
			});

			return Tuple.Create<double, double>(high, low);
		}

		/// <summary>
		/// Get historical time-series data from IB
		/// </summary>
		public async Task<List<HistoricalDataMessage>> GetHistoricalData(
			Contract Contract, DateTime EndTime, BarSize BarSize,
			int DurationDays = 1, HistoricalDataType DataType = HistoricalDataType.TRADES, bool UseRTH = false
			)
		{
			return await GetHistoricalData(Contract, EndTime, BarSize, DurationDays + " D", DataType, UseRTH);
		}

		private async Task<List<HistoricalDataMessage>> GetHistoricalData(
			Contract Contract, DateTime EndTime, BarSize BarSize,
			string Duration = "1 D", HistoricalDataType DataType = HistoricalDataType.TRADES, bool UseRTH = false
			)
		{
			var dataBars = await this.Dispatch<HistoricalDataMessage>((requestId) =>
			{
				this.Log("Requesting Historical Data for {0} ending on {1}...", Contract.LocalSymbol, EndTime);
				_ibClient.ClientSocket.reqHistoricalData(
					requestId,
					Contract,
					EndTime.ToString("yyyyMMdd HH:mm:ss"),
					Duration,
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
