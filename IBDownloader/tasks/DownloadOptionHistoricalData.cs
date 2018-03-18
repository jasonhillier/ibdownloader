using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IBApi;

namespace IBDownloader.Tasks
{
	class DownloadOptionHistoricalData : BaseTask
	{
		public DownloadOptionHistoricalData(IBDTaskHandler TaskHandler)
			: base(TaskHandler)
		{

		}

		public override async System.Threading.Tasks.Task<TaskResultData> ExecuteAsync(IBDTaskInstruction instruction)
		{
			DateTime endTime = instruction.GetParameter("EndTime").ParseElse(DateTime.Now);
			TimeSpan duration = instruction.GetParameter("Duration").ParseElse(TimeSpan.FromDays(1));
			Managers.BarSize barSize = instruction.GetParameter("BarSize").ParseElse(Managers.BarSize.M15);

			DateTime earliestDate = await _Controller.HistoricalDataManager.GetEarliestDataTime(instruction.contract);

			this.Log("Earliest date is {0}", earliestDate);

			var data = await _Controller.HistoricalDataManager.GetHistoricalData(instruction.contract, endTime, barSize, duration, Managers.HistoricalDataType.BID_ASK);
			this.Log("Downloaded {0} bars.", data.Count);

			return new TaskResultData(instruction, data.Count > 0, data);
		}
	}
}
