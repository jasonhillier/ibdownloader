using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IBApi;

namespace IBDownloader.Tasks
{
	class BuildOptionDownloadTasks : ListOptionContracts
	{
		public BuildOptionDownloadTasks(IBDTaskHandler TaskHandler)
			: base(TaskHandler)
		{

		}

		public override async System.Threading.Tasks.Task<TaskResultData> ExecuteAsync(IBDTaskInstruction instruction)
		{
			var optionList = await base.ExecuteAsync(instruction);

			if (!optionList.HasData)
				return TaskResultData.Failure(instruction, "No option contracts!");

			List<IBDTaskInstruction> instructionSet = new List<IBDTaskInstruction>();
			foreach(var optionContractId in (IEnumerable<int>)optionList.Data)
			{
				instructionSet.Add(new IBDTaskInstruction("DownloadOptionHistoricalData")
				{
					ConId = optionContractId
				});
			}

			return new TaskResultData(instruction, true, instructionSet);
		}
	}
}
