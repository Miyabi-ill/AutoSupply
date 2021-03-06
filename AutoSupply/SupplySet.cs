using AutoSupply.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSupply
{
    public class SupplySet
    {
        [JsonProperty("available_team")]
        [DefaultValue("all")]
        private string team = "all";

        private string name;

        private bool needParse = true;

        [JsonIgnore]
        public Guid ID { get; } = Guid.NewGuid();

        [JsonProperty("supply_name")]
        public string Name
        {
            get { return name; }
            private set { name = value.ToUpperInvariant(); }
        }

        [JsonIgnore]
        public List<int> TeamIDs { get; private set; }

        [JsonProperty("heal_on_respawn")]
        [DefaultValue(false)]
        public bool HealOnRespawn { get; }

        [JsonProperty("items")]
        public List<ItemData> Items { get; private set; } = new List<ItemData>();

        [JsonProperty("ammos")]
        public List<ItemData> Ammos { get; private set; } = new List<ItemData>();

        [JsonProperty("armor")]
        public List<ArmorData> Armors { get; private set; } = new List<ArmorData>();

        [JsonProperty("accessory")]
        public List<ItemData> Accessorys { get; private set; } = new List<ItemData>();

        [JsonProperty("vanity_armor")]
        public List<ArmorData> VanityArmors { get; private set; } = new List<ArmorData>();

        [JsonProperty("vanity_accessory")]
        public List<ItemData> VanityAccessorys { get; private set; } = new List<ItemData>();

        [JsonProperty("buffs")]
        public List<BuffData> Buffs { get; private set; } = new List<BuffData>();

        [JsonProperty("armor_dye")]
        public List<ArmorData> ArmorDyes { get; private set; } = new List<ArmorData>();

        [JsonProperty("accessory_dye")]
        public List<ItemData> AccessoryDyes { get; private set; } = new List<ItemData>();

        [JsonProperty("misc_items")]
        public List<MiscData> MiscItems { get; private set; } = new List<MiscData>();

        [JsonProperty("misc_dye")]
        public List<MiscData> MiscDyes { get; private set; } = new List<MiscData>();

        [JsonProperty("max_hp")]
        [DefaultValue(100)]
        public int HP { get; private set; }

        [JsonProperty("max_mp")]
        [DefaultValue(20)]
        public int MP { get; private set; }

        [JsonProperty("extra_accessory_slot")]
        [DefaultValue(true)]
        public bool ExtraAccessory { get; private set; } = true;

        public void Parse()
        {
            if (!needParse)
            {
                return;
            }

            ParseTeam();

            foreach (var item in Items)
            {
                item.Parse();
            }

            foreach (var ammo in Ammos)
            {
                ammo.Parse();
            }

            foreach (var armor in Armors)
            {
                armor.Parse();
            }

            foreach (var accessory in Accessorys)
            {
                accessory.Parse();
            }

            foreach (var vanityArmor in VanityArmors)
            {
                vanityArmor.Parse();
            }

            foreach (var vanityAccessory in VanityAccessorys)
            {
                vanityAccessory.Parse();
            }

            foreach (var buff in Buffs)
            {
                buff.Parse();
            }

            foreach (var armorDye in ArmorDyes)
            {
                armorDye.Parse();
            }

            foreach (var accessoryDye in AccessoryDyes)
            {
                accessoryDye.Parse();
            }

            foreach (var miscItem in MiscItems)
            {
                miscItem.Parse();
            }

            foreach (var miscDye in MiscDyes)
            {
                miscDye.Parse();
            }

            needParse = false;

            void ParseTeam()
            {
                var list = new List<int>();
                var teams = team.Split(',');

                foreach (string team in teams)
                {
                    int id;
                    switch (team.Trim().ToUpperInvariant())
                    {
                        case "ALL":
                            TeamIDs = new List<int>() { 0, 1, 2, 3, 4, 5 };
                            needParse = false;
                            return;

                        case "WHITE":
                            id = 0;
                            break;

                        case "RED":
                            id = 1;
                            break;

                        case "GREEN":
                            id = 2;
                            break;

                        case "BLUE":
                            id = 3;
                            break;

                        case "YELLOW":
                            id = 4;
                            break;

                        case "PINK":
                            id = 5;
                            break;

                        default:
                            id = -1;
                            break;
                    }

                    list.Add(id);
                }

                TeamIDs = list;
            }
        }
    }
}
