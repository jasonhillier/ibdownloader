/* Copyright (C) 2013 Interactive Brokers LLC. All rights reserved.  This code is subject to the terms
 * and conditions of the IB API Non-Commercial License or the IB API Commercial License, as applicable. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBApi;

namespace IBDownloader.messages
{
    class PositionMultiEndMessage : Managers.IBMultiMessageData
	{
        private int reqId;
        
        public PositionMultiEndMessage(int reqId)
        {
            RequestId = reqId;
        }

        public int RequestId
        {
            get { return reqId; }
            set { reqId = value; }
        }
    }
}
