﻿using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

var ipAddress = new IPEndPoint(IPAddress.Loopback, 6379);
TcpListener tcp = new(ipAddress);

try
{
    while (true)
    {
        tcp.Start();

        //new Thread(new ThreadStart(async() =>
        //{
        Task.Run(async () => await handleClientAsync(await tcp.AcceptTcpClientAsync(),ipAddress));
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


static async Task handleClientAsync(TcpClient client, IPEndPoint ipAddress)
{
     
        client.ReceiveTimeout= 5000;
        client.SendTimeout= 5000;

   
        while (true)
        {
             NetworkStream stream = client.GetStream();
    try
    {

            var windowSize = stream.ReadByte();
            if (windowSize != 0)
            {

                var message = $"";
                var buffer = new byte[windowSize];
                await stream.ReadAsync(buffer);
                var encoder = Encoding.UTF8.GetString(buffer);
                var data = string.Join("", encoder.ToArray());

                if (data.Contains("ECHO"))
                {
                    var index = data.IndexOf("ECHO");
                    var echoStart = data[index];//+5+4
                    var INDEX = data.Length - (index + 10);
                    var echoedMessage = data.Substring(index + 10, INDEX);
              var result =   echoedMessage.Replace("\r\n", "").Replace("\0","");
                    message += $"${result.Length}\r\n{result}\r\n";
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(message));

                }
                else
                {

                message += "+PONG\r\n";
               await  stream.WriteAsync(Encoding.UTF8.GetBytes(message));
                }
            }

    }
    catch (Exception)
    {
        Console.WriteLine("-Error");
        await stream.WriteAsync(Encoding.UTF8.GetBytes("-Error"));

        throw;
    }


        }


} 





