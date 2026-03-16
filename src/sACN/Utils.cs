
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using sACN.Structs;

namespace sACN
{
    public static class UtilsOld
    {
        
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

    public static class SacnPacketHelper
    {
        // ACN Packet Identifier constant
        public static readonly byte[] E131_ACN_PID = { 0x41, 0x53, 0x43, 0x2d, 0x45, 0x31, 0x2e, 0x31, 0x37, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Deserializes a byte array into a specified struct type, handling network byte order (big-endian) conversion.
        /// </summary>
        public static T DeserializeNetworkToHost<T>(byte[] data) where T : struct
        {
            int structSize = Marshal.SizeOf(typeof(T));
            if (data.Length < structSize)
            {
                throw new ArgumentException($"Byte array is too small to deserialize into {typeof(T).Name}. Expected at least {structSize} bytes, but got {data.Length} bytes.");
            }

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                T rawStruct = (T)Marshal.PtrToStructure(ptr, typeof(T));

                // Perform byte order conversion from Network (Big-Endian) to Host
                if (typeof(T) == typeof(ARootLayer))
                {
                    return (T)(object)((ARootLayer)(object)rawStruct).ToHostOrder();
                }
                else if (typeof(T) == typeof(AFramingLayer))
                {
                    return (T)(object)((AFramingLayer)(object)rawStruct).ToHostOrder();
                }
                else if (typeof(T) == typeof(ADMPLayer))
                {
                    return (T)(object)((ADMPLayer)(object)rawStruct).ToHostOrder();
                }

                return rawStruct; // Return as-is if no specific conversion is needed for T
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
        }

        /// <summary>
        /// Serializes a struct into a byte array. This is a generic helper used by SerializePacket.
        /// </summary>
        private static byte[] SerializeInternal<T>(T obj) where T : struct
        {
            int size = Marshal.SizeOf(obj);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(obj, ptr, false);
                byte[] data = new byte[size];
                Marshal.Copy(ptr, data, 0, size);
                return data;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Serializes an sACN packet layer (Root, Framing, or DMP) into a byte array,
        /// ensuring correct network byte order (big-endian).
        /// </summary>
        /// <param name="packetLayer">An instance of AcnRootLayer, AcnFramingLayer, or AcnDmpLayer.</param>
        /// <returns>A byte array representing the serialized packet layer in network byte order.</returns>
        /// <exception cref="ArgumentException">Thrown if the provided object is not a recognized sACN layer struct.</exception>
        public static byte[] SerializePacket(object packetLayer)
        {
            if (packetLayer is ARootLayer rootLayer)
            {
                return SerializeInternal(rootLayer.ToNetworkOrder());
            }
            else if (packetLayer is AFramingLayer framingLayer)
            {
                return SerializeInternal(framingLayer.ToNetworkOrder());
            }
            else if (packetLayer is ADMPLayer dmpLayer)
            {
                return SerializeInternal(dmpLayer.ToNetworkOrder());
            }
            else
            {
                throw new ArgumentException("Provided object is not a recognized sACN packet layer (AcnRootLayer, AcnFramingLayer, or AcnDmpLayer).");
            }
        }

        /// <summary>
        /// Generates a new standard UUID (GUID) as a 16-byte array.
        /// </summary>
        public static byte[] GenerateNewCid()
        {
            return Guid.NewGuid().ToByteArray();
        }

        /// <summary>
        /// Extracts the Flags (high 4 bits) and Length (low 12 bits) from an sACN flength field.
        /// </summary>
        public static void ExtractFlagsAndLength(ushort flength, out byte flags, out ushort length)
        {
            flags = (byte)(flength >> 12);
            length = (ushort)(flength & 0x0FFF);
        }

        /// <summary>
        /// Combines Flags (high 4 bits) and Length (low 12 bits) into a single UInt16 flength field.
        /// </summary>
        public static ushort CombineFlagsAndLength(byte flags, ushort length)
        {
            if (flags > 0xF)
            {
                throw new ArgumentOutOfRangeException(nameof(flags), "Flags value must be between 0 and 15.");
            }
            if (length > 0xFFF)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length value must be between 0 and 4095.");
            }
            ushort combinedFlength = (ushort)(flags << 12);
            combinedFlength |= length;
            return combinedFlength;
        }
    }

}

