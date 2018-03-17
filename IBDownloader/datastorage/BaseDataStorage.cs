using IBDownloader.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IBDownloader.DataStorage
{
    abstract class BaseDataStorage
    {
		protected IDataProcessor _dataProcessor;
		protected ConcurrentQueue<IDataRow> _dataQueue = new ConcurrentQueue<IDataRow>();

		public BaseDataStorage(IDataProcessor DataProcessor)
		{
			_dataProcessor = DataProcessor;
		}

		public virtual async void ProcessTaskResultAsync(TaskResultData ResultData)
		{
			if (_dataProcessor.CheckIfSupported(ResultData))
			{
				//TODO: use a periodic processing queue to bundle and submit data
				await Task.Delay(1);

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
