using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.IO;
using static SunscreenMod.Flags;

namespace SunscreenMod
{
    public class ModEntry : Mod
    {
        /*********
        ** Fields
        *********/
        internal static ModEntry Instance { get; private set; }
        internal HarmonyInstance Harmony { get; private set; }
        internal JsonAssets.IApi JA { get; private set; }

        private Lotions Lotion;
        private SunscreenProtection Sunscreen;
        private Sunburn Burn;
        private Reactions Reacts;

        internal bool IsSaveReady = false;

        /*********
        ** Accessors
        *********/
        internal protected static ModConfig Config => ModConfig.Instance;


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
            //EventPatches.Apply();
            //FarmPatches.Apply();

            // Add console commands.
            ConsoleCommands.Apply();

            // Listen for game events.
            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += this.onReturnedToTitle;
            helper.Events.GameLoop.SaveLoaded += this.onSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.onDayStarted;
            helper.Events.GameLoop.DayEnding += this.onDayEnding;
            helper.Events.GameLoop.Saving += this.onSaving;
            helper.Events.GameLoop.OneSecondUpdateTicked += this.onOneSecondUpdateTicked;

            helper.Events.Player.Warped += this.onWarped;

            helper.Events.Input.ButtonPressed += this.onButtonPressed;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** GameLoop Event handlers
        ****/
        /// <summary>
        /// SUMMARY
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ModConfig.SetUpMenu();
            JA = Helper.ModRegistry.GetApi<JsonAssets.IApi>("spacechase0.JsonAssets");
            if (JA != null)
            {
                JA.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets", "JA"));
            }
            else
            {
                Monitor.LogOnce("Could not connect to Json Assets. It may not be installed or working properly.", LogLevel.Error);
            }

            Lotion = new Lotions();
        }

        /// <summary>
        /// SUMMARY
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            //clear any previous data from other saves? Initialize zeros and all?
            IsSaveReady = false;
            Reacts = null;
            Sunscreen = null;
            Burn = null;
        }

        /// <summary>
        /// SUMMARY
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            //clear any previous data from other saves
            //initialize new instances of Burn and Sunscreen
            Reacts = new Reactions();
            Sunscreen = new SunscreenProtection();
            Burn = new Sunburn();

            IsSaveReady = true;
        }

        /// <summary>
        /// SUMMARY
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onDayStarted(object sender, DayStartedEventArgs e)
        {
            if (HasFlag("NewDay")) //Ensure once a day only, including for farmhands
            {
                DoNewDayStuff();
                //read data from mail flags for burn level and new burn
            }
        }

        /// <summary>
        /// SUMMARY
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onDayEnding(object sender, DayEndingEventArgs e)
        {
            //decrease active sunburn level by 1 (ready for next day)
            //add new burn to current burn level, max 3 - discard new burn data??? (want to alert next day? on save loaded or game started?)
            //clear burn debuffs
            AddFlag("NewDay");
        }

        /// <summary>
        /// SUMMARY
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onSaving(object sender, SavingEventArgs e)
        {
            //write current burn level to flags (if not already)
            //write new burn level to flags (if not already)
        }

        /// <summary>
        /// SUMMARY HERE
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!IsSaveReady)
            {
                return; //Ignore this before a save is loaded.
            }
            // bool Game1.player.swimming!
            Reacts.NearbyNPCsReact();
        }


        /****
        ** Player Event handlers
        ****/
        /// <summary>
        /// SUMMARY
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onWarped(object sender, WarpedEventArgs e)
        {
            if (!IsSaveReady)
            {
                return; //Ignore this before a save is loaded.
            }
            //clear the list of NPCs who have reacted (or initialize a new one??)
            Reacts.ClearReacts();
        }


        /****
        ** Input Event handlers
        ****/
        /// <summary>
        /// SUMMARY
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !IsSaveReady)
            {
                return; //Ignore button input before a save is loaded.
            }

            if (Game1.didPlayerJustRightClick() || //Did we use the right button to apply a lotion?
                (Constants.TargetPlatform == GamePlatform.Android && e.Button == SButton.MouseLeft)) //Android support
            {
                if (CanUseItem() && HoldingNonEdibleObject() && TappedOnFarmer(e.Cursor))
                {
                    Item itemToUse = (Item)Game1.player.ActiveObject;
                    if (Lotion.IsLotion(itemToUse))
                    {
                        Lotion.ApplyQuestion(itemToUse);
                        Helper.Input.Suppress(e.Button);
                    }
                }
            }
        }

        /****
        ** Helper Functions
        ****/
        //Stuff for onButtonPressed
        private bool CanUseItem()
        {
            return Game1.activeClickableMenu == null &&
                Game1.currentMinigame == null &&
                !Game1.eventUp && !Game1.dialogueUp && //Not in a menu or minigame or event or dialogue
                !Game1.player.isEating &&
                !Game1.player.canOnlyWalk && !Game1.player.FarmerSprite.PauseForSingleAnimation && !Game1.fadeToBlack; //Idk, vanilla code has em
        }
        private bool HoldingNonEdibleObject()
        {
            return Game1.player.ActiveObject != null && //Player is holding something, it is an Object
                Game1.player.ActiveObject.Edibility == -300; //It is not an edible object
        }
        private bool TappedOnFarmer(ICursorPosition cursor)
        {
            int x = (int)cursor.AbsolutePixels.X;
            int y = (int)cursor.AbsolutePixels.Y;
            if (Constants.TargetPlatform == GamePlatform.Android)
            {
                return new Rectangle((int)Game1.player.position.X, (int)Game1.player.position.Y - 85, 64, 125).Contains(x, y); //From android code
            }
            else
            {
                return Game1.player.GetBoundingBox().Contains(x, y);
            }
        }
        //Stuff for onDayStarted and onSaveLoaded
        private void DoNewDayStuff()
        {
            Monitor.Log("Doing new day stuff.", LogLevel.Info);
            RemoveFlag("NewDay");
        }
    }
}
