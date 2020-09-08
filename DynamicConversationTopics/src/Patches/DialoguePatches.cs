using Harmony;
using CIL = Harmony.CodeInstruction;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;

namespace DynamicConversationTopics
{
    /// <summary>The class for patching methods related to NPC dialogue handling.</summary>
    public class DialoguePatches
    {
        /*********
        ** Accessors
        *********/
        private static IModHelper Helper => ModEntry.Instance.Helper;
        private static IMonitor Monitor => ModEntry.Instance.Monitor;
        private static HarmonyInstance Harmony => ModEntry.Instance.Harmony;

        internal protected static ModConfig Config => ModConfig.Instance;


        /*********
        ** Fields
        *********/
        //The section of CodeInstructions where we should exit the loop if HasSpokenRecently is true.
        //Desired index is 0 (the start of the function).
        static List<CIL> matchStart = new List<CIL>() { };

        //The section of CodeInstructions where we should add a call to AddToRecentTopicSpeakers.
        //Desired index is right after the Brtrue_S, so index 4 in this list.
        static List<CIL> matchAddToRecentTopicSpeakers = new List<CIL>()
        {
            //(C#)
            // if (!s.Contains("dumped"))
			// {
			//     Game1.player.mailReceived.Add(base.Name + "_" + eventMessageKey);
		    // }

            //(CIL)
            //ldloc.s s
            //ldstr "dumped"
            //callvirt instance bool [mscorlib]System.String::Contains(string)
            //brtrue.s IL_00c9 (offset 9?)
            //call class StardewValley.Farmer StardewValley.Game1::get_player()
            //ldfld class [Netcode]Netcode.NetStringList StardewValley.Farmer::mailReceived
            //ldarg.0
            //call instance string StardewValley.Character::get_Name()
            //ldstr "_"
            //ldloc.0
            //call string [mscorlib]System.String::Concat(string, string, string)
            //callvirt instance void class [Netcode]Netcode.NetList`2<string, class [Netcode]Netcode.NetString>::Add(!0)

            new CIL(OpCodes.Ldloc_S, null), //Null to match any variable
            new CIL(OpCodes.Ldstr, "dumped"),
            new CIL(OpCodes.Callvirt, typeof(string).GetMethod("Contains", new Type[]{ typeof(string) })),
            new CIL(OpCodes.Brtrue, null), //Null to match any offset
            new CIL(OpCodes.Call, typeof(Game1).GetMethod("get_player")),
            new CIL(OpCodes.Ldfld, typeof(Farmer).GetField("mailReceived")),
            new CIL(OpCodes.Ldarg_0),
            new CIL(OpCodes.Call, typeof(Character).GetMethod("get_Name")),
            new CIL(OpCodes.Ldstr, "_"),
            new CIL(OpCodes.Ldloc_0),
            new CIL(OpCodes.Call, typeof(string).GetMethod("Concat", new Type[]{ typeof(string), typeof(string), typeof(string) })),
            new CIL(OpCodes.Callvirt, typeof(Netcode.NetList<string, Netcode.NetString>).GetMethod("Add", new Type[] { typeof(string)}))
        };

        //The section of CodeInstructions where we should identify the Leave_S operand and use it to exit the loop earlier
        //Desired index is Leave_S, so index 3 in this list.
        static List<CIL> matchEndForeachLoop = new List<CIL>()
        {
            //ldloca.s 2
            //call instance bool valuetype [Netcode]Netcode.NetDictionary`5/KeysCollection/Enumerator<string, int32, class [Netcode]Netcode.NetInt, class StardewValley.SerializableDictionary`2<string, int32>, class StardewValley.Network.NetStringDictionary`2<int32, class [Netcode]Netcode.NetInt>>::MoveNext()
            //brtrue IL_0023
            //leave.s IL_00ed

            new CIL(OpCodes.Ldloca_S, null), //Null to match any variable
            new CIL(OpCodes.Call, null), //Null to match whatever the heck that is D:
            new CIL(OpCodes.Brtrue, null), //Null to match any offset
            new CIL(OpCodes.Leave, null) //This is the branch instruction whose operand label I want to copy
        };


