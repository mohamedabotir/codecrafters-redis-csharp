using System.Net;
using System.Net.Sockets;
using System.Text;

var ipAddress = new IPEndPoint(IPAddress.Any, 6379);
TcpListener tcp = new(ipAddress);
try
{
	tcp.Start();
	using TcpClient client= await tcp.AcceptTcpClientAsync();
    await using NetworkStream stream = client.GetStream();
    var message = $"📅 {DateTime.Now} 🕛";
    var dateTimeBytes = Encoding.UTF8.GetBytes(message);
    await stream.WriteAsync(dateTimeBytes);

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
