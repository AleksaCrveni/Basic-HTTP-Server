using System.Net;
using System.Net.Sockets;

namespace Server
{
  public class HTTPServer
  {
    public Socket _socket;
    public string IP;
    public int PORT;
    public HTTPServer(string ip, int port) 
    {
      IP = ip;
      PORT = port;
      _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      //IPAddress hostIp = (Dns.Resolve(IPAddress.Any.ToString())).AddressList[0];
      IPAddress hostIP = (Dns.GetHostEntry(IP)).AddressList[0];
      IPEndPoint ep = new IPEndPoint(hostIP, port);
      _socket.Bind(ep);
      
    }
    
    public void Start()
    {
      try
      {
        _socket.Listen();
        Process();
      }
      catch (SocketException ex)
      {
        Console.WriteLine($"Socket Exception #{ex.NativeErrorCode}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Exception: {ex.Message}");
      }
      finally
      {
        _socket.Close();
      }
    }

    public void Process()
    {
      while (true)
      {
      }
    }

  }
}
