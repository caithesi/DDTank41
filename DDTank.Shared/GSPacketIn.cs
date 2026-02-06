using System;

namespace DDTank.Shared
{
    public class GSPacketIn : PacketIn
    {
        public const ushort HDR_SIZE = 20;
        public const short HEADER = 29099; // 0x71AB

        protected int m_clientId;
        protected short m_code;
        protected int m_parameter1;
        protected int m_parameter2;

        public int ClientID { get => m_clientId; set => m_clientId = value; }
        public short Code { get => m_code; set => m_code = value; }
        public int Parameter1 { get => m_parameter1; set => m_parameter1 = value; }
        public int Parameter2 { get => m_parameter2; set => m_parameter2 = value; }

        public GSPacketIn(short code) : this(code, 0, 8192) { }

        public GSPacketIn(byte[] buf, int size) : base(buf, size) { }

        public GSPacketIn(short code, int clientId, int size) : base(new byte[size], 20)
        {
            m_code = code;
            m_clientId = clientId;
            m_offset = 20;
        }

        public short CheckSum()
        {
            short num = 119;
            int i = 6;
            while (i < m_length)
            {
                num = (short)(num + m_buffer[i++]);
            }
            return (short)(num & 0x7F7F);
        }

        public void ReadHeader()
        {
            m_offset = 0;
            ReadShort(); // Header mark
            m_length = ReadShort();
            ReadShort(); // Checksum
            m_code = ReadShort();
            m_clientId = ReadInt();
            m_parameter1 = ReadInt();
            m_parameter2 = ReadInt();
        }

        public void WriteHeader()
        {
            int currentOffset = m_offset;
            m_offset = 0;
            WriteShort(HEADER);
            WriteShort((short)m_length);
            WriteShort(CheckSum());
            WriteShort(m_code);
            WriteInt(m_clientId);
            WriteInt(m_parameter1);
            WriteInt(m_parameter2);
            m_offset = currentOffset;
        }
    }
}
