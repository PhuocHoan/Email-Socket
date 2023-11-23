using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;


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
            return ContentType.TryGetValue(extension, out string? result) ? result : "unknown";
        }

        // Might not work on other OS
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

        public static string GetMainHeader(Email email, string boundary)
        {
            StringBuilder header = new StringBuilder();
            header.AppendLine($"Message-ID: <{Guid.NewGuid()}@{email.From.Split(new[] { '@' }, 2)[1]}>");

            // Get the local time zone & format
            TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
            TimeSpan utcOffset = localTimeZone.BaseUtcOffset;
            string formattedUtcOffset = $"{(utcOffset.Hours >= 0 ? "+" : "-")}{Math.Abs(utcOffset.Hours):00}{utcOffset.Minutes:00}";
            string formattedDateTime = DateTime.Now.ToString($"ddd, dd MMM yyyy HH:mm:ss {formattedUtcOffset}");

            header.AppendLine($"Date: {formattedDateTime}");
            header.AppendLine("MIME-Version: 1.0");
            header.AppendLine($"From: {email.From}");

            header.Append($"To: ");
            for (int i = 0; i < email.To.Count; ++i)
            {
                header.Append($"{email.To[i]}");
                if (i != email.To.Count - 1)
                    header.Append(", ");
            }
            header.AppendLine();

            header.Append($"Cc: ");
            if (email.CC.Count > 0)
            {
                for (int i = 0; i < email.CC.Count; ++i)
                {
                    header.Append($"{email.CC[i]}");
                    if (i != email.CC.Count - 1)
                        header.Append(", ");
                }
            }

            header.AppendLine();
            if (email.Subject != null)
                header.AppendLine($"Subject: {email.Subject}");
            string emailContentType = GetEmailContentType(email);
            if (emailContentType.Equals("text/plain"))
            {
                header.AppendLine($"Content-Type: text/plain; charset=UTF-8; format=flowed");
                header.AppendLine($"Content-Transfer-Encoding: 7bit");
            }
            else 
                header.AppendLine($"Content-Type: multipart/mixed; boundary=\"{boundary}\"");
            header.AppendLine();

            return header.ToString();
        }

        public static string GetMimePartHeader(string boundary, string? fileName, bool isFile = true)
        {
            StringBuilder header = new StringBuilder();
        
            header.AppendLine($"{boundary}");
            if (isFile)
            {
                header.AppendLine($"Content-Type: {GetFileContentType(fileName!)}; name=\"{Path.GetFileName(fileName)}\"");
                header.AppendLine($"Content-Disposition: attachment; filename=\"{Path.GetFileName(fileName)}\"");
                header.AppendLine("Content-Transfer-Encoding: base64");
            }
            else
            {
                header.AppendLine($"Content-Type: text/plain; charset=UTF-8; format=flowed");
                header.AppendLine("Content-Transfer-Encoding: 7bit");
            }
            header.AppendLine();

            return header.ToString();
        }

        // public static string GetFileHeader(Email email)
        // {
        //     StringBuilder header = new StringBuilder();
        //     string emailContentType = GetEmailContentType(email);
        //     header.AppendLine($"Content-Type: {emailContentType}{(emailContentType.Equals("multipart/mixed") ? "; boundary=\"0urte3m~cust0m1zedb0und3ry\"" : "")}");
        //     header.AppendLine();

        //     if (emailContentType.Equals("text/plain"))
        //         header.AppendLine(email.Body);
        //     else
        //     {
        //         header.AppendLine("--0urte3m~cust0m1zedb0und3ry");
        //         header.AppendLine("Content-Type: text/plain; charset=us-ascii");
        //         header.AppendLine("Content-Transfer-Encoding: 7bit");
        //         header.AppendLine();
        //         header.AppendLine(email.Body);
        //         header.AppendLine();

        //         foreach (Attachment file in email.Attachments)
        //         {
        //             header.AppendLine("--0urte3m~cust0m1zedb0und3ry");
        //             header.AppendLine($"Content-Type: {GetFileContentType(file.FileDir)}; name = \"{Path.GetFileName(file.FileDir)}\"");
        //             header.AppendLine("Content-Transfer-Encoding: base64");
        //             header.AppendLine();
        //             header.AppendLine(Convert.ToBase64String(file.Data));
        //             header.AppendLine();
        //         }

        //         content.AppendLine("--0urte3m~cust0m1zedb0und3ry--");
        //     }

        //     return content.ToString();
        // }

        // public static Email MimeParser(string mimeMessage)
        // {
        //     Email email = new Email();
        //     StringReader reader = new StringReader(mimeMessage);

        //     // Parse headers
        //     string line;
        //     while (!string.IsNullOrEmpty(line = reader.ReadLine()))
        //     {
        //         string[] headerParts = line.Split(new[] { ':' }, 2);
        //         if (headerParts.Length == 2)
        //         {
        //             string headerName = headerParts[0].Trim();
        //             string headerValue = headerParts[1].Trim();

        //             switch (headerName.ToLower())
        //             {
        //                 case "from":
        //                     email.From = headerValue;
        //                     break;
        //                 case "to":
        //                     email.To.AddRange(headerValue.Split(',').Select(address => address.Trim()));
        //                     break;
        //                 case "cc":
        //                     email.CC.AddRange(headerValue.Split(',').Select(address => address.Trim()));
        //                     break;
        //                 case "subject":
        //                     email.Subject = headerValue;
        //                     break;
        //                 case "content-type":
        //                     if (headerValue.StartsWith("multipart/mixed", StringComparison.OrdinalIgnoreCase))
        //                         // Handle multipart/mixed
        //                         ParseMultipartMixed(reader, email);
        //                     break;
        //             }
        //         }
        //     }

        //     return email;
        // }

        // private static void ParseMultipartMixed(StringReader reader, Email email)
        // {
        //     string boundary = GetBoundary(reader.ReadLine());
        //     while (true)
        //     {
        //         // Skip until the boundary
        //         while (!reader.ReadLine().StartsWith("--" + boundary)) { }

        //         // Parse part headers
        //         Dictionary<string, string> partHeaders = new Dictionary<string, string>();
        //         while (!string.IsNullOrEmpty(reader.PeekLine()))
        //         {
        //             string partHeaderLine = reader.ReadLine();
        //             if (partHeaderLine.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase))
        //             {
        //                 partHeaders["Content-Type"] = partHeaderLine.Substring("Content-Type:".Length).Trim();
        //             }
        //             else if (partHeaderLine.StartsWith("Content-Transfer-Encoding:", StringComparison.OrdinalIgnoreCase))
        //             {
        //                 partHeaders["Content-Transfer-Encoding"] = partHeaderLine.Substring("Content-Transfer-Encoding:".Length).Trim();
        //             }
        //             else if (partHeaderLine.StartsWith("Content-Disposition:", StringComparison.OrdinalIgnoreCase))
        //             {
        //                 // Extract filename from Content-Disposition if needed
        //                 Match match = Regex.Match(partHeaderLine, "filename=\"(.+?)\"");
        //                 if (match.Success)
        //                 {
        //                     string filename = match.Groups[1].Value;
        //                     email.Attachments.Add(new Attachment(@$".\{filename}"));
        //                 }
        //             }
        //         }

        //         // Read part body
        //         string partBody = "";
        //         while (true)
        //         {
        //             string line = reader.PeekLine();
        //             if (line == null || line.StartsWith("--" + boundary))
        //             {
        //                 break;
        //             }
        //             partBody += reader.ReadLine() + Environment.NewLine;
        //         }

        //         // Handle part based on Content-Type
        //         if (partHeaders.TryGetValue("Content-Type", out string contentType))
        //         {
        //             if (contentType.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase))
        //                 email.Body = partBody;
        //             else
        //             {
        //                 // Attachments, you can handle decoding base64 and saving to file if needed

        //                 email.Attachments.Find()
        //             }
        //         }
        //     }
        // }

        // static string GetBoundary(string line)
        // {
        //     if (line.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase))
        //     {
        //         // Extract boundary from Content-Type
        //         Match match = Regex.Match(line, "boundary=\"(.+?)\"");
        //         if (match.Success)
        //             return match.Groups[1].Value;
        //     }
        //     return "";
        // }

        // public static Email MimeParser(string mime)
        // {
        //     string[] lines = mime.Split("\r\n");
        //     int i = 0; // i is index of lines
        //     string contentType = "";
        //     if (mime.Equals("Content-Type: multipart/mixed; boundary=\"--LLVJuUtfdf3nwd0phoib0wRo"))
        //     {
        //         contentType = lines[i++];
        //     }
        //     string messageID = lines[i++].Substring(lines[i].IndexOf("<") + 1, lines[i].IndexOf(">") - lines[i].IndexOf("<") - 1);
        //     string dateTime = lines[i].Substring(lines[i].IndexOf(" ") + 1);
        //     i += 3;
        //     string Content_Languague = lines[i].Substring(lines[i++].IndexOf(" ") + 1);
        //     string[] to = null!, cc = null!, bcc = null!;
        //     if (lines[i].Contains("To: "))
        //     {
        //         lines[i] = lines[i].Substring(lines[i].IndexOf(" ") + 1);
        //         to = lines[i++].Split(", ");
        //     }
        //     if (lines[i].Contains("CC: "))
        //     {
        //         lines[i] = lines[i].Substring(lines[i].IndexOf(" ") + 1);
        //         cc = lines[i++].Split(", ");
        //     }
        //     if (lines[i].Contains("BCC: "))
        //     {
        //         lines[i] = lines[i].Substring(lines[i].IndexOf(" ") + 1);
        //         bcc = lines[i++].Split(", ");
        //     }
        //     string from = lines[i].Substring(lines[i++].IndexOf(" ") + 1);
        //     string subject = lines[i++].Substring(lines[i++].IndexOf(" ") + 1);
        //     if (lines[i].Contains("Content-Type: text/plain; charset=UTF-8; format=flowed"))
        //     {
        //         i += 3;
        //     }
        //     else
        //     {
        //         i += 6;
        //     }
        //     string body = "";
        //     while (lines[i] != "--LLVJuUtfdf3nwd0phoib0wRo")
        //     {
        //         body += lines[i++];
        //     }
        //     body = body.Remove(body.LastIndexOf("\r\n"));
        //     Email email = new Email(messageID, dateTime, Content_Languague, from, to!, cc!, bcc!, subject, body);
        //     if (contentType.Contains("multipart/mixed"))
        //     {
        //         while (!lines[i].Equals("--LLVJuUtfdf3nwd0phoib0wRo--"))
        //         {
        //             string fileName = lines[++i].Substring(lines[i].IndexOf("\"") + 1, lines[i].Length - 2 - lines[i].IndexOf("\""));
        //             string fileBody = "";
        //             i += 4;
        //             while (lines[i] != "--LLVJuUtfdf3nwd0phoib0wRo")
        //             {
        //                 fileBody += lines[i++];
        //             }
        //             Attachment attachment = new Attachment
        //             {
        //                 FileDir = fileName,
        //                 Data = Convert.FromBase64String(fileBody)
        //             };
        //             email.Attachments.Add(attachment);
        //         }
        //     }
        //     return email;
        // }

    }
}
