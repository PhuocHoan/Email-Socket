using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using Config;

namespace SMTP
{
    class Smtp_Client
    {
        public void Connection() {
            // Get file json path
            string file = Path.GetFullPath("Config.json");
            ConfigJson.Load(file);
            // Specify the SMTP server and port
            IPAddress MailServer = IPAddress.Parse(ConfigJson.General!.MailServer!);
            short SmtpPort = ConfigJson.General.SmtpPort;

            // Create a socket and connect to the SMTP server
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(MailServer, SmtpPort);

            // Specify sender and recipient email addresses
            string tmp = ConfigJson.General.Username!;
            string from = tmp.Substring(tmp.IndexOf('<'), tmp.Length - 1 - tmp.IndexOf('<'));
            string to = "phuochoan17032004@gmail.com, ahihi@testing.com";
            string[] Recipient = AdequateMail(to);

            // Check if the connection is successful
            if (!socket.Connected)
            {
                Console.WriteLine("Failed to connect to the SMTP server.");
                return;
            }

            using NetworkStream networkStream = new NetworkStream(socket);
            using StreamReader reader = new StreamReader(networkStream, Encoding.ASCII);
            using StreamWriter writer = new StreamWriter(networkStream, Encoding.ASCII);
            writer.AutoFlush = true;

            // Read the welcome message from the server
            string? welcomeMessage = reader.ReadLine();
            Console.WriteLine(welcomeMessage);

            // Send the EHLO command to the server
            writer.WriteLine("EHLO example.com");

            // Read and print the response
            string? ehloResponse = reader.ReadLine();
            Console.WriteLine(ehloResponse);

            // Send the MAIL FROM command
            writer.WriteLine($"MAIL FROM:<{from}>");

            // Read and print the response
            string? mailFromResponse = reader.ReadLine();
            Console.WriteLine(mailFromResponse);

            // Send the RCPT TO command
            writer.WriteLine($"RCPT TO:<{to}>");

            // Read and print the response
            string? rcptToResponse = reader.ReadLine();
            Console.WriteLine(rcptToResponse);

            // Send the DATA command
            writer.WriteLine("DATA");

            // Read and print the response
            string? dataResponse = reader.ReadLine();
            Console.WriteLine(dataResponse);

            /* Send the email content */            

            // Read and print the response
            string? sendDataResponse = reader.ReadLine();
            Console.WriteLine(sendDataResponse);

            // Send the QUIT command
            writer.WriteLine("QUIT");

            // Read and print the response
            string? quitResponse = reader.ReadLine();
            Console.WriteLine(quitResponse);

            networkStream.Close();
            writer.Close();
            reader.Close();
            socket.Close();
        }
        private string[] AdequateMail(string mail) {
            // To get each mail receiver adequately
            string[] mails = mail.Split(",");
            for (int i = 0; i < mails.Length; ++i) {
                mails[i] = mails[i].Trim();
            }
            return mails;
        }
    }
}