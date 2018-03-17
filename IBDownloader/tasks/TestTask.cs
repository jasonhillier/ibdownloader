using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IBApi;

namespace IBDownloader.tasks
{
    class TestTask : BaseTask
    {
		public TestTask(IBDTaskHandler TaskHandler)
			: base(TaskHandler)
		{

		}

		public override async System.Threading.Tasks.Task<TaskResultData> ExecuteAsync(IBDTaskInstruction instruction)
		{
			var accSummary = await _Controller.AccountManager.GetAccountSummary();

			accSummary.All((i) =>
			{
				Framework.Log("\n{0}=", i.Key);
				Framework.DebugPrint(i.Value);
				return true;
			});


			var contract = new Contract()
				{
					Symbol = "SPY",
					SecType = "STOCK"
				};
			ContractDetails details = await _Controller.ContractManager.GetContractDetails(contract);

			Framework.Log("CONTRACT INFO");
			Framework.Log(details.Cusip);

			return new TaskResultData(instruction, true, accSummary);
		}
    }
}
