using System.Net.Sockets;
using System.Text;

namespace Utils
{
  /// <summary>
  /// This class just exists so that we can have same interface/wrapper for testing and actual code because Socket, Stream and NetworkStream do not really share same interface
  /// and makes testing stuff a bother
  /// That said it still doesnt 
  /// </summary>
  public class FakeSocket
  {
    private Socket _socket;
    private Stream _stream;
    private bool _isSocket;
    public FakeSocket(Socket s)
    {
      _socket = s;
      _isSocket = true;
    }

    public FakeSocket(Stream s)
    {
      _stream = s;
      _isSocket = false;
    }

    public int Read(ref Span<byte> buffer)
    {
      if (_isSocket)
      {
        return _socket.Receive(buffer);
      } 
      else
      {
        return _stream.Read(buffer);
      }
    }

    public int Write(ref Span<byte> buffer)
    {
      if (_isSocket)
      {
        return _socket.Send(buffer);
      } 
      else
      {
        _stream.Write(buffer);
        return 1;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bytesPerRead">This is mostly for testing chunked parsing</param>
    /// <returns></returns>
    public RequestLine ParseRequestLine(int bytesPerRead = 4)
    { 
      RequestLine l = new RequestLine();
      string raw = ReadLine(bytesPerRead);
      return l;
    }
    public string ReadLine(int bytesPerRead)
    {
      if (bytesPerRead <= 0)
        throw new Exception("Bytes per read must be positive non zero number!");
      Span<byte> buffer = new byte[bytesPerRead];
      return ReadLine(bytesPerRead, ref buffer);
    }
    public string ReadLine(int bytesPerRead, ref Span<byte> buffer)
    {
      return ""; 
      while (true)
      {
        int rb = Read(ref buffer);
        if (rb == 0)
          break;

      }
    }
  }
}
