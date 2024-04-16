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

    byte[] response = new byte[0];

    string[] args = Environment.GetCommandLineArgs();
    bool sendFile = false;
    byte[] fileContent = new byte[0];

    if (args.Length > 1 && args[1] == "--directory")
    {
        bool requestWithFile = requestPath.Contains("/files");
        if (requestWithFile)
        {
            string directoryPath = args[2];
            string filePath = requestPath.Split("/files/")[1];
            string fileFullPath = Path.Combine(directoryPath, filePath);

            if (File.Exists(fileFullPath))
            {
                fileContent = File.ReadAllBytes(fileFullPath);
                sendFile = true;
                response = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {fileContent.Length}\r\n\r\n");
            }
            else
            {
                response = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\nContent-Length: 9\r\n\r\nNot Found");
            }
        }
        else
        {
            response = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\nContent-Length: 9\r\n\r\nNot Found");
        }
    }
    else
    {
        response = Encoding.UTF8.GetBytes(okStatusCode ? $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {responseContent.Length}\r\n\r\n{responseContent}"
        : "HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\nContent-Length: 9\r\n\r\nNot Found");
    }

    if (sendFile)
    {
        stream.Write(response, 0, response.Length);
        stream.Write(fileContent, 0, fileContent.Length);
    }
    else
    {
        stream.Write(response, 0, response.Length);
    }

    client.Close();
}