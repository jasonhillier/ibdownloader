using System;
using System.Threading;

namespace IBDownloader
{
    class Program
    {
		

		static void Main(string[] args)
        {
			var controller = new IBController();
			controller.Connect();

			var taskHandler = new IBDTaskHandler(controller);
			taskHandler.Begin();
			taskHandler.OnTaskResult += TaskHandler_OnTaskResult;

			taskHandler.AddTask(new IBDTaskInstruction("TestTask"));
			Console.WriteLine("Hello World!");
			taskHandler.AddTask(new IBDTaskInstruction("thing 2"));

			Console.ReadLine();
        }

		private static void TaskHandler_OnTaskResult(tasks.TaskResultData obj)
		{
			Console.WriteLine("hey stuff=" + obj.Data.GetType().Name);
		}
	}
}
