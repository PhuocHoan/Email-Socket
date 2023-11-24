using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


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

            if (email.To.Count > 0) {
                header.Append($"To: ");
                for (int i = 0; i < email.To.Count; ++i)
                {
                    header.Append($"{email.To[i]}");
                    if (i != email.To.Count - 1)
                        header.Append(", ");
                }
                header.AppendLine();
            }

            if (email.Cc.Count > 0)
            header.Append($"Cc: ");
            if (email.Cc.Count > 0)
            {
                for (int i = 0; i < email.Cc.Count; ++i)
                {
                    header.Append($"{email.Cc[i]}");
                    if (i != email.Cc.Count - 1)
                        header.Append(", ");
                }
                header.AppendLine();
            }

            if (email.Bcc.Count > 0)
            {
                header.Append($"Bcc: ");
                for (int i = 0; i < email.Bcc.Count; ++i)
                {
                    header.Append($"{email.Bcc[i]}");
                    if (i != email.Bcc.Count - 1)
                        header.Append(", ");
                }
                header.AppendLine();
            }   
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
            return header.ToString();
        }

        public static string GetMimePartHeader(string boundary, string? fileName = null, bool isFile = true)
        {
            StringBuilder header = new StringBuilder();

            header.AppendLine($"{boundary}");
            if (isFile)
            {
                header.AppendLine($"Content-Type: {GetFileContentType(fileName!)};{(GetFileContentType(fileName!) == "text/plain" ? " charset=UTF-8;" : "")} name=\"{Path.GetFileName(fileName)}\"");
                header.AppendLine($"Content-Disposition: attachment; filename=\"{Path.GetFileName(fileName)}\"");
                header.AppendLine($"Content-Transfer-Encoding: base64");
            }
            else
            {
                header.AppendLine($"Content-Type: text/plain; charset=UTF-8; format=flowed");
                header.AppendLine("Content-Transfer-Encoding: 7bit");
            }
            return header.ToString();
        }

        public static Email MimeParser(string mimeMessage)
        {
            Email email = new Email();
            StringReader reader = new StringReader(mimeMessage);

            // Parse headers
            string line;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                string[] headerParts = line.Split(new[] { ':' }, 2);
                if (headerParts.Length == 2)
                {
                    string headerName = headerParts[0].Trim();
                    string headerValue = headerParts[1].Trim();

                    switch (headerName)
                    {
                        case "From":
                            email.From = headerValue;
                            break;
                        case "To":
                            email.To.AddRange(headerValue.Split(',').Select(address => address.Trim()));
                            break;
                        case "Cc":
                            email.Cc.AddRange(headerValue.Split(',').Select(address => address.Trim()));
                            break;
                        case "Bcc":
                            email.Bcc.AddRange(headerValue.Split(',').Select(address => address.Trim()));
                            break;
                        case "Subject":
                            email.Subject = headerValue;
                            break;
                        case "Content-Type":
                            if (headerValue.StartsWith("multipart/mixed", StringComparison.OrdinalIgnoreCase))
                                // Handle multipart/mixed
                                ParseMultipartMixed(reader, email, GetBoundary(line));
                            break;
                    }
                }
            }

            return email;
        }

        private static void ParseMultipartMixed(StringReader reader, Email email, string boundary)
        {
            // Skip until the boundary
            while (!reader.ReadLine().StartsWith(boundary)) { }
            string line;
            // define index of attachment
            int i = 0; 
            // Parse part headers
            Dictionary<string, string> partHeaders = new Dictionary<string, string>();
            while ((line = reader.ReadLine()) != boundary)
            {
                if (line.StartsWith("Content-Type:", StringComparison.OrdinalIgnoreCase))
                {
                    partHeaders["Content-Type"] = line.Substring("Content-Type:".Length).Trim();
                }
                else if (line.StartsWith("Content-Transfer-Encoding:", StringComparison.OrdinalIgnoreCase))
                {
                    partHeaders["Content-Transfer-Encoding"] = line.Substring("Content-Transfer-Encoding:".Length).Trim();
                    // Base64 encoding
                    if (partHeaders["Content-Transfer-Encoding"].Equals("base64"))
                    {
                        // skip 1 line
                        reader.ReadLine();
                        // Parse part body
                        StringBuilder AttachmentContent = new StringBuilder();
                        while ((line = reader.ReadLine()) != boundary)
                            AttachmentContent.Append(line);
                        
                        // Decode base64
                        email.Attachments[i].Data = Convert.FromBase64String(AttachmentContent.ToString());
                        // Save to file
                        File.WriteAllBytes(@$".\{email.Attachments[i].Directory}", email.Attachments[i].Data);
                        ++i;
                    }
                    // 7 bit encoding
                    else {
                        // Skip 1 line
                        reader.ReadLine();
                        // Parse part body
                        StringBuilder body = new StringBuilder();
                        while ((line = reader.ReadLine()) != boundary)
                            body.Append(line);
                        // Remove last 2 characters "\r\n"
                        body.Remove(body.Length - 2, 2);
                        email.Body = body.ToString();
                    }
                }
                else if (line.StartsWith("Content-Disposition:", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract filename from Content-Disposition if needed
                    Match match = Regex.Match(line, "filename=\"(.+?)\"");
                    if (match.Success)
                    {
                        string filename = match.Groups[1].Value;
                        email.Attachments.Add(new Attachment(@$".\{filename}"));
                    }
                }
            }
        }    

        static string GetBoundary(string line)
        {
            // Extract boundary from Content-Type
            Match match = Regex.Match(line, "boundary=\"(.+?)\"");
            if (match.Success)
                return match.Groups[1].Value;
            return "";
        }
    }
}
