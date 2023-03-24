using System.Net;
using System.Net.Sockets;
using System.Text;

var ipAddress = new IPEndPoint(IPAddress.Any, 6379);
TcpListener tcp = new (ipAddress);
 
 try
{
     tcp.Start();
    while (true)
    {
 
        
        Task.Run(async ()=>await handleClientAsync(await tcp.AcceptTcpClientAsync(),tcp));
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
 

  static async Task handleClientAsync(TcpClient client, TcpListener tcp) {
    while (true)
    {
       
     NetworkStream stream = client.GetStream();

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
            Console.WriteLine(message);
            var dateTimeBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(dateTimeBytes);
        }
        else
        {


           
            client.Close();
            
        }
        
    }
}
