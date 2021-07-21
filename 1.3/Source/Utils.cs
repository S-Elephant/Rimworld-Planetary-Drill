// Copyright 2019 Squirting Elephant.
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Verse;

namespace SquirtingElephant.Helpers
{
    public static class Extensions
    {
        #region Rect

        public static Rect Add(this Rect r1, Rect r2)
        {
            return new Rect(r1.x + r2.x, r1.y + r2.y, r1.width + r2.width, r1.height + r2.height);
        }

        public static Rect Subtract(this Rect r1, Rect r2)
        {
            return new Rect(r1.x - r2.x, r1.y - r2.y, r1.width - r2.width, r1.height - r2.height);
        }

        #region Add

        public static Rect Add_X(this Rect r, float x)
        {
            return new Rect(r.x + x, r.y, r.width, r.height);
        }

        public static Rect Add_Y(this Rect r, float y)
        {
            return new Rect(r.x, r.y + y, r.width, r.height);
        }

        public static Rect Add_Width(this Rect r, float width)
        {
            return new Rect(r.x, r.y, r.width + width, r.height);
        }

        public static Rect Add_Height(this Rect r, float height)
        {
            return new Rect(r.x, r.y, r.width, r.height + height);
        }

        #endregion

        #region Replace

        public static Rect Replace_X(this Rect r, float x)
        {
            return new Rect(x, r.y, r.width, r.height);
        }

        public static Rect Replace_Y(this Rect r, float y)
        {
            return new Rect(r.x, y, r.width, r.height);
        }

        public static Rect Replace_Width(this Rect r, float width)
        {
            return new Rect(r.x, r.y, width, r.height);
        }

        public static Rect Replace_Height(this Rect r, float height)
        {
            return new Rect(r.x, r.y, r.width, height);
        }

        #endregion

        #endregion

        /// <summary>
        /// Translates and capitalizes the first character.
        /// </summary>
        public static string TC(this string s)
        {
            return s.Translate().CapitalizeFirst();
        }
    }
    
    #region Table

    public abstract class TableEntity
    {
        protected readonly TableData TableData;
        public string Name = string.Empty;

        private Rect _Rect;

        /// <summary>
        /// Please do not edit this outside of TableData. This value is calculated.
        /// </summary>
        public Rect Rect
        {
            get { return _Rect; }
            set { _Rect = value; }
        }

        public TableEntity(TableData tableData)
        {
            this.TableData = tableData;
        }
    }

    public class TableColumn : TableEntity
    {
        private float _Width = 0f;

        public float Width
        {
            get { return _Width; }
            set
            {
                if (value != _Width)
                {
                    if (value > 0f)
                    {
                        _Width = value;
                        TableData.Update();
                    }
                    else
                    {
                        Log.Error(string.Format("TableRow received a value of {0} for its Height.", value));
                    }
                }
            }
        }

        public TableColumn(TableData tableData, float width) : base(tableData)
        {
            this.Width = width;
        }
    }

    public class TableRow : TableEntity
    {
        private List<TableField> _Fields = new List<TableField>();

        public List<TableField> Fields
        {
            get { return _Fields; }
            set { _Fields = value; }
        }

        private float _Height;

        public float Height
        {
            get { return _Height; }
            set
            {
                if (value != _Height)
                {
                    if (value > 0f)
                    {
                        _Height = value;
                        TableData.Update();
                    }
                    else
                    {
                        Log.Error(string.Format("TableRow received a value of {0} for its Height.", value));
                    }
                }
            }
        }

        public TableRow(TableData tableData, float height) : base(tableData)
        {
            this.Height = height;
            SetFields(tableData);
        }

        private void SetFields(TableData tableData)
        {
            Fields.Clear();
            tableData.Columns.ForEach(c => Fields.Add(new TableField(tableData, c, this)));
        }

        public void UpdateFields()
        {
            Fields.ForEach(f => f.Update());
        }
    }

    public class TableField : TableEntity
    {
        private TableColumn _Column;

