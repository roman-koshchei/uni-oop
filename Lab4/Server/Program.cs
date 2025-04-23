using Server;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

var PORT = 11000;
var BUFFER_SIZE = 8192;

var ipHost = Dns.GetHostEntry("localhost");
var ipAddress = ipHost.AddressList[0];
var localEndPoint = new IPEndPoint(ipAddress, PORT);
var runListener = true;

Socket listener = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

var db = new ConcurrentDictionary<Guid, Student>();

try
{
    listener.Bind(localEndPoint);
    listener.Listen(16);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"Lab 4 server started.");
    Console.WriteLine($"Waiting for connections on port {PORT}...");
    Console.ResetColor();

    while (runListener)
    {
        Console.WriteLine("Waiting for a new client connection...");

        Socket handler = listener.Accept();
        _ = Task.Run(async () =>
        {
            Console.WriteLine($"Client {handler.RemoteEndPoint} is connected!");

            var runClient = true;
            while (runClient)
            {
                byte[] buffer = new byte[BUFFER_SIZE];
                int bytesRead = await handler.ReceiveAsync(buffer);

                if (bytesRead > 0)
                {
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Client {handler.RemoteEndPoint} sent:");
                    Console.WriteLine(data);

                    try
                    {
                        var command = Command.Parse(data);

                        if (command.Name == "add")
                        {
                            var name = command.Arguments["name"];
                            var grade = int.Parse(command.Arguments["grade"]);

                            var student = new Student(name, grade);
                            var id = Guid.NewGuid();

                            if (db.TryAdd(id, student))
                            {
                                await SendString(handler, $"{id} | {student.Name} | {student.Grade}");
                            }
                            else
                            {
                                await SendString(handler, $"Can't add student to the database");
                            }
                        }
                        else if (command.Name == "edit")
                        {
                            var id = Guid.Parse(command.Arguments["id"]);
                            var grade = int.Parse(command.Arguments["grade"]);

                            if (db.TryGetValue(id, out var student))
                            {
                                if (db.TryUpdate(id, new Student(student.Name, grade), student))
                                {
                                    await SendString(handler, $"Student grade is successfully updated");
                                }
                                else
                                {
                                    await SendString(handler, $"Can't update student {student.Name}");
                                }
                            }
                            else
                            {
                                await SendString(handler, $"Student with {id} isn't found");
                            }
                        }
                        else if (command.Name == "remove")
                        {
                            var id = Guid.Parse(command.Arguments["id"]);

                            if (db.TryRemove(id, out var student))
                            {
                                await SendString(handler, $"Student {student.Name} is removed from database");
                            }
                            else
                            {
                                await SendString(handler, $"Can't remove student with id {id}");
                            }
                        }
                        else if (command.Name == "show")
                        {
                            if (!db.IsEmpty)
                            {
                                StringBuilder sb = new();
                                foreach (var (id, student) in db)
                                {
                                    sb.AppendLine($"{id} | {student.Name} | {student.Grade}");
                                }
                                await SendString(handler, sb.ToString().Trim('\n'));
                            }
                            else
                            {
                                await SendString(handler, "Database is empty");
                            }
                        }
                        else if (command.Name == "help")
                        {
                            await SendHelp(handler);
                        }
                        else if (command.Name == "close")
                        {
                            Console.WriteLine($"Closing connection with client {handler.RemoteEndPoint}");
                            runClient = false;
                        }
                        else
                        {
                            await SendString(handler, $"Command {command.Name} isn't supported");
                        }
                    }
                    catch (ParseCommandException ex)
                    {
                        await SendString(handler, ex.Message);
                    }
                    catch (Exception ex)
                    {
                        await SendString(handler, ex.Message);
                    }
                }
            }

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        });
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Server is shutting down, because of next error:");
    Console.WriteLine(ex);
    Console.ResetColor();
}
finally
{
    listener.Close();
}

static async Task SendString(Socket socket, string data)
{
    await socket.SendAsync(Encoding.UTF8.GetBytes(data));
}

static async Task SendHelp(Socket socket)
{
    var sb = new StringBuilder();
    sb.AppendLine("add --name {string} --grade {int}");
    sb.AppendLine("edit --id {string} --grade {int}");
    sb.AppendLine("remove --id {string}");
    sb.AppendLine("show");
    sb.AppendLine("close");

    await SendString(socket, sb.ToString().Trim('\n'));
}