using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace AutoSupply.Commands
{
    internal class SwitchSupplyEnableCommand
    {
        internal static bool IsSupplyEnabled { get; private set; } = true;

        public static void SwitchSupply(CommandArgs args)
        {
            if (args.Parameters.Count != 1 || args.Parameters[0] == null || (args.Parameters[0] != "enable" && args.Parameters[0] != "disable"))
            {
                args.Player.SendErrorMessage(string.Format("Invalid parameters! Usage: {0}changesupplystatus <enable/disable>", new object[] { TShock.Config.Settings.CommandSpecifier }));
                return;
            }
           
            if (args.Parameters[0] == "enable")
            {
                IsSupplyEnabled = true;
                args.Player.SendSuccessMessage("Acquisition of supplies is now enabled");
            }
            else if (args.Parameters[0] == "disable")
            {
                IsSupplyEnabled = false;
                args.Player.SendSuccessMessage("Acquisition of supplies is now disabled");
            }
        }
    }
}
