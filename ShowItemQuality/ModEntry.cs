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
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(HUDPatch), nameof(HUDPatch.HUDMessageDraw_Transpiler)))
            );

            // Apply the patch to use most recent item in a stack to display HUD icon
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Game1), nameof(StardewValley.Game1.addHUDMessage)),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(HUDPatch), nameof(HUDPatch.addHUDMessage_Postfix)))
            );
        }
        /*********
        ** Harmony patches
        *********/
        /// <summary>Contains patches for patching game code in the StardewValley.HUDMessage class.</summary>
        internal class HUDPatch
        {
            /// <summary>Changes an argument in this.messageSubject.drawInMenu() to use StackDrawType.Draw instead of StackDrawType.Hide)</summary>
            internal static IEnumerable<CodeInstruction> HUDMessageDraw_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                try
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
                catch (Exception ex)
                {
                    ModMonitor.Log($"Failed in {nameof(HUDMessageDraw_Transpiler)}:\n{ex}", LogLevel.Error);
                    return instructions; // use original code
                }
            }
            /// <summary>When adding an HUD message that stacks with a previous one, use the newest message to update messageSubject.</summary>
            internal static void addHUDMessage_Postfix(HUDMessage message)
            {
                try
                {
                    if (message.type != null || message.whatType != 0)
                    {
                        for (int index = 0; index < Game1.hudMessages.Count; ++index)
                        {
                            if (message.type != null && Game1.hudMessages[index].type != null && (Game1.hudMessages[index].type.Equals(message.type) && Game1.hudMessages[index].add == message.add))
                            {
                                // Altered code to affect and keep current message in the place of existing one
                                // Keep the updated stack number
                                message.number = Game1.hudMessages[index].number; 
                                // Replace the old HUDMessage with the new one, instead of keeping the old.
                                Game1.hudMessages.RemoveAt(index);
                                Game1.hudMessages.Insert(index, message);
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModMonitor.Log($"Failed in {nameof(addHUDMessage_Postfix)}:\n{ex}", LogLevel.Error);
                }
            }
        }
    }
}