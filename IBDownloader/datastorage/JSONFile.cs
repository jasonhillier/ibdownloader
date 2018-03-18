using IBDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace IBDownloader.DataStorage
{
	class JSONFile : BaseDataStorage
	{
		public JSONFile(IDataProcessor DataProcessor = null)
			: base(DataProcessor)
		{

		}

		public override void ProcessTaskResult(TaskResultData ResultData)
		{
			string fileName = ResultData.Instruction.GetParameter("file");
			if (String.IsNullOrEmpty(fileName)) fileName = "file.json";

			string jsonData = JsonConvert.SerializeObject(ResultData.Data);
			File.WriteAllBytes(fileName, Encoding.UTF8.GetBytes(jsonData));
		}
	}
}
