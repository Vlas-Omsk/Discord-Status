using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GitHashes
{
    class StreamFormatReader : FileStream
    {
        public StreamFormatReader(string path) : base(path, FileMode.Open)
        {
        }

        private byte[] savedHeader = null;
        private int headerPos = 0;

        private byte[] getHeader()
        {
            return savedHeader ?? (savedHeader = Encoding.UTF8.GetBytes("blob " + Length + "\0"));
        }

        private long getTrueLength()
        {
            var savepos = Position;
            long length = 0;
            while (ReadByte() != -1)
                length++;
            Position = savepos;
            return length;
        }

        public override long Length => getTrueLength();

        public override int ReadByte()
        {
            var b = base.ReadByte();
            if (b == 13)
            {
                b = base.ReadByte();
                if (b != 10)
                {
                    b = 13;
                    Position--;
                }
            }
            return b;
        }

        public override int Read(byte[] array, int offset, int count)
        {
            var header = getHeader();
            var readCount = 0;
            if (headerPos < header.Length)
            {
                var readLen = Math.Min(header.Length, count);
                for (var i = Position; i < readLen; i++)
                {
                    array[offset + i - Position] = header[i];
                }

                headerPos += readLen;
                offset += readLen;
                count -= readLen;
                readCount = readLen;
            }

            for (var i = offset; i < offset + count; i++)
            {
                var b = ReadByte();
                if (b == -1)
                    return readCount;
                array[i] = (byte)b;
                readCount++;
            }

            return readCount;
        }
    }
}
