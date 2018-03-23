using IBDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IBDownloader.DataStorage
{
	public class Stdout : BaseDataStorage
	{
		public Stdout(IDataProcessor DataProcessor)
			: base(DataProcessor)
		{

		}

		public override void ProcessTaskResult(TaskResultData ResultData)
		{
			if (_dataProcessor != null)
			{
				base.ProcessTaskResult(ResultData);
				return;
			}

			Framework.DebugPrint(ResultData);
		}

		protected override async Task ProcessQueue()
		{
			var rows = Dequeue();
			Framework.DebugPrint(rows);

			await Task.Delay(1);
		}
	}
}
