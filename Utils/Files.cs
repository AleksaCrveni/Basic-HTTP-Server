namespace Utils
{
  public static class Files
  {
    public static string RootFolder { get; set; }
    public static string GETExample { get; set; }
    public static string POSTExample { get; set; }
    static Files()
    {
      RootFolder = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "Files");
      GETExample = Path.Combine(RootFolder, "get.http");
      POSTExample = Path.Combine(RootFolder, "post.http");
    }
  }
  


  
}
