﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBDownloader.messages
{
    class HistoricalTickBidAskEndMessage
    {
        public int ReqId { get; private set; }

        public HistoricalTickBidAskEndMessage(int reqId)
        {
            ReqId = reqId;
        }
    }
}
