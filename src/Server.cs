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
catch (Exception)
{

    throw;
}
finally
{
    //tcp.Stop();
}


static async Task handleClientAsync(TcpClient client)
{
    try
    {
        client.ReceiveTimeout= 0;
        client.SendTimeout= 0;


        while (true)
        {
             NetworkStream stream = client.GetStream();

            var windowSize = stream.ReadByte();
            if (windowSize != 0)
            {

                var message = $"";
               

                message += "+PONG\r\n";
                var dateTimeBytes = Encoding.UTF8.GetBytes(message);
              await  stream.WriteAsync(dateTimeBytes);
            }
           
        }
    }
    catch (Exception)
    {

        throw;
    }finally
    {
        client.Close(); 
        client.Dispose();
    }
} 





