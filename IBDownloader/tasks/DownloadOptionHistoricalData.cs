using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IBApi;
using IBDownloader.messages;

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
			Managers.BarSize barSize = instruction.GetParameter("BarSize").ParseElse(Managers.BarSize.M15);

			if (! await this.LookupDerivative(instruction))
				return TaskResultData.Failure(instruction, "Matching contract info for derivative not found!");

			DateTime earliestDate = await _Controller.HistoricalDataManager.GetEarliestDataTime(instruction.contract);
			earliestDate = earliestDate.StartOfDay();
			TimeSpan duration = DateTime.Now.EndOfDay().AddDays(1) - earliestDate;

			this.Log("Earliest date for {0} is {1}", instruction.ConId, earliestDate);

			List<HistoricalDataMessage> bars = new List<HistoricalDataMessage>();
			if (duration.TotalDays > 0)
			{
				int days = (int)duration.TotalDays;

				for(var i=1; i<=days; i++)
				{
					//we can safely download up to 7 days at a time
					int pageSize = 1;
					if (days - i > 3) pageSize = 3;
					if (days - i > 7) pageSize = 7;
					i += pageSize - 1;

					var data = await _Controller.HistoricalDataManager.GetHistoricalData(
						instruction.contract,
						earliestDate.AddDays(i), 
						barSize,
						pageSize,
						Managers.HistoricalDataType.BID_ASK);
					this.Log("Downloaded {0} bars.", data.Count);

					bars.AddRange(data);
				}
			}

			

			return new TaskResultData(instruction, bars.Count > 0, bars);
		}
	}
}
