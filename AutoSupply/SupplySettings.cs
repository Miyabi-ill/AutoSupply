using AutoSupply.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSupply
{
    public class SupplySettings
    {
        public const string GMSET_NAME = "GM-XXXXXX";
        private const string USE_DEFAULT_CONFIG_MESSAGE = "Supply Settings: Using default configs.";

        public SupplySettings(List<SupplySet> supply_sets, List<MapData> maps)
        {
            SupplySets = supply_sets;
            Maps = maps;

            DefaultSet = SupplySets?.FirstOrDefault(x => x.Name == "DEFAULT");
            DefaultMap = Maps?.FirstOrDefault(x => x.Name == "DEFAULT");

            GMSet = SupplySets?.FirstOrDefault(x => x.Name == GMSET_NAME);

            if (supply_sets != null)
            {
                foreach (var supply in supply_sets)
                {
                    supply.Parse();
                }
            }
        }

        [JsonProperty("supply_sets")]
        public List<SupplySet> SupplySets { get; }

        [JsonProperty("maps")]
        public List<MapData> Maps { get; }

        [JsonProperty("supply_command")]
        public string SupplyCommand = "change";

        [JsonProperty("buff_time")]
        [DefaultValue(216000)]
        public int BuffTime { get; } = 216000;

        [JsonIgnore]
        public SupplySet DefaultSet { get; }

        [JsonIgnore]
        public MapData DefaultMap { get; }

        [JsonIgnore]
        public SupplySet GMSet { get; }

        public static SupplySettings Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine(USE_DEFAULT_CONFIG_MESSAGE);
                var settings = new SupplySettings(new List<SupplySet>(), new List<MapData>());
                
                return settings;
            }
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Read(fs);
            }
        }

        public static SupplySettings Read(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<SupplySettings>(sr.ReadToEnd());
            }
        }
    }
}
