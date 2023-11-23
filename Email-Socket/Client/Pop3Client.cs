using System.Net;
using System.Net.Sockets;
using System.Text;
using EmailHandler;

namespace EmailSocket
{
    class Message_Box
    {
        // bool is to check whether the message is read
        // string is to store the message content
        public static List<Email>? box { get; set; }
    }
    class Pop3_Client
    {
        public void Connection(ConfigJson Config)
        {
            // Get MailServer, POP3 port
            IPAddress MailServer = IPAddress.Parse(Config.General!.MailServer!);
            short pop3Port = Config.General.Pop3Port;

            // Get Username, Password
            string? tmp = Config.General.Username;
            string username = tmp!.Substring(0, tmp.IndexOf("<") - 2);
            string password = Config.General.Password!;

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Connect to the POP3 server
            socket.Connect(MailServer, pop3Port);

            // Check if the connection is successful
            if (!socket.Connected)
            {
                Console.WriteLine("Failed to connect to the POP3 server.");
                return;
            }

            NetworkStream networkStream = new NetworkStream(socket);
            StreamReader reader = new StreamReader(networkStream, Encoding.ASCII);
            StreamWriter writer = new StreamWriter(networkStream, Encoding.ASCII);
            writer.AutoFlush = true;
            // Read the welcome message from the server
            string? welcomeMessage = reader.ReadLine();
            Console.WriteLine(welcomeMessage);

            // Send the USER command
            writer.WriteLine($"USER {username}");
            Console.WriteLine(reader.ReadLine());

            // Send the PASS command
            writer.WriteLine($"PASS {password}");
            Console.WriteLine(reader.ReadLine());

            // Send the STAT command to get the number of messages in the mailbox
            writer.WriteLine("STAT");

            string? statResponse = reader.ReadLine();
            Console.WriteLine(reader.ReadLine());

            // Extract the number of messages from the STAT response
            int messageCount = int.Parse(statResponse!.Split(' ')[1]);

            // Retrieve each email
            for (int i = 1; i <= messageCount; i++)
            {
                // Send the RETR command to retrieve the i-th message
                writer.WriteLine($"RETR {i}");


                // Read the response
                string? retrResponse = reader.ReadLine();
                Console.WriteLine(retrResponse);

                // Read the mail content
                string emailContent = reader.ReadToEnd();

                // Add mail to box
                // Message_Box.box!.Add(Mime.MimeParser(emailContent));
                // StringBuilder emailContent = new StringBuilder();
                // string line;
                // while (!(line = reader.ReadLine()!).Equals("."))
                // {
                //     emailContent.AppendLine(line);
                // }

            }
            // Send the QUIT command
            writer.WriteLine("QUIT");
            Console.WriteLine(reader.ReadLine());

            networkStream.Close();
            writer.Close();
            reader.Close();
            socket.Close();
        }
    }
}