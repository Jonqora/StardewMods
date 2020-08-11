using StardewModdingAPI;

namespace DynamicConversationTopics
{
    class ModConfig
    {
        public SButton debugKey { get; set; }

        public ModConfig()
        {
            debugKey = SButton.J;
        }
    }
}
