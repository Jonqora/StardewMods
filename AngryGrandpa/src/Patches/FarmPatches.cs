using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using Object = StardewValley.Object;
using Netcode;
using System;
using xTile.Dimensions;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace AngryGrandpa
{
	class FarmPatches
	{
		private static IModHelper Helper => ModEntry.Instance.Helper;
		private static IMonitor Monitor => ModEntry.Instance.Monitor;
		private static ModConfig Config => ModConfig.Instance;

		private static HarmonyInstance Harmony => ModEntry.Instance.Harmony;

		public static void Apply()
		{
			Harmony.Patch(
				original: AccessTools.Method(typeof(Farm),
					nameof(Farm.checkAction)),
				prefix: new HarmonyMethod(typeof(FarmPatches),
					nameof(FarmPatches.farmCheckAction_Prefix))
			);
            Harmony.Patch(
                original: AccessTools.Method(typeof(Farm),
                    nameof(Farm.addGrandpaCandles)),
                prefix: new HarmonyMethod(typeof(FarmPatches),
                    nameof(FarmPatches.addGrandpaCandles_Prefix))
            );
        }

		public static bool farmCheckAction_Prefix(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, Farm __instance, ref bool __result)
		{
			try
			{
                Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
                switch (__instance.map.GetLayer("Buildings").Tiles[tileLocation] != null ? __instance.map.GetLayer("Buildings").Tiles[tileLocation].TileIndex : -1)
                {
                    case 1956:
                    case 1957:
                    case 1958: // Any of the three tiles that make up grandpa's shrine
                        // Clicking on grandpa's note for the first time?
                        if (!__instance.hasSeenGrandpaNote)
                        {
                            __instance.hasSeenGrandpaNote = true;
                            Game1.activeClickableMenu = (IClickableMenu)new LetterViewerMenu(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaNote", (object)Game1.player.Name).Replace('\n', '^'));
                            if ( !Game1.player.mailReceived.Contains("6324grandpaNoteMail") ) // Add mail to collections
                            {
                                Game1.player.mailReceived.Insert(0, "6324grandpaNoteMail");
                            }
                            __result = true;
                            return false; // Alter __result, don't run original code.
                        }
                        // Check for new bonus rewards. Players must do an evaluation with this mod to activate them.
                        if (Config.BonusRewards && Game1.player.mailReceived.Contains("6324hasDoneModdedEvaluation") )
                        {
                            // Give new 1-candle reward (Ancient seed artifact)
                            if ((int)(NetFieldBase<int, NetInt>)__instance.grandpaScore >= 1 && !Game1.player.mailReceived.Contains("6324reward1candle") )
                            {
                                who.addItemByMenuIfNecessaryElseHoldUp((Item)new Object(Vector2.Zero, 114, 1), new ItemGrabMenu.behaviorOnItemSelect(grandpa1CandleCallback));
                                __result = true;
                                return false; // Alter __result, don't run original code.
                            }
                            // Give new 2-candle reward (Dinosaur egg)
                            if ((int)(NetFieldBase<int, NetInt>)__instance.grandpaScore >= 2 && !Game1.player.mailReceived.Contains("6324reward2candle") )
                            {
                                who.addItemByMenuIfNecessaryElseHoldUp((Item)new Object(Vector2.Zero, 107, 1), new ItemGrabMenu.behaviorOnItemSelect(grandpa2CandleCallback));
                                __result = true;
                                return false; // Alter __result, don't run original code.
                            }
                            // Give new 3-candle reward (Prismatic shard)
                            if ((int)(NetFieldBase<int, NetInt>)__instance.grandpaScore >= 3 && !Game1.player.mailReceived.Contains("6324reward3candle") )
                            {
                                who.addItemByMenuIfNecessaryElseHoldUp((Item)new Object(Vector2.Zero, 74, 1), new ItemGrabMenu.behaviorOnItemSelect(grandpa3CandleCallback));
                                __result = true;
                                return false; // Alter __result, don't run original code.
                            } 
                        }
                        // Give 4-candle reward (Statue of Perfection)
                        if ( Config.StatuesForFarmhands && Game1.player.mailReceived.Contains("6324hasDoneModdedEvaluation") )
                        { // ANGRY GRANDPA LOGIC
                            if ((int)(NetFieldBase<int, NetInt>)__instance.grandpaScore >= 4 && !Game1.player.mailReceived.Contains("6324reward4candle"))
                            {
                                who.addItemByMenuIfNecessaryElseHoldUp((Item)new Object(Vector2.Zero, 160, false), new ItemGrabMenu.behaviorOnItemSelect(grandpa4CandleCallback));
                                __result = true;
                                return false; // Alter __result, don't run original code.
                            }
                        }
                        else // VANILLA LOGIC
                        {
                            if ((int)(NetFieldBase<int, NetInt>)__instance.grandpaScore >= 4 && !Utility.doesItemWithThisIndexExistAnywhere(160, true))
                            {
                                who.addItemByMenuIfNecessaryElseHoldUp((Item)new Object(Vector2.Zero, 160, false), new ItemGrabMenu.behaviorOnItemSelect(__instance.grandpaStatueCallback));
                                __result = true;
                                return false; // Alter __result, don't run original code.
                            }
                        }
                        // Accept diamond or prompt for diamond
                        if (Game1.year > Config.YearsBeforeEvaluation && (int)(NetFieldBase<int, NetInt>)__instance.grandpaScore > 0) // && (int)(NetFieldBase<int, NetInt>)__instance.grandpaScore < 4) // Allow endless re-evaluations
                        {
                            if (who.ActiveObject != null && (int)(NetFieldBase<int, NetInt>)who.ActiveObject.parentSheetIndex == 72) // && (int)(NetFieldBase<int, NetInt>)__instance.grandpaScore < 4) // Allow endless re-evaluations
                            {
                                who.reduceActiveItemByOne();
                                __instance.playSound("stoneStep", NetAudio.SoundContext.Default);
                                __instance.playSound("fireball", NetAudio.SoundContext.Default);
                                DelayedAction.playSoundAfterDelay("yoba", 800, (GameLocation)__instance, -1);
                                DelayedAction.showDialogueAfterDelay(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_PlaceDiamond"), 1200);
                                // Game1.multiplayer.broadcastGrandpaReevaluation(); // Re-implemented below
                                IReflectedField<Multiplayer> mp = Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");
                                Helper.Reflection.GetMethod(mp.GetValue(), "broadcastGrandpaReevaluation").Invoke();
                                Game1.player.freezePause = 1200;
                                __result = true;
                                return false; // Alter __result, don't run original code.
                            }
                            if (who.ActiveObject == null || (int)(NetFieldBase<int, NetInt>)who.ActiveObject.parentSheetIndex != 72)
                            {
                                Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_DiamondSlot"));
                                __result = true;
                                return false; // Alter __result, don't run original code.
                            }
                            break;
                        }
                        // Uh-oh, if somehow an evaluation is needed, request one for free.
                        if ((int)(NetFieldBase<int, NetInt>)__instance.grandpaScore == 0 && Game1.year > Config.YearsBeforeEvaluation)
                        {
                            Game1.player.eventsSeen.Remove(558292); // Remove re-evaluation event
                            if (Game1.player.eventsSeen.Contains(558291) // Has done first evaluation? (If not, it can still trigger)
                                && !Game1.player.eventsSeen.Contains(321777)) // Re-evaluation request
                            {
                                Game1.player.eventsSeen.Add(321777);
                                break;
                            }
                            break;
                        }
                        break;
                    default:
                        return true; // Run original code if not one of the shrine tiles
                }
                // return base.checkAction(tileLocation, viewport, who) || Game1.didPlayerJustRightClick(true) && __instance.CheckInspectAnimal(rect, who); // Re-implemented below.
                var baseMethod = typeof(BuildableGameLocation).GetMethod("checkAction");
                var ftn = baseMethod.MethodHandle.GetFunctionPointer();
                var baseCheckAction = (Func<Location, xTile.Dimensions.Rectangle, Farmer, bool>)Activator.CreateInstance(
                    typeof(Func<Location, xTile.Dimensions.Rectangle, Farmer, bool>), __instance, ftn);
                __result = baseCheckAction(tileLocation, viewport, who) || (Game1.didPlayerJustRightClick(true) && __instance.CheckInspectAnimal(rect, who));
                return false; // Alter __result, don't run original code.
            }
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(farmCheckAction_Prefix)}:\n{ex}",
					LogLevel.Error);
                return true; // Run original code
			}
		}

        private static void grandpa1CandleCallback(Item item, Farmer who)
        {
            //who = Game1.player;
            if (item == null
                || !(item is Object)
                || ((bool)(NetFieldBase<bool, NetBool>)(item as Object).bigCraftable || (int)(NetFieldBase<int, NetInt>)(item as Object).parentSheetIndex != 114)
                || who == null)
            {
                Monitor.Log("Callback conditions not satisfied.", LogLevel.Debug);
                return;
            }
            if (!who.mailReceived.Contains("6324reward1candle"))
            {
                who.mailReceived.Add("6324reward1candle");
            }
        }

        private static void grandpa2CandleCallback(Item item, Farmer who)
        {
            //who = Game1.player; 
            if (item == null
                || !(item is Object)
                || ((bool)(NetFieldBase<bool, NetBool>)(item as Object).bigCraftable || (int)(NetFieldBase<int, NetInt>)(item as Object).parentSheetIndex != 107)
                || who == null)
            {
                Monitor.Log("Callback conditions not satisfied.", LogLevel.Debug);
                return;
            }
            if (!who.mailReceived.Contains("6324reward2candle"))
            {
                who.mailReceived.Add("6324reward2candle");
            }
        }

        private static void grandpa3CandleCallback(Item item, Farmer who)
        {
            //who = Game1.player;
            if (item == null
                || !(item is Object)
                || ((bool)(NetFieldBase<bool, NetBool>)(item as Object).bigCraftable || (int)(NetFieldBase<int, NetInt>)(item as Object).parentSheetIndex != 74)
                || who == null)
            {
                Monitor.Log("Callback conditions not satisfied.", LogLevel.Debug);
                return;
            }
            if (!who.mailReceived.Contains("6324reward3candle"))
            {
                who.mailReceived.Add("6324reward3candle");
            }
        }

        private static void grandpa4CandleCallback(Item item, Farmer who)
        {
            who = Game1.player;
            if (item == null
                || !(item is Object)
                || !((bool)(NetFieldBase<bool, NetBool>)(item as Object).bigCraftable || (int)(NetFieldBase<int, NetInt>)(item as Object).parentSheetIndex != 160)
                || who == null)
                return;
            if (!who.mailReceived.Contains("6324reward4candle"))
            {
                who.mailReceived.Add("6324reward4candle");
            }
            if (!who.mailReceived.Contains("grandpaPerfect"))
            {
                who.mailReceived.Add("grandpaPerfect");
            }
        }

        public static void addGrandpaCandles_Prefix(Farm __instance)
        {
            try
            {
                if ((int)(NetFieldBase<int, NetInt>)__instance.grandpaScore <= 0)
                    return;
                // Remove all candlesticks
                RemoveCandlesticks(__instance);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(addGrandpaCandles_Prefix)}:\n{ex}",
                    LogLevel.Error);
            }
        }

        public static void RemoveCandlesticks(Farm farm)
        {
            Microsoft.Xna.Framework.Rectangle candlestickSourceRect = new Microsoft.Xna.Framework.Rectangle(577, 1985, 2, 5);
            //Microsoft.Xna.Framework.Rectangle candlestickPositions = new Microsoft.Xna.Framework.Rectangle(468, 344, 148 + 1, 60 + 1);
            for (int index = farm.temporarySprites.Count - 1; index >= 0; --index)
            {
                TemporaryAnimatedSprite sprite = farm.temporarySprites[index];
                if (sprite.sourceRect == candlestickSourceRect
                    && Helper.Reflection.GetField<string>(sprite, "textureName").GetValue() == "LooseSprites\\Cursors")
                    //&& candlestickPositions.Contains((int)sprite.Position.X, (int)sprite.Position.Y))
                {
                    farm.temporarySprites.RemoveAt(index);
                }
            }
        }
    }
}