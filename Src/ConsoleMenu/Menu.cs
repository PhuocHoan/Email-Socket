using System.Text;
using EmailClient;
using EmailConfig;
using EmailDatabase;
using EmailHandler;

namespace ConsoleMenu
{
    class Menu
    {
        private int SelectedIndex;
        private string[] Options;
        private string Header;
        private string Prompt;
        public Menu(string header, string prompt, string[] options)
        {
            Header = header;
            Prompt = prompt;
            Options = options;
            SelectedIndex = 0;
        }
        private void Display()
        {
            Console.WriteLine(Header);
            Console.WriteLine();
            Console.WriteLine(Prompt);
            for (int i = 0; i < Options.Length; i++)
            {
                string currentOption = Options[i];
                string prefix;
                
                if (i == SelectedIndex)
                {
                    prefix = "*";
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;
                }
                else
                {
                    prefix = " ";
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                }

                Console.WriteLine($"{prefix} << {currentOption} >>");
            }

            Console.ResetColor();
        }

        public int Run()
        {
            ConsoleKey keyPressed;
            do
            {
                Console.Clear();
                Display();
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                keyPressed = keyInfo.Key;

                if (keyPressed == ConsoleKey.UpArrow)
                {
                    if (SelectedIndex > 0)
                        SelectedIndex--;
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    if (SelectedIndex < Options.Length - 1)
                    SelectedIndex++;
                }

            } while (keyPressed != ConsoleKey.Enter);

            return SelectedIndex;
        }
    }

    class ProgramMenu
    {
        static string mainHeader = @"███╗░░░███╗██╗░░░██╗  ███████╗███╗░░░███╗░█████╗░██╗██╗░░░░░
████╗░████║╚██╗░██╔╝  ██╔════╝████╗░████║██╔══██╗██║██║░░░░░
██╔████╔██║░╚████╔╝░  █████╗░░██╔████╔██║███████║██║██║░░░░░
██║╚██╔╝██║░░╚██╔╝░░  ██╔══╝░░██║╚██╔╝██║██╔══██║██║██║░░░░░
██║░╚═╝░██║░░░██║░░░  ███████╗██║░╚═╝░██║██║░░██║██║███████╗
╚═╝░░░░░╚═╝░░░╚═╝░░░  ╚══════╝╚═╝░░░░░╚═╝╚═╝░░╚═╝╚═╝╚══════╝";
        static string composeHeader = @"███████████████████████████████████████████
█─▄▄▄─█─▄▄─█▄─▀█▀─▄█▄─▄▄─█─▄▄─█─▄▄▄▄█▄─▄▄─█
█─███▀█─██─██─█▄█─███─▄▄▄█─██─█▄▄▄▄─██─▄█▀█
▀▄▄▄▄▄▀▄▄▄▄▀▄▄▄▀▄▄▄▀▄▄▄▀▀▀▄▄▄▄▀▄▄▄▄▄▀▄▄▄▄▄▀";
        static string mailBoxHeader = @"██████████████████████████████████████████
█▄─▀█▀─▄██▀▄─██▄─▄█▄─▄███▄─▄─▀█─▄▄─█▄─▀─▄█
██─█▄█─███─▀─███─███─██▀██─▄─▀█─██─██▀─▀██
▀▄▄▄▀▄▄▄▀▄▄▀▄▄▀▄▄▄▀▄▄▄▄▄▀▄▄▄▄▀▀▄▄▄▄▀▄▄█▄▄▀";
        static string inboxHeader = @"██████████████████████████████
█▄─▄█▄─▀█▄─▄█▄─▄─▀█─▄▄─█▄─▀─▄█
██─███─█▄▀─███─▄─▀█─██─██▀─▀██
▀▄▄▄▀▄▄▄▀▀▄▄▀▄▄▄▄▀▀▄▄▄▄▀▄▄█▄▄▀";
        static string projectHeader = @"██████████████████████████████████████████
█▄─▄▄─█▄─▄▄▀█─▄▄─███▄─▄█▄─▄▄─█─▄▄▄─█─▄─▄─█
██─▄▄▄██─▄─▄█─██─█─▄█─███─▄█▀█─███▀███─███
▀▄▄▄▀▀▀▄▄▀▄▄▀▄▄▄▄▀▄▄▄▀▀▀▄▄▄▄▄▀▄▄▄▄▄▀▀▄▄▄▀▀";
        static string importantHeader = @"████████████████████████████████████████████████████████
█▄─▄█▄─▀█▀─▄█▄─▄▄─█─▄▄─█▄─▄▄▀█─▄─▄─██▀▄─██▄─▀█▄─▄█─▄─▄─█
██─███─█▄█─███─▄▄▄█─██─██─▄─▄███─████─▀─███─█▄▀─████─███
▀▄▄▄▀▄▄▄▀▄▄▄▀▄▄▄▀▀▀▄▄▄▄▀▄▄▀▄▄▀▀▄▄▄▀▀▄▄▀▄▄▀▄▄▄▀▀▄▄▀▀▄▄▄▀▀";
        static string workHeader = @"████████████████████████████
█▄─█▀▀▀█─▄█─▄▄─█▄─▄▄▀█▄─█─▄█
██─█─█─█─██─██─██─▄─▄██─▄▀██
▀▀▄▄▄▀▄▄▄▀▀▄▄▄▄▀▄▄▀▄▄▀▄▄▀▄▄▀";
        static string spamHeader = @"███████████████████████████
█─▄▄▄▄█▄─▄▄─██▀▄─██▄─▀█▀─▄█
█▄▄▄▄─██─▄▄▄██─▀─███─█▄█─██
▀▄▄▄▄▄▀▄▄▄▀▀▀▄▄▀▄▄▀▄▄▄▀▄▄▄▀";

