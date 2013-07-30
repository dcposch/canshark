using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SSCP.Telem.Can;
using SSCP.Telem.Catalog;

namespace SSCP.Telem.CanShark {
    public class TelemFormModel {
        //user input current settings (+defaults)
        private CanSettings settings = new CanSettings();
        private CanMessageRepo canRepo;
        private CatalogRepo catalogRepo;
        private CanBus canBus;

        //view model
        private int filterDevid = -1, filterMsgid = -1;
        private List<int> displayedMsgIndexes = null; //null for all msgs, else a list of msg ixs
        private Predicate<CanMessage> filter = new Predicate<CanMessage>(msg => true);
        private Source<CanMessage> source;

        public TelemFormModel() {
            canRepo = CanMessageRepo.Create(settings.TelemLogPath);
            catalogRepo = CatalogRepo.Create();
        }

        public event Action<CanMessage> NewMessage;
        public event Action<CanValue> NewValue;

        public CanMessageRepo CanRepo {
            get { return canRepo; }
        }
        public CatalogRepo CatalogRepo {
            get { return catalogRepo; }
        }
        public CanBus CanBus {
            get { return canBus; }
            set {
                this.canBus = value;
                this.canBus.Receive += new Action<CanMessage>(canBus_OnReceive);
            }
        }

        public CanMessage? GetCanMessageRow(int rowIndex) {
            int ix = -1;
            if (displayedMsgIndexes != null && rowIndex < displayedMsgIndexes.Count) {
                ix = displayedMsgIndexes[rowIndex];
            } else if (rowIndex < canRepo.Count) {
                ix = rowIndex;
            }
            if (ix < 0) {
                Debug.WriteLine("cell val out of bounds...");
                return null;
            }
            return canRepo[ix];
        }

        private void canBus_OnReceive(CanMessage message)
        {
            Debug.WriteLine("new message! "+message.ToString());
            canRepo.Append(message);
            catalogRepo.PostCatalogMessage(message);
            //TODO: CanValue val = catalogRepo....
        }

        private bool TryParseFilter(string expression, out int devid, out int msgid) {
            devid = -1;
            msgid = -1;

            if (expression == null) {
                return false;
            }
            string[] parts = Regex.Split(expression.Trim(), @"\s+");
            if (parts.Length > 2) {
                return false;
            }

            if (parts.Length == 0) {
                return true;
            }
            if (!ByteUtils.TryParseHex(parts[0], out devid)) {
                //TODO: lookup by name
                devid = -1;
                return false;
            }

            if (parts.Length == 2 && !ByteUtils.TryParseHex(parts[1], out msgid)) {
                return false;
            }
            return true;
        }

        public Predicate<CanMessage> GetPredicate(string expression) {
            int devid, msgid;
            if (!TryParseFilter(expression, out devid, out msgid)) {
                return null;
            }
            if (devid < 0) {
                return null;
            }
            return new Predicate<CanMessage>(msg => {
                CatalogID id = new CatalogID(msg.id);
                if (id.DevID != devid)
                    return false;
                if (msgid >= 0 && id.VarID != msgid)
                    return false;
                return true;
            });
        }
    }
}
