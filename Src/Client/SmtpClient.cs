using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using EmailConfig;
using EmailHandler;

namespace EmailClient
{
    class Smtp_Client
    {
        private NetworkStream? networkStream;
        private StreamWriter? streamWriter;
        private StreamReader? streamReader;
        public IPAddress server;
        public short port;
        public ConfigJson config;
        public Smtp_Client(ConfigJson config)
        {
            this.config = config;
            server = IPAddress.Parse(config.General!.MailServer!);
            port = config.General!.SmtpPort!;
        }

        private void Connect(Socket socket)
        {
            socket.Connect(server, port);
            networkStream = new NetworkStream(socket);
            streamWriter = new StreamWriter(networkStream, Encoding.ASCII) { AutoFlush = true };
            streamReader = new StreamReader(networkStream, Encoding.ASCII);

            // Check if the connection is successful
            if (!socket.Connected)
                throw new Exception("Failed connecting to server!");
        }

        private void Close(Socket socket)
        {
            networkStream!.Close();
            streamReader!.Close();
            streamWriter!.Close();
            socket.Close();
        }

        public void SendEmail(Email email)
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Connect(socket);

            streamWriter!.WriteLine("EHLO example.com");
            streamReader!.ReadLine();
            streamWriter.WriteLine($"MAIL FROM:<{email.From}>");
            streamReader!.ReadLine();

            SortedSet<string> peopleToSend = new SortedSet<string>();
            foreach (string to in email.To)
                peopleToSend.Add(to);
            foreach (string Cc in email.Cc!)
                peopleToSend.Add(Cc);
            foreach (string Bcc in email.Bcc!)
                peopleToSend.Add(Bcc);
            foreach (string to in peopleToSend)
            {
                streamWriter.WriteLine($"RCPT TO: {to}");
                streamReader!.ReadLine();
            }

            // Begin to send email content
            streamWriter.WriteLine("DATA");
            streamReader!.ReadLine();
            string randomBoundary = string.Format("----------{0:N}", Guid.NewGuid());
            streamWriter!.Write(Mime.GetMainHeader(email, randomBoundary));

            if (email.Attachments.Count == 0)
                streamWriter.WriteLine(email.Body);
            else
            {
                streamWriter.WriteLine("This is a multi-part message in MIME format.");
                streamWriter.Write(Mime.GetMimePartHeader(randomBoundary, null, false));
                streamWriter.WriteLine(email.Body);
                streamWriter.WriteLine();
                foreach (Attachment attachment in email.Attachments)
                {
                    // Send header of each files
                    streamWriter.Write(Mime.GetMimePartHeader(randomBoundary, attachment.FilePath));
                    if (Mime.GetFileContentType(attachment.FilePath!).Equals("text/plain"))
                        SendFile(attachment.FilePath!, true);
                    else SendFile(attachment.FilePath!);
                }
                streamWriter.WriteLine();
                streamWriter.WriteLine($"--{randomBoundary}--");
            }
            streamWriter.WriteLine(".");
            streamReader!.ReadLine();

            streamWriter.WriteLine("QUIT");
            streamReader!.ReadLine();

            Close(socket);
        }

        private void SendFile(string filePath, bool isText = false)
        {
            byte[] buffer = new byte[72];

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                if (!isText)
                {
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
                else
                {
                    // Handle for .txt files
                    int read;
                    while ((read = fileStream.Read(buffer)) != 0)
                        networkStream!.Write(buffer, 0, read);
                    networkStream!.Write("\r\n"u8.ToArray());
                }
        }
    }
}