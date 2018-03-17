/* Copyright (C) 2013 Interactive Brokers LLC. All rights reserved.  This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBApi;

namespace IBDownloader.messages
{
    class ExecutionMessage : Managers.IBMultiMessageData
	{
        private int reqId;
        private Contract contract;
        private Execution execution;

        public ExecutionMessage(int ReqId, Contract contract, Execution execution)
        {
            reqId = ReqId;
            Contract = contract;
            Execution = execution;
        }

        public Contract Contract
        {
            get { return contract; }
            set { contract = value; }
        }
        
        public Execution Execution
        {
            get { return execution; }
            set { execution = value; }
        }

        public int RequestId
        {
            get { return reqId; }
            set { reqId = value; }
        }

    }
}
