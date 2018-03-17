using System;
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


			List<Contract> contracts = await _Controller.ContractManager.GetContracts(Managers.SecurityType.STK, "SPY");
			var contract = contracts.First();

			List<ContractDetails> details = await _Controller.ContractManager.GetContractDetails(contract);

			Framework.Log("CONTRACT INFO");
			details.All((i) =>
			{
				Framework.Log(i.LongName);
				return true;
			});

			var optionProfiles = await _Controller.OptionManager.GetOptionProfiles(contract);
			optionProfiles.All((profile) =>
			{
				Framework.Log("Option profile for SPY on {0}", profile.Exchange);
				Framework.Log("STRIKES");
				profile.Strikes.All((s) =>
				{
					Framework.Log(s.ToString());
					return true;
				});
				return true;
			});

			Framework.Log("req hist data...");
			var data = await _Controller.HistoricalDataManager.GetHistoricalData(contract, DateTime.Now, Managers.BarSize.M15);
			data.All((bar) =>
			{
				Framework.Log("{0} close={1}", bar.Date, bar.Close);
				return true;
			});


			return new TaskResultData(instruction, true, accSummary);
		}
    }
}
