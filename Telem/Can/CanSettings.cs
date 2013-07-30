using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace SSCP.Telem.Can
{
    public class CanSettings
    {
        public string TelemLogPath = "../../../../telem.log";
        public CanUsbSettings CanUsbSettings = new CanUsbSettings();
        public CanIPSettings CanIPSettings = new CanIPSettings();
    }

    public class CanUsbSettings
    {
        public String port = "COM1";
        public int baudRate = 125000;
    }

    public class CanIPSettings
    {
        public IPAddress MulticastIP = new IPAddress(new byte[] { 224, 0, 0, 112 });
        public IPAddress RemoteIP = new IPAddress(new byte[] { 192, 168, 1, 22 });
        public int LocalPort = 2000;
        public int RemotePort = 2000;

        public IPEndPoint GetRemoteEndpoint(){
            return new IPEndPoint(RemoteIP, RemotePort);
        }
    }
}
