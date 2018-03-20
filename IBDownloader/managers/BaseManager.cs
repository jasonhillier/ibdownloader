using IBDownloader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace IBDownloader.Managers
{
	public interface IBMultiMessageData
	{
		int RequestId { get; }
	}
	abstract class BaseManager : IFrameworkLoggable
	{
		const int WAIT_TIMEOUT = 120; //no responses for x seconds
		protected IBClient _ibClient;
		protected IBController _Controller;
		private static int _requestIdCounter = 1;
		private ConcurrentDictionary<int, ConcurrentBag<IBMultiMessageData>> _pendingRequestResults = new ConcurrentDictionary<int, ConcurrentBag<IBMultiMessageData>>();
		private ConcurrentDictionary<int, DateTime> _pendingRequestStatus = new ConcurrentDictionary<int, DateTime>();

		public BaseManager(IBController Controller, IBClient ibClient)
		{
			_Controller = Controller;
			_ibClient = ibClient;
			_ibClient.Error += _ibClient_Error;
		}

		public int GetNextTaskId()
		{
			return Interlocked.Increment(ref _requestIdCounter);
		}

		/*
		protected T GetPendingRequestData<T>(int requestId) where T : IMessageData
		{
			return (T)_pendingRequestResults[requestId];
		}
		*/

		protected bool SetPendingRequestData<T>(int requestId, List<T> Data) where T : IBMultiMessageData
		{
			if (!CheckRequestStillPending(requestId, typeof(T)))
				return false;

			ResetPendingRequestTimeout(requestId);

			ConcurrentBag<IBMultiMessageData> dataList = new ConcurrentBag<IBMultiMessageData>();
			Data.All((i) =>
			{
				dataList.Add(i);
				return true;
			});

			_pendingRequestResults[requestId] = dataList;
			return true;
		}

		protected void AppendPendingRequestData<T>(T DataMessage) where T : IBMultiMessageData
		{
			if (!CheckRequestStillPending(DataMessage.RequestId, typeof(T)))
				return;

			ResetPendingRequestTimeout(DataMessage.RequestId);

			//TODO: some type validation with original
			_pendingRequestResults[DataMessage.RequestId].Add(DataMessage);
			return;
		}

		protected bool CheckRequestStillPending(int requestId, Type ExpectedMessageType = null)
		{
			var isPending = _pendingRequestStatus.ContainsKey(requestId) &&
							_pendingRequestStatus[requestId] != DateTime.MinValue;

			if (!isPending)
			{
				if (ExpectedMessageType == null)
					ExpectedMessageType = typeof(System.DBNull);

				this.LogWarn("Request {0} for {1} was already completed!", requestId, ExpectedMessageType.Name);
			}

			return isPending;
		}

		protected void ResetPendingRequestTimeout(int requestId)
		{
			if (_pendingRequestStatus.ContainsKey(requestId) &&
				_pendingRequestStatus[requestId] != DateTime.MinValue)
			{
				_pendingRequestStatus[requestId] = DateTime.Now;
			}
		}

		private void _ibClient_Error(int reqId, int arg2, string arg3, Exception arg4)
		{
			//indicate that this request id is done doing whatever it was supposed to do
			_pendingRequestStatus[reqId] = DateTime.MinValue;

			//all we can do really
			Framework.Log("IB ERROR: {0} {1} {2} {3}", reqId, arg2, arg3, arg4);

			//assume that IB won't send anything more related to this request
			HandleEndMessage(reqId);
		}

		protected void HandleEndMessage(int requestId)
		{
			Task.Delay(100).Wait(); //make sure enough time is give to process related messages

			_pendingRequestStatus[requestId] = DateTime.MinValue;
		}

		protected void HandleEndMessage(IBMultiMessageData EndMessage)
		{
			Task.Delay(100).Wait(); //make sure enough time is give to process related messages

			_pendingRequestStatus[EndMessage.RequestId] = DateTime.MinValue;
		}

		protected async Task<List<T>> Dispatch<T>(Func<int, bool> initiator) where T : IBMultiMessageData
		{
			int requestId = this.GetNextTaskId();
			var job = this.Dispatch<T>(requestId).GetAwaiter();

			if (!initiator(requestId))
			{
				//initiator aborted request
				this.HandleEndMessage(requestId);
			}

			while (!job.IsCompleted)
			{
				await Task.Delay(100);
			}

			return job.GetResult();
		}

		protected async Task<List<T>> Dispatch<T>(int requestId) where T : IBMultiMessageData
		{
			_pendingRequestResults[requestId] = new ConcurrentBag<IBMultiMessageData>();
			_pendingRequestStatus[requestId] = DateTime.Now;

			
			while (_pendingRequestStatus[requestId] != DateTime.MinValue)
			{
				await Task.Delay(1000);

				if ((DateTime.Now - _pendingRequestStatus[requestId]).TotalSeconds > WAIT_TIMEOUT &&
					_pendingRequestStatus[requestId] != DateTime.MinValue)
				{
					this.LogError("Request {1} of {2} timed out.", requestId, typeof(T).GetType().Name);
					_pendingRequestStatus[requestId] = DateTime.MinValue; //let it end
				}
			}

			_pendingRequestStatus.Remove(requestId, out _);
			ConcurrentBag<IBMultiMessageData> data = null;
			_pendingRequestResults.Remove(requestId, out data);

			this.Log("Received {0} messages", data.Count);
			
			return data.ToList().ConvertAll((i)=>
			{
				return (T)i;
			});
		}
	}
}
