using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IBApi;
using IBDownloader.managers;
using IBSampleApp;
using IBSampleApp.messages;
using System.Linq;

namespace IBDownloader
{
	class IBController
	{
		protected IBClient _ibClient;
		private EReaderMonitorSignal _signal = new EReaderMonitorSignal();
		private Queue<string> _commandQueue = new Queue<string>();

		public IBController()
		{
			_ibClient = new IBClient(_signal);
			_ibClient.ConnectionClosed += _ibClient_ConnectionClosed;
			_ibClient.NextValidId += HandleMessage;

			this.AccountManager = new AccountManager(_ibClient);
		}

		public void Connect()
		{
			try
			{
				_ibClient.ClientId = 1;
				_ibClient.ClientSocket.eConnect("localhost", 4001, _ibClient.ClientId);

				var reader = new EReader(_ibClient.ClientSocket, _signal);

				reader.Start();

				Console.WriteLine("con");

				new Thread(() => { while (_ibClient.ClientSocket.IsConnected()) { _signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

				this.AddTask("wtf");
			}
			catch (Exception ex)
			{
				throw ex;
				//HandleErrorMessage(new ErrorMessage(-1, -1, "Please check your connection attributes."));
			}
		}

		public AccountManager AccountManager { get; private set; }

		public async void AddTask(string Task)
		{
			_commandQueue.Enqueue(Task);

			Console.WriteLine("== Requesting Account Info ==");
			var accSummary = await AccountManager.GetAccountSummary();

			accSummary.All((i) =>
			{
				Console.WriteLine("\n{0}=", i.Key);
				Log(i.Value);
				return true;
			});
		}

		private void HandleMessage(ConnectionStatusMessage m)
		{
			Console.WriteLine("status con:" + m.IsConnected);
		}

		private void _ibClient_ConnectionClosed()
		{
			Console.WriteLine("it ded");
		}

		static void Log(object @object)
		{
			foreach (var property in @object.GetType().GetProperties())
			{
				var value = property.GetValue(@object, null);
				if (value != null)
					Console.WriteLine(property.Name + ": " + value.ToString());
			}
		}
	}
}
