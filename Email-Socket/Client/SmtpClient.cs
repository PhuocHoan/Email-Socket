using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using EmailSocket;
using EmailHandler;
using System.Security.Cryptography;
using System;

namespace SMTP
{
    class Smtp_Client
    {
        private Socket socket;
        private NetworkStream? networkStream;
        private StreamReader? streamReader;
        private StreamWriter? streamWriter;
        public IPAddress server;
        public short port;

        public Smtp_Client()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ConfigJson config = ConfigJson.Load(Path.GetFullPath("Config.json"));
            server = IPAddress.Parse(config.General!.MailServer!);
            port = config.General!.SmtpPort!;
        }

        public Smtp_Client(string server, short port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.server = IPAddress.Parse(server);
            this.port = port;
        }

        public void Connect()
        {
            socket.Connect(server, port);
            networkStream = new NetworkStream(socket);
            streamReader = new StreamReader(networkStream, Encoding.ASCII);
            streamWriter = new StreamWriter(networkStream, Encoding.ASCII);
            streamWriter.AutoFlush = true;

            // Check if the connection is successful
            if (!socket.Connected)
                throw new Exception("Failed connecting to server!");
        }

        public void Close()
        {
            streamWriter!.WriteLine("QUIT");
            networkStream!.Close();
            streamWriter!.Close();
            streamReader!.Close();
            socket.Close();
        }

        private string[] AdequateMail(string mail)
        {
            // To get each mail receiver adequately
            string[] mails = mail.Split(",");
            for (int i = 0; i < mails.Length; ++i)
            {
                mails[i] = mails[i].Trim();
            }
            return mails;
        }

        public void SendEmail(Email email)
        {
            if (!socket.Connected)
                throw new Exception("Haven't connected to the server!");

            string? response;

            streamWriter!.WriteLine("EHLO example.com");
            response = streamReader!.ReadLine();
            Console.WriteLine(response);
            streamWriter.WriteLine($"MAIL FROM:<{email.From}>");
            response = streamReader!.ReadLine();
            Console.WriteLine(response);

            SortedSet<string> peopleToSend = new SortedSet<string>();
            foreach (string to in email.To)
                peopleToSend.Add(to);
            foreach (string cc in email.CC)
                peopleToSend.Add(cc);
            foreach (string bcc in email.BCC)
                peopleToSend.Add(bcc);
            foreach (string to in peopleToSend)
            {
                streamWriter.WriteLine($"RCPT TO:<{to}>");
                response = streamReader!.ReadLine();
                Console.WriteLine(response);
            }

            // Begin to send email content
            streamWriter.WriteLine("DATA");
            response = streamReader!.ReadLine();
            Console.WriteLine(response);
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
                    streamWriter.Write(Mime.GetMimePartHeader(randomBoundary, attachment.Directory));
                    if (Mime.GetFileContentType(attachment.Directory!).Equals("text/plain"))
                        SendFile(attachment.Directory!, true);
                    else SendFile(attachment.Directory!);
                }
                streamWriter.WriteLine();
                streamWriter.WriteLine($"--{randomBoundary}--");
            }
            streamWriter.WriteLine(".");
            response = streamReader!.ReadLine();
            Console.WriteLine(response);
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
                            networkStream.Write(buffer, 0, read);
                            networkStream.Write("\r\n"u8.ToArray());
                        }
                    }
                }
                else
                {
                    int read;
                    while ((read = fileStream.Read(buffer)) != 0)
                        networkStream.Write(buffer, 0, read);
                    networkStream.Write("\r\n"u8.ToArray());
                }
        }
    }

}