
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using sACN.Structs;

namespace sACN.Utils
{
    public static class Serializers
    {
        // ACN Packet Identifier constant
        public static readonly byte[] E131_ACN_PID = { 0x41, 0x53, 0x43, 0x2d, 0x45, 0x31, 0x2e, 0x31, 0x37, 0x00, 0x00, 0x00 };

        /// <summary>
        /// Deserializes a byte array into a specified struct type, handling network byte order (big-endian) conversion.
        /// </summary>
        private static T DeserializeInternal<T>(byte[] data) where T : struct
        {
            int structSize = Marshal.SizeOf(typeof(T));
            if (data.Length < structSize)
            {
                throw new ArgumentException($"Byte array is too small to deserialize into {typeof(T).Name}.");
            }

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
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
        /// 
        public static Packet DeserializePacket(byte[] data)
        {
            // Define the sizes and offsets of each layer
            int rootSize = Marshal.SizeOf<Root>();
            int frameSize = Marshal.SizeOf<Frame>();
            int dmpSize = Marshal.SizeOf<DMP>();

            // Extract byte arrays for each layer
            byte[] rootBytes = data.Take(rootSize).ToArray();
            byte[] frameBytes = data.Skip(rootSize).Take(frameSize).ToArray();
            byte[] dmpBytes = data.Skip(rootSize + frameSize).Take(dmpSize).ToArray();

            // Deserialize and convert each layer from network to host order
            Root rootLayer = DeserializeInternal<Root>(rootBytes).ToHostOrder();
            Frame frameLayer = DeserializeInternal<Frame>(frameBytes).ToHostOrder();
            DMP dmpLayer = DeserializeInternal<DMP>(dmpBytes).ToHostOrder();

            // Assemble the final Packet struct
            return new Packet
            {
                root = rootLayer,
                frame = frameLayer,
                dmp = dmpLayer
            };
        }
        public static byte[] SerializePacket(Packet Packet)
        {
            List<byte> data = new List<byte>();
            data.AddRange(SerializeInternal(Packet.root.ToNetworkOrder()));
            data.AddRange(SerializeInternal(Packet.frame.ToNetworkOrder()));
            data.AddRange(SerializeInternal(Packet.dmp.ToNetworkOrder()));

            return data.ToArray();
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
