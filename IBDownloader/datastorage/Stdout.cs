﻿using IBDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IBDownloader.DataStorage
{
	class Stdout : BaseDataStorage
	{
		public Stdout(IDataProcessor DataProcessor)
			: base(DataProcessor)
		{

		}

		public override void ProcessTaskResultAsync(TaskResultData ResultData)
		{
			Framework.DebugPrint(ResultData.Data);
		}
	}
}
