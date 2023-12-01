using Email_Handler;
using Email_Smtp;
using Email_Pop3;
using Email_Config;
using Email_Database;
using System.Runtime.CompilerServices;
class Program
{
    static void Main(string[] args)
    {
        ConfigJson config = new ConfigJson();
        if (!File.Exists(Path.GetFullPath("Config.json"))) {
            config = ConfigJson.CreateDefaultJsonFile();
        }
        else {
            string FilePath = Path.GetFullPath("Config.json");
            config = ConfigJson.Load(FilePath);
        }
        
        Smtp_Client smtp = new Smtp_Client(config);
        Pop3_Client pop3 = new Pop3_Client(config);

        smtp.Connect();
        Email mail = new Email("hoan@testing.com", "xyz@abc.com");
        mail.To.Add("levana@gmail.com");
        mail.To.Add("asd@asd.com");
        mail.Cc.Add("dtna@gmail.com");
        mail.Bcc.Add("hophuochoan@gmail.com");
        mail.Subject = "ASAP";
        mail.Body = "This is ViRus haha report";
        // mail.AddAttachment(@"C:\Users\Phuoc Hoan\OneDrive\Instruction to nvim.txt");
        // mail.AddAttachment(@"C:\Users\Phuoc Hoan\OneDrive\Tài liệu\IT.docx");
        // mail.AddAttachment(@"C:\Users\Phuoc Hoan\OneDrive - VNU-HCMUS\Work Space\My Uni\2nd year\4th Semester\Object-oriented Programming\LN01 - Basic Concepts.pdf");
        // mail.AddAttachment(@"C:\Users\Phuoc Hoan\OneDrive\Pictures\Screenshots\Screenshot 2023-11-19 064152.png");
        // mail.AddAttachment(@"C:\Users\Phuoc Hoan\Downloads\Program\ChromeSetup.exe");
        // mail.AddAttachment(@"C:\Users\Phuoc Hoan\OneDrive - VNU-HCMUS\Work Space\My Uni\2nd year\4th Semester\Object-oriented Programming\Lab\Week 7\22127119_Week07_07.zip");
        // mail.AddAttachment(@"C:\Users\Phuoc Hoan\OneDrive - VNU-HCMUS\Work Space\My Uni\2nd year\4th Semester\Computer Networking\Slide tiếng Anh\Chapter_9_Multimedia_Networking_V7.0.pptx");
        smtp.SendEmail(mail);
        smtp.Close();

        pop3.Connect();
        pop3.ReceiveEmail();
        pop3.Close();
    }
}