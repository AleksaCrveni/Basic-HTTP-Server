using System.Text;

namespace Server
{
  public static class StreamHelper
  {
    public static List<string> ReturnNewLines(Stream s)
    {
      StringBuilder sb = new StringBuilder();
      return ReturnNewLines(s, sb);
    }
    public static List<string> ReturnNewLines(Stream s, StringBuilder sb)
    {
      List<string> res = new List<string>();
      sb.Clear();
      int readCount = 8;
      Span<byte> buffer = stackalloc byte[8];
      Encoding enc = Encoding.Default;
      while (true)
      {
        int bytesRead = s.Read(buffer);
        if (bytesRead == 0)
          break;

        int ind = buffer.IndexOf((byte)'\n');
        if (ind != -1)
        {
          sb.Append(enc.GetString(buffer.Slice(0, ind)));
          res.Add(sb.ToString());
          sb.Clear();
          sb.Append(enc.GetString(buffer.Slice(ind + 1)));
        }
        else
        {
          sb.Append(enc.GetString(buffer.Slice(0, bytesRead)));
        }
        
      }

      if (sb.Length > 0)
        res.Add(sb.ToString());

      sb.Clear();
      return res;
    }
  }
}
