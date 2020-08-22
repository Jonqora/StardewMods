using System;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace SunscreenMod
{
    /// <summary>Class containing the mod's console commands.</summary>
    public class ConsoleCommands
    {
        /*********
        ** Accessors
        *********/
        protected static IModHelper Helper => ModEntry.Instance.Helper;
        protected static IMonitor Monitor => ModEntry.Instance.Monitor;
        protected static ModConfig Config => ModConfig.Instance;


        /*********
        ** Fields
        *********/
        protected static ITranslationHelper i18n = Helper.Translation;


        /*********
        ** Public methods
        *********/
        /// <summary>
        /// Use the Mod Helper to register the commands in this class.
        /// </summary>
        public static void Apply()
        {
            string NL = Environment.NewLine;

            Helper.ConsoleCommands.Add("playSound",
                "Plays a soundbank sound for testing purposes." + NL + "Usage: playSound <soundName> [pitch]",
                cmdPlaySound);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Invokes the playSound console commands (first argument specifies the sound name).</summary>
        /// <param name="command">The name of the command invoked.</param>
        /// <param name="args">The arguments received by the command. Each word after the command name is a separate argument.</param>
        private static void cmdPlaySound(string _command, string[] _args)
        {
            try
            {
                if (_args.Length > 0)
                {
                    if (_args.Length == 1)
                    {
                        Game1.playSound(_args[0]);
                        Monitor.Log($"Success! Played sound <{_args[0]}>", LogLevel.Info);
                    }
                    else
                    {
                        int pitch;
                        if (Int32.TryParse(_args[1], out pitch))
                        {
                            Game1.playSoundPitched(_args[0], pitch);
                            Monitor.Log($"Success! Played sound <{_args[0]}> with pitch <{pitch}>", LogLevel.Info);
                        }
                        else
                        {
                            Monitor.Log($"ERROR: The optional second argument must be an integer (used for pitched sounds).\n" +
                                $"Usage: playSound <soundname> [pitch]", LogLevel.Info);
                        }
                    }
                }
                else
                {
                    Monitor.Log($"You must enter a sound name with this command. The integer [pitch] argument is optional.\n" +
                        $"Usage: playSound <soundname> [pitch]", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Command playSound failed:\n{ex}", LogLevel.Warn);
            }
        }
    }
}