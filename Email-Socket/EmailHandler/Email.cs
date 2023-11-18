using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EmailHandler
{
    public class Attachment
    {
        public string FileDir { get; set; }
        public byte[] Data { get; set; }
    }
    public class Email
    {
        // Header
        public string From { get; set; }
        public List<string> To { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public List<Attachment> Attachments { get; }

        public void AddAttachment(string fileName)
        {
            FileInfo file = new(fileName);
            if (file.Exists)
            {
                if (file.Length <= 3000000)
                {
                    Attachment attachment = new Attachment
                    {
                        FileDir = fileName,
                        Data = File.ReadAllBytes(fileName)
                    };
                    Attachments.Add(attachment);
                }
                else throw new FileLoadException();
            }
            else throw new FileNotFoundException();
        }


        public Email(string from, string to)
        {
            From = from;
            To = new List<string> { to };
            Attachments = new List<Attachment>();
        }
    }
}
