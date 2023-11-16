using System.Collections.Generic;
using System.Text.Json;

using System;

namespace Config
{

    class ConfigJson
    {
        public class Source{

        }
        public void loadJson() {
            // get config.json path on your local computer
            string fullPath = Path.GetFullPath("config.json");
            // read file config.json
            using (StreamReader read = new StreamReader(fullPath))
            {
                Dictionary<string, object> items = JsonSerializer.Deserialize<Dictionary<string, object>>(read.ReadToEnd())!;
                general = (Dictionary<string, string>)items["General"];
                filter = (List<Dictionary<string, string>>)items["Filter"];
            }
        }
    }
}