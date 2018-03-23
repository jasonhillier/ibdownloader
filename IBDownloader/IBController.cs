using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IBApi;
using IBDownloader.Managers;
using IBDownloader;
using IBDownloader.messages;

namespace IBDownloader
{
	public class IBController : IFrameworkLoggable
	{
		private protected IBClient _ibClient;
		private EReaderMonitorSignal _signal = new EReaderMonitorSignal();

		public IBController()
		{
			_ibClient = new IBClient(_signal);
			_ibClient.ConnectionClosed += _ibClient_ConnectionClosed;
			_ibClient.NextValidId += HandleMessage;

			this.IsConnected = false;
			this.AccountManager = new AccountManager(this, _ibClient);
			this.ContractManager = new ContractManager(this, _ibClient);
			this.HistoricalDataManager = new HistoricalDataManager(this, _ibClient);
			this.OptionManager = new OptionManager(this, _ibClient);
		}

		public bool IsConnected { get; private set; }
		public AccountManager AccountManager { get; private set; }
		public ContractManager ContractManager { get; private set; }
		public HistoricalDataManager HistoricalDataManager { get; private set; }
		public OptionManager OptionManager { get; private set; }

		public bool Connect()
		{
			try
			{
				string host = Framework.Settings.Get("IB_HOST", "localhost");
				int port = Framework.Settings.Get("IB_PORT", 4001);
				int clientId = Framework.Settings.Get("IB_CLIENTID", 11);

				this.Log("Connecting to IB gateway on {0}:{1}... (client_id={2})", host, port, clientId);

				_ibClient.ClientId = clientId;
				_ibClient.ClientSocket.eConnect(host, port, _ibClient.ClientId);

				var reader = new EReader(_ibClient.ClientSocket, _signal);

				reader.Start();

				//process incoming tcp data
				new Thread(() => { while (_ibClient.ClientSocket.IsConnected()) { _signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();

				//startup and connect ran fine
				return true;
			}
			catch (Exception ex)
			{
				throw ex;
				//HandleErrorMessage(new ErrorMessage(-1, -1, "Please check your connection attributes."));
				return false;
			}
		}

		private void HandleMessage(ConnectionStatusMessage m)
		{
			this.IsConnected = m.IsConnected;
			Framework.Log(this, "Connected to IBGateway:" + m.IsConnected);
		}

		private void _ibClient_ConnectionClosed()
		{
			this.IsConnected = false;
			Framework.LogError("Connection to IBGateway lost!");

			//this isn't supposed to happen
			//TODO: maybe do some retry/reconnect handling
			throw new Exception("Connection to IBGateway lost!");
		}
	}
}
