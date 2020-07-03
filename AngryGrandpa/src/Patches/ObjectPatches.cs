using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;
using Netcode;
using System;

namespace AngryGrandpa
{
	class ObjectPatches
	{
		private static IModHelper Helper => ModEntry.Instance.Helper;
		private static IMonitor Monitor => ModEntry.Instance.Monitor;
		private static ModConfig Config => ModConfig.Instance;
		private static HarmonyInstance Harmony => ModEntry.Instance.Harmony;

		protected static ITranslationHelper i18n = Helper.Translation;

		public static void Apply()
		{
			Harmony.Patch(
				original: AccessTools.Method(typeof(Object),
					nameof(Object.checkForSpecialItemHoldUpMeessage)),
				postfix: new HarmonyMethod(typeof(UtilityPatches),
					nameof(ObjectPatches.checkForSpecialItemHoldUpMeessage_Postfix))
			);
		}

		public static string checkForSpecialItemHoldUpMeessage_Postfix(string __result, Object __instance)
		{
			try
			{
				if (!(bool)(NetFieldBase<bool, NetBool>)__instance.bigCraftable &&
					(NetFieldBase<string, NetString>)__instance.type != (NetString)null && __instance.type.Equals((object)"Arch") &&
					Game1.getFarm().grandpaScore != 0 &&
					Game1.currentLocation is Farm)
				{
					switch ((int)(NetFieldBase<int, NetInt>)__instance.parentSheetIndex)
					{
						case 114: // Ancient seed
							__result = i18n.Get("Object.cs.1CandleReward");
							break;
						case 107: // Dinosaur egg
							__result = i18n.Get("Object.cs.2CandleReward");
							break;
						case 74: // Prismatic shard
							__result = i18n.Get("Object.cs.3CandleReward");
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Monitor.Log($"Failed in {nameof(checkForSpecialItemHoldUpMeessage_Postfix)}:\n{ex}",
					LogLevel.Error);
			}
			return __result;
		}
	}
}
