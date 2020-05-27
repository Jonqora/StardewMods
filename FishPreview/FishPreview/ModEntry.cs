using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace FishPreview
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private ModConfig Config;

        private bool isCatching = false;
        private int whichFish;
        private Object fishSprite;
        private bool caughtSpecies;
        private bool showFish;
        private bool showText;
        private string textValue;
        private IList<string> displayOrder;
        private Vector2 textSize;
        private int margin = 18;
        private float scale = 0.7f;
        private int boxwidth;
        private int boxheight;
        private int x;
        private int y;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Display.RenderedActiveMenu += this.OnRenderMenu;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            // this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
        }

        private void getDisplayOrder(){

        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // read the Config for display position and get list priority for displayOrder
            switch (Config.FishDisplayPosition)
            {
                case "Top":
                    displayOrder = new List<string>() { "Top", "UpperRight", "UpperLeft", "LowerRight" };
                    break;
                case "UpperRight":
                    displayOrder = new List<string>() { "UpperRight", "UpperLeft", "LowerRight" };
                    break;
                case "UpperLeft":
                    displayOrder = new List<string>() { "UpperLeft", "UpperRight", "LowerLeft" };
                    break;
                case "Bottom":
                    displayOrder = new List<string>() { "Bottom", "LowerRight", "LowerLeft", "UpperRight" };
                    break;
                case "LowerRight":
                    displayOrder = new List<string>() { "LowerRight", "LowerLeft", "UpperRight" };
                    break;
                case "LowerLeft":
                    displayOrder = new List<string>() { "LowerLeft", "LowerRight", "UpperLeft" };
                    break;
                default:
                    displayOrder = new List<string>() { "UpperRight", "UpperLeft", "LowerLeft" };
                    this.Monitor.Log($"Invalid config value {Config.FishDisplayPosition} for FishDisplayPosition. Valid entries include Top, Bottom, UpperRight, UpperLeft, LowerRight and LowerLeft.", LogLevel.Warn);
                    break;
            }
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (Game1.player == null || !Game1.player.IsLocalPlayer)
            {
                return;
            }

            if (e.NewMenu is BobberBar bar)
            {
                isCatching = true;
                this.Monitor.Log($"{Game1.player.Name} has started fishing.", LogLevel.Debug);

                // figure out which fish is being caught
                whichFish = Helper.Reflection.GetField<int>(bar, "whichFish").GetValue();
                this.Monitor.Log($"Currently catching: {whichFish}", LogLevel.Debug);

                // save fish object to use in drawing // check for errors?
                fishSprite = new Object(whichFish, 1);

                // determine if species has been caught before
                caughtSpecies = Game1.player.fishCaught.ContainsKey(whichFish) && Game1.player.fishCaught[whichFish][0] > 0;
                //this.Monitor.Log($"Game1.player.fishCaught[{whichFish}] = {Game1.player.fishCaught[whichFish]}", LogLevel.Debug);
                //this.Monitor.Log($"Game1.player.fishCaught = {Game1.player.fishCaught}", LogLevel.Debug);

                // determine value of showFish value
                showFish = Config.ShowUncaughtFishSpecies || caughtSpecies;

                // determine value of showText value
                showText = Config.ShowFishName;

                // determine text to show if true
                if (showText && showFish)
                {
                    textValue = fishSprite.DisplayName;  // TODO obtain translation text
                    this.Monitor.Log($"Fish name: {textValue}", LogLevel.Debug);
                }
                else
                {
                    textValue = "???";
                }
                

                // determine width and height of display box
                boxwidth = 150; boxheight = 100;

                textSize = Game1.dialogueFont.MeasureString(textValue) * scale;

                if (showText && showFish) { boxwidth = Math.Max(150, (2 * margin) + (int)textSize.X); }
                if (showText) { boxheight += (int)textSize.Y; }


                // determine x and y positions
                foreach (string position in displayOrder)
                {
                    // set the correct display coordinates from position values in order of priority
                    switch (position)
                    {
                        case "Top":
                            x = bar.xPositionOnScreen + (bar.width / 2) - (boxwidth / 2) + 32;
                            y = bar.yPositionOnScreen - boxheight - 16;
                            break;
                        case "UpperRight":
                            x = bar.xPositionOnScreen + bar.width + 64;
                            y = bar.yPositionOnScreen;
                            break;
                        case "UpperLeft":
                            x = bar.xPositionOnScreen - boxwidth - 16;
                            y = bar.yPositionOnScreen;
                            break;
                        case "Bottom":
                            x = bar.xPositionOnScreen + (bar.width / 2) - (boxwidth / 2) + 32;
                            y = bar.yPositionOnScreen + bar.height - 16;
                            break;
                        case "LowerRight":
                            x = bar.xPositionOnScreen + bar.width + 64;
                            y = bar.yPositionOnScreen + bar.height - boxheight - 32;
                            break;
                        case "LowerLeft":
                            x = bar.xPositionOnScreen - boxwidth - 16;
                            y = bar.yPositionOnScreen + bar.height - boxheight - 32;
                            break;
                        default:
                            // default to UpperRight position
                            x = bar.xPositionOnScreen + bar.width + 64;
                            y = bar.yPositionOnScreen;
                            this.Monitor.Log($"Invalid position {position} listed in displayOrder.", LogLevel.Debug);
                            break;
                    }

                    // if the box display is in bounds, break the loop. Otherwise proceed to alternative display position(s).
                    if (x >= 0 && y >= 0 && x + boxwidth <= Game1.viewport.Width && y + boxheight <= Game1.viewport.Height)
                    {
                        break;
                    }
                }

            }
            else
            {
                isCatching = false;
            }

            if (e.OldMenu is BobberBar)
            {
                this.Monitor.Log($"{Game1.player.Name} is done fishing.", LogLevel.Debug);
            }
        }

        private void OnRenderMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (Game1.player == null || !Game1.player.IsLocalPlayer)
            {
                return;
            }

            if (Game1.activeClickableMenu is BobberBar bar && isCatching == true)
            {
                // draw box of height and width at location
                IClickableMenu.drawTextureBox(Game1.spriteBatch, x, y, boxwidth, boxheight, Color.White);

                // if showFish, center the fish x
                if (showFish)
                {
                    fishSprite.drawInMenu(Game1.spriteBatch, new Vector2(x + (boxwidth / 2) - 32, y + 18), 1.0f, 1.0f, 1.0f, StackDrawType.Hide);
                    
                    // if showFish and showText, center the text x below the fish
                    if (showText)
                    {
                        Game1.spriteBatch.DrawString(Game1.dialogueFont, textValue, new Vector2(x + (boxwidth / 2) - ((int)textSize.X / 2), y + 82), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }
                }
                // else (if not showFish), center the text x&y
                else
                {
                    Game1.spriteBatch.DrawString(Game1.dialogueFont, textValue, new Vector2(x + (boxwidth / 2) - ((int)textSize.X / 2), y + (boxheight / 2) - ((int)textSize.Y / 2)), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
            }
        }
    }
}