using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Uri serverUri = new("ws://localhost:5146/api/chat?name={name}");

        using (ClientWebSocket clientWebSocket = new())
        {
            try
            {
                await clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);

                _ = ReceiveMessage(clientWebSocket);

                while (true)
                {
                    Console.Write("Text your message! (or 'exit' to get out): ");
                    string name = Console.ReadLine();

                    if (name.ToLower() == "exit")
                        break;

                    await SendMessage(clientWebSocket, name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
            finally
            {
                if (clientWebSocket.State == WebSocketState.Open)
                    await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing your connection", CancellationToken.None);
            }
        }
    }

    static async Task SendMessage(ClientWebSocket webSocket, string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    static async Task ReceiveMessage(ClientWebSocket webSocket)
    {
        byte[] buffer = new byte[1024];
        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Recived message: {receivedMessage}");
            }
        }
    }
}
