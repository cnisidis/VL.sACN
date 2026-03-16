using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace sACN.Structs
{
    // --- ACN Root Layer: 38 bytes ---
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Root
    {
        public UInt16 preamble_size;     // Preamble Size (uint16_t)
        public UInt16 postamble_size;    // Post-amble Size (uint16_t)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] acn_pid;           // ACN Packet Identifier (uint8_t[12])
        public UInt16 flength;           // Flags (high 4 bits) & Length (low 12 bits) (uint16_t)
        public UInt32 vector;              // Layer Vector (uint32_t)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] cid;               // Component Identifier (UUID) (uint8_t[16])

        // Helper for endianness conversion (Host to Network)
        public Root ToNetworkOrder()
        {
            Root networkOrderLayer = this;
            networkOrderLayer.preamble_size = (ushort)IPAddress.HostToNetworkOrder((short)this.preamble_size);
            networkOrderLayer.postamble_size = (ushort)IPAddress.HostToNetworkOrder((short)this.postamble_size);
            networkOrderLayer.flength = (ushort)IPAddress.HostToNetworkOrder((short)this.flength);
            networkOrderLayer.vector = (uint)IPAddress.HostToNetworkOrder((int)this.vector);
            // acn_pid and cid are byte arrays, individual bytes don't need endianness swap
            return networkOrderLayer;
        }

        // Helper for endianness conversion (Network to Host)
        public Root ToHostOrder()
        {
            Root hostOrderLayer = this;
            hostOrderLayer.preamble_size = (ushort)IPAddress.NetworkToHostOrder((short)this.preamble_size);
            hostOrderLayer.postamble_size = (ushort)IPAddress.NetworkToHostOrder((short)this.postamble_size);
            hostOrderLayer.flength = (ushort)IPAddress.NetworkToHostOrder((short)this.flength);
            hostOrderLayer.vector = (uint)IPAddress.NetworkToHostOrder((int)this.vector);
            return hostOrderLayer;
        }

        public Guid GetCID()
        {
            return new Guid(cid);
        }

        public string GetIdentifier()
        {
            return Encoding.UTF8.GetString(acn_pid).Replace('\0', ' ').TrimEnd();

        }

        public void Split(out string Identifier, out Guid CID)
        {
            Identifier = GetIdentifier();
            CID = GetCID();
        }
        public void Info(out int length, out byte flags)
        {
            flags = (byte)(flength >> 12);

            // Extract low 12 bits for length
            // Use a bitwise AND with a mask (0x0FFF) to isolate the low 12 bits.
            length = (ushort)(flength & 0x0FFF);


        }
    }
}
