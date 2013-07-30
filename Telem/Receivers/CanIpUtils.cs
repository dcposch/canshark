using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SSCP.Telem.Can;

namespace SSCP.Telem.Receivers
{
    public class CanIpUtils
    {
        /// <summary>
        /// Calculates the 16-bit CRC checksum used in CAN msgs.
        /// </summary>
        public static unsafe ushort Checksum(void* _data, int len)
        {
            byte* data = (byte*)_data;
            uint sum;
            for (sum = 0; len >= 2; data += 2, len -= 2)
                sum += (uint)data[0] << 8 | data[1];
            if (len > 0)
                sum += (uint)data[0] << 8;
            while (sum > 0xffff)
                sum = (uint)(sum >> 16) + (sum & 0xffff);
            sum = ~sum;
            return (sum != 0) ? (ushort)sum : (ushort)0xffff;
        }
        /// <summary>
        /// Reads the stream, looking for an encapsulated CAN packet.
        /// See CanMessage. Starts reading when it sees the magic bytes.
        /// 
        /// Note: does NOT discard messages for bad cksum. Returns null on EOF.
        /// </summary>
        public static Nullable<CanIpMessage> ReadMsg(Stream stream)
        {
            unsafe
            {
                CanIpMessage ret;
                byte* bytep = (byte*)&ret;
                ulong lastBytes = 0;
                int lastByte; int pos = 0;
                while ((lastByte = stream.ReadByte()) >= 0)
                {
                    lastBytes = (lastBytes >> 8) | ((ulong)lastByte << 56);
                    if (lastBytes == CanIpMessage.SYNC_CODE)
                    {
                        *((ulong*)&ret) = lastBytes;
                        pos = 7;
                    }
                    if (pos > 0)
                    {
                        bytep[pos++] = (byte)lastByte;
                    }
                    if (pos == CanMessage.SIZEOF_CANMSG)
                    {
                        break;
                    }
                }
                if (lastByte < 0)
                {
                    return null;
                }
                return ret;
            }
        }

        public static CanMessage ConvertToCanMessage(CanIpMessage msg)
        {
            if (msg.ComputeChecksum() != msg.cksum)
            {
                throw new Exception("invalid checksum: " + msg);
            }
            CanMessage ret = new CanMessage();
            ret.data = msg.packet.Data;
            ret.dlc = msg.packet.DLC;
            ret.id = (int)(msg.packet.ExtId > 0 ? msg.packet.ExtId : msg.packet.StdId);
            ret.ide = msg.packet.ExtId > 0;
            ret.rtr = msg.packet.RTR > 0;
            ret.utc = DateTime.UtcNow; //msg.timestamp is worthless
            return ret;
        }
    }
}
