using System;

namespace SSCP.Telem.Catalog {
    public struct CatalogID {
        private readonly int id;
        public CatalogID(int id) {
            this.id = id;
        }
        public CatalogID(byte devid, byte varid, byte index, CatalogOp opid) {
            this.id = (devid << 20) | (varid << 12) | (index << 4) | (int)opid;
        }

        private int Get(int minbit, int nbits) {
            return (id >> minbit) % (1 << nbits);
        }

        public int ID {
            get {
                return id;
            }
        }

        public bool IsCat {
            get {
                return Get(28, 1) > 0;
            }
        }

        public byte DevID {
            get {
                return (byte)Get(20, 8);
            }
        }
        public byte VarID {
            get {
                return (byte)Get(12, 8);
            }
        }

        public byte Index {
            get {
                return (byte)Get(4, 8);
            }
        }
        public CatalogOp OpID {
            get {
                return (CatalogOp)Get(0, 4);
            }
        }
    }

    public enum CatalogOp {
        GET_TYPEID = 0,
        GET_LENGTH = 1,
        GET_FLAGS = 2,
        GET_NAME_LENGTH = 3,
        GET_NAME = 4,
        GET_VALUE = 5,
        GET_HASH = 6,
        GET_ANNOUNCE = 7,
        SET_ANNOUNCE = 14,
        SET_VALUE = 15
    }

    public static class RequiredVarIDs {
        /// <summary>
        /// Catalog protocol version
        /// 
        /// Type: uint32
        /// </summary>
        public const byte VID_CATALOG_VERSION = 0;
        /// <summary>
        /// Catalog hash. This is separate from the
        /// software hash so that software updates on a
        /// given board can be detected, and further
        /// distinguished by whether or not they 
        /// change the board's catalog.
        /// 
        /// Type: uint32
        /// </summary>
        public const byte VID_CATALOG_HASH = 1;
        /// <summary>
        /// The set of all variable ids in this board's catalog.
        /// Each one can be queried for type, name, etc, and 
        /// ultimately for its latest value.
        /// 
        /// Type: uint32[]
        /// </summary>
        public const byte VID_VIDS_IN_USE = 2;
        /// <summary>
        /// Board name, ASCII, not null-terminated.
        /// 
        /// Type: char[]
        /// </summary>
        public const byte VID_BOARD_NAME = 3;
        /// <summary>
        /// A hash of all code flashed onto the board.
        /// Useful for tracking revisions.
        /// </summary>
        public const byte VID_SOFTWARE_HASH = 13;
        public const byte VID_HARDWARE_REVISION = 14;
        public const byte VID_SERIAL_NUMBER = 15;
        public const byte VID_SYSTEM_TIME = 16;
        public const byte VID_REBOOT_FLAGS = 17;
        public const byte VID_REBOOT_STRING = 18;
        public const byte VID_SAVE_PARAMS = 19;

    }
}