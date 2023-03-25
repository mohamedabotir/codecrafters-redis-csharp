using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

var ipAddress = new IPEndPoint(IPAddress.Any, 6379);
TcpListener tcp = new(ipAddress);

try
{
    while (true)
    {
        tcp.Start();

        //new Thread(new ThreadStart(async() =>
        //{
        Task.Run(async () => await handleClientAsync(await tcp.AcceptTcpClientAsync()));
        //})).Start();
    }


}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);   
    throw;
}
finally
{
    //tcp.Stop();
}


static async Task handleClientAsync(TcpClient client)

{ 

        while (true)
{

        NetworkStream stream = client.GetStream();
    try
    {


        var windowSize = stream.ReadByte();
        if (windowSize != 0)
        {

            var message = $"";


            message += "+PONG\r\n";
            var dateTimeBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(dateTimeBytes);
        }
    }
    catch (Exception ex)
    {
        var dateTimeBytes = Encoding.UTF8.GetBytes("-Error");
        await stream.WriteAsync(dateTimeBytes);
            Console.WriteLine(ex.Message);
        throw;
    } 
}
        
}





