using System.Text;

namespace Utils
{
  public static class MessageHelper
  {
    /// <summary>
    /// Very non performant solutions, make it work first
    /// </summary>
    /// <param name="lines"></param>
    /// <returns></returns>
    /// <exception cref="InvalidDataException"></exception>
    public static MyHTTPRequest ParseRequest(List<string> lines)
    {
      MyHTTPRequest request = new MyHTTPRequest();
      int currLine = 0;
      if (lines[0] == "\r\n")
        currLine++;
      string[] line = lines[currLine++].Split(' ');
      if (line.Length != 3)
        throw new InvalidDataException("Invalid request line data length!");

      if (!Enum.TryParse<HTTPMethod>(line[0], out HTTPMethod method))
        throw new InvalidDataException("Invalid HTTP Method");
      request.Method = method;

      string target = line[1];
      if (string.IsNullOrEmpty(target))
        throw new InvalidDataException("Invalid request target");
      int queryIndex = target.IndexOf('?');

      if (queryIndex == -1)
      {
        request.Target = target;
      }
      else
      {
        request.Target = target.AsSpan().Slice(0, queryIndex).ToString();
        string[] query = target.AsSpan().Slice(queryIndex + 1).ToString().Split('=');
        for (int i = 0; i < query.Length; i++)
        {
          if (i + 1 >= query.Length)
            break;
          request.Query[query[i++]] = query[i];
        }
      }

      // no checks performed
      request.Version = line[2];
      
      // NOTE: do proper checks with spaces ref -> rfc 9112 5.1
      for (; currLine < lines.Count; currLine++)
      {
        //temp
        if (lines[currLine] == "" || lines[currLine] == "\r\n")
          break;

        ReadOnlySpan<char> span = lines[currLine].AsSpan();
        int index = span.IndexOf(':');
        if (index == -1)
          throw new InvalidDataException("Invalid Header!");
        
        if (span[index-1] == ' ')
          throw new InvalidDataException(": can't be preceded by whitespace!");
        string key = span.Slice(0, index).ToString();

        int valStart = index + 1;
        if (span[valStart] == ' ')
          valStart++;
        int valEnd = span.Length;
        //int valEnd = valStart;
        // this will be needed for stream version
        //for (; valEnd < span.Length; valEnd++)
        //{
        //  if (span[valEnd] == ' ' || span[valEnd] == '\r' | span[valEnd] == '\n')
        //    break;
        //}
        //if (valEnd == span.Length)
        //  throw new InvalidDataException("Invalid field value! No end of the value found!");

        string value = span.Slice(valStart, valEnd - valStart).ToString();

        request.Headers[key] = value;
      }

      // TODO: do some headers verification list

      string? lenStr = request.Headers.GetValueOrDefault("Content-Length");
      if (lenStr == null)
        throw new InvalidDataException("Missing critical headers!");
      int len = Convert.ToInt32(lenStr);

      if (currLine >= lines.Count || (lines[currLine] != "\r\n" && lines[currLine] != ""))
        throw new InvalidDataException("Header r n separation not found");

      // this is stupid and will be changed onec we start checking these specifically from stream/socket
      StringBuilder sb = new StringBuilder();
      for (int i = currLine; i < lines.Count; i++)
      {
        sb.Append(lines[i]);
      }

      request.Body = sb.ToString();
      return request;
    }
  }
}
