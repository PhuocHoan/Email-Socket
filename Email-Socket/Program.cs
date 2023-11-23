using EmailHandler;
using SMTP;
class Program
{
    static void Main(string[] args)
    {
        Smtp_Client smtp = new Smtp_Client();
        //Pop3_Client pop3 = new Pop3_Client();
        smtp.Connect();
        Email mail = new Email("abc@xyz.com", "xyz@abc.com");
        mail.To.Add("dnat@gmail.com");
        mail.To.Add("asd@asd.com");
        mail.CC.Add("dtna@gmail.com");
        mail.Body = "This is bla bla bla";
        mail.AddAttachment(@"D:\Uni\Yuusha.pdf");
        mail.AddAttachment(@"D:\Uni\test.docx");
        smtp.SendEmail(mail);
        smtp.Close();
    }
}