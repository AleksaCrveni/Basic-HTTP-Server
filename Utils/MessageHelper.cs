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
        int valEnd = span.Length
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

    public static MyHTTPRequest ParseRequest(List<string> lines)
    {
      return new MyHTTPRequest();
      //MyHTTPRequest request = new MyHTTPRequest();
      
      //// NOTE: do proper checks with spaces ref -> rfc 9112 5.1
      //for (; currLine < lines.Count; currLine++)
      //{
      //  //temp
      //  if (lines[currLine] == "" || lines[currLine] == "\r\n")
      //    break;

      //  ReadOnlySpan<char> span = lines[currLine].AsSpan();
      //  int index = span.IndexOf(':');
      //  if (index == -1)
      //    throw new InvalidDataException("Invalid Header!");
        
      //  if (span[index-1] == ' ')
      //    throw new InvalidDataException(": can't be preceded by whitespace!");
      //  string key = span.Slice(0, index).ToString();

      //  int valStart = index + 1;
      //  if (span[valStart] == ' ')
      //    valStart++;
      //  int valEnd = span.Length;
      //  //int valEnd = valStart;
      //  // this will be needed for stream version
      //  //for (; valEnd < span.Length; valEnd++)
      //  //{
      //  //  if (span[valEnd] == ' ' || span[valEnd] == '\r' | span[valEnd] == '\n')
      //  //    break;
      //  //}
      //  //if (valEnd == span.Length)
      //  //  throw new InvalidDataException("Invalid field value! No end of the value found!");

      //  string value = span.Slice(valStart, valEnd - valStart).ToString();

      //  request.Headers[key] = value;
      //}

      //// TODO: do some headers verification list

      //string? lenStr = request.Headers.GetValueOrDefault("Content-Length");
      //if (lenStr == null)
      //  throw new InvalidDataException("Missing critical headers!");
      //int len = Convert.ToInt32(lenStr);

      //if (currLine >= lines.Count || (lines[currLine] != "\r\n" && lines[currLine] != ""))
      //  throw new InvalidDataException("Header r n separation not found");

      //// this is stupid and will be changed onec we start checking these specifically from stream/socket
      //StringBuilder sb = new StringBuilder();
      //for (int i = currLine; i < lines.Count; i++)
      //{
      //  sb.Append(lines[i]);
      //}

      //request.Body = sb.ToString();
      //return request;
    }

    public static MyHTTPResponse CreateResponse(HttpStatusCode statusCode, string? body)
    {
      return new MyHTTPResponse();
    }
  }
}
