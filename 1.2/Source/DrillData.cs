using RimWorld;
using SquirtingElephant.Helpers;
using System.Collections.Generic;
using Verse;

namespace SquirtingElephant.PlanetaryDrill
{
    public class DrillData : IExposable
    {
        #region Fields
        
        private ThingDef _ThingDefToDrill = null;
        public ThingDef ThingDefToDrill
        {
            get
            {
                if (_ThingDefToDrill == null) { _ThingDefToDrill = Utils.GetDefByDefName<ThingDef>(ThingDefToDrillName); }
                return _ThingDefToDrill;
            }
            set
            {
                _ThingDefToDrill = value;
                ThingDefToDrillName = value?.defName;
            }
        }

        /// <summary>
        /// For the love of god, never change this value after _ThingDefToDrill has been set. 
        /// </summary>
        private string ThingDefToDrillName = null;

        public int WorkAmount;
        /// <summary>
        /// How many ores are dug up each time.
        /// </summary>
        public int YieldAmount;

        private string RecipeDefName => $"SEPD_{ThingDefToDrillName}_DrillRecipe";

        private int _MaxYieldAmount = 0;
        public int MaxYieldAmount
        {
            get
            {
                if (_MaxYieldAmount == 0 && _ThingDefToDrill != null)
                    _MaxYieldAmount = MAX_ITEM_SPAWN_COUNT * _ThingDefToDrill.stackLimit;

                return _MaxYieldAmount;
            }
        }

        private const int MAX_ITEM_SPAWN_COUNT = (3 /* pi */ * 12 * 12 /* radius² */ * 2) - 16;

        #endregion
        
        public DrillData(string thingDefToDrillName) : this(thingDefToDrillName, 1, 1) {}

        public DrillData(string thingDefToDrillName, int workAmount, int yieldAmount)
        {
            ThingDefToDrillName = thingDefToDrillName;
            WorkAmount = workAmount;
            YieldAmount = yieldAmount;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref ThingDefToDrillName, $"{Constants.SETTINGS_PREFIX}{ThingDefToDrillName}_DefName", ThingDefToDrillName, true);
            Scribe_Values.Look(ref WorkAmount, $"{Constants.SETTINGS_PREFIX}{ThingDefToDrillName}_WorkAmount", WorkAmount, true);
            Scribe_Values.Look(ref YieldAmount, $"{Constants.SETTINGS_PREFIX}{ThingDefToDrillName}_DrillAmount", YieldAmount, true);
        }

        public RecipeDef CreateDrillRecipe()
        {
            string recipeDefName = RecipeDefName;
            RecipeDef drillRecipe = DefDatabase<RecipeDef>.GetNamed(recipeDefName, false);
            if (drillRecipe == null)
            {
                drillRecipe = CreateDrillRecipeDef();
                DefDatabase<RecipeDef>.Add(drillRecipe);
            }
            else
            {
                drillRecipe.workAmount = WorkAmount;
                drillRecipe.products = CreateProducts();
            }

            return drillRecipe;
        }

        private List<ThingDefCountClass> CreateProducts() => new List<ThingDefCountClass> { new ThingDefCountClass(ThingDefToDrill, YieldAmount) };

        private RecipeDef CreateDrillRecipeDef()
        {
            RecipeDef rd = new RecipeDef
            {
                defName = RecipeDefName,
                defaultIngredientFilter = new ThingFilter(),
                effectWorking = DefDatabase<EffecterDef>.GetNamed("Smith"),
                workSkill = DefDatabase<SkillDef>.GetNamed("Mining"),
                workSpeedStat = DefDatabase<StatDef>.GetNamed("MiningSpeed"),
                workSkillLearnFactor = 0.5f,
                jobString = $"Drilling {ThingDefToDrill.label}",
                workAmount = WorkAmount,
                ingredients = new List<IngredientCount>(),
                soundWorking = DefDatabase<SoundDef>.GetNamed("Recipe_Machining"),
                label = $"Drill {ThingDefToDrill.label}", // TODO: translate this.
                description = $"Drill {ThingDefToDrill.label}", // TODO: translate this.
                products = CreateProducts()
            };

            return rd;
        }
    }
}
