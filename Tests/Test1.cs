using Server;
using System.Diagnostics;
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
      string[] lines = File.ReadAllLines(Files.POSTExample);
      MyHTTPRequest request = MessageHelper.ParseRequest(lines.ToList());
      Debug.Assert(request.RequestLine.Method == HTTPMethod.POST);
      Debug.Assert(request.RequestLine.Target == "/coffee");
      Debug.Assert(request.RequestLine.Version == "HTTP/1.1");
      Debug.Assert(request.Headers["Host"] == "localhost:42069");
      Debug.Assert(request.Headers["User-Agent"] == "curl/8.13.0");
      Debug.Assert(request.Headers["Accept"] == "*/*");
    }
    [TestMethod]
    public void ParsePOST()
    {
      string[] lines = File.ReadAllLines(Files.POSTExample);
      MyHTTPRequest request = MessageHelper.ParseRequest(lines.ToList());
      Debug.Assert(request.RequestLine.Method == HTTPMethod.POST);
      Debug.Assert(request.RequestLine.Target == "/coffee");
      Debug.Assert(request.RequestLine.Version == "HTTP/1.1");
      Debug.Assert(request.RequestLine.Query["q"] == "test");
      Debug.Assert(request.Headers["Host"] == "localhost:42069");
      Debug.Assert(request.Headers["User-Agent"] == "curl/8.13.0");
      Debug.Assert(request.Headers["Accept"] == "*/*");
      Debug.Assert(request.Headers["Content-Type"] == "application/json");
      Debug.Assert(request.Headers["Content-Length"] == "22");
      Debug.Assert(request.Body == "{\"flavor\":\"dark mode\"}");
    }
    public Stream TurnStringIntoStream(string s) => new MemoryStream(Encoding.Default.GetBytes(s));
  }
}
