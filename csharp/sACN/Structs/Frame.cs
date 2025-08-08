using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace sACN.Structs
{
    // --- Framing Layer: 77 bytes ---
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Frame
    {
        public UInt16 flength;           // Flags (high 4 bits) & Length (low 12 bits) (uint16_t)
        public UInt32 vector;              // Layer Vector (uint32_t)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] source_name;       // User Assigned Name of Source (UTF-8) (uint8_t[64])
        public byte priority;            // Packet Priority (0-200, default 100) (uint8_t)
        public UInt16 reserved;          // Reserved (should be always 0) (uint16_t)
        public byte seq_number;          // Sequence Number (uint8_t)
        public byte options;             // Options Flags (uint8_t)
        public UInt16 universe;          // DMX Universe Number (uint16_t)

        public string GetName()
        {
            return Encoding.UTF8.GetString(source_name).Replace('\0', ' ').TrimEnd();
        }

        public int GetSeqNumber()
        {
            return seq_number;
        }

        public int GetUniverse()
        {
            return universe;
        }

        public void SetSeqNumber(int seq)
        {
            this.seq_number = (byte)seq;
        }

        public void Info(out int length, out byte flags)
        {
            flags = (byte)(flength >> 12);

            // Extract low 12 bits for length
            // Use a bitwise AND with a mask (0x0FFF) to isolate the low 12 bits.
            length = (ushort)(flength & 0x0FFF);


        }

        public void Split(out string Name, out int Universe, out int SeqNumber, out FramingOptionsType Type, out int Priority)
        {
            Name = GetName();
            Universe = universe;
            SeqNumber = seq_number;
            Type = (FramingOptionsType)options;
            Priority = priority;
        }

        public Frame ToNetworkOrder()
        {
            Frame networkOrderLayer = this;
            networkOrderLayer.flength = (ushort)IPAddress.HostToNetworkOrder((short)flength);
            networkOrderLayer.vector = (uint)IPAddress.HostToNetworkOrder((int)vector);
            networkOrderLayer.reserved = (ushort)IPAddress.HostToNetworkOrder((short)reserved);
            networkOrderLayer.universe = (ushort)IPAddress.HostToNetworkOrder((short)universe);
            return networkOrderLayer;
        }

        // Helper for endianness conversion (Network to Host)
        public Frame ToHostOrder()
        {
            Frame hostOrderLayer = this;
            hostOrderLayer.flength = (ushort)IPAddress.NetworkToHostOrder((short)flength);
            hostOrderLayer.vector = (uint)IPAddress.NetworkToHostOrder((int)vector);
            hostOrderLayer.reserved = (ushort)IPAddress.NetworkToHostOrder((short)reserved);
            hostOrderLayer.universe = (ushort)IPAddress.NetworkToHostOrder((short)universe);
            return hostOrderLayer;
        }

    }
}
