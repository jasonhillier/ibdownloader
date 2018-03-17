using IBDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace IBDownloader.DataStorage
{
	public interface IDataRow
	{
		string id { get; }
		DateTime date { get; }
	}

	interface IDataProcessor
    {
		bool CheckIfSupported(TaskResultData taskResult);
		IDataRow Convert(TaskResultData taskResult);
		//IEnumerable<IDataRow> Convert(IEnumerable<TaskResultData> taskResults);
    }
}
