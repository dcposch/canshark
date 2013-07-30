using System;
using System.Threading;
using System.Diagnostics;
using SSCP.Telem.Can;

namespace SSCP.Telem.Catalog {
    public class CatalogClient {
        private DateTime _timeLastReply;
        private CanBus _canBus;

        public CatalogClient(CanBus canBus) {
            _canBus = canBus;
            _canBus.Receive += new Action<CanMessage>(canBus_OnReceive);
        }

        void canBus_OnReceive(CanMessage msg) {
            CatalogID id = new CatalogID(msg.id);
            Debug.WriteLine("Got message! " + msg + " CatalogID " + id);
        }

        private void SendControl(CatalogID catid) {
            CanMessage message = new CanMessage();
            message.id = catid.ID;
            message.ide = true;
            message.rtr = false;
            message.dlc = 0;
            message.data = 0;
            _canBus.Send(message);
        }

        private void StartRead(byte devid, byte vid) {
            SendControl(new CatalogID(devid, vid, 0, CatalogOp.GET_LENGTH));
            SendControl(new CatalogID(devid, vid, 0, CatalogOp.GET_TYPEID));
        }

        public void RequestCatalog() {
            Debug.Assert(_canBus.IsConnected(), "Can't request catalog while not connected");

            // request the catalog
            for(int devid = 0; devid < 256; devid++){
                Debug.WriteLine("Requesting dev " + devid);
                StartRead((byte)devid, RequiredVarIDs.VID_BOARD_NAME);
                Thread.Sleep(10);
            }
        }
    }
}