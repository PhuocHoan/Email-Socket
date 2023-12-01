using System.Text.Json;

namespace Email_Config
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

        // Create default config.json if not having config.json yet
        public static ConfigJson CreateDefaultJsonFile()
        {
            ConfigJson config = new ConfigJson();
            config.General = new GeneralSettings();
            config.General.Username = "levana@gmail.com";
            config.General.Password = "12345678";
            config.General.MailServer = "127.0.0.1";
            config.General.SmtpPort = 25;
            config.General.Pop3Port = 110;
            config.General.AutoLoad = 10;

            config.Filters = new List<Filter>();
            Filter filter = new Filter();

            filter.Criteria = "From";
            filter.Values = new List<string>() {"ahihi@testing.com", "ahuu@testing.com"};
            filter.Folder = "Project";
            config.Filters.Add(filter);

            filter.Criteria = "Subject";
            filter.Values = new List<string>() {"urgent", "ASAP"};
            filter.Folder = "Important";
            config.Filters.Add(filter);

            filter.Criteria = "Content";
            filter.Values = new List<string>() {"report", "meeting"};
            filter.Folder = "Work";
            config.Filters.Add(filter);

            filter.Criteria = "Spam";
            filter.Values = new List<string>() {"virus", "hack", "crack"};
            filter.Folder = "Spam";
            config.Filters.Add(filter);

            SaveNewFile("Config.json", config);
            return config;
        }

        // Load data from config.json
        public static ConfigJson Load(string FileName)
        {
            using FileStream stream = new(FileName, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<ConfigJson>(stream);
        }

        // create new config.json if not having one and save data to it
        public static void SaveNewFile(string FileName, ConfigJson config)
        {
            using FileStream stream = new(FileName, FileMode.CreateNew, FileAccess.Write);
            JsonSerializer.Serialize(stream, config, new JsonSerializerOptions { WriteIndented = true, AllowTrailingCommas = true });
        }

        // save data to config.json
        public static void Save(string FolderPath, ConfigJson config)
        {
            using FileStream stream = new(FolderPath, FileMode.Truncate, FileAccess.Write);
            JsonSerializer.Serialize(stream, config, new JsonSerializerOptions { WriteIndented = true, AllowTrailingCommas = true });
        }
    }
}