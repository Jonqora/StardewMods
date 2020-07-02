﻿using Harmony;
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

namespace AngryGrandpa
{
	class EventPatches
	{
		private static IModHelper Helper => ModEntry.Instance.Helper;
		private static IMonitor Monitor => ModEntry.Instance.Monitor;
		private static ModConfig Config => ModConfig.Instance;

		private static HarmonyInstance Harmony => ModEntry.Instance.Harmony;

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
		}

		public static void grandpaEvaluations_Prefix()
		{
			try
			{
				Helper.Content.InvalidateCache(asset // Get updated assets to use during evaluation.
				=> asset.AssetNameEquals("Strings\\StringsFromCSLocations"));
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(grandpaEvaluations_Prefix)}:\n{ex}",
					LogLevel.Error);
			}
		}
		
		public static void grandpaEvaluations_Postfix()
		{
			try
			{
				Game1.player.eventsSeen.Remove(2146991); // Extra removal needed for initial evaluation after a reset command
				Game1.player.eventsSeen.Remove(321777); // Remove re-evaluation request flag
				if (Config.ShowPointsTotal)
				{
					int grandpaScore = Utility.getGrandpaScore();
					int maxScore = Config.GetMaxScore();
					string displayText = $"Grandpa's Score: {grandpaScore} of {maxScore} Great Honors";
					location.temporarySprites.Add(new TemporaryAnimatedSprite()
					{
						text = displayText,
						local = true,
						position = new Vector2((float)(Game1.viewport.Width / 2) - Game1.dialogueFont.MeasureString(displayText).X / 2f, (float)(Game1.tileSize / 2)), // was /4,  
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
	}
}