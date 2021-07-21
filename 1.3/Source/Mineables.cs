using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SquirtingElephant.PlanetaryDrill
{
    public static class Mineables
    {
        /// <summary>
        /// Everything that can possible be mined by the Planetary Drill, ever.
        /// </summary>
        public static HashSet<ThingDef> AllMineables;

        public static void FillAllMineables()
        {
            AllMineables = DefDatabase<ThingDef>.AllDefs.Where(d => d.category == ThingCategory.Item).ToHashSet();
        }
    }
}