        private static ConfigJson config = ConfigJson.Load(Path.GetFullPath("Config.json"));
        private static string connectionString = $"Data Source=.\\Data\\{config.General!.Username}.db";
        private static EmailDbContext dbContext = new EmailDbContext(connectionString);

        static string[] menuOptions = { "Compose new email", "Check your mailbox", "Exit" };
        static string[] mailBoxOptions = { "Inbox", "Project", "Important", "Work", "Spam", "Back to main menu" };

        static Menu mainMenu = new Menu(mainHeader, "Welcome to our Made-w-love Email Client <3!\nFeel free to navigate through the menu to get started.\n", menuOptions);

        public static void MenuRun(Smtp_Client smtp)
        {
            while (true)
            {
                int selectedIndex = mainMenu.Run();
                switch (selectedIndex)
                {
                    case 0:
                        Email? _newEmail = ComposeMailMenu();
                        if (_newEmail == null)
                            continue;

                        Console.WriteLine("\nSending email... Please wait...");
                        smtp.SendEmail(_newEmail);
                        Console.WriteLine("\nEmail sent successfully! Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case 1:
                        MailBoxMenu();
                        break;
                    case 2:
                        Console.WriteLine("\nThanks for using our application <3!\nPress any key to exit...");
                        Console.ReadKey(true);
                        return;

                }
            }
        }

        public static Email? ComposeMailMenu()
        {
            Console.Clear();
            Console.WriteLine(composeHeader);
            Console.WriteLine();
            Console.WriteLine("<NOTE> Just checking things out? Press Enter to get back to the Main menu!");
            Console.WriteLine("       If you want to include many addresses in \"To\", \"Cc\", \"Bcc\", please seperate them using commas (,).");
            Console.WriteLine("       You can also left these fields blank by pressing Enter: \"Cc\", \"Bcc\", \"Subject\", \"Content\".");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Enter)
                return null;

            Email _newEmail = new Email();
            string temp;
            _newEmail.From = config.General!.Username!;
            Console.Write("\n<*> To: ");
            while (string.IsNullOrEmpty(temp = Console.ReadLine()!))
            {
                Console.WriteLine("There must be at least one recipient!");
                Console.Write("<*> To: ");
            }

            string[] addresses = temp.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string to in addresses)
                _newEmail.To.Add(to);

