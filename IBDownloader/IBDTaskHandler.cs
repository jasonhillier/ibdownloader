using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IBDownloader.Tasks;
using System.Reflection;
using IBApi;

namespace IBDownloader
{
	[Serializable]
	class IBDTaskInstruction
	{
		public IBDTaskInstruction() { }
		public IBDTaskInstruction(string TaskType)
		{
			this.taskType = TaskType;
			this.contract = new Contract();
			this.parameters = new Dictionary<string, string>();
		}

		/// <summary>
		/// Gets a paramter value, or from settings if not found
		/// </summary>
		public string GetParameter(string Key)
		{
			if (parameters != null && parameters.ContainsKey(Key))
				return parameters[Key];
			else
				return Framework.Settings[Key];
		}

		public string Symbol
		{
			get	{ return contract.Symbol; }
			set	{ this.contract.Symbol = value; }
		}

		public Dictionary<string, string> parameters { get; set; }
		public Contract contract { get; set; }
		public string taskType { get; set; }
		public dynamic datum { get; set; }
	}

    class IBDTaskHandler
    {
		private ConcurrentQueue<IBDTaskInstruction> _TaskQueue = new ConcurrentQueue<IBDTaskInstruction>();
		private bool _AbortFlag;

		public IBDTaskHandler(IBController Controller)
		{
			this.Controller = Controller;
		}

		public event Action<TaskResultData> OnTaskResult;
		public IBController Controller { get; private set; }

		/// <summary>
		/// Begin handling tasks in background
		/// </summary>
		public void Begin()
		{
			_AbortFlag = false;
			_RunTasks().ConfigureAwait(false);
		}

		/// <summary>
		/// Begin handling tasks in background
		/// </summary>
		public async System.Threading.Tasks.Task BeginAsync()
		{
			_AbortFlag = false;
			await _RunTasks();
		}

		/// <summary>
		/// Stop handling tasks in queue
		/// </summary>
		public void Stop()
		{
			_AbortFlag = true;
		}

		/// <summary>
		/// Add task to queue
		/// </summary>
		public void AddTask(IBDTaskInstruction Task)
		{
			_TaskQueue.Enqueue(Task);
		}

		//Run tasks as they come
		private async System.Threading.Tasks.Task _RunTasks()
		{
			while (!_AbortFlag)
			{
				await System.Threading.Tasks.Task.Delay(100);

				if (_TaskQueue.Count > 0 && this.Controller.IsConnected)
				{
					IBDTaskInstruction instruction;
					if (_TaskQueue.TryDequeue(out instruction))
					{
						BaseTask task;
						try
						{
							task = CreateTask(instruction.taskType);
						}
						catch
						{
							Framework.LogError("No task of type {0} is defined!", instruction.taskType);
							continue;
						}

						TaskResultData resultData = null;
						try
						{
							resultData = await task.ExecuteAsync(instruction);
						}
						catch (Exception ex)
						{
							Framework.LogError("Error in task for instruction {0}", instruction.taskType);
							Framework.LogError(ex.Message);
							Framework.LogError(ex.StackTrace);
							return;
						}

						if (resultData != null)
						{
							Framework.Log("Completed task for instruction {0}", instruction.taskType);

							if (this.OnTaskResult != null)
								this.OnTaskResult(resultData);
						}
					}
				}
			}
		}

		public BaseTask CreateTask(Type TaskType)
		{
			return Activator.CreateInstance(TaskType) as BaseTask;
		}

		public BaseTask CreateTask(string TaskType)
		{
			Type type = Assembly.GetExecutingAssembly().GetType("IBDownloader.Tasks." + TaskType);
			return Activator.CreateInstance(type, this) as BaseTask;
		}
	}
}
