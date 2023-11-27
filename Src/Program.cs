using Email_Handler;
using Email_Smtp;
using Email_Pop3;
using Email_Config;
using Email_Database;
class Program
{
    static void Main(string[] args)
    {
        ConfigJson config = ConfigJson.Load("Config.json");

        Smtp_Client smtp = new Smtp_Client(config);
        Pop3_Client pop3 = new Pop3_Client(config);

        smtp.Connect();
        Email mail = new Email("ahihi@testing.com", "xyz@abc.com");
        mail.To.Add("levana@gmail.com");
        mail.To.Add("asd@asd.com");
        mail.Cc.Add("dtna@gmail.com");
        mail.Bcc.Add("Hello@gmail.com");
        mail.Subject = "Test";
        mail.Body = "This is bla bla bla";
        mail.AddAttachment(@"C:\Users\Phuoc Hoan\OneDrive\Instruction to nvim.txt");
        mail.AddAttachment(@"C:\Users\Phuoc Hoan\OneDrive\Tài liệu\IT.docx");
        smtp.SendEmail(mail);
        smtp.Close();

        pop3.Connect();
        pop3.ReceiveEmail();
        pop3.Close();
    
    }
}