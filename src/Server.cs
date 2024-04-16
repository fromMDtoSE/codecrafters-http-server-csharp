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
        TcpClient tcpClientHandler = await server.AcceptTcpClientAsync();
        _ = Task.Run(() => HandleRequest(tcpClientHandler));
    }
}
finally
{
    server.Dispose();
}

void HandleRequest(TcpClient client)
{
    using NetworkStream stream = client.GetStream();
    byte[] buffer = new byte[1024];
    int bytesRead = stream.Read(buffer, 0, buffer.Length);
    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

    string requestPath = request.Split(" ")[1];
    bool okStatusCode = requestPath == "/" || requestPath.Contains("/echo") || requestPath.Contains("/user-agent");
    int reqestPathLength = requestPath.Length;

    string responseContent = requestPath.Contains("/echo")
    ? requestPath.Split("/echo/")[1] : requestPath.Contains("/user-agent")
    ? request.Split("User-Agent: ")[1].Split("\r\n")[0] : string.Empty;

    Console.WriteLine(responseContent);

    byte[] response = Encoding.UTF8.GetBytes(okStatusCode ? $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {responseContent.Length}\r\n\r\n{responseContent}"
    : "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\nContent-Length: 9\r\n\r\nNot Found");

    stream.Write(response, 0, response.Length);
    client.Close();
}