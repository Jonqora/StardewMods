using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace AngryGrandpa
{
	internal class GrandpaNoteEditor : IAssetEditor
	{
		protected static IModHelper Helper => ModEntry.Instance.Helper;
		protected static IMonitor Monitor => ModEntry.Instance.Monitor;
		protected static ModConfig Config => ModConfig.Instance;

		protected static ITranslationHelper i18n = Helper.Translation;

		public bool CanEdit<_T> (IAssetInfo asset)
		{
			return asset.AssetNameEquals($"Strings\\Locations") ||
				asset.AssetNameEquals($"Data\\mail");
		}

		public void Edit<_T> (IAssetData asset)
		{
			string keyGameData;
			string keyModData;
			string value;

			// Get the game's data key, and the corresponding key for it in mod data
			if (asset.AssetNameEquals($"Strings\\Locations"))
			{
				keyGameData = "Farm_GrandpaNote";
				keyModData = "GrandpaNote";
			}
			else if (asset.AssetNameEquals($"Data\\mail"))
			{
				keyGameData = "6324grandpaNoteMail";
				keyModData = "GrandpaNoteMail";
			}
			else { return; }

			// Get and edit the appropriate string values
			if (Config.YearsBeforeEvaluation >= 10)
			{
				keyModData += "TenPlusYears";
				var smapiSDate = new SDate(1, "spring", Config.YearsBeforeEvaluation + 1).ToLocaleString();
				value = i18n.Get(keyModData, new { smapiSDate });
			}
			else // YearsBeforeEvaluation < 10
			{
				var ordinalYear = i18n.Get("GrandpaOrdinalYears").ToString().Split('|')[Config.YearsBeforeEvaluation];
				value = i18n.Get(keyModData, new { ordinalYear });
			}

			// Apply the changes to game data
			var data = asset.AsDictionary<string, string>().Data;
			data[keyGameData] = value;

			Monitor.Log($"Edited {asset.AssetName}", LogLevel.Debug);
		}
	}
}