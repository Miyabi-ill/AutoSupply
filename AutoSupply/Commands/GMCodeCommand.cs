using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace AutoSupply.Commands
{
    public static class GMCodeCommand
    {
        internal static readonly int GMCODE = new Random().Next(0, 999999);

        internal static void GetGMCode(CommandArgs args)
        {
            args.Player.SendSuccessMessage(string.Format(CultureInfo.InvariantCulture, "{0:D6}", GMCODE));
        }
    }
}
