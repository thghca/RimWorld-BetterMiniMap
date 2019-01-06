using Verse;
using RimWorld;
using Harmony;
using System.Linq;

namespace BetterMiniMap
{
    [StaticConstructorOnStartup]
    public class MiniMap_GameComponent : GameComponent
    {
        // used to toggle minimap
        private static bool researchPal; 
        private static bool relationsTab;

        // TODO: remove static context
        private static MiniMapManager miniMapManager; 

        public static MiniMapManager MiniMapManager
        {
            get => miniMapManager;
        }

        public MiniMap_GameComponent(Game g) : this() { }
        public MiniMap_GameComponent()
        {
            BetterMiniMapMod.InitSettingsIfNeed();
            miniMapManager = new MiniMapManager();
        }

        internal static MiniMapWindow MiniMap
        {
            get => miniMapManager.GetMiniMap(Find.CurrentMap);
        }

        static MiniMap_GameComponent()
        {
            // used for toggling minimap 
            researchPal = ModLister.AllInstalledMods.FirstOrDefault(m => m.Name == "ResearchPal")?.Active == true;
            relationsTab = ModLister.AllInstalledMods.FirstOrDefault(m => m.Name == "Relations Tab")?.Active == true;
        }

        #region HarmonyPatches

        /*static void ToggleMiniMap(MainTabsRoot __instance, MainButtonDef newTab)
        {
#if DEBUG
            Log.Message($"MainTabsRoot.ToggleTab: {newTab} {__instance.OpenTab != null}");
#endif
            if (MiniMap.Active)
            {
                if (__instance.OpenTab != null)
                {
                    switch (newTab.defName)
                    {
                        case "Research":
                            if (researchPal) MiniMap.Toggle(false);
                            break;
                        case "Factions":
                            if (relationsTab) MiniMap.Toggle(false);
                            break;
                        default:
                            MiniMap.Toggle(!WorldRendererUtility.WorldRenderedNow);
                            break;
                    }
                }
                else
                    MiniMap.Toggle(!WorldRendererUtility.WorldRenderedNow);
            }
        }*/

        /*static void ToggleMiniMap_WorldTab()
        {
#if DEBUG
            Log.Message($"MainButtonWorker_ToggleWorld.Activate: {Find.World.renderer.wantedMode}");
#endif
            if (MiniMap.Active)
                MiniMap.Toggle(!WorldRendererUtility.WorldRenderedNow);
        }*/

        static void AddMiniMap(Map map)
        {
            if (!BetterMiniMapMod.modSettings.singleMode)
                miniMapManager.AddMiniMap(map);
        }

        static void RemoveMiniMap(Map map)
        {
            if (!BetterMiniMapMod.modSettings.singleMode)
                miniMapManager.RemoveMiniMap(map);
        }

        static void PlaySettings_DoPlaySettingsGlobalControls_Prefix(WidgetRow row, bool worldView)
        {
            if (!BetterMiniMapMod.modSettings.singleMode && row.ButtonIcon(MiniMapTextures.MapManagerIcon))
                Find.WindowStack.Add(MiniMapManager.MiniMapMenu);
        }

        static void CurrentMap_Prefix(Game __instance, Map value)
        {
            if (BetterMiniMapMod.modSettings.singleMode && __instance.currentMapIndex >= 0 && __instance.currentMapIndex < __instance.Maps.Count && __instance.Maps.Contains(value))
                MiniMapManager.SetMap(value);
        }

        #endregion HarmonyPatches

        public override void GameComponentOnGUI()
        {
            base.GameComponentOnGUI();
            MiniMapManager.GameComponentOnGUI();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<MiniMapManager>(ref miniMapManager, "miniMapManager");

            // Handles upgrading settings
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (miniMapManager == null)
                    miniMapManager = new MiniMapManager();
            } 
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            if (BetterMiniMapMod.modSettings.singleMode)
                MiniMapManager.AddMiniMap(Find.CurrentMap);
        }
    }
}