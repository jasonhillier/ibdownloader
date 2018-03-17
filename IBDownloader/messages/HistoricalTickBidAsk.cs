using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBDownloader.messages
{
    class HistoricalTickBidAskMessage : Managers.IBMultiMessageData
	{
        public int RequestId { get; set; }
        public long Time { get; set; }
        public int Mask { get; set; }
        public double PriceBid { get; set; }
        public double PriceAsk { get; set; }
        public long SizeBid { get; set; }
        public long SizeAsk { get; set; }

        public HistoricalTickBidAskMessage(int reqId, long time, int mask, double priceBid, double priceAsk, long sizeBid, long sizeAsk)
        {
            RequestId = reqId;
            Time = time;
            Mask = mask;
            PriceBid = priceBid;
            PriceAsk = priceAsk;
            SizeBid = sizeBid;
            SizeAsk = sizeAsk;
        }
    }
}
