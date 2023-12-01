namespace Email_Handler
{

    public class Attachment
    {
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public byte[]? Data { get; set; }

        public Attachment(string dir)
        {
            FileName = dir;
            FilePath = Path.GetFullPath(dir);
        }
        public Attachment() {}

    }
    public class Email
    {
        // Header
        public string? MessageId { get; set; }
        public string? From { get; set; }
        public List<string> To { get; set; }
        public List<string> Cc { get; set; }
        public List<string> Bcc { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public bool Status { get; set; }
        public List<Attachment> Attachments { get; set; }

        public Email(string from, string to)
        {
            From = from;
            To = new List<string> { to };
            Cc = new List<string>();
            Bcc = new List<string>();
            Attachments = new List<Attachment>();

        }

        public Email()
        {
            To = new List<string>();
            Cc = new List<string>();
            Bcc = new List<string>();
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