        public TableColumn Column
        {
            get { return _Column; }
            private set { _Column = value; }
        }

        private TableRow _Row;

        public TableRow Row
        {
            get { return _Row; }
            private set { _Row = value; }
        }

        public static TableField Invalid
        {
            get { return new TableField(null, null, null); }
        }

        public TableField(TableData tableData, TableColumn column, TableRow row) : base(tableData)
        {
            this.Column = column;
            this.Row = row;
            Update();
        }

        public void Update()
        {
            Rect = new Rect(Column.Rect.x, Row.Rect.y, Column.Rect.width, Row.Rect.height);
        }
    }

    public class TableData
    {
        #region Properties

        public float Bottom
        {
            get { return TableRect.yMax; }
        }

        private Vector2 _Spacing;

        public Vector2 Spacing
        {
            get { return _Spacing; }
            set
            {
                if (value != _Spacing)
                {
                    _Spacing = value;
                    Update();
                }
            }
        }

        private Vector2 _TableOffset;

        public Vector2 TableOffset
        {
            get { return _TableOffset; }
            set
            {
                if (value != _TableOffset)
                {
                    _TableOffset = value;
                    Update();
                }
            }
        }

        private List<TableColumn> _Columns = new List<TableColumn>();

        /// <summary>
        /// Column Datas.
        /// </summary>
        public List<TableColumn> Columns
        {
            get { return _Columns; }
            set { _Columns = value; }
        }

        private List<TableRow> _Rows = new List<TableRow>();

        public List<TableRow> Rows
        {
            get { return _Rows; }
            set { _Rows = value; }
        }

        /// <summary>
        /// Used privately within methods to temporarily disable the updating without changing the UpdateEnabled setting.
        /// Please set this value back to true at the end of your method or whatever you are doing.
        /// </summary>
        private bool PrivateUpdateEnabled = true;

        public bool UpdateEnabled = true;

        private Rect _TableRect = Rect.zero;

        public Rect TableRect
        {
            get { return _TableRect; }
            private set { _TableRect = value; }
        }

