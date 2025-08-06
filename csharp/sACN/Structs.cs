using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace sACN
{
    public enum FramingOptionsType
    {
        E131_OPT_TERMINATED = 6,
        E131_OPT_PREVIEW = 7,

    }

    public enum ErrorType
    {
        E131_ERR_NONE,
        E131_ERR_NULLPTR,
        E131_ERR_PREAMBLE_SIZE,
        E131_ERR_POSTAMBLE_SIZE,
        E131_ERR_ACN_PID,
        E131_ERR_VECTOR_ROOT,
        E131_ERR_VECTOR_FRAME,
        E131_ERR_VECTOR_DMP,
        E131_ERR_TYPE_DMP,
        E131_ERR_FIRST_ADDR_DMP,
        E131_ERR_ADDR_INC_DMP,

    }

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
            return (int)seq_number;
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
            Universe = (int)this.universe;
            SeqNumber = (int)this.seq_number;   
            Type = (FramingOptionsType)options;
            Priority = (int)this.priority;
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
            return (int)prop_val[512];
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
    }

    
}

