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
        
        /// <summary>
		/// Indicate if this task wants repeated execution until no data is left
        /// </summary>
		public virtual bool IsMulti { get { return false; } }

		public virtual System.Threading.Tasks.Task<TaskResultData> ExecuteAsync(IBDTaskInstruction Instruction)
		{
			throw new NotImplementedException("Check if IsMulti flag is set according to implemented Execute method!");
		}

		public virtual System.Threading.Tasks.Task<TaskResultData> ExecuteMultiAsync(IBDTaskInstruction Instruction)
        {
			throw new NotImplementedException("Check if IsMulti flag is set according to implemented Execute method!");
        }

		public async System.Threading.Tasks.Task<bool> LookupDerivative(IBDTaskInstruction Instruction)
		{
			var selectedContracts = await _Controller.ContractManager.GetContractDetails(Instruction.contract);
			if (selectedContracts.Count < 1)
				return false;
			//reset to full derivative contract definition
			Instruction.contract = selectedContracts[0].Summary;
			int underlyingConId = selectedContracts[0].UnderConId;
			//lookup underlying
			var underlying = await _Controller.ContractManager.GetContract(underlyingConId, string.Empty); //TODO: empty exchange may not always work
			if (underlying == null)
				return false;

			Instruction.metadata["underlying"] = underlying;

			return true;
		}

		public async System.Threading.Tasks.Task<DateTime> GetStartDate(IBDTaskInstruction Instruction)
		{
			DateTime startDate = Instruction.GetParameter("StartDate").ParseElse(DateTime.MinValue);

			if (startDate == DateTime.MinValue)
			{
				startDate = await _Controller.HistoricalDataManager.GetEarliestDataTime(Instruction.contract);
			}
			startDate = startDate.StartOfDay();

			return startDate;
		}
	}
}
