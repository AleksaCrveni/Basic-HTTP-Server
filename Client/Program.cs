using Client;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Client Window Started!");

string ipPreset = "127.0.0.1";
int port = 42069;
Socket s = null;
s = Commands.Connect(ipPreset, port);


WriteL(s.Connected == true ? "CONNECTED" : "NOT CONNECTED!");
while (true)
{
  string? line = ReadL();
  if (line == null || line == "EXIT")
  {
    WriteL("Closing...");
    break;
  }

  if (s.Connected == false)
  {
    s.Close();
    s = Commands.Connect(ipPreset, port);
    if (s.Connected == false)
    {
      WriteL("Message not broadcasted. Unable to connect to the server!");
      continue;
    }
  }
  if (line.Trim() == "")
    continue;

  int sb = 0;
  try
  {
    sb = s.Send(Encoding.Default.GetBytes(line));
  } catch (Exception e)
  {
    s.Close();
    WriteL($"Error: {e.Message}");
    WriteL("Attempting to recconect!");
    s = Commands.Connect(ipPreset, port);
    if (s.Connected == false)
    {
      WriteL("Message not broadcasted. Unable to connect to the server!");
      continue;
    }
  }
  
  if (sb == 0)
    WriteL("Failed to send data!");
}

Console.ReadKey();
string? ReadL() => Console.ReadLine();
void WriteL(string line) => Console.WriteLine(line);