using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;
using SSCP.Telem.Can;

namespace SSCP.Telem.Receivers
{
    /*public class CanTcpReceiver : Source<CanMessage>
    {
        CanIPSettings settings;
        TcpClient tcpClient;

        public CanTcpReceiver(CanIPSettings settings)
        {
            Debug.Assert(settings != null);
            this.settings = settings;
            tcpClient = new TcpClient();
        }

        public bool IsConnected()
        {
            return tcpClient.Connected;
        }

        public void Connect()
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(settings.GetRemoteEndpoint());
            tcpClient.NoDelay = true;
            if (!tcpClient.Connected)
                throw new Exception("couldn't connect to CAN+TCP stream");
        }

        public bool HasNext()
        {
            return IsConnected();
        }

        public CanMessage Next()
        {
            var msg = CanIpUtils.ReadMsg(tcpClient.GetStream());
            return CanIpUtils.ConvertToCanMessage(msg.Value);
        }
    }*/
}
