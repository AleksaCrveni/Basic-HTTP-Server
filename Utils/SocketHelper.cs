using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Utils;

namespace Server
{
  public static class SocketHelper
  {
    /// <summary>
    /// Parse and validation of request line and headers
    /// </summary>
    /// <param name="r">Chunk reader that can read from the socket in efficient manner</param>
    /// <param name="request">Request that needs to be filled</param>
    /// <returns>HTTPStatus code that incidates and error if its differnt than 200, if there are any issues with parsing</returns>
    public static HttpStatusCode ParseRequest(ChunkReader r, ref MyHTTPRequest request)
    {
      string line = r.ReadLine();
      // spec says that we have to allow AT LEAST 1 empty line preceding request line
      if (line == "")
        line = r.ReadLine();

      (RequestLine? rl, HttpStatusCode code) = MessageHelper.ParseRequestLine(line);
      if (rl == null)
        return code;

      line = r.ReadLine();
      List<string> headerLines = new List<string>();
      while (line != "")
      {
        headerLines.Add(line);
        line = r.ReadLine();
      }

      (Dictionary<string, string>? headers, code) = MessageHelper.ParseMessageHeaders(headerLines);
      if (headers == null)
        return code;

      // header verification , optimize later
      string? transferEncoding = headers.GetValueOrDefault("Transfer-Encoding");
      string? contentLength = headers.GetValueOrDefault("Content-Length");
      if (transferEncoding != null && contentLength != null)
      {
        Debug.WriteLine("TransferEncoding and ContentLength not found!");
        return HttpStatusCode.LengthRequired;
      }

      Span<byte> bytes = new Span<byte>();
      if (transferEncoding != null)
      {
        Debug.WriteLine("TransferEncoding not supported yet!");
        return HttpStatusCode.InternalServerError;
      } 
      else
      {
        r.ReadNextSpanOfBytes(Convert.ToInt32(contentLength), ref bytes);
        request.Body = bytes;
      }

      return HttpStatusCode.OK;
    }
  }z
}
