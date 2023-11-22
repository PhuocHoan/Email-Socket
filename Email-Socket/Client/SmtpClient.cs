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

            streamWriter!.WriteLine("EHLO example.com");
            streamWriter.WriteLine($"MAIL FROM:<{email.From}>");

            SortedSet<string> peopleToSend = new SortedSet<string>();
            foreach (string to in email.To)
                peopleToSend.Add(to);
            foreach (string cc in email.CC)
                peopleToSend.Add(cc);
            foreach (string bcc in email.BCC)
                peopleToSend.Add(bcc);
            foreach (string to in peopleToSend)
                streamWriter.WriteLine($"RCPT TO:<{to}>");

            // Begin to send email content
            streamWriter.WriteLine("DATA");
            string randomBoundary = string.Format("----------{0:N}", Guid.NewGuid());
            streamWriter!.Write(Mime.GetMainHeader(email, randomBoundary));

            if (email.Attachments.Count == 0)
                streamWriter.Write(email.Body);
            else
            {
                streamWriter.Write(Mime.GetMimePartHeader(randomBoundary, null, false));
                streamWriter.WriteLine(email.Body);
                streamWriter.WriteLine();
                foreach (string attachment in email.Attachments)
                {
                    // Send header of each files
                    streamWriter.Write(Mime.GetMimePartHeader(randomBoundary, attachment));
                    SendFile(attachment);
                }
            }
            streamWriter.WriteLine();
            streamWriter.WriteLine($"{randomBoundary}--");
            streamWriter.WriteLine(".");
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
                    networkStream.Write(buffer, 0, read);
                    networkStream.Write("\r\n"u8.ToArray());
                }
            }
        }
    }

}