using StardewModdingAPI;
using System;
using System.Linq;

namespace AngryGrandpa
{
    public class ModConfig
    {
        protected static IModHelper Helper => ModEntry.Instance.Helper;
        protected static IMonitor Monitor => ModEntry.Instance.Monitor;

        internal static ModConfig Instance { get; private set; }

        // Fields for config values
        public string GrandpaDialogue 
        { 
            get { return _grandpaDialogue; } 
            set 
            {
                if (GrandpaDialogueChoices.Contains(value)) _grandpaDialogue = value;
                else
                {
                    string fallback = GrandpaDialogueChoices[0]; // Default to "Original"
                    _grandpaDialogue = fallback;
                    Monitor.Log($"Invalid config value \"{value}\" for ScoringSystem. \n                Accepted values are [{string.Join(", ", GrandpaDialogueChoices)}]. \n                Config has been reset to default value \"{fallback}\".", LogLevel.Warn);
                }
            } 
        }
        internal static string[] GrandpaDialogueChoices = new string[] { "Original", "Vanilla", "Nuclear" };
        private string _grandpaDialogue;

        public bool? GenderNeutrality
        {
            get { Monitor.Log("Ran GenderNeutrality.get()", LogLevel.Debug); return _genderNeutrality; }
            set
            {
                Monitor.Log("Ran GenderNeutrality.set()", LogLevel.Debug);
                if (value == null)
                {
                    Monitor.Log("Looking for Hana.GenderNeutralityMod", LogLevel.Debug);
                    if (Helper.ModRegistry.IsLoaded("Hana.GenderNeutralityMod"))
                    {
                        _genderNeutrality = true;
                        Monitor.Log($"GenderNeutralityMod detected. Initializing AngryGrandpa gender neutrality config to {_genderNeutrality}", LogLevel.Info);
                    }
                    else { _genderNeutrality = false; Monitor.Log($"GenderNeutralityMod NOT detected.", LogLevel.Info); }
                }
                else { _genderNeutrality = value ?? false; }
            }
        }
        private bool _genderNeutrality;

        public bool ShowPointsTotal { get; set; }

        public string ScoringSystem
        {
            get { return _scoringSystem; }
            set
            {
                if (ScoringSystemChoices.Contains(value)) _scoringSystem = value;
                else
                {
                    string fallback = ScoringSystemChoices[1]; // Default to "Vanilla"
                    _scoringSystem = fallback;
                    Monitor.Log($"Invalid config value \"{value}\" for ScoringSystem. \n                Accepted values are [{string.Join(", ", ScoringSystemChoices)}]. \n                Config has been reset to default value \"{fallback}\".", LogLevel.Warn);
                }
            }
        }
        internal static string[] ScoringSystemChoices = new string[] { "Original", "Vanilla", "Hard", "Expert" };
        private string _scoringSystem;

        public int YearsBeforeEvaluation 
        {
            get { return _yearsBeforeEvaluation; }
            set {
                if (value >= 0) _yearsBeforeEvaluation = value;
                else
                {
                    _yearsBeforeEvaluation = 2; // Default to 2 years
                    Monitor.Log($"Invalid config value [{value}] for YearsBeforeEvaluation. \n                You must enter a non-negative integer. \n                Config has been reset to default value [2].", LogLevel.Warn);
                }
            }
        }
        private int _yearsBeforeEvaluation;

        public bool BonusRewards { get; set; }

        // Custom constructor, set defaults here
        public ModConfig()
        {
            this.GrandpaDialogue = "Original";
            this.GenderNeutrality = null; // Initialize with null so we can intercept and detect the appropriate default settings
            this.ShowPointsTotal = true;
            this.ScoringSystem = "Vanilla";
            this.YearsBeforeEvaluation = 2;
            this.BonusRewards = true;

            Monitor.Log("Ran ModConfig()", LogLevel.Debug);

            /**if (this.GenderNeutrality ?? false)
            {
                Monitor.Log($"GenderNeutralityMod detected. Initializing AngryGrandpa gender neutrality config to {this.GenderNeutrality}", LogLevel.Info);
            }**/
        }

