using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IBDownloader.messages
{
    class SecurityDefinitionOptionParameterEndMessage : Managers.IBMultiMessageData
	{
        private int reqId;

        public SecurityDefinitionOptionParameterEndMessage(int reqId)
        {
            this.reqId = reqId;
        }

		public int RequestId
		{
			get
			{
				return reqId;
			}
		}
    }
}
