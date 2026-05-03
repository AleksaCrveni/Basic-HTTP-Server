using System.Net;
using System.Net.Sockets;

namespace Client
{
  public class Commands
  {
    
    public static Socket Connect(string IP, int PORT)
    {
      Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      //IPAddress hostIp = (Dns.Resolve(IPAddress.Any.ToString())).AddressList[0];
      IPAddress hostIP = (Dns.GetHostEntry(IP)).AddressList[0];
      IPEndPoint ep = new IPEndPoint(hostIP, PORT);
      try
      {
        s.Connect(ep);
      } catch (Exception ex)
      {

      }

      return s;
    }
  }
}
