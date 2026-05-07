using Server;
using System.Diagnostics;
using System.Net;
using System.Text;
using Utils;

namespace Tests
{
  [TestClass]
  public sealed class Test1
  {
    [TestMethod]
    public void ParseGET()
    {
      string input = "GET / HTTP/1.1\r\nHost: localhost:42069\r\nUser-Agent: curl/7.81.0\r\nAccept: */*\r\n\r\n";
      FakeSocket fs = new FakeSocket(new MemoryStream(Encoding.Default.GetBytes(input)));
      MyHTTPRequest req = new MyHTTPRequest();
      ChunkReader r = new ChunkReader(fs);
      HttpStatusCode status = SocketHelper.ParseRequest(r, req);
      Debug.Assert(status == HttpStatusCode.OK);
      Debug.Assert(req.RequestLine.Method == HTTPMethod.GET);
      Debug.Assert(req.RequestLine.Target == "/");
      Debug.Assert(req.RequestLine.Version == "HTTP/1.1");
      Debug.Assert(req.Headers["Host"] == "localhost:42069");
      Debug.Assert(req.Headers["User-Agent"] == "curl/7.81.0");
      Debug.Assert(req.Headers["Accept"] == "*/*");
    }
    [TestMethod]
    public void ParsePOST()
    {
      //string[] lines = File.ReadAllLines(Files.POSTExample);
      //MyHTTPRequest request = MessageHelper.ParseRequest(lines.ToList());
      //Debug.Assert(request.RequestLine.Method == HTTPMethod.POST);
      //Debug.Assert(request.RequestLine.Target == "/coffee");
      //Debug.Assert(request.RequestLine.Version == "HTTP/1.1");
      //Debug.Assert(request.RequestLine.Query["q"] == "test");
      //Debug.Assert(request.Headers["Host"] == "localhost:42069");
      //Debug.Assert(request.Headers["User-Agent"] == "curl/8.13.0");
      //Debug.Assert(request.Headers["Accept"] == "*/*");
      //Debug.Assert(request.Headers["Content-Type"] == "application/json");
      //Debug.Assert(request.Headers["Content-Length"] == "22");
      //Debug.Assert(request.Body == "{\"flavor\":\"dark mode\"}");
    }
    public Stream TurnStringIntoStream(string s) => new MemoryStream(Encoding.Default.GetBytes(s));
  }
}
