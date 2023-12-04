using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var ws = new ClientWebSocket();
        string name;
        Console.Write("text your username:  ");
        while (true)
        {
            name = Console.ReadLine();
            break;
        };

        Console.WriteLine("Connecting to server...");
        await ws.ConnectAsync(new Uri($"ws://localhost:5146/api/chat?name={name}"), CancellationToken.None);
        Console.WriteLine("Connected");

        using var receiveTask = Task.Run(async () =>
        {
            var buffer = new byte[1024 * 4];
            while (true)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine("Received: " + message);
            }
        });

        // Send a message to the server
        string initialMessage = "Hello, server!";
        byte[] initialMessageBytes = Encoding.UTF8.GetBytes(initialMessage);
        await ws.SendAsync(new ArraySegment<byte>(initialMessageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

        // Keep the console application running until the user decides to exit
        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();

        // Close the WebSocket connection
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);

        // Wait for the receiveTask to complete
        await receiveTask;
    }
}
