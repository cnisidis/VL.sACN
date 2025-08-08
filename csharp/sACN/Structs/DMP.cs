using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace sACN.Structs
{
    // --- Device Management Protocol (DMP) Layer: 523 bytes ---
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DMP
    {
        public UInt16 flength;          /* Flags (high 4 bits) / Length (low 12 bits) */
        public byte vector;           /* Layer Vector */
        public byte type;             /* Address Type & Data Type */
        public UInt16 first_addr;       /* First Property Address */
        public UInt16 addr_inc;         /* Address Increment */
        public UInt16 prop_val_cnt;     /* Property Value Count (1 + number of slots) */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 513)]
        public byte[] prop_val;    /* Property Values (DMX start code + slots data) */


        public Spread<byte> GetDMXValues()
        {
            return prop_val.Skip(1).ToSpread();
        }

        public int GetValuesCount()
        {
            return prop_val_cnt-1;
        }

        public int GetStartCode()
        {
            return prop_val[0];
        }

        public void Split(out int Values, out int StartCode)
        {
            Values = prop_val_cnt-1;
            StartCode = GetStartCode();
        }

        public void Info(out int length, out byte flags)
        {
            flags = (byte)(flength >> 12);

            // Extract low 12 bits for length
            // Use a bitwise AND with a mask (0x0FFF) to isolate the low 12 bits.
            length = (ushort)(flength & 0x0FFF);


        }

        public DMP ToNetworkOrder()
        {
            DMP networkOrderLayer = this;
            networkOrderLayer.flength = (ushort)IPAddress.HostToNetworkOrder((short)flength);
            networkOrderLayer.first_addr = (ushort)IPAddress.HostToNetworkOrder((short)first_addr);
            networkOrderLayer.addr_inc = (ushort)IPAddress.HostToNetworkOrder((short)addr_inc);
            networkOrderLayer.prop_val_cnt = (ushort)IPAddress.HostToNetworkOrder((short)prop_val_cnt);
            return networkOrderLayer;
        }

        // Helper for endianness conversion (Network to Host)
        public DMP ToHostOrder()
        {
            DMP hostOrderLayer = this;
            hostOrderLayer.flength = (ushort)IPAddress.NetworkToHostOrder((short)flength);
            hostOrderLayer.first_addr = (ushort)IPAddress.NetworkToHostOrder((short)first_addr);
            hostOrderLayer.addr_inc = (ushort)IPAddress.HostToNetworkOrder((short)addr_inc);
            hostOrderLayer.prop_val_cnt = (ushort)IPAddress.NetworkToHostOrder((short)prop_val_cnt);
            return hostOrderLayer;
        }
    }
}