            Console.Write("<*> Cc: ");
            if (!string.IsNullOrEmpty(temp = Console.ReadLine()!))
            {
                addresses = temp.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string cc in addresses)
                    _newEmail.Cc!.Add(cc);
            }

            Console.Write("<*> Bcc: ");
            if (!string.IsNullOrEmpty(temp = Console.ReadLine()!))
            {
                addresses = temp.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string bcc in addresses)
                    _newEmail.Bcc!.Add(bcc);
            }

            Console.Write("<*> Subject: ");
            if (!string.IsNullOrEmpty(temp = Console.ReadLine()!))
                _newEmail.Subject = temp;

            Console.WriteLine("<*> Content: ");
            if (!string.IsNullOrEmpty(temp = Console.ReadLine()!))
                _newEmail.Body = temp;

            Console.Write("<*> Enter number of attachments: ");
            while (string.IsNullOrEmpty(temp = Console.ReadLine()!))
            {
                Console.WriteLine("Invalid input!");
                Console.Write("<*> Enter number of attachments: ");
            }
            while (int.Parse(temp!) < 0)
            {
                Console.WriteLine("Invalid number, must be at least 0!");
                Console.Write("<*> Enter number of attachments: ");
                temp = Console.ReadLine()!;
            }

            int numOfFiles = int.Parse(temp);
            int i = 1;
            while (i <= numOfFiles)
            {
                Console.Write($" + File #{i} path: ");
                if (string.IsNullOrEmpty(temp = Console.ReadLine()!))
                {
                    Console.WriteLine("File path can't be empty!");
                    continue;
                }

                try
                {
                    Mime.GetFileContentType(temp);
                    _newEmail.AddAttachment(temp);
                    i++;
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("File not found!");
                }
                catch (NotSupportedException)
                {
                    Console.WriteLine("This file type is not supported! Please try again.");
                }
            }

            return _newEmail;
        }

        public static void MailBoxMenu()
        {
            Menu mailBoxMenu = new Menu(mailBoxHeader, "Please choose your desired email folder.\n", mailBoxOptions);
            int selectedIndex = mailBoxMenu.Run();

            switch (selectedIndex)
            {
                case 0:
                    CustomEmailMenu("Inbox");
                    break;
                case 1:
                    CustomEmailMenu("Project");
                    break;
                case 2:
                    CustomEmailMenu("Important");
                    break;
                case 3:
                    CustomEmailMenu("Work");
                    break;
                case 4:
                    CustomEmailMenu("Spam");
                    break;
            }
        }

        public static void CustomEmailMenu(string menu)
        {
            List<Email> emailList = dbContext.GetEmailsByFolder(menu);
            string[] emails = new string[emailList.Count + 1];
            emails[0] = "Back to Mailbox menu";
            for (int i = 0; i < emailList.Count; ++i)
                emails[i + 1] = EmailTag(emailList[i]);

            int selectedIndex = 0;
            switch (menu)
            {
                case "Inbox":
                    Menu inboxMenu = new Menu(inboxHeader, "All of your non-filtered emails are here.\n", emails);
                    selectedIndex = inboxMenu.Run();
                    break;
                case "Project":
                    Menu projectMenu = new Menu(projectHeader, "All of your project-related emails are here.\n", emails);
                    selectedIndex = projectMenu.Run();
                    break;
                case "Important":
                    Menu importantMenu = new Menu(importantHeader, "All of your important emails are here.\n", emails);
                    selectedIndex = importantMenu.Run();
                    break;
                case "Work":
                    Menu workMenu = new Menu(workHeader, "All of your non-filtered emails are here.\n", emails);
                    selectedIndex = workMenu.Run();
                    break;
                case "Spam":
                    Menu spamMenu = new Menu(spamHeader, "All of your spam emails are here.\n", emails);
                    selectedIndex = spamMenu.Run();
                    break;
            }
            
            Console.Clear();
            if (selectedIndex == 0)
            {
                MailBoxMenu();
            }
            else
            {
                // Update the read status  
                dbContext.UpdateEmailStatus(emailList[selectedIndex - 1].MessageId!, true);
                EmailViewer(emailList[selectedIndex - 1], menu);
            }
        }

        public static void EmailViewer(Email email, string fromMenu)
        {
            Console.WriteLine("From: " + email.From);

            if (email.To.Count > 0)
            {
                Console.Write("To: ");
                for (int i = 0; i < email.To.Count; ++i)
                {
                    if (i != email.To.Count - 1)
                        Console.Write(email.To[i] + ", ");
                    else
                        Console.Write(email.To[i]);
                }
            }
            
            if (email.Cc!.Count > 0)
            {
                Console.Write("\nCc: ");
                for (int i = 0; i < email.Cc.Count; ++i)
                {
                    if (i != email.Cc.Count - 1)
                        Console.Write(email.Cc[i] + ", ");
                    else
                        Console.Write(email.Cc[i]);
                }
            }

            HashSet<string> bccDetector = new HashSet<string>();
            foreach (string address in email.To)
                bccDetector.Add(address);
            foreach (string address in email.Cc)
                bccDetector.Add(address);
            if (!bccDetector.Contains(config.General!.Username!))
            {
                Console.WriteLine($"\nBcc: You <{config.General!.Username!}>" );
            }
            

            if (string.IsNullOrEmpty(email.Subject))
                Console.WriteLine("\nSubject: Email with no subject.");
            else
                Console.WriteLine("\nSubject: " + email.Subject);
            Console.WriteLine();

            if (string.IsNullOrEmpty(email.Body))
                Console.WriteLine("(empty)");
            else
                Console.WriteLine(email.Body);
            Console.WriteLine();

            if (email.Attachments.Count > 0)
            {
                Console.WriteLine("Attachments: ");
                foreach (var attachment in email.Attachments)
                    Console.WriteLine("  <*> " + attachment.FileName);

                Console.WriteLine("\nDo you want to save attachments to computer? ");
                do
                {
                    Console.Write("Press y/n: ");
                    char c;
                    char.TryParse(Console.ReadLine(), out c);
                    if (char.ToLower(c).Equals('y'))
                    {
                        string? folder;
                        do
                        {
                            Console.Write("Enter your save location (without file name): ");
                            folder = Console.ReadLine();
                            if (string.IsNullOrEmpty(folder))
                                Console.WriteLine("Invalid folder! Please re-enter!");
                        } while (string.IsNullOrEmpty(folder));

                        foreach (var attachment in email.Attachments)
                        {
                            Mime.SaveFile(folder, attachment.FileName!, attachment.Data!);
                            dbContext.UpdateAttachmentFilePath(email.MessageId!, folder + attachment.FileName);
                        }

                        break;
                    }
                    else if (char.ToLower(c).Equals('n'))
                        break;
                    else
                        Console.WriteLine("Invalid choice! Please try again.");
                } while (true);
            }

            Console.Write("\nPress any key to go back...");
            Console.ReadKey(true);
            switch (fromMenu)
            {
                case "Inbox":   
                    CustomEmailMenu("Inbox");
                    break;
                case "Project":
                    CustomEmailMenu("Project");
                    break;
                case "Important":
                    CustomEmailMenu("Important");
                    break;
                case "Work":
                    CustomEmailMenu("Work");
                    break;
                case "Spam":
                    CustomEmailMenu("Spam");
                    break;
            }
        }

        public static string EmailTag(Email email)
        {
            StringBuilder tag = new StringBuilder();
            if (!email.Status)
                tag.Append("(Unread) ");
            tag.Append($"{email.From} - ");
            if (string.IsNullOrEmpty(email.Subject))
                tag.Append("Email with no subject.");
            else tag.Append(email.Subject);

            return tag.ToString();
        }
    }
}