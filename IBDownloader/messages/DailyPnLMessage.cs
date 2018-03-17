using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBDownloader.messages
{
    class PnLMessage : Managers.IBMultiMessageData
	{
        public int RequestId { get; private set; }
        public double DailyPnL { get; private set; }
        public double UnrealizedPnL { get; private set; }
        public double RealizedPnL { get; private set; }

        public PnLMessage(int reqId, double dailyPnL, double unrealizedPnL, double realizedPnL)
        {
			RequestId = reqId;
            DailyPnL = dailyPnL;
            UnrealizedPnL = unrealizedPnL;
            RealizedPnL = realizedPnL;
        }
    }
}
