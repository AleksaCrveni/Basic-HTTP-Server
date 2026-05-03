using Server;
int port = 42069;
string IP = "127.0.0.1";

HTTPServer server = new HTTPServer(IP, port);
server.Start();

Console.ReadKey();