using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBDownloader.messages
{
    class SoftDollarTiersMessage : Managers.IBMultiMessageData
	{
        public int RequestId { get; private set; }
        public IBApi.SoftDollarTier[] Tiers { get; private set; }

        public SoftDollarTiersMessage(int reqId, IBApi.SoftDollarTier[] tiers)
        {
            this.RequestId = reqId;
            this.Tiers = tiers;
        }
    }
}
