using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Harmony;
using Netcode;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;

namespace AngryGrandpa
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        internal static ModEntry Instance { get; private set; }
        internal HarmonyInstance Harmony { get; private set; }
        internal protected static ModConfig Config => ModConfig.Instance;

        /// <summary>Whether the next tick is the first one.</summary>
        private bool IsFirstTick = true;

        private int CurrentYear; // Monitor for changes


        /*********
        ** Accessors 
        *********/
        /// <summary>Provides methods for interacting with the mod directory.</summary>
        //public static IModHelper ModHelper { get; private set; }


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
            EventPatches.Apply();
            FarmPatches.Apply();
            ObjectPatches.Apply();
            UtilityPatches.Apply();

            // Add console commands.
            addConsoleCommands();

            // Listen for game events. (Make these into an event handler thing?)
            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.GameLoop.UpdateTicked += this.onUpdateTicked;
            helper.Events.Input.ButtonPressed += this.onButtonPressed;
            helper.Events.GameLoop.SaveLoaded += this.onSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.onDayStarted;

            // Set up portrait asset editor. This one is added early since it never changes.
            helper.Content.AssetEditors.Add(new PortraitEditor());
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

        private void onUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (this.IsFirstTick)
            {
                this.IsFirstTick = false;
            }
            else // Is second update tick
            {
                Instance.Helper.Events.GameLoop.UpdateTicked -= this.onUpdateTicked; // Don't check again

                // Set up asset loaders/editors.
                Instance.Helper.Content.AssetEditors.Add(new GrandpaNoteEditor());
                Instance.Helper.Content.AssetEditors.Add(new EventEditor());
                Instance.Helper.Content.AssetEditors.Add(new EvaluationEditor());
            }
        }

        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button != SButton.O)
                return;

            ModConfig.Print(); // Print config values to console when "O" key is pressed.
        }

        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Game1.getFarm().hasSeenGrandpaNote
                && Game1.player.mailReceived[0] != "6324grandpaNoteMail" ) // Missing (or misplaced) AG mail flag?
            {
                Game1.player.mailReceived.Remove("6324grandpaNoteMail");
                Game1.player.mailReceived.Insert(0, "6324grandpaNoteMail"); // Insert grandpa note first on mail tab
            }
            CurrentYear = Game1.year; // Track year for updating cached Events
        }

        private void onDayStarted(object sender, DayStartedEventArgs e)
        {
            resetEventsCacheIfYearChanged();
        }

        private void onWarped(object sender, WarpedEventArgs e)
        {
            resetEventsCacheIfYearChanged();
        }

        private void resetEventsCacheIfYearChanged()
        {
            if (Game1.year != CurrentYear) // Invalidate cache to reload assets that contain references to years passed
            {
                CurrentYear = Game1.year; // Update tracked value
                Helper.Content.InvalidateCache(asset // Trigger changed assets to reload on next use.
                => asset.AssetNameEquals("Data\\Events\\Farmhouse")
                || asset.AssetNameEquals("Data\\Events\\Farm"));
            }
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
                int farmScore = Game1.getFarm().grandpaScore.Value;
                Monitor.Log($"DEBUG", LogLevel.Debug);
                Monitor.Log($"Actual current Farm.grandpaScore value: {farmScore}", LogLevel.Debug);
                bool farmNote = Game1.getFarm().hasSeenGrandpaNote;
                Monitor.Log($"Actual current Farm.hasSeenGrandpaNote value: {farmNote}", LogLevel.Debug); 
                List<int> eventsAG = new List<int> { 558291, 558292, 2146991, 321777 };
                List<string> mailAG = new List<string> { "6324grandpaNoteMail", "6324reward1candle", "6324reward2candle", "6324reward3candle", "6324reward4candle", "6324bonusRewardsEnabled", "6324hasDoneModdedEvaluation" };
                Monitor.Log($"Actual eventsSeen entries: {string.Join(", ", eventsAG.Where(Game1.player.eventsSeen.Contains).ToList())}", LogLevel.Debug);
                Monitor.Log($"Actual mailReceived entries: {string.Join(", ", mailAG.Where(Game1.player.mailReceived.Contains).ToList())}", LogLevel.Debug);
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
                // Game1.player.eventsSeen.Remove(2146991); // Candle lighting (this is now removed by command_grandpaEvaluation postfix)
                Game1.player.eventsSeen.Remove(321777); // Evaluation request
                Game1.getFarm().hasSeenGrandpaNote = false; // Seen the note on the shrine
                Game1.player.mailReceived.Remove("grandpaPerfect"); // Received the statue of perfection
                Game1.getFarm().grandpaScore.Value = 0; // Reset grandpaScore
                FarmPatches.RemoveCandlesticks(Game1.getFarm()); // Removes all candlesticks (not flames).
                Game1.getFarm().removeTemporarySpritesWithIDLocal(6666f); // Removes candle flames.
                
                // Remove flags added by this mod
                Game1.player.mailReceived.Remove("6324grandpaNoteMail"); // Mail entry
                Game1.player.mailReceived.Remove("6324reward1candle");
                Game1.player.mailReceived.Remove("6324reward2candles");
                Game1.player.mailReceived.Remove("6324reward3candles");

                if (!Game1.player.eventsSeen.Contains(2146991))
                {
                    Game1.player.eventsSeen.Add(2146991); // Make sure they can't see candle event before the next evaluation.
                }

                Monitor.Log($"Reset grandpaScore and associated event and mail flags.", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"reset_evaluation failed:\n{ex}", LogLevel.Error);
            }
        }
    }
}