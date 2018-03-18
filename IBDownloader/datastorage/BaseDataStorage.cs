using IBDownloader.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IBDownloader.DataStorage
{
    abstract class BaseDataStorage : IFrameworkLoggable
    {
		protected IDataProcessor _dataProcessor;
		protected ConcurrentQueue<IDataRow> _dataQueue = new ConcurrentQueue<IDataRow>();

		public BaseDataStorage(IDataProcessor DataProcessor = null)
		{
			_dataProcessor = DataProcessor;
		}

		public virtual void ProcessTaskResult(TaskResultData ResultData)
		{
			if (_dataProcessor != null && _dataProcessor.CheckIfSupported(ResultData))
			{
				//TODO: create a periodic processing queue to bundle and submit data

				IDataRow row = _dataProcessor.Convert(ResultData);

				_dataQueue.Enqueue(row);
			}
		}

		/// <summary>
		/// give data storage time to finish commit
		/// </summary>
		public virtual async void FlushAsync()
		{
			await Task.Delay(1);
		}
	}
}
