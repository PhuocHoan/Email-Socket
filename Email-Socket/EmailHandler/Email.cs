using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailHandler
{

    public class Attachment
    {
        public string Directory { get; set; }
        public byte[]? Data { get; set; }

        public Attachment(string dir)
        {
            Directory = dir;
        }

    }
    public class Email
    {
        // Header
        public string From { get; set; }
        public List<string> To { get; set; }
        public List<string> CC { get; set; }
        public List<string> BCC { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<Attachment> Attachments { get; }

        public Email(string from, string to)
        {
            From = from;
            To = new List<string> { to };
            CC = new List<string>();
            BCC = new List<string>();
            Attachments = new List<Attachment>();

        }

        public Email()
        {
            To = new List<string>();
            CC = new List<string>();
            BCC = new List<string>();
            Attachments = new List<Attachment>();
        }

        public void AddAttachment(string fileName)
        {
            FileInfo file = new(fileName);
            if (file.Exists)
            {
                if (file.Length <= 3000000)
                    Attachments.Add(new Attachment(fileName));
                else throw new Exception("Size of file surpassed 3Mb!");
            }
            else throw new FileNotFoundException();
        }

    }
}
