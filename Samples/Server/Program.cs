using System;
using System.Collections.Generic;
using System.Timers;
using NDde.Server;
using System.Threading.Tasks;
using System.Diagnostics;
using SocketIOClient;
using System.Text;
using Transport = SocketIOClient.Transport;

namespace Server
{
    public class Server
    {
        public static async Task Main(string[] args)
        {
            try
            {
                // Create a server that will register the service name 'myapp'.
                using (DdeServer server = new MyServer("VNI"))
                {
                    // Register the service name.
                    server.Register();

                    // Wait for the user to press ENTER before proceding.
                    Console.WriteLine("Press ENTER to quit...");
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Press ENTER to quit...");
                Console.ReadLine();
            }

            // Show Debug and Trace messages
            Console.OutputEncoding = Encoding.UTF8;
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

            var uri = new Uri("http://localhost:11003/");

            var socket = new SocketIO(uri, new SocketIOOptions
            {
                Transport = Transport.TransportProtocol.WebSocket,
                Query = new Dictionary<string, string>
                {
                    {"token", "V3" }
                },
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
            socket.OnAny((name, response) =>
            {
                Console.WriteLine(name);
                Console.WriteLine(response);
            });
            //socket.On("hi", response =>
            //{
            //    // Console.WriteLine(response.ToString());
            //    Console.WriteLine(response.GetValue<string>());
            //});

            //Console.WriteLine("Press any key to continue");
            //Console.ReadLine();

            await socket.ConnectAsync();

            Console.ReadLine();
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
            await socket.EmitAsync("1 params", Encoding.UTF8.GetBytes("test"));
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

            private void OnTimerElapsed(object sender, ElapsedEventArgs args)
            {
                // Advise all topic name and item name pairs.
                Advise("*", "*");
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
                if (format == 1)
                {
                    return new RequestResult(System.Text.Encoding.ASCII.GetBytes("Time=" + DateTime.Now.ToString() + "\0"));
                }
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
                if (format == 1)
                {
                    return System.Text.Encoding.ASCII.GetBytes("Time=" + DateTime.Now.ToString() + "\0");
                }
                return null;
            }

        } // class

    } // class

} // namespace
