using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Timer = System.Timers.Timer;

internal class Program
{
    public static ConcurrentDictionary<string, object> _cache = new();
    public static ConcurrentDictionary<string, Timer> expiration = new();
    private static void Main(string[] args)
    {
        var ipAddress = new IPEndPoint(IPAddress.Any, 6379);
        TcpListener tcp = new(ipAddress);
        try
        {
            while (true)
            {
                tcp.Start();

                //new Thread(new ThreadStart(async() =>
                //{
                Task.Run(() => HandleClient(tcp.AcceptTcpClient(), _cache, expiration));
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


        static void AddExpiration(string key, TimeSpan expirationPeriod, ConcurrentDictionary<string, object> _cache, ConcurrentDictionary<string, Timer> expiration)
        {
            Timer val;
            if (expiration.ContainsKey(key))
            {

                expiration[key].Stop();
                expiration[key].Dispose();
                expiration.Remove(key, out val);
            }

            var timer = new Timer(expirationPeriod.TotalSeconds);


            timer.Elapsed += (sender, arg) =>
            {
                removeKey(key, _cache, expiration);
            };
            timer.Start();
            if (!expiration.ContainsKey(key))
                expiration.TryAdd(key, timer);
        }

        static void removeKey(string key, ConcurrentDictionary<string, object> _cache, ConcurrentDictionary<string, Timer> expiration)
        {
            lock (_cache)
            {
                Timer timerVal;
                object cachedVal = default(object);
                if (_cache.ContainsKey(key))
                {
                    _cache.Remove(key, out cachedVal);
                }

                if (expiration.ContainsKey(key))
                {
                    expiration[key].Stop();
                    expiration[key].Dispose();
                    expiration.Remove(key, out timerVal);
                }
                Console.WriteLine($"{key} EXpired and deleted {cachedVal}");
            }

        }

        static void HandleClient(TcpClient client, ConcurrentDictionary<string, object> _cache, ConcurrentDictionary<string, Timer> expirationSource)
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




                var encoder = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                var data = string.Join("", encoder.ToArray());
                if (data.Contains("set", StringComparison.OrdinalIgnoreCase))
                {

                    var result = data.Replace("\r\n", "").Replace("\0", "");
                    var index = result.IndexOf("set");//4+3
                    var command = result.Substring(index, 3);

                    var indexKey = index + 3 + 2;
                    var indexOfValue = result.IndexOf('$', indexKey);
                    var indexKeyValue = result.Substring(indexKey, indexOfValue - indexKey);
                    var endOfKeyValue = result.Length - indexOfValue;
                    var KeyWithExpirationValue = result.Substring(indexOfValue + 2, endOfKeyValue - 2);
                    var expirationSegmentIndex = KeyWithExpirationValue.IndexOf("$");
                    // var message = $"${result.Length}\r\n{result}\r\n";
                    var KeyValue = expirationSegmentIndex == -1 ? KeyWithExpirationValue : KeyWithExpirationValue.Substring(0, expirationSegmentIndex);
                    if (expirationSegmentIndex != -1)
                    {

                        var expiration = KeyWithExpirationValue.Substring(expirationSegmentIndex + 2, KeyWithExpirationValue.Length - (expirationSegmentIndex + 2));
                        var expirationPeriodIndex = expiration.IndexOf("x", StringComparison.OrdinalIgnoreCase) + 3;
                        var period = expiration.Length - expirationPeriodIndex;
                        var ExpirationValue = Convert.ToInt32(expiration.Substring(expirationPeriodIndex, period));
                        AddExpiration(indexKeyValue, TimeSpan.FromSeconds(ExpirationValue), _cache, expirationSource);
                    }
                    _cache.AddOrUpdate(indexKeyValue, KeyValue, (key, old) => KeyValue);

                    if (stream.CanWrite)
                        stream.Write(Encoding.ASCII.GetBytes("+OK\r\n"), 0, Encoding.ASCII.GetBytes("+OK\r\n").Length);


                }
                else if (data.Contains("get", StringComparison.OrdinalIgnoreCase))
                {
                    var result = data.Replace("\r\n", "").Replace("\0", "");
                    var index = result.IndexOf("get");//4+3
                    var command = result.Substring(index, 3);

                    var indexKey = index + 3 + 2;
                    var indexOfValue = result.Length - indexKey;
                    var indexKeyValue = result.Substring(indexKey, indexOfValue);
                    // var message = $"${result.Length}\r\n{result}\r\n";
                    if (_cache.ContainsKey(indexKeyValue))
                    {
                        var value = (string)_cache[indexKeyValue];
                        if (stream.CanWrite)
                            stream.Write(Encoding.ASCII.GetBytes($"${value.Length}\r\n{value}\r\n"), 0, Encoding.ASCII.GetBytes($"${value.Length}\r\n{value}\r\n").Length);

                    }
                    else

                        //stream.Write(Encoding.ASCII.GetBytes("-Error invalid Key\r\n"), 0, Encoding.ASCII.GetBytes("-Error invalid Key\r\n").Length); 
                        stream.Write(Encoding.ASCII.GetBytes("$-1\r\n"), 0, Encoding.ASCII.GetBytes("$-1\r\n").Length);
                }
                else if (data.Contains("echo"))
                {
                    var index = data.IndexOf("echo");
                    var echoStart = data[index];//+5+4
                    var INDEX = data.Length - (index + 10);
                    var echoedMessage = data.Substring(index + 10, INDEX);
                    var result = echoedMessage.Replace("\r\n", "").Replace("\0", "");
                    var message = $"${result.Length}\r\n{result}\r\n";
                    if (stream.CanWrite)
                        stream.Write(Encoding.ASCII.GetBytes(message), 0, Encoding.ASCII.GetBytes(message).Length);
                }
                else
                {


                    // Echo the data back to the client
                    byte[] dataSent = Encoding.ASCII.GetBytes("+PONG\r\n");
                    stream.Write(dataSent, 0, dataSent.Length);
                }
            }

            // Clean up the client connection
            client.Close();
            Console.WriteLine("Client disconnected.");
        }
    }
}