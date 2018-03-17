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
	abstract class BaseManager
	{
		protected IBClient _ibClient;
		private static int _requestIdCounter = 1;
		private ConcurrentDictionary<int, ConcurrentBag<IBMultiMessageData>> _pendingRequestResults = new ConcurrentDictionary<int, ConcurrentBag<IBMultiMessageData>>();
		private ConcurrentDictionary<int, bool> _pendingRequestStatus = new ConcurrentDictionary<int, bool>();

		public BaseManager(IBClient ibClient)
		{
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
			if (!CheckRequestStillPending(requestId))
				return false;

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
			if (!CheckRequestStillPending(DataMessage.RequestId))
				return;

			//TODO: some type validation with original
			_pendingRequestResults[DataMessage.RequestId].Add(DataMessage);
			return;
		}

		protected bool CheckRequestStillPending(int requestId)
		{
			var isPending = _pendingRequestStatus.ContainsKey(requestId) &&
							!_pendingRequestStatus[requestId];

			if (!isPending)
			{
				Framework.LogError("Request {0} was already completed!", requestId);
			}

			return isPending;
		}

		private void _ibClient_Error(int arg1, int arg2, string arg3, Exception arg4)
		{
			//indicate that this request id is done doing whatever it was supposed to do
			_pendingRequestStatus[arg1] = true;

			//all we can do really
			Console.WriteLine("IB ERROR: {0} {1} {2} {3}", arg1, arg2, arg3, arg4);
		}

		protected void HandleEndMessage(int requestId)
		{
			_pendingRequestStatus[requestId] = true;
		}

		protected void HandleEndMessage(IBMultiMessageData EndMessage)
		{
			_pendingRequestStatus[EndMessage.RequestId] = true;
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

		protected async Task<List<T>> Dispatch<T>(int taskId) where T : IBMultiMessageData
		{
			_pendingRequestResults[taskId] = new ConcurrentBag<IBMultiMessageData>();
			_pendingRequestStatus[taskId] = false;

			//TODO: timeout handler
			while (!_pendingRequestStatus[taskId])
			{
				await Task.Delay(1000);
			}

			_pendingRequestStatus.Remove(taskId, out _);
			ConcurrentBag<IBMultiMessageData> data = null;
			_pendingRequestResults.Remove(taskId, out data);
			
			return data.ToList().ConvertAll((i)=>
			{
				return (T)i;
			});
		}
	}
}
