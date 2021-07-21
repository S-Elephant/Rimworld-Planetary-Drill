using SquirtingElephant.Helpers;
using System.Linq;
using UnityEngine;
using Verse;

namespace SquirtingElephant.PlanetaryDrill
{
    public class PD_Settings : Mod
    {
        #region Fields
        
        public static SettingsData Settings;
        private Vector2 ScrollPosition = Vector2.zero;
        private Rect ViewRect = new Rect(0.0f, 0.0f, 100.0f, 10000.0f);

        public static TableData Table = new TableData(
            new Vector2(10f, 0f), new Vector2(10f, 10f),
            new[] { 225f, 200f, 200f },
            new[] { ROW_HEIGHT });

        private const float NUMERIC_INPUT_WIDTH = 100f;
        private const float BUTTON_HEIGHT = 22f;
        private const int WORK_AMOUNT_MIN = 1;
        private const int WORK_AMOUNT_MAX = 100000;
        private const int YIELD_AMOUNT_MIN = 1;
        private const float ROW_HEIGHT = 32f;
        private const float ICON_SIZE = ROW_HEIGHT;
        
        #endregion

        public PD_Settings(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SettingsData>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var ls = new Listing_Standard();
            ls.Begin(inRect);
            
            CreateRegularSettings(ls);
            CreateOpenConfigFolderButton();
            ls.GapLine();
            float drillableScrollStart_Y = CreateDrillableButtons(ls) + 12f + ROW_HEIGHT;
            CreateRebootNote(ls);

            #region Drillables Scroll View
            ls.BeginScrollView(new Rect(0, drillableScrollStart_Y, inRect.width, inRect.height - drillableScrollStart_Y - ROW_HEIGHT), ref ScrollPosition, ref ViewRect);
            CreateDrillableHeaders();
            
            int rowIdx = 1;
            foreach (DrillData dd in Settings.Drillables.Values)
                CreateDrillableSettingsFields(dd, rowIdx++);

            ls.GetRect(Table.TableRect.height);

            ls.EndScrollView(ref ViewRect);
            #endregion
            ls.End();
            Global.ApplySettingsToDefs();

            base.DoSettingsWindowContents(inRect);
        }

        private void CreateRebootNote(Listing_Standard ls)
        {
            Rect rowRect = ls.GetRect(ROW_HEIGHT);
            Widgets.Label(rowRect, "SEPD_RebootMessage".Translate().CapitalizeFirst());
        }

        private void CreateRegularSettings(Listing_Standard ls)
        {
            Utils.MakeTextFieldNumericLabeled(ls, "SEPD_DrillResearchCost", ref Settings.DrillResearchCost, 1, 100000);
            Utils.MakeTextFieldNumericLabeled(ls, "SEPD_DrillPowerConsumption", ref Settings.DrillPowerConsumption, 1, 50000);

            Utils.MakeCheckboxLabeled(ls, "SEPD_FilterDestroyOnDrop", ref Settings.FilterDestroyOnDrop);
            Utils.MakeCheckboxLabeled(ls, "SEPD_FilterMadeFromStuff", ref Settings.FilterMadeFromStuff);
            Utils.MakeCheckboxLabeled(ls, "SEPD_FilterRottable", ref Settings.FilterRottable);
            Utils.MakeCheckboxLabeled(ls, "SEPD_FilterScatterableOnMapGen", ref Settings.FilterScatterableOnMapGen);
        }

        private void CreateDrillableHeaders()
        {
            Widgets.Label(Table.GetHeaderRect(1), "SEPD_WorkAmount".Translate().CapitalizeFirst());
            Widgets.Label(Table.GetHeaderRect(2), "SEPD_YieldAmount".Translate().CapitalizeFirst());
        }

        private void CreateOpenConfigFolderButton()
        {
            string modconfigFolderPath = Utils.GetModSettingsFolderPath();
            if (Widgets.ButtonText(new Rect(0, 5, 200, BUTTON_HEIGHT), "SEPD_OpenConfigFolder".Translate().CapitalizeFirst(), active: System.IO.Directory.Exists(modconfigFolderPath)))
                Utils.OpenModSettingsFolder();
        }

        /// <summary>
        /// Creates the drillable buttons and returns the bottom location of those buttons.
        /// </summary>
        private float CreateDrillableButtons(Listing_Standard ls)
        {
            var buttonRow = ls.GetRect(BUTTON_HEIGHT);

            Rect addDrillableBtnRect =  new Rect(buttonRow.x, buttonRow.y, buttonRow.width / 3, buttonRow.height);
            Rect removeDrillableBtnRect = new Rect(addDrillableBtnRect.xMax, addDrillableBtnRect.y, addDrillableBtnRect.width, addDrillableBtnRect.height);
            Rect resetDrillableBtnRect = new Rect(removeDrillableBtnRect.xMax, addDrillableBtnRect.y, addDrillableBtnRect.width, addDrillableBtnRect.height);

            CreateAndAddDrillableButton(addDrillableBtnRect);
            CreateRemoveDrillableButton(removeDrillableBtnRect);
            CreateResetDrillableButton(resetDrillableBtnRect);

            return addDrillableBtnRect.yMax;
        }

