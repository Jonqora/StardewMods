using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;

namespace AngryGrandpa
{
    public class ModConfig
    {
        protected static IModHelper Helper => ModEntry.Instance.Helper;
        protected static IMonitor Monitor => ModEntry.Instance.Monitor;

        internal static ModConfig Instance { get; private set; }

        #region Properties and Fields for config values
        public string GrandpaDialogue 
        { 
            get { return _grandpaDialogue; } 
            set 
            {
                if (GrandpaDialogueChoices.Contains(value)) _grandpaDialogue = value;
                else
                {
                    string fallback = GrandpaDialogueDefault;
                    _grandpaDialogue = fallback;
                    Monitor.Log($"Invalid config value \"{value}\" for GrandpaDialogue.\n" + 
                        $"Accepted values are [{string.Join(", ", GrandpaDialogueChoices)}].\n" + 
                        $"GrandpaDialogue has been reset to default value \"{fallback}\"", LogLevel.Warn);
                }
            } 
        }
        private static readonly string[] GrandpaDialogueChoices = new string[] { "Original", "Vanilla", "Nuclear" };
        private static readonly string GrandpaDialogueDefault = GrandpaDialogueChoices[0]; // Default to "Original"
        private string _grandpaDialogue = GrandpaDialogueDefault;

        public bool GenderNeutrality
        {
            get 
            {
                if (_genderNeutrality == null) // Determine the appropriate default setting, set it, and then return it
                {
                    Monitor.Log("Looking for Hana.GenderNeutralityMod", LogLevel.Debug);
                    bool hasMod = Helper.ModRegistry.IsLoaded("Hana.GenderNeutralityMod");
                    if (hasMod)
                    {
                        Monitor.Log($"GenderNeutralityMod detected. Setting up AngryGrandpa config with GenderNeutrality: {hasMod.ToString().ToLower()}", LogLevel.Info);
                    }
                    else
                    {
                        Monitor.Log($"GenderNeutralityMod not detected. Setting up AngryGrandpa config with GenderNeutrality: {hasMod.ToString().ToLower()}", LogLevel.Info);
                    }
                    _genderNeutrality = hasMod; // set default
                }
                return _genderNeutrality.GetValueOrDefault(); 
            }
            set { _genderNeutrality = value; }
        }
        private bool? _genderNeutrality = null; // Initialized with null before determining which default setting to use

        public bool ExpressivePortraits
        {
            get { return _expressivePortraits; }
            set
            {
                _expressivePortraits = value;
                setPortraitTokens();
            }
        }
        private bool _expressivePortraits; // Initialize this one in the constructor

        public string ScoringSystem
        {
            get { return _scoringSystem; }
            set
            {
                if (ScoringSystemChoices.Contains(value)) _scoringSystem = value;
                else
                {
                    string fallback = ScoringSystemDefault;
                    _scoringSystem = fallback;
                    Monitor.Log($"Invalid config value \"{value}\" for ScoringSystem.\n" + 
                        $"Accepted values are [{string.Join(", ", ScoringSystemChoices)}].\n" + 
                        $"ScoringSystem has been reset to default value \"{fallback}\"", LogLevel.Warn);
                }
            }
        }
        private static readonly string[] ScoringSystemChoices = new string[] { "Original", "Vanilla", "Hard", "Expert" };
        private static readonly string ScoringSystemDefault = ScoringSystemChoices[1]; // Default to "Vanilla"
        private string _scoringSystem = ScoringSystemDefault;

        public int YearsBeforeEvaluation 
        {
            get { return _yearsBeforeEvaluation; }
            set {
                if (value >= 0) _yearsBeforeEvaluation = value;
                else
                {
                    _yearsBeforeEvaluation = 2; // Default to 2 years
                    Monitor.Log($"Invalid config value [{value}] for YearsBeforeEvaluation.\n" + 
                        $"You must enter a non-negative integer.\n" + 
                        $"YearsBeforeEvaluation has been reset to default value [{_yearsBeforeEvaluation}].", LogLevel.Warn);
                }
            }
        }
        private int _yearsBeforeEvaluation = 2;

        public bool ShowPointsTotal { get; set; } = true;

        public bool BonusRewards { get; set; } = true;

        private int[] CustomCandleScores // Change this to public when I update to allow custom configs
        { 
            get { return _customCandleScores; }
            set 
            { 
                if ( !(value.Length == 4 && value[0] == 0) ) // Wrong length or first number not zero
                {
                    Monitor.Log($"Invalid config entry [{value}] for CustomCandleScores.\n" +
                        $"You must enter a list of four numbers with the first number equal to 0.\n" +
                        $"CustomCandleScores has been reset.", LogLevel.Warn);
                    return;
                }
                else if ( !(value[0] <= value[1] && value[1] <= value[2] && value[2] <= value[3]) ) // Not in ascending order
                {
                    Monitor.Log($"Invalid config entry [{value}] for CustomCandleScores.\n" +
                        $"You must enter a list of four numbers in increasing order.\n" +
                        $"CustomCandleScores has been reset.", LogLevel.Warn);
                    return;
                }
                _customCandleScores = value;
            }
        }
        private int[] _customCandleScores = new int[4] { 0, 4, 8, 12 };
        #endregion

