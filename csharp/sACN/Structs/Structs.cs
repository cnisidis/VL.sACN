using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace sACN.Structs
{
    

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ARootLayer
    {
        public ushort preamble_size;     // Preamble Size (uint16_t)
        public ushort postamble_size;    // Post-amble Size (uint16_t)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] acn_pid;           // ACN Packet Identifier (uint8_t[12])
        public ushort flength;           // Flags (high 4 bits) & Length (low 12 bits) (uint16_t)
        public uint vector;              // Layer Vector (uint32_t)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] cid;               // Component Identifier (UUID) (uint8_t[16])



        public Guid GetCID()
        {
            return new Guid(cid);   
        }

        public string GetIdentifier()
        {
            return Encoding.UTF8.GetString(acn_pid).Replace('\0',' ').TrimEnd();
            
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

        // Helper for endianness conversion (Host to Network)
        public ARootLayer ToNetworkOrder()
        {
            ARootLayer networkOrderLayer = this;
            networkOrderLayer.preamble_size = (ushort)IPAddress.HostToNetworkOrder((short)preamble_size);
            networkOrderLayer.postamble_size = (ushort)IPAddress.HostToNetworkOrder((short)postamble_size);
            networkOrderLayer.flength = (ushort)IPAddress.HostToNetworkOrder((short)flength);
            networkOrderLayer.vector = (uint)IPAddress.HostToNetworkOrder((int)vector);
            // acn_pid and cid are byte arrays, individual bytes don't need endianness swap
            return networkOrderLayer;
        }

        // Helper for endianness conversion (Network to Host)
        public ARootLayer ToHostOrder()
        {
            ARootLayer hostOrderLayer = this;
            hostOrderLayer.preamble_size = (ushort)IPAddress.NetworkToHostOrder((short)preamble_size);
            hostOrderLayer.postamble_size = (ushort)IPAddress.NetworkToHostOrder((short)postamble_size);
            hostOrderLayer.flength = (ushort)IPAddress.NetworkToHostOrder((short)flength);
            hostOrderLayer.vector = (uint)IPAddress.NetworkToHostOrder((int)vector);
            return hostOrderLayer;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct AFramingLayer
    {
        public ushort flength;           // Flags (high 4 bits) & Length (low 12 bits) (uint16_t)
        public uint vector;              // Layer Vector (uint32_t)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] source_name;       // User Assigned Name of Source (UTF-8) (uint8_t[64])
        public byte priority;            // Packet Priority (0-200, default 100) (uint8_t)
        public ushort reserved;          // Reserved (should be always 0) (uint16_t)
        public byte seq_number;          // Sequence Number (uint8_t)
        public byte options;             // Options Flags (uint8_t)
        public ushort universe;          // DMX Universe Number (uint16_t)

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

        public void  Info(out int length, out byte flags )
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

        public AFramingLayer ToNetworkOrder()
        {
            AFramingLayer networkOrderLayer = this;
            networkOrderLayer.flength = (ushort)IPAddress.HostToNetworkOrder((short)flength);
            networkOrderLayer.vector = (uint)IPAddress.HostToNetworkOrder((int)vector);
            networkOrderLayer.reserved = (ushort)IPAddress.HostToNetworkOrder((short)reserved);
            networkOrderLayer.universe = (ushort)IPAddress.HostToNetworkOrder((short)universe);
            return networkOrderLayer;
        }

        // Helper for endianness conversion (Network to Host)
        public AFramingLayer ToHostOrder()
        {
            AFramingLayer hostOrderLayer = this;
            hostOrderLayer.flength = (ushort)IPAddress.NetworkToHostOrder((short)flength);
            hostOrderLayer.vector = (uint)IPAddress.NetworkToHostOrder((int)vector);
            hostOrderLayer.reserved = (ushort)IPAddress.NetworkToHostOrder((short)reserved);
            hostOrderLayer.universe = (ushort)IPAddress.NetworkToHostOrder((short)universe);
            return hostOrderLayer;
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ADMPLayer
    {
        ushort flength;          /* Flags (high 4 bits) / Length (low 12 bits) */
        byte vector;           /* Layer Vector */
        ushort type;             /* Address Type & Data Type */
        ushort first_addr;       /* First Property Address */
        ushort addr_inc;         /* Address Increment */
        ushort prop_val_cnt;     /* Property Value Count (1 + number of slots) */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 513)]
        byte[] prop_val;    /* Property Values (DMX start code + slots data) */


        public Spread<byte> GetDMXValues()
        {
            return prop_val.Take(512).ToSpread();
        }
        
        public int GetValuesCount()
        {
            return prop_val_cnt;
        }

        public int GetStartCode()
        {
            return prop_val[512];
        }

        public void Split(out int Values, out int StartCode)
        {
            Values = prop_val_cnt;
            StartCode = GetStartCode();
        }

        public void Info(out int length, out byte flags)
        {
            flags = (byte)(flength >> 12);

            // Extract low 12 bits for length
            // Use a bitwise AND with a mask (0x0FFF) to isolate the low 12 bits.
            length = (ushort)(flength & 0x0FFF);


        }

        public ADMPLayer ToNetworkOrder()
        {
            ADMPLayer networkOrderLayer = this;
            networkOrderLayer.flength = (ushort)IPAddress.HostToNetworkOrder((short)flength);
            networkOrderLayer.first_addr = (ushort)IPAddress.HostToNetworkOrder((short)first_addr);
            networkOrderLayer.addr_inc = (ushort)IPAddress.HostToNetworkOrder((short)addr_inc);
            networkOrderLayer.prop_val_cnt = (ushort)IPAddress.HostToNetworkOrder((short)prop_val_cnt);
            return networkOrderLayer;
        }

        // Helper for endianness conversion (Network to Host)
        public ADMPLayer ToHostOrder()
        {
            ADMPLayer hostOrderLayer = this;
            hostOrderLayer.flength = (ushort)IPAddress.NetworkToHostOrder((short)flength);
            hostOrderLayer.first_addr = (ushort)IPAddress.NetworkToHostOrder((short)first_addr);
            hostOrderLayer.addr_inc = (ushort)IPAddress.HostToNetworkOrder((short)addr_inc);
            hostOrderLayer.prop_val_cnt = (ushort)IPAddress.NetworkToHostOrder((short)prop_val_cnt);
            return hostOrderLayer;
        }
    }

    
}

