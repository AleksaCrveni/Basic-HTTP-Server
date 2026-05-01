using System.Net;
using System.Net.Sockets;

namespace Server
{
  public class HTTPServer
  {
    public Socket _listener;
    public string IP;
    public int PORT;
    public HTTPServer(string ip, int port) 
    {
      IP = ip;
      PORT = port;
      _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      //IPAddress hostIp = (Dns.Resolve(IPAddress.Any.ToString())).AddressList[0];
      IPAddress hostIP = (Dns.GetHostEntry(IP)).AddressList[0];
      IPEndPoint ep = new IPEndPoint(hostIP, port);
      _listener.Bind(ep);
      
    }
    
    public void Start()
    {
      try
      {
        _listener.Listen();
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
        _listener.Close();
      }
    }

    public void Process()
    {
      while (true)
      {
        Socket s = _listener.Accept();
        Log("Connection accepted!");

      }
    }

    public void Log(string s)
    {
      Console.WriteLine(s);
    }
  }
}
