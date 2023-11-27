using System.Net;
using System.Net.Sockets;
using System.Text;
using Email_Handler;
using Email_Config;
using System.Text.RegularExpressions;
using Email_Database;

namespace Email_Pop3
{   
    class Pop3_Client
    {
        private static string connectionString = "Data Source=Emails.db";
        public EmailDbContext dbContext = new EmailDbContext(connectionString);
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
            Console.WriteLine(streamReader.ReadLine());
            Match match = Regex.Match(config.General!.Username!, "<(.+?)>");
            streamWriter!.WriteLine($"USER {match.Groups[1].Value}");
            Console.WriteLine(streamReader.ReadLine());

            // Send the PASS command
            streamWriter!.WriteLine($"PASS {config.General!.Password}");
            Console.WriteLine(streamReader.ReadLine());

            // Send the STAT command to get the number of messages in the mailbox
            streamWriter!.WriteLine("STAT");
            string? statResponse = streamReader.ReadLine();
            Console.WriteLine(statResponse);
            // Extract the number of messages from the STAT response
            int messageCount = int.Parse(statResponse!.Split(' ')[1]);

            for (int i = 1; i <= messageCount; i++)
            {
                // Send the RETR command to retrieve the i-th message
                streamWriter!.WriteLine($"RETR {i}");
                Console.WriteLine(streamReader.ReadLine());
                string message = "";
                while (true)
                {
                    string? line = streamReader.ReadLine();
                    if (line == ".")
                        break;
                    message += line + '\n';
                }
                Email email = Mime.MimeParser(message);
                FilterEmail.Filter(email, config, dbContext);
            }
            List<Email> emails = dbContext.GetEmailsByFolder("Project");
            dbContext.UpdateEmailStatus(emails[0].MessageId, true);
            dbContext.UpdateAttachmentFilePath(emails[0].MessageId, @"C:\Users\Phuoc Hoan\OneDrive - VNU-HCMUS\Work Space\My Uni\2nd year\4th Semester\Computer Networking\Lab\Project1 Socket\Email-Socket\Src");
        }
        public void Close()
        {
            streamWriter.WriteLine("QUIT");
            Console.WriteLine(streamReader!.ReadLine());
            networkStream!.Close();
            streamWriter!.Close();
            streamReader!.Close();
            socket.Close();
        }
    }
}