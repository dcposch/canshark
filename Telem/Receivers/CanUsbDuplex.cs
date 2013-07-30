using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;
using SSCP.Telem.Can;

namespace SSCP.Telem.Receivers
{
    public class CanUsbDuplex : CanBus, IDisposable
    {
        public static readonly CanUsbSettings defaultSettings = new CanUsbSettings() { 
            baudRate = 125000, 
            port = "COM1" 
        };

        //settings
        private string _baudRate, _port;

        //state
        private bool _disposing;
        private bool _attached = true;
        private bool _connected;
        private uint _handle;

        //stats
        private int _goodMsgs, _badMsgs, _sentMsgs;
        private DateTime _connectTime;

        //polling
        private Thread _pollMessagesThread;

        public event Action<CanMessage> Receive;

        public CanUsbDuplex() : this(defaultSettings) { }
        public CanUsbDuplex(CanUsbSettings settings)
        {
            ApplySettings(settings);
            StartPolling();
        }

        private void ApplySettings(CanUsbSettings settings) {
            switch (settings.baudRate) {
                case 125000:
                    _baudRate = LAWICEL.CAN_BAUD_125K;
                    break;
                case 500000:
                    _baudRate = LAWICEL.CAN_BAUD_500K;
                    break;
                case 1000000:
                    _baudRate = LAWICEL.CAN_BAUD_1M;
                    break;
                default:
                    throw new ArgumentException("Unsupported CanUSB baud rate: " + settings.baudRate);
            }
            _port = settings.port;
        }

        public void StartPolling()
        {
            _pollMessagesThread = new Thread(new ThreadStart(PollForMessages));
            _pollMessagesThread.Name = "CANUSBClient._pollMessagesThread";
            _pollMessagesThread.IsBackground = true;
            _pollMessagesThread.Priority = ThreadPriority.Normal;
            _pollMessagesThread.Start();
        }

        public bool Disconnect()
        {
            if (_attached && _connected) {
                LAWICEL.canusb_Close(_handle);
                _connected = false;
                return true;
            } else {
                return false;
            }
        }

        public bool IsConnected()
        {
            return _connected;
        }

        public void Connect()
        {
            if (!_attached) {
                throw new Exception("can't Connect() when not attached");
            }
            if (_connected) {
                throw new Exception("can't Connect() when already connected");
            }
            //LAWICEL.canusb_getFirstAdapter
            _handle = LAWICEL.canusb_Open(IntPtr.Zero, _baudRate, 
                LAWICEL.CANUSB_ACCEPTANCE_CODE_ALL, 
                LAWICEL.CANUSB_ACCEPTANCE_MASK_ALL, 
                LAWICEL.CANUSB_FLAG_TIMESTAMP);
            if (_handle != 0) {
                _connected = true;
                _goodMsgs = _badMsgs = 0;
                _connectTime = DateTime.Now;
            } else {
                throw new Exception("canusb_Open() failed, could not open connection");
            }
        }

        private void PollForMessages()
        {
            LAWICEL.CANMsg msg;
            while (true)
            {
                if (_disposing) {
                    break;
                } else if (_connected) {
                    int err = LAWICEL.canusb_Read(_handle, out msg);
                    if (err == LAWICEL.ERROR_CANUSB_OK) {
                        _goodMsgs++;
                        CanMessage message = new CanMessage();
                        message.utc = DateTime.UtcNow;
                        message.data = msg.data;
                        message.dlc = msg.len;
                        message.id = (int)msg.id;
                        message.ide = (msg.flags & LAWICEL.CANMSG_EXTENDED) > 0;
                        message.rtr = (msg.flags & LAWICEL.CANMSG_RTR) > 0;

                        if (Receive != null) {
                            Receive(message);
                        } else {
                            Debug.WriteLine("no listeners, dropping can frame");
                        }
                    } else {
                        _badMsgs++;
                        Thread.Sleep(0);
                    }
                } else {
                    Thread.Sleep(500);
                }
            }
        }

        public void Send(CanMessage message) {
            LAWICEL.CANMsg msg = new LAWICEL.CANMsg();
            msg.data = message.data;
            msg.flags = (byte)(
                (message.ide ? LAWICEL.CANMSG_EXTENDED : 0) |
                (message.rtr ? LAWICEL.CANMSG_RTR : 0));
            msg.id = (uint)message.id;
            msg.len = (byte)message.dlc;
            _sentMsgs++;
            LAWICEL.canusb_Write(_handle, ref msg);
        }
        
        public void Dispose()
        {
            _disposing = true;
            _pollMessagesThread.Interrupt();
            _pollMessagesThread.Join();
        }

        /*void PollForDevices()
        {
            while (true)
            {
                if (_disposing) break;

                bool deviceAttached = true; // USBHelper.checkForDevice();
                if (_attached && !deviceAttached)
                {
                    if (_connected)
                        LAWICEL.canusb_Close(_handle);
                    _connected = false;
                    _attached = false;
                    if (DeviceDetached != null)
                        DeviceDetached.Invoke(this, new EventArgs());
                }
                else if (!_connected && deviceAttached)
                {
                    _attached = true;
                    if (DeviceAttached != null)
                        DeviceAttached.Invoke(this, new EventArgs());
                }

                Thread.Sleep(100);
            }
        }



        private class USBHelper : Win32Usb
        {
            public static bool checkForDevice()
            {
                string strSearch = "vid_0403+pid_ffa8";
                Guid guid = HIDGuid;
                IntPtr hInfoSet = SetupDiGetClassDevs(ref guid, null, IntPtr.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);	// this gets a list of all HID devices currently connected to the computer (InfoSet)
                try
                {
                    Win32Usb.DeviceInterfaceData oInterface = new Win32Usb.DeviceInterfaceData();	// build up a device interface data block
                    oInterface.Size = Marshal.SizeOf(oInterface);
                    // Now iterate through the InfoSet memory block assigned within Windows in the call to SetupDiGetClassDevs
                    // to get device details for each device connected
                    int nIndex = 0;
                    while (SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref guid, (uint)nIndex, ref oInterface))	// this gets the device interface information for a device at index 'nIndex' in the memory block
                    {
                        string strDevicePath = GetDevicePath(hInfoSet, ref oInterface);	// get the device path (see helper method 'GetDevicePath')
                        if (strDevicePath.IndexOf(strSearch) >= 0)	// do a string search, if we find the VID/PID string then we found our device!
                            return true;
                        nIndex++;
                    }
                }
                catch (Exception)
                {
                }
                return false;
            }
            private static string GetDevicePath(IntPtr hInfoSet, ref Win32Usb.DeviceInterfaceData oInterface)
            {
                uint nRequiredSize = 0;
                // Get the device interface details
                if (!SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, IntPtr.Zero, 0, ref nRequiredSize, IntPtr.Zero))
                {
                    DeviceInterfaceDetailData oDetail = new DeviceInterfaceDetailData();
                    oDetail.Size = 5;	// hardcoded to 5! Sorry, but this works and trying more future proof versions by setting the size to the struct sizeof failed miserably. If you manage to sort it, mail me! Thx
                    if (SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, ref oDetail, nRequiredSize, ref nRequiredSize, IntPtr.Zero))
                    {
                        return oDetail.DevicePath;
                    }
                }
                return null;
            }
        }*/
    }
}
