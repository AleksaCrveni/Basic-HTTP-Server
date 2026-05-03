using System.Net;
using System.Net.Sockets;
using System.Text;

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
      Console.WriteLine("Server started!");
      byte[] checkAliveBuffer = new byte[1] { 1 };
      byte[] buffer = new byte[1024];
      while (true)
      {
        Socket s = _listener.Accept();
        Log("Connection accepted!");
        while (true)
        {
          // i feel like this sucks completely
          if (s == null)
          {
            Log("Force closed connection!");
            break;
          }
          try
          {
            int sb = s.Send(checkAliveBuffer);
            if (sb == 0)
              break;
          } catch (Exception ex)
          {
            break;
          }

          if (s.Available != 0)
          {
            //buffer = new byte[s.Available];
            //s.Receive(buffer, SocketFlags.None);
            List<string> strings = SocketHelper.ReturnNewLines(s);
            Log($"Received data at {DateTime.Now.ToLongTimeString()}:");
            foreach (string str in strings)
              Log(str);
        //    File.WriteAllBytes("get.http", buffer);
          }
        }

        if (s != null)
        {
          Log("Closing connection");
          s.Close();
          Log("Connection closed!");
        }
      }
    }

    public void Log(string s)
    {
      Console.WriteLine(s);
    }
  }
}
