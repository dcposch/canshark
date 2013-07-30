using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using SSCP.Telem.Can;

namespace SSCP.Telem.Receivers
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CanIpMessage
    {
        public const int SIZEOF_CANMSG = 36;

        public ulong syncCode;
        public ushort cksum;
        public uint timestamp;
        public ushort pad;
        public CanRxTxMessage packet;

        public const ulong SYNC_CODE =
                (ulong)'S' << (8 * 0) |
                (ulong)'T' << (8 * 1) |
                (ulong)'A' << (8 * 2) |
                (ulong)'N' << (8 * 3) |
                (ulong)'F' << (8 * 4) |
                (ulong)'O' << (8 * 5) |
                (ulong)'R' << (8 * 6) |
                (ulong)'D' << (8 * 7);

        public unsafe bool Validate()
        {
            if (syncCode != SYNC_CODE)
                return false;
            ushort cksum = this.cksum;
            //zero checksum field before computing checksum
            this.cksum = 0;
            bool correct = cksum == ComputeChecksum();
            this.cksum = cksum;
            return correct;
        }

        public unsafe ushort ComputeChecksum()
        {
            fixed (void* data = &this)
            {
                return CanIpUtils.Checksum(data, sizeof(CanMessage));
            }
        }
    }
}
