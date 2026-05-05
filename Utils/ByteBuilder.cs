using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace Utils
{
  public class ByteBuilder
  {
    public int INIT_SIZE = 1024;
    public byte[]? _rentedBuffer;
    public int _readPos;
    public int _writePos;

    public ByteBuilder()
    {
      _rentedBuffer = ArrayPool<byte>.Shared.Rent(INIT_SIZE);
      _readPos = 0;
      _writePos = 0;
    }

    public void Append(byte[] appendBuffer)
    {
      Append(appendBuffer.AsSpan());
    }
    public void Append(Span<byte> appendBuffer)
    {
      if  (appendBuffer.Length == 1 && (uint)_writePos < (uint)_rentedBuffer.Length)
      {
        _rentedBuffer[_writePos] = appendBuffer[0];
        _writePos++;
        return;
      }

      // these calcs could probably be done smarter, but this is straightforward way
      int secondAvailableSectionSize = _rentedBuffer.Length - _writePos;
      if (secondAvailableSectionSize < appendBuffer.Length)
      {
        // this is section between start of the buffer and _readPos
        int firstAvailableSectionSize = _readPos;
        int usedSectionLen = _writePos - _readPos; // Length of used section which becomes _writePos after copying
        if (secondAvailableSectionSize + firstAvailableSectionSize > appendBuffer.Length)
        {
          // this means that we can just move data to the start and we will haev enought space
          _rentedBuffer.AsSpan(_readPos, usedSectionLen).CopyTo(_rentedBuffer);
          _readPos = 0;
        }
        else
        {
          // this means that even with moving data we have to grow
          GrowAndMoveToStart(appendBuffer.Length - usedSectionLen);
        }

        appendBuffer.CopyTo(_rentedBuffer.AsSpan(usedSectionLen));
        _writePos = usedSectionLen + appendBuffer.Length;
      }
      else
      {
        appendBuffer.CopyTo(_rentedBuffer.AsSpan(_writePos));
        _writePos += appendBuffer.Length;
      }

      Debug.Assert(_readPos <= _writePos);
    }

    private void GrowAndMoveToStart(int extraBytesAfterPosNeeded)
    {
      if (_rentedBuffer == null)
        return;

      int newCapacity = (int)Math.Max(
                (uint)(_writePos + extraBytesAfterPosNeeded),
                Math.Min((uint)_rentedBuffer.Length * 2, Array.MaxLength));

      byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newCapacity);

      int usedSectionLen = _writePos = _writePos - _readPos;
      _rentedBuffer.AsSpan(_readPos, usedSectionLen).CopyTo(newBuffer); // move data to start
      _readPos = 0;

      byte[]? toReturn = _rentedBuffer;
      if (toReturn != null)
        ArrayPool<byte>.Shared.Return(toReturn);

      _rentedBuffer = newBuffer;
    }

    public void Dispose()
    {
      if (_rentedBuffer != null)
        ArrayPool<byte>.Shared.Return(_rentedBuffer);
    }

    public void Clear()
    {
      _readPos = 0;
      _writePos = 0;
    }

    
    public string ToString() => ToString(Encoding.Default);
    public string ToString(Encoding enc)
    {
      string res = enc.GetString(_rentedBuffer.AsSpan(_readPos, _writePos));
      Clear();
      return res;
    }
    public byte Last() => _writePos > 0 ? _rentedBuffer[_writePos] : (byte)0;

    public string ToString(int size) => ToString(size, Encoding.Default);
    public string ToString(int size, Encoding enc)
    {
      if (_readPos + size > _writePos)
        throw new InvalidDataException("Chunking unavailble data!");

      string res = enc.GetString(_rentedBuffer.AsSpan(_readPos, _readPos + size));
      _readPos += size;
      return res;
    }

  }
}
