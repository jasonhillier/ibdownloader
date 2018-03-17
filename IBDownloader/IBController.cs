﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IBApi;
using IBDownloader.managers;
using IBDownloader;
using IBDownloader.messages;

namespace IBDownloader
{
	class IBController
	{
		protected IBClient _ibClient;
		private EReaderMonitorSignal _signal = new EReaderMonitorSignal();

		public IBController()
		{
			_ibClient = new IBClient(_signal);
			_ibClient.ConnectionClosed += _ibClient_ConnectionClosed;
			_ibClient.NextValidId += HandleMessage;

			this.IsConnected = false;
			this.AccountManager = new AccountManager(_ibClient);
			this.ContractManager = new ContractManager(_ibClient);
		}

		public void Connect()
		{
			try
			{
				Framework.Log("Connecting to IB gateway on localhost:4001...");

				_ibClient.ClientId = 21;
				_ibClient.ClientSocket.eConnect("localhost", 4001, _ibClient.ClientId);

				var reader = new EReader(_ibClient.ClientSocket, _signal);

				reader.Start();

				//process incoming tcp data
				new Thread(() => { while (_ibClient.ClientSocket.IsConnected()) { _signal.waitForSignal(); reader.processMsgs(); } }) { IsBackground = true }.Start();
			}
			catch (Exception ex)
			{
				throw ex;
				//HandleErrorMessage(new ErrorMessage(-1, -1, "Please check your connection attributes."));
			}
		}

		public bool IsConnected { get; private set; }
		public AccountManager AccountManager { get; private set; }
		public ContractManager ContractManager { get; private set; }

		private void HandleMessage(ConnectionStatusMessage m)
		{
			this.IsConnected = m.IsConnected;
			Framework.Log("Connected to IBGateway:" + m.IsConnected);
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
