﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IBApi;

namespace IBDownloader.Tasks
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
				Framework.Log("\n{0}=", i.Tag);
				Framework.DebugPrint(i);
				return true;
			});


			var contract = new Contract()
				{
					Symbol = "SPY",
					SecType = "STK",
					Exchange = "SMART"
				};
			List<ContractDetails> details = await _Controller.ContractManager.GetContractDetails(contract);

			Framework.Log("CONTRACT INFO");
			details.All((i) =>
			{
				Framework.Log(i.LongName);
				return true;
			});

			return new TaskResultData(instruction, true, accSummary);
		}
    }
}
