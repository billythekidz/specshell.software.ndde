using System;
using System.Text;
using System.Timers;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

using NDde.Server;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using Transport = SocketIOClient.Transport;
using System.Xml.Linq;
using Newtonsoft.Json;
//using System.Text.Json.Serialization;

namespace Server
{
    public struct Tick
    {
        public long time;
        public string symbol;
        public double price;
        public long volume;
        public int digits;

        public string Price => price.ToString("F" + digits);
        public string Volume => volume.ToString();
        public DateTime Time => DateTime.FromBinary(time);
    }
    public class Server
    {
        private static MyServer _server;
        //private static MyServer DDE { get
        //    {
        //        if (_server == null)
        //        {

        //        }
        //        return _server;
        //    }
        //}
        private static Dictionary<string, Tick> _lastTicks = new Dictionary<string, Tick>();
        public static async Task Main(string[] args)
        {            
            // Show Debug and Trace messages
            Console.OutputEncoding = Encoding.UTF8;
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            var uri = new Uri("http://localhost:11003/");

            var socket = new SocketIO(uri
            , new SocketIOOptions
              {
                  Transport = Transport.TransportProtocol.WebSocket,
                AutoUpgrade = true,
                EIO = 4
                //  Query = new Dictionary<string, string>
                //{
                //    {"token", "V3" }
                //},
            });
            //var uri = new Uri("http://localhost:11002/");

            //var socket = new SocketIO(uri, new SocketIOOptions
            //{
            //    Transport = Transport.TransportProtocol.Polling,
            //    AutoUpgrade = false,
            //    EIO = 3,
            //    Query = new Dictionary<string, string>
            //    {
            //        {"token", "V2" }
            //    },
            //});

            socket.OnConnected += Socket_OnConnected;
            //socket.OnPing += Socket_OnPing;
            //socket.OnPong += Socket_OnPong;
            socket.OnDisconnected += Socket_OnDisconnected;
            socket.OnReconnectAttempt += Socket_OnReconnecting;
            socket.OnAny(Socket_TickReceived);
            //socket.On("hi", response =>
            //{
            //    // Console.WriteLine(response.ToString());
            //    Console.WriteLine(response.GetValue<string>());
            //});

            //Console.WriteLine("Press any key to continue");
            //Console.ReadLine();

            //await
            socket.ConnectAsync();
            try
            {
                // Create a server that will register the service name 'myapp'.
                _server = new MyServer("VNI");
                _server.Register();
                // Register the service name.                    
                // Wait for the user to press ENTER before proceding.
                Console.WriteLine("Press ENTER to quit...");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Press ENTER to quit...");
                Console.ReadLine();
            }
            Console.ReadLine();
        }
        private static void Socket_TickReceived(string name, SocketIOResponse response)
        {
            //var tick = response.GetValue<Tick>();
            //tick.symbol = "A";
            //tick.price = 1000;
            //tick.volume = 1;
            //tick.time = 1900000;
            //tick.digits = 1;
            var jsonString = response.GetValue().ToString();
            var tick = JsonConvert.DeserializeObject<Tick>(jsonString);
            //Console.WriteLine("tick: " + JsonConvert.SerializeObject(tick));
            Console.WriteLine("tick: " + tick.symbol);
            Console.WriteLine("tick: " + jsonString);
            if (name != tick.symbol) return;
            _server.TickReceived(tick);
            //long time = 
            //string symbol;
            //double price;
            //long volume;
            //int digits;
            //DDE.TickReceived();
        }
        private static void Socket_OnReconnecting(object sender, int e)
        {
            Console.WriteLine($"{DateTime.Now} Reconnecting: attempt = {e}");
        }

        private static void Socket_OnDisconnected(object sender, string e)
        {
            Console.WriteLine("disconnect: " + e);
        }

