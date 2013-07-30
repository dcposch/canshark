using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSCP.Telem.Can {
    public interface CanBus : Source<CanMessage>, Sink<CanMessage> {
        bool IsConnected();
        /// <summary>
        /// Attempts to connect.
        /// </summary>
        void Connect();
        /// <summary>
        /// Indicates whether data is still available. For file streams or similar, this is false on EOF/end. For network receivers, this is always true.
        /// </summary>
    }
}