        private const float DEFAULT_ROW_HEIGHT = 32f;
        
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableOffset"></param>
        /// <param name="spacing"></param>
        /// <param name="colWidths"></param>
        /// <param name="rowHeights"></param>
        /// <param name="colCount"></param>
        /// <param name="rowCount">Note: A header-row also counts as 1 rowcount.</param>
        public TableData(Vector2 tableOffset, Vector2 spacing, float[] colWidths, float[] rowHeights, int colCount = -1, int rowCount = -1)
        {
            Initialize(tableOffset, spacing, colWidths, rowHeights, colCount, rowCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableOffset"></param>
        /// <param name="tableSpacing"></param>
        /// <param name="colWidths"></param>
        /// <param name="rowHeights"></param>
        /// <param name="colCount">If this value is greater than <c>colWidths</c> then the last <c>colWidths</c> will be used for the extra columns.</param>
        /// <param name="rowCount">If this value is greater than <c>rowHeights</c> then the last <c>rowHeights</c> will be used for the extra rows.</param>
        private void Initialize(Vector2 tableOffset, Vector2 tableSpacing, float[] colWidths, float[] rowHeights, int colCount = -1, int rowCount = -1)
        {
            // For performance reasons disable the updating and update after the table initialization instead.
            PrivateUpdateEnabled = false;

            this._TableOffset = tableOffset;
            this._Spacing = tableSpacing;

            /// Add Columns.
            foreach (float colWidth in colWidths)
            {
                AddColumns(colWidth);
            }

            AddColumns(colWidths.Last(), colCount - colWidths.Length);

            /// Add Rows.
            foreach (float rowHeight in rowHeights)
            {
                AddRow(rowHeight);
            }

            AddRow(GetLastRowHeight(), rowCount - Rows.Count);

            PrivateUpdateEnabled = true;
            Update();
        }

        public void Update(bool force = false)
        {
            if ((!UpdateEnabled || !PrivateUpdateEnabled) && !force)
            {
                return;
            }

            SetTableRect();
            UpdateColumns();
            UpdateRowsAndFields();
        }

        private float CalcTableWidth()
        {
            if (Columns.Count == 0)
            {
                return 0f;
            }

            float result = Columns[0].Width;
            for (int i = 1; i < Columns.Count; i++)
            {
                result += Columns[i].Width + Spacing.x;
            }

            return result;
        }

        private float CalcTableHeight()
        {
            if (Rows.Count == 0)
            {
                return 0f;
            }

            float result = Rows[0].Height;
            for (int i = 1; i < Rows.Count; i++)
            {
                result += Rows[i].Height + Spacing.y;
            }

            return result;
        }

        private void SetTableRect()
        {
            TableRect = new Rect(TableOffset.x, TableOffset.y, CalcTableWidth(), CalcTableHeight());
        }

        /// <summary>
        /// Please also call UpdateRowsAndFields() after changing any column through here.
        /// </summary>
        public void UpdateColumns()
        {
            float nextColStart_X = TableRect.x;
            foreach (TableColumn col in Columns)
            {
                col.Rect = new Rect(nextColStart_X, TableRect.y, col.Width, TableRect.height);
                nextColStart_X = col.Rect.xMax + Spacing.x;
            }
        }

        public void UpdateRowsAndFields()
        {
            float nextRowStart_Y = TableRect.y;
            foreach (TableRow row in Rows)
            {
                row.Rect = new Rect(TableRect.x, nextRowStart_Y, TableRect.width, row.Height);
                nextRowStart_Y = row.Rect.yMax + Spacing.y;
                row.UpdateFields();
            }
        }

        /// <summary>
        /// Note: Will do nothing if <c>amount</c> is zero or less.
        /// </summary>
        public void AddRow(float rowHeight, int amount = 1)
        {
            if (amount == 0)
            {
                return;
            }

            PrivateUpdateEnabled = false;
            for (int i = 0; i < amount; i++)
            {
                Rows.Add(new TableRow(this, rowHeight));
            }

            PrivateUpdateEnabled = true;
            Update();
        }

        /// <summary>
        /// Note: Will do nothing if <c>amount</c> is zero or less.
        /// </summary>
        private void AddColumns(float colWidth, float amount = 1)
        {
            if (amount == 0)
            {
                return;
            }

            PrivateUpdateEnabled = false;
            for (int i = 0; i < amount; i++)
            {
                Columns.Add(new TableColumn(this, colWidth));
            }

            PrivateUpdateEnabled = true;
            Update();
        }

        private void CreateRowsUntil(int rowIdx)
        {
            AddRow(GetLastRowHeight(), rowIdx + 1 - Rows.Count);
        }

        public TableField GetField(int colIdx, int rowIdx)
        {
            if (colIdx >= Columns.Count)
            {
                Log.Error(string.Format("Attemped to access a column that's out of bounds. Received: {0}.", colIdx));
                return TableField.Invalid;
            }

            CreateRowsUntil(rowIdx);
            return Rows[rowIdx].Fields[colIdx];
        }

        private float GetLastRowHeight()
        {
            return Rows.Count > 0 ? Rows.Last().Height : DEFAULT_ROW_HEIGHT;
        }

        #region Get Rect Helpers

        public Rect GetRowRect(int rowIdx)
        {
            CreateRowsUntil(rowIdx);
            return Rows[rowIdx].Rect;
        }

        public Rect GetHeaderRect(int colIdx)
        {
            return GetField(colIdx, 0).Rect;
        }

        public Rect GetFieldRect(int colIdx, int rowIdx)
        {
            return GetField(colIdx, rowIdx).Rect;
        }

        #endregion

        /// <summary>
        /// Will highlight the entire table-row if the mouse is over it. Call this somewhere in DoWindowContents() while a Listing_Standard is active.
        /// </summary>
        /// <param name="rowIdx">The current row index to apply this to.</param>
        public void ApplyMouseOverEntireRow(int rowIdx)
        {
            Rect rowRect = GetRowRect(rowIdx);
            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }
        }

        #region Debug

#if DEBUG
        private static Texture2D TableRectTexure = null;
        private static Texture2D ColTexure = null;
        private static Texture2D RowTexure = null;
        private static Texture2D FieldTexture = null;

        /// <summary>
        /// Notes:
        /// 1. Drawing the Rows bugs (they are not wide enough? Why?)
        /// 2. Sometimes Rimworld complains about more calls to BeginScrollView() than to EndScrollView(). Why? I have no idea but it may happen when adding Widgets through a <T> method.
        /// </summary>
        public void DrawTableDebug()
        {
            if (TableRect.width == 0f || TableRect.height == 0f)
            {
                Log.Error("The TableRect its width and/or height are zero.");
                return;
            }

            /// Initialize textures.
            int texWidth = (int) TableRect.width;
            int texHeight = (int) TableRect.height;
            Utils.SetupTextureAndColorIt(ref TableRectTexure, texWidth, texHeight, Color.black);
            Utils.SetupTextureAndColorIt(ref ColTexure, texWidth, texHeight, Color.blue);
            Utils.SetupTextureAndColorIt(ref RowTexure, texWidth, texHeight, Color.yellow);
            Utils.SetupTextureAndColorIt(ref FieldTexture, texWidth, texHeight, Color.red);

            // Draw TableRect.
            Widgets.Label(TableRect, new GUIContent(TableRectTexure));
            // Draw all columns.
            Columns.ForEach(c => Widgets.Label(c.Rect, new GUIContent(ColTexure)));
            foreach (TableRow row in Rows)
            {
                // Draw the rows.
                Widgets.Label(row.Rect, new GUIContent(RowTexure));
                // Draw the fields of this row.
                row.Fields.ForEach(f => Widgets.Label(f.Rect, new GUIContent(FieldTexture)));
            }
        }

        public void LogDebugData()
        {
            string rowLocs = string.Empty;
            Rows.ForEach(r => rowLocs += r.Rect.ToString() + "  ");
            Log.Message(string.Format("Table Debug. Table Rect: {0}, colCnt: {1}, rowCnt: {2}, rowLocs: {3}", TableRect.ToString(), Columns.Count.ToString(), Rows.Count.ToString(), rowLocs));
        }
#endif

        #endregion
    }

