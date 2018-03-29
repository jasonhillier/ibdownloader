using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Shuttle.Core.Cron;

namespace IBDownloader
{
    public class CronScheduler
    {
		private const int CHECK_INTERVAL = 1000 * 59; //just under a minute
		private IBDTaskHandler _taskHandler;
		private List<CronTask> _cronTasks;
		private bool _abortFlag = false;

		public CronScheduler(IBDTaskHandler TaskHandler)
		{
			_taskHandler = TaskHandler;
		}

		public async System.Threading.Tasks.Task RunFileAsync(string FilePathName)
		{
			using (StreamReader file = new StreamReader(File.Open(FilePathName, FileMode.Open)))
			{
				string cronTaskListJson = file.ReadToEnd();

				_cronTasks = JsonConvert.DeserializeObject<List<CronTask>>(cronTaskListJson);
				Framework.Log("Scheduling {0} tasks...", _cronTasks.Count);
			}

			foreach(var task in _cronTasks)
			{
				Framework.Log("Scheduled {0} for {1}", task.instruction.taskType, task.Expression.NextOccurrence().ToString());
			}

			while(!_abortFlag)
			{
				foreach (var task in _cronTasks)
				{
					if (task.CheckIfTriggered())
					{
						Framework.Log("Running scheduled task {0}", task.instruction.taskType);
						_taskHandler.AddTask(task.instruction);
					}
				}

				await System.Threading.Tasks.Task.Delay(CHECK_INTERVAL);
			}
		}

		public class CronTask
		{
			CronExpression _cronExpression = null;

			public bool CheckIfTriggered()
			{
				DateTime moment = this.Expression.GetNextOccurrence(DateTime.Now);

				//should be right on the money
				if ((DateTime.Now - moment).TotalMinutes < 1)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			public CronExpression Expression
			{
				get
				{
					if (_cronExpression == null)
						_cronExpression = new CronExpression(this.cron);
					return _cronExpression;
				}
			}
			public string cron { get; set; }
			public IBDTaskInstruction instruction { get; set; }
		}
	}
}
