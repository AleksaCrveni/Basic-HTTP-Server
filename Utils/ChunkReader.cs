using System.Buffers;
using System.Text;

namespace Utils
{
  public class ChunkReader : IDisposable
  {
    private FakeSocket _socket;
    private ByteBuilder _byteBuilder;
    private int _BPR = 1024; // bytes per read
    public ChunkReader(FakeSocket s)
    {
      _socket = s;
    }
    public ChunkReader(int bytesPerRead, FakeSocket s)
    {
      _socket = s;
      _BPR = bytesPerRead;
    }

    public string ReadLine() => ReadLine(Encoding.Default);
    public string ReadLine(Encoding enc)
    {
      return "";
      byte[] buff = ArrayPool<byte>.Shared.Rent(_BPR);
      Span<byte> buffer = buff.AsSpan();
      bool checkNextStart = false;

      // check if there is something in the buffer
      // since we might have loaded new line already as a cutoff
  




      while (true)
      {
        int rb = _socket.Read(ref buffer);
        if (rb == 0)
          break;
        int i = 0;
        if (checkNextStart)
        {
          if (buffer[i] == '\n')
          {
            string res = _byteBuilder.ToString();
          }
          i++;
        }

        for (; i < buffer.Length; i++)
        {
          if (buffer[i] == '\r')
          {
            // check if next is \n
            // check on next read
            if (i + 1 < buffer.Length)
            {

            }
            else
            {
              checkNextStart = true;
            }
          }
          else if (buffer[i] == '\n')
          {
          
          }

        }
      }

      ArrayPool<byte>.Shared.Return(buff);
    }

    public void ReadNextSpanOfBytes(int numOfBytes, ref Span<byte> span)
    {
      // 
    }
    public void Dispose()
    {
      _byteBuilder.Dispose();
    }
  }
}
