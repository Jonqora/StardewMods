using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static System.Math;

namespace AngryGrandpa
{
	internal class EventEditor : IAssetEditor
	{
		protected static IModHelper Helper => ModEntry.Instance.Helper;
		protected static IMonitor Monitor => ModEntry.Instance.Monitor;
		protected static ModConfig Config => ModConfig.Instance;

		protected static ITranslationHelper i18n = Helper.Translation;

		public bool CanEdit<_T> (IAssetInfo asset)
		{
			return asset.AssetNameEquals($"Data\\Events\\Farm") ||
				asset.AssetNameEquals($"Data\\Events\\Farmhouse");
		}

		public void Edit<_T> (IAssetData asset)
		{
			var data = asset.AsDictionary<string, string>().Data;

			// Change the event key for CandleEvent
			if ( asset.AssetNameEquals($"Data\\Events\\Farm") )
			{
				string entry = "CandleEvent";
				string value = i18n.Get(entry + ".gameValue");
				string gameKey = i18n.Get(entry + ".gameKey");

				Regex regex = new Regex(@"^2146991/.*"); // Matches any event key that starts with "2146991/"

				List<string> todelete = new List<string> { };
				foreach (string k in data.Keys)
				{
					if (regex.Match(k).Success)
					{
						value = data[k]; // Keep the event script to reassign
						todelete.Add(k);
					}
				}
				todelete.ForEach(k => data.Remove(k)); // Remove the old event key

				// Replace /y 3/ (or any other y number) with the appropriate value
				if (Config.YearsBeforeEvaluation > 0)
				{
					gameKey = Regex.Replace(gameKey, @"/y [0-9]+", $@"/y {Config.YearsBeforeEvaluation + 1}");
				}
				else // YBE == 0, remove year precondition entirely
				{
					gameKey = Regex.Replace(gameKey, @"/y [0-9]+", "");
				}

				data[gameKey] = value; // Insert the event data with correct preconditions.
			}

			// Change the event key for EvaluationEvent and edit the scripts for EvaluationEvent, RepeatEvaluationEvent
			else if (asset.AssetNameEquals($"Data\\Events\\Farmhouse"))
			{
				// Prepare tokens (even if there's only one)
				string countYears;
				int yearsPassed = Max(Game1.year - 1, Config.YearsBeforeEvaluation); // Accurate dialogue even for delayed event
				if (yearsPassed >= 10)
				{
					if (Config.GrandpaDialogue == "Nuclear") { countYears = i18n.Get("GrandpaCountManyYears.Nuclear"); }
					else { countYears = i18n.Get("GrandpaCountManyYears"); }
				}
				else // yearsPassed < 10
				{
					countYears = i18n.Get("GrandpaCountYears").ToString().Split('|')[yearsPassed];
				}

				// Delete old entries for the Evaluation event
				Regex regex = new Regex(@"^558291/.*"); // Matches any event key that starts with "558291/"
				List<string> todelete = data.Keys.Where(k => regex.Match(k).Success).ToList();
				todelete.ForEach(k => data.Remove(k)); // Remove the old event script completely

				// Organize the keys and values with corrected event preconditions
				foreach (string entry in new List<string> { "EvaluationEvent", "RepeatEvaluationEvent" })
				{
					string gameKey = i18n.Get(entry + ".gameKey");
					string modKey = entry + "." + Config.GrandpaDialogue;
					if (Config.GenderNeutrality) { modKey += "-gn"; }

					// Replace /y 3/ (or any other y number) with the appropriate value
					if ( Config.YearsBeforeEvaluation > 0 )
					{
						gameKey = Regex.Replace( gameKey, @"/y [0-9]+", $@"/y {Config.YearsBeforeEvaluation + 1}" );
					}
					else // YBE == 0, remove year precondition entirely
					{
						gameKey = Regex.Replace( gameKey, @"/y [0-9]+", "" );
					}
					string value = i18n.Get(modKey, new { countYears });

					data[gameKey] = value; // Insert the new event data.
				}
			}
			else { return; }

			Monitor.Log($"Edited {asset.AssetName}", LogLevel.Debug);
		}
	}
}