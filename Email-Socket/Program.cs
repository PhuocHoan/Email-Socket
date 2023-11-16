using System;
using SMTP;
using POP3;
using Config;

class Program
{
    static void Main(string[] args)
    {
        // Smtp_Client smtp = new Smtp_Client();
        // Pop3_Client pop3 = new Pop3_Client();
        ConfigJson config = new ConfigJson();
        config.loadJson();
        Console.WriteLine(config.general["Username"]);
        // smtp.Connection();
        // pop3.Connection();
    }
}