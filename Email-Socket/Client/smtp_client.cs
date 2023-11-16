using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SMTP
{
    class Smtp_Client
    {
        public void Connection() {
            // Specify the SMTP server and port
            IPAddress smtpServer = IPAddress.Parse("127.0.0.1");
            int smtpPort = 25;

            // Specify sender and recipient email addresses
            string from = "your_email@example.com";
            string to = "phuochoan17032004@gmail.com";

            // Create a socket and connect to the SMTP server
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(smtpServer, smtpPort);

            // Check if the connection is successful
            if (!socket.Connected)
            {
                Console.WriteLine("Failed to connect to the SMTP server.");
                return;
            }

            NetworkStream networkStream = new NetworkStream(socket);
            StreamReader reader = new StreamReader(networkStream, Encoding.ASCII);
            StreamWriter writer = new StreamWriter(networkStream, Encoding.ASCII);
            // Read the welcome message from the server
            string? welcomeMessage = reader.ReadLine();
            Console.WriteLine(welcomeMessage);

            // Send the EHLO command to the server
            writer.WriteLine("EHLO example.com");
            writer.Flush();

            // Read and print the response
            string? ehloResponse = reader.ReadLine();
            Console.WriteLine(ehloResponse);

            // Send the MAIL FROM command
            writer.WriteLine($"MAIL FROM:<{from}>");
            writer.Flush();

            // Read and print the response
            string? mailFromResponse = reader.ReadLine();
            Console.WriteLine(mailFromResponse);

            // Send the RCPT TO command
            writer.WriteLine($"RCPT TO:<{to}>");
            writer.Flush();

            // Read and print the response
            string? rcptToResponse = reader.ReadLine();
            Console.WriteLine(rcptToResponse);

            // Send the DATA command
            writer.WriteLine("DATA");
            writer.Flush();

            // Read and print the response
            string? dataResponse = reader.ReadLine();
            Console.WriteLine(dataResponse);

            // Read and print the response
            string? sendDataResponse = reader.ReadLine();
            Console.WriteLine(sendDataResponse);

            // Send the QUIT command
            writer.WriteLine("QUIT");
            writer.Flush();

            // Read and print the response
            string? quitResponse = reader.ReadLine();
            Console.WriteLine(quitResponse);

            networkStream.Close();
            writer.Close();
            reader.Close();
            socket.Close();
        }
        void MailContent() {
            // Send the email content
            // writer.WriteLine("Subject: Test Email");
            // writer.WriteLine("From: " + from);
            // writer.WriteLine("To: " + to);
            // writer.WriteLine("Content-Type: text/plain; charset=utf-8");
            // writer.WriteLine();
            // writer.WriteLine("This is a test email sent using raw sockets in C#.");
            // writer.WriteLine('.');
            // writer.Flush();
        }
    }
}