using IBDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace IBDownloader.DataStorage
{
	public class JSONFile : BaseDataStorage
	{
		public JSONFile(string FilePathName = "file.json")
			: base()
		{
			this.FilePathName = FilePathName;
		}

		public string FilePathName { get; set; }

		public override void ProcessTaskResult(TaskResultData ResultData)
		{
			string fileName = ResultData.Instruction.GetParameter("file");
			if (String.IsNullOrEmpty(fileName)) fileName = FilePathName;

			string jsonData = JsonConvert.SerializeObject(ResultData.Data);
			File.WriteAllBytes(fileName, Encoding.UTF8.GetBytes(jsonData));

			this.Log("Wrote JSON to file " + fileName);
		}
	}
}
