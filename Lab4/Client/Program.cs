using System.Net;
using System.Net.Sockets;
using System.Text;

Console.WriteLine("Client app started.");
Console.WriteLine("help - get all commands");
Console.WriteLine("Please enter commands in the next format:");
Console.WriteLine("Example of correct command:");
Console.WriteLine("command --arg-name-1 value --arg-name-2 another value");
Console.WriteLine("add --name Roman Koshchei --grade 100");
Console.WriteLine("Value goes as long as it meets end of the command or another key, no need for \"");

var SERVER_IP = "localhost";
var SERVER_PORT = 11000;

var BUFFER_SIZE = 8192;

var ipHost = Dns.GetHostEntry(SERVER_IP);
var ipAddress = ipHost.AddressList[0];
var remoteEndPoint = new IPEndPoint(ipAddress, SERVER_PORT);

Socket sender = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

sender.Connect(remoteEndPoint);

var run = true;
while (run)
{
    string message = (Console.ReadLine() ?? "").Trim();
    byte[] buffer = Encoding.UTF8.GetBytes(message);

    _ = await sender.SendAsync(buffer);

    if (message == "close")
    {
        run = false;
    }
    else
    {
        buffer = new byte[BUFFER_SIZE];
        _ = await sender.ReceiveAsync(buffer);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(Encoding.UTF8.GetString(buffer));
        Console.ResetColor();
    }
}

sender.Shutdown(SocketShutdown.Both);
sender.Close();