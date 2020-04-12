using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSupply.IO
{
    public class ArmorData : ItemData
    {
        [JsonProperty("armor_slot")]
        private string slot = "";

        [JsonIgnore]
        public int SlotID
        {
            get
            {
                switch (slot.ToUpperInvariant())
                {
                    case "HEAD":
                        return 0;
                    case "BODY":
                        return 1;
                    case "LEG":
                        return 2;
                    default:
                        return -1;
                }
            }
        }
    }
}
