using SMTP;
using POP3;
class Program
{
    static void Main(string[] args)
    {
        Smtp_Client smtp = new Smtp_Client();
        Pop3_Client pop3 = new Pop3_Client();
        smtp.Connection();
        pop3.Connection();
    }
}