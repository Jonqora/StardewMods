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
                prefix: new HarmonyMethod(AccessTools.Method(typeof(HUDPatch), nameof(HUDPatch.addHUDMessage_Prefix)))
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

            /// <summary>When adding a new HUD message that stacks with a previous one, use the newest Message's messageSubject.</summary>
            internal static bool addHUDMessage_Prefix(HUDMessage message)
            {
                try
                {
                    if (message.type != null || message.whatType != 0)
                    {
                        for (int index = 0; index < Game1.hudMessages.Count; ++index)
                        {
                            if (message.type != null && Game1.hudMessages[index].type != null && (Game1.hudMessages[index].type.Equals(message.type) && Game1.hudMessages[index].add == message.add))
                            {
                                //Game1.hudMessages[index].number = message.add ? Game1.hudMessages[index].number + message.number : Game1.hudMessages[index].number - message.number;

                                //Altered code to affect and keep current message in the place of existing one
                                message.number = message.add ? Game1.hudMessages[index].number + message.number : Game1.hudMessages[index].number - message.number;
                                Game1.hudMessages.RemoveAt(index);
                                Game1.hudMessages.Insert(index, message);

                                Game1.hudMessages[index].timeLeft = 3500f;
                                Game1.hudMessages[index].transparency = 1f;
                                return false; // don't run original logic
                            }
                            if (message.whatType == Game1.hudMessages[index].whatType && message.whatType != 1 && (message.message != null && message.message.Equals(Game1.hudMessages[index].message)))
                            {
                                Game1.hudMessages[index].timeLeft = message.timeLeft;
                                Game1.hudMessages[index].transparency = 1f;
                                return false; // don't run original logic
                            }
                        }
                    }
                    Game1.hudMessages.Add(message);
                    for (int index = Game1.hudMessages.Count - 1; index >= 0; --index)
                    {
                        if (Game1.hudMessages[index].noIcon)
                        {
                            HUDMessage hudMessage = Game1.hudMessages[index];
                            Game1.hudMessages.RemoveAt(index);
                            Game1.hudMessages.Add(hudMessage);
                        }
                    }

                    return false; // don't run original logic
                }
                catch (Exception ex)
                {
                    ModMonitor.Log($"Failed in {nameof(addHUDMessage_Prefix)}:\n{ex}", LogLevel.Error);
                    return true; // run original logic
                }
            }
        }
    }
}