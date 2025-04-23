using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClientGui;

public class Client
{
    public Client()
    {
        var SERVER_IP = "localhost";
        var SERVER_PORT = 11000;

        var BUFFER_SIZE = 8192;

        var ipHost = Dns.GetHostEntry(SERVER_IP);
        var ipAddress = ipHost.AddressList[0];
        var remoteEndPoint = new IPEndPoint(ipAddress, SERVER_PORT);

        Socket sender = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        sender.Connect(remoteEndPoint);
    }

    public async Task<string> Send(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        _ = await sender.SendAsync(buffer);

        buffer = new byte[BUFFER_SIZE];
        _ = await sender.ReceiveAsync(buffer);
    }
}