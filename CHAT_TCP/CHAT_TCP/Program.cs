using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Text;


ServerResender server = new ServerResender();
await server.ListenAsync();

class ServerResender
{
    public List<ClientEx> clientList = new List<ClientEx>();
    TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888);

    static int idc = 1;

    public async Task ListenAsync()
    {
        try
        {
            tcpListener.Start();
            Console.WriteLine("Сервер Запущен, ожидает подключает...");

            while (true)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                
                ClientEx clientEx = new ClientEx(tcpClient, this, idc);
                clientList.Add(clientEx);
                Task receiveTask = Task.Run(clientEx.ProcessAsync);
                idc++;
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine("\tОшибка подключения\n" + ex.ToString());
        }
        finally
        {
            Disconect();  
        }
    }

    public void Disconect()
    {
        foreach (ClientEx cli in clientList)
        {
            cli.Close();
        }
        tcpListener.Stop();
    }

    protected internal void RemoveCon(int idrem)
    {
        ClientEx? client = clientList.FirstOrDefault(c => c.id == idrem);

        if (client != null) clientList.Remove(client);
        client?.Close();
    }

    public async Task MessageResendAsync(string messa, int idsend)
    {
        foreach (var client in clientList)
        {
            if (client.id != idsend)
            {
                await client.Writer.WriteLineAsync(client.IPAddr.ToString() + ": " + messa);
                await client.Writer.FlushAsync();
            }
        }
    }

    public async Task MessageIPResendAsync(string messa, int idsend)
    {
        foreach (var client in clientList)
        {
            if (client.id == idsend)
            {
                await client.Writer.WriteLineAsync(messa);
                await client.Writer.FlushAsync();
            }
        }
    }
}

class ClientEx
{
    public int id;
    public StreamReader Reader { get; set; }
    public StreamWriter Writer { get; set; }
    public IPAddress IPAddr { get; set; }

    TcpClient client;
    ServerResender server;


    public ClientEx(TcpClient tcpc, ServerResender servre, int id)
    {
        this.id = id;
        client = tcpc;
        server = servre;
        IPAddr = ((IPEndPoint)tcpc.Client.RemoteEndPoint).Address;

        NetworkStream netst = client.GetStream();

        Writer = new StreamWriter(netst);
        Reader = new StreamReader(netst);
    }

    public void Close()
    {
        Writer.Close();
        Reader.Close();
        client.Close();
    }

    public async Task ProcessAsync()
    {
        try
        {
            StringBuilder allip = new StringBuilder();

            foreach(ClientEx cl in server.clientList)
            {
                allip.Append(cl.IPAddr + "\n");
            }

            string helloMess = $"{IPAddr} присоединился в чат\n\n\nподключенные IP адреса\n" + allip + "\n";

            await server.MessageResendAsync(helloMess, id);
            await server.MessageIPResendAsync(helloMess, id);
            Console.WriteLine(helloMess);

            while(true)
            {
                try
                {
                    helloMess = await Reader.ReadLineAsync();
                    if (helloMess == null) continue;

                    string mess = $"{IPAddr.ToString()}: {helloMess}";

                    await server.MessageResendAsync(helloMess, id);
                    Console.WriteLine(mess);
                }
                catch
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            server.RemoveCon(id);

            StringBuilder allip = new StringBuilder();
            foreach (ClientEx cl in server.clientList)
            {
                allip.Append(cl.IPAddr + "\n");
            }

            string outMess = $"{IPAddr.ToString()} покинул чат\n\n\n\nподключенные IP адреса\n" + allip + "\n";
            await server.MessageResendAsync(outMess, id);

            Console.WriteLine(outMess);
        }
    }
}
