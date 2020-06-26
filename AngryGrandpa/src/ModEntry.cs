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

            // Add console commands.
            //Helper.ConsoleCommands.Add("command_string", "Description of command function.", cmdFunctionName);
            //reset_evaluation? print_config? evaluation_points?

            // Listen for game events. (Make these into an event handler thing?)
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            // Set up asset loaders/editors.
            //Helper.Content.AssetEditors.Add(new EventsEditor());

        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Set up Generic Mod Config Menu if available.
            ModConfig.SetUpMenu();
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button != SButton.O)
                return;

            ModConfig.Print(); // Print config values to console when "O" key is pressed.
        }
    }
}