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
        private StreamWriter? streamWriter;
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

            // Check if the connection is successful
            if (!socket.Connected)
                throw new Exception("Failed connecting to server!");
        }

        public void Close()
        {
            streamWriter.WriteLine("QUIT");
            networkStream!.Close();
            streamWriter!.Close();
            socket.Close();
        }

        // private string[] AdequateMail(string mail)
        // {
        //     // To get each mail receiver adequately
        //     string[] mails = mail.Split(",");
        //     for (int i = 0; i < mails.Length; ++i)
        //     {
        //         mails[i] = mails[i].Trim();
        //     }
        //     return mails;
        // }

        public void SendEmail(Email email)
        {
            if (!socket.Connected)
                throw new Exception("Haven't connected to the server!");

            streamWriter!.WriteLine("EHLO example.com");
            streamWriter.WriteLine($"MAIL FROM <{email.From}>");

            SortedSet<string> peopleToSend = new SortedSet<string>();
            foreach (string to in email.To)
                peopleToSend.Add(to);
            foreach (string Cc in email.Cc)
                peopleToSend.Add(Cc);
            foreach (string Bcc in email.Bcc)
                peopleToSend.Add(Bcc);
            foreach (string to in peopleToSend)
                streamWriter.WriteLine($"RCPT TO {to}");

            // Begin to send email content
            streamWriter.WriteLine("DATA");
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
                    streamWriter.WriteLine(Mime.GetMimePartHeader(randomBoundary, attachment.Directory));
                    SendFile(attachment.Directory);
                }
            }
            streamWriter.WriteLine();
            streamWriter.WriteLine($"{randomBoundary}--");
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