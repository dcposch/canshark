using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

namespace SSCP.Telem.Receivers
{
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CanRxTxMessage 
		{
			public uint StdId;  /*		Specifies the standard identifier.
																This parameter can be a value between 0 to 0x7FF. */
			
			public uint ExtId;	/*		Specifies the extended identifier.
																This parameter can be a value between 0 to 0x1FFFFFFF. */
			
			public CAN_ID IDE;	/*		Specifies the type of identifier for the message that will be received.
																This parameter can be a value of @ref CAN_identifier_type */
			
			public byte RTR;		/*		Specifies the type of frame for the received message.
																This parameter can be a value of @ref CAN_remote_transmission_request */
			
			public byte DLC;		/*		Specifies the length of the frame that will be received.
																This parameter can be a value between 0 and 8 */
			
			public ulong Data;	/*		Contains the data to be received. It ranges from 0 to 0xFF. */
			
			public byte FMI;		/*		Specifies the index of the filter the message stored in the mailbox passes through.
																This parameter can be a value between 0 to 0xFF */

			public enum CAN_ID : byte { CAN_ID_STD = 0, CAN_ID_EXT = 4 };


            //TODO: update this for Xenith telem (which uses ExtId)
            public int Devid
            {
                get
                {
                    return (int)(StdId >> 6);
                }
            }

            public int Msgid
            {
                get
                {
                    return (int)(StdId & 0x1F);
                }
            }

            public override string ToString()
            {
                return String.Format("{0} stdid {1:x4} (devid 0x{2:x2} msgid 0x{3:x2}) ide {4} rtr {5} fmi {6} dlc {7} data 0x{8:x16}",
                    this.GetType().Name, StdId, Devid, Msgid, IDE, RTR, FMI, DLC, Data);
            }
		}
}
