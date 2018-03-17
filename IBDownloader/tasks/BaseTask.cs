using System;
using System.Collections.Generic;
using System.Text;

namespace IBDownloader.Tasks
{
	class TaskResultData
	{
		public TaskResultData(IBDTaskInstruction Instruction, bool HasData=false, object DataObject = null)
		{
			this.Instruction = Instruction;
			this.HasData = HasData;
			this.Data = DataObject;
		}

		public IBDTaskInstruction Instruction {get; set;}
		public bool HasData { get; set; }
		public object Data { get; set; }
	}

    abstract class BaseTask
    {
		protected IBDTaskHandler _TaskHandler;
		protected IBController _Controller;

		public BaseTask(IBDTaskHandler TaskHandler)
		{
			_TaskHandler = TaskHandler;
			_Controller = _TaskHandler.Controller;
		}

		public abstract System.Threading.Tasks.Task<TaskResultData> ExecuteAsync(IBDTaskInstruction Instruction);
	}
}