        private static async void Socket_OnConnected(object sender, EventArgs e)
        {
            Console.WriteLine("Socket_OnConnected");
            var socket = sender as SocketIO;
            Console.WriteLine("Socket.Id:" + socket.Id);

            //while (true)
            //{
            //    await Task.Delay(1000);
            //await socket.EmitAsync("hi", DateTime.Now.ToString());
            //await socket.EmitAsync("welcome");
            //await socket.EmitAsync("1 params", Encoding.UTF8.GetBytes("test"));
            //}
            //byte[] bytes = Encoding.UTF8.GetBytes("ClientCallsServerCallback_1Params_0");
            //await socket.EmitAsync("client calls the server's callback 1", bytes);
            //await socket.EmitAsync("1 params", Encoding.UTF8.GetBytes("hello world"));
        }

        private static void Socket_OnPing(object sender, EventArgs e)
        {
            Console.WriteLine("Ping");
        }

        private static void Socket_OnPong(object sender, TimeSpan e)
        {
            Console.WriteLine("Pong: " + e.TotalMilliseconds);
        }
        private sealed class MyServer : DdeServer
        {
            //private System.Timers.Timer _Timer = new System.Timers.Timer();

            public MyServer(string service) : base(service)
            {
                // Create a timer that will be used to advise clients of new data.
                //_Timer.Elapsed += this.OnTimerElapsed;
                //_Timer.Interval = 1000;
                //_Timer.SynchronizingObject = this.Context;                
            }

            //private void OnTimerElapsed(object sender, ElapsedEventArgs args)
            //{
            //    // Advise all topic name and item name pairs.
            //    Advise("*", "*");
            //}

            public void TickReceived(Tick tick)//long time, string symbol, double price, long volume, int digits=2)
            {
                //if (string.IsNullOrEmpty(tick.symbol)) return;
                //Tick tick;
                //tick.time = time;
                //tick.symbol = symbol;
                //tick.price = price;
                //tick.volume = volume;
                //tick.digits = digits;
                Console.WriteLine(tick.symbol);
                Console.WriteLine(tick.price);
                //if (!_lastTicks.ContainsKey(tick.symbol) || tick.time > _lastTicks[tick.symbol].time)
                {
                    _lastTicks[tick.symbol] = tick;
                    //Advise("Price", tick.symbol);
                    //Advise("Volume", tick.symbol);
                    // Advise all topic name and item name pairs.
                    Advise("*", "*");
                }
            }

            public override void Register()
            {
                base.Register();
                //_Timer.Start();
            }

            public override void Unregister()
            {
                //_Timer.Stop();
                base.Unregister();
            }

            protected override bool OnBeforeConnect(string topic)
            {
                Console.WriteLine("OnBeforeConnect:".PadRight(16)
                    + " Service='" + base.Service + "'"
                    + " Topic='" + topic + "'");

                return true;
            }

            protected override void OnAfterConnect(DdeConversation conversation)
            {
                Console.WriteLine("OnAfterConnect:".PadRight(16)
                    + " Service='" + conversation.Service + "'"
                    + " Topic='" + conversation.Topic + "'"
                    + " Handle=" + conversation.Handle.ToString());
            }

            protected override void OnDisconnect(DdeConversation conversation)
            {
                Console.WriteLine("OnDisconnect:".PadRight(16)
                    + " Service='" + conversation.Service + "'"
                    + " Topic='" + conversation.Topic + "'"
                    + " Handle=" + conversation.Handle.ToString());
            }

            protected override bool OnStartAdvise(DdeConversation conversation, string item, int format)
            {
                Console.WriteLine("OnStartAdvise:".PadRight(16)
                    + " Service='" + conversation.Service + "'"
                    + " Topic='" + conversation.Topic + "'"
                    + " Handle=" + conversation.Handle.ToString()
                    + " Item='" + item + "'"
                    + " Format=" + format.ToString());

                // Initiate the advisory loop only if the format is CF_TEXT.
                return format == 1;
            }

            protected override void OnStopAdvise(DdeConversation conversation, string item)
            {
                Console.WriteLine("OnStopAdvise:".PadRight(16)
                    + " Service='" + conversation.Service + "'"
                    + " Topic='" + conversation.Topic + "'"
                    + " Handle=" + conversation.Handle.ToString()
                    + " Item='" + item + "'");
            }

