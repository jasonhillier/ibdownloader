using System;
using System.Collections.Generic;
using System.Text;

namespace IBDownloader.Tasks
{
	public class TaskResultData
	{
		public TaskResultData(IBDTaskInstruction Instruction, bool HasData=false, object DataObject = null)
		{
			this.Instruction = Instruction;
			this.HasData = HasData;
			this.Data = DataObject;
			this.Metadata = new Dictionary<string, object>();
		}

		public IBDTaskInstruction Instruction {get; set;}
		public bool HasData { get; set; }
		public object Data { get; set; }

		public Dictionary<string, object> Metadata {get;set;}

		public static TaskResultData Failure(IBDTaskInstruction Instruction, string Message = null)
		{
			if (!string.IsNullOrEmpty(Message))
			{
				Framework.LogError("Task failed: " + Message);
			}

			return new TaskResultData(Instruction, false);
		}
	}

    public abstract class BaseTask : IFrameworkLoggable
    {
		protected IBDTaskHandler _TaskHandler;
		protected IBController _Controller;

		public BaseTask(IBDTaskHandler TaskHandler)
		{
			_TaskHandler = TaskHandler;
			_Controller = _TaskHandler.Controller;
		}

		public abstract System.Threading.Tasks.Task<TaskResultData> ExecuteAsync(IBDTaskInstruction Instruction);

		public async System.Threading.Tasks.Task<bool> LookupDerivative(IBDTaskInstruction Instruction)
		{
			var selectedContracts = await _Controller.ContractManager.GetContractDetails(Instruction.contract);
			if (selectedContracts.Count < 1)
				return false;
			//reset to full derivative contract definition
			Instruction.contract = selectedContracts[0].Summary;
			int underlyingConId = selectedContracts[0].UnderConId;
			//lookup underlying
			var underlying = await _Controller.ContractManager.GetContract(underlyingConId);
			if (underlying == null)
				return false;

			Instruction.metadata["underlying"] = underlying;

			return true;
		}
	}
}
