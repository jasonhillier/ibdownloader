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
			double limitStrikeHigh = 0;
			double limitStrikeLow = 0;

			//if (instruction.GetParameter("filter.range").ParseElse<double>(0) > 0)
			{
				//get the high-low during the past 6mos so we can filter down the option chain
				var highLow = await _Controller.HistoricalDataManager.GetPriceRange(instruction.contract, 6);
				double rangeDev = (highLow.Item1 - highLow.Item2) * instruction.GetParameter("filter.range").ParseElse<double>(0.3); //  +/- 30%
				limitStrikeHigh = highLow.Item1 + rangeDev;
				limitStrikeLow = highLow.Item2 - rangeDev;
				this.Log("Filtering to strikes between {0} and {1}", limitStrikeLow, limitStrikeHigh);
			}

			var optionChain = await _Controller.OptionManager.GetOptionChain(instruction.contract);

			List<int> flatContractIdList = new List<int>();
			optionChain.Expirations.All((expiration) =>
			{
				expiration.Value.Puts.All((contract) =>
				{
					if (contract.Value.Strike >= limitStrikeLow &&
						contract.Value.Strike <= limitStrikeHigh)
					{
						flatContractIdList.Add(contract.Value.ConId);
					}
					return true;
				});
				expiration.Value.Calls.All((contract) =>
				{
					if (contract.Value.Strike >= limitStrikeLow &&
						contract.Value.Strike <= limitStrikeHigh)
					{
						flatContractIdList.Add(contract.Value.ConId);
					}
					return true;
				});
				return true;
			});

			this.Log("Returning {0} option contracts.", flatContractIdList.Count);

			return new TaskResultData(instruction, flatContractIdList.Count > 0, flatContractIdList);
		}
	}
}
