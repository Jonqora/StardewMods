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
                        if (!__instance.hasSeenGrandpaNote)
                        {
                            __instance.hasSeenGrandpaNote = true;
                            Game1.activeClickableMenu = (IClickableMenu)new LetterViewerMenu(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaNote", (object)Game1.player.Name).Replace('\n', '^'));
                            __result = true;
                            return false; // Alter __result, don't run original code.
                        }
                        if (Game1.year >= 3 && (int)(NetFieldBase<int, NetInt>)__instance.grandpaScore > 0 && (int)(NetFieldBase<int, NetInt>)__instance.grandpaScore < 4)
                        {
                            if (who.ActiveObject != null && (int)(NetFieldBase<int, NetInt>)who.ActiveObject.parentSheetIndex == 72 && (int)(NetFieldBase<int, NetInt>)__instance.grandpaScore < 4)
                            {
                                who.reduceActiveItemByOne();
                                __instance.playSound("stoneStep", NetAudio.SoundContext.Default);
                                __instance.playSound("fireball", NetAudio.SoundContext.Default);
                                DelayedAction.playSoundAfterDelay("yoba", 800, (GameLocation)__instance, -1);
                                DelayedAction.showDialogueAfterDelay(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_PlaceDiamond"), 1200);
                                // Game1.multiplayer.broadcastGrandpaReevaluation();
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
                        if ((int)(NetFieldBase<int, NetInt>)__instance.grandpaScore >= 4 && !Utility.doesItemWithThisIndexExistAnywhere(160, true))
                        {
                            who.addItemByMenuIfNecessaryElseHoldUp((Item)new Object(Vector2.Zero, 160, false), new ItemGrabMenu.behaviorOnItemSelect(__instance.grandpaStatueCallback));
                            __result = true;
                            return false; // Alter __result, don't run original code.
                        }
                        if ((int)(NetFieldBase<int, NetInt>)__instance.grandpaScore == 0 && Game1.year >= 3)
                        {
                            Game1.player.eventsSeen.Remove(558292);
                            if (!Game1.player.eventsSeen.Contains(321777))
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
                // return base.checkAction(tileLocation, viewport, who) || Game1.didPlayerJustRightClick(true) && __instance.CheckInspectAnimal(rect, who);
                var baseMethod = typeof(BuildableGameLocation).GetMethod("checkAction");
                //var baseMethod = Helper.Reflection.GetMethod(typeof(BuildableGameLocation), "checkAction").MethodInfo; // PICK ONE.
                var ftn = baseMethod.MethodHandle.GetFunctionPointer();
                var baseCheckAction = (Func<Location, xTile.Dimensions.Rectangle, Farmer, bool>)Activator.CreateInstance(
                    typeof(Func<Location, xTile.Dimensions.Rectangle, Farmer, bool>), __instance, ftn);
                __result = baseCheckAction(tileLocation, viewport, who) || (Game1.didPlayerJustRightClick(true) && __instance.CheckInspectAnimal(rect, who));
                //__result = Helper.Reflection.GetMethod(typeof(BuildableGameLocation), "checkAction").Invoke<bool>(__instance, new object[] { tileLocation, viewport, who }) || (Game1.didPlayerJustRightClick(true) && __instance.CheckInspectAnimal(rect, who));
                return false; // Alter __result, don't run original code.
            }
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(farmCheckAction_Prefix)}:\n{ex}",
					LogLevel.Error);
                return true; // Run original code
			}
		}
	}
}