using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IBDownloader.Tasks;
using System.Reflection;

namespace IBDownloader
{

	class IBDTaskInstruction
	{
		public IBDTaskInstruction(string TaskType)
		{
			this.TaskType = TaskType;
		}

		public string TaskType { get; set; }
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
							task = CreateTask(instruction.TaskType);
						}
						catch
						{
							Framework.LogError("No task of type {0} is defined!", instruction.TaskType);
							continue;
						}

						await System.Threading.Tasks.Task.Run(async () =>
						{
							var resultData = await task.ExecuteAsync(instruction);
							if (this.OnTaskResult != null)
								this.OnTaskResult(resultData);
						});
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
