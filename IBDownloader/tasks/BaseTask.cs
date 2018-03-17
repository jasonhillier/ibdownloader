using System;
using System.Collections.Generic;
using System.Text;

namespace IBDownloader.tasks
{
    abstract class BaseTask
    {
		protected IBDTaskHandler _TaskHandler;

		public BaseTask(IBDTaskHandler TaskHandler)
		{
			_TaskHandler = TaskHandler;
		}

		public abstract System.Threading.Tasks.Task ExecuteAsync(IBDTaskInstruction Instruction);
	}
}
