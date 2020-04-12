using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace AutoSupply.IO
{
    public class BuffData
    {
        private static readonly ReadOnlyDictionary<string, int> BuffDict;

        private bool needParse = true;

        [JsonProperty("buff_name")]
        public string BuffName { get; private set; }

        [JsonIgnore]
        public int ID { get; private set; }

        static BuffData()
        {
            var dict = new Dictionary<string, int>();
            var type = typeof(BuffID);
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.Name == "Count")
                {
                    continue;
                }

                dict.Add(field.Name, (int)field.GetValue(null));
            }

            BuffDict = new ReadOnlyDictionary<string, int>(dict);
        }

        public void Parse()
        {
            if (!needParse)
            {
                return;
            }

            if (int.TryParse(BuffName, out int id))
            {
                ID = id;
            }
            else
            {
                if (BuffDict.TryGetValue(BuffName, out id))
                {
                    ID = id;
                }
                else
                {
                    throw new InvalidOperationException("BuffName did not match:" + BuffName + "\nAvailable Buff Name:\n" + string.Join("\n", BuffDict.Keys));
                }
            }

            needParse = false;
        }
    }
}
