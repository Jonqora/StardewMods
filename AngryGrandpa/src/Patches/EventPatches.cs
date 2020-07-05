using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using Netcode;
using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace AngryGrandpa
{
	class EventPatches
	{
		private static IModHelper Helper => ModEntry.Instance.Helper;
		private static IMonitor Monitor => ModEntry.Instance.Monitor;
		private static ModConfig Config => ModConfig.Instance;
		private static HarmonyInstance Harmony => ModEntry.Instance.Harmony;

		protected static ITranslationHelper i18n = Helper.Translation;

		public static void Apply()
		{
			Harmony.Patch(
				original: AccessTools.Method(typeof(Event),
					nameof(Event.command_grandpaEvaluation)),
				prefix: new HarmonyMethod(typeof(EventPatches),
					nameof(EventPatches.grandpaEvaluations_Prefix)),
				postfix: new HarmonyMethod(typeof(EventPatches),
					nameof(EventPatches.grandpaEvaluations_Postfix))
			);
			Harmony.Patch(
				original: AccessTools.Method(typeof(Event),
					nameof(Event.command_grandpaEvaluation2)),
				prefix: new HarmonyMethod(typeof(EventPatches),
					nameof(EventPatches.grandpaEvaluations_Prefix)),
				postfix: new HarmonyMethod(typeof(EventPatches),
					nameof(EventPatches.grandpaEvaluations_Postfix))
			);
			Harmony.Patch(
				original: AccessTools.Method(typeof(Event),
					nameof(Event.skipEvent)),
				postfix: new HarmonyMethod(typeof(EventPatches),
					nameof(EventPatches.skipEvent_Postfix))
			);
		}

		public static void grandpaEvaluations_Prefix()
		{
			var game = Game1.game1;
			try
			{
				Helper.Content.InvalidateCache("Strings\\StringsFromCSFiles"); // Refresh cache before use
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(grandpaEvaluations_Prefix)}:\n{ex}",
					LogLevel.Error);
			}
		}
		
		public static void grandpaEvaluations_Postfix(GameLocation location)
		{
			try
			{
				Game1.player.eventsSeen.Remove(2146991); // Candles event removal needed for initial evaluation after a reset command
				Game1.player.eventsSeen.Remove(321777); // Remove re-evaluation request flag
				// Add a mail flag the FIRST time this mod is used for any evaluation. This activates bonus rewards.
				if (!Game1.player.mailReceived.Contains("6324hasDoneModdedEvaluation"))
				{
					Game1.player.mailReceived.Add("6324hasDoneModdedEvaluation");
				}

				if (Config.ShowPointsTotal)
				{
					int grandpaScore = Utility.getGrandpaScore();
					int maxScore = Config.GetMaxScore();
					string displayText = i18n.Get("Event.cs.ShowGrandpaScore", new { grandpaScore, maxScore });
					location.temporarySprites.Add(new TemporaryAnimatedSprite()
					{
						text = displayText,
						local = true,
						position = new Vector2((float)(Game1.viewport.Width / 2) - Game1.dialogueFont.MeasureString(displayText).X / 2f, (float)(Game1.tileSize / 2)), // was originally /4,  
						color = Color.White,
						interval = 15000f, // Lasts for 15 seconds
						layerDepth = 1f,
						animationLength = 1,
						initialParentTileIndex = 1,
						currentParentTileIndex = 1,
						totalNumberOfLoops = 1
					});
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(grandpaEvaluations_Postfix)}:\n{ex}",
					LogLevel.Error);
			}
		}

		public static void skipEvent_Postfix(Event __instance)
		{
			try
			{
				switch(__instance.id)
				{
					case 558291: // Initial
					case 558292: // Reevaluation
						Game1.player.eventsSeen.Remove(2146991); // Candles event removal for initial eval after a reset cmd
						Game1.player.eventsSeen.Remove(321777); // Remove re-evaluation request flag
						if (!Game1.player.mailReceived.Contains("6324hasDoneModdedEvaluation"))
						{
							Game1.player.mailReceived.Add("6324hasDoneModdedEvaluation"); // Activate bonus rewards
						}
						break;
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(skipEvent_Postfix)}:\n{ex}",
					LogLevel.Error);
			}
		}
	}
}