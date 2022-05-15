using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace AutoSupply.IO
{
    public class ItemData
    {
        private bool need_parse = true;

        [JsonProperty("item_name")]
        public string ItemName { get; private set; } = "";

        [JsonProperty("prefix")]
        [DefaultValue("0")]
        public string PrefixName { get; private set; } = "0";

        [JsonProperty("stack")]
        [DefaultValue(1)]
        public int Stack { get; private set; } = 1;

        [JsonIgnore]
        public int ItemID { get; private set; }

        [JsonIgnore]
        public int Prefix { get; private set; }

        public int MaxItemCount { get; } = (short)typeof(Terraria.ID.ItemID).GetField("Count", BindingFlags.Public | BindingFlags.Static).GetValue(null);

        public void Parse()
        {
            if (!need_parse)
            {
                return;
            }

            //ItemIDから先に、全ての名前と衝突させる．数値ならそのまま使う．
            if (int.TryParse(ItemName, out int id))
            {
                ItemID = id;
            }
            else
            {
                var found_item = new List<Item>();
                if (string.IsNullOrEmpty(ItemName))
                {
                    ItemID = 0;
                }
                else
                {
                    Item item = new Item();

                    //大文字に正規化してから比較する．
                    Main.player[Main.myPlayer] = new Player();
                    for (int i = 1; i < MaxItemCount; i++)
                    {
                        item.netDefaults(i);
                        if (!string.IsNullOrWhiteSpace(item.Name))
                        {
                            if (item.Name.Equals(ItemName, StringComparison.OrdinalIgnoreCase))
                            {
                                found_item = new List<Item> { item };
                                break;
                            }
                            //アイテムが一意に定まるならいいようにしてるけどしない方がいいかも．
                            if (item.Name.ToUpperInvariant().StartsWith(ItemName, StringComparison.OrdinalIgnoreCase))
                            {
                                found_item.Add(item.Clone());
                            }
                        }
                    }
                    if (found_item.Count == 1)
                    {
                        ItemID = found_item[0].type;
                    }
                    else
                    {
                        throw new ArgumentException("Name not found:" + ItemName);
                    }
                }
            }

            //prefixも大体同じ．
            if (int.TryParse(PrefixName, out id))
            {
                Prefix = id;
            }
            else
            {
                const int FIRST_ITEM_PREFIX = 1;
                const int LAST_ITEM_PREFIX = 83;

                Item item = new Item();
                if (string.IsNullOrEmpty(PrefixName))
                {
                    Prefix = 0;
                }
                else
                {
                    item.SetDefaults(0);
                     var found_prefix = new List<int>();
                    for (int i = FIRST_ITEM_PREFIX; i <= LAST_ITEM_PREFIX; i++)
                    {
                        item.prefix = (byte)i;
                        string prefixName = item.AffixName().Trim();
                        if (prefixName.Equals(PrefixName, StringComparison.OrdinalIgnoreCase))
                        {
                            found_prefix = new List<int>() { i };
                            break;
                        }
                        else if (prefixName.StartsWith(PrefixName, StringComparison.OrdinalIgnoreCase))
                        {
                            found_prefix.Add(i);
                        }
                    }
                    if (found_prefix.Count == 1)
                    {
                        Prefix = found_prefix[0];
                    }
                }
            }
            need_parse = false;
        }
    }
}
