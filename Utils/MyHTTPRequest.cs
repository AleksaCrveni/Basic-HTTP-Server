namespace Utils
{
  public class MyHTTPRequest
  {
    public HTTPMethod Method;
    public string Target;
    public string Version;
    public Dictionary<string, string> Headers = new Dictionary<string, string>();
    public Dictionary<string, string> Query = new Dictionary<string, string>();
    public string Body;
  }

  public enum HTTPMethod
  {
    GET,
    POST,
  }

}
