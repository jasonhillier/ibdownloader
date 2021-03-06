using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBApi;

namespace IBDownloader.messages
{
    class TickByTickBidAskMessage : Managers.IBMultiMessageData
	{
        public int RequestId { get; private set; }
        public long Time { get; private set; }
        public double BidPrice { get; private set; }
        public double AskPrice { get; private set; }
        public long BidSize { get; private set; }
        public long AskSize { get; private set; }
        public TickAttrib Attribs { get; private set; }

        public TickByTickBidAskMessage(int reqId, long time, double bidPrice, double askPrice, long bidSize, long askSize, TickAttrib attribs)
        {
            RequestId = reqId;
            Time = time;
            BidPrice = bidPrice;
            AskPrice = askPrice;
            BidSize = bidSize;
            AskSize = askSize;
            Attribs = attribs;
        }
    }
}
