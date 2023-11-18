using System.Net;
using System.Net.Sockets;
using System.Text;
using Config;

namespace POP3
{
    class Pop3_Client
    {
        public void Connection() {
            // Get MailServer, POP3 port
            IPAddress MailServer = IPAddress.Parse(ConfigJson.General!.MailServer!);
            short pop3Port = ConfigJson.General.Pop3Port;

            // Get Username, Password
            string? tmp = ConfigJson.General.Username;
            string username = tmp!.Substring(0, tmp.IndexOf("<") - 2);
            string password = ConfigJson.General.Password!;

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
            // Read the welcome message from the server
            string? welcomeMessage = reader.ReadLine();
            Console.WriteLine(welcomeMessage);

            // Send the USER command
            writer.WriteLine($"USER {username}");
            writer.Flush();
            Console.WriteLine(reader.ReadLine());

            // Send the PASS command
            writer.WriteLine($"PASS {password}");
            writer.Flush();
            Console.WriteLine(reader.ReadLine());

            // Send the STAT command to get the number of messages in the mailbox
            writer.WriteLine("STAT");
            writer.Flush();
            string? statResponse = reader.ReadLine();
            Console.WriteLine(statResponse);

            // Extract the number of messages from the STAT response
            int messageCount = int.Parse(statResponse!.Split(' ')[1]);

            // Retrieve each email
            for (int i = 1; i <= messageCount; i++)
            {
                // Send the RETR command to retrieve the i-th message
                writer.WriteLine($"RETR {i}");
                writer.Flush();

                // Read the response
                string? retrResponse = reader.ReadLine();
                Console.WriteLine(retrResponse);

                // Read and print the email content
                StringBuilder emailContent = new StringBuilder();
                string line;
                while (!(line = reader.ReadLine()!).Equals("."))
                {
                    emailContent.AppendLine(line);
                }

                Console.WriteLine($"Email {i} Content:\n{emailContent}");
                Console.WriteLine("-------------------------------------------------");
            }

            // Send the QUIT command
            writer.WriteLine("QUIT");
            writer.Flush();
            Console.WriteLine(reader.ReadLine());

            networkStream.Close();
            writer.Close();
            reader.Close();
            socket.Close();
        }
    }
}