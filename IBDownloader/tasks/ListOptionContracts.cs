using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IBApi;

namespace IBDownloader.Tasks
{
	class ListOptionContracts : BaseTask
	{
		public ListOptionContracts(IBDTaskHandler TaskHandler)
			: base(TaskHandler)
		{

		}

		public override async System.Threading.Tasks.Task<TaskResultData> ExecuteAsync(IBDTaskInstruction instruction)
		{
			var optionChain = await _Controller.OptionManager.GetOptionChain(instruction.contract);

			List<int> flatContractIdList = new List<int>();
			optionChain.Expirations.All((expiration) =>
			{
				expiration.Value.Puts.All((contract) =>
				{
					flatContractIdList.Add(contract.Value.ConId);
					return true;
				});
				expiration.Value.Calls.All((contract) =>
				{
					flatContractIdList.Add(contract.Value.ConId);
					return true;
				});
				return true;
			});

			return new TaskResultData(instruction, flatContractIdList.Count > 0, flatContractIdList);
		}
	}
}
