using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
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

    byte[] response = Encoding.UTF8.GetBytes(okStatusCode ? $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {responseContent.Length}\r\n\r\n{responseContent}"
    : "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\nContent-Length: 9\r\n\r\nNot Found");

    string[] args = Environment.GetCommandLineArgs();

    if (args[1] == "--directory")
    {
        bool requestWithFile = requestPath.Contains("/files");
        if (requestWithFile)
        {
            string filePath = requestPath.Split("/files")[1];
            string fileFullPath = args[2] + filePath;
            if (File.Exists(filePath))
            {
                byte[] fileContent = File.ReadAllBytes(fileFullPath);
                response =
                    Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {fileContent.Length}\r\n\r\n");
            }
            else
            {
                response = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\nContent-Length: 9\r\n\r\nNot Found");
            }
        }
    }

    stream.Write(response, 0, response.Length);
    client.Close();
}