        // Generic Mod Config Menu helper functions
        internal static void Load() 
        { 
            Instance = Helper.ReadConfig<ModConfig>(); 
            // Check for GNM - if null, do the log thing and set
        }
        internal static void Save()
        {
            Helper.WriteConfig(Instance); 
        }
        internal static void Reset()
        {
            Instance = new ModConfig();
            // Set GN config to default and if GNM present, do the log thing
        }

        internal static void SetUpMenu()
        {
            // Register API stuff for Generic Mod Config Menu
            var api = Helper.ModRegistry.GetApi<GenericModConfigMenu.IApi>
                ("spacechase0.GenericModConfigMenu");

            if (api == null)
                return;

            var manifest = ModEntry.Instance.ModManifest;
            api.RegisterModConfig(manifest, Reset, Save);
            string NL = Environment.NewLine;

            api.RegisterLabel(manifest, "Dialogue Options", "");

            api.RegisterChoiceOption(manifest,
                    "Choose grandpa's dialogue style",
                    $"Changes the dialogue used during evaluation and re-evaluation events.{NL}" +
                    $"  Original - Harsher dialogue found in early versions of the game{NL}" +
                    $"  Vanilla - Normal dialogue used in the game ever since version 1.05{NL}" +
                    $"  Nuclear - Grandpa is very enthusiastic about his opinions. WARNING:Profanity!",
                    () => Instance.GrandpaDialogue,
                    (string val) => Instance.GrandpaDialogue = val,
                    ModConfig.GrandpaDialogueChoices);

            api.RegisterSimpleOption(manifest,
                    "Use gender-neutral dialogue",
                    "Removes references to player gender from dialogue strings",
                    () => Instance.GenderNeutrality ?? false,
                    (bool val) => Instance.GenderNeutrality = val);

            api.RegisterLabel(manifest, "", "");
            api.RegisterLabel(manifest, "Scoring Options", "");

            api.RegisterChoiceOption(manifest,
                    "Choose scoring system",
                    $"Changes how points are scored and how many are required to earn 4 candles:{NL}" +
                    $"  Original - Original game evaluation: 13 possible points, 12+ earns 4 candles{NL}" +
                    $"  Vanilla - Normal game evaluation: 21 possible points, 12+ earns 4 candles{NL}" +
                    $"  Hard - Harder scoring option: needs 18/21 points to earn 4 candles{NL}" +
                    $"  Expert - Hardest scoring option: needs all 21 points for 4 candles!",
                    () => Instance.ScoringSystem,
                    (string val) => Instance.ScoringSystem = val,
                    ModConfig.ScoringSystemChoices);

            api.RegisterSimpleOption(manifest,
                    "Years before evaluation",
                    $"How many in-game years to wait before grandpa's first visit{NL}" +
                    $"  Default is [2] - grandpa will appear Spring 1 of Year 3",
                    () => Instance.YearsBeforeEvaluation,
                    (int val) => Instance.YearsBeforeEvaluation = val);

            api.RegisterSimpleOption(manifest,
                    "Display points total",
                    "Shows your raw score during the evaluation",
                    () => Instance.ShowPointsTotal,
                    (bool val) => Instance.ShowPointsTotal = val);

            api.RegisterLabel(manifest, "", "");
            api.RegisterLabel(manifest, "Bonus Rewards", "");

            api.RegisterSimpleOption(manifest,
                    "Enable bonus rewards",
                    "Gives new bonus rewards for earning 1-3 candles",
                    () => Instance.BonusRewards,
                    (bool val) => Instance.BonusRewards = val);

            Monitor.Log("Added Angry Grandpa Config to GMCM", LogLevel.Info);
        }

        internal static void Print()
        {
            Monitor.Log(
                $@"====================
                ANGRY GRANDPA CONFIG
                GrandpaDialogue: ""{Instance.GrandpaDialogue}""
                GenderNeutrality: {Instance.GenderNeutrality}
                ShowPointsTotal: {Instance.ShowPointsTotal}
                ScoringSystem: ""{Instance.ScoringSystem}""
                YearsBeforeEvaluation: {Instance.YearsBeforeEvaluation}
                BonusRewards: {Instance.BonusRewards}
                ====================", LogLevel.Debug);
        }
    }
}
