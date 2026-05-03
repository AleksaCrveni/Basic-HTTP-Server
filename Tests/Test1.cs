using Server;
using System.Diagnostics;
using System.Text;

namespace Tests
{
  [TestClass]
  public sealed class Test1
  {
    [TestMethod]
    public void TestNewLines()
    {
      string input = "This is\nNew \n LineTest";
      string[] expected = input.Split('\n');

      List<string> res = SocketHelper.ReturnNewLines(TurnStringIntoStream(input));
      Debug.Assert(res.Count == expected.Length);
      for (int i = 0; i < expected.Length; i++)
        Debug.Assert(expected[i] == res[i]);
    }

    public Stream TurnStringIntoStream(string s) => new MemoryStream(Encoding.Default.GetBytes(s));
  }
}
