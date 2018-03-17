using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBDownloader.messages
{
    class HistoricalTickLastEndMessage : Managers.IBMultiMessageData
	{
        public int RequestId { get; private set; }

        public HistoricalTickLastEndMessage(int reqId)
        {
            RequestId = reqId;
        }
    }
}
