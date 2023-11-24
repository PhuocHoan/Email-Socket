using EmailHandler;
using SMTP;
using POP3;
using EmailSocket;
class Program
{
    static void Main(string[] args)
    {
        ConfigJson config = ConfigJson.Load(Path.GetFullPath("Config.json"));

        Smtp_Client smtp = new Smtp_Client(config);
        Pop3_Client pop3 = new Pop3_Client(config);

        smtp.Connect();
        Email mail = new Email("phuochoan17032004@gmail.com", "xyz@abc.com");
        mail.To.Add("phuochoan17032004@gmail.com");
        mail.To.Add("asd@asd.com");
        mail.Cc.Add("dtna@gmail.com");
        mail.Bcc.Add("Hello@gmail.com");
        mail.Body = "This is bla bla bla";
        mail.AddAttachment(@"C:\Users\Phuoc Hoan\OneDrive - VNU-HCMUS\Instruction to nvim.txt");
        mail.AddAttachment(@"C:\Users\Phuoc Hoan\OneDrive\Tài liệu\IT.docx");
        smtp.SendEmail(mail);
        smtp.Close();

        pop3.Connect();
        pop3.ReceiveEmail();
        pop3.Close();
    
    }
}