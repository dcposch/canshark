using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace SSCP.Telem.Can
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct CanMessage 
	{
        public static readonly int SIZEOF_CANMSG = 28;

        public DateTime utc;

        public int id;
        public int dlc;
        public bool ide;
        public bool rtr;
		public ulong data;

        public unsafe static Nullable<CanMessage> Read(Stream stream)
        {
            Debug.Assert(sizeof(CanMessage) == CanMessage.SIZEOF_CANMSG);
            byte[] buffer = new byte[CanMessage.SIZEOF_CANMSG];
            int nread = stream.Read(buffer, 0, CanMessage.SIZEOF_CANMSG);
            if (nread == 0) {
                return null;
            }
            if (nread != CanMessage.SIZEOF_CANMSG) {
                throw new Exception("unable to read CanMessage from stream...");
            }
            CanMessage ret;
            fixed (byte* p = &buffer[0])
            {
                ret = *((CanMessage*)p);
            }
            return ret;
        }

        public override string ToString()
        {
            return String.Format("{0} id {1:x4} ide {2} rtr {3} dlc {4} data 0x{5:x16}",
                this.GetType().Name, id, ide, rtr, dlc, data);
        }
	}
}
