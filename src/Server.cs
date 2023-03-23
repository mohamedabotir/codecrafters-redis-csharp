using System.Net;
using System.Net.Sockets;
using System.Text;

var ipAddress = new IPEndPoint(IPAddress.Any, 6379);
TcpListener tcp = new(ipAddress);
byte[] data  ;
string result = "";
var windowSize = 0;
try
{
	tcp.Start();
	using TcpClient client= await tcp.AcceptTcpClientAsync();
    await using NetworkStream stream = client.GetStream();
    windowSize = stream.ReadByte();
    var message = $"" ;
    if (windowSize != 0)
    {

    data = new byte[windowSize];
    await stream.ReadAsync(data);
      //result = string.Join(" ", data.ToArray());
       // message += $"{Encoding.UTF8.GetString(data)}";
        message+= "+PONG\r\n";
    }
    var dateTimeBytes = Encoding.UTF8.GetBytes(message);
    await stream.WriteAsync(dateTimeBytes);
    Console.ReadKey();
    Console.WriteLine($"Sent message: \"{message}\"");
}
catch (Exception)
{

	throw;
}
finally
{
    tcp.Stop();
}
