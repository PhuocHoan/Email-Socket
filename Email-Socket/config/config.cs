using System.Text.Json;

namespace Config
{
    class GeneralSettings {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? MailServer { get; set; }
        public short SmtpPort { get; set; }
        public short Pop3Port { get; set; }
        public short AutoLoad { get; set; }
    }
    class Filter {
        public  string? Criteria { get; set; }
        public  List<string>? Values { get; set; }
        public  string? Folder { get; set; }
    }
    class ConfigJson
    {
        public static GeneralSettings? General { get; set; }
        public static List<Filter>? Filters { get; set; }

        public static ConfigJson Load(string file)
        {
            using FileStream stream = new(file, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize<ConfigJson>(stream)!;
        }
        public static void Save(string file, ConfigJson config)
        {
            using FileStream stream = new(file, FileMode.Truncate, FileAccess.Write);
            JsonSerializer.Serialize(stream, config);
        }
    }
}