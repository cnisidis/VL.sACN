using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace sACN
{
    public static class Utils
    {
        public const int PORT = 5568;
        public const int MAXUNIVERSE = 63999;
        public const int MINUNIVERSE = 1;
        public const int MINSLOTS = 1;
        public const int MAXSLOTS = 512;
        const byte E131_DEFAULT_PRIORITY = 0x64;

        /* E1.31 Private Constants */
        public const ushort _E131_PREAMBLE_SIZE = 0x0010;
        public const ushort _E131_POSTAMBLE_SIZE = 0x0000;
        public static readonly byte[] _E131_ACN_PID = { 0x41, 0x53, 0x43, 0x2d, 0x45, 0x31, 0x2e, 0x31, 0x37, 0x00, 0x00, 0x00 };
        const uint _E131_ROOT_VECTOR = 0x00000004;
        const uint _E131_FRAME_VECTOR = 0x00000002;
        const byte _E131_DMP_VECTOR = 0x02;
        const byte _E131_DMP_TYPE = 0xa1;
        const ushort _E131_DMP_FIRST_ADDR = 0x0000;
        const ushort _E131_DMP_ADDR_INC = 0x0001;
        public static T Deserialize<T>(byte[] data) where T : struct
        {
            // Allocate a GCHandle to "pin" the byte array in memory,
            // preventing the garbage collector from moving it.
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                // Get the address of the pinned data.
                IntPtr ptr = handle.AddrOfPinnedObject();

                // Use Marshal.PtrToStructure to copy the data from the unmanaged memory
                // location to a new instance of the specified struct type.
                // The method automatically handles the layout based on the StructLayout attribute.
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                // Always free the GCHandle to avoid memory leaks.
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        // You can also create a specific function for your RootLayer.
        public static ARootLayer DeserializeRootLayer(byte[] data)
        {
            return Deserialize<ARootLayer>(data);
        }

        public static AFramingLayer DeserializeFramingLayer(byte[] data)
        {
            return Deserialize<AFramingLayer>(data);
        }

        public static ADMPLayer DeserializeDMPLayer(byte[] data)
        {
            return Deserialize<ADMPLayer>(data);
        }

        public static APacket DeserializeAPacket(byte[] data)
        {
            var packet = new APacket();
            packet.Root = Deserialize<ARootLayer>(data);
            packet.Frame = Deserialize<AFramingLayer>(data.Skip(38).ToArray());
            packet.DMP = Deserialize<ADMPLayer>(data.Skip(38+77).ToArray());
            return packet;
        }


        public static ushort CombineFlagsAndLength(byte flags, ushort length)
        {
            if (flags > 0xF) // 4 bits can hold values 0-15 (0x0 to 0xF)
            {
                throw new ArgumentOutOfRangeException(nameof(flags), "Flags value must be between 0 and 15.");
            }
            if (length > 0xFFF) // 12 bits can hold values 0-4095 (0x0 to 0xFFF)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length value must be between 0 and 4095.");
            }

            // Shift flags to the high 4 bits position
            ushort combinedFlength = (ushort)(flags << 12);

            // Combine with length using bitwise OR
            combinedFlength |= length;

            return combinedFlength;
        }
    }
    
    public class APacket
    {
        
        public ARootLayer Root;
        public AFramingLayer Frame;
        public ADMPLayer DMP;

        private string Identifier = "ASC - E1.17";
        private byte[] Values;
        private int Priority;
        Guid CID;
        
        private int Sequence;
        private string Name;

        public APacket() { }
        public APacket(string Name, int Priority = 100)
        {
            this.Root = new ARootLayer();
            this.Frame = new AFramingLayer();
            this.DMP = new ADMPLayer();
            this.CID = new Guid();
            this.Priority = Priority;
            this.Name = Name;
        }

        public void Split(out ARootLayer RootLayer, out AFramingLayer FramingLayer, out ADMPLayer DMPLayer)
        {
            RootLayer = this.Root;
            FramingLayer = this.Frame;
            DMPLayer = this.DMP;
        }


        public void SetValues(Spread<byte> Values)
        {
            this.Values = Values.ToArray();
        }
        
        public void SetPriority(int Value)
        {
            this.Priority = Value;
        }

        public void SetName(string Name)
        {
            this.Name = Name;
        }

        public void Update()
        {
            Sequence ++;
            Sequence = Sequence % 256;
        }

        public void GetSequence(out int Seq)
        {
            Seq = this.Sequence;
        }
        
    }



}

