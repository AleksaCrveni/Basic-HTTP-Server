using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;

namespace Utils
{
  public class ChunkReader : IDisposable
  {
    private FakeSocket _socket;
    public ByteBuilder _byteBuilder;
    private int _BPR = 1024; // bytes per read
    public ChunkReader(FakeSocket s)
    {
      _socket = s;
      _byteBuilder = new ByteBuilder();
    }
    public ChunkReader(int bytesPerRead, FakeSocket s)
    {
      _socket = s;
      _BPR = bytesPerRead;
      _byteBuilder = new ByteBuilder();
    }

    public string ReadLine() => ReadLine(Encoding.Default);
    /// <summary>
    /// I tried to handle all edge cases, but i feel like this isnt best way to write thius
    /// I think it would probably be less complicated if we always appended buffer to
    /// byteBuilder first and then just iterated over new range that was added
    /// </summary>
    /// <param name="enc"></param>
    /// <returns></returns>
    public string ReadLine(Encoding enc)
    {
      string res = string.Empty;
      // check if there is something in the buffer
      // since we might have loaded new line already as a cutoff

      // clean read we want just \n or \r\n. It also servers as a correction to convert lone \rs in 
      (int Read, int Write) pos = _byteBuilder.GetPositions();
      for (int i = pos.Read; i < pos.Write; i++)
      {
        if (_byteBuilder._rentedBuffer[i] == '\r')
        {
          // make sure we are not at last byte of used area
          if (i + 1 < pos.Write)
          {
            if (_byteBuilder._rentedBuffer[i + 1] == '\n')
            {
              res = _byteBuilder.ToString(i - pos.Read);
              _byteBuilder.EmptyRead(2);
              return res;
            }
            else
            {
              // spec says that if there is \r not followed by \n it should be converted to whitespace
              _byteBuilder._rentedBuffer[i] = (byte)' ';
            }
          }
        }
        else if (_byteBuilder._rentedBuffer[i] == '\n')
        {
          // alone \n is legal
          res = _byteBuilder.ToString(i - pos.Read);
          _byteBuilder.EmptyRead(1); // read past \n
          return res;
        }
      }


      byte[] buff = ArrayPool<byte>.Shared.Rent(_BPR);
      Span<byte> buffer = buff.AsSpan(0, _BPR);
      bool checkNextStart = false;

      while (true)
      {
        int rb = _socket.Read(ref buffer);
        if (rb == 0)
          break;
        int i = 0;
        // do this since its easier to check if last cutoff was \r and replace it
        if (checkNextStart)
        {
          if (buffer[0] == '\n')
          {
            // -2 to exlcude \r that we wil sk ip later
            res = _byteBuilder.ToString(_byteBuilder._writePos - _byteBuilder._readPos - 1);
            _byteBuilder.EmptyRead(1);

            _byteBuilder.Append(buff, 1, rb);
            ArrayPool<byte>.Shared.Return(buff);
            return res;
          }
          else
          {
            _byteBuilder.ReplaceLast((byte)' ');
          }
          i++;
        }

        for (; i < buffer.Length; i++)
        {
          if (buffer[i] == '\r')
          {
            // is last char
            if (i + 1 == buffer.Length)
            {
              checkNextStart = true;
            }
            else
            {
              if (buffer[i + 1] == '\n')
              {
                // append up to \r\n and read it all since we know its only line at this point of the function
                _byteBuilder.Append(buffer.Slice(0, i));
                res = _byteBuilder.ToString();

                // this will handle index out of range edge cases like if i + 1 was last char and stuff
                _byteBuilder.Append(buff, i + 2, rb);
                ArrayPool<byte>.Shared.Return(buff);
                return res;
              }
              else
              {
                // spec says that if there is \r not followed by \n it should be converted to whitespace
                buffer[i] = (byte)' ';
              }
            }
            // make sure we are not at last byte of used area
          }
          else if (buffer[i] == '\n')
          {
            if (i == 0)
            {
              // this can happen if we do check start because rest of the buffer we might append after we find our line ends with \r and won't know
              if (_byteBuilder.Last() == '\r')
              {
                res = _byteBuilder.ToString(_byteBuilder._writePos - _byteBuilder._readPos - 1);
                _byteBuilder.EmptyRead(1);
                _byteBuilder.Append(buff, 1, rb);
                ArrayPool<byte>.Shared.Return(buff);
                return res;
              }
            }
            else
            {
              // alone \n is legal as well
              _byteBuilder.Append(buffer.Slice(0, i));
              res = _byteBuilder.ToString();
              _byteBuilder.Append(buff, i + 1, rb);
              ArrayPool<byte>.Shared.Return(buff);
              return res;
            }
          }

        }

        // we know we didnt find line yet so we will append entire buffer including \r case
        _byteBuilder.Append(buffer);
      }

      if (_byteBuilder.Last() == '\r')
      {
        res = _byteBuilder.ToString(_byteBuilder._writePos - _byteBuilder._readPos - 1);
        _byteBuilder.Clear();
      }
      else
      {
        res = _byteBuilder.ToString();
      }
      ArrayPool<byte>.Shared.Return(buff);
      return res;
    }

    public byte[]? GetNextNBytes(int N)
    {
      if (_byteBuilder.GetUsedSpace() >= N)
      {
        return _byteBuilder.ToArray(N);
      }
      byte[] buff = ArrayPool<byte>.Shared.Rent(_BPR);
      Span<byte> buffer = buff.AsSpan(0, _BPR);
      while (_byteBuilder.GetUsedSpace() < N)
      {
        int rb = _socket.Read(ref buffer);
        if (rb == 0)
          break;
        _byteBuilder.Append(buffer);
      }
      
      if (_byteBuilder.GetUsedSpace() < N)
      {
        return null;
      }
      ArrayPool<byte>.Shared.Return(buff);
      return _byteBuilder.ToArray(N);
    }
    public void Dispose()
    {
      _byteBuilder.Dispose();
    }
  }
}
