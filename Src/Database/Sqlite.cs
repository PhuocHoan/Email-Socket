using EmailHandler;
using Microsoft.Data.Sqlite;

namespace EmailDatabase
{
    public class EmailDbContext
    {
        private readonly string connectionString;

        // Create the database file if it doesn't exist
        public EmailDbContext(string connectionString)
        {
            this.connectionString = connectionString;

            // Create tables if they don't exist
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Emails (
                            MessageId TEXT PRIMARY KEY,
                            Folder TEXT NOT NULL,
                            FromAddress TEXT NOT NULL,
                            ToAddresses TEXT,
                            CcAddresses TEXT,
                            Subject TEXT,
                            Body TEXT,
                            Status INTEGER NOT NULL CHECK (Status IN (0, 1))
                        );

                        CREATE TABLE IF NOT EXISTS Attachments (
                            MessageId TEXT,
                            FileName TEXT NOT NULL,
                            FilePath TEXT,
                            Content BLOB,
                            PRIMARY KEY (MessageId, FileName),
                            FOREIGN KEY (MessageId) REFERENCES Emails (MessageId)
                        );
                    ";
                    command.ExecuteNonQuery();
                }
            }
        }

        // Insert an email into the database
        public void AddEmail(Email email, string folder)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    // Check if the email already exists
                    bool emailExists = EmailExists(connection, email.MessageId!);
                    if (!emailExists)
                    {
                        // If the email doesn't exist, insert it
                        command.CommandText = $@"
                            INSERT INTO Emails (MessageId, Folder, FromAddress, ToAddresses, CcAddresses, Subject, Body, Status)
                            VALUES (@MessageId, @Folder, @FromAddress, @ToAddresses, @CcAddresses, @Subject, @Body, @Status)
                        ";

                        command.Parameters.AddWithValue("@MessageId", email.MessageId);
                        command.Parameters.AddWithValue("@Folder", folder); // This is where the folder information is inserted
                        command.Parameters.AddWithValue("@FromAddress", email.From);
                        command.Parameters.AddWithValue("@ToAddresses", (email.To.Count > 0) ? string.Join(",", email.To) : DBNull.Value);
                        command.Parameters.AddWithValue("@CcAddresses", (email.Cc!.Count > 0) ? string.Join(",", email.Cc) : DBNull.Value);
                        command.Parameters.AddWithValue("@Subject", email.Subject != null ? email.Subject : DBNull.Value);
                        command.Parameters.AddWithValue("@Body", email.Body != null ? email.Body : DBNull.Value);
                        command.Parameters.AddWithValue("@Status", email.Status);

                        command.ExecuteNonQuery();
                        // Insert attachments into the common Attachments table
                        foreach (var attachment in email.Attachments)
                        {
                            // Check if the attachment already exists
                            bool attachmentExists = AttachmentExists(connection, email.MessageId!, attachment.FileName!);
                            if (!attachmentExists)
                            {
                                using (SqliteCommand attachmentCommand = connection.CreateCommand())
                                {
                                    attachmentCommand.CommandText = $@"
                                        INSERT INTO Attachments (MessageId, FileName, FilePath, Content)
                                        VALUES (@MessageId, @FileName, @FilePath, @Content)
                                    ";

                                    attachmentCommand.Parameters.AddWithValue("@MessageId", email.MessageId);
                                    attachmentCommand.Parameters.AddWithValue("@FileName", attachment.FileName);
                                    attachmentCommand.Parameters.AddWithValue("@FilePath", attachment.FilePath);
                                    attachmentCommand.Parameters.AddWithValue("@Content", attachment.Data != null ? attachment.Data : DBNull.Value); // Assuming Content is byte[]

                                    attachmentCommand.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool EmailExists(SqliteConnection connection, string messageId)
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Emails WHERE MessageId = @MessageId";
                command.Parameters.AddWithValue("@MessageId", messageId);

                // Execute the query and check if the email exists
                int cnt = Convert.ToInt32(command.ExecuteScalar());
                return cnt > 0;
            }
        }

        private bool AttachmentExists(SqliteConnection connection, string messageId, string fileName)
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Attachments WHERE MessageId = @MessageId AND FileName = @FileName";
                command.Parameters.AddWithValue("@MessageId", messageId);
                command.Parameters.AddWithValue("@FileName", fileName);

                // Execute the query and check if the attachment exists
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        // Retrieve an email by folder
        public List<Email> GetEmailsByFolder(string folder)
        {
            List<Email> emails = new List<Email>();

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            e.MessageId, 
                            e.FromAddress, 
                            e.ToAddresses, 
                            e.CcAddresses,
                            e.Subject, 
                            e.Body, 
                            e.Folder,
                            e.Status,
                            a.FileName,
                            a.FilePath,
                            a.Content
                        FROM Emails e
                        LEFT JOIN Attachments a ON e.MessageId = a.MessageId
                        WHERE e.Folder = @Folder
                    ";

                    command.Parameters.AddWithValue("@Folder", folder);

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string messageId = reader["MessageId"].ToString()!;

                            // Check if the email is already in the list
                            Email email = emails.Find(e => e.MessageId == messageId)!;

                            if (email == null)
                            {
                                string? cc = reader["CcAddresses"].ToString();
                                email = new Email
                                {
                                    MessageId = messageId,
                                    From = reader["FromAddress"].ToString()!,
                                    To = reader["ToAddresses"].ToString()!.Split(',').ToList(),
                                    Cc = !string.IsNullOrEmpty(cc) ? cc.Split(',').ToList() : new List<string>(),
                                    Subject = reader["Subject"].ToString(),
                                    Body = reader["Body"].ToString(),
                                    Status = reader["Status"].ToString()!.Equals("0") ? false : true,
                                    Attachments = new List<Attachment>()
                                };

                                emails.Add(email);
                            }

                            // Add attachments to the email
                            if (!reader.IsDBNull(reader.GetOrdinal("FileName")))
                            {
                                Attachment attachment = new Attachment
                                {
                                    FileName = reader["FileName"] is DBNull ? null : reader["FileName"].ToString(),
                                    FilePath = reader["FilePath"] is DBNull ? null : reader["FilePath"].ToString(),
                                    Data = reader["Content"] is DBNull ? null : reader["Content"] as byte[]
                                };

                                email.Attachments.Add(attachment);
                            }
                        }
                    }
                }
            }
            return emails;
        }
        // Update the status of an email
        public void UpdateEmailStatus(string messageId, bool newStatus)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Emails 
                        SET Status = @Status 
                        WHERE MessageId = @MessageId
                    ";
                    command.Parameters.AddWithValue("@Status", newStatus);
                    command.Parameters.AddWithValue("@MessageId", messageId);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Update the file path of an attachment
        public void UpdateAttachmentFilePath(string messageId, string newFilePath)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (SqliteCommand command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Attachments 
                        SET FilePath = @FilePath 
                        WHERE MessageId = @MessageId
                    ";
                    command.Parameters.AddWithValue("@FilePath", newFilePath);
                    command.Parameters.AddWithValue("@MessageId", messageId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}