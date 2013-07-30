using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SSCP.Telem.Can;

namespace SSCP.Telem.Catalog {
    public class CatalogRepo {
        private Dictionary<int, Device> devices;
        private Dictionary<int, Dictionary<int, Var>> vars;

        private CatalogRepo(){}
        public static CatalogRepo Create(){
            return new CatalogRepo();
        }

        public ICollection<Device> GetDevices() {
            return devices.Values;
        }
        public Device GetDevice(int devid) {
            return devices[devid];
        }
        public ICollection<Var> GetVars(int devid) {
            return vars[devid].Values;
        }
        public Var GetVar(int devid, int varid) {
            return vars[devid][varid];
        }

        public void PostCatalogMessage(CanMessage canMessage) {
            // ignore request packets, only response packets modify the catalog
            if(canMessage.dlc == 0){
                return;
           } 

            // we've got at least one response -> the board (and var) exist
            CatalogID id = new CatalogID(canMessage.id);
            if (!devices.ContainsKey(id.DevID)) {
                devices[id.DevID] = new Device();
                vars[id.DevID] = new Dictionary<int,Var>();
            }
            if (!vars[id.DevID].ContainsKey(id.VarID)) {
                vars[id.DevID][id.VarID] = new Var();
            }

            //...and we've learned either data or metadata about that var
            Var var = vars[id.DevID][id.VarID];
            switch (id.OpID) {
                case CatalogOp.GET_NAME_LENGTH:
                    CatAssert(id.Index == 0 && canMessage.dlc == 4, "GET_NAME_LENGTH");
                    var.Length = (int)canMessage.data;
                    break;
                case CatalogOp.GET_NAME:
                    char[] newName = UpdateVar(var.Name.ToCharArray(), canMessage);
                    var.Name = new string(newName);
                    break;
            }
        }

        private T[] UpdateVar<T>(T[] oldVar, CanMessage canMessage){
            CatalogID id = new CatalogID(canMessage.id);
            var elemType = typeof(T);
            var elemSize = Marshal.SizeOf(elemType);
            T[] newVar;
            unsafe {
                int numElems = canMessage.dlc / elemSize;
                CatAssert(canMessage.dlc % elemSize == 0, "Message length is not a multiple of element size");
                int newLen = Math.Max(oldVar.Length, id.Index + numElems);
                if (newLen > oldVar.Length) {
                    Debug.WriteLine("Length didn't precede message: " + id);
                    newVar = new T[newLen];
                    Array.Copy(oldVar, newVar, oldVar.Length);
                } else {
                    newVar = oldVar;
                }

                // Make sure the array won't be moved around by the GC 
                ulong data = canMessage.data;
                byte* dataBytes = (byte*)&data;
                var handle = GCHandle.Alloc(newVar, GCHandleType.Pinned);
                var destination = (byte*)handle.AddrOfPinnedObject().ToPointer();
                for (int i = 0; i < canMessage.dlc; i++) {
                    destination[i + id.Index*elemSize] = dataBytes[i];
                }
                handle.Free();
            }
            return newVar;
        }

        private void CatAssert(bool condition, string message) {
            if (!condition) {
                throw new ArgumentException("Invalid catalog message: "+message);
            }
        }

        public class Device {
            public int DevID { get; private set; }
            public String Name { get; private set; }
        }

        public class Var {
            public int DevID {get; set; }
            public int VarID {get; set; }
            public Type Type {get; set; }
            public int Length {get; set; }
            public String Name {get; set; }
        }

        public enum Type {
            Uint8, Uint16, Uint32, Uint64,
            Int8, Int16, Int32, Int64,
            Char, Float, Double, Boolean,
            Enum, BitField
        }
    }
}
