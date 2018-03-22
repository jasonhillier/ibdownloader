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
			Framework.DebugPrint(ResultData.Data);
		}
	}
}