    #endregion
    
    public static class SeMath
    {
        public static int RoundToNearestMultiple(int value, float multiple)
        {
            return (int)(Math.Round(value / multiple) * multiple);
        }

        /// <summary>
        /// Calculates the X-location of the column on <c>colIdx</c>.
        /// </summary>
        /// <param name="offset_X">X-offset for the first column.</param>
        /// <param name="colWidth">Column Width (each column has the same width).</param>
        /// <param name="spacing_X">Spacing between columns.</param>
        /// <param name="colIdx">Column index for the column that you need the x-location for.</param>
        /// <returns></returns>
        public static float CalcColumn_X(float offset_X, float colWidth, float spacing_X, int colIdx)
        {
            return offset_X + (colWidth + spacing_X) * colIdx;
        }
    }

    public static class Utils
    {
#region Core
        
        private const string DEF_NOT_FOUND_FORMAT = "Unable to find {0}: {1}. Please ensure that this def exists in the database and that the database was loaded before trying to locate this.";
        public static void LogDefNotFoundWarning(string defName, string defType = "Def") { Log.Warning(string.Format(DEF_NOT_FOUND_FORMAT, defType, defName)); }
        public static void LogDefNotFoundError(string defName, string defType = "Def") { Log.Error(string.Format(DEF_NOT_FOUND_FORMAT, defType, defName)); }
        private const string SINGLE_WHITE_SPACE = " ";
        
#endregion

#region Middle Methods
        public static T GetDefByDefName<T>(string recipeDefName, bool errorOnNotFound = true) where T : Def
        {
            T def = DefDatabase<T>.GetNamed(recipeDefName);
            if (def != null)
            {
                return def;
            }

            if (errorOnNotFound)
            {
                LogDefNotFoundError(recipeDefName, typeof(T).Name);
            }

            return null;
        }

