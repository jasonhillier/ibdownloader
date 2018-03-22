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
		protected bool _processingStarted = false;

		public BaseDataStorage(IDataProcessor DataProcessor = null)
		{
			_dataProcessor = DataProcessor;
		}

		public virtual void ProcessTaskResult(TaskResultData ResultData)
		{
			try
			{
				if (_dataProcessor != null && _dataProcessor.CheckIfSupported(ResultData))
				{
					//TODO: create a periodic processing queue to bundle and submit data
					this.PreConvert(ResultData);

					var rows = _dataProcessor.Convert(ResultData);

					foreach (var row in rows)
						_dataQueue.Enqueue(row);

					if (!_processingStarted)
						this.BeginProcessing();
				}
			} catch (Exception ex)
			{
				this.LogError("Result processing error:");
				this.LogError(ex.Message);
				this.LogError(ex.StackTrace);
			}
		}

		/// <summary>
		/// Dequeue all available items waiting in queue
		/// </summary>
		protected virtual List<IDataRow> Dequeue()
		{
			IDataRow row;
			List<IDataRow> rows = new List<IDataRow>();
			while (_dataQueue.TryDequeue(out row))
			{
				rows.Add(row);
			}

			return rows;
		}

		protected virtual void BeginProcessing()
		{
			_processingStarted = true;

			Task.Run(async () =>
			{
				//give it a sec for buffering to start
				await Task.Delay(1000);

				if (_dataQueue.Count>0)
				{
					try
					{
						await ProcessQueue();
					}
					catch (Exception ex)
					{
						this.LogError("Queue processing error:");
						this.LogError(ex.Message);
						this.LogError(ex.StackTrace);
					}
				}

				_processingStarted = false;
			});
		}

		protected virtual async Task ProcessQueue()
		{
			await Task.Delay(1);
			//not implemented
		}

		protected virtual void PreConvert(TaskResultData ResultData)
		{
			if (_dataProcessor is IDataPreProcessor)
			{
				((IDataPreProcessor)_dataProcessor).PreConvert(this, ResultData);
			}
		}

		/// <summary>
		/// give data storage time to finish commit
		/// </summary>
		public virtual async Task FlushAsync(bool waitForData = false)
		{
			if (_dataProcessor == null)
				return;

			//TODO: timeout?
			if (waitForData)
			{
				while(!_processingStarted)
				{
					await Task.Delay(1000);
				}
			}

			//wait for all data to get processed
			while (_dataQueue.Count > 0 || _processingStarted)
			{
				await Task.Delay(1000);
			}
		}
	}
}
