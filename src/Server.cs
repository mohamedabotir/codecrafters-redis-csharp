﻿using Polly;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

var ipAddress = new IPEndPoint(IPAddress.Loopback, 6379);
TcpListener tcp = new(ipAddress);

try
{
        tcp.Start();
    while (true)
    {

        Thread clientThread = new Thread(() => HandleClient(tcp.AcceptTcpClient()));
        clientThread.Start();
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


//static void handleClientAsync(TcpClient client, IPEndPoint ipAddress)
//{


//             NetworkStream stream = client.GetStream();
//    while (true)
//        {
//    try
//    {

//                var buffer = new byte[1024];
//            var windowSize = stream.Read(buffer,0,1024);
//            if (windowSize != 0)
//            {

//                var message = $"";
//                var encoder = Encoding.UTF8.GetString(buffer,0,windowSize);
//                var data = string.Join("", encoder.ToArray());

//                if (data.Contains("ECHO"))
//                {
//                    var index = data.IndexOf("ECHO");
//                    var echoStart = data[index];//+5+4
//                    var INDEX = data.Length - (index + 10);
//                    var echoedMessage = data.Substring(index + 10, INDEX);
//              var result =   echoedMessage.Replace("\r\n", "").Replace("\0","");
//                    message += $"${result.Length}\r\n{result}\r\n";
//                    if(stream.CanWrite)
//                     stream.Write(Encoding.UTF8.GetBytes(message),0, Encoding.UTF8.GetBytes(message).Length);

//                }
//                else
//                {

//                message += "+PONG\r\n";
//                    if (stream.CanWrite)
//                        stream.Write(Encoding.UTF8.GetBytes(message), 0, Encoding.UTF8.GetBytes(message).Length);

//                }
//            }
//            else
//            {
//                break;
//            }
//            client.Close();

//    }
//    catch (Exception EX)
//    {
//        Console.WriteLine(EX.Message);

//        throw; 
//    }


//        }


//} 

static void HandleClient(TcpClient client)
{
    NetworkStream stream = client.GetStream();

    byte[] buffer = new byte[1024];
    int bytesRead;

    while (true)
    {
        bytesRead = 0;

        try
        {
            // Read from the stream
            bytesRead = stream.Read(buffer, 0, 1024);
        }
        catch
        {
            // A socket error has occurred
            break;
        }

        if (bytesRead == 0)
        {
            // The client has disconnected from the server
            break;
        }



        // Convert the data received into a string
       



        var encoder = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        var data = string.Join("", encoder.ToArray());

        if (data.Contains("ECHO"))
        {
            var index = data.IndexOf("ECHO");
            var echoStart = data[index];//+5+4
            var INDEX = data.Length - (index + 10);
            var echoedMessage = data.Substring(index + 10, INDEX);
            var result = echoedMessage.Replace("\r\n", "").Replace("\0", "");
          var  message = $"${result.Length}\r\n{result}\r\n";
            if (stream.CanWrite)
                stream.Write(Encoding.UTF8.GetBytes(message), 0, Encoding.UTF8.GetBytes(message).Length);
        }
        else
        {


            // Echo the data back to the client
            byte[] dataSent = Encoding.UTF8.GetBytes("+PONG\r\n");
        stream.Write(dataSent, 0, dataSent.Length);
         }
    }

    // Clean up the client connection
    client.Close();
    Console.WriteLine("Client disconnected.");
}





