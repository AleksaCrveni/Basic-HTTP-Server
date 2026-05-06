using System.Net;

namespace Utils
{
  public class MyHTTPResponse
  {
    public HttpStatusCode Status;
    public Dictionary<string, string> Headers = new Dictionary<string, string>();
    public byte[]? Body;
  }
}