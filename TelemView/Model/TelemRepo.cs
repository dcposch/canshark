using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.IO.MemoryMappedFiles;
using SSCP.Telem.Can;
using SSCP.Telem.Catalog;

namespace SSCP.Telem.CanShark  {
    public class CanMessageRepo
    {
        private FileStream logFile;
        private Dictionary<int, List<int>> devidIndex = new Dictionary<int, List<int>>();
        private Dictionary<int, List<int>> varidIndex = new Dictionary<int, List<int>>();

        public int Count { get; private set; }

        private CanMessageRepo(String path) {
            logFile = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public static CanMessageRepo Create(String path) {
            return new CanMessageRepo(path);
        }

        private void BuildIndex()
        {
            logFile.Seek(0, SeekOrigin.Begin);
            varidIndex.Clear();
            devidIndex.Clear();
            Count = 0;
            Nullable<CanMessage> msg;
            while ((msg = ReadMsg()) != null)
            {
                var message = msg.Value;
                validate(message);
                appendToIndex(message);
            }
        }

        private void validate(CanMessage message) {
            // TODO: be more flexible...
            Debug.Assert(message.ide);
            CatalogID id = new CatalogID(message.id);
            Debug.Assert(id.IsCat);
        }

        private void appendToIndex(CanMessage message)
        {
            //add to devid index
            CatalogID id = new CatalogID(message.id);
            if (!devidIndex.ContainsKey(id.DevID))
            {
                devidIndex.Add(id.DevID, new List<int>());
            }
            devidIndex[id.DevID].Add(Count);

            //add to varid index
            int key = GetKey(id.DevID, id.VarID);
            if (!varidIndex.ContainsKey(key))
            {
                varidIndex.Add(key, new List<int>());
            }
            varidIndex[key].Add(Count);

            //add to global index
            Count++;
        }

        /// <summary>
        /// Gets a list of the message indicies that have this devid. Really fast lookup.
        /// 
        /// See this[int ix] to get a message by index.
        /// </summary>
        public List<int> GetIndices(int devid)
        {
            return devidIndex.ContainsKey(devid) ? devidIndex[devid] : null;
        }

        private int GetKey(int devid, int varid)
        {
            Debug.Assert(0 <= devid && devid < 256);
            Debug.Assert(0 <= varid && varid < 256);
            return (devid << 8) | varid;
        }
        
        /// <summary>
        /// Gets a list of the message indicies that have this devid+varid. Really fast lookup.
        /// 
        /// See this[int ix] to get a message by index.
        /// </summary>
        public List<int> GetIndices(int devid, int varid)
        {
            int key = GetKey(devid, varid);
            return varidIndex.ContainsKey(key) ? varidIndex[key] : null;
        }

        public CanMessage this[int ix]
        {
            get
            {
                unsafe
                {
                    long offset = ix * CanMessage.SIZEOF_CANMSG;
                    logFile.Seek(offset, SeekOrigin.Begin);
                    return ReadMsg().Value;
                }
            }
        }

        private Nullable<CanMessage> ReadMsg()
        {
            return CanMessage.Read(logFile);
        }

        public void Append(CanMessage msg)
        {
            //serialize 
            logFile.Seek(0, SeekOrigin.End);
            byte[] bytes;
            unsafe{
                bytes = ByteUtils.ToByteArray((byte*)&msg, CanMessage.SIZEOF_CANMSG);
            }

            //write
            logFile.Write(bytes, 0, bytes.Length);

            //add to index
            appendToIndex(msg);
        }
    }
}
