using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sACN.Utils
{
    public static class Constants
    {
        public const int  PORT = 5568;
        public const int MAXUNIVERSE = 63999;
        public const int MINUNIVERSE = 1;
        public const int MINSLOTS = 1;
        public const int MAXSLOTS = 512;
        const byte E131_DEFAULT_PRIORITY = 0x64;

        /* E1.31 Private Constants */
        // ACN Packet Identifier constant (for context)
        public const ushort _E131_PREAMBLE_SIZE = 0x0010;
        public const ushort _E131_POSTAMBLE_SIZE = 0x0000;
        public static readonly byte[] _E131_ACN_PID = { 0x41, 0x53, 0x43, 0x2d, 0x45, 0x31, 0x2e, 0x31, 0x37, 0x00, 0x00, 0x00 };
        const uint _E131_ROOT_VECTOR = 0x00000004;
        const uint _E131_FRAME_VECTOR = 0x00000002;
        const byte _E131_DMP_VECTOR = 0x02;
        const byte _E131_DMP_TYPE = 0xa1;
        const ushort _E131_DMP_FIRST_ADDR = 0x0000;
        const ushort _E131_DMP_ADDR_INC = 0x0001;

       
        
        // --- New Constants for Preamble Calculation ---
        // Assuming the LENGTH_CONFIG bit is the lowest bit (0x0001) of the PREAMBLE_CONFIG value.
        public const ushort LENGTH_CONFIG_MASK = 0x0001;
        public const ushort MAX_PREAMBLE_BYTES = 255;
    }
}
