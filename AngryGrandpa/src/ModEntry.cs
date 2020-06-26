using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using Harmony;
using Netcode;

namespace AngryGrandpa
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        internal static ModEntry Instance { get; private set; }
        internal HarmonyInstance Harmony { get; private set; }
        internal protected static ModConfig Config => ModConfig.Instance;


        /*********
        ** Accessors 
        *********/
        /// <summary>Provides methods for interacting with the mod directory.</summary>
        public static IModHelper ModHelper { get; private set; }


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Make resources available.
            Instance = this;
            ModConfig.Load();

            // Apply Harmony patches.
            Harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            UtilityPatches.Apply();
            EventPatches.Apply();
            FarmPatches.Apply();

            // Add console commands.
            addConsoleCommands();
            //Helper.ConsoleCommands.Add("command_string", "Description of command function.", cmdFunctionName);
            //reset_evaluation? print_config?

            // Listen for game events. (Make these into an event handler thing?)
            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.Input.ButtonPressed += this.onButtonPressed;

            // Set up asset loaders/editors.
            //Helper.Content.AssetEditors.Add(new EventsEditor());

        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Set up Generic Mod Config Menu if available.
            ModConfig.SetUpMenu();
        }

        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button != SButton.O)
                return;

            ModConfig.Print(); // Print config values to console when "O" key is pressed.
        }

        private void addConsoleCommands()
        {
            Helper.ConsoleCommands.Add("grandpa_score",
                "Estimates the result of a farm evaluation using grandpa's scoring criteria.\n\nUsage: grandpa_score",
                cmdGrandpaScore);
            Helper.ConsoleCommands.Add("reset_evaluation",
                "Removes all event flags related to grandpa's evaluation(s).\n\nUsage: reset_evaluation",
                cmdResetEvaluation);
        }

        /// <summary>Gives a farm evaluation in console output when the 'grandpa_score' command is invoked.</summary>
        /// <param name="command">The name of the command invoked.</param>
        /// <param name="args">The arguments received by the command. Each word after the command name is a separate argument.</param>
        private void cmdGrandpaScore(string _command, string[] args)
        {
            try
            {
                if (!Context.IsWorldReady)
                {
                    throw new Exception("Cannot evaluate score without an active save.");
                }
                int grandpaScore = Utility.getGrandpaScore();
                int maxScore = Config.GetMaxScore();
                int candles = Utility.getGrandpaCandlesFromScore(grandpaScore);
                Monitor.Log($"Grandpa's Score: {grandpaScore} of {maxScore} Great Honors\nNumber of candles earned: {candles}\nScoring system: \"{Config.ScoringSystem}\"\nCandle score thresholds: [{Config.GetScoreForCandles(1)}, {Config.GetScoreForCandles(2)}, {Config.GetScoreForCandles(3)}, {Config.GetScoreForCandles(4)}]",
                    LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"grandpa_score failed:\n{ex}",
                    LogLevel.Error);
            }
        }

        /// <summary>Resets all event flags related to grandpa's evaluation(s) when the 'reset_evaluation' command is invoked.</summary>
        /// <param name="command">The name of the command invoked.</param>
        /// <param name="args">The arguments received by the command. Each word after the command name is a separate argument.</param>
        private void cmdResetEvaluation(string _command, string[] args)
        {
            try
            {
                if (!Context.IsWorldReady)
                {
                    throw new Exception("Cannot remove event flags without an active save.");
                }
                Game1.player.eventsSeen.Remove(558291); // Initial evaluation
                Game1.player.eventsSeen.Remove(558292); // Re-evaluation
                Game1.player.eventsSeen.Remove(2146991); // Candle lighting
                Game1.player.eventsSeen.Remove(321777); // Evaluation request
                Game1.getFarm().hasSeenGrandpaNote = false; // Seen the note on the shrine
                Game1.player.mailReceived.Remove("grandpaPerfect"); // Received the statue of perfection
                //Remove the candles and candle sprites???
                //Reset grandpaScore?

                Monitor.Log($"Reset event flags successfully.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"reset_evaluation failed:\n{ex}", LogLevel.Error);
            }
        }
    }
}