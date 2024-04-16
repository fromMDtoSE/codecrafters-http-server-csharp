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

        // read the stream and see if path is "/" to return 200 Ok else return 404 Not Found
        byte[] buffer = new byte[1024];
        await stream.ReadAsync(buffer, 0, buffer.Length);
        string request = Encoding.UTF8.GetString(buffer);
        string path = request.Split("\r\n")[0].Split(" ")[1];
        byte[] response = Encoding.UTF8.GetBytes(path == "/" ? "HTTP/1.1 200 OK\r\n\r\n" : "HTTP/1.1 404 Not Found\r\n\r\n");
        await stream.WriteAsync(response);
    }
}
finally
{
    server.Dispose();
}