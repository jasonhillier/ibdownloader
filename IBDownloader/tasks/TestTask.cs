using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace IBDownloader.tasks
{
    class TestTask : BaseTask
    {
		public TestTask(IBDTaskHandler TaskHandler)
			: base(TaskHandler)
		{

		}

		public override async System.Threading.Tasks.Task ExecuteAsync(IBDTaskInstruction instruction)
		{
			Framework.Log("== Requesting Account Info ==");
			var accSummary = await _TaskHandler.Controller.AccountManager.GetAccountSummary();

			accSummary.All((i) =>
			{
				Framework.Log("\n{0}=", i.Key);
				Framework.DebugPrint(i.Value);
				return true;
			});
		}
    }
}
