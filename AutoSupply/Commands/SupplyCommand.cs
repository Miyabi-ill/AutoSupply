using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace AutoSupply.Commands
{
    public static class SupplyCommand
    {
        internal static void SupplyChangeCommand(CommandArgs args)
        {
            Player player = args.TPlayer;
            var settings = AutoSupplyMain.Instance.Settings;

            if (settings == null)
            {
                return;
            }

            var canChooseSet = new List<SupplySet>();
            foreach (var set in settings.SupplySets)
            {
                // 不要なはず
                set.Parse();

                // チームで選択可能かを判断
                if (set.TeamIDs.Contains(player.team))
                {
                    canChooseSet.Add(set);
                }
            }

            if (settings.GMSet != null
                && !canChooseSet.Contains(settings.GMSet)
                && settings.GMSet.TeamIDs.Contains(player.team))
            {
                canChooseSet.Add(settings.GMSet);
            }

            string supplySelectErrorMsg = string.Format("Invalid supply name! Usage: {0}{1} <supply_name>\nAvailable supply list: {2}",
                new object[] {
                    TShock.Config.Settings.CommandSpecifier,
                    settings.SupplyCommand,
                    string.Join(", ", canChooseSet.Select(x => x.Name.ToLower(CultureInfo.InvariantCulture)))
                                                  .Replace(SupplySettings.GMSET_NAME.ToLowerInvariant(), string.Format(CultureInfo.InvariantCulture, "GM-{0:D6}", GMCodeCommand.GMCODE)),
                });

            // BAN範囲か
            if (AutoSupplyMain.Instance.IsInBanRange(player.Center.X, player.Center.Y))
            {
                args.Player.SendErrorMessage("You can\'t get supply in the BAN area.");
                return;
            }

            // コマンドのフォーマットが正しいか
            if (args.Parameters.Count != 1 || string.IsNullOrEmpty(args.Parameters[0]))
            {
                args.Player.SendErrorMessage(supplySelectErrorMsg);
                return;
            }

            Guid supplyId = default;
            string setName = args.Parameters[0].ToUpperInvariant();
            SupplySet selectedSet = null;

            // GMセット以外で一致を取得
            foreach (var set in canChooseSet)
            {
                if (set.Name == SupplySettings.GMSET_NAME)
                {
                    continue;
                }

                if (set.Name == setName)
                {
                    selectedSet = set;
                    supplyId = set.ID;
                    AutoSupplyMain.Instance.PlayerLastSupplyedId[player.whoAmI] = supplyId;
                    break;
                }
            }

            // GMセットで一致を取得
            if (setName == string.Format(CultureInfo.InvariantCulture, "GM-{0:D6}", GMCodeCommand.GMCODE))
            {
                selectedSet = settings.GMSet;
                supplyId = selectedSet.ID;
                AutoSupplyMain.Instance.PlayerLastSupplyedId[player.whoAmI] = supplyId;
            }

            // 一致がなければエラー
            if (supplyId == default || selectedSet == null)
            {
                args.Player.SendErrorMessage(supplySelectErrorMsg);
                return;
            }

            Supply(args.Player, selectedSet);
        }

        public static void Supply(TSPlayer tsPlayer, SupplySet set)
        {
            if (tsPlayer == null
                || set == null)
            {
                return;
            }

            var player = tsPlayer.TPlayer;
            var oldSupplyId = AutoSupplyMain.Instance.PlayerLastSupplyedId[player.whoAmI];
            
            // ログ用
            string oldSupplySetName = AutoSupplyMain.Instance.Settings.SupplySets.FirstOrDefault(x => x.ID == oldSupplyId)?.Name ?? "None";

            AutoSupplyMain.WriteLog(string.Format(
                    CultureInfo.InvariantCulture,
                    "SUPPLY:{0},{1},{2},{3},{4}",
                    player.name,
                    oldSupplySetName,
                    set.Name,
                    player.position.X,
                    player.position.Y));

            AutoSupplyMain.Instance.PlayerLastSupplyedId[player.whoAmI] = set.ID;

            // Clear inventory
            for (int invIndex = 0; invIndex < NetItem.InventorySlots; invIndex++)
            {
                player.inventory[invIndex] = new Item();
            }

            for (int invIndex = 0; invIndex < NetItem.ArmorSlots; invIndex++)
            {
                player.armor[invIndex] = new Item();
            }

            for (int invIndex = 0; invIndex < NetItem.DyeSlots; invIndex++)
            {
                player.dye[invIndex] = new Item();
            }

            for (int invIndex = 0; invIndex < NetItem.MiscEquipSlots; invIndex++)
            {
                player.miscEquips[invIndex] = new Item();
            }

            for (int invIndex = 0; invIndex < NetItem.MiscDyeSlots; invIndex++)
            {
                player.miscDyes[invIndex] = new Item();
            }

            // Clear buffs
            int buffLength = player.buffType.Length;
            for (int buffIndex = 0; buffIndex < buffLength; buffIndex++)
            {
                player.DelBuff(buffIndex);
            }

            // Clear own projectiles
            int projLength = Main.projectile.Length;
            for (int projIndex = 0; projIndex < projLength; projIndex++)
            {
                if (Main.projectile[projIndex].owner == player.whoAmI)
                {
                    Main.projectile[projIndex].Kill();
                }
            }

            const int MAX_INVENTORY = 49;
            const int VANITY_START = 10;
            const int ACCESSORY_START = 3;

            // Inventory
            for (int j = 0; j < set.Items.Count; j++)
            {
                set.Items[j].Parse();
                player.inventory[j].SetDefaults(set.Items[j].ItemID);
                player.inventory[j].prefix = (byte)set.Items[j].Prefix;
                player.inventory[j].stack = set.Items[j].Stack;
            }

            // Ammos
            for (int j = 0; j < set.Ammos.Count; j++)
            {
                set.Ammos[j].Parse();
                player.inventory[MAX_INVENTORY - j].SetDefaults(set.Ammos[j].ItemID);
                player.inventory[MAX_INVENTORY - j].prefix = (byte)set.Ammos[j].Prefix;
                player.inventory[MAX_INVENTORY - j].stack = set.Ammos[j].Stack;
            }

            // Armors
            for (int j = 0; j < set.Armors.Count; j++)
            {
                set.Armors[j].Parse();
                int index = set.Armors[j].SlotID;
                player.armor[index].SetDefaults(set.Armors[j].ItemID);
                player.armor[index].prefix = (byte)set.Armors[j].Prefix;
                player.armor[index].stack = set.Armors[j].Stack;
            }

            // VanityArmors
            for (int j = 0; j < set.VanityArmors.Count; j++)
            {
                set.VanityArmors[j].Parse();
                int index = set.VanityArmors[j].SlotID;
                player.armor[VANITY_START + index].SetDefaults(set.VanityArmors[j].ItemID);
                player.armor[VANITY_START + index].prefix = (byte)set.VanityArmors[j].Prefix;
                player.armor[VANITY_START + index].stack = set.VanityArmors[j].Stack;
            }

            // Armor Dyes
            for (int j = 0; j < set.ArmorDyes.Count; j++)
            {
                set.ArmorDyes[j].Parse();
                player.dye[j].SetDefaults(set.ArmorDyes[j].ItemID);
                player.dye[j].prefix = (byte)set.ArmorDyes[j].Prefix;
                player.dye[j].stack = set.ArmorDyes[j].Stack;
            }

            // Accessories
            for (int j = 0; j < set.Accessorys.Count; j++)
            {
                set.Accessorys[j].Parse();
                player.armor[ACCESSORY_START + j].SetDefaults(set.Accessorys[j].ItemID);
                player.armor[ACCESSORY_START + j].prefix = (byte)set.Accessorys[j].Prefix;
                player.armor[ACCESSORY_START + j].stack = set.Accessorys[j].Stack;
            }

            // Vanity Accessories
            for (int j = 0; j < set.VanityAccessorys.Count; j++)
            {
                set.VanityAccessorys[j].Parse();
                player.armor[VANITY_START + ACCESSORY_START + j].SetDefaults(set.VanityAccessorys[j].ItemID);
                player.armor[VANITY_START + ACCESSORY_START + j].prefix = (byte)set.VanityAccessorys[j].Prefix;
                player.armor[VANITY_START + ACCESSORY_START + j].stack = set.VanityAccessorys[j].Stack;
            }

            // Accessory Dyes
            for (int j = 0; j < set.AccessoryDyes.Count; j++)
            {
                set.AccessoryDyes[j].Parse();
                player.dye[ACCESSORY_START + j].SetDefaults(set.AccessoryDyes[j].ItemID);
                player.dye[ACCESSORY_START + j].prefix = (byte)set.AccessoryDyes[j].Prefix;
                player.dye[ACCESSORY_START + j].stack = set.AccessoryDyes[j].Stack;
            }

            // Misc Items
            for (int j = 0; j < set.MiscItems.Count; j++)
            {
                set.MiscItems[j].Parse();
                int index = set.MiscItems[j].SlotID;
                player.miscEquips[index].SetDefaults(set.MiscItems[j].ItemID);
                player.miscEquips[index].prefix = (byte)set.MiscItems[j].Prefix;
                player.miscEquips[index].stack = set.MiscItems[j].Stack;
            }

            // Misc Dyes
            for (int j = 0; j < set.MiscDyes.Count; j++)
            {
                set.MiscDyes[j].Parse();
                int index = set.MiscDyes[j].SlotID;
                player.miscDyes[index].SetDefaults(set.MiscDyes[j].ItemID);
                player.miscDyes[index].prefix = (byte)set.MiscDyes[j].Prefix;
                player.miscDyes[index].stack = set.MiscDyes[j].Stack;
            }

            player.statLifeMax = set.HP;
            player.statManaMax = set.MP;

            if (player.statLife > player.statLifeMax)
            {
                player.statLife = player.statLifeMax;
            }

            if (player.statMana > player.statManaMax)
            {
                player.statMana = player.statManaMax;
            }

            // Send Charactor
            int playerIndex = tsPlayer.Index;
            bool isSSC = Main.ServerSideCharacter;

            if (!isSSC)
            {
                Main.ServerSideCharacter = true;
                NetMessage.SendData((int)PacketTypes.WorldInfo, playerIndex, -1, null, 0, 0f, 0f, 0f, 0, 0, 0);
                tsPlayer.IgnoreSSCPackets = true;
            }

            // Send Life
            NetMessage.SendData((int)PacketTypes.PlayerHp, -1, -1, null, playerIndex, 0f, 0f, 0f, 0, 0, 0);

            // Send Mana
            NetMessage.SendData((int)PacketTypes.PlayerMana, -1, -1, null, playerIndex, 0f, 0f, 0f, 0, 0, 0);

            // Send charactor info (include extra accessory slot)
            player.extraAccessory = set.ExtraAccessory;
            NetMessage.SendData((int)PacketTypes.PlayerInfo, -1, -1, null, playerIndex, 0f, 0f, 0f, 0, 0, 0);

            int masterInvIndex = 0;
            for (int invIndex = 0; invIndex < NetItem.InventorySlots; invIndex++)
            {
                NetMessage.SendData(5, -1, -1, null, playerIndex, masterInvIndex, player.inventory[invIndex].prefix, 0, 0, 0, 0);
                masterInvIndex++;
            }

            // Armors include accessories
            for (int invIndex = 0; invIndex < NetItem.ArmorSlots; invIndex++)
            {
                NetMessage.SendData(5, -1, -1, null, playerIndex, masterInvIndex, player.armor[invIndex].prefix, 0, 0, 0, 0);
                masterInvIndex++;
            }

            for (int invIndex = 0; invIndex < NetItem.DyeSlots; invIndex++)
            {
                NetMessage.SendData(5, -1, -1, null, playerIndex, masterInvIndex, player.dye[invIndex].prefix, 0, 0, 0, 0);
                masterInvIndex++;
            }

            // Hooks, Light Pet, etc...
            for (int invIndex = 0; invIndex < NetItem.MiscEquipSlots; invIndex++)
            {
                NetMessage.SendData(5, -1, -1, null, playerIndex, masterInvIndex, player.miscEquips[invIndex].prefix, 0, 0, 0, 0);
                masterInvIndex++;
            }

            for (int invIndex = 0; invIndex < NetItem.MiscDyeSlots; invIndex++)
            {
                NetMessage.SendData(5, -1, -1, null, playerIndex, masterInvIndex, player.miscDyes[invIndex].prefix, 0, 0, 0, 0);
                masterInvIndex++;
            }

            var trashItem = Main.player[playerIndex].trashItem;
            NetMessage.SendData(5, -1, -1, new NetworkText(trashItem.Name, NetworkText.Mode.Formattable), playerIndex, 179f, trashItem.prefix, 0.0f, 0, 0, 0);

            if (!isSSC)
            {
                Main.ServerSideCharacter = false;

                // Send world info
                NetMessage.SendData((int)PacketTypes.WorldInfo, playerIndex, -1, null, 0, 0.0f, 0.0f, 0.0f, 0, 0, 0);
                tsPlayer.IgnoreSSCPackets = false;
            }

            AutoSupplyMain.Instance.SetBuffs(set, playerIndex);
            return;
        }
    }
}
