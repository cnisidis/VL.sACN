
using Microsoft.VisualBasic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;


namespace sACN.Structs
{
    
    public struct Packet
    {
        
        public Root root;
       
        public Frame frame;
        public DMP dmp;

        


        public void Split(out Root Root, out Frame Frame, out DMP DMP)
        {
            Root = this.root;
            Frame = this.frame;
            DMP = this.dmp;
        }


        public static Packet Create(Guid CID, int Universe, string Identifier)
        {


            Identifier = Identifier.PadRight(64).TrimEnd();
            
            Root root = new Root()
            {
                preamble_size = sACN.Utils.Constants._E131_PREAMBLE_SIZE,
                postamble_size = Utils.Constants._E131_POSTAMBLE_SIZE,
                acn_pid = SacnPacketHelper.E131_ACN_PID,
                flength = SacnPacketHelper.CombineFlagsAndLength(0x7, 638), // Flags=7, Length=638 (Total packet size)
                vector = 0x00000004, // ACN_ROOT_LAYER_VECTOR
                cid = CID.ToByteArray()
            };
            Frame frame = new Frame()
            {
                flength = SacnPacketHelper.CombineFlagsAndLength(0x7, 600), // Flags=7, Length=600 (Framing + DMP)
                vector = 0x00000002, // ACN_FRAMING_LAYER_VECTOR
                source_name = Encoding.ASCII.GetBytes(Identifier).Concat(new byte[64 - Identifier.Length]).ToArray(),
                priority = 100,
                reserved = 0,
                seq_number = 1,
                options = 0,
                universe = (ushort)Universe // DMX Universe 1
            };

            DMP dmp = new DMP()
            {
                flength = SacnPacketHelper.CombineFlagsAndLength(0x7, 523), // Flags=7, Length=523 (DMP layer size)
                vector = 0x02, // DMP_LAYER_VECTOR
                type = 0xa1, // Address Type and Data Type
                first_addr = 0, // DMX Start Code (0)
                addr_inc = 1, // Address Increment
                prop_val_cnt = 513, // 1 for start code + 512 DMX slots
                prop_val = new byte[513] // DMX data
            };
            return new Packet() { root = root, dmp = dmp, frame = frame};
        }

        public void Update(byte DMXStartCode, Spread<byte> DMXValues, int seq)
        {
            
            //int seq = this.frame.seq_number;
            //seq += 1;
            //packet.frame.seq_number = packet.frame.seq_number % 256;
            //seq = seq % 256;

            this.frame.SetSeqNumber(seq);

            byte[] scode = { DMXStartCode };
            if (DMXValues.Count == 512) 
            {
                this.dmp.prop_val = scode.Concat(DMXValues.ToArray()).ToArray();
            }
            
        }
    }
}