            protected override ExecuteResult OnExecute(DdeConversation conversation, string command)
            {
                Console.WriteLine("OnExecute:".PadRight(16)
                    + " Service='" + conversation.Service + "'"
                    + " Topic='" + conversation.Topic + "'"
                    + " Handle=" + conversation.Handle.ToString()
                    + " Command='" + command + "'");

                // Tell the client that the command was processed.
                return ExecuteResult.Processed;
            }

            protected override PokeResult OnPoke(DdeConversation conversation, string item, byte[] data, int format)
            {
                Console.WriteLine("OnPoke:".PadRight(16)
                    + " Service='" + conversation.Service + "'"
                    + " Topic='" + conversation.Topic + "'"
                    + " Handle=" + conversation.Handle.ToString()
                    + " Item='" + item + "'"
                    + " Data=" + data.Length.ToString()
                    + " Format=" + format.ToString());

                // Tell the client that the data was processed.
                return PokeResult.Processed;
            }

            protected override RequestResult OnRequest(DdeConversation conversation, string item, int format)
            {
                Console.WriteLine("OnRequest:".PadRight(16)
                    + " Service='" + conversation.Service + "'"
                    + " Topic='" + conversation.Topic + "'"
                    + " Handle=" + conversation.Handle.ToString()
                    + " Item='" + item + "'"
                    + " Format=" + format.ToString());

                // Return data to the client only if the format is CF_TEXT.
                if (_lastTicks.ContainsKey(item))
                {
                    //var Price = BitConverter.GetBytes(_lastTicks[item].price);//price.ToString("F" + _lastTicks[item].digits);
                    //var Volume = BitConverter.GetBytes(_lastTicks[item].volume);//volume.ToString();
                    //public DateTime Time => DateTime.FromBinary(time);
                    var Price = System.Text.Encoding.ASCII.GetBytes(_lastTicks[item].Price);
                    var Volume = System.Text.Encoding.ASCII.GetBytes(_lastTicks[item].Volume);
                    Console.WriteLine(_lastTicks[item].Price + " " + _lastTicks[item].Volume);
                    if (conversation.Topic == "Price")
                        return new RequestResult(Price);
                    if (conversation.Topic == "Volume")
                        return new RequestResult(Volume);
                }
                //   if (format == 1)
                //{
                //    return new RequestResult(System.Text.Encoding.ASCII.GetBytes("Time=" + DateTime.Now.ToString() + "\0"));
                //}
                return RequestResult.NotProcessed;
            }

            protected override byte[] OnAdvise(string topic, string item, int format)
            {
                Console.WriteLine("OnAdvise:".PadRight(16)
                    + " Service='" + this.Service + "'"
                    + " Topic='" + topic + "'"
                    + " Item='" + item + "'"
                    + " Format=" + format.ToString());

                // Send data to the client only if the format is CF_TEXT.
                if (_lastTicks.ContainsKey(item))
                {
                    //var Price = BitConverter.GetBytes(_lastTicks[item].price);//price.ToString("F" + _lastTicks[item].digits);
                    //var Volume = BitConverter.GetBytes(_lastTicks[item].volume);//volume.ToString();
                    var Price = System.Text.Encoding.ASCII.GetBytes(_lastTicks[item].Price);
                    var Volume = System.Text.Encoding.ASCII.GetBytes(_lastTicks[item].Volume);
                    //public DateTime Time => DateTime.FromBinary(time);
                    Console.WriteLine(_lastTicks[item].Price + " " + _lastTicks[item].Volume);
                    if (topic == "Price")
                        return Price;
                    if (topic == "Volume")
                        return Volume;
                    //return System.Text.Encoding.ASCII.GetBytes("Time=" + DateTime.Now.ToString() + "\0");
                }
                return null;
            }

        } // class

    } // class

} // namespace