        public static void SetThingStat(string thingDefName, string statDefName, float newValue)
        {
            ThingDef def = GetDefByDefName<ThingDef>(thingDefName);
            if (def != null)
            {
                def.statBases.Find(s => s.stat.defName == statDefName).value = newValue;
            }
        }
#endregion

        public enum ESide { LeftHalf, RightHalf, BothSides }

        /// <summary>
        /// Note that this method assumes that windows is installed on the C-drive.
        /// </summary>
        public static string GetModSettingsFolderPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return $@"C:\Users\{Environment.UserName}\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Config";
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "~/Library/Application Support/RimWorld/Config";

            return "~/.config/unity3d/Ludeon Studios/RimWorld by Ludeon Studios/Config"; // Unix
        }

        public static void OpenModSettingsFolder()
        {
            string path = GetModSettingsFolderPath();
            if (System.IO.Directory.Exists(path))
                System.Diagnostics.Process.Start(path);
            else
                Log.Error(string.Format("Unable to open path: {0}. This error is not problematic and doesn't hurt your game.", path));
        }

        public static void SetupTextureAndColorIt(ref Texture2D texture, int width, int height, Color color)
        {
            bool paintTexture = false;
            if (texture == null)
            {
                texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
                paintTexture = true;
            }
            else if (texture.width != width || texture.height != height)
            {
                if (width == 0 || height == 0)
                    Log.Error("Received either a 0 for the texture width and/or height.");
                paintTexture = (texture.width < width) || (texture.height < height);
                texture.width = width;
                texture.height = height;
            }

            if (paintTexture)
            {
                Color[] textureArray = texture.GetPixels();
                for (var i = 0; i < textureArray.Length; ++i)
                    textureArray[i] = color;
                texture.SetPixels(textureArray);
                texture.Apply();
            }
        }

        public static IEnumerable<Building> GetBuildingsByDefName(string defName)
        {
            if (Current.Game == null || Current.Game.CurrentMap == null)
                return Enumerable.Empty<Building>();
            
            return Current.Game.CurrentMap.listerBuildings.allBuildingsColonist.Where(b=>b.def.defName == defName);
        }

        public static void AddRecipeUnique(ThingDef thingDef, RecipeDef recipe)
        {
            if (!thingDef.recipes.All(r => r.defName != recipe.defName))
                thingDef.recipes.Add(recipe);
        }

        /// <summary>
        /// Looks up recipes in one ThingDef and adds references to them to another ThingDef.recipes.
        /// </summary>
        public static void CopyRecipesFromAToB(string sourceDefName, string destinationDefName)
        {
            ThingDef source = Utils.GetDefByDefName<ThingDef>(sourceDefName);
            ThingDef destination = Utils.GetDefByDefName<ThingDef>(destinationDefName);

            foreach (RecipeDef recipe in source.recipes)
                AddRecipeUnique(destination, recipe);
        }

        public static void AddRecipesToDef(string thingDefName, bool errorOnRecipeNotFound, params string[] recipeDefNames)
        {
            if (recipeDefNames.Length == 0)
                return;
            
            ThingDef td = Utils.GetDefByDefName<ThingDef>(thingDefName, false);
            if (td == null)
                return;

            foreach (string recipeDefName in recipeDefNames)
            {
                RecipeDef recipe = Utils.GetDefByDefName<RecipeDef>(recipeDefName, errorOnRecipeNotFound);
                if (recipe != null)
                    AddRecipeUnique(td, recipe);
            }
        }

        private static Rect GetRectFor(Listing_Standard ls, ESide side, float rowHeight)
        {
            switch (side)
            {
                case ESide.LeftHalf:
                    return ls.GetRect(rowHeight).LeftHalf();
                case ESide.RightHalf:
                    return ls.GetRect(rowHeight).RightHalf();
                case ESide.BothSides:
                    return ls.GetRect(rowHeight);
                default:
                    throw new ArgumentException("Unexpected value", nameof(side));
            }
        }

