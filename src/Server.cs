using System.Net;
using System.Net.Sockets;
using System.Text;

var ipAddress = new IPEndPoint(IPAddress.Any, 6379);
TcpListener tcp = new (ipAddress);
 
 try
{
    while (true)
    {
     tcp.Start();
 
        
        Task.Run(async ()=>await handleClientAsync(await tcp.AcceptTcpClientAsync(),tcp));
    }
        
     
}
catch (Exception)
{

	throw;
}
finally
{
    //tcp.Stop();
}
 

  static async Task handleClientAsync(TcpClient client, TcpListener tcp) {
    while (true)
    {

        NetworkStream stream = client.GetStream();

        var windowSize = stream.ReadByte();
        var message = $"";


        message += "+PONG\r\n";
        var dateTimeBytes = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(dateTimeBytes);


    }
     
}
