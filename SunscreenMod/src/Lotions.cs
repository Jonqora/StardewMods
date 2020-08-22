using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunscreenMod
{
    class Lotions
    {
        protected static IModHelper Helper => ModEntry.Instance.Helper;
        protected static IMonitor Monitor => ModEntry.Instance.Monitor;
        private static ModConfig Config => ModConfig.Instance;


        protected static ITranslationHelper i18n = Helper.Translation;

        protected static JsonAssets.IApi JA = ModEntry.Instance.JA;

        public int[] LotionIDs { get; private set; }

        //Lotion types this mod adds to the game
        public bool IsLotion(Item item)
        {
            if (item is StardewValley.Object &&
                !(item as StardewValley.Object).bigCraftable.Value &&
                !(item is Wallpaper) &&
                !(item is Furniture))
            {
                LotionIDs = new int[] {
                    JA.GetObjectId("SPF60 Sunscreen"),
                    JA.GetObjectId("Aloe Vera Gel")
                };
                if (LotionIDs.Contains(item.ParentSheetIndex))
                {
                    return true;
                }
            }
            return false;
        }

        //Called when a user clicks on their player while holding a lotion bottle
        public void ApplyQuestion(Item item)
        {
            //TODO: Check if it can be applied (Aloe Vera Gel can only be applied once a day)
            if (false)
            {
                Game1.drawDialogueNoTyping(i18n.Get("Error.AlreadyUsedLotionToday"));
                return;
            }

            string question = i18n.Get("Question.ApplyLotion", new { lotionName = item.DisplayName });
            Response[] yesNoResponses = Game1.currentLocation.createYesNoResponses();
            GameLocation.afterQuestionBehavior afterAnswer = ApplyLotionAnswer;

            Game1.currentLocation.createQuestionDialogue(question, yesNoResponses, afterAnswer);
        }

        public void ApplyLotionAnswer(Farmer who, string whichAnswer)
        {
            Monitor.Log($"Player chose answer: {whichAnswer}", LogLevel.Debug);
            if (whichAnswer == "No")
            {
                return; //Don't use any lotion
            }

            if (Game1.player.ActiveObject.ParentSheetIndex == JA.GetObjectId("SPF60 Sunscreen"))
            {
                ApplySunscreen(who);
            }
            else if (Game1.player.ActiveObject.ParentSheetIndex == JA.GetObjectId("SPF60 Sunscreen"))
            {
                ApplyAloeGel(who);
            }
            //remove one from the stack
            Game1.player.reduceActiveItemByOne();
            Game1.player.jump(4.0f); //Default jump is 8f
            DelayedAction.playSoundAfterDelay("slimedead", 500);
            //sound effect and/or animation? OOH slimeHit, slimedead, cavedrip, shadowHit, killAnimal, fishSlap, harvest, hitEnemy, dropItemInWater, pullItemFromWater, bob, waterSlosh, slosh, 
            //Candidates: slimeHit, slimedead, fishSlap, bob
        }

        public void ApplySunscreen(Farmer who)
        {
            //apply the sunscreen's effects
            Game1.addHUDMessage(new HUDMessage(i18n.Get("Apply.Sunscreen"), Color.OrangeRed, 5250f));

            //make skin briefly white????
        }

        public void ApplyAloeGel(Farmer who)
        {
            //apply the gel's effects
            who.Stamina = Math.Min((float)who.MaxStamina, who.Stamina + (float)Config.EnergyLossPerLevel);
            Game1.addHUDMessage(new HUDMessage(i18n.Get("Apply.AloeGel"), 4)); //stamina_type HUD message

            //make skin briefly green????
            
        }
    }
}
