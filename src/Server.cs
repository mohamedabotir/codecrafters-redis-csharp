using System.Net;
using System.Net.Sockets;
using System.Text;

var ipAddress = new IPEndPoint(IPAddress.Any, 6379);
TcpListener tcp = new(ipAddress);
byte[] data  ;
 var windowSize = 0;
List<TcpClient> connectionsPool = new  ();
try
{

	tcp.Start();
            
    while (true)
    {
        
            await handleClientAsync(await tcp.AcceptTcpClientAsync());
        
    }
}
catch (Exception)
{

	throw;
}
finally
{
    tcp.Stop();
}

  static async Task handleClientAsync(TcpClient client) {
    while (true)
    {
        if(client.Connected)
        {

    await using NetworkStream stream = client.GetStream();

    var windowSize = stream.ReadByte();
    var message = $"";
    if (windowSize != 0)
    {

       var data = new byte[windowSize];
        await stream.ReadAsync(data);
        var result = string.Join("", data.ToArray());
        var splitting = Encoding.UTF8.GetString(data);
        var length = splitting.Split("\\n").Length;
        if (length > 1)
        {
            message += $"*{length}\r\n+PONG\r\n+PONG\r\n";
        }
        else
            message += "+PONG\r\n";
    }
    var dateTimeBytes = Encoding.UTF8.GetBytes(message);
    await stream.WriteAsync(dateTimeBytes);
        }
    }
}
