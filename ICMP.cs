using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traceroute
{
    public class ICMP
    {
        public byte id;
        public byte type;
        public byte code;
        public UInt16 checkSum;
        public int messageSize;
        public byte[] message = new byte[1024];

        public ICMP()
        { }

        public ICMP(byte[] data, int size)
        {
            id = data[26];
            type = data[20];
            code = data[21];
            checkSum = BitConverter.ToUInt16(data, 22);
            messageSize = size - 24;
            Buffer.BlockCopy(data, 24, message, 0, messageSize);
        }

        public byte[] GetBytes()
        {
            byte[] data = new byte[messageSize + 10];
            Buffer.BlockCopy(BitConverter.GetBytes(type), 0, data, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(code), 0, data, 1, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(checkSum), 0, data, 2, 2);
            Buffer.BlockCopy(message, 0, data, 4, messageSize);
            Buffer.BlockCopy(BitConverter.GetBytes(id), 0, data, data.Length - 1, 1);
            return data;
        }

        public UInt16 GetCheckSum()
        {
            UInt32 check = 0;
            byte[] data = GetBytes();
            int packetsize = messageSize + 8;
            int index = 0;

            while (index < packetsize)
            {
                check += Convert.ToUInt32(BitConverter.ToUInt16(data, index));
                index += 2;
            }
            check = (check >> 16) + (check & 0xffff);
            check += (check >> 16);
            return (UInt16)(~check);
        }
    }
}
