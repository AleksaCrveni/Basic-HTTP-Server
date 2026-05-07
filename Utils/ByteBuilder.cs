using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace Utils
{
  /// <summary>
  /// Constraints:
  // * ReadPos <= WritePos
  // * Can be only used where we want to be able to just evict/use portions of the buffer
  // starting from ReadPos up to WritePos in linear fashion.

  /*
   * Explanation:
   * Can be used as a byte/string builder
   * I.e We can use it when in networking where we want to read data from the socket 
     but do not know full length of the line or string that we are trying to take.
     So we have to keep track of read and write positions and expand buffer and move data on need
   * "Fragmentation" of the buffer occurs when we would read X amount of bytes of data from the socket
     and that read would contain multiple lines or strings, so when we would read only 1 string or line
     our buffer would end up looking like that
   * To optimize copying of data :
     -> We will first calculate if there is available space to append AFTER writePos up to end of the buffer
       -> If YES we will just append data there and END
       -> If NO we will will check if there is TOTAL Available space to append new data.
           -> If YES we will move Used data to the start of the buffer and reset update Read and Write positions,
               append data and again update WritePos
           -> If NOT we will GROW buffer (actually rent new one and return current one) by 2 times or up to MAX array size,
               move data to the start of the buffer and update Read and Write positions and then append data and update Write Pos
   */

  /// </summary>
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

    public void Append(byte[] appendBuffer, int start, int actualSize)
    {
      if (start + 1 >= actualSize)
        return;
      Append(appendBuffer.AsSpan(start, actualSize - start));
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
    public byte Last() => _writePos > 0 ? _rentedBuffer[_writePos - 1] : (byte)0;
    public void ReplaceLast(byte b)
    {
      if (_writePos == 0)
        _rentedBuffer[0] = b;
      else
        _rentedBuffer[_writePos - 1] = b;
    }

    public string ToString(int size) => ToString(size, Encoding.Default);
    public string ToString(int size, Encoding enc)
    {
      if (_readPos + size > _writePos)
        throw new InvalidDataException("Chunking unavailble data!");

      string res = enc.GetString(_rentedBuffer.AsSpan(_readPos, size));
      _readPos += size;
      return res;
    }

    public (int readPos, int writePos) GetPositions() => (_readPos, _writePos);
    public void EmptyRead(int size)
    {
      // gurantee we dont move past write pos
      int actualMoveSize = Math.Min(size, _writePos - _readPos);
      Debug.Assert(_readPos <= _writePos);
      _readPos += actualMoveSize;
    }
    public int GetUsedSpace() => _writePos - _readPos;
    public byte[] ToArray() => _rentedBuffer.AsSpan(_readPos, GetUsedSpace()).ToArray();
    public byte[] ToArray(int size)
    {
      if (_readPos + size > _writePos)
        throw new InvalidDataException("Reading too much data!");
      return _rentedBuffer.AsSpan(_readPos, size).ToArray();
    }
  }
}
