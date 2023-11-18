using System.Text;
using Microsoft.Win32;

namespace EmailHandler
{
    public class Mime
    {
        private static IDictionary<string, string> ContentType = new Dictionary<string, string>
        {
            {".txt", "text/plain"},
            {".doc", "application/msword"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".mp3", "audio/mpeg"},
            {".wav", "audio/wav"},
            {".jpe", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".png", "image/png"},
            {".pdf", "application/pdf"},
            {".7z", "application/x-7z-compressed"},
            {".bin", "application/octet-stream"},
            {".zip", "application/x-zip-compressed"},
            {".rar", "application/octet-stream"},
            {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
        };

        public static string GetFileContentType(string fileName)
        {
            string? extension = Path.GetExtension(fileName).ToLower();
            string? result;
            return ContentType.TryGetValue(extension, out result) ? result : "unknown";
        }

        public static string? GetFileExtension(string mimeType)
        {
            var key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
            var value = key?.GetValue("Extension", null);

            return value?.ToString();
        }

        public static string GetEmailContentType(Email email)
        {
            if (email.Attachments.Count > 0)
                return "multipart/mixed";
            else return "text/plain";
        }

        public static string MimeFormatter(Email email)
        {
            StringBuilder content = new StringBuilder();
            content.AppendLine("MIME-Version: 1.0");
            content.AppendLine($"From: <{email.From}>");
            content.Append($"To: ");
            for (int i = 0; i < email.To.Count; ++i)
            {
                content.Append($"<{email.To[i]}>");
                if (i != email.To.Count - 1)
                    content.Append(", ");
            }

            content.AppendLine($"Subject: {email.Subject}");
            string emailContentType = GetEmailContentType(email);
            content.AppendLine($"Content-Type: {emailContentType}{(emailContentType.Equals("multipart/mixed") ? "; boundary=\"0urte3m~cust0m1zedb0und3ry\"" : "")}");
            content.AppendLine();

            if (emailContentType.Equals("text/plain"))
                content.AppendLine(email.Body);
            else
            {
                content.AppendLine("--0urte3m~cust0m1zedb0und3ry");
                content.AppendLine("Content-Type: text/plain; charset=us-ascii");
                content.AppendLine("Content-Transfer-Encoding: 7bit");
                content.AppendLine();
                content.AppendLine(email.Body);
                content.AppendLine();

                foreach (Attachment file in email.Attachments)
                {
                    content.AppendLine("--0urte3m~cust0m1zedb0und3ry");
                    content.AppendLine($"Content-Type: {GetFileContentType(file.FileDir)}; name = \"{Path.GetFileName(file.FileDir)}\"");
                    content.AppendLine("Content-Transfer-Encoding: base64");
                    content.AppendLine();
                    content.AppendLine(Convert.ToBase64String(file.Data));
                    content.AppendLine();
                }

                content.AppendLine("--0urte3m~cust0m1zedb0und3ry--");
            }

            return content.ToString();
        }

        // public static Email MimeParser(string mime)
        // {
        //     string[] lines = mime.Split("\r\n");
        //     string[] from = lines[1].Split(" ");
        //     string[] to = lines[2].Split(" ");
        //     string[] subject = lines[3].Split(" ");
        //     string[] contentType = lines[4].Split(" ");
        //     string[] contentTransferEncoding = lines[5].Split(" ");
        //     string[] body = lines[6].Split(" ");

        //     Email email = new Email(from[1], to[1]);
        //     email.Subject = subject[1];
        //     email.Body = body[1];

        //     if (contentType[1].Equals("multipart/mixed"))
        //     {
        //         int i = 8;
        //         while (!lines[i].Equals("--0urte3m~cust0m1zedb0und3ry--"))
        //         {
        //             string[] fileHeader = lines[i].Split(" ");
        //             string[] fileContentType = lines[i + 1].Split(" ");
        //             string[] fileContentTransferEncoding = lines[i + 2].Split(" ");
        //             string[] fileBody = lines[i + 3].Split(" ");

        //             Attachment attachment = new Attachment
        //             {
        //                 FileDir = fileHeader[2],
        //                 Data = Convert.FromBase64String(fileBody[1])
        //             };
        //             email.Attachments.Add(attachment);
        //             i += 5;
        //         }
        //     }

        //     return email;
        // }

    }
}
