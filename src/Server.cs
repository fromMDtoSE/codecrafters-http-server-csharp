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

    var requestPath = GetRequestPath(request);
    var okStatusCode = IsValidRequest(requestPath);
    int reqestPathLength = requestPath.Length;
    string responseContent = GetResponseContent(requestPath, reqestPathLength, request);

    string[] args = Environment.GetCommandLineArgs();

    if (args.Length > 1 && args[1] == "--directory")
    {
        RespondToFileRequest(requestPath, request, args, stream);
    }
    else
    {
        var response = GenerateResponseBytes(okStatusCode, false, false, responseContent.Length, responseContent.Length, responseContent);
        stream.Write(response, 0, response.Length);
    }

    client.Close();
}

string GetRequestPath(string request) => request.Split(" ")[1];
bool IsValidRequest(string requestPath) => requestPath.Contains("/echo") || requestPath.Contains("/user-agent") || requestPath.Contains("/files");
string GetResponseContent(string requestPath, int reqestPathLength, string request) => requestPath.Contains("/echo")
    ? requestPath.Split("/echo/")[1] : requestPath.Contains("/user-agent")
    ? request.Split("User-Agent: ")[1].Split("\r\n")[0] : string.Empty;

void RespondToFileRequest(string requestPath, string request, string[] args, NetworkStream stream)
{
    string directoryPath = args[2];
    string filePath = requestPath.Split("/files/")[1];
    string fileFullPath = Path.Combine(directoryPath, filePath);
    bool requestWithFile = requestPath.Contains("/files");
    if (requestWithFile && request.Split(" ")[0].ToLower() == "post")
    {
        File.WriteAllText(fileFullPath, request.Split("\r\n\r\n")[1]);
        byte[] response = GenerateResponseBytes(true, true, true, 0, 0, "");
        stream.Write(response, 0, response.Length);
    }
    else if (requestWithFile && request.Split(" ")[0].ToLower() == "get")
    {
        if (File.Exists(fileFullPath))
        {
            byte[] fileContent = File.ReadAllBytes(fileFullPath);
            byte[] response = GenerateResponseBytes(true, true, false, fileContent.Length, 0, "");
            stream.Write(response, 0, response.Length);
            stream.Write(fileContent, 0, fileContent.Length);
        }
        else
        {
            byte[] response = GenerateResponseBytes(false, false, false, 0, 0, "");
            stream.Write(response, 0, response.Length);
        }
    }
}

byte[] GenerateResponseBytes(bool isSuccessResponse, bool respondWithFile, bool isPost, int fileContentLength, int responseContentLength, string responseContent) =>
isSuccessResponse && respondWithFile && isPost
? Encoding.UTF8.GetBytes($"HTTP/1.1 201 Created\r\n\r\n")
: isSuccessResponse && respondWithFile && !isPost
    ? Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {fileContentLength}\r\n\r\n")
    : isSuccessResponse && !respondWithFile && !isPost
        ? Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {responseContentLength}\r\n\r\n{responseContent}")
        : Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\nContent-Length: 9\r\n\r\nNot Found");