using System.Diagnostics;
using System.Net;
using System.Text;

namespace Utils
{
  public static class MessageHelper
  {
    public static (RequestLine? rl, HttpStatusCode code) ParseRequestLine(string line)
    {
      RequestLine rl = new RequestLine();
      string[] split = line.Split(' ');
      if (split.Length != 3)
      {
        Debug.WriteLine("Bad request line!");
        return (null, HttpStatusCode.BadRequest);
      }

      if (!Enum.TryParse<HTTPMethod>(split[0], out HTTPMethod method))
      {
        Debug.WriteLine("Method not allowed!");
        return (null, HttpStatusCode.MethodNotAllowed);
      }
        
      rl.Method = method;
      string target = split[1];
      //TODO(@Aleksa): do some more target validatiion
      if (string.IsNullOrEmpty(target))
      {
        Debug.WriteLine("Invalid target!");
        return (null, HttpStatusCode.BadRequest);
      }

      // 2k limit for now
      if (target.Length > 2000)
      {
        Debug.WriteLine("Target too long!");
        return (null, HttpStatusCode.RequestUriTooLong);
      }

      int queryIndex = target.IndexOf('?');

      if (queryIndex == -1)
      {
        rl.Target = target;
      }
      else
      {
        rl.Target = target.AsSpan().Slice(0, queryIndex).ToString();
        string[] query = target.AsSpan().Slice(queryIndex + 1).ToString().Split('=');
        for (int i = 0; i < query.Length; i++)
        {
          if (i + 1 >= query.Length)
            break;
          rl.Query[query[i++]] = query[i];
        }
      }

      // no checks performed
      rl.Version = split[2];
      if (rl.Version != "HTTP/1.1")
      {
        Debug.WriteLine($"HTTP version not supported. Got {rl.Version}");
        return (null, HttpStatusCode.HttpVersionNotSupported);
      }
      return (rl, HttpStatusCode.OK);
    }

    //TODO(@Aleksa): Do 5.2 Obsolote Line Folding properly
    public static (Dictionary<string, string>? headers, HttpStatusCode) ParseMessageHeaders(List<string> lines)
    {
      Dictionary<string, string> headers = new Dictionary<string, string>();
      Debug.WriteLine("Headers:");
      // NOTE: do proper checks with spaces ref -> rfc 9112 5.1
      foreach (string line in lines)
      {
        if (line == "" || line == "\r\n")
        {
          Debug.WriteLine("Empty header line!");
          return (null, HttpStatusCode.BadRequest);
        }

        ReadOnlySpan<char> span = line.AsSpan();
        int index = span.IndexOf(':');
        if (index == -1)
        {
          Debug.WriteLine($"Invalid Header line. Got: {line}");
          return (null, HttpStatusCode.BadRequest);
        }

        // Specified by spec that there can't be whitespace between column and end of field name
        if (span[index - 1] == ' ')
        {
          Debug.WriteLine($"No whitespace is allowed between the field name and colon.");
          return (null, HttpStatusCode.BadRequest);
        }

        string key = span.Slice(0, index).ToString();

        // preceding and succeed single whitespaces need to be ignored but are valid!
        int valStart = index + 1;
        if (span[valStart] == ' ')
          valStart++;
        int valEnd = span.Length;
        if (span[span.Length - 1] == ' ')
          valEnd--;

        if (valEnd <= valStart)
        {
          Debug.WriteLine("Empty field value!");
          return (null, HttpStatusCode.BadRequest);
        }

        string value = span.Slice(valStart, valEnd - valStart).ToString();

        Debug.WriteLine($"{key}: {value}");

        headers[key] = value;
      }
      return (headers, HttpStatusCode.OK);
    }

    public static MyHTTPResponse CreateResponse(HttpStatusCode statusCode, string? body)
    {
      MyHTTPResponse r = new MyHTTPResponse();
      r.Status = statusCode;
      r.Version = "HTTP/1.1";
      (Dictionary<string, string> Headers, byte[] Body) data = GetDefaultHeadersAndBody(statusCode, body);
      r.Headers = data.Headers;
      r.Body = data.Body;
      return r;
    }
    
    public static (Dictionary<string, string> Headers, byte[] body) GetDefaultHeadersAndBody(HttpStatusCode statusCode, string? body)
    {
      Encoding enc = Encoding.Default;
      Dictionary<string, string> headers = new Dictionary<string, string>();
      byte[] bodyArr = Array.Empty<byte>();
      switch (statusCode)
      {
        case HttpStatusCode.OK:
          headers.Add("Host", "localhost:42069");
          if (body != null)
          {
            headers.Add("Content-Type", "text/plain");
            bodyArr = enc.GetBytes(body);
            headers.Add("Content-Length", bodyArr.Length.ToString());
            headers.Add("Accept", "*/*");
          }
          break;
        default:
          throw new NotSupportedException($"{statusCode.ToString()} not supported yet!");
      };

      return (headers, bodyArr);
    }
  }
}
