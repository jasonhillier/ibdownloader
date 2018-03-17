using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBDownloader.messages
{
    class HistoricalTickBidAskEndMessage : Managers.IBMultiMessageData
	{
        public int RequestId { get; private set; }

        public HistoricalTickBidAskEndMessage(int reqId)
        {
            RequestId = reqId;
        }
    }
}
