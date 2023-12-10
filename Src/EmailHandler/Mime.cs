using System.Text;
using System.Text.RegularExpressions;

namespace EmailHandler
{
    public class Mime
    {
        private static Dictionary<string, string> ContentType = new Dictionary<string, string>
        {
            {".7z", "application/x-7z-compressed"},
            {".bin", "application/octet-stream"},
            {".csv", "text/csv"},
            {".doc", "application/msword"},
            {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
            {".gif", "image/gif"},
            {".jar", "application/java-archive"},
            {".jpe", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".json", "application/json"},
            {".mp3", "audio/mpeg"},
            {".mp4", "video/mp4"},
            {".pdf", "application/pdf"},
            {".png", "image/png"},
            {".ppt", "application/vnd.ms-powerpoint"},
            {".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation"},
            {".rar", "application/octet-stream"},
            {".tar", "application/x-tar"},
            {".txt", "text/plain"},
            {".wav", "audio/wav"},
            {".xls", "application/vnd.ms-excel"},
            {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
            {".zip", "application/x-zip-compressed"},
        };

        public static string GetFileContentType(string fileName)
        {
            string? extension = Path.GetExtension(fileName).ToLower();
            if (ContentType.TryGetValue(extension, out string? result))
                return result;
            else throw new NotSupportedException();
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
            header.AppendLine($"Message-ID: <{Guid.NewGuid()}@{email.From!.Split(new[] { '@' }, 2)[1]}>");

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

            if (email.Cc!.Count > 0)
            {
                header.Append($"Cc: ");
                for (int i = 0; i < email.Cc.Count; ++i)
                {
                    header.Append($"{email.Cc[i]}");
                    if (i != email.Cc.Count - 1)
                        header.Append(", ");
                }
                header.AppendLine();
            }

            if (!string.IsNullOrEmpty(email.Subject))
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

            header.AppendLine($"--{boundary}");
            if (isFile)
            {
                header.AppendLine($"Content-Type: {GetFileContentType(fileName!)}; name=\"{Path.GetFileName(fileName)}\"");
                header.AppendLine($"Content-Disposition: attachment; filename=\"{Path.GetFileName(fileName)}\"");
                header.AppendLine($"Content-Transfer-Encoding: {(GetFileContentType(fileName!).Equals("text/plain") ? "7bit" : "base64")}");
            }
            else
            {
                header.AppendLine($"Content-Type: text/plain; charset=UTF-8; format=flowed");
                header.AppendLine("Content-Transfer-Encoding: 7bit");
            }

            header.AppendLine();

            return header.ToString();
        }

        public static Email MimeParser(string mimeMessage)
        {
            Email email = new Email();
            StringReader reader = new StringReader(mimeMessage);

            // Parse headers
            string line;
            while (!string.IsNullOrEmpty(line = reader.ReadLine()!))
            {
                string[] headerParts = line.Split(new[] { ':' }, 2);
                if (headerParts.Length == 2)
                {
                    string headerName = headerParts[0].Trim();
                    string headerValue = headerParts[1].Trim();

                    switch (headerName)
                    {
                        case "Message-ID":
                            Match match = Regex.Match(headerValue, "<(.+?)>");
                            if (match.Success)
                                email.MessageId = match.Groups[1].Value;
                            else
                                email.MessageId = null;
                            break;
                        case "From":
                            email.From = headerValue;
                            break;
                        case "To":
                            email.To.AddRange(headerValue.Split(',').Select(address => address.Trim()));
                            break;
                        case "Cc":
                            email.Cc!.AddRange(headerValue.Split(',').Select(address => address.Trim()));
                            break;
                        case "Bcc":
                            email.Bcc!.AddRange(headerValue.Split(',').Select(address => address.Trim()));
                            break;
                        case "Subject":
                            email.Subject = headerValue;
                            break;
                        case "Content-Type":
                            if (headerValue.StartsWith("multipart/mixed", StringComparison.OrdinalIgnoreCase))
                                // Handle multipart/mixed
                                ParseMultipartMixed(new StringReader(mimeMessage), email, GetBoundary(line));
                            else
                            {
                                // Simply just search for the body part
                                ParseSimplePart(new StringReader(mimeMessage), email);
                            }
                            break;
                    }
                }
            }

            return email;
        }

        private static void ParseSimplePart(StringReader reader, Email email)
        {
            // Skip until the body
            string? line;
            while (!string.IsNullOrEmpty(line = reader.ReadLine())) { }

            // Parse part body
            string body = reader.ReadToEnd();
            email.Body = body;
        }

        private static void ParseMultipartMixed(StringReader reader, Email email, string boundary)
        {
            // Skip until the boundary
            string? line;
            do
            {
                line = reader.ReadLine();
            } while (string.IsNullOrEmpty(line) ? true : !line.StartsWith("--" + boundary));
            // define index of attachment
            int i = 0;
            // Parse part headers
            Dictionary<string, string> partHeaders = new Dictionary<string, string>();
            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
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
                        while ((line = reader.ReadLine()) != ("--" + boundary))
                        {
                            if (line == "--" + boundary + "--")
                            {
                                break;
                            }
                            AttachmentContent.Append(line);
                        }
                        // Decode base64
                        email.Attachments[i].Data = Convert.FromBase64String(AttachmentContent.ToString());
                        ++i;
                    }
                    // 7 bit encoding
                    else
                    {
                        // Skip 1 line
                        reader.ReadLine();
                        // Parse part body
                        StringBuilder body = new StringBuilder();
                        while ((line = reader.ReadLine()) != ("--" + boundary))
                            body.AppendLine(line);
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
                        email.Attachments.Add(new Attachment(@$"{filename}"));
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

        public static bool SaveFile(string folderPath, string fileName, byte[] fileContent)
        {
            // Combine the folder path and file name to get the full path
            string filePath = Path.Combine(folderPath, fileName);

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Can't find the folder! Please try again.");
                return false;
            }

            // Check if the file already exists
            if (File.Exists(filePath))
            {
                Console.WriteLine($"File '{fileName}' already exists in the folder. Choose a different name or handle accordingly.");
                return false;
            }
            else
            {
                // Save the file
                File.WriteAllBytes(filePath, fileContent);
                Console.WriteLine($"File '{fileName}' saved to the folder.");
                return true;
            }
        }

    }
}