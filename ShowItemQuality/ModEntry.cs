using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Reflection.Emit;

namespace ShowItemQuality
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Accessors 
        *********/
        /// <summary>Provides methods for logging to the console.</summary>
        public static IMonitor ModMonitor { get; private set; }

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModMonitor = this.Monitor;

            HarmonyInstance harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            // Apply the patch to show item quality when drawing in HUD
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.HUDMessage), nameof(StardewValley.HUDMessage.draw)),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(HUDPatch), nameof(HUDPatch.DrawMethodTranspiler)))
            );
        }
        /*********
        ** Harmony patches
        *********/
        /// <summary>Contains patches for patching game code in the StardewValley.HUDMessage class.</summary>
        internal class HUDPatch
        {
            /// <summary>Changes an argument in this.messageSubject.drawInMenu() to use StackDrawType.Draw instead of StackDrawType.Hide)</summary>
            internal static IEnumerable<CodeInstruction> DrawMethodTranspiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count - 1; i++)
                {
                    // find Enum value of 0 (StackDrawType.Hide) loaded for the last argument of this.messageSubject.drawInMenu()
                    if (codes[i].opcode == OpCodes.Ldc_I4_0 &&
                        codes[i + 1].opcode == OpCodes.Callvirt &&
                        codes[i + 1].operand.ToString() == "Void drawInMenu(Microsoft.Xna.Framework.Graphics.SpriteBatch, Microsoft.Xna.Framework.Vector2, Single, Single, Single, StardewValley.StackDrawType)")
                    {
                        // change to Enum value 1 (StackDrawType.Draw)
                        codes[i].opcode = OpCodes.Ldc_I4_1;
                        break;
                    }
                }
                return codes.AsEnumerable();
            }
        }
    }
}