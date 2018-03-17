using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBDownloader.messages
{
    class HistoricalTickEndMessage : Managers.IBMultiMessageData
	{
        public int RequestId { get; private set; }

        public HistoricalTickEndMessage(int reqId)
        {
            RequestId = reqId;
        }
    }
}
