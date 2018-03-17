using IBSampleApp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IBDownloader.managers
{
	abstract class BaseManager
	{
		protected IBClient _ibClient;
		private static int _requestIdCounter = 1;
		private ConcurrentDictionary<int, object> _pendingRequestResults = new ConcurrentDictionary<int, object>();
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

		protected T GetPendingRequestData<T>(int requestId)
		{
			return (T)_pendingRequestResults[requestId];
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

		protected void MarkCompleted(int requestId)
		{
			_pendingRequestStatus[requestId] = true;
		}

		protected async Task<T> Dispatch<T>(int taskId) where T : new()
		{
			_pendingRequestResults[taskId] = new T();
			_pendingRequestStatus[taskId] = false;

			//TODO: timeout handler
			while (!_pendingRequestStatus[taskId])
			{
				await Task.Delay(1000);
			}

			_pendingRequestStatus.Remove(taskId, out _);
			object data = null;
			_pendingRequestResults.Remove(taskId, out data);

			return (T)data;
		}
	}
}
