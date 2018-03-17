using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBDownloader.messages
{
    class HeadTimestampMessage : Managers.IBMultiMessageData
	{
        public int RequestId { get; private set; }
        public string HeadTimestamp { get; private set; }

        public HeadTimestampMessage(int reqId, string headTimestamp)
        {
            this.RequestId = reqId;
            this.HeadTimestamp = headTimestamp;
        }
    }
}