        /*********
        ** Public methods
        *********/
        /// <summary>
        /// Applies the harmony patches defined in this class.
        /// </summary>
        public static void Apply()
        {
            // Add checks to limit conversation topic dialogues so they aren't all displayed one after the other.
            Harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.checkForNewCurrentDialogue)),
                transpiler: new HarmonyMethod(AccessTools.Method(
                    typeof(DialoguePatches),
                    nameof(DialoguePatches.checkForNewCurrentDialogue_Transpiler)
                    ))
                );
        }

        /// <summary>
        /// Finds any sequence matching target code, then adds conditional skip logic and tracking to conversation topic dialogue checker.
        /// </summary>
        /// <param name="instructions">Harmony-provided CodeInstruction enumerable for the original checkForNewCurrentDialogue method</param>
        /// <returns>Altered CodeInstruction enumerable if a location was found and patches applied; else returns original codes</returns>
        public static IEnumerable<CIL> checkForNewCurrentDialogue_Transpiler(IEnumerable<CIL> instructions)
        {
            try
            {
                var codes = new List<CIL>(instructions);

                int? insertSkipLocation = Utilities.findListMatch(codes, matchStart);
                int? insertTrackLocation = insertSkipLocation == null ? null : Utilities.findListMatch(codes, matchAddToRecentTopicSpeakers, insertSkipLocation.Value, 4);
                int? findTargetLocation = insertTrackLocation == null ? null : Utilities.findListMatch(codes, matchEndForeachLoop, insertTrackLocation.Value, 3);

                if (insertSkipLocation != null &&
                    insertTrackLocation != null &&
                    findTargetLocation != null)
                {
                    // Debug output
                    Monitor.Log($"Found patch location for {nameof(checkForNewCurrentDialogue_Transpiler)}:\n" +
                        $"insertSkipLocation = {insertSkipLocation}\n" +
                        $"insertTrackLocation = {insertTrackLocation}\n" +
                        $"findTargetLocation = {findTargetLocation}", Config.DebugMode ? LogLevel.Debug : LogLevel.Trace);

                    // Identify the target label needed for our new conditional branch
                    Label skipTargetLabel = (Label)codes[findTargetLocation.Value].operand;

                    // Compose new instructions for tracking recent NPCs who gave conversation topic dialogue
                    // AddToRecentTopicSpeakers(this)
                    var trackCodesToInsert = new List<CIL>
                    {
                        new CIL(OpCodes.Ldarg_0),
                        new CIL(OpCodes.Call, Helper.Reflection.GetMethod(
                            typeof(DialoguePatches),nameof(AddToRecentTopicSpeakers)).MethodInfo)
                    };

                    // Compose new instructions to skip checking for conversation topic dialogue if spoken recently
                    // if (!HasSpokenRecently(this))
                    // {
                    //   foreach (string s in Game1.player.activeDialogueEvents.Keys)
                    //   {
                    //     ...
                    //   }
                    // }
                    // ...
                    var skipCodesToInsert = new List<CIL>
                    {
                        new CIL(OpCodes.Ldarg_0),
                        new CIL(OpCodes.Call, Helper.Reflection.GetMethod(
                            typeof(DialoguePatches),nameof(HasSpokenRecently)).MethodInfo),
                        new CIL(OpCodes.Brtrue, skipTargetLabel)
                    };

                    // Inject the instruction sets (last section to first to avoid index mixups)
                    codes.InsertRange(insertTrackLocation.Value, trackCodesToInsert);
                    codes.InsertRange(insertSkipLocation.Value, skipCodesToInsert);

                    Monitor.LogOnce($"Applied harmony patch to class NPC: {nameof(checkForNewCurrentDialogue_Transpiler)}", LogLevel.Trace);
                    return codes.AsEnumerable();
                }
                else
                {
                    // Debug output
                    Monitor.Log($"Failed to find patch location for {nameof(checkForNewCurrentDialogue_Transpiler)}:\n" +
                        $"insertSkipLocation = {insertSkipLocation}\n" +
                        $"insertTrackLocation = {insertTrackLocation}\n" +
                        $"findTargetLocation = {findTargetLocation}", Config.DebugMode ? LogLevel.Debug : LogLevel.Trace);

                    Monitor.Log($"Couldn't apply harmony patch to class NPC: {nameof(checkForNewCurrentDialogue_Transpiler)}\n" +
                    $"The quality-of-life feature to space out topic dialogues will be inactive, but all main features of this mod should continue to work correctly.\n" +
                    $"You don't need to worry about this error in your game, but please report it to the mod author anyway for bugtesting.", LogLevel.Warn);
                    return instructions; // use original code if could not apply both edits correctly
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(checkForNewCurrentDialogue_Transpiler)}:\n{ex}", LogLevel.Error);
                return instructions; // use original code
            }
        }

        /// <summary>
        /// Checks to see if the player has already seen conversation topic dialogue from an NPC recently.
        /// </summary>
        /// <param name="npc">NPC to check</param>
        /// <returns>true if NPC found in RecentTopicSpeakers</returns>
        public static bool HasSpokenRecently(NPC npc)
        {
            try
            {
                return ModEntry.Instance.RecentTopicSpeakers.ContainsKey(npc.Name);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(HasSpokenRecently)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }

        /// <summary>
        /// Adds an NPC to RecentTopicSpeakers with the current integer game time.
        /// </summary>
        /// <param name="npc">NPC to add</param>
        public static void AddToRecentTopicSpeakers(NPC npc)
        {
            try
            {
                string name = npc.Name;
                int gametime = Game1.timeOfDay;
                ModEntry.Instance.RecentTopicSpeakers[name] = gametime;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(AddToRecentTopicSpeakers)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
