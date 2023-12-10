using ConsoleMenu;
using EmailClient;
using EmailConfig;

class Program
{
    static void Main(string[] args)
    {
        // Create folder to store database
        if (!Directory.Exists(".\\Data"))
        {
            Directory.CreateDirectory(".\\Data");
        }

        // Create/Load config file
        ConfigJson config = new ConfigJson();
        if (!File.Exists(Path.GetFullPath("Config.json")))
        {
            config = ConfigJson.CreateDefaultJsonFile();
        }
        else
        {
            string FilePath = Path.GetFullPath("Config.json");
            config = ConfigJson.Load(FilePath);
        }

        Smtp_Client smtp = new Smtp_Client(config);
        Pop3_Client pop3 = new Pop3_Client(config);

        // Create a new thread for auto-retrieve emails each {config.Autoload} seconds
        Task autoLoad = new Task(
            () =>
            {
                while (true)
                {
                    pop3.ReceiveEmail();
                    // task sleep for 10 seconds
                    Thread.Sleep(config.General!.AutoLoad * 1000);
                }
            }
        );

        autoLoad.Start();

        // The whole menu of the program here
        ProgramMenu.MenuRun(smtp);
    }
}