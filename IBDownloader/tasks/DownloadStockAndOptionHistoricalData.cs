using IBApi;
using IBDownloader.messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace IBDownloader.Tasks
{
	class DownloadStockAndOptionHistoricalData : DownloadOptionHistoricalData
	{
		//TODO: for now, just building this once per run
		private static Dictionary<DateTime, HistoricalDataMessage> _equityDataCache = new Dictionary<DateTime, HistoricalDataMessage>();

		public DownloadStockAndOptionHistoricalData(IBDTaskHandler TaskHandler)
			: base(TaskHandler)
		{

		}

		public async System.Threading.Tasks.Task RetrieveEquityData(IBDTaskInstruction optionInstruction)
		{
			if (_equityDataCache.Count == 0)
			{
				if (!await this.LookupDerivative(optionInstruction))
				{
					//right now, this is fatal
					throw new Exception("Failed to find derivative!");
				}

				var underlying = (Contract)optionInstruction.metadata["underlying"];

				var downloadTask = new DownloadHistoricalData(_TaskHandler);

				var ins = new IBDTaskInstruction() { contract = underlying, parameters = optionInstruction.parameters.Clone() }; //clone parameters so we get same settings (barSize etc)
				ins.parameters["StartDate"] = DateTime.Now.AddMonths(-6).ToString();
				var result = await downloadTask.ExecuteAsync(ins);


				if (!result.HasData) //right now this is fatal
					throw new Exception("No data found for underlying!");

				foreach (var quote in (List<HistoricalDataMessage>)result.Data)
				{
					var quoteDate = (DateTime)Framework.ParseDateTz(quote.Date);
					_equityDataCache[quoteDate] = quote;
				}
			}

			optionInstruction.metadata["UnderlyingData"] = _equityDataCache;
		}

		public override async System.Threading.Tasks.Task<TaskResultData> ExecuteAsync(IBDTaskInstruction instruction)
		{
			//First, make sure we have the underlying symbol's data
			await RetrieveEquityData(instruction);

			//then, follow instruction to download option
			return await base.ExecuteAsync(instruction);
		}
	}
}
