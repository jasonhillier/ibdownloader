using System;
using System.Collections.Generic;
using System.Text;
using IBDownloader.messages;
using IBApi;
using IBDownloader.Tasks;
using System.Collections;

namespace IBDownloader.DataStorage.Processors
{
	public class DirectProcessor : IDataProcessor, IFrameworkLoggable
	{
		public virtual bool CheckIfSupported(TaskResultData taskResult)
		{
			if (taskResult.Data is IEnumerable<IDataRow>)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public virtual IEnumerable<IDataRow> Convert(TaskResultData taskResult)
		{
			return (IEnumerable<IDataRow>)taskResult.Data;
		}
	}
}