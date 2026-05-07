using System.Buffers;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Utils;

namespace Server
{
  public class HTTPServer
  {
    public Socket _listener;
    public string IP;
    public int PORT;
    public bool _respWithErrors = true; // this is just simple way to block server errors when 500 occurs or when user responds with it
    public byte[] CRLF = [(byte)'\r', (byte)'\n'];
    public byte SPACE = (byte)' ';
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
        FakeSocket fs = new FakeSocket(s);
        using (ChunkReader r = new ChunkReader(fs))
        {
          while (true)
          {
            // i feel like this sucks completely
            if (s == null)
            {
              Log("Force closed connection!");
              break;
            }

            MyHTTPRequest req = new MyHTTPRequest();
            try
            {
              HttpStatusCode respCode = SocketHelper.ParseRequest(r, req);
              if (respCode != HttpStatusCode.OK)
              {
                SendResponse(fs, MessageHelper.CreateResponse(respCode, null));
                break;
              }
            }
            catch (SocketException ex)
            {
              // we should check error and see if its client or server issue
              // this wold mean that something happened with the connection i think, so don't try to read/send again
              Debug.WriteLine($"Socket exception. NativeError #{ex.NativeErrorCode}");
              break;
            }
            catch (Exception ex)
            {
              Debug.WriteLine($"Server error: {ex.Message}");
              SendResponse(fs, MessageHelper.CreateResponse(HttpStatusCode.InternalServerError, _respWithErrors ? ex.Message : null));
              break;
            }

            // call method to process based on target

            SendResponse(fs, MessageHelper.CreateResponse(HttpStatusCode.OK, "Test response"));
            break;
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

    // Not very efficient fix later maybe
    public void SendResponse(FakeSocket s, MyHTTPResponse r)
    {
      Encoding enc = Encoding.Default;

      using (Stream stream = s.GetStream())
      {
        stream.Write(enc.GetBytes(r.Version));
        stream.WriteByte(SPACE);
        stream.Write(enc.GetBytes(((int)r.Status).ToString()));
        stream.WriteByte(SPACE);
        stream.Write(CRLF);
        foreach (KeyValuePair<string, string> kvp in r.Headers)
        {
          stream.Write(enc.GetBytes(kvp.Key));
          stream.WriteByte((byte)':');
          stream.WriteByte(SPACE);
          stream.Write(enc.GetBytes(kvp.Value));
          stream.Write(CRLF);
        }

        stream.Write(CRLF);
        if (r.Body.Length > 0)
          stream.Write(r.Body);
      }
    }

    public void Log(string s)
    {
      Console.WriteLine(s);
    }
  }
}