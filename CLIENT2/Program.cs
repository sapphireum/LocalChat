using System.Net.Sockets;

string host = "127.0.0.1";
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

    Task.Run(() => ReceiveMessageAsync(Reader));

    await SendMessageAsync(Writer);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
Writer?.Close();
Reader?.Close();


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

            // message == null || message == ""
            if (string.IsNullOrEmpty(message)) continue;

            Print(message);
        }
        catch
        {
            break;
        }
    }
}


// чтобы вывод не накладывался на ввод нового сообщения
void Print(string message)
{
    var position = Console.GetCursorPosition(); // получаем текущую позицию курсора
    int left = position.Left;                   // смещение в символах относительно левого края
    int top = position.Top;                     // смещение в строках относительно верха

    // копируем ранее введенные символы в строке на следующую строку
    Console.MoveBufferArea(0, top, left, 1, 0, top + 1);

    // устанавливаем курсор в начало текущей строки
    Console.SetCursorPosition(0, top);

    // в текущей строке выводит полученное сообщение
    Console.WriteLine(message);

    // переносим курсор на следующую строку
    // и пользователь продолжает ввод уже на следующей строке
    Console.SetCursorPosition(left, top + 1);
}