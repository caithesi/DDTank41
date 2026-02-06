using System;
using System.Text;
using System.Threading;

namespace DDTank.Shared
{
    public class PacketIn
    {
        public volatile bool isSended = true;
        private byte lastbits;
        protected byte[] m_buffer;
        protected int m_length;
        public volatile int m_loop;
        protected int m_offset;
        public volatile int m_sended;
        public volatile int packetNum;

        public byte[] Buffer => m_buffer;
        public int DataLeft => m_length - m_offset;
        public int Length => m_length;
        public int Offset
        {
            get => m_offset;
            set => m_offset = value;
        }

        public PacketIn(byte[] buf, int len)
        {
            m_buffer = buf;
            m_length = len;
            m_offset = 0;
        }

        public virtual int CopyFrom(byte[] src, int srcOffset, int offset, int count)
        {
            if (count < m_buffer.Length && count - srcOffset < src.Length)
            {
                System.Buffer.BlockCopy(src, srcOffset, m_buffer, offset, count);
                return count;
            }
            return -1;
        }

        public virtual int CopyTo(byte[] dst, int dstOffset, int offset)
        {
            int count = ((m_length - offset < dst.Length - dstOffset) ? (m_length - offset) : (dst.Length - dstOffset));
            if (count > 0)
            {
                System.Buffer.BlockCopy(m_buffer, offset, dst, dstOffset, count);
            }
            return count;
        }

        public virtual bool ReadBoolean() => m_buffer[m_offset++] != 0;
        public virtual byte ReadByte() => m_buffer[m_offset++];
        
        public virtual byte[] ReadBytes() => ReadBytes(m_length - m_offset);
        public virtual byte[] ReadBytes(int maxLen)
        {
            byte[] destinationArray = new byte[maxLen];
            Array.Copy(m_buffer, m_offset, destinationArray, 0, maxLen);
            m_offset += maxLen;
            return destinationArray;
        }

        public virtual int ReadInt()
        {
            byte v1 = ReadByte();
            byte v2 = ReadByte();
            byte v3 = ReadByte();
            byte v4 = ReadByte();
            return (v1 << 24) | (v2 << 16) | (v3 << 8) | v4;
        }

        public virtual short ReadShort()
        {
            byte v1 = ReadByte();
            byte v2 = ReadByte();
            return (short)((v1 << 8) | v2);
        }

        public virtual string ReadString()
        {
            short count = ReadShort();
            string str = Encoding.UTF8.GetString(m_buffer, m_offset, count);
            m_offset += count;
            return str.Replace("\0", "");
        }

        public virtual void WriteByte(byte val)
        {
            if (m_offset == m_buffer.Length)
            {
                byte[] sourceArray = m_buffer;
                m_buffer = new byte[m_buffer.Length * 2];
                Array.Copy(sourceArray, m_buffer, sourceArray.Length);
            }
            m_buffer[m_offset++] = val;
            m_length = ((m_offset > m_length) ? m_offset : m_length);
        }

        public virtual void WriteInt(int val)
        {
            WriteByte((byte)(val >> 24));
            WriteByte((byte)((val >> 16) & 0xFF));
            WriteByte((byte)((val >> 8) & 0xFF));
            WriteByte((byte)(val & 0xFF));
        }

        public virtual void WriteShort(short val)
        {
            WriteByte((byte)(val >> 8));
            WriteByte((byte)(val & 0xFF));
        }

        public virtual void WriteString(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                WriteShort((short)(bytes.Length + 1));
                Write(bytes, 0, bytes.Length);
                WriteByte(0);
            }
            else
            {
                WriteShort(1);
                WriteByte(0);
            }
        }

        public virtual void Write(byte[] src, int offset, int len)
        {
            if (m_offset + len >= m_buffer.Length)
            {
                byte[] sourceArray = m_buffer;
                m_buffer = new byte[m_buffer.Length * 2];
                Array.Copy(sourceArray, m_buffer, sourceArray.Length);
                Write(src, offset, len);
            }
            else
            {
                Array.Copy(src, offset, m_buffer, m_offset, len);
                m_offset += len;
                m_length = ((m_offset > m_length) ? m_offset : m_length);
            }
        }
    }
}
