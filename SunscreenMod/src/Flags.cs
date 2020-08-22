using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SunscreenMod
{
    class Flags
    {
        protected static IMonitor Monitor => ModEntry.Instance.Monitor;

        static string FlagBase { get; } = "6676SunscreenMod.";
        static string[] FlagList { get; } = {
            "NewDay",
            "SunburnLevel_1",
            "SunburnLevel_2",
            "SunburnLevel_3",
            "NewBurnDamageLevel_1",
            "NewBurnDamageLevel_2",
            "NewBurnDamageLevel_3"
        };

        public static bool IsFlag(string flagName)
        {
            return FlagList.Contains(flagName);
        }
        public static bool IsFullFlag(string fullFlagName)
        {
            return fullFlagName.StartsWith(FlagBase) && FlagList.Contains(fullFlagName.Skip(FlagBase.Length));
        }

        public static bool HasFlag(string flagName)
        {
            if (!IsFlag(flagName))
            {
                Monitor.Log($"ERROR: flag {flagName} does not exist in the Flags list.", LogLevel.Error);
                return false;
            }
            return Game1.player.mailReceived.Contains(FlagBase + flagName);
        }

        public static void AddFlag(string flagName)
        {
            if (!IsFlag(flagName))
            {
                Monitor.Log($"ERROR: flag {flagName} does not exist in the Flags list.", LogLevel.Error);
                return;
            }
            if (!Game1.player.mailReceived.Contains(FlagBase + flagName))
            {
                Game1.player.mailReceived.Add(FlagBase + flagName);
            }
        }

        public static void AddFlags(List<string> flagNames)
        {
            foreach (string flag in flagNames) { AddFlag(flag); }
        }

        public static void RemoveFlag(string flagName)
        {
            if (!IsFlag(flagName))
            {
                Monitor.Log($"ERROR: flag {flagName} does not exist in the Flags list.", LogLevel.Error);
                return;
            }
            while (Game1.player.mailReceived.Contains(FlagBase + flagName))
            {
                Game1.player.mailReceived.Remove(FlagBase + flagName);
            }
        }

        public static void RemoveFlags(List<string> flagNames)
        {
            foreach (string flag in flagNames) { RemoveFlag(flag); }
        }
    }
}