        public static void MakeCheckboxLabeled(Listing_Standard ls, string translationKey, ref bool checkedSetting, ESide side = ESide.RightHalf, float rowHeight = 32f)
        {
            Rect boxRect = GetRectFor(ls, side, rowHeight);
            Widgets.CheckboxLabeled(boxRect, translationKey.Translate().CapitalizeFirst() + SINGLE_WHITE_SPACE, ref checkedSetting);
        }

        public static void MakeTextFieldNumericLabeled<T>(Listing_Standard ls, string translationKey, ref T setting, float min = 1, float max = 1000, ESide side = ESide.RightHalf, float rowHeight = 32f) where T : struct
        {
            Rect boxRect = GetRectFor(ls, side, rowHeight);
            string buffer = setting.ToString();
            Widgets.TextFieldNumericLabeled(boxRect, translationKey.Translate().CapitalizeFirst() + SINGLE_WHITE_SPACE, ref setting, ref buffer, min, max);
        }

        public static void EditPowerGenerationValue(string thingDefName, int newPowerGenerationAmount)
        {
            ThingDef thingDef = GetDefByDefName<ThingDef>(thingDefName);
            if (thingDef != null)
                thingDef.comps.OfType<CompProperties_Power>().First().basePowerConsumption = -Math.Abs(newPowerGenerationAmount);
        }

        public static void SetWorkAmount(string recipeDefName, int newWorkAmount)
        {
            RecipeDef rd = GetDefByDefName<RecipeDef>(recipeDefName);
            if (rd != null)
                rd.workAmount = newWorkAmount;
        }

        public static void SetYieldAmount(string recipeDefName, int newYieldAmount)
        {
            RecipeDef def = GetDefByDefName<RecipeDef>(recipeDefName);
            if (def != null)
                def.products.ForEach(p => p.count = newYieldAmount);
        }

        public static void SetResearchBaseCost(string researchDefName, int newResearchCost)
        {
            ResearchProjectDef rpd = GetDefByDefName<ResearchProjectDef>(researchDefName);
            if (rpd != null)
                rpd.baseCost = newResearchCost;
        }

        public static void SetThingMaxHp(string thingDefName, int newHP)
        {
            SetThingStat(thingDefName, "MaxHitPoints", newHP);
        }

        public static void SetThingMaxBeauty(string thingDefName, int newBeauty)
        {
            SetThingStat(thingDefName, "Beauty", newBeauty);
        }

        public static void SetThingTurretBurstCooldown(string thingDefName, float newTurretBurstCooldown)
        {
            ThingDef def = GetDefByDefName<ThingDef>(thingDefName);
            if (def != null)
                def.building.turretBurstCooldownTime = newTurretBurstCooldown;
        }

        public static void SetThingSteelCost(string thingDefName, int newSteelCost)
        {
            ThingDef def = GetDefByDefName<ThingDef>(thingDefName);
            if (def != null)
            {
                ThingDefCountClass costDef = def.costList.FirstOrDefault(c => c.thingDef == ThingDefOf.Steel);
                if (costDef != null) { costDef.count = newSteelCost; }
            }
        }

        public static void SetThingComponentCost(string thingDefName, int newComponentCost)
        {
            ThingDef def = GetDefByDefName<ThingDef>(thingDefName);
            if (def != null)
            {
                ThingDefCountClass costDef = def.costList.FirstOrDefault(c => c.thingDef == ThingDefOf.ComponentIndustrial);
                if (costDef != null) { costDef.count = newComponentCost; }
            }
        }

        public static void SetThingComponentSpacerCost(string thingDefName, int newComponentSpacerCost)
        {
            ThingDef def = GetDefByDefName<ThingDef>(thingDefName);
            if (def != null)
            {
                ThingDefCountClass costDef = def.costList.FirstOrDefault(c => c.thingDef == ThingDefOf.ComponentSpacer);
                if (costDef != null) { costDef.count = newComponentSpacerCost; }
            }
        }
    }
}
