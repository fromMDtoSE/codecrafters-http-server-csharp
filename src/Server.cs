using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);

try
{
    server.Start();
    while (true)
    {
        using var tcpClientHandler = await server.AcceptTcpClientAsync();
        await using NetworkStream stream = tcpClientHandler.GetStream();

        var response = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
        await stream.WriteAsync(response);
    }
}
finally
{
    server.Dispose();
}