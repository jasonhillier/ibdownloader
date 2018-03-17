using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IBApi;

namespace IBDownloader.messages
{
    class TickByTickMidPointMessage : Managers.IBMultiMessageData
	{
        public int RequestId { get; private set; }
        public long Time { get; private set; }
        public double MidPoint { get; private set; }

        public TickByTickMidPointMessage(int reqId, long time, double midPoint)
        {
            RequestId = reqId;
            Time = time;
            MidPoint = midPoint;
        }
    }
}
