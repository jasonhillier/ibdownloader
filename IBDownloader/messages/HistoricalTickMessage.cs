using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBDownloader.messages
{
    class HistoricalTickMessage : Managers.IBMultiMessageData
	{
        public int RequestId { get; private set; }
        public long Time { get; private set; }
        public double Price { get; private set; }
        public long Size { get; private set; }

        public HistoricalTickMessage(int reqId, long time, double price, long size)
        {
            RequestId = reqId;
            Time = time;
            Price = price;
            Size = size;
        }
    }
}
