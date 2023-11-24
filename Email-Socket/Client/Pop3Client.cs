using System.Net;
using System.Net.Sockets;
using System.Text;
using EmailHandler;
using EmailSocket;
using System.Text.RegularExpressions;

namespace POP3
{
    class Message_Box
    {
        // bool is to check whether the message is read
        // string is to store the message content
        public static List<Email>? box { get; set; }
    }
    class Pop3_Client
    {
        private Socket socket;
        private NetworkStream? networkStream;
        private StreamReader? streamReader;
        private StreamWriter? streamWriter;
        public IPAddress server;
        public short port;
        public ConfigJson config;
        public Pop3_Client(ConfigJson config)
        {
            this.config = config;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server = IPAddress.Parse(config.General!.MailServer!);
            port = config.General!.Pop3Port!;
        }
        public void Connect()
        {
            socket.Connect(server, port);
            networkStream = new NetworkStream(socket);
            streamReader = new StreamReader(networkStream, Encoding.ASCII);
            streamWriter = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true };

            // Check if the connection is successful
            if (!socket.Connected)
                throw new Exception("Failed connecting to server!");
        }
        public void ReceiveEmail() {
            // Send the USER command
            Match match = Regex.Match(config.General!.Username!, "<(.+?)>");
            streamWriter!.WriteLine($"USER {match.Groups[1].Value}");

            // Send the PASS command
            streamWriter!.WriteLine($"PASS {config.General!.Password}");

            // Send the STAT command to get the number of messages in the mailbox
            streamWriter!.WriteLine("STAT");

            string? statResponse = streamReader!.ReadLine();
            // Extract the number of messages from the STAT response
            int messageCount = int.Parse(statResponse!.Split(' ')[1]);

            // Retrieve each email
            for (int i = 1; i <= messageCount; i++)
            {
                // Send the RETR command to retrieve the i-th message
                streamWriter!.WriteLine($"RETR {i}");
                string emailContent = streamReader.ReadToEnd();
            }

        }
        public void Close()
        {
            streamWriter.WriteLine("QUIT");
            networkStream!.Close();
            streamWriter!.Close();
            streamReader!.Close();
            socket.Close();
        }
    }
}