using System.Net;
using System.Net.Sockets;
using System.Text;
using EmailConfig;
using EmailDatabase;
using EmailHandler;

namespace EmailClient
{
    class Pop3_Client
    {
        public ConfigJson config;
        private string connectionString;
        public EmailDbContext dbContext;
        private NetworkStream? networkStream;
        private StreamReader? streamReader;
        private StreamWriter? streamWriter;
        public IPAddress server;
        public short port;

        public Pop3_Client(ConfigJson config)
        {
            this.config = config;
            connectionString = $"Data Source=.\\Data\\{config.General!.Username}.db";
            dbContext = new EmailDbContext(connectionString);
            server = IPAddress.Parse(config.General!.MailServer!);
            port = config.General!.Pop3Port!;
        }

        public void Connect(Socket socket)
        {
            socket.Connect(server, port);
            networkStream = new NetworkStream(socket);
            streamReader = new StreamReader(networkStream, Encoding.ASCII);
            streamWriter = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true };
            streamReader.ReadLine();
        }

        public void ReceiveEmail()
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Connect(socket);
            // Send the USER command
            //Console.WriteLine(streamReader!.ReadLine());
            streamWriter!.WriteLine($"USER {config.General!.Username}");
            streamReader!.ReadLine();

            // Send the PASS command
            streamWriter!.WriteLine($"PASS {config.General!.Password}");
            streamReader.ReadLine();

            // Send the STAT command to get the number of messages in the mailbox
            streamWriter!.WriteLine("STAT");
            string? statResponse = streamReader.ReadLine();

            // Extract the number of messages from the STAT response
            int messageCount = int.Parse(statResponse!.Split(' ')[1]);

            for (int i = 1; i <= messageCount; i++)
            {
                // Send the RETR command to retrieve the i-th message
                streamWriter!.WriteLine($"RETR {i}");
                streamReader!.ReadLine();

                string? line;
                string message = "";

                // This is for handling time-out connection while processing large emails
                bool isDone = false;
                Task refresh = new Task(
                    () =>
                    {
                        while (!isDone)
                        {
                            streamWriter!.WriteLine($"NOOP");
                            streamReader!.ReadLine();
                            // task sleep for 10 seconds
                            Thread.Sleep(10000);
                        }
                    }
                );

                refresh.Start();
                while (true)
                {
                    line = streamReader.ReadLine();
                    if (line == ".")
                        break;
                    message += line + '\n';
                }
                isDone = true;

                streamWriter!.WriteLine($"DELE {i}");
                streamReader.ReadLine();
                Email email = Mime.MimeParser(message);
                FilterEmail.Filter(email, config, dbContext);
            }
            streamWriter!.WriteLine($"QUIT");
            streamReader.ReadLine();
            Close(socket);
        }

        public void Close(Socket socket)
        {
            networkStream!.Close();
            streamWriter!.Close();
            streamReader!.Close();
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

    }
}