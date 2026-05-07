namespace Utils
{
  public class MyHTTPRequest
  {
    public RequestLine RequestLine;
    public Dictionary<string, string> Headers = new Dictionary<string, string>();
    public byte[] Body;
    public MyHTTPRequest()
    {
    }
  }

  public class RequestLine
  {
    public HTTPMethod Method;
    public string Target;
    public string Version;
    public Dictionary<string, string> Query = new Dictionary<string, string>();
  }
  public enum HTTPMethod
  {
    GET,
    POST,
  }

}
