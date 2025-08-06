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
            packet.RootLayer = Deserialize<ARootLayer>(data);
            packet.FramingLayer = Deserialize<AFramingLayer>(data.Skip(38).ToArray());
            packet.DMPLayer = Deserialize<ADMPLayer>(data.Skip(38+77).ToArray());
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
        public ARootLayer RootLayer;
        public AFramingLayer FramingLayer;
        public ADMPLayer DMPLayer;

        private string Identifier = "ASC - E1.17";
        private byte[] Values;
        private int Priority;
        Guid CID;
        
        private int Sequence;
        private string Name;

        public APacket() { }
        public APacket(string Name, int Priority = 100)
        {
            this.RootLayer = new ARootLayer();
            this.FramingLayer = new AFramingLayer();
            this.DMPLayer = new ADMPLayer();
            this.CID = new Guid();
            this.Priority = Priority;
            this.Name = Name;
        }

        public void Split(out ARootLayer RootLayer, out AFramingLayer FramingLayer, out ADMPLayer DMPLayer)
        {
            RootLayer = this.RootLayer;
            FramingLayer = this.FramingLayer;
            DMPLayer = this.DMPLayer;
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

