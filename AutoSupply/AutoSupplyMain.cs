using AutoSupply.Commands;
using AutoSupply.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace AutoSupply
{
    [ApiVersion(2, 1)]
    public class AutoSupplyMain : TerrariaPlugin
    {
        public override string Author => "Miyabi";
        public override string Description => "Auto supply to anonymous user.";
        public override string Name => "AutoSupply";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        private static readonly string PVP_CONFIG_PATH = Path.GetFullPath(Path.Combine(TShock.SavePath, "SupplyConfig.json"));

        public AutoSupplyMain(Main game)
            : base(game)
        {
            Settings = SupplySettings.Read(PVP_CONFIG_PATH);
            if (Settings == null)
            {
                Console.WriteLine("Config is null.");
                Settings = new SupplySettings(new List<SupplySet>(), new List<MapData>());
            }
            else
            {
                Console.WriteLine(string.Format("Supply config at {0} loaded.", PVP_CONFIG_PATH));
            }

            Instance = this;
        }

        public static AutoSupplyMain Instance { get; private set; }

        public SupplySettings Settings { get; private set; }

        private MapData CurrentMap { get; set; }

        public Guid[] PlayerLastSupplyedId { get; } = new Guid[256];

        private List<string> AlreadyDefaultSuppliedNames { get; } = new List<string>();

        public static void WriteLog(string message)
        {
            TShock.Log.Info(message);
        }

        public override void Initialize()
        {
            GeneralHooks.ReloadEvent += OnReload;
            OTAPI.Hooks.World.IO.PostLoadWorld += SetCurrentMap;

            ServerApi.Hooks.GamePostUpdate.Register(this, OnUpdate);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            GetDataHandlers.PlayerSpawn += OnSpawn;

            TShockAPI.Commands.ChatCommands.Add(new Command(Permissions.godmode, GMCodeCommand.GetGMCode, "gmcode"));
            TShockAPI.Commands.ChatCommands.Add(new Command(Permissions.canchat, SupplyCommand.SupplyChangeCommand, Settings.SupplyCommand));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= OnReload;
                OTAPI.Hooks.World.IO.PostLoadWorld -= SetCurrentMap;

                ServerApi.Hooks.GamePostUpdate.Deregister(this, OnUpdate);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                GetDataHandlers.PlayerSpawn -= OnSpawn;
            }

            base.Dispose(disposing);
        }

        public bool IsInBanRange(float playerX, float playerY)
        {
            if (CurrentMap == null)
            {
                return false;
            }

            playerX /= 16f;
            playerY /= 16f;

            foreach (var area in CurrentMap.WhiteList)
            {
                if (area.ContainsPoint(playerX, playerY))
                {
                    return false;
                }
            }

            foreach (var area in CurrentMap.BlackList)
            {
                if (area.ContainsPoint(playerX, playerY))
                {
                    return true;
                }
            }

            return false;
        }

        public bool SetBuffs(SupplySet set, int playerId)
        {
            if (playerId < 0)
            {
                return false;
            }

            var player = TShock.Players[playerId].TPlayer;
            
            if (player == null)
            {
                return false;
            }
            
            int length = Player.maxBuffs;
            for (int i = 0; i < length; i++)
            {
                player.buffTime[i] = 0;
                player.buffType[i] = 0;
            }

            NetMessage.SendData((int)PacketTypes.PlayerBuff, -1, -1, null, playerId);

            foreach (var buff in set.Buffs)
            {
                buff.Parse();
                TShock.Players[playerId].SetBuff(buff.ID, Settings.BuffTime);
            }

            return true;
        }

        private void OnSpawn(object sender, GetDataHandlers.SpawnEventArgs args)
        {
            int playerIndex = args.PlayerId;
            if (PlayerLastSupplyedId[playerIndex] != default)
            {
                SetBuffs(Settings.SupplySets.First(x => x.ID == PlayerLastSupplyedId[playerIndex]), playerIndex);
            }

            if (Settings.HealOnRespawn)
            {
                TShock.Players[playerIndex].Heal(TShock.Players[playerIndex].TPlayer.statLifeMax);
            }
        }

        private void OnUpdate(EventArgs args)
        {
            foreach (var player in TShock.Players)
            {
                if (player == null
                    || !player.Active
                    || player.State != 10)
                {
                    continue;
                }

                if (PlayerLastSupplyedId[player.TPlayer.whoAmI] == default
                    && !player.HasPermission(Permissions.godmode)
                    && !AlreadyDefaultSuppliedNames.Contains(player.Name))
                {
                    SupplyCommand.Supply(player, Settings.DefaultSet);
                    AlreadyDefaultSuppliedNames.Add(player.Name);
                }
            }
        }

        private void OnLeave(LeaveEventArgs args)
        {
            PlayerLastSupplyedId[args.Who] = default;
        }

        private void OnReload(ReloadEventArgs args)
        {
            Settings = SupplySettings.Read(PVP_CONFIG_PATH);
            if (Settings == null)
            {
                Console.WriteLine("Config is null.");
                Settings = new SupplySettings(new List<SupplySet>(), new List<MapData>());
            }
        }

        private void SetCurrentMap(bool loadFromCloud)
        {
            CurrentMap = null;
            if (Settings == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(Main.worldName))
            {
                CurrentMap = Settings.DefaultMap;
                return;
            }

            foreach (var map in Settings.Maps)
            {
                if (Main.worldName.Contains(map.Name))
                {
                    CurrentMap = map;
                    break;
                }
            }

            if (CurrentMap == null)
            {
                CurrentMap = Settings.DefaultMap;
            }
        }
    }
}
