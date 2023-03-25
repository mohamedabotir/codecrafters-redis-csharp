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

        //new Thread(new ThreadStart(async() =>
        //{
          Task.Run (async()=>await handleClientAsync(await tcp.AcceptTcpClientAsync()));
        //})).Start();
     }
        
     
}
catch (Exception)
{
    Console.WriteLine("-Error");

    throw;
}
finally
{
    //tcp.Stop();
}
 

  static async Task handleClientAsync(TcpClient client) {
    try
    {



        while (true)
        {
            var message = $"";
            

            NetworkStream stream = client.GetStream();

            var windowSize = stream.ReadByte();
            if (windowSize != 0)
            {
                

                message += "+PONG\r\n";
               
                var dateTimeBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(dateTimeBytes);
            }
            
        }
    }
    catch (Exception EX)
        {
        Console.WriteLine("-Error");
        }
    }
         
     
      
       
        
