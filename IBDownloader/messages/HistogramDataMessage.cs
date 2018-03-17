using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBDownloader.messages
{
    class HistogramDataMessage : Managers.IBMultiMessageData
	{
        public int RequestId { get; private set; }
        public IBApi.HistogramEntry[] Data { get; private set; }

        public HistogramDataMessage(int reqId, IBApi.HistogramEntry[] data)
        {
            RequestId = reqId;
            Data = data;
        }
    }
}
