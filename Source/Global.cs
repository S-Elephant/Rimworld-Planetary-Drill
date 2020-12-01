using RimWorld;
using SquirtingElephant.Helpers;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SquirtingElephant.PlanetaryDrill
{
    public static class Global
    {
        public static void ApplySettingsToDefs()
        {
            Utils.SetResearchBaseCost("PlanetaryDrilling", PD_Settings.Settings.DrillResearchCost);

            ThingDef planetaryDrillDef = Utils.GetDefByDefName<ThingDef>("SE_PlanetaryDrill");
            if (planetaryDrillDef != null)
            {
                IEnumerable<RecipeDef> dds = PD_Settings.Settings.Drillables.Values.Select(dd => dd.CreateDrillRecipe());
                planetaryDrillDef.recipes = dds.ToList();

                planetaryDrillDef.comps.OfType<CompProperties_Power>().First().basePowerConsumption = PD_Settings.Settings.DrillPowerConsumption;
            }
        }
    }
}
