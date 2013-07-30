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
    /*public class CanUdpReceiver : Source<CanMessage>
    {
        UdpClient client;
        IPEndPoint endpoint;
        CanIPSettings settings;

        public CanUdpReceiver(CanIPSettings settings)
        {
            Debug.Assert(settings != null);
            this.settings = settings;
        }

        public CanIPSettings CanIPSettings
        {
            get
            {
                return settings;
            }
        }


        public bool HasNext()
        {
            return IsConnected() && client.Available > 0;
        }

        public bool IsConnected()
        {
            return client != null && client.Client.Connected;
        }

        private bool IsInSubnet(UnicastIPAddressInformation intf, IPAddress addr)
        {
            if (intf == null || addr == null || intf.Address == null || intf.IPv4Mask == null || addr.AddressFamily == null)
                return false;

            //must by ipv4
            if (intf.Address.AddressFamily != addr.AddressFamily)
                return false;
            if (addr.AddressFamily != AddressFamily.InterNetwork)
                return false;

            //use prefix + subnet mask to check
            byte[] prefix = intf.Address.GetAddressBytes();
            byte[] mask = intf.IPv4Mask.GetAddressBytes();
            byte[] ip = addr.GetAddressBytes();
            for (int i = 0; i < 4; i++)
                if ((ip[i] & mask[i]) != (prefix[i] & mask[i]))
                    return false;
            return true;
        }

        public void Connect()
        {
            //params
            IPAddress multicastIP = settings.MulticastIP;
            IPAddress remoteIP = settings.RemoteIP;
            int localPort = settings.LocalPort;
            int remotePort = settings.RemotePort;
            IPAddress localIP = null;

            //connect
            try
            {
                client = new UdpClient(localPort, AddressFamily.InterNetwork);
                localIP =
                    (from ni in NetworkInterface.GetAllNetworkInterfaces()
                     where ni.GetIPProperties() != null
                     let props = ni.GetIPProperties()
                     from ipAddress in props.UnicastAddresses
                     where IsInSubnet(ipAddress, remoteIP)
                     select ipAddress.Address).FirstOrDefault();
                if (localIP == null)
                    throw new Exception("can't find a network interface to connect on");
                endpoint = new IPEndPoint(remoteIP, remotePort);
                client.JoinMulticastGroup(multicastIP, localIP);
            }
            catch (Exception e)
            {
                throw new Exception("can't connect to CAN+UDP stream", e);
            }

            //log
            Debug.WriteLine(string.Format("Connected CanWifiUdpReceiver, listening on {0}:{1}, remote endpoint {2}:{3}, multicast group {4}",
                localIP, localPort, remoteIP, remotePort, multicastIP));
        }

        public unsafe CanMessage Next()
        {
            while (true)
            {
                byte[] packet = client.Receive(ref endpoint);
                try
                {
                    fixed (byte* p = &packet[0])
                    {
                        if (packet.Length != sizeof(CanIpMessage))
                        {
                            Debug.WriteLine("dropping incomplete packet...");
                            continue;
                        }
                        CanIpMessage* rawmsg = (CanIpMessage*)p;
                        if (!rawmsg->Validate())
                        {
                            Debug.WriteLine("dropping bad packet...");
                            continue;
                        }
                        return CanIpUtils.ConvertToCanMessage(*rawmsg);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }
    }*/
}
