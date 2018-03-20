using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IBDownloader.Tasks;
using System.Reflection;
using IBApi;
using System.Threading;

namespace IBDownloader
{
	[Serializable]
	class IBDTaskInstruction
	{
		public IBDTaskInstruction()
		{
			this.contract = new Contract() { Exchange = "SMART", Currency = "USD" };
			this.parameters = new Dictionary<string, string>();
			this.metadata = new Dictionary<string, object>();
		}

		public IBDTaskInstruction(string TaskType)
			: this()
		{
			this.taskType = TaskType;
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
		public string SecType
		{
			get { return contract.SecType; }
			set { contract.SecType = value; }
		}
		public int ConId
		{
			get { return contract.ConId; }
			set { this.contract.ConId = value; }
		}

		public Dictionary<string, object> metadata { get; set; }
		public Dictionary<string, string> parameters { get; set; }
		public Contract contract { get; set; }
		public string taskType { get; set; }
		public dynamic datum { get; set; }
	}

    class IBDTaskHandler : IFrameworkLoggable
    {
		private int _TotalTaskCounter = 0; //count how many total tasks have been run
		private ConcurrentQueue<IBDTaskInstruction> _TaskQueue = new ConcurrentQueue<IBDTaskInstruction>();
		private bool _pendingTasks = false;
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
		/// Begin handling tasks in background, stop when all tasks have finished processing.
		/// </summary>
		public async System.Threading.Tasks.Task BeginAsync()
		{
			_AbortFlag = false;
			_RunTasks().ConfigureAwait(false);

			//wait around until there is nothing left to do
			while(_TaskQueue.Count > 0 || _pendingTasks)
			{
				await System.Threading.Tasks.Task.Delay(1000);
			}

			//then stop it
			this.Stop();
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
			this.Log("Waiting for tasks...");

			while (!_AbortFlag)
			{
				await System.Threading.Tasks.Task.Delay(100);

				if (!this.Controller.IsConnected)
					continue;

				if (_TaskQueue.Count == 0)
				{
					_pendingTasks = false;
				}
				else
				{
					_pendingTasks = true;

					IBDTaskInstruction instruction;
					if (_TaskQueue.TryDequeue(out instruction))
					{
						_TotalTaskCounter++;
						this.Log("Running task {0} with {1} remaining.", _TotalTaskCounter, _TaskQueue.Count);

						BaseTask task;
						try
						{
							task = CreateTask(instruction.taskType);
						}
						catch
						{
							this.LogError("No task of type {0} is defined!", instruction.taskType);
							continue;
						}

						TaskResultData resultData = null;
						try
						{
							resultData = await task.ExecuteAsync(instruction);
						}
						catch (Exception ex)
						{
							this.LogError("Error in task for instruction {0}", instruction.taskType);
							this.LogError(ex.Message);
							this.LogError(ex.StackTrace);
							return;
						}

						if (resultData != null)
						{
							this.Log("Completed task for instruction {0}", instruction.taskType);

							try
							{
								if (this.OnTaskResult != null)
									this.OnTaskResult(resultData);
							}
							catch (Exception ex)
							{
								this.LogError("Error in processing task result for instruction {0}", instruction.taskType);
								this.LogError(ex.Message);
								this.LogError(ex.StackTrace);
								return;
							}
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
