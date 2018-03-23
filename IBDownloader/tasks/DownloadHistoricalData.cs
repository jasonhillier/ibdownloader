﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IBApi;
using IBDownloader.messages;

namespace IBDownloader.Tasks
{
	class DownloadHistoricalData : BaseTask
	{
		public DownloadHistoricalData(IBDTaskHandler TaskHandler)
			: base(TaskHandler)
		{

		}

		public override async System.Threading.Tasks.Task<TaskResultData> ExecuteAsync(IBDTaskInstruction instruction)
		{
			Managers.BarSize barSize = instruction.GetParameter("BarSize").ParseElse(Managers.BarSize.M15);
			DateTime startDate = instruction.GetParameter("StartDate").ParseElse(DateTime.MinValue);

			var selectedContracts = await _Controller.ContractManager.GetContractDetails(instruction.contract);
			if (selectedContracts.Count < 1)
				return TaskResultData.Failure(instruction, "No matching contracts found!");

			DateTime earliestDate = startDate;
			if (startDate == DateTime.MinValue)
			{
				earliestDate = await _Controller.HistoricalDataManager.GetEarliestDataTime(instruction.contract);
			}
			earliestDate = earliestDate.StartOfDay();
			TimeSpan duration = DateTime.Now.EndOfDay().AddDays(1) - earliestDate;

			this.Log("Fetching data for {0} starting from {1}", instruction.ConId, earliestDate);

			List<HistoricalDataMessage> bars = new List<HistoricalDataMessage>();
			if (duration.TotalDays > 0)
			{
				int days = (int)duration.TotalDays;

				for(var i=1; i<=days; i++)
				{
					//we can safely download up to 30 days at a time
					int pageSize = (days - i) + 1;
					if (pageSize > 30) pageSize = 30;
					i += pageSize - 1;

					var data = await _Controller.HistoricalDataManager.GetHistoricalData(
						instruction.contract,
						earliestDate.AddDays(i),
						barSize,
						pageSize,
						Managers.HistoricalDataType.TRADES,
						true);
					this.Log("Downloaded {0} bars.", data.Count);

					bars.AddRange(data);
				}
			}
			

			return new TaskResultData(instruction, bars.Count > 0, bars);
		}
	}
}
