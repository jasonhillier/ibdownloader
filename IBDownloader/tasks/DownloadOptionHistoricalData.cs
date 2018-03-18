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
			Managers.BarSize barSize = instruction.GetParameter("BarSize").ParseElse(Managers.BarSize.M15);

			var data = await _Controller.HistoricalDataManager.GetHistoricalData(instruction.contract, endTime, barSize);
			Framework.Log("Downloaded {0} bars.", data.Count);

			return new TaskResultData(instruction, data.Count > 0, data);
		}
	}
}
