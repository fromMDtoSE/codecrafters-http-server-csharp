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

        // read the stream and echo back with the random string given
        byte[] buffer = new byte[1024];
        await stream.ReadAsync(buffer, 0, buffer.Length);
        string request = Encoding.UTF8.GetString(buffer);

        // string requestPath = request.Split(" ")[1];
        // bool basePathOrEcho = requestPath == "/" || requestPath.Contains("/echo");
        // int reqestPathLength = requestPath.Length;

        // string randomStringFromRequest = requestPath.Contains("/echo") ? requestPath.Split("/echo/")[1] : string.Empty;
        // Console.WriteLine(randomStringFromRequest);

        // byte[] response = Encoding.UTF8.GetBytes(basePathOrEcho ? $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {randomStringFromRequest.Length}\r\n\r\n{randomStringFromRequest}"
        // : "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\nContent-Length: 9\r\n\r\nNot Found");

        string userAgent = request.Split("User-Agent: ")[1];
        Console.WriteLine(userAgent);

        byte[] response = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgent.Length}\r\n\r\n{userAgent}");
        await stream.WriteAsync(response);
    }
}
finally
{
    server.Dispose();
}