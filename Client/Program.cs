using System.Net.Sockets;

Console.WriteLine("Добро пожаловать в клиент чата, введите IP адресс сервера в локальной сети:");
string host = Console.ReadLine();
int port = 8888;
using TcpClient client = new TcpClient();

Console.WriteLine("Добро пожаловать в чат");

StreamReader? Reader = null;
StreamWriter? Writer = null;

try
{
    client.Connect(host, port);

    Reader = new StreamReader(client.GetStream());
    Writer = new StreamWriter(client.GetStream());

    if (Writer is null || Reader is null) return;

    Task receiveTask = Task.Run(() => ReceiveMessageAsync(Reader));

    await SendMessageAsync(Writer);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
Writer?.Close();
Reader?.Close();

void Print(string message)
{
    var position = Console.GetCursorPosition();
    int left = position.Left;
    int top = position.Top;

    /*Console.MoveBufferArea(0, top, left, 1, 0, top + 1);*/
    Console.SetCursorPosition(0, top);
    Console.WriteLine(message);
    Console.SetCursorPosition(left, top + 1);
}

async Task SendMessageAsync(StreamWriter writer)
{
    Console.WriteLine("Для отправки сообщений введите сообщение и нажмите Enter");

    while (true)
    {
        Console.Write("вы: ");
        string? message = Console.ReadLine();
        await writer.WriteLineAsync(message);
        await writer.FlushAsync();
    }
}

async Task ReceiveMessageAsync(StreamReader reader)
{
    while (true)
    {
        try
        {
            string? message = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(message)) continue;
            Print(message);
        }
        catch
        {
            break;
        }
    }
}