        #region ModConfig constructor
        public ModConfig()
        {
            ExpressivePortraits = true; // This makes sure setPortraitTokens runs on setup
        }
        #endregion

        #region Utility functions and fields to access config data
        internal int GetScoreForCandles(int candles)
        {
            if (candles < 1 || candles > 4)
            {
                throw new System.ArgumentOutOfRangeException("candles", candles, "candles must be an integer between 1 and 4 inclusive.");
            }
            int[] candleScores;
            if (ScoringSystem == "Original" || ScoringSystem == "Vanilla") 
            {
                candleScores = new int[4] { 0, 4, 8, 12 }; 
            }
            else if (ScoringSystem == "Hard") 
            {
                candleScores = new int[4] { 0, 10, 14, 18 }; 
            }
            else if (ScoringSystem == "Expert")
            {
                candleScores = new int[4] { 0, 15, 18, 21 };
            }
            else if (ScoringSystem == "Custom")
            {
                candleScores = CustomCandleScores;
            }
            else { throw new System.InvalidOperationException("ModConfig.ScoringSystem has an unaccounted-for value."); }
            return candleScores[candles - 1];
        }

        internal int GetMaxScore()
        {
            if (ScoringSystem == "Original")
            {
                return 13;
            }
            else return 21;
        }

        private static readonly List<string> PortraitNames = 
            new List<string>
        {
            "gpaNeutral",
            "gpaHappy",
            "gpaTears",
            "gpaShock",
            "gpaLove",
            "gpaAngry",
            "gpaSigh",
            "gpaRage",
            "gpaFrown",
            "gpaStern",
            "gpaSurprise",
            "gpaJoy"
        };

        // EventEditor and EvaluationEditor each grab this dictionary for translation tokens
        internal Dictionary<string, string> PortraitTokens = new Dictionary<string, string>();

        private void setPortraitTokens()
        {
            PortraitTokens.Clear();
            int count = 0;
            foreach (string emotion in PortraitNames)
            {
                if (ExpressivePortraits)
                {
                    PortraitTokens[emotion] = "$" + count.ToString();
                }
                else PortraitTokens[emotion] = "";

                count++;
            }
        }
        #endregion

        #region Generic Mod Config Menu helper functions
        internal static void Load() 
        { 
            Instance = Helper.ReadConfig<ModConfig>();
        }
        internal static void Save()
        {
            Helper.WriteConfig(Instance);
            ModConfig.Print();
            Helper.Content.InvalidateCache(asset // Trigger changed assets to reload on next use.
                => asset.AssetNameEquals("Strings\\Locations") 
                || asset.AssetNameEquals("Data\\mail")
                || asset.AssetNameEquals("Data\\Events\\Farmhouse")
                || asset.AssetNameEquals("Data\\Events\\Farm")
                || asset.AssetNameEquals("Portraits\\Grandpa"));
        }

        internal static void Reset()
        {
            Instance = new ModConfig();
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
                    () => Instance.GenderNeutrality,
                    (bool val) => Instance.GenderNeutrality = val);

            api.RegisterSimpleOption(manifest,
                    "Expressive dialogue portraits",
                    "Grandpa gets a variety of new facial expressions",
                    () => Instance.ExpressivePortraits,
                    (bool val) => Instance.ExpressivePortraits = val);

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
                    "Show points total",
                    "Displays your raw score during the evaluation",
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
        #endregion

        internal static void Print()
        {
            Monitor.Log(
                $"CONFIG\n" +
                $"====================\n" +
                $"GrandpaDialogue: \"{Instance.GrandpaDialogue}\"\n" +
                $"GenderNeutrality: {Instance.GenderNeutrality.ToString().ToLower()}\n" +
                $"ExpressivePortraits: {Instance.ExpressivePortraits.ToString().ToLower()}\n" +
                $"ScoringSystem: \"{Instance.ScoringSystem}\"\n" +
                $"YearsBeforeEvaluation: {Instance.YearsBeforeEvaluation}\n" +
                $"ShowPointsTotal: {Instance.ShowPointsTotal.ToString().ToLower()}\n" +
                $"BonusRewards: {Instance.BonusRewards.ToString().ToLower()}\n" +
                $"====================", LogLevel.Debug); // Use .ToLower to make bool capitalization match config.json format
        }
    }
}
