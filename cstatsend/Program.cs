using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;

namespace cstatsend
{
    class Program
    {
        public static Settings settings;

        static void Main(string[] args)
        {
            string thisPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string settingsFile = File.ReadAllText(Path.Combine(thisPath, "cstatsend.json"));

            settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(settingsFile);

            Stats stats = new Stats()
            {
                key = settings.key,
                uid = settings.uid
            };

            string statsJSON = Newtonsoft.Json.JsonConvert.SerializeObject(stats);

            Task.Run(async () =>
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage resp = await client.PostAsync(settings.url, new StringContent(statsJSON));
            }).Wait();

            
        }
        
    }
}
