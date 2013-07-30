using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace SSCP.Telem.Can
{
    // Very simple receiver interface.
    public interface Source<T>
    {
        event Action<T> Receive;
    }
}
