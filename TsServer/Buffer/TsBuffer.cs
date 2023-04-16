using System.Numerics;

namespace TsServer.Buffer
{
    public struct BufferInformation
    {
        public uint offset;
        public uint length;
    }

    internal class TsBuffer
    {
        private byte[] _buffer = Array.Empty<byte>();
        public byte[] Buffer
        {
            get => _buffer;
        }

        private uint _pos;
        public uint Pos
        {
            get => _pos;
        }

        public byte this[uint index] => _buffer[index];

        public static TsBuffer LoadFromFile(string fileLocation, BufferInformation bufferInformation)
        {
            try
            {
                var _buffer = new byte[bufferInformation.length];
                using FileStream fs = new(fileLocation, FileMode.Open, FileAccess.Read);
                fs.Seek(bufferInformation.offset, 0);
                fs.Read(_buffer, 0, _buffer.Length);
                fs.Close();
                fs.Dispose();
                return new TsBuffer()
                {
                    _buffer = _buffer
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return new TsBuffer();
            }
        }

        public byte ReadByte()
        {
            try
            {
                return _buffer[_pos];
            }
            catch
            {
                return 0;
            }
            finally
            {
                _pos++;
            }
        }

        public T GetNumberAt<T>(uint index) where T : INumber<T>
        {
            return _buffer.GetNumberAt<T>(index, ref _pos);
        }

        public T GetNumber<T>() where T : INumber<T>
        {
            return _buffer.GetNumberAt<T>(_pos, ref _pos);
        }

        public ushort Read16()
        {
            return _buffer.Read16(_pos, ref _pos);
        }

        public int Read32()
        {
            return _buffer.Read32(_pos, ref _pos);
        }

        public uint ReadU32()
        {
            return _buffer.ReadU32(_pos, ref _pos);
        }

        public void SetPosition(uint pos)
        {
            _pos = pos;
        }

        public void IncreasePosition(uint offset)
        {
            _pos += offset;
        }

        public void DecreasePosition(uint offset)
        {
            _pos -= offset;
        }

        public void Clear()
        {
            Array.Clear(_buffer);
        }
    }
}
