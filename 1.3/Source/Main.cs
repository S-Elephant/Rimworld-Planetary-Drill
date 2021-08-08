using Verse;

namespace SquirtingElephant.PlanetaryDrill
{
    [StaticConstructorOnStartup]
    public class Main // Ignore the message "Class 'Main' is never used because some IDE's can't detect the StaticConstructorOnStartup-attribute."
    {
        /// <summary>
        /// This constructor is executed after the XML-database has been fully loaded.
        /// </summary>
        static Main()
        {
            Mineables.FillAllMineables();

            if (PD_Settings.Settings.IsFirstRun)
            {
                Log.Message("SEPD_FirstRunMessage".Translate().CapitalizeFirst());
                PD_Settings.Settings.IsFirstRun = false;
                PD_Settings.Settings.ResetDrillableSettings();
            }
            else
                PD_Settings.RemoveInvalidSettings();
            
            PD_Settings.ApplySettingsToDefs();
        }
    }
}
