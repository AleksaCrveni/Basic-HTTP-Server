using System.Diagnostics;
using Utils;

namespace Tests
{
  [TestClass]
  public class ByteBuilderTests
  {
    [TestMethod]
    // This doesnt actually test if data is propely copied
    // we are just testing states for now;
    public void TestBasic()
    {
      ByteBuilder byteBuilder = new ByteBuilder();
      int initActualSize = byteBuilder._rentedBuffer.Length;
      Debug.Assert(initActualSize > 0);
      int firstAlloc = initActualSize / 2;
      byte[] firstBuffer = new byte[firstAlloc];

      Debug.Assert(byteBuilder._readPos == 0);
      Debug.Assert(byteBuilder._writePos == 0);

      // append first 
      byteBuilder.Append(firstBuffer);
      Debug.Assert(byteBuilder._readPos == 0);
      Debug.Assert(byteBuilder._writePos == firstBuffer.Length);


      // take half of first
      int firstTake = firstAlloc / 2;

      byteBuilder.ToString(firstTake);
      Debug.Assert(byteBuilder._readPos == firstTake);
      Debug.Assert(byteBuilder._writePos == firstBuffer.Length);
      
      // append half of first
      int secondAlloc = firstAlloc / 2;
      byte[] secondBuffer = new byte[secondAlloc];

      byteBuilder.Append(secondBuffer);
      Debug.Assert(byteBuilder._rentedBuffer.Length == initActualSize); // DIDNT GROW
      Debug.Assert(byteBuilder._readPos == firstTake);
      Debug.Assert(byteBuilder._writePos == firstBuffer.Length + secondBuffer.Length);

      // append 1/4th + some to get it to move 
      byte[] thirdBuffer = new byte[secondAlloc / 2 + 15];

      int currWritePos = byteBuilder._writePos;
      byteBuilder.Append(thirdBuffer);
      Debug.Assert(byteBuilder._rentedBuffer.Length == initActualSize); // DIDNT GROW
      Debug.Assert(byteBuilder._readPos == firstTake);
      Debug.Assert(byteBuilder._writePos == currWritePos + thirdBuffer.Length);


      // read a bit

      int secondTake = 100;
      currWritePos = byteBuilder._writePos;
      byteBuilder.ToString(100);
      Debug.Assert(byteBuilder._rentedBuffer.Length == initActualSize); // DIDNT GROW
      Debug.Assert(byteBuilder._readPos == firstTake + 100);
      Debug.Assert(byteBuilder._writePos == currWritePos);

      // make it grow and move
      int usedLen = byteBuilder._writePos - byteBuilder._readPos;
      byteBuilder.Append(firstBuffer);
      Debug.Assert(byteBuilder._rentedBuffer.Length > initActualSize);
      Debug.Assert(byteBuilder._readPos == 0);
      Debug.Assert(byteBuilder._writePos == usedLen + firstBuffer.Length);
    }
  }
}
