using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;


namespace GyroShooterClient
{
    public class GyroClient
    {
        StreamSocket sock;
        DataReader reader;
        DataWriter writer;
        static StreamSocketListener listener;

        public GyroClient()
        {
        }

        public async Task Connect(string hostName, int port = 28008)
        {
            sock = new StreamSocket();
            await sock.ConnectAsync(new Windows.Networking.HostName(hostName), port.ToString());
            InitReaders();
        }

        private void InitReaders()
        {
            reader = new DataReader(sock.InputStream);
            writer = new DataWriter(sock.OutputStream);
        }

        public static async void Listen(int port = 28008)
        {
            listener = new StreamSocketListener();
            listener.ConnectionReceived += listener_ConnectionReceived;

            await listener.BindServiceNameAsync(port.ToString());
        }

        public static event EventHandler<GyroClient> ClientConnected;

        public static void listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            GyroClient client = new GyroClient();
            client.sock = args.Socket;
            client.InitReaders();

            if (ClientConnected != null)
            {
                ClientConnected(null, client);
            }

        }

        public async void WriteCommand(string command, float value)
        {
            writer.WriteUInt32(writer.MeasureString(command));
            writer.WriteString(command);
            writer.WriteSingle(value);
            await writer.StoreAsync();
            await writer.FlushAsync();
        }

        public async Task<GyroCommand> GetCommand()
        {
            await reader.LoadAsync(sizeof(int));
            uint len = reader.ReadUInt32();
            await reader.LoadAsync(len);
            string command = reader.ReadString(len);
            await reader.LoadAsync(sizeof(float));
            float param = reader.ReadSingle();

            return new GyroCommand() { Value = param, Command = command};
        }
    }

    public class GyroCommand
    {
        public float Value { get; set; }
        public string Command { get; set; }

        public override string ToString()
        {
            return Command + " = " + Value;
        }
    }
}
