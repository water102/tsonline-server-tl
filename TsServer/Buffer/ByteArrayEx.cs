using System;
using System.Numerics;
using System.Text;

namespace TsServer.Buffer
{
    public static class ByteArrayEx
    {
        public static T GetNumberAt<T>(this byte[] _buffer, uint index) where T : INumber<T>
        {
            return T.Parse(_buffer[index].ToString(), null);
        }

        public static T GetNumberAt<T>(this byte[] _buffer, uint index, ref uint pos) where T : INumber<T>
        {
            pos = index;
            return GetNumberAt<T>(_buffer, index);
        }

        public static string GetString(this byte[] _buffer, int startAt, int count)
        {
            return Encoding.Default.GetString(_buffer, startAt, count);
        }

        public static ushort Read16(this byte[] _buffer, uint index)
        {
            return (ushort)(_buffer[index] + (_buffer[index + 1] << 8));
        }

        public static ushort Read16(this byte[] _buffer, uint index, ref uint pos)
        {
            try
            {
                return Read16(_buffer, index);
            }
            catch
            {
                return 0;
            }
            finally
            {
                pos += 2;
            }
        }

        public static int Read32(this byte[] _buffer, uint index)
        {
            return BitConverter.ToInt32(_buffer, (int)index);
        }

        public static int Read32(this byte[] _buffer, uint index, ref uint pos)
        {
            try
            {
                return Read32(_buffer, index);
            }
            catch
            {
                return 0;
            }
            finally
            {
                pos += 4;
            }
        }

        public static uint ReadU32(this byte[] _buffer, uint index)
        {
            return BitConverter.ToUInt32(_buffer, (int)index);
        }

        public static uint ReadU32(this byte[] _buffer, uint index, ref uint pos)
        {
            try
            {
                return ReadU32(_buffer, index);
            }
            catch
            {
                return 0;
            }
            finally
            {
                pos += 4;
            }
        }
    }
}
