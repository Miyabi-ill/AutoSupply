using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSupply.IO
{
    public class MiscData : ItemData
    {
        [JsonProperty("misc_slot")]
        string slot = "";

        [JsonIgnore]
        public int SlotID
        {
            get
            {
                switch (slot.ToUpperInvariant())
                {
                    case "PET":
                        return 0;
                    case "LIGHT_PET":
                        return 1;
                    case "MINECART":
                        return 2;
                    case "MOUNT":
                        return 3;
                    case "HOOK":
                        return 4;
                    default:
                        return -1;
                }
            }
        }
    }
}
