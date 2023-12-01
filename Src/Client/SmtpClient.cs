using System.Net;
using System.Net.Sockets;
using System.Text;
using Email_Config;
using Email_Handler;
using System.Security.Cryptography;

namespace Email_Smtp
{
    class Smtp_Client
    {
        private Socket socket;
        private NetworkStream? networkStream;
        private StreamWriter? streamWriter;
        private StreamReader? streamReader;
        public IPAddress server;
        public short port;
        public ConfigJson config;
        public Smtp_Client(ConfigJson config)
        {
            this.config = config;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server = IPAddress.Parse(config.General!.MailServer!);
            port = config.General!.SmtpPort!;
        }
        public void Connect()
        {
            socket.Connect(server, port);
            networkStream = new NetworkStream(socket);
            streamWriter = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true };
            streamReader = new StreamReader(networkStream, Encoding.ASCII);
            // Check if the connection is successful
            if (!socket.Connected)
                throw new Exception("Failed connecting to server!");
        }

        public void Close()
        {
            streamWriter!.WriteLine("QUIT");
            Console.WriteLine(streamReader!.ReadLine());
            networkStream!.Close();
            streamReader!.Close();
            streamWriter!.Close();
            socket.Close();
        }

        public void SendEmail(Email email)
        {
            if (!socket.Connected)
                throw new Exception("Haven't connected to the server!");

            Console.WriteLine(streamReader!.ReadLine());
            streamWriter!.WriteLine("EHLO example.com");
            Console.WriteLine(streamReader.ReadLine());
            
            streamWriter.WriteLine($"MAIL FROM: <{email.From}>");
            Console.WriteLine(streamReader.ReadLine());

            SortedSet<string> peopleToSend = new SortedSet<string>();
            foreach (string to in email.To)
                peopleToSend.Add(to);
            foreach (string Cc in email.Cc)
                peopleToSend.Add(Cc);
            foreach (string Bcc in email.Bcc)
                peopleToSend.Add(Bcc);
            foreach (string to in peopleToSend)
            {
                streamWriter.WriteLine($"RCPT TO: {to}");
                Console.WriteLine(streamReader!.ReadLine());
            }
            
            // Begin to send email content
            streamWriter.WriteLine("DATA");
            Console.WriteLine(streamReader!.ReadLine());
            string randomBoundary = string.Format("----------{0:N}", Guid.NewGuid());
            streamWriter!.WriteLine(Mime.GetMainHeader(email, randomBoundary));
            
            if (email.Attachments.Count == 0)
                streamWriter.WriteLine(email.Body);
            else
            {
                streamWriter.WriteLine(Mime.GetMimePartHeader(randomBoundary, null, false));
                streamWriter.WriteLine(email.Body);
                streamWriter.WriteLine();
                foreach (Attachment attachment in email.Attachments)
                {
                    // Send header of each files
                    streamWriter.WriteLine(Mime.GetMimePartHeader(randomBoundary, attachment.FilePath));
                    SendFile(attachment.FilePath!);
                }
                streamWriter.WriteLine();
                streamWriter.WriteLine($"{randomBoundary}--");
            }
            streamWriter.WriteLine(".");
            Console.WriteLine(streamReader!.ReadLine());
        }

        private void SendFile(string filePath)
        {
            byte[] buffer = new byte[72];

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            using (CryptoStream b64Stream = new(fileStream, new ToBase64Transform(), CryptoStreamMode.Read))
            {
                int read;
                while ((read = b64Stream.Read(buffer)) != 0)
                {
                    networkStream!.Write(buffer, 0, read);
                    networkStream.Write("\r\n"u8.ToArray());
                }
            }
        }
    }

}