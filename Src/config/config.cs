using System.Text.Json;

namespace EmailSocket
{
    class GeneralSettings
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? MailServer { get; set; }
        public short SmtpPort { get; set; }
        public short Pop3Port { get; set; }
        public short AutoLoad { get; set; }
    }
    class Filter
    {
        public string? Criteria { get; set; }
        public List<string>? Values { get; set; }
        public string? Folder { get; set; }
    }
    class ConfigJson
    {
        public GeneralSettings? General { get; set; }
        public List<Filter>? Filters { get; set; }
        
        public static ConfigJson LoadData()
        {
            // if not having config.json yet, create one
            ConfigJson config = new ConfigJson();
            config.General = new GeneralSettings();
            config.Filters = new List<Filter>();

            Console.WriteLine("General information");

            Console.Write("Username: ");
            config.General.Username = Console.ReadLine();

            Console.Write("Password: ");
            config.General.Password = Console.ReadLine();

            Console.Write("IP mail server: ");
            config.General.MailServer = Console.ReadLine();

            while (true) {
                try {
                    Console.Write("Smtp port: ");
                    config.General.SmtpPort = Convert.ToInt16(Console.ReadLine());
                    break;
                }
                catch (FormatException) {
                    Console.WriteLine("Invalid input, please type again (value must be a number)");
                }
            }
            while (true) {
                try {
                    Console.Write("Pop3 port: ");
                    config.General.Pop3Port = Convert.ToInt16(Console.ReadLine());
                    break;
                }
                catch (FormatException) {
                    Console.WriteLine("Invalid input, please type again (value must be a number)");
                }
            }
            while (true) {
                try {
                    Console.Write("Autoload time: ");
                    config.General.AutoLoad = Convert.ToInt16(Console.ReadLine());
                    break;
                }
                catch (FormatException) {
                    Console.WriteLine("Invalid input, please type again (value must be a number)");
                }
            }

            Console.WriteLine("Mail filter list");
            while (true) {
                Filter filter = new Filter();
                
                Console.Write("Criteria: ");
                filter.Criteria = Console.ReadLine();

                Console.Write("Values: ");
                string[] values = Console.ReadLine().Split(',');
                filter.Values = new List<string>();
                for (int i = 0; i < values.Length; ++i)
                {
                    values[i] = values[i].Trim();
                    filter.Values.Add(values[i]);
                }

                Console.Write("Folder: ");
                filter.Folder = Console.ReadLine();

                config.Filters.Add(filter);

                Console.Write("Wanting to exit, please type (yes or no): ");
                string choice = Console.ReadLine().ToLower();
                if (choice == "yes" || choice == "y") {
                    break;
                }
            }

            return config;
        }

        public static void ChangeData(ref ConfigJson config) {
            // if having data from config.json already
            while (true) {
                Console.WriteLine("(File Config) Choose what type you want to change: ");
                Console.WriteLine("1: General information");
                Console.WriteLine("2: Mail filter list");
                Console.WriteLine("3: Exit");
                short choice;
                while (true) {
                    try {
                        choice = Convert.ToInt16(Console.ReadLine());
                        break;
                    }
                    catch (FormatException) {
                        Console.WriteLine("Invalid input, please type again (value must be a number)");
                    }
                }
                if (choice == 1) {
                    while (true) {
                        Console.WriteLine("(General information) Choose category you want to change");
                        Console.WriteLine("1: Username");
                        Console.WriteLine("2: Password");
                        Console.WriteLine("3: IP mail server");
                        Console.WriteLine("4: Smtp port");
                        Console.WriteLine("5: Pop3 port");
                        Console.WriteLine("6: Autoload time");
                        Console.WriteLine("7: Exit");
                        while (true) {
                            try {
                                choice = Convert.ToInt16(Console.ReadLine());
                                break;
                            }
                            catch (FormatException) {
                                Console.WriteLine("Invalid input, please type again (value must be a number)");
                            }
                        }
                        if (choice < 1 || choice > 6) {
                            break;
                        }
                        switch (choice) {
                            case 1:
                                Console.Write("Username: ");
                                config.General.Username = Console.ReadLine();
                                break;
                            case 2:
                                Console.Write("Password: ");
                                config.General.Password = Console.ReadLine();
                                break;
                            case 3:
                                Console.Write("IP mail server: ");
                                config.General.Password = Console.ReadLine();
                                break;
                            case 4:
                                while (true) {
                                    try {
                                        Console.Write("Smtp port: ");
                                        config.General.SmtpPort = Convert.ToInt16(Console.ReadLine());
                                        break;
                                    }
                                    catch (FormatException) {
                                        Console.WriteLine("Invalid input, please type again (value must be a number)");
                                    }
                                }
                                break;
                            case 5:
                                while (true) {
                                    try {
                                        Console.Write("Pop3 port: ");
                                        config.General.Pop3Port = Convert.ToInt16(Console.ReadLine());
                                        break;
                                    }
                                    catch (FormatException) {
                                        Console.WriteLine("Invalid input, please type again (value must be a number)");
                                    }
                                }
                                break;
                            case 6:
                                while (true) {
                                    try {
                                        Console.Write("Autoload time: ");
                                        config.General.AutoLoad = Convert.ToInt16(Console.ReadLine());
                                        break;
                                    }
                                    catch (FormatException) {
                                        Console.WriteLine("Invalid input, please type again (value must be a number)");
                                    }
                                }
                                break;
                        }
                    }
                }
                else if (choice == 2) {
                    while (true) {
                        Console.WriteLine("(Mail filter list) Choose");
                        Console.WriteLine("1: change data");
                        Console.WriteLine("2: add new data");
                        Console.WriteLine("3: Exit");
                        while (true) {
                            try {
                                choice = Convert.ToInt16(Console.ReadLine());
                                break;
                            }
                            catch (FormatException) {
                                Console.WriteLine("Invalid input, please type again (value must be a number)");
                            }
                        }
                        if (choice < 1 || choice > 2) {
                            break;
                        }
                        if (choice == 1) {
                            for (int i = 0; i < config.Filters.Count(); ++i) {
                                Console.WriteLine("Criteria: " + config.Filters[i].Criteria);
                                Console.Write("Values: ");
                                foreach (var x in config.Filters[i].Values) {
                                    Console.Write(x + ", ");
                                }
                                Console.WriteLine();
                                Console.WriteLine("Folder: " + config.Filters[i].Folder);

                                Console.WriteLine("(Change) Choose what category you want to change");
                                Console.WriteLine("1: Criteria");
                                Console.WriteLine("2: Values");
                                Console.WriteLine("3: Folder");
                                Console.WriteLine("4: Continue");
                                Console.WriteLine("5: Exit");
                                while (true) {
                                    try {
                                        choice = Convert.ToInt16(Console.ReadLine());
                                        break;
                                    }
                                    catch (FormatException) {
                                        Console.WriteLine("Invalid input, please type again (value must be a number)");
                                    }
                                }
                                if (choice < 1 || choice > 4) {
                                    break;
                                }
                                switch (choice) {
                                    case 1:
                                        Console.Write("Criteria: ");
                                        config.Filters[i].Criteria = Console.ReadLine();
                                        break;
                                    case 2:
                                        for (int j = 0; j < config.Filters[i].Values.Count(); ++j) {
                                            Console.WriteLine(config.Filters[i].Values[j]);
                                            Console.WriteLine("(Values) Choose:");
                                            Console.WriteLine("1: Change");
                                            Console.WriteLine("2: Continue");
                                            Console.WriteLine("3: Add");
                                            Console.WriteLine("4: Exit");
                                            while (true) {
                                                try {
                                                    choice = Convert.ToInt16(Console.ReadLine());
                                                    break;
                                                }
                                                catch (FormatException) {
                                                    Console.WriteLine("Invalid input, please type again (value must be a number)");
                                                }
                                            }
                                            if (choice == 1) {
                                                Console.WriteLine("Enter value: ");
                                                config.Filters[i].Values[j] = Console.ReadLine().Trim();
                                            }
                                            else if (choice == 2) {
                                                continue;
                                            }
                                            else if (choice == 3) {
                                                Console.WriteLine("Enter value: ");
                                                config.Filters[i].Values.Add(Console.ReadLine().Trim());
                                            }
                                            else {
                                                break;
                                            }
                                        }
                                        break;
                                    case 3:
                                        Console.Write("Folder: ");
                                        config.Filters[i].Folder = Console.ReadLine();
                                        break;
                                }
                            }
                        }
                        else {
                            while (true) {
                                Filter filter = new Filter();
        
                                Console.Write("Criteria: ");
                                filter.Criteria = Console.ReadLine();

                                Console.Write("Values: ");
                                filter.Values = new List<string>();
                                string[] values = Console.ReadLine().Split(',');
                                for (int i = 0; i < values.Length; ++i)
                                {
                                    values[i] = values[i].Trim();
                                    filter.Values.Add(values[i]);
                                }

                                Console.Write("Folder: ");
                                filter.Folder = Console.ReadLine();

                                config.Filters.Add(filter);
                            
                                Console.Write("Wanting to exit, please type (yes or no): ");
                                string c = Console.ReadLine().ToLower();
                                if (c == "yes" || c == "y") {
                                    break;
                                }
                            }
                        }
                    }
                }
                else {
                    break;
                }
            }
        }

        public static ConfigJson Load(string file)
        {
            using FileStream stream = new(file, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<ConfigJson>(stream);
        }

        public static void SaveNewFile(string file, ConfigJson config)
        {
            using FileStream stream = new(file, FileMode.CreateNew, FileAccess.Write);
            JsonSerializer.Serialize(stream, config, new JsonSerializerOptions { WriteIndented = true, AllowTrailingCommas = true });
        }
        
        public static void Save(string file, ConfigJson config)
        {
            using FileStream stream = new(file, FileMode.Truncate, FileAccess.Write);
            JsonSerializer.Serialize(stream, config, new JsonSerializerOptions { WriteIndented = true, AllowTrailingCommas = true });
        }

    }
}