        private Rect GetSliderField(int colIdx, int rowIdx)
        {
            Rect field = Table.GetFieldRect(colIdx, rowIdx);
            return new Rect(field.x + NUMERIC_INPUT_WIDTH + 2, field.y, field.width - NUMERIC_INPUT_WIDTH - 2, field.height);
        }

        private void CreateDrillableSettingsFields(DrillData dd, int rowIdx)
        {
            // Icon
            Widgets.ThingIcon(Table.GetFieldRect(0, rowIdx).Replace_Width(ICON_SIZE), dd.ThingDefToDrill, null, 1f);

            // ThingDef.label
            Widgets.Label(Table.GetFieldRect(0, rowIdx).Add_X(ICON_SIZE + 2), dd.ThingDefToDrill.label);

            // Work Amount
            string bufferWorkAmount = dd.WorkAmount.ToString();
            Widgets.TextFieldNumeric(Table.GetFieldRect(1, rowIdx).Replace_Width(NUMERIC_INPUT_WIDTH), ref dd.WorkAmount, ref bufferWorkAmount, WORK_AMOUNT_MIN, WORK_AMOUNT_MAX);
            int count = (int)Widgets.HorizontalSlider(GetSliderField(1, rowIdx), dd.WorkAmount, WORK_AMOUNT_MIN, WORK_AMOUNT_MAX);
            if (count != dd.WorkAmount)
                dd.WorkAmount = count;

            // Yield Amount
            string bufferYieldAmount = dd.YieldAmount.ToString();
            Widgets.TextFieldNumeric(Table.GetFieldRect(2, rowIdx).Replace_Width(NUMERIC_INPUT_WIDTH), ref dd.YieldAmount, ref bufferYieldAmount, YIELD_AMOUNT_MIN, dd.MaxYieldAmount);
            int countYield = (int)Widgets.HorizontalSlider(GetSliderField(2, rowIdx), dd.YieldAmount, YIELD_AMOUNT_MIN, dd.MaxYieldAmount);
            if (countYield != dd.YieldAmount)
                dd.YieldAmount = countYield;

            // Row Mouse Hover
            Table.ApplyMouseOverEntireRow(rowIdx);
            TooltipHandler.TipRegion(Table.GetRowRect(rowIdx).LeftHalf(), dd.ThingDefToDrill.description);
        }

        #region Create Drillable Buttons
        private void CreateAndAddDrillableButton(Rect inRect)
        {
            if (!Widgets.ButtonText(inRect, "SEPD_AddDrillable".Translate().CapitalizeFirst(), active: Mineables.AllMineables.Except(Settings.Drillables.Values.Select(d => d.ThingDefToDrill)).Any()))
                return;
            {
                Find.WindowStack.Add(new FloatMenu(
                    Mineables.AllMineables
                        .Where(Settings.CanItemBeDrilledAccordingToSettings)
                        .Except(Settings.Drillables.Values.Select(d=>d.ThingDefToDrill)) // Don't display items that were already added.
                        .Select(m => new FloatMenuOption(m.label, () => Settings.Drillables.Add(m.defName, new DrillData(m.defName, 1000, 1)), m))
                        .ToList()));
            }
        }

        private static void CreateRemoveDrillableButton(Rect inRect)
        {
            if (!Widgets.ButtonText(inRect, "SEPD_RemoveDrillable".Translate().CapitalizeFirst(), active: Settings.Drillables.Any()))
                return;
            
            Find.WindowStack.Add(new FloatMenu(
                Settings.Drillables
                    .Select(kvp => new FloatMenuOption(kvp.Value.ThingDefToDrill.label, () => Settings.Drillables.Remove(kvp.Key), kvp.Value.ThingDefToDrill))
                    .ToList()));
            Table.Rows.Remove(Table.Rows.Last());
        }

        private void CreateResetDrillableButton(Rect inRect)
        {
            if (Widgets.ButtonText(inRect, "SEPD_ResetDrillables".Translate().CapitalizeFirst()))
            {
                while (Table.Rows.Count > 1)
                    Table.Rows.Remove(Table.Rows.Last());
                Settings.ResetDrillableSettings();
            }
        }
        #endregion

        public override string SettingsCategory() => "SEPD_SettingsCategory".Translate();
    }
}
