using Harmony;
using StardewModdingAPI;
using StardewValley;
using System;

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace BugFixAddItem
{
    class UtilityPatches
	{
		private static IModHelper Helper => ModEntry.Instance.Helper;
		private static IMonitor Monitor => ModEntry.Instance.Monitor;

		private static HarmonyInstance Harmony => ModEntry.Instance.Harmony;

		public static void Apply()
		{
			Harmony.Patch(
				original: AccessTools.Method( typeof(Utility), nameof(Utility.addItemToInventory) ),
				transpiler: new HarmonyMethod( AccessTools.Method(typeof(UtilityPatches), nameof(UtilityPatches.addItemToInventory_Transpiler) ) )
			);
		}

        internal static IEnumerable<CodeInstruction> addItemToInventory_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count - 1; i++)
                {
                    // Find any null value appearing as the last argument of a ItemGrabMenu.behaviorOnItemSelect delegate method call
                    if (codes[i].opcode == OpCodes.Ldnull &&
                        codes[i + 1].opcode == OpCodes.Callvirt &&
                        codes[i + 1].operand.ToString() == "callvirt instance void StardewValley.Menus.ItemGrabMenu/behaviorOnItemSelect::Invoke(class StardewValley.Item, class StardewValley.Farmer)")
                    {
                        // change (Farmer) null to Game1.player
                        codes[i].opcode = OpCodes.Ldsfld;
                        codes[i].operand = Game1.player;
                        Monitor.Log($"Edited OpCode: {codes[i]}", LogLevel.Debug);
                        break;
                    }
                }
                return codes.AsEnumerable();
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(addItemToInventory_Transpiler)}:\n{ex}", LogLevel.Error);
                return instructions; // use original code
            }
        }
    }